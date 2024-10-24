using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;
using AlienRace.ExtendedGraphics;
using System.IO;

namespace NareisLib
{
    public class MultiRenderComp : ThingComp
    {
        //存储当前Pawn身体，发型，服装的层渲染数据，
        //key为pawn需要替换或者添加图像的部件的originalDefName，格式为Type_OriginalDefName，Type为这个Def的类型，
        //value为存储需要渲染的图像的名称列表以及在哪个层渲染的信息
        public Dictionary<string, MultiTexEpoch> storedDataBody, storedDataHair, storedDataApparel = new Dictionary<string, MultiTexEpoch>(); 

        //用于缓存每次处理完的所有层数据,方向为正面，侧面，背面，反侧面，
        //key为TextureRenderLayer转为整数值，
        //value为经y轴排序后的贴图名字列表
        public Dictionary<int, List<MultiTexBatch>> cachedDataSouth, cachedDataEast, cachedDataNorth, cachedDataWest = new Dictionary<int, List<MultiTexBatch>>();

        //用于缓存已经初始化了的身体，头发，服装的TextureLevels，
        //第一个key为Type_OriginalDefName，Type为这个Def的类型，
        //第二个key为从multiTexBatch读取的TextureLevelName，
        //value为其对应的TextureLevels
        public Dictionary<string, Dictionary<string, TextureLevels>> cachedBodyGraphicData, cachedHairGraphicData, cachedApparelGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();

        //用于缓存上面三个dict为一个
        public Dictionary<string, Dictionary<string, TextureLevels>> cachedAllOriginalDefForGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();

        //用于缓存设置了linkedWithAction后链接到同一个目标的所有图层
        public List<TextureLevels> cachedLinkedActionManagerList = new List<TextureLevels>();

        //用于缓存以上三个dict的总和，
        //key为从multiTexBatch读取的TextureLevelName，
        //value为其对应的TextureLevels
        //public Dictionary<string, TextureLevels> cachedAllGraphicData = new Dictionary<string, TextureLevels>();
        public List<TextureLevels> cachedAllOriginalDefForGraphicDataList = new List<TextureLevels>();

        //用于缓存具有随机状态的贴图的当前的pattern，
        //key为此贴图的Type_OriginalDefName_KeyName，
        //value为此贴图目前应使用的pattern序号
        //public Dictionary<string, int> cachedRandomGraphicPattern = new Dictionary<string, int>();

        //用于缓存是否需要覆盖原部位的层名称列表,其中身体使用Body，头部使用Head，头发使用Hair来表示，其余覆盖部位用其defName表示
        public Dictionary<OverrideClass, bool> cachedOverrideBody/*, cachedOverrideApparel, cachedOverrideHair*/ = new Dictionary<OverrideClass, bool>();

        //用于缓存当前帧需要隐藏或替换层的数据
        public Dictionary<string, TextureLevelHideOption> cachedHideOrReplaceDict = new Dictionary<string, TextureLevelHideOption>();

        //用于缓存当前已注册过的TextureLevels的ActionManager，每当有TextureLevels变动时都重新生成
        public List<ActionManager> cachedActionManagers = new List<ActionManager>();

        //用于缓存当前pawn的race关联的RenderPlanDef
        //public string cachedRenderPlanDefName = "";

        //用于存储当前pawn的手部DefName
        public string storedHandDefName = "";

        //用于缓存当前手对应的持有装备角度
        public float holdEquipmentAngle = 0f;

        ExtendedGraphicsPawnWrapper pawnWarpper = null;

        public string pawnName = "";

        //public int timeTickLineIndex = 0;
        //public TextureLevelRandomPatternSet[] patternLine = new TextureLevelRandomPatternSet[] { };

        public bool PrefixResolved = false;

