using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace NareisLib
{
    public class DAL_Rot4LayerSet
    {
        public Dictionary<Rot4, float> yRot4Offset = new Dictionary<Rot4, float>();

        public Dictionary<Rot4, List<DAL_RendLayerSet>> yRot4OffsetList = new Dictionary<Rot4, List<DAL_RendLayerSet>>();
    }
}
