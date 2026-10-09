using UnityEngine;

namespace ArrowBuster
{
    /// <summary>No wind. Used until wind fields exist (M4) and in tests.</summary>
    public sealed class NullFlightEnvironment : IFlightEnvironment
    {
        public static readonly NullFlightEnvironment Instance = new NullFlightEnvironment();

        public Vector3 SampleWind(Vector3 position) => Vector3.zero;
    }
}
