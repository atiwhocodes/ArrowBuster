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

## Milestones (docs/planning/08_PRODUCTION_ROADMAP.md, D-033 — tick as completed)
- [x] 0. Project setup: URP, Android/iOS modules, portrait/IL2CPP settings, folders, asmdefs, LevelData, MCP
- [ ] 1. Graybox core feel: repo hygiene, layers/physics settings, bow drag input, trajectory preview, kinematic Oak arrow,
        materials + breakables, fixed camera, crate tower — gate: arrow feel + physics stability spike
- [ ] 2. Core loop: LevelData v2 + loader, quiver, objectives, protected vase, ropes, kill zones, settle/win/fail,
        stars, one-tap restart, graybox HUD, analytics debug sink, 10 graybox levels (incl. 5 vertical-slice levels)
- [ ] 3. Vertical slice: Greenwood section art, SFX/VFX/haptics, hit-stop, tutorial prompt, polished HUD + win/fail,
        device build, 5–10 player playtest — gate: VS go/no-go
- [ ] 4. World 1 systems + tooling: all 5 materials, Heavyhead, powder barrel, lantern/orb/dummy/fox, level editor +
        validator + solvability bot, World 1 levels 1–20 graybox
- [ ] 5. World 1 content + art lock: Greenwood kit, W1 final, world map v1, save/progression, Home, first-session flow
- [ ] 6. World 2 Sunscar (21–40): balloons, oil/fire, Fire + Split arrows, moving shields, boulders, levers
- [ ] 7. World 3 Frostspire (41–60): ice, wind, Bounce arrow, portals, moving platforms; SDK integration spike
- [ ] 8. Meta + monetisation: world map, coins, cosmetics, daily, ads/IAP/analytics/crash/remote config + consent
- [ ] 9. Device optimization, quality tiers, balance, QA, accessibility
- [ ] 10. Closed test build, store assets, submission candidate

## Build notes
- Android builds on this Windows PC (File ▸ Build Profiles ▸ Android). iOS: Unity exports an Xcode project; compile/sign on a Mac.
- Menu `Arrow Buster ▸ Setup` re-applies player settings and recreates the scene list if needed.
