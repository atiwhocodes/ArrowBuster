# 04 — Level Pipeline

> Owner: Content / Level Design Engineer (`LEVEL`). Design authority: `PO`. Status: Draft v1 — 2026-10-09.
> Sources: [`/mvp.md`](../../mvp.md) §3–7, §10; names from [`01_TECHNICAL_ARCHITECTURE.md`](01_TECHNICAL_ARCHITECTURE.md); decisions in [`10_DECISION_LOG.md`](10_DECISION_LOG.md) (notably D-010, D-011, D-014, D-023, D-024, D-037–D-040, D-045–D-047, D-050).
> Related docs: physics rules → [`03_PHYSICS_AND_OBJECTS.md`](03_PHYSICS_AND_OBJECTS.md); vertical slice levels → [`11_VERTICAL_SLICE.md`](11_VERTICAL_SLICE.md); QA solvability suite → [`07_QA_PERFORMANCE_RELEASE.md`](07_QA_PERFORMANCE_RELEASE.md); skill → [`../skills/level-design-and-validation.md`](../skills/level-design-and-validation.md).

---

## 1. Principles

1. **A level is data plus a prefab, never code** (D-010). If a level seems to need a script, either it needs a new reusable component (request it from `PROPS`/`PHYS` through a ticket) or the design is wrong.
2. **Every level has a recorded intended solution** (`IntendedShot` list). The automated solvability bot replays it with aim jitter. A level that fails the bot is not done.
3. **Graybox all levels of a world before art.** Art comes in as prefab-variant swaps that never change colliders or mass (`01` §12).
4. **One new idea per level, two tutorial levels per new object** (§4, §6): first an obvious use, then a combination with something already known.
5. **No repeated layout or solution pattern within 10 levels** (§6, P-03). Enforced by the `solutionArchetype` + `mechanicTags` warnings (rule W-04).
6. **Fair over clever.** The intended aim window is ≥ 8% of screen width at target distance (§7). Never require a pixel-perfect shot for 3★.

---

## 2. LevelData schema v2

`LevelData` (`Scripts/Runtime/Levels/LevelData.cs`) evolves from the M0 version in **AB-016**:
- Public fields become `[SerializeField] private` fields with read-only properties.
- Renamed fields carry `[FormerlySerializedAs("<oldName>")]` so the existing `W1_L01` asset keeps its data.
- `StarsFor()` moves to `StarRules.Compute()` (D-024). A `StarsFor` wrapper is kept so `LevelDataTests` still compiles. It is marked `[Obsolete]` and removed in M4.

### 2.1 Field table

| # | Field (serialized) | Type | Default | Validation | Purpose |
|---|---|---|---|---|---|
| **Identity** |
| 1 | `_worldId` (was `worldId`) | `int` [1..3] | 1 | Must match the file name `W<w>_L<nn>` (V-02) | World membership |
| 2 | `_levelNumber` (was `levelNumber`) | `int` [1..20] | 1 | Must match the file name (V-02) | World-local number (D-047) |
| 3 | `LevelId` (property, not serialized) | `string` | `"W1_L01"` | — | Analytics `level_id`, save key, doc file name |
| 4 | `GlobalIndex` (property, exists) | `int` | — | — | Analytics `global_level`; "L28" in docs |
| 5 | `_displayNameKey` (was `displayName`) | `string` | `""` | Warn if empty at status ≥ Art | `UIStrings` key, e.g. `lvl.w1_l05.name` |
| **Layout** |
| 6 | `_layoutPrefab` (was `layoutPrefab`) | `GameObject` | null | **Error** if null or the root lacks `LevelLayout` (V-01) | Geometry: `Lvl_W<w>_L<nn>` |
| **Quiver** |
| 7 | `_quiver` (was `quiver`) | `List<QuiverEntry>` (ordered) | `[Oak×3]` | Total 1–7 (V-03). Special types only from their first-appearance level on (V-11). | Curated quiver, consumed in list order (D-023) |
| 8 | `TotalArrows` (property, exists) | `int` | — | — | HUD, star math |
| **Stars** |
| 9 | `_goldPar` (was `goldPar`) | `int` ≥ 1 | 1 | 1 ≤ par ≤ TotalArrows (V-04); `TotalArrows − par` in [1,3] (W-03) | 3★ threshold; 2★ = par + 1 |
| **Objectives meta** (auto-synced from the layout by the validator — never hand-edited) |
| 10 | `_objectiveSummary` | `List<ObjectiveSummaryEntry>` {`ObjectiveKind kind`, `int count`} | empty | Must equal the layout's `Objective` components (V-05) | HUD icons before load, world-map preview, analytics |
| 11 | `_protectedSummary` | `List<ProtectedKind>` | empty | Must equal the layout's `ProtectedObject` components | Level-start banner ("Protect the vase") |
| **Teaching** |
| 12 | `_isTutorial` | `bool` | false | W-08 | Marks the 2 intro levels of each mechanic |
| 13 | `_tutorialPrompt` (was `tutorialPrompt` string; FormerlySerializedAs + migration copies text into `textKey`) | `TutorialPromptData` | none | V-13 if `textKey` is set | Optional callout (see §7) |
| 14 | `_showGhostHand` | `bool` | false (true only on W1_L01) | — | Onboarding drag animation until the first draw |
| 15 | `_revealsArrow` + `_revealArrowType` | `bool` + `ArrowType` | false | Must match D-014 levels (V-11) | Shows the arrow reveal card before the first draw |
| 16 | `_trajectoryPreviewScale` | `float` [0.4..1] (M0 code has `Range(0.3,1)`; AB-016 raises the floor, D-073) | 1 | **Must be 1.0 for global 1–15** (V-14) | Multiplies `GameplayTuning.previewBaseSeconds` |
| **Intended solution** |
| 17 | `_intendedShots` | `List<IntendedShot>` | empty | Required at status ≥ Graybox; count = par; types follow quiver order (V-12) | Solvability bot input; QA reference |
| 18 | `_aimToleranceDeg` | `float` | 0 (auto) | ≥ the computed 8% window (W-10) | Override for bot jitter; 0 = derived from target distance (§6.3) |
| 19 | `_powerTolerance` | `float` | 0.03 | 0.02–0.06 | Bot power jitter |
| **Meta / economy** |
| 20 | `_bullseyeEnabled` | `bool` | auto | Must match whether a `BullseyeMarker` exists (V-17) | Bullseye medal (D-045) |
| 21 | `_allowRewardedArrow` | `bool` | true | — | Designer opt-out for the fail-screen +1 arrow. Early levels are excluded at runtime by `ads.rewarded_arrow.min_global_level` (default 4). `AdPolicy` rules: `06` §7.1 |
| 21a | `_rewardedArrowViableWithTwo` | `bool` | false | — | Designer asserts one good shot can still clear two remaining objectives (`06` §7.1 viable-state heuristic) |
| 21b | `_bonusArrowType` | `ArrowType` | Oak | Must be in the level's quiver types or Oak | Type of the rewarded +1 arrow (D-062) |
| 21c | `_expectedResolveSeconds` | `float` | 0 (auto from bot) | 0–10 | Bot/QA expectation: the intended solution's win fires within this time after the last impact (`02` §9 AC2) |
| **Classification** |
| 22 | `_isSetPiece` | `bool` | false | Must be true for every 5th global level (W-11) | Cinematic framing, bigger budget |
| 23 | `_isBoss` | `bool` | false | Only L20/L40/L60 | Boss presentation; quiver/par exceptions |
| 24 | `_mechanicTags` | `List<MechanicTag>` | empty | ≥ 1 | Repetition and introduction checks, analytics dims |
| 25 | `_introducesTags` | `List<MechanicTag>` | empty | ⊆ `_mechanicTags`; a tag may be introduced only once in the game (V-20) | Two-tutorial-levels cadence check (W-05) |
| 26 | `_solutionArchetype` | `SolutionArchetype` | `DirectHit` | W-04 | Repetition guard (P-03) |
| 27 | `_difficulty` | `int` [1..5] | 1 | — | Difficulty curve tracking (§12) |
| **Production** |
| 28 | `_status` | `LevelStatus` {Draft, Graybox, Art, Final} | Draft | Gates which rules are Errors (§5) | Delivery tracker |
| 29 | `_designerNotes` | `string` (TextArea) | `""` | — | Free notes. The full solution doc lives in `docs/levels/W#_L##.md`. |
| **Reserved (unused in MVP)** |
| 30 | `_choiceSlots` | `List<ArrowType>` | empty | Must be empty in MVP | Choice levels (D-046) |
| 31 | `_allowSwap` | `bool` | false | Must be false in MVP | Tap-to-swap arrow (D-023 checkpoint) |

