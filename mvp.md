# ArrowBuster — MVP Game Design Document

**Working title:** ArrowBuster  
**Genre:** Casual physics puzzle / skill-based mobile game  
**Platform:** iOS and Android, portrait-first  
**Audience:** Broad casual players, 13+, short-session puzzle fans  
**MVP target:** A polished, ad-ready vertical slice with 60 hand-authored levels across 3 worlds

---

## 1. Product Summary

### One-line pitch
Pull back a bow, find the weak point, and use a limited quiver of arrows to collapse, cut, pop, and clear clever obstacle structures.

### Core fantasy
The player is a precision fantasy archer solving compact destruction puzzles. A satisfying shot may snap a rope, topple a crate tower, burst a lantern, release a boulder, or trigger a cascading chain reaction that clears the board with one arrow.

### Product thesis
ArrowBuster takes the instantly understandable **aim → release → watch physics resolve** loop and gives it a distinct archer identity. The bow creates a more tactile input than a cannon, while arrows allow richer interactions than simple impact: piercing, pinning, cutting, popping, igniting, and triggering. The game should be simple enough to understand in three seconds and strategic enough to replay for a three-star clear.

### Reference boundary
The visual reference supplied shows a bright, portrait puzzle game based on limited shots, destructible structures, and escalating level variety. Knock Fever’s store listing similarly frames its game around precision shots, physics-driven collapses, chain reactions, and many short levels. ArrowBuster should use only that broad genre pattern—not its art, UI, assets, copy, structure designs, name, branding, or level layouts. Its differentiator is the bow-and-arrow interaction system and an original fantasy-adventure presentation. [web:4]

---

## 2. Player Experience

### Session loop
1. Player sees a compact scene and the objective.
2. Player reads the weak point: rope, support, explosive pot, target, balloon, or unstable stack.
3. Player aims by dragging the bow string, previews an arc, and releases.
4. Arrow hits; physics and chain reactions resolve.
5. Player clears all required objectives before their arrows run out.
6. Player earns stars, coins, and optional bonus rewards; the next level is one tap away.

### Emotional beats
- **Curiosity:** “What is the intended trick?”
- **Tension:** Few arrows remain.
- **Impact:** A crisp release, arrow whistle, impact, and short hit-stop.
- **Payoff:** A whole structure cascades down from a smart shot.
- **Mastery:** Replay in fewer arrows to earn three stars.

### Design pillars
- **One-thumb clarity:** No complex HUD, no aiming tutorial wall.
- **Fair physics:** An object’s reaction must look and feel plausible; avoid secretly weighted or sticky pieces.
- **Readable puzzles:** Players can identify key materials and weak points before firing.
- **Fast recovery:** Fail/restart in one tap; keep level loads under a second.
- **Satisfying spectacle:** Make every impact feel substantial without making the simulation confusing.

---

## 3. Core Mechanics

### Aim and fire
- The bow sits fixed at the bottom-center of the portrait screen.
- Player touches the bow/string area and drags backward to set direction and draw strength.
- Release fires one arrow along a ballistic path.
- A dotted trajectory appears only while drawing. The first 15 levels use a generous, accurate preview; later levels may reduce its length but never remove it entirely.
- Arrow speed and gravity are tuned for a clean 1.5–2.5 second flight at normal range.
- The player can aim horizontally and vertically within a camera-safe firing cone; no camera movement during the first MVP worlds.

### Arrow behavior
- Standard arrows deal impulse damage, can lodge in soft surfaces, and may nudge or topple dynamic objects.
- A lodged arrow should visibly vibrate for 0.2 seconds, then remain in the world until the level settles or it is removed by physics.
- At most 8 active arrows are simulated; older embedded arrows can become static decorations to protect performance.
- An arrow that misses leaves the play field after a short boundary distance and counts as used.

### Win and fail
**Win:** All mandatory objectives are cleared and the world has settled for 0.75 seconds.

**Fail:** The quiver is empty and one or more mandatory objectives remain; or a protected object is broken/falls into a kill zone.

