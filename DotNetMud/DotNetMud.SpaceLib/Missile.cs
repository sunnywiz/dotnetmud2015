using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Missile: StdObject, IObject2D, HighFrequencyUpdateTimer.IHighFrequencyUpdateTarget
    {
        private decimal IWillExpireAtInSeconds = 0m;

        public double X { get; set; }
        public double Y { get; set; }
        public double DX { get; set; }
        public double DY { get; set; }
        public double R { get; set; }
        public double DR { get; set; }
        public string Id
        {
            get { return ReadableId; }
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
                var now = GlobalTime.NowInMs;
                return IWillExpireAtInSeconds * 1000m - now; 
            }
            set
            {
                var now = GlobalTime.NowInMs;
                IWillExpireAtInSeconds = (now + value) / 1000m; 
                HighFrequencyUpdateTimer.Register(this);
            }
        }

        public void HiFrequencyUpdate(HighFrequencyUpdateTimer.HighFrequencyUpdateInfo info)
        {
            // TODO: this is where steering code would go if we have a target. 

            if (info.ThisNowInSeconds > IWillExpireAtInSeconds)
            {
                // I have expired!
                this.Destroy();
            }
        }
    }
}