using System;
using System.IO;
using AndamRunner;
using UnityEngine;
using UnityEngine.UI;

namespace AndamRunnerEditor
{
    public static class SmokeChecks
    {
        private static int assertions;
        private static void Check(bool condition,string message)
        { assertions++; if(!condition) throw new Exception("Smoke check failed: "+message); }

        public static void Run(GameManager game)
        {
            assertions=0;
            game.UseTestLeaderboard(new LeaderboardStore(null,false));
            Check(game.State==RunState.Splash && game.UI.CurrentPage==MenuPage.Splash,"credits splash at launch");
            Click(game,"CONTINUE");
            Check(game.State==RunState.Ready && game.UI.CurrentPage==MenuPage.Home,"splash continue opens menu");
            Click(game,"HOW TO PLAY"); Check(game.UI.CurrentPage==MenuPage.Instructions,"instructions navigation");
            game.Simulate(1); Check(game.Distance==0,"menu does not run the game");
            if(Resources.Load<Texture2D>("HowToBackground")==null) { Click(game,"SUPPLIES"); Click(game,"STAY SAFE"); }
            game.UI.HandleBack();
            Click(game,"ABOUT THE GAME"); Check(game.UI.CurrentPage==MenuPage.About,"about navigation");
            bool paintedAbout=Resources.Load<Texture2D>("AboutBackground")!=null;
            if(paintedAbout)
            {
                Click(game,"<  BACK"); Check(game.UI.CurrentPage==MenuPage.Home,"painted About back restores home");
                Click(game,"CREDITS");
            }
            else Click(game,"MEET THE TEAM  /  CREDITS");
            Check(game.UI.CurrentPage==MenuPage.Credits,"credits navigation");
            Check(game.Information.developer=="UP Cebu Center for Environmental Informatics","requested developer credit");
            Check(game.Information.partner=="City of Lapu-Lapu Disaster Risk Reduction and Management Department","requested partner credit");
            game.UI.HandleBack(); Check(game.UI.CurrentPage==(paintedAbout ? MenuPage.Home : MenuPage.About),"credits back restores its originating page");
            if(!paintedAbout) game.UI.HandleBack();
            Click(game,"LEADERBOARD");
            Check(game.UI.CurrentPage==MenuPage.Leaderboard && game.Leaderboard.Entries.Count==0,"empty leaderboard");
            game.UI.HandleBack(); Click(game,"LET'S GET READY   >");
            Check(game.State==RunState.Playing && game.UI.CurrentPage==MenuPage.Game,"menu play wiring");
            CheckLeaderboardPersistence();
            CheckPace();
            RunSession session=new RunSession();
            for(int i=0;i<PreparednessContent.SupplyCount;i++) session.Collect((SupplyKind)i);
            Check(session.Score==130 && session.UniqueSupplies==9 && session.Collected==9,"nine useful supplies include cash and food");
            session.Collect(SupplyKind.Water); Check(session.UniqueSupplies==9 && session.Score==140,"repeat supply scoring");
            session.Hit(); session.Hit(); session.Hit(); session.Hit();
            Check(session.Lives==0 && session.Score==110,"three lives and single point penalty per life");
            session.Collect(SupplyKind.GoBag); Check(session.Score==110,"no scoring after game over");
            session.Reset(); session.Hit(); Check(session.Score==0,"score cannot become negative");
            game.StartRun();
            for(int i=0;i<5;i++) game.Move(-1); Check(game.Player.Lane==0,"left lane clamp");
            for(int i=0;i<5;i++) game.Move(1); Check(game.Player.Lane==2,"right lane clamp");
            game.Player.ResetRunner(); game.Player.Jump(); game.Player.Tick(.46f,false);
            Check(Mathf.Abs(game.Player.Height-PlayerRunner.JumpHeight)<.01f,"jump apex");
            game.Player.Jump(); game.Player.Tick(.46f,false); Check(game.Player.Height==0 && !game.Player.IsJumping,"jump cannot restart in midair and lands on road");
            for(int kind=0;kind<4;kind++)
            {
                TrackEntity hazard=game.Spawner.SpawnHazard((HazardKind)kind,1,1f);
                Check(hazard!=null,"hazard pool capacity");
                bool contact=hazard.IsContact(new Vector3(0,2.3f,0),new Vector3(0,2.3f,0),3,-3);
                Check(!contact,"all ground obstacles are jumpable");
                Check(hazard.IsContact(Vector3.zero,Vector3.zero,3,-3),"swept hazard contact at slow frame");
                hazard.Consume(); Check(!hazard.IsContact(Vector3.zero,Vector3.zero,1,-1),"consumed object cannot collide twice");
            }
            CheckDuck(game);
            JumpChecks.Run(game);
            CheckNewItems(game);
            PowerUpChecks.Run(game);
            RunCadenceChecks.Run(game);
            game.StartRun(); game.Spawner.SpawnSupply(SupplyKind.GoBag,1,.1f); game.Simulate(.1f);
            Check(game.Session.Score==20 && game.Session.Collected==1,"real supply pickup");
            game.Simulate(.1f); Check(game.Session.Score==20,"pickup only scored once");
            game.Spawner.SpawnHazard(HazardKind.RoadBlock,1,.1f); game.Simulate(.1f);
            Check(game.Session.Lives==2 && game.Session.Score==10,"real collision and penalty");
            game.Spawner.SpawnHazard(HazardKind.RoadBlock,1,.1f); game.Simulate(.1f);
            Check(game.Session.Lives==2,"collision grace prevents stacked lives loss");
            game.StartRun(); game.Hit(HazardKind.Floodwater); game.Simulate(1.31f); game.Hit(HazardKind.PowerLine); game.Simulate(1.31f); game.Hit(HazardKind.RoadBlock);
            Check(game.State==RunState.Results && game.Session.Lives==0,"game over and results");
            int saved=game.Leaderboard.Entries.Count;
            Check(saved==1 && game.LastRank==1,"completed run saved automatically");
            Click(game,"SCORES"); Check(game.UI.CurrentPage==MenuPage.Leaderboard,"results leaderboard navigation");
            game.UI.HandleBack(); Check(game.UI.CurrentPage==MenuPage.Results,"leaderboard returns to results");
            game.FinishRun(); Check(game.Leaderboard.Entries.Count==saved,"results cannot save same run twice");
            Click(game,"MAIN MENU"); Check(game.State==RunState.Ready && game.UI.CurrentPage==MenuPage.Home,"results to menu");
            game.StartRun();
            Check(game.Speed==RunPace.BaseSpeed && game.Pace.NextMilestone==250,"restart resets speed and milestones");
            Check(game.Session.Score==0 && game.Session.Lives==3 && game.Distance==0 && game.Player.Lane==1 && game.Player.Height==0 && game.Spawner.ActiveCount==6,"restart resets run and one tutorial jump reward");
            game.TogglePause(); game.Simulate(5f); Check(game.Distance==0 && game.State==RunState.Paused,"pause freezes movement");
            game.TogglePause(); Check(game.State==RunState.Playing,"resume");
            for(int i=0;i<150;i++) game.Simulate(1f/30f); float distance30=game.Distance;
            game.StartRun(); for(int i=0;i<600;i++) game.Simulate(1f/120f);
            Check(Mathf.Abs(distance30-game.Distance)<.1f,"30 and 120 fps travel consistency");
            // Exercise recycling independently of run life state over a large travel distance.
            game.Road.ResetRoad();
            for(int i=0;i<10000;i++) game.Road.Tick(.3f);
            foreach(Transform chunk in game.Road.transform)
                Check(chunk.localPosition.z>=-36 && chunk.localPosition.z<144,"road chunk stays within bounded coordinates");
            game.StartRun();
            for(int i=0;i<6000;i++)
            {
                ChooseSafeLane(game);
                game.Simulate(1f/60f);
                Check(game.Spawner.ActiveCount<=SpawnManager.MaximumActiveEntities,"bounded active pool during endless spawning");
                if(game.State==RunState.Results) game.StartRun();
            }
            Check(game.Spawner.ActiveCount>0,"objects keep spawning");
            game.StartRun(); Check(game.Spawner.ActiveCount==6 && game.Session.Lives==3,"restart after extended run");
            Debug.Log("ANDAM: "+assertions+" smoke assertions passed.");
        }

