using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace DotNetMud.Driver
{
    public class HighFrequencyUpdateTimer
    {
        const long DesiredHfUpdateIntervalInMs = 10;

        private static readonly System.Timers.Timer TickyTimer;
        private static readonly List<WeakReference<IHighFrequencyUpdateTarget>> Targets;
        private static readonly Stopwatch SlownessTimer;

        private static decimal _lastGlobaltimeNowInMs;
        private static object _lock = new object(); 

        static HighFrequencyUpdateTimer()
        {
            SlownessTimer = new Stopwatch();
            Targets = new List<WeakReference<IHighFrequencyUpdateTarget>>();

            TickyTimer = new System.Timers.Timer
            {
                AutoReset = false,
                Interval = DesiredHfUpdateIntervalInMs
            };
            TickyTimer.Elapsed += TickyTimerOnElapsed;
            TickyTimer.Start();
        }
        private static void TickyTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (_lock)
            {
                SlownessTimer.Restart();
                try
                {
                    if (_lastGlobaltimeNowInMs == 0)
                    {
                        _lastGlobaltimeNowInMs = GlobalTime.NowInMs;
                        return;
                    }
                    var thisNow = GlobalTime.NowInMs;
                    var info = new HighFrequencyUpdateInfo()
                    {
                        LastNowInSeconds = _lastGlobaltimeNowInMs/1000.0m,
                        ThisNowInSeconds = thisNow/1000.0m,
                        ElapsedSeconds = (thisNow - _lastGlobaltimeNowInMs)/1000.0m,
                        Rate = GlobalTime.RateOfTime
                    };
                    _lastGlobaltimeNowInMs = thisNow;
                    for (var index = Targets.Count - 1; index >= 0; index--)
                    {
                        var wr = Targets[index];
                        IHighFrequencyUpdateTarget t;
                        if (wr.TryGetTarget(out t))
                        {
                            // ReSharper disable once SuspiciousTypeConversion.Global
                            var u = t as StdObject;
                            if (u != null)
                            {
                                if (!u.IsDestroyed)
                                {
                                    t.HiFrequencyUpdate(info);
                                    continue;
                                }
                            }
                        }
                        // all other cases, falure to resolve, don't try again. 
                        Targets.RemoveAt(index);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Exception during HFupdate, ignored: {ex.Message}");
                    // really, do nothing. 
                }
                finally
                {
                    SlownessTimer.Stop();
                    var myRunTime = SlownessTimer.ElapsedMilliseconds;
                    PerfLogging.SomethingIsCurrently("HfUpdate TimeTaken", myRunTime);
                    PerfLogging.SomethingIsCurrently("HfUpdate Q Length", Targets.Count);

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

                    TickyTimer.Interval = sleepTime;
                    TickyTimer.Start();
                }
            }
        }

        public static void DeRegister(IHighFrequencyUpdateTarget target)
        {
            lock (_lock)
            {
                if (!(target is StdObject)) return;
                for (int index = 0; index < Targets.Count; index++)
                {
                    var weakRef = Targets[index];
                    IHighFrequencyUpdateTarget strongRef;
                    if (weakRef.TryGetTarget(out strongRef))
                    {
                        if (strongRef == target)
                        {
                            Targets.RemoveAt(index);
                        }
                    }
                }
            }
        }

        public static void Register(IHighFrequencyUpdateTarget target)
        {
            lock (_lock)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (!(target is StdObject)) return;
                foreach (var weakRef in Targets)
                {
                    IHighFrequencyUpdateTarget strongRef;
                    if (weakRef.TryGetTarget(out strongRef))
                    {
                        if (strongRef == target) return;
                    }
                }
                Targets.Add(new WeakReference<IHighFrequencyUpdateTarget>(target));
            }
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