**Soft-lock protection:** If no dynamic object moves for 2 seconds after a shot, show “1 arrow left” or “Out of arrows,” as applicable. Never make players wait through a long settle state.

### Scoring and stars
- Every level has a three-star par based on arrows used.
- **3 stars:** Clear at or below the gold arrow par.
- **2 stars:** Clear with one additional arrow.
- **1 star:** Clear with any remaining valid clear.
- Optional “Bullseye” medal: hit a marked weak-point target with a direct arrow. It is cosmetic/meta progression only in the MVP.
- Do not score based on raw physics damage. It makes success opaque; arrow efficiency is clear and controllable.

### Difficulty levers
- Fewer starting arrows.
- More stable, wider, or heavier structures.
- Targets positioned behind blockers.
- Moving or timed hazards.
- Multiple required objectives.
- More than one viable—but differently efficient—solution.
- Environmental forces, introduced one at a time.

---

## 4. Objects and Rules

### Required objectives
| Object | Player interaction | Clear condition | Purpose |
|---|---|---|---|
| Red crest target | Direct arrow hit | Break all required targets | Teaches precision |
| Supply crate | Impact / falling debris | Destroy or push below line | Core knockdown object |
| Hanging lantern | Direct hit or falling debris | Break it | Can be used as a weak point |
| Cursed orb | Direct hit only | Shatter it | Prevents random brute-force clears |
| Enemy training dummy | Knock over or direct hit | Touch ground / break | Adds character and comedy |
| Banner rope | Arrow cut | Drop attached load | Introduces trigger puzzles |

### Structural materials
| Material | Visual language | Physics behavior | Arrow response |
|---|---|---|---|
| Straw / wicker | Pale woven yellow | Light; low stability | Easy pierce and push |
| Timber | Warm brown beams | Medium mass; breaks after repeated stress | Lodges or fractures |
| Stone | Gray-blue blocks | Heavy; strong; dangerous when falling | Bounces or chips |
| Ice crystal | Bright cyan translucent | Medium; slides easily; brittle | Shatters into harmless fragments |
| Metal | Dark steel with gold trims | Heavy and nearly unbreakable | Ricochets at shallow angles |

### Interactive props
| Prop | Rule | Strategic use |
|---|---|---|
| Rope / chain | Cut with any arrow | Release bridges, crates, counterweights |
| Balloon cluster | Pops on arrow or impact | Lifts a small object; removing it drops the load |
| Oil jar | Breaks on strong hit; creates brief fire zone | Burns ropes and straw after delay |
| Powder barrel | Breaks and produces a controlled radial blast | Clears dense stacks; cannot destroy protected objects nearby |
| Rolling boulder | Released by a cut or impact | Sweeps a lane and creates domino effects |
| Spring plate | Activates when hit or weighted | Launches an object upward/sideways |
| Wind fan | Always on in later worlds | Curves light arrows and pushes balloons |
| Rotating shield | Moves at a predictable speed | Timing puzzle; blocks direct lines |
| Portal ring | Teleports a fired arrow to paired exit | Signature late-MVP puzzle object |

### Protected objects and hazards
- **Royal vase:** Must not break; failing it immediately ends the level.
- **Sleeping fox:** Must not be hit or knocked off its ledge; friendly character hazard.
- **Water pit:** Objects below the line are removed; can be an objective clear route or an accidental loss route.
- **Spike bed:** Destroys falling objects but also removes arrows; primarily a visual/strategy hazard.
- **Shielded target:** Only vulnerable after its cover is displaced.

### MVP safety and readability rules
- No object needs more than three distinct visual states.
- Red = required target; green/gold = helpful interactive element; gray/blue = structure; purple = hazard/protected-state indicator.
- Every newly introduced object receives two tutorial levels: one obvious, one combined with a previous mechanic.
- Use particles only after the key outcome has become readable. Avoid covering a target with explosion effects.

---

## 5. Arrow Types and Unlocks

### MVP arrow inventory
The base arrow is available forever. Special arrows unlock by world completion and are limited per level by the level designer, not by a consumable economy.

