# Arrow Buster — Documentation Hub

Portrait mobile physics-puzzle archery game (Unity 6000.6.5f1, URP, iOS + Android).
North star: *"I saw the trick, made one clean bow shot, and the whole scene came down exactly as I hoped."*

## 1. Where things live

| What | Where | Notes |
|---|---|---|
| **Product source of truth (MVP spec)** | [`/mvp.md`](../mvp.md) | `§N` in every doc = a section of `mvp.md`. Changes need OWNER approval plus a decision entry. `docs/GDD.md` is only a pointer (D-002). |
| Claude Code project rules | [`/CLAUDE.md`](../CLAUDE.md) | Tech stack, conventions, workflow rules, milestone checklist |
| Getting-started playbook (human) | [`CLAUDE_CODE_PLAYBOOK.md`](CLAUDE_CODE_PLAYBOOK.md) | Daily session setup, starter prompts, git setup, phone builds |
| Planning documents | [`planning/`](planning/) | Table below |
| Agent system | [`agents/AGENT_SYSTEM.md`](agents/AGENT_SYSTEM.md) | Roles, ownership, locks, integration checklist |
| Task / handoff templates | [`agents/TASK_TEMPLATE.md`](agents/TASK_TEMPLATE.md), [`agents/HANDOFF_TEMPLATE.md`](agents/HANDOFF_TEMPLATE.md) | Use for every delegated task |
| Shared-file locks | [`agents/FILE_LOCKS.md`](agents/FILE_LOCKS.md) | Claim before editing a hot file |
| Skills / playbooks | [`skills/`](skills/) | 14 step-by-step workflows (list below) |
| Subagent definitions | [`/.claude/agents/`](../.claude/agents/) | One file per role (`ab-*.md`) |
| Slash commands (to install) | [`claude-commands/`](claude-commands/) | Copy to `.claude/commands/` to enable `/next` and `/new-level` |
| Per-level intended solutions | `docs/levels/W<w>_L<nn>.md` | Created during level production (template in `04` §14) |
| QA artefacts | `docs/qa/` | Test runs, bug log, playtest notes (formats in `07`) |
| Asset licence register | `docs/art/ASSET_LICENSES.md` | Created by ART at the first third-party import (D-089) |

### Planning set

| # | Document | Answers |
|---|---|---|
| 00 | [MVP Analysis](planning/00_MVP_ANALYSIS.md) | What are we building, what's in/out, assumptions, open questions, risks, repo audit |
| 01 | [Technical Architecture](planning/01_TECHNICAL_ARCHITECTURE.md) | **Canonical names**: packages, scenes, asmdefs, folders, naming, events, data, pooling, save, builds, budgets |
| 02 | [Gameplay Systems](planning/02_GAMEPLAY_SYSTEMS.md) | State machine, bow, preview, arrows, impacts, quiver, settle/win/fail, stars, tutorial, camera, hit-stop, haptics |
| 03 | [Physics & Objects](planning/03_PHYSICS_AND_OBJECTS.md) | 2.5D model, layers, materials, breakables, objectives, every prop/hazard, determinism, interaction and test matrices |
| 04 | [Level Pipeline](planning/04_LEVEL_PIPELINE.md) | LevelData schema, authoring without code, validation, editor tools, prefab catalogue, 60-level tracker, difficulty curve |
| 05 | [Art, Audio & UX](planning/05_ART_AUDIO_UX.md) | Style and originality guardrails, asset lists, import rules, VFX/SFX/music/haptics, screens, HUD, accessibility |
| 06 | [Meta, Monetisation & Analytics](planning/06_META_MONETISATION_ANALYTICS.md) | Progression, economy, cosmetics, daily, save schema, ads/IAP rules, consent, event dictionary, KPIs, remote config |
| 07 | [QA, Performance & Release](planning/07_QA_PERFORMANCE_RELEASE.md) | Test suites, solvability, perf plan, device matrix, store checklists, closed test, soft-launch criteria, release gates |
| 08 | [Production Roadmap](planning/08_PRODUCTION_ROADMAP.md) | Milestones M0–M10 (owner execution order, D-102), critical path, compressed 10-week reference, realistic solo plan, cut list, post-MVP backlog |
| 09 | [Backlog](planning/09_BACKLOG.md) | Epics, stories, labels, DoR/DoD, **first 25 tickets AB-001…AB-025** |
| 10 | [Decision Log](planning/10_DECISION_LOG.md) | Every architecture and scope decision (D-001…). `Proposed` ones await owner confirmation. |
| 11 | [Vertical Slice](planning/11_VERTICAL_SLICE.md) | The 5-level fun proof, its tickets, asset list and go/no-go playtest |

