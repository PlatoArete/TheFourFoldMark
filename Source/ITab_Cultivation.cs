using UnityEngine;
using Verse;
using RimWorld;

namespace RimWorldCultivation
{
    public class ITab_Cultivation : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(420f, 350f);

        private static readonly Texture2D QiBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.15f, 0.45f, 0.75f));
        private static readonly Texture2D ProgressBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.6f, 0.2f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.15f, 0.15f, 0.15f));

        private CompCultivation SelectedComp
        {
            get
            {
                Thing thing = Find.Selector.SingleSelectedThing;
                if (thing is Pawn pawn)
                {
                    return pawn.GetComp<CompCultivation>();
                }
                return null;
            }
        }

        public override bool IsVisible
        {
            get
            {
                CompCultivation comp = this.SelectedComp;
                return comp != null && comp.Pawn.Faction != null && comp.Pawn.Faction.IsPlayer;
            }
        }

        public ITab_Cultivation()
        {
            this.size = WinSize;
            this.labelKey = "TabCultivation";
        }

        protected override void FillTab()
        {
            CompCultivation comp = this.SelectedComp;
            if (comp == null)
            {
                return;
            }

            Rect rect = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(15f);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect);

            // Emergent Title
            Text.Font = GameFont.Medium;
            string title = comp.GetEmergentTitle() ?? "Uninitiated Cultivator";
            Widgets.Label(listing.GetRect(35f), title);
            Text.Font = GameFont.Small;

            listing.Gap(10f);

            // Qi Reserve Bar
            Rect qiRect = listing.GetRect(24f);
            float qiPercent = comp.qiCurrent / comp.qiMax;
            Widgets.FillableBar(qiRect, qiPercent, QiBarTex, EmptyBarTex, doBorder: true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(qiRect, $"Qi Reserve: {(int)comp.qiCurrent} / {(int)comp.qiMax}");
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(qiRect, "Fuel used for active combat stance. Restored via meditation, work, and combat.");

            listing.Gap(8f);

            // Toggle Qi Release Stance
            bool release = comp.qiReleaseActive;
            Widgets.CheckboxLabeled(listing.GetRect(24f), "Release Qi (Active Combat Stance)", ref release);
            if (release != comp.qiReleaseActive)
            {
                if (release && comp.qiCurrent <= 0f)
                {
                    Messages.Message("No Qi reserve to activate combat stance.", MessageTypeDefOf.RejectInput, historical: false);
                }
                else
                {
                    comp.qiReleaseActive = release;
                }
            }

            listing.Gap(10f);

            // Cultivation Progress Bar
            Rect progressRect = listing.GetRect(24f);
            float progressPercent = comp.qiProgress / 100f;
            Widgets.FillableBar(progressRect, progressPercent, ProgressBarTex, EmptyBarTex, doBorder: true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(progressRect, $"Cultivation Progress: {comp.qiProgress:F1}%");
            Text.Anchor = TextAnchor.UpperLeft;

            listing.Gap(8f);

            // Breakthrough Button
            if (comp.markDef != null && comp.qiProgress >= 100f && (comp.materialDef == null || comp.elementDef == null))
            {
                Rect btnRect = listing.GetRect(30f);
                if (Widgets.ButtonText(btnRect, "Trigger Breakthrough!"))
                {
                    Find.WindowStack.Add(new Dialog_Breakthrough(comp));
                }
            }
            else
            {
                listing.Gap(30f);
            }

            listing.Gap(10f);
            Widgets.DrawLineHorizontal(rect.x, listing.CurHeight, rect.width);
            listing.Gap(10f);

            // Display Current Layers
            Text.Font = GameFont.Tiny;
            listing.Label($"1. MARK OF MARTIAL DOCTRINE: {(comp.markDef != null ? comp.markDef.label : "None (Perform Tattoo Ritual)")}");
            listing.Label($"2. CONSTITUTIONAL MATERIAL: {(comp.materialDef != null ? comp.materialDef.label : "Locked (Awaiting Breakthrough)")}");
            listing.Label($"3. KINETIC ELEMENT: {(comp.elementDef != null ? comp.elementDef.label : "Locked (Awaiting Breakthrough)")}");
            listing.Label($"4. DIVINE BEAST ALIGNMENT: {(comp.beastDef != null ? comp.beastDef.label : "Locked (Perform Connection Ritual)")}");
            Text.Font = GameFont.Small;

            listing.End();
        }
    }
}
