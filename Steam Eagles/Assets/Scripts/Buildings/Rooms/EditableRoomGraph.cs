using QuikGraph;

namespace Buildings.Rooms
{
    public class EditableRoomGraph : RoomGraph
    {
        public EditableRoomGraph(Rooms rooms) : base(rooms)
        {
        }
        
        
        public void AddEdge(Room room1, Room room2)
        {
            room1.AddConnectedRoom(room2);
            room2.AddConnectedRoom(room1);
            if (!Graph.ContainsVertex(room1)) Graph.AddVertex(room1);
            if (!Graph.ContainsVertex(room2)) Graph.AddVertex(room2);
            Graph.AddEdge(new Edge<Room>(room1, room2));
        }
        
        public void RemoveEdge(Room room1, Room room2)
        {
            room1.RemoveConnectedRoom(room2);
            room2.RemoveConnectedRoom(room1);
            if(Graph.TryGetEdge(room1, room2, out var e1))
            {
                Graph.RemoveEdge(e1);
            }

            if (Graph.TryGetEdge(room2, room1, out var e2))
            {
                Graph.RemoveEdge(e2);
            }
        }
    }
}