using System;
using System.Collections.Generic;
using System.Linq;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Ship : StdObject, IObject2D
    {
        public void WelcomeNewPlayer()
        {
            var r = new Random();
            X = 200;
            Y = 100;
            DX = 10;
            DY = 5;
            R = 45;
            DR = -5; 
            Name = this.ObjectId;
            Image = "http://pixeljoint.com/files/icons/spaceship1_final.png";

            var space = Driver.GlobalObjects.FindSingleton(typeof (Space2D)) as Space2D;
            space.Objects.Add(this);
            this.Container = space; 
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

        public object ClientRequestsPollFromServer()
        {
            var result = new PollResult() {Me = Object2DDto.CopyFrom(this)};

            if (Container != null)
            {
                result.Others = Container.Objects.Where(o => o != this).Select(Object2DDto.CopyFrom).ToList(); 
            }

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