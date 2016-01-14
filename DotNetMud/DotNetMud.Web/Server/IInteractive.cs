namespace DotNetMud.Server
{
    public interface IInteractive
    {
        void ReceiveInput(string line);
        void SendOutput(string text);
    }
}