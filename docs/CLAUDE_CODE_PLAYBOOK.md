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
The full plan lives in `docs/README.md`. Tickets in `docs/planning/09_BACKLOG.md` follow the owner-approved execution order (D-102). AB-001 (git baseline) is done — the repo is github.com/atiwhocodes/ArrowBuster.
1. `Read docs/README.md. Then do AB-002 (project hygiene: packages, layers, physics settings, PlayMode test asmdef). Plan first, then implement and read the Console via MCP.`
2. `Do AB-003 (Services, GameEvents, StaticReset, Log, TimeScaleController). Run the EditMode tests via MCP.`
3. `Do AB-004 to AB-009 one ticket at a time: camera + play plane, tuning SOs, BallisticSolver, bow input, trajectory preview, Oak arrow.`
4. `Do AB-010 to AB-015 and AB-047: materials, impact rules, breakables, structure prefabs, the physics stability spike (gates PG-1…PG-6), the DevOverlay, BuildScript + Android dev APK. Then run the M1 feel-gate (G-M1) playtest with me on the phone.`
5. `Milestone 2: AB-016 to AB-025 — the five vertical-slice levels ONLY (level loading, quiver, objectives, settle/win/fail, ropes, HUD, analytics debug sink). No extra levels, special arrows, maps, cosmetics or ads yet.`
6. `Milestone 3: AB-026 to AB-045 + AB-158 — slice art, feedback, polished HUD/Win/Fail, Android + iOS device builds, then the 5–10 external-player playtest and the G0 feel-lock decision.`

Owner to-dos alongside: Google Play account/testing check in week 1, Mac + Apple Developer account by week 3, store accounts + IAP products by week 5.

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
- **iOS:** needs a Mac with Xcode + Apple Developer account — required by project week 3 so the vertical slice is tested on iPhone (D-100). Switch to iOS, build → open the Xcode project on the Mac → run.
