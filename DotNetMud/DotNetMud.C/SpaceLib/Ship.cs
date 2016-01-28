using DotNetMud.A.Server;

namespace DotNetMud.C.SpaceLib
{
    public class Ship : StdObject, IObject2D, IInteractive
    {
        public void ReceiveInput(string line)
        {
            // nothing to do yet. 
        }

        public void SendOutput(string text)
        {
            // nothing yet. 
            // Driver<SpaceGameSpecifics>.Instance.SendTextToPlayerObject(this, text);
        }

        public object RequestPoll(string pollName, object clientState)
        {
            return new PollResult1();
        }

        public Space2D Container { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double DX { get; set; }
        public double DY { get; set; }
        public double R { get; set; }
        public double DR { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }
}