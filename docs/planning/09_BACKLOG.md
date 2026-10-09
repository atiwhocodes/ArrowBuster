# 09 — Backlog

> Owner: Product Owner (`PO`) for priority; Lead Integrator (`INT`) for ordering and dependencies. Status: v2 — 2026-10-09 (owner decisions D-080…D-103 applied; milestones re-mapped per D-102).
> Milestones and schedule: [`08_PRODUCTION_ROADMAP.md`](08_PRODUCTION_ROADMAP.md). Names: [`01_TECHNICAL_ARCHITECTURE.md`](01_TECHNICAL_ARCHITECTURE.md). Decisions: [`10_DECISION_LOG.md`](10_DECISION_LOG.md).
> Task delegation format: [`../agents/TASK_TEMPLATE.md`](../agents/TASK_TEMPLATE.md). Each ticket below becomes one agent task.

---

## 1. Label legend

| Label family | Values | Meaning |
|---|---|---|
| **Discipline / role** | `PO` `ARCH` `CORE` `PHYS` `PROPS` `SYS` `UI` `LEVEL` `ART` `PLAT` `MON` `QA` `INT` `OWNER` | Owning specialist (first listed = accountable; `+` = contributor) |
| **Priority** | `P0` must ship · `P1` should ship (cut list candidate) · `P2` could ship (cut by default if late) · `Cut` not in MVP (kept for traceability, D-101) | MVP priority |
| **Milestone** | `M1` … `M10` | Target milestone (D-102 structure, see `08` §2) |
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
| E-01 | Project foundation, tooling & CI | P0 | ARCH / INT | M0, M1, M3, M4 | AB-001 (done), 002, 003, 015, 047, 068, 158 |
| E-02 | Bow & aiming | P0 | CORE | M1, M4 | AB-004–008, 170 |
| E-03 | Arrows & ballistics (5 arrow types) | P0 | CORE | M1, M4 | AB-005, 006, 009, 011, 049, 088, 089, 105, 106, 170 |
| E-04 | Physics, materials & destruction | P0 | PHYS | M1, M4, M8 | AB-002, 010–014, 048, 050, 058, 092, 094, 104, 141, 160 |
| E-05 | Objectives, protected objects & hazards | P0 | PROPS | M2, M4 | AB-018, 021, 052–057 |
| E-06 | Interactive props (8; Spring Plate cut D-086) | P0 | PROPS | M2, M4 | AB-020, 051, 085–087, 090–092, 105, 107 (AB-093, AB-108 Cut) |
| E-07 | Game loop & level flow | P0 | CORE | M2 | AB-016, 017, 019 |
| E-08 | Level pipeline & tooling | P0 | LEVEL | M2, M4 | AB-016, 025, 059–062, 159 |
| E-09 | Content — VS + graybox all 60 + device validation | P0 | LEVEL | M2, M3, M5 | AB-025, 041, 046, 064–066, 083, 100, 101, 114, 115, 175, 177 |
| E-10 | Content — final art passes (W1–W3) | P0 | LEVEL / ART | M6 | AB-082, 102, 116 |
| E-11 | Playtesting rounds (VS, graybox worlds, post-art, closed test) | P0 | QA / PO | M3, M5, M6, M9 | AB-045, 084, 103, 118, 176, 154 |
| E-12 | UI/UX & onboarding | P0 | UI | M2, M3, M4, M6, M7, M8 | AB-023, 036–040, 067, 076, 077, 079–081, 119, 122, 124, 125, 136, 143, 147, 164, 172 |
| E-13 | Art, VFX, audio & haptics | P0 | ART | M3, M6 | AB-026–045 (VS), 069–075, 096–099, 110–113, 123, 137, 158, 161, 171 |
| E-14 | Progression, economy & cosmetics | P0/P1 | SYS | M2, M6 | AB-022, 078, 120, 121, 124, 126, 130 |
| E-15 | Monetisation, consent, analytics, crash, RC (Firebase + LevelPlay, D-090) | P0 | MON | M2, M6, M7, M8 | AB-024, 117, 127–129, 131–135, 148, 163, 172 |
| E-16 | Performance & platform | P0 | PLAT | M1, M3, M7, M8, M9 | AB-047, 138–140, 146, 149, 152, 165, 168, 169, 177 |
| E-17 | QA & regression | P0 | QA | all | AB-014, 062, 063, 095, 109, 144, 148, 159, 175 |
| E-18 | Release, store & soft launch | P0 | PLAT / PO / OWNER | M1, M3, M6, M8, M9, M10 | AB-150–157, 162, 166, 167, 173, 174 |

---

## 5. User stories (player-facing)

