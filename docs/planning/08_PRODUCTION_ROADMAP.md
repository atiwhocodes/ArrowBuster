# 08 — Production Roadmap

> Owner: Product Owner / Game Design Lead (`PO`), co-owned with Lead Integrator (`INT`) for scheduling. Status: **v2 — 2026-10-09 (owner-approved; D-080…D-103 applied).**
> Milestone structure: decision **D-102** (supersedes D-033 and the D-078 gate names). Ticket IDs (`AB-###`) are defined in [`09_BACKLOG.md`](09_BACKLOG.md). Names follow [`01_TECHNICAL_ARCHITECTURE.md`](01_TECHNICAL_ARCHITECTURE.md).
> Source of truth for scope: [`/mvp.md`](../../mvp.md) §14–16. Analysis and risks: [`00_MVP_ANALYSIS.md`](00_MVP_ANALYSIS.md). Decisions: [`10_DECISION_LOG.md`](10_DECISION_LOG.md).

**Governing principle (OWNER):** protect the core physics-puzzle loop, cut optional complexity early, lock technology now, and delay all third-party SDK work until the vertical slice is fun.

---

## 0. Approved execution order (D-102) — governs everything below

| Step | What | Milestone | Status |
|---:|---|---|---|
| 1 | Initialise Git, configure Unity YAML merge, first clean baseline commit | M0 | ✅ AB-001 — commit `7a06460`, `github.com/atiwhocodes/ArrowBuster` (private) |
| 2 | Apply project settings, layers, physics settings, test assemblies, folder structure | M1 | **Next: AB-002** |
| 3 | Build the five-level vertical slice **only** | M1–M3 | — |
| 4 | Run 5–10 **external** casual-player tests **before** special arrows, maps, cosmetics, ads or 60 levels | M3 | — |
| 5 | Lock bow feel, trajectory accuracy, arrow collision reliability, restart time, physics stability | M3 (G0) | — |
| 6 | Build reusable material and interactive-object systems | M4 | — |
| 7 | Graybox all 60 levels using reusable prefabs and level data | M5 | — |
| 8 | Validate every intended level solution on actual target devices | M5 | — |
| 9 | Final world art, sound, VFX, UI polish and cosmetics (+ meta screens) | M6 | — |
| 10 | Integrate ads, IAP, consent, analytics, crash reporting, Remote Config — only after the core game is stable | M7 | — |
| 11 | UK closed test, then Canada/Australia soft launch | M9, M10 | — |
| 12 | Only after soft-launch data: optimise level balance, ad frequency, pricing, conversion | post-MVP | — |

**Locked technology (D-103):** Unity 6000.6.5f1 for the whole MVP (no upgrade, D-080) · URP · 3D PhysX constrained to the XY plane · 2.5D portrait diorama · kinematic swept arrow sharing `BallisticSolver` with the preview · `LevelData` SO + layout prefab · local JSON save only · uGUI + TextMeshPro · UI Toolkit for editor tools only · bundled content, no Addressables · Unity LevelPlay behind `IAdsService` · Firebase (Analytics, Crashlytics, Remote Config) behind service interfaces · Unity IAP behind `IIapService`.

**OWNER calendar tasks (project weeks, independent of milestone):**
- **Week 1** — verify the Google Play account type and the current closed-testing requirement; if it applies, start recruiting eligible testers (AB-167, D-099).
- **Week 3** — Mac access + Apple Developer account ready; iOS device builds from the VS on (AB-168, D-100).
- **Week 5** — Apple/Google developer app records + IAP products `remove_ads` (£3.99 / USD 3.99) and `starter_pack` (£2.99 / USD 2.99) created (AB-162, D-091, D-100).
- **Before M7 starts** — privacy policy hosted (AB-163, D-097).
- **M8** — legal/store-name/domain clearance for "Arrow Buster" before store assets (AB-156, D-081).

---

## 1. Vertical slice (summary)

Full specification, per-level design and the M3 ticket list: [`11_VERTICAL_SLICE.md`](11_VERTICAL_SLICE.md).

| Element | Slice content |
|---|---|
| Environment | One polished Greenwood Range section (training-valley diorama, ruins, grass, bright daytime) |
| Core action | Bow drag/draw/release (D-018), trajectory preview (D-085), Oak Arrow only |
| Objects | Timber crates/beams, red crest target, cuttable rope (D-009), royal vase (protected), water kill zone |
| Loop | Limited quiver, win/fail/soft-lock (D-015), re-nock 0.35 s with firing disabled after clear/Won/Failed (D-083), 1-tap restart (< 1 s), 1–3★ (`StarRules`) |
| Feedback | Basic material SFX (timber/rope/vase), break VFX, hit-stop, haptics (light/medium/success) |
| UX | One tutorial prompt (L1 ghost-hand + one "Aim for the rope" callout), clean HUD, polished Win and Fail screens |
| Data | Debug-sink analytics for the loop events (`level_started`, `arrow_fired`, `object_triggered`, `level_completed`, `level_failed`, `level_restarted`, `level_quit`) — no vendor SDK |
| Levels | VS-01 = W1_L01 direct hit · VS-02 = W1_L04 weak support · VS-03 = W1_L07 rope drop · VS-04 = W1_L10 protected-vase precision · VS-05 = W1_L16 combined (D-050) |
| Platforms | Android device build + iOS device build (Mac by week 3, D-100) |
| Gate question | Do 5–10 **external** casual players, with no instructions, discover weak points, enjoy the shot-to-collapse moment, and voluntarily replay for 3★? |

**Nothing else is built before the VS gate:** no special arrows, no world map, no cosmetics, no ads/IAP/SDKs, no content beyond the five VS levels.

---

## 2. Milestone overview

| ID | Name | Exec steps | Compressed ref (10 wk) | Realistic (project weeks, §8) | Exit gate (`07` §16) | Primary owners |
|---|---|---|---|---|---|---|
| M0 | Project setup | 1 | done | done (2026-10-08/09) | — | ARCH, INT |
| M1 | Foundations & Core Feel | 2 (+5 start) | Week 1 | Weeks 1–2 | **G-M1** Feel & Physics (PG-1…PG-6) | CORE, PHYS, ARCH, OWNER |
| M2 | Core Loop + VS Graybox | 3 | Week 2 | Weeks 3–4 | internal loop check | CORE, PROPS, LEVEL, UI, SYS, MON |
| M3 | Vertical Slice + External Playtest + Feel Lock | 3–5 | Week 3 | Weeks 5–6 (+1 iterate) | **G0** Vertical Slice & Feel Lock | ART, UI, CORE, LEVEL, QA, PO, OWNER |
| M4 | Systems Complete | 6 | Week 4 | Weeks 7–10 | **G1** Systems Complete | PROPS, PHYS, CORE, LEVEL, QA |
| M5 | Content Graybox + Device Validation | 7–8 | Week 5 | Weeks 11–15 | **G2** Content Graybox Complete | LEVEL, QA, PLAT |
| M6 | Art, Audio, UI Polish & Meta | 9 | Week 6 | Weeks 16–22 | **G3** Content & Art Complete | ART, UI, SYS, LEVEL |
| M7 | Platform Services & Monetisation | 10 | Week 7 | Weeks 23–25 | **G4** Feature Complete | MON, PLAT, UI, OWNER |
| M8 | Optimisation, Balance, QA & Release Prep | — | Week 8 | Weeks 26–28 | **G5** Release Candidate | PLAT, QA, LEVEL, UI, ART |
| M9 | UK Closed Test & Submission | 11a | Weeks 9–10 | Weeks 29–31 (incl. ≥ 14-day closed test) | **G-Release** | INT, PLAT, QA, PO, OWNER |
| M10 | Soft Launch (Canada, Australia) | 11b | after week 10 | Week 32+ | Soft-launch scale / iterate / kill (`07` §15) | MON, PO, OWNER |

