using System;
using System.IO;
using System.Text;

namespace BareMud1
{
    public class User : StdObject, IInteractive
    {
        private readonly MudHub _mudHub;
        private readonly string _connectionId = null; 

        public User(MudHub mudHub, string connectionId, string nick)
        {
            _mudHub = mudHub;
            _connectionId = connectionId; 
            Short = nick;
            Long = "The amazing " + nick; 
        }

        public void ReceiveInput(string line)
        {
            if (line == "look")
            {
                DoLook();
            }
            else
            {
                SendOutput("unknown command");
            }
        }

        private void DoLook()
        {
            var parent = this.Parent;
            if (parent == null)
            {
                SendOutput("You are hanging in the void.");
                return; 
            }
            SendOutput(parent.Short);
            SendOutput(parent.Long);
            SendOutput("Here you see: ");
            foreach (var obj in parent.GetInventory())
            {
                if (obj == this) continue; 
                SendOutput(String.Format("  {0}", obj.Short));
            }
        }

        public void SendOutput(string text)
        {
            _mudHub.Clients.Client(_connectionId).sendToClient(text); 
        }
    }
}