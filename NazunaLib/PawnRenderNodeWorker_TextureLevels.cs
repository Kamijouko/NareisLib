using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements.Experimental;
using Verse;
using static RimWorld.MechClusterSketch;

namespace NareisLib
{
    public class PawnRenderNodeWorker_TextureLevels : PawnRenderNodeWorker
    {
        public Vector3 BaseHeadOffsetAt(Rot4 rotation, Pawn pawn)
        {
            Vector2 vector = pawn.story.bodyType.headOffset * Mathf.Sqrt(pawn.ageTracker.CurLifeStage.bodySizeFactor);
            switch (rotation.AsInt)
            {
                case 0:
                    return new Vector3(0f, 0f, vector.y);
                case 1:
                    return new Vector3(vector.x, 0f, vector.y);
                case 2:
                    return new Vector3(0f, 0f, vector.y);
                case 3:
                    return new Vector3(-vector.x, 0f, vector.y);
                default:
                    Log.Error("BaseHeadOffsetAt error in " + pawn);
                    return Vector3.zero;
            }
        }

        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            TextureLevelsToNode tlNode = node as TextureLevelsToNode ?? null;
            if (tlNode == null)
                return base.CanDrawNow(node, parms);
            MultiRenderComp comp = parms.pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return base.CanDrawNow(node, parms);

            TextureLevels data = tlNode.textureLevels;
            MultiTexBatch batch = tlNode.multiTexBatch;
            string mlPrefixName = batch.multiTexDefName + "_";

            if ((!comp.cachedHideOrReplaceDict.NullOrEmpty()
                    && comp.cachedHideOrReplaceDict.ContainsKey(mlPrefixName + data.textureLevelsName)
                    && comp.cachedHideOrReplaceDict[mlPrefixName + data.textureLevelsName].hide)
                || !data.CanRender(parms.pawn, batch.keyName))
                return false;

            if (batch.renderSwitch.x == 0 && parms.facing == Rot4.South)
                return false;
            if (batch.renderSwitch.y == 0 && (parms.facing == Rot4.East || parms.facing == Rot4.West))
                return false;
            if (batch.renderSwitch.z == 0 && parms.facing == Rot4.North)
                return false;

            return base.CanDrawNow(node, parms);
        }

        protected override Vector3 PivotFor(PawnRenderNode node, PawnDrawParms parms)
        {
            TextureLevelsToNode tlNode = node as TextureLevelsToNode ?? null;
            if (tlNode == null || tlNode.Props.parentTagDef == PawnRenderNodeTagDefOf.Head)
                return base.PivotFor(node, parms);
            TextureLevels data = tlNode.textureLevels;
            if (!data.offsetToHead)
                return base.PivotFor(node, parms);

            Vector3 offset = base.PivotFor(node, parms);
            if (parms.pawn.RaceProps.Humanlike)
            {
                switch (data.meshType)
                {
                    case "Body":
                        break;
                    case "Head":
                        offset += BaseHeadOffsetAt(parms.facing, parms.pawn);
                        break;
                    case "Hair":
                        offset += BaseHeadOffsetAt(parms.facing, parms.pawn);
                        break;
                }
            }
            return offset;
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            TextureLevelsToNode tlNode = node as TextureLevelsToNode ?? null;
            if (tlNode == null)
                return base.OffsetFor(node, parms, out pivot);
            TextureLevels data = tlNode.textureLevels;

            Vector3 local = base.OffsetFor(node, parms, out pivot);
            pivot = PivotFor(node, parms);
            Vector3 dataOffset = data.DrawOffsetForRot(parms.facing);
            if (data.useStaticYOffset)
                local.y = local.y + dataOffset.y * 0.01f;
            if (data.usePublicYOffset)
                dataOffset.y *= 0.01f;
            else
                dataOffset.y *= 0.0001f;
            Vector3 pos = local + dataOffset;

            return pos;
        }
    }
}
