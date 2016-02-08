using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Ship : StdObject, IObject2D
    {
        private static int playerNumber = 0;

        private string[] shipImages = new[]
        {
            "http://pixeljoint.com/files/icons/spaceship1_final.png",
            "http://orig15.deviantart.net/0400/f/2014/172/0/d/spaceship_warp_animation_by_nnj3de-d7ncf40.gif",
            "http://pixeljoint.com/files/icons/spaceship.png",
            "http://i24.photobucket.com/albums/c43/onlyeye/spaceship.gif"
        };
        public void WelcomeNewPlayer()
        {
            var r = new Random();
            X = playerNumber * 100 + 100;
            Y = 0;
            DX = 1;
            DY = 1;
            R = 45;
            DR = 5;
            Name = this.ObjectId;
            Image = shipImages[playerNumber % shipImages.Length];

            var space = Driver.GlobalObjects.FindSingleton(typeof(Space2D)) as Space2D;
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
                avgTimeBetweenPolls = avgTimeBetweenPolls * 0.9m + sample * 0.1m;
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
            var result = new PollResult() { Me = Object2DDto.CopyFrom(this) };

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

    public class GlobalTimers
    {
        /// <summary>
        /// number of milliseconds.   Can be slowed down. 
        /// </summary>
        public static decimal Now
        {
            get
            {
                var nowElapsedMillis = timer.ElapsedMilliseconds;
                var numberOfMillisElapsedSinceLastCheck = nowElapsedMillis - elapsedMillisAtLasSample;
                var result = nowAtLastSample + numberOfMillisElapsedSinceLastCheck * RateOfTime;
                elapsedMillisAtLasSample = nowElapsedMillis;
                nowAtLastSample = result;
                return result;
            }
        }

        public static decimal RateOfTime { get; private set; }

        private static Stopwatch timer;
        private static decimal nowAtLastSample;
        private static long elapsedMillisAtLasSample;

        const long DesiredHfUpdateIntervalInMs = 100;  

        static GlobalTimers()
        {
            RateOfTime = 1.0m; // number of millis per second. 
            timer = new Stopwatch();
            hfUpdateSlownessTimer = new Stopwatch(); 
            timer.Start();
            nowAtLastSample = 0;
            elapsedMillisAtLasSample = 0;

            // TODO: High Frequency stuff probably should be its own class.
            // except if its taking too long, probably want to slow time down. 
            hfTargets = new List<WeakReference<IHighFrequencyUpdateTarget>>();

            var time = Now;
            hfTimer = new System.Timers.Timer();
            hfTimer.AutoReset = false;
            hfTimer.Interval = DesiredHfUpdateIntervalInMs;
            hfTimer.Elapsed += HfTimerOnElapsed;
            hfTimer.Start();
        }

        private static decimal hfLastNow = 0;
        private static Stopwatch hfUpdateSlownessTimer;
        private static void HfTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            hfUpdateSlownessTimer.Restart(); 
            try
            {
                if (hfLastNow == 0)
                {
                    hfLastNow = Now;
                    return; 
                }
                var thisNow = Now;
                var info = new HighFrequencyUpdateInfo()
                {
                    LastNowInSeconds = hfLastNow / 1000.0m,
                    ThisNowInSeconds = thisNow / 1000.0m,
                    ElapsedSeconds = (thisNow - hfLastNow)/1000.0m,
                    Rate = RateOfTime
                };
                hfLastNow = thisNow; 
                for (int index = hfTargets.Count-1; index >=0; index--)
                {
                    var wr = hfTargets[index];
                    IHighFrequencyUpdateTarget t;
                    if (wr.TryGetTarget(out t))
                    {
                        t.HiFrequencyUpdate(info);
                    }
                    else
                    {
                        hfTargets.RemoveAt(index);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Exception during HFupdate, ignored: {ex.Message}");
                // really, do nothing. 
            }
            finally
            {
                hfUpdateSlownessTimer.Stop(); 
                var myRunTime = hfUpdateSlownessTimer.ElapsedMilliseconds;
                Trace.WriteLine($"HfUpdate took {myRunTime}ms");
                var sleepTime = DesiredHfUpdateIntervalInMs - myRunTime;

                if (sleepTime < 0)
                {
                    sleepTime = 1;
                    // this is where rate decrease goes. 
                }
                else
                {
                    // this is where possible speed time back up again goes
                }

                hfTimer.Interval = sleepTime; 
                hfTimer.Start();
            }
        }

        private static System.Timers.Timer hfTimer;
        private static List<WeakReference<IHighFrequencyUpdateTarget>> hfTargets;
        public static void RegisterForHighFrequencyUpdate(IHighFrequencyUpdateTarget target)
        {
            hfTargets.Add(new WeakReference<IHighFrequencyUpdateTarget>(target));
        }

        public interface IHighFrequencyUpdateTarget
        {
            void HiFrequencyUpdate(HighFrequencyUpdateInfo info);
        }

        public class HighFrequencyUpdateInfo
        {
            public decimal LastNowInSeconds { get; set; }
            public decimal ThisNowInSeconds { get; set; }
            public decimal Rate { get; set; }
            public decimal ElapsedSeconds { get; set; }
        }
    }
}