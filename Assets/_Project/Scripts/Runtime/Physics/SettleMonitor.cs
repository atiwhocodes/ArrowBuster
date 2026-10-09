using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// "Calm" detection (02 §8.1, D-015): the world is calm when no tracked, non-kinematic, non-ambient body moves
    /// faster than the thresholds and nothing external (flying arrows, timers) is active.
    /// </summary>
    public sealed class SettleMonitor
    {
        private readonly float _linearThresholdSqr;
        private readonly float _angularThresholdRad;

        public float CalmSeconds { get; private set; }

        public SettleMonitor(float calmLinearSpeed, float calmAngularSpeedDeg)
        {
            _linearThresholdSqr = calmLinearSpeed * calmLinearSpeed;
            _angularThresholdRad = calmAngularSpeedDeg * Mathf.Deg2Rad;
        }

        public void Reset() => CalmSeconds = 0f;

        /// <summary>Samples once per FixedUpdate.</summary>
        public void Sample(IReadOnlyList<PlanarBody> bodies, bool externalActivity, float dt)
        {
            bool moving = externalActivity;
            if (!moving)
            {
                for (int i = 0; i < bodies.Count; i++)
                {
                    PlanarBody planar = bodies[i];
                    if (planar == null || planar.Removed || planar.IsAmbient) continue;
                    Rigidbody body = planar.Body;
                    if (body.isKinematic || body.IsSleeping()) continue;
                    if (body.linearVelocity.sqrMagnitude > _linearThresholdSqr || body.angularVelocity.magnitude > _angularThresholdRad)
                    {
                        moving = true;
                        break;
                    }
                }
            }
            CalmSeconds = moving ? 0f : CalmSeconds + dt;
        }
    }
}
