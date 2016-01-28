using DotNetMud.A.Server;

namespace DotNetMud.C.SpaceLib
{
    public class Ship : StdObject, IInteractive
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
    }
}