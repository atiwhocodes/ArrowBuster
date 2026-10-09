# Skill: QA Regression and Device Test

**Primary roles:** QA (owner), INT (pre-merge gate), PLAT (device builds), every engineer (own-system tests)

## Purpose
Catch regressions before they reach `main` and before every milestone gate. The skill runs:
- the automated suites (EditMode, PlayMode, solvability bot, idle stability, interaction matrix, flow smoke);
- manual checklists;
- a device-matrix pass.

It produces a written test run with a clear go/no-go. It also owns bug triage.

## When to invoke
- **Pre-merge (INT):** the automated subset on every branch before merging to `main`.
- **Milestone gate:** the full regression at every formal gate (G-M1, G0, G1, G2, G3, G4, G5, G-Release — `07` §16, D-102).
- **Release candidates:** the full regression + device matrix + monetisation/consent checks.
- **Bug reports:** reproduce → triage → ticket.

**Do NOT invoke** for exploratory fun/balance playtests (use [`gameplay-balance-playtest.md`](gameplay-balance-playtest.md)) or for perf deep dives (use [`mobile-performance-profiling.md`](mobile-performance-profiling.md)).

## Inputs
- Test plans and device matrix in [`07_QA_PERFORMANCE_RELEASE.md`](../planning/07_QA_PERFORMANCE_RELEASE.md).
- The build under test: a branch SHA, or a device build with its version (`0.<milestone>.<patch>`, Android `bundleVersionCode`).
- Previous test run (for diffing) in `docs/qa/test-runs/`.
- Bug log `docs/qa/BUGS.md` (or the issue tracker, if adopted).

## Step-by-step workflow

