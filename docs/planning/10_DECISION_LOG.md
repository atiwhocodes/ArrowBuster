# 10 — Decision Log

> Append-only record of architecture, scope and design decisions. Process: see [`../agents/AGENT_SYSTEM.md`](../agents/AGENT_SYSTEM.md) §9.
> **Statuses:** `Accepted` (in force) · `Proposed` (in force as the working default, **awaiting owner confirmation**) · `Superseded by D-xxx` · `Rejected`.
> **Owners** use agent role IDs: PO, ARCH, CORE, PHYS, PROPS, SYS, UI, LEVEL, ART, PLAT, MON, QA, INT, and **OWNER** (the human project owner).
> Never edit the decision text of an accepted entry. To change a decision, add a new entry that supersedes it.

## Template

```markdown
### D-0XX — <short title>
| Field | Value |
|---|---|
| Decision ID | D-0XX |
| Date | YYYY-MM-DD |
| Context | Why a decision was needed; the mvp.md § reference |
| Decision | What we will do (imperative, testable) |
| Alternatives considered | Option — why not |
| Consequences | What this enables, costs, constrains; follow-up tickets |
| Owner | Role ID |
| Status | Proposed / Accepted / Superseded by D-0YY / Rejected |
```

## Index

| ID | Title | Owner | Status |
|---|---|---|---|
| D-001 | `docs/` lowercase is the documentation root | ARCH | Accepted |
| D-002 | `mvp.md` is the single design source; `docs/GDD.md` becomes a pointer | PO | Accepted |
| D-003 | URP Forward renderer, one real-time light, faked DOF | ARCH | Accepted |
| D-004 | 2.5D: 3D PhysX bodies locked to the z = 0 plane | ARCH | Accepted |
| D-005 | Kinematic swept arrow; shared solver for preview and flight | CORE | Accepted |
| D-006 | Physics settings baseline (1/60 s, solver 8/2, enhanced determinism, start asleep) | PHYS | Accepted |
| D-007 | Input via `Pointer.current` + `BowInputReader`; remove the template input asset | CORE | Accepted |
| D-008 | Cosmetic debris collides only with Environment and never deals damage | PHYS | Accepted |
| D-009 | Ropes = joint + trigger capsule + LineRenderer | PROPS | Accepted |
| D-010 | Level = `LevelData` SO + layout prefab; JSON only for saves | ARCH | Accepted |
| D-011 | Restart and next level re-instantiate the layout; no scene reload | CORE | Accepted |
| D-012 | uGUI + TextMeshPro for runtime UI | UI | Accepted |
| D-013 | No Addressables in MVP | ARCH | Accepted |
| D-014 | Special-arrow first appearances follow §6 map beats (L13/L30/L36/L47) | PO | Accepted (owner 2026-10-09, D-082) |
| D-015 | Out-of-arrows fail confirmation timing | CORE | Accepted |
| D-016 | Player may fire during resolution after a 0.35 s re-nock cooldown | PO | Superseded by D-083 |
| D-017 | Preview shows wind, portal pass-through and Bounce ricochet | PO | Superseded by D-085 |
| D-018 | Draw starts anywhere in the lower aim zone; relative drag | CORE | Proposed |
| D-019 | Split Arrow uses a timed split shown in the preview | PO | Superseded by D-088 |
| D-020 | Powder blasts neither damage nor push protected objects | PO | Accepted (owner 2026-10-09, D-092) |
| D-021 | Spring plate is Should-have, first cut candidate | PO | Superseded by D-086 |
| D-022 | Counterweights = hinge levers + rope-hung weights; no pulleys | PROPS | Accepted |
| D-023 | Quiver is consumed in authored order; no in-level arrow swap | PO | Proposed |
| D-024 | Clears using a rewarded bonus arrow are capped at 1★ | PO | Superseded by D-084 |
| D-025 | Interstitial policy defaults | MON | Superseded by D-093 |
| D-026 | Coin reward values | SYS | Proposed |
| D-027 | Daily challenge minimal scope | SYS | Accepted (owner 2026-10-09, D-094) |
| D-028 | One Runtime asmdef + Integrations asmdef for vendor SDKs | ARCH | Accepted |
| D-029 | Static `Services` locator; no DI framework | ARCH | Accepted |
| D-030 | `GameEvents` static bus for cross-cutting listeners only | ARCH | Accepted |
| D-031 | In-house `UiTween`; LitMotion is the approved fallback | ARCH | Accepted |
| D-032 | In-house native haptics bridge | PLAT | Accepted |
| D-033 | Milestone plan restructured around a Vertical Slice gate at M3 | PO | Superseded by D-102 |
| D-034 | Git + LFS + worktree-per-agent workflow; `main` protected by the Integrator | INT | Accepted (owner 2026-10-09; done in AB-001) |
| D-035 | Vendor SDK selection deferred to the M8 gate; interfaces + mocks from M2 | MON | Superseded by D-090 |
| D-036 | Arrow flight time: test MVP 1.5–2.5 s against a snappier profile at the M1 gate | PO | Proposed |
| D-037 | Objective clear rules per kind | PO | Accepted |
| D-038 | Protected-object fail rules | PO | Accepted |
| D-039 | Kill-zone and clear-line semantics | PO | Accepted |
| D-040 | Trajectory preview ends at the first collider hit, with an impact marker | CORE | Superseded by D-085 |
| D-041 | Low-FOV perspective camera fitted to play-area width; pillarbox on tablets | CORE | Accepted |
| D-042 | Asset originality and licensing policy | OWNER | Superseded by D-089 |
| D-043 | Stay on Unity 6000.6.5f1; LTS upgrade spike at M9 start | ARCH | Superseded by D-080 |
| D-044 | Specialist subagents defined in `.claude/agents/` | INT | Accepted |
| D-045 | Bullseye medals in MVP as data + simple collection view | PO | Accepted |
| D-046 | Choice levels deferred (schema reserved) | PO | Accepted |
| D-047 | Level numbering: world-local IDs + global index | LEVEL | Accepted |
| D-048 | Collision layer set and matrix | PHYS | Accepted |
| D-049 | Arrows are not affected by explosions or physics after resolution | CORE | Accepted |
| D-050 | Vertical-slice levels are real World 1 levels, not throwaway | LEVEL | Accepted |
| D-051 | Colour-language reconciliation for materials | PO | Proposed |
| D-052 | Naming additions: world codes and environment/marker prefabs | ARCH | Accepted |
| D-053 | No ranger avatar in MVP; the fox is the brand mascot | PO | Proposed |
| D-054 | Colour-assist outline implementation | UI | Accepted |
| D-055 | Audio import presets and loudness targets | ART | Accepted |
| D-056 | Placeholder build check | ART | Accepted |
| D-057 | Hit-stop under Reduced Motion | PO | Proposed |
| D-058 | First launch drops straight into W1_L01 | PO | Proposed |
| D-059 | `MaterialKind.Earth` for environment ground | PHYS | Proposed |
| D-060 | No gameplay fracture in MVP | PHYS | Accepted |
| D-061 | `TimeScaleController` is the only writer of `Time.timeScale` | ARCH | Accepted |
| D-062 | Rewarded bonus arrow: type and resume behaviour | PO | Proposed |
| D-063 | Rewarded +1 arrow viable-state heuristic | PO | Proposed |
| D-064 | World-completion bow skins are also coin-buyable | SYS | Proposed |
| D-065 | Starter Pack restore semantics | MON | Accepted |
| D-066 | Single per-level remote-config knob | PO | Accepted |
| D-067 | Offline first launch in a consent region | MON | Accepted |
| D-068 | Stale file locks | INT | Accepted |
| D-069 | Squash-merge per ticket; short-lived branches | INT | Accepted |
| D-070 | `GameEnums` values are append-only | ARCH | Accepted |
| D-071 | XL tickets are split before Ready | INT | Accepted |
| D-072 | Store accounts and IAP products are created early | OWNER | Superseded by D-100 |
| D-073 | Trajectory preview floor 0.4 | PO | Proposed |
| D-074 | Canonical solvability tolerance | QA | Accepted |
| D-075 | Level classification enums live in `Levels/LevelEnums.cs` | LEVEL | Accepted |
| D-076 | Vertical-slice tutorial scope | PO | Accepted |
| D-077 | Shielded target is a pattern, not a component | PO | Accepted |
| D-078 | Gate naming | INT | Superseded by D-102 |
| D-079 | Google Play closed-testing requirement is a schedule risk | OWNER | Superseded by D-099 |
| D-080 | Unity 6000.6.5f1 locked for the whole MVP | OWNER | Accepted |
| D-081 | Public name vs technical identifiers | OWNER | Accepted |
| D-082 | Special-arrow first appearances locked | OWNER | Accepted |
| D-083 | Re-nock during active physics; no firing after the outcome is decided | OWNER | Accepted |
| D-084 | Bonus-arrow clears capped at 1★, disclosed before the ad | OWNER | Accepted |
| D-085 | Preview shows wind, portal exits and the first bounce | OWNER | Accepted |
| D-086 | Spring Plates cut from the launch MVP | OWNER | Accepted |
| D-087 | No pulley simulation | OWNER | Accepted |
| D-088 | Timed Split Arrow with visible split feedback | OWNER | Accepted |
| D-089 | Asset licensing and AI-generated content policy | OWNER | Accepted |
| D-090 | Vendors: Firebase + Unity LevelPlay + Unity IAP behind adapters | OWNER | Accepted |
| D-091 | Launch IAP pricing | OWNER | Accepted |
| D-092 | Powder blasts never touch protected objects directly | OWNER | Accepted |
| D-093 | Interstitial policy (final) | OWNER | Accepted |
| D-094 | Daily Challenge scope (cut-first) | OWNER | Accepted |
| D-095 | Rename Royal Violet → Royal Amethyst | OWNER | Accepted |
| D-096 | Consent-gated analytics, ad personalisation and crash reporting (UK/EEA) | OWNER | Accepted |
| D-097 | Privacy policy hosted before SDK integration | OWNER | Accepted |
| D-098 | Test and launch markets | OWNER | Accepted |
| D-099 | Verify Google Play testing requirements in Week 1 | OWNER | Accepted |
| D-100 | Mac + Apple Developer account by week 3; store accounts and IAP products by week 5 | OWNER | Accepted |
| D-101 | Official cut-first scope and launch cosmetic set | OWNER | Accepted |
| D-102 | Execution order and milestone restructure | OWNER | Accepted |
| D-103 | Locked technical direction | OWNER | Accepted |
| D-104 | Final owner confirmations: Starter Pack positioning, cosmetic-only rule, hard cuts, four delivery phases | OWNER | Accepted |
| D-105 | Oak impact tuning and the timber "weak point" pole | CORE / PHYS | Proposed |
| D-106 | Vertical-slice layouts deviate from the 11 sketches (approach-path rule) | LEVEL | Proposed |
| D-107 | Procedural placeholder art, synthesised audio and code-built UI for the vertical slice | ART / UI | Proposed |
| D-108 | Implementation conventions adopted during the VS build | ARCH | Proposed |

---

### D-001 — `docs/` lowercase is the documentation root
| Field | Value |
|---|---|
| Decision ID | D-001 |
| Date | 2026-10-09 |
| Context | The repo had `Docs/` (from M0). The planning brief specifies `docs/...` paths. Windows is case-insensitive, but the iOS build happens on a Mac and CI may run on Linux. |
| Decision | Rename `Docs/` → `docs/` (done 2026-10-09). All references use lowercase `docs/`. |
| Alternatives considered | Keep `Docs/` and write `docs/` paths anyway — breaks on case-sensitive file systems and confuses agents. |
| Consequences | CLAUDE.md, the playbook and the slash commands were updated. Git records the lowercase path from the first commit (git is not yet initialised, so there is no history issue). |
| Owner | ARCH |
| Status | Accepted |

### D-002 — `mvp.md` is the single design source; `docs/GDD.md` becomes a pointer
| Field | Value |
|---|---|
| Decision ID | D-002 |
| Date | 2026-10-09 |
| Context | `docs/GDD.md` and `/mvp.md` were byte-identical except for "Arrow Buster" vs "ArrowBuster". Two copies will drift. |
| Decision | `/mvp.md` is the product source of truth. `docs/GDD.md` is replaced by a short pointer file. Section numbers (`§N`) refer to `mvp.md`. Edits to `mvp.md` require OWNER approval and a decision entry. |
| Alternatives considered | Keep both in sync — error-prone. Delete GDD.md — breaks habits and old prompts. |
| Consequences | No content was lost (the diff was only the name spelling). |
| Owner | PO |
| Status | Accepted |