| ID | Story | Acceptance criteria | Epic | Tickets |
|---|---|---|---|---|
| US-01 | As a player, I can pull back anywhere near the bow and release to shoot, so aiming feels natural with one thumb. | Press in the aim zone starts a draw. Releasing above `minFirePower` fires. Releasing below it cancels without spending an arrow. Works with mouse and touch. | E-02 | AB-007, AB-009 |
| US-02 | As a player, I see a dotted arc while drawing that shows exactly where my arrow will go. | Preview shown only while drawing. Parity with flight ≤ 1 mm over 3 s. Shows wind influence, portal exit paths and the first bounce (D-085); otherwise ends at the first hit with an impact ring. Length scales with `trajectoryPreviewScale` (never removed). | E-02 | AB-006, AB-008, AB-170 |
| US-03 | As a player, arrows react believably to what they hit (stick in wood, glance off stone/metal, cut ropes, pop balloons). | Outcomes per the `03` material × arrow table. A lodged arrow vibrates 0.2 s. Ropes and balloons are passed through. | E-03 | AB-011 |
| US-04 | As a player, I know what I must destroy and what I must protect. | Red crest/icon + outline on required objects; purple outline + icon on protected objects. Objective icons in the HUD cross out on clear. Colour is never the only signal. | E-05, E-12 | AB-018, AB-023 |
| US-05 | As a player, a level ends fairly and quickly. | Win after 0.75 s calm (cap 3 s). Out-of-arrows fail after 2 s calm (cap 5 s). Protected loss fails immediately with a reason. Soft-lock toast appears after 2 s calm. I can fire again 0.35 s after a release while physics resolves; firing is disabled once all objectives are cleared or the level is Won/Failed (D-083). | E-07 | AB-019 |
| US-06 | As a player, I can retry instantly. | 1 tap, any state. Restart < 1 s on device. No confirmation dialog. | E-07 | AB-016, AB-019, AB-023 |
| US-07 | As a player, I earn up to 3 stars by using fewer arrows. | ≤ par 3★, par+1 2★, else 1★. Bonus-arrow clear capped at 1★ but counts as completed (D-084). Best kept. | E-07, E-14 | AB-017, AB-022 |
| US-08 | As a new player, I'm shooting within 15 seconds and learn without a tutorial wall. | First launch goes straight to L1 with a ghost-hand hint. Median ≤ 15 s to the first shot. Callouts only in designated levels. | E-12 | AB-079, VS tickets |
| US-09 | As a player, special arrows appear as curated surprises that unlock new tricks. | Reveal card on first appearance: Heavyhead W1_L13 (L13), Split W2_L10 (L30), Fire W2_L16 (L36), Bounce W3_L07 (L47) (D-082). Quiver order shown in the HUD. Never purchasable. | E-03, E-12 | AB-049, AB-067, AB-088, AB-089, AB-106 |
| US-10 | As a player, I progress through a world map and unlock new worlds. | 20 nodes per world with stars. The next world unlocks at 15/20 clears. Locked-world teaser. `world_unlocked` fired. Built in M6, after the VS playtest and content graybox (D-102). | E-12, E-14 | AB-076, AB-078, AB-119 |
| US-11 | As a player, I earn coins and spend them only on cosmetics. | Coin values per D-026. Bow Forge grid with equip/buy. Launch set: 3 bow skins (Oak Ranger, Moonwood, Royal Amethyst) + 2 trails (Gold Spark, Leaf Swirl) (D-101, D-095). Cosmetics never change physics or aim. | E-14 | AB-120–AB-123 |
| US-12 | As a player, I can come back daily for a small challenge. | Unlocks after clearing Global L10. 3 cleared levels per local day (date-seeded). Quiver = gold par. 100 coins once per day. No leaderboard/streak/backend/push. Cut-first if it risks the campaign (D-094, D-101). | E-14 | AB-124 |
| US-13 | As a player who failed with one objective left, I can optionally watch an ad for one more arrow. | Offered max once per attempt, only if the D-063/§9 viability rule holds. Never on a protected-loss fail. The offer shows "Bonus Arrow Used — 1★ Max" before I accept (D-084). Retry stays the dominant button. | E-15 | AB-127–AB-129, AB-172 |
| US-14 | As a paying player, I can remove interstitial ads and restore my purchase. | `remove_ads` (£3.99 / USD 3.99) disables interstitials permanently. Rewarded stays opt-in. Starter Pack (£2.99 / USD 2.99) = Royal Amethyst + Gold Spark + 500 coins, never power (D-091). Restore works on a reinstall (iOS + Android). | E-15 | AB-131, AB-136 |
| US-15 | As a player in the EEA/UK (or on iOS), I'm asked for consent and can change it later. | UMP form before any ads/analytics/crash init; analytics, ad personalisation and non-essential crash reporting are consent-gated (D-096). ATT after UMP on iOS. A Settings → Privacy entry re-opens the options. | E-15 | AB-132 |
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
| TS-05 | As a developer, vendor SDKs never leak into game code. | Only `ArrowBuster.Integrations` references vendor assemblies (Firebase, LevelPlay, UMP, Unity IAP — D-090). `ArchitectureRulesTests` green. Project compiles with SDKs removed. | AB-068, AB-117, AB-128 |
| TS-06 | As a developer, every level is validated before it ships. | `LevelValidator` runs from the menu, in tests and before every build; build aborts on errors. | AB-059, AB-047/AB-152 |
| TS-07 | As a developer, saves never corrupt and always migrate. | Atomic write + `.bak`. Migration tests from v1. Corrupt-file test falls back to `.bak`. | AB-022, AB-126 |
| TS-08 | As a developer, the game picks a quality tier and holds the frame budget. | `QualityTierSelector` + runtime guard. Matrix results logged. | AB-138, AB-139 |
| TS-09 | As a developer, ad rules are enforced by tested pure logic. | `AdPolicy` tests cover every D-093 clause (10 min cumulative, ≥ 3 levels, ≥ 120 s, never after fail / rewarded ad / purchase / onboarding / app resume) and the §9 rewarded rules (D-063). | AB-127 |

## 7. Content-production stories

