# Building Arrow Buster with Claude Code

## Every session
1. Open **Unity Hub ▸ Arrow Buster** and wait for the editor.
2. In Unity: **Window ▸ MCP for Unity** → make sure it says *Session Active*. If not, click **Start Server**.
3. Open PowerShell (or VS Code terminal) in the project folder and start Claude Code:
   ```
   cd "$HOME\Arrow Buster"
   claude
   ```
4. Check the Unity link: type `/mcp` — the Unity server should show as connected.

## How to ask
- Work one milestone (see `CLAUDE.md`) at a time, one feature per request.
- Ask for a plan first on anything big: *"Plan milestone 1 step by step, don't code yet."*
- Then: *"Implement step 1, check the Unity console, and tell me how to test it."*
- Test in Unity's Game view set to a portrait resolution (e.g. 1080x1920), then on a real phone.
- When it works: *"Commit this with a clear message."* (run `git init` once first — see below).
- If something feels wrong, describe the *feel*: "arrow is too floaty", "tower falls too easily".

## Starter prompts (in order)
The full plan lives in `docs/README.md`. Tickets AB-001…AB-025 in `docs/planning/09_BACKLOG.md` are the first 25 tasks in dependency order.
1. `Read docs/README.md. Then do AB-001 (git baseline) — show me the commands first.`
2. `Do AB-002 (project hygiene: packages, layers, physics settings, PlayMode test asmdef). Plan first, then implement and read the Console via MCP.`
3. `Do AB-003 (Services, GameEvents, StaticReset, Log). Run the EditMode tests via MCP.`
4. `Do AB-004 to AB-009 one ticket at a time: camera + play plane, tuning SOs, BallisticSolver, bow input, trajectory preview, Oak arrow.`
5. `Do AB-010 to AB-015 and AB-047: materials, impact rules, breakables, structure prefabs, the physics stability spike (gates PG-1…PG-6), the DevOverlay, BuildScript + Android dev APK. Then run the M1 feel-gate (G-M1) playtest with me on the phone.`
6. `Milestone 2: continue with AB-016 to AB-025 and AB-046 (level loading, quiver, objectives, settle/win/fail, ropes, HUD, analytics, vertical-slice levels + 5 more graybox levels).`

Delegating to specialist subagents: see `docs/agents/AGENT_SYSTEM.md` (roles live in `.claude/agents/`).

Handy slash commands: `/next` (pick up the next ticket) and `/new-level` (author a level from the tracker).
They ship in `docs/claude-commands/`. In your first session ask Claude: *"Copy docs/claude-commands/*.md into .claude/commands/"*, then restart `claude`.

## One-time Git setup (recommended)
```
cd "$HOME\Arrow Buster"
git init
git lfs install
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
git add .
git commit -m "Project setup"
```
Then create a private GitHub repo and push. Commit after every working feature so you can roll back.

## Building to a phone
- **Android:** enable Developer options + USB debugging on the phone, plug in, **File ▸ Build Profiles ▸ Android ▸ Build And Run**.
- **iOS:** needs a Mac with Xcode + Apple Developer account. Switch to iOS, build → open the Xcode project on the Mac → run.
