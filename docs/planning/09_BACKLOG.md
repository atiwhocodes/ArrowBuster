# 09 — Backlog

> Owner: Product Owner (`PO`) for priority; Lead Integrator (`INT`) for ordering and dependencies. Status: Draft v1 — 2026-10-09.
> Milestones and schedule: [`08_PRODUCTION_ROADMAP.md`](08_PRODUCTION_ROADMAP.md). Names: [`01_TECHNICAL_ARCHITECTURE.md`](01_TECHNICAL_ARCHITECTURE.md). Decisions: [`10_DECISION_LOG.md`](10_DECISION_LOG.md).
> Task delegation format: [`../agents/TASK_TEMPLATE.md`](../agents/TASK_TEMPLATE.md). Each ticket below becomes one agent task.

---

## 1. Label legend

| Label family | Values | Meaning |
|---|---|---|
| **Discipline / role** | `PO` `ARCH` `CORE` `PHYS` `PROPS` `SYS` `UI` `LEVEL` `ART` `PLAT` `MON` `QA` `INT` `OWNER` | Owning specialist (first listed = accountable; `+` = contributor) |
| **Priority** | `P0` must ship · `P1` should ship (cut list candidate) · `P2` could ship (cut by default if late) | MVP priority |
| **Milestone** | `M1` … `M10` | Target milestone (D-033) |
| **Risk** | `R-H` high · `R-M` medium · `R-L` low | Technical/schedule risk of the ticket itself |
| **Dependency** | `dep:AB-###` | Must be merged to `main` first |
| **Complexity** | `S` ≤ 0.5 d · `M` 1–2 d · `L` 3–5 d · `XL` > 1 wk (must be split before Ready) | Effort for one dev + agents |
| **Type** | `story` (player-facing) · `tech` · `content` · `qa` · `art` · `release` | Ticket kind |

Label string format on a ticket: `[CORE][P0][M1][R-M][dep:AB-006][M][tech]`.

---

## 2. Definition of Ready (DoR)

A ticket may be picked up by an agent only when all of these are true:
1. It has a unique `AB-###` id, a title and an accountable role.
2. Linked `mvp.md` § and planning-doc section; open design questions resolved or defaulted by a decision (`D-0xx`).
3. **Acceptance criteria are measurable** (numbers, states, test names) — no "feels good" without a test protocol.
4. Files/folders it will touch are listed and are inside the role's allowed paths (`AGENT_SYSTEM.md`). Shared files are named and free in `FILE_LOCKS.md`.
5. All `dep:` tickets are merged to `main`.
6. Size is S/M/L (XL is split).
7. Test approach is named (EditMode / PlayMode / manual protocol / device).

## 3. Definition of Done (DoD)

1. Acceptance criteria met and demonstrated (editor Play mode via MCP and/or device as the ticket requires).
2. Compiles with **zero errors and zero new warnings** in the Unity Console (read via MCP after recompile).
3. Required EditMode/PlayMode tests written and green. Full existing suite still green.
4. Follows `01` conventions: names, folders, `[SerializeField] private`, XML docs on public APIs, no magic numbers, no `UnityEngine.Random` in outcomes, static reset, zero per-frame GC in hot paths.
5. No edits outside the allowed paths. File locks released.
6. Data/prefab changes pass `LevelValidator` / `PrefabValidator` (once those exist).
7. Handoff report written (`HANDOFF_TEMPLATE.md`). Decision log updated if a decision was made. The `08` / CLAUDE.md milestone checklist updated by INT on merge.
8. INT merged to `main` via the integration checklist. A device build is required where the ticket says "device".

---

## 4. Epics (prioritised)

| Epic | Name | Priority | Owner | Milestones | Tickets |
|---|---|---|---|---|---|
| E-01 | Project foundation, tooling & CI | P0 | ARCH / INT | M1, M3, M4 | AB-001–003, 015, 047, 068, 158 |
| E-02 | Bow & aiming | P0 | CORE | M1 | AB-004–008 |
| E-03 | Arrows & ballistics (5 arrow types) | P0 | CORE | M1, M4, M6, M7 | AB-005, 006, 009, 011, 049, 088, 089, 105, 106 |
| E-04 | Physics, materials & destruction | P0 | PHYS | M1, M4, M6, M7, M9 | AB-002, 010–014, 048, 050, 058, 092, 094, 104, 141, 160 |
| E-05 | Objectives, protected objects & hazards | P0 | PROPS | M2, M4 | AB-018, 021, 052–057 |
| E-06 | Interactive props | P0 (SpringPlate P2) | PROPS | M2, M4, M6, M7 | AB-020, 051, 085–087, 090–093, 105, 107, 108 |
| E-07 | Game loop & level flow | P0 | CORE | M2 | AB-016, 017, 019 |
| E-08 | Level pipeline & tooling | P0 | LEVEL | M2, M4 | AB-016, 025, 059–062 |
| E-09 | Content — World 1 Greenwood Range | P0 | LEVEL | M2–M5 | AB-025, 046, 064–066, 082–084 |
| E-10 | Content — World 2 Sunscar Canyon | P0 | LEVEL | M6 | AB-100–103 |
| E-11 | Content — World 3 Frostspire Keep | P0 | LEVEL | M7 | AB-114–116, 118 |
| E-12 | UI/UX & onboarding | P0 | UI | M2, M3, M5, M8, M9 | AB-023, 067, 076, 077, 079–081, 119, 122, 124, 125, 136, 143, 147, 164 |
| E-13 | Art, VFX, audio & haptics | P0 | ART | M3, M5–M8 | AB-026–045 (VS), 069–075, 096–099, 110–113, 123, 137, 158, 161 |
| E-14 | Progression, economy & cosmetics | P0/P1 | SYS | M2, M5, M8 | AB-022, 078, 120, 121, 124, 126, 130 |
| E-15 | Monetisation, consent, analytics, crash, RC | P0 | MON | M2, M7, M8, M10 | AB-024, 117, 127–129, 131–135, 148, 154, 163 |
| E-16 | Performance & platform | P0 | PLAT | M1, M9, M10 | AB-047, 138–140, 145, 146, 149, 152, 165 |
| E-17 | QA & playtesting | P0 | QA | all | AB-014, 062, 063, 084, 095, 103, 109, 118, 144, 148, 153, 159 |
| E-18 | Release & store | P0 | PLAT / PO / OWNER | M5, M8, M10 | AB-150–157, 162, 166 |

---

## 5. User stories (player-facing)

