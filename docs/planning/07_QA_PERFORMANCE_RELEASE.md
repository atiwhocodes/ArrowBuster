# 07 — QA, Performance & Release

> Owners: QA & Playtest Lead (`QA`) for test strategy, regression and gates; Mobile Performance & Platform Engineer (`PLAT`) for performance, devices and store builds; `MON` for monetisation/privacy checks; `INT` enforces gates at merge time.
> Source: [`/mvp.md`](../../mvp.md) §3, §7, §12, §13, §15. Budgets: [`01_TECHNICAL_ARCHITECTURE.md`](01_TECHNICAL_ARCHITECTURE.md) §11. Monetisation rules: [`06_META_MONETISATION_ANALYTICS.md`](06_META_MONETISATION_ANALYTICS.md). Status: Draft v1 — 2026-10-09.

**QA principle:** every gameplay outcome a player relies on is either **unit-tested** (pure logic), **bot-tested** (physics/level solvability in PlayMode), or **device-verified** (feel, perf, platform). Nothing ships on "it worked in the editor once".

---

## 1. Unit-test candidates (EditMode, `Tests/EditMode/`)

| Test class | System under test | Key cases |
|---|---|---|
| `BallisticSolverTests` | `BallisticSolver` | Zero-gravity straight line. Apex time = v·sinθ/g within 1e-4. Symmetric range on flat ground. Step determinism (same input → bit-identical output over 300 steps). Wind acceleration adds the expected drift. dt invariance within tolerance for 1/60 vs 1/120 (documents the error). Portal transform: position/velocity rotated correctly. Bounce reflection: angle in = angle out, speed × restitution. |
| `ArrowPreviewParityTests` (EditMode part) | Preview sampler vs flight integrator | Same initial `ArrowFlightState` → preview points equal flight positions ≤ 1 mm over 3 s, with and without a wind sampler. Split marker at `splitTime`. |
| `DrawModelTests` | `DrawModel` / `AimState` (D-018) | Drag down → aim up. Power clamps 0–1. Power = length / (0.22 × screen height). Below `minFirePower` → `Cancel`. Angle clamped to 8°–172°. Press over UI ignored. Secondary pointer ignored. Resolution independence (720p vs 1440p give the same `AimState` for the same normalised drag). |
| `QuiverModelTests` | `QuiverModel` | Consumes in authored order (D-023). `Next` preview. Empty after N shots. Bonus arrow append. `Changed` event count. Total = sum of entries. |
| `StarRulesTests` | `StarRules` (D-024) | arrowsUsed ≤ par → 3. par+1 → 2. par+2 → 1. Bonus arrow → 1 regardless. Best stars never decrease (with ProgressionService). |
| `LevelDataTests` (exists, extend) | `LevelData` | Existing star/total/global index tests. goldPar ≤ total arrows. Schema v2 fields default sanely. `levelId` format `W#_L##`. |
| `SettleMonitorTests` | `SettleMonitor` state machine with a fake clock + fake bodies (D-015) | Win after 0.75 s calm. Win at cap 3 s with jitter. Out-of-arrows fail after 2 s calm. Fail at cap 5 s. Objective cleared during the fail wait → win. Protected lost during the win settle → fail. Ambient/kinematic bodies ignored. Soft-lock toast once per shot. |
| `GameplayStateMachineTests` | `GameplayController` transitions (logic extracted to a plain class) | Ready → Drawing → Ready (cooldown 0.35 s, D-016). Pause blocks input and clock. No fire while `Won`/`Failed`. Restart from any state → `Loading`. |
| `ObjectiveClearRuleTests` | `ObjectiveClearRule` evaluator (D-037) | Per kind: CursedOrb ignores impacts/blasts and clears on an arrow hit. Dummy tilt > 70° for 0.3 s clears; 0.2 s doesn't. Crate below the clear line clears. BannerRope clears on cut and on burn. |
| `ProtectedRulesTests` | `ProtectedObject` rules (D-038) | Vase: arrow hit fails; impulse ≥ fragile fails; below → ok. Fox: displacement > 0.6 m fails; tilt > 45° fails; kill zone fails. |
| `ArrowImpactResolverTests` | Outcome rules (pure part) | Per material × arrow type × incidence angle → outcome table (embed/deflect/ricochet/pass/cut/pop). Impulse clamped to `maxArrowImpulse`. Bounce arrow ricochets once on metal only. Heavyhead never embeds in straw (pierce) — per `03` table. |
| `ExplosionSolverTests` | `ExplosionSolver` | Falloff curve. Protected layer excluded (D-020). Upward modifier capped. Chain delay 0.15 s ordering deterministic. |
| `AdPolicyTests` | `AdPolicy` | 18 cases listed in `06` §7.3 |
| `EconomyServiceTests` | `EconomyService` + `EconomyConfig` | First-clear 20 + 10×★. Replay deltas. Plain replay 5. Bullseye once. `TrySpend` fails on insufficient balance and does not change the balance. Clamp at max. Events fire with source/sink. |
| `ProgressionServiceTests` | Unlocks (`06` §1) | Linear unlock. World 2 at 15/20 clears (14 → locked). Best values monotonic. `world_unlocked` raised once. |
| `InventoryServiceTests` | Cosmetics | Default owned/equipped. World completion grants the skin, or 300 coins if already owned. Starter pack grants items + coins once (`starterPackCoinsGranted`). Badges not purchasable. |
| `DailyChallengeServiceTests` | D-027 | Same date + installId → same 3 levels. Only cleared levels. < 3 cleared handled. Locked before L10. Reward once per day. Date rollover. |
| `SaveMigratorTests` | `SaveMigrator` | Each fixture `save_v<N>.json` migrates to the latest. Unknown future version is not overwritten. Corrupt file → `.bak` → fresh. |
| `JsonSaveServiceTests` | Atomic write | Write → read round-trip equality. Simulated crash between tmp write and replace leaves a valid file. Debounce collapses bursts. |
| `RemoteConfigClampTests` | `RemoteConfigDefaults` | Every whitelisted key has a default + range. Out-of-range values clamp. Unknown keys are ignored. |
| `UIStringsTests` | `UIStrings` | Every key used by prefabs exists. No empty strings. |
| `LevelValidatorTests` | `LevelValidator` rules (`04`) | Fixtures of bad layouts trigger the expected errors: z ≠ 0, missing `PlanarBody`, > 60 bodies, mass ratio > 10:1, overlap at spawn, unpacked prefab, objective below the clear line, goldPar > arrows, missing intended shots. |
| `ArchitectureRulesTests` | Namespace dependencies (`01` §10.3) | Gameplay/Physics/Arrows/Props types don't reference `ArrowBuster.UI`/`Feedback`/`Integrations`. No `UnityEngine.Random` usage in gameplay namespaces (IL scan). Static mutable fields have a `StaticReset` registration (reflection). |
| `PrefabValidatorTests` | `PrefabValidator` | World art variants keep base collider bounds and mass. Layers match the prefab category. |
| `TimeScaleControllerTests` | `TimeScaleController` (D-061) | Priority Pause > slow-mo > hit-stop. Overlapping requests don't stack. Restore uses unscaled time. No other writer of `Time.timeScale` (grep check lives in `ArchitectureRulesTests`). |
| `ServicesTests` | `Services` / `ServiceInstaller` | Null-object defaults are never null. `EnsureInstalled()` is idempotent. Tests can swap implementations. |
| `AnalyticsEventsTests` | `AnalyticsEvents` + `AnalyticsBridge` | Every event in `06` §11 has a builder. Required common params are present. No PII fields. |
| `TutorialTriggerTests` | `TutorialPromptController` triggers | Each `TutorialTrigger` fires once. `oncePerInstall` respects `seenPrompts`. Dismiss on draw. |
| `FeedbackDirectorTests` | `FeedbackDirector` | Each `GameEvents` event maps to the expected mock audio/VFX/haptic/hit-stop calls. Settings off → no haptic calls. Reduced Motion → hit-stop ≤ 40 ms (D-057). |
| `ArrowDefinitionTests` · `MaterialBodyTests` · `PlanarBodyTests` · `BreakableTests` | Data validation and component math | Range validation. `PlanarBody` applies the constraints and z = 0. `MaterialBody` sets mass/physic material from the profile. HP math. |
| `PhysicsSettingsTests` · `PrefabLibraryTests` | Project settings and the prefab library | Layers 6–16, matrix (`03` §2), Δt 1/60, solver 8/2. Every library prefab has `PlanarBody` + `MaterialBody` + the correct layer. |
| `ReachabilityTests` | `BallisticSolver` coverage | An Oak arrow can reach every 0.25 m cell of the structure zone (y 4–15, \|x\| ≤ 4.5) on a 1° × 0.01-power aim grid (`02` §4). |

