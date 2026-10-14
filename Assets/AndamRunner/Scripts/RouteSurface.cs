using UnityEngine;

namespace AndamRunner
{
    public sealed class RouteSurface : MonoBehaviour
    {
        private Mesh mesh;
        private Vector3[] original, changed, baseNormals, normals;
        private float sourceRise;
        private float fittedStart=float.NaN;
        public int VertexCount => changed.Length;
        public void Initialize(float rise)
        {
            sourceRise=rise;
            MeshFilter filter=GetComponent<MeshFilter>();
            mesh=Instantiate(filter.sharedMesh); mesh.name="Pooled curved road surface"; mesh.MarkDynamic(); filter.sharedMesh=mesh;
            original=mesh.vertices; baseNormals=mesh.normals; changed=new Vector3[original.Length]; normals=new Vector3[original.Length];
        }
        public void Fit(float worldStart,float playerDistance)
        {
            float playerHeight=CoastalRouteProfile.Elevation(playerDistance);
            transform.localPosition=new Vector3(0,-playerHeight,0);
            if(fittedStart==worldStart) return;
            fittedStart=worldStart;
            for(int i=0;i<original.Length;i++)
            {
                Vector3 p=original[i]; float d=worldStart+p.z;
                p.y+=CoastalRouteProfile.Elevation(d)-sourceRise*p.z/CoastalRouteProfile.SegmentLength;
                changed[i]=p;
                Vector3 normal=baseNormals[i];
                normal.z-=(CoastalRouteProfile.Slope(d)-sourceRise/CoastalRouteProfile.SegmentLength)*normal.y;
                normals[i]=normal.normalized;
            }
            mesh.vertices=changed; mesh.normals=normals; mesh.RecalculateBounds();
        }
        private void OnDestroy() { if(mesh!=null) Destroy(mesh); }
    }
}
