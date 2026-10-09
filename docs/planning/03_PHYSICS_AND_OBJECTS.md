# 03 — Physics and Objects

> Owners: Physics & Destruction Engineer (`PHYS`) for §1–6 and §12–14; Interactive Objects Engineer (`PROPS`) for §7–11 (objectives, object implementations, interaction matrices, protected/kill-zone behaviour); `QA` for §15 test execution (written by the owning engineer); `PO` for gameplay rules (D-037/D-038/D-039/D-020).
> Status: Draft v1 — 2026-10-09. Names follow [`01_TECHNICAL_ARCHITECTURE.md`](01_TECHNICAL_ARCHITECTURE.md). Arrow-side rules (impact resolver, arrow types) are in [`02_GAMEPLAY_SYSTEMS.md`](02_GAMEPLAY_SYSTEMS.md) §6.
> All numbers are **graybox starting values**. They are tuned in the M1 physics spike (AB-014) and in each world milestone. A changed value goes into the SO asset; this table is then updated by the owner.

---

## 1. 2.5D physics model (D-004, D-006)

| Rule | Value / mechanism |
|---|---|
| Engine | PhysX (built-in 3D physics), `Physics.simulationMode = FixedUpdate` |
| Play plane | z = `GameConstants.PlayPlaneZ` = 0 |
| Gameplay rigidbody constraints | `FreezePositionZ \| FreezeRotationX \| FreezeRotationY` — PhysX lock flags, solved inside the solver (stable stacks) |
| Enforcement | `PlanarBody.Awake()` applies the constraints, snaps z to 0 and sets `maxDepenetrationVelocity = 3`. `LevelValidator` errors on any gameplay body with \|z\| > 0.001 or missing `PlanarBody`. |
| Collider depth | All gameplay colliders are **1.0 m deep in z** (centred on z = 0). Static environment colliders are 2.0 m deep. Visual meshes may be deeper or shallower. |
| Collider shapes | Box (default), Capsule, Sphere. Convex MeshCollider only for irregular props (≤ 32 tris, convex). No concave mesh colliders on dynamic bodies. Compound shapes = one Rigidbody with several child primitive colliders. |
| Authoring grid | Positions snap to 0.05 m. Stacked pieces touch with **0 overlap and ≤ 0.005 m gap** (authoring target). `LevelValidator` runs `Physics.ComputePenetration` for all pairs at load and errors at penetration > 0.005 m (rule V-10 in `04` §9). PG-2 proves overlaps up to 0.002 m are harmless. |
| Fixed Δt / solver | 1/60 s · max allowed Δt 0.1 s · solver iterations 8 / velocity iterations 2 · enhanced determinism ON · bounce threshold 1.0 · default contact offset 0.01 · auto-sync transforms OFF (D-006) |
| Solver type | Projected Gauss-Seidel (PGS) default. **TGS is A/B-tested in AB-014.** Adopt it if the 10-crate tower drift is < 50% of PGS at ≤ +15% physics ms. |
| Start state | `LevelLoader` calls `Rigidbody.Sleep()` on every tracked body after instantiation. Bodies wake on contact/impulse. |
| Gravity | World gravity (0, −9.81, 0) for bodies. Arrows use their own `GameplayTuning.arrowGravity` (kinematic, see `02` §4). |
| Cosmetic debris | The only unconstrained dynamic bodies. Layer `Debris` (collides with Environment only). May tumble in z for spectacle (D-008). |
| Physics 2D | Not used. The `Physics2D` module stays installed but idle. **Fallback plan** if the AB-014 gates fail: a timeboxed 2-day spike of Box2D (PhysicsCore2D / Physics2D) with 3D visuals, decided by ARCH + PO (D-004). |

### 1.1 M1 physics spike gates (AB-014) — must pass before M2 level authoring

| Gate | Pass condition |
|---|---|
| PG-1 Idle stability | A 10-crate (1×1 timber) tower + 3-crate pyramid + plank bridge, idle 10 s after load: max drift < 1 cm, rotation < 0.5°, no body wakes |
| PG-2 Spawn safety | Same scene with deliberate 0.002 m authoring overlaps: no body exceeds 0.5 m/s at spawn |
| PG-3 Collapse resolve | An Oak hit on the bottom crate: the tower collapses, and calm (D-015 definition) is reached in < 3.0 s in 9/10 runs, < 4 s in 10/10 |
| PG-4 Repeatability | The same recorded shot 20× in one session (editor) → identical end-state hash (positions rounded to 1 cm) in ≥ 19/20 runs |
| PG-5 Cost | Worst collapse frame physics ≤ 5 ms on the Mid-tier device (Android A54 or iPhone 11) |
| PG-6 Z drift | No gameplay body ever has \|z\| > 0.001 m or a non-zero X/Y rotation during PG-1…PG-4 (asserted every `FixedUpdate`) |

---

## 2. Collision layers and matrix (D-048)

Constants: `Physics/PhysicsLayers.cs` (layer indices + masks). Applied by `Editor/Setup/LayerSetup.cs` (AB-002). `TagManager.asset` and `DynamicsManager.asset` are **PHYS-owned shared files** (lock before editing).

| # | Layer | Contents |
|---|---|---|
| 0 | Default | Nothing gameplay-related (validator warns) |
| 6 | Environment | Static ground, walls, platforms, ledges, kinematic movers (shields), portal frames |
| 7 | Structure | Dynamic blocks/beams/planks/crates (non-objective), levers, boulders* |
| 8 | Objective | Required objectives' physical bodies (crest, supply crate, lantern, orb, dummy) |
| 9 | Protected | Royal vase, sleeping fox, royal relic |
| 10 | Prop | Oil jar, powder barrel, boulder*, balloon hit volumes (triggers) |
| 11 | Rope | Rope/chain cut capsules (triggers) |
| 12 | Portal | Portal ring entry discs (triggers) |
| 13 | Arrow | Spent arrows only (flying arrows are kinematic sweeps, no collider) |
| 14 | Debris | Cosmetic fragments |
| 15 | KillZone | Water / spikes / pit triggers |
| 16 | Field | Wind zones, fire zones, explosion helper volumes (triggers; mostly queried by overlap) |

\* Boulders sit on `Prop` because they're "released" props in level logic. They behave like heavy Structure.

### 2.1 Layer collision matrix (✔ = collides/contacts or trigger events; — = ignored)

| | Env | Struct | Obj | Prot | Prop | Rope | Portal | Arrow | Debris | Kill | Field |
|---|---|---|---|---|---|---|---|---|---|---|---|
| **Environment** | — | ✔ | ✔ | ✔ | ✔ | — | — | ✔ | ✔ | — | — |
| **Structure** | ✔ | ✔ | ✔ | ✔ | ✔ | — | — | — | — | ✔ | — |
| **Objective** | ✔ | ✔ | ✔ | ✔ | ✔ | — | — | — | — | ✔ | — |
| **Protected** | ✔ | ✔ | ✔ | ✔ | ✔ | — | — | — | — | ✔ | — |
| **Prop** | ✔ | ✔ | ✔ | ✔ | ✔ | — | — | — | — | ✔ | ✔ |
| **Rope** | — | — | — | — | — | — | — | — | — | — | — |
| **Portal** | — | — | — | — | — | — | — | — | — | — | — |
| **Arrow** | ✔ | — | — | — | — | — | — | — | — | ✔ | — |
| **Debris** | ✔ | — | — | — | — | — | — | — | — | ✔ | — |
| **KillZone** | — | ✔ | ✔ | ✔ | ✔ | — | — | ✔ | ✔ | — | — |
| **Field** | — | — | — | — | ✔ | — | — | — | — | — | — |

Notes: Rope and Portal colliders are triggers touched **only by queries**. Field ↔ Prop stays enabled per D-048, but **no gameplay rule relies on trigger callbacks between triggers** (their ordering is not deterministic): balloon wind is sampled through `WindFieldSampler` (§8.7) and fire uses timed overlap queries (§8.8). Kill zones are the only gameplay triggers that use `OnTriggerEnter` (dynamic body vs trigger; decisions are latched, §11).

### 2.2 Query masks (in `PhysicsLayers`)

| Mask | Layers | Trigger interaction | Used by |
|---|---|---|---|
| `ArrowCastMask` | Environment, Structure, Objective, Protected, Prop, Rope, Portal | `Collide`, then filtered through `PhysicsBodyRegistry`: only Rope, Portal and Balloon triggers react; other triggers are skipped | `ArrowProjectile`, `TrajectoryPreview` |
| `ExplosionMask` | Structure, Objective, Prop, Rope | Collide (ropes are cut in the inner radius) | `Explosion` — **excludes Protected** (D-020) |
| `FireOverlapMask` | Rope, Structure, Objective, Prop | Collide | `FireZone`, `Burnable` spread |
| `TrackedBodyLayers` | Structure, Objective, Protected, Prop | — | `PhysicsBodyRegistry` / `SettleMonitor` |
| `GroundProbeMask` | Environment | Ignore | `OilJar` spill placement, `TrainingDummy` ground check |
| `KillZoneMask` | KillZone | Collide | Flying-arrow and preview kill-zone point check (`02` §5.1). KillZone is deliberately **not** in `ArrowCastMask`. |

