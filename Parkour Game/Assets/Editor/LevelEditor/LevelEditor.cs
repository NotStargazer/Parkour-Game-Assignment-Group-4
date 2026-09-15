using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LevelEditor
{
    public class LevelEditor : EditorWindow
    {
        private static SerializedObject _currentLevelBlock;
        
        private static EditorWindow _window;
        private static PrefabStage _stage;
        private static string[] _prefabs;
        private static int _levelIndex;
        private static bool _skybox;
        private static string _priorScene;
        private static bool _switchingLevel;

        private static readonly string LEVEL_DIRECTORY = Application.dataPath + "/Level Blocks/";
        private const string LEVEL_ASSET_PATH = "Assets/Level Blocks/";

        [MenuItem("Level Editor/Open")]
        private static void OpenEditor()
        {
            _window = GetWindow<LevelEditor>("Level Editor");

            if (!Directory.Exists(LEVEL_DIRECTORY))
            {
                Directory.CreateDirectory(LEVEL_DIRECTORY);
            }

            ReloadLevels();
        }

        private static void ReloadLevels()
        {
            _prefabs = Directory.GetFiles(LEVEL_DIRECTORY, "*.prefab");
            for (var index = 0; index < _prefabs.Length; index++)
            {
                _prefabs[index] = Path.GetFileNameWithoutExtension(_prefabs[index]);
            }
            
            if (_prefabs.Length == 0 && _currentLevelBlock == null)
            {
                return;
            }

            _levelIndex = 0;
            _priorScene = SceneManager.GetActiveScene().path;
            _skybox = SceneView.lastActiveSceneView.sceneViewState.showSkybox;
            LoadLevel();
            SceneView.lastActiveSceneView.sceneViewState.showSkybox = false;
        }
        
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            EditorSceneManager.sceneClosed += _ =>
            {
                if (!_switchingLevel)
                {
                    _window.Close();
                }
            };
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
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

            var hasLevels = _prefabs is { Length: > 0 };
            
            var x = gap;
            GUI.Button(new Rect(x, yOffset, indexWidth, 20), "Prev");
            x += indexWidth + gap;
            GUI.enabled = hasLevels;
            GUI.TextField(new Rect(x, yOffset, nameWidth, 20), "");
            GUI.enabled = true;
            x += nameWidth + gap;
            GUI.Button(new Rect(x, yOffset, indexWidth, 20), "Next");

            if (hasLevels)
            {
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
            _stage = PrefabStageUtility.OpenPrefab(LEVEL_ASSET_PATH + _prefabs[_levelIndex] + ".prefab");
            var instanceRoot = _stage.openedFromInstanceRoot;
            var levelRoot = instanceRoot.GetComponent<LevelBlock>();
            _currentLevelBlock = new SerializedObject(levelRoot);
        }
        
        private void OnSceneGUI(SceneView obj)
        {
            
        }
    }
}
