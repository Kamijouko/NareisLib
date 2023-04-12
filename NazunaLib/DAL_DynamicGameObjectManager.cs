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
    public sealed class DAL_DynamicGameObjectManager : IExposable
    {
        public void ExposeData()
        {

        }

        public DAL_DynamicGameObjectManager()
        {

        }

        public GameObject SetupGameObject(string modId, DAL_GameObjectPlanData data, Transform parent)
        {
            if (!DAL_DynamicGameObjectPrefabManager.Initialized)
            {
                return null;
            }
            string key = modId + ":" + data.name;
            GameObject objPre = DAL_DynamicGameObjectPrefabManager.PreGameObjectDict[key];
            Vector3 pos = new Vector3(parent.position.x, data.yOffset, parent.position.z);
            GameObject obj = UnityEngine.Object.Instantiate(objPre, pos, Quaternion.Euler(90f, 0f, 0f), parent);
            obj.AddComponent<DAL_GameObject>();
            return obj;
        }

        //private Dictionary<string, GameObject> activeGameObjectDict = new Dictionary<string, GameObject>();

        //private Dictionary<string, GameObject> unactiveGameObjectDict = new Dictionary<string, GameObject>();
    }
}
