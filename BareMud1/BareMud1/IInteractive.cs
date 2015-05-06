using System.IO;

namespace BareMud1
{
    public interface IInteractive
    {
        void ReceiveInput(string line);
        void SendOutput(string text); 
    }
}