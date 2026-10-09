# 02 — Gameplay Systems

> Owner: Core Gameplay Engineer (`CORE`), with sections owned by `PHYS` (settle), `UI` (tutorial view) and `ART` (feedback execution) as marked.
> Status: Draft v1 — 2026-10-09. Names follow [`01_TECHNICAL_ARCHITECTURE.md`](01_TECHNICAL_ARCHITECTURE.md). Decisions are cited as D-0xx ([`10_DECISION_LOG.md`](10_DECISION_LOG.md)) and design sections as §N ([`/mvp.md`](../../mvp.md)).
> Physics, materials and object behaviours are specified in [`03_PHYSICS_AND_OBJECTS.md`](03_PHYSICS_AND_OBJECTS.md). Level data is in [`04_LEVEL_PIPELINE.md`](04_LEVEL_PIPELINE.md). Test procedures are in [`07_QA_PERFORMANCE_RELEASE.md`](07_QA_PERFORMANCE_RELEASE.md).

---

## 0. Conventions used in this document

- **World units = metres.** Play plane is z = `GameConstants.PlayPlaneZ` (0). The play area is 10 m wide at z = 0, with a design height of 17.8 m (9:16). The origin is at bottom-centre. The bow pivot sits at about (0, 1.5, 0). The structure zone spans y ≈ 4–15. The HUD band takes the top ~9% of the safe area.
- **Time:** gameplay timers use `LevelClock` (Core), which counts **fixed-step time since level start**. It advances only in `FixedUpdate`, so hit-stop and pause never change outcomes. UI timers use unscaled time.
- **Tuning** lives in `GameplayTuning` (one SO, `ScriptableObjects/Config/GameplayTuning.asset`, CORE-owned) and `ArrowDefinition` (`AD_<Type>`). `GameConstants` holds only global product targets.
- **"Spec" vs "Snappy"** (D-036): two `GameplayTuning` + `AD_Oak` presets exist until the M1 feel gate. Values below list Spec first, then Snappy in brackets.

### 0.1 `GameplayTuning` — full field list (defaults)

| Field | Default | Used by | Notes |
|---|---|---|---|
| `aimZoneScreenFraction` | 0.55 | DrawModel | Bottom 55% of the safe area starts a draw (D-018) |
| `fullDrawScreenFraction` | 0.22 | DrawModel | Drag length for power = 1, as a fraction of screen height |
| `dragDeadzoneScreenFraction` | 0.015 | DrawModel | Below this the angle is not updated (prevents jitter on touch-down) |
| `minFirePower` | 0.15 | DrawModel | Releasing below this cancels the draw (no arrow spent) |
| `minAimAngleDeg` / `maxAimAngleDeg` | 8 / 172 | DrawModel | Firing cone, measured from +X |
| `powerCurveExponent` | 1.0 | DrawModel | 1 = linear. Tune at the M1 gate (0.8–1.3 range) |
| `renockCooldown` | 0.35 s | GameplayController | D-083 (supersedes D-016) |
| `drawHapticPowerThreshold` | = `minFirePower` | BowController | Light haptic when the draw becomes fireable |
| `fullDrawHapticPower` | 0.98 | BowController | Second Light tick + string glow (§11) |
| `arrowGravity` | 5.0 m/s² [12.0] | BallisticSolver | Arrow gravity, decoupled from world gravity (−9.81 for bodies) |
| `previewBaseSeconds` | 1.6 s [1.0] | TrajectoryPreview | × `LevelData.trajectoryPreviewScale` |
| `previewDotSpacingSeconds` | 0.05 s | TrajectoryPreview | |
| `previewMaxSteps` | 150 | TrajectoryPreview | Safety cap (2.5 s at 60 Hz) |
| `arrowMaxLifetime` | 6 s | ArrowProjectile | Then resolved as OutOfBounds |
| `playBoundsMargin` | 2 m | PlayBounds | Outside the camera frame |
| `nockOffset` | 0.45 m | BowController | Arrow spawn distance from the pivot along the aim direction |
| `sweepRadius` | 0.06 m | ArrowProjectile, TrajectoryPreview | |
| `meaningfulImpulseThreshold` | 3.0 Ns | ArrowImpactResolver | For hit-stop and Medium haptic |
| *(hit-stop min/max)* | — | HitStop | **Not duplicated here** — read directly from `GameConstants.HitStopMinSeconds` / `HitStopMaxSeconds` (0.040 / 0.070), the single source |
| `hitStopTimeScale` | 0.05 | HitStop | |
| `hitStopCooldown` | 0.30 s | HitStop | |
| `protectedFocusSlowMo` | 0.3× for 0.5 s (unscaled) | GameplayController | D-038 |
| `calmLinearSpeed` | 0.05 m/s | SettleMonitor | D-015 |
| `calmAngularSpeedDeg` | 5 °/s | SettleMonitor | |
| `lodgedVibrateAmplitudeDeg` / `Hz` | 6° / 18 Hz | ArrowProjectile | Over `GameConstants.LodgedArrowVibrateSeconds` (0.2 s), exponential decay |
| `spentArrowFadeSeconds` | 1.5 s | ArrowProjectile | |
| `introDurationSeconds` | 0.6 s | GameplayController | Level name + objective icons pop |
| `softLockToastSeconds` | 1.6 s | HudView | |
| `outOfArrowsToastSeconds` | 0.6 s | GameplayController | Before the Fail panel |

New `GameConstants` (D-015): `OutOfArrowsMaxWaitSeconds = 5f`, `WinSettleMaxSeconds = 3f`. Existing: `WinSettleSeconds = 0.75f`, `SoftLockSettleSeconds = 2f`, `MaxActiveArrows = 8`, `MaxDebrisFragments = 40`.

---

## 1. Game state machine

**Owner:** CORE · **Files:** `Gameplay/GameplayController.cs`, `Gameplay/GameplayState.cs`, `Gameplay/LevelSession.cs` · **Tickets:** AB-019 (core), AB-016 (loading), AB-017 (quiver/stars)

### 1.1 States

```csharp
public enum GameplayState { Loading, Intro, Ready, Drawing, Cooldown, AwaitingResolution, WinPending, Won, Failed, Paused }
```

| State | Input | Physics | Meaning |
|---|---|---|---|
| `Loading` | off | — | `LevelLoader.Load` running (≤ 300 ms target) |
| `Intro` | off (tap to skip) | asleep | 0.6 s level intro, plus any arrow-reveal card / blocking tutorial |
| `Ready` | draw allowed | live | Arrows remain, no draw in progress |
| `Drawing` | draw in progress | live | `AimState` is updating and the preview is visible |
| `Cooldown` | off | live | `renockCooldown` after a release (0.35 s). The next arrow may then be drawn while physics is still resolving (D-083) |
| `AwaitingResolution` | off | live | Quiver empty (or the last arrow is still flying). Waiting for win or fail (D-015) |
| `WinPending` | **off — firing disabled** (D-083) | live | All required objectives cleared; waiting for 0.75 s calm (cap 3 s) |
| `Won` | results UI only — **no firing** (D-083) | live (frozen visually after 1 s) | Terminal |
| `Failed` | results UI only — **no firing** (D-083); also off during the 0.6 s "Out of arrows" toast | live | Terminal |
| `Paused` | pause UI | `timeScale = 0` | Overlay; stores `_stateBeforePause` |

### 1.2 Diagram

