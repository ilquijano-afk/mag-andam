using UnityEngine;
using UnityEngine.Rendering;

namespace AndamRunner
{
    public sealed class OpongContactShadow : MonoBehaviour
    {
        private PlayerRunner player;
        private Mesh mesh;
        private Material material;
        public void Initialize(PlayerRunner runner)
        {
            player=runner;
            const int sides=20;
            var vertices=new Vector3[sides+1]; var colors=new Color[sides+1]; var triangles=new int[sides*3];
            colors[0]=new Color(.13f,.20f,.23f,.19f);
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides; vertices[i+1]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)); colors[i+1]=new Color(.13f,.20f,.23f,0);
                triangles[i*3]=0; triangles[i*3+1]=1+(i+1)%sides; triangles[i*3+2]=i+1;
            }
            mesh=new Mesh{name="Soft 20-triangle Opong contact shadow",vertices=vertices,colors=colors,triangles=triangles}; mesh.RecalculateBounds();
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            material=new Material(Resources.Load<EnvironmentAssetPack>(EnvironmentAssetPack.ResourcePath).Shadow);
            MeshRenderer renderer=gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial=material; renderer.shadowCastingMode=ShadowCastingMode.Off; renderer.receiveShadows=false;
            Fit();
        }
        public void Fit()
        {
            if(player==null) return;
            transform.position=new Vector3(player.transform.position.x,.025f,0);
            transform.localScale=new Vector3(1.05f-player.Height*.08f,1,1.42f-player.Height*.09f);
        }
        private void LateUpdate() { Fit(); }
        private void OnDestroy() { if(mesh!=null) Destroy(mesh); if(material!=null) Destroy(material); }
    }
}