### D-003 — URP Forward renderer, one real-time light, faked DOF
| Field | Value |
|---|---|
| Decision ID | D-003 |
| Date | 2026-10-09 |
| Context | §11 wants a stylised 3D toy diorama with shallow DOF on the background. §12 wants 60 FPS on iPhone 11 and 30 FPS on low-end. |
| Decision | URP Forward. One real-time directional light (shadows on High/Mid only). Stylised Shader Graph lit shader with a per-world palette texture. Background "DOF" comes from pre-blurred art layers + fog, never a real-time DOF pass. Bloom on High only. |
| Alternatives considered | Built-in RP (legacy). HDRP (not mobile). Real-time DOF post (≈1–2 ms on Mid, unreadable on Low). |
| Consequences | Art must author blurred background cards. Per-tier URP assets (`Mobile_High/Mid/Low_RPAsset`) are created in M9. |
| Owner | ARCH |
| Status | Accepted |

### D-004 — 2.5D: 3D PhysX bodies locked to the z = 0 plane
| Field | Value |
|---|---|
| Decision ID | D-004 |
| Date | 2026-10-09 |
| Context | §12 recommends 3D rigidbodies constrained to a play plane. The brief requires an explicit 2D/2.5D/3D recommendation. |
| Decision | Gameplay bodies: `FreezePositionZ | FreezeRotationX | FreezeRotationY`, transform z = `GameConstants.PlayPlaneZ`, enforced by `PlanarBody` and the validator. Cosmetic debris alone is unconstrained (Debris layer). |
| Alternatives considered | Pure 2D/Box2D (best stability, loses the diorama look) — **documented fallback** if the M1 physics spike fails. 3D visuals + 2D physics (dual worlds, young API). Full 3D (unreadable, drift). |
| Consequences | Needs stacking-stability discipline (D-006). The M1 spike must hit its gates: a 10-crate tower idle for 10 s with < 1 cm drift, no explosive depenetration on spawn, collapse resolves in < 3 s. |
| Owner | ARCH |
| Status | Accepted |

### D-005 — Kinematic swept arrow; shared solver for preview and flight
| Field | Value |
|---|---|
| Decision ID | D-005 |
| Date | 2026-10-09 |
| Context | The arrow must never tunnel, the preview must match the flight exactly, and portals, wind and ricochet must be predictable. |
| Decision | The flying arrow is not a dynamic rigidbody. `ArrowProjectile` integrates position/velocity in `FixedUpdate` via `BallisticSolver.Step(state, dt, env)` (semi-implicit Euler, gravity + `WindField` acceleration) and sweeps each step with `Physics.SphereCastNonAlloc` (radius 0.06 m) against the arrow cast mask. On a hit, `ArrowImpactResolver` picks an outcome (embed / deflect / ricochet / cut-and-continue / pop-and-continue / portal / break target) and applies an explicit, clamped impulse to the hit rigidbody at the contact point. `TrajectoryPreview` calls the same `Step` with the same dt. Spent (non-embedded) arrows become a short-lived dynamic body on the `Arrow` layer (collides with Environment only) that fades out after 1.5 s. |
| Alternatives considered | Dynamic rigidbody arrow + continuous collision detection — CCD still misses thin ropes, the preview drifts from the result, and ricochet/portal are hard to control. |
| Consequences | Impulse transfer is a tunable (`ArrowDefinition.impulseScale`, `MaterialProfile.impulseTransfer`). An EditMode parity test asserts preview == flight. Arrows never push objects except through resolved impulses. |
| Owner | CORE |
| Status | Accepted |

### D-006 — Physics settings baseline
| Field | Value |
|---|---|
| Decision ID | D-006 |
| Date | 2026-10-09 |
| Context | Defaults are Δt 0.02, solver 6/1, enhanced determinism off. Stacks and repeatable solutions need more. |
| Decision | Fixed Δt = 1/60 s. Maximum allowed Δt = 0.1 s. Default solver iterations 8, velocity iterations 2. Enhanced determinism ON. Bounce threshold 1.0. Sleep threshold 0.005 (default). Default `maxDepenetrationVelocity` 3 m/s on gameplay bodies. Default max angular speed 25 rad/s. **All gameplay bodies are put to sleep on level load** and wake on contact. Auto sync transforms OFF. Interpolation off by default (on only for large visible falling props where tests show stutter). |
| Alternatives considered | 50 Hz (cheaper, coarser arrow sweep, visible stutter at 60 FPS). TGS solver — evaluate in the M1 spike as an A/B option (it may improve stacking). |
| Consequences | Physics cost is ~20% higher than at 50 Hz; within budget at ≤ 60 bodies. The Low tier at 30 FPS runs 2 physics steps per frame. Applied by `PhysicsSetup` (editor) in AB-002. |
| Owner | PHYS |
| Status | Accepted |

### D-007 — Input via `Pointer.current` + `BowInputReader`
| Field | Value |
|---|---|
| Decision ID | D-007 |
| Date | 2026-10-09 |
| Context | CLAUDE.md mandates the Input System and `Pointer.current`. The template's `InputSystem_Actions.inputactions` is a generic FPS map. |
| Decision | `BowInputReader` polls `Pointer.current` (press, position) in `Update` and outputs `PointerSample` structs. It ignores presses that start over UI (`EventSystem.IsPointerOverGameObject(pointerId)`) and ignores secondary touches. Remove the template action asset. UI uses `InputSystemUIInputModule` with its default actions. |
| Alternatives considered | An action asset with Press/Position actions (more indirection, nothing gained for one pointer). EnhancedTouch (needed only for multi-touch). |
| Consequences | `DrawModel` stays pure and testable (input samples in → `AimState` out). |
| Owner | CORE |
| Status | Accepted |

### D-008 — Cosmetic debris collides only with Environment and never deals damage
| Field | Value |
|---|---|
| Decision ID | D-008 |
| Date | 2026-10-09 |
| Context | §11 wants debris to clear cleanly. Fragments hitting gameplay bodies create unexplained chain reactions and cost CPU. |
| Decision | Break fragments live on the `Debris` layer (collide with `Environment` only), are unconstrained in Z, use seeded cosmetic randomness, fade after 1.2–2.0 s and are pooled (cap 40 / Low 20). Gameplay-relevant falling objects are whole structure pieces, never fragments. |
| Alternatives considered | Fragments as gameplay bodies (more emergent, less fair, perf risk). |
| Consequences | "Falling debris" in §4 (supply crate, lantern) means falling **structure pieces**. Documented in `03` and in the level-design rules. |
| Owner | PHYS |
| Status | Accepted |

### D-009 — Ropes = joint + trigger capsule + LineRenderer
| Field | Value |
|---|---|
| Decision ID | D-009 |
| Date | 2026-10-09 |
| Context | Ropes/chains hold loads, bridges and counterweights. Segment-chain ropes are jittery and expensive. |
| Decision | `RopeCuttable` connects anchor (static point or body) → load body with a `ConfigurableJoint` (linear limit = rope length, soft spring for slight give). The cut detector is a trigger `CapsuleCollider` on the `Rope` layer, updated each `FixedUpdate` between the endpoints. The visual is `RopeView` (LineRenderer with a cosmetic sag curve). Chains are the same component with a chain visual. A cut destroys the joint and raises `PropTriggered(RopeCut)`. Ropes can also be burned (`Burnable`). |
| Alternatives considered | Rigidbody segment chains (unstable, 10× bodies). Verlet visual rope with a joint (nicer sag, more code; post-MVP polish option). |
| Consequences | A rope can't wrap around objects (acceptable). The rope collider is queried only by arrow sweeps and fire overlaps. |
| Owner | PROPS |
| Status | Accepted |

### D-010 — Level = `LevelData` SO + layout prefab; JSON only for saves
| Field | Value |
|---|---|
| Decision ID | D-010 |
| Date | 2026-10-09 |
| Context | The brief requires a data-driven level pipeline with no code per level. M0 already has `LevelData` with a `layoutPrefab`. |
| Decision | Keep it. `LevelData` holds the rules; the prefab holds the geometry, built only from library prefabs. `WorldData` + `LevelCatalog` hold progression order. Runtime-written data (save) is JSON. |
| Alternatives considered | JSON levels + runtime spawner (needs a custom editor, loses prefab-mode WYSIWYG). Scene per level (slow restart, YAML conflicts, lighting overhead). |
| Consequences | Designers and agents author levels in prefab mode via MCP. Validation is an editor tool plus tests. |
| Owner | ARCH |
| Status | Accepted |

### D-011 — Restart and next level re-instantiate the layout; no scene reload
| Field | Value |
|---|---|
| Decision ID | D-011 |
| Date | 2026-10-09 |
| Context | §12: restart under 1 s. |
| Decision | `LevelLoader` returns pooled objects, destroys the layout instance and instantiates a fresh one in the already-loaded Gameplay scene. |
| Alternatives considered | Reload the scene (slower, re-inits UI and pools). Snapshot/restore transforms (fragile with broken joints and destroyed objects). |
| Consequences | Layout prefabs must not depend on scene references. All runtime wiring goes through `LevelLayout` and registries. |
| Owner | CORE |
| Status | Accepted |

### D-012 — uGUI + TextMeshPro for runtime UI
| Field | Value |
|---|---|
| Decision ID | D-012 |
| Date | 2026-10-09 |
| Context | Portrait mobile UI with safe areas, animated star reveals, particles in UI. |
| Decision | uGUI canvases (Screen Space – Camera for the HUD with world-space-aware toasts; Overlay for menus), TMP text, a `SafeAreaFitter` on each canvas root. UI Toolkit is used only in editor windows. |
| Alternatives considered | UI Toolkit runtime (good for data-heavy menus; weaker for particles-in-UI and juicy animation; agents know it less well). |
| Consequences | Atlases via Sprite Atlas v2. One canvas per screen to limit rebuilds. |
| Owner | UI |
| Status | Accepted |

### D-013 — No Addressables in MVP
| Field | Value |
|---|---|
| Decision ID | D-013 |
| Date | 2026-10-09 |
| Context | 60 small levels and 3 environments. Download budget ≤ 150 MB. |
| Decision | Direct references through `LevelCatalog`. Environment art is loaded through `WorldData` references when entering Gameplay. |
| Alternatives considered | Addressables (remote content updates, memory control) — overhead not justified yet. |
| Consequences | Revisit post-MVP for live content worlds 4+. Keep `WorldData` the only entry point to world art so a migration is localised. |
| Owner | ARCH |
| Status | Accepted |

### D-014 — Special-arrow first appearances follow the §6 map beats
| Field | Value |
|---|---|
| Decision ID | D-014 |
| Date | 2026-10-09 |
| Context | The §5 unlock column (W1L15, W2L8=L28, W2L18=L38, W3L10=L50) conflicts with the §6 map beats (Heavyhead 13–15, Split 30–32, fire combos 36–39, Bounce 47–49) and the "two tutorial levels per new mechanic" rule. |
| Decision | First curated appearance: **Heavyhead L13, Split L30, Fire L36, Bounce L47.** "Unlock" = first appearance + an arrow reveal card + an entry in the Bow Forge arrow codex. |
| Alternatives considered | Follow §5 literally (breaks the tutorial cadence for Split/Bounce; Heavyhead appears after its own tutorial block). |
| Consequences | §10 "special-arrow reveal by L15 at the latest" is satisfied (L13). `04` level plan uses these numbers. **Needs OWNER confirmation.** |
| Owner | PO |
| Status | Accepted (owner 2026-10-09, D-082) |

