using System;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Turns pointer input into draws (02 §2.3): gates by <see cref="CanDraw"/>, raises draw events, drives the
    /// preview and the bow visuals, and requests a shot on a fireable release. The gameplay controller owns the
    /// quiver and spawning.
    /// </summary>
    public sealed class BowController : MonoBehaviour
    {
        [SerializeField] private BowView _view;
        [SerializeField] private TrajectoryPreview _preview;

        private readonly BowInputReader _input = new BowInputReader();
        private DrawModel _model;
        private GameplayTuning _tuning;
        private bool _thresholdRaised;
        private bool _fullDrawRaised;

        /// <summary>Asked every frame; false cancels any draw (D-083).</summary>
        public Func<bool> CanDraw;

        /// <summary>Raised on a fireable release.</summary>
        public event Action<AimState> FireRequested;

        public bool IsDrawing => _model != null && _model.Active;
        public AimState CurrentAim => _model != null ? _model.Current : default;
        public BowView View => _view;
        public TrajectoryPreview Preview => _preview;

        /// <summary>Arrow tip position for an aim (the nock point in front of the pivot).</summary>
        public Vector3 NockPosition(AimState aim) => _tuning.BowPivot + aim.Direction * _tuning.NockOffset;

        public void Initialise(GameplayTuning tuning)
        {
            _tuning = tuning;
            _model = new DrawModel(tuning);
            transform.position = tuning.BowPivot;
        }

        public void SetNockedArrow(ArrowDefinition next) => _view?.SetNocked(next);

        public void CancelDraw(bool silent)
        {
            if (_model == null || !_model.Active) return;
            _model.Cancel();
            _preview?.Hide();
            _view?.Relax();
            if (!silent) GameEvents.RaiseDrawCancelled(_model.Current);
        }

        private void Update()
        {
            if (_model == null) return;
            bool allowed = CanDraw == null || CanDraw();
            PointerSample sample = _input.Read();

            if (!allowed)
            {
                if (_model.Active) CancelDraw(silent: true);
                return;
            }

            switch (sample.Phase)
            {
                case PointerPhase.Began:
                    if (!sample.StartedOverUi && InAimZone(sample.ScreenPosition)) BeginDraw(sample.ScreenPosition);
                    break;
                case PointerPhase.Moved:
                    if (_model.Active) UpdateDraw(sample.ScreenPosition);
                    break;
                case PointerPhase.Ended:
                    if (_model.Active) Release();
                    break;
            }
        }

        private bool InAimZone(Vector2 position)
        {
            Rect safe = Screen.safeArea;
            return position.y <= safe.yMin + _tuning.AimZoneScreenFraction * safe.height;
        }

        private void BeginDraw(Vector2 position)
        {
            _model.Begin(position);
            _thresholdRaised = false;
            _fullDrawRaised = false;
            GameEvents.RaiseDrawStarted(_model.Current);
            _view?.ShowAim(_model.Current);
        }

        private void UpdateDraw(Vector2 position)
        {
            AimState aim = _model.Update(position, Screen.height);
            _view?.ShowAim(aim);
            _preview?.Show(aim, NockPosition(aim));

            if (aim.IsFireable && !_thresholdRaised)
            {
                _thresholdRaised = true;
                GameEvents.RaiseDrawThresholdReached(aim);
            }
            if (aim.IsFullDraw && !_fullDrawRaised)
            {
                _fullDrawRaised = true;
                GameEvents.RaiseDrawThresholdReached(aim);
            }
        }

        private void Release()
        {
            AimState aim = _model.End();
            _preview?.Hide();
            if (aim.IsFireable)
            {
                _view?.PlayRelease();
                FireRequested?.Invoke(aim);
            }
            else
            {
                _view?.Relax();
                GameEvents.RaiseDrawCancelled(aim);
            }
        }

        /// <summary>Dev/bot entry: fires an exact aim without touch input (IntendedSolutionRunner, tests).</summary>
        public void FireExact(float angleDeg, float power01)
        {
            var aim = new AimState(angleDeg, Mathf.Clamp01(power01), true, power01 >= _tuning.FullDrawHapticPower, 0f);
            _view?.ShowAim(aim);
            _view?.PlayRelease();
            FireRequested?.Invoke(aim);
        }
    }
}
