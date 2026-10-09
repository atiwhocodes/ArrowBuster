# Task Template (standard delegation format)

> Every delegated task uses this format. An agent must refuse to start if **Acceptance criteria** or **Files allowed to modify** are missing (Definition of Ready, `../planning/09_BACKLOG.md`).
> Roles and paths: [`AGENT_SYSTEM.md`](AGENT_SYSTEM.md). Locks: [`FILE_LOCKS.md`](FILE_LOCKS.md). Handoff: [`HANDOFF_TEMPLATE.md`](HANDOFF_TEMPLATE.md).

## Blank template

```markdown
## AB-### — <imperative title>

| Field | Value |
|---|---|
| Ticket ID | AB-### |
| Title | <imperative, ≤ 10 words> |
| Role | <PO/ARCH/CORE/PHYS/PROPS/SYS/UI/LEVEL/ART/PLAT/MON/QA/INT> (subagent: ab-<role-file>) |
| Milestone | M<n> — <name> |
| Priority | P0 must / P1 should / P2 could |
| Complexity / time-box | S (≤ 2 h) / M (≤ 1 day) / L (≤ 3 days — split if possible) / XL (must split) |
| Risk | R-H / R-M / R-L (+ risk IDs from 00, e.g. T-02) |
| Depends on | AB-###, AB-### (must be merged) |
| Blocks | AB-### |
| Branch | <role>/AB-###-<slug> |
| Worktree / Unity instance | <path or isolated> / <Name@hash or none (code-only)> |

### Context
- mvp.md: §<n> <topic>
- Planning: docs/planning/<doc>.md §<n>
- Decisions: D-0xx, D-0yy
- Skills to follow: docs/skills/<skill>.md

### Goal
<1–3 sentences: what exists after this task and why it matters to the player/project.>

### Scope
**In:**
- …
**Out (do not do):**
- …

### Files allowed to modify
- <exact paths/globs; new files listed explicitly>

### Files to lock (FILE_LOCKS.md)
- <hot files, or "none">

### Acceptance criteria (testable)
1. Given … when … then …
2. …

### Required tests
- EditMode: <TestClass.TestName …>
- PlayMode: <…>
- Manual / device: <…>

### Validation steps
1. Wait for recompile; `read_console` via MCP → 0 errors, 0 new warnings.
2. `run_tests` EditMode (+ PlayMode if listed) → green.
3. Play-mode check via MCP: <scene, steps, expected result>.
4. Device check: <device tier, steps> or "not required (pure logic)".

### Deliverables
- <code files, assets, tests, doc updates>

### Handoff requirements
- HANDOFF_TEMPLATE.md report, plus: <perf table / screenshots / tuning table / tracker rows>.
- Release locks listed above.
```

---

## Filled example — AB-006 BallisticSolver

## AB-006 — Implement pure BallisticSolver with parity tests

| Field | Value |
|---|---|
| Ticket ID | AB-006 |
| Title | Implement pure BallisticSolver with parity tests |
| Role | CORE (subagent: `ab-core-gameplay`) |
| Milestone | M1 — Graybox Core Feel |
| Priority | P0 |
| Complexity / time-box | M (≤ 1 day) |
| Risk | R-H (T-02 tunnelling, T-03 preview ≠ flight) |
| Depends on | AB-003 (Core skeleton: `LevelClock`, `StaticReset`), AB-005 (`GameplayTuning`, `ArrowDefinition`) |
| Blocks | AB-008 TrajectoryPreview, AB-009 ArrowProjectile |
| Branch | `core/AB-006-ballistic-solver` |
| Worktree / Unity instance | isolated worktree / none for coding; INT runs EditMode tests in the main editor before merge |

### Context
- mvp.md: §3 Aim and fire (ballistic path, preview while drawing, 1.5–2.5 s flight), §12 physics tuning targets.
- Planning: `docs/planning/02_GAMEPLAY_SYSTEMS.md` §Ballistic projectile simulation and §Aim assist / trajectory display; `01` §9 (pure logic, zero GC), §10.3 dependency rules.
- Decisions: D-005 (shared solver for flight + preview), D-006 (Δt 1/60), D-085 (preview shows wind, portal exits and the first bounce via the environment), D-036 (Spec vs Snappy presets).
- Skills: `docs/skills/unity-gameplay-implementation.md`, `docs/skills/unity-feature-planning.md`.

