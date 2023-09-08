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
        //填入Hediff的defName
        public HediffDef hediff;

        //控制此Hediff效果是否在Job效果存在时渲染，
        //如果开启的话必须为当前的Job效果也准备一份Hediff效果贴图
        public bool enableWithJob = false;

        //此Hediff的优先级，当这些Hediff可以同时存在于某个部位时才须设置，优先渲染数字大的效果贴图
        public int priority = 0;

        //当当前贴图属于以下列表中时才具有此hediff特殊效果
        public List<string> texList = new List<string>();

        //严重度对应的贴图名称前缀列表
        public List<TextureLevelHediffSeveritySet> severity = new List<TextureLevelHediffSeveritySet>();

        //是否使用严重度，适用于不具有严重度的Hediff
        public bool useSeverity = true;

        //不使用严重度时应用的前缀
        public string noSeverityPrefix = "";

        public string GetCurHediffPrefix(ExtendedGraphicsPawnWrapper pawn, BodyPartDef part, string partLabel)
        {
            string result = "";
            if (useSeverity)
            {
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
            }
            else if (pawn.HasHediffOfDefAndPart(hediff, part, partLabel))
            {
                result = noSeverityPrefix;
            }
            return result;
        }
    }
}
