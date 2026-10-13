using System.Collections.Generic;
using UnityEngine;

namespace AndamRunner
{
    // Original 3D interpretations of the user's runner sheet. No world-space item labels.
    public static class RunnerReferenceArt
    {
        private static readonly Color Blue=new Color32(20,107,255,255);
        private static readonly Color Aqua=new Color32(44,218,255,255);
        private static readonly Color Dark=new Color32(43,47,52,255);
        private static readonly Color Gold=new Color32(255,211,23,255);
        private static readonly Color Red=new Color32(255,60,83,255);
        private static readonly Color Ivory=new Color32(255,252,237,255);
        private static readonly Color Violet=new Color32(151,65,255,255);
        private static readonly Color SupplyMint=new Color32(48,255,167,255);
        private static readonly Color DistractorOrange=new Color32(255,132,26,255);
        private static readonly Color Wood=new Color32(137,92,53,255);
        private static Mesh rock;

        public static Transform PowerUp(Transform parent,PowerUpKind kind)
        {
            Surfaces();
            Transform art=PrimitiveArt.Root(kind+" / power-up icon",parent,Vector3.zero);
            Color color=kind==PowerUpKind.DoubleScore ? new Color32(27,210,126,255) : kind==PowerUpKind.QuadrupleScore ? Violet : Gold;
            Disk(art,"Bonus contrast rim",new Vector3(0,.041f,0),new Vector3(.54f,.02f,.54f),Dark);
            Disk(art,"Blue bonus marker",new Vector3(0,.045f,0),new Vector3(.47f,.02f,.47f),new Color32(48,216,255,255));
            if(kind==PowerUpKind.Magnet)
            {
                StorybookArt.Stroke(art,"Red horseshoe curve",new Vector3(-.28f,.72f,0),new Vector3(0,.21f,0),new Vector3(.28f,.72f,0),.11f,Red);
                for(int side=-1;side<=1;side+=2)
                {
                    Round(art,"Magnet upright",new Vector3(side*.28f,.83f,0),new Vector3(.20f,.35f,.22f),Red);
                    Round(art,"White magnet pole",new Vector3(side*.28f,1.02f,0),new Vector3(.20f,.13f,.22f),Ivory);
                    Round(art,"Blue pole cap",new Vector3(side*.28f,1.10f,0),new Vector3(.20f,.06f,.22f),Blue);
                }
            }
            else
            {
                Round(art,"Colorful multiplier badge",new Vector3(0,.72f,0),new Vector3(.85f,.88f,.24f),color);
                Round(art,"Cream number face",new Vector3(0,.72f,-.137f),new Vector3(.73f,.75f,.025f),Ivory);
                int number=kind==PowerUpKind.DoubleScore?2:kind==PowerUpKind.QuadrupleScore?4:8;
                int mask=number==2?91:number==4?102:127;
                Vector3[] positions={new Vector3(-.14f,.93f,-.165f),new Vector3(-.035f,.825f,-.165f),new Vector3(-.035f,.615f,-.165f),new Vector3(-.14f,.51f,-.165f),new Vector3(-.245f,.615f,-.165f),new Vector3(-.245f,.825f,-.165f),new Vector3(-.14f,.72f,-.165f)};
                for(int i=0;i<7;i++) if((mask & (1<<i))!=0)
                    PrimitiveArt.MeshPart(art,"Number segment",StorybookArt.RoundedBox(2),positions[i],i==0||i==3||i==6?new Vector3(.235f,.055f,.036f):new Vector3(.055f,.205f,.036f),color);
                for(int side=-1;side<=1;side+=2)
                    PrimitiveArt.MeshPart(art,"Multiply symbol",StorybookArt.RoundedBox(2),new Vector3(.20f,.72f,-.165f),new Vector3(.060f,.29f,.036f),color,new Vector3(0,0,side*45));
            }
            Star(art,new Vector3(.42f,1.13f,0),.10f,Gold);
            StorybookArt.CombineBone(art,true);
            return art;
        }

