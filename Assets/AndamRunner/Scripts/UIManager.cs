using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AndamRunner
{
    public sealed partial class UIManager : MonoBehaviour
    {
        private GameManager game;
        private RectTransform safe;
        private GameObject hud, results, pause;
        private readonly GameObject[] pages = new GameObject[9];
        public MenuPage CurrentPage { get; private set; }
        private InputField nickname;
        public string Nickname => nickname != null ? nickname.text : "OPONG";
        private Text score, lives, distance, speed, feedback, resultsScore, resultsSummary, homeAction;
        private Image feedbackBackground;
        private float feedbackUntil;
        private int lastDistance=-1;
        private int lastSpeedLevel=-1;
        private Rect lastSafeArea;
        private int lastWidth, lastHeight;
        private Font font;
        private StorybookPortrait portrait;
        public void RenderPortrait() { if(portrait!=null && portrait.isActiveAndEnabled) portrait.Render(); }
        private static readonly Color White = Color.white;
        private static readonly Color Panel = new Color32(255,250,239,248);
        private static readonly Color DarkPanel = new Color32(13,47,68,240);

        public void Initialize(GameManager manager)
        {
            game=manager; font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Canvas canvas=gameObject.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=10;
            CanvasScaler scaler=gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(720,1280); scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if(EventSystem.current==null)
            {
                GameObject events=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule)); events.transform.SetParent(transform,false);
            }
            safe=Rect(transform,"Phone safe area",Vector2.zero,Vector2.one);
            BuildHud(); BuildResults(); BuildPause(); BuildMenuPages(); ApplySafeArea();
        }

        private void Update()
        {
            if(lastSafeArea!=Screen.safeArea || lastWidth!=Screen.width || lastHeight!=Screen.height) ApplySafeArea();
            if(feedbackBackground.gameObject.activeSelf && Time.unscaledTime>feedbackUntil) feedbackBackground.gameObject.SetActive(false);
            TickSplash();
        }

        private void ApplySafeArea()
        {
            lastSafeArea=Screen.safeArea; lastWidth=Screen.width; lastHeight=Screen.height;
            if(Screen.width<=0 || Screen.height<=0) return;
            safe.anchorMin=new Vector2(lastSafeArea.x/Screen.width,lastSafeArea.y/Screen.height);
            safe.anchorMax=new Vector2(lastSafeArea.xMax/Screen.width,lastSafeArea.yMax/Screen.height);
            safe.offsetMin=safe.offsetMax=Vector2.zero;
            Canvas.ForceUpdateCanvases();
            foreach(SafeArtworkFrame frame in GetComponentsInChildren<SafeArtworkFrame>(true)) frame.FitNow();
        }

        private void BuildHud()
        {
            hud=Rect(safe,"Run HUD",Vector2.zero,Vector2.one).gameObject;
            Register(MenuPage.Game,hud);
            RectTransform top=Card("Score strip",new Vector2(.03f,.89f),new Vector2(.97f,.98f),DarkPanel,hud.transform);
            TextLabel(top,"RESILIENCE SCORE",.04f,.58f,.43f,.94f,17,PreparednessContent.Mint,true,TextAnchor.MiddleLeft);
            score=TextLabel(top,"0",.04f,.06f,.43f,.62f,38,White,true,TextAnchor.MiddleLeft);
            lives=TextLabel(top,"LIVES  3 / 3",.45f,.51f,.77f,.96f,22,White,true);
            distance=TextLabel(top,"0 m",.45f,.23f,.77f,.51f,22,PreparednessContent.Mint);
            speed=TextLabel(top,"SPEED  "+RunPace.BaseSpeed+" m/s",.45f,.005f,.77f,.23f,15,White);
            ButtonAt(top,"II",.82f,.13f,.97f,.86f,PreparednessContent.Teal,game.TogglePause,25);
            RectTransform message=Card("Learning feedback",new Vector2(.035f,.135f),new Vector2(.965f,.25f),Panel,hud.transform);
            feedbackBackground=message.GetComponent<Image>();
            feedback=TextLabel(message,"",.04f,.06f,.96f,.94f,24,PreparednessContent.Ink,true);
            ClearFeedback();
            ButtonAt(hud.transform,"< LEFT",.035f,.035f,.255f,.115f,DarkPanel,()=>game.Move(-1),23);
            ButtonAt(hud.transform,"JUMP",.272f,.035f,.492f,.115f,PreparednessContent.Teal,game.Jump,23);
            ButtonAt(hud.transform,"DUCK",.509f,.035f,.729f,.115f,PreparednessContent.Teal,game.Duck,23);
            ButtonAt(hud.transform,"RIGHT >",.746f,.035f,.966f,.115f,DarkPanel,()=>game.Move(1),23);
            RectTransform route=Card("Evacuation route",new Vector2(.10f,.845f),new Vector2(.90f,.877f),DarkPanel,hud.transform);
            TextLabel(route,"TO THE EVACUATION CENTER",.025f,.02f,.975f,.98f,16,White,true);
        }

        private void BuildResults()
        {
            RectTransform layer=PageLayer(MenuPage.Results,false);
            results=pages[(int)MenuPage.Results];
            if(illustratedPages[(int)MenuPage.Results])
            {
                // Center the score in the open space between the heading and yellow summary panel.
                resultsScore=TextLabel(layer,"0",.08f,.585f,.92f,.715f,180,PreparednessContent.Teal,true,TextAnchor.MiddleCenter);
                resultsScore.name="Live score";
                resultsScore.resizeTextMinSize=48;
                //resultsSummary=TextLabel(layer,"",.13f,.447f,.87f,.492f,18,PreparednessContent.Ink);
                resultsSummary=TextLabel(layer,"",.13f,.492f,.87f,.559f,25,PreparednessContent.Ink);
                IllustratedButtonAt(layer,"PLAY AGAIN",.257f,.175f,.747f,.252f,game.StartRun);
                IllustratedButtonAt(layer,"MAIN MENU",.068f,.095f,.493f,.168f,game.ReturnToMenu);
                IllustratedButtonAt(layer,"SCORES",.508f,.095f,.933f,.168f,ShowLeaderboard);
                return;
            }
            RectTransform card=Card("Results",new Vector2(.05f,.07f),new Vector2(.95f,.93f),Panel,layer); results=layer.gameObject;
            TextLabel(card,"ANDAM, OPONG!",.04f,.90f,.96f,.97f,27,PreparednessContent.Teal,true);
            TextLabel(card,"EVERY RUN\nBUILDS READINESS",.04f,.77f,.96f,.90f,42,PreparednessContent.Ink,true);
            resultsScore=TextLabel(card,"0",.06f,.64f,.94f,.77f,68,PreparednessContent.Teal,true);
            TextLabel(card,"RESILIENCE SCORE",.05f,.59f,.95f,.64f,21,PreparednessContent.Ink,true);
            resultsSummary=TextLabel(card,"",.07f,.455f,.93f,.59f,25,PreparednessContent.Ink);
            RectTransform action=Card("Action with an adult",new Vector2(.065f,.22f),new Vector2(.935f,.45f),new Color32(209,237,220,255),card);
            TextLabel(action,"Resilience Starts at Home",.04f,.73f,.96f,.96f,27,PreparednessContent.Teal,true);
            homeAction=TextLabel(action,"",.05f,.06f,.95f,.73f,26,PreparednessContent.Ink,true);
            ButtonAt(card,"RIDE AGAIN",.07f,.12f,.93f,.20f,PreparednessContent.Teal,game.StartRun,30);
            ButtonAt(card,"MAIN MENU",.07f,.045f,.48f,.105f,PreparednessContent.Ink,game.ReturnToMenu,21);
            ButtonAt(card,"SCORES",.52f,.045f,.93f,.105f,PreparednessContent.Ink,ShowLeaderboard,21);
            TextLabel(card,"Learn with an adult. Follow official local guidance.",.05f,.004f,.95f,.037f,16,PreparednessContent.Ink);
        }

        private void BuildPause()
        {
            RectTransform card=Card("Pause menu",new Vector2(.08f,.27f),new Vector2(.92f,.72f),Panel); pause=card.gameObject;
            Register(MenuPage.Pause,pause);
            TextLabel(card,"TAKE A BREATHER",.04f,.77f,.96f,.93f,36,PreparednessContent.Ink,true);
            TextLabel(card,"Follow official evacuation instructions.\nStay with an adult and use the designated route.",.06f,.48f,.94f,.75f,26,PreparednessContent.Ink);
            ButtonAt(card,"KEEP RIDING",.1f,.29f,.9f,.44f,PreparednessContent.Teal,game.TogglePause,27);
            ButtonAt(card,"FINISH & LEARN",.1f,.08f,.9f,.23f,PreparednessContent.Ink,game.FinishRun,26);
        }

        private void Register(MenuPage page,GameObject obj) { pages[(int)page]=obj; }
        private void SetPage(MenuPage page)
        {
            CurrentPage=page;
            for(int i=0;i<pages.Length;i++) if(pages[i]!=null) pages[i].SetActive(i==(int)page);
            if(page==MenuPage.Pause) hud.SetActive(true);
        }
        public void ShowReady() { SetPage(MenuPage.Home); nickname.SetTextWithoutNotify(game.RiderName); }
        public void ShowPlay() { SetPage(MenuPage.Game); }
        public void ShowPause() { SetPage(MenuPage.Pause); }
        public void ShowResults()
        {
            SetPage(MenuPage.Results);
            resultsScore.text=game.Session.Score.ToString();
            string rank=game.LastRank>0 ? $"#{game.LastRank} on this device" : "Keep riding for a place in the top 10!";
            if(!game.Leaderboard.LastSaveSucceeded) rank="Score could not be saved on this device.";
            resultsSummary.text=illustratedPages[(int)MenuPage.Results]
                ? $"{game.RiderName}  -  {Mathf.FloorToInt(game.Distance)} m  -  {game.Session.UniqueSupplies}/{PreparednessContent.SupplyCount} supplies\n{rank}  -  Distractors: {game.Session.DistractorsCollected}"
                : $"{Mathf.FloorToInt(game.Distance)} m  /  {game.Session.Collected} supplies  /  {game.Session.UniqueSupplies} of {PreparednessContent.SupplyCount} types\n{game.RiderName}  -  {rank}";
            int actionIndex=(game.Session.Collected+Mathf.FloorToInt(game.Distance/40f))%PreparednessContent.HomeActions.Length;
            if(homeAction!=null) homeAction.text=PreparednessContent.HomeActions[actionIndex];
        }
        public void Refresh()
        {
            score.text=game.Session.Score.ToString(); lives.text=$"LIVES  {game.Session.Lives} / 3";
            lastDistance=-1; RefreshDistance();
        }
        public void RefreshDistance()
        {
            int meters=Mathf.FloorToInt(game.Distance);
            if(meters==lastDistance && lastSpeedLevel==game.Pace.Level) return;
            lastDistance=meters; lastSpeedLevel=game.Pace.Level; distance.text=meters+" m";
            speed.text="SPEED  "+game.Speed.ToString("0.#")+" m/s";
        }
        public void Feedback(string message,bool positive)
        {
            feedback.text=message;
            feedbackBackground.color=positive ? Panel : new Color32(255,225,191,252);
            feedbackBackground.gameObject.SetActive(true); feedbackUntil=Time.unscaledTime+4.5f;
        }

        public void ClearFeedback()
        {
            feedback.text="";
            feedbackBackground.gameObject.SetActive(false);
        }

        private static RectTransform Rect(Transform parent,string name,Vector2 min,Vector2 max)
        {
            GameObject obj=new GameObject(name,typeof(RectTransform)); RectTransform rect=(RectTransform)obj.transform;
            rect.SetParent(parent,false); rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
            return rect;
        }
        private RectTransform Card(string name,Vector2 min,Vector2 max,Color color,Transform parent=null)
        {
            RectTransform rect=Rect(parent!=null?parent:safe,name,min,max);
            Image image=rect.gameObject.AddComponent<Image>(); image.color=color; image.raycastTarget=false;
            UISkin.Round(image);
            return rect;
        }
        private Text TextLabel(Transform parent,string message,float x0,float y0,float x1,float y1,int size,Color color,bool bold=false,TextAnchor alignment=TextAnchor.MiddleCenter)
        {
            RectTransform rect=Rect(parent,message,new Vector2(x0,y0),new Vector2(x1,y1)); Text text=rect.gameObject.AddComponent<Text>();
            text.font=font; text.text=message; text.fontSize=size; text.color=color; text.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
            text.alignment=alignment; text.horizontalOverflow=HorizontalWrapMode.Wrap; text.verticalOverflow=VerticalWrapMode.Truncate;
            text.resizeTextForBestFit=true; text.resizeTextMinSize=Mathf.Max(14,size-5); text.resizeTextMaxSize=size;
            text.raycastTarget=false; return text;
        }
        private Button ButtonAt(Transform parent,string message,float x0,float y0,float x1,float y1,Color color,UnityAction action,int size)
        {
            RectTransform rect=Card(message,new Vector2(x0,y0),new Vector2(x1,y1),color,parent); rect.GetComponent<Image>().raycastTarget=true;
            Button button=rect.gameObject.AddComponent<Button>(); button.targetGraphic=rect.GetComponent<Image>();
            ColorBlock colors=button.colors; colors.highlightedColor=new Color(.90f,.97f,.96f); colors.pressedColor=new Color(.72f,.88f,.85f); button.colors=colors;
            button.onClick.AddListener(action); button.navigation=new Navigation{mode=Navigation.Mode.None};
            TextLabel(rect,message,.025f,.03f,.975f,.97f,size,White,true); return button;
        }
    }
}
