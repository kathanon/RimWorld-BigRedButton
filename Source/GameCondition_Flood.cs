using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BigRedButton;
public class GameCondition_Flood : GameCondition {
    private int nextTick = 0;
    private int dir = Rand.Range(1, 4);
    private int nextLine = 0;
    private int pass = 1;

    public override void GameConditionTick() {
        if (TicksPassed >= nextTick) {
            nextTick += 30;

            var map = AffectedMaps.First();
            DoLine(map);

            nextLine++;
            var size = map.Size;
            if (nextLine >= ((dir < 3) ? size.x : size.z)) {
                nextLine = 0;
                pass++;
            }
        }
    }

    private void DoLine(Map map) {
        int x1, dx = 0, z1, dz = 0, n;
        var size = map.Size;
        if (dir < 3) {
            x1 = (dir == 1) ? nextLine : size.x - nextLine - 1;
            z1 = 0;
            n = size.z;
            dz = 1;
        } else {
            z1 = (dir == 3) ? nextLine : size.z - nextLine - 1;
            x1 = 0;
            n = size.x;
            dx = 1;
        }

        IntVec3 cell = new(x1, 0, z1);
        for (int i = 0; i < n; i++, cell.x += dx, cell.z += dz) {
            TerrainDef current = cell.GetTerrain(map), next;
            bool shallow = !current.IsWater;

            var destroy = cell.GetThingList(map)
                .Where(thing => !(thing.def.building?.isNaturalRock ?? false)
                        && (!thing.def.blockWind || pass > 1 || current == TerrainDefOf.Bridge)
                        && thing.def.destroyable
                        && !(thing is Pawn && shallow))
                .ToList();

            if (current == TerrainDefOf.WaterShallow
                    || current == TerrainDefOf.WaterMovingChestDeep
                    || current == TerrainDefOf.WaterMovingShallow) {
                next = TerrainDefOf.WaterDeep;
            } else if (current == TerrainDefOf.WaterOceanShallow) {
                next = TerrainDefOf.WaterOceanDeep;
            } else if (current == TerrainDefOf.Bridge 
                    || cell.Walkable(map) 
                    || destroy.Count > 0) {
                next = TerrainDefOf.WaterShallow;
            } else {
                next = current;
            }

            foreach (var thing in destroy) {
                thing.Destroy();
            }
            destroy = cell.GetThingList(map)
                .Where(t => !t.def.destroyable)
                .ToList();
            foreach (var thing in destroy) {
                thing.DeSpawn();
            }


            if (next != current) {
                map.terrainGrid.SetTerrain(cell, next);
            }
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref nextTick, "nextTick");
        Scribe_Values.Look(ref dir,      "dir");
        Scribe_Values.Look(ref nextLine, "nextLine");
        Scribe_Values.Look(ref pass,     "pass");
    }
}
