using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace NazunaLib
{
    public class DAL_AnimateRequireVerb
    {
        public List<GraphicData> verbWarmUpFrames = new List<GraphicData>();

        public List<GraphicData> verbWarmUpFramesEast = new List<GraphicData>();

        public List<GraphicData> verbWarmUpFramesWest = new List<GraphicData>();

        public List<GraphicData> verbWarmUpFramesNorth = new List<GraphicData>();

        public List<GraphicData> verbWarmUpFramesSouth = new List<GraphicData>();

        public bool isRot4Frames = false;

        public Type requireVerb;
    }
}
