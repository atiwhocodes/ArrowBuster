using NUnit.Framework;

namespace ArrowBuster.Tests
{
    public class StarRulesTests
    {
        [TestCase(1, 1, 3)]
        [TestCase(0, 1, 3)]
        [TestCase(2, 1, 2)]
        [TestCase(3, 1, 1)]
        [TestCase(2, 2, 3)]
        [TestCase(3, 2, 2)]
        [TestCase(6, 2, 1)]
        public void Compute_ByArrowsUsed(int used, int par, int expected)
        {
            Assert.AreEqual(expected, StarRules.Compute(used, par, false));
        }

        [Test]
        public void Compute_BonusArrowCapsAtOneStar()
        {
            Assert.AreEqual(1, StarRules.Compute(1, 2, true));
        }
    }
}
