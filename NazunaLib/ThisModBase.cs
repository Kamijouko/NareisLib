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
                if (__instance.workerClass != typeof(PawnRenderNodeWorker_TextureLevels))
                {
                    if (__instance.subworkerClasses.NullOrEmpty())
                        __instance.subworkerClasses = new List<Type>();
                    if (!__instance.subworkerClasses.Contains(typeof(DefaultNodeSubWorker)) && !__instance.subworkerClasses.Contains(typeof(TextureLevelsToNodeSubWorker)))
                        __instance.subworkerClasses.Add(typeof(DefaultNodeSubWorker));
                } 
            }
        }

        //给所有PawnRenderNode添加SubWorker
        /*[HarmonyPatch(typeof(PawnRenderTree))]
        [HarmonyPatch("AdjustParms")]
        public class AdjustParmsPatch
        {
            static void Postfix(PawnRenderTree __instance, ref PawnDrawParms parms)
            {
                ulong skipFlags = parms.skipFlags;
                TraverseTree(__instance.pawn, __instance.rootNode, delegate(PawnRenderNode node)
                {
                    TextureLevelsToNode tlNode = node as TextureLevelsToNode;
                    if (tlNode == null || tlNode.CurProps.textureLevels.renderSkipFlags == null)
                        return;
                    using (List<RenderSkipFlagDef>.Enumerator enumerator = tlNode.CurProps.textureLevels.renderSkipFlags.GetEnumerator())
                    {
                        while (enumerator.MoveNext()) 
                        {
                            RenderSkipFlagDef renderSkipFlagDef = enumerator.Current;
                            if (renderSkipFlagDef != RenderSkipFlagDefOf.None)
                                skipFlags |= renderSkipFlagDef;
                        }
                    }
                });
                parms.skipFlags = skipFlags;
            }

            private static void TraverseTree(Pawn pawn, PawnRenderNode rootNode, Action<PawnRenderNode> action)
            {
                Queue<PawnRenderNode> nodeQueue = new Queue<PawnRenderNode>();
                try
                {
                    nodeQueue.Enqueue(rootNode);
                    while (nodeQueue.Count > 0)
                    {
                        PawnRenderNode pawnRenderNode = nodeQueue.Dequeue();
                        if (pawnRenderNode == null)
                        {
                            Log.ErrorOnce(string.Format("Node is null - you must called EnsureGraphicsInitialized() on the drawn dynamic thing {0} before drawing it.", pawn), Gen.HashCombine<int>(1743846, pawn.GetHashCode()));
                            break;
                        }
                        action(pawnRenderNode);
                        if (pawnRenderNode.children != null)
                        {
                            foreach (PawnRenderNode item in pawnRenderNode.children)
                            {
                                nodeQueue.Enqueue(item);
                            }
                        }
                    }
                }
                catch (Exception arg)
                {
                    Log.Error(string.Format("Exception traversing pawn render node tree {0}: {1}", rootNode, arg));
                }
                finally
                {
                    nodeQueue.Clear();
                }
            }
        }*/


        //修补SetupDynamicNodes
        //这里将一次性处理不显示的Node及其子Node的迁移
        //此补丁函数的时机是在将新加的节点放进tmpChildTagNodes里，但还没有将这些节点通过AddChildren添加进指定父节点的时候
        [HarmonyPatch(typeof(PawnRenderTree))]
        [HarmonyPatch("SetupDynamicNodes")]
        public class SetupDynamicNodesPatch
        {
            static void Postfix(PawnRenderTree __instance, Dictionary<PawnRenderNodeTagDef, PawnRenderNode> ___nodesByTag, Dictionary<PawnRenderNodeTagDef, List<PawnRenderNode>> ___tmpChildTagNodes)
            {
                
                MultiRenderComp comp = __instance.pawn.GetComp<MultiRenderComp>();
                if (comp == null || (comp != null && comp.GetAllHideOriginalDefData.NullOrEmpty()))
                    return;

                //根据overrideClass提供的信息找到指定的Node，将其移除
                //并根据comp.GetAllHideOriginalDefData的键值对知道是否应该保留其Children里的子Node
                //如果保留，则将这些子Node加入移除对象的父Node的Children里
                foreach (OverrideClass over in comp.GetAllHideOriginalDefData.Keys)
                {
                    //如果overrideClass指定的nodeTagDef不为null，并且当前渲染树确实记录了该nodeTagDef所对应的PawnRenderNode
                    //如果over.parentRenderNodeTagDef为null而over.parentRenderDebugLabel不为""的话，则将其按照衣服(Apparel)对待，详见此if的else部分说明
                    if (over.parentRenderNodeTagDef != null && ___nodesByTag.ContainsKey(over.parentRenderNodeTagDef) && ___nodesByTag[over.parentRenderNodeTagDef] != null)
                    {
                        PawnRenderNode parentNode = null;
                        PawnRenderNode parentSubNode = null;
                        //首先从默认渲染树的根节点根据over里指定的tagDef进行遍历
                        TraverseTree(__instance.pawn, __instance.rootNode, delegate (PawnRenderNode node)
                        {
                            if (node.Props.tagDef == over.parentRenderNodeTagDef)
                            {
                                //如果over里指定了debugLabel的话则在找到了对应tagDef的节点后再继续对其子节点遍历
                                //如果没有指定的话直接将对应tagDef的节点赋值给parentNode并提前结束遍历
                                if (over.parentRenderDebugLabel != "")
                                {
                                    TraverseTree(__instance.pawn, node, delegate (PawnRenderNode subNode)
                                    {
                                        //只有找到debugLabel的节点时才会对parentNode与parentSubNode进行赋值
                                        //如果没有对应debugLabel的节点的话两个都不赋值
                                        if (subNode.Props.debugLabel == over.parentRenderDebugLabel)
                                        {
                                            parentNode = node;
                                            parentSubNode = subNode;
                                            return false;
                                        }
                                        return true;
                                    });
                                    if (parentSubNode != null)
                                        return false;
                                    else
                                        return true;
                                }
                                parentNode = node;
                                return false;
                            }
                            return true;
                        });
                        //如果在默认渲染树里没有找到tagDef对应的节点时，则对___tmpChildTagNodes整体进行遍历
                        if (parentNode == null)
                        {
                            foreach (KeyValuePair<PawnRenderNodeTagDef, List<PawnRenderNode>> pair in ___tmpChildTagNodes)
                            {
                                foreach (PawnRenderNode node in pair.Value)
                                {
                                    TraverseTree(__instance.pawn, node, delegate (PawnRenderNode child)
                                    {
                                        if (child.Props.tagDef == over.parentRenderNodeTagDef)
                                        {
                                            parentNode = node;
                                            if (over.parentRenderDebugLabel != "")
                                            {
                                                TraverseTree(__instance.pawn, node, delegate (PawnRenderNode subChild)
                                                {
                                                    if (subChild.Props.debugLabel == over.parentRenderDebugLabel)
                                                    {
                                                        parentSubNode = subChild;
                                                        return false;
                                                    }
                                                    return true;
                                                });
                                                if (parentSubNode != null)
                                                    return false;
                                            }
                                            return false;
                                        }
                                        return true;
                                    });
                                    if (parentNode != null)
                                        break;
                                }
                                if (parentNode != null)
                                    break;
                            }
                            //如果在___tmpChildTagNodes里找到了tagDef对应的节点，则判断此节点是在值的List里还是在某个children里
                            if (parentNode != null)
                            {
                                PawnRenderNode[] tmpChildren = null;
                                //如果over指定了debugLabel
                                if (over.parentRenderDebugLabel != "")
                                {
                                    //如果parentSubNode是位于在tmpChildTagNodes里找到的parentNode的children里的，则从它的parent进行移除
                                    if (parentSubNode != null)
                                    {
                                        if (!parentSubNode.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                        {
                                            foreach (PawnRenderNode child in parentSubNode.children)
                                                child.parent = parentSubNode.parent;
                                            tmpChildren = parentSubNode.children;
                                        }
                                        if (!tmpChildren.NullOrEmpty())
                                            parentSubNode.parent.AddChildren(tmpChildren);
                                        parentSubNode.parent.children.RemoveWhere(remove => remove == parentSubNode);
                                        continue;
                                    }
                                    //如果parentNode的子节点下并没有对应debugLabel的节点，则直接在tmpChildTagNodes根据parentNode的tagDef进行查询
                                    else if (___tmpChildTagNodes.ContainsKey(parentNode.Props.tagDef) && !___tmpChildTagNodes[parentNode.Props.tagDef].NullOrEmpty())
                                    {
                                        foreach(PawnRenderNode child in ___tmpChildTagNodes[parentNode.Props.tagDef])
                                        {
                                            TraverseTree(__instance.pawn, child, delegate (PawnRenderNode subChild)
                                            {
                                                if (subChild.Props.debugLabel == over.parentRenderDebugLabel)
                                                {
                                                    parentSubNode = subChild;
                                                    return false;
                                                }
                                                return true;
                                            });
                                            if (parentSubNode != null)
                                                break;
                                        }
                                        if (parentSubNode != null)
                                        {
                                            //在找到的情况下，如果其节点就是在___tmpChildTagNodes[parentNode.Props.tagDef]的列表里的话
                                            //则直接从___tmpChildTagNodes[parentNode.Props.tagDef]的列表中移除
                                            if (___tmpChildTagNodes[parentNode.Props.tagDef].Contains(parentSubNode))
                                            {
                                                if (!parentSubNode.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                                    tmpChildren = parentSubNode.children;
                                                if (!tmpChildren.NullOrEmpty())
                                                    ___tmpChildTagNodes[parentNode.Props.tagDef].Concat(tmpChildren);
                                                ___tmpChildTagNodes[parentNode.Props.tagDef].Remove(parentSubNode);
                                                continue;
                                            }
                                            //否则如果它其实是在___tmpChildTagNodes[parentNode.Props.tagDef]列表中节点的子节点中的话
                                            //则从其parent的children中移除
                                            else
                                            {
                                                if (!parentSubNode.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                                {
                                                    foreach (PawnRenderNode sub in parentSubNode.children)
                                                        sub.parent = parentSubNode.parent;
                                                    tmpChildren = parentSubNode.children;
                                                }
                                                if (!tmpChildren.NullOrEmpty())
                                                    parentSubNode.parent.AddChildren(tmpChildren);
                                                parentSubNode.parent.children.RemoveWhere(remove => remove == parentSubNode);
                                                continue;
                                            }
                                        }
                                    }
                                }
                                //如果over没有指定debugLabel
                                else
                                {
                                    //如果parentNode是在___tmpChildTagNodes值列表中的节点（而不是某个节点的children中的子节点）
                                    if (___tmpChildTagNodes.Values.Any(x=>x.Contains(parentNode)))
                                    {
                                        if (!parentNode.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                            tmpChildren = parentNode.children;
                                        if (!tmpChildren.NullOrEmpty())
                                            ___tmpChildTagNodes.Values.First(x=>x.Contains(parentNode)).Concat(tmpChildren);
                                        ___tmpChildTagNodes.Values.First(x => x.Contains(parentNode)).Remove(parentSubNode);
                                        continue;
                                    }
                                    //如果parentNode是在___tmpChildTagNodes值列表中某个节点的children中（或是多层children中）
                                    else
                                    {
                                        if (!parentNode.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                        {
                                            foreach (PawnRenderNode sub in parentNode.children)
                                                sub.parent = parentNode.parent;
                                            tmpChildren = parentNode.children;
                                        }
                                        if (!tmpChildren.NullOrEmpty())
                                            parentNode.parent.AddChildren(tmpChildren);
                                        parentNode.parent.children.RemoveWhere(remove => remove == parentNode);
                                        continue;
                                    }
                                }
                            }
                        }
                        //如果该节点确实是位于默认的渲染树中的话
                        else
                        {
                            PawnRenderNode[] tmpChildren = null;
                            if (over.parentRenderDebugLabel != "" && parentSubNode != null)
                            {
                                if (!parentSubNode.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                {
                                    foreach (PawnRenderNode sub in parentSubNode.children)
                                        sub.parent = parentSubNode.parent;
                                    tmpChildren = parentSubNode.children;
                                }
                                if (!tmpChildren.NullOrEmpty())
                                    parentSubNode.parent.AddChildren(tmpChildren);
                                parentSubNode.parent.children.RemoveWhere(remove => remove == parentSubNode);
                                continue;
                            }
                            else
                            {
                                if (!parentNode.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                    tmpChildren = parentNode.children;
                                if (!tmpChildren.NullOrEmpty())
                                    parentNode.parent.children.Concat(tmpChildren);
                                parentNode.parent.children.RemoveWhere(remove => remove == parentNode);
                                continue;
                            }
                        }
                        
                        /*
                        //如果overrideClass指定的debugLabel设置不为""，则开始遍历渲染树的两个部分
                        //否则将仅根据nodeTagDef在___nodesByTag里所对应的Node进行操作，详见此if的else部分说明
                        //第一个部分是遍历预先设定好了的包含一些ChildrenNode的PawnRenderNode
                        //第二个部分是遍历一些临时生成或者需要运行中加入指定父Node的PawnRenderNode(这些Node同样有可能包含预设定好的Children)
                        if (over.parentRenderDebugLabel != "")
                        {
                            //非常麻烦的遍历所有带有nodeTagDef的Node及其Children里的所有子Node
                            PawnRenderNode parent = null;
                            TraverseTree(__instance.pawn, ___nodesByTag[over.parentRenderNodeTagDef], delegate (PawnRenderNode node)
                            {
                                if (node.Props.debugLabel == over.parentRenderDebugLabel)
                                {
                                    parent = node.parent;
                                    return false;
                                }
                                return true;
                            });
                            if (parent != null 
                                && !parent.children.NullOrEmpty() 
                                && parent.children.FirstOrDefault(x=>x.Props.debugLabel == over.parentRenderDebugLabel) != null)
                            {
                                PawnRenderNode[] tmpChildren = null;
                                parent.children.RemoveWhere(delegate (PawnRenderNode node)
                                {
                                    if (node.Props.debugLabel == over.parentRenderDebugLabel)
                                    {
                                        if (!node.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                        {
                                            foreach (PawnRenderNode child in node.children)
                                                child.parent = parent;
                                            tmpChildren = node.children;
                                        }
                                        return true;
                                    }
                                    return false;
                                });
                                if (!tmpChildren.NullOrEmpty())
                                    parent.AddChildren(tmpChildren);
                                continue;
                            }
                            //非常麻烦的遍历即将加入渲染树的列表中tagDef所对应Node的Children里的所有Node及其所有子Node
                            if (parent == null 
                                && ___tmpChildTagNodes.ContainsKey(over.parentRenderNodeTagDef) 
                                && !___tmpChildTagNodes[over.parentRenderNodeTagDef].NullOrEmpty())
                            {
                                ___tmpChildTagNodes[over.parentRenderNodeTagDef].RemoveWhere(delegate (PawnRenderNode node)
                                {
                                    PawnRenderNode n = null;
                                    TraverseTree(__instance.pawn, node, delegate (PawnRenderNode child)
                                    {
                                        if (child.Props.debugLabel == over.parentRenderDebugLabel)
                                        {
                                            n = child;
                                            return false;
                                        }
                                        return true;
                                    });
                                    if (n != null)
                                    {
                                        PawnRenderNode[] tmpChildren = null;
                                        if (n == node)
                                        {
                                            if (!n.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                            {
                                                foreach (PawnRenderNode child in n.children)
                                                    child.parent = ___nodesByTag[over.parentRenderNodeTagDef];
                                                tmpChildren = n.children;
                                            }
                                            if (!tmpChildren.NullOrEmpty())
                                                ___nodesByTag[over.parentRenderNodeTagDef].AddChildren(tmpChildren);
                                            return true;
                                        }
                                        else
                                        {
                                            if (!n.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                            {
                                                foreach (PawnRenderNode child in n.children)
                                                    child.parent = n.parent;
                                                tmpChildren = n.children;
                                            }
                                            if (!tmpChildren.NullOrEmpty())
                                                n.parent.AddChildren(tmpChildren);
                                            n.parent.children.RemoveWhere(remove => remove == n);
                                        }
                                    }
                                    return false;
                                });
                            }
                        }
                        else
                        {
                            //检查当前渲染树中是否有具有符合over.parentRenderNodeTagDef但不是___nodesByTag[over.parentRenderNodeTagDef]对应的值的node
                            //满足则将非___nodesByTag[over.parentRenderNodeTagDef]的值的node移除
                            //并根据comp.GetAllHideOriginalDefData[over]的结果决定是否将移除节点的children引继给___nodesByTag[over.parentRenderNodeTagDef]的节点
                            PawnRenderNode parent = null;
                            TraverseTree(__instance.pawn, __instance.rootNode, delegate (PawnRenderNode node)
                            {
                                if (node.Props.tagDef == over.parentRenderNodeTagDef && ___nodesByTag[over.parentRenderNodeTagDef] != node)
                                {
                                    parent = node.parent;
                                    return false;
                                }
                                return true;
                            });
                            if (parent != null
                                && !parent.children.NullOrEmpty()
                                && parent.children.FirstOrDefault(x => x.Props.tagDef == over.parentRenderNodeTagDef && ___nodesByTag[over.parentRenderNodeTagDef] != x) != null)
                            {
                                PawnRenderNode[] tmpChildren = null;
                                parent.children.RemoveWhere(delegate (PawnRenderNode node)
                                {
                                    if (node.Props.tagDef == over.parentRenderNodeTagDef && ___nodesByTag[over.parentRenderNodeTagDef] != node)
                                    {
                                        if (!node.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                        {
                                            foreach (PawnRenderNode child in node.children)
                                                child.parent = parent;
                                            tmpChildren = node.children;
                                        }
                                        return true;
                                    }
                                    return false;
                                });
                                if (!tmpChildren.NullOrEmpty())
                                {
                                    ___nodesByTag[over.parentRenderNodeTagDef].AddChildren(tmpChildren);
                                }
                                continue;
                            }
                            //检查 ___tmpChildTagNodes的所有value中是否有符合over.parentRenderNodeTagDef但不是___nodesByTag[over.parentRenderNodeTagDef]对应的值的node

                            if (comp.GetAllHideOriginalDefData[over])
                            {

                            }
                        }
                        */
                    }
                    //如果over.parentRenderNodeTagDef为null的话则视为Apparel（服装/装备）
                    else if(over.parentRenderDebugLabel != "")
                    {
                        PawnRenderNode parentNode = null;
                        PawnRenderNode[] tmpChildren = null;
                        PawnRenderNodeTagDef tmpTagDef = null;
                        if (___tmpChildTagNodes.ContainsKey(PawnRenderNodeTagDefOf.ApparelBody))
                        {
                            parentNode = ___tmpChildTagNodes[PawnRenderNodeTagDefOf.ApparelBody].FirstOrDefault(x => x.Props.debugLabel == over.parentRenderDebugLabel);
                            tmpTagDef = PawnRenderNodeTagDefOf.ApparelBody;
                        }
                        if (parentNode == null && ___tmpChildTagNodes.ContainsKey(PawnRenderNodeTagDefOf.ApparelHead))
                        {
                            parentNode = ___tmpChildTagNodes[PawnRenderNodeTagDefOf.ApparelHead].FirstOrDefault(x => x.Props.debugLabel == over.parentRenderDebugLabel);
                            tmpTagDef = PawnRenderNodeTagDefOf.ApparelHead;
                        }        
                        if (parentNode != null)
                        {
                            if (!parentNode.children.NullOrEmpty() && comp.GetAllHideOriginalDefData[over])
                                tmpChildren = parentNode.children;
                            if (!tmpChildren.NullOrEmpty())
                                ___tmpChildTagNodes[tmpTagDef].Concat(tmpChildren);
                            ___tmpChildTagNodes[tmpTagDef].Remove(parentNode);
                            continue;
                        }
                    }
                }
            }

            //action返回false则提前结束此函数
            private static void TraverseTree(Pawn pawn, PawnRenderNode rootNode, Func<PawnRenderNode, bool> action)
            {
                Queue<PawnRenderNode> nodeQueue = new Queue<PawnRenderNode>();
                try
                {
                    nodeQueue.Enqueue(rootNode);
                    while (nodeQueue.Count > 0)
                    {
                        PawnRenderNode pawnRenderNode = nodeQueue.Dequeue();
                        if (pawnRenderNode == null)
                        {
                            Log.ErrorOnce(string.Format("Node is null - you must called EnsureGraphicsInitialized() on the drawn dynamic thing {0} before drawing it.", pawn), Gen.HashCombine<int>(1743846, pawn.GetHashCode()));
                            break;
                        }
                        if (!action(pawnRenderNode))
                            break;
                        if (pawnRenderNode.children != null)
                        {
                            foreach (PawnRenderNode item in pawnRenderNode.children)
                            {
                               nodeQueue.Enqueue(item);
                            }
                        }
                    }
                }
                catch (Exception arg)
                {
                    Log.Error(string.Format("Exception traversing pawn render node tree {0}: {1}", rootNode, arg));
                }
                finally
                {
                    nodeQueue.Clear();
                }
            }
        }

        //给所有PawnRenderNode修补AddChildren
        [HarmonyPatch(typeof(PawnRenderNode))]
        [HarmonyPatch("AddChildren")]
        public class AddChildrenPatch
        {
            static bool Prefix(PawnRenderNode __instance, PawnRenderNode[] newChildren)
            {
                foreach (PawnRenderNode node in newChildren)
                {
                    node.parent = __instance;
                }
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
                //Log.Message("ModLoaded11111111");
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
