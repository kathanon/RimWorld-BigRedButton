using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace BigRedButton;

[StaticConstructorOnStartup]
public static class Button {
    private static readonly Action[] actions = {
        Random,   BossButton,    Harepocalypse,   RabbitOfCaerbannog,     Nuke,   DeathRay,    MeteoriteSwarm,    Flood
    };
    private static readonly string[] names = {
        "Random", "Boss Button", "Harepocalypse", "Rabbit of Caerbannog", "Nuke", "Death Ray", "Meteorite Swarm", "Flood"
    };
    private static int selected = Main.Instance.Settings.Selected;
    private static float lastPress = 0f;
    private static bool doSave = true;

    private static readonly PawnKindDef warmAnimal = FindAnimal(20f, "Hare");
    private static readonly PawnKindDef coldAnimal = FindAnimal(-45f, "Snowhare");
    private static readonly PawnKindDef beast = 
        DefDatabase<PawnKindDef>.GetNamed("kathanon_BigRedButton_RabbitOfCaerbannog");
    private static readonly EffecterDef nukeEffect = 
        DefDatabase<EffecterDef>.GetNamed("GiantExplosion"); 
    private static readonly DamageDef nukeDamageType = 
        DefDatabase<DamageDef>.GetNamed("kathanon_BigRedButton_Nuke"); 
    private static readonly SoundDef nukeSound = 
        DefDatabase<SoundDef>.GetNamed("Explosion_GiantBomb"); 
    private static readonly ThingDef deathRay = 
        DefDatabase<ThingDef>.GetNamed("kathanon_BigRedButton_DeathRay"); 
    private static readonly GameConditionDef swarm = 
        DefDatabase<GameConditionDef>.GetNamed("kathanon_BigRedButton_MeteoriteSwarm"); 
    private static readonly GameConditionDef flood = 
        DefDatabase<GameConditionDef>.GetNamed("kathanon_BigRedButton_Flood"); 

    private static readonly Gizmo gizmo = new Command_Action {
            defaultLabel = "Big Red Button",
            defaultDesc = "Whatever you do, do not push the Big Red Button.",
            icon = Textures.Button,
            action = Push,
            disabled = Disabled
        };

    public static Gizmo Gizmo {
        get {
            gizmo.disabled = Disabled;
            return gizmo;
        }
    }

    public static bool Disabled 
        => Time.realtimeSinceStartup - lastPress < 10.0f;

    public static void Notify_Loaded() 
        => doSave = true;

    public static void Push() {
        lastPress = Time.realtimeSinceStartup;
        Save();
        actions[selected]();
    }

    private static void Save() {
        if (doSave) {
            GameDataSaveLoader.SaveGame(Strings.Name);
            doSave = false;
        }
    }

    private static Map HomeMap
        => Find.CurrentMap.IsPlayerHome ? Find.CurrentMap : Find.RandomPlayerHomeMap;


    private static void Random() 
        => actions[Rand.RangeInclusive(1, actions.Length - 1)]();

    private static void BossButton() 
        => BossButton();

