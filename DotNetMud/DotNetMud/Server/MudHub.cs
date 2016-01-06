using System;
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
        private static IGameSpecifics _gameSpecifics; 

        public MudHub()
        {
            Console.WriteLine("MudHub Ctor");
            if (_gameSpecifics == null)
            {
                _gameSpecifics = new SampleGameSpecifics();
            }
        }

        // For the most part, whenever this gets something to do, it could/should send it off 
        // to Driver.   exception:  when its bootstrapping Driver. 

        // There's some cases where driver needs to send stuff to a client.  That's done by 
        // a captured hubcontext, it does NOT call back into here.. yet. 

        public void userCommand(string cmd)
        {
            Driver.Instance.ReceiveUserCommand(Context.ConnectionId, cmd);
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            if (Driver.Instance.Context == null)
            {
                Driver.Instance.Context = GlobalHost.ConnectionManager.GetHubContext<MudHub>();
                Driver.Instance.GameSpecifics = _gameSpecifics; 
            }

            var interactive = _gameSpecifics.CreateNewPlayer();
            Console.WriteLine("incoming connection {0} assigned to {1}", Context.ConnectionId, interactive);
            Driver.Instance.RegisterInteractive(interactive,Context.ConnectionId);

            _gameSpecifics.WelcomeNewPlayer(interactive); 
            return base.OnConnected();
        }
        // TODO: move an interactive connection to a new mud object

    }
}