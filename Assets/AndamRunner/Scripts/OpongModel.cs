using UnityEngine;
using UnityEngine.Rendering;

namespace AndamRunner
{
    // An original articulated 3D interpretation of the supplied Opong mascot.
    // Separate body, head and shoulder pivots preserve the balancing animation.
    public static class OpongModel
    {
        private static readonly Color Skin=new Color32(242,197,154,255);
        private static readonly Color Cheek=new Color32(237,163,127,255);
        private static readonly Color Rust=new Color32(174,77,45,255);
        private static readonly Color Stitch=new Color32(205,125,82,255);
        private static readonly Color Hair=new Color32(29,29,31,255);
        private static readonly Color Brown=new Color32(106,47,34,255);
        private static readonly Color Gold=new Color32(214,169,76,255);

        public static Transform Create(Transform visual)
        {
            Transform body=PrimitiveArt.Root("Opong storybook character",visual,new Vector3(0,.29f,0));
            body.localRotation=Quaternion.identity;
            for(int side=-1;side<=1;side+=2)
            {
                Transform pivot=PrimitiveArt.Root(side<0?"Leg L":"Leg R",body,new Vector3(side*.23f,.76f,0));
                Transform legArt=PrimitiveArt.Root("Leg geometry",pivot,-pivot.localPosition);
                float z=side<0 ? .07f : -.07f;
                Vector3[] leg=StorybookArt.Curve(new Vector3(side*.23f,.76f,z-.05f),new Vector3(side*.33f,.43f,z+.12f),new Vector3(side*.29f,.15f,z),10);
                var widths=new float[leg.Length]; for(int i=0;i<widths.Length;i++) widths[i]=Mathf.Lerp(.155f,.118f,i/(float)(widths.Length-1));
                PrimitiveArt.MeshPart(legArt,"Bent leg",StorybookArt.Tube("Bent leg",leg,widths,.95f,10),Vector3.zero,Vector3.one,Skin);
                Ball(legArt,"Bare foot",new Vector3(side*.30f,.10f,z+.055f),new Vector3(.38f,.21f,.47f),Skin);
                for(int toe=0;toe<5;toe++)
                {
                    float x=side*.30f+(toe-2)*.047f;
                    Ball(legArt,"Sculpted toe",new Vector3(x,.081f,z+.23f),new Vector3(.06f,.077f,.102f),Skin);
                }
                StorybookArt.Band(legArt,"Patterned ankle cuff",new Vector3(side*.29f,.23f,z),.134f,.136f,.095f,Hair);
                for(int mark=0;mark<3;mark++) Triangle(legArt,new Vector3(side*.29f+(mark-1)*.071f,.23f,z+.136f),.045f,PreparednessContent.Cream);
                StorybookArt.CombineBone(legArt,true);
            }
            PrimitiveArt.MeshPart(body,"Shaped torso",StorybookArt.Ellipse("Shaped torso",new[]{.83f,.91f,1.07f,1.26f,1.38f,1.48f},new[]{.22f,.29f,.30f,.34f,.30f,.13f},new[]{.16f,.20f,.215f,.22f,.19f,.10f},24),Vector3.zero,Vector3.one,Skin);
            Ball(body,"Neck",new Vector3(0,1.48f,0),new Vector3(.26f,.26f,.24f),Skin);
            PrimitiveArt.MeshPart(body,"Flared woven wrap",StorybookArt.Ellipse("Flared woven wrap",new[]{.56f,.59f,.76f,.89f,.91f},new[]{.45f,.46f,.43f,.36f,.33f},new[]{.27f,.28f,.27f,.23f,.215f},24),Vector3.zero,Vector3.one,Rust);
            StorybookArt.Band(body,"Soft belt",new Vector3(0,.87f,0),.352f,.233f,.095f,Brown);
            PrimitiveArt.Box(body,"Rounded patterned apron",new Vector3(0,.61f,.272f),new Vector3(.31f,.52f,.09f),Rust);
            for(int row=0;row<3;row++)
            {
                float y=.77f-row*.15f;
                Diamond(body,new Vector3(0,y,.327f),.056f,Brown);
                if(row<2) StorybookArt.Stroke(body,"Apron woven line",new Vector3(-.10f,y-.067f,.324f),new Vector3(0,y-.078f,.331f),new Vector3(.10f,y-.067f,.324f),.011f,Stitch);
            }
            Ball(body,"Waist knot",new Vector3(0,.86f,.27f),new Vector3(.15f,.11f,.08f),Rust);
            StorybookArt.Stroke(body,"Belt tie L",new Vector3(-.04f,.86f,.26f),new Vector3(-.32f,.94f,.24f),new Vector3(-.45f,.76f,.25f),.055f,Rust,.4f);
            for(int side=-1;side<=1;side+=2)
            {
                Transform panel=PrimitiveArt.Root("Side woven panel "+side,body,new Vector3(side*.30f,.75f,.217f));
                panel.localScale=new Vector3(.23f,.22f,.08f); panel.localRotation=Quaternion.Euler(0,side*30,side*8);
                PrimitiveArt.MeshPart(panel,"Woven panel",StorybookArt.RoundedBox(3),Vector3.zero,Vector3.one,Rust);
                Diamond(panel,new Vector3(0,0,.51f),.15f,Brown);
                StorybookArt.CombineBone(panel,true);
                StorybookArt.Stroke(body,"Curved leather sash",new Vector3(side*.23f,.88f,.22f),new Vector3(side*.20f,1.35f,.30f),new Vector3(side*.27f,1.43f,-.02f),.074f,Rust,.62f);
                StorybookArt.Stroke(body,"Sash behind shoulder",new Vector3(side*.27f,1.43f,-.02f),new Vector3(side*.22f,1.27f,-.23f),new Vector3(side*.23f,.89f,-.20f),.074f,Rust,.62f);
                StorybookArt.Stroke(body,"Chest tattoo",new Vector3(side*.065f,1.33f,.22f),new Vector3(side*.040f,1.12f,.233f),new Vector3(side*.040f,.95f,.208f),.010f,Hair);
                Arm(body,side);
            }
            StorybookArt.Stroke(body,"Necklace arc",new Vector3(-.20f,1.40f,.17f),new Vector3(0,1.20f,.30f),new Vector3(.20f,1.40f,.17f),.014f,Hair);
            for(int i=0;i<7;i++) { float x=(i-3)*.045f; Triangle(body,new Vector3(x,1.33f+Mathf.Abs(x)*.46f,.235f),.042f,Hair); }
            Head(body);
            StorybookArt.CombineBone(body);
            foreach(Renderer renderer in visual.GetComponentsInChildren<Renderer>()) renderer.shadowCastingMode=ShadowCastingMode.On;
            return body;
        }

