# 00 — MVP Analysis

> **Source of truth:** [`/mvp.md`](../../mvp.md) (section refs below are written `§N`). This document interprets it; it never overrides it.
> When this analysis and `mvp.md` disagree, `mvp.md` wins unless a decision in [`10_DECISION_LOG.md`](10_DECISION_LOG.md) explicitly resolves the conflict.
> Status: **v2 — owner-approved 2026-10-09** (all open questions resolved; see §8 and D-080…D-104). Owner: Product Owner / Game Design Lead (`PO`).

---

## 1. Concise game summary

Arrow Buster is a **portrait, one-thumb, physics-puzzle archery game** for iOS and Android. Each level is a compact diorama — crate towers, hanging loads, ropes, balloons, barrels, shields, portals — with one or more **red required objectives** and sometimes **purple protected objects**. The bow sits fixed at the bottom-centre. The player drags back to draw, sees a dotted ballistic arc, and releases. Arrows embed, deflect, ricochet, cut, pop, ignite or teleport depending on the **arrow type** and the **material** they hit. Physics resolves in seconds. Clear every required objective before the **curated quiver** runs out. **Stars come only from arrow efficiency** (gold par → 3★, par+1 → 2★, any clear → 1★).

The MVP ships **60 hand-authored levels** across **3 worlds** (Greenwood Range, Sunscar Canyon, Frostspire Keep), **5 arrow types** (Oak, Heavyhead, Split, Fire, Bounce), **5 materials**, **6 required-objective types**, **≥8 interactive props**, protected objects and kill zones. Around that sit a world map, coins, cosmetic-only spending, a basic daily challenge, fair rewarded ads, capped interstitials, Remove Ads and a cosmetic starter-pack IAP, analytics, crash reporting and consent.

## 2. Player fantasy

> *"I saw the trick, made one clean bow shot, and the whole scene came down exactly as I hoped."* (§16)

A precision fantasy archer who **reads a structure, finds its weak point and wins with economy, not brute force**. The bow is a tactile hero prop. Arrows are clever tools rather than bullets: they pin, cut, pop, ignite and bank.

## 3. Core loop

```
 ┌───────────────────────────────────────────────────────────────────────────┐
 │ READ (≤5 s)      Scene + objective icons + materials/colours are legible │
 │   ↓                                                                       │
 │ AIM              Touch near bow → drag back → dotted arc (draw only)      │
 │   ↓                                                                       │
 │ RELEASE          Snap + whistle; arrow flies 1.5–2.5 s (target, D-036)    │
 │   ↓                                                                       │
 │ IMPACT           Material/arrow rule → embed / deflect / cut / pop / burn │
 │   ↓              40–70 ms hit-stop on meaningful direct impacts           │
 │ RESOLVE          Physics + chain reactions; settle ≤ ~2 s of calm         │
 │   ↓                                                                       │
 │ EVALUATE         All required cleared + 0.75 s settle → WIN               │
 │                  Protected lost → FAIL now · quiver empty + unresolved → FAIL│
 │   ↓                                                                       │
 │ REWARD / RETRY   Stars, coins, Next (1 tap) · Retry (1 tap, < 1 s)        │
 └───────────────────────────────────────────────────────────────────────────┘
 Meta loop: stars → world unlock (15/20 clears) → special arrows appear → coins → cosmetics → daily challenge → replay for 3★ / Bullseye medals
```

## 4. Core pillars (from §2, made testable)

| Pillar | What it means in practice | Measurable check |
|---|---|---|
| **One-thumb clarity** | Single-pointer input. No loadout screen. HUD has 4 zones only (§8). | New testers fire within 15 s with no instructions (§10, §15). |
| **Fair physics** | No hidden weights or sticky pieces. Deterministic gameplay physics; randomness only in cosmetics. | Every level's intended solution passes the automated solvability bot within a ±tolerance aim window (see `07`). |
| **Readable puzzles** | Material colour language + silhouettes + icons. Purple = protected, red = required. | 5-second screenshot test: ≥ 4/5 testers name the objective and one weak point. |
| **Fast recovery** | Retry is one tap and takes < 1 s. Level load is < 1 s. | Measured restart time p95 < 1.0 s on iPhone 11 and on the low-tier Android. |
| **Satisfying spectacle** | Hit-stop, material SFX, debris that clears itself. VFX stays off the outcome until it's readable. | Playtest "favourite moment" survey; VFX never hides an objective (QA check). |

