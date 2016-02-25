using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace DotNetMud.Driver
{
    public class PerfLogging
    {
        private static System.Timers.Timer reportingTimer;
        private static Stopwatch ongoingTimer;

        private static Dictionary<string, Gurp> gurple;  

        private class Gurp
        {
            public int Count;
            public decimal? Value;
            public decimal? MinValue;
            public decimal? MaxValue;
            public decimal AverageSum; 
        }

        static PerfLogging()
        {
            gurple = new Dictionary<string, Gurp>(); 
            reportingTimer = new System.Timers.Timer()
            {
                Interval = 5000, 
                AutoReset = true, 
                Enabled=true
            };
            reportingTimer.Elapsed += ReportingTimer_Elapsed;
            ongoingTimer = new Stopwatch();
            ongoingTimer.Start(); 
        }

        private static void ReportingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var name in gurple.Keys.OrderBy(x => x))
            {
                var gurp = gurple[name];
                if (gurp.Count <= 0) continue;
                Trace.WriteLine(
                    // ReSharper disable once UseStringInterpolation too long
                    String.Format("{0}: rate:{1}/sec, Min/Avg/Max: {2:F2} {3:F2} {4:F2}",
                        name,
                        (Convert.ToDecimal(reportingTimer.Interval)/1000.0m*gurp.Count),
                        gurp.MinValue,
                        gurp.AverageSum/gurp.Count,
                        gurp.MaxValue
                        ));
                gurp.Count = 0;
                gurp.AverageSum = 0; 
            }
        }

        /// <summary>
        /// measures how often something is happening in FPS or something. 
        /// </summary>
        /// <param name="what"></param>
        public static void SomethingHappened(string name)
        {
            SomethingIsCurrently(name,0m);
        }

        /// <summary>
        /// keeps track of where a value for something is. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="newValue"></param>
        public static void SomethingIsCurrently(string name, decimal newValue)
        {
            Gurp gurp;
            if (gurple.TryGetValue(name, out gurp))
            {
                // we have a previous record
                gurp.Value = newValue;
                if (!gurp.MinValue.HasValue || newValue < gurp.MinValue.Value) gurp.MinValue = newValue;
                if (!gurp.MaxValue.HasValue || newValue > gurp.MaxValue.Value) gurp.MaxValue = newValue;

                gurp.AverageSum += newValue;
                gurp.Count++; 
            }
            else
            {
                // no previous record, create one
                gurple[name] = new Gurp()
                {
                    Count = 0,
                    AverageSum = 0,
                    MaxValue = null,
                    MinValue = null,
                    Value = null
                };
            }
        }
    }
}