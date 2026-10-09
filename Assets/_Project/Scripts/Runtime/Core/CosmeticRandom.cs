using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Seeded randomness for cosmetics only (debris spin, particles, audio pitch, shake). Gameplay outcomes must
    /// never use it, nor <c>UnityEngine.Random</c> (01 §9 rule 2).
    /// </summary>
    public static class CosmeticRandom
    {
        private static System.Random _random = new System.Random(1);

        public static void Seed(int seed) => _random = new System.Random(seed);

        public static float Value => (float)_random.NextDouble();

        public static float Range(float min, float max) => min + (max - min) * Value;

        public static Vector3 InsideUnitSphere()
        {
            return new Vector3(Range(-1f, 1f), Range(-1f, 1f), Range(-1f, 1f));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _random = new System.Random(1);
    }
}
