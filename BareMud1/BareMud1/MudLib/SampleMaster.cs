using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareMud1.MudLib
{
    public class SampleMaster : IGameSpecifics
    {
        public IInteractive CreateNewPlayer()
        {
            return new User();
        }
    }
}