        private static void CheckNewItems(GameManager game)
        {
            game.StartRun();
            game.Spawner.ClearForChecks();
            game.Spawner.SpawnSupply(SupplyKind.Cash,1,.1f); game.Simulate(.1f);
            Check(game.Session.Score==10 && game.Session.UniqueSupplies==1,"actual cash pickup earns points and checklist type");
            game.Spawner.SpawnSupply(SupplyKind.Food,1,.1f); game.Simulate(.1f);
            Check(game.Session.Score==25 && game.Session.UniqueSupplies==2,"actual non-perishable food pickup earns points and checklist type");
            int supplies=game.Session.Collected,mask=game.Session.SupplyMask;
            game.Pace.Advance(250); float boosted=game.Speed;
            for(int i=0;i<PreparednessContent.DistractorCount;i++)
            {
                TrackEntity item=game.Spawner.SpawnDistractor((DistractorKind)i,1,.1f);
                Check(item!=null && item.IsDistractor && !item.IsSupply && item.IsPickup && !item.SpansAllLanes,"distractor uses pickup collision rules, not hazard rules");
                Check(!item.IsContact(Vector3.up*2,Vector3.up*2,3,-3),"jumping can avoid an unwanted pickup");
                game.Simulate(.1f);
                Check(game.Session.Score==25-(i+1)*5,"each real distractor pickup subtracts exactly five points");
                Check(game.Session.Lives==3 && game.Speed==boosted && game.GraceRemaining==0,"distractors do not cost lives, reset speed or trigger damage grace");
                Check(game.Session.Collected==supplies && game.Session.UniqueSupplies==2 && game.Session.SupplyMask==mask,"distractors cannot advance the useful-supply checklist");
                Check(game.Session.DistractorsCollected==i+1,"distractor contact counted once");
            }
            game.CollectDistractor(DistractorKind.Toys); Check(game.Session.Score==0,"distractor deductions cannot make scores negative");
            game.TogglePause(); game.CollectDistractor(DistractorKind.Beer);
            Check(game.Session.DistractorsCollected==6,"paused runs do not accept distractor pickups");
            game.TogglePause(); game.FinishRun(); game.CollectDistractor(DistractorKind.Makeup);
            Check(game.Session.DistractorsCollected==6,"finished runs do not accept distractor pickups");
            game.UseTestLeaderboard(new LeaderboardStore(null,false));
            game.StartRun(); Check(game.Session.DistractorsCollected==0,"restart clears distractor count");
            var seenSupplies=new System.Collections.Generic.HashSet<SupplyKind>();
            var seenDistractors=new System.Collections.Generic.HashSet<DistractorKind>();
            for(int i=0;i<1500;i++)
            {
                foreach(TrackEntity entity in game.Spawner.Active)
                {
                    if(entity.IsSupply) seenSupplies.Add(entity.Supply);
                    if(entity.IsDistractor)
                    {
                        seenDistractors.Add(entity.Distractor);
                        foreach(TrackEntity other in game.Spawner.Active)
                            if(!other.IsPickup && Mathf.Abs(entity.transform.position.z-other.transform.position.z)<.1f)
                                Check(false,"distractor rows must not overlap hazard/cable rows");
                    }
                }
                ChooseSafeLane(game); game.Simulate(1f/60f);
                if(game.State==RunState.Results) game.StartRun();
            }
            Check(seenSupplies.Count==9 && seenDistractors.Count==5,"natural spawning presents all nine supplies and all five distractors");
            game.UseTestLeaderboard(new LeaderboardStore(null,false));
        }

