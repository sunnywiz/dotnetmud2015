using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareMud1.MudLib
{
    public class SampleGameSpecifics : IGameSpecifics
    {
        public IInteractive CreateNewPlayer()
        {
            return new User(); 
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
        }
    }
}
