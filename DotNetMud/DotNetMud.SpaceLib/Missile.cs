using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Missile: StdObject, IObject2D, GlobalTimers.IHighFrequencyUpdateTarget
    {
        private decimal IWillExpireAtInSeconds = 0m;
        public Missile()
        {
            _registered = false; 
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double DX { get; set; }
        public double DY { get; set; }
        public double R { get; set; }
        public double DR { get; set; }
        public string Id
        {
            get { return ObjectId; }
            set
            {
                //ignore
            }
        }
        public string Name { get; set; }
        public string Image { get; set; }

        public Space2D Container { get; set; }

        public decimal DurationRemainingInGameMs
        {
            get
            {
                var now = GlobalTimers.NowInMs;
                return IWillExpireAtInSeconds * 1000m - now; 
            }
            set
            {
                var now = GlobalTimers.NowInMs;
                IWillExpireAtInSeconds = (now + value) / 1000m; 
                GlobalTimers.RegisterForHighFrequencyUpdate(this);
            }
        }

        public void HiFrequencyUpdate(GlobalTimers.HighFrequencyUpdateInfo info)
        {
            // TODO: this is where steering code would go if we have a target. 

            if (info.ThisNowInSeconds > IWillExpireAtInSeconds)
            {
                // I have expired!
                this.Container.Objects.Remove(this); 
                Driver.GlobalObjects.RemoveStdObjectFromGame(this);
                // TODO: Should deregister for hi-frequency update on destruct.  Make that part of GlobalObjects.RemoveStdObjectFromGame which means move globaltimers up to Driver.
            }
        }
    }
}