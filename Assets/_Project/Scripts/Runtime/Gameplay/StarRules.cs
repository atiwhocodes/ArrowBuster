namespace ArrowBuster
{
    /// <summary>Stars from arrow efficiency only (mvp.md §3): ≤ par 3★, par + 1 2★, else 1★; bonus-arrow clears cap at 1★ (D-084).</summary>
    public static class StarRules
    {
        public static int Compute(int arrowsUsed, int goldPar, bool bonusArrowUsed)
        {
            if (bonusArrowUsed) return 1;
            if (arrowsUsed <= goldPar) return 3;
            if (arrowsUsed == goldPar + 1) return 2;
            return 1;
        }
    }
}
