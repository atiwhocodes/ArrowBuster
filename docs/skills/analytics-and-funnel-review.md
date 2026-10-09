# Skill: Analytics and Funnel Review

**Primary roles:** MON (owner), PO, LEVEL (difficulty diagnosis), QA (event verification)

## Purpose
Two jobs:
1. **Instrument:** make sure every analytics event in the dictionary ([`06_META_MONETISATION_ANALYTICS.md`](../planning/06_META_MONETISATION_ANALYTICS.md)) fires once, at the right moment, with correct parameters, through `IAnalyticsService`. Consent rules are respected throughout.
2. **Review:** read funnels and level metrics to diagnose difficulty and pacing against the mvp §13 KPI targets (L1 completion ≥ 95%, W1 completion ≥ 45% of L5 reachers, median retries 1–3, investigate ≥ 20% quit-before-first-shot or ≥ 8 median retries).

## When to invoke
- AB-024 (DebugAnalyticsService + loop events), each milestone that adds events (M5 progression, M8 monetisation/meta/vendor), and before any closed test or soft launch.
- Weekly during the closed test / soft launch for funnel reviews.

**Do NOT invoke** to add A/B tests before soft-launch baselines exist (post-MVP only), or to optimise ad density before core completion is healthy (mvp §13).

## Inputs
- The event dictionary in 06 (names, params, trigger moments). It must include the mvp §13 events verbatim:
  `level_started`, `arrow_fired`, `object_triggered`, `level_completed`, `level_failed`, `level_restarted`, `rewarded_offer_shown`, `rewarded_offer_accepted`, `world_unlocked`, `cosmetic_equipped`, `iap_completed`.
- Common params: `world_id`, `level_id`, `global_level`, `attempt_number`, `arrow_type`, `arrows_start`, `arrows_used`, `result`, `session_id`.
- `AnalyticsEvents` constants + param builders (`Scripts/Runtime/Services/AnalyticsEvents.cs`).
- For reviews: an export or dashboard from the chosen vendor (D-035), or `DebugAnalyticsService` CSVs from playtests.

## Step-by-step workflow

### A. Instrumentation
1. **Constants only.** Event and param names come from `AnalyticsEvents` (snake_case). No string literals at call sites. Each event has a builder method (`AnalyticsEvents.LevelCompleted(in LevelResultInfo)`).
2. **Glue lives in one place:** an `AnalyticsBridge` listener in Services subscribes to `GameEvents` (and to service events such as `ProgressionService.WorldUnlocked`, `IIapService.PurchaseCompleted`) and calls `Services.Analytics.Log(...)`. Gameplay never calls analytics directly (01 §10.3).
3. **Consent gate:** `IAnalyticsService` queues nothing and sends nothing until `IConsentService` resolves. If analytics consent is denied, use the vendor's consent mode / disable collection (vendor-specific, *verify at time of use*). `DebugAnalyticsService` always logs locally in dev.
4. **Session/attempt bookkeeping:**
   - `session_id` = a GUID per app foreground session (new after 30 min in the background);
   - `attempt_number` = per-level attempt counter from the save (increments on `level_started`);
   - `global_level` from `LevelData.GlobalIndex` (D-047).
5. **Verify in the editor:**
   - `manage_editor` play VS-01..05: win, fail, restart, quit;
   - `read_console` filtered by `[Analytics]`, or open the CSV at `Application.persistentDataPath/analytics_debug.csv`;
   - check each row against the dictionary: name, params present, values plausible, fired exactly once.
6. **Automate:** `Tests/PlayMode/GameFlowSmokeTests` installs a `RecordingAnalyticsService` and asserts the exact event sequence for scripted flows: start → fire×N → complete; start → fail → restart; rewarded offer shown/accepted.
7. **Vendor verification (M8+):**
   - Firebase DebugView (`adb shell setprop debug.firebase.analytics.app com.attila.arrowbuster`; iOS `-FIRDebugEnabled` launch argument), or the vendor's equivalent live-debug tool;
   - confirm events arrive with params; check param count/length limits (*verify at time of use*).