        public static Transform Supply(Transform parent,SupplyKind kind)
        {
            Surfaces();
            Transform art=PrimitiveArt.Root(PreparednessContent.SupplyNames[(int)kind]+" / icon only",parent,Vector3.zero);
            Disk(art,"Pickup contrast rim",new Vector3(0,.041f,0),new Vector3(.54f,.02f,.54f),Dark);
            Disk(art,"Pickup glow",new Vector3(0,.045f,0),new Vector3(.47f,.02f,.47f),SupplyMint);
            switch(kind)
            {
                case SupplyKind.GoBag:
                    Round(art,"Emergency backpack",new Vector3(0,.66f,0),new Vector3(.74f,.84f,.39f),new Color32(255,104,31,255));
                    Round(art,"Front pocket",new Vector3(0,.52f,-.24f),new Vector3(.55f,.35f,.14f),Gold);
                    Round(art,"Backpack top handle",new Vector3(0,1.13f,0),new Vector3(.31f,.075f,.075f),Dark);
                    for(int side=-1;side<=1;side+=2) Round(art,"Shoulder strap",new Vector3(side*.24f,.71f,.24f),new Vector3(.09f,.65f,.08f),Dark);
                    Cross(art,new Vector3(0,.54f,-.319f),.17f,Ivory);
                    break;
                case SupplyKind.Water: Bottle(art); break;
                case SupplyKind.FirstAid: FirstAid(art); break;
                case SupplyKind.Flashlight: Flashlight(art); break;
                case SupplyKind.Whistle:
                    Round(art,"Whistle rounded chamber",new Vector3(0,.62f,.07f),new Vector3(.46f,.31f,.36f),Gold);
                    Round(art,"Whistle mouthpiece",new Vector3(0,.66f,-.20f),new Vector3(.23f,.17f,.38f),Gold);
                    Thin(art,"Air slot",new Vector3(0,.755f,-.08f),new Vector3(.14f,.018f,.14f),Dark);
                    PrimitiveArt.MeshPart(art,"Lanyard ring",CoastalPolish.Hoop(),new Vector3(0,.73f,.31f),Vector3.one*.15f,Dark);
                    break;
                case SupplyKind.Radio:
                    Round(art,"Emergency radio",new Vector3(0,.61f,0),new Vector3(.78f,.53f,.35f),Red);
                    Round(art,"Speaker panel",new Vector3(-.15f,.61f,-.185f),new Vector3(.34f,.36f,.025f),Dark);
                    for(int i=0;i<4;i++) Thin(art,"Speaker grille",new Vector3(-.15f,.49f+i*.078f,-.21f),new Vector3(.26f,.018f,.015f),Ivory);
                    Ball(art,"Radio tuning knob",new Vector3(.23f,.62f,-.215f),new Vector3(.15f,.15f,.05f),Gold);
                    Round(art,"Radio handle",new Vector3(0,.91f,0),new Vector3(.36f,.07f,.10f),Dark);
                    StorybookArt.Stroke(art,"Radio antenna",new Vector3(-.25f,.85f,.02f),new Vector3(-.31f,1.11f,.02f),new Vector3(-.36f,1.30f,.02f),.018f,Ivory);
                    break;
                case SupplyKind.ContactCard:
                    Round(art,"Family contact card",new Vector3(0,.67f,0),new Vector3(.84f,.59f,.07f),Ivory);
                    Ball(art,"Family portrait",new Vector3(-.23f,.72f,-.046f),new Vector3(.20f,.20f,.024f),Blue);
                    for(int i=0;i<3;i++) Thin(art,"Contact card markings",new Vector3(.12f,.82f-i*.13f,-.048f),new Vector3(.33f,.023f,.012f),Blue);
                    break;
                case SupplyKind.Cash: Cash(art); break;
                case SupplyKind.Food: Food(art); break;
            }
            Star(art,new Vector3(.38f,.33f,-.06f),.10f,Gold);
            Star(art,new Vector3(-.35f,.96f,.05f),.065f,Ivory);
            StorybookArt.CombineBone(art,true);
            return art;
        }

        private static void Cash(Transform art)
        {
            Color note=new Color32(39,220,107,255);
            for(int i=0;i<3;i++)
                Round(art,"Fictional emergency banknote",new Vector3((i-1)*.035f,.54f+i*.065f,.055f-i*.06f),new Vector3(.78f,.36f,.055f),i==2?note:SupplyMint);
            FaceDisk(art,"Banknote emblem",new Vector3(.035f,.67f,-.105f),.115f,PreparednessContent.Cream);
            Thin(art,"Peso symbol stem",new Vector3(.01f,.67f,-.116f),new Vector3(.022f,.17f,.009f),note);
            Thin(art,"Peso symbol upper bar",new Vector3(.052f,.718f,-.116f),new Vector3(.10f,.020f,.009f),note);
            Thin(art,"Peso symbol lower bar",new Vector3(.052f,.682f,-.116f),new Vector3(.10f,.020f,.009f),note);
            Thin(art,"Note border left",new Vector3(-.275f,.67f,-.105f),new Vector3(.027f,.24f,.008f),Ivory);
            Thin(art,"Note border right",new Vector3(.33f,.67f,-.105f),new Vector3(.027f,.24f,.008f),Ivory);
            Transform coin=PrimitiveArt.Root("Emergency coin",art,new Vector3(.30f,.35f,-.18f)); coin.localRotation=Quaternion.Euler(-90,0,0);
            Band(coin,"Gold coin edge",Vector3.zero,.13f,.13f,.055f,Gold);
            Disk(coin,"Gold coin face",new Vector3(0,.03f,0),new Vector3(.12f,.008f,.12f),Gold);
            Disk(coin,"Gold coin reverse",new Vector3(0,-.03f,0),new Vector3(.12f,.008f,.12f),Gold);
        }

