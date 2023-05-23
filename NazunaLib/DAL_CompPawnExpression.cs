using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NareisLib
{
    public class DAL_CompPawnExpression : ThingComp
    {
        public Pawn pawn;

        public DAL_PawnUpper curUpper = null;
        public DAL_PawnExpression curExpression;
        public DAL_PawnExpressionDriver curDriver;
        public DAL_PawnExpressionDriver lastDriver;
        public DAL_ResolveTriggerDef curTriggerDef;
        public float curExpressionLenthSeconds = -1;

        private int curExpresseionLenthTick = -1;
        private int curBlinkTick = -1;

        /*private List<DAL_ResolveTrigger> triggerList = new List<DAL_ResolveTrigger>();
        public List<DAL_PawnExpressionDataDef> verbExList = new List<DAL_PawnExpressionDataDef>();
        public List<DAL_PawnExpressionDataDef> jobExList = new List<DAL_PawnExpressionDataDef>();
        public List<DAL_PawnExpressionDataDef> needExList = new List<DAL_PawnExpressionDataDef>();
        public List<DAL_PawnExpressionDataDef> damageExList = new List<DAL_PawnExpressionDataDef>();
        public List<DAL_PawnExpressionDataDef> thinkExList = new List<DAL_PawnExpressionDataDef>();
        public List<DAL_PawnExpressionDataDef> hediffExList = new List<DAL_PawnExpressionDataDef>();*/

        public DAL_PawnExpressionDataDef natureDef;
        public DAL_PawnExpressionDataDef deathDef;
        public DAL_PawnExpressionDataDef blinkDef;
        public DAL_PawnExpressionDataDef damageDef;
        public List<DAL_PawnExpressionData> faceBlinkLayerList = new List<DAL_PawnExpressionData>();
        public List<DAL_PawnExpressionData> eyesBlinkLayerList = new List<DAL_PawnExpressionData>();
        public List<DAL_PawnExpressionData> mouseBlinkLayerList = new List<DAL_PawnExpressionData>();
        public List<DAL_PawnExpressionData> faceDamageLayerList = new List<DAL_PawnExpressionData>();
        public List<DAL_PawnExpressionData> eyesDamageLayerList = new List<DAL_PawnExpressionData>();
        public List<DAL_PawnExpressionData> mouseDamageLayerList = new List<DAL_PawnExpressionData>();

        public bool initialization = false;

        public DAL_CompProperties_PawnExpression Props
        {
            get
            {
                return (DAL_CompProperties_PawnExpression)props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!initialization)
            {
                if (!Init())
                {
                    return;
                }
                initialization = true;
            }
            //Messages.Message("ticked", MessageTypeDefOf.PositiveEvent);
            if (curExpression != null)
            {
                if (curExpression.lastexpressionDriverMade == null)
                {
                    if (curDriver != null)
                    {
                        lastDriver = curDriver;
                        curDriver.Clear();
                    }
                    curExpression.TryMakeDriver(pawn, out curDriver);
                }
            }
            if (curDriver != null)
            {
                curDriver.DriverTick();
            }
            if (!curExpression.def.isSustainable)
            {
                PawnExpressionChangeManager();
            }
            else
            {
                if (curExpression.CheckExpressionFlags(DAL_PawnExpressionFlags.Nature))
                {

                }
                else if (curExpression.enabled)
                {
                    curExpression.enabled = curTriggerDef.CheckPawn(pawn);
                }
                else
                {
                    if (curExpression != null)
                    {
                        DAL_PawnExpressionMaker.ReturnToPool(ref curExpression);
                    }
                    curExpression = DAL_PawnExpressionMaker.MakeExpression(natureDef, this);
                    SetExpressionTime();
                }
            }
            if (!pawn.Dead && curExpression.CheckExpressionFlags(DAL_PawnExpressionFlags.Death))
            {
                DAL_PawnExpressionMaker.ReturnToPool(ref curExpression);
                curExpression = DAL_PawnExpressionMaker.MakeExpression(natureDef, this);
                SetExpressionTime();
            }
            /*if (pawn.Dead)
            {
                if (deathDef != null && !curExpression.CheckExpressionFlags(DAL_PawnExpressionFlags.Death))
                {
                    Messages.Message("checked", MessageTypeDefOf.PositiveEvent);
                    if (curExpression != null)
                    {
                        DAL_PawnExpression ex = curExpression;
                        DAL_PawnExpressionMaker.ReturnToPool(ex);
                    }
                    curExpression = DAL_PawnExpressionMaker.MakeExpression(deathDef, this);
                    SetExpressionTime();
                }
            }*/
            if (curExpression.def.canBlink && !pawn.Dead)
            {
                EyeBlinkManager();
            }
            SleepManager();
        }

        private bool Init()
        {
            DAL_PawnExpressionDataDef def = Props.expressionPlanDef.expressionDB.FirstOrDefault<DAL_PawnExpressionDataDef>(x => x.expressionType == DAL_PawnExpressionFlags.Nature);
            if (def == null)
            {
                Log.Error("Error while initial a PawnExpression Comp. " + parent + " has no Nature Expression.");
                return false;
            }
            pawn = parent as Pawn;
            curExpression = DAL_PawnExpressionMaker.MakeExpression(def, this);
            SetExpressionTime();
            curExpression.TryMakeDriver(pawn, out curDriver);
            natureDef = Props.expressionPlanDef.expressionDB.Find(x => x.expressionType == DAL_PawnExpressionFlags.Nature);
            deathDef = Props.expressionPlanDef.expressionDB.Find(x => x.expressionType == DAL_PawnExpressionFlags.Death);
            damageDef = Props.expressionPlanDef.expressionDB.FirstOrDefault(x => x.expressionType == DAL_PawnExpressionFlags.Damage && x.triggerDef.anyDamage);
            blinkDef = Props.expressionPlanDef.expressionDB.FirstOrDefault(x => x.expressionType == DAL_PawnExpressionFlags.Blink);
            if (damageDef?.renderFlag == DAL_PawnExpressionRenderFlags.All)
            {
                damageDef.TryGetData(DAL_PawnExpressionPartFlags.Facial, ref faceDamageLayerList);
            }
            else if (damageDef?.renderFlag == DAL_PawnExpressionRenderFlags.Both)
            {
                damageDef.TryGetData(DAL_PawnExpressionPartFlags.BothEye, ref eyesDamageLayerList);
                damageDef.TryGetData(DAL_PawnExpressionPartFlags.Mouse, ref mouseDamageLayerList);
            }
            else if (damageDef != null)
            {
                damageDef.TryGetData(DAL_PawnExpressionPartFlags.LeftEye, ref eyesDamageLayerList);
                damageDef.TryGetData(DAL_PawnExpressionPartFlags.RightEye, ref eyesDamageLayerList);
                damageDef.TryGetData(DAL_PawnExpressionPartFlags.Mouse, ref mouseDamageLayerList);
            }
            else
            {
                damageDef = null;
            }
            if (blinkDef?.renderFlag == DAL_PawnExpressionRenderFlags.All)
            {
                blinkDef.TryGetData(DAL_PawnExpressionPartFlags.Facial, ref faceBlinkLayerList);
            }
            else if (blinkDef?.renderFlag == DAL_PawnExpressionRenderFlags.Both)
            {
                blinkDef.TryGetData(DAL_PawnExpressionPartFlags.BothEye, ref eyesBlinkLayerList);
                blinkDef.TryGetData(DAL_PawnExpressionPartFlags.Mouse, ref mouseBlinkLayerList);
            }
            else if (blinkDef != null)
            {
                blinkDef.TryGetData(DAL_PawnExpressionPartFlags.LeftEye, ref eyesBlinkLayerList);
                blinkDef.TryGetData(DAL_PawnExpressionPartFlags.RightEye, ref eyesBlinkLayerList);
                blinkDef.TryGetData(DAL_PawnExpressionPartFlags.Mouse, ref mouseBlinkLayerList);
            }
            else
            {
                blinkDef = null;
            }
            Messages.Message("comp inited", MessageTypeDefOf.PositiveEvent);
            //triggerList.Clear();
            //Props.expressionPlanDef.GetTrigger(ref triggerList);
            return true;
        }

        public void SetExpressionTime()
        {
            if (curExpression.def.isSustainable)
            {
                curTriggerDef = curExpression.def.triggerDef;
                curExpression.enabled = true;
            }
            else
            {
                curExpressionLenthSeconds = Find.TickManager.TicksGame + curExpression.def.expressionLenthSeconds.Value.RandomInRange;
            }
        }

        private void PawnExpressionChangeManager()
        {
            if (curExpresseionLenthTick <= Find.TickManager.TicksGame)
            {
                DAL_PawnExpression expression = null;
                /*foreach (DAL_PawnExpressionDataDef ex in Props.expressionPlanDef.expressionDB)
                {
                    if (ex == curExpression.def)
                    {
                        continue;
                    }
                    DAL_ResolveTriggerDef def = ex.triggerDef;
                    if (def.IsNullDef())
                    {
                        continue;
                    }
                    if (def.verb != null && pawn.verbTracker.PrimaryVerb.GetType() != def.verb)
                    {
                        continue;
                    }
                    if (def.jobDef != null && def.jobDef != pawn.jobs.curJob.def)
                    {
                        continue;
                    }
                    if (def.needDef != null && pawn.needs.AllNeeds.FirstOrDefault(x => x.def == def.needDef) == null)
                    {
                        continue;
                    }
                    if (def.thoughtDef != null)
                    {
                        if (!def.thoughtDef.IsMemory)
                        {
                            continue;
                        }
                        if (pawn.needs.mood.thoughts.memories.Memories.FirstOrDefault(x => x.def == def.thoughtDef) == null)
                        {
                            continue;
                        }
                    }
                    if (def.hediffDef != null && pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def == def.hediffDef) == null)
                    {
                        continue;
                    }
                    expression = DAL_PawnExpressionMaker.MakeExpression(ex, this);
                }*/
                if (!curExpression.enabled && !curExpression.CheckExpressionFlags(DAL_PawnExpressionFlags.Nature))
                {
                    expression = DAL_PawnExpressionMaker.MakeExpression(natureDef, this);
                }
                if (expression != null && expression.def != curExpression.def)
                {
                    if (curExpression != null)
                    {
                        DAL_PawnExpressionMaker.ReturnToPool(ref curExpression);
                    }
                    curExpression = expression;
                    SetExpressionTime();
                    Messages.Message("expression backed", MessageTypeDefOf.PositiveEvent);
                }
                //curExpresseionLenthTick += curExpressionLenthSeconds.SecondsToTicks();
            }
        }

        private void PawnUpperManager()
        {

        }

        private void SleepManager()
        {
            if (pawn != null && curDriver != null)
            {
                if (pawn.jobs?.curDriver?.asleep == true)
                {
                    curDriver.layDownResting = true;
                }
                else
                {
                    curDriver.layDownResting = false;
                }
            }
        }

        public void DamageManager()
        {
            if (curExpression != null && curExpression.lastexpressionDriverMade != null)
            {
                curExpression.Damager(true);
            }
        }

        private void EyeBlinkManager()
        {
            if (curExpression != null && curExpression.lastexpressionDriverMade != null)
            {
                if (curBlinkTick <= Find.TickManager.TicksGame && !curExpression.Blink)
                {
                    curBlinkTick = Find.TickManager.TicksGame + Props.blinkIntervalSeconds.Value.RandomInRange.SecondsToTicks();
                    curExpression.Blinker(true);
                    Messages.Message("blinked", MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }


        
    }
}
