using System;
using System.Collections.Generic;
using System.IO;
using AndamRunner;
using UnityEngine;

namespace AndamRunnerEditor
{
    public static class PowerUpChecks
    {
        private static int assertions;
        private static void Check(bool condition,string message)
        { assertions++; if(!condition) throw new Exception("Power-up check failed: "+message); }

        public static void Run(GameManager game)
        {
            assertions=0;
            CheckTimersAndScoring();
            CheckJumpTrails(game);
            CheckActualPickups(game);
            CheckNaturalSpawning(game);
            Directory.CreateDirectory("Docs");
            File.WriteAllText("Docs/Power-up-Validation.txt","Unity "+Application.unityVersion+" on "+DateTime.UtcNow.ToString("u")+"\n"+
                assertions+" assertions passed with isolated leaderboard storage.\n"+
                "One raised useful supply per jumpable obstacle: timed jumps collect it, standing and lane dodges cannot. All lanes, base/top speed and 30/120 fps checked.\n"+
                "Bonuses unlock at exactly 500m. 10-second timers, strongest multiplier, independent weaker timers, pause, expiry, restart and actual multiplied pickups checked. Penalties remain unchanged.\n"+
                "Magnet attracts nearby raised/adjacent supplies; distractors, obstacles, power-ups and distant supplies are excluded.\n"+
                "Natural spawning over 120,000m has no power-ups before 500m, at least 400m between bonuses, only one bonus on screen, bounded pools and the requested rarity order. Weighted selector checked over 100,000 draws.\n");
            Debug.Log("ANDAM_POWERUP_PASS: "+assertions+" jump reward, multiplier, magnet, unlock and rarity assertions.");
            game.StartRun();
        }

        private static void CheckTimersAndScoring()
        {
            var powers=new RunPowerUps();
            powers.Activate(PowerUpKind.DoubleScore); Check(powers.ScoreMultiplier==2,"2x activation");
            powers.Activate(PowerUpKind.QuadrupleScore); Check(powers.ScoreMultiplier==4,"4x replaces 2x instead of multiplying it");
            powers.Activate(PowerUpKind.OctupleScore); powers.Tick(5);
            powers.Activate(PowerUpKind.DoubleScore); powers.Activate(PowerUpKind.Magnet);
            Check(powers.ScoreMultiplier==8 && powers.MultiplierRemaining==5,"weaker pickup cannot extend 8x");
            powers.Tick(5); Check(powers.ScoreMultiplier==2 && powers.MagnetActive,"weaker bonus can outlast expired stronger bonus");
            powers.Tick(4.99f); Check(powers.ScoreMultiplier==2,"ten-second duration not shortened");
            powers.Tick(.02f); Check(powers.ScoreMultiplier==1 && !powers.MagnetActive,"all bonuses expire");
            powers.Activate(PowerUpKind.OctupleScore); powers.Reset(); Check(powers.ScoreMultiplier==1,"reset clears bonuses");
            var session=new RunSession();
            foreach(int multiplier in new[]{2,4,8}) session.Collect(SupplyKind.Water,multiplier);
            Check(session.Score==140 && session.Collected==3 && session.UniqueSupplies==1,"multiplied positive points do not multiply supply counts");
            session.CollectDistractor(DistractorKind.Toys); session.Hit(); Check(session.Score==125 && session.Lives==2,"penalties stay five and ten points");
            var counts=new int[4]; var random=new System.Random(4083);
            for(int i=0;i<100000;i++) counts[(int)RunPowerUps.Choose(random)]++;
            Check(counts[1]>counts[2] && counts[2]>counts[0] && counts[0]>counts[3],"weighted rarity: 2x, 4x, magnet, then 8x");
            Check(Math.Abs(counts[1]-70000)<1000 && Math.Abs(counts[2]-22000)<1000 && Math.Abs(counts[0]-6000)<500 && Math.Abs(counts[3]-2000)<300,"70/22/6/2 percent weights");
        }

