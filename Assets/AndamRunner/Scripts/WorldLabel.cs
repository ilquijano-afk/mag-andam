using UnityEngine;

namespace AndamRunner
{
    // TextMesh's front faces -Z. Keep labels readable from the active camera.
    public sealed class WorldLabel : MonoBehaviour
    {
        private Transform view;
        private void LateUpdate()
        {
            if(view==null && Camera.main!=null) view=Camera.main.transform;
            if(view==null) return;
            Vector3 away=transform.position-view.position; away.y=0;
            if(away.sqrMagnitude>.001f) transform.rotation=Quaternion.LookRotation(away);
        }
    }
}
