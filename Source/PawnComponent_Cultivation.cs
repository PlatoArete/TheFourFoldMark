using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace RimWorldCultivation
{
    public class CompProperties_Cultivation : CompProperties
    {
        public CompProperties_Cultivation()
        {
            this.compClass = typeof(CompCultivation);
        }
    }

    public class CompCultivation : ThingComp
    {
        public Pawn Pawn => (Pawn)this.parent;

        // Cultivation path choices (null if not unlocked yet)
        public CultivationLayerDef markDef;
        public CultivationLayerDef materialDef;
        public CultivationLayerDef elementDef;
        public CultivationLayerDef beastDef;

        // Qi Pools
        public float qiCurrent = 0f;
        public float qiMax = 100f;
        public float qiProgress = 0f; // Progression bar (0 to 100) toward next breakthrough

        // Stances
        public bool qiReleaseActive = false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref this.markDef, "markDef");
            Scribe_Defs.Look(ref this.materialDef, "materialDef");
            Scribe_Defs.Look(ref this.elementDef, "elementDef");
            Scribe_Defs.Look(ref this.beastDef, "beastDef");
            Scribe_Values.Look(ref this.qiCurrent, "qiCurrent", 0f);
            Scribe_Values.Look(ref this.qiMax, "qiMax", 100f);
            Scribe_Values.Look(ref this.qiProgress, "qiProgress", 0f);
            Scribe_Values.Look(ref this.qiReleaseActive, "qiReleaseActive", false);
        }

        public string GetEmergentTitle()
        {
            string material = this.materialDef != null ? this.materialDef.titleSuffix : "";
            string element = this.elementDef != null ? this.elementDef.titleSuffix : "";
            string mark = this.markDef != null ? this.markDef.titleSuffix : "";
            string beast = this.beastDef != null ? this.beastDef.titleSuffix : "";

            if (string.IsNullOrEmpty(mark) && string.IsNullOrEmpty(material) && string.IsNullOrEmpty(element) && string.IsNullOrEmpty(beast))
            {
                return null;
            }

            List<string> parts = new List<string>();
            parts.Add("The");
            if (!string.IsNullOrEmpty(material)) parts.Add(material);
            if (!string.IsNullOrEmpty(element)) parts.Add(element);
            if (!string.IsNullOrEmpty(mark)) parts.Add(mark);
            if (!string.IsNullOrEmpty(beast)) parts.Add(beast);

            return string.Join(" ", parts);
        }

        public override void CompTick()
        {
            base.CompTick();

            if (this.qiReleaseActive)
            {
                // Drain 1 Qi per second (1f / 60f = 0.01667f per tick)
                this.qiCurrent = Math.Max(0f, this.qiCurrent - 0.01667f);
                if (this.qiCurrent <= 0f)
                {
                    this.qiReleaseActive = false;
                }
            }
            else if (this.Pawn.IsHashIntervalTick(30))
            {
                if (this.markDef != null && this.IsDoingMarkWork())
                {
                    // Gain 0.0015 Qi per check (0.00005f per tick)
                    this.qiCurrent = Math.Min(this.qiMax, this.qiCurrent + 0.0015f);
                    this.qiProgress = Math.Min(100f, this.qiProgress + 0.0015f);
                }
            }
        }

        private bool IsDoingMarkWork()
        {
            if (this.Pawn.jobs?.curJob == null) return false;

            WorkGiverDef workGiver = this.Pawn.jobs.curJob.workGiverDef;
            if (workGiver == null || workGiver.workType == null) return false;

            WorkTypeDef workType = workGiver.workType;
            string defName = this.markDef.defName;

            if (defName == "Mark_Ox")
            {
                return workType == WorkTypeDefOf.Mining || workType == WorkTypeDefOf.Construction;
            }
            if (defName == "Mark_Fox")
            {
                return workType == WorkTypeDefOf.Warden || workType.defName == "Cooking" || workType.defName == "Artistic";
            }
            if (defName == "Mark_Raven")
            {
                return workType == WorkTypeDefOf.Research || workType == WorkTypeDefOf.Crafting || workType == WorkTypeDefOf.Construction;
            }
            if (defName == "Mark_Tiger")
            {
                return workType == WorkTypeDefOf.Handling || workType == WorkTypeDefOf.Warden;
            }
            if (defName == "Mark_Wolf")
            {
                return workType == WorkTypeDefOf.Handling || workType == WorkTypeDefOf.Warden || workType == WorkTypeDefOf.Hunting;
            }
            if (defName == "Mark_Crane")
            {
                return workType == WorkTypeDefOf.Doctor || workType.defName == "Artistic" || workType == WorkTypeDefOf.Research;
            }

            return false;
        }

        public override string CompInspectStringExtra()
        {
            string title = this.GetEmergentTitle();
            if (!string.IsNullOrEmpty(title))
            {
                return "Cultivation Title: " + title + "\nQi: " + (int)this.qiCurrent + "/" + (int)this.qiMax;
            }
            return base.CompInspectStringExtra();
        }
    }

    [StaticConstructorOnStartup]
    public static class CultivationInitializer
    {
        static CultivationInitializer()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.race != null && def.race.Humanlike)
                {
                    if (def.comps == null)
                    {
                        def.comps = new List<CompProperties>();
                    }
                    def.comps.Add(new CompProperties_Cultivation());

                    if (def.inspectorTabs == null)
                    {
                        def.inspectorTabs = new List<Type>();
                    }
                    if (!def.inspectorTabs.Contains(typeof(ITab_Cultivation)))
                    {
                        def.inspectorTabs.Add(typeof(ITab_Cultivation));
                    }

                    if (def.inspectorTabsResolved == null)
                    {
                        def.inspectorTabsResolved = new List<InspectTabBase>();
                    }
                    bool alreadyExists = false;
                    foreach (var tab in def.inspectorTabsResolved)
                    {
                        if (tab.GetType() == typeof(ITab_Cultivation))
                        {
                            alreadyExists = true;
                            break;
                        }
                    }
                    if (!alreadyExists)
                    {
                        def.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Cultivation)));
                    }
                }
            }
        }
    }
}
