using System;
using UnityEngine;

namespace AndamRunner
{
    public sealed class RunPowerUps
    {
        public const float UnlockDistance=500f, Duration=10f, MagnetRadius=9f;
        private readonly float[] remaining=new float[PreparednessContent.PowerUpCount];
        public bool MagnetActive => Remaining(PowerUpKind.Magnet)>0;
        public int ScoreMultiplier => Remaining(PowerUpKind.OctupleScore)>0 ? 8 : Remaining(PowerUpKind.QuadrupleScore)>0 ? 4 : Remaining(PowerUpKind.DoubleScore)>0 ? 2 : 1;
        public float MultiplierRemaining => ScoreMultiplier==8 ? Remaining(PowerUpKind.OctupleScore) : ScoreMultiplier==4 ? Remaining(PowerUpKind.QuadrupleScore) : ScoreMultiplier==2 ? Remaining(PowerUpKind.DoubleScore) : 0;
        public float Remaining(PowerUpKind kind) => remaining[(int)kind];
        public void Reset() { Array.Clear(remaining,0,remaining.Length); }
        public void Activate(PowerUpKind kind)
        {
            int index=(int)kind;
            if(index<0 || index>=remaining.Length) throw new ArgumentOutOfRangeException(nameof(kind));
            remaining[index]=Duration;
        }
        public void Tick(float delta)
        {
            if(delta<=0) return;
            for(int i=0;i<remaining.Length;i++) remaining[i]=Mathf.Max(0,remaining[i]-delta);
        }
        // Conditional weights when a bonus row is chosen: 70% 2x, 22% 4x, 6% magnet, 2% 8x.
        public static PowerUpKind Choose(System.Random random)
        {
            int roll=random.Next(1000);
            return roll<700 ? PowerUpKind.DoubleScore : roll<920 ? PowerUpKind.QuadrupleScore : roll<980 ? PowerUpKind.Magnet : PowerUpKind.OctupleScore;
        }
    }
}
