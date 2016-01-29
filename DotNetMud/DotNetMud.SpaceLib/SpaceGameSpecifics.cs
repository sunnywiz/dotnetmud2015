using System;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class SpaceGameSpecifics : IGameSpecifics
    {
        private static int playerNumber = 0; 
        public IInteractive CreateNewPlayer()
        {
            var player =
                Driver<SpaceGameSpecifics>.Instance.CreateNewStdObject(
                    "assembly://DotNetMud.C//DotNetMud.C.SpaceLib.Ship");
            var p2 = player as IInteractive;
            var p3 = player as IObject2D;

            if (p3 != null)
            {
                var r = new Random();
                p3.X = r.NextDouble()*300.0 - 100.0;
                p3.Y = r.NextDouble()*300.0 - 100.0;
                p3.R = r.NextDouble()*Math.PI*2; 
                p3.DX = r.NextDouble() * 10.0 - 5.0;
                p3.DY = r.NextDouble() * 10.0 - 5.0;
                p3.DR = r.NextDouble() * 10.0 - 5.0;
                p3.Name = "P" + (++playerNumber);
                p3.Image = "Ship1";
                // TODO: will probably need a sizing thing for the image as well. 
            }
            return p2; 
        }

        public void WelcomeNewPlayer(IInteractive newPlayer)
        {
            var x = newPlayer as IObject2D;
            if (x == null) return; 
            var space =
                Driver<SpaceGameSpecifics>.Instance.FindSingletonByUri(
                    "assembly://DotNetMud.C/DotNetMud.C.SpaceLib.Space2D") as Space2D;
            if (space == null) return; 
            space.Objects.Add(x);
            x.Container = space; 
        }

        public void PlayerGotDisconnected(IInteractive playerObject, bool wasItIntentional)
        {
            var x = playerObject as IObject2D;
            if (x == null) return;
            var space =
                Driver<SpaceGameSpecifics>.Instance.FindSingletonByUri(
                    "assembly://DotNetMud.C/DotNetMud.C.SpaceLib.Space2D") as Space2D;
            if (space == null) return;
            space.Objects.Remove(x);
            x.Container = null; 
            Driver<SpaceGameSpecifics>.Instance.RemoveStdObjectFromGame(playerObject as StdObject);
        }
    }
}
