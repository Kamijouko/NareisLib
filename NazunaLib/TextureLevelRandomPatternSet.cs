using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace NazunaLib
{
    public class TextureLevelRandomPatternSet
    {
        public List<string> texList = new List<string>();

        public int patterns = 1;

        public Vector2Int timeIntervalOfTicks = new Vector2Int(1200, 1800);

        public string keyName = "";
        public int cachedPattern = 1;
        public int cachedActionTimeOfTicks = 0;

        public int RandomNextIntervalAndPattern()
        {
            int tick = Find.TickManager.TicksGame;
            cachedPattern = Rand.Range(1, patterns);
            return cachedActionTimeOfTicks = Rand.Range(tick + timeIntervalOfTicks.x, tick + timeIntervalOfTicks.y);
        }
    }
}
