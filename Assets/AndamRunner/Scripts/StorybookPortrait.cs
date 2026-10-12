using UnityEngine;
using UnityEngine.UI;

namespace AndamRunner
{
    // Menu-only transparent portrait of the same animated model used in the run.
    // Layer 8 belongs to Opong. The main camera still includes all layers.
    public sealed class StorybookPortrait : MonoBehaviour
    {
        private Camera portraitCamera;
        private RenderTexture target;
        public void Initialize(GameManager game,RawImage image)
        {
            foreach(Transform part in game.Player.Visual.GetComponentsInChildren<Transform>(true)) part.gameObject.layer=8;
            target=new RenderTexture(512,512,24,RenderTextureFormat.ARGB32){name="Opong live portrait",antiAliasing=2}; target.Create();
            image.texture=target;
            GameObject obj=new GameObject("Menu portrait camera"); obj.transform.SetParent(game.transform,false);
            portraitCamera=obj.AddComponent<Camera>(); portraitCamera.enabled=false;
            portraitCamera.clearFlags=CameraClearFlags.SolidColor; portraitCamera.backgroundColor=Color.clear;
            portraitCamera.cullingMask=1<<8; portraitCamera.orthographic=true; portraitCamera.orthographicSize=1.57f;
            portraitCamera.nearClipPlane=.1f; portraitCamera.farClipPlane=15f; portraitCamera.allowHDR=false;
            portraitCamera.transform.position=new Vector3(-.6f,3f,6.4f);
            portraitCamera.transform.LookAt(new Vector3(0,1.48f,0)); portraitCamera.targetTexture=target;
        }
        private void LateUpdate() { Render(); }
        public void Render() { if(portraitCamera!=null) portraitCamera.Render(); }
        private void OnDestroy()
        {
            if(portraitCamera!=null) Destroy(portraitCamera.gameObject);
            if(target!=null) { target.Release(); Destroy(target); }
        }
    }
}
