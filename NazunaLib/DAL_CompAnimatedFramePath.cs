using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace NazunaLib
{
    public class DAL_CompAnimatedFramePath
    {
        public string name;

        public bool thingHasMoveAnime = false;
        public bool thingHasDirectAnime = false;

        public bool isBackGraph = false;
        public bool isStill = false;
        public bool isMoving = false;
        public bool isPrimary = false;


        public bool hairBreastOn = false;
        public bool headBreastOn = false;
        public bool apparelBreastOn = false;
        public string breastSize;

        public bool hairBodySetOn = false;
        public bool headBodySetOn = false;
        public bool apparelBodySetOn = false;
        public BodyTypeDef bodyType;

        public string hairDefName;

        public string bodyPartCustomName;
        public List<BodyPartGroupDef> bodyPartGroups = new List<BodyPartGroupDef>();

        public bool hasHediffEffect = false;
        public bool isHediffEffect = false;
        public List<DAL_AnimeHediffSet> replaceHediffSet = new List<DAL_AnimeHediffSet>();

        public float meshSize = -1f;
        public Dictionary<Rot4, int> frameCount = new Dictionary<Rot4, int>();
        public Rot4 direction;
        public float defaultYOffSet;
        public DAL_Rot4LayerSet ySetFacing;

        //public List<DAL_RendLayerSet> ySetList;

        public GraphicData frame;
    }
}