## 5. Must-have versus post-MVP scope

### 5.1 MVP — must ship (§14 plus implied requirements)

| Area | Must-have items |
|---|---|
| Core action | Drag/draw/release bow; trajectory preview (shortened, never removed, after L15); ballistic arrows; quiver; win/fail/settle; soft-lock protection; 1-tap restart; stars |
| Arrows | Oak, Heavyhead, Split, Fire, Bounce — level-curated quivers, no player loadout |
| Materials | Straw/wicker, Timber, Stone, Ice crystal, Metal (MaterialProfile-driven) |
| Required objectives (6) | Red crest target, Supply crate, Hanging lantern, Cursed orb, Training dummy, Banner rope |
| Interactive props (8) | Rope/chain, Balloon cluster, Oil jar, Powder barrel, Rolling boulder, Wind fan, Rotating/moving shield, Portal ring. *(Spring plate is **cut**, D-086; moving ice platforms are **cut**, D-104.)* |
| Protected/hazards | Royal vase, Sleeping fox, Royal relic (L60), Water pit, Spike bed, Pit/out-of-bounds, Shielded target (a physical-cover pattern, not a new system) |
| Content | 60 levels, 3 worlds × 20, a set piece every 5th level, boss puzzles at L20/40/60, 2 tutorial levels per new mechanic |
| Meta | World map (vertical path, 20 nodes per world), 15/20 unlock rule, stars, coins, cosmetics — **exactly 3 bow skins (Oak Ranger, Moonwood, Royal Amethyst) + 2 trails (Gold Spark, Leaf Swirl)**; quiver badges only if low-cost and non-blocking (D-101, D-104) — basic daily challenge (cut-first, D-094), Bullseye medal collection |
| UX | Static splash (D-101), Home, World map, Gameplay HUD, Win, Fail, Bow Forge, Settings (music, SFX, haptics, reduced particles, colour-assist outlines, restore purchases, privacy) |
| Feedback | Material SFX, impact VFX, haptics (draw threshold / impact / success), hit-stop, music per world |
| Business | Rewarded +1 arrow (1★ cap, disclosed before the ad — D-084), rewarded 2× coins, capped interstitials (D-093), Remove Ads IAP (£/USD 3.99), Cosmetic Starter Pack IAP (£/USD 2.99) — D-091; cosmetics never affect gameplay (D-104) |
| Platform | Firebase Analytics + Crashlytics + Remote Config, Unity LevelPlay ads, Unity IAP — all behind interfaces (D-090); consent-aware init with UK/EEA consent gating of analytics, ad personalisation and crash reporting (D-096; UMP + ATT); local JSON save; iOS + Android builds; 60 FPS iPhone 11, 30 FPS low-end fallback |

### 5.2 Cut and cut-first (owner decisions D-086, D-101, D-104 — full order in `08` §Cut list)

**Cut from the MVP:** Spring Plates · choice-loadout levels · moving ice platforms (L53–55 use static ice slides + moving shields) · animated branded splash (static splash only) · more than 3 bow skins / 2 trails (Ember, Frostglass, Cyan Streak, Ember Ash are post-MVP).

**Cut-first (build only if the vertical slice is excellent and the project is genuinely ahead of schedule):** Daily Challenge if it risks the 60-level campaign · quiver badges if not low-cost · any cloud save, leaderboard, achievement, push-notification, seasonal or multiplayer system (already post-MVP).

The MVP lives or dies on: **bow feel, fair trajectory, reliable physics, readable structures, satisfying collapses, level variety and rapid restart.**

### 5.3 Post-MVP (explicitly deferred)

Drill Arrow · cloud save · weekly challenge map + leaderboard · seasonal cosmetic path · themed limited skins · worlds 4+ · in-game hint system · achievements (Game Center / Play Games) · localisation beyond English · push notifications · Addressables/remote content delivery · A/B tests (allowed only after soft-launch baselines exist).

### 5.4 Non-goals (never in MVP, §14)

PvP, guilds, chat, UGC levels, procedural levels presented as handcrafted, real-time multiplayer, character progression or gear stats, energy timers, narrative cutscenes or dialogue trees, more than 3 worlds or 5 arrow types, sold or consumable special arrows, hard currency for retries.

## 6. Smallest playable vertical slice (the "fun proof")

