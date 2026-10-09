# Skill: Git Worktree and Integration

**Primary roles:** INT (owner), every agent (branch/worktree hygiene), ARCH (shared-file arbitration)

## Purpose
Let several Claude Code agents work in parallel on a Unity project **without corrupting YAML assets, fighting over shared files or breaking `main`**. Policy: D-034. `main` is always green, and only INT merges into it.

## When to invoke
- AB-001 (one-time repository setup, needs OWNER approval).
- At the start of every ticket (create a branch/worktree, claim locks) and at its end (handoff, merge).
- When a merge conflict touches `.unity`, `.prefab`, `.asset` or `.meta` files.

**Do NOT invoke** to push to a remote or commit unless OWNER/INT asked (CLAUDE.md rule 6).

## Inputs
- `.gitignore` and `.gitattributes` at the repo root (exist: LFS for binaries, `merge=unityyamlmerge` for `.unity/.prefab/.asset`).
- `docs/agents/FILE_LOCKS.md` (lock table).
- The ticket ID and role → branch name `<role>/<ticket>-<slug>` (e.g. `core/AB-006-ballistic-solver`, `level/AB-025-vs-levels`). Lowercase role prefix: `core`, `phys`, `props`, `sys`, `ui`, `level`, `art`, `plat`, `mon`, `qa`, `arch`, `int`, `po`.

## Step-by-step workflow

### A. One-time setup (AB-001 — OWNER approval required)
```bash
cd "C:/Users/attil/Arrow Buster"
git init -b main
git lfs install
# UnityYAMLMerge (Smart Merge) driver — path matches the installed editor
git config merge.unityyamlmerge.name "Unity SmartMerge (UnityYamlMerge)"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
git config core.autocrlf false        # .gitattributes controls eol
git add .gitattributes .gitignore && git commit -m "AB-001: git attributes and ignore rules"
git add -A && git status              # check: no Library/, Temp/, Logs/, UserSettings/, *.csproj staged
git commit -m "AB-001: project baseline (M0 setup + planning docs)"
git lfs ls-files                      # binaries listed
```
- Add any missing LFS patterns before the first binary arrives (`*.tif *.aif *.aiff *.mp4 *.psb *.exr *.hdr *.fbx`).
- The editor setting **Version Control Mode = Visible Meta Files** and **Asset Serialization = Force Text** are already set (verified in `EditorSettings.asset`).
- Optional remote: OWNER creates a private GitHub repo, then `git remote add origin … && git push -u origin main`.