        private static void Arm(Transform body,int side)
        {
            Transform arm=PrimitiveArt.Root(side<0?"Arm L":"Arm R",body,new Vector3(side*.31f,1.34f,0));
            arm.localRotation=Quaternion.Euler(0,0,side*32);
            Vector3[] p=StorybookArt.Curve(Vector3.zero,new Vector3(0,-.30f,0),new Vector3(0,-.49f,.08f),10);
            var widths=new float[p.Length]; for(int i=0;i<widths.Length;i++) widths[i]=Mathf.Lerp(.123f,.084f,i/(float)(widths.Length-1));
            PrimitiveArt.MeshPart(arm,"Shaped arm",StorybookArt.Tube("Shaped arm",p,widths,1,10),Vector3.zero,Vector3.one,Skin);
            StorybookArt.Band(arm,"Wrist band",new Vector3(0,-.36f,.046f),.096f,.098f,.072f,Hair);
            for(int i=0;i<3;i++) Triangle(arm,new Vector3((i-1)*.045f,-.13f,.119f),.045f,Hair);
            Ball(arm,"Clenched fist",new Vector3(0,-.52f,.09f),new Vector3(.25f,.25f,.23f),Skin);
            for(int i=0;i<4;i++)
            {
                Transform finger=Ball(arm,"Rounded finger knuckle",new Vector3((i-1.5f)*.047f,-.545f,.185f),new Vector3(.060f,.13f-(i%3)*.009f,.080f),Skin);
                finger.localRotation=Quaternion.Euler(-24,0,(i-1.5f)*4);
            }
            Ball(arm,"Thumb",new Vector3(side*.096f,-.48f,.16f),new Vector3(.10f,.135f,.115f),Skin);
            StorybookArt.CombineBone(arm,true);
        }

