using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NareisLib
{
    public enum TextureRenderLayer : byte
    {
        None,

        //各方向都不变
        //使用为PawnRenderer类的DrawPawnBody方法打的Prefix补丁进行渲染
        BottomOverlay,

        //在描绘侧面时不变
        //在描绘背面时为FrontShell层
        //使用为PawnRenderer类的DrawPawnBody方法打的Prefix补丁进行渲染
        BottomShell,

        //在描绘侧面时为Hair层
        //在描绘背面时为Hair层
        //使用为PawnRenderer类的DrawPawnBody方法打的Prefix补丁进行渲染
        BottomHair,

        //各方向都不变，如果不关闭原版身体的渲染的话会渲染在原版的身体之下
        //使用为PawnRenderer类的DrawPawnBody方法打的Prefix补丁进行渲染，并且对PawnGraphicSet类的MatsBodyBaseAt方法进行Prefix补丁
        Body,


        //在描绘正面侧面时为HandOne层
        //此层作为给HandOne层变换所用，不应该直接设定为该层
        //该手部层侧面会在衣服层上绘制，默认在shell下绘制，可在TextureLevels里设置绘制在shell层上
        //使用为PawnRenderer类的DrawPawnBody方法打的Finalizer补丁进行渲染
        //HandTwo,


        //各方向都不变
        //对于最后一层是shell层以外的衣服使用为PawnRenderer类的DrawPawnBody方法打的Finalizer补丁进行渲染，shell层使用DrawBodyApparel方法打的Prefix方法进行渲染
        //用于服装的渲染，其他部位设置此层不会生效
        Apparel,

        //在描绘背面时为HandTwo层
        //该手部层正面背面均会在衣服层下绘制
        //使用为PawnRenderer类的DrawPawnBody方法打的Finalizer补丁进行渲染
        //HandOne,

        //各方向不变
        //该手部层正面背面均会在衣服层上绘制，默认在shell下绘制，可在TextureLevels里设置绘制在shell层上
        //使用为PawnRenderer类的DrawPawnBody方法打的Finalizer补丁进行渲染
        Hand,

        

        //各方向都不变
        //使用IL语言在PawnRenderer类的DrawPawnInternal中修改头部进行渲染
        Head,

        //各方向都不变
        //使用IL语言在PawnRenderer类的DrawHeadHair中渲染头发前进行渲染
        //用于服装的渲染，其他部位设置此层不会生效
        FaceMask,

        //在描绘侧面时不变
        //在描绘背面时为BottmHair层
        //使用IL语言在PawnRenderer类的DrawHeadHair中修改头发进行渲染
        Hair,

        

        //各方向不变
        //使用IL语言在PawnRenderer类的DrawHeadHair中渲染头发后渲染装备前进行渲染
        //用于服装的渲染，其他部位设置此层不会生效
        HeadMask,

        //在描绘侧面时不变
        //在描绘背面时为BottmShell层
        //使用IL语言在PawnRenderer类的DrawHeadHair中渲染头发后渲染装备前进行渲染
        //用于服装的渲染，其他部位设置此层不会生效
        FrontShell,

        //各方向不变
        //使用IL语言在PawnRenderer类的DrawHeadHair中修改装备进行渲染
        //用于服装的渲染，其他部位设置此层不会生效
        Hat,

        //各方向不变
        //使用为PawnRenderer类的DrawPawnInternal方法打的Postfix补丁进行渲染
        Overlay
    }
}
