using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NareisLib
{
    public class DefaultNodeSubWorker : PawnRenderSubWorker
    {
        public override bool CanDrawNowSub(PawnRenderNode node, PawnDrawParms parms)
        {
            
            MultiRenderComp comp = parms.pawn.GetComp<MultiRenderComp>();
            if (comp == null)
                return base.CanDrawNowSub(node, parms);
            if (comp.GetAllHideOriginalDefData.Contains(node.Props.tagDef.defName) 
                || (node.apparel != null && comp.GetAllHideOriginalDefData.Contains(node.apparel.def.defName)))
                return false;
            /*TextureLevelsToNode tlNode = node as TextureLevelsToNode ?? null;
            if (tlNode == null)
                return true;*/

            //TextureLevels data = tlNode.textureLevels;
            //MultiTexBatch batch = tlNode.multiTexBatch;

            /*if (comp.TryGetStoredTextureLevels(batch.GetType_OriginalDefName, batch.textureLevelsName, out data))
            { }*/
            


            return base.CanDrawNowSub(node, parms);
        }
    }
}
