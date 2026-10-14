using UnityEngine;

namespace AndamRunner
{
    public static class EnvironmentSegmentFactory
    {
        public static Transform Create(string id,Transform parent,bool graybox)
        {
            Transform root=PrimitiveArt.Root(id,parent,Vector3.zero);
            Transform surface=PrimitiveArt.Root("Surface",root,Vector3.zero);
            bool single=id.Contains("SingleLane"), alley=id.Contains("Alley"), ramp=id.Contains("Ramp") || id.Contains("Descent");
            bool flood=id.Contains("Flood"), coast=id.Contains("Coastal"), refuge=id.Contains("Refuge") || id.Contains("Elevated");
            bool storm=id.Contains("Typhoon"), underpass=id.Contains("Underpass");
            float width=single?3.2f:CoastalRouteProfile.RoadWidth, walk=alley?.85f:1.65f;
            Color road=ramp || refuge?new Color32(117,133,139,255):EnvironmentModelFactory.Asphalt;
            if(storm) road=new Color32(67,88,104,255);
            // Short surface cells are deformed along the gentle route profile at runtime.
            for(int cell=0;cell<12;cell++)
            {
                float z=cell*2+1;
                Box(surface,"Road cell",new Vector3(0,-.15f,z),new Vector3(width,.30f,2),road);
                foreach(int side in new[]{-1,1})
                {
                    Box(surface,"Raised sidewalk",new Vector3(side*(width*.5f+walk*.5f),-.015f,z),new Vector3(walk,.27f,2),EnvironmentModelFactory.Concrete);
                    Box(surface,"Visible curb",new Vector3(side*(width*.5f+.055f),.09f,z),new Vector3(.11f,.18f,2),EnvironmentModelFactory.Cream);
                    Box(surface,"Continuous road boundary",new Vector3(side*(width*.5f-.14f),.012f,z),new Vector3(.12f,.024f,2),EnvironmentModelFactory.Cream);
                    if(id.Contains("Drainage")) Box(surface,"Bounded drainage channel",new Vector3(side*(width*.5f+walk-.20f),.124f,z),new Vector3(.26f,.018f,2),EnvironmentModelFactory.Ink);
                    if(ramp || refuge)
                        Box(surface,"Ramp handrail",new Vector3(side*(width*.5f+walk-.15f),1.10f,z),new Vector3(.12f,.12f,2),EnvironmentModelFactory.Cream);
                }
                if(coast) Box(surface,"Low continuous seawall",new Vector3(width*.5f+walk-.10f,.38f,z),new Vector3(.30f,.77f,2),EnvironmentModelFactory.Cream);
            }
            if(!single) foreach(int side in new[]{-1,1}) for(int dash=0;dash<4;dash++)
                Box(surface,"Lane dash",new Vector3(side*PlayerRunner.LaneWidth*.5f,.018f,1.8f+dash*6),new Vector3(.11f,.025f,2.2f),EnvironmentModelFactory.Cream);
            if(ramp || refuge || id.Contains("Alternate"))
                for(int i=0;i<2;i++) EnvironmentModelFactory.Arrow(surface,new Vector3(0,.028f,6+i*12),EnvironmentModelFactory.SafeGreen);
            if(ramp || refuge) foreach(int side in new[]{-1,1}) for(int i=0;i<3;i++)
                Box(surface,"Handrail post",new Vector3(side*(width*.5f+walk-.15f),.60f,2+i*10),new Vector3(.13f,1.05f,.13f),EnvironmentModelFactory.Concrete);
            Transform props=PrimitiveArt.Root("Props",root,Vector3.zero);
            if(alley)
            {
                Place(props,"PF_Prop_House_Cream",new Vector3(single?-4.10f:-7.05f,0,5),-90,graybox);
                Place(props,"PF_Prop_SariSariStore",new Vector3(single?4.75f:7.65f,0,7),90,graybox);
                Place(props,"PF_Prop_House_Coral",new Vector3(single?-4.10f:-7.05f,0,18),-90,graybox);
                Place(props,"PF_Prop_Streetlight",new Vector3(single?2.15f:6.1f,0,21),0,graybox);
            }
            else if(refuge)
            {
                Place(props,id.Contains("Gate")?"PF_Prop_EvacuationCenter":"PF_Prop_House_Cream",new Vector3(-9,0,12),-90,graybox);
                Place(props,"PF_Prop_TropicalTree",new Vector3(7.1f,0,7),0,graybox);
                Place(props,"PF_Prop_Streetlight",new Vector3(6.2f,0,21),0,graybox);
            }
            else if(ramp || flood)
            {
                Place(props,"PF_Prop_House_Cream",new Vector3(-8.2f,0,13),-90,graybox);
                Place(props,"PF_Prop_CoconutPalm",new Vector3(8.1f,0,6),0,graybox);
                Place(props,"PF_Prop_Streetlight",new Vector3(-6.25f,0,22),0,graybox);
            }
            else
            {
                Place(props,id.Contains("Stores")?"PF_Prop_SariSariStore":"PF_Prop_House_Coral",new Vector3(-8.2f,0,6),-90,graybox);
                Place(props,"PF_Prop_House_Cream",new Vector3(-8.2f,0,19),-90,graybox);
                Place(props,"PF_Prop_CoconutPalm",new Vector3(7.1f,0,9),0,graybox);
                Place(props,"PF_Prop_Streetlight",new Vector3(-6.35f,0,22),0,graybox);
            }
            if(underpass)
            {
                // Above Opong's maximum jump silhouette; scenery only, never a duck hazard.
                Box(surface,"Overpass deck",new Vector3(0,6.65f,12),new Vector3(14,.38f,4),EnvironmentModelFactory.Concrete);
                foreach(int side in new[]{-1,1}) Box(surface,"Off-road bridge support",new Vector3(side*6.4f,3.2f,12),new Vector3(.7f,6.4f,.8f),EnvironmentModelFactory.Concrete);
            }
            if(id.Contains("Earthquake"))
            {
                Box(surface,"Blocked sidewalk only",new Vector3(-5.1f,.38f,11),new Vector3(.80f,.76f,1.6f),EnvironmentModelFactory.Yellow);
                EnvironmentModelFactory.Arrow(surface,new Vector3(0,.035f,15),EnvironmentModelFactory.SafeGreen);
            }
            if(flood)
            {
                Transform water=PrimitiveArt.Root("Water",root,Vector3.zero);
                foreach(int side in new[]{-1,1}) Box(water,"Water outside the protected dry road",new Vector3(side*10.0f,-.30f,12),new Vector3(7,.04f,24),EnvironmentModelFactory.Teal);
            }
            return root;
        }
        private static void Box(Transform t,string n,Vector3 p,Vector3 s,Color c)
        { PrimitiveArt.Shape(PrimitiveType.Cube,t,n,p,s,c); }
        private static void Place(Transform parent,string id,Vector3 position,float yaw,bool graybox)
        {
            Transform prop;
            GameObject saved=graybox?null:Resources.Load<GameObject>("Environment/Props/"+id);
            if(saved!=null) prop=Object.Instantiate(saved,parent,false).transform;
            else if(graybox)
            {
                prop=PrimitiveArt.Root(id,parent,Vector3.zero);
                bool tree=id.Contains("Tree") || id.Contains("Palm"), pole=id.Contains("Streetlight");
                Vector3 size=pole?new Vector3(.3f,5,.3f):tree?new Vector3(2.5f,4.8f,2.5f):new Vector3(3.8f,3.5f,3.8f);
                Box(prop,"Graybox volume",Vector3.up*size.y*.5f,size,tree?EnvironmentModelFactory.Leaf:EnvironmentModelFactory.Cream);
            }
            else prop=EnvironmentModelFactory.Model(id,parent);
            prop.name=id; prop.localPosition=position; prop.localRotation=Quaternion.Euler(0,yaw,0);
        }
    }
}
