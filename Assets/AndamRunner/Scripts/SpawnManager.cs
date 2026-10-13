using System.Collections.Generic;
using UnityEngine;

namespace AndamRunner
{
    public sealed class SpawnManager : MonoBehaviour
    {
        public const int MaximumActiveEntities=80;
        public const float ReactionTime=.75f, MinimumPowerUpGap=400f, MaximumPowerUpGap=550f;
        // Reserve the longest action, preparation for the next jump and time to react.
        public static float ObstacleRecoveryTime => Mathf.Max(PlayerRunner.JumpDuration,PlayerRunner.DuckDuration)+PlayerRunner.JumpDuration*.5f+ReactionTime;
        private readonly List<TrackEntity> pool = new List<TrackEntity>(192);
        private readonly List<TrackEntity> active = new List<TrackEntity>(MaximumActiveEntities);
        private System.Random random;
        private System.Random powerRandom;
        private GameManager manager;
        private float untilNextRow;
        private int previousSafeLane = 1;
        private int rows;
        private int distractorRows;
        private int obstacleGroups;
        private float lastObstacleEndDistance;
        private float nextPowerUpDistance;
        private bool firstPowerUp;
        private readonly int[] safeChoices=new int[3];
        public int ActiveCount => active.Count;
        public IReadOnlyList<TrackEntity> Active => active;

        public void Initialize(GameManager game)
        {
            manager=game;
            // Prewarm pickups for dry routes and one raised jump reward; no phone-time allocation.
            for (int lane = 0; lane < 3; lane++)
            {
                for (int kind = 0; kind < PreparednessContent.SupplyCount; kind++)
                    for (int copy = 0; copy < 3; copy++)
                    {
                        Transform root = PrimitiveArt.Root("Pooled supply",transform,Vector3.zero);
                        TrackEntity entity = root.gameObject.AddComponent<TrackEntity>();
                        entity.ConfigureSupply((SupplyKind)kind,lane,PrimitiveArt.SupplyArt(root,(SupplyKind)kind));
                        pool.Add(entity);
                    }
                for(int kind=0;kind<PreparednessContent.PowerUpCount;kind++)
                    for(int copy=0;copy<3;copy++)
                    {
                        Transform root=PrimitiveArt.Root("Pooled power-up",transform,Vector3.zero);
                        TrackEntity entity=root.gameObject.AddComponent<TrackEntity>();
                        entity.ConfigurePowerUp((PowerUpKind)kind,lane,RunnerReferenceArt.PowerUp(root,(PowerUpKind)kind));
                        pool.Add(entity);
                    }
                for(int kind=0;kind<PreparednessContent.DistractorCount;kind++)
                    for(int copy=0;copy<2;copy++)
                    {
                        Transform root=PrimitiveArt.Root("Pooled distractor",transform,Vector3.zero);
                        TrackEntity entity=root.gameObject.AddComponent<TrackEntity>();
                        entity.ConfigureDistractor((DistractorKind)kind,lane,RunnerReferenceArt.Distractor(root,(DistractorKind)kind));
                        pool.Add(entity);
                    }
                for (int kind = 0; kind < PreparednessContent.HazardCount; kind++)
                {
                    if(kind==(int)HazardKind.HangingCable) continue;
                    for (int copy = 0; copy < 2; copy++)
                    {
                        Transform root = PrimitiveArt.Root("Pooled hazard",transform,Vector3.zero);
                        TrackEntity entity = root.gameObject.AddComponent<TrackEntity>();
                        entity.ConfigureHazard((HazardKind)kind,lane,PrimitiveArt.HazardArt(root,(HazardKind)kind));
                        pool.Add(entity);
                    }
                }
            }
            // Full-width cables have one entity, preventing three penalties for one obstacle.
            for(int copy=0;copy<2;copy++)
            {
                Transform root=PrimitiveArt.Root("Pooled full-width hanging cable",transform,Vector3.zero);
                TrackEntity entity=root.gameObject.AddComponent<TrackEntity>();
                entity.ConfigureHazard(HazardKind.HangingCable,1,PrimitiveArt.HazardArt(root,HazardKind.HangingCable));
                pool.Add(entity);
            }
            ResetTrack();
        }

        public void ResetTrack()
        {
            foreach (TrackEntity entity in pool) entity.gameObject.SetActive(false);
            active.Clear(); random = new System.Random(7351); powerRandom=new System.Random(9241); firstPowerUp=true; untilNextRow = 8f;
            previousSafeLane = 1; rows = 0; distractorRows=0; obstacleGroups=1;
            lastObstacleEndDistance=float.NegativeInfinity; nextPowerUpDistance=RunPowerUps.UnlockDistance;
            SpawnSupply(SupplyKind.GoBag,1,18f);
            SpawnSupply(SupplyKind.Water,0,30f);
            SpawnHazard(HazardKind.Floodwater,2,36f);
            SpawnSupply(SupplyKind.FirstAid,1,44f);
            SpawnSupply(SupplyKind.ContactCard,2,58f);
        }

