using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
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