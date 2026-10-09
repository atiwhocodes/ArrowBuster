# Skill: Mobile Performance Profiling

**Primary roles:** PLAT (owner), PHYS, CORE, ART, QA

## Purpose
Measure and fix CPU, GPU, memory, GC, load-time and battery/thermal costs **on real devices**, against the budgets in [`01`](../planning/01_TECHNICAL_ARCHITECTURE.md) §11. Targets: 60 FPS on Mid (iPhone 11), 30 FPS on Low, restart ≤ 300 ms Mid (hard limit 1 s, mvp §12).

## When to invoke
- Milestone gates G-M1 (PG-5 physics number), G0 (VS device builds, Android + iOS from project week 3, D-100), G2 (M5: on-device validation of all 60 graybox levels, heaviest set pieces), G3 (M6, final art), G5 (M8 full pass) — names per D-102.
- After adding a new world art kit, VFX family, shader, or a level flagged as a "set piece" (every 5th level).
- When the DevOverlay or a tester reports a frame drop, a hitch, heat, or a slow restart.

**Do NOT invoke** for editor-only slowness (compile times — that's ARCH), or to "optimise" code with no measured problem.

## Inputs
- Device matrix from `07_QA_PERFORMANCE_RELEASE.md` (at minimum: iPhone 11, iPhone SE 2/3, Galaxy A13/A14-class Low, Pixel 6a/A54 Mid).
- A **Development Build** with *Autoconnect Profiler* and *Deep Profiling OFF* (deep profiling only for targeted captures). Build Profile `Android-Dev` / `iOS-Dev` (`AB_DEV` define → DevOverlay).
- Profiling level list: L1 (baseline), L20, L40, L60 (bosses), the heaviest set piece per world, a fire/explosion-heavy level, a wind/portal level.
- Quality tier definitions (01 §3) and `QualityTierSelector`.

## Step-by-step workflow
1. **Build.** `manage_build` (or `Arrow Buster ▸ Build ▸ Android Dev`) → install via `adb install -r Builds/Android/ArrowBuster-dev.apk`. For iOS: export to `Builds/iOS`, build from Xcode on the Mac with a Development profile.
2. **Warm up.** Play 2 minutes before measuring (shader warm-up, thermal state). Note the device temperature/battery state. Lock 60 FPS (`Application.targetFrameRate` from `GameConstants`).
3. **Connect the profiler.**
   - Android: `adb forward tcp:34999 localabstract:Unity-com.attila.arrowbuster` (or attach automatically), then Unity Profiler ▸ target device. Logs: `adb logcat -s Unity ActivityManager DEBUG`.
   - iOS: Profiler ▸ attach over the network/USB; Xcode Instruments (Time Profiler, Allocations, Metal System Trace) for GPU.
   - From an agent: `manage_profiler` to start/stop captures and save `.data` files.
4. **Capture scenarios** (≥ 600 frames each, saved as `docs/qa/perf/captures/<date>_<device>_<level>_<scenario>.data`, git-ignored or LFS):
   - (a) idle aiming with the preview showing;
   - (b) the shot + the biggest collapse;
   - (c) restart ×5 (measure `LevelLoader.Load` markers);
   - (d) Win panel with the star animation;
   - (e) world map scroll.
5. **Read the numbers.** Main-thread ms, `FixedUpdate.PhysicsFixedUpdate` ms, script ms, render thread ms, SRP batches, triangles, `GC.Alloc` per frame, total memory. Use the Profile Analyzer (optional package) to compare against the previous capture.
6. **Compare to budgets** (01 §11): physics ≤ 3.5 ms avg / 5 ms peak, scripts ≤ 2 ms, rendering ≤ 5 ms, batches ≤ 120, GC 0 B/frame in gameplay, resident memory ≤ 450 MB Low, restart ≤ 300 ms Mid.
7. **Diagnose top offenders** (fix only what is over budget):
   - Physics → body count (validator), sleeping, colliders (prefer boxes/capsules over mesh colliders), debris cap, explosion overlap frequency.
   - Scripts → per-frame allocs, `GetComponent` in loops, registry iteration, trajectory preview recomputed every frame even when the aim didn't change.
   - Rendering → overdraw from VFX/transparent water/ice, shadow casters (disable on small props), SRP Batcher compatibility (Frame Debugger), texture sizes.
   - Memory → the Memory Profiler package (M8) snapshot; look for duplicate textures, audio loaded as decompressed-on-load for long clips, leaked level instances.
8. **Thermal soak** (M8): play 20 minutes on the Low device; record FPS at 0/10/20 min. If sustained FPS drops below 27 on Low, cut particle density / render scale for the Low tier.
9. **Tier validation:** force each tier via the DevOverlay and confirm visual parity of gameplay-relevant information (objective outlines, protected indicators must survive Low).
10. **Record** results and open tickets for each over-budget item (label `PLAT` + the owning role, priority by severity).

## Output artefacts
- `docs/qa/perf/YYYY-MM-DD_<milestone>_perf-report.md` — a table per device × scenario with ms, batches, memory, GC, restart time; pass/fail vs budget.
- Captures in `docs/qa/perf/captures/` (LFS) or referenced paths.
- Tickets `AB-###` for regressions.
- Updates to the tier table in 01 §3 through an ARCH decision entry, if tiers change.

## Quality checklist
- [ ] Measured on real devices (not the editor) for every pass/fail claim.
- [ ] Development build, deep profiling off, after warm-up.
- [ ] All five scenarios captured on at least one Mid and one Low device.
- [ ] Restart time measured with markers (p95 of 5).
- [ ] GC alloc per frame = 0 B during aiming and flight.
- [ ] Thermal soak done (M8).
- [ ] Gameplay readability preserved on the Low tier.
- [ ] Report written; regressions ticketed.

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| Hitch on the first explosion/break | Shader variants compiled on first use; pool not prewarmed | Shader variant collection warm-up in Boot; prewarm the VFX/debris pools (01 §13) |
| 30 FPS on an iPhone despite low ms | `targetFrameRate` overridden or vSync settings; Low Power Mode on | Check `GameBootstrap`; note Low Power Mode in the report |
| Steady GC every frame | LINQ/string concat in preview or HUD text updates every frame | Cache; update TMP only on change; `SetText` with numeric args |
| Physics spike during collapse | Mesh colliders, too many awake bodies, debris colliding with gameplay | Primitive colliders; Debris layer matrix (D-008); cap bodies |
| Memory grows per restart | Layout instance or event subscriptions leaked | Verify destroy in `LevelLoader`; unsubscribe on disable; static reset |
| Profiler won't connect on Android | Port not forwarded, or not a development build | `adb forward ...`; rebuild with Development + Autoconnect |
| IL2CPP release behaves differently from the dev build | Code stripping removed reflection-used types | `link.xml` for reflected types; test the release config before the M9 closed test |
| Overheating on Low | Uncapped 60 FPS on the Low tier; heavy bloom | Enforce 30 FPS Low; bloom only on High |

## Example task prompt for a sub-agent
```text
Agent: ab-mobile-platform
Skill: docs/skills/mobile-performance-profiling.md
Ticket: AB-139 (M8) Device-matrix profiling pass — set pieces first (L40, L60, L55)
Inputs: Android-Dev build from main@<sha>; devices: Pixel 6a (Mid), Galaxy A13 (Low); iPhone 11 via the Mac
Deliverables: docs/qa/perf/2026-xx-xx_M9_perf-report.md with scenarios a–e for each level/device; tickets for anything over the 01 §11 budgets.
Boundaries: measure + diagnose only; fixes go to the owning role's tickets unless it's PLAT-owned (quality settings, URP tier assets).
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
