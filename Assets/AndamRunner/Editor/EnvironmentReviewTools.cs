using System;
using System.IO;
using AndamRunner;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AndamRunnerEditor
{
    [InitializeOnLoad]
    public static class EnvironmentReviewTools
    {
        private const string Pending="Andam.EnvironmentPreview";
        static EnvironmentReviewTools() { EditorApplication.playModeStateChanged+=OnPlay; }
        [MenuItem("ANDAM/Environment/3. Validate and render the vertical slice")]
        public static void Begin()
        {
            PrototypeTools.PrepareProject(); SessionState.SetBool(Pending,true); EditorApplication.isPlaying=true;
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pending,false)) return;
            SessionState.SetBool(Pending,false);
            try
            {
                GameManager game=UnityEngine.Object.FindAnyObjectByType<GameManager>(); game.enabled=false;
                game.UseTestLeaderboard(new LeaderboardStore(null,false)); EnvironmentChecks.Run(game);
                foreach(InputField field in game.UI.GetComponentsInChildren<InputField>(true)) field.SetTextWithoutNotify("OPONG");
                Render(game);
                if(Application.isBatchMode) EditorApplication.Exit(0); else { game.enabled=true; game.StartRun(); }
            }
            catch(Exception error) { Debug.LogException(error); if(Application.isBatchMode) EditorApplication.Exit(1); else EditorApplication.isPlaying=false; }
        }
        public static void Render(GameManager game)
        {
            string prefix=game.Road.Pack.Graybox?"Graybox":"Slice";
            foreach(var sample in new[]{new {Name="01-CoastalRoad",Distance=120f},new {Name="02-BarangayAlley",Distance=336f},new {Name="03-UphillFloodEscape",Distance=540f},new {Name="04-DryRefuge",Distance=600f}})
            {
                game.StartRun(); game.Spawner.ClearForChecks(); game.Pace.Advance(sample.Distance); game.Road.SetDistance(sample.Distance);
                game.Environment.Apply(); game.Player.SetRoadSlope(game.Road.SlopeAtZ(0)); game.Player.Tick(0,false); game.UI.Refresh();
                game.Spawner.SpawnSupply(SupplyKind.Water,0,22);
                game.Spawner.SpawnHazard(HazardKind.RoadBlock,2,31);
                foreach(TrackEntity entity in game.Spawner.Active) entity.Animate(Time.time,game.Road.HeightAtZ(entity.transform.position.z),game.Road.SlopeAtZ(entity.transform.position.z));
                foreach(Vector2Int size in new[]{new Vector2Int(720,1280),new Vector2Int(720,1080),new Vector2Int(720,1600)})
                    PreviewRenderer.Capture(game,prefix+"-"+sample.Name+"-"+size.y,size.x,size.y);
            }
            if(!game.Road.Pack.Graybox)
            {
                game.FinishRun();
                foreach(Vector2Int size in new[]{new Vector2Int(720,1280),new Vector2Int(720,1080),new Vector2Int(720,1600)})
                    PreviewRenderer.Capture(game,"Slice-05-HomeLesson-"+size.y,size.x,size.y);
            }
            Debug.Log("ANDAM_ENVIRONMENT_RENDER_PASS: "+prefix+" coast, alley, uphill flood escape and refuge at three phone sizes.");
        }
    }
}
