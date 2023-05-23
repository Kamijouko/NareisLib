using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using AlienRace;
using UnityEngine;

namespace NareisLib
{
    public class DAL_AlienRaceThingDef : ThingDef_AlienRace
    {
        public override void ResolveReferences()
        {
            base.ResolveReferences();
        }

        public List<DAL_PawnBreastInfo> breastSizeList = new List<DAL_PawnBreastInfo>();

        public List<DAL_GraphicPaths> dalGraphicPaths = new List<DAL_GraphicPaths>();

        public List<DAL_BodyTypeLimitThreshold> bodyTypeGroup = new List<DAL_BodyTypeLimitThreshold>();

        public bool isFemaleRace;

        public DAL_ReplaceApparelGraphicPaths replaceApparelGraphicPaths;
    }
}
