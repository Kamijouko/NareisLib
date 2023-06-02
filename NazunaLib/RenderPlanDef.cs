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

        //xml里不需要设置
        public List<string> combinedPlanDefNames = new List<string>();

        public RenderPlanDef(string race)
        {
            defName = race + "_planDef_combine";
            races[0] = race;
        }

        public void Combine(RenderPlanDef def)
        {
            //races = races.Concat(def.races).Distinct().ToList();
            foreach (MultiTexDef multiDef in def.plans)
            {
                if (!plans.Exists(x => x.defName == multiDef.defName))
                    plans.Add(multiDef);
            }
        }
    }
}
