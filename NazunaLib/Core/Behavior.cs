using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace NareisLib
{
    public class Behavior
    {
        //只有此列表中所指定的贴图才能触发此行为，
        //例如当TextureLevels里指定了随机的贴图时，只有这个列表里也包含了随机到的贴图时才会触发此行为
        //此列表中填入的应是不带任何前后缀的贴图名，
        //这里的前后缀指的是例如hediff前缀、方向后缀、性别后缀等等，当然.png这样的格式后缀也不需要
        public List<string> textures = new List<string>();

        //此行为所使用的贴图具有的外部路径
        public string exPath;

        //这里指定的是该行为所使用的内部路径库，
        //这里的第一个元素即为默认值，Posture一般为Standing
        //key为job时的姿势，value为对应的贴图额外路径列表
        //注意此路径所指定的文件夹必须包含在TextureLevels所指定的文件夹内
        public Dictionary<PawnPosture, List<string>> pathDict;

        //以下两项控制是否在移动或非移动中也继续行为
        public bool rendMoving = true;
        public bool rendUnMoving = true;

        //此设置决定切换到此Behavior时是否使用列表中随机的元素，
        //如果为false则使用列表中第一个项。
        public bool randomFirst = true;

        //是否在pathDict的当前value中随机选择贴图变换
        public bool randomChange = false;
        //randomChange开启后此设置才生效，
        //表示固定的贴图变换时间间隔（秒）
        public float actionStaticChangeSeconds = 10f;
        //randomChange开启后此设置才生效，
        //表示是否使用随机的时间间隔，否则使用上一项固定的时间间隔
        public bool useRandomTimeDelta = false;
        //randomChange开启后此设置才生效，
        //表示随机的贴图变换时间间隔范围
        public Vector2 actionRandomChangeSeconds = new Vector2(10f, 20f);
        //randomChange开启后此设置才生效，
        //此设置决定是否是根据列表中的顺序来变换
        public bool loopChange = false;
       

        //是否与另一个Behavior的状态相关联
        //此设置开启后上方除exPath和textures以外所有选项将不再具有作用，当前Behavior将使用下面指定的textureLevels的ActionManager里产生的结果
        public bool linkedWithAction = false;
        //linkedWithAction开启后此设置才生效，
        //与其关联的MultiTexDef表示方法，例如手臂应设置为"NareisLib.HandTypeDef_Comp里设置的手臂defName"
        //其中"_"前的部分表示MultiTexDef里设置的原Def的类型，比如"ThingDef"
        //"_"后的部分表示MultiTexDef里设置的原Def的defName
        public string type_originalDefName;
        //linkedWithAction开启后此设置才生效，
        //表示其关联的那层TextureLevels的textureLevelsName，与MultiTexDef的levels列表里设置的相对应
        public string textureLevelsName;
        //linkedWithAction开启后此设置才生效，
        //设置此部件检测被link的部件状态的时间间隔（秒）
        public float linkedActionSyncDelta = 0.2f;


        public int GetActionRandomChangeTick()
        {
            return Rand.Range(actionRandomChangeSeconds.x, actionRandomChangeSeconds.y).SecondsToTicks();
        }

    }
}
