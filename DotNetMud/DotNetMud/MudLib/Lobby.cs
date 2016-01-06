namespace DotNetMud.MudLib
{
    public class Lobby : Room
    {
        private static Lobby _instance;

        // TODO: do something about deriving stuff by name. 
        public static Lobby Instance
        {
            get
            {
                if (_instance == null) _instance = new Lobby();
                return _instance; 
            }
        }

        public Lobby()
        {
            Short = "The Lobby";
            Long = "Its a nice lobby. ";
        }
    }
}