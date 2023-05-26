using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HugsLib;

namespace NareisLib
{
    public class ThisModBase : ModBase
    {
        public override string ModIdentifier { get; } = "NareisLib.kamijouko.nazunarei";

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            ModStaticMethod.ThisMod = this;
            if (!ModStaticMethod.AllLevelsLoaded)
            {
                LoadAndResolveAllPlanDefs();
                ModStaticMethod.AllLevelsLoaded = true;
            }
        }

        public static void LoadAndResolveAllPlanDefs()
        {
            List<RenderPlanDef> list = DefDatabase<RenderPlanDef>.AllDefsListForReading;
            if (list.NullOrEmpty())
                return;
            foreach (RenderPlanDef plan in list)
            {
                if (plan.plans.NullOrEmpty())
                    continue;
                string planDef = plan.defName;
                if (!ThisModData.DefAndKeyDatabase.ContainsKey(planDef))
                    ThisModData.DefAndKeyDatabase[planDef] = new Dictionary<Type, Dictionary<string, MultiTexDef>>();
                foreach (MultiTexDef def in plan.plans)
                {
                    if (def.levels.NullOrEmpty() 
                        || def.originalDefClass == null 
                        || def.originalDef == null 
                        || (ThisModData.DefAndKeyDatabase[planDef].ContainsKey(def.originalDefClass) && ThisModData.DefAndKeyDatabase[planDef][def.originalDefClass].ContainsKey(def.originalDef)))
                        continue;

                    if (!ThisModData.DefAndKeyDatabase[planDef].ContainsKey(def.originalDefClass))
                        ThisModData.DefAndKeyDatabase[planDef][def.originalDefClass] = new Dictionary<string, MultiTexDef>();
                    foreach (TextureLevels level in def.levels)
                    {
                        level.GetAllGraphicDatas(def.path);
                    }
                    ThisModData.DefAndKeyDatabase[planDef][def.originalDefClass][def.originalDef] = def;
                }
            }
        }
    }
}
