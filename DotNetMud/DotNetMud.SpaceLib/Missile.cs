using System;
using System.Timers;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Missile: StdObject2D, IWantToHitThings
    {
        private decimal IWillExpireAtInSeconds = 0m;
        private System.Timers.Timer _expirationTimer = null; 


        // TODO: probably need to make a Game-time timer so that in x Game-TIme-ms something fires. 
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
                IWillExpireAtInSeconds = (now + value)/1000m;
                if (_expirationTimer != null)
                {
                    _expirationTimer.Stop();
                    _expirationTimer = null;
                }
                _expirationTimer = new Timer()
                {
                    Interval = Convert.ToDouble(value),
                    AutoReset = false,
                };
                _expirationTimer.Elapsed += ExpirationTimerOnElapsed;
                _expirationTimer.Start();
            }
        }

        private void ExpirationTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // there's a possibility if game time stops, that time will not have yet elapsed. If so, go ahead and overshoot. 
            var now = GlobalTime.NowInMs;
            var timeToGo = IWillExpireAtInSeconds*1000 - now;
            if (timeToGo > 0)
            {
                _expirationTimer.Interval = Convert.ToDouble(timeToGo)*1.5;
                _expirationTimer.Start();
                return; 
            }
            // otherwise, time has expired. 
            this.Destroy(); 
        }

        public void IHaveHit(ICanBeHitByThings target)
        {
            if (this.Parent is Space2D)
            {
                var explosion = new Explosion() {X = this.X, Y = this.Y, DX = target.DX, DY = target.DY};
                explosion.MoveTo(this.Parent);
                explosion.StartExpirationTimer();
            }
            this.Destroy();
        }
        
    }

    public class Explosion : StdObject2D
    {
        public Explosion()
        {
            Image = "http://i87.servimg.com/u/f87/12/97/11/39/small_10.gif";
            DR = 360; 
        }

        private System.Timers.Timer _expirationTimer; 

        public void StartExpirationTimer()
        {
            _expirationTimer = new System.Timers.Timer()
            {
                Interval = 1000.0, 
                AutoReset = false
            };
            _expirationTimer.Elapsed += (sender, args) => { this.Destroy(); };
            _expirationTimer.Start(); 
        }
    }
}