using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

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
            
            MultiRenderComp comp = parms.pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return base.CanDrawNow(node, parms);

            TextureLevelsToNode tlNode = node as TextureLevelsToNode ?? null;
            if (tlNode == null)
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

        public override void AppendDrawRequests(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
        {
            TextureLevelsToNode tlNode = node as TextureLevelsToNode ?? null;
            if (tlNode == null)
                base.AppendDrawRequests(node, parms, requests);

            Material finalizedMaterial = GetFinalizedMaterial(node, parms);
            if (finalizedMaterial == null)
                return;
            Mesh mesh = tlNode.GetMesh(parms);
            if (mesh == null)
                return;

            requests.Add(new PawnGraphicDrawRequest(node, mesh, finalizedMaterial));

            /*TextureLevels data = tlNode.textureLevels;
            Pawn pawn = parms.pawn;
            Rot4 facing = parms.facing;

            Mesh mesh = null;

            if (data.meshSize != Vector2.zero)
            {
                mesh = NareisLib_MeshPool.GetMeshSetForSize(data.meshSize.x, data.meshSize.y).MeshAt(facing, data.flipped);
            }
            else
            {
                Mesh bodyMesh;
                NareisLib_GraphicMeshSet bodyMeshSet = null;
                NareisLib_GraphicMeshSet hairMeshSet = null;
                NareisLib_GraphicMeshSet headMeshSet = null;
                if (pawn.RaceProps.Humanlike)
                {
                    bodyMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeBodySetForPawn(pawn);
                    headMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
                    hairMeshSet = NareisLib_MeshPoolUtility.GetHumanlikeHairSetForPawn(pawn);
                    bodyMesh = bodyMeshSet.MeshAt(facing);
                }
                else
                    bodyMesh = tlNode.GetMesh(parms);

                if (pawn.RaceProps.Humanlike)
                {
                    switch (data.meshType)
                    {
                        case "Body": mesh = bodyMeshSet.MeshAt(facing, data.flipped); break;
                        case "Head": mesh = headMeshSet.MeshAt(facing, data.flipped); break;
                        case "Hair": mesh = hairMeshSet.MeshAt(facing, data.flipped); break;
                    }
                }
                else
                    mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(GetGraphic(node, parms), facing) : bodyMesh;
                if (mesh == null)
                    return;
            }*/

            /*TextureLevels data = tlNode.textureLevels;
            Pawn pawn = parms.pawn;
            Rot4 facing = parms.facing;

            Mesh mesh = null;
            if (pawn.RaceProps.Humanlike)
                mesh = tlNode.nMeshSet.MeshAt(facing, data.flipped);
            else
                mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(GetGraphic(node, parms), facing) : tlNode.GetMesh(parms);
            if (mesh == null)
                return;
            requests.Add(new PawnGraphicDrawRequest(node, mesh, finalizedMaterial));*/
        }
    }
}
