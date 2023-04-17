using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using HugsLib;

namespace NazunaLib
{
    public class TextureLevels : GraphicData
    {
        //贴图名称前缀（不带文件夹路径）
        public List<string> prefix = new List<string>();

        //渲染的层级信息
        public TextureRenderLayer renderLayer = TextureRenderLayer.None;

        //可选参数，设定某种前缀的生成权重
        public Dictionary<string, int> weightOfThePrefix = new Dictionary<string, int>();

        //可选参数，针对某张贴图设定权重（不带文件格式后缀）
        public Dictionary<string, int> weightOfTheName = new Dictionary<string, int>();

        //默认的权重（在未设定权重的情况下所有贴图的权重均为1）
        public const int normalWaight = 1;



        //xml里无需设定并且设定无效
        public Dictionary<string, int> preFixWeights = new Dictionary<string, int>();

        //xml里无需设定并且设定无效
        public Dictionary<string, int> texWeights = new Dictionary<string, int>();

        //xml里无需设定并且设定无效
        public Dictionary<string, string[]> preFixToTexFullname = new Dictionary<string, string[]>();




        public void GetAllGraphicDatas(string folderPath)
        {
            if (prefix.NullOrEmpty())
            {
                if (texPath == null)
                    return;
                string tmpKeyName = "singleLevel" + ThisModData.TmpLevelID;
                if (!ThisModData.TexLevelsDatabase.Keys.Contains(tmpKeyName))
                {
                    prefix.Add(tmpKeyName);
                    GraphicData data = this;
                    ResolveSingleTexWeights(tmpKeyName);
                    ThisModData.TexLevelsDatabase.Add(tmpKeyName, data);
                    ThisModData.TmpLevelID++;
                }
            }

            string folderAbsDir = Path.Combine(ModStaticMethod.RootDir, folderPath);

            string[] names = new string[] { };

            foreach (string pre in prefix)
            {
                string[] sufname = Directory.GetFiles(folderAbsDir, pre + "*");
                if (sufname.Length > 0)
                {
                    preFixToTexFullname.Add(pre, sufname);
                    names.Concat(sufname);
                    ResolvePrefixWeights(pre);
                }
            }
            if (names.Length > 0)
            {
                foreach (string name in names)
                {
                    string keyName = Path.GetFileNameWithoutExtension(name);
                    if (!ThisModData.TexLevelsDatabase.Keys.Contains(keyName))
                    {
                        GraphicData data = this;
                        data.texPath = Path.Combine(folderPath, keyName);
                        ResolveSingleTexWeights(keyName);
                        ThisModData.TexLevelsDatabase.Add(keyName, data);
                    }
                }
            }
        } 

        public void ResolveSingleTexWeights(string keyName)
        {
            if (!weightOfTheName.NullOrEmpty())
            {
                if (weightOfTheName.ContainsKey(keyName))
                    texWeights.Add(keyName, weightOfTheName[keyName]);
                else
                    texWeights.Add(keyName, normalWaight);
            }
            else
                texWeights.Add(keyName, normalWaight);
        }

        public void ResolvePrefixWeights(string pre)
        {
            if (!weightOfThePrefix.NullOrEmpty())
            {
                if (weightOfThePrefix.ContainsKey(pre))
                    preFixWeights.Add(pre, weightOfThePrefix[pre]);
                else
                    preFixWeights.Add(pre, normalWaight);
            }
            else
                preFixWeights.Add(pre, normalWaight);
        }
    }
}
