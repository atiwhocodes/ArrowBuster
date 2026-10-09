# 01 — Technical Architecture

> Owner: Unity Technical Architect (`ARCH`). Status: **v2 — owner-approved 2026-10-09; technical direction locked (D-103).**
> This is the **canonical reference for names**: folders, assemblies, layers, classes, events, ScriptableObjects and save keys. Every other planning doc and every agent task must use these names. Changing a name here requires a decision-log entry ([`10_DECISION_LOG.md`](10_DECISION_LOG.md)).

---

## 1. Engine, render pipeline and the 2D / 2.5D / 3D choice

### 1.1 Unity version
- **Develop on Unity 6000.6.5f1** (already installed, fixed in CLAUDE.md).
- **Locked (D-080):** Unity 6000.6.5f1 is used for the whole MVP. There is **no upgrade spike**. The editor is upgraded only after launch, or if a release-blocking iOS/Android issue requires it (OWNER approval + decision entry).
- Any exceptional upgrade happens on its own branch, at a milestone boundary, by `ARCH`, and must re-pass all tests, the solvability bot and device builds.

### 1.2 Rendering — URP (decision D-003)
| Option | Verdict |
|---|---|
| Built-in RP | Rejected. In maintenance mode, no SRP Batcher, and the newest mobile features target URP. |
| **URP (Forward)** | **Chosen.** Already configured (`Mobile_RPAsset`). SRP Batcher, per-tier quality assets, Shader Graph for stylised materials, good mobile tooling. |
| HDRP | Rejected — not for mobile. |

