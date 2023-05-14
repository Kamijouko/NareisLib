using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using AlienRace;
using HugsLib;
using HarmonyLib;
using UnityEngine;
using System.Reflection.Emit;
using Verse.AI.Group;
using System.Reflection;
using Verse.AI;
using RimWorld.Planet;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.UIElements.Experimental;

namespace NazunaLib
{
    [StaticConstructorOnStartup]
    public class HarmonyMain
    {
        static HarmonyMain()
        {
            var harmonyInstance = new Harmony("NazunaReiLib.kamijouko");

            //harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawPawnBody", null, null), null, null, new HarmonyMethod(typeof(DAL_PawnAndApparelPatch), "DrawBodyPatchTranspiler", null), null);

            //harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawBodyApparel", null, null), null, null, new HarmonyMethod(typeof(DAL_PawnAndApparelPatch), "DrawApparelPatchTranspiler", null), null);

            //harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", null, null), null, null, new HarmonyMethod(typeof(DAL_PawnAndApparelPatch), "DrawHeadPatchTranspiler", null), null);

            //harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawHeadHair", null, null), new HarmonyMethod(typeof(DAL_PawnAndApparelPatch), "DrawHairPatchPrefix", null), null, null, null);         

            
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics", null, null), null, null, null, new HarmonyMethod(typeof(PawnRenderPatchs), "ResolveAllGraphicsFinalizer", null));
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics", null, null), null, new HarmonyMethod(typeof(PawnRenderPatchs), "ResolveHairGraphicsPostfix", null), null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics", null, null), null, new HarmonyMethod(typeof(PawnRenderPatchs), "ResolveApparelGraphicsPostfix", null), null, null);

            harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawPawnBody", null, null), new HarmonyMethod(typeof(PawnRenderPatchs), "DrawPawnBodyPrefix", null), null, null, null);

            //harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }


    //[已停用]Pawn服装替换
    //[HarmonyPatch(typeof(PawnGraphicSet))]
    //[HarmonyPatch("ResolveApparelGraphics")]
    public class DAL_BodyApparelHarmonyPatch
    {
        /*[HarmonyAfter(new string[] { "rimworld.erdelf.alien_race.main" })]
        [HarmonyPriority(390)]
        static void Postfix(PawnGraphicSet __instance)
        {
            List<DAL_ReplaceApparelProperties> list = null;
            DAL_ApparelForDifferentRace apparelForDifferentRace = null;

            Nazuna_Pawn dalPawn = __instance.pawn as Nazuna_Pawn;
            DAL_AlienRaceThingDef raceDef = __instance.pawn.def as DAL_AlienRaceThingDef;

            if (raceDef != null)
            {
                if (raceDef.replaceApparelGraphicPaths != null)
                {
                    list = raceDef.replaceApparelGraphicPaths.replacePropertiesList;
                }
            }

            int count = __instance.apparelGraphics.Count();
            for (int i = 0; i < count; i++)
            {
                ApparelGraphicRecord item;

                DAL_ApparelThingDef apparelDef = __instance.apparelGraphics[i].sourceApparel.def as DAL_ApparelThingDef;
                if (apparelDef != null)
                {
                    Apparel apparelSource = __instance.apparelGraphics[i].sourceApparel;
                    if (apparelDef.differentRaceGroup != null)
                    {
                        apparelForDifferentRace = apparelDef.differentRaceGroup.FirstOrDefault(delegate (DAL_ApparelForDifferentRace adr)
                        {
                            List<string> defNameList = adr.raceDefNameList;
                            return defNameList.Contains(__instance.pawn.def.defName);
                        });
                    }
                    if (apparelForDifferentRace == null)
                    {
                        if (apparelDef.breastSizeOn)
                        {
                            if (dalPawn != null && dalPawn.genderHasBeFixed)
                            {
                                if (raceDef != null && !raceDef.breastSizeList.NullOrEmpty())
                                {
                                    apparelSource.TryGetDALGraphicApparel(ApparelBodyTypeFromBreastSize(raceDef.breastSizeList, dalPawn.breastSize), out item, null, dalPawn.breastSize, apparelDef.defaultWornApparelShader.Shader, null, null);
                                    __instance.apparelGraphics[i] = item;
                                }
                                else
                                {
                                    if (apparelDef.useGenderWornGraphic)
                                    {
                                        apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, __instance.pawn.gender.ToString(), null, apparelDef.defaultWornApparelShader.Shader, null, null);
                                        __instance.apparelGraphics[i] = item;
                                    }
                                    else
                                    {
                                        apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, null, null, apparelDef.defaultWornApparelShader.Shader, null, null);
                                        __instance.apparelGraphics[i] = item;
                                    }
                                }
                            }
                            else
                            {
                                if (apparelDef.useGenderWornGraphic)
                                {
                                    apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, __instance.pawn.gender.ToString(), null, apparelDef.defaultWornApparelShader.Shader, null, null);
                                    __instance.apparelGraphics[i] = item;
                                }
                                else
                                {
                                    apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, null, null, apparelDef.defaultWornApparelShader.Shader, null, null);
                                    __instance.apparelGraphics[i] = item;
                                }
                            }

                        }
                        else
                        {
                            if (apparelDef.useGenderWornGraphic)
                            {
                                apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, __instance.pawn.gender.ToString(), null, apparelDef.defaultWornApparelShader.Shader, null, null);
                                __instance.apparelGraphics[i] = item;
                            }
                            else
                            {
                                apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, null, null, apparelDef.defaultWornApparelShader.Shader, null, null);
                                __instance.apparelGraphics[i] = item;
                            }
                        }
                    }
                    else
                    {
                        if (apparelForDifferentRace.breastSizeOn)
                        {
                            if (dalPawn != null && dalPawn.genderHasBeFixed)
                            {
                                if (raceDef != null && !raceDef.breastSizeList.NullOrEmpty())
                                {
                                    apparelSource.TryGetDALGraphicApparel(ApparelBodyTypeFromBreastSize(raceDef.breastSizeList, dalPawn.breastSize), out item, null, dalPawn.breastSize, apparelForDifferentRace.defaultReplaceshader.Shader, apparelForDifferentRace, null);
                                    __instance.apparelGraphics[i] = item;
                                }
                                else
                                {
                                    if (apparelForDifferentRace.useGenderWornGraphic)
                                    {
                                        apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, __instance.pawn.gender.ToString(), null, apparelForDifferentRace.defaultReplaceshader.Shader, apparelForDifferentRace, null);
                                        __instance.apparelGraphics[i] = item;
                                    }
                                    else
                                    {
                                        apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, null, null, apparelForDifferentRace.defaultReplaceshader.Shader, apparelForDifferentRace, null);
                                        __instance.apparelGraphics[i] = item;
                                    }
                                }
                            }
                            else
                            {
                                if (apparelForDifferentRace.useGenderWornGraphic)
                                {
                                    apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, __instance.pawn.gender.ToString(), null, apparelForDifferentRace.defaultReplaceshader.Shader, apparelForDifferentRace, null);
                                    __instance.apparelGraphics[i] = item;
                                }
                                else
                                {
                                    apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, null, null, apparelForDifferentRace.defaultReplaceshader.Shader, apparelForDifferentRace, null);
                                    __instance.apparelGraphics[i] = item;
                                }

                            }
                        }
                        else
                        {
                            if (apparelForDifferentRace.useGenderWornGraphic)
                            {
                                apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, __instance.pawn.gender.ToString(), null, apparelForDifferentRace.defaultReplaceshader.Shader, apparelForDifferentRace, null);
                                __instance.apparelGraphics[i] = item;
                            }
                            else
                            {
                                apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, null, null, apparelForDifferentRace.defaultReplaceshader.Shader, apparelForDifferentRace, null);
                                __instance.apparelGraphics[i] = item;
                            }
                        }
                    }
                }
                if (list != null)
                {
                    Apparel apparelSource = __instance.apparelGraphics[i].sourceApparel;
                    DAL_ReplaceApparelProperties replaceProperties = list.FirstOrDefault(delegate (DAL_ReplaceApparelProperties rp)
                    {
                        List<string> defNameList = rp.replaceDefName;
                        return defNameList.Contains(apparelSource.def.defName);
                    });
                    if (replaceProperties != null)
                    {
                        if (raceDef.breastSizeList.NullOrEmpty())
                        {
                            if (replaceProperties.useGenderWornGraphic)
                            {
                                apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, __instance.pawn.gender.ToString(), null, replaceProperties.defaultWornShader.Shader, null, replaceProperties);
                                __instance.apparelGraphics[i] = item;

                            }
                            else
                            {
                                apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, null, null, replaceProperties.defaultWornShader.Shader, null, replaceProperties);
                                __instance.apparelGraphics[i] = item;
                            }
                        }
                        else
                        {
                            if (dalPawn != null && dalPawn.genderHasBeFixed)
                            {
                                if (raceDef.replaceApparelGraphicPaths.allOfBreastSizeOn)
                                {
                                    apparelSource.TryGetDALGraphicApparel(ApparelBodyTypeFromBreastSize(raceDef.breastSizeList, dalPawn.breastSize), out item, null, dalPawn.breastSize, replaceProperties.defaultWornShader.Shader, null, replaceProperties);
                                    __instance.apparelGraphics[i] = item;
                                }
                                else
                                {
                                    if (replaceProperties.thisBreastSizeOn)
                                    {
                                        apparelSource.TryGetDALGraphicApparel(ApparelBodyTypeFromBreastSize(raceDef.breastSizeList, dalPawn.breastSize), out item, null, dalPawn.breastSize, replaceProperties.defaultWornShader.Shader, null, replaceProperties);
                                        __instance.apparelGraphics[i] = item;
                                    }
                                    else
                                    {
                                        if (replaceProperties.useGenderWornGraphic)
                                        {
                                            apparelSource.TryGetDALGraphicApparel(dalPawn.story.bodyType, out item, dalPawn.gender.ToString(), null, replaceProperties.defaultWornShader.Shader, null, replaceProperties);
                                            __instance.apparelGraphics[i] = item;
                                        }
                                        else
                                        {
                                            apparelSource.TryGetDALGraphicApparel(dalPawn.story.bodyType, out item, null, null, replaceProperties.defaultWornShader.Shader, null, replaceProperties);
                                            __instance.apparelGraphics[i] = item;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (replaceProperties.useGenderWornGraphic)
                                {
                                    apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, __instance.pawn.gender.ToString(), null, replaceProperties.defaultWornShader.Shader, null, replaceProperties);
                                    __instance.apparelGraphics[i] = item;
                                }
                                else
                                {
                                    apparelSource.TryGetDALGraphicApparel(__instance.pawn.story.bodyType, out item, null, null, replaceProperties.defaultWornShader.Shader, null, replaceProperties);
                                    __instance.apparelGraphics[i] = item;
                                }
                            }
                        }
                    }
                }
            }
            
        }*/

