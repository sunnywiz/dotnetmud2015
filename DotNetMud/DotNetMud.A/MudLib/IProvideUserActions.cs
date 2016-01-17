using System.Collections.Generic;

namespace DotNetMud.A.MudLib
{
    public interface IProvideUserActions
    {
        IEnumerable<UserAction> GetUserActions();
    }
}