        private static void Food(Transform art)
        {
            Transform can=PrimitiveArt.Root("Ready-to-eat pull-tab can",art,new Vector3(-.19f,.58f,0));
            Band(can,"Canned food",Vector3.zero,.23f,.23f,.56f,Violet);
            Disk(can,"Can lid",new Vector3(0,.29f,0),new Vector3(.225f,.016f,.225f),Ivory);
            Disk(can,"Can base",new Vector3(0,-.29f,0),new Vector3(.225f,.016f,.225f),Ivory);
            Round(can,"Pull tab",new Vector3(0,.312f,-.025f),new Vector3(.085f,.018f,.15f),Dark);
            FaceDisk(can,"Food badge",new Vector3(0,0,-.241f),.16f,Ivory);
            StorybookArt.Patch(can,"Fish food pictogram",new[]{new Vector3(-.13f,0,-.252f),new Vector3(-.055f,.05f,-.252f),new Vector3(.055f,.06f,-.252f),new Vector3(.12f,0,-.252f),new Vector3(.055f,-.055f,-.252f),new Vector3(-.055f,-.045f,-.252f)},Gold);
            Transform packet=PrimitiveArt.Root("Sealed cracker packet",art,new Vector3(.25f,.61f,-.025f)); packet.localRotation=Quaternion.Euler(0,0,-10);
            Round(packet,"Non-perishable snack packet",Vector3.zero,new Vector3(.35f,.65f,.18f),Ivory);
            for(int side=-1;side<=1;side+=2) Thin(packet,"Sealed packet seam",new Vector3(0,side*.29f,-.101f),new Vector3(.31f,.055f,.018f),PreparednessContent.Orange);
            for(int i=0;i<2;i++) Thin(packet,"Cracker pictogram",new Vector3(0,.09f-i*.18f,-.104f),new Vector3(.18f,.13f,.020f),Gold,new Vector3(0,0,i*15));
        }

        public static Transform Distractor(Transform parent,DistractorKind kind)
        {
            Surfaces();
            Transform art=PrimitiveArt.Root(PreparednessContent.DistractorNames[(int)kind]+" / icon only",parent,Vector3.zero);
            Disk(art,"Distractor contrast rim",new Vector3(0,.041f,0),new Vector3(.54f,.02f,.54f),Dark);
            Disk(art,"Distractor orange marker",new Vector3(0,.045f,0),new Vector3(.47f,.02f,.47f),DistractorOrange);
            switch(kind)
            {
                case DistractorKind.GymEquipment: Dumbbell(art); break;
                case DistractorKind.Toys: ToyRobot(art); break;
                case DistractorKind.Beer: BeerBottle(art); break;
                case DistractorKind.Makeup: Makeup(art); break;
                case DistractorKind.GameConsole: GameConsole(art); break;
            }
            StorybookArt.CombineBone(art,true);
            return art;
        }

        private static void Dumbbell(Transform art)
        {
            Transform weight=PrimitiveArt.Root("Bulky gym dumbbell",art,new Vector3(0,.62f,0)); weight.localRotation=Quaternion.Euler(0,0,-15);
            Round(weight,"Dumbbell handle",Vector3.zero,new Vector3(.88f,.12f,.12f),Ivory);
            for(int side=-1;side<=1;side+=2)
            {
                Transform plate=PrimitiveArt.Root("Rounded weight plates",weight,new Vector3(side*.37f,0,0)); plate.localRotation=Quaternion.Euler(0,0,90);
                Band(plate,"Purple weight",Vector3.zero,.28f,.28f,.20f,Violet);
                for(int end=-1;end<=1;end+=2)
                {
                    Transform face=PrimitiveArt.Root("Solid weight face",plate,new Vector3(0,end*.10f,0));
                    if(end<0) face.localRotation=Quaternion.Euler(180,0,0);
                    Disk(face,"Purple end cap",Vector3.zero,new Vector3(.266f,.01f,.266f),Violet,12);
                }
                Band(plate,"Pink weight collar",new Vector3(0,side*.12f,0),.20f,.20f,.07f,Red);
                Transform collar=PrimitiveArt.Root("Weight collar face",plate,new Vector3(0,side*.155f,0));
                if(side<0) collar.localRotation=Quaternion.Euler(180,0,0);
                Disk(collar,"Pink end cap",Vector3.zero,new Vector3(.19f,.01f,.19f),Red,12);
            }
        }

