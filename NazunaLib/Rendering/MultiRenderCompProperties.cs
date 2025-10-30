using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NareisLib
{
    public class MultiRenderCompProperties : CompProperties
    {
        public Dictionary<string, int> handDefNameAndWeights;

        public float apparelInterval = 0.0014478763f;

        public MultiRenderCompProperties()
        {
            compClass = typeof(MultiRenderComp);
        }
    }
}
