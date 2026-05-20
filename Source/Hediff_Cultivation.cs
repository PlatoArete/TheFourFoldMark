using System;
using System.Text;
using Verse;
using RimWorld;

namespace RimWorldCultivation
{
    public class Hediff_Cultivation : Hediff
    {
        private CompCultivation comp;

        public CompCultivation CultivationComp
        {
            get
            {
                if (comp == null)
                {
                    comp = this.pawn.GetComp<CompCultivation>();
                }
                return comp;
            }
        }

        public override string Label
        {
            get
            {
                if (this.CultivationComp != null)
                {
                    string title = this.CultivationComp.GetEmergentTitle();
                    if (!string.IsNullOrEmpty(title))
                    {
                        return title;
                    }
                }
                return base.Label;
            }
        }

        public override string TipStringExtra
        {
            get
            {
                if (this.CultivationComp == null)
                {
                    return base.TipStringExtra;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Active Cultivation Layers:");
                
                if (this.CultivationComp.markDef != null)
                {
                    sb.AppendLine($"- Mark: {this.CultivationComp.markDef.label}");
                    this.AppendLayerStats(sb, this.CultivationComp.markDef);
                }
                if (this.CultivationComp.materialDef != null)
                {
                    sb.AppendLine($"- Material: {this.CultivationComp.materialDef.label}");
                    this.AppendLayerStats(sb, this.CultivationComp.materialDef);
                }
                if (this.CultivationComp.elementDef != null)
                {
                    sb.AppendLine($"- Element: {this.CultivationComp.elementDef.label}");
                    this.AppendLayerStats(sb, this.CultivationComp.elementDef);
                }
                if (this.CultivationComp.beastDef != null)
                {
                    sb.AppendLine($"- Divine Beast: {this.CultivationComp.beastDef.label}");
                    this.AppendLayerStats(sb, this.CultivationComp.beastDef);
                }

                if (this.CultivationComp.markDef == null &&
                    this.CultivationComp.materialDef == null &&
                    this.CultivationComp.elementDef == null &&
                    this.CultivationComp.beastDef == null)
                {
                    sb.AppendLine("- No layers aligned yet.");
                }

                sb.AppendLine();
                sb.AppendLine("Active combat stance toggled via the 'Cultivation' inspect tab.");

                return sb.ToString().TrimEnd();
            }
        }

        private void AppendLayerStats(StringBuilder sb, CultivationLayerDef layerDef)
        {
            if (layerDef == null) return;

            if (layerDef.passiveStatOffsets != null && layerDef.passiveStatOffsets.Count > 0)
            {
                sb.AppendLine("  Passive stats:");
                foreach (StatModifier modifier in layerDef.passiveStatOffsets)
                {
                    if (modifier.stat != null)
                    {
                        sb.AppendLine($"    • {modifier.stat.LabelCap}: {modifier.ValueToStringAsOffset}");
                    }
                }
            }

            if (layerDef.combatStatOffsets != null && layerDef.combatStatOffsets.Count > 0)
            {
                sb.AppendLine("  Combat stats (doubled when Qi Released):");
                foreach (StatModifier modifier in layerDef.combatStatOffsets)
                {
                    if (modifier.stat != null)
                    {
                        sb.AppendLine($"    • {modifier.stat.LabelCap}: {modifier.ValueToStringAsOffset}");
                    }
                }
            }
        }

        public override bool ShouldRemove => this.CultivationComp == null || 
            (this.CultivationComp.markDef == null &&
             this.CultivationComp.materialDef == null &&
             this.CultivationComp.elementDef == null &&
             this.CultivationComp.beastDef == null);
    }
}
