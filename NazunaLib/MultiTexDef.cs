using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace NareisLib
{
    public class MultiTexDef : Def
    {
        //渲染的部位的Def类型(例如HeadTypeDef或者BodyTypeDef)，手填写HandTypeDef
        public Type originalDefClass;
        
        //基于渲染的Def
        public string originalDef;

        //是否渲染原层级的贴图
        public bool renderOriginTex = false;

        //当renderOriginTex为false时，将隐藏渲染树里对应这里设置的tagDef，如果游戏中这里设置的tagDef已经被隐藏则自动不做处理
        public PawnRenderNodeTagDef renderBaseNode;

        //贴图文件夹所在路径
        public string path = "";

        //指定图层以及贴图名
        public List<TextureLevels> levels = new List<TextureLevels>();



        //xml里无需设定并且设定无效
        //public MultiTexEpoch cacheOfLevels = new MultiTexEpoch();

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            //cacheOfLevels.typeOriginalDefName = originalDefClass.ToStringSafe() + "_" + originalDef;
        }
    }
}
