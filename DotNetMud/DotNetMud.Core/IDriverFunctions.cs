using System;
using DotNetMud.Core;

namespace DotNetMud.Core
{
    public interface IDriverFunctions
    {
        void SendTextToPlayerObject(IInteractive player, string message);
        void RedirectNextUserInput(IInteractive player, Action<string> action);

        /// <summary>
        /// Attempts to find the singleton specified.  If not already created, creates it. 
        /// </summary>
        /// <param name="uri"></param>
        StdObject FindSingletonByUri(string uri);

        StdObject CreateNewStdObject(string uri);

        /// <summary>
        /// Do as much as I can to forget about an object. 
        /// </summary>
        /// <param name="ob"></param>
        void RemoveStdObjectFromGame(StdObject ob);

        /// <summary>
        /// Tell all the interactives (people who can receive messages) in a room something. 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="message"></param>
        void TellRoom(StdObject room, string message);

        IInteractive[] ListOfInteractives();
    }
}