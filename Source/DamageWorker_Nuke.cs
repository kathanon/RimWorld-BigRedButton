using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BigRedButton;
public class DamageWorker_Nuke : DamageWorker_AddInjury {
    public override IEnumerable<IntVec3> ExplosionCellsToHit(IntVec3 center,
                                                             Map map,
                                                             float radius,
                                                             IntVec3? needLOSToCell1 = null,
                                                             IntVec3? needLOSToCell2 = null,
                                                             FloatRange? affectedAngle = null) {
        int n = GenRadial.RadialPattern.Length;
        for (int i = 0; i < n; i++) {
            var pos = center + GenRadial.RadialPattern[i];
            if (UseCell(pos, map)) {
                yield return pos;
            }
        }

        float sq2 = Mathf.Sqrt(2f);
        for (float r = GenRadial.MaxRadialPatternRadius; r < radius; r += 1f) {
            float rmin = r, rmax = r + 1f, rmin2 = rmin * rmin, rmax2 = rmax * rmax;
            int min = GenRadial.NumCellsInRadius(rmin / 5f - sq2);
            int max = GenRadial.NumCellsInRadius(rmax / 5f + sq2);
            for (var i = min; i < max; i++) {
                var step = GenRadial.RadialPattern[i];
                var mid = step * 5;
                var inner = mid + new IntVec3(Math.Sign(step.x), 0, Math.Sign(step.z)) * -2;
                if (!(inner + center).InBounds(map)) {
                    if (step.x == step.z) yield break;
                    continue;
                }

                for (int x = -2; x <= 2; x++) {
                    for (int z = -2; z <= 2; z++) {
                        var adj = new IntVec3(mid.x + x, 0, mid.z + z);
                        float dist2 = adj.LengthHorizontalSquared;
                        if (dist2 < rmin2 || dist2 >= rmax2) continue;
                        var pos = center + adj;
                        if (UseCell(pos, map)) {
                            yield return pos;
                        }
                    }
                }
            }
        }

        static bool UseCell(IntVec3 loc, Map map) {
            if (!loc.InBounds(map)) return false;
            if (!(loc.GetRoof(map)?.isThickRoof ?? false)) return true;
            var props = loc.GetEdifice(map)?.def.building;
            if (props == null) return true;
            return !props.isNaturalRock && !props.isResourceRock;
        }
    }
}
