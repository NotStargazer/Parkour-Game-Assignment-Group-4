using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelEditor
{
    public class LevelEditorWindow : EditorWindow
    {
        private static EditorWindow _window;
        private static PrefabStage _stage;
        private static string[] _prefabs;
        private static int _levelIndex;
        private static bool _skybox;
        private static string _priorScene;
        private static bool _switchingLevel;
        private static bool _wantsToQuit;

        private static readonly string LEVEL_DIRECTORY = Application.dataPath + "/Level Blocks/";
        private static readonly string GEOMETRY_DIRECTORY = Application.dataPath + "/Editor/Geometry";
        private static readonly string OBJECT_DIRECTORY = Application.dataPath + "/Editor/Objects";
        private const string LEVEL_ASSET_PATH = "Assets/Level Blocks/";

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            EditorApplication.wantsToQuit += QuitCheck;
        }
        
        //Tragically need to do this so the editor doesn't die when trying to relaunch the project
        private static bool QuitCheck()
        {
            if (HasOpenInstances<LevelEditorWindow>())
            {
                _wantsToQuit = true;
                _window.Close();
                return false;
            }

            return true;
        }
        
        [MenuItem("Level Editor/Open")]
        private static void OpenEditor()
        {
            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            _window = GetWindow<LevelEditorWindow>("Level Editor", inspectorType);

            if (!Directory.Exists(LEVEL_DIRECTORY))
            {
                Directory.CreateDirectory(LEVEL_DIRECTORY);
            }
            if (!Directory.Exists(GEOMETRY_DIRECTORY))
            {
                Directory.CreateDirectory(GEOMETRY_DIRECTORY);
            }
            if (!Directory.Exists(OBJECT_DIRECTORY))
            {
                Directory.CreateDirectory(OBJECT_DIRECTORY);
            }
        }

        private static void ReloadLevels()
        {
            _prefabs = Directory.GetFiles(LEVEL_DIRECTORY, "*.prefab");
            for (var index = 0; index < _prefabs.Length; index++)
            {
                _prefabs[index] = Path.GetFileNameWithoutExtension(_prefabs[index]);
            }
            
            if (_prefabs.Length == 0 && !LevelEditor.HasLevel)
            {
                return;
            }

            _priorScene = SceneManager.GetActiveScene().path;
            _skybox = SceneView.lastActiveSceneView && SceneView.lastActiveSceneView.sceneViewState.showSkybox;
            LoadLevel();
            if (SceneView.lastActiveSceneView)
            {
                SceneView.lastActiveSceneView.sceneViewState.showSkybox = false;
            }
        }
        
        private void OnEnable()
        {
            Tools.hidden = true;
            var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            _window ??= GetWindow<LevelEditorWindow>("Level Editor", inspectorType);
            ReloadLevels();
            
            SceneView.duringSceneGui += LevelEditor.OnSceneGUI;
            EditorSceneManager.sceneClosed += SceneClosed;
        }

        private void SceneClosed(Scene _)
        {
            if (!_switchingLevel)
            {
                _window.Close();
            }
        }

        private void OnDestroy()
        {
            if (_wantsToQuit)
            {
                EditorApplication.Exit(0);
                return;
            }
            
            Tools.hidden = false;
            EditorSceneManager.sceneClosed -= SceneClosed;
            SceneView.duringSceneGui -= LevelEditor.OnSceneGUI;
            if (!string.IsNullOrEmpty(_priorScene))
            {
                EditorSceneManager.OpenScene(_priorScene);
            }
            SceneView.lastActiveSceneView.sceneViewState.showSkybox = _skybox;
        }

        private void OnGUI()
        {
            const float yOffset = 10f;
            const float yOffsetCenter = 200f;
            const float width = 125f;
            const float indexWidth = 50f;
            const float gap = 5f;
            var centerX =  EditorGUIUtility.currentViewWidth * 0.5f;
            var nameWidth = EditorGUIUtility.currentViewWidth - (gap * 4 + indexWidth * 2);

            
            var x = gap;
            GUI.Button(new Rect(x, yOffset, indexWidth, 20), "Prev");
            x += indexWidth + gap;
            GUI.enabled = LevelEditor.HasLevel;
            var newName = 
                EditorGUI.DelayedTextField(new Rect(x, yOffset, nameWidth, 20), LevelEditor.LevelName);
            if (LevelEditor.LevelName != newName)
            {
                LevelEditor.LevelName = newName;
            }
            GUI.enabled = true;
            x += nameWidth + gap;
            GUI.Button(new Rect(x, yOffset, indexWidth, 20), "Next");

            if (LevelEditor.HasLevel)
            {
                GUILayout.Space(40);
                LevelEditor.OnGUI();
                
                return;
            }
            
            GUI.Label(new Rect(centerX - width * 0.5f, yOffsetCenter - 20, width, 20), "No levels available",
                new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = new GUIStyleState { textColor = Color.white }
                });
            var create = GUI.Button(new Rect(centerX - width * 0.5f, yOffsetCenter, width, 20), "Create Level");
            if (create)
            {
                CreateLevel();
            }
        }
        
        private static void CreateLevel()
        {
            var levelRoot = new GameObject($"Level {_prefabs.Length + 1}");
            levelRoot.AddComponent<LevelBlock>();
            PrefabUtility.SaveAsPrefabAsset(levelRoot, LEVEL_ASSET_PATH + $"Level {_prefabs.Length + 1}" + ".prefab");
            ReloadLevels();
        }

        private static void LoadLevel()
        {
            _switchingLevel = true;
            _stage = PrefabStageUtility.OpenPrefab(LEVEL_ASSET_PATH + _prefabs[_levelIndex] + ".prefab");
            LevelEditor.OpenLevelForEditing(_stage.prefabContentsRoot);
            _switchingLevel = false;
        }
    }
}
