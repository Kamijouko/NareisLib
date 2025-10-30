using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NareisLib
{
    public class ActionDef : Def
    {
        //行为库，
        //key为JobDef的defName，
        //value为所对应的行为
        public Dictionary<JobDef, Behavior> behaviors = new Dictionary<JobDef, Behavior>();
    }
}