        //返回Pawn胸部类型
        private static BodyTypeDef ApparelBodyTypeFromBreastSize(List<DAL_PawnBreastInfo> list, string breastSize)
        {
            if (list != null && breastSize != null)
            {
                int count = list.Count();
                for (int i = 0; i < count; i++)
                {
                    string size = list[i].breastSize;
                    if (size == breastSize)
                    {
                        return list[i].apparelDisplayBodyType;
                    }
                }
            }
            return null;
        }
    }





    //[已停用]Pawn身体类型修正
    //[HarmonyPatch(typeof(PawnGraphicSet))]
    //[HarmonyPatch("ResolveAllGraphics")]
    public class DAL_BodyHarmonyPatch
    {
        /*[HarmonyAfter(new string[] { "rimworld.erdelf.alien_race.main" })]
        [HarmonyPriority(380)]
        static void Postfix(PawnGraphicSet __instance)
        {
            Nazuna_Pawn pawn = __instance.pawn as Nazuna_Pawn;
            if (pawn != null)
            {
                DAL_AlienRaceThingDef racedef = pawn.def as DAL_AlienRaceThingDef;
                if (racedef != null)
                {
                    if (!(__instance.pawn as Nazuna_Pawn).genderHasBeFixed)
                    {
                        Traverse story = Traverse.Create(__instance.pawn.story);
                        if (racedef.isFemaleRace)
                        {
                            __instance.pawn.gender = Gender.Female;

                            (__instance.pawn as Nazuna_Pawn).breastSize = BreastRandom(racedef.breastSizeList);
                        }
                        else
                        {
                            if (pawn.gender == Gender.Female)
                            {
                                (__instance.pawn as Nazuna_Pawn).breastSize = BreastRandom(racedef.breastSizeList);
                            }
                            else
                            {
                                (__instance.pawn as Nazuna_Pawn).breastSize = "None";
                                if (__instance.pawn.story.hairDef.styleGender != StyleGender.Male && __instance.pawn.story.hairDef.styleGender != StyleGender.MaleUsually && __instance.pawn.story.hairDef.styleGender != StyleGender.Any)
                                {
                                    story.Field("hairDef").SetValue((!racedef.alienRace.styleSettings.NullOrEmpty() && racedef.alienRace.styleSettings[typeof(HairDef)].hasStyle) ? racedef.alienRace.styleSettings[typeof(HairDef)].styleTags.RandomElement().GetRandomHairDefFromHairTag("Male") : null);
                                }
                            }
                        }
                        GraphicPaths currentGraphicPath = racedef.dalGraphicPaths.GetDALCurrentGraphicPath(pawn.breastSize, pawn.ageTracker.CurLifeStage) ?? racedef.alienRace.graphicPaths.GetDALCurrentGraphicPath(pawn.ageTracker.CurLifeStage);
                        story.Field("headGraphicPath").SetValue(currentGraphicPath.head.NullOrEmpty() ? "" : racedef.alienRace.generalSettings.alienPartGenerator.RandomAlienHead(currentGraphicPath.head, __instance.pawn));

                        BodyTypeDef currentBodyTypeDef = racedef.alienRace.generalSettings.alienPartGenerator.alienbodytypes.FirstOrDefault(btd => btd.defName == BodyTypeFromBreastSize(racedef.bodyTypeGroup, pawn.breastSize));
                        story.Field("bodyType").SetValue(currentBodyTypeDef ?? racedef.alienRace.generalSettings.alienPartGenerator.alienbodytypes.First());

                        (__instance.pawn as Nazuna_Pawn).genderHasBeFixed = true;
                    }

                    if (true)
                    {
                        AlienPartGenerator.AlienComp comp = __instance.pawn.GetComp<AlienPartGenerator.AlienComp>();
                        GraphicPaths currentGraphicPath = (racedef.dalGraphicPaths.GetDALCurrentGraphicPath(pawn.breastSize, pawn.ageTracker.CurLifeStage) ?? racedef.alienRace.graphicPaths.GetDALCurrentGraphicPath(pawn.ageTracker.CurLifeStage));
                        string maskPath = currentGraphicPath.bodyMasks.NullOrEmpty() ? string.Empty : (currentGraphicPath.bodyMasks + ((((comp.bodyMaskVariant >= 0) ? comp.bodyMaskVariant : (comp.bodyMaskVariant = Rand.Range(0, currentGraphicPath.BodyMaskCount))) > 0) ? comp.bodyMaskVariant.ToString() : string.Empty));
                        __instance.nakedGraphic = (!currentGraphicPath.body.NullOrEmpty() ? racedef.alienRace.generalSettings.alienPartGenerator.GetNakedGraphic(pawn.story.bodyType, (ContentFinder<Texture2D>.Get(AlienPartGenerator.GetNakedPath(pawn.story.bodyType, currentGraphicPath.body, racedef.alienRace.generalSettings.alienPartGenerator.useGenderedBodies ? pawn.gender.ToString() : "") + "_northm", false) == null) ? ShaderDatabase.Cutout : ShaderDatabase.CutoutComplex, __instance.pawn.story.SkinColor, racedef.alienRace.generalSettings.alienPartGenerator.SkinColor(pawn, false), currentGraphicPath.body, pawn.gender.ToString(), maskPath) : null);
                    }

                    if (__instance.pawn.GetComp<DAL_CompAnimated>() == null)
                    {
                    }
                    else
                    {
                        DAL_CompAnimated comp = (__instance.pawn as Nazuna_Pawn).GetComp<DAL_CompAnimated>();
                        if ((__instance.pawn as Nazuna_Pawn).genderHasBeFixed)
                        {
                            comp.Fixed = true;
                        }
                    }

                    __instance.ResolveApparelGraphics();
                }
                else
                {

                    __instance.ResolveApparelGraphics();
                }
            }
            else
            {
                DAL_CompAnimated comp = __instance.pawn.GetComp<DAL_CompAnimated>();
                if (comp != null)
                {
                    comp.Fixed = true;
                }
            }
        }*/

        private static string BreastRandom(List<DAL_PawnBreastInfo> breastSizeList)
        {
            if (breastSizeList != null)
            {
                float weights = 0;
                float num = 0;
                float value = Rand.Value;
                int count = breastSizeList.Count();
                for (int i = 0; i < count; i++)
                {
                    weights += breastSizeList[i].weight;
                }
                for (int n = 0; n < count - 1; n++)
                {
                    num += breastSizeList[n].weight;
                    if (value < num / weights)
                    {
                        return breastSizeList[n].breastSize;
                    }
                }
                return breastSizeList[count - 1].breastSize;
            }
            return null;
        }

        private static string BodyTypeFromBreastSize(List<DAL_BodyTypeLimitThreshold> list, string breastSize)
        {
            if (list != null && breastSize != null)
            {
                List<string> list2 = new List<string>();
                int count = list.Count();
                for (int i = 0; i < count; i++)
                {
                    if (list[i].breastSizeList.Contains(breastSize))
                    {
                        list2.Add(list[i].bodyType.defName);
                    }
                }
                return list2.RandomElement();
            }
            return null;
        }
    }






    //[已停用]无动画底发修正
    //[HarmonyPatch(typeof(PawnRenderer))]
    //[HarmonyPatch("DrawHeadHair")]
    public class DAL_HairPatch
    {
        /*static bool Prefix(PawnRenderer __instance, Vector3 rootLoc, float angle, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, ref Pawn ___pawn)
        {
            DAL_CompHeadTurning compHeadTurning = __instance.graphics.pawn.GetComp<DAL_CompHeadTurning>();
            Rot4 headFacing = (compHeadTurning != null && !flags.FlagSet(PawnRenderFlags.Portrait) && compHeadTurning.Props.enable && compHeadTurning.init) ? compHeadTurning.headFacing : bodyFacing;
            Vector3 headOffset = Vector3.zero;
            //Messages.Message("1", null, MessageTypeDefOf.ThreatBig, false);

            //画正常头发的底发
            if (__instance.graphics?.headGraphic != null)
            {
                Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
                List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                headOffset = quaternion * __instance.BaseHeadOffsetAt(headFacing);
                bool hasSurFaceApparel = false;
                if (!flags.FlagSet(PawnRenderFlags.Portrait) || !Prefs.HatsOnlyOnMap)
                {
                    for (int i = 0; i < apparelGraphics.Count; i++)
                    {
                        if (apparelGraphics[i].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparelGraphics[i].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover)
                        {
                            if (!apparelGraphics[i].sourceApparel.def.apparel.hatRenderedFrontOfFace && !apparelGraphics[i].sourceApparel.def.apparel.forceRenderUnderHair)
                            {
                                hasSurFaceApparel = true;
                            }
                        }
                    }
                }
                if (!hasSurFaceApparel && bodyDrawType != RotDrawMode.Dessicated && !flags.FlagSet(PawnRenderFlags.HeadStump))
                {
                    string defname = __instance.graphics.pawn.story.hairDef.defName;
                    DAL_BaseHairDef baseHair = DefDatabase<DAL_BaseHairDef>.AllDefsListForReading.FirstOrDefault(x => x.hairDefName == defname);
                    if (baseHair == null)
                    {
                        return true;
                    }
                    Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);

                    Graphic baseHairGraphic = GraphicDatabase.Get<Graphic_Multi>(baseHair.baseTexPath, ShaderDatabase.Transparent, Vector2.one, __instance.graphics.pawn.story.HairColor);
                    Material baseMat = baseHairGraphic.MatAt(headFacing, null);
                    if (!flags.FlagSet(PawnRenderFlags.Portrait) && __instance.graphics.pawn.IsInvisible())
                    {
                        baseMat = InvisibilityMatPool.GetInvisibleMat(baseMat);
                    }
                    else
                    {
                        baseMat = __instance.graphics.flasher.GetDamagedMat(baseMat);
                    }

                    Vector3 loc2 = rootLoc + headOffset;
                    loc2.y += 0.001f;
                    GenDraw.DrawMeshNowOrLater(mesh3, loc2, quaternion, baseMat, flags.FlagSet(PawnRenderFlags.DrawNow));

                }
            }
            return true;
        }*/
    }






