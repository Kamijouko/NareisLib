using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class ITab_ContentsNewOutfitStand : ITab_ContentsBase
    {
        public override IList<Thing> container
        {
            get
            {
                Building_NewOutfitStand stand = OutfitStand;
                return stand?.HeldItems != null ? stand.HeldItems.ToList() : new List<Thing>();
            }
        }

        public override bool IsVisible
        {
            get
            {
                return base.SelThing != null && OutfitStand != null && base.IsVisible;
            }
        }

        public Building_NewOutfitStand OutfitStand
        {
            get
            {
                return base.SelThing as Building_NewOutfitStand;
            }
        }

        public override bool VisibleInBlueprintMode => false;

        public ITab_ContentsNewOutfitStand()
        {
            labelKey = "TabCasketContents";
            containedItemsKey = "TabCasketContents";
        }

        protected override void DoItemsLists(Rect inRect, ref float curY)
        {
            ListContainedApparel(inRect, container, ref curY);
        }

        private void ListContainedApparel(Rect inRect, IList<Thing> apparel, ref float curY)
        {
            GUI.BeginGroup(inRect);
            Widgets.ListSeparator(ref curY, inRect.width, containedItemsKey.Translate());
            bool any = false;
            for (int i = 0; i < apparel.Count; i++)
            {
                Thing thing = apparel[i];
                if (thing != null)
                {
                    any = true;
                    DoRow(thing, inRect.width, i, ref curY);
                }
            }
            if (!any)
            {
                Widgets.NoneLabel(ref curY, inRect.width, null);
            }
            GUI.EndGroup();
        }

        private void DoRow(Thing thing, float width, int i, ref float curY)
        {
            Building_NewOutfitStand stand = OutfitStand;
            if (stand == null)
            {
                return;
            }
            Rect rect = new Rect(0f, curY, width, 28f);
            Widgets.InfoCardButton(0f, curY, thing);
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlightSelected(rect);
            }
            else if (i % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect);
            }
            Rect rect2 = new Rect(rect.width - 24f, curY, 24f, 24f);
            if (Widgets.ButtonImage(rect2, DropTex.Texture, true, null))
            {
                IntVec3 position;
                if (!stand.OccupiedRect().AdjacentCells.Where((IntVec3 x) => x.Walkable(stand.Map)).TryRandomElement(out position))
                {
                    position = stand.Position;
                }
                Thing thing2;
                stand.TryDrop(thing, position, ThingPlaceMode.Near, 1, out thing2);
                CompForbiddable compForbiddable;
                if (thing2.TryGetComp(out compForbiddable))
                {
                    compForbiddable.Forbidden = true;
                }
            }
            else if (Widgets.ButtonInvisible(rect, true))
            {
                Find.Selector.ClearSelection();
                Find.Selector.Select(thing, true, true);
            }
            TooltipHandler.TipRegionByKey(rect2, "EjectApparelTooltip");
            Widgets.ThingIcon(new Rect(24f, curY, 28f, 28f), thing, 1f, null, false, 1f, false);
            Rect rect3 = new Rect(60f, curY, rect.width - 36f, rect.height);
            rect3.xMax = rect2.xMin;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect3, thing.LabelCap.Truncate(rect3.width, null));
            Text.Anchor = TextAnchor.UpperLeft;
            if (Mouse.IsOver(rect))
            {
                TargetHighlighter.Highlight(thing, true, false, false);
                TooltipHandler.TipRegion(rect, thing.DescriptionDetailed);
            }
            curY += 28f;
        }

        private static readonly CachedTexture DropTex = new CachedTexture("UI/Buttons/Drop");
    }
}
