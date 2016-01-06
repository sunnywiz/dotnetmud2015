using System;
using System.Collections.Generic;
using System.Linq;
using BareMud1.MudLib;
using Microsoft.AspNet.SignalR;

namespace BareMud1
{
    /// <summary>
    /// This is the raw implementation of "chatting with clients over Signal/R. 
    /// It knows about various game details, and knows about connection Id's of players. 
    /// 
    /// This is also where old-style mud driver calls like "get me the list of players" gets implemented. 
    /// </summary>
    public class MudHub : Hub
    {
        private static MudHub _instance;
        private static IGameSpecifics _master;
        private static Dictionary<string, IInteractive> _connectionToPlayer;
        private static Dictionary<IInteractive, string> _playerToConnection;    

        public MudHub()
        {
            Console.WriteLine("MudHub Ctor");
            if (_instance == null)
            {
                _instance = this;
                // I'm not sure this is right.    
                _master = new SampleMaster();
                _connectionToPlayer = new Dictionary<string, IInteractive>();
                _playerToConnection = new Dictionary<IInteractive, string>();
            }
        }

        public static MudHub Instance
        {
            get { return _instance; }
        }

        public void userCommand(string cmd)
        {
            IInteractive player ;
            if (_connectionToPlayer.TryGetValue(Context.ConnectionId, out player))
            {
                player.ReceiveInput(cmd);
            }
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            if (_instance == null) _instance = this;

            var interactive = _master.CreateNewPlayer();
            Console.WriteLine("incoming connection {0} assigned to {1}", Context.ConnectionId,interactive);
            _connectionToPlayer[Context.ConnectionId] = interactive;
            _playerToConnection[interactive] = Context.ConnectionId; 

            return base.OnConnected();

        }

        // TODO: move an interactive connection to a new mud object

        public void SendTextToPlayerObject(IInteractive player, string message)
        {
            string connectionId;
            if (_playerToConnection.TryGetValue(player, out connectionId))
            {
                var client = Clients.Client(connectionId);
                if (client != null)
                {
                    client.sendToClient(message);
                }
            }
        }
    }
}