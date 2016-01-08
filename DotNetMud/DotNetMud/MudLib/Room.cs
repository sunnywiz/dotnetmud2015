using DotNetMud.Server;

namespace DotNetMud.MudLib
{
    /// <summary>
    /// Standard room object - rooms should inherit from here. 
    /// TODO: add directions into/out of room objects
    /// </summary>
    public class Room : StdObject
    {
        public Room()
        {
            Short = "A room.";
            Long = "You are standing in a generic room."; 
        }
    }
}