Gate names (D-102, replacing D-078): **G-M1** (M1) · **G0** Vertical Slice & Feel Lock (M3) · **G1** Systems Complete (M4) · **G2** Content Graybox Complete (M5) · **G3** Content & Art Complete (M6) · **G4** Feature Complete (M7) · **G5** Release Candidate (M8) · **G-Release** (M9) · soft-launch criteria (M10). PG-1…PG-6 (`03` §1.1) and VS-G1…VS-G8 (`11` §8.1) are unchanged.

Complexity scale: **S** ≤ 0.5 day · **M** 1–2 days · **L** 3–5 days · **XL** > 1 week (split before starting, D-071).

---

## 3. Milestones in detail

Each milestone lists: goal · task breakdown (tickets) · dependencies · deliverable owner · complexity · acceptance criteria · exit gate · risks.

### M0 — Project setup ✅

Done 2026-10-08/09: URP, Android/iOS settings (portrait, IL2CPP/ARM64, iOS 15), folder map, 3 asmdefs, `GameConstants`, `GameEnums`, `LevelData`, `QuiverEntry`, `ProjectSetup` menu, 4 scenes in the build list, `LevelDataTests`, MCP for Unity, full planning set. **AB-001 done:** git + LFS + UnityYAMLMerge, baseline commit `7a06460` pushed to the private repo. Known debt is carried into AB-002/AB-016 (see `00` §13).

---

### M1 — Foundations & Core Feel

**Goal:** settings, layers, physics, test assemblies and folders in place (step 2), and a graybox where drawing the bow, reading the arc and knocking down a crate tower already feels good on a phone. Physics stacks are stable enough to commit to D-004.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-002 | Project hygiene: remove unused packages + template leftovers, PlayMode asmdef, `LayerSetup` + `PhysicsSetup` (D-006, D-048) — **next ticket** | ARCH + PHYS | M |
| AB-003 | Core skeleton: `Services`, `ServiceInstaller`, `GameEvents`, `StaticReset`, `Log`, `LevelClock`, `TimeScaleController` (D-061), service interfaces incl. `ICrashReportingService` with null/mock implementations | ARCH | M |
| AB-004 | Gameplay scene graybox: `GameplayRoot`, `CameraRig` + `CameraFramer` (D-041), ground, lighting, `PlayBounds` | CORE | M |
| AB-005 | `GameplayTuning` + `ArrowDefinition` + `AD_Oak` ("Spec" + "Snappy" presets, D-036) | CORE | S |
| AB-006 | `BallisticSolver` (pure) + EditMode tests incl. preview/flight parity | CORE | M |
| AB-007 | `BowInputReader` + `DrawModel`/`AimState` (D-018) + tests | CORE | M |
| AB-008 | `TrajectoryPreview` (pooled dots, impact marker; supports wind/portal/first-bounce continuation, D-085) | CORE | M |
| AB-009 | `ArrowProjectile` kinematic sweep + `ArrowSpawner`/`ArrowRegistry` (cap 8) | CORE | L |
| AB-010 | `MaterialProfile` + 5 graybox profiles + `MaterialBody` + `PlanarBody` | PHYS | M |
| AB-011 | `ArrowImpactResolver` rules + clamped impulses | CORE + PHYS | L |
| AB-012 | `Breakable` + `ImpactDamage` + `DebrisPool` | PHYS | M |
| AB-013 | Graybox structure prefab library v1 | PHYS | S |
| AB-014 | Physics stability spike + crate-tower sandbox + M1 feel-gate playtest | PHYS + QA + PO | M |
| AB-015 | `DevOverlay` (AB_DEV) + cheats | CORE | S |
| AB-047 | `BuildScript` v1 + Android dev APK (needed for the on-device feel gate) | PLAT | M |
| AB-167 | **OWNER week 1:** Google Play account type + closed-testing requirement verified; tester recruitment started if it applies (D-099) | OWNER + PLAT | S |
| AB-168 | **OWNER by week 3:** Mac + Apple Developer account + one signed empty iOS device build (D-100) | OWNER + PLAT | S |

- **Dependencies:** AB-001 ✅ → everything. AB-002 → AB-010/AB-012 (layers). AB-003 → AB-004/AB-009/AB-015. AB-005 + AB-006 → AB-008/AB-009. AB-009 + AB-010 → AB-011 → AB-012 → AB-014. AB-047 → AB-014 (device test), AB-168.
- **Deliverable owner:** CORE (bow/arrow), PHYS (stability), INT (integration on `main`), OWNER (accounts).
- **Milestone complexity:** L (17 tickets, 2 on the critical path at L).
- **Acceptance criteria (measurable):**
  1. Preview vs. actual flight deviation ≤ 1 mm over 3 s (EditMode parity test green).
  2. No arrow tunnelling: 1,000 automated shots at 0.1 m-thick planks and 0.06 m rope colliders across the full power range → 100% hit registration (PlayMode test).
  3. Physics spike gates **PG-1…PG-6** in `03` §1.1 all pass (idle drift < 1 cm, spawn safety, collapse calm < 3 s in 9/10 runs, repeatability ≥ 19/20, worst-frame physics ≤ 5 ms on Mid, zero Z drift) — recorded in `docs/qa/test-runs/`.
  4. Android dev APK runs ≥ 58 FPS median on a Mid-tier device in the sandbox. 0 B/frame GC in gameplay (Profiler).
  5. Zero Console errors/warnings from project code. All EditMode tests green.
  6. AB-167 result recorded (week 1). AB-168 iOS empty build signed and run (by week 3).
- **Exit gate — G-M1 (PO + OWNER):** ≥ 5 testers try both "Spec" and "Snappy" presets on device. Pick one (D-036 → Accepted) and confirm or adjust the draw mapping (D-018). **If physics gate PG-1 or PG-2 (`03` §1.1) fails after 2 days of tuning (incl. TGS solver A/B), trigger the D-004 fallback review** (2D physics) before M2 starts.
- **Risks & mitigations:** T-01 stacking instability → spike first, mass ratios ≤ 10:1, start asleep. T-02/T-03 → shared solver + sweep. "Floaty" feel → two presets compared on device, not in the editor. Store/Mac lead times → OWNER week-1/3 tasks.

---

### M2 — Core Loop + VS Graybox