        public MultiRenderCompProperties Props
        {
            get
            {
                return (MultiRenderCompProperties)props;
            }
        }
        public string GetCurHandDefName
        {
            get
            {
                if (storedHandDefName == "" && !Props.handDefNameAndWeights.NullOrEmpty())
                    storedHandDefName = Props.handDefNameAndWeights.Keys.RandomElementByWeight(x => Props.handDefNameAndWeights[x]);
                return storedHandDefName;
            }
            set
            {
                storedHandDefName = value;
            }
        }
        public Dictionary<string, Dictionary<string, TextureLevels>> GetAllOriginalDefForGraphicDataDict
        {
            get
            {
                return cachedAllOriginalDefForGraphicData;
            }
        }
        public Dictionary<OverrideClass, bool> GetAllHideOriginalDefData
        {
            get
            {
                if (cachedOverrideBody != null)
                {
                    return cachedOverrideBody;
                }
                return new Dictionary<OverrideClass, bool>();
            }
        }
        public List<MultiTexBatch> GetAllBatch
        {
            get
            {
                return storedDataBody.Values.Concat(storedDataApparel.Values).Concat(storedDataHair.Values).SelectMany(x => x.batches).ToList();
            }
        }
        protected Pawn PawnOwner
        {
            get
            {
                Pawn result;
                if ((result = (parent as Pawn)) != null)
                {
                    return result;
                }
                return null;
            }
        }
        public Texture2D GetGizmoIcon
        {
            get
            {
                Texture2D result = TexCommand.Install;
                if (!ThisModData.RacePlansDatabase[PawnOwner.def.defName].actionSettingGizmo_IconPath.NullOrEmpty())
                {
                    result = ContentFinder<Texture2D>.Get(ThisModData.RacePlansDatabase[PawnOwner.def.defName].actionSettingGizmo_IconPath, true);
                }
                return result;
            }
        }


        public MultiRenderComp()
        {
            //Log.Message("new Comp");
        }

        public bool TryGetStoredTextureLevels(string type_OriDef, string texLevelsName, out TextureLevels level)
        {
            if (!GetAllOriginalDefForGraphicDataDict.NullOrEmpty()
                && GetAllOriginalDefForGraphicDataDict.ContainsKey(type_OriDef)
                && GetAllOriginalDefForGraphicDataDict[type_OriDef].ContainsKey(texLevelsName))
            {
                level = GetAllOriginalDefForGraphicDataDict[type_OriDef][texLevelsName];
                return true;
            }
            level = null;
            return false;
        }


