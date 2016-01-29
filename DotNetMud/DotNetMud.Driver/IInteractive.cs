namespace DotNetMud.Driver
{
    public interface IInteractive
    {
        void ReceiveInput(string line);
        void SendOutput(string text);
        object RequestPoll(string pollName, object clientState);
    }
}