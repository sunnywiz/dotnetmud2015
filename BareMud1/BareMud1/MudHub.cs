using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;

namespace BareMud1
{
    public class MudHub : Hub
    {
        public static Dictionary<string, IInteractive> UserDictionary;

        private static MudHub _instance; 

        static MudHub()
        {
            UserDictionary = new Dictionary<string, IInteractive>();
        }

        public static MudHub Instance {  get { return _instance; } }

        public void nickname(string nick)
        {
            string connectionId = this.Context.ConnectionId;
            var user = new User(connectionId, nick);
            UserDictionary[connectionId] = user;
            user.MoveTo(Program.StartRoom);
            user.ReceiveInput("look");
        }

        public void userCommand(string cmd)
        {
            IInteractive interactive;
            if (UserDictionary.TryGetValue(Context.ConnectionId, out interactive))
            {
                interactive.ReceiveInput(cmd);
            }
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            if (_instance == null) _instance = this; 
            Console.WriteLine("incoming connection {0}", Context.ConnectionId);
            this.Clients.Client(Context.ConnectionId).sendToClient("Greetings Human");
            return base.OnConnected();
        }
        // client:  .sendToClient(message)
    }
}