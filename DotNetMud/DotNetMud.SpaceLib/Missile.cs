using System;
using System.Timers;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Missile: StdObject2D, IWantToHitThings
    {
        private decimal _willExpireAtInGameSeconds = 0m;
        private System.Timers.Timer _expirationTimer = null; 

        public Ship Launcher { get; set; }

        // TODO: probably need to make a Game-time timer so that in x Game-TIme-ms something fires. 
        public decimal DurationRemainingInGameMs
        {
            get
            {
                var now = GlobalTime.NowInMs;
                return _willExpireAtInGameSeconds * 1000m - now; 
            }
            set
            {
                var now = GlobalTime.NowInMs;
                _willExpireAtInGameSeconds = (now + value)/1000m;
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
            var timeToGo = _willExpireAtInGameSeconds*1000 - now;
            if (timeToGo > 0)
            {
                _expirationTimer.Interval = Convert.ToDouble(timeToGo)*1.5;
                _expirationTimer.Start();
                return; 
            }
            // otherwise, time has expired. 
            this.Destroy(); 
        }

        public bool AcceptMeHasHit(ICanBeHitByThings target)
        {
            if (Launcher == null) return false;  // not armed yet. 
            if (target == Launcher) return false;  // can't hit myself. 
            return true; 
        }

        public void MeHasHit(ICanBeHitByThings target)
        {
            if (this.Parent is Space2D)
            {
                var explosion = new Explosion() {X = this.X, Y = this.Y, DX = target.DX, DY = target.DY};
                explosion.MoveTo(this.Parent);
                explosion.StartExpirationTimer();
            }

            if (Launcher != null && !Launcher.IsDestroyed && target is Ship)
            {
                Launcher.CountAHitAgainst((Ship)target);
            }

            this.Destroy();
        }
        
    }
}