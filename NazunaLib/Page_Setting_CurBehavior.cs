using JetBrains.Annotations;
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
        MultiRenderComp comp;
        bool hasCachedPos = true;

        public override string PageTitle
        {
            get
            {
                return "SettingActionManagerWindow_Title".Translate();
            }
        }

        //UI初始化
        public Page_Setting_CurBehavior(MultiRenderComp comp)
        {
            layer = WindowLayer.Dialog;
            forcePause = false;
            resizeable = false;
            doCloseX = true;
            onlyOneOfTypeAllowed = true;
            preventCameraMotion = false;
            closeOnAccept = true;
            draggable = true;
            this.comp = comp;
        }


        //绘制UI主体
        public override void DoWindowContents(Rect inRect)
        {
            if (hasCachedPos)
            {
                windowRect.position = ThisModData.CachedActionSettingWindowPos;
                hasCachedPos = false;
            }
            windowRect.width = 700f;
            windowRect.height = 800f;

            DrawPageTitle(inRect);
            Rect mainRect = GetMainRect(inRect, 0f, false);




            return;
        }

        //关闭UI前事件
        public override void PreClose()
        {
            base.PreClose();
            ThisModData.CachedActionSettingWindowPos = windowRect.position;
            hasCachedPos = true;
        }
    }
}
