using UnityEditor;
using UnityEngine;

namespace LevelEditor
{
    public static class LevelEditorUtility
    {
        public static void DrawSolidCube(Vector3 center, Vector3 size, Color color, Color outlineColor)
        {
            var h = size * 0.5f;
            var lbl = new Vector3(-h.x,  h.y, -h.z) + center;
            var lbr = new Vector3( h.x,  h.y, -h.z) + center;
            var lfl = new Vector3(-h.x,  h.y,  h.z) + center;
            var lfr = new Vector3( h.x,  h.y,  h.z) + center;
            var ubl = new Vector3(-h.x, -h.y, -h.z) + center;
            var ubr = new Vector3( h.x, -h.y, -h.z) + center;
            var ufl = new Vector3(-h.x, -h.y,  h.z) + center;
            var ufr = new Vector3( h.x, -h.y,  h.z) + center;
            
            //Left
            Handles.DrawSolidRectangleWithOutline(new [] { lbl, ubl, ufl, lfl },
                color, outlineColor);
            //Right
            Handles.DrawSolidRectangleWithOutline(new [] { lbr, ubr, ufr, lfr },
                color, outlineColor);
            //Back
            Handles.DrawSolidRectangleWithOutline(new [] { lbl, ubl, ubr, lbr },
                color, outlineColor);
            //Front
            Handles.DrawSolidRectangleWithOutline(new [] { lfl, ufl, ufr, lfr },
                color, outlineColor);
            //Bottom
            Handles.DrawSolidRectangleWithOutline(new [] { lbl, lfl, lfr, lbr },
                color, outlineColor);
            //Top
            Handles.DrawSolidRectangleWithOutline(new [] { ubl, ufl, ufr, ubr },
                color, outlineColor);
        }

        public static void DrawBoundsHandles(ref Vector4 expanse, ref float height)
        {
            expanse.x = Handles.ScaleValueHandle(expanse.x,
                new Vector3(-expanse.x, height * 0.5f, 0), Quaternion.Euler(0, 90, 0), 2,
                Handles.RectangleHandleCap, EditorSnapSettings.gridSize.x);
            expanse.y = Handles.ScaleValueHandle(expanse.y,
                new Vector3(expanse.y, height * 0.5f, 0), Quaternion.Euler(0, 90, 0), 2,
                Handles.RectangleHandleCap, EditorSnapSettings.gridSize.x);
            expanse.z = Handles.ScaleValueHandle(expanse.z,
                new Vector3(0, height * 0.5f, -expanse.z), Quaternion.Euler(0, 0, 0), 2,
                Handles.RectangleHandleCap, EditorSnapSettings.gridSize.z);
            expanse.w = Handles.ScaleValueHandle(expanse.w,
                new Vector3(0, height * 0.5f, expanse.w), Quaternion.Euler(0, 0, 0), 2,
                Handles.RectangleHandleCap, EditorSnapSettings.gridSize.z);
        }
    }
}