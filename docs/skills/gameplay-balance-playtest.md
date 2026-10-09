# Skill: Gameplay Balance and Playtest

**Primary roles:** PO (owner), LEVEL, QA (session running), CORE (feel tuning), MON (only after core KPIs are healthy)

## Purpose
Run structured playtests and balance passes that answer the product questions:
- Is aim-and-release understood without instructions?
- Do players find weak points?
- Does the collapse feel great?
- Are quivers/pars fair?
- Do players want to replay for 3★?

Then turn observations into specific tuning (`GameplayTuning`, `AD_*`, `MP_*`, `LevelData.quiver`/`goldPar`/`trajectoryPreviewScale`) or level rework. The targets are mvp §10 (first shot < 15 s, first chain reaction by L3, first 3★ by L4, W1 finale in ~20–30 min) and §15 (≥ 80% complete W1 without help).

## When to invoke
- **M1 feel gate — G-M1** (AB-014): "Spec" vs "Snappy" flight presets (D-036), draw mapping (D-018), preview behaviour (D-085).
- **M3 Vertical Slice & Feel Lock gate — G0:** 5–10 **external** casual players on VS-01..05, before any special arrows, maps, cosmetics, ads or the 60 levels (D-102); go/no-go against thresholds VS-G1…VS-G8 and the decision rules in `11` §8.1 (AB-045).
- **M5** internal playtests of the 60 graybox levels + device validation (G2); **M8** full balance pass of all 60; the UK closed test (M9); post-soft-launch tuning only after Canada/Australia data (M10+, D-102 step 12).

**Do NOT invoke** to validate solvability mechanically (use the solvability bot in [`unity-physics-validation.md`](unity-physics-validation.md)) or for monetisation tuning before completion KPIs are healthy.

## Inputs
- A device build (Android-Dev or TestFlight) with the DevOverlay hidden but `DebugAnalyticsService` CSV logging ON.
- The level set + intended-solution docs (`docs/levels/W*_L*.md`).
- The playtest script (below), consent form for testers, an observation sheet.
- Prior funnel/level metrics if they exist ([`analytics-and-funnel-review.md`](analytics-and-funnel-review.md)).

## Step-by-step workflow

### A. Plan
1. **Hypotheses** (≤ 5), e.g.:
   - "≥ 4/5 players fire within 15 s with no instruction";
   - "≥ 3/5 cut the rope on VS-03 within 2 attempts";
   - "Snappy flight is preferred and more accurate than Spec".
2. **Testers:** 5–10 casual mobile players who are not developers, a mix of ages 13+; ≥ 2 who rarely play puzzle games. Their own phones where possible, plus one Low Android.
3. **Build variants:** for A/B (M1), alternate the preset order between testers (A-first / B-first).

### B. Run (per tester, 20–30 min)
1. **Script:** "Play as you would at home. Please think aloud. I can't help you — I'll only answer after." Give no instructions about the bow.
2. Observe silently and note:
   - time to the first touch and the first shot;
   - first-attempt aim behaviour (pull back vs push forward);
   - per level: attempts, verbal confusion ("what do I hit?"), the moment of insight, reactions to the collapse (smile/"oh!"), and reactions to a protected-object fail (fair vs unfair);
   - whether they replay for stars unprompted.
3. **Post-session interview** (5 min):
   - What was the goal?
   - Favourite moment?
   - Anything unfair or confusing?
   - Would you play again tomorrow?
   - 1–5 ratings for: aiming feel, fairness, satisfaction, clarity.
4. Collect the CSV (`analytics_debug.csv`) from the device: `adb pull /sdcard/Android/data/com.attila.arrowbuster/files/analytics_debug.csv`; on iOS via the Files app/Xcode container.

### C. Analyse
1. **Per-level table:**
   - attempts to clear;
   - arrows used vs par;
   - stars;
   - quit-before-first-shot;
   - median time to first shot;
   - fail reasons;
   - insight observed (Y/N).
2. **Flags:**
   - median attempts ≥ 4 on a tutorial level, or ≥ 8 on any level → too hard;
   - 3★ on the first try by > 80% of testers on a non-tutorial level → too easy;
   - < 50% find the intended trick → readability problem;
   - "unfair" mentioned by ≥ 2 testers → physics/readability bug;
   - repetition complaints → archetype variety (04 tracker).
