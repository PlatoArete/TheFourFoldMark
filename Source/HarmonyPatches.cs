using System;
using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;

namespace RimWorldCultivation
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("com.rimworld.cultivation");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(JobDriver_Meditate), "MeditationTick")]
    public static class Patch_JobDriver_Meditate_MeditationTick
    {
        public static void Postfix(JobDriver_Meditate __instance)
        {
            Pawn pawn = __instance.pawn;
            if (pawn == null) return;

            CompCultivation comp = pawn.GetComp<CompCultivation>();
            if (comp != null)
            {
                // Active meditation gains 0.0003 Qi per tick (about 18 Qi per day of continuous meditation)
                comp.qiCurrent = Math.Min(comp.qiMax, comp.qiCurrent + 0.0003f);

                // Accumulate cultivation progress toward breakthrough
                comp.qiProgress = Math.Min(100f, comp.qiProgress + 0.0003f);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
    public static class Patch_Pawn_PostApplyDamage
    {
        public static void Postfix(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            if (totalDamageDealt <= 0) return;

            // Target pawn (taking damage) gains Qi and progress
            CompCultivation targetComp = __instance.GetComp<CompCultivation>();
            if (targetComp != null)
            {
                // 10% of damage taken converted to immediate Qi reserve, 5% to breakthrough progress
                targetComp.qiCurrent = Math.Min(targetComp.qiMax, targetComp.qiCurrent + totalDamageDealt * 0.1f);
                targetComp.qiProgress = Math.Min(100f, targetComp.qiProgress + totalDamageDealt * 0.05f);
            }

            // Instigator pawn (dealing damage) gains Qi and progress
            if (dinfo.Instigator is Pawn instigator)
            {
                CompCultivation instigatorComp = instigator.GetComp<CompCultivation>();
                if (instigatorComp != null)
                {
                    // 15% of damage dealt converted to immediate Qi reserve, 8% to breakthrough progress
                    instigatorComp.qiCurrent = Math.Min(instigatorComp.qiMax, instigatorComp.qiCurrent + totalDamageDealt * 0.15f);
                    instigatorComp.qiProgress = Math.Min(100f, instigatorComp.qiProgress + totalDamageDealt * 0.08f);
                }
            }
        }
    }
}