        private static void Ready(GameManager game,float distance=0,int lane=1)
        {
            game.StartRun(); game.Spawner.ClearForChecks(); game.Pace.Advance(distance);
            game.Player.ChangeLane(lane-1); game.Player.Tick(.2f,false);
        }
        private static void Frames(GameManager game,float duration,float frame)
        {
            while(duration>0) { float step=Mathf.Min(duration,frame); duration=Mathf.Max(0,duration-step); game.Simulate(step); }
        }
        private static void CheckJumpTrails(GameManager game)
        {
            for(int kind=0;kind<4;kind++)
                for(int lane=0;lane<3;lane++)
                    foreach(int level in new[]{0,6})
                        foreach(float frame in new[]{1f/30,1f/120})
                        {
                            Ready(game,level*250,lane);
                            game.Spawner.SpawnHazard((HazardKind)kind,lane,game.Speed*.46f);
                            int count=0,expected=0;
                            foreach(TrackEntity item in game.Spawner.Active) if(item.IsJumpReward)
                            {
                                count++; expected+=PreparednessContent.Points[(int)item.Supply];
                                Check(!item.IsContact(Vector3.zero,Vector3.zero,3,-3),"ground cannot collect raised rewards");
                            }
                            Check(count==1,"exactly one air reward over each jump obstacle");
                            game.Jump(); Frames(game,1.1f,frame);
                            Check(game.Session.Collected==1 && game.Session.Score==expected && game.Session.Lives==3,"jump collects the single reward without a hazard hit");
                        }
            for(int kind=0;kind<4;kind++)
            {
                Ready(game); game.Spawner.SpawnHazard((HazardKind)kind,1,game.Speed*.46f); Frames(game,1.1f,1f/30);
                Check(game.Session.Collected==0 && game.Session.Score==0 && game.Session.Lives==2,"standing loses a life and cannot take air rewards");
                Ready(game,0,0); game.Spawner.SpawnHazard((HazardKind)kind,1,game.Speed*.46f); Frames(game,1.1f,1f/30);
                Check(game.Session.Collected==0 && game.Session.Lives==3,"dodging preserves lives but misses the jump trail");
            }
            Ready(game); game.Spawner.SpawnHazard(HazardKind.FloodStreak,0,30); game.Spawner.SpawnHazard(HazardKind.HangingCable,1,70);
            foreach(TrackEntity item in game.Spawner.Active) Check(!item.IsJumpReward,"no jump rewards over long floods or duck cables");
        }

        private static void CheckActualPickups(GameManager game)
        {
            Ready(game,499);
            Check(game.Spawner.SpawnPowerUp(PowerUpKind.OctupleScore,1,.1f)==null,"field bonus blocked before 500m");
            game.CollectPowerUp(PowerUpKind.OctupleScore); Check(game.PowerUps.ScoreMultiplier==1,"activation blocked before 500m");
            game.Pace.Advance(1);
            Check(game.Spawner.SpawnPowerUp(PowerUpKind.DoubleScore,1,.1f)!=null,"field bonus allowed at 500m");
            game.Simulate(.1f); Check(game.PowerUps.ScoreMultiplier==2 && game.Session.Collected==0 && game.Session.Score==0,"actual 2x pickup activates without pretending to be a supply");
            game.Spawner.SpawnSupply(SupplyKind.Cash,1,.1f); game.Simulate(.1f); Check(game.Session.Score==20,"actual 2x cash pickup");
            game.Spawner.SpawnPowerUp(PowerUpKind.QuadrupleScore,1,.1f); game.Simulate(.1f);
            game.Spawner.SpawnSupply(SupplyKind.FirstAid,1,.1f); game.Simulate(.1f); Check(game.Session.Score==80,"actual 4x aid pickup");
            game.Spawner.SpawnPowerUp(PowerUpKind.OctupleScore,1,.1f); game.Simulate(.1f);
            game.Spawner.SpawnSupply(SupplyKind.Food,1,.1f); game.Simulate(.1f); Check(game.Session.Score==200,"actual 8x food pickup");
            game.Spawner.SpawnDistractor(DistractorKind.Toys,1,.1f); game.Simulate(.1f); Check(game.Session.Score==195 && game.Session.Lives==3,"8x does not multiply distraction penalty");
            game.Spawner.SpawnHazard(HazardKind.RoadBlock,1,.1f); game.Simulate(.1f); Check(game.Session.Score==185 && game.Session.Lives==2,"8x does not multiply collision penalty");
            float remaining=game.PowerUps.MultiplierRemaining;
            game.TogglePause(); game.Simulate(20); Check(game.PowerUps.MultiplierRemaining==remaining,"pause freezes bonus timers");
            game.TogglePause(); game.Simulate(10.01f); Check(game.PowerUps.ScoreMultiplier==1,"actual bonus expires during play");
            Ready(game,500); game.Spawner.SpawnPowerUp(PowerUpKind.Magnet,1,.1f); game.Simulate(.1f);
            TrackEntity food=game.Spawner.SpawnSupply(SupplyKind.Food,0,7,2.2f);
            TrackEntity toy=game.Spawner.SpawnDistractor(DistractorKind.Toys,2,7);
            TrackEntity bonus=game.Spawner.SpawnPowerUp(PowerUpKind.OctupleScore,2,7);
            TrackEntity distant=game.Spawner.SpawnSupply(SupplyKind.Cash,0,30);
            game.Simulate(.3f);
            Check(food.Consumed && game.Session.Score==15 && game.Session.Collected==1,"magnet collects an adjacent raised supply");
            Check(!toy.Consumed && toy.transform.position.x==PlayerRunner.LaneWidth && game.Session.DistractorsCollected==0,"magnet excludes unwanted items");
            Check(!bonus.Consumed && bonus.transform.position.x==PlayerRunner.LaneWidth && game.PowerUps.ScoreMultiplier==1,"magnet does not collect other bonuses");
            Check(!distant.Consumed && distant.transform.position.x==-PlayerRunner.LaneWidth,"magnet range is bounded");
            game.StartRun(); Check(!game.PowerUps.MagnetActive && game.PowerUps.ScoreMultiplier==1,"restart clears actual bonuses");
        }