**Rule:** a new pure-logic class lands with its test class in the same PR. Coverage target: ≥ 80% line coverage on `Core`, `Gameplay` (logic classes), `Progression`, `Services/AdPolicy` (Code Coverage package optional, M9).

## 2. PlayMode-test candidates (`Tests/PlayMode/`)

| Test class | What it proves | Method |
|---|---|---|
| `LevelSolvabilityTests` | Every level's intended solution wins within par, **robustly** | For each `LevelData` in `LevelCatalog`: load the level via `LevelLoader`, replay `IntendedShot`s with the jitter grid (§5), apply the §5.1 pass criteria (all samples `LevelWon`; nominal + ≥ 7/9 samples per shot with `arrowsUsed ≤ goldPar`). Time-scaled to 4× where physics allows (`Time.timeScale` only, fixed Δt unchanged). Parameterised `[TestCaseSource]` per level. Tag `[Category("Solvability")]`. |
| `LevelIdleStabilityTests` | Layouts don't move, break or clear when untouched | Load each level, simulate 3 s with no input: max body displacement < 1 cm, no `ObjectBroken`, no `ObjectiveCleared`, no `ProtectedLost`, all bodies asleep at the end. |
| `ArrowPreviewParityTests` (PlayMode part) | The preview predicts the real in-scene flight including collisions | Sandbox scene with walls/rope/portal/wind: fire 20 scripted shots. Compare the predicted impact point with the actual one: ≤ 2 cm (static targets). |
| `InteractionMatrixTests` | Every cell of `03`'s interaction matrix behaves as specified | One micro-scene per cell (arrow type × material/prop): e.g. Fire × Rope → burns in `burnTime ± 1 frame`; Bounce × Metal → single ricochet; Split × Balloon → pops; Heavyhead × Stone → pushes, no embed; Oak × Ice → shatter at HP 0. Explicit "no interaction" cells are asserted too (e.g. Fire × Stone → nothing). |
| `RestartPerfTests` | §12 restart < 1 s (target 300 ms) | Load the heaviest layouts (L20/L40/L60), measure `LevelLoader.Load` wall time over 10 restarts in the editor and in a dev player build via the Unity Test Framework player run; assert p95 < 300 ms (Mid) and record the GC alloc. |
| `GameFlowSmokeTests` | Boot → Home → Map → Gameplay → Win → Next → Fail → Retry → Home | Mock services. Simulated taps through `UIRouter`. Asserts no errors in the log and correct scene/state sequence. |
| `UiFlowTests` | Screen routing and button wiring (UI-owned smoke) | Drive `UIRouter` through every screen in the `05` screen inventory with mock services. Assert each opens/closes, Back works, buttons are interactable, safe area is applied at 9:16 and 9:21. |
| `ArrowCapTests` | ≤ 8 active arrows; old embedded arrows → static decor | Fire 12 arrows into timber. Assert counts and no colliders on decor arrows. |
| `DebrisCapTests` | Debris ≤ 40 / 20 on Low | Break 20 crates simultaneously. Assert the pool cap and the oldest-recycled order. |
| `ExplosionChainTests` | Barrel chains resolve deterministically | 4-barrel chain: identical outcome hash over 5 runs. Protected untouched by the blast. |
| `KillZoneTests` | D-039 semantics | Objective into water → cleared. Protected into spikes → fail. Arrow into the pit → resolved. |
| `DeterminismReplayTests` | Same inputs → same outcome on this platform | Replay each VS level's intended solution 5×. The final state hash (positions rounded to 1 mm, broken set, cleared set) must be identical. |
| `StaticResetTests` | No leaks across Enter Play Mode without domain reload | Enter/exit play twice programmatically. Event subscriber counts and registries return to zero. |
| `ArrowTunnellingTests` | No missed hits at any speed | 1,000 automated shots at 0.1 m planks and 0.06 m rope colliders across the full power range → 100% hit registration (M1 acceptance). |
| `PreviewBlockingTests` | The preview never draws through solids (D-040) | Preview against plank/crate/stone stops at the first hit; it passes through rope and portal triggers. |
| `ArrowEmbedTests` · `ArrowPoolTests` · `PoolLeakTests` | Arrow lifecycle and pooling | Embed per material/angle (`03` matrix). Pool size stable after prewarm. 0 orphaned arrows after `LevelLoader.Load`. |
| `BowInputTests` | `BowInputReader` with `InputTestFixture` + simulated `Touchscreen` | A press over UI doesn't start a draw. A second touch is ignored. Release below `minFirePower` cancels without spending an arrow. |
| `CameraFramerTests` | `CameraFramer` (D-041) | At 9:16, 9:19.5, 9:21, 3:4, 2:3 the play-area corners project inside the safe area. |
| `DebrisIsolationTests` · `StackStabilityTests` · `RopeCutTests` · `ObjectiveIntegrationTests` | Physics/object integration | Debris never touches gameplay layers (D-008). Stack spike scenes (PG-1/PG-2). A rope cut drops the load cleanly. Objectives clear in-scene per D-037. |
| `HudBindingTests` | HUD ↔ gameplay | Quiver, next arrow, objective cross-outs and toasts update from `GameEvents`. |

