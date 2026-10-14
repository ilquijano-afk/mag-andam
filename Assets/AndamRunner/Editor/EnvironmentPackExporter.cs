using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AndamRunner;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace AndamRunnerEditor
{
    public static class EnvironmentPackExporter
    {
        public const string Root="Assets/AndamRunner/Environment";
        public const string ResourcesRoot="Assets/AndamRunner/Resources/Environment";
        private sealed class Spec
        {
            public string Id,Folder,Purpose;
            public Vector3 Size;
            public EnvironmentLayer Layer;
            public bool Segment;
            public float Rise;
            public int Lanes;
        }
        public static void EnsurePack()
        {
            if(AssetDatabase.LoadAssetAtPath<EnvironmentAssetPack>(ResourcesRoot+"/EnvironmentAssetPack.asset")==null) Generate(false);
        }
        [MenuItem("ANDAM/Environment/1. Build graybox asset pack")]
        public static void Graybox() { Generate(true); }
        [MenuItem("ANDAM/Environment/2. Rebuild polished asset pack")]
        public static void Polished() { Generate(false); }

        [MenuItem("ANDAM/Environment/4. Export reusable Unity package")]
        public static void ExportUnityPackage()
        {
            string output=System.Environment.GetEnvironmentVariable("ANDAM_ART_PACKAGE");
            if(string.IsNullOrWhiteSpace(output)) output="Builds/Artwork/ANDAM-Environment-and-Runner-0.7.0.unitypackage";
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
            AssetDatabase.ExportPackage(new[]{"Assets/AndamRunner","Assets/Scenes/MainGame.unity"},output,ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);
            Debug.Log("ANDAM_ART_PACKAGE_PASS: "+Path.GetFullPath(output));
        }

        private static Material Material(string name,string shader)
        {
            string path=Root+"/Materials/"+name+".mat";
            Material mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader source=Shader.Find(shader); if(source==null) throw new Exception("Missing environment shader: "+shader);
            if(mat==null) { mat=new Material(source); AssetDatabase.CreateAsset(mat,path); }
            mat.shader=source; mat.enableInstancing=true; EditorUtility.SetDirty(mat); return mat;
        }
        public static void Generate(bool graybox)
        {
            Directory.CreateDirectory(Root+"/Meshes"); Directory.CreateDirectory(Root+"/Materials"); Directory.CreateDirectory(ResourcesRoot);
            foreach(string folder in new[]{"Props","Segments","Gameplay","Player","UI"}) Directory.CreateDirectory(ResourcesRoot+"/"+folder);
            AssetDatabase.Refresh();
            EnvironmentAssetPack pack=AssetDatabase.LoadAssetAtPath<EnvironmentAssetPack>(ResourcesRoot+"/EnvironmentAssetPack.asset");
            if(pack==null) { pack=ScriptableObject.CreateInstance<EnvironmentAssetPack>(); AssetDatabase.CreateAsset(pack,ResourcesRoot+"/EnvironmentAssetPack.asset"); }
            pack.Graybox=graybox;
            pack.Palette=Material("MAT_Environment_VertexPalette","Andam/EnvironmentPalette");
            pack.GameplayPalette=Material("MAT_Gameplay_VertexPalette","Andam/EnvironmentPalette"); pack.GameplayPalette.SetFloat("_Shade",.12f);
            pack.BackgroundPalette=Material("MAT_Background_SoftPalette","Andam/EnvironmentPalette"); pack.BackgroundPalette.SetFloat("_Shade",.08f); pack.BackgroundPalette.SetColor("_Tint",new Color(.85f,.91f,.93f));
            pack.Water=Material("MAT_Water_QuietTeal","Andam/QuietWater"); pack.Water.SetColor("_Color",new Color32(88,151,160,255));
            pack.Rain=Material("MAT_Rain_SoftStreaks","Andam/QuietRain"); pack.Rain.SetFloat("_Amount",0);
            pack.Shadow=Material("MAT_Opong_ContactShadow","Andam/ContactShadow");
            var assets=new List<GameObject>(); var catalog=new StringBuilder("Asset name,Layer,Purpose,Dimensions or UI pixels,Lanes,Prominent props,Triangles,Materials,Colors,Collision\n");
            List<Spec> specs=Specs();
            // Graybox foundation is validated before this command is run in polished mode.
            // Props export first so modular segments can share their saved meshes.
            foreach(Spec spec in specs)
            {
                Transform root;
                if(spec.Segment) root=EnvironmentSegmentFactory.Create(spec.Id,null,graybox);
                else if(spec.Folder=="Gameplay") root=Gameplay(spec.Id);
                else if(spec.Folder=="UI") root=UserInterface(spec.Id);
                else if(spec.Folder=="Player")
                {
                    root=new GameObject(spec.Id).transform;
                    GameObject source=Resources.Load<GameObject>("OpongStorybook");
                    if(source==null) throw new Exception("Prepare the Opong prefab before exporting the environment.");
                    UnityEngine.Object.Instantiate(source,root,false);
                    foreach(MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
                        if(filter.sharedMesh==null)
                        {
                            MeshRenderer renderer=filter.GetComponent<MeshRenderer>();
                            if(renderer!=null) UnityEngine.Object.DestroyImmediate(renderer);
                            UnityEngine.Object.DestroyImmediate(filter);
                        }
                }
                else if(graybox)
                {
                    root=new GameObject(spec.Id).transform;
                    PrimitiveArt.Shape(PrimitiveType.Cube,root,"Graybox silhouette",Vector3.up*spec.Size.y*.5f,spec.Size,EnvironmentModelFactory.Cream);
                }
                else root=EnvironmentModelFactory.Model(spec.Id,null);
                try
                {
                    root.name=spec.Id;
                    var info=root.gameObject.AddComponent<EnvironmentAssetInfo>();
                    info.AssetId=spec.Id; info.Purpose=spec.Purpose; info.Layer=spec.Layer; info.SuggestedSize=spec.Size;
                    info.RoadSegment=spec.Segment; info.LaneCount=spec.Lanes; info.SurfaceRise=spec.Rise;
                    if(spec.Segment)
                    {
                        Transform surface=root.Find("Surface");
                        Bake(surface,pack.Palette,graybox,spec.Id+"_Surface",spec.Rise);
                        Transform props=root.Find("Props"); info.ProminentProps=props.childCount;
                        foreach(Transform prop in props)
                        {
                            if(prop.GetComponentInChildren<MeshRenderer>()!=null && graybox) Bake(prop,pack.Palette,true,spec.Id+"_"+prop.name,0);
                            prop.localPosition+=Vector3.up*spec.Rise*prop.localPosition.z/24f;
                        }
                        Transform water=root.Find("Water"); if(water!=null) Bake(water,pack.Water,false,spec.Id+"_Water",0);
                        PrimitiveArt.Root("Entry",root,Vector3.zero);
                        PrimitiveArt.Root("Exit",root,new Vector3(0,spec.Rise,24));
                        Transform lanes=PrimitiveArt.Root("Lane anchors",root,Vector3.zero);
                        for(int lane=0;lane<spec.Lanes;lane++) PrimitiveArt.Root("Lane "+lane,lanes,new Vector3(spec.Lanes==1?0:(lane-1)*PlayerRunner.LaneWidth,0,0));
                    }
                    else if(spec.Folder!="Player" && spec.Folder!="UI") Bake(root,spec.Folder=="Gameplay"?pack.GameplayPalette:spec.Layer==EnvironmentLayer.Background?pack.BackgroundPalette:pack.Palette,graybox && spec.Folder!="Gameplay",spec.Id,0);
                    // Combined procedural art retains hidden source nodes. Keep their
                    // transforms for rig references, but never serialize temporary meshes.
                    foreach(MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
                        if(!filter.gameObject.activeInHierarchy || filter.sharedMesh==null)
                        {
                            MeshRenderer renderer=filter.GetComponent<MeshRenderer>();
                            if(renderer!=null) UnityEngine.Object.DestroyImmediate(renderer);
                            UnityEngine.Object.DestroyImmediate(filter);
                        }
                    if(spec.Folder!="UI" && spec.Folder!="Player")
                    {
                        Transform collision=PrimitiveArt.Root("Collision design only - disabled",root,Vector3.zero);
                        BoxCollider box=collision.gameObject.AddComponent<BoxCollider>(); box.center=new Vector3(0,spec.Size.y*.5f,spec.Segment?12:0); box.size=spec.Size; box.enabled=false;
                    }
                    foreach(MeshFilter mesh in root.GetComponentsInChildren<MeshFilter>()) if(mesh.sharedMesh!=null) info.Triangles+=mesh.sharedMesh.triangles.Length/3;
                    foreach(SkinnedMeshRenderer mesh in root.GetComponentsInChildren<SkinnedMeshRenderer>()) if(mesh.sharedMesh!=null) info.Triangles+=mesh.sharedMesh.triangles.Length/3;
                    string path=ResourcesRoot+"/"+spec.Folder+"/"+spec.Id+".prefab";
                    GameObject saved=PrefabUtility.SaveAsPrefabAsset(root.gameObject,path); assets.Add(saved);
                    var materialNames=new SortedSet<string>(); var swatches=new SortedSet<string>();
                    foreach(Renderer renderer in root.GetComponentsInChildren<Renderer>()) foreach(Material mat in renderer.sharedMaterials) if(mat!=null) materialNames.Add(mat.name);
                    foreach(MeshFilter filter in root.GetComponentsInChildren<MeshFilter>()) foreach(Color color in filter.sharedMesh.colors) swatches.Add("#"+ColorUtility.ToHtmlStringRGB(color));
                    string paletteText=spec.Folder=="Player"?"Warm skin; black hair; red-brown costume; ivory/teal shield; existing 1024 atlas":spec.Folder=="UI"?"Cream #EDDEBE; teal/ink; white text":string.Join(" ",swatches);
                    catalog.AppendLine(string.Join(",",Quote(spec.Id),Quote(spec.Layer.ToString()),Quote(spec.Purpose),Quote(spec.Size.ToString("0.##")),spec.Lanes,info.ProminentProps,info.Triangles,Quote(spec.Folder=="UI"?"uGUI":string.Join("; ",materialNames)),Quote(paletteText),Quote(spec.Folder=="UI"?"UI raycast only":"Disabled design proxy; gameplay uses swept contacts")));
                }
                finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
                AssetDatabase.SaveAssets();
            }
            pack.Assets=assets.ToArray(); EditorUtility.SetDirty(pack); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Directory.CreateDirectory("Docs"); File.WriteAllText("Docs/Environment-Asset-Catalog.csv",catalog.ToString());
            Debug.Log("ANDAM_ENVIRONMENT_PACK_PASS: "+assets.Count+" "+(graybox?"graybox":"polished")+" prefabs exported with shared materials, pivots and disabled collision proxies.");
        }
        private static string Quote(string text) => "\""+text.Replace("\"","\"\"")+"\"";

        private static void Bake(Transform root,Material material,bool graybox,string id,float rise)
        {
            var vertices=new List<Vector3>(); var normals=new List<Vector3>(); var colors=new List<Color>(); var indices=new List<int>();
            MeshRenderer[] old=root.GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer renderer in old)
            {
                MeshFilter filter=renderer.GetComponent<MeshFilter>(); if(filter==null || filter.sharedMesh==null) continue;
                Mesh mesh=filter.sharedMesh; Vector3[] v=mesh.vertices,n=mesh.normals; Color[] existing=mesh.colors;
                Matrix4x4 matrix=root.worldToLocalMatrix*filter.transform.localToWorldMatrix, normalMatrix=matrix.inverse.transpose;
                Color color=renderer.sharedMaterial!=null && renderer.sharedMaterial.HasProperty("_Color")?renderer.sharedMaterial.color:Color.white;
                int offset=vertices.Count;
                for(int i=0;i<v.Length;i++)
                {
                    Vector3 p=matrix.MultiplyPoint3x4(v[i]); p.y+=rise*p.z/24f;
                    vertices.Add(p); Vector3 normal=normalMatrix.MultiplyVector(n.Length==v.Length?n[i]:Vector3.up).normalized; normal.z-=rise/24f*normal.y; normals.Add(normal.normalized);
                    Color c=existing.Length==v.Length?existing[i]:color;
                    if(graybox) { float gray=Mathf.Lerp(.30f,.83f,c.grayscale); c=new Color(gray,gray,gray,1); }
                    colors.Add(c);
                }
                foreach(int index in mesh.triangles) indices.Add(offset+index);
            }
            Mesh merged=new Mesh{name=id,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
            merged.SetVertices(vertices); merged.SetNormals(normals); merged.SetColors(colors); merged.SetTriangles(indices,0); merged.RecalculateBounds();
            string path=Root+"/Meshes/"+id+".asset";
            Mesh saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved==null) { AssetDatabase.CreateAsset(merged,path); saved=merged; }
            else { EditorUtility.CopySerialized(merged,saved); EditorUtility.SetDirty(saved); UnityEngine.Object.DestroyImmediate(merged); }
            foreach(MeshRenderer renderer in old) UnityEngine.Object.DestroyImmediate(renderer.gameObject);
            Transform visual=PrimitiveArt.Root("Visual",root,Vector3.zero); visual.gameObject.AddComponent<MeshFilter>().sharedMesh=saved;
            MeshRenderer result=visual.gameObject.AddComponent<MeshRenderer>(); result.sharedMaterial=material; result.shadowCastingMode=ShadowCastingMode.Off; result.receiveShadows=false;
        }

        private static Transform Gameplay(string id)
        {
            Transform root=new GameObject(id).transform;
            if(id.StartsWith("PF_Supply_")) RunnerReferenceArt.Supply(root,(SupplyKind)Enum.Parse(typeof(SupplyKind),id.Substring(10)));
            else if(id.StartsWith("PF_Distractor_")) RunnerReferenceArt.Distractor(root,(DistractorKind)Enum.Parse(typeof(DistractorKind),id.Substring(14)));
            else if(id.StartsWith("PF_PowerUp_")) RunnerReferenceArt.PowerUp(root,(PowerUpKind)Enum.Parse(typeof(PowerUpKind),id.Substring(11)));
            else RunnerReferenceArt.Hazard(root,(HazardKind)Enum.Parse(typeof(HazardKind),id.Substring(10)));
            return root;
        }
        private static Transform UserInterface(string id)
        {
            var root=new GameObject(id,typeof(RectTransform),typeof(Image)); ((RectTransform)root.transform).sizeDelta=new Vector2(600,id.Contains("Lesson")?154:id.Contains("HUD")?116:44);
            root.GetComponent<Image>().color=id.Contains("Lesson")?EnvironmentModelFactory.Cream:EnvironmentModelFactory.Ink;
            var label=new GameObject("Editable text",typeof(RectTransform),typeof(Text)); label.transform.SetParent(root.transform,false);
            RectTransform rect=(RectTransform)label.transform; rect.anchorMin=Vector2.zero; rect.anchorMax=Vector2.one; rect.offsetMin=new Vector2(18,8); rect.offsetMax=new Vector2(-18,-8);
            Text text=label.GetComponent<Text>(); text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize=24; text.alignment=TextAnchor.MiddleCenter; text.raycastTarget=false;
            text.color=id.Contains("Lesson")?EnvironmentModelFactory.Ink:Color.white; text.text=id.Contains("Lesson")?"Resilience Starts at Home\nPractise your evacuation route with an adult.":id.Contains("HUD")?"RESILIENCE SCORE     LIVES 3 / 3":"FOLLOW THE DRY SAFE ROUTE";
            return root.transform;
        }
        private static List<Spec> Specs()
        {
            var all=new List<Spec>();
            void Add(string id,string folder,string purpose,Vector3 size,EnvironmentLayer layer=EnvironmentLayer.Midground,bool segment=false,float rise=0,int lanes=0)
            { all.Add(new Spec{Id=id,Folder=folder,Purpose=purpose,Size=size,Layer=layer,Segment=segment,Rise=rise,Lanes=lanes}); }
            string[] props={"CoconutPalm","TropicalTree","UtilityPole","SariSariStore","House_Cream","House_Coral","ResidentialBlock","Carinderia","Resort","ConstructionBarrier","Streetlight","Seawall","CoastalFence","PedestrianFootbridge","EvacuationCenter"};
            Vector3[] sizes={new Vector3(4.4f,5,4.4f),new Vector3(2.8f,4.1f,2.8f),new Vector3(1.2f,5,6),new Vector3(4.3f,3.7f,5.1f),new Vector3(4.3f,3.7f,4.3f),new Vector3(4.3f,3.7f,4.3f),new Vector3(4.3f,6.2f,4.3f),new Vector3(4.7f,3.2f,3.6f),new Vector3(7.4f,6,5.8f),new Vector3(2.1f,1,0.5f),new Vector3(1.1f,5,0.4f),new Vector3(.35f,.9f,6),new Vector3(.12f,1.3f,4.8f),new Vector3(3,2.1f,6),new Vector3(6.9f,4.8f,5.5f)};
            for(int i=0;i<props.Length;i++) Add("PF_Prop_"+props[i],"Props",props[i]+"; local city identity outside the playable corridor",sizes[i]);
            Add("PF_Background_CommercialTower","Props","Simplified distant commercial skyline",new Vector3(4.5f,10.5f,4),EnvironmentLayer.Background);
            Add("PF_Background_Bangka","Props","Distant Mactan fishing boat and outriggers",new Vector3(2.5f,.6f,3.1f),EnvironmentLayer.Background);
            Add("PF_Background_SoftCloud","Props","Simple opaque distant cloud silhouette; slow parallax",new Vector3(7.5f,2.2f,2.7f),EnvironmentLayer.Background);
            Add("PF_Route_WideStaircase","Props","Optional wide pedestrian stairs; use the parallel ramp for shield riding",new Vector3(4,1.2f,6));
            Add("PF_Route_ElevatedSidewalk","Props","Optional raised dry sidewalk connector",new Vector3(3,1,6));
            foreach(string name in new[]{"RoadBarricade","ParkedVehicle","FallenBranch","BrokenPavement","FallenSign","ConstructionDebris","RoadsidePuddle","BoundedFlood","BlockedPassage"})
                Add("PF_Obstacle_"+name,"Props","Optional readable "+name+" obstacle model; assign an appropriate swept collision rule before spawning",new Vector3(2.2f,name=="ParkedVehicle"?1.7f:1.0f,name=="BoundedFlood"?4:1.6f),EnvironmentLayer.Foreground);
            foreach(string name in new[]{"Straight_Asphalt","Sidewalks","Drainage","Coastal_Promenade","Coastal_Stores","Alley_ThreeLane","Alley_Drainage","Ramp_Gentle","Ramp_Parking","FloodEscape_Ramp","Elevated_Walkway","Refuge_Gate","AlternateRoute","Flooded_Variant","Typhoon_Variant","Earthquake_Aftermath","Underpass","Safe_Descent"})
            {
                float rise=name.Contains("Ramp")?1.2f:name=="Safe_Descent"?-1.2f:0;
                Add("PF_Segment_"+name,"Segments","Reusable 24m three-lane "+name+" module",new Vector3(8.4f,.3f,24),EnvironmentLayer.Foreground,true,rise,3);
            }
            Add("PF_Segment_Alley_SingleLane","Segments","Optional single-lane spur; not used by the three-lane vertical slice",new Vector3(3.2f,.3f,24),EnvironmentLayer.Foreground,true,0,1);
            Add("PF_Player_OpongShieldboard","Player","Existing rigged Opong, expressive face and shield board; eight movement states",new Vector3(1.7f,3.1f,2.7f),EnvironmentLayer.Foreground);
            for(int i=0;i<PreparednessContent.SupplyCount;i++) Add("PF_Supply_"+(SupplyKind)i,"Gameplay","Useful preparedness supply; existing points and single raised jump reward",new Vector3(1,1.3f,1),EnvironmentLayer.Foreground);
            for(int i=0;i<PreparednessContent.DistractorCount;i++) Add("PF_Distractor_"+(DistractorKind)i,"Gameplay","Unnecessary item; five-point penalty without losing a life",new Vector3(1.3f,1.3f,1.1f),EnvironmentLayer.Foreground);
            for(int i=0;i<PreparednessContent.PowerUpCount;i++) Add("PF_PowerUp_"+(PowerUpKind)i,"Gameplay","Ten-second arcade bonus after 500m; spaced at least 400m apart",new Vector3(1.1f,1.3f,1.1f),EnvironmentLayer.Foreground);
            for(int i=0;i<PreparednessContent.HazardCount;i++) Add("PF_Hazard_"+(HazardKind)i,"Gameplay","Existing hazard silhouette and swept collision behavior",new Vector3(i==(int)HazardKind.HangingCable?8.2f:2.5f,2.4f,i==(int)HazardKind.FloodStreak?32:2),EnvironmentLayer.Foreground);
            Add("PF_UI_RunHUD","UI","Readable score and life strip",new Vector3(600,116,0),EnvironmentLayer.ScreenUI);
            Add("PF_UI_RouteBanner","UI","Quiet district and dry-route cue",new Vector3(600,44,0),EnvironmentLayer.ScreenUI);
            Add("PF_UI_HomeLessonCard","UI","End-of-run action to practise with an adult",new Vector3(600,154,0),EnvironmentLayer.ScreenUI);
            return all;
        }
    }
}