```
                 Load(level)
   ┌──────────┐  done   ┌───────┐ 0.6 s / tap  ┌────────┐ press in aim zone ┌─────────┐
   │ Loading  ├────────►│ Intro ├─────────────►│ Ready  ├──────────────────►│ Drawing │
   └──────────┘         └───────┘              └──┬──▲──┘◄──────────────────┴────┬────┘
                                                  │  │     release < minFirePower │
                                                  │  │     (cancel, no arrow)     │ release ≥ minFirePower
                                                  │  │                            │ [Quiver.TryConsume]
                                                  │  │ cooldown done ∧ remaining>0│
                                                  │  └──────────────┐   ┌─────────▼──┐
                                                  │                 └───┤  Cooldown  │
                                                  │                     └─────┬──────┘
                                                  │ remaining == 0            │ cooldown done ∧ remaining == 0
                                                  ▼                           ▼
                                     ┌────────────────────────────────────────────┐
                                     │            AwaitingResolution              │
                                     │ guard FAIL: allArrowsResolved ∧             │
                                     │  (calm ≥ 2.0 s ∨ sinceResolved ≥ 5.0 s)     │
                                     └──────────────┬─────────────────────────────┘
                                                    │ fail guard
  ANY of {Ready, Drawing, Cooldown,                 ▼
   AwaitingResolution}                     ┌─────────────────┐
   ── ObjectiveTracker.AllCleared ──►      │ "Out of arrows" │──0.6 s──► Failed(OutOfArrows)
   ┌─────────────┐                          └─────────────────┘
   │ WinPending  │── calm ≥ 0.75 s ∨ pendingTime ≥ 3.0 s ──► Won
   └─────────────┘
  ANY non-terminal state ── ProtectedLost ──► [0.5 s slow-mo focus] ──► Failed(ProtectedLost)   (priority over Won)
  ANY non-terminal state ── Pause() ──► Paused ── Resume() ──► previous state
  Won/Failed ── Retry() ──► Loading (same level, attempt+1) · Next() ──► Loading (next level) · Home() ──► SceneFlow
```

### 1.3 Per-FixedUpdate evaluation (pseudocode)

```text
FixedUpdate():
  if state ∈ {Loading, Intro, Won, Failed, Paused}: return
  levelClock.Tick(dt)
  settle.Sample(arrowRegistry.FlyingCount > 0)      // updates calmSeconds (see §8)

  if protectedTracker.LostThisStep:                 // highest priority
      EnterFailed(ProtectedLost); return
  if objectiveTracker.AllCleared and state != WinPending:
      CancelDrawIfAny(refundArrow:false)            // draw not consumed yet → nothing spent
      Enter(WinPending); winPendingStart = clock.Now
  if state == WinPending:
      if settle.CalmSeconds ≥ WinSettleSeconds or clock.Now - winPendingStart ≥ WinSettleMaxSeconds:
          EnterWon()
      return
  if state == AwaitingResolution and arrowRegistry.FlyingCount == 0:
      sinceResolved = clock.Now - lastArrowResolvedAt
      if settle.CalmSeconds ≥ SoftLockSettleSeconds or sinceResolved ≥ OutOfArrowsMaxWaitSeconds:
          ShowOutOfArrowsToast(); EnterFailed(OutOfArrows) after 0.6 s (unscaled)
  if state ∈ {Ready, Cooldown} and quiver.Remaining > 0 and !softLockToastShownForShot
     and arrowRegistry.FlyingCount == 0 and settle.CalmSeconds ≥ SoftLockSettleSeconds:
      GameEvents.SoftLockPrompt(quiver.Remaining); softLockToastShownForShot = true
```

### 1.4 `LevelSession` (runtime record, pure C#)

Fields: `LevelData Level`, `int Attempt`, `int ArrowsStart`, `int ArrowsUsed`, `bool BonusArrowUsed`, `bool BonusOfferedThisAttempt`, `bool BullseyeHit`, `float StartRealtime`, `FailReason? FailReason`, `int ShotsFired`, `List<float> ShotTimes`. `LevelResultInfo` and `LevelSessionInfo` payloads are built from it.

### 1.5 Edge cases

| Case | Rule |
|---|---|
| Objectives clear while the player is drawing | The draw is cancelled; the arrow is **not** consumed. → WinPending |
| Objectives clear from the last arrow while it is still flying | WinPending; the flying arrow continues visually and is ignored by the guards |
| Protected lost during WinPending | Failed(ProtectedLost) — the vase broke before the win was confirmed (D-038) |
| Protected lost and objectives cleared on the same step | Failed (protected has priority) |
| App backgrounded | `AppLifecycle` → `Pause()`; a draw in progress is cancelled |
| Retry pressed during resolution | Allowed from the HUD at any time → `LevelRestarted`, attempt + 1 |
| Rewarded bonus arrow accepted on Fail | `Failed → Ready` with `quiver.AddBonus(level.bonusArrowType)` and `BonusArrowUsed = true`. The layout is **not** reloaded: the world state persists. Only allowed if the fail reason is OutOfArrows (never ProtectedLost); `AdPolicy` decides (see `06`). |
| Fire attempt in WinPending / Won / Failed / fail-toast | Rejected by `GameplayController.CanDraw()` (state guard, D-083). `BowInputReader` samples are ignored; no arrow is consumed and no `DrawStarted` is raised |
| Level has 0 objectives | Validator error. Runtime: log an error, then Won after Intro (dev only) |

### 1.6 Tests
- EditMode `GameplayStateMachineTests` (state logic extracted to the pure `GameplayStateMachine` helper inside `GameplayController.cs`, driven by a fake clock and fake trackers): win after 0.75 s calm; win cap 3 s with perpetual jitter; out-of-arrows fail at 2 s calm; fail cap 5 s; protected priority over win; draw cancelled on objective clear; bonus arrow resumes to Ready; pause stores and restores; **`CanDraw()` false in WinPending/Won/Failed and during the fail toast, true again 0.35 s after a release while bodies are still moving (D-083)**.
- PlayMode `GameFlowSmokeTests`: Load → fire intended shots → Won → Next → Loading of the next level; Retry restores the initial layout hash.

### 1.7 Acceptance criteria
- AC1: All 10 states are reachable in `GameFlowSmokeTests`. No illegal transition is logged in 100 random-input runs (monkey test, seeded).
- AC2: Win is declared ≤ 0.80 s after the world is calm and ≤ 3.05 s after objectives clear, in every case (fake-clock tests).
- AC3: Out-of-arrows fail is shown ≤ 5.7 s after the last arrow resolves, including the toast.
- AC4: Retry → Ready takes ≤ 1.0 s wall-clock on the Mid tier (p95 over 20 retries; `RestartPerfTests` + device check).
- AC5 (D-083): in a scripted collapse, a second arrow can be released 0.35 s (±1 fixed step) after the first while ≥ 1 body is above the calm threshold; 0 arrows are consumed by input received in WinPending/Won/Failed (fake-clock test, 100 seeded runs).

---

## 2. Bow input and draw mechanics

**Owner:** CORE · **Files:** `Bow/BowInputReader.cs`, `Bow/DrawModel.cs`, `Bow/AimState.cs`, `Bow/BowController.cs`, `Bow/BowView.cs` · **Tickets:** AB-007 (reader + model), AB-009 (firing integration) · **Decisions:** D-007, D-018

### 2.1 Pipeline

```
Pointer.current (Update) → BowInputReader → PointerSample{phase, screenPos, pointerId, unscaledTime}
   → DrawModel (pure) → AimState{angleDeg, power01, isFireable, isFullDraw, dragPixels}
   → BowController (state gate, events, haptics, fire) → BowView (rotation, string pull, glow)
                                                       → TrajectoryPreview (while Drawing)
```

- `BowInputReader`: polls `Pointer.current.press.isPressed` and `.position`. It emits `Began` when a press starts, `Moved` while held, and `Ended` on release. A press **beginning over UI** (`EventSystem.current.IsPointerOverGameObject(pointerId)`) is ignored until release. Secondary touches are ignored: the reader locks to the first `Touchscreen.current.primaryTouch`. If `Pointer.current` is null (no device), it emits nothing.
- **Aim zone:** `pressPos.y ≤ safeArea.yMin + aimZoneScreenFraction × safeArea.height`. A press outside it is ignored (so a top-of-screen tap never starts a draw).

### 2.2 DrawModel mapping (pure; EditMode-tested)

