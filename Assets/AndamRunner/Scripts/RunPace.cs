using UnityEngine;

namespace AndamRunner
{
    // Distance milestones persist after a bump; only the current speed level resets.
    public sealed class RunPace
    {
        public const float BaseSpeed=24f, SpeedStep=2f, MaximumSpeed=46f, Milestone=250f;
        public const int MaximumLevel=6;
        public static float TopSpeed => Mathf.Min(MaximumSpeed,BaseSpeed+MaximumLevel*SpeedStep);
        public float Distance { get; private set; }
        public float NextMilestone { get; private set; }=Milestone;
        public int Level { get; private set; }
        public float Speed => Mathf.Min(MaximumSpeed,BaseSpeed+Level*SpeedStep);
        public void Restart() { Distance=0; Level=0; NextMilestone=Milestone; }
        public void ResetSpeed() { Level=0; }
        public void Advance(float meters)
        {
            if(meters<=0) return;
            Distance+=meters;
            while(Distance>=NextMilestone)
            { Level=Mathf.Min(MaximumLevel,Level+1); NextMilestone+=Milestone; }
        }
    }
}
