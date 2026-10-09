# File Locks

> Single-owner rule: at any moment exactly one ticket may write to a shared file. Full protocol: [`AGENT_SYSTEM.md` §5](AGENT_SYSTEM.md#5-single-owner-rule-and-lock-protocol). Ownership map: [`AGENT_SYSTEM.md` §4](AGENT_SYSTEM.md#4-file-ownership-map).

## Protocol (short form)
1. **Claim:** add a row to *Active locks* **before** editing (narrowest path; never `Assets/**`).
2. **Check:** if a matching row exists, do not edit. Wait, or ask INT to sequence or split.
3. **Release:** list the released locks in your handoff. INT deletes the row after merge/abandon and appends it to *Lock history*.
4. **Stale:** a lock older than 2 working days without a handoff → INT pings, then force-releases (noted in history).
5. **Scenes/prefabs:** one agent per `.unity`/`.prefab` at a time, across all worktrees.
6. **Append-only files** (decision log, enums): take a short lock to reserve the next ID, then release it in the same session.
7. Times in ISO 8601 local time (`2026-10-09T14:30`).

## Active locks

| File / glob | Owner role | Ticket | Branch | Since | Expected release |
|---|---|---|---|---|---|
| — | — | — | — | — | — |

## Permanent owners of hot files
Editing these **always** requires an active lock row, even by the permanent owner, whenever another ticket is in flight.

| File | Permanent owner | May be locked by |
|---|---|---|
| `Assets/_Project/Scripts/Runtime/Core/GameConstants.cs` | ARCH | CORE, PHYS |
| `Assets/_Project/Scripts/Runtime/Core/GameEnums.cs` | ARCH | CORE, PROPS, SYS |
| `Assets/_Project/Scripts/Runtime/Core/GameEvents.cs` | ARCH | CORE, PROPS, PHYS |
| `Assets/_Project/Scripts/Runtime/Core/GameEventPayloads.cs` | ARCH | CORE, PROPS, PHYS |
| `Assets/_Project/Scripts/Runtime/Levels/LevelData.cs` | LEVEL | CORE, SYS |
| `Assets/_Project/Scripts/Runtime/Levels/QuiverEntry.cs` | LEVEL | CORE |
| `Assets/_Project/Scripts/Runtime/Physics/PhysicsLayers.cs` | PHYS | — |
| `Assets/_Project/Scripts/Runtime/Services/I*.cs` | ARCH | MON |
| `**/*.asmdef` | ARCH | — |
| `Packages/manifest.json`, `Packages/packages-lock.json` | ARCH | MON, PLAT (with a decision entry) |
| `ProjectSettings/TagManager.asset` | PHYS | ARCH |
| `ProjectSettings/DynamicsManager.asset` | PHYS | — |
| `ProjectSettings/TimeManager.asset` | PHYS | — |
| `ProjectSettings/ProjectSettings.asset` | PLAT | ARCH, MON |
| `ProjectSettings/QualitySettings.asset` | PLAT | ART |
| `ProjectSettings/GraphicsSettings.asset` | PLAT | ART |
| `ProjectSettings/EditorBuildSettings.asset` | PLAT | ARCH |
| `ProjectSettings/EditorSettings.asset` | ARCH | — |
| `Assets/Settings/*RPAsset*.asset`, `Assets/Settings/*Renderer*.asset` | PLAT | ART |
| `Assets/_Project/Scenes/Boot.unity` | ARCH | — |
| `Assets/_Project/Scenes/Home.unity` | UI | — |
| `Assets/_Project/Scenes/WorldMap.unity` | UI | — |
| `Assets/_Project/Scenes/Gameplay.unity` | CORE | — |
| `Assets/_Project/Prefabs/Roots/AppRoot.prefab` | ARCH | MON, ART |
| `Assets/_Project/Prefabs/Roots/GameplayRoot.prefab` | CORE | UI, ART |
| `Assets/_Project/Prefabs/Roots/HomeRoot.prefab`, `WorldMapRoot.prefab` | UI | ART |
| `Assets/_Project/ScriptableObjects/Config/GameplayTuning.asset` | CORE | PO |
| `Assets/_Project/ScriptableObjects/Config/EconomyConfig.asset` | SYS | PO |
| `Assets/_Project/ScriptableObjects/Config/RemoteConfigDefaults.asset` | MON | SYS |
| `Assets/_Project/Scripts/Editor/Levels/LevelValidator.cs` | LEVEL | PHYS |
| `CLAUDE.md` | INT (milestones) / ARCH (conventions) | all via proposal |
| `mvp.md` | OWNER | PO (proposal only, D-002) |
| `docs/planning/10_DECISION_LOG.md` | append-only | any role (short ID-reservation lock) |
| `docs/planning/09_BACKLOG.md` | PO | INT (status column) |

## Lock history

| File / glob | Role | Ticket | Since | Released | Note |
|---|---|---|---|---|---|
| — | — | — | — | — | — |
