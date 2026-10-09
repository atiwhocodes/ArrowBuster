# 11 — Vertical Slice Plan

> Owner: `PO` (scope and the gate decision), `LEVEL` (levels), `INT` (integration). Status: Draft v1 — 2026-10-09.
> Milestones: built across **M1 → M2 → M3**; the **VS gate** closes M3 ([`08_PRODUCTION_ROADMAP.md`](08_PRODUCTION_ROADMAP.md), D-033).
> Level rules: [`04_LEVEL_PIPELINE.md`](04_LEVEL_PIPELINE.md). Systems: [`02_GAMEPLAY_SYSTEMS.md`](02_GAMEPLAY_SYSTEMS.md), [`03_PHYSICS_AND_OBJECTS.md`](03_PHYSICS_AND_OBJECTS.md). Art/audio: [`05_ART_AUDIO_UX.md`](05_ART_AUDIO_UX.md). The VS levels are real World 1 levels (D-050).

---

## 1. Purpose

Prove the **north star** with the smallest polished build:

> *"I saw the trick, made one clean bow shot, and the whole scene came down."*

**Gate question:** Do 5–10 casual players, given no instructions, (a) fire within 15 s, (b) discover weak points and the rope trick on their own, (c) enjoy the shot-to-collapse moment, and (d) voluntarily replay for 3★?

If not, we iterate on feel and readability before building World 1 content (M4+).

---

## 2. In scope

| # | Item | Definition of "in" for the VS | Main tickets |
|---|---|---|---|
| 1 | One polished Greenwood Range environment section | "Ranger Training Grounds": one backdrop diorama (grass valley, ruined stone arch, archery butts, fence, two tree clusters, pre-blurred hill cards, sky gradient) reused by all 5 levels with 2 backdrop variants. Final-quality lighting and palette. | AB-026, AB-027 |
| 2 | Bow drag / draw / release | D-018 relative drag in the aim zone. String stretch, release snap, full-draw glow. Haptics per `05` §12: `Light` when the draw becomes fire-able, `Selection` at full draw. | AB-007, AB-028 |
| 3 | Accurate trajectory preview | The same `BallisticSolver` as flight. Dots end at the first hit with an impact ring (D-040). Shown only while drawing. Scale 1.0. | AB-006, AB-008 |
| 4 | Standard Oak Arrow | Kinematic swept flight, embed in timber, deflect off stone, cut ropes and continue, pooled, cap 8. Tuning preset chosen at the M1 gate (D-036). | AB-005, AB-009, AB-011 |
| 5 | Crates / timber structure | `Struct_Crate_Timber_1x1/2x1`, `Struct_Post_Timber_0.5x2`, `Struct_Plank_Timber_4x0.25`, plus `Env_*` static ledges. `MP_Timber` final-ish tuning. | AB-010, AB-012, AB-013 |
| 6 | Required target | `Obj_CrestTarget` (red crest + icon + outline), breaks on an arrow hit or a heavy impact. | AB-018 |
| 7 | Rope / cut interaction | `Prop_Rope` holding a timber crate (D-009). Snap SFX + VFX. | AB-020 |
| 8 | Physics collapse | Stable stacks (P-01), readable collapses that settle ≤ 3 s, debris fades. | AB-012, AB-014 |
| 9 | Protected object | `Prot_RoyalVase` (purple outline + icon), fail reason "The vase broke!". | AB-018 |
| 10 | Limited quiver | `QuiverModel`, HUD quiver icons, next-arrow display. | AB-017, AB-036 |
| 11 | Win / fail condition | D-015 timings, protected-loss fail, soft-lock toast. | AB-019 |
| 12 | Restart | One tap, < 1 s on device (target ≤ 300 ms), from HUD/Fail/Win. | AB-016, AB-023 |
| 13 | 1–3★ calculation | `StarRules` (par, par+1, else 1★). Saved best. | AB-017, AB-022 |
| 14 | Basic SFX / VFX / haptics | Draw creak, release snap, whistle, timber impact/break, crest shatter, rope snap, vase break, win/fail stings, one music loop. Timber/crest/vase break VFX. Hit-stop 40–70 ms. Haptics per §8. | AB-030–AB-034 |
| 15 | One tutorial prompt | `TutorialPromptController` with exactly **one** authored callout ("Aim for the rope", VS-03), plus the L1 ghost-hand onboarding. | AB-039 |
| 16 | One clean HUD | Level label + pause (top-left), objective icons with a cross-out (top-centre), quiver + restart (top-right), clean bottom. Safe area. | AB-023, AB-036 |
| 17 | Polished Win / Fail screens | Win: star reveal (sequenced, sound per star), "Next" dominant, "Replay". Fail: reason line, "Retry" dominant. The coin line is shown with placeholder values (not banked; economy is M8). | AB-037, AB-038 |
| 18 | Analytics for the loop | `level_started`, `arrow_fired`, `object_triggered`, `level_completed`, `level_failed`, `level_restarted` + `level_quit` (defined in `06` §11) with §13 params, captured by `DebugAnalyticsService` into a per-session CSV. | AB-024, AB-042 |
| 19 | VS flow | Boot → VS playlist (`LC_VerticalSlice`) → VS-01..05 → a "Slice complete" card listing stars → replay any. Progress saved locally. | AB-040, AB-022 |
| 20 | Device build | Android dev APK on one Mid and one Low device (iOS if a Mac is available). | AB-043 |

