using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ArrowBuster.Tests
{
    /// <summary>
    /// Solvability bot (07 §5.2): every level in the main catalog must be won at gold par by replaying its recorded
    /// intended shots through the real game loop.
    /// </summary>
    public class VerticalSliceSolvabilityTests
    {
        [UnityTest, Timeout(240000)]
        public IEnumerator EveryCatalogLevel_IsWonAtGoldPar()
        {
            TestSave.UseTempSave();
            yield return SceneManager.LoadSceneAsync("Gameplay");
            float until = Time.realtimeSinceStartup + 10f;
            while (GameplayController.Instance == null && Time.realtimeSinceStartup < until) yield return null;
            Assert.IsNotNull(GameplayController.Instance, "Gameplay scene has no GameplayController");
            Assert.Greater(GameplayController.Instance.Catalog.Count, 0, "Main catalog is empty");

            yield return null;
            LevelAudit.RunAll();
            yield return null;
            while (LevelAudit.Running) yield return null;

            string summary = LevelAudit.Summary;
            Assert.IsNotNull(summary);
            int count = GameplayController.Instance.Catalog.Count;
            StringAssert.StartsWith($"[Audit] {count}/{count} levels pass at par", summary, summary);
        }
    }
}
