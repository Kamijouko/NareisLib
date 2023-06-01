using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace NareisLib
{
    public class MultiTexBatch : IExposable
    {
        //每个Batch对应一个层的数据

        public System.Type originalDefClass = null;

        public string originalDefName = "";

        public string multiTexDefName = "";

        public string keyName = "";

        public string textureLevelsName = "";

        public TextureRenderLayer layer = TextureRenderLayer.None;

        //public List<string> keyList = new List<string>();

        //public List<string> keyListSouth = new List<string>();
        //public List<string> keyListEast = new List<string>();
        //public List<string> keyListNorth = new List<string>();
        //public List<string> keyListWest = new List<string>();

        public Vector3 renderSwitch = Vector3.one;
        public IntVec3 saveRenderSwitch;

        public MultiTexBatch(System.Type type, string original, string def, string key, string levelsName, TextureRenderLayer la, Vector3 r/*, List<string> list*/)
        {
            originalDefClass = type;
            originalDefName = original;
            multiTexDefName = def;
            keyName = key;
            textureLevelsName = levelsName;
            layer = la;
            //keyList = list;
            renderSwitch = r;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<System.Type>(ref originalDefClass, "originalDefClass", null, false);
            Scribe_Values.Look<string>(ref originalDefName, "originalDefName", "", false);
            Scribe_Values.Look<string>(ref multiTexDefName, "multiTexDefName", "", false);
            Scribe_Values.Look<string>(ref keyName, "keyName", "", false);
            Scribe_Values.Look<string>(ref textureLevelsName, "textureLevelsName", "", false);
            Scribe_Values.Look<TextureRenderLayer>(ref layer, "layer", TextureRenderLayer.None, false);
            //Scribe_Collections.Look<string>(ref keyList, "keyList", LookMode.Value, Array.Empty<object>());

            saveRenderSwitch = new IntVec3(renderSwitch);
            Scribe_Values.Look<IntVec3>(ref saveRenderSwitch, "saveRenderSwitch", IntVec3.Invalid, false);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (saveRenderSwitch != IntVec3.Invalid)
                {
                    renderSwitch = saveRenderSwitch.ToVector3();
                }
            }
        }
    }
}
