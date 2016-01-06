using System;
using DotNetMud.Server;

namespace DotNetMud.MudLib
{
    public class User : StdObject, IInteractive
    {
        public void ReceiveInput(string line)
        {
            // TODO: expand receiveInput to do things based on environment or this object or whatever. 
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
            Driver.Instance.SendTextToPlayerObject(this, text);
        }
    }
}