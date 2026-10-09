using System;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Decides when teaching prompts appear (02 §11, D-076): the L1 ghost hand until the first draw, and at most one
    /// text callout per level pointing at a <see cref="TutorialAnchor"/>, dismissed on draw and re-shown after N
    /// misses. Views subscribe to the events; Gameplay never references UI.
    /// </summary>
    public sealed class TutorialPromptController : MonoBehaviour
    {
        private LevelData _level;
        private Transform _anchor;
        private int _misses;
        private int _clearsAtFire;
        private bool _calloutVisible;
        private bool _ghostVisible;

        /// <summary>Show (true) or hide (false) the onboarding ghost hand.</summary>
        public event Action<bool> GhostHandChanged;

        /// <summary>Show a callout with text at a world anchor (null anchor = hide).</summary>
        public event Action<string, Transform> CalloutChanged;

        private void OnEnable()
        {
            GameEvents.LevelStarted += OnLevelStarted;
            GameEvents.DrawStarted += OnDrawStarted;
            GameEvents.ArrowFired += OnArrowFired;
            GameEvents.ArrowResolved += OnArrowResolved;
            GameEvents.GameplayStateChanged += OnStateChanged;
        }

        private void OnDisable()
        {
            GameEvents.LevelStarted -= OnLevelStarted;
            GameEvents.DrawStarted -= OnDrawStarted;
            GameEvents.ArrowFired -= OnArrowFired;
            GameEvents.ArrowResolved -= OnArrowResolved;
            GameEvents.GameplayStateChanged -= OnStateChanged;
        }

        private void OnLevelStarted(LevelSessionInfo info)
        {
            GameplayController controller = GameplayController.Instance;
            _level = controller != null ? controller.Level : null;
            _misses = 0;
            SetGhost(false);
            SetCallout(false);
            if (_level == null) return;

            LevelLayout layout = controller.Layout;
            TutorialPromptData prompt = _level.TutorialPrompt;
            _anchor = prompt.HasCallout && layout != null && !string.IsNullOrEmpty(prompt.anchorId)
                ? layout.FindAnchor(prompt.anchorId)?.transform
                : null;
        }

        private void OnStateChanged(GameplayState state)
        {
            if (state == GameplayState.Won || state == GameplayState.Failed || state == GameplayState.WinPending || state == GameplayState.Paused)
            {
                SetGhost(false);
                SetCallout(false);
                return;
            }
            if (state != GameplayState.Ready || _level == null) return;
            GameplayController controller = GameplayController.Instance;
            if (controller == null || controller.Quiver.Used > 0) return;
            if (_level.ShowGhostHand) SetGhost(true);
            if (_level.TutorialPrompt.HasCallout && _misses == 0) SetCallout(true);
        }

        private void OnDrawStarted(AimState aim)
        {
            SetGhost(false);
            SetCallout(false);
        }

        private void OnArrowFired(ArrowFiredInfo info)
        {
            SetGhost(false);
            SetCallout(false);
            GameplayController controller = GameplayController.Instance;
            _clearsAtFire = controller != null ? controller.Objectives.Total - controller.Objectives.Remaining : 0;
        }

        private void OnArrowResolved(ArrowResolvedInfo info)
        {
            GameplayController controller = GameplayController.Instance;
            if (controller == null || _level == null) return;
            int clears = controller.Objectives.Total - controller.Objectives.Remaining;
            if (clears > _clearsAtFire) return;
            _misses++;
            int reshow = _level.TutorialPrompt.reshowAfterMisses;
            if (reshow > 0 && _misses >= reshow && controller.Quiver.Remaining > 0) SetCallout(true);
        }

        private void SetGhost(bool visible)
        {
            if (_ghostVisible == visible) return;
            _ghostVisible = visible;
            GhostHandChanged?.Invoke(visible);
        }

        private void SetCallout(bool visible)
        {
            if (!visible && !_calloutVisible) return;
            _calloutVisible = visible;
            CalloutChanged?.Invoke(visible && _level != null ? _level.TutorialPrompt.text : null, visible ? _anchor : null);
        }
    }
}
