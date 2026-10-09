# Arrow Buster — Claude Code guide

Casual physics-puzzle archery game for iOS + Android (portrait). Pull back a bow, find the weak point,
collapse structures with a limited quiver. Design source of truth: `mvp.md` (§N refs). Plans, architecture,
agent system and backlog: start at `docs/README.md`. Read the relevant section before building a system. North star: *"I saw the trick, made one clean bow shot, and the whole scene came down."*

## Tech stack (fixed decisions)
- Unity 6.6 (6000.6.5f1), URP, C#. Single Unity project at repo root.
- 3D rigidbodies constrained to a 2.5D play plane at `z = GameConstants.PlayPlaneZ` (freeze Z position, X/Y rotation).
- Portrait 9:16 reference resolution, fixed 3/4 camera, no camera movement in MVP.
- Input: Unity **Input System** package (`Pointer.current` covers mouse + touch). Do not use legacy `UnityEngine.Input`.
  `ArrowBuster.Runtime.asmdef` already references `Unity.InputSystem`.
- Target: 60 FPS on iPhone 11-class devices; 30 FPS low-end fallback.
- No backend in v1. Local save (JSON). Ads/IAP/analytics come in milestone 8 behind interfaces in `Scripts/Runtime/Services`.
- Unity MCP (MCP for Unity) is connected: use it to read the Console, inspect/create GameObjects, prefabs,
  scenes and ScriptableObjects, and run tests. The Editor must be open with the MCP server running.

## Folder map
```
Assets/_Project/
  Scripts/Runtime/   ArrowBuster.Runtime asmdef, namespace ArrowBuster
    Core/ Gameplay/ Bow/ Arrows/ Physics/ Objectives/ Props/ Levels/ Progression/ UI/ Feedback/ Services/ Platform/
  Scripts/Integrations/ ArrowBuster.Integrations asmdef (vendor SDK adapters only, from M8)
  Scripts/Editor/    ArrowBuster.Editor asmdef (editor-only tools, level editor, setup, build)
  Tests/EditMode/    NUnit tests (run via Test Runner or MCP); add tests for pure logic (stars, quiver, save)
  Tests/PlayMode/    physics, level solvability bot, flow and perf smoke tests
  Prefabs/           Bow, Arrows, Objectives, Structures, Props, Hazards, UI, Levels/World1-3
  ScriptableObjects/ Levels/World1-3 (LevelData), Arrows, Materials, Cosmetics
  Scenes/            Boot, Home, WorldMap, Gameplay
  Art/ Audio/ UI/
mvp.md               design source of truth (docs/GDD.md is only a pointer)
docs/                README, planning/00–11, agents/, skills/, levels/, qa/
.claude/agents/      specialist subagent definitions (see docs/agents/AGENT_SYSTEM.md)
```
Full canonical folder/class/layer/event names: `docs/planning/01_TECHNICAL_ARCHITECTURE.md`.
Everything we author lives under `Assets/_Project`. Never edit `Library/`, `Temp/`, or third-party package folders.

## Conventions
- Namespace `ArrowBuster` (sub-namespaces optional, e.g. `ArrowBuster.Bow`). One public class per file, file name = class name.
- Tuning numbers: global product targets in `GameConstants`; per-system tuning in ScriptableObjects
  (`GameplayTuning`, `ArrowDefinition`, `MaterialProfile`, `EconomyConfig`, ...) — no magic numbers in systems.
- Data-driven content: levels = `LevelData` asset + layout prefab. Never hard-code a level in a scene or script.
- Prefer composition: small MonoBehaviours (`Breakable`, `Objective`, `RopeCuttable`, ...) over deep inheritance.
- Events via C# `event`/`Action`; avoid `FindObjectOfType` in hot paths; pool arrows and debris.
- Physics in `FixedUpdate`; input in `Update`. Keep outcomes deterministic enough that intended solutions always work.
  No `UnityEngine.Random` in gameplay outcomes (cosmetics only, via a seeded `CosmeticRandom`).
- Enter Play Mode runs WITHOUT domain/scene reload: every mutable static (events, registries, `Services`) must be
  reset in a `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` method.
- Arrows are kinematic swept projectiles; preview and flight share `BallisticSolver` (D-005). Layers per D-048.
- `[SerializeField] private` fields over public fields. XML doc on public APIs.
- Colors: red = required target, green/gold = helpful interactive, gray/blue = structure, purple = hazard/protected.

## Workflow rules for Claude
1. Before coding a feature, read its `mvp.md` section, its `docs/planning/` section and its ticket in
   `docs/planning/09_BACKLOG.md`, then state the plan in a few bullets.
2. After any C# change: wait for recompile, then **read the Unity Console via MCP** and fix all errors/warnings you caused.
3. Verify in the editor (enter Play mode via MCP or ask me to test on device) before marking a task done.
4. Graybox first (primitives + flat colors). No final art, monetization or polish until the arrow feel is locked.
5. Original work only: never copy the reference game's art, UI, names, structures or level layouts.
6. Update the milestone checklist below when something is done. Commit with a clear message when I ask.
7. Multi-agent work follows `docs/agents/AGENT_SYSTEM.md`: stay inside your role's allowed paths, claim shared
   files in `docs/agents/FILE_LOCKS.md`, hand off with `docs/agents/HANDOFF_TEMPLATE.md`. No new packages
   or scope changes without an entry in `docs/planning/10_DECISION_LOG.md`.

