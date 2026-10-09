# Skill: Unity Gameplay Implementation

**Primary roles:** CORE, PHYS, PROPS, SYS, UI (anyone writing runtime C# in `ArrowBuster.Runtime`)

## Purpose
Implement an approved plan as compiling, tested, editor-verified C#, prefabs and ScriptableObjects that follow the project conventions (CLAUDE.md + [`01`](../planning/01_TECHNICAL_ARCHITECTURE.md) §9). The output must be mergeable by INT without rework.

## When to invoke
- After [`unity-feature-planning.md`](unity-feature-planning.md) produced an approved plan (or for an S ticket with clear scope).
- When fixing a gameplay bug that has a reproduction.

**Do NOT invoke** for level layout authoring (use [`level-design-and-validation.md`](level-design-and-validation.md)), art/audio import ([`asset-pipeline-and-imports.md`](asset-pipeline-and-imports.md)), or SDK work ([`ads-iap-and-consent-review.md`](ads-iap-and-consent-review.md)).

## Inputs
- The approved plan and ticket (`AB-###`).
- Locks held in `docs/agents/FILE_LOCKS.md` for every shared file you will touch.
- Your own branch/worktree: `<role>/AB-###-<slug>` (see [`git-worktree-and-integration.md`](git-worktree-and-integration.md)).
- The Unity Editor open on **your** worktree with the MCP server running. If several instances are connected, pin yours with `set_active_instance` (e.g. `ArrowBuster-wt-core@<hash>`).

## Step-by-step workflow
1. **Sync.** `git fetch && git rebase origin/main` (before you start, never mid-task with uncommitted YAML). Open the Editor and confirm via the `editor_state` resource that `isCompiling == false`.
2. **Write tests first for pure logic** in `Assets/_Project/Tests/EditMode/<System>Tests.cs` (namespace `ArrowBuster.Tests`). Examples: `BallisticSolverTests.Step_MatchesAnalyticArc`, `DrawModelTests.ReleaseBelowMinPower_Cancels`, `QuiverModelTests.ConsumesInAuthoredOrder`, `StarRulesTests.BonusArrowClear_CapsAtOneStar`.
3. **Implement pure classes** (no `MonoBehaviour`) in the folder from 01 §7. Rules:
   - `[SerializeField] private` + `_camelCase` fields; `[Tooltip]` on every tunable; XML doc on public APIs.
   - No `UnityEngine.Random` in Gameplay/Arrows/Physics/Objectives/Props. Use `CosmeticRandom` only for cosmetics.
   - No LINQ, `string.Format` or `new List` inside `Update`/`FixedUpdate`; use `NonAlloc` queries with preallocated buffers.
4. **Implement MonoBehaviour shells** that adapt Unity to the pure core:
   - Input in `Update` (`BowInputReader` polls `Pointer.current`).
   - Physics in `FixedUpdate` (arrow flight via `BallisticSolver.Step`, `SettleMonitor` sampling).
   - Visual interpolation in `Update`/`LateUpdate`.
   - Look up via registries (`PhysicsBodyRegistry`, `ArrowRegistry`, `ObjectiveTracker`), never `FindObjectOfType` in hot paths.
5. **Static state.** Every static field/event gets a reset in `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` (use `StaticReset`). Enter Play Mode runs **without domain reload or scene reload**.
6. **Events.** Raise `GameEvents.<Event>` with the payload struct from `Core/GameEventPayloads.cs`. Never call `AudioService`, `VfxService`, `HapticsService` or analytics from gameplay code.
7. **Data assets.** Create SOs via MCP `manage_scriptable_object` (or the `[CreateAssetMenu]` menu) in `Assets/_Project/ScriptableObjects/<Folder>/` with canonical names (`AD_Oak`, `MP_Timber`, `GameplayTuning`).
8. **Prefabs.** Use `manage_prefabs` / `manage_gameobject` / `manage_components` to build or modify prefabs in `Assets/_Project/Prefabs/<Folder>/`. Set the layer per D-048 (`PhysicsLayers`). Add `PlanarBody` to every gameplay rigidbody. Edit the prefab, not the scene instance; avoid scene overrides.
9. **Compile check.** `refresh_unity` → poll `editor_state.isCompiling` → `read_console` (types: Error, Warning). Fix **every** error and warning you caused (CLAUDE.md rule 2). Re-run until clean.
10. **Run tests.** `run_tests` (mode EditMode, then PlayMode if relevant) → `get_test_job` until complete. All green.
11. **Play-verify.** `manage_scene` open `Assets/_Project/Scenes/Gameplay.unity` → set `GameplayController._editorFallbackLevel` to the sandbox or VS level → `manage_editor` action `play` → exercise the feature (for input-driven features, ask OWNER to drag, or use the DevOverlay/test hooks) → `read_console` during play → `manage_editor` action `stop`. Play **twice in a row** to catch static-state leaks.
12. **Perf sanity** (for hot-path code): with `manage_profiler`, capture ~300 frames in play; confirm 0 B GC alloc/frame in your methods. Log the numbers in the handoff.
13. **Commit** small, logical commits on your branch: `AB-###: <imperative summary>`. Only when OWNER or INT asks (CLAUDE.md rule 6).
14. **Release locks** in `FILE_LOCKS.md` and write the handoff ([`HANDOFF_TEMPLATE.md`](../agents/HANDOFF_TEMPLATE.md)).

## Output artefacts
- Runtime code: `Assets/_Project/Scripts/Runtime/<Folder>/<Type>.cs` (+ `.meta`).
- Tests: `Assets/_Project/Tests/EditMode/<System>Tests.cs`, `Assets/_Project/Tests/PlayMode/<System>Tests.cs`.
- Assets: SOs and prefabs under canonical folders (+ `.meta`).
- Handoff report with console status, test results, play-verify notes and profiler numbers.

## Quality checklist
- [ ] Console clean (0 errors, 0 new warnings) after `refresh_unity`.
- [ ] EditMode + PlayMode tests green; new logic covered.
- [ ] Played twice in a row without errors or duplicated event handlers.
- [ ] No `UnityEngine.Random` in gameplay outcomes; no `Find*` in hot paths; no LINQ in Update/FixedUpdate.
- [ ] All tunables in SOs with tooltips; `GameConstants` only for global targets.
- [ ] Layers and `PlanarBody` set on new gameplay prefabs.
- [ ] Every new file has a `.meta`; no files under `Library/`, `Temp/` or packages touched.
- [ ] Dependency rules (01 §10.3) respected; `ArchitectureRulesTests` green.
- [ ] Locks released; handoff written.

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| Handlers fire 2×, 3× after repeated Play presses | Static events not reset (domain reload disabled) | Register a `StaticReset` in `SubsystemRegistration` |
| `NullReferenceException` on `Services.Analytics` when playing Gameplay directly | Boot didn't run | `GameBootstrap` → `ServiceInstaller.EnsureInstalled()` editor fallback |
| Arrow passes through a thin rope or plank | Dynamic rigidbody arrow or a too-thin cast | Use the kinematic sweep (D-005), `SphereCastNonAlloc` radius 0.06 m, include Rope with `QueryTriggerInteraction.Collide` |
| Preview and flight diverge | Different dt or integrator in preview | Both call `BallisticSolver.Step` with fixed dt 1/60 |
| Objects slowly drift in Z | Missing `PlanarBody` / constraints | Add `PlanarBody`; the validator flags it |
| GC spikes every shot | LINQ, `foreach` over interfaces, string concat, `GetComponents` allocs | Preallocate buffers, cache components |
| Missing script on a prefab after a merge | `.cs` renamed without its `.meta`, or `.meta` not committed | Always move scripts in the Editor or move the `.meta` too |
| Test asmdef can't see a type | Type is `internal` or in the Editor asmdef | Make it public in Runtime, or add an `InternalsVisibleTo` |

## Example task prompt for a sub-agent
```text
Agent: ab-core-gameplay
Skill: docs/skills/unity-gameplay-implementation.md
Ticket: AB-006 BallisticSolver (pure) + EditMode tests incl. preview/flight parity
Approved plan: (paste)
Branch/worktree: core/AB-006-ballistic-solver at ../ab-wt/core-AB-006
Files: Scripts/Runtime/Arrows/BallisticSolver.cs, Scripts/Runtime/Arrows/ArrowFlightState.cs, Tests/EditMode/BallisticSolverTests.cs
Shared files: none
Acceptance: Step() is semi-implicit Euler with gravity + IFlightEnvironment wind; 3 s of steps matches an independent re-simulation within 1 mm;
zero allocations (verified via the profiler/GC test); console clean; tests green.
Boundaries: no MonoBehaviours, no rendering.
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
