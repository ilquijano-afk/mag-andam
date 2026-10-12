using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AndamRunner
{
    // Menu content is separate from the run HUD, so beginners can change each independently.
    public sealed partial class UIManager
    {
        private const float SplashDuration=5f;
        private float splashTime;
        private Image splashProgress;
        private MenuPage leaderboardBack=MenuPage.Home;
        private MenuPage creditsBack=MenuPage.About;
        private MenuPage aboutBack=MenuPage.Home;
        private readonly Text[] rankNames=new Text[LeaderboardStore.Capacity];
        private readonly Text[] rankScores=new Text[LeaderboardStore.Capacity];
        private readonly Text[] rankDetails=new Text[LeaderboardStore.Capacity];
        private readonly Image[] rankRows=new Image[LeaderboardStore.Capacity];
        private Text emptyLeaderboard;
        private readonly GameObject[] instructionTabs=new GameObject[3];
        private readonly Image[] instructionButtons=new Image[3];
        private readonly bool[] illustratedPages=new bool[9];

        private void BuildMenuPages()
        {
            BuildSplash(); BuildHome(); BuildInstructions(); BuildAbout(); BuildCredits(); BuildLeaderboard();
        }

        private RectTransform PageLayer(MenuPage page,bool home)
        {
            RectTransform root=Rect(safe,page+" page",Vector2.zero,Vector2.one);
            Register(page,root.gameObject);
            string resource=page switch
            {
                MenuPage.Home=>"MenuBackground",
                MenuPage.Credits=>"CreditsBackground",
                MenuPage.Leaderboard=>"LeaderboardBackground",
                MenuPage.Instructions=>"HowToBackground",
                MenuPage.About=>"AboutBackground",
                MenuPage.Splash=>"SplashscreenBackground",
                MenuPage.Results=>"ScoreBackground",
                _=>null
            };
            Texture2D artwork=resource!=null ? Resources.Load<Texture2D>(resource) : null;
            illustratedPages[(int)page]=artwork!=null;
            if(artwork!=null)
            {
                Image surround=root.gameObject.AddComponent<Image>();
                surround.color=new Color32(15,34,54,255); surround.raycastTarget=true;
                // Fit the complete composition; the artwork and its tap areas share one frame.
                RectTransform frame=Rect(root,"Artwork and touch targets",Vector2.zero,Vector2.one);
                AspectRatioFitter frameFit=frame.gameObject.AddComponent<AspectRatioFitter>();
                frameFit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;
                frameFit.aspectRatio=(float)artwork.width/artwork.height;
                RawImage picture=frame.gameObject.AddComponent<RawImage>();
                picture.texture=artwork; picture.raycastTarget=false;
                return frame;
            }
            RectTransform illustration=Rect(root,"Painted Lapu-Lapu coast",Vector2.zero,Vector2.one);
            RawImage painting=illustration.gameObject.AddComponent<RawImage>();
            Texture2D coast=Resources.Load<Texture2D>("StorybookCoast"); painting.texture=coast; painting.raycastTarget=false;
            AspectRatioFitter fit=illustration.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio=coast!=null ? (float)coast.width/coast.height : 2f/3f;
            RectTransform backdrop=Rect(root,"Menu atmosphere",Vector2.zero,Vector2.one);
            Image shade=backdrop.gameObject.AddComponent<Image>();
            shade.color=home ? new Color32(8,38,58,255) : new Color32(8,38,58,234);
            if(home) shade.sprite=UISkin.Fade;
            shade.raycastTarget=true;
            return root;
        }

        private void BuildSplash()
        {
            RectTransform page=PageLayer(MenuPage.Splash,false);
            if(illustratedPages[(int)MenuPage.Splash])
            {
                // The startup PNG is a complete composition. Any tap skips its five-second display.
                IllustratedButtonAt(page,"CONTINUE",0,0,1,1,game.ReturnToMenu);
                return;
            }
            TextLabel(page,"ANDAM LAPU-LAPU",.06f,.90f,.94f,.97f,37,PreparednessContent.Mint,true);
            RectTransform frame=Card("Opong mascot",new Vector2(.35f,.70f),new Vector2(.65f,.88f),Color.white,page);
            RectTransform picture=Rect(frame,"Opong reference",new Vector2(.04f,.04f),new Vector2(.96f,.96f));
            RectTransform fitted=Rect(picture,"Mascot picture",Vector2.zero,Vector2.one);
            RawImage image=fitted.gameObject.AddComponent<RawImage>(); image.texture=Resources.Load<Texture2D>("OpongReference"); image.raycastTarget=false;
            AspectRatioFitter aspect=fitted.gameObject.AddComponent<AspectRatioFitter>(); aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent; aspect.aspectRatio=1;
            TextLabel(page,"DEVELOPED BY",.08f,.635f,.92f,.68f,18,PreparednessContent.Mint,true);
            TextLabel(page,game.Information.developer,.08f,.545f,.92f,.635f,30,White,true);
            TextLabel(page,"IN PARTNERSHIP WITH",.08f,.475f,.92f,.525f,18,PreparednessContent.Mint,true);
            TextLabel(page,game.Information.partner,.08f,.335f,.92f,.475f,28,White,true);
            TextLabel(page,game.Information.theme,.07f,.23f,.93f,.30f,27,PreparednessContent.Mint,true);
            TextLabel(page,"A little preparedness. A brighter tomorrow.",.07f,.19f,.93f,.235f,18,White);
            ButtonAt(page,"CONTINUE",.24f,.08f,.76f,.145f,PreparednessContent.Teal,game.ReturnToMenu,25);
            RectTransform progress=Card("Splash progress",new Vector2(.24f,.062f),new Vector2(.76f,.066f),PreparednessContent.Mint,page);
            splashProgress=progress.GetComponent<Image>(); splashProgress.type=Image.Type.Filled; splashProgress.fillMethod=Image.FillMethod.Horizontal;
        }

        private void BuildHome()
        {
            RectTransform page=PageLayer(MenuPage.Home,true);
            if(illustratedPages[(int)MenuPage.Home])
            {
                IllustratedButtonAt(page,"LET'S GET READY   >",.105f,.239f,.905f,.416f,game.StartRun);
                IllustratedButtonAt(page,"HOW TO PLAY",.082f,.145f,.500f,.217f,ShowInstructions);
                IllustratedButtonAt(page,"LEADERBOARD",.525f,.145f,.943f,.217f,ShowLeaderboard);
                IllustratedButtonAt(page,"ABOUT THE GAME",.082f,.057f,.500f,.131f,ShowAbout);
                IllustratedButtonAt(page,"CREDITS",.525f,.057f,.943f,.131f,()=>ShowCredits(MenuPage.Home));
                RectTransform rider=Card("Rider nickname",new Vector2(.10f,.008f),new Vector2(.94f,.046f),DarkPanel,page);
                TextLabel(rider,"NICKNAME",.035f,.04f,.30f,.96f,16,PreparednessContent.Mint,true,TextAnchor.MiddleLeft);
                BuildNickname(rider,new Vector2(.32f,.07f),new Vector2(.98f,.93f),22);
                return;
            }
            RectTransform portraitFrame=Rect(page,"Live 3D Opong",new Vector2(.07f,.398f),new Vector2(.93f,.733f));
            Image grounding=Rect(portraitFrame,"Soft board shadow",new Vector2(.25f,.005f),new Vector2(.75f,.13f)).gameObject.AddComponent<Image>();
            grounding.sprite=UISkin.GroundShadow; grounding.color=new Color(.09f,.20f,.24f,.22f); grounding.raycastTarget=false;
            RectTransform portraitImage=Rect(portraitFrame,"Opong model view",Vector2.zero,Vector2.one);
            RawImage view=portraitImage.gameObject.AddComponent<RawImage>(); view.raycastTarget=false;
            AspectRatioFitter portraitFit=portraitImage.gameObject.AddComponent<AspectRatioFitter>();
            portraitFit.aspectMode=AspectRatioFitter.AspectMode.FitInParent; portraitFit.aspectRatio=1;
            portrait=portraitImage.gameObject.AddComponent<StorybookPortrait>(); portrait.Initialize(game,view);
            TextLabel(page,"ANDAM LAPU-LAPU  /  IDDRR",.06f,.943f,.94f,.982f,18,PreparednessContent.Mint,true);
            Text title=TextLabel(page,"READY, SET,\nRESILIENT!",.05f,.81f,.95f,.944f,59,White,true);
            Shadow shadow=title.gameObject.AddComponent<Shadow>(); shadow.effectColor=new Color(0,0,0,.25f); shadow.effectDistance=new Vector2(0,-3);
            RectTransform theme=Card("Theme banner",new Vector2(.10f,.764f),new Vector2(.90f,.813f),DarkPanel,page);
            TextLabel(theme,game.Information.theme,.035f,.04f,.965f,.96f,26,PreparednessContent.Mint,true);
            TextLabel(page,"OPONG'S COASTAL ADVENTURE",.10f,.730f,.90f,.757f,18,PreparednessContent.Ink,true);
            RectTransform card=Card("Home controls",new Vector2(.045f,.035f),new Vector2(.955f,.402f),Panel,page);
            TextLabel(card,"RIDER NICKNAME",.06f,.815f,.40f,.943f,18,PreparednessContent.Ink,true,TextAnchor.MiddleLeft);
            BuildNickname(card,new Vector2(.43f,.81f),new Vector2(.94f,.95f),27);
            ButtonAt(card,"LET'S GET READY   >",.06f,.53f,.94f,.76f,PreparednessContent.Teal,game.StartRun,30);
            ButtonAt(card,"HOW TO PLAY",.06f,.29f,.485f,.485f,PreparednessContent.Ink,ShowInstructions,23);
            ButtonAt(card,"LEADERBOARD",.515f,.29f,.94f,.485f,PreparednessContent.Ink,ShowLeaderboard,23);
            ButtonAt(card,"ABOUT THE GAME",.06f,.07f,.485f,.245f,PreparednessContent.Ink,ShowAbout,20);
            ButtonAt(card,"CREDITS",.515f,.07f,.94f,.245f,PreparednessContent.Ink,()=>ShowCredits(MenuPage.Home),22);
        }

        private void BuildNickname(Transform parent,Vector2 min,Vector2 max,int size)
        {
            RectTransform field=Card("Nickname",min,max,new Color32(226,236,230,255),parent);
            field.GetComponent<Image>().raycastTarget=true;
            nickname=field.gameObject.AddComponent<InputField>(); nickname.targetGraphic=field.GetComponent<Image>();
            nickname.textComponent=TextLabel(field,"OPONG",.07f,.07f,.93f,.93f,size,PreparednessContent.Ink,true,TextAnchor.MiddleLeft);
            nickname.characterLimit=12; nickname.contentType=InputField.ContentType.Standard; nickname.lineType=InputField.LineType.SingleLine;
            nickname.onValidateInput=(text,index,letter)=>
            {
                char c=char.ToUpperInvariant(letter);
                return (c>='A' && c<='Z') || (c>='0' && c<='9') || c==' ' || c=='-' ? c : '\0';
            };
            nickname.SetTextWithoutNotify(game.RiderName);
        }

        private Button IllustratedButtonAt(Transform parent,string name,float x0,float y0,float x1,float y1,UnityEngine.Events.UnityAction action)
        {
            Button button=ButtonAt(parent,name,x0,y0,x1,y1,Color.clear,action,23);
            button.transition=Selectable.Transition.None;
            button.targetGraphic.canvasRenderer.cullTransparentMesh=false;
            button.GetComponentInChildren<Text>().enabled=false;
            return button;
        }

        private void IllustratedBack(RectTransform page,float y0=.042f,float y1=.120f)
        {
            IllustratedButtonAt(page,"<  BACK",.045f,y0,.423f,y1,HandleBack);
        }

        private RectTransform Chapter(MenuPage page,string eyebrow,string title,string subtitle)
        {
            RectTransform layer=PageLayer(page,false);
            if(illustratedPages[(int)page])
            {
                if(page==MenuPage.About) IllustratedBack(layer,.026f,.091f);
                else IllustratedBack(layer);
                return Rect(layer,title+" content",new Vector2(.055f,.155f),new Vector2(.945f,.795f));
            }
            TextLabel(layer,eyebrow,.07f,.938f,.93f,.98f,18,PreparednessContent.Mint,true,TextAnchor.MiddleLeft);
            TextLabel(layer,title,.07f,.853f,.93f,.94f,46,White,true,TextAnchor.MiddleLeft);
            TextLabel(layer,subtitle,.07f,.805f,.93f,.859f,20,White,false,TextAnchor.MiddleLeft);
            RectTransform card=Card(title+" content",new Vector2(.045f,.115f),new Vector2(.955f,.786f),Panel,layer);
            ButtonAt(layer,"<  BACK",.07f,.035f,.38f,.091f,PreparednessContent.Teal,HandleBack,23);
            TextLabel(layer,"READY, SET, RESILIENT!",.44f,.041f,.94f,.083f,16,PreparednessContent.Mint,true,TextAnchor.MiddleRight);
            return card;
        }

        private void BuildInstructions()
        {
            RectTransform card=Chapter(MenuPage.Instructions,"A LITTLE PRACTICE GOES A LONG WAY","HOW TO PLAY","Collect supplies. Choose safe routes. Bring a lesson home.");
            if(illustratedPages[(int)MenuPage.Instructions]) return;
            string[] titles={"CONTROLS","SUPPLIES","STAY SAFE"};
            for(int i=0;i<3;i++)
            {
                int tab=i;
                Button button=ButtonAt(card,titles[i],.04f+i*.32f,.897f,.32f+i*.32f,.968f,PreparednessContent.Teal,()=>SelectInstructionTab(tab),19);
                instructionButtons[i]=button.GetComponent<Image>();
                instructionTabs[i]=Rect(card,titles[i]+" instructions",new Vector2(.05f,.04f),new Vector2(.95f,.86f)).gameObject;
            }
            Transform controls=instructionTabs[0].transform;
            InstructionCard(controls,"01  FOLLOW THE OPEN ROUTE","Swipe LEFT or RIGHT to change lanes.\nAvoid floodwater, fallen wires and closed roads.",.745f,.98f);
            InstructionCard(controls,"02  JUMP & DUCK","Swipe UP for small debris. Swipe DOWN for low disconnected cables across all lanes.",.50f,.73f);
            InstructionCard(controls,"03  SPEED & SUPPLIES","Speed rises every 250 m; a bump resets it.\nCollect supplies. Three lives. Tap II to pause.",.255f,.485f);
            InstructionCard(controls,"04  BUTTONS & KEYBOARD","Tap LEFT, RIGHT, JUMP or DUCK.\nEditor: A/D or arrows; Space jumps; S ducks.",.01f,.24f);
            Transform supplies=instructionTabs[1].transform;
            float supplyStride=.86f/PreparednessContent.SupplyCount;
            for(int i=0;i<PreparednessContent.SupplyCount;i++)
            {
                float top=.975f-i*supplyStride;
                RectTransform row=Card("Supply "+i,new Vector2(0,top-supplyStride+.017f),new Vector2(1,top),i%2==0 ? new Color32(225,238,229,255):Color.white,supplies);
                TextLabel(row,PreparednessContent.SupplyNames[i],.05f,.05f,.77f,.95f,25,PreparednessContent.Ink,true,TextAnchor.MiddleLeft);
                TextLabel(row,"+"+PreparednessContent.Points[i],.80f,.05f,.96f,.95f,27,PreparednessContent.Teal,true,TextAnchor.MiddleRight);
            }
            TextLabel(supplies,"Find all "+PreparednessContent.SupplyCount+" useful types. Distractors cost 5 points, not lives.",0,.01f,1,.10f,19,PreparednessContent.Ink);
            Transform safety=instructionTabs[2].transform;
            InstructionCard(safety,"FLOODWATER & FALLEN WIRES","Stay away. Change lanes in the game.\nIn real life, tell an adult and use a safe route.",.68f,.97f);
            InstructionCard(safety,"PREPAREDNESS STARTS AT HOME","Pack a go-bag with an adult. Keep exits clear.\nAsk an adult to secure heavy furniture.",.36f,.65f);
            InstructionCard(safety,"FOLLOW OFFICIAL INSTRUCTIONS","Evacuate with an adult when instructed.\nThe blue cable is a game-only duck obstacle.\nStay away from real electrical wires.",.035f,.33f);
            SelectInstructionTab(0);
        }

        private void InstructionCard(Transform parent,string title,string body,float bottom,float top)
        {
            RectTransform card=Card(title,new Vector2(0,bottom),new Vector2(1,top),new Color32(225,238,229,255),parent);
            TextLabel(card,title,.055f,.65f,.945f,.94f,23,PreparednessContent.Teal,true,TextAnchor.MiddleLeft);
            TextLabel(card,body,.055f,.08f,.945f,.66f,24,PreparednessContent.Ink,false,TextAnchor.MiddleLeft);
        }

        private void BuildAbout()
        {
            RectTransform card=Chapter(MenuPage.About,"SMALL CHOICES. STRONGER COMMUNITIES.","ABOUT THE GAME","A coastal adventure inspired by Lapu-Lapu City, Cebu.");
            if(illustratedPages[(int)MenuPage.About]) return;
            TextLabel(card,game.Information.theme,.06f,.86f,.94f,.965f,32,PreparednessContent.Teal,true,TextAnchor.MiddleLeft);
            TextLabel(card,game.Information.description,.06f,.60f,.94f,.855f,26,PreparednessContent.Ink,false,TextAnchor.MiddleLeft);
            RectTransform learning=Card("What you take home",new Vector2(.055f,.34f),new Vector2(.945f,.57f),new Color32(225,238,229,255),card);
            TextLabel(learning,"PLAY. PREPARE. SHARE.",.05f,.66f,.95f,.94f,24,PreparednessContent.Teal,true,TextAnchor.MiddleLeft);
            TextLabel(learning,"Created for the International Day for Disaster Risk Reduction.\nEvery run ends with a practical action to do with an adult.",.05f,.06f,.95f,.66f,24,PreparednessContent.Ink,false,TextAnchor.MiddleLeft);
            TextLabel(card,"The setting and shield riding are fictional. Your game score is for fun; it does not measure real emergency readiness. Follow official local instructions in a real emergency.",.06f,.15f,.94f,.32f,20,PreparednessContent.Ink,false,TextAnchor.MiddleLeft);
            ButtonAt(card,"MEET THE TEAM  /  CREDITS",.06f,.045f,.94f,.12f,PreparednessContent.Teal,()=>ShowCredits(MenuPage.About),24);
        }

        private void BuildCredits()
        {
            RectTransform card=Chapter(MenuPage.Credits,"THE PEOPLE BEHIND THE ADVENTURE","CREDITS","Together for safer homes and a more resilient community.");
            if(illustratedPages[(int)MenuPage.Credits]) return;
            RectTransform scrollRoot=Rect(card,"Credits scroll",new Vector2(.06f,.075f),new Vector2(.94f,.96f));
            Image scrollHit=scrollRoot.gameObject.AddComponent<Image>(); scrollHit.color=Color.clear; scrollHit.raycastTarget=true;
            ScrollRect scroll=scrollRoot.gameObject.AddComponent<ScrollRect>(); scroll.horizontal=false; scroll.movementType=ScrollRect.MovementType.Clamped;
            RectTransform viewport=Rect(scrollRoot,"Viewport",Vector2.zero,Vector2.one); viewport.gameObject.AddComponent<RectMask2D>();
            RectTransform content=Rect(viewport,"Credit sections",new Vector2(0,1),Vector2.one); content.pivot=new Vector2(.5f,1);
            VerticalLayoutGroup layout=content.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing=18; layout.childControlWidth=true; layout.childControlHeight=true; layout.childForceExpandHeight=false;
            ContentSizeFitter fitter=content.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport=viewport; scroll.content=content;
            if(game.Information.credits!=null) foreach(CreditSection section in game.Information.credits)
            {
                AddCreditLine(content,section.heading,20,PreparednessContent.Teal,true);
                AddCreditLine(content,section.names,26,PreparednessContent.Ink,false);
            }
            TextLabel(card,"Scroll to see all credits",.06f,.015f,.94f,.06f,16,PreparednessContent.Teal);
        }

        private void AddCreditLine(Transform parent,string text,int size,Color color,bool bold)
        {
            Text line=TextLabel(parent,text,0,0,1,1,size,color,bold,TextAnchor.UpperLeft);
            line.resizeTextForBestFit=false; line.verticalOverflow=VerticalWrapMode.Overflow;
            // Text is a layout element; preferred height automatically includes wrapped organization names.
        }

        private void BuildLeaderboard()
        {
            RectTransform card=Chapter(MenuPage.Leaderboard,"LOCAL CHAMPIONS. SAFER COMMUNITIES.","LEADERBOARD","Your top 10 runs, saved on this device.");
            TextLabel(card,"RIDER",.15f,.90f,.65f,.968f,19,PreparednessContent.Teal,true,TextAnchor.MiddleLeft);
            TextLabel(card,"SCORE",.69f,.90f,.94f,.968f,19,PreparednessContent.Teal,true,TextAnchor.MiddleRight);
            for(int i=0;i<LeaderboardStore.Capacity;i++)
            {
                float top=.886f-i*.076f;
                RectTransform row=Card("Rank "+(i+1),new Vector2(.04f,top-.069f),new Vector2(.96f,top),Color.white,card);
                rankRows[i]=row.GetComponent<Image>();
                TextLabel(row,(i+1).ToString("00"),.02f,.06f,.13f,.94f,24,PreparednessContent.Teal,true);
                rankNames[i]=TextLabel(row,"",.16f,.39f,.68f,.98f,24,PreparednessContent.Ink,true,TextAnchor.MiddleLeft);
                rankDetails[i]=TextLabel(row,"",.16f,.035f,.69f,.44f,16,PreparednessContent.Ink,false,TextAnchor.MiddleLeft);
                rankScores[i]=TextLabel(row,"",.71f,.05f,.95f,.95f,27,PreparednessContent.Teal,true,TextAnchor.MiddleRight);
            }
            emptyLeaderboard=TextLabel(card,"YOUR FIRST ADVENTURE\nSTARTS THE STORY.\n\nFinish a run to save your score here.",.09f,.28f,.91f,.75f,30,PreparednessContent.Ink,true);
            TextLabel(card,"Higher score wins. Supplies, then distance, break ties.",.04f,.03f,.96f,.097f,17,PreparednessContent.Ink);
        }

        public void ShowSplash() { splashTime=0; SetPage(MenuPage.Splash); }
        private void TickSplash()
        {
            if(CurrentPage!=MenuPage.Splash) return;
            splashTime+=Time.unscaledDeltaTime;
            if(splashProgress!=null) splashProgress.fillAmount=Mathf.Clamp01(splashTime/SplashDuration);
            if(splashTime>=SplashDuration) game.ReturnToMenu();
        }
        public void ShowInstructions() { SetPage(MenuPage.Instructions); SelectInstructionTab(0); }
        public void ShowAbout() { aboutBack=CurrentPage==MenuPage.Results ? MenuPage.Results : MenuPage.Home; SetPage(MenuPage.About); }
        public void ShowCredits(MenuPage returnTo=MenuPage.About) { creditsBack=returnTo; SetPage(MenuPage.Credits); }
        public void SelectInstructionTab(int tab)
        {
            for(int i=0;i<instructionTabs.Length;i++)
            {
                if(instructionTabs[i]==null || instructionButtons[i]==null) continue;
                instructionTabs[i].SetActive(i==tab); instructionButtons[i].color=i==tab ? PreparednessContent.Teal : PreparednessContent.Ink;
            }
        }
        public void ShowLeaderboard()
        {
            leaderboardBack=CurrentPage==MenuPage.Results ? MenuPage.Results : MenuPage.Home;
            SetPage(MenuPage.Leaderboard); RefreshLeaderboard();
        }
        private void RefreshLeaderboard()
        {
            var entries=game.Leaderboard.Entries; emptyLeaderboard.gameObject.SetActive(entries.Count==0);
            for(int i=0;i<rankRows.Length;i++)
            {
                rankRows[i].gameObject.SetActive(i<entries.Count);
                if(i>=entries.Count) continue;
                LeaderboardEntry entry=entries[i]; bool latest=entry.runId==game.RunId;
                rankRows[i].color=latest ? new Color32(208,238,221,255) : i==0 ? new Color32(255,230,178,255) : i%2==0 ? new Color32(236,240,232,255) : White;
                rankNames[i].text=entry.nickname; rankScores[i].text=entry.score.ToString();
                rankDetails[i].text=$"{entry.supplies} / {PreparednessContent.SupplyCount} supplies  -  {entry.distance} m";
            }
        }
        public void HandleBack()
        {
            switch(CurrentPage)
            {
                case MenuPage.Splash: game.ReturnToMenu(); break;
                case MenuPage.Game:
                case MenuPage.Pause: game.TogglePause(); break;
                case MenuPage.Leaderboard: SetPage(leaderboardBack); break;
                case MenuPage.Credits: SetPage(creditsBack); break;
                case MenuPage.About: SetPage(aboutBack); break;
                case MenuPage.Results: game.ReturnToMenu(); break;
                default: SetPage(MenuPage.Home); break;
            }
        }
    }
}
