﻿using System;
using System.Collections.Generic;
using System.Text;
using KCSG;
using RimWorld;
using UnityEngine;
using UnityEngine.UI;
using Verse;
using Verse.Noise;
using Verse.Sound;
using static UnityEngine.GraphicsBuffer;

namespace AlphaPrefabs
{
    public class Building_DeployedPrefab : Building
    {
        public PrefabDef prefab;
        public StructureLayoutDef variantLayout;
        public string newLabel;
        public int tickCounter;
        string cachedLabel = "";
        public string variationString = "";

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref prefab, "prefab");
            Scribe_Defs.Look(ref variantLayout, "variantLayout");
            Scribe_Values.Look(ref newLabel, "newLabel");
            Scribe_Values.Look(ref variationString, "variationString");

        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }


            Command_Action buildPrefab = new Command_Action();
            buildPrefab.defaultLabel = "AP_BuildPrefab".Translate();
            buildPrefab.defaultDesc = "AP_BuildPrefabDesc".Translate();
            buildPrefab.icon = ContentFinder<Texture2D>.Get("UI/AP_BuildPrefab", true);
            buildPrefab.hotKey = KeyBindingDefOf.Misc1;
            buildPrefab.action = delegate
            {
                Map map = Map;
                StructureLayoutDef layoutToUse;
                if (variantLayout != null)
                {
                    layoutToUse = variantLayout;
                }
                else
                {
                    layoutToUse = prefab.layout;
                }
                var cleanCellRect = CellRect.CenteredOn(Position, layoutToUse.Sizes.x, layoutToUse.Sizes.z);
                
                if (CheckNoBuildingsOrWater(cleanCellRect)) {
                    InternalDefOf.AP_BuildPrefab.PlayOneShot(new TargetInfo(Position, map, false));
                    GenOption.GetAllMineableIn(cleanCellRect, map);
                    LayoutUtils.CleanRect(layoutToUse, map, cleanCellRect, false);
                    layoutToUse.Generate(cleanCellRect, map);
                }
                   

            };
            yield return buildPrefab;

            Command_Action undeployPrefab = new Command_Action();
            undeployPrefab.defaultLabel = "Undeploy prefab";
            undeployPrefab.defaultLabel = "AP_UndeployPrefab".Translate();
            undeployPrefab.defaultDesc = "AP_UndeployPrefabDesc".Translate();
            undeployPrefab.icon = ContentFinder<Texture2D>.Get("UI/AP_UndeployPrefab", true);
            undeployPrefab.hotKey = KeyBindingDefOf.Misc2;
            undeployPrefab.action = delegate
            {
                InternalDefOf.AP_DeployPrefab.PlayOneShot(new TargetInfo(Position, Map, false));
                ThingDef newThing = InternalDefOf.AP_Prefab;
                Thing prefab = GenSpawn.Spawn(newThing, Position, Map, WipeMode.Vanish);
                Thing_Prefab prefabItem = prefab as Thing_Prefab;
                prefabItem.prefab = this.prefab;
                prefabItem.newLabel = this.newLabel;
                if (variantLayout != null)
                {
                    prefabItem.variantLayout = this.variantLayout;
                    prefabItem.variationString = this.variationString;
                }
                this.DeSpawn();
            };
            yield return undeployPrefab;
        }

        public bool CheckNoBuildingsOrWater(CellRect cellRect)
        {

            foreach(IntVec3 cell in cellRect.Cells)
            {
                if (cell.GetEdifice(Map)!=null && cell.GetEdifice(Map)?.def!=InternalDefOf.AP_DeployedPrefab )
                {
                    Messages.Message("AP_OccupiedBy".Translate(cell.GetEdifice(Map)?.LabelCap), cell.GetEdifice(Map), MessageTypeDefOf.NegativeEvent);
                    return false;

                }
                TerrainDef terrain = cell.GetTerrain(Map);
                if (terrain.passability== Traversability.Impassable)
                {
                    Messages.Message("AP_ImpassableTerrain".Translate(terrain.LabelCap), new LookTargets(cell.ToVector3().ToIntVec3(), Map), MessageTypeDefOf.NegativeEvent);
                    return false;

                }
                Thing thing2 = Map.thingGrid.ThingAt(cell, ThingDefOf.SteamGeyser);
                if(thing2!=null) {
                    Messages.Message("AP_GeyserAt".Translate(), thing2, MessageTypeDefOf.NegativeEvent);
                    return false;

                }

            }

            return true;

        }

        public override void Draw()
        {
            
            var vector = DrawPos;
            float speed = 0.1f;

            float oscillation = Mathf.Cos(Current.Game.tickManager.TicksGame*speed / Mathf.PI);

            vector.y += 6;
            vector.z += 0.8f + oscillation/4;
            GraphicsCache.graphicOrb?.DrawFromDef(vector, Rot4.North, null);
            base.Draw();
            
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());
           
            sb.AppendLine("AP_WillTurnInto".Translate(newLabel));
            if (variationString != "")
            {
                sb.AppendLine("AP_VariantLayout".Translate(variationString));
            }


            return sb.ToString().Trim();


           
        }

        public override string Label
        {
            get
            {
                if (cachedLabel.NullOrEmpty())
                {
                    cachedLabel = def.label + ": " + newLabel;
                }
                return cachedLabel;
            }

        }





    }
}