| ID | Story | Acceptance criteria | Tickets |
|---|---|---|---|
| CS-01 | VS levels VS-01..05 (W1_L01/04/07/10/16) graybox → final — the ONLY content built before the external playtest (D-102) | Per `11` level specs. Bot green with jitter. Playtest targets met. | AB-025, AB-026–045 |
| CS-02 | W1 graybox batch 0 (L02, L03, L05, L06, L08) — M5, after the VS gate and M4 systems (D-102) | Mechanics per the `04` plan using the real M4 objects. Validator 0 errors. Intended shots recorded. | AB-046 |
| CS-03 | W1 graybox batches A–C (L09, L11–15, L17–20) | Includes Heavyhead intro L13 and the L20 boss (3 watchtowers, powder barrel, sleeping fox, ≤ 4 arrows). | AB-064–AB-066 |
| CS-04 | W1 final art + balance (L1–20) | Status Final (M6). Solution docs written at graybox time (AB-083, M5). Device bot green. | AB-082, AB-083 |
| CS-05 | W2 graybox L21–30 and L31–40 | Balloons L21–23, oil L24–26, shields L27–29, Split L30–32, counterweights/boulders L33–35, fire combos L36–39 (Fire arrow L36), L40 boss siege gate (5 arrows). | AB-100, AB-101 |
| CS-06 | W2 final art + balance | As CS-04 for W2. | AB-102 |
| CS-07 | W3 graybox L41–50 and L51–60 | Ice L41–43, wind L44–46, Bounce L47–49 (first at W3_L07), portals L50–52, L53–55 low-risk timing with **static** ice slides + `KinematicMover` shields (moving ice platforms cut, D-104, D-101), multi-system L56–59, L60 finale. | AB-114, AB-115 |
| CS-08 | W3 final art + balance | As CS-04 for W3. | AB-116 |
| CS-09 | Environment kits ×3 | ~25 modular pieces per world, palette texture, pillarbox scenery ±3 m, blurred background cards (D-003). | VS art tickets, AB-070, AB-096, AB-110 |
| CS-10 | Cosmetics | Launch set (D-101): 3 bow skins (Oak Ranger default; Moonwood = W1 completion or 1,200 coins; Royal Amethyst = Starter Pack or 2,500 coins) + 2 trails (Gold Spark = Starter Pack or 1,000 coins; Leaf Swirl = 600 coins) + quiver badges. Ember/Frostglass skins and Cyan Streak/Ember Ash trails are post-MVP. | AB-123 |

## 8. QA stories

| ID | Story | Acceptance criteria | Tickets |
|---|---|---|---|
| QS-01 | Physics stability spike & feel gate | D-004 gates measured and reported. ≥ 5 testers on device compare presets. | AB-014 |
| QS-02 | Solvability bot | Replays intended shots on the `07` §5.1 grid (δθ = atan(0.4 m/d) ≥ 0.5°, δp ±0.03, 9 samples per shot, D-074). Runs over a `LevelCatalog`. JUnit-style report. | AB-025, AB-062 |
| QS-03 | Idle stability suite | Every level: 3 s with no shots, < 1 cm drift, no objective/protected state change. | AB-062 |
| QS-04 | Interaction matrix suites per world | One PlayMode test per non-empty cell of the `03` interaction matrix. | AB-063, AB-095, AB-109 |
| QS-05 | Playtest rounds (VS external, graybox W1/W2/W3, post-art full game, UK closed test) | Script from skill `gameplay-balance-playtest.md`. Report in `docs/qa/`. | AB-045, AB-084, AB-103, AB-118, AB-176, AB-154 |
| QS-06 | Device solvability (graybox, M5) + full regression (M8) | All 60 intended solutions replayed on Low Android + iPhone 11-class at graybox (AB-175), then again on art builds (AB-144). 0 P0/P1 open. | AB-175, AB-144 |
| QS-07 | Analytics verification | Every event/param in the `06` dictionary observed. No PII. | AB-148 |
| QS-08 | Monetisation compliance checks | No interstitial in the first 10 cumulative minutes, after a fail, rewarded ad, purchase, onboarding or app resume (D-093). Rewarded opt-in only with the 1★ disclosure. Remove Ads honoured. Restore works. | AB-127, AB-144 |

---

## 9. Recommended first 25 tickets (dependency order)

**Approved execution order (D-102) — governs all scheduling:**
1. Git baseline ✅ (AB-001) → 2. settings, layers, physics, test assemblies, folders (AB-002…) → 3. build the five-level vertical slice only → 4. 5–10 **external** casual-player tests before any special arrows, maps, cosmetics, ads or 60-level content → 5. lock bow feel, trajectory accuracy, arrow collision reliability, restart time, physics stability → 6. reusable material + interactive-object systems → 7. graybox all 60 levels → 8. validate every intended solution on target devices → 9. final art, sound, VFX, UI polish, cosmetics → 10. ads, IAP, consent, analytics, crash, Remote Config → 11. UK closed test, then Canada/Australia soft launch → 12. post-soft-launch optimisation only.

**Next ticket: AB-002.** No ticket in M1–M3 depends on special arrows, the world map, cosmetics, ads or content beyond the five VS levels. OWNER tasks run in parallel: AB-167 (Play account verification, week 1), AB-168 (Mac + Apple Developer account, by week 3), AB-162 (store accounts + IAP products, by week 5).

