#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using System.Linq;
using System.IO;

public class GCVR_Setup_Tool : EditorWindow
{
    [MenuItem("Tools/Google Cardboard Setup")]
    public static void ShowWindow()
    {
        GetWindow<GCVR_Setup_Tool>("Google Cardboard Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Google Cardboard Setup for Beginners", EditorStyles.boldLabel);

        if (GUILayout.Button("Switch Platform to Android"))
        {
            SwitchPlatformToAndroid();
        }

        if (GUILayout.Button("Configure Player Settings"))
        {
            ConfigurePlayerSettings();
        }

        if (GUILayout.Button("Enable Cardboard XR Plugin"))
        {
            EnableCardboardXRPlugin();
        }

        if (GUILayout.Button("Update Gradle Files"))
        {
            UpdateGradleFiles();
        }
    }

    static void SwitchPlatformToAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        Debug.Log("Switched to Android Platform");
    }

    static void ConfigurePlayerSettings()
    {
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

        // Set Graphics APIs
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        var graphicsAPIs = new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 };
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, graphicsAPIs);

        // Set Scripting Backend
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;

        Debug.Log("Player settings configured.");
    }

    static void EnableCardboardXRPlugin()
    {
        XRGeneralSettings settings = XRGeneralSettings.Instance;
        if (settings != null && settings.Manager != null)
        {
            XRManagerSettings xrManager = settings.Manager;
            var cardboardLoaderType = typeof(Google.XR.Cardboard.XRLoader);

            // Check if Cardboard Loader exists
            var loaderExists = xrManager.activeLoaders.Any(loader => loader.GetType() == cardboardLoaderType);

            if (!loaderExists)
            {
                // Add Cardboard XR Loader
                XRLoader loader = (XRLoader)ScriptableObject.CreateInstance(cardboardLoaderType);
                if (xrManager.TryAddLoader(loader))
                {
                    Debug.Log("Google Cardboard XR Loader enabled.");
                    EditorUtility.SetDirty(settings); // Mark settings dirty to ensure it saves
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh(); // Refresh the asset database
                }
                else
                {
                    Debug.LogError("Failed to enable Google Cardboard XR Loader.");
                }
            }
            else
            {
                Debug.Log("Google Cardboard XR Loader already exists.");
            }

            // Force refresh the XR Plugin Management UI
            var cardboardLoader = xrManager.activeLoaders.FirstOrDefault(l => l.GetType() == cardboardLoaderType);
            if (cardboardLoader != null)
            {
                settings.Manager.TryAddLoader(cardboardLoader);
                EditorUtility.SetDirty(settings); // Save changes
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(); // Ensure the UI is refreshed
                Debug.Log("Cardboard XR Plugin is now enabled in the XR Management.");
            }
        }
        else
        {
            Debug.LogError("XR General Settings or XR Manager Settings are not available.");
        }
    }

    void UpdateGradleFiles()
{
    // Path to the mainTemplate.gradle file
    string gradlePath = "Assets/Plugins/Android/mainTemplate.gradle";
    if (File.Exists(gradlePath))
    {
        string content = File.ReadAllText(gradlePath);
        
        // Check if the required dependencies are already present
        if (!content.Contains("androidx.appcompat"))
        {
            // Append dependencies to the file
            content += @"
            dependencies {
                implementation 'androidx.appcompat:appcompat:1.6.1'
                implementation 'com.google.android.gms:play-services-vision:20.1.3'
                implementation 'com.google.android.material:material:1.6.1'
                implementation 'com.google.protobuf:protobuf-javalite:3.19.4'
            }";
            File.WriteAllText(gradlePath, content);
            Debug.Log("Added dependencies to mainTemplate.gradle.");
        }
    }

    // Path to the gradleTemplate.properties file
    string gradlePropertiesPath = "Assets/Plugins/Android/gradleTemplate.properties";
    if (File.Exists(gradlePropertiesPath))
    {
        string content = File.ReadAllText(gradlePropertiesPath);
        
        // Check if Jetifier and AndroidX are already enabled
        if (!content.Contains("android.enableJetifier"))
        {
            // Append properties to the file
            content += @"
            android.enableJetifier=true
            android.useAndroidX=true
            ";
            File.WriteAllText(gradlePropertiesPath, content);
            Debug.Log("Added properties to gradleTemplate.properties.");
        }
    }
}

}
#endif
