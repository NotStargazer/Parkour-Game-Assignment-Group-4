using System;
using Level;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LevelEditor
{
    public static class LevelEditorUtility
    {
        //Short for solid cube verts
        private static Vector3[][] _scv = 
        {
            new Vector3[4], new Vector3[4], new Vector3[4], new Vector3[4], new Vector3[4], new Vector3[4]
        };
        
        public static void DrawSolidCube(Vector3 center, Vector3 size, Color color, Color outlineColor)
        {
            var h = size * 0.5f;
            var lbl = new Vector3(-h.x, -h.y, -h.z) + center;
            var lbr = new Vector3( h.x, -h.y, -h.z) + center;
            var lfl = new Vector3(-h.x, -h.y,  h.z) + center;
            var lfr = new Vector3( h.x, -h.y,  h.z) + center;
            var ubl = new Vector3(-h.x,  h.y, -h.z) + center;
            var ubr = new Vector3( h.x,  h.y, -h.z) + center;
            var ufl = new Vector3(-h.x,  h.y,  h.z) + center;
            var ufr = new Vector3( h.x,  h.y,  h.z) + center;

            _scv[0][0] = lbl;_scv[0][1] = ubl;_scv[0][2] = ufl;_scv[0][3] = lfl;
            _scv[1][0] = lbr;_scv[1][1] = ubr;_scv[1][2] = ufr;_scv[1][3] = lfr;
            _scv[2][0] = lbl;_scv[2][1] = ubl;_scv[2][2] = ubr;_scv[2][3] = lbr;
            _scv[3][0] = lfl;_scv[3][1] = ufl;_scv[3][2] = ufr;_scv[3][3] = lfr;
            _scv[4][0] = lbl;_scv[4][1] = lfl;_scv[4][2] = lfr;_scv[4][3] = lbr;
            _scv[5][0] = ubl;_scv[5][1] = ufl;_scv[5][2] = ufr;_scv[5][3] = ubr;
            
            //Left
            Handles.DrawSolidRectangleWithOutline(_scv[0], color, outlineColor);
            //Right
            Handles.DrawSolidRectangleWithOutline(_scv[1], color, outlineColor);
            //Back
            Handles.DrawSolidRectangleWithOutline(_scv[2], color, outlineColor);
            //Front
            Handles.DrawSolidRectangleWithOutline(_scv[3], color, outlineColor);
            //Bottom
            Handles.DrawSolidRectangleWithOutline(_scv[4] ,color, outlineColor);
            //Top
            Handles.DrawSolidRectangleWithOutline(_scv[5], color, outlineColor);
        }

        private readonly struct BoundsPlane
        {
            public readonly float Distance;
            public readonly int Index;

            public BoundsPlane(float distance, int index)
            {
                Distance = distance;
                Index = index;
            }
        }

        private static BoundsPlane[] _bp = new BoundsPlane[5];
        
        public static void DrawExpanseHandles(ref Vector4 expanse, ref float height)
        {
            var camRight = SceneView.lastActiveSceneView.camera.transform.right;
            var camUp = SceneView.lastActiveSceneView.camera.transform.up;
            var camPos = SceneView.lastActiveSceneView.camera.transform.position;
            var delta = Event.current.delta;
            var invDelta = -Event.current.delta;

            var leftPos = new Vector3(-expanse.x, height * 0.5f, (expanse.w - expanse.z) * 0.5f);
            var rightPos = new Vector3(expanse.y, height * 0.5f, (expanse.w - expanse.z) * 0.5f);
            var backPos = new Vector3((expanse.y - expanse.x) * 0.5f, height * 0.5f, -expanse.z);
            var forwardPos = new Vector3((expanse.y - expanse.x) * 0.5f, height * 0.5f, expanse.w);
            var upPos = new Vector3((expanse.y - expanse.x) * 0.5f, height, (expanse.w - expanse.z) * 0.5f);

            if (Event.current.isMouse && Event.current.type != EventType.MouseDrag)
            {
                _bp[0] = new BoundsPlane(Vector3.Distance(camPos, leftPos), 1);
                _bp[1] = new BoundsPlane(Vector3.Distance(camPos, rightPos), 2);
                _bp[2] = new BoundsPlane(Vector3.Distance(camPos, backPos), 3);
                _bp[3] = new BoundsPlane(Vector3.Distance(camPos, forwardPos), 4);
                _bp[4] = new BoundsPlane(Vector3.Distance(camPos, upPos), 5);
                
                Array.Sort(_bp, (x, y) =>
                {
                    if (x.Distance > y.Distance)
                    {
                        return -1;
                    }

                    if (x.Distance < y.Distance)
                    {
                        return 1;
                    }

                    return 0;
                });
            }
            
            foreach (var bp in _bp)
            {
                switch (bp.Index)
                {
                    case 1:
                        Event.current.delta = Vector3.Dot(camRight, Vector3.left) > 0 ? delta : invDelta;
                        _rectHandleSize = new Vector2(expanse.z + expanse.w, height);
                        expanse.x = Handles.ScaleValueHandle(expanse.x, leftPos, Quaternion.Euler(0, 90, 0),
                            HandleUtility.GetHandleSize(leftPos),
                            VariableRectangleHandleCap, 0);
                        if (EditorSnapSettings.gridSnapEnabled && Event.current.GetTypeForControl(GUIUtility.hotControl) == EventType.Used)
                        {
                            expanse.x = Snapping.Snap(expanse.x, EditorSnapSettings.gridSize.x);
                        }
                        break;
                    case 2:
                        Event.current.delta = Vector3.Dot(camRight, Vector3.right) > 0 ? delta : invDelta;
                        _rectHandleSize = new Vector2(expanse.z + expanse.w, height);
                        expanse.y = Handles.ScaleValueHandle(expanse.y, rightPos, Quaternion.Euler(0, 90, 0), 
                            HandleUtility.GetHandleSize(rightPos),
                            VariableRectangleHandleCap, 0);
                        if (EditorSnapSettings.gridSnapEnabled && Event.current.GetTypeForControl(GUIUtility.hotControl) == EventType.Used)
                        {
                            expanse.y = Snapping.Snap(expanse.y, EditorSnapSettings.gridSize.x);
                        }
                        break;
                    case 3:
                        Event.current.delta = Vector3.Dot(camRight, Vector3.back) > 0 ? delta : invDelta;
                        _rectHandleSize = new Vector2(expanse.x + expanse.y, height);
                        expanse.z = Handles.ScaleValueHandle(expanse.z, backPos, Quaternion.Euler(0, 0, 0), 
                            HandleUtility.GetHandleSize(backPos),
                            VariableRectangleHandleCap, 0);
                        if (EditorSnapSettings.gridSnapEnabled && Event.current.GetTypeForControl(GUIUtility.hotControl) == EventType.Used)
                        {
                            expanse.z = Snapping.Snap(expanse.z, EditorSnapSettings.gridSize.z);
                        }
                        break;
                    case 4:
                        Event.current.delta = Vector3.Dot(camRight, Vector3.forward) > 0 ? delta : invDelta;
                        _rectHandleSize = new Vector2(expanse.x + expanse.y, height);
                        expanse.w = Handles.ScaleValueHandle(expanse.w, forwardPos, Quaternion.Euler(0, 0, 0), 
                            HandleUtility.GetHandleSize(forwardPos),
                            VariableRectangleHandleCap, 0);
                        if (EditorSnapSettings.gridSnapEnabled && Event.current.GetTypeForControl(GUIUtility.hotControl) == EventType.Used)
                        {
                            expanse.w = Snapping.Snap(expanse.w, EditorSnapSettings.gridSize.z);
                        }
                        break;
                    case 5:
                        Event.current.delta = Vector3.Dot(camUp, Vector3.up) > 0 ? delta : invDelta;
                        _rectHandleSize = new Vector2(expanse.x + expanse.y, expanse.z + expanse.w);
                        var ctrl = GUIUtility.GetControlID(FocusType.Passive);
                        height = Handles.ScaleValueHandle(ctrl, height, upPos, Quaternion.Euler(90, 0, 0), 
                            HandleUtility.GetHandleSize(upPos),
                            VariableRectangleHandleCap, 0);
                        if (EditorSnapSettings.gridSnapEnabled && Event.current.GetTypeForControl(GUIUtility.hotControl) == EventType.Used)
                        {
                            height = Snapping.Snap(height, EditorSnapSettings.gridSize.y);
                        }
                        break;
                }
            }

            Event.current.delta = delta;
        }

        private static Vector2 _rectHandleSize;
        //Short for rect verts
        private static Vector3[] _rv = new Vector3[4];
        
        public static void VariableRectangleHandleCap(
            int controlID,
            Vector3 position,
            Quaternion rotation,
            float _,
            EventType eventType)
        {
            var size = _rectHandleSize * 0.5f;
            switch (eventType)
            {
                case EventType.MouseMove:
                case EventType.Layout:
                    HandleUtility.AddControl(controlID, DistanceToRectangle(position, rotation, size));
                    break;
                case EventType.Repaint:
                    var x = rotation * new Vector3(size.x, 0, 0);
                    var y = rotation * new Vector3(0, size.y, 0);
                    _rv[0] = position + x + y;
                    _rv[1] = position + x - y;
                    _rv[2] = position - x - y;
                    _rv[3] = position - x + y;
                    Handles.DrawSolidRectangleWithOutline(_rv,
                        new Color(1,1,0, 0.05f),
                        Color.clear);
                    break;
            }
        }
        
        //Short for distance rect verts
        private static readonly Vector3[] _drv = new Vector3[5];
        
        public static float DistanceToRectangle(
            Vector3 position,
            Quaternion rotation,
            Vector2 size)
        {
            var x = rotation * new Vector3(size.x, 0.0f, 0.0f);
            var y = rotation * new Vector3(0.0f, size.y, 0.0f);
            _drv[0] = HandleUtility.WorldToGUIPoint(position + x + y);
            _drv[1] = HandleUtility.WorldToGUIPoint(position + x - y);
            _drv[2] = HandleUtility.WorldToGUIPoint(position - x - y);
            _drv[3] = HandleUtility.WorldToGUIPoint(position - x + y);
            _drv[4] = _drv[0];
            var mousePosition = Event.current.mousePosition;
            var withinRect = false;
            var prevI = 4;
            for (var vi = 0; vi < 5; ++vi)
            {
                if ((double) _drv[vi].y > mousePosition.y != 
                    _drv[prevI].y > (double) mousePosition.y
                    && mousePosition.x < (_drv[prevI].x - (double)_drv[vi].x) 
                    * (mousePosition.y - (double) _drv[vi].y)
                    / (_drv[prevI].y - (double) _drv[vi].y) + _drv[vi].x)
                {
                    withinRect = !withinRect;
                }
                prevI = vi;
            }
            if (withinRect)
            {
                return 0.0f;
            }
            var rectangleInternal = -1f;
            var viNext = 1;
            for (var vi = 0; vi < 4; ++vi)
            {
                var lineSegment = HandleUtility.DistancePointToLineSegment(
                    mousePosition, _drv[vi], _drv[viNext++]);
                if (lineSegment < (double) rectangleInternal || rectangleInternal < 0.0)
                {
                    rectangleInternal = lineSegment;
                }
            }
            return rectangleInternal;
        }

        public static void DrawGates(Vector2 entrance, Vector2 exit, Vector4 expanse)
        {
            var entrancePos = new Vector3(entrance.x, entrance.y, -expanse.z);
            var exitPos = new Vector3(exit.x, exit.y, expanse.w);
            entrancePos.y += 1f;
            exitPos.y += 1f;
            
            Handles.color = new Color(0.2f, 0.6f, 1f, 0.2f);
            Handles.DrawSolidDisc(entrancePos, Vector3.forward, 1f);
            Handles.color = new Color(1f, 0.6f, 0.2f, 0.2f);
            Handles.DrawSolidDisc(exitPos, Vector3.forward, 1f);
        }
        
        public static void DrawGateHandle(ref Vector2 entrance, ref Vector2 exit, Vector4 expanse, float height)
        {
            var entrancePos = new Vector3(entrance.x, entrance.y, -expanse.z);
            var exitPos = new Vector3(exit.x, exit.y, expanse.w);
            entrancePos.y += 1f;
            exitPos.y += 1f;
            
            Handles.color = new Color(0.2f, 0.6f, 1f, 1f);
            var outVal = Handles.Slider2D(entrancePos,
                Vector3.forward, Vector3.right, Vector3.up,
                1f, Handles.CircleHandleCap, EditorSnapSettings.gridSize.x);

            entrance = new Vector2(outVal.x, outVal.y - 1f);
            
            Handles.color = new Color(1f, 0.6f, 0.2f, 1f);
            outVal = Handles.Slider2D(exitPos, 
                Vector3.forward, Vector3.right, Vector3.up,
                1f, Handles.CircleHandleCap, EditorSnapSettings.gridSize.x);
            
            exit = new Vector2(outVal.x, outVal.y - 1f);
            
            entrance =
                new Vector2(
                    Mathf.Clamp(entrance.x, -expanse.x + 1, expanse.y - 1),
                    Mathf.Clamp(entrance.y, 0, height - 2));
            exit =
                new Vector2(
                    Mathf.Clamp(exit.x, -expanse.x + 1, expanse.y - 1),
                    Mathf.Clamp(exit.y, 0, height - 2));
            
        }

        public static bool TrySelectGeometry(out ILevelObject levelObject)
        {
            levelObject = null;
            var go = HandleUtility.PickGameObject(Event.current.mousePosition, false);
            return go && go.TryGetComponent(out levelObject);
        }
        
        public static Vector3? UpdatePlacement(Vector4 expanse, float height, ILevelObject levelObject)
        {
            var camera = SceneView.lastActiveSceneView.camera;
            var mousePos = Event.current.mousePosition;
            var flippedMousePos = new Vector2(mousePos.x, camera.pixelHeight - mousePos.y);
            var ray = camera.ScreenPointToRay(flippedMousePos);
            var plane = new Plane(Vector3.up, Vector3.zero);

            Handles.color = Color.white;
            
            if (plane.Raycast(ray, out var enter))
            {
                var point = ray.GetPoint(enter);
                point = new Vector3(
                    Mathf.Clamp(point.x, 0.5f - expanse.x, expanse.y - 0.5f),
                    height,
                    Mathf.Clamp(point.z, 0.5f - expanse.z, expanse.w - 0.5f));
                point = Snapping.Snap(point, EditorSnapSettings.gridSize);
                Handles.color = new Color(0.75f, 0.9f, 1f, 0.35f);
                var prior = Handles.zTest;
                Handles.zTest = CompareFunction.LessEqual;
                if (!levelObject.Mesh)
                {
                    levelObject.Regenerate();
                }
                DrawMesh(levelObject.Mesh, point, levelObject.Scale, levelObject.Rotation);
                Handles.zTest = prior;
                return point;
            }
            
            return Vector3.zero;
        }

        public static void DrawMesh(Mesh mesh, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            for (var i = 0; i < mesh.triangles.Length; i += 3)
            {
                var t1 = mesh.triangles[i];
                var t2 = mesh.triangles[i + 1];
                var t3 = mesh.triangles[i + 2];
                
                var v1 = rotation * new Vector3(mesh.vertices[t1].x * scale.x, mesh.vertices[t1].y * scale.y, mesh.vertices[t1].z * scale.z) + position;
                var v2 = rotation * new Vector3(mesh.vertices[t2].x * scale.x, mesh.vertices[t2].y * scale.y, mesh.vertices[t2].z * scale.z) + position;
                var v3 = rotation * new Vector3(mesh.vertices[t3].x * scale.x, mesh.vertices[t3].y * scale.y, mesh.vertices[t3].z * scale.z) + position;
                var v4 = rotation * new Vector3(mesh.vertices[t1].x * scale.x, mesh.vertices[t1].y * scale.y, mesh.vertices[t1].z * scale.z) + position;
                
                Handles.DrawAAConvexPolygon(v1, v2, v3, v4);
            }
        }

        public static void SelectionHandles(ref Vector3 posVal, ref Vector3 sclVal, ref Quaternion rotVal, bool posOnly)
        {
            if (Event.current.type == EventType.MouseDown)
            {
                for (var index = 0; index < HV.Length; index++)
                {
                    HV[index] = 1f;
                }
            }

            if (posOnly)
            {
                posVal = Handles.DoPositionHandle(posVal, Quaternion.identity);
                return;
            }
            
            switch (Tools.current)
            {
                case Tool.Rotate:
                    rotVal = Handles.DoRotationHandle(rotVal, posVal);
                    break;
                case Tool.Scale:
                    SideExpandHandle(ref posVal, ref sclVal);
                    break;
                default:
                    posVal = Handles.DoPositionHandle(posVal, Quaternion.identity);
                    break;
            }
        }

        //Short for handle values
        private static readonly float[] HV = { 1f, 1f, 1f, 1f, 1f, 1f };
        
        public static void SideExpandHandle(ref Vector3 position, ref Vector3 scale)
        {
            Handles.color = Handles.xAxisColor;
            var val = Handles.ScaleSlider(HV[0], position,
                Vector3.left, Quaternion.identity,
                HandleUtility.GetHandleSize(position), EditorSnapSettings.gridSize.x);
            var difference = val - HV[0];
            HV[0] = val;
            position.x -= difference * 0.5f;
            scale.x += difference;
            
            val = Handles.ScaleSlider(HV[1], position,
                Vector3.right, Quaternion.identity,
                HandleUtility.GetHandleSize(position), EditorSnapSettings.gridSize.x);
            difference = val - HV[1];
            HV[1] = val;
            position.x += difference * 0.5f;
            scale.x += difference;
            
            Handles.color = Handles.yAxisColor;
            val = Handles.ScaleSlider(HV[2], position,
                Vector3.down, Quaternion.identity,
                HandleUtility.GetHandleSize(position), EditorSnapSettings.gridSize.y);
            difference = val - HV[2];
            HV[2] = val;
            position.y -= difference * 0.5f;
            scale.y += difference;
            
            val = Handles.ScaleSlider(HV[3], position,
                Vector3.up, Quaternion.identity,
                HandleUtility.GetHandleSize(position), EditorSnapSettings.gridSize.y);
            difference = val - HV[3];
            HV[3] = val;
            position.y += difference * 0.5f;
            scale.y += difference;
            
            Handles.color = Handles.zAxisColor;
            val = Handles.ScaleSlider(HV[4], position,
                Vector3.back, Quaternion.identity,
                HandleUtility.GetHandleSize(position), EditorSnapSettings.gridSize.z);
            difference = val - HV[4];
            HV[4] = val;
            position.z -= difference * 0.5f;
            scale.z += difference;
            
            val = Handles.ScaleSlider(HV[5], position,
                Vector3.forward, Quaternion.identity,
                HandleUtility.GetHandleSize(position), EditorSnapSettings.gridSize.z);
            difference = val - HV[5];
            HV[5] = val;
            position.z += difference * 0.5f;
            scale.z += difference;
        }
    }
}