## 3. Explicitly out of scope (VS)

- Special arrows (Heavyhead, Split, Fire, Bounce).
- Materials other than timber (stone only as static `Env` decor).
- Props other than rope.
- Objectives other than the crest.
- Fox and relic.
- World map, Home screen, Bow Forge, cosmetics, coins economy, daily challenge, Bullseye medals.
- Ads, IAP, consent flow, vendor analytics/crash SDKs, remote config fetch (local defaults only).
- Settings screen. Only Music/SFX/Haptics toggles exist, inside the Pause panel.
- Accessibility toggles beyond the haptics toggle (colour-assist outlines, reduced particles and reduced motion arrive in M4–M5 per `05` §16; Low-tier auto-on in M9). The always-on shape + icon rule (`05` §16) **is** in the VS.
- Localisation (English strings still go through `UIStrings` keys).
- Quality-tier auto-selection (fixed Mid settings; Low device runs at 30 FPS cap manually).
- Final logo, store assets, splash animation (static splash only).

---

## 4. Systems and tickets

### 4.1 From M1/M2 (already defined in the backlog)

`AB-001` … `AB-025` (see [`09_BACKLOG.md`](09_BACKLOG.md)) deliver the graybox VS: the full gameplay loop with graybox art and the 5 levels as graybox (AB-025).

### 4.2 VS ticket list (M3) — AB-026 … AB-045 (+ AB-158)