        private static void ToyRobot(Transform art)
        {
            Round(art,"Bulky toy robot head",new Vector3(0,.93f,0),new Vector3(.58f,.34f,.28f),Gold);
            Round(art,"Toy robot body",new Vector3(0,.57f,0),new Vector3(.43f,.39f,.24f),Blue);
            Thin(art,"Toy antenna",new Vector3(0,1.15f,0),new Vector3(.045f,.12f,.045f),Dark);
            Ball(art,"Toy antenna tip",new Vector3(0,1.23f,0),Vector3.one*.09f,Red);
            for(int side=-1;side<=1;side+=2)
            {
                FaceDisk(art,"Toy eye",new Vector3(side*.135f,.95f,-.151f),.045f,Dark);
                Thin(art,"Toy arm",new Vector3(side*.30f,.62f,0),new Vector3(.14f,.11f,.12f),Gold);
                Ball(art,"Toy rounded hand",new Vector3(side*.38f,.57f,0),Vector3.one*.15f,Red);
                Thin(art,"Toy boot",new Vector3(side*.14f,.31f,-.03f),new Vector3(.18f,.16f,.28f),Red);
            }
            Thin(art,"Toy smile",new Vector3(0,.85f,-.151f),new Vector3(.15f,.020f,.008f),Dark);
            FaceDisk(art,"Toy body button",new Vector3(0,.60f,-.133f),.07f,Gold);
        }

        private static void BeerBottle(Transform art)
        {
            Color amber=new Color32(146,87,32,255);
            PrimitiveArt.MeshPart(art,"Unbranded beer bottle",StorybookArt.Ellipse("Amber bottle shoulders",new[]{.25f,.29f,.75f,.86f,1.02f,1.16f},new[]{.16f,.19f,.19f,.11f,.085f,.085f},new[]{.16f,.19f,.19f,.11f,.085f,.085f},12),Vector3.zero,Vector3.one,amber);
            Disk(art,"Beer bottle base",new Vector3(0,.251f,0),new Vector3(.16f,.012f,.16f),amber);
            Band(art,"Plain silver bottle cap",new Vector3(0,1.17f,0),.105f,.105f,.055f,Ivory);
            Disk(art,"Bottle cap top",new Vector3(0,1.20f,0),new Vector3(.10f,.008f,.10f),Ivory);
            Round(art,"Plain beer label",new Vector3(0,.60f,-.192f),new Vector3(.28f,.32f,.024f),Ivory);
            Thin(art,"Grain label stem",new Vector3(0,.60f,-.211f),new Vector3(.014f,.21f,.008f),Gold);
            for(int i=0;i<3;i++) Thin(art,"Grain label marking",new Vector3((i%2==0?1:-1)*.035f,.56f+i*.055f,-.211f),new Vector3(.062f,.022f,.008f),Gold,new Vector3(0,0,i%2==0?30:-30));
        }

        private static void Makeup(Transform art)
        {
            Color pink=new Color32(255,58,173,255);
            Band(art,"Lipstick gold tube",new Vector3(-.24f,.52f,-.015f),.12f,.12f,.39f,Gold);
            Band(art,"Lipstick rose tip",new Vector3(-.24f,.83f,-.015f),.087f,.087f,.28f,pink);
            Disk(art,"Lipstick tip",new Vector3(-.24f,.974f,-.015f),new Vector3(.08f,.01f,.08f),pink);
            Round(art,"Makeup compact case",new Vector3(.18f,.47f,0),new Vector3(.48f,.15f,.40f),pink);
            Round(art,"Open compact lid",new Vector3(.18f,.77f,.13f),new Vector3(.48f,.45f,.07f),pink);
            FaceDisk(art,"Compact mirror",new Vector3(.18f,.78f,.086f),.16f,new Color32(200,230,235,255));
            Thin(art,"Mirror shine",new Vector3(.12f,.83f,.077f),new Vector3(.028f,.15f,.008f),Ivory,new Vector3(0,0,-25));
        }

