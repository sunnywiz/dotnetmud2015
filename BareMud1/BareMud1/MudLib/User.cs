using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BareMud1
{
    public class User : StdObject, IInteractive
    {
        public User()
        {

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
            MudHub.Instance.SendTextToPlayerObject(this, text);
        }
    }
}