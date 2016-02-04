using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DotNetMud.SpaceLib;
using Microsoft.AspNet.SignalR;

namespace DotNetMud.Web.Hubs
{
    /// <summary>
    /// This is the raw implementation of "chatting with clients over Signal/R. 
    /// It tries to pawn whatever it can over to driver.cs
    /// </summary>
    public class SpaceHub : Hub
    {
        // For the most part, whenever this gets something to do, it could/should send it off 
        // to Driver.   exception:  when its bootstrapping Driver. 

        // There's some cases where driver needs to send stuff to a client.  That's done by 
        // a captured hubcontext, it does NOT call back into here.. yet. 
        private static IHubContext _context;
        private static Dictionary<string, Ship> _connectionToPlayer;
        private static Dictionary<Ship, string> _playerToConnection;

        public SpaceHub()
        {
            if (_context == null) _context = GlobalHost.ConnectionManager.GetHubContext<SpaceHub>();
            if (_connectionToPlayer == null) _connectionToPlayer = new Dictionary<string, Ship>();
            if (_playerToConnection == null) _playerToConnection = new Dictionary<Ship, string>();
        }


        public void ClientRequestsPollFromServer()
        {
            Ship player;
            if (_connectionToPlayer.TryGetValue(Context.ConnectionId, out player))
            {
                var pollResult = player.ClientRequestsPollFromServer();
                Clients.Caller.ServerSendsPollResultToClient(pollResult);

            }
        }


        public override Task OnConnected()
        {
            var interactive = new Ship();
            Console.WriteLine("incoming connection {0} assigned to {1}", Context.ConnectionId, interactive);
            _connectionToPlayer[Context.ConnectionId] = interactive;
            _playerToConnection[interactive] = Context.ConnectionId;
            interactive.WelcomeNewPlayer();
            return base.OnConnected(); 
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

    }
}