        private static void CheckPace()
        {
            var pace=new RunPace();
            pace.Advance(249); Check(pace.Speed==RunPace.BaseSpeed,"configured starting speed is constant before 250 meters");
            pace.Advance(1); Check(pace.Speed==Mathf.Min(RunPace.MaximumSpeed,RunPace.BaseSpeed+RunPace.SpeedStep) && pace.NextMilestone==500,"first speed step at exactly 250 meters");
            pace.Advance(250); Check(pace.Speed==Mathf.Min(RunPace.MaximumSpeed,RunPace.BaseSpeed+2*RunPace.SpeedStep),"second speed step at exactly 500 meters");
            pace.Advance(1250); Check(pace.Speed==Mathf.Min(RunPace.MaximumSpeed,RunPace.BaseSpeed+6*RunPace.SpeedStep) && pace.Distance==1750,"large distance steps process milestones and cap speed");
            pace.ResetSpeed(); Check(pace.Speed==RunPace.BaseSpeed && pace.Distance==1750 && pace.NextMilestone==2000,"bump preserves distance and the next global milestone");
            pace.Advance(249); Check(pace.Speed==RunPace.BaseSpeed,"speed stays reset until next milestone");
            pace.Advance(1); Check(pace.Speed==Mathf.Min(RunPace.MaximumSpeed,RunPace.BaseSpeed+RunPace.SpeedStep),"boost resumes from base at next milestone after a bump");
            pace.Restart(); Check(pace.Distance==0 && pace.Speed==RunPace.BaseSpeed && pace.NextMilestone==250,"new run resets pace");
        }

