using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class OutfitStandRenderPlanExtension : DefModExtension
    {
        public string renderPlanDef;
        public Vector3 standOffset = Vector3.zero;
        public List<OutfitStandRaceOffset> raceOffsets;
    }

    public class OutfitStandRaceOffset
    {
        public string raceDefName;
        public Vector3 offset = Vector3.zero;
        public string bodyTexPath;
        public string bodyTexPathMale;
        public string bodyTexPathFemale;
        public string headTexPath;
        public string headTexPathMale;
        public string headTexPathFemale;
    }
}
