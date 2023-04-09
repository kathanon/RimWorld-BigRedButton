using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BigRedButton;
public class DeathRay : PowerBeam {
    public const int TicksPerCell = 200;

    private IntVec3 curDestination;

    private IntVec3 travelMove;
    private int ticksToTravel;
    private int travelStartTick;
    private int curTick;

    public DeathRay() {
        duration = int.MaxValue;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        if (respawningAfterLoad) return;

        curTick = Find.TickManager.TicksGame;
        NewDestination(map);
    }

    private void NewDestination(Map map) {
        curDestination = map.RandomCell();
        CalculateTravel();
    }

    private void CalculateTravel() {
        travelMove = curDestination - Position;
        ticksToTravel = (int) (travelMove.LengthHorizontal * TicksPerCell);
        travelStartTick = curTick;
    }

    private float TravelLeft 
        => (ticksToTravel - (curTick - travelStartTick)) / (float) ticksToTravel;

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref curDestination, "curDestination");

        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            CalculateTravel();
        }
    }

    public override void Tick() {
        base.Tick();
        curTick = Find.TickManager.TicksGame;
        var newPos = curDestination - new IntVec3(travelMove.ToVector3Shifted() * TravelLeft);
        if (newPos != Position) {
            Position = newPos;
            MoteMaker.MakePowerBeamMote(newPos, Map);

        }
        if (Spawned && curTick >= travelStartTick + ticksToTravel) {
            NewDestination(Map);
        }
    }
}

public static class DeathRayExtension {
    public static IntVec3 RandomCell(this Map map) 
        => new(Rand.Range(0, map.Size.x), 0, Rand.Range(0, map.Size.z));
}