### D-015 — Out-of-arrows fail confirmation timing
| Field | Value |
|---|---|
| Decision ID | D-015 |
| Date | 2026-10-09 |
| Context | §3: fail when the quiver is empty and objectives remain; soft-lock: if nothing moves for 2 s after a shot, show the message; never make players wait through a long settle. |
| Decision | When the last arrow has **resolved** (embedded/spent/out of bounds) and objectives remain, `GameplayController` enters `AwaitingResolution`. Fail is confirmed when the world is calm for `SoftLockSettleSeconds` (2.0 s), or `OutOfArrowsMaxWaitSeconds` (5.0 s) has elapsed since resolution, whichever comes first. It then shows the "Out of arrows" toast (0.6 s) → Fail panel. If objectives clear during the wait, the win path runs instead. **Win:** all required objectives cleared → calm for `WinSettleSeconds` (0.75 s) or `WinSettleMaxSeconds` (3.0 s) → Win. A protected loss before the win is confirmed → Fail. **Soft-lock prompt** (arrows remaining): world calm for 2 s after a shot → toast "N arrows left" (once per shot). "Calm" = no tracked non-kinematic, non-ambient body above 0.05 m/s linear or 5°/s angular. |
| Alternatives considered | Fixed delay after the last shot (feels laggy or premature). Waiting indefinitely for sleep (jittery bodies cause long waits). |
| Consequences | Add `OutOfArrowsMaxWaitSeconds` and `WinSettleMaxSeconds` to `GameConstants`. EditMode tests on the `SettleMonitor` state machine with a fake clock. |
| Owner | CORE |
| Status | Accepted |

### D-016 — Player may fire during resolution after a 0.35 s re-nock cooldown
| Field | Value |
|---|---|
| Decision ID | D-016 |
| Date | 2026-10-09 |
| Context | §2 "fast recovery". Unspecified whether the player must wait for physics. |
| Decision | The next arrow can be drawn 0.35 s (`GameplayTuning.renockCooldown`) after release, regardless of physics state. Max active arrows still apply. |
| Alternatives considered | Lock input until settle (slow, frustrating). No cooldown (accidental double fires). |
| Consequences | Win/fail logic must be continuous. Analytics capture time between shots. |
| Owner | PO |
| Status | Superseded by D-083 |

### D-017 — Preview shows wind, portal pass-through and Bounce ricochet
| Field | Value |
|---|---|
| Decision ID | D-017 |
| Date | 2026-10-09 |
| Context | §3: the preview is generous for L1–15, then can shorten but is never removed. §6: portals teach "visible entry-to-exit trajectory". Fair-physics pillar. |
| Decision | Within its length budget (`LevelData.trajectoryPreviewScale` × `GameplayTuning.previewBaseSeconds`), the preview simulates exactly what the arrow will do: wind acceleration, portal teleport and the single Bounce ricochet. Difficulty comes from a shorter preview, never from hidden forces. Moving obstacles are sampled at their current pose (shields can move, which is the timing skill). |
| Alternatives considered | Show only the raw gravity arc (wind/portal shots become guesswork). |
| Consequences | `BallisticSolver` must be pure and environment-aware via an `IFlightEnvironment` (wind sampler, portal lookup). **Needs OWNER confirmation.** |
| Owner | PO |
| Status | Superseded by D-085 |

### D-018 — Draw starts anywhere in the lower aim zone; relative drag
| Field | Value |
|---|---|
| Decision ID | D-018 |
| Date | 2026-10-09 |
| Context | §3: "touches the bow/string area and drags backward". Strict hit-testing of a small bow hurts one-thumb play on tall phones. |
| Decision | A press anywhere in the **aim zone** (bottom 55% of the safe area, excluding HUD buttons) starts a draw. The aim is the **vector from the press point to the current point, inverted** (pull back to shoot forward). Power = drag length ÷ (`GameplayTuning.fullDrawScreenFraction` × screen height, default 0.22), clamped 0–1. Releasing below `minFirePower` (0.15) cancels with no arrow spent. The angle is clamped to the firing cone (`minAimAngleDeg` 8°, `maxAimAngleDeg` 172° from +X). The bow visually rotates and the string stretches. |
| Alternatives considered | Must touch the bow (precise, but awkward on large phones). Aim by touching the target point (no tactile draw; violates the fantasy). |
| Consequences | Cancel gesture = release near the start point. Tutorial L1 shows a ghost hand dragging down from the bow. **Needs OWNER confirmation after the M1 feel test.** |
| Owner | CORE |
| Status | Proposed |

### D-019 — Split Arrow uses a timed split shown in the preview
| Field | Value |
|---|---|
| Decision ID | D-019 |
| Date | 2026-10-09 |
| Context | §5: "splits into three short-range arrows after first impact or timed trigger". |
| Decision | It splits automatically at flight time `ArrowDefinition.splitTime` (default 0.45 s) into 3 children at −12°/0°/+12° relative to the current velocity. Children are Oak-like with mass ×0.5 and range ≤ 0.8 s. The preview draws the split marker and the three short child arcs. If the parent impacts before `splitTime`, it splits at the impact point: the children fan forward (reflected about the surface tangent, ±12°) offset 0.15 m off the surface, with the struck object receiving the parent impulse. |
| Alternatives considered | Tap-to-split (extra input, breaks one-thumb clarity). Impact-only split (hard to read and aim). |
| Consequences | 9 extra arrow pool entries. The children count as part of the same quiver arrow. **Needs OWNER confirmation.** |
| Owner | PO |
| Status | Superseded by D-088 |

### D-020 — Powder blasts neither damage nor push protected objects
| Field | Value |
|---|---|
| Decision ID | D-020 |
| Date | 2026-10-09 |
| Context | §4: a powder barrel "cannot destroy protected objects nearby". |
| Decision | `Explosion` skips the `Protected` layer for both damage and force. Structure pieces thrown by the blast may still hit protected objects and cause a loss (a player responsibility, and visually readable). Fragments cannot (D-008). |
| Alternatives considered | Damage immunity only (the blast could still shove the fox off its ledge, which reads as unfair). |
| Consequences | Level validation warns when a protected object sits in a blast radius with unconstrained heavy pieces between them. |
| Owner | PO |
| Status | Accepted (owner 2026-10-09, D-092) |

### D-021 — Spring plate is Should-have, first cut candidate
| Field | Value |
|---|---|
| Decision ID | D-021 |
| Date | 2026-10-09 |
| Context | §4 lists 9 props; §14 requires ≥ 8; §6 schedules no spring plate. |
| Decision | Build `SpringPlate` only if M6 is on schedule; feature it in L33–35. Otherwise cut. 8 props remain. |
| Alternatives considered | Force it into W1 (overloads the teaching curve). |
| Consequences | Listed second in the cut list. |
| Owner | PO |
| Status | Superseded by D-086 |

### D-022 — Counterweights = hinge levers + rope-hung weights; no pulleys
| Field | Value |
|---|---|
| Decision ID | D-022 |
| Date | 2026-10-09 |
| Context | §6 L33–35 mentions counterweights. PhysX has no pulley joint. |
| Decision | Counterweight puzzles use a `Struct_Lever_*` prefab (plank + HingeJoint to a static pivot) and rope-hung weights (`RopeCuttable`). |
| Alternatives considered | Custom pulley constraint (time sink, instability). |
| Consequences | No new component. One new prefab family. |
| Owner | PROPS |
| Status | Accepted |

### D-023 — Quiver is consumed in authored order; no in-level arrow swap
| Field | Value |
|---|---|
| Decision ID | D-023 |
| Date | 2026-10-09 |
| Context | §5: a fixed curated quiver, e.g. "2 Oak + 1 Heavyhead"; no loadout choice. Unspecified: can the player choose which arrow to fire next? |
| Decision | `LevelData.quiver` is an ordered list, consumed front to back. The HUD shows the next arrow prominently. Designers order arrows so the intended solution is natural. |
| Alternatives considered | Tap-to-swap the next arrow (more agency, adds an input concept). Re-evaluate at the VS/M5 playtest. |
| Consequences | Simple `QuiverModel`. Arrow-swap can be added later without schema changes (`allowSwap` flag reserved). **Needs OWNER confirmation.** |
| Owner | PO |
| Status | Proposed |

### D-024 — Clears using a rewarded bonus arrow are capped at 1★
| Field | Value |
|---|---|
| Decision ID | D-024 |
| Date | 2026-10-09 |
| Context | §9 offers a rewarded +1 arrow on fail. Unspecified star impact. |
| Decision | A bonus-arrow clear counts as a clear, awards 1★ (or keeps the previous best), and coins as for 1★. Best-star records never decrease. |
| Alternatives considered | Normal star math (ads could buy 2★; undermines mastery). |
| Consequences | `StarRules.Compute(arrowsUsed, goldPar, bonusArrowUsed)`. Unit-tested. **Needs OWNER confirmation.** |
| Owner | PO |
| Status | Superseded by D-084 |

### D-025 — Interstitial policy defaults
| Field | Value |
|---|---|
| Decision ID | D-025 |
| Date | 2026-10-09 |
| Context | §9: at most after every 3 completed levels, never after a fail, never in the first 10 minutes. |
| Decision | `AdPolicy.CanShowInterstitial` is true only when all of these hold: `removeAds == false`; lifetime active playtime ≥ 600 s; completed levels since the last interstitial ≥ 3; seconds since the last interstitial ≥ 120; the trigger is a **Win → Next/Home transition**; the previous result was not a fail; the current level is not L1–L5; no rewarded ad in the last 60 s. All thresholds are remote-config keys with these defaults. |
| Alternatives considered | Session-based 10-minute window (resets every session; too aggressive). |
| Consequences | Pure, unit-tested `AdPolicy`. Analytics logs every suppression reason in dev builds. |
| Owner | MON |
| Status | Superseded by D-093 |

### D-026 — Coin reward values
| Field | Value |
|---|---|
| Decision ID | D-026 |
| Date | 2026-10-09 |
| Context | §9: coins come from completions, the daily quest and optional rewarded video, and buy cosmetics only. |
| Decision | First clear: 20 + 10 × stars. Replay that improves stars: 10 × new stars gained. Replay with no improvement: 5. Bullseye first time: 15. Daily challenge complete: 100. Rewarded 2×: doubles the level reward (not the daily). Cosmetic prices: trails 400–600, bow skins 900–1,500. All in `EconomyConfig`. |
| Alternatives considered | Coins scaled by world (adds complexity, little value). |
| Consequences | 3★ on all 60 levels ≈ 3,000 coins; plus dailies → about 4–6 cosmetics in the first month. Tuned in M9. |
| Owner | SYS |
| Status | Proposed |

### D-027 — Daily challenge minimal scope
| Field | Value |
|---|---|
| Decision ID | D-027 |
| Date | 2026-10-09 |
| Context | §10/§14: "complete three selected old levels with a fixed challenge quiver"; basic. |
| Decision | Unlocks after clearing L10. Each local calendar day: pick 3 **cleared** levels via a hash of `yyyyMMdd` + `installId`. Challenge quiver = Oak × gold par (special arrows kept if the level requires them, count = par). Completing all 3 grants 100 coins once per day. No streaks, no timers, offline-only. |
| Alternatives considered | Server-driven dailies (no backend in v1). |
| Consequences | Clock tampering is tolerated (cosmetic coins only). |
| Owner | SYS |
| Status | Accepted (owner 2026-10-09, D-094) |

### D-028 — One Runtime asmdef + Integrations asmdef for vendor SDKs
| Field | Value |
|---|---|
| Decision ID | D-028 |
| Date | 2026-10-09 |
| Context | Small project. Vendor SDKs must not leak into game code. |
| Decision | Keep `ArrowBuster.Runtime` as one assembly. Add `ArrowBuster.Integrations` (M8) for vendor adapters behind `#if` defines. Add `ArrowBuster.Tests.PlayMode` (AB-002). |
| Alternatives considered | Many fine-grained asmdefs (overhead now). |
| Consequences | `ArchitectureRulesTests` enforce namespace dependency rules. |
| Owner | ARCH |
| Status | Accepted |

### D-029 — Static `Services` locator; no DI framework
| Field | Value |
|---|---|
| Decision ID | D-029 |
| Date | 2026-10-09 |
| Context | Need swappable implementations (mock vs vendor) with low ceremony. |
| Decision | `Services` static class with typed properties set by `ServiceInstaller` (Boot) or by an editor fallback. Null-object defaults. Reset via `StaticReset`. |
| Alternatives considered | Zenject/VContainer (learning curve, overkill). Singleton MonoBehaviours everywhere (hidden coupling). |
| Consequences | Tests swap services via `Services.Install(...)`. |
| Owner | ARCH |
| Status | Accepted |

