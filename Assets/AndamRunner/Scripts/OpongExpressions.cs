using UnityEngine;

namespace AndamRunner
{
    public enum OpongMood { Ready,Focused,Surprised,Determined,Worried,Hurt,Proud }

    // Facial joints are part of the single skinned character mesh.
    public sealed class OpongExpressions : MonoBehaviour
    {
        private Transform leftEye,rightEye,leftBrow,rightBrow,smile,surprised,concern;
        private float blinkAt,blinkStart=-10;
        public OpongMood Mood { get; private set; }
        private void Awake() { Initialize(); }
        public void Initialize()
        {
            Transform head=transform.Find("Opong storybook character/Spiky hair + headband + earrings");
            if(head==null) return;
            leftEye=head.Find("Eye L"); rightEye=head.Find("Eye R"); leftBrow=head.Find("Brow L"); rightBrow=head.Find("Brow R");
            smile=head.Find("Smile"); surprised=head.Find("Surprised mouth"); concern=head.Find("Concerned mouth");
            blinkAt=Time.unscaledTime+2.8f; Apply(OpongMood.Ready,false);
        }
        public void Apply(OpongMood mood,bool allowBlink=true)
        {
            if(leftEye==null) return;
            Mood=mood;
            float now=Time.unscaledTime;
            if(allowBlink && now>=blinkAt) { blinkStart=now; blinkAt=now+3.1f; }
            float blink=allowBlink ? Mathf.Sin(Mathf.Clamp01((now-blinkStart)/.16f)*Mathf.PI) : 0;
            float eyeHeight=mood==OpongMood.Hurt ? .55f : mood==OpongMood.Focused || mood==OpongMood.Determined ? .90f : 1;
            Vector3 eyes=new Vector3(1,Mathf.Max(.08f,eyeHeight*(1-blink)),1); leftEye.localScale=rightEye.localScale=eyes;
            float tilt=mood==OpongMood.Determined || mood==OpongMood.Focused ? 11 : mood==OpongMood.Worried || mood==OpongMood.Hurt ? -14 : 0;
            leftBrow.localRotation=Quaternion.Euler(0,0,-tilt); rightBrow.localRotation=Quaternion.Euler(0,0,tilt);
            bool open=mood==OpongMood.Surprised || mood==OpongMood.Hurt;
            bool worried=mood==OpongMood.Worried;
            smile.localScale=open || worried ? Vector3.one*.001f : new Vector3(mood==OpongMood.Proud?1.15f:1,1,1);
            surprised.localScale=open ? Vector3.one : Vector3.one*.001f;
            concern.localScale=worried ? Vector3.one : Vector3.one*.001f;
        }
    }
}
