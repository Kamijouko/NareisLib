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
using AlienRace;
using AlienRace.ExtendedGraphics;

namespace NazunaLib
{
    public class TextureLevels : GraphicData
    {
        //贴图名称前缀（不带文件夹路径）此定义出的贴图名为唯一标识符，请不要重复使用相同的贴图名（前缀可以重复）
        public List<string> prefix = new List<string>();

        //渲染的层级信息
        public TextureRenderLayer renderLayer = TextureRenderLayer.None;



        //可选参数，设置特殊hediff所对应的名称前缀（根据hediff严重度）
        public List<TextureLevelHediffSet> hediffSets = new List<TextureLevelHediffSet>();

        //可选参数，设置特殊job所对应的名称前缀
        public TextureLevelJobSet jobSets;

        //可选参数，设置此层的贴图具有的pattern以及随机时间的间隔
        public TextureLevelRandomPatternSet patternSets;

        //可选参数，渲染时使用的Mesh类型，有Hair，Head和Body三种（注意大小写）非Humanlike的pawn只有Body
        public string meshType = "Body";

        //可选参数，渲染时使用的Mesh大小，为zero时不会识别，默认为zero
        public Vector2 meshSize = Vector2.zero;

        //可选参数，区分性别，在文件名的“_方向”之前添加“_性别”，目前只支持"_Male"和"_Female"
        public bool hasGender = false;
        public bool renderMale = false;
        public bool renderFemale = false;

        //可选参数，链接到身体部位，与下面的label二选一
        public BodyPartDef bodyPart = null;

        //可选参数，链接到身体部位
        public string bodyPartLabel = null;

        //可选参数，当渲染的部位不是AlienRace的BodyAddon时使用，控制是否在倒地时渲染
        public bool renderOnGround = true;

        //可选参数，当渲染的部位不是AlienRace的BodyAddon时使用，控制是否在床上时渲染
        public bool renderInBed = true;

        //可选参数，该贴图是否有正在腐烂时的版本
        public bool hasRotting = false;

        //可选参数，该贴图是否有骨架版本
        public bool hasDessicated = false;

        //可选参数，如果该贴图为手部层，则这个选项控制Hand层和HandTwo层是否绘制在shell层下方
        public bool handDrawBehindShell = true;

        //可选参数，x、y、z分别表示正面侧面和背面，为1时会被渲染，为0时会被忽略
        public Vector3 renderSwitch = Vector3.one;

        //可选参数，设定某种前缀prefix的生成权重
        public Dictionary<string, int> weightOfThePrefix = new Dictionary<string, int>();

        //可选参数，针对某张贴图的特定名称设定权重（不带文件格式后缀）
        public Dictionary<string, int> weightOfTheName = new Dictionary<string, int>();

        //可选参数，当前层是否为袖子
        public bool isSleeve = false;
        //可选参数，和上方参数联动，当前袖子对应哪个手部的贴图，填入贴图名称（不带前后缀）
        public List<string> sleeveTexList = new List<string>();
        //可选参数，设置袖子是否在shell层下方
        public bool sleeveDrawBehindShell = true;

        //默认的权重（在未设定权重的情况下所有贴图的权重均为1）常数，无法更改
        public const int normalWaight = 1;



        //xml里无需设定并且设定无效
        public Dictionary<string, int> preFixWeights = new Dictionary<string, int>();

        //xml里无需设定并且设定无效
        public Dictionary<string, int> texWeights = new Dictionary<string, int>();

        //xml里无需设定并且设定无效
        public Dictionary<string, string[]> preFixToTexName = new Dictionary<string, string[]>();

        //xml里无需设定并且设定无效
        public string keyName = "";

        //xml里无需设定并且设定无效
        public string originalDef = "";

        //xml里无需设定并且设定无效
        public string hediffPrefix = "";

        //xml里无需设定并且设定无效
        public string jobPrefix = "";

        //xml里无需设定并且设定无效
        public string genderSuffix = "";

        //xml里无需设定并且设定无效
        public string folder = "";

        //xml里无需设定并且设定无效
        public Graphic cacheGraphic;



