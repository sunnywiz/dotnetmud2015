using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;

namespace DotNetMud.Server
{
    /// <summary>
    /// This is what most mud code would know as the driver - the O/S of the mud, as it were. 
    /// It tries to offload what it can from MudHub
    /// </summary>
    public class Driver
    {
        private static Driver _instance;

        private readonly Dictionary<string, IInteractive> _connectionToPlayer;
        private readonly Dictionary<IInteractive, string> _playerToConnection;
        private readonly Dictionary<string, Action<string>> _registeredNextInputRedirects;

        public Driver()
        {
            _connectionToPlayer = new Dictionary<string, IInteractive>();
            _playerToConnection = new Dictionary<IInteractive, string>();
            _registeredNextInputRedirects = new Dictionary<string, Action<string>>();
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
                Action<string> action;
                if (_registeredNextInputRedirects.TryGetValue(connectionId, out action) && action != null)
                {
                    _registeredNextInputRedirects.Remove(connectionId);
                    action(cmd);
                    return;
                }
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

        public void RedirectNextUserInput(IInteractive player, Action<string> action)
        {
            string connectionId;
            if (_playerToConnection.TryGetValue(player, out connectionId))
            {
                _registeredNextInputRedirects[connectionId] = action;
            }
        }
    }
}