---

## 3. Rigidbody and joint policies

### 3.1 Rigidbody defaults (applied by `PlanarBody`/`MaterialBody`; overridable on the prefab only with a validator-visible reason field)

| Setting | Default | Exceptions |
|---|---|---|
| Mass | `MaterialProfile.massPerCell × volumeCells` (1 cell = 1×1×1 m) | Objectives/protected use their own prefab mass (tables below) |
| Linear / angular drag | 0.05 / 0.20 | Balloon-supported loads: linear drag 1.0 while ≥ 1 balloon is attached; boulder angular drag 0.05 |
| Interpolation | None | `Interpolate` only on falling hero props if stutter is visible (case-by-case, PHYS approval) |
| Collision detection | Discrete | `ContinuousSpeculative` for boulders, small props (< 0.4 m) and kinematic movers |
| `maxDepenetrationVelocity` | 3 m/s | — |
| `maxAngularVelocity` | 25 rad/s | — |
| `solverIterations` override | none (8) | Tall stacks (> 8 bodies tall) may set 12 on the base pieces (validator info message) |
| Sleep threshold | project default 0.005 | — |
| Kinematic movers | `isKinematic = true`, moved only via `MovePosition`/`MoveRotation` in `FixedUpdate` from `LevelClock` | — |

**Mass-ratio rule:** two dynamic bodies that touch at load should differ in mass by ≤ 10:1 (validator warning W-01 in `04` §9). Metal pieces (16 kg/cell vs 1.2 kg/cell straw) are usually kinematic or static to avoid this.

### 3.2 Joint policy

| Use | Joint | Settings | Notes |
|---|---|---|---|
| Rope / chain (load hang) | `ConfigurableJoint` | X/Y/Z motion `Limited`, linear limit = rope length, `contactDistance` 0.01, spring 0 (hard), angular all `Free` except X/Y `Locked` (plane), projection `PositionAndRotation` (dist 0.05, angle 5°) | Owned by `RopeCuttable`. Anchor = static world point (`connectedBody = null`) or a body. |
| Lever / seesaw / hinged bridge / swinging sign | `HingeJoint` | Axis = (0, 0, 1) **always**, `useLimits` with ±35° default, `bounciness` 0, no motor, `enablePreprocessing` true | Validator errors if the axis is not ±Z |
| Glued compound | — | Use one Rigidbody + several colliders | **No FixedJoint chains** (unstable, expensive) |
| Balloon tether | none (force-based, see §8.7) | — | Cosmetic tether line only |
| Breakable connection (peg holding a boulder) | none — the peg is a `Breakable` Structure body blocking the boulder | — | Joint `breakForce` is **not** used for gameplay (hard to tune, invisible). Exception: `HingeJoint.breakForce` on decorative pieces only. |

Budget: ≤ 12 joints per level (ropes count as 1 each). Validator warning at > 12, error at > 20.

---

## 4. Material profiles

`MaterialProfile` SO (`ScriptableObjects/Materials/MP_<Material>.asset`), referenced by `MaterialBody`. Each profile also references a `PhysicsMaterial` asset `PM_<Material>` (`Art/Materials/Physics/`) and the audio/VFX events.

### 4.1 Fields and graybox values

| Field | Straw / wicker | Timber | Stone | Ice crystal | Metal | *Earth (environment only)* |
|---|---|---|---|---|---|---|
| `kind` (`MaterialKind`) | Straw | Timber | Stone | Ice | Metal | *(proposed `Earth` = 5, D-059)* |
| `massPerCell` (kg / 1 m³) | 1.2 | 4.0 | 12.0 | 5.0 | 16.0 | static |
| Static / dynamic friction | 0.60 / 0.50 | 0.65 / 0.55 | 0.80 / 0.70 | **0.08 / 0.04** | 0.40 / 0.30 | 0.8 / 0.7 |
| Friction combine | Average | Average | Average | **Minimum** (ice slides on anything) | Average | Average |
| Bounciness / bounce combine | 0.05 / Minimum | 0.05 / Minimum | 0.02 / Minimum | 0.10 / Minimum | 0.15 / Minimum | 0 |
| `maxHP` (Medium size class) | 12 | 20 | 80 | 10 | ∞ (`unbreakable`) | ∞ |
| `damageThreshold` (collision impulse, Ns, below = no damage) | 0.8 | 2.5 | 6.0 | 1.5 | — | — |
| `collisionDamageScale` | 1.0 | 0.5 | 0.3 | 1.2 (brittle) | 0 | — |
| `breakImpulse` (single impact ⇒ instant break, Ns) | 8 | 25 | 120 | 14 | — | — |
| `explosionDamageScale` | 1.5 | 1.0 | 0.5 | 1.5 | 0 (force only) | 0 |
| Arrow embed | always (θ ≤ 80°) | θ ≤ 60° ∧ v ≥ 5 m/s | never | never | never | always |
| `ricochetMaxGrazingDeg` / speed factor | — | — | — | — | 25° / 0.6 | — |
| `impulseTransfer` (arrow → body) | 1.0 | 0.9 | 0.7 | 1.0 | 0.5 | 0 |
| `arrowDamageMultiplier` | 1.5 | 1.0 | 0.5 | 1.0 | 0 | 0 |
| Burnable | **yes** (burn 2.0 s, then breaks) | no | no | no | no | no |
| Debris (fragments S / M / L) | 2 / 3 / 4 tufts | 3 / 4 / 6 splinters | 2 / 3 / 5 chunks | 4 / 5 / 7 shards | — | — |
| Debris lifetime | 1.2 s | 1.6 s | 2.0 s | 1.2 s (shrinks) | — | — |
| SFX (`SoundEvent`) | `SE_Impact_Straw` (soft thump/rustle), `SE_Break_Straw` (crunch) | `SE_Impact_Timber` (knock), `SE_Break_Timber` (crack), `SE_Arrow_EmbedWood` (thunk) | `SE_Impact_Stone` (thunk), `SE_Break_Stone` (crumble), `SE_Arrow_Chip` | `SE_Impact_Ice` (chime), `SE_Break_Ice` (shatter) | `SE_Impact_Metal` (ping), `SE_Arrow_Ricochet` (zing) | `SE_Arrow_EmbedEarth` |
| VFX (`VfxEvent`) | `VFX_Impact_Straw`, `VFX_Break_Straw` (chaff puff) | `VFX_Impact_Timber`, `VFX_Break_Timber` (splinter burst) | `VFX_Impact_Stone` (dust), `VFX_Break_Stone` | `VFX_Impact_Ice` (sparkle), `VFX_Break_Ice` (glitter shards) | `VFX_Impact_Metal` (sparks) | `VFX_Impact_Earth` (dirt) |
| Colour language (§4) | pale woven yellow | warm brown | grey-blue | bright cyan, translucent | dark steel + gold trim | world ground palette |
| Visual states (≤ 3) | intact / frayed (≤ 50% HP) / gone | intact / cracked (≤ 50%) / broken | intact / chipped (≤ 50%) / crumbled | intact / cracked (≤ 50%) / shattered | intact / dented (cosmetic on hit) | — |
| Graybox colour (M1–M5; art in M6) | #E8D27A | #9A6B3F | #7D8AA0 | #7FE3F2 (alpha 0.75) | #3A3F47 | #6B8E4E |

### 4.2 Size classes (HP and debris scale)

| Class | Definition | HP multiplier | Example |
|---|---|---|---|
| Small | volume ≤ 0.5 cells or any dimension ≤ 0.25 m | × 0.4 | Peg 0.25×0.5, plank 3×0.2 |
| Medium | 0.5–1.5 cells | × 1.0 | Crate 1×1 |
| Large | > 1.5 cells | × 1.8 | Beam 3×0.5 timber (1.5 → Medium), block 2×1 (Large) |

### 4.3 Calibration examples (sanity checks for AB-014)
- An Oak arrow head-on into a 1×1 timber crate: J = 6 × 1.0 × 0.9 = 5.4 Ns → Δv ≈ 1.35 m/s at the contact point → the top crate of a 3-stack tips. Damage 5.4 → 4 hits to break (HP 20). **Readable: "arrows push and lodge; they don't delete crates."**
- Heavyhead into a timber peg (Small, HP 8): damage 18 × 3 × 1.0 = 54 → breaks. Into a 1×1 crate (HP 20): breaks. Into a stone block (12 kg): J = 18 × 0.7 = 12.6 Ns → Δv ≈ 1.05 m/s (pushes stone off a ledge, as §5 intends).
- A timber crate falling 3 m onto timber: v ≈ 7.7 m/s, impulse ≈ 31 Ns → (31 − 2.5) × 0.5 ≈ 14 damage → cracked, not broken. A second fall breaks it ("breaks after repeated stress", §4).
- A 1×1 stone block falling 2 m onto a vase: impulse ≫ 4 Ns → vase breaks (stone is "dangerous when falling").
- Ice block knocked off a ledge 1.5 m: impulse ≈ 27 Ns ≥ breakImpulse 14 → shatters into harmless shards (§4).