### B. Funnel and level review
1. **Core funnel:** install → consent resolved → `level_started` L1 → first `arrow_fired` (target < 15 s from launch) → `level_completed` L1 → L5 → L10 → L15 (Heavyhead reveal) → L20 → `world_unlocked` W2 → L40 → L60.
2. **Per-level table** (`global_level` × metric):
   - starts, completion rate;
   - median attempts to first clear;
   - % quit before the first shot;
   - median `arrows_used` vs `goldPar`;
   - star distribution;
   - fail reasons (`out_of_arrows` vs `protected_lost`);
   - `level_restarted` per start;
   - time to first shot.
3. **Flags:**
   - quit-before-first-shot ≥ 20% → readability problem;
   - median retries ≥ 8 → too hard / unfair;
   - 3★ rate > 80% on a non-tutorial level → too easy;
   - protected-lost > 40% of fails → protected placement unfair;
   - spike in `object_triggered` absence (players never cut the rope) → teaching failure.
4. **Hand flagged levels to LEVEL + PO** as tickets with data. Remote-config levers (06) allow quick fixes (e.g. `levels.extra_oak.<levelId>`), but level rework is preferred.
5. **Monetisation sanity (after completion is healthy):** rewarded offer shown → accepted rate, +1-arrow conversion to a win, interstitial suppression reasons (dev logs). Never tune ads up while L1/W1 KPIs are below target.

## Output artefacts
- `Scripts/Runtime/Services/AnalyticsEvents.cs`, `AnalyticsBridge.cs`, `DebugAnalyticsService.cs` (MON-owned).
- `Tests/PlayMode/GameFlowSmokeTests.cs` event-sequence assertions.
- `docs/qa/analytics/YYYY-MM-DD_event-verification.md` (dictionary × observed table).
- `docs/qa/analytics/YYYY-MM-DD_funnel-review.md` (funnel chart/table, flagged levels, actions, owners).

## Quality checklist
- [ ] All mvp §13 events implemented with the exact names; common params present where relevant.
- [ ] No string-literal event names at call sites; gameplay doesn't call analytics.
- [ ] Nothing is sent before consent resolves; denial respected.
- [ ] Each event fires exactly once per trigger (no double-fire on the second editor Play).
- [ ] `attempt_number`, `session_id`, `global_level` correct across restarts and app backgrounding.
- [ ] Smoke test asserts the sequences; green.
- [ ] Vendor DebugView verified on Android and iOS (M8+).
- [ ] Funnel review lists flagged levels with owners and actions.

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| `level_completed` logged twice | Bridge subscribed twice (domain reload disabled) | Subscribe/unsubscribe symmetrically; `StaticReset` |
| `attempt_number` resets every session | Counter kept in memory, not the save | Persist per-level attempts in `SaveGame` |
| Events missing in production | Analytics initialised before consent, then blocked; or the SDK stripped by IL2CPP | Init after consent; `link.xml` per SDK guidance |
| Param values truncated or dropped | Vendor limits on param count/length | Keep to the dictionary; *verify vendor limits at time of use* |
| Time to first shot is inflated | Measured from process start including the consent dialog | Measure from the `Home` → Play tap and from launch separately |
| Funnel conclusions from tiny samples | Closed test with < 50 players | Treat as qualitative; pair with playtest observation |

## Example task prompt for a sub-agent
```text
Agent: ab-monetisation-analytics
Skill: docs/skills/analytics-and-funnel-review.md (section A)
Ticket: AB-024 IAnalyticsService + DebugAnalyticsService + loop events
Spec: mvp.md §13; docs/planning/06_META_MONETISATION_ANALYTICS.md event dictionary; D-035
Deliverables: Services/IAnalyticsService.cs, DebugAnalyticsService.cs (console + CSV at persistentDataPath/analytics_debug.csv),
AnalyticsEvents.cs (constants + builders), AnalyticsBridge.cs listening to GameEvents for level_started, arrow_fired, object_triggered,
level_completed, level_failed, level_restarted; Tests/PlayMode/GameFlowSmokeTests.cs sequence assertions;
docs/qa/analytics/2026-10-xx_event-verification.md.
Boundaries: no vendor SDKs; no edits to gameplay scripts (consume GameEvents only).
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
