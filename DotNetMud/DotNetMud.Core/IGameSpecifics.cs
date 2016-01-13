using DotNetMud.Core;

namespace DotNetMud.Core
{
    /// <summary>
    /// This is what the game implementation implements.  These are the hooks into custom logic that the driver
    /// needs.  Things like, what's the player object, and what to do when a user first connects, etc. 
    /// 
    /// In days of old, this was implemented by "master.cs". 
    /// </summary>
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

        /// <summary>
        /// Let the game know that somebody got disconnected. 
        /// </summary>
        /// <param name="wasItIntentional"></param>
        void PlayerGotDisconnected(IInteractive playerObject, bool wasItIntentional); 
    }
}