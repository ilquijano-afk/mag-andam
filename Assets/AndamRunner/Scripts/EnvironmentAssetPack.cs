using UnityEngine;

namespace AndamRunner
{
    [CreateAssetMenu(menuName="ANDAM/Environment asset pack")]
    public sealed class EnvironmentAssetPack : ScriptableObject
    {
        public const string ResourcePath="Environment/EnvironmentAssetPack";
        public bool Graybox;
        public Material Palette, GameplayPalette, BackgroundPalette, Water, Rain, Shadow;
        public GameObject[] Assets;

        public GameObject Find(string id)
        {
            foreach(GameObject asset in Assets) if(asset!=null && asset.name==id) return asset;
            return null;
        }
        public GameObject Segment(float distance,int variation)
        {
            CoastalDistrict district=CoastalRouteProfile.District(distance+12);
            string id;
            switch(district)
            {
                case CoastalDistrict.Alley: id=variation%2==0?"PF_Segment_Alley_ThreeLane":"PF_Segment_Alley_Drainage"; break;
                case CoastalDistrict.Uphill: id="PF_Segment_FloodEscape_Ramp"; break;
                case CoastalDistrict.Refuge: id=CoastalRouteProfile.Cycle(distance)<624?"PF_Segment_Refuge_Gate":"PF_Segment_Elevated_Walkway"; break;
                case CoastalDistrict.Descent: id="PF_Segment_Safe_Descent"; break;
                default: id=variation%2==0?"PF_Segment_Coastal_Promenade":"PF_Segment_Coastal_Stores"; break;
            }
            return Find(id);
        }
    }
}
