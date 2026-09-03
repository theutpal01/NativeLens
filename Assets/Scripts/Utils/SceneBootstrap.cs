using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using NativeLens.Managers;
using NativeLens.Data;

namespace NativeLens.Utils
{
    /// <summary>
    /// Bootstrap script to set up the initial scene with all required components.
    /// Attach to an empty GameObject in the first scene.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        [Header("Required Prefabs/References")]
        [SerializeField] private PlantDatabase plantDatabase;
        [SerializeField] private GameObject arSessionPrefab;
        [SerializeField] private GameObject xrOriginPrefab;
        [SerializeField] private GameObject uiCanvasPrefab;

        [Header("Scene Setup")]
        [SerializeField] private bool autoSetup = true;

        private void Awake()
        {
            if (autoSetup)
            {
                SetupScene();
            }
        }

        /// <summary>
        /// Static build method for CI/CD (GitHub Actions, etc.)
        /// Called via: -executeMethod NativeLens.Utils.SceneBootstrap.BuildAPK
        /// </summary>
        public static void BuildAPK()
        {
            Debug.Log("=== NativeLens CI Build Started ===");
            
            // Ensure we're in the right scene
            var scenePath = "Assets/Scenes/ARScene.unity";
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogError($"Scene not found: {scenePath}");
                EditorApplication.Exit(1);
                return;
            }
            
            EditorSceneManager.OpenScene(scenePath);
            
            // Ensure PlantDatabase exists and is initialized
            var dbPath = "Assets/Resources/PlantData/PlantDatabase.asset";
            var plantDB = AssetDatabase.LoadAssetAtPath<PlantDatabase>(dbPath);
            if (plantDB == null)
            {
                Debug.LogError($"PlantDatabase not found at {dbPath}");
                EditorApplication.Exit(1);
                return;
            }
            
            // Initialize MVP plants if needed
            if (plantDB.TotalPlantCount == 0)
            {
                plantDB.InitializeMVPPlants();
                EditorUtility.SetDirty(plantDB);
                AssetDatabase.SaveAssets();
                Debug.Log("Initialized PlantDatabase with 7 MVP species");
            }
            
            // Build settings
            var buildPath = "build/NativeLens.apk";
            System.IO.Directory.CreateDirectory("build");
            
            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { scenePath },
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };
            
            Debug.Log($"Building APK to: {buildPath}");
            
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"=== BUILD SUCCESSFUL ===");
                Debug.Log($"APK size: {report.summary.totalSize / (1024 * 1024):F1} MB");
                Debug.Log($"Output: {buildPath}");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"=== BUILD FAILED ===");
                Debug.LogError($"Errors: {report.summary.totalErrors}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error)
                            Debug.LogError($"[{step.name}] {msg.content}");
                    }
                }
                EditorApplication.Exit(1);
            }
        }

        private void SetupScene()
        {
            // 1. Ensure PlantDatabase exists
            if (plantDatabase != null)
            {
                var plantDataManager = PlantDataManager.Instance;
                if (plantDataManager != null)
                {
                    // Use reflection to set the private field since it's serialized
                    var field = typeof(PlantDataManager).GetField("plantDatabase", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(plantDataManager, plantDatabase);
                    }
                }
            }

            // 2. Setup AR Session if not present
            if (FindObjectOfType<UnityEngine.XR.ARFoundation.ARSession>() == null && arSessionPrefab != null)
            {
                Instantiate(arSessionPrefab);
            }

            // 3. Setup XR Origin if not present
            if (FindObjectOfType<UnityEngine.XR.ARFoundation.XROrigin>() == null && xrOriginPrefab != null)
            {
                Instantiate(xrOriginPrefab);
            }

            // 4. Setup UI Canvas if not present
            if (FindObjectOfType<Canvas>() == null && uiCanvasPrefab != null)
            {
                Instantiate(uiCanvasPrefab);
            }

            // 5. Ensure EventSystem exists for UI
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Debug.Log("NativeLens scene bootstrap complete");
        }

        [ContextMenu("Force Setup")]
        public void ForceSetup()
        {
            SetupScene();
        }
    }
}