Running: Test Runner, or MCP `run_tests` (`EditMode`, `PlayMode`). CI runs both on every PR to `main` once CI exists (`01` §17). `Solvability` category runs nightly and before every gate (it is slow).

## 3. Manual testing procedures

| Procedure | When | Steps (summary) | Record in |
|---|---|---|---|
| **Feature smoke** | Every merged feature | Run the feature's acceptance criteria from its ticket on the editor + one Android device | PR handoff report |
| **Level playthrough** | New/changed level | A tester who has not seen the solution plays it cold: time to first shot, attempts, did they find the intended trick, stars. Then the designer plays the intended solution 3× on device. | `docs/levels/W#_L##.md` "Test log" |
| **Regression checklist** | Every milestone gate + every release candidate | `docs/qa/REGRESSION_CHECKLIST.md` (core loop, each object type, UI flows, settings, save, ads/IAP mocks) | `docs/qa/test-runs/<date>_<build>.md` |
| **Feel review** | M1 gate, M3 gate, M5 gate | 5+ testers. Script: no instructions; observe the draw gesture, misfires, hesitation; ask for 3 words describing the shot; preference test of Spec vs Snappy (D-036) | `docs/qa/playtests/<date>_<topic>.md` |
| **Exploratory session** | Weekly (60 min, charter-based) | Charters: "break the physics", "cheese levels with fewer arrows", "spam restart/pause", "interrupt everything" | Bug log |
| **Device pass** | Each gate | Device matrix subset (§6) | Test-run file |

Playtest script (VS and W1 gates): consent form, no coaching, think-aloud optional, 20–30 min, then a 6-question survey (fun 1–5, clarity 1–5, "did you know what to hit?", favourite moment, most frustrating level, would you keep playing Y/N). Observe and log: the weak points discovered without hints (§16 action 2), retries per level, quits.

## 4. Physics regression suite

Tagged `[Category("Physics")]` (PlayMode), run on every PR touching `Physics/`, `Arrows/`, `Props/`, `Objectives/` or `ProjectSettings/DynamicsManager.asset`:

1. `LevelIdleStabilityTests` (all levels)
2. `LevelSolvabilityTests` (all levels, jitter grid)
3. `InteractionMatrixTests`
4. `DeterminismReplayTests`
5. `ExplosionChainTests`, `ArrowCapTests`, `DebrisCapTests`, `KillZoneTests`
6. **Stack benchmarks** (sandbox scenes): 10-crate tower idle 10 s (drift < 1 cm); 3×5 timber wall hit by Heavyhead (collapse resolves < 3 s); stone on ice slide (stops within the expected 0.5 m band); a lever with a weight (settles < 2 s).
7. **Golden outcome hashes:** for the 5 VS levels + every boss level, store the expected final-state hash per platform in `Tests/PlayMode/Golden/`. A hash change fails the test. If intentional, update with a decision note in the PR.

