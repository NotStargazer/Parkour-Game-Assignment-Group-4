using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LevelEditor
{
    public static class LevelEditor
    {
        private enum Tool
        {
            Bounds,
            Gates,
            Select,
            Geometry,
            Objects,
        }
        
        private const string LEVEL_ASSET_PATH = "Assets/Level Blocks/";
        
        private static SerializedObject _currentLevelBlock;
        private static GameObject _levelRoot;

        private static SerializedObject[] _components;

        private static Tool _currentTool;
        
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
        
        public static void OpenLevelForEditing(GameObject prefabRoot)
        {
            LevelRoot = prefabRoot;
            _currentLevelBlock = prefabRoot.TryGetComponent(out LevelBlock levelBlock)
                ? new SerializedObject(levelBlock)
                : new SerializedObject(prefabRoot.AddComponent<LevelBlock>());
        }
        
        public static void ChangeSelection(GameObject selection)
        {
            var components = selection.GetComponents<Component>();
        }
        
        public static void OnGUI()
        {
            EditorGUILayout.LabelField("Tools");
            _currentTool = (Tool)GUILayout.Toolbar((int)_currentTool, new []
            {
                EditorGUIUtility.IconContent("d_RectTool On"),
                EditorGUIUtility.IconContent("d_ToolHandleCenter"),
                EditorGUIUtility.IconContent("d_Grid.Default"),
                EditorGUIUtility.IconContent("d_PreMatCube"),
                EditorGUIUtility.IconContent("d_PreMatSphere"),
            });
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Level Metadata", new GUIStyle("CN Box"));
            _currentLevelBlock.Update();
            {
                var expanse = _currentLevelBlock.FindProperty("_expanse");
                var height = _currentLevelBlock.FindProperty("_height");
                var val = expanse.vector4Value;

                EditorGUILayout.BeginHorizontal();
                val.x = EditorGUILayout.FloatField("Left", val.x);
                val.y = EditorGUILayout.FloatField("Right", val.y);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                val.z = EditorGUILayout.FloatField("Forward", val.z);
                val.w = EditorGUILayout.FloatField("Backwards", val.w);
                EditorGUILayout.EndHorizontal();

                height.floatValue = EditorGUILayout.FloatField("Height", height.floatValue);

                val.x = Mathf.Max(val.x, 1);
                val.y = Mathf.Max(val.y, 1);
                val.z = Mathf.Max(val.z, 1);
                val.w = Mathf.Max(val.w, 1);
                height.floatValue = Mathf.Max(height.floatValue, 2);

                expanse.vector4Value = val;
            }
            _currentLevelBlock.ApplyModifiedProperties();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Selection", new GUIStyle("CN Box"));
        }
        
        public static void OnSceneGUI(SceneView sceneView)
        {
            _currentLevelBlock.Update();
            var expanse = _currentLevelBlock.FindProperty("_expanse").vector4Value;
            var height = _currentLevelBlock.FindProperty("_height").floatValue;

            var center = new Vector3(expanse.y - expanse.x, height, expanse.w - expanse.z) * 0.5f;
            var size = new Vector3(expanse.y + expanse.x, height, expanse.w + expanse.z);
            
            //Draw the level Block
            LevelEditorUtility.DrawSolidCube(center, size, 
                new Color(1f, 1f, 0f, 0.05f),
                new Color(1f, 1f, 0f, 0.75f));

            switch (_currentTool)
            {
                case Tool.Bounds:
                    LevelEditorUtility.DrawBoundsHandles(ref expanse, ref height);
                    break;
                case Tool.Gates:
                    break;
                case Tool.Select:
                    break;
                case Tool.Geometry:
                    break;
                case Tool.Objects:
                    break;
            }
            
            _currentLevelBlock.FindProperty("_expanse").vector4Value = expanse;
            _currentLevelBlock.FindProperty("_height").floatValue = height;
            _currentLevelBlock.ApplyModifiedProperties();
        }
    }
}