```text
Begin(pressPos, screen):            origin = pressPos; lastAngle = 90°; active = true
Update(curPos):
  pull  = origin - curPos                         // pull back → shoot forward (slingshot)
  len   = |pull|
  if len ≥ dragDeadzoneScreenFraction × screen.height:
      angle = atan2(pull.y, pull.x) in degrees    // 0° = +X (right), 90° = straight up
      if angle < 0: angle = (pull.x ≥ 0) ? minAimAngleDeg : maxAimAngleDeg   // dragging upward clamps to cone edge
      lastAngle = clamp(angle, minAimAngleDeg, maxAimAngleDeg)
  raw   = clamp01(len / (fullDrawScreenFraction × screen.height))
  power = raw ^ powerCurveExponent
  return AimState(lastAngle, power, isFireable: power ≥ minFirePower, isFullDraw: power ≥ fullDrawHapticPower, len)
End(): active = false; return last AimState
LaunchVelocity(aim, def) = dir(aim.angleDeg) × lerp(def.minSpeed, def.maxSpeed, aim.power01)   // dir in XY plane
```

Notes:
- Screen space is used (not world space) so the gesture feels identical on every aspect ratio and camera tilt.
- No smoothing filter, because aim must be exactly reproducible from the release sample. Jitter is handled by the deadzone only.
- `AimState` is a `readonly struct` (no GC).

### 2.3 BowController responsibilities
1. Gate draws: only in `Ready`; only if `ArrowRegistry` can spawn (the cap evicts the oldest embedded arrow).
2. Raise `DrawStarted`, `DrawThresholdReached` (once per draw when it first becomes fireable, and again on full draw), `DrawCancelled`.
3. On `Ended`: if `isFireable` → `quiver.TryConsume(out type)` → `ArrowSpawner.Fire(type, nockPos, LaunchVelocity)` → raise `ArrowFired` → go to `Cooldown`. Otherwise cancel.
4. Show the **nocked arrow** of the next quiver type on the bow while `Ready`/`Drawing` (visual only).

`BowView`: rotates the bow root to `angleDeg − 90°` around Z (max visual rotation ±82°), pulls the string back `power × 0.35 m`, lights the string emissive at full draw, and plays a release snap animation (0.12 s). Purely visual — no gameplay data flows back.

### 2.4 Edge cases
| Case | Rule |
|---|---|
| Drag back to the origin, then release | power < minFirePower → cancel (the intuitive "undo") |
| A finger slides into the HUD buttons while drawing | Continues drawing (UI is ignored after the press began in the aim zone) |
| Release exactly on the threshold | `≥` fires. The same rule is shared by preview and fire. |
| Draw started, then objectives cleared | Cancelled by the controller (§1.5) |
| Mouse in editor | Same path via `Pointer.current`. A right-click cancel is dev-only (`AB_DEV`). |
| Orientation/resolution change mid-draw | Cancel the draw, then recompute safe area and camera framing |

### 2.5 Tests and acceptance
- EditMode `DrawModelTests`: pull straight down → 90°; pull down-left → aims up-right (≈ 45° for an equal-component pull); upward drag clamps to 8°/172°; power linear at exponent 1; deadzone holds the previous angle; below `minFirePower` → not fireable; screen-size independence (the same fractional drag on 1080×1920 and 1170×2532 gives the same `AimState`).
- PlayMode `BowInputTests` (InputTestFixture with a simulated `Touchscreen`): press over a UI button doesn't start a draw; a second touch is ignored; cancel spends no arrow. Requires the `Unity.InputSystem.TestFramework` reference in `ArrowBuster.Tests.PlayMode` (created in AB-002).
- **AC:** 15-second first-shot test (§10): ≥ 9/10 first-time testers fire within 15 s of Gameplay appearing, with no text (L1 ghost hand only). Input latency press→preview visible ≤ 1 frame + render (≤ 33 ms on Mid, measured with a high-speed camera or frame capture once in M9).

---

## 3. Aim assist and trajectory display

**Owner:** CORE · **Files:** `Bow/TrajectoryPreview.cs` (+ pooled dot prefab `Prefabs/Bow/PreviewDot.prefab`, impact ring `PreviewImpactRing.prefab`) · **Ticket:** AB-008 · **Decisions:** D-085 (supersedes D-017, D-040), D-088

### 3.1 Policy
- **No aim magnetism, snapping or auto-aim.** The only aim assist is an *honest* preview (fair-physics pillar).
- The preview is visible **only while Drawing** (§3), and only when `isFireable` (below the threshold it shows a faint 3-dot stub).
- Length = `previewBaseSeconds × LevelData.trajectoryPreviewScale`. L1–15 must use scale 1.0 (validator V-14, error). The scale is never below the D-073 floor of 0.4, and the remote floor `levels.preview_scale_min` is applied on top — the preview is never removed.
- The preview simulates exactly what the arrow will do (D-085): gravity, **`WindField` acceleration**, **portal exit paths** (entry ring → exit ring → continued arc), the **first bounce** and the Split split-point + child stubs. **Never hide a mechanic that changes the arrow's path**; difficulty comes only from a shorter preview, object placement, timing and limited arrows.
- **First bounce (D-085):** if `ArrowImpactResolver.Predict` returns `Ricochet` at a hit (the Bounce arrow on metal always; any arrow at a shallow-angle metal ricochet per §6.3), the preview draws **one** reflected segment, then stops at the next blocking hit (impact ring) or the length budget. A second ricochet is never previewed. Any other blocking hit ends the preview with an impact ring. Ropes and balloons are drawn as pass-through (with a small "snip"/"pop" tick mark). Moving obstacles are sampled at their **current** pose.

### 3.2 Algorithm

```text
Rebuild(aim, arrowDef):
  state = BallisticSolver.Launch(nockPos(aim), LaunchVelocity(aim, arrowDef))
  steps = min(ceil(length / fixedDt), previewMaxSteps)
  for i in 0..steps:
     next = BallisticSolver.Step(state, fixedDt, env, arrowDef)
     hit  = ArrowSweep.Cast(state.position, next.position, sweepRadius, ArrowCastMask)   // the SAME helper the arrow uses
     if hit:
        o = ArrowImpactResolver.Predict(arrowDef, state.velocity, hit)   // pure, no side effects (uses IArrowHittable.Evaluate only, §6.2)
        // KillZone: the preview ends where the arc enters a KillZone (same KillZoneMask point check as the arrow, §5.1)
        if o.kind ∈ {PassThrough(rope/balloon)}: mark tick; continue from hit with o.velocityAfter
        elif o.kind == Teleport: emit portal markers; state = o.exitState; continue
        elif o.kind == Ricochet and ricochetsShown < 1: mark bounce; ricochetsShown += 1; state = o.stateAfter; continue   // D-085: first bounce for any arrow
        else: place impact ring at hit.point; break
     if arrowDef.behaviour == Split and state.time crosses splitTime: emit split marker (glowing ring, D-088) + 3 child stubs (0.35 s each, no further splitting)
     place a dot every previewDotSpacingSeconds; dots fade alpha 1 → 0.25 along length
     state = next
```

- The preview is rebuilt every frame while drawing (≤ 150 sweeps ≈ 0.1–0.2 ms on Mid; budget 0.3 ms). Allocation-free buffers.
- Dots are world-space quads on the play plane (z = −0.05 so they render in front), using one material (SRP-batched). Pool: 40 dots + 4 rings + 6 markers.
- The first ricochet is previewed for **every** arrow type (D-085). Bounce stays valuable because only Bounce *guarantees* a ricochet at any θ ≤ 80° with 0.95 restitution; other arrows ricochet only at grazing angles (γ ≤ 25°) with heavy speed loss (§6.3), so their previewed bounce is short.

