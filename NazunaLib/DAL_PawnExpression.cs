using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace NareisLib
{
    public class DAL_PawnExpression : IExposable
    {
        public Pawn pawn;
        public DAL_PawnExpressionDataDef def;
        public DAL_PawnExpressionDriver lastexpressionDriverMade;
        public DAL_CompPawnExpression curComp;
        public List<DAL_GraphicInfo> curGraphicList = new List<DAL_GraphicInfo>();

        public bool enabled = false;

        private DAL_PawnExpressionFlags curExpressionType = DAL_PawnExpressionFlags.Nature;

        public DAL_PawnExpression()
        {
        }

        public DAL_PawnExpression(DAL_PawnExpressionDataDef dataDef, DAL_CompPawnExpression comp)
        {
            def = dataDef;
            curComp = comp;
        }

        public bool Blink
        {
            get
            {
                return lastexpressionDriverMade.blink;
            }
        }

        public bool TryMakeDriver(Pawn driverPawn, out DAL_PawnExpressionDriver driver)
        {
            if (curExpressionType == DAL_PawnExpressionFlags.Death)
            {
                Messages.Message("dead driver ok", MessageTypeDefOf.PositiveEvent);
            }
            if (driverPawn.GetComp<DAL_CompPawnExpression>() == null)
            {
                driver = null;
                return false;
            }
            driver = (DAL_PawnExpressionDriver)Activator.CreateInstance(def.driverClass);
            driver.pawn = driverPawn;
            driver.def = def;
            driver.expression = this;
            lastexpressionDriverMade = driver;
            curExpressionType = driver.def.expressionType;
            return true;
        }

        public bool CheckExpressionFlags(DAL_PawnExpressionFlags type)
        {
            if (type == curExpressionType)
            {
                return true;
            }
            return false;
        }

        public void Blinker(bool blink)
        {
            if (lastexpressionDriverMade.blink != blink)
            {
                lastexpressionDriverMade.blink = blink;
            }
        }

        public void Damager(bool damaged)
        {
            if (lastexpressionDriverMade.damaged != damaged)
            {
                lastexpressionDriverMade.damaged = damaged;
            }
        }

        public void Clear()
        {
            pawn = null;
            def = null;
            lastexpressionDriverMade = null;
            curComp = null;
            curGraphicList = new List<DAL_GraphicInfo>();
            curExpressionType = DAL_PawnExpressionFlags.Nature;
    }

        public void ExposeData()
        {
        }
    }
}
