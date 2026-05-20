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
                }
                if (this.CultivationComp.materialDef != null)
                {
                    sb.AppendLine($"- Material: {this.CultivationComp.materialDef.label}");
                }
                if (this.CultivationComp.elementDef != null)
                {
                    sb.AppendLine($"- Element: {this.CultivationComp.elementDef.label}");
                }
                if (this.CultivationComp.beastDef != null)
                {
                    sb.AppendLine($"- Divine Beast: {this.CultivationComp.beastDef.label}");
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

        public override bool ShouldRemove => this.CultivationComp == null || 
            (this.CultivationComp.markDef == null &&
             this.CultivationComp.materialDef == null &&
             this.CultivationComp.elementDef == null &&
             this.CultivationComp.beastDef == null);
    }
}