| ID | Story | Acceptance criteria | Epic | Tickets |
|---|---|---|---|---|
| US-01 | As a player, I can pull back anywhere near the bow and release to shoot, so aiming feels natural with one thumb. | Press in the aim zone starts a draw. Releasing above `minFirePower` fires. Releasing below it cancels without spending an arrow. Works with mouse and touch. | E-02 | AB-007, AB-009 |
| US-02 | As a player, I see a dotted arc while drawing that shows exactly where my arrow will go. | Preview shown only while drawing. Parity with flight ≤ 1 mm over 3 s. Ends at the first hit with an impact ring (D-040). Length scales with `trajectoryPreviewScale`. | E-02 | AB-006, AB-008 |
| US-03 | As a player, arrows react believably to what they hit (stick in wood, glance off stone/metal, cut ropes, pop balloons). | Outcomes per the `03` material × arrow table. A lodged arrow vibrates 0.2 s. Ropes and balloons are passed through. | E-03 | AB-011 |
| US-04 | As a player, I know what I must destroy and what I must protect. | Red crest/icon + outline on required objects; purple outline + icon on protected objects. Objective icons in the HUD cross out on clear. Colour is never the only signal. | E-05, E-12 | AB-018, AB-023 |
| US-05 | As a player, a level ends fairly and quickly. | Win after 0.75 s calm (cap 3 s). Out-of-arrows fail after 2 s calm (cap 5 s). Protected loss fails immediately with a reason. Soft-lock toast appears after 2 s calm. | E-07 | AB-019 |
| US-06 | As a player, I can retry instantly. | 1 tap, any state. Restart < 1 s on device. No confirmation dialog. | E-07 | AB-016, AB-019, AB-023 |
| US-07 | As a player, I earn up to 3 stars by using fewer arrows. | ≤ par 3★, par+1 2★, else 1★. Bonus-arrow clear capped at 1★. Best kept. | E-07, E-14 | AB-017, AB-022 |
| US-08 | As a new player, I'm shooting within 15 seconds and learn without a tutorial wall. | First launch goes straight to L1 with a ghost-hand hint. Median ≤ 15 s to the first shot. Callouts only in designated levels. | E-12 | AB-079, VS tickets |
| US-09 | As a player, special arrows appear as curated surprises that unlock new tricks. | Reveal card on first appearance (L13/L30/L36/L47). Quiver order shown in the HUD. Never purchasable. | E-03, E-12 | AB-049, AB-067, AB-088, AB-089, AB-105 |
| US-10 | As a player, I progress through a world map and unlock new worlds. | 20 nodes per world with stars. The next world unlocks at 15/20 clears. Locked-world teaser. `world_unlocked` fired. | E-12, E-14 | AB-076, AB-078, AB-119 |
| US-11 | As a player, I earn coins and spend them only on cosmetics. | Coin values per D-026. Bow Forge grid with equip/buy. Cosmetics never change physics or aim. | E-14 | AB-120–AB-123 |
| US-12 | As a player, I can come back daily for a small challenge. | Unlocks after L10. 3 cleared levels per local day. Quiver = gold par. 100 coins once per day. | E-14 | AB-124 |
| US-13 | As a player who failed with one objective left, I can optionally watch an ad for one more arrow. | Offered max once per attempt, only if the D-024/§9 viability rule holds. Never on a protected-loss fail. Retry stays the dominant button. | E-15 | AB-127–AB-129 |
| US-14 | As a paying player, I can remove interstitial ads and restore my purchase. | `remove_ads` disables interstitials permanently. Rewarded stays opt-in. Restore works on a reinstall (iOS + Android). | E-15 | AB-131 |
| US-15 | As a player in the EEA/UK (or on iOS), I'm asked for consent and can change it later. | UMP form before any ads/analytics init. ATT after UMP on iOS. A Settings → Privacy entry re-opens the options. | E-15 | AB-132 |
| US-16 | As a player with accessibility needs, I can reduce particles/motion, turn off haptics and enable colour-assist outlines. | All toggles persist and take effect immediately. Colour-assist adds pattern/icon outlines to every material and objective class. | E-12 | AB-080, AB-143 |
| US-17 | As a player, every impact sounds and feels distinct per material. | SFX per material (wood crack, stone thunk, ice chime, rope twang, metal ping). Hit-stop 40–70 ms on meaningful direct hits. Haptics per §8. | E-13 | VS tickets, AB-073, AB-098, AB-112 |
| US-18 | As a player, I can chase Bullseye medals for extra mastery. | A marked weak point awards a medal on a win with a direct hit. Shown on the map node and as a collection count. | E-05, E-14 | AB-057, AB-125 |

## 6. Technical stories

| ID | Story | Acceptance criteria | Tickets |
|---|---|---|---|
| TS-01 | As a developer, I can press Play in any scene and get working services. | `ServiceInstaller.EnsureInstalled()` installs defaults. Gameplay runs its `_editorFallbackLevel`. | AB-003 |
| TS-02 | As a developer, static state never leaks between play sessions (domain reload disabled). | `StaticReset` used by every static. A PlayMode test enters play twice and asserts clean registries/events. | AB-003 |
| TS-03 | As a developer, arrow flight and preview share one solver. | `BallisticSolver` is pure. A parity EditMode test is green. | AB-006 |
| TS-04 | As a developer, physics layers and settings are applied by script, not by hand. | `LayerSetup` + `PhysicsSetup` menu items are idempotent. An EditMode test asserts the matrix and settings. | AB-002 |
| TS-05 | As a developer, vendor SDKs never leak into game code. | Only `ArrowBuster.Integrations` references vendor assemblies. `ArchitectureRulesTests` green. Project compiles with SDKs removed. | AB-068, AB-117, AB-128 |
| TS-06 | As a developer, every level is validated before it ships. | `LevelValidator` runs from the menu, in tests and before every build; build aborts on errors. | AB-059, AB-047/AB-152 |
| TS-07 | As a developer, saves never corrupt and always migrate. | Atomic write + `.bak`. Migration tests from v1. Corrupt-file test falls back to `.bak`. | AB-022, AB-126 |
| TS-08 | As a developer, the game picks a quality tier and holds the frame budget. | `QualityTierSelector` + runtime guard. Matrix results logged. | AB-138, AB-139 |
| TS-09 | As a developer, ad rules are enforced by tested pure logic. | `AdPolicy` tests cover every D-025 clause and §9 rule. | AB-127 |

## 7. Content-production stories

| ID | Story | Acceptance criteria | Tickets |
|---|---|---|---|
| CS-01 | VS levels VS-01..05 (W1_L01/04/07/10/16) graybox → final | Per `11` level specs. Bot green with jitter. Playtest targets met. | AB-025, AB-026–045 |
| CS-02 | W1 graybox batch 0 (L02, L03, L05, L06, L08) | Mechanics per the `04` plan. Validator 0 errors. Intended shots recorded. | AB-046 |
| CS-03 | W1 graybox batches A–C (L09, L11–15, L17–20) | Includes Heavyhead intro L13 and the L20 boss (3 watchtowers, powder barrel, sleeping fox, ≤ 4 arrows). | AB-064–AB-066 |
| CS-04 | W1 final art + balance (L1–20) + solution docs | Status Final. `docs/levels/W1_Lxx.md` complete. Device bot green. | AB-082, AB-083 |
| CS-05 | W2 graybox L21–30 and L31–40 | Balloons L21–23, oil L24–26, shields L27–29, Split L30–32, counterweights/boulders L33–35, fire combos L36–39 (Fire arrow L36), L40 boss siege gate (5 arrows). | AB-100, AB-101 |
| CS-06 | W2 final art + balance | As CS-04 for W2. | AB-102 |
| CS-07 | W3 graybox L41–50 and L51–60 | Ice L41–43, wind L44–46, Bounce L47–49, portals L50–52, moving ice L53–55, multi-system L56–59, L60 finale. | AB-114, AB-115 |
| CS-08 | W3 final art + balance | As CS-04 for W3. | AB-116 |
| CS-09 | Environment kits ×3 | ~25 modular pieces per world, palette texture, pillarbox scenery ±3 m, blurred background cards (D-003). | VS art tickets, AB-070, AB-096, AB-110 |
| CS-10 | Cosmetics | 5 bow skins (Oak Ranger, Moonwood, Ember, Frostglass, Royal Violet), 4 trails, quiver badges. | AB-123 |

## 8. QA stories

| ID | Story | Acceptance criteria | Tickets |
|---|---|---|---|
| QS-01 | Physics stability spike & feel gate | D-004 gates measured and reported. ≥ 5 testers on device compare presets. | AB-014 |
| QS-02 | Solvability bot | Replays intended shots on the `07` §5.1 grid (δθ = atan(0.4 m/d) ≥ 0.5°, δp ±0.03, 9 samples per shot, D-074). Runs over a `LevelCatalog`. JUnit-style report. | AB-025, AB-062 |
| QS-03 | Idle stability suite | Every level: 3 s with no shots, < 1 cm drift, no objective/protected state change. | AB-062 |
| QS-04 | Interaction matrix suites per world | One PlayMode test per non-empty cell of the `03` interaction matrix. | AB-063, AB-095, AB-109 |
| QS-05 | Playtest rounds (VS, W1, W2, W3, closed test) | Script from skill `gameplay-balance-playtest.md`. Report in `docs/qa/`. | VS, AB-084, AB-103, AB-118, AB-154 |
| QS-06 | Full regression + device solvability | All 60 on Mid Android + iPhone 11-class. 0 P0/P1 open. | AB-144 |
| QS-07 | Analytics verification | Every event/param in the `06` dictionary observed. No PII. | AB-148 |
| QS-08 | Monetisation compliance checks | No interstitial in the first 10 min or after a fail. Rewarded opt-in only. Remove Ads honoured. Restore works. | AB-127, AB-144 |

---

## 9. Recommended first 25 tickets (dependency order)

### AB-001 — Version control baseline
`[INT][P0][M1][R-L][S][tech]` · depends on: none (**OWNER approval required**)
- **Description:** Initialise git at the repo root, enable LFS, configure the UnityYAMLMerge driver for `.unity/.prefab/.asset`, verify `.gitignore`/`.gitattributes` (add LFS patterns for `.aif/.aiff/.mp4/.mov/.psb/.tif/.tiff` if missing), make the first commit on `main`, document the remote setup.
- **Acceptance criteria:** `git status` is clean after the commit. `git lfs ls-files` lists no binaries yet but `git check-attr -a` shows LFS on `.png`. `git config merge.unityyamlmerge.driver` points to the installed Unity `UnityYAMLMerge.exe`. `Library/`, `Temp/`, `*.csproj` are not tracked.
- **Files:** `.gitattributes`, `.gitignore`, `.git/config` (local), `docs/README.md` (git section — INT owns).
- **Tests:** manual checklist from skill `git-worktree-and-integration.md`.

