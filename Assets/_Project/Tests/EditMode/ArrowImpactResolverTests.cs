using NUnit.Framework;
using UnityEngine;

namespace ArrowBuster.Tests
{
    public class ArrowImpactResolverTests
    {
        private ArrowDefinition _oak;
        private GameplayTuning _tuning;
        private MaterialProfile _timber;
        private MaterialProfile _stone;
        private MaterialProfile _metal;
        private GameObject _host;

        [SetUp]
        public void SetUp()
        {
            _oak = ScriptableObject.CreateInstance<ArrowDefinition>();
            _oak.Configure(ArrowType.Oak, null, 4f, 13f, 1f, 1f, 15f, 20f, 1f, 2);
            _tuning = GameplayTuning.CreateDefault();
            _timber = Profile(MaterialKind.Timber, embeds: true, ricochetGrazing: 0f);
            _stone = Profile(MaterialKind.Stone, embeds: false, ricochetGrazing: 0f);
            _metal = Profile(MaterialKind.Metal, embeds: false, ricochetGrazing: 25f);
            _host = new GameObject("ImpactHost");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
            Object.DestroyImmediate(_oak);
            Object.DestroyImmediate(_tuning);
            Object.DestroyImmediate(_timber);
            Object.DestroyImmediate(_stone);
            Object.DestroyImmediate(_metal);
        }

        private static MaterialProfile Profile(MaterialKind kind, bool embeds, float ricochetGrazing)
        {
            var p = ScriptableObject.CreateInstance<MaterialProfile>();
            p.Configure(kind, Color.white, 4f, 0.6f, 0.5f, PhysicsMaterialCombine.Average, 0.05f, false, 20f, 2.5f,
                0.5f, 25f, embeds, 60f, 5f, 0.3f, ricochetGrazing, 0.6f, 1f, 1f, 4, 1.6f);
            return p;
        }

        private BodyEntry Entry(MaterialProfile profile, bool dynamic)
        {
            var material = _host.GetComponent<MaterialBody>();
            if (material == null) material = _host.AddComponent<MaterialBody>();
            material.SetProfile(profile);
            Rigidbody body = null;
            if (dynamic)
            {
                body = _host.GetComponent<Rigidbody>();
                if (body == null) body = _host.AddComponent<Rigidbody>();
                body.isKinematic = false;
            }
            return new BodyEntry { Body = body, Material = material, Kind = BodyKind.Structure };
        }

        [Test]
        public void HeadOnTimber_Embeds_WithMeaningfulImpulse()
        {
            ImpactOutcome o = ArrowImpactResolver.Resolve(_oak, _tuning, new Vector3(10f, 0f, 0f), Vector3.left,
                Entry(_timber, dynamic: true), ArrowHitReaction.UseMaterial, 2);

            Assert.AreEqual(ImpactKind.Embed, o.Kind);
            Assert.AreEqual(15f, o.Impulse, 1e-3f);
            Assert.IsTrue(o.Meaningful);
            Assert.AreEqual(Vector3.zero, o.VelocityAfter);
        }

        [Test]
        public void Impulse_ClampsToMax()
        {
            ImpactOutcome o = ArrowImpactResolver.Resolve(_oak, _tuning, new Vector3(30f, 0f, 0f), Vector3.left,
                Entry(_timber, dynamic: true), ArrowHitReaction.UseMaterial, 2);

            Assert.AreEqual(20f, o.Impulse, 1e-3f);
        }

        [Test]
        public void StaticBody_ReceivesNoImpulse()
        {
            ImpactOutcome o = ArrowImpactResolver.Resolve(_oak, _tuning, new Vector3(10f, 0f, 0f), Vector3.left,
                Entry(_timber, dynamic: false), ArrowHitReaction.UseMaterial, 2);

            Assert.AreEqual(0f, o.Impulse);
            Assert.IsFalse(o.Meaningful);
        }

        [Test]
        public void Stone_Deflects_AndReflectsVelocity()
        {
            ImpactOutcome o = ArrowImpactResolver.Resolve(_oak, _tuning, new Vector3(10f, 0f, 0f), Vector3.left,
                Entry(_stone, dynamic: true), ArrowHitReaction.UseMaterial, 2);

            Assert.AreEqual(ImpactKind.Deflect, o.Kind);
            Assert.Less(o.VelocityAfter.x, 0f);
            Assert.AreEqual(15f * 0.6f, o.Impulse, 1e-3f);
        }

        [Test]
        public void GrazingMetal_Ricochets_OnlyWhileRicochetsLeft()
        {
            var grazing = new Vector3(10f, -1f, 0f); // about 6 degrees to the surface
            ImpactOutcome bounce = ArrowImpactResolver.Resolve(_oak, _tuning, grazing, Vector3.up,
                Entry(_metal, dynamic: false), ArrowHitReaction.UseMaterial, 1);
            Assert.AreEqual(ImpactKind.Ricochet, bounce.Kind);
            Assert.IsTrue(bounce.Continues);
            Assert.Greater(bounce.VelocityAfter.y, 0f);

            ImpactOutcome spent = ArrowImpactResolver.Resolve(_oak, _tuning, grazing, Vector3.up,
                Entry(_metal, dynamic: false), ArrowHitReaction.UseMaterial, 0);
            Assert.AreEqual(ImpactKind.Stop, spent.Kind);
        }

        [Test]
        public void HittableReaction_OverridesMaterial()
        {
            ImpactOutcome o = ArrowImpactResolver.Resolve(_oak, _tuning, new Vector3(10f, 0f, 0f), Vector3.left,
                Entry(_timber, dynamic: false), new ArrowHitReaction(ArrowHitReactionKind.PassThrough, 0.9f, true), 2);

            Assert.AreEqual(ImpactKind.PassThrough, o.Kind);
            Assert.IsTrue(o.Meaningful);
            Assert.AreEqual(9f, o.VelocityAfter.x, 1e-3f);
        }

        [Test]
        public void NoMaterial_EnvironmentEmbeds_OtherStops()
        {
            var ground = new BodyEntry { Kind = BodyKind.Environment };
            Assert.AreEqual(ImpactKind.Embed, ArrowImpactResolver.Resolve(_oak, _tuning, Vector3.down * 10f, Vector3.up,
                ground, ArrowHitReaction.UseMaterial, 2).Kind);
            Assert.AreEqual(ImpactKind.Stop, ArrowImpactResolver.Resolve(_oak, _tuning, Vector3.down * 10f, Vector3.up,
                null, ArrowHitReaction.UseMaterial, 2).Kind);
        }

        [TestCase(ImpactKind.Embed, 1f)]
        [TestCase(ImpactKind.Deflect, 0.6f)]
        [TestCase(ImpactKind.Shatter, 0.5f)]
        [TestCase(ImpactKind.Ricochet, 0.3f)]
        [TestCase(ImpactKind.PassThrough, 0.2f)]
        [TestCase(ImpactKind.Ignore, 0f)]
        public void OutcomeFactor_MatchesSpec(ImpactKind kind, float expected)
        {
            Assert.AreEqual(expected, ArrowImpactResolver.OutcomeFactor(kind));
        }
    }
}
