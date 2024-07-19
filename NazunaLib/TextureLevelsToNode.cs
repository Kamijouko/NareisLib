using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NareisLib
{
    public class TextureLevelsToNode : PawnRenderNode
    {
        public TextureLevelsToNode(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props, tree)
        {
        }

        public TextureLevelsToNodeProperties CurProps
        {
            get
            {
                return (TextureLevelsToNodeProperties)Props;
            }
        }

        public TextureLevels textureLevels;

        public MultiTexBatch multiTexBatch;

        public MultiRenderComp comp;

        public override Graphic GraphicFor(Pawn pawn)
        {
            return base.GraphicFor(pawn);
        }
    }
}