### B. Start a task
1. Pick a mode:
   - **Code-only / docs-only** tasks (pure C#, tests, docs) → a worktree. The editor is optional; batchmode tests are possible.
   - **Editor-dependent** tasks (prefabs, scenes, MCP play-verify) → a worktree **with its own Unity editor**. **At most 2 Unity editors at once** on this PC (RAM/CPU; each needs its own `Library/`).
   - **Shared hot files** (`Gameplay.unity`, `GameplayRoot.prefab`, `TagManager.asset`, `DynamicsManager.asset`, `ProjectSettings.asset`, `Packages/manifest.json`, `GameConstants.cs`, `GameEnums.cs`, `GameEvents.cs`/`GameEventPayloads.cs`, `LevelData.cs`, `LevelEnums.cs`, `GameplayTuning.asset`, asmdefs, `CLAUDE.md`; full list in AGENT_SYSTEM.md §4.2) → claim a lock first and keep the change small.
2. Create the worktree:
   ```bash
   git fetch origin 2>/dev/null; git switch main && git pull --ff-only 2>/dev/null
   git worktree add "../ab-wt/core-AB-006" -b core/AB-006-ballistic-solver main
   ```
   The first Unity open of a new worktree re-imports everything (minutes, growing with art). To shorten it, you may copy `Library/` from the main checkout **while both editors are closed**; this is optional and only safe when both are on the same Unity version.
3. **MCP pinning:** open the worktree in Unity Hub (Add ▸ project from disk), start the MCP server in that editor (Window ▸ MCP for Unity), then in the agent session call `set_active_instance` with that instance's `Name@hash` (read the `mcpforunity://instances` resource). Every MCP call then targets your worktree, not someone else's editor.
4. **Claim locks:** add a row to `docs/agents/FILE_LOCKS.md` (file, ticket, role, branch, since). Locks idle > 2 working days may be force-released by INT (D-068). Lock rows are committed on `main` by INT, or written in the INT-owned main checkout — INT is the arbiter. Never edit a locked file you don't hold.

### C. During the task
- Commit small and often on the branch: `AB-006: add BallisticSolver.Step with wind sampling`.
- Always commit an asset together with its `.meta`. Never move/rename assets outside the Editor (or move the `.meta` too).
- Rebase **your own unpublished branch** on `main` at task start and before handoff (`git rebase main`). Never rebase shared/published branches.
- Do not reformat files you don't own (it creates noise conflicts).

### D. Handoff → integration (INT)
1. The agent writes the handoff ([`HANDOFF_TEMPLATE.md`](../agents/HANDOFF_TEMPLATE.md)) and releases its locks (or marks them "release on merge").
2. INT runs the **integration checklist** (AGENT_SYSTEM.md):
   - `git switch main && git merge --squash core/AB-006-ballistic-solver && git commit -m "AB-006: BallisticSolver + parity tests"` (one squash commit per ticket, D-069; the branch was rebased beforehand);
   - open the main checkout in Unity → `refresh_unity` → `read_console` clean → `run_tests` EditMode + PlayMode green → validator green → short play of `LC_VerticalSlice`.
3. **Merge order** when several branches are ready:
   1. ARCH/infra changes (asmdefs, packages, settings, Core);
   2. PHYS/CORE systems;
   3. PROPS/SYS;
   4. UI/Feedback;
   5. LEVEL content;
   6. ART variants.
   Re-test after each merge that touches shared files.
4. Remove finished worktrees:
   ```bash
   git worktree remove "../ab-wt/core-AB-006"
   git branch -d core/AB-006-ballistic-solver
   git worktree prune
   ```

### E. Resolving conflicts
- **C# / Markdown:** a normal text merge. Prefer the owner's version for owned files and ask the owner.
- **`.unity` / `.prefab` / `.asset`:** let UnityYAMLMerge run (`git mergetool` is not needed; the driver runs on merge). If it still conflicts:
  1. Don't hand-merge large YAML blobs.
  2. Pick one side wholesale: `git checkout --theirs -- path.prefab` (or `--ours`) **together with its `.meta`**.
  3. Re-apply the other side's change in the Editor via MCP (`manage_prefabs` / `manage_components`).
  4. Verify the prefab opens, the console is clean and tests pass.
- **`.meta` conflict:** the GUID must stay the one already on `main` (other assets reference it). Never accept a new GUID for an existing asset.
- **Recovering from a bad prefab merge already on `main`:**
  1. `git log -- Assets/_Project/Prefabs/<path>.prefab` to find the last good commit.
  2. `git restore --source <good-sha> -- <path>.prefab <path>.prefab.meta`.
  3. Re-apply lost changes via MCP.
  4. Run tests + validator.
  5. Commit `INT: restore <prefab> after bad merge (AB-###)`.
  If many files are affected: `git revert <squash-sha>` (one commit per ticket, D-069) and re-integrate the branch properly.

## Output artefacts
- Branches `<role>/<ticket>-<slug>`; worktrees under `../ab-wt/`.
- `docs/agents/FILE_LOCKS.md` updated (claim/release).
- One squash commit per ticket on `main` (`AB-###: <summary>`, D-069); an integration note in the handoff thread.

## Quality checklist
- [ ] Branch name follows `<role>/<ticket>-<slug>`; one ticket per branch.
- [ ] Locks claimed before touching shared files and released after the merge.
- [ ] ≤ 2 Unity editors running; MCP pinned to the right instance (`set_active_instance`).
- [ ] Every asset is committed with its `.meta`; no `Library/`, `Temp/`, `UserSettings/`, `*.csproj` in commits.
- [ ] Binaries in LFS (`git lfs ls-files`).
- [ ] Rebased on `main` before handoff; squash-merged by INT only (D-069); branch lived ≤ 3 working days.
- [ ] Post-merge: console clean, tests green, validator green.
- [ ] Worktree removed and pruned after the merge.

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| "Another Unity instance is running with this project" | Two editors pointing at the same folder | One editor per worktree folder |
| MCP edits land in the wrong project | Multiple instances connected, none pinned | `set_active_instance` at session start; verify with `manage_scene` get-active-scene |
| New worktree takes 10+ min to open | A fresh `Library/` import | Expected; optionally copy `Library/` while editors are closed; schedule it |
| Prefab conflict markers (`<<<<<<<`) inside YAML | The UnityYAMLMerge driver isn't configured in this clone/worktree | The `git config` commands in A (config is per-repo, shared by worktrees) |
| "Missing prefab"/broken references after a merge | `.meta` GUID changed in one branch | Restore the original `.meta` from `main`; re-link |
| Huge repo / slow clone | A binary committed without LFS | Add the pattern; `git lfs migrate import --include="*.ext" --everything` (INT, coordinate first) |
| Lock ignored, two agents edited `Gameplay.unity` | Lock protocol skipped | Keep scene content minimal (root prefab); revert one change and redo it through the lock |
| CRLF noise in YAML diffs | `autocrlf` on | `core.autocrlf false`; `.gitattributes` eol rules |

## Example task prompt for a sub-agent
```text
Agent: ab-lead-integrator
Skill: docs/skills/git-worktree-and-integration.md (section D)
Ticket: Integrate AB-006, AB-007, AB-010 into main
Inputs: handoffs for core/AB-006-ballistic-solver, core/AB-007-bow-input, phys/AB-010-material-profiles; FILE_LOCKS.md
Steps: merge order phys → core (AB-010, AB-006, AB-007); after each merge: refresh_unity, read_console, run_tests (EditMode+PlayMode);
release locks; remove worktrees. Stop on the first red result and send it back to the owning agent with the failure log.
Deliverables: one squash commit per ticket on main (only if OWNER has authorised commits), integration notes appended to each handoff.
```
