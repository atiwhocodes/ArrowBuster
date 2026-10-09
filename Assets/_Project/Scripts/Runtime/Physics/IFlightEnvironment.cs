using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Environment sampled by the ballistic solver (02 §4.1). Declared in Physics so Props never reference Arrows.</summary>
    public interface IFlightEnvironment
    {
        /// <summary>Wind acceleration (m/s²) at a position on the play plane.</summary>
        Vector3 SampleWind(Vector3 position);
    }
}
