using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ArrowBuster.Tests
{
    /// <summary>Verifies the project matches D-006 (physics/time) and D-048 (layers + matrix, 03 §2).</summary>
    public class PhysicsSettingsTests
    {
        private const float Epsilon = 1e-5f;

        [TestCase(6, "Environment")]
        [TestCase(7, "Structure")]
        [TestCase(8, "Objective")]
        [TestCase(9, "Protected")]
        [TestCase(10, "Prop")]
        [TestCase(11, "Rope")]
        [TestCase(12, "Portal")]
        [TestCase(13, "Arrow")]
        [TestCase(14, "Debris")]
        [TestCase(15, "KillZone")]
        [TestCase(16, "Field")]
        public void LayerNames_MatchD048(int layer, string expected)
        {
            Assert.AreEqual(expected, LayerMask.LayerToName(layer), "Run Arrow Buster ▸ Setup ▸ Layers.");
            Assert.AreEqual(expected, PhysicsLayers.NameOf(layer));
        }

        [Test]
        public void CollisionMatrix_ProjectMatchesSpec()
        {
            for (int a = 0; a < 32; a++)
            for (int b = 0; b < 32; b++)
            {
                if (!PhysicsLayers.IsGameplayLayer(a) && !PhysicsLayers.IsGameplayLayer(b)) continue;
                Assert.AreEqual(!PhysicsLayers.ShouldCollide(a, b), Physics.GetIgnoreLayerCollision(a, b),
                    $"Layers {a}/{b}: run Arrow Buster ▸ Setup ▸ Layers.");
            }
        }

        [Test]
        public void CollisionMatrix_IsSymmetric()
        {
            for (int a = 0; a < 32; a++)
            for (int b = 0; b < 32; b++)
                Assert.AreEqual(PhysicsLayers.ShouldCollide(a, b), PhysicsLayers.ShouldCollide(b, a), $"{a}/{b}");
        }

        // Spot checks against the 03 §2.1 table, independent of the spec arrays.
        [TestCase(PhysicsLayers.Debris, new[] { PhysicsLayers.Environment, PhysicsLayers.KillZone })]
        [TestCase(PhysicsLayers.Arrow, new[] { PhysicsLayers.Environment, PhysicsLayers.KillZone })]
        [TestCase(PhysicsLayers.Field, new[] { PhysicsLayers.Prop })]
        [TestCase(PhysicsLayers.Rope, new int[0])]
        [TestCase(PhysicsLayers.Portal, new int[0])]
        [TestCase(PhysicsLayers.Environment, new[] { PhysicsLayers.Structure, PhysicsLayers.Objective, PhysicsLayers.Protected,
                                                     PhysicsLayers.Prop, PhysicsLayers.Arrow, PhysicsLayers.Debris })]
        [TestCase(PhysicsLayers.KillZone, new[] { PhysicsLayers.Structure, PhysicsLayers.Objective, PhysicsLayers.Protected,
                                                  PhysicsLayers.Prop, PhysicsLayers.Arrow, PhysicsLayers.Debris })]
        public void Layer_CollidesOnlyWithDocumentedLayers(int layer, int[] expected)
        {
            for (int other = 0; other < 32; other++)
            {
                bool shouldCollide = System.Array.IndexOf(expected, other) >= 0;
                Assert.AreEqual(shouldCollide, PhysicsLayers.ShouldCollide(layer, other), $"{layer} vs {other}");
            }
        }

        [Test]
        public void QueryMasks_MatchDocumentedLayers()
        {
            Assert.IsTrue(Has(PhysicsLayers.ArrowCastMask, PhysicsLayers.Rope));
            Assert.IsTrue(Has(PhysicsLayers.ArrowCastMask, PhysicsLayers.Portal));
            Assert.IsFalse(Has(PhysicsLayers.ArrowCastMask, PhysicsLayers.KillZone), "KillZone uses KillZoneMask point checks.");
            Assert.IsFalse(Has(PhysicsLayers.ArrowCastMask, PhysicsLayers.Debris));
            Assert.IsFalse(Has(PhysicsLayers.ArrowCastMask, PhysicsLayers.Arrow));
            Assert.IsFalse(Has(PhysicsLayers.ExplosionMask, PhysicsLayers.Protected), "D-092: blasts never touch protected objects.");
            Assert.AreEqual(1 << PhysicsLayers.KillZone, PhysicsLayers.KillZoneMask);
        }

        [Test]
        public void Time_MatchesD006()
        {
            Assert.AreEqual(PhysicsSettingsSpec.FixedDeltaTime, Time.fixedDeltaTime, Epsilon, "Run Arrow Buster ▸ Setup ▸ Physics.");
            Assert.AreEqual(1f / 60f, PhysicsSettingsSpec.FixedDeltaTime, Epsilon);
            Assert.AreEqual(0.1f, Time.maximumDeltaTime, Epsilon);
        }

        [Test]
        public void Solver_MatchesD006()
        {
            Assert.AreEqual(8, Physics.defaultSolverIterations);
            Assert.AreEqual(2, Physics.defaultSolverVelocityIterations);
            Assert.AreEqual(PhysicsSettingsSpec.BounceThreshold, Physics.bounceThreshold, Epsilon);
            Assert.AreEqual(PhysicsSettingsSpec.SleepThreshold, Physics.sleepThreshold, Epsilon);
            Assert.AreEqual(PhysicsSettingsSpec.DefaultMaxAngularSpeed, Physics.defaultMaxAngularSpeed, Epsilon);
            Assert.AreEqual(PhysicsSettingsSpec.DefaultMaxDepenetrationVelocity, Physics.defaultMaxDepenetrationVelocity, Epsilon);
        }

        // No public (non-obsolete) API exposes these two settings, so read the serialized project settings.
        [TestCase("m_EnableEnhancedDeterminism", PhysicsSettingsSpec.EnhancedDeterminism)]
        [TestCase("m_AutoSyncTransforms", PhysicsSettingsSpec.AutoSyncTransforms)]
        public void SerializedPhysicsFlag_MatchesD006(string propertyName, bool expected)
        {
            var dynamics = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset")[0]);
            SerializedProperty property = dynamics.FindProperty(propertyName);
            Assert.IsNotNull(property, propertyName);
            Assert.AreEqual(expected, property.boolValue, "Run Arrow Buster ▸ Setup ▸ Physics.");
        }

        private static bool Has(int mask, int layer) => (mask & (1 << layer)) != 0;
    }
}