### AB-001 — Version control baseline ✅ DONE
`[INT][P0][M0][R-L][S][tech]` · depends on: none · **Done 2026-10-09:** commit `7a06460`, pushed to `github.com/atiwhocodes/ArrowBuster` (private), LFS + UnityYAMLMerge configured.
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
- **Description:** `Services` (static, typed properties, null-object defaults), `ServiceInstaller` (Boot + `EnsureInstalled()` editor fallback), `GameEvents` + `GameEventPayloads` (all events in `01` §10.2), `StaticReset`, `Log` (+ `LogCat`), `LevelClock`, `TimeScaleController` (sole `Time.timeScale` writer; pause > slow-mo > hit-stop, D-061), `CosmeticRandom`, `BuildConfig` SO, interfaces in `Services/` (`IAnalyticsService`, `IAdsService`, `IIapService`, `IConsentService`, `IRemoteConfigService`, `ICrashReportingService`, `ISaveService`, `IHapticsService`, `IAudioService`) with Null/Mock implementations. Interfaces are vendor-neutral even though Firebase + LevelPlay are chosen (D-090); no SDK code before M7. Update `GameBootstrap` to call `EnsureInstalled()`.
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
- **Description:** pure static `BallisticSolver.Step(ref ArrowFlightState, float dt, IFlightEnvironment env)` (semi-implicit Euler; gravity × gravityScale + env wind acceleration), `IFlightEnvironment` with a null implementation (wind/portal/first-bounce hooks filled in M4, D-085), `Launch(AimState, ArrowDefinition)` → initial state, and a helper for flight-time estimation.
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
- **Description:** pooled dot renderer (40 dots) sampling `BallisticSolver` with the same dt. It stops at the first hit on the arrow cast mask (passing Rope/Portal), shows an impact ring, length = `previewBaseSeconds × LevelData.trajectoryPreviewScale`. Visible only while drawing. Architecture must support one post-ricochet segment and portal/wind continuation (D-085); the metal first-bounce itself is validated in AB-170 (M4).
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
- **Acceptance criteria:** gates measured with numbers. Recommendation for D-036/D-018 statuses and the D-085 preview rule. Fallback trigger evaluated.
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
- **Description:** `QuiverModel` (ordered consumption, next arrow, bonus arrow add, `Changed` event). `StarRules.Compute(arrowsUsed, goldPar, bonusArrowUsed)` per §3 + D-084 (bonus clear = completed, max 1★).
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
- **Description:** `SettleMonitor` (calm definition, ignores kinematic/ambient, fake-clock friendly). `GameplayController` states `Loading, Ready, Drawing, Cooldown, AwaitingResolution, Won, Failed, Paused` implementing D-015/D-083. Public commands `Restart()`, `Pause()`, `Resume()`, `GrantBonusArrow()`, `Quit()`. Raises `LevelStarted/Won/Failed/Restarted/Quit`, `SoftLockPrompt`, `GameplayStateChanged`.
- **Acceptance criteria:** all D-015 timings covered by tests. Protected loss during the win-settle → fail. Firing allowed during resolution after the 0.35 s cooldown; firing disabled once all required objectives are cleared and in Won/Failed (D-083) — both covered by `GameplayStateMachineTests`.
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
- **Acceptance criteria:** a full play of the 5 VS levels produces a valid CSV. No PII.
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
| AB-043 | VS device builds + device validation — **Android and iOS** (iOS build via AB-169 on the Mac) | PLAT | M | P0 | AB-041, AB-169 | R-M | Android Mid: avg ≥ 58 FPS, 1% low ≥ 45; Android Low: ≥ 30 FPS; **iPhone 11-class: avg ≥ 58 FPS**; restart p95 < 1 s on both; memory < 450 MB; all 5 VS intended solutions replayed on both devices (`IntendedSolutionRunner` or manual) with the same outcome as the editor; safe areas, touch feel and haptics checked on iOS |
| AB-044 | VS regression checklist + QA pass + bug triage | QA | M | P0 | AB-043 | R-L | Checklist in `docs/qa/VS_REGRESSION.md`; 0 open P0/P1 bugs |
| AB-045 | VS playtest (5–10 players) + report + gate decision | QA + PO | M | P0 | AB-044 | R-H | Report in `docs/qa/VS_PLAYTEST_REPORT.md`; go/no-go entry added to `10_DECISION_LOG.md` |

**M3 addition (added in review; `11` §4.2 should mirror it):**

| ID | Title | Role | Size | Priority | Depends on | Risk | Acceptance (short) |
|---|---|---|---|---|---|---|---|
| AB-158 | `AssetImportRules` (AssetPostprocessor presets, D-055) + `Art/_Placeholder` label + placeholder build check in `BuildScript` (D-056) | ART + PLAT | S | P0 | AB-047 | R-L | The first imported art/audio gets preset settings automatically; a ClosedTest/Release build with a `Placeholder`-labelled dependency fails |
| AB-162 | **OWNER (by project week 5):** create the Apple Developer + Google Play Console app records and the IAP products `remove_ads` (£3.99 / USD 3.99) and `starter_pack` (£2.99 / USD 2.99) in both consoles (D-091, D-100) | OWNER + PLAT | S | P0 | AB-167, AB-168 | R-H | Both app records exist; both products are in "ready to submit"/draft state with the agreed price tiers; IDs match `IapCatalog` |
| AB-169 | iOS VS device build on the Mac (touch feel, safe areas, haptics, perf smoke; no SDKs) (D-100) | PLAT | M | P0 | AB-041, AB-168 | R-M | VS playlist runs on an iPhone 11-class device at ≥ 58 FPS median; safe area correct; haptics fire; build steps documented |

**M3 also delivers the feel lock (D-102 step 5):** the G0 gate records the locked values for bow feel (D-018 mapping, D-036 preset), trajectory accuracy (parity ≤ 1 mm), arrow collision reliability (1,000-shot tunnelling suite green), restart time (p95 < 1 s on device) and physics stability (PG-1…PG-6 still green with art prefabs).

Critical path: AB-041 → AB-043 (+ AB-169 iOS) → AB-044 → AB-045. Art (AB-026/027/029) runs in parallel with feedback (AB-030–034) and UI (AB-035–039). The AB-045 testers must be **external** casual players (not friends who have seen the game, D-102 step 4).

---

## 11. Later tickets (re-mapped to the D-102 milestones)

### M1 additions — Foundations & Core Feel

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-047 | `BuildScript` v1 + `BuildVersioning` + Android dev APK (Build Profiles `Android-Dev`) | PLAT | M | P0 | AB-002 | R-M |
| AB-167 | **OWNER (project week 1):** verify the Google Play developer account type and the current closed-testing requirement; if the personal-account rule applies, start recruiting eligible testers now and record the numbers in `docs/qa/` (D-099) | OWNER + PLAT | S | P0 | — | R-H |
| AB-168 | **OWNER (by project week 3):** Mac access + Apple Developer Program membership active; Xcode installed; one empty iOS build signed and run on device (D-100) | OWNER + PLAT | S | P0 | AB-047 | R-H |

### M4 — Systems Complete (D-102 step 6)

