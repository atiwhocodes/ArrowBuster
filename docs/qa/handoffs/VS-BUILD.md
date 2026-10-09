# Handoff — Vertical slice build (AB-003 … AB-025, M1–M2 core + five VS levels)

**Role:** ARCH + CORE + PHYS + PROPS + LEVEL + UI + FEEL (single agent, acting as each) · **Branch:** `core/vertical-slice-build` · **Commit(s):** `6b63a69` (slice), follow-up commit (tests, UI tween fix, docs)
**Status:** Partial. The game is playable end to end in the editor and passes the automated suites. Device feel gate (AB-014), DevOverlay (AB-015) and some ticket-level test suites are still open (see §6).

## 1. Summary
- **Core loop works:** drag anywhere in the lower 55% of the screen to draw. The dotted preview shows the real path. Release fires a swept kinematic arrow, which embeds, deflects or ricochets by material, or cuts ropes. Structures break and topple. Win after calm settle, fail on out-of-arrows or a broken vase. One-tap retry, pause, next level.
- **Five Greenwood levels** (W1_L01/04/07/10/16) built from JSON blueprints. The solvability bot wins every one at gold par (3★).
- **Original procedural art + synthesised audio** (D-107): Greenwood backdrop, bow, arrows, crates, crests, vase, rope. ~50 SFX + a music loop. Code-built HUD/panels with the OFL Lilita One font.
- **Feedback layer:** material SFX, chip/puff particles, hit-stop, camera shake, native haptics (iOS/Android bridge), Reduced Motion setting.
- **Services/save/analytics:** service locator with null defaults, atomic JSON save with `.bak` fallback, debug analytics CSV with the 02/06 loop events.

| # | Acceptance (rolled up from AB-003…AB-025) | Result | Evidence |
|---|---|---|---|
| 1 | Press Play in any scene → working services | ✅ | `GameBootstrap` → `ServiceInstaller.EnsureInstalled`; Gameplay scene runs standalone (PlayMode suites load it directly) |
| 2 | Mouse/touch drag aims, cancel below threshold spends nothing | ✅ | `GameplayFlowTests.PointerDrag_ShowsPreview_AndFiresOnRelease`, `TinyDrag_IsCancelled_AndConsumesNoArrow` (virtual Input System mouse) |
| 3 | Preview visible while drawing, same solver as flight | ✅ | Preview + flight both use `BallisticSolver` + `ArrowSweep`; flow test asserts dots visible. ⚠️ no explicit 2 cm parity test yet |
| 4 | Deterministic ballistic solver | ✅ | `BallisticSolverTests` (analytic apex, straight line, determinism, wind) |
| 5 | Impact rules + impulse clamp | ✅ | `ArrowImpactResolverTests` (embed, clamp, static, deflect, ricochet budget, hittable override, outcome factors) |
| 6 | Stars / quiver rules incl. bonus cap | ✅ | `StarRulesTests`, `QuiverModelTests` |
| 7 | Win/fail flow (out of arrows, protected lost), no firing after a result | ✅ | `MissingEveryArrow_FailsOutOfArrows`, `HittingTheVase_FailsProtectedLost` |
| 8 | Pause freezes time; retry resets quiver/objectives | ✅ | `PauseAndResume_FreezeAndRestoreTime`, `Retry_ResetsQuiverAndObjectives` |
| 9 | Save atomic + backup restore | ✅ | `JsonSaveServiceTests` (round trip, corrupt main → .bak) |
| 10 | Camera fits width at 9:16 / 9:19.5, height at 3:4 | ✅ | `CameraFramerTests` |
| 11 | 5/5 VS levels solvable at par | ✅ | `VerticalSliceSolvabilityTests` → `[Audit] 5/5 levels pass at par` |
| 12 | 1,000-shot tunnelling, restart < 300 ms, stack stability, 10/10 jitter runs | ❌ not yet | follow-ups below |

