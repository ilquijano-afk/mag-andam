using System;

namespace AndamRunner
{
    // Plain C# state: easy to understand and test independently of the scene.
    public sealed class RunSession
    {
        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int Collected { get; private set; }
        public int UniqueSupplies { get; private set; }
        public int SupplyMask { get; private set; }
        public int DistractorsCollected { get; private set; }
        public bool IsOver => Lives <= 0;

        public RunSession() { Reset(); }

        public void Reset()
        {
            Score = 0;
            Lives = 3;
            Collected = 0;
            UniqueSupplies = 0;
            SupplyMask = 0;
            DistractorsCollected = 0;
        }

        public void Collect(SupplyKind kind)
        {
            int index = (int)kind;
            if (index < 0 || index >= PreparednessContent.SupplyCount) throw new ArgumentOutOfRangeException(nameof(kind));
            if (IsOver) return;
            Score += PreparednessContent.Points[index];
            Collected++;
            int bit = 1 << index;
            if ((SupplyMask & bit) == 0) UniqueSupplies++;
            SupplyMask |= bit;
        }

        public void Hit()
        {
            if (IsOver) return;
            Lives--;
            Score = Math.Max(0, Score - 10);
        }

        public void CollectDistractor(DistractorKind kind)
        {
            int index=(int)kind;
            if(index<0 || index>=PreparednessContent.DistractorCount) throw new ArgumentOutOfRangeException(nameof(kind));
            if(IsOver) return;
            Score=Math.Max(0,Score-PreparednessContent.DistractorPenalty);
            DistractorsCollected++;
        }
    }
}