        private static void GameConsole(Transform art)
        {
            Round(art,"Unbranded game console",new Vector3(0,.82f,.10f),new Vector3(.75f,.44f,.22f),Violet);
            Thin(art,"Console vent",new Vector3(0,.89f,-.020f),new Vector3(.56f,.045f,.015f),Dark);
            FaceDisk(art,"Console power light",new Vector3(.25f,.77f,-.021f),.025f,Blue);
            Round(art,"Game controller",new Vector3(0,.46f,-.14f),new Vector3(.57f,.23f,.15f),Dark);
            for(int side=-1;side<=1;side+=2) Ball(art,"Controller rounded grip",new Vector3(side*.23f,.35f,-.12f),new Vector3(.25f,.25f,.20f),Dark);
            Cross(art,new Vector3(-.16f,.47f,-.225f),.12f,Ivory);
            for(int i=0;i<4;i++) { float a=i*Mathf.PI/2; FaceDisk(art,"Controller button",new Vector3(.16f+Mathf.Cos(a)*.06f,.47f+Mathf.Sin(a)*.06f,-.225f),.026f,i%2==0?Red:Blue); }
        }

        private static void Bottle(Transform art)
        {
            Transform bottle=PrimitiveArt.Root("Blue water bottle",art,new Vector3(0,.13f,0));
            bottle.localRotation=Quaternion.Euler(0,0,-12);
            PrimitiveArt.MeshPart(bottle,"Sculpted bottle",StorybookArt.Ellipse("Bottle shoulders",new[]{.10f,.16f,.24f,.79f,.88f,1.01f,1.07f},new[]{.16f,.21f,.23f,.23f,.21f,.12f,.12f},new[]{.16f,.21f,.23f,.23f,.21f,.12f,.12f},16),Vector3.zero,Vector3.one,Aqua);
            Disk(bottle,"Bottle bottom",new Vector3(0,.105f,0),new Vector3(.16f,.02f,.16f),Aqua);
            Band(bottle,"Blue cap",new Vector3(0,1.08f,0),.16f,.16f,.16f,Blue);
            Disk(bottle,"Cap top",new Vector3(0,1.16f,0),new Vector3(.155f,.02f,.155f),Blue);
            Band(bottle,"Blue water label",new Vector3(0,.57f,0),.255f,.255f,.30f,Blue);
            for(int side=0;side<8;side++)
            {
                float a=side*Mathf.PI/4;
                Thin(bottle,"Cap rib",new Vector3(Mathf.Cos(a)*.16f,1.085f,Mathf.Sin(a)*.16f),new Vector3(.014f,.11f,.014f),new Color32(82,164,246,255));
            }
            StorybookArt.Patch(bottle,"Water drop symbol",new[]{new Vector3(0,.69f,-.263f),new Vector3(.09f,.55f,-.263f),new Vector3(.068f,.49f,-.263f),new Vector3(0,.47f,-.263f),new Vector3(-.068f,.49f,-.263f),new Vector3(-.09f,.55f,-.263f)},Ivory);
            Thin(bottle,"Bottle shine",new Vector3(-.12f,.76f,-.199f),new Vector3(.025f,.15f,.012f),new Color32(205,241,251,255));
        }

        private static void FirstAid(Transform art)
        {
            Round(art,"Cream medical case",new Vector3(0,.60f,0),new Vector3(.88f,.69f,.36f),Ivory);
            for(int side=-1;side<=1;side+=2)
            {
                Round(art,"Red case clasp",new Vector3(side*.43f,.61f,-.015f),new Vector3(.075f,.26f,.38f),Red);
                Round(art,"Red handle upright",new Vector3(side*.20f,1.005f,0),new Vector3(.08f,.18f,.09f),Red);
            }
            Round(art,"Carry handle",new Vector3(0,1.075f,0),new Vector3(.44f,.09f,.10f),Red);
            FaceDisk(art,"Red medical badge",new Vector3(0,.62f,-.193f),.225f,Red);
            Cross(art,new Vector3(0,.62f,-.207f),.29f,Ivory);
        }

        private static void Flashlight(Transform art)
        {
            Transform torch=PrimitiveArt.Root("Gold and graphite flashlight",art,new Vector3(0,.70f,0));
            torch.localRotation=Quaternion.Euler(-38,0,-48);
            PrimitiveArt.MeshPart(torch,"Flashlight grip",StorybookArt.Ellipse("Torch grip",new[]{-.40f,-.35f,.09f,.13f},new[]{.095f,.12f,.12f,.14f},new[]{.095f,.12f,.12f,.14f},16),Vector3.zero,Vector3.one,Dark);
            Band(torch,"Gold grip collar",new Vector3(0,-.26f,0),.137f,.137f,.07f,Gold);
            Band(torch,"Gold neck collar",new Vector3(0,.075f,0),.16f,.16f,.085f,Gold);
            PrimitiveArt.MeshPart(torch,"Wide torch head",StorybookArt.Ellipse("Torch head",new[]{.13f,.19f,.27f,.35f},new[]{.14f,.22f,.245f,.245f},new[]{.14f,.22f,.245f,.245f},16),Vector3.zero,Vector3.one,Dark);
            Band(torch,"Gold lens rim",new Vector3(0,.34f,0),.27f,.27f,.075f,Gold);
            Disk(torch,"Warm light lens",new Vector3(0,.38f,0),new Vector3(.215f,.015f,.215f),new Color32(255,232,153,255));
            Disk(torch,"Lens highlight",new Vector3(-.055f,.398f,-.035f),new Vector3(.11f,.008f,.11f),Ivory);
            Round(torch,"Switch",new Vector3(0,-.04f,-.125f),new Vector3(.085f,.16f,.025f),Gold);
        }

