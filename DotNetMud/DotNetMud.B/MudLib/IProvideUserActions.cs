using System.Collections.Generic;

namespace DotNetMud.B.MudLib
{
    public interface IProvideUserActions
    {
        IEnumerable<UserAction> GetUserActions();
    }
}