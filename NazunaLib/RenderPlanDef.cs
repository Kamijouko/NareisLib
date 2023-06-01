using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace NareisLib
{
    public class RenderPlanDef : Def
    {
        public List<string> races = new List<string>();
        public List<MultiTexDef> plans = new List<MultiTexDef>();

        public void Combie(RenderPlanDef def)
        {
            races = races.Concat(def.races).Distinct().ToList();
            foreach (MultiTexDef multiDef in def.plans)
            {
                if (!plans.Exists(x => x.defName == multiDef.defName))
                    plans.Add(multiDef);
            }
        }
    }
}
