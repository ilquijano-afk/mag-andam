using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AndamRunner
{
    // Original procedural scenery and shield geometry; shared materials reduce memory/draw cost.
    public static class PrimitiveArt
    {
        private static readonly Dictionary<Color, Material> Materials = new Dictionary<Color, Material>();
        private static Mesh pyramid;
        private static Mesh board;
        private static Mesh triangle;
        public static readonly Color Skin = new Color32(234, 177, 113, 255);
        public static readonly Color Rust = new Color32(174, 94, 68, 255);
        public static readonly Color Hair = new Color32(31, 35, 37, 255);
        public static readonly Color Gold = new Color32(244, 191, 70, 255);
        public static readonly Color Sea = new Color32(91, 181, 190, 255);

        public static Material Mat(Color color)
        {
            if (Materials.TryGetValue(color, out Material material) && material != null) return material;
            Material source = Resources.Load<Material>("AndamBase");
            material = source != null ? new Material(source) : new Material(Shader.Find("Standard"));
            material.name = "Andam " + ColorUtility.ToHtmlStringRGB(color);
            material.color = color;
            material.enableInstancing = true;
            material.SetFloat("_Glossiness", 0.08f);
            material.SetFloat("_Sheen", Mathf.Max(color.r,Mathf.Max(color.g,color.b))<.30f ? .18f : .025f);
            Materials[color] = material;
            return material;
        }

        public static Transform Root(string name, Transform parent, Vector3 position)
        {
            Transform result = new GameObject(name).transform;
            result.SetParent(parent, false);
            result.localPosition = position;
            return result;
        }

        public static Transform Shape(PrimitiveType type, Transform parent, string name, Vector3 position, Vector3 scale, Color color, Vector3 rotation = default)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            obj.transform.localRotation = Quaternion.Euler(rotation);
            Collider collider = obj.GetComponent<Collider>();
            if (Application.isPlaying) Object.Destroy(collider); else Object.DestroyImmediate(collider);
            Renderer renderer = obj.GetComponent<Renderer>();
            renderer.sharedMaterial = Mat(color);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return obj.transform;
        }

        public static Transform Box(Transform parent, string name, Vector3 position, Vector3 scale, Color color, Vector3 rotation = default)
        {
            // Long ground strips stay square so recycled road chunks meet without gaps.
            if(Mathf.Min(scale.x,Mathf.Min(scale.y,scale.z))<.03f || Mathf.Max(scale.x,Mathf.Max(scale.y,scale.z))>15f)
                return Shape(PrimitiveType.Cube,parent,name,position,scale,color,rotation);
            return MeshPart(parent,name,StorybookArt.RoundedBox(),position,scale,color,rotation);
        }
        public static Transform Ball(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        { return Shape(PrimitiveType.Sphere, parent, name, position, scale, color); }

        public static Transform MeshPart(Transform parent, string name, Mesh mesh, Vector3 position, Vector3 scale, Color color, Vector3 rotation = default)
        {
            Transform obj = Root(name, parent, position);
            obj.localScale = scale;
            obj.localRotation = Quaternion.Euler(rotation);
            obj.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = obj.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = Mat(color);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return obj;
        }

        public static Mesh Pyramid()
        {
            if (pyramid != null) return pyramid;
            pyramid = new Mesh { name = "Low-poly pyramid" };
            Vector3 a = new Vector3(-0.5f, 0, -0.5f), b = new Vector3(0.5f, 0, -0.5f);
            Vector3 c = new Vector3(0.5f, 0, 0.5f), d = new Vector3(-0.5f, 0, 0.5f), top = Vector3.up;
            pyramid.vertices = new[] { a, top, b, b, top, c, c, top, d, d, top, a, a, b, c, a, c, d };
            pyramid.triangles = new[] { 0,1,2, 3,4,5, 6,7,8, 9,10,11, 12,13,14, 15,16,17 };
            pyramid.RecalculateNormals();
            return pyramid;
        }

        private static Mesh ShieldMesh()
        {
            if (board != null) return board;
            // A flat, elongated shield outline with shallow notches at both ends.
            Vector2[] rim = { new Vector2(-.48f,-1.15f), new Vector2(-.25f,-1.27f), new Vector2(0,-1.15f), new Vector2(.25f,-1.27f),
                new Vector2(.48f,-1.15f), new Vector2(.62f,-.7f), new Vector2(.62f,.7f), new Vector2(.48f,1.15f),
                new Vector2(.25f,1.27f), new Vector2(0,1.15f), new Vector2(-.25f,1.27f), new Vector2(-.48f,1.15f),
                new Vector2(-.62f,.7f), new Vector2(-.62f,-.7f) };
            var smooth=new List<Vector2>();
            for(int i=0;i<rim.Length;i++) for(int step=0;step<4;step++)
            {
                float t=step/4f;
                Vector2 a= rim[(i+rim.Length-1)%rim.Length],b=rim[i],c=rim[(i+1)%rim.Length],d=rim[(i+2)%rim.Length];
                smooth.Add(.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));
            }
            var vertices = new List<Vector3>(); var faces = new List<int>();
            for (int i = 0; i < smooth.Count; i++)
            {
                Vector2 a = smooth[i], b = smooth[(i + 1) % smooth.Count];
                Vector3 ta=new Vector3(a.x*.97f,.06f,a.y*.97f),tb=new Vector3(b.x*.97f,.06f,b.y*.97f);
                Vector3 ua=new Vector3(a.x,.025f,a.y),ub=new Vector3(b.x,.025f,b.y);
                Vector3 la=new Vector3(a.x,-.025f,a.y),lb=new Vector3(b.x,-.025f,b.y);
                Vector3 ba=new Vector3(a.x*.97f,-.06f,a.y*.97f),bb=new Vector3(b.x*.97f,-.06f,b.y*.97f);
                int start=vertices.Count;
                vertices.AddRange(new[]{new Vector3(0,.06f,0),tb,ta, ba,bb,new Vector3(0,-.06f,0), ta,tb,ub,ta,ub,ua, ua,ub,lb,ua,lb,la, la,lb,bb,la,bb,ba});
                for(int j=0;j<24;j++) faces.Add(start+j);
            }
            board = new Mesh { name = "Opong rounded beveled shield", vertices = vertices.ToArray(), triangles = faces.ToArray() };
            board.RecalculateNormals(); return board;
        }

        private static Mesh Triangle()
        {
            if (triangle != null) return triangle;
            triangle = new Mesh { name = "Shield inlay", vertices = new[] { new Vector3(-.10f,0,-.13f), new Vector3(0,0,.13f), new Vector3(.10f,0,-.13f) }, triangles = new[] { 0,1,2 } };
            triangle.RecalculateNormals(); return triangle;
        }

        public static Transform CreateOpong(Transform parent)
        {
            Transform visual = Root("Opong + shield board", parent, Vector3.zero);
            Transform shield = Root("White shield surfboard (dry road only)", visual, new Vector3(0,.20f,0));
            MeshPart(shield, "Navy rim", ShieldMesh(), Vector3.zero, new Vector3(1.12f,1f,1.06f), PreparednessContent.Ink);
            MeshPart(shield, "White shield", ShieldMesh(), new Vector3(0,.04f,0), Vector3.one, PreparednessContent.Cream);
            for (int i = 0; i < 7; i++)
            {
                float z = -.9f + i * .3f;
                MeshPart(shield, "Blue mint trim L", Triangle(), new Vector3(-.45f,.106f,z), Vector3.one, i%2 == 0 ? PreparednessContent.Teal : PreparednessContent.Mint, new Vector3(0,90,0));
                MeshPart(shield, "Blue mint trim R", Triangle(), new Vector3(.45f,.106f,z), Vector3.one, i%2 == 0 ? PreparednessContent.Ink : PreparednessContent.Mint, new Vector3(0,-90,0));
            }
            MeshPart(shield,"Resilience emblem",StorybookArt.Sphere(10,6),new Vector3(0,.11f,.8f),new Vector3(.22f,.025f,.22f),PreparednessContent.Teal);
            Box(shield,"Emblem leaf",new Vector3(.13f,.11f,.7f),new Vector3(.1f,.018f,.2f),PreparednessContent.Mint,new Vector3(0,30,0));
            OpongModel.Create(visual);
            StorybookArt.CombineBone(shield,true);
            return visual;
        }

        public static Transform SupplyArt(Transform parent, SupplyKind kind)
        { return RunnerReferenceArt.Supply(parent,kind); }

        public static Transform HazardArt(Transform parent, HazardKind kind)
        { return RunnerReferenceArt.Hazard(parent,kind); }

        public static Transform Label(Transform parent,string message,Vector3 position,float size,Color color)
        {
            Transform label = Root(message,parent,position);
            label.localRotation = Quaternion.identity;
            label.gameObject.AddComponent<WorldLabel>();
            TextMesh text = label.gameObject.AddComponent<TextMesh>();
            text.text = message; text.characterSize = size; text.fontSize = 48;
            text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.color = color;
            label.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            return label;
        }
    }
}
