using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace NareisLib
{
    public static class ThisModData
    {
        //key为race的defName，
        //value为设置了这个race的RenderPlanDef列表,
        //此Database并不包含所有的race，只有曾在RenderPlanDef里设置过的race才会包含在内
        public static Dictionary<string, List<RenderPlanDef>> RacePlansDatabase = new Dictionary<string, List<RenderPlanDef>>();

        //第一个key为值的TextureLevels所属的Type_originalDefName，
        //第二个key为每个TextureLevels设置的textureLevelsName所对应的图层唯一标识符（不可重名），
        //value为其对应的GraphicData，这里存储了整个mod里所有TextureLevels所能生成的GraphicData方便直接根据图像名称调用
        public static Dictionary<string, Dictionary<string, TextureLevels>> TexLevelsDatabase = new Dictionary<string, Dictionary<string, TextureLevels>>();

        //第一个key为此plan的plandef，
        //第二个key为MultiTexDef里指定的基于渲染的部件的Type_originalDefName，
        //value为其MultiTexDef自身，这里存储了整个mod里的所有MultiTexDef方便查询
        public static Dictionary<string, Dictionary<string, MultiTexDef>> DefAndKeyDatabase = new Dictionary<string, Dictionary<string, MultiTexDef>>();

        //此为当TextureLevels所指定的prefix列表中没有图像或者没有指定的时候，根据原有参数texPath生成的图像所对应的自动生成的名称带的编号
        public static int TmpLevelID = 0;
    }
}
