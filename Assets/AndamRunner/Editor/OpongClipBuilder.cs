using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AndamRunnerEditor
{
    // Generic transform clips for the weighted chibi rig; shield riding retains planted feet.
    public static class OpongClipBuilder
    {
        private const string Body="Opong storybook character";
        private const string Head=Body+"/Spiky hair + headband + earrings";
        public static RuntimeAnimatorController Build(string folder)
        {
            string animations=folder+"/Animations"; Directory.CreateDirectory(animations); AssetDatabase.Refresh();
            string path=animations+"/OpongAndam.controller";
            AnimatorController controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if(controller==null) controller=AnimatorController.CreateAnimatorControllerAtPath(path);
            AnimatorStateMachine machine=controller.layers[0].stateMachine;
            foreach(ChildAnimatorState state in machine.states) machine.RemoveState(state.state);
            string[] names={"Idle","Run Loop","Jump Start","Air","Land","Slide Duck","Hit","Victory"};
            foreach(string name in names)
            {
                float duration=name=="Idle"?2f:name=="Run Loop"?.8f:name=="Jump Start"?.15f:name=="Land"?.18f:name=="Hit"?.42f:name=="Victory"?1.1f:.6f;
                bool loop=name=="Idle" || name=="Run Loop" || name=="Air" || name=="Victory";
                var clip=new AnimationClip{name=name,frameRate=30};
                bool idle=name=="Idle" || name=="Victory";
                Constant(clip,Body,"localPosition.x",0,duration); Constant(clip,Body,"localPosition.y",.29f,duration); Constant(clip,Body,"localPosition.z",0,duration);
                Constant(clip,Body,"localScale.x",1,duration); Constant(clip,Body,"localScale.y",1,duration); Constant(clip,Body,"localScale.z",1,duration);
                Constant(clip,Body,"localEulerAnglesRaw.x",idle?0:-4,duration); Constant(clip,Body,"localEulerAnglesRaw.y",idle?0:-35,duration);
                Wave(clip,Body,"localEulerAnglesRaw.z",0,idle?1.5f:2.6f,duration);
                Wave(clip,Head,"localEulerAnglesRaw.y",idle?-4:8,idle?4:2,duration);
                Constant(clip,Head,"localEulerAnglesRaw.x",0,duration); Constant(clip,Head,"localEulerAnglesRaw.z",0,duration);
                Constant(clip,Head,"localPosition.x",0,duration); Constant(clip,Head,"localPosition.y",1.88f,duration); Constant(clip,Head,"localPosition.z",0,duration);
                Constant(clip,Head,"localScale.x",1,duration); Constant(clip,Head,"localScale.y",1,duration); Constant(clip,Head,"localScale.z",1,duration);
                foreach(int side in new[]{-1,1})
                {
                    string arm=Body+(side<0?"/Arm L":"/Arm R"); string leg=Body+(side<0?"/Leg L":"/Leg R");
                    Wave(clip,arm,"localEulerAnglesRaw.z",side*(idle?28:42),side*(idle?3:6),duration);
                    Constant(clip,arm,"localEulerAnglesRaw.x",idle?0:-5,duration); Constant(clip,arm,"localEulerAnglesRaw.y",0,duration);
                    Wave(clip,leg,"localEulerAnglesRaw.x",0,idle?0:side*2.5f,duration);
                    Constant(clip,leg,"localEulerAnglesRaw.y",0,duration); Constant(clip,leg,"localEulerAnglesRaw.z",0,duration);
                    if(name=="Air" || name=="Jump Start") Constant(clip,arm,"localEulerAnglesRaw.z",side*70,duration);
                    if(name=="Victory") Wave(clip,arm,"localEulerAnglesRaw.z",side*125,side*10,duration);
                    if(name=="Hit") Constant(clip,arm,"localEulerAnglesRaw.z",side*78,duration);
                    if(name=="Slide Duck") Constant(clip,arm,"localEulerAnglesRaw.z",side*20,duration);
                }
                if(name=="Idle" || name=="Run Loop") Wave(clip,Body,"localPosition.y",.29f,.012f,duration);
                if(name=="Jump Start" || name=="Land") Pulse(clip,Body,"localScale.y",1,.91f,duration);
                if(name=="Air") Constant(clip,Body,"localEulerAnglesRaw.x",-8,duration);
                if(name=="Hit") { Pulse(clip,Body,"localEulerAnglesRaw.x",-4,-18,duration); Pulse(clip,Head,"localEulerAnglesRaw.x",0,-9,duration); }
                if(name=="Slide Duck")
                {
                    Constant(clip,Body,"localScale.y",.64f,duration); Constant(clip,Body,"localEulerAnglesRaw.x",12,duration);
                    // Preserve the large chibi head instead of flattening the face with the torso.
                    Constant(clip,Head,"localScale.y",1.5f,duration);
                    Constant(clip,Head,"localPosition.y",1.55f,duration); Constant(clip,Head,"localPosition.z",.25f,duration);
                }
                AnimationClipSettings settings=AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime=loop; AnimationUtility.SetAnimationClipSettings(clip,settings);
                string clipPath=animations+"/"+name.Replace(" ","-")+".anim";
                AnimationClip saved=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if(saved==null) { saved=clip; AssetDatabase.CreateAsset(saved,clipPath); }
                else { EditorUtility.CopySerialized(clip,saved); EditorUtility.SetDirty(saved); Object.DestroyImmediate(clip); }
                AnimatorState state=machine.AddState(name); state.motion=saved;
                if(name=="Idle") machine.defaultState=state;
            }
            EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets(); return controller;
        }
        private static void Set(AnimationClip clip,string path,string property,AnimationCurve curve)
        { for(int i=0;i<curve.length;i++) { AnimationUtility.SetKeyLeftTangentMode(curve,i,AnimationUtility.TangentMode.Auto); AnimationUtility.SetKeyRightTangentMode(curve,i,AnimationUtility.TangentMode.Auto); } clip.SetCurve(path,typeof(Transform),property,curve); }
        private static void Constant(AnimationClip clip,string path,string property,float value,float duration)
        { Set(clip,path,property,AnimationCurve.Constant(0,duration,value)); }
        private static void Wave(AnimationClip clip,string path,string property,float value,float amplitude,float duration)
        { Set(clip,path,property,new AnimationCurve(new Keyframe(0,value),new Keyframe(duration*.25f,value+amplitude),new Keyframe(duration*.5f,value),new Keyframe(duration*.75f,value-amplitude),new Keyframe(duration,value))); }
        private static void Pulse(AnimationClip clip,string path,string property,float value,float peak,float duration)
        { Set(clip,path,property,new AnimationCurve(new Keyframe(0,value),new Keyframe(duration*.5f,peak),new Keyframe(duration,value))); }
    }
}
