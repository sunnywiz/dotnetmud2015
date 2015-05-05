using System;
using System.IO;
using System.Text;

namespace BareMud1
{
    public class User : StdObject, IInteractive
    {
        private TextWriter _outputStream;

        public User()
        {
            _outputStream = new StringWriter(new StringBuilder());
            Short = "generic user";
            Long = "This is a generic user";
        }

        public void RegisterOutputStream(TextWriter @out)
        {
            _outputStream = @out; 
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
            _outputStream.WriteLineAsync(text);
        }
    }
}