using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Honest aim preview (02 §3, D-085): simulates the exact flight with <see cref="BallisticSolver"/> and
    /// <see cref="ArrowSweep"/>, passes through ropes, shows the first bounce, and ends at the first blocking hit
    /// with an impact ring. Visible only while drawing. Dots are pooled world-space quads on the play plane.
    /// </summary>
    public sealed class TrajectoryPreview : MonoBehaviour
    {
        private const int MaxSubCasts = 4;
        private const float DotZ = -0.05f;

        [SerializeField] private Mesh _dotMesh;
        [SerializeField] private Material _dotMaterial;
        [SerializeField] private Material _ringMaterial;
        [SerializeField, Range(10, 80)] private int _dotCount = 40;
        [SerializeField] private float _dotSize = 0.13f;
        [SerializeField] private Color _dotColor = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] private Color _weakColor = new Color(1f, 1f, 1f, 0.35f);

        private readonly HashSet<Collider> _passed = new HashSet<Collider>();
        private Transform[] _dots;
        private MeshRenderer[] _dotRenderers;
        private Transform _ring;
        private Transform _bounceMark;
        private MaterialPropertyBlock _block;
        private GameplayTuning _tuning;
        private ArrowDefinition _def;
        private IFlightEnvironment _env = NullFlightEnvironment.Instance;
        private float _scale = 1f;

        /// <summary>Last simulated impact point (bots and tests), or NaN when none.</summary>
        public Vector3 LastImpactPoint { get; private set; }

        /// <summary>Arrow speed at the last impact or pass-through of the target (m/s).</summary>
        public float LastImpactSpeed { get; private set; }

        /// <summary>Collider that ended the last simulation (null if none).</summary>
        public Collider LastImpactCollider { get; private set; }

        /// <summary>Pass-through colliders (ropes, balloons) crossed by the last simulation.</summary>
        public IReadOnlyCollection<Collider> LastPassed => _passed;

        public void Configure(GameplayTuning tuning, ArrowDefinition def, float levelScale, IFlightEnvironment env)
        {
            _tuning = tuning;
            _def = def;
            _scale = Mathf.Clamp(levelScale, 0.4f, 1f); // D-073: never below 0.4, never removed
            _env = env ?? NullFlightEnvironment.Instance;
        }

        private void Awake()
        {
            _block = new MaterialPropertyBlock();
            _dots = new Transform[_dotCount];
            _dotRenderers = new MeshRenderer[_dotCount];
            for (int i = 0; i < _dotCount; i++)
            {
                GameObject dot = CreateQuad("Dot_" + i, _dotMaterial);
                _dots[i] = dot.transform;
                _dotRenderers[i] = dot.GetComponent<MeshRenderer>();
            }
            _ring = CreateQuad("ImpactRing", _ringMaterial != null ? _ringMaterial : _dotMaterial).transform;
            _bounceMark = CreateQuad("BounceMark", _ringMaterial != null ? _ringMaterial : _dotMaterial).transform;
            Hide();
        }

        public void Hide()
        {
            if (_dots == null) return;
            foreach (Transform dot in _dots) dot.gameObject.SetActive(false);
            _ring.gameObject.SetActive(false);
            _bounceMark.gameObject.SetActive(false);
        }

        public void Show(AimState aim, Vector3 tip)
        {
            if (_tuning == null || _def == null || _dots == null) return;
            int used = Simulate(aim, tip, place: true);
            for (int i = used; i < _dots.Length; i++) _dots[i].gameObject.SetActive(false);
        }

        /// <summary>Runs the preview simulation; returns the number of dots placed.</summary>
        public int Simulate(AimState aim, Vector3 tip, bool place, float secondsOverride = -1f)
        {
            float dt = Time.fixedDeltaTime;
            float length = secondsOverride > 0f ? secondsOverride : _tuning.PreviewBaseSeconds * _scale;
            int steps = secondsOverride > 0f ? Mathf.CeilToInt(length / dt) : Mathf.Min(Mathf.CeilToInt(length / dt), _tuning.PreviewMaxSteps);
            ArrowFlightState state = BallisticSolver.Launch(tip, _def.LaunchVelocity(aim), _def.MaxRicochets);

            _passed.Clear();
            LastImpactPoint = new Vector3(float.NaN, float.NaN, float.NaN);
            LastImpactCollider = null;
            int dotIndex = 0;
            int bouncesShown = 0;
            float nextDot = _tuning.PreviewDotSpacingSeconds;
            bool weak = !aim.IsFireable;
            int maxDots = weak ? 3 : _dots.Length;
            if (place)
            {
                _ring.gameObject.SetActive(false);
                _bounceMark.gameObject.SetActive(false);
            }

            for (int step = 0; step < steps; step++)
            {
                ArrowFlightState next = BallisticSolver.Step(state, dt, _tuning.ArrowGravity, _def.GravityScale, _env, _def.WindResponse);
                Vector3 from = state.Position;
                Vector3 to = next.Position;
                Vector3 velocity = next.Velocity;
                int ricochets = state.RicochetsLeft;
                bool stopped = false;

                for (int cast = 0; cast < MaxSubCasts; cast++)
                {
                    if (!ArrowSweep.Cast(from, to, _tuning.SweepRadius, _passed, out RaycastHit hit)) break;
                    BodyEntry entry = PhysicsBodyRegistry.Get(hit.collider);
                    var info = new ArrowHitInfo(hit.point, hit.normal, velocity, _def.Type, isPreview: true);
                    ArrowHitReaction reaction = entry != null && entry.Hittable != null ? entry.Hittable.Evaluate(info) : ArrowHitReaction.UseMaterial;
                    ImpactOutcome outcome = ArrowImpactResolver.Resolve(_def, _tuning, velocity, hit.normal, entry, reaction, ricochets);
                    float remaining = Mathf.Max(0f, (to - from).magnitude - hit.distance);
                    Vector3 hitPoint = from + (to - from).normalized * hit.distance;

                    if (outcome.Kind == ImpactKind.Ricochet && bouncesShown < 1)
                    {
                        bouncesShown++;
                        ricochets--;
                        if (place) PlaceMarker(_bounceMark, hitPoint, 0.32f);
                        velocity = outcome.VelocityAfter;
                        from = hitPoint + hit.normal * (_tuning.SweepRadius + 0.01f);
                        to = from + velocity.normalized * remaining;
                        continue;
                    }
                    if (outcome.Kind == ImpactKind.PassThrough || outcome.Kind == ImpactKind.Shatter)
                    {
                        _passed.Add(hit.collider);
                        LastImpactSpeed = velocity.magnitude;
                        velocity = outcome.VelocityAfter;
                        from = hitPoint;
                        to = from + velocity.normalized * remaining;
                        continue;
                    }
                    LastImpactPoint = hitPoint;
                    LastImpactCollider = hit.collider;
                    LastImpactSpeed = velocity.magnitude;
                    if (place && !weak) PlaceMarker(_ring, hitPoint, 0.42f);
                    stopped = true;
                    break;
                }
                if (stopped) break;

                state = new ArrowFlightState(to, velocity, next.Time, ricochets);
                if (ArrowSweep.InKillZone(state.Position, _tuning.SweepRadius, out _))
                {
                    LastImpactPoint = state.Position;
                    break;
                }
                if (state.Time >= nextDot && dotIndex < maxDots)
                {
                    if (place) PlaceDot(dotIndex, state.Position, (float)step / steps, weak);
                    dotIndex++;
                    nextDot += _tuning.PreviewDotSpacingSeconds;
                }
            }
            return dotIndex;
        }

        private void PlaceDot(int index, Vector3 position, float progress, bool weak)
        {
            Transform dot = _dots[index];
            dot.gameObject.SetActive(true);
            dot.position = new Vector3(position.x, position.y, DotZ);
            float size = _dotSize * Mathf.Lerp(1f, 0.55f, progress);
            dot.localScale = new Vector3(size, size, size);
            Color color = weak ? _weakColor : _dotColor;
            color.a *= Mathf.Lerp(1f, 0.25f, progress);
            _block.SetColor("_BaseColor", color);
            _dotRenderers[index].SetPropertyBlock(_block);
        }

        private static void PlaceMarker(Transform marker, Vector3 position, float size)
        {
            marker.gameObject.SetActive(true);
            marker.position = new Vector3(position.x, position.y, DotZ - 0.01f);
            marker.localScale = new Vector3(size, size, size);
        }

        private GameObject CreateQuad(string name, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = _dotMesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return go;
        }
    }
}
