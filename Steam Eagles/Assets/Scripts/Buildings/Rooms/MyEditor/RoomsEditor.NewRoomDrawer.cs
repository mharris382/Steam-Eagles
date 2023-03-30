#if UNITY_EDITOR
using System;
using CoreLib;
using UnityEditor;
using UnityEngine;

namespace Buildings.Rooms.MyEditor
{
    public partial class RoomsEditor
    {
        public class NewRoomDrawer 
        {
            public NewRoomDrawer(global::Buildings.Rooms.Rooms rooms)
            {
                _rooms = rooms;
            }
            
            public bool IsFinished {
                get; 
                private set;
            }

            private bool _isDrawing;
            public bool IsDrawing
            {
                get => _isDrawing;
                set
                {
                    if (_isDrawing != value)
                    {
                        _isDrawing = value;
                        if (value)
                        {
                            SceneView.beforeSceneGui += BeforeSceneGUI;
                        }
                        else
                        {
                            SceneView.beforeSceneGui -= BeforeSceneGUI;
                        }
                    }
                }
            }

            private void BeforeSceneGUI(SceneView obj)
            {
                var position = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                position.z = 0;
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    DoConfirmButton(position);
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                {
                    DoCancelButton();
                    Event.current.Use();
                }
                else
                {
                    DoPreviewAction(position);
                }
            }

            public bool IsDrawingSecondPoint { get; private set; }
            
            public Vector3 FirstPoint { get; private set; }
            public Vector3 SecondPoint { get; private set; }
            
            public Vector3 SelectedPoint { get; private set; }
            
            

            public Vector3 FirstPointLocal => _rooms.Building.transform.InverseTransformPoint(FirstPoint);

            public Rect GetWorldSpaceRect()
            {
                if (!IsFinished)
                {
                    throw new Exception();
                }

                var minX = Mathf.Min(FirstPoint.x, SecondPoint.x);
                var minY = Mathf.Min(FirstPoint.y, SecondPoint.y);
                var maxX = Mathf.Max(FirstPoint.x, SecondPoint.x);
                var maxY = Mathf.Max(FirstPoint.y, SecondPoint.y);
                return Rect.MinMaxRect(minX, minY, maxX, maxY);
                var center = (FirstPoint + SecondPoint) / 2f;
                var size = SecondPoint - FirstPoint;
                return new Rect(center, size);
            }

            private Bounds GetLocalSpaceBounds()
            {
                var buildingTransform = _rooms.Building.transform;
                var wsRect = GetWorldSpaceRect();
                var center = buildingTransform.InverseTransformPoint(wsRect.center);
                var bounds = new Bounds(center, wsRect.size);
                return bounds;
            }
            private Bounds GetLocalSpaceBounds(Vector3 firstPoint, Vector3 secondPoint)
            {
                var minX = Mathf.Min(firstPoint.x, secondPoint.x);
                var minY = Mathf.Min(firstPoint.y, secondPoint.y);
                var maxX = Mathf.Max(firstPoint.x, secondPoint.x);
                var maxY = Mathf.Max(firstPoint.y, secondPoint.y);
                var rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
                var buildingTransform = _rooms.Building.transform;
                var wsRect = rect;
                var center = buildingTransform.InverseTransformPoint(wsRect.center);
                var bounds = new Bounds(center, wsRect.size);
                return bounds;
            }
            private Bounds GetBounds(Vector3 firstPoint, Vector3 secondPoint)
            {
                var minX = Mathf.Min(firstPoint.x, secondPoint.x);
                var minY = Mathf.Min(firstPoint.y, secondPoint.y);
                var maxX = Mathf.Max(firstPoint.x, secondPoint.x);
                var maxY = Mathf.Max(firstPoint.y, secondPoint.y);
                var rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
                var bounds = new Bounds((firstPoint + secondPoint)/2f, (firstPoint - secondPoint));
                return bounds;
            }
            private readonly global::Buildings.Rooms.Rooms _rooms;
            public bool IsValid => _rooms.HasBuilding;


            public void OnSceneGUI()
            {
                var position = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                position.z = 0;
                DoPreviewAction(position);
            }


            private void DoPreviewAction(Vector3 position)
            {
                void DrawSelectedPoint(Vector3 validPoint)
                {
                    Handles.DrawWireDisc(validPoint, Vector3.forward, 0.5f);
                }
                void DrawSelectedArea(Vector3 validPoint)
                {
                    Handles.DrawWireDisc(FirstPoint, Vector3.forward, 0.5f);
                    Handles.DrawWireDisc(validPoint, Vector3.forward, 0.5f);
                    Vector3[] verts = new Vector3[4]
                    {
                        new Vector3(FirstPoint.x, FirstPoint.y, 0),
                        new Vector3(FirstPoint.x, position.y, 0),
                        new Vector3(position.x, position.y, 0),
                        new Vector3(position.x, FirstPoint.y, 0),
                    };
                    Handles.DrawSolidRectangleWithOutline(verts, new Color(1, 0, 0, 0.2f), Color.red.Lighten(0.5f));
                }

                using (new HandlesScope(Color.red))
                {
                    var validPoint = position;
                    if (IsDrawingSecondPoint)
                    {
                        DrawSelectedArea(validPoint);
                    }
                    else
                    {
                        DrawSelectedPoint(validPoint);
                    }
                }
            }

            private Vector3 Local(Vector3 position)
            {
                return _rooms.Building.transform.InverseTransformPoint(position);
            }

            private Vector3 World(Vector3 position)
            {
                return _rooms.Building.transform.TransformPoint(position);
            }
            
            
            
            private void DoConfirmButton(Vector3 position)
            {
                if (!IsDrawingSecondPoint)
                {
                    FirstPoint = position;
                    IsDrawingSecondPoint = true;
                    return;
                }
                else
                {
                    SecondPoint = position;
                    IsFinished = true;
                }
            }
            
            private void DoCancelButton()
            {
                if(IsDrawingSecondPoint)
                {
                    IsDrawingSecondPoint = false;
                    return;
                }
                Dispose();
            }

            
            public void StartDrawing()
            {
                if(IsDrawing) return;
                if(!IsValid) return;
                IsDrawing = true;
                IsDrawingSecondPoint = false;
                IsFinished = false;
            }

            public void Dispose()
            {
                IsFinished = false;
                IsDrawing = false;
                IsDrawingSecondPoint = false;
            }
        }
    }
}
#endif