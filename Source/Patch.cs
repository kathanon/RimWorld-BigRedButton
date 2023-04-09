using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BigRedButton;
[HarmonyPatch]
public static class Patch {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GizmoGridDrawer), nameof(GizmoGridDrawer.DrawGizmoGrid))]
    public static void DrawGizmoGrid(ref IEnumerable<Gizmo> gizmos) 
        => gizmos = gizmos.Prepend(Button.Gizmo);


    [HarmonyPostfix]
    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    public static void Load() 
        => Button.Notify_Loaded();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
    public static void NewGame() 
        => Button.Notify_Loaded();


    private static bool inDTOG = false;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ArchitectCategoryTab), nameof(ArchitectCategoryTab.DesignationTabOnGUI))]
    public static void DesignationTabOnGUI_Pre() 
        => inDTOG = true;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ArchitectCategoryTab), nameof(ArchitectCategoryTab.DesignationTabOnGUI))]
    public static void DesignationTabOnGUI_Post() 
        => inDTOG = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GizmoGridDrawer), nameof(GizmoGridDrawer.DrawGizmoGrid))]
    public static void DrawGizmoGrid(ref Gizmo mouseoverGizmo) {
        if (inDTOG && mouseoverGizmo == Button.Gizmo) {
            mouseoverGizmo = null;
        }
    }


    private static readonly List<Gizmo> empty = new();

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapInterface), nameof(MapInterface.MapInterfaceOnGUI_AfterMainTabs))]
    public static void AfterMainTabs() {
        if (Find.MainTabsRoot.OpenTab == null && Find.Selector.NumSelected == 0) {
            float x = InspectPaneUtility.PaneWidthFor(null) + GizmoGridDrawer.GizmoSpacing.y;
            GizmoGridDrawer.DrawGizmoGrid(empty, x, out _);
        }
    }
}
