using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotNetMud.Driver;
using DotNetMud.Mudlib;
using Microsoft.AspNet.SignalR;

namespace DotNetMud.Web.Hubs
{
    /// <summary>
    /// This is the raw implementation of "chatting with clients over Signal/R. 
    /// It is very tightly coupled to the in-game object that does the real stuff. 
    /// The trick is to make the in game object not aware of this object. 
    /// </summary>
    public class MudHub : Hub
    {

        private static IHubContext _context;
        private static Dictionary<string, User> _connectionToPlayer;
        private static Dictionary<User, string> _playerToConnection;

        public MudHub()
        {
            if (_context == null) _context = GlobalHost.ConnectionManager.GetHubContext<MudHub>();
            if (_connectionToPlayer == null) _connectionToPlayer = new Dictionary<string, User>();
            if (_playerToConnection == null) _playerToConnection = new Dictionary<User, string>();

            if (User.ServerSendsTextToClientCallback == null)
            {
                // this way .Mudlib does NOT know about the web host, but it works anyway. 
                User.ServerSendsTextToClientCallback = (u, message) =>
                {
                    string connectionId;
                    if (_playerToConnection.TryGetValue(u, out connectionId))
                    {
                        var client = _context.Clients.Client(connectionId);
                        if (client != null)
                        {
                            client.ServerSendsTextToClient(message);
                        }
                    }
                };
            }

            if (User.ListOfUsersCallback == null)
            {
                User.ListOfUsersCallback = () =>
                {
                    var users = _connectionToPlayer.Values.Where(u => !u.IsDestroyed).ToArray();
                    return users;
                };
            }
        }

        public override Task OnConnected()
        {
            var interactive = new User();
            Console.WriteLine("incoming connection {0} assigned to {1}", Context.ConnectionId, interactive);
            _connectionToPlayer[Context.ConnectionId] = interactive;
            _playerToConnection[interactive] = Context.ConnectionId;
            interactive.WelcomeNewPlayer();
            return base.OnConnected();
        }


        public void ClientSendsUserCommandToServer(string cmd)
        {
            Console.WriteLine("ReceivedUserCommand: {0} sent {1}", Context.ConnectionId, cmd);
            User player;
            if (_connectionToPlayer.TryGetValue(Context.ConnectionId, out player))
            {
                player.ClientSendsUserCommandToServer(cmd);
            }
        }

        public void ClientRequestsPollFromServer()
        {
            User player; 
            if (_connectionToPlayer.TryGetValue(Context.ConnectionId, out player))
            {
                var pollResult = player.ClientRequestsPollFromServer();
                Clients.Caller.ServerSendsPollResultToClient(pollResult);

            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            User player;
            if (_connectionToPlayer.TryGetValue(Context.ConnectionId, out player))
            {
                player.PlayerGotDisconnected(stopCalled);
                _connectionToPlayer.Remove(Context.ConnectionId);
                _playerToConnection.Remove(player);
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}