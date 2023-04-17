using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace NazunaLib
{
    public class MultiTexDef : Def
    {
        //渲染的部位类型
        public RenderClass renderClass = RenderClass.None;
        
        //基于渲染的Def
        public Def originalDef;

        //是否渲染原层级的贴图
        public bool rendOriginTex = false;

        //贴图文件夹所在路径
        public string path = "";

        //指定图层以及贴图名
        public List<TextureLevels> levels = new List<TextureLevels>();



        //xml里无需设定并且设定无效
        public MultiTexEpoch cacheOfLevels = new MultiTexEpoch();

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (levels.NullOrEmpty())
                return;
            foreach (TextureLevels level in levels)
            {
                cacheOfLevels.batches.Add(new MultiTexBatch(level.renderLayer, new List<string>()));
            }
        }
    }
}