### 2.2 Supporting types (all in `Scripts/Runtime/Levels/`)

```csharp
[Serializable] public struct IntendedShot {           // IntendedShot.cs
    public ArrowType arrowType;     // must equal the quiver entry consumed at this step
    public float angleDeg;          // aim angle from +X, 8..172 (D-018 cone)
    public float power01;           // 0.15..1
    public float delayAfterPrevious;// seconds after the previous release (≥ GameplayTuning.renockCooldown, 0.35)
    public string note;             // "cuts the left rope"
}
[Serializable] public struct TutorialPromptData {     // TutorialPromptData.cs
    public string textKey;          // UIStrings key, e.g. "tut.w1_l07.rope"
    public string anchorId;         // TutorialAnchor.id inside the layout ("" = screen centre-top)
    public TutorialTrigger trigger; // OnLevelStart | OnFirstDrawStart | AfterMisses | AfterFirstShotResolved
    public int missCount;           // used by AfterMisses (default 2)
    public TutorialDismiss dismiss; // OnFirstDraw | OnArrowFired | OnTap | AfterSeconds
    public float seconds;           // used by AfterSeconds (default 4)
}
[Serializable] public struct ObjectiveSummaryEntry { public ObjectiveKind kind; public int count; }
public enum LevelStatus { Draft, Graybox, Art, Final }
public enum MechanicTag { Crest, SupplyCrate, Lantern, CursedOrb, Dummy, BannerRope, Rope, Chain,
    Straw, Timber, Stone, Ice, Metal, PowderBarrel, Balloon, OilJar, FireZone, Boulder, Lever,
    SpringPlate, Shield, WindFan, Portal, MovingPlatform, RoyalVase, SleepingFox, RoyalRelic,
    WaterPit, SpikeBed, Pit, ShieldedTarget, Heavyhead, Split, Fire, Bounce }
public enum SolutionArchetype { DirectHit, WeakSupport, RopeDrop, Restraint, ChainBlast,
    OrderOfOperations, DelayedReaction, TimingShot, MultiTarget, Sweep, Lever, WindCurve,
    BankShot, PortalShot, ToolSelection, MultiStep }
```

These enums go in `Levels/` because only level tooling and analytics use them. `GameEnums.cs` (Core) stays for gameplay-wide enums. `MechanicTag` and `SolutionArchetype` live in `Levels/LevelEnums.cs`.

### 2.3 `LevelLayout` (component on the layout prefab root)

| Field | Type | Default | Purpose |
|---|---|---|---|
| `_framing` | `CameraFraming` {`Vector2 centerOffset`, `float widthMeters`, `float pitchDeg`} | (0,0), 10, 8 | Per-level override for `CameraFramer` (D-041). Set pieces may use 11–12 m width. |
| `_clearLineEnabled` / `_clearLineY` | `bool` / `float` | false / 2.5 | D-039 clear line (CrestTarget, SupplyCrate, TrainingDummy only) |
| `_playBounds` | `Rect` | x −6.5..6.5, y −2..22 | `PlayBounds` removal region (pit semantics) |
| `_environmentRoot` | `Transform` | child `Environment` | Static blocking: ledges, pillars, ground (layer `Environment`) |
| `_gameplayRoot` | `Transform` | child `Gameplay` | All dynamic/interactive prefab instances |
| `_decorRoot` | `Transform` | child `Decor` | Non-colliding dressing (removed on the Low tier if flagged `LowTierStrip`) |
| `_tutorialAnchors` | `TutorialAnchor[]` (auto-collected) | — | Callout targets by `id` |
| `_backdropVariant` | `int` | 0 | Picks one of `WorldData.backdrops` (variety without new art) |

**Required hierarchy** (enforced by V-21):

```
Lvl_W1_L05 (LevelLayout)
├── Environment   static blocking prefabs (Env_*) – layer Environment, no Rigidbody
├── Gameplay      library prefab instances only (Struct_*, Obj_*, Prot_*, Prop_*, Haz_*, Marker_*)
└── Decor         optional, colliders forbidden
```

### 2.4 `WorldData` and `LevelCatalog`

| Asset | Path | Fields | Owner |
|---|---|---|---|
| `WorldData` (`WD_Greenwood`, `WD_Sunscar`, `WD_Frostspire`) | `ScriptableObjects/Worlds/` | `worldId`, `nameKey`, `levels` (`LevelData[20]`, ordered), `unlockRequiresClearsInPrevious` (15, from `GameConstants.LevelsToUnlockNextWorld`), `environmentPrefab` (backdrop diorama), `backdrops` (variants), `music` (`SoundEvent`), `mapBackdrop` (Sprite), `themeColor`, `completionReward` (`CosmeticDefinition`) | SYS schema, LEVEL content |
| `LevelCatalog` | `ScriptableObjects/Levels/` | `worlds` (`WorldData[]`), `playlists` (`LevelPlaylist[]` {`id`, `LevelData[]`}) | LEVEL |
| Catalog instances | — | `LC_Main` (the game), `LC_VerticalSlice` (VS-01..05, D-050), `LC_DebugAll` (every level, any status, dev builds only) | LEVEL |

`LevelCatalogBuilder` (editor) rebuilds `WorldData.levels` from the `ScriptableObjects/Levels/World<n>/` folders sorted by `LevelId`. It errors on gaps or duplicates. **Nobody hand-edits the level arrays.**

---

## 3. Level scene / prefab strategy

- Levels are **prefabs instantiated into `Gameplay.unity` under `LevelRoot`** (D-011). There are no level scenes.
- Each layout is a **plain prefab** (not a variant) created from `Lvl_Template_Base`. That template contains `LevelLayout`, the 3 group roots, a default `Env_Ground_12x1` and a `Haz_Pit` under the board.
- Inside `Gameplay/`, every object is a **library prefab instance** (nested prefab). Overrides are limited to: transform (position/rotation in the XY plane), prefab-exposed tuning fields (rope length, mover speed/phase, objective clear-rule overrides marked `[LevelTunable]`) and object names. **Mesh, collider, mass and material overrides are forbidden** (V-08, enforced by `PrefabValidator`).
- **Snapping:** positions on a 0.05 m grid, rotations in 5° steps, z = 0 exactly. `LevelEditorWindow` applies the snapping. MCP authoring must pass snapped values.
- **Stacks are authored touching, never overlapping** (V-10). Use the "Settle Preview" button (§8.1) to bake a resting pose before saving.
- **Pattern starters:** `Lvl_Pattern_<Name>` prefabs in `Prefabs/Levels/_Patterns/` hold reusable starting compositions (Weak Leg, Hanging Load, Safe Cargo, Boulder Run, Sky Drop, Burning Bridge, Mirror Gate, Rift Shot — the §7 templates). A designer duplicates a pattern's `Gameplay` children into a new level and then makes it original. Patterns are never shipped as levels. *(This naming is a proposed addition to `01` §8.)*

---

## 4. Creating a level without new code

### 4.1 In the Unity Editor (designer)

