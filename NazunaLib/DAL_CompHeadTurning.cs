using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace NareisLib
{
    public class DAL_CompHeadTurning : ThingComp
    {
        public Rot4 headFacing;

        public Pawn pawn;

        //public bool shouldUpdate = false;

        public bool init = false;

        private bool canBeTurning = true;

        private int curTick = 0;

        private int curTurningTick = 0;

        private int curCounter = 0;

        private Rot4 curBodyRotation;

        public DAL_CompProperties_HeadTurning Props
        {
            get
            {
                return (DAL_CompProperties_HeadTurning)props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Props.enable)
            {
                UpdateHeadFacing();
            }
        }

        public void UpdateHeadFacing()
        {
            pawn = parent as Pawn;
            if (pawn != null && pawn.Spawned && !pawn.Dead && !pawn.Downed)
            {
                if (!init)
                {
                    headFacing = pawn.Rotation;
                    curBodyRotation = pawn.Rotation;
                    init = true;
                }
                if (pawn.jobs.curDriver?.asleep == true || (pawn.stances?.curStance != null && pawn.stances?.curStance as Stance_Busy != null))
                {
                    return;
                }
                if (Find.TickManager.TicksGame > curTurningTick && headFacing != curBodyRotation)
                {
                    headFacing = curBodyRotation;
                    //shouldUpdate = true;
                }

                if (curBodyRotation != pawn.Rotation)
                {
                    headFacing = pawn.Rotation;
                    curBodyRotation = pawn.Rotation;
                    //shouldUpdate = true;
                }

                if (curCounter >= Props.turningLimit)
                {
                    canBeTurning = false;
                    curTick = Props.coolDownSeconds.SecondsToTicks() + Find.TickManager.TicksGame;
                }
                if (Find.TickManager.TicksGame > curTick && canBeTurning == false)
                {
                    canBeTurning = true;
                    curCounter = 0;
                }
            }
        }

        public void TurningCaculate(Pawn target, bool notCounter = false)
        {
            Rot4 result = pawn.Rotation;
            if (canBeTurning)
            {
                IntVec3 pos1 = pawn.Position;
                IntVec3 pos2 = target.Position;
                IntVec3 dist = pos2 - pos1;
                float angle = (float)(Math.Atan(dist.z / (dist.x != 0f ? dist.x : 0.01f)) * 180 / Math.PI);
                if (dist.x < 0)
                {
                    if (dist.z > 0)
                    {
                        angle = GenMath.PositiveMod(angle, 180f);
                    }
                    else
                    {
                        angle += 180f;
                    }
                }
                else
                {
                    angle = GenMath.PositiveMod(angle, 360f);
                }
                result = AngleCaculate(result, angle);
                if (result != pawn.Rotation)
                {
                    curBodyRotation = pawn.Rotation;
                    curTurningTick = Rand.Range(Props.minTurningSeconds, Props.maxTurningSeconds).SecondsToTicks() + Find.TickManager.TicksGame;
                    /*if (!notCounter)
                    {
                        curCounter++;
                    }*/
                    //shouldUpdate = true;
                }
            }
            headFacing = result;
        }

        private Rot4 AngleCaculate(Rot4 result, float angle)
        {
            if (result == Rot4.South)
            {
                if (angle <= 91f && angle >= 89f)
                {
                    return Rot4.South;
                }
                if ((angle < 89f && angle >= 0f) || (angle <= 360f && angle >= 315))
                {
                    return Rot4.East;
                }
                if (angle > 91f && angle <= 225f)
                {
                    return Rot4.West;
                }
                if (angle > 225f && angle < 315f)
                {
                    return Rot4.South;
                }
            }

            if (result == Rot4.North)
            {
                if (angle <= 271f && angle >= 269f)
                {
                    return Rot4.North;
                }
                if ((angle > 271f && angle <= 360f) || (angle <= 45f && angle >= 0f))
                {
                    return Rot4.East;
                }
                if (angle < 269f && angle >= 135f)
                {
                    return Rot4.West;
                }
                if (angle < 135f && angle > 45f)
                {
                    return Rot4.North;
                }
            }

            if (result == Rot4.East)
            {
                if (angle <= 181f && angle >= 179f)
                {
                    return Rot4.East;
                }
                if (angle > 181f && angle <= 315f)
                {
                    return Rot4.South;
                }
                if (angle < 179f && angle >= 45f)
                {
                    return Rot4.North;
                }
                if ((angle < 45f && angle >= 0f) || (angle > 315f && angle <= 360f))
                {
                    return Rot4.East;
                }
            }

            if (result == Rot4.West)
            {
                if (angle <= 359f && angle >= 1f)
                {
                    return Rot4.West;
                }
                if (angle < 359f && angle >= 225f)
                {
                    return Rot4.South;
                }
                if (angle > 1f && angle <= 135f)
                {
                    return Rot4.North;
                }
                if (angle > 135f && angle < 225f)
                {
                    return Rot4.West;
                }
            }

            return result;
        }
    }
}
