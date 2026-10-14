using System.Collections.Generic;
using UnityEngine;

namespace AndamRunner
{
    // Authoring recipes. The editor exports shared meshes/prefabs; phones instantiate
    // those saved assets and never run these modelling recipes during a run.
    public static class EnvironmentModelFactory
    {
        public static readonly Color Asphalt=new Color32(74,89,103,255), Concrete=new Color32(203,207,197,255), Cream=new Color32(237,222,190,255);
        public static readonly Color Teal=new Color32(92,153,153,255), Leaf=new Color32(107,148,105,255), Coral=new Color32(198,137,116,255);
        public static readonly Color Roof=new Color32(148,106,88,255), Ink=new Color32(71,91,98,255), SkyGlass=new Color32(134,177,185,255);
        public static readonly Color SafeGreen=new Color32(52,167,136,255), Yellow=new Color32(246,193,65,255), Wood=new Color32(145,116,81,255);
        private static void Box(Transform t,string name,Vector3 p,Vector3 s,Color c,Vector3 r=default)
        { PrimitiveArt.Shape(PrimitiveType.Cube,t,name,p,s,c,r); }
        private static void Ellipsoid(Transform t,string name,Vector3 p,Vector3 s,Color c)
        { PrimitiveArt.MeshPart(t,name,StorybookArt.Sphere(8,4),p,s,c); }

        public static Transform Model(string id,Transform parent)
        {
            Transform root=PrimitiveArt.Root(id,parent,Vector3.zero);
            switch(id)
            {
                case "PF_Prop_CoconutPalm": Palm(root); break;
                case "PF_Prop_TropicalTree":
                    Box(root,"Trunk",new Vector3(0,1.3f,0),new Vector3(.30f,2.6f,.30f),Wood);
                    Ellipsoid(root,"Quiet leaf canopy",new Vector3(0,3,0),new Vector3(2.8f,2.1f,2.8f),Leaf); break;
                case "PF_Prop_UtilityPole": Pole(root,false); break;
                case "PF_Prop_Streetlight": Pole(root,true); break;
                case "PF_Prop_SariSariStore": House(root,Teal,true,false); break;
                case "PF_Prop_House_Cream": House(root,Cream,false,false); break;
                case "PF_Prop_House_Coral": House(root,Coral,false,false); break;
                case "PF_Prop_ResidentialBlock": House(root,Concrete,false,true); break;
                case "PF_Prop_Carinderia": Eatery(root); break;
                case "PF_Prop_Resort": Resort(root); break;
                case "PF_Background_CommercialTower": Tower(root); break;
                case "PF_Background_SoftCloud":
                    for(int i=0;i<3;i++) Ellipsoid(root,"Soft distant cloud lobe",new Vector3(-2+i*2,.3f+(i%2)*.5f,0),new Vector3(3.5f,1.7f,2.7f),new Color32(237,242,235,255));
                    break;
                case "PF_Prop_ConstructionBarrier": Barricade(root); break;
                case "PF_Prop_Seawall": Box(root,"Concrete seawall",new Vector3(0,.45f,0),new Vector3(.35f,.9f,6),Cream); break;
                case "PF_Prop_CoastalFence": Fence(root); break;
                case "PF_Background_Bangka": Boat(root); break;
                case "PF_Prop_PedestrianFootbridge": Bridge(root); break;
                case "PF_Prop_EvacuationCenter": Center(root); break;
                case "PF_Obstacle_RoadBarricade": Barricade(root); break;
                case "PF_Obstacle_ParkedVehicle": Vehicle(root); break;
                case "PF_Obstacle_FallenBranch": Branch(root); break;
                case "PF_Obstacle_BrokenPavement": Pavement(root); break;
                case "PF_Obstacle_FallenSign": FallenSign(root); break;
                case "PF_Obstacle_ConstructionDebris": Debris(root); break;
                case "PF_Obstacle_RoadsidePuddle": Puddle(root,false); break;
                case "PF_Obstacle_BoundedFlood": Puddle(root,true); break;
                case "PF_Obstacle_BlockedPassage":
                    Barricade(root); Box(root,"Clear closure panel",new Vector3(0,1.35f,0),new Vector3(2.1f,.6f,.12f),Ink); break;
                case "PF_Route_WideStaircase": Steps(root); break;
                case "PF_Route_ElevatedSidewalk":
                    Box(root,"Raised sidewalk",new Vector3(0,.45f,0),new Vector3(3,.9f,6),Concrete);
                    Arrow(root,new Vector3(0,.92f,0),SafeGreen); break;
            }
            return root;
        }

