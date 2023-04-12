using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace NazunaLib
{
    public class Pawn_AniStatic
    {
        public static Dictionary<string, Pawn_AniQueue> QueueDatabase = new Dictionary<string, Pawn_AniQueue>();

        public static void InitAniQueuePack()
        {
            List<DAL_RaceExpressionPlanDef> list = DefDatabase<DAL_RaceExpressionPlanDef>.AllDefsListForReading;

            List<DAL_PawnExpressionData> dataList = new List<DAL_PawnExpressionData>();
            foreach (DAL_RaceExpressionPlanDef plan in list)
            {
                foreach (DAL_PawnExpressionDataDef def in plan.expressionDB)
                {
                    foreach (DAL_PawnExpressionData data in def.dataList)
                    {
                        dataList.Add(data);
                    }
                }
            }
            foreach (DAL_PawnExpressionData def in dataList)
            {
                List<DAL_GraphicInfo> graphList = new List<DAL_GraphicInfo>();

            }
        }
    }
}
