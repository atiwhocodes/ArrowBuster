using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Solvability-bot helper (04 §8, 07 §5): scans the aim space with the exact trajectory-preview simulation and
    /// returns every (angle, power) whose path hits — or passes through — a target collider. Pure queries, no
    /// physics stepping, so a full scan takes milliseconds. Also reports the most robust aim (window centre).
    /// </summary>
    public static class AimFinder
    {
        public readonly struct Result
        {
            public readonly List<Vector2> Hits;
            public readonly Vector2 Best;
            public readonly int BestNeighbours;

            public Result(List<Vector2> hits, Vector2 best, int bestNeighbours)
            {
                Hits = hits;
                Best = best;
                BestNeighbours = bestNeighbours;
            }

            public bool Found => Hits.Count > 0;
        }

        public static Result Find(BowController bow, TrajectoryPreview preview, Collider target,
            float angleStep = 0.5f, float powerStep = 0.02f, float minPower = 0.2f, float minImpactSpeed = 0f)
        {
            var hits = new List<Vector2>();
            var set = new HashSet<Vector2Int>();
            for (float angle = 10f; angle <= 170f; angle += angleStep)
            {
                for (float power = minPower; power <= 1.0001f; power += powerStep)
                {
                    var aim = new AimState(angle, Mathf.Min(1f, power), true, power >= 0.98f, 0f);
                    preview.Simulate(aim, bow.NockPosition(aim), place: false, secondsOverride: 4f);
                    bool hit = preview.LastImpactCollider == target;
                    if (!hit)
                        foreach (Collider passed in preview.LastPassed)
                            if (passed == target) { hit = true; break; }
                    if (!hit || preview.LastImpactSpeed < minImpactSpeed) continue;
                    hits.Add(new Vector2(angle, power));
                    set.Add(new Vector2Int(Mathf.RoundToInt(angle / angleStep), Mathf.RoundToInt(power / powerStep)));
                }
            }

            Vector2 best = Vector2.zero;
            int bestCount = -1;
            foreach (Vector2 h in hits)
            {
                int a = Mathf.RoundToInt(h.x / angleStep), p = Mathf.RoundToInt(h.y / powerStep), count = 0;
                for (int da = -3; da <= 3; da++)
                    for (int dp = -2; dp <= 2; dp++)
                        if (set.Contains(new Vector2Int(a + da, p + dp))) count++;
                if (count > bestCount)
                {
                    bestCount = count;
                    best = h;
                }
            }
            return new Result(hits, best, bestCount);
        }
    }
}