| ID | Title | Role | Size | Depends on | Acceptance (short) |
|---|---|---|---|---|---|
| AB-026 | Greenwood VS environment section v1 | ART | L | AB-004, AB-027 | Backdrop prefab + 2 variants; ≤ 40 k tris; ≤ 25 batches; reads well behind red/purple objects (5-second test) |
| AB-027 | Stylised lit shader + Greenwood palette + material swap | ART + ARCH | M | AB-002 | One Shader Graph lit shader, SRP-Batcher compatible; `T_Greenwood_Palette`; graybox → art swap via prefab variants only |
| AB-028 | Bow hero model v1 + BowView feedback | ART + CORE | M | AB-007 | Carved bow mesh ≤ 6 k tris; string stretch follows power; release snap anim; full-draw glow |
| AB-029 | VS prop art v1 (crates, posts, plank, crest, vase, rope) | ART | M | AB-013, AB-018, AB-020 | Art variants with identical colliders/mass (PrefabValidator green); 3 visual states max (§4) |
| AB-030 | Break VFX v1 + VfxService pooling | ART + PHYS | M | AB-012 | `VFX_Impact_Timber`, `VFX_Break_Timber`, `VFX_Objective_Cleared`, `VFX_Protected_Lost`, `VFX_RopeSnap`, `VFX_Splash_Water`, `VFX_Pit_Poof` (IDs per `05` §9.2); pooled; never covers an objective > 0.3 s |
| AB-031 | AudioService + SoundEvent library + VS SFX/music | ART | M | AB-003 | Mixer groups; 24 voices; ~16 SFX events + 1 Greenwood loop; per-event cooldown/pitch range |
| AB-032 | FeedbackDirector (GameEvents → SFX/VFX/haptics/hit-stop) | ART + CORE | M | AB-030, AB-031, AB-033, AB-034 | Mapping table data-driven; no gameplay code calls feedback directly |
| AB-033 | HitStop + CameraShake (reduced-motion aware) | CORE | S | AB-003 | 40–70 ms on meaningful direct impacts, max 1 per 0.3 s; physics outcome identical with/without (test) |
| AB-034 | HapticsService native bridge + editor stub | PLAT | M | AB-003 | iOS + Android implementations (D-032); Light/Medium/Success/Failure verified on device |
| AB-035 | UiTween utility | UI | S | AB-003 | Scale/fade/move/punch, easing set, unscaled time, zero GC per tween after warm-up |
| AB-036 | HUD polish (objective icons, quiver, next arrow, toasts) | UI | M | AB-023, AB-035 | Matches the §8 layout; cross-out anim on `ObjectiveCleared`; soft-lock toast; safe area on 9:16–9:21 |
| AB-037 | Win panel polished (star reveal, Next/Replay) | UI | M | AB-035, AB-031 | Stars sequence ≤ 1.6 s, skippable by tap; Next reachable in ≤ 1 tap at ≤ 0.6 s after the panel appears |
| AB-038 | Fail panel polished (reason, Retry dominant) | UI | S | AB-035 | Reason text by `FailReason`; Retry is the largest button; Retry → playable ≤ 1 s |
| AB-039 | Tutorial prompt system + L1 ghost hand + VS-03 callout | UI + CORE | M | AB-016, AB-035 | `TutorialPromptController`, `TutorialAnchor`, triggers/dismiss per `04` §7; prompts never overlap the shot line |
| AB-040 | VS playlist flow (`LC_VerticalSlice`) + slice-complete card | LEVEL + UI | S | AB-016, AB-022 | Boot → VS-01; Next advances; completion card shows per-level best stars; replay any |
| AB-041 | VS levels art pass + final tuning + re-recorded shots | LEVEL | M | AB-026, AB-029, M1 gate | All 5 levels at status **Final** (`04` §5); bot P-01..P-07 green with art prefabs |
| AB-042 | Analytics VS verification + CSV export + `level_quit` | MON | S | AB-024 | Every playtest session produces a CSV with session_id, attempt_number and full params; 0 missing/duplicate events in the QA script |
| AB-043 | Android VS device build + perf smoke | PLAT | M | AB-041 | Mid: avg ≥ 58 FPS, 1% low ≥ 45; Low: ≥ 30 FPS; restart p95 < 1 s; memory < 450 MB |
| AB-044 | VS regression checklist + QA pass + bug triage | QA | M | AB-043 | Checklist in `docs/qa/VS_REGRESSION.md`; 0 open P0/P1 bugs |
| AB-045 | VS playtest (5–10 players) + report + gate decision | QA + PO | M | AB-044 | Report in `docs/qa/VS_PLAYTEST_REPORT.md`; go/no-go entry added to `10_DECISION_LOG.md` |
| AB-158 | AssetImportRules (AssetPostprocessor) + placeholder build check (D-055, D-056) | ART + PLAT | S | AB-047 | Import presets applied automatically to new art/audio; ClosedTest/Release builds fail on any `Placeholder`-labelled dependency |

