using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NazunaLib
{
    public class MultiTexEpoch : IExposable
    {
        public List<MultiTexBatch> batches = new List<MultiTexBatch>();
        public void ExposeData()
        {
            Scribe_Collections.Look<MultiTexBatch>(ref batches, "batches", LookMode.Deep, Array.Empty<object>());
        }
    }
}
