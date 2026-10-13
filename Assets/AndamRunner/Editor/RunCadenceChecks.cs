using System;
using System.Collections.Generic;
using System.IO;
using AndamRunner;
using UnityEngine;

namespace AndamRunnerEditor
{
    public static class RunCadenceChecks
    {
        private sealed class ObstacleGroup
        {
            public float Distance, HalfDepth;
            public bool Cable, Flood;
        }
        private static int assertions;
        private static readonly float[] Frames={1f/15,1f/30,1f/60,1f/120};
        private static void Check(bool condition,string message)
        { assertions++; if(!condition) throw new Exception("Run cadence check failed: "+message); }

        public static void Run(GameManager game)
        {
            assertions=0;
            float duckJumpGap=float.PositiveInfinity, jumpDuckGap=float.PositiveInfinity, jumpJumpGap=float.PositiveInfinity;
            foreach(float frame in Frames)
            {
                List<ObstacleGroup> groups=GatherNaturalGroups(game,frame);
                bool cable=false,flood=false;
                for(int i=1;i<groups.Count;i++)
                {
                    ObstacleGroup before=groups[i-1], after=groups[i];
                    float gap=after.Distance-before.Distance;
                    float clearTime=(gap-before.HalfDepth-after.HalfDepth)/RunPace.TopSpeed;
                    Check(clearTime>=SpawnManager.ObstacleRecoveryTime-.01f,"actual generated footprints leave recovery time at top speed");
                    cable|=after.Cable; flood|=after.Flood;
                    if(before.Cable && !after.Cable && !after.Flood) duckJumpGap=Mathf.Min(duckJumpGap,gap);
                    if(!before.Cable && !before.Flood && after.Cable) jumpDuckGap=Mathf.Min(jumpDuckGap,gap);
                    if(!before.Cable && !before.Flood && !after.Cable && !after.Flood) jumpJumpGap=Mathf.Min(jumpJumpGap,gap);
                }
                Check(cable && flood,"paced route still includes hanging cables and long floods");
            }
            Check(!float.IsPositiveInfinity(duckJumpGap) && !float.IsPositiveInfinity(jumpDuckGap) && !float.IsPositiveInfinity(jumpJumpGap),"natural route exercises all action transitions");
            foreach(int level in new[]{0,RunPace.MaximumLevel})
                for(int lane=0;lane<3;lane++)
                    for(int kind=0;kind<4;kind++)
                        foreach(float frame in Frames)
                        {
                            ReplayPair(game,HazardKind.HangingCable,(HazardKind)kind,duckJumpGap,level,lane,frame);
                            ReplayPair(game,(HazardKind)kind,HazardKind.HangingCable,jumpDuckGap,level,lane,frame);
                            ReplayPair(game,(HazardKind)kind,(HazardKind)kind,jumpJumpGap,level,lane,frame);
                        }
            File.WriteAllText("Docs/Run-Cadence-Validation.txt","Unity "+Application.unityVersion+" on "+DateTime.UtcNow.ToString("u")+"\n"+
                assertions+" assertions passed with isolated scores.\n"+
                "Natural routes over 4,000 meters at 15/30/60/120 fps keep obstacle footprints separated by action completion, next-jump preparation and 0.75 seconds of reaction time at the highest reachable speed.\n"+
                "288 duck-to-jump, jump-to-duck and jump-to-jump sequences replay the shortest naturally generated gaps across all three lanes, four ground obstacles, base/top speed and four frame rates. Late valid ducks still leave time to jump; all sequences preserve three lives and execute the second action.\n"+
                "Natural rows give exactly one raised supply per regular ground hazard, none over long floods or hanging cables. Cable/flood variety, safe routes and bounded pools are retained.\n");
            Debug.Log("ANDAM_CADENCE_PASS: "+assertions+" natural spacing and action transition assertions.");
            game.StartRun();
        }

