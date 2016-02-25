using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Missile: StdObject2D, HighFrequencyUpdateTimer.IHighFrequencyUpdateTarget
    {
        private decimal IWillExpireAtInSeconds = 0m;

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