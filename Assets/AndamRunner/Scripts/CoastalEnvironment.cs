using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AndamRunner
{
    public sealed class CoastalEnvironment : MonoBehaviour
    {
        private RepeatingRoad road;
        private Camera view;
        private Light sunlight;
        private Material sky, palette, backgroundPalette, water, rain;
        private Transform background, sea, rainRoot;
        private readonly List<Transform> skyline=new List<Transform>();
        private readonly List<Vector3> skylineOrigins=new List<Vector3>();
        public float StormAmount { get; private set; }

        public void Initialize(RepeatingRoad movingRoad,Camera camera,Light sun)
        {
            road=movingRoad; view=camera; sunlight=sun;
            sky=new Material(Resources.Load<Material>("CoastalSky"));
            palette=new Material(road.Pack.Palette); backgroundPalette=new Material(road.Pack.BackgroundPalette);
            water=new Material(road.Pack.Water); rain=new Material(road.Pack.Rain);
            // Material substitutions happen once, across both active and pooled views.
            foreach(Renderer renderer in road.GetComponentsInChildren<Renderer>(true))
            {
                if(renderer.sharedMaterial==road.Pack.Palette) renderer.sharedMaterial=palette;
                else if(renderer.sharedMaterial==road.Pack.Water) renderer.sharedMaterial=water;
            }
            RenderSettings.skybox=sky;
            background=PrimitiveArt.Root("Quiet distant island skyline",transform,Vector3.zero);
            for(int i=0;i<8;i++)
            {
                string id=i%3==0?"PF_Background_CommercialTower":"PF_Prop_ResidentialBlock";
                Transform building=Instantiate(road.Pack.Find(id),background,false).transform;
                Vector3 p=new Vector3(i%2==0?-23-i:23+i,0,68+i*10);
                building.localPosition=p; building.localScale=Vector3.one*(i%3==0?.9f:.75f);
                foreach(Renderer renderer in building.GetComponentsInChildren<Renderer>()) { renderer.sharedMaterial=backgroundPalette; renderer.shadowCastingMode=ShadowCastingMode.Off; }
                skyline.Add(building); skylineOrigins.Add(p);
            }
            Transform boat=Instantiate(road.Pack.Find("PF_Background_Bangka"),background,false).transform;
            boat.localPosition=new Vector3(18,-.50f,52); skyline.Add(boat); skylineOrigins.Add(boat.localPosition);
            foreach(Renderer renderer in boat.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial=backgroundPalette;
            for(int i=0;i<3;i++)
            {
                Transform cloud=Instantiate(road.Pack.Find("PF_Background_SoftCloud"),background,false).transform;
                cloud.localScale=Vector3.one*(i==1?1.6f:1.2f);
                cloud.localPosition=new Vector3(-16+i*16,17+i*2,84+i*14);
                foreach(Renderer renderer in cloud.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial=backgroundPalette;
                skyline.Add(cloud); skylineOrigins.Add(cloud.localPosition);
            }
            sea=PrimitiveArt.Shape(PrimitiveType.Cube,background,"Quiet opaque ocean",new Vector3(34,-.75f,55),new Vector3(50,.08f,200),EnvironmentModelFactory.Teal);
            sea.GetComponent<Renderer>().sharedMaterial=water;
            rainRoot=PrimitiveArt.Root("Restrained rain behind gameplay",view.transform,new Vector3(0,0,35));
            MeshFilter filter=rainRoot.gameObject.AddComponent<MeshFilter>(); filter.sharedMesh=RainMesh();
            MeshRenderer shower=rainRoot.gameObject.AddComponent<MeshRenderer>(); shower.sharedMaterial=rain; shower.shadowCastingMode=ShadowCastingMode.Off; shower.receiveShadows=false;
            Apply();
        }
        private static Mesh RainMesh()
        {
            var vertices=new List<Vector3>(); var uv=new List<Vector2>(); var triangles=new List<int>();
            for(int i=0;i<18;i++)
            {
                float x=-11+(i*7%23), y=15+(i%3)*.8f, phase=(i*.618f)%1;
                int n=vertices.Count;
                vertices.AddRange(new[]{new Vector3(x-.016f,y,0),new Vector3(x+.016f,y,0),new Vector3(x-.17f,y-.65f,0),new Vector3(x-.20f,y-.65f,0)});
                for(int j=0;j<4;j++) uv.Add(new Vector2(phase,0));
                triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            var mesh=new Mesh{name="18 quiet rain streaks"}; mesh.SetVertices(vertices); mesh.SetUVs(0,uv); mesh.SetTriangles(triangles,0); mesh.bounds=new Bounds(Vector3.zero,new Vector3(26,40,1)); return mesh;
        }
        public void Apply()
        {
            if(road==null) return;
            StormAmount=CoastalRouteProfile.Storm(road.Distance);
            sky.SetColor("_Top",Color.Lerp(new Color32(132,196,219,255),new Color32(120,148,172,255),StormAmount));
            sky.SetColor("_Horizon",Color.Lerp(new Color32(221,232,224,255),new Color32(190,209,216,255),StormAmount));
            palette.SetColor("_Tint",Color.Lerp(Color.white,new Color(.90f,.94f,.99f),StormAmount));
            rain.SetFloat("_Amount",StormAmount*.65f); rainRoot.gameObject.SetActive(StormAmount>.01f);
            sunlight.color=Color.Lerp(new Color(1,.96f,.86f),new Color(.90f,.95f,1),StormAmount); sunlight.intensity=1.0f;
            RenderSettings.fog=true; RenderSettings.fogMode=FogMode.Linear; RenderSettings.fogStartDistance=70; RenderSettings.fogEndDistance=145;
            RenderSettings.fogColor=Color.Lerp(new Color32(205,225,225,255),new Color32(185,208,219,255),StormAmount);
            view.backgroundColor=RenderSettings.fogColor;
            float ground=CoastalRouteProfile.Elevation(road.Distance);
            sea.localPosition=new Vector3(34,-.75f-ground,55);
            for(int i=0;i<skyline.Count;i++)
            {
                Vector3 p=skylineOrigins[i]; p.z=45+Mathf.Repeat(p.z-45-road.Distance*.08f,90); p.y-=ground; skyline[i].localPosition=p;
            }
        }
        private void LateUpdate() { Apply(); }
        private void OnDestroy()
        {
            foreach(Material material in new[]{sky,palette,backgroundPalette,water,rain}) if(material!=null) Destroy(material);
        }
    }
}
