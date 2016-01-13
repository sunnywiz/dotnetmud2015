namespace DotNetMud.Core
{
    public interface IInteractive
    {
        void ReceiveInput(string line);
        void SendOutput(string text);
    }
}