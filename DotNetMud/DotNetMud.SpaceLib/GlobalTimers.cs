using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace DotNetMud.SpaceLib
{
    // TODO: GLobalTimers might want to move up to the Driver proper.   

    public class GlobalTimers
    {
        /// <summary>
        /// number of milliseconds.   Can be slowed down. 
        /// </summary>
        public static decimal NowInMs
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

        const long DesiredHfUpdateIntervalInMs = 10;  

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

            var time = NowInMs;
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
                    hfLastNow = NowInMs;
                    return; 
                }
                var thisNow = NowInMs;
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
                PerfLogging.SomethingIsCurrently("HfUpdate TimeTaken",myRunTime);
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

