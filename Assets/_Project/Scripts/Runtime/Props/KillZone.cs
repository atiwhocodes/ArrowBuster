using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Water / spikes / pit trigger (D-039). Any gameplay body entering is removed: objectives clear, protected
    /// objects fail, structure is gone. Arrows check kill zones themselves (point query).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class KillZone : MonoBehaviour
    {
        [SerializeField] private KillZoneKind _kind = KillZoneKind.Water;

        public KillZoneKind Kind => _kind;

        public void SetKind(KillZoneKind kind) => _kind = kind;

        private void Awake()
        {
            gameObject.layer = PhysicsLayers.KillZone;
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null) return;
            var planar = body.GetComponent<PlanarBody>();
            if (planar == null || planar.Removed) return;

            Vector3 position = body.position;
            var materialBody = body.GetComponent<MaterialBody>();
            MaterialKind material = materialBody != null ? materialBody.Kind : MaterialKind.Earth;
            if (_kind == KillZoneKind.Spikes)
            {
                var breakable = body.GetComponent<Breakable>();
                if (breakable != null) breakable.RequestBreak(DamageSource.KillZone);
            }
            else if (_kind == KillZoneKind.Water)
            {
                GameEvents.RaiseObjectBroken(new BreakInfo(material, position, 1f, DamageSource.KillZone));
            }
            planar.Kill(_kind);
        }
    }
}
