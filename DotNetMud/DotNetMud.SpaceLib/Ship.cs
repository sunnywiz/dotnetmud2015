using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Ship : StdObject, IObject2D, GlobalTimers.IHighFrequencyUpdateTarget
    {
        private static int playerNumber = 0;

        private string[] shipImages = new[]
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
            var r = new Random();
            X = r.NextDouble()*400.0 - 200.0;
            Y = r.NextDouble()*400.0 - 200.0;
            DX = 0;
            DY = 0;
            R = r.NextDouble() * 360.0;
            DR = 0;
            Name = this.ReadableId;
            Image = shipImages[playerNumber % shipImages.Length];

            var space = Driver.GlobalObjects.FindSingleton(typeof(Space2D)) as Space2D;
            this.MoveTo(space);

            GlobalTimers.RegisterForHighFrequencyUpdate(this);

            playerNumber++;
        }

        public double X { get; set; }
        public double Y { get; set; }
        /// <summary>
        /// per second
        /// </summary>
        public double DX { get; set; }
        public double DY { get; set; }

        /// <summary>
        /// Degrees
        /// </summary>
        public double R { get; set; }
        public double DR { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }

        public string Id
        {
            get { return ReadableId; }
            set
            {
                //ignore
            }
        }

        public decimal DesiredThrust { get; set; }
        public decimal DesiredLeft { get; set; }
        public decimal DesiredRight { get; set; }

        private decimal lastFireTimeInGameMs = 0; 
        private const decimal FireSpeedInGameMs = 100.0m; 

        private Stopwatch t1 = new Stopwatch(); 
        public object ClientRequestsPollFromServer(decimal thrustMs, decimal leftMs, decimal rightMs, decimal fireMs)
        {            
            PerfLogging.SomethingHappened(Id+" ClientRequestPoll");

            DesiredThrust = 0;
            DesiredLeft = 0;
            DesiredRight = 0;

            if (t1.IsRunning)
            {
                var elapsedMs = t1.ElapsedMilliseconds;
                if (elapsedMs > 0)
                {
                    DesiredThrust = thrustMs/elapsedMs;
                    DesiredLeft = leftMs/elapsedMs;
                    DesiredRight = rightMs/elapsedMs; 
                }
            }

            if (fireMs > 0)
            {
                var serverTimeNowInGameMs = GlobalTimers.NowInMs;
                var elapsed = serverTimeNowInGameMs - lastFireTimeInGameMs;
                if (elapsed > FireSpeedInGameMs)
                {
                    // do some firing! 
                    FireMissile(); 
                    lastFireTimeInGameMs = serverTimeNowInGameMs; 
                }
            }

            var result = new PollResult()
            {
                Me = Object2DDto.CopyFrom(this),
                ServerTimeInSeconds = GlobalTimers.NowInMs/1000.0m,
                ServerTimeRate = GlobalTimers.RateOfTime
            };

            if (Parent != null)
            {
                result.Others = Parent.GetInventory<IObject2D>().Where(o => o != this).Select(Object2DDto.CopyFrom).ToList();
            }
            t1.Restart();
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
                missile.Name = "";
                missile.Image = "http://userbag.co.uk/demo/g1_demo/g_7/missile.png"; 

                missile.X = this.X;
                missile.Y = this.Y;

                missile.DX = this.DX;
                missile.DY = this.DY;
                missile.R = this.R;
                var angle = R * Math.PI / 180.0;
                missile.DX = missile.DX + Math.Cos(angle)*MissileFIreSpeedInGameUnitsPerSec;
                missile.DY = missile.DY + Math.Sin(angle)*MissileFIreSpeedInGameUnitsPerSec;
               
                missile.DurationRemainingInGameMs = MissileDurationInGameMs; 
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
        }

        public void ShipDisconnected(bool stopCalled)
        {
            this.Destroy();
        }

        public void HiFrequencyUpdate(GlobalTimers.HighFrequencyUpdateInfo info)
        {
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
            // thrust is harder, do that later. 
        }
    }
}