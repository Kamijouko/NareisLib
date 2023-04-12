using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace NazunaLib
{
    public class DAL_ApparelForDifferentRace
    {
        public List<string> raceDefNameList;

        public ShaderTypeDef defaultReplaceshader;

        public string defaultReplaceWornGraphicPath;

        public bool breastSizeOn = false;

        public List<DAL_BreastSizeReplacePaths> breastSizeReplacePaths = new List<DAL_BreastSizeReplacePaths>();

        public bool useGenderWornGraphic = false;

        public List<DAL_GenderReplacePaths> genderReplacePaths = new List<DAL_GenderReplacePaths>();
    }
}
