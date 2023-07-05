using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HugsLib;
using HarmonyLib;
using AlienRace;
using HugsLib.Settings;

namespace NareisLib
{
    [EarlyInit]
    public class ThisModBase : ModBase
    {
        public override string ModIdentifier { get; } = "NareisLib.kamijouko.nazunarei";


        //给所有Pawn添加多层渲染Comp，CompTick有触发条件所以不存在性能问题
        [HarmonyPatch(typeof(ThingDef))]
        [HarmonyPatch("ResolveReferences")]
        public class InitialModThingDefCompPatch
        {
            static bool Prefix(ThingDef __instance)
            {
                if (__instance.thingClass != typeof(Pawn) || __instance.comps.Exists(x => x.GetType() == typeof(MultiRenderCompProperties)))
                    return true;
                __instance.comps.Add(new MultiRenderCompProperties());
                return true;
            }
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            ModStaticMethod.ThisMod = this;
            if (!ModStaticMethod.AllLevelsLoaded)
            {
                LoadAndResolveAllPlanDefs();
                ModStaticMethod.AllLevelsLoaded = true;
            }

            debugToggle = Settings.GetHandle<bool>(
                "displayDebugInfo",
                "displayDebugInfo_title".Translate(),
                "displayDebugInfo_desc".Translate(),
                false);

            apparelLevelsDisplayToggle = Settings.GetHandle<bool>(
                "displayLevelsInfo",
                "displayLevelsInfo_title".Translate(),
                "displayLevelsInfo_desc".Translate(),
                false);
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
                            ThisModData.RacePlansDatabase[race] = new RenderPlanDef(race);
                        ThisModData.RacePlansDatabase[race].Combine(plan);
                    }
                }
            }


            foreach (RenderPlanDef plan in ThisModData.RacePlansDatabase.Values)
            {
                if (plan.plans.NullOrEmpty())
                    continue;
                
                string planDef = plan.defName;
                if (!ThisModData.DefAndKeyDatabase.ContainsKey(planDef))
                    ThisModData.DefAndKeyDatabase[planDef] = new Dictionary<string, MultiTexDef>();

                foreach (MultiTexDef def in plan.plans)
                {
                    if (def.levels.NullOrEmpty() 
                        || def.originalDefClass == null 
                        || def.originalDef == null 
                        || ThisModData.DefAndKeyDatabase[planDef].ContainsKey(def.originalDefClass.ToStringSafe() + "_" + def.originalDef))
                        continue;

                    string type_originalDefName = def.originalDefClass.ToStringSafe() + "_" + def.originalDef;

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

        public SettingHandle<bool> debugToggle;
        public SettingHandle<bool> apparelLevelsDisplayToggle;

    }
}
