using System;
using System.Collections.Generic;
using System.IO;
using AndamRunner;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace AndamRunnerEditor
{
    public static class OpongPrefabExporter
    {
        public const string Folder="Assets/AndamRunner/Models/OpongAndam";
        [MenuItem("ANDAM/5. Rebuild Opong model prefab")]
        public static void Export()
        {
            Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            Transform model=PrimitiveArt.CreateOpong(null); model.name="Opong Andam - Shield Rider";
            try
            {
                var colors=new List<Color>(); var filters=new List<MeshFilter>(model.GetComponentsInChildren<MeshFilter>());
                foreach(MeshFilter filter in filters)
                {
                    Color color=filter.GetComponent<Renderer>().sharedMaterial.color;
                    if(!colors.Contains(color)) colors.Add(color);
                }
                colors.Sort((a,b)=>string.CompareOrdinal(ColorUtility.ToHtmlStringRGB(a),ColorUtility.ToHtmlStringRGB(b)));
                int grid=colors.Count<=16?4:8;
                Texture2D atlas=PaintAtlas(colors,grid);
                Shader shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/AndamRunner/Shaders/OpongPainted.shader");
                const string materialPath=Folder+"/OpongAndam.mat";
                Material material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if(material==null) { material=new Material(shader); AssetDatabase.CreateAsset(material,materialPath); }
                material.shader=shader; material.mainTexture=atlas; material.enableInstancing=true; EditorUtility.SetDirty(material);
                var vertices=new List<Vector3>(); var normals=new List<Vector3>(); var uv=new List<Vector2>(); var triangles=new List<int>();
                var weights=new List<BoneWeight>(); var bones=new List<Transform>(); var poses=new List<Matrix4x4>();
                foreach(MeshFilter filter in filters)
                {
                    Transform bone=filter.transform.parent; int boneIndex=bones.IndexOf(bone);
                    if(boneIndex<0) { boneIndex=bones.Count; bones.Add(bone); poses.Add(bone.worldToLocalMatrix*model.localToWorldMatrix); }
                    Mesh source=filter.sharedMesh; Matrix4x4 matrix=model.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                    Matrix4x4 normalMatrix=matrix.inverse.transpose; Vector3[] sourceVertices=source.vertices,sourceNormals=source.normals;
                    int offset=vertices.Count,tile=colors.IndexOf(filter.GetComponent<Renderer>().sharedMaterial.color);
                    Bounds bounds=source.bounds; Vector3 size=bounds.size;
                    for(int i=0;i<sourceVertices.Length;i++)
                    {
                        Vector3 p=sourceVertices[i],n=sourceNormals[i];
                        vertices.Add(matrix.MultiplyPoint3x4(p)); normals.Add(normalMatrix.MultiplyVector(n).normalized);
                        float u=(p.x-bounds.min.x)/Mathf.Max(size.x,.001f),v=(p.y-bounds.min.y)/Mathf.Max(size.y,.001f);
                        if(Mathf.Abs(n.y)>.8f) v=(p.z-bounds.min.z)/Mathf.Max(size.z,.001f);
                        uv.Add(new Vector2((tile%grid+.12f+Mathf.Clamp01(u)*.76f)/grid,(tile/grid+.12f+Mathf.Clamp01(v)*.76f)/grid));
                        weights.Add(new BoneWeight{boneIndex0=boneIndex,weight0=1});
                    }
                    foreach(int index in source.triangles) triangles.Add(offset+index);
                }
                var mesh=new Mesh{name="Opong Andam - mobile skinned mesh",indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
                mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0,uv); mesh.SetTriangles(triangles,0);
                mesh.boneWeights=weights.ToArray(); mesh.bindposes=poses.ToArray(); mesh.RecalculateBounds();
                CheckRigidTriangles(mesh,"generated");
                string meshPath=Folder+"/OpongAndam.asset"; Mesh saved=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if(saved==null) { saved=mesh; AssetDatabase.CreateAsset(saved,meshPath); }
                else
                {
                    // CopySerialized can leave native mesh indices out of step with skin channels.
                    // Reassign the buffers explicitly while preserving the mesh asset's GUID.
                    saved.Clear(); saved.name=mesh.name; saved.indexFormat=mesh.indexFormat;
                    saved.SetVertices(vertices); saved.SetNormals(normals); saved.SetUVs(0,uv); saved.SetTriangles(triangles,0);
                    saved.boneWeights=weights.ToArray(); saved.bindposes=poses.ToArray(); saved.RecalculateBounds();
                    EditorUtility.SetDirty(saved); Object.DestroyImmediate(mesh);
                }
                CheckRigidTriangles(saved,"saved");
                // Geometry is stored in one skin; named transforms remain as the editable animation rig.
                foreach(MeshFilter filter in filters) { Object.DestroyImmediate(filter.GetComponent<Renderer>()); Object.DestroyImmediate(filter); }
                Transform[] parts=model.GetComponentsInChildren<Transform>(true);
                for(int i=parts.Length-1;i>=0;i--) if(!parts[i].gameObject.activeSelf) Object.DestroyImmediate(parts[i].gameObject);
                SkinnedMeshRenderer skin=model.gameObject.AddComponent<SkinnedMeshRenderer>();
                skin.sharedMesh=saved; skin.sharedMaterial=material; skin.bones=bones.ToArray(); skin.rootBone=model;
                skin.localBounds=new Bounds(new Vector3(0,1.5f,0),new Vector3(4,4,4)); skin.shadowCastingMode=ShadowCastingMode.On;
                Animator animator=model.gameObject.AddComponent<Animator>(); animator.runtimeAnimatorController=OpongClipBuilder.Build(Folder);
                animator.applyRootMotion=false; animator.cullingMode=AnimatorCullingMode.AlwaysAnimate; animator.keepAnimatorStateOnDisable=true;
                OpongExpressions expression=model.gameObject.AddComponent<OpongExpressions>(); expression.Initialize();
                PrefabUtility.SaveAsPrefabAsset(model.gameObject,"Assets/AndamRunner/Resources/OpongStorybook.prefab");
                AssetDatabase.SaveAssets();
                CheckRigidTriangles(saved,"persisted");
                Debug.Log("ANDAM_OPONG_EXPORT: "+triangles.Count/3+" triangles; one skinned renderer, one material, "+bones.Count+" joints, 1024 atlas and animation clips.");
            }
            finally { Object.DestroyImmediate(model.gameObject); }
        }

        private static void CheckRigidTriangles(Mesh mesh,string phase)
        {
            BoneWeight[] weights=mesh.boneWeights; int[] triangles=mesh.triangles;
            for(int i=0;i<triangles.Length;i+=3)
                if(weights[triangles[i]].boneIndex0!=weights[triangles[i+1]].boneIndex0 || weights[triangles[i]].boneIndex0!=weights[triangles[i+2]].boneIndex0)
                    throw new InvalidOperationException("Skin indices changed during "+phase+" at triangle "+i/3+": "+triangles[i]+","+triangles[i+1]+","+triangles[i+2]);
            Debug.Log("ANDAM_RIG_INDICES: "+phase+" triangles retain their joint weights.");
        }

        private static Texture2D PaintAtlas(List<Color> colors,int grid)
        {
            const int size=1024; int tileSize=size/grid; var image=new Texture2D(size,size,TextureFormat.RGBA32,false);
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                int tile=x/tileSize+(y/tileSize)*grid; Color color=tile<colors.Count ? colors[tile] : Color.white;
                float wash=.992f+Mathf.PerlinNoise(x*.045f,y*.045f)*.014f;
                bool dark=Mathf.Max(color.r,Mathf.Max(color.g,color.b))<.3f;
                bool gold=color.r>.65f && color.g>.45f && color.b<.40f;
                bool ivory=color.r>.95f && color.g>.95f;
                float sheen=dark ? .32f : gold ? .26f : ivory ? .16f : .055f;
                pixels[y*size+x]=new Color(color.r*wash,color.g*wash,color.b*wash,sheen);
            }
            image.SetPixels(pixels); image.Apply(); string path=Folder+"/OpongPalette-1024.png";
            File.WriteAllBytes(path,image.EncodeToPNG()); Object.DestroyImmediate(image); AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            TextureImporter importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.maxTextureSize=1024; importer.mipmapEnabled=false; importer.wrapMode=TextureWrapMode.Clamp;
            importer.textureCompression=TextureImporterCompression.Compressed; importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
