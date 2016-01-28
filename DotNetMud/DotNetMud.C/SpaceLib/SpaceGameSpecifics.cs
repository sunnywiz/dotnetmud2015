using System;
using DotNetMud.A.Server;

namespace DotNetMud.C.SpaceLib
{
    public class SpaceGameSpecifics : IGameSpecifics
    {
        public IInteractive CreateNewPlayer()
        {
            return Driver<SpaceGameSpecifics>.Instance.CreateNewStdObject("assembly://DotNetMud.C//DotNetMud.C.SpaceLib.Ship") as IInteractive;
        }

        public void WelcomeNewPlayer(IInteractive newPlayer)
        {
            return; 
        }

        public void PlayerGotDisconnected(IInteractive playerObject, bool wasItIntentional)
        {
        }
    }
}
