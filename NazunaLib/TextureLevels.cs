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
        //贴图名称列表（可加入文件夹路径）
        public List<string> suffix = new List<string>();

        //渲染的层级信息
        public TextureRenderMode renderMode = TextureRenderMode.None;

        public void GetAllGraphicDatas(string folderPath)
        {
            string folderAbsDir = Path.Combine(ModStaticMethod.RootDir, folderPath);

            Dictionary<string, GraphicData> result = new Dictionary<string, GraphicData>();
            string[] names = new string[] { };

            foreach (string suf in suffix)
            {
                string[] sufname = new string[] { };
                sufname = Directory.GetFiles(folderAbsDir, suf + "*");
                if (sufname.Length > 0)
                {
                    names.Concat(sufname);
                }
            }
            if (names.Length > 0)
            {
                foreach (string name in names)
                {
                    string keyName = Path.GetFileNameWithoutExtension(name);
                    if (!ModStaticMethod.PawnTexLevelsDatabase.Keys.Contains(keyName))
                    {
                        GraphicData data = this;
                        data.texPath = Path.Combine(folderPath, keyName);
                        ModStaticMethod.PawnTexLevelsDatabase.Add(keyName, data);
                    }
                }
            }
        } 
    }
}
