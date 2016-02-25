using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class StdObject2D : StdObject, IObject2D
    {
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
        public new long Id { get { return base.Id;  } set { } }
        public string Name { get; set; }
        public string Image { get; set; }
    }
}