    //初始化后加载资源全靠它！！！（已暂时停用）
    //[HarmonyPatch(typeof(UIRoot_Entry))]
    //[HarmonyPatch("DoMainMenu")]
    public class InitialModPatch
    {
        /*static bool Prefix(UIRoot_Entry __instance)
        {
            if (!ModStaticMethod.AllLevelsLoaded)
            {
                LoadAndResolveAllPlanDefs();
                ModStaticMethod.AllLevelsLoaded = true;
            }
            return true;
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
                Dictionary<string, MultiTexDef> data = new Dictionary<string, MultiTexDef>();
                foreach (MultiTexDef def in plan.plans)
                {
                    if (def.levels.NullOrEmpty() || data.ContainsKey(def.originalDef))
                        continue;
                    foreach (TextureLevels level in def.levels)
                    {
                        level.GetAllGraphicDatas(def.path);
                    }
                    data[def.originalDef] = def;
                }
                ThisModData.DefAndKeyDatabase[planDef] = data;
            }
        }*/
    }



    public class PawnRenderPatchs
    {
        //子方法组
        private static bool ShellFullyCoversHead(PawnRenderFlags flags, PawnRenderer render)
        {
            if (!flags.FlagSet(PawnRenderFlags.Clothes))
            {
                return false;
            }

            List<ApparelGraphicRecord> apparelGraphics = render.graphics.apparelGraphics;
            for (int index = 0; index < apparelGraphics.Count; index++)
            {
                if (apparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell && apparelGraphics[index].sourceApparel.def.apparel.shellCoversHead)
                {
                    return true;
                }

            }
            return false;
        }

        private static Vector3 OffsetBeardLocationForCrownType(HeadTypeDef headType, Rot4 headFacing, Vector3 beardLoc, Pawn pawn)
        {
            if (pawn.story.headType.defName == "Male_NarrowNormal" && pawn.story.headType.defName == "Female_NarrowNormal")
            {
                if (headFacing == Rot4.East)
                    beardLoc += Vector3.right * -0.05f;
                else if (headFacing == Rot4.West)
                    beardLoc += Vector3.right * 0.05f;
                beardLoc += Vector3.forward * -0.05f;
            }
            return beardLoc;
        }

        private static Material OverrideMaterialIfNeeded(Material original, Pawn pawn, PawnRenderer render, bool portrait = false)
        {
            Material baseMat = (!portrait && pawn.IsInvisible()) ? InvisibilityMatPool.GetInvisibleMat(original) : original;
            return render.graphics.flasher.GetDamagedMat(baseMat);
        }

        private static Material HairMatAt(Rot4 facing, Graphic hairGraphic, PawnGraphicSet pgs, bool portrait = false, bool cached = false)
        {
            if (hairGraphic == null)
            {
                return null;
            }
            Material material = hairGraphic.MatAt(facing, null);
            if (!portrait && pgs.pawn.IsInvisible())
            {
                material = InvisibilityMatPool.GetInvisibleMat(material);
            }
            if (!cached)
            {
                return pgs.flasher.GetDamagedMat(material);
            }
            return material;
        }

        /*public static Mesh GetPawnHairMesh(PawnRenderFlags renderFlags, Pawn pawn, Rot4 headFacing, PawnGraphicSet graphics, DAL_GraphicInfo g = null)
        {
            AlienPartGenerator.AlienComp comp = pawn.GetComp<AlienPartGenerator.AlienComp>();
            if (comp == null)
            {
                return graphics.HairMeshSet.MeshAt(headFacing);
            }
            if (g != null && g.meshSize != -1f)
            {
                return new GraphicMeshSet(1.5f * g.meshSize).MeshAt(headFacing);
            }
            return (renderFlags.FlagSet(PawnRenderFlags.Portrait) ? comp.alienPortraitHeadGraphics.hairSetAverage : comp.alienHeadGraphics.hairSetAverage).MeshAt(headFacing);
        }*/

        

        //[已停用]将根据层设定的渲染位置将keyName分配到Comp的列表里存储
        public static void SwitchTexToComp(TextureRenderLayer layer, string key, ref MultiRenderComp comp)
        {
            /*switch (layer)
            {
                case TextureRenderLayer.BehindTheBottomHair: comp.BehindTheBottomHair.Add(key); break;
                case TextureRenderLayer.BottomHair: comp.BottomHair.Add(key); break;

                case TextureRenderLayer.BehindTheShell: comp.BehindTheShell.Add(key); break;
                case TextureRenderLayer.Shell: comp.Shell.Add(key); break;
                case TextureRenderLayer.FrontOfShell: comp.FrontOfShell.Add(key); break;

                case TextureRenderLayer.BehindTheBody: comp.BehindTheBody.Add(key); break;
                case TextureRenderLayer.Body: comp.Body.Add(key); break;
                case TextureRenderLayer.FrontOfBody: comp.FrontOfBody.Add(key); break;

                case TextureRenderLayer.BehindTheApparel: comp.BehindTheApparel.Add(key); break;
                case TextureRenderLayer.Apparel: comp.Apparel.Add(key); break;
                case TextureRenderLayer.FrontOfApparel: comp.FrontOfApparel.Add(key); break;

                case TextureRenderLayer.BehindTheHead: comp.BehindTheHead.Add(key); break;
                case TextureRenderLayer.Head: comp.Head.Add(key); break;
                case TextureRenderLayer.FrontOfHead: comp.FrontOfHead.Add(key); break;

                case TextureRenderLayer.BehindTheMask: comp.BehindTheMask.Add(key); break;
                case TextureRenderLayer.Mask: comp.Mask.Add(key); break;
                case TextureRenderLayer.FrontOfMask: comp.FrontOfMask.Add(key); break;

                case TextureRenderLayer.BehindTheHair: comp.BehindTheHair.Add(key); break;
                case TextureRenderLayer.Hair: comp.Hair.Add(key); break;
                case TextureRenderLayer.FrontOfHair: comp.FrontOfHair.Add(key); break;

                case TextureRenderLayer.BehindTheHat: comp.BehindTheHat.Add(key); break;
                case TextureRenderLayer.Hat: comp.Hat.Add(key); break;
                case TextureRenderLayer.FrontOfHat: comp.FrontOfHat.Add(key); break;
            }*/
        }



        //给所有Pawn添加多层渲染Comp，没有CompTick所以不存在性能问题
        public static void AddComp(ref MultiRenderComp comp, ref Pawn pawn)
        {
            comp = (MultiRenderComp)Activator.CreateInstance(typeof(MultiRenderComp));
            comp.parent = pawn;
            Traverse p = Traverse.Create(pawn);
            List<ThingComp> list = (List<ThingComp>)p.Field("comps").GetValue();
            list.Add(comp);
            p.Field("comps").SetValue(list);
            comp.Initialize(new MultiRenderCompProperties());
        }


        //处理defName所指定的MultiTexDef，
        //对其属性levels里所存储的所有TextureLevels都根据指定的权重随机一个贴图的名称，
        //并将名称记录进一个从其属性cacheOfLevels得来的MultiTexEpoch中所对应渲染图层的MultiTexBatch的名称列表里，
        //最终返回这个MultiTexEpoch
        public static MultiTexEpoch ResolveMultiTexDef(MultiTexDef def, out Dictionary<string, TextureLevels> data, bool isOwnerable = false)
        {
            //Log.Warning(plan+","+defName);
            MultiTexEpoch epoch = def.cacheOfLevels;
            data = new Dictionary<string, TextureLevels>();
            foreach (TextureLevels level in def.levels)
            {
                string pre = level.prefix.RandomElementByWeight(x => level.preFixWeights[x]);
                string keyName = level.preFixToTexName[pre].RandomElementByWeight(x => level.texWeights[x]);

                if (!epoch.batches.Exists(x => x.keyName == keyName))
                    epoch.batches.Add(new MultiTexBatch(def.defName, keyName, level.renderLayer, new List<string>(), level.renderSwitch));
                epoch.batches.First(x => x.keyName == keyName).keyList.Add(keyName);

                level.keyName = keyName;
                if (level.patternSets != null)
                    level.patternSets.keyName = keyName;
                data[keyName] = level;
            }
            /*if (isOwnerable)
            {
                foreach (MultiTexBatch batch in epoch.batches)
                {
                    batch.keyListSouth = batch.keyList;
                    batch.keyListEast = batch.keyList;
                    batch.keyListNorth = batch.keyList;
                    batch.keyListWest = batch.keyList;
                    if (batch.keyList.Count() > 1)
                    {
                        batch.keyListSouth.Sort((i, j) => ThisModData.TexLevelsDatabase[i].drawOffsetSouth.Value.y.CompareTo(ThisModData.TexLevelsDatabase[j].drawOffsetSouth.Value.y));
                        batch.keyListEast.Sort((i, j) => ThisModData.TexLevelsDatabase[i].drawOffsetEast.Value.y.CompareTo(ThisModData.TexLevelsDatabase[j].drawOffsetEast.Value.y));
                        batch.keyListNorth.Sort((i, j) => ThisModData.TexLevelsDatabase[i].drawOffsetNorth.Value.y.CompareTo(ThisModData.TexLevelsDatabase[j].drawOffsetNorth.Value.y));
                        batch.keyListWest.Sort((i, j) => ThisModData.TexLevelsDatabase[i].drawOffsetWest.Value.y.CompareTo(ThisModData.TexLevelsDatabase[j].drawOffsetWest.Value.y));
                    }
                }
            }*/
            return epoch;
        }


        //从comp的storedData里获取TextureLevels数据，用于处理读取存档时从已有的storedData字典中得到的epoch
        public static Dictionary<string, TextureLevels> GetLevelsDictFromEpoch(MultiTexEpoch epoch)
        {
            return epoch.batches.ToDictionary(k => k.keyName, v => ResolveKeyNameForLevel(ThisModData.TexLevelsDatabase[v.keyName], v.keyName));
        }
        //上方法的子方法，为获取到的TextureLevels进行赋值操作
        public static TextureLevels ResolveKeyNameForLevel(TextureLevels level, string key)
        {
            level.keyName = key;
            if (level.patternSets != null)
                level.patternSets.keyName = key;
            return level;
        }



        [CompilerGenerated]
		[StructLayout(LayoutKind.Auto)]
		private struct AlienAddonClass
		{
			public Pawn pawn;
			public Rot4 rotation;
			public bool isPortrait;
			public bool isInvisible;
			public Vector3 vector;
			public Vector3 headOffset;
			public Quaternion quat;
			public PawnRenderFlags renderFlags;
		}


