using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class TextureLevelsToNodeSubWorker : PawnRenderSubWorker
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
        public override bool CanDrawNowSub(PawnRenderNode node, PawnDrawParms parms)
        {
            TextureLevelsToNode tlNode = node as TextureLevelsToNode ?? null;
            if (tlNode == null)
                return true;
            MultiRenderComp comp = parms.pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return true;

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

            return true;
        }

        public override void TransformOffset(PawnRenderNode node, PawnDrawParms parms, ref Vector3 offset, ref Vector3 pivot)
        {
            TextureLevelsToNode tlNode = node as TextureLevelsToNode ?? null;
            if (tlNode == null || tlNode.Props.parentTagDef == PawnRenderNodeTagDefOf.Head)
            {
                base.TransformOffset(node, parms, ref offset, ref pivot);
                return;
            }
            TextureLevels data = tlNode.textureLevels;
            if (!data.offsetToHead)
            {
                base.TransformOffset(node, parms, ref offset, ref pivot);
                return;
            }
                

            if (parms.pawn.RaceProps.Humanlike)
            {
                switch (data.meshType)
                {
                    case "Body":
                        break;
                    case "Head":
                        pivot += BaseHeadOffsetAt(parms.facing, parms.pawn);
                        break;
                    case "Hair":
                        pivot += BaseHeadOffsetAt(parms.facing, parms.pawn);
                        break;
                }
            }

            Vector3 local = offset;
            Vector3 dataOffset = data.DrawOffsetForRot(parms.facing);
            if (data.useStaticYOffset)
                local.y = local.y + dataOffset.y * 0.01f;
            if (data.usePublicYOffset)
                dataOffset.y *= 0.01f;
            else
                dataOffset.y *= 0.0001f;
            Vector3 pos = local + dataOffset;

            offset = pos;

            //base.TransformOffset(node, parms, ref offset, ref pivot);
        }

        public override void TransformRotation(PawnRenderNode node, PawnDrawParms parms, ref Quaternion rotation)
        {
            base.TransformRotation(node, parms, ref rotation);
        }

        public override void TransformScale(PawnRenderNode node, PawnDrawParms parms, ref Vector3 scale)
        {
            base.TransformScale(node, parms, ref scale);
        }

        public override void TransformLayer(PawnRenderNode node, PawnDrawParms parms, ref float layer)
        {
            base.TransformLayer(node, parms, ref layer);
        }

        public override void EditMaterial(PawnRenderNode node, PawnDrawParms parms, ref Material material)
        {
            base.EditMaterial(node, parms, ref material);
        }

        public override void EditMaterialPropertyBlock(PawnRenderNode node, Material material, PawnDrawParms parms, ref MaterialPropertyBlock block)
        {
            base.EditMaterialPropertyBlock(node, material, parms, ref block);
        }
    }
}
