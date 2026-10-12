using UnityEngine;

namespace AndamRunner
{
    public static class CoastalPolish
    {
        private static Mesh frond, ring, pennant;
        public static Mesh PalmFrond()
        {
            if(frond!=null) return frond;
            var vertices=new Vector3[18]; var triangles=new int[60];
            for(int i=0;i<6;i++)
            {
                float t=i/5f, width=Mathf.Sin(t*Mathf.PI)*.48f+.025f;
                float y=Mathf.Sin(t*Mathf.PI)*.24f-t*t*.56f;
                vertices[i*3]=new Vector3(-width,y,t*2.8f); vertices[i*3+1]=new Vector3(0,y+.08f,t*2.8f); vertices[i*3+2]=new Vector3(width,y,t*2.8f);
            }
            for(int i=0;i<5;i++)
            {
                int v=i*3,k=i*12; int[] face={v,v+3,v+1,v+1,v+3,v+4,v+1,v+4,v+2,v+2,v+4,v+5};
                for(int j=0;j<12;j++) triangles[k+j]=face[j];
            }
            frond=new Mesh {name="Curved coconut frond",vertices=vertices,triangles=triangles}; frond.RecalculateNormals(); return frond;
        }
        public static Mesh Hoop()
        {
            if(ring!=null) return ring;
            const int segments=20, sides=6; var vertices=new Vector3[segments*sides]; var triangles=new int[segments*sides*6];
            for(int i=0;i<segments;i++) for(int j=0;j<sides;j++)
            {
                float a=i*Mathf.PI*2/segments,b=j*Mathf.PI*2/sides,radius=.5f+Mathf.Cos(b)*.12f;
                vertices[i*sides+j]=new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,Mathf.Sin(b)*.12f);
                int a0=i*sides+j,a1=((i+1)%segments)*sides+j,a2=((i+1)%segments)*sides+(j+1)%sides,a3=i*sides+(j+1)%sides,k=(i*sides+j)*6;
                triangles[k]=a0; triangles[k+1]=a1; triangles[k+2]=a2; triangles[k+3]=a0; triangles[k+4]=a2; triangles[k+5]=a3;
            }
            ring=new Mesh {name="Gold hoop",vertices=vertices,triangles=triangles}; ring.RecalculateNormals(); return ring;
        }
        private static Mesh Pennant()
        {
            if(pennant!=null) return pennant;
            pennant=new Mesh {name="Fiesta pennant",vertices=new[] {new Vector3(-.20f,0,0),new Vector3(.20f,0,0),new Vector3(0,-.40f,0)},triangles=new[]{0,2,1,0,1,2}};
            pennant.RecalculateNormals(); return pennant;
        }
        public static void AddStreetDetails(Transform root,int index)
        {
            // Decoration stays beside the playable road and never hides a hazard.
            for(int i=0;i<2;i++)
            {
                float z=-10+i*12;
                PrimitiveArt.Box(root,"Promenade paving joint",new Vector3(6,.155f,z),new Vector3(3.5f,.012f,.04f),new Color32(220,210,188,255)); Lamp(root,new Vector3(4.9f,0,z));
            }
            Bench(root,new Vector3(6.2f,0,4));
            Transform bush=PrimitiveArt.Root("Bougainvillea planter",root,new Vector3(-5.2f,0,-1));
            PrimitiveArt.Box(bush,"Planter",new Vector3(0,.30f,0),new Vector3(.72f,.60f,.72f),PrimitiveArt.Rust);
            PrimitiveArt.Ball(bush,"Leaves",new Vector3(0,.85f,0),new Vector3(1.1f,.75f,.9f),new Color32(50,135,94,255));
            for(int i=0;i<4;i++) PrimitiveArt.Ball(bush,"Pink blossom",new Vector3(Mathf.Cos(i*1.6f)*.42f,1.02f,Mathf.Sin(i*1.6f)*.32f),Vector3.one*.21f,new Color32(233,116,157,255));
            if(index%2==0) Bunting(root);
            if(index==2 || index==5) Jeepney(root,new Vector3(-7,0,-3),index==2 ? PreparednessContent.Teal : PreparednessContent.Orange);
        }
        private static void Lamp(Transform parent,Vector3 position)
        {
            Transform lamp=PrimitiveArt.Root("Coastal street lamp",parent,position);
            PrimitiveArt.Shape(PrimitiveType.Cylinder,lamp,"Pole",new Vector3(0,1.55f,0),new Vector3(.12f,1.55f,.12f),PreparednessContent.Ink);
            PrimitiveArt.Box(lamp,"Lantern",new Vector3(0,3.2f,0),new Vector3(.34f,.43f,.34f),new Color32(255,229,154,255));
            PrimitiveArt.MeshPart(lamp,"Lantern cap",PrimitiveArt.Pyramid(),new Vector3(0,3.42f,0),new Vector3(.48f,.23f,.48f),PreparednessContent.Ink);
        }
        private static void Bench(Transform parent,Vector3 position)
        {
            Transform bench=PrimitiveArt.Root("Seafront bench",parent,position);
            for(int i=0;i<3;i++) PrimitiveArt.Box(bench,"Wooden seat slat",new Vector3(0,.6f,-.22f+i*.22f),new Vector3(1.7f,.09f,.16f),PrimitiveArt.Rust);
            for(int i=-1;i<=1;i+=2) PrimitiveArt.Box(bench,"Bench foot",new Vector3(i*.65f,.3f,0),new Vector3(.09f,.6f,.48f),PreparednessContent.Ink);
            PrimitiveArt.Box(bench,"Bench back",new Vector3(0,.95f,.27f),new Vector3(1.7f,.4f,.08f),PrimitiveArt.Rust);
        }
        private static void Bunting(Transform parent)
        {
            Transform flags=PrimitiveArt.Root("Barangay bunting",parent,new Vector3(-4.8f,4.2f,0));
            PrimitiveArt.Box(flags,"Bunting string",Vector3.zero,new Vector3(.02f,.02f,20),PreparednessContent.Ink);
            Color[] colors={PreparednessContent.Mint,PreparednessContent.Orange,PreparednessContent.Teal,new Color32(235,136,130,255)};
            for(int i=0;i<12;i++) PrimitiveArt.MeshPart(flags,"Fiesta flag",Pennant(),new Vector3(0,0,-9+i*1.6f),Vector3.one,colors[i%4],new Vector3(0,90,0));
            for(int i=-1;i<=1;i+=2) PrimitiveArt.Box(flags,"Bunting pole",new Vector3(0,-2.1f,i*10),new Vector3(.09f,4.2f,.09f),PreparednessContent.Ink);
        }
        private static void Jeepney(Transform parent,Vector3 position,Color color)
        {
            Transform jeep=PrimitiveArt.Root("Parked colorful jeepney",parent,position);
            PrimitiveArt.Box(jeep,"Jeepney body",new Vector3(0,.82f,0),new Vector3(1.35f,.82f,3.7f),color);
            PrimitiveArt.Box(jeep,"Cabin",new Vector3(0,1.53f,-.12f),new Vector3(1.24f,.77f,2.72f),PreparednessContent.Cream);
            PrimitiveArt.Box(jeep,"Chrome roof",new Vector3(0,1.97f,-.12f),new Vector3(1.44f,.12f,3),new Color32(188,206,207,255));
            PrimitiveArt.Box(jeep,"Windshield",new Vector3(0,1.54f,1.28f),new Vector3(1.10f,.54f,.03f),PreparednessContent.Ink);
            for(int i=-1;i<=1;i+=2)
            {
                for(int window=0;window<3;window++) PrimitiveArt.Box(jeep,"Passenger window",new Vector3(i*.635f,1.55f,-.9f+window*.74f),new Vector3(.03f,.50f,.54f),PreparednessContent.Ink);
                for(int axle=-1;axle<=1;axle+=2) PrimitiveArt.Shape(PrimitiveType.Cylinder,jeep,"Tire",new Vector3(i*.70f,.42f,axle*1.12f),new Vector3(.56f,.13f,.56f),PrimitiveArt.Hair,new Vector3(0,0,90));
                PrimitiveArt.Ball(jeep,"Headlight",new Vector3(i*.48f,.85f,1.88f),new Vector3(.21f,.21f,.06f),PreparednessContent.Cream);
            }
            PrimitiveArt.Box(jeep,"Front bumper",new Vector3(0,.52f,1.91f),new Vector3(1.53f,.14f,.15f),new Color32(188,206,207,255));
        }
    }
}
