using NUnit.Framework;

namespace ArrowBuster.Tests
{
    public class QuiverModelTests
    {
        [Test]
        public void Load_ExpandsEntriesInAuthoredOrder()
        {
            var quiver = new QuiverModel();
            quiver.Load(new[] { new QuiverEntry(ArrowType.Oak, 2), new QuiverEntry(ArrowType.Heavyhead, 1) });

            Assert.AreEqual(3, quiver.Total);
            Assert.AreEqual(ArrowType.Oak, quiver.TypeAt(0));
            Assert.AreEqual(ArrowType.Oak, quiver.TypeAt(1));
            Assert.AreEqual(ArrowType.Heavyhead, quiver.TypeAt(2));
        }

        [Test]
        public void TryConsume_StopsWhenEmpty()
        {
            var quiver = new QuiverModel();
            quiver.Load(new[] { new QuiverEntry(ArrowType.Oak, 1) });

            Assert.IsTrue(quiver.TryConsume(out ArrowType first));
            Assert.AreEqual(ArrowType.Oak, first);
            Assert.AreEqual(0, quiver.Remaining);
            Assert.IsNull(quiver.Peek());
            Assert.IsFalse(quiver.TryConsume(out _));
            Assert.AreEqual(1, quiver.Used);
        }

        [Test]
        public void AddBonus_AppendsAndFlags()
        {
            var quiver = new QuiverModel();
            quiver.Load(new[] { new QuiverEntry(ArrowType.Oak, 1) });
            quiver.TryConsume(out _);
            quiver.AddBonus(ArrowType.Oak);

            Assert.IsTrue(quiver.BonusUsed);
            Assert.AreEqual(1, quiver.Remaining);
        }

        [Test]
        public void Load_ResetsState_AndRaisesChanged()
        {
            var quiver = new QuiverModel();
            int changes = 0;
            quiver.Changed += () => changes++;
            quiver.Load(new[] { new QuiverEntry(ArrowType.Oak, 2) });
            quiver.TryConsume(out _);
            quiver.AddBonus(ArrowType.Oak);
            quiver.Load(new[] { new QuiverEntry(ArrowType.Oak, 2) });

            Assert.AreEqual(0, quiver.Used);
            Assert.IsFalse(quiver.BonusUsed);
            Assert.AreEqual(4, changes);
        }
    }
}