## 2. Files changed (by area)
| Path | Change | Notes |
|---|---|---|
| `Scripts/Runtime/Core/*` | new/modified | events, payloads, enums, `TimeScaleController`, `LevelClock`, `CosmeticRandom`, `Log`, `AppRoot`, `AppConfig`, `ProceduralTextures` |
| `Scripts/Runtime/Services/*` | new | interfaces + null/debug services, `Services` locator, `AnalyticsBridge` |
| `Scripts/Runtime/Bow/*`, `Arrows/*` | new | input, draw model, preview, solver, sweep, resolver, projectile, spawner |
| `Scripts/Runtime/Physics/*` | new | `PlanarBody`, `MaterialProfile/Body`, `Breakable`, `ImpactDamage`, `DebrisPool`, registry, `SettleMonitor` |
| `Scripts/Runtime/Objectives/*`, `Props/*` | new | crest objective, protected vase, rope, kill zones |
| `Scripts/Runtime/Levels/*` | new/modified | `LevelData` v2, layout, loader, catalog, anchors |
| `Scripts/Runtime/Gameplay/*` | new | controller state machine, tuning, quiver, stars, framer, tutorial prompts, bot (`AimFinder`, `IntendedSolutionRunner`, `LevelAudit`) |
| `Scripts/Runtime/UI/*` | new | `GameplayUI`, `UiFactory`, `UiTween`, theme, safe area |
| `Scripts/Runtime/Feedback/*`, `Plugins/iOS/ABHaptics.mm` | new | audio, VFX, haptics, shake, feedback director |
| `Scripts/Editor/Content/*`, `Editor/Levels/*` | new | art kit, SFX synth, prefab factory, content generator, blueprint DTOs + importer |
| `Levels/Blueprints/*.json` | new | 5 VS levels with recorded intended shots |
| `Art/**`, `Audio/**`, `Prefabs/**`, `ScriptableObjects/**`, `Resources/AppConfig.asset`, `UI/Fonts/*` | generated | via Arrow Buster ▸ Build ▸ Generate Content |
| `Scenes/Boot.unity`, `Scenes/Gameplay.unity` | modified | generated scenes |
| `Tests/EditMode/*` (8 new fixtures), `Tests/PlayMode/*` (2 new fixtures) | new | see §4 |
| `docs/art/ASSET_LICENSES.md` | new | licence register |
| `docs/planning/10_DECISION_LOG.md` | modified | D-105 … D-108 (Proposed) |

## 3. Decisions
- **Made (within authority), logged as Proposed:** D-105 Oak impulse 15/20 Ns + weak timber pole (HP 4). D-106 approach-path rule and VS layout changes (VS-04 redesigned). D-107 procedural art/audio, code-built UI. D-108 hit-stop inside FeedbackDirector, bot timeScale exception, runInBackground, JSON blueprints, `UiTween.Delay` owner.
- **Needed (OWNER):** D-102 says build *only* the vertical slice until 5–10 external casual players have tested it (gate G0). Confirm whether to (a) run that playtest with the attached APK now, or (b) override the gate and continue into M4 systems / more content.

## 4. Tests run + results
| Suite | Filter | Passed | Failed | Skipped | Notes |
|---|---|---|---|---|---|
| EditMode | all | 80 | 0 | 0 | 0.17 s |
| PlayMode | all | 9 | 0 | 0 | 39.5 s, includes the full 5-level solvability audit |
| Manual (editor) | play all 5 levels, win/fail/pause panels | ✅ | | | screenshots taken during the build |
| Build | Android development APK (IL2CPP, ARM64) | ✅ | | | `Builds/Android/ArrowBuster-dev.apk`, 43 MB, 0 errors, 6.6 min; not yet run on a device |

## 5. Unity Console state
- After the final recompile: 0 errors, 0 warnings from project code.
- Android build warnings (208) are all from Unity packages (obsolete PVRTC compression on editor icons) plus the URP Default Volume Profile auto-update notice; none from `Assets/_Project`.
- Test-only: "There are no audio listeners in the scene" appears while PlayMode smoke tests run in the runner's empty scene (AppRoot audio exists before the Gameplay camera). Harmless.

## 6. Known issues / limitations
- Not yet built: DevOverlay/cheats (AB-015), physics stability spike + device feel gate (AB-014), 1,000-shot tunnelling suite, preview/flight 2 cm parity PlayMode test, restart-time perf test, jitter-grid robustness runs (07 §5.1), PrefabLibraryTests, SaveMigrator.
- UI is code-built (no `Prefabs/UI/*`), and Home/WorldMap scenes are placeholders (disabled in build settings). Boot goes straight to Gameplay → the slice catalog.
- Art is procedural placeholder (D-107). Final Greenwood art is M6.

## 7. Evidence
- Audit log: `[Audit] 5/5 levels pass at par` — W1_L01 1/1 3★ sim 1.6 s · W1_L04 1/1 4.8 s · W1_L07 1/1 5.3 s · W1_L10 1/1 3.1 s · W1_L16 2/2 7.8 s.
- Analytics CSV chain verified in the Console (`level_started` … `level_completed`).

## 8. Follow-ups
- AB-014 device feel gate (needs testers), AB-015 DevOverlay, AB-009 tunnelling suite, AB-008 parity test, AB-016 restart perf test, AB-025 jitter grid (10/10 runs). PlayMode tests now use a temp save folder (`TestSave`).

## 9. Locks released
- none held
