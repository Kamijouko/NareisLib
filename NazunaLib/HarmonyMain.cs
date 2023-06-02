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
using System.Security.Cryptography;


namespace NareisLib
{
    [StaticConstructorOnStartup]
    public class HarmonyMain
    {
        static HarmonyMain()
        {
            var harmonyInstance = new Harmony("NareisLib.kamijouko.nazunarei");

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



        //给所有Pawn添加多层渲染Comp，CompTick有触发条件所以不存在性能问题
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
        public static MultiTexEpoch ResolveMultiTexDef(MultiTexDef def, out Dictionary<string, TextureLevels> data)
        {
            //Log.Warning(plan+","+defName);
            MultiTexEpoch epoch = def.cacheOfLevels;
            List<MultiTexBatch> batches = new List<MultiTexBatch>();
            data = new Dictionary<string, TextureLevels>();
            foreach (TextureLevels level in def.levels)
            {
                string pre = "";
                string keyName = "";
                if (level.prefix.NullOrEmpty() && level.texPath != null)
                {
                    keyName = TextureLevels.ResolveKeyName(Path.GetFileNameWithoutExtension(level.texPath));
                }    
                else
                {
                    pre = level.prefix.RandomElementByWeight(x => level.preFixWeights[x]);
                    keyName = level.preFixToTexName[pre].RandomElementByWeight(x => level.texWeights[x]);
                }

                if (!batches.Exists(x => x.textureLevelsName == level.textureLevelsName))
                    batches.Add(new MultiTexBatch(def.originalDefClass, def.originalDef, def.defName, keyName, level.textureLevelsName, level.renderLayer, level.renderSwitch));
                //epoch.batches.First(x => x.keyName == keyName).keyList.Add(keyName);

                TextureLevels textureLevels = ThisModData.TexLevelsDatabase[def.originalDefClass.ToStringSafe()+"_"+def.originalDef][level.textureLevelsName].Clone();
                textureLevels.keyName = keyName;
                if (textureLevels.patternSets != null)
                    textureLevels.patternSets.typeOriginalDefNameKeyName = textureLevels.originalDefClass.ToStringSafe() + "_" + textureLevels.originalDef + "_" + keyName;
                if (!data.ContainsKey(textureLevels.textureLevelsName))
                    data[textureLevels.textureLevelsName] = textureLevels;
            }
            epoch.batches = batches;
            return epoch;
        }


        //从comp的storedData里获取TextureLevels数据，用于处理读取存档时从已有的storedData字典中得到的epoch
        public static Dictionary<string, TextureLevels> GetLevelsDictFromEpoch(MultiTexEpoch epoch)
        {
            return epoch.batches.ToDictionary(k => k.textureLevelsName, v => ResolveKeyNameForLevel(ThisModData.TexLevelsDatabase[v.originalDefClass.ToStringSafe()+"_"+v.originalDefName][v.textureLevelsName].Clone(), v.keyName));
        }
        //上方法的子方法，为获取到的TextureLevels进行赋值操作
        public static TextureLevels ResolveKeyNameForLevel(TextureLevels level, string key)
        {
            level.keyName = key;
            if (level.patternSets != null)
                level.patternSets.typeOriginalDefNameKeyName = level.originalDefClass.ToStringSafe() + "_" + level.originalDef + "_" + key;
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
                AddComp(ref comp, ref pawn);
            //comp.cachedRenderPlanDefName = plan;

            List<string> cachedOverride = new List<string>();
            Dictionary<string, Dictionary<string, TextureLevels>> cachedGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();
            Dictionary<string, MultiTexEpoch> data = new Dictionary<string, MultiTexEpoch>();
            HeadTypeDef head = pawn.story.headType;
            string fullOriginalDefName = typeof(HeadTypeDef).ToStringSafe() + "_" + head.defName;
            if (head != null && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
            {
                string keyName = head.defName;
                MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                if (comp.storedDataBody.NullOrEmpty() || !comp.storedDataBody.ContainsKey(keyName))
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
            fullOriginalDefName = typeof(BodyTypeDef).ToStringSafe() + "_" + body.defName;
            if (body != null && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
            {
                string keyName = body.defName;
                MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                if (comp.storedDataBody.NullOrEmpty() || !comp.storedDataBody.ContainsKey(keyName))
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
                if (comp.storedDataBody.NullOrEmpty() || !comp.storedDataBody.ContainsKey(hand))
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
            if (!ThisModData.RacePlansDatabase.ContainsKey(race))
                return;
            RenderPlanDef def = ThisModData.RacePlansDatabase[race];
            string plan = def.defName;
            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                AddComp(ref comp, ref pawn);

            List<string> cachedOverride = new List<string>();
            Dictionary<string, Dictionary<string, TextureLevels>> cachedGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();
            Dictionary<string, MultiTexEpoch> data = new Dictionary<string, MultiTexEpoch>();
            HairDef hair = pawn.story.hairDef;
            string fullOriginalDefName = typeof(HairDef).ToStringSafe() + "_" + hair.defName;
            if (hair != null && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
            {
                string keyName = hair.defName;
                MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                if (comp.storedDataHair.NullOrEmpty() || !comp.storedDataHair.ContainsKey(fullOriginalDefName))
                {
                    Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                    data[fullOriginalDefName] = ResolveMultiTexDef(multidef, out cachedData);
                    cachedGraphicData[fullOriginalDefName] = cachedData;
                }
                else
                {
                    data[fullOriginalDefName] = comp.storedDataHair[fullOriginalDefName];
                    cachedGraphicData[fullOriginalDefName] = GetLevelsDictFromEpoch(data[fullOriginalDefName]);
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
                AddComp(ref comp, ref pawn);

            List<string> cachedOverride = new List<string>();
            Dictionary<string, Dictionary<string, TextureLevels>> cachedGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();
            Dictionary<string, MultiTexEpoch> data = new Dictionary<string, MultiTexEpoch>();
            using (List<Apparel>.Enumerator enumerator = __instance.pawn.apparel.WornApparel.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    string fullOriginalDefName = typeof(ThingDef).ToStringSafe() + "_" + enumerator.Current.def.defName;
                    if (ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
                    {
                        string keyName = enumerator.Current.def.defName;
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


        //BottomOverlay BottomHair BottomShell BodyPrefix
        static bool DrawPawnBodyPrefix(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, float angle, Rot4 facing, RotDrawMode bodyDrawType, PawnRenderFlags flags, ref bool __state)
        {
            //Log.Warning(___pawn.Name + " flags: DrawNow = " + flags.FlagSet(PawnRenderFlags.DrawNow).ToStringSafe());
            __state = false;
            MultiRenderComp comp = ___pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return __state = true;
            if (!comp.PrefixResolved)
                __instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(facing);
            if (curDirection.NullOrEmpty())
                return __state = true;

            //Log.Warning("curDirection curDirection curDirection : " + curDirection.Count);
            //Log.Warning("curDirection curDirection curDirection : " + curDirection.ContainsKey((int)TextureRenderLayer.BottomHair).ToStringSafe());
            //Log.Warning("curDirection curDirection curDirection : " + curDirection[(int)TextureRenderLayer.BottomHair].Count);
            //Log.Warning("curDirection curDirection curDirection : " + curDirection[(int)TextureRenderLayer.BottomHair].First().textureLevelsName);

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 vector = rootLoc;
            vector.y += 0.006687258f;/*原身体为0.008687258f，反映精度为0.0003f*/
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

            List<int> renderLayers = new List<int>() { (int)TextureRenderLayer.BottomOverlay, (int)TextureRenderLayer.BottomHair, (int)TextureRenderLayer.BottomShell}; 
            
            foreach (int level in renderLayers)
            {
                if (curDirection.ContainsKey(level))
                {
                    foreach (MultiTexBatch batch in curDirection[level])
                    {
                        string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                        string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;

                        //Log.Warning("MultiTexBatch MultiTexBatch MultiTexBatch : " + typeOriginalDefName);

                        //Log.Warning("GraphicData GraphicData GraphicData : " + comp.GetAllOriginalDefForGraphicDataDict.Count);
                        //Log.Warning("GraphicData GraphicData GraphicData : " + comp.GetAllOriginalDefForGraphicDataDict.First().Key);
                        //Log.Warning("GraphicData GraphicData GraphicData : " + comp.GetAllOriginalDefForGraphicDataDict.First().Value.First().Key);

                        if (comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty()
                            || !comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(typeOriginalDefName)
                            || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName].ContainsKey(batch.textureLevelsName)
                            || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName].CanRender(___pawn, batch.keyName))
                            continue;

                        //Log.Warning("TTTTTTTTTTTTTTTTTTTTTTTTTTT");

                        TextureLevels data = comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName];
                        Mesh mesh = null;
                        Vector3 offset = Vector3.zero;
                        if (data.meshSize != Vector2.zero)
                        {
                            mesh = MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing);
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
                                    case "Body": mesh = bodyMesh; break;
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
                        }
                        int pattern = 0;
                        if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                            pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];
                        string condition = "";
                        if (data.hasRotting && bodyDrawType == RotDrawMode.Rotting)
                            condition = "Rotting";
                        if (data.hasDessicated && bodyDrawType == RotDrawMode.Dessicated)
                            condition = "Dessicated";
                        Vector3 dataOffset = data.DrawOffsetForRot(facing);
                        dataOffset.y *= 0.0001f;
                        Vector3 pos = vector + offset + dataOffset;
                        Material mat = data.GetGraphic(batch.keyName, pattern, condition).MatAt(facing, null);
                        GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }
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


        //Body HandOne Hand HandTwo Apparel(除了shell层) DrawPawnBodyFinalizer
        static void DrawPawnBodyFinalizer(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, float angle, Rot4 facing, RotDrawMode bodyDrawType, PawnRenderFlags flags, bool __state)
        {
            MultiRenderComp comp = ___pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return;
            if (!comp.PrefixResolved)
                __instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(facing);
            if (curDirection.NullOrEmpty())
                return;

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 vector = rootLoc;
            vector.y += 0.008687258f;
            Vector3 loc = vector;
            loc.y += 0.0014478763f;
            Mesh bodyMesh = null;
            Mesh hairMesh = null;
            Mesh headMesh = null;
            if (___pawn.RaceProps.Humanlike)
            {
                bodyMesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(___pawn).MeshAt(facing);
                headMesh = HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(___pawn).MeshAt(facing);
                hairMesh = HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(___pawn).MeshAt(facing);
            }
            else
                bodyMesh = __instance.graphics.nakedGraphic.MeshAt(facing);


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

            List<int> renderLayers;
            if (__state)
                renderLayers = new List<int>() { (int)TextureRenderLayer.HandOne, (int)TextureRenderLayer.Hand, (int)TextureRenderLayer.HandTwo };
            else
                renderLayers = new List<int>() { (int)TextureRenderLayer.Body, (int)TextureRenderLayer.HandOne, (int)TextureRenderLayer.Hand, (int)TextureRenderLayer.HandTwo };
            foreach (int level in renderLayers)
            {
                if (curDirection.ContainsKey(level))
                {
                    foreach (MultiTexBatch batch in curDirection[level])
                    {
                        string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                        string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;

                        if (comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty() 
                            || !comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(typeOriginalDefName)
                            || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName].ContainsKey(batch.textureLevelsName)
                            || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName].CanRender(___pawn, batch.keyName))
                            continue;

                        TextureLevels data = comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName];
                        Mesh mesh = null;
                        Vector3 offset = Vector3.zero;
                        if (data.meshSize != Vector2.zero)
                        {
                            mesh = MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing);
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
                                    case "Body": mesh = bodyMesh; break;
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
                        }
                        int pattern = 0;
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
                            pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];
                        string condition = "";
                        if (data.hasRotting && bodyDrawType == RotDrawMode.Rotting)
                            condition = "Rotting";
                        if (data.hasDessicated && bodyDrawType == RotDrawMode.Dessicated)
                            condition = "Dessicated";
                        Vector3 handOffset = Vector3.zero;
                        if (data.handDrawBehindShell && !data.isSleeve)
                        {
                            if (level == (int)TextureRenderLayer.Hand || level == (int)TextureRenderLayer.HandTwo)
                                handOffset.y = (__instance.graphics.apparelGraphics.Count + 1) * 0.0028957527f;
                        }
                        else if (data.sleeveDrawBehindShell && data.isSleeve)
                        {
                            handOffset.y = (__instance.graphics.apparelGraphics.Count + 2) * 0.0028957527f;
                        }
                        else
                        {
                            if (facing != Rot4.North)
                                handOffset.y = 0.014478763f;
                            else
                                handOffset.y = 0.011583012f;
                        }
                        Vector3 dataOffset = data.DrawOffsetForRot(facing);
                        dataOffset.y *= 0.0001f;
                        Vector3 pos = vector + offset + dataOffset + handOffset;
                        Material material = data.GetGraphic(batch.keyName, pattern, condition).MatAt(facing, null);
                        Material mat = (___pawn.RaceProps.IsMechanoid 
                            && ___pawn.Faction != null 
                            && ___pawn.Faction != Faction.OfMechanoids) ? __instance.graphics.GetOverlayMat(material, ___pawn.Faction.MechColor) : material;
                        GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }
                }
            }
            vector.y += 0.0028957527f;

            if (!__state && flags.FlagSet(PawnRenderFlags.Clothes) && curDirection.ContainsKey((int)TextureRenderLayer.Apparel))
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

                        //如果是需要多层的服装的话
                        string apparelTypeOriginalDefName = apparel.sourceApparel.def.GetType().ToStringSafe() + "_" + apparel.sourceApparel.def.defName;
                        if (!comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty() && comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(apparelTypeOriginalDefName))
                        {
                            foreach (MultiTexBatch batch in curDirection[(int)TextureRenderLayer.Apparel])
                            {
                                string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                                string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;

                                if (typeOriginalDefName == apparelTypeOriginalDefName 
                                    && comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName].ContainsKey(batch.textureLevelsName))
                                {
                                    TextureLevels data = comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName];
                                    Mesh mesh = null;
                                    Vector3 offset = Vector3.zero;
                                    if (data.meshSize != Vector2.zero)
                                    {
                                        mesh = MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing);
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
                                                case "Body": mesh = bodyMesh; break;
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
                                    }
                                    int pattern = 0;
                                    if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                                        pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];
                                    string condition = "";
                                    if (data.hasRotting && bodyDrawType == RotDrawMode.Rotting)
                                        condition = "Rotting";
                                    if (data.hasDessicated && bodyDrawType == RotDrawMode.Dessicated)
                                        condition = "Dessicated";
                                    Vector3 dataOffset = data.DrawOffsetForRot(facing);
                                    dataOffset.y *= 0.0001f;
                                    Vector3 pos = vector + offset + dataOffset;
                                    Material material = data.GetGraphic(batch.keyName, pattern, condition).MatAt(facing, null);
                                    Material mat = (___pawn.RaceProps.IsMechanoid && ___pawn.Faction != null && ___pawn.Faction != Faction.OfMechanoids)
                                        ? __instance.graphics.GetOverlayMat(material, ___pawn.Faction.MechColor)
                                        : material;
                                    Material apparelMat = flags.FlagSet(PawnRenderFlags.Cache)
                                        ? mat
                                        : OverrideMaterialIfNeeded(__instance, mat, ___pawn, flags.FlagSet(PawnRenderFlags.Portrait));
                                    GenDraw.DrawMeshNowOrLater(mesh, pos, quat, apparelMat, flags.FlagSet(PawnRenderFlags.DrawNow));
                                }
                            }
                        }
                        vector.y += 0.0028957527f;
                    }
                }
            }
            if (ModsConfig.IdeologyActive && __instance.graphics.bodyTattooGraphic != null && bodyDrawType != RotDrawMode.Dessicated && (facing != Rot4.North || ___pawn.style.BodyTattoo.visibleNorth))
            {
                GenDraw.DrawMeshNowOrLater(__instance.GetBodyOverlayMeshSet().MeshAt(facing), loc, quat, __instance.graphics.bodyTattooGraphic.MatAt(facing, null), flags.FlagSet(PawnRenderFlags.DrawNow));
            }
        }


        //Apparel(Shell层) DrawBodyApparelPrefix 
        static bool DrawBodyApparelPrefix(PawnRenderer __instance, Pawn ___pawn, Vector3 shellLoc, Vector3 utilityLoc, Mesh bodyMesh, float angle, Rot4 bodyFacing, PawnRenderFlags flags)
        {
            bool patchResult = true;
            
            MultiRenderComp comp = ___pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return true;
            if (!comp.PrefixResolved)
                __instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(bodyFacing);
            if (curDirection.NullOrEmpty())
                return true;

            List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;

            bool hasMultiTexApparel = !comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty() 
                && apparelGraphics.Exists(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell 
                    && !x.sourceApparel.def.apparel.shellRenderedBehindHead 
                    && comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(x.sourceApparel.def.defName));

            if (curDirection.ContainsKey((int)TextureRenderLayer.Apparel) && hasMultiTexApparel)
            {
                patchResult = false;

                Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
                Mesh hairMesh = null;
                Mesh headMesh = null;
                if (___pawn.RaceProps.Humanlike)
                {
                    headMesh = HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(___pawn).MeshAt(bodyFacing);
                    hairMesh = HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(___pawn).MeshAt(bodyFacing);
                }

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
                                loc.y += 0.0028957527f;
                            GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quat, material, flags.FlagSet(PawnRenderFlags.DrawNow));
                        }

                        //如果是多层服装的话
                        string apparelTypeOriginalDefName = apparel.sourceApparel.def.GetType().ToStringSafe() + "_" + apparel.sourceApparel.def.defName;
                        if (!comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty() && comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(apparelTypeOriginalDefName))
                        {
                            List<int> renderLayers = new List<int>() { (int)TextureRenderLayer.Apparel, (int)TextureRenderLayer.FrontShell };
                            foreach (int layer in renderLayers)
                            {
                                Vector3 loc = shellLoc;
                                if (apparel.sourceApparel.def.apparel.shellCoversHead)
                                    loc.y += 0.0028957527f;
                                if (layer == (int)TextureRenderLayer.FrontShell)
                                    loc.y = 0.0304054035f;
                                foreach (MultiTexBatch batch in curDirection[layer])
                                {
                                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                                    string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;

                                    if (typeOriginalDefName == apparelTypeOriginalDefName 
                                        && comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName].ContainsKey(batch.textureLevelsName))
                                    {
                                        TextureLevels data = comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName];
                                        Mesh mesh = null;
                                        
                                        Vector3 offset = Vector3.zero;
                                        if (data.meshSize != Vector2.zero)
                                        {
                                            mesh = MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(bodyFacing);
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
                                                    case "Body": mesh = bodyMesh; break;
                                                    case "Head":
                                                        mesh = headMesh;
                                                        offset = quat * __instance.BaseHeadOffsetAt(bodyFacing);
                                                        break;
                                                    case "Hair":
                                                        mesh = hairMesh;
                                                        offset = quat * __instance.BaseHeadOffsetAt(bodyFacing);
                                                        break;
                                                }
                                            }
                                            else
                                                mesh = bodyMesh;
                                        }
                                        int pattern = 0;
                                        if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                                            pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];
                                        Vector3 dataOffset = data.DrawOffsetForRot(bodyFacing);
                                        dataOffset.y *= 0.0001f;
                                        Vector3 pos = loc + offset + dataOffset;
                                        Material material = data.GetGraphic(batch.keyName, pattern).MatAt(bodyFacing, null);
                                        Material mat = flags.FlagSet(PawnRenderFlags.Cache)
                                            ? material
                                            : OverrideMaterialIfNeeded(__instance, material, ___pawn, flags.FlagSet(PawnRenderFlags.Portrait));
                                        GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                                    }
                                }
                            }
                        }
                    }

                    //渲染背包/工具层
                    if (PawnRenderer.RenderAsPack(apparel.sourceApparel))
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
            if (comp == null)
                return;
            
            if (!comp.PrefixResolved)
                instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(facing);

            int layer = (int)TextureRenderLayer.Head;

            //是否绘制原head贴图
            if (curDirection.NullOrEmpty()
                || !curDirection.ContainsKey(layer) 
                || comp.GetAllHideOriginalDefData.NullOrEmpty() 
                || !comp.GetAllHideOriginalDefData.Contains("Head"))
            {
                GenDraw.DrawMeshNowOrLater(headMesh, loc, quat, headMat, drawNow);
            }

            //绘制多层贴图
            if (!curDirection.NullOrEmpty() && curDirection.ContainsKey(layer))
            {
                Mesh bodyMesh = null;
                Mesh hairMesh = null;
                if (pawn.RaceProps.Humanlike)
                {
                    bodyMesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn).MeshAt(facing);
                    hairMesh = HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(pawn).MeshAt(facing);
                }
                else
                    bodyMesh = instance.graphics.nakedGraphic.MeshAt(facing);

                foreach (MultiTexBatch batch in curDirection[layer])
                {
                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                    string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;

                    if (comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty()
                        || !comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(typeOriginalDefName)
                        || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName].ContainsKey(batch.textureLevelsName)
                        || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName].CanRender(pawn, batch.keyName))
                        continue;

                    TextureLevels data = comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName];
                    Mesh mesh = null;
                    Vector3 offset = Vector3.zero;
                    if (data.meshSize != Vector2.zero)
                    {
                        mesh = MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing);
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
                                case "Body": mesh = bodyMesh; break;
                                case "Head":
                                    mesh = headMesh;
                                    offset = quat * instance.BaseHeadOffsetAt(facing);
                                    break;
                                case "Hair":
                                    mesh = hairMesh;
                                    offset = quat * instance.BaseHeadOffsetAt(facing);
                                    break;
                            }
                        }
                        else
                            mesh = bodyMesh;
                    }
                    int pattern = 0;
                    if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                        pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];
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
                    Vector3 dataOffset = data.DrawOffsetForRot(facing);
                    dataOffset.y *= 0.0001f;
                    Vector3 pos = headYOffset + offset + dataOffset;
                    Material material = data.GetGraphic(batch.keyName, pattern, condition).MatAt(facing, null);
                    Material mat = GetHeadOverrideMat(material, instance, flags.FlagSet(PawnRenderFlags.Portrait), !flags.FlagSet(PawnRenderFlags.Cache));
                    GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, drawNow);
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



        //FaceMask Hair HeadMask Hat DrawHeadHairTranspiler
        public static IEnumerable<CodeInstruction> DrawHeadHairPatchTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo drawMeshNowOrLater = AccessTools.Method(typeof(GenDraw), "DrawMeshNowOrLater", new Type[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) }, null);
            FieldInfo pawn = AccessTools.Field(typeof(PawnRenderer), "pawn");
            MethodInfo drawHeadHairHeadTranPatch = AccessTools.Method(typeof(PawnRenderPatchs), "DrawHeadHairHairTranPatch", null, null);
            MethodInfo drawHeadHairFaceMaskTranPatch = AccessTools.Method(typeof(PawnRenderPatchs), "DrawHeadHairFaceMaskTranPatch", null, null);
            MethodInfo drawHeadHairHeadMaskTranPatch = AccessTools.Method(typeof(PawnRenderPatchs), "DrawHeadHairHeadMaskTranPatch", null, null);
            List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
            int num;
            for (int i = 0; i < instructionList.Count; i = num + 1)
            {
                CodeInstruction instruction = instructionList[i];
                if (i > 10 && instructionList[i - 2].opcode == OpCodes.Ldloc_S && instructionList[i - 2].OperandIs(6) && instructionList[i - 3].OperandIs(drawMeshNowOrLater))
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


                if (instruction.OperandIs(drawMeshNowOrLater) && instructionList[i - 37].opcode == OpCodes.Ldloc_2/* && instructionList[i + 4].OperandIs(6)*/)
                {
                    //Log.Warning("RunPatched");
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

                if (i > 10 && instructionList[i - 2].opcode == OpCodes.Ldloc_S && instructionList[i - 2].OperandIs(6) && instructionList[i - 6].OperandIs(drawMeshNowOrLater))
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
            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return;
            if (!comp.PrefixResolved)
                instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(facing);

            Mesh bodyMesh = null;
            Mesh hairMesh = null;
            Mesh headMesh = null;
            if (pawn.RaceProps.Humanlike)
            {
                bodyMesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn).MeshAt(facing);
                headMesh = HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn).MeshAt(facing);
                hairMesh = HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(pawn).MeshAt(facing);
            }
            else
                bodyMesh = instance.graphics.nakedGraphic.MeshAt(facing);

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 hairYOffset = vector + headOffset;
            hairYOffset.y += 0.028957527f;
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
                            loc.y += !(facing == Rot4.North) || apparel.sourceApparel.def.apparel.hatRenderedAboveBody ? 0.03185328f : 0.002895753f;
                    }
                    //是否绘制原装备的贴图
                    if (comp.GetAllHideOriginalDefData.NullOrEmpty() 
                        || !comp.GetAllHideOriginalDefData.Contains(apparel.sourceApparel.def.defName))
                    {
                        Material original = apparel.graphic.MatAt(facing, null);
                        Material mat = flags.FlagSet(PawnRenderFlags.Cache) ? original : OverrideMaterialIfNeeded(original, pawn, instance, flags.FlagSet(PawnRenderFlags.Portrait));
                        GenDraw.DrawMeshNowOrLater(hairMesh, loc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }

                    //如果是多层服装的话
                    string apparelTypeOriginalDefName = apparel.sourceApparel.def.GetType().ToStringSafe() + "_" + apparel.sourceApparel.def.defName;
                    if (!curDirection.NullOrEmpty() && curDirection.ContainsKey(layer) 
                        && !comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty() 
                        && comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(apparelTypeOriginalDefName))
                    {
                        foreach (MultiTexBatch batch in curDirection[layer])
                        {
                            string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                            string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;

                            if (typeOriginalDefName == apparelTypeOriginalDefName 
                                && comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName].ContainsKey(batch.textureLevelsName))
                            {
                                TextureLevels data = comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName];
                                Mesh mesh = null;
                                if (data.meshSize != Vector2.zero)
                                {
                                    mesh = MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing);
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
                                                mesh = bodyMesh;
                                                loc.x = vector.x;
                                                loc.z = vector.z;
                                                break;
                                            case "Head": mesh = headMesh; break;
                                            case "Hair": mesh = hairMesh; break;
                                        }
                                    }
                                    else
                                    {
                                        mesh = bodyMesh;
                                        loc.x = vector.x;
                                        loc.z = vector.z;
                                    }
                                }                                        
                                int pattern = 0;
                                if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                                    pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];
                                Vector3 dataOffset = data.DrawOffsetForRot(facing);
                                dataOffset.y *= 0.0001f;
                                Vector3 pos = loc + dataOffset;
                                Material material = data.GetGraphic(batch.keyName, pattern).MatAt(facing, null);
                                Material mat = flags.FlagSet(PawnRenderFlags.Cache)
                                    ? material
                                    : OverrideMaterialIfNeeded(instance, material, pawn, flags.FlagSet(PawnRenderFlags.Portrait));
                                GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                            }
                        }
                    }
                }
            }
        }

        public static void DrawHeadHairHairTranPatch(Mesh hairMesh, Vector3 loc, Quaternion quat, Material hairMat, bool drawNow, Vector3 vector, Vector3 headOffset, Rot4 facing, PawnRenderFlags flags, Pawn pawn, PawnRenderer instance)
        {
            //Log.Warning("run hair patch");
            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return;
            if (!comp.PrefixResolved)
                instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(facing);

            int layer = (int)TextureRenderLayer.Hair;
            //是否绘制原头发贴图
            if (curDirection.NullOrEmpty()
                || !curDirection.ContainsKey(layer) 
                || comp.GetAllHideOriginalDefData.NullOrEmpty() 
                || !comp.GetAllHideOriginalDefData.Contains("Hair"))
            {
                GenDraw.DrawMeshNowOrLater(hairMesh, loc, quat, hairMat, drawNow);
            }

            //绘制多层头发贴图
            if (!curDirection.NullOrEmpty() && curDirection.ContainsKey(layer))
            {
                Vector3 hairYOffset = vector + headOffset;
                hairYOffset.y += 0.028957527f;
                Mesh bodyMesh = null;
                Mesh headMesh = null;
                if (pawn.RaceProps.Humanlike)
                {
                    bodyMesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn).MeshAt(facing);
                    headMesh = HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn).MeshAt(facing);
                }
                else
                    bodyMesh = instance.graphics.nakedGraphic.MeshAt(facing);

                foreach (MultiTexBatch batch in curDirection[layer])
                {
                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                    string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;

                    if (comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty()
                        || !comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(typeOriginalDefName)
                        || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName].ContainsKey(batch.textureLevelsName)
                        || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName].CanRender(pawn, batch.keyName))
                        continue;

                    TextureLevels data = comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName];
                    Mesh mesh = null;
                    Vector3 hairPos = hairYOffset;
                    if (data.meshSize != Vector2.zero)
                    {
                        mesh = MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing);
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
                                    mesh = bodyMesh;
                                    hairPos.x = vector.x;
                                    hairPos.z = vector.z;
                                    break;
                                case "Head": mesh = headMesh; break;
                                case "Hair": mesh = hairMesh; break;
                            }
                        }
                        else
                        {
                            mesh = bodyMesh;
                            hairPos.x = vector.x;
                            hairPos.z = vector.z;
                        } 
                    }
                    int pattern = 0;
                    if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                        pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];
                    Vector3 dataOffset = data.DrawOffsetForRot(facing);
                    dataOffset.y *= 0.0001f;
                    Vector3 pos = hairPos + dataOffset;
                    Material material = data.GetGraphic(batch.keyName, pattern).MatAt(facing, null);
                    Material mat = GetHairOverrideMat(material, instance, flags.FlagSet(PawnRenderFlags.Portrait), !flags.FlagSet(PawnRenderFlags.Cache));
                    GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, drawNow);
                }
            }
        }

        public static void DrawHeadHairHeadMaskTranPatch(float angle, Vector3 vector, Vector3 headOffset, Rot4 facing, PawnRenderFlags flags, List<ApparelGraphicRecord> apparelGraphics, bool shouldDraw, Pawn pawn, PawnRenderer instance)
        {
            MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return;
            if (!comp.PrefixResolved)
                instance.graphics.ResolveAllGraphics();

            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(facing);

            Mesh bodyMesh = null;
            Mesh hairMesh = null;
            Mesh headMesh = null;
            if (pawn.RaceProps.Humanlike)
            {
                bodyMesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn).MeshAt(facing);
                headMesh = HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn).MeshAt(facing);
                hairMesh = HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(pawn).MeshAt(facing);
            }
            else
                bodyMesh = instance.graphics.nakedGraphic.MeshAt(facing);

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 hairYOffset = vector + headOffset;
            hairYOffset.y += 0.028957527f;

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
                            loc.y += 0.02216602f;
                        else
                            loc.y += !(facing == Rot4.North) || apparel.sourceApparel.def.apparel.hatRenderedAboveBody ? 0.03185328f : 0.002895753f;
                    }
                    //是否绘制原装备的贴图
                    if (comp.GetAllHideOriginalDefData.NullOrEmpty() || !comp.GetAllHideOriginalDefData.Contains(apparel.sourceApparel.def.defName))
                    {
                        Material original = apparel.graphic.MatAt(facing, null);
                        Material mat = flags.FlagSet(PawnRenderFlags.Cache) ? original : OverrideMaterialIfNeeded(original, pawn, instance, flags.FlagSet(PawnRenderFlags.Portrait));
                        GenDraw.DrawMeshNowOrLater(hairMesh, loc, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                    }

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
                                foreach (MultiTexBatch batch in curDirection[layer])
                                {
                                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                                    string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;

                                    if (typeOriginalDefName == apparelTypeOriginalDefName 
                                        && comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName].ContainsKey(batch.textureLevelsName))
                                    {
                                        TextureLevels data = comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName];
                                        Mesh mesh = null;
                                        if (data.meshSize != Vector2.zero)
                                        {
                                            mesh = MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing);
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
                                                        mesh = bodyMesh;
                                                        loc.x = vector.x;
                                                        loc.z = vector.z;
                                                        break;
                                                    case "Head": mesh = headMesh; break;
                                                    case "Hair": mesh = hairMesh; break;
                                                }
                                            }
                                            else
                                            {
                                                mesh = bodyMesh;
                                                loc.x = vector.x;
                                                loc.z = vector.z;
                                            }
                                        }
                                        int pattern = 0;
                                        if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                                            pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];
                                        Vector3 dataOffset = data.DrawOffsetForRot(facing);
                                        dataOffset.y *= 0.0001f;
                                        Vector3 pos = loc + dataOffset;
                                        Material material = data.GetGraphic(batch.keyName, pattern).MatAt(facing, null);
                                        Material mat = flags.FlagSet(PawnRenderFlags.Cache)
                                            ? material
                                            : OverrideMaterialIfNeeded(instance, material, pawn, flags.FlagSet(PawnRenderFlags.Portrait));
                                        GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        //Overlay RenderPawnInternalPostfix
        static void RenderPawnInternalPostfix(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
        {
            MultiRenderComp comp = ___pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return;
            if (!comp.PrefixResolved)
                __instance.graphics.ResolveAllGraphics();
            Rot4 facing = bodyFacing;
            Dictionary<int, List<MultiTexBatch>> curDirection = comp.GetDataOfDirection(facing);
            if (curDirection.NullOrEmpty())
                return;

            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 bodyLoc = rootLoc;
            bodyLoc.y += 0.037644785f;

            if (curDirection.ContainsKey((int)TextureRenderLayer.Overlay))
            {
                Mesh bodyMesh = null;
                Mesh hairMesh = null;
                Mesh headMesh = null;
                if (___pawn.RaceProps.Humanlike)
                {
                    bodyMesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(___pawn).MeshAt(facing);
                    headMesh = HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(___pawn).MeshAt(facing);
                    hairMesh = HumanlikeMeshPoolUtility.GetHumanlikeHairSetForPawn(___pawn).MeshAt(facing);
                }
                else
                    bodyMesh = __instance.graphics.nakedGraphic.MeshAt(facing);

                foreach (MultiTexBatch batch in curDirection[(int)TextureRenderLayer.Overlay])
                {
                    string typeOriginalDefName = batch.originalDefClass.ToStringSafe() + "_" + batch.originalDefName;
                    string typeOtiginalDefNameKeyName = typeOriginalDefName + "_" + batch.keyName;

                    if (comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty()
                        || !comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(typeOriginalDefName)
                        || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName].ContainsKey(batch.textureLevelsName)
                        || !comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName].CanRender(___pawn, batch.keyName))
                        continue;

                    TextureLevels data = comp.GetAllOriginalDefForGraphicDataDict[typeOriginalDefName][batch.textureLevelsName];
                    Mesh mesh = null;
                    Vector3 offset = Vector3.zero;
                    if (data.meshSize != Vector2.zero)
                    {
                        mesh = MeshPool.GetMeshSetForWidth(data.meshSize.x, data.meshSize.y).MeshAt(facing);
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
                                case "Body": mesh = bodyMesh; break;
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
                    }
                    int pattern = 0;
                    if (!comp.cachedRandomGraphicPattern.NullOrEmpty() && comp.cachedRandomGraphicPattern.ContainsKey(typeOtiginalDefNameKeyName))
                        pattern = comp.cachedRandomGraphicPattern[typeOtiginalDefNameKeyName];
                    string condition = "";
                    if (data.hasRotting && bodyDrawType == RotDrawMode.Rotting)
                        condition = "Rotting";
                    if (data.hasDessicated && bodyDrawType == RotDrawMode.Dessicated)
                        condition = "Dessicated";
                    Vector3 dataOffset = data.DrawOffsetForRot(facing);
                    dataOffset.y *= 0.0001f;
                    Vector3 pos = bodyLoc + offset + dataOffset;
                    Material mat = data.GetGraphic(batch.keyName, pattern, condition).MatAt(facing, null);
                    GenDraw.DrawMeshNowOrLater(mesh, pos, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
                }
            }
        }



        //备用随机算法
        public static string GetRandom(System.Random rand, Dictionary<string, int> list)
        {
            int i = rand.Next(list.Values.Max() + 1);
            List<string> result = list.Keys.Where(x => list[x] >= i).ToList();
            return result.RandomElement();
        }



    }



    //初始化AB包加载（暂时停用）
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

}
