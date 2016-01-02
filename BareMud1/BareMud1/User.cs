using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BareMud1
{
    public class User : StdObject, IInteractive
    {
        private readonly string _connectionId = null; 

        public User(string connectionId, string nick)
        {
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

        public List<PollResult> DoPoll()
        {
            var result = new List<PollResult>();
            if (this.Parent != null)
            {
                result.Add(new PollResult() {Id = "ROOM", Name = "Id", Value = this.Parent.Id});
                var inventory = this.Parent.GetInventory();
                result.Add(new PollResult()
                {
                    Id = this.Parent.Id,
                    Name = "INVENTORY",
                    Value = inventory.Select(x => x.Id).ToArray()
                });
                foreach (var ob in inventory)
                {
                    result.Add(new PollResult()
                    {
                        Id = ob.Id, 
                        Name="Short", 
                        Value = ob.Short
                    });
                    result.Add(new PollResult()
                    {
                        Id = ob.Id,
                        Name = "Long",
                        Value = ob.Short
                    });
                }
            }
            return result; 
        }

        public void SendOutput(string text)
        {
            var clients = MudHub.Instance.Clients;
            var myClient = clients.Client(_connectionId);
            if (myClient == null) return; 
            myClient.sendToClient(text); 
        }
    }
}