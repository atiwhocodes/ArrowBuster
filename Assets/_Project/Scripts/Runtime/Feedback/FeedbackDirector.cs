using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// The only listener that plays feedback (02 §14): maps <see cref="GameEvents"/> to SFX, VFX, haptics,
    /// hit-stop and camera shake. Gameplay code never calls feedback directly.
    /// </summary>
    public sealed class FeedbackDirector : MonoBehaviour
    {
        private const float ChainWindowSeconds = 1.5f;

        private float _lastHitStop = -10f;
        private float _lastClearTime = -10f;
        private int _chainIndex;

        private static IAudioService Audio => Services.Audio;
        private static IHapticsService Haptics => Services.Haptics;

        private void OnEnable()
        {
            GameEvents.DrawStarted += OnDrawStarted;
            GameEvents.DrawThresholdReached += OnDrawThreshold;
            GameEvents.DrawCancelled += OnDrawCancelled;
            GameEvents.ArrowFired += OnArrowFired;
            GameEvents.ArrowImpact += OnArrowImpact;
            GameEvents.ObjectBroken += OnObjectBroken;
            GameEvents.PropTriggered += OnPropTriggered;
            GameEvents.ObjectiveCleared += OnObjectiveCleared;
            GameEvents.ProtectedLost += OnProtectedLost;
            GameEvents.LevelWon += OnLevelWon;
            GameEvents.LevelFailed += OnLevelFailed;
            GameEvents.LevelStarted += OnLevelStarted;
        }

        private void OnDisable()
        {
            GameEvents.DrawStarted -= OnDrawStarted;
            GameEvents.DrawThresholdReached -= OnDrawThreshold;
            GameEvents.DrawCancelled -= OnDrawCancelled;
            GameEvents.ArrowFired -= OnArrowFired;
            GameEvents.ArrowImpact -= OnArrowImpact;
            GameEvents.ObjectBroken -= OnObjectBroken;
            GameEvents.PropTriggered -= OnPropTriggered;
            GameEvents.ObjectiveCleared -= OnObjectiveCleared;
            GameEvents.ProtectedLost -= OnProtectedLost;
            GameEvents.LevelWon -= OnLevelWon;
            GameEvents.LevelFailed -= OnLevelFailed;
            GameEvents.LevelStarted -= OnLevelStarted;
        }

        private static bool ReducedMotion => Services.Save?.Data.settings.reducedMotion ?? false;

        private void OnLevelStarted(LevelSessionInfo info)
        {
            _chainIndex = 0;
            AppConfig config = AppConfig.Load();
            if (config != null && config.SoundLibrary != null) Audio.PlayMusic(config.SoundLibrary.Music);
            CameraShake.Disabled = ReducedMotion;
            if (VfxService.Instance != null)
                VfxService.Instance.ReducedParticles = Services.Save?.Data.settings.reducedParticles ?? false;
        }

        private void OnDrawStarted(AimState aim) => Audio.Play(SfxId.BowDraw, 0.6f);

        private void OnDrawThreshold(AimState aim)
        {
            Haptics.Play(HapticKind.Light);
            if (aim.IsFullDraw) Audio.Play(SfxId.BowFullDraw, 0.5f);
        }

        private void OnDrawCancelled(AimState aim) => Audio.Play(SfxId.DrawCancel, 0.5f);

        private void OnArrowFired(ArrowFiredInfo info)
        {
            Audio.Play(SfxId.BowRelease);
            Audio.Play(SfxId.ArrowWhoosh, 0.5f, Mathf.Lerp(0.85f, 1.15f, info.Power01));
            Haptics.Play(HapticKind.Light);
        }

        private void OnArrowImpact(ArrowImpactInfo info)
        {
            Color color = MaterialPalette.For(info.Material);
            switch (info.Outcome)
            {
                case ImpactKind.PassThrough:
                    return; // the prop raises its own event (rope cut, balloon pop)
                case ImpactKind.Embed:
                    Audio.Play(info.Material == MaterialKind.Straw ? SfxId.ImpactStraw : SfxId.EmbedWood);
                    break;
                default:
                    Audio.Play(ImpactSfx(info.Material));
                    break;
            }

            if (VfxService.Instance != null)
                VfxService.Instance.Chips(info.Point, color, info.IsMeaningful ? 7 : 4, 2.2f, 0.09f, 0.35f);

            if (!info.IsMeaningful) return;
            Haptics.Play(HapticKind.Medium);
            TryHitStop(info.Speed);
        }

        private void OnObjectBroken(BreakInfo info)
        {
            if (info.Cause == DamageSource.KillZone)
            {
                Audio.Play(SfxId.Splash, 0.8f);
                if (VfxService.Instance != null)
                    VfxService.Instance.Puff(info.Position, MaterialPalette.Water, 8, 2.5f, 0.45f, 0.6f);
                return;
            }

            Audio.Play(BreakSfx(info.Material), Mathf.Clamp(0.6f + info.Size * 0.3f, 0.6f, 1f));
            if (VfxService.Instance == null) return;
            Color color = MaterialPalette.For(info.Material);
            int count = Mathf.Clamp(Mathf.RoundToInt(8 + info.Size * 6f), 8, 18);
            VfxService.Instance.Chips(info.Position, color, count, 3.2f, 0.16f, 0.6f);
            VfxService.Instance.Puff(info.Position, Color.Lerp(color, Color.white, 0.5f), 4, 1.2f, 0.6f, 0.5f);
            if (info.Size > 0.8f) CameraShake.Request(0.04f, 0.15f);
        }

        private void OnPropTriggered(PropTriggerInfo info)
        {
            if (info.Kind == PropTriggerKind.RopeCut)
            {
                Audio.Play(SfxId.RopeCut);
                Haptics.Play(HapticKind.Medium);
                if (VfxService.Instance != null)
                    VfxService.Instance.Chips(info.Position, MaterialPalette.Helpful, 6, 1.8f, 0.07f, 0.4f);
            }
        }

        private void OnObjectiveCleared(ObjectiveInfo info)
        {
            float now = Time.unscaledTime;
            _chainIndex = now - _lastClearTime <= ChainWindowSeconds ? _chainIndex + 1 : 0;
            _lastClearTime = now;

            Audio.Play(SfxId.BreakCrest);
            Audio.Play(SfxId.ObjectiveCleared, 0.8f, 1f + 0.12f * Mathf.Min(_chainIndex, 5));
            if (_chainIndex > 0) Audio.Play(SfxId.ChainHit, 0.7f, 1f + 0.1f * _chainIndex);

            if (VfxService.Instance != null)
            {
                VfxService.Instance.Chips(info.Position, MaterialPalette.Required, 14, 3.6f, 0.14f, 0.7f);
                VfxService.Instance.Chips(info.Position, MaterialPalette.Helpful, 10, 4.5f, 0.08f, 0.8f);
            }
            Haptics.Play(HapticKind.Medium);
        }

        private void OnProtectedLost(ProtectedInfo info)
        {
            Audio.Play(SfxId.BreakVase);
            Audio.Play(SfxId.ProtectedAlarm, 0.8f);
            Haptics.Play(HapticKind.Heavy);
            CameraShake.Request(0.06f, 0.2f);
            if (VfxService.Instance != null)
                VfxService.Instance.Chips(info.Position, MaterialPalette.Protected, 16, 3f, 0.13f, 0.7f);
        }

        private void OnLevelWon(LevelResultInfo info)
        {
            Audio.Play(SfxId.WinSting);
            Haptics.Play(HapticKind.Success);
        }

        private void OnLevelFailed(LevelResultInfo info)
        {
            Audio.Play(SfxId.FailSting);
            Haptics.Play(HapticKind.Failure);
        }

        private void TryHitStop(float speed)
        {
            AppConfig config = AppConfig.Load();
            GameplayTuning tuning = config != null ? config.Tuning : null;
            float cooldown = tuning != null ? tuning.HitStopCooldown : 0.3f;
            float scale = tuning != null ? tuning.HitStopTimeScale : 0.05f;
            float now = Time.unscaledTime;
            if (now - _lastHitStop < cooldown) return;
            _lastHitStop = now;

            float duration = Mathf.Lerp(GameConstants.HitStopMinSeconds, GameConstants.HitStopMaxSeconds, Mathf.InverseLerp(5f, 13f, speed));
            if (ReducedMotion) duration = Mathf.Min(duration, GameConstants.HitStopMinSeconds); // D-057
            TimeScaleController.RequestHitStop(duration, scale);
        }

        private static SfxId ImpactSfx(MaterialKind kind)
        {
            switch (kind)
            {
                case MaterialKind.Straw: return SfxId.ImpactStraw;
                case MaterialKind.Timber: return SfxId.ImpactTimber;
                case MaterialKind.Stone: return SfxId.ImpactStone;
                case MaterialKind.Ice: return SfxId.ImpactIce;
                case MaterialKind.Metal: return SfxId.ImpactMetal;
                default: return SfxId.ImpactEarth;
            }
        }

        private static SfxId BreakSfx(MaterialKind kind)
        {
            switch (kind)
            {
                case MaterialKind.Straw: return SfxId.BreakStraw;
                case MaterialKind.Stone: return SfxId.BreakStone;
                case MaterialKind.Ice: return SfxId.BreakIce;
                default: return SfxId.BreakTimber;
            }
        }
    }
}
