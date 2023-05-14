using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using System.IO;
using AlienRace;
using System.Collections;

namespace NazunaLib
{
    public abstract class DAL_PawnExpressionDriver : IExposable
    {
        public Pawn pawn;
        public DAL_PawnExpressionDataDef def;
        public DAL_PawnExpression expression;

        private bool initialization = false;
        private int curTick = -1;
        public bool blink = false;
        public bool layDownResting = false;
        public bool damaged = false;

        public DAL_PawnExpressionCrossRenderFlags curCrossFlags = DAL_PawnExpressionCrossRenderFlags.None;

        public Dictionary<string, int> curIndexArray = new Dictionary<string, int>();

        private AlienPartGenerator.AlienComp alienComp;

        private List<DAL_PawnExpressionData> faceLayerList = new List<DAL_PawnExpressionData>();
        private List<DAL_PawnExpressionData> eyesLayerList = new List<DAL_PawnExpressionData>();
        private List<DAL_PawnExpressionData> mouseLayerList = new List<DAL_PawnExpressionData>();


        public virtual bool Init(DAL_PawnExpressionRenderFlags renderFlags)
        {
            bool result = false;
            if (renderFlags == DAL_PawnExpressionRenderFlags.All)
            {
                result = def.TryGetData(DAL_PawnExpressionPartFlags.Facial, ref faceLayerList);
            }
            if (renderFlags == DAL_PawnExpressionRenderFlags.Both)
            {
                result = def.TryGetData(DAL_PawnExpressionPartFlags.BothEye, ref eyesLayerList) && def.TryGetData(DAL_PawnExpressionPartFlags.Mouse, ref mouseLayerList);
            }
            if (renderFlags == DAL_PawnExpressionRenderFlags.Split)
            {
                result = def.TryGetData(DAL_PawnExpressionPartFlags.LeftEye, ref eyesLayerList) && def.TryGetData(DAL_PawnExpressionPartFlags.RightEye, ref eyesLayerList) && def.TryGetData(DAL_PawnExpressionPartFlags.Mouse, ref mouseLayerList);
            }
            alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
            Messages.Message("driver inited", MessageTypeDefOf.PositiveEvent);
            return result;
        }

        public virtual void DriverTick()
        {
            if (!initialization)
            {
                initialization = Init(def.renderFlag);
            }
            if (ShouldResolveExpression())
            {
                ResolveExpression(def.expressionType, def.renderFlag, curCrossFlags);
            }
        }

        public virtual bool ShouldResolveExpression()
        {
            if (expression.CheckExpressionFlags(def.expressionType))
            {
                return true;
            }
            return false;
        }

        public virtual void ResolveExpression(DAL_PawnExpressionFlags flags = DAL_PawnExpressionFlags.Nature, DAL_PawnExpressionRenderFlags renderFlags = DAL_PawnExpressionRenderFlags.All, DAL_PawnExpressionCrossRenderFlags crossFlags = DAL_PawnExpressionCrossRenderFlags.None)
        {
            if (crossFlags != DAL_PawnExpressionCrossRenderFlags.None)
            {
                ResolveCrossExpression();
                return;
            }

            ResolveDataList(renderFlags);
        }

        public virtual void ResolveDataList(DAL_PawnExpressionRenderFlags renderFlags)
        {
            if (def.ticksBetweenFrame > 0 && curTick <= Find.TickManager.TicksGame)
            {
                if (damaged)
                {
                    if (def.hasSpecialDamager)
                    {
                        if (def.canBlink && (blink || layDownResting))
                        {
                            if (def.hasSpecialBlinker)
                            {
                                curTick = Find.TickManager.TicksGame + def.ticksBlink;
                            }
                            else
                            {
                                curTick = Find.TickManager.TicksGame + expression.curComp.blinkDef.expressionLenthSeconds.Value.RandomInRange.SecondsToTicks();
                            }
                        }
                        else
                        {
                            curTick = Find.TickManager.TicksGame + def.ticksDamage;
                        }
                    }
                    else
                    {
                        if (expression.curComp.damageDef.canBlink && (blink || layDownResting))
                        {
                            if (def.hasSpecialBlinker)
                            {
                                curTick = Find.TickManager.TicksGame + def.ticksBlink;
                            }
                            else
                            {
                                curTick = Find.TickManager.TicksGame + expression.curComp.blinkDef.expressionLenthSeconds.Value.RandomInRange.SecondsToTicks();
                            }
                        }
                        else
                        {
                            curTick = Find.TickManager.TicksGame + expression.curComp.damageDef.expressionLenthSeconds.Value.RandomInRange.SecondsToTicks();
                        }
                    }
                }
                else
                {
                    if (def.canBlink && (blink || layDownResting))
                    {
                        if (def.hasSpecialBlinker)
                        {
                            curTick = Find.TickManager.TicksGame + def.ticksBlink;
                        }
                        else
                        {
                            curTick = Find.TickManager.TicksGame + expression.curComp.blinkDef.expressionLenthSeconds.Value.RandomInRange.SecondsToTicks();
                        }
                    }
                    else
                    {
                        curTick = Find.TickManager.TicksGame + def.ticksBetweenFrame;
                    }
                }
                if (renderFlags == DAL_PawnExpressionRenderFlags.All && !faceLayerList.NullOrEmpty())
                {
                    ResolveFacial();
                }
                if (renderFlags == DAL_PawnExpressionRenderFlags.Both && !eyesLayerList.NullOrEmpty() && !mouseLayerList.NullOrEmpty())
                {
                    ResolveBoth();
                }
                if (renderFlags == DAL_PawnExpressionRenderFlags.Split && !eyesLayerList.NullOrEmpty() && !mouseLayerList.NullOrEmpty())
                {
                    ResolveSplit();
                }
            }
        }