        public static Transform Hazard(Transform parent,HazardKind kind)
        {
            Surfaces();
            Transform art=PrimitiveArt.Root(PreparednessContent.HazardNames[(int)kind]+" / warning symbols",parent,Vector3.zero);
            switch(kind)
            {
                case HazardKind.Floodwater: Flood(art); break;
                case HazardKind.PowerLine: Wires(art); break;
                case HazardKind.Debris: Rubble(art); break;
                case HazardKind.RoadBlock: Barrier(art,Vector3.zero); break;
                case HazardKind.HangingCable: HangingCable(art); break;
                case HazardKind.FloodStreak: Flood(art,PreparednessContent.FloodStreakLength,true); break;
            }
            StorybookArt.CombineBone(art,true);
            return art;
        }

        private static void Flood(Transform art,float length=4.7f,bool streak=false)
        {
            Color waterColor=streak ? new Color32(22,133,224,255) : Aqua;
            Round(art,"Floodwater footprint",new Vector3(0,.055f,0),new Vector3(2.25f,.10f,length),new Color32(31,145,207,255));
            const int across=8; int along=Mathf.CeilToInt(length/.4f); var vertices=new List<Vector3>(); var faces=new List<int>();
            for(int row=0;row<=along;row++) for(int col=0;col<=across;col++)
            {
                float x=-1.08f+2.16f*col/across,z=-length*.5f+.07f+(length-.14f)*row/along;
                vertices.Add(new Vector3(x,.17f+Mathf.Sin(x*3.2f+z*4.4f)*.030f+Mathf.Cos(z*3)*.012f,z));
            }
            for(int row=0;row<along;row++) for(int col=0;col<across;col++)
            { int a=row*(across+1)+col,b=a+1,c=a+across+1,d=c+1; faces.AddRange(new[]{a,c,b,b,c,d}); }
            Mesh water=new Mesh{name="Rolling stylized floodwater",vertices=vertices.ToArray(),triangles=faces.ToArray()}; water.RecalculateNormals();
            PrimitiveArt.MeshPart(art,"Blue rolling floodwater",water,Vector3.zero,Vector3.one,waterColor);
            for(int i=0;i<Mathf.FloorToInt(length);i++)
            {
                float z=-length*.5f+.65f+i;
                StorybookArt.Stroke(art,"Foam crest",new Vector3(-.85f,.24f,z),new Vector3(-.1f,.30f,z-.2f),new Vector3(.83f,.245f,z+.05f),.019f,new Color32(220,247,255,255));
            }
            Round(art,"Floating timber",new Vector3(-.36f,.20f,-.65f),new Vector3(.85f,.20f,.26f),Wood,new Vector3(0,29,11));
            Barrier(art,new Vector3(0,0,length*.5f-.60f),.58f);
            if(streak) Barrier(art,new Vector3(0,0,-length*.5f+.60f),.58f);
        }

        private static void Rubble(Transform art)
        {
            Disk(art,"Broken pavement shadow",new Vector3(0,.045f,0),new Vector3(1.05f,.015f,.78f),Dark);
            for(int i=0;i<8;i++)
            {
                float angle=i*Mathf.PI/4;
                Vector3 p=new Vector3(Mathf.Cos(angle)*.74f,.08f,Mathf.Sin(angle)*.49f);
                PrimitiveArt.MeshPart(art,"Tumbled pavement stone",Rock(),p,new Vector3(.40f,.22f+(i%3)*.08f,.35f),i%2==0?new Color32(100,103,107,255):new Color32(126,116,99,255),new Vector3(0,i*43,0));
            }
            for(int i=0;i<3;i++) Round(art,"Cracked concrete slab",new Vector3((i-1)*.48f,.20f,(i%2)*.23f-.08f),new Vector3(.70f,.20f,.45f),new Color32(111,115,119,255),new Vector3(0,i*29-21,i*5-5));
            Thin(art,"Concrete crack",new Vector3(-.3f,.309f,-.08f),new Vector3(.025f,.01f,.34f),Dark,new Vector3(0,24,0));
            Thin(art,"Warning pavement edge",new Vector3(0,.066f,-.72f),new Vector3(1.5f,.035f,.06f),Gold);
        }

