# Skill: Audio, VFX and Haptics Integration

**Primary roles:** ART (owner), PLAT (haptics native bridge), CORE (hit-stop), UI (UI sounds)

## Purpose
Wire feedback (SFX, music, VFX, hit-stop, camera micro-shake, haptics) to gameplay **through `GameEvents` only**, so it feels substantial (mvp §11) without hiding outcomes or harming performance:
- 40–70 ms hit-stop on meaningful direct impacts;
- material-specific sounds;
- escalating percussion on chain reactions;
- debris that clears cleanly;
- haptics: light on the draw threshold, medium on impact, a success pattern on clear (mvp §8);
- particles only after the key outcome is readable (mvp §4).

## When to invoke
- M3 (VS feedback basics), M4 (graybox feedback hooks for new props/arrows incl. the Split pre-split pulse `VFX_Arrow_SplitPulse`, D-088), M6 (full art/audio pass for all three worlds), M8 (polish + reduced-particles tier).
- Whenever a new `GameEvents` payload kind, material, prop or arrow is added.

**Do NOT invoke** for import settings (use [`asset-pipeline-and-imports.md`](asset-pipeline-and-imports.md)), or to add gameplay logic. Feedback code never changes outcomes.

## Inputs
- The `GameEvents` list (01 §10.2) and payload structs.
- The SFX/VFX/haptics plans per material/object in [`05_ART_AUDIO_UX.md`](../planning/05_ART_AUDIO_UX.md).
- `MaterialProfile` references (each `MP_*` points to its `SoundEvent`/`VfxEvent` set via `SoundLibrary`).
- Settings flags: music, SFX, haptics, reduced particles, reduced motion.

## Step-by-step workflow
1. **Data first.** Create `SoundEvent` SOs (`SE_<Category>_<Name>`) in `ScriptableObjects/Audio/`:
   - fields: clips[], volume, pitch range, cooldown (ms), max simultaneous voices, priority, mixer group.
   Create `VfxEvent` SOs (`VfxEvent_<Name>`) in `ScriptableObjects/VFX/`:
   - fields: prefab (`VFX_*`), pool size, lifetime, reduced-particles variant, min interval.
2. **Map events → feedback** in `FeedbackDirector` (`Scripts/Runtime/Feedback/`), which subscribes in `OnEnable`, unsubscribes in `OnDisable`, and also has a `StaticReset` for any static caches:
   - `DrawThresholdReached` → `HapticKind.Light`, `SE_Bow_DrawTick`, bow glow (BowView).
   - `ArrowFired` → `SE_Bow_Release` + `SE_Arrow_Whistle` (pitch follows speed).
   - `ArrowImpact` → material SFX from `SoundLibrary[materialKind]` (wood crack, stone thunk, ice chime, rope twang, metal ping); impact VFX; `HapticKind.Medium` if `isMeaningful`; `HitStop.Request(0.04–0.07 s)` if `isMeaningful` and direct (rate-limited 1 per 0.3 s).
   - `ObjectBroken` → break SFX/VFX per material; the debris pool is handled by PHYS.
   - `PropTriggered` → per-kind SFX/VFX (rope snap, balloon pop, barrel blast with a short camera micro-shake, oil ignite, portal whoosh, spring boing).
   - `ObjectiveCleared` → crest-break sting; chain counter → escalating percussion layer (index = clears within a 1.5 s window).
   - `LevelWon` → success sting + `HapticKind.Success`; `LevelFailed` → soft fail sting + `HapticKind.Failure`; `ProtectedLost` → distinct alarm cue + slow-mo is owned by CORE.
3. **Hit-stop** (`HitStop` in Feedback decides; `TimeScaleController` in Core is the only writer of `Time.timeScale`, D-061):
   - requests time scale 0.05 for the real-time duration (40–70 ms);
   - restores with unscaled time;
   - never stacks; rate-limited to 1 per 0.3 s;
   - under Reduced Motion: kept but capped at 40 ms (D-057).
   Sim time is unaffected in outcome terms (physics just pauses), so determinism holds.
4. **Readability rule:**
   - impact VFX ≤ 0.4 s, alpha falloff, never larger than 1.5 m;
   - explosions spawn smoke **after** the 0.15 s flash;
   - no particle overlaps a remaining objective for > 0.3 s (QA check via screenshots);
   - Reduced Particles → the `reducedVariant` (≈ 50%) and no smoke.