        private static void Head(Transform body)
        {
            Transform head=PrimitiveArt.Root("Spiky hair + headband + earrings",body,new Vector3(0,1.88f,0));
            head.localRotation=Quaternion.identity;
            Transform shell=PrimitiveArt.Root("Head shell",head,Vector3.zero);
            Ball(shell,"Hair cap",new Vector3(0,.04f,0),new Vector3(1.19f,1.08f,1.01f),Hair);
            Ball(shell,"Sculpted rounded face",new Vector3(0,-.065f,.11f),new Vector3(1.16f,1.10f,.94f),Skin);
            for(int i=0;i<7;i++)
            {
                float x=(i-3)*.126f;
                PrimitiveArt.MeshPart(shell,"Swept crown lock",StorybookArt.HairTuft(),new Vector3(x,.32f,-.075f+Mathf.Abs(x)*.06f),new Vector3(1.14f,.41f+(.4f-Mathf.Abs(x))*.12f,1.02f),Hair,new Vector3(-12,i%2==0?18:-8,(3-i)*26-13));
            }
            for(int i=0;i<4;i++)
            { float angle=i*90; PrimitiveArt.MeshPart(shell,"Layered back hair lock",StorybookArt.HairTuft(),new Vector3(Mathf.Cos(angle*Mathf.Deg2Rad)*.37f,.18f,-.22f),new Vector3(1.1f,.38f,1.04f),Hair,new Vector3(-32,angle,48)); }
            for(int side=-1;side<=1;side+=2)
            {
                PrimitiveArt.MeshPart(shell,"Side lock",StorybookArt.HairTuft(),new Vector3(side*.43f,.03f,.11f),new Vector3(.64f,.34f,.7f),Hair,new Vector3(0,side*30,-side*112));
                Ball(shell,"Ear",new Vector3(side*.555f,-.14f,.12f),new Vector3(.21f,.29f,.21f),Skin);
                Ball(shell,"Inner ear",new Vector3(side*.596f,-.135f,.212f),new Vector3(.085f,.17f,.045f),Cheek);
                PrimitiveArt.MeshPart(shell,"Gold hoop earring",CoastalPolish.Hoop(),new Vector3(side*.592f,-.31f,.22f),new Vector3(.18f,.225f,.21f),Gold);
            }
            PrimitiveArt.MeshPart(shell,"Broad curved headband",HeadbandMesh(.24f),Vector3.zero,Vector3.one,Rust);
            PrimitiveArt.MeshPart(shell,"Headband lower seam",HeadbandMesh(.024f,-.108f),Vector3.zero,new Vector3(1.004f,1,1.004f),Brown);
            for(int i=0;i<7;i++)
            {
                float a=(i-3)*.24f;
                Transform mark=PrimitiveArt.Root("Embossed headband diamond",shell,new Vector3(Mathf.Sin(a)*.644f,.268f+Mathf.Cos(a)*.04f,Mathf.Cos(a)*.584f));
                mark.localRotation=Quaternion.Euler(0,a*Mathf.Rad2Deg,0); Diamond(mark,Vector3.zero,.060f,Brown);
            }
            Ball(shell,"Headband knot",new Vector3(.54f,.245f,-.20f),new Vector3(.18f,.17f,.18f),Rust);
            StorybookArt.Stroke(shell,"Ribbon tail upper",new Vector3(.56f,.25f,-.23f),new Vector3(.75f,.29f,-.34f),new Vector3(.81f,.14f,-.31f),.082f,Rust,.35f);
            StorybookArt.Stroke(shell,"Ribbon tail lower",new Vector3(.56f,.23f,-.23f),new Vector3(.71f,.06f,-.33f),new Vector3(.73f,-.10f,-.25f),.081f,Rust,.35f);
            for(int i=0;i<3;i++)
            {
                Vector3[] points=StorybookArt.Curve(new Vector3(.15f+i*.11f,.24f,.53f),new Vector3(.12f+i*.11f,.13f,.66f),new Vector3(-.025f+i*.13f,.006f+i*.032f,.572f),8);
                var widths=new float[points.Length];
                for(int j=0;j<widths.Length;j++) { float t=j/(float)(widths.Length-1); widths[j]=.087f*Mathf.Pow(Mathf.Max(0,Mathf.Sin((.3f+t*.7f)*Mathf.PI)),.75f)+.001f; }
                PrimitiveArt.MeshPart(shell,"Tapered forehead fringe",StorybookArt.Tube("Swept fringe",points,widths,.72f,8),Vector3.zero,Vector3.one,Hair);
            }
            Ball(shell,"Rounded nose",new Vector3(0,-.22f,.587f),new Vector3(.115f,.12f,.105f),Skin);
            CreateFace(head);
            StorybookArt.CombineBone(shell,true);
        }

