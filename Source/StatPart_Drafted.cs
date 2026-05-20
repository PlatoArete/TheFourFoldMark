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
                    bool qiActive = comp.qiReleaseActive;

                    // Accumulate from all active layers
                    this.AccumulateOffsets(comp.markDef, this.parentStat, qiActive, ref totalOffset);
                    this.AccumulateOffsets(comp.materialDef, this.parentStat, qiActive, ref totalOffset);
                    this.AccumulateOffsets(comp.elementDef, this.parentStat, qiActive, ref totalOffset);
                    this.AccumulateOffsets(comp.beastDef, this.parentStat, qiActive, ref totalOffset);

                    val += totalOffset;
                }
            }
        }

        private void AccumulateOffsets(CultivationLayerDef layerDef, StatDef stat, bool qiActive, ref float totalOffset)
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

            // Combat stats are always on at base value, and doubled when Qi Release is active
            if (layerDef.combatStatOffsets != null)
            {
                foreach (StatModifier modifier in layerDef.combatStatOffsets)
                {
                    if (modifier.stat == stat)
                    {
                        totalOffset += modifier.value;
                        if (qiActive)
                        {
                            totalOffset += modifier.value; // Double the bonus
                        }
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
                    float baseCombatOffset = 0f;
                    float stanceBoostOffset = 0f;
                    bool qiActive = comp.qiReleaseActive;

                    // Calculate detailed breakdowns
                    this.CalculateDetailedOffsets(comp.markDef, this.parentStat, qiActive, ref passiveOffset, ref baseCombatOffset, ref stanceBoostOffset);
                    this.CalculateDetailedOffsets(comp.materialDef, this.parentStat, qiActive, ref passiveOffset, ref baseCombatOffset, ref stanceBoostOffset);
                    this.CalculateDetailedOffsets(comp.elementDef, this.parentStat, qiActive, ref passiveOffset, ref baseCombatOffset, ref stanceBoostOffset);
                    this.CalculateDetailedOffsets(comp.beastDef, this.parentStat, qiActive, ref passiveOffset, ref baseCombatOffset, ref stanceBoostOffset);

                    List<string> lines = new List<string>();
                    if (passiveOffset != 0f)
                    {
                        lines.Add("Cultivation passive: " + this.parentStat.ValueToString(passiveOffset, ToStringNumberSense.Offset));
                    }
                    if (baseCombatOffset != 0f)
                    {
                        lines.Add("Cultivation base combat: " + this.parentStat.ValueToString(baseCombatOffset, ToStringNumberSense.Offset));
                    }
                    if (stanceBoostOffset != 0f)
                    {
                        lines.Add("Cultivation Qi stance boost: " + this.parentStat.ValueToString(stanceBoostOffset, ToStringNumberSense.Offset));
                    }

                    if (lines.Count > 0)
                    {
                        return string.Join("\n", lines);
                    }
                }
            }
            return null;
        }

        private void CalculateDetailedOffsets(CultivationLayerDef layerDef, StatDef stat, bool qiActive, ref float passive, ref float baseCombat, ref float stanceBoost)
        {
            if (layerDef == null) return;

            if (layerDef.passiveStatOffsets != null)
            {
                foreach (StatModifier modifier in layerDef.passiveStatOffsets)
                {
                    if (modifier.stat == stat)
                    {
                        passive += modifier.value;
                    }
                }
            }

            if (layerDef.combatStatOffsets != null)
            {
                foreach (StatModifier modifier in layerDef.combatStatOffsets)
                {
                    if (modifier.stat == stat)
                    {
                        baseCombat += modifier.value;
                        if (qiActive)
                        {
                            stanceBoost += modifier.value;
                        }
                    }
                }
            }
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