        #if UNITY_EDITOR
        // Isolate an obstacle check from tutorial rows when designers change the run speed.
        public void ClearForChecks()
        {
            foreach(TrackEntity entity in pool) entity.gameObject.SetActive(false);
            active.Clear(); untilNextRow=10000f; lastObstacleEndDistance=float.NegativeInfinity;
        }
        #endif

        public void Tick(float delta, float speed, PlayerRunner player, GameManager game, Vector3 playerFrom,float duckBefore=0)
        {
            float step = delta * speed;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                TrackEntity entity = active[i];
                float fromZ = entity.transform.position.z;
                float toZ = fromZ - step;
                entity.transform.position += Vector3.back * step;
                if(entity.IsSupply && game.PowerUps.MagnetActive &&
                    entity.transform.position.z>=-1 && (entity.transform.position-player.transform.position).sqrMagnitude<=RunPowerUps.MagnetRadius*RunPowerUps.MagnetRadius)
                    entity.transform.position=Vector3.MoveTowards(entity.transform.position,player.transform.position,32f*delta);
                toZ=entity.transform.position.z;
                entity.Animate(Time.time);
                if(entity.SpansAllLanes && !entity.Warned && toZ<speed*.65f+.9f && toZ>0)
                {
                    entity.Warned=true; game.Feedback("LOW CABLE ACROSS ALL LANES\nSwipe DOWN or tap DUCK now!",false);
                }
                if(!entity.IsPickup && entity.Hazard==HazardKind.FloodStreak && !entity.Warned &&
                    entity.Lane==player.Lane && toZ<entity.HalfDepth+speed*1.2f && toZ>entity.HalfDepth)
                {
                    entity.Warned=true;
                    game.Feedback("LONG FLOOD AHEAD\nChange lanes! Too long to jump.",false);
                }
                if (!entity.IsPickup && !entity.Warned && toZ < 22f && toZ > 8f &&
                    entity.Lane == player.Lane && (entity.Hazard == HazardKind.Floodwater || entity.Hazard == HazardKind.PowerLine))
                {
                    entity.Warned = true;
                    game.Feedback(PreparednessContent.HazardNames[(int)entity.Hazard] + "\nSwipe UP to jump or change lanes!", false);
                }
                if (entity.IsContact(playerFrom,player.transform.position,fromZ,toZ,duckBefore,delta))
                {
                    // Disable immediately even during the brief grace period: never score the same object twice.
                    entity.Consume();
                    if(entity.IsSupply) game.Collect(entity.Supply);
                    else if(entity.IsDistractor) game.CollectDistractor(entity.Distractor);
                    else if(entity.IsPowerUp) game.CollectPowerUp(entity.PowerUp);
                    else game.Hit(entity.Hazard);
                    active.RemoveAt(i);
                    if (game.State != RunState.Playing) return;
                }
                else if (toZ < -Mathf.Max(8f,entity.HalfDepth+2f))
                {
                    entity.Consume(); active.RemoveAt(i);
                }
            }
            untilNextRow -= step;
            // Keep overshoot, so spawning density does not depend on frame rate.
            while (untilNextRow <= 0f)
            {
                SpawnRow(78f + untilNextRow);
                untilNextRow += 13.5f;
            }
        }

