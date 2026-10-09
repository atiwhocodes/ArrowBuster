# Skill: Unity Feature Planning

**Primary roles:** PO, ARCH, CORE, PHYS, PROPS, SYS, UI, LEVEL (anyone starting a ticket bigger than S)

## Purpose
Turn a ticket (`AB-###`) into a short, reviewable implementation plan **before any code or asset is touched**. The plan must name exact files, classes, prefabs, ScriptableObjects, tests and acceptance checks, using the canonical names in [`01_TECHNICAL_ARCHITECTURE.md`](../planning/01_TECHNICAL_ARCHITECTURE.md). This enforces CLAUDE.md rule 1 ("read its section and state the plan in a few bullets") and stops agents from inventing parallel systems.

## When to invoke
- At the start of every ticket rated **M, L or XL**, and of any S ticket that touches a shared file (see [`FILE_LOCKS.md`](../agents/FILE_LOCKS.md)).
- When a ticket's scope is unclear or it touches two or more role-owned folders.
- When a playtest or bug suggests a design change (plan the change, don't hot-patch it).

**Do NOT invoke** for typo fixes, tuning one value in an existing SO, or re-running tests. Use [`unity-gameplay-implementation.md`](unity-gameplay-implementation.md) directly when a plan already exists and has been approved.

## Inputs
- The ticket from [`09_BACKLOG.md`](../planning/09_BACKLOG.md), filled into [`TASK_TEMPLATE.md`](../agents/TASK_TEMPLATE.md).
- The relevant `mvp.md` § and planning doc section (`02` gameplay, `03` physics/objects, `04` levels, `05` art/UX, `06` meta/monetisation, `07` QA).
- Applicable decisions in [`10_DECISION_LOG.md`](../planning/10_DECISION_LOG.md) (search for the feature's keywords).
- The current repo state: `git log --oneline -20` and the files in the target folder.
- `docs/agents/FILE_LOCKS.md` (who currently owns which shared file).

## Step-by-step workflow
1. **Read the spec slice.** Open `mvp.md` at the referenced §, then the planning-doc section. Write down 3–6 acceptance criteria in your own words. If they conflict with a decision, the decision wins; if they conflict with `mvp.md` and no decision exists, **stop and escalate to PO**.
2. **Inventory existing code.** Use `Grep`/`Glob` on `Assets/_Project/Scripts/Runtime/**` for the class names you expect (e.g. `ArrowProjectile`, `SettleMonitor`). Never create a second class with an overlapping job. For live editor state, call `find_gameobjects` / `manage_scene` (action `get_hierarchy`) on `Gameplay.unity` via MCP.
3. **Map to canonical names.** For each new type, pick its folder from 01 §7 and its name from 01 (or propose a new name and flag it as an ARCH review item). For each asset, apply 01 §8 naming (`Struct_…`, `Obj_…`, `AD_…`, `MP_…`, `SE_…`).
4. **Decide the data split.** Tuning numbers go in a ScriptableObject (`GameplayTuning`, `ArrowDefinition`, `MaterialProfile`, `EconomyConfig`…). Only global product targets go in `GameConstants`. Note every new serialized field with its default and tooltip text.
5. **Decide events.** Will the feature raise or consume a `GameEvents` entry from 01 §10.2? A **new** `GameEvents` entry requires ARCH review and an update to 01 §10.2 (D-030). Prefer local C# component events.
6. **Check dependency rules** (01 §10.3). Gameplay/Physics/Arrows/Props must never reference UI, Feedback or Integrations.
7. **List shared-file touches.** Typical candidates: `GameConstants.cs`, `GameEnums.cs`, `GameEvents.cs`, `TagManager.asset`, `DynamicsManager.asset`, `ProjectSettings.asset`, `Packages/manifest.json`, `Gameplay.unity`, `GameplayRoot.prefab`, `LevelData.cs`, asmdefs, `CLAUDE.md`. Claim them in `FILE_LOCKS.md` **before** starting.
8. **Design the tests first.** Pure logic → EditMode test in `Tests/EditMode/<System>Tests.cs`. Physics, flow or level behaviour → PlayMode test in `Tests/PlayMode/`. Name each test method.
9. **Define the verification run.** What you will do via MCP: `refresh_unity` → `read_console` → `run_tests` → `manage_editor` play in `Gameplay.unity` with `_editorFallbackLevel` = a sandbox level → observe → `manage_editor` stop.
10. **Estimate and slice.** If the plan exceeds about 1 day of agent work (≈ an L), split it into sub-tickets that each produce a compiling, tested increment.
11. **Write the plan** (template below) into the ticket's handoff thread and wait for approval (the human OWNER, or INT for routine tickets).

### Plan template (paste into the task thread)
```markdown
## Plan — AB-### <title>
Spec: mvp.md §x.y · docs/planning/0N §z · decisions D-0xx
Acceptance (restated): 1… 2… 3…
New/changed files:
- Scripts/Runtime/<Folder>/<Type>.cs — <responsibility>
- ScriptableObjects/<Folder>/<Asset>.asset — <fields + defaults>
- Prefabs/<Folder>/<Prefab>.prefab — <components>
Shared files to lock: …
Events raised/consumed: …
Tests: Tests/EditMode/<X>Tests.cs::<Method> …; Tests/PlayMode/<Y>Tests.cs::<Method> …
Verification: <MCP steps + what I expect to see>
Out of scope: …
Risks/unknowns: …
Estimate: S/M/L
```

## Output artefacts
- The plan, inside the task handoff (see [`HANDOFF_TEMPLATE.md`](../agents/HANDOFF_TEMPLATE.md)).
- Lock rows added to `docs/agents/FILE_LOCKS.md`.
- If a new decision is needed: a `Proposed` entry drafted for `docs/planning/10_DECISION_LOG.md` (ARCH/PO commits it).

## Quality checklist
- [ ] Every new type has a folder and name that match 01 §7/§8.
- [ ] No magic numbers; each tunable has a named SO field and a default.
- [ ] No duplicate of an existing system (searched first).
- [ ] Tests are named, and pure logic is separated from MonoBehaviours.
- [ ] Shared files are listed and locked.
- [ ] Dependency rules in 01 §10.3 are respected.
- [ ] The plan says what "done" looks like in the editor (an observable behaviour).
- [ ] Out-of-scope items are listed so the next agent doesn't assume them.
- [ ] Estimate ≤ L, or the ticket is split.

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| Two classes do the same job (`ArrowManager` and `ArrowSpawner`) | Skipped the inventory step | Grep first; reuse the canonical names from 01 |
| Plan says "tune later", then numbers are hard-coded | No data split decided | Name the SO field in the plan |
| Agent edits `Gameplay.unity` while another agent does too | Shared file not locked | Lock in `FILE_LOCKS.md`; prefer editing `GameplayRoot.prefab` |
| Feature works only when Boot ran first | Forgot the editor play-from-any-scene rule | Plan `ServiceInstaller.EnsureInstalled()` usage + `_editorFallbackLevel` |
| Static event fires twice on the second Play | Enter Play Mode with domain reload disabled (EditorSettings options = 3) | Plan a `StaticReset` registration for every static |
| Scope creep into polish | Acceptance criteria not restated | List "Out of scope" explicitly |

## Example task prompt for a sub-agent
```text
Agent: ab-core-gameplay
Skill: docs/skills/unity-feature-planning.md (plan only — do not write code)
Ticket: AB-019 SettleMonitor + GameplayController state machine (win/fail/soft-lock)
Spec: mvp.md §3 "Win and fail"; docs/planning/02_GAMEPLAY_SYSTEMS.md (state machine, settle); D-015, D-083
Inputs: Scripts/Runtime/Core/GameConstants.cs, Scripts/Runtime/Physics (PhysicsBodyRegistry if present)
Deliverable: a plan in the Plan template format covering GameplayState enum, SettleMonitor (pure core + MonoBehaviour shell),
GameplayController transitions, new GameConstants (OutOfArrowsMaxWaitSeconds, WinSettleMaxSeconds), EditMode tests with a fake clock.
Shared files to lock: GameConstants.cs, GameEvents.cs.
Boundaries: no UI code; raise GameEvents only.
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
