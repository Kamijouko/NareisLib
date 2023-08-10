using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class Page_Setting_CurBehavior : Page
    {
        public Page_Setting_CurBehavior()
        {
            layer = WindowLayer.Dialog;
            forcePause = false;
            resizeable = false;
            doCloseX = true;
            onlyOneOfTypeAllowed = true;
            preventCameraMotion = false;
            closeOnAccept = true;
            draggable = true;
            windowRect = new Rect((UI.screenWidth - 250f) / 2, (UI.screenHeight - 250f) / 2, 250f, 250f);
        }

        public override void DoWindowContents(Rect inRect)
        {
            return;
        }

        
    }
}
