namespace ArrowBuster
{
    /// <summary>Records level results into the save (AB-022). Best stars never decrease (D-084).</summary>
    public static class ProgressionService
    {
        /// <summary>Applies a win to the save and returns true if the star record improved.</summary>
        public static bool RecordWin(SaveGame save, string levelId, int stars, int arrowsUsed, bool bullseye)
        {
            LevelProgress row = save.GetOrCreate(levelId);
            row.attempts++;
            bool improved = !row.cleared || stars > row.bestStars;
            row.cleared = true;
            if (stars > row.bestStars) row.bestStars = stars;
            if (row.bestArrowsUsed == 0 || arrowsUsed < row.bestArrowsUsed) row.bestArrowsUsed = arrowsUsed;
            row.bullseye |= bullseye;
            return improved;
        }

        public static void RecordFail(SaveGame save, string levelId) => save.GetOrCreate(levelId).attempts++;

        public static int BestStars(SaveGame save, string levelId) => save.Find(levelId)?.bestStars ?? 0;
    }
}
