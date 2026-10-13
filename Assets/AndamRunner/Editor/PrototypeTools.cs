using System;
using System.IO;
using AndamRunner;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace AndamRunnerEditor
{
    [InitializeOnLoad]
    public static class PrototypeTools
    {
        public const string ScenePath = "Assets/Scenes/MainGame.unity";
        private const string SmokeKey = "Andam.SmokePending";
        private const string BatchKey = "Andam.SmokeBatch";

        static PrototypeTools()
        {
            EditorApplication.playModeStateChanged += OnPlayMode;
            if(!Application.isBatchMode)
                EditorApplication.delayCall += () =>
                {
                    if(!EditorApplication.isPlayingOrWillChangePlaymode && !File.Exists(ScenePath)) PrepareProject();
                };
        }

        [MenuItem("ANDAM/1. Prepare or open MainGame")]
        public static void PrepareProject()
        {
            Directory.CreateDirectory("Assets/Scenes"); Directory.CreateDirectory("Assets/AndamRunner/Resources");
            AssetDatabase.Refresh();
            const string materialPath="Assets/AndamRunner/Resources/AndamBase.mat";
            Shader toon=AssetDatabase.LoadAssetAtPath<Shader>("Assets/AndamRunner/Shaders/CoastalToon.shader");
            if(AssetDatabase.LoadAssetAtPath<Material>(materialPath)==null)
            {
                var material=new Material(toon!=null ? toon : Shader.Find("Standard")); material.color=Color.white; material.enableInstancing=true;
                material.SetFloat("_Glossiness",.08f); AssetDatabase.CreateAsset(material,materialPath);
            }
            Material baseMaterial=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(toon!=null && baseMaterial.shader!=toon) { baseMaterial.shader=toon; EditorUtility.SetDirty(baseMaterial); }
            const string skyPath="Assets/AndamRunner/Resources/CoastalSky.mat";
            Shader sky=AssetDatabase.LoadAssetAtPath<Shader>("Assets/AndamRunner/Shaders/CoastalSky.shader");
            if(sky!=null && AssetDatabase.LoadAssetAtPath<Material>(skyPath)==null) AssetDatabase.CreateAsset(new Material(sky),skyPath);
            TextureImporter image=AssetImporter.GetAtPath("Assets/AndamRunner/Resources/OpongReference.png") as TextureImporter;
            if(image!=null) { image.maxTextureSize=512; image.textureCompression=TextureImporterCompression.Compressed; image.mipmapEnabled=false; image.SaveAndReimport(); }
            TextureImporter coast=AssetImporter.GetAtPath("Assets/AndamRunner/Resources/StorybookCoast.png") as TextureImporter;
            if(coast!=null)
            {
                coast.maxTextureSize=2048; coast.textureCompression=TextureImporterCompression.Compressed;
                coast.mipmapEnabled=false; coast.wrapMode=TextureWrapMode.Clamp; coast.SaveAndReimport();
            }
            Material backdrop=PrepareMaterial("IllustratedBackdrop","IllustratedBackdrop");
            TextureImporter storm=AssetImporter.GetAtPath("Assets/AndamRunner/Resources/StormEvacuationCoast.png") as TextureImporter;
            if(storm!=null) { storm.maxTextureSize=2048; storm.textureCompression=TextureImporterCompression.Compressed; storm.mipmapEnabled=false; storm.wrapMode=TextureWrapMode.Clamp; storm.SaveAndReimport(); }
            foreach(string name in new[]{"MenuBackground","CreditsBackground","LeaderboardBackground","HowToBackground","AboutBackground","SplashscreenBackground","ScoreBackground"})
            {
                TextureImporter menu=AssetImporter.GetAtPath("Assets/AndamRunner/Resources/"+name+".png") as TextureImporter;
                if(menu==null) continue;
                menu.textureType=TextureImporterType.Default; menu.maxTextureSize=2048;
                menu.npotScale=TextureImporterNPOTScale.None; menu.mipmapEnabled=false;
                menu.wrapMode=TextureWrapMode.Clamp; menu.filterMode=FilterMode.Bilinear;
                menu.textureCompression=TextureImporterCompression.CompressedHQ; menu.compressionQuality=100;
                menu.SaveAndReimport();
            }
            backdrop.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AndamRunner/Resources/StorybookCoast.png");
            EditorUtility.SetDirty(backdrop);
            Material lanes=PrepareMaterial("IllustratedGround","IllustratedGround");
            lanes.color=new Color(.34f,.40f,.51f,.80f); EditorUtility.SetDirty(lanes);
            if(!File.Exists(ScenePath))
            {
                Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                new GameObject("Ready, Set, Resilient! - runtime scene builder").AddComponent<GameManager>();
                GameObject cameraObject=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)); cameraObject.tag="MainCamera";
                Camera camera=cameraObject.GetComponent<Camera>(); camera.backgroundColor=new Color32(167,220,229,255); camera.clearFlags=CameraClearFlags.SolidColor;
                cameraObject.transform.position=new Vector3(-.55f,5.6f,-9f); cameraObject.transform.LookAt(new Vector3(0,1.5f,12f));
                GameObject sunObject=new GameObject("Tropical sunlight",typeof(Light)); Light sun=sunObject.GetComponent<Light>();
                sun.type=LightType.Directional; sun.intensity=1.1f; sun.color=new Color(1,.95f,.83f); sun.shadows=LightShadows.None;
                sunObject.transform.rotation=Quaternion.Euler(45,-35,0);
                EditorSceneManager.SaveScene(scene,ScenePath);
            }
            else EditorSceneManager.OpenScene(ScenePath);
            EditorBuildSettings.scenes=new[] { new EditorBuildSettingsScene(ScenePath,true) };
            ConfigureSettings(); AssetDatabase.SaveAssets();
            Debug.Log("ANDAM: MainGame is ready. Press Play. Android support installed: "+BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android));
        }

        private static void ConfigureSettings()
        {
            PlayerSettings.companyName="ANDAM Lapu-Lapu"; PlayerSettings.productName="Ready Set Resilient";
            PlayerSettings.bundleVersion="0.6.4"; PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"org.andamlapulapu.readysetresilient");
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft=false; PlayerSettings.allowedAutorotateToLandscapeRight=false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.defaultScreenWidth=720; PlayerSettings.defaultScreenHeight=1280; PlayerSettings.resizableWindow=false;
            PlayerSettings.runInBackground=false;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[] { GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.Android.forceInternetPermission=false;
            // Legacy Input Manager keeps touch + keyboard + uGUI working without extra dependencies.
            SerializedObject settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            SerializedProperty input=settings.FindProperty("activeInputHandler"); if(input!=null) { input.intValue=0; settings.ApplyModifiedPropertiesWithoutUndo(); }
            QualitySettings.vSyncCount=0; QualitySettings.antiAliasing=2; QualitySettings.shadows=ShadowQuality.Disable;
            QualitySettings.pixelLightCount=1;
        }

        private static Material PrepareMaterial(string name,string shaderName)
        {
            string path="Assets/AndamRunner/Resources/"+name+".mat";
            Shader shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/AndamRunner/Shaders/"+shaderName+".shader");
            if(shader==null) throw new InvalidOperationException("Missing artwork shader: "+shaderName);
            Material material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null) { material=new Material(shader); AssetDatabase.CreateAsset(material,path); }
            else { material.shader=shader; EditorUtility.SetDirty(material); }
            return material;
        }

        [MenuItem("ANDAM/2. Run gameplay smoke checks")]
        public static void RunSmokeTests()
        {
            if(EditorApplication.isPlaying) { Debug.LogWarning("Stop Play mode before running smoke checks."); return; }
            PrepareProject(); SessionState.SetBool(SmokeKey,true); SessionState.SetBool(BatchKey,Application.isBatchMode);
            EditorApplication.isPlaying=true;
        }

        private static void OnPlayMode(PlayModeStateChange change)
        {
            if(change==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(SmokeKey,false))
                ExecuteRuntimeChecks();
        }

        private static void ExecuteRuntimeChecks()
        {
            bool batch=SessionState.GetBool(BatchKey,false); SessionState.SetBool(SmokeKey,false);
            try
            {
                GameManager game=UnityEngine.Object.FindAnyObjectByType<GameManager>();
                if(game==null) throw new Exception("GameManager missing from MainGame.");
                game.enabled=false;
                SmokeChecks.Run(game);
                Debug.Log("ANDAM_SMOKE_PASS: gameplay, menu navigation, credits and persistent top-ten leaderboard checks passed.");
                File.WriteAllText("Docs/Validation.txt","Unity "+Application.unityVersion+"\nGameplay, menu navigation, credits and leaderboard persistence checks passed on "+DateTime.UtcNow.ToString("u")+"\nSee Editor-smoke.log for details. Tests use isolated storage; real player scores are untouched. Android APK and physical-device touch tests are separate checks.\n");
                if(batch) EditorApplication.Exit(0); else { game.enabled=true; game.StartRun(); }
            }
            catch(Exception error)
            {
                Debug.LogException(error);
                if(batch) EditorApplication.Exit(1); else EditorApplication.isPlaying=false;
            }
        }

        [MenuItem("ANDAM/3. Build Android APK")]
        public static void BuildAndroid()
        {
            PrepareProject();
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
                throw new InvalidOperationException("Install Android Build Support + Android SDK & NDK Tools + OpenJDK for Unity "+Application.unityVersion+" in Unity Hub > Installs > Manage > Add modules.");
            Directory.CreateDirectory("Builds/Android"); EditorUserBuildSettings.buildAppBundle=false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
            BuildReport report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{ScenePath},locationPathName="Builds/Android/ReadySetResilient.apk",target=BuildTarget.Android,options=BuildOptions.Development });
            if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Android build failed: "+report.summary.result);
            Debug.Log("ANDAM APK built: Builds/Android/ReadySetResilient.apk");
        }

        [MenuItem("ANDAM/Build Mac preview")]
        public static void BuildMac()
        {
            PrepareProject(); Directory.CreateDirectory("Builds/Mac");
            BuildReport report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{ScenePath},locationPathName="Builds/Mac/ReadySetResilient.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.Development });
            if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Mac preview build failed: "+report.summary.result);
            Debug.Log("ANDAM_MAC_BUILD_PASS");
        }
    }
}
