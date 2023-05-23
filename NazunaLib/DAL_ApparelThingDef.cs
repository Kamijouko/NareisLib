using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class DAL_ApparelThingDef : ThingDef
    {
        public bool breastSizeOn = false;

        public List<DAL_BreastSizeReplacePaths> breastSizeReplacePaths = new List<DAL_BreastSizeReplacePaths>();

        public bool useGenderWornGraphic = false;

        public List<DAL_GenderReplacePaths> genderReplacePaths = new List<DAL_GenderReplacePaths>();

        public ShaderTypeDef defaultWornApparelShader;

        public List<DAL_ApparelForDifferentRace> differentRaceGroup = new List<DAL_ApparelForDifferentRace>();
    }
}