Critical path: AB-041 → AB-043 → AB-044 → AB-045. Art (AB-026/027/029) runs in parallel with feedback (AB-030–034) and UI (AB-035–039) from the M3 start.

---

## 5. Board conventions for the sketches

The play area is 10 m wide (x −5 … +5) at z = 0. Origin is at bottom-centre, the bow pivot is at (0, 1.5), and the HUD band is the top ~1.6 m. Sketches are not to scale vertically. `◎` crest target, `▣` timber crate, `║` timber post, `═` plank, `▓` static environment (`Env_*`), `|` rope, `⚱` royal vase (purple), `~` water/pit kill zone. **All angles and powers below are placeholders.** They are re-recorded with `ShotRecorder` after the M1 tuning gate (D-036) and live in `LevelData._intendedShots`.

---

## 6. The five vertical-slice levels

### VS-01 — W1_L01 "First Mark" (direct target hit)

```
 y 12 |                     ◎           |   Obj_CrestTarget (2.0, 10.6)
 y 10 |                    ▓▓▓          |   Env_Stump (timber look, static) top y = 10.0
      |                                 |
 y  2 |               (bow)             |
 y  0 |▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓|   Env_Ground
        -5               0              +5
```

| Field | Value |
|---|---|
| Objective | Break 1 crest target |
| Arrows / gold par | 3 Oak / **1** (3★ = 1 arrow, 2★ = 2, 1★ = 3) |
| Intended solution | One Oak at ≈ 64°, power ≈ 0.75. A direct hit shatters the crest. |
| Teaching | The draw gesture, the preview arc and the release. "Red crest = break it." |
| Likely failure modes | Pushes forward instead of pulling back → the ghost hand repeats. Tiny drag below `minFirePower` → cancels silently (a brief string "twang" + the preview flashes so it is not mysterious). Overshoots → the preview ring shows the impact point; the second try usually hits. |
| Tutorial copy | Ghost hand (no text callout). The level-start banner shows "Break the target!" (`lvl.banner.break_targets`). |
| Aim window | ≥ 12% (bot) |
| Success criteria | ≥ 95% of testers clear it; ≥ 90% fire the first shot ≤ 15 s after the level appears; median arrows used ≤ 2. |

### VS-02 — W1_L04 "Shaky Stilts" (weak-support collapse)

```
 y 13 |                  ◎A             |   crest A on top of tower
      |                  ▣              |
      |                  ▣              |   3 × Struct_Crate_Timber_1x1 (tower)
      |                  ▣              |
 y  8 |                ══════           |   Struct_Plank_Timber_4x0.25
      |                ║    ║           |   2 × Struct_Post_Timber_0.5x2 (stilts) – LEFT stilt is the weak point
 y  6 |▓▓▓▓▓▓▓   ◎B   ▓▓▓▓▓▓▓▓▓▓▓       |   lower-left ledge (crest B) | right ledge holding the stilts
      |                                 |
 y  2 |               (bow)             |
      |~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~|   Haz_Pit below the board
```