        private void SpawnRow(float z)
        {
            rows++;
            int nextGroup=obstacleGroups+1;
            bool cableRow=nextGroup%5==0;
            bool floodRow=!cableRow && nextGroup%7==0;
            float halfDepth=floodRow ? PreparednessContent.FloodStreakLength*.5f : cableRow ? .9f : 2.4f;
            bool obstacleRow=CanPlaceObstacle(z,halfDepth);
            // Long floods reserve their whole footprint; a recovery interval separates
            // every obstacle group, including full-width cables and the flood's tail.
            bool newFlood=false;
            if(obstacleRow && floodRow)
            {
                int first=random.Next(3);
                for(int i=0;i<3;i++)
                {
                    int lane=(first+i)%3;
                    if(!CanStartFlood(lane,z)) continue;
                    newFlood=SpawnHazard(HazardKind.FloodStreak,lane,z)!=null;
                    if(newFlood) break;
                }
            }
            // A guaranteed safe lane, at most one lane from the previous safe route.
            int low = Mathf.Max(0,previousSafeLane-1), high = Mathf.Min(2,previousSafeLane+1);
            int count=0;
            for(int lane=low;lane<=high;lane++) if(!FloodedAt(lane,z)) safeChoices[count++]=lane;
            if(count==0) return;
            int safe = safeChoices[random.Next(count)];
            previousSafeLane = safe;
            // Cycle every useful kind, including cash and food, before randomizing.
            SupplyKind supply = rows <= PreparednessContent.SupplyCount ? (SupplyKind)((rows+2)%PreparednessContent.SupplyCount) : (SupplyKind)random.Next(PreparednessContent.SupplyCount);
            SpawnSupply(supply,safe,z);
            if(manager.Distance>=nextPowerUpDistance)
            {
                PowerUpKind kind=firstPowerUp ? PowerUpKind.DoubleScore : RunPowerUps.Choose(powerRandom);
                if(!FloodedAt(safe,z+4f) && SpawnPowerUp(kind,safe,z+4f)!=null)
                {
                    firstPowerUp=false;
                    nextPowerUpDistance=manager.Distance+MinimumPowerUpGap+(float)powerRandom.NextDouble()*(MaximumPowerUpGap-MinimumPowerUpGap);
                }
            }
            if(newFlood) { obstacleGroups=nextGroup; return; }
            if(obstacleRow && cableRow)
            {
                if(SpawnHazard(HazardKind.HangingCable,1,z)!=null) obstacleGroups=nextGroup;
                return;
            }
            // The correct supply keeps its clear lane; distractors occupy another lane.
            // Cycle all five so every requested object appears, without cable-row conflicts.
            if(rows%3==0)
            {
                int lane=(safe+1)%3;
                if(FloodedAt(lane,z)) lane=(safe+2)%3;
                SpawnDistractor((DistractorKind)(distractorRows++%PreparednessContent.DistractorCount),lane,z);
            }
            if(obstacleRow && !floodRow && rows%3!=0)
            {
                int hazardLane = (safe + (random.Next(2)==0 ? 1 : 2)) % 3;
                bool spawned=!FloodedAt(hazardLane,z) && SpawnHazard((HazardKind)random.Next(4),hazardLane,z)!=null;
                int otherLane=3-safe-hazardLane;
                if(rows>7 && rows%4==0 && !FloodedAt(otherLane,z))
                    spawned=SpawnHazard((HazardKind)random.Next(4),otherLane,z)!=null || spawned;
                if(spawned) obstacleGroups=nextGroup;
            }
        }

        private bool CanPlaceObstacle(float z,float halfDepth)
        {
            // Use the highest reachable speed, so a future milestone cannot shorten
            // the reserved recovery time. Keep this reservation after a hazard is hit.
            return manager.Distance+z-halfDepth>=lastObstacleEndDistance+RunPace.TopSpeed*ObstacleRecoveryTime;
        }

        private bool FloodedAt(int lane,float z)
        {
            foreach(TrackEntity entity in active)
                if(!entity.IsPickup && !entity.Consumed && entity.Hazard==HazardKind.FloodStreak && entity.Lane==lane &&
                    Mathf.Abs(entity.transform.position.z-z)<=entity.HalfDepth+2.4f) return true;
            return false;
        }

        private bool CanStartFlood(int lane,float z)
        {
            foreach(TrackEntity entity in active)
                if(!entity.Consumed && (entity.Lane==lane || entity.SpansAllLanes) &&
                    Mathf.Abs(entity.transform.position.z-z)<=PreparednessContent.FloodStreakLength*.5f+entity.HalfDepth) return false;
            return true;
        }

        public TrackEntity SpawnSupply(SupplyKind kind, int lane, float z,float height=0) { return Spawn(true,(int)kind,lane,z,false,height); }
        public TrackEntity SpawnDistractor(DistractorKind kind,int lane,float z) { return Spawn(false,(int)kind,lane,z,true); }
        public TrackEntity SpawnHazard(HazardKind kind, int lane, float z)
        {
            TrackEntity hazard=Spawn(false,(int)kind,kind==HazardKind.HangingCable?1:lane,z);
            if(hazard!=null) lastObstacleEndDistance=Mathf.Max(lastObstacleEndDistance,manager.Distance+z+hazard.HalfDepth);
            if(hazard!=null && kind!=HazardKind.HangingCable && kind!=HazardKind.FloodStreak)
            {
                SpawnSupply((SupplyKind)random.Next(PreparednessContent.SupplyCount),lane,z,2.2f);
            }
            return hazard;
        }
        public TrackEntity SpawnPowerUp(PowerUpKind kind,int lane,float z)
        {
            if(manager.Distance<RunPowerUps.UnlockDistance) return null;
            foreach(TrackEntity entity in pool)
                if(!entity.gameObject.activeSelf && entity.IsPowerUp && entity.PowerUp==kind && entity.Lane==lane)
                { entity.Spawn(z); active.Add(entity); return entity; }
            return null;
        }
        private TrackEntity Spawn(bool supply, int kind, int lane, float z,bool distractor=false,float height=0)
        {
            foreach (TrackEntity entity in pool)
            {
                if (entity.gameObject.activeSelf || entity.IsPowerUp || entity.IsSupply != supply || entity.IsDistractor!=distractor || entity.Lane != lane) continue;
                if ((supply ? (int)entity.Supply : distractor ? (int)entity.Distractor : (int)entity.Hazard) != kind) continue;
                entity.Spawn(z,height); active.Add(entity); return entity;
            }
            // A busy pool skips the object rather than allocating on a phone.
            return null;
        }
    }
}