        //对当前Level应该渲染的贴图路径进行处理，并且将每个路径都对应一个GraphicData
        public void GetAllGraphicDatas(string folderPath)
        {
            if (graphicClass == null)
                graphicClass = typeof(Graphic_Multi);
            if (drawOffsetSouth == null)
                drawOffsetSouth = Vector3.zero;
            if (drawOffsetEast == null)
                drawOffsetEast = Vector3.zero;
            if (drawOffsetNorth == null)
                drawOffsetNorth = Vector3.zero;
            if (drawOffsetWest == null)
                drawOffsetWest = Vector3.zero;
            folder = folderPath;

            //处理当prefix列表为空的时候，就使用原GraphicData的texPath路径
            if (prefix.NullOrEmpty())
            {
                if (texPath == null)
                    return;
                string tmpKeyName = "singleLevel" + ThisModData.TmpLevelID;
                if (!ThisModData.TexLevelsDatabase.Keys.Contains(tmpKeyName))
                {
                    prefix.Add(tmpKeyName);
                    //GraphicData data = this;
                    preFixToTexName[tmpKeyName] = new string[] { tmpKeyName };
                    ResolvePrefixWeights(tmpKeyName);
                    ResolveSingleTexWeights(tmpKeyName);
                    ThisModData.TexLevelsDatabase.Add(tmpKeyName, this);
                    ThisModData.TmpLevelID++;
                }
                return;
            }

            //组合指定的文件夹路径
            string folderAbsDir = Path.Combine(ModStaticMethod.RootDir, "Textures", folderPath);

            string[] names = new string[] { };

            //处理prefix对应的图层的全名并且赋予它prefix预设权重列表所对应的权重
            foreach (string pre in prefix)
            {
                string[] fullName = Directory.GetFiles(folderAbsDir, pre + "*");
                if (fullName.Length > 0)
                {
                    string[] resolvedName = fullName.Select(x => ResolveKeyNameGenderAndDrict(Path.GetFileNameWithoutExtension(x))).Distinct().ToArray();
                    preFixToTexName[pre] = resolvedName;
                    names = names.Concat(resolvedName).ToArray();
                    ResolvePrefixWeights(pre);
                }
            }

            //处理路径全名为不带后缀的名称，并且如果有针对特定名称列表的话就给特定名称赋予权重
            if (names.Length > 0)
            {
                foreach (string name in names)
                {
                    //Log.Warning(name);
                    if (!ThisModData.TexLevelsDatabase.Keys.Contains(name))
                    {
                        //GraphicData data = this;
                        texPath = Path.Combine(folderPath, name);
                        ResolveSingleTexWeights(name);
                        ThisModData.TexLevelsDatabase.Add(name, this);
                    }
                }
            }
        } 

        //赋予特定名称权重
        public void ResolveSingleTexWeights(string keyName)
        {
            if (!weightOfTheName.NullOrEmpty())
            {
                if (weightOfTheName.ContainsKey(keyName))
                    texWeights[keyName] = weightOfTheName[keyName];
                else
                    texWeights[keyName] = normalWaight;
            }
            else
                texWeights[keyName] = normalWaight;
        }

        //赋予prefix权重
        public void ResolvePrefixWeights(string pre)
        {
            if (!weightOfThePrefix.NullOrEmpty())
            {
                if (weightOfThePrefix.ContainsKey(pre))
                    preFixWeights[pre] = weightOfThePrefix[pre];
                else
                    preFixWeights[pre] = normalWaight;
            }
            else
                preFixWeights[pre] = normalWaight;
        }

        //去掉贴图名称中的额外信息
        public static string ResolveKeyNameGenderAndDrict(string key)
        {
            key = key.Replace("_south", "");
            key = key.Replace("_east", "");
            key = key.Replace("_north", "");
            key = key.Replace("_west", "");
            key = key.Replace("_southm", "");
            key = key.Replace("_eastm", "");
            key = key.Replace("_northm", "");
            key = key.Replace("_westm", "");
            key = key.Replace("_Male", "");
            key = key.Replace("_Female", "");
            key = key.Replace("_Rotting", "");
            key = key.Replace("_Dessicated", "");
            return key;
        }

        

        //检查关联的bodyPart是否还存在
        public bool RequiredBodyPartExistsFor(ExtendedGraphicsPawnWrapper pawn)
        {
            if (pawn.HasNamedBodyPart(this.bodyPart, this.bodyPartLabel))
            {
                return true;
            }
            return hediffSets.Any(x => x.hediff == HediffDefOf.MissingBodyPart);
        }