All reusable systems for all three worlds, built against sandbox scenes and micro-levels only (no campaign levels yet).

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-048 | Material profiles pass: Straw + Stone final; Ice/Metal graybox-ready | PHYS | M | P0 | AB-010, G0 | R-M |
| AB-049 | Heavyhead arrow (`AD_Heavyhead`, high impulse, low speed) + preview parity | CORE | M | P0 | AB-011 | R-M |
| AB-050 | `ExplosionSolver` (pure) + `Explosion` (falloff, upward clamp, no damage/force on Protected, D-092) | PHYS | M | P0 | AB-012 | R-H |
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
| AB-063 | Interaction-matrix PlayMode tests — W1-mechanic cells | QA | M | P0 | AB-049–AB-056 | R-M |
| AB-067 | `ArrowRevealCard` (first appearance of a special arrow, `LevelData._revealsArrow`) | UI | S | P1 | AB-023, AB-049 | R-L |
| AB-068 | CI: GitHub Actions + GameCI (EditMode + PlayMode on PRs, nightly Android APK) + `ArchitectureRulesTests` | INT + ARCH | M | P1 | AB-001 | R-M |
| AB-085 | `Balloon` (buoyant lift, pop on arrow/impact, wind via `WindFieldSampler`) | PROPS | M | P0 | AB-011 | R-M |
| AB-086 | `Burnable` + `FireZone` (overlap ticks, deterministic spread/burn timers) | PROPS | M | P0 | AB-020 | R-H |
| AB-087 | `OilJar` (strong hit → break → `FireZone`) | PROPS | S | P0 | AB-086 | R-M |
| AB-088 | Fire arrow (`FireArrowBehaviour`, `AD_Fire`, ignites rope/straw/oil) | CORE | M | P0 | AB-086 | R-M |
| AB-089 | Split arrow (`SplitArrowBehaviour`, timed split at `splitTime` + on-impact forward fan, preview split marker + child arcs, graybox pre-split pulse ~0.12 s before the split, D-088) | CORE | L | P0 | AB-049 | R-H |
| AB-090 | `KinematicMover` (rotate / ping-pong / loop on `LevelClock`) + `Prop_Shield_Metal_*` | PROPS | M | P0 | AB-003 | R-M |
| AB-091 | Rolling boulder prefabs + boulder-lane kit pieces | PROPS | S | P0 | AB-058 | R-L |
| AB-092 | Lever / seesaw + rope-hung weight prefabs `Struct_Lever_*` (no pulleys, D-087) | PHYS | S | P0 | AB-058 | R-M |
| AB-094 | Metal material: ricochet angle tuning + shield validation | PHYS | S | P0 | AB-048 | R-M |
| AB-095 | Interaction-matrix PlayMode tests — W2-mechanic cells (fire × straw/rope/oil, balloon × wind/arrow, split × targets, metal ricochet) | QA | M | P0 | AB-085–AB-092, AB-094 | R-M |
| AB-104 | Ice material behaviour (low friction slides, brittle shatter, harmless fragments) | PHYS | M | P0 | AB-048 | R-M |
| AB-105 | `WindField` (arrow acceleration via `IFlightEnvironment`, balloon force, fan prefab, wind shown in the preview D-085) | PROPS + CORE | M | P0 | AB-006, AB-085 | R-M |
| AB-106 | Bounce arrow (`BounceArrowBehaviour`, single full-speed metal ricochet, first bounce shown in the preview D-085) | CORE | M | P0 | AB-094, AB-170 | R-M |
| AB-107 | `PortalRing` / `PortalPair` (arrow-only teleport, direction transform, cooldown, preview shows the exit path D-085) | PROPS + CORE | L | P0 | AB-006 | R-H |
| AB-109 | Interaction-matrix PlayMode tests — W3-mechanic cells (ice, wind × arrows/balloons, bounce × metal, portal × arrow types) | QA | M | P0 | AB-104–AB-107 | R-M |
| AB-159 | `IntendedSolutionRunner` (dev-build DevOverlay replay of intended shots on device; solvability layer 2, `07` §5.2) | QA + CORE | S | P0 | AB-015, AB-025, AB-047 | R-L |
| AB-160 | `MP_Earth` ground profile + `Ground` tag + validator rule "Earth only on Environment" (D-059, D-037) | PHYS | S | P1 | AB-010, AB-059 | R-L |
| AB-170 | Preview first bounce (D-085): when `ArrowImpactResolver` predicts Ricochet at the first hit (Bounce on metal; any arrow at a shallow metal angle), draw the reflected segment to the next hit / length budget; parity tests for wind, portal exit and first bounce | CORE | M | P0 | AB-008, AB-011, AB-094 | R-M |

### M5 — Content Graybox + Device Validation (D-102 steps 7–8)

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-046 | W1 graybox batch 0: W1_L02, L03, L05, L06, L08 (real M4 objects — no stand-ins needed) | LEVEL | M | P0 | AB-025, AB-048, AB-052, AB-054, AB-059 | R-L |
| AB-064 | W1 graybox batch A: L09, L11, L12 | LEVEL | M | P0 | AB-048, AB-052–AB-056, AB-059 | R-L |
| AB-065 | W1 graybox batch B: L13–L15 (Heavyhead intro at W1_L13, shielded target L14, fox 2nd use L15) | LEVEL | L | P0 | AB-048, AB-049, AB-053, AB-056, AB-059 | R-M |
| AB-066 | W1 graybox batch C: L17–L20 (two-step cascades, powder barrel, L20 boss) | LEVEL | L | P0 | AB-051, AB-052, AB-054, AB-056, AB-059 | R-M |
| AB-100 | W2 graybox L21–30 (balloons, oil, shields, Split intro at W2_L10 = L30) | LEVEL | L | P0 | AB-085–AB-090, AB-059 | R-M |
| AB-101 | W2 graybox L31–40 (Split, counterweights/boulders, Fire intro at W2_L16 = L36, combos, L40 boss) | LEVEL | L | P0 | AB-088, AB-089, AB-091, AB-092, AB-100 | R-M |
| AB-114 | W3 graybox L41–50 (ice, wind, Bounce intro at W3_L07 = L47, portals L50) | LEVEL | L | P0 | AB-104–AB-107, AB-059 | R-M |
| AB-115 | W3 graybox L51–60 (portals, L53–55 static ice slides + mover shields, multi-system, L60 finale with `Prot_RoyalRelic`) | LEVEL | L | P0 | AB-114 | R-H |
| AB-083 | Intended-solution docs for all 60 (`docs/levels/W#_L##.md`) written as each batch lands | LEVEL | M | P0 | AB-046, AB-064–AB-066, AB-100, AB-101, AB-114, AB-115 | R-L |
| AB-084 | W1 graybox playtest round (6–8 external players) + report | QA + PO | M | P0 | AB-046, AB-064–AB-066 | R-M |
| AB-103 | W2 graybox playtest round + report | QA | M | P0 | AB-100, AB-101 | R-L |
| AB-118 | W3 graybox playtest round + report | QA | M | P0 | AB-114, AB-115 | R-L |
| AB-175 | On-device validation of all 60 intended solutions (`IntendedSolutionRunner` on Low Android + iPhone 11-class; editor/device outcome mismatch → physics ticket) | QA + PLAT | L | P0 | AB-159, AB-083, AB-168 | R-H |
| AB-177 | Graybox perf baseline on set pieces (every 5th level + bosses) on the Low and Mid devices; body-budget fixes | PLAT + PHYS | M | P0 | AB-101, AB-115 | R-M |

