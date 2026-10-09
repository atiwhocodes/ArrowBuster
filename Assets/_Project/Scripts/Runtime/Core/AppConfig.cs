using TMPro;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Bootstrap configuration loaded from Resources so the code-created <see cref="AppRoot"/> can reach its assets
    /// (sound library, tuning, level playlist). One asset: Assets/_Project/Resources/AppConfig.asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Arrow Buster/App Config", fileName = "AppConfig")]
    public sealed class AppConfig : ScriptableObject
    {
        public const string ResourcePath = "AppConfig";

        [SerializeField] private SoundLibrary _soundLibrary;
        [SerializeField] private GameplayTuning _tuning;
        [SerializeField] private LevelCatalog _mainCatalog;
        [Tooltip("Unlit particle material (referenced here so the shader is never stripped from builds).")]
        [SerializeField] private Material _particleMaterial;
        [Tooltip("Lit material used by cosmetic debris; tinted per material with a property block.")]
        [SerializeField] private Material _debrisMaterial;
        [SerializeField] private Mesh _debrisMesh;
        [Tooltip("UI font (OFL, see docs/art/ASSET_LICENSES.md). Null = TMP default.")]
        [SerializeField] private TMP_FontAsset _uiFont;

        public SoundLibrary SoundLibrary => _soundLibrary;
        public GameplayTuning Tuning => _tuning;
        public LevelCatalog MainCatalog => _mainCatalog;
        public Material ParticleMaterial => _particleMaterial;
        public Material DebrisMaterial => _debrisMaterial;
        public Mesh DebrisMesh => _debrisMesh;
        public TMP_FontAsset UiFont => _uiFont;

        public void SetUiFont(TMP_FontAsset font) => _uiFont = font;

        public void Configure(SoundLibrary soundLibrary, GameplayTuning tuning, LevelCatalog mainCatalog,
            Material particleMaterial, Material debrisMaterial, Mesh debrisMesh)
        {
            _soundLibrary = soundLibrary;
            _tuning = tuning;
            _mainCatalog = mainCatalog;
            _particleMaterial = particleMaterial;
            _debrisMaterial = debrisMaterial;
            _debrisMesh = debrisMesh;
        }

        private static AppConfig _cached;

        /// <summary>Loads (and caches) the config. Returns null and logs an error if it is missing.</summary>
        public static AppConfig Load()
        {
            if (_cached == null)
            {
                _cached = Resources.Load<AppConfig>(ResourcePath);
                if (_cached == null) Log.Error(LogCat.Core, "Resources/AppConfig.asset is missing. Run Arrow Buster ▸ Build ▸ Generate Content.");
            }
            return _cached;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _cached = null;
    }
}
