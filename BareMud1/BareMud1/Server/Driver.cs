using System.Collections.Generic;
using Microsoft.AspNet.SignalR;

namespace BareMud1
{
    public class Driver
    {
        private static Driver _instance;

        private readonly Dictionary<string, IInteractive> _connectionToPlayer;
        private readonly Dictionary<IInteractive, string> _playerToConnection;

        public Driver()
        {
            _connectionToPlayer = new Dictionary<string, IInteractive>();
            _playerToConnection = new Dictionary<IInteractive, string>();
        }

        public static Driver Instance
        {
            get
            {
                if (_instance == null) _instance = new Driver();
                return _instance; 
            }
        }

        #region INTERNAL things are called from MudHub  -- not accessible to master / rest of the game

        internal IHubContext Context;
        internal IGameSpecifics GameSpecifics { get; set; }

        internal void RegisterInteractive(IInteractive player, string connectionId)
        {
            _connectionToPlayer[connectionId] = player;
            _playerToConnection[player] = connectionId;
        }

        internal void ReceiveUserCommand(string connectionId, string cmd)
        {
            IInteractive player;
            if (_connectionToPlayer.TryGetValue(connectionId, out player))
            {
                player.ReceiveInput(cmd);
            }
        }

        #endregion

        public void SendTextToPlayerObject(IInteractive player, string message)
        {
            string connectionId;
            if (_playerToConnection.TryGetValue(player, out connectionId))
            {
                var client = Context.Clients.Client(connectionId);
                if (client != null)
                {
                    client.sendToClient(message);
                }
            }
        }

    }
}