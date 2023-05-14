using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NazunaLib
{
    public enum TextureRenderLayer : byte
    {
        None,

        //BehindTheBottomHair,
        //在描绘侧面时为Hair层
        //在描绘背面时为Hair层
        //使用为PawnRenderer类的DrawPawnBody方法打的Prefix补丁进行渲染
        BottomHair,

        //BehindTheShell,
        //在描绘侧面时不变
        //在描绘背面时为FaceMask层
        //使用为PawnRenderer类的DrawPawnBody方法打的Prefix补丁进行渲染
        BottomShell,
        //FrontOfShell,

        //BehindTheBody,
        //各方向都不变
        //使用为PawnRenderer类的DrawPawnBody方法打的Prefix补丁进行渲染，并且对PawnGraphicSet类的MatsBodyBaseAt方法进行Prefix补丁
        Body,
        //FrontOfBody,

        //在描绘侧面时为HandTwo层
        //在描绘背面时不变
        //使用为PawnRenderer类的DrawPawnBody方法打的Finalizer补丁进行渲染
        HandOne,

        //BehindTheApparel,
        //各方向都不变
        //使用为PawnRenderer类的DrawBodyApparel方法打的Prefix补丁进行渲染
        Apparel,
        //FrontOfApparel,

        //各方向不变
        //使用IL语言在PawnRenderer类的DrawPawnInternal中渲染头部前渲染
        Hand,

        //在描绘正面时为HandOne层
        //此层作为给HandOne层变换所用，不应该直接设定为该层
        //使用IL语言在PawnRenderer类的DrawPawnInternal中渲染头部前渲染
        HandTwo,

        //BehindTheHead,
        //各方向都不变
        //使用IL语言在PawnRenderer类的DrawPawnInternal中修改头部进行渲染
        Head,
        //FrontOfHead,

        //BehindTheFaceMask,
        //在描绘侧面时不变
        //在描绘背面时为Shell层
        //使用IL语言在PawnRenderer类的DrawHeadHair中渲染头发前进行渲染
        FaceMask,
        //FrontOfFaceMask,

        //BehindTheHair,
        //在描绘侧面时不变
        //在描绘背面时为BottmHair层
        //使用IL语言在PawnRenderer类的DrawHeadHair中修改头发进行渲染
        Hair,
        //FrontOfHair,

        //各方向不变
        //使用IL语言在PawnRenderer类的DrawHeadHair中渲染头发后渲染装备前进行渲染，对象为身体
        FrontShell,

        //各方向不变
        //使用IL语言在PawnRenderer类的DrawHeadHair中渲染头发后渲染装备前进行渲染，对象为头部
        HeadMask,

        //BehindTheHat,
        //各方向不变
        //使用IL语言在PawnRenderer类的DrawHeadHair中修改装备进行渲染
        Hat,
        //FrontOfHat

        //各方向不变
        //使用为PawnRenderer类的DrawPawnInternal方法打的Postfix补丁进行渲染
        Overlay
    }
}
