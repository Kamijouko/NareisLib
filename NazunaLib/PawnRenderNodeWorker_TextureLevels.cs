using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class PawnRenderNodeWorker_TextureLevels : PawnRenderNodeWorker
    {
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

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            return base.OffsetFor(node, parms, out pivot);
        }
    }
}
