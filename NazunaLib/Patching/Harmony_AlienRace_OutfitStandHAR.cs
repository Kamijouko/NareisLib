using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace NareisLib
{
    [HarmonyPatch]
    internal static class AlienRace_OutfitStandHAR_PostSpawnSetup_Patch
    {
        private static MethodBase TargetMethod()
        {
            Type compType = AccessTools.TypeByName("AlienRace.Comp_OutfitStandHAR");
            return compType != null ? AccessTools.Method(compType, "PostSpawnSetup") : null;
        }

        private static bool Prefix(ThingComp __instance)
        {
            if (__instance?.parent is Building_NewOutfitStand)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch]
    internal static class AlienRace_OutfitStandHAR_CompGetGizmosExtra_Patch
    {
        private static MethodBase TargetMethod()
        {
            Type compType = AccessTools.TypeByName("AlienRace.Comp_OutfitStandHAR");
            return compType != null ? AccessTools.Method(compType, "CompGetGizmosExtra") : null;
        }

        private static bool Prefix(ThingComp __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance?.parent is Building_NewOutfitStand)
            {
                __result = Enumerable.Empty<Gizmo>();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch]
    internal static class AlienRace_OutfitStandHAR_PostExposeData_Patch
    {
        private static MethodBase TargetMethod()
        {
            Type compType = AccessTools.TypeByName("AlienRace.Comp_OutfitStandHAR");
            return compType != null ? AccessTools.Method(compType, "PostExposeData") : null;
        }

        private static bool Prefix(ThingComp __instance)
        {
            if (__instance?.parent is Building_NewOutfitStand)
            {
                return false;
            }
            return true;
        }
    }
}
