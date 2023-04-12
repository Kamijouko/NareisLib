using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace NazunaLib
{
    public class PawnRenderPlanDef : Def
    {
        public List<RenderPlanDefSet> plans = new List<RenderPlanDefSet>();
    }
}
