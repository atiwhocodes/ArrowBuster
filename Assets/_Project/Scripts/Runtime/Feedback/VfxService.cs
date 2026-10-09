using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Pooled particle effects (05 §9). Two world-space emitters (hard chips and soft puffs) are driven with
    /// <c>Emit</c>, so any number of bursts share two systems and there is no per-effect instantiation.
    /// Readability rule: effects stay small and short so the outcome is visible first.
    /// </summary>
    public sealed class VfxService : MonoBehaviour
    {
        private ParticleSystem _chips;
        private ParticleSystem _puffs;
        private bool _reduced;

        public static VfxService Instance { get; private set; }

        public bool ReducedParticles
        {
            get => _reduced;
            set => _reduced = value;
        }

        private void Awake()
        {
            Instance = this;
            AppConfig config = AppConfig.Load();
            Material material = config != null ? config.ParticleMaterial : null;
            _chips = CreateSystem("Chips", material, softTexture: false, gravity: 1.6f, maxParticles: 500);
            _puffs = CreateSystem("Puffs", material, softTexture: true, gravity: -0.05f, maxParticles: 300);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Hard chips flying outward (splinters, shards, sparks).</summary>
        public void Chips(Vector3 position, Color color, int count, float speed, float size, float lifetime)
        {
            Emit(_chips, position, color, count, speed, size, lifetime, spin: true);
        }

        /// <summary>Soft puffs (dust, chaff, splash).</summary>
        public void Puff(Vector3 position, Color color, int count, float speed, float size, float lifetime)
        {
            Emit(_puffs, position, color, count, speed, size, lifetime, spin: false);
        }

        private void Emit(ParticleSystem system, Vector3 position, Color color, int count, float speed, float size,
            float lifetime, bool spin)
        {
            if (system == null) return;
            if (_reduced) count = Mathf.Max(1, Mathf.RoundToInt(count * 0.4f));
            var p = new ParticleSystem.EmitParams { applyShapeToPosition = false };
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = CosmeticRandom.InsideUnitSphere();
                dir.z *= 0.35f;
                p.position = position + dir * 0.15f;
                p.velocity = dir.normalized * speed * CosmeticRandom.Range(0.5f, 1f) + Vector3.up * speed * 0.35f;
                p.startColor = Color.Lerp(color, Color.white, CosmeticRandom.Range(0f, 0.2f));
                p.startSize = size * CosmeticRandom.Range(0.6f, 1.2f);
                p.startLifetime = lifetime * CosmeticRandom.Range(0.7f, 1.1f);
                p.rotation = spin ? CosmeticRandom.Range(0f, 360f) : 0f;
                p.angularVelocity = spin ? CosmeticRandom.Range(-540f, 540f) : 0f;
                system.Emit(p, 1);
            }
        }

        private ParticleSystem CreateSystem(string name, Material material, bool softTexture, float gravity, int maxParticles)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = gravity;
            main.maxParticles = maxParticles;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = false;

            ParticleSystem.ColorOverLifetimeModule col = system.colorOverLifetime;
            col.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            col.color = gradient;

            ParticleSystem.SizeOverLifetimeModule sizeOverLife = system.sizeOverLifetime;
            sizeOverLife.enabled = true;
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, softTexture
                ? AnimationCurve.Linear(0f, 0.6f, 1f, 1.4f)
                : AnimationCurve.Linear(0f, 1f, 1f, 0.3f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            if (material != null)
            {
                var instance = new Material(material);
                instance.mainTexture = softTexture ? ProceduralTextures.SoftCircle : ProceduralTextures.RoundedSquare;
                renderer.sharedMaterial = instance;
            }
            system.Play();
            return system;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;
    }
}
