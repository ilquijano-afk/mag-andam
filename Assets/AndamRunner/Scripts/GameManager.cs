using UnityEngine;

namespace AndamRunner
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameManager : MonoBehaviour
    {
        public RunState State { get; private set; } = RunState.Splash;
        public RunSession Session { get; } = new RunSession();
        public PlayerRunner Player { get; private set; }
        public SpawnManager Spawner { get; private set; }
        public RepeatingRoad Road { get; private set; }
        public UIManager UI { get; private set; }
        public LeaderboardStore Leaderboard { get; private set; }
        public GameInformation Information { get; private set; }
        public int LastRank { get; private set; }
        public string RunId { get; private set; }
        public string RiderName { get; private set; } = "OPONG";
        public RunPace Pace { get; }=new RunPace();
        public RunPowerUps PowerUps { get; }=new RunPowerUps();
        public float Distance => Pace.Distance;
        public float Speed => Pace.Speed;
        public float GraceRemaining { get; private set; }
        private SwipeInput input;
        private CameraFollow follow;

        private void Awake()
        {
            Application.targetFrameRate=60;
            Screen.sleepTimeout=SleepTimeout.NeverSleep;
            Information = GameInformation.Load();
            Leaderboard = new LeaderboardStore(System.IO.Path.Combine(Application.persistentDataPath,"andam-leaderboard-v1.json"));
            RiderName = Leaderboard.LastNickname;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.40f,.48f,.53f);
            RenderSettings.ambientEquatorColor = new Color(.36f,.38f,.38f);
            RenderSettings.ambientGroundColor = new Color(.34f,.29f,.26f);
            RenderSettings.skybox = Resources.Load<Material>("CoastalSky");
            RenderSettings.fog=true; RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color32(167,220,229,255); RenderSettings.fogStartDistance=44; RenderSettings.fogEndDistance=100;
            Player=NewChild("Player - Opong",transform).AddComponent<PlayerRunner>(); Player.Initialize();
            Road=NewChild("Repeating coastal road",transform).AddComponent<RepeatingRoad>(); Road.Initialize();
            Spawner=NewChild("Pooled supplies and hazards",transform).AddComponent<SpawnManager>(); Spawner.Initialize(this);
            Camera camera=Camera.main;
            if(camera==null) { camera=NewChild("Main Camera",transform).AddComponent<Camera>(); camera.tag="MainCamera"; camera.gameObject.AddComponent<AudioListener>(); }
            camera.clearFlags=RenderSettings.skybox!=null ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor; camera.backgroundColor=RenderSettings.fogColor;
            camera.fieldOfView=62; camera.nearClipPlane=.15f; camera.farClipPlane=120;
            camera.gameObject.AddComponent<CoastalIllustrationBackground>();
            follow=camera.gameObject.AddComponent<CameraFollow>(); follow.Initialize(Player);
            if(FindAnyObjectByType<Light>()==null)
            {
                Light sun=NewChild("Tropical sunlight",transform).AddComponent<Light>();
                sun.type=LightType.Directional; sun.color=new Color(1,.95f,.83f); sun.intensity=1.1f;
                sun.transform.rotation=Quaternion.Euler(45,-35,0);
            }
            Light sunlight=FindAnyObjectByType<Light>(); sunlight.shadows=LightShadows.Soft; sunlight.shadowStrength=.4f; sunlight.intensity=1.05f;
            QualitySettings.shadows=ShadowQuality.All; QualitySettings.shadowDistance=28; QualitySettings.shadowCascades=0;
            QualitySettings.shadowResolution=ShadowResolution.Medium; QualitySettings.antiAliasing=2;
            follow.SetPresentation(true);
            input=gameObject.AddComponent<SwipeInput>(); input.Initialize(this);
            UI=NewChild("Phone interface",transform).AddComponent<UIManager>(); UI.Initialize(this);
            UI.ShowSplash(); UI.Refresh();
        }

        private static GameObject NewChild(string name,Transform parent)
        {
            GameObject obj=new GameObject(name); obj.transform.SetParent(parent,false); return obj;
        }

        private void Update()
        {
            if(State != RunState.Playing) return;
            Simulate(Mathf.Min(Time.deltaTime,.10f));
        }

        public void Simulate(float delta)
        {
            if(State != RunState.Playing || delta <= 0f) return;
            // Substep contacts and motion after a slow frame; distance, spawning and jump stay consistent.
            float remaining=delta;
            while(remaining>0f && State==RunState.Playing)
            {
                float step=Mathf.Min(remaining,1f/60f); remaining-=step;
                GraceRemaining=Mathf.Max(0f,GraceRemaining-step);
                PowerUps.Tick(step);
                Vector3 before=Player.transform.position;
                float duckBefore=Player.DuckRemaining;
                Player.Tick(step,GraceRemaining>0f);
                float travel=Speed*step; Pace.Advance(travel); Road.Tick(travel);
                Spawner.Tick(step,travel/step,Player,this,before,duckBefore);
            }
            UI.RefreshDistance();
            UI.RefreshPowerUps();
        }

        public void StartRun()
        {
            RiderName = LeaderboardStore.CleanNickname(UI.Nickname);
            RunId=System.Guid.NewGuid().ToString("N"); LastRank=0;
            Session.Reset(); Pace.Restart(); PowerUps.Reset(); GraceRemaining=0;
            Player.ResetRunner(); Player.Show(); Spawner.ResetTrack(); Road.ResetRoad(); follow.SetPresentation(false); follow.ResetCamera(); input.ResetGesture();
            State=RunState.Playing; UI.ShowPlay(); UI.Refresh();
            UI.ClearFeedback();
        }

        public void ReturnToMenu()
        {
            if(State==RunState.Playing || State==RunState.Paused) FinishRun();
            State=RunState.Ready;
            Player.ResetRunner(); Player.Show(); Road.ResetRoad(); Spawner.ResetTrack();
            input.ResetGesture(); follow.SetPresentation(true); UI.ShowReady();
        }

        #if UNITY_EDITOR
        // Automated checks inject isolated storage; real player scores remain untouched.
        public void UseTestLeaderboard(LeaderboardStore store) { Leaderboard=store; }
        #endif

        public void Move(int direction) { if(State==RunState.Playing) Player.ChangeLane(direction); }
        public void Jump() { if(State==RunState.Playing) Player.Jump(); }
        public void Duck() { if(State==RunState.Playing) Player.Duck(); }

        public void Collect(SupplyKind kind)
        {
            if(State!=RunState.Playing) return;
            Session.Collect(kind,PowerUps.ScoreMultiplier); UI.Refresh(); Player.Visual.GetComponent<OpongAnimation>().ReactToPickup();
        }

        public void CollectPowerUp(PowerUpKind kind)
        {
            if(State!=RunState.Playing || Distance<RunPowerUps.UnlockDistance) return;
            PowerUps.Activate(kind); UI.RefreshPowerUps();
            Player.Visual.GetComponent<OpongAnimation>().ReactToPickup();
        }

        public void Hit(HazardKind kind)
        {
            if(State!=RunState.Playing) return;
            Pace.ResetSpeed(); UI.RefreshDistance();
            if(GraceRemaining>0) return;
            Session.Hit(); GraceRemaining=1.3f; UI.Refresh(); Player.Visual.GetComponent<OpongAnimation>().ReactToHit();
            Feedback("-10  "+PreparednessContent.HazardTips[(int)kind],false);
            if(Session.IsOver) FinishRun();
        }
        public void CollectDistractor(DistractorKind kind)
        {
            if(State!=RunState.Playing) return;
            Session.CollectDistractor(kind); UI.Refresh(); Player.Visual.GetComponent<OpongAnimation>().ReactToDistractor();
        }
        public void Feedback(string message,bool positive) { UI.Feedback(message,positive); }

        public void FinishRun()
        {
            if(State!=RunState.Playing && State!=RunState.Paused) return;
            LastRank=Leaderboard.Record(RunId,RiderName,Session.Score,Session.UniqueSupplies,Mathf.FloorToInt(Distance));
            State=RunState.Results; Player.Show(); input.ResetGesture(); UI.ShowResults();
        }

        public void TogglePause()
        {
            if(State==RunState.Playing) { State=RunState.Paused; Player.Show(); input.ResetGesture(); UI.ShowPause(); }
            else if(State==RunState.Paused) { State=RunState.Playing; input.ResetGesture(); UI.ShowPlay(); }
        }
        private void OnApplicationPause(bool paused) { if(paused && State==RunState.Playing) TogglePause(); }
        private void OnApplicationFocus(bool focused) { if(!focused && State==RunState.Playing) TogglePause(); }
    }
}
