using System;
using System.IO;
using AndamRunner;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AndamRunnerEditor
{
    [InitializeOnLoad]
    public static class JumpChecks
    {
        private const string Pending="Andam.JumpChecks";
        private static int assertions;
        private static readonly float[] Frames={1f/15,1f/30,1f/60,1f/120};
        static JumpChecks() { EditorApplication.playModeStateChanged+=OnPlayMode; }

        [MenuItem("ANDAM/Check jumps, floods and screen fitting")]
        public static void Begin()
        {
            PrototypeTools.PrepareProject();
            SessionState.SetBool(Pending,true);
            EditorApplication.isPlaying=true;
        }

        private static void OnPlayMode(PlayModeStateChange change)
        {
            if(change!=PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending,false)) return;
            SessionState.SetBool(Pending,false);
            try
            {
                GameManager game=UnityEngine.Object.FindAnyObjectByType<GameManager>();
                game.enabled=false; Run(game);
                if(Application.isBatchMode) EditorApplication.Exit(0);
                else { game.enabled=true; game.StartRun(); }
            }
            catch(Exception error)
            {
                Debug.LogException(error);
                if(Application.isBatchMode) EditorApplication.Exit(1); else EditorApplication.isPlaying=false;
            }
        }

        public static void Run(GameManager game)
        {
            assertions=0;
            game.UseTestLeaderboard(new LeaderboardStore(null,false));
            for(int kind=0;kind<4;kind++)
                for(int lane=0;lane<3;lane++)
                    for(int level=0;level<=6;level++)
                        foreach(float frame in Frames)
                        {
                            Ready(game,lane,level);
                            float speed=game.Speed;
                            TrackEntity hazard=game.Spawner.SpawnHazard((HazardKind)kind,lane,speed*PlayerRunner.JumpDuration*.5f);
                            Check(hazard!=null,"ground hazard available");
                            PressJump(game);
                            SimulateFrames(game,PlayerRunner.JumpDuration+.18f,frame);
                            Check(game.Session.Lives==3 && game.GraceRemaining==0 && game.Speed==speed,
                                $"{(HazardKind)kind} cleared in lane {lane}, speed {speed}, frame {frame}");
                            Check(!game.Player.IsJumping && game.Player.Height==0,
                                "landing behind a cleared obstacle does not cause a hit");
                        }
            for(int kind=0;kind<4;kind++)
            {
                Ready(game,1,1);
                game.Session.Collect(SupplyKind.GoBag);
                game.Spawner.SpawnHazard((HazardKind)kind,1,.1f);
                game.Jump(); game.Simulate(.1f);
                Check(game.Session.Lives==2 && game.Session.Score==10 && game.Speed==RunPace.BaseSpeed,
                    "a jump started inside an obstacle is too late and applies one normal penalty");
                game.Simulate(.1f); Check(game.Session.Lives==2,"same obstacle cannot hit twice");
                Ready(game,1,0);
                game.Spawner.SpawnHazard((HazardKind)kind,1,game.Speed*(PlayerRunner.JumpDuration+.2f));
                game.Jump(); SimulateFrames(game,1.35f,1f/30);
                Check(game.Session.Lives==2,"jumping too early and landing before the obstacle still hits");
            }
            CheckSweeps(game);
            CheckCables(game);
            CheckLongFloods(game);
            CheckArtworkFrames();
            Directory.CreateDirectory("Docs");
            File.WriteAllText("Docs/Jump-Validation.txt",
                "Unity "+Application.unityVersion+" checks passed on "+DateTime.UtcNow.ToString("u")+"\n"+
                assertions+" assertions; isolated scores, no real leaderboard changes.\n"+
                "336 timed ground-obstacle jumps: four regular hazards, all lanes, all seven speeds, 15/30/60/120 fps.\n"+
                "Early/late jumps, landing, swept lane crossings, single penalties and speed resets checked.\n"+
                "Full-width hanging cables require ducking in every lane at base and maximum speed.\n"+
                "32-meter flood streaks cannot be jumped at base or maximum speed; lane changes clear them.\n"+
                "Natural spawning over 3,120 meters keeps flood lanes free of other objects and preserves dry supply routes.\n"+
                "Seven page images fit short/tall phones, notched safe areas and tablets, including resize/re-enable checks.\n"+
                "Physical-device gestures and appearance require a separate phone check.\n");
            Debug.Log("ANDAM_JUMP_PASS: "+assertions+" jump, long flood, duck and safe artwork assertions.");
            game.StartRun();
        }

        private static void Ready(GameManager game,int lane,int level)
        {
            game.StartRun(); game.Spawner.ClearForChecks(); game.Player.ChangeLane(lane-1); game.Player.Tick(.2f,false);
            game.Pace.Advance(level*RunPace.Milestone);
        }
        private static void PressJump(GameManager game)
        {
            foreach(Button button in game.UI.GetComponentsInChildren<Button>())
                if(button.name=="JUMP") { button.onClick.Invoke(); Check(game.Player.IsJumping,"JUMP button starts the arc"); return; }
            Check(false,"active JUMP button exists");
        }
        private static void SimulateFrames(GameManager game,float duration,float frame)
        {
            while(duration>0)
            {
                float step=Mathf.Min(duration,frame); duration=Mathf.Max(0,duration-step);
                game.Simulate(step);
            }
        }
        private static void CheckSweeps(GameManager game)
        {
            Ready(game,1,0);
            for(int kind=0;kind<4;kind++)
            {
                TrackEntity hazard=game.Spawner.SpawnHazard((HazardKind)kind,1,1);
                Check(hazard.IsContact(Vector3.zero,Vector3.zero,6,-6),"ground collision cannot tunnel during a slow frame");
                Check(!hazard.IsContact(Vector3.up*2.3f,Vector3.up*2.3f,6,-6),"swept jump clears ground hazards");
                Check(!hazard.IsContact(Vector3.zero,Vector3.up*2.3f,6,0),"height before entering footprint does not cause a false hit");
                Check(hazard.IsContact(Vector3.zero,Vector3.up*2.3f,0,-6),"low height while inside footprint still collides");
                Check(!hazard.IsContact(new Vector3(3,0,0),new Vector3(2.6f,0,0),6,-6),"adjacent lane stays clear");
                hazard.Consume(); Check(!hazard.IsContact(Vector3.zero,Vector3.zero,1,-1),"consumed hazard cannot collide twice");
            }
            TrackEntity barrier=game.Spawner.SpawnHazard(HazardKind.RoadBlock,1,1);
            Check(barrier.IsContact(new Vector3(3,0,0),Vector3.zero,3,-3),"diagonal entry into obstacle footprint is swept");
            Ready(game,1,0);
            game.Spawner.SpawnHazard(HazardKind.Floodwater,1,game.Speed*.16f); game.Jump(); game.Simulate(.2f);
            Check(game.Session.Lives==2,"short flood checks its leading edge, not only its center");
            Ready(game,1,0);
            game.Spawner.SpawnHazard(HazardKind.Floodwater,1,game.Speed*.8f); game.Jump(); game.Simulate(1);
            Check(game.Session.Lives==2,"landing inside the short flood still collides");
        }
        private static void CheckCables(GameManager game)
        {
            for(int lane=0;lane<3;lane++)
                foreach(int level in new[]{0,6})
                    foreach(float frame in Frames)
                    {
                        Ready(game,lane,level);
                        game.Spawner.SpawnHazard(HazardKind.HangingCable,1,game.Speed*.46f); game.Duck();
                        SimulateFrames(game,1.1f,frame);
                        Check(game.Session.Lives==3,"duck clears hanging cable in each lane and frame rate");
                        Ready(game,lane,level);
                        game.Spawner.SpawnHazard(HazardKind.HangingCable,1,game.Speed*.46f); game.Jump();
                        SimulateFrames(game,1.1f,frame);
                        Check(game.Session.Lives==2,"jump cannot clear hanging cable; one entity causes one life loss");
                    }
        }
        private static void CheckLongFloods(GameManager game)
        {
            float fastest=Mathf.Min(RunPace.MaximumSpeed,RunPace.BaseSpeed+6*RunPace.SpeedStep);
            float waterClearanceTime=PlayerRunner.JumpDuration*(1-2*Mathf.Asin(.75f/PlayerRunner.JumpHeight)/Mathf.PI);
            Check(PreparednessContent.FloodStreakLength>fastest*waterClearanceTime,
                "long flood exceeds the distance a jump can clear above the water");
            foreach(int level in new[]{0,6})
                for(int lane=0;lane<3;lane++)
                {
                    Ready(game,lane,level);
                    game.Spawner.SpawnHazard(HazardKind.FloodStreak,lane,16+game.Speed*.2f); game.Jump();
                    SimulateFrames(game,1.3f,1f/30);
                    Check(game.Session.Lives==2,"a timed jump must land inside the long flood");
                    Ready(game,lane,level);
                    game.Spawner.SpawnHazard(HazardKind.FloodStreak,lane,16+game.Speed*.3f);
                    game.Move(lane==2?-1:1); SimulateFrames(game,1.2f,1f/30);
                    Check(game.Session.Lives==3,"lane change clears the long flood");
                }
            Ready(game,1,0);
            game.Spawner.ResetTrack();
            game.Player.transform.position=new Vector3(100,0,0);
            bool seen=false;
            for(int step=0;step<1200;step++)
            {
                game.Spawner.Tick(.1f,26,game.Player,game,game.Player.transform.position);
                Check(game.Spawner.ActiveCount<=SpawnManager.MaximumActiveEntities,"long-flood spawning remains bounded");
                foreach(TrackEntity flood in game.Spawner.Active)
                {
                    if(flood.IsPickup || flood.Hazard!=HazardKind.FloodStreak || flood.Consumed) continue;
                    seen=true;
                    foreach(TrackEntity other in game.Spawner.Active)
                        if(other!=flood && !other.Consumed && other.Lane==flood.Lane)
                            Check(Mathf.Abs(other.transform.position.z-flood.transform.position.z)>flood.HalfDepth+other.HalfDepth,
                                "flood footprint has no overlapping supplies, distractors or hazards");
                }
                foreach(TrackEntity supply in game.Spawner.Active)
                    if(supply.IsSupply && !supply.IsJumpReward && !supply.Consumed)
                        foreach(TrackEntity hazard in game.Spawner.Active)
                            if(!hazard.IsPickup && !hazard.SpansAllLanes && !hazard.Consumed && hazard.Lane==supply.Lane)
                                Check(Mathf.Abs(hazard.transform.position.z-supply.transform.position.z)>hazard.HalfDepth+supply.HalfDepth,
                                    "every useful-supply row retains a dry route");
            }
            Check(seen,"long floods occur during natural spawning");
            Ready(game,1,0);
            TrackEntity tail=game.Spawner.SpawnHazard(HazardKind.FloodStreak,0,-9);
            game.Simulate(.1f); Check(!tail.Consumed && tail.gameObject.activeSelf,"long flood stays visible until its trailing edge passes");
        }
        private static void CheckArtworkFrames()
        {
            var bounds=new GameObject("Safe artwork layout check",typeof(RectTransform));
            RectTransform parent=(RectTransform)bounds.transform;
            try
            {
                foreach(string name in new[]{"SplashscreenBackground","MenuBackground","CreditsBackground","HowToBackground","AboutBackground","LeaderboardBackground","ScoreBackground"})
                {
                    Texture2D image=Resources.Load<Texture2D>(name); Check(image!=null,"page artwork exists: "+name);
                    var child=new GameObject(name,typeof(RectTransform)); child.transform.SetParent(parent,false);
                    var fit=child.AddComponent<SafeArtworkFrame>(); fit.Initialize((float)image.width/image.height);
                    RectTransform rect=(RectTransform)child.transform;
                    foreach(Vector2 size in new[]{new Vector2(720,1080),new Vector2(720,1600),new Vector2(390,763),new Vector2(430,839),new Vector2(768,1024)})
                    {
                        parent.sizeDelta=size; fit.FitNow();
                        Check(rect.rect.width<=size.x+.01f && rect.rect.height<=size.y+.01f,"complete image stays inside safe bounds");
                        Check(Mathf.Abs(rect.rect.width/rect.rect.height-(float)image.width/image.height)<.0001f,"page aspect is preserved");
                        Check(rect.anchoredPosition==Vector2.zero,"image stays centered");
                        child.SetActive(false); parent.sizeDelta=size*.8f; child.SetActive(true); fit.FitNow();
                        Check(rect.rect.width<=parent.rect.width+.01f && rect.rect.height<=parent.rect.height+.01f,"inactive page refits on return");
                    }
                    UnityEngine.Object.DestroyImmediate(child);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(bounds); }
        }
        private static void Check(bool condition,string message)
        { assertions++; if(!condition) throw new Exception("Jump check failed: "+message); }
    }
}
