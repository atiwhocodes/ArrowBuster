using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Kinematic flight state of an arrow tip (02 §4.1).</summary>
    public readonly struct ArrowFlightState
    {
        public readonly Vector3 Position;
        public readonly Vector3 Velocity;
        public readonly float Time;
        public readonly int RicochetsLeft;

        public ArrowFlightState(Vector3 position, Vector3 velocity, float time, int ricochetsLeft)
        {
            Position = position;
            Velocity = velocity;
            Time = time;
            RicochetsLeft = ricochetsLeft;
        }

        public ArrowFlightState With(Vector3 position, Vector3 velocity, int ricochetsLeft) =>
            new ArrowFlightState(position, velocity, Time, ricochetsLeft);
    }
}