3. **Decide the fix type**, in this order of preference:
   1. readability (colour/outline/silhouette/callout);
   2. layout (move/resize);
   3. quiver/par;
   4. global tuning (affects all levels — re-record intended shots afterwards!).
4. **Global tuning changes** (`GameplayTuning`, `AD_*`, `MP_*`) need CORE/PHYS + PO agreement, a re-run of the solvability bot on **all** levels, and a decision entry if they alter D-036/D-018/D-040.

### D. Balance rules (apply in the M8 full pass)
- Quiver/par per mvp §7: tutorial 3 / par 1; normal 4 / par 2; set piece 5 / par 3. Boss levels per §6 (L20 ≤ 4 arrows, L40 5 arrows).
- 1★ must be reachable by a novice with the full quiver; 3★ needs the trick, not pixel precision (aim window 8–12% of screen width).
- Difficulty curve: rated 1–5 per level (04); no two consecutive levels rated ≥ 4 except before a boss; a "breather" level (≤ 2) after each set piece.
- Trajectory preview: scale 1.0 for L1–15; reduce gradually after that (never below 0.4 in MVP; never removed — mvp §3).
- Special arrows are curated per level; never more specials than the intended solution needs + 1.

## Output artefacts
- `docs/qa/playtests/YYYY-MM-DD_<milestone>_playtest.md`: hypotheses, testers (anonymised: T1..Tn, age band, play habits), the per-level table, quotes, ratings, flags, decisions, follow-up tickets.
- Tuning/level tickets `AB-###` (labels `LEVEL`/`CORE`/`PHYS`, milestone).
- At gates: a go/no-go statement by PO (G-M1, G0 Vertical Slice & Feel Lock, G2 Content Graybox Complete — names per D-102, checklists in `07` §16).

## Quality checklist
- [ ] Hypotheses written before the sessions.
- [ ] ≥ 5 non-developer testers (VS gate: 5–10); no coaching during play.
- [ ] Time to first shot and per-level attempts recorded for every tester.
- [ ] CSV analytics collected and merged with the observations.
- [ ] Every flag has a fix type and an owner ticket.
- [ ] Global tuning changes trigger a full solvability-bot re-run + re-recorded shots.
- [ ] Gate decision documented with evidence.
- [ ] Testers' data anonymised; consent recorded (no personal data in the repo).

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| "Everyone loved it" but metrics show quits | Developers/friends as testers; coaching | Recruit outside the circle; silent observation |
| Players push forward instead of pulling back | Draw affordance unclear | L1 ghost-hand animation; bow string stretch visual; consider a D-018 variant |
| Tuning fix breaks 20 other levels | Global value changed for one level's problem | Fix locally first (layout/quiver); global changes need a full bot re-run |
| 3★ trivial everywhere | Preview too generous (D-085 impact marker) | Shorten `trajectoryPreviewScale` after L15; tighten par |
| Testers blame physics as "random" | Hidden forces, debris confusion, jittering bodies | Readability pass; verify determinism; D-008 debris rules |
| Conclusions from 2 testers | Too small a sample | ≥ 5 per hypothesis; mark low-confidence findings |

## Example task prompt for a sub-agent
```text
Agent: ab-qa-playtest (session prep + analysis) with ab-product-owner (gate decision)
Skill: docs/skills/gameplay-balance-playtest.md
Ticket: AB-045 M3 Vertical Slice playtest (5–10 players) — VS go/no-go
Inputs: Android-Dev build 0.3.x with the analytics CSV on; docs/planning/11_VERTICAL_SLICE.md success criteria; docs/levels/W1_L01,04,07,10,16.md
Deliverables: docs/qa/playtests/2026-10-xx_M3_playtest.md with hypotheses (first shot < 15 s; rope found on VS-03; vase level judged fair;
voluntary 3★ replays), a per-level table, flags, follow-up AB tickets, and a PO go/no-go recommendation.
Boundaries: no tuning changes during the sessions; propose them as tickets.
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
