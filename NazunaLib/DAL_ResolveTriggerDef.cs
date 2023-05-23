using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace NareisLib
{
    public class DAL_ResolveTriggerDef : Def
    {
        public bool isDead = false;
        public bool anyDamage = false;
        public Type verb;
        public JobDef jobDef;
        public NeedDef needDef;
        public DamageDef damageDef;
        public ThoughtDef thoughtDef;
        public HediffDef hediffDef;
        public MentalStateDef mentalStateDef;

        public bool IsNullDef()
        {
            if (/*verb == null && */jobDef == null && needDef == null && thoughtDef == null && hediffDef == null && !anyDamage && damageDef == null && mentalStateDef == null)
            {
                return true;
            }
            return false;
        }

        public static async Task<bool> CheckTaskA(DAL_ResolveTriggerDef def, Pawn p)
        {
            return await Task.Run(() => def.CheckA(p));
        }
        public static async Task<bool> CheckTaskB(DAL_ResolveTriggerDef def, Pawn p)
        {
            return await Task.Run(() => def.CheckB(p));
        }

        public bool CheckPawn(Pawn p)
        {
            Task<bool> taskA = Task.Run(() => CheckTaskA(this, p));
            Task<bool> taskB = Task.Run(() => CheckTaskB(this, p));
            return taskA.Result && taskB.Result;
        }

        public bool CheckA(Pawn pawn)
        {
            if (IsNullDef())
            {
                return false;
            }
            if (anyDamage && damageDef != null)
            {
                return false;
            }
            /*if (verb != null && pawn.verbTracker.PrimaryVerb.GetType() != verb)
            {
                return false;
            }*/
            if (jobDef != null && jobDef != pawn.jobs.curJob.def)
            {
                return false;
            }
            if (mentalStateDef != null && pawn.MentalStateDef != mentalStateDef)
            {
                return false;
            }
            return true;
        }

        public bool CheckB(Pawn pawn)
        {
            if (needDef != null && pawn.needs.AllNeeds.FirstOrDefault(x => x.def == needDef) == null)
            {
                return false;
            }
            if (thoughtDef != null)
            {
                if (!thoughtDef.IsMemory)
                {
                    return false;
                }
                if (pawn.needs.mood.thoughts.memories.Memories.FirstOrDefault(x => x.def == thoughtDef) == null)
                {
                    return false;
                }
            }
            if (hediffDef != null && pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def == hediffDef) == null)
            {
                return false;
            }
            return true;
        }
    }
}