1. **Check the tracker** (§11). Confirm the beat, mechanics, quiver and par. Write the intended-solution doc first (`docs/levels/W#_L##.md`, template §14). Status Draft.
2. `Arrow Buster ▸ Levels ▸ Level Editor` → **New Level** → pick world, number and (optionally) a pattern. The wizard creates:
   - `ScriptableObjects/Levels/World<w>/W<w>_L<nn>.asset`
   - `Prefabs/Levels/World<w>/Lvl_W<w>_L<nn>.prefab` (from `Lvl_Template_Base`, plus pattern children)

   It then wires `_layoutPrefab`, opens the prefab in prefab mode and opens the palette.
3. **Block out** with the palette (drag `Env_*`, `Struct_*`, `Obj_*`, `Prot_*`, `Prop_*`, `Haz_*`, `Marker_*`). Snapping is automatic. Keep the structure zone y ≈ 4–15. The bow pivot is at (0, 1.5).
4. **Set rules** in the `LevelDataInspector`: quiver (ordered), gold par, tags, archetype, tutorial prompt, preview scale. Click **Sync from Layout** (fills the objective/protected summaries and bullseye).
5. **Settle Preview** (simulate 3 s in edit mode, show drift, optionally bake the poses).
6. **Play** (`▶ Play This Level`) → shoot → when you win with the intended solution, **ShotRecorder ▸ Save as Intended** (writes `_intendedShots`).
7. **Validate** (`Validate This Level`) — all Errors must be fixed. Run **Bot Check** (runs the PlayMode solvability + jitter + idle tests for this level only, about 20 s).
8. Set status **Graybox**. Update the tracker row and the solution doc (measured aim windows from the bot report).
9. Hand off for review (skill `level-design-and-validation.md`). `PO` signs off design, `QA` signs off solvability.

### 4.2 Via Claude Code + MCP (agent)

Use the slash command `/new-level` (source: `docs/claude-commands/new-level.md`) or this sequence:

1. Read the tracker row, `mvp.md` §6–7, this doc §5–6. Propose the layout (ASCII), quiver, par and intended solution, then **wait for PO approval**.
2. `manage_scriptable_object` → create `W<w>_L<nn>` (type `LevelData`) in `ScriptableObjects/Levels/World<w>/`.
3. `manage_prefabs` → instantiate `Lvl_Template_Base`, save as `Lvl_W<w>_L<nn>`, add library prefab instances under `Gameplay` with snapped positions (`manage_gameobject` with exact transforms; z = 0).
4. `manage_scriptable_object` → set `_layoutPrefab`, quiver, par, tags, archetype, prompt.
5. `execute_menu_item` → `Arrow Buster/Levels/Sync From Layout`, then `Arrow Buster/Levels/Validate Selected`.
6. `read_console` → fix all Errors.
7. Intended shots: compute the candidate angle/power analytically or by `ShotRecorder` batch search (`Arrow Buster/Levels/Search Shot` — sweeps angle/power with `BallisticSolver` against the target point and outputs candidates). Write them into `_intendedShots`.
8. `run_tests` (PlayMode, filter `LevelSolvabilityTests.W<w>_L<nn>*`) → must pass.
9. Write `docs/levels/W<w>_L<nn>.md`. Update the tracker row. Report using `docs/agents/HANDOFF_TEMPLATE.md`.

Agents **must not** create new MonoBehaviours for a level. Missing behaviour → escalate to `PROPS`/`PHYS` (ticket).

---

## 5. Required metadata by status

| Status | Must be present (Errors if missing) |
|---|---|
| Draft | identity, layout prefab, quiver, par, ≥ 1 objective |
| Graybox | + intended shots, mechanic tags, archetype, difficulty, solution doc, all V-rules pass, bot passes (P-01..P-04) |
| Art | + art prefab variants swapped, `displayNameKey`, tutorial strings in `UIStrings`, Low-tier perf check (P-05) |
| Final | + PO sign-off, QA device pass (on-device intended solution reproduced on Mid and Low tier), designer notes cleared of TODOs |

---

## 6. Quiver, objectives and star par

### 6.1 Quiver configuration
- An ordered `List<QuiverEntry>`, consumed front to back (D-023). The HUD shows the next arrow.
- Put **special arrows where the intended solution uses them**. Example: Boulder-style levels are `[Oak, Heavyhead, Oak]` if the Heavyhead is the second shot.
- Total arrows ≤ 7 (fits the HUD; under `MaxActiveArrows` 8).
- Remote tuning: a whitelisted per-level `extra_oak` override may add Oak arrows at the end of the quiver for emergency balance (key spec in [`06`](06_META_MONETISATION_ANALYTICS.md)). It never removes arrows and never changes par.

### 6.2 Objective definitions (placed as prefabs; rules per D-037)

| Kind | Prefab | Clears by (default) | Designer-tunable | Notes |
|---|---|---|---|---|
| CrestTarget | `Obj_CrestTarget` | Arrow hit; impact ≥ `breakImpulse`; kill zone / clear line | `breakImpulse` preset Low/Med/High | Red, crest icon + outline |
| SupplyCrate | `Obj_SupplyCrate_1x1`, `_2x1` | HP break; centre below clear line; kill zone | HP preset | Same physics as `Struct_Crate_Timber`, red bands |
| HangingLantern | `Obj_HangingLantern` | Arrow hit; impact ≥ low threshold; ground contact after a fall | rope length (if hung) | Usually hung with `Prop_Rope` |
| CursedOrb | `Obj_CursedOrb` (+ `_FrostSeal` variant for L60) | Direct arrow hit only | — | Kinematic/anchored (V-16) |
| TrainingDummy | `Obj_TrainingDummy` | Tilt > 70° for 0.3 s; touches `Ground`; break | — | Comedy reaction anim |
| BannerRope | `Obj_BannerRope` | Cut or burned | length, load | A rope that is itself an objective |

Every `Objective` instance also has a designer flag **`requiresSpentProp`** (default false; set it when only a single-use prop, e.g. the one boulder, can clear this objective). `AdPolicy` uses it in the rewarded-arrow viable-state heuristic (`06` §7.1).

Protected: `Prot_RoyalVase`, `Prot_SleepingFox`, `Prot_RoyalRelic` (rules D-038). A **shielded target** is a pattern, not a type: a `CrestTarget` covered by a movable `Struct_*` piece. It is first used at L14.

### 6.3 Star-par rules
- **3★:** `arrowsUsed ≤ goldPar`. **2★:** `arrowsUsed == goldPar + 1`. **1★:** any other clear, and **any clear that used a rewarded bonus arrow** (D-024).
- `goldPar` = the number of arrows in the **intended solution** (the cleverest fair solution, not a lucky one).
- Quiver buffer = `TotalArrows − goldPar`:

| Level type | Typical quiver / par (§7) | Buffer |
|---|---|---|
| Tutorial | 3 / 1 | +2 |
| Normal puzzle | 4 / 2 (or 3 / 1) | +2 |
| Set piece | 5 / 3 (or 4 / 2) | +2 |
| Boss | 4–5 / 2–3 | +1–2 (W-03 exception allowed for `_isBoss`) |
| Late tension levels (W3) | par + 1 | +1 (allowed, warn only) |

- **Aim window rule (§7):** every intended shot must succeed across a window of ≥ 8% of screen width at the target distance (≥ 12% for L1–15 and tutorial levels).
  - Half-window in metres: `h = 0.04 × 10 m = 0.4 m` (0.6 m for 12%).
  - Bot angle tolerance: `tolDeg = atan(h / d)` in degrees, where `d` = straight-line distance from the bow pivot to the shot's first intended contact. Example: d = 10 m → ±2.3° (8%) / ±3.4° (12%). Power tolerance: ±`_powerTolerance` (default 0.03).
  - The bot fires 5 variants: centre, ±angle, ±power. All must win with `arrowsUsed ≤ goldPar` (P-03).
- **Never** rely on a debris bounce, a rebound off a moving object at an exact frame, or a sub-0.5 m landing zone for a 3★ shot.

---

## 7. Tutorial callout support