Any change to `DynamicsManager`, `MaterialProfile` assets, `ArrowDefinition` assets or `GameplayTuning` **requires a full solvability run** before merge (INT checklist).

## 5. Level solvability verification

### 5.1 Tolerance definition
§7: "intended aim windows approximately 8–12% of screen width at target distance". The play area is 10 m wide (`01`), so the window is 0.8–1.2 m at the play plane.

- **Angle jitter δθ** per shot = the angle subtending **±4% of play-area width (±0.4 m)** at the shot's intended first-impact distance *d*: `δθ = atan(0.4 / d)`, floored at **0.5°**.
- **Power jitter δp** = **±3%** of power (absolute 0.03 on the 0–1 scale).
- **Grid:** 9 samples per shot = nominal, ±δθ, ±δp, and the 4 corners. For multi-shot solutions, each shot is jittered independently in a 9-sample sweep while the others stay nominal (9 × shots runs).
- **Pass criteria:** all samples win; the nominal run and ≥ 7/9 samples per shot achieve ≤ goldPar arrows (3★). The level designer must widen the window (move/resize the weak point) if fewer pass.
- **Timing levels** (moving shields/platforms): the shot release time is jittered ±0.1 s as a third axis.

### 5.2 Three-layer verification
| Layer | Who / what | When | Pass |
|---|---|---|---|
| 1. Bot (editor) | `LevelSolvabilityTests` | Every level change, nightly, gates | §5.1 |
| 2. Bot (device) | Dev-build `IntendedSolutionRunner` (DevOverlay ▸ "Run intended solution", `AB_DEV`) replays the same shots on device and logs win/arrows | Each gate on 1 iOS + 1 Android (Low tier) | Same outcome as the editor for every level. A mismatch → physics ticket. |
| 3. Human | A tester plays every level on device at least once per world gate (§3) | W1/W2/W3 gates, M9 | Clears without help; time and attempts logged |

The level is marked `Status = Final` in `LevelData` only when all 3 layers pass (see `04` delivery tracker).

## 6. Device matrix

| Tier | Platform | Device (example) | OS | Aspect / notes | Priority |
|---|---|---|---|---|---|
| **Owner's device** | _TBD — fill in_ | _TBD_ | _TBD_ | _TBD_ | P0 (daily) |
| Mid (baseline) | iOS | **iPhone 11** | iOS 17/18 | 19.5:9, notch | P0 |
| Mid | iOS | iPhone SE (2nd/3rd gen) | iOS 16+ | **16:9**, small screen, Touch ID, no notch | P0 |
| High | iOS | iPhone 15 / 16 | latest | Dynamic Island, 19.5:9 | P1 |
| High | iOS | iPhone Pro Max (ProMotion) | latest | 120 Hz → must lock 60 | P1 |
| Tablet | iOS | iPad (9th/10th gen) | iPadOS 16+ | **4:3**, pillarbox (D-041) | P1 |
| Min OS | iOS | Any device on **iOS 15** | 15.x | minimum target | P2 |
| **Low** | Android | Samsung Galaxy A13 / A14 | Android 12–14 | 20:9, 3–4 GB, Mali | **P0** |
| Low | Android | Xiaomi Redmi 10A-class (Helio G25/P35) | Android 11–12 | 20:9, 2–3 GB | P1 |
| Mid | Android | Pixel 6a or Galaxy A54 | Android 14–15 | punch-hole | P0 |
| High | Android | Galaxy S2x / Pixel 8+ | Android 15+ | 120 Hz → lock 60 | P1 |
| Min OS | Android | Any device on **Android 8.0 (API 26)** | 8.x | minimum SDK | P2 |
| Tall | Android | 21:9 device (e.g. Xperia) | any | extra-tall framing | P2 |
| Foldable | Android | Galaxy Z Flip/Fold (optional) | any | aspect change at runtime | P3 |

Cloud device farms (Firebase Test Lab / AWS Device Farm) are optional for the P2/P3 rows at M9. *Verify cost and Unity support before use.*

## 7. Mobile performance test plan

**Budgets:** `01` §11 (Mid = iPhone 11 at 60 FPS; Low at 30 FPS).

| Scenario | Content | Metrics | Pass |
|---|---|---|---|
| P1 Idle level | L20, no input, 30 s | Frame time p50/p95, CPU main, GPU, batches | p95 ≤ 16.6 ms (Mid), ≤ 33.3 ms (Low) |
| P2 Worst collapse | Heaviest set piece (L20/40/60): fire the intended solution | Physics ms peak, frame p99, debris count | Physics ≤ 5 ms worst frame (Mid). No frame > 50 ms. |
| P3 Explosion chain | Sandbox: 4 barrels + 30 bodies | Same | Same |
| P4 Particle storm | Fire + oil + 3 breaks + a trail (Low tier: 50% particles) | GPU ms, overdraw | GPU ≤ 8 ms (Mid) |
| P5 Restart spam | 20 restarts in 30 s | Load ms, GC alloc, memory growth | p95 < 300 ms. Memory growth < 5 MB after 20 restarts. |
| P6 Menu/Map | Scroll the world map, open Bow Forge with the turntable | UI rebuild ms, batches | 60 FPS (Mid) |
| P7 Cold start | Kill app → launch → Home | Boot ms (`app_open.boot_ms`) | ≤ 4 s Mid, ≤ 6 s Low |
| P8 Soak | 30 min autoplay (IntendedSolutionRunner loop over all levels) | Memory trend, thermal state, frame drift | No leak trend. FPS ≥ 55 avg on Mid after 30 min. |
| P9 Thermal | 20 min continuous play on Low | FPS over time, `quality_tier_changed` | Stays ≥ 28 FPS or the tier guard steps down gracefully |
| P10 Download/install size | Release AAB/IPA | Store-reported size | ≤ 150 MB |

