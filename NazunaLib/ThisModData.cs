using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NazunaLib
{
    public static class ThisModData
    {
        public static Dictionary<string, GraphicData> TexLevelsDatabase = new Dictionary<string, GraphicData>();

        public static Dictionary<string, MultiTexDef> DefAndKeyDatabase = new Dictionary<string, MultiTexDef>();

        public static int TmpLevelID = 0;
    }
}
