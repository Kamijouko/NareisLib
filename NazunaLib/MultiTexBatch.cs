using System;
using System.Collections.Generic;
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

        public string multiTexDefName = "";

        public string keyName = "";

        public TextureRenderLayer layer = TextureRenderLayer.None;

        //public List<string> keyList = new List<string>();

        //public List<string> keyListSouth = new List<string>();
        //public List<string> keyListEast = new List<string>();
        //public List<string> keyListNorth = new List<string>();
        //public List<string> keyListWest = new List<string>();

        public Vector3 renderSwitch = Vector3.one;
        public IntVec3 saveRenderSwitch;

        public MultiTexBatch(string def, string key, TextureRenderLayer la, Vector3 r/*, List<string> list*/)
        {
            multiTexDefName = def;
            keyName = key;
            layer = la;
            //keyList = list;
            renderSwitch = r;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref multiTexDefName, "multiTexDefNamr", "", false);
            Scribe_Values.Look<string>(ref keyName, "keyName", "", false);
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
