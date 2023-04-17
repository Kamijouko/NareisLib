using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NazunaLib
{
    public class MultiTexBatch : IExposable
    {
        public TextureRenderLayer layer = TextureRenderLayer.None;

        public List<string> keyList = new List<string>();

        public MultiTexBatch(TextureRenderLayer la, List<string> list)
        {
            layer = la;
            keyList = list;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<TextureRenderLayer>(ref layer, "layer", TextureRenderLayer.None, true);
            Scribe_Collections.Look<string>(ref keyList, "keyList", LookMode.Value, Array.Empty<object>());
        }
    }
}