5. **Audio mix:**
   - AudioMixer groups Master/Music/SFX/UI with snapshots `Gameplay`, `Paused`, `Results`;
   - duck music −6 dB on big chain reactions;
   - 24-voice pool with priority stealing (01 §13);
   - per-event cooldowns so 10 simultaneous crate breaks don't produce 10 identical clips (cap 3 voices per event, pitch variance ±8% via `CosmeticRandom`).
6. **Music:** `MusicPlayer` per world (`MUS_<World>_Loop`); crossfade 1 s between Home/Map/Gameplay; the results sting ducks the music. Respect the Music toggle instantly.
7. **Haptics** (PLAT owns the native code):
   - `IHapticsService` → `Plugins/iOS/ABHaptics.mm` (UIImpactFeedbackGenerator light/medium/heavy, UINotificationFeedbackGenerator success/failure; call `prepare()` on draw start);
   - Android `VibrationEffect` via `AndroidJavaObject` (`EFFECT_TICK`/`EFFECT_CLICK` on API 29+, `createOneShot` fallback; `VIBRATE` permission);
   - editor = log only;
   - rate-limit 1 per 50 ms; settings toggle respected; every haptic has an audio/visual twin.
8. **Verify in the editor:** `refresh_unity` → `read_console` → `manage_editor` play VS levels → fire at each material → confirm SFX/VFX/hit-stop → profile with `manage_profiler` (`AudioSource` count, particle count, GC = 0).
9. **Verify on device:** haptics on an iPhone (Taptic) and two Androids (different vibrators); volume levels on the phone speaker (a mix that works on laptop speakers is often too bassy); Low tier particle cost.

## Output artefacts
- `ScriptableObjects/Audio/SE_*.asset`, `ScriptableObjects/Audio/SoundLibrary.asset`, `ScriptableObjects/VFX/*.asset`
- `Prefabs/VFX/VFX_*.prefab`, `Audio/Mixers/ArrowBuster.mixer`
- `Scripts/Runtime/Feedback/*.cs`, `Plugins/iOS/ABHaptics.mm`
- `docs/qa/test-runs/YYYY-MM-DD_feedback-pass.md` (event → feedback coverage table, device notes)

## Quality checklist
- [ ] Every `GameEvents` kind and every material has a mapped SFX (placeholder allowed, marked) and VFX where specified.
- [ ] Hit-stop only on meaningful direct impacts; rate-limited; off under Reduced Motion.
- [ ] No VFX hides a remaining objective; smoke delayed after the flash.
- [ ] Voice caps and cooldowns prevent clipping and spam; the mix is checked on a phone speaker.
- [ ] Haptics: light/medium/success mapped; toggle works; rate-limited; tested on iOS + 2 Androids.
- [ ] Reduced Particles variant exists for every VFX.
- [ ] Zero GC from feedback in gameplay; pools prewarmed.
- [ ] Feedback code has no reference to gameplay internals except event payloads.

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| Sounds play twice after the second editor Play | Listener subscribed twice (domain reload disabled, static subscription) | Subscribe in `OnEnable`/`OnDisable`; `StaticReset` |
| Game freezes after a hit | Hit-stop restore used scaled time | Restore with an unscaled timer; guard against stacking |
| Haptics silent on Android | Missing `VIBRATE` permission, or the device's haptic setting is off | Manifest permission; fall back to `createOneShot` |
| iOS haptic lag on the first impact | Generator not prepared | `prepare()` on `DrawStarted` |
| Explosion VFX covers the target, so the player can't see the result | VFX too big or long | Size/lifetime caps; delay smoke |
| Audio clipping during collapses | Too many concurrent voices | Per-event voice caps + mixer limiter |
| First-play stutter | VFX/audio not preloaded | Prewarm pools; preload clips on level load |

## Example task prompt for a sub-agent
```text
Agent: ab-art-vfx-audio
Skill: docs/skills/audio-vfx-haptics-integration.md
Ticket: AB-032 (M3, with AB-030/031/033/034) Feedback basics for the vertical slice: FeedbackDirector, AudioService, VfxService, HitStop + timber/target/vase/rope cues
Inputs: GameEvents (01 §10.2); docs/planning/05_ART_AUDIO_UX.md SFX plan; placeholder SFX in Audio/SFX/_Placeholder
Coordinate with: ab-mobile-platform (HapticsService native bridge, same milestone) — consume only IHapticsService
Deliverables: SE_* + SoundLibrary + VFX_* (Break_Timber, Impact_Generic, Break_Crest, Rope_Snap), Audio/Mixers/ArrowBuster.mixer,
docs/qa/test-runs/2026-10-xx_feedback-pass.md coverage table.
Boundaries: listen to GameEvents only; no edits to gameplay scripts (request new event fields from ab-tech-architect).
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
