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

        //是否隐藏指定层级的贴图
        public bool renderOriginTex = false;

        //如果指定的隐藏的节点下有子节点的话是否保留，为false的话将也隐藏这些子节点
        //当有重复指定隐藏同一个节点的情况时，只要其中有一个指定需要隐藏子节点，则优先隐藏全部子节点
        public bool keepSubNode = false;

        //可选参数，可在内部设置通过定位到渲染树里设置了对应tagDef并且其自身或其子Node具有对应的debugLabel来指定隐藏某个具体的父Node或子Node
        public OverrideClass overrideOriginSet = new OverrideClass();



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
