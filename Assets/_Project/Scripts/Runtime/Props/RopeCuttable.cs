using System;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Rope/chain (D-009): a ConfigurableJoint distance limit between an anchor and a load, a trigger capsule on the
    /// Rope layer that arrows cut (pass-through, any arrow), and a LineRenderer with a slight cosmetic sag.
    /// Breaking the load or anchor also cuts it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RopeCuttable : MonoBehaviour, IArrowHittable
    {
        [SerializeField] private Rigidbody _load;
        [Tooltip("Attach point on the load, in the load's local space.")]
        [SerializeField] private Vector3 _loadAttach = new Vector3(0f, 0.5f, 0f);
        [Tooltip("Static anchor point (used when Anchor Body is empty).")]
        [SerializeField] private Transform _anchor;
        [SerializeField] private Rigidbody _anchorBody;
        [SerializeField, Min(0.05f)] private float _cutRadius = 0.15f;
        [SerializeField] private LineRenderer _line;
        [SerializeField, Range(0f, 0.3f)] private float _sag = 0.06f;
        [Tooltip("When Load is empty, attach to the first body found straight below the anchor within this distance.")]
        [SerializeField, Min(0.1f)] private float _autoAttachDistance = 6f;

        private ConfigurableJoint _joint;
        private CapsuleCollider _cutZone;
        private Vector3[] _points = new Vector3[8];
        private float _cutAt = -1f;
        private Vector3 _cutPoint;

        public bool IsCut { get; private set; }

        public event Action<RopeCuttable> Cut;

        public void Configure(Rigidbody load, Vector3 loadAttach, Transform anchor, Rigidbody anchorBody, LineRenderer line)
        {
            _load = load;
            _loadAttach = loadAttach;
            _anchor = anchor;
            _anchorBody = anchorBody;
            _line = line;
        }

        private Vector3 AnchorWorld => _anchorBody != null ? _anchorBody.transform.position : _anchor != null ? _anchor.position : transform.position;
        private Vector3 LoadWorld => _load != null ? _load.transform.TransformPoint(_loadAttach) : transform.position;

        private void Awake()
        {
            if (_load == null) AutoAttach();
            var zone = new GameObject("CutZone") { layer = PhysicsLayers.Rope };
            zone.transform.SetParent(transform, false);
            _cutZone = zone.AddComponent<CapsuleCollider>();
            _cutZone.isTrigger = true;
            _cutZone.direction = 1;
            _cutZone.radius = _cutRadius;

            if (_load != null)
            {
                _joint = _load.gameObject.AddComponent<ConfigurableJoint>();
                _joint.autoConfigureConnectedAnchor = false;
                _joint.anchor = _loadAttach;
                _joint.connectedBody = _anchorBody;
                _joint.connectedAnchor = _anchorBody != null ? Vector3.zero : AnchorWorld;
                _joint.xMotion = ConfigurableJointMotion.Limited;
                _joint.yMotion = ConfigurableJointMotion.Limited;
                _joint.zMotion = ConfigurableJointMotion.Limited;
                _joint.angularXMotion = ConfigurableJointMotion.Free;
                _joint.angularYMotion = ConfigurableJointMotion.Free;
                _joint.angularZMotion = ConfigurableJointMotion.Free;
                _joint.linearLimit = new SoftJointLimit { limit = Vector3.Distance(AnchorWorld, LoadWorld), contactDistance = 0.01f };
                _joint.enableCollision = false;

                var loadBreakable = _load.GetComponent<Breakable>();
                if (loadBreakable != null) loadBreakable.Broken += OnEndBroken;
            }
            if (_anchorBody != null)
            {
                var anchorBreakable = _anchorBody.GetComponent<Breakable>();
                if (anchorBreakable != null) anchorBreakable.Broken += OnEndBroken;
            }
            UpdateCutZone();
            UpdateLine();
        }

        private void AutoAttach()
        {
            Physics.SyncTransforms();
            Vector3 origin = AnchorWorld + Vector3.down * 0.05f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _autoAttachDistance, PhysicsLayers.TrackedBodyLayers, QueryTriggerInteraction.Ignore)
                && hit.rigidbody != null)
            {
                _load = hit.rigidbody;
                _loadAttach = _load.transform.InverseTransformPoint(hit.point);
            }
            else
            {
                Log.Warn(LogCat.Level, name + ": rope found no body below its anchor.");
            }
        }

        private void FixedUpdate()
        {
            if (!IsCut) UpdateCutZone();
        }

        private void LateUpdate() => UpdateLine();

        public ArrowHitReaction Evaluate(in ArrowHitInfo hit) =>
            IsCut ? ArrowHitReaction.UseMaterial : new ArrowHitReaction(ArrowHitReactionKind.PassThrough, 1f, meaningful: true);

        public void Apply(in ArrowHitInfo hit)
        {
            if (hit.IsPreview || IsCut) return;
            CutAt(hit.Point);
        }

        public void CutAt(Vector3 point)
        {
            if (IsCut) return;
            IsCut = true;
            _cutAt = Time.time;
            _cutPoint = point;
            if (_joint != null) Destroy(_joint);
            if (_load != null) _load.WakeUp();
            _cutZone.enabled = false;
            Cut?.Invoke(this);
            GameEvents.RaisePropTriggered(new PropTriggerInfo(PropTriggerKind.RopeCut, point));
        }

        private void OnEndBroken(Breakable breakable, DamageSource source) => CutAt((AnchorWorld + LoadWorld) * 0.5f);

        private void UpdateCutZone()
        {
            Vector3 a = AnchorWorld;
            Vector3 b = LoadWorld;
            Vector3 mid = (a + b) * 0.5f;
            Vector3 d = b - a;
            _cutZone.transform.position = mid;
            _cutZone.transform.rotation = d.sqrMagnitude > 1e-6f ? Quaternion.FromToRotation(Vector3.up, d.normalized) : Quaternion.identity;
            _cutZone.height = d.magnitude + _cutRadius * 2f;
        }

        private void UpdateLine()
        {
            if (_line == null) return;
            Vector3 a = AnchorWorld;
            Vector3 b = LoadWorld;
            if (!IsCut)
            {
                _line.positionCount = _points.Length;
                for (int i = 0; i < _points.Length; i++)
                {
                    float t = i / (float)(_points.Length - 1);
                    Vector3 p = Vector3.Lerp(a, b, t);
                    p.x += Mathf.Sin(t * Mathf.PI) * _sag;
                    p.z = -0.02f;
                    _points[i] = p;
                }
                _line.SetPositions(_points);
                return;
            }
            // After the cut: a stub hangs from the anchor and shortens away.
            float k = Mathf.Clamp01((Time.time - _cutAt) / 0.4f);
            Vector3 stubEnd = Vector3.Lerp(_cutPoint, a + Vector3.down * 0.25f, k);
            _line.positionCount = 2;
            _line.SetPosition(0, new Vector3(a.x, a.y, -0.02f));
            _line.SetPosition(1, new Vector3(stubEnd.x, stubEnd.y, -0.02f));
        }
    }
}
