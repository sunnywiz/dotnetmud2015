using System.Diagnostics;

// ReSharper disable RedundantNameQualifier

namespace DotNetMud.Driver
{
    public class GlobalTime
    {
        /// <summary>
        /// number of milliseconds.   Can be slowed down. 
        /// </summary>
        public static decimal NowInMs
        {
            get
            {
                var nowElapsedMillis = Timer.ElapsedMilliseconds;
                var numberOfMillisElapsedSinceLastCheck = nowElapsedMillis - _elapsedMillisAtLasSample;
                var result = _nowAtLastSample + numberOfMillisElapsedSinceLastCheck * RateOfTime;
                _elapsedMillisAtLasSample = nowElapsedMillis;
                _nowAtLastSample = result;
                return result;
            }
        }

        public static decimal RateOfTime { get; }

        private static readonly Stopwatch Timer;
        private static decimal _nowAtLastSample;
        private static long _elapsedMillisAtLasSample;


        static GlobalTime()
        {
            RateOfTime = 1.0m; // number of millis per second. 
            Timer = new Stopwatch();
            Timer.Start();
            _nowAtLastSample = 0;
            _elapsedMillisAtLasSample = 0;

            // ReSharper disable once UnusedVariable
            var time = NowInMs;
        }

    }
}

