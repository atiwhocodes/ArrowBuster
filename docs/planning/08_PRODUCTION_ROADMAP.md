# 08 — Production Roadmap

> Owner: Product Owner / Game Design Lead (`PO`), co-owned with Lead Integrator (`INT`) for scheduling. Status: Draft v1 — 2026-10-09.
> Milestone structure: decision **D-033** (Vertical Slice gate at M3). Ticket IDs (`AB-###`) are defined in [`09_BACKLOG.md`](09_BACKLOG.md). Names follow [`01_TECHNICAL_ARCHITECTURE.md`](01_TECHNICAL_ARCHITECTURE.md).
> Source of truth for scope: [`/mvp.md`](../../mvp.md) §14–16. Analysis and risks: [`00_MVP_ANALYSIS.md`](00_MVP_ANALYSIS.md). Decisions: [`10_DECISION_LOG.md`](10_DECISION_LOG.md).

---

## 1. Vertical slice (summary)

Full specification, per-level design and the M3 ticket list: [`11_VERTICAL_SLICE.md`](11_VERTICAL_SLICE.md).

| Element | Slice content |
|---|---|
| Environment | One polished Greenwood Range section (training-valley diorama, ruins, grass, bright daytime) |
| Core action | Bow drag/draw/release (D-018), trajectory preview (D-040), Oak Arrow only |
| Objects | Timber crates/beams, red crest target, cuttable rope (D-009), royal vase (protected), water kill zone |
| Loop | Limited quiver, win/fail/soft-lock (D-015), 1-tap restart (< 1 s), 1–3★ (`StarRules`) |
| Feedback | Basic material SFX (timber/rope/vase), break VFX, hit-stop, haptics (light/medium/success) |
| UX | One tutorial prompt (L1 ghost-hand + one "Aim for the rope" callout), clean HUD, polished Win and Fail screens |
| Data | Debug-sink analytics for the loop events (`level_started`, `arrow_fired`, `object_triggered`, `level_completed`, `level_failed`, `level_restarted`) |
| Levels | VS-01 = W1_L01 direct hit · VS-02 = W1_L04 weak support · VS-03 = W1_L07 rope drop · VS-04 = W1_L10 protected-vase precision · VS-05 = W1_L16 combined (D-050) |
| Gate question | Do 5–10 casual players, with no instructions, discover weak points, enjoy the shot-to-collapse moment, and voluntarily replay for 3★? |

---

## 2. Milestone overview

| ID | Name | Calendar (10-wk plan) | Realistic (solo + AI, §6) | Gate type | Primary owners |
|---|---|---|---|---|---|
| M0 | Project setup | done (2026-10-08) | done | — | ARCH |
| M1 | Graybox Core Feel | Week 1 | Weeks 1–2 | **Feel + physics-stability gate** | CORE, PHYS, ARCH |
| M2 | Core Loop + VS Graybox | Week 2 | Weeks 3–4 | Loop-complete gate | CORE, PROPS, LEVEL, UI, SYS, MON |
| M3 | Vertical Slice | Week 3 | Week 5 (+ playtest) | **VS go/no-go (OWNER)** | ART, UI, CORE, LEVEL, QA |
| M4 | World 1 Systems & Tooling | Week 4 | Weeks 6–7 | Tooling + W1 graybox gate | PROPS, PHYS, LEVEL, QA |
| M5 | World 1 Content & Art Lock | Week 5 | Weeks 8–10 | **Art-direction lock + W1 playtest** | ART, LEVEL, UI, SYS |
| M6 | World 2 Sunscar Canyon | Week 6 | Weeks 11–14 | W2 content gate | PROPS, CORE, LEVEL, ART |
| M7 | World 3 Frostspire Keep | Week 7 | Weeks 15–18 | W3 content gate + SDK spike | PROPS, CORE, LEVEL, ART, MON |
| M8 | Meta & Monetisation | Week 8 | Weeks 19–21 | Feature-complete gate | SYS, UI, MON, PLAT |
| M9 | Optimisation, Balance, QA, Accessibility | Week 9 | Weeks 22–24 | **Content/perf freeze** | PLAT, QA, LEVEL, UI |
| M10 | Closed Test & Submission Candidate | Week 10 | Weeks 25–26 (+ store-policy lead time) | **Release gates** | INT, PLAT, QA, PO, OWNER |

---

## 3. Milestones in detail

Each milestone lists: goal · task breakdown (tickets) · dependencies · deliverable owner · complexity · acceptance criteria · exit gate · risks.

**Formal gate names** (checklists in [`07` §Release gates](07_QA_PERFORMANCE_RELEASE.md)): M1 → **G-M1** Feel & Physics (incl. PG-1…PG-6 from `03` §1.1) · M3 → **G0** Vertical Slice (VS-G1…VS-G8 from `11` §8.1) · M5 → **G1** World 1 · M7 → **G2** Content Complete · M8 → **G3** Feature Complete · M9 → **G4** Release Candidate · M10 → **G-Release** Submission. M2, M4 and M6 have internal exit checks only.
Complexity scale: **S** ≤ 0.5 day · **M** 1–2 days · **L** 3–5 days · **XL** > 1 week (split before starting).

### M0 — Project setup ✅

Done 2026-10-08: URP, Android/iOS settings (portrait, IL2CPP/ARM64, iOS 15), folder map, 3 asmdefs, `GameConstants`, `GameEnums`, `LevelData`, `QuiverEntry`, `ProjectSetup` menu, 4 scenes in the build list, `LevelDataTests`, MCP for Unity. Known debt is carried into AB-001/AB-002/AB-016 (see `00` §13).

---

### M1 — Graybox Core Feel

