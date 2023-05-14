using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NazunaLib
{
    public class TextureLevelHediffSeveritySet
    {
        //当前hediff的严重度
        public float severity = 0f;

        //hediff效果贴图对应文件名前缀，加在原贴图prefix之前
        public string prefix = "";
    }
}
