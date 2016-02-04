using System;
using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Ship : StdObject, IObject2D
    {

        public object RequestPoll(string pollName, object clientState)
        {
            return new PollResult1()
            {
                // WHERE ARE WE LEAVING OFF. 
                // we need a smaller DTO for geometry that doesn't have the Container.  
                // I think the Container is a SpaceLibObject that also implments the interface. 
                // then we need to provie all the geometry for all the objects in the pollresult. 
                // then we need to echo that information on the client
                // then we test two people logging in
                // then we need to put a canvas on the client .. when we get an update, draw the update
                // then we need to set up an animation loop on the server
                // then we need to see the client get updated in a choppy way 
                // then we need the animation loop on the client to interpolate in a smooth way
                // then we need to send thrust info back to the server
            };
        }

        public void WelcomeNewPlayer()
        {
            var r = new Random();
            X = r.NextDouble()*200 - 100;
            Y = r.NextDouble()*200 - 100;
            DX = r.NextDouble()*10 - 5;
            DY = r.NextDouble()*10 - 5;

            var space = Driver.GlobalObjects.FindSingleton(typeof (Space2D)) as Space2D;
            space.Objects.Add(this);
        }

        public Space2D Container { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double DX { get; set; }
        public double DY { get; set; }
        public double R { get; set; }
        public double DR { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }

        public object ClientRequestsPollFromServer()
        {
            var result = new PollResult() {Me = Object2DDto.CopyFrom(this)};
            return result; 
        }

        public class PollResult
        {
            public IObject2D Me { get; set; }
        }
    }
}