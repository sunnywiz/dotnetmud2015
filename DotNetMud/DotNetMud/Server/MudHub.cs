using System;
using System.Threading.Tasks;
using DotNetMud.MudLib;
using Microsoft.AspNet.SignalR;

namespace DotNetMud.Server
{
    /// <summary>
    /// This is the raw implementation of "chatting with clients over Signal/R. 
    /// It tries to pawn whatever it can over to driver.cs
    /// </summary>
    public class MudHub : Hub
    {
        public MudHub()
        {
            Console.WriteLine("MudHub Ctor");
        }

        // For the most part, whenever this gets something to do, it could/should send it off 
        // to Driver.   exception:  when its bootstrapping Driver. 

        // There's some cases where driver needs to send stuff to a client.  That's done by 
        // a captured hubcontext, it does NOT call back into here.. yet. 

        public void userCommand(string cmd)
        {
            Driver.Instance.ReceiveUserCommand(Context.ConnectionId, cmd);
        }

        public override Task OnConnected()
        {
            DriverShouldCaptureSignalRContext();
            Driver.Instance.ReceiveNewPlayer(Context.ConnectionId);
            return base.OnConnected();
        }

        private static void DriverShouldCaptureSignalRContext()
        {
            if (Driver.Instance.Context == null)
            {
                Driver.Instance.Context = GlobalHost.ConnectionManager.GetHubContext<MudHub>();
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            DriverShouldCaptureSignalRContext();
            Driver.Instance.ReceiveDisconnection(Context.ConnectionId, stopCalled);
            return base.OnDisconnected(stopCalled);
        }

        // TODO: move an interactive connection to a new mud object

    }
}