using UnityEngine;

namespace AndamRunner
{
    public sealed class RepeatingRoad : MonoBehaviour
    {
        private const int ChunkCount = 7;
        private const float ChunkLength = 24f;
        private readonly Transform[] chunks = new Transform[ChunkCount];

        public void Initialize()
        {
            for(int i=0;i<ChunkCount;i++)
            {
                chunks[i] = PrimitiveArt.Root("Coastal neighborhood " + i,transform,Vector3.zero);
                MakeChunk(chunks[i],i);
                StorybookArt.CombineBone(chunks[i],true);
                foreach(Renderer renderer in chunks[i].GetComponentsInChildren<Renderer>()) renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            ResetRoad();
        }
        public void ResetRoad()
        {
            for(int i=0;i<ChunkCount;i++) chunks[i].localPosition = new Vector3(0,0,(i-1)*ChunkLength);
        }
        public void Tick(float distance)
        {
            foreach(Transform chunk in chunks)
            {
                chunk.localPosition += Vector3.back * distance;
                while(chunk.localPosition.z < -ChunkLength*1.5f) chunk.localPosition += Vector3.forward * ChunkLength * ChunkCount;
            }
        }

        private void MakeChunk(Transform root,int index)
        {
            if(Resources.Load<Material>("IllustratedGround")!=null) { MakeIllustratedLanes(root); return; }
            Color road = new Color32(176,177,159,255);
            PrimitiveArt.Box(root,"Dry three-lane road",new Vector3(0,-.17f,0),new Vector3(8.4f,.3f,24),road);
            PrimitiveArt.Box(root,"Land",new Vector3(-12,-.38f,0),new Vector3(19,.4f,24),new Color32(175,197,139,255));
            PrimitiveArt.Box(root,"Seafront promenade",new Vector3(6,-.05f,0),new Vector3(3.6f,.4f,24),PreparednessContent.Cream);
            PrimitiveArt.Box(root,"House-side sidewalk",new Vector3(-5.1f,-.05f,0),new Vector3(1.8f,.4f,24),PreparednessContent.Cream);
            PrimitiveArt.Box(root,"Sea (scenery only)",new Vector3(28,-.48f,0),new Vector3(40,.25f,24),PrimitiveArt.Sea);
            PrimitiveArt.Box(root,"Seawall",new Vector3(8.0f,.1f,0),new Vector3(.38f,.75f,24),new Color32(235,218,185,255));
            PrimitiveArt.Box(root,"Road edge L",new Vector3(-4.04f,.006f,0),new Vector3(.09f,.015f,24),Color.white);
            PrimitiveArt.Box(root,"Road edge R",new Vector3(4.04f,.006f,0),new Vector3(.09f,.015f,24),Color.white);
            for(int lane=-1;lane<=1;lane+=2)
                for(int i=0;i<6;i++) PrimitiveArt.Box(root,"Lane dash",new Vector3(lane*1.3f,.008f,-10+i*4),new Vector3(.07f,.012f,1.5f),Color.white);
            for(int i=0;i<4;i++)
            {
                PrimitiveArt.Box(root,"Rail post",new Vector3(7.94f,.83f,-9+i*6),new Vector3(.12f,1f,.12f),Color.white);
                PrimitiveArt.Box(root,"Sea ripple",new Vector3(15+i*3,-.34f,-9+i*6),new Vector3(2.8f,.015f,.08f),new Color32(169,226,230,255));
            }
            PrimitiveArt.Box(root,"White seafront rail",new Vector3(7.94f,1.21f,0),new Vector3(.10f,.09f,24),Color.white);
            House(root,new Vector3(-9,0,-6),index);
            House(root,new Vector3(-9.6f,0,7),index+1);
            Palm(root,new Vector3(6.3f,0,-6),index%2==0?1f:.86f);
            Palm(root,new Vector3(-6.4f,0,4),.82f);
            if(index%2==0) Boat(root,new Vector3(14,-.1f,3));
            CoastalPolish.AddStreetDetails(root,index);
            if(index==0 || index==4)
            {
                PrimitiveArt.Box(root,"Community sign post",new Vector3(-5.5f,1.3f,-8),new Vector3(.1f,2.6f,.1f),PreparednessContent.Ink);
                PrimitiveArt.Box(root,"Community sign",new Vector3(-5.5f,2.5f,-8),new Vector3(2.2f,.9f,.09f),PreparednessContent.Teal);
                PrimitiveArt.Label(root,index==0?"ANDAM\nLAPU-LAPU":"BARANGAY\nANDAM",new Vector3(-5.5f,2.52f,-8.065f),.13f,Color.white);
            }
            if(index==3)
            {
                // A small fictional barangay readiness point, not a real evacuation direction.
                PrimitiveArt.Box(root,"Preparedness pavilion",new Vector3(-9,1.2f,0),new Vector3(3f,2.4f,3),PreparednessContent.Cream);
                PrimitiveArt.MeshPart(root,"Pavilion roof",PrimitiveArt.Pyramid(),new Vector3(-9,2.4f,0),new Vector3(3.6f,1.1f,3.6f),PreparednessContent.Teal);
                PrimitiveArt.Label(root,"RESILIENCE\nSTARTS AT HOME",new Vector3(-9,1.6f,-1.56f),.11f,PreparednessContent.Ink);
            }
        }

        private static Material paintedMarkers;
        private static void MakeIllustratedLanes(Transform root)
        {
            Material ground=Resources.Load<Material>("IllustratedGround");
            if(paintedMarkers==null) { paintedMarkers=new Material(ground); paintedMarkers.color=new Color(1,.97f,.85f,.96f); }
            Transform road=PrimitiveArt.Box(root,"Illustrated three-lane playfield",new Vector3(0,-.055f,0),new Vector3(8.4f,.1f,24),Color.white);
            road.GetComponent<Renderer>().sharedMaterial=ground;
            for(int lane=-1;lane<=1;lane+=2)
                for(int i=0;i<6;i++)
                {
                    Transform dash=PrimitiveArt.Box(root,"Readable lane marker",new Vector3(lane*1.3f,.008f,-10+i*4),new Vector3(.075f,.014f,1.5f),Color.white);
                    dash.GetComponent<Renderer>().sharedMaterial=paintedMarkers;
                }
            foreach(int side in new[]{-1,1})
            {
                Transform edge=PrimitiveArt.Box(root,"Painted road edge",new Vector3(side*4.12f,.009f,0),new Vector3(.085f,.012f,24),Color.white);
                edge.GetComponent<Renderer>().sharedMaterial=paintedMarkers;
            }
        }

        private static void House(Transform parent,Vector3 position,int colorIndex)
        {
            Color[] colors = { new Color32(243,208,140,255), new Color32(229,159,136,255), new Color32(154,202,181,255), new Color32(188,180,210,255) };
            Transform house = PrimitiveArt.Root("Colorful coastal home",parent,position);
            PrimitiveArt.Box(house,"Home walls",new Vector3(0,1.5f,0),new Vector3(4.8f,3,4.5f),colors[colorIndex%colors.Length]);
            PrimitiveArt.MeshPart(house,"Tin hipped roof",PrimitiveArt.Pyramid(),new Vector3(0,3,0),new Vector3(5.3f,1.5f,5f),PrimitiveArt.Rust);
            PrimitiveArt.Box(house,"Painted roof eaves",new Vector3(0,3.02f,0),new Vector3(5.15f,.18f,4.82f),new Color32(241,216,175,255));
            PrimitiveArt.Box(house,"Door frame",new Vector3(2.41f,1.05f,0),new Vector3(.15f,2.18f,1.18f),PreparednessContent.Cream);
            PrimitiveArt.Box(house,"Door",new Vector3(2.43f,1,0),new Vector3(.06f,2,1f),PreparednessContent.Ink);
            PrimitiveArt.Box(house,"Window",new Vector3(2.44f,1.9f,1.4f),new Vector3(.06f,.9f,.8f),PreparednessContent.Cream);
            PrimitiveArt.Box(house,"Front window",new Vector3(.9f,1.8f,-2.28f),new Vector3(1.2f,1f,.05f),PreparednessContent.Ink);
            PrimitiveArt.Box(house,"Window frame",new Vector3(.9f,1.8f,-2.32f),new Vector3(.06f,1f,.05f),Color.white);
            PrimitiveArt.Box(house,"Roadside window glass",new Vector3(2.49f,1.9f,1.4f),new Vector3(.045f,.74f,.64f),new Color32(119,173,178,255));
            PrimitiveArt.Box(house,"Window crossbar",new Vector3(2.52f,1.9f,1.4f),new Vector3(.04f,.76f,.045f),PreparednessContent.Cream);
            for(int side=-1;side<=1;side+=2)
                PrimitiveArt.Box(house,"Painted shutter",new Vector3(2.47f,1.9f,1.4f+side*.53f),new Vector3(.09f,.89f,.22f),PreparednessContent.Teal,new Vector3(0,side*12,0));
            PrimitiveArt.Box(house,"Flower window box",new Vector3(2.62f,1.40f,1.4f),new Vector3(.36f,.23f,1.03f),PrimitiveArt.Rust);
            for(int flower=0;flower<5;flower++)
            {
                float z=1.05f+flower*.175f;
                PrimitiveArt.Ball(house,"Window box foliage",new Vector3(2.64f,1.56f,z),new Vector3(.35f,.24f,.30f),new Color32(109,164,105,255));
                PrimitiveArt.Ball(house,"Small painted bloom",new Vector3(2.81f,1.61f,z),new Vector3(.13f,.14f,.14f),flower%2==0?PreparednessContent.Orange:PreparednessContent.Cream);
            }
            PrimitiveArt.Box(house,"Welcome step",new Vector3(2.67f,.18f,0),new Vector3(.56f,.17f,1.3f),new Color32(224,205,169,255));
            if(colorIndex%3==0)
            {
                PrimitiveArt.Box(house,"Sari-sari awning",new Vector3(2.9f,2.3f,0),new Vector3(1.25f,.15f,3f),PreparednessContent.Mint,new Vector3(0,0,-8));
                for(int stripe=0;stripe<6;stripe++) PrimitiveArt.Box(house,"Awning stripe",new Vector3(2.91f,2.39f,-1.25f+stripe*.5f),new Vector3(1.26f,.018f,.22f),Color.white,new Vector3(0,0,-8));
                for(int scallop=0;scallop<6;scallop++) PrimitiveArt.Ball(house,"Scalloped awning hem",new Vector3(3.5f,2.19f,-1.25f+scallop*.5f),new Vector3(.10f,.29f,.48f),scallop%2==0?PreparednessContent.Mint:PreparednessContent.Cream);
                PrimitiveArt.Label(house,"SARI-SARI",new Vector3(0,2.3f,-2.35f),.13f,PreparednessContent.Ink);
            }
        }

        private static void Palm(Transform parent,Vector3 position,float scale)
        {
            Transform palm = PrimitiveArt.Root("Coconut palm",parent,position); palm.localScale = Vector3.one*scale;
            PrimitiveArt.Shape(PrimitiveType.Cylinder,palm,"Palm trunk",new Vector3(0,2.1f,0),new Vector3(.28f,2.1f,.28f),new Color32(159,112,69,255),new Vector3(0,0,-6));
            for(int i=0;i<6;i++)
            {
                float angle=i*60; Transform leaf = PrimitiveArt.Root("Palm frond",palm,new Vector3(.2f,4.3f,0));
                leaf.localRotation=Quaternion.Euler(0,angle,0);
                PrimitiveArt.MeshPart(leaf,"Curved leaf",CoastalPolish.PalmFrond(),Vector3.zero,Vector3.one,new Color32(102,158,103,255),new Vector3(-9,0,0));
            }
            PrimitiveArt.Ball(palm,"Coconut",new Vector3(.20f,4.1f,.2f),new Vector3(.25f,.3f,.25f),PrimitiveArt.Rust);
        }

        private static void Boat(Transform parent,Vector3 position)
        {
            Transform boat=PrimitiveArt.Root("Bangka with outriggers",parent,position);
            PrimitiveArt.Shape(PrimitiveType.Capsule,boat,"Boat hull",Vector3.zero,new Vector3(.8f,2.1f,.5f),Color.white,new Vector3(90,0,0));
            PrimitiveArt.Box(boat,"Bangka top",new Vector3(0,.16f,0),new Vector3(.64f,.1f,2.7f),PreparednessContent.Teal);
            for(int i=-1;i<=1;i+=2)
            {
                PrimitiveArt.Box(boat,"Outrigger crossbar",new Vector3(0,.25f,i*.7f),new Vector3(3.2f,.07f,.07f),PrimitiveArt.Rust);
                PrimitiveArt.Shape(PrimitiveType.Capsule,boat,"Bamboo float",new Vector3(i*1.5f,.02f,0),new Vector3(.16f,1.65f,.16f),GoldColor(),new Vector3(90,0,0));
            }
        }
        private static Color GoldColor() { return new Color32(218,191,112,255); }
    }
}
