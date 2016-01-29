using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DotNetMud.C.SpaceLib;
using DotNetMud.Driver;
using Microsoft.AspNet.SignalR;

namespace DotNetMud.Web.Hubs
{
    /// <summary>
    /// This is the raw implementation of "chatting with clients over Signal/R. 
    /// It tries to pawn whatever it can over to driver.cs
    /// </summary>
    public class SpaceHub : Hub
    {
        // For the most part, whenever this gets something to do, it could/should send it off 
        // to Driver.   exception:  when its bootstrapping Driver. 

        // There's some cases where driver needs to send stuff to a client.  That's done by 
        // a captured hubcontext, it does NOT call back into here.. yet. 

        public void userCommand(string cmd)
        {
            Driver<SpaceGameSpecifics>.Instance.ReceiveUserCommand(Context.ConnectionId, cmd);
        }

        public void requestPoll(string pollName, object clientState /* TODO: can't really use clientState for much yet need PersistentConnection */)
        {
            var pollResult = Driver<SpaceGameSpecifics>.Instance.RequestPoll(Context.ConnectionId,pollName, clientState);
            Clients.Caller.pollResult(pollName, pollResult);
        }

        public override Task OnConnected()
        {
            Trace.WriteLine("OnConnected");
            DriverShouldCaptureSignalRContext();
            Driver<SpaceGameSpecifics>.Instance.ReceiveNewPlayer(Context.ConnectionId);
            return base.OnConnected();
        }

        private static void DriverShouldCaptureSignalRContext()
        {
            if (Driver<SpaceGameSpecifics>.Instance.SendToClientCallBack == null)
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<MudHub>();
                Driver<SpaceGameSpecifics>.Instance.SendToClientCallBack = (connectionId, message) =>
                {
                    var client = context.Clients.Client(connectionId);
                    if (client != null)
                    {
                        client.sendToClient(message);
                    }
                };
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            DriverShouldCaptureSignalRContext();
            Driver<SpaceGameSpecifics>.Instance.ReceiveDisconnection(Context.ConnectionId, stopCalled);
            return base.OnDisconnected(stopCalled);
        }

        // TODO: move an interactive connection to a new mud object

    }
}