| Arrow | Unlock | Effect | MVP role |
|---|---|---|---|
| Oak Arrow | Start | Standard impact / embed | Baseline skill |
| Heavyhead Arrow | World 1, level 15 | High impulse; low travel speed | Break supports and push stone |
| Split Arrow | World 2, level 8 | Splits into three short-range arrows after first impact or timed trigger | Broad target coverage |
| Fire Arrow | World 2, level 18 | Ignites rope, straw, and oil jars | Delayed chain reaction |
| Bounce Arrow | World 3, level 10 | Predictable single ricochet from metal | Bank-shot puzzles |
| Drill Arrow | Post-MVP reserve | Pierces one timber layer | Do not implement for launch |

### Loadout rule
- Most early levels give only Oak Arrows.
- A special-arrow level starts with a fixed, curated quiver—for example, `2 Oak + 1 Heavyhead`.
- The game does not let players choose a loadout in the MVP. This maintains level authorship and prevents complexity.
- From level 31 onward, occasional “choice levels” show two special-arrow slots; the player taps which of two tools to include. This is optional if scope becomes tight.

### Cosmetic unlocks
- Bow skins: Oak Ranger, Moonwood, Ember, Frostglass, Royal Violet.
- Arrow trails: gold spark, leaf swirl, cyan streak, ember ash.
- Quiver badges: completion rewards only.
- Cosmetics must not change arrow physics, aim assist, damage, or win chance.

---

## 6. Worlds and Maps

### World map format
Use a vertical scrollable path of 20 circular level nodes per world. A three-node chapter cluster sits on a themed illustrated backdrop. The current world is open; the next unlocks at 15/20 completions, while 3-star performance is optional.

| World | Levels | Theme | New mechanic | Visual identity |
|---|---:|---|---|---|
| 1. Greenwood Range | 1–20 | Ranger training valley | Timber, ropes, crates, targets | Lush grass, ruins, bright daytime |
| 2. Sunscar Canyon | 21–40 | Desert caravan outpost | Balloons, oil, fire, moving shields | Orange stone, wind, sunset |
| 3. Frostspire Keep | 41–60 | Icy mountain fortress | Ice, ricochets, portals, wind | Blue ice, aurora, snow |

### Map 1: Greenwood Range
**Purpose:** Teach the fantasy and the core collapse loop.

- Levels 1–3: Direct target hits; wide, forgiving aim lines.
- Levels 4–6: Wooden crate towers; discover low-support hits.
- Levels 7–9: Ropes releasing hanging loads.
- Levels 10–12: Protected vase and “do not hit” rules.
- Levels 13–15: Heavyhead Arrow introduction.
- Levels 16–19: Two-step cascades—cut rope, then topple target.
- Level 20 boss puzzle: Three stacked watchtowers, one powder barrel, one protected fox. Clear using 4 arrows or fewer.

### Map 2: Sunscar Canyon
**Purpose:** Turn direct aiming into sequence planning.

- Levels 21–23: Balloon-supported crates and drop timing.
- Levels 24–26: Oil jars and delayed burning ropes.
- Levels 27–29: Moving shields with clearly telegraphed loops.
- Levels 30–32: Split Arrow introduction; hit several targets behind a central obstacle.
- Levels 33–35: Counterweights and boulder lanes.
- Levels 36–39: Combine fire + balloons + protected objects.
- Level 40 boss puzzle: Caravan siege gate. Trigger a boulder, ignite the support rope, then clear four target crests in 5 arrows.

### Map 3: Frostspire Keep
**Purpose:** Deliver the “clever shot” climax with controlled advanced systems.

- Levels 41–43: Ice blocks that slide and shatter.
- Levels 44–46: Wind gust lanes affecting light arrows.
- Levels 47–49: Bounce Arrow introduction using metal shields.
- Levels 50–52: Portal pairs; teach visible entry-to-exit trajectory.
- Levels 53–55: Moving ice platforms and low-risk timing.
- Levels 56–59: Multi-system puzzles with alternative solutions.
- Level 60 finale: Break the frost seal on a fortress tower by using a bank shot through a portal, then collapse the structure without hitting a royal relic.

