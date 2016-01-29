namespace DotNetMud.Mudlib
{
    public class Lobby : Room
    {
        public Lobby()
        {
            Short = "The Lobby";
            Description = "Its a nice lobby. ";
            AddDirection("east", typeof(VisitingRoom1));
        }
    }

    public class VisitingRoom1 : Room
    {
        public VisitingRoom1()
        {
            Short = "Visiting Room 1";
            Description = "You are in a plush little visiting room with couches everywhere.";
            AddDirection("west", typeof(Lobby));
        }
    }
}