---

## 5. Breakable system

**Owner:** PHYS · **Files:** `Physics/Breakable.cs`, `Physics/ImpactDamage.cs`, `Physics/DebrisPool.cs`, `Physics/DebrisPiece.cs` · **Ticket:** AB-012

### 5.1 States and flow
```
Intact ──HP ≤ 50%──► Damaged (swap mesh/material variant; SFX) ──HP ≤ 0 or impulse ≥ breakImpulse──► Broken
Broken: disable renderers + colliders + rigidbody (kinematic, parked) → spawn debris from DebrisPool
        → raise Breakable.Broken (local) + GameEvents.ObjectBroken(BreakInfo)
             subscribers (Breakable knows none of them): RopeCuttable on this body or its anchor → treated as cut;
             embedded ArrowProjectiles → Spent; Objective / ProtectedObject → clear / fail
        → registry marks Removed (excluded from settle)
```

### 5.2 Damage sources

| Source | Entry point | Formula |
|---|---|---|
| Arrow | `ArrowImpactResolver` → `Breakable.ApplyDamage(d, DamageSource.Arrow)` | `02` §6.4 |
| Collision | `ImpactDamage.OnCollisionEnter` (on each Breakable body) | `I = collision.impulse.magnitude`; if `I ≥ breakImpulse` → Break; else `HP -= max(0, I − damageThreshold) × collisionDamageScale` |
| Explosion | `Explosion` → `ApplyDamage(d, DamageSource.Explosion)` | §8.10 |
| Fire | `Burnable` (straw only) → `Break(cause: Fire)` after the burn time | §8.8 |
| Kill zone | `KillZone` → `Remove(cause: KillZone)` (no debris for Water/Pit, debris for Spikes) | §8.17 |

### 5.3 Guard rails
- **Spawn grace:** collision damage is ignored for 0.5 s of `LevelClock` after load (sleep → wake noise).
- **Pair rate limit:** a given body pair can deal collision damage at most once per 0.1 s (stops grinding contacts from slowly destroying stacks).
- **Resting contact:** impulses from contacts with relative speed < 0.5 m/s are ignored.
- **Deterministic order:** damage is applied immediately in the callback; breaks requested during a step are processed at the end of that `FixedUpdate` in `PhysicsBodyRegistry` index order (`Breakable.FlushPendingBreaks`).
- **No gameplay fracture:** a broken object never spawns new gameplay bodies in MVP (only cosmetic debris). *(D-060 — snap-in-half planks are a post-MVP option.)*

### 5.4 Debris (D-008)
- `DebrisPool` holds 40 `DebrisPiece`s (Low tier 20) per scene, with shared meshes per material and size; recycle the oldest on overflow.
- Spawn velocity = the broken body's velocity + an outward cosmetic spread (`CosmeticRandom`, seeded by level + object index) of 1.5–3 m/s, z spread ±1 m/s, random spin.
- Lifetime per material (§4.1), then scale to 0 over 0.25 s and return. `reducedParticles` halves the fragment counts.
- Fragments never trigger objectives, protected loss, explosions or ropes (layer matrix guarantees this). They *can* enter kill zones (splash VFX only).

---

## 6. Component design (composition)

All gameplay prefabs derive from one of four base prefabs: `Struct_Base`, `Obj_Base`, `Prop_Base`, `Prot_Base` (Prefabs/Structures, Objectives, Props, Protected). Each base carries `PlanarBody` + `MaterialBody` + the correct layer.

| Component | Folder | Responsibility | Requires | Owner |
|---|---|---|---|---|
| `PlanarBody` | Physics | Applies plane constraints, z snap, depenetration clamp. Holds `IsAmbient` (settle exemption, set by Props via `MarkAmbient(untilLevelTime)`) and the `Removed` latch. Raises `KilledBy(KillZoneKind)` when a kill zone or `PlayBounds` takes it (Objective/ProtectedObject subscribe). Registers with `PhysicsBodyRegistry`. | Rigidbody | PHYS |
| `IArrowHittable`, `ArrowHitInfo`, `IExplosionReactive`, `IFlightEnvironment` | Physics | **Cross-module contracts.** `IArrowHittable.Evaluate` (pure; used by the preview too) + `Apply` (side effects). `IExplosionReactive.OnExplosion(in ExplosionHit)`. `IFlightEnvironment.SampleWind`. They let Arrows/Physics reach props and objectives without referencing Props/Objectives types (`01` §10.3). | — | PHYS (ARCH review) |
| `MaterialBody` | Physics | Links `MaterialProfile`; applies mass (from cells) + `PhysicsMaterial` at `Awake`; exposes `SizeClass` | Collider(s) | PHYS |
| `Breakable` | Physics | HP, visual states, break flow, debris, `Broken` event, `ApplyDamage`, `Break(cause)` | MaterialBody | PHYS |
| `ImpactDamage` | Physics | Collision impulse → Breakable damage, with guard rails | Breakable, Rigidbody | PHYS |
| `PhysicsBodyRegistry` | Physics | Per-level list of tracked bodies + collider-instance-id → entry map (BodyKind, refs). Rebuilt on load. No `GetComponent` at hit time. | — | PHYS |
| `SettleMonitor` | Physics | Calm sampling (`02` §8) | Registry | PHYS |
| `ExplosionSolver` (pure) / `Explosion` | Physics | Radial impulse/damage math / runtime query + queue | — | PHYS |
| `Objective` | Objectives | Kind, `ObjectiveClearRule` flags (D-037 defaults per kind), cleared state, HUD icon | Body or RopeCuttable | PROPS |
| `ObjectiveTracker` | Objectives | Counts and orders objectives; `ObjectiveCleared`; `AllCleared` | — | PROPS |
| `ProtectedObject` | Objectives | Kind, fail thresholds (D-038), start pose, purple outline | Rigidbody | PROPS |
| `ProtectedTracker` | Objectives | Latches `ProtectedLost` | — | PROPS |
| `BullseyeMarker` | Objectives | Marked weak point (radius 0.25 m). Implements `IArrowHittable`; a direct hit raises its `Hit` event, and `GameplayController` sets `LevelSession.BullseyeHit` (D-045) | Child of a body | PROPS |
| `RopeCuttable` + `RopeView` | Props | Joint rope + cut capsule + visual (D-009) | — | PROPS |
| `Burnable` | Props | Ignite/burn/spread/consume (straw, rope, oil jar, powder barrel fuse) | — | PROPS |
| `FireZone` | Props | Temporary fire area (oil spill) | — | PROPS |
| `OilJar` | Props | Break → FireZone | Breakable | PROPS |
| `PowderBarrel` | Props | Triggers → `Explosion` | Breakable | PROPS |
| `Balloon` | Props | Lift force on the load, pop, wind response | — | PROPS |
| `WindField` | Props | Constant acceleration zone. Implements the `IFlightEnvironment` wind sampling (via `WindFieldSampler`) | BoxCollider (trigger, Field) | PROPS |
| `KinematicMover` | Props | Deterministic pose(t) for shields (moving ice platforms are cut, D-104) | Rigidbody (kinematic) | PROPS |
| `PortalRing` / `PortalPair` | Props | Arrow teleport mapping | Trigger collider (Portal) | PROPS |
| `KillZone` | Props | Water/Spikes/Pit removal semantics (D-039) | Trigger (KillZone) | PROPS |
| `PlayBounds` | Props | Out-of-frame removal | — | PROPS |
| `LevelLayout` | Levels | Root of a layout prefab: framing, `clearLineY`, tutorial anchors, mover phase list | — | LEVEL/CORE |
| `TutorialAnchor` | Levels | Named anchor for prompts | — | LEVEL |

**Interfaces:** exactly the four Physics contracts above, and no more without an ARCH decision. The registry maps each collider instance id to `{BodyKind, PlanarBody, Breakable?, IArrowHittable?, IExplosionReactive?}`, cached at load (no `GetComponent` at hit time). `ArrowImpactResolver` and `Explosion` dispatch through the interfaces, so neither Arrows nor Physics references a Props/Objectives type. Implementers: `IArrowHittable` — RopeCuttable, Balloon, PortalRing, Objective (orb/crest/lantern clear), ProtectedObject, BullseyeMarker. `IExplosionReactive` — RopeCuttable (inner-radius cut), Balloon (pop), PowderBarrel (chain), CursedOrb's Objective (explicit immunity).

---

## 7. Objective system

**Owner:** PROPS (components) / PO (rules) · **Tickets:** AB-018 (framework + CrestTarget + SupplyCrate), AB-052 (HangingLantern), AB-053 (CursedOrb), AB-054 (TrainingDummy), AB-055 (BannerRope) · **Decision:** D-037

- **Every `Objective` component in a layout is required.** There are no optional objectives in MVP; decorative crates are plain Structure.
- `ObjectiveClearRule` (flags enum): `ArrowHit`, `Broken`, `ImpactAboveThreshold`, `BelowClearLine`, `KillZone`, `KnockedOver`, `TouchesGround`, `RopeCut`. Defaults per kind are filled by `Objective.Reset()`/`OnValidate`. Overrides need a validator-visible reason.