        private static void Wires(Transform art)
        {
            Round(art,"Electrical exclusion patch",new Vector3(0,.04f,0),new Vector3(2.25f,.065f,1.8f),new Color32(246,168,48,255));
            for(int side=-1;side<=1;side+=2)
            {
                Round(art,"Damaged wooden post",new Vector3(side*.89f,.47f,.1f),new Vector3(.19f,.91f,.23f),Wood,new Vector3(0,0,-side*9));
                Band(art,"Cable ties",new Vector3(side*.87f,.72f,.10f),.15f,.17f,.09f,Dark);
            }
            for(int i=0;i<3;i++)
                StorybookArt.Stroke(art,"Sagging fallen cable",new Vector3(-.92f,.80f-i*.08f,-.16f+i*.12f),new Vector3(0,-.47f+i*.14f,-.45f),new Vector3(.92f,.76f-i*.04f,.07f+i*.12f),.035f,Dark);
            for(int i=0;i<3;i++)
            {
                Vector3 p=new Vector3((i-1)*.56f,.31f+(i%2)*.10f,-.52f);
                Bolt(art,p,.26f,Ivory); Bolt(art,p+new Vector3(0,0,-.012f),.20f,Gold);
            }
        }

        // A fictional disconnected cable beneath a covered passage. No electrical bolts:
        // this obstacle's downward arrows must differ from the live-wire stay-away symbol.
        private static void HangingCable(Transform art)
        {
            Color blueCable=new Color32(48,93,122,255);
            Color canopy=new Color32(34,145,150,255);
            for(int side=-1;side<=1;side+=2)
            {
                Round(art,"Covered passage post",new Vector3(side*4.10f,1.70f,.25f),new Vector3(.25f,3.40f,.30f),Wood);
                Round(art,"Passage foot",new Vector3(side*4.10f,.10f,.25f),new Vector3(.48f,.20f,.50f),Ivory);
                Band(art,"Disconnected cable end cap",new Vector3(side*3.92f,3.03f,0),.12f,.12f,.18f,PreparednessContent.Mint);
            }
            Round(art,"Shelter canopy",new Vector3(0,3.55f,.30f),new Vector3(8.65f,.22f,1.35f),canopy);
            for(int i=0;i<2;i++)
                StorybookArt.Stroke(art,"Low disconnected hanging wire",new Vector3(-3.94f,3.05f+i*.12f,i*.13f),new Vector3(0,1.55f+i*.14f,i*.13f),new Vector3(3.94f,3.05f+i*.12f,i*.13f),.045f,blueCable);
            for(int lane=-1;lane<=1;lane++)
            {
                float x=lane*PlayerRunner.LaneWidth;
                Thin(art,"Duck symbol plate",new Vector3(x,2.47f,-.10f),new Vector3(.48f,.48f,.035f),canopy);
                StorybookArt.Patch(art,"Down arrow pictogram",new[]{new Vector3(x-.055f,2.64f,-.126f),new Vector3(x+.055f,2.64f,-.126f),new Vector3(x+.055f,2.45f,-.126f),new Vector3(x+.17f,2.45f,-.126f),new Vector3(x,2.27f,-.126f),new Vector3(x-.17f,2.45f,-.126f),new Vector3(x-.055f,2.45f,-.126f)},Ivory);
            }
        }

        private static void Barrier(Transform art,Vector3 p,float scale=1)
        {
            Transform barrier=PrimitiveArt.Root("Yellow-black safety barrier",art,p); barrier.localScale=Vector3.one*scale;
            for(int side=-1;side<=1;side+=2)
            {
                Round(barrier,"Wooden barrier post",new Vector3(side*.86f,.52f,0),new Vector3(.24f,1.04f,.30f),Wood,new Vector3(0,0,-side*5));
                Thin(barrier,"Post grain",new Vector3(side*.87f,.56f,-.158f),new Vector3(.025f,.69f,.012f),new Color32(174,125,78,255));
            }
            Round(barrier,"Dark warning board",new Vector3(0,.65f,-.12f),new Vector3(2.10f,.50f,.17f),Dark);
            for(int i=0;i<4;i++)
            {
                float x=-.94f+i*.52f;
                StorybookArt.Patch(barrier,"Yellow diagonal stripe",new[]{new Vector3(x,.42f,-.211f),new Vector3(x+.25f,.88f,-.211f),new Vector3(x+.48f,.88f,-.211f),new Vector3(x+.23f,.42f,-.211f)},Gold);
            }
        }

