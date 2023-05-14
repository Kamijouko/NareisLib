using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace NazunaLib
{
    public static class DAL_DynamicGameObjectPrefabManager
    {
        public static void InitGameObjectToList()
        {
            /*initialized = false;
            GameObjectPlanListDef def = GOPListDefOf.DAL_GOPList;
            if (def == null || (def != null && def.gameObjectPlanList.NullOrEmpty()))
                return;
            foreach (DAL_GameObjectPlanDef plan in def.gameObjectPlanList)
            {
                string path = Path.Combine(ModLister.GetActiveModWithIdentifier("NazunaReiLib.kamijouko").RootDir.ToString(), "Assets");
                AssetBundle pack = AssetBundle.LoadFromFile(Path.Combine(path, plan.packName));
                foreach (DAL_GameObjectPlanData data in plan.dataList)
                {
                    string key = plan.modId + ":" + data.name;
                    if (!preGameObjectDict.ContainsKey(key))
                    {
                        GameObject objPre = pack.LoadAsset<GameObject>(data.name);
                        preGameObjectDict.Add(key, objPre);
                    }
                }
                pack.Unload(false);
            }
            initialized = true;*/
        }

        public static bool Initialized
        {
            get
            {
                return initialized;
            }
        }

        public static Dictionary<string, GameObject> PreGameObjectDict
        {
            get
            {
                return preGameObjectDict;
            }
        }

        private static bool initialized = false;

        private static Dictionary<string, GameObject> preGameObjectDict = new Dictionary<string, GameObject>();
    }
}
