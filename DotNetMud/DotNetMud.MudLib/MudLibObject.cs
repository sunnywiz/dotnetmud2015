using System;
using System.Collections.Generic;
using System.Linq;
using DotNetMud.Driver;

namespace DotNetMud.Mudlib
{
    public class MudLibObject : StdObject
    {
        // Short and Long are Mudlib specific things. 
        public string Short { get; set; }
        public virtual string Long => string.Empty;

        /// <summary>
        /// Tell all the interactives (people who can receive messages) in a room something. 
        /// </summary>
        /// <param name="room"></param>
        /// <param name="message"></param>
        public static void TellRoom(MudLibObject room, string message)
        {
            if (room == null || String.IsNullOrEmpty(message)) return;
            foreach (var user in room.GetInventory<User>())
            {
                user.ServerSendsTextToClient(message);
            }
        }

    }
}