        public override void CompTick()
        {
            base.CompTick();

            /*if (parent as Pawn != null && (parent as Pawn).Faction != null && (parent as Pawn).Faction.IsPlayer)
                Log.Warning("手臂：" + GetCurHandDefName);*/
            if (!parent.Spawned)
                return;
            if (pawnWarpper == null)
            {
                if (parent as Pawn != null && (parent as Pawn).Faction != null && (parent as Pawn).Faction.IsPlayer)
                    pawnWarpper = new ExtendedGraphicsPawnWrapper((Pawn)parent);
            }
            else if (ModStaticMethod.ThisMod.pawnCurJobDisplayToggle)
            {
                if (pawnWarpper.CurJob != null)
                    Log.Warning(pawnName + "当前工作：" + pawnWarpper.CurJob.def.defName);
                else
                    Log.Warning(pawnName + "当前工作：Null");
            }
            if (!cachedActionManagers.NullOrEmpty())
            {
                foreach (ActionManager manager in cachedActionManagers)
                    manager.ActionOnTick();
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            foreach (TextureLevels t in cachedAllOriginalDefForGraphicDataList)
            {
                if (t.actionManager.def != null)
                    t.actionManager.Destory();
            }
        }


        //根据方向获取comp存储的keyName的Dict
        public Dictionary<int, List<MultiTexBatch>> GetDataOfDirection(Rot4 facing)
        {
            switch (facing.AsInt)
            {
                case 2: return cachedDataSouth;
                case 1: return cachedDataEast;
                case 3: return cachedDataWest;
                case 0: return cachedDataNorth;
                default: return null;
            }
        }


        //对MultiTexEpoch所有的MultiTexBatch的Layer，针对每个方向进行分类和排序，并记入缓存
        public void ResolveAllLayerBatch()
        {
            List<MultiTexBatch> list = GetAllBatch;
            if (ModStaticMethod.ThisMod.debugToggle)
                Log.Warning("batch:" + list.Count().ToString());
            

            cachedAllOriginalDefForGraphicData = cachedBodyGraphicData.Concat(cachedHairGraphicData).Concat(cachedApparelGraphicData).ToDictionary(k => k.Key, v => v.Value);
            cachedAllOriginalDefForGraphicDataList = cachedAllOriginalDefForGraphicData.Values.SelectMany(x => x.Values).ToList();
            //cachedAllGraphicData = cachedAllOriginalDefForGraphicData.SelectMany(x => x.Value).ToDictionary(k => k.Key, v => v.Value);

            //获取临时的所有应该隐藏某个图层的列表
            cachedHideOrReplaceDict = cachedAllOriginalDefForGraphicDataList.Where(x => !x.hideList.NullOrEmpty()).SelectMany(x => x.hideList).ToLookup(k => k.defLevelName).ToDictionary(g => g.Key, g => g.First());


            if (ModStaticMethod.ThisMod.debugToggle)
            {
                Log.Warning("south:" + cachedDataSouth.SelectMany(x => x.Value).Count().ToString());
                Log.Warning("east:" + cachedDataEast.SelectMany(x => x.Value).Count().ToString());
                Log.Warning("north:" + cachedDataNorth.SelectMany(x => x.Value).Count().ToString());
                Log.Warning("AllGraphicData:" + GetAllOriginalDefForGraphicDataDict.Values.SelectMany(x => x.Values).Count().ToString());
                Log.Warning("levels:" + ThisModData.TexLevelsDatabase.Values.SelectMany(x => x.Values).Count().ToString());
                Log.Warning("plans:" + ThisModData.DefAndKeyDatabase.Values.SelectMany(x => x.Values).Count().ToString());
            }
        }



        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (PawnOwner != null && PawnOwner.Faction == Faction.OfPlayer && Find.Selector.SingleSelectedThing == PawnOwner && ThisModData.RacePlansDatabase.ContainsKey(PawnOwner.def.defName))
            {
                List<TextureLevels> levels = cachedAllOriginalDefForGraphicDataList.Where(x => x.actionManager.def != null).ToList();

                foreach (TextureLevels level in levels)
                {
                    
                }
                yield return new Command_Action
                {
                    defaultLabel = ThisModData.RacePlansDatabase[PawnOwner.def.defName].actionSettingGizmo_Label,
                    defaultDesc = ThisModData.RacePlansDatabase[PawnOwner.def.defName].actionSettingGizmo_Desc,
                    icon = GetGizmoIcon,
                    action = () =>
                    {
                        Find.WindowStack.Add(new Page_Setting_CurBehavior(this));
                    }
                };
            }
            yield break;
        }


        //下方法的子方法，为获取到的TextureLevels进行赋值操作
        public static TextureLevels ResolveKeyNameForLevel(TextureLevels level, string key, MultiTexBatch batch, List<TextureLevels> actionList = null, List<ActionManager> actionManagerList = null, Apparel apparel = null)
        {
            level.keyName = key;
            level.cachedBatch = batch;
            level.cachedApparel = apparel;
            if (level.actionManager.def != null)
            {
                actionManagerList.Add(level.actionManager);
                if (level.actionManager.def.behaviors.Values.Any(x => x.linkedWithAction) && !actionList.Contains(level))
                    actionList.Add(level);
            }
            /*if (level.patternSets != null)
                level.patternSets.typeOriginalDefNameKeyName = level.originalDefClass.ToStringSafe() + "_" + level.originalDef + "_" + key;*/
            return level;
        }

        //从comp的storedData里获取TextureLevels数据，用于处理读取存档时从已有的storedData字典中得到的epoch
        public static Dictionary<string, TextureLevels> GetLevelsDictFromEpoch(MultiTexEpoch epoch, List<TextureLevels> actionList = null, List<ActionManager> actionManagerList = null, Apparel apparel = null)
        {
            return !epoch.batches.NullOrEmpty() 
                ? epoch.batches.ToDictionary(k => k.textureLevelsName, v => ResolveKeyNameForLevel(ThisModData.TexLevelsDatabase[$"{v.originalDefClass.ToStringSafe()}_{v.originalDefName}"][v.textureLevelsName].Clone(), v.keyName, v, actionList, actionManagerList, apparel)) 
                : new Dictionary<string, TextureLevels>();
        }

