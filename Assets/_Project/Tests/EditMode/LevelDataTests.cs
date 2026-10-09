using System.Collections.Generic;
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
            _level.GoldPar = 2;
            _level.SetQuiver(new List<QuiverEntry> { new QuiverEntry(ArrowType.Oak, 3), new QuiverEntry(ArrowType.Heavyhead, 1) });
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
        public void GlobalIndex_World2Level10_Is30()
        {
            _level.WorldId = 2;
            _level.LevelNumber = 10;
            Assert.AreEqual(30, _level.GlobalIndex);
            Assert.AreEqual("W2_L10", _level.LevelId);
        }
    }
}
