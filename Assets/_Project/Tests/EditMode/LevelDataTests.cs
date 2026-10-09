using NUnit.Framework;
using UnityEngine;

namespace ArrowBuster.Tests
{
    public class LevelDataTests
    {
        private LevelData _level;

        [SetUp]
        public void SetUp()
        {
            _level = ScriptableObject.CreateInstance<LevelData>();
            _level.goldPar = 2;
            _level.quiver.Clear();
            _level.quiver.Add(new QuiverEntry(ArrowType.Oak, 3));
            _level.quiver.Add(new QuiverEntry(ArrowType.Heavyhead, 1));
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_level);

        [TestCase(1, 3)]
        [TestCase(2, 3)]
        [TestCase(3, 2)]
        [TestCase(4, 1)]
        public void StarsFor_FollowsGoldParRule(int arrowsUsed, int expectedStars)
        {
            Assert.AreEqual(expectedStars, _level.StarsFor(arrowsUsed));
        }

        [Test]
        public void TotalArrows_SumsQuiver() => Assert.AreEqual(4, _level.TotalArrows);

        [Test]
        public void GlobalIndex_World2Level8_Is28()
        {
            _level.worldId = 2;
            _level.levelNumber = 8;
            Assert.AreEqual(28, _level.GlobalIndex);
        }
    }
}
