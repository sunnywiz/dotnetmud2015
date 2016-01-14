using System.Collections.Generic;

namespace DotNetMud.MudLib
{
    public interface IProvideUserActions
    {
        IEnumerable<UserAction> GetUserActions();
    }
}