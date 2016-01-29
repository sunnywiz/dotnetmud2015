using System.Collections.Generic;

namespace DotNetMud.Mudlib
{
    public interface IProvideUserActions
    {
        IEnumerable<UserAction> GetUserActions();
    }
}