### A. Automated suites
1. Editor (agent): `refresh_unity` → `read_console` must show 0 errors.
2. Run `run_tests` mode EditMode → `get_test_job` until done; then mode PlayMode (includes `LevelSolvabilityTests`, `LevelIdleStabilityTests`, `InteractionMatrixTests`, `ArrowPreviewParityTests`, `RestartPerfTests`, `GameFlowSmokeTests`).
3. CLI / CI equivalent (Windows):
   ```
   "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe" -batchmode -nographics -projectPath "C:\Users\attil\Arrow Buster" -runTests -testPlatform EditMode -testResults Builds\TestResults\editmode.xml -logFile Builds\TestResults\editmode.log
   ```
   Repeat with `-testPlatform PlayMode`. (Close the interactive editor for that project first — Unity can't open the same project twice.)
4. Run `Arrow Buster ▸ Levels ▸ Validate All` → 0 errors.
5. Record pass/fail counts and the durations of the slowest tests.

### B. Manual core regression (editor, ~20 min; device at milestone gates)
Play the `LC_VerticalSlice` playlist + 1 level per mechanic introduced so far:
- [ ] Draw starts anywhere in the aim zone; release below min power cancels with no arrow spent; the cone is clamped.
- [ ] The preview matches the flight; it ends at the first hit with a marker; it passes through ropes/portals; it reflects for Bounce.
- [ ] The arrow embeds in timber/straw, deflects off stone, ricochets off metal at shallow angles; the lodged vibrate is 0.2 s; at most 8 active arrows (oldest becomes decor).
- [ ] Rope cut drops its load; kill zones remove objects; objectives in water count as cleared; protected objects in water fail.
- [ ] Win after a 0.75 s calm; out-of-arrows fail after 2 s calm or 5 s max; the soft-lock toast appears with arrows left.
- [ ] Protected loss → immediate fail with a reason message.
- [ ] Stars: par → 3★, par+1 → 2★, otherwise 1★; a bonus-arrow clear → 1★.
- [ ] Restart is one tap and < 1 s; Next is one tap; pause/resume works; Android back = pause.
- [ ] The second editor Play shows no duplicate events, sounds or analytics (static reset).
- [ ] Settings toggles persist across an app restart.

### C. Device matrix pass (milestone gates, RCs)
1. Install the build: `adb install -r <apk>` / TestFlight.
2. On each device in the 07 matrix (minimum: iPhone 11, iPhone SE, notch/Dynamic Island iPhone, iPad, Low Android, Mid Android, punch-hole Android), run:
   - the B checklist (short form);
   - safe-area screenshots;
   - FPS via the DevOverlay on L1 + the heaviest level;
   - the lifecycle checks below.
3. Lifecycle:
   - background/foreground during flight, during the Win panel and during an ad;
   - incoming call / Control Centre;
   - low-battery mode;
   - kill the app mid-level → relaunch → progress intact, no corrupt save;
   - airplane mode from a cold start;
   - storage nearly full (the save write fails gracefully and keeps the `.bak`).
4. Logs: `adb logcat -v time -s Unity AndroidRuntime DEBUG > docs/qa/test-runs/logs/<date>_<device>.log`; Xcode console for iOS. Search for `Exception`, `NullReference`, `ANR`, `SIGABRT`.

### D. Triage
For each defect, add a row to `docs/qa/BUGS.md` (ID `BUG-###`):

| Field | Content |
|---|---|
| Title | Short description |
| Build/SHA, device/OS | Where it was seen |
| Steps | Numbered repro steps |
| Expected / Actual | — |
| Repro rate | e.g. 3/5 |
| Severity | S1 crash, data loss or blocker · S2 a core loop or level unsolvable · S3 functional with a workaround · S4 cosmetic |
| Owner role | Role ID |
| Linked ticket | `AB-###` |

S1/S2 block merges and gates. A physics flake (non-repeatable) → run the solvability bot ×10 on that level and attach the results.

### E. Report
Write the test run. A gate is **GO** only if:
- all automated tests are green;
- there are 0 open S1/S2;
- the B checklist passes on at least 1 Mid + 1 Low device;
- no unresolved perf budget breach marked "gate-blocking" by PLAT.

## Output artefacts
- `docs/qa/test-runs/YYYY-MM-DD_<milestone|branch>_regression.md` (build, suites + counts, checklist results, device table, bugs found, GO/NO-GO)
- `docs/qa/BUGS.md` rows; `AB-###` tickets for fixes
- Logs in `docs/qa/test-runs/logs/` (git-ignored if large; reference paths)

## Quality checklist
- [ ] Console clean; EditMode + PlayMode green; validator 0 errors.
- [ ] Manual checklist B run on the stated build; any skipped items explicitly marked "not run".
- [ ] Device matrix covered for gates/RCs, including lifecycle and offline checks.
- [ ] Every defect has repro steps, severity, an owner and a build ID.
- [ ] GO/NO-GO stated with reasons; never "should work".

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| Tests pass in the editor UI but fail in batchmode | Tests depend on an open scene or editor state | Tests load their own scenes/levels; no reliance on the selection |
| PlayMode tests pass alone, fail together | Static state leaking between tests | `StaticReset` + `[UnitySetUp]` cleanup; reload the scene per fixture |
| "Project is already open" in CLI runs | The interactive editor holds the project lock | Close it, or run in a separate worktree |
| Device-only crash in release | IL2CPP stripping / AOT generic missing | `link.xml`; test a release-config build before the RC |
| Save lost after a force-kill | Write not atomic | `save.tmp` + `File.Replace` + `.bak` (01 §14) |
| Flaky solvability on one level | Pixel-perfect shot or a jittering body | Jitter runs; send to LEVEL/PHYS with data |
| Bug can't be reproduced | Missing build/SHA or attempt context | Require build ID + DevOverlay screenshot in the report |

## Example task prompt for a sub-agent
```text
Agent: ab-qa-playtest
Skill: docs/skills/qa-regression-and-device-test.md (A–E)
Ticket: AB-044 VS regression checklist + QA pass (G0 Vertical Slice & Feel Lock gate)
Build: main@<sha>, Android-Dev 0.3.x (APK path), iOS TestFlight 0.3.x
Devices: Pixel 6a, Galaxy A13, iPhone 11, iPhone 15 (Dynamic Island)
Deliverables: docs/qa/test-runs/2026-10-xx_M3_regression.md with GO/NO-GO; BUGS.md rows for every defect; tickets for S1/S2.
Boundaries: do not fix code; file tickets to the owning roles.
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
