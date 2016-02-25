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
            Objects = new List<IObject2D>();

            var planet = Driver.GlobalObjects.CreateNewStdObject(typeof (Planet)) as Planet;
            if (planet != null)
            {
                planet.Name = "Planet1";
                planet.Image = "https://cdn3.iconfinder.com/data/icons/nx11/Internet%20-%20Blue.png";
                // TODO: i think inventory management should move back up to StdObject since both space and regular need it. 
                Objects.Add(planet);
            }

            GlobalTimers.RegisterForHighFrequencyUpdate(this);
        }

        public List<IObject2D> Objects { get; private set; }

        public void HiFrequencyUpdate(GlobalTimers.HighFrequencyUpdateInfo info)
        {
            var dt = Convert.ToDouble(info.ElapsedSeconds);
            foreach (var ob in Objects)
            {
                ob.X += ob.DX * dt ;
                ob.Y += ob.DY * dt ;
                ob.R += ob.DR * dt ;
            }
        }
    }
}