Full definition: [`11_VERTICAL_SLICE.md`](11_VERTICAL_SLICE.md). In summary: one Greenwood Range environment section, Oak Arrow only, timber crates, red crest target, a cuttable rope, a royal vase, limited quiver, win/fail/restart, 1–3★, basic SFX/VFX/haptics, one tutorial prompt, a clean HUD, polished Win/Fail screens and debug-sink analytics. It contains **5 levels**: direct hit → weak-support collapse → rope-cut drop → protected-vase precision → combined test.

**Go/no-go question it answers:** *Do 5–10 casual players, given no instructions, discover weak points, enjoy the shot-to-collapse moment, and voluntarily replay for 3★?*

## 7. Assumptions (A-xx) — adopted unless the owner overrides

| ID | Assumption | Basis |
|---|---|---|
| A-01 | Team is one developer plus AI coding agents. Art may be partly contracted. | The prompt says "small indie team… solo/indie"; repo history matches. |
| A-02 | English only at launch, with all UI strings behind keys (localisation-ready). | §8 mentions no languages. |
| A-03 | iPhone and iPad (universal) plus Android phones. Tablets get a pillarboxed play area with extended scenery. | `targetDevice: 2` in ProjectSettings. |
| A-04 | Levels use world-local IDs (`W2_L10`) **and** a global index 1–60. | D-047; the §5 table now lists both (D-082). |
| A-05 | "Unlock" for special arrows means **first curated appearance plus a collection showcase**. Players never hold special arrows outside levels. | §5 loadout rule. |
| A-06 | No backend; all economy is client-side. Tampering risk is accepted because nothing purchasable affects fairness. | §12. |
| A-07 | Rated 13+, not child-directed. No Families/COPPA program. Still use a non-personalised-ads fallback. | §0 audience 13+. |
| A-08 | Licensed fonts and processed licensed SFX are allowed, logged in `docs/art/ASSET_LICENSES.md`. Brand-defining visuals and music are original. AI-generated assets are concepts/placeholders only. | **Decided** (D-089). |
| A-09 | Unity 6000.6.5f1 is **locked for the whole MVP**; no upgrade spike. | **Decided** (D-080). |
| A-10 | Physics: 3D PhysX bodies constrained to the XY plane (MVP §12, CLAUDE.md). The arrow is a **kinematic swept projectile**, not a dynamic rigidbody. | Decision D-005. |

## 8. Owner decisions (Q-xx) — all resolved 2026-10-09

All 22 open questions were answered by the OWNER on 2026-10-09. Full entries: [`10_DECISION_LOG.md`](10_DECISION_LOG.md) D-080…D-104.

