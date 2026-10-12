using UnityEngine;

namespace AndamRunner
{
    // Collision is swept along the road, so low frame rates cannot tunnel through objects.
    // Visual primitives have no physics colliders; one system owns every contact and score.
    public sealed class TrackEntity : MonoBehaviour
    {
        public bool IsSupply { get; private set; }
        public bool IsDistractor { get; private set; }
        public bool IsPickup => IsSupply || IsDistractor;
        public SupplyKind Supply { get; private set; }
        public DistractorKind Distractor { get; private set; }
        public HazardKind Hazard { get; private set; }
        public int Lane { get; private set; }
        public bool Consumed { get; private set; }
        public bool Warned { get; set; }
        public bool SpansAllLanes => !IsPickup && Hazard==HazardKind.HangingCable;
        private Transform art;
        private float phase;

        public void ConfigureSupply(SupplyKind kind, int lane, Transform visual)
        {
            IsSupply = true; IsDistractor=false; Supply = kind; Lane = lane; art = visual; phase = lane * 1.8f;
            gameObject.SetActive(false);
        }
        public void ConfigureHazard(HazardKind kind, int lane, Transform visual)
        {
            IsSupply = false; IsDistractor=false; Hazard = kind; Lane = lane; art = visual;
            gameObject.SetActive(false);
        }
        public void ConfigureDistractor(DistractorKind kind,int lane,Transform visual)
        {
            IsSupply=false; IsDistractor=true; Distractor=kind; Lane=lane; art=visual; phase=lane*1.8f;
            gameObject.SetActive(false);
        }
        public void Spawn(float z)
        {
            Consumed = false; Warned = false;
            transform.position = new Vector3((Lane - 1) * PlayerRunner.LaneWidth, 0f, z);
            if (art != null) art.localPosition = Vector3.zero;
            gameObject.SetActive(true);
        }
        public void Consume() { Consumed = true; gameObject.SetActive(false); }

        public void Animate(float time)
        {
            if (!IsPickup) return;
            art.localPosition = new Vector3(0f, Mathf.Sin(time * 3f + phase) * 0.11f, 0f);
            art.localRotation = Quaternion.Euler(0f, Mathf.Sin(time * 1.6f + phase) * 18f, 0f);
        }

        public bool IsContact(Vector3 playerFrom, Vector3 playerTo, float fromZ, float toZ,float duckRemainingAtStart=0,float delta=0)
        {
            if (Consumed || !gameObject.activeSelf) return false;
            float halfDepth = IsPickup ? 0.65f : Hazard == HazardKind.Floodwater ? 2.4f : 0.9f;
            if (toZ > halfDepth || fromZ < -halfDepth) return false;
            float denominator = fromZ - toZ;
            float t = denominator > 0.00001f ? Mathf.Clamp01(fromZ / denominator) : 1f;
            Vector3 atCrossing = Vector3.Lerp(playerFrom, playerTo, t);
            if(SpansAllLanes)
            {
                if(Mathf.Abs(atCrossing.x)>PlayerRunner.LaneWidth*1.5f) return false;
                bool ducking=duckRemainingAtStart-delta*t>0 && atCrossing.y<.1f;
                return !ducking;
            }
            if (Mathf.Abs(atCrossing.x - transform.position.x) > (IsPickup ? 0.95f : 1.15f)) return false;
            if (IsPickup) return atCrossing.y < 1.5f;
            // Flood/wire/block zones are forbidden at every jump height. Board cannot make them safe.
            return Hazard != HazardKind.Debris || atCrossing.y < 0.95f;
        }
    }
}