**Goal:** a graybox where drawing the bow, reading the arc and knocking down a crate tower already feels good on a phone. Physics stacks are stable enough to commit to D-004.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-001 | Version control baseline (git init, LFS, UnityYAMLMerge, first commit) — *OWNER approval* | INT | S |
| AB-002 | Project hygiene: remove unused packages + template leftovers, PlayMode asmdef, `LayerSetup` + `PhysicsSetup` (D-006, D-048) | ARCH + PHYS | M |
| AB-003 | Core skeleton: `Services`, `ServiceInstaller`, `GameEvents`, `StaticReset`, `Log`, `LevelClock`, `TimeScaleController` (D-061), null/mock services | ARCH | M |
| AB-004 | Gameplay scene graybox: `GameplayRoot`, `CameraRig` + `CameraFramer` (D-041), ground, lighting, `PlayBounds` | CORE | M |
| AB-005 | `GameplayTuning` + `ArrowDefinition` + `AD_Oak` ("Spec" + "Snappy" presets, D-036) | CORE | S |
| AB-006 | `BallisticSolver` (pure) + EditMode tests incl. preview/flight parity | CORE | M |
| AB-007 | `BowInputReader` + `DrawModel`/`AimState` (D-018) + tests | CORE | M |
| AB-008 | `TrajectoryPreview` (pooled dots, impact marker D-040) | CORE | M |
| AB-009 | `ArrowProjectile` kinematic sweep + `ArrowSpawner`/`ArrowRegistry` (cap 8) | CORE | L |
| AB-010 | `MaterialProfile` + 5 graybox profiles + `MaterialBody` + `PlanarBody` | PHYS | M |
| AB-011 | `ArrowImpactResolver` rules + clamped impulses | CORE + PHYS | L |
| AB-012 | `Breakable` + `ImpactDamage` + `DebrisPool` | PHYS | M |
| AB-013 | Graybox structure prefab library v1 | PHYS | S |
| AB-014 | Physics stability spike + crate-tower sandbox + M1 feel-gate playtest | PHYS + QA + PO | M |
| AB-015 | `DevOverlay` (AB_DEV) + cheats | CORE | S |
| AB-047 | `BuildScript` v1 + Android dev APK (needed for the on-device feel gate) | PLAT | M |

- **Dependencies:** AB-001 → everything (branching). AB-002 → AB-010/AB-012 (layers). AB-003 → AB-004/AB-009/AB-015. AB-005 + AB-006 → AB-008/AB-009. AB-009 + AB-010 → AB-011 → AB-012 → AB-014. AB-047 → AB-014 (device test).
- **Deliverable owner:** CORE (bow/arrow), PHYS (stability), INT (integration on `main`).
- **Milestone complexity:** L (≈ 16 tickets, 2 on the critical path at L).
- **Acceptance criteria (measurable):**
  1. Preview vs. actual flight deviation ≤ 1 mm over 3 s (EditMode parity test green).
  2. No arrow tunnelling: 1,000 automated shots at 0.1 m-thick planks and 0.06 m rope colliders across the full power range → 100% hit registration (PlayMode test).
  3. Physics spike gates **PG-1…PG-6** in `03` §1.1 all pass (idle drift < 1 cm, spawn safety, collapse calm < 3 s in 9/10 runs, repeatability ≥ 19/20, worst-frame physics ≤ 5 ms on Mid, zero Z drift) — recorded in `docs/qa/test-runs/`.
  4. Gate **G-M1** checklist in `07` §16 is green.
  5. Android dev APK runs ≥ 58 FPS median on a Mid-tier device in the sandbox. 0 B/frame GC in gameplay (Profiler).
  6. Zero Console errors/warnings from project code. All EditMode tests green.
- **Exit gate — G-M1 (PO + OWNER):** ≥ 5 testers try both "Spec" and "Snappy" presets on device. Pick one (D-036 → Accepted) and confirm or adjust the draw mapping (D-018) and preview rule (D-040). **If physics gate PG-1 or PG-2 (`03` §1) fails after 2 days of tuning (incl. TGS solver A/B), trigger the D-004 fallback review** (2D physics) before M2 starts.
- **Risks & mitigations:** T-01 stacking instability → spike first, mass ratios ≤ 10:1, start asleep. T-02/T-03 → shared solver + sweep. "Floaty" feel → two presets compared on device, not in the editor.

---

### M2 — Core Loop + VS Graybox

**Goal:** a complete, data-driven level loop: load level → shoot → objectives/protected/rope/kill zones → win/fail/soft-lock → stars → save → next/retry. Contains 10 graybox W1 levels including the 5 VS levels.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-016 | `LevelData` schema v2 + `LevelLayout` + `LevelLoader` (restart < 300 ms) | LEVEL + CORE | L |
| AB-017 | `QuiverModel` + `StarRules` (D-024) + tests | CORE | S |
| AB-018 | `Objective` + `ObjectiveClearRule` + `ObjectiveTracker` (CrestTarget, SupplyCrate) + `ProtectedObject`/`ProtectedTracker` (RoyalVase) | PROPS | L |
| AB-019 | `SettleMonitor` + `GameplayController` state machine (D-015, D-016) + tests | CORE + PHYS | L |
| AB-020 | `RopeCuttable` + `RopeView` (D-009) | PROPS | M |
| AB-021 | `KillZone` (Water/Spikes/Pit) + clear line (D-039) | PROPS | S |
| AB-022 | `JsonSaveService` v1 + `SaveMigrator` scaffold + `ProgressionService.RecordResult` + tests | SYS | M |
| AB-023 | Graybox HUD + Win/Fail/Pause panels + `SafeAreaFitter` | UI | M |
| AB-024 | `IAnalyticsService` + `DebugAnalyticsService` + loop events | MON | S |
| AB-025 | VS levels VS-01..05 graybox + `ShotRecorder` + intended shots + solvability bot v1 | LEVEL + QA | L |
| AB-046 | Five additional graybox W1 levels (W1_L02, L03, L05, L06, L08) → 10 graybox levels (M2 systems only; dummy/straw/lantern stand-ins upgraded in AB-064) | LEVEL | M |

- **Dependencies:** M1 gate passed (flight tuning fixed before authoring geometry, D-036). AB-016 → AB-025/AB-046. AB-018 + AB-020 + AB-021 → AB-019 → AB-023. AB-017 → AB-019/AB-022. AB-003 → AB-024.
- **Deliverable owner:** CORE (loop), LEVEL (content), INT.
- **Milestone complexity:** L.
- **Acceptance criteria:**
  1. All 10 levels load from `LevelData` only. No level-specific code. `LevelLoader` restart p95 < 300 ms in the editor on the dev PC and < 1 s on the Mid device.
  2. Win requires all required objectives cleared + 0.75 s calm (cap 3 s). Protected loss → immediate fail with a reason. Out-of-arrows fail after 2 s calm (cap 5 s). Soft-lock toast appears. All are covered by EditMode state-machine tests with a fake clock.
  3. Stars computed by `StarRules`; best stars persisted across app restarts (`save.json` verified).
  4. Solvability bot v1: each of the 10 levels' recorded intended shots wins in the editor 10/10 runs.
  5. Debug analytics CSV contains the 6 loop events with the §13 params for a full play of all 10 levels.
  6. A 1-tap Retry from any state, including mid-flight and mid-collapse.