        private static Mesh Rock()
        {
            if(rock!=null) return rock;
            var v=new List<Vector3>(); var t=new List<int>(); const int sides=7;
            for(int level=0;level<2;level++) for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,r=(level==0?.52f:.42f)*(1+(i%3-1)*.12f);
                v.Add(new Vector3(Mathf.Cos(a)*r,level==0?0:.70f+(i%2)*.16f,Mathf.Sin(a)*r));
            }
            v.Add(new Vector3(.03f,1,0));
            for(int i=0;i<sides;i++) { int j=(i+1)%sides; t.AddRange(new[]{i,i+sides,j,j,i+sides,j+sides,i+sides,14,j+sides}); }
            rock=new Mesh{name="Rounded irregular rubble",vertices=v.ToArray(),triangles=t.ToArray()}; rock.RecalculateNormals(); return rock;
        }

        private static void Surfaces()
        {
            PrimitiveArt.Mat(Blue).SetFloat("_Sheen",.13f);
            PrimitiveArt.Mat(Aqua).SetFloat("_Sheen",.18f);
            PrimitiveArt.Mat(Dark).SetFloat("_Sheen",.18f);
            PrimitiveArt.Mat(Gold).SetFloat("_Sheen",.22f);
            PrimitiveArt.Mat(Ivory).SetFloat("_Sheen",.07f);
        }

        private static Transform Round(Transform parent,string name,Vector3 p,Vector3 scale,Color color,Vector3 rotation=default)
        { return PrimitiveArt.MeshPart(parent,name,StorybookArt.RoundedBox(3),p,scale,color,rotation); }
        private static Transform Thin(Transform parent,string name,Vector3 p,Vector3 scale,Color color,Vector3 rotation=default)
        { return PrimitiveArt.Shape(PrimitiveType.Cube,parent,name,p,scale,color,rotation); }
        private static Transform Ball(Transform parent,string name,Vector3 p,Vector3 scale,Color color)
        { return PrimitiveArt.MeshPart(parent,name,StorybookArt.Sphere(8,6),p,scale,color); }
        private static Transform Band(Transform parent,string name,Vector3 p,float width,float depth,float height,Color color)
        { return PrimitiveArt.MeshPart(parent,name,StorybookArt.Ellipse(name,new[]{-height*.5f,-height*.4f,height*.4f,height*.5f},new[]{width*.95f,width,width,width*.95f},new[]{depth*.95f,depth,depth,depth*.95f},12),p,Vector3.one,color); }
        private static void Disk(Transform parent,string name,Vector3 p,Vector3 scale,Color color,int sides=24)
        {
            var v=new List<Vector3>{Vector3.zero}; var t=new List<int>();
            for(int i=0;i<sides;i++) { float a=i*Mathf.PI*2/sides; v.Add(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))); }
            for(int i=0;i<sides;i++) t.AddRange(new[]{0,(i+1)%sides+1,i+1});
            var mesh=new Mesh{name=name,vertices=v.ToArray(),triangles=t.ToArray()}; mesh.RecalculateNormals();
            PrimitiveArt.MeshPart(parent,name,mesh,p,scale,color);
        }
        private static void FaceDisk(Transform parent,string name,Vector3 p,float radius,Color color)
        {
            var outline=new Vector3[24];
            for(int i=0;i<outline.Length;i++) { float a=-i*Mathf.PI/12; outline[i]=p+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0); }
            StorybookArt.Patch(parent,name,outline,color);
        }
        private static void Cross(Transform parent,Vector3 p,float size,Color color)
        {
            Thin(parent,"Medical cross vertical",p,new Vector3(size*.28f,size,.014f),color);
            Thin(parent,"Medical cross horizontal",p-new Vector3(0,0,.004f),new Vector3(size,size*.28f,.014f),color);
        }
        private static void Star(Transform parent,Vector3 p,float size,Color color)
        {
            var outline=new Vector3[8];
            for(int i=0;i<8;i++) { float a=Mathf.PI*.5f-i*Mathf.PI/4,r=i%2==0?size:size*.30f; outline[i]=p+new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,0); }
            StorybookArt.Patch(parent,"Pickup sparkle",outline,color);
        }
        private static void Bolt(Transform parent,Vector3 p,float size,Color color)
        {
            Vector2[] shape={new Vector2(-.13f,1),new Vector2(.60f,.06f),new Vector2(.15f,.06f),new Vector2(.30f,-1),new Vector2(-.65f,.03f),new Vector2(-.17f,.03f)};
            var outline=new Vector3[shape.Length]; for(int i=0;i<shape.Length;i++) outline[i]=p+new Vector3(shape[i].x*size,shape[i].y*size,0);
            StorybookArt.Patch(parent,"Electrical bolt symbol",outline,color);
        }
    }
}
