using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace RimWorldCultivation
{
    public class StatPart_Cultivation : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn)
            {
                CompCultivation comp = pawn.GetComp<CompCultivation>();
                if (comp != null)
                {
                    float totalOffset = 0f;
                    bool isCombat = (pawn.Drafted || pawn.InMentalState) && comp.qiReleaseActive;

                    // Accumulate from all active layers
                    this.AccumulateOffsets(comp.markDef, this.parentStat, isCombat, ref totalOffset);
                    this.AccumulateOffsets(comp.materialDef, this.parentStat, isCombat, ref totalOffset);
                    this.AccumulateOffsets(comp.elementDef, this.parentStat, isCombat, ref totalOffset);
                    this.AccumulateOffsets(comp.beastDef, this.parentStat, isCombat, ref totalOffset);

                    val += totalOffset;
                }
            }
        }

        private void AccumulateOffsets(CultivationLayerDef layerDef, StatDef stat, bool isCombat, ref float totalOffset)
        {
            if (layerDef == null)
            {
                return;
            }

            // Passive stats are always on
            if (layerDef.passiveStatOffsets != null)
            {
                foreach (StatModifier modifier in layerDef.passiveStatOffsets)
                {
                    if (modifier.stat == stat)
                    {
                        totalOffset += modifier.value;
                    }
                }
            }

            // Combat stats are only active when drafted or in mental state
            if (isCombat && layerDef.combatStatOffsets != null)
            {
                foreach (StatModifier modifier in layerDef.combatStatOffsets)
                {
                    if (modifier.stat == stat)
                    {
                        totalOffset += modifier.value;
                    }
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.Thing is Pawn pawn)
            {
                CompCultivation comp = pawn.GetComp<CompCultivation>();
                if (comp != null)
                {
                    float passiveOffset = 0f;
                    float combatOffset = 0f;
                    bool isCombat = (pawn.Drafted || pawn.InMentalState) && comp.qiReleaseActive;

                    // Calculate breakdown for explanation
                    this.AccumulateOffsets(comp.markDef, this.parentStat, false, ref passiveOffset);
                    this.AccumulateOffsets(comp.materialDef, this.parentStat, false, ref passiveOffset);
                    this.AccumulateOffsets(comp.elementDef, this.parentStat, false, ref passiveOffset);
                    this.AccumulateOffsets(comp.beastDef, this.parentStat, false, ref passiveOffset);

                    if (isCombat)
                    {
                        float total = 0f;
                        this.AccumulateOffsets(comp.markDef, this.parentStat, true, ref total);
                        this.AccumulateOffsets(comp.materialDef, this.parentStat, true, ref total);
                        this.AccumulateOffsets(comp.elementDef, this.parentStat, true, ref total);
                        this.AccumulateOffsets(comp.beastDef, this.parentStat, true, ref total);
                        combatOffset = total - passiveOffset;
                    }

                    List<string> lines = new List<string>();
                    if (passiveOffset != 0f)
                    {
                        lines.Add("Cultivation passive: " + this.parentStat.ValueToString(passiveOffset, ToStringNumberSense.Offset));
                    }
                    if (isCombat && combatOffset != 0f)
                    {
                        lines.Add("Cultivation combat: " + this.parentStat.ValueToString(combatOffset, ToStringNumberSense.Offset));
                    }

                    if (lines.Count > 0)
                    {
                        return string.Join("\n", lines);
                    }
                }
            }
            return null;
        }
    }

    [StaticConstructorOnStartup]
    public static class StatPartInitializer
    {
        static StatPartInitializer()
        {
            // Inject StatPart_Cultivation into all loaded StatDefs dynamically
            foreach (StatDef statDef in DefDatabase<StatDef>.AllDefs)
            {
                if (statDef.parts == null)
                {
                    statDef.parts = new List<StatPart>();
                }
                StatPart_Cultivation part = new StatPart_Cultivation();
                part.parentStat = statDef;
                statDef.parts.Add(part);
            }
        }
    }
}