### Goal
A deterministic, allocation-free `BallisticSolver` that advances an arrow state by one fixed step, with gravity and an optional flight environment (wind sampler). Both `ArrowProjectile` (flight) and `TrajectoryPreview` will call it, so the preview always equals the real flight.

### Scope
**In:**
- `ArrowFlightState` readonly struct: `Position` (Vector3, z = PlayPlaneZ), `Velocity`, `Time`.
- `IFlightEnvironment` interface: `Vector3 SampleAcceleration(Vector3 position, float time)`. `NullFlightEnvironment` returns zero.
- `BallisticSolver.Step(in ArrowFlightState s, float dt, float gravity, IFlightEnvironment env)`: semi-implicit Euler; z is forced to `GameConstants.PlayPlaneZ`.
- `BallisticSolver.LaunchVelocity(float angleDeg, float power01, ArrowDefinition def)`: maps the aim to the initial velocity using `def.minLaunchSpeed`/`maxLaunchSpeed`.
- `BallisticSolver.Simulate(in state, dt, steps, gravity, env, Span/array buffer)` → number of points written (no allocations).
**Out:**
- Collision sweeps (AB-009), rendering dots (AB-008), portals/bounce handling (M4 — the environment hook only), any MonoBehaviour.

### Files allowed to modify
- `Assets/_Project/Scripts/Runtime/Arrows/BallisticSolver.cs` (new)
- `Assets/_Project/Scripts/Runtime/Arrows/ArrowFlightState.cs` (new)
- `Assets/_Project/Scripts/Runtime/Arrows/IFlightEnvironment.cs` (new), `NullFlightEnvironment.cs` (new)
- `Assets/_Project/Tests/EditMode/BallisticSolverTests.cs` (new)

### Files to lock (FILE_LOCKS.md)
- none (no hot files; `GameConstants.PlayPlaneZ` is read-only usage)

### Acceptance criteria (testable)
1. Given launch angle 90° and gravity g, when simulated, the apex time equals v0/g within 1 frame (1/60 s).
2. Given angle 45°, flat environment and no wind, the horizontal distance at return to launch height equals v0²·sin(2θ)/g within 2%.
3. `Simulate` over 180 steps and 180 successive `Step` calls produce positions identical to within 1e-5 m (**parity**).
4. With a constant wind environment (+2 m/s² X), the x-position deviates from the no-wind run by ½·a·t² within 2% at t = 1.5 s.
5. z of every produced state == `GameConstants.PlayPlaneZ`.
6. `Step`/`Simulate` allocate 0 bytes (verified with `GC.GetAllocatedBytesForCurrentThread` delta in the test).
7. With the `AD_Oak` "Spec" preset at power 1 and 45°, the flight time back to launch height lies within 1.5–2.5 s. With "Snappy" it lies within 0.9–1.4 s (D-036). If AB-005 values miss this, report it rather than changing the assets.
8. No `UnityEngine.Random`, no MonoBehaviour, no references outside `Core`/`Arrows`.

### Required tests
- EditMode: `BallisticSolverTests.ApexTime_Vertical`, `.Range_45Degrees`, `.SimulateMatchesStep_Parity`, `.Wind_ConstantAcceleration`, `.StaysOnPlayPlane`, `.ZeroAllocation`, `.PresetFlightTimes` (TestCase per preset).
- PlayMode: none (parity with real flight is AB-009's `ArrowPreviewParityTests`).
- Manual / device: not required.

### Validation steps
1. Wait for recompile; `read_console` → 0 errors, 0 new warnings.
2. `run_tests` mode EditMode, filter `BallisticSolverTests` → all green; then the full EditMode suite → green.
3. Play-mode check: not applicable (no runtime object).
4. Device check: not required (pure logic).

### Deliverables
- 4 runtime files + 1 test file with XML docs on public APIs.
- Short note in the handoff on integrator choice (semi-implicit Euler) and numeric tolerances.

### Handoff requirements
- `HANDOFF_TEMPLATE.md` report with the test results table.
- Note any `ArrowDefinition`/`GameplayTuning` fields AB-005 must add or rename.
- Locks: none to release.
