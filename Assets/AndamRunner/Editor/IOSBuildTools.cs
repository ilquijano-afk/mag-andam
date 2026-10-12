using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace AndamRunnerEditor
{
    public static class IOSBuildTools
    {
        public const string BundleId="org.andamlapulapu.readysetresilient";

        // Unity exports the complete native project. A Mac with Xcode performs signing/export.
        [MenuItem("ANDAM/Build iOS Xcode project")]
        public static void Build()
        {
            if(EditorApplication.isPlaying)
                throw new InvalidOperationException("Stop Play mode before exporting iOS.");
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS,BuildTarget.iOS))
                throw new InvalidOperationException("Install iOS Build Support for Unity "+Application.unityVersion+" in Unity Hub.");
            PrototypeTools.PrepareProject();
            Configure();
            EditorUserBuildSettings.development=false;
            EditorUserBuildSettings.allowDebugging=false;
            EditorUserBuildSettings.connectProfiler=false;
            string output=Environment.GetEnvironmentVariable("ANDAM_IOS_OUTPUT");
            if(string.IsNullOrWhiteSpace(output)) output="Builds/iOS/Xcode";
            output=Path.GetFullPath(output);
            Directory.CreateDirectory(output);
            BuildReport report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes=new[]{PrototypeTools.ScenePath},
                locationPathName=output,
                target=BuildTarget.iOS,
                options=BuildOptions.None
            });
            if(report.summary.result!=BuildResult.Succeeded)
                throw new Exception("iOS project export failed: "+report.summary.result+". Inspect the Unity build log.");
            File.WriteAllText(Path.Combine(Directory.GetParent(output).FullName,"Unity-export-status.txt"),
                "Unity "+Application.unityVersion+" exported the iOS release Xcode project successfully.\n"+
                "Bundle ID: "+BundleId+"\nMinimum iOS: 15.0\nGraphics: Metal\nBackend: IL2CPP\nOrientation: Portrait\n"+
                "Output: "+output+"\n"+
                "Development player, script debugging and profiler are disabled.\n"+
                "This is a native project export, not a signed IPA. Xcode archive/signing and device testing are separate steps.\n");
            Debug.Log("ANDAM_IOS_EXPORT_PASS: "+output);
        }

        public static void Configure()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,BundleId);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS,new[]{GraphicsDeviceType.Metal});
            PlayerSettings.iOS.sdkVersion=iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetOSVersionString="15.0";
            PlayerSettings.iOS.targetDevice=iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.buildNumber="2";
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft=false;
            PlayerSettings.allowedAutorotateToLandscapeRight=false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            string team=Environment.GetEnvironmentVariable("ANDAM_APPLE_TEAM_ID");
            if(!string.IsNullOrWhiteSpace(team))
            {
                if(team.Length!=10 || !System.Text.RegularExpressions.Regex.IsMatch(team,"^[A-Z0-9]{10}$"))
                    throw new InvalidOperationException("ANDAM_APPLE_TEAM_ID must be the 10-character Apple Developer Team ID.");
                PlayerSettings.iOS.appleDeveloperTeamID=team;
                PlayerSettings.iOS.appleEnableAutomaticSigning=true;
            }
            AssetDatabase.SaveAssets();
        }
    }
}