| Tool | Use |
|---|---|
| Unity Profiler (dev build, Autoconnect) | CPU/GPU/physics/GC per frame, deep profile for spikes |
| Profile Analyzer | Compare captures before/after changes (median/p95) |
| Memory Profiler package | Snapshots: textures, meshes, audio, leaks across restarts |
| Frame Debugger | Batches, overdraw, SRP Batcher compatibility |
| Xcode Instruments (Time Profiler, Allocations, Metal System Trace, Energy) | iOS native CPU/GPU/thermal |
| Android GPU Inspector / Perfetto / Android Studio Profiler | Android GPU counters, system trace, memory |
| `DevOverlay` (`AB_DEV`) | On-device FPS, physics ms, bodies awake, debris, arrows |

**Process:** PLAT captures a baseline per milestone gate. Results go in `docs/qa/perf/<date>_<build>.md` with a table per scenario/device. A regression > 10% vs the previous gate → P1 bug.

## 8. Crash and memory test plan

| Test | Procedure | Pass |
|---|---|---|
| Background/resume | Home button mid-draw, mid-flight, mid-collapse, on the Win panel, during an ad, during the IAP sheet. Resume after 5 s and after 10 min. | No crash. Draw cancelled (no arrow spent). Physics resumes paused → continues. Session rules (`06` §11) correct. The save was written on pause. |
| Interruptions | Incoming call, alarm, notification shade, Control Center, low-battery dialog, headphones unplug, Bluetooth audio switch | Audio ducks/resumes. Game pauses (Pause panel shown). No stuck input. |
| Ad interruption | Rewarded/interstitial shown → background the app during the ad → resume | No duplicate reward. No lost reward if the SDK reports completion. Game state intact. |
| Low memory | iOS: Instruments memory pressure/simulated warning. Android: `adb shell am send-trim-memory <pkg> RUNNING_CRITICAL`, plus a background kill with Developer Options "Don't keep activities". | `Application.lowMemory` handler unloads unused assets. A cold restore after a kill returns to Home with the save intact. |
| Process kill during save | Kill the app at 100 random points while spamming level clears (scripted via adb) | The save always loads (main or `.bak`). No progress loss beyond the last clear. |
| Soak | P8 above, plus 2 h idle on Home | No crash, no leak |
| Orientation/lock | Rotate the device, enable rotation lock, split screen (Android), Slide Over (iPad) | Portrait locked. Split screen either disallowed or handled without crash. |
| Storage full | Fill the device storage, then clear a level | Save failure is logged non-fatal. No crash. Toast "Couldn't save progress" (dev wording TBD). |
| Clock change | Change the date forward/back | The daily regenerates. No crash. No negative timers. |
| Crash reporter verification | Dev menu "Force crash" / "Force non-fatal" (closed-test builds) | Appears in the vendor dashboard symbolicated (IL2CPP symbols uploaded) |

**Closed test KPI:** crash-free sessions ≥ 99.5%. ANR rate (Android vitals) below the Play "bad behaviour" threshold — *verify the current threshold values in Play Console*.

## 9. Accessibility checks