        //预处理Pawn身体的多层渲染数据，只会在pawn出现或生成时执行一次，在整体预处理方法中最后执行（因为原方法的顺序）
        static void ResolveAllGraphicsFinalizer(PawnGraphicSet __instance)
        {
            if (!ModStaticMethod.AllLevelsLoaded || ThisModData.DefAndKeyDatabase.NullOrEmpty())
                return;
            Pawn pawn = __instance.pawn;
            string race = pawn.def.defName;
            RenderPlanDef def = DefDatabase<RenderPlanDef>.AllDefs.FirstOrDefault(x => x.races.Contains(race));
            if (def == null)
                return;
            string plan = def.defName;
            
            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                AddComp(ref comp, ref pawn);

            List<string> cachedOverride = new List<string>();
            Dictionary<string, Dictionary<string, TextureLevels>> cachedGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();
            Dictionary<string, MultiTexEpoch> data = new Dictionary<string, MultiTexEpoch>();
            HeadTypeDef head = pawn.story.headType;
            if (head != null && ThisModData.DefAndKeyDatabase[plan].ContainsKey(head.defName))
            {
                string keyName = head.defName;
                MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][keyName];
                if (comp.storedDataBody.NullOrEmpty() || !comp.storedDataBody.ContainsKey(keyName))
                {
                    Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                    data[keyName] = ResolveMultiTexDef(multidef, out cachedData);
                    cachedGraphicData[keyName] = cachedData;
                }
                else
                {
                    data[keyName] = comp.storedDataBody[keyName];
                    cachedGraphicData[keyName] = GetLevelsDictFromEpoch(data[keyName]);
                }
                if (!multidef.renderOriginTex)
                    cachedOverride.Add("Head");
            }
            BodyTypeDef body = pawn.story.bodyType;
            if (body != null && ThisModData.DefAndKeyDatabase[plan].ContainsKey(body.defName))
            {
                string keyName = body.defName;
                MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][keyName];
                if (comp.storedDataBody.NullOrEmpty() || !comp.storedDataBody.ContainsKey(keyName))
                {
                    Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                    data[keyName] = ResolveMultiTexDef(multidef, out cachedData);
                    cachedGraphicData[keyName] = cachedData;
                }
                else
                {
                    data[keyName] = comp.storedDataBody[keyName];
                    cachedGraphicData[keyName] = GetLevelsDictFromEpoch(data[keyName]);
                }
                if (!multidef.renderOriginTex)
                    cachedOverride.Add("Body");
            }
            string hand = comp.Props.handDefName;
            if (hand != "" && ThisModData.DefAndKeyDatabase[plan].ContainsKey(hand))
            {
                MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][hand];
                if (comp.storedDataBody.NullOrEmpty() || !comp.storedDataBody.ContainsKey(hand))
                {
                    Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                    data[hand] = ResolveMultiTexDef(multidef, out cachedData);
                    cachedGraphicData[hand] = cachedData;
                }
                else
                {
                    data[hand] = comp.storedDataBody[hand];
                    cachedGraphicData[hand] = GetLevelsDictFromEpoch(data[hand]);
                }
            }
            comp.cachedOverrideBody = cachedOverride;
            comp.cachedBodyGraphicData = cachedGraphicData;
            comp.storedDataBody = data;
            comp.ResolveAllLayerBatch();
            comp.PrefixResolved = true;
        }

        //预处理Pawn的头发，在发型变换时会执行
        static void ResolveHairGraphicsPostfix(PawnGraphicSet __instance)
        {
            if (!ModStaticMethod.AllLevelsLoaded || ThisModData.DefAndKeyDatabase.NullOrEmpty())
                return;
            Pawn pawn = __instance.pawn;
            string race = pawn.def.defName;
            RenderPlanDef def = DefDatabase<RenderPlanDef>.AllDefs.FirstOrDefault(x => x.races.Contains(race));
            if (def == null)
                return;
            string plan = def.defName;
            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                AddComp(ref comp, ref pawn);

            List<string> cachedOverride = new List<string>();
            Dictionary<string, Dictionary<string, TextureLevels>> cachedGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();
            Dictionary<string, MultiTexEpoch> data = new Dictionary<string, MultiTexEpoch>();
            HairDef hair = pawn.story.hairDef;
            if (hair != null && ThisModData.DefAndKeyDatabase[plan].ContainsKey(hair.defName))
            {
                string keyName = hair.defName;
                MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][keyName];
                if (comp.storedDataHair.NullOrEmpty() || !comp.storedDataHair.ContainsKey(keyName))
                {
                    Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                    data[keyName] = ResolveMultiTexDef(multidef, out cachedData);
                    cachedGraphicData[keyName] = cachedData;
                }
                else
                {
                    data[keyName] = comp.storedDataHair[keyName];
                    cachedGraphicData[keyName] = GetLevelsDictFromEpoch(data[keyName]);
                }
                if (!multidef.renderOriginTex)
                    cachedOverride.Add("Hair");
            }
            comp.cachedOverrideHair = cachedOverride;
            comp.cachedHairGraphicData = cachedGraphicData;
            comp.storedDataHair = data;
            if (comp.PrefixResolved)
                comp.ResolveAllLayerBatch();
        }

        //预处理Pawn穿着的多层渲染服装，在服装变换时会执行
        static void ResolveApparelGraphicsPostfix(PawnGraphicSet __instance)
        {
            if (!ModStaticMethod.AllLevelsLoaded || ThisModData.DefAndKeyDatabase.NullOrEmpty())
                return;
            Pawn pawn = __instance.pawn;
            string race = pawn.def.defName;
            RenderPlanDef def = DefDatabase<RenderPlanDef>.AllDefs.FirstOrDefault(x => x.races.Contains(race));
            if (def == null)
                return;
            string plan = def.defName;
            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                AddComp(ref comp, ref pawn);

            List<string> cachedOverride = new List<string>();
            Dictionary<string, Dictionary<string, TextureLevels>> cachedGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();
            Dictionary<string, MultiTexEpoch> data = new Dictionary<string, MultiTexEpoch>();
            using (List<Apparel>.Enumerator enumerator = __instance.pawn.apparel.WornApparel.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (ThisModData.DefAndKeyDatabase[plan].ContainsKey(enumerator.Current.def.defName))
                    {
                        string keyName = enumerator.Current.def.defName;
                        MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][keyName];
                        if (comp.storedDataApparel.NullOrEmpty() || !comp.storedDataApparel.ContainsKey(keyName))
                        {
                            Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                            data[keyName] = ResolveMultiTexDef(multidef, out cachedData);
                            cachedGraphicData[keyName] = cachedData;
                        }
                        else
                        {
                            data[keyName] = comp.storedDataApparel[keyName];
                            cachedGraphicData[keyName] = GetLevelsDictFromEpoch(data[keyName]);
                        }
                        if (!multidef.renderOriginTex)
                            cachedOverride.Add(keyName);
                    }
                }
            }
            comp.cachedOverrideApparel = cachedOverride;
            comp.cachedApparelGraphicData = cachedGraphicData;
            comp.storedDataApparel = data;
            if (comp.PrefixResolved)
                comp.ResolveAllLayerBatch();
        }


        //BottomHair BottomShell Body Prefix
        static bool DrawPawnBodyPrefix(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, float angle, Rot4 facing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
        {
            //Log.Warning(___pawn.Name + " flags: DrawNow = " + flags.FlagSet(PawnRenderFlags.DrawNow).ToStringSafe());
            MultiRenderComp comp = ___pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return true;
            if (!comp.PrefixResolved)
                __instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<string>> curDirection = comp.GetDataOfDirection(facing);
            if (curDirection.NullOrEmpty())
                return true;

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 vector = rootLoc;
            vector.y += 0.003f;
            Vector3 loc = vector;
            loc.y += 0.001f;
            Mesh bodyMesh = null;
            Mesh hairMesh = null;
            Mesh headMesh = null;
            if (___pawn.RaceProps.Humanlike)
            {
                bodyMesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(___pawn).MeshAt(facing);
                headMesh = HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(___pawn).MeshAt(facing);
                hairMesh = HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(___pawn).MeshAt(facing);/*__instance.graphics.HairMeshSet.MeshAt(facing);*/
            }
            else
                bodyMesh = __instance.graphics.nakedGraphic.MeshAt(facing);

            List<int> renderLayers = new List<int>() { (int)TextureRenderLayer.BottomHair, (int)TextureRenderLayer.BottomShell}; 
            
            foreach (int level in renderLayers)
            {
                if (curDirection.ContainsKey(level))
                {
                    foreach (string keyName in curDirection[level])
                    {
                        if (comp.cachedAllGraphicData[keyName] != null && comp.cachedAllGraphicData[keyName].CanRender(___pawn, keyName))
                        {
                            TextureLevels data = comp.cachedAllGraphicData[keyName];
                            Mesh mesh = null;
                            Vector3 offset = Vector3.zero;
                            if (___pawn.RaceProps.Humanlike)
                            {
                                switch (data.meshType)
                                {
                                    case "Body": 
                                        mesh = bodyMesh; 
                                        break;
                                    case "Head": 
                                        mesh = headMesh; 
                                        offset = quat * __instance.BaseHeadOffsetAt(facing);
                                        break;
                                    case "Hair": 
                                        mesh = hairMesh;
                                        offset = quat * __instance.BaseHeadOffsetAt(facing);
                                        break;
                                }
                            }
                            else
                                mesh = bodyMesh;
                            int pattern = 0;
                            if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(keyName))
                                pattern = comp.cachedRandomGraphicPattern[keyName];
                            Vector3 pos = vector + offset + data.DrawOffsetForRot(facing);
                            Material mat = data.GetGraphic(keyName, pattern).MatAt(facing, null);
                            GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }
                    }
                }
            }

            int layer = (int)TextureRenderLayer.Body;
            if (curDirection.ContainsKey(layer))
            {
                foreach (string keyName in curDirection[layer])
                {
                    TextureLevels data = comp.cachedAllGraphicData[keyName];
                    Mesh mesh = null;
                    Vector3 offset = Vector3.zero;
                    if (___pawn.RaceProps.Humanlike)
                    {
                        switch (data.meshType)
                        {
                            case "Body":
                                mesh = bodyMesh;
                                break;
                            case "Head":
                                mesh = headMesh;
                                offset = quat * __instance.BaseHeadOffsetAt(facing);
                                break;
                            case "Hair":
                                mesh = hairMesh;
                                offset = quat * __instance.BaseHeadOffsetAt(facing);
                                break;
                        }
                    }
                    else
                        mesh = bodyMesh;
                    int pattern = 0;
                    if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(keyName))
                        pattern = comp.cachedRandomGraphicPattern[keyName];
                    string condition = "";
                    if (bodyDrawType == RotDrawMode.Rotting && data.hasRotting)
                        condition = "Rotting";
                    if (bodyDrawType == RotDrawMode.Dessicated && data.hasDessicated)
                        condition = "Dessicated";
                    Vector3 pos = vector + offset + data.DrawOffsetForRot(facing);
                    Material mat = data.GetGraphic(keyName, pattern, condition).MatAt(facing, null);
                    GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                }
                if (!comp.GetAllOverrideData.NullOrEmpty() && comp.GetAllOverrideData.Contains("Body"))
                {
                    if (ModsConfig.IdeologyActive && __instance.graphics.bodyTattooGraphic != null && bodyDrawType != RotDrawMode.Dessicated && (facing != Rot4.North || ___pawn.style.BodyTattoo.visibleNorth))
                    {
                        GenDraw.DrawMeshNowOrLater(__instance.GetBodyOverlayMeshSet().MeshAt(facing), loc, quat, __instance.graphics.bodyTattooGraphic.MatAt(facing, null), flags.FlagSet(PawnRenderFlags.DrawNow));
                    }
                    return false;
                }  
            }

            return true;
        }

        static void DrawPawnBodyFinalizer(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, float angle, Rot4 facing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
        {

        }













        //备用随机算法
        public static string GetRandom(System.Random rand, Dictionary<string, int> list)
        {
            int i = rand.Next(list.Values.Max() + 1);
            List<string> result = list.Keys.Where(x => list[x] >= i).ToList();
            return result.RandomElement();
        }

        //[已停用]头发渲染修正（包括头部装备），使用prefix
        static bool DrawHairPatchPrefix(PawnRenderer __instance, Vector3 rootLoc, float angle, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, ref Pawn ___pawn)
        {
            DAL_CompAnimated compAnimated = ___pawn.GetComp<DAL_CompAnimated>();
            DAL_CompHeadTurning compHeadTurning = ___pawn.GetComp<DAL_CompHeadTurning>();
            bool isSleepOrLayDownRest = (___pawn.jobs?.curDriver != null && (___pawn.jobs?.curDriver.asleep == true || ___pawn.jobs?.curJob.def == JobDefOf.LayDownResting || ___pawn.jobs.curJob?.def == JobDefOf.LayDown || ___pawn.jobs.curJob?.def.defName == "Skygaze"));
            if (compAnimated?.hairGraphicList.NullOrEmpty() == false || __instance.graphics.apparelGraphics?.Exists(x => x.sourceApparel.GetComp<DAL_CompAnimated>()?.apparelGraphicList.NullOrEmpty() == false && (x.sourceApparel.def.apparel.hatRenderedFrontOfFace || ((x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) && !x.sourceApparel.def.apparel.forceRenderUnderHair))) == true || compHeadTurning != null)
            {
                //头部转向图像部分修补（头部图像）
                Rot4 headFacing = (compHeadTurning != null && !flags.FlagSet(PawnRenderFlags.Portrait) && !flags.FlagSet(PawnRenderFlags.HeadStump) && compHeadTurning.Props.enable && compHeadTurning.init && !isSleepOrLayDownRest) ? compHeadTurning.headFacing : bodyFacing;
                Vector3 headOffset = Vector3.zero;
                //Messages.Message("1", null, MessageTypeDefOf.ThreatBig, false);b

                //__instance.graphics.ClearCache();
                if (ShellFullyCoversHead(flags, __instance))
                {
                    //return true;
                }

                List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
                headOffset = quat * __instance.BaseHeadOffsetAt(headFacing);
                Vector3 onHeadLoc = rootLoc + headOffset;
                onHeadLoc.y += 0.02895753f;
                //bool hasSurFaceApparel = false;
                bool notRendBeard = headFacing == Rot4.North || __instance.graphics.pawn.style == null || __instance.graphics.pawn.style.beardDef == BeardDefOf.NoBeard;
                bool hasHeadApparel = flags.FlagSet(PawnRenderFlags.Headgear) && (!flags.FlagSet(PawnRenderFlags.Portrait) || !Prefs.HatsOnlyOnMap || flags.FlagSet(PawnRenderFlags.StylingStation));
                if (hasHeadApparel)
                {
                    for (int index = 0; index < apparelGraphics.Count; ++index)
                    {
                        if (apparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover)
                        {
                            if (apparelGraphics[index].sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
                            {
                                notRendBeard = true;
                            }
                            if (!apparelGraphics[index].sourceApparel.def.apparel.hatRenderedFrontOfFace && !apparelGraphics[index].sourceApparel.def.apparel.forceRenderUnderHair)
                            {
                                //hasSurFaceApparel = true;
                            }
                        }
                    }
                }

                //画脸部纹身
                if (ModsConfig.IdeologyActive && __instance.graphics.faceTattooGraphic != null && (bodyDrawType != RotDrawMode.Dessicated && !flags.FlagSet(PawnRenderFlags.HeadStump)) && (bodyFacing != Rot4.North || __instance.graphics.pawn.style.FaceTattoo.visibleNorth))
                {
                    Vector3 loc = onHeadLoc;
                    loc.y -= 0.001447876f;
                    GenDraw.DrawMeshNowOrLater(__instance.graphics.HairMeshSet.MeshAt(headFacing), loc, quat, __instance.graphics.faceTattooGraphic.MatAt(headFacing, (Thing)null), flags.FlagSet(PawnRenderFlags.DrawNow));
                }

                //画胡子
                if (!notRendBeard && bodyDrawType != RotDrawMode.Dessicated && (!flags.FlagSet(PawnRenderFlags.HeadStump) && __instance.graphics.pawn.style != null) && __instance.graphics.pawn.style.beardDef != null)
                {
                    Vector3 loc = OffsetBeardLocationForCrownType(__instance.graphics.pawn.story.headType, headFacing, onHeadLoc, __instance.graphics.pawn) + __instance.graphics.pawn.style.beardDef.GetOffset(__instance.graphics.pawn.story.headType, headFacing);
                    Mesh mesh = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                    Material mat = __instance.graphics.BeardMatAt(headFacing, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
                    if (mat != null)
                    {
                        GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }
                }

                //画在头后面的装备图层
                if (hasHeadApparel)
                {
                    for (int index = 0; index < apparelGraphics.Count; ++index)
                    {
                        if (apparelGraphics[index].sourceApparel.def.apparel.forceRenderUnderHair)
                        {
                            DrawApparel(apparelGraphics[index], headFacing);
                        }
                    }
                }

                //画头发
                if (/*!hasSurFaceApparel &&*/ bodyDrawType != RotDrawMode.Dessicated && !flags.FlagSet(PawnRenderFlags.HeadStump))
                {
                    if (compAnimated?.hairGraphicList.NullOrEmpty() == false)
                    {
                        //Mesh mesh = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                        Mesh mesh = null;
                        Material mat = null;
                        //画动画底发
                        if (compAnimated.hairGraphicList.Exists(x => x.isbaseHair))
                        {
                            compAnimated.hairGraphicList.Sort((x, y) => x.yOffSet.CompareTo(y.yOffSet));
                            foreach (DAL_GraphicInfo g in compAnimated.hairGraphicList)
                            {
                                if (g.isbaseHair)
                                {
                                    mesh = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                                    mat = HairMatAt(headFacing, g.graphic, __instance.graphics, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
                                    if (mat != null)
                                    {
                                        Vector3 baseHeadLoc = headOffset;
                                        baseHeadLoc.y = baseHeadLoc.y + g.yOffSet;
                                        GenDraw.DrawMeshNowOrLater(
                                            mesh, baseHeadLoc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                                    }
                                }
                            }
                        }

                        //画动画头发
                        compAnimated.hairGraphicList.Sort((x, y) => x.yOffSet.CompareTo(y.yOffSet));
                        foreach (DAL_GraphicInfo g in compAnimated.hairGraphicList)
                        {
                            if (!g.isbaseHair)
                            {
                                mesh = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                                mat = HairMatAt(headFacing, g.graphic, __instance.graphics, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
                                if (mat != null)
                                {
                                    Vector3 HeadLoc = onHeadLoc;
                                    HeadLoc.y = HeadLoc.y + g.yOffSet;
                                    GenDraw.DrawMeshNowOrLater(mesh, HeadLoc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                                }
                            }
                        }
                    }
                    else
                    {
                        //画原版头发的底发
                        string defname = __instance.graphics.pawn.story.hairDef.defName;
                        DAL_BaseHairDef baseHair = DefDatabase<DAL_BaseHairDef>.AllDefsListForReading.FirstOrDefault(x => x.hairDefName == defname);
                        if (baseHair != null)
                        {
                            Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);

                            Graphic baseHairGraphic = GraphicDatabase.Get<Graphic_Multi>(baseHair.baseTexPath, ShaderDatabase.Transparent, Vector2.one, __instance.graphics.pawn.story.HairColor);
                            Material baseMat = HairMatAt(headFacing, baseHairGraphic, __instance.graphics, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
                            if (!flags.FlagSet(PawnRenderFlags.Portrait) && __instance.graphics.pawn.IsInvisible())
                            {
                                baseMat = InvisibilityMatPool.GetInvisibleMat(baseMat);
                            }
                            else
                            {
                                baseMat = __instance.graphics.flasher.GetDamagedMat(baseMat);
                            }

                            Vector3 loc2 = rootLoc + headOffset;
                            loc2.y -= 0.001f;
                            GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, baseMat, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }

                        //画原版头发
                        Mesh mesh = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                        Material mat = HairMatAt(headFacing, __instance.graphics.hairGraphic, __instance.graphics, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
                        if (mat != null)
                        {
                            GenDraw.DrawMeshNowOrLater(mesh, onHeadLoc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }
                    }
                }


                if (!hasHeadApparel)
                {
                    return false;
                }

                //画在头前面的装备图层
                for (int index = 0; index < apparelGraphics.Count; index++)
                {
                    if ((apparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparelGraphics[index].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) && !apparelGraphics[index].sourceApparel.def.apparel.forceRenderUnderHair)
                    {
                        DrawApparel(apparelGraphics[index], headFacing);
                    }
                }
                return false;

                //画头部装备的方法
                void DrawApparel(ApparelGraphicRecord apparelRecord, Rot4 headfacing)
                {

                    Mesh mesh = null;
                    Material mat = null;
                    if (!apparelRecord.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                    {
                        if (apparelRecord.sourceApparel.GetComp<DAL_CompAnimated>()?.DALProps.apparelFrameList?.Exists(x => x.isApparel) == true)
                        {
                            DAL_CompAnimated comp = apparelRecord.sourceApparel.GetComp<DAL_CompAnimated>();
                            comp.apparelGraphicList.Sort((x, y) => x.yOffSet.CompareTo(y.yOffSet));
                            foreach (DAL_GraphicInfo g in comp.apparelGraphicList)
                            {
                                mesh = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                                mat = g.graphic.MatAt(headfacing, null);
                                Vector3 Loc = onHeadLoc;
                                Loc.y = Loc.y + g.yOffSet;
                                GenDraw.DrawMeshNowOrLater(mesh, Loc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                            }

                        }
                        else
                        {
                            Material original = apparelRecord.graphic.MatAt(headfacing, null);
                            mesh = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            mat = flags.FlagSet(PawnRenderFlags.Cache) ? original : OverrideMaterialIfNeeded(original, __instance.graphics.pawn, __instance, flags.FlagSet(PawnRenderFlags.Portrait));
                            GenDraw.DrawMeshNowOrLater(mesh, onHeadLoc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }
                    }
                    else
                    {
                        //Messages.Message("1", null, MessageTypeDefOf.ThreatBig, false);
                        Vector3 loc = rootLoc + headOffset;
                        if (apparelRecord.sourceApparel.def.apparel.hatRenderedBehindHead)
                        {
                            loc.y += 0.02216602f;
                        }
                        else
                        {
                            loc.y += !(bodyFacing == Rot4.North) || apparelRecord.sourceApparel.def.apparel.hatRenderedAboveBody ? 0.03185328f : 0.002895753f;
                        }
                        if (apparelRecord.sourceApparel.GetComp<DAL_CompAnimated>()?.apparelGraphicList.NullOrEmpty() == false)
                        {
                            DAL_CompAnimated comp = apparelRecord.sourceApparel.GetComp<DAL_CompAnimated>();
                            comp.apparelGraphicList.Sort((x, y) => x.yOffSet.CompareTo(y.yOffSet));
                            foreach (DAL_GraphicInfo g in comp.apparelGraphicList)
                            {
                                mesh = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                                mat = g.graphic.MatAt(bodyFacing, null);
                                Vector3 Loc = loc;
                                Loc.y = Loc.y + g.yOffSet;
                                GenDraw.DrawMeshNowOrLater(mesh, Loc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                            }
                        }
                        else
                        {
                            Material original = apparelRecord.graphic.MatAt(bodyFacing, (Thing)null);
                            mesh = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            mat = flags.FlagSet(PawnRenderFlags.Cache) ? original : OverrideMaterialIfNeeded(original, __instance.graphics.pawn, __instance, flags.FlagSet(PawnRenderFlags.Portrait));
                            GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }
                    }
                }
            }
            else
            {
                return true;
            }

        }

        



        //身体(包括穿在身体上的衣服)渲染修正，使用IL
        public static IEnumerable<CodeInstruction> DrawBodyPatchTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo matsBodyBaseAtInfo = AccessTools.Method(typeof(PawnGraphicSet), "MatsBodyBaseAt", null, null);
            List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
            int num;
            for (int i = 0; i < instructionList.Count; i = num + 1)
            {
                CodeInstruction instruction = instructionList[i];
                if (i > 10 && instructionList[i - 2].OperandIs(matsBodyBaseAtInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 3);//facing

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);//bodyDrawType

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);//flags

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);//bodyMesh
                    yield return new CodeInstruction(OpCodes.Ldind_Ref, null);

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 1);//vector

                    yield return new CodeInstruction(OpCodes.Ldloc_0, null);//quat

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 3);//refList

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 0);//render

                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderPatchs), "DrawBodyTranPatch", null, null));

                    yield return new CodeInstruction(OpCodes.Stloc_3, null);
                }
                yield return instruction;
                num = i;
            }
            yield break;
        }

        public static List<Material> DrawBodyTranPatch(Rot4 facing, RotDrawMode bodyDrawType, PawnRenderFlags flags, Mesh bodyMesh, Vector3 vector, Quaternion quat, List<Material> refList, PawnRenderer render)
        {
            bool hasBodyAnimeComp = false;
            bool hasApparelAnimeComp = false;
            if (bodyDrawType == RotDrawMode.Fresh)
            {
                if (render.graphics.apparelGraphics.Exists(x => x.sourceApparel.GetComp<DAL_CompAnimated>()?.DALProps.apparelFrameList?.Exists(y => y.isApparel) != null && (x.sourceApparel.def.apparel.shellRenderedBehindHead || x.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell) && !PawnRenderer.RenderAsPack(x.sourceApparel) && x.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && x.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover))
                {
                    hasApparelAnimeComp = true;
                }
                if (render.graphics.pawn.GetComp<DAL_CompAnimated>()?.DALProps.pawnFrameList?.Exists(x => x.isBody) == true)
                {
                    hasBodyAnimeComp = true;
                }
                if (hasBodyAnimeComp)
                {
                    DAL_CompAnimated comp = render.graphics.pawn.GetComp<DAL_CompAnimated>();
                    comp.nakedGraphicList.Sort((x, y) => x.yOffSet.CompareTo(y.yOffSet));
                    Mesh mesh = bodyMesh;
                    Material mat = null;
                    foreach (DAL_GraphicInfo g in comp.nakedGraphicList)
                    {
                        if (g.meshSize != -1f)
                        {
                            mesh = new GraphicMeshSet(1.5f * g.meshSize).MeshAt(facing);
                        }
                        mat = g.graphic.MatAt(facing, null);
                        Vector3 vector2 = vector;
                        vector2.y += g.yOffSet;
                        GenDraw.DrawMeshNowOrLater(mesh, vector, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }
                    //Messages.Message("1", null, MessageTypeDefOf.ThreatBig, false);
                }
                else if (hasApparelAnimeComp)
                {
                    Material mat = flags.FlagSet(PawnRenderFlags.Cache) ? render.graphics.nakedGraphic.MatAt(facing, null) : OverrideMaterialIfNeeded(render.graphics.nakedGraphic.MatAt(facing, null), render.graphics.pawn, render, flags.FlagSet(PawnRenderFlags.Portrait));
                    GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                }
                if (hasApparelAnimeComp)
                {
                    vector.y += 0.0028957527f;
                    for (int i = 0; i < render.graphics.apparelGraphics.Count; i++)
                    {
                        if ((render.graphics.apparelGraphics[i].sourceApparel.def.apparel.shellRenderedBehindHead || render.graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell) && !PawnRenderer.RenderAsPack(render.graphics.apparelGraphics[i].sourceApparel) && render.graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && render.graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover)
                        {
                            if (render.graphics.apparelGraphics[i].sourceApparel.GetComp<DAL_CompAnimated>()?.DALProps.apparelFrameList?.Exists(x => x.isApparel) != null)
                            {
                                DAL_CompAnimated comp2 = render.graphics.apparelGraphics[i].sourceApparel.GetComp<DAL_CompAnimated>();
                                comp2.apparelGraphicList.Sort((x, y) => x.yOffSet.CompareTo(y.yOffSet));
                                Mesh mesh = bodyMesh;
                                Material mat = null;
                                foreach (DAL_GraphicInfo g in comp2.apparelGraphicList)
                                {
                                    if (g.meshSize != -1f)
                                    {
                                        mesh = new GraphicMeshSet(1.5f * g.meshSize).MeshAt(facing);
                                    }
                                    mat = g.graphic.MatAt(facing, null);
                                    Vector3 vector2 = vector;
                                    vector2.y += g.yOffSet;
                                    GenDraw.DrawMeshNowOrLater(mesh, vector2, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                                }
                            }

                        }
                    }
                }
                else if (hasBodyAnimeComp)
                {
                    vector.y += 0.0028957527f;
                    for (int i = 0; i < render.graphics.apparelGraphics.Count; i++)
                    {
                        if ((render.graphics.apparelGraphics[i].sourceApparel.def.apparel.shellRenderedBehindHead || render.graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell) && !PawnRenderer.RenderAsPack(render.graphics.apparelGraphics[i].sourceApparel) && render.graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && render.graphics.apparelGraphics[i].sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover)
                        {
                            Material mat = flags.FlagSet(PawnRenderFlags.Cache) ? render.graphics.apparelGraphics[i].graphic.MatAt(facing, null) : OverrideMaterialIfNeeded(render.graphics.apparelGraphics[i].graphic.MatAt(facing, null), render.graphics.pawn, render, flags.FlagSet(PawnRenderFlags.Portrait));
                            GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }
                    }
                }
            }

            if (hasApparelAnimeComp || hasBodyAnimeComp)
            {
                refList.Clear();
                return refList;
            }
            return refList;
        }



        //服装渲染修正，使用IL
        public static IEnumerable<CodeInstruction> DrawApparelPatchTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo renderAsPackInfo = AccessTools.Method(typeof(PawnRenderer), "RenderAsPack", null, null);
            MethodInfo drawMeshNowOrLaterInfo = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) }, null);
            List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
            int num;
            for (int i = 0; i < instructionList.Count; i = num + 1)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                if (i < instructionList.Count - 50 && instructionList[i + 47].OperandIs(renderAsPackInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 1);//quaternion

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 1);//shellLoc

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 3);//bodyMesh

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);//bodyFacing

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);//flags

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 3);//apparelGraphicRecord

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 0);//render

                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderPatchs), "DrawApparelTranPatch", null, null));
                    //i = i + 44;
                    i = instructionList.FirstIndexOf(x => x.OperandIs(drawMeshNowOrLaterInfo) && instructionList[instructionList.IndexOf(x) + 3].OperandIs(renderAsPackInfo));
                }
                num = i;
            }
            yield break;
        }

        public static void DrawApparelTranPatch(Quaternion quaternion, Vector3 shellLoc, Mesh bodyMesh, Rot4 bodyFacing, PawnRenderFlags flags, ApparelGraphicRecord apparelGraphicRecord, PawnRenderer render)
        {
            if (apparelGraphicRecord.sourceApparel.GetComp<DAL_CompAnimated>()?.DALProps.apparelFrameList?.Exists(x => x.isApparel) != null)
            {
                DAL_CompAnimated comp = apparelGraphicRecord.sourceApparel.GetComp<DAL_CompAnimated>();
                comp.apparelGraphicList.Sort((x, y) => x.yOffSet.CompareTo(y.yOffSet));
                Mesh mesh = bodyMesh;
                Material material = null;
                foreach (DAL_GraphicInfo g in comp.apparelGraphicList)
                {
                    if (g.meshSize != -1f)
                    {
                        mesh = new GraphicMeshSet(1.5f * g.meshSize).MeshAt(bodyFacing);
                    }
                    material = g.graphic.MatAt(bodyFacing, null);
                    Vector3 loc = shellLoc;
                    if (apparelGraphicRecord.sourceApparel.def.apparel.shellCoversHead)
                    {
                        loc.y += 0.0028957527f;
                    }
                    loc.y += g.yOffSet;
                    GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
                }
            }
            else
            {
                Material material = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                material = (flags.FlagSet(PawnRenderFlags.Cache) ? material : OverrideMaterialIfNeeded(material, render.graphics.pawn, render, flags.FlagSet(PawnRenderFlags.Portrait)));
                Vector3 loc = shellLoc;
                if (apparelGraphicRecord.sourceApparel.def.apparel.shellCoversHead)
                {
                    loc.y += 0.0028957527f;
                }
                GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
            }
        }

        



        //头部渲染修正，使用IL
        /*public static IEnumerable<CodeInstruction> DrawHeadPatchTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo baseHeadOffsetAtInfo = AccessTools.Method(typeof(PawnRenderer), "BaseHeadOffsetAt", null, null);
            MethodInfo drawMeshNowOrLaterInfo = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) }, null);
            List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
            int num;
            for (int i = 0; i < instructionList.Count; i = num + 1)
            {
                CodeInstruction instruction = instructionList[i];
                yield return instruction;
                if (i < instructionList.Count - 50 && instructionList[i + 8].OperandIs(baseHeadOffsetAtInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 0);//instance

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 0);//quaternion

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 6);//vector2

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 2);//a

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);//bodyFacing

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);//flags

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);//bodyDrawType

                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderPatchs), "DrawHeadTranPatch", null, null));

                    yield return new CodeInstruction(OpCodes.Stloc_S, 6);

                    i = instructionList.FirstIndexOf(x => x.OperandIs(drawMeshNowOrLaterInfo));
                }
                num = i;
            }
            yield break;
        }

        public static Vector3 DrawHeadTranPatch(PawnRenderer instance, Quaternion quaternion, Vector3 vector2, Vector3 a, Rot4 bodyFacing, PawnRenderFlags flags, RotDrawMode bodyDrawType)
        {
            Mesh headMesh = null;
            //头部转向图像部分修补（头部图像）
            DAL_CompHeadTurning compHeadTurning = instance.graphics.pawn.GetComp<DAL_CompHeadTurning>();
            bool isSleepOrLayDownRest = instance.graphics.pawn.jobs?.curDriver != null && (instance.graphics.pawn.jobs?.curDriver.asleep == true || instance.graphics.pawn.jobs?.curJob.def == JobDefOf.LayDownResting || instance.graphics.pawn.jobs?.curJob.def == JobDefOf.LayDown || instance.graphics.pawn.jobs?.curJob.def.defName == "Skygaze");
            Rot4 headFacing = (compHeadTurning != null && !flags.FlagSet(PawnRenderFlags.Portrait) && !flags.FlagSet(PawnRenderFlags.HeadStump) && compHeadTurning.Props.enable && compHeadTurning.init && !isSleepOrLayDownRest) ? compHeadTurning.headFacing : bodyFacing;
            AlienPartGenerator.AlienComp compA = instance.graphics.pawn.GetComp<AlienPartGenerator.AlienComp>();
            if (compA == null)
            {
                headMesh = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
            }
            else if (!flags.FlagSet(PawnRenderFlags.Portrait))
            {
                headMesh = compA.alienHeadGraphics.headSet.MeshAt(headFacing);
            }
            else
            {
                headMesh = compA.alienPortraitHeadGraphics.headSet.MeshAt(headFacing);
            }
            DAL_CompAnimated comp = instance.graphics.pawn.GetComp<DAL_CompAnimated>();
            NazunaLib.DAL_CompPawnExpression excomp = instance.graphics.pawn.GetComp<NazunaLib.DAL_CompPawnExpression>();
            if (comp != null)
            {
                comp = instance.graphics.pawn.GetComp<DAL_CompAnimated>();
                comp.headGraphicList.Sort((x, y) => x.yOffSet.CompareTo(y.yOffSet));
                List<DAL_GraphicInfo> list = new List<DAL_GraphicInfo>();
                if (comp.DALProps.pawnFrameList?.Exists(x => x.isHead) != true)
                {
                    list.Add(new DAL_GraphicInfo(instance.graphics.headGraphic, false, 0f));
                    list.AddRange(comp.headGraphicList);
                }
                else
                {
                    list = comp.headGraphicList;
                }
                if (excomp != null && excomp.initialization)
                {
                    list.AddRange(excomp.curExpression.curGraphicList);
                }
                if (!list.NullOrEmpty())
                {
                    //Messages.Message("graphic loaded", MessageTypeDefOf.PositiveEvent);
                    vector2 = quaternion * instance.BaseHeadOffsetAt(headFacing);
                    Material material = null;
                    foreach (DAL_GraphicInfo g in list)
                    {
                        if (g.meshSize != -1f)
                        {
                            headMesh = new GraphicMeshSet(1.5f * g.meshSize).MeshAt(headFacing);
                        }
                        material = HeadMatAt(instance.graphics, g.graphic, headFacing, bodyDrawType, flags.FlagSet(PawnRenderFlags.HeadStump), flags.FlagSet(PawnRenderFlags.Portrait), !flags.FlagSet(PawnRenderFlags.Cache));
                        if (material != null)
                        {
                            Vector3 vector3 = vector2;
                            vector3.y += g.yOffSet;
                            GenDraw.DrawMeshNowOrLater(headMesh, a + vector2, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }
                    }
                }
            }
            else
            {
                if (instance.graphics.headGraphic != null)
                {
                    vector2 = quaternion * instance.BaseHeadOffsetAt(headFacing);
                    Material material = HeadMatAt(instance.graphics, instance.graphics.headGraphic, headFacing, bodyDrawType, flags.FlagSet(PawnRenderFlags.HeadStump), flags.FlagSet(PawnRenderFlags.Portrait), !flags.FlagSet(PawnRenderFlags.Cache));
                    if (material != null)
                    {
                        GenDraw.DrawMeshNowOrLater(headMesh, a + vector2, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }
                }
            }
            return vector2;
        }

        public static Material HeadMatAt(PawnGraphicSet pgs, Graphic graph, Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false, bool portrait = false, bool allowOverride = true)
        {
            Material material = null;
            if (bodyCondition == RotDrawMode.Fresh)
            {
                if (stump)
                {
                    material = pgs.headStumpGraphic.MatAt(facing, null);
                }
                else if (portrait)
                {
                    material = graph.MatAt(Rot4.South, null);
                }
                else
                {
                    material = graph.MatAt(facing, null);
                }
            }
            else if (bodyCondition == RotDrawMode.Rotting)
            {
                if (stump)
                {
                    material = pgs.desiccatedHeadStumpGraphic.MatAt(facing, null);
                }
                else
                {
                    material = pgs.desiccatedHeadGraphic.MatAt(facing, null);
                }
            }
            else if (bodyCondition == RotDrawMode.Dessicated && !stump)
            {
                material = pgs.skullGraphic.MatAt(facing, null);
            }
            if (material != null && allowOverride)
            {
                if (!portrait && pgs.pawn.IsInvisible())
                {
                    material = InvisibilityMatPool.GetInvisibleMat(material);
                }
                material = pgs.flasher.GetDamagedMat(material);
            }
            return material;
        }*/


    }




    //[HarmonyPatch(typeof(Pawn_ApparelTracker))]
    //[HarmonyPatch("ApparelTrackerTick")]
    public class DAL_ApparelTickPatch
    {
        static void Postfix(Pawn_ApparelTracker __instance)
        {
            foreach (Apparel apparel in __instance.WornApparel)
            {
                if (apparel.GetComp<DAL_CompAnimated>()?.DALProps.customFrameList?.Exists(x => x.isOverlay) == true)
                {
                    apparel.GetComp<DAL_CompAnimated>().PostDraw();
                }
            }
        }
    }



    //动画Pawn动画渲染框架核心修正
    //[HarmonyPatch(typeof(PawnRenderer))]
    //[HarmonyPatch("GetBlitMeshUpdatedFrame")]
    public class DAL_PawnAnimatedPatch
    {
        static bool Prefix(PawnRenderer __instance, PawnTextureAtlasFrameSet frameSet, Rot4 rotation, PawnDrawMode drawMode, ref Pawn ___pawn, ref Mesh __result)
        {
            //Messages.Message("PawnCacheRenderer", ___pawn, MessageTypeDefOf.ThreatBig, false);

            /*if (__instance.graphics?.pawn?.GetComp<DAL_CompAnimated>() != null || __instance.graphics?.pawn?.apparel?.WornApparel?.Exists(x => x.GetComp<DAL_CompAnimated>() != null) == true || __instance.graphics?.pawn?.GetComp<DAL_CompHeadTurning>() != null || __instance.graphics?.pawn?.GetComp<DAL_CompPawnExpression>() != null)
            {
                int index = frameSet.GetIndex(rotation, drawMode);
                Find.PawnCacheCamera.rect = frameSet.uvRects[index];
                Find.PawnCacheRenderer.RenderPawn(___pawn, frameSet.atlas, Vector3.zero, 1f, 0f, rotation, true, drawMode == PawnDrawMode.BodyAndHead, true, true, false, default(Vector3), null, null, false);
                Find.PawnCacheCamera.rect = new Rect(0f, 0f, 1f, 1f);
                frameSet.isDirty[index] = false;
                __result = frameSet.meshes[index];
                if (__instance.graphics?.pawn?.GetComp<DAL_CompHeadTurning>()?.shouldUpdate == true)
                {
                    __instance.graphics.pawn.GetComp<DAL_CompHeadTurning>().shouldUpdate = false;
                }
                return false;
            }*/
            return true;
        }
    }



    //初始化AB包加载
    //[HarmonyPatch(typeof(Root))]
    //[HarmonyPatch("CheckGlobalInit")]
    public class DAL_GameObjectPrefabLoadPatch
    {
        static void Postfix()
        {
            if (DAL_DynamicGameObjectPrefabManager.Initialized)
            {
                return;
            }
            DAL_DynamicGameObjectPrefabManager.InitGameObjectToList();
        }
    }



    //DAL_WorldCurrent构建
    [HarmonyPatch(typeof(World))]
    [HarmonyPatch("ConstructComponents")]
    public class DAL_WorldCurrentConstructPatch
    {
        static void Postfix()
        {
            DAL_WorldCurrent.GOM = new DAL_DynamicGameObjectManager();
        }
    }





    //交流时转头 暂停维护
    //[HarmonyPatch(typeof(Pawn_InteractionsTracker))]
    //[HarmonyPatch("TryInteractWith")]
    public class DAL_InteractionHeadTurningPatch
    {
        static void Postfix(Pawn ___pawn, Pawn recipient, InteractionDef intDef)
        {
            /*if (intDef.Worker as InteractionWorker_Chitchat == null)
            {
                return;
            }*/

            /*DAL_CompHeadTurning compA = ___pawn.GetComp<DAL_CompHeadTurning>();
            if (compA != null && compA.Props.enable && compA.init && ___pawn.GetPosture() == PawnPosture.Standing)
            {
                compA.TurningCaculate(recipient, true);
            }
            DAL_CompHeadTurning compB = recipient.GetComp<DAL_CompHeadTurning>();
            if (compB != null && compB.Props.enable && compB.init && recipient.GetPosture() == PawnPosture.Standing)
            {
                compB.TurningCaculate(___pawn);
            }

            List<Pawn> list = ___pawn.Map.mapPawns.AllPawnsSpawned;
            foreach (Pawn pawn in list)
            {
                if (pawn == ___pawn || pawn == recipient)
                {
                    continue;
                }
                DAL_CompHeadTurning comp = pawn.GetComp<DAL_CompHeadTurning>();
                if (comp != null && comp.Props.enable && comp.init)
                {
                    if (___pawn.Position.InHorDistOf(pawn.Position, comp.Props.attentionDist))
                    {
                        if (GenSight.LineOfSight(___pawn.Position, pawn.Position, ___pawn.Map, true, null, 0, 0) && pawn.GetPosture() == PawnPosture.Standing)
                        {
                            comp.TurningCaculate(___pawn);
                        }
                    }
                }
            }*/
        }
    }


    public class DAL_ExpressionCheckTask
    {
        public static async Task ForeachTask(List<DAL_PawnExpressionDataDef> list, Action<DAL_PawnExpressionDataDef> action)
        {
            await Task.Run(() =>
            {
                foreach (DAL_PawnExpressionDataDef def in list)
                {
                    action.Invoke(def);
                }
            });
        }
    }


    //Pawn表情死亡检测 暂停维护
    //[HarmonyPatch(typeof(Pawn_HealthTracker))]
    //[HarmonyPatch("SetDead")]
    public class DAL_PawnExpressionDeadPatch
    {
        static bool Prefix(Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            /*DAL_CompPawnExpression comp = ___pawn.GetComp<DAL_CompPawnExpression>();
            if (comp != null)
            {
                bool triggerHasThis = false;
                DAL_PawnExpressionDataDef data = null;

                foreach (DAL_PawnExpressionDataDef def in comp.Props.expressionPlanDef.expressionDB)
                {
                    if (def.triggerDef.isDead)
                    {
                        triggerHasThis = true;
                        data = def;
                        break;
                    }
                }
                if (triggerHasThis && comp.curExpression.def != data)
                {
                    if (comp.curExpression != null)
                    {
                        DAL_PawnExpressionMaker.ReturnToPool(ref comp.curExpression);
                    }
                    comp.curExpression = DAL_PawnExpressionMaker.MakeExpression(data, comp);
                    comp.SetExpressionTime();
                    if (comp.curDriver != null)
                    {
                        comp.lastDriver = comp.curDriver;
                        comp.curDriver.Clear();
                    }
                    comp.curExpression.TryMakeDriver(___pawn, out comp.curDriver);
                    if (comp.curDriver != null)
                    {
                        comp.curDriver.DriverTick();

                    }
                    Messages.Message("dead change", MessageTypeDefOf.PositiveEvent);
                }
            }*/
            return true;
        }
    }



    //Pawn表情Damage检测 暂停维护
    //[HarmonyPatch(typeof(Pawn))]
    //[HarmonyPatch("PostApplyDamage")]
    //[HarmonyPatch(new Type[] { })]
    public class DAL_PawnExpressionDamagePatch
    {
        static void Postfix(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            /*DAL_CompPawnExpression comp = __instance.GetComp<DAL_CompPawnExpression>();
            if (comp != null && !__instance.Dead)
            {
                bool triggerHasThisDamage = false;
                DAL_PawnExpressionDataDef data = null;
                foreach (DAL_PawnExpressionDataDef def in comp.Props.expressionPlanDef.expressionDB)
                {
                    if (def.triggerDef.damageDef == dinfo.Def)
                    {
                        triggerHasThisDamage = true;
                        data = def;
                        break;
                    }
                }
                if (triggerHasThisDamage && data?.triggerDef.CheckPawn(__instance) == true && comp.curExpression.def != data)
                {
                    if (comp.curExpression != null)
                    {
                        DAL_PawnExpressionMaker.ReturnToPool(ref comp.curExpression);
                    }
                    comp.curExpression = DAL_PawnExpressionMaker.MakeExpression(data, comp);
                    comp.SetExpressionTime();
                    Messages.Message("damage change", MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    comp.DamageManager();
                    Messages.Message("damage", MessageTypeDefOf.PositiveEvent);
                }
            }*/
        }
    }


    //Pawn表情Job变换检测 暂停维护
    //[HarmonyPatch(typeof(Pawn_JobTracker))]
    //[HarmonyPatch("StartJob")]
    //[HarmonyPatch(new Type[] { })]
    public class DAL_PawnExpressionJobPatch
    {
        static void Postfix(Pawn_JobTracker __instance, Pawn ___pawn, Job newJob)
        {
            if (___pawn != null && newJob != null && !___pawn.Dead)
            {
                /*DAL_CompPawnExpression comp = ___pawn.GetComp<DAL_CompPawnExpression>();
                if (comp != null)
                {
                    bool triggerHasThisJob = false;
                    DAL_PawnExpressionDataDef data = null;

                    foreach (DAL_PawnExpressionDataDef def in comp.Props.expressionPlanDef.expressionDB)
                    {
                        if (def.triggerDef.jobDef == newJob.def)
                        {
                            triggerHasThisJob = true;

                            data = def;
                            break;
                        }
                    }
                    if (triggerHasThisJob && data?.triggerDef.CheckPawn(___pawn) == true && comp.curExpression.def != data)
                    {
                        if (comp.curExpression != null)
                        {
                            DAL_PawnExpressionMaker.ReturnToPool(ref comp.curExpression);
                        }
                        comp.curExpression = DAL_PawnExpressionMaker.MakeExpression(data, comp);
                        comp.SetExpressionTime();
                        Messages.Message("job changed", MessageTypeDefOf.PositiveEvent);
                    }
                }*/
            }
        }
    }



    //Pawn表情Need变换检测 暂停维护
    //[HarmonyPatch(typeof(Need))]
    //[HarmonyPatch("SetInitialLevel")]
    public class DAL_PawnExpressionNeedPatch
    {
        static void Postfix(Need __instance, Pawn ___pawn)
        {
            /*DAL_CompPawnExpression comp = ___pawn.GetComp<DAL_CompPawnExpression>();
            if (comp != null && !___pawn.Dead)
            {
                bool triggerHasThisNeed = false;
                DAL_PawnExpressionDataDef data = null;
                foreach (DAL_PawnExpressionDataDef def in comp.Props.expressionPlanDef.expressionDB)
                {
                    if (def.triggerDef.needDef == __instance.def)
                    {
                        triggerHasThisNeed = true;
                        data = def;
                        break;
                    }
                }
                if (triggerHasThisNeed && data?.triggerDef.CheckPawn(___pawn) == true && comp.curExpression.def != data)
                {
                    if (comp.curExpression != null)
                    {

                        DAL_PawnExpressionMaker.ReturnToPool(ref comp.curExpression);
                    }
                    comp.curExpression = DAL_PawnExpressionMaker.MakeExpression(data, comp);
                    comp.SetExpressionTime();
                }
            }*/
        }
    }



    //Pawn表情Verb变换检测 暂停维护
    //[HarmonyPatch(typeof(Verb))]
    //[HarmonyPatch("TryStartCastOn")]
    //[HarmonyPatch(new Type[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(bool), typeof(bool), typeof(bool) })]
    public class DAL_PawnExpressionVerbPatch
    {
        static void Postfix(bool __result, Verb __instance)
        {
            /*if (__instance.CasterIsPawn && !__instance.CasterPawn.Dead)
            {
                DAL_CompPawnExpression comp = __instance.CasterPawn.GetComp<DAL_CompPawnExpression>();
                if (comp != null && __result)
                {
                    bool triggerHasThisVerb = false;
                    DAL_PawnExpressionDataDef data = null;
                    foreach (DAL_PawnExpressionDataDef def in comp.Props.expressionPlanDef.expressionDB)
                    {
                        if (def.triggerDef.verb == __instance.GetType())
                        {
                            triggerHasThisVerb = true;
                            data = def;
                            break;
                        }
                    }
                    if (triggerHasThisVerb && data?.triggerDef.CheckPawn(__instance.CasterPawn) == true && comp.curExpression.def != data)
                    {
                        if (comp.curExpression != null)
                        {

                            DAL_PawnExpressionMaker.ReturnToPool(ref comp.curExpression);
                        }
                        comp.curExpression = DAL_PawnExpressionMaker.MakeExpression(data, comp);
                        comp.SetExpressionTime();
                    }
                }
            }*/
        }
    }



    //Pawn表情Hediff变换检测 暂停维护
    //[HarmonyPatch(typeof(Hediff))]
    //[HarmonyPatch("PostMake")]
    public class DAL_PawnExpressionHediffPatch
    {
        static void Postfix(Hediff __instance)
        {
            /*DAL_CompPawnExpression comp = __instance.pawn.GetComp<DAL_CompPawnExpression>();
            if (comp != null && !__instance.pawn.Dead)
            {
                bool triggerHasThisHediff = false;
                DAL_PawnExpressionDataDef data = null;
                foreach (DAL_PawnExpressionDataDef def in comp.Props.expressionPlanDef.expressionDB)
                {
                    if (def.triggerDef.hediffDef == __instance.def)
                    {
                        triggerHasThisHediff = true;
                        data = def;
                        break;
                    }
                }
                if (triggerHasThisHediff && data?.triggerDef.CheckPawn(__instance.pawn) == true && comp.curExpression.def != data)
                {
                    if (comp.curExpression != null)
                    {

                        DAL_PawnExpressionMaker.ReturnToPool(ref comp.curExpression);
                    }
                    comp.curExpression = DAL_PawnExpressionMaker.MakeExpression(data, comp);
                    comp.SetExpressionTime();
                }
            }*/
        }
    }



    //Pawn表情Thought变换检测 暂停维护
    //[HarmonyPatch(typeof(Thought))]
    //[HarmonyPatch("Init")]
    public class DAL_PawnExpressionThoughtPatch
    {
        static void Postfix(Thought __instance)
        {
            /*if (__instance.def.IsMemory && __instance.pawn != null && !__instance.pawn.Dead)
            {
                DAL_CompPawnExpression comp = __instance.pawn.GetComp<DAL_CompPawnExpression>();
                if (comp != null)
                {
                    bool triggerHasThisHediff = false;
                    DAL_PawnExpressionDataDef data = null;
                    foreach (DAL_PawnExpressionDataDef def in comp.Props.expressionPlanDef.expressionDB)
                    {
                        if (def.triggerDef.thoughtDef == __instance.def)
                        {
                            triggerHasThisHediff = true;
                            data = def;
                            break;
                        }
                    }
                    if (triggerHasThisHediff && data?.triggerDef.CheckPawn(__instance.pawn) == true && comp.curExpression.def != data)
                    {
                        if (comp.curExpression != null)
                        {
                            DAL_PawnExpressionMaker.ReturnToPool(ref comp.curExpression);
                        }
                        comp.curExpression = DAL_PawnExpressionMaker.MakeExpression(data, comp);
                        comp.SetExpressionTime();
                    }
                }
            }*/
        }
    }
}