| Kind | Prefab | Default rules | Body | Colour/silhouette |
|---|---|---|---|---|
| CrestTarget | `Obj_CrestTarget` (standing, dynamic 2 kg timber), `Obj_CrestTarget_Mounted` (kinematic on a static post) | ArrowHit, ImpactAboveThreshold (6 Ns), BelowClearLine, KillZone | Medium timber disc 0.8 m Ø | Red disc, white ring, crest icon, strong outline |
| SupplyCrate | `Obj_SupplyCrate_Timber_1x1` (+ `_Straw_` variant) | Broken, BelowClearLine, KillZone | Timber crate (HP 20) | Red bands + crate icon decal |
| HangingLantern | `Obj_HangingLantern` (dynamic 1 kg, hung by `RopeCuttable`) | ArrowHit, Broken (HP 1), ImpactAboveThreshold (2 Ns; ground contact > 2 m/s breaks it), KillZone | Glass-like, custom `MaterialBody` override (Ice-like brittleness, not ice-coloured) | Red lantern with emissive core |
| CursedOrb | `Obj_CursedOrb` (kinematic, floating) | **ArrowHit only** (immune to impacts, blasts, fire) | Sphere 0.6 m Ø | Red orb with a dark swirl + rune icon; a distinct "direct hit only" crosshair icon in the HUD |
| TrainingDummy | `Obj_TrainingDummy` (dynamic 3 kg, straw-stuffed, wide base 0.6 m, COM low) | KnockedOver (up-vector > 70° for 0.3 s), TouchesGround (contact with Environment collider tagged `Ground`), Broken (HP 12, straw) | Capsule + base | Red tabard, comedic face; topples readably |
| BannerRope | `Prop_Rope_Banner` + `Objective` | RopeCut (incl. burned) | Rope | Red banner cloth on the rope |

- `TouchesGround` uses the **`Ground` tag** on base-ground Environment colliders (not ledges/platforms). The tag is added in AB-002.
- `BelowClearLine`: evaluated each FixedUpdate as `body.worldCenterOfMass.y < LevelLayout.clearLineY` (if the clear line is enabled). Validator V-15 (`04` §9) requires every objective's and protected object's start bounds to sit > 0.3 m above the clear line.
- Clearing is **latched** (an objective never un-clears).
- HUD order = hierarchy order under `LevelLayout`. Objective icons cross out on `ObjectiveCleared`.

---

## 8. Interactive objects — implementation plans

**Ticket map** (`09`): structures AB-013 / AB-058 · objectives §8.1 (§7 list) · rope AB-020 · kill zones + clear line AB-021 · royal vase AB-018 · sleeping fox AB-056 · bullseye AB-057 · explosion AB-050 · powder barrel AB-051 · balloon AB-085 · Burnable/FireZone AB-086 · oil jar AB-087 · shields (`KinematicMover`) AB-090 · boulders AB-091 · levers AB-092 · ~~spring plate AB-093~~ **cut (D-086)** · metal tuning AB-094 · ice AB-104 · wind AB-105 · portals AB-107 · ~~moving ice platforms AB-108~~ **cut (D-104)** · royal relic = the vase component with a relic prefab (art AB-111) · interaction-matrix tests AB-063 (W1), AB-095 (W2), AB-109 (W3).

**Milestone (D-102):** every system in this section is built in **M4 Systems Complete**, after the G0 Vertical Slice & Feel Lock GO — except the VS set already built in M2/M3 (crate, crest target, rope, royal vase, kill zones). **MVP prop set = 8** (rope/chain, balloon cluster, oil jar, powder barrel, rolling boulder, wind fan, rotating/moving shield, portal pair) — meets §14 "≥ 8". Spring plates are cut (D-086).

Common to every object: built from a base prefab, ≤ 3 visual states, validated by `PrefabValidator` (layer, PlanarBody, collider depth, mass in range). It raises `GameEvents.PropTriggered` with `PropTriggerKind` for analytics `object_triggered`. It has a sandbox scene `Sandbox_PROPS_<Name>.unity` and ≥ 1 `InteractionMatrixTests` case. **Colour:** helpful interactive = green/gold, hazard/protected = purple, required = red.

### 8.1 Required objectives
See §7. Implementation order: CrestTarget + SupplyCrate (AB-018, M2) → HangingLantern (AB-052), CursedOrb (AB-053), TrainingDummy (AB-054), BannerRope (AB-055) in M4.

### 8.2 Destroyable crates and structural objects
- Prefab family `Struct_<Shape>_<Material>_<Size>`: `Crate 1x1, 2x1`, `Beam 3x0.5, 4x0.5`, `Plank 3x0.2, 4x0.2`, `Block 1x1, 2x1, 1x2`, `Peg 0.25x0.5`, `Pillar 0.5x2, 0.5x3`, `Platform` (static Environment), `Lever 4x0.25` (§8.19). Materials: Straw (crate, bale), Timber (all), Stone (block, pillar, beam), Ice (block, beam, plank), Metal (plate, shield — mostly kinematic/static).
- Library v1 (AB-013): timber Crate 1x1/2x1, Beam 3x0.5, Plank 3x0.2/4x0.2, Peg, Pillar 0.5x2, Platform 2/4/6 m. Expanded per world.

### 8.3 Hanging lanterns (Objective)
- Hung by 1 `RopeCuttable` (or 2 for a swing-locked variant). Cut → falls → breaks on landing if v > 2 m/s. Arrow hit → shatters (the arrow passes through ×0.7 → one shot can chain into a second lantern).
- Edge: a lantern landing softly on straw (v < 2 m/s) does **not** break. Designers avoid that unless intended; the validator reports the drop height under every lantern.

### 8.4 Cursed orbs (Objective, direct-hit only)
- Kinematic Rigidbody (excluded from settle). A cosmetic visual bob (child transform, ±0.05 m, 0.8 Hz) never moves the collider.
- Immune to: impacts, blasts, fire, debris, wind. Cleared only by an arrow sweep hit (any arrow type, including split children and ricochets/portal exits — "direct" means an *arrow*, not falling objects).
- If it enters a KillZone (it shouldn't — it's kinematic), it is cleared and the validator flags the level.

### 8.5 Training dummies (Objective)
- Dynamic, straw material (burnable!), mass 3 kg override, COM lowered by 0.2 m (wobbly but stands).
- Knocked-over detection: `Vector3.Angle(transform.up, Vector3.up) > 70°` continuously for 0.3 s of `LevelClock`.
- Comedic reaction animation (visual child) on the first hit. Doesn't affect physics.

### 8.6 Ropes and chains (`RopeCuttable`, `RopeView`) — D-009
- **Authoring:** place `Prop_Rope` (or `Prop_Rope_Chain`, `Prop_Rope_Banner`) with two handles: `anchor` (a world point or body) and `attachedBody` + local attach point. Length = the authored distance (auto) + `slack` (default 0.0 m).
- **Runtime:** `ConfigurableJoint` on `attachedBody` (§3.2). A cut capsule (radius 0.08 m, Rope layer, trigger) is repositioned each FixedUpdate between the current endpoints (allocation-free).
- **Cut sources:** arrow sweep (any arrow, pass-through), explosion inner radius (≤ 1.2 m from the blast centre), `Burnable` consumption (rope burn time 1.2 s; chains are not burnable), or the attached/anchor body breaking.
- **Effects:** destroy the joint, play the snap VFX/SFX, the two rope ends recoil (visual, 0.3 s), raise `PropTriggered(RopeCut)`, notify `Objective` (BannerRope).
- **Multiple ropes on one load:** each is independent (the load swings on the remaining rope — a classic two-step puzzle).
- **Visual:** `RopeView` = LineRenderer with 12 points. Taut = straight. Slack = a parabola sag of `0.05 × length`. Chain = a tiled link texture (no physics links).
- **Edge cases:** an arrow grazing the rope's end within 0.1 m of the anchor still cuts it. Ropes never collide with anything (no wrapping).

### 8.7 Balloon clusters (`Balloon`)
- **Prefab:** `Prop_Balloon_Cluster_<1|2|3>` attached to a load body. The balloons are **not rigidbodies**. Each balloon has a trigger sphere (0.35 m) on the Prop layer for arrow and impact detection, positioned by a cosmetic spring above the attach point.
- **Lift:** in FixedUpdate apply `AddForceAtPosition(up × liftPerBalloon × count, attachPoint)` to the load. Defaults: `liftPerBalloon` such that an authored `count` balances `loadMass × 9.81 × liftRatio`, with `liftRatio` 1.05 (slight upward) and a **tether rope** below or a ceiling stop above to hold it still. While ≥ 1 balloon is attached the load has linear drag 1.0 and is marked `IsAmbient` when |v| < 0.3 m/s.
- **Pop:** an arrow sweep (PassThrough ×0.9), a trigger contact with a Structure/Prop/Objective at relative speed > 1.0 m/s, an explosion within radius, fire within 0.4 m. Pop order is player-controlled ("Sky Drop" template: pop in the correct order).
- **Wind:** each FixedUpdate, sample `WindFieldSampler.SampleWind(attachPoint)` (the same sampler the arrows use; no trigger callbacks) and add a force `windAccel × balloonWindMass (0.4 kg) × count` at the attach point.
- Colour: green/gold balloons (helpful interactive).

