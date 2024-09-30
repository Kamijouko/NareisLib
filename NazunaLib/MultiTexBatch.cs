using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace NareisLib
{
    public class MultiTexBatch : IExposable
    {
        //每个Batch对应一个层的数据

        public Type originalDefClass;

        public string originalDefName;

        public string multiTexDefName;

        public string keyName;

        public string textureLevelsName;

        public TextureRenderLayer layer;

        public bool staticLayer;

        public bool donotChangeLayer;

        //public List<string> keyList = new List<string>();

        //public List<string> keyListSouth = new List<string>();
        //public List<string> keyListEast = new List<string>();
        //public List<string> keyListNorth = new List<string>();
        //public List<string> keyListWest = new List<string>();

        public Vector3 renderSwitch = new Vector3(1f, 1f, 1f);
        public IntVec3 saveRenderSwitch;

        public string GetMultiTexDef_TextureLevelsDefName
        {
            get { return $"{multiTexDefName}_{textureLevelsName}"; }
        }

        public string GetType_OriginalDefName
        {
            get { return $"{originalDefClass.ToStringSafe()}_{originalDefName}"; }
        }

        public MultiTexBatch()
        {

        }

        public MultiTexBatch(System.Type type, string original, string def, string key, string levelsName, TextureRenderLayer la, Vector3 r, bool stLayer = false, bool dnChangeLayer = false/*, List<string> list*/)
        {
            originalDefClass = type;
            originalDefName = original;
            multiTexDefName = def;
            keyName = key;
            textureLevelsName = levelsName;
            layer = la;
            renderSwitch = r;
            staticLayer = stLayer;
            donotChangeLayer = dnChangeLayer;
        }

        public MultiTexBatch Clone()
        {
            return new MultiTexBatch(originalDefClass, originalDefName, multiTexDefName, keyName, textureLevelsName, layer, renderSwitch, staticLayer, donotChangeLayer);
        }

        public void ExposeData()
        {
            //Log.Warning("Batch Loading");
            
            Scribe_Values.Look<Type>(ref originalDefClass, "originalDefClass", null, false);

            Scribe_Values.Look<string>(ref originalDefName, "originalDefName", null, false);
            Scribe_Values.Look<string>(ref multiTexDefName, "multiTexDefName", null, false);
            Scribe_Values.Look<string>(ref keyName, "keyName", null, false);
            Scribe_Values.Look<string>(ref textureLevelsName, "textureLevelsName", null, false);
            Scribe_Values.Look<bool>(ref staticLayer, "staticLayer", false, false);
            Scribe_Values.Look<bool>(ref donotChangeLayer, "donotChangeLayer", false, false);
            Scribe_Values.Look<TextureRenderLayer>(ref layer, "layer", TextureRenderLayer.None, false);

            if (Scribe.mode == LoadSaveMode.Saving)
                saveRenderSwitch = new IntVec3((int)renderSwitch.x, (int)renderSwitch.y, (int)renderSwitch.z);
            Scribe_Values.Look<IntVec3>(ref saveRenderSwitch, "saveRenderSwitch", default(IntVec3), false);
            if (Scribe.mode == LoadSaveMode.LoadingVars && saveRenderSwitch != IntVec3.Invalid)
                renderSwitch = saveRenderSwitch.ToVector3();
        }
    }
}