**Goal:** a complete, data-driven level loop for the **five VS levels only**: load level → shoot → objectives/protected/rope/kill zones → win/fail/soft-lock → stars → save → next/retry.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-016 | `LevelData` schema v2 + `LevelLayout` + `LevelLoader` (restart < 300 ms) | LEVEL + CORE | L |
| AB-017 | `QuiverModel` + `StarRules` (bonus clear = completed, max 1★, D-084) + tests | CORE | S |
| AB-018 | `Objective` + `ObjectiveClearRule` + `ObjectiveTracker` (CrestTarget, SupplyCrate) + `ProtectedObject`/`ProtectedTracker` (RoyalVase) | PROPS | L |
| AB-019 | `SettleMonitor` + `GameplayController` state machine (D-015, D-083) + tests | CORE + PHYS | L |
| AB-020 | `RopeCuttable` + `RopeView` (D-009) | PROPS | M |
| AB-021 | `KillZone` (Water/Spikes/Pit) + clear line (D-039) | PROPS | S |
| AB-022 | `JsonSaveService` v1 + `SaveMigrator` scaffold + `ProgressionService.RecordResult` + tests | SYS | M |
| AB-023 | Graybox HUD + Win/Fail/Pause panels + `SafeAreaFitter` | UI | M |
| AB-024 | `IAnalyticsService` + `DebugAnalyticsService` + loop events (no vendor SDK) | MON | S |
| AB-025 | VS levels VS-01..05 graybox + `ShotRecorder` + intended shots + solvability bot v1 | LEVEL + QA | L |

- **Dependencies:** G-M1 passed (flight tuning fixed before authoring geometry, D-036). AB-016 → AB-025. AB-018 + AB-020 + AB-021 → AB-019 → AB-023. AB-017 → AB-019/AB-022. AB-003 → AB-024.
- **Deliverable owner:** CORE (loop), LEVEL (VS content), INT.
- **Milestone complexity:** L (10 tickets). *(The former "5 extra graybox levels" ticket AB-046 moved to M5 — D-102 step 3: VS only.)*
- **Acceptance criteria:**
  1. All 5 VS levels load from `LevelData` only. No level-specific code. `LevelLoader` restart p95 < 300 ms in the editor and < 1 s on the Mid device.
  2. Win requires all required objectives cleared + 0.75 s calm (cap 3 s). Protected loss → immediate fail with a reason. Out-of-arrows fail after 2 s calm (cap 5 s). Soft-lock toast appears. Firing re-enabled 0.35 s after release while physics resolves, disabled once all objectives are cleared and in Won/Failed (D-083). All covered by EditMode state-machine tests with a fake clock.
  3. Stars computed by `StarRules`; best stars persisted across app restarts (`save.json` verified).
  4. Solvability bot v1: each VS level's recorded intended shots win in the editor 10/10 runs.
  5. Debug analytics CSV contains the loop events with the §13 params for a full play of all 5 levels.
  6. A 1-tap Retry from any state, including mid-flight and mid-collapse.
- **Exit check (PO + INT, internal):** an internal play-through of the 5 VS levels on device with no blockers. The VS level designs (`11`) are approved for art.
- **Risks & mitigations:** settle edge cases (T-05) → `SettleMonitor` ignores kinematic/ambient bodies, hard caps. Scope creep → no new mechanics, HUD graybox only, polish is M3.

---

### M3 — Vertical Slice + External Playtest + Feel Lock

**Goal:** the 5 VS levels at "polished slice" quality in one Greenwood section, played by **external** casual players; the core feel is then **locked** (D-102 steps 3–5).

| Ticket(s) | Scope | Owner |
|---|---|---|
| AB-026 … AB-045 | Defined in [`11_VERTICAL_SLICE.md`](11_VERTICAL_SLICE.md) §4.2: Greenwood section art v1, bow hero v1, crate/target/vase/rope art v1, `FeedbackDirector`, `AudioService`, `VfxService`, `HitStop`, `HapticsService`, tutorial prompt system, `UiTween`, polished HUD + Win/Fail, `LC_VerticalSlice` playlist, Android build, QA pass, playtest | ART, UI, CORE, LEVEL, PLAT, QA, PO |
| AB-158 | `AssetImportRules` + placeholder build check (D-055/D-056) | ART + PLAT |
| AB-169 | iOS VS device build on the Mac (touch feel, safe areas, haptics, perf smoke; no SDKs) | PLAT |
| AB-162 | **OWNER by week 5:** Apple/Google app records + IAP products at the D-091 price tiers | OWNER + PLAT |

- **Dependencies:** M2 exit. AB-168 (Mac) → AB-169. Art direction exploration may start in M2 (ART, non-blocking).
- **Deliverable owner:** ART (look and feel), UI (screens), QA (playtest), PO + OWNER (gate).
- **Milestone complexity:** L–XL (art-bound; 23 tickets).
- **Acceptance criteria:**
  1. All 5 VS levels playable in order from the playlist on Android **and iOS** at ≥ 58 FPS median on Mid.
  2. Feedback present on every meaningful impact: SFX per material, break VFX, hit-stop 40–70 ms, haptics (draw threshold light, impact medium, clear success). All toggleable in a minimal settings stub.
  3. The VS acceptance criteria A-1…A-12 in `11` §7 are met before the playtest.
  4. Playtest with 5–10 **external** casual players, no instructions, scored against **VS-G1…VS-G8** in `11` §8.1.
  5. A 5-second screenshot test: ≥ 4/5 testers name the objective and one weak point per level.
  6. **Feel lock (step 5) recorded in the decision log:** bow feel (D-018 mapping + D-036 preset final), trajectory accuracy (parity ≤ 1 mm), arrow collision reliability (1,000-shot tunnelling suite green), restart time (p95 < 1 s on Android + iOS), physics stability (PG-1…PG-6 green with art prefabs). Zero crashes in the playtest sessions.
