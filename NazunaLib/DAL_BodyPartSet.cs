using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace NareisLib
{
    public class DAL_BodyPartSet
    {
        public string planName;

        public int weight;

        public List<DAL_CompAnimatedFrameList> bodyAddonList = new List<DAL_CompAnimatedFrameList>();
    }
}
