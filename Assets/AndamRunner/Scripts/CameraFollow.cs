using UnityEngine;

namespace AndamRunner
{
    public sealed class CameraFollow : MonoBehaviour
    {
        private PlayerRunner player;
        private bool presentation;
        public void Initialize(PlayerRunner runner) { player = runner; ResetCamera(); }
        public void ResetCamera()
        {
            transform.position = new Vector3(0,4.3f,-9f);
            transform.rotation = Quaternion.LookRotation(new Vector3(0,-.2f,11.5f)-transform.position);
        }
        public void SetPresentation(bool value)
        {
            presentation=value;
            CoastalIllustrationBackground background=GetComponent<CoastalIllustrationBackground>();
            if(background!=null) background.SetStorm(!value);
            if(value)
            {
                transform.position=new Vector3(-3.8f,3.2f,6.8f);
                transform.LookAt(new Vector3(0,.72f,0));
            }
        }
        private void LateUpdate()
        {
            if (player == null) return;
            if (presentation) return;
            Vector3 position = new Vector3(player.transform.position.x*.07f,4.3f,-9f);
            transform.position = Vector3.Lerp(transform.position,position,1f-Mathf.Exp(-5f*Time.unscaledDeltaTime));
            transform.rotation = Quaternion.LookRotation(new Vector3(player.transform.position.x*.05f,-.2f,11.5f)-transform.position);
        }
    }
}
