using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NareisLib
{
    public class DAL_PawnUpperData
    {
        public int ticksBetweenFrame = 4;
        public Dictionary<Rot4, int> frameCount = new Dictionary<Rot4, int>() { { Rot4.North, 1 }, { Rot4.East, 1 }, { Rot4.South, 1 }, { Rot4.West, 1 } };
        public GraphicData frame;
    }
}