        public virtual void ResolveFacial()
        {
            List<DAL_PawnExpressionData> list = new List<DAL_PawnExpressionData>();
            /*if (((!blink && !layDownResting) || def.hasSpecialBlinker) || (!damaged || def.hasSpecialDamager))*/
            if (((!blink && !layDownResting) || def.hasSpecialBlinker) && (!damaged || def.hasSpecialDamager))
            {
                foreach (DAL_PawnExpressionData data in faceLayerList)
                {
                    if ((blink || layDownResting) && !data.isBlink)
                    {
                        continue;
                    }
                    if (!(blink || layDownResting) && data.isBlink)
                    {
                        continue;
                    }
                    if (damaged && !data.isDamage)
                    {
                        continue;
                    }
                    if (!damaged && data.isDamage)
                    {
                        continue;
                    }
                    if (data.corwnType != null && (alienComp == null/* || alienComp.crownType != data.corwnType*/))
                    {
                        continue;
                    }
                    if (data.hasMovingExpression && data.isMoving != pawn.pather.MovingNow)
                    {
                        continue;
                    }
                    if (def.useGendered && data.gender != pawn.gender)
                    {
                        continue;
                    }

                    list.Add(data);
                    if (!curIndexArray.Keys.Contains(data.partName))
                    {
                        curIndexArray.Add(data.partName, 0);
                    }
                    else
                    {
                        curIndexArray[data.partName] = (curIndexArray[data.partName] + 1) % data.frameCount[pawn.Rotation];
                    }
                }
                if (blink)
                {
                    expression.Blinker(false);
                }
                if (damaged)
                {
                    expression.Damager(false);
                }
            }
            else if ((blink || layDownResting) && !def.hasSpecialBlinker && !damaged)
            {
                foreach (DAL_PawnExpressionData data in expression.curComp.faceBlinkLayerList)
                {
                    if (!data.isBlink)
                    {
                        continue;
                    }
                    if (data.corwnType != null && (alienComp == null/* || alienComp.crownType != data.corwnType*/))
                    {
                        continue;
                    }
                    if (data.hasMovingExpression && data.isMoving != pawn.pather.MovingNow)
                    {
                        continue;
                    }
                    if (def.useGendered && data.gender != pawn.gender)
                    {
                        continue;
                    }

                    list.Add(data);
                    if (!curIndexArray.Keys.Contains(data.partName))
                    {
                        curIndexArray.Add(data.partName, 0);
                    }
                    else
                    {
                        curIndexArray[data.partName] = (curIndexArray[data.partName] + 1) % data.frameCount[pawn.Rotation];
                    }
                }
                if (blink)
                {
                    expression.Blinker(false);
                }
            }
            else if (damaged && !def.hasSpecialDamager)
            {
                foreach (DAL_PawnExpressionData data in expression.curComp.faceDamageLayerList)
                {
                    if (!data.isDamage)
                    {
                        continue;
                    }
                    if (data.corwnType != null && (alienComp == null/* || alienComp.crownType != data.corwnType*/))
                    {
                        continue;
                    }
                    if (data.hasMovingExpression && data.isMoving != pawn.pather.MovingNow)
                    {
                        continue;
                    }
                    if (def.useGendered && data.gender != pawn.gender)
                    {
                        continue;
                    }

                    list.Add(data);
                    if (!curIndexArray.Keys.Contains(data.partName))
                    {
                        curIndexArray.Add(data.partName, 0);
                    }
                    else
                    {
                        curIndexArray[data.partName] = (curIndexArray[data.partName] + 1) % data.frameCount[pawn.Rotation];
                    }
                }
                expression.Damager(false);
            }

            if (!list.NullOrEmpty())
            {
                ResolveData(list);
            }
        }

        public virtual void ResolveBoth()
        {

        }

        public virtual void ResolveSplit()
        {

        }

        public virtual void ResolveCrossExpression()
        {

        }

        public virtual void ResolveData(List<DAL_PawnExpressionData> dataList)
        {
            List<DAL_GraphicInfo> list = new List<DAL_GraphicInfo>();
            foreach (DAL_PawnExpressionData data in dataList)
            {
                DAL_GraphicData n = new DAL_GraphicData(data.frame);
                if (data.gender != Gender.None)
                {
                    n.texPath = Path.Combine(n.texPath, data.gender.ToString() + "_" + data.partName);
                }
                n.texPath += curIndexArray[data.partName].ToString();
                list.Add(new DAL_GraphicInfo(n.Graph, false, 0.1f));
            }
            if (!list.NullOrEmpty())
            {
                expression.curGraphicList.Clear();
                expression.curGraphicList = list;
            }
        }

        public virtual void Clear()
        {
            pawn = null;
            def = null;
            expression = null;

            initialization = false;
            curTick = -1;
            blink = false;
            layDownResting = false;
            damaged = false;

            curCrossFlags = DAL_PawnExpressionCrossRenderFlags.None;

            curIndexArray.Clear();

            alienComp = null;

            faceLayerList.Clear();
            eyesLayerList.Clear();
            mouseLayerList.Clear();
    }

        public virtual void ExposeData()
        {
        }
    }
}