### D-030 — `GameEvents` static bus for cross-cutting listeners only
| Field | Value |
|---|---|
| Decision ID | D-030 |
| Date | 2026-10-09 |
| Context | CLAUDE.md prefers C# events. Feedback, analytics and UI need to observe gameplay without coupling. |
| Decision | `GameEvents` static events with `readonly struct` payloads (canonical list in `01` §10.2). Gameplay raises; Feedback/UI/Analytics/Tutorial listen. Local wiring stays on component events. |
| Alternatives considered | ScriptableObject event channels (asset sprawl). Direct calls (coupling). |
| Consequences | New events need an ARCH review and an update to `01` §10.2. |
| Owner | ARCH |
| Status | Accepted |

### D-031 — In-house `UiTween`; LitMotion is the approved fallback
| Field | Value |
|---|---|
| Decision ID | D-031 |
| Date | 2026-10-09 |
| Context | UI juice (star pops, button bounce, panel slides) needs tweening. Avoid casual packages. |
| Decision | Write `UiTween` (scale/fade/move/punch, easing set, unscaled time, pooled handles). If at M5 the UI needs sequences or many simultaneous tweens, adopt LitMotion via UPM (MIT) with an ARCH decision entry. |
| Alternatives considered | DOTween (Asset Store distribution, not UPM-friendly). PrimeTween (fine; equivalent alternative). |
| Consequences | ~1 day of work in M3. |
| Owner | ARCH |
| Status | Accepted |

### D-032 — In-house native haptics bridge
| Field | Value |
|---|---|
| Decision ID | D-032 |
| Date | 2026-10-09 |
| Context | §8: light on draw threshold, medium on impact, success pattern on clear; a toggle is needed. `Handheld.Vibrate` is too crude. |
| Decision | `IHapticsService` with `HapticKind {Light, Medium, Heavy, Success, Failure, Selection}`. iOS: `Plugins/iOS/ABHaptics.mm` (UIImpactFeedbackGenerator, UINotificationFeedbackGenerator). Android: `VibrationEffect.createOneShot`/`createWaveform` via `AndroidJavaObject`, with `EFFECT_TICK`/`EFFECT_CLICK` predefined effects on API 29+. Rate-limited to 1 per 50 ms. |
| Alternatives considered | A third-party haptics plugin (licence/maintenance uncertainty). |
| Consequences | The `VIBRATE` permission is added to the Android manifest. Tested on the device matrix. |
| Owner | PLAT |
| Status | Accepted |

### D-033 — Milestone plan restructured around a Vertical Slice gate at M3
| Field | Value |
|---|---|
| Decision ID | D-033 |
| Date | 2026-10-09 |
| Context | §14 week plan puts art/feedback in week 5. The brief requires a polished 5-level vertical slice that proves fun first. |
| Decision | M1 Graybox Core Feel → M2 Core Loop + VS graybox → **M3 Vertical Slice (go/no-go)** → M4 World 1 systems & tooling → M5 World 1 content & art lock → M6 World 2 → M7 World 3 (+ SDK spike) → M8 Meta & monetisation → M9 Optimisation/QA/accessibility → M10 Closed test & submission candidate. CLAUDE.md milestones are updated to match. |
| Alternatives considered | Follow §14 literally (risks building 20 levels before proving fun with feedback). |
| Consequences | One Greenwood environment section is art-polished in M3; the full Greenwood art lands in M5. See `08`. |
| Owner | PO |
| Status | Superseded by D-102 |

### D-034 — Git + LFS + worktree-per-agent workflow
| Field | Value |
|---|---|
| Decision ID | D-034 |
| Date | 2026-10-09 |
| Context | No repo yet. Multiple agents need isolation. Unity cannot open one project folder twice. |
| Decision | `git init` + `git lfs install` + UnityYAMLMerge driver (AB-001, needs OWNER approval). `main` is always green and only INT merges to it. Code-only tasks use git worktrees (`../ab-wt/<branch>`). Editor-dependent tasks run in at most **2** concurrent worktrees (each a separate Unity instance; the MCP instance is pinned per agent). Details: skill `git-worktree-and-integration.md`. |
| Alternatives considered | Single working copy with sequential agents (safe but slow). Unity Version Control (no team need). |
| Consequences | Each worktree has its own `Library/` (first import cost). File locks in `docs/agents/FILE_LOCKS.md`. |
| Owner | INT |
| Status | Accepted (owner 2026-10-09; done in AB-001) |

### D-035 — Vendor SDK selection deferred to the M8 gate
| Field | Value |
|---|---|
| Decision ID | D-035 |
| Date | 2026-10-09 |
| Context | Choosing SDKs early bloats builds and slows iteration. Choosing late risks integration surprises. |
| Decision | Interfaces + Debug/Mock implementations from M2. An integration **spike** on a branch in M7 (Android build with the chosen mediation + Firebase + UMP + Unity IAP compiles and shows test ads). Final selection at the M8 gate using the criteria in `06`. |
| Alternatives considered | Integrate at M1 (slows every build). Integrate at M10 (too risky). |
| Consequences | Analytics events are verified in the Debug sink long before the vendor exists. |
| Owner | MON |
| Status | Superseded by D-090 |

### D-036 — Arrow flight time tuning
| Field | Value |
|---|---|
| Decision ID | D-036 |
| Date | 2026-10-09 |
| Context | §3/§12 target a 1.5–2.5 s flight across a normal board. On a ~10 × 18 m portrait board, that implies low gravity and low speed, which may feel floaty rather than crisp. |
| Decision | M1 builds two `GameplayTuning` + `AD_Oak` presets: **"Spec"** (1.5–2.5 s flights) and **"Snappy"** (0.9–1.4 s). The M1 feel gate picks one with ≥ 5 testers (preference + hit accuracy). The MVP target stays the default until then. |
| Alternatives considered | Hard-code the spec value (risky for feel). |
| Consequences | Level geometry is authored only after the gate (M2), because flight time changes reachable arcs. |
| Owner | PO |
| Status | Proposed |

### D-037 — Objective clear rules per kind
| Field | Value |
|---|---|
| Decision ID | D-037 |
| Date | 2026-10-09 |
| Context | §4 objective table; needs precise, testable rules. |
| Decision | `Objective` clear rules (flags, defaults per kind): **CrestTarget** — broken by an arrow hit, or by an impact impulse ≥ `breakImpulse`, or below the clear line / in a kill zone. **SupplyCrate** — broken (HP), or centre below the clear line, or in a kill zone. **HangingLantern** — broken by an arrow or an impact ≥ threshold (low threshold); falls = breaks on ground contact. **CursedOrb** — *direct arrow hit only*; impacts and blasts are ignored; anchored kinematic by design (if it ever enters a kill zone it counts as cleared and the validator flags the level). **TrainingDummy** — knocked over (up-vector > 70° from vertical for 0.3 s), or touches the `Environment` ground surface tagged `Ground`, or broken. **BannerRope** — cut or burned. |
| Alternatives considered | A generic HP-only rule (makes orb and dummy indistinct). |
| Consequences | `ObjectiveClearRule` flags + an EditMode-tested evaluator. |
| Owner | PO |
| Status | Accepted |

### D-038 — Protected-object fail rules
| Field | Value |
|---|---|
| Decision ID | D-038 |
| Date | 2026-10-09 |
| Context | §4: royal vase must not break; sleeping fox must not be hit or knocked off its ledge. |
| Decision | `ProtectedObject` fails the level on: **RoyalVase/RoyalRelic** — any arrow hit, impact impulse ≥ `fragileImpulse`, or entering a kill zone / below the clear line. **SleepingFox** — any arrow hit, displacement > 0.6 m from the start position, tilt > 45°, or entering a kill zone. Impacts below the threshold are allowed (gentle bumps OK). The fail is immediate: 0.5 s slow-motion focus on the object, then the Fail panel with a reason ("The vase broke!" / "You woke the fox!"). |
| Alternatives considered | Any contact fails (too punishing with stacked props). |
| Consequences | Purple outline + icon on all protected objects. Thresholds live on the prefab and are validated. |
| Owner | PO |
| Status | Accepted |

### D-039 — Kill-zone and clear-line semantics
| Field | Value |
|---|---|
| Decision ID | D-039 |
| Date | 2026-10-09 |
| Context | §4: water removes objects below the line; spikes destroy falling objects and remove arrows; crates may be "pushed below line". |
| Decision | `KillZone` (trigger, kind Water/Spikes/Pit) removes any entering gameplay body after its feedback. Objectives entering = **cleared**. Protected entering = **fail**. Arrows entering = resolved (`KillZone`). `LevelLayout.clearLineY` (optional) marks objectives whose centre drops below it as cleared (CrestTarget, SupplyCrate, TrainingDummy only). `PlayBounds` (± margin outside the camera frame) removes anything leaving the board with the same semantics as Pit. |
| Alternatives considered | Kill zones only (forces water everywhere). |
| Consequences | The validator checks that the clear line is below every objective's start position. |
| Owner | PO |
| Status | Accepted |

### D-040 — Trajectory preview ends at the first collider hit, with an impact marker
| Field | Value |
|---|---|
| Decision ID | D-040 |
| Date | 2026-10-09 |
| Context | Readability vs. difficulty. A full arc through solid objects misleads; showing the hit point helps precision. |
| Decision | Preview dots stop at the first collider on the arrow cast mask (except Rope/Portal, which it passes through as the arrow would) and show a small impact ring. It is length-limited by `trajectoryPreviewScale`. |
| Alternatives considered | Arc ignores colliders (classic, less readable). |
| Consequences | Revisit at the M1 feel gate: if 3★ becomes trivial, reduce `previewBaseSeconds` for later worlds. |
| Owner | CORE |
| Status | Superseded by D-085 |

### D-041 — Low-FOV perspective camera fitted to play-area width; pillarbox on tablets
| Field | Value |
|---|---|
| Decision ID | D-041 |
| Date | 2026-10-09 |
| Context | §3/§11: fixed 3/4 view, portrait 9:16 reference, taller devices adapt. |
| Decision | A perspective camera (vertical FOV ≈ 30°, pitched down ≈ 8°) frames the 10 m-wide play area at z = 0. `CameraFramer` keeps the full width visible for aspect ratios 9:16 to 9:21 (extra height shows sky/ground). Wider than 9:16 (tablets, 3:4) fits the height and shows extra scenery at the sides (pillarbox art). Per-level overrides come from `LevelLayout.framing`. No camera motion in MVP except optional micro-shake (disabled by Reduced Motion). |
| Alternatives considered | Orthographic (flat, loses diorama depth). Cinemachine (unneeded). |
| Consequences | Environment art extends ±3 m beyond the play area on each side. |
| Owner | CORE |
| Status | Accepted |

### D-042 — Asset originality and licensing policy
| Field | Value |
|---|---|
| Decision ID | D-042 |
| Date | 2026-10-09 |
| Context | The brief requires all production assets to be original and nothing copied from competitors. |
| Decision | Identity assets (bow, characters, props, environments, UI, logo, icon, music, store art, copy) are custom-made. Fonts: OFL or a purchased commercial licence. SFX: custom or licensed libraries with commercial rights, always processed/layered. Every third-party item is logged in `docs/art/ASSET_LICENSES.md` (created by ART at the first import). No AI-generated asset ships without OWNER sign-off on the tool's licence terms. Placeholders live in `Art/_Placeholder/` and are banned from release builds by a build check. |
| Alternatives considered | Fully original including SFX (expensive for a solo dev). |
| Consequences | **Needs OWNER confirmation**, especially on AI-generated content. |
| Owner | OWNER |
| Status | Superseded by D-089 |

### D-043 — Stay on Unity 6000.6.5f1; LTS upgrade spike at M9 start
| Field | Value |
|---|---|
| Decision ID | D-043 |
| Date | 2026-10-09 |
| Context | CLAUDE.md fixes 6000.6.5f1. The brief asks for a stable LTS. LTS status of 6000.6 is unverified. |
| Decision | Develop on 6000.6.5f1. At M9 start, ARCH verifies the newest Unity 6 LTS and runs a ≤ 1-day spike; adopt if all gates are green. |
| Alternatives considered | Downgrade now to an older LTS (risk of package incompatibilities and lost features). |
| Consequences | Update CLAUDE.md if the version changes. |
| Owner | ARCH |
| Status | Superseded by D-080 |

