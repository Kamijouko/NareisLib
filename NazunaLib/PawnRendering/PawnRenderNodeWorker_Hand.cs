using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class PawnRenderNodeWorker_Hand : PawnRenderNodeWorker_Body
    {
        public override void AppendDrawRequests(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
        {
            TextureLevelsToNode tlNode = node as TextureLevelsToNode ?? null;
            if (tlNode == null)
                base.AppendDrawRequests(node, parms, requests);

            Material finalizedMaterial = GetFinalizedMaterial(node, parms);
            if (finalizedMaterial == null)
            {
                return;
            }

            TextureLevels data = tlNode.textureLevels;
            Pawn pawn = parms.pawn;
            Rot4 facing = parms.facing;
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
            {
                bodyMesh = node.GetMesh(parms);
            }

            Mesh mesh = null;
            if (data.meshSize != Vector2.zero)
            {
                mesh = NareisLib_MeshPool.GetMeshSetForSize(data.meshSize.x, data.meshSize.y).MeshAt(facing, data.flipped);
            }
            else
            {
                if (pawn.RaceProps.Humanlike)
                {
                    switch (data.meshType)
                    {
                        case "Body": mesh = bodyMeshSet.MeshAt(facing, data.flipped); break;
                        case "Head":
                            mesh = headMeshSet.MeshAt(facing, data.flipped);
                            break;
                        case "Hair":
                            mesh = hairMeshSet.MeshAt(facing, data.flipped);
                            break;
                    }
                }
                else
                    mesh = data.flipped ? NareisLib_MeshPool.FlippedMeshAt(GetGraphic(node, parms), facing) : bodyMesh;
            }
            if (mesh == null)
            {
                return;
            }

            requests.Add(new PawnGraphicDrawRequest(node, mesh, finalizedMaterial));
        }
    }
}
