using System.IO;
using Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace LevelEditor
{
    public static class LevelEditor
    {
        private enum Tool
        {
            Bounds,
            Gates,
            Geometry,
            Objects,
        }
        
        private const string LEVEL_ASSET_PATH = "Assets/Level Blocks/";
        private const string GEOMETRY_PATH = "Assets/Editor/Geometry/";
        private const string OBJECT_PATH = "Assets/Editor/Objects/";
        private static readonly string GEOMETRY_DIRECTORY = Application.dataPath + "/Editor/Geometry";
        private static readonly string OBJECT_DIRECTORY = Application.dataPath + "/Editor/Objects";
        
        private static SerializedObject _currentLevelBlock;
        private static GameObject _levelRoot;

        private static SerializedObject[] _components;

        private static Tool _currentTool;
        private static int _currentGeometry;
        private static int _currentObject;
        private static ILevelObject _currentSelection;
        private static ILevelObject[] _geometryPrefabs;
        private static ILevelObject[] _objectPrefabs;
        private static GUIContent[] _geometryIcons;
        private static GUIContent[] _objectIcons;
        
        private static GameObject LevelRoot
        {
            get
            {
                if (!_levelRoot)
                {
                    var stage = PrefabStageUtility.GetCurrentPrefabStage();
                    _levelRoot = stage.prefabContentsRoot;
                }

                return _levelRoot;
            }
            set => _levelRoot = value;
        }
        private static Transform LevelTransform => LevelRoot.transform;
        public static bool HasLevel => _currentLevelBlock != null;
        
        public static string LevelName
        {
            get => LevelRoot.name;
            set
            {
                var oldName = LevelRoot.name;
                LevelRoot.name = value;
                AssetDatabase.RenameAsset(LEVEL_ASSET_PATH + oldName + ".prefab", value);
                AssetDatabase.ForceReserializeAssets(new []{LEVEL_ASSET_PATH + value + ".prefab"});
            }
        }

        [InitializeOnLoadMethod] [MenuItem("Level Editor/Reload Objects")]
        private static void OnLoad()
        {
            var ga = Directory.GetFiles(GEOMETRY_DIRECTORY, "*.prefab");
            var oa = Directory.GetFiles(OBJECT_DIRECTORY, "*.prefab");
            
            _geometryPrefabs = new ILevelObject[ga.Length];
            _objectPrefabs = new ILevelObject[oa.Length];
            _geometryIcons = new GUIContent[ga.Length];
            _objectIcons = new GUIContent[oa.Length];
            
            for (var i = 0; i < ga.Length; i++)
            {              
                var name = Path.GetFileNameWithoutExtension(ga[i]);
                _geometryPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(GEOMETRY_PATH + name + ".prefab")
                    .GetComponent<ILevelObject>();
                _geometryIcons[i] = new GUIContent();
            }            
            
            for (var i = 0; i < oa.Length; i++)
            {
                var name = Path.GetFileNameWithoutExtension(oa[i]);
                _objectPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(OBJECT_PATH + name + ".prefab")
                    .GetComponent<ILevelObject>();
                _objectIcons[i] = new GUIContent();
            }
        }
        
        public static void OpenLevelForEditing(GameObject prefabRoot)
        {
            LevelRoot = prefabRoot;
            _currentLevelBlock = prefabRoot.TryGetComponent(out LevelBlock levelBlock)
                ? new SerializedObject(levelBlock)
                : new SerializedObject(prefabRoot.AddComponent<LevelBlock>());
        }

        private static readonly GUIContent[] TOOLBAR_CONTENT =
        {
            EditorGUIUtility.IconContent("d_RectTool On"),
            EditorGUIUtility.IconContent("d_ToolHandleCenter"),
            EditorGUIUtility.IconContent("d_PreMatCube"),
            EditorGUIUtility.IconContent("d_PreMatSphere"),
        };
        
        public static void OnGUI()
        {
            EditorGUILayout.LabelField("Tools");
            _currentTool = (Tool)GUILayout.Toolbar((int)_currentTool, TOOLBAR_CONTENT);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Level Metadata", new GUIStyle("CN Box"));
            _currentLevelBlock.Update();
            {
                var expanse = _currentLevelBlock.FindProperty("_expanse");
                var height = _currentLevelBlock.FindProperty("_height");
                var entrance = _currentLevelBlock.FindProperty("_entrance");
                var exit = _currentLevelBlock.FindProperty("_exit");
                var expanseVal = expanse.vector4Value;

                EditorGUILayout.BeginHorizontal();
                expanseVal.x = EditorGUILayout.FloatField("Left", expanseVal.x);
                expanseVal.y = EditorGUILayout.FloatField("Right", expanseVal.y);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                expanseVal.z = EditorGUILayout.FloatField("Backwards", expanseVal.z);
                expanseVal.w = EditorGUILayout.FloatField("Forwards", expanseVal.w);
                EditorGUILayout.EndHorizontal();

                height.floatValue = EditorGUILayout.FloatField("Height", height.floatValue);

                expanseVal.x = Mathf.Max(expanseVal.x, 1);
                expanseVal.y = Mathf.Max(expanseVal.y, 1);
                expanseVal.z = Mathf.Max(expanseVal.z, 1);
                expanseVal.w = Mathf.Max(expanseVal.w, 1);
                height.floatValue = Mathf.Max(height.floatValue, 2);
                expanse.vector4Value = expanseVal;

                entrance.vector2Value = EditorGUILayout.Vector2Field("Entrance Point", entrance.vector2Value);
                exit.vector2Value = EditorGUILayout.Vector2Field("Exit Point", exit.vector2Value);

                entrance.vector2Value =
                    new Vector2(
                        Mathf.Clamp(entrance.vector2Value.x, -expanseVal.x + 1, expanseVal.y - 1),
                        Mathf.Clamp(entrance.vector2Value.y, 0, height.floatValue - 2));
                exit.vector2Value =
                    new Vector2(
                        Mathf.Clamp(exit.vector2Value.x, -expanseVal.x + 1, expanseVal.y - 1),
                        Mathf.Clamp(exit.vector2Value.y, 0, height.floatValue - 2));
            }
            _currentLevelBlock.ApplyModifiedProperties();

            if (_currentTool == Tool.Geometry)
            {
                for (var i = 0; i < _geometryIcons.Length; i++)
                {
                    var content = _geometryIcons[i];
                    var go = _geometryPrefabs[i].GameObject;
                    content.image = AssetPreview.GetAssetPreview(go);
                    content.tooltip = go.name;
                }
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Geometry", new GUIStyle("CN Box"));
                _currentGeometry = GUILayout.SelectionGrid(_currentGeometry, _geometryIcons, 4, GUILayout.MaxHeight(64));
            }
            if (_currentTool == Tool.Objects)
            {
                for (var i = 0; i < _objectIcons.Length; i++)
                {
                    var content = _objectIcons[i];
                    var go = _objectPrefabs[i].GameObject;
                    content.image = AssetPreview.GetAssetPreview(go);
                    content.tooltip = go.name;
                }
                _currentObject = GUILayout.SelectionGrid(_currentObject, _objectIcons, 4);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Selection", new GUIStyle("CN Box"));
        }
        
        public static void OnSceneGUI(SceneView sceneView)
        {
            _currentLevelBlock.Update();
            var expanse = _currentLevelBlock.FindProperty("_expanse").vector4Value;
            var height = _currentLevelBlock.FindProperty("_height").floatValue;
            var entrance = _currentLevelBlock.FindProperty("_entrance").vector2Value;
            var exit = _currentLevelBlock.FindProperty("_exit").vector2Value;

            var center = new Vector3(expanse.y - expanse.x, height, expanse.w - expanse.z) * 0.5f;
            var size = new Vector3(expanse.y + expanse.x, height, expanse.w + expanse.z);
            
            //Draw the level Block
            Handles.color = new Color(1, 1, 0, 0.75f);
            Handles.DrawWireCube(center, size);
            LevelEditorUtility.DrawGates(entrance, exit, expanse);

            switch (_currentTool)
            {
                case Tool.Bounds:
                    LevelEditorUtility.DrawExpanseHandles(ref expanse, ref height);
                    break;
                case Tool.Gates:
                    LevelEditorUtility.DrawGateHandle(ref entrance, ref exit, expanse, height);
                    break;
                case Tool.Geometry:
                    HandleGeometryTool(expanse);
                    break;
                case Tool.Objects:
                    break;
            }
            
            _currentLevelBlock.FindProperty("_expanse").vector4Value = expanse;
            _currentLevelBlock.FindProperty("_height").floatValue = height;
            _currentLevelBlock.FindProperty("_entrance").vector2Value = entrance;
            _currentLevelBlock.FindProperty("_exit").vector2Value = exit;
            _currentLevelBlock.ApplyModifiedProperties();
        }

        private static Vector3? _placementPoint;
        
        private static void HandleGeometryTool(Vector4 expanse)
        {
            if (Event.current.isMouse && _placementPoint.HasValue)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.keyCode == KeyCode.Mouse0)
                    {
                        if (!LevelEditorUtility.TrySelectGeometry(out _currentSelection))
                        {
                            _currentLevelBlock.Update();
                            var newObject = PrefabUtility.InstantiatePrefab(_geometryPrefabs[_currentGeometry].GameObject, _levelRoot.transform) as GameObject;
                            newObject.name = _geometryPrefabs[_currentGeometry].GameObject.name;
                            newObject.transform.position = _placementPoint.Value;
                            Undo.RegisterCreatedObjectUndo(newObject, "Add Level Geometry");
                            var levelObjects = _currentLevelBlock.FindProperty("_levelObjects");
                            levelObjects.InsertArrayElementAtIndex(levelObjects.arraySize);
                            var element = levelObjects.GetArrayElementAtIndex(levelObjects.arraySize - 1);
                            element.boxedValue = newObject;
                            _currentLevelBlock.ApplyModifiedProperties();
                        }

                        Event.current.Use();
                    }

                    if (Event.current.keyCode == KeyCode.Mouse1)
                    {
                        if (LevelEditorUtility.TrySelectGeometry(out _currentSelection))
                        {
                            _currentLevelBlock.Update();
                            Undo.RecordObject(_currentSelection.GameObject, "Delete Object");
                            Object.DestroyImmediate(_currentSelection.GameObject);
                            var levelObjects = _currentLevelBlock.FindProperty("_levelObjects");
                            for (var i = 0; i < levelObjects.arraySize; i++)
                            {
                                if (levelObjects.GetArrayElementAtIndex(i).boxedValue == null)
                                {
                                    levelObjects.DeleteArrayElementAtIndex(i);
                                }
                            }
                            _currentLevelBlock.ApplyModifiedProperties();
                            Event.current.Use();
                        }
                    }
                }
            }

            if (Event.current.keyCode != KeyCode.Mouse0)
            {
                _placementPoint = LevelEditorUtility.UpdatePlacement(expanse, _geometryPrefabs[_currentGeometry]);
            }
        }
    }
}