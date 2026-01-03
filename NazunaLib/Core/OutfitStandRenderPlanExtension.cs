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
        public Gender defaultGender = Gender.Male;
        public string raceButtonIconPath;
        public string genderButtonIconPath;
        public string bodyButtonIconPath;
        public string headButtonIconPath;
        public string swapOutfitButtonIconPath;
        public string swapAllApparelButtonIconPath;
        public string modelScaleButtonIconPath;
        public string modelZButtonIconPath;
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