        //处理defName所指定的MultiTexDef，
        //对其属性levels里所存储的所有TextureLevels都根据指定的权重随机一个贴图的名称，
        //并将名称记录进一个从其属性cacheOfLevels得来的MultiTexEpoch中所对应渲染图层的MultiTexBatch的名称列表里，
        //最终返回这个MultiTexEpoch
        public static MultiTexEpoch ResolveMultiTexDef(MultiTexDef def, 
            out Dictionary<string, TextureLevels> data,
            List<TextureLevels> actionList = null, 
            List<ActionManager> actionManagerList = null,
            Apparel apparel = null)
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

                    MultiTexBatch batch = new MultiTexBatch(def.originalDefClass, def.originalDef, def.defName, keyName, level.textureLevelsName, level.renderLayer, level.renderSwitch, level.staticLayer, level.donotChangeLayer);
                    if (!batches.Exists(x => x.textureLevelsName == level.textureLevelsName))
                        batches.Add(batch);

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
                        textureLevels.cachedBatch = batch;
                        textureLevels.cachedApparel = apparel;
                        /*if (textureLevels.patternSets != null)
                            textureLevels.patternSets.typeOriginalDefNameKeyName = textureLevels.originalDefClass.ToStringSafe() + "_" + textureLevels.originalDef + "_" + keyName;*/
                        if (!data.ContainsKey(textureLevels.textureLevelsName))
                            data[textureLevels.textureLevelsName] = textureLevels;
                        if (textureLevels.actionManager.def != null)
                        {
                            actionManagerList.Add(textureLevels.actionManager);
                            if (textureLevels.actionManager.def.behaviors.Values.Any(x => x.linkedWithAction) && !actionList.Contains(textureLevels))
                                actionList.Add(textureLevels);
                        }
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
        

