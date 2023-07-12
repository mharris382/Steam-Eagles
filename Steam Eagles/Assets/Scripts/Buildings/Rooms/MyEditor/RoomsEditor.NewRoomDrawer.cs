#if UNITY_EDITOR
using System;
using CoreLib;
using UnityEditor;
using UnityEngine;

namespace Buildings.Rooms.MyEditor
{
    public class GridHelper
    {
        private readonly Grid _grid;
        public Grid Grid => _grid;

        public GridHelper(Rooms rooms)
        {
            _grid = rooms.GetComponent<Grid>();
            var allGrids = rooms.GetComponentsInChildren<Grid>();
            Vector3 largestCellSize = Vector3.negativeInfinity;
            foreach (var grid in allGrids)
            {
                if (grid.cellSize.x > largestCellSize.x)
                {
                    largestCellSize = grid.cellSize;
                    _grid = grid;
                }
            }
        }
        
        
        public Vector3 SnappedPoint(Vector3 original)
        {
            var cell = _grid.WorldToCell(original);
            return _grid.CellToWorld(cell);
        }
    }
    public partial class RoomsEditor
    {
        public class NewRoomDrawer 
        {
            private readonly global::Buildings.Rooms.Rooms _rooms;
            private readonly Grid _grid;
            public NewRoomDrawer(global::Buildings.Rooms.Rooms rooms)
            {
                _rooms = rooms;
                _grid = rooms.GetComponent<Grid>();
                var allGrids = rooms.GetComponentsInChildren<Grid>();
                Vector3 largestCellSize = Vector3.negativeInfinity;
                foreach (var grid in allGrids)
                {
                    if (grid.cellSize.x > largestCellSize.x)
                    {
                        largestCellSize = grid.cellSize;
                        _grid = grid;
                    }
                }
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

       
            public bool IsDrawingSecondPoint { get; private set; }
            
            public Vector3 FirstPoint { get; private set; }
            public Vector3 SecondPoint { get; private set; }
            
            public Vector3 SelectedPoint { get; private set; }

            public Vector3Int FirstCell => _grid.WorldToCell(FirstPoint);
            public Vector3Int SecondCell => _grid.WorldToCell(SecondPoint);
            
            public Vector3 SnappedFirstPoint => _grid.CellToWorld(FirstCell);
            public Vector3 SnappedSecondPoint => _grid.CellToWorld(SecondCell);

            public Vector3 FirstPointLocal => _rooms.Building.transform.InverseTransformPoint(FirstPoint);

            Vector3 SnappedPoint(Vector3 original)
            {
                var cell = _grid.WorldToCell(original);
                return _grid.CellToWorld(cell);
            }
            public Rect GetWorldSpaceRect()
            {
                if (!IsFinished)
                {
                    throw new Exception();
                }

                var minX = Mathf.Min(SnappedFirstPoint.x, SnappedSecondPoint.x);
                var minY = Mathf.Min(SnappedFirstPoint.y, SnappedSecondPoint.y);
                var maxX = Mathf.Max(SnappedFirstPoint.x, SnappedSecondPoint.x);
                var maxY = Mathf.Max(SnappedFirstPoint.y, SnappedSecondPoint.y);
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
     
            public bool IsValid => _rooms.HasBuilding;
            
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


            public void OnSceneGUI()
            {
                var position = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                position.z = 0;
                DoPreviewAction(SnappedPoint(position));
            }


            private void DoPreviewAction(Vector3 position)
            {
                void DrawSelectedPoint(Vector3 validPoint)
                {
                    Handles.DrawWireDisc(SnappedPoint(validPoint), Vector3.forward, 0.5f);
                }
                void DrawSelectedArea(Vector3 validPoint)
                {
                    Handles.DrawWireDisc(SnappedFirstPoint, Vector3.forward, 0.5f);
                    Handles.DrawWireDisc(SnappedPoint(validPoint), Vector3.forward, 0.5f);
                    Vector3[] verts = new Vector3[4]
                    {
                        new Vector3(FirstPoint.x, FirstPoint.y, 0),
                        new Vector3(FirstPoint.x, position.y, 0),
                        new Vector3(position.x, position.y, 0),
                        new Vector3(position.x, FirstPoint.y, 0),
                    };
                    for (int i = 0; i < verts.Length; i++)
                    {
                        verts[i] = SnappedPoint(verts[i]);
                    }
                    Handles.DrawSolidRectangleWithOutline(verts, new Color(1, 0, 0, 0.2f), Color.red.Lighten(0.5f));
                }

                using (new HandlesScope(Color.red))
                {
                    var validPoint = position;
                    if (IsDrawingSecondPoint)
                    {
                        DrawSelectedArea(SnappedPoint(validPoint));
                    }
                    else
                    {
                        DrawSelectedPoint(SnappedPoint(validPoint));
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