### AB-002 — Project hygiene + layers + physics settings
`[ARCH+PHYS][P0][M1][R-M][M][tech]` · depends on: AB-001
- **Description:** Remove `com.unity.visualscripting`, `com.unity.collab-proxy`, `com.unity.ai.navigation`. Delete template leftovers (`Assets/Scenes/SampleScene.unity`, `Assets/TutorialInfo/`, `Assets/Readme.asset`, `Assets/InputSystem_Actions.inputactions`, `Settings/SampleSceneProfile.asset`). Create `ArrowBuster.Tests.PlayMode` asmdef. Add editor tools `LayerSetup` (D-048 layers + collision matrix) and `PhysicsSetup` (D-006 settings), plus `PhysicsLayers` constants. Verify the MCP package is excluded from player builds.
- **Acceptance criteria:** packages removed and the project compiles with 0 errors/warnings. Layers 6–16 named exactly as in D-048. Matrix matches `03` §2. Fixed Δt 0.0166667, max Δt 0.1, solver 8/2, enhanced determinism on. Both menu items idempotent.
- **Files:** `Packages/manifest.json` (ARCH lock), `ProjectSettings/TagManager.asset` + `DynamicsManager.asset` + `TimeManager.asset` (PHYS lock), `Scripts/Editor/Setup/LayerSetup.cs`, `PhysicsSetup.cs`, `Scripts/Runtime/Physics/PhysicsLayers.cs`, `Tests/PlayMode/ArrowBuster.Tests.PlayMode.asmdef`.
- **Tests:** EditMode `PhysicsSettingsTests` (asserts layers, matrix, timestep, solver).

### AB-003 — Core skeleton
`[ARCH][P0][M1][R-M][M][tech]` · depends on: AB-001
- **Description:** `Services` (static, typed properties, null-object defaults), `ServiceInstaller` (Boot + `EnsureInstalled()` editor fallback), `GameEvents` + `GameEventPayloads` (all events in `01` §10.2), `StaticReset`, `Log` (+ `LogCat`), `LevelClock`, `TimeScaleController` (sole `Time.timeScale` writer; pause > slow-mo > hit-stop, D-061), `CosmeticRandom`, `BuildConfig` SO, interfaces in `Services/` (`IAnalyticsService`, `IAdsService`, `IIapService`, `IConsentService`, `IRemoteConfigService`, `ICrashReporter`, `ISaveService`, `IHapticsService`, `IAudioService`) with Null/Mock implementations. Update `GameBootstrap` to call `EnsureInstalled()`.
- **Acceptance criteria:** pressing Play in any scene installs services. Entering Play twice leaves no stale subscribers (test). No `FindObjectOfType` used.
- **Files:** `Scripts/Runtime/Core/*`, `Scripts/Runtime/Services/*`, `Assets/_Project/Prefabs/Roots/AppRoot.prefab`, `Scenes/Boot.unity` (ARCH lock).
- **Tests:** EditMode `ServicesTests`, `StaticResetTests`, `TimeScaleControllerTests` (priority arbitration, unscaled restore). PlayMode `StaticResetTests`.

### AB-004 — Gameplay scene graybox
`[CORE][P0][M1][R-L][M][tech]` · depends on: AB-002, AB-003
- **Description:** `GameplayRoot.prefab` with `CameraRig` + `CameraFramer` (perspective FOV ≈ 30°, pitch ≈ 8°, fits the 10 m width; pillarbox rule D-041), graybox ground on `Environment`, directional light, `PlayBounds`, `LevelRoot` transform. Scene contains only `GameplayRoot`.
- **Acceptance criteria:** play-area width fully visible at 9:16, 9:19.5, 9:21. Height-fit at 3:4. Gizmo shows the design rect (10 × 17.8 m, origin bottom-centre, bow pivot ≈ (0, 1.5, 0)).
- **Files:** `Scripts/Runtime/Gameplay/CameraFramer.cs`, `Scripts/Runtime/Props/PlayBounds.cs`, `Prefabs/Roots/GameplayRoot.prefab`, `Scenes/Gameplay.unity` (CORE lock).
- **Tests:** EditMode `CameraFramerTests` (frustum math per aspect).

### AB-005 — GameplayTuning + ArrowDefinition + AD_Oak
`[CORE][P0][M1][R-L][S][tech]` · depends on: AB-003
- **Description:** `GameplayTuning` SO (draw mapping, renock 0.35 s, settle thresholds, hit-stop, preview base seconds and dot spacing). `ArrowDefinition` SO (type, speed range, gravity scale, mass, impulse scale, embed/ricochet params, wind response, behaviour kind, prefab, icon). Create `AD_Oak` and two tuning presets ("Spec" 1.5–2.5 s, "Snappy" 0.9–1.4 s) per D-036.
- **Acceptance criteria:** every field has `[Tooltip]`. Presets switchable via a DevOverlay toggle (after AB-015). No tuning constants in code.
- **Files:** `Scripts/Runtime/Gameplay/GameplayTuning.cs`, `Scripts/Runtime/Arrows/ArrowDefinition.cs`, `ScriptableObjects/Arrows/AD_Oak.asset`, `ScriptableObjects/Config/GameplayTuning_Spec.asset`, `_Snappy.asset`.
- **Tests:** EditMode `ArrowDefinitionTests` (validation ranges).

### AB-006 — BallisticSolver
`[CORE][P0][M1][R-M][M][tech]` · depends on: AB-005
- **Description:** pure static `BallisticSolver.Step(ref ArrowFlightState, float dt, IFlightEnvironment env)` (semi-implicit Euler; gravity × gravityScale + env wind acceleration), `IFlightEnvironment` with a null implementation (wind/portal hooks for M7), `Launch(AimState, ArrowDefinition)` → initial state, and a helper for flight-time estimation.
- **Acceptance criteria:** deterministic (same input → bit-identical output). Parity: the preview sampler and the projectile path are identical within 1 mm over 3 s. Flight time for a reference shot matches the active preset band.
- **Files:** `Scripts/Runtime/Arrows/BallisticSolver.cs`, `ArrowFlightState.cs`, `IFlightEnvironment.cs`.
- **Tests:** EditMode `BallisticSolverTests` (analytic comparison, determinism, parity, flight-time band).

### AB-007 — BowInputReader + DrawModel
`[CORE][P0][M1][R-M][M][story]` · depends on: AB-003, AB-005
- **Description:** `BowInputReader` polls `Pointer.current` in `Update` → `PointerSample`. It ignores presses over UI and secondary touches. `DrawModel` (pure) maps samples → `AimState {angleDeg, power01, isDrawing, canFire}` per D-018 (aim zone bottom 55% of the safe area, full draw 22% of screen height, `minFirePower` 0.15, cone 8°–172°). `BowController` raises `DrawStarted`/`DrawCancelled`/`DrawThresholdReached`.
- **Acceptance criteria:** mouse and touch both work. Cancel below the threshold spends nothing. Angle clamped. Behaviour is resolution-independent.
- **Files:** `Scripts/Runtime/Bow/BowInputReader.cs`, `DrawModel.cs`, `AimState.cs`, `BowController.cs`, `BowView.cs` (graybox), `Prefabs/Bow/Bow_Graybox.prefab`.
- **Tests:** EditMode `DrawModelTests` (zone, mapping, clamp, cancel, multiple resolutions).

### AB-008 — TrajectoryPreview
`[CORE][P0][M1][R-L][M][story]` · depends on: AB-006, AB-007
- **Description:** pooled dot renderer (40 dots) sampling `BallisticSolver` with the same dt. It stops at the first hit on the arrow cast mask (passing Rope/Portal), shows an impact ring (D-040), length = `previewBaseSeconds × LevelData.trajectoryPreviewScale`. Visible only while drawing.
- **Acceptance criteria:** 0 GC/frame while drawing. Dots match the flight path (visual + parity test). Hidden on release/cancel.
- **Files:** `Scripts/Runtime/Bow/TrajectoryPreview.cs`, `Prefabs/Bow/TrajectoryDot.prefab`.
- **Tests:** PlayMode `ArrowPreviewParityTests` (preview endpoint == arrow impact point ± 2 cm).