### M6 — Art, Audio, UI Polish & Meta (D-102 step 9)

Parallel-track allowance: environment-kit modelling (AB-070, AB-096, AB-110) may start after the G0 art-direction lock and run during M4–M5; integration into levels (AB-082/AB-102/AB-116) happens here.

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-069 | Art direction lock document + style frames + palette + kit rules (OWNER sign-off, D-089 licensing/AI checklist) — drafted right after G0 | ART | M | P0 | G0 | R-M |
| AB-070 | Greenwood environment kit full (~25 pieces; split into 3 L: terrain/ruins, foliage/backdrop cards, platforms/towers) | ART | XL | P0 | AB-069 | R-H |
| AB-071 | W1 object art (crates, beams, target, lantern, orb, dummy, vase, fox, barrel, rope/chain, water pit) as world variants | ART | L | P0 | AB-069, AB-058 | R-M |
| AB-072 | Bow hero model final + string stretch/glow shader | ART | M | P0 | AB-069 | R-M |
| AB-073 | W1 SFX set (timber, straw, stone, rope, chain, barrel blast, vase, fox, dummy, lantern, orb) — licensed libraries allowed if processed + registered (D-089) | ART | M | P0 | AB-031 | R-L |
| AB-074 | W1 VFX set (break per material, blast, rope snap, orb shatter) | ART | M | P0 | AB-030 | R-L |
| AB-075 | `MusicPlayer` (per-world tracks, crossfade, ducking) + Greenwood music (original, D-089) | ART | M | P1 | AB-031 | R-L |
| AB-076 | World Map (W1 path, 20 nodes, stars, Bullseye pip, locked teaser) | UI | L | P0 | AB-061, AB-078 | R-M |
| AB-077 | Home screen v1 (PLAY, current world card, shop/settings entries) | UI | M | P0 | AB-023 | R-L |
| AB-078 | `ProgressionService` full (unlock 15/20, best records, Bullseye, playtime counters incl. cumulative active play for D-093) + tests | SYS | M | P0 | AB-022 | R-L |
| AB-079 | First-session flow: first launch → L1 directly; ≤ 15 s to the first shot | UI + SYS | M | P0 | AB-077, AB-078 | R-M |
| AB-080 | Settings panel v1 (music, SFX, haptics, reduced particles, colour-assist, reduced motion) | UI | M | P0 | AB-022 | R-L |
| AB-081 | Pause panel, level intro card (name + objectives), fail-reason copy | UI | S | P0 | AB-023 | R-L |
| AB-082 | W1 art + balance pass L1–20 (status Final) | LEVEL | L | P0 | AB-070, AB-071, AB-083 | R-M |
| AB-096 | Sunscar environment kit (split into 3 L) | ART | XL | P0 | AB-069 | R-H |
| AB-097 | W2 object art (balloons, oil jar, shields, boulder, lever, spike bed, sandstone variants) | ART | L | P0 | AB-096 | R-M |
| AB-098 | W2 SFX/VFX (fire, burn, pop, metal ping, boulder roll, split whoosh) | ART | M | P0 | AB-073 | R-L |
| AB-099 | Sunscar music | ART | S | P1 | AB-075 | R-L |
| AB-102 | W2 art + balance pass (status Final) | LEVEL | L | P0 | AB-096, AB-097, AB-083 | R-M |
| AB-110 | Frostspire environment kit (split into 3 L; aurora backdrop cards) | ART | XL | P0 | AB-069 | R-H |
| AB-111 | W3 object art (ice blocks, fans, portal rings, metal plates, royal relic) | ART | L | P0 | AB-110 | R-M |
| AB-112 | W3 SFX/VFX (ice chime/shatter, wind gusts, portal enter/exit) | ART | M | P0 | AB-073 | R-L |
| AB-113 | Frostspire music | ART | S | P1 | AB-075 | R-L |
| AB-116 | W3 art + balance pass (status Final) | LEVEL | L | P0 | AB-110, AB-111, AB-083 | R-M |
| AB-119 | World Map final (3 worlds, unlock 15/20, world transitions, `world_unlocked`) | UI + SYS | L | P0 | AB-076, AB-078 | R-M |
| AB-120 | `EconomyService` + `EconomyConfig` (D-026) + tests | SYS | M | P0 | AB-078 | R-L |
| AB-121 | `CosmeticCatalog`/`CosmeticDefinition` + `InventoryService` (own, equip, `cosmetic_equipped`) + tests | SYS | M | P0 | AB-120 | R-L |
| AB-122 | Bow Forge screen (collection grid, equip, coin purchase, arrow codex) | UI | L | P0 | AB-121 | R-M |
| AB-123 | Cosmetic assets — launch set only (D-101): bow skins Oak Ranger, Moonwood, Royal Amethyst (D-095); trails Gold Spark, Leaf Swirl; quiver badges | ART | M | P1 | AB-072 | R-M |
| AB-124 | `DailyChallengeService` + `DailyChallengePanel` (unlock after Global L10, local date seed, gold-par quiver; D-094) — **cut-first** if it risks the campaign | SYS + UI | M | P1 | AB-078, AB-120 | R-M |
| AB-125 | Bullseye collection view | UI | S | P2 | AB-057, AB-122 | R-L |
| AB-126 | Save schema full (economy, cosmetics, monetisation, daily, consent, tutorial flags) + migrations + tests | SYS | M | P0 | AB-022 | R-M |
| AB-137 | Static splash/loading screen (animated branded splash cut, D-101) | ART + UI | S | P1 | AB-072 | R-L |
| AB-161 | `ChainReactionTracker` + escalating percussion layers + success sting (`05` SFX/music plan) | ART + CORE | S | P1 | AB-032, AB-073 | R-L |
| AB-163 | **Due before M7 starts (D-097):** privacy policy drafted from the `07` §13 inventory (analytics, crash reporting, advertising, IAP processing, consent choices, support contact), hosted, URL in `app.privacy_policy_url` + Settings; legal/privacy review of the consent-gating approach (D-096) | OWNER + MON | S | P0 | — | R-H |
| AB-164 | `ModalDialog` + `CreditsPanel` (attributions from `docs/art/ASSET_LICENSES.md`) + Settings links (Privacy, Restore Purchases, Credits) | UI | S | P0 | AB-080 | R-L |
| AB-166 | Start the Google Play closed track early if D-099 applies (signed AAB with the upload key, tester list, day counter) — the UK tester pool from AB-167 | PLAT + OWNER | S | P0 (conditional) | AB-047, AB-162, AB-167 | R-M |
| AB-171 | Split-arrow pre-split pulse VFX final (`VFX_Arrow_SplitPulse`: glowing ring/trail pulse ~0.12 s before the split, D-088) | ART | S | P0 | AB-089, AB-098 | R-L |
| AB-176 | Post-art full-game playtest (W1–W3, 8–10 external players) + report before G3 | QA + PO | M | P0 | AB-082, AB-102, AB-116 | R-M |

