using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Orchestrates one level at a time (02 §1): loading, the state machine, win/fail/settle evaluation (D-015),
    /// re-nock cooldown (D-083), protected-loss priority (D-038) and progression. Gameplay raises
    /// <see cref="GameEvents"/>; UI, feedback and analytics listen. UI calls the public commands.
    /// </summary>
    public sealed class GameplayController : MonoBehaviour
    {
        [SerializeField] private BowController _bow;
        [SerializeField] private ArrowSpawner _spawner;
        [SerializeField] private Transform _levelRoot;
        [SerializeField] private CameraFramer _framer;
        [Tooltip("Playlist override. Empty = AppConfig main catalog.")]
        [SerializeField] private LevelCatalog _catalogOverride;
        [Tooltip("Editor: start on this level when playing the Gameplay scene directly.")]
        [SerializeField] private LevelData _editorFallbackLevel;

        private readonly QuiverModel _quiver = new QuiverModel();
        private readonly ObjectiveTracker _objectives = new ObjectiveTracker();
        private readonly ProtectedTracker _protected = new ProtectedTracker();
        private GameplayTuning _tuning;
        private LevelCatalog _catalog;
        private LevelLoader _loader;
        private SettleMonitor _settle;
        private LevelData _level;
        private int _levelIndex;
        private int _attempt;
        private int _arrowsUsed;
        private float _startRealtime;

        private GameplayState _state = GameplayState.Loading;
        private GameplayState _stateBeforePause;
        private float _introEndsAt;
        private float _cooldownUntil;
        private float _winPendingStart;
        private float _lastArrowResolvedAt;
        private bool _softLockShownForShot;
        private float _pendingResultAt = -1f;
        private bool _pendingWin;
        private FailReason _pendingFailReason;
        private Rect _bounds;

        public static GameplayController Instance { get; private set; }

        public GameplayState State => _state;
        public LevelData Level => _level;
        public LevelCatalog Catalog => _catalog;
        public int LevelIndex => _levelIndex;
        public int Attempt => _attempt;
        public QuiverModel Quiver => _quiver;
        public ObjectiveTracker Objectives => _objectives;
        public ProtectedTracker Protected => _protected;
        public BowController Bow => _bow;
        public ArrowSpawner Spawner => _spawner;
        public LevelLayout Layout => _loader?.Layout;
        public GameplayTuning Tuning => _tuning;
        public float CalmSeconds => _settle != null ? _settle.CalmSeconds : 0f;
        public bool HasNextLevel => _catalog != null && _levelIndex + 1 < _catalog.Count;

        private void Awake()
        {
            Instance = this;
            ServiceInstaller.EnsureInstalled();
            AppConfig config = AppConfig.Load();
            _tuning = config != null && config.Tuning != null ? config.Tuning : GameplayTuning.CreateDefault();
            _catalog = _catalogOverride != null ? _catalogOverride : config != null ? config.MainCatalog : null;
            _loader = new LevelLoader(_levelRoot != null ? _levelRoot : transform);
            _settle = new SettleMonitor(_tuning.CalmLinearSpeed, _tuning.CalmAngularSpeedDeg);

            _bow.Initialise(_tuning);
            _bow.CanDraw = CanDraw;
            _bow.FireRequested += OnFireRequested;
        }

        private void OnEnable()
        {
            GameEvents.ArrowResolved += OnArrowResolved;
            GameEvents.DrawStarted += OnDrawStarted;
            GameEvents.DrawCancelled += OnDrawCancelled;
        }

        private void OnDisable()
        {
            GameEvents.ArrowResolved -= OnArrowResolved;
            GameEvents.DrawStarted -= OnDrawStarted;
            GameEvents.DrawCancelled -= OnDrawCancelled;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            TimeScaleController.ClearAll();
        }

        private void Start()
        {
            int index = FirstUnclearedIndex();
#if UNITY_EDITOR
            if (_editorFallbackLevel != null && _catalog != null)
            {
                int fallback = _catalog.IndexOf(_editorFallbackLevel);
                if (fallback >= 0) index = fallback;
            }
#endif
            LoadLevel(index, retry: false);
        }

        // ---------------------------------------------------------------- commands (UI, bots)

        /// <summary>True while the player may start or continue a draw (D-083).</summary>
        public bool CanDraw() => (_state == GameplayState.Ready || _state == GameplayState.Drawing) && _quiver.Remaining > 0;

        public void Retry()
        {
            if (_level == null) return;
            if (_state != GameplayState.Won && _state != GameplayState.Failed)
            {
                GameEvents.RaiseLevelRestarted(SessionInfo());
            }
            LoadLevel(_levelIndex, retry: true);
        }

        public void Next()
        {
            if (!HasNextLevel) return;
            LoadLevel(_levelIndex + 1, retry: false);
        }

        public void PlayLevel(int index) => LoadLevel(index, retry: false);

        public void Pause()
        {
            if (_state == GameplayState.Paused || _state == GameplayState.Won || _state == GameplayState.Failed) return;
            _bow.CancelDraw(silent: true);
            _stateBeforePause = _state == GameplayState.Drawing ? GameplayState.Ready : _state;
            SetState(GameplayState.Paused);
            TimeScaleController.SetPaused(true);
        }

        public void Resume()
        {
            if (_state != GameplayState.Paused) return;
            TimeScaleController.SetPaused(false);
            SetState(_stateBeforePause);
        }

        /// <summary>Bot/dev: fire an exact shot (IntendedSolutionRunner, tests). Respects the state guard.</summary>
        public bool FireExact(float angleDeg, float power01)
        {
            if (!CanDraw()) return false;
            _bow.FireExact(angleDeg, power01);
            return true;
        }

        // ---------------------------------------------------------------- loading

        private void LoadLevel(int index, bool retry)
        {
            if (_catalog == null || _catalog.Count == 0)
            {
                Log.Error(LogCat.Level, "No level catalog configured.");
                return;
            }
            index = Mathf.Clamp(index, 0, _catalog.Count - 1);
            LevelData level = _catalog.Get(index);
            _attempt = retry && level == _level ? _attempt + 1 : 1;
            _levelIndex = index;
            _level = level;

            SetState(GameplayState.Loading);
            _bow.CancelDraw(silent: true);
            LevelLayout layout = _loader.Load(level, _spawner);

            _quiver.Load(level.Quiver);
            Transform root = _loader.Instance != null ? _loader.Instance.transform : transform;
            _objectives.Rebuild(root);
            _protected.Rebuild(root);
            _settle.Reset();
            _arrowsUsed = 0;
            _softLockShownForShot = false;
            _pendingResultAt = -1f;
            _lastArrowResolvedAt = 0f;
            _startRealtime = Time.realtimeSinceStartup;

            if (_framer != null) _framer.Frame(layout);
            _bounds = ComputeBounds();
            ConfigurePreview();
            _bow.SetNockedArrow(NextDefinition());

            if (_objectives.Total == 0) Log.Error(LogCat.Level, level.LevelId + " has no objectives.");

            _introEndsAt = Time.unscaledTime + _tuning.IntroDurationSeconds;
            SetState(GameplayState.Intro);
            GameEvents.RaiseLevelStarted(SessionInfo());
        }

        private void ConfigurePreview()
        {
            ArrowType? next = _quiver.Peek();
            ArrowDefinition def = next.HasValue ? _spawner.Definition(next.Value) : _spawner.Definition(ArrowType.Oak);
            if (_bow.Preview != null) _bow.Preview.Configure(_tuning, def, _level.TrajectoryPreviewScale, NullFlightEnvironment.Instance);
        }

        private ArrowDefinition NextDefinition()
        {
            ArrowType? next = _quiver.Peek();
            return next.HasValue ? _spawner.Definition(next.Value) : null;
        }

        private Rect ComputeBounds()
        {
            float margin = _tuning.PlayBoundsMargin;
            Rect view = _framer != null ? _framer.VisibleRect : new Rect(-5f, -1f, 10f, 20f);
            return new Rect(view.xMin - margin, Mathf.Min(view.yMin, -1f) - margin, view.width + margin * 2f, view.height + margin * 6f);
        }

        private int FirstUnclearedIndex()
        {
            if (_catalog == null || Services.Save == null) return 0;
            for (int i = 0; i < _catalog.Count; i++)
            {
                LevelData level = _catalog.Get(i);
                if (level != null && !(Services.Save.Data.Find(level.LevelId)?.cleared ?? false)) return i;
            }
            return 0;
        }

        // ---------------------------------------------------------------- per-frame

        private void Update()
        {
            if (_state == GameplayState.Intro && Time.unscaledTime >= _introEndsAt) SetState(GameplayState.Ready);

            if (_pendingResultAt >= 0f && Time.unscaledTime >= _pendingResultAt)
            {
                _pendingResultAt = -1f;
                if (_pendingWin) GameEvents.RaiseLevelWon(ResultInfo(FailReason.None));
                else GameEvents.RaiseLevelFailed(ResultInfo(_pendingFailReason));
            }
        }

        private void FixedUpdate()
        {
            Breakable.FlushPendingBreaks();
            if (_state == GameplayState.Loading || _state == GameplayState.Intro || _state == GameplayState.Paused) return;

            float dt = Time.fixedDeltaTime;
            LevelClock.Tick(dt);
            LevelLayout layout = _loader.Layout;
            _objectives.Tick(layout != null && layout.ClearLineEnabled, layout != null ? layout.ClearLineY : 0f);
            _protected.Tick();
            KillOutOfBounds();
            _settle.Sample(PhysicsBodyRegistry.TrackedBodies, _spawner.FlyingCount > 0, dt);

            if (_state == GameplayState.Won || _state == GameplayState.Failed) return;

            if (_protected.LostThisLevel)
            {
                EnterProtectedFail();
                return;
            }

            if (_objectives.AllCleared && _state != GameplayState.WinPending)
            {
                _bow.CancelDraw(silent: true);
                _winPendingStart = LevelClock.Now;
                SetState(GameplayState.WinPending);
            }

            if (_state == GameplayState.WinPending)
            {
                if (_settle.CalmSeconds >= GameConstants.WinSettleSeconds || LevelClock.Now - _winPendingStart >= GameConstants.WinSettleMaxSeconds)
                    EnterWon();
                return;
            }

            if (_state == GameplayState.Cooldown && LevelClock.Now >= _cooldownUntil)
            {
                if (_quiver.Remaining > 0)
                {
                    _bow.SetNockedArrow(NextDefinition());
                    ConfigurePreview();
                    SetState(GameplayState.Ready);
                }
                else SetState(GameplayState.AwaitingResolution);
            }

            if (_state == GameplayState.AwaitingResolution && _spawner.FlyingCount == 0)
            {
                float sinceResolved = LevelClock.Now - _lastArrowResolvedAt;
                if (_settle.CalmSeconds >= GameConstants.SoftLockSettleSeconds || sinceResolved >= GameConstants.OutOfArrowsMaxWaitSeconds)
                    EnterOutOfArrowsFail();
                return;
            }

            if ((_state == GameplayState.Ready || _state == GameplayState.Cooldown) && _quiver.Remaining > 0 && !_softLockShownForShot
                && _arrowsUsed > 0 && _spawner.FlyingCount == 0 && _settle.CalmSeconds >= GameConstants.SoftLockSettleSeconds)
            {
                _softLockShownForShot = true;
                GameEvents.RaiseSoftLockPrompt(_quiver.Remaining);
            }
        }

        private void KillOutOfBounds()
        {
            var bodies = PhysicsBodyRegistry.TrackedBodies;
            for (int i = 0; i < bodies.Count; i++)
            {
                PlanarBody body = bodies[i];
                if (body == null || body.Removed) continue;
                Vector3 p = body.transform.position;
                if (p.x < _bounds.xMin || p.x > _bounds.xMax || p.y < _bounds.yMin) body.Kill(KillZoneKind.Pit);
            }
        }

        // ---------------------------------------------------------------- transitions

        private void OnFireRequested(AimState aim)
        {
            if (!CanDraw()) return;
            if (!_quiver.TryConsume(out ArrowType type)) return;
            ArrowDefinition def = _spawner.Definition(type);
            if (def == null) return;

            Vector3 tip = _bow.NockPosition(aim);
            _spawner.Fire(type, _tuning, NullFlightEnvironment.Instance, tip, def.LaunchVelocity(aim), _bounds);
            _arrowsUsed++;
            _softLockShownForShot = false;
            _cooldownUntil = LevelClock.Now + _tuning.RenockCooldown;
            SetState(GameplayState.Cooldown);
            GameEvents.RaiseArrowFired(new ArrowFiredInfo(type, aim.AngleDeg, aim.Power01, _arrowsUsed, _quiver.Remaining, tip));
        }

        private void OnArrowResolved(ArrowResolvedInfo info) => _lastArrowResolvedAt = LevelClock.Now;

        private void OnDrawStarted(AimState aim)
        {
            if (_state == GameplayState.Ready) SetState(GameplayState.Drawing);
        }

        private void OnDrawCancelled(AimState aim)
        {
            if (_state == GameplayState.Drawing) SetState(GameplayState.Ready);
        }

        private void EnterWon()
        {
            SetState(GameplayState.Won);
            int stars = StarRules.Compute(_arrowsUsed, _level.GoldPar, _quiver.BonusUsed);
            if (Services.Save != null)
            {
                ProgressionService.RecordWin(Services.Save.Data, _level.LevelId, stars, _arrowsUsed, bullseye: false);
                Services.Save.Save();
            }
            _pendingWin = true;
            _pendingResultAt = Time.unscaledTime + 0.45f;
        }

        private void EnterOutOfArrowsFail()
        {
            SetState(GameplayState.Failed);
            _pendingWin = false;
            _pendingFailReason = FailReason.OutOfArrows;
            _pendingResultAt = Time.unscaledTime + _tuning.OutOfArrowsToastSeconds + 0.4f;
            RecordFail();
        }

        private void EnterProtectedFail()
        {
            _bow.CancelDraw(silent: true);
            SetState(GameplayState.Failed);
            TimeScaleController.RequestSlowMo(_tuning.ProtectedFocusTimeScale, _tuning.ProtectedFocusSeconds);
            _pendingWin = false;
            _pendingFailReason = FailReason.ProtectedLost;
            _pendingResultAt = Time.unscaledTime + _tuning.ProtectedFocusSeconds + 0.3f;
            RecordFail();
        }

        private void RecordFail()
        {
            if (Services.Save == null) return;
            ProgressionService.RecordFail(Services.Save.Data, _level.LevelId);
            Services.Save.Save();
        }

        private void SetState(GameplayState next)
        {
            if (_state == next) return;
            _state = next;
            GameEvents.RaiseGameplayStateChanged(next);
        }

        private LevelSessionInfo SessionInfo() =>
            new LevelSessionInfo(_level.LevelId, _level.WorldId, _level.GlobalIndex, _attempt, _level.TotalArrows);

        private LevelResultInfo ResultInfo(FailReason reason)
        {
            int stars = reason == FailReason.None ? StarRules.Compute(_arrowsUsed, _level.GoldPar, _quiver.BonusUsed) : 0;
            return new LevelResultInfo(SessionInfo(), _arrowsUsed, stars, _quiver.BonusUsed, false,
                Time.realtimeSinceStartup - _startRealtime, reason, _objectives.Remaining, _level.GoldPar);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;
    }
}
