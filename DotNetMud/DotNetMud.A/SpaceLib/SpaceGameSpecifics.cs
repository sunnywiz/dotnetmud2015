using System;
using DotNetMud.A.Server;

namespace DotNetMud.A.MudLib
{
    public class SpaceGameSpecifics : IGameSpecifics
    {
        public IInteractive CreateNewPlayer()
        {
            throw new NotImplementedException("return Driver<SpaceGameSpecifics>.Instance.CreateNewStdObject(builtin://DotNetMud.A.MudLib.User) as IInteractive; ");
        }

        public void WelcomeNewPlayer(IInteractive newPlayer)
        {
            return; 
        }

        public void PlayerGotDisconnected(IInteractive playerObject, bool wasItIntentional)
        {
            throw new NotImplementedException();
            var ob2 = (playerObject as StdObject);
            if (ob2 != null)
            {
                if (ob2.Parent != null) Driver<SpaceGameSpecifics>.Instance.TellRoom(ob2.Parent,$"{ob2.Short} vanishes in a puff of smoke.");
                Driver<SpaceGameSpecifics>.Instance.RemoveStdObjectFromGame(ob2);
            }
        }
    }
}