| Mechanism | Where | Rules |
|---|---|---|
| Ghost hand | `_showGhostHand` (W1_L01 only) | Loops a drag-back gesture from the bow until the first `DrawStarted`. Returns after 8 s idle while Ready. |
| Callout | `_tutorialPrompt` (≤ 1 per level) | Text ≤ 32 characters (e.g. "Aim for the rope"). Points at a `TutorialAnchor` (`Marker_TutorialAnchor` prefab with `id`). Never covers the anchor's target. Used **only** in `_isTutorial` levels (§3). |
| Arrow reveal card | `_revealsArrow` (L13, L30, L36, L47) | One-tap card: arrow icon + one-line verb ("Heavyhead: hits hard, flies low"). Shown before the first draw. |
| Level-start banner | automatic from `_protectedSummary` / objective summary | "Break the targets" / "Protect the vase" for 1.2 s. Not a tutorial; all levels. |
| Miss hint | `trigger = AfterMisses` | Optional on tutorial levels: shows the callout after N misses (default 2) if it was dismissed. |

Copy is stored in `UIStrings` under the keys `tut.w<w>_l<nn>.<slug>`. `TutorialPromptController` (Gameplay) listens to `GameEvents` and is owned by `UI`/`CORE`. Seen prompts are saved (`SaveGame.tutorialSeen`) and are not repeated on retries within a session; they are shown again on a fresh visit.

---

## 8. Level editor tooling (Editor assembly, `Scripts/Editor/Levels/`)

### 8.1 `LevelEditorWindow` (UI Toolkit, `Arrow Buster ▸ Levels ▸ Level Editor`)
- **Left pane — Level list:** all `LevelData` grouped by world, with columns status, par/total, difficulty, last validation result (✔/⚠/✖), archetype. Filter by status/tag. Double-click opens the prefab.
- **Toolbar:** New Level (wizard), Duplicate, Validate This, Validate World, Validate All, Bot Check (this level), ▶ Play This Level (sets `GameplayController._editorFallbackLevel` via `EditorPrefs`, opens Gameplay, enters Play mode), Open Solution Doc.
- **Palette pane:** thumbnails of library prefabs by category (Environment, Structure, Objective, Protected, Prop, Hazard, Marker). Click-to-place at the scene-view cursor with 0.05 m snapping, z locked to 0, and rotation in 5° steps (`Q`/`E`).
- **Settle Preview:** simulates physics for 3 s in edit mode (`Physics.simulationMode = Script`), shows the drift per body (colour heat), and offers "Bake rest pose" (applies the settled transforms, rounded to the grid only if drift < 0.02 m) or "Revert".
- **Budget HUD (scene view overlay):** dynamic body count (x/60), protected-in-blast warnings, camera frame and safe-area overlay for 9:16, 9:19.5, 9:21, 3:4. Shows `clearLineY`, `PlayBounds` and the bow pivot.

