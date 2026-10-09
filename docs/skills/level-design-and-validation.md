# Skill: Level Design and Validation

**Primary roles:** LEVEL (owner), PO, QA, PHYS (support)

## Purpose
Produce a hand-authored level — a `LevelData` asset + a `Lvl_W<w>_L<nn>` layout prefab + an intended-solution doc — that is readable, fair, solvable within tolerance, original, and fits its slot in the world's teaching curve. **No code per level** (D-010).

## When to invoke
- Creating or reworking any of the 60 levels (including VS-01..05 = W1_L01/04/07/10/16, D-050).
- After a playtest or analytics flag (quit-before-first-shot ≥ 20% or median retries ≥ 8, mvp §13).
- When a library prefab or a physics/arrow tuning change invalidates recorded shots.

**Do NOT invoke** to create new mechanics/components (that is PROPS/CORE via [`unity-feature-planning.md`](unity-feature-planning.md)), or for art dressing only (ART swaps prefab variants).

## Inputs
- Slot definition from [`04_LEVEL_PIPELINE.md`](../planning/04_LEVEL_PIPELINE.md) (world plan, mechanic introduced, template, target quiver/par, difficulty 1–5, solution archetype) and mvp §6–7.
- The library prefab catalogue (`Prefabs/Structures`, `Objectives`, `Protected`, `Props`, `Hazards`) — use only these.
- The current `GameplayTuning` / `AD_*` presets (the flight profile chosen at the M1 gate, D-036).
- Delivery tracker (in 04) to check "no repeated layout or solution pattern within 10 levels" (mvp §6).

## Step-by-step workflow
1. **Brief (≤ 10 lines)** → propose it to PO and wait for OK (same as the `/new-level` slash command):
   - level ID;
   - mechanic taught/combined;
   - template (Weak Leg, Hanging Trouble, Safe Cargo…) or a new archetype;
   - quiver (ordered, e.g. `Oak×2, Heavyhead×1`, D-023);
   - gold par;
   - the intended solution in one sentence;
   - the likely player failure;
   - tutorial callout (only for designated teaching levels).
2. **Paper check:**
   - par vs quiver follows mvp §7 (tutorial 3/par 1, normal 4/par 2, set piece 5/par 3);
   - ≤ 60 dynamic bodies (aim ≤ 45);
   - one new mechanic at most;
   - the intended shot's aim window is ≥ 8% of screen width at target distance.
3. **Create the assets** (MCP):
   - `manage_prefabs` create `Assets/_Project/Prefabs/Levels/World<w>/Lvl_W<w>_L<nn>.prefab` from the `LevelLayout` base;
   - `manage_scriptable_object` create `Assets/_Project/ScriptableObjects/Levels/World<w>/W<w>_L<nn>.asset` (`LevelData`) and link `layoutPrefab`.
4. **Block out** in prefab mode with `manage_gameobject` / `manage_prefabs`, using **library prefab instances only** (no unpacking, no loose primitives except static `Environment` blocking):
   - snap positions to a 0.05 m grid, z = 0;
   - keep inside the play area (x ∈ [-5, 5], structure zone y ≈ 4–15, below the HUD band);
   - set `LevelLayout.clearLineY` if crates/targets/dummies may be cleared by falling (D-039);
   - add a `TutorialAnchor` where a callout points;
   - add a `BullseyeMarker` on the weak point for about half the levels (D-045).
5. **Readability pass:** red objectives, purple protected objects, gray/blue structure, green/gold helpful props (CLAUDE.md colours) — never colour alone: crest shape/icon + outline (mvp §8). Take a screenshot (`manage_scene` screenshot or the Game view) and run the **5-second test** in your head: can you name the objective and one weak point?
6. **Validate:** `Arrow Buster ▸ Levels ▸ Validate Selected` (`LevelValidator`) → fix every error; review warnings (overlaps, body count, mass ratios, clear line, blast radius vs protected, unreachable bullseye, preview scale ≠ 1 before L15, quiver < par).
7. **Play it:**
   - `manage_scene` open `Gameplay.unity`, set `_editorFallbackLevel`, then `manage_editor` play;
   - ask OWNER (or use DevOverlay aim cheats) to find the intended solution;
   - record it with **ShotRecorder** (DevOverlay ▸ Record shots) — it writes `intendedShots` into the `LevelData`;
   - `manage_editor` stop.
