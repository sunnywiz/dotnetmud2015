
namespace DotNetMud.Core
{
    public interface IHubToDriver
    {
        void RegisterInteractive(IInteractive player, string connectionId);
        void ReceiveUserCommand(string connectionId, string cmd);
        void ReceiveDisconnection(string connectionId, bool stopCalled);
        void ReceiveNewPlayer(string connectionId);
    }
}