### Level design cadence
- New mechanic: two tutorial levels.
- One consolidation level.
- One twist/combo level.
- Every fifth level: a larger “set-piece” puzzle with cinematic framing, but no boss health bars.
- Avoid repeating a visual layout or solution pattern within 10 levels. Long-term repetition is a stated risk in this genre; the MVP should prove the content pipeline before scaling. [web:4]

---

## 7. Detailed Level Templates

These templates are production-ready patterns, not copied layouts.

| Template | Quiver | Setup | Intended solution | Skill taught |
|---|---|---|---|---|
| Weak Leg | 3 Oak | Target sits atop a three-crate tower | Hit lowest outer crate; tower tilts into target | Structural reading |
| Hanging Trouble | 2 Oak | Two targets under a suspended crate | Cut rope; crate clears both | Trigger over brute force |
| Safe Cargo | 3 Oak | Crates beside protected vase | Strike upper support away from vase | Precision and restraint |
| Boulder Run | 2 Oak + Heavyhead | Boulder held by timber peg | Heavyhead breaks peg; boulder sweeps targets | Tool selection |
| Sky Drop | 3 Oak | Crate lifted by balloon cluster | Pop balloons in correct order; avoid protected item | Order of operations |
| Burning Bridge | 1 Oak + Fire | Rope behind a shield | Fire arrow into straw wick; rope burns; load falls | Delayed result |
| Mirror Gate | 2 Oak + Bounce | Targets behind metal shield | Bank one arrow off marked plate | Geometry |
| Rift Shot | 2 Oak | Portal exit above weak beam | Fire through portal; arrow exits downward | Spatial planning |

### Star par examples
- Tutorial: 3 arrows available, gold par = 1.
- Normal puzzle: 4 arrows available, gold par = 2.
- Set piece: 5 arrows available, gold par = 3.
- Never require a pixel-perfect shot for three stars; intended aim windows should be approximately 8–12% of screen width at target distance.

---

## 8. Screens and UX

### Gameplay HUD
- Top-left: current level and pause button.
- Top-center: mandatory objective icons; crossed out when cleared.
- Top-right: arrows remaining as clear arrow icons, plus a restart button.
- Bottom: bow, draw string, and no clutter.
- Before firing, use a small “Aim for the rope” callout only in designated teaching levels.

### Core screens
1. **Splash / loading:** brief branded bow draw and arrow impact.
2. **Home:** large `PLAY` button, current-world card, shop/cosmetics, settings.
3. **World map:** level nodes, stars, locked-world teaser.
4. **Gameplay:** portrait scene and minimal HUD.
5. **Win:** stars animate, coin reward, `Next`, `Replay`, and optional rewarded bonus.
6. **Fail:** quick message, `Retry` as the dominant action, optional rewarded extra arrow.
7. **Bow forge / cosmetics:** collection grid; no confusing upgrade stats.

### Controls and accessibility
- One-handed input; no required multitouch.
- Haptics: light on draw threshold, medium on impact, success pattern on clear; toggle in settings.
- Settings: music, SFX, haptics, reduced particles, color-assist outlines, restore purchases, privacy.
- Color cannot be the only objective signal: targets also use crest shape/icon and strong outline.

---

## 9. Economy and Monetization

### MVP currency
**Coins** are earned on level completion, daily quest, and optional rewarded videos. Coins buy cosmetics only. Do not sell power that invalidates puzzle fairness.

### Ethical monetization structure
| Placement | Offer | Rule |
|---|---|---|
| Failed level | Rewarded video for +1 arrow | Max one offer per attempt; only if one objective remains or a viable state is detected |
| Win screen | Rewarded video for 2× coins | Optional; never blocks Next |
| Home/shop | Interstitial | At most after every 3 completed levels, never after a fail, never in first 10 minutes |
| IAP | Remove Ads | Removes interstitials; rewarded ads remain opt-in |
| IAP | Cosmetic starter pack | Bow skin + trail + coins, no power |

### Post-MVP monetization candidates
- Seasonal cosmetic path with optional premium track.
- Weekly challenge map with fixed arrows and a global score leaderboard.
- Limited themed bow skins tied to world expansions.