**Precedence when documents disagree:** `mvp.md` → `10_DECISION_LOG.md` (latest accepted entry wins) → `01` (names) → the topic doc (02–09, 11). Report any disagreement to INT; don't silently pick one.

## 2. How the agent system works (summary)

- **13 roles** (PO, ARCH, CORE, PHYS, PROPS, SYS, UI, LEVEL, ART, PLAT, MON, QA + INT the Lead Integrator). Each role has a mission, allowed paths, tests and a definition of done in [`AGENT_SYSTEM.md`](agents/AGENT_SYSTEM.md), and a matching subagent in `.claude/agents/`.
- **Ownership:** every folder has one owning role. Hot shared files (`GameConstants.cs`, `GameEvents`, `LevelData.cs`, `ProjectSettings/*.asset`, `Packages/manifest.json`, scenes, root prefabs, `CLAUDE.md`) have a permanent owner. Anyone else must **lock** them in `FILE_LOCKS.md` first. Only one agent holds a lock at a time.
- **Flow:** PO defines/prioritises → ARCH reviews cross-system impact → the engineer implements on a branch → QA validates → **INT merges** after the integration checklist. Scope or architecture changes become a decision-log entry.

## 3. How to delegate a task safely

1. Pick the next ticket from [`09_BACKLOG.md`](planning/09_BACKLOG.md). Respect the dependency order and check its *Definition of Ready*.
2. Fill [`TASK_TEMPLATE.md`](agents/TASK_TEMPLATE.md): context links, **files allowed to modify**, files to lock, acceptance criteria, required tests, validation steps.
3. Claim the locks in [`FILE_LOCKS.md`](agents/FILE_LOCKS.md).
4. Launch the role's subagent (Claude Code `Agent` tool, `subagent_type` = e.g. `ab-core-gameplay`), on its own branch/worktree (§4).
5. The agent must finish with a [`HANDOFF_TEMPLATE.md`](agents/HANDOFF_TEMPLATE.md) report: files changed, tests run with results, Console state, known issues, locks to release.
6. INT runs the integration checklist (`AGENT_SYSTEM.md` §Integration checklist), merges, releases locks, ticks the milestone checklist in `CLAUDE.md`.

**Never delegate in parallel** two tasks that touch the same scene, prefab, ScriptableObject or `ProjectSettings` file. Prefer parallelising code-only tasks.

## 4. Branches and worktrees

Git is initialised (AB-001 done): private repo **github.com/atiwhocodes/ArrowBuster**, branch `main`, LFS + UnityYAMLMerge configured. Full workflow: [`skills/git-worktree-and-integration.md`](skills/git-worktree-and-integration.md).

```powershell
# one-time (AB-001)
git init; git lfs install
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"

# per task: branch name = <role>/<ticket>-<slug>
git switch -c core/AB-006-ballistic-solver                         # simple: one working copy
git worktree add ../ab-wt/core-AB-006 -b core/AB-006-ballistic-solver main   # parallel: separate folder
# opening a worktree in Unity creates its own Library/ (first import takes minutes); max 2 Unity editors at once
# with 2 editors, pin MCP per agent: set_active_instance("<Name@hash>")
git worktree remove ../ab-wt/core-AB-006                           # after INT squash-merges (D-069)
```

