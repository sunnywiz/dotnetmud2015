namespace DotNetMud.C.SpaceLib
{
    /// <summary>
    /// All things representable on the 2D grid. 
    /// </summary>
    public abstract class Object2D : IObject2D
    {
        public Space2D Container { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double DX { get; set; }
        public double DY { get; set; }
        public double R { get; set; }  // in radians
        public double DR { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
    }
}