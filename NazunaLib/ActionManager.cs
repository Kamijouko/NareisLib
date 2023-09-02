using AlienRace.ExtendedGraphics;
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

        string valuePath;
        string fullPath;
        //string curPrefix = "";
        JobDef curJob = null;
        Behavior curBehavior = null;
        Action behaviorAction = null;

        

        int tickDelay = 10;
        Action tickAction = null;

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

        /*public string GetCurPrefix
        {
            get
            {
                return curPrefix;
            }
        }*/

        public ActionManager()
        {

        }

        public bool IsApplicable(ExtendedGraphicsPawnWrapper pawn)
        {
            return curBehavior == null || ((curBehavior.rendMoving && pawn.Moving) || (curBehavior.rendUnMoving && !pawn.Moving));
        }


        public void StateUpdate(ExtendedGraphicsPawnWrapper pawn, Pawn obj, JobDef job, string key)
        {
            if (job == null || key == null || key == "")
                return;

            if ((curJob != job || curBehavior == null) && def.behaviors.TryGetValue(job, out curBehavior) && curBehavior.textures.Contains(key))
            {
                if (curBehavior.linkedWithAction)
                {
                    MultiRenderComp comp = obj.GetComp<MultiRenderComp>();
                    if (comp != null 
                        && !comp.GetAllOriginalDefForGraphicDataDict.NullOrEmpty()
                        && comp.GetAllOriginalDefForGraphicDataDict.ContainsKey(curBehavior.type_originalDefName)
                        && comp.GetAllOriginalDefForGraphicDataDict[curBehavior.type_originalDefName].ContainsKey(curBehavior.textureLevelsName))
                    {
                        valuePath = "";
                        fullPath = Path.Combine(
                            curBehavior.exPath,
                            valuePath = comp.GetAllOriginalDefForGraphicDataDict[curBehavior.type_originalDefName][curBehavior.textureLevelsName].actionManager.GetCurPath
                            );
                        return;
                    }
                    
                }
                //exPath = curBehavior.exPath;
                //curPrefix = "";
                valuePath = "";
                fullPath = "";
                List<string> list = new List<string> { "" };
                if (!curBehavior.pathDict.TryGetValue(pawn.GetPosture(), out list) || !list.NullOrEmpty())
                    fullPath = Path.Combine(curBehavior.exPath, valuePath = list.RandomElement());

                if (curBehavior.randomChange)
                {
                    if (behaviorAction != null)
                        UnregisterBehavior();
                    behaviorAction = () =>
                    {
                        valuePath = "";
                        fullPath = "";
                        List<string> paths = new List<string> { "" }; ;
                        if (!curBehavior.pathDict.TryGetValue(pawn.GetPosture(), out paths) || !paths.NullOrEmpty())
                            fullPath = Path.Combine(curBehavior.exPath, valuePath = list.RandomElement());
                    };
                    RegisterBehavior();
                }
            }
            else 
                fullPath = "";
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
                behaviorAction = () =>
                {
                    HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(behaviorAction, curBehavior.GetActionRandomChangeTick(), null, false);
                    //curPrefix = curBehavior.prefixDict.RandomElement();
                };
                HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(behaviorAction, curBehavior.GetActionRandomChangeTick(), null, false);
            }
        }

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
