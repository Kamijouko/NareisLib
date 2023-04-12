using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
//using CompAnimated;

namespace NazunaLib
{
    public class DAL_CompProperties_Animated : CompProperties
    {
        public DAL_CompProperties_Animated()
        {
            this.compClass = typeof(DAL_CompAnimated);
        }

        public bool isPawn = false;
        public bool isApparel = false;
        public bool isThing = false;

        public List<DAL_AnimateRequireVerb> requireWarmUpVerbs = new List<DAL_AnimateRequireVerb>();

        public float secondsBetweenFrames;

        public SoundDef sound;

        public DAL_AnimeHairGraphicPlanDef hairPlan;
        public DAL_AnimeBodyPartPlanDef addonPlan;
        public DAL_GameObjectPlanDef objectPlan;

        public List<DAL_CompAnimatedFrameList> pawnFrameList = new List<DAL_CompAnimatedFrameList>();
        public List<DAL_CompAnimatedFrameList> apparelFrameList = new List<DAL_CompAnimatedFrameList>();
        public List<DAL_CompAnimatedFrameList> thingFrameList = new List<DAL_CompAnimatedFrameList>();
        public List<DAL_CompAnimatedFrameList> customFrameList = new List<DAL_CompAnimatedFrameList>();
    }
}
