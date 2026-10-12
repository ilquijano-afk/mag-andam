using System.Collections.Generic;
using UnityEngine;

namespace AndamRunner
{
    public sealed class SpawnManager : MonoBehaviour
    {
        private readonly List<TrackEntity> pool = new List<TrackEntity>(110);
        private readonly List<TrackEntity> active = new List<TrackEntity>(32);
        private System.Random random;
        private float untilNextRow;
        private int previousSafeLane = 1;
        private int rows;
        private int distractorRows;
        private readonly int[] safeChoices=new int[3];
        public int ActiveCount => active.Count;
        public IReadOnlyList<TrackEntity> Active => active;

        public void Initialize()
        {
            // Two copies per kind/lane cover the visible distance without runtime Instantiate/Destroy.
            for (int lane = 0; lane < 3; lane++)
            {
                for (int kind = 0; kind < PreparednessContent.SupplyCount; kind++)
                    for (int copy = 0; copy < 2; copy++)
                    {
                        Transform root = PrimitiveArt.Root("Pooled supply",transform,Vector3.zero);
                        TrackEntity entity = root.gameObject.AddComponent<TrackEntity>();
                        entity.ConfigureSupply((SupplyKind)kind,lane,PrimitiveArt.SupplyArt(root,(SupplyKind)kind));
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
            active.Clear(); random = new System.Random(7351); untilNextRow = 8f;
            previousSafeLane = 1; rows = 0; distractorRows=0;
            SpawnSupply(SupplyKind.GoBag,1,18f);
            SpawnSupply(SupplyKind.Water,0,30f);
            SpawnHazard(HazardKind.Floodwater,2,36f);
            SpawnSupply(SupplyKind.FirstAid,1,44f);
            SpawnHazard(HazardKind.Debris,0,47f);
            SpawnSupply(SupplyKind.ContactCard,2,58f);
            SpawnHazard(HazardKind.PowerLine,1,62f);
        }

        #if UNITY_EDITOR
        // Isolate an obstacle check from tutorial rows when designers change the run speed.
        public void ClearForChecks()
        {
            foreach(TrackEntity entity in pool) entity.gameObject.SetActive(false);
            active.Clear(); untilNextRow=10000f;
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
            // Reserve the neighboring rows too: the streak must never hide a pickup or
            // close the guaranteed dry route while it occupies one lane for 32 meters.
            bool newFlood=false;
            if(rows>=5 && rows%18==5)
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
            if(newFlood) return;
            // Cable rows require a duck rather than a lane choice. Neighboring rows are clear
            // of ground hazards so the player has time to finish ducking before the next jump.
            if(rows>=9 && rows%9==0) { SpawnHazard(HazardKind.HangingCable,1,z); return; }
            if(rows>=8 && (rows%9==8 || rows%9==1)) return;
            // The correct supply keeps its clear lane; distractors occupy another lane.
            // Cycle all five so every requested object appears, without cable-row conflicts.
            if(rows%3==0)
            {
                int lane=(safe+1)%3;
                if(FloodedAt(lane,z)) lane=(safe+2)%3;
                SpawnDistractor((DistractorKind)(distractorRows++%PreparednessContent.DistractorCount),lane,z);
            }
            if (rows % 3 != 0)
            {
                int hazardLane = (safe + (random.Next(2)==0 ? 1 : 2)) % 3;
                if(!FloodedAt(hazardLane,z)) SpawnHazard((HazardKind)random.Next(4),hazardLane,z);
                int otherLane=3-safe-hazardLane;
                if (rows > 7 && rows % 4 == 0 && !FloodedAt(otherLane,z)) SpawnHazard((HazardKind)random.Next(4),otherLane,z);
            }
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

        public TrackEntity SpawnSupply(SupplyKind kind, int lane, float z) { return Spawn(true,(int)kind,lane,z); }
        public TrackEntity SpawnDistractor(DistractorKind kind,int lane,float z) { return Spawn(false,(int)kind,lane,z,true); }
        public TrackEntity SpawnHazard(HazardKind kind, int lane, float z)
        { return Spawn(false,(int)kind,kind==HazardKind.HangingCable?1:lane,z); }
        private TrackEntity Spawn(bool supply, int kind, int lane, float z,bool distractor=false)
        {
            foreach (TrackEntity entity in pool)
            {
                if (entity.gameObject.activeSelf || entity.IsSupply != supply || entity.IsDistractor!=distractor || entity.Lane != lane) continue;
                if ((supply ? (int)entity.Supply : distractor ? (int)entity.Distractor : (int)entity.Hazard) != kind) continue;
                entity.Spawn(z); active.Add(entity); return entity;
            }
            // A busy pool skips the object rather than allocating on a phone.
            return null;
        }
    }
}
