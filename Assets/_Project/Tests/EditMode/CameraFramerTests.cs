using NUnit.Framework;
using UnityEngine;

namespace ArrowBuster.Tests
{
    public class CameraFramerTests
    {
        private const float Fov = 40f;
        private const float HalfWidth = 5.4f;
        private const float MinHeight = 18f;

        [TestCase(9f / 16f)]
        [TestCase(9f / 19.5f)]
        [TestCase(3f / 4f)]
        public void Compute_AlwaysShowsFullWidthAndMinHeight(float aspect)
        {
            CameraFramer.Compute(aspect, Fov, HalfWidth, MinHeight, out float distance, out float height);

            float tanV = Mathf.Tan(Fov * 0.5f * Mathf.Deg2Rad);
            Assert.GreaterOrEqual(distance * tanV * aspect, HalfWidth - 1e-3f);
            Assert.GreaterOrEqual(height, MinHeight - 1e-3f);
            Assert.AreEqual(2f * distance * tanV, height, 1e-3f);
        }

        [Test]
        public void Compute_TallPhone_IsWidthBound()
        {
            CameraFramer.Compute(9f / 19.5f, Fov, HalfWidth, MinHeight, out _, out float height);
            Assert.Greater(height, MinHeight);
        }

        [Test]
        public void Compute_Tablet_IsHeightBound()
        {
            CameraFramer.Compute(3f / 4f, Fov, HalfWidth, MinHeight, out _, out float height);
            Assert.AreEqual(MinHeight, height, 1e-3f);
        }
    }
}