## Canonical delivery order (owner-approved, D-102 / D-104 — do not reorder)
- **Phase 1:** git/project baseline → project settings, physics layers, test assemblies, foundations → the five-level
  vertical slice ONLY → 5–10 external casual-player playtest gate.
- **Phase 2:** lock bow feel, arrow flight, preview accuracy, collision reliability, restart time, physics stability →
  reusable materials, objectives, destruction and interactive-object systems → graybox all 60 levels (prefabs + LevelData).
- **Phase 3:** validate every intended solution on target devices → final world art, UI, SFX, VFX, music, cosmetics, polish.
- **Phase 4:** consent, analytics, Crashlytics, Remote Config, ads, IAP → optimisation/QA → UK internal test, then closed
  test → Canada + Australia soft launch → optimise monetisation/balance only after real retention and funnel data.
No special arrows, maps, cosmetics, ads or extra levels before the vertical-slice gate (G0) says GO.

## Locked decisions (D-080…D-104)
- Unity 6000.6.5f1 for the whole MVP — never upgrade the editor (D-080). Public name "Arrow Buster"; technical identifiers `ArrowBuster` (D-081).
- Vendors: Firebase (Analytics, Crashlytics, Remote Config) + Unity LevelPlay + Unity IAP, only behind `IAnalyticsService`,
  `ICrashReportingService`, `IRemoteConfigService`, `IAdsService`, `IIapService`, `IConsentService`; vendor code only in
  `ArrowBuster.Integrations`, integrated in M7 (D-090). UK/EEA consent gates analytics, ad personalisation and crash reporting (D-096).
- Special arrows first appear at Heavyhead W1_L13 (L13), Split W2_L10 (L30), Fire W2_L16 (L36), Bounce W3_L07 (L47) (D-082).
- Bonus-arrow clears are capped at 1★ (D-084). Cosmetics are cosmetic-only: never aim, damage, quiver, stars or progression (D-104).
- Cut from MVP: Spring Plates, moving ice platforms, choice levels, animated splash; launch cosmetics = 3 bow skins + 2 trails (D-101, D-104).

## Milestones (docs/planning/08_PRODUCTION_ROADMAP.md — tick as completed)
- [x] 0. Project setup + AB-001 git baseline (private repo github.com/atiwhocodes/ArrowBuster, branch `main`)
- [ ] 1. Foundations & core feel: project hygiene, layers/physics settings, services skeleton, camera, bow input, BallisticSolver,
        trajectory preview, kinematic Oak arrow, materials + breakables, crate tower, DevOverlay — gate **G-M1** (PG-1…PG-6).
        STATUS: all built except DevOverlay (AB-015); device feel gate AB-014 not yet run.
        OWNER: Play account rules (week 1), Mac + Apple Developer account (week 3)
- [x] 2. Core loop + vertical-slice graybox: LevelData v2 + loader, quiver, objectives, vase, ropes, kill zones, settle/win/fail,
        stars, restart, graybox HUD, analytics debug sink — the 5 VS levels ONLY (done 2026-10-09, docs/qa/handoffs/VS-BUILD.md)
- [ ] 3. Vertical slice + external playtest + feel lock: Greenwood section art, SFX/VFX/haptics, hit-stop, tutorial prompt,
        polished HUD + win/fail, Android + iOS device builds, 5–10 external casual players — gate **G0** (VS-G1…VS-G8 + feel lock).
        STATUS: procedural Greenwood art, SFX/VFX/haptics, hit-stop, prompts, HUD/panels and an Android dev APK done; playtest pending.
        OWNER: store accounts + IAP products (week 5)
- [ ] 4. Systems complete: all materials, objectives, protected objects, 8 props, hazards, Heavyhead/Split/Fire/Bounce,
        interaction-matrix tests, level editor + validator + solvability bot v2 — gate **G1**
- [ ] 5. Content graybox + device validation: all 60 levels graybox with intended shots, bot green, every intended solution
        validated on target devices — gate **G2**
- [ ] 6. Art, audio, UI polish & meta: 3 world art kits, VFX/SFX/music, levels to Final, Home, world map, progression, coins,
        3 skins + 2 trails, Bow Forge, Bullseye, daily challenge (cut-first), settings/accessibility — gate **G3**
- [ ] 7. Platform services & monetisation: privacy policy hosted first; UMP + ATT consent; Firebase; LevelPlay rewarded +
        interstitial with AdPolicy; Unity IAP (remove_ads, starter_pack, restore) — gate **G4**
- [ ] 8. Optimisation, QA & release prep: quality tiers, device-matrix profiling, regression, playtest-driven difficulty tuning,
        accessibility, store assets, name clearance — gate **G5** Release Candidate
- [ ] 9. UK internal test, then closed test, then submission — gate **G-Release**
- [ ] 10. Soft launch in Canada + Australia; data-driven optimisation only afterwards

Next ticket: **AB-002** (docs/planning/09_BACKLOG.md), then AB-003… in dependency order.

## Build notes
- Android builds on this Windows PC (File ▸ Build Profiles ▸ Android). iOS: Unity exports an Xcode project; compile/sign on a Mac.
- Menu `Arrow Buster ▸ Setup` re-applies player settings and recreates the scene list if needed.
