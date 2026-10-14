using UnityEngine;

namespace AndamRunner
{
    public enum EnvironmentLayer { Foreground, Midground, Background, ScreenUI }

    public sealed class EnvironmentAssetInfo : MonoBehaviour
    {
        public string AssetId, Purpose;
        public EnvironmentLayer Layer;
        public Vector3 SuggestedSize;
        public bool RoadSegment;
        public int LaneCount, ProminentProps;
        public float SurfaceRise;
        public int Triangles;
    }
}