| ID | Question | Owner decision | Decision |
|---|---|---|---|
| Q-01 | Unity version for the MVP | **Unity 6000.6.5f1 locked for the whole MVP.** No upgrade spike; upgrade only post-launch or for a release-blocking platform issue. | D-080 |
| Q-02 | Product name | Public/UI/store: **Arrow Buster**. Technical identifiers: **`ArrowBuster`** (lowercase where platform convention requires, e.g. `com.attila.arrowbuster`). Legal/store/domain clearance before store assets (M8). | D-081 |
| Q-03 | Special-arrow first appearances | **Heavyhead Global L13 / W1_L13 · Split Global L30 / W2_L10 · Fire Global L36 / W2_L16 · Bounce Global L47 / W3_L07.** mvp.md §5 updated. | D-082 |
| Q-04 | Fire while physics resolves? | Yes, after a **0.35 s re-nock cooldown**; firing is disabled once all objectives are cleared and in Won/Failed. | D-083 |
| Q-05 | Bonus-arrow clear stars | **Capped at 1★**; counts as completed. "Bonus Arrow Used — 1★ Max" is shown before the ad. | D-084 |
| Q-06 | Preview content | Shows **wind influence, portal exit paths and the first bounce.** Never hide path-changing mechanics. | D-085 |
| Q-07 | Spring plate | **Cut** from the launch MVP. | D-086 |
| Q-08 | Pulleys | **No pulleys**: rope-hung weights, hinge seesaws, dropping loads, rolling boulders. | D-087 |
| Q-09 | Split Arrow trigger | **Timed split** with a preview marker and a pre-split glow pulse; impact before the split → forward fan. | D-088 |
| Q-10 | Licensed assets | Licensed fonts and processed licensed SFX allowed (recorded). Brand-defining visuals and music original. | D-089 |
| Q-11 | Vendors | **Firebase** (Analytics, Crashlytics, Remote Config) + **Unity LevelPlay** (ads) + Unity IAP, all behind `IAnalyticsService`, `ICrashReportingService`, `IRemoteConfigService`, `IAdsService`, `IIapService`; mocks until M7. | D-090 |
| Q-12 | IAP prices | **Remove Ads £3.99 / USD 3.99** (interstitials only). **Cosmetic Starter Pack £2.99 / USD 2.99** = Royal Amethyst + Gold Spark + 500 coins; positioned as convenience, never exclusive. | D-091, D-104 |
| Q-13 | Blasts vs protected objects | No direct damage or force; structure pieces moved by the blast can still break them. | D-092 |
| Q-14 | Interstitials | ≥ 10 min cumulative active play per install, ≥ 3 completed levels since the last one, ≥ 120 s cooldown; **never** after a fail, during a level, during onboarding, after a purchase, after a rewarded ad, or on app resume. Remote-configurable. | D-093 |
| Q-15 | Daily Challenge | Unlocks after Global L10; 3 already-completed levels by local date seed; gold-par quiver; no leaderboard/streak/backend/push. Cut-first. | D-094 |
| Q-16 | "Royal Violet" | Renamed **Royal Amethyst**. | D-095 |
| Q-17 | Crash reporting before consent | **Consent-gated** in UK/EEA together with analytics and ad personalisation (consent-aware SDK init). Final legal/privacy review before launch. | D-096 |
| Q-18 | Privacy policy | OWNER hosts it **before SDK integration — at the start of M7 at the latest.** | D-097 |
| Q-19 | Markets | **UK internal test first, then the closed test; soft launch in Canada and Australia** after the closed test. | D-098, D-104 |
| Q-20 | Google Play testing rule | OWNER verifies in **project week 1**; start recruiting eligible testers early if it applies. | D-099 |
| Q-21 | AI-generated assets | **Concepts and temporary placeholders only**, unless the licence, commercial rights and originality review are documented. | D-089 |
| Q-22 | Mac / Apple account | **Mac + Apple Developer account by project week 3.** Apple/Google developer accounts + store IAP products by **project week 5**. | D-100 |

Additional owner confirmations (D-104): Royal Amethyst is both in the Starter Pack **and** purchasable for 2,500 coins; no cosmetic is described as exclusive, premium-only, paid-only or limited; cosmetics never change aim assist, damage, quiver, stars, progression or any gameplay outcome; Spring Plates and moving ice platforms are **cut** (AB-093, AB-108); delivery follows four phases (`08` §0).

**Remaining open items** (not blocking implementation): see [`10_DECISION_LOG.md`](10_DECISION_LOG.md) entries still marked `Proposed` — they stay in force as working defaults and are confirmed at the gate named in each entry.

## 9. Scope risks

| ID | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| S-01 | **60 polished levels in 10 weeks** is unrealistic for one dev — content and art are the critical path. | High | High | Template-driven level production + validator + solvability bot. Graybox all 60 before art. Realistic plan in `08` (≈31 weeks to submission, D-102). Cut list. |
| S-02 | **Three bespoke 3D environment art kits** plus a hero bow, characters, VFX and UI. | High | High | Low-poly, gradient-atlas style (one 256 px palette texture per world). Modular kit of ~25 pieces per world. Lock art direction at the VS gate. |
| S-03 | 8 props + 5 arrows + 5 materials create a combinatorial interaction matrix (fire × straw × oil × balloon × wind...). | High | Medium | Interaction matrix in `03` §9 with explicit "no interaction" cells. Each cell gets a test level in `Tests/PlayMode`. |
| S-04 | Monetisation/consent/SDK integration routinely eats 1–2 weeks (EDM4U, CocoaPods, privacy manifests). | Medium | High | Interfaces + mock services from M2. SDK spike in M7. Pick one mediation stack only. |
| S-05 | Hidden mechanics not in the prop list: moving ice platforms (now cut, D-104), counterweights, shielded targets, "choice levels", Bullseye. | Medium | Medium | Each mapped onto an existing generic component (KinematicMover, HingeJoint lever, physical cover, LevelData flag, BullseyeMarker). See `03`. |
| S-06 | Daily challenge requires date handling and level-reuse UX. | Medium | Low | Offline local-date seed, 3 levels, no streaks in MVP. |
| S-07 | Set-piece/boss levels (L20/40/60) are larger and are physics-perf outliers. | Medium | Medium | A per-level body budget is enforced by the validator (≤ 60 dynamic gameplay bodies). Set pieces are profiled on the low tier. |

