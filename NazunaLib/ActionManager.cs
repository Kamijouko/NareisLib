using AlienRace.ExtendedGraphics;
using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
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

        public string gizmoLabel = "";
        public string gizmoIconPath = "";

        string exPath = "";
        //string curPrefix = "";
        JobDef curJob = null;
        Behavior curBehavior = null;
        Action behaviorAction = null;

        

        int tickDelay = 10;
        Action tickAction = null;

        public Texture2D GetGizmoIcon
        {
            get
            {
                Texture2D result = TexCommand.GatherSpotActive;
                if (!gizmoIconPath.NullOrEmpty())
                {
                    result = ContentFinder<Texture2D>.Get(gizmoIconPath, true);
                }
                return result;
            }
        }

        public string GetExPath
        {
            get
            {
                return exPath;
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


        public void StateUpdate(ExtendedGraphicsPawnWrapper pawn, JobDef job, string key)
        {
            if (job == null || key == null || key == "")
                return;

            if ((curJob != job || curBehavior == null) && def.behaviors.TryGetValue(job, out curBehavior) && curBehavior.textures.Contains(key))
            {
                //exPath = curBehavior.exPath;
                //curPrefix = "";
                exPath = "";
                List<string> list;
                if (!curBehavior.pathDict.TryGetValue(pawn.GetPosture(), out list) || !list.NullOrEmpty())
                    exPath = list.First();

                if (curBehavior.randomChange)
                {
                    if (behaviorAction != null)
                        UnregisterBehavior();
                    behaviorAction = () =>
                    {
                        exPath = "";
                        List<string> paths;
                        if (!curBehavior.pathDict.TryGetValue(pawn.GetPosture(), out paths) || !paths.NullOrEmpty())
                            exPath = paths.RandomElement();
                    };
                    RegisterBehavior();
                }
            }
            else
                exPath = "";
        }

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
            result.exPath = exPath;
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
