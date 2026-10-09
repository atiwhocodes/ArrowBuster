using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Solvability bot over a whole playlist (AB-025, 07 §5.2 layer 1/2): plays every level's recorded intended
    /// shots through the real game loop and logs one line per level plus a summary. Dev tool.
    /// </summary>
    public sealed class LevelAudit : MonoBehaviour
    {
        public static string Summary { get; private set; }
        public static bool Running { get; private set; }

        public static void RunAll()
        {
            if (Running || GameplayController.Instance == null) return;
            new GameObject("LevelAudit").AddComponent<LevelAudit>();
        }

        private IEnumerator Start()
        {
            Running = true;
            Summary = null;
            GameplayController controller = GameplayController.Instance;
            var lines = new List<string>();
            int passed = 0;
            for (int i = 0; i < controller.Catalog.Count; i++)
            {
                controller.PlayLevel(i);
                LevelData level = controller.Level;
                if (level.IntendedShots.Count == 0)
                {
                    lines.Add($"{level.LevelId}: NO SHOTS RECORDED");
                    continue;
                }
                IntendedSolutionRunner.Run(level.IntendedShots, restartFirst: false);
                yield return null;
                while (IntendedSolutionRunner.Running) yield return null;
                IntendedSolutionRunner.Report? report = IntendedSolutionRunner.Last;
                bool ok = report.HasValue && report.Value.Won && report.Value.ArrowsUsed <= level.GoldPar;
                if (ok) passed++;
                lines.Add(report.HasValue
                    ? $"{level.LevelId}: {(ok ? "PASS" : "FAIL")} won={report.Value.Won} stars={report.Value.Stars} arrows={report.Value.ArrowsUsed}/{level.GoldPar} " +
                      $"fail={report.Value.FailReason} sim={report.Value.SimSeconds:0.0}s"
                    : $"{level.LevelId}: NO REPORT");
            }
            var sb = new StringBuilder($"[Audit] {passed}/{controller.Catalog.Count} levels pass at par\n");
            foreach (string line in lines) sb.AppendLine("  " + line);
            Summary = sb.ToString();
            Debug.Log(Summary);
            Running = false;
            Destroy(gameObject);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Summary = null;
            Running = false;
        }
    }
}
