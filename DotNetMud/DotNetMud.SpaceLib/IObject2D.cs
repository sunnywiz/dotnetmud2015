// ReSharper disable InconsistentNaming

using Newtonsoft.Json.Serialization;

namespace DotNetMud.SpaceLib
{
    public interface IObject2D
    {
        double X { get; set; }
        double Y { get; set; }
        double DX { get; set; }
        double DY { get; set; }
        double R { get; set; }
        double DR { get; set; }
        double Radius { get; set; }
        long Id { get; set; }
        string Name { get; set; }
        string Image { get; set; }
    }
}