### D-044 — Specialist subagents defined in `.claude/agents/`
| Field | Value |
|---|---|
| Decision ID | D-044 |
| Date | 2026-10-09 |
| Context | The agent system must be usable by Claude Code, not only documented. |
| Decision | Each role in `docs/agents/AGENT_SYSTEM.md` has a thin `.claude/agents/<role>.md` definition. It points to the canonical doc for responsibilities, so the docs remain the single source. |
| Alternatives considered | Docs only (agents must be briefed by hand every time). |
| Consequences | Changes to roles update both files. INT owns them. |
| Owner | INT |
| Status | Accepted |

### D-045 — Bullseye medals in MVP as data + simple collection view
| Field | Value |
|---|---|
| Decision ID | D-045 |
| Date | 2026-10-09 |
| Context | §3/§10: optional Bullseye medal for hitting a marked weak point; cosmetic/meta only. |
| Decision | `BullseyeMarker` component on authored weak points (≤ 1 per level, optional). A direct arrow hit within its radius sets a flag, awarded on win. Stored per level. The World map node shows a medal pip. The Bow Forge shows a count. |
| Alternatives considered | Cut entirely (loses a cheap replay hook). |
| Consequences | Level designers mark ~50% of levels. The validator checks it is reachable via an intended shot. |
| Owner | PO |
| Status | Accepted |

### D-046 — Choice levels deferred (schema reserved)
| Field | Value |
|---|---|
| Decision ID | D-046 |
| Date | 2026-10-09 |
| Context | §5 marks L31+ choice levels optional if scope is tight. |
| Decision | Not in the MVP plan. `LevelData.choiceSlots` is reserved (unused). Revisit after M7 if ahead of schedule. |
| Alternatives considered | Build in M6 (UI + validation + solvability for 2 variants per level). |
| Consequences | First item on the cut list (already cut by default). |
| Owner | PO |
| Status | Accepted |

### D-047 — Level numbering
| Field | Value |
|---|---|
| Decision ID | D-047 |
| Date | 2026-10-09 |
| Context | §5 uses world-local numbers; §6 uses global numbers. |
| Decision | Asset IDs are world-local (`W2_L08`). Docs and analytics carry both `level_id` = `"W2_L08"` and `global_level` = 28. In docs, "L28" always means global. |
| Alternatives considered | Global-only IDs (breaks the M0 asset naming). |
| Consequences | `LevelData.GlobalIndex` already exists. |
| Owner | LEVEL |
| Status | Accepted |

### D-048 — Collision layer set and matrix
| Field | Value |
|---|---|
| Decision ID | D-048 |
| Date | 2026-10-09 |
| Context | No custom layers exist. Arrow queries, debris isolation and triggers need a fixed scheme. |
| Decision | Layers: 6 `Environment`, 7 `Structure`, 8 `Objective`, 9 `Protected`, 10 `Prop`, 11 `Rope`, 12 `Portal`, 13 `Arrow`, 14 `Debris`, 15 `KillZone`, 16 `Field`. Matrix and arrow cast mask in `03` §2. Constants in `Physics/PhysicsLayers.cs`. Applied by `LayerSetup` (editor) in AB-002. |
| Alternatives considered | Fewer layers with tags (slower queries, ambiguous). |
| Consequences | `TagManager.asset` and `DynamicsManager.asset` are PHYS-owned shared files. |
| Owner | PHYS |
| Status | Accepted |

### D-049 — Arrows are not affected by explosions or physics after resolution
| Field | Value |
|---|---|
| Decision ID | D-049 |
| Date | 2026-10-09 |
| Context | Embedded arrows riding on falling structures could act as levers or spears if they kept colliders. |
| Decision | An embedded arrow becomes a collider-less visual child of the struck body. Spent arrows collide only with Environment. Flying arrows are kinematic and ignore blasts and wind except via `BallisticSolver` (wind only). |
| Alternatives considered | Physical embedded arrows (emergent but unfair). |
| Consequences | Simpler, cheaper, predictable. |
| Owner | CORE |
| Status | Accepted |

### D-050 — Vertical-slice levels are real World 1 levels, not throwaway
| Field | Value |
|---|---|
| Decision ID | D-050 |
| Date | 2026-10-09 |
| Context | Avoid wasted content work. |
| Decision | VS-01 = W1_L01, VS-02 = W1_L04, VS-03 = W1_L07, VS-04 = W1_L10, VS-05 = W1_L16. They are played in order via the `LC_VerticalSlice` playlist during M3. |
| Alternatives considered | Bespoke VS levels (wasted work). |
| Consequences | These five levels are held to final quality first. |
| Owner | LEVEL |
| Status | Accepted |

### D-051 — Colour-language reconciliation for materials
| Field | Value |
|---|---|
| Decision ID | D-051 |
| Date | 2026-10-09 |
| Context | §4 says gray/blue = structure, yet timber is warm brown and straw pale yellow (05 §1.3). |
| Decision | Structural materials keep their §4 identity colours at ≥ 20% lower saturation than the reserved accent hues (red required, green/gold interactive, purple protected/hazard). Neutral supports (platforms, stone, metal frames) use gray/blue. |
| Alternatives considered | Make every structure gray/blue (loses material readability, which §4 depends on). |
| Consequences | Palette sheets per world in 05. The 5-second screenshot test (00 §4) validates it at the VS gate. |
| Owner | PO |
| Status | Proposed |

### D-052 — Naming additions: world codes and environment/marker prefabs
| Field | Value |
|---|---|
| Decision ID | D-052 |
| Date | 2026-10-09 |
| Context | 01 §8 lacked patterns for environment pieces, level markers and level templates (raised by 04 and 05). |
| Decision | World codes `GW` (Greenwood Range), `SC` (Sunscar Canyon), `FK` (Frostspire Keep). Patterns: `Env_<WorldCode>_<Name>` (environment kit pieces), `Marker_<Name>` (TutorialAnchor, ClearLine, BowAnchor), `Lvl_Pattern_<Name>` (reusable sub-layouts), `Lvl_Template_Base` (empty level template). Added to 01 §8. |
| Alternatives considered | Free naming (asset sprawl; breaks validator rules that match prefixes). |
| Consequences | The validator and AssetImportRules match on these prefixes. |
| Owner | ARCH |
| Status | Accepted |

### D-053 — No ranger avatar in MVP; the fox is the brand mascot
| Field | Value |
|---|---|
| Decision ID | D-053 |
| Date | 2026-10-09 |
| Context | Character scope is unspecified; avatars cost rigging/animation and clutter the bottom of the screen (§8). |
| Decision | The player is the archer (first-person bow). An optional gloved draw hand is P2. The Sleeping Fox doubles as the brand mascot (icon, splash, store art). |
| Alternatives considered | A visible ranger character (animation and outfit scope; competes with the bow as hero prop). |
| Consequences | Saves ~1–2 weeks of art. Marketing art plans around the fox and the bow. |
| Owner | PO |
| Status | Proposed |

### D-054 — Colour-assist outline implementation
| Field | Value |
|---|---|
| Decision ID | D-054 |
| Date | 2026-10-09 |
| Context | §8 requires colour-assist outlines and "colour is never the only signal". |
| Decision | An inverted-hull outline pass in the shared stylised shader (`SG_AB_StylizedLit`) plus world-space category glyphs. Portal pairs are distinguished by colour **and** glyph. Off by default; offered once in Settings. |
| Alternatives considered | Post-process edge detection (too expensive on Low). |
| Consequences | Works on the Low tier. Checked with colour-blindness simulators (07 accessibility checks). |
| Owner | UI |
| Status | Accepted |

### D-055 — Audio import presets and loudness targets
| Field | Value |
|---|---|
| Decision ID | D-055 |
| Date | 2026-10-09 |
| Context | Consistent mix and memory across hundreds of clips. |
| Decision | Import presets and loudness targets as in 05 §7.3 (enforced by `AssetImportRules`). 2D mix only, with screen-x panning. |
| Alternatives considered | Per-clip manual settings (drift, memory spikes). |
| Consequences | AssetPostprocessor rules ship in M3 with the first SFX batch. |
| Owner | ART |
| Status | Accepted |

### D-056 — Placeholder build check
| Field | Value |
|---|---|
| Decision ID | D-056 |
| Date | 2026-10-09 |
| Context | D-042 bans placeholders from release builds; it needs enforcement. |
| Decision | Placeholder assets live in `Art/_Placeholder/` and carry the `Placeholder` asset label. `BuildScript` fails ClosedTest/Release builds when any build dependency has that label or path. |
| Alternatives considered | Manual review (error-prone). |
| Consequences | Dev builds may still use placeholders. |
| Owner | ART |
| Status | Accepted |

### D-057 — Hit-stop under Reduced Motion
| Field | Value |
|---|---|
| Decision ID | D-057 |
| Date | 2026-10-09 |
| Context | Accessibility setting vs the core impact feel (§11 40–70 ms hit-stop). |
| Decision | Reduced Motion disables camera shake, slow-mo zoom and UI bounce overshoot, but **keeps hit-stop, capped at 40 ms** (a pause is not motion). |
| Alternatives considered | Disable hit-stop entirely (removes core feedback for no clear accessibility benefit). |
| Consequences | 02 §13, 05 and 07 accessibility checks all use this rule. |
| Owner | PO |
| Status | Proposed |

### D-058 — First launch drops straight into W1_L01
| Field | Value |
|---|---|
| Decision ID | D-058 |
| Date | 2026-10-09 |
| Context | §10: first shot within 15 s of install. Boot + consent + Home + map would exceed this. |
| Decision | On first launch: Boot → consent (only where legally required) → Gameplay W1_L01 with the ghost hand. Home appears after the first win. Returning players land on Home. |
| Alternatives considered | Show Home first (adds 2 taps and ~5 s). |
| Consequences | The `first_shot` analytics event measures it. The consent UI must be fast. |
| Owner | PO |
| Status | Proposed |

### D-059 — `MaterialKind.Earth` for environment ground
| Field | Value |
|---|---|
| Decision ID | D-059 |
| Date | 2026-10-09 |
| Context | Arrows hitting ground need a material response (embed, thud SFX), but ground is not one of the 5 structural materials. |
| Decision | Append `Earth = 5` to `MaterialKind` (append-only, D-070) for `Environment` ground/turf only. Arrows embed in it. It is never used on Structure/Objective/Prop bodies (validator rule). |
| Alternatives considered | Treat ground as Timber (wrong SFX/VFX). |
| Consequences | `MP_Earth` profile. §14 "five materials" stays true for structures. |
| Owner | PHYS |
| Status | Proposed |

### D-060 — No gameplay fracture in MVP
| Field | Value |
|---|---|
| Decision ID | D-060 |
| Date | 2026-10-09 |
| Context | Splitting broken objects into new gameplay bodies multiplies body counts and non-determinism. |
| Decision | A broken object disappears and spawns only cosmetic debris (D-008). Snap-in-half planks are post-MVP. |
| Alternatives considered | Two-piece gameplay fracture for beams (more emergent, harder to make solvable). |
| Consequences | Levels are designed around whole-piece collapses. |
| Owner | PHYS |
| Status | Accepted |

### D-061 — `TimeScaleController` is the only writer of `Time.timeScale`
| Field | Value |
|---|---|
| Decision ID | D-061 |
| Date | 2026-10-09 |
| Context | Pause, protected-loss slow-mo and hit-stop all change time scale and could fight each other. |
| Decision | `Core/TimeScaleController` owns `Time.timeScale` with priority Pause > slow-mo > hit-stop. `Feedback/HitStop` decides when to request a hit-stop. GameplayController requests pause and slow-mo. `ArchitectureRulesTests` greps for other writers. |
| Alternatives considered | Let `HitStop` own time scale (Feedback would then own pause, a layering violation). |
| Consequences | Added to 01 §7 Core. |
| Owner | ARCH |
| Status | Accepted |

### D-062 — Rewarded bonus arrow: type and resume behaviour
| Field | Value |
|---|---|
| Decision ID | D-062 |
| Date | 2026-10-09 |
| Context | §9 +1 arrow on fail; some levels can't be finished with an Oak arrow. |
| Decision | The bonus arrow type is `LevelData.bonusArrowType` (default Oak). Play resumes from the current physical state (no reset), with a "+1" HUD badge. One offer per attempt. If the bonus arrow also fails, the Fail panel offers Retry only. Stars capped at 1★ (D-024). |
| Alternatives considered | Restart the level with +1 arrow (less useful, still an ad view). |
| Consequences | 02 §9, 04 §2 field 21b, 06 §7.1. |
| Owner | PO |
| Status | Proposed |

