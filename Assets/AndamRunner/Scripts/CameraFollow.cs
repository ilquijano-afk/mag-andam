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
            transform.position = new Vector3(0,5f,-13.5f);
            transform.rotation = Quaternion.Euler(12,0,0);
            Camera camera=GetComponent<Camera>();
            camera.fieldOfView=2*Mathf.Atan(Mathf.Tan(28*Mathf.Deg2Rad)*Mathf.Max(1,.5625f/camera.aspect))*Mathf.Rad2Deg;
        }
        public void SetPresentation(bool value)
        {
            presentation=value;
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
            // Fixed angle and horizon: lane changes and slopes never shake or zoom the camera.
            ResetCamera();
        }
    }
}
