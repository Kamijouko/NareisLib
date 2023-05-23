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
    public class DAL_ReplaceApparelProperties
    {
        public List<string> replaceDefName;

        public bool thisBreastSizeOn;

        public bool useGenderWornGraphic;

        public string defaultWornGraphicPath;

        public ShaderTypeDef defaultWornShader;

        public List<DAL_BreastSizeReplacePaths> breastSizeReplacePaths = new List<DAL_BreastSizeReplacePaths>();

        public List<DAL_GenderReplacePaths> genderReplacePaths = new List<DAL_GenderReplacePaths>();
    }
}