### Avoid
- Selling unlimited special arrows.
- Requiring ad views to continue the first session.
- Fake progress gates, forced tutorial ads, or 30-second interstitials after every level.
- A hard currency that is necessary to retry standard levels.

---

## 10. Retention and Content

### First-session target
- Shot 1 within 15 seconds of installing.
- First chain reaction by level 3.
- First three-star result by level 4.
- Special-arrow reveal by level 15 at the latest.
- World 1 finale reachable in approximately 20–30 minutes of casual play.

### MVP retention hooks
- Three-star completion is optional perfection, not a blocker.
- Daily target: complete three selected old levels with a fixed “challenge quiver.”
- World-completion bow skin unlock.
- “Bullseye” medals form a visible collection, encouraging replay.

### Post-MVP content plan
- Ship 20-level worlds every 3–4 weeks only after analytics confirm healthy completion and replay rates.
- Introduce one new system per world, not a pile of mechanics.
- Add a weekly one-screen puzzle after at least 100 base levels exist.
- Build a lightweight internal level editor using reusable prefabs, material profiles, objective rules, and star par values.

---

## 11. Art and Audio Direction

### Visual direction
- Original stylized 3D, toy-diorama look: bold shapes, polished materials, saturated but controlled color palette.
- Camera: fixed 3/4 view with a shallow depth-of-field only on non-interactive background elements.
- The bow is a hero prop: carved wood, gold inlays, magical string glow at full draw.
- Objects must remain visually legible against the background; use clean silhouettes and limited texture noise.
- Build original asset kits and UI language. Do not imitate the supplied game’s cannon, purple frame, wordmarks, can/tower assets, exact map presentation, or screenshot composition.

### Impact feedback
- Bow string stretch + release snap.
- Arrow whistle rises with speed.
- 40–70 ms hit-stop on meaningful direct impacts.
- Material-specific sounds: wood crack, stone thunk, ice chime/shatter, rope twang, metal ping.
- Chain reaction uses escalating percussion, then a short success sting.
- Physics debris disappears cleanly after resolution; do not leave a cluttered board.

---

## 12. Technical MVP Specification

### Recommended implementation
- **Engine:** Unity 6 + URP for quickest mobile physics/polish workflow, or Godot 4 if the team is strongly Godot-native.
- **Physics:** 3D rigidbodies constrained to a mostly 2.5D play plane. This gives convincing collapses while keeping camera, aiming, and performance controllable.
- **Orientation:** Portrait, 9:16 reference; adapt UI for taller devices.
- **Target devices:** iPhone 11 / comparable Android at 60 FPS; low-end fallback at 30 FPS with reduced debris and particles.
- **Backend:** None required for v1 except analytics, remote config, crash reporting, and IAP/ad SDKs.
- **Save data:** Local level progress, stars, cosmetic inventory, settings; cloud save can be post-MVP.

### Systems checklist
- Bow input and trajectory prediction.
- Arrow projectile pooling.
- Material-based collision response and breakable prefabs.
- Objective manager and clear/fail evaluator.
- Level configuration data (arrows, stars, objects, tutorial prompt).
- World-map progression.
- Cosmetic inventory/equip system.
- Ads, IAP, analytics, consent, and settings.
- Audio/haptics manager.
- Internal level editor / inspector tooling.

### Physics tuning targets
| Parameter | Target |
|---|---|
| Arrow travel time | 1.5–2.5 seconds across normal board |
| Level completion duration | 20–60 seconds on first successful try |
| Settle timeout | 2 seconds without meaningful motion |
| Debris cap | 40 dynamic fragments maximum |
| Active arrow cap | 8, then freeze/despawn old arrows |
| Restart time | Under 1 second |
| First app interaction | Under 15 seconds |

---

## 13. Analytics Events

Track events with `world_id`, `level_id`, `attempt_number`, `arrow_type`, `arrows_start`, `arrows_used`, `result`, and `session_id` where relevant.

