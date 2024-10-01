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
using System.Diagnostics;
using System.Reflection;

namespace NareisLib
{
    [StaticConstructorOnStartup]
    public class HarmonyMain
    {
        static HarmonyMain()
        {
            var harmonyInstance = new Harmony("NareisLib.kamijouko.nazunarei");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

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

        //给所有PawnRenderNode添加SubWorker
        [HarmonyPatch(typeof(PawnRenderNodeProperties))]
        [HarmonyPatch("EnsureInitialized")]
        public class InitialModPawnRenderNodeSubWorkerPatch
        {
            static void Postfix(PawnRenderNodeProperties __instance)
            {
                if (__instance.workerClass != typeof(PawnRenderNodeWorker_TextureLevels) && !__instance.subworkerClasses.Contains(typeof(DefaultNodeSubWorker)) && !__instance.subworkerClasses.Contains(typeof(TextureLevelsToNodeSubWorker)))
                    __instance.subworkerClasses.Add(typeof(DefaultNodeSubWorker));
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

            pawnCurJobDisplayToggle = Settings.GetHandle<bool>(
                "pawnCurJobInfo",
                "pawnCurJobInfo_title".Translate(),
                "pawnCurJobInfo_desc".Translate(),
                false);
        }

        public static void LoadAndResolveAllPlanDefs()
        {
            ThisModData.SuffixList = DefDatabase<BodyTypeDef>.AllDefsListForReading.Select(x => x.defName).Concat(DefDatabase<HeadTypeDef>.AllDefsListForReading.Select(x => x.defName)).ToList();

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
                
                if (!ThisModData.DefAndKeyDatabase.ContainsKey(plan.defName))
                    ThisModData.DefAndKeyDatabase[plan.defName] = new Dictionary<string, MultiTexDef>();

                foreach (MultiTexDef def in plan.plans)
                {
                    if (def.levels.NullOrEmpty() 
                        || def.originalDefClass == null 
                        || def.originalDef == null 
                        || ThisModData.DefAndKeyDatabase[plan.defName].ContainsKey(def.originalDefClass.ToStringSafe() + "_" + def.originalDef))
                        continue;

                    string type_originalDefName = def.originalDefClass.ToStringSafe() + "_" + def.originalDef;

                    if (!ThisModData.TexLevelsDatabase.ContainsKey(type_originalDefName))
                        ThisModData.TexLevelsDatabase[type_originalDefName] = new Dictionary<string, TextureLevels>();

                    foreach (TextureLevels level in def.levels)
                    {
                        level.GetAllGraphicDatas(def);
                    }

                    ThisModData.DefAndKeyDatabase[plan.defName][type_originalDefName] = def;
                }
                
            }
            list = null;
            GC.Collect();
        }

        public SettingHandle<bool> debugToggle;
        public SettingHandle<bool> apparelLevelsDisplayToggle;
        public SettingHandle<bool> pawnCurJobDisplayToggle;

    }
}
