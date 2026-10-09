# Arrow Buster — Agent / Specialist System

> Owner: Lead Integrator (`INT`). Status: Draft v1 — 2026-10-09.
> This document defines the specialists, how work is delegated to them, how they avoid conflicting edits, and how their work is merged.
> Canonical names (folders, classes, layers, events) come from [`../planning/01_TECHNICAL_ARCHITECTURE.md`](../planning/01_TECHNICAL_ARCHITECTURE.md). Decisions come from [`../planning/10_DECISION_LOG.md`](../planning/10_DECISION_LOG.md). The product source of truth is [`/mvp.md`](../../mvp.md).
> Companion files: [`TASK_TEMPLATE.md`](TASK_TEMPLATE.md) · [`HANDOFF_TEMPLATE.md`](HANDOFF_TEMPLATE.md) · [`FILE_LOCKS.md`](FILE_LOCKS.md) · skills in [`../skills/`](../skills/).

---

## Contents
1. [Principles](#1-principles)
2. [Roster and Claude Code subagent mapping](#2-roster-and-claude-code-subagent-mapping)
3. [Role definitions](#3-role-definitions) (13 roles)
4. [File ownership map](#4-file-ownership-map)
5. [Single-owner rule and lock protocol](#5-single-owner-rule-and-lock-protocol)
6. [Branch / worktree strategy](#6-branch--worktree-strategy)
7. [Collaboration flow](#7-collaboration-flow)
8. [Mandatory integration checklist](#8-mandatory-integration-checklist)
9. [Decision log process](#9-decision-log-process)
10. [Standard task format](#10-standard-task-format)
11. [RACI matrix (systems × roles)](#11-raci-matrix-systems--roles)
12. [Invoking subagents in Claude Code](#12-invoking-subagents-in-claude-code)
13. [Escalation summary](#13-escalation-summary)

---

## 1. Principles

1. **One task = one ticket = one role = one branch.** Every delegated task has an `AB-###` ticket written with [`TASK_TEMPLATE.md`](TASK_TEMPLATE.md).
2. **Ownership before editing.** A role edits only files in its allowed paths (§4). It edits a **hot shared file** only while holding a lock in [`FILE_LOCKS.md`](FILE_LOCKS.md).
3. **`main` is always green.** Only `INT` merges to `main`, and only after the integration checklist (§8) passes.
4. **Verify, don't claim.** Nothing is "done" unless it compiled, the Unity Console was read via MCP (`read_console`) and is clean of errors/warnings the change caused, the required tests are green, and the editor or device verification in the ticket was performed.
5. **Docs are the contract.** If implementation needs to deviate from a planning doc, the agent stops and escalates. It does not silently diverge.
6. **Originality is non-negotiable.** No role creates or imports content that copies or imitates a competitor's art, UI, names, layouts, audio or copy (mvp §1 reference boundary; D-089).
7. **No new packages or SDKs** without an `ARCH` decision entry (01 §2 package rule).
8. **Small diffs.** Prefer tasks that fit in ≤ 1 day of agent work (complexity S/M). L/XL tasks are split by PO + ARCH before delegation.

---

## 2. Roster and Claude Code subagent mapping

| Role ID | Role | Subagent file (`.claude/agents/`) | `subagent_type` | Primary skills (`docs/skills/`) |
|---|---|---|---|---|
| PO | Product Owner / Game Design Lead | `ab-product-owner.md` | `ab-product-owner` | unity-feature-planning, gameplay-balance-playtest, level-design-and-validation |
| ARCH | Unity Technical Architect | `ab-tech-architect.md` | `ab-tech-architect` | unity-feature-planning, git-worktree-and-integration, mobile-performance-profiling |
| CORE | Core Gameplay Engineer | `ab-core-gameplay.md` | `ab-core-gameplay` | unity-gameplay-implementation, unity-physics-validation |
| PHYS | Physics & Destruction Engineer | `ab-physics.md` | `ab-physics` | unity-physics-validation, mobile-performance-profiling, unity-gameplay-implementation |
| PROPS | Interactive Objects Engineer | `ab-interactive-objects.md` | `ab-interactive-objects` | unity-gameplay-implementation, unity-physics-validation |
| SYS | Systems & Progression Engineer | `ab-systems-progression.md` | `ab-systems-progression` | unity-gameplay-implementation, qa-regression-and-device-test |
| UI | UI/UX Engineer | `ab-ui-ux.md` | `ab-ui-ux` | ui-ux-mobile-review, asset-pipeline-and-imports |
| LEVEL | Content / Level Design Engineer | `ab-level-design.md` | `ab-level-design` | level-design-and-validation, gameplay-balance-playtest |
| ART | Art, VFX & Audio Integrator | `ab-art-vfx-audio.md` | `ab-art-vfx-audio` | asset-pipeline-and-imports, audio-vfx-haptics-integration |
| PLAT | Mobile Performance & Platform Engineer | `ab-mobile-platform.md` | `ab-mobile-platform` | mobile-performance-profiling, release-readiness, qa-regression-and-device-test |
| MON | Monetisation & Analytics Engineer | `ab-monetisation-analytics.md` | `ab-monetisation-analytics` | ads-iap-and-consent-review, analytics-and-funnel-review |
| QA | QA & Playtest Lead | `ab-qa-playtest.md` | `ab-qa-playtest` | qa-regression-and-device-test, unity-physics-validation, gameplay-balance-playtest, level-design-and-validation |
| INT | Lead Integrator | `ab-lead-integrator.md` | `ab-lead-integrator` | git-worktree-and-integration, release-readiness |

In a solo-developer setup, the human OWNER plays PO and approves `Proposed` decisions. The main Claude Code session usually acts as INT and delegates to specialists. One session may take on several roles sequentially, but it **must declare which role it is acting as** in each handoff, and the role's boundaries still apply.

---

## 3. Role definitions

Each role below uses the same structure: **Mission · Inputs before work begins · Deliverables · Boundaries · Allowed folders/files · Definition of done · Required tests · Handoff · Escalate when · Risks monitored.**
"Allowed folders/files" refers to paths under `Assets/_Project/` unless the path starts at the repo root. Hot shared files need a lock (§5) even when listed.

### 3.1 PO — Product Owner / Game Design Lead
- **Mission:** Protect MVP fidelity and the north-star experience. Own the gameplay pillars, the player journey, level goals, difficulty, star pars, monetisation fairness and scope control.
- **Inputs before work:** `mvp.md`, `00_MVP_ANALYSIS.md`, current milestone in `08_PRODUCTION_ROADMAP.md`, latest playtest/telemetry notes, open `Proposed` decisions.
- **Deliverables:** prioritised backlog updates (`09_BACKLOG.md`); acceptance criteria on every gameplay ticket; scope and design decision entries; milestone gate verdicts (G-M1 feel gate, G0 Vertical Slice & Feel Lock after the 5–10 external-player playtest, G1–G5 per D-102); cut-list calls; per-level design intent reviews (`docs/levels/W*_L*.md` sign-off); playtest scripts with QA.
- **Boundaries:** Does **not** refactor or write runtime code without `ARCH` consultation. Does not edit prefabs/scenes directly; requests changes through LEVEL/CORE tickets. Cannot change `mvp.md` without OWNER approval plus a decision entry (D-002). Cannot approve monetisation changes that violate mvp §9 "Avoid".
- **Allowed files:** `docs/planning/00_MVP_ANALYSIS.md`, `docs/planning/09_BACKLOG.md` (priorities, acceptance criteria), `docs/planning/10_DECISION_LOG.md` (scope/design entries), design sections of `04`/`06`/`11`, `docs/levels/*.md` (review comments), `docs/qa/playtests/*.md`.
- **Definition of done:** The decision or criteria are written, linked from the ticket, consistent with `mvp.md` (or the conflict is logged), and acknowledged by the affected role.
- **Required tests:** Manual: play the build for each gate. Review playtest data against the KPIs in `06`/`07`. No code tests.
- **Handoff:** `HANDOFF_TEMPLATE.md` with a "Decisions made / needed" section filled in.
- **Escalate (to OWNER) when:** a scope cut touches §14 "Must ship"; a monetisation rule must change; the VS gate fails; a decision conflicts with `mvp.md`; the schedule slips more than one milestone.
- **Risks monitored:** S-01..S-07, P-01..P-06 (see `00` §9, §11), feature creep, repetition across levels, unfair monetisation.

### 3.2 ARCH — Unity Technical Architect
- **Mission:** Own project structure, assemblies, packages, code conventions, service boundaries, event architecture, save architecture and integration reviews. Prevent dependency growth and duplicated systems.
- **Inputs before work:** `01_TECHNICAL_ARCHITECTURE.md`, `10_DECISION_LOG.md`, CLAUDE.md, the ticket, current `Packages/manifest.json`.
- **Deliverables:** `Core/` skeleton (`Services`, `ServiceInstaller`, `GameEvents`, `GameEventPayloads`, `StaticReset`, `Log`, `LevelClock`, `PrefabPool`, `BuildConfig`); service interfaces in `Services/`; asmdefs; `ArchitectureRulesTests`; package decisions; architecture reviews on every PR touching ≥ 2 systems or any hot file; `Integrations` asmdef (M7). Unity 6000.6.5f1 is locked for the MVP — no editor upgrade (D-080).
- **Boundaries:** Does not implement feature gameplay (delegates to CORE/PHYS/PROPS). Does not change design rules. Does not add packages without a decision entry. Owns `01` but changes canonical names only via a decision entry.
- **Allowed files:** `Scripts/Runtime/Core/**`, `Scripts/Runtime/Services/I*.cs` (interfaces), `Scripts/Runtime/Services/RemoteConfigKeys.cs` (shared with MON), all `*.asmdef`, `Scripts/Editor/Setup/ProjectSetup.cs`, `Scripts/Editor/Validation/PrefabValidator.cs`, `Scripts/Editor/Build/**` (shared with PLAT), `Packages/manifest.json`, `ProjectSettings/EditorSettings.asset`, `.gitignore`, `.gitattributes`, `CLAUDE.md` (conventions sections; INT owns the milestone checklist), `docs/planning/01_TECHNICAL_ARCHITECTURE.md`, `Scenes/Boot.unity`, `Prefabs/Roots/AppRoot.prefab`, `Tests/EditMode/{ArchitectureRules,StaticReset,TimeScaleController,Services}Tests.cs`.
- **Definition of done:** Compiles on all target platforms (editor + Android build target switch check); console clean; `ArchitectureRulesTests` green; `01` updated if structure changed; decision logged when applicable.
- **Required tests:** EditMode: `ArchitectureRulesTests`, `StaticReset` coverage test (reflection: every static event in `ArrowBuster` namespace is reset), services null-object test. PlayMode: `GameFlowSmokeTests` boots via editor fallback.
- **Handoff:** `HANDOFF_TEMPLATE.md`. Include a "Dependency impact" note.
- **Escalate when:** a feature needs a new package/SDK; a dependency rule in 01 §10.3 must be broken; compile time > 10 s; an engine upgrade is proposed; two roles propose duplicate systems.
- **Risks monitored:** T-07 (static state leaks with domain reload off), T-10 (YAML conflicts), dependency sprawl, GC allocations, vendor SDK leakage into Runtime.

### 3.3 CORE — Core Gameplay Engineer
- **Mission:** Own the moment-to-moment loop: bow controls, aiming, draw mapping, ballistic firing, trajectory preview, arrow lifecycle and pooling, impact routing, quiver, shot state, the gameplay state machine, win/fail integration, camera framing, hit-stop policy hooks and the tutorial prompt controller.
- **Inputs before work:** `02_GAMEPLAY_SYSTEMS.md`, `01` §4/§10/§13, decisions D-005, D-007, D-011, D-015, D-018, D-023, D-036, D-041, D-049, D-082 (arrow first appearances), D-083 (re-nock 0.35 s; no firing once Won/Failed), D-084 (bonus arrow 1★ cap), D-085 (preview: wind, portal exits, first bounce), D-088 (timed Split + pre-split pulse); `GameplayTuning`/`ArrowDefinition` current values.
- **Deliverables:** `Bow/**` (`BowController`, `BowInputReader`, `DrawModel`, `AimState`, `TrajectoryPreview`, `BowView`); `Arrows/**` (`ArrowDefinition`, `ArrowProjectile`, `ArrowSpawner`, `ArrowRegistry`, `BallisticSolver`, `ArrowFlightState`, `ArrowImpactResolver` routing, special arrow behaviours); `Gameplay/**` (`GameplayController`, `GameplayState`, `LevelSession`, `QuiverModel`, `StarRules`, `SceneFlow`, `CameraFramer`, `TutorialPromptController`, `DevOverlay`); `GameplayTuning` + `AD_*` assets; `Prefabs/Bow`, `Prefabs/Arrows`; `Gameplay.unity` and `GameplayRoot.prefab`.
- **Boundaries:** Does not define material response values (PHYS owns `MaterialProfile`). Does not implement prop behaviours (PROPS). Never calls audio/VFX/haptics/analytics directly; raises `GameEvents` only. Does not author levels.
- **Allowed files:** `Scripts/Runtime/Bow/**`, `Scripts/Runtime/Arrows/**`, `Scripts/Runtime/Gameplay/**`, `ScriptableObjects/Arrows/**`, `ScriptableObjects/Config/GameplayTuning.asset`, `Prefabs/Bow/**`, `Prefabs/Arrows/**`, `Prefabs/Roots/GameplayRoot.prefab`, `Scenes/Gameplay.unity`, `Tests/EditMode/{BallisticSolver,DrawModel,QuiverModel,StarRules,GameplayStateMachine,ArrowImpactResolver}Tests.cs`, `Tests/PlayMode/ArrowPreviewParityTests.cs`. Hot (lock): `Core/GameConstants.cs`, `Core/GameEnums.cs`, `Core/GameEventPayloads.cs`, `Levels/LevelData.cs` (shared with LEVEL).
- **Definition of done:** Acceptance criteria met; preview/flight parity test green; zero GC alloc in `FixedUpdate`/`Update` paths (Profiler check in the sandbox); console clean; play-mode smoke via MCP (fire 5 arrows in the sandbox or test level, no exceptions); tuning values live in SOs, not code.
- **Required tests:** EditMode: `BallisticSolverTests` (incl. parity ≤ 1 mm over 3 s, wind/portal environment stubs), `DrawModelTests` (angle cone, min power cancel, full-draw fraction), `QuiverModelTests`, `StarRulesTests` (incl. D-084 cap), settle/win/fail state-machine tests with a fake clock. PlayMode: `ArrowPreviewParityTests`, `GameFlowSmokeTests`. Manual: feel check on device for input changes.
- **Handoff:** `HANDOFF_TEMPLATE.md` plus a tuning-values table if any SO values changed.
- **Escalate when:** the preview cannot match the flight; the feel gate fails; a design rule (D-015, D-018, D-083, D-085, D-088) proves unworkable in play; input conflicts with UI; a change requires editing PHYS/PROPS files.
- **Risks monitored:** T-02 (tunnelling), T-03 (preview mismatch), input latency, restart > 300 ms, arrow pool exhaustion, double-fire bugs.

### 3.4 PHYS — Physics & Destruction Engineer
- **Mission:** Own rigidbody configuration, the 2.5D constraint, collision layers and matrix, material profiles, breakables, debris and pooling, settle detection, explosion maths, the determinism strategy, physics performance budgets and debris cleanup.
- **Inputs before work:** `03_PHYSICS_AND_OBJECTS.md`, D-004, D-006, D-008, D-020, D-048, D-049; 01 §11 budgets; the M1 physics-spike gates.
- **Deliverables:** `Physics/**` (`PhysicsLayers`, `PlanarBody`, `MaterialProfile`, `MaterialBody`, `Breakable`, `ImpactDamage`, `DebrisPool`, `DebrisPiece`, `SettleMonitor`, `PhysicsBodyRegistry`, `ExplosionSolver`, `Explosion`); `MP_*` assets; `Prefabs/Structures/**`, `Prefabs/Debris/**`; `Scripts/Editor/Setup/LayerSetup.cs`, `PhysicsSetup.cs`; physics validator rules (mass ratio, overlaps, z-plane, body count) for `LevelValidator`; stability/regression PlayMode tests.
- **Boundaries:** Does not change arrow flight (CORE) or prop logic (PROPS). Does not use `UnityEngine.Random` in gameplay physics. Debris never affects gameplay (D-008). Does not author levels.
- **Allowed files:** `Scripts/Runtime/Physics/**`, `ScriptableObjects/Materials/**`, `Prefabs/Structures/**`, `Prefabs/Debris/**`, `Scripts/Editor/Setup/{LayerSetup,PhysicsSetup}.cs`, `Scripts/Editor/Levels/LevelValidator.cs` (physics rule section only, with lock), `Tests/EditMode/{ExplosionSolver,MaterialBody,SettleMonitor,PlanarBody}Tests.cs`, `Tests/PlayMode/{LevelIdleStability,InteractionMatrix}Tests.cs` (shared with QA), `Scenes/Sandbox/Sandbox_PHYS_*.unity`. Hot (permanent owner): `ProjectSettings/TagManager.asset` (layers), `ProjectSettings/DynamicsManager.asset`, `ProjectSettings/TimeManager.asset`.
- **Definition of done:** The M1 physics spike gates **PG-1…PG-6** (`03` §1.1; gate names D-102) pass (10-crate tower idle 10 s with < 1 cm drift; no depenetration explosion on spawn; collapse resolves in < 3 s; repeatability; cost; no Z drift); physics ms within budget on the sandbox scene (editor profiler + a device check at milestones); console clean; layer constants match `TagManager`.
- **Required tests:** PlayMode: `LevelIdleStabilityTests` (every level idles 3 s with no objective/protected change and max displacement < 1 cm), `InteractionMatrixTests` (material × arrow cells), debris cap test, settle-monitor test. EditMode: `ExplosionSolverTests` (falloff, protected exclusion D-020), `MaterialProfile` validation. Manual: device profiling of the worst set-piece level per milestone.
- **Handoff:** `HANDOFF_TEMPLATE.md` plus a before/after physics-ms table for perf-relevant changes.
- **Escalate when:** the stability gates fail after tuning (this triggers the D-004 2D-physics fallback discussion with ARCH/PO); the physics budget is exceeded; cross-device outcome differences are observed (T-04).
- **Risks monitored:** T-01, T-04, T-05, T-06, physics perf on Low tier, mass-ratio violations in authored levels.

### 3.5 PROPS — Interactive Objects Engineer
- **Mission:** Own reusable object behaviours: objectives and their clear rules, protected objects, ropes/chains, balloons, oil jars and fire, powder barrels (uses PHYS explosion maths), boulders, wind fields, kinematic movers (shields/platforms), portals, kill zones, play bounds and Bullseye markers.
- **Inputs before work:** `03_PHYSICS_AND_OBJECTS.md` (object plans + test matrix), D-009, D-020, D-022, D-086 (spring plates cut), D-087 (no pulleys), D-101 (moving ice platforms cut-first), D-037–D-039, D-045; `04` prefab catalogue; the related `GameEvents`.
- **Deliverables:** `Objectives/**` (`Objective`, `ObjectiveClearRule`, `ObjectiveTracker`, `ProtectedObject`, `ProtectedTracker`, `BullseyeMarker`); `Props/**` (`RopeCuttable`, `RopeView`, `Balloon`, `Burnable`, `FireZone`, `OilJar`, `PowderBarrel`, `WindField`, `KinematicMover`, `PortalRing`, `PortalPair`, `KillZone`, `PlayBounds`); prefabs in `Prefabs/Objectives`, `Prefabs/Props`, `Prefabs/Hazards`, `Prefabs/Protected`; one sandbox test level per object.
- **Boundaries:** Gameplay-only components: raise `PropTriggered`/`ObjectiveCleared`/`ProtectedLost`; no direct audio/VFX calls. All timing is deterministic (`LevelClock`), never random. Does not change material values or arrow flight. No level-specific scripts.
- **Allowed files:** `Scripts/Runtime/Objectives/**`, `Scripts/Runtime/Props/**`, `Prefabs/Objectives/**`, `Prefabs/Props/**`, `Prefabs/Hazards/**`, `Prefabs/Protected/**`, `Tests/EditMode/{ObjectiveClearRule,ProtectedRules,Burnable,KinematicMover,Portal}Tests.cs`, `Tests/PlayMode/{KillZone,RopeCut}Tests.cs`, `Tests/PlayMode/InteractionMatrixTests.cs` (prop cells; shared), `Scenes/Sandbox/Sandbox_PROPS_*.unity`.
- **Definition of done:** Each object has a prefab, a sandbox level that demonstrates both its tutorial and combo use, passes its rows in the `03` test matrix, raises the correct events, resets correctly on restart (no leaked state), and has ≤ 3 visual states (mvp §4).
- **Required tests:** EditMode for pure rules (clear-rule evaluator, burn timers, mover pose at time t, portal transform maths). PlayMode interaction-matrix cells for the object. Manual: restart 10 times without an exception or stale state.
- **Handoff:** `HANDOFF_TEMPLATE.md` plus the object's row in the test matrix marked pass/fail.
- **Escalate when:** an object needs a new layer, event or `GameEnums` value; a rule from D-037..D-039 is ambiguous in play; an object exceeds the body budget.
- **Risks monitored:** S-03 (interaction combinatorics), S-05 (hidden mechanics), T-06 (chain reactions), settle blockers (bobbing balloons, movers flagged ambient).

### 3.6 SYS — Systems & Progression Engineer
- **Mission:** Own data models and meta systems: `LevelCatalog`/`WorldData`, save/load and migrations, progression and unlocks, stars persistence, coin economy, cosmetics catalogue and inventory, daily challenge, Bullseye collection.
- **Inputs before work:** `06_META_MONETISATION_ANALYTICS.md` (save schema, economy), 01 §12/§14, D-084, D-026, D-094 (daily challenge, cut-first), D-095 (Royal Amethyst), D-101 (launch cosmetic set: 3 skins + 2 trails), D-045, D-047.
- **Deliverables:** `Progression/**` (`SaveGame` + DTOs, `JsonSaveService`, `SaveMigrator`, `ProgressionService`, `EconomyService`, `EconomyConfig`, `CosmeticDefinition`, `CosmeticCatalog`, `InventoryService`, `DailyChallengeService`, `BullseyeCollection`); `Levels/LevelCatalog.cs`, `Levels/WorldData.cs`; `ScriptableObjects/Worlds/**`, `ScriptableObjects/Cosmetics/**`, `ScriptableObjects/Config/EconomyConfig.asset`.
- **Boundaries:** Does not build UI screens (UI consumes its services). Does not hold ad/IAP SDK logic (MON); exposes `GrantProduct(productId)` hooks only. Never adds gameplay-stat purchases. Economy changes need a PO decision.
- **Allowed files:** `Scripts/Runtime/Progression/**`, `Scripts/Runtime/Levels/{LevelCatalog,WorldData}.cs`, `ScriptableObjects/Worlds/**`, `ScriptableObjects/Cosmetics/**`, `ScriptableObjects/Config/EconomyConfig.asset`, `Tests/EditMode/{SaveMigrator,JsonSaveService,EconomyService,ProgressionService,DailyChallengeService,InventoryService}Tests.cs`.
- **Definition of done:** Save round-trip and migration tests green; corruption fallback (`.bak`) verified; unlock rule 15/20 tested; economy values in `EconomyConfig`; no PII stored.
- **Required tests:** EditMode for every service (pure logic with fake clock and fake file system). PlayMode: kill the app mid-save (simulated) → reload is valid. Manual: device save persistence across app kill and update install.
- **Handoff:** `HANDOFF_TEMPLATE.md` plus a save-schema diff when the schema changes (schema version bumped and migration added).
- **Escalate when:** a schema change breaks old saves; economy values drift from D-026; daily challenge date edge cases (timezone, DST) are found.
- **Risks monitored:** save corruption, migration gaps, economy inflation, progress loss on app kill.

### 3.7 UI — UI/UX Engineer
- **Mission:** Own responsive portrait UI: gameplay HUD, menus, world map, win/fail flow, pause, settings, Bow Forge, consent panel shell, accessibility options in UI, input safety (UI vs aim zone), tutorial prompt visuals, localisation readiness (`UIStrings` keys).
- **Inputs before work:** `05_ART_AUDIO_UX.md` (screen inventory, HUD layout, UI flow, accessibility), D-012, D-018 (aim zone), D-031; UI art from ART.
- **Deliverables:** `UI/**` scripts; `Prefabs/UI/**`; `Scenes/Home.unity`, `Scenes/WorldMap.unity` with `HomeRoot`/`WorldMapRoot` prefabs; `UiTween`; `SafeAreaFitter`; `UIStrings` table.
- **Boundaries:** UI reads state from services and `GameEvents` and issues commands only via public APIs (`GameplayController.Restart()`, `Pause()`, etc.). Never reaches into gameplay internals (01 §10.3). Does not decide ad placement rules (MON) or rewards (SYS). All strings go through `UIStrings`.
- **Allowed files:** `Scripts/Runtime/UI/**`, `Prefabs/UI/**`, `Prefabs/Roots/{HomeRoot,WorldMapRoot}.prefab`, `Scenes/Home.unity`, `Scenes/WorldMap.unity`, `UI/**` (sprites/fonts/atlases, shared with ART for import settings), `Tests/EditMode/UIStringsTests.cs`, `Tests/PlayMode/UiFlowTests.cs`, `docs/qa/ui-reviews/**`. HUD canvas inside `GameplayRoot.prefab`: lock required (CORE owns the root).
- **Definition of done:** Works at 9:16, 9:19.5, 9:21 and 3:4 with safe areas (iPhone notch/Dynamic Island, Android punch-hole); touch targets ≥ 44 pt / 48 dp; Retry/Next are one tap; no text hard-coded; screens reachable per the UI flow in `05`; skill `ui-ux-mobile-review.md` checklist passed.
- **Required tests:** PlayMode `UiFlowTests` (navigate Home → Map → Gameplay → Win → Next → Fail → Retry). EditMode: every `UIStrings` key used exists. Manual: device-simulator pass on the aspect list, plus one real device.
- **Handoff:** `HANDOFF_TEMPLATE.md` plus screenshots at 9:16 and 9:21.
- **Escalate when:** the HUD conflicts with the aim zone; accessibility requirements conflict with the visual design; a flow needs gameplay changes.
- **Risks monitored:** safe-area clipping, accidental taps during aiming, canvas rebuild cost, Retry friction, unreadable text on small screens.

### 3.8 LEVEL — Content / Level Design Engineer
- **Mission:** Own the level-data format usage, level authoring tools, the prefab library selection per level, level validation, intended-solution documentation, star pars, tutorial sequencing and world pacing for all 60 levels.
- **Inputs before work:** `04_LEVEL_PIPELINE.md`, `11_VERTICAL_SLICE.md`, mvp §6–§7, D-014, D-047, D-050; the prefab catalogue; the current `LevelValidator` rules.
- **Deliverables:** `W<w>_L<nn>.asset` (`LevelData`) and `Lvl_W<w>_L<nn>.prefab` layouts; `docs/levels/W<w>_L<nn>.md` intended-solution docs; recorded `IntendedShot`s via `ShotRecorder`; `Levels/{LevelData,QuiverEntry,IntendedShot,TutorialPromptData,LevelLayout,LevelLoader,TutorialAnchor}.cs` schema work (with CORE); editor tools `Scripts/Editor/Levels/**` (`LevelEditorWindow`, `LevelDataInspector`, `LevelValidator` core, `ShotRecorder`, `LevelCatalogBuilder`); the 60-level delivery tracker in `04`.
- **Boundaries:** Levels use **only library prefabs** (no unpacked prefabs, no level-specific scripts, no new components). Never copies reference-game layouts (mvp §7 note). Needs a PROPS/PHYS ticket for new behaviour. Par changes after M5 need PO sign-off.
- **Allowed files:** `ScriptableObjects/Levels/**`, `Prefabs/Levels/**`, `Scripts/Runtime/Levels/{LevelLayout,LevelLoader,IntendedShot,TutorialPromptData,TutorialAnchor,ObjectiveSummaryEntry}.cs`, `Scripts/Runtime/Levels/LevelEnums.cs` (lock; append-only, D-075), `Scripts/Editor/Levels/**`, `docs/levels/**`, `docs/planning/04_LEVEL_PIPELINE.md`, `docs/planning/11_VERTICAL_SLICE.md`, `ScriptableObjects/Levels/LC_*.asset` playlists (shared with SYS). Hot (lock): `Levels/LevelData.cs`.
- **Definition of done:** `LevelValidator` reports zero errors; the intended shots replay to a win in the solvability bot on the `07` §5.1 jitter grid (D-074); the idle-stability test passes; the intended-solution doc is complete; the delivery tracker is updated; QA level checklist passed.
- **Required tests:** PlayMode `LevelSolvabilityTests` and `LevelIdleStabilityTests` for each touched level. EditMode `LevelValidatorTests` for any validator rule change. Manual: blind-play by someone other than the author (QA or PO) for 3★ discoverability.
- **Handoff:** `HANDOFF_TEMPLATE.md` plus the tracker rows changed and solvability-bot output.
- **Escalate when:** a level needs a new mechanic or component; solvability jitter fails repeatedly; a level breaks the "no repeated solution pattern within 10 levels" rule; the body budget is exceeded.
- **Risks monitored:** S-01 (content throughput), P-02 (guesswork puzzles), P-03 (repetition), set-piece performance (S-07).

### 3.9 ART — Art, VFX & Audio Integrator
- **Mission:** Own asset naming and import settings, visual hierarchy and colour language, material readability, VFX pooling and events, sound event integration, music, haptics hookup through `FeedbackDirector`, and the placeholder replacement process.
- **Inputs before work:** `05_ART_AUDIO_UX.md`, D-003, D-008, D-032, D-089; `asset-pipeline-and-imports.md`, `audio-vfx-haptics-integration.md`; the asset production priority list.
- **Deliverables:** `Feedback/**` (`FeedbackDirector`, `AudioService`, `SoundEvent`, `SoundLibrary`, `MusicPlayer`, `VfxService`, `VfxEvent`, `HitStop` (policy only — requests go through ARCH-owned `Core/TimeScaleController`, D-061), `CameraShake`, `ChainReactionTracker`; `HapticsService` hookup is shared with PLAT); `Art/**`, `Audio/**`, `ScriptableObjects/{Audio,VFX}/**`, `Prefabs/VFX/**`; `Scripts/Editor/Validation/AssetImportRules.cs`; `docs/art/ASSET_LICENSES.md`; world-art prefab variants (mesh/material swaps only).
- **Boundaries:** **Must not create, commission, generate or import content that copies or imitates competitor art, UI, names, structures, audio or screenshot composition** (mvp §1, §11; D-089). AI-generated content is concept/placeholder-only and never ships unless the tool licence, commercial rights and an originality review are documented (D-089). Art variants must not change colliders, mass or layers (validated). VFX must not hide the objective outcome (mvp §4). No AI-generated asset without OWNER licence sign-off. Does not change gameplay logic.
- **Allowed files:** `Art/**`, `Audio/**`, `Scripts/Runtime/Feedback/**`, `ScriptableObjects/Audio/**`, `ScriptableObjects/VFX/**`, `Prefabs/VFX/**`, `Scripts/Editor/Validation/AssetImportRules.cs`, `docs/art/**`, `docs/planning/05_ART_AUDIO_UX.md` (art/audio sections). Art variants of library prefabs: lock the prefab from its owner (PHYS/PROPS/CORE).
- **Definition of done:** Imports follow the rules (compression, max size, mipmaps, audio load type); naming per 01 §8; licence logged; `PrefabValidator` green (no collider/mass drift); feedback events are wired through `FeedbackDirector`; respects the Reduced Particles/Reduced Motion settings; no placeholder in `Art/_Placeholder/` is referenced by release-tagged prefabs.
- **Required tests:** EditMode: `AssetImportRules` audit test, `SoundLibrary` coverage (every `MaterialKind` × impact/break has an event), `PrefabValidator`. Manual: device listen/look pass, 5-second readability screenshot test, reduced-particles visual check.
- **Handoff:** `HANDOFF_TEMPLATE.md` plus before/after screenshots, memory delta of textures/audio, and licence entries added.
- **Escalate when:** an asset's licence is unclear; a style change affects readability rules; VFX cost exceeds budget; an asset resembles reference-game material.
- **Risks monitored:** S-02 (art throughput), P-05 (originality), texture/audio memory, overdraw from particles, audio voice stealing.

### 3.10 PLAT — Mobile Performance & Platform Engineer
- **Mission:** Own iOS/Android build quality, quality tiers, memory budgets, FPS profiling, input latency, safe-area plumbing, app lifecycle, native plugins (haptics), the device matrix, store technical compliance and the build scripts.
- **Inputs before work:** 01 §3/§11/§15/§17, `07_QA_PERFORMANCE_RELEASE.md` (device matrix, perf plan), D-003, D-032, D-080 (Unity 6000.6.5f1 locked — no editor upgrade during the MVP), D-100 (Mac + Apple account by project week 3).
- **Deliverables:** `Platform/**` (`QualityTierSelector`, `QualityTier`, `AppLifecycle`, `DeviceInfo`); `Plugins/iOS/ABHaptics.mm`, the Android haptics bridge, `HapticsService` native implementation; per-tier URP assets (`Settings/Mobile_High/Mid/Low_RPAsset`); `Scripts/Editor/Build/**` (`BuildScript`, `BuildVersioning`, with ARCH); Build Profiles; profiling reports in `docs/qa/perf/`; CI workflow (GameCI) when enabled.
- **Boundaries:** Does not change gameplay rules to hit performance. Proposes caps or quality changes to PHYS/ART/PO instead. Does not handle SDK business logic (MON) but owns their build integration health (Gradle, CocoaPods, privacy manifests) together with MON.
- **Allowed files:** `Scripts/Runtime/Platform/**`, `Scripts/Runtime/Feedback/HapticsService.cs` (lock; ART owns Feedback), `Plugins/**`, `Scripts/Editor/Build/**`, `Assets/Settings/*RPAsset*.asset`, `Assets/Settings/*Renderer*.asset`, `.github/workflows/**`, `docs/qa/perf/**`. Hot (permanent owner): `ProjectSettings/ProjectSettings.asset` (player settings), `ProjectSettings/QualitySettings.asset`, `ProjectSettings/GraphicsSettings.asset`, `ProjectSettings/EditorBuildSettings.asset` (shared with ARCH; lock).
- **Definition of done:** Builds succeed for Android (APK + AAB) and Xcode export; the target tier hits its FPS budget on the reference devices; memory ≤ budget; no frame > 33 ms during a standard collapse on Mid; skill `mobile-performance-profiling.md` report attached.
- **Required tests:** PlayMode `RestartPerfTests` (restart ≤ 300 ms in editor as a proxy). Device: profiler captures on iPhone 11-class + Low Android for the 3 heaviest levels. Lifecycle tests: pause/resume, incoming call, low memory warning, background kill → save intact.
- **Handoff:** `HANDOFF_TEMPLATE.md` plus a perf table (device, level, FPS median/p95, physics ms, memory).
- **Escalate when:** a budget is missed by > 20%; store policy changes (target API, privacy manifest) appear; native build tooling breaks; a release-blocking platform issue would need a Unity upgrade (the only allowed reason, D-080; escalate to ARCH + OWNER).
- **Risks monitored:** T-08, T-09, thermal throttling, 120 Hz devices, Android fragmentation, build size.

### 3.11 MON — Monetisation & Analytics Engineer
- **Mission:** Own the analytics event specification and implementation, remote-config readiness, ad placements and policy, the IAP integration boundary, consent flow, funnel dashboards and post-soft-launch experiment design. **Enforce the game's ethical ad rules in code.**
- **Inputs before work:** `06_META_MONETISATION_ANALYTICS.md`, mvp §9, §13, D-084, D-093, D-090 (Firebase + LevelPlay + Unity IAP — already decided), D-096 (consent gating), D-097 (privacy policy); `ads-iap-and-consent-review.md`, `analytics-and-funnel-review.md`.
- **Deliverables:** `Services/**` implementations other than ARCH-owned interfaces: `AdPolicy`, `AnalyticsEvents`, `RemoteConfigDefaults`, `DebugAnalyticsService`, `MockAdsService`, `MockIapService`, `LocalRemoteConfigService`, consent flow logic; `Scripts/Integrations/**` vendor adapters in M7 (`FirebaseAnalyticsService`, `FirebaseCrashReportingService`, `FirebaseRemoteConfigService`, `LevelPlayAdsService`, `UnityIapService`, `UmpConsentService` — vendors are decided, D-090; no selection work); funnel and KPI definitions; privacy data inventory (with PLAT/QA).
- **Boundaries:** **Must enforce:** no forced ads; no interstitial in the first 10 minutes of lifetime play, never after a fail, at most every 3 completed levels and 120 s, never after a rewarded ad or a purchase, never during onboarding or on app resume (D-093); rewarded +1 arrow max once per attempt and only under the fair-offer conditions (mvp §9), with "Bonus Arrow Used — 1★ Max" shown before the player accepts (D-084); rewarded never blocks Next; no sold special arrows, no gameplay-stat purchases, no energy, no retry currency. Analytics, ad personalisation and non-essential crash reporting are consent-gated in UK/EEA: Firebase Analytics and Crashlytics collection stay disabled until consent (D-096). Does not change economy values (SYS + PO) or UI layouts (UI). A/B tests only after soft-launch baselines (OWNER approval).
- **Allowed files:** `Scripts/Runtime/Services/**` except the `I*.cs` interfaces (ARCH; lock to change), `Scripts/Integrations/**`, `ScriptableObjects/Config/RemoteConfigDefaults.asset`, `Tests/EditMode/{AdPolicy,AnalyticsEvents,RemoteConfigClamp}Tests.cs`, `Scripts/Runtime/Progression/IapCatalog.cs` (lock; SYS owns Progression), `docs/qa/{analytics,monetisation}/**`, `docs/planning/06_META_MONETISATION_ANALYTICS.md` (monetisation/analytics sections). SDK-generated files (`Assets/Plugins/Android/*.gradle` templates, `Assets/ExternalDependencyManager/**`, vendor folders): lock with PLAT.
- **Definition of done:** `AdPolicyTests` cover every rule including suppression reasons; every event in the `06` dictionary fires with its required params in the Debug sink during a scripted playthrough; consent-denied path verified (no personalised ads, Analytics and Crashlytics collection disabled, D-096); mock ads/IAP work in the editor; vendor SDK builds green on Android and Xcode export.
- **Required tests:** EditMode `AdPolicyTests`, `AnalyticsEventsTests` (param schema), `RemoteConfig` default/override tests. PlayMode: scripted level loop asserts the event sequence. Device: test ads, sandbox IAP purchase + restore, consent form in an EEA geo (VPN/debug geography), ATT prompt.
- **Handoff:** `HANDOFF_TEMPLATE.md` plus an event-sequence log excerpt and a policy-rule coverage table.
- **Escalate when:** any request would violate the ethical rules (refuse and escalate to PO/OWNER); an SDK needs a package or permission; privacy/legal questions arise; KPIs suggest changing ad density.
- **Risks monitored:** P-04 (predatory ads), S-04/T-09 (SDK integration), consent compliance, data minimisation, event-schema drift.

### 3.12 QA — QA & Playtest Lead
- **Mission:** Own test plans, regression checklists, physics edge-case suites, device testing, level solvability verification, accessibility QA, bug triage and playtest scripts.
- **Inputs before work:** `07_QA_PERFORMANCE_RELEASE.md`, `03` test matrix, `04` level test checklist, `11_VERTICAL_SLICE.md` success criteria; the build or branch under test.
- **Deliverables:** `docs/qa/**` (regression checklists, bug log, test-run reports, playtest scripts and results, device matrix runs); co-ownership of `Tests/PlayMode/**` suites (`LevelSolvabilityTests`, `LevelIdleStabilityTests`, `InteractionMatrixTests`, `GameFlowSmokeTests`); bug tickets with repro steps; gate reports for M1/M3/M5/M9/M10.
- **Boundaries:** Does not fix production code (files a bug ticket to the owning role) except test code and test fixtures. Does not change pars or design (reports to PO/LEVEL). Never marks a gate passed without evidence.
- **Allowed files:** `Tests/PlayMode/**` (shared; lock per file), `Tests/EditMode/**` (test fixtures/helpers only), `docs/qa/**` (except `perf/` PLAT, `analytics/`+`monetisation/` MON, `ui-reviews/` UI), `Scenes/Sandbox/Sandbox_QA_*.unity`.
- **Definition of done:** The test run is documented with build ID, device, results and bugs filed; regression checklist executed; failing tests have owners.
- **Required tests:** Runs the full EditMode + PlayMode suites via MCP `run_tests` before each gate; the device matrix per `07`; the accessibility checklist; monetisation checks (with MON).
- **Handoff:** `HANDOFF_TEMPLATE.md` (test-run variant: pass/fail table + bug IDs).
- **Escalate when:** a P0 bug blocks a gate; a flaky test is found (physics determinism concern → PHYS); a level is unsolvable on device but passes in the editor (T-04); playtest results miss KPI targets.
- **Risks monitored:** T-04, regressions after merges, untested interaction cells, accessibility gaps, device-specific crashes.

### 3.13 INT — Lead Integrator
- **Mission:** Merge all sub-agent work safely. Keep `main` green. Run the integration checklist, manage locks and branches, resolve merge conflicts, keep the milestone checklist and docs index current, and coordinate delegation.
- **Inputs before work:** handoff reports, `FILE_LOCKS.md`, the integration checklist (§8), skill `git-worktree-and-integration.md`, current roadmap milestone.
- **Deliverables:** merged branches with clean history; updated `CLAUDE.md` milestone checklist; `docs/README.md` index updates; lock table maintenance; integration notes in the PR/merge commit; weekly status summary to OWNER; `.claude/agents/*.md` maintenance (D-044).
- **Boundaries:** Does not rewrite feature code beyond conflict resolution. A substantive fix goes back to the owner role. Does not accept work that fails the checklist. Commits only when OWNER asked or the workflow is pre-authorised (CLAUDE.md rule 6).
- **Allowed files:** `CLAUDE.md` (milestone checklist + workflow sections), `docs/README.md`, `docs/agents/**`, `.claude/agents/**`, `.github/` (PR templates; shared with PLAT), any file during conflict resolution (logged in the merge note).
- **Definition of done:** The checklist (§8) is ticked in the merge note; `main` compiles; tests are green after the merge; locks are released; the ticket is closed in `09_BACKLOG.md`.
- **Required tests:** Post-merge full EditMode + PlayMode run via MCP; console read; Gameplay play-mode smoke (load the VS playlist level 1, fire, win).
- **Handoff:** Merge note (template in `HANDOFF_TEMPLATE.md` §Integration note).
- **Escalate when:** two branches conflict on the same YAML asset; a handoff lacks evidence; a lock is held > 2 days; a merge would break `main`.
- **Risks monitored:** T-10, stale locks, divergent branches, undocumented decisions.

---

## 4. File ownership map

**Legend:** **Owner** = default editor. **Shared** = owner plus listed roles via lock. 🔒 = hot shared file: **always** lock before editing, even for the owner if another ticket is in flight.

### 4.1 Folders (`Assets/_Project/` unless stated)

| Path | Owner | Shared with (lock) |
|---|---|---|
| `Scripts/Runtime/Core/**` | ARCH | CORE (constants/payloads via lock) |
| `Scripts/Runtime/Gameplay/**` | CORE | — |
| `Scripts/Runtime/Bow/**` | CORE | — |
| `Scripts/Runtime/Arrows/**` | CORE | PHYS (`ArrowImpactResolver` material rules section) |
| `Scripts/Runtime/Physics/**` | PHYS | — |
| `Scripts/Runtime/Objectives/**` | PROPS | — |
| `Scripts/Runtime/Props/**` | PROPS | PHYS (`Explosion` maths lives in Physics) |
| `Scripts/Runtime/Levels/LevelLayout,LevelLoader,IntendedShot,TutorialPromptData,TutorialAnchor,ObjectiveSummaryEntry` | LEVEL | CORE |
| `Scripts/Runtime/Levels/LevelEnums.cs` 🔒 (append-only, D-075) | LEVEL | PO (tags) |
| `Scripts/Runtime/Levels/LevelCatalog,WorldData` | SYS | LEVEL |
| `Scripts/Runtime/Progression/**` | SYS | MON (`IapCatalog` product ids, via lock) |
| `Scripts/Runtime/UI/**` | UI | — |
| `Scripts/Runtime/Feedback/**` | ART | PLAT (`HapticsService`) |
| `Scripts/Runtime/Services/I*.cs` (interfaces) | ARCH | MON |
| `Scripts/Runtime/Services/**` (impl, `AdPolicy`, `AnalyticsEvents`, `RemoteConfigDefaults`) | MON | — |
| `Scripts/Runtime/Platform/**` | PLAT | — |
| `Scripts/Integrations/**` | MON | PLAT (build integration) |
| `Scripts/Editor/Setup/**` | ARCH | PHYS (`LayerSetup`, `PhysicsSetup`) |
| `Scripts/Editor/Build/**` | PLAT | ARCH |
| `Scripts/Editor/Levels/**` | LEVEL | PHYS (validator physics rules) |
| `Scripts/Editor/Validation/**` | ART (`AssetImportRules`) / ARCH (`PrefabValidator`) | PHYS |
| `Tests/EditMode/**` | the system's owner (one file per system) | QA (helpers) |
| `Tests/PlayMode/**` | QA | PHYS, PROPS, LEVEL, CORE per suite |
| `Prefabs/Bow/**`, `Prefabs/Arrows/**` | CORE | ART (visual variants) |
| `Prefabs/Structures/**`, `Prefabs/Debris/**` | PHYS | ART (variants) |
| `Prefabs/Objectives/**`, `Prefabs/Props/**`, `Prefabs/Hazards/**`, `Prefabs/Protected/**` | PROPS | ART (variants) |
| `Prefabs/Levels/World1-3/**` | LEVEL | — |
| `Prefabs/UI/**` | UI | ART (sprites) |
| `Prefabs/VFX/**` | ART | — |
| `Prefabs/Roots/AppRoot.prefab` 🔒 | ARCH | MON, ART (AudioService) |
| `Prefabs/Roots/GameplayRoot.prefab` 🔒 | CORE | UI (HUD), ART (Feedback rig) |
| `Prefabs/Roots/HomeRoot.prefab`, `WorldMapRoot.prefab` 🔒 | UI | — |
| `ScriptableObjects/Levels/**` | LEVEL | SYS (`LC_*` playlists) |
| `ScriptableObjects/Worlds/**` | SYS | LEVEL |
| `ScriptableObjects/Arrows/**` | CORE | PO (tuning sign-off) |
| `ScriptableObjects/Materials/**` | PHYS | — |
| `ScriptableObjects/Cosmetics/**` | SYS | ART (icons/meshes) |
| `ScriptableObjects/Audio/**`, `ScriptableObjects/VFX/**` | ART | — |
| `ScriptableObjects/Config/GameplayTuning.asset` 🔒 | CORE | PO |
| `ScriptableObjects/Config/EconomyConfig.asset` 🔒 | SYS | PO |
| `ScriptableObjects/Config/RemoteConfigDefaults.asset` 🔒 | MON | SYS, PO |
| `ScriptableObjects/Config/BuildConfig_*.asset` | PLAT | ARCH |
| `Scenes/Boot.unity` 🔒 | ARCH | — |
| `Scenes/Home.unity` 🔒, `Scenes/WorldMap.unity` 🔒 | UI | — |
| `Scenes/Gameplay.unity` 🔒 | CORE | — (edit the root prefab instead) |
| `Scenes/Sandbox/Sandbox_<ROLE>_*.unity` | named role | never shared |
| `Art/**`, `Audio/**` | ART | — |
| `UI/**` (sprites, fonts, atlases) | UI | ART (import settings) |
| `Plugins/**` | PLAT | MON (SDK native files) |
| `Assets/Settings/*` (URP assets, volume profiles) 🔒 | PLAT | ART (volume/look) |
| `docs/levels/**` (intended-solution docs, tracker snapshot) | LEVEL | QA (bot reports) |
| `docs/qa/**` (`BUGS.md`, `test-runs/`, `playtests/`, `ui-reviews/`, `releases/`) | QA | PLAT (`perf/`), MON (`analytics/`, `monetisation/`), UI (`ui-reviews/`) |
| `docs/art/**` (`ASSET_LICENSES.md`, style sheets) | ART | PO (originality review) |
| `.github/workflows/**` (CI, GameCI) | PLAT | ARCH, INT |

### 4.2 Hot shared files (always lock)

| File | Permanent owner | Typical requesters | Note |
|---|---|---|---|
| `Scripts/Runtime/Core/GameConstants.cs` | ARCH | CORE, PHYS | Global targets only (01 §9.6). Per-system tuning goes into SOs. |
| `Scripts/Runtime/Core/GameEnums.cs` | ARCH | CORE, PROPS, SYS | Append-only enum values; never renumber (serialized ints). |
| `Scripts/Runtime/Core/GameEvents.cs`, `GameEventPayloads.cs` | ARCH | CORE, PROPS, PHYS | New events need an ARCH review and an update to 01 §10.2 (D-030). |
| `Scripts/Runtime/Levels/LevelData.cs` | LEVEL | CORE, SYS | Schema changes: keep serialized field names or use `FormerlySerializedAs`. |
| `Scripts/Runtime/Levels/QuiverEntry.cs` | LEVEL | CORE | — |
| `Scripts/Runtime/Levels/LevelEnums.cs` | LEVEL | PO, QA | `MechanicTag`/`SolutionArchetype`/`LevelStatus`; append-only (D-070, D-075). |
| `Scripts/Runtime/Physics/PhysicsLayers.cs` | PHYS | — | Must match `TagManager.asset`. |
| `**/*.asmdef` | ARCH | — | — |
| `Packages/manifest.json`, `packages-lock.json` | ARCH | MON, PLAT (via decision) | Package rule: decision entry first. |
| `ProjectSettings/TagManager.asset` | PHYS | ARCH | Layers per D-048. |
| `ProjectSettings/DynamicsManager.asset` | PHYS | — | D-006. |
| `ProjectSettings/TimeManager.asset` | PHYS | — | Fixed Δt. |
| `ProjectSettings/ProjectSettings.asset` | PLAT | ARCH, MON | Player settings, defines, bundle ids. |
| `ProjectSettings/QualitySettings.asset`, `GraphicsSettings.asset` | PLAT | ART | — |
| `ProjectSettings/EditorBuildSettings.asset` | PLAT | ARCH | Scene list. |
| `ProjectSettings/EditorSettings.asset` | ARCH | — | Serialization / Enter Play Mode options. |
| `Assets/Settings/*RPAsset*.asset`, `*Renderer*.asset` | PLAT | ART | — |
| All 4 production scenes | see §4.1 | — | Keep scene content to one root prefab. |
| All `Prefabs/Roots/*.prefab` | see §4.1 | — | — |
| `/CLAUDE.md` | INT (checklist) / ARCH (conventions) | all (via proposal) | — |
| `/mvp.md` | OWNER | PO proposes | Edits need OWNER approval + decision (D-002). |
| `docs/planning/00` | PO | — | — |
| `docs/planning/01` | ARCH | — | Canonical names. |
| `docs/planning/02` | CORE | PO | — |
| `docs/planning/03` | PHYS (physics) / PROPS (objects) | — | — |
| `docs/planning/04`, `11` | LEVEL | PO | — |
| `docs/planning/05` | ART (art/audio) / UI (UX sections) | — | — |
| `docs/planning/06` | MON (monetisation/analytics) / SYS (meta/save) | PO | — |
| `docs/planning/07` | QA (QA) / PLAT (perf/release) | — | — |
| `docs/planning/08`, `09` | PO | INT (status) | — |
| `docs/planning/10_DECISION_LOG.md` | append-only, any role | — | Append under a lock to avoid ID collisions (§9). |
| `docs/README.md`, `docs/agents/**`, `.claude/agents/**` | INT | — | — |
| `docs/skills/**` | INT | skill-area owner proposes | — |

---

## 5. Single-owner rule and lock protocol

**Rule:** at any moment, **exactly one agent (ticket) may hold write access to a shared file.** Folder ownership (§4.1) gives default write access to non-hot files. Every 🔒 hot file (§4.2), and any file outside the role's own folders, requires a lock entry in [`FILE_LOCKS.md`](FILE_LOCKS.md).

**Protocol:**
1. **Claim.** Before editing, add a row to the *Active locks* table: file/glob, role, ticket, branch, since (ISO date-time), expected release. Commit the lock row to `main` (INT can do this on behalf of the agent) **or**, in a single-session setup, record it before spawning the agent.
2. **Check.** If the file is already locked, do **not** edit it. Options: wait; ask the holder via INT to split the change; or request INT to sequence the tickets.
3. **Scope.** Lock the narrowest path possible (a file, not a folder). Never lock `Assets/**`.
4. **Release.** On handoff, the agent lists released locks. INT removes the rows after merging (or after abandoning the branch).
5. **Expiry (D-068).** A lock older than **2 working days** without a handoff is stale. INT pings the holder; if there is no response, INT reverts the branch's claim and records it in the lock history.
6. **Scenes/prefabs:** never have two agents editing the same `.unity` or `.prefab` concurrently, even in different worktrees. YAML merges of the same asset are treated as conflicts to redo, not to hand-merge (UnityYAMLMerge is a last resort, and the merged asset must be re-opened and saved in the editor).
7. **Append-only files** (`10_DECISION_LOG.md`, `GameEnums.cs`, `LevelEnums.cs` — enum values are never renumbered, D-070/D-075): take a short lock to reserve the next ID/value, then release it in the same session.

---

## 6. Branch / worktree strategy

> Prerequisite: AB-001 (git init + LFS + UnityYAMLMerge driver, OWNER approval). Detailed commands: [`../skills/git-worktree-and-integration.md`](../skills/git-worktree-and-integration.md). Decision D-034.

| Item | Rule |
|---|---|
| Trunk | `main` is always green and only INT merges. Tag milestone gates `m1-gate`, `m3-vs`, … |
| Branch names | `<role>/<ticket>-<slug>`, e.g. `core/AB-006-ballistic-solver`, `phys/AB-012-breakable` (01 §8). Role prefix is lowercase. |
| Branch lifetime | ≤ 3 working days (D-069). Rebase on `main` before handoff. |
| Worktrees | One worktree per in-flight ticket under a sibling folder: `C:\Users\attil\ab-wt\<role>-<ticket>` (`git worktree add ../ab-wt/core-AB-006 -b core/AB-006-ballistic-solver main`). Never nest worktrees inside the project. |
| Unity `Library/` | Each worktree is a separate Unity project with its own `Library/`. The first open costs a full import, so reuse long-lived worktrees per role where possible (`ab-wt/core`, `ab-wt/phys`) and reset them to new branches. |
| Concurrency cap | **Max 2 Unity Editor instances** open at once (RAM/CPU on one PC). Additional agents run **code-only** tasks (pure C# + EditMode tests run later by INT) or docs tasks. |
| MCP routing | With 2 editors open, each agent pins its instance first: read `mcpforunity://instances`, then `set_active_instance` with the exact `Name@hash` of its worktree's editor, or pass `unity_instance` per call. An agent never sends MCP commands to an unpinned instance. |
| Preferred parallel work | Pure logic (`BallisticSolver`, `DrawModel`, `QuiverModel`, `StarRules`, `AdPolicy`, `SaveMigrator`, `EconomyService`), docs, test authoring. Scene/prefab/asset work runs **serialised** per asset. |
| Never | Two agents in the same scene or prefab; editing in the main working copy while an agent has its worktree open on the same files; committing `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, builds. |
| Claude Code isolation | Spawn editor-free agents with `isolation: "worktree"` (auto-cleaned if unchanged). For editor-dependent tasks, INT prepares the worktree, opens Unity on it, then spawns the agent with that path and instance. |
| Merge style | Squash-merge per ticket (D-069): `AB-006: BallisticSolver + parity tests`. Large LFS assets are reviewed for size. |

---

## 7. Collaboration flow

```
            ┌──────────────┐
OWNER ─────►│      PO      │  backlog item + acceptance criteria (09), scope decision (10)
            └──────┬───────┘
                   ▼
            ┌──────────────┐  feasibility, files, locks, events, packages;
            │     ARCH     │  splits L/XL; writes the ticket with TASK_TEMPLATE
            │   review     │  (or approves PO/INT-written ticket)
            └──────┬───────┘
                   ▼
            ┌──────────────┐  INT assigns role, creates branch/worktree, records locks
            │ INT dispatch │
            └──────┬───────┘
                   ▼
   ┌────────────────────────────────────────────┐
   │ Engineer role (CORE/PHYS/PROPS/SYS/UI/      │  skill: unity-feature-planning → plan bullets
   │ LEVEL/ART/PLAT/MON)                          │  implement → MCP read_console → tests
   │                                              │  → play-mode smoke → HANDOFF report
   └──────────────────┬───────────────────────────┘
                      ▼
            ┌──────────────┐  verifies acceptance criteria, runs suites,
            │      QA      │  device check if required, files bugs (loop back ↑)
            └──────┬───────┘
                   ▼
            ┌──────────────┐  integration checklist (§8), merge to main,
            │  INT merge   │  release locks, update CLAUDE.md checklist / 09 status
            └──────┬───────┘
                   ▼
            ┌──────────────┐
            │ PO accept    │  gate tickets / player-facing changes only
            └──────────────┘
```

Shortcuts: pure-docs tickets skip QA. Pure-logic tickets with full EditMode coverage may skip the QA device check before M3. Tickets touching ≥ 2 systems or any 🔒 file always need ARCH review before merge.

---

## 8. Mandatory integration checklist

INT copies this into the merge note and ticks every line. Any unticked line blocks the merge.

**Build health**
- [ ] Branch rebased on current `main`. No unrelated changes in the diff.
- [ ] Unity recompiled. **`read_console` via MCP shows 0 errors and 0 new warnings** caused by the change.
- [ ] Compiles with the Android build target active (switch-check or `BuildScript` dry run at milestones).
- [ ] EditMode tests green (`run_tests` EditMode).
- [ ] PlayMode tests green (`run_tests` PlayMode), including `LevelSolvabilityTests` and `LevelIdleStabilityTests` when levels/physics/arrows changed.
- [ ] **Full solvability run** (all levels, `07` §5.1 jitter grid, D-074) whenever the change touches `DynamicsManager.asset`, `TimeManager.asset`, any `MaterialProfile` or `ArrowDefinition` asset, `GameplayTuning.asset`, `BallisticSolver`/`ArrowImpactResolver`, or a shared structure/prop prefab (`07` §4).
- [ ] `LevelValidator` (all levels) reports 0 errors when levels, prefabs or physics changed.

**Scope & rules**
- [ ] Files changed ⊆ the role's allowed paths + locked files. Locks were held for every 🔒 file.
- [ ] No new package/SDK, or a decision entry exists (01 §2).
- [ ] No `UnityEngine.Random` in gameplay folders. No `FindObjectOfType`/`Find` in hot paths. Static state resets via `StaticReset`.
- [ ] No hard-coded tuning numbers. Values live in `GameConstants` (global) or SOs.
- [ ] No gameplay → UI/Feedback/Integrations references (01 §10.3). `ArchitectureRulesTests` green.
- [ ] Originality: new assets logged in `docs/art/ASSET_LICENSES.md`. Nothing derived from the reference game.
- [ ] Monetisation changes: `AdPolicyTests` green and the ethical rules unchanged (or OWNER-approved decision).

**Unity asset hygiene**
- [ ] Every new asset has its `.meta` committed. No orphan `.meta` files. No GUID changes on existing assets.
- [ ] No `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `*.csproj`, build output in the diff.
- [ ] YAML sanity: changed `.unity`/`.prefab`/`.asset` files open without errors. No merge markers. No accidental mass re-serialisation (diff size is plausible).
- [ ] Binary assets go through LFS (`git lfs ls-files` shows them).
- [ ] Scenes still contain only their root prefab(s) plus the scene-level camera/light where specified.

**Verification**
- [ ] Play-mode smoke via MCP: open `Gameplay.unity`, enter Play, load `LC_VerticalSlice` level 1 (or the touched level), fire, reach win/fail, exit Play, read the console again.
- [ ] Ticket-specific manual/device verification done (evidence in the handoff).
- [ ] Performance-sensitive change: perf table attached (PLAT format).

**Docs & tracking**
- [ ] Handoff report complete ([`HANDOFF_TEMPLATE.md`](HANDOFF_TEMPLATE.md)).
- [ ] Planning docs updated if behaviour or names changed. Decisions logged.
- [ ] `09_BACKLOG.md` ticket status updated. `CLAUDE.md` milestone checklist ticked if a milestone completed. `04` delivery tracker updated for levels.
- [ ] Locks released in `FILE_LOCKS.md`.

---

## 9. Decision log process

Log file: [`../planning/10_DECISION_LOG.md`](../planning/10_DECISION_LOG.md) (template at its top).

| Aspect | Rule |
|---|---|
| Who may add | Any role may **propose** (status `Proposed`). The entry's owner is the role accountable for the area. |
| When required | New package/SDK; change to a canonical name in 01; any deviation from `mvp.md`; new layer/event/enum family; save-schema change; economy/monetisation/ads change; level-format change; scope cut/add; tuning that changes a §12 target; engine upgrade. |
| Accepted by | **Technical** (architecture, physics, tooling): ARCH (or PHYS for physics settings) accepts after review. **Design/scope**: PO accepts. **Scope cuts to §14 Must-ship, monetisation/ads/IAP/pricing, privacy/consent, asset licensing, engine version: OWNER must confirm.** Until then they stay `Proposed` but act as the working default. |
| IDs | Next free `D-0xx`. Reserve it with a short lock on the file (§5.7). |
| Immutability | Accepted entries are never edited in substance. A change = a new entry with `Status: Superseded by D-0yy` set on the old one (the only allowed edit). |
| Linking | Tickets, docs and code comments (`// D-015`) reference decision IDs. The handoff lists decisions made/needed. |
| Review cadence | INT lists open `Proposed` entries in the weekly status and at every milestone gate for OWNER confirmation. |

---

## 10. Standard task format

Every delegated task uses [`TASK_TEMPLATE.md`](TASK_TEMPLATE.md). Minimum fields: ticket ID, title, role, milestone, priority, context links (mvp §, planning doc §, decisions), goal, scope in/out, files allowed to modify, files to lock, dependencies, testable acceptance criteria, required tests, validation steps (MCP console read, Play mode, device), deliverables, handoff requirements, complexity/time-box. **An agent must refuse to start a task missing acceptance criteria or allowed files.** It asks INT/PO instead (Definition of Ready in `09`).

---

## 11. RACI matrix (systems × roles)

R = Responsible (does the work) · A = Accountable (approves) · C = Consulted · I = Informed.

| System / area | PO | ARCH | CORE | PHYS | PROPS | SYS | UI | LEVEL | ART | PLAT | MON | QA | INT |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Scope, pillars, backlog priority | A/R | C | I | I | I | I | I | C | I | I | C | C | I |
| Project structure, asmdefs, packages | I | A/R | C | C | C | C | C | I | I | C | C | I | I |
| Services skeleton, events, save architecture | I | A/R | C | C | C | C | C | I | C | C | C | I | I |
| Bow input, aim, preview, arrow flight | C | C | A/R | C | I | I | C | I | I | C | I | C | I |
| Arrow types (Oak/Heavy/Split/Fire/Bounce) | A | C | R | C | C | I | I | C | C | I | I | C | I |
| Impact resolution (arrow × material) | C | C | R | A/R | C | I | I | I | I | I | I | C | I |
| Physics settings, layers, materials, breakables, debris, settle | I | C | C | A/R | C | I | I | C | C | C | I | C | I |
| Objectives, protected, kill zones | A | I | C | C | R | I | I | C | I | I | I | C | I |
| Interactive props (rope…portal) | A | I | C | C | R | I | I | C | C | I | I | C | I |
| Gameplay state machine, win/fail, stars, quiver | A | C | R | C | C | C | I | I | I | I | I | C | I |
| Level format, editor tools, validator, solvability bot | C | C | C | C | C | I | I | A/R | I | I | I | R | I |
| 60 levels content + intended solutions | A | I | I | I | I | I | I | R | C | I | I | C | I |
| Tutorial prompts & onboarding | A | I | R | I | I | I | R | R | C | I | C | C | I |
| Save/progression/economy/cosmetics/daily | A | C | I | I | I | R | C | I | C | I | C | C | I |
| HUD, menus, map, settings, accessibility UI | A | I | C | I | I | C | R | I | C | C | C | C | I |
| Art, VFX, audio, music, haptics feel | A | I | I | I | I | I | C | I | R | C | I | C | I |
| Haptics native bridge | I | C | I | I | I | I | I | I | C | R/A | I | C | I |
| Quality tiers, perf, memory, builds, device matrix | I | C | C | C | I | I | I | I | C | A/R | I | C | I |
| Ads, IAP, consent, remote config | A* | C | I | I | I | C | C | I | I | C | R | C | I |
| Analytics events + funnels | C | C | C | I | C | C | C | I | I | I | A/R | C | I |
| QA plans, regression, playtests, gates | C | I | I | I | I | I | I | C | I | C | I | A/R | I |
| Merges, locks, checklist, CLAUDE.md milestones | I | C | I | I | I | I | I | I | I | I | I | C | A/R |
| Release readiness & store submission | A* | C | I | I | I | I | C | I | C | R | R | R | C |

\* OWNER confirms monetisation, privacy and submission decisions (§9).

---

## 12. Invoking subagents in Claude Code

1. **Write the ticket** with `TASK_TEMPLATE.md` (paste it into the prompt, or link the ticket row in `docs/planning/09_BACKLOG.md` (e.g. AB-006) and include the filled template).
2. **Record locks** in `FILE_LOCKS.md` for hot files.
3. **Spawn** with the Agent tool:
   - `subagent_type`: the role's file name without `.md`, e.g. `ab-core-gameplay`.
   - `isolation: "worktree"` for code-only/docs tasks (the agent gets its own git worktree, auto-cleaned if unchanged).
   - For editor-dependent tasks: INT first creates the worktree and opens Unity on it, then states in the prompt: worktree path, Unity instance `Name@hash` to pin, and the sandbox scene to use.
4. **Prompt skeleton:**
   ```
   Act as <ROLE> per docs/agents/AGENT_SYSTEM.md §3.x.
   Ticket: <filled TASK_TEMPLATE block>.
   Use skills: docs/skills/<skill>.md.
   Worktree: <path or "isolated">; Unity instance: <Name@hash or "none – code only">.
   Locks held: <list>. Do not edit anything else.
   Finish with a HANDOFF_TEMPLATE.md report.
   ```
5. **Receive** the handoff → QA (if required) → INT runs §8 → merge.
6. **Parallelism:** at most 2 editor-bound agents plus any number of code-only/docs agents with **disjoint allowed paths**. INT checks for disjointness before spawning.

---

## 13. Escalation summary

| Trigger | Escalate to |
|---|---|
| Deviation from `mvp.md`, scope cut to Must-ship, new mechanic | PO → OWNER |
| New package/SDK, canonical name change, dependency rule break | ARCH |
| Physics stability gate failure, determinism issue on device | PHYS → ARCH + PO (D-004 fallback) |
| Ethical-ads violation request, privacy/consent question | MON → PO → OWNER |
| Asset licence or originality doubt | ART → OWNER |
| Perf budget miss > 20%, store policy change | PLAT → ARCH + PO |
| P0 bug blocking a gate, flaky tests | QA → owning role + INT |
| Lock conflict, stale lock, YAML conflict | INT |
