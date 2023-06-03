using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using AlienRace.ExtendedGraphics;

namespace NareisLib
{
    public class TextureLevelHediffSet
    {
        //填入hediff的defName
        public HediffDef hediff;


        //此hediff的优先级，当这些hediff可以同时存在于某个部位时才须设置，优先渲染数字大的效果贴图
        public int priority = 0;

        //当当前贴图属于以下列表中时才具有此hediff特殊效果
        public List<string> texList = new List<string>();

        //严重度对应的贴图名称前缀列表
        public List<TextureLevelHediffSeveritySet> severity = new List<TextureLevelHediffSeveritySet>();

        public string GetCurHediffPrefix(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel)
        {
            string result = "";
            float tmp = 0f;
            float maxSeverityOfHediff = pawn.SeverityOfHediffsOnPart(hediff, part, partLabel).Max();
            foreach (TextureLevelHediffSeveritySet set in severity)
            {
                if (maxSeverityOfHediff >= set.severity && set.severity >= tmp)
                {
                    tmp = set.severity;
                    result = set.prefix;
                }
            }
            return result;
        }
    }
}
