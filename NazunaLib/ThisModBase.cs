using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HugsLib;

namespace NazunaLib
{
    public class ThisModBase : ModBase
    {
        public override string ModIdentifier { get; } = "NazunaReiLib.kamijouko";

        public override void DefsLoaded()
        {
            base.DefsLoaded();
            ModStaticMethod.ThisMod = this;
        }

        public static int SortSouth()
        {

        }
    }
}
