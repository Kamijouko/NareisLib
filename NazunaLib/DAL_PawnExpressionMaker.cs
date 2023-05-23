using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NareisLib
{
    public static class DAL_PawnExpressionMaker
    {
        public static DAL_PawnExpression MakeExpression(DAL_PawnExpressionDataDef def, DAL_CompPawnExpression comp)
        {
            DAL_PawnExpression exp = SimplePool<DAL_PawnExpression>.Get();
            exp.def = def;
			exp.curComp = comp;
            return exp;
        }

		public static void ReturnToPool(ref DAL_PawnExpression exp)
		{
			if (exp == null)
			{
				return;
			}
			if (SimplePool<DAL_PawnExpression>.FreeItemsCount >= 1000)
			{
				return;
			}
			exp.Clear();
			SimplePool<DAL_PawnExpression>.Return(exp);
		}
	}
}
