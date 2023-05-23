using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace NareisLib
{
    public static class ModStaticMethod
    {
        public static bool AllLevelsLoaded { get; set; } = false;

        public static string RootDir
        {
            get
            {
                return ThisMod == null ? ModLister.GetActiveModWithIdentifier("NazunaReiLib.kamijouko").RootDir.ToString() : ThisMod.ModContentPack.RootDir;
            }
        }

        public static ThisModBase ThisMod { get; set; } = null;
    }
}