- **Exit gate (PO + INT):** an internal play-through of 10 levels on device with no blockers. The VS level designs (`11`) are approved for art.
- **Risks & mitigations:** settle edge cases (T-05) → `SettleMonitor` ignores kinematic/ambient bodies, hard caps. Scope creep in HUD → graybox only, polish is M3.

---

### M3 — Vertical Slice

**Goal:** the 5 VS levels at "polished slice" quality in one Greenwood section. They prove the fun to outside players.

- **Tasks:** AB-026 … AB-045 — defined in [`11_VERTICAL_SLICE.md`](11_VERTICAL_SLICE.md) §4.2 — plus AB-158 (`AssetImportRules` + placeholder build check, D-055/D-056). The scope covers the Greenwood section art v1, bow hero v1, crate/target/vase/rope art v1, `FeedbackDirector`, `AudioService`, `VfxService`, `HitStop`, `HapticsService`, the tutorial prompt system, `UiTween`, polished HUD + Win/Fail, the `LC_VerticalSlice` playlist, an Android build and the playtest.
- **Dependencies:** M2 exit. Art direction exploration may start in M2 (ART, non-blocking).
- **Deliverable owner:** ART (look and feel), UI (screens), QA (playtest), PO (gate).
- **Milestone complexity:** L–XL (art-bound).
- **Acceptance criteria:**
  1. All 5 VS levels playable in order from the playlist on Android (and on iOS if a Mac is available) at ≥ 58 FPS median on Mid.
  2. Feedback present on every meaningful impact: SFX per material, break VFX, hit-stop 40–70 ms, haptics (draw threshold light, impact medium, clear success). All toggleable in a minimal settings stub.
  3. The VS acceptance criteria A-1…A-12 in `11` §7 are met before the playtest.
  4. Playtest with 5–10 casual players, no instructions, scored against **VS-G1…VS-G8** in `11` §8.1 (canonical thresholds — e.g. first shot ≤ 15 s for ≥ 90% of testers, ≥ 80% complete all 5 levels with ≤ 1 assist, ≥ 50% voluntary replay).
  5. A 5-second screenshot test: ≥ 4/5 testers name the objective and one weak point per level.
  6. Restart < 1 s on device. Zero crashes in the playtest session.
