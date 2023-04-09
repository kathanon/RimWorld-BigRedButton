using System.Linq;
using UnityEngine;
using Verse;

namespace BigRedButton;

public class Settings : ModSettings {
    private static string[] names;

    public int selected;

    public void DoGUI(Rect rect) {
        if (names == null) {
            //names = Button.names;
        }

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
