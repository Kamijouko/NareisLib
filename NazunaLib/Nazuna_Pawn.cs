using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace NazunaLib
{
    public class Nazuna_Pawn : Pawn
    {
        public string breastSize;

        public bool genderHasBeFixed = false;

        //public DAL_Seirei seireiSlot = null;

        //public List<DAL_SeireiImagineTagDef> imagineTags = new List<DAL_SeireiImagineTagDef>();

        public DAL_AlienRaceThingDef DalDef
        {
            get
            {
                return (DAL_AlienRaceThingDef)this.def;
            }
        }

        public override void Tick()
        {
            base.Tick();
            /*if (seireiSlot != null && seireiSlot.transfor == (DAL_SeireiTransfor.Full | DAL_SeireiTransfor.Half))
            {
                seireiSlot.DALResolveGameObject(DrawPos);
            }*/
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref this.breastSize, "breastSize", null, true);
            Scribe_Values.Look<bool>(ref this.genderHasBeFixed, "genderHasBeFixed", false, true);
            //Scribe_Deep.Look<DAL_Seirei>(ref seireiSlot, "seireiSlot", new object[]{ this });
        }
    }
}
