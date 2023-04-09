using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BigRedButton;
public class GameCondition_MeteoriteSwarm : GameCondition {
    private int nextTick = 0;

    public override void GameConditionTick() {
        if (TicksPassed >= nextTick) {
            nextTick += Rand.Range(10, 20);
            foreach (var map in AffectedMaps) {
                CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming,
                                                     map,
                                                     out var cell,
                                      minDistToEdge: 3,
                                allowCellsWithItems: true,
                            allowCellsWithBuildings: true,
                          avoidColonistsIfExplosive: false);
                List<Thing> list = ThingSetMakerDefOf.Meteorite.root.Generate();
                SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, cell, map);
            }
        }
    }
}
