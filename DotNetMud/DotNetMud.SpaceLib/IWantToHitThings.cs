namespace DotNetMud.SpaceLib
{
    public interface IWantToHitThings : IObject2D
    {
        void IHaveHit(ICanBeHitByThings target);
    }
}