## 10. Primary technical risks

| ID | Risk | Mitigation (owner) |
|---|---|---|
| T-01 | **Stacked-body stability** in PhysX (jitter, creep, explosions on spawn) breaks "fair physics". | Bodies start asleep. Authoring snaps to a 0.05 m grid with zero overlap. `maxDepenetrationVelocity` clamp. Solver iterations 8/2. Mass-ratio validator (≤ 10:1 touching). 3-second idle-stability test per level. Physics spike gate in M1. (Physics) |
| T-02 | **Arrow tunnelling / unreliable hits** at high speed. | Kinematic swept arrow (SphereCast per fixed step) with a single shared integrator for flight and preview. No dynamic-rigidbody arrows. (Core) |
| T-03 | **Preview ≠ actual flight**, which destroys trust. | The same `BallisticSolver.Step()` drives both. EditMode test asserts preview == flight within 1 mm over 3 s. (Core) |
| T-04 | **Non-determinism** across devices (PhysX on ARM vs x86) makes intended solutions flaky. | No gameplay randomness. Enhanced determinism enabled. Solutions are designed with tolerance (8–12% aim window, §7). The bot tests ±jitter and every level is checked on-device. (Physics/QA) |
| T-05 | **Long or ambiguous settle** (balloons bobbing, movers, rolling boulders). | SettleMonitor ignores kinematic and "ambient" bodies. Hard caps: out-of-arrows resolution cap 5 s, win-settle cap 3 s. (Physics) |
| T-06 | **Chain-reaction blow-ups** (explosion + stack + barrel chain). | Clamped impulses. Explosion force with falloff and capped upward modifier. Barrel chain delay 0.15 s. Debris never collides with gameplay bodies. (Physics/Interactive) |
| T-07 | **Enter Play Mode with domain + scene reload disabled** (`EditorSettings` options = 3): static state leaks between plays. | Every static (events, registries, service locator) resets in `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`. Enforced in code review. (Architect) |
| T-08 | **Mobile perf** with 3D shadows, physics and particles at 60 FPS on iPhone 11 / 30 FPS on low-end Android. | Budgets in `01` §11 and `07`. Quality tiers. Debris cap 40/20. Single directional light. Fake depth of field in the art. (Platform) |
| T-09 | **Ad/consent SDK build breakage** (Gradle/EDM4U/CocoaPods, privacy manifests). | Vendors fixed early (Firebase + LevelPlay + Unity IAP, D-090); optional isolated compile spike late in M6; integration in M7 behind `ArrowBuster.Integrations`. iOS device builds from M3 (Mac by week 3, D-100). (Monetisation/Platform) |
| T-10 | **AI agents editing shared YAML** (scenes, prefabs, TagManager) concurrently cause corrupt merges. | Single-owner file locks, minimal scene content, prefab-first workflow, UnityYAMLMerge driver configured. (Integrator) |

## 11. Product risks

| ID | Risk | Mitigation |
|---|---|---|
| P-01 | Genre familiarity: players see "another knock-down game". | Lean into the bow's tactile draw, arrow-specific verbs (cut, pin, pop, ignite, bank, portal) and an original fantasy ranger identity. Store creatives show *arrow-only* tricks. |
| P-02 | Puzzles feel like guesswork ("why did that not fall?"). | Readability pillar. Material colour language. 5-second screenshot test. Fail-reason messaging ("The vase broke!"). |
| P-03 | Repetition within 20–60 levels (§6 calls it a genre risk). | Rule: no repeated layout or solution pattern within 10 levels. The delivery tracker logs a "solution archetype" tag per level and the validator warns. |
| P-04 | Ads feel predatory and harm retention/reviews. | Ethical rules enforced in code (`AdPolicy`) with unit tests. No ads in the first 10 minutes. Never after a fail. |
| P-05 | Originality/IP challenge vs. the reference game. | Originality guardrails in `05`. Asset-licence register. Review of store screenshots against the reference before submission. |
| P-06 | Soft-launch KPIs miss (L1 95%, W1 45%). | Funnel instrumented from the VS onward. Remote-config tunables for quiver sizes and preview length. Fast patch path. |

## 12. Suggested decisions for unspecified areas (summary — full rationale in `10_DECISION_LOG.md`)

