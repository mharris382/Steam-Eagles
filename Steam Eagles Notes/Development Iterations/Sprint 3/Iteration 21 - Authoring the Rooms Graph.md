%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 20: Authoring the Rooms Graph
Previous Iteration: 
Next Iteration: 


## Goal
create a rooms graph editor 


### Hypothesis


----
## Result


### Room Graph Classes
![[Rooms Graph]]

![[EditableRoomGraph]]

----
### Graph Editor Class
```cs
  public class RoomsGraphEditor
    {
        private readonly Rooms _target;
        private bool _isGraphDirty;
        private bool _isEditingGraph;
        private EditableRoomGraph _editableGraph;
        private Func<Transform> _buildingTransform;
        private List<(Vector3, Vector3)> _graphLines;
        
        private Room _selectedRoom;
        
        public RoomsGraphEditor(Rooms target)
        {
            _target = target;
            _buildingTransform = () => target.Building != null ? target.Building.transform : null;
            _editableGraph = new EditableRoomGraph(target);
            _isEditingGraph = false;
        }

        public void Cleanup()
        {
            _selectedRoom = null;
            _isEditingGraph = false;
        }

        public void OnSceneGUI(float handleSize)
        {
            var building = _buildingTransform();
            if (building == null) return;
            if (_isEditingGraph)
            {
                HashSet<Vector3> roomWithDiscs = new HashSet<Vector3>();

                void DrawDisc(Vector3 centerWs)
                {
                    if(roomWithDiscs.Contains(centerWs)) return;
                    Handles.DrawSolidDisc(centerWs, Vector3.forward, handleSize);
                    roomWithDiscs.Add(centerWs);
                }
                var lines = GetGraphLines(building);
                foreach (var line in lines)
                {
                    var p0 = line.Item1;
                    var p1 = line.Item2;
                    Handles.DrawLine(p0, p1);
                    DrawDisc(p0);
                    DrawDisc(p1);
                }
            }
        }
        
        public void OnPreSceneGUI(float handleSize)
        {
            if (_isEditingGraph)
            {
                var bt = _buildingTransform();
                if(bt == null) return;
                
                
                if (Event.current.control)
                {
                    using (new HandlesScope(Color.red))
                    {
                        DrawRemoveEdgeHandles(handleSize);
                    }
                    return;
                }
                
                using (new HandlesScope(Color.yellow))
                {
                    var pos = HandleUtility.WorldToGUIPoint(Event.current.mousePosition);
                    var lsPos = (Vector2)bt.TransformPoint(pos);

                    var hoveringRoom = GetHoveringRoom(lsPos);
                    var wsPos = (Vector3)pos;
                    DrawLineBetweenRooms(handleSize, wsPos, _selectedRoom, hoveringRoom);
                    
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                    {
                        if (_selectedRoom == null)
                        {
                            Cleanup();
                        }
                        else
                        {
                            _selectedRoom = null;
                        }
                        Event.current.Use();
                        return;
                    }
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        if (_selectedRoom != null)
                        {
                            Event.current.Use();
                            CreateRoomEdge(_selectedRoom, hoveringRoom);
                            _selectedRoom = hoveringRoom;
                        }
                        else
                        {
                            Event.current.Use();
                            _selectedRoom = hoveringRoom;
                        }
                    }
                }
                
            }
            
        }

        private static void DrawLineBetweenRooms(float handleSize, Vector3 wsMousePosition, Room r, Room hoveringRoom)
        {
            if (r == null)
            {
                Handles.DrawSolidDisc(wsMousePosition, Vector3.forward, handleSize);
                return;
            }
            
            Debug.Assert(r.BuildingTransform != null);
            var p0 = r.WorldCenter;
            
            if(hoveringRoom == null || hoveringRoom == r)
            {
                using (new HandlesScope(Color.grey))
                {
                    Handles.DrawSolidDisc(wsMousePosition, Vector3.forward, handleSize);
                    Handles.DrawLine(p0, wsMousePosition);
                    return;
                }
            }
            
            Debug.Assert(hoveringRoom.BuildingTransform != null);
            var p1 = hoveringRoom.WorldCenter;
            Handles.DrawLine(p0, p1);
        }

        private Room GetHoveringRoom(Vector2 lsPos)
        {
            Room hoveringRoom = null;
            foreach (var room in _target.AllRooms)
            {
                if (room.ContainsLocalPosition(lsPos))
                {
                    hoveringRoom = room;
                    break;
                }
            }

            return hoveringRoom;
        }

        private void DrawRemoveEdgeHandles(float handleSize)
        {
            var edges = _editableGraph.Graph.Edges;
            foreach (var edge in edges)
            {
                var srcPosWs = edge.Source.WorldCenter;
                var desPosWs = edge.Target.WorldCenter;
                var midPosWs = (srcPosWs + desPosWs) / 2;
                Handles.DrawLine(srcPosWs, desPosWs);
                if (Handles.Button(midPosWs, Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap))
                {
                    RemoveRoomEdge(edge.Source, edge.Target);
                    return;
                }
                
            }
        }

        private void CreateRoomEdge(Room selectedRoom, Room hoveringRoom)
        {
            if (!_editableGraph.Graph.ContainsEdge(selectedRoom, hoveringRoom))
            {
                _editableGraph.AddEdge(selectedRoom, hoveringRoom);
                _isGraphDirty = true;
            }
            if(!_editableGraph.Graph.ContainsEdge(hoveringRoom, selectedRoom))
            {
                _editableGraph.AddEdge(hoveringRoom, selectedRoom);
                _isGraphDirty = true;
            }
            _selectedRoom = hoveringRoom;
        }

        private void RemoveRoomEdge(Room selectedRoom, Room hoveringRoom)
        {
            if (_editableGraph.Graph.ContainsEdge(selectedRoom, hoveringRoom))
            {
                _editableGraph.RemoveEdge(selectedRoom, hoveringRoom);
                _isGraphDirty = true;
            }
            if(!_editableGraph.Graph.ContainsEdge(hoveringRoom, selectedRoom))
            {
                _editableGraph.RemoveEdge(hoveringRoom, selectedRoom);
                _isGraphDirty = true;
            }
        }

        List<(Vector3, Vector3)> GetGraphLines(Transform buildingTransform)
        {
            if (_graphLines != null && !_isGraphDirty)
            {
                return _graphLines;
            }
            List<(Vector3, Vector3)> lines = new List<(Vector3, Vector3)>();
            
            foreach (var edge in _editableGraph.Graph.Edges)
            {
                var srcPosWs = buildingTransform.TransformPoint(edge.Source.Bounds.center);
                var desPosWs = buildingTransform.TransformPoint(edge.Target.Bounds.center);
                lines.Add((srcPosWs, desPosWs));
            }
            _graphLines = lines;
            _isGraphDirty = false;
            return lines;
        }


        void DrawRoom(Room room)
        {
            var transform = _buildingTransform();
            if (transform == null) return;
            var centerWS = transform.TransformPoint(room.roomBounds.center);
            var handleSize = HandleUtility.GetHandleSize(centerWS) * 0.2f;
        }


        private bool IsWorldPointInsideRoom(Vector3 wsPoint, Room room)
        {
            return room.ContainsWorldPosition(wsPoint);
            var transform = _buildingTransform();
            if (transform == null)
            {
                Debug.LogError("Building transform is null");
                return false;
            }
            var localPoint = transform.InverseTransformPoint(wsPoint);
            localPoint.z = room.roomBounds.center.z;
            return room.roomBounds.Contains(localPoint);
        }
    }
```

## Reflection

initially [[Iteration 22 - The Rooms Graph]] was going to be the first iteration in the room tracker, because I have yet to actually do any writeups for editor code;  however I decided to treat the editor code here with a bit more care and put all the editor code relating to the rooms graph as a regular POCO that is called by the Rooms editor.  




### What was learned or accomplished?

my first attempt was mega attrocious, so I decided to serialize the graph by having each room store a list of neighbor rooms.  Then I created a POCO data structure that would create an actual graph given a `Rooms` object.  The idea here was to use this graph at runtime for various room algorithms (everything from AI traversal to player character room tracking for cinematics).

I then created a subclass of this class to be used by the editor for authoring the rooms graph.

```ad-note
collapse: open
This setup assumes that the graph is only editted in edit mode, at runtime it asssumes the room graph to have a fixed topology
```


### Where to go now?

