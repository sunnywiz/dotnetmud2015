using System.Collections.Generic;
using System.IO;

namespace BareMud1
{
    public interface IInteractive
    {
        void ReceiveInput(string line);
        void SendOutput(string text);
        List<PollResult> DoPoll(); 

    }

    public class PollResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }

    }
}