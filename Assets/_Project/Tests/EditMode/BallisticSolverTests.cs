using NUnit.Framework;
using UnityEngine;

namespace ArrowBuster.Tests
{
    public class BallisticSolverTests
    {
        private const float Dt = 1f / 60f;

        [Test]
        public void Launch_ProjectsOntoPlayPlane()
        {
            ArrowFlightState s = BallisticSolver.Launch(new Vector3(1f, 2f, 5f), new Vector3(3f, 4f, 9f), 2);

            Assert.AreEqual(GameConstants.PlayPlaneZ, s.Position.z);
            Assert.AreEqual(0f, s.Velocity.z);
            Assert.AreEqual(2, s.RicochetsLeft);
            Assert.AreEqual(0f, s.Time);
        }

        [Test]
        public void Step_WithoutGravity_IsStraightLine()
        {
            ArrowFlightState s = BallisticSolver.Launch(Vector3.zero, new Vector3(10f, 0f, 0f), 0);
            for (int i = 0; i < 60; i++) s = BallisticSolver.Step(s, Dt, 9.81f, 0f, null, 1f);

            Assert.AreEqual(10f, s.Position.x, 1e-3f);
            Assert.AreEqual(0f, s.Position.y, 1e-4f);
            Assert.AreEqual(1f, s.Time, 1e-4f);
        }

        [Test]
        public void Step_ApexMatchesAnalyticHeight()
        {
            const float g = 9.81f, vy = 10f;
            ArrowFlightState s = BallisticSolver.Launch(Vector3.zero, new Vector3(0f, vy, 0f), 0);
            float apex = 0f;
            for (int i = 0; i < 180; i++)
            {
                s = BallisticSolver.Step(s, Dt, g, 1f, null, 1f);
                apex = Mathf.Max(apex, s.Position.y);
            }
            // Semi-implicit Euler undershoots by about v*dt/2; 0.1 m is well inside the preview tolerance.
            Assert.AreEqual(vy * vy / (2f * g), apex, 0.1f);
        }

        [Test]
        public void Step_IsDeterministic()
        {
            ArrowFlightState a = BallisticSolver.Launch(Vector3.zero, new Vector3(4.2f, 11.3f, 0f), 0);
            ArrowFlightState b = a;
            for (int i = 0; i < 120; i++)
            {
                a = BallisticSolver.Step(a, Dt, 9.81f, 1f, null, 1f);
                b = BallisticSolver.Step(b, Dt, 9.81f, 1f, null, 1f);
            }
            Assert.AreEqual(a.Position, b.Position);
            Assert.AreEqual(a.Velocity, b.Velocity);
        }

        [Test]
        public void Step_AppliesWindTimesResponse()
        {
            var wind = new ConstantWind(new Vector3(2f, 0f, 0f));
            ArrowFlightState s = BallisticSolver.Launch(Vector3.zero, Vector3.zero, 0);
            for (int i = 0; i < 60; i++) s = BallisticSolver.Step(s, Dt, 9.81f, 0f, wind, 0.5f);

            Assert.AreEqual(1f, s.Velocity.x, 1e-3f);
        }

        private sealed class ConstantWind : IFlightEnvironment
        {
            private readonly Vector3 _wind;
            public ConstantWind(Vector3 wind) => _wind = wind;
            public Vector3 SampleWind(Vector3 position) => _wind;
        }
    }
}