        private static void CreateFace(Transform head)
        {
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*.238f;
                Transform eye=PrimitiveArt.Root(side<0?"Eye L":"Eye R",head,new Vector3(x,-.09f,.54f));
                Ball(eye,"Eye socket and lashes",new Vector3(0,.01f,-.011f),new Vector3(.335f,.398f,.094f),Hair);
                Ball(eye,"Ivory eye",Vector3.zero,new Vector3(.286f,.348f,.078f),new Color32(255,249,232,255));
                Ball(eye,"Brown iris",new Vector3(0,-.01f,.036f),new Vector3(.218f,.282f,.061f),new Color32(97,58,38,255));
                Ball(eye,"Dark pupil",new Vector3(0,.008f,.061f),new Vector3(.14f,.204f,.032f),Hair);
                Ball(eye,"Large catchlight",new Vector3(-.038f,.066f,.079f),new Vector3(.076f,.076f,.015f),Color.white);
                Ball(eye,"Small catchlight",new Vector3(.039f,-.067f,.074f),new Vector3(.028f,.028f,.010f),Color.white);
                StorybookArt.CombineBone(eye,true);
                Transform brow=PrimitiveArt.Root(side<0?"Brow L":"Brow R",head,new Vector3(x,.174f,.535f));
                StorybookArt.Stroke(brow,"Expressive brow",new Vector3(-.126f,-.025f,0),new Vector3(0,.071f,.012f),new Vector3(.126f,-.016f,-.015f),.039f,Hair,.72f);
            }
            Transform smile=PrimitiveArt.Root("Smile",head,Vector3.zero);
            StorybookArt.Patch(smile,"Open smile",new[]{new Vector3(-.15f,-.335f,.521f),new Vector3(-.105f,-.400f,.513f),new Vector3(0,-.424f,.515f),new Vector3(.105f,-.400f,.513f),new Vector3(.15f,-.335f,.521f),new Vector3(0,-.36f,.55f)},Brown);
            StorybookArt.Patch(smile,"Happy teeth",new[]{new Vector3(-.12f,-.345f,.534f),new Vector3(-.073f,-.382f,.539f),new Vector3(0,-.393f,.546f),new Vector3(.073f,-.382f,.539f),new Vector3(.12f,-.345f,.534f),new Vector3(0,-.367f,.561f)},new Color32(255,250,232,255));
            Ball(smile,"Smile tongue",new Vector3(0,-.405f,.531f),new Vector3(.09f,.023f,.015f),new Color32(237,123,104,255));
            StorybookArt.CombineBone(smile,true);
            Transform surprised=PrimitiveArt.Root("Surprised mouth",head,new Vector3(0,-.37f,.526f));
            Ball(surprised,"O mouth",Vector3.zero,new Vector3(.13f,.18f,.025f),Brown);
            Transform concern=PrimitiveArt.Root("Concerned mouth",head,new Vector3(0,-.37f,.522f));
            StorybookArt.Stroke(concern,"Concerned curve",new Vector3(-.115f,-.015f,0),new Vector3(0,.045f,.022f),new Vector3(.115f,-.015f,0),.015f,Brown,.6f);
            // All expression bones are nonzero at bind time; the expression driver hides alternate mouths.
        }