    private static void Harepocalypse() {
        var map = HomeMap;
        int tile = map.Tile;
        bool cold = Find.World.tileTemperatures.GetOutdoorTemp(tile) < -10f;
        var animal = cold ? coldAnimal : warmAnimal;
        List<Thing> pawns = new();
        for (int i = 0; i < 1200; i++) {
            RCellFinder.TryFindRandomPawnEntryCell(out var cell, map, CellFinder.EdgeRoadChance_Animal);
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(animal, null, PawnGenerationContext.NonPlayer, tile));
            GenSpawn.Spawn(pawn, cell, map, Rot4.FromAngleFlat((map.Center - cell).AngleFlat));
            pawn.health.AddHediff(HediffDefOf.Scaria);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
            pawns.Add(pawn);
        }
        Find.LetterStack.ReceiveLetter("Harepocalypse", "The Harepocalypse is here!", LetterDefOf.ThreatBig, pawns);
    }

    private static void RabbitOfCaerbannog() {
        var map = HomeMap;
        int tile = map.Tile;
        RCellFinder.TryFindRandomPawnEntryCell(out var cell, map, CellFinder.EdgeRoadChance_Animal);
        Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(beast, null, PawnGenerationContext.NonPlayer, tile));
        GenSpawn.Spawn(pawn, cell, map, Rot4.FromAngleFlat((map.Center - cell).AngleFlat));
        pawn.health.AddHediff(HediffDefOf.Scaria);
        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
        Find.LetterStack.ReceiveLetter("Death approaches...", "Death awaits you all with nasty, big, pointy teeth.", LetterDefOf.ThreatBig, pawn);
    }

    private static void Nuke() {
        var map = HomeMap;
        var pos = map.Center;
        Effecter effecter = nukeEffect.Spawn();
        effecter.Trigger(new TargetInfo(pos, map), new TargetInfo(pos, map));
        effecter.Cleanup();
        GenExplosion.DoExplosion(center: pos,
                                 map: map,
                                 radius: 200f,
                                 damType: nukeDamageType,
                                 explosionSound: nukeSound,
                                 chanceToStartFire: 0.22f, 
                                 instigator: null);
    }

    private static void DeathRay() {
        var map = HomeMap;
        DeathRay ray = (DeathRay) GenSpawn.Spawn(deathRay, map.RandomCell(), map);
        ray.StartStrike();
    }

    private static void MeteoriteSwarm() 
        => Condition(swarm, 6f, 12f);

    private static void Flood() 
        => Condition(flood, 6f, 12f);


    private static void Condition(GameConditionDef def, float hMinDuration, float hMaxDuration) {
        int duration = (int) (Rand.Range(hMinDuration, hMaxDuration) * 2500);
        var condition = GameConditionMaker.MakeCondition(def, duration);
        HomeMap.GameConditionManager.RegisterCondition(condition);
        Find.LetterStack.ReceiveLetter(def.LabelCap, def.letterText, def.letterDef);
    }

    private static PawnKindDef FindAnimal(float temperature, string def) {
        var res = DefDatabase<PawnKindDef>.GetNamed(def);
        if (def == null) {
            var all  = DefDatabase<PawnKindDef>.AllDefs;
            var hunt = all .Where(CanManhunter);
            var temp = hunt.Where(SuitableTemperature);
            var weak = temp.Where(IsWeak);
            res =  weak.RandomElement()
                ?? temp.RandomElement()
                ?? hunt.RandomElement();
        }
        return res;
        bool IsWeak(PawnKindDef kind)
        => kind.combatPower > 15f && kind.combatPower < 40f;
        bool SuitableTemperature(PawnKindDef kind)
        => temperature > kind.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin)
        && temperature < kind.race.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax);
        bool CanManhunter(PawnKindDef kind)
        => kind.RaceProps.Animal
        && kind.canArriveManhunter 
            && kind.RaceProps.CanPassFences;
    }

    public class Settings : ModSettings {
        public int Selected => selected;

        public void DoGUI(Rect rect) {
            var r = rect;
            Text.Anchor = TextAnchor.MiddleLeft;
            string label = "Event:";
            r.height = 28f;
            r.width = Text.CalcSize(label).x;
            Widgets.Label(r, label);
            GenUI.ResetLabelAlign();

            r.x += r.width + 8f;
            r.width = names.Max(x => Text.CalcSize(x).x) + 22f;
            if (Widgets.ButtonText(r, names[selected])) {
                Find.WindowStack.Add(new FloatMenu(
                    names.Select(
                        (n, i) => new FloatMenuOption(
                            n, 
                            () => selected = i))
                    .ToList()));
            }
        }

        public override void ExposeData() {
            Scribe_Values.Look(ref selected, "event", 0);
        }
    }
}
