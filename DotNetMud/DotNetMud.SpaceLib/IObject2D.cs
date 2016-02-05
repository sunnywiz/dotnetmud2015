// ReSharper disable InconsistentNaming
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
        string Id { get; set; }
        string Name { get; set; }
        string Image { get; set; }
    }

    public class Object2DDto : IObject2D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double DX { get; set; }
        public double DY { get; set; }
        public double R { get; set; }
        public double DR { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Id { get; set; }

        public static IObject2D CopyFrom(IObject2D t)
        {
            return new Object2DDto()
            {
                X = t.X,
                Y = t.Y,
                DX = t.DX,
                DY = t.DY,
                Name = t.Name,
                Image = t.Image,
                DR = t.DR,
                R = t.R,
                Id = t.Id
            };
        }
    }
}