# Handoff Report Template

> Every agent ends a task with this report. INT will not merge without one. QA uses the *Test-run variant*. INT uses the *Integration note*.
> Evidence beats claims: paste console/test output excerpts, never "should work".

## Blank template

```markdown
# Handoff — AB-### <title>

**Role:** <ROLE> (acting as; subagent ab-...) · **Branch:** <branch> · **Commit(s):** <hash or "uncommitted in worktree <path>">
**Status:** Ready to merge / Ready for QA / Blocked / Partial (explain)

## 1. Summary
<3–6 bullets: what was built/changed and how it meets the goal. Map each acceptance criterion → met / not met.>

| # | Acceptance criterion | Result | Evidence |
|---|---|---|---|
| 1 | … | ✅ / ❌ / ⚠️ | test name / screenshot / log line |

## 2. Files changed
| Path | Change (new/modified/deleted) | Notes |
|---|---|---|

## 3. Decisions
- **Made (within authority):** …
- **Needed (proposed D-0xx text or question for PO/ARCH/OWNER):** …

## 4. Tests run + results
| Suite | Filter | Passed | Failed | Skipped | Notes |
|---|---|---|---|---|---|
| EditMode | … | | | | |
| PlayMode | … | | | | |
| Manual / device | … | | | | device, build id |

## 5. Unity Console state
- `read_console` after final recompile: <N errors, N warnings> (paste the relevant lines or "clean").
- Warnings pre-existing / not caused by this change: …

## 6. Known issues / limitations
- …

## 7. Evidence
- Screenshots / GIFs / profiler captures / perf table / tuning table (role-specific extras from the ticket).

## 8. Follow-ups
- New tickets suggested (title, role, reason).

## 9. Locks released
- <file — lock row removed / "none held">
```

---

## Test-run variant (QA)

```markdown
# Test run — <build id / branch> — <date>
Scope: <tickets / milestone gate> · Devices: <list> · Runner: QA
| Area | Cases | Pass | Fail | Blocked | Bug IDs |
|---|---|---|---|---|---|
Gate verdict: PASS / FAIL (reason) · Recommendation: …
```

## Integration note (INT, pasted into the squash-merge description)

```markdown
AB-### <title> — merged by INT
Checklist (AGENT_SYSTEM.md §8): all ticked / exceptions: …
Post-merge: EditMode N/N, PlayMode N/N, console clean, smoke: W1_L01 win ✔
Locks released: … · Decisions referenced: D-…
```

---

## Filled example

# Handoff — AB-006 Implement pure BallisticSolver with parity tests

**Role:** CORE (subagent `ab-core-gameplay`) · **Branch:** `core/AB-006-ballistic-solver` · **Commit(s):** `a1b2c3d`
**Status:** Ready to merge (no QA device check required: pure logic)

## 1. Summary
- Added an allocation-free `BallisticSolver` (semi-implicit Euler) with `Step`, `Simulate` and `LaunchVelocity`.
- Added `ArrowFlightState` and the `IFlightEnvironment` hook (`NullFlightEnvironment` default) for future wind/portal support (D-017).
- Positions are clamped to `GameConstants.PlayPlaneZ`.

| # | Acceptance criterion | Result | Evidence |
|---|---|---|---|
| 1 | Apex time v0/g ± 1 frame | ✅ | `ApexTime_Vertical` |
| 2 | 45° range within 2% | ✅ | `Range_45Degrees` (error 0.6%) |
| 3 | Simulate == Step parity 1e-5 m | ✅ | `SimulateMatchesStep_Parity` |
| 4 | Wind deviation within 2% | ✅ | `Wind_ConstantAcceleration` |
| 5 | z == PlayPlaneZ | ✅ | `StaysOnPlayPlane` |
| 6 | 0 B allocation | ✅ | `ZeroAllocation` |
| 7 | Preset flight times | ⚠️ | Spec 1.62 s ✅; Snappy 1.47 s ❌ (target 0.9–1.4 s) — see §3 |
| 8 | No Random / MonoBehaviour / foreign refs | ✅ | code review + `ArchitectureRulesTests` |

## 2. Files changed
| Path | Change | Notes |
|---|---|---|
| `Scripts/Runtime/Arrows/BallisticSolver.cs` | new | static class, XML docs |
| `Scripts/Runtime/Arrows/ArrowFlightState.cs` | new | readonly struct |
| `Scripts/Runtime/Arrows/IFlightEnvironment.cs` | new | — |
| `Scripts/Runtime/Arrows/NullFlightEnvironment.cs` | new | singleton instance |
| `Tests/EditMode/BallisticSolverTests.cs` | new | 7 tests, 9 cases |

## 3. Decisions
- **Made:** semi-implicit Euler (velocity first, then position) to match PhysX's integration order. Same dt as `Time.fixedDeltaTime`.
- **Needed:** the `AD_Oak` "Snappy" preset (AB-005) needs `maxLaunchSpeed` ≈ 21 → 24 m/s or gravity scale 1.3 to reach ≤ 1.4 s. → CORE follow-up on AB-005 values; the PO decides at the M1 feel gate (D-036).

## 4. Tests run + results
| Suite | Filter | Passed | Failed | Skipped | Notes |
|---|---|---|---|---|---|
| EditMode | BallisticSolverTests | 8 | 1 | 0 | `PresetFlightTimes(Snappy)` — asset value issue, not solver |
| EditMode | all | 12 | 1 | 0 | same failure |
| PlayMode | — | — | — | — | n/a |

## 5. Unity Console state
- `read_console` after the final recompile: 0 errors, 0 warnings.

## 6. Known issues / limitations
- The Snappy preset test fails until the AB-005 values are adjusted (marked `[Category("Tuning")]` so INT can exclude it if the PO wants to merge first).

## 7. Evidence
- Test runner output excerpt attached. No screenshots (pure logic).

## 8. Follow-ups
- AB-005b "Retune Snappy preset" (CORE, S).

## 9. Locks released
- none held