### 8.2 `LevelDataInspector` (custom inspector for `LevelData`)
- Header card: `LevelId`, global index, status dropdown, difficulty stars, validation summary.
- Quiver editor: reorderable list with arrow icons. Shows "3★ ≤ par, 2★ = par+1" live, and the buffer with colour (W-03).
- Objectives/protected summary (read-only) + **Sync from Layout**.
- Intended shots: table + per-row **Ghost Preview** (draws that shot's arc in the scene view using `BallisticSolver`) + **Replay All** (Play mode, auto-fire) + measured aim window from the last bot run.
- Tutorial prompt: key picker from `UIStrings`, live text preview, anchor dropdown populated from the layout's `TutorialAnchor` ids.

### 8.3 `ShotRecorder` (Runtime part `#if AB_DEV` + Editor part)
- Records every `ArrowFired` (`arrowType`, `angleDeg`, `power01`, time since the previous release) during Play mode.
- On `LevelWon`, offers "Save as Intended (n shots, ★x)". It refuses if stars < 3.
- **Search Shot** (editor): given a target `Transform` and arrow type, sweeps angle 8°–172° and power 0.15–1 with `BallisticSolver` (no physics) and lists shots whose preview arc passes within 0.15 m of the target, ranked by the widest tolerance. Used by agents to seed intended shots.

### 8.4 `LevelValidator` (static, Editor)
- Runs the V/W rules (§9) on one level, a world or all levels.
- Entry points: menu, `LevelEditorWindow`, `BuildScript` pre-build (Errors abort the build; `01` §17), EditMode test `LevelValidatorTests.AllLevelsPassStaticRules` (CI).
- Output: console entries with a clickable context object, plus `Logs/level_validation.json` (consumed by the tracker update script).

### 8.5 `LevelCatalogBuilder`
- `Arrow Buster ▸ Levels ▸ Rebuild Catalog`: fills `WorldData.levels` and the `LC_DebugAll` playlist. It verifies 20 levels per world once status ≥ Graybox for all, and writes a Markdown tracker snapshot to `docs/levels/_tracker_snapshot.md` (generated; do not hand-edit).

---

## 9. Validation rules

Severity: **E** = Error (blocks status ≥ Graybox and builds), **W** = Warning (must be acknowledged in designer notes at status Final). PlayMode rules (P-xx) run in the test runner.

### 9.1 Static rules (editor, fast)

| ID | Sev | Rule |
|---|---|---|
| V-01 | E | `_layoutPrefab` is set and its root has `LevelLayout`. |
| V-02 | E | The asset name is `W<w>_L<nn>` and matches `_worldId`/`_levelNumber`. `LevelId` is unique across the catalog. |
| V-03 | E | The quiver is non-empty. Each count ≥ 1. `TotalArrows` is in 1..7. |
| V-04 | E | 1 ≤ `goldPar` ≤ `TotalArrows`. |
| V-05 | E | ≥ 1 `Objective` in the layout. `_objectiveSummary` and `_protectedSummary` equal the layout contents. |
| V-06 | E | Every `Rigidbody` under `Gameplay` has `PlanarBody`, transform z = 0 (± 0.001) and the planar constraints (D-004). |
| V-07 | E | Dynamic (non-kinematic) gameplay bodies ≤ 60. **W** above 45. |
| V-08 | E | Every child of `Gameplay`/`Environment` is a prefab instance from `Prefabs/{Structures,Objectives,Protected,Props,Hazards}` or `Env_*`/`Marker_*`. No unpacked instances, no missing prefabs. No mesh/collider/mass/material overrides. |
| V-09 | E | Layers match the prefab category (`Obj_*` → Objective, `Prot_*` → Protected, `Prop_*` → Prop (rope collider → Rope, portal trigger → Portal), `Struct_*` → Structure, `Env_*` → Environment, `Haz_*` → KillZone) (D-048). |
| V-10 | E | No initial penetration > 0.005 m between gameplay colliders (`Physics.ComputePenetration`). |
| V-11 | E | Special arrows appear in the quiver only at or after their first appearance (Heavyhead ≥ L13, Split ≥ L30, Fire ≥ L36, Bounce ≥ L47, D-014). `_revealsArrow` is true exactly on those four levels. |
| V-12 | E (≥ Graybox) | `_intendedShots.Count == goldPar`. The shot arrow types equal the quiver order prefix. `delayAfterPrevious` ≥ 0.35 s. Angles are in the 8°–172° cone. Power is in 0.15–1. |
| V-13 | E | If `_tutorialPrompt.textKey` is set: the key exists in `UIStrings`, the text is ≤ 32 characters, and `anchorId` resolves (or is empty). |
| V-14 | E | `GlobalIndex ≤ 15` ⇒ `_trajectoryPreviewScale == 1`. Otherwise ≥ 0.4 (D-073; never removed, §3). |
| V-15 | E | If the clear line is enabled: every objective's start bounds min y > `clearLineY` + 0.3, and every protected object's start bounds min y > `clearLineY` + 0.3. |
| V-16 | E | Every `Obj_CursedOrb` is kinematic or jointed to `Environment`. |
| V-17 | E | ≤ 1 `Marker_Bullseye`. `_bullseyeEnabled` equals whether it exists. |
| V-18 | E | Ropes have a valid anchor and load. Portal pairs are complete. Movers have a period > 0. Wind fans have the force direction in the XY plane. |
| V-19 | E | All objectives, protected objects and the bow's aim zone lie inside the 9:16 camera frame at the level's framing. Nothing required lies under the top HUD band (top 9%). |
| V-20 | E | Each `MechanicTag` appears in `_introducesTags` of exactly one level in the catalog. |
| V-21 | E | The layout hierarchy is `Environment` / `Gameplay` / `Decor`. `Decor` has no colliders. |
| V-22 | E (MVP) | `_choiceSlots` is empty and `_allowSwap` is false. |
| V-23 | E/W | Per-level physics budgets (`03` §13): joints ≤ 12 (W) / ≤ 20 (E); ropes ≤ 8; balloons ≤ 9; movers ≤ 4; portal pairs ≤ 2; powder barrels ≤ 6; wind fields ≤ 3 (E above each cap). |
| V-24 | E | Every `HingeJoint` axis is ±Z (planar rotation only). |
| V-25 | E | No `KinematicMover` sweep path intersects a resting dynamic body's start bounds (movers must not shove stacks at load). |
| V-26 | E | Every point below the lowest platform inside `PlayBounds` is covered by a `KillZone` or the `PlayBounds` pit (no body can come to rest off-screen). |
| V-27 | E | `MaterialKind.Earth` (D-059) appears only on `Environment` colliders. |
| W-01 | W | Mass ratio > 10:1 between bodies whose colliders touch at start. |
| W-02 | W | A protected object is within a `Prop_PowderBarrel` blast radius with dynamic bodies between the barrel and it (D-020). |
| W-03 | W | `TotalArrows − goldPar` ∉ [1, 3] (except `_isBoss`, where [1,2] is the norm). |
| W-04 | W | The same `_solutionArchetype` **and** the same primary tag (first entry of `_mechanicTags`) appear in another level within ±10 global indices (P-03). |
| W-05 | W | A tag introduced at level N does not appear in levels N+1..N+3 (two-tutorial-levels rule). |
| W-06 | W | A tag is used before the level that introduces it. |
| W-07 | W | A protected object prefab is missing `ProtectedOutline` (purple outline + icon). |
| W-08 | W | `_isTutorial` is true but there is no tag in `_introducesTags`, or vice versa within the first 2 uses. |
| W-09 | W | Status Final but `docs/levels/<LevelId>.md` is missing or still has `TODO`. |
| W-10 | W | The bot-measured aim window < 8% (< 12% for L1–15). |
| W-11 | W | `GlobalIndex % 5 == 0` but `_isSetPiece` is false, or vice versa. |
| W-12 | W | `_difficulty` jumps by > 2 from the previous level (except after a boss). |

### 9.2 PlayMode rules (`Tests/PlayMode`, owned by QA, run per level)

| ID | Sev | Test | Pass criteria |
|---|---|---|---|
| P-01 | E | `LevelIdleStabilityTests` | Load, simulate 3 s without shooting: no objective cleared, no protected lost, max body displacement < 0.01 m, calm state reached ≤ 1 s. |
| P-02 | E | `LevelSolvabilityTests` (centre) | Replay `_intendedShots` → `LevelWon`, stars = 3. |
| P-03 | E | `LevelSolvabilityTests` (jitter) | 9-sample grid per shot (nominal, ±δθ, ±δp, 4 corners; [`07` §5.1](07_QA_PERFORMANCE_RELEASE.md#51-tolerance-definition), D-074): all samples win; nominal + ≥ 7/9 within `goldPar`. |
| P-04 | E | Settle time | Win declared ≤ 4.0 s after the final intended shot resolves. |
| P-05 | W | Perf sample | Peak physics step time and awake-body count recorded. Flagged if > 2× the median of its world. The Low-tier device check is mandatory for flagged levels. |
| P-06 | E | No soft-lock | Firing every arrow straight up (angle 90°, power 1.0) ends in `LevelFailed` within `TotalArrows × 3 s + 5 s`. |
| P-07 | E | Restart | `LevelLoader.Load` twice in a row → identical body count and positions (determinism of a fresh load) and ≤ 300 ms in the editor. |

---

## 10. Level test checklist (per level, before status Final)

- [ ] Static validation: 0 Errors; Warnings acknowledged in notes.
- [ ] P-01..P-07 green in the test runner (attach the report line to the solution doc).
- [ ] 5-second read test: a person new to the level names the objective and one weak point in 5 s (2 of 2 testers).
- [ ] Intended solution reproduced **by hand** 3/3 times in the editor and 2/2 on a Mid device and 1/1 on a Low device.
- [ ] At least one alternative (less efficient) solution exists and yields 1–2★ (no "only one way" levels except tutorials).
- [ ] Failure readable: protected loss shows the right reason; out-of-arrows fails within 5 s of calm.
- [ ] No objective or protected object hidden by HUD, particles or the bow at any aspect ratio (9:16, 9:21, 3:4).
- [ ] Colour language: red required, purple protected, green/gold helpful props, gray/blue structure (§4). The colour-assist outline mode looks correct.
- [ ] Physics count within budget. No visible jitter at rest. Debris clears within 2 s.
- [ ] Tutorial copy ≤ 32 characters, appears/dismisses correctly, doesn't block the shot line.
- [ ] Analytics: `level_started`, `arrow_fired` (×n), `object_triggered` for props, `level_completed`/`level_failed` fire once with correct params (Debug sink CSV).
- [ ] Tracker row and solution doc updated (archetype, measured windows, status).

---

## 11. 60-level delivery tracker

Legend: **Q** = quiver **in firing order** (D-023; O = Oak, H = Heavyhead, S = Split, F = Fire, B = Bounce). The intended shots are always the first **Par** entries of Q (V-12), so any arrow the intended solution needs sits within the first Par slots. **Par** = gold par, **★** set piece, **⚔** boss, **New** = mechanic introduced (tutorial level 1 = obvious, 2 = combined). All rows start **Not started** (W1_L01 has an M0 placeholder asset). Quivers and pars are planning targets; final values come from bot runs and playtests.

### World 1 — Greenwood Range (teach the core collapse loop)

| # | ID | §6 beat | Introduced (New) / practised | Objectives | Q | Par | Flag | VS | Status |
|---:|---|---|---|---|---|---:|---|---|---|
| 1 | W1_L01 | 1–3 Direct hits | **New:** Crest, Oak, preview, Timber (static stump) | Crest ×1 | 3O | 1 | tutorial | VS-01 | Not started |
| 2 | W1_L02 | 1–3 | Crest (2nd use: two heights) | Crest ×2 | 3O | 2 | tutorial | | Not started |
| 3 | W1_L03 | 1–3 | **New:** Training dummy; first chain (post topples onto the dummy) *(M2 graybox uses stand-ins, AB-046; upgraded in AB-064)* | Dummy ×1 | 3O | 1 | tutorial | | Not started |
| 4 | W1_L04 | 4–6 Crate towers | **New:** Timber crate tower / weak-support collapse (skill, no new object) | Crest ×2 | 3O | 1 | tutorial | VS-02 | Not started |
| 5 | W1_L05 | 4–6 | **New:** Supply crate, Water pit (clear route) | Supply crate ×3 | 4O | 2 | ★ | | Not started |
| 6 | W1_L06 | 4–6 | **New:** Straw; Supply crate (2nd) + Dummy *(M2 graybox uses stand-ins, AB-046; upgraded in AB-064)* | Supply ×2, Dummy ×1 | 4O | 2 | tutorial | | Not started |
| 7 | W1_L07 | 7–9 Ropes | **New:** Rope (hanging load) | Crest ×2 | 2O | 1 | tutorial | VS-03 | Not started |
| 8 | W1_L08 | 7–9 | **New:** Hanging lantern; Rope (2nd); Water pit (2nd: a cut lantern drops into water = cleared) *(M2 graybox uses stand-ins, AB-046; upgraded in AB-064)* | Lantern ×2 | 3O | 1 | tutorial | | Not started |
| 9 | W1_L09 | 7–9 | **New:** Banner rope; Lantern (2nd); Straw (2nd: bales under the banners) | Banner rope ×2, Lantern ×1 | 4O | 2 | tutorial | | Not started |
| 10 | W1_L10 | 10–12 Protected | **New:** Royal vase; Stone (static pedestal only) | Crest ×1 | 3O | 1 | ★ tutorial | VS-04 | Not started |
| 11 | W1_L11 | 10–12 | **New:** Cursed orb; Vase (2nd); Banner rope (2nd) | Orb ×1, Banner rope ×1 | 4O | 2 | tutorial | | Not started |
| 12 | W1_L12 | 10–12 | **New:** Sleeping fox (on a ledge) | Supply ×2 | 3O | 1 | tutorial | | Not started |
| 13 | W1_L13 | 13–15 Heavyhead | **New:** Heavyhead (reveal), Stone (dynamic) | Crest ×1 | 1H+2O | 1 | tutorial | | Not started |
| 14 | W1_L14 | 13–15 | **New:** Shielded target (stone cover over the orb); Heavyhead (2nd); Cursed orb (2nd); Stone (2nd) | Orb ×1, Crest ×1 | 1O+1H+1O | 2 | tutorial | | Not started |
| 15 | W1_L15 | 13–15 | Heavyhead consolidation (which target deserves the Heavyhead); Shielded target (2nd); Sleeping fox (2nd) | Supply ×2, Crest ×1 | 1O+1H+2O | 2 | ★ tutorial | | Not started |
| 16 | W1_L16 | 16–19 Cascades | Combined: rope → collapse, vase restraint | Crest ×3 | 4O | 2 | | VS-05 | Not started |
| 17 | W1_L17 | 16–19 | **New:** Powder barrel (obvious blast) | Supply ×3 | 3O | 1 | tutorial | | Not started |
| 18 | W1_L18 | 16–19 | Two-step cascade: rope → rolling barrel → stack; fox beside water (loss route) | Crest ×2, Dummy ×1 | 4O | 2 | tutorial | | Not started |
| 19 | W1_L19 | 16–19 | Powder barrel (2nd) + Cursed orb + Heavyhead | Orb ×1, Supply ×2 | 1O+1H+2O | 2 | | | Not started |
| 20 | W1_L20 | Boss | Three watchtowers, one powder barrel, one fox; clear in ≤ 4 arrows (§6) | Crest ×3, Lantern ×1 | 1O+1H+2O | 2 | ★ ⚔ | | Not started |

### World 2 — Sunscar Canyon (turn direct aiming into sequence planning)

| # | ID | §6 beat | Introduced / practised | Objectives | Q | Par | Flag | Status |
|---:|---|---|---|---|---|---:|---|---|
| 21 | W2_L01 | 21–23 Balloons | **New:** Balloon cluster (pop → load drops) | Crest ×1 | 3O | 1 | tutorial | Not started |
| 22 | W2_L02 | 21–23 | Balloon (2nd) + vase: pop order matters | Supply ×2 | 3O | 2 | tutorial | Not started |
| 23 | W2_L03 | 21–23 | Balloons lifting a platform with a dummy; consolidation | Dummy ×2 | 4O | 2 | | Not started |
| 24 | W2_L04 | 24–26 Oil | **New:** Oil jar, fire zone, Burnable straw/rope (delayed) | Banner rope ×1 | 3O | 1 | tutorial | Not started |
| 25 | W2_L05 | 24–26 | Oil (2nd) + rope bridge burns, load falls | Crest ×2, Supply ×1 | 4O | 2 | ★ tutorial | Not started |
| 26 | W2_L06 | 24–26 | **New:** Spike bed; Oil + vase twist | Lantern ×2 | 4O | 2 | | Not started |
| 27 | W2_L07 | 27–29 Shields | **New:** Rotating shield (Metal, ricochet) | Crest ×1 | 3O | 1 | tutorial | Not started |
| 28 | W2_L08 | 27–29 | Patrol shield (2nd) in front of a chain (chain variant of Rope, no tutorial); Spike bed (2nd) | Crest ×2 | 4O | 2 | tutorial | Not started |
| 29 | W2_L09 | 27–29 | Shield + balloons + orb consolidation | Orb ×1, Crest ×1 | 4O | 2 | | Not started |
| 30 | W2_L10 | 30–32 Split | **New:** Split Arrow (reveal): 3 crests behind a central pillar | Crest ×3 | 1S+2O | 1 | ★ tutorial | Not started |
| 31 | W2_L11 | 30–32 | Split (2nd) pops a balloon trio | Supply ×1, Crest ×1 | 1S+2O | 1 | tutorial | Not started |
| 32 | W2_L12 | 30–32 | Split + rotating shield | Lantern ×3 | 1O+2S | 2 | | Not started |
| 33 | W2_L13 | 33–35 Boulders | **New:** Rolling boulder (rope-held), lane sweep | Crest ×3 | 3O | 1 | tutorial | Not started |
| 34 | W2_L14 | 33–35 | **New:** Lever/counterweight (D-022); boulder (2nd) [opt. SpringPlate variant, D-021] | Supply ×2 | 3O | 1 | tutorial | Not started |
| 35 | W2_L15 | 33–35 | Boulder + lever + Split | Crest ×2, Dummy ×2 | 2O+1S+2O | 3 | ★ | Not started |
| 36 | W2_L16 | 36–39 Fire combos | **New:** Fire Arrow (reveal): straw wick behind shield → rope burns | Banner rope ×1, Crest ×1 | 1F+1O | 1 | tutorial | Not started |
| 37 | W2_L17 | 36–39 | Fire (2nd) + balloons | Supply ×2 | 1O+1F+1O | 2 | tutorial | Not started |
| 38 | W2_L18 | 36–39 | Fire + oil + fox restraint | Crest ×2 | 1O+1F+1O | 2 | | Not started |
| 39 | W2_L19 | 36–39 | Fire + balloons + vase + Split | Lantern ×2, Crest ×1 | 1S+1F+2O | 2 | | Not started |
| 40 | W2_L20 | Boss | Caravan siege gate: trigger boulder, ignite support rope, clear four crests in 5 arrows (§6) | Crest ×4 | 1O+1F+1S+2O | 3 | ★ ⚔ | Not started |

### World 3 — Frostspire Keep (the "clever shot" climax)

| # | ID | §6 beat | Introduced / practised | Objectives | Q | Par | Flag | Status |
|---:|---|---|---|---|---|---:|---|---|
| 41 | W3_L01 | 41–43 Ice | **New:** Ice (slides on ledges, shatters) | Crest ×1 | 3O | 1 | tutorial | Not started |
| 42 | W3_L02 | 41–43 | Ice (2nd) + Heavyhead shatter | Supply ×2 | 1H+2O | 1 | tutorial | Not started |
| 43 | W3_L03 | 41–43 | Ice + vase (harmless fragments) | Crest ×2 | 4O | 2 | | Not started |
| 44 | W3_L04 | 44–46 Wind | **New:** Wind fan lane curves light arrows | Crest ×1 | 3O | 1 | tutorial | Not started |
| 45 | W3_L05 | 44–46 | Wind (2nd) pushes balloons | Lantern ×2 | 4O | 2 | ★ tutorial | Not started |
| 46 | W3_L06 | 44–46 | Wind + Heavyhead (resists wind): which target gets the heavy shot | Orb ×1, Supply ×1 | 1O+1H+1O | 2 | | Not started |
| 47 | W3_L07 | 47–49 Bounce | **New:** Bounce Arrow (reveal), marked metal plate | Crest ×2 | 1B+2O | 1 | tutorial | Not started |
| 48 | W3_L08 | 47–49 | Bounce (2nd) cuts a hidden rope | Banner rope ×1, Crest ×1 | 1O+1B+1O | 2 | tutorial | Not started |
| 49 | W3_L09 | 47–49 | Bounce + rotating shield | Orb ×2 | 2B+1O | 2 | | Not started |
| 50 | W3_L10 | 50–52 Portals | **New:** Portal pair (entry → exit visible) | Crest ×1 | 2O | 1 | ★ tutorial | Not started |
| 51 | W3_L11 | 50–52 | Portal (2nd) + ice slide | Supply ×2 | 3O | 1 | tutorial | Not started |
| 52 | W3_L12 | 50–52 | Portal + Bounce | Crest ×2 | 1O+1B+1O | 2 | | Not started |
| 53 | W3_L13 | 53–55 Moving ice | **New:** Moving ice platform (KinematicMover) | Crest ×1 | 3O | 1 | tutorial | Not started |
| 54 | W3_L14 | 53–55 | Moving platform (2nd) + wind | Lantern ×1, Dummy ×1 | 3O | 2 | tutorial | Not started |
| 55 | W3_L15 | 53–55 | Platforms + portal set piece | Crest ×3 | 2O+1B+2O | 3 | ★ | Not started |
| 56 | W3_L16 | 56–59 Multi-system | Alternative solutions: the Heavyhead can open either the stone base or the lever; the Split finishes either route | Supply ×3 | 1H+1S+2O | 2 | | Not started |
| 57 | W3_L17 | 56–59 | Fox + portal + balloons (Bounce bank into the portal) | Crest ×2, Orb ×1 | 1O+1B+2O | 2 | | Not started |
| 58 | W3_L18 | 56–59 | **New (variant):** Royal relic (vase rules) + Split + rope | Lantern ×2, Crest ×1 | 1O+1S+2O | 2 | | Not started |
| 59 | W3_L19 | 56–59 | Fire + oil + wind + relic | Banner rope ×1, Crest ×2 | 1O+1F+2O | 2 | | Not started |
| 60 | W3_L20 | Finale | Break the frost seal (`Obj_CursedOrb_FrostSeal`) with a bank shot through a portal, then collapse the tower without hitting the royal relic (§6) | Orb ×1, Crest ×2 | 1B+1H+3O | 3 | ★ ⚔ | Not started |

**Tracker upkeep:** `LEVEL` updates the Status column (Not started → Draft → Graybox → Art → Final) in the same change as the asset. `LevelCatalogBuilder` regenerates `docs/levels/_tracker_snapshot.md` with validation and bot results for cross-checking.

---

## 12. World-by-world mechanic introduction plan

Each "New" entry gets two tutorial levels (obvious → combined) and then appears in a consolidation or twist level. "First seen" = the first level where it is gameplay-relevant.

| Mechanic | Kind | First seen (obvious) | 2nd tutorial (combined) | Consolidation/twist | Boss/finale use |
|---|---|---|---|---|---|
| Timber, Pit (out of bounds) | Material / hazard | L1 (implicit: stump + `Lvl_Template_Base` pit) | L4 | everywhere | — |
| Crest target | Objective | L1 | L2 | L4, L7 | all |
| Training dummy | Objective | L3 | L6 | L18, L23 | — |
| Weak-support collapse | Skill | L4 | L5 | L15, L16 | L20 |
| Supply crate | Objective | L5 | L6 | L12, L15 | — |
| Water pit | Hazard (kill zone) | L5 (clear route) | L8 | L18 (loss route near the fox), L22 | — |
| Straw / wicker | Material | L6 | L9 | L11 (screen in front of the orb), L24 (burnable) | L40 |
| Rope | Prop | L7 | L8 | L11, L14, L16 | L20, L40 |
| Hanging lantern | Objective | L8 | L9 | L20 | L20 |
| Banner rope | Objective | L9 | L11 | L24, L36 | — |
| Royal vase | Protected | L10 | L11 | L16, L22 | — |
| Cursed orb | Objective | L11 | L14 | L19, L29, L46 | L60 (frost seal) |
| Sleeping fox | Protected | L12 | L15 | L18, L38, L57 | L20 |
| Stone | Material | L13 (static from L10) | L14 | L15 | L20 |
| **Heavyhead** | Arrow | **L13** | L14 | L15, L19 | L20 |
| Shielded target | Pattern | L14 | L15 | L27 | — |
| Powder barrel | Prop | L17 | L19 | L35 | L20 |
| Balloon cluster | Prop | L21 | L22 | L23, L29 | — |
| Oil jar + fire zone + Burnable | Prop | L24 | L25 | L26, L38 | L40 |
| Spike bed | Hazard | L26 | L28 | L41 | — |
| Metal + Rotating/patrol shield | Material + Prop | L27 | L28 | L29, L32 | L40 |
| Chain (rope variant) | Prop visual variant | L28 (same rules as Rope, no tutorial) | — | L33 | L40 |
| **Split** | Arrow | **L30** | L31 | L32, L35 | L40 |
| Rolling boulder | Prop | L33 | L34 | L35 | L40 |
| Lever (counterweight) | Prop | L34 | L35 | L56 | — |
| Spring plate *(should-have, D-021)* | Prop | L34 (variant) | L35 | — | — |
| **Fire** | Arrow | **L36** | L37 | L38, L39 | L40 |
| Ice | Material | L41 | L42 | L43, L51 | L60 |
| Wind fan | Prop | L44 | L45 | L46, L54 | — |
| **Bounce** | Arrow | **L47** | L48 | L49, L52 | L60 |
| Portal pair | Prop | L50 | L51 | L52, L55 | L60 |
| Moving ice platform | Prop (KinematicMover) | L53 | L54 | L55 | — |
| Royal relic | Protected (vase variant) | L58 | L59 | — | L60 |

**Checks against `mvp.md`:**
- First chain reaction by L3 ✔.
- First 3★ by L4 (par-1 levels L1, L3, L4) ✔.
- Special arrow by L15 (L13) ✔.
- Every 5th level is a set piece ✔.
- Bosses match §6 ✔.
- 6 objectives, 5 materials and ≥ 8 props all introduced ✔.

---

## 13. Difficulty curve

Sawtooth per block: an intro level dips in difficulty, the combined level rises, a set piece spikes moderately, and the level after a boss is gentle. Targets (verified in playtests in M5/M9 and at soft launch via analytics):

| Metric | W1 L1–9 | W1 L10–20 | W2 | W3 | Boss levels |
|---|---|---|---|---|---|
| `_difficulty` range | 1–2 | 2–3 | 2–4 | 3–5 | 4–5 |
| Quiver size | 2–4 | 3–4 | 3–5 | 3–5 | 4–5 |
| Gold par | 1–2 | 1–2 | 1–3 | 1–3 | 2–3 |
| First-attempt clear rate (any ★) | ≥ 85% (L1 ≥ 95%) | ≥ 70% | ≥ 60% | ≥ 50% | ≥ 35% |
| Median retries to first clear | 0–1 | 1–2 | 1–2 | 1–3 | ≤ 4 |
| 3★ on first clear | 40–60% | 30–45% | 25–40% | 20–35% | 15–25% |
| Aim window (bot) | ≥ 12% | ≥ 12% (L10–15), ≥ 10% (L16–20) | ≥ 10% | ≥ 8% | ≥ 8% |
| `trajectoryPreviewScale` | 1.0 | 1.0 (≤ L15), 0.9 | 0.85–0.9 | 0.7–0.8 | as world |
| Time to first-success | 10–25 s | 20–40 s | 25–50 s | 30–60 s | 45–90 s (§12: 20–60 s typical) |

Red flags from §13 trigger a redesign: quit-before-first-shot ≥ 20%, or median retries ≥ 8. The W1 completion target is ≥ 45% of installers who reach L5, and ≥ 80% of playtesters complete W1 without help (§15).

---

## 14. Intended-solution documentation template

File: `docs/levels/W<w>_L<nn>.md` (one per level, created in Draft).

```markdown
# W1_L05 — <Display name> (global L5)
Status: Draft | Graybox | Art | Final        Difficulty: 2/5        Set piece: yes/no   Boss: no
Beat (mvp.md §6): Levels 4–6 crate towers     Introduces: SupplyCrate, WaterPit     Archetype: WeakSupport
Quiver (ordered): Oak ×4        Gold par: 2        Buffer: +2        Preview scale: 1.0
Objectives: SupplyCrate ×3      Protected: —       Bullseye: yes (marker on left stilt)

## Layout sketch (play area 10 m wide; bow at bottom centre)
    (ASCII sketch with coordinates of key pieces)

## Intended solution (matches LevelData._intendedShots)
| # | Arrow | Angle° | Power | Delay s | Aim at | Expected result |
|---|---|---|---|---|---|---|
| 1 | Oak | 64.0 | 0.72 | — | left stilt (Bullseye) | tower tilts right, 2 crates into water |
| 2 | Oak | 58.5 | 0.80 | 0.8 | remaining crate on ledge | pushed below clear line |

## Why this is the intended trick (one paragraph — the "aha")
## Alternative solutions (expected ★)
## Likely player failure modes → design response
## Tutorial copy (key + text ≤ 32 chars + anchor), if any
## Bot report (paste from LevelSolvabilityTests): window %, settle s, peak bodies, physics ms
## Playtest notes (date, n testers, first-try %, median retries, quotes)
## Change log
```

---

## 15. Reusable prefab catalogue (level-designer palette)

Naming follows `01` §8; `Env_*`, `Marker_*` and `Lvl_Pattern_*` were added there by D-052 (static blocking, authoring markers, pattern starters). The `Env_<Shape>_<Size>` blocking pieces below are **world-agnostic graybox** prefabs; their art variants take the world code prefix (`Env_GW_Ledge_4x0.5`, D-052). "Needed by" = the milestone in which the prefab must exist in graybox form. Art variants follow in the world's art milestone (M3 for VS pieces, M5/M6/M7 otherwise): `Struct_`/`Obj_`/`Prop_`/`Prot_` variants use the `_Greenwood`/`_Sunscar`/`_Frostspire` suffix (`01` §12); `Env_` variants use the world-code prefix (D-052). Asset IDs in this table match `05` §4.

| Category | Prefab | Key components | Layer | Needed by | Owner |
|---|---|---|---|---|---|
| Template | `Lvl_Template_Base` | `LevelLayout`, group roots, `Env_Ground_12x1`, `Haz_Pit` | — | M2 | LEVEL |
| Pattern | `Lvl_Pattern_WeakLeg`, `_HangingLoad`, `_SafeCargo`, `_BoulderRun`, `_SkyDrop`, `_BurningBridge`, `_MirrorGate`, `_RiftShot` | Composition only (from §7 templates) | — | M4 (first 3), M6, M7 | LEVEL |
| Environment | `Env_Ground_12x1`, `Env_Ledge_2x0.5`, `Env_Ledge_4x0.5`, `Env_Ledge_6x0.5`, `Env_Pillar_0.5x4`, `Env_Beam_6x0.4`, `Env_Stump`, `Env_Awning_6`, `Env_Perch_1.5`, `Env_Ramp_3x1` | Static colliders; `Ground` tag on walkable tops | Environment | M2 (Ramp: M7 ice slides) | LEVEL + ART |
| Structure — timber | `Struct_Crate_Timber_1x1`, `Struct_Crate_Timber_2x1`, `Struct_Post_Timber_0.5x2`, `Struct_Plank_Timber_4x0.25`, `Struct_Beam_Timber_3x0.5`, `Struct_Peg_Timber`, `Struct_Bridge_Timber` (rope-held drawbridge, P1) | `PlanarBody`, `MaterialBody(MP_Timber)`, `Breakable` | Structure | M1 (peg: M6) | PHYS |
| Structure — straw | `Struct_Bale_Straw_1x1`, `Struct_Screen_Straw_1x2` | + `Burnable` (M6) | Structure | M4 | PHYS |
| Structure — stone | `Struct_Block_Stone_1x1`, `Struct_Beam_Stone_3x0.5`, `Struct_Cover_Stone_1.5x1` (shielded-target cover) | `MaterialBody(MP_Stone)` | Structure | M4 | PHYS |
| Structure — ice | `Struct_Block_Ice_1x1`, `Struct_Slab_Ice_2x0.5` | `MaterialBody(MP_Ice)`, low friction | Structure | M7 | PHYS |
| Structure — metal | `Struct_Plate_Metal_2x0.25`, `Struct_Plate_Metal_Marked` (Bounce bank plate) | `MaterialBody(MP_Metal)`, unbreakable | Structure | M6 (marked: M7) | PHYS |
| Structure — lever | `Struct_Lever_Timber_4x0.5` (plank + `HingeJoint` to a static pivot, D-022) | | Structure | M6 | PROPS |
| Objective | `Obj_CrestTarget`, `Obj_SupplyCrate_1x1`, `Obj_SupplyCrate_2x1`, `Obj_HangingLantern`, `Obj_CursedOrb`, `Obj_CursedOrb_FrostSeal`, `Obj_TrainingDummy`, `Obj_BannerRope` | `Objective` (+ `ObjectiveClearRule`) | Objective (rope collider: Rope) | M2 (crest, crate) · M4 (lantern, orb, dummy, banner) · M7 (frost seal) | PROPS |
| Protected | `Prot_RoyalVase`, `Prot_SleepingFox`, `Prot_RoyalRelic` | `ProtectedObject`, `ProtectedOutline` | Protected | M2 (vase) · M4 (fox) · M7 (relic) | PROPS |
| Prop | `Prop_Rope`, `Prop_Rope_Chain` | `RopeCuttable`, `RopeView`, `Burnable` | Rope | M2 (chain: M6) | PROPS |
| Prop | `Prop_PowderBarrel` | `Breakable`, `PowderBarrel`, `Explosion` | Prop | M4 | PROPS |
| Prop | `Prop_BalloonCluster_2`, `Prop_BalloonCluster_3` | `Balloon` | Prop | M6 | PROPS |
| Prop | `Prop_OilJar`, `Prop_StrawWick` | `OilJar` → `FireZone`; `Burnable` | Prop | M6 | PROPS |
| Prop | `Prop_Boulder` | `PlanarBody`, `MaterialBody(MP_Stone)` (sphere) | Prop | M6 | PROPS |
| Prop | `Prop_Shield_Rotating`, `Prop_Shield_Patrol` | `KinematicMover`, `MaterialBody(MP_Metal)` | Prop (kinematic) | M6 | PROPS |
| Prop | `Prop_SpringPlate` *(should-have, D-021)* | `SpringPlate` | Prop | M6 (if on schedule) | PROPS |
| Prop | `Prop_WindFan` | `WindField` (+ fan visual) | Field | M7 | PROPS |
| Prop | `Prop_PortalPair` | `PortalPair` + 2 × `PortalRing` | Portal | M7 | PROPS |
| Prop | `Prop_Platform_Moving_Ice` | `KinematicMover`, `MaterialBody(MP_Ice)` | Environment (kinematic) | M7 | PROPS |
| Hazard | `Haz_Pit`, `Haz_WaterPit`, `Haz_SpikeBed` | `KillZone` (Pit/Water/Spikes) | KillZone | M2 (pit, water) · M6 (spikes) | PROPS |
| Marker | `Marker_TutorialAnchor` | `TutorialAnchor` (`id`) | — (no collider) | M3 | LEVEL |
| Marker | `Marker_Bullseye` | `BullseyeMarker` (radius 0.25 m) | — (queried on arrow impact) | M4 | PROPS |

**Catalogue rules:**
- A new library prefab needs a ticket, an owner from the table, a thumbnail for the palette and a `PrefabValidator` pass.
- Level designers never create library prefabs inside `Prefabs/Levels/`.
- Size suffixes are in metres (`W x H`), measured on the play plane.
