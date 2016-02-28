using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Ship : StdObject2D, ICanBeHitByThings, HighFrequencyUpdateTimer.IHighFrequencyUpdateTarget
    {
        private static int _playerNumber = 0;
        private static readonly object _lock = new object(); 

        private readonly string[] _shipImages = new[]
        {
            // Sorry and/or Thank you folks from google image search. 
            // will replace with own artwork at some point. 

            "http://pixeljoint.com/files/icons/spaceship1_final.png",
            "http://www.i2clipart.com/cliparts/8/b/7/1/clipart-spacecraft-ii-8b71.png",
            "http://pixeljoint.com/files/icons/spaceship.png",
            "http://i24.photobucket.com/albums/c43/onlyeye/spaceship.gif",
            "http://www.i2clipart.com/cliparts/b/5/5/b/clipart-spaceship-glider-b55b.png",
            "http://www.pubzi.com/f/202060-sm-202053-Spaceship-Glider.png",
            "http://livebooklet.com/userFiles/a/2/4/4/5/1/7/ZOVM1UUhhhS7Wm56NMYe3r/LYuCzKXZ.png"
        };

        public void WelcomeNewPlayer()
        {
            lock (_lock)
            {
                var r = new Random();
                X = r.NextDouble()*400.0 - 200.0;
                Y = r.NextDouble()*400.0 - 200.0;
                DX = 0;
                DY = 0;
                R = r.NextDouble()*360.0;
                DR = 0;
                Radius = 50;  // should customize
                Name = "Player" + _playerNumber;
                Image = _shipImages[_playerNumber%_shipImages.Length];

                var space = Driver.GlobalObjects.FindSingleton(typeof (Space2D)) as Space2D;
                this.MoveTo(space);

                HighFrequencyUpdateTimer.Register(this);
                _playerNumber++;
            }
        }

        public decimal DesiredThrust { get; set; }
        public decimal DesiredLeft { get; set; }
        public decimal DesiredRight { get; set; }

        private decimal _lastFireTimeInGameMs = 0; 
        private const decimal FireSpeedInGameMs = 100.0m; 

        private readonly Stopwatch _timerBetweenClientRequestsPollFromServer = new Stopwatch(); 
        public object ClientRequestsPollFromServer(decimal thrustMs, decimal leftMs, decimal rightMs, decimal fireMs)
        {            
            PerfLogging.SomethingHappened(Id+" ClientRequestPoll");

            DesiredThrust = 0;
            DesiredLeft = 0;
            DesiredRight = 0;

            if (_timerBetweenClientRequestsPollFromServer.IsRunning)
            {
                var elapsedMs = _timerBetweenClientRequestsPollFromServer.ElapsedMilliseconds;
                if (elapsedMs > 0)
                {
                    DesiredThrust = thrustMs/elapsedMs;
                    DesiredLeft = leftMs/elapsedMs;
                    DesiredRight = rightMs/elapsedMs; 
                }
            }

            if (fireMs > 0)
            {
                var serverTimeNowInGameMs = GlobalTime.NowInMs;
                var elapsed = serverTimeNowInGameMs - _lastFireTimeInGameMs;
                if (elapsed > FireSpeedInGameMs)
                {
                    // do some firing! 
                    FireMissile(); 
                    _lastFireTimeInGameMs = serverTimeNowInGameMs; 
                }
            }

            var result = new PollResult()
            {
                Me = Object2DDto.CopyFrom(this),
                ServerTimeInSeconds = GlobalTime.NowInMs/1000.0m,
                ServerTimeRate = GlobalTime.RateOfTime, 
                MeHasBeenHitCount = this.MeHasBeenHitCount, 
                MeHasHitSomeoneCount = this.MeHasHitSomeoneCount
            };

            if (Parent != null)
            {
                result.Others = Parent.GetInventory<IObject2D>().Where(o => o != this).Select(Object2DDto.CopyFrom).ToList();
            }
            _timerBetweenClientRequestsPollFromServer.Restart();
            return result;         
        }

        const double MissileFIreSpeedInGameUnitsPerSec = 300.0;
        private const decimal MissileDurationInGameMs = 5000m; 

        public void FireMissile()
        {
            if (this.Parent == null) return;   // we're not in space. 
            var missile = Driver.GlobalObjects.CreateNewStdObject<Missile>();
            if (missile != null)
            {
                missile.Name = " ";
                missile.Image = "http://userbag.co.uk/demo/g1_demo/g_7/missile.png";

                missile.Radius = 3;
                var angle = R * Math.PI / 180.0;
                missile.X = this.X ;
                missile.Y = this.Y ;

                missile.DX = this.DX;
                missile.DY = this.DY;
                missile.R = this.R;
                missile.DX = missile.DX + Math.Cos(angle)*MissileFIreSpeedInGameUnitsPerSec;
                missile.DY = missile.DY + Math.Sin(angle)*MissileFIreSpeedInGameUnitsPerSec;
               
                missile.DurationRemainingInGameMs = MissileDurationInGameMs;
                missile.Launcher = this; 
                missile.MoveTo(this.Parent);
            }
        }

        public class PollResult
        {
            public PollResult()
            {
                Others = new List<IObject2D>();
            }

            public IObject2D Me { get; set; }
            public decimal ServerTimeInSeconds { get; set; }
            public decimal ServerTimeRate { get; set; }

            public List<IObject2D> Others { get; set; }
            public int MeHasBeenHitCount { get; set; }
            public int MeHasHitSomeoneCount { get; set; }
        }

        public void ShipDisconnected(bool stopCalled)
        {
            this.Destroy();
        }

        public void HiFrequencyUpdate(HighFrequencyUpdateTimer.HighFrequencyUpdateInfo info)
        {
            // TODO: should move the thrust thingies to properties .. customizable ships.. later. 
            DR = 0;
            if (DesiredLeft > 0)
            {
                DR = DR - Convert.ToDouble(DesiredLeft*90.0m);  // 90 degrees a second
            }
            if (DesiredRight > 0)
            {
                DR = DR + Convert.ToDouble(DesiredRight*90m);  
            }
            if (DesiredThrust > 0)
            {
                var angle = R*Math.PI/180.0;
                DX = DX + Math.Cos(angle)*50.0*Convert.ToDouble(DesiredThrust * info.ElapsedSeconds);
                DY = DY + Math.Sin(angle)*50.0*Convert.ToDouble(DesiredThrust * info.ElapsedSeconds);
            }
        }

        // Can't start with I, Resharper complains :) 
        public int MeHasBeenHitCount { get; set; }
        public int MeHasHitSomeoneCount { get; set; }

        public bool AcceptHasBeenHitBy(IWantToHitThings hitter)
        {
            return true; 
        }

        public void MeHasBeenHitBy(IWantToHitThings hitter)
        {
            MeHasBeenHitCount++; 
        }

        public void CountAHitAgainst(Ship target)
        {
            MeHasHitSomeoneCount++; 
        }
    }
}