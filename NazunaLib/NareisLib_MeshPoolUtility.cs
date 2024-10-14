using AlienRace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public static class NareisLib_MeshPoolUtility
    {
        public static NareisLib_GraphicMeshSet GetHumanlikeBodySetForPawn(Pawn pawn)
        {
            float lifestageFactor;
            if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.bodyWidth != null)
            {
                lifestageFactor = pawn.ageTracker.CurLifeStage.bodyWidth.Value;
                //return NareisLib_MeshPool.GetMeshSetForWidth(pawn.ageTracker.CurLifeStage.bodyWidth.Value);
            }
            else
                lifestageFactor = 1.5f;
            Vector2 ? vector;
            if (!AlienRenderTreePatches.IsPortrait(pawn))
            {
                AlienPartGenerator.AlienComp comp = pawn.GetComp<AlienPartGenerator.AlienComp>();
                vector = ((comp != null) ? new Vector2?(comp.customDrawSize) : null);
            }
            else
            {
                AlienPartGenerator.AlienComp comp2 = pawn.GetComp<AlienPartGenerator.AlienComp>();
                vector = ((comp2 != null) ? new Vector2?(comp2.customPortraitDrawSize) : null);
            }
            Vector2 vector2 = vector ?? Vector2.one;
            Vector2 vector4 = new Vector2(lifestageFactor, lifestageFactor);
            return NareisLib_MeshPool.GetMeshSetForWidth(vector2.x * vector4.x, vector2.y * vector4.y);
            //return NareisLib_MeshPool.humanlikeBodySet;
        }

        public static NareisLib_GraphicMeshSet GetHumanlikeHairSetForPawn(Pawn pawn)
        {
            Vector2 vec = pawn.story.headType.hairMeshSize;
            if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.headSizeFactor != null)
            {
                vec *= pawn.ageTracker.CurLifeStage.headSizeFactor.Value;
            }
            Vector2? vector;
            if (!AlienRenderTreePatches.IsPortrait(pawn))
            {
                AlienPartGenerator.AlienComp comp = pawn.GetComp<AlienPartGenerator.AlienComp>();
                vector = ((comp != null) ? new Vector2?(comp.customHeadDrawSize) : null);
            }
            else
            {
                AlienPartGenerator.AlienComp comp2 = pawn.GetComp<AlienPartGenerator.AlienComp>();
                vector = ((comp2 != null) ? new Vector2?(comp2.customPortraitHeadDrawSize) : null);
            }
            vec = (vector ?? Vector2.one) * vec;
            return NareisLib_MeshPool.GetMeshSetForWidth(vec.x, vec.y);
        }

        public static NareisLib_GraphicMeshSet GetHumanlikeHeadSetForPawn(Pawn pawn)
        {
            float lifestageFactor;
            if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.bodyWidth != null)
            {
                lifestageFactor = pawn.ageTracker.CurLifeStage.bodyWidth.Value;
                //return NareisLib_MeshPool.GetMeshSetForWidth(pawn.ageTracker.CurLifeStage.bodyWidth.Value);
            }
            else
                lifestageFactor = 1.5f;
            Vector2? vector;
            if (!AlienRenderTreePatches.IsPortrait(pawn))
            {
                AlienPartGenerator.AlienComp comp = pawn.GetComp<AlienPartGenerator.AlienComp>();
                vector = ((comp != null) ? new Vector2?(comp.customHeadDrawSize) : null);
            }
            else
            {
                AlienPartGenerator.AlienComp comp2 = pawn.GetComp<AlienPartGenerator.AlienComp>();
                vector = ((comp2 != null) ? new Vector2?(comp2.customPortraitHeadDrawSize) : null);
            }
            Vector2 vector2 = vector ?? Vector2.one;
            Vector2 vector4 = new Vector2(lifestageFactor, lifestageFactor);
            return NareisLib_MeshPool.GetMeshSetForWidth(vector2.x * vector4.x, vector2.y * vector4.y);
            //return NareisLib_MeshPool.humanlikeHeadSet;
        }
    }
}
