using System;
using System.Collections.Generic;
using System.Linq;
using BareMud1.MudLib;
using Microsoft.AspNet.SignalR;

namespace BareMud1
{
    public class MudHub : Hub
    {
        private static MudHub _instance;
        private static IGameSpecifics _master;
        private static Dictionary<string, IInteractive> _players;  

        public MudHub()
        {
            if (_instance == null)
            {
                _instance = this;
                _master = new SampleMaster();
                _players = new Dictionary<string, IInteractive>();
            }
        }

        public static MudHub Instance
        {
            get { return _instance; }
        }

        public void userCommand(string cmd)
        {
            //var playerInfo = _playerInfo.FirstOrDefault(x => x.ContextId == Context.ConnectionId);

            //if (playerInfo != null && playerInfo.PlayerObject != null)
            //{
            //    playerInfo.PlayerObject.ReceiveInput(cmd);
            //}
        }


        public override System.Threading.Tasks.Task OnConnected()
        {
            if (_instance == null) _instance = this;
            Console.WriteLine("incoming connection {0}", Context.ConnectionId);

            var interactive = _master.CreateNewPlayer();
            _players[Context.ConnectionId] = interactive;

            return base.OnConnected();
        }

        // client:  .sendToClient(message)
    }
}