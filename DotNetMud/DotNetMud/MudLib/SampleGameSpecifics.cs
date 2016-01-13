using System;
using DotNetMud.Core;
using DotNetMud.Server;

namespace DotNetMud.MudLib
{
    public class SampleGameSpecifics : IGameSpecifics
    {
        public IInteractive CreateNewPlayer()
        {
            return Driver.Instance.CreateNewStdObject("builtin://DotNetMud.MudLib.User") as IInteractive; 
        }

        public void WelcomeNewPlayer(IInteractive newPlayer)
        {
            newPlayer.SendOutput("Welcome to DotNetMud2015. ");
            newPlayer.SendOutput("");
            newPlayer.SendOutput("This is a sample mud library, more for setting up a driver and library");
            newPlayer.SendOutput("than for actual gaming.   ");
            newPlayer.SendOutput("");
            newPlayer.SendOutput("What name shall i know you by? ");

            Driver.Instance.RedirectNextUserInput(newPlayer, (string text) =>
            {
                var np2 = newPlayer as User;
                np2.Short = text;
                np2.SendOutput("Welcome, "+text);
                var room = Driver.Instance.FindSingletonByUri("builtin://DotNetMud.MudLib.Lobby");
                if (room != null)
                {
                    Driver.Instance.TellRoom(room, $"{np2.Short} arrives in a puff of smoke!");
                    np2.MoveTo(room);
                }

                np2.ReceiveInput("look");
            });
        }

        public void PlayerGotDisconnected(IInteractive playerObject, bool wasItIntentional)
        {
            var ob2 = (playerObject as StdObject);
            if (ob2 != null)
            {
                if (ob2.Parent != null) Driver.Instance.TellRoom(ob2.Parent,$"{ob2.Short} vanishes in a puff of smoke.");
                Driver.Instance.RemoveStdObjectFromGame(ob2);
            }
        }
    }
}