### AB-009 — ArrowProjectile + ArrowSpawner + ArrowRegistry
`[CORE][P0][M1][R-H][L][tech]` · depends on: AB-004, AB-006, AB-007
- **Description:** kinematic arrow integrated in `FixedUpdate`, swept with `SphereCastNonAlloc` (r = 0.06 m) against the cast mask. Visual interpolation in `Update`. Pooled via `PrefabPool<ArrowProjectile>`. `ArrowRegistry` caps active arrows at 8 (oldest embedded → static decor). Out-of-bounds → resolved. Raises `ArrowFired`, `ArrowImpact`, `ArrowResolved`.
- **Acceptance criteria:** 1,000-shot no-tunnelling test passes (0.1 m planks, 0.06 m ropes). Pool never allocates after warm-up. The 9th arrow converts the oldest embedded arrow.
- **Files:** `Scripts/Runtime/Arrows/ArrowProjectile.cs`, `ArrowSpawner.cs`, `ArrowRegistry.cs`, `Scripts/Runtime/Core/PrefabPool.cs`, `Prefabs/Arrows/Arrow_Oak.prefab`.
- **Tests:** PlayMode `ArrowTunnellingTests`, `ArrowPoolTests`.

### AB-010 — MaterialProfile + MaterialBody + PlanarBody
`[PHYS][P0][M1][R-M][M][tech]` · depends on: AB-002
- **Description:** `MaterialProfile` SO (density, friction, bounciness, HP, damage threshold, impulse transfer, embed rule, ricochet angle, debris set, SFX/VFX event refs, burnable flag). Five graybox profiles `MP_Straw/Timber/Stone/Ice/Metal` (values per `03`). `MaterialBody` applies mass from volume × density and the PhysicMaterial. `PlanarBody` applies constraints and z snap.
- **Acceptance criteria:** any body with `PlanarBody` has the correct constraints at `Awake`. Masses computed deterministically. Validator hook reports z ≠ 0.
- **Files:** `Scripts/Runtime/Physics/MaterialProfile.cs`, `MaterialBody.cs`, `PlanarBody.cs`, `ScriptableObjects/Materials/MP_*.asset`, `Art/Materials/PhysicMaterials/*`.
- **Tests:** EditMode `MaterialBodyTests`, `PlanarBodyTests`.

### AB-011 — ArrowImpactResolver
`[CORE+PHYS][P0][M1][R-H][L][tech]` · depends on: AB-009, AB-010
- **Description:** pure rule evaluation (arrow def × material × angle × speed → `ImpactOutcome`: Embed / Deflect / Ricochet / CutContinue / PopContinue / Portal / BreakTarget) + clamped impulse application at the contact point + damage forwarding. Embedded arrows parent as collider-less visuals (D-049). Spent arrows get a dynamic body on the `Arrow` layer and fade after 1.5 s.
- **Acceptance criteria:** every cell of the `03` arrow × material table has a unit test. Impulse never exceeds the `MaxArrowImpulse` clamp. A lodged arrow vibrates 0.2 s.
- **Files:** `Scripts/Runtime/Arrows/ArrowImpactResolver.cs`, `ImpactOutcome.cs`, edits to `ArrowProjectile.cs`.
- **Tests:** EditMode `ArrowImpactResolverTests`. PlayMode `ArrowEmbedTests`.

### AB-012 — Breakable + ImpactDamage + DebrisPool
`[PHYS][P0][M1][R-M][M][tech]` · depends on: AB-010, AB-011
- **Description:** `Breakable` (HP from the material, up to 3 visual states: intact/damaged/broken). `ImpactDamage` turns collision impulses above the threshold into damage, with a 0.5 s spawn grace. `DebrisPool` spawns material fragments on the `Debris` layer (cap 40 / Low 20, fade 1.2–2.0 s, `CosmeticRandom`). Raises `ObjectBroken`.
- **Acceptance criteria:** timber crate breaks after its defined stress. Fragments never touch gameplay layers. Pool recycles the oldest.
- **Files:** `Scripts/Runtime/Physics/Breakable.cs`, `ImpactDamage.cs`, `DebrisPool.cs`, `DebrisPiece.cs`, `Prefabs/Debris/*`.
- **Tests:** EditMode `BreakableTests` (HP math). PlayMode `DebrisIsolationTests`.

### AB-013 — Graybox structure prefab library v1
`[PHYS][P0][M1][R-L][S][content]` · depends on: AB-010, AB-012
- **Description:** `Struct_Base` + `Struct_Crate_Timber_1x1`, `_2x1`, `Struct_Beam_Timber_3x0.5`, `Struct_Plank_Timber_4x0.2`, `Struct_Platform_Static_*` (Environment), all snapped to a 0.05 m grid, flat colours per the colour language.
- **Acceptance criteria:** all prefabs pass the `PrefabValidator` rules (layer, `PlanarBody`, `MaterialBody`, collider within the mesh).
- **Files:** `Prefabs/Structures/*`, `Art/Materials/M_Graybox_*`.
- **Tests:** EditMode `PrefabLibraryTests` (iterates the folder).

### AB-014 — Physics stability spike + M1 feel gate
`[PHYS+QA+PO][P0][M1][R-H][M][qa]` · depends on: AB-011, AB-012, AB-013, AB-047
- **Description:** `Sandbox_PHYS_StackTest` with a 10-crate tower and variants. Measure the D-004 gates. A/B PGS vs TGS solver. Device feel test of the "Spec" vs "Snappy" presets with ≥ 5 testers. Report in `docs/qa/M1_feel_gate.md`.
- **Acceptance criteria:** gates measured with numbers. Recommendation for D-036/D-018/D-040 statuses. Fallback trigger evaluated.
- **Files:** `Scenes/Sandbox/Sandbox_PHYS_StackTest.unity`, `docs/qa/M1_feel_gate.md`.
- **Tests:** PlayMode `StackStabilityTests` + manual protocol.

### AB-015 — DevOverlay + cheats
`[CORE][P1][M1][R-L][S][tech]` · depends on: AB-003
- **Description:** `DevOverlay` compiled under `AB_DEV`: FPS, physics ms, awake bodies, gameplay state, arrows, settle timer, last 5 errors. Cheats: +arrow, win, fail, tuning preset swap, slow-mo, show intended shots.
- **Acceptance criteria:** absent from release builds (define check). 0 GC when hidden.
- **Files:** `Scripts/Runtime/Gameplay/DevOverlay.cs`, `Prefabs/UI/UI_DevOverlay.prefab`.
- **Tests:** EditMode define-strip test.

### AB-016 — LevelData v2 + LevelLayout + LevelLoader
`[LEVEL+CORE][P0][M2][R-M][L][tech]` · depends on: AB-004, AB-013, M1 gate
- **Description:** migrate `LevelData` to `[SerializeField] private` + properties (same serialized names). Add: `levelId`, required-objective summary, `IntendedShot` list, `TutorialPromptData`, flags (`isTutorial`, `isSetPiece`, `allowRewardedArrow`, `bullseyeEnabled`), `status` (Draft/Graybox/Art/Final), `mechanicTags`, `solutionArchetype`, `difficulty 1–5`, `designerNotes`, reserved `choiceSlots`/`allowSwap`. `LevelLayout` root component (framing, clear line Y, tutorial anchors, bounds). `LevelLoader` per `01` §4 (restart steps).
- **Acceptance criteria:** existing `LevelDataTests` still green. Restart < 300 ms (PlayMode perf test). Layout prefabs have no scene references.
- **Files:** `Scripts/Runtime/Levels/LevelData.cs` (LEVEL lock), `IntendedShot.cs`, `TutorialPromptData.cs`, `LevelLayout.cs`, `LevelLoader.cs`, `TutorialAnchor.cs`.
- **Tests:** EditMode `LevelDataTests` (extended). PlayMode `RestartPerfTests`.

### AB-017 — QuiverModel + StarRules
`[CORE][P0][M2][R-L][S][tech]` · depends on: AB-016
- **Description:** `QuiverModel` (ordered consumption, next arrow, bonus arrow add, `Changed` event). `StarRules.Compute(arrowsUsed, goldPar, bonusArrowUsed)` per §3 + D-024.
- **Acceptance criteria:** all star edge cases are tested. Quiver order matches the authored list.
- **Files:** `Scripts/Runtime/Gameplay/QuiverModel.cs`, `StarRules.cs`.
- **Tests:** EditMode `QuiverModelTests`, `StarRulesTests`.

### AB-018 — Objectives + Protected (vase)
`[PROPS][P0][M2][R-M][L][tech]` · depends on: AB-012, AB-016
- **Description:** `Objective` + `ObjectiveClearRule` flags + evaluator (D-037) for CrestTarget and SupplyCrate. `ObjectiveTracker` (registry, remaining count, `ObjectiveCleared`). `ProtectedObject` + `ProtectedTracker` with RoyalVase rules (D-038). Prefabs `Obj_CrestTarget`, `Obj_SupplyCrate`, `Prot_RoyalVase` with colour/icon language.
- **Acceptance criteria:** each clear/fail rule has a test. Objective icons have shape + colour.
- **Files:** `Scripts/Runtime/Objectives/*`, `Prefabs/Objectives/*`, `Prefabs/Protected/Prot_RoyalVase.prefab`.
- **Tests:** EditMode `ObjectiveClearRuleTests`. PlayMode `ObjectiveIntegrationTests`.

