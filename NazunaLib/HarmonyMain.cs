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
using UnityEngine.UIElements;
using static HarmonyLib.Code;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements.Experimental;
using UnityEngine.Assertions.Must;

namespace NareisLib
{
    [StaticConstructorOnStartup]
    public class HarmonyMain
    {
        static HarmonyMain()
        {
            var harmonyInstance = new Harmony("NareisLib.kamijouko.nazunarei");

            //harmonyInstance.Patch(AccessTools.Method(typeof(ThingWithComps), "PostMake", null, null), null, new HarmonyMethod(typeof(PawnRenderPatchs), "PostMakeAddCompPostfix", null), null, null);
            //harmonyInstance.Patch(AccessTools.Method(typeof(ThingWithComps), "ExposeData", null, null), null, new HarmonyMethod(typeof(PawnRenderPatchs), "ExposeDataAddCompPostfix", null), null, null);
            //harmonyInstance.Patch(AccessTools.Method(typeof(ThingWithComps), "InitializeComps", null, null), new HarmonyMethod(typeof(PawnRenderPatchs), "InitializeCompsAddCompPrefix", null), null, null, null);
            
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveAllGraphics", null, null), null, null, null, new HarmonyMethod(typeof(PawnRenderPatchs), "ResolveAllGraphicsFinalizer", null));
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics", null, null), null, new HarmonyMethod(typeof(PawnRenderPatchs), "ResolveHairGraphicsPostfix", null), null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnGraphicSet), "ResolveApparelGraphics", null, null), null, new HarmonyMethod(typeof(PawnRenderPatchs), "ResolveApparelGraphicsPostfix", null), null, null);

            harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawPawnBody", null, null), new HarmonyMethod(typeof(PawnRenderPatchs), "DrawPawnBodyPrefix", null), null, null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawPawnBody", null, null), null, null, null, new HarmonyMethod(typeof(PawnRenderPatchs), "DrawPawnBodyFinalizer", null));
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawBodyApparel", null, null), new HarmonyMethod(typeof(PawnRenderPatchs), "DrawBodyApparelPrefix", null), null, null, null);
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", null, null), null, null, new HarmonyMethod(typeof(PawnRenderPatchs), "RenderPawnInternalHeadPatchTranspiler", null), null);
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderer), "DrawHeadHair", null, null), null, null, new HarmonyMethod(typeof(PawnRenderPatchs), "DrawHeadHairPatchTranspiler", null), null);
            harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", null, null), null, new HarmonyMethod(typeof(PawnRenderPatchs), "RenderPawnInternalPostfix", null), null, null);

        }
    }



    //初始化后加载资源全靠它！！！（已停用）
    //转移到带有[EarlyInit]属性的ThisModBase类中执行加载
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
        //给所有Pawn添加多层渲染Comp，CompTick有触发条件所以不存在性能问题[停用]
        //现在转移到带有[EarlyInit]属性的ThisModBase类中添加
        /*public static bool InitializeCompsAddCompPrefix(ThingWithComps __instance)
        {
            Pawn pawn = __instance as Pawn;
            if (pawn == null || pawn.def.comps.Exists(x => x.GetType() == typeof(MultiRenderCompProperties)))
                return true;
            pawn.def.comps.Add(new MultiRenderCompProperties());
            return true;
        }*/

        public static void PostMakeAddCompPostfix(ThingWithComps __instance, List<ThingComp> ___comps)
        {
            Pawn pawn = __instance as Pawn;
            if (pawn == null)
                return;
            ThingComp comp = null;
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                comp = (ThingComp)Activator.CreateInstance(typeof(MultiRenderComp));
                comp.parent = pawn;
                ___comps.Add(comp);
                comp.Initialize(pawn.def.comps.Exists(x => x.GetType() == typeof(MultiRenderCompProperties))
                                ? pawn.def.comps.First(x => x.GetType() == typeof(MultiRenderCompProperties))
                                : new MultiRenderCompProperties());
            }
            if (comp != null)
                comp.PostPostMake();
        }


        //处理defName所指定的MultiTexDef，
        //对其属性levels里所存储的所有TextureLevels都根据指定的权重随机一个贴图的名称，
        //并将名称记录进一个从其属性cacheOfLevels得来的MultiTexEpoch中所对应渲染图层的MultiTexBatch的名称列表里，
        //最终返回这个MultiTexEpoch
        public static MultiTexEpoch ResolveMultiTexDef(MultiTexDef def, out Dictionary<string, TextureLevels> data)
        {
            MultiTexEpoch epoch = new MultiTexEpoch(def.originalDefClass.ToStringSafe() + "_" + def.originalDef);
            List<MultiTexBatch> batches = new List<MultiTexBatch>();
            data = new Dictionary<string, TextureLevels>();
            try
            {
                foreach (TextureLevels level in def.levels)
                {
                    string pre = "";
                    string keyName = "";
                    if (level.prefix.NullOrEmpty() && level.texPath != null)
                    {
                        keyName = TextureLevels.ResolveKeyName(Path.GetFileNameWithoutExtension(level.texPath));
                    }
                    else if (!level.prefix.NullOrEmpty())
                    {
                        pre = level.prefix.RandomElementByWeight(x => level.preFixWeights.ContainsKey(x) ? level.preFixWeights[x] : 1);
                        keyName = level.preFixToTexName[pre].RandomElementByWeight(x => level.texWeights[pre].ContainsKey(x) ? level.texWeights[pre][x] : 1);
                    }

                    if (!batches.Exists(x => x.textureLevelsName == level.textureLevelsName))
                        batches.Add(new MultiTexBatch(def.originalDefClass, def.originalDef, def.defName, keyName, level.textureLevelsName, level.renderLayer, level.renderSwitch, level.staticLayer));

                    if (ModStaticMethod.ThisMod.debugToggle)
                    {
                        Log.Warning("render switch : " + level.renderSwitch.ToStringSafe());
                        Log.Warning("render layer : " + level.renderLayer.ToStringSafe());
                    }

                    string type_defName = def.originalDefClass.ToStringSafe() + "_" + def.originalDef;
                    if (ThisModData.TexLevelsDatabase.ContainsKey(type_defName) && ThisModData.TexLevelsDatabase[type_defName].ContainsKey(level.textureLevelsName))
                    {
                        TextureLevels textureLevels = ThisModData.TexLevelsDatabase[type_defName][level.textureLevelsName].Clone();
                        textureLevels.keyName = keyName;
                        /*if (textureLevels.patternSets != null)
                            textureLevels.patternSets.typeOriginalDefNameKeyName = textureLevels.originalDefClass.ToStringSafe() + "_" + textureLevels.originalDef + "_" + keyName;*/
                        if (!data.ContainsKey(textureLevels.textureLevelsName))
                            data[textureLevels.textureLevelsName] = textureLevels;
                    }
                }
                epoch.batches = batches;
            }
            catch (Exception ex)
            {
                throw new Exception("一个MultiTexDef:" + def.defName + "出错了, 这很有可能是其levels中的某个textureLevelsName配置不符合规范, 或者是对应的贴图及其路径内包含错误(请尝试检查标点符号以及贴图和路径是否存在等等)", ex);
            }
            
            return epoch;
        }


        //从comp的storedData里获取TextureLevels数据，用于处理读取存档时从已有的storedData字典中得到的epoch
        public static Dictionary<string, TextureLevels> GetLevelsDictFromEpoch(MultiTexEpoch epoch)
        {
            return !epoch.batches.NullOrEmpty() ? epoch.batches.ToDictionary(k => k.textureLevelsName, v => ResolveKeyNameForLevel(ThisModData.TexLevelsDatabase[v.originalDefClass.ToStringSafe() + "_" + v.originalDefName][v.textureLevelsName].Clone(), v.keyName)) : new Dictionary<string, TextureLevels>();
        }
        //上方法的子方法，为获取到的TextureLevels进行赋值操作
        public static TextureLevels ResolveKeyNameForLevel(TextureLevels level, string key)
        {
            level.keyName = key;
            /*if (level.patternSets != null)
                level.patternSets.typeOriginalDefNameKeyName = level.originalDefClass.ToStringSafe() + "_" + level.originalDef + "_" + key;*/
            return level;
        }


        //预处理Pawn身体的多层渲染数据，只会在pawn出现或生成时执行一次，在整体预处理方法中最后执行（因为原方法的顺序）
        static void ResolveAllGraphicsFinalizer(PawnGraphicSet __instance)
        {
            if (!ModStaticMethod.AllLevelsLoaded || ThisModData.DefAndKeyDatabase.NullOrEmpty())
                return;
            Pawn pawn = __instance.pawn;
            string race = pawn.def.defName;
            if (!ThisModData.RacePlansDatabase.ContainsKey(race))
                return;
            RenderPlanDef def = ThisModData.RacePlansDatabase[race];
            string plan = def.defName;

            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return;//AddComp(ref comp, ref pawn);
            //comp.cachedRenderPlanDefName = plan;

            List<string> cachedOverride = new List<string>();
            Dictionary<string, Dictionary<string, TextureLevels>> cachedGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();
            Dictionary<string, MultiTexEpoch> data = new Dictionary<string, MultiTexEpoch>();
            HeadTypeDef head = pawn.story.headType;
            string headName = head != null ? head.defName : "";
            string fullOriginalDefName = typeof(HeadTypeDef).ToStringSafe() + "_" + headName;
            if (ThisModData.DefAndKeyDatabase.ContainsKey(plan))
            {
                if (head != null && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
                {
                    MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                    if (comp.storedDataBody.NullOrEmpty() || !comp.storedDataBody.ContainsKey(fullOriginalDefName))
                    {
                        Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                        data[fullOriginalDefName] = ResolveMultiTexDef(multidef, out cachedData);
                        cachedGraphicData[fullOriginalDefName] = cachedData;
                    }
                    else
                    {
                        data[fullOriginalDefName] = comp.storedDataBody[fullOriginalDefName];
                        cachedGraphicData[fullOriginalDefName] = GetLevelsDictFromEpoch(data[fullOriginalDefName]);
                    }
                    if (!multidef.renderOriginTex)
                        cachedOverride.Add("Head");
                }
                BodyTypeDef body = pawn.story.bodyType;
                string bodyName = body != null ? body.defName : "";
                fullOriginalDefName = typeof(BodyTypeDef).ToStringSafe() + "_" + bodyName;
                if (body != null && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
                {
                    MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                    if (comp.storedDataBody.NullOrEmpty() || !comp.storedDataBody.ContainsKey(fullOriginalDefName))
                    {
                        Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                        data[fullOriginalDefName] = ResolveMultiTexDef(multidef, out cachedData);
                        cachedGraphicData[fullOriginalDefName] = cachedData;
                    }
                    else
                    {
                        data[fullOriginalDefName] = comp.storedDataBody[fullOriginalDefName];
                        cachedGraphicData[fullOriginalDefName] = GetLevelsDictFromEpoch(data[fullOriginalDefName]);
                    }
                    if (!multidef.renderOriginTex)
                        cachedOverride.Add("Body");
                }
                string hand = comp.GetCurHandDefName;
                fullOriginalDefName = typeof(HandTypeDef).ToStringSafe() + "_" + hand;
                if (hand != "" && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
                {
                    MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                    if (comp.storedDataBody.NullOrEmpty() || !comp.storedDataBody.ContainsKey(fullOriginalDefName))
                    {
                        Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                        data[fullOriginalDefName] = ResolveMultiTexDef(multidef, out cachedData);
                        cachedGraphicData[fullOriginalDefName] = cachedData;
                    }
                    else
                    {
                        data[fullOriginalDefName] = comp.storedDataBody[fullOriginalDefName];
                        cachedGraphicData[fullOriginalDefName] = GetLevelsDictFromEpoch(data[fullOriginalDefName]);
                    }
                }
            }
            comp.cachedOverrideBody = cachedOverride;
            comp.cachedBodyGraphicData = cachedGraphicData;
            comp.storedDataBody = data;
            comp.ResolveAllLayerBatch();
            comp.PrefixResolved = true;
            comp.pawnName = pawn.Name.ToStringFull;
        }

        //预处理Pawn的头发，在发型变换时会执行
        static void ResolveHairGraphicsPostfix(PawnGraphicSet __instance)
        {
            if (!ModStaticMethod.AllLevelsLoaded || ThisModData.DefAndKeyDatabase.NullOrEmpty())
                return;
            Pawn pawn = __instance.pawn;
            string race = pawn.def.defName;
            if (!ThisModData.RacePlansDatabase.ContainsKey(race))
                return;
            RenderPlanDef def = ThisModData.RacePlansDatabase[race];
            string plan = def.defName;
            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return;//AddComp(ref comp, ref pawn);

            List<string> cachedOverride = new List<string>();
            Dictionary<string, Dictionary<string, TextureLevels>> cachedGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();
            Dictionary<string, MultiTexEpoch> data = new Dictionary<string, MultiTexEpoch>();
            HairDef hair = pawn.story.hairDef;
            string keyName = hair != null ? hair.defName : "";
            string fullOriginalDefName = typeof(HairDef).ToStringSafe() + "_" + keyName;
            if (hair != null && ThisModData.DefAndKeyDatabase.ContainsKey(plan) && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
            {
                
                MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                if (comp.storedDataHair.NullOrEmpty() || !comp.storedDataHair.ContainsKey(fullOriginalDefName))
                {
                    //Log.Warning("Hair Respawning Hair Respawning Hair Respawning");
                    Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                    data[fullOriginalDefName] = ResolveMultiTexDef(multidef, out cachedData);
                    cachedGraphicData[fullOriginalDefName] = cachedData;
                }
                else
                {
                    //Log.Warning("Hair Loading Hair Loading Hair Loading");
                    //Log.Warning("TexLevelsDatabase : " + ThisModData.TexLevelsDatabase.Count);
                    data[fullOriginalDefName] = comp.storedDataHair[fullOriginalDefName];
                    //Log.Warning("first key : " + batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName);
                    //Log.Warning("TexLevelsDatabase has first key : " + ThisModData.TexLevelsDatabase.ContainsKey(batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName).ToStringSafe());
                    //Log.Warning("second key : " + batch.textureLevelsName);
                    //Log.Warning("TexLevelsDatabase has second key : " + ThisModData.TexLevelsDatabase[batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName].ContainsKey(batch.textureLevelsName).ToStringSafe());
                    
                    cachedGraphicData[fullOriginalDefName] = GetLevelsDictFromEpoch(comp.storedDataHair[fullOriginalDefName]);
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
            if (!ThisModData.RacePlansDatabase.ContainsKey(race))
                return;
            RenderPlanDef def = ThisModData.RacePlansDatabase[race];
            string plan = def.defName;
            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return;//AddComp(ref comp, ref pawn);

            List<string> cachedOverride = new List<string>();
            Dictionary<string, Dictionary<string, TextureLevels>> cachedGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();
            Dictionary<string, MultiTexEpoch> data = new Dictionary<string, MultiTexEpoch>();
            using (List<Apparel>.Enumerator enumerator = __instance.pawn.apparel.WornApparel.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string keyName = enumerator.Current.def.defName;
                    string fullOriginalDefName = typeof(ThingDef).ToStringSafe() + "_" + keyName;
                    if (ThisModData.DefAndKeyDatabase.ContainsKey(plan) && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
                    {
                        MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                        if (comp.storedDataApparel.NullOrEmpty() || !comp.storedDataApparel.ContainsKey(fullOriginalDefName))
                        {
                            Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                            data[fullOriginalDefName] = ResolveMultiTexDef(multidef, out cachedData);
                            cachedGraphicData[fullOriginalDefName] = cachedData;
                        }
                        else
                        {
                            data[fullOriginalDefName] = comp.storedDataApparel[fullOriginalDefName];
                            cachedGraphicData[fullOriginalDefName] = GetLevelsDictFromEpoch(data[fullOriginalDefName]);
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


        //BottomOverlay BodyPrefix
        static bool DrawPawnBodyPrefix(PawnRenderer __instance, Pawn ___pawn, ref bool __state, Vector3 rootLoc, float angle, Rot4 facing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
        {
            //Log.Warning(___pawn.Name + " flags: DrawNow = " + flags.FlagSet(PawnRenderFlags.DrawNow).ToStringSafe());
            __state = false;
            MultiRenderComp comp = ___pawn.GetComp<MultiRenderComp>();
            AlienPartGenerator.AlienComp alienComp = ___pawn.GetComp<AlienPartGenerator.AlienComp>();
            if (comp == null)
                return __state = true;
            //if (!comp.PrefixResolved)
                //__instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(facing);
            if (curDirection.NullOrEmpty())
                return __state = true;

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 vector = rootLoc;
            vector.y += 0.002687258f;/*原身体为0.008687258f，反映精度为0.0003f*/
            Mesh bodyMesh = null;
            NareisLib_GraphicMeshSet bodyMeshSet = null;
            NareisLib_GraphicMeshSet hairMeshSet = null;
            NareisLib_GraphicMeshSet headMeshSet = null;
            if (___pawn.RaceProps.Humanlike)
            {
                bodyMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeBodySetForPawn(___pawn);
                headMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHeadSetForPawn(___pawn);
                hairMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHairSetForPawn(___pawn);
            }
            else
                bodyMesh = __instance.graphics.nakedGraphic.MeshAt(facing);

            bool displayLevelInfo = ModStaticMethod.ThisMod.apparelLevelsDisplayToggle;

            if (displayLevelInfo)
            {
                Log.Warning(" ");
                Log.Warning("-------------------------------------------------------------------");
                Log.Warning("当前面向：" + facing.ToStringHuman());
                Log.Warning("从 " + vector.y.ToString() + " 开始渲染底部图层");
            }
            

            List<int> renderLayers = new List<int>() { (int)TextureRenderLayer.BottomOverlay };
            int level = (int)TextureRenderLayer.BottomOverlay;

            Vector3 local = vector;
            if (displayLevelInfo)
                Log.Warning(" BottomOverlay层: 从" + local.y.ToString() + "开始");

            /*foreach (int level in renderLayers)
            {
                if (level == (int)TextureRenderLayer.BottomOverlay)
                {
                    local.y -= 0.002f;
                    
                }
                else
                {
                    if (displayLevelInfo)
                        Log.Warning(" BottomHair层: 从" + local.y.ToString() + "开始");
                    if (___pawn.story.headType == HeadTypeDefOf.Stump)
                        continue;
                } 
            }*/

            if (curDirection.ContainsKey(level))
            {
                Color colorTwo = alienComp != null ? alienComp.GetChannel("hair").second : Color.white;
                foreach (MultiTexBatch batch in curDirection[level])
                {
                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                    //string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;
                    string mlPrefixName = batch.multiTexDefName + "_";

                    TextureLevels data;
                    if (!comp.TryGetStoredTextureLevels(typeOriginalDefName, batch.textureLevelsName, out data) 
                        || (!comp.cachedHideOrReplaceDict.NullOrEmpty() 
                            && comp.cachedHideOrReplaceDict.ContainsKey(mlPrefixName + data.textureLevelsName) 
                            && comp.cachedHideOrReplaceDict[mlPrefixName + data.textureLevelsName].hide)
                        || !data.CanRender(___pawn, batch.keyName))
                        continue;

                    /*if (comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty()
                        || !comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(typeOriginalDefName)
                        || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName].ContainsKey(batch.textureLevelsName)
                        || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName].CanRender(___pawn, batch.keyName))
                        continue;
                    TextureLevels data = comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName];*/

                    Color colorOne = data.useStaticColor ? data.color : ___pawn.story.HairColor;
                    colorTwo = data.useStaticColor ? data.colorTwo : colorTwo;

                    Mesh mesh = null;
                    Vector3 offset = Vector3.zero;
                    if (data.meshSize != Vector2.zero)
                    {
                        mesh = NareisLib_MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing, data.flipped);
                        switch (data.meshType)
                        {
                            case "Head": offset = quat * __instance.BaseHeadOffsetAt(facing); break;
                            case "Hair": offset = quat * __instance.BaseHeadOffsetAt(facing); break;
                        }
                    }
                    else
                    {
                        if (___pawn.RaceProps.Humanlike)
                        {
                            switch (data.meshType)
                            {
                                case "Body": 
                                    mesh = bodyMeshSet.MeshAt(facing, data.flipped); 
                                    break;
                                case "Head":
                                    mesh = headMeshSet.MeshAt(facing, data.flipped);
                                    offset = quat * __instance.BaseHeadOffsetAt(facing);
                                    break;
                                case "Hair":
                                    mesh = hairMeshSet.MeshAt(facing, data.flipped);
                                    offset = quat * __instance.BaseHeadOffsetAt(facing);
                                    break;
                            }
                        }
                        else
                            mesh = bodyMesh;
                    }
                    /*int pattern = 0;
                    if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                        pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];*/
                    string condition = "";
                    if (data.hasRotting && bodyDrawType == RotDrawMode.Rotting)
                        condition = "Rotting";
                    if (data.hasDessicated && bodyDrawType == RotDrawMode.Dessicated)
                        condition = "Dessicated";
                    string bodyType = "";
                    string headType = "";
                    if (data.useBodyType)
                        bodyType = ___pawn.story.bodyType.defName;
                    else if (data.useHeadType)
                        headType = ___pawn.story.headType.defName;
                    Vector3 dataOffset = data.DrawOffsetForRot(facing);
                    if (data.useStaticYOffset)
                        local.y = rootLoc.y + dataOffset.y * 0.01f;
                    if (data.usePublicYOffset)
                        dataOffset.y *= 0.01f;
                    else
                        dataOffset.y *= 0.0001f;
                    Vector3 pos = local + offset + dataOffset;
                    Rot4 matFacing = data.switchEastWest && (facing == Rot4.East || facing == Rot4.West) ? new Rot4(4 - facing.AsInt) : facing;
                    Material mat = data.GetGraphic(batch.keyName, colorOne, colorTwo, condition, bodyType, headType).MatAt(matFacing, null);
                    GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    if (displayLevelInfo)
                        Log.Warning(" " + data.originalDef + "------------" + data.textureLevelsName + ": " + pos.y.ToString());
                }
            }


            if (!curDirection.ContainsKey((int)TextureRenderLayer.Body) && !curDirection.ContainsKey((int)TextureRenderLayer.Apparel))
                __state = true;

            return __state;
        }


        //下面补丁的子方法
        public static Material OverrideMaterialIfNeeded(PawnRenderer instance, Material original, Pawn pawn, bool portrait = false)
        {
            Material baseMat = (!portrait && pawn.IsInvisible()) ? InvisibilityMatPool.GetInvisibleMat(original) : original;
            return instance.graphics.flasher.GetDamagedMat(baseMat);
        }

        public static Material OverrideMaterialIfNeeded(Material original, Pawn pawn, PawnRenderer render, bool portrait = false)
        {
            Material baseMat = (!portrait && pawn.IsInvisible()) ? InvisibilityMatPool.GetInvisibleMat(original) : original;
            return render.graphics.flasher.GetDamagedMat(baseMat);
        }


        //Body HandOne Hand HandTwo Apparel(除了shell层) BottomShell DrawPawnBodyFinalizer
        static void DrawPawnBodyFinalizer(PawnRenderer __instance, Pawn ___pawn, bool __state, Vector3 rootLoc, float angle, Rot4 facing, RotDrawMode bodyDrawType, PawnRenderFlags flags, out Mesh bodyMesh)
        {
            bodyMesh = null;
            NareisLib_GraphicMeshSet bodyMeshSet = null;
            NareisLib_GraphicMeshSet hairMeshSet = null;
            NareisLib_GraphicMeshSet headMeshSet = null;
            if (___pawn.RaceProps.Humanlike)
            {
                bodyMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeBodySetForPawn(___pawn);
                headMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHeadSetForPawn(___pawn);
                hairMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHairSetForPawn(___pawn);
                bodyMesh = bodyMeshSet.MeshAt(facing);
            }
            else
            {
                bodyMesh = __instance.graphics.nakedGraphic.MeshAt(facing);
            }
                

            MultiRenderComp comp = ___pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return;
            AlienPartGenerator.AlienComp alienComp = ___pawn.GetComp<AlienPartGenerator.AlienComp>();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(facing);
            if (curDirection.NullOrEmpty())
                return;

            bool displayLevelInfo = ModStaticMethod.ThisMod.apparelLevelsDisplayToggle;

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 vector = rootLoc;
            vector.y += 0.008687258f;
            Vector3 loc = vector;
            loc.y += 0.0014478763f;
            

            //如果原方法未执行且并不具有多层身体或者不隐藏原身体
            if (!__state
                && (!curDirection.ContainsKey((int)TextureRenderLayer.Body) 
                    || (!comp.GetAllHideOriginalDefData.NullOrEmpty() 
                        && !comp.GetAllHideOriginalDefData.Contains("Body"))))
            {
                Material bodyMat;
                PawnGraphicSet pawnSet = __instance.graphics;
                if (bodyDrawType == RotDrawMode.Fresh)
                {
                    if (___pawn.Dead && pawnSet.corpseGraphic != null)
                        bodyMat = pawnSet.corpseGraphic.MatAt(facing, null);
                    else
                        bodyMat = pawnSet.nakedGraphic.MatAt(facing, null);
                }
                else if (bodyDrawType == RotDrawMode.Rotting || pawnSet.dessicatedGraphic == null)
                    bodyMat = pawnSet.rottingGraphic.MatAt(facing, null);
                else
                    bodyMat = pawnSet.dessicatedGraphic.MatAt(facing, null);
                Material material = (___pawn.RaceProps.IsMechanoid && ___pawn.Faction != null && ___pawn.Faction != Faction.OfMechanoids) 
                    ? __instance.graphics.GetOverlayMat(bodyMat, ___pawn.Faction.MechColor) 
                    : bodyMat;
                Material mat = flags.FlagSet(PawnRenderFlags.Cache) ? material : OverrideMaterialIfNeeded(material, ___pawn, __instance, flags.FlagSet(PawnRenderFlags.Portrait));
                GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
            }

            //绘制（身体）和手臂
            if (displayLevelInfo)
            {
                Log.Warning(" ");
                Log.Warning("从 " + vector.y.ToString() + " 开始渲染身体图层");
            }
                

            List<int> renderLayers;
            if (__state)
                renderLayers = new List<int>() { (int)TextureRenderLayer.Hand};
            else
                renderLayers = new List<int>() { (int)TextureRenderLayer.Body, (int)TextureRenderLayer.Hand };
            foreach (int level in renderLayers)
            {
                if (displayLevelInfo)
                {
                    if (level == (int)TextureRenderLayer.Body)
                        Log.Warning(" Body层: 从" + vector.y.ToString() + "开始");
                    if (level == (int)TextureRenderLayer.Hand)
                        Log.Warning(" Hand层: 从" + vector.y.ToString() + "开始");
                }

                Vector3 locVec = vector;

                if (curDirection.ContainsKey(level))
                {
                    Color colorOne = alienComp == null ? ___pawn.story.SkinColor : alienComp.GetChannel("skin").first;
                    Color colorTwo = alienComp == null ? Color.white : alienComp.GetChannel("skin").second;
                    foreach (MultiTexBatch batch in curDirection[level])
                    {
                        string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                        //string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;
                        string mlPrefixName = batch.multiTexDefName + "_";

                        TextureLevels data;
                        if (!comp.TryGetStoredTextureLevels(typeOriginalDefName, batch.textureLevelsName, out data)
                            || (!comp.cachedHideOrReplaceDict.NullOrEmpty()
                                && comp.cachedHideOrReplaceDict.ContainsKey(mlPrefixName + data.textureLevelsName)
                                && comp.cachedHideOrReplaceDict[mlPrefixName + data.textureLevelsName].hide)
                            || !data.CanRender(___pawn, batch.keyName))
                            continue;

                        colorOne = data.useStaticColor ? data.color : colorOne;
                        colorTwo = data.useStaticColor ? data.colorTwo : colorTwo;
                        Mesh mesh = null;
                        Vector3 offset = Vector3.zero;
                        if (data.meshSize != Vector2.zero)
                        {
                            mesh = NareisLib_MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing, data.flipped);
                            switch (data.meshType)
                            {
                                case "Head": offset = quat * __instance.BaseHeadOffsetAt(facing); break;
                                case "Hair": offset = quat * __instance.BaseHeadOffsetAt(facing); break;
                            }
                        }
                        else
                        {
                            if (___pawn.RaceProps.Humanlike)
                            {
                                switch (data.meshType)
                                {
                                    case "Body": 
                                        mesh = bodyMeshSet.MeshAt(facing, data.flipped); 
                                        break;
                                    case "Head":
                                        mesh = headMeshSet.MeshAt(facing, data.flipped);
                                        offset = quat * __instance.BaseHeadOffsetAt(facing);
                                        break;
                                    case "Hair":
                                        mesh = hairMeshSet.MeshAt(facing, data.flipped);
                                        offset = quat * __instance.BaseHeadOffsetAt(facing);
                                        break;
                                }
                            }
                            else
                                mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(__instance.graphics.nakedGraphic, facing) : bodyMesh;
                        }
                        /*int pattern = 0;
                        if (data.isSleeve)
                        {
                            string handPrefix = typeof(HandTypeDef).ToStringSafe() + "_" + comp.GetCurHandDefName + "_";
                            string handKeyName = data.sleeveTexList.FirstOrDefault(x => comp.cachedRandomGraphicPattern.Keys.Contains(handPrefix + x));
                            if (handKeyName != null)
                                pattern = comp.cachedRandomGraphicPattern[handPrefix + handKeyName];
                            else
                                continue;
                        }  
                        else if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                            pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];*/
                        string condition = "";
                        if (data.hasRotting && bodyDrawType == RotDrawMode.Rotting)
                            condition = "Rotting";
                        if (data.hasDessicated && bodyDrawType == RotDrawMode.Dessicated)
                            condition = "Dessicated";
                        if (data.hasStump && flags.FlagSet(PawnRenderFlags.HeadStump))
                            condition = "Stump";
                        string bodyType = "";
                        if (data.useBodyType)
                            bodyType = ___pawn.story.bodyType.defName;
                        Vector3 handOffset = Vector3.zero;
                        if (!data.useStaticYOffset && !data.usePublicYOffset)
                        {
                            if (data.handDrawHigherOfShell && !data.isSleeve)
                            {
                                if (facing != Rot4.North)
                                    handOffset.y = 0.02027027f - 0.008687258f + 0.0028957527f + comp.Props.apparelInterval;
                                else
                                    handOffset.y = 0.023166021f - 0.008687258f + 0.0028957527f + comp.Props.apparelInterval;
                            }
                            else if (!data.isSleeve)
                            {
                                handOffset.y = 0.02027027f - 0.008687258f - 0.0015f;
                            }
                        }
                        Vector3 dataOffset = data.DrawOffsetForRot(facing);
                        if (data.useStaticYOffset)
                            locVec.y = rootLoc.y + dataOffset.y * 0.01f;
                        if (data.usePublicYOffset)
                            dataOffset.y *= 0.01f;
                        else
                            dataOffset.y *= 0.0001f;
                        Vector3 pos = locVec + offset + handOffset + dataOffset;
                        Rot4 matFacing = data.switchEastWest && (facing == Rot4.East || facing == Rot4.West) ? new Rot4(4 - facing.AsInt) : facing;
                        Material material = data.GetGraphic(batch.keyName, colorOne, colorTwo, condition, bodyType).MatAt(matFacing, null);
                        Material mat = (___pawn.RaceProps.IsMechanoid 
                            && ___pawn.Faction != null 
                            && ___pawn.Faction != Faction.OfMechanoids) ? __instance.graphics.GetOverlayMat(material, ___pawn.Faction.MechColor) : material;
                        GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                        if (displayLevelInfo)
                            Log.Warning(" " + data.originalDef + "------------" + data.textureLevelsName + ": " + pos.y.ToString());
                    }
                }
            }
            vector.y += comp.Props.apparelInterval;
            //vector.y += 0.0028957527f;

            
            //绘制衣服(非shell)
            if (!__state && flags.FlagSet(PawnRenderFlags.Clothes) && curDirection.ContainsKey((int)TextureRenderLayer.Apparel))
            {
                if (displayLevelInfo)
                {
                    Log.Warning(" ");
                    Log.Warning("从 " + vector.y.ToString() + " 开始渲染衣服(包含渲染在头后方的Shell层的衣服)");
                    Log.Warning("每层间间隔为 " + comp.Props.apparelInterval.ToString() + "，换算在xml里的间隔为 " + (comp.Props.apparelInterval * 100).ToString());
                }
                

                for (int i = 0; i < __instance.graphics.apparelGraphics.Count; i++)
                {
                    ApparelGraphicRecord apparel = __instance.graphics.apparelGraphics[i];
                    if ((apparel.sourceApparel.def.apparel.shellRenderedBehindHead || apparel.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell)
                        && !PawnRenderer.RenderAsPack(apparel.sourceApparel)
                        && apparel.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead
                        && apparel.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover)
                    {
                        //如果当前服装的def没在需要被隐藏的列表里的话
                        if (!comp.GetAllHideOriginalDefData.NullOrEmpty() && !comp.GetAllHideOriginalDefData.Contains(apparel.sourceApparel.def.defName))
                        {
                            //先画此服装
                            Material apparelMAt = (___pawn.RaceProps.IsMechanoid && ___pawn.Faction != null && ___pawn.Faction != Faction.OfMechanoids)
                                ? __instance.graphics.GetOverlayMat(apparel.graphic.MatAt(facing, null), ___pawn.Faction.MechColor)
                                : apparel.graphic.MatAt(facing, null);
                            Material material = flags.FlagSet(PawnRenderFlags.Cache)
                                ? apparelMAt
                                : OverrideMaterialIfNeeded(__instance, apparelMAt, ___pawn, flags.FlagSet(PawnRenderFlags.Portrait));

                            GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, material, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }

                        //如果是需要多层的服装的话
                        string apparelTypeOriginalDefName = apparel.sourceApparel.def.GetType().ToStringSafe() + "_" + apparel.sourceApparel.def.defName;
                        if (!comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty() && comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(apparelTypeOriginalDefName))
                        {
                            Color apparelColor = apparel.sourceApparel.DrawColor;
                            List<int> layers = new List<int>() { (int)TextureRenderLayer.Apparel };
                            if (curDirection.ContainsKey((int)TextureRenderLayer.BottomShell))
                                layers = new List<int>() { (int)TextureRenderLayer.BottomShell, (int)TextureRenderLayer.Apparel };

                            foreach (int layer in layers)
                            {
                                Vector3 local = vector;
                                if (layer == (int)TextureRenderLayer.BottomShell)
                                    local.y = rootLoc.y + 0.006687258f;

                                foreach (MultiTexBatch batch in curDirection[layer])
                                {
                                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                                    //string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;
                                    string mlPrefixName = batch.multiTexDefName + "_";

                                    TextureLevels data;
                                    if (typeOriginalDefName == apparelTypeOriginalDefName
                                        && comp.TryGetStoredTextureLevels(typeOriginalDefName, batch.textureLevelsName, out data)
                                        && (comp.cachedHideOrReplaceDict.NullOrEmpty()
                                            || !comp.cachedHideOrReplaceDict.ContainsKey(mlPrefixName + data.textureLevelsName)
                                            || !comp.cachedHideOrReplaceDict[mlPrefixName + data.textureLevelsName].hide)
                                        && data.CanRender(___pawn, batch.keyName))
                                    {
                                        apparelColor = data.useStaticColor ? data.color : apparelColor;
                                        Color colorTwo = data.useStaticColor ? data.colorTwo : Color.white;
                                        Mesh mesh = null;
                                        Vector3 offset = Vector3.zero;
                                        if (data.meshSize != Vector2.zero)
                                        {
                                            mesh = NareisLib_MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing, data.flipped);
                                            switch (data.meshType)
                                            {
                                                case "Head": offset = quat * __instance.BaseHeadOffsetAt(facing); break;
                                                case "Hair": offset = quat * __instance.BaseHeadOffsetAt(facing); break;
                                            }
                                        }
                                        else
                                        {
                                            if (___pawn.RaceProps.Humanlike)
                                            {
                                                switch (data.meshType)
                                                {
                                                    case "Body": mesh = bodyMeshSet.MeshAt(facing, data.flipped); break;
                                                    case "Head":
                                                        mesh = headMeshSet.MeshAt(facing, data.flipped);
                                                        offset = quat * __instance.BaseHeadOffsetAt(facing);
                                                        break;
                                                    case "Hair":
                                                        mesh = hairMeshSet.MeshAt(facing, data.flipped);
                                                        offset = quat * __instance.BaseHeadOffsetAt(facing);
                                                        break;
                                                }
                                            }
                                            else
                                                mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(__instance.graphics.nakedGraphic, facing) : bodyMesh;
                                        }
                                        /*int pattern = 0;
                                        if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                                            pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];*/
                                        /*string condition = "";
                                        if (data.hasRotting && bodyDrawType == RotDrawMode.Rotting)
                                            condition = "Rotting";
                                        if (data.hasDessicated && bodyDrawType == RotDrawMode.Dessicated)
                                            condition = "Dessicated";*/
                                        string bodyType = "";
                                        if (data.useBodyType)
                                            bodyType = ___pawn.story.bodyType.defName;
                                        Vector3 handOffset = Vector3.zero;
                                        if (!data.useStaticYOffset && !data.usePublicYOffset)
                                        {
                                            if (data.sleeveDrawHigherOfShell && data.isSleeve)
                                            {
                                                if (facing != Rot4.North)
                                                    handOffset.y = 0.02027027f - 0.008687258f + 0.0028957527f + comp.Props.apparelInterval;
                                                else
                                                    handOffset.y = 0.023166021f - 0.008687258f + 0.0028957527f + comp.Props.apparelInterval;
                                            }
                                            else if (data.isSleeve)
                                            {
                                                handOffset.y = 0.02027027f - 0.008687258f - 0.001f;
                                            }
                                        }
                                        Vector3 dataOffset = data.DrawOffsetForRot(facing);
                                        if (data.useStaticYOffset)
                                            local.y = rootLoc.y + dataOffset.y * 0.01f;
                                        if (data.usePublicYOffset)
                                            dataOffset.y *= 0.01f;
                                        else
                                            dataOffset.y *= 0.0001f;
                                        Vector3 pos = local + offset + handOffset + dataOffset;
                                        Rot4 matFacing = data.switchEastWest && (facing == Rot4.East || facing == Rot4.West) ? new Rot4(4 - facing.AsInt) : facing;
                                        Material material = data.GetGraphic(batch.keyName, apparelColor, colorTwo, "", bodyType).MatAt(matFacing, null);
                                        Material mat = (___pawn.RaceProps.IsMechanoid && ___pawn.Faction != null && ___pawn.Faction != Faction.OfMechanoids)
                                            ? __instance.graphics.GetOverlayMat(material, ___pawn.Faction.MechColor)
                                            : material;
                                        Material apparelMat = flags.FlagSet(PawnRenderFlags.Cache)
                                            ? mat
                                            : OverrideMaterialIfNeeded(__instance, mat, ___pawn, flags.FlagSet(PawnRenderFlags.Portrait));
                                        GenDraw.DrawMeshNowOrLater(mesh, pos, quat, apparelMat, flags.FlagSet(PawnRenderFlags.DrawNow));
                                        if (displayLevelInfo)
                                            Log.Warning(" " + data.originalDef + "------------" + data.textureLevelsName + ": " + pos.y.ToString());
                                    }
                                }
                            }
                        }
                        vector.y += comp.Props.apparelInterval;
                        //vector.y += 0.0028957527f;
                    }
                }
            }
            else if (!__state && flags.FlagSet(PawnRenderFlags.Clothes))
            {
                for (int i = 0; i < __instance.graphics.apparelGraphics.Count; i++)
                {
                    ApparelGraphicRecord apparel = __instance.graphics.apparelGraphics[i];
                    if ((apparel.sourceApparel.def.apparel.shellRenderedBehindHead || apparel.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell)
                        && !PawnRenderer.RenderAsPack(apparel.sourceApparel)
                        && apparel.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead
                        && apparel.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover)
                    {
                        //如果当前服装的def没在需要被隐藏的列表里的话
                        if (!comp.GetAllHideOriginalDefData.NullOrEmpty() && !comp.GetAllHideOriginalDefData.Contains(apparel.sourceApparel.def.defName))
                        {
                            //先画此服装
                            Material apparelMAt = (___pawn.RaceProps.IsMechanoid && ___pawn.Faction != null && ___pawn.Faction != Faction.OfMechanoids)
                                ? __instance.graphics.GetOverlayMat(apparel.graphic.MatAt(facing, null), ___pawn.Faction.MechColor)
                                : apparel.graphic.MatAt(facing, null);
                            Material material = flags.FlagSet(PawnRenderFlags.Cache)
                                ? apparelMAt
                                : OverrideMaterialIfNeeded(__instance, apparelMAt, ___pawn, flags.FlagSet(PawnRenderFlags.Portrait));

                            GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, material, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }
                        vector.y += comp.Props.apparelInterval;
                    }
                }
            }
            
            if (ModsConfig.IdeologyActive && __instance.graphics.bodyTattooGraphic != null && bodyDrawType != RotDrawMode.Dessicated && (facing != Rot4.North || ___pawn.style.BodyTattoo.visibleNorth))
            {
                GenDraw.DrawMeshNowOrLater(__instance.GetBodyOverlayMeshSet().MeshAt(facing), loc, quat, __instance.graphics.bodyTattooGraphic.MatAt(facing, null), flags.FlagSet(PawnRenderFlags.DrawNow));
            }
        }


        //Apparel(Shell层) BottomShell FrontShell DrawBodyApparelPrefix 
        static bool DrawBodyApparelPrefix(PawnRenderer __instance, Pawn ___pawn, Vector3 shellLoc, Vector3 utilityLoc, Mesh bodyMesh, float angle, Rot4 bodyFacing, PawnRenderFlags flags)
        {
            bool patchResult = true;
            
            MultiRenderComp comp = ___pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return true;
            //if (!comp.PrefixResolved)
                //__instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(bodyFacing);
            if (curDirection.NullOrEmpty())
                return true;

            bool displayLevelInfo = ModStaticMethod.ThisMod.apparelLevelsDisplayToggle;

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);

            NareisLib_GraphicMeshSet bodyMeshSet = null;
            NareisLib_GraphicMeshSet hairMeshSet = null;
            NareisLib_GraphicMeshSet headMeshSet = null;
            if (___pawn.RaceProps.Humanlike)
            {
                bodyMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeBodySetForPawn(___pawn);
                headMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHeadSetForPawn(___pawn);
                hairMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHairSetForPawn(___pawn);
            }

            List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;

            bool hasMultiTexApparel = !comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty() 
                && apparelGraphics.Exists(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell 
                    && !x.sourceApparel.def.apparel.shellRenderedBehindHead 
                    && comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(x.sourceApparel.def.GetType().ToStringSafe() + "_" + x.sourceApparel.def.defName));
            
            if (curDirection.ContainsKey((int)TextureRenderLayer.Apparel) && hasMultiTexApparel)
            {
                patchResult = false;

                if (displayLevelInfo)
                    Log.Warning("从 " + shellLoc.y.ToString() + " 开始渲染外套(不渲染在头后方的Shell层的衣服)");

                for (int i = 0; i < apparelGraphics.Count; i++)
                {
                    ApparelGraphicRecord apparel = apparelGraphics[i];
                    //如果是shell服装的话
                    if (apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell && !apparel.sourceApparel.def.apparel.shellRenderedBehindHead)
                    {
                        //如果当前服装的def没在需要被隐藏的列表里的话
                        if (!comp.GetAllHideOriginalDefData.NullOrEmpty() && !comp.GetAllHideOriginalDefData.Contains(apparel.sourceApparel.def.defName))
                        {
                            //先画此服装
                            Material apparelMat = apparel.graphic.MatAt(bodyFacing, null);
                            Material material = flags.FlagSet(PawnRenderFlags.Cache)
                                ? apparelMat
                                : OverrideMaterialIfNeeded(__instance, apparelMat, ___pawn, flags.FlagSet(PawnRenderFlags.Portrait));
                            Vector3 loc = shellLoc;
                            if (apparel.sourceApparel.def.apparel.shellCoversHead)
                                //loc.y += 0.0014478763f;
                                loc.y += 0.0028957527f;
                            GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quat, material, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }

                        //如果是多层服装的话
                        string apparelTypeOriginalDefName = apparel.sourceApparel.def.GetType().ToStringSafe() + "_" + apparel.sourceApparel.def.defName;
                        if (!comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty() 
                            && comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(apparelTypeOriginalDefName) 
                            && curDirection.ContainsKey((int)TextureRenderLayer.Apparel))
                        {
                            Vector3 loc = shellLoc;
                            if (apparel.sourceApparel.def.apparel.shellCoversHead)
                                //loc.y += 0.0014478763f;
                                loc.y += 0.0028957527f;
                            Color apparelColor = apparel.sourceApparel.DrawColor;

                            List<int> layers = new List<int>() { (int)TextureRenderLayer.Apparel };
                            if (curDirection.ContainsKey((int)TextureRenderLayer.BottomShell))
                                layers = new List<int>() { (int)TextureRenderLayer.BottomShell, (int)TextureRenderLayer.Apparel };
                            
                            foreach (int layer in layers)
                            {
                                Vector3 local = loc;
                                if (layer == (int)TextureRenderLayer.BottomShell)
                                {
                                    local.y = shellLoc.y - (bodyFacing != Rot4.North ? 0.02027027f : 0.023166021f) + 0.005687258f;
                                    if (displayLevelInfo)
                                        Log.Warning(" BottomShell层: 从" + local.y.ToString() + "开始");
                                }
                                else
                                {
                                    if (displayLevelInfo)
                                        Log.Warning(" Apparel(Shell)层: 从" + local.y.ToString() + "开始");
                                }

                                foreach (MultiTexBatch batch in curDirection[layer])
                                {
                                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                                    //string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;
                                    string mlPrefixName = batch.multiTexDefName + "_";

                                    TextureLevels data;
                                    if (typeOriginalDefName == apparelTypeOriginalDefName
                                        && comp.TryGetStoredTextureLevels(typeOriginalDefName, batch.textureLevelsName, out data)
                                        && (comp.cachedHideOrReplaceDict.NullOrEmpty()
                                            || !comp.cachedHideOrReplaceDict.ContainsKey(mlPrefixName + data.textureLevelsName)
                                            || !comp.cachedHideOrReplaceDict[mlPrefixName + data.textureLevelsName].hide)
                                        && data.CanRender(___pawn, batch.keyName))
                                    {

                                        apparelColor = data.useStaticColor ? data.color : apparelColor;
                                        Color colorTwo = data.useStaticColor ? data.colorTwo : Color.white;

                                        Mesh mesh = null;

                                        Vector3 offset = Vector3.zero;
                                        if (data.meshSize != Vector2.zero)
                                        {
                                            mesh = NareisLib_MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(bodyFacing, data.flipped);
                                            switch (data.meshType)
                                            {
                                                case "Head": offset = quat * __instance.BaseHeadOffsetAt(bodyFacing); break;
                                                case "Hair": offset = quat * __instance.BaseHeadOffsetAt(bodyFacing); break;
                                            }
                                        }
                                        else
                                        {
                                            if (___pawn.RaceProps.Humanlike)
                                            {
                                                switch (data.meshType)
                                                {
                                                    case "Body": 
                                                        mesh = bodyMeshSet.MeshAt(bodyFacing, data.flipped); 
                                                        break;
                                                    case "Head":
                                                        mesh = headMeshSet.MeshAt(bodyFacing, data.flipped);
                                                        offset = quat * __instance.BaseHeadOffsetAt(bodyFacing);
                                                        break;
                                                    case "Hair":
                                                        mesh = hairMeshSet.MeshAt(bodyFacing, data.flipped);
                                                        offset = quat * __instance.BaseHeadOffsetAt(bodyFacing);
                                                        break;
                                                }
                                            }
                                            else
                                                mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(__instance.graphics.nakedGraphic, bodyFacing) : bodyMesh;
                                        }
                                        //Log.Warning("has mesh mesh mesh : " + (bodyMesh != null).ToStringSafe());
                                        /*int pattern = 0;
                                        if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                                            pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];*/
                                        string bodyType = "";
                                        if (data.useBodyType)
                                            bodyType = ___pawn.story.bodyType.defName;
                                        Vector3 dataOffset = data.DrawOffsetForRot(bodyFacing);
                                        if (data.useStaticYOffset)
                                            local.y = shellLoc.y - (bodyFacing != Rot4.North ? 0.02027027f : 0.023166021f) + dataOffset.y * 0.01f;
                                        if (data.usePublicYOffset)
                                            dataOffset.y *= 0.01f;
                                        else
                                            dataOffset.y *= 0.0001f;
                                        Vector3 pos = local + offset + dataOffset;
                                        Rot4 matFacing = data.switchEastWest && (bodyFacing == Rot4.East || bodyFacing == Rot4.West) ? new Rot4(4 - bodyFacing.AsInt) : bodyFacing;
                                        Material material = data.GetGraphic(batch.keyName, apparelColor, colorTwo, "" , bodyType).MatAt(matFacing, null);
                                        Material mat = flags.FlagSet(PawnRenderFlags.Cache)
                                            ? material
                                            : OverrideMaterialIfNeeded(__instance, material, ___pawn, flags.FlagSet(PawnRenderFlags.Portrait));
                                        GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                                        if (displayLevelInfo)
                                            Log.Warning(" " + data.originalDef + "------------" + data.textureLevelsName + ": " + pos.y.ToString());
                                    }
                                }
                            }
                        }
                    }

                    //渲染背包/工具层
                    if (PawnRenderer.RenderAsPack(apparel.sourceApparel) && !comp.GetAllHideOriginalDefData.NullOrEmpty() && !comp.GetAllHideOriginalDefData.Contains(apparel.sourceApparel.def.defName))
                    {
                        Material material2 = apparel.graphic.MatAt(bodyFacing, null);
                        material2 = (flags.FlagSet(PawnRenderFlags.Cache) ? material2 : OverrideMaterialIfNeeded(material2, ___pawn, __instance, flags.FlagSet(PawnRenderFlags.Portrait)));
                        if (apparel.sourceApparel.def.apparel.wornGraphicData != null)
                        {
                            Vector2 vector = apparel.sourceApparel.def.apparel.wornGraphicData.BeltOffsetAt(bodyFacing, ___pawn.story.bodyType);
                            Vector2 vector2 = apparel.sourceApparel.def.apparel.wornGraphicData.BeltScaleAt(bodyFacing, ___pawn.story.bodyType);
                            Matrix4x4 matrix = Matrix4x4.Translate(utilityLoc) * Matrix4x4.Rotate(quat) * Matrix4x4.Translate(new Vector3(vector.x, 0f, vector.y)) * Matrix4x4.Scale(new Vector3(vector2.x, 1f, vector2.y));
                            GenDraw.DrawMeshNowOrLater(bodyMesh, matrix, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }
                        else
                        {
                            GenDraw.DrawMeshNowOrLater(bodyMesh, shellLoc, quat, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }
                    }
                }
            }

            if (curDirection.ContainsKey((int)TextureRenderLayer.FrontShell))
            {
                Vector3 loc = shellLoc;
                loc.y = shellLoc.y - (bodyFacing != Rot4.North ? 0.02027027f : 0.023166021f) + 0.0304054035f;

                if (displayLevelInfo)
                    Log.Warning(" FrontShell层: 从" + loc.y.ToString() + "开始");

                foreach (MultiTexBatch batch in curDirection[(int)TextureRenderLayer.FrontShell])
                {
                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                    //string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;
                    string mlPrefixName = batch.multiTexDefName + "_";

                    TextureLevels data;
                    if (!comp.TryGetStoredTextureLevels(typeOriginalDefName, batch.textureLevelsName, out data)
                        || (!comp.cachedHideOrReplaceDict.NullOrEmpty()
                            && comp.cachedHideOrReplaceDict.ContainsKey(mlPrefixName + data.textureLevelsName)
                            && comp.cachedHideOrReplaceDict[mlPrefixName + data.textureLevelsName].hide)
                        || !data.CanRender(___pawn, batch.keyName))
                        continue;

                    Apparel apparel = apparelGraphics.FirstOrDefault(x => x.sourceApparel.def.defName == batch.originalDefName).sourceApparel;
                    Color apparelColor = (apparel == null || data.useStaticColor) ? data.color : apparel.DrawColor;
                    Color colorTwo = data.useStaticColor ? data.colorTwo : Color.white;
                    Mesh mesh = null;

                    Vector3 offset = Vector3.zero;
                    if (data.meshSize != Vector2.zero)
                    {
                        mesh = NareisLib_MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(bodyFacing, data.flipped);
                        switch (data.meshType)
                        {
                            case "Head": offset = quat * __instance.BaseHeadOffsetAt(bodyFacing); break;
                            case "Hair": offset = quat * __instance.BaseHeadOffsetAt(bodyFacing); break;
                        }
                    }
                    else
                    {
                        if (___pawn.RaceProps.Humanlike)
                        {
                            switch (data.meshType)
                            {
                                case "Body":
                                    mesh = bodyMeshSet.MeshAt(bodyFacing, data.flipped);
                                    break;
                                case "Head":
                                    mesh = headMeshSet.MeshAt(bodyFacing, data.flipped);
                                    offset = quat * __instance.BaseHeadOffsetAt(bodyFacing);
                                    break;
                                case "Hair":
                                    mesh = hairMeshSet.MeshAt(bodyFacing, data.flipped);
                                    offset = quat * __instance.BaseHeadOffsetAt(bodyFacing);
                                    break;
                            }
                        }
                        else
                            mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(__instance.graphics.nakedGraphic, bodyFacing) : bodyMesh;
                    }
                    //Log.Warning("has mesh mesh mesh : " + (bodyMesh != null).ToStringSafe());
                    /*int pattern = 0;
                    if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                        pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];*/
                    string bodyType = "";
                    if (data.useBodyType)
                        bodyType = ___pawn.story.bodyType.defName;
                    Vector3 dataOffset = data.DrawOffsetForRot(bodyFacing);
                    if (data.useStaticYOffset)
                        loc.y = shellLoc.y - (bodyFacing != Rot4.North ? 0.02027027f : 0.023166021f) + dataOffset.y * 0.01f;
                    if (data.usePublicYOffset)
                        dataOffset.y *= 0.01f;
                    else
                        dataOffset.y *= 0.0001f;
                    Vector3 pos = loc + offset + dataOffset;
                    Rot4 matFacing = data.switchEastWest && (bodyFacing == Rot4.East || bodyFacing == Rot4.West) ? new Rot4(4 - bodyFacing.AsInt) : bodyFacing;
                    Material material = data.GetGraphic(batch.keyName, apparelColor, colorTwo, "", bodyType).MatAt(matFacing, null);
                    Material mat = flags.FlagSet(PawnRenderFlags.Cache)
                        ? material
                        : OverrideMaterialIfNeeded(__instance, material, ___pawn, flags.FlagSet(PawnRenderFlags.Portrait));
                    GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    if (displayLevelInfo)
                        Log.Warning(" " + data.originalDef + "------------" + data.textureLevelsName + ": " + pos.y.ToString());
                }
            }

            return patchResult;
        }



        //下面转释器方法的子方法，用于覆盖隐形贴图和受伤贴图等，特定用于头部
        public static Material GetHeadOverrideMat(Material mat, PawnRenderer instance, bool portrait = false, bool allowOverride = true)
        {
            Material material = mat;
            if (material != null && allowOverride)
            {
                if (!portrait && instance.graphics.pawn.IsInvisible())
                {
                    material = InvisibilityMatPool.GetInvisibleMat(material);
                }
                material = instance.graphics.flasher.GetDamagedMat(material);
            }
            return material;
        }




        //Head RenderPawnInternalTranspiler
        public static IEnumerable<CodeInstruction> RenderPawnInternalHeadPatchTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo drawMeshNowOrLater = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) }, null);
            FieldInfo pawn = AccessTools.Field(typeof(PawnRenderer), "pawn");
            MethodInfo renderPawnInternalHeadTranPatch = AccessTools.Method(typeof(PawnRenderPatchs), "RenderPawnInternalHeadTranPatch", null, null);
            List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
            int num;
            for (int i = 0; i < instructionList.Count; i = num + 1)
            {
                CodeInstruction instruction = instructionList[i];
                //将其插入到绘制头部贴图的DrawMeshNowOrLater前一行，并跳过原方法
                if (instruction.OperandIs(drawMeshNowOrLater))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_3);//headYOffset

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);//bodyDrawType

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4);//facing

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 6);//flags

                    yield return new CodeInstruction(OpCodes.Ldarg_0);//this.

                    yield return new CodeInstruction(OpCodes.Ldfld, pawn);//pawn

                    yield return new CodeInstruction(OpCodes.Ldarg_0);//PawnRenderer instance

                    yield return new CodeInstruction(OpCodes.Call, renderPawnInternalHeadTranPatch);

                    i++;
                }
                yield return instructionList[i];
                num = i;
            }
            yield break;
        }

        public static void RenderPawnInternalHeadTranPatch(Mesh headMesh, Vector3 loc, Quaternion quat, Material headMat, bool drawNow, Vector3 headYOffset, RotDrawMode bodyDrawType, Rot4 facing, PawnRenderFlags flags, Pawn pawn, PawnRenderer instance)
        {
            //Log.Warning("run head patch");
            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
            //if (comp != null && !comp.PrefixResolved)
            //instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp != null ? comp.GetDataOfDirection(facing) : new Dictionary<int, List<MultiTexBatch>>();

            bool displayLevelInfo = ModStaticMethod.ThisMod.apparelLevelsDisplayToggle;

            int layer = (int)TextureRenderLayer.Head;

            //是否绘制原head贴图
            if (comp == null
                || curDirection.NullOrEmpty()
                || !curDirection.ContainsKey(layer) 
                || comp.GetAllHideOriginalDefData.NullOrEmpty() 
                || !comp.GetAllHideOriginalDefData.Contains("Head"))
            {
                GenDraw.DrawMeshNowOrLater(headMesh, loc, quat, headMat, drawNow);
            }

            if (comp == null)
                return;

            //绘制多层贴图
            if (!curDirection.NullOrEmpty() && curDirection.ContainsKey(layer))
            {
                if (displayLevelInfo)
                {
                    Log.Warning(" ");
                    Log.Warning("从 " + headYOffset.y.ToString() + " 开始渲染头部图层");
                    Log.Warning(" Head层: 从" + headYOffset.y.ToString() + "开始");
                } 

                Mesh bodyMesh = null;
                NareisLib_GraphicMeshSet bodyMeshSet = null;
                NareisLib_GraphicMeshSet hairMeshSet = null;
                NareisLib_GraphicMeshSet headMeshSet = null;
                if (pawn.RaceProps.Humanlike)
                {
                    bodyMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeBodySetForPawn(pawn);
                    headMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
                    hairMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHairSetForPawn(pawn);
                }
                else
                    bodyMesh = instance.graphics.nakedGraphic.MeshAt(facing);

                Color colorOne = alienComp == null ? pawn.story.SkinColor : alienComp.GetChannel("skin").first;
                Color colorTwo = alienComp == null ? Color.white : alienComp.GetChannel("skin").second;

                foreach (MultiTexBatch batch in curDirection[layer])
                {
                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                    //string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;
                    string mlPrefixName = batch.multiTexDefName + "_";

                    TextureLevels data;
                    if (!comp.TryGetStoredTextureLevels(typeOriginalDefName, batch.textureLevelsName, out data)
                        || (!comp.cachedHideOrReplaceDict.NullOrEmpty()
                            && comp.cachedHideOrReplaceDict.ContainsKey(mlPrefixName + data.textureLevelsName)
                            && comp.cachedHideOrReplaceDict[mlPrefixName + data.textureLevelsName].hide)
                        || !data.CanRender(pawn, batch.keyName))
                        continue;

                    colorOne = data.useStaticColor ? data.color : colorOne;
                    colorTwo = data.useStaticColor ? data.colorTwo : colorTwo;

                    Mesh mesh = null;
                    Vector3 offset = Vector3.zero;
                    if (data.meshSize != Vector2.zero)
                    {
                        mesh = NareisLib_MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing, data.flipped);
                        switch (data.meshType)
                        {
                            case "Head": offset = quat * instance.BaseHeadOffsetAt(facing); break;
                            case "Hair": offset = quat * instance.BaseHeadOffsetAt(facing); break;
                        }
                    }
                    else
                    {
                        if (pawn.RaceProps.Humanlike)
                        {
                            switch (data.meshType)
                            {
                                case "Body": 
                                    mesh = bodyMeshSet.MeshAt(facing, data.flipped); 
                                    break;
                                case "Head":
                                    mesh = data.flipped ? headMeshSet.MeshAt(facing, data.flipped) : headMesh;
                                    offset = quat * instance.BaseHeadOffsetAt(facing);
                                    break;
                                case "Hair":
                                    mesh = hairMeshSet.MeshAt(facing, data.flipped);
                                    offset = quat * instance.BaseHeadOffsetAt(facing);
                                    break;
                            }
                        }
                        else
                            mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(instance.graphics.nakedGraphic, facing) : bodyMesh;
                    }
                    /*int pattern = 0;
                    if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                        pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];*/
                    string condition = "";
                    if (flags.FlagSet(PawnRenderFlags.HeadStump))
                    {
                        if (data.hasStump)
                            condition = "Stump";
                        else
                            continue;
                    }
                    if (data.hasRotting && bodyDrawType == RotDrawMode.Rotting)
                        condition = "Rotting";
                    if (data.hasDessicated && bodyDrawType == RotDrawMode.Dessicated)
                        condition = "Dessicated";
                    string headType = "";
                    if (data.useHeadType)
                        headType = pawn.story.headType.defName;
                    Vector3 dataOffset = data.DrawOffsetForRot(facing);
                    if (data.useStaticYOffset)
                        headYOffset.y = headYOffset.y - (facing != Rot4.North ? 0.023166021f : 0.02027027f) - dataOffset.y * 0.01f;
                    if (data.usePublicYOffset)
                        dataOffset.y *= 0.01f;
                    else
                        dataOffset.y *= 0.0001f;
                    Vector3 pos = headYOffset + offset + dataOffset;
                    Rot4 matFacing = data.switchEastWest && (facing == Rot4.East || facing == Rot4.West) ? new Rot4(4 - facing.AsInt) : facing;
                    Material material = data.GetGraphic(batch.keyName, colorOne, colorTwo, condition, "", headType).MatAt(matFacing, null);
                    Material mat = GetHeadOverrideMat(material, instance, flags.FlagSet(PawnRenderFlags.Portrait), !flags.FlagSet(PawnRenderFlags.Cache));
                    GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, drawNow);
                    if (displayLevelInfo)
                        Log.Warning(" " + data.originalDef + "------------" + data.textureLevelsName + ": " + pos.y.ToString());
                }
            }
        }




        //下面的转释器方法的子方法，用于覆盖隐形贴图和效果贴图等，特定用于头发
        public static Material GetHairOverrideMat(Material mat, PawnRenderer instance, bool portrait = false, bool cached = true)
        {
            Material material = mat;
            if (!portrait && instance.graphics.pawn.IsInvisible())
            {
                material = InvisibilityMatPool.GetInvisibleMat(material);
            }
            if (!cached)
            {
                return instance.graphics.flasher.GetDamagedMat(material);
            }
            return material;
        }



        //FaceMask BottomHair Hair HeadMask Hat ，头发显示控制DrawHeadHairTranspiler
        public static IEnumerable<CodeInstruction> DrawHeadHairPatchTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo drawMeshNowOrLater = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) }, null);
            FieldInfo pawn = AccessTools.Field(typeof(PawnRenderer), "pawn");
            MethodInfo drawHeadHairHeadTranPatch = AccessTools.Method(typeof(PawnRenderPatchs), "DrawHeadHairHairTranPatch", null, null);
            MethodInfo drawHeadHairFaceMaskTranPatch = AccessTools.Method(typeof(PawnRenderPatchs), "DrawHeadHairFaceMaskTranPatch", null, null);
            MethodInfo drawHeadHairHeadMaskTranPatch = AccessTools.Method(typeof(PawnRenderPatchs), "DrawHeadHairHeadMaskTranPatch", null, null);
            MethodInfo drawHeadHairDisplaySwitchPatch = AccessTools.Method(typeof(PawnRenderPatchs), "DrawHeadHairDisplaySwitchPatch", null, null);
            List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
            int num;
            for (int i = 0; i < instructionList.Count; i = num + 1)
            {
                CodeInstruction instruction = instructionList[i];
                if (i > 10 && instructionList[i - 1].opcode == OpCodes.Brfalse_S && instructionList[i - 2].opcode == OpCodes.Ldloc_S /*&& instructionList[i - 2].OperandIs(6)*/ && instructionList[i - 3].OperandIs(drawMeshNowOrLater))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_3);//angle

                    yield return new CodeInstruction(OpCodes.Ldarg_1);//rootLoc vector

                    yield return new CodeInstruction(OpCodes.Ldarg_2);//headOffset

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);//headfacing

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 7);//flags

                    yield return new CodeInstruction(OpCodes.Ldloc_1);//apparelGraphics

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);//shouldDraw

                    yield return new CodeInstruction(OpCodes.Ldarg_0);//this.

                    yield return new CodeInstruction(OpCodes.Ldfld, pawn);//pawn

                    yield return new CodeInstruction(OpCodes.Ldarg_0);//PawnRenderer instance

                    yield return new CodeInstruction(OpCodes.Call, drawHeadHairFaceMaskTranPatch);

                    i += 34;
                }

                if (i > 10 && instructionList[i - 1].opcode == OpCodes.Ldloc_2 && instructionList[i + 1].opcode == OpCodes.Ldarg_0)
                {
                    yield return new CodeInstruction(OpCodes.Call, drawHeadHairDisplaySwitchPatch);
                }

                if (i > 40 && instruction.OperandIs(drawMeshNowOrLater) && instructionList[i - 37].opcode == OpCodes.Ldloc_2/* && instructionList[i + 4].OperandIs(6)*/)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);//rootLoc vector

                    yield return new CodeInstruction(OpCodes.Ldarg_2);//headOffset

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);//headfacing

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 7);//flags

                    yield return new CodeInstruction(OpCodes.Ldarg_0);//this.

                    yield return new CodeInstruction(OpCodes.Ldfld, pawn);//pawn

                    yield return new CodeInstruction(OpCodes.Ldarg_0);//PawnRenderer instance

                    yield return new CodeInstruction(OpCodes.Call, drawHeadHairHeadTranPatch);

                    i++;
                }

                if (i > 10 && instructionList[i - 1].opcode == OpCodes.Brfalse && instructionList[i - 2].opcode == OpCodes.Ldloc_S/* && instructionList[i - 2].OperandIs(6)*/ && instructionList[i - 6].OperandIs(drawMeshNowOrLater))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_3);//angle

                    yield return new CodeInstruction(OpCodes.Ldarg_1);//rootLoc vector

                    yield return new CodeInstruction(OpCodes.Ldarg_2);//headOffset

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 5);//headfacing

                    yield return new CodeInstruction(OpCodes.Ldarg_S, 7);//flags

                    yield return new CodeInstruction(OpCodes.Ldloc_1);//apparelGraphics

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);//shouldDraw

                    yield return new CodeInstruction(OpCodes.Ldarg_0);//this.

                    yield return new CodeInstruction(OpCodes.Ldfld, pawn);//pawn

                    yield return new CodeInstruction(OpCodes.Ldarg_0);//PawnRenderer instance

                    yield return new CodeInstruction(OpCodes.Call, drawHeadHairHeadMaskTranPatch);

                    i += 52;
                }
                yield return instructionList[i];
                num = i;
            }
            yield break;
        }

        public static void DrawHeadHairFaceMaskTranPatch(float angle, Vector3 vector, Vector3 headOffset, Rot4 facing, PawnRenderFlags flags, List<ApparelGraphicRecord> apparelGraphics, bool shouldDraw, Pawn pawn, PawnRenderer instance)
        {
            //Log.Warning("HeadGear RunPatched");

            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            
            //if (comp != null && !comp.PrefixResolved)
                //instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp != null ? comp.GetDataOfDirection(facing) : new Dictionary<int, List<MultiTexBatch>>();

            Mesh bodyMesh = null;
            NareisLib_GraphicMeshSet bodyMeshSet = null;
            NareisLib_GraphicMeshSet hairMeshSet = null;
            NareisLib_GraphicMeshSet headMeshSet = null;
            if (pawn.RaceProps.Humanlike)
            {
                bodyMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeBodySetForPawn(pawn);
                headMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
                hairMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHairSetForPawn(pawn);
            }
            else
                bodyMesh = instance.graphics.nakedGraphic.MeshAt(facing);

            bool displayLevelInfo = ModStaticMethod.ThisMod.apparelLevelsDisplayToggle;

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 hairYOffset = vector + headOffset;
            hairYOffset.y += 0.026957527f;

            if (displayLevelInfo)
                Log.Warning(" FaceMask层: 从" + hairYOffset.y.ToString() + "开始");

            int layer = (int)TextureRenderLayer.FaceMask;

            for (int index = 0; index < apparelGraphics.Count; index++)
            {
                ApparelGraphicRecord apparel = apparelGraphics[index];
                if ((!shouldDraw || apparel.sourceApparel.def.apparel.hatRenderedFrontOfFace) && apparel.sourceApparel.def.apparel.forceRenderUnderHair)
                {
                    Vector3 loc = hairYOffset;
                    if (apparel.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                    {
                        loc = vector + headOffset;
                        if (apparel.sourceApparel.def.apparel.hatRenderedBehindHead)
                            loc.y += 0.02216602f;
                        else
                            loc.y += !(facing == Rot4.North) || apparel.sourceApparel.def.apparel.hatRenderedAboveBody ? 0.03085328f : 0.002895753f;
                    }
                    //是否绘制原装备的贴图
                    if (comp == null
                        || comp.GetAllHideOriginalDefData.NullOrEmpty() 
                        || !comp.GetAllHideOriginalDefData.Contains(apparel.sourceApparel.def.defName))
                    {
                        Material original = apparel.graphic.MatAt(facing, null);
                        Material mat = flags.FlagSet(PawnRenderFlags.Cache) ? original : OverrideMaterialIfNeeded(original, pawn, instance, flags.FlagSet(PawnRenderFlags.Portrait));
                        GenDraw.DrawMeshNowOrLater(hairMeshSet.MeshAt(facing), loc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }

                    if (comp == null)
                        continue;

                    //如果是多层服装的话
                    string apparelTypeOriginalDefName = apparel.sourceApparel.def.GetType().ToStringSafe() + "_" + apparel.sourceApparel.def.defName;
                    if (!curDirection.NullOrEmpty() && curDirection.ContainsKey(layer) 
                        && !comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty() 
                        && comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(apparelTypeOriginalDefName))
                    {
                        Color apparelColor = apparel.sourceApparel.DrawColor;
                        foreach (MultiTexBatch batch in curDirection[layer])
                        {
                            string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                            //string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;
                            string mlPrefixName = batch.multiTexDefName + "_";

                            TextureLevels data;
                            if (typeOriginalDefName == apparelTypeOriginalDefName
                                && comp.TryGetStoredTextureLevels(typeOriginalDefName, batch.textureLevelsName, out data)
                                && (comp.cachedHideOrReplaceDict.NullOrEmpty()
                                    || !comp.cachedHideOrReplaceDict.ContainsKey(mlPrefixName + data.textureLevelsName)
                                    || !comp.cachedHideOrReplaceDict[mlPrefixName + data.textureLevelsName].hide)
                                && data.CanRender(pawn, batch.keyName))
                            {
                                apparelColor = data.useStaticColor ? data.color : apparelColor;
                                Color colorTwo = data.useStaticColor ? data.colorTwo : Color.white;

                                Mesh mesh = null;
                                if (data.meshSize != Vector2.zero)
                                {
                                    mesh = NareisLib_MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing, data.flipped);
                                    if (data.meshType == "Body")
                                    {
                                        loc.x = vector.x;
                                        loc.z = vector.z;
                                    }
                                }
                                else
                                {
                                    if (pawn.RaceProps.Humanlike)
                                    {
                                        switch (data.meshType)
                                        {
                                            case "Body": 
                                                mesh = bodyMeshSet.MeshAt(facing, data.flipped);
                                                loc.x = vector.x;
                                                loc.z = vector.z;
                                                break;
                                            case "Head": mesh = headMeshSet.MeshAt(facing, data.flipped); break;
                                            case "Hair": mesh = hairMeshSet.MeshAt(facing, data.flipped); break;
                                        }
                                    }
                                    else
                                    {
                                        mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(instance.graphics.nakedGraphic, facing) : bodyMesh;
                                        loc.x = vector.x;
                                        loc.z = vector.z;
                                    }
                                }                                        
                                /*int pattern = 0;
                                if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                                    pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];*/
                                string headType = "";
                                if (data.useHeadType)
                                    headType = pawn.story.headType.defName;
                                Vector3 dataOffset = data.DrawOffsetForRot(facing);
                                if (data.useStaticYOffset)
                                    loc.y = vector.y + dataOffset.y * 0.01f;
                                if (data.usePublicYOffset)
                                    dataOffset.y *= 0.01f;
                                else
                                    dataOffset.y *= 0.0001f;
                                Vector3 pos = loc + dataOffset;
                                Rot4 matFacing = data.switchEastWest && (facing == Rot4.East || facing == Rot4.West) ? new Rot4(4 - facing.AsInt) : facing;
                                Material material = data.GetGraphic(batch.keyName, apparelColor, colorTwo, "", "", headType).MatAt(matFacing, null);
                                Material mat = flags.FlagSet(PawnRenderFlags.Cache)
                                    ? material
                                    : OverrideMaterialIfNeeded(instance, material, pawn, flags.FlagSet(PawnRenderFlags.Portrait));
                                GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                                if (displayLevelInfo)
                                    Log.Warning(" " + data.originalDef + "------------" + data.textureLevelsName + ": " + pos.y.ToString());
                            }
                        }
                    }
                }
            }
        }

        public static void DrawHeadHairHairTranPatch(Mesh hairMesh, Vector3 loc, Quaternion quat, Material hairMat, bool drawNow, Vector3 vector, Vector3 headOffset, Rot4 facing, PawnRenderFlags flags, Pawn pawn, PawnRenderer instance)
        {
            //Log.Warning("Hair RunPatched");

            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();

            //if (comp != null && !comp.PrefixResolved)
                //instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp != null ? comp.GetDataOfDirection(facing) : new Dictionary<int, List<MultiTexBatch>>();

            bool displayLevelInfo = ModStaticMethod.ThisMod.apparelLevelsDisplayToggle;

            //是否绘制原头发贴图
            if (comp == null
                || curDirection.NullOrEmpty()
                || !curDirection.ContainsKey((int)TextureRenderLayer.Hair) 
                || comp.GetAllHideOriginalDefData.NullOrEmpty() 
                || !comp.GetAllHideOriginalDefData.Contains("Hair"))
            {
                GenDraw.DrawMeshNowOrLater(hairMesh, loc, quat, hairMat, drawNow);
            }

            if (comp == null || curDirection.NullOrEmpty())
                return;

            Vector3 hairYOffset = vector + headOffset;
            Mesh bodyMesh = null;
            NareisLib_GraphicMeshSet bodyMeshSet = null;
            NareisLib_GraphicMeshSet hairMeshSet = null;
            NareisLib_GraphicMeshSet headMeshSet = null;
            if (pawn.RaceProps.Humanlike)
            {
                bodyMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeBodySetForPawn(pawn);
                headMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
                hairMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHairSetForPawn(pawn);
            }
            else
                bodyMesh = instance.graphics.nakedGraphic.MeshAt(facing);

            Color colorOne = pawn.story.HairColor;
            Color colorTwo = alienComp != null ? alienComp.GetChannel("hair").second : Color.white;

            List<int> renderLayers = new List<int>() { (int)TextureRenderLayer.BottomHair, (int)TextureRenderLayer.Hair };

            foreach (int layer in renderLayers)
            {
                Vector3 hairPos = hairYOffset;
                if (layer == (int)TextureRenderLayer.BottomHair)
                {
                    hairPos.y += 0.004687258f;
                    if (displayLevelInfo)
                        Log.Warning(" BottomHair层: 从" + hairPos.y.ToString() + "开始");
                }
                else
                {
                    hairPos.y += 0.028957527f;
                    if (displayLevelInfo)
                        Log.Warning(" Hair层: 从" + hairPos.y.ToString() + "开始");
                }

                //绘制多层头发贴图
                if (curDirection.ContainsKey(layer))
                {

                    foreach (MultiTexBatch batch in curDirection[layer])
                    {
                        string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                        //string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;
                        string mlPrefixName = batch.multiTexDefName + "_";

                        TextureLevels data;
                        if (!comp.TryGetStoredTextureLevels(typeOriginalDefName, batch.textureLevelsName, out data)
                            || (!comp.cachedHideOrReplaceDict.NullOrEmpty()
                                && comp.cachedHideOrReplaceDict.ContainsKey(mlPrefixName + data.textureLevelsName)
                                && comp.cachedHideOrReplaceDict[mlPrefixName + data.textureLevelsName].hide)
                            || !data.CanRender(pawn, batch.keyName))
                            continue;

                        colorOne = data.useStaticColor ? data.color : colorOne;
                        colorTwo = data.useStaticColor ? data.colorTwo : colorTwo;

                        Mesh mesh = null;
                        
                        if (data.meshSize != Vector2.zero)
                        {
                            mesh = NareisLib_MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing, data.flipped);
                            if (data.meshType == "Body")
                            {
                                hairPos.x = vector.x;
                                hairPos.z = vector.z;
                            }
                        }
                        else
                        {
                            if (pawn.RaceProps.Humanlike)
                            {
                                switch (data.meshType)
                                {
                                    case "Body":
                                        mesh = bodyMeshSet.MeshAt(facing, data.flipped);
                                        hairPos.x = vector.x;
                                        hairPos.z = vector.z;
                                        break;
                                    case "Head": mesh = headMeshSet.MeshAt(facing, data.flipped); break;
                                    case "Hair": 
                                        mesh = data.flipped ? hairMeshSet.MeshAt(facing, data.flipped) : hairMesh; 
                                        break;
                                }
                            }
                            else
                            {
                                mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(instance.graphics.nakedGraphic, facing) : bodyMesh;
                                hairPos.x = vector.x;
                                hairPos.z = vector.z;
                            }
                        }
                        /*int pattern = 0;
                        if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                            pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];*/
                        string headType = "";
                        if (data.useHeadType)
                            headType = pawn.story.headType.defName;
                        Vector3 dataOffset = data.DrawOffsetForRot(facing);
                        //Log.Warning("当前使用facing：" + facing.ToStringHuman());
                        //Log.Warning("当前方向图层偏移：" + dataOffset.ToStringSafe());
                        if (data.useStaticYOffset)
                            hairPos.y = vector.y + dataOffset.y * 0.01f;
                        if (data.usePublicYOffset)
                            dataOffset.y *= 0.01f;
                        else
                            dataOffset.y *= 0.0001f;
                        Vector3 pos = hairPos + dataOffset;
                        Rot4 matFacing = data.switchEastWest && (facing == Rot4.East || facing == Rot4.West) ? new Rot4(4 - facing.AsInt) : facing;
                        Material material = data.GetGraphic(batch.keyName, colorOne, colorTwo, "", "", headType).MatAt(matFacing, null);
                        Material mat = GetHairOverrideMat(material, instance, flags.FlagSet(PawnRenderFlags.Portrait), !flags.FlagSet(PawnRenderFlags.Cache));
                        GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, drawNow);
                        if (displayLevelInfo)
                            Log.Warning(" " + data.originalDef + "------------" + data.textureLevelsName + ": " + pos.y.ToString());
                    }
                }
            }

            
        }

        public static void DrawHeadHairHeadMaskTranPatch(float angle, Vector3 vector, Vector3 headOffset, Rot4 facing, PawnRenderFlags flags, List<ApparelGraphicRecord> apparelGraphics, bool shouldDraw, Pawn pawn, PawnRenderer instance)
        {
            //Log.Warning("Hat RunPatched");

            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            
            //if (comp != null && !comp.PrefixResolved)
                //instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp != null ? comp.GetDataOfDirection(facing) : new Dictionary<int, List<MultiTexBatch>>();

            Mesh bodyMesh = null;
            NareisLib_GraphicMeshSet bodyMeshSet = null;
            NareisLib_GraphicMeshSet hairMeshSet = null;
            NareisLib_GraphicMeshSet headMeshSet = null;
            if (pawn.RaceProps.Humanlike)
            {
                bodyMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeBodySetForPawn(pawn);
                headMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
                hairMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHairSetForPawn(pawn);
            }
            else
                bodyMesh = instance.graphics.nakedGraphic.MeshAt(facing);

            bool displayLevelInfo = ModStaticMethod.ThisMod.apparelLevelsDisplayToggle;

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 hairYOffset = vector + headOffset;
            //hairYOffset.y += 0.028957527f;
            hairYOffset.y += 0.030405403f;

            if (displayLevelInfo)
                Log.Warning(" HeadMask, Hat层: 从" + hairYOffset.y.ToString() + "开始");

            for (int index = 0; index < apparelGraphics.Count; index++)
            {
                ApparelGraphicRecord apparel = apparelGraphics[index];
                if ((!shouldDraw || apparel.sourceApparel.def.apparel.hatRenderedFrontOfFace) 
                    && (apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) 
                    && !apparel.sourceApparel.def.apparel.forceRenderUnderHair)
                {
                    Vector3 loc = hairYOffset;
                    if (apparel.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                    {
                        loc = vector + headOffset;
                        if (apparel.sourceApparel.def.apparel.hatRenderedBehindHead)
                            //loc.y += 0.02216602f;
                            loc.y += 0.0236138963f;
                        else
                            loc.y += !(facing == Rot4.North) || apparel.sourceApparel.def.apparel.hatRenderedAboveBody ? 0.03185328f : 0.0028957527f;
                    }
                    //是否绘制原装备的贴图
                    if (comp == null
                        || comp.GetAllHideOriginalDefData.NullOrEmpty() 
                        || !comp.GetAllHideOriginalDefData.Contains(apparel.sourceApparel.def.defName))
                    {
                        Material original = apparel.graphic.MatAt(facing, null);
                        Material mat = flags.FlagSet(PawnRenderFlags.Cache) ? original : OverrideMaterialIfNeeded(original, pawn, instance, flags.FlagSet(PawnRenderFlags.Portrait));
                        GenDraw.DrawMeshNowOrLater(hairMeshSet.MeshAt(facing), loc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }

                    if (comp == null)
                        continue;

                    //如果是多层服装的话
                    string apparelTypeOriginalDefName = apparel.sourceApparel.def.GetType().ToStringSafe() + "_" + apparel.sourceApparel.def.defName;
                    if (!curDirection.NullOrEmpty()
                        && !comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty() 
                        && comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(apparelTypeOriginalDefName))
                    {
                        List<int> renderLayers = new List<int>() { (int)TextureRenderLayer.HeadMask, (int)TextureRenderLayer.Hat };
                        foreach (int layer in renderLayers)
                        {
                            if (curDirection.ContainsKey(layer))
                            {
                                Color apparelColor = apparel.sourceApparel.DrawColor;
                                foreach (MultiTexBatch batch in curDirection[layer])
                                {
                                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                                    //string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;
                                    string mlPrefixName = batch.multiTexDefName + "_";

                                    TextureLevels data;
                                    if (typeOriginalDefName == apparelTypeOriginalDefName
                                        && comp.TryGetStoredTextureLevels(typeOriginalDefName, batch.textureLevelsName, out data)
                                        && (comp.cachedHideOrReplaceDict.NullOrEmpty()
                                            || !comp.cachedHideOrReplaceDict.ContainsKey(mlPrefixName + data.textureLevelsName)
                                            || !comp.cachedHideOrReplaceDict[mlPrefixName + data.textureLevelsName].hide)
                                        && data.CanRender(pawn, batch.keyName))
                                    {
                                        apparelColor = data.useStaticColor ? data.color : apparelColor;
                                        Color colorTwo = data.useStaticColor ? data.colorTwo : Color.white;

                                        Mesh mesh = null;
                                        if (data.meshSize != Vector2.zero)
                                        {
                                            mesh = NareisLib_MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing, data.flipped);
                                            if (data.meshType == "Body")
                                            {
                                                loc.x = vector.x;
                                                loc.z = vector.z;
                                            }
                                        }
                                        else
                                        {
                                            if (pawn.RaceProps.Humanlike)
                                            {
                                                switch (data.meshType)
                                                {
                                                    case "Body":
                                                        mesh = bodyMeshSet.MeshAt(facing, data.flipped);
                                                        loc.x = vector.x;
                                                        loc.z = vector.z;
                                                        break;
                                                    case "Head": mesh = headMeshSet.MeshAt(facing, data.flipped); break;
                                                    case "Hair": mesh = hairMeshSet.MeshAt(facing, data.flipped); break;
                                                }
                                            }
                                            else
                                            {
                                                mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(instance.graphics.nakedGraphic, facing) : bodyMesh;
                                                loc.x = vector.x;
                                                loc.z = vector.z;
                                            }
                                        }
                                        /*int pattern = 0;
                                        if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                                            pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];*/
                                        string headType = "";
                                        if (data.useHeadType)
                                            headType = pawn.story.headType.defName;
                                        Vector3 dataOffset = data.DrawOffsetForRot(facing);
                                        if (data.useStaticYOffset)
                                            loc.y = vector.y + dataOffset.y * 0.01f;
                                        if (data.usePublicYOffset)
                                            dataOffset.y *= 0.01f;
                                        else
                                            dataOffset.y *= 0.0001f;
                                        Vector3 pos = loc + dataOffset;
                                        Rot4 matFacing = data.switchEastWest && (facing == Rot4.East || facing == Rot4.West) ? new Rot4(4 - facing.AsInt) : facing;
                                        Material material = data.GetGraphic(batch.keyName, apparelColor, colorTwo, "", "", headType).MatAt(matFacing, null);
                                        Material mat = flags.FlagSet(PawnRenderFlags.Cache)
                                            ? material
                                            : OverrideMaterialIfNeeded(instance, material, pawn, flags.FlagSet(PawnRenderFlags.Portrait));
                                        GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                                        if (displayLevelInfo)
                                            Log.Warning(" " + data.originalDef + "------------" + data.textureLevelsName + ": " + pos.y.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool DrawHeadHairDisplaySwitchPatch(bool flag1)
        {
            return true;
        }


        //Overlay RenderPawnInternalPostfix
        static void RenderPawnInternalPostfix(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
        {
            MultiRenderComp comp = ___pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return;
            //if (!comp.PrefixResolved)
                //__instance.graphics.ResolveAllGraphics();
            Rot4 facing = bodyFacing;
            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(facing);
            if (curDirection.NullOrEmpty())
                return;
            
            Mesh bodyMesh = null;
            NareisLib_GraphicMeshSet bodyMeshSet = null;
            NareisLib_GraphicMeshSet hairMeshSet = null;
            NareisLib_GraphicMeshSet headMeshSet = null;
            if (___pawn.RaceProps.Humanlike)
            {
                bodyMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeBodySetForPawn(___pawn);
                headMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHeadSetForPawn(___pawn);
                hairMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHairSetForPawn(___pawn);
            }
            else
                bodyMesh = __instance.graphics.nakedGraphic.MeshAt(facing);

            bool displayLevelInfo = ModStaticMethod.ThisMod.apparelLevelsDisplayToggle;

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);

            if (curDirection.ContainsKey((int)TextureRenderLayer.Overlay))
            {
                Vector3 bodyLoc = rootLoc;
                bodyLoc.y += 0.037644785f;

                if (displayLevelInfo)
                {
                    Log.Warning(" ");
                    Log.Warning("从 " + bodyLoc.y.ToString() + " 开始渲染顶部图层");
                }

                foreach (MultiTexBatch batch in curDirection[(int)TextureRenderLayer.Overlay])
                {
                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                    //string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;
                    string mlPrefixName = batch.multiTexDefName + "_";

                    TextureLevels data;
                    if (!comp.TryGetStoredTextureLevels(typeOriginalDefName, batch.textureLevelsName, out data)
                        || (!comp.cachedHideOrReplaceDict.NullOrEmpty()
                            && comp.cachedHideOrReplaceDict.ContainsKey(mlPrefixName + data.textureLevelsName)
                            && comp.cachedHideOrReplaceDict[mlPrefixName + data.textureLevelsName].hide)
                        || !data.CanRender(___pawn, batch.keyName))
                        continue;

                    Mesh mesh = null;
                    Vector3 offset = Vector3.zero;
                    if (data.meshSize != Vector2.zero)
                    {
                        mesh = NareisLib_MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing, data.flipped);
                        switch (data.meshType)
                        {
                            case "Head": offset = quat * __instance.BaseHeadOffsetAt(facing); break;
                            case "Hair": offset = quat * __instance.BaseHeadOffsetAt(facing); break;
                        }
                    }
                    else
                    {
                        if (___pawn.RaceProps.Humanlike)
                        {
                            switch (data.meshType)
                            {
                                case "Body": 
                                    mesh = bodyMeshSet.MeshAt(facing, data.flipped); 
                                    break;
                                case "Head":
                                    mesh = headMeshSet.MeshAt(facing, data.flipped);
                                    offset = quat * __instance.BaseHeadOffsetAt(facing);
                                    break;
                                case "Hair":
                                    mesh = hairMeshSet.MeshAt(facing, data.flipped);
                                    offset = quat * __instance.BaseHeadOffsetAt(facing);
                                    break;
                            }
                        }
                        else
                            mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(__instance.graphics.nakedGraphic, facing) : bodyMesh;
                    }
                    /*int pattern = 0;
                    if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                        pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];*/
                    string condition = "";
                    if (data.hasRotting && bodyDrawType == RotDrawMode.Rotting)
                        condition = "Rotting";
                    if (data.hasDessicated && bodyDrawType == RotDrawMode.Dessicated)
                        condition = "Dessicated";
                    string bodyType = "";
                    string headType = "";
                    if (data.useBodyType)
                        bodyType = ___pawn.story.bodyType.defName;
                    else if (data.useHeadType)
                        headType = ___pawn.story.headType.defName;
                    Vector3 dataOffset = data.DrawOffsetForRot(facing);
                    if (data.useStaticYOffset)
                        bodyLoc.y = rootLoc.y + dataOffset.y * 0.01f;
                    if (data.usePublicYOffset)
                        dataOffset.y *= 0.01f;
                    else
                        dataOffset.y *= 0.0001f;
                    Vector3 pos = bodyLoc + offset + dataOffset;
                    Rot4 matFacing = data.switchEastWest && (facing == Rot4.East || facing == Rot4.West) ? new Rot4(4 - facing.AsInt) : facing;
                    Material mat = data.GetGraphic(batch.keyName, data.color, data.colorTwo, condition, bodyType, headType).MatAt(matFacing, null);
                    GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    if (displayLevelInfo)
                        Log.Warning(" " + data.originalDef + "------------" + data.textureLevelsName + ": " + pos.y.ToString());
                }
            }
        }



        //备用随机算法
        public static string GetRandom(System.Random rand, Dictionary<string, int> list)
        {
            int i = rand.Next(list.Values.Max());
            List<string> result = list.Keys.Where(x => list[x] >= i).ToList();
            return result.RandomElement();
        }



    }



    //初始化AB包加载（暂时停用）
    //[HarmonyPatch(typeof(Root))]
    //[HarmonyPatch("CheckGlobalInit")]
    /*public class DAL_GameObjectPrefabLoadPatch
    {
        static void Postfix()
        {
            if (DAL_DynamicGameObjectPrefabManager.Initialized)
            {
                return;
            }
            DAL_DynamicGameObjectPrefabManager.InitGameObjectToList();
        }
    }*/



    //DAL_WorldCurrent构建
    /*[HarmonyPatch(typeof(World))]
    [HarmonyPatch("ConstructComponents")]
    public class DAL_WorldCurrentConstructPatch
    {
        static void Postfix()
        {
            DAL_WorldCurrent.GOM = new DAL_DynamicGameObjectManager();
        }
    }*/

}