- **Exit gate — G0 Vertical Slice & Feel Lock (OWNER go/no-go):** decision rules in [`11` §8.1](11_VERTICAL_SLICE.md#81-go--no-go-thresholds): **GO** → feel locked, art direction provisionally locked, systems production starts. **GO with fixes** → fix tickets added to M4. **NO-GO** → one iteration week on feel/readability, then retest with new external players; a second NO-GO escalates to OWNER before any further work (do not start M4).
- **Risks & mitigations:** art bottleneck (S-02) → low-poly gradient-atlas style, one section only. Playtester recruitment → book external testers during M2 (not friends who have seen the game). iOS surprises → Mac by week 3 (AB-168).

---

### M4 — Systems Complete

**Goal:** every reusable material, objective, protected object, prop, hazard and special arrow for all three worlds exists and is tested in sandbox/micro-levels, and levels can be produced without code (D-102 step 6). No campaign levels are built yet.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-048 | Material profiles pass: Straw, Stone final; Ice/Metal graybox-ready | PHYS | M |
| AB-049 | Heavyhead arrow (`AD_Heavyhead`) | CORE | M |
| AB-050 | `ExplosionSolver` + `Explosion` (no damage/force on Protected, D-092) | PHYS | M |
| AB-051 | `PowderBarrel` prop + chain delay | PROPS | M |
| AB-052 / 053 / 054 / 055 | HangingLantern, CursedOrb, TrainingDummy, BannerRope objectives | PROPS | S–M |
| AB-056 | SleepingFox protected (D-038) | PROPS | M |
| AB-057 | `BullseyeMarker` + award on win (D-045) | PROPS + SYS | S |
| AB-058 | Prefab library v2 + `PrefabValidator` | PHYS + PROPS | M |
| AB-059 / 060 / 061 | `LevelValidator`, `LevelEditorWindow`, `WorldData`/`LevelCatalog` + builder | LEVEL | L / L / M |
| AB-062 | Solvability bot v2 (`07` §5.1 jitter grid) + idle stability | QA | M |
| AB-063 / 095 / 109 | Interaction-matrix PlayMode tests (W1, W2, W3 mechanic cells) | QA | M each |
| AB-067 | `ArrowRevealCard` | UI | S |
| AB-068 | CI: GitHub Actions + GameCI | INT + ARCH | M |
| AB-085 / 086 / 087 | `Balloon`, `Burnable` + `FireZone`, `OilJar` | PROPS | M / M / S |
| AB-088 | Fire arrow | CORE | M |
| AB-089 | Split arrow (timed split, preview marker + child arcs, pre-split pulse, D-088) | CORE | L |
| AB-090 | `KinematicMover` + moving/rotating shields | PROPS | M |
| AB-091 / 092 | Rolling boulders; levers/seesaws + rope-hung weights (no pulleys, D-087) | PROPS / PHYS | S |
| AB-094 | Metal ricochet tuning | PHYS | S |
| AB-104 | Ice behaviour | PHYS | M |
| AB-105 | `WindField` (wind shown in the preview, D-085) | PROPS + CORE | M |
| AB-106 | Bounce arrow (first bounce in the preview) | CORE | M |
| AB-107 | `PortalRing`/`PortalPair` (exit path in the preview) | PROPS + CORE | L |
| AB-159 | `IntendedSolutionRunner` (on-device replay) | QA + CORE | S |
| AB-160 | `MP_Earth` + `Ground` tag (D-059) | PHYS | S |
| AB-170 | Preview first bounce + wind/portal/bounce parity tests (D-085) | CORE | M |

*Not built:* `SpringPlate` (AB-093, cut D-086), moving ice platforms (AB-108, cut D-104).

- **Dependencies:** G0 GO. AB-050 → AB-051. AB-049 → AB-089. AB-086 → AB-087/AB-088. AB-094 → AB-170 → AB-106. AB-085 → AB-105. AB-059/AB-061 → AB-062.
- **Deliverable owner:** PROPS + PHYS (systems), CORE (arrows/preview), LEVEL (tooling), QA (matrix tests).
- **Milestone complexity:** XL (36 tickets; parallelises well across agents — code-only tickets in worktrees).
- **Acceptance criteria:**
  1. Every non-empty cell of the `03` interaction matrix has a green PlayMode test (AB-063/095/109).
  2. All four special arrows work in sandbox micro-levels; Split preview shows the split marker + 3 child arcs; wind, portal exit and first bounce previews match flight in parity tests (≤ 1 mm over 3 s / ≤ 2 cm at impact).
  3. Shields/movers deterministic across restarts (same pose at the same `LevelClock` time). Explosions never move or damage Protected objects (test).
  4. `LevelValidator`, `PrefabValidator`, bot v2 and idle-stability suite run from menu and CI; a new level can be created by a non-programmer flow (editor window → prefab → LevelData → validate → bot) in ≤ 30 min (timed dry-run).
  5. PG-1…PG-6 still green; 0 Console errors/warnings.
- **Exit gate — G1 Systems Complete (PO + ARCH):** every system needed by the 60-level plan in `04` exists with tests; no level in `04` §11 needs a new component.
- **Risks:** tooling overreach → validator and bot are P0, the editor window is P1 (inspector-only fallback). Explosion/fire chain blow-ups (T-06) → clamps, timer-based fire, matrix tests. Portal/bounce brittleness → aim windows enforced by the bot in M5.

---

### M5 — Content Graybox + Device Validation

**Goal:** all 60 levels in graybox with recorded intended solutions, validated in the editor **and on actual target devices** (D-102 steps 7–8). Fun is validated in graybox playtests before any final art is applied.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-046 | W1 graybox batch 0: L02, L03, L05, L06, L08 | LEVEL | M |
| AB-064 / 065 / 066 | W1 graybox batches A–C (L09–L15 incl. Heavyhead at W1_L13; L17–L20 incl. boss) | LEVEL | M / L / L |
| AB-100 / 101 | W2 graybox L21–30 (Split at W2_L10 = L30) and L31–40 (Fire at W2_L16 = L36, L40 boss) | LEVEL | L |
| AB-114 / 115 | W3 graybox L41–50 (Bounce at W3_L07 = L47) and L51–60 (static ice slides L53–55, L60 finale) | LEVEL | L |
| AB-083 | Intended-solution docs for all 60 | LEVEL | M |
| AB-084 / 103 / 118 | Graybox playtest rounds per world (external players) | QA + PO | M |
| AB-175 | On-device validation of all 60 intended solutions (Low Android + iPhone 11-class) | QA + PLAT | L |
| AB-177 | Graybox perf baseline on set pieces + body-budget fixes | PLAT + PHYS | M |

- **Dependencies:** G1. AB-059/AB-062 → every batch. AB-159 + AB-168 → AB-175. Batches → AB-083 → AB-175.
- **Deliverable owner:** LEVEL (content), QA (validation), PLAT (devices).
- **Milestone complexity:** XL (14 tickets, content-bound; agents graybox via MCP, the human judges fun).
- **Acceptance criteria:**
  1. 60/60 levels in the catalogs with `status = Graybox` (`04` §1), `LevelValidator` 0 errors, `docs/levels/W#_L##.md` complete.
  2. Bot green on the `07` §5.1 jitter grid for all 60 (all 9 samples win, ≥ 7/9 within par); idle stability < 1 cm in 3 s.
  3. **Device layer:** `IntendedSolutionRunner` reproduces the editor outcome for every level on one Low Android and one iPhone 11-class device; mismatches fixed or redesigned.
  4. Special arrows first appear exactly at L13 / L30 / L36 / L47 (validator V-11, D-082); two-tutorial-levels cadence holds; no repeated solution archetype within 10 levels.
  5. Graybox playtests: ≥ 80% of W1 testers clear W1 unaided; median retries 1–3 per normal level; no level with ≥ 20% quit before first shot.
  6. Set pieces: Low ≥ 29 FPS median, Mid ≥ 58 FPS median in graybox; restart < 1 s.
- **Exit gate — G2 Content Graybox Complete (PO + LEVEL + QA):** all of the above; difficulty curve reviewed against `04` §13.
- **Risks:** content throughput → templates + agents; deep cut D1 (40 levels) is the OWNER escape hatch (§9). Device/editor physics mismatch (T-04) → found here, before art, when redesign is cheap.

---

### M6 — Art, Audio, UI Polish & Meta

**Goal:** final world art, audio, VFX, UI polish, cosmetics and meta screens on top of validated content (D-102 step 9). Levels move to `Final`.

| Group | Tickets | Owner |
|---|---|---|
| Art direction + kits | AB-069 (lock doc, OWNER sign-off), AB-070 Greenwood, AB-096 Sunscar, AB-110 Frostspire (XL → 3 L each) | ART |
| Object art | AB-071 (W1), AB-097 (W2), AB-111 (W3), AB-072 bow hero final | ART |
| Audio / VFX | AB-073, AB-074, AB-075 (+ Greenwood music), AB-098, AB-099, AB-112, AB-113, AB-161 `ChainReactionTracker`, AB-171 split pulse VFX | ART (+ CORE) |
| Level final passes | AB-082 (W1), AB-102 (W2), AB-116 (W3) | LEVEL |
| Front end + meta | AB-076/AB-119 World Map, AB-077 Home, AB-078 `ProgressionService`, AB-079 first-session flow, AB-080 Settings, AB-081 Pause/intro/fail copy, AB-120 Economy, AB-121 Inventory, AB-122 Bow Forge, AB-123 cosmetics (3 skins + 2 trails), AB-124 Daily Challenge (**cut-first**), AB-125 Bullseye view (P2), AB-126 save schema full, AB-137 static splash, AB-164 dialogs/credits | UI, SYS, ART |
| OWNER / platform | AB-163 privacy policy hosted **before M7 starts**, AB-166 early Play closed track (if D-099 applies) | OWNER, MON, PLAT |
| Playtest | AB-176 post-art full-game playtest | QA + PO |

Parallel-track allowance: environment-kit modelling may start right after G0 and run during M4–M5; integration into levels happens here. No SDK code in M6 (an isolated compile-spike branch late in M6 is allowed, never merged before M7).

- **Dependencies:** G2. AB-069 → kits → object art → final passes. AB-078 → AB-076/AB-119/AB-120 → AB-121 → AB-122/AB-123.
- **Milestone complexity:** XL (39 tickets; art-bound).
- **Acceptance criteria:**
  1. 60/60 levels at `status = Final`; validator 0 errors; bot layers 1–2 green on art builds.
  2. Cold start → first shot ≤ 15 s on Mid (median of 5 fresh installs); first launch drops into W1_L01.
  3. Launch cosmetic set only (D-101): Oak Ranger, Moonwood, Royal Amethyst (D-095) + Gold Spark, Leaf Swirl; cosmetics never change physics/aim (test).
  4. World map with 3 worlds, 15/20 unlock rule, stars, locked teasers; progression and save survive app restarts and migrations.
  5. Art direction document signed by OWNER with the D-089 originality/licensing/AI checklist; `docs/art/ASSET_LICENSES.md` complete; no AI-generated shipped assets without documented rights.
  6. Post-art playtest (AB-176): ≥ 80% complete W1 unaided; fairness rating ≥ 4/5; no readability regressions vs graybox.
  7. Privacy policy URL live (AB-163) before M7 starts.
- **Exit gate — G3 Content & Art Complete (OWNER + PO).**
- **Risks:** art throughput (S-02) → measure pieces/day at week 18; contract environment kits if < 60% of plan. Meta scope creep → Daily Challenge is cut-first; Bullseye view P2.

---

### M7 — Platform Services & Monetisation

**Goal:** integrate ads, IAP, consent, analytics, crash reporting and Remote Config — only now that the core game is stable (D-102 step 10). Vendors are fixed (D-090): **Firebase** (Analytics, Crashlytics, Remote Config) + **Unity LevelPlay** (ad mediation) + **Unity IAP**, all behind adapters in `ArrowBuster.Integrations`.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-117 | SDK integration baseline: Firebase + LevelPlay + UMP + Unity IAP compile on Android + iOS; test ads | MON + PLAT | L |
| AB-127 | `AdPolicy` (D-093) + exhaustive tests | MON | M |
| AB-128 | `LevelPlayAdsService` (rewarded + interstitial) | MON | L |
| AB-129 | Rewarded +1 arrow flow (D-062, D-063, D-084) | UI + CORE | M |
| AB-130 | Rewarded 2× coins (never blocks Next) | UI + SYS | S |
| AB-131 | Unity IAP: `remove_ads` £3.99 / USD 3.99, `starter_pack` £2.99 / USD 2.99, restore (D-091) | MON | L |
| AB-132 | Consent: UMP + ATT + consent-aware init (analytics, ad personalisation, non-essential crash reporting gated in UK/EEA, D-096) | MON + UI | L |
| AB-133 | `FirebaseAnalyticsService` + full event dictionary | MON | M |
| AB-134 | `FirebaseCrashReportingService` (`ICrashReportingService`) + IL2CPP symbols | PLAT + MON | M |
| AB-135 | `FirebaseRemoteConfigService` + whitelisted keys | MON | M |
| AB-136 | `ShopScreen` + Home offers | UI | M |
| AB-165 | iOS build path with SDKs (`iOSPostProcess`, ATT string, privacy manifests) | PLAT | M |
| AB-172 | "Bonus Arrow Used — 1★ Max" disclosure before the ad (D-084) | UI | S |

- **Dependencies:** G3 **and** AB-163 (privacy policy hosted) before AB-117 starts. AB-162 (store products) → AB-131. AB-168 → AB-165.
- **Milestone complexity:** L (13 tickets; SDK build risk T-09).
- **Acceptance criteria:**
  1. `AdPolicy` tests cover every D-093 clause: no interstitial before 10 min cumulative active play per install; ≥ 3 completed levels and ≥ 120 s since the last; only on Win → Next/Home; never after a fail, rewarded ad, purchase, onboarding (L1–L5) or app resume; all thresholds remote-configurable. Rewarded is opt-in only; max one +1-arrow offer per attempt; Remove Ads removes interstitials only.
  2. The +1 arrow offer shows "Bonus Arrow Used — 1★ Max" before the player accepts; a bonus clear counts as completed and records 1★ max.
  3. Purchase, restore and Remove Ads work in sandbox on both stores; Starter Pack grants Royal Amethyst + Gold Spark + 500 coins and nothing gameplay-relevant.
  4. Consent: no Firebase Analytics/Crashlytics collection and no personalised ads before consent in UK/EEA (verified via logging proxy + debug views). ATT after UMP on iOS. Declining still yields a fully playable game.
  5. All events in the `06` dictionary appear in the Firebase debug view with correct params; a crash test build reports a symbolicated crash on both platforms (after consent).
  6. Offline launch: game fully playable, ads unavailable gracefully, no blocking spinners > 2 s.
  7. `ArrowBuster.Runtime` has no vendor references (`ArchitectureRulesTests`); project compiles with SDKs removed.
- **Exit gate — G4 Feature Complete:** no new features after this point (bug fixes and balance only).
- **Risks:** T-09 SDK build breakage → EDM4U pinned, Android first then iOS, Mac build check weekly. Consent legal review (D-096) → OWNER obtains final privacy review before G-Release.

---

### M8 — Optimisation, Balance, QA & Release Prep

**Goal:** meet perf budgets on the device matrix, balance all 60 levels, finish accessibility, run full regression, produce store assets after name clearance.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-138 | `QualityTierSelector` + per-tier URP assets + runtime guard | PLAT | M |
| AB-139 | Device profiling pass across the matrix + fixes | PLAT | L |
| AB-140 | Memory + download-size pass | PLAT + ART | M |
| AB-141 | Physics perf pass on art set pieces | PHYS | M |
| AB-142 | Balance pass all 60 (par, quiver, preview scale ≥ 0.4) | LEVEL + PO | L |
| AB-143 | Accessibility: colour-assist outlines, reduced particles/motion, icon redundancy | UI + ART | M |
| AB-144 | Full regression + on-device solvability on art builds (all 60) | QA | L |
| AB-146 | Haptics tuning on the device matrix | PLAT | S |
| AB-147 | Localisation-readiness audit | UI | S |
| AB-148 | Analytics QA incl. consent-off path | QA + MON | M |
| AB-149 | App lifecycle (pause/resume, interruptions, no interstitial on resume) | PLAT | M |
| AB-156 | Legal / store-name / domain clearance for "Arrow Buster" (D-081) | PO + OWNER | S |
| AB-150 | Store listing assets (after AB-156) | ART + PO | L |

*Removed:* the Unity LTS upgrade spike (AB-145) — Unity 6000.6.5f1 is locked for the MVP (D-080).

- **Acceptance criteria:** budgets in `01` §11 met on all matrix devices (Mid 60 FPS ≥ 95% of frames on the 10 heaviest levels; Low 30 FPS ≥ 95%). Resident memory ≤ 450 MB on Low. Download ≤ 150 MB. All 60 levels bot-green on a Mid Android + an iPhone 11-class device. 0 open P0/P1 bugs. Accessibility checklist in `07` passed. Name clearance documented before any store asset is final.
- **Exit gate — G5 Release Candidate (`07` §16):** content and perf freeze; only release-blocking fixes after this.

---

### M9 — UK Closed Test & Submission

**Goal:** a UK closed test (TestFlight + Google Play closed track), funnel fixes, and a submission that passes release gates (D-098).

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-151 | App Privacy labels, Play Data safety, SDK privacy manifests vs. the final policy | MON + OWNER | M |
| AB-152 | Release build pipeline: signing, AAB, Xcode archive, versioning | PLAT | M |
| AB-153 | UK closed test distribution (TestFlight + Play closed track) | PLAT + QA | M |
| AB-154 | Closed-test funnel review + fixes | MON + PO | L |
| AB-155 | Store compliance + age-rating questionnaires | PLAT + PO | S |
| AB-157 | Release-gate sign-off + submission | INT + QA + PO | S |

- **Acceptance criteria:** closed test per `07` §14 with UK testers: ≥ 20 external testers for ≥ 14 days (or the Play-required tester count/duration if longer — known since week 1 via AB-167; if it applies the Play track started early via AB-166). Success metrics in `07` §14 met (incl. ≥ 80% of W1 starters complete W1 unaided, median retries 1–3, fairness ≥ 4.0/5). Level 1 completion ≥ 95%. Crash-free sessions ≥ 99.5% (consented users). No forced ad in the first 10 cumulative minutes (telemetry-verified). Final legal/privacy review done (D-096). Release gates in `07` all green.
- **Exit gate — G-Release (`07` §16):** OWNER approves submission.

---

### M10 — Soft Launch (Canada, Australia)

**Goal:** release in Canada and Australia (English only) and decide scale / iterate / kill from real data (D-098, D-102 step 11).

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-173 | Soft-launch release in Canada + Australia (country availability, production Remote Config, privacy-law check PIPEDA / Quebec Law 25 / Australian Privacy Act) | PLAT + OWNER | M |
| AB-174 | Soft-launch KPI read-out (`07` §15) → scale / iterate / kill decision logged | MON + PO + OWNER | M |

- **Acceptance criteria:** KPI read-out against `07` §15 after the agreed observation window; decision recorded in `10_DECISION_LOG.md`.
- **After M10 (step 12, post-MVP):** only then optimise level balance, ad frequency, pricing and conversion, with A/B tests per `06` §15.

---

## 4. Sprint / phase plan

**Cadence:** 1-week sprints. In the realistic plan, one milestone spans 2–7 sprints.

| Day | Ritual | Who |
|---|---|---|
| Mon | Sprint planning: pick tickets meeting Definition of Ready, assign roles, claim file locks (`docs/agents/FILE_LOCKS.md`) | PO + INT (+ OWNER) |
| Mon–Thu | Agent tasks in branches/worktrees; one editor-bound task per Unity instance (max 2) | Specialist agents |
| Daily | INT squash-merges green branches to `main` using the integration checklist (D-069) | INT |
| Thu | Device build: Android every sprint; iOS every sprint from M3 (Mac by week 3) | PLAT |
| Fri | Playtest/feel review, bot run over the catalog, sprint review, decision-log updates | PO, QA, OWNER |

**Phases (grouping milestones):**

| Phase | Milestones | Theme | Output |
|---|---|---|---|
| A — Prove | M1–M3 | Foundations, feel, loop, vertical slice, external playtest | Go/no-go on the fun; core feel locked |
| B — Systems | M4 | All reusable mechanics + tooling | Repeatable content machine |
| C — Content | M5 | 60 levels graybox, device-validated | Fun and solvability proven before art |
| D — Polish | M6 | Art, audio, VFX, UI, meta, cosmetics | Content & art complete |
| E — Business | M7 | SDKs, consent, monetisation | Feature complete |
| F — Ship | M8–M10 | Perf, balance, QA, UK closed test, CA/AU soft launch | Release + soft-launch decision |

---

## 5. Dependency graph

```
AB-001 ✅ ─► AB-002 ─┬─► AB-010 ─► AB-011 ─► AB-012 ─► AB-013 ─► AB-014 ─► G-M1
                     │                 ▲
             AB-003 ─┼─► AB-004 ───────┤        AB-047 (Android build) ─► AB-014
                     │                 │        AB-167 (wk 1) · AB-168 (wk 3, Mac)
             AB-005 ─┴─► AB-006 ─► AB-008
                          └──► AB-009 ─┘   AB-007 ─► AB-009   AB-015 ◄─ AB-003

G-M1 ─► AB-016 ─┬─► AB-025 (5 VS levels only)
        AB-017 ─┤
        AB-018 ─┼─► AB-019 ─► AB-023 ;  AB-022 ;  AB-024 (debug analytics)
        AB-020 ─┤
        AB-021 ─┘
              ▼
   M2 check ─► AB-026..045 + AB-158 + AB-169 (iOS) ─► AB-045 external playtest ─► G0 (feel lock)
                                                       AB-162 (wk 5: store records + IAP products)
              ▼
   M4 systems: materials · objectives · protected · 8 props · 4 special arrows · AB-170 preview
               · validator/editor/catalog/bot v2 · matrix tests ─► G1
              ▼
   M5 content: graybox W1/W2/W3 batches ─► AB-083 docs ─► AB-175 device validation ─► G2
              ▼
   M6 polish: AB-069 lock ─► kits ─► object art ─► final passes · meta/UI/cosmetics ─► G3
              │                                   AB-163 privacy policy (before M7)
              ▼
   M7 SDKs: AB-117 ─► LevelPlay / IAP / consent / Firebase adapters ─► G4
              ▼
   M8 perf/balance/QA/store assets ─► G5 ─► M9 UK closed test ─► G-Release ─► M10 CA/AU soft launch
```

Cross-milestone hard dependencies:
- `BallisticSolver` environment hooks (wind/portal/first bounce, D-085) are designed in AB-006/AB-008 and filled in AB-105/AB-107/AB-170 (M4).
- `LevelValidator` (AB-059) and bot v2 (AB-062) must exist before any campaign content beyond the VS (M5).
- `AdPolicy` (AB-127) needs `ProgressionService` cumulative-playtime and level counters (AB-078, M6).
- SDK integration (AB-117) needs the hosted privacy policy (AB-163), store products (AB-162) and the Mac (AB-168).

---

## 6. Critical path

```
[AB-002 settings/layers] → [AB-006 solver] → [AB-009 arrow] → [AB-011 impact] → [AB-012 breakable]
  → [AB-014 PHYSICS + FEEL GATE G-M1] → [AB-016 level schema/loader] → [AB-018 objectives] → [AB-019 game loop]
  → [AB-025 VS levels] → [M3 art + feedback polish] → [AB-045 EXTERNAL PLAYTEST → G0 FEEL LOCK]
  → [M4 systems + AB-059 validator + AB-062 bot → G1] → [M5 60-level graybox → AB-175 device validation → G2]
  → [AB-069 art lock → 3 environment kits → final passes → G3] → [AB-163 privacy policy → AB-117 SDKs → G4]
  → [M8 device perf + balance → G5] → [M9 UK closed test ≥ 14 days → G-Release] → [M10 CA/AU soft launch]
```

**Critical-path owners:** CORE (M1–M2), ART (M3, M6 environment kits = the longest chain), LEVEL (M5 content), MON/PLAT (M7 SDKs), OWNER (gates, accounts in weeks 1/3/5, privacy policy, closed-test duration).
**Near-critical:** environment-kit modelling — start it right after G0 on the parallel track, or M6 becomes the bottleneck.

---

## 7. Compressed 10-week reference (vs. mvp §14)

| Week | mvp §14 deliverable | This roadmap (D-102) | Deviation and why |
|---:|---|---|---|
| 1 | Graybox bow input, ballistic arrow, camera, one breakable tower | **M1** AB-002…AB-015, AB-047 (+ AB-167 OWNER) | Adds settings/layers, the stability spike and an on-device feel gate |
| 2 | Objective/fail/win loop, restart, 10 graybox levels | **M2** — loop + **5 VS levels only** | Only the VS levels are built before the external playtest (step 3) |
| 3 | Materials, ropes, targets, protected objects, level data format | **M3** Vertical Slice + external playtest + feel lock (+ AB-168 Mac) | **Major deviation:** one-section art + feedback pulled forward from week 5 to prove fun first |
| 4 | Heavyhead, balloons, powder barrels; first 20 levels | **M4** all systems for all worlds (+ AB-162 by week 5) | Systems for all three worlds are built before any campaign level |
| 5 | Art direction lock, Greenwood final art, VFX/SFX/haptics | **M5** graybox all 60 + device validation | Content and device validation precede final art |
| 6 | Fire, split arrow, Sunscar content to 40 | **M6** art, audio, UI polish, meta, cosmetics | Art for all worlds in one milestone |
| 7 | Ice, bounce, portal, Frostspire content to 60 | **M7** SDKs, consent, monetisation | SDKs only after the core game is stable |
| 8 | World map, cosmetics, economy, ads/IAP, analytics | **M8** optimisation, balance, QA, store assets | Meta moved to M6, SDKs to M7 |
| 9 | Device optimisation, balance, QA, accessibility, consent | **M9** UK closed test + submission | Consent moved to M7 (it gates SDK init) |
| 10 | Closed test, funnel fixes, store assets, submission candidate | **M9** continues (≥ 14-day closed test) → **M10** soft launch after week 10 | Soft launch in Canada/Australia added as M10 |

**Honest assessment:** the 10-week reference is achievable only with a dedicated artist (or contracted kits), a second engineer, or heavy cuts. For one developer + AI agents, plan with §8.

---

## 8. Realistic contingency plan — 1 developer + AI agents

### 8.1 Capacity assumptions

| Assumption | Value |
|---|---|
| Developer focused hours | ~30 h/week (rest: admin, reviews, playtests, store setup) |
| AI-agent leverage | ~2–3× on well-specified code/tool/test tickets; ~1.2× on level graybox (agents build via MCP, the human judges fun); **~1× on art direction, feel tuning, device testing, playtests, store/legal** |
| Concurrent Unity instances | ≤ 2 (D-034) |
| Art | Either (a) the developer produces low-poly gradient-atlas art with agent help for kit assembly/import, or (b) a contracted 3D artist for environment kits starting after G0. **Plan assumes (a) with optional (b).** |
| Level throughput (after M4 tooling) | Graybox: 3–4 levels/day. Final art + balance: 2 levels/day. |
| Buffer | 1 iterate week reserved at the VS gate + 2 unallocated weeks |

### 8.2 Schedule (project weeks; ≈ 31 weeks to submission, soft launch from week 32)

| Weeks | Milestone | Agent-parallelisable work | Human-bound work |
|---|---|---|---|
| 1–2 | M1 Foundations & Core Feel | Settings/layers tools, solver, input model, tests, pooling, DevOverlay | Feel tuning on device, stability judgement; **wk 1: Play account check (AB-167)** |
| 3–4 | M2 Core Loop + VS graybox | Objectives, rope, kill zones, save, HUD graybox, analytics sink | VS level design (fun), approving designs; **wk 3: Mac + Apple account (AB-168)**; recruit external testers |
| 5–6 (+1) | M3 Vertical Slice + playtest | Feedback wiring, UiTween, tutorial system, build scripts, iOS build | **Art direction**, slice art, SFX selection, **external playtest**, go/no-go, feel lock; **wk 5: store records + IAP products (AB-162)** |
| 7–10 | M4 Systems Complete | All props/objectives/arrows, validator, editor window, catalog, bot v2, matrix tests, CI | Mechanic feel reviews; environment-kit modelling may start (parallel track) |
| 11–15 | M5 Content graybox + device validation | Level graybox via MCP, intended shots, bot runs, solution docs | Fun judgement, graybox playtests, **on-device validation** |
| 16–22 | M6 Art, audio, UI polish & meta | Kit import/prefab variants, map/home/forge UI, progression, economy, save | **Art** (kits, objects, bow), music/SFX selection, art-lock sign-off, post-art playtest; **privacy policy hosted by wk 22** |
| 23–25 | M7 Platform services & monetisation | Adapters, `AdPolicy`, consent flow, IAP flow, analytics mapping | SDK console setup, consent review, sandbox purchases on devices |
| 26–28 | M8 Optimisation, balance, QA, release prep | Profiling scripts, regression runs, bot over catalog, analytics QA | **Device-matrix testing**, balance judgement, accessibility review, name clearance, store assets |
| 29–31 | M9 UK closed test & submission | Funnel fixes, build pipeline | Closed-test management (≥ 14 days), store listings, submission |
| 32+ | M10 Canada/Australia soft launch | KPI dashboards | Scale / iterate / kill decision |

**Long-lead human items (now front-loaded):** Google Play account check (week 1), Mac + Apple Developer (week 3), store records + IAP products (week 5), external playtester pool (recruit 15–20 by week 4), privacy policy (before week 23), name clearance (M8).

### 8.3 Decision points

| When | Decision | Options | Default |
|---|---|---|---|
| Week 1 | Play testing rule applies? (AB-167) | Start tester pool now / not needed | Start if it applies |
| End of M1 (wk 2) | Physics model holds? | Continue 2.5D PhysX / 2D fallback (D-004) | Continue |
| G0 (wk 6–7) | Go / go with fixes / no-go; feel lock | — | Go with up to 1 iterate week |
| End of M4 (wk 10) | Any system still missing for `04`? | Build / redesign levels around it | Redesign levels, no new systems |
| End of M5 (wk 15) | Schedule slip > 3 weeks? | Proceed / **deep cut D1: 40 levels** | Proceed if slip ≤ 3 weeks |
| Week 18 | Art throughput sufficient? | Continue solo / contract kits / simplify art | Contract if < 60% of plan |
| Mid M6 | Daily Challenge in or out? | Build / cut (D-101 item 6) | Cut if it risks G3 |
| G-Release | Submit? | — | OWNER approval |
| After M10 read-out | Scale / iterate / kill | — | Per `07` §15 |

---

## 9. Cut list (ordered — cut from the top; each line states impact)

### 9.1 Official cut-first list (D-101 — do not build unless the VS is excellent **and** the project is genuinely ahead of schedule)

| # | Cut | Status | Impact on core loop |
|---:|---|---|---|
| 1 | Spring Plates (AB-093) | **Cut** (D-086) | None — 8 props remain (§14 ≥ 8 met) |
| 2 | Choice-loadout levels | **Cut** (D-046) | None |
| 3 | Moving ice platforms (AB-108) → static ice slides + mover shields in L53–55 | **Cut** (D-104) | None |
| 4 | Animated branded splash → static splash (AB-137) | **Cut** | None |
| 5 | More than 3 bow skins / 2 trails at launch | **Cut** — launch set: Oak Ranger, Moonwood, Royal Amethyst; Gold Spark, Leaf Swirl | None (cosmetic only) |
| 6 | Daily Challenge (AB-124) if it risks the 60-level campaign | Cut-first (P1) | None |
| 7 | Cloud save, leaderboards, achievements, push notifications, seasonal, multiplayer | **Not in MVP** | None |

### 9.2 Further cuts (if still behind)

| # | Cut | Saves | Impact |
|---:|---|---|---|
| 8 | Bullseye collection **UI** (keep data + map pip) | 1 day | None |
| 9 | Split-on-impact variant (keep the timed split only) | 1–2 days | Minor (levels designed for the timed split) |
| 10 | Set-piece cinematic framing → standard framing | 1–2 days | None |
| 11 | iPad-specific layout polish (keep generic pillarbox) | 1–2 days | None |
| 12 | Per-world music variations → one track per world | 1 day | None |
| 13 | Hit-stop/camera micro-shake variants → a single preset | < 1 day | Minimal |
| 14 | Quiver badges → only world-completion badges | 1 day | None |
| 15 | `LevelEditorWindow` → inspector-only workflow | 3 days | Slower authoring |

### 9.3 Deep cuts (require OWNER decision + decision-log entry)

| # | Deep cut | Saves | Consequence |
|---:|---|---|---|
| D1 | Ship with **40 levels (W1–W2)**; W3 as the first content update | 4–5 weeks | Fails the §14 "60 levels" definition; Bounce/portals/wind/ice slip post-launch |
| D2 | Portals post-MVP (W3 uses wind/ice/bounce only) | 1 week | Loses the signature late-MVP object; L50–52 and L60 redesigned |
| D3 | iOS launch after Android (staggered) | Mac/test overhead | Smaller launch footprint |

**Never cut:** bow draw feel and haptic/audio snap · accurate trajectory preview (incl. wind, portal exit, first bounce) · arrow collision reliability · limited quiver + stars by arrow efficiency · 1-tap restart < 1 s · settle/soft-lock rules · reliable physics + the solvability bot + device validation · readable structures (colour + silhouette + icon) · satisfying collapses · level variety · ropes · protected objects · consent/privacy compliance · crash reporting (consent-aware) · ethical ad rules · original brand-defining art/IP.

---

## 10. Post-MVP backlog (ordered by expected value; not scheduled)

| # | Item | Notes / prerequisite |
|---:|---|---|
| 1 | **Step 12 optimisation:** level balance, ad frequency, pricing, conversion | Only from soft-launch data (M10); A/B tests per `06` §15 |
| 2 | New worlds (4+) every 3–4 weeks | Only after analytics show healthy W1–W3 completion/replay (§10) |
| 3 | Deferred cosmetics: Ember and Frostglass bow skins; Cyan Streak and Ember Ash trails | First content update (D-101) |
| 4 | Spring Plates | Cut from launch (D-086) |
| 5 | Moving ice platforms | Cut from launch (D-104) |
| 6 | Choice levels | Schema reserved (D-046) |
| 7 | Animated branded splash | Static splash at launch (D-101) |
| 8 | Addressables + remote content delivery | Needed for live worlds without full app updates (D-013 revisit) |
| 9 | In-level arrow swap ("tap to swap next arrow") | Re-evaluate from playtest data (D-023) |
| 10 | Drill Arrow (pierces one timber layer) | Explicitly post-MVP in §5 |
| 11 | Hint system (soft highlight after N fails) | Must stay ethical; no paywalled solutions |
| 12 | Weekly challenge map + global leaderboard | Needs a backend; after ≥ 100 base levels (§10) |
| 13 | Seasonal cosmetic path (optional premium track) | §9 post-MVP candidate |
| 14 | Cloud save (Game Center / Play Games / UGS) | Save schema already has `installId` + `schemaVersion` |
| 15 | Achievements | Low cost after cloud identity |
| 16 | Localisation (top 5–8 languages) | `UIStrings` keys make it mechanical |
| 17 | Push notifications | Opt-in only; consent-aware |
| 18 | Unity version upgrade | Only post-launch or for a release-blocking platform issue (D-080) |
| 19 | Visual Verlet rope polish | D-009 alternative |