### 3.3 Acceptance and tests
- **Parity (blocking):** `ArrowPreviewParityTests` (EditMode, pure solver + mock environment) — for 20 seeded aims × each arrow type, with wind on/off, the preview points equal the flight positions within **1 mm** over 3 s, including **wind** (constant and gust-free fields), **portal exits** and the **first bounce for every arrow type that can ricochet** (Bounce on metal; Oak/Split child at γ ≤ 25° on metal), and the Split marker time (±1 fixed step). PlayMode variant against real colliders: the final impact point is within 2 cm.
- AC (D-085): in every W3 wind/portal/bounce level (L44–L60) the bot-recorded intended shot's first-impact point lies on the previewed path within 2 cm; no level relies on an un-previewed path change (validator + `LevelSolvabilityTests` log).
- AC: the preview never shows through a solid (non-rope, non-portal) collider (PlayMode `PreviewBlockingTests`).
- AC: the dot count/length matches the level scale (W1 L1–15 scale 1.0 verified by `LevelValidator`).

---

## 4. Ballistic projectile simulation

**Owner:** CORE · **Files:** `Arrows/BallisticSolver.cs` (static, pure), `Arrows/ArrowFlightState.cs` · **Ticket:** AB-006 · **Decision:** D-005

### 4.1 Data

```csharp
public struct ArrowFlightState { public Vector3 Position; public Vector3 Velocity; public float Time; public int RicochetsLeft; public bool HasSplit; public int TeleportsLeft; }
public interface IFlightEnvironment { Vector3 SampleWind(Vector3 position); }   // declared in Physics/ (shared contract, so Props never references Arrows); implemented by WindFieldSampler (Props); NullFlightEnvironment in tests
```

### 4.2 Step (semi-implicit Euler — the single source of truth for flight and preview)

```text
Launch(pos, vel): return { Position = pos with z = PlayPlaneZ, Velocity = vel with z = 0, Time = 0, RicochetsLeft = def.maxRicochets, TeleportsLeft = 3 }
Step(s, dt, env, def):
   a  = (0, -tuning.arrowGravity × def.gravityScale, 0) + env.SampleWind(s.Position) × def.windResponse
   v' = s.Velocity + a × dt
   p' = s.Position + v' × dt
   p'.z = PlayPlaneZ; v'.z = 0
   return s with Position = p', Velocity = v', Time = s.Time + dt
```

- `dt` is always `Time.fixedDeltaTime` (1/60). The preview **must** pass the same dt (never `Time.deltaTime`).
- Wind is sampled at the **start** of the step (a deterministic, constant field; `WindField` has no gusts — see `03` §8.12).
- No drag term in MVP (simpler preview, matches the spec). If the M1 feel test wants drag, add `def.linearDrag` inside `Step` only — parity is automatic.

### 4.3 Reference trajectories (Spec preset, Oak, no wind)

World-space targets; launch from the pivot (0, 1.5); `arrowGravity` 5 m/s²; values computed from the closed form (vx = Δx/T, vy = (Δy + ½gT²)/T).

| Target (world) | Launch (angle from +X, speed) | Flight time |
|---|---|---|
| (0.5, 6) near lob | 87°, 6.8 m/s | 1.5 s |
| (3, 10) typical | 80°, 9.4 m/s | 1.8 s |
| (4.5, 15) far top corner | 80°, 11.8 m/s | 2.3 s |

Snappy preset: the same targets at ≈ 0.9–1.3 s. These numbers are for sanity checks in `BallisticSolverTests`, not hard requirements; the M1 gate (AB-014) sets the final values.

### 4.4 Tests and acceptance
- EditMode `BallisticSolverTests`: free fall matches closed form within 0.5% over 2 s at dt = 1/60; z always 0; zero wind == `NullFlightEnvironment`; a wind of +X shifts the landing x monotonically; time accumulates exactly (n × dt).
- AC: across the full aim cone and power range (both presets), an Oak arrow reaches every point of the structure zone (y 4–15, |x| ≤ 4.5) — verified by `ReachabilityTests` (pure sweep of a 1° × 0.01-power aim grid; every 0.25 m grid cell of the zone is crossed by ≥ 1 trajectory).

---

## 5. Arrow lifecycle

**Owner:** CORE · **Files:** `Arrows/ArrowProjectile.cs`, `Arrows/ArrowSpawner.cs`, `Arrows/ArrowRegistry.cs`, `Arrows/ArrowDefinition.cs` · **Ticket:** AB-009

### 5.1 Lifecycle states

```
Pooled ──Fire()──► Flying ──hit: Embed──────────► Embedded ──(cap evicted)──► Decor ──(level reset)──► Pooled
                     │   ──hit: Deflect/Stop──────► Spent (dynamic, fades 1.5 s) ──► Pooled
                     │   ──hit: PassThrough/Ricochet/Teleport──► Flying (continues)
                     │   ──KillZone / PlayBounds / lifetime 6 s──► Resolved silently ──► Pooled
                     └── Split: at splitTime → spawns 3 children (Flying); parent → Pooled (visual hand-off)
Embedded ──host breaks──► Spent · Embedded ──host removed by KillZone──► Pooled
```

- **`ArrowResolved`** is raised exactly once per quiver arrow, when Flying ends for the arrow and **all** of its split children.
- **Flying:** `FixedUpdate` → `Step` → sweep (loop up to 4 sub-casts per step for pass-through outcomes, with remaining distance) → resolver → **kill-zone check**: `Physics.CheckSphere(position, sweepRadius, PhysicsLayers.KillZoneMask, QueryTriggerInteraction.Collide)`. A hit resolves the arrow as `KillZone` (the arrow cast mask deliberately excludes KillZone, so the sweep never treats water as a solid). The `PlayBounds` rectangle check runs in the same step. Visual transform: `Update` interpolates between the previous and current fixed states (`alpha = (Time.time − Time.fixedTime) / fixedDeltaTime`), with rotation `LookRotation(velocity, Vector3.back)`.
- **Embedded:** `SetParent(hitBody.transform, worldPositionStays: true)`, tip at hit point + 0.08 m penetration along velocity; colliders disabled (D-049); vibrate 0.2 s (6°, 18 Hz, decaying); `ArrowProjectile` stays enabled only to listen for the host `Breakable.Broken`.
- **Spent:** enable a small `Rigidbody` (mass 0.05, Arrow layer → collides with Environment only, `PlanarBody` constraints), set the velocity from the outcome, fade the material alpha over 1.5 s, then return to the pool. Not tracked by `SettleMonitor`.
- **Decor:** when `ArrowRegistry.ActiveCount == MaxActiveArrows (8)` and a new arrow fires, the **oldest Embedded** arrow becomes Decor: the script is disabled and the mesh stays parented. Decor objects are released on level reset. If no arrow is embedded, the oldest **Spent** arrow is recycled immediately. Flying arrows are never evicted (at most 1 + 3 split children fly at once in practice).

### 5.2 Pooling
- `ArrowSpawner` holds a `PrefabPool<ArrowProjectile>` per `ArrowType`. Prewarm: 8 Oak + 3 per special type present in `LevelData.quiver` (+9 for Split). The pools survive restarts (D-011); `LevelLoader` calls `ArrowSpawner.ReleaseAll()` first.
- **AC:** zero `Instantiate` calls during gameplay after the prewarm (Profiler marker check in `RestartPerfTests`).

---

## 6. Collision and impact resolution

**Owner:** CORE (resolver logic) + PHYS (material data, damage application) · **Files:** `Arrows/ArrowImpactResolver.cs`, `Arrows/ImpactOutcome.cs` · **Ticket:** AB-011 · **Decisions:** D-005, D-049

### 6.1 Inputs and outputs

```csharp
public readonly struct ImpactContext { ArrowDefinition Def; Vector3 Velocity; RaycastHit Hit; BodyKind Kind; MaterialProfile Material; bool IsFirstImpact; float FlightTime; }
public enum ImpactKind { Embed, Deflect, Stop, Ricochet, PassThrough, Shatter, Teleport, Ignore }
public readonly struct ImpactOutcome { ImpactKind Kind; Vector3 VelocityAfter; float Impulse; float Damage; bool Meaningful; PropTriggerKind Trigger; }
```

