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
        public string handDefName = "";

        public MultiRenderCompProperties()
        {
            compClass = typeof(MultiRenderComp);
        }
    }
}
