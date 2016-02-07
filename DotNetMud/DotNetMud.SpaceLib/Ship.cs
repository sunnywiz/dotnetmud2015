using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Ship : StdObject, IObject2D
    {
        private static int playerNumber = 0;

        private string[] shipImages = new [] 
        {
            "http://pixeljoint.com/files/icons/spaceship1_final.png",
            "http://orig15.deviantart.net/0400/f/2014/172/0/d/spaceship_warp_animation_by_nnj3de-d7ncf40.gif",
            "http://pixeljoint.com/files/icons/spaceship.png",
            "http://i24.photobucket.com/albums/c43/onlyeye/spaceship.gif"
        };
        public void WelcomeNewPlayer()
        {
            var r = new Random();
            X = playerNumber*100+100;
            Y = 0;
            DX = 1;
            DY = 1;
            R = 45;
            DR = 5; 
            Name = this.ObjectId;
            Image = shipImages[playerNumber%shipImages.Length];

            var space = Driver.GlobalObjects.FindSingleton(typeof (Space2D)) as Space2D;
            space.Objects.Add(this);
            this.Container = space;

            playerNumber++; 
        }

        public Space2D Container { get; set; }
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
            get { return ObjectId; }
            set
            {
                //ignore
            }
        }

        
        private Stopwatch timeBetweenPolls = new Stopwatch();
        private Stopwatch logEvery5Secnds = new Stopwatch();
        private decimal avgTimeBetweenPolls = 0.0m; 
        public object ClientRequestsPollFromServer()
        {
            if (timeBetweenPolls.IsRunning)
            {
                var sample = Convert.ToDecimal(timeBetweenPolls.ElapsedMilliseconds);
                avgTimeBetweenPolls = avgTimeBetweenPolls*0.9m + sample*0.1m;
                if (!logEvery5Secnds.IsRunning)
                {
                    logEvery5Secnds.Start();
                }
                else
                {
                    if (logEvery5Secnds.ElapsedMilliseconds > 5000)
                    {
                        logEvery5Secnds.Restart();
                        Trace.WriteLine($"{ObjectId} polling at {avgTimeBetweenPolls} ms");
                    }
                }
            }
            var result = new PollResult() {Me = Object2DDto.CopyFrom(this)};

            if (Container != null)
            {
                result.Others = Container.Objects.Where(o => o != this).Select(Object2DDto.CopyFrom).ToList(); 
            }

            timeBetweenPolls.Restart();
            return result; 
        }

        public class PollResult
        {
            public PollResult()
            {
                Others = new List<IObject2D>();
            }
            public IObject2D Me { get; set; }
            public List<IObject2D> Others { get; set; } 
        }

        public void ShipDisconnected(bool stopCalled)
        {
            this.Container.Objects.Remove(this);
            this.Container = null; 
            this.Destroy();
        }
    }
}