`BodyKind` comes from the `PhysicsBodyRegistry` collider → entry lookup (built on level load, no `GetComponent` at hit time): `Environment, Structure, Objective(kind), Protected(kind), Rope, Balloon, Portal, Prop(kind), Unknown`.

Definitions: incidence `θ = angle(−v, n)` (0° = head-on); grazing `γ = 90° − θ`; `speed = |v|`.

### 6.2 Rule priority (first match wins)

| # | Target | Outcome (Oak baseline) | Side effects |
|---|---|---|---|
| 1 | Portal trigger, front side (`dot(v, ring.up) < 0`, see `03` §8.15), `TeleportsLeft > 0` | **Teleport** | Exit state from `PortalPair.Map` · `PropTriggered(PortalEnter)` |
| 2 | Rope / chain | **PassThrough**, speed × 1.0 | `RopeCuttable.Cut()` (Fire also ignites the rope stub visuals) |
| 3 | Balloon | **PassThrough**, speed × 0.9 | `Balloon.Pop(i)` |
| 4 | Protected (any kind) | **Stop** → Spent (drops) | `ProtectedObject.NotifyArrowHit()` → `ProtectedLost(Hit)` |
| 5 | Objective CursedOrb / HangingLantern | **Shatter**, PassThrough speed × 0.7 | Objective cleared / Breakable.Break |
| 6 | Objective CrestTarget | **Stop** (arrow becomes Spent as the crest breaks) | Breaks; cleared |
| 7 | Bounce arrow vs Metal (`RicochetsLeft > 0`, θ ≤ 80°) | **Ricochet**, reflect × 0.95 | `PropTriggered`-free; SFX ping |
| 8 | Material rule (table 6.3) via `MaterialBody` | Embed / Deflect / Stop / Ricochet / Shatter | Impulse + damage |
| 9 | Environment without `MaterialBody` | Treated as `MP_Earth` (Embed, no impulse) | — |
| 10 | Unknown | Stop → Spent | Dev warning |

Objectives of kind SupplyCrate / TrainingDummy / BannerRope go through rows 2 and 8 (they are physical crates/dummies/ropes).

**Dispatch (dependency rule, `01` §10.3):** the resolver never references Props/Objectives types. The registry entry for the hit collider carries `BodyKind` plus an optional `IArrowHittable` (declared in `Physics/`). The resolver calls `Evaluate(in ArrowHitInfo)` (pure; also used by the preview) to get the reaction (pass-through factor, teleport exit state, stop, clear), then `Apply(in ArrowHitInfo)` for the side effects (cut, pop, notify protected, flag bullseye). Implemented by `RopeCuttable`, `Balloon`, `PortalRing`, `ProtectedObject`, `Objective` and `BullseyeMarker` (`SpringPlate` is cut from the MVP, D-086). The "Side effects" column above describes what each `Apply` does.

### 6.3 Arrow type × material outcome table

`reflect(v,n,k)` = reflected velocity × k. "Spent" = drops as a dynamic arrow. Impulse transfer and damage are defined in §6.4. Material numbers are in `03` §4.

| Arrow ↓ / Material → | Straw | Timber | Stone | Ice | Metal |
|---|---|---|---|---|---|
| **Oak** | Embed (θ ≤ 80°) | Embed if θ ≤ 60° ∧ speed ≥ 5 m/s, else Deflect `reflect(0.3)` → Spent | Deflect `reflect(0.25)` → Spent ("chip") | If damage ≥ HP: **Shatter** + PassThrough × 0.5; else Deflect `reflect(0.4)` → Spent | γ ≤ 25° → **Ricochet** `reflect(0.6)` (max 2); else Stop → Spent ("clank") |
| **Heavyhead** | Embed (pierces; never deflects) | Breaks it if damage ≥ HP (then PassThrough × 0.4), else Embed (θ ≤ 70°) | Stop → Spent; full impulse transfer (pushes stone) | Shatter if damage ≥ HP + PassThrough × 0.5; else Stop | Stop → Spent (never ricochets); impulse applied |
| **Split** (parent, before split) | as Oak; triggers impact split | as Oak; impact split | as Oak; impact split | as Oak; impact split | as Oak; impact split (children may ricochet) |
| **Split child** | Embed | Embed if θ ≤ 60° ∧ speed ≥ 4, else Spent | Spent | Deflect `reflect(0.4)` | γ ≤ 25° → Ricochet × 0.6 (max 1), else Spent |
| **Fire** | Embed + **Ignite** | as Oak (no ignition — timber is not burnable) | as Oak | as Oak (no melt in MVP) | as Oak |
| **Bounce** | as Oak | as Oak | as Oak | as Oak | **Ricochet** `reflect(0.95)` once at any θ ≤ 80° (row 7), then behaves as Oak |

### 6.4 Impulse and damage

```text
J_raw   = def.impactImpulse × (speed / def.referenceSpeed) × material.impulseTransfer × outcomeFactor(kind)
J       = clamp(J_raw, 0, def.maxImpulse)
outcomeFactor: Embed 1.0 · Stop 1.0 · Deflect 0.6 · Shatter 0.5 · Ricochet 0.3 · PassThrough 0.2 · Teleport/Ignore 0
apply   : body.AddForceAtPosition(normalize(v.xy) × J, hit.point, ForceMode.Impulse) if body is non-kinematic (wake it)
damage  = J × def.damageScale × material.arrowDamageMultiplier  → Breakable.ApplyDamage(damage, DamageSource.Arrow)
meaningful = (J ≥ meaningfulImpulseThreshold ∧ kind ∈ {Embed, Stop, Shatter}) ∨ caused a break ∨ cut a rope ∨ cleared an objective
```

The impulse is deliberately **not** `arrowMass × v` (that is too small to topple crates). It is a tuned "hit strength" that stays readable and stable. `referenceSpeed` = 10 m/s.

### 6.5 Special behaviours (strategy classes selected by `ArrowDefinition.behaviour`)

- **Heavyhead** — no separate class (data only: low speed, high impulse/damage, gravityScale 1.25, windResponse 0.25, `maxRicochets = 0`).
- **`SplitArrowBehaviour`** (D-088, supersedes D-019): `splitTime` is configurable per `AD_Split` (default 0.45 s). At `Time ≥ splitTime − splitPulseLeadSeconds` (0.12 s) it plays the cosmetic split cue — a glowing ring / trail pulse (`VFX_Arrow_SplitPulse`, a pooled child particle on the `Arrow_Split` prefab, no gameplay effect) — so the rule is readable before it happens; at `Time ≥ splitTime` and `!HasSplit` → spawn 3 children at the parent position with velocities rotated −12°/0°/+12° and ×1.0 speed; each child is `ArrowType.Split` with the `isChild` flag (mass factor 0.5 → impulse ×0.5, damage ×0.6, lifetime 0.8 s, windResponse 1.4, `maxRicochets = 1`). Impact before split: the children spawn at `hit.point + n × 0.15` with directions = the tangent-reflected velocity ±12° (forward fan) and the struck body receives the parent impulse. Children never split. Quiver: counts as 1 arrow.
- **`FireArrowBehaviour`**: on a hit, if the target has `Burnable` → `Burnable.Ignite(IgniteSource.FireArrow)`. Passing through a rope also ignites the rope's remaining visual (cosmetic). If it embeds in a non-burnable target, the flame VFX lingers 1.5 s with **no gameplay effect** (keeps fire rules readable; see `03` §8.8). Trail VFX: flame + ember.
- **`BounceArrowBehaviour`**: enables row 7 (one guaranteed metal ricochet with 0.95 restitution); the preview shows that bounce (D-085 — the preview shows the first bounce for every arrow; Bounce is the only one that guarantees it). After the bounce it is Oak in every way (`RicochetsLeft = 0` for Oak-style metal ricochets too).

