using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace NazunaLib
{
    public class DAL_CompAnimatedFrameList
    {
        public bool isHair;

        public bool isHead;

        public bool isHeadAddon;

        public bool isBody;

        public bool isBodyAddon;

        public bool isApparel;

        public bool isThing;

        public bool isOverlay;

        public bool onlyMale = false;

        public bool onlyFemale = false;

        public DAL_CompAnimatedFramePath framePaths;

        public int dropFrameCount;

        public GraphicData dropFrame;
    }
}