| Check | Pass condition |
|---|---|
| Colour not the only signal (§8) | Required objectives have crest shape/icon + strong outline. Protected objects have a purple outline **and** a shield icon. Verified with colour-blind simulation (protanopia, deuteranopia, tritanopia — e.g. Unity's color vision simulation or screenshot filters). |
| Colour-assist outlines toggle | ON adds high-contrast outlines to objectives, protected objects, ropes and interactive props. Readable on all 3 world palettes. |
| Reduced particles | Halves/limits particles (≥ 50% fewer) and hides non-essential ambient FX. Outcomes stay readable. |
| Reduced motion | Disables camera shake, slow-mo focus zoom and UI bounce overshoot. Hit-stop is kept but capped at 40 ms (D-057). |
| Haptics toggle | OFF = zero vibration calls (verified with a haptics log in dev). |
| Audio | Music and SFX sliders independent. Every critical event also has a visual cue (no audio-only info). |
| One-handed input (§8) | The full game is playable with one thumb in the lower 55% of the screen. No multitouch required. HUD buttons ≥ 48×48 dp (Android) / 44×44 pt (iOS). |
| Text | TMP minimum 14 pt on the reference resolution. Contrast ≥ 4.5:1 for body text, ≥ 3:1 for large text and icons. |
| Safe areas | No HUD or button under the notch, Dynamic Island, punch-hole or home indicator, on every device in §6. |
| Screen reader | Out of MVP scope (game is visual-physics). Menus have logical button order and labels for future support. Logged as post-MVP. |
| Timing | No timed input requirements other than the optional moving-shield timing levels. Those have wide windows (§5.1 timing jitter). |

## 10. Monetisation checks

| Check | Method | Pass |
|---|---|---|
| No interstitial before 10 min lifetime play | Fresh install, play fast (clear ≥ 6 levels in < 10 min) | Zero `interstitial_shown`. `interstitial_suppressed(reason=grace)` in the dev CSV. |
| Never after a fail | Fail → Retry → Win → Next; and Fail → Home | No interstitial on fail transitions. Only the Win → Next/Home trigger is eligible. |
| ≥ 3 levels between | Clear 3, 4, 5... levels after the grace period | Intervals respected; also the 120 s cooldown |
| Remove Ads | Buy `remove_ads` in the sandbox → play 10 levels | Zero interstitials. Rewarded offers still appear and work. |
| Rewarded +1 arrow rules | Scripted states: protected lost, 2 remaining without flag, daily challenge, L1–3, ad not loaded, second fail in the same attempt | Offer hidden in each case. Shown only per `06` §7.1. |
| Bonus-arrow star cap | Clear with the bonus arrow | 1★ (or previous best kept). `bonus_arrow_used = true`. |
| 2× coins never blocks Next | Tap Next while the ad loads/plays | Next always works. Base coins already credited. |
| Test ads only in dev | Dev/closed-test builds configured with test ad units / test devices | No live ads served to internal testers. `BuildConfig.adsTestMode` verified in each build profile. **Release build check** fails if test mode is on in Release, or live units in Dev. |
| IAP sandbox | iOS sandbox tester + Play licence testers | Purchase, cancel, failure, pending (Android), interrupted purchase (kill app mid-purchase) → grant on next launch |
| Restore | Reinstall → Settings ▸ Restore | `remove_ads` + starter cosmetics restored. Starter coins granted once per install (`06` §8). |
| Price display | Different store countries/currencies in the sandbox | Localised price strings. No hard-coded prices. |
| Consent gating | EEA test (UMP debug geography) and non-EEA; deny all / accept all | No analytics/ads init before resolution. Denied → non-personalised ads, analytics adapter disabled. Settings ▸ Privacy choices reopens the form. |
| ATT | iOS: Allow / Ask Not to Track | Ads work in both cases. No IDFA when denied. |
| Analytics dictionary | QA plays the FTUE + a fail + a win + a purchase with `DebugAnalyticsService` | CSV events/params exactly match `06` §11 (scripted diff check) |

## 11. Offline / poor-network behaviour

Principle: **the game is 100% playable offline.** Network-dependent features degrade silently.

| Scenario | Expected behaviour |
|---|---|
| First launch fully offline | Consent update fails → conservative defaults (`06` §9). Remote config uses `RemoteConfigDefaults`. Analytics queued/disabled. Ads not loaded → no offers shown. Game playable. Consent retried on the next online launch. |
| Offline mid-session | No change to gameplay. Rewarded offers hidden (not ready). Interstitials skipped. |
| Slow network at boot (3G/lossy, 2 s+ latency) | Boot never waits > 3 s for consent or > 2 s for remote config (timeouts). Home appears ≤ 6 s on Low. |
| Ad starts loading, network drops | Load fails silently. `rewarded_offer_failed` logged only if the player tapped. No spinner deadlock. |
| Network drops during a rewarded ad | Per SDK completion callback: grant only on completion. No duplicate grant on retry. |
| IAP offline | The store reports unavailable → the button shows "Store unavailable" (disabled). No crash. Pending purchases are processed when back online. |
| Remote config fetched with invalid values | Clamped to safe ranges (`06` §14) |
| Captive portal / DNS failure | Same as offline |
| Airplane-mode toggle during Boot | No hang. Graceful fallbacks. |

Test tools: device airplane mode, iOS Network Link Conditioner, Android emulator network throttling, or a proxy (e.g. Charles) for throttling/blocking specific SDK hosts.

## 12. Store-submission checklist

### 12.1 Shared
- [ ] App name / title, subtitle, short and long description — **original copy**; trademark search done (Q-02)
- [ ] App icon original (1024×1024 master)
- [ ] Screenshots and preview video captured from real gameplay; **reviewed against the reference game for originality** (P-05)
- [ ] Privacy policy URL live (`06` §9); support URL/email
- [ ] Age rating questionnaire (IARC for Google; Apple age rating) — no gambling, mild cartoon "fantasy violence" (arrows vs. objects/dummies) to be answered honestly
- [ ] Ads declared ("Contains ads") and IAPs configured with matching product IDs (`remove_ads`, `starter_pack`)
- [ ] Release build: `AB_DEV`/`AB_CHEATS` off, `BuildConfig = Release`, test ads off, placeholders banned (D-042 build check), `LevelValidator` clean, solvability suite green
- [ ] Version name/code incremented (`BuildVersioning`)

### 12.2 Apple App Store
- [ ] Built with the Xcode/iOS SDK version Apple currently requires — **verify at submission**
- [ ] Bundle ID final (`ProjectSetup.BundleId` reviewed — currently `com.attila.arrowbuster`)
- [ ] `PrivacyInfo.xcprivacy`: our app manifest + each SDK's manifest present (required-reason APIs declared) — **verify every SDK ships one**
- [ ] App Privacy "nutrition labels" filled from §13 data inventory
- [ ] `NSUserTrackingUsageDescription` (if ATT is used), e.g. *"Your data will be used to show you more relevant ads. Arrow Buster works the same either way."* — final wording by OWNER
- [ ] `SKAdNetworkItems` from the chosen ad network list in Info.plist — **verify the current list from the vendor**
- [ ] Restore Purchases button present (Settings)
- [ ] Screenshots: 6.9" iPhone (1320×2868 portrait) or 6.5" (1284×2778 / 1242×2688) set; iPad 13" (2064×2752) if universal — **verify the currently required sizes in App Store Connect**
- [ ] TestFlight external beta review passed before submission
- [ ] Export compliance (encryption): standard HTTPS only → exempt; answer accordingly

### 12.3 Google Play
- [ ] **Target API level** meets the current Play requirement for new apps/updates — **verify in Play Console at M10** (`AndroidTargetSdkVersion` is "Auto" = highest installed; confirm the SDK installed matches the requirement)
- [ ] AAB signed with the upload key (keystore outside the repo); Play App Signing enabled
- [ ] 64-bit only (ARM64, already set). 16 KB page-size compatibility for native libs — **verify Unity and the SDKs comply with current Play requirements**
- [ ] Data safety form from the §13 inventory; Ads declaration; content rating (IARC); target audience 13+ (not designed for children)
- [ ] Advertising ID permission (`com.google.android.gms.permission.AD_ID`) declared consistently with the ads SDK and the data-safety answers
- [ ] `VIBRATE` permission (haptics, D-032); no other dangerous permissions
- [ ] Store listing graphics: icon 512×512, feature graphic 1024×500, phone screenshots (portrait, min 320 px / max 3840 px per side, 9:16 recommended) — **verify current specs**
- [ ] Closed testing track run with ≥ the tester count/duration Play currently requires for new personal developer accounts before production access — **verify the current rule** at M5 (AB-162); if it applies, start the track in M8 (AB-166, D-079)
- [ ] Pre-launch report reviewed (crashes, accessibility, security warnings)

## 13. Privacy-policy data inventory

To be finalised after vendor selection (M8). Rows marked *TBD vendor* are placeholders.

| Source / SDK | Data | Purpose | Linked to user? | Tracking (ATT sense)? | Collected when |
|---|---|---|---|---|---|
| Arrow Buster (local save) | Progress, coins, settings, consent flags, install GUID | Gameplay | Stored on device only | No | Always (not transmitted) |
| Analytics (*TBD vendor*, e.g. Firebase/GA4) | App-instance ID, gameplay events (`06` §11), device model/OS, app version, coarse location (from IP), session data | Product analytics, balancing | Pseudonymous ID | No (unless linked with ad IDs — avoid) | After consent where required |
| Crash reporting (*TBD*, e.g. Crashlytics) | Crash stack traces, device state, OS, installation UUID, breadcrumbs (event names, level id) | Stability | Pseudonymous | No | Legal basis TBD by OWNER (`06` §9) |
| Remote config (*TBD*) | Installation ID, app version, country | Config delivery | Pseudonymous | No | Boot |
| Ad mediation + networks (*TBD*) | IDFA (if ATT granted)/GAID (subject to consent), IP, device info, ad interactions, coarse location | Ad serving, frequency capping, attribution | Yes (ad ID) | Yes if personalised | After consent |
| Google UMP | Consent string (TCF), region | Consent management | — | No | First launch / Settings |
| App Store / Google Play (via Unity IAP) | Purchase history, receipts | Purchases and restore | Store account (handled by stores) | No | On purchase |
| Unity Engine runtime | *Verify* whether any Unity services telemetry is enabled (e.g. Unity Analytics module is disabled in Services settings) | — | — | — | Verify at M8 |

**Retention and deletion:** each vendor's retention settings are configured to the minimum useful (e.g. 14 months for analytics) — *verify the available options*. Deletion requests go through the support email; the policy explains that local data is removed by uninstalling.

## 14. Beta / closed-test plan (M10; the Play closed track may start in M8 per D-079)

| Item | Plan |
|---|---|
| Channels | iOS **TestFlight** (internal → external group). Google Play **internal testing** → **closed testing** track. |
| Build | Closed-test configuration: `AB_DEV` on (DevOverlay hidden by default, opened by a 5-tap gesture), test ads, crash reporting on, analytics to a **separate test property/project** |
| Cohort | 20–40 testers outside the dev circle (friends-of-friends, casual puzzle players, mix of iOS/Android, ≥ 30% on Low-tier Android). Meet Play's minimum closed-test requirement (§12.3). |
| Duration | 14 days minimum (or Play's required duration if longer) |
| Instructions | "Play naturally, no instructions." A feedback link in Settings (form). In-game survey after L10 and after W1 clear (2–3 questions). |
| Survey | Fun (1–5), clarity of goals (1–5), fairness of physics (1–5), favourite/least favourite level, would-recommend (0–10), ads felt (too many / fine / didn't notice) |
| Success metrics | Crash-free sessions ≥ 99.5%. L1 completion ≥ 95%. ≥ 80% of testers who start W1 complete it without help (§15). Median retries per normal level 1–3. No level with ≥ 20% quit before first shot. Fairness score ≥ 4.0/5. Zero S1 bugs open. |
| Outputs | `docs/qa/closed-test/<date>_report.md` + prioritised fix list → funnel fixes in M10 |

## 15. Soft-launch success/failure criteria (post-M10)

Soft launch in 1–3 test markets (e.g. markets chosen for English proficiency and cost — OWNER decision). Minimum sample before deciding: ~1,000 installs per platform, or 14 days.

| Metric | Scale (proceed to wider launch) | Iterate (fix and re-test 2–4 weeks) | Kill / rethink |
|---|---|---|---|
| L1 completion (§13) | ≥ 95% | 85–95% | < 85% after 2 iterations |
| W1 completion among L5 reachers (§13) | ≥ 45% | 30–45% | < 30% after 2 iterations |
| Median retries per normal level (§13) | 1–3 | 3–5 on > 5 levels | Systemic > 5 |
| Level abandonment flags (§13) | ≤ 2 levels flagged | 3–8 flagged | > 8 flagged |
| Crash-free sessions | ≥ 99.8% | 99.0–99.8% | < 99.0% (blocks any launch) |
| D1 retention | **Benchmark vs channel/CPI (§13)**. Provisional bands for casual puzzle, to be replaced by channel data: ≥ 40% | 30–40% | < 25% |
| D7 retention | Provisional: ≥ 12% | 7–12% | < 5% |
| Playtime/DAU | ≥ 15 min | 8–15 min | < 6 min |
| LTV vs CPI | LTV (D30 projection) ≥ CPI | 0.6–1.0× CPI | < 0.5× CPI after monetisation fixes |

Rule (§13): **do not optimise ad density before the core-fun metrics (L1, W1, retries, D1) are in the Scale column.**

## 16. Release gates

Each gate is a checklist that **INT** signs off in `docs/qa/gates/<gate>.md`, with evidence links. The gate fails if any **bold** item fails.

| Gate | Milestone | Checklist |
|---|---|---|
| **G-M1 Feel & Physics** | End of M1 | **Physics spike gates PG-1…PG-6 met ([`03` §1.1](03_PHYSICS_AND_OBJECTS.md), D-004).** **Preview parity test green.** Feel playtest ≥ 5 testers. Spec vs Snappy chosen (D-036). Android device build runs ≥ 55 FPS on the Mid device in the crate sandbox. |
| **G0 Vertical Slice** | End of M3 | **5 VS levels pass solvability layers 1–3.** **VS playtest go/no-go thresholds VS-G1…VS-G8 met per the decision rules in [`11` §8.1](11_VERTICAL_SLICE.md#81-go--no-go-thresholds)** (5–10 casual players, no instructions, §16). HUD/Win/Fail polished. SFX/VFX/haptics present. Analytics CSV matches the dictionary for loop events. Restart < 1 s on device. 60 FPS Mid / 30 FPS Low on VS levels. **Go/no-go decision recorded in the decision log.** |
| **G1 World 1** | End of M5 | **L1–20 Final status (3-layer solvability).** First shot < 15 s on a fresh install. W1 playtest: ≥ 80% complete W1 without help (§15). Save/progression tests green. Art direction locked (documented). No S1/S2 open. |
| **G2 Content Complete** | End of M7 | **All 60 levels at `Final` status (`04` §1) with green bot solvability (layers 1–2)**; W2/W3 art in. The M9 balance pass may still retune quivers/par. Interaction matrix tests green. SDK spike build boots on Android + iOS. Perf P2 on Low ≥ 28 FPS for set pieces. |
| **G3 Feature Complete** | End of M8 | **All MVP features in (§14 must-ship list `00` §5.1).** Ads/IAP/consent/analytics/crash/RC functional with test ads/sandbox. Monetisation checks (§10) pass. Save schema frozen at v1 (or migrations tested). |
| **G4 Release Candidate** | End of M9 | **Full regression pass. All 60 levels Final. Perf budgets met on P0 devices. Accessibility checks pass. Crash-free ≥ 99.5% in internal soak.** Zero S1/S2 open. LTS decision made (D-043). Store checklist drafted. |
| **G-Release Submission** | M10 | **Closed-test success metrics (§14) met or waived with OWNER sign-off.** Store checklist §12 complete. Privacy policy live and inventory §13 final. Release build checks (test ads off, placeholders banned, cheats off). OWNER final approval. |

## 17. Bug severity, priority and triage

| Severity | Definition | Examples |
|---|---|---|
| **S1 Blocker** | Crash, data loss, progression blocker, purchase not granted, compliance breach | Save wiped; level unwinnable; ad before consent; crash on boot |
| **S2 Major** | Core loop broken or unfair in a way players notice; major perf failure | Intended solution fails on device; preview mismatch; < 45 FPS on Mid; interstitial after a fail |
| **S3 Minor** | Noticeable defect with a workaround | Wrong SFX, UI overlap on one device, debris clipping |
| **S4 Trivial** | Cosmetic polish | Typo, 1-px misalignment |

| Priority | Meaning |
|---|---|
| **P0** | Fix now. Blocks the current gate/build. |
| **P1** | Fix within the current milestone |
| **P2** | Fix before release (G4) |
| **P3** | Backlog / post-MVP |

**Triage process:** QA logs bugs in `docs/qa/BUG_LOG.md` (or GitHub Issues once the repo is on GitHub, labels `bug`, `S1–S4`, `P0–P3`, role ID). Template: ID `BUG-###`, title, build, device/OS, steps, expected, actual, frequency (x/10), evidence (video/log/profiler capture), severity, suggested owner. Triage runs twice a week (QA + PO + INT): assign the owner role, priority and milestone. S1 = same-day triage. Physics bugs need a minimal repro sandbox scene (`Scenes/Sandbox/Sandbox_QA_BUG###.unity`) or a recorded shot (`IntendedShot` list) so the bot can reproduce them. A fixed bug is verified by QA on the original device class before it is closed. Regressions get a test added.

## 18. Where QA artefacts live

```
docs/qa/
  REGRESSION_CHECKLIST.md        living checklist (QA-owned)
  BUG_LOG.md                     until GitHub Issues is adopted
  DEVICE_MATRIX.md               §6 with actual device inventory + owner device
  test-runs/<date>_<build>.md    manual/regression run results
  playtests/<date>_<topic>.md    playtest notes + survey results
  perf/<date>_<build>.md         performance captures summary (raw captures stored outside git or LFS)
  gates/<gate>.md                gate sign-off checklists with evidence
  closed-test/<date>_report.md   closed-test report
docs/levels/W#_L##.md            per-level intended solution + test log (template in 04)
Assets/_Project/Tests/EditMode   unit tests (+ Fixtures/)
Assets/_Project/Tests/PlayMode   PlayMode suites (+ Golden/ hashes)
```

Files are created by the first task that needs them (QA owns `docs/qa/`).