### D-063 — Rewarded +1 arrow viable-state heuristic
| Field | Value |
|---|---|
| Decision ID | D-063 |
| Date | 2026-10-09 |
| Context | §9: offer only if one objective remains or a viable state is detected. |
| Decision | `AdPolicy.CanOfferBonusArrow` per 06 §7.1. All of these must hold: an OutOfArrows fail (never ProtectedLost); no prior offer this attempt; not a daily run; global level ≥ `ads.rewarded_arrow.min_global_level` (4); remaining required ≤ 1, or ≤ 2 when `rewardedArrowViableWithTwo`; no remaining objective flagged `requiresSpentProp`; `allowRewardedArrow` true; kill switch on; ad loaded; consent resolved. |
| Alternatives considered | A physics-based reachability solver (expensive, unreliable). |
| Consequences | Unit tests `AdPolicyTests` (06). |
| Owner | PO |
| Status | Proposed |

### D-064 — World-completion bow skins are also coin-buyable
| Field | Value |
|---|---|
| Decision ID | D-064 |
| Date | 2026-10-09 |
| Context | World-completion rewards should not lock cosmetics behind skill for players who want them early. |
| Decision | Moonwood/Ember/Frostglass (world-completion rewards) can also be bought with coins before completion. If already owned on completion, the reward converts to 300 coins. |
| Alternatives considered | Completion-exclusive skins (stronger prestige, but frustrating for some). |
| Consequences | Catalogue in 06. |
| Owner | SYS |
| Status | Proposed |

### D-065 — Starter Pack restore semantics
| Field | Value |
|---|---|
| Decision ID | D-065 |
| Date | 2026-10-09 |
| Context | Non-consumable restore on reinstall or a new device must not duplicate coins. |
| Decision | Restore re-grants the cosmetics. The pack's coins are granted once per install and tracked in the save. |
| Alternatives considered | Never restore coins (unfair after a device change). Always restore coins (farmable). |
| Consequences | `IapCatalog` + `InventoryService` tests. |
| Owner | MON |
| Status | Accepted |

### D-066 — Single per-level remote-config knob
| Field | Value |
|---|---|
| Decision ID | D-066 |
| Date | 2026-10-09 |
| Context | Need an emergency balance lever without opening per-level remote tuning. |
| Decision | Only `levels.extra_oak.<levelId>` (int 0–1) exists. It appends Oak arrows to the quiver; par is unchanged. Every use is logged in this decision log. |
| Alternatives considered | Remote quiver/par overrides (breaks authored solutions and star fairness). |
| Consequences | 06 remote-config table; 04 §6.1. |
| Owner | PO |
| Status | Accepted |

### D-067 — Offline first launch in a consent region
| Field | Value |
|---|---|
| Decision ID | D-067 |
| Date | 2026-10-09 |
| Context | UMP needs the network to determine region and show the form. |
| Decision | If consent cannot be resolved, treat it as **denied**: no personalised ads, analytics limited per 06. Retry silently on the next online launch. The game is fully playable offline. |
| Alternatives considered | Block play until online (violates offline-first). |
| Consequences | 07 offline scenarios table. |
| Owner | MON |
| Status | Accepted |

### D-068 — Stale file locks
| Field | Value |
|---|---|
| Decision ID | D-068 |
| Date | 2026-10-09 |
| Context | Locks in FILE_LOCKS.md can block others if an agent stalls. |
| Decision | INT may force-release a lock after 2 working days without activity, after notifying the holder and recording it in the lock history. |
| Alternatives considered | No expiry (deadlocks). |
| Consequences | AGENT_SYSTEM.md lock protocol. |
| Owner | INT |
| Status | Accepted |

### D-069 — Squash-merge per ticket; short-lived branches
| Field | Value |
|---|---|
| Decision ID | D-069 |
| Date | 2026-10-09 |
| Context | Keep `main` history readable and reduce YAML merge pain. |
| Decision | One squash commit per ticket on `main`. Branches live ≤ 3 working days. Rebase on `main` before handoff. |
| Alternatives considered | Merge commits (noisy history). Long-lived feature branches (prefab conflicts). |
| Consequences | skill git-worktree-and-integration.md. |
| Owner | INT |
| Status | Accepted |

### D-070 — `GameEnums` values are append-only
| Field | Value |
|---|---|
| Decision ID | D-070 |
| Date | 2026-10-09 |
| Context | Enums are serialized as integers in assets and saves. |
| Decision | Never renumber or remove enum members. Deprecated members are marked `[Obsolete]`. |
| Alternatives considered | Free editing (silently corrupts assets and saves). |
| Consequences | Code-review rule plus a check in `ArchitectureRulesTests`. |
| Owner | ARCH |
| Status | Accepted |

### D-071 — XL tickets are split before Ready
| Field | Value |
|---|---|
| Decision ID | D-071 |
| Date | 2026-10-09 |
| Context | Several M5–M7 tickets are XL (AB-070, AB-096, AB-110). |
| Decision | XL tickets must be split into `AB-###a/b/c` sub-tickets (each ≤ L) before they meet the Definition of Ready. |
| Alternatives considered | Allow XL tickets (unreviewable handoffs). |
| Consequences | 09 Definition of Ready. |
| Owner | INT |
| Status | Accepted |

### D-072 — Store accounts and IAP products are created early
| Field | Value |
|---|---|
| Decision ID | D-072 |
| Date | 2026-10-09 |
| Context | Account verification, tax/banking and product review have external lead times. |
| Decision | OWNER creates the Apple Developer and Google Play Console accounts and the IAP products by M5 (10-week plan) / week 10 (realistic plan). |
| Alternatives considered | Create them at M10 (high risk of submission slip). |
| Consequences | Unblocks the M7 SDK spike (sandbox IAP) and the closed test. |
| Owner | OWNER |
| Status | Superseded by D-100 |

### D-073 — Trajectory preview floor 0.4
| Field | Value |
|---|---|
| Decision ID | D-073 |
| Date | 2026-10-09 |
| Context | §3: the preview may shorten after L15 but is never removed. |
| Decision | `LevelData.trajectoryPreviewScale` range is [0.4, 1.0] and must be 1.0 for L1–15. Remote floor `levels.preview_scale_min` defaults to 0.4. |
| Alternatives considered | 0.3 floor (M0 code default; too short to read wind/portal paths). |
| Consequences | AB-016 updates the M0 `Range(0.3f,1f)` attribute. |
| Owner | PO |
| Status | Proposed |

