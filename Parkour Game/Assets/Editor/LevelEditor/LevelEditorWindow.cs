using System;
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
        private static string[] _levelPrefabNames;
        private static int _levelIndex;
        private static bool _skybox;
        private static string _priorScene;
        private static bool _switchingLevel;
        private static bool _wantsToQuit;
        private static Vector2 _scroll;

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
            LevelEditor.Window = _window;

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
            _levelPrefabNames = Directory.GetFiles(LEVEL_DIRECTORY, "*.prefab");
            for (var index = 0; index < _levelPrefabNames.Length; index++)
            {
                _levelPrefabNames[index] = Path.GetFileNameWithoutExtension(_levelPrefabNames[index]);
            }
            
            if (_levelPrefabNames.Length == 0 && !LevelEditor.HasLevel)
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
            LevelEditor.Window = _window;
            ReloadLevels();
            LevelEditor.OnLoad();
            
            SceneView.duringSceneGui += LevelEditor.OnSceneGUI;
            PrefabStage.prefabStageClosing += SceneClosed;
            EditorSceneManager.sceneClosed += SceneClosed;
        }

        private static void SceneClosed(Scene _)
        {
            if (!_switchingLevel)
            {
                _window.Close();
            }
        }
        
        private void SceneClosed(PrefabStage obj)
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
            
            LevelEditor.Reset();
            Tools.hidden = false;
            PrefabStage.prefabStageClosing -= SceneClosed;
            EditorSceneManager.sceneClosed -= SceneClosed;
            SceneView.duringSceneGui -= LevelEditor.OnSceneGUI;
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
            var prev = GUI.Button(new Rect(x, yOffset, indexWidth, 20), "Prev");
            x += indexWidth + gap;
            GUI.enabled = LevelEditor.HasLevel;
            var newName = 
                EditorGUI.DelayedTextField(new Rect(x, yOffset, nameWidth, 20), 
                    LevelEditor.HasLevel ? LevelEditor.LevelName : string.Empty);
            if (LevelEditor.HasLevel && LevelEditor.LevelName != newName)
            {
                LevelEditor.LevelName = newName;
            }
            GUI.enabled = true;
            x += nameWidth + gap;
            var next = GUI.Button(new Rect(x, yOffset, indexWidth, 20), "Next");

            if (next)
            {
                ChangeLevel(_levelIndex + 1);
            }

            if (prev)
            {
                ChangeLevel(_levelIndex - 1);
            }

            if (LevelEditor.HasLevel)
            {
                GUILayout.Space(40);
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Add Level"))
                    {
                        var level = _levelPrefabNames.Length;
                        CreateLevel();
                        ChangeLevel(level);
                    }

                    if (GUILayout.Button("Delete Level"))
                    {
                        if (EditorUtility.DisplayDialog("Delete Level",
                                "Are you sure you want to delete this level?",
                                "Delete", "Cancel"))
                        {
                            DeleteLevel();
                        }
                    }
                }
                GUILayout.EndHorizontal();
                _scroll = GUILayout.BeginScrollView(_scroll, new GUIStyle
                {
                    fixedWidth = _window.position.width,
                    stretchWidth = false,
                });
                LevelEditor.OnGUI();
                EditorGUILayout.EndScrollView();
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
        
        private static void ChangeLevel(int index)
        {
            var old = _levelIndex;
            //Ensure it stays within the index range
            _levelIndex = index < 0 ? _levelPrefabNames.Length - 1 : index % _levelPrefabNames.Length;

            if (_levelPrefabNames.Length == 0)
            {
                _levelIndex = 0;
                _levelPrefabNames = Array.Empty<string>();
                EditorSceneManager.OpenScene(_priorScene);
                return;
            }
            
            if (old != _levelIndex)
            {
                LoadLevel(false);
            }
        }
        
        private static void CreateLevel()
        {
            var levelRoot = new GameObject($"Level {_levelPrefabNames.Length + 1}", typeof(LevelBlock));
            PrefabUtility.SaveAsPrefabAsset(levelRoot, LEVEL_ASSET_PATH + $"Level {_levelPrefabNames.Length + 1}" + ".prefab");
            DestroyImmediate(levelRoot);
            ReloadLevels();
        }

        private static void DeleteLevel()
        {
            AssetDatabase.DeleteAsset(LEVEL_ASSET_PATH + _levelPrefabNames[_levelIndex] + ".prefab");
            ReloadLevels();
            ChangeLevel(_levelIndex - 1);
        }
        
        //unity editor is very clunky sometimes
        private static void LoadLevel(bool init = true)
        {
            _switchingLevel = true;
            if (init)
            {
                _stage = PrefabStageUtility.GetCurrentPrefabStage();
                if (!_stage)
                {
                    _stage = PrefabStageUtility.OpenPrefab(LEVEL_ASSET_PATH + _levelPrefabNames[_levelIndex] + ".prefab");
                }
                else
                {
                    _levelIndex = Array.FindIndex(_levelPrefabNames,
                        s => s == Path.GetFileNameWithoutExtension(_stage.assetPath));
                }
            }
            else
            {
                _stage = PrefabStageUtility.OpenPrefab(LEVEL_ASSET_PATH + _levelPrefabNames[_levelIndex] + ".prefab");
            }
            LevelEditor.OpenLevelForEditing(_stage.prefabContentsRoot);
            _switchingLevel = false;
        }

        public static void RepaintGUI()
        {
            _window?.Repaint();
        }
    }
}