### AB-019 — SettleMonitor + GameplayController
`[CORE+PHYS][P0][M2][R-H][L][tech]` · depends on: AB-017, AB-018, AB-020, AB-021
- **Description:** `SettleMonitor` (calm definition, ignores kinematic/ambient, fake-clock friendly). `GameplayController` states `Loading, Ready, Drawing, Cooldown, AwaitingResolution, Won, Failed, Paused` implementing D-015/D-016. Public commands `Restart()`, `Pause()`, `Resume()`, `GrantBonusArrow()`, `Quit()`. Raises `LevelStarted/Won/Failed/Restarted/Quit`, `SoftLockPrompt`, `GameplayStateChanged`.
- **Acceptance criteria:** all D-015 timings covered by tests. Protected loss during the win-settle → fail. Firing allowed during resolution after the cooldown.
- **Files:** `Scripts/Runtime/Physics/SettleMonitor.cs`, `PhysicsBodyRegistry.cs`, `Scripts/Runtime/Gameplay/GameplayController.cs`, `GameplayState.cs`, `LevelSession.cs`.
- **Tests:** EditMode `GameplayStateMachineTests`, `SettleMonitorTests`. PlayMode `GameFlowSmokeTests`.

### AB-020 — RopeCuttable + RopeView
`[PROPS][P0][M2][R-M][M][tech]` · depends on: AB-011
- **Description:** per D-009: ConfigurableJoint limit, trigger capsule on `Rope` updated per `FixedUpdate`, LineRenderer sag visual, cut via arrow sweep (arrow continues), `PropTriggered(RopeCut)`, `Prop_Rope` prefab (+ anchor variants).
- **Acceptance criteria:** a rope never misses a cut at any arrow speed (part of the tunnelling suite). The load drops cleanly with no jitter at rest.
- **Files:** `Scripts/Runtime/Props/RopeCuttable.cs`, `RopeView.cs`, `Prefabs/Props/Prop_Rope*.prefab`.
- **Tests:** PlayMode `RopeCutTests`.

### AB-021 — KillZone + clear line
`[PROPS][P0][M2][R-L][S][tech]` · depends on: AB-018
- **Description:** `KillZone` kinds Water/Spikes/Pit with D-039 semantics. `LevelLayout.clearLineY` evaluation. `PlayBounds` uses the same semantics. Prefabs `Haz_WaterPit`, `Haz_SpikeBed`.
- **Acceptance criteria:** objective entering → cleared. Protected → fail. Arrow → resolved. Debris → returned to the pool.
- **Files:** `Scripts/Runtime/Props/KillZone.cs`, `Prefabs/Hazards/*`.
- **Tests:** PlayMode `KillZoneTests`.

### AB-022 — JsonSaveService v1 + ProgressionService.RecordResult
`[SYS][P0][M2][R-M][M][tech]` · depends on: AB-003, AB-017
- **Description:** `ISaveService` → `JsonSaveService` (atomic write + `.bak`, SHA-256, debounce). `SaveGame` v1 (`schemaVersion`, `installId`, `firstLaunchUtc`, `lifetimePlaySeconds`, `levels[]`, `settings`). `SaveMigrator` scaffold. `ProgressionService.RecordResult(LevelResultInfo)` (best stars/arrows, cleared, attempts).
- **Acceptance criteria:** survives a kill during write (`.bak` restore test). Best stars never decrease.
- **Files:** `Scripts/Runtime/Progression/SaveGame.cs` (+ DTOs), `JsonSaveService.cs`, `SaveMigrator.cs`, `ProgressionService.cs`.
- **Tests:** EditMode `JsonSaveServiceTests`, `SaveMigratorTests`, `ProgressionServiceTests`.

### AB-023 — Graybox HUD + Win/Fail/Pause panels
`[UI][P0][M2][R-L][M][story]` · depends on: AB-019
- **Description:** `HudView` (top-left level + pause, top-centre `ObjectiveIconsView`, top-right `QuiverView` + restart), `WinPanel` (stars, Next, Replay), `FailPanel` (reason, Retry dominant), `PausePanel`, `SafeAreaFitter`, `ToastView` (soft-lock). Graybox visuals. Strings via `UIStrings` keys.
- **Acceptance criteria:** safe area respected on notch/punch-hole simulators. Retry is 1 tap. HUD never overlaps the aim zone.
- **Files:** `Scripts/Runtime/UI/*`, `Prefabs/UI/*`.
- **Tests:** PlayMode `HudBindingTests`. Manual: device simulator aspect sweep.

### AB-024 — Analytics interface + debug sink + loop events
`[MON][P0][M2][R-L][S][tech]` · depends on: AB-003, AB-019
- **Description:** `AnalyticsEvents` (names + param builders), `DebugAnalyticsService` (log + CSV in `persistentDataPath` in dev), a glue listener mapping `GameEvents` → `level_started`, `arrow_fired`, `object_triggered`, `level_completed`, `level_failed`, `level_restarted` with `world_id`, `level_id`, `global_level`, `attempt_number`, `arrow_type`, `arrows_start`, `arrows_used`, `result`, `session_id`.
- **Acceptance criteria:** a full play of 10 levels produces a valid CSV. No PII.
- **Files:** `Scripts/Runtime/Services/AnalyticsEvents.cs`, `DebugAnalyticsService.cs`, `AnalyticsBridge.cs`, `AnalyticsContext.cs` (common params; names per `01` §7).
- **Tests:** EditMode `AnalyticsEventsTests` (param completeness).

### AB-025 — VS levels graybox + ShotRecorder + solvability bot v1
`[LEVEL+QA][P0][M2][R-M][L][content]` · depends on: AB-016, AB-018–AB-021
- **Description:** build VS-01..05 (W1_L01/04/07/10/16) per `11`. `ShotRecorder` editor tool (records angle/power/arrow type/delay into `LevelData.intendedShots`). Solvability bot v1 (PlayMode test replaying intended shots per level in `LC_VerticalSlice`).
- **Acceptance criteria:** 5/5 levels win 10/10 bot runs. Validator-lite checks (z = 0, layers, prefab-only).
- **Files:** `Prefabs/Levels/World1/Lvl_W1_L01/04/07/10/16.prefab`, `ScriptableObjects/Levels/World1/W1_L01…`, `ScriptableObjects/Levels/LC_VerticalSlice.asset`, `Scripts/Editor/Levels/ShotRecorder.cs`, `Tests/PlayMode/LevelSolvabilityTests.cs`, `docs/levels/W1_L01.md` …
- **Tests:** PlayMode `LevelSolvabilityTests`.

---

## 10. M3 tickets — AB-026 … AB-045 (Vertical Slice)

Mirrors [`11_VERTICAL_SLICE.md`](11_VERTICAL_SLICE.md) §4.2, which owns the full definitions. Labels: milestone **M3**, all P0 (the VS gate needs every one).