| Event | When | Decision enabled |
|---|---|---|
| `level_started` | Gameplay loads | Funnel entry |
| `arrow_fired` | Arrow released | Aim behavior / tool use |
| `object_triggered` | Rope cut, barrel lit, portal entered, etc. | Puzzle comprehension |
| `level_completed` | Clear resolves | Completion and stars |
| `level_failed` | Quiver empty/protected failure | Difficulty diagnosis |
| `level_restarted` | Retry tapped | Friction / rage-retry signal |
| `rewarded_offer_shown` | Offer visible | Monetization exposure |
| `rewarded_offer_accepted` | Player opts in | Offer relevance |
| `world_unlocked` | New world opens | Progression pacing |
| `cosmetic_equipped` | Skin/trail selected | Cosmetic demand |
| `iap_completed` | Purchase verified | Revenue tracking |

### KPI targets for soft launch
- Level 1 completion: 95%+.
- World 1 completion: 45%+ among installers who reach level 5.
- Median retries per normal level: 1–3.
- Level abandonment spike: investigate any level with 20%+ quit rate before first shot or 8+ median retries.
- Rewarded ad acceptance: optimize only after core completion is healthy.
- D1 and D7 targets should be benchmarked against the actual acquisition channel and CPI during soft launch; do not optimize ad density before verifying core fun.

---

## 14. Production Scope

### Must ship (MVP)
- 60 levels, 3 worlds, 5 implemented arrow types including base.
- Six required-object types, five materials, and at least eight interactive props.
- Three polished environment themes.
- World map, stars, coin cosmetics, basic daily challenge.
- Rewarded extra-arrow and rewarded double-coin placements.
- Remove-ads and cosmetic-starter IAP.
- Analytics, crash reporting, consent/privacy flow, sound, haptics, accessibility toggles.

### Explicitly not MVP
- PvP, guilds, chat, user-generated levels.
- Procedural levels presented as handcrafted content.
- Real-time multiplayer.
- Complex character progression, gear stat upgrades, energy timers.
- Narrative cutscenes or dialogue trees.
- More than three worlds or five special arrow types.

### Suggested 10-week build plan
| Week | Deliverable |
|---:|---|
| 1 | Graybox bow input, ballistic arrow, camera, one breakable tower |
| 2 | Objective/fail/win loop, restart, 10 graybox levels |
| 3 | Materials, ropes, targets, protected objects, level data format |
| 4 | Heavyhead, balloons, powder barrels; first 20 levels playable |
| 5 | Art direction lock, Greenwood final art, VFX/SFX/haptics pass |
| 6 | Fire, split arrow, Sunscar content to 40 levels |
| 7 | Ice, bounce, portal, Frostspire content to 60 levels |
| 8 | World map, cosmetics, economy, ads/IAP and analytics integration |
| 9 | Device optimization, balance, QA, accessibility and consent work |
| 10 | Closed test, funnel fixes, store assets, submission candidate build |

---

## 15. Acceptance Criteria

The MVP is ready for a closed test when:

- A new user can understand aim-and-release without reading instructions.
- Every one of 60 levels has an intended solution, a clear star par, and a reproducible pass on target devices.
- Physics outcomes are deterministic enough that the intended solution succeeds reliably; controlled randomness must never turn a good shot into an unexplained fail.
- At least 80% of playtesters can complete World 1 without external help.
- No forced ad interrupts the first 10 minutes; standard retries are always free.
- All objects have readable silhouettes, material sounds, and appropriate destruction/interaction feedback.
- The game uses wholly original art, naming, UI, audio, copy, and level layouts.

---

## 16. Immediate Next Actions

1. Build the graybox prototype using only Oak Arrows, crates, targets, ropes, and gravity.
2. Test 15 levels with 5–10 casual mobile players; observe whether they discover weak points without hints.
3. Lock arrow feel and fairness before producing final art or monetization.
4. Create a reusable level-prefab library rather than hand-coding scenes.
5. Finish World 1 to a polished standard, validate session retention, then scale Worlds 2–3.

**North-star experience:** “I saw the trick, made one clean bow shot, and the whole scene came down exactly as I hoped.”