### 6.6 Tests and acceptance
- EditMode `ArrowImpactResolverTests`: a parameterised table over (arrow type × material × θ ∈ {0, 30, 60, 70, 85}° × speed ∈ {3, 6, 10}) asserts `ImpactKind` and impulse within ±1% of the table. Priority rows 1–10 each have one test.
- PlayMode `InteractionMatrixTests` (see `03` §15) assert world outcomes.
- AC: no arrow tunnels through any collider ≥ 0.1 m thick at 25 m/s (`ArrowTunnellingTests`, 1,000 seeded shots at a 0.1 m plank: 0 misses).

---

## 7. Embedding, bounce, despawn and pooling summary

| Outcome | Arrow after | Collider | Tracked by Settle | Returns to pool |
|---|---|---|---|---|
| Embed | Child of host, vibrates 0.2 s | none | no | On level reset, or Decor on cap |
| Deflect / Stop | Spent dynamic, fades | Arrow layer (Environment only) | no | After 1.5 s |
| Ricochet | Continues Flying | — (kinematic sweep) | Flying blocks calm | — |
| PassThrough | Continues Flying | — | — | — |
| Shatter | Continues at reduced speed | — | — | — |
| KillZone / OutOfBounds / lifetime | Disappears (Water: 0.3 s sink VFX) | — | — | Immediately |

**AC:** after 200 seeded random shots across the VS levels, pool sizes are stable (no growth after prewarm) and there are 0 orphaned arrow GameObjects after `LevelLoader.Load` (`PoolLeakTests`).

---

## 8. Win / fail / settle detection

**Owners:** PHYS (`SettleMonitor`, `PhysicsBodyRegistry`) + CORE (guards in `GameplayController`) · **Files:** `Physics/SettleMonitor.cs`, `Physics/PhysicsBodyRegistry.cs`, `Objectives/ObjectiveTracker.cs`, `Objectives/ProtectedTracker.cs` · **Tickets:** AB-018, AB-019 · **Decision:** D-015

### 8.1 Calm definition

```text
Sample() each FixedUpdate:
  moving = false
  for body in registry.TrackedBodies:          // registered gameplay rigidbodies of the current layout
     if body.isKinematic or body.IsSleeping() or body.Removed or body.Planar.IsAmbient: continue
     if body.velocity.sqrMagnitude > calmLinearSpeed² or |body.angularVelocity| > calmAngularSpeedDeg × Deg2Rad:
         moving = true; break
  moving |= externalActivity                    // passed in by GameplayController: arrowRegistry.FlyingCount > 0 (keeps Physics free of Arrows)
  moving |= TimedEvents.PendingCount > 0        // burning ropes, fuses, explosion queue about to fire
  calmSeconds = moving ? 0 : calmSeconds + dt
```

Signature: `SettleMonitor.Sample(bool externalActivity)`, called once per `FixedUpdate` by `GameplayController` (§1.3).

- **Ambient bodies** (`PlanarBody.IsAmbient`): balloon-supported loads while slower than 0.3 m/s, and bodies riding kinematic movers (contact with a `KinematicMover` in the last 0.1 s). The flag is **set by the Props components** (`Balloon`, `KinematicMover`) via `PlanarBody.MarkAmbient(untilLevelTime)`; Physics never references Props. See `03` §8.
- **Not tracked:** debris, spent arrows, kinematic movers, cursed orbs (kinematic).
- `TimedEvents` is a tiny registry (Core `LevelClock` scheduler) owned by PHYS/PROPS. It holds all deterministic delayed actions.

### 8.2 Tracker rules
- `ObjectiveTracker.AllCleared` = `remaining == 0` (D-037 rules evaluated by each `Objective`). It raises `ObjectiveCleared` per objective.
- `ProtectedTracker.LostThisStep` — set by any `ProtectedObject` meeting a D-038 condition. Latched.

### 8.3 Acceptance
- AC1: across all levels in `LevelIdleStabilityTests` (definition in `04` P-01 / `07` §2): 3 s simulated without a shot → calm reached ≤ 1 s after load, max body displacement < 1 cm, no objective cleared, no protected object lost, no `ObjectBroken`.
- AC2: after the intended solution, `Won` fires ≤ `LevelData.expectedResolveSeconds` (`04` §2 field 21c) after the last intended arrow's final impact. When the field is 0 (auto), the bot records the measured value + 0.8 s margin on the first green run, and later runs must not exceed it.
- AC3: no level waits more than 5 s after the last arrow before Fail (enforced by the cap).

---

## 9. Quiver management

**Owner:** CORE · **Files:** `Gameplay/QuiverModel.cs` (pure), `UI/QuiverView.cs` (UI) · **Ticket:** AB-017 · **Decision:** D-023

```csharp
public sealed class QuiverModel {
  public QuiverModel(IReadOnlyList<QuiverEntry> entries);       // expands to an ordered queue, e.g. [Oak, Oak, Heavyhead]
  public int Remaining { get; } public int Used { get; } public int Start { get; }
  public bool BonusUsed { get; }
  public ArrowType? Peek();                                      // next arrow (HUD highlight + nocked visual)
  public bool TryConsume(out ArrowType type);                   // only on a successful release
  public void AddBonus(ArrowType type);                          // appends 1 arrow, sets BonusUsed (rewarded ad)
  public event Action Changed;
}
```

