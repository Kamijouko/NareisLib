using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace NareisLib
{
    public class RenderPlanDef : Def
    {
        public List<string> races = new List<string>();
        public List<MultiTexDef> plans = new List<MultiTexDef>();

        public int combinePriority = -1;
        public string actionSettingGizmo_Label = "";
        public string actionSettingGizmo_Desc = "";
        public string actionSettingGizmo_IconPath = "";

        //xml里不需要设置
        public List<string> combinedPlanDefNames = new List<string>();

        public RenderPlanDef()
        {

        }

        public RenderPlanDef(string race)
        {
            defName = race + "_planDef_combine";
            races.Add(race);
        }

        public void Combine(RenderPlanDef def)
        {
            //races = races.Concat(def.races).Distinct().ToList();
            foreach (MultiTexDef multiDef in def.plans)
            {
                if (!plans.Exists(x => x.defName == multiDef.defName))
                    plans.Add(multiDef);
            }
            if (combinePriority < def.combinePriority)
            {
                combinePriority = def.combinePriority;
                if (!def.actionSettingGizmo_Label.NullOrEmpty())
                    actionSettingGizmo_Label = def.actionSettingGizmo_Label;
                if (!def.actionSettingGizmo_Desc.NullOrEmpty())
                    actionSettingGizmo_Desc = def.actionSettingGizmo_Desc;
                if (!def.actionSettingGizmo_IconPath.NullOrEmpty())
                    actionSettingGizmo_IconPath = def.actionSettingGizmo_IconPath;
            }
        }
    }
}
