using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NazunaLib
{
    public class DAL_PawnExpressionDataDef : Def
    {
        public Type driverClass;
        public int ticksBetweenFrame = 4;
        public int ticksBlink = 6;
        public int ticksDamage = 12;
        public FloatRange? expressionLenthSeconds = new FloatRange(1f, 3f);
        public DAL_PawnExpressionFlags expressionType = DAL_PawnExpressionFlags.Nature;
        public DAL_PawnExpressionRenderFlags renderFlag = DAL_PawnExpressionRenderFlags.All;
        public bool isSustainable = true;
        public bool canBlink = true;
        public bool useGendered = true;
        public bool hasSpecialBlinker = false;
        public bool hasSpecialDamager = false;

        public DAL_ResolveTriggerDef triggerDef;
        public float hediffSeverity = 0f;
        public int thoughtStageIndex = 0;

        public List<DAL_PawnExpressionData> dataList = new List<DAL_PawnExpressionData>();

        public bool TryGetData(DAL_PawnExpressionPartFlags flag, ref List<DAL_PawnExpressionData> result)
        {
            foreach(DAL_PawnExpressionData data in dataList)
            {
                if (data.partFlag == flag)
                {
                    result.Add(data);
                }
            }
            return true;
        }

    }
    
}
