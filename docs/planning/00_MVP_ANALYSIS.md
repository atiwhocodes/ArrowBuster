# 00 — MVP Analysis

> **Source of truth:** [`/mvp.md`](../../mvp.md) (section refs below are written `§N`). This document interprets it; it never overrides it.
> When this analysis and `mvp.md` disagree, `mvp.md` wins unless a decision in [`10_DECISION_LOG.md`](10_DECISION_LOG.md) explicitly resolves the conflict.
> Status: **Draft v1 — 2026-10-09.** Owner: Product Owner / Game Design Lead (`PO`).

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
| Interactive props (≥8 of 9) | Rope/chain, Balloon cluster, Oil jar, Powder barrel, Rolling boulder, Wind fan, Rotating/moving shield, Portal ring; *(Spring plate is listed in §4 but never scheduled in §6 — see Q-07)* |
| Protected/hazards | Royal vase, Sleeping fox, Royal relic (L60), Water pit, Spike bed, Pit/out-of-bounds, Shielded target (a physical-cover pattern, not a new system) |
| Content | 60 levels, 3 worlds × 20, a set piece every 5th level, boss puzzles at L20/40/60, 2 tutorial levels per new mechanic |
| Meta | World map (vertical path, 20 nodes per world), 15/20 unlock rule, stars, coins, cosmetics (bow skins, trails, quiver badges), basic daily challenge, Bullseye medal collection |
| UX | Splash, Home, World map, Gameplay HUD, Win, Fail, Bow Forge, Settings (music, SFX, haptics, reduced particles, colour-assist outlines, restore purchases, privacy) |
| Feedback | Material SFX, impact VFX, haptics (draw threshold / impact / success), hit-stop, music per world |
| Business | Rewarded +1 arrow (fair rules), rewarded 2× coins, capped interstitials, Remove Ads IAP, cosmetic starter pack IAP |
| Platform | Analytics, crash reporting, remote config, consent/privacy (GDPR/UMP, ATT), local JSON save, iOS + Android builds, 60 FPS iPhone 11, 30 FPS low-end fallback |

### 5.2 Should-have (in MVP, but first to cut — full order in `08` §Cut list)

- "Choice levels" from L31 (§5 already marks them optional).
- Spring plate prop.
- Moving ice platforms (L53–55) — can become static slides.
- Animated branded splash (a static splash is fine).
- Five bow skins and four trails — the minimum viable set is 3 skins and 2 trails.

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
| A-04 | Levels use world-local numbers (`W2_L08`) **and** a global index 1–60. The §5 unlock table uses world-local numbers. | §5 "World 2, level 8". |
| A-05 | "Unlock" for special arrows means **first curated appearance plus a collection showcase**. Players never hold special arrows outside levels. | §5 loadout rule. |
| A-06 | No backend; all economy is client-side. Tampering risk is accepted because nothing purchasable affects fairness. | §12. |
| A-07 | Rated 13+, not child-directed. No Families/COPPA program. Still use a non-personalised-ads fallback. | §0 audience 13+. |
| A-08 | Licensed stock **SFX sources** may be used only if heavily processed and logged in an asset-licence register. Identity assets (bow, characters, props, UI, logo, music) are custom. | The originality rule is strict for identity. Pending owner confirmation (Q-10). |
| A-09 | Unity 6000.6.5f1 (current editor) stays for development. Upgrading to the newest Unity 6 **LTS** stream is evaluated as a timeboxed spike before M9 (see Q-01). | CLAUDE.md fixes the version; LTS status needs verification. |
| A-10 | Physics: 3D PhysX bodies constrained to the XY plane (MVP §12, CLAUDE.md). The arrow is a **kinematic swept projectile**, not a dynamic rigidbody. | Decision D-005. |

## 8. Open questions (Q-xx) — need an owner decision; each has a recommended default

