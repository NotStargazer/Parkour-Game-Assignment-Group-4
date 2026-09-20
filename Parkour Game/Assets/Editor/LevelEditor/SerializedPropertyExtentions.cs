using System;
using System.Reflection;
using UnityEditor;

namespace LevelEditor
{
    public static class SerializedPropertyExtensions
    {
        public static bool HasAttributes<T>(this SerializedProperty prop) where T : Attribute
        {
            var target = prop.serializedObject.targetObject;
            var type = target.GetType();

            var field = type.GetField(prop.propertyPath,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            return field != null && Attribute.IsDefined(field, typeof(T));
        }
    }
}