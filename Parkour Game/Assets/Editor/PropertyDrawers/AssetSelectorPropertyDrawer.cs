using System.IO;
using UnityEditor;
using UnityEngine;
using Utility;

namespace PropertyDrawers
{
    [CustomPropertyDrawer(typeof(AssetSelectorAttribute))]
    public class AssetSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (attribute is not AssetSelectorAttribute at) return;
            var fullPath = Application.dataPath + at.Path;
            var files = Directory.GetFiles(fullPath, $"*.{at.AssetType}");
            ArrayUtility.Insert(ref files, 0, "None");
            var index = 0;
            for (var i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i]);
                if (property.stringValue == files[i])
                {
                    index = i;
                }
            }

            position = EditorGUI.PrefixLabel(position, label);
            index = EditorGUI.Popup(position, index, files);
            property.stringValue = files[index];
        }
    }
}