| ID | Title | Role | Size | Priority | Depends on | Risk | Acceptance (short) |
|---|---|---|---|---|---|---|---|
| AB-026 | Greenwood VS environment section v1 | ART | L | P0 | AB-004, AB-027 | R-H | Backdrop prefab + 2 variants; ≤ 40 k tris; ≤ 25 batches; reads well behind red/purple objects (5-second test) |
| AB-027 | Stylised lit shader + Greenwood palette + material swap | ART + ARCH | M | P0 | AB-002 | R-M | One Shader Graph lit shader, SRP-Batcher compatible; `T_Greenwood_Palette`; graybox → art swap via prefab variants only |
| AB-028 | Bow hero model v1 + BowView feedback | ART + CORE | M | P0 | AB-007 | R-M | Carved bow mesh ≤ 6 k tris; string stretch follows power; release snap anim; full-draw glow |
| AB-029 | VS prop art v1 (crates, posts, plank, crest, vase, rope) | ART | M | P0 | AB-013, AB-018, AB-020 | R-L | Art variants with identical colliders/mass (PrefabValidator green); 3 visual states max (§4) |
| AB-030 | Break VFX v1 + VfxService pooling | ART + PHYS | M | P0 | AB-012 | R-M | `VFX_Break_Timber`, `VFX_Break_Crest`, `VFX_Break_Vase`, `VFX_Impact_Timber`, `VFX_RopeSnap`; pooled; never covers an objective > 0.3 s |
| AB-031 | AudioService + SoundEvent library + VS SFX/music | ART | M | P0 | AB-003 | R-L | Mixer groups; 24 voices; ~16 SFX events + 1 Greenwood loop; per-event cooldown/pitch range |
| AB-032 | FeedbackDirector (GameEvents → SFX/VFX/haptics/hit-stop) | ART + CORE | M | P0 | AB-030, AB-031, AB-033, AB-034 | R-L | Mapping table data-driven; no gameplay code calls feedback directly |
| AB-033 | HitStop + CameraShake (reduced-motion aware) | CORE | S | P0 | AB-003 | R-L | 40–70 ms on meaningful direct impacts, max 1 per 0.3 s; physics outcome identical with/without (test) |
| AB-034 | HapticsService native bridge + editor stub | PLAT | M | P0 | AB-003 | R-M | iOS + Android implementations (D-032); Light/Medium/Success/Failure verified on device |
| AB-035 | UiTween utility | UI | S | P0 | AB-003 | R-L | Scale/fade/move/punch, easing set, unscaled time, zero GC per tween after warm-up |
| AB-036 | HUD polish (objective icons, quiver, next arrow, toasts) | UI | M | P0 | AB-023, AB-035 | R-L | Matches the §8 layout; cross-out anim on `ObjectiveCleared`; soft-lock toast; safe area on 9:16–9:21 |
| AB-037 | Win panel polished (star reveal, Next/Replay) | UI | M | P0 | AB-035, AB-031 | R-L | Stars sequence ≤ 1.6 s, skippable by tap; Next reachable in ≤ 1 tap at ≤ 0.6 s after the panel appears |
| AB-038 | Fail panel polished (reason, Retry dominant) | UI | S | P0 | AB-035 | R-L | Reason text by `FailReason`; Retry is the largest button; Retry → playable ≤ 1 s |
| AB-039 | Tutorial prompt system + L1 ghost hand + VS-03 callout | UI + CORE | M | P0 | AB-016, AB-035 | R-L | `TutorialPromptController`, `TutorialAnchor`, triggers/dismiss per `04` §7; prompts never overlap the shot line |
| AB-040 | VS playlist flow (`LC_VerticalSlice`) + slice-complete card | LEVEL + UI | S | P0 | AB-016, AB-022 | R-L | Boot → VS-01; Next advances; completion card shows per-level best stars; replay any |
| AB-041 | VS levels art pass + final tuning + re-recorded shots | LEVEL | M | P0 | AB-026, AB-029, M1 gate | R-M | All 5 levels at status **Final** (`04` §5); bot P-01..P-07 green with art prefabs |
| AB-042 | Analytics VS verification + CSV export + `level_quit` | MON | S | P0 | AB-024 | R-L | Every playtest session produces a CSV with session_id, attempt_number and full params; 0 missing/duplicate events in the QA script |
| AB-043 | Android VS device build + perf smoke | PLAT | M | P0 | AB-041 | R-M | Mid: avg ≥ 58 FPS, 1% low ≥ 45; Low: ≥ 30 FPS; restart p95 < 1 s; memory < 450 MB |
| AB-044 | VS regression checklist + QA pass + bug triage | QA | M | P0 | AB-043 | R-L | Checklist in `docs/qa/VS_REGRESSION.md`; 0 open P0/P1 bugs |
| AB-045 | VS playtest (5–10 players) + report + gate decision | QA + PO | M | P0 | AB-044 | R-H | Report in `docs/qa/VS_PLAYTEST_REPORT.md`; go/no-go entry added to `10_DECISION_LOG.md` |

**M3 addition (added in review; `11` §4.2 should mirror it):**

| ID | Title | Role | Size | Priority | Depends on | Risk | Acceptance (short) |
|---|---|---|---|---|---|---|---|
| AB-158 | `AssetImportRules` (AssetPostprocessor presets, D-055) + `Art/_Placeholder` label + placeholder build check in `BuildScript` (D-056) | ART + PLAT | S | P0 | AB-047 | R-L | The first imported art/audio gets preset settings automatically; a ClosedTest/Release build with a `Placeholder`-labelled dependency fails |

Critical path: AB-041 → AB-043 → AB-044 → AB-045. Art (AB-026/027/029) runs in parallel with feedback (AB-030–034) and UI (AB-035–039).

---

## 11. Later tickets (AB-046 onward)

### M1/M2 carry-ins

| ID | Title | Role | Size | Priority | Depends on | Risk | Milestone |
|---|---|---|---|---|---|---|---|
| AB-046 | Five additional graybox W1 levels (W1_L02, L03, L05, L06, L08) → 10 graybox levels. Uses M2 systems only: L03/L06/L08 use stand-ins (dummy → crest on a post, straw → light timber, lantern → crest on a rope) until AB-048/AB-052/AB-054 land; AB-064 upgrades them | LEVEL | M | P0 | AB-025 | R-L | M2 |
| AB-047 | `BuildScript` v1 + `BuildVersioning` + Android dev APK (Build Profiles `Android-Dev`) | PLAT | M | P0 | AB-002 | R-M | M1 |

### M4 — World 1 Systems & Tooling

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-048 | Material profiles pass: Straw + Stone final; Ice/Metal graybox-ready | PHYS | M | P0 | AB-010, M3 | R-M |
| AB-049 | Heavyhead arrow (`AD_Heavyhead`, high impulse, low speed) + preview parity | CORE | M | P0 | AB-011 | R-M |
| AB-050 | `ExplosionSolver` (pure) + `Explosion` (falloff, upward clamp, Protected excluded D-020) | PHYS | M | P0 | AB-012 | R-H |
| AB-051 | `PowderBarrel` prop + chain delay 0.15 s + `Prop_PowderBarrel` | PROPS | M | P0 | AB-050 | R-M |
| AB-052 | HangingLantern objective (`Obj_HangingLantern`) | PROPS | S | P0 | AB-018, AB-020 | R-L |
| AB-053 | CursedOrb objective (direct arrow hit only, kinematic anchor) | PROPS | S | P0 | AB-018 | R-L |
| AB-054 | TrainingDummy objective (knock-over 70°/0.3 s, ground touch, break) | PROPS | M | P0 | AB-018 | R-M |
| AB-055 | BannerRope objective + `Prop_Rope_Chain` visual variant | PROPS | S | P0 | AB-020 | R-L |
| AB-056 | SleepingFox protected (hit, displacement > 0.6 m, tilt > 45°, kill zone) | PROPS | M | P0 | AB-018 | R-M |
| AB-057 | `BullseyeMarker` + award on win + save flag | PROPS + SYS | S | P1 | AB-022 | R-L |
| AB-058 | Prefab library v2 (straw/stone structures, all objectives/protected/hazards) + `PrefabValidator` | PHYS + PROPS | M | P0 | AB-048 | R-L |
| AB-059 | `LevelValidator` v1 (all `04` rules) + EditMode tests + pre-build hook | LEVEL | L | P0 | AB-016, AB-058 | R-M |
| AB-060 | `LevelEditorWindow` + `LevelDataInspector` (create level, place library prefabs, validate, record shots) | LEVEL | L | P1 | AB-059 | R-M |
| AB-061 | `WorldData` + `LevelCatalog` (`LC_World1..3`) + `LevelCatalogBuilder` | LEVEL | M | P0 | AB-016 | R-L |
| AB-062 | Solvability bot v2 (`07` §5.1 9-sample jitter grid) + idle stability suite over catalogs | QA | M | P0 | AB-025, AB-061 | R-M |
| AB-063 | Interaction-matrix PlayMode tests — W1 cells | QA | M | P0 | AB-049–AB-056 | R-M |
| AB-064 | W1 graybox batch A: L09, L11, L12 + upgrade the AB-046 stand-ins in L03/L06/L08 to the real dummy/straw/lantern | LEVEL | M | P0 | AB-048, AB-052–AB-056, AB-059 | R-L |
| AB-065 | W1 graybox batch B: L13–L15 (Heavyhead intro + consolidation, shielded target L14, fox 2nd use L15) | LEVEL | L | P0 | AB-048, AB-049, AB-053, AB-056, AB-059 | R-M |
| AB-066 | W1 graybox batch C: L17–L20 (two-step cascades, powder barrel, L20 boss) | LEVEL | L | P0 | AB-051, AB-052, AB-054, AB-056, AB-059 | R-M |
| AB-067 | `ArrowRevealCard` (first appearance of a special arrow, `LevelData._revealsArrow`) | UI | S | P1 | AB-023, AB-049 | R-L |
| AB-068 | CI: GitHub Actions + GameCI (EditMode + PlayMode on PRs, nightly Android APK) + `ArchitectureRulesTests` | INT + ARCH | M | P1 | AB-001 | R-M |
| AB-159 | `IntendedSolutionRunner` (dev-build DevOverlay replay of intended shots on device; solvability layer 2, `07` §5.2) | QA + CORE | S | P0 | AB-015, AB-025, AB-047 | R-L |
| AB-160 | `MP_Earth` ground profile + `Ground` tag + validator rule "Earth only on Environment" (D-059, D-037) | PHYS | S | P1 | AB-010, AB-059 | R-L |

