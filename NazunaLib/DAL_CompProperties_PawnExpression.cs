using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NazunaLib
{
    public class DAL_CompProperties_PawnExpression : CompProperties
    {
        public DAL_CompProperties_PawnExpression()
        {
            this.compClass = typeof(DAL_CompPawnExpression);
        }

        public FloatRange? blinkIntervalSeconds = new FloatRange(6f, 15f);

        public DAL_RaceExpressionPlanDef expressionPlanDef;

        public DAL_RaceUpperPlanDef upperPlanDef;
    }
}
