using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NareisLib
{
    public class OverrideClass
    {
        //可选参数，通过定位到渲染树里设置了对应tagDef并且其自身或其子Node具有对应的debugLabel来指定隐藏某个具体的父Node或子Node
        public PawnRenderNodeTagDef parentRenderNodeTagDef = null;
        public string parentRenderDebugLabel = "";

        public bool IsNull { get { return parentRenderNodeTagDef == null && parentRenderDebugLabel == ""; } }

        public OverrideClass(PawnRenderNodeTagDef tag = null, string debugLabel = "") 
        {
            parentRenderNodeTagDef = tag;
            parentRenderDebugLabel = debugLabel;
        }

        // 重写 Equals 方法
        public override bool Equals(object obj)
        {
            if (obj is OverrideClass otherObj)
            {
                return this.parentRenderNodeTagDef == otherObj.parentRenderNodeTagDef && this.parentRenderDebugLabel == otherObj.parentRenderDebugLabel;
            }
            return false;
        }

        // 重写 GetHashCode 方法
        public override int GetHashCode()
        {
            return (parentRenderNodeTagDef, parentRenderDebugLabel).GetHashCode();
        }
    }
}