- **Exit gate — G0 (OWNER go/no-go):** decision rules in [`11` §8.1](11_VERTICAL_SLICE.md#81-go--no-go-thresholds) (VS-G1…VS-G8): **GO** → art direction is provisionally locked and production starts. **GO with fixes** → fix tickets added to M4. **NO-GO** → one iteration week on feel/readability, then retest with new players; a second NO-GO escalates to OWNER before any content scaling (do not start M4).
- **Risks & mitigations:** art bottleneck (S-02) → a low-poly gradient-atlas style, one section only. Playtester recruitment → book testers during M2.

---

### M4 — World 1 Systems & Tooling

**Goal:** every W1 mechanic exists, levels can be produced without code, and all 20 W1 levels are playable in graybox with recorded intended solutions.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-048 | Material profiles pass: Straw, Stone final; Ice/Metal "ready" graybox | PHYS | M |
| AB-049 | Heavyhead arrow (`AD_Heavyhead`) | CORE | M |
| AB-050 | `ExplosionSolver` (pure) + `Explosion` + tests (D-020) | PHYS | M |
| AB-051 | `PowderBarrel` prop + chain delay | PROPS | M |
| AB-052 | HangingLantern objective | PROPS | S |
| AB-053 | CursedOrb objective (direct-hit only) | PROPS | S |
| AB-054 | TrainingDummy objective (knock-over/ground rule) | PROPS | M |
| AB-055 | BannerRope objective + chain visual variant | PROPS | S |
| AB-056 | SleepingFox protected (displacement/tilt, D-038) | PROPS | M |
| AB-057 | `BullseyeMarker` + award on win (D-045) | PROPS + SYS | S |
| AB-058 | Prefab library v2 + `PrefabValidator` | PHYS + PROPS | M |
| AB-059 | `LevelValidator` v1 + EditMode tests | LEVEL | L |
| AB-060 | `LevelEditorWindow` + `LevelDataInspector` | LEVEL | L |
| AB-061 | `WorldData`/`LevelCatalog` + `LevelCatalogBuilder` | LEVEL | M |
| AB-062 | Solvability bot v2 (jitter tolerance) + idle stability over the catalog | QA | M |
| AB-063 | Interaction-matrix tests — W1 cells | QA | M |
| AB-064 | W1 graybox batch A: L09, L11, L12 + upgrade AB-046 stand-ins (L03/L06/L08) | LEVEL | M |
| AB-065 | W1 graybox batch B: L13–L15 (Heavyhead) | LEVEL | L |
| AB-066 | W1 graybox batch C: L17–L20 (incl. L20 boss) | LEVEL | L |
| AB-067 | Arrow reveal card (first appearance) | UI | S |
| AB-068 | CI: GitHub Actions + GameCI test runs | INT + ARCH | M |
| AB-159 | `IntendedSolutionRunner` (on-device replay, solvability layer 2) | QA + CORE | S |
| AB-160 | `MP_Earth` ground profile + `Ground` tag (D-059) | PHYS | S |

- **Dependencies:** M3 GO. AB-050 → AB-051 → AB-066 (L20 boss). AB-059/AB-061 → AB-062 → batches. AB-049 → AB-065.
- **Acceptance criteria:**
  1. W1 L1–20 all present in `LC_World1` with `status ≥ Graybox`. Each has ≥ 1 recorded intended solution and a `docs/levels/W1_Lxx.md` stub.
  2. `LevelValidator`: 0 errors across W1. The bot passes each level on the `07` §5.1 jitter grid (D-074; all 9 samples win, ≥ 7/9 within par — the §7 aim-window rule).
  3. Idle stability: all W1 levels drift < 1 cm in 3 s with no shots fired.
  4. A new level can be created by a non-programmer flow (editor window → prefab → LevelData → validate → bot) in ≤ 30 min for a simple layout (timed dry-run by LEVEL).
  5. Every W1 object type has the two-level tutorial cadence (§4), verified against the `04` mechanic plan.
- **Exit gate (PO + LEVEL):** an internal W1 graybox play-through. Difficulty curve reviewed against `04`.
- **Risks:** tooling overreach → the validator and bot are P0, the editor window is P1 (inspector-only fallback). Explosion chain blow-ups (T-06) → clamps + matrix tests.

---

### M5 — World 1 Content & Art Lock

**Goal:** World 1 complete at final quality. Art direction locked. Front-end flow (Home, World Map W1, progression, settings) usable. First-session targets met.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-069 | Art direction lock: style frames, palette, kit rules — *OWNER sign-off* | ART | M |
| AB-070 | Greenwood environment kit (full, ~25 pieces) | ART | XL → split into 3 L |
| AB-071 | W1 object art: crate, beam, target, lantern, orb, dummy, vase, fox, barrel, rope/chain | ART | L |
| AB-072 | Bow hero model final + string glow | ART | M |
| AB-073 | W1 SFX set (timber, straw, stone, rope, barrel, vase, fox) | ART | M |
| AB-074 | W1 VFX set (breaks per material, blast, rope snap) | ART | M |
| AB-075 | `MusicPlayer` + Greenwood music | ART | M |
| AB-076 | World Map v1 (W1 nodes, stars, locked teaser) | UI | L |
| AB-077 | Home screen v1 | UI | M |
| AB-078 | `ProgressionService` full (unlock 15/20, best records, Bullseye) + tests | SYS | M |
| AB-079 | First-session flow (Boot → L1 directly on first launch; < 15 s to the first shot) | UI + SYS | M |
| AB-080 | Settings panel v1 (music, SFX, haptics, reduced particles, colour-assist, reduced motion) | UI | M |
| AB-081 | Pause panel, level intro card, fail-reason copy | UI | S |
| AB-082 | W1 art + balance pass L1–20 | LEVEL | L |
| AB-083 | W1 intended-solution docs complete | LEVEL | S |
| AB-084 | W1 playtest round (8–10 players) + report | QA + PO | M |
| AB-161 | `ChainReactionTracker` + escalating percussion + success sting | ART + CORE | S |
| AB-162 | **OWNER:** Apple/Google developer accounts + Play closed-testing rule check (D-072, D-079) | OWNER + PLAT | S |

- **Dependencies:** AB-069 (lock) → AB-070/AB-071 → AB-082. AB-078 → AB-076.
- **Acceptance criteria:**
  1. W1 20/20 levels at `status = Final`. Validator 0 errors. Bot green on device (Android Mid + Low).
  2. Cold start → first shot ≤ 15 s on Mid (median of 5 fresh installs).
  3. W1 playtest: ≥ 80% of testers finish W1 unaided (§15 target); ≥ 1 chain reaction by L3 and ≥ 1 three-star by L4 for ≥ 70% of testers (§10).
  4. Mid tier ≥ 58 FPS median, Low ≥ 29 FPS median on every W1 level. Restart < 1 s.
  5. The art direction document is signed by OWNER (D-042 originality checklist attached).
- **Exit gate — G1 World 1 (OWNER + PO; checklist `07` §16):** World 1 is "shippable quality". Art pipeline throughput is measured (pieces/day) → re-plan M6/M7 art if under target.
- **Risks:** art throughput (S-02) → measure and contract out if < 60% of plan. W1 too hard/easy → remote-tunable quiver size is not used for this; fix in data before M6.

---

### M6 — World 2 Sunscar Canyon

**Goal:** Sunscar mechanics (balloons, oil + fire, Fire arrow, Split arrow, moving shields, boulder lanes, counterweights) and L21–40 at final quality.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-085 | `Balloon` (buoyancy, pop, wind-responsive hook) | PROPS | M |
| AB-086 | `Burnable` + `FireZone` (deterministic spread timers) | PROPS | M |
| AB-087 | `OilJar` (break → `FireZone`) | PROPS | S |
| AB-088 | Fire arrow (`FireArrowBehaviour`, `AD_Fire`) | CORE | M |
| AB-089 | Split arrow (`SplitArrowBehaviour`, child arcs in the preview, D-019) | CORE | L |
| AB-090 | `KinematicMover` (rotate/ping-pong/loop on `LevelClock`) + moving shield prefabs | PROPS | M |
| AB-091 | Rolling boulder prefabs + boulder lanes | PROPS | S |
| AB-092 | Lever / counterweight prefabs (D-022) | PHYS | S |
| AB-093 | `SpringPlate` (P2, D-021 — only if on schedule) | PROPS | M |
| AB-094 | Metal ricochet tuning (shields) | PHYS | S |
| AB-095 | Interaction-matrix tests — W2 cells | QA | M |
| AB-096 | Sunscar environment kit | ART | XL → split |
| AB-097 | W2 object art (balloons, oil jar, shield, boulder, lever) | ART | L |
| AB-098 | W2 SFX/VFX (fire, burn, pop, metal ping, boulder roll) | ART | M |
| AB-099 | Sunscar music | ART | S |
| AB-100 | W2 graybox L21–30 | LEVEL | L |
| AB-101 | W2 graybox L31–40 (incl. L40 boss) | LEVEL | L |
| AB-102 | W2 art + balance pass | LEVEL | L |
| AB-103 | W2 playtest round | QA | M |

- **Acceptance criteria:** L21–40 at Final, validator 0 errors, bot green with jitter. Fire/balloon/shield matrix cells all have passing PlayMode tests. The Split preview shows the split marker + 3 child arcs and matches flight (parity test). Shields are deterministic across restarts (same pose at the same `LevelClock` time). Perf budgets hold on L25/L30/L35/L40 set pieces (Low ≥ 29 FPS).
- **Exit gate:** W2 internal play-through + playtest. The decision point for SpringPlate (build or cut) is at mid-M6.
- **Risks:** fire spread non-determinism → timer-based, no physics triggers (overlap ticks). Split clutter → children cap 3, ≤ 0.8 s life.

---

### M7 — World 3 Frostspire Keep (+ SDK spike)

**Goal:** ice, wind, Bounce arrow, portals and moving ice platforms. L41–60 at final quality, including the L60 finale. The vendor SDK integration spike is de-risked on a branch.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-104 | Ice behaviour (low friction, brittle shatter, harmless fragments) | PHYS | M |
| AB-105 | `WindField` (arrow acceleration in `BallisticSolver` + balloon force; preview, D-017) | PROPS + CORE | M |
| AB-106 | Bounce arrow (`BounceArrowBehaviour`; preview ricochet) | CORE | M |
| AB-107 | `PortalRing`/`PortalPair` (arrow teleport, preview pass-through) | PROPS + CORE | L |
| AB-108 | Moving ice platforms via `KinematicMover` (P2 — static fallback) | PROPS | S |
| AB-109 | Interaction-matrix tests — W3 cells | QA | M |
| AB-110 | Frostspire environment kit | ART | XL → split |
| AB-111 | W3 object art (ice blocks, fans, portals, metal plates, royal relic) | ART | L |
| AB-112 | W3 SFX/VFX (ice chime/shatter, wind, portal) | ART | M |
| AB-113 | Frostspire music | ART | S |
| AB-114 | W3 graybox L41–50 | LEVEL | L |
| AB-115 | W3 graybox L51–60 (incl. L60 finale) | LEVEL | L |
| AB-116 | W3 art + balance pass | LEVEL | L |
| AB-117 | SDK integration spike branch (mediation + UMP + Firebase + Unity IAP compile; test ads on Android) | MON + PLAT | L |
| AB-118 | W3 playtest round | QA | M |

- **Acceptance criteria:** L41–60 at Final. Portal and Bounce previews match flight (parity tests). The wind preview matches flight in the parity test. L60 is solvable by the bot with jitter. The spike branch produces an Android build showing test rewarded + interstitial ads, the UMP form and the IAP sandbox catalogue, without touching `ArrowBuster.Runtime`.
- **Exit gate — G2 Content Complete (`07` §16):** content complete (60/60 Final). The SDK vendor recommendation goes to OWNER for the M8 decision (D-035).
- **Risks:** portals/bounce make solutions brittle → aim windows enforced by the bot. The SDK spike fails → fall back to the alternative vendor (MAX ↔ LevelPlay) within M8 week 1.

---

### M8 — Meta & Monetisation

**Goal:** feature complete. World map with 3 worlds, economy, cosmetics, daily challenge, Bullseye collection, ethical ads, IAP, consent, analytics vendor, crash reporting, remote config.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-119 | World Map final (3 worlds, unlock 15/20, `world_unlocked`) | UI + SYS | L |
| AB-120 | `EconomyService` + `EconomyConfig` (D-026) + tests | SYS | M |
| AB-121 | `CosmeticCatalog`/`CosmeticDefinition` + `InventoryService` + tests | SYS | M |
| AB-122 | Bow Forge screen (collection grid, equip, buy with coins) | UI | L |
| AB-123 | Cosmetic assets: 5 bow skins, 4 trails, quiver badges | ART | L |
| AB-124 | `DailyChallengeService` + UI (D-027) | SYS + UI | M |
| AB-125 | Bullseye collection view | UI | S |
| AB-126 | Save schema full + migrations + tests | SYS | M |
| AB-127 | `AdPolicy` (D-025) + exhaustive tests | MON | M |
| AB-128 | Ads vendor adapter (rewarded, interstitial) in `ArrowBuster.Integrations` | MON | L |
| AB-129 | Rewarded +1 arrow flow (Fail panel; D-024) | UI + CORE | M |
| AB-130 | Rewarded 2× coins flow (Win panel) | UI + SYS | S |
| AB-131 | Unity IAP: `remove_ads`, `starter_pack`, restore | MON | L |
| AB-132 | Consent: UMP + ATT + `ConsentPanel` + privacy settings entry | MON + UI | L |
| AB-133 | Analytics vendor adapter + full event dictionary | MON | M |
| AB-134 | Crash reporter adapter + IL2CPP symbol upload | PLAT + MON | M |
| AB-135 | Remote config adapter + whitelisted keys | MON | M |
| AB-136 | Shop/Home offers: starter pack + Remove Ads UI | UI | M |
| AB-137 | Splash/loading screen (branded bow draw, static fallback) | ART + UI | S |
| AB-163 | Privacy policy hosted + `app.privacy_policy_url`; crash-reporting legal basis (Q-17/Q-18) | OWNER + MON | S |
| AB-164 | `ModalDialog` + `CreditsPanel` + Settings links (Privacy, Restore, Credits) | UI | S |
| AB-165 | iOS build path: `iOSPostProcess` (ATT string, privacy manifests) + first iOS device build with SDKs | PLAT | M |
| AB-166 | Early Google Play closed track if D-079 applies | PLAT + OWNER | S |

- **Acceptance criteria:**
  1. `AdPolicy` unit tests cover every rule in D-025 and §9 (no interstitial < 10 min lifetime, never after fail, ≥ 3 levels between, Remove Ads honoured, rewarded always opt-in, max one +1-arrow offer per attempt).
  2. Purchase, restore and Remove Ads work in the sandbox on both stores. Purchases survive reinstall via restore.
  3. Consent: no analytics/ads SDK call before consent resolves (verified by logging proxy). ATT shown after UMP on iOS. Declining still yields a fully playable game.
  4. All events in the `06` dictionary appear in the vendor debug view with correct params.
  5. A crash test build reports a symbolicated crash on both platforms.
  6. Offline launch: game fully playable, ads gracefully unavailable, no blocking spinners > 2 s.
- **Exit gate — G3 Feature Complete (`07` §16):** **feature complete** — no new features after this point (bug fixes and balance only).
- **Risks:** T-09 SDK build breakage → spike done in M7, pinned EDM4U, Mac build check weekly. Store product setup lead time → OWNER creates store accounts by M5 and IAP products in the consoles before the M7 spike (D-072).

---

### M9 — Optimisation, Balance, QA, Accessibility

**Goal:** meet perf budgets on the device matrix, balance all 60 levels, finish accessibility, run full regression.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-138 | `QualityTierSelector` + per-tier URP assets + runtime guard | PLAT | M |
| AB-139 | Device profiling pass across the matrix + fixes | PLAT | L |
| AB-140 | Memory + download-size pass (texture compression, audio import) | PLAT + ART | M |
| AB-141 | Physics perf pass on set pieces (L5, L10 … L60) | PHYS | M |
| AB-142 | Balance pass all 60 (par, quiver, preview scale) | LEVEL + PO | L |
| AB-143 | Accessibility: colour-assist outlines, reduced particles/motion, icon redundancy | UI + ART | M |
| AB-144 | Full regression + on-device solvability (all 60) | QA | L |
| AB-145 | Unity LTS upgrade spike (D-043) | ARCH | S |
| AB-146 | Haptics tuning on the device matrix | PLAT | S |
| AB-147 | Localisation-readiness audit (`UIStrings`, no hard-coded text) | UI | S |
| AB-148 | Analytics QA: event verification vs. the dictionary | QA + MON | M |
| AB-149 | App lifecycle: pause/resume, interruptions, low memory, ad interruption recovery | PLAT | M |

- **Acceptance criteria:** budgets in `01` §11 met on all matrix devices (Mid 60 FPS ≥ 95% of frames on the 10 heaviest levels; Low 30 FPS ≥ 95%). Resident memory ≤ 450 MB on Low. Download ≤ 150 MB. All 60 levels bot-green on a Mid Android + an iPhone 11-class device. 0 open P0/P1 bugs. Accessibility checklist in `07` passed.
- **Exit gate — G4 Release Candidate (`07` §16):** **content and perf freeze**. Only release-blocking fixes after this.

---

### M10 — Closed Test & Submission Candidate

**Goal:** run a closed test, fix funnel issues, produce store assets and a submission-candidate build that passes release gates.

| Ticket | Title | Owner | Size |
|---|---|---|---|
| AB-150 | Store listing assets (icon, screenshots, preview video) — original, reviewed against the reference | ART + PO | L |
| AB-151 | Privacy policy, App Privacy labels, Play Data safety | MON + OWNER | M |
| AB-152 | Release build pipeline: signing, AAB, Xcode archive, versioning | PLAT | M |
| AB-153 | Closed test distribution (TestFlight + Play closed track) | PLAT + QA | M |
| AB-154 | Closed-test funnel review + fixes | MON + PO | L |
| AB-155 | Store compliance + age-rating questionnaires | PLAT + PO | S |
| AB-156 | Trademark/store-name search (Q-02) | PO + OWNER | S |
| AB-157 | Release-gate sign-off | INT + QA + PO | S |

- **Acceptance criteria:** closed test per `07` §14: ≥ 20 external testers for ≥ 14 days (or the Play-required tester count/duration if longer — verified at M5 via AB-162; if D-079 applies, the Play closed track already started in M8 via AB-166). Success metrics in `07` §14 met (incl. ≥ 80% of W1 starters complete W1 unaided, median retries 1–3, fairness ≥ 4.0/5). Level 1 completion ≥ 95%. Crash-free sessions ≥ 99.5%. No forced ad in the first 10 minutes (telemetry-verified). Release gates in `07` all green.
- **Exit gate — G-Release (`07` §16):** OWNER approves submission.

---

## 4. Sprint / phase plan

**Cadence:** 1-week sprints. In the realistic plan, one milestone spans 1–4 sprints.

| Day | Ritual | Who |
|---|---|---|
| Mon | Sprint planning: pick tickets meeting Definition of Ready, assign roles, claim file locks (`docs/agents/FILE_LOCKS.md`) | PO + INT (+ OWNER) |
| Mon–Thu | Agent tasks in branches/worktrees; one editor-bound task per Unity instance (max 2) | Specialist agents |
| Daily | INT merges green branches to `main` using the integration checklist | INT |
| Thu | Device build (Android every sprint; iOS from M3 when a Mac is available) | PLAT |
| Fri | Playtest/feel review, bot run over the catalog, sprint review, decision-log updates | PO, QA, OWNER |

**Phases (grouping milestones):**

| Phase | Milestones | Theme | Output |
|---|---|---|---|
| A — Prove | M1–M3 | Feel, loop, vertical slice | Go/no-go on the fun |
| B — Pipeline | M4–M5 | Tooling + World 1 final | Repeatable content machine, art lock |
| C — Scale | M6–M7 | Worlds 2–3 | 60 levels final |
| D — Business | M8 | Meta + monetisation | Feature complete |
| E — Ship | M9–M10 | Perf, balance, QA, store | Submission candidate |

---

## 5. Dependency graph

```
AB-001 ─► AB-002 ─┬─► AB-010 ─► AB-011 ─► AB-012 ─► AB-013 ─► AB-014 (M1 GATE)
                  │                 ▲
AB-001 ─► AB-003 ─┼─► AB-004 ───────┤
                  │                 │
          AB-005 ─┴─► AB-006 ─► AB-008
                       │
                       └──► AB-009 ─┘          AB-047 (Android build) ─► AB-014
                       AB-007 ─► AB-009
                       AB-015 (DevOverlay) ◄─ AB-003

M1 GATE ─► AB-016 ─┬─► AB-025 (VS levels) ─► AB-046 (5 more levels)
          AB-017 ──┤
          AB-018 ──┼─► AB-019 (GameplayController) ─► AB-023 (HUD/panels)
          AB-020 ──┤                          └────► AB-022 (save)
          AB-021 ──┘   AB-024 (analytics) ◄─ AB-003
                                     ▼
                         M2 GATE ─► AB-026..045 (VS polish) ─► M3 VS GATE
                                                                   ▼
        AB-048..AB-068 (W1 systems + tooling) ─► M4 ─► AB-069 art lock ─► M5 (W1 final)
                                                                   ▼
                  M6 (W2: fire/split/balloon/shield) ─► M7 (W3: ice/wind/bounce/portal) + AB-117 SDK spike
                                                                   ▼
                       M8 (meta/monetisation) ─► M9 (perf/balance/QA) ─► M10 (closed test, submit)
```

Cross-milestone hard dependencies:
- `BallisticSolver` environment hooks (wind/portal, D-017) are designed in AB-006 but filled in AB-105/AB-107.
- `LevelValidator` (AB-059) must exist before mass content (AB-064+).
- `AdPolicy` (AB-127) needs `ProgressionService` playtime/level counters (AB-078).
- The store developer accounts (OWNER) must exist by M7 for product setup and by M8 for sandbox IAP.

---

## 6. Critical path

```
[AB-001 repo] → [AB-002 layers/physics] → [AB-006 solver] → [AB-009 arrow] → [AB-011 impact] → [AB-012 breakable]
   → [AB-014 PHYSICS + FEEL GATE] → [AB-016 level schema/loader] → [AB-018 objectives] → [AB-019 game loop]
   → [AB-025 VS levels] → [M3 art + feedback polish] → [VS GO/NO-GO]
   → [AB-059 validator + AB-062 bot] → [W1 content batches] → [AB-069 ART LOCK] → [AB-070 Greenwood kit] → [W1 final]
   → [W2 kit + L21–40] → [W3 kit + L41–60] → [M8 SDK integration] → [M9 device perf] → [M10 closed test ≥ 7–14 days] → SUBMIT
```

**Critical-path owners:** CORE (M1–M2), ART (M3, M5–M7 environment kits = the longest chain), LEVEL (content batches), MON/PLAT (M8 SDKs), OWNER (gates, store accounts, closed-test duration).
**Near-critical:** UI front-end (World Map, Bow Forge) — can slip ≤ 1 week without moving the critical path if started in M5.

---

## 7. 10-week MVP plan (aligned to mvp §14)

| Week | mvp §14 deliverable | This roadmap (D-033) | Deviation and why |
|---:|---|---|---|
| 1 | Graybox bow input, ballistic arrow, camera, one breakable tower | **M1** AB-001…AB-015, AB-047 | Adds repo hygiene, the stability spike and an on-device feel gate |
| 2 | Objective/fail/win loop, restart, 10 graybox levels | **M2** AB-016…AB-025, AB-046 | Ropes, protected vase, kill zones and the level data format move here from week 3 (needed by the VS) |
| 3 | Materials, ropes, targets, protected objects, level data format | **M3 Vertical Slice** AB-026…AB-045 | **Major deviation:** one-section art + feedback + polished screens are pulled forward from week 5 to prove fun before scaling content |
| 4 | Heavyhead, balloons, powder barrels; first 20 levels | **M4** W1 systems + tooling; W1 1–20 graybox | Balloons move to W2 (their world per §6). Materials pass happens here. |
| 5 | Art direction lock, Greenwood final art, VFX/SFX/haptics | **M5** art lock, W1 final, Home/Map v1 | Basic feedback already exists from M3 |
| 6 | Fire, split arrow, Sunscar content to 40 | **M6** | Same |
| 7 | Ice, bounce, portal, Frostspire content to 60 | **M7** + SDK spike | Adds a de-risking spike for M8 |
| 8 | World map, cosmetics, economy, ads/IAP, analytics | **M8** | The analytics *interface* has existed since M2 |
| 9 | Device optimisation, balance, QA, accessibility, consent | **M9** | Consent moves to M8 (it must gate SDK init) |
| 10 | Closed test, funnel fixes, store assets, submission candidate | **M10** | Store-policy lead times flagged (closed-test duration) |

**Honest assessment:** the 10-week plan is achievable only with a dedicated artist (or contracted kits), a second engineer, or heavy scope cuts. Per world, about 40% of effort is art. For one developer + AI agents, use §8.

---

## 8. Realistic contingency plan — 1 developer + AI agents

### 8.1 Capacity assumptions

| Assumption | Value |
|---|---|
| Developer focused hours | ~30 h/week (rest: admin, reviews, playtests, store setup) |
| AI-agent leverage | ~2–3× on well-specified code/tool/test tickets; ~1.2× on level graybox (agents build via MCP, human judges fun); **~1× on art direction, feel tuning, device testing, playtests, store/legal** |
| Concurrent Unity instances | ≤ 2 (D-034) |
| Art | Either (a) the developer produces low-poly gradient-atlas art with agent help for kit assembly/import, or (b) a contracted 3D artist for environment kits from M5. **Plan below assumes (a) with optional (b) from M5.** |
| Level throughput (after M4 tooling) | Graybox: 3–4 levels/day. Final art + balance: 2 levels/day. |
| Buffer | 2 weeks unallocated (≈ 8%) + 1 iterate week reserved at the VS gate |

### 8.2 Schedule (≈ 24 weeks + 2 buffer = 26; 22 if no buffer is used)

| Weeks | Phase / milestone | Agent-parallelisable work | Human-bound work |
|---|---|---|---|
| 1–2 | M1 Graybox Core Feel | Solver, input model, tests, pooling, layer tools, DevOverlay (CORE/PHYS/ARCH agents) | Feel tuning on device, stability judgement, AB-001 approval |
| 3–4 | M2 Core Loop + VS graybox | Objectives, rope, kill zones, save, HUD graybox, analytics sink, validator scaffolding | Level design of VS-01..05 (fun), approving designs |
| 5 (+1 iterate) | M3 Vertical Slice | Feedback wiring, UiTween, tutorial system, build scripts | **Art direction**, the slice art, SFX selection, **playtest with 5–10 people**, go/no-go |
| 6–7 | M4 W1 systems + tooling | All W1 props/objectives, validator, editor window, catalog, bot v2, CI | W1 level design review |
| 8–10 | M5 W1 final + art lock | Kit import/prefab variants, map/home UI, progression, settings | **Art lock**, Greenwood kit modelling, W1 playtest, balance, **create Apple/Google developer accounts** (D-072) and verify the Play closed-testing rule (D-079) |
| 11–14 | M6 World 2 | Balloon/fire/split/shield systems + matrix tests; L21–40 graybox via MCP | Sunscar art, fun review, W2 playtest |
| 15–18 | M7 World 3 + SDK spike | Ice/wind/bounce/portal systems; L41–60 graybox; SDK spike branch | Frostspire art, W3 playtest, **IAP products ready for the SDK spike** (accounts created by wk 10, D-072) |
| 19–21 | M8 Meta & monetisation | Economy, cosmetics, daily, AdPolicy, adapters, save migrations | Vendor choice (OWNER), cosmetic art, privacy-policy text, consent review, **start the Play closed track if D-079 applies** |
| 22–24 | M9 Opt/balance/QA/a11y | Profiling scripts, regression runs, bot over catalog, analytics QA | **Device-matrix testing**, balance judgement, accessibility review |
| 25–26 | M10 Closed test + submit | Fix funnel issues, build pipeline | Store assets, closed-test management, **store-policy waiting periods**, submission |

**Long-lead human items to start early:**
- Apple Developer Program enrolment (start by week 10; it can take days to weeks).
- Google Play developer account + identity verification (start by week 10). If it is a **new personal account**, Google has required a closed test with a minimum tester count over 14 days before production access — **verify the current rule**. Start the closed track by week 22.
- Mac access for iOS builds (from M3 ideally; mandatory by M8).
- Playtester pool (recruit 15–20 casual players by week 4).

### 8.3 Decision points

| When | Decision | Options | Default |
|---|---|---|---|
| End of M1 (wk 2) | Physics model holds? | Continue 2.5D PhysX / switch to 2D fallback (D-004) | Continue |
| VS gate (wk 5–6) | Go / iterate / no-go | — | Go with up to 1 iterate week |
| End of M5 (wk 10) | Art throughput sufficient? | Continue solo / contract environment kits / simplify art (flat-colour) | Contract if < 60% of plan |
| Mid M6 (wk 12) | SpringPlate build or cut | — | Cut unless ahead |
| End of M6 (wk 14) | Schedule slip > 2 weeks? | Proceed with W3 / **deep cut: ship closed test with 40 levels (W1–W2)**, W3 as first update | Proceed if slip ≤ 2 weeks |
| End of M7 (wk 18) | Vendor selection | MAX vs LevelPlay; Firebase vs UGS | Per the `06` criteria |
| M9 start (wk 22) | Unity LTS upgrade | Adopt / stay | Adopt only if all gates green |

---

## 9. Cut list (ordered — cut from the top; each line states impact)

| # | Cut | Saves | Impact on core loop |
|---:|---|---|---|
| 1 | Choice levels (L31+) — **already cut** (D-046) | 1 week | None |
| 2 | `SpringPlate` prop (D-021) | 2–3 days | None (8 props remain) |
| 3 | Moving ice platforms → static ice slides (L53–55) | 1–2 days | None |
| 4 | Cosmetics count: 5 → 3 bow skins, 4 → 2 trails | 3–5 art days | None (cosmetic only) |
| 5 | Animated branded splash → static logo | 1–2 days | None |
| 6 | Bullseye collection **UI** (keep data + map pip) | 1 day | None |
| 7 | Split-on-impact variant (keep the timed split only, D-019) | 1–2 days | Minor (levels designed for the timed split) |
| 8 | Set-piece cinematic framing → standard framing per level | 1–2 days | None |
| 9 | iPad-specific layout polish (keep generic pillarbox) | 1–2 days | None |
| 10 | Daily challenge simplification: 1 level/day instead of 3, no special arrows | 1–2 days | None |
| 11 | Per-world music variations → one track per world | 1 day | None |
| 12 | Hit-stop/camera micro-shake polish variants → a single preset | < 1 day | Minimal |
| 13 | Quiver badges → only world-completion badges | 1 day | None |

**Deep cuts (require OWNER decision + decision-log entry):**

| # | Deep cut | Saves | Consequence |
|---:|---|---|---|
| D1 | Ship the closed test/launch with **40 levels (W1–W2)**; W3 as the first content update | 4 weeks | Fails the §14 "60 levels" MVP definition; Bounce/portals/wind/ice slip post-launch |
| D2 | Portals post-MVP (W3 uses wind/ice/bounce only) | 1 week | Loses the "signature late-MVP" object; L50–52 and the L60 finale redesigned |
| D3 | Daily challenge post-MVP | 3–4 days | §14 lists it as must-ship |
| D4 | iOS launch after Android (staggered) | Mac/test overhead during M10 | Smaller launch footprint |

**Never cut:** bow draw feel and haptic/audio snap · accurate trajectory preview · limited quiver + stars by arrow efficiency · 1-tap restart < 1 s · settle/soft-lock rules · ropes · protected objects · material readability (colour + silhouette + icon) · fair, deterministic physics + the solvability bot · consent/privacy compliance · crash reporting · ethical ad rules (no ads in the first 10 min, never after fail) · original art/IP.

---

## 10. Post-MVP backlog (ordered by expected value; not scheduled)

| # | Item | Notes / prerequisite |
|---:|---|---|
| 1 | A/B tests (quiver sizes, preview length, ad cadence) | Only after soft-launch baselines (`06`) |
| 2 | New worlds (4+) every 3–4 weeks | Only after analytics show healthy W1–W3 completion/replay (§10) |
| 3 | Addressables + remote content delivery | Needed for live worlds without full app updates (D-013 revisit) |
| 4 | In-level arrow swap ("tap to swap next arrow") | Re-evaluate from playtest data (D-023) |
| 5 | Choice levels | Schema reserved (D-046) |
| 6 | Drill Arrow (pierces one timber layer) | Explicitly post-MVP in §5 |
| 7 | Hint system (soft highlight after N fails, maybe rewarded) | Must stay ethical; no paywalled solutions |
| 8 | Weekly challenge map + global leaderboard | Needs a backend; after ≥ 100 base levels (§10) |
| 9 | Seasonal cosmetic path (optional premium track) | §9 post-MVP candidate |
| 10 | Limited themed bow skins per world expansion | §9 |
| 11 | Cloud save (Game Center / Play Games / UGS) | Save schema already has `installId` + `schemaVersion` |
| 12 | Achievements (Game Center / Play Games) | Low cost after cloud identity |
| 13 | Localisation (top 5–8 languages) | `UIStrings` keys make it mechanical; adopt `com.unity.localization` |
| 14 | Push notifications (daily challenge reminder) | Opt-in only; consent-aware |
| 15 | Visual Verlet rope polish | D-009 alternative |
