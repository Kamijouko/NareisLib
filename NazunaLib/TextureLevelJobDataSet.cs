using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using AlienRace.ExtendedGraphics;

namespace NazunaLib
{
    public class TextureLevelJobDataSet
    {
        //需要识别的jobDef
        public JobDef job;

        //当当前贴图属于以下列表中时才具有此job特殊效果贴图
        public List<string> texList = new List<string>();

        //key为job时的姿势，value为对应的贴图前缀
        public Dictionary<PawnPosture, string> rendPostures;

        public bool rendMoving = true;
        public bool rendUnmoving = true;

        public bool IsApplicable(ExtendedGraphicsPawnWrapper pawn, out string prefix)
        {
            return (!rendPostures.TryGetValue(pawn.GetPosture(), out prefix) || (prefix != null)) && ((rendMoving && pawn.Moving) || (rendUnmoving && !pawn.Moving));
        }
    }
}
