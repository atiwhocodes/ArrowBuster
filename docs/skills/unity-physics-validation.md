# Skill: Unity Physics Validation

**Primary roles:** PHYS (owner), PROPS, CORE, LEVEL, QA

## Purpose
Prove that the physics is **stable, fair, fast to settle and repeatable enough** that every intended solution works (mvp.md §2 "Fair physics", §15 "deterministic enough"). The skill covers:
- the **M1 physics stability spike** (go/no-go for D-004),
- per-prefab/per-level stability checks,
- the **solvability bot** with aim jitter,
- interaction-matrix regression.

## When to invoke
- AB-014 (M1 stability spike + feel gate).
- After any change to `DynamicsManager.asset`, `TagManager.asset` (layers/matrix), `MaterialProfile` values, `ArrowDefinition` impulse values, `Breakable`, `ImpactDamage`, `Explosion`, `RopeCuttable` or `SettleMonitor`.
- When a new structure/prop prefab is added to the library.
- Before marking any level `Final`, and in every milestone regression run.
- When a bug report says "it fell over by itself", "it exploded", "it worked once but not again" or "the level never ends".

**Do NOT invoke** for pure visual/audio changes, UI, or economy work.

## Inputs
- Physics baseline D-006 (Δt 1/60, max Δt 0.1, solver 8/2, enhanced determinism ON, bodies start asleep, `maxDepenetrationVelocity` 3).
- Layers and matrix D-048 / `03_PHYSICS_AND_OBJECTS.md` §2.
- Budgets from 01 §11 (≤ 60 dynamic gameplay bodies, physics ≤ 3.5 ms Mid).
- `LevelData.intendedShots` (list of `IntendedShot`: arrowType, angleDeg, power01, delayAfterPrevious) recorded by `ShotRecorder`.
- Sandbox scene: `Assets/_Project/Scenes/Sandbox/Sandbox_PHYS_StackTest.unity`.

## Step-by-step workflow

### A. Settings audit (2 min, every run)
1. Read `ProjectSettings/DynamicsManager.asset` and `ProjectSettings/TimeManager.asset`. Confirm: `Fixed Timestep: 0.016666668`, `Maximum Allowed Timestep: 0.1`, `m_DefaultSolverIterations: 8`, `m_DefaultSolverVelocityIterations: 2`, `m_EnableEnhancedDeterminism: 1`, `m_BounceThreshold: 1`, `m_AutoSyncTransforms: 0`.
2. Confirm layers 6–16 exist with the D-048 names (`TagManager.asset`) and the collision matrix matches 03 §2. If not, re-run `Arrow Buster ▸ Setup ▸ Physics` / `Layers` (editor tools `PhysicsSetup`, `LayerSetup`). Never hand-edit these files without a lock.

### B. M1 stability spike (AB-014) — go/no-go gates for D-004
Build in `Sandbox_PHYS_StackTest.unity` with `manage_scene` + `manage_prefabs`:
- **S1:** a 10-crate tower (`Struct_Crate_Timber_1x1`, snapped to a 0.05 m grid, zero overlap).
- **S2:** a 3-wide × 4-high mixed stack (timber + stone).
- **S3:** a plank bridge on two supports with a crate on top.
- **S4:** a 20-body collapse scenario hit by one Oak arrow.

| Gate | Pass criterion | How to measure |
|---|---|---|
| PG-1 Idle stability | 10-crate tower + 3-crate pyramid + plank bridge idle 10 s: max drift < 1 cm, rotation < 0.5°, no body wakes | PlayMode test `LevelIdleStabilityTests` logs max displacement |
| PG-2 Spawn safety | With deliberate 0.002 m authoring overlaps, no body exceeds 0.5 m/s at spawn | Same test, velocity sampling |
| PG-3 Collapse resolve | An Oak hit on the bottom crate → calm (D-015) in < 3.0 s in 9/10 runs, < 4 s in 10/10 | `SettleMonitor` timing log |
| PG-4 Repeatability (same machine) | The same recorded shot ×20 → identical end-state hash (positions rounded to 1 cm) in ≥ 19/20 runs | Solvability bot, jitter 0 |
| PG-5 Cost | Worst collapse frame physics ≤ 5 ms on the Mid device (iPhone 11 or Galaxy A54); ≤ 3.5 ms average | `manage_profiler` in the editor + a device dev build |
| PG-6 Z drift | No gameplay body ever has \|z\| > 0.001 m or a non-zero X/Y rotation | Test asserts each FixedUpdate |

3. Run the A/B: PhysX default solver vs TGS (Temporal Gauss-Seidel) on PG-1/PG-3; record both.
4. **Decision:** all gates pass → keep D-004 and log the result in the handoff. Any gate fails after tuning (mass ratios, solver, sleep thresholds, contact offset) → escalate to ARCH + PO with data. The documented fallback is 2D physics (D-004).

### C. Prefab validation (per new or changed library prefab)
1. Validate via `PrefabValidator` (Editor) or by inspection with `manage_prefabs`/`manage_components`:
   - layer per D-048;
   - `PlanarBody` present on any Rigidbody;
   - `MaterialBody` → a `MP_*` profile;
   - mass from the profile (density × volume);
   - collider bounds equal to the base variant (world art variants must not change colliders, 01 §12).
2. Mass-ratio check: touching bodies within a prefab ≤ 10:1.
3. Drop-test: instantiate on `Environment` ground, play 3 s, confirm it sleeps.

