using AlienRace.ExtendedGraphics;
using HarmonyLib;
using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class ActionManager
    {
        //使用的行为树，填入defName
        public ActionDef def = null;

        private string valuePath;
        private string fullPath;
        //string curPrefix = "";
        private JobDef curJob = null;
        private Behavior curBehavior = null;
        private Action behaviorAction = null;


        private int valueIndex = -1;
        private int tickDelay = 10;
        private Action tickAction = null;

        public string GetCurPath
        {
            get
            {
                return valuePath;
            }
        }

        public string GetFullPath
        {
            get
            {
                return fullPath;
            }
        }

        public ActionManager()
        {

        }

        /// <summary>
        /// 最终确认是否渲染
        /// </summary>
        /// <param name="pawn">包装后的Pawn信息获取器</param>
        /// <returns></returns>
        public bool IsApplicable(ExtendedGraphicsPawnWrapper pawn)
        {
            return curBehavior == null || ((curBehavior.rendMoving && pawn.Moving) || (curBehavior.rendUnMoving && !pawn.Moving));
        }


        /// <summary>
        /// 更新ActionManager的CurBehavior的状态
        /// </summary>
        /// <param name="pawn">包装后的Pawn信息获取器</param>
        /// <param name="obj">Pawn本身</param>
        /// <param name="job">Pawn当前的Job</param>
        /// <param name="key">TextureLevels此时为此Pawn随机到的贴图名称</param>
        public void StateUpdate(ExtendedGraphicsPawnWrapper pawn, Pawn obj, JobDef job, string key)
        {
            if (job == null || key == null || key == "")
                return;
            
            if (curJob == null || curJob.defName != job.defName || curBehavior == null)
            {
                curJob = job;
                valueIndex = -1;
                valuePath = "";
                fullPath = "";
                if (behaviorAction != null)
                {
                    UnregisterBehavior();
                    behaviorAction = null;
                }
                if (def.behaviors.TryGetValue(job, out curBehavior) && curBehavior.textures.Contains(key))
                {
                    //当使用链接至其他ActionManager时
                    if (curBehavior.linkedWithAction)
                    {
                        MultiRenderComp comp = obj.GetComp<MultiRenderComp>();
                        TextureLevels level;
                        if (comp != null && comp.TryGetStoredTextureLevels(curBehavior.type_originalDefName, curBehavior.textureLevelsName, out level))
                        {
                            if (valuePath != level.actionManager.GetCurPath && level.actionManager.GetCurPath != "")
                            {
                                fullPath = Path.Combine(curBehavior.exPath, valuePath = level.actionManager.GetCurPath);
                                GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(obj);
                            }
                            behaviorAction = () =>
                            {
                                if (valuePath != level.actionManager.GetCurPath && level.actionManager.GetCurPath != "")
                                {
                                    fullPath = Path.Combine(curBehavior.exPath, valuePath = level.actionManager.GetCurPath);
                                    GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(obj);
                                }
                            };
                            HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(behaviorAction, curBehavior.linkedActionSyncDelta.SecondsToTicks(), null, true);
                            return;
                        }
                    }

                    //在正常状态下的逻辑
                    List<string> list;
                    if (curBehavior.pathDict.TryGetValue(pawn.GetPosture(), out list) && !list.NullOrEmpty())
                        fullPath = Path.Combine(curBehavior.exPath, valuePath = curBehavior.randomFirst ? list.RandomElement() : list[0]);
                    //当启用了随机动作变换时
                    if (curBehavior.randomChange)
                    {
                        behaviorAction = () =>
                        {
                            valuePath = "";
                            fullPath = "";
                            List<string> paths;
                            if (curBehavior.pathDict.TryGetValue(pawn.GetPosture(), out paths) && !paths.NullOrEmpty())
                                fullPath = Path.Combine(curBehavior.exPath, valuePath = curBehavior.loopChange ? paths[GetNextIndex(paths)] : paths.RandomElement());
                            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(obj);
                        };
                        RegisterBehavior();
                    }
                    return;
                }
                else
                    fullPath = "";

                GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(obj);
            }
        }

        /// <summary>
        /// 获取轮转变换的下一个索引
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private int GetNextIndex(List<string> list)
        {
            if (valueIndex >= list.Count() - 1)
                return valueIndex = 0;
            else
                return valueIndex++;
        }

        /// <summary>
        /// 注册随机行为以及随机时间
        /// </summary>
        private void RegisterBehavior()
        {
            
            if (!curBehavior.useRandomTimeDelta)
            {
                //behaviorAction = () => curPrefix = curBehavior.prefixDict.RandomElement();
                HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(behaviorAction, curBehavior.actionStaticChangeSeconds.SecondsToTicks(), null, true);
            }  
            else
            {
                Action loopAction = behaviorAction;
                behaviorAction = () =>
                {
                    loopAction();
                    HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(behaviorAction, curBehavior.GetActionRandomChangeTick(), null, false);
                    //curPrefix = curBehavior.prefixDict.RandomElement();
                };
                HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(behaviorAction, curBehavior.GetActionRandomChangeTick(), null, false);
            }
        }

        /// <summary>
        /// 取消注册当前行为
        /// </summary>
        private void UnregisterBehavior()
        {
            HugsLibController.Instance.TickDelayScheduler.TryUnscheduleCallback(behaviorAction);
        }

        public ActionManager Clone()
        {
            ActionManager result = new ActionManager();
            result.def = def;
            result.fullPath = fullPath;
            //result.curPrefix = curPrefix;
            result.curJob = curJob;
            result.curBehavior = curBehavior;
            result.behaviorAction = behaviorAction;
            result.tickDelay = tickDelay;
            result.tickAction = tickAction;
            return result;
        }

        public void RegisterTickability()
        {
            tickAction = () =>
            {
                HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(() =>
                {
                    CustomTick();
                }, tickDelay, null, true);
            };
            tickAction();
        }

        public void UnregisterTickability()
        {
            HugsLibController.Instance.TickDelayScheduler.TryUnscheduleCallback(tickAction);
        }

        private void CustomTick()
        {
            // logic here
        }
    }
}
