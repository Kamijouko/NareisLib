using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace NareisLib
{
    public class TextureLevelJobSet
    {
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

        //列出所有需要对应的Job及其贴图和前缀信息
        public List<TextureLevelJobDataSet> jobs = new List<TextureLevelJobDataSet>();

        public TextureLevelJobSet Clone()
        {
            TextureLevelJobSet result = new TextureLevelJobSet();
            if (!jobs.NullOrEmpty())
                result.jobs = new List<TextureLevelJobDataSet>(jobs);
            return result;
        }
    }
}
