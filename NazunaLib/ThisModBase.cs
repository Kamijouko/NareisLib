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
                
                if (!plan.races.NullOrEmpty())
                {
                    foreach (string race in plan.races)
                    {
                        if (!ThisModData.RacePlansDatabase.ContainsKey(race))
                            ThisModData.RacePlansDatabase[race] = new List<RenderPlanDef>();
                        if (!ThisModData.RacePlansDatabase[race].Exists(x => x.defName == plan.defName))
                            ThisModData.RacePlansDatabase[race].Add(plan);
                    }
                }
                string planDef = plan.defName;
                if (!ThisModData.DefAndKeyDatabase.ContainsKey(planDef))
                    ThisModData.DefAndKeyDatabase[planDef] = new Dictionary<string, MultiTexDef>();

                foreach (MultiTexDef def in plan.plans)
                {
                    string type_originalDefName = def.originalDefClass.ToStringSafe() + "_" + def.originalDef;

                    if (def.levels.NullOrEmpty() 
                        || def.originalDefClass == null 
                        || def.originalDef == null 
                        || ThisModData.DefAndKeyDatabase[planDef].ContainsKey(type_originalDefName))
                        continue;

                    if (!ThisModData.TexLevelsDatabase.ContainsKey(type_originalDefName))
                        ThisModData.TexLevelsDatabase[type_originalDefName] = new Dictionary<string, TextureLevels>();

                    foreach (TextureLevels level in def.levels)
                    {
                        level.GetAllGraphicDatas(def);
                    }

                    ThisModData.DefAndKeyDatabase[planDef][type_originalDefName] = def;
                }
            }
        }
    }
}