### M5 — World 1 Content & Art Lock

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-069 | Art direction lock document + style frames + palette + kit rules (OWNER sign-off, D-042 checklist) | ART | M | P0 | M3 gate | R-M |
| AB-070 | Greenwood environment kit full (~25 pieces; split into 3 L tickets: terrain/ruins, foliage/backdrop cards, platforms/towers) | ART | XL | P0 | AB-069 | R-H |
| AB-071 | W1 object art (crates, beams, target, lantern, orb, dummy, vase, fox, barrel, rope/chain, water pit) as world variants | ART | L | P0 | AB-069, AB-058 | R-M |
| AB-072 | Bow hero model final + string stretch/glow shader | ART | M | P0 | AB-069 | R-M |
| AB-073 | W1 SFX set (timber, straw, stone, rope, chain, barrel blast, vase, fox, dummy, lantern, orb) | ART | M | P0 | VS audio | R-L |
| AB-074 | W1 VFX set (break per material, blast, rope snap, orb shatter) | ART | M | P0 | VS VFX | R-L |
| AB-075 | `MusicPlayer` (per-world tracks, crossfade, ducking) + Greenwood music | ART | M | P1 | VS audio | R-L |
| AB-076 | World Map v1 (W1 path, 20 nodes, stars, Bullseye pip, locked teaser) | UI | L | P0 | AB-061, AB-078 | R-M |
| AB-077 | Home screen v1 (PLAY, current world card, shop/settings entries) | UI | M | P0 | AB-023 | R-L |
| AB-078 | `ProgressionService` full (unlock 15/20, best records, Bullseye, playtime counters) + tests | SYS | M | P0 | AB-022 | R-L |
| AB-079 | First-session flow: first launch → L1 directly; ≤ 15 s to the first shot | UI + SYS | M | P0 | AB-077, AB-078 | R-M |
| AB-080 | Settings panel v1 (music, SFX, haptics, reduced particles, colour-assist, reduced motion) | UI | M | P0 | AB-022 | R-L |
| AB-081 | Pause panel, level intro card (name + objectives), fail-reason copy | UI | S | P0 | AB-023 | R-L |
| AB-082 | W1 art + balance pass L1–20 (status Final) | LEVEL | L | P0 | AB-070, AB-071 | R-M |
| AB-083 | W1 intended-solution docs complete (`docs/levels/W1_L01…L20.md`) | LEVEL | S | P0 | AB-082 | R-L |
| AB-084 | W1 playtest round (8–10 players) + report | QA + PO | M | P0 | AB-082, AB-079 | R-M |
| AB-161 | `ChainReactionTracker` + escalating percussion layers + success sting (`05` SFX/music plan) | ART + CORE | S | P1 | AB-032, AB-073 | R-L |
| AB-162 | **OWNER:** create the Apple Developer + Google Play Console accounts; verify the Play closed-testing rule for this account type (D-072, D-079) | OWNER + PLAT | S | P0 | — | R-H |

### M6 — World 2 Sunscar Canyon

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-085 | `Balloon` (buoyant lift, pop on arrow/impact, Field-layer wind hook) | PROPS | M | P0 | AB-011 | R-M |
| AB-086 | `Burnable` + `FireZone` (overlap ticks, deterministic spread/burn timers) | PROPS | M | P0 | AB-020 | R-H |
| AB-087 | `OilJar` (strong hit → break → `FireZone`) | PROPS | S | P0 | AB-086 | R-M |
| AB-088 | Fire arrow (`FireArrowBehaviour`, `AD_Fire`, ignites rope/straw/oil) | CORE | M | P0 | AB-086 | R-M |
| AB-089 | Split arrow (`SplitArrowBehaviour`, timed + on-impact split, preview child arcs, D-019) | CORE | L | P0 | AB-049 | R-H |
| AB-090 | `KinematicMover` (rotate / ping-pong / loop on `LevelClock`) + `Prop_Shield_Metal_*` | PROPS | M | P0 | AB-003 | R-M |
| AB-091 | Rolling boulder prefabs + boulder-lane level kit pieces | PROPS | S | P0 | AB-058 | R-L |
| AB-092 | Lever / counterweight prefabs `Struct_Lever_*` (D-022) | PHYS | S | P0 | AB-058 | R-M |
| AB-093 | `SpringPlate` (hit or weighted → launch impulse) — only if on schedule (D-021) | PROPS | M | P2 | AB-058 | R-M |
| AB-094 | Metal material: ricochet angle tuning + shield validation | PHYS | S | P0 | AB-048 | R-M |
| AB-095 | Interaction-matrix PlayMode tests — W2 cells (fire × straw/rope/oil, balloon × wind/arrow, split × targets, metal ricochet) | QA | M | P0 | AB-085–AB-094 | R-M |
| AB-096 | Sunscar environment kit (split into 3 L) | ART | XL | P0 | AB-069 | R-H |
| AB-097 | W2 object art (balloons, oil jar, shields, boulder, lever, spike bed, sandstone variants) | ART | L | P0 | AB-096 | R-M |
| AB-098 | W2 SFX/VFX (fire, burn, pop, metal ping, boulder roll, split whoosh) | ART | M | P0 | AB-073 | R-L |
| AB-099 | Sunscar music | ART | S | P1 | AB-075 | R-L |
| AB-100 | W2 graybox L21–30 (balloons, oil, shields, Split intro L30) | LEVEL | L | P0 | AB-085–AB-090 | R-M |
| AB-101 | W2 graybox L31–40 (Split, counterweights/boulders, Fire intro L36, combos, L40 boss) | LEVEL | L | P0 | AB-088, AB-089, AB-091, AB-092 | R-M |
| AB-102 | W2 art + balance pass + solution docs | LEVEL | L | P0 | AB-096, AB-097 | R-M |
| AB-103 | W2 playtest round + report | QA | M | P0 | AB-102 | R-L |

### M7 — World 3 Frostspire Keep (+ SDK spike)

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-104 | Ice material behaviour (low friction slides, brittle shatter, harmless fragments) | PHYS | M | P0 | AB-048 | R-M |
| AB-105 | `WindField` (arrow acceleration via `IFlightEnvironment`, balloon force, fan prefab, preview D-017) | PROPS + CORE | M | P0 | AB-006, AB-085 | R-M |
| AB-106 | Bounce arrow (`BounceArrowBehaviour`, single full-speed metal ricochet, preview) | CORE | M | P0 | AB-094 | R-M |
| AB-107 | `PortalRing` / `PortalPair` (arrow-only teleport, direction transform, cooldown, preview pass-through) | PROPS + CORE | L | P0 | AB-006 | R-H |
| AB-108 | Moving ice platforms via `KinematicMover` (static fallback per cut list) | PROPS | S | P2 | AB-090, AB-104 | R-L |
| AB-109 | Interaction-matrix PlayMode tests — W3 cells (ice, wind × arrows/balloons, bounce × metal, portal × arrow types) | QA | M | P0 | AB-104–AB-107 | R-M |
| AB-110 | Frostspire environment kit (split into 3 L; aurora backdrop cards) | ART | XL | P0 | AB-069 | R-H |
| AB-111 | W3 object art (ice blocks, fans, portal rings, metal plates, royal relic) | ART | L | P0 | AB-110 | R-M |
| AB-112 | W3 SFX/VFX (ice chime/shatter, wind gusts, portal enter/exit) | ART | M | P0 | AB-073 | R-L |
| AB-113 | Frostspire music | ART | S | P1 | AB-075 | R-L |
| AB-114 | W3 graybox L41–50 (ice, wind, Bounce intro L47, portals L50) | LEVEL | L | P0 | AB-104–AB-107 | R-M |
| AB-115 | W3 graybox L51–60 (portals, moving ice, multi-system, L60 finale with `Prot_RoyalRelic`) | LEVEL | L | P0 | AB-114 | R-H |
| AB-116 | W3 art + balance pass + solution docs | LEVEL | L | P0 | AB-110, AB-111 | R-M |
| AB-117 | SDK integration spike branch: mediation + UMP + Firebase + Unity IAP compile; test ads on Android; iOS pods resolve | MON + PLAT | L | P0 | AB-003 | R-H |
| AB-118 | W3 playtest round + report | QA | M | P0 | AB-116 | R-L |

