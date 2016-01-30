using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            if (User.ServerSendsTextTOClientCallback == null)
            {
                // this way .Mudlib does NOT know about the web host, but it works anyway. 
                User.ServerSendsTextTOClientCallback = (u, message) =>
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

        //public void ClientRequestPollFromServer(string pollName, object clientState /* TODO: can't really use clientState for much yet need PersistentConnection */)
        //{
        //    var pollResult = Driver<SampleGameSpecifics>.Instance.RequestPoll(Context.ConnectionId,pollName, clientState);
        //    Clients.Caller.pollResult(pollName, pollResult);
        //}

        //private static void DriverShouldCaptureSignalRContext()
        //{
        //    if (Driver<SampleGameSpecifics>.Instance.SendToClientCallBack == null)
        //    {
        //        var context = GlobalHost.ConnectionManager.GetHubContext<MudHub>();
        //        Driver<SampleGameSpecifics>.Instance.SendToClientCallBack = (connectionId, message) =>
        //        {
                    
        //        };
        //    }
        //}

        //public override Task OnDisconnected(bool stopCalled)
        //{
        //    DriverShouldCaptureSignalRContext();
        //    Driver<SampleGameSpecifics>.Instance.ReceiveDisconnection(Context.ConnectionId, stopCalled);
        //    return base.OnDisconnected(stopCalled);
        //}

        // TODO: move an interactive connection to a new mud object
    }
}