using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Space2D : StdObject, GlobalTimers.IHighFrequencyUpdateTarget
    {
        private readonly Stopwatch _measurer;
        private long _lastSampledMilli; 

        public Space2D()
        {
            var planet = Driver.GlobalObjects.CreateNewStdObject<Planet>();
            if (planet != null)
            {
                planet.Name = "Planet1";
                planet.Image = "https://cdn3.iconfinder.com/data/icons/nx11/Internet%20-%20Blue.png";
                planet.MoveTo(this);
            }
            GlobalTimers.RegisterForHighFrequencyUpdate(this);
        }

        public void HiFrequencyUpdate(GlobalTimers.HighFrequencyUpdateInfo info)
        {
            var dt = Convert.ToDouble(info.ElapsedSeconds);
            foreach (var ob in GetInventory<IObject2D>())
            {
                ob.X += ob.DX * dt ;
                ob.Y += ob.DY * dt ;
                ob.R += ob.DR * dt ;
            }
        }
    }
}