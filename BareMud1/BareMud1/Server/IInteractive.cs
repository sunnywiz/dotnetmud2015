using System.Collections.Generic;
using System.IO;

namespace BareMud1
{
    public interface IInteractive
    {
        void ReceiveInput(string line);
        void SendOutput(string text);
    }

    public interface IGameSpecifics
    {
        /// <summary>
        /// This is supposed to return whatever object your player is going to be.  It needs to implement
        /// IInteractive.    Don't do anything fancy with it yet, just create it, i have some housekeeping
        /// to do..  
        /// </summary>
        /// <returns></returns>
        IInteractive CreateNewPlayer();

        /// <summary>
        /// On housekeeping completed, this is where you set up the player to feel more at home. 
        /// Most likely, its a welcome screen of sorts.   use player.SendOutput() to send stuff to them. 
        /// </summary>
        /// <param name="newPlayer"></param>
        void WelcomeNewPlayer(IInteractive newPlayer);
    }
}