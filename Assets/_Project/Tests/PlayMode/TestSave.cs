using System.IO;

namespace ArrowBuster.Tests
{
    /// <summary>Points <see cref="Services.Save"/> at a fresh temp folder so PlayMode runs never write the real save.</summary>
    internal static class TestSave
    {
        public static void UseTempSave()
        {
            string dir = Path.Combine(Path.GetTempPath(), "ArrowBusterPlayModeSave_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            Services.OverrideSave(new JsonSaveService(dir));
        }
    }
}