### M8 — Meta & Monetisation

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-119 | World Map final (3 worlds, unlock 15/20, world transitions, `world_unlocked`) | UI + SYS | L | P0 | AB-076, AB-078 | R-M |
| AB-120 | `EconomyService` + `EconomyConfig` (D-026) + tests | SYS | M | P0 | AB-078 | R-L |
| AB-121 | `CosmeticCatalog`/`CosmeticDefinition` + `InventoryService` (own, equip, `cosmetic_equipped`) + tests | SYS | M | P0 | AB-120 | R-L |
| AB-122 | Bow Forge screen (collection grid, equip, coin purchase, arrow codex) | UI | L | P0 | AB-121 | R-M |
| AB-123 | Cosmetic assets: 5 bow skins, 4 trails, quiver badges | ART | L | P1 | AB-072 | R-M |
| AB-124 | `DailyChallengeService` + `DailyChallengePanel` (D-027) | SYS + UI | M | P1 | AB-078, AB-120 | R-M |
| AB-125 | Bullseye collection view | UI | S | P2 | AB-057, AB-122 | R-L |
| AB-126 | Save schema full (economy, cosmetics, monetisation, daily, consent, tutorial flags) + migrations + tests | SYS | M | P0 | AB-022 | R-M |
| AB-127 | `AdPolicy` (D-025, §9) + exhaustive EditMode tests | MON | M | P0 | AB-078 | R-M |
| AB-128 | Ads vendor adapter in `ArrowBuster.Integrations` (rewarded + interstitial, test mode per `BuildConfig`) | MON | L | P0 | AB-117, AB-127 | R-H |
| AB-129 | Rewarded +1 arrow flow (Fail panel `RewardedOfferButton`; `06` §7.1 viability rule D-063; `LevelData.bonusArrowType` + resume in place D-062; `GrantBonusArrow`; 1★ cap D-024) | UI + CORE | M | P0 | AB-127, AB-128 | R-M |
| AB-130 | Rewarded 2× coins flow (Win panel, never blocks Next) | UI + SYS | S | P0 | AB-128, AB-120 | R-L |
| AB-131 | Unity IAP: `remove_ads`, `starter_pack`, restore, local receipt validation | MON | L | P0 | AB-117, AB-126 | R-H |
| AB-132 | Consent: UMP + ATT + `ConsentPanel` + Settings → Privacy; SDK init gated on consent | MON + UI | L | P0 | AB-117 | R-H |
| AB-133 | Analytics vendor adapter + full `06` event dictionary + funnels | MON | M | P0 | AB-117, AB-024 | R-M |
| AB-134 | Crash reporter adapter + IL2CPP symbol upload build step | PLAT + MON | M | P0 | AB-117 | R-M |
| AB-135 | Remote config adapter + whitelisted keys + cached last-known values | MON | M | P0 | AB-117 | R-M |
| AB-136 | `ShopScreen` + Home offers: starter pack + Remove Ads UI (localized store prices, restore entry) | UI | M | P0 | AB-131 | R-L |
| AB-137 | Splash/loading screen (branded bow draw; static fallback) | ART + UI | S | P2 | AB-072 | R-L |
| AB-163 | Privacy policy drafted from the `07` §13 inventory, hosted, URL in `app.privacy_policy_url` + Settings; crash-reporting legal basis confirmed (Q-17, Q-18) | OWNER + MON | S | P0 | AB-117 | R-H |
| AB-164 | `ModalDialog` + `CreditsPanel` (attributions from `docs/art/ASSET_LICENSES.md`) + Settings links (Privacy, Restore Purchases, Credits) | UI | S | P0 | AB-080 | R-L |
| AB-165 | iOS build path: `iOSPostProcess` (ATT usage string, privacy-manifest merge), first iOS device build with all SDKs on a Mac (Q-22) | PLAT | M | P0 | AB-047, AB-117 | R-H |
| AB-166 | Start the Google Play closed track early if D-079 applies (signed AAB with the upload key, tester list, day counter) | PLAT + OWNER | S | P0 (conditional) | AB-047, AB-162 | R-M |

### M9 — Optimisation, Balance, QA, Accessibility

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-138 | `QualityTierSelector` + `Mobile_High/Mid/Low_RPAsset` + runtime downgrade guard | PLAT | M | P0 | M8 | R-M |
| AB-139 | Device-matrix profiling pass + fixes (CPU/GPU/physics per heaviest levels) | PLAT | L | P0 | AB-138 | R-H |
| AB-140 | Memory + download-size pass (ASTC/ETC2 compression, audio import, atlas sizes) | PLAT + ART | M | P0 | AB-139 | R-M |
| AB-141 | Physics perf pass on set pieces (every 5th level) + body-budget fixes | PHYS | M | P0 | AB-139 | R-M |
| AB-142 | Balance pass all 60 (par, quiver, preview scale, difficulty curve per `04`) | LEVEL + PO | L | P0 | M8 | R-M |
| AB-143 | Accessibility: colour-assist outlines/patterns, reduced particles & motion, icon redundancy, touch-target sizes | UI + ART | M | P0 | AB-080 | R-L |
| AB-144 | Full regression + on-device solvability (all 60 on Mid Android + iPhone 11-class) | QA | L | P0 | AB-142 | R-M |
| AB-145 | Unity LTS upgrade spike (D-043) | ARCH | S | P1 | M8 | R-M |
| AB-146 | Haptics tuning on the device matrix | PLAT | S | P1 | VS haptics | R-L |
| AB-147 | Localisation-readiness audit (all text via `UIStrings`, layout tolerates +30% length) | UI | S | P1 | M8 | R-L |
| AB-148 | Analytics QA: event + param verification against the `06` dictionary; funnel dry-run | QA + MON | M | P0 | AB-133 | R-L |
| AB-149 | App lifecycle: pause/resume mid-flight, calls/notifications, low-memory, ad-interruption recovery, save on pause | PLAT | M | P0 | AB-128 | R-M |

### M10 — Closed Test & Submission Candidate

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-150 | Store listing assets (icon, screenshots ×5–8 per platform, preview video) — original, compared against the reference | ART + PO | L | P0 | M9 | R-M |
| AB-151 | App Privacy nutrition labels, Play Data safety form, SDK privacy manifests verified against the final policy (`07` §13) | MON + OWNER | M | P0 | AB-132, AB-163, AB-165 | R-H |
| AB-152 | Release build pipeline: signing (upload key off-repo), AAB, Xcode archive checklist, versioning | PLAT | M | P0 | AB-047 | R-M |
| AB-153 | Closed test distribution (TestFlight + Play closed track), tester onboarding (continues AB-166 if it started in M8) | PLAT + QA | M | P0 | AB-152 | R-M |
| AB-154 | Closed-test funnel review (L1 ≥ 95%, W1 ≥ 45% of L5 reachers, retries, abandonment spikes) + fixes | MON + PO | L | P0 | AB-153 | R-M |
| AB-155 | Store compliance + age-rating questionnaires (IARC / App Store) | PLAT + PO | S | P0 | AB-150 | R-L |
| AB-156 | Trademark/store-name search (Q-02) and final name lock | PO + OWNER | S | P0 | — (start by M8) | R-M |
| AB-157 | Release-gate sign-off (`07` gates) and submission | INT + QA + PO | S | P0 | AB-151–AB-156 | R-M |

---

## 12. Ticket count summary

| Milestone | ID range(s) | Count |
|---|---|---|
| M1 | AB-001 – AB-015, AB-047 | 16 |
| M2 | AB-016 – AB-025, AB-046 | 11 |
| M3 | AB-026 – AB-045 (defined in `11`), AB-158 | 21 |
| M4 | AB-048 – AB-068, AB-159, AB-160 | 23 |
| M5 | AB-069 – AB-084, AB-161, AB-162 | 18 |
| M6 | AB-085 – AB-103 | 19 |
| M7 | AB-104 – AB-118 | 15 |
| M8 | AB-119 – AB-137, AB-163 – AB-166 | 23 |
| M9 | AB-138 – AB-149 | 12 |
| M10 | AB-150 – AB-157 | 8 |
| **Total** | **AB-001 – AB-166** | **166** |

XL tickets (AB-070, AB-096, AB-110) must be split into 3 L tickets each (e.g. `AB-070a/b/c`) before they meet the Definition of Ready.
