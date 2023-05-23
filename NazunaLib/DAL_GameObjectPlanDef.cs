using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NareisLib
{
    public class DAL_GameObjectPlanDef : Def
    {
        public string packName;

        public string modId;

        public List<DAL_GameObjectPlanData> dataList = new List<DAL_GameObjectPlanData>();

    }
}
