using System.IO;

namespace BareMud1
{
    internal interface IInteractive
    {
        void RegisterOutputStream(TextWriter @out);
        void ReceiveInput(string line);
        void SendOutput(string text); 
    }
}