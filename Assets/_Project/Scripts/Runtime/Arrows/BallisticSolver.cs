using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// The single source of truth for arrow flight AND the trajectory preview (D-005, 02 §4.2): semi-implicit Euler
    /// with gravity and wind, always on the play plane. Pure and allocation-free.
    /// </summary>
    public static class BallisticSolver
    {
        public static ArrowFlightState Launch(Vector3 position, Vector3 velocity, int ricochets)
        {
            position.z = GameConstants.PlayPlaneZ;
            velocity.z = 0f;
            return new ArrowFlightState(position, velocity, 0f, ricochets);
        }

        public static ArrowFlightState Step(in ArrowFlightState s, float dt, float gravity, float gravityScale,
            IFlightEnvironment env, float windResponse)
        {
            Vector3 a = new Vector3(0f, -gravity * gravityScale, 0f);
            if (env != null) a += env.SampleWind(s.Position) * windResponse;
            Vector3 v = s.Velocity + a * dt;
            Vector3 p = s.Position + v * dt;
            p.z = GameConstants.PlayPlaneZ;
            v.z = 0f;
            return new ArrowFlightState(p, v, s.Time + dt, s.RicochetsLeft);
        }
    }
}
