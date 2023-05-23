using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NareisLib
{
    public class DAL_RaceExpressionPlanDef : Def
    {
        public List<DAL_PawnExpressionDataDef> expressionDB = new List<DAL_PawnExpressionDataDef>();

        /*public void GetTrigger(ref List<DAL_ResolveTrigger> list)
        {
            foreach (DAL_PawnExpressionDataDef def in expressionDB)
            {
                DAL_ResolveTrigger trigegr = new DAL_ResolveTrigger(def.triggerDef);
                list.Add(trigegr);
            }
        }*/
    }
}
