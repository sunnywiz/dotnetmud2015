namespace DotNetMud.SpaceLib
{
    public interface ICanBeHitByThings : IObject2D
    {
        bool AcceptHasBeenHitBy(IWantToHitThings hitter);
        void MeHasBeenHitBy(IWantToHitThings hitter);
    }
}