using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;

namespace NativeLens.Editor
{
    /// <summary>
    /// Editor utility to set up the NativeLens AR scene automatically.
    /// Run via Tools → NativeLens → Setup AR Scene
    /// </summary>
    public class SceneSetupWizard : EditorWindow
    {
        [MenuItem("Tools/NativeLens/Setup AR Scene")]
        public static void SetupARScene()
        {
            // Create or get the AR Scene
            var scenePath = "Assets/Scenes/ARScene.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);

            // 1. Create AR Session
            var arSessionGO = new GameObject("AR Session");
            var arSession = arSessionGO.AddComponent<ARSession>();

            // 2. Create XR Origin
            var xrOriginGO = new GameObject("XR Origin");
            var xrOrigin = xrOriginGO.AddComponent<XROrigin>();

            // Camera Offset
            var cameraOffsetGO = new GameObject("Camera Offset");
            cameraOffsetGO.transform.SetParent(xrOriginGO.transform);
            cameraOffsetGO.transform.localPosition = new Vector3(0, 1.5f, 0); // Eye height

            // Main Camera
            var cameraGO = new GameObject("Main Camera");
            cameraGO.transform.SetParent(cameraOffsetGO.transform);
            cameraGO.transform.localPosition = Vector3.zero;
            cameraGO.transform.localRotation = Quaternion.identity;
            cameraGO.tag = "MainCamera";
            
            var camera = cameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 1000f;

            // AR Camera Background
            cameraGO.AddComponent<ARCameraBackground>();

            // Assign to XR Origin
            xrOrigin.Camera = camera;
            xrOrigin.CameraFloorOffsetObject = cameraOffsetGO.transform;

            // AR Components on XR Origin
            xrOriginGO.AddComponent<ARRaycastManager>();
            xrOriginGO.AddComponent<ARPlaneManager>();
            xrOriginGO.AddComponent<ARAnchorManager>();

            // 3. Create UI Canvas
            var canvasGO = new GameObject("UI Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            
            var canvasScaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 4. Create Managers Container
            var managersGO = new GameObject("Managers");
            
            // Add all manager scripts
            managersGO.AddComponent<NativeLens.Managers.GameManager>();
            managersGO.AddComponent<NativeLens.Managers.PlantDataManager>();
            managersGO.AddComponent<NativeLens.Managers.ARManager>();
            managersGO.AddComponent<NativeLens.Managers.GalleryManager>();
            managersGO.AddComponent<NativeLens.Managers.PlantIdentificationManager>();
            managersGO.AddComponent<NativeLens.Managers.BotanicalGuideManager>();
            managersGO.AddComponent<NativeLens.Managers.ObservationManager>();
            managersGO.AddComponent<NativeLens.Managers.UIManager>();

            // 5. Create Bootstrap
            var bootstrapGO = new GameObject("SceneBootstrap");
            bootstrapGO.AddComponent<NativeLens.Utils.SceneBootstrap>();

            // 6. Save scene
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("NativeLens AR Scene setup complete!");
            EditorUtility.DisplayDialog("NativeLens Setup", "AR Scene created successfully!\n\nNext steps:\n1. Assign PlantDatabase to PlantDataManager\n2. Create UI prefabs and assign to managers\n3. Configure AR Session settings\n4. Build and test on device", "OK");
        }

        [MenuItem("Tools/NativeLens/Create Plant Database Asset")]
        public static void CreatePlantDatabaseAsset()
        {
            var asset = ScriptableObject.CreateInstance<NativeLens.Data.PlantDatabase>();
            var path = "Assets/Resources/PlantData/PlantDatabase.asset";
            
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            
            // Initialize with MVP plants
            asset.InitializeMVPPlants();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            
            Debug.Log("PlantDatabase asset created at: " + path);
            EditorUtility.DisplayDialog("NativeLens", "PlantDatabase created with 7 MVP species!", "OK");
        }

        [MenuItem("Tools/NativeLens/Validate Project Setup")]
        public static void ValidateSetup()
        {
            var issues = new System.Collections.Generic.List<string>();

            // Check packages
            var manifestPath = "Packages/manifest.json";
            if (!System.IO.File.Exists(manifestPath))
            {
                issues.Add("Packages/manifest.json not found");
            }

            // Check PlantDatabase
            var db = AssetDatabase.LoadAssetAtPath<NativeLens.Data.PlantDatabase>("Assets/Resources/PlantData/PlantDatabase.asset");
            if (db == null)
            {
                issues.Add("PlantDatabase asset not found at Assets/Resources/PlantData/PlantDatabase.asset");
            }
            else if (db.Plants.Count != 7)
            {
                issues.Add($"PlantDatabase has {db.Plants.Count} plants, expected 7");
            }

            // Check AR Scene
            var scenePath = "Assets/Scenes/ARScene.unity";
            if (!System.IO.File.Exists(scenePath))
            {
                issues.Add("ARScene.unity not found at Assets/Scenes/ARScene.unity");
            }

            // Check scripts
            var requiredScripts = new[]
            {
                "GameManager", "PlantDataManager", "ARManager", "GalleryManager",
                "PlantIdentificationManager", "BotanicalGuideManager", "ObservationManager", "UIManager"
            };

            foreach (var scriptName in requiredScripts)
            {
                var guids = AssetDatabase.FindAssets(scriptName + " t:Script");
                if (guids.Length == 0)
                {
                    issues.Add($"Script not found: {scriptName}.cs");
                }
            }

            string message = issues.Count == 0 
                ? "✅ All checks passed! Project setup is valid." 
                : "❌ Issues found:\n\n" + string.Join("\n", issues);

            EditorUtility.DisplayDialog("NativeLens Validation", message, "OK");
        }
    }
}