- Arrows used = number of successful releases (split children don't count). Cancelled draws don't consume.
- The bonus arrow type is `LevelData.bonusArrowType` (default Oak; designers set a special type if the level is unwinnable with Oak).
- **HUD (UI):** arrow icons right-to-left in queue order; the next arrow is highlighted and larger; consumed icons dim and drop away (≤ 0.25 s tween). Special types use distinct icon silhouettes (not only colour).
- Tests: EditMode `QuiverModelTests` — order preserved, consume/peek, remaining never < 0, bonus append + flag, and the `Changed` event count.
- AC: the HUD count always equals `Remaining` (UI test via event); the analytics `arrows_used` equals `Used`.

---

## 10. Star rating

**Owner:** CORE (rules) / SYS (persistence) · **Files:** `Gameplay/StarRules.cs` (pure static), `Levels/LevelData.cs` (`StarsFor` delegates) · **Ticket:** AB-017 · **Decision:** D-084 (supersedes D-024)

```text
StarRules.Compute(arrowsUsed, goldPar, bonusArrowUsed):
  if bonusArrowUsed:            return 1
  if arrowsUsed ≤ goldPar:      return 3
  if arrowsUsed == goldPar + 1: return 2
  return 1
Best stars stored = max(previousBest, new)   (ProgressionService.RecordResult)
```

- Never scored on physics damage (§3).
- **Bonus-arrow cap (D-084):** a clear that used the rewarded bonus arrow is a normal completion for progression (unlocks, world count, coins as 1★) but never earns 2★/3★. The Fail-panel offer (`RewardedOfferButton`, UI) must show **"Bonus Arrow Used — 1★ Max"** (`UIStrings` key `fail.bonus_arrow.star_cap`) **before** the player accepts the ad — see `06` §7.1 and `05` (Fail panel).
- Authoring rules (validated in `04`): `1 ≤ goldPar ≤ TotalArrows`. For non-tutorial levels, `TotalArrows ≥ goldPar + 1` so 2★ is possible. §7 examples: tutorial 3/par 1, normal 4/par 2, set piece 5/par 3.
- The win screen shows stars earned + "Clear in N arrows for ★★★" when < 3★ (UI).
- Tests: the existing `LevelDataTests.StarsFor_FollowsGoldParRule` stays green. New `StarRulesTests` cover the bonus cap and par edge cases.

---

## 11. Tutorial prompt system

**Owners:** CORE (`TutorialPromptController`, triggers), UI (callout/ghost-hand views), LEVEL (content) · **Files:** `Gameplay/TutorialPromptController.cs`, `Levels/TutorialPromptData.cs`, `Levels/TutorialAnchor.cs`, `UI/ToastView.cs` · **Milestone:** M3 · **Tickets:** AB-039 (prompt system, L1 ghost hand, VS-03 callout), AB-067 (arrow reveal card, M4)

### 11.1 Data (`TutorialPromptData` in `LevelData._tutorialPrompt` — at most one callout per level; the ghost hand and the arrow reveal card are separate LevelData flags `_showGhostHand` / `_revealsArrow`; canonical schema in [`04` §2](04_LEVEL_PIPELINE.md))

| Field | Type | Default | Notes |
|---|---|---|---|
| `textKey` | string | — | `UIStrings` key, ≤ 32 characters of English (validator V-13), e.g. `tut.aim_rope` = "Aim for the rope" |
| `visual` | `TutorialVisual { Callout, Highlight }` | Callout | Ghost hand and arrow reveal are driven by the LevelData flags, not by this struct |
| `trigger` | `TutorialTrigger { OnLevelStart, AfterMisses, OnArrowsRemaining, OnFirstAppearanceOfArrow }` | OnLevelStart | |
| `triggerParam` | int | 0 | e.g. misses = 2, arrowsRemaining = 1 |
| `anchorId` | string | — | Matches a `TutorialAnchor.id` inside the layout prefab (validator checks it) |
| `dismissOn` | `{ DrawStarted, Tap, Timeout }` | DrawStarted | |
| `oncePerInstall` | bool | true for callouts | Seen keys stored in `SaveGame.tutorial.seenPrompts` |

### 11.2 Rules
- At most **one** prompt visible. Never a text wall (§2): ≤ 1 line, an icon or arrow pointing to the anchor.
- **L1 ghost hand:** loops a drag-down-from-bow animation until the first `DrawStarted`. If idle 4 s after a cancel or miss, it reappears.
- **Arrow reveal card** (`OnFirstAppearanceOfArrow`): automatic when the quiver contains a type never seen (save flag). Shown during `Intro`; tap to dismiss; ≤ 6 words + an arrow icon + a 1-loop animation. Fires for Heavyhead L13, Split L30, Fire L36, Bounce L47 (D-014).
- **New-object intros** use Callout + Highlight (pulsing outline on the anchor) in the first of the two tutorial levels (§4 rule).
- `AfterMisses` (e.g. 2 arrows spent without clearing an objective) is allowed only in designated teaching levels (LevelData flag `isTutorial`).
- Prompts are suppressed on replays of cleared levels unless `oncePerInstall = false`.

### 11.3 Acceptance
- AC: a VS playtest with ≥ 8/10 testers completing VS-03 (rope) without verbal help, with the "Aim for the rope" callout only.
- AC: the validator fails a level whose `anchorId` doesn't exist in its layout.
- Tests: EditMode `TutorialTriggerTests` (trigger evaluation, once-per-install); PlayMode L1 ghost hand appears < 0.7 s after Intro.

---

## 12. Camera logic

**Owner:** CORE · **Files:** `Gameplay/CameraFramer.cs`, `Prefabs/Roots/GameplayRoot.prefab` (CameraRig child), `Feedback/CameraShake.cs` (ART executes) · **Ticket:** AB-004 · **Decision:** D-041

### 12.1 Framing algorithm

```text
Inputs: LevelLayout.framing { center = (0, 8.9), playWidth = 10, playHeight = 17.8, pitchDeg = 8, fovV = 30 }
aspect = Screen.safeArea width/height (portrait)
tanH   = tan(fovV/2)
dWidth  = (playWidth/2  × 1.03) / (tanH × aspect)       // fit width (3% margin)
dHeight = (playHeight/2 × 1.03) / tanH                  // fit height
d = (aspect ≤ 9/16 + ε) ? max(dWidth, dHeight × hudCompensation) : dHeight     // tall phones fit width; tablets fit height (pillarbox)
camera.rotation = Euler(pitchDeg, 0, 0); camera.position = center - camera.forward × d
Then shift by the safe-area offset so the HUD band (top ~9%) never covers the structure zone.
```

- **Static per level.** Recomputed on `LevelStarted` and on a resolution/safe-area change (foldables, split screen). Set pieces get a per-level framing override (e.g. wider `playWidth` 12 m for L20/40/60); there is no camera motion (§3).
- **Screen↔world** helpers: `CameraFramer.WorldToScreen` (tutorial anchors, objective icon flights), `ScreenToPlane` (dev tools only — aiming is screen-space).
- **Camera shake** (`CameraShake`): explosions (0.05 m, 0.15 s) and set-piece collapses only. Disabled when `Settings.reducedMotion`. It offsets a child transform so framing math is unaffected.
- Tests: PlayMode `CameraFramerTests` at 9:16, 9:19.5, 9:21, 3:4, 2:3 — the play-area corners project inside the safe area, with the top HUD band not overlapping y > 15.
- AC: on every device-matrix aspect ratio, the full structure zone and the bow are visible and the bow is not covered by the home indicator (manual device check, `07`).

---

## 13. Time scaling and hit-stop policy

**Owner:** CORE (policy) · ART (tuning feel) · **Files:** `Core/TimeScaleController.cs`, `Feedback/HitStop.cs`, `Feedback/CameraShake.cs` · **Ticket:** AB-033 (M3)

- **A single writer of `Time.timeScale` (D-061):** `Core/TimeScaleController` exposes `RequestHitStop(seconds)`, `RequestSlowMo(scale, seconds)`, `SetPaused(bool)` with fixed priority Pause > slow-mo > hit-stop. `Feedback/HitStop` decides *when* a hit-stop is warranted (meaningful direct impact, rate limit, Reduced Motion cap) and calls the controller. GameplayController uses it for pause and the protected-loss slow-mo. Nothing else writes `timeScale` (code-review rule; a grep check in `ArchitectureRulesTests`).
- Priority: **Paused (0) > protected-focus slow-mo (0.3) > hit-stop (0.05) > normal (1)**.
- **Hit-stop:** triggered by `ArrowImpact` with `isMeaningful` (§6.4), or by `ObjectBroken` for Objective kinds. Duration = lerp(40 ms, 70 ms, (J − 3)/(maxImpulse − 3)), measured in **unscaled** time. Cooldown 0.3 s (a chain of breaks produces one hit-stop, not ten). Never during `Paused`, `Won` or `Failed`.
- **`fixedDeltaTime` is never changed.** Slower time = fewer physics steps per real second, so outcomes are identical (the determinism rule). Audio is unaffected (`AudioListener` isn't time-scaled; pitch is not bent).
- Reduced Motion setting: hit-stop stays (it isn't motion) but is capped at 40 ms; camera shake is off.
- Tests: EditMode `TimeScaleControllerTests` (priority, cooldown, unscaled duration). PlayMode determinism: the same shot with and without hit-stop gives an identical end-state hash (`DeterminismReplayTests` in `03`).

---

## 14. Haptics and feedback events

**Owners:** CORE (raises events), ART (`FeedbackDirector` mapping, SFX/VFX assets), PLAT (`HapticsService` native bridge) · **Files:** `Feedback/FeedbackDirector.cs`, `Feedback/HapticsService.cs`, `Feedback/HapticKind.cs`, `Services/IHapticsService.cs` · **Milestone:** M3 · **Tickets:** AB-032 (FeedbackDirector), AB-034 (HapticsService native bridge), AB-030/AB-031 (VFX/audio services), AB-146 (haptics tuning, M9) · **Decision:** D-032

### 14.1 Event → feedback map (FeedbackDirector is the only listener that plays feedback)

> **Canonical IDs:** the SFX/VFX asset IDs and the `HapticKind` per event are owned by [`05` §10.2 and §12](05_ART_AUDIO_UX.md). This table defines *which events* produce feedback and the hit-stop/shake policy. Where a SFX name or haptic strength below differs from `05`, **`05` wins**.

| GameEvent | SFX (`SoundEvent`) | VFX (`VfxEvent`) | Haptic | Hit-stop / shake |
|---|---|---|---|---|
| `DrawStarted` | `SE_Bow_DrawStart` (creak loop, pitch rises with power) | String glow ramps | — | — |
| `DrawThresholdReached` (fireable) | — | — | **Light** (§8) | — |
| `DrawThresholdReached` (full) | `SE_Bow_FullDraw` tick | String glow max | Light (Selection) | — |
| `DrawCancelled` | `SE_Bow_Relax` | — | — | — |
| `ArrowFired` | `SE_Bow_Release` + `SE_Arrow_Whistle` (pitch ∝ speed) | Trail (cosmetic trail from equipped `CD_Trail_*`) | — | — |
| `ArrowImpact` meaningful | `SE_Impact_<Material>` (heavy variant) | `VFX_Impact_<Material>` | **Medium** | Hit-stop |
| `ArrowImpact` minor | `SE_Impact_<Material>` (light variant) | small puff | — | — |
| `ObjectBroken` | `SE_Break_<Material>` | `VFX_Break_<Material>` + debris | Medium (Heavy for explosions) | — (shake for explosions) |
| `PropTriggered` | per prop (`SE_Rope_Snap`, `SE_Balloon_Pop`, `SE_Barrel_Blast`, `SE_Oil_Ignite`, `SE_Portal_Whoosh`, `SE_Boulder_Release`) | per prop | Medium for blasts | shake for blasts |
| `ObjectiveCleared` | `SE_Objective_Clear` (escalating pitch per objective in a 2 s window = chain-reaction percussion, §11) | crest burst + icon flies to HUD | Light | — |
| `ProtectedLost` | `SE_Protected_Lost` | purple flash on the object | **Heavy** | slow-mo focus |
| `LevelWon` | `SE_Win_Sting` | confetti (after the outcome is readable) | **Success** pattern | — |
| `LevelFailed` | `SE_Fail_Soft` | — | Failure (soft) | — |
| `SoftLockPrompt` | `SE_UI_Nudge` | HUD arrow pulse | — | — |

### 14.2 Rules
- **Readability first (§4):** VFX for a break spawns only after the hit-stop ends. Particles never cover an uncleared objective for more than 0.3 s (`VfxEvent.avoidObjectives`: spawn depth behind objectives, max alpha 0.6 over red targets).
- **Rate limits:** haptics ≤ 1 per 50 ms and ≤ 6 per second; SFX per `SoundEvent` has a max of 3 voices and a 40 ms retrigger cooldown; chains escalate pitch, not volume.
- **Settings:** `haptics`, `sfx`, `music`, `reducedParticles` (VFX particle counts × 0.4, no confetti), `reducedMotion` (no shake, hit-stop ≤ 40 ms) — read by FeedbackDirector and HitStop at play time.
- `HapticKind { Light, Medium, Heavy, Success, Failure, Selection }`. The editor implementation logs to the DevOverlay. Device mapping is in `05`.
- Tests: EditMode `FeedbackDirectorTests` (each event maps to the expected calls on mock services; settings off ⇒ no haptic calls; rate-limit behaviour).
- AC: on the device matrix, every row of 14.1 is perceptible and none is missing (manual checklist in `07`). The haptics toggle immediately silences all haptics.

---

## 15. Arrow-type specification (graybox tuning, `AD_<Type>` assets)

Spec preset values (Snappy in brackets where different). Final values are set at the M1/M3 gates (Oak, AB-005/AB-014; locked at G0) and in **M4 Systems Complete** for every special arrow (Heavyhead AB-049, Fire AB-088, Split AB-089, Bounce AB-106) — D-102. No special arrow is built before the G0 GO.

| Field | Oak | Heavyhead | Split | Fire | Bounce |
|---|---|---|---|---|---|
| `type` / `behaviour` | Oak / Standard | Heavyhead / Heavy | Split / Split | Fire / Fire | Bounce / Bounce |
| First appearance (D-082) | L1 (W1_L01) | L13 (W1_L13) | L30 (W2_L10) | L36 (W2_L16) | L47 (W3_L07) |
| `minSpeed` / `maxSpeed` (m/s) | 4 / 13 [6 / 20] | 3.5 / 10 [5 / 15] | 4 / 13 [6 / 20] | 4 / 13 [6 / 20] | 4 / 13 [6 / 20] |
| `gravityScale` | 1.0 | 1.25 | 1.0 | 1.0 | 1.0 |
| `windResponse` | 1.0 | 0.25 | 1.0 (children 1.4) | 1.0 | 1.0 |
| `impactImpulse` @ 10 m/s (Ns) | 6 | 18 | 6 (children 3) | 6 | 6 |
| `maxImpulse` (Ns) | 10 | 30 | 10 (children 5) | 10 | 10 |
| `damageScale` | 1.0 | 3.0 | 1.0 (children 0.6) | 1.0 | 1.0 |
| `embedMaxIncidenceDeg` (timber) | 60 | 70 | 60 | 60 | 60 |
| `embedMinSpeed` (m/s) | 5 | 3 | 5 (children 4) | 5 | 5 |
| `maxRicochets` (metal, grazing ≤ 25°) | 2 | 0 | 1 (children) | 2 | 1 guaranteed (any θ ≤ 80°, ×0.95), then Oak rules |
| Special params | — | — | `splitTime` 0.45 s (configurable, D-088), `splitPulseLeadSeconds` 0.12, `splitSpreadDeg` 12, `childCount` 3, `childLifetime` 0.8 s | `igniteOnHit` true, `lingerFlameSeconds` 1.5 (cosmetic) | `bounceRestitution` 0.95, `bounceMaxIncidenceDeg` 80 |
| Preview extras (D-085) | first bounce if a grazing metal ricochet is predicted | — (never ricochets) | split marker + pulse cue + 3 stubs (0.35 s) | flame-coloured dots; first bounce as Oak | shows the guaranteed bounce |
| Readability (silhouette) | thin shaft, leaf fletch | fat iron head, short | three-prong head | ember-wrapped head | rounded rubber-gold head |
| Pool prewarm | 8 | 3 | 3 + 9 children | 3 | 3 |

Arrow mass is not physically simulated while flying (kinematic). Spent arrows use mass 0.05 kg (Environment collisions only).

---

## 16. Dependency summary (ticket order)

| System | Depends on | Tickets |
|---|---|---|
| Services/events/clock | — | AB-003 |
| Camera/play plane | AB-003 | AB-004 |
| Tuning SOs | AB-003 | AB-005 |
| BallisticSolver | AB-005 | AB-006 |
| Draw input | AB-005 | AB-007 |
| Preview | AB-006, AB-007, AB-010 (cast mask/layers from AB-002) | AB-008 |
| Arrow flight/pool | AB-006, AB-002 | AB-009 |
| Impact resolver | AB-009, AB-010 | AB-011 |
| Breakables | AB-010 | AB-012 |
| Level loading | AB-004, AB-013 | AB-016 |
| Quiver/stars | AB-005 | AB-017 |
| Objectives/protected | AB-012, AB-016 | AB-018 |
| Settle + state machine | AB-017, AB-018 | AB-019 |
| Ropes / kill zones | AB-011, AB-016 | AB-020, AB-021 |
| HUD | AB-019 | AB-023 |
| Feedback services (VFX, audio, haptics) | AB-003, AB-012 | AB-030, AB-031, AB-034 |
| FeedbackDirector, hit-stop/TimeScaleController | AB-030, AB-031, AB-033, AB-034 | AB-032, AB-033 |
| Tutorial prompts | AB-016, AB-035 | AB-039 (+ AB-067 arrow reveal card) |
| Heavyhead | AB-011 | AB-049 (M4) |
| Fire / Split | AB-086 / AB-049 | AB-088 / AB-089 (M4) |
| Wind (`IFlightEnvironment`) / Bounce / Portals | AB-006 / AB-094 / AB-006 | AB-105 / AB-106 / AB-107 (M4) |
| Rewarded bonus arrow (`QuiverModel.AddBonus`) | AB-017, AB-128 | AB-129 (M7) |

Order rule (D-102): every M4 row starts only after the G0 Vertical Slice & Feel Lock GO.