### D. Level validation (every level; automated)
1. `LevelValidator` (`Arrow Buster ▸ Levels ▸ Validate All`): ≤ 60 dynamic bodies, no overlaps (`Physics.ComputePenetration` between bodies > 0.005 m = error), all gameplay bodies asleep at load, clear line below all objectives (D-039), protected objects outside unguarded blast radii (D-020 warning), mass ratios ≤ 10:1.
2. **Idle test:** `Tests/PlayMode/LevelIdleStabilityTests` loads each `LevelData` through `LevelLoader`, simulates 3 s with no shots, and asserts no objective cleared, no protected lost and max displacement < 1 cm.

### E. Solvability bot with aim jitter
1. `Tests/PlayMode/LevelSolvabilityTests` (parameterised over all `LevelData` in `LevelCatalog`):
   - load the level;
   - replay `intendedShots` through the real `BowController` fire path (bypassing input), waiting `delayAfterPrevious` in sim time;
   - assert `LevelWon` with `arrowsUsed ≤ goldPar`.
2. **Jitter pass:** replay each shot on the canonical tolerance grid in [`07` §5.1](../planning/07_QA_PERFORMANCE_RELEASE.md#51-tolerance-definition) (D-074: δθ = atan(0.4 m / d) ≥ 0.5°, δp ±0.03, 9 samples per shot; all win, ≥ 7/9 within par). Timing levels add ±0.1 s release jitter. Shots that fail jitter are "pixel-perfect" — redesign them (LEVEL), don't loosen the test.
3. Run on CI/editor every merge to `main` that touches physics, props, arrows or levels. Run on a **device dev build** (Android Mid + iPhone 11) at each milestone gate via the DevOverlay "Run solvability bot" cheat. PhysX is not cross-platform deterministic, so results must also be checked on-device.
4. Record failures with level ID, shot index, perturbation and outcome.

### F. Interaction matrix regression
`Tests/PlayMode/InteractionMatrixTests` runs one mini-scene per cell of the 03 §9 matrix (e.g. Fire × Rope burns in ≤ burnTime, Explosion × Protected applies no force, Split × Balloon pops 3, Bounce × Metal reflects exactly once, Debris × Objective no contact). Each cell asserts the documented outcome, including "no interaction" cells.

## Output artefacts
- `docs/qa/test-runs/YYYY-MM-DD_physics-<topic>.md` (gate table with measured numbers, solver A/B, device used).
- The M1 spike result appended to the AB-014 handoff, plus a decision-log update request if D-004/D-006 change.
- Failing-level list → bug tickets `AB-###` labelled `PHYS` or `LEVEL`, `R-H`.
- Test code: `Assets/_Project/Tests/PlayMode/LevelIdleStabilityTests.cs`, `LevelSolvabilityTests.cs`, `InteractionMatrixTests.cs`.

## Quality checklist
- [ ] Settings audit matches D-006/D-048.
- [ ] PG-1…PG-6 measured and recorded (M1), with device numbers for PG-5.
- [ ] Every library prefab passes the prefab validation.
- [ ] Every level passes the validator + idle test.
- [ ] Every level's intended solution wins at jitter 0 and meets the jitter pass rule.
- [ ] Interaction matrix tests green, including "no interaction" cells.
- [ ] No `UnityEngine.Random` in outcome code (grep).
- [ ] Device verification done at milestone gates (not only the editor).

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| Stack jitters or slides on spawn | Overlapping colliders, bodies not put to sleep, contact offset too large | Snap to the 0.05 m grid; `Rigidbody.Sleep()` on load (`LevelLoader`); validator overlap check |
| Pieces explode outward at load | Penetration resolved at high depenetration velocity | `maxDepenetrationVelocity = 3`; fix the overlaps |
| Tower collapses on its own after ~5 s | Mass ratio > 10:1 (heavy stone on light straw), or the sleep threshold too low | Rebalance masses via profiles; check ratios |
| Arrow "phases" through a plank | Sweep radius/step too small, or a dynamic arrow | Kinematic `SphereCastNonAlloc` per step (D-005); verify the cast mask |
| Same shot, different outcome on device vs editor | PhysX float differences on ARM vs x86 | Widen design tolerance; rely on the jitter pass; verify on device |
| Level never ends (no Win/Fail) | Ambient bodies (balloons, movers) counted in calm detection; jittering body | Mark ambient/kinematic in `PhysicsBodyRegistry`; caps 3 s / 5 s (D-015) |
| Bodies move in Z | `PlanarBody` missing on a prefab variant | Prefab validation; add the component to the base prefab |
| Results differ between the 1st and 2nd Play in the editor | Static registry not reset (domain reload disabled) | `StaticReset` for `PhysicsBodyRegistry` and `LevelClock` |
| Chain explosion launches the whole level | Uncapped impulse or zero-delay barrel chains | Impulse clamp; barrel chain delay 0.15 s; capped upward modifier |

## Example task prompt for a sub-agent
```text
Agent: ab-physics
Skill: docs/skills/unity-physics-validation.md (sections A + B)
Ticket: AB-014 Physics stability spike + crate-tower sandbox + M1 feel gate playtest (physics half)
Inputs: D-004, D-006, D-048; prefabs from AB-013; MaterialProfiles from AB-010
Deliverables: Scenes/Sandbox/Sandbox_PHYS_StackTest.unity with S1–S4; Tests/PlayMode/LevelIdleStabilityTests.cs;
docs/qa/test-runs/2026-10-xx_physics-m1-spike.md with the PG-1…PG-6 table, PhysX vs TGS comparison and an Android Mid device number for PG-5.
Shared files: DynamicsManager.asset (lock it before any A/B change; restore the baseline afterwards).
Escalate to ab-tech-architect + ab-product-owner if any gate fails after tuning.
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
