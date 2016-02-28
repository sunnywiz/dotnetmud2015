using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Space2D : StdObject, HighFrequencyUpdateTimer.IHighFrequencyUpdateTarget
    {
        public Space2D()
        {
            var planet = Driver.GlobalObjects.CreateNewStdObject<Planet>();
            if (planet != null)
            {
                planet.Name = "Planet1";
                planet.Image = "https://cdn3.iconfinder.com/data/icons/nx11/Internet%20-%20Blue.png";
                planet.MoveTo(this);
                planet.Radius = 70; 
            }
            HighFrequencyUpdateTimer.Register(this);
        }

        public void HiFrequencyUpdate(HighFrequencyUpdateTimer.HighFrequencyUpdateInfo info)
        {
            MoveEverybodyAccordingToTheirVelocities(info);
            CheckForHits();
        }

        private void CheckForHits()
        {
            var timer = new Stopwatch();
            timer.Start(); 
            var targets = this.GetInventory<ICanBeHitByThings>();
            var hitters = this.GetInventory<IWantToHitThings>();

            // TODO: collision detection O(N^2). Make this better; research: QuadTree or other.  
            // TODO: possibly use long arithmetic for rough checks first.  Maybe sorted X. 
            int numHits = 0; 
            foreach (var h in hitters)
            {
                foreach (var t in targets)
                {
                    // rough check.  
                    if (Math.Abs(h.X - t.X) < h.Radius + t.Radius)
                    {
                        if (Math.Abs(h.Y - t.Y) < h.Radius + t.Radius)
                        {
                            // oo! pretty close.  Do the full check. 
                            var distSq = (h.X - t.X)*(h.X - t.X) + (h.Y - t.Y)*(h.Y - t.Y);
                            if (distSq < (h.Radius + t.Radius)*(h.Radius + t.Radius))
                            {
                                // hit!   if they accept.   Examples: missiles won't hit launcher. 
                                if (h.AcceptMeHasHit(t) && t.AcceptHasBeenHitBy(h))
                                {
                                    h.MeHasHit(t);
                                    t.MeHasBeenHitBy(h);
                                }
                                numHits++;
                            }
                        }
                    }
                }
            }
            timer.Stop(); 
            PerfLogging.SomethingIsCurrently("CheckForHits.TimeInMs",timer.ElapsedMilliseconds);
            PerfLogging.SomethingIsCurrently("CheckForHits.NumHits",numHits);
        }


        private void MoveEverybodyAccordingToTheirVelocities(HighFrequencyUpdateTimer.HighFrequencyUpdateInfo info)
        {
            var dt = Convert.ToDouble(info.ElapsedSeconds);
            foreach (var ob in GetInventory<IObject2D>())
            {
                ob.X += ob.DX*dt;
                ob.Y += ob.DY*dt;
                ob.R += ob.DR*dt;
            }
        }
    }
}