using NUnit.Framework;

namespace ArrowBuster.Tests
{
    public class TimeScaleResolveTests
    {
        [Test]
        public void PauseWins()
        {
            Assert.AreEqual(0f, TimeScaleController.Resolve(true, 1f, 0.3f, 5f, 0.05f, 5f));
        }

        [Test]
        public void SlowMoBeatsHitStop()
        {
            Assert.AreEqual(0.3f, TimeScaleController.Resolve(false, 1f, 0.3f, 5f, 0.05f, 5f));
        }

        [Test]
        public void HitStopAppliesUntilExpiry()
        {
            Assert.AreEqual(0.05f, TimeScaleController.Resolve(false, 1f, 0.3f, 0f, 0.05f, 1.06f));
            Assert.AreEqual(1f, TimeScaleController.Resolve(false, 1.06f, 0.3f, 0f, 0.05f, 1.06f));
        }

        [Test]
        public void NothingActive_IsRealTime()
        {
            Assert.AreEqual(1f, TimeScaleController.Resolve(false, 10f, 0.3f, 2f, 0.05f, 3f));
        }
    }
}
