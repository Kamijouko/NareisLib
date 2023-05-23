using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NareisLib
{
    public class RenderBatch : IComparer<RenderBatch>
    {
        public List<Graphic> list = new List<Graphic>();

        public int Compare(RenderBatch x, RenderBatch y)
        {
            throw new NotImplementedException();
        }
    }
}
