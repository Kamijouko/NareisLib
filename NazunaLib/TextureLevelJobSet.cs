using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NazunaLib
{
    public class TextureLevelJobSet
    {
        //是否在无job时渲染
        public bool rendNoJob = true;

        private Dictionary<JobDef, TextureLevelJobDataSet> jobMap = new Dictionary<JobDef, TextureLevelJobDataSet>();
        public Dictionary<JobDef, TextureLevelJobDataSet> JobMap
        {
            get
            {
                Dictionary<JobDef, TextureLevelJobDataSet> result;
                if ((result = jobMap) == null)
                {
                    result = (jobMap = jobs.ToDictionary((TextureLevelJobDataSet set) => set.job));
                }
                return result;
            }
        }

        public List<TextureLevelJobDataSet> jobs = new List<TextureLevelJobDataSet>();
    }
}