        private static Transform Ball(Transform parent,string name,Vector3 position,Vector3 scale,Color color)
        {
            int segments=10,rings=6;
            if(name.Contains("face")) { segments=24; rings=16; }
            else if(name=="Hair cap") { segments=20; rings=14; }
            else if(name.Contains("Eye") || name.Contains("eye") || name.Contains("iris") || name.Contains("pupil")) { segments=12; rings=8; }
            else if(name.Contains("toe") || name.Contains("finger") || name.Contains("catchlight")) { segments=8; rings=6; }
            return PrimitiveArt.MeshPart(parent,name,StorybookArt.Sphere(segments,rings),position,scale,color);
        }

        private static void Diamond(Transform parent,Vector3 p,float radius,Color color)
        {
            Vector3[] rim={Vector3.up*radius,Vector3.left*radius,Vector3.down*radius,Vector3.right*radius};
            var vertices=new Vector3[8]; var faces=new int[30];
            for(int i=0;i<4;i++)
            {
                vertices[i]=p+rim[i]; vertices[i+4]=p+rim[i]*.73f+Vector3.forward*radius*.22f;
                int j=(i+1)%4,k=i*6; faces[k]=i; faces[k+1]=j; faces[k+2]=j+4; faces[k+3]=i; faces[k+4]=j+4; faces[k+5]=i+4;
            }
            faces[24]=4; faces[25]=5; faces[26]=6; faces[27]=4; faces[28]=6; faces[29]=7;
            var mesh=new Mesh{name="Beveled embossed diamond",vertices=vertices,triangles=faces}; mesh.RecalculateNormals();
            PrimitiveArt.MeshPart(parent,"Dark diamond bevel",mesh,Vector3.zero,Vector3.one,color);
            StorybookArt.Patch(parent,"Raised diamond center",new[]{p+rim[0]*.57f+Vector3.forward*radius*.235f,p+rim[1]*.57f+Vector3.forward*radius*.235f,p+rim[2]*.57f+Vector3.forward*radius*.235f,p+rim[3]*.57f+Vector3.forward*radius*.235f},Stitch);
        }

        private static Mesh HeadbandMesh(float height,float yOffset=0)
        {
            const int sides=24; var vertices=new Vector3[(sides+1)*4]; var faces=new int[sides*18];
            float[] y={-.5f,-.41f,.41f,.5f};
            for(int row=0;row<4;row++) for(int i=0;i<=sides;i++)
            {
                float a=i*Mathf.PI*2/sides,k=row==0 || row==3 ? .987f : 1;
                vertices[row*(sides+1)+i]=new Vector3(Mathf.Sin(a)*.644f*k,.268f+yOffset+Mathf.Cos(a)*.040f+y[row]*height,Mathf.Cos(a)*.580f*k);
            }
            for(int row=0;row<3;row++) for(int i=0;i<sides;i++)
            { int a=row*(sides+1)+i,b=a+1,c=a+sides+1,d=c+1,k=(row*sides+i)*6; faces[k]=a; faces[k+1]=b; faces[k+2]=c; faces[k+3]=b; faces[k+4]=d; faces[k+5]=c; }
            var mesh=new Mesh{name="Curved woven headband",vertices=vertices,triangles=faces}; mesh.RecalculateNormals();
            Vector3[] n=mesh.normals; for(int row=0;row<4;row++) { int a=row*(sides+1),b=a+sides; n[a]=n[b]=(n[a]+n[b]).normalized; } mesh.normals=n; return mesh;
        }
        private static void Triangle(Transform parent,Vector3 p,float size,Color color)
        { StorybookArt.Patch(parent,"Triangle pattern",new[]{p+new Vector3(-size*.5f,-size*.5f,0),p+new Vector3(size*.5f,-size*.5f,0),p+Vector3.up*size*.5f},color); }
    }
}
