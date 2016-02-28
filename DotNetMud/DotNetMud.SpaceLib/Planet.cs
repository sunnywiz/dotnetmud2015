using DotNetMud.Driver;

namespace DotNetMud.SpaceLib
{
    public class Planet : StdObject2D, ICanBeHitByThings
    {
        public int HitCount;
        public bool AcceptHasBeenHitBy(IWantToHitThings hitter)
        {
            return true; 
        }

        public void MeHasBeenHitBy(IWantToHitThings hitter)
        {
            HitCount++;
            this.Name = $"Planet (HitCount: {HitCount})";
        }
    }

}