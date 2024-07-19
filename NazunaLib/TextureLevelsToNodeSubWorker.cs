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
        public override bool CanDrawNowSub(PawnRenderNode node, PawnDrawParms parms)
        {
            

            return base.CanDrawNowSub(node, parms);
        }

        public override void TransformOffset(PawnRenderNode node, PawnDrawParms parms, ref Vector3 offset, ref Vector3 pivot)
        {
            base.TransformOffset(node, parms, ref offset, ref pivot);
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