        private static void CheckDuck(GameManager game)
        {
            game.StartRun(); game.Duck();
            Check(game.Player.IsDucking && game.Player.Height==0,"duck stays on the shield");
            game.Jump(); Check(!game.Player.IsJumping,"jump cannot interrupt a duck");
            game.Player.Tick(.5f,false); float remaining=game.Player.DuckRemaining;
            game.Duck(); Check(game.Player.DuckRemaining==remaining,"repeated input cannot extend a duck indefinitely");
            game.TogglePause(); game.Simulate(2);
            Check(game.Player.DuckRemaining==remaining,"pause freezes the duck timer");
            game.TogglePause(); game.Player.Tick(.61f,false);
            Check(!game.Player.IsDucking,"duck ends automatically");
            game.Jump(); game.Duck(); Check(game.Player.IsJumping && !game.Player.IsDucking,"cannot duck in the air");
            game.Player.ResetRunner();
            for(int lane=0;lane<3;lane++)
            {
                TrackEntity cable=game.Spawner.SpawnHazard(HazardKind.HangingCable,lane,1);
                Check(cable!=null && cable.SpansAllLanes && cable.Lane==1 && cable.transform.position.x==0,"one centered obstacle spans all lanes");
                Vector3 position=new Vector3((lane-1)*PlayerRunner.LaneWidth,0,0);
                Check(cable.IsContact(position,position,3,-3),"standing hits the hanging cable in every lane");
                Check(!cable.IsContact(position,position,3,-3,1,.1f),"ducking clears the hanging cable in every lane");
                Check(cable.IsContact(position+Vector3.up*2.3f,position+Vector3.up*2.3f,3,-3,1,.1f),"jump cannot clear the full-width cable");
                Check(cable.IsContact(position,position,3,-3,.04f,.1f),"duck expired before a swept crossing cannot avoid contact");
                Check(!cable.IsContact(position,position,3,-3,.06f,.1f),"active duck at a swept crossing avoids contact");
                cable.Consume();
            }
            game.StartRun(); game.Spawner.SpawnHazard(HazardKind.HangingCable,2,.1f); game.Duck(); game.Simulate(.1f);
            Check(game.Session.Lives==3,"actual duck avoids full-width contact");
            game.Simulate(1.1f); Check(game.Session.Lives==3 && !game.Player.IsDucking,"cable stays cleared after passing and standing up");
            game.StartRun(); game.Pace.Advance(250); game.Spawner.SpawnHazard(HazardKind.HangingCable,0,.1f); game.Simulate(.1f);
            Check(game.Session.Lives==2 && game.Speed==RunPace.BaseSpeed && game.Distance>250,"one full-width hit deducts one life and resets speed");
            game.Pace.Advance(250); game.Hit(HazardKind.Debris);
            Check(game.Session.Lives==2 && game.Speed==RunPace.BaseSpeed,"grace prevents another life loss while every bump still resets speed");
            game.StartRun(); Click(game,"DUCK"); Check(game.Player.IsDucking,"touch duck button is connected");
            game.StartRun(); Check(!game.Player.IsDucking,"restart clears duck state");
            Check(Camera.main.GetComponent<CoastalIllustrationBackground>().Storm,"gameplay uses the storm evacuation backdrop");
        }