| Area left unspecified by §1–16 | Decision |
|---|---|
| Arrow physics model | Kinematic swept projectile with explicit impulses on impact (D-005) |
| Fixed timestep | 1/60 s, max allowed timestep 0.1 s, solver 8/2, enhanced determinism ON (D-006) |
| Debris collisions | Debris collides only with the static environment and never deals damage (D-008) |
| Rope implementation | Joint-based constraint + trigger capsule for cutting + LineRenderer visual — no segment chains (D-009) |
| Level format | `LevelData` ScriptableObject (rules) + layout prefab (geometry); JSON only for saves (D-010) |
| Level restart | Re-instantiate the layout prefab inside the Gameplay scene, no scene reload (D-011) |
| UI tech | uGUI + TextMeshPro; UI Toolkit only for editor tooling (D-012) |
| Fire-next-arrow timing | Allowed during resolution after a 0.35 s cooldown; disabled once objectives are cleared or the level is won/failed (D-083) |
| Fail confirmation | After the last arrow resolves: wait for 2 s calm (§3 soft-lock rule), hard-capped at 5 s (D-015) |
| Coin values | First clear 20 + 10/★; new-star replay 10/★; plain replay 5; daily 100 (D-026) |
| Addressables | Not in MVP (D-013) |
| Tweening | Small in-house `UiTween` first; LitMotion is the approved fallback (D-031) |
| Haptics | In-house thin native bridge (iOS UIImpactFeedbackGenerator / Android VibrationEffect) (D-032) |
| Docs location | `docs/` (lowercase); `docs/GDD.md` becomes a pointer to `/mvp.md` (D-001, D-002) |
| Vendors | Firebase (Analytics, Crashlytics, Remote Config) + Unity LevelPlay + Unity IAP behind interfaces (D-090) |
| Locked tech direction | Unity 6000.6.5f1 · URP · 2.5D PhysX · kinematic swept arrow · LevelData + prefab · local JSON · uGUI/TMP · no Addressables (D-103) |

## 13. Repository audit snapshot (2026-10-09)

| Item | State | Action |
|---|---|---|
| Unity | 6000.6.5f1, URP 17.6 (Mobile + PC RP assets), Input System 1.20, uGUI 2.6, Test Framework 1.8, Timeline, MCP for Unity v10 | Keep. Remove Visual Scripting, Collab Proxy and AI Navigation (AB-002). |
| Milestone 0 | Done: folder map, 3 asmdefs, `GameConstants`, `GameEnums`, `LevelData`, `QuiverEntry`, `ProjectSetup` menu, 4 empty scenes in the build list, `LevelDataTests` | Preserve. Extend `LevelData` to schema v2 in M2. |
| Code debt | `LevelData` uses public fields (CLAUDE.md wants `[SerializeField] private`). `ObjectiveKind`/`ProtectedKind` enums are closed sets (fine). No PlayMode test asmdef. | Migrate during AB-016. Add `ArrowBuster.Tests.PlayMode` in AB-002. |
| Physics settings | Defaults: fixed Δt 0.02, solver 6/1, enhanced determinism off, no custom layers | AB-002 applies D-006 and the layer table in `03`. |
| Editor settings | Force Text serialization ✔. Enter Play Mode options: domain **and** scene reload disabled | Keep (fast iteration) but enforce static-reset rules (T-07). |
| URP Mobile | Forward, render scale 0.8, MSAA off, HDR on, soft shadows off, shadow distance 50 | Tune per tier in M8 (HDR off on Low, shadow distance ≈ 25). |
| Template leftovers | `Assets/Scenes/SampleScene.unity`, `Assets/TutorialInfo/`, `Assets/Readme.asset`, `Assets/InputSystem_Actions.inputactions` (default map), `Settings/PC_*`, `SampleSceneProfile` | Delete in AB-002 (keep `PC_RPAsset` only if editor-only quality is wanted). Remove the template input asset; bow input polls `Pointer.current` via `BowInputReader` (D-007). |
| Version control | **Done (AB-001):** git + LFS + UnityYAMLMerge driver; baseline commit `7a06460` pushed to the private repo `github.com/atiwhocodes/ArrowBuster` (`main`). | — |
| Docs | `Docs/GDD.md` was an identical copy of `mvp.md` (name spelling only). `Docs/claude-commands/` holds 2 slash commands (not installed). | Folder renamed to `docs/`. GDD now points to `mvp.md`. Commands updated; install them into `.claude/commands/` on request. |
| Claude agents | No `.claude/` folder | Subagent definitions created in `.claude/agents/` (see `docs/agents/AGENT_SYSTEM.md`). |