8. **Solvability bot:** `run_tests` filter `LevelSolvabilityTests` for this level → it must win at jitter 0 and pass the jitter rule (see [`unity-physics-validation.md`](unity-physics-validation.md) §E). Also `LevelIdleStabilityTests`.
9. **Alternative solutions:** try 2–3 brute-force approaches. If brute force with ≤ par arrows beats the intent trivially, tighten (move the objective behind a blocker, use a CursedOrb, reduce the quiver). Alternatives with **different efficiency** are good (mvp §3).
10. **Write the intended-solution doc** `docs/levels/W<w>_L<nn>.md` using the template from 04 (objective, quiver, par, intended solution with shot list, alternative solutions + their arrow counts, failure modes, tutorial copy, bullseye, success criteria, archetype tag).
11. **Update the tracker** in 04 (status Draft → Graybox → Art → Final; archetype; difficulty).
12. **Commit** the prefab + asset + `.meta` + doc together on `level/AB-###-w1-l05`.

## Output artefacts
- `Assets/_Project/Prefabs/Levels/World<w>/Lvl_W<w>_L<nn>.prefab`
- `Assets/_Project/ScriptableObjects/Levels/World<w>/W<w>_L<nn>.asset` (with `intendedShots`, `goldPar`, `quiver`, `tutorialPrompt`, `trajectoryPreviewScale`, flags)
- `docs/levels/W<w>_L<nn>.md`
- Tracker row update in `docs/planning/04_LEVEL_PIPELINE.md` (LEVEL holds the lock)

## Quality checklist
- [ ] Brief approved by PO; one new mechanic at most; fits the world plan and the 2-tutorial-levels rule.
- [ ] Only library prefabs; z = 0; grid-snapped; ≤ 60 dynamic bodies.
- [ ] Validator: 0 errors.
- [ ] Idle test green; solvability bot green incl. jitter rule.
- [ ] Gold par achievable with a non-pixel-perfect shot; 1★ clear always possible with the full quiver.
- [ ] 5-second readability test passes; colour + shape signal for objectives/protected objects.
- [ ] VFX/props never hide an objective at rest.
- [ ] No layout/solution archetype repeated within the last 10 levels.
- [ ] Original layout — not derived from a reference-game screenshot.
- [ ] Intended-solution doc written; tracker updated.

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| Level "solves itself" or collapses at load | Overlapping pieces; bodies not asleep | Validator overlap check; grid snap; `LevelLoader` sleeps bodies |
| Bot passes at jitter 0, fails under jitter | Pixel-perfect intended shot | Widen the target, move the weak point, lower the stack — don't relax the test |
| Recorded shots stopped working | Tuning or prefab change after recording | Re-record via ShotRecorder; add the level to the regression list |
| Prefab instance shows "missing prefab" after a merge | Library prefab GUID changed / `.meta` lost | Never delete+recreate library prefabs; restore the `.meta` from git |
| YAML conflict in `Lvl_*.prefab` | Two agents edited the same level | One owner per level (lock); recover per [`git-worktree-and-integration.md`](git-worktree-and-integration.md) |
| Players never notice the rope | Weak readability; preview ends before the rope | Contrast/outline, teaching callout, preview scale 1.0 in early levels |
| Level never ends | Ambient bodies counted; kinematic mover not flagged | Mark ambient in the prefab; check `SettleMonitor` config |
| Protected loss feels unfair | Blast or structure path not visible | Move the protected object, add cover, follow D-020 |

## Example task prompt for a sub-agent
```text
Agent: ab-level-design
Skill: docs/skills/level-design-and-validation.md
Ticket: AB-025 VS levels VS-01..05 graybox (W1_L01/04/07/10/16) + ShotRecorder + intended shots + solvability bot v1 — part: VS-03 (W1_L07 rope drop)
Spec: mvp.md §6 Map 1 L7–9, §7 "Hanging Trouble"; docs/planning/11_VERTICAL_SLICE.md VS-03; D-009, D-039, D-050
Deliverables: Prefabs/Levels/World1/Lvl_W1_L07.prefab, ScriptableObjects/Levels/World1/W1_L07.asset (Oak×2, par 1),
docs/levels/W1_L07.md, intended shots recorded, LevelSolvabilityTests green incl. jitter.
Boundaries: library prefabs only; no new components (request them from ab-interactive-objects).
Step 1: post the brief and wait for ab-product-owner / OWNER approval.
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