        private static List<ObstacleGroup> GatherNaturalGroups(GameManager game,float frame)
        {
            game.StartRun(); game.Player.transform.position=new Vector3(100,0,0);
            var lastZ=new Dictionary<TrackEntity,float>(); var groups=new List<ObstacleGroup>();
            while(game.Distance<4000)
            {
                foreach(TrackEntity hazard in game.Spawner.Active)
                {
                    if(hazard.IsPickup) continue;
                    float z=hazard.transform.position.z;
                    bool spawned=!lastZ.TryGetValue(hazard,out float previous) || z>previous+.1f;
                    lastZ[hazard]=z;
                    if(!spawned) continue;
                    int rewards=0;
                    foreach(TrackEntity item in game.Spawner.Active)
                        if(item.IsJumpReward && item.Lane==hazard.Lane && Mathf.Abs(item.transform.position.z-z)<.1f) rewards++;
                    bool cable=hazard.Hazard==HazardKind.HangingCable, flood=hazard.Hazard==HazardKind.FloodStreak;
                    Check(rewards==(!cable && !flood ? 1 : 0),"one raised supply over each natural jump hazard only");
                    float world=game.Distance+z;
                    ObstacleGroup group=groups.Count>0 && Mathf.Abs(groups[groups.Count-1].Distance-world)<.1f ? groups[groups.Count-1] : null;
                    if(group==null) { group=new ObstacleGroup{Distance=world}; groups.Add(group); }
                    group.HalfDepth=Mathf.Max(group.HalfDepth,hazard.HalfDepth); group.Cable|=cable; group.Flood|=flood;
                }
                float speed=game.Speed;
                game.Pace.Advance(speed*frame);
                game.Spawner.Tick(frame,speed,game.Player,game,game.Player.transform.position);
                Check(game.Spawner.ActiveCount<=SpawnManager.MaximumActiveEntities,"paced spawning stays bounded");
            }
            Check(groups.Count>=20,"route continues to offer obstacles without crowding them");
            return groups;
        }

        private static void ReplayPair(GameManager game,HazardKind first,HazardKind second,float gap,int level,int lane,float frame)
        {
            game.StartRun(); game.Spawner.ClearForChecks(); game.Pace.Advance(level*RunPace.Milestone);
            game.Player.ChangeLane(lane-1); game.Player.Tick(.2f,false);
            float speed=game.Speed, firstCenter=1f, secondCenter=firstCenter+gap/speed;
            game.Spawner.SpawnHazard(first,lane,speed*firstCenter);
            game.Spawner.SpawnHazard(second,lane,speed*secondCenter);
            // Exercise a late, valid duck: its full timer must finish before the next jump.
            float firstInput=firstCenter-(first==HazardKind.HangingCable?.10f:PlayerRunner.JumpDuration*.5f);
            float secondInput=secondCenter-(second==HazardKind.HangingCable?.65f:PlayerRunner.JumpDuration*.5f);
            float firstFinish=firstInput+(first==HazardKind.HangingCable?PlayerRunner.DuckDuration:PlayerRunner.JumpDuration);
            Check(secondInput-firstFinish>=SpawnManager.ReactionTime-.01f,"ample reaction time after the first action");
            bool pressedFirst=false,pressedSecond=false;
            float elapsed=0;
            while(elapsed<secondCenter+1.3f)
            {
                if(!pressedFirst && elapsed>=firstInput)
                {
                    if(first==HazardKind.HangingCable) game.Duck(); else game.Jump();
                    Check(first==HazardKind.HangingCable?game.Player.IsDucking:game.Player.IsJumping,"first action accepted");
                    pressedFirst=true;
                }
                if(!pressedSecond && elapsed>=secondInput)
                {
                    Check(!game.Player.IsJumping && !game.Player.IsDucking,"first action fully recovered before the next input");
                    if(second==HazardKind.HangingCable) game.Duck(); else game.Jump();
                    Check(second==HazardKind.HangingCable?game.Player.IsDucking:game.Player.IsJumping,"second action accepted immediately");
                    pressedSecond=true;
                }
                game.Simulate(frame); elapsed+=frame;
            }
            Check(pressedFirst && pressedSecond && game.Session.Lives==3 && game.GraceRemaining==0,"both generated obstacles cleared without damage");
        }
    }
}