        private static void House(Transform root,Color wall,bool store,bool tall)
        {
            float h=tall?5.2f:2.7f;
            Box(root,"Simple concrete walls",new Vector3(0,h*.5f,0),new Vector3(3.8f,h,3.8f),wall);
            PrimitiveArt.MeshPart(root,"Broad hipped tin roof",PrimitiveArt.Pyramid(),new Vector3(0,h,0),new Vector3(4.3f,.95f,4.3f),Roof);
            Box(root,"Roof eave",new Vector3(0,h+.025f,0),new Vector3(4.3f,.13f,4.3f),Cream);
            Box(root,"Door",new Vector3(-.65f,.98f,-1.915f),new Vector3(.88f,1.96f,.05f),Ink);
            Box(root,"Window recess",new Vector3(.77f,1.6f,-1.922f),new Vector3(1.0f,.91f,.055f),Ink);
            Box(root,"Opaque capiz-inspired window",new Vector3(.77f,1.6f,-1.96f),new Vector3(.82f,.73f,.025f),SkyGlass);
            Box(root,"Window divider",new Vector3(.77f,1.6f,-1.982f),new Vector3(.05f,.75f,.025f),Cream);
            if(tall) for(int i=-1;i<=1;i+=2) Box(root,"Upper window",new Vector3(i*.85f,4.03f,-1.93f),new Vector3(.95f,1.05f,.05f),SkyGlass);
            if(store)
            {
                Box(root,"Store counter",new Vector3(0,.82f,-2.05f),new Vector3(2.7f,.32f,.65f),Wood);
                Box(root,"Broad sari-sari awning",new Vector3(0,2.16f,-2.4f),new Vector3(3.4f,.13f,1.1f),Teal,new Vector3(-7,0,0));
                for(int i=-1;i<=1;i+=2) Box(root,"Awning cream band",new Vector3(i*.93f,2.22f,-2.4f),new Vector3(.40f,.035f,1.11f),Cream,new Vector3(-7,0,0));
                Box(root,"Store display opening",new Vector3(0,1.5f,-1.96f),new Vector3(2.45f,1.1f,.04f),Ink);
            }
        }
        private static void Palm(Transform root)
        {
            PrimitiveArt.MeshPart(root,"Low-poly trunk",StorybookArt.Tube("Palm trunk",new[]{Vector3.zero,new Vector3(.15f,2.2f,0),new Vector3(.36f,4.7f,0)},new[]{.24f,.20f,.15f},1,6),Vector3.zero,Vector3.one,Wood);
            for(int i=0;i<6;i++) PrimitiveArt.MeshPart(root,"Broad coconut frond",CoastalPolish.PalmFrond(),new Vector3(.36f,4.7f,0),new Vector3(.72f,1,.78f),Leaf,new Vector3(-8,i*60,0));
            Ellipsoid(root,"Coconut cluster",new Vector3(.45f,4.4f,0),new Vector3(.48f,.42f,.48f),Wood);
        }
        private static void Pole(Transform root,bool light)
        {
            Box(root,"Concrete pole",new Vector3(0,2.5f,0),new Vector3(.20f,5,.20f),Concrete);
            if(light)
            {
                Box(root,"Lamp arm",new Vector3(.45f,4.82f,0),new Vector3(.95f,.10f,.10f),Ink);
                Box(root,"Soft unlit lantern",new Vector3(.86f,4.68f,0),new Vector3(.52f,.24f,.34f),Cream);
            }
            else
            {
                Box(root,"Utility crossarm",new Vector3(0,4.65f,0),new Vector3(1.2f,.11f,.12f),Ink);
                Box(root,"Simplified overhead wire",new Vector3(.30f,4.72f,0),new Vector3(.045f,.045f,6),Ink);
            }
        }
        private static void Eatery(Transform root)
        {
            Box(root,"Eatery rear wall",new Vector3(0,1.25f,1.1f),new Vector3(4,2.5f,.3f),Cream);
            for(int side=-1;side<=1;side+=2) Box(root,"Open veranda post",new Vector3(side*1.7f,1.25f,-1.1f),new Vector3(.18f,2.5f,.18f),Wood);
            PrimitiveArt.MeshPart(root,"Carinderia tin canopy",PrimitiveArt.Pyramid(),new Vector3(0,2.5f,0),new Vector3(4.7f,.65f,3.6f),Teal);
            Box(root,"Service counter",new Vector3(0,.85f,.65f),new Vector3(3.4f,.35f,.60f),Coral);
            Box(root,"One communal table",new Vector3(0,.8f,-.6f),new Vector3(2.4f,.13f,.80f),Wood);
            for(int side=-1;side<=1;side+=2) Box(root,"Bench",new Vector3(0,.4f,-.6f+side*.7f),new Vector3(2.35f,.16f,.30f),Wood);
        }
        private static void Resort(Transform root)
        {
            Box(root,"Resort wings",new Vector3(0,2.4f,0),new Vector3(6.6f,4.8f,5),Cream);
            PrimitiveArt.MeshPart(root,"Tropical roof",PrimitiveArt.Pyramid(),new Vector3(0,4.8f,0),new Vector3(7.4f,1.1f,5.8f),Coral);
            for(int x=-1;x<=1;x++) for(int row=0;row<2;row++) Box(root,"Broad shaded window",new Vector3(x*1.8f,1.2f+row*2.1f,-2.51f),new Vector3(1.15f,1.2f,.045f),SkyGlass);
        }
        private static void Tower(Transform root)
        {
            Color quiet=new Color32(164,189,188,255);
            Box(root,"Quiet skyline silhouette",new Vector3(0,5,0),new Vector3(4.5f,10,4),quiet);
            Box(root,"Setback crown",new Vector3(0,10.25f,0),new Vector3(3.6f,.5f,3.2f),Concrete);
            for(int i=0;i<3;i++) Box(root,"Broad window band",new Vector3(0,2+i*2.6f,-2.015f),new Vector3(3.9f,.55f,.04f),new Color32(146,175,178,255));
        }
        private static void Fence(Transform root)
        {
            for(int i=-1;i<=1;i++) Box(root,"Fence post",new Vector3(0,.65f,i*2.4f),new Vector3(.12f,1.3f,.12f),Cream);
            for(int i=0;i<2;i++) Box(root,"Coastal rail",new Vector3(0,.50f+i*.55f,0),new Vector3(.10f,.10f,4.8f),Cream);
        }
        private static void Boat(Transform root)
        {
            Ellipsoid(root,"Bangka hull",new Vector3(0,.15f,0),new Vector3(.95f,.42f,3.1f),Cream);
            Box(root,"Hull deck",new Vector3(0,.36f,0),new Vector3(.72f,.12f,2.5f),Teal);
            for(int side=-1;side<=1;side+=2)
            {
                Box(root,"Bamboo outrigger",new Vector3(side*1.2f,.20f,0),new Vector3(.12f,.12f,3.1f),Wood);
                Box(root,"Outrigger brace",new Vector3(0,.35f,side*.77f),new Vector3(2.5f,.065f,.065f),Wood);
            }
        }
        private static void Bridge(Transform root)
        {
            Box(root,"Footbridge deck",new Vector3(0,1.2f,0),new Vector3(3,.24f,6),Concrete);
            for(int side=-1;side<=1;side+=2)
            {
                Box(root,"Bridge rail",new Vector3(side*1.43f,2,0),new Vector3(.12f,.12f,6),Cream);
                for(int z=-1;z<=1;z++) Box(root,"Rail post",new Vector3(side*1.43f,1.63f,z*2.6f),new Vector3(.10f,.85f,.10f),Cream);
            }
        }
        private static void Center(Transform root)
        {
            Box(root,"Raised community center",new Vector3(0,1.85f,0),new Vector3(6,3.2f,4.6f),Cream);
            PrimitiveArt.MeshPart(root,"Welcoming green roof",PrimitiveArt.Pyramid(),new Vector3(0,3.45f,0),new Vector3(6.9f,1.25f,5.5f),Teal);
            Box(root,"Wide safe doorway",new Vector3(0,1.4f,-2.315f),new Vector3(1.9f,2.4f,.05f),Ink);
            for(int side=-1;side<=1;side+=2) Box(root,"Center window",new Vector3(side*2,1.95f,-2.32f),new Vector3(1.05f,1.25f,.05f),SkyGlass);
            Box(root,"Green route sign",new Vector3(0,3.13f,-2.36f),new Vector3(1.8f,.55f,.06f),SafeGreen);
            Box(root,"Sign arrow shaft",new Vector3(0,3.13f,-2.405f),new Vector3(.72f,.09f,.025f),Cream);
            PrimitiveArt.MeshPart(root,"Sign arrow tip",Triangle(),new Vector3(.46f,3.13f,-2.425f),new Vector3(.22f,.22f,1),Cream,new Vector3(0,0,-90));
        }
        private static void Barricade(Transform root)
        {
            for(int side=-1;side<=1;side+=2) Box(root,"Barrier foot",new Vector3(side*.82f,.45f,0),new Vector3(.15f,.9f,.50f),Wood);
            Box(root,"Broad hazard board",new Vector3(0,.78f,0),new Vector3(2.1f,.40f,.16f),Yellow);
            for(int i=-1;i<=1;i++) Box(root,"Readable dark stripe",new Vector3(i*.65f,.78f,-.086f),new Vector3(.20f,.41f,.022f),Ink,new Vector3(0,0,-20));
        }
        private static void Vehicle(Transform root)
        {
            Box(root,"Parked minivan body",new Vector3(0,.72f,0),new Vector3(1.55f,.75f,3),Teal);
            Box(root,"Minivan cabin",new Vector3(0,1.28f,.15f),new Vector3(1.46f,.77f,2.35f),Cream);
            Box(root,"Wide windshield",new Vector3(0,1.32f,-1.04f),new Vector3(1.22f,.48f,.025f),Ink);
            for(int side=-1;side<=1;side+=2) for(int axle=-1;axle<=1;axle+=2)
                PrimitiveArt.MeshPart(root,"Low-poly tire",StorybookArt.Sphere(8,4),new Vector3(side*.77f,.38f,axle*.95f),new Vector3(.20f,.54f,.54f),Ink);
            Box(root,"Yellow hazard bumper",new Vector3(0,.46f,-1.55f),new Vector3(1.55f,.12f,.14f),Yellow);
        }
        private static void Branch(Transform root)
        {
            Box(root,"One fallen limb",new Vector3(0,.35f,0),new Vector3(2,.28f,.28f),Wood,new Vector3(0,12,6));
            Box(root,"Branch fork",new Vector3(.40f,.45f,.25f),new Vector3(1.1f,.16f,.16f),Wood,new Vector3(0,-38,14));
            Ellipsoid(root,"Single leaf cluster",new Vector3(.85f,.62f,.55f),new Vector3(.75f,.52f,.65f),Leaf);
            Box(root,"Hazard base",new Vector3(0,.025f,0),new Vector3(2.2f,.025f,.85f),Yellow);
        }
        private static void Pavement(Transform root)
        {
            Box(root,"Dark bounded pavement opening",new Vector3(0,.025f,0),new Vector3(2.15f,.04f,1.5f),Ink);
            for(int side=-1;side<=1;side+=2) Box(root,"One broken slab",new Vector3(side*.63f,.18f,0),new Vector3(.8f,.24f,1.4f),Concrete,new Vector3(0,side*12,side*10));
            Box(root,"Readable hazard rim",new Vector3(0,.05f,-.78f),new Vector3(2.2f,.06f,.09f),Yellow);
        }
        private static void FallenSign(Transform root)
        {
            Box(root,"Fallen pole",new Vector3(0,.17f,0),new Vector3(2.0f,.13f,.13f),Concrete,new Vector3(0,18,0));
            Box(root,"Simple fallen sign",new Vector3(.55f,.34f,.1f),new Vector3(.95f,.08f,.72f),Yellow,new Vector3(12,0,0));
        }
        private static void Debris(Transform root)
        {
            for(int i=0;i<3;i++) Box(root,"Limited rubble",new Vector3(-.65f+i*.60f,.17f+i*.08f,(i%2)*.30f),new Vector3(.63f,.34f,.55f),Concrete,new Vector3(0,i*22,12));
            Box(root,"Debris hazard board",new Vector3(0,.065f,-.50f),new Vector3(2.2f,.065f,.10f),Yellow);
        }
        private static void Puddle(Transform root,bool flood)
        {
            Box(root,"Clear water boundary",new Vector3(0,.035f,0),new Vector3(flood?2.2f:1.65f,.06f,flood?4f:1.2f),Ink);
            Box(root,"Bright bounded water",new Vector3(0,.07f,0),new Vector3(flood?2.05f:1.50f,.035f,flood?3.8f:1.05f),new Color32(60,179,221,255));
            Box(root,"Hazard leading edge",new Vector3(0,.09f,flood?-2f:-.6f),new Vector3(flood?2.25f:1.7f,.055f,.09f),Yellow);
        }
        private static void Steps(Transform root)
        {
            for(int i=0;i<6;i++) Box(root,"Wide low step",new Vector3(0,(i+1)*.10f,-2.5f+i),new Vector3(4,(i+1)*.20f,1),Concrete);
        }
        public static void Arrow(Transform root,Vector3 p,Color c)
        {
            Box(root,"Safe route arrow shaft",p+new Vector3(0,0,-.30f),new Vector3(.22f,.025f,.85f),c);
            PrimitiveArt.MeshPart(root,"Safe route arrow head",Triangle(),p+new Vector3(0,.014f,.30f),new Vector3(.63f,.70f,1),c,new Vector3(90,0,0));
        }
        private static Mesh arrowTriangle;
        private static Mesh Triangle()
        {
            if(arrowTriangle==null) { arrowTriangle=new Mesh{name="Simple pictogram triangle",vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(0,.5f,0)},triangles=new[]{0,2,1}}; arrowTriangle.RecalculateNormals(); }
            return arrowTriangle;
        }
    }
}