        public void PreResolveAllLayerBatch()
        {
            if (!ModStaticMethod.AllLevelsLoaded || ThisModData.DefAndKeyDatabase.NullOrEmpty())
                return;
            if (!(parent is Pawn))
                return;
            Pawn pawn = (Pawn)parent;
            string race = pawn.def.defName;
            if (!ThisModData.RacePlansDatabase.ContainsKey(race))
                return;
            RenderPlanDef def = ThisModData.RacePlansDatabase[race];
            string plan = def.defName;
            /*MultiRenderComp comp = pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return;//AddComp(ref comp, ref pawn);*/
            Dictionary<OverrideClass, bool> cachedOverride = new Dictionary<OverrideClass, bool>();
            Dictionary<string, Dictionary<string, TextureLevels>> cachedGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>();
            Dictionary<string, MultiTexEpoch> data = new Dictionary<string, MultiTexEpoch>();
            List<TextureLevels> cachedActionList = new List<TextureLevels>();
            List<ActionManager> cachedAction = new List<ActionManager>();
            if (ThisModData.DefAndKeyDatabase.ContainsKey(plan))
            {
                //头部
                HeadTypeDef head = pawn.story.headType;
                string headName = head != null ? head.defName : "";
                string fullOriginalDefName = typeof(HeadTypeDef).ToStringSafe() + "_" + headName;
                if (head != null && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
                {
                    MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                    if (storedDataBody.NullOrEmpty() || !storedDataBody.ContainsKey(fullOriginalDefName))
                    {
                        Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                        data[fullOriginalDefName] = ResolveMultiTexDef(multidef, out cachedData, cachedActionList, cachedAction);
                        cachedGraphicData[fullOriginalDefName] = cachedData;
                    }
                    else
                    {
                        data[fullOriginalDefName] = storedDataBody[fullOriginalDefName];
                        cachedGraphicData[fullOriginalDefName] = GetLevelsDictFromEpoch(data[fullOriginalDefName], cachedActionList, cachedAction);
                    }
                    if (!multidef.renderOriginTex)
                    {
                        if (!multidef.overrideOriginSet.IsNull)
                        {
                            if (!cachedOverride.ContainsKey(multidef.overrideOriginSet))
                                cachedOverride.Add(multidef.overrideOriginSet, multidef.keepSubNode);
                            else if (cachedOverride[multidef.overrideOriginSet] && !multidef.keepSubNode)
                                cachedOverride[multidef.overrideOriginSet] = multidef.keepSubNode;
                        }
                        else
                        {
                            OverrideClass over = new OverrideClass(null, multidef.originalDef);
                            if (!cachedOverride.ContainsKey(over))
                                cachedOverride.Add(over, multidef.keepSubNode);
                            else if (cachedOverride[over] && !multidef.keepSubNode)
                                cachedOverride[over] = multidef.keepSubNode;
                        }
                    }
                    /*if (!multidef.renderOriginTex && !cachedOverride.Contains(multidef.renderDebugLabel ?? "Head"))
                        cachedOverride.Add(multidef.renderDebugLabel ?? "Head");*/
                }
                //身体
                BodyTypeDef body = pawn.story.bodyType;
                string bodyName = body != null ? body.defName : "";
                fullOriginalDefName = typeof(BodyTypeDef).ToStringSafe() + "_" + bodyName;
                if (body != null && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
                {
                    MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                    if (storedDataBody.NullOrEmpty() || !storedDataBody.ContainsKey(fullOriginalDefName))
                    {
                        Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                        data[fullOriginalDefName] = ResolveMultiTexDef(multidef, out cachedData, cachedActionList, cachedAction);
                        cachedGraphicData[fullOriginalDefName] = cachedData;
                    }
                    else
                    {
                        data[fullOriginalDefName] = storedDataBody[fullOriginalDefName];
                        cachedGraphicData[fullOriginalDefName] = GetLevelsDictFromEpoch(data[fullOriginalDefName], cachedActionList, cachedAction);
                    }
                    if (!multidef.renderOriginTex)
                    {
                        if (!multidef.overrideOriginSet.IsNull)
                        {
                            if (!cachedOverride.ContainsKey(multidef.overrideOriginSet))
                                cachedOverride.Add(multidef.overrideOriginSet, multidef.keepSubNode);
                            else if (cachedOverride[multidef.overrideOriginSet] && !multidef.keepSubNode)
                                cachedOverride[multidef.overrideOriginSet] = multidef.keepSubNode;
                        }
                        else
                        {
                            OverrideClass over = new OverrideClass(null, multidef.originalDef);
                            if (!cachedOverride.ContainsKey(over))
                                cachedOverride.Add(over, multidef.keepSubNode);
                            else if (cachedOverride[over] && !multidef.keepSubNode)
                                cachedOverride[over] = multidef.keepSubNode;
                        }
                    }
                }
                //手部
                string hand = GetCurHandDefName;
                fullOriginalDefName = typeof(HandTypeDef).ToStringSafe() + "_" + hand;
                if (hand != "" && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
                {
                    MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                    if (storedDataBody.NullOrEmpty() || !storedDataBody.ContainsKey(fullOriginalDefName))
                    {
                        Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                        data[fullOriginalDefName] = ResolveMultiTexDef(multidef, out cachedData, cachedActionList, cachedAction);
                        cachedGraphicData[fullOriginalDefName] = cachedData;
                    }
                    else
                    {
                        data[fullOriginalDefName] = storedDataBody[fullOriginalDefName];
                        cachedGraphicData[fullOriginalDefName] = GetLevelsDictFromEpoch(data[fullOriginalDefName], cachedActionList, cachedAction);
                    }
                    if (!multidef.renderOriginTex)
                    {
                        if (!multidef.overrideOriginSet.IsNull)
                        {
                            if (!cachedOverride.ContainsKey(multidef.overrideOriginSet))
                                cachedOverride.Add(multidef.overrideOriginSet, multidef.keepSubNode);
                            else if (cachedOverride[multidef.overrideOriginSet] && !multidef.keepSubNode)
                                cachedOverride[multidef.overrideOriginSet] = multidef.keepSubNode;
                        }
                        else
                        {
                            OverrideClass over = new OverrideClass(null, multidef.originalDef);
                            if (!cachedOverride.ContainsKey(over))
                                cachedOverride.Add(over, multidef.keepSubNode);
                            else if (cachedOverride[over] && !multidef.keepSubNode)
                                cachedOverride[over] = multidef.keepSubNode;
                        }
                    }
                }
                //cachedOverrideBody = cachedOverride;
                cachedBodyGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>(cachedGraphicData);
                storedDataBody = new Dictionary<string, MultiTexEpoch>(data);
                cachedGraphicData.Clear();
                data.Clear();
                //cachedOverride.Clear();
                //头发
                HairDef hair = pawn.story.hairDef;
                string keyName = hair != null ? hair.defName : "";
                fullOriginalDefName = typeof(HairDef).ToStringSafe() + "_" + keyName;
                if (hair != null && ThisModData.DefAndKeyDatabase.ContainsKey(plan) && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
                {

                    MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][fullOriginalDefName];
                    if (storedDataHair.NullOrEmpty() || !storedDataHair.ContainsKey(fullOriginalDefName))
                    {
                        Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                        data[fullOriginalDefName] = ResolveMultiTexDef(multidef, out cachedData, cachedActionList, cachedAction);
                        cachedGraphicData[fullOriginalDefName] = cachedData;
                    }
                    else
                    {
                        data[fullOriginalDefName] = storedDataHair[fullOriginalDefName];

                        cachedGraphicData[fullOriginalDefName] = GetLevelsDictFromEpoch(storedDataHair[fullOriginalDefName], cachedActionList, cachedAction);
                    }
                    if (!multidef.renderOriginTex)
                    {
                        if (!multidef.overrideOriginSet.IsNull)
                        {
                            if (!cachedOverride.ContainsKey(multidef.overrideOriginSet))
                                cachedOverride.Add(multidef.overrideOriginSet, multidef.keepSubNode);
                            else if (cachedOverride[multidef.overrideOriginSet] && !multidef.keepSubNode)
                                cachedOverride[multidef.overrideOriginSet] = multidef.keepSubNode;
                        }
                        else
                        {
                            OverrideClass over = new OverrideClass(null, multidef.originalDef);
                            if (!cachedOverride.ContainsKey(over))
                                cachedOverride.Add(over, multidef.keepSubNode);
                            else if (cachedOverride[over] && !multidef.keepSubNode)
                                cachedOverride[over] = multidef.keepSubNode;
                        }
                    }
                }
                //cachedOverrideHair = cachedOverride;
                cachedHairGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>(cachedGraphicData);
                storedDataHair = new Dictionary<string, MultiTexEpoch>(data);
                cachedGraphicData.Clear();
                data.Clear();
                //cachedOverride.Clear();
                //衣服
                using (List<Apparel>.Enumerator enumerator = pawn.apparel.WornApparel.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        string appKeyName = enumerator.Current.def.defName;
                        string appFullOriginalDefName = typeof(ThingDef).ToStringSafe() + "_" + appKeyName;
                        if (ThisModData.DefAndKeyDatabase.ContainsKey(plan) && ThisModData.DefAndKeyDatabase[plan].ContainsKey(fullOriginalDefName))
                        {
                            string apparelDefName = enumerator.Current.def.defName;
                            MultiTexDef multidef = ThisModData.DefAndKeyDatabase[plan][appFullOriginalDefName];
                            if (storedDataApparel.NullOrEmpty() || !storedDataApparel.ContainsKey(appFullOriginalDefName))
                            {
                                Dictionary<string, TextureLevels> cachedData = new Dictionary<string, TextureLevels>();
                                data[appFullOriginalDefName] = ResolveMultiTexDef(multidef, out cachedData, cachedActionList, cachedAction, enumerator.Current);
                                cachedGraphicData[appFullOriginalDefName] = cachedData;
                            }
                            else
                            {
                                data[appFullOriginalDefName] = storedDataApparel[appFullOriginalDefName];
                                cachedGraphicData[appFullOriginalDefName] = GetLevelsDictFromEpoch(data[appFullOriginalDefName], cachedActionList, cachedAction, enumerator.Current);
                            }
                            if (!multidef.renderOriginTex)
                            {
                                if (!multidef.overrideOriginSet.IsNull)
                                {
                                    if (!cachedOverride.ContainsKey(multidef.overrideOriginSet))
                                        cachedOverride.Add(multidef.overrideOriginSet, multidef.keepSubNode);
                                    else if (cachedOverride[multidef.overrideOriginSet] && !multidef.keepSubNode)
                                        cachedOverride[multidef.overrideOriginSet] = multidef.keepSubNode;
                                }
                                else
                                {
                                    OverrideClass over = new OverrideClass(null, multidef.originalDef);
                                    if (!cachedOverride.ContainsKey(over))
                                        cachedOverride.Add(over, multidef.keepSubNode);
                                    else if (cachedOverride[over] && !multidef.keepSubNode)
                                        cachedOverride[over] = multidef.keepSubNode;
                                }
                            }
                        }
                    }

                }
                //cachedOverrideApparel = cachedOverride;
                cachedOverrideBody = new Dictionary<OverrideClass, bool>(cachedOverride);
                cachedApparelGraphicData = new Dictionary<string, Dictionary<string, TextureLevels>>(cachedGraphicData);
                cachedLinkedActionManagerList = new List<TextureLevels>(cachedActionList);
                cachedActionManagers = new List<ActionManager>(cachedAction);
                storedDataApparel = new Dictionary<string, MultiTexEpoch>(data);
                cachedOverride.Clear();
                cachedGraphicData.Clear();
                cachedActionList.Clear();
                cachedAction.Clear();
                data.Clear();
                ResolveAllLayerBatch();
                PrefixResolved = true;
                pawnName = pawn.Name.ToStringFull;
            }
        }


        /// <summary>
        /// 将身体每个部分的图层转换为原版的Node，1.5专用
        /// </summary>
        /// <returns></returns>
        public override List<PawnRenderNode> CompRenderNodes()
        {
            
            if (parent as Pawn == null)
                return base.CompRenderNodes();

            Pawn pawn = (Pawn)parent;
            PreResolveAllLayerBatch();
            List<PawnRenderNode> result = new List<PawnRenderNode>();
            if (!cachedAllOriginalDefForGraphicDataList.NullOrEmpty())
                result = cachedAllOriginalDefForGraphicDataList.Select(x => x.GetPawnRenderNode(this, pawn) as PawnRenderNode).ToList();
            //Log.Message($"Comp true:{cachedAllOriginalDefForGraphicDataList.Count}");

            return result;


           
        }

        





        public override void PostExposeData()
        {
            base.PostExposeData();
            //Log.Warning("Comp Loading");
            //GetAllBatch.Where(x => x.layer == TextureRenderLayer.Apparel).SelectMany(x => x.keyList).Distinct().ToList().Sort((x, y) => ThisModData.TexLevelsDatabase[x].DrawOffsetForRot(Rot4.South).y.CompareTo(ThisModData.TexLevelsDatabase[y].DrawOffsetForRot(Rot4.South).y));
            Scribe_Values.Look<string>(ref pawnName, "pawnName", null, false);
            Scribe_Values.Look<string>(ref storedHandDefName, "storedHandDefName", null, false);
            Scribe_Collections.Look<string, MultiTexEpoch>(ref storedDataBody, "storedDataBody", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look<string, MultiTexEpoch>(ref storedDataHair, "storedDataHair", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look<string, MultiTexEpoch>(ref storedDataApparel, "storedDataApparel", LookMode.Value, LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (storedDataBody == null)
                    storedDataBody = new Dictionary<string, MultiTexEpoch>();
                if (storedDataHair == null)
                    storedDataHair = new Dictionary<string, MultiTexEpoch>();
                if (storedDataApparel == null)
                    storedDataApparel = new Dictionary<string, MultiTexEpoch>();
                //Log.Warning("Comp Load data Body : " + storedDataBody.Count);
                //Log.Warning("Comp Load data Hair : " + storedDataHair.Count);
                //Log.Warning("Comp Load data Apparel : " + storedDataApparel.Count);
            }
        }
    }
}