| Field | Value |
|---|---|
| Objective | Break 2 crest targets (A on the tower top, B on the lower-left ledge) |
| Arrows / gold par | 3 Oak / **1** |
| Intended solution | Hit the **left stilt** low (≈ 58°, power ≈ 0.62). The stilt kicks out, the plank tips left and the tower topples onto the lower ledge. Crest A breaks on landing (Low `breakImpulse`); a falling crate crushes crest B. |
| Alternative | Two direct hits (A then B) = 2 arrows = 2★. A hit on the right stilt tips the tower right into the pit: A cleared via pit, B remains → 2 arrows → 2★. |
| Likely failure modes | Aims straight at crest A (gets 2★ — fine, teaches that efficiency matters). Hits the tower's middle crate: the crate pops out but the tower stays → a "nearly" moment, the next shot is informed. The tower falls right (wrong stilt) → B remains. |
| Tutorial copy | None (discovery level; VS-G3 measures unaided discovery, so no hint system in the VS). The 2★ result itself ("1 arrow = 3★" par line on the Win panel, AB-037) is the nudge. |
| Aim window | ≥ 12% on the left stilt (hit band = the stilt's lower 1.2 m) |
| Success criteria | ≥ 70% of testers find the 1-arrow collapse within 3 attempts; ≥ 85% clear on the first attempt (any ★); first 3★ result in the slice. |

### VS-03 — W1_L07 "Crate Drop" (rope-cut drop)

```
 y 15 |▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓                |   Env_Beam overhead (static) x −3…+3
      |         |                       |   Prop_Rope (length 2.6 m), TutorialAnchor "rope" at its midpoint
 y 12 |       [▣▣]                      |   Struct_Crate_Timber_2x1 (heavy load)
      |                                 |
 y  7 |      ◎   ◎                      |   2 × crest, 1.2 m apart, directly under the crate
 y  6 |▓▓▓▓▓▓▓▓▓▓▓▓▓▓                   |   Env_Ledge x −3…+1
 y  2 |               (bow)             |
```

| Field | Value |
|---|---|
| Objective | Break 2 crest targets |
| Arrows / gold par | 2 Oak / **1** |
| Intended solution | Cut the rope (≈ 82°, power ≈ 0.80; the rope trigger radius is 0.15 m and the arrow passes through and continues). The 2×1 crate drops 4.5 m and lands across both crests (impact ≫ crest `breakImpulse`). |
| Alternative | Two direct hits = 2 arrows = 2★. No spare arrow, so a miss on the direct route means fail → this pushes players to try the rope. |
| Likely failure modes | Ignores the callout and shoots the crests (2★ or fail). Grazes past the rope (the preview ring shows the arc going past). Hits the crate instead: the crate swings (rope joint soft spring), wobbles and settles; the arrow embeds and nothing breaks → readable "not that". |
| Tutorial copy | **The VS's one callout:** "Aim for the rope" (`tut.w1_l07.rope`, 16 chars). Anchor `rope`. Trigger `OnLevelStart`, dismiss `DrawStarted` (`02` §11.1). Re-shown with `AfterMisses` (2). |
| Aim window | ≥ 12% (the rope is a vertical target, so the window is generous in angle) |
| Success criteria | ≥ 80% of testers cut the rope within 2 attempts; ≥ 60% get 3★; `object_triggered` (rope_cut) logged for ≥ 80% of sessions. |

### VS-04 — W1_L10 "Market Shelf" (protected-object precision, set piece)

```
 y 13 |   ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓            |   Env_Awning (stall roof) – blocks lobbed shots onto the crest
      |                                 |
 y 10 |   ⚱      ▣  ═══◎═══  ▣▲         |   vase (−3.6) | left stack (−0.8) | plank + crest | right stack (+2.4), ▲ = exposed top crate
      |   ▓▓     ▣            ▣         |   Env stone pedestal under the vase (static, decor-stone)
 y  8 |▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓       |   Env_Ledge (shelf) x −5…+3.4; open drop on the right
      |                             ~~~~|   Haz_Pit (right side, below y 4)
 y  2 |               (bow)             |
```

| Field | Value |
|---|---|
| Objective | Break 1 crest target. **Protect the royal vase.** |
| Arrows / gold par | 3 Oak / **1** |
| Intended solution | Strike the **exposed top crate of the right stack** from below-right (≈ 52°, power ≈ 0.70). It is knocked off the shelf to the right, the plank's right end drops, and the crest slides off into the pit (cleared). Everything moves away from the vase. |
| Alternative | Knocking the right stack's base also works but is less reliable (validated as a 1–2★ route). |
| Likely failure modes | Shoots the left stack → the plank tips left and the crest/crates slide toward the vase → "The vase broke!" (immediate fail, clear reason). Lobs at the crest → blocked by the awning (the arrow embeds in the awning; this teaches the shield read). Direct hit on the vase → fail (the purple outline warns). |
| Tutorial copy | No callout (one-callout budget). The level-start banner shows "Protect the vase!" (`lvl.banner.protect_vase`) with the purple vase icon. |
| Aim window | ≥ 12% on the exposed crate (hit band 1.0 m × 1.0 m) |
| Success criteria | ≥ 80% name the vase as "don't hit" in the 5-second test; ≥ 75% clear within 3 attempts; fail reason understood by ≥ 90% (post-session question). |

### VS-05 — W1_L16 "Ranger's Reckoning" (combined final test)

```
 y 16 |          ▓▓▓▓▓▓▓▓▓▓▓▓            |   Env_Beam overhead
 y 15 |                |          ◎C    |   Prop_Rope → Struct_Crate_Timber_2x1; crest C on a high perch (Env, x 4.0, y 14.5)
 y 13 |               [▣▣]        ▓▓    |
 y 11 |            ◎A                    |   tower: crest A on top
      |            ▣▣                    |
      |            ◎B                    |   crest B mid-tier on a plank
      |           ══════                 |
      |           ║    ║                 |   stilts (left stilt faces the vase side!)
 y  7 | ⚱▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓          |   vase on the far-left of the ledge (−4.2, 7.5)
      |                          ~~~~~~~~|   Haz_WaterPit to the right
 y  2 |               (bow)             |
```

| Field | Value |
|---|---|
| Objective | Break 3 crest targets. Protect the royal vase. |
| Arrows / gold par | 4 Oak / **2** |
| Intended solution | **Shot 1:** cut the rope (≈ 78°, p ≈ 0.85). The heavy crate lands on the right half of the tower top and the tower collapses **rightward** into the water, clearing A and B. **Shot 2:** a direct hit on crest C on the high perch (≈ 70°, p ≈ 0.95). Bullseye (M4+, not in VS): the rope. |
| Alternatives | Shoot the right stilt → the tower falls right (A, B cleared), then C → also 2 arrows (3★). Validated as an equal route (two viable, differently aimed solutions). Direct hits on A, B and C = 3 arrows = 2★. |
| Likely failure modes | Shoots the **left** stilt (familiar from VS-02) → the tower falls left onto the vase → fail. This deliberately tests reading over habit. Forgets crest C (out of the main cluster) → the HUD objective icons show 1 remaining. Weak draw on C (it needs near full power; the preview shows it). |
| Tutorial copy | None. |
| Aim window | ≥ 10% (the L16–20 band per `04` §13) |
| Success criteria | ≥ 70% clear within 4 attempts; ≥ 40% 3★ within the session; ≤ 20% of fails are "unclear why" in the post-session interview. |

---

## 7. VS acceptance criteria (all must be met before the playtest)

| # | Criterion | How measured |
|---|---|---|
| A-1 | All 5 levels at status Final; static validation 0 Errors; P-01…P-07 green | Test runner + `LevelValidator` report |
| A-2 | Preview/flight parity: max deviation ≤ 1 mm over 3 s of flight | `ArrowPreviewParityTests` |
| A-3 | Restart: editor ≤ 300 ms; Mid device p95 < 1.0 s (20 restarts) | `RestartPerfTests` + on-device log |
| A-4 | FPS: Mid device avg ≥ 58, 1% low ≥ 45 during VS-05 collapse; Low device ≥ 30 avg | Profiler / on-device FPS overlay (DevOverlay) |
| A-5 | GC: 0 B/frame during aim and flight (Mid build, deep profile off) | Profiler GC Alloc column |
| A-6 | Cold start → VS-01 playable ≤ 6 s on Mid | Stopwatch × 3 |
| A-7 | Zero Console errors/exceptions in a 10-minute scripted soak (play all 5 levels twice, restart spam, pause/resume, app background/foreground) | `read_console` + device logcat |
| A-8 | Every listed SFX event plays and is audible on device speakers; haptics fire on Android (and iOS if available) and can be toggled off | QA checklist |
| A-9 | VFX never covers a remaining objective for > 0.3 s | QA visual check on all 5 levels |
| A-10 | Analytics: for a scripted run (win, fail, restart, quit) the CSV contains exactly the expected events with correct params | AB-042 script diff |
| A-11 | HUD/safe area correct at 9:16, 9:19.5, 9:21 (and 3:4 pillarbox if tested) | Screenshots in `docs/qa/` |
| A-12 | No original-asset violations: every asset is custom or listed in `docs/art/ASSET_LICENSES.md` | ART check |

---

## 8. VS gate — playtest protocol

**Participants:** 5–10 casual mobile players (not team members, not game developers). At least 3 play mobile puzzle games weekly; a mix of ages 16–55; at least 3 have never seen the game. Remote sessions via screen-share are allowed if a physical device is used.

**Device:** Android Mid-tier phone (primary), Low-tier phone for 2 sessions. Volume on, haptics on. DevOverlay hidden. Fresh install (save cleared).

**Script (≈ 20 min):**
1. (1 min) Consent to observe and record notes. "We are testing the game, not you. Please think out loud."
2. (0 min) Hand over the phone on the splash screen. **No instructions.** Do not explain the bow.
3. (≈ 12 min) Free play VS-01 → VS-05. The observer helps only if stuck > 2 min on one level ("assist", logged with the level and the hint given).
4. (2 min) After the "Slice complete" card, say "Feel free to keep playing or stop whenever you like." Note voluntary replays (count and level).
5. (2 min) 5-second test: show screenshots of VS-02 and VS-04 for 5 s each. Ask "What do you need to do? What must you avoid? Where would you shoot?"
6. (3 min) Questionnaire: fun (1–7), clarity of goals (1–7), bow control feel (1–7), "Did anything feel unfair or random? When?", "Best moment?", "Would you play more levels?" (Y/N), hardest level.

**Observer log (per level):** time to first shot, attempts, arrows per attempt, stars, assists, verbal reactions (coded: *aha / laugh / frustration / confusion*), first-attempt strategy (direct vs. trick).

**Telemetry:** the analytics CSV per session (AB-042) is attached to the report.

### 8.1 Go / no-go thresholds

| ID | Metric | Go threshold |
|---|---|---|
| VS-G1 | First shot fired ≤ 15 s after VS-01 appears, no help | ≥ 90% of testers |
| VS-G2 | Complete all 5 levels with ≤ 1 assist in total | ≥ 80% |
| VS-G3 | Discover the trick (3★ or verbalised) on VS-02 **or** VS-03 within 2 attempts | ≥ 70% |
| VS-G4 | Mean fun / clarity / bow feel | ≥ 5.0 / ≥ 5.5 / ≥ 5.0 (of 7) |
| VS-G5 | Voluntary replay for more stars | ≥ 50% replay at least one level |
| VS-G6 | "Unfair/random physics" complaints | ≤ 1 tester reports it more than once |
| VS-G7 | 5-second test identifies the vase as protected | ≥ 80% |
| VS-G8 | Median attempts per level | ≤ 3 on every level |

**Decision rules:**
- All of VS-G1…VS-G8 met → **GO**. Proceed to M4. Log a decision entry.
- 1–2 misses outside VS-G4 → **GO with fixes**. Fix tickets are added to M4; no retest needed.
- VS-G4 missed, or ≥ 3 misses → **NO-GO**. A one-week iteration on feel/readability: revisit D-036 (flight time), D-018 (draw mapping), D-040 (preview end-point), the hit-stop strength and level clarity. Then retest with 5 new players.
- A second NO-GO → `PO` escalates to `OWNER` for a concept review (draw scheme alternatives, camera, physics fallback per D-004) before any further content work.

The report goes in `docs/qa/VS_PLAYTEST_REPORT.md`: per-tester table, metric summary, top 5 issues with ticket IDs, and a recommendation.

---

## 9. VS asset list

| Category | Asset | Spec / notes | Owner |
|---|---|---|---|
| Environment | `Env_GW_Backdrop_TrainingGrounds` backdrop prefab (+2 variants) | Grass valley, ruined stone arch, archery butts, fence, 2 tree clusters, pre-blurred hill cards (`T_GW_BG_*`), sky (`T_GW_Sky`); ≤ 40 k tris; `T_GW_Palette` | ART |
| Environment | Greenwood art variants `Env_GW_Ground_12x1`, `Env_GW_Ledge_4x0.5`, `Env_GW_Ledge_6x0.5`, `Env_GW_Beam_6x0.4`, `Env_GW_Stump`, `Env_GW_Awning_6`, `Env_GW_Perch_1.5` of the `04` §15 blocking set (D-052) | Static blocking with art; layer Environment; colliders unchanged from the graybox pieces | ART + LEVEL |
| Structures | `Struct_Crate_Timber_1x1`, `Struct_Crate_Timber_2x1`, `Struct_Post_Timber_0.5x2`, `Struct_Plank_Timber_4x0.25` | Art variants; 3 states (intact / cracked / broken → debris) | ART + PHYS |
| Objective | `Obj_CrestTarget` | Red crest with an original emblem (crossed arrows over a leaf), strong outline, shatter state | ART |
| Protected | `Prot_RoyalVase` | Purple-glazed vase, gold rim, purple outline + shield icon | ART |
| Prop | `Prop_Rope` | Rope LineRenderer material + an anchor ring mesh | ART |
| Hazard | `Haz_Pit`, `Haz_WaterPit` | Pit: dark void edge + purple marker stones; water: stylised water strip + purple edge posts (`05` §4.5) | ART |
| Bow / arrow | `SM_Bow_OakRanger` + `AC_Bow` (hero bow, default skin `CD_Bow_OakRanger`), `Arrow_Oak` | Bow 4–6 k tris, string as a LineRenderer; arrow ≤ 250 tris + `VFX_Trail_Default` | ART |
| Debris | `Debris_Timber_01…03`, `Debris_Crest_01…02`, `Debris_Vase_01…02` | 20–80 tris each, pooled (`05` §4.2) | ART + PHYS |
| VFX | `VFX_Bow_FullDrawGlow`, `VFX_Bow_Release`, `VFX_Trail_Default`, `VFX_Impact_Timber`, `VFX_Break_Timber`, `VFX_Objective_Cleared`, `VFX_Protected_Lost`, `VFX_RopeSnap`, `VFX_Splash_Water`, `VFX_Pit_Poof`, `VFX_UI_StarBurst` | Particle budgets per `05` §9.2; mobile shaders | ART |
| SFX | Draw creak (loop by power), full-draw tick, release snap, arrow whistle (pitch by speed), timber impact ×3, timber break ×2, arrow embed thunk ×2, crest shatter, rope snap, vase break, splash, win sting, fail sting, star pop ×3 (rising pitch), UI tap, objective cleared chime | Original or licensed + processed (D-042); `SE_*` events | ART |
| Music | `MUS_GW_Loop` + `MUS_Sting_Win`, `MUS_Sting_Fail` | 60–120 s seamless loop (`05` §11), light and adventurous; ducked during the win sting | ART |
| UI | HUD icons (pause, restart, crest, vase, Oak arrow), star (empty/full), panel frames (original wood-and-leaf style), buttons, toast frame, ghost hand sprite, callout bubble | Original UI language (no purple frames; §11) | UI + ART |
| Fonts | 1 display + 1 body font | OFL or licensed (D-042) | UI |
| Strings | `UIStrings` entries: banners, tutorial callout, fail reasons, win tips, buttons | English, ≤ 32 chars for callouts | UI + LEVEL |
