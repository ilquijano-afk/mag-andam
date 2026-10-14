using UnityEngine;

namespace AndamRunner
{
    // Collision is swept along the road, so low frame rates cannot tunnel through objects.
    // Visual primitives have no physics colliders; one system owns every contact and score.
    public sealed class TrackEntity : MonoBehaviour
    {
        public bool IsSupply { get; private set; }
        public bool IsDistractor { get; private set; }
        public bool IsPowerUp { get; private set; }
        public bool IsPickup => IsSupply || IsDistractor || IsPowerUp;
        public PowerUpKind PowerUp { get; private set; }
        public float PickupHeight { get; private set; }
        public bool IsJumpReward => IsSupply && PickupHeight>1.5f;
        public SupplyKind Supply { get; private set; }
        public DistractorKind Distractor { get; private set; }
        public HazardKind Hazard { get; private set; }
        public int Lane { get; private set; }
        public bool Consumed { get; private set; }
        public bool Warned { get; set; }
        public bool SpansAllLanes => !IsPickup && Hazard==HazardKind.HangingCable;
        public float HalfDepth => IsPickup ? .65f : Hazard switch
        {
            HazardKind.Floodwater => 2.4f,
            HazardKind.FloodStreak => PreparednessContent.FloodStreakLength*.5f,
            _ => .9f
        };
        private Transform art;
        private float phase;

        public void ConfigureSupply(SupplyKind kind, int lane, Transform visual)
        {
            IsSupply = true; IsDistractor=false; IsPowerUp=false; Supply = kind; Lane = lane; art = visual; phase = lane * 1.8f;
            gameObject.SetActive(false);
        }
        public void ConfigureHazard(HazardKind kind, int lane, Transform visual)
        {
            IsSupply = false; IsDistractor=false; IsPowerUp=false; Hazard = kind; Lane = lane; art = visual;
            gameObject.SetActive(false);
        }
        public void ConfigureDistractor(DistractorKind kind,int lane,Transform visual)
        {
            IsSupply=false; IsDistractor=true; IsPowerUp=false; Distractor=kind; Lane=lane; art=visual; phase=lane*1.8f;
            gameObject.SetActive(false);
        }
        public void ConfigurePowerUp(PowerUpKind kind,int lane,Transform visual)
        {
            IsSupply=false; IsDistractor=false; IsPowerUp=true; PowerUp=kind; Lane=lane; art=visual; phase=lane*1.8f;
            gameObject.SetActive(false);
        }
        public void Spawn(float z,float height=0)
        {
            Consumed = false; Warned = false;
            PickupHeight=height;
            transform.position = new Vector3((Lane - 1) * PlayerRunner.LaneWidth, height, z);
            if (art != null) { art.localPosition = Vector3.zero; art.localRotation=Quaternion.identity; }
            gameObject.SetActive(true);
        }
        public void Consume() { Consumed = true; gameObject.SetActive(false); }

        public void Animate(float time,float surfaceHeight=0,float roadSlope=0)
        {
            art.localPosition = new Vector3(0f,surfaceHeight+(IsPickup?Mathf.Sin(time*3f+phase)*.11f:0),0f);
            art.localRotation = Quaternion.Euler(-Mathf.Atan(roadSlope)*Mathf.Rad2Deg,IsPickup?Mathf.Sin(time*1.6f+phase)*18f:0,0);
        }

        public bool IsContact(Vector3 playerFrom, Vector3 playerTo, float fromZ, float toZ,float duckRemainingAtStart=0,float delta=0)
        {
            if (Consumed || !gameObject.activeSelf) return false;
            float halfDepth = HalfDepth;
            if (toZ > halfDepth || fromZ < -halfDepth) return false;
            if (!IsPickup && !SpansAllLanes)
            {
                // The shield must clear the whole ground obstacle, including the long water patch.
                // Restrict the sweep to the time actually inside its lane and footprint: a landing
                // behind an obstacle must not turn a successful jump into a delayed hit.
                float enter=0f, exit=1f;
                if (!ClipSweep(fromZ,toZ,halfDepth,ref enter,ref exit) ||
                    !ClipSweep(playerFrom.x-transform.position.x,playerTo.x-transform.position.x,1.15f,ref enter,ref exit)) return false;
                float lowestHeight=Mathf.Min(Mathf.Lerp(playerFrom.y,playerTo.y,enter),Mathf.Lerp(playerFrom.y,playerTo.y,exit));
                return lowestHeight < GroundClearanceHeight;
            }
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
            if (IsPickup) return Mathf.Abs(atCrossing.y-transform.position.y) < (IsJumpReward ? .65f : 1.5f);
            return false;
        }

        // Heights include a small margin above the matching artwork; Opong's shield is at root Y.
        private float GroundClearanceHeight => Hazard switch
        {
            HazardKind.Floodwater => .75f,
            HazardKind.FloodStreak => .75f,
            HazardKind.PowerLine => 1.05f,
            HazardKind.Debris => .65f,
            _ => 1.15f
        };

        private static bool ClipSweep(float from,float to,float halfExtent,ref float enter,ref float exit)
        {
            float movement=to-from;
            if (Mathf.Abs(movement)<.00001f) return Mathf.Abs(from)<=halfExtent;
            float first=(-halfExtent-from)/movement, last=(halfExtent-from)/movement;
            enter=Mathf.Max(enter,Mathf.Min(first,last));
            exit=Mathf.Min(exit,Mathf.Max(first,last));
            return enter<=exit;
        }
    }
}
