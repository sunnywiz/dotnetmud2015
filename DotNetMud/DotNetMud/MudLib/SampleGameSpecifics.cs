using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // TODO: implement Driver.InputTo(interactive, function() ) to be able to capture user's input       

            Driver.Instance.RedirectNextUserInput(newPlayer, (string text) =>
            {
                var np2 = newPlayer as User;
                np2.Short = text;
                np2.Long = "The Amazing " + text;
                np2.SendOutput("Welcome, "+text);
                np2.MoveTo(Driver.Instance.FindSingletonByUri("builtin://DotNetMud.MudLib.Lobby"));
                np2.ReceiveInput("look");
            });
        }
    }
}
