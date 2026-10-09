using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ArrowBuster.Tests
{
    /// <summary>
    /// End-to-end flows through the real Gameplay scene: pointer drag to fire, out-of-arrows fail, protected-object
    /// fail, pause/resume. Catalog order: 0 = W1_L01, 4 = W1_L16 (vase on a low ledge).
    /// </summary>
    public class GameplayFlowTests
    {
        private Mouse _mouse;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            TestSave.UseTempSave();
            yield return SceneManager.LoadSceneAsync("Gameplay");
            float until = Time.realtimeSinceStartup + 10f;
            while (GameplayController.Instance == null && Time.realtimeSinceStartup < until) yield return null;
            Assert.IsNotNull(GameplayController.Instance);
            _mouse = InputSystem.AddDevice<Mouse>("TestMouse");
        }

        [TearDown]
        public void TearDown()
        {
            if (_mouse != null) InputSystem.RemoveDevice(_mouse);
            TimeScaleController.ClearAll();
        }

        private static GameplayController Controller => GameplayController.Instance;

        private static IEnumerator LoadAndWaitReady(int index)
        {
            Controller.PlayLevel(index);
            yield return WaitFor(() => Controller.State == GameplayState.Ready, 5f);
        }

        private static IEnumerator WaitFor(System.Func<bool> condition, float seconds)
        {
            float until = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < until) yield return null;
            Assert.IsTrue(condition(), "Timed out waiting for condition; state = " + Controller.State);
        }

        private IEnumerator MouseTo(Vector2 position, bool pressed)
        {
            _mouse.MakeCurrent();
            InputSystem.QueueStateEvent(_mouse, new MouseState { position = position, buttons = (ushort)(pressed ? 1 : 0) });
            yield return null;
            _mouse.MakeCurrent();
            yield return null;
        }

        [UnityTest]
        public IEnumerator PointerDrag_ShowsPreview_AndFiresOnRelease()
        {
            yield return LoadAndWaitReady(0);
            int fired = 0;
            System.Action<ArrowFiredInfo> onFired = _ => fired++;
            GameEvents.ArrowFired += onFired;
            try
            {
                var press = new Vector2(Screen.width * 0.5f, Screen.height * 0.3f);
                var pull = new Vector2(-0.05f, -0.15f) * Screen.height;
                yield return MouseTo(press, true);
                Assert.AreEqual(GameplayState.Drawing, Controller.State, "press in the aim zone starts a draw");

                yield return MouseTo(press + pull, true);
                AimState aim = Controller.Bow.CurrentAim;
                Assert.Greater(aim.AngleDeg, 60f);
                Assert.Less(aim.AngleDeg, 85f);
                Assert.IsTrue(aim.IsFireable);
                int visibleDots = 0;
                foreach (Transform child in Controller.Bow.Preview.transform)
                    if (child.gameObject.activeSelf) visibleDots++;
                Assert.Greater(visibleDots, 3, "trajectory preview is visible while drawing");

                yield return MouseTo(press + pull, false);
                Assert.AreEqual(1, fired);
                Assert.AreEqual(1, Controller.Quiver.Used);
            }
            finally
            {
                GameEvents.ArrowFired -= onFired;
            }
        }

        [UnityTest]
        public IEnumerator TinyDrag_IsCancelled_AndConsumesNoArrow()
        {
            yield return LoadAndWaitReady(0);
            var press = new Vector2(Screen.width * 0.5f, Screen.height * 0.3f);
            yield return MouseTo(press, true);
            yield return MouseTo(press + new Vector2(0f, -4f), true);
            yield return MouseTo(press + new Vector2(0f, -4f), false);

            Assert.AreEqual(0, Controller.Quiver.Used);
            yield return WaitFor(() => Controller.State == GameplayState.Ready, 2f);
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator MissingEveryArrow_FailsOutOfArrows()
        {
            yield return LoadAndWaitReady(0);
            LevelResultInfo? failed = null;
            System.Action<LevelResultInfo> onFailed = r => failed = r;
            GameEvents.LevelFailed += onFailed;
            try
            {
                int total = Controller.Quiver.Total;
                for (int i = 0; i < total; i++)
                {
                    yield return WaitFor(() => Controller.CanDraw(), 5f);
                    Assert.IsTrue(Controller.FireExact(170f, 0.2f)); // short lob into the ground, left of the bow
                }
                yield return WaitFor(() => failed.HasValue, 15f);
                Assert.AreEqual(FailReason.OutOfArrows, failed.Value.FailReason);
                Assert.AreEqual(GameplayState.Failed, Controller.State);
                Assert.IsFalse(Controller.FireExact(90f, 1f), "no firing after the level has failed");
            }
            finally
            {
                GameEvents.LevelFailed -= onFailed;
            }
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator HittingTheVase_FailsProtectedLost()
        {
            yield return LoadAndWaitReady(4);
            ProtectedObject vase = Controller.Layout.GetComponentInChildren<ProtectedObject>();
            Assert.IsNotNull(vase, "W1_L16 has a protected vase");
            Collider vaseCollider = null;
            foreach (Collider c in vase.GetComponentsInChildren<Collider>())
                if (!c.isTrigger) { vaseCollider = c; break; }
            Assert.IsNotNull(vaseCollider);

            AimFinder.Result aim = AimFinder.Find(Controller.Bow, Controller.Bow.Preview, vaseCollider, 1f, 0.04f);
            Assume.That(aim.Found, "no direct line to the vase in this layout");

            LevelResultInfo? failed = null;
            System.Action<LevelResultInfo> onFailed = r => failed = r;
            GameEvents.LevelFailed += onFailed;
            try
            {
                Assert.IsTrue(Controller.FireExact(aim.Best.x, aim.Best.y));
                yield return WaitFor(() => failed.HasValue, 15f);
                Assert.AreEqual(FailReason.ProtectedLost, failed.Value.FailReason);
            }
            finally
            {
                GameEvents.LevelFailed -= onFailed;
            }
        }

        [UnityTest]
        public IEnumerator PauseAndResume_FreezeAndRestoreTime()
        {
            yield return LoadAndWaitReady(0);
            Controller.Pause();
            Assert.AreEqual(GameplayState.Paused, Controller.State);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsFalse(Controller.CanDraw());

            Controller.Resume();
            Assert.AreEqual(GameplayState.Ready, Controller.State);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator Retry_ResetsQuiverAndObjectives()
        {
            yield return LoadAndWaitReady(0);
            Assert.IsTrue(Controller.FireExact(170f, 0.2f));
            yield return WaitFor(() => Controller.Quiver.Used == 1, 2f);

            Controller.Retry();
            yield return WaitFor(() => Controller.State == GameplayState.Ready, 5f);
            Assert.AreEqual(0, Controller.Quiver.Used);
            Assert.AreEqual(Controller.Objectives.Total, Controller.Objectives.Remaining);
            Assert.Greater(Controller.Attempt, 1);
        }
    }
}