        //检查是否在地上或床上渲染
        public bool VisibleForPostureOf(ExtendedGraphicsPawnWrapper pawn)
        {
            return (pawn.GetPosture() == PawnPosture.Standing || this.renderOnGround) && (pawn.VisibleInBed() || this.renderInBed);
        }

        //检查当前Pawn的job是否符合并对jobPrefix赋值
        public bool ResolvePrefixForJob(ExtendedGraphicsPawnWrapper pawn, string keyName)
        {
            if (jobSets == null)
                return true;
            if (pawn.CurJob != null)
            {
                TextureLevelJobDataSet data;
                return !jobSets.JobMap.TryGetValue(pawn.CurJob.def, out data) || (data.texList.Contains(keyName) && data.IsApplicable(pawn, out jobPrefix));
            }
            return jobSets.rendNoJob;
        }

        //检查当前Pawn当前部位的hediff是否符合并对hediffPrefix赋值
        public bool ResolvePrefixForHediff(ExtendedGraphicsPawnWrapper pawn, string keyName)
        {
            if (hediffSets == null || (bodyPart == null && bodyPartLabel == ""))
                return true;
            int priority = 0;
            foreach (TextureLevelHediffSet set in hediffSets)
            {
                if (!set.texList.Contains(keyName))
                    continue;
                if (set.priority >= priority)
                {
                    hediffPrefix = set.GetCurHediffPrefix(pawn, bodyPart, bodyPartLabel);
                    priority = set.priority;
                }
            }
            return true;
        }

        //检查当前Pawn的性别并对genderSuffix赋值
        public bool ResolveSuffixForGenderOf(ExtendedGraphicsPawnWrapper pawn)
        {
            if (!hasGender)
                return true;
            if (pawn.GetGender() != Gender.Female)
            {
                genderSuffix = "_Male";
                return renderMale;
            }
            genderSuffix = "_Female";
            return renderFemale;
        }


        //最终判定以及处理该graphicData
        public bool CanRender(Pawn pawn, string keyName)
        {
            ExtendedGraphicsPawnWrapper obj = new ExtendedGraphicsPawnWrapper(pawn);
            return RequiredBodyPartExistsFor(obj) && VisibleForPostureOf(obj) && ResolvePrefixForJob(obj, keyName) && ResolvePrefixForHediff(obj, keyName) &&　ResolveSuffixForGenderOf(obj);
        }

        //初始化，与基类的Init方法相同但显式
        public void Initialization()
        {
            if (graphicClass == null)
            {
                cacheGraphic = null;
                return;
            }
            ShaderTypeDef cutout = this.shaderType;
            if (cutout == null)
            {
                cutout = ShaderTypeDefOf.Cutout;
            }
            Shader shader = cutout.Shader;
            cacheGraphic = GraphicDatabase.Get(this.graphicClass, this.texPath, shader, this.drawSize, this.color, this.colorTwo, this, this.shaderParameters, this.maskPath);
            if (onGroundRandomRotateAngle > 0.01f)
            {
                cacheGraphic = new Graphic_RandomRotated(cacheGraphic, this.onGroundRandomRotateAngle);
            }
            if (this.Linked)
            {
                cacheGraphic = GraphicUtility.WrapLinked(cacheGraphic, this.linkType);
            }
        }

        //取得完整的贴图名称，前缀顺序为pattern，job，hediff
        public string GetFullKeyName(string keyName, int pattern = 0, string condition = "")
        {
            string patternPrefix = "";
            if (pattern > 0)
                patternPrefix = "Pattern" + pattern.ToString() + "_";
            if (condition != "")
                condition = "_" + condition;
            return patternPrefix + jobPrefix + hediffPrefix + keyName + genderSuffix + condition;
        }

        //取得graphic，修改了基类的属性Graphic，参数为完全处理完毕后多层渲染comp里记录的keyName（列表在MultiTexBatch里）
        public Graphic GetGraphic(string keyName, int pattern = 0, string condition = "")
        {
            string path = Path.Combine(folder, GetFullKeyName(keyName, pattern, condition));
            if (texPath != path || cacheGraphic == null)
            {
                texPath = path;
                Initialization();
            }
            return cacheGraphic;
        }
    }
}