### M7 — Platform Services & Monetisation (D-102 step 10)

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-117 | SDK integration baseline (D-090): Firebase (Analytics, Crashlytics, Remote Config) + Unity LevelPlay + Google UMP + Unity IAP added to `ArrowBuster.Integrations`; EDM4U pinned; Android + iOS builds compile; LevelPlay test ads on Android | MON + PLAT | L | P0 | AB-003, AB-162, AB-163, AB-168 | R-H |
| AB-127 | `AdPolicy` (D-093 interstitial rules: ≥ 10 min cumulative active play, ≥ 3 levels, ≥ 120 s, Win → Next/Home only; never after fail, rewarded ad, purchase, onboarding L1–L5, app resume; §9 rewarded rules D-063) + exhaustive EditMode tests | MON | M | P0 | AB-078 | R-M |
| AB-128 | `LevelPlayAdsService` adapter (`IAdsService`; rewarded + interstitial, test mode per `BuildConfig`) | MON | L | P0 | AB-117, AB-127 | R-H |
| AB-129 | Rewarded +1 arrow flow (Fail panel `RewardedOfferButton`; `06` §7.1 viability rule D-063; `LevelData.bonusArrowType` + resume in place D-062; `GrantBonusArrow`; 1★ cap D-084) | UI + CORE | M | P0 | AB-127, AB-128 | R-M |
| AB-130 | Rewarded 2× coins flow (Win panel, never blocks Next) | UI + SYS | S | P0 | AB-128, AB-120 | R-L |
| AB-131 | Unity IAP: `remove_ads` (£3.99 / USD 3.99), `starter_pack` (£2.99 / USD 2.99: Royal Amethyst + Gold Spark + 500 coins, no power), restore, local receipt validation (D-091, D-065) | MON | L | P0 | AB-117, AB-126 | R-H |
| AB-132 | Consent: UMP + ATT + `ConsentPanel` + Settings → Privacy; consent-aware SDK init — before consent in UK/EEA: no personalised ads, Firebase Analytics and Crashlytics collection disabled (D-096) | MON + UI | L | P0 | AB-117 | R-H |
| AB-133 | `FirebaseAnalyticsService` adapter + full `06` event dictionary + funnels | MON | M | P0 | AB-117, AB-024 | R-M |
| AB-134 | `FirebaseCrashReportingService` (`ICrashReportingService`, consent-gated) + IL2CPP symbol upload build step | PLAT + MON | M | P0 | AB-117, AB-132 | R-M |
| AB-135 | `FirebaseRemoteConfigService` adapter + whitelisted keys (incl. all D-093 thresholds) + cached last-known values | MON | M | P0 | AB-117 | R-M |
| AB-136 | `ShopScreen` + Home offers: starter pack + Remove Ads UI (localized store prices, restore entry) | UI | M | P0 | AB-131 | R-L |
| AB-165 | iOS build path with SDKs: `iOSPostProcess` (ATT usage string, privacy-manifest merge), iOS device build with all SDKs | PLAT | M | P0 | AB-047, AB-117, AB-168 | R-H |
| AB-172 | Bonus-arrow star-cap disclosure: "Bonus Arrow Used — 1★ Max" (`fail.bonus_arrow.star_cap`) on `RewardedOfferButton` before the ad + "+1" HUD badge; UI test (D-084) | UI | S | P0 | AB-129 | R-L |