        private static void CheckNaturalSpawning(GameManager game)
        {
            game.StartRun(); game.Player.transform.position=new Vector3(100,0,0);
            var lastZ=new Dictionary<TrackEntity,float>(); var counts=new int[4]; bool raised=false;
            float lastPowerDistance=float.NegativeInfinity;
            for(int i=0;i<40000;i++)
            {
                game.Pace.Advance(3); game.Spawner.Tick(.1f,30,game.Player,game,game.Player.transform.position);
                Check(game.Spawner.ActiveCount<=SpawnManager.MaximumActiveEntities,"bonuses and jump rewards use bounded pools");
                int visibleBonuses=0;
                foreach(TrackEntity item in game.Spawner.Active)
                {
                    if(item.IsJumpReward) raised=true;
                    if(!item.IsPowerUp) continue;
                    visibleBonuses++;
                    Check(game.Distance>=500,"no natural power-ups before the unlock distance");
                    float z=item.transform.position.z;
                    if(!lastZ.TryGetValue(item,out float previous) || z>previous+.1f)
                    {
                        counts[(int)item.PowerUp]++;
                        float worldDistance=game.Distance+z;
                        Check(worldDistance-lastPowerDistance>=SpawnManager.MinimumPowerUpGap-.1f,"at least 400 meters between all natural bonuses");
                        if(!float.IsNegativeInfinity(lastPowerDistance))
                            Check(worldDistance-lastPowerDistance<=SpawnManager.MaximumPowerUpGap+14f,"bonus spacing remains within one supply row of the selected interval");
                        lastPowerDistance=worldDistance;
                    }
                    lastZ[item]=z;
                    foreach(TrackEntity hazard in game.Spawner.Active)
                        if(!hazard.IsPickup && !hazard.SpansAllLanes && hazard.Lane==item.Lane)
                            Check(Mathf.Abs(hazard.transform.position.z-z)>hazard.HalfDepth+item.HalfDepth,"bonus is on an open route");
                }
                Check(visibleBonuses<=1,"never crowd the route with several bonuses");
            }
            Check(raised,"natural obstacle rows offer raised rewards");
            Check(counts[1]>counts[2] && counts[2]>counts[0] && counts[0]>counts[3] && counts[3]>0,"natural field rarity matches the requested order and includes 8x");
        }
    }
}
