using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Space2D : StdObject
    {
        private readonly Stopwatch _measurer;
        private long _lastSampledMilli; 

        public Space2D()
        {
            Objects = new List<IObject2D>();

            Objects.Add(new Planet() { Name="Planet1",X=0,Y=0,Image="Planet1"});

            _measurer = new Stopwatch();
            _measurer.Start();

            var timer = new System.Timers.Timer
            {
                Interval = 100,
                AutoReset = true
            };
            // 100 msec? = 10x a sec? 
            timer.Elapsed += (sender, args) => TimeUpdate();
            timer.Start(); 

            _lastSampledMilli = _measurer.ElapsedMilliseconds;
        }

        public List<IObject2D> Objects { get; private set; }

        public void TimeUpdate()
        {
            var currentMilli = _measurer.ElapsedMilliseconds; 
            if (_lastSampledMilli == 0)
            {
                _lastSampledMilli = currentMilli;
                return;   // no-op the first time. 
            }

            var dt = (currentMilli - _lastSampledMilli);
            _lastSampledMilli = currentMilli;

            foreach (var ob in Objects)
            {
                ob.X += ob.DX*dt/1000.0;
                ob.Y += ob.DY*dt/1000.0;
                ob.R += ob.DR*dt/1000.0; 
            }
        }
    }

    public class Planet : StdObject, IObject2D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double DX { get; set; }
        public double DY { get; set; }
        public double R { get; set; }
        public double DR { get; set; }
        public string Id
        {
            get { return ObjectId; }
            set
            {
                //ignore
            }
        }
        public string Name { get; set; }
        public string Image { get; set; }
    }

}