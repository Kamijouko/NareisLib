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
        BottomHair,

        //BehindTheShell,
        Shell,
        //FrontOfShell,

        //BehindTheBody,
        Body,
        //FrontOfBody,

        //BehindTheApparel,
        Apparel,
        //FrontOfApparel,

        //BehindTheHead,
        Head,
        //FrontOfHead,

        //BehindTheFaceMask,
        FaceMask,
        //FrontOfFaceMask,

        //BehindTheHair,
        Hair,
        //FrontOfHair,

        HeadMask,

        //BehindTheHat,
        Hat
        //FrontOfHat
    }
}