### M8 — Optimisation, Balance, QA & Release Prep

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-138 | `QualityTierSelector` + `Mobile_High/Mid/Low_RPAsset` + runtime downgrade guard | PLAT | M | P0 | G4 | R-M |
| AB-139 | Device-matrix profiling pass + fixes (CPU/GPU/physics per heaviest levels) | PLAT | L | P0 | AB-138 | R-H |
| AB-140 | Memory + download-size pass (ASTC/ETC2 compression, audio import, atlas sizes) | PLAT + ART | M | P0 | AB-139 | R-M |
| AB-141 | Physics perf pass on art set pieces (every 5th level) + body-budget fixes | PHYS | M | P0 | AB-139 | R-M |
| AB-142 | Balance pass all 60 (par, quiver, preview scale, difficulty curve per `04`) | LEVEL + PO | L | P0 | G4 | R-M |
| AB-143 | Accessibility: colour-assist outlines/patterns, reduced particles & motion, icon redundancy, touch-target sizes | UI + ART | M | P0 | AB-080 | R-L |
| AB-144 | Full regression + on-device solvability on art builds (all 60 on Mid Android + iPhone 11-class) | QA | L | P0 | AB-142 | R-M |
| AB-146 | Haptics tuning on the device matrix | PLAT | S | P1 | AB-034 | R-L |
| AB-147 | Localisation-readiness audit (all text via `UIStrings`, layout tolerates +30% length) | UI | S | P1 | G4 | R-L |
| AB-148 | Analytics QA: event + param verification against the `06` dictionary; consent-off path verified; funnel dry-run | QA + MON | M | P0 | AB-133 | R-L |
| AB-149 | App lifecycle: pause/resume mid-flight, calls/notifications, low-memory, ad-interruption recovery, no interstitial on resume, save on pause | PLAT | M | P0 | AB-128 | R-M |
| AB-156 | Legal / store-name / domain clearance for "Arrow Buster" (D-081) and final name lock — before store assets are produced | PO + OWNER | S | P0 | — | R-M |
| AB-150 | Store listing assets (icon, screenshots ×5–8 per platform, preview video) — original, compared against the reference | ART + PO | L | P0 | AB-156, AB-176 | R-M |

### M9 — UK Closed Test & Submission (D-098)

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-151 | App Privacy nutrition labels, Play Data safety form, SDK privacy manifests verified against the final policy (`07` §13) | MON + OWNER | M | P0 | AB-132, AB-163, AB-165 | R-H |
| AB-152 | Release build pipeline: signing (upload key off-repo), AAB, Xcode archive checklist, versioning | PLAT | M | P0 | AB-047 | R-M |
| AB-153 | UK closed test distribution (TestFlight + Play closed track, UK testers; ≥ 14 days or the store-required tester count/duration if longer; continues AB-166 if it started earlier) | PLAT + QA | M | P0 | AB-152 | R-M |
| AB-154 | Closed-test funnel review (L1 ≥ 95%, W1 ≥ 45% of L5 reachers, retries, abandonment spikes) + fixes | MON + PO | L | P0 | AB-153 | R-M |
| AB-155 | Store compliance + age-rating questionnaires (IARC / App Store) | PLAT + PO | S | P0 | AB-150 | R-L |
| AB-157 | Release-gate sign-off (`07` G-Release) and submission | INT + QA + PO | S | P0 | AB-151–AB-156 | R-M |

### M10 — Soft Launch (Canada, Australia) (D-098)

| ID | Title | Role | Size | Priority | Depends on | Risk |
|---|---|---|---|---|---|---|
| AB-173 | Soft-launch release in Canada and Australia (store country availability, release build, Remote Config production values, privacy-law check: PIPEDA / Quebec Law 25, Australian Privacy Act) | PLAT + OWNER | M | P0 | AB-157 | R-M |
| AB-174 | Soft-launch KPI read-out against `07` soft-launch criteria (L1 ≥ 95%, W1 ≥ 45%, retries, crash-free, D1/D7 vs channel) → scale / iterate / kill decision logged; step-12 optimisation tickets created only from this data | MON + PO + OWNER | M | P0 | AB-173 | R-M |

### Cut / post-MVP (kept for traceability)

| ID | Title | Status | Decision |
|---|---|---|---|
| AB-093 | `SpringPlate` (hit or weighted → launch impulse) | **Cut** from the MVP — post-MVP backlog | D-086, D-104 |
| AB-108 | Moving ice platforms via `KinematicMover` | **Cut** from the MVP (L53–55 use static ice slides + moving shields) — post-MVP backlog | D-101, D-104 |
| AB-145 | Unity LTS upgrade spike | **Cut** — Unity 6000.6.5f1 locked for the MVP | D-080 |

---

## 12. Ticket count summary

| Milestone | ID range(s) | Count |
|---|---|---|
| M0 | AB-001 ✅ | 1 |
| M1 | AB-002 – AB-015, AB-047, AB-167, AB-168 | 17 |
| M2 | AB-016 – AB-025 | 10 |
| M3 | AB-026 – AB-045, AB-158, AB-162, AB-169 | 23 |
| M4 | AB-048 – AB-063, AB-067, AB-068, AB-085 – AB-092, AB-094, AB-095, AB-104 – AB-107, AB-109, AB-159, AB-160, AB-170 | 36 |
| M5 | AB-046, AB-064 – AB-066, AB-083, AB-084, AB-100, AB-101, AB-103, AB-114, AB-115, AB-118, AB-175, AB-177 | 14 |
| M6 | AB-069 – AB-082 (except AB-083/084), AB-096 – AB-099, AB-102, AB-110 – AB-113, AB-116, AB-119 – AB-126, AB-137, AB-161, AB-163, AB-164, AB-166, AB-171, AB-176 | 39 |
| M7 | AB-117, AB-127 – AB-136, AB-165, AB-172 | 13 |
| M8 | AB-138 – AB-144, AB-146 – AB-150, AB-156 | 13 |
| M9 | AB-151 – AB-155, AB-157 | 6 |
| M10 | AB-173, AB-174 | 2 |
| Cut | AB-093, AB-108, AB-145 | 3 |
| **Total** | **AB-001 – AB-177** | **177** (174 active + 3 cut) |

XL tickets (AB-070, AB-096, AB-110) must be split into 3 L tickets each (e.g. `AB-070a/b/c`) before they meet the Definition of Ready (D-071).