Rules: `main` is always green and only INT merges into it (one squash commit per ticket, branches live ≤ 3 working days — D-069). Rebase your branch on `main` before handoff. Stale locks (> 2 working days) may be force-released by INT (D-068). Never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/` or builds (already in `.gitignore`).

## 5. How to run validation

| Check | How |
|---|---|
| Compile + Console | After any C# change: wait for the recompile, then MCP `read_console` (errors + warnings). Fix everything you caused. |
| EditMode tests | Unity ▸ Window ▸ General ▸ Test Runner ▸ EditMode ▸ Run All — or MCP `run_tests` (mode EditMode) then `get_test_job` |
| PlayMode tests (physics, solvability bot, flow) | Test Runner ▸ PlayMode — or MCP `run_tests` (mode PlayMode). The test asmdef is created by AB-002. |
| Headless / CI (editor closed) | `"C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\attil\Arrow Buster" -runTests -testPlatform EditMode -testResults Builds/TestResults/editmode.xml` |
| Level validator | `Arrow Buster ▸ Levels ▸ Validate All` (`LevelValidator` v1 = AB-059, M4; until then the AB-025 solvability bot covers the VS levels). From M4 it also runs automatically before every build. |
| Play-mode smoke | MCP `manage_editor` (play) in `Gameplay.unity`. The editor fallback level loads without Boot. Fire a shot, then `read_console`, then stop. |
| Device | `Arrow Buster ▸ Build ▸ Android Dev` (from M3), then follow [`skills/qa-regression-and-device-test.md`](skills/qa-regression-and-device-test.md) |
| Performance | [`skills/mobile-performance-profiling.md`](skills/mobile-performance-profiling.md) against the budgets in `01` §11 |

## 6. Recommended implementation order (owner-approved execution order, D-102)

Read `mvp.md`, then `00` → `01` → `11` (vertical slice), then this README's §3. Owner decisions are logged as D-080…D-103 in `10_DECISION_LOG.md`.

| Step | What | Milestone / gate |
|---|---|---|
| 1 | Git baseline, Unity YAML merge, first commit — **done** (AB-001, github.com/atiwhocodes/ArrowBuster) | M0 |
| 2 | Project settings, layers, physics settings, test assemblies, folders — **next: AB-002** | M1 (AB-002 → AB-015, AB-047; gate **G-M1**, PG-1…PG-6) |
| 3 | Build the five-level vertical slice **only** | M2 (AB-016 → AB-025) + M3 (AB-026 → AB-045, AB-158) |
| 4 | 5–10 **external** casual-player tests — before special arrows, maps, cosmetics, ads or the 60 levels | M3 (AB-045) |
| 5 | Lock bow feel, trajectory accuracy, arrow collision reliability, restart time, physics stability | M3 — gate **G0** Vertical Slice & Feel Lock (VS-G1…VS-G8) |
| 6 | Reusable material and interactive-object systems (all 5 materials, 6 objectives, 8 props, 4 special arrows, level tooling) | M4 — gate **G1** Systems Complete |
| 7 | Graybox all 60 levels with reusable prefabs + LevelData (incl. AB-046) | M5 |
| 8 | Validate every intended solution on actual target devices | M5 — gate **G2** Content Graybox Complete |
| 9 | Final world art, sound, VFX, UI polish, meta and cosmetics (3 skins + 2 trails) | M6 — gate **G3** Content & Art Complete |
| 10 | Ads (LevelPlay), IAP, consent, Firebase analytics/crash/remote config — only once the core game is stable; privacy policy hosted first | M7 — gate **G4** Feature Complete |
| — | Optimisation, balance, QA, accessibility, store assets, name clearance | M8 — gate **G5** Release Candidate |
| 11 | UK closed test → submission; then Canada + Australia soft launch | M9 (gate **G-Release**) → M10 |
| 12 | Only after soft-launch data: balance, ad frequency, pricing, conversion | post-MVP |

Owner calendar items (project weeks, independent of milestones): week 1 verify the Google Play account testing requirement (D-099); week 3 Mac + Apple Developer account ready for iOS device builds (D-100); week 5 Apple/Google developer accounts + store IAP products created (D-100).

Skills index: [feature planning](skills/unity-feature-planning.md) · [gameplay implementation](skills/unity-gameplay-implementation.md) · [physics validation](skills/unity-physics-validation.md) · [performance profiling](skills/mobile-performance-profiling.md) · [level design & validation](skills/level-design-and-validation.md) · [UI/UX review](skills/ui-ux-mobile-review.md) · [asset pipeline](skills/asset-pipeline-and-imports.md) · [audio/VFX/haptics](skills/audio-vfx-haptics-integration.md) · [analytics & funnels](skills/analytics-and-funnel-review.md) · [ads/IAP/consent](skills/ads-iap-and-consent-review.md) · [QA regression & devices](skills/qa-regression-and-device-test.md) · [release readiness](skills/release-readiness.md) · [git worktrees & integration](skills/git-worktree-and-integration.md) · [balance & playtest](skills/gameplay-balance-playtest.md)