        private static void Click(GameManager game,string name)
        {
            foreach(Button button in game.UI.GetComponentsInChildren<Button>(true))
                if(button.gameObject.name==name && button.gameObject.activeInHierarchy) { button.onClick.Invoke(); return; }
            throw new Exception("Missing active button: "+name);
        }

        private static void CheckLeaderboardPersistence()
        {
            string folder=Path.Combine(Directory.GetCurrentDirectory(),"Temp","AndamSmoke-"+Guid.NewGuid().ToString("N"));
            string path=Path.Combine(folder,"scores.json");
            try
            {
                var board=new LeaderboardStore(path);
                board.Record("lower","  cebu kid  ",20,1,100);
                board.Record("more-supplies","MAYA",20,3,10);
                board.Record("farther","LITO",20,3,200);
                Check(board.Entries[0].runId=="farther" && board.Entries[1].runId=="more-supplies","leaderboard ties resolve by supplies then distance");
                Check(board.Record("farther","different",999,7,999)==1 && board.Entries.Count==3,"leaderboard deduplicates run IDs");
                var reopened=new LeaderboardStore(path);
                Check(reopened.Entries.Count==3 && reopened.Entries[0].nickname=="LITO" && reopened.LastNickname=="LITO","leaderboard persists after reopening");
                Check(LeaderboardStore.CleanNickname("<script>!").IndexOf('<')<0 && LeaderboardStore.CleanNickname("abcdefghijklmnop").Length==12,"nickname constraints");
                for(int i=0;i<15;i++) reopened.Record("extra"+i,"OPONG",100+i,7,1000);
                Check(reopened.Entries.Count==10 && reopened.Entries[0].score==114 && reopened.Entries[9].score==105,"top ten keeps highest scores");
                File.WriteAllText(path,"invalid JSON");
                var recovered=new LeaderboardStore(path);
                Check(recovered.Entries.Count==10 && recovered.Entries[0].score==113,"backup recovers a damaged save");
                File.WriteAllText(path,"{\"version\":1,\"lastNickname\":\"\",\"entries\":[null]}");
                var empty=new LeaderboardStore(path);
                Check(empty.Entries.Count==0 && empty.LastNickname=="OPONG ANDAM","invalid saved entries are ignored and the mascot nickname is restored");
                empty.Record("complete-kit","OPONG",130,PreparednessContent.SupplyCount,250);
                var complete=new LeaderboardStore(path);
                Check(complete.Entries.Count==1 && complete.Entries[0].supplies==9,"all nine supply types persist after reopening");
            }
            finally { if(Directory.Exists(folder)) Directory.Delete(folder,true); }
        }

        private static void ChooseSafeLane(GameManager game)
        {
            TrackEntity nearest=null;
            foreach(TrackEntity entity in game.Spawner.Active)
            {
                float z=entity.transform.position.z;
                if(entity.SpansAllLanes && z>0 && z<game.Speed*.65f+.9f) { game.Duck(); return; }
                if(entity.IsPickup || z<0 || z>17 || entity.Lane!=game.Player.Lane) continue;
                if(nearest==null || z<nearest.transform.position.z) nearest=entity;
            }
            if(nearest==null) return;
            for(int lane=0;lane<3;lane++)
            {
                bool clear=true;
                foreach(TrackEntity entity in game.Spawner.Active)
                    if(!entity.IsPickup && entity.Lane==lane && Mathf.Abs(entity.transform.position.z-nearest.transform.position.z)<4) clear=false;
                if(clear && lane!=game.Player.Lane) { game.Move(lane>game.Player.Lane?1:-1); return; }
            }
        }
    }
}