| ID | Question | Recommended default (used by all plans until answered) |
|---|---|---|
| Q-01 | Ship on 6000.6.x or move to the latest Unity 6 LTS before release? | Develop on 6000.6.5f1. Run an upgrade spike at the start of M9 and adopt LTS if it is green. |
| Q-02 | Final product name — "Arrow Buster" vs "ArrowBuster"? Trademark/store-name search? | Use "Arrow Buster" (matches ProjectSetup and GDD). Do a store/trademark search before M10. |
| Q-03 | §5 unlock levels conflict with the §6 map beats (Heavyhead 15 vs 13–15; Split W2L8=28 vs 30–32; Bounce W3L10=50 vs 47–49; Fire W2L18=38 vs "36–39 combine fire"). | **Use the §6 map beats.** First appearances: Heavyhead L13, Split L30, Fire L36, Bounce L47. (D-014) |
| Q-04 | Can the player fire the next arrow while physics is still resolving? | **Yes**, after a 0.35 s re-nock cooldown. Win/fail evaluation is continuous. (D-016) |
| Q-05 | Do clears that used a rewarded bonus arrow earn 2–3★? | **No — capped at 1★.** The level still counts as cleared. (D-024) |
| Q-06 | Does the trajectory preview show wind, portal exits and Bounce ricochets? | **Yes to all three, within preview length.** Hidden forces are unfair. Difficulty comes from a shorter preview, not deception. (D-017) |
| Q-07 | Spring plate is listed but never scheduled. | Keep it as a Should-have and place it in W2 L33–35 (counterweights/boulders) only if M6 is on schedule. Otherwise cut — 8 props remain. (D-021) |
| Q-08 | "Counterweights" (L33–35): true pulleys? | **No pulleys.** Use seesaw levers (hinge) and rope-hung weights. (D-022) |
| Q-09 | Split Arrow trigger: impact or timer? | **Timed split** at a fixed flight time (shown in the preview). If it impacts before the split, it splits at impact into a forward fan. (D-019) |
| Q-10 | May licensed stock assets (SFX libraries, fonts) be used at all? | Fonts: an OFL/commercial licence is OK. SFX: licensed sources OK if processed. Everything else custom. (A-08) |
| Q-11 | Ad mediation / analytics / crash vendor? | Decide at the M8 gate using the criteria in `01` §5. Default lean: **AppLovin MAX or Unity LevelPlay** for mediation, plus **Firebase** (Analytics + Crashlytics + Remote Config). Interfaces are built in M2 regardless. |
| Q-12 | IAP prices? | Remove Ads at USD 3.99 tier; Starter Pack at USD 2.99 tier. Remote-configurable display only; store prices set in the consoles. |
| Q-13 | Does the powder-barrel blast push protected objects, or only not damage them? | **No damage and no blast force on protected objects.** Debris thrown by the blast is cosmetic and cannot hit them. Structure pieces thrown by the blast *can* — that is the player's responsibility. (D-020) |
| Q-14 | The interstitial "first 10 minutes" — per session or per install? | **Per install, cumulative active playtime ≥ 10 min**, plus ≥ 3 completed levels since the last interstitial, plus a ≥ 120 s cooldown, plus never after a fail. All values are remote-configurable. (D-025) |
| Q-15 | Daily challenge source levels when the player has few completions? | Unlocks after completing L10. It picks 3 completed levels by date-seeded hash; quiver = gold par of each level. (D-027) |
| Q-16 | Does the "Royal Violet" bow skin conflict with "purple = hazard/protected"? | The bow is never in the play field, so this is acceptable. Rename to "Royal Amethyst" only if testers confuse it. Low priority. |
| Q-17 | Legal basis for running crash reporting before/without consent in EEA/UK? | Crash reporting without identifiers under legitimate interest, disclosed in the privacy policy. **Needs legal/OWNER confirmation before M8** (`06` §9). |
| Q-18 | Who writes and hosts the privacy policy (URL needed for both stores and in Settings)? | OWNER provides a hosted policy by M8, built from the data inventory in `07` §13. |
| Q-19 | Soft-launch markets? | Pick 1–2 English-speaking, lower-CPI markets after the closed test (decided at G-Release). Affects consent regions and remote-config defaults. |
| Q-20 | Google Play account type — does the new-personal-account closed-testing requirement apply? | Verify by M5. If it applies, start the Play closed track by M8 (D-079). |
| Q-21 | May AI-generated art/audio be used, and under which tool licence? | Not in shipped assets until OWNER signs off per tool (D-042). Concept/reference use only. |
| Q-22 | When is a Mac (and Apple Developer account) available for iOS builds? | Ideally from M3 for the VS device check; **mandatory by M8** (ATT, privacy manifests, IAP sandbox). Store accounts and IAP products by M5 (D-072). |

## 9. Scope risks

