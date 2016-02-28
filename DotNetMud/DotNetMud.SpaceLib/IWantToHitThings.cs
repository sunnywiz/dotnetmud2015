namespace DotNetMud.SpaceLib
{
    public interface IWantToHitThings : IObject2D
    {
        bool AcceptMeHasHit(ICanBeHitByThings target);
        void MeHasHit(ICanBeHitByThings target);
    }
}