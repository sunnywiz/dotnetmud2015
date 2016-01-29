using DotNetMud.Driver;

namespace DotNetMud.B.MudLib
{
    public class SampleGameSpecifics : IGameSpecifics
    {
        public IInteractive CreateNewPlayer()
        {
            return Driver<SampleGameSpecifics>.Instance.CreateNewStdObject("assembly://DotNetMud.B/DotNetMud.B.MudLib.User") as IInteractive; 
        }

        public void WelcomeNewPlayer(IInteractive newPlayer)
        {
            newPlayer.SendOutput("Welcome to DotNetMud2015. ");
            newPlayer.SendOutput("");
            newPlayer.SendOutput("This is a sample mud library, more for setting up a driver and library");
            newPlayer.SendOutput("than for actual gaming.   ");
            newPlayer.SendOutput("");
            newPlayer.SendOutput("What name shall i know you by? ");

            Driver<SampleGameSpecifics>.Instance.RedirectNextUserInput(newPlayer, (string text) =>
            {
                var np2 = newPlayer as User;
                np2.Short = text;
                np2.SendOutput("Welcome, "+text);
                var room = Driver<SampleGameSpecifics>.Instance.FindSingletonByUri("assembly://DotNetMud.B/DotNetMud.B.MudLib.Lobby") as MudLibObject;
                if (room != null)
                {
                    MudLibObject.TellRoom(room, $"{np2.Short} arrives in a puff of smoke!");
                    np2.MoveTo(room);
                }

                np2.ReceiveInput("look");
            });
        }

        public void PlayerGotDisconnected(IInteractive playerObject, bool wasItIntentional)
        {
            var ob2 = (playerObject as User);
            if (ob2 != null)
            {
                if (ob2.Parent != null) MudLibObject.TellRoom(ob2.Parent,$"{ob2.Short} vanishes in a puff of smoke.");
                Driver<SampleGameSpecifics>.Instance.RemoveStdObjectFromGame(ob2);
            }
        }
    }
}
