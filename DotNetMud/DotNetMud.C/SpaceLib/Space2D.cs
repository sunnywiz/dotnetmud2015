using System.Collections.Generic;
using DotNetMud.A.Server;

namespace DotNetMud.C.SpaceLib
{
    public class Space2D : StdObject
    {
        public Space2D()
        {
            Short = "Solar System";
            Objects = new List<IObject2D>();
        }
        public List<IObject2D> Objects { get; private set; }
    }
}