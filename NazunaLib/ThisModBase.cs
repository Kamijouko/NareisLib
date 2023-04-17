using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HugsLib;

namespace NazunaLib
{
    public class ThisModBase : ModBase
    {
        public override string ModIdentifier { get; } = "NazunaReiLib.kamijouko";

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            ModStaticMethod.ThisMod = this;
            LoadAndResolveAllPlanDefs();
            ModStaticMethod.AllLevelsLoaded = true;
        }

        public void LoadAndResolveAllPlanDefs()
        {
            List<RenderPlanDef> list = DefDatabase<RenderPlanDef>.AllDefsListForReading;
            if (list.NullOrEmpty())
                return;
            foreach (RenderPlanDef plan in list)
            {
                if (plan.plans.NullOrEmpty())
                    continue;
                foreach (MultiTexDef def in plan.plans)
                {
                    if (def.levels.NullOrEmpty() || ThisModData.DefAndKeyDatabase.ContainsKey(def.originalDef.defName))
                        continue;
                    foreach (TextureLevels level in def.levels)
                    {
                        level.GetAllGraphicDatas(def.path);
                    }
                    ThisModData.DefAndKeyDatabase.Add(def.originalDef.defName, def);
                }
            }
        }
    }
}
