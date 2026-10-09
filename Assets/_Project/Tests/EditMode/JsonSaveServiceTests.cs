using System.IO;
using NUnit.Framework;

namespace ArrowBuster.Tests
{
    public class JsonSaveServiceTests
    {
        private string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "ArrowBusterSaveTests_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
        }

        [Test]
        public void FreshInstall_StartsEmpty()
        {
            var save = new JsonSaveService(_dir);

            Assert.AreEqual(SaveGame.CurrentSchemaVersion, save.Data.schemaVersion);
            Assert.IsEmpty(save.Data.levels);
            Assert.IsNotNull(save.Data.settings);
        }

        [Test]
        public void SaveThenLoad_RoundTrips()
        {
            var first = new JsonSaveService(_dir);
            LevelProgress row = first.Data.GetOrCreate("W1_L04");
            row.cleared = true;
            row.bestStars = 3;
            row.attempts = 2;
            first.Data.coins = 120;
            first.Save();

            var second = new JsonSaveService(_dir);
            LevelProgress loaded = second.Data.Find("W1_L04");
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.cleared);
            Assert.AreEqual(3, loaded.bestStars);
            Assert.AreEqual(2, loaded.attempts);
            Assert.AreEqual(120, second.Data.coins);
            Assert.AreEqual(first.Data.installId, second.Data.installId);
        }

        [Test]
        public void CorruptMainFile_FallsBackToBackup()
        {
            var save = new JsonSaveService(_dir);
            save.Data.coins = 10;
            save.Save();
            save.Data.coins = 20;
            save.Save(); // second save moves the first into save.bak

            File.WriteAllText(Path.Combine(_dir, "save.json"), "{ not json");
            var reloaded = new JsonSaveService(_dir);
            Assert.AreEqual(10, reloaded.Data.coins);
        }

        [Test]
        public void GetOrCreate_DoesNotDuplicateRows()
        {
            var data = new SaveGame();
            data.GetOrCreate("W1_L01");
            data.GetOrCreate("W1_L01");
            Assert.AreEqual(1, data.levels.Count);
        }
    }
}
