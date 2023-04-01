%%
see: [[Iteration 21 - Authoring the Rooms Graph]], [[EditableRoomGraph]]
%%

```cs
 public class RoomGraph
    {
        AdjacencyGraph<Room, Edge<Room>> graph = new AdjacencyGraph<Room, Edge<Room>>();

        public AdjacencyGraph<Room, Edge<Room>> Graph => graph;
        public RoomGraph(Rooms rooms)
        {
            foreach (var room in rooms.AllRooms)
            {
                graph.AddVertex(room);
                foreach (var neighbor in room.connectedRooms)
                {
                    graph.AddVerticesAndEdge(new Edge<Room>(room, neighbor));
                }
            }
        }
        
```