### D-074 — Canonical solvability tolerance
| Field | Value |
|---|---|
| Decision ID | D-074 |
| Date | 2026-10-09 |
| Context | Five slightly different jitter definitions existed across the planning docs. |
| Decision | 07 §5.1 is canonical. δθ = atan(0.4 m / d), floored at 0.5° (±4% of play-area width at first-impact distance d). δp = ±0.03. 9 samples per shot (nominal, ±δθ, ±δp, 4 corners). All samples must win, and the nominal + ≥ 7/9 samples must be within gold par. Timing levels add ±0.1 s release jitter. |
| Alternatives considered | Fixed ±0.75°/±2% (doesn't scale with distance). ±1.5°/±3% with 5 samples (coarser). |
| Consequences | 03, 04, 08, 09 and the skills reference 07 §5.1. |
| Owner | QA |
| Status | Accepted |

### D-075 — Level classification enums live in `Levels/LevelEnums.cs`
| Field | Value |
|---|---|
| Decision ID | D-075 |
| Date | 2026-10-09 |
| Context | `MechanicTag`, `SolutionArchetype` and `LevelStatus` are level-pipeline types and change often. |
| Decision | Keep them out of `Core/GameEnums.cs` (reduces hot-file contention). The append-only rule (D-070) applies too. |
| Alternatives considered | Put them in GameEnums. |
| Consequences | 04 §2.2. |
| Owner | LEVEL |
| Status | Accepted |

### D-076 — Vertical-slice tutorial scope
| Field | Value |
|---|---|
| Decision ID | D-076 |
| Date | 2026-10-09 |
| Context | §8: callouts only in designated teaching levels; no tutorial wall. |
| Decision | The VS has exactly one text callout (VS-03 "Aim for the rope") plus the L1 ghost-hand drag. Level-start banners ("Protect the vase") are not tutorials. |
| Alternatives considered | More prompts (risks a tutorial wall). |
| Consequences | 11 §2. |
| Owner | PO |
| Status | Accepted |

### D-077 — Shielded target is a pattern, not a component
| Field | Value |
|---|---|
| Decision ID | D-077 |
| Date | 2026-10-09 |
| Context | §4 lists "shielded target: only vulnerable after its cover is displaced". |
| Decision | Implemented as a `CrestTarget` physically covered by a movable `Struct_*` piece. No new code. First used at L14. |
| Alternatives considered | A `Shielded` component with vulnerability state (redundant with physics). |
| Consequences | 04 §6.2, 03. |
| Owner | PO |
| Status | Accepted |

### D-078 — Gate naming
| Field | Value |
|---|---|
| Decision ID | D-078 |
| Date | 2026-10-09 |
| Context | Several docs used G1–G8 for different gates. |
| Decision | Milestone gates: **G-M1** (M1), **G0** Vertical Slice (M3), **G1** World 1 (M5), **G2** Content Complete (M7), **G3** Feature Complete (M8), **G4** Release Candidate (M9), **G-Release** (M10) — checklists in 07. Physics spike gates: **PG-1…PG-6** (03 §1.1). VS playtest thresholds: **VS-G1…VS-G8** (11 §8.1). |
| Alternatives considered | — |
| Consequences | 08 lists the mapping. |
| Owner | INT |
| Status | Superseded by D-102 |

### D-079 — Google Play closed-testing requirement is a schedule risk
| Field | Value |
|---|---|
| Decision ID | D-079 |
| Date | 2026-10-09 |
| Context | Newer personal Google Play developer accounts must run a closed test with a minimum number of opted-in testers for a minimum continuous period before production access (recently 12 testers for 14 days). The exact current numbers must be verified. |
| Decision | Verify the current policy and account type by M5. If it applies, start the Play closed track by M8 at the latest, so the requirement is met before the M10 submission. |
| Alternatives considered | Ignore until M10 (likely 2+ week slip). |
| Consequences | 08 M10 duration. 07 closed-test plan. |
| Owner | OWNER |
| Status | Superseded by D-099 |

### D-080 — Unity 6000.6.5f1 locked for the whole MVP
| Field | Value |
|---|---|
| Decision ID | D-080 |
| Date | 2026-10-09 |
| Context | Q-01. Owner approval 2026-10-09 (plan review). A late engine upgrade is unnecessary risk in a physics-heavy project. |
| Decision | Develop, test and ship the MVP on Unity 6000.6.5f1. No upgrade spike. Upgrade only after launch, or if a release-blocking iOS/Android issue requires it (decision entry required). |
| Alternatives considered | LTS upgrade spike at M9 (D-043). |
| Consequences | D-043 superseded. The M9 spike ticket is removed. Unity version changes need OWNER approval. |
| Owner | OWNER |
| Status | Accepted |

### D-081 — Public name vs technical identifiers
| Field | Value |
|---|---|
| Decision ID | D-081 |
| Date | 2026-10-09 |
| Context | Q-02. Owner approval 2026-10-09 (plan review). |
| Decision | Public/UI/store name: **Arrow Buster**. Technical identifiers use **`ArrowBuster`**: namespaces, asmdefs, class prefixes, save file/keys, analytics app naming, repo name. Where a platform convention needs lowercase, use the lowercase token (bundle ID `com.attila.arrowbuster`, snake_case analytics events). Legal/store/domain clearance is done before store-submission assets are produced (M8). |
| Alternatives considered | Mixed spellings (as in the original mvp.md). |
| Consequences | mvp.md title updated. The local folder `Arrow Buster` keeps its name (renaming would break Unity Hub paths); the GitHub repo is `ArrowBuster`. |
| Owner | OWNER |
| Status | Accepted |

### D-082 — Special-arrow first appearances locked
| Field | Value |
|---|---|
| Decision ID | D-082 |
| Date | 2026-10-09 |
| Context | Q-03. Owner approval 2026-10-09 (plan review). Resolves the §5/§6 conflict. |
| Decision | Heavyhead: Global L13 / W1_L13. Split: Global L30 / W2_L10. Fire: Global L36 / W2_L16. Bounce: Global L47 / W3_L07. mvp.md §5 updated. |
| Alternatives considered | The original §5 unlock column. |
| Consequences | D-014 accepted. Validator rule V-11 enforces it. |
| Owner | OWNER |
| Status | Accepted |

### D-083 — Re-nock during active physics; no firing after the outcome is decided
| Field | Value |
|---|---|
| Decision ID | D-083 |
| Date | 2026-10-09 |
| Context | Q-04. Owner approval 2026-10-09 (plan review). |
| Decision | The next arrow is available 0.35 s after release while physics still resolves (rapid chain reactions are allowed). Firing is disabled as soon as all required objectives are cleared (win pending), and in Won and Failed states. |
| Alternatives considered | Wait for settle (slow). No cooldown (accidental double fires). |
| Consequences | D-016 superseded. `GameplayStateMachineTests` covers no-fire after objectives cleared. |
| Owner | OWNER |
| Status | Accepted |

### D-084 — Bonus-arrow clears capped at 1★, disclosed before the ad
| Field | Value |
|---|---|
| Decision ID | D-084 |
| Date | 2026-10-09 |
| Context | Q-05. Owner approval 2026-10-09 (plan review). |
| Decision | A clear using the rewarded bonus arrow counts as completed (normal completion progression) but never earns 2★ or 3★. The offer shows **"Bonus Arrow Used — 1★ Max"** before the player accepts the ad. |
| Alternatives considered | Normal star math. |
| Consequences | D-024 superseded. UI string key `fail.bonus_arrow.star_cap`. |
| Owner | OWNER |
| Status | Accepted |

### D-085 — Preview shows wind, portal exits and the first bounce
| Field | Value |
|---|---|
| Decision ID | D-085 |
| Date | 2026-10-09 |
| Context | Q-06. Owner approval 2026-10-09 (plan review). Hidden path changes are unfair. |
| Decision | Within its length budget the preview shows wind influence, the path after a portal exit, and the **first bounce**: if `ArrowImpactResolver` predicts a Ricochet at the first hit (always for Bounce on metal; shallow-angle metal ricochets for other arrows), one reflected segment is drawn until the next hit; otherwise the preview stops at the first hit with an impact marker. Difficulty comes from shorter previews, placement, timing and limited arrows — never invisible forces. |
| Alternatives considered | Raw gravity arc only. |
| Consequences | D-017 and D-040 superseded. Parity tests cover wind, portal and first-bounce cases. |
| Owner | OWNER |
| Status | Accepted |

### D-086 — Spring Plates cut from the launch MVP
| Field | Value |
|---|---|
| Decision ID | D-086 |
| Date | 2026-10-09 |
| Context | Q-07. Owner approval 2026-10-09 (plan review). |
| Decision | No spring plate is scheduled or implemented. Revisit only if all core systems, 60 levels, performance work and store integration are ahead of schedule. Launch props: rope/chain, balloon, oil jar, powder barrel, rolling boulder, wind fan, rotating/moving shield, portal (8; meets §14). |
| Alternatives considered | Should-have in M6 (D-021). |
| Consequences | D-021 superseded. Ticket moved to Cut / post-MVP. |
| Owner | OWNER |
| Status | Accepted |

### D-087 — No pulley simulation
| Field | Value |
|---|---|
| Decision ID | D-087 |
| Date | 2026-10-09 |
| Context | Q-08. Owner approval 2026-10-09 (plan review). |
| Decision | Counterweight puzzles use rope-hung weights, hinge seesaws, dropping loads and rolling boulders. No true pulleys or multi-rope tension simulation. |
| Alternatives considered | Custom pulley constraint. |
| Consequences | Confirms D-022. |
| Owner | OWNER |
| Status | Accepted |

### D-088 — Timed Split Arrow with visible split feedback
| Field | Value |
|---|---|
| Decision ID | D-088 |
| Date | 2026-10-09 |
| Context | Q-09. Owner approval 2026-10-09 (plan review). |
| Decision | The Split Arrow splits at a fixed, configurable flight time (`ArrowDefinition.splitTime`) shown by a marker in the preview. If it collides first, it splits on impact into a forward fan. A glowing ring/trail pulse (`VFX_Arrow_SplitPulse`) plays ~0.12 s before the split. |
| Alternatives considered | Tap-to-split. Impact-only split. |
| Consequences | D-019 superseded. |
| Owner | OWNER |
| Status | Accepted |

### D-089 — Asset licensing and AI-generated content policy
| Field | Value |
|---|---|
| Decision ID | D-089 |
| Date | 2026-10-09 |
| Context | Q-10 + Q-21. Owner approval 2026-10-09 (plan review). |
| Decision | Licensed fonts and licensed SFX libraries are allowed (SFX processed), each recorded in `docs/art/ASSET_LICENSES.md` with source and licence. Original/custom: bow, arrows, props, UI, logo, environments, characters, level compositions, icons, VFX identity and **music**. AI-generated assets are for internal concepts and temporary placeholders only. They never ship (hero art, logos, characters, environments, UI, music, competitor-adjacent material) unless the tool licence, commercial rights and an originality review are documented. |
| Alternatives considered | Fully original including SFX (cost). Unrestricted AI use (legal/originality risk). |
| Consequences | D-042 superseded. Placeholder build check (D-056) also blocks AI placeholders. |
| Owner | OWNER |
| Status | Accepted |

### D-090 — Vendors: Firebase + Unity LevelPlay + Unity IAP behind adapters
| Field | Value |
|---|---|
| Decision ID | D-090 |
| Date | 2026-10-09 |
| Context | Q-11. Owner approval 2026-10-09 (plan review). |
| Decision | Firebase for Analytics, Crashlytics and Remote Config. Unity LevelPlay for ad mediation. Unity IAP for purchases. All behind our interfaces in M2 — `IAnalyticsService`, `ICrashReportingService`, `IRemoteConfigService`, `IAdsService`, `IIapService`, `IConsentService` — with mock implementations until the SDK milestone (M7). Vendor code lives only in `ArrowBuster.Integrations`. Consent via Google UMP + iOS ATT (exact UMP–LevelPlay integration path verified at integration). |
| Alternatives considered | AppLovin MAX, AdMob mediation, GameAnalytics, Unity Gaming Services analytics/RC. |
| Consequences | D-035 superseded. `ICrashReporter` renamed `ICrashReportingService`. |
| Owner | OWNER |
| Status | Accepted |

### D-091 — Launch IAP pricing
| Field | Value |
|---|---|
| Decision ID | D-091 |
| Date | 2026-10-09 |
| Context | Q-12. Owner approval 2026-10-09 (plan review). |
| Decision | Remove Ads: £3.99 / USD 3.99 (removes interstitials only; rewarded ads stay optional). Cosmetic Starter Pack: £2.99 / USD 2.99 = Royal Amethyst bow skin + Gold Spark trail + 500 coins. Never stat boosts, arrows or puzzle advantages. Prices are set in the store consoles; other markets use the nearest price tier. |
| Alternatives considered | Earlier tier proposals (Q-12). |
| Consequences | 06 IAP catalogue updated. |
| Owner | OWNER |
| Status | Accepted |

### D-092 — Powder blasts never touch protected objects directly
| Field | Value |
|---|---|
| Decision ID | D-092 |
| Date | 2026-10-09 |
| Context | Q-13. Owner approval 2026-10-09 (plan review). |
| Decision | Explosions apply no damage and no force to protected objects. Structure pieces moved by the blast can still break them (player responsibility). |
| Alternatives considered | Damage immunity only. |
| Consequences | Confirms D-020. |
| Owner | OWNER |
| Status | Accepted |

### D-093 — Interstitial policy (final)
| Field | Value |
|---|---|
| Decision ID | D-093 |
| Date | 2026-10-09 |
| Context | Q-14. Owner approval 2026-10-09 (plan review). |
| Decision | An interstitial may show only on a Win → Next/Home transition when: cumulative active gameplay ≥ 10 min per install; ≥ 3 completed levels since the last interstitial; ≥ 120 s since the last interstitial; Remove Ads not owned. Never after a fail, during a level, during onboarding (global L1–L5 / first-session FTUE), after a purchase (same session), on the transition right after any rewarded ad, or on app resume from background. All thresholds are Remote Config values. |
| Alternatives considered | D-025 defaults. |
| Consequences | D-025 superseded. `AdPolicyTests` cover every exclusion. |
| Owner | OWNER |
| Status | Accepted |

### D-094 — Daily Challenge scope (cut-first)
| Field | Value |
|---|---|
| Decision ID | D-094 |
| Date | 2026-10-09 |
| Context | Q-15. Owner approval 2026-10-09 (plan review). |
| Decision | Unlocks after clearing Global L10. Each local day picks 3 already-completed levels by local date seed; the quiver is the gold-par arrow count. No leaderboard, streak, backend or push notification. Cut-first if it risks the 60-level campaign. |
| Alternatives considered | Server-driven dailies. |
| Consequences | Confirms D-027. |
| Owner | OWNER |
| Status | Accepted |

### D-095 — Rename Royal Violet → Royal Amethyst
| Field | Value |
|---|---|
| Decision ID | D-095 |
| Date | 2026-10-09 |
| Context | Q-16. Owner approval 2026-10-09 (plan review). Avoids association with the purple protected-object language. |
| Decision | The bow skin is **Royal Amethyst** (`CD_Bow_RoyalAmethyst`). |
| Alternatives considered | Keep "Royal Violet". |
| Consequences | mvp.md §5 updated. |
| Owner | OWNER |
| Status | Accepted |

### D-096 — Consent-gated analytics, ad personalisation and crash reporting (UK/EEA)
| Field | Value |
|---|---|
| Decision ID | D-096 |
| Date | 2026-10-09 |
| Context | Q-17. Owner approval 2026-10-09 (plan review). Don't rely on a legal assumption that crash reporting is exempt. |
| Decision | Where consent is required (UK/EEA, as reported by UMP): before consent, ads are non-personalised, Firebase Analytics collection is disabled and Crashlytics collection is disabled; only the minimum technically essential diagnostics permitted by the chosen privacy implementation run. SDKs are initialised consent-aware (consent → ATT → enable collection per choices). Final legal/privacy review before launch. |
| Alternatives considered | Crash reporting under legitimate interest. |
| Consequences | D-067 (offline = denied) still applies. 06 and 07 updated. |
| Owner | OWNER |
| Status | Accepted |

### D-097 — Privacy policy hosted before SDK integration
| Field | Value |
|---|---|
| Decision ID | D-097 |
| Date | 2026-10-09 |
| Context | Q-18. Owner approval 2026-10-09 (plan review). |
| Decision | The OWNER hosts the privacy policy before SDK integration begins — at the start of M7 at the latest. It lists analytics, crash reporting, advertising, IAP processing, consent choices and a support contact. The URL is shown in Settings and both store listings. |
| Alternatives considered | Policy at M8/M10. |
| Consequences | Privacy-policy ticket moved to the M7 start. |
| Owner | OWNER |
| Status | Accepted |

### D-098 — Test and launch markets
| Field | Value |
|---|---|
| Decision ID | D-098 |
| Date | 2026-10-09 |
| Context | Q-19. Owner approval 2026-10-09 (plan review). |
| Decision | Run a UK closed test first (TestFlight + Google Play closed track; the owner can observe it directly). Then soft launch in **Canada and Australia** (English only). Verify local privacy requirements (Canada PIPEDA / Quebec Law 25, Australia Privacy Act) before the soft launch. |
| Alternatives considered | Mixed-language soft launch. |
| Consequences | 07 closed-test and soft-launch plans updated. |
| Owner | OWNER |
| Status | Accepted |

### D-099 — Verify Google Play testing requirements in Week 1
| Field | Value |
|---|---|
| Decision ID | D-099 |
| Date | 2026-10-09 |
| Context | Q-20. Owner approval 2026-10-09 (plan review). |
| Decision | In project week 1 the OWNER verifies the Google Play account type and testing/production-access rules. If the personal-account closed-test requirement applies, start recruiting eligible testers early so the requirement is met long before submission. |
| Alternatives considered | Verify by M5 (D-079). |
| Consequences | D-079 superseded. M1 owner ticket. |
| Owner | OWNER |
| Status | Accepted |

### D-100 — Mac + Apple Developer account by week 3; store accounts and IAP products by week 5
| Field | Value |
|---|---|
| Decision ID | D-100 |
| Date | 2026-10-09 |
| Context | Q-22. Owner approval 2026-10-09 (plan review). iOS issues (perf, safe areas, haptics, touch feel, ATT, build) must surface early. |
| Decision | Mac access and an Apple Developer account are ready by project week 3 (regular iOS device builds from the vertical slice onward). Apple and Google developer accounts and store IAP products are created by project week 5. |
| Alternatives considered | Mac by M8; accounts by M5 (D-072). |
| Consequences | D-072 superseded. iOS device build is part of the G0 gate. |
| Owner | OWNER |
| Status | Accepted |

### D-101 — Official cut-first scope and launch cosmetic set
| Field | Value |
|---|---|
| Decision ID | D-101 |
| Date | 2026-10-09 |
| Context | Owner scope cuts. Owner approval 2026-10-09 (plan review). |
| Decision | Cut-first (build only if the VS is excellent and the project is genuinely ahead of schedule): Spring Plates (cut, D-086); choice-loadout levels (cut); moving ice platforms (L53–55 use static ice slides + moving shields); animated splash (static splash only); more than 3 bow skins / 2 trails; Daily Challenge if it risks the campaign; cloud save, leaderboards, achievements, push notifications, seasonal and multiplayer systems. **Launch cosmetics:** bow skins Oak Ranger (default), Moonwood (World 1 completion or 1,200 coins, D-064), Royal Amethyst (Starter Pack; also 2,500 coins — *coin price proposed by the lead, confirm*); trails Gold Spark (Starter Pack or 1,000 coins), Leaf Swirl (600 coins). World 2/3 completion grants quiver badges + 300 coins. Ember, Frostglass, Cyan Streak and Ember Ash are post-MVP. |
| Alternatives considered | Ship the full mvp.md cosmetic list. |
| Consequences | Supersedes the cosmetic parts of the earlier catalogue. The MVP must live or die on bow feel, fair trajectory, reliable physics, readable structures, satisfying collapses, level variety and rapid restart. |
| Owner | OWNER |
| Status | Accepted |

### D-102 — Execution order and milestone restructure
| Field | Value |
|---|---|
| Decision ID | D-102 |
| Date | 2026-10-09 |
| Context | Owner-required execution order. Owner approval 2026-10-09 (plan review). |
| Decision | 1 Git baseline (done, AB-001) → 2 settings, layers, physics, test asmdefs, folders → 3 build the 5-level vertical slice only → 4 5–10 external casual-player tests before special arrows, maps, cosmetics, ads or 60 levels → 5 lock bow feel, trajectory accuracy, collision reliability, restart time, physics stability → 6 reusable material + interactive-object systems → 7 graybox all 60 levels → 8 validate every intended solution on target devices → 9 final art, sound, VFX, UI polish, cosmetics → 10 ads, IAP, consent, analytics, crash, Remote Config → 11 UK closed test, then Canada/Australia soft launch → 12 post-soft-launch optimisation. **Milestones:** M1 Foundations & Core Feel (G-M1) · M2 Core Loop + VS Graybox · M3 Vertical Slice + External Playtest + Feel Lock (G0) · M4 Systems Complete (G1) · M5 Content Graybox + Device Validation (G2) · M6 Art, Audio, UI Polish & Meta (G3) · M7 Platform Services & Monetisation (G4) · M8 Optimisation, Balance, QA & Release Prep (G5 Release Candidate) · M9 UK Closed Test & Submission (G-Release) · M10 Soft Launch (Canada, Australia). |
| Alternatives considered | World-by-world milestones (D-033). |
| Consequences | D-033 and D-078 superseded. 08/09 restructured; ticket IDs kept, milestones re-mapped. |
| Owner | OWNER |
| Status | Accepted |

### D-103 — Locked technical direction
| Field | Value |
|---|---|
| Decision ID | D-103 |
| Date | 2026-10-09 |
| Context | Owner approval 2026-10-09 (plan review). |
| Decision | Unity 6000.6.5f1 · URP · 3D PhysX constrained to the XY plane · 2.5D portrait diorama · kinematic swept arrow driven by the same `BallisticSolver` as the preview · `LevelData` SO + layout prefab · local JSON save only · uGUI + TextMeshPro · UI Toolkit only for editor tools · bundled launch content, no Addressables · LevelPlay behind `IAdsService` · Firebase behind service interfaces. |
| Alternatives considered | — |
| Consequences | Confirms D-003…D-013 and D-005. Changing any item needs OWNER approval. |
| Owner | OWNER |
| Status | Accepted |

### D-104 — Final owner confirmations: Starter Pack positioning, cosmetic-only rule, hard cuts, four delivery phases
| Field | Value |
|---|---|
| Decision ID | D-104 |
| Date | 2026-10-09 |
| Context | Owner final confirmation 2026-10-09 (second review), resolving the open items in D-101. |
| Decision | **(1) Royal Amethyst** is unlocked immediately by the Cosmetic Starter Pack **and** is purchasable for **2,500 coins** by free players (the D-101 coin price is confirmed). **(2) Positioning:** never call Royal Amethyst (or any cosmetic) "exclusive", "premium-only", "paid-only" or "limited". The Starter Pack value proposition is immediate unlock of Royal Amethyst + one included arrow trail (Gold Spark) + a modest coin bundle (500) — good-value convenience, not gameplay advantage or permanent exclusivity. **(3) Cosmetic-only:** no cosmetic changes aim assist, arrow damage/physics, quiver, stars, progression or any other gameplay outcome. **(4) Hard cuts:** Spring Plates (AB-093) and moving ice platforms (AB-108) are **Cut** from the MVP, not merely deferred; L53–55 use static ice slides + moving shields. Launch cosmetics are exactly 3 bow skins + 2 arrow trails; quiver badges ship only if low-cost and non-blocking. **(5) Delivery phases** (grouping of the D-102 order; canonical): **Phase 1** = git/project baseline, project settings/physics layers/test assemblies/foundations, the five-level vertical slice only, casual-player playtest gate (M0–M3). **Phase 2** = lock bow feel, arrow flight, preview accuracy, collision reliability, restart time and physics stability (G0); build reusable materials, objectives, destruction and interactive-object systems (M4); graybox all 60 levels with reusable prefabs + LevelData (M5). **Phase 3** = validate every intended solution on target devices (M5 exit, G2); final world art, UI, SFX, VFX, music, cosmetics and polish (M6). **Phase 4** = consent, analytics, Crashlytics, Remote Config, ads and IAP (M7); optimisation/QA/release prep (M8); UK internal test first, then the closed test (M9); Canada and Australia soft launch after the closed test (M10); optimise monetisation and balance only after real retention and funnel data exists (post-soft-launch). Pre-launch M8 work is limited to playtest-driven difficulty tuning (par/quiver fixes), not monetisation or economy optimisation. |
| Alternatives considered | Starter-pack-exclusive skin (weaker ethics, no long-term coin sink). Keeping moving ice platforms as cut-first. |
| Consequences | Closes the D-101 confirmation note. AB-093 and AB-108 status = Cut. 05/06 shop copy must avoid exclusivity wording. M8 "balance pass" is renamed "playtest-driven difficulty tuning". |
| Owner | OWNER |
| Status | Accepted |

### D-105 — Oak impact tuning and the timber "weak point" pole
| Field | Value |
|---|---|
| Decision ID | D-105 |
| Date | 2026-10-09 |
| Context | Building the VS on the 02 §6.4 spec values (Oak impactImpulse 6 Ns, max 10 Ns) produced hits that barely moved a loaded timber stilt; the "one clean shot" collapse in VS-02/VS-05 did not happen. |
| Decision | `AD_Oak` impactImpulse **15 Ns**, maxImpulse **20 Ns** (minSpeed 4, maxSpeed 13 m/s unchanged). New prefab `Struct_Pole_Timber_0.25x2` with a Breakable HP override of **4**: one solid Oak hit snaps it. It is the readable weak point used as the weak leg in VS-02 and VS-05. |
| Alternatives considered | Lower the timber density (makes every structure floaty). Raise only the impulse (the loaded post still did not move enough). Use explosive props in the VS (out of scope for M3). |
| Consequences | 02 §6.4 example values are superseded by the `AD_Oak` asset. Re-check at the M1 feel gate (AB-014) on device. The weak pole joins the structure library (AB-013). |
| Owner | CORE / PHYS |
| Status | Proposed (PO to confirm at the VS playtest gate) |

### D-106 — Vertical-slice layouts deviate from the 11 sketches (approach-path rule)
| Field | Value |
|---|---|
| Decision ID | D-106 |
| Date | 2026-10-09 |
| Context | The aim-space scan (`AimFinder`, every angle × power through the real preview simulation) showed that several 11 §3 sketches were unsolvable: the bow sits bottom-centre and arrows never curve back, so targets resting on ledges were unreachable from below. |
| Decision | Level design rule: **every required target needs a clear approach path** (a gap beside the ledge, or an exposed inward-facing side). VS-01 crest moved onto a slim post. VS-02/VS-05 use the weak pole. VS-04 "Market Shelf" redesigned as a precision shot (hit the crest, not the crates, or the crates topple into the vase). VS-05 rig lowered so the rope cut zone is reachable. Layouts are authored as JSON blueprints and imported (04 §4). |
| Alternatives considered | Moving the bow per level (breaks the fixed-camera/bow contract). Allowing arrows through ledges. |
| Consequences | 11 §3 sketches are now illustrative; the blueprints in `Assets/_Project/Levels/Blueprints/` are the source of truth. The rule is added to the level-design checklist. `AimFinder` becomes part of the level validator (AB-059). |
| Owner | LEVEL |
| Status | Proposed (PO to confirm at the VS playtest gate) |

### D-107 — Procedural placeholder art, synthesised audio and code-built UI for the vertical slice
| Field | Value |
|---|---|
| Decision ID | D-107 |
| Date | 2026-10-09 |
| Context | The owner asked for a fully playable game with assets. No AI 3D/2D generation provider is configured in the editor tooling, and D-089 allows AI assets only as placeholders anyway. |
| Decision | All VS art is **original and procedural**: meshes, materials and gradients from `ArtKit` (Arrow Buster ▸ Build ▸ Generate Content), UI sprites generated at runtime (`ProceduralTextures`), SFX and the music loop synthesised by `SfxSynth`. The UI is built in code by `GameplayUI` + `UiFactory` (no UI prefabs yet). The only third-party asset is the OFL font Lilita One (logged in `docs/art/ASSET_LICENSES.md`). |
| Alternatives considered | Asset Store packs (licence review and style lock-in before the M5 art direction lock). AI generators (not configured; placeholder-only per D-089). |
| Consequences | Satisfies graybox-first (CLAUDE.md rule 4) with a readable Greenwood palette. Final art replaces the generated assets in M6 (Phase 3). AB-023's `Prefabs/UI/*` deliverable is replaced by code-built views until the UI art pass. |
| Owner | ART / UI |
| Status | Proposed (PO to confirm at the VS playtest gate) |

### D-108 — Implementation conventions adopted during the VS build
| Field | Value |
|---|---|
| Decision ID | D-108 |
| Date | 2026-10-09 |
| Context | Small structural choices made while implementing AB-003…AB-025 that differ from the ticket text. |
| Decision | (1) Hit-stop policy lives in `FeedbackDirector` (no separate `HitStop` class); it still requests time scale only through `TimeScaleController` (D-061). (2) `IntendedSolutionRunner` and `LevelAudit` may set `Time.timeScale` for faster bot runs — dev/test-only exception to D-061. (3) `PlayerSettings.runInBackground = true` so editor/MCP play sessions keep ticking unfocused (no effect on mobile). (4) Levels are authored as JSON blueprints → `LevelData` + layout prefab via `LevelBlueprintImporter` (the ShotRecorder of AB-025 is replaced by recorded shots in the blueprint). (5) `UiTween.Delay` takes an owner object so delayed UI callbacks are dropped when their UI is destroyed (scene reload). |
| Alternatives considered | Separate HitStop component. Recording shots via an editor window. |
| Consequences | AGENT_SYSTEM ownership for hit-stop stays with FEEL. Bot runs restore `Time.timeScale = 1` when they finish (`IntendedSolutionRunner`). |
| Owner | ARCH |
| Status | Proposed (PO to confirm at the VS playtest gate) |
