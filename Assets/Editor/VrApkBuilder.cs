// VrApkBuilder.cs
// Configura o projeto p/ Meta Quest e builda o APK num clique.
// Menus (Unity):
//   Tools > VR Project > Configure Project for Quest    (so configura)
//   Tools > VR Project > Build VR APK (Quest)           (configura + builda)
//
// PRE-REQUISITOS (Editor/Hub):
//   - Android Build Support + NDK + SDK + OpenJDK instalados (estao).
//   - XR Plug-in Management > Android > OpenXR LIGADO + feature "Meta Quest Support"
//     + interaction profile "Oculus Touch Controller" (Leo fez na Fase 2).
//   - Rodar antes:  Tools > VR Project > Setup VR Rig in ALL Scenes
//   - Remover o XR Device Simulator de qualquer cena salva antes de buildar.
#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class VrApkBuilder
{
    const string BUNDLE_ID = "com.unisagrado.projetorv";
    const string OUTPUT_DIR = "Builds/Android";
    const string OUTPUT_FILE = "ProjetoRV-VR.apk";

    [MenuItem("Tools/VR Project/Configure Project for Quest")]
    public static void ConfigureMenu()
    {
        ApplyQuestSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("[VrApkBuilder] Player Settings configurados para Quest.");
    }

    [MenuItem("Tools/VR Project/Build VR APK (Quest)")]
    public static void BuildMenu()
    {
        // 1. troca plataforma se preciso
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.Log("[VrApkBuilder] Trocando plataforma p/ Android (pode demorar alguns minutos)...");
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogError("[VrApkBuilder] Falha ao trocar p/ Android. Verifique o Android Build Support no Hub.");
                return;
            }
        }

        // 2. aplica config
        ApplyQuestSettings();
        AssetDatabase.SaveAssets();

        // 3. cenas
        var sceneList = EditorBuildSettings.scenes;
        if (sceneList == null || sceneList.Length == 0)
        {
            Debug.LogError("[VrApkBuilder] Nenhuma cena em Build Settings. Adicione Scene_Lobby + os 4 continentes.");
            return;
        }
        var enabledScenes = System.Array.FindAll(sceneList, s => s.enabled);
        if (enabledScenes.Length == 0)
        {
            Debug.LogError("[VrApkBuilder] Todas as cenas em Build Settings estao desmarcadas.");
            return;
        }
        var scenePaths = System.Array.ConvertAll(enabledScenes, s => s.path);

        // 4. apk (nao aab)
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.androidCreateSymbolsZip = false;
        EditorUserBuildSettings.development = false;

        Directory.CreateDirectory(OUTPUT_DIR);
        string output = Path.Combine(OUTPUT_DIR, OUTPUT_FILE);

        var opts = new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = output,
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.None,
        };

        Debug.Log("[VrApkBuilder] Iniciando build em " + output + " (" + scenePaths.Length + " cenas)...");
        BuildReport report = BuildPipeline.BuildPlayer(opts);
        var s = report.summary;

        string sizeStr = (s.totalSize / (1024f * 1024f)).ToString("0.0") + " MB";
        Debug.Log("[VrApkBuilder] Build " + s.result +
                  " | duracao: " + s.totalTime +
                  " | tamanho: " + sizeStr +
                  " | erros: " + s.totalErrors +
                  " | warnings: " + s.totalWarnings);

        if (s.result == BuildResult.Succeeded)
        {
            Debug.Log("[VrApkBuilder] APK em: " + Path.GetFullPath(output));
            EditorUtility.RevealInFinder(Path.GetFullPath(output));
        }
        else
        {
            Debug.LogError("[VrApkBuilder] Build FALHOU. Veja os erros acima.");
        }
    }

    static void ApplyQuestSettings()
    {
        // identidade do app
        var current = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
        if (string.IsNullOrEmpty(current) || current.StartsWith("com.DefaultCompany") || current.StartsWith("com.unity"))
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, BUNDLE_ID);

        PlayerSettings.bundleVersion = "0.1-early";
        PlayerSettings.Android.bundleVersionCode = 1;

        // Android core
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;          // Quest requer >= 29
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;               // Quest e ARM64-only
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        // Graphics: Vulkan primeiro, GLES3 como fallback
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[]
        {
            GraphicsDeviceType.Vulkan,
            GraphicsDeviceType.OpenGLES3,
        });

        // VR/render
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.stereoRenderingPath = StereoRenderingPath.Instancing;                  // Single Pass Instanced (perf Quest)
        PlayerSettings.MTRendering = true;

        // orientacao
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;

        // splash
        PlayerSettings.SplashScreen.show = false;  // remove o "Made with Unity" (versao Personal mantem)

        Debug.Log("[VrApkBuilder] Settings aplicadas: bundleId=" + PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) +
                  " | IL2CPP/ARM64 | Vulkan+GLES3 | Linear+Instancing | minSDK=29");
    }
}
#endif
