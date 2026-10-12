using UnityEngine;
using UnityEngine.Rendering;

namespace AndamRunner
{
    // A camera-fitted illustration behind the real moving 3D playfield.
    // It also works with Camera.Render and offscreen phone previews.
    [RequireComponent(typeof(Camera))]
    public sealed class CoastalIllustrationBackground : MonoBehaviour
    {
        private Camera view;
        private Transform painting;
        private Material material;
        private Texture2D picture;
        public bool Storm { get; private set; }
        private void Awake()
        {
            view=GetComponent<Camera>(); picture=Resources.Load<Texture2D>("StorybookCoast");
            Material source=Resources.Load<Material>("IllustratedBackdrop");
            if(picture==null || source==null) return;
            painting=PrimitiveArt.Root("Illustrated Barangay Mactan coast",transform,new Vector3(0,0,100));
            Mesh mesh=new Mesh{name="Coastal backdrop quad",vertices=new[]{new Vector3(-1,-1,0),new Vector3(1,-1,0),new Vector3(1,1,0),new Vector3(-1,1,0)},uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up},triangles=new[]{0,1,2,0,2,3}};
            painting.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            MeshRenderer renderer=painting.gameObject.AddComponent<MeshRenderer>();
            material=new Material(source); material.mainTexture=picture; renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
            view.clearFlags=CameraClearFlags.SolidColor;
            Fit();
        }
        private void LateUpdate() { Fit(); }
        public void SetStorm(bool value)
        {
            Texture2D next=Resources.Load<Texture2D>(value?"StormEvacuationCoast":"StorybookCoast");
            if(next==null || material==null) return;
            Storm=value; picture=next; material.mainTexture=picture; Fit();
            Light sunlight=FindAnyObjectByType<Light>();
            if(sunlight!=null) { sunlight.color=value?new Color(.88f,.93f,1):new Color(1,.95f,.83f); sunlight.intensity=value?1:1.05f; }
        }
        public void Fit()
        {
            if(painting==null) return;
            float height=Mathf.Tan(view.fieldOfView*.5f*Mathf.Deg2Rad)*100;
            painting.localScale=new Vector3(height*view.aspect,height,1);
            // Bring the painted road's horizon up to the camera's lane horizon.
            float cropHeight=Storm?1:.76f;
            float width=Mathf.Min(1,view.aspect*cropHeight/((float)picture.width/picture.height));
            material.SetVector("_Crop",new Vector4((1-width)*.5f,0,width,cropHeight));
        }
        private void OnDestroy() { if(material!=null) Destroy(material); }
    }
}