Rules: Forward rendering path (Forward+ only if more than 4 real-time lights appear, which they shouldn't), **one real-time directional light**, everything else baked or unlit-ish stylised. **No real-time post-process depth of field.** The §11 "shallow DOF on background" is faked by pre-blurred background layers and atmospheric fog. Bloom on High tier only. One shared stylised lit shader (Shader Graph) with a gradient-palette texture per world.

### 1.3 Physics dimensionality (decision D-004)
| Option | Pros | Cons | Verdict |
|---|---|---|---|
| Pure 2D (sprites + Box2D) | Cheapest. No depth drift by construction. Strong stacking (Box2D v3 / PhysicsCore2D). | Loses the §11 "stylised 3D toy diorama" look. 3D cosmetic debris impossible. Deviates from MVP §12 and CLAUDE.md. | Rejected for MVP. **Fallback** if the M1 physics spike fails its stability gates. |
| 3D visuals + 2D physics | Diorama look plus 2D stability. | Two coordinate worlds. Collider authoring mismatch on 3D meshes. The new PhysicsCore2D API is young and agents know it poorly. | Rejected (re-evaluate only via the M1 fallback). |
| Full 3D | Maximum spectacle. | Depth drift, aiming in 3D, camera complexity, unreadable outcomes. | Rejected. |
| **2.5D: 3D PhysX constrained to the play plane** | Matches MVP §12 and CLAUDE.md. 3D meshes and colliders. Cosmetic debris may tumble in depth. Mature API; agents know it well. | PhysX is not cross-platform deterministic, so stacks need care. | **Chosen.** |

**Constraint model:** every *gameplay* rigidbody uses `RigidbodyConstraints.FreezePositionZ | FreezeRotationX | FreezeRotationY`. Its transform Z equals `GameConstants.PlayPlaneZ` (0). These are PhysX lock flags applied inside the solver, so stacks stay stable. `PlanarBody` (component) enforces this at `Awake`, and the level validator rejects violations at edit time. **Cosmetic debris is the only unconstrained dynamic object**, and it lives on the `Debris` layer, which collides only with `Environment`.

---

## 2. Packages

| Package | Status | Why | Integration style |
|---|---|---|---|
| `com.unity.render-pipelines.universal` 17.6 | Installed — keep | Rendering (D-003) | Native Unity package |
| `com.unity.inputsystem` 1.20 | Installed — keep | `Pointer.current` covers mouse + touch + pen; UI module | Native |
| `com.unity.ugui` 2.6 (includes TextMeshPro) | Installed — keep | Runtime UI (D-012) | Native |
| `com.unity.test-framework` 1.8 | Installed — keep | EditMode + PlayMode tests, solvability bot | Native |
| `com.coplaydev.unity-mcp` v10 | Installed — keep (editor-only) | Agent ↔ Editor bridge | Dev tool; verify it is excluded from player builds (AB-002) |
| `com.unity.ide.rider` / `ide.visualstudio` | Installed — keep | IDEs | Dev tool |
| `com.unity.timeline` | Installed — **optional** | Splash and set-piece intro framing. Remove at M8 if unused. | Native |
| `com.unity.visualscripting` | Installed — **remove** (AB-002) | Unused. Adds compile time and confuses agents. | — |
| `com.unity.collab-proxy` | Installed — **remove** | Unity Version Control is not used (Git) | — |
| `com.unity.ai.navigation` | Installed — **remove** | No NavMesh in this game | — |
| `com.unity.purchasing` (Unity IAP) | **Selected — add in M7** (D-090) | Store abstraction for the App Store and Google Play, restore purchases | Native package behind `IIapService` |
| **Unity LevelPlay** (ad mediation) | **Selected — add in M7** (D-090) | Rewarded + interstitial with mediation | Vendor SDK behind `IAdsService`, in the `ArrowBuster.Integrations` asmdef |
| Google UMP (consent) | **Add in M7 with ads** (integration path with LevelPlay verified at integration) | IAB TCF v2.2 certified CMP for EEA/UK. Free. Usually bundled or supported by mediation. | Vendor SDK behind `IConsentService` |
| Firebase (Analytics, Crashlytics, Remote Config) | **Selected — add in M7** (D-090) | One vendor family for 3 needs. Mature IL2CPP symbolication. | Vendor SDK behind `IAnalyticsService` / `ICrashReportingService` / `IRemoteConfigService` |
| External Dependency Manager (EDM4U) | Comes with Firebase/ads | Resolves Gradle and CocoaPods dependencies | Vendor tool. Pin its version. |
| `com.unity.mobile.android-logcat` | Optional dev tool | Device logs from the editor | Dev only |
| `com.unity.memoryprofiler` | Optional dev tool (M8) | Memory snapshots | Dev only |
| `com.unity.probuilder` | Optional (graybox environments only) | Quick environment blocking | Editor-time authoring; final art replaces it |
| `com.unity.addressables` | **Not in MVP** (D-013) | 60 small levels load directly. Revisit for live content after launch. | Post-MVP |
| `com.unity.localization` | **Not in MVP** | English only. Strings go through `UIStrings` keys so migration later is mechanical. | Post-MVP |
| `com.unity.cinemachine` | **Not used** | Fixed camera per level; our own `CameraFramer` is ~80 lines | — |
| Tweening (LitMotion / PrimeTween / DOTween) | **Not yet** (D-031) | In-house `UiTween` (≤ 200 lines, unscaled time) covers MVP. LitMotion (MIT, UPM) is the pre-approved fallback if UI animation volume grows at M5. | Small custom abstraction |
| Haptics plugin (e.g. Nice Vibrations) | **Not used** (D-032) | A thin native bridge is ~150 lines total and has no licence uncertainty | Small custom abstraction |
| Cloud save (UGS Cloud Save / Play Games / iCloud) | **Post-MVP** | Not required by §12 | Post-MVP |

**Package rule:** a new package or SDK needs a decision-log entry by `ARCH`. The entry covers why, licence, size impact, platform impact and the removal path. Agents never add packages on their own.

---

## 3. Target platforms and device performance tiers

| Platform | Minimum | Notes |
|---|---|---|
| iOS | iOS 15.0 (already set). iPhone and iPad universal. | Built on a Mac via an Xcode export. Lock 60 FPS on ProMotion devices. |
| Android | API 26 (Android 8.0), ARM64, IL2CPP (already set). Target API = the latest that Google Play requires at submission time (verify in M8 and again at M9 submission). | AAB for store, APK for dev |

| Tier | Example devices | FPS | Render scale | Shadows | Debris cap | Particles | HDR/Bloom |
|---|---|---|---|---|---|---|---|
| **High** | iPhone 13+, Galaxy S22+, Pixel 7+ | 60 | 0.9 | Main light, hard, 25 m | 40 | 100% | HDR on, bloom on |
| **Mid** (baseline) | **iPhone 11**, iPhone SE 2/3, Galaxy A54, Pixel 6a | 60 | 0.8 | Main light, hard, 20 m | 40 | 100% | HDR off, bloom off |
| **Low** | Galaxy A13/A14, Redmi 10A-class, ≤ 3–4 GB RAM | **30** | 0.7 | Off (blob decal under props) | 20 | 50% | off |

Tier selection is done by `QualityTierSelector` (Platform). It uses a lookup heuristic (`SystemInfo.systemMemorySize`, `graphicsMemorySize`, `processorCount`, GPU name allow/deny lists) and then a **runtime guard**: if the median frame time over the first 5 s of gameplay is above 1.25× budget, drop one tier and persist that choice. A Settings override is available only in dev builds.

---

## 4. Scene architecture

```
Boot.unity ──► Home.unity ──► WorldMap.unity ──► Gameplay.unity
   │              ▲   ▲              │   ▲              │  (levels are prefabs instantiated here;
   │              │   └──────────────┘   └──────────────┘   Next/Retry never reloads the scene)
   └─ persistent "AppRoot" (DontDestroyOnLoad): Services, AudioService, SceneFlow, TransitionOverlay
```

| Scene | Contents (keep minimal: one root prefab per scene) | Owner |
|---|---|---|
| `Boot` | `AppRoot.prefab` (ServiceInstaller, AudioService, SceneFlow, TransitionOverlay canvas), splash. Loads the save, initialises consent → analytics/crash → remote config (2 s timeout) → Home. Target < 3 s to Home on Mid. | ARCH |
| `Home` | `HomeRoot.prefab` (Home screen canvas, Bow Forge, Settings) | UI |
| `WorldMap` | `WorldMapRoot.prefab` (map canvas, world backdrops) | UI |
| `Gameplay` | `GameplayRoot.prefab` (CameraRig, Lighting, Bow, ArrowSpawner, GameplayController, HUD canvas, Win/Fail/Pause panels, Feedback rig). The level layout prefab is instantiated under `LevelRoot` at runtime. | CORE |

**Editor play-from-any-scene:** `GameBootstrap` (`BeforeSceneLoad`) calls `ServiceInstaller.EnsureInstalled()`. When Boot did not run, it installs editor defaults (Debug analytics, Mock ads/IAP, Local remote config, in-memory or real save). `GameplayController` has a serialized `_editorFallbackLevel` (LevelData) for pressing Play in Gameplay directly. **This is mandatory for agent iteration via MCP.**

**Level load / restart (D-011):** `LevelLoader.Load(LevelData)` runs these steps:
1. Return all arrows, debris, VFX and audio to their pools.
2. Destroy the previous layout instance.
3. Instantiate `LevelData.layoutPrefab` under `LevelRoot`.
4. Call `PhysicsBodyRegistry.Rebuild()`.
5. Put all bodies to sleep.
6. Apply `LevelLayout` framing.
7. Reset the deterministic level clock to 0.
8. Raise `LevelStarted`.

Target: < 300 ms on Mid, measured by the PlayMode perf test.

---

## 5. Integration strategy summary (the required recommendation list)

| Need | Recommendation | Type | When |
|---|---|---|---|
| Input | Input System `Pointer.current` polled in `Update`, behind `BowInputReader`. UI uses `InputSystemUIInputModule`. Draws that start over UI are ignored. | Native package + thin adapter | M1 |
| UI | uGUI + TextMeshPro. Canvas per screen. `SafeAreaFitter`. UI Toolkit only for editor tooling. | Native | M2 |
| Tweening/animation | In-house `UiTween` (unscaled time, pooled) plus Animator only for character/bow rigs. LitMotion is the fallback (D-031). | Small custom abstraction | M3 |
| Audio | Built-in AudioSource pool (24 voices) + AudioMixer (Master/Music/SFX/UI groups) + `SoundEvent` ScriptableObjects. No FMOD/Wwise. | Native + small abstraction | M3 |
| Haptics | `IHapticsService`. iOS `UIImpactFeedbackGenerator`/`UINotificationFeedbackGenerator` via `Plugins/iOS/ABHaptics.mm`. Android `VibrationEffect` via `AndroidJavaObject`. Editor no-op logger. | Small custom abstraction | M3 |
| Ads | `IAdsService` + `AdPolicy` (pure rules, unit-tested; interstitial rules D-093, bonus arrow D-063/D-084). **Unity LevelPlay** adapter (`LevelPlayAdsService`) in M7. Mock in editor/dev. | Selected SDK behind interface | Interface M2, SDK M8 |
| IAP | Unity IAP behind `IIapService`. Products: `remove_ads` (non-consumable), `starter_pack` (non-consumable). Restore on iOS from Settings. Prices D-091. | Native package | M7 |
| Analytics | `IAnalyticsService` with `DebugAnalyticsService` (logs + CSV in dev) from M2. **Firebase Analytics** adapter in M7, collection disabled until consent where required (D-096). Event dictionary in `06`. | Selected SDK behind interface | Interface M2, SDK M7 |
| Consent/privacy | `IConsentService`: Google UMP for GDPR/UK + US-state notices, iOS ATT prompt after UMP, consent stored in the save. Analytics, ad personalisation and crash reporting are enabled **only after** consent resolves where consent is required (UK/EEA, D-096). | SDK (UMP) + small abstraction | M8 |
| Crash reporting | `ICrashReportingService` (breadcrumbs, non-fatal logging). **Firebase Crashlytics** adapter, collection consent-gated in UK/EEA (D-096). Play Console vitals and Xcode Organizer as free baselines. Needed by the closed test. | Selected SDK behind interface | Interface M2, SDK M7 |
| Remote config | `IRemoteConfigService` with `RemoteConfigDefaults` SO (all keys plus defaults). **Firebase Remote Config** adapter. Fetch at Boot with a 2 s timeout, use the cached last-known values, never block play. | Selected SDK behind interface | Interface M2, SDK M7 |
| Local save | `ISaveService` → `JsonSaveService` (JsonUtility, atomic write + `.bak`, versioned migrations) | Small custom abstraction | M2 (progress), M6 (full meta) |
| Cloud save | **Post-MVP.** The save schema has `installId` and `schemaVersion` so a cloud merge can be added later. | Post-MVP | — |

---

## 6. Assembly definition strategy

| Assembly | Folder | References | Notes |
|---|---|---|---|
| `ArrowBuster.Runtime` (exists) | `Scripts/Runtime` | `Unity.InputSystem`, `Unity.TextMeshPro`, `UnityEngine.UI` | All game code. Namespace `ArrowBuster` (sub-namespaces allowed: `ArrowBuster.Bow` etc.). **Must not reference any vendor SDK.** |
| `ArrowBuster.Integrations` (new, M7) | `Scripts/Integrations` | Runtime + vendor assemblies | Vendor adapters only (`FirebaseAnalyticsService`, `FirebaseCrashReportingService`, `FirebaseRemoteConfigService`, `LevelPlayAdsService`, `UnityIapService`, `UmpConsentService`). Each adapter is wrapped in `#if AB_FIREBASE` style defines set by `versionDefines`/scripting defines, so the project compiles with SDKs absent. |
| `ArrowBuster.Editor` (exists) | `Scripts/Editor` | Runtime | ProjectSetup, BuildScript, level tools, validators |
| `ArrowBuster.Tests.EditMode` (exists) | `Tests/EditMode` | Runtime (+ Editor for validator tests) | Pure logic tests |
| `ArrowBuster.Tests.PlayMode` (new, AB-002) | `Tests/PlayMode` | Runtime | Physics, solvability bot, flow, perf smoke |

**One Runtime assembly for MVP.** Boundaries are enforced by namespace + folder ownership + the dependency rules in §10, and checked by `ArchitectureRulesTests` (reflection: e.g. no type in `ArrowBuster.Gameplay` references `ArrowBuster.UI`). Split Runtime into more assemblies only if incremental compile exceeds ~10 s.

---

## 7. Folder structure (canonical)

```
Assets/_Project/
  Scripts/
    Runtime/                         ArrowBuster.Runtime
      Core/        GameConstants, GameEnums, GameBootstrap, Services, ServiceInstaller, GameEvents,
                   GameEventPayloads, Log, StaticReset, PrefabPool, LevelClock (+ TimedEvents scheduler),
                   TimeScaleController (sole Time.timeScale writer, D-061), CosmeticRandom, BuildConfig
      Gameplay/    GameplayController (+ pure GameplayStateMachine), GameplayState, LevelSession, QuiverModel,
                   StarRules, GameplayTuning (SO), SceneFlow (app flow), CameraFramer, TutorialPromptController,
                   DevOverlay + IntendedSolutionRunner (AB_DEV only)
      Bow/         BowController, BowInputReader, DrawModel, AimState, TrajectoryPreview, BowView
      Arrows/      ArrowDefinition, ArrowProjectile, ArrowSpawner, ArrowRegistry, BallisticSolver,
                   ArrowFlightState, ArrowImpactResolver, ImpactOutcome, ImpactContext,
                   ImpactKind, SplitArrowBehaviour,
                   FireArrowBehaviour, BounceArrowBehaviour
      Physics/     IFlightEnvironment, IArrowHittable, ArrowHitInfo, IExplosionReactive (contracts that
                   Props/Objectives implement), PhysicsLayers, PhysicsSettingsSpec (D-006 values), PlanarBody, MaterialProfile, MaterialBody, Breakable, ImpactDamage,
                   DebrisPool, DebrisPiece, SettleMonitor, PhysicsBodyRegistry, BodyKind, ExplosionSolver,
                   Explosion, PhysicsDebugDraw + PhysicsStatsLogger (AB_DEV)
      Objectives/  Objective, ObjectiveClearRule, ObjectiveTracker, ProtectedObject, ProtectedTracker,
                   BullseyeMarker
      Props/       RopeCuttable, RopeView, Balloon, Burnable, FireZone, OilJar, PowderBarrel,
                   WindField, WindFieldSampler, KinematicMover, PortalRing, PortalPair, KillZone,
                   PlayBounds   (SpringPlate: **cut from MVP**, D-086 — not built)
      Levels/      LevelData, QuiverEntry, IntendedShot, TutorialPromptData, ObjectiveSummaryEntry, LevelLayout,
                   LevelLoader, LevelCatalog, WorldData, TutorialAnchor, LevelEnums (MechanicTag,
                   SolutionArchetype, LevelStatus — D-075)
      Progression/ SaveGame (+DTOs), JsonSaveService, SaveMigrator, ProgressionService, EconomyService,
                   EconomyConfig, CosmeticDefinition, CosmeticCatalog, InventoryService, IapCatalog,
                   DailyChallengeService, BullseyeCollection
      UI/          UIRouter, ScreenBase, HomeScreen, WorldMapScreen, LevelNodeView, HudView,
                   ObjectiveIconsView, QuiverView, WinPanel, FailPanel, PausePanel, SettingsPanel,
                   BowForgeScreen, ShopScreen, DailyChallengePanel, ArrowRevealCard, ConsentPanel, ModalDialog,
                   CreditsPanel, RewardedOfferButton, SafeAreaFitter, UiTween, UIStrings, ToastView
      Feedback/    FeedbackDirector, AudioService, SoundEvent, SoundLibrary, MusicPlayer, VfxService,
                   VfxEvent, HitStop (requests via TimeScaleController), CameraShake, ChainReactionTracker,
                   HapticsService, HapticKind
      Services/    IAnalyticsService, IAdsService, IIapService, IConsentService, IRemoteConfigService,
                   ICrashReportingService, ISaveService, IHapticsService, IAudioService, AdPolicy,
                   RemoteConfigDefaults, RemoteConfigKeys, AnalyticsEvents (names + param builders),
                   AnalyticsBridge (GameEvents → analytics), AnalyticsContext (common params),
                   Debug*/Mock*/Recording* implementations (e.g. DebugAnalyticsService, RecordingAnalyticsService)
      Platform/    QualityTierSelector (raises TierChanged → `quality_tier_changed`), QualityTier, AppLifecycle,
                   DeviceInfo
    Integrations/                    ArrowBuster.Integrations (M7): Firebase + LevelPlay + Unity IAP + UMP adapters (D-090)
    Editor/                          ArrowBuster.Editor
      Setup/ ProjectSetup, LayerSetup, PhysicsSetup
      Build/ BuildScript, BuildVersioning, iOSPostProcess (Info.plist strings, privacy manifest checks)
      Levels/ LevelEditorWindow, LevelDataInspector, LevelValidator, ShotRecorder, LevelCatalogBuilder
      Validation/ PrefabValidator, AssetImportRules (AssetPostprocessor)
  Tests/
    EditMode/  <System>Tests.cs (BallisticSolverTests, DrawModelTests, QuiverModelTests, StarRulesTests,
               AdPolicyTests, SaveMigratorTests, EconomyServiceTests, LevelValidatorTests, ...)
    PlayMode/  LevelSolvabilityTests, LevelIdleStabilityTests, ArrowPreviewParityTests,
               InteractionMatrixTests, RestartPerfTests, GameFlowSmokeTests
  Prefabs/     Bow/ Arrows/ Structures/ Objectives/ Props/ Hazards/ Protected/ UI/ Roots/ Levels/World1-3/ Debris/ VFX/
  ScriptableObjects/ Levels/World1-3/ Worlds/ Arrows/ Materials/ Cosmetics/ Audio/ VFX/
               Config/ (GameplayTuning.asset, EconomyConfig.asset, RemoteConfigDefaults.asset, BuildConfig_<Env>.asset)
  Scenes/      Boot, Home, WorldMap, Gameplay  (+ Sandbox/Sandbox_<ROLE>_<Topic>.unity scratch scenes, never in build)
  Art/         Models/ Materials/ Textures/ Shaders/ Animations/ VFX/ Sprites/ Environments/<World>/
  Audio/       SFX/<Category>/ Music/ Mixers/
  UI/          Sprites/ Fonts/ Atlases/
  Plugins/     iOS/ Android/        (native haptics bridge)
docs/          planning/ agents/ skills/ levels/ art/ qa/{test-runs,perf,playtests,analytics,monetisation,releases,ui-reviews}
               (qa/BUGS.md = bug log; see docs/README.md)
.claude/agents/ subagent definitions
.github/workflows/ CI (GameCI), owned by PLAT, from M4 (required by M7)
```

Editor menu roots (canonical): `Arrow Buster ▸ Setup ▸ {1. Apply Mobile Player Settings, 2. Create Scenes + Build List, 3. Switch Platform to Android, Run All (1 + 2)}` (exist in `ProjectSetup.cs`) plus `{Layers, Physics}` (added by AB-002), `Arrow Buster ▸ Levels ▸ {Level Editor, Validate All, Validate Selected, Build Catalog}`, `Arrow Buster ▸ Build ▸ {Android Dev, Android Release, iOS Dev, iOS Release}`. Tag: `Ground` (Environment colliders that count as ground for TrainingDummy, D-037).

New folders vs. M0: `Gameplay/`, `Platform/`, `Integrations/`, `Prefabs/Protected`, `Prefabs/Roots`, `Prefabs/Debris`, `Prefabs/VFX`, `ScriptableObjects/{Worlds,Audio,Config,VFX}`, `Tests/PlayMode`, `Plugins/`, `Scenes/Sandbox`. They are created by the first ticket that needs them.

---

## 8. Naming conventions

| Thing | Pattern | Example |
|---|---|---|
| C# types | PascalCase; one public type per file, file = type | `ArrowProjectile.cs` |
| Private fields | `_camelCase`; serialized = `[SerializeField] private` | `[SerializeField] private float _maxDrawPixels;` |
| Constants | PascalCase in `GameConstants` (global targets only) | `WinSettleSeconds` |
| Events | `event Action<T> Verbed` (past tense) | `ObjectiveCleared` |
| Level rules asset | `W<w>_L<nn>` (keep the M0 convention) | `W1_L05.asset` |
| Level layout prefab | `Lvl_W<w>_L<nn>` | `Lvl_W1_L05.prefab` |
| Vertical-slice playlist | `LC_VerticalSlice.asset` (LevelCatalog) | — |
| Structure prefab | `Struct_<Shape>_<Material>_<Size>` | `Struct_Crate_Timber_1x1`, `Struct_Beam_Stone_3x0.5` |
| Objective prefab | `Obj_<Kind>` | `Obj_CrestTarget`, `Obj_CursedOrb` |
| Protected prefab | `Prot_<Kind>` | `Prot_RoyalVase`, `Prot_SleepingFox` |
| Prop prefab | `Prop_<Name>[_Variant]` | `Prop_PowderBarrel`, `Prop_Rope_Chain` |
| Hazard prefab | `Haz_<Name>` | `Haz_WaterPit`, `Haz_SpikeBed` |
| Arrow prefab / definition | `Arrow_<Type>` / `AD_<Type>` | `Arrow_Heavyhead`, `AD_Heavyhead` |
| Material profile | `MP_<Material>` | `MP_Timber` |
| Cosmetic | `CD_<Slot>_<Name>` | `CD_Bow_Moonwood`, `CD_Trail_LeafSwirl` |
| Sound event | `SE_<Category>_<Name>` | `SE_Impact_Stone` |
| Audio clip | `SFX_<Category>_<Name>_<nn>` / `MUS_<World>_<Name>` | `SFX_Break_Timber_02.ogg` |
| Render material | `M_<Name>[_Variant]` | `M_Timber_Greenwood` |
| Texture | `T_<Name>_<Map>` (BaseMap, Normal, Mask, Palette) | `T_Greenwood_Palette` |
| Mesh | `SM_<Name>` | `SM_Crate_Timber_1x1` |
| VFX prefab | `VFX_<Event>_<Material?>` | `VFX_Break_Ice` |
| UI sprite | `UI_<Group>_<Name>` | `UI_Icon_Arrow_Fire` |
| World code (D-052) | `GW` Greenwood Range · `SC` Sunscar Canyon · `FK` Frostspire Keep | — |
| Environment kit piece | `Env_<WorldCode>_<Name>` | `Env_GW_RuinArch` |
| Level marker prefab | `Marker_<Name>` | `Marker_TutorialAnchor`, `Marker_ClearLine` |
| Reusable sub-layout / template | `Lvl_Pattern_<Name>` / `Lvl_Template_Base` | `Lvl_Pattern_WatchTower` |
| Scenes | PascalCase | `Gameplay.unity` |
| Sandbox scenes | `Sandbox_<AgentRole>_<Topic>` | `Sandbox_PHYS_StackTest.unity` |
| Analytics events | `snake_case` verbs | `level_completed` |
| Remote config keys | `dot.separated.snake` | `ads.interstitial.min_levels_between` |
| Git branches | `<role>/<ticket>-<slug>` | `core/AB-006-ballistic-solver` |

---

## 9. Coding conventions (additions to CLAUDE.md)

1. **Physics in `FixedUpdate`, input in `Update`, visuals in `LateUpdate`.** Arrow flight runs in `FixedUpdate`. The arrow's visual transform is interpolated in `Update` between the last two fixed states.
2. **No randomness in gameplay outcomes.** `UnityEngine.Random` is banned in Gameplay/Arrows/Physics/Objectives/Props. Cosmetic randomness uses `CosmeticRandom` (a seeded `System.Random`) and may only touch debris spin, particles, audio pitch and camera shake.
3. **Static state must reset.** Enter Play Mode runs without domain or scene reload. Every `static` mutable field and every static event is cleared in a `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` method. Use the `StaticReset` helper.
4. **Zero per-frame GC allocations** in gameplay loops. Use `NonAlloc` physics queries with preallocated buffers, no LINQ in `Update`/`FixedUpdate`, no string building in hot paths.
5. **Pure logic in plain C# classes** (`BallisticSolver`, `DrawModel`, `QuiverModel`, `StarRules`, `AdPolicy`, `EconomyService`, `SaveMigrator`, `ExplosionSolver`, `ArrowImpactResolver` rules) so EditMode tests cover them.
6. **Tuning lives in data.** `GameConstants` holds only global product targets (FPS, settle times, caps). Per-system tuning lives in ScriptableObjects (`GameplayTuning`, `ArrowDefinition`, `MaterialProfile`, `EconomyConfig`, `RemoteConfigDefaults`). This also removes merge contention on `GameConstants.cs`.
7. **No `Find*`/`GetComponent` in hot paths.** Use registries (`PhysicsBodyRegistry`, `ArrowRegistry`, `ObjectiveTracker`) populated on level load.
8. **XML docs on public APIs.** `[Tooltip]` on every serialized tuning field.
9. **Logging** through `Log.Info/Warn/Error(LogCat, msg)`. `Info` is `[Conditional("AB_DEV")]`. Errors also go to `ICrashReportingService` breadcrumbs.
10. **Null-safe services:** `Services.Analytics` etc. are never null (null-object implementations are installed by default).

---

## 10. Event / messaging approach and dependency boundaries

### 10.1 Two tiers
- **Local wiring:** C# `event Action<T>` on the owning component (e.g. `RopeCuttable.Cut`, `Breakable.Broken`, `QuiverModel.Changed`). The listener is wired by the parent or registry on level load.
- **Cross-cutting gameplay bus:** `GameEvents` (static class, reset per `StaticReset`). It exists so that **Feedback, Analytics, UI and Tutorial** can listen without gameplay knowing they exist. Gameplay code **raises** events and never calls audio/VFX/haptics/analytics directly.

### 10.2 Canonical `GameEvents` (payloads are `readonly struct`s in `Core/GameEventPayloads.cs`)

| Event | Payload (key fields) | Raised by | Typical listeners |
|---|---|---|---|
| `LevelStarted` | `LevelSessionInfo` (levelId, worldId, globalIndex, attempt, arrowsStart, quiver) | GameplayController | Analytics, HUD, Tutorial, Music |
| `DrawStarted` / `DrawCancelled` | `AimState` | BowController | Feedback, Tutorial |
| `DrawThresholdReached` | `AimState` (power crossed `GameplayTuning.drawHapticThreshold`) | BowController | Haptics, BowView glow |
| `ArrowFired` | `ArrowFiredInfo` (arrowType, angleDeg, power01, arrowIndex, arrowsRemaining) | BowController | Analytics, Feedback, HUD |
| `ArrowImpact` | `ArrowImpactInfo` (arrowType, materialKind, targetKind, point, normal, speed, outcome, isMeaningful) | ArrowProjectile | Feedback (SFX/VFX/hit-stop/haptic), Bullseye |
| `ArrowResolved` | `ArrowResolvedInfo` (arrowId, finalOutcome: Embedded/Spent/OutOfBounds/KillZone) | ArrowProjectile | GameplayController |
| `ObjectBroken` | `BreakInfo` (materialKind, position, sizeClass, cause) | Breakable | Feedback, Debris |
| `PropTriggered` | `PropTriggerInfo` (propKind, position, cause) — rope cut, balloon pop, barrel blast, oil ignite, portal enter, spring fire, boulder release | Props | Analytics `object_triggered`, Feedback |
| `ObjectiveCleared` | `ObjectiveInfo` (kind, index, remaining) | ObjectiveTracker | HUD, Feedback |
| `ProtectedLost` | `ProtectedInfo` (kind, reason: Broken/Hit/Displaced/KillZone) | ProtectedTracker | GameplayController, Feedback |
| `SoftLockPrompt` | `int arrowsRemaining` | GameplayController | HUD toast |
| `GameplayStateChanged` | `GameplayState` | GameplayController | UI, DevOverlay |
| `LevelWon` | `LevelResultInfo` (arrowsUsed, stars, bonusArrowUsed, bullseye, durationSec, attempt) | GameplayController | Progression, Analytics, UI, Feedback |
| `LevelFailed` | `LevelResultInfo` + `FailReason` | GameplayController | Analytics, UI, AdPolicy |
| `LevelRestarted` / `LevelQuit` | `LevelSessionInfo` | GameplayController / UI | Analytics |

App-level (non-gameplay) events live on their services: `ProgressionService.WorldUnlocked`, `EconomyService.CoinsChanged`, `InventoryService.CosmeticEquipped`, `IIapService.PurchaseCompleted`, `IAdsService.RewardGranted`.

### 10.3 Dependency rules (allowed → arrows)

```
Core ◄── everything
Levels ◄── Gameplay, Objectives, Props, Physics, Editor
Physics ◄── Arrows, Props, Objectives, Gameplay
Arrows  ◄── Bow, Gameplay
Objectives, Props ◄── Gameplay
Gameplay ──raises──► GameEvents ◄──listens── Feedback, UI, Services(analytics glue), Tutorial
Progression ◄── UI, Gameplay (only via ProgressionService.RecordResult)
Services (interfaces) ◄── anyone; implementations only installed by ServiceInstaller
Arrows, Physics ──► never reference Props/Objectives types; they reach them only via the Physics interfaces
                   (IArrowHittable, IExplosionReactive, IFlightEnvironment) that Props/Objectives implement
UI ──► Gameplay only through public commands (Restart(), Pause(), Fire bonus arrow) – never internals
FORBIDDEN: Gameplay/Physics/Arrows/Props → UI, Feedback, Integrations; Runtime → Integrations; Runtime → UnityEditor (except #if UNITY_EDITOR gizmos)
```

---

## 11. Performance budgets (Mid tier = iPhone 11, 16.6 ms frame)

| Budget | Target | Hard limit |
|---|---|---|
| Physics (`FixedUpdate` total incl. simulation) | ≤ 3.5 ms/frame | 5 ms worst frame during collapses |
| Gameplay scripts | ≤ 2 ms | 3 ms |
| Rendering main thread | ≤ 5 ms | 7 ms |
| SRP batches / draw calls | ≤ 120 | 180 |
| Visible triangles | ≤ 150 k | 250 k |
| Dynamic gameplay rigidbodies per level | ≤ 45 typical | **60** (validator error) |
| Debris fragments | 40 (Low: 20) | pool-recycled, oldest first |
| Active arrows | 8 (`MaxActiveArrows`) | older embedded arrows become static decor |
| GC alloc in gameplay | 0 B/frame | — |
| App memory (resident) | ≤ 450 MB on Low tier | 600 MB |
| Download size | ≤ 150 MB (below the iOS 200 MB cellular prompt) | 200 MB |
| Level load / restart | ≤ 300 ms Mid | 1 s (§12) |
| Cold start to Home | ≤ 4 s Mid | 6 s |

---

## 12. ScriptableObject / data strategy (decision D-010)

| Data | Format | Why |
|---|---|---|
| Level rules (quiver, par, objectives meta, tutorial, intended shots, flags) | `LevelData` SO (one per level) | Inspector-editable, diffable YAML, references prefabs by GUID, testable |
| Level geometry | Layout prefab (`Lvl_W1_L05`) built from nested library prefabs | WYSIWYG in prefab mode via MCP. Restart = re-instantiate. |
| World structure | `WorldData` SO (id, name, 20 LevelData refs, unlock rule, theme refs, music, map art) + `LevelCatalog` SO (ordered worlds; also playlists like `LC_VerticalSlice`) | Progression configured without code |
| Arrow tuning + behaviour | `ArrowDefinition` SO per type | Designers tune feel; behaviour enum selects the strategy class |
| Material tuning | `MaterialProfile` SO per material | Drives mass/friction/bounce/break/embed/ricochet/SFX/VFX/debris |
| Global gameplay tuning | `GameplayTuning` SO (draw mapping, cooldowns, settle thresholds, hit-stop, preview density) | One asset, owned by CORE |
| Economy | `EconomyConfig` SO; `CosmeticCatalog` + `CosmeticDefinition` SOs | Prices/rewards in data. Remote config can override a whitelisted subset. |
| Audio/VFX | `SoundEvent`, `VfxEvent` SOs + `SoundLibrary` (material → events) | Art/Audio integrator swaps placeholders without code |
| Remote config defaults | `RemoteConfigDefaults` SO | Offline-first defaults, typed accessors |
| Save | JSON (`save.json`) — the only runtime-written data | Platform-safe, versioned |

**Special-arrow loadouts** are `LevelData.quiver` (ordered `List<QuiverEntry>`; the order is the firing order, e.g. `[Oak×2, Heavyhead×1]` → the player fires the next arrow in the list; the HUD shows the upcoming arrow). **No loadout screen** (§5). Choice levels (should-have) add `LevelData.choiceSlots` (two `ArrowType`s) — schema reserved, UI deferred.

**Quiver order rule (D-023):** the quiver is consumed in list order and the HUD previews the next arrow. *Alternative considered:* letting the player tap the HUD to pick which arrow type to fire next. That is deferred because it adds an input concept. Designers order the quiver so the intended solution is natural. If playtests show frustration, add "tap to swap" in M5 (decision checkpoint).

**Prefab variant strategy:**
- `Struct_Base` → per-material variants (`Struct_Crate_Timber_1x1` …); geometry variants are separate prefabs sharing the material variant pattern.
- `Obj_Base`, `Prop_Base`, `Prot_Base` abstract-ish bases carry `PlanarBody` + `MaterialBody` + layer. Kind-specific prefabs are variants.
- **World art swaps are prefab variants** (`Struct_Crate_Timber_1x1_Greenwood` → `_Sunscar`) that change only mesh/material, never colliders or mass. A `PrefabValidator` test asserts that collider bounds and mass match the base.
- Levels contain **only library prefab instances** (no unpacked prefabs, no loose primitives except `Environment` static blocking). The validator enforces this.

---

## 13. Object pooling strategy

Built on `UnityEngine.Pool.ObjectPool<T>` wrapped by `PrefabPool<T>` (Core). Pools are owned per scene root and survive level restarts.

| Pool | Prewarm | Max | Owner | Overflow policy |
|---|---|---|---|---|
| Arrows (per `ArrowType`) | 8 Oak, 3 per special type in the level's quiver | 8 active (`MaxActiveArrows`) + 9 split children | CORE | Oldest **embedded** arrow → converted to static decor (no collider, no script) and returned |
| Debris fragments (per material) | 40 shared (Low: 20) | `MaxDebrisFragments` | PHYS | Recycle the oldest |
| VFX (per `VfxEvent`) | 2–4 each | 6 each | ART | Skip spawn + dev warning |
| Audio voices | 24 AudioSources | 24 | ART | Steal the lowest priority / oldest |
| Trajectory dots | 40 | 40 | CORE | n/a |
| UI toasts / floating icons | 4 | 8 | UI | Recycle |

---

## 14. Save system architecture

- `ISaveService` → `JsonSaveService`. File: `Application.persistentDataPath/save.json`. Write path: serialize → `save.tmp` → `File.Replace(tmp, save.json, save.bak)`. Load: `save.json` → fall back to `save.bak` → new save. Failures are logged to the crash reporter as non-fatal.
- `SaveGame` root DTO with `schemaVersion` (starts at 1). `SaveMigrator` holds ordered `IMigration` steps (`1→2`, …) with EditMode tests for each.
- Lists instead of dictionaries (JsonUtility). Lookups are built in memory after load.
- **Write triggers:** level result, purchase/restore, cosmetic purchase/equip, settings panel close, daily claim, `OnApplicationPause(true)`, `OnApplicationQuit`. Debounced to ≤ 1 write per 0.5 s except on pause and purchase.
- **Integrity:** a SHA-256 of the payload is stored alongside it (it detects corruption, not cheating). Tampering is tolerated — no competitive or paid advantage exists.
- Detailed schema: `06_META_MONETISATION_ANALYTICS.md` §Save data.

---

## 15. Configuration and environment strategy

| Concern | Mechanism |
|---|---|
| Environments | `BuildConfig` SO per environment: `Dev`, `ClosedTest`, `Release`. Fields: `environment`, `adsTestMode`, `analyticsDebug`, `cheatsEnabled`, `remoteConfigEnvironment`, `logLevel`. |
| Scripting defines | `AB_DEV` (dev/closed-test builds: DevOverlay, verbose logs, cheats), `AB_CHEATS`, vendor defines `AB_FIREBASE`, `AB_LEVELPLAY`, `AB_UNITY_IAP` |
| Unity 6 Build Profiles | `Android-Dev`, `Android-Release`, `iOS-Dev`, `iOS-Release` — each sets defines and the `BuildConfig` reference |
| Secrets | Keystore, store API keys and `google-services.json` / `GoogleService-Info.plist` stay **outside git** or are git-ignored. `docs/README.md` explains where they live. |
| Remote config | `RemoteConfigKeys` constants + `RemoteConfigDefaults` SO. Only whitelisted keys are remote (list in `06`). |

---

## 16. Error logging and crash reporting

- `Log` wrapper with categories (`Bow`, `Arrow`, `Physics`, `Level`, `Save`, `Ads`, `IAP`, `Analytics`, `UI`).
- `Application.logMessageReceived` hook in `AppLifecycle` forwards `Error`/`Exception` to `ICrashReportingService.RecordNonFatal` with breadcrumbs (last 20 GameEvents names + levelId).
- Crash SDK: **Firebase Crashlytics** in M7 (D-090), with an IL2CPP symbol-upload build step; collection disabled until consent in UK/EEA (D-096). Free baselines: Play Console Android vitals, Xcode Organizer.
- Dev builds: `DevOverlay` shows the last 5 errors on screen.

---

## 17. Build pipeline

1. **Local scripted builds** (`Arrow Buster ▸ Build ▸ …`) via `BuildScript`. Also callable as `-executeMethod ArrowBuster.Editor.BuildScript.BuildAndroidDev` etc. for CLI/CI. It sets the version from `BuildVersioning` (`0.<milestone>.<patch>`, Android `bundleVersionCode` = monotonically increasing integer stored in `ProjectSettings`), runs `LevelValidator` over all levels first and aborts on errors.
2. **Android:** APK (dev, Development Build + Autoconnect Profiler optional) / AAB (closed test + release, signed with an upload key outside the repo).
3. **iOS:** Xcode project export to `Builds/iOS` → copied to the Mac → archive/sign in Xcode. A checklist lives in skill `release-readiness.md`.
4. **CI (recommended from M4, required from M7; the repo is on GitHub (`atiwhocodes/ArrowBuster`)):** GitHub Actions + GameCI. It runs EditMode + PlayMode tests on every PR to `main` and builds an Android dev APK nightly. Needs a Unity license secret. The iOS build stays manual on the Mac.

---

## 18. Security / privacy considerations

- No accounts, no PII. The analytics user id is a random `installId` GUID (resettable by reinstall). No IDFA unless ATT is granted; no Android Advertising ID use if consent is denied (configured in the ads SDK).
- Consent before collection (D-096): UMP (UK/EEA/CH and US-state regulations) → ATT (iOS) → then enable Firebase Analytics, Crashlytics and ad personalisation per the user's choices. Before consent (or if consent is denied/unresolved, D-067): non-personalised ads, Analytics and Crashlytics collection disabled, only the minimum technically essential diagnostics permitted by the chosen privacy implementation. Final legal/privacy review before launch. The privacy policy is hosted before M7 starts (D-097).
- Each SDK's iOS privacy manifest (`PrivacyInfo.xcprivacy`) is verified in M7 and again at M8. App Store privacy labels and the Google Play Data safety form are filled from the data inventory in `07`.
- IAP: non-consumables only. Local receipt validation via Unity IAP's validator (obfuscated tangle files are generated and git-ignored). Restore purchases on iOS.
- The save file has no secrets. Remote config cannot grant purchases.

---

## 19. Architecture decision records (summary — full entries in `10_DECISION_LOG.md`)

| ID | Decision |
|---|---|
| D-003 | URP Forward, one real-time light, no real-time DOF |
| D-004 | 2.5D PhysX bodies locked to z = 0; 2D physics is the documented fallback |
| D-005 | Kinematic swept arrow with explicit impulses; preview and flight share `BallisticSolver` |
| D-006 | Fixed Δt 1/60, max Δt 0.1, solver 8/2, enhanced determinism on, bodies start asleep |
| D-007 | Input via `Pointer.current` polling + the `BowInputReader` adapter; the default `InputSystem_Actions` asset is removed |
| D-008 | Cosmetic debris collides only with `Environment` and never deals damage |
| D-009 | Ropes = joint constraint + trigger capsule + LineRenderer |
| D-010 | LevelData SO + layout prefab; JSON only for saves |
| D-011 | Restart / next = re-instantiate the layout prefab in Gameplay, no scene reload |
| D-012 | uGUI + TMP for runtime UI |
| D-013 | No Addressables in MVP |
| D-028 | Single Runtime asmdef + an Integrations asmdef for vendor SDKs |
| D-029 | Static `Services` locator populated by `ServiceInstaller` (no DI framework) |
| D-030 | `GameEvents` static bus for cross-cutting listeners only |
| D-080 | Unity 6000.6.5f1 locked for the MVP (no upgrade spike) |
| D-086 | Spring Plate cut from the MVP |
| D-090 | Firebase (Analytics, Crashlytics, Remote Config) + Unity LevelPlay + Unity IAP behind interfaces |
| D-096 | UK/EEA consent gating of analytics, ad personalisation and crash reporting |
| D-103 | Locked technical direction |
| D-052 | World codes + `Env_`/`Marker_`/`Lvl_Pattern_` naming |
| D-060 | No gameplay fracture; broken objects spawn cosmetic debris only |
| D-061 | `TimeScaleController` is the only writer of `Time.timeScale` |
| D-070 | Enum values are append-only |
| D-074 | Canonical solvability tolerance (`07` §5.1) |
