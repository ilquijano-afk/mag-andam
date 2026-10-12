using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AndamRunner
{
    // Small reusable sculpting tools. Meshes use smooth normals and no physics colliders.
    public static class StorybookArt
    {
        private static Mesh tuft;
        private static readonly Dictionary<int,Mesh> roundedBoxes=new Dictionary<int,Mesh>();
        private static readonly Dictionary<int,Mesh> spheres=new Dictionary<int,Mesh>();
        public static Mesh Sphere(int segments=12,int rings=8)
        {
            int key=segments*100+rings;
            if(spheres.TryGetValue(key,out Mesh cached)) return cached;
            var vertices=new List<Vector3>(); var normals=new List<Vector3>(); var uv=new List<Vector2>(); var triangles=new List<int>();
            for(int row=0;row<=rings;row++) for(int side=0;side<=segments;side++)
            {
                float a=row*Mathf.PI/rings,b=side*Mathf.PI*2/segments;
                Vector3 n=new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b));
                vertices.Add(n*.5f); normals.Add(n); uv.Add(new Vector2(side/(float)segments,row/(float)rings));
            }
            for(int row=0;row<rings;row++) for(int side=0;side<segments;side++)
            {
                int a=row*(segments+1)+side,b=a+1,c=a+segments+1,d=c+1;
                if(row!=0) triangles.AddRange(new[]{a,b,c});
                if(row!=rings-1) triangles.AddRange(new[]{b,d,c});
            }
            var mesh=new Mesh{name="Smooth chibi sphere "+segments+"x"+rings,vertices=vertices.ToArray(),normals=normals.ToArray(),uv=uv.ToArray(),triangles=triangles.ToArray()};
            spheres[key]=mesh; return mesh;
        }
        public static Mesh RoundedBox(int steps=6)
        {
            if(roundedBoxes.TryGetValue(steps,out Mesh cached)) return cached;
            var vertices=new List<Vector3>(); var normals=new List<Vector3>(); var faces=new List<int>();
            Vector3[] axes={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
            const float bevel=.12f;
            foreach(Vector3 axis in axes)
            {
                Vector3 u=Vector3.Cross(axis,Mathf.Abs(axis.y)>.9f ? Vector3.forward : Vector3.up);
                Vector3 v=Vector3.Cross(axis,u); int start=vertices.Count;
                for(int y=0;y<=steps;y++) for(int x=0;x<=steps;x++)
                {
                    Vector3 p=axis*.5f+u*(x/(float)steps-.5f)+v*(y/(float)steps-.5f);
                    Vector3 core=new Vector3(Mathf.Clamp(p.x,-.5f+bevel,.5f-bevel),Mathf.Clamp(p.y,-.5f+bevel,.5f-bevel),Mathf.Clamp(p.z,-.5f+bevel,.5f-bevel));
                    Vector3 n=(p-core).normalized; vertices.Add(core+n*bevel); normals.Add(n);
                }
                for(int y=0;y<steps;y++) for(int x=0;x<steps;x++)
                { int a=start+y*(steps+1)+x,b=a+1,c=a+steps+1,d=c+1; faces.AddRange(new[]{a,b,d,a,d,c}); }
            }
            Mesh roundedBox=new Mesh{name="Soft beveled storybook block",vertices=vertices.ToArray(),normals=normals.ToArray(),triangles=faces.ToArray()};
            roundedBox.RecalculateBounds(); roundedBoxes[steps]=roundedBox; return roundedBox;
        }

        public static Vector3[] Curve(Vector3 a,Vector3 b,Vector3 c,int steps=16)
        {
            var points=new Vector3[steps+1];
            for(int i=0;i<=steps;i++) { float t=i/(float)steps; points[i]=(1-t)*(1-t)*a+2*(1-t)*t*b+t*t*c; }
            return points;
        }

        public static Mesh Tube(string name,Vector3[] points,float[] radii,float depth=1,int sides=10)
        {
            var vertices=new List<Vector3>(); var faces=new List<int>();
            for(int row=0;row<points.Length;row++)
            {
                Vector3 tangent=(points[Mathf.Min(row+1,points.Length-1)]-points[Mathf.Max(row-1,0)]).normalized;
                Vector3 u=Vector3.Cross(tangent,Mathf.Abs(tangent.z)>.9f ? Vector3.up : Vector3.forward).normalized;
                Vector3 v=Vector3.Cross(tangent,u).normalized;
                for(int side=0;side<=sides;side++)
                { float angle=side*Mathf.PI*2/sides; vertices.Add(points[row]+radii[row]*(u*Mathf.Cos(angle)+v*Mathf.Sin(angle)*depth)); }
            }
            for(int row=0;row<points.Length-1;row++) for(int side=0;side<sides;side++)
            { int a=row*(sides+1)+side,b=a+1,c=a+sides+1,d=c+1; faces.AddRange(new[]{a,b,c,b,d,c}); }
            int bottom=vertices.Count; vertices.Add(points[0]); int top=vertices.Count; vertices.Add(points[points.Length-1]);
            for(int side=0;side<sides;side++)
            { int a=side,b=side+1,c=(points.Length-1)*(sides+1)+side; faces.AddRange(new[]{bottom,b,a,top,c,c+1}); }
            var mesh=new Mesh{name=name,vertices=vertices.ToArray(),triangles=faces.ToArray()}; mesh.RecalculateNormals();
            // Seam normals must agree; otherwise the paper shading reveals an artificial crease.
            Vector3[] normals=mesh.normals;
            for(int row=0;row<points.Length;row++) { int a=row*(sides+1),b=a+sides; Vector3 n=(normals[a]+normals[b]).normalized; normals[a]=normals[b]=n; }
            mesh.normals=normals; return mesh;
        }

        public static Transform Stroke(Transform parent,string name,Vector3 a,Vector3 b,Vector3 c,float radius,Color color,float depth=1)
        {
            Vector3[] points=Curve(a,b,c,6); var widths=new float[points.Length];
            for(int i=0;i<widths.Length;i++) widths[i]=radius*(.70f+.30f*Mathf.Sin(i*Mathf.PI/(widths.Length-1)));
            return PrimitiveArt.MeshPart(parent,name,Tube(name,points,widths,depth,6),Vector3.zero,Vector3.one,color);
        }

        public static Mesh HairTuft()
        {
            if(tuft!=null) return tuft;
            Vector3[] points=Curve(Vector3.zero,new Vector3(-.08f,.64f,0),new Vector3(.28f,1f,.025f),8);
            var widths=new float[points.Length];
            for(int i=0;i<widths.Length;i++) { float t=i/(float)(widths.Length-1); widths[i]=.18f*Mathf.Pow(Mathf.Max(0,Mathf.Sin((.23f+t*.77f)*Mathf.PI)),.7f)+.002f; }
            tuft=Tube("Sculpted tapered hair lock",points,widths,.90f,8); return tuft;
        }

        public static Mesh Ellipse(string name,float[] heights,float[] widths,float[] depths,int sides=32)
        {
            var vertices=new List<Vector3>(); var triangles=new List<int>();
            for(int row=0;row<heights.Length;row++) for(int side=0;side<=sides;side++)
            { float a=side*Mathf.PI*2/sides; vertices.Add(new Vector3(Mathf.Cos(a)*widths[row],heights[row],Mathf.Sin(a)*depths[row])); }
            for(int row=0;row<heights.Length-1;row++) for(int side=0;side<sides;side++)
            { int a=row*(sides+1)+side,b=a+1,c=a+sides+1,d=c+1; triangles.AddRange(new[]{a,c,b,b,c,d}); }
            var mesh=new Mesh{name=name,vertices=vertices.ToArray(),triangles=triangles.ToArray()}; mesh.RecalculateNormals();
            Vector3[] n=mesh.normals;
            for(int row=0;row<heights.Length;row++) { int a=row*(sides+1),b=a+sides; n[a]=n[b]=(n[a]+n[b]).normalized; }
            mesh.normals=n; return mesh;
        }

        public static Transform Band(Transform parent,string name,Vector3 position,float width,float depth,float height,Color color)
        {
            float[] y={-height*.5f,-height*.42f,height*.42f,height*.5f};
            return PrimitiveArt.MeshPart(parent,name,Ellipse(name,y,new[]{width*.97f,width,width,width*.97f},new[]{depth*.97f,depth,depth,depth*.97f},20),position,Vector3.one,color);
        }

        public static Transform Patch(Transform parent,string name,Vector3[] outline,Color color)
        {
            var v=new List<Vector3>(); var t=new List<int>(); Vector3 center=Vector3.zero;
            foreach(Vector3 p in outline) center+=p; center/=outline.Length; v.Add(center); v.AddRange(outline);
            for(int i=0;i<outline.Length;i++) t.AddRange(new[]{0,i+1,(i+1)%outline.Length+1});
            var mesh=new Mesh{name=name,vertices=v.ToArray(),triangles=t.ToArray()}; mesh.RecalculateNormals();
            return PrimitiveArt.MeshPart(parent,name,mesh,Vector3.zero,Vector3.one,color);
        }

        public static void CombineBone(Transform bone,bool descendants=false)
        {
            var groups=new Dictionary<Material,List<CombineInstance>>(); var old=new List<MeshRenderer>();
            foreach(MeshRenderer renderer in bone.GetComponentsInChildren<MeshRenderer>())
            {
                if(!descendants && renderer.transform.parent!=bone) continue;
                MeshFilter filter=renderer.GetComponent<MeshFilter>(); if(filter==null) continue;
                Material mat=renderer.sharedMaterial;
                if(!groups.TryGetValue(mat,out List<CombineInstance> instances)) groups[mat]=instances=new List<CombineInstance>();
                instances.Add(new CombineInstance{mesh=filter.sharedMesh,transform=bone.worldToLocalMatrix*renderer.transform.localToWorldMatrix}); old.Add(renderer);
            }
            foreach(var pair in groups)
            {
                var mesh=new Mesh{name=bone.name+" / "+pair.Key.name}; mesh.CombineMeshes(pair.Value.ToArray(),true,true);
                Transform part=PrimitiveArt.MeshPart(bone,mesh.name,mesh,Vector3.zero,Vector3.one,pair.Key.color);
                part.GetComponent<Renderer>().sharedMaterial=pair.Key;
                part.GetComponent<Renderer>().shadowCastingMode=pair.Value.Count>0 ? old.Find(r=>r.sharedMaterial==pair.Key).shadowCastingMode : ShadowCastingMode.Off;
            }
            foreach(MeshRenderer renderer in old) renderer.gameObject.SetActive(false);
        }
    }
}
