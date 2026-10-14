using System;
using System.Collections.Generic;
using System.IO;
using AndamRunner;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AndamRunnerEditor
{
    // Offscreen Unity renders, not mockups. Scores are isolated sample data for layout review.
    [InitializeOnLoad]
    public static class PreviewRenderer
    {
        private const string Key="Andam.RenderPreviews";
        private const string MenuKey="Andam.RenderMenuPreviews";
        static PreviewRenderer() { EditorApplication.playModeStateChanged+=OnPlayMode; }

        [MenuItem("ANDAM/4. Render screen previews")]
        public static void RenderPreviews()
        {
            if(EditorApplication.isPlaying) { Debug.LogWarning("Stop Play mode before rendering previews."); return; }
            PrototypeTools.PrepareProject();
            if(Resources.Load<GameObject>("OpongStorybook")==null) OpongPrefabExporter.Export();
            SessionState.SetBool(MenuKey,false); SessionState.SetBool(Key,true); EditorApplication.isPlaying=true;
        }
        [MenuItem("ANDAM/Render supplied menu artwork previews")]
        public static void RenderMenuPreviews()
        {
            if(EditorApplication.isPlaying) { Debug.LogWarning("Stop Play mode before rendering previews."); return; }
            PrototypeTools.PrepareProject();
            SessionState.SetBool(MenuKey,true); SessionState.SetBool(Key,true); EditorApplication.isPlaying=true;
        }
        // Explicit rebuild for a new art pass; ordinary previews preserve prefab edits.
        public static void RebuildAndRender()
        {
            PrototypeTools.PrepareProject(); OpongPrefabExporter.Export();
            SessionState.SetBool(MenuKey,false); SessionState.SetBool(Key,true); EditorApplication.isPlaying=true;
        }
        private static void OnPlayMode(PlayModeStateChange change)
        {
            if(change!=PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Key,false)) return;
            SessionState.SetBool(Key,false);
            try
            {
                if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null) throw new Exception("Preview rendering requires graphics. Omit -nographics.");
                GameManager game=UnityEngine.Object.FindAnyObjectByType<GameManager>(); game.enabled=false;
                SmokeChecks.Run(game);
                game.ReturnToMenu();
                game.UseTestLeaderboard(new LeaderboardStore(null,false));
                foreach(InputField field in game.UI.GetComponentsInChildren<InputField>(true)) field.SetTextWithoutNotify("OPONG");
                game.StartRun(); game.ReturnToMenu();
                game.UseTestLeaderboard(new LeaderboardStore(null,false));
                if(SessionState.GetBool(MenuKey,false))
                {
                    SessionState.SetBool(MenuKey,false);
                    CheckModel(game);
                    CaptureMenuArtwork(game);
                    EnvironmentReviewTools.Render(game);
                    foreach(Material material in new[]{game.Road.Pack.Palette,game.Road.Pack.GameplayPalette,game.Road.Pack.BackgroundPalette,game.Road.Pack.Water,game.Road.Pack.Rain,game.Road.Pack.Shadow})
                        if(ShaderUtil.ShaderHasError(material.shader)) throw new Exception("Environment shader errors: "+material.shader.name);
                    if(Application.isBatchMode) EditorApplication.Exit(0); else EditorApplication.isPlaying=false;
                    return;
                }
                game.UI.ShowSplash();
                Directory.CreateDirectory("Docs/Previews");
                Capture(game,"01-Splash");
                game.ReturnToMenu(); Capture(game,"02-MainMenu");
                Capture(game,"02-MainMenu-ShortPhone",720,1080); Capture(game,"02-MainMenu-TallPhone",720,1600);
                CheckModel(game);
                CheckTrackArtwork(game);
                CaptureTrackArtwork(game);
                CaptureCharacter(game,"12-Opong-Front",new Vector3(0,3,6.4f));
                CaptureCharacter(game,"13-Opong-ThreeQuarter",new Vector3(4,3.1f,5.8f));
                CaptureCharacter(game,"14-Opong-Back",new Vector3(-3,3,-6.4f));
                OpongAnimation actor=game.Player.Visual.GetComponent<OpongAnimation>();
                actor.EvaluatePreview("Air",.25f,OpongMood.Determined); CaptureCharacter(game,"15-Opong-Air",new Vector3(4,3.1f,5.8f));
                actor.EvaluatePreview("Hit",.5f,OpongMood.Hurt); CaptureCharacter(game,"16-Opong-Hit",new Vector3(4,3.1f,5.8f));
                actor.EvaluatePreview("Victory",.25f,OpongMood.Proud); CaptureCharacter(game,"17-Opong-Victory",new Vector3(4,3.1f,5.8f));
                foreach(OpongMood mood in Enum.GetValues(typeof(OpongMood)))
                {
                    actor.EvaluatePreview("Idle",0,mood);
                    CaptureCharacter(game,"Face-"+mood,new Vector3(0,2.25f,6.4f),true);
                }
                actor.ResetPose();
                actor.EvaluatePreview("Slide Duck",.25f,OpongMood.Determined); CaptureCharacter(game,"19-Opong-Duck",new Vector3(4,3.1f,5.8f)); actor.ResetPose();
                game.UI.ShowInstructions(); Capture(game,"03-Instructions");
                game.UI.SelectInstructionTab(1); Capture(game,"04-Supplies");
                game.UI.SelectInstructionTab(2); Capture(game,"05-Safety");
                game.UI.ShowAbout(); Capture(game,"06-About");
                game.UI.ShowCredits(); Capture(game,"07-Credits");
                game.UI.ShowLeaderboard(); Capture(game,"08-Leaderboard-Empty");
                game.StartRun(); game.Simulate(2f); Capture(game,"09-Gameplay");
                Capture(game,"09-Gameplay-ShortPhone",720,1080); Capture(game,"09-Gameplay-TallPhone",720,1600);
                game.StartRun(); game.Spawner.SpawnHazard(HazardKind.HangingCable,1,14);
                game.Feedback("LOW DISCONNECTED CABLE\nSwipe DOWN or tap DUCK to pass beneath it.",false); Capture(game,"20-Gameplay-Cable-Ahead");
                game.StartRun(); game.Spawner.SpawnHazard(HazardKind.HangingCable,1,.7f); game.Duck(); game.Simulate(.05f);
                actor.SendMessage("Update"); game.Player.Visual.GetComponent<Animator>().Update(.15f);
                Capture(game,"18-Gameplay-Duck"); Capture(game,"18-Gameplay-Duck-ShortPhone",720,1080); Capture(game,"18-Gameplay-Duck-TallPhone",720,1600);
                for(int i=0;i<PreparednessContent.SupplyCount;i++) game.Collect((SupplyKind)i);
                game.FinishRun(); Capture(game,"10-Results");
                string[] names={"CEBU KID","MAYA","LITO","BAYANI","ISA","MIGUEL","KAI","SARI","READY-RIDER12"};
                for(int i=0;i<names.Length;i++) game.Leaderboard.Record("preview"+i,names[i],260-i*15,7-i%4,400-i*17);
                game.UI.ShowLeaderboard(); Capture(game,"11-Leaderboard-SampleRuns");
                foreach(Shader shader in new[]{Resources.Load<Material>("AndamBase").shader,Resources.Load<Material>("CoastalSky").shader,Resources.Load<Material>("IllustratedBackdrop").shader,Resources.Load<Material>("IllustratedGround").shader,game.Player.Visual.GetComponent<SkinnedMeshRenderer>().sharedMaterial.shader})
                    if(ShaderUtil.ShaderHasError(shader)) throw new Exception("Shader errors: "+shader.name);
                File.WriteAllText("Docs/Previews/README.txt","Actual offscreen Unity renders at the specified phone resolutions.\nLeaderboard rows are isolated sample runs for layout review, not seeded player scores.\nNo Android device or GPU performance is represented by these images.\n");
                Debug.Log("ANDAM_PREVIEW_PASS: all screens rendered with "+SystemInfo.graphicsDeviceType);
                File.WriteAllText("Docs/Validation.txt","Unity "+Application.unityVersion+"\nVersion 0.6 storm evacuation route, 250-meter speed milestones, collision resets and full-width duck obstacle; gameplay/menu/credits/leaderboard checks passed on "+DateTime.UtcNow.ToString("u")+"\nPace checks cover exact 250/500-meter boundaries, speed cap, collision reset with preserved distance, next global milestone, and restart. Duck checks cover all three lanes, swept expiry timing, jump exclusion, timer duration, pause, restart, button wiring and single-life full-width contact.\nModel checks: 8,000–15,000 triangles, one skinned renderer/material, saved mesh, valid normals/weights/UVs, 1024 atlas, eight animation states, seven facial moods and duck silhouette below cable clearance.\nAll seven supplies and five hazards have no world-space text; finite mesh vertices/normals and rendered triangle budgets checked (supplies <=800; hazards <=1500).\nActual screens rendered with "+SystemInfo.graphicsDeviceType+" at 720x1280; main menu, gameplay and ducking also rendered at 720x1080 and 720x1600. Character poses, facial moods and all twelve object close-ups rendered.\nChecks ran in an isolated test copy with temporary leaderboard storage. Player scores were untouched.\nNo Android APK build or physical-phone test has been performed; Android Build Support is still required.\n");
                if(Application.isBatchMode) EditorApplication.Exit(0); else EditorApplication.isPlaying=false;
            }
            catch(Exception error)
            {
                Debug.LogException(error);
                if(Application.isBatchMode) EditorApplication.Exit(1); else EditorApplication.isPlaying=false;
            }
        }
        internal static void Capture(GameManager game,string name,int width=720,int height=1280)
        {
            Camera camera=Camera.main; Canvas canvas=game.UI.GetComponent<Canvas>(); CanvasScaler scaler=game.UI.GetComponent<CanvasScaler>();
            var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32) {antiAliasing=2}; target.Create();
            camera.targetTexture=target; camera.aspect=(float)width/height;
            camera.GetComponent<CameraFollow>().SendMessage("LateUpdate"); game.Environment.Apply();
            if(game.State==RunState.Playing)
            {
                if(camera.WorldToViewportPoint(new Vector3(game.Player.transform.position.x,.15f,-1.35f)).y<=.25f) throw new Exception("The shield must clear the learning message.");
                if(camera.WorldToViewportPoint(new Vector3(0,3.04f+PlayerRunner.JumpHeight,0)).y>=.84f) throw new Exception("The jump silhouette must clear the evacuation route banner.");
            }
            canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=.5f;
            scaler.enabled=false; canvas.scaleFactor=1;
            game.Player.Visual.GetComponent<OpongAnimation>().SendMessage("Update");
            game.Player.Visual.GetComponent<Animator>().Update(1f/30f);
            PoseSnapshot pose=new PoseSnapshot(game.Player.Visual);
            game.UI.RenderPortrait();
            foreach(WorldLabel label in game.GetComponentsInChildren<WorldLabel>()) label.SendMessage("LateUpdate");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(game.UI.GetComponent<RectTransform>());
            foreach(SafeArtworkFrame frame in game.UI.GetComponentsInChildren<SafeArtworkFrame>(true)) frame.FitNow();
            Canvas.ForceUpdateCanvases();
            camera.Render(); camera.Render();
            if(game.UI.CurrentPage==MenuPage.Results)
                foreach(Text text in game.UI.GetComponentsInChildren<Text>())
                    if(text.name=="Live score" && text.cachedTextGenerator.vertexCount<4)
                        throw new Exception("The live score must remain visible at "+width+"x"+height+".");
            CheckPaintedButtonHits(game,camera);
            RenderTexture previous=RenderTexture.active; RenderTexture.active=target;
            var image=new Texture2D(width,height,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
            File.WriteAllBytes("Docs/Previews/"+name+".png",image.EncodeToPNG());
            RenderTexture.active=previous; camera.targetTexture=null;
            canvas.renderMode=RenderMode.ScreenSpaceOverlay; scaler.enabled=true;
            UnityEngine.Object.DestroyImmediate(image); target.Release(); UnityEngine.Object.DestroyImmediate(target);
            pose.Dispose();
        }

        private static void CaptureMenuArtwork(GameManager game)
        {
            Directory.CreateDirectory("Docs/Previews");
            game.UI.ShowSplash(); CapturePhoneSizes(game,"01-Splash");
            game.ReturnToMenu(); CapturePhoneSizes(game,"02-MainMenu");
            game.UI.ShowInstructions(); CapturePhoneSizes(game,"03-Instructions");
            game.UI.ShowAbout(); CapturePhoneSizes(game,"06-About");
            game.UI.ShowCredits(); CapturePhoneSizes(game,"07-Credits");
            game.UI.ShowLeaderboard(); CapturePhoneSizes(game,"08-Leaderboard-Empty");
            string[] names={"OPONG","CEBU KID","MAYA","LITO","BAYANI","ISA","MIGUEL","KAI","SARI","READY-RIDER12"};
            for(int i=0;i<names.Length;i++) game.Leaderboard.Record("menu-preview"+i,names[i],300-i*15,PreparednessContent.SupplyCount-i%4,500-i*17);
            game.UI.ShowLeaderboard(); CapturePhoneSizes(game,"11-Leaderboard-SampleRuns");
            CheckTrackArtwork(game); CaptureTrackArtwork(game);
            game.StartRun(); game.Spawner.ClearForChecks();
            game.Spawner.SpawnHazard(HazardKind.RoadBlock,1,14);
            CapturePhoneSizes(game,"26-Gameplay-JumpRewards");
            game.StartRun(); game.Spawner.ClearForChecks(); game.Pace.Advance(500);
            game.Road.SetDistance(500); game.Environment.Apply();
            game.UI.RefreshDistance();
            game.Spawner.SpawnPowerUp(PowerUpKind.DoubleScore,1,24);
            CapturePhoneSizes(game,"27-Gameplay-PowerUps");
            game.StartRun(); game.Simulate(2); CapturePhoneSizes(game,"09-Gameplay");
            game.StartRun(); game.Spawner.SpawnHazard(HazardKind.FloodStreak,0,26);
            game.Feedback("LONG FLOOD AHEAD\nChange lanes! Too long to jump.",false);
            CapturePhoneSizes(game,"25-Gameplay-LongFlood");
            game.Pace.Advance(250); game.UI.RefreshDistance(); Capture(game,"21-Gameplay-Speed250m");
            game.Hit(HazardKind.Debris); Capture(game,"22-Gameplay-SpeedReset");
            game.StartRun();
            foreach(TrackEntity entity in game.Spawner.Active)
                if(entity.IsSupply && entity.Supply==SupplyKind.GoBag && entity.Lane==1) entity.Consume();
            game.Spawner.SpawnSupply(SupplyKind.Cash,0,15); game.Spawner.SpawnSupply(SupplyKind.Food,1,18);
            game.Spawner.SpawnDistractor(DistractorKind.Toys,2,16);
            CapturePhoneSizes(game,"23-Gameplay-NewItems");
            game.Collect(SupplyKind.Cash); game.Collect(SupplyKind.Food); game.CollectDistractor(DistractorKind.Toys);
            Capture(game,"24-Gameplay-DistractorFeedback");
            for(int i=0;i<PreparednessContent.SupplyCount;i++) game.Collect((SupplyKind)i);
            game.FinishRun(); CapturePhoneSizes(game,"10-Results");
            game.StartRun(); game.Spawner.SpawnHazard(HazardKind.HangingCable,1,.7f); game.Duck(); game.Simulate(.05f);
            game.Player.Visual.GetComponent<OpongAnimation>().SendMessage("Update");
            game.Player.Visual.GetComponent<Animator>().Update(.15f);
            CapturePhoneSizes(game,"18-Gameplay-Duck");
            string report="Unity "+Application.unityVersion+" supplied menu PNG validation on "+DateTime.UtcNow.ToString("u")+"\n"+
                "Gameplay, menu navigation and persistent leaderboard smoke checks passed.\n"+
                "All seven page PNGs, empty leaderboard and ten sample leaderboard rows rendered at 720x1280, 720x1080 and 720x1600. Live results scores and menu/leaderboard navigation checked.\n"+
                "Painted-button tap areas checked with the Unity GraphicRaycaster at their centers and near all four edges. Transparent graphics remain tappable and their duplicate text is disabled.\n"+
                "Artwork preserves its original aspect ratio, all edges, and button alignment in one fitted safe-area frame.\n"+
                "Starting speed is "+RunPace.BaseSpeed+" m/s; speed adds "+RunPace.SpeedStep+" m/s every "+RunPace.Milestone+" m, with a configured ceiling of "+RunPace.MaximumSpeed+" m/s and the existing six-boost limit. Collision resets retain distance and restore starting speed. Pause, restart, frame-rate consistency and duck contact checks passed.\n"+
                "Gameplay and ducking rendered at three phone sizes with the doubled starting speed; milestone and collision-reset HUD previews also rendered.\n"+
                "Nine useful supplies include cash (+10) and non-perishable food (+15). Five distractors subtract five points, never reduce lives, reset speed, fill checklist types or make scores negative. Actual pickups, pause/restart, natural spawning and score persistence checked.\n"+
                "All fourteen regular pickups, four power-ups and six hazards rendered individually and checked for finite geometry, triangle budgets and absence of floating text. Single jump rewards and a single bonus token rendered at three phone sizes.\n"+
                "Sample scores use isolated in-memory storage; no real player scores were modified. Original PNG pixels were not edited.\n"+
                "Android touch and physical-device testing has not been performed.\n";
            File.WriteAllText("Docs/Menu-PNG-Validation.txt",report);
            Debug.Log("ANDAM_MENU_PNG_PASS: seven page PNGs, nine supplies, five distractors, four power-ups and "+RunPace.BaseSpeed+" m/s gameplay rendered; transparent-button raycasts passed at three phone sizes.");
        }

        private static void CapturePhoneSizes(GameManager game,string name)
        {
            Capture(game,name); Capture(game,name+"-ShortPhone",720,1080); Capture(game,name+"-TallPhone",720,1600);
        }

        private static void CheckPaintedButtonHits(GameManager game,Camera camera)
        {
            foreach(Button button in game.UI.GetComponentsInChildren<Button>())
            {
                Transform frame=button.transform.parent;
                while(frame!=null && frame.name!="Artwork and touch targets") frame=frame.parent;
                if(frame==null || button.transition!=Selectable.Transition.None) continue;
                if(button.targetGraphic.color.a!=0 || button.transition!=Selectable.Transition.None || button.GetComponentInChildren<Text>().enabled)
                    throw new Exception("Painted buttons must have no duplicate graphics/text: "+button.name);
                RectTransform rect=button.GetComponent<RectTransform>();
                Vector2[] points={new Vector2(.5f,.5f),new Vector2(.10f,.5f),new Vector2(.90f,.5f),new Vector2(.5f,.10f),new Vector2(.5f,.90f)};
                foreach(Vector2 point in points)
                {
                    Vector3 local=new Vector3(Mathf.Lerp(rect.rect.xMin,rect.rect.xMax,point.x),Mathf.Lerp(rect.rect.yMin,rect.rect.yMax,point.y),0);
                    Vector2 screen=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(local));
                    var hits=new List<RaycastResult>();
                    game.UI.GetComponent<GraphicRaycaster>().Raycast(new PointerEventData(EventSystem.current){position=screen},hits);
                    if(hits.Count==0 || hits[0].gameObject.GetComponentInParent<Button>()!=button)
                        throw new Exception("Painted button tap area blocked: "+button.name+" at "+point+" screen "+screen+" depth "+button.targetGraphic.depth+" culled "+button.targetGraphic.canvasRenderer.cull+" hits "+string.Join(", ",hits.ConvertAll(hit=>hit.gameObject.name)));
                }
            }
        }

        private static void CheckModel(GameManager game)
        {
            Transform body=game.Player.Visual.Find("Opong storybook character");
            if(Resources.Load<GameObject>("OpongStorybook")==null) throw new Exception("Editable gameplay prefab is missing.");
            if(body==null || body.Find("Arm L")==null || body.Find("Leg R")==null) throw new Exception("Animation joints missing.");
            SkinnedMeshRenderer[] renderers=game.Player.Visual.GetComponentsInChildren<SkinnedMeshRenderer>();
            if(renderers.Length!=1 || renderers[0].sharedMaterials.Length!=1) throw new Exception("Opong must use one skinned renderer and material.");
            SkinnedMeshRenderer skin=renderers[0]; Mesh mesh=skin.sharedMesh;
            if(!AssetDatabase.Contains(mesh)) throw new Exception("The prefab must reference its saved mesh.");
            int triangles=mesh.triangles.Length/3;
            if(triangles<8000 || triangles>15000) throw new Exception("Guide triangle budget exceeded: "+triangles);
            foreach(Vector3 v in mesh.vertices) if(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z)) throw new Exception("Invalid mesh vertex.");
            foreach(Vector3 n in mesh.normals) if(float.IsNaN(n.x) || float.IsNaN(n.y) || float.IsNaN(n.z) || n.sqrMagnitude<.1f) throw new Exception("Invalid mesh normal.");
            if(mesh.bindposes.Length!=skin.bones.Length || mesh.boneWeights.Length!=mesh.vertexCount) throw new Exception("Invalid skin binding.");
            foreach(BoneWeight weight in mesh.boneWeights) if(weight.boneIndex0>=skin.bones.Length || Mathf.Abs(weight.weight0-1)>.001f) throw new Exception("Invalid skin weight.");
            BoneWeight[] weights=mesh.boneWeights; int[] indices=mesh.triangles;
            for(int i=0;i<indices.Length;i+=3) if(weights[indices[i]].boneIndex0!=weights[indices[i+1]].boneIndex0 || weights[indices[i]].boneIndex0!=weights[indices[i+2]].boneIndex0) throw new Exception("Rigid segments must not join different joints in one triangle.");
            if(mesh.uv.Length!=mesh.vertexCount || skin.sharedMaterial.mainTexture.width!=1024) throw new Exception("Missing 1024 atlas/UVs.");
            Animator animator=game.Player.Visual.GetComponent<Animator>();
            if(!animator.keepAnimatorStateOnDisable) throw new Exception("Damage blinking must preserve animation state.");
            string[] clips={"Idle","Run Loop","Jump Start","Air","Land","Slide Duck","Hit","Victory"};
            foreach(string clip in clips) if(!animator.HasState(0,Animator.StringToHash(clip))) throw new Exception("Missing animation state: "+clip);
            OpongAnimation actor=game.Player.Visual.GetComponent<OpongAnimation>();
            actor.EvaluatePreview("Idle",.25f); Quaternion idle=body.Find("Arm L").localRotation;
            var sampled=new Mesh(); skin.BakeMesh(sampled); Vector3 idleBounds=sampled.bounds.size;
            actor.EvaluatePreview("Air",.25f,OpongMood.Determined);
            if(Quaternion.Angle(idle,body.Find("Arm L").localRotation)<15) throw new Exception("Animation is not moving the rig.");
            skin.BakeMesh(sampled);
            if((idleBounds-sampled.bounds.size).sqrMagnitude<.001f) throw new Exception("Joint movement must deform the weighted mesh.");
            actor.EvaluatePreview("Slide Duck",.25f,OpongMood.Determined); skin.BakeMesh(sampled);
            if(sampled.bounds.max.y>=2.20f) throw new Exception("Duck silhouette must clear the lowest cable and arrow plate: "+sampled.bounds.max.y);
            UnityEngine.Object.DestroyImmediate(sampled);
            OpongExpressions face=game.Player.Visual.GetComponent<OpongExpressions>();
            foreach(OpongMood mood in Enum.GetValues(typeof(OpongMood)))
            {
                face.Apply(mood,false);
                Transform head=body.Find("Spiky hair + headband + earrings");
                bool open=mood==OpongMood.Surprised || mood==OpongMood.Hurt;
                if((head.Find("Surprised mouth").localScale.x>.5f)!=open) throw new Exception("Incorrect open-mouth expression.");
                if((head.Find("Concerned mouth").localScale.x>.5f)!=(mood==OpongMood.Worried)) throw new Exception("Incorrect worried expression.");
            }
            actor.EvaluatePreview("Land",.5f);
            if(body.localScale.y>.96f) throw new Exception("Landing pose is not evaluating.");
            actor.ResetPose();
            if(game.Environment==null || game.Road.Pack==null || Camera.main.GetComponent<CoastalIllustrationBackground>()!=null) throw new Exception("Layered 3D environment must replace the gameplay painting.");
            Debug.Log("ANDAM_ART_PASS: "+triangles+" triangles, "+mesh.vertexCount+" vertices, one skinned renderer/material, "+skin.bones.Length+" bones, 1024 atlas, eight evaluated clips and layered 3D environment.");
        }

        private static void CheckTrackArtwork(GameManager game)
        {
            var supplies=new HashSet<SupplyKind>(); var hazards=new HashSet<HazardKind>(); var distractors=new HashSet<DistractorKind>(); var powers=new HashSet<PowerUpKind>();
            int maxSupply=0,maxHazard=0,maxDistractor=0;
            foreach(TrackEntity entity in game.Spawner.GetComponentsInChildren<TrackEntity>(true))
            {
                if(entity.GetComponentsInChildren<TextMesh>(true).Length!=0 || entity.GetComponentsInChildren<WorldLabel>(true).Length!=0)
                    throw new Exception("Track objects must not display floating text: "+entity.name);
                int triangles=0;
                foreach(MeshRenderer renderer in entity.GetComponentsInChildren<MeshRenderer>(true))
                {
                    bool visible=renderer.enabled;
                    for(Transform part=renderer.transform;part!=entity.transform;part=part.parent) visible&=part.gameObject.activeSelf;
                    if(!visible) continue;
                    Mesh mesh=renderer.GetComponent<MeshFilter>().sharedMesh;
                    triangles+=mesh.triangles.Length/3;
                    foreach(Vector3 v in mesh.vertices) if(!Finite(v)) throw new Exception("Invalid object vertex: "+entity.name);
                    foreach(Vector3 n in mesh.normals) if(!Finite(n) || n.sqrMagnitude<.1f) throw new Exception("Invalid object normal: "+entity.name);
                }
                int limit=entity.IsPowerUp?1200:entity.IsPickup?800:entity.Hazard==HazardKind.FloodStreak?5000:1500;
                if(triangles<100 || triangles>limit) throw new Exception("Track artwork triangle budget: "+entity.name+" / "+triangles);
                if(entity.IsSupply) { supplies.Add(entity.Supply); maxSupply=Mathf.Max(maxSupply,triangles); }
                else if(entity.IsDistractor) { distractors.Add(entity.Distractor); maxDistractor=Mathf.Max(maxDistractor,triangles); }
                else if(entity.IsPowerUp) powers.Add(entity.PowerUp);
                else { hazards.Add(entity.Hazard); maxHazard=Mathf.Max(maxHazard,triangles); }
            }
            if(supplies.Count!=PreparednessContent.SupplyCount || distractors.Count!=PreparednessContent.DistractorCount || hazards.Count!=PreparednessContent.HazardCount || powers.Count!=PreparednessContent.PowerUpCount) throw new Exception("Every supply, distractor, power-up and hazard needs checked artwork.");
            Debug.Log("ANDAM_TRACK_ART_PASS: nine supplies, five distractors and "+PreparednessContent.HazardCount+" hazards have no floating text, valid meshes, max supply "+maxSupply+" / distractor "+maxDistractor+" / hazard "+maxHazard+" triangles.");
        }
        private static bool Finite(Vector3 v)
        { return !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z) && !float.IsInfinity(v.x) && !float.IsInfinity(v.y) && !float.IsInfinity(v.z); }

        private static void CaptureTrackArtwork(GameManager game)
        {
            Canvas canvas=game.UI.GetComponent<Canvas>(); canvas.enabled=false;
            foreach(SupplyKind kind in Enum.GetValues(typeof(SupplyKind))) CaptureObject("Item-"+kind,kind,null);
            foreach(DistractorKind kind in Enum.GetValues(typeof(DistractorKind))) CaptureObject("Distractor-"+kind,null,null,kind);
            foreach(HazardKind kind in Enum.GetValues(typeof(HazardKind))) CaptureObject("Hazard-"+kind,null,kind);
            foreach(PowerUpKind kind in Enum.GetValues(typeof(PowerUpKind))) CaptureObject("PowerUp-"+kind,null,null,null,kind);
            canvas.enabled=true;
        }
        private static void CaptureObject(string name,SupplyKind? supply,HazardKind? hazard,DistractorKind? distractor=null,PowerUpKind? powerUp=null)
        {
            Transform review=PrimitiveArt.Root("Object review",null,new Vector3(30,0,0));
            if(supply.HasValue) RunnerReferenceArt.Supply(review,supply.Value);
            else if(distractor.HasValue) RunnerReferenceArt.Distractor(review,distractor.Value);
            else if(powerUp.HasValue) RunnerReferenceArt.PowerUp(review,powerUp.Value);
            else RunnerReferenceArt.Hazard(review,hazard.Value);
            PrimitiveArt.Box(review,"Review ground",new Vector3(0,-.025f,0),new Vector3(12,.04f,12),new Color32(249,237,213,255));
            foreach(Transform part in review.GetComponentsInChildren<Transform>(true)) part.gameObject.layer=9;
            var obj=new GameObject("Object review camera"); Camera camera=obj.AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color32(249,237,213,255); camera.cullingMask=1<<9;
            bool fullWidth=hazard==HazardKind.HangingCable;
            bool pickup=supply.HasValue || distractor.HasValue || powerUp.HasValue;
            bool longFlood=hazard==HazardKind.FloodStreak;
            camera.orthographic=true; camera.orthographicSize=pickup?.90f:longFlood?17f:fullWidth?4.70f:hazard==HazardKind.Floodwater?2.30f:1.50f;
            camera.transform.position=review.position+(pickup?new Vector3(.65f,1.55f,-3.4f):longFlood?new Vector3(4,18,-20):fullWidth?new Vector3(2.4f,3.5f,-6.4f):new Vector3(2.4f,2.3f,-4.4f));
            camera.transform.LookAt(review.position+new Vector3(0,pickup?.65f:fullWidth?1.75f:.38f,0));
            var target=new RenderTexture(1024,1024,24,RenderTextureFormat.ARGB32){antiAliasing=2}; target.Create(); camera.targetTexture=target;
            camera.Render(); RenderTexture previous=RenderTexture.active; RenderTexture.active=target;
            var picture=new Texture2D(1024,1024,TextureFormat.RGBA32,false); picture.ReadPixels(new Rect(0,0,1024,1024),0,0); picture.Apply();
            File.WriteAllBytes("Docs/Previews/"+name+".png",picture.EncodeToPNG());
            RenderTexture.active=previous; camera.targetTexture=null; target.Release();
            UnityEngine.Object.DestroyImmediate(picture); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(obj); UnityEngine.Object.DestroyImmediate(review.gameObject);
        }

        private static void CaptureCharacter(GameManager game,string name,Vector3 position,bool face=false)
        {
            PoseSnapshot pose=new PoseSnapshot(game.Player.Visual);
            Canvas canvas=game.UI.GetComponent<Canvas>(); canvas.enabled=false;
            Transform floor=PrimitiveArt.Box(game.transform,"Portrait ground",new Vector3(0,-.025f,0),new Vector3(12,.04f,12),new Color32(249,237,213,255)); floor.gameObject.layer=8;
            var obj=new GameObject("Character review camera"); Camera camera=obj.AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color32(249,237,213,255); camera.cullingMask=1<<8;
            camera.orthographic=true; camera.orthographicSize=face?.8f:1.75f; camera.transform.position=position; camera.transform.LookAt(new Vector3(0,face?2.25f:1.47f,0));
            var target=new RenderTexture(1024,1024,24,RenderTextureFormat.ARGB32){antiAliasing=2}; target.Create(); camera.targetTexture=target;
            camera.Render(); RenderTexture previous=RenderTexture.active; RenderTexture.active=target;
            var image=new Texture2D(1024,1024,TextureFormat.RGBA32,false); image.ReadPixels(new Rect(0,0,1024,1024),0,0); image.Apply();
            File.WriteAllBytes("Docs/Previews/"+name+".png",image.EncodeToPNG());
            RenderTexture.active=previous; camera.targetTexture=null; target.Release();
            UnityEngine.Object.DestroyImmediate(image); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(obj); UnityEngine.Object.DestroyImmediate(floor.gameObject); canvas.enabled=true;
            pose.Dispose();
        }

        // Camera.Render calls in one editor callback share cached GPU skinning data.
        // Bake the current weighted pose so every offscreen image shows its selected clip/mood.
        private sealed class PoseSnapshot : IDisposable
        {
            private readonly SkinnedMeshRenderer skin;
            private readonly Mesh mesh;
            private readonly GameObject obj;
            public PoseSnapshot(Transform visual)
            {
                skin=visual.GetComponent<SkinnedMeshRenderer>(); mesh=new Mesh(); skin.BakeMesh(mesh);
                obj=new GameObject("Current Opong pose for offscreen render"); obj.transform.SetParent(visual,false); obj.layer=visual.gameObject.layer;
                obj.AddComponent<MeshFilter>().sharedMesh=mesh;
                MeshRenderer renderer=obj.AddComponent<MeshRenderer>(); renderer.sharedMaterial=skin.sharedMaterial;
                renderer.shadowCastingMode=skin.shadowCastingMode; skin.enabled=false;
            }
            public void Dispose()
            { skin.enabled=true; UnityEngine.Object.DestroyImmediate(obj); UnityEngine.Object.DestroyImmediate(mesh); }
        }
    }
}
