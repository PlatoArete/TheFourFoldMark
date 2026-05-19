using System.Collections.Generic;
using Verse;
using RimWorld;

namespace RimWorldCultivation
{
    public enum CultivationLayerType
    {
        Mark,
        Material,
        Element,
        DivineBeast
    }

    public class SkillGain
    {
        public SkillDef skill;
        public int amount;
    }

    public class CultivationLayerDef : Def
    {
        public CultivationLayerType layerType;

        // Suffix used to synthesize the pawn's title (e.g. "Gate", "Mirage", "Drake")
        public string titleSuffix;

        // Civilian benefits: Passive, permanent, always active
        public List<StatModifier> passiveStatOffsets = new List<StatModifier>();
        public List<SkillGain> civilianSkillPulls = new List<SkillGain>();

        // Martial benefits: Active in combat (when drafted or in mental state)
        public List<StatModifier> combatStatOffsets = new List<StatModifier>();

        // Active abilities granted to the pawn when this layer is unlocked
        public List<AbilityDef> grantedAbilities = new List<AbilityDef>();
    }
}
