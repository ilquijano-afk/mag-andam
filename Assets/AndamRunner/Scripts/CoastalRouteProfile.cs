using UnityEngine;

namespace AndamRunner
{
    public enum CoastalDistrict { Coast, Alley, Uphill, Refuge, Descent }

    // Distances stay on the gameplay Z axis. Visual height is measured relative to
    // the rider, keeping the chase camera steady without changing collision logic.
    public static class CoastalRouteProfile
    {
        public const float CycleLength=960f, SegmentLength=24f, RoadWidth=8.4f, SummitHeight=4f;
        public static float Cycle(float distance) => Mathf.Repeat(distance,CycleLength);
        public static CoastalDistrict District(float distance)
        {
            float d=Cycle(distance);
            return d<240 || d>=840 ? CoastalDistrict.Coast : d<480 ? CoastalDistrict.Alley : d<600 ? CoastalDistrict.Uphill : d<720 ? CoastalDistrict.Refuge : CoastalDistrict.Descent;
        }
        public static float Elevation(float distance)
        {
            float d=Cycle(distance);
            if(d<480 || d>=840) return 0;
            if(d<600) return SummitHeight*Mathf.SmoothStep(0,1,(d-480)/120);
            if(d<720) return SummitHeight;
            return SummitHeight*(1-Mathf.SmoothStep(0,1,(d-720)/120));
        }
        public static float Slope(float distance) => (Elevation(distance+.2f)-Elevation(distance-.2f))/.4f;
        public static float Storm(float distance)
        {
            float d=Cycle(distance);
            return Mathf.SmoothStep(0,1,(d-408)/144)*(1-Mathf.SmoothStep(0,1,(d-744)/144));
        }
        public static string RouteCaption(float distance)
        {
            switch(District(distance))
            {
                case CoastalDistrict.Alley: return "BARANGAY ALLEY  •  FOLLOW THE CLEAR ROUTE";
                case CoastalDistrict.Uphill: return "UPHILL ESCAPE  •  STAY ON THE DRY ROUTE";
                case CoastalDistrict.Refuge: return "DRY HIGH GROUND  •  EVACUATION CENTER";
                case CoastalDistrict.Descent: return "SAFE RAMP  •  KEEP YOUR ROUTE CLEAR";
                default: return "COASTAL ROAD  •  TO THE EVACUATION CENTER";
            }
        }
        public static string HomeLesson(float distance)
        {
            if(distance>=480) return "With an adult, practise your route to dry high ground. Never enter floodwater or go near fallen wires. Follow official evacuation instructions.";
            if(distance>=240) return "With an adult, learn your barangay's evacuation route and meeting place. Keep passages clear and help prepare your family's go-bag.";
            return "Before a storm, prepare your go-bag with an adult. Check water, food, first aid and family contacts. Learn where your family will meet.";
        }
    }
}
