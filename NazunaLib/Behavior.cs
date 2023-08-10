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
        //此行为所使用的贴图是否具有额外路径，
        //注意此路径所指定的文件夹必须包含在TextureLevels所指定的文件夹内
        // string exPath;

        //只有此列表中所指定的贴图才能触发此行为，
        //例如当TextureLevels里指定了随机的贴图时，只有这个列表里也包含了随机到的贴图时才会触发此行为
        //此列表中填入的应是不带任何前后缀的贴图名，
        //这里的前后缀指的是例如hediff前缀、方向后缀、性别后缀等等，当然.png这样的格式后缀也不需要
        public List<string> textures = new List<string>();

        //这里指定的是该行为所使用的额外路径库，
        //这里的第一个元素即为默认值，Posture一般为Standing
        //key为job时的姿势，value为对应的贴图额外路径列表
        //注意此路径所指定的文件夹必须包含在TextureLevels所指定的文件夹内
        public Dictionary<PawnPosture, List<string>> pathDict;

        //以下两项控制是否在移动或非移动中也继续行为
        public bool rendMoving = true;
        public bool rendUnMoving = true;

        //是否根据时间随机在prefixList中选择前缀来进行随机贴图变换
        public bool randomChange = false;
        //与上一项联动，上一项开启后此设置才生效，
        //表示是否使用随机的时间间隔，否则使用下下一项固定的时间间隔
        public bool useRandomTimeDelta = false;
        //与上一项联动，上一项开启后此设置才生效，
        //表示随机的贴图变换时间间隔范围
        public Vector2 actionRandomChangeSeconds = new Vector2(10f, 20f);
        //与上上一项联动，上上一项关闭时此设置才生效，
        //表示固定的贴图变换时间间隔
        public float actionStaticChangeSeconds = 10f;


        public int GetActionRandomChangeTick()
        {
            return Rand.Range(actionRandomChangeSeconds.x, actionRandomChangeSeconds.y).SecondsToTicks();
        }

    }
}
