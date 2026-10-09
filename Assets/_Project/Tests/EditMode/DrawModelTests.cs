using NUnit.Framework;
using UnityEngine;

namespace ArrowBuster.Tests
{
    public class DrawModelTests
    {
        private const float ScreenHeight = 1000f;

        // Full draw at 22% of screen height, deadzone 1.5%, fireable from 15%, angles 8..172, linear curve.
        private static DrawModel Create() => new DrawModel(0.22f, 0.015f, 0.15f, 8f, 172f, 1f, 0.98f);

        [Test]
        public void PullStraightDown_AimsStraightUp()
        {
            DrawModel model = Create();
            model.Begin(new Vector2(500f, 500f));
            AimState aim = model.Update(new Vector2(500f, 390f), ScreenHeight);

            Assert.AreEqual(90f, aim.AngleDeg, 1e-3f);
            Assert.AreEqual(0.5f, aim.Power01, 1e-3f);
            Assert.IsTrue(aim.IsFireable);
            Assert.IsFalse(aim.IsFullDraw);
        }

        [Test]
        public void PullDownLeft_AimsUpRight()
        {
            DrawModel model = Create();
            model.Begin(new Vector2(500f, 500f));
            AimState aim = model.Update(new Vector2(400f, 400f), ScreenHeight);

            Assert.AreEqual(45f, aim.AngleDeg, 1e-3f);
        }

        [Test]
        public void PowerClampsAtFullDraw()
        {
            DrawModel model = Create();
            model.Begin(new Vector2(500f, 800f));
            AimState aim = model.Update(new Vector2(500f, 100f), ScreenHeight);

            Assert.AreEqual(1f, aim.Power01, 1e-4f);
            Assert.IsTrue(aim.IsFullDraw);
        }

        [Test]
        public void InsideDeadzone_NewDrawStartsStraightUp()
        {
            DrawModel model = Create();
            model.Begin(new Vector2(500f, 500f));
            model.Update(new Vector2(400f, 400f), ScreenHeight);
            model.End();
            model.Begin(new Vector2(500f, 500f));
            AimState aim = model.Update(new Vector2(505f, 500f), ScreenHeight);

            Assert.AreEqual(90f, aim.AngleDeg, 1e-3f);
            Assert.IsFalse(aim.IsFireable);
        }

        [Test]
        public void PullUpward_ClampsToShallowestAngle()
        {
            DrawModel model = Create();
            model.Begin(new Vector2(500f, 500f));
            AimState right = model.Update(new Vector2(400f, 600f), ScreenHeight);
            Assert.AreEqual(8f, right.AngleDeg, 1e-3f);

            model.Begin(new Vector2(500f, 500f));
            AimState left = model.Update(new Vector2(600f, 600f), ScreenHeight);
            Assert.AreEqual(172f, left.AngleDeg, 1e-3f);
        }

        [Test]
        public void End_ReturnsLastAim_AndDeactivates()
        {
            DrawModel model = Create();
            model.Begin(new Vector2(500f, 500f));
            AimState last = model.Update(new Vector2(500f, 300f), ScreenHeight);
            AimState released = model.End();

            Assert.IsFalse(model.Active);
            Assert.AreEqual(last.Power01, released.Power01);
            Assert.AreEqual(last.AngleDeg, released.AngleDeg);
        }
    }
}
