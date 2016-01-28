// ReSharper disable InconsistentNaming
namespace DotNetMud.C.SpaceLib
{
    public interface IObject2D
    {
        Space2D Container { get; set; }
        double X { get; set; }
        double Y { get; set; }
        double DX { get; set; }
        double DY { get; set; }
        double R { get; set; }
        double DR { get; set; }
        string Name { get; set; }
        string Image { get; set; }
    }
}