| ID | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| S-01 | **60 polished levels in 10 weeks** is unrealistic for one dev — content and art are the critical path. | High | High | Template-driven level production + validator + solvability bot. Graybox all 60 before art. Realistic plan in `08` (≈22–26 weeks). Cut list. |
| S-02 | **Three bespoke 3D environment art kits** plus a hero bow, characters, VFX and UI. | High | High | Low-poly, gradient-atlas style (one 256 px palette texture per world). Modular kit of ~25 pieces per world. Lock art direction at the VS gate. |
| S-03 | 9 props + 5 arrows + 5 materials create a combinatorial interaction matrix (fire × straw × oil × balloon × wind...). | High | Medium | Interaction matrix in `03` §9 with explicit "no interaction" cells. Each cell gets a test level in `Tests/PlayMode`. |
| S-04 | Monetisation/consent/SDK integration routinely eats 1–2 weeks (EDM4U, CocoaPods, privacy manifests). | Medium | High | Interfaces + mock services from M2. SDK spike in M7. Pick one mediation stack only. |
| S-05 | Hidden mechanics not in the prop list: moving ice platforms, counterweights, shielded targets, "choice levels", Bullseye. | Medium | Medium | Each mapped onto an existing generic component (KinematicMover, HingeJoint lever, physical cover, LevelData flag, BullseyeMarker). See `03`. |
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
| T-09 | **Ad/consent SDK build breakage** (Gradle/EDM4U/CocoaPods, privacy manifests). | SDK spike branch in M7. Android-first verification. Mac build check every milestone from M8. (Monetisation/Platform) |
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
| Fire-next-arrow timing | Allowed during resolution after a 0.35 s cooldown (D-016) |
| Fail confirmation | After the last arrow resolves: wait for 2 s calm (§3 soft-lock rule), hard-capped at 5 s (D-015) |
| Coin values | First clear 20 + 10/★; new-star replay 10/★; plain replay 5; daily 100 (D-026) |
| Addressables | Not in MVP (D-013) |
| Tweening | Small in-house `UiTween` first; LitMotion is the approved fallback (D-031) |
| Haptics | In-house thin native bridge (iOS UIImpactFeedbackGenerator / Android VibrationEffect) (D-032) |
| Docs location | `docs/` (lowercase); `docs/GDD.md` becomes a pointer to `/mvp.md` (D-001, D-002) |

## 13. Repository audit snapshot (2026-10-09)

| Item | State | Action |
|---|---|---|
| Unity | 6000.6.5f1, URP 17.6 (Mobile + PC RP assets), Input System 1.20, uGUI 2.6, Test Framework 1.8, Timeline, MCP for Unity v10 | Keep. Remove Visual Scripting, Collab Proxy and AI Navigation (AB-002). |
| Milestone 0 | Done: folder map, 3 asmdefs, `GameConstants`, `GameEnums`, `LevelData`, `QuiverEntry`, `ProjectSetup` menu, 4 empty scenes in the build list, `LevelDataTests` | Preserve. Extend `LevelData` to schema v2 in M2. |
| Code debt | `LevelData` uses public fields (CLAUDE.md wants `[SerializeField] private`). `ObjectiveKind`/`ProtectedKind` enums are closed sets (fine). No PlayMode test asmdef. | Migrate during AB-016. Add `ArrowBuster.Tests.PlayMode` in AB-002. |
| Physics settings | Defaults: fixed Δt 0.02, solver 6/1, enhanced determinism off, no custom layers | AB-002 applies D-006 and the layer table in `03`. |
| Editor settings | Force Text serialization ✔. Enter Play Mode options: domain **and** scene reload disabled | Keep (fast iteration) but enforce static-reset rules (T-07). |
| URP Mobile | Forward, render scale 0.8, MSAA off, HDR on, soft shadows off, shadow distance 50 | Tune per tier in M9 (HDR off on Low, shadow distance ≈ 25). |
| Template leftovers | `Assets/Scenes/SampleScene.unity`, `Assets/TutorialInfo/`, `Assets/Readme.asset`, `Assets/InputSystem_Actions.inputactions` (default map), `Settings/PC_*`, `SampleSceneProfile` | Delete in AB-002 (keep `PC_RPAsset` only if editor-only quality is wanted). Remove the template input asset; bow input polls `Pointer.current` via `BowInputReader` (D-007). |
| Version control | **No git repo.** `.gitignore` and `.gitattributes` (LFS + unityyamlmerge) exist. Git 2.55 and LFS 3.7 are installed. | AB-001: `git init`, `git lfs install`, configure the UnityYAMLMerge driver, first commit (needs owner approval). |
| Docs | `Docs/GDD.md` was an identical copy of `mvp.md` (name spelling only). `Docs/claude-commands/` holds 2 slash commands (not installed). | Folder renamed to `docs/`. GDD now points to `mvp.md`. Commands updated; install them into `.claude/commands/` on request. |
| Claude agents | No `.claude/` folder | Subagent definitions created in `.claude/agents/` (see `docs/agents/AGENT_SYSTEM.md`). |
