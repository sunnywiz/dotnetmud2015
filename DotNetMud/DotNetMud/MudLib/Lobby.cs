namespace DotNetMud.MudLib
{
    public class Lobby : Room
    {
        public Lobby()
        {
            Short = "The Lobby";
            Description = "Its a nice lobby. ";
            AddDirection("east","builtin://DotNetMud.MudLib.VisitingRoom1");
        }
    }

    public class VisitingRoom1 : Room
    {
        public VisitingRoom1()
        {
            Short = "Visiting Room 1";
            Description = "You are in a plush little visiting room with couches everywhere.";
            AddDirection("west", "builtin://DotNetMud.MudLib.Lobby");
        }
    }
}