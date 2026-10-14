using UnityEngine;

namespace AndamRunner
{
    public sealed class PlayerRunner : MonoBehaviour
    {
        public const float LaneWidth = 2.6f;
        public const float JumpDuration = 0.92f;
        public const float JumpHeight = 2.3f;
        public const float DuckDuration = 1.1f;
        public int Lane { get; private set; } = 1;
        public float Height { get; private set; }
        public bool IsJumping => jumpTime >= 0f;
        public float JumpProgress => IsJumping ? jumpTime/JumpDuration : 0;
        public bool IsDucking => DuckRemaining>0;
        public float DuckRemaining { get; private set; }
        public Transform Visual { get; private set; }
        private float jumpTime = -1f;
        private float lean;
        private float roadPitch;
        public void SetRoadSlope(float slope) { roadPitch=-Mathf.Atan(slope)*Mathf.Rad2Deg*.65f; }

        public void Initialize()
        {
            GameObject prefab=Resources.Load<GameObject>("OpongStorybook");
            Visual=prefab!=null ? Instantiate(prefab,transform,false).transform : PrimitiveArt.CreateOpong(transform);
            Visual.gameObject.AddComponent<OpongAnimation>().Initialize(this);
            ResetRunner();
        }

        public void ResetRunner()
        {
            Lane = 1;
            jumpTime = -1f;
            DuckRemaining=0;
            Height = 0f;
            lean = 0f;
            roadPitch=0;
            transform.position = Vector3.zero;
            if (Visual != null)
            {
                Visual.localRotation = Quaternion.identity;
                OpongAnimation animation=Visual.GetComponent<OpongAnimation>(); if(animation!=null) animation.ResetPose();
            }
        }

        public void ChangeLane(int direction) { Lane = Mathf.Clamp(Lane + direction, 0, 2); }
        public void Jump() { if (!IsJumping && !IsDucking) jumpTime = 0f; }
        public void Duck() { if (!IsJumping && !IsDucking) DuckRemaining=DuckDuration; }

        public void Tick(float delta, bool hurt)
        {
            float target = (Lane - 1) * LaneWidth;
            float x = Mathf.MoveTowards(transform.position.x, target, 15f * delta);
            DuckRemaining=Mathf.Max(0,DuckRemaining-delta);
            if (IsJumping)
            {
                jumpTime += delta;
                if (jumpTime >= JumpDuration) { jumpTime = -1f; Height = 0f; }
                else Height = Mathf.Sin(jumpTime / JumpDuration * Mathf.PI) * JumpHeight;
            }
            // The board rides on the dry road. Analytic height prevents falling through it.
            transform.position = new Vector3(x, Height, 0f);
            float wantedLean = Mathf.Clamp((target - x) * -10f, -16f, 16f);
            lean = Mathf.Lerp(lean, wantedLean, 1f - Mathf.Exp(-12f * delta));
            float bob = Mathf.Sin(Time.time * 7f) * 0.025f;
            Visual.localPosition = new Vector3(0f, bob, 0f);
            Visual.localRotation = Quaternion.Euler(roadPitch, 0f, lean);
            // Visible feedback uses whole-model blinking, not a new material per frame.
            Visual.gameObject.SetActive(!hurt || Mathf.FloorToInt(Time.time * 12f) % 2 == 0);
        }

        public void Show() { if (Visual != null) Visual.gameObject.SetActive(true); }
    }
}
