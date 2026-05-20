using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace RimWorldCultivation
{
    public class Dialog_Breakthrough : Window
    {
        private readonly CompCultivation comp;

        public override Vector2 InitialSize => new Vector2(700f, 420f);

        public Dialog_Breakthrough(CompCultivation comp)
        {
            this.comp = comp;
            this.forcePause = true;
            this.closeOnClickedOutside = false;
            this.doCloseButton = false;
            this.doCloseX = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 40f), "A Cultivation Breakthrough Imminent!");
            Text.Font = GameFont.Small;

            Rect contentRect = new Rect(0f, 45f, inRect.width, inRect.height - 50f);

            bool choosingMaterial = comp.materialDef == null;
            if (choosingMaterial)
            {
                this.DrawOptions(contentRect, "Choose your Constitutional Material layer:", new List<string> { "Material_Wood", "Material_Steel", "Material_Jade" }, def =>
                {
                    comp.materialDef = def;
                });
            }
            else
            {
                this.DrawOptions(contentRect, "Choose your Kinetic Element layer:", new List<string> { "Element_Air", "Element_Fire", "Element_Water" }, def =>
                {
                    comp.elementDef = def;
                });
            }
        }

        private void DrawOptions(Rect rect, string heading, List<string> defNames, Action<CultivationLayerDef> onSelect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);
            listing.Label(heading);
            listing.Gap(15f);

            float colWidth = (rect.width - 20f) / 3f;
            Rect columnsRect = listing.GetRect(rect.height - 80f);

            for (int i = 0; i < defNames.Count; i++)
            {
                CultivationLayerDef def = DefDatabase<CultivationLayerDef>.GetNamed(defNames[i], errorOnFail: false);
                if (def == null) continue;

                Rect colRect = new Rect(columnsRect.x + i * (colWidth + 10f), columnsRect.y, colWidth, columnsRect.height);
                Widgets.DrawBoxSolidWithOutline(colRect, new Color(0.12f, 0.12f, 0.12f), new Color(0.3f, 0.3f, 0.3f));

                Rect innerRect = colRect.ContractedBy(8f);
                Listing_Standard colListing = new Listing_Standard();
                colListing.Begin(innerRect);

                Text.Font = GameFont.Medium;
                colListing.Label(def.label);
                Text.Font = GameFont.Small;
                colListing.Gap(8f);

                Text.Font = GameFont.Tiny;
                colListing.Label(def.description);
                colListing.Gap(8f);

                // List stats
                if (def.passiveStatOffsets != null && def.passiveStatOffsets.Count > 0)
                {
                    colListing.Label("Passive Bonuses:");
                    foreach (StatModifier mod in def.passiveStatOffsets)
                    {
                        if (mod.stat != null)
                        {
                            colListing.Label($" - {mod.stat.LabelCap}: +{mod.ValueToStringAsOffset}");
                        }
                    }
                    colListing.Gap(6f);
                }

                if (def.combatStatOffsets != null && def.combatStatOffsets.Count > 0)
                {
                    colListing.Label("Active Combat Bonuses:");
                    foreach (StatModifier mod in def.combatStatOffsets)
                    {
                        if (mod.stat != null)
                        {
                            colListing.Label($" - {mod.stat.LabelCap}: {mod.ValueToStringAsOffset}");
                        }
                    }
                }
                Text.Font = GameFont.Small;

                // Select Button
                Rect selectBtnRect = new Rect(0f, innerRect.height - 30f, innerRect.width, 30f);
                if (Widgets.ButtonText(selectBtnRect, "Align"))
                {
                    onSelect(def);
                    comp.qiProgress = 0f; // Reset progress bar for next tier
                    comp.Pawn.Drawer.renderer.SetAllGraphicsDirty();
                    comp.UpdateHediffState();
                    
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    Messages.Message($"{comp.Pawn.Name.ToStringShort} has successfully aligned with the {def.label}!", comp.Pawn, MessageTypeDefOf.PositiveEvent);
                    this.Close();
                }

                colListing.End();
            }

            listing.End();
        }
    }
}
