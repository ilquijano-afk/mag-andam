using UnityEngine;

namespace AndamRunner
{
    // Clip-driven shield riding. Gameplay height/collisions remain owned by PlayerRunner.
    public sealed class OpongAnimation : MonoBehaviour
    {
        private Animator animator;
        private OpongExpressions face;
        private Transform[] streaks;
        private GameManager game;
        private PlayerRunner runner;
        private bool wasJumping;
        private float hitUntil,landUntil,pickupUntil,distractorUntil;
        private string current;
        public void Initialize(PlayerRunner player)
        {
            runner=player; game=player.GetComponentInParent<GameManager>();
            animator=GetComponent<Animator>(); face=GetComponent<OpongExpressions>();
            if(face!=null) face.Initialize();
            streaks=new Transform[4];
            for(int i=0;i<streaks.Length;i++) streaks[i]=PrimitiveArt.Box(transform,"Shield motion streak",new Vector3(i%2==0?-.40f:.40f,.05f,-1.7f),new Vector3(.04f,.01f,1),PreparednessContent.Mint);
            ResetPose();
        }
        public void ResetPose()
        {
            wasJumping=false; hitUntil=landUntil=pickupUntil=distractorUntil=0; current=null;
            if(animator!=null) { animator.speed=1; animator.Rebind(); Play("Idle",false); animator.Update(0); }
            if(face!=null) face.Apply(OpongMood.Ready,false);
        }
        public void ReactToHit() { hitUntil=Time.unscaledTime+.42f; }
        public void ReactToPickup() { pickupUntil=Time.unscaledTime+.28f; }
        public void ReactToDistractor() { distractorUntil=Time.unscaledTime+.60f; }
        private void Play(string clip,bool blend=true)
        {
            if(animator==null || animator.runtimeAnimatorController==null || current==clip) return;
            current=clip;
            if(blend) animator.CrossFadeInFixedTime(clip,.07f); else animator.Play(clip,0,0);
        }
        private void Update()
        {
            if(game==null) return;
            bool paused=game.State==RunState.Paused,riding=game.State==RunState.Playing;
            if(animator!=null) animator.speed=paused ? 0 : 1;
            if(paused) { foreach(Transform streak in streaks) streak.gameObject.SetActive(false); return; }
            float now=Time.unscaledTime;
            if(wasJumping && !runner.IsJumping && riding) landUntil=now+.16f;
            wasJumping=runner.IsJumping;
            string clip="Idle"; OpongMood mood=OpongMood.Ready;
            if(game.State==RunState.Results) { clip="Victory"; mood=OpongMood.Proud; }
            else if(riding)
            {
                clip="Run Loop"; mood=OpongMood.Focused;
                if(now<pickupUntil) mood=OpongMood.Proud;
                if(now<distractorUntil) mood=OpongMood.Worried;
                if(now<landUntil) { clip="Land"; mood=OpongMood.Determined; }
                if(runner.IsJumping) { clip=runner.JumpProgress<.16f ? "Jump Start" : "Air"; mood=OpongMood.Determined; }
                if(runner.IsDucking) { clip="Slide Duck"; mood=OpongMood.Determined; }
                if(now<hitUntil) { clip="Hit"; mood=OpongMood.Hurt; }
            }
            Play(clip); if(face!=null) face.Apply(mood);
            for(int i=0;i<streaks.Length;i++)
            {
                streaks[i].gameObject.SetActive(riding && runner.Height<.2f);
                if(!riding) continue;
                float phase=Mathf.Repeat(Time.time*2.5f+(i/2)*.5f,1);
                streaks[i].localPosition=new Vector3(i%2==0?-.42f:.42f,.045f,-1.25f-phase*2.1f);
                streaks[i].localScale=new Vector3(.045f*(1-phase),.012f,.8f*(1-phase));
            }
        }
        public void EvaluatePreview(string clip,float normalizedTime=0,OpongMood mood=OpongMood.Ready)
        {
            if(animator!=null) { animator.speed=1; animator.Play(clip,0,normalizedTime); animator.Update(0); }
            if(face!=null) face.Apply(mood,false);
        }
    }
}
