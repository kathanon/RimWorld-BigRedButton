using HarmonyLib;
using Verse;
using UnityEngine;
using RimWorld;

namespace BigRedButton {
    [StaticConstructorOnStartup]
    public class Main : Mod {
        public static Main Instance { get; private set; }

        public Button.Settings Settings => GetSettings<Button.Settings>();

        static Main() {
            var harmony = new Harmony(Strings.ID);
            harmony.PatchAll();
        }

        public Main(ModContentPack content) : base(content) {
            Instance = this;
        }

        public override string SettingsCategory() 
            => Strings.Name;

        public override void DoSettingsWindowContents(Rect inRect) 
            => Settings.DoGUI(inRect);
    }
}
