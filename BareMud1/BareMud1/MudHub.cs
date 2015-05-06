using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;

namespace BareMud1
{
    public class MudHub : Hub
    {
        public Dictionary<string, IInteractive> UserDictionary;

        public MudHub()
        {
            UserDictionary = new Dictionary<string, IInteractive>();
        }

        public void nickname(string nick)
        {
            string connectionId = this.Context.ConnectionId;
            var user = new User(this, connectionId, nick);
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
            Console.WriteLine("incoming connection {0}", Context.ConnectionId);
            this.Clients.Client(Context.ConnectionId).sendToClient("Greetings Human");
            return base.OnConnected();
        }
        // client:  .sendToClient(message)
    }
}