### 8.8 Fire system (`Burnable`, `FireZone`) and oil jars (`OilJar`)
- **`Burnable` states:** Normal → Burning (on ignite) → Consumed.
  - Straw (any straw `MaterialBody`) burn 2.0 s → `Breakable.Break(Fire)`. Rope burn 1.2 s → cut. Oil jar burn 0 s → break + FireZone ×1.5. Powder barrel fuse 0.4 s → explode.
  - **Spread:** every 0.25 s (`TimedEvents` tick), each burning object overlaps `FireOverlapMask` within `spreadRadius` 0.6 m. Any `Burnable` continuously exposed for `igniteDelay` 0.5 s ignites. Processed in registry order.
  - Cap: 12 simultaneously burning objects; the extra ignitions queue (logged in dev).
  - **Never burns:** timber, stone, ice, metal, protected objects, cursed orbs, environment.
  - Extinguish: entering a Water kill zone (the object is removed anyway) — no rain or other extinguishers in MVP.
- **`FireZone`:** a box (default 1.5 × 0.6 m) placed on the surface found by a ground probe (≤ 3 m down) under the broken oil jar. Lifetime 3.0 s. Ignites `Burnable`s overlapping it after 0.5 s of exposure. Visual: low flames + heat shimmer (no light).
- **`OilJar`:** `Prop_OilJar` dynamic 1.5 kg, `Breakable` HP 4, damageThreshold 1.5, breakImpulse 5 → **any direct arrow breaks it** (6 × 1.0 ≥ 4) ("breaks on strong hit", §4). Falls > 1 m → breaks. On break: spill FireZone (lit immediately — the jar's oil is ignited on break per §4 "creates brief fire zone"). If the break was caused by a Fire arrow, the zone is 1.5× wider and lasts 4 s.
- Readability: oil jars are dark clay with a gold oil-drop icon (helpful interactive = gold).

### 8.9 Powder barrels (`PowderBarrel`)
- `Prop_PowderBarrel`: dynamic 3 kg (timber-like), `Breakable` HP 6. **Triggers:** any arrow hit; impact ≥ 8 Ns; fire (fuse 0.4 s, sparks VFX); another explosion within its radius (chain delay 0.15 s).
- On trigger → `Explosion.Request(position, BarrelBlastProfile)`, then the barrel breaks (debris + blast VFX).
- **Telegraph:** red-black hazard bands? **No** — red is reserved for required objectives. Barrels use dark wood + **gold** powder-keg icon and a pulsing fuse when lit.

### 8.10 Explosions (`Explosion`, `ExplosionSolver`)

```text
ExplosionSolver.Compute(center, body, profile):
  d   = distance(center, closestPoint(body.collider, center))
  if d > profile.radius: none
  f   = (1 - d / radius)^2
  dir = normalize((body.worldCenterOfMass - center).xy) ; dir.y += profile.upwardModifier (0.3); normalize
  impulse = dir × profile.maxImpulse × f          (BarrelBlast: radius 2.5 m, maxImpulse 25 Ns, clamp per body 25 Ns)
  damage  = profile.maxDamage × f × material.explosionDamageScale     (maxDamage 30)
```

- Affects `ExplosionMask` (Structure, Objective, Prop, Rope inner radius 1.2 m). **Protected: no damage, no force (D-020).** CursedOrb: immune. Balloons in radius pop.
- **Queue:** at most 1 explosion is processed per FixedUpdate. Chained barrels wait 0.15 s (`TimedEvents`) — readable cascades, bounded cost. Max 6 explosions per level (validator).
- No occlusion (a simple radial blast); designers use distance and blockers.
- Feedback: hit-stop is not used for blasts (it's already a big moment). Camera shake (unless reduced motion), Heavy haptic, blast VFX **after** a 1-frame delay so the break is readable.

### 8.11 Rolling boulders
- `Prop_Boulder_<0.8|1.2>`: a sphere, Stone, mass 20 kg (ratio vs 4 kg timber = 5:1 ✓), `ContinuousSpeculative`, angular drag 0.05, rolling friction emulated by angular drag (PhysX has none).
- **Release patterns (no special code):** (a) held by a timber `Peg` (Small, HP 8) → Heavyhead breaks it (the "Boulder Run" template); (b) hung by a `RopeCuttable`; (c) resting on a lever/bridge.
- A boulder lane is a static Environment slope ≥ 6° with side lips. The validator warns if a boulder can reach a protected object along the lane without a blocker (heuristic: overlap of the lane AABB).
- Settle: a slowly rolling boulder counts as motion. The 5 s fail cap covers pathological cases. Lanes must end in a kill zone or a stop wall.

### 8.12 Wind fans / wind zones (`WindField`)
- `Prop_WindFan` (static Environment fan model + rotating blade visual) + `WindField` box trigger (Field layer) with `acceleration` (vector in XY, e.g. (4, 0) m/s²).
- **Always on, constant, no gusts in MVP** (§4 "always on"). "Gust lanes" (L44–46) = narrow WindFields with stronger acceleration (≤ 8 m/s²).
- Affects: **arrows** via `BallisticSolver` (× `windResponse`; Heavyhead 0.25, so it barely bends), **balloons** (§8.7). **Does not affect** structures, objectives, protected, debris (simplifies readability: "wind bends arrows and pushes balloons").
- The preview shows wind-bent arcs (D-085). Visual: streak particles flowing along the acceleration direction (Low tier: 50% particles, a static streak texture fallback).
- `WindFieldSampler` (an `IFlightEnvironment` impl) caches WindField boxes at load; the sample = the sum of the accelerations of the boxes containing the point (max 2 overlapping, validator).

### 8.13 Rotating / moving shields (`KinematicMover`)
- `Prop_Shield_Rotating_<size>`, `Prop_Shield_Patrol_<size>`: Metal plate (kinematic, Environment layer) + a gold pivot/rail telegraph.
- `KinematicMover` modes: `Rotate` (deg/s about the pivot Z axis), `PingPong` (A↔B with sine ease, period T), `Loop` (closed waypoint path, constant speed).
- **Deterministic pose:** `pose = f(LevelClock.Now + phaseOffset)`, set via `MovePosition`/`MoveRotation` each FixedUpdate → identical every attempt; restart resets the clock.
- Telegraph: the rotation arc or path is drawn as a dotted gold decal. Speed ≤ 90°/s rotation, ≤ 2 m/s translation (§6 "clearly telegraphed loops"). Period 2–4 s.
- Movers push dynamic bodies they touch (kinematic). Design rule: a mover's swept area must not intersect resting dynamic bodies at load (validator: sweep the path AABB vs bodies → error).
- Arrow interaction: Metal rules (ricochet at grazing; Bounce arrow bank shots).

### 8.14 Moving ice platforms — cut from the MVP (D-101, D-104)
- **Not built for the MVP.** L53–55 use **static ice slides/ramps** (`Struct_Slab_Ice_*` on tilted `Environment` ledges; Ice `PhysicsMaterial` friction 0.08, Minimum combine) plus `KinematicMover` metal shields for the "low-risk timing" beat. `KinematicMover` itself stays (shields).
- If the project is genuinely ahead of schedule after G3, the original design can return as a `KinematicMover` (PingPong) with an Ice material; contact-ambient rule: bodies touching a mover within the last 0.1 s are `IsAmbient` only while their speed < 0.3 m/s relative to it. Requires an OWNER decision.

### 8.15 Portal pairs (`PortalRing`, `PortalPair`)
- `Prop_PortalPair`: two rings (A, B). Each has a static Environment frame collider (a torus approximated by 4 boxes) and an entry disc trigger (radius 0.6 m, Portal layer). **Convention:** the entry normal is the ring's `transform.up` (it must lie in the XY plane; the ring rotates only about Z). An arrow enters when `dot(v, ring.up) < 0`. Author it with the gizmo arrow. Colour: green/gold swirl, with the pair linked by matching rune colour (≤ 2 pairs per level, distinct colours).
- **Only arrows teleport** (§4); bodies and debris pass the disc trigger unaffected (Portal layer collides with nothing).
- **Mapping:** entering A from its front side → `local = A.InverseTransform(hitPoint)`; exit position = `B.Transform(mirror(local))`; exit velocity = `B.rotation × Quaternion.Euler(0, 0, 180) × inverse(A.rotation) × v` (the speed is preserved). The arrow ignores B's trigger for 0.1 s. `TeleportsLeft` decrements (max 3 → avoids infinite loops). Preview identical, including the exit path (D-085).
- Edge: entering from the back side → the frame behaves as Environment (Stone rules).

### 8.16 Spring plates — cut from MVP (D-086); post-MVP note only
Not built, not in the prefab catalogue, not used by any level. A post-MVP design would be a static base + sensor trigger launching bodies with a deterministic `VelocityChange` impulse.

### 8.17 Kill zones (`KillZone`) and `PlayBounds` — D-039

| Kind | Prefab | Visual | Entering gameplay body | Entering objective | Entering protected | Arrow | Debris |
|---|---|---|---|---|---|---|---|
| Water | `Haz_WaterPit_<w>` | Animated water surface, splash | Splash VFX, body made kinematic and sunk over 0.4 s, then removed | **Cleared** | **Fail** (KillZone) | Removed (small splash) | Removed (tiny splash) |
| Spikes | `Haz_SpikeBed_<w>` | Purple-tinted spikes | Crunch SFX, `Breakable.Break(KillZone)` (debris) or removal if unbreakable | **Cleared** | **Fail** | Removed ("spikes also remove arrows", §4) | Removed |
| Pit | `Haz_Pit` | Dark void / off-screen | Silent removal after its centre crosses the trigger | **Cleared** | **Fail** | Removed | Removed |

- **Notification path:** `KillZone`/`PlayBounds` call `PlanarBody.Kill(kind)` (latched). `PlanarBody` raises `KilledBy(kind)`, and `Objective` (→ cleared) and `ProtectedObject` (→ `ProtectedLost(KillZone)`) subscribe. The decision is taken at trigger entry (§11 invariant 2); the sink/crunch visuals follow.
- **`PlayBounds`:** each FixedUpdate, check tracked bodies' positions against the framing rectangle + `playBoundsMargin` (2 m). Leaving = the Pit semantics. Arrows: lifetime/out-of-bounds resolution (`02` §5).
- A removed body is excluded from settle, and its embedded arrows go back to the pool.

### 8.18 Protected objects — royal vase, sleeping fox, royal relic (D-038)

| Kind | Prefab | Body | Fail conditions | Visual |
|---|---|---|---|---|
| RoyalVase | `Prot_RoyalVase` | dynamic 2 kg, friction 0.6, COM low | arrow hit; collision impulse ≥ `fragileImpulse` 4 Ns; KillZone / below clear line / out of bounds | Porcelain white + purple glaze, purple outline + shield icon |
| SleepingFox | `Prot_SleepingFox` | dynamic 3 kg, friction 0.9, very low COM, capsule | arrow hit; displacement > 0.6 m from start; tilt > 45°; KillZone / out of bounds. **Not breakable.** | Orange fox curled asleep, purple "Zzz" outline + icon; wakes and runs off on fail |
| RoyalRelic | `Prot_RoyalRelic` (L60) | dynamic 5 kg | as the vase with `fragileImpulse` 6 Ns | Gold relic + purple aura |

- **Not affected by:** explosions (D-020), debris fragments (D-008), fire (not burnable), wind.
- On fail: `ProtectedLost(kind, reason)` → 0.5 s slow-mo focus (`02` §13) → Fail panel with the reason copy (`fail.vase_broke`, `fail.fox_woke`, `fail.relic_broke`).
- Colour-assist mode: a thicker outline + icon badge (`05`).

### 8.19 Levers / counterweights (D-022)
- `Struct_Lever_Timber_4x0.25`: plank + `HingeJoint` (axis Z) to a static pivot block. Limits ±35° (designer 10–60°).
- Counterweight puzzle: a weight hangs from a `RopeCuttable` on one end, the payload on the other. Cutting the rope releases the lever. Or drop a weight on one end to catapult (launch speeds are deterministic; validate in the bot).
- No pulleys (D-022).

### 8.20 Shielded target pattern
- No new component (S-05): an objective physically covered by Structure pieces or a static/kinematic metal plate. "Only vulnerable after its cover is displaced" emerges from occlusion.
- Validator: if an objective has no unobstructed line from any aim in the cone *and* no intended shot clears it, warn "unreachable" (the solvability bot is the final proof).

---

## 9. Interaction matrices

### 9.1 Arrow type × target (outcomes; ✗ = no interaction / blocked like a solid)

| Target ↓ / Arrow → | Oak | Heavyhead | Split (parent/children) | Fire | Bounce |
|---|---|---|---|---|---|
| Straw | Embed + push | Embed + strong push/damage | Embed | Embed + **ignite** (burns 2 s) | Embed |
| Timber | Embed (θ ≤ 60°) / deflect | Break if damage ≥ HP, else embed | Embed / deflect | as Oak (✗ ignite) | as Oak |
| Stone | Deflect, chip | Stop, strong push | Deflect | as Oak | as Oak |
| Ice | Shatter if HP ≤ dmg, else deflect | Shatter likely | Shatter/deflect | as Oak (✗ melt) | as Oak |
| Metal (incl. shields) | Ricochet if γ ≤ 25° else stop | Stop (✗ ricochet) | children ricochet ≤ 1 | as Oak | **Guaranteed ricochet ×0.95**, once |
| Earth / ground | Embed | Embed | Embed | Embed, flame fizzles | Embed |
| Rope | Cut, pass | Cut, pass | Cut, pass | Cut, pass (+ ember VFX) | Cut, pass |
| Chain | Cut, pass | Cut, pass | Cut, pass | Cut, pass | Cut, pass |
| Balloon | Pop, pass | Pop, pass | Pop, pass | Pop, pass | Pop, pass |
| Oil jar | Break → fire zone | Break → fire zone | Break → fire zone | Break → **big** fire zone | Break → fire zone |
| Powder barrel | Explode | Explode | Explode | Explode | Explode |
| Crest target | Break (clear) | Break | Break | Break | Break |
| Cursed orb | Shatter (clear), pass ×0.7 | Shatter | Shatter (children count) | Shatter | Shatter (also after a bounce) |
| Hanging lantern | Shatter (clear), pass ×0.7 | Shatter | Shatter | Shatter | Shatter |
| Training dummy | Embed + push (straw) | Strong push | Embed | Embed + **ignite** (straw dummy burns → broken → clear) | Embed |
| Supply crate | as its material | as material | as material | as material | as material |
| Royal vase / relic | **FAIL** | **FAIL** | **FAIL** | **FAIL** | **FAIL** |
| Sleeping fox | **FAIL** | **FAIL** | **FAIL** | **FAIL** | **FAIL** |
| Portal ring (front) | Teleport | Teleport | Teleport | Teleport | Teleport |
| Portal frame / back | Stone rules | Stone rules | Stone rules | Stone rules | Stone rules |
| Wind field | Bends ×1.0 | Bends ×0.25 | Bends ×1.0 / ×1.4 | Bends ×1.0 | Bends ×1.0 |
| Water / spikes / pit | Removed | Removed | Removed | Removed (extinguished) | Removed |

### 9.2 Effect × object (✗ = explicitly no interaction)

| Object ↓ / Effect → | Fire (zone or spread) | Explosion | Wind | Falling Stone/heavy body | Kill zone | Kinematic mover contact |
|---|---|---|---|---|---|---|
| Straw structure | Burns 2 s → breaks | Damage ×1.5 + force | ✗ | Damage (collision) | Removed | Pushed |
| Timber structure | ✗ | Damage + force | ✗ | Damage | Removed | Pushed |
| Stone structure | ✗ | Damage ×0.5 + force | ✗ | Damage | Removed | Pushed |
| Ice structure | ✗ (no melt) | Damage ×1.5 + force | ✗ | Shatter if ≥ 14 Ns | Removed | Slides |
| Metal piece | ✗ | Force only | ✗ | ✗ damage | Removed | Pushed |
| Rope | Burns 1.2 s → cut | Cut if ≤ 1.2 m | ✗ | Cut only if its anchor/attached body breaks | n/a | ✗ |
| Chain | ✗ | Cut if ≤ 1.2 m | ✗ | as rope | n/a | ✗ |
| Balloon | Pops (≤ 0.4 m) | Pops (in radius) | Pushed | Pops on contact > 1 m/s | n/a | Pops on contact |
| Oil jar | Breaks → bigger fire | Breaks → fire zone | ✗ | Breaks ≥ 5 Ns | Removed (no fire in water) | Pushed |
| Powder barrel | Fuse 0.4 s → explode | Chain-explode after 0.15 s | ✗ | Explode ≥ 8 Ns | Removed (✗ explode in water; spikes → explode) | Pushed |
| Boulder | ✗ | Force | ✗ | ✗ | Removed | Pushed |
| Crest target | ✗ | Break if impulse ≥ 6 | ✗ | Break ≥ 6 Ns | Cleared | Pushed |
| Cursed orb | ✗ | ✗ | ✗ | ✗ | Cleared (+ validator flag) | ✗ |
| Lantern | ✗ | Break | ✗ | Break ≥ 2 Ns | Cleared | Break if > 2 m/s |
| Training dummy | Burns → broken → cleared | Damage + force | ✗ | Damage / knock-over | Cleared | Pushed |
| Vase / relic | ✗ | **✗ (D-020)** | ✗ | Fail ≥ 4/6 Ns | **Fail** | Pushed (fail on impulse) |
| Sleeping fox | ✗ | **✗ (D-020)** | ✗ | Displacement/tilt fail | **Fail** | Pushed (fail on displacement) |
| Debris fragment | ✗ | ✗ | ✗ | ✗ | Removed (splash only) | ✗ |
| Spent arrow | ✗ | ✗ | ✗ | ✗ | Removed | ✗ |
| Embedded arrow | rides its host | rides its host | ✗ | rides its host | removed with its host | rides |

### 9.3 Prop × prop highlights (designer cheat-sheet)
- Fire arrow → straw wick → rope (spread 0.6 m) → load falls (the "Burning Bridge" template). The wick must be ≤ 0.6 m from the rope. The wick ignites on the hit. The rope ignites after 0.5 s of exposure, detected on the next 0.25 s spread tick (so 0.5–0.75 s after the hit), and burns 1.2 s → cut at ≈ 1.7–1.95 s. **Test window (FR-01): the load falls 1.7–2.5 s after the hit.** (The wick itself burns out at 2.0 s, after the rope has already ignited.)
- Barrel → barrel chain: 0.15 s per link, max 6 per level.
- Barrel → rope within 1.2 m: cut. Barrel → balloon in radius: pop.
- Oil jar → straw / dummy / rope / barrel: ignites after 0.5 s.
- Wind × balloon: drift. Wind × arrow: bend. Wind × everything else: ✗.
- Boulder × anything: heavy collision rules (damage per material). Boulder × protected: possible fail (designer responsibility; validator heuristic).

---

## 10. Protected-object behaviour summary
- Thresholds are per prefab (`fragileImpulse`, `maxDisplacement`, `maxTiltDeg`). Changing them requires a PO decision (fairness).
- Gentle bumps are allowed (< threshold). Designers keep heavy stone out of reach unless that is the puzzle.
- The `ProtectedTracker` latch is checked first every FixedUpdate (priority over win, `02` §1.3).
- QA: each protected kind has positive/negative threshold tests (§15).

## 11. Kill-zone behaviour summary
See §8.17. Invariants:
1. A body is removed **once** (`PlanarBody.Removed` latch).
2. The objective/protected result is decided at **trigger entry** (not on removal completion).
3. Removal never spawns gameplay bodies.
4. A kill zone spans the full width of its pit (validator: no gap where bodies can rest beside it inside the pit).

---

## 12. Determinism and replayability

| Risk | Effect | Mitigation | Verified by |
|---|---|---|---|
| PhysX float differences ARM vs x86 / iOS vs Android | The same input gives slightly different collapses | Solutions are designed with tolerance (aim window 8–12% of screen width, §7); no knife-edge balance puzzles; intended solutions are re-verified on device | Solvability bot ±jitter; on-device QA replay (dev build `ShotRecorder` replay) |
| Instantiation / registry order | Different solver ordering | Instantiate from the prefab in a fixed hierarchy order; the registry is sorted by hierarchy index; breaks are flushed in index order | `DeterminismReplayTests` |
| Frame-rate dependence | 30 vs 60 FPS gives different results | All gameplay in fixed steps; `fixedDeltaTime` is never changed; timers on `LevelClock` | Run the bot at 30 and 60 FPS caps → same win/lose |
| Hit-stop / slow-mo | Altered physics | `timeScale` only reduces the number of steps per real second, not Δt | `DeterminismReplayTests` with hit-stop on/off |
| Randomness | Unexplained outcomes | `UnityEngine.Random` is banned in gameplay folders; cosmetics use `CosmeticRandom` | `ArchitectureRulesTests` grep |
| Trigger callback ordering | Kill-zone vs objective order | Decisions are latched; evaluation is idempotent | Unit tests on the trackers |
| Sleep state differences | A body awake in one run, asleep in another | All bodies are put to sleep on load; spawn grace | Idle stability tests |
| Analog aim input | Players never replicate exactly | Wide windows + the preview + tolerance tests (canonical grid in [`07` §5.1](07_QA_PERFORMANCE_RELEASE.md#51-tolerance-definition), D-074: δθ = atan(0.4 m / d) ≥ 0.5°, δp ±0.03, 9 samples; all win, ≥ 7/9 within par) | `LevelSolvabilityTests` |
| Dictionary/HashSet iteration | Nondeterministic ordering | Lists only for gameplay iteration | Code review |
| Long chains (barrels, fire spread) | Small variations amplify | Chain delays are quantised to `TimedEvents` ticks (0.05 s grid); explosion queue 1 per step | Bot jitter tests on chain levels |

**Replayability tools:** `ShotRecorder` (editor-only, `Editor/Levels/`) records `IntendedShot {arrowType, angleDeg, power01, delayAfterPrevious}` (`04` §2.2) into `LevelData._intendedShots`. On device, dev builds replay them with `IntendedSolutionRunner` (DevOverlay ▸ "Run intended solution", `AB_DEV`), which is layer 2 of `07` §5.2.

---

## 13. Physics performance budgets

| Metric | Budget (per level unless stated) | Enforced by |
|---|---|---|
| Dynamic gameplay bodies | ≤ 45 typical, **≤ 60 hard** | Validator V-07 (error > 60, warning > 45) |
| Simultaneously awake bodies during a collapse | ≤ 40 | Profiling (DevOverlay counter) |
| Contact pairs | ≤ 300 | Profiler (`Physics.Processing`) |
| Joints (ropes count 1) | ≤ 12 (warning), 20 (error) | Validator |
| Ropes | ≤ 8 | Validator |
| Balloons | ≤ 9 | Validator |
| Kinematic movers | ≤ 4 | Validator |
| Portal pairs | ≤ 2 | Validator |
| Powder barrels / explosions | ≤ 6 | Validator |
| Simultaneous burning objects | ≤ 12 | Runtime cap |
| Wind fields | ≤ 3 (≤ 2 overlapping) | Validator |
| Debris fragments | 40 (Low 20) global | `DebrisPool` |
| Overlap queries per FixedUpdate | ≤ 20 | Code review + profiler marker |
| Preview sweeps while drawing | ≤ 150/frame (≤ 0.3 ms Mid) | `02` §3 |
| Physics time (Mid) | ≤ 3.5 ms avg, ≤ 5 ms worst collapse frame | PlayMode perf smoke + device profiling |
| GC alloc in physics paths | 0 B | Profiler + `NonAlloc` APIs |

"Validator" rows other than V-07 (joints, ropes, balloons, movers, portal pairs, barrels, wind fields, hinge axis = ±Z, mover sweep vs resting bodies, kill-zone coverage) are **physics budget rules that `LevelValidator` (AB-059) must implement**. They are proposed for `04` §9 as V-23…V-27; until they are added there, this table is the spec.

---

## 14. Debug tooling

| Tool | Where | What |
|---|---|---|
| DevOverlay — Physics page (`AB_DEV`) | runtime | FPS, physics ms (last/avg/max), awake bodies / tracked, contacts, calm timer bar, state, arrows active/embedded/decor, debris count, explosions queued, burning count |
| Physics gizmos (editor + dev builds) | `PhysicsDebugDraw` (part of DevOverlay) | Sleep colour (green asleep / yellow awake / red moving > calm threshold), last 1 s of impulse arrows (magnitude labels), HP bars over breakables, explosion radii, rope tension colour, wind vectors, mover paths, portal mappings, clear line, kill-zone bounds, play bounds |
| Slow-mo / step | DevOverlay buttons | 0.25× time, pause + single FixedUpdate step |
| "Show intended shots" | DevOverlay + LevelEditorWindow | Ghost arcs of `LevelData.intendedShots` |
| "Run intended solution" (`IntendedSolutionRunner`) | DevOverlay | Plays recorded shots on device (QA, `07` §5.2 layer 2) |
| Cheats (`AB_CHEATS`) | DevOverlay | +arrows, infinite arrows, unlock all, clear objective, break all, level select |
| Sandbox scenes | `Scenes/Sandbox/Sandbox_PHYS_*`, `Sandbox_PROPS_*` | Per-object test rigs (never in the build) |
| `LevelValidator` (editor) | Arrow Buster ▸ Levels ▸ Validate All / Validate Selected | Physics checks: z-plane, PlanarBody/MaterialBody present, layers, penetration at load, mass ratios, body/joint/rope/... budgets, hinge axes, mover sweeps vs bodies, clear line placement, kill-zone coverage, blast radius vs protected, unpacked prefabs, orphan colliders on Default layer |
| `PhysicsStatsLogger` (dev) | runtime → CSV in `persistentDataPath` | Per-shot: max awake bodies, physics ms max, calm time, debris spawned — feeds `07` perf reports |

---

## 15. Test matrix per object type

PlayMode tests live in `Tests/PlayMode/InteractionMatrixTests.cs` (one fixture per object family, using prefab rigs spawned in an empty test scene with ground). Shots are injected with `ArrowSpawner.FireForTest(type, pos, vel)`. PX rows ship with AB-014 (M1 spike). Object rows are written by the owning ticket (§8 ticket map) and are collected and run by QA in AB-063 (W1), AB-095 (W2) and AB-109 (W3). "Auto" = automated; "Manual" = device/visual checklist in `07`.

| ID | Object | Test name | Setup | Expected | Type |
|---|---|---|---|---|---|
| PX-01 | Plane constraint | `PlanarBody_StaysOnPlane` | 20 random crates collapse | All \|z\| < 0.001, x/y rotation 0 after 5 s | Auto |
| PX-02 | Stack stability | `Tower10_IdleStable` | 10-crate tower, no shot, 10 s | Drift < 1 cm, no wake | Auto |
| PX-03 | Spawn safety | `Overlap2mm_NoExplosion` | 0.002 m overlaps | Max speed < 0.5 m/s | Auto |
| PX-04 | Determinism | `SameShotTwice_SameHash` | Recorded shot ×20 | ≥ 19/20 identical hash | Auto |
| PX-05 | Determinism | `HitStopOnOff_SameHash` | Same shot ± hit-stop | Identical hash | Auto |
| MT-01 | Straw | `Oak_EmbedsStraw_AndPushes` | Straw bale on ground, head-on | Embedded, Δv > 0 | Auto |
| MT-02 | Timber | `Oak_EmbedsTimber_Under60` / `Oak_DeflectsTimber_Over60` | 1×1 crate, θ 30° / 75° | Embed / Spent | Auto |
| MT-03 | Timber | `CrateFall3m_CracksNotBreaks` / `CrateFall3mTwice_Breaks` | Drop tests | Damaged / Broken | Auto |
| MT-04 | Stone | `Oak_DeflectsStone` / `Heavyhead_PushesStone` | 1×1 stone on a ledge | Spent; block moves ≥ 0.5 m off the ledge | Auto |
| MT-05 | Ice | `IceBlock_SlidesOnTimber` / `IceFall_Shatters` | 10° slope; 1.5 m drop | Slides > 1 m; shatters into shards | Auto |
| MT-06 | Metal | `Oak_RicochetsGrazing` / `Oak_StopsHeadOn` / `Bounce_AlwaysRicochetsOnce` | Plate at 15° / 80° / 60° grazing | Ricochet / Spent / exactly 1 bounce | Auto |
| MT-07 | Debris | `Debris_DoesNotTriggerObjectives` | Break a crate above a crest target | Crest intact; debris returns to the pool ≤ 2.5 s | Auto |
| OB-01 | CrestTarget | `Crest_ArrowHit_Clears` / `Crest_FallingCrate_Clears` / `Crest_LightBump_Survives` | Standing crest | Cleared / cleared / intact (impulse 3 Ns) | Auto |
| OB-02 | SupplyCrate | `Crate_BelowClearLine_Clears` / `Crate_KillZone_Clears` | Push off a ledge | Cleared on line cross / entry | Auto |
| OB-03 | Lantern | `Lantern_RopeCut_FallBreaks` / `Lantern_SoftLand_Survives` | Lantern hung 3 m / 0.3 m above straw | Cleared / intact | Auto |
| OB-04 | CursedOrb | `Orb_ImpactIgnored` / `Orb_BlastIgnored` / `Orb_ArrowClears` / `Orb_BounceArrowClears` | Orb + falling crate / barrel / arrow / bounce | ✗ / ✗ / cleared / cleared | Auto |
| OB-05 | Dummy | `Dummy_KnockedOver_Clears` / `Dummy_Wobble_DoesNotClear` / `Dummy_TouchGround_Clears` | Push hard / light hit / knock off a platform | Cleared / not cleared (tilt < 70°) / cleared | Auto |
| OB-06 | BannerRope | `Banner_Cut_Clears` / `Banner_Burn_Clears` | Rope objective | Cleared ≤ 1 step / ≤ 1.3 s | Auto |
| RP-01 | Rope | `Rope_ArrowCut_ReleasesLoad` / `Rope_ArrowPassesThrough` | Crate hung by rope | Load falls; arrow continues ≥ 95% speed | Auto |
| RP-02 | Rope | `Rope_TwoRopes_SwingThenDrop` | Two ropes, cut one | Swings, does not fall; cut the second → falls | Auto |
| RP-03 | Chain | `Chain_NotBurnable` | Fire zone under a chain | Not cut after 5 s | Auto |
| BL-01 | Balloon | `Balloon_HoldsLoad_Calm` | 2-balloon crate with tether | Calm reached ≤ 1 s (ambient) | Auto |
| BL-02 | Balloon | `Balloon_PopOrder_DropsLoad` | Pop 1 then 2 | Partial sink then fall | Auto |
| BL-03 | Balloon | `Balloon_Wind_Drifts` | Wind (3, 0) | Load x increases; no wind effect on a nearby crate | Auto |
| OJ-01 | Oil jar | `OilJar_ArrowBreaks_FireZone` | Jar on a ledge above a straw bale | FireZone spawns; straw burns → broken ≤ 3 s | Auto |
| OJ-02 | Oil jar | `FireArrow_OilJar_BigZone` | Fire arrow hit | Zone width 2.25 m, 4 s | Auto |
| FR-01 | Fire | `FireArrow_StrawWick_BurnsRope` | "Burning Bridge" rig | Load falls 1.7–2.5 s after the hit | Auto |
| FR-02 | Fire | `Fire_NeverIgnitesTimberStoneIceMetal` | Fire zone touching each | No change after 6 s | Auto |
| FR-03 | Fire | `Fire_SpreadCap12` | 20 straw bales in a row | ≤ 12 burning at once | Auto |
| PB-01 | Barrel | `Barrel_ArrowExplodes_DamagesStack` | Barrel under a timber stack | Stack broken/displaced | Auto |
| PB-02 | Barrel | `Barrel_Chain_015sDelay` | 3 barrels 2 m apart | 3 explosions ≥ 0.15 s apart | Auto |
| PB-03 | Barrel | `Barrel_ProtectedImmune` | Vase + fox at 1 m | No fail from the blast (D-020) | Auto |
| PB-04 | Barrel | `Barrel_CutsRopeInInnerRadius` | Rope at 1.0 m / 1.5 m | Cut / intact | Auto |
| BD-01 | Boulder | `Boulder_PegBreak_HeavyheadReleases` | "Boulder Run" rig | Boulder rolls the lane; targets cleared | Auto |
| BD-02 | Boulder | `Boulder_OakDoesNotBreakPeg` | Same rig, Oak | Peg intact after 1 Oak hit (5.4 < HP 8); broken after 2. This is intentional: Heavyhead = 1 arrow (3★ route); 2 Oaks = a less efficient alternate route | Auto |
| WD-01 | Wind | `Wind_BendsOak_NotHeavyhead` | Wind (4, 0); same aim | Oak lands ≥ 1 m further than in no-wind; Heavyhead shifts < 0.3 m | Auto |
| WD-02 | Wind | `Wind_PreviewMatchesFlight` | Wind field | Parity ≤ 1 mm | Auto |
| SH-01 | Shield | `Mover_PoseDeterministic` | Pose at t = 1.234 s in two runs | Identical | Auto |
| SH-02 | Shield | `Mover_BlocksArrow_Timed` | Rotating shield crossing the aim line | Blocked at t1, passes at t2 | Auto |
| IP-01 | Ice slide (static, D-101) | `IceSlide_CrateSlidesToEdge` | Crate released on a 15° static ice ramp | Crate slides ≥ 2 m and leaves the ramp within 2.5 s; same end hash on repeat | Auto |
| PT-01 | Portal | `Portal_TeleportsArrow_PreservesSpeed` / `Portal_BackSide_Blocks` / `Portal_BodiesIgnore` | Pair rig | Exit velocity rotated, \|v\| equal / blocked / a crate passes through the disc | Auto |
| PT-02 | Portal | `Portal_LoopCap3` | Facing pair | ≤ 3 teleports, then the arrow continues/resolves | Auto |
| PV-01 | Vase | `Vase_GentleBump_Survives` / `Vase_StoneFall_Fails` / `Vase_ArrowHit_Fails` | Impulse 3 Ns / stone 2 m drop / arrow | Intact / fail / fail | Auto |
| PF-01 | Fox | `Fox_Displaced06_Fails` / `Fox_Nudge_Survives` / `Fox_Tilt45_Fails` | Push 0.7 m / 0.3 m / tip | Fail / ok / fail | Auto |
| KZ-01 | Water | `Water_ObjectiveCleared_ProtectedFails_ArrowRemoved` | Drop each into water | Cleared / fail / removed | Auto |
| KZ-02 | Spikes | `Spikes_BreaksAndRemovesArrows` | Crate + arrow into spikes | Broken (debris), arrow removed | Auto |
| KZ-03 | Bounds | `PlayBounds_RemovesOffscreen` | Fling a crate sideways | Removed; objective cleared if it was one | Auto |
| LV-01 | Lever | `Lever_CounterweightRopeCut_Launches` | Lever rig | Payload rises ≥ 1 m; repeatable hash | Auto |
| VS-ALL | Visual readability | Material colours, ≤ 3 states, VFX not hiding objectives | Device screenshots per object | Checklist pass | **Manual** |
| SFX-ALL | Audio | Each material/prop SFX audible, no clipping in chains | Device | Checklist pass | **Manual** |

Coverage rule: a new object type can't be merged without (a) a sandbox scene, (b) ≥ 2 automated rows above, (c) validator rules for its budgets, and (d) an entry in the interaction matrices (§9).
