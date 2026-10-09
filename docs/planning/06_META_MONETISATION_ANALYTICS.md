# 06 — Meta, Monetisation & Analytics

> Owners: Systems & Progression Engineer (`SYS`) for progression/economy/save; Monetisation & Analytics Engineer (`MON`) for ads/IAP/consent/analytics/remote config. `PO` owns fairness and scope.
> Source: [`/mvp.md`](../../mvp.md) §5 (cosmetics), §6 (worlds/map), §9 (economy & monetisation), §10 (retention), §12 (save/backend), §13 (analytics/KPIs), §14 (scope).
> Canonical names: [`01_TECHNICAL_ARCHITECTURE.md`](01_TECHNICAL_ARCHITECTURE.md). Decisions: [`10_DECISION_LOG.md`](10_DECISION_LOG.md). Status: Draft v1 — 2026-10-09.

**Fairness contract (non-negotiable, enforced by code + tests):**
1. Nothing purchasable or ad-rewarded changes arrow physics, aim assist, damage, preview length or win chance. The single exception is the opt-in, capped **+1 Oak arrow** on fail, which is limited to 1★ (D-024).
2. Retries are always free and instant. No energy, no lives, no timers.
3. No ad is ever required to progress, and no forced ad plays in the first 10 minutes of lifetime play.
4. Special arrows are never sold, never consumable, never stockpiled (§5, §9 "Avoid").

---

## 1. World progression

| Rule | Value | Source |
|---|---|---|
| Worlds | 3 × 20 levels: Greenwood Range (L1–20), Sunscar Canyon (L21–40), Frostspire Keep (L41–60) | §6 |
| Level unlock inside a world | Linear: clearing `Wn_Lk` unlocks `Wn_Lk+1`. Any clear counts (1★ is enough). | §6, §10 "three-star is optional perfection" |
| World unlock | World *n+1* opens when **15 of 20** levels of world *n* are cleared (`GameConstants.LevelsToUnlockNextWorld`). Stars are not required. | §6 |
| Level 1 of the new world | Unlocked immediately when the world opens. Levels 16–20 of the previous world stay open and playable. | — |
| Boss levels (L20/40/60) | Normal unlock (after k−1 is cleared). They are not required to open the next world. | §6 |
| Replay | Any cleared level is replayable forever with no cost. Best stars/arrows are kept and never decrease. | §3 |
| Locked-world teaser | The map shows the next world's backdrop greyed out with a "Clear 15 levels to unlock (n/15)" label. | §8 |
| `world_unlocked` event | Fired once per world, at the moment the 15th clear is recorded. | §13 |

Implementation: `ProgressionService` (SYS) owns `IsUnlocked(levelId)`, `IsWorldUnlocked(worldId)`, `RecordResult(LevelResultInfo)`. All rules are data-driven from `WorldData.unlockRequirement` (default 15) and `LevelCatalog` order. EditMode tests: `ProgressionServiceTests` (see `07` §1).

## 2. Stars and the reward loop

```
Clear level ──► StarRules.Compute(arrowsUsed, goldPar, bonusArrowUsed)  (D-024)
   │               3★ ≤ par · 2★ = par+1 · 1★ otherwise · bonus arrow ⇒ 1★
   ├─► best stars updated (max) ──► star_improved (if higher)
   ├─► coins (EconomyService, §3 table) ──► Win panel: "+N coins" · optional "×2 (video)"
   ├─► Bullseye flag (if marker hit and level won) ──► bullseye_earned
   ├─► world progress n/20 ──► world_unlocked at 15
   └─► badge checks (world cleared / world perfect / bullseye milestones)
Replay motivations: 3★ (shown as empty star slots on map node), Bullseye pip, daily challenge.
```

Star totals per world (max 60) are shown on the map header. **Stars gate nothing** in the MVP. They are a mastery display and drive the "World Perfect" badges only.

## 3. Coins (D-026)

| Source | Amount | Notes |
|---|---|---|
| First clear | 20 + 10 × stars (30 / 40 / 50) | Bonus-arrow clear = 30 (1★) |
| Replay improving stars | 10 × stars gained | e.g. 1★ → 3★ = 20 |
| Replay with no improvement | 5 | Anti-grind is unnecessary: coins only buy cosmetics |
| Bullseye (first time per level) | 15 | Awarded on the win where it was first earned |
| Daily challenge complete (3/3) | 100 | Once per local day (D-027) |
| Rewarded 2× on Win panel | Doubles that level's **level reward** (not Bullseye, not daily) | Once per win, optional |
| Starter Pack IAP | 1,000 | One-time |

**Sinks:** cosmetics only (§4). Coins have no other use, no expiry and no cap (stored as `int`, clamped to 9,999,999).
**Expected earnings:** all 60 levels at 3★ ≈ 3,000 coins + Bullseyes (~30 × 15 = 450) + dailies (≈ 100/day). That is enough for 4–6 cosmetics in the first month (tuned in M9 via `EconomyConfig`).

`EconomyService` API: `Balance`, `Earn(amount, source)`, `TrySpend(amount, sink) → bool`, event `CoinsChanged`. Every change raises analytics `coins_earned` / `coins_spent` with a `source`/`sink` string.

## 4. Cosmetics catalogue

Rules (§5): cosmetics never change gameplay. Equip is instant and free. Owned items are shown in the **Bow Forge** grid with a 3D turntable preview of the bow and trail. `CosmeticDefinition` SO fields: `id`, `slot` (Bow/Trail/Badge), `displayNameKey`, `unlockSource` (Default / WorldComplete / Coins / StarterPack / Achievement), `coinPrice`, `worldId`, `achievementId`, `previewPrefab`, `icon`, `sortOrder`.

### 4.1 Bow skins (5)

| ID | Name | Unlock source | Coin price | Notes |
|---|---|---|---|---|
| `CD_Bow_OakRanger` | Oak Ranger | **Default** (equipped at install) | — | Hero bow, carved wood + gold inlay |
| `CD_Bow_Moonwood` | Moonwood | **World 1 completion** (clear L20) — *or* buy earlier | 900 | Silver-blue wood, moon-glow string |
| `CD_Bow_Ember` | Ember | **World 2 completion** (clear L40) — *or* buy earlier | 1,200 | Charred wood, ember-glow string |
| `CD_Bow_Frostglass` | Frostglass | **World 3 completion** (clear L60) — *or* buy earlier | 1,500 | Translucent ice limbs |
| `CD_Bow_RoyalViolet` | Royal Violet | **Starter Pack** — *or* coins | 1,500 | Kept off the play field, so no clash with the purple = protected rule (Q-16) |

"World completion" means all 20 levels of the world cleared. If a player already bought the skin, the completion reward converts to **300 coins** so the milestone still pays out.

### 4.2 Arrow trails (4)

| ID | Name | Unlock source | Coin price |
|---|---|---|---|
| `CD_Trail_GoldSpark` | Gold Spark | **Starter Pack** — *or* coins | 600 |
| `CD_Trail_LeafSwirl` | Leaf Swirl | Coins | 400 |
| `CD_Trail_CyanStreak` | Cyan Streak | Coins | 500 |
| `CD_Trail_EmberAsh` | Ember Ash | Coins | 600 |
| *(default)* | None / subtle white streak | Default | — |

Trails are cosmetic VFX on the flying arrow (pooled, Low tier = 50% particles). They never obscure the preview dots, which are hidden after release anyway.

### 4.3 Quiver badges (completion rewards only — never sold, §5)

| ID | Badge | Condition |
|---|---|---|
| `CD_Badge_W1Clear` / `W2Clear` / `W3Clear` | Ranger of Greenwood / Sunscar / Frostspire | Clear 20/20 in the world |
| `CD_Badge_W1Perfect` / `W2Perfect` / `W3Perfect` | Perfect Range (per world) | 60/60★ in the world |
| `CD_Badge_Bullseye10` / `Bullseye30` / `BullseyeAll` | Bullseye Hunter I/II/III | 10 / 30 / all Bullseye medals |
| `CD_Badge_Daily7` | Daily Devotee | 7 daily challenges completed (lifetime, not a streak) |

One badge can be equipped. It is shown on the quiver in the HUD corner and on the Home screen.

**Minimum viable set (cut list, `08`):** 3 skins (Oak Ranger, Moonwood, Royal Violet) + 2 trails (Gold Spark, Leaf Swirl) + world-clear badges.

## 5. Unlock rules summary

| Unlockable | Rule | Owner |
|---|---|---|
| Levels | Linear within a world (§1) | SYS |
| Worlds | 15/20 clears of the previous world | SYS |
| Special arrows | First curated appearance (Heavyhead L13, Split L30, Fire L36, Bounce L47 — D-014) shows a one-tap reveal card and adds the arrow to the Bow Forge codex. It is never a player-held item. | SYS + UI |
| Daily challenge | After clearing L10 (D-027) | SYS |
| Bow Forge (shop) | Visible from the first Home visit. Purchases are possible as soon as the balance allows. | UI |
| Interstitials | Never before 600 s of lifetime play or L6 (D-025) | MON |
| Rewarded +1 arrow | From L4 (`ads.rewarded_arrow.min_global_level`), see §7.1 | MON |

## 6. Daily challenge (D-027)

| Aspect | Spec |
|---|---|
| Unlock | After L10 is cleared |
| Selection | At first Home visit of a local calendar day: seed = hash(`yyyyMMdd` + `installId`) → pick 3 distinct **cleared** levels with `System.Random(seed)`. Prefer one per cleared world (round-robin). If fewer than 3 cleared levels are eligible, pick from all cleared levels. |
| Quiver | "Challenge quiver" = gold par of each level. Oak × par, except levels whose intended solution needs special arrows: those keep the authored special arrows, truncated to `goldPar` total in authored order. |
| Rules | Same physics. Retry is free. Rewarded +1 arrow is **not offered** in daily runs. Stars are not recorded for daily runs (they don't affect level records). |
| Reward | 100 coins when all 3 are done the same day. Claim on the daily card. |
| UI | Home "Daily Target" card: 3 level thumbnails with check marks, a claim button, and a "new in hh:mm" countdown until local midnight. |
| Offline | Fully offline. Clock changes are tolerated (cosmetic coins only). |
| Analytics | `daily_challenge_started` (first level of the day's set), `daily_challenge_completed` |
| Out of scope | Streaks, leaderboards, server-picked levels, weekly map (post-MVP) |

## 7. Ads

All ad logic goes through `IAdsService` (vendor adapter) and **`AdPolicy`** (pure C#, unit-tested, in `Services/`). The UI never decides eligibility itself. It asks `AdPolicy` and shows/hides offers.

### 7.1 Rewarded: +1 arrow on fail (§9)

**Eligibility — every condition must be true** (`AdPolicy.CanOfferBonusArrow(context)`):

| # | Condition | Default / key |
|---|---|---|
| 1 | Fail reason is `OutOfArrows`. **Never** for `ProtectedLost`. | hard rule |
| 2 | No bonus arrow has been offered in this attempt (max **one offer per attempt**, accepted or declined) | hard rule |
| 3 | The run is a normal level, not a daily challenge | hard rule |
| 4 | Global level ≥ `ads.rewarded_arrow.min_global_level` (keeps the first 3 levels ad-free) | 4 |
| 5 | **Viable state heuristic** (below) is true | — |
| 6 | `LevelData.allowRewardedArrow` is true (designer opt-out for levels where +1 trivialises the puzzle) | default true |
| 7 | Remote kill switch `ads.rewarded_arrow.enabled` is true | true |
| 8 | A rewarded ad is loaded (`IAdsService.IsRewardedReady`). **If not ready, the offer is hidden**, never shown as a dead button. | — |
| 9 | Consent flow has resolved (personalised or non-personalised is fine) | — |

**Viable state heuristic** (implements §9 "only if one objective remains or a viable state is detected"):
- `remainingRequired ≤ ads.rewarded_arrow.max_remaining_objectives` (default **1**), **or**
- `remainingRequired ≤ 2` **and** `LevelData.rewardedArrowViableWithTwo == true` (the designer asserts one good shot can still clear two, e.g. two targets under one rope load),
- **and** every remaining objective is still inside `PlayBounds` and not anchored behind an already-spent mechanic. In the MVP this is approximated by "no remaining objective is flagged `requiresSpentProp`". The validator lets designers mark objectives that only a now-consumed prop (e.g. a single boulder) could clear; if that prop is consumed, the state is not viable.

**Grant behaviour:**
- The bonus arrow is **Oak** unless `LevelData.bonusArrowType` overrides it (D-062).
- Play **resumes from the current physical state** (no reset). The HUD shows the bonus arrow with a distinct "+1" badge.
- A clear using the bonus arrow = **1★** (or the existing best is kept) and 1★ coins (D-024). `LevelResultInfo.bonusArrowUsed = true`.
- If the bonus arrow also fails, the Fail panel shows **Retry only** (no second offer).
- Ad failed or closed early → no reward, Fail panel stays with Retry dominant, `rewarded_offer_failed`.
- Retry is always the **dominant** button (size and position). The ad offer is secondary (§8).

### 7.2 Rewarded: 2× coins on Win (§9)

- Shown on the Win panel only if a rewarded ad is ready, `ads.rewarded_double.enabled`, and the level reward is > 0.
- The base coins are **credited immediately** when the panel opens. The ad adds the second half on `RewardGranted`.
- **Never blocks Next:** `Next` and `Replay` are always active, including while the ad is loading. Tapping Next dismisses the offer.
- Once per win. Not offered on daily-challenge rewards.

### 7.3 Interstitials (D-025)

`AdPolicy.CanShowInterstitial(ctx)` returns true only when **all** of these hold:

| # | Condition | Default key |
|---|---|---|
| 1 | `removeAds == false` | IAP state |
| 2 | Lifetime active playtime ≥ 600 s | `ads.interstitial.first_install_grace_seconds` = 600 |
| 3 | Completed levels since the last interstitial ≥ 3 | `ads.interstitial.min_levels_between` = 3 |
| 4 | Seconds since the last interstitial ≥ 120 | `ads.interstitial.min_seconds_between` = 120 |
| 5 | Trigger point is the **Win panel → Next / Home** transition (never mid-level, never on Fail, never on app open) | hard rule |
| 6 | The **previous result was a win** (never after a fail, including fail → retry → win in the same transition chain: the last *result* must be a win) | hard rule |
| 7 | Current global level ≥ 6 | `ads.interstitial.min_global_level` = 6 |
| 8 | No rewarded ad finished in the last 60 s | `ads.interstitial.rewarded_cooldown_seconds` = 60 |
| 9 | Kill switch `ads.interstitial.enabled` | true |
| 10 | Ad is loaded. If it is not ready, skip silently (no waiting, no spinner). | — |

Counter rules: `levelsSinceInterstitial` increments on each first-clear or replay clear (not daily-run clears), and resets when an interstitial is **shown**. Lifetime playtime counts only foreground time in Gameplay/Map/Home, accumulated by `AppLifecycle`.

**`AdPolicyTests` (EditMode) — required cases:**
1. `Interstitial_BlockedBeforeGracePeriod` (599 s → false; 600 s → true when other conditions are met)
2. `Interstitial_BlockedAfterFail`
3. `Interstitial_RequiresThreeLevelsBetween` (2 → false, 3 → true)
4. `Interstitial_RespectsMinSecondsBetween`
5. `Interstitial_BlockedWhenRemoveAdsOwned`
6. `Interstitial_BlockedBelowMinGlobalLevel`
7. `Interstitial_BlockedAfterRecentRewarded`
8. `Interstitial_OnlyOnWinTransitionTrigger` (each `AdTrigger` enum value)
9. `Interstitial_KillSwitch`
10. `BonusArrow_NeverOnProtectedLost`
11. `BonusArrow_OncePerAttempt`
12. `BonusArrow_RequiresViableState` (remaining 1 → true; 2 without flag → false; 2 with flag → true; `requiresSpentProp` → false)
13. `BonusArrow_NotInDailyChallenge`
14. `BonusArrow_HiddenWhenAdNotReady`
15. `BonusArrow_BlockedBelowMinLevel`
16. `DoubleCoins_NeverBlocksNext` (policy returns an offer object with `blocking == false` always)
17. `Policy_UsesRemoteConfigOverrides` (fake RC values change thresholds)
18. `Policy_ClampsUnsafeRemoteValues` (e.g. `min_levels_between` = 0 is clamped to the safe minimum of 3, `first_install_grace_seconds` = 60 → 600; §14 ranges)

## 8. IAP catalogue

Unity IAP behind `IIapService`. Store product IDs are identical on both stores.

| Product ID | Type | Contents | Price tier (Q-12) | Restore |
|---|---|---|---|---|
| `remove_ads` | Non-consumable | Sets `removeAds = true`. **Removes interstitials only.** Rewarded ads remain available and opt-in (§9). | USD 3.99 tier | Yes |
| `starter_pack` | Non-consumable (one-time) | `CD_Bow_RoyalViolet` + `CD_Trail_GoldSpark` + 1,000 coins. **No gameplay power.** | USD 2.99 tier | Yes. Cosmetics are re-granted. The 1,000 coins are granted **at most once per install**: the `starterPackCoinsGranted` save flag blocks repeat grants (e.g. tapping Restore again); a restore on a fresh install grants them once (D-065). |

Rules:
- The Starter Pack is hidden once owned. If the player already owns one of its cosmetics via coins, the pack card says so ("includes 1 item you own"). No partial refund in coins (keep it simple; disclosed on the card).
- Prices are displayed from store metadata (`localizedPriceString`), never hard-coded.
- **Restore Purchases** button in Settings (required on iOS; harmless on Android, where the store also restores automatically on init).
- Purchase flow: `iap_started` → store sheet → success → grant → **save immediately** → `iap_completed` (verified). Failure or cancel → `iap_failed` with reason.
- Receipt validation: local, via Unity IAP's receipt validator (obfuscated tangle files are git-ignored). Server validation is post-MVP. *Verify the current Unity IAP v5 API and validator availability at integration time.*
- No consumable coin packs in the MVP (§9 lists only Remove Ads + Starter Pack). Post-MVP candidates are in §15.

## 9. Consent and privacy

| Requirement | Implementation |
|---|---|
| Audience | 13+, not child-directed (A-07). Store age ratings follow the questionnaires (`07` §12). |
| EEA/UK/Switzerland (GDPR/UK GDPR) | Google **UMP** consent form (IAB TCF v2.2) before any personalised ads or non-essential analytics. |
| US states with privacy laws | UMP's US-states regulation messaging (opt-out of sale/share) where the chosen ad stack supports it. *Verify UMP/mediation support at integration time.* |
| iOS ATT | Show the ATT system prompt **after** the UMP flow completes, and only when relevant (ads may personalise). No custom pre-prompt in the MVP, except an optional one-line explainer screen if a playtest shows confusion. `NSUserTrackingUsageDescription` text in `07` §12. |
| Rest of world | Default to personalised ads allowed where legal. Analytics is on. Privacy choices are still accessible in Settings. |
| Data collected | No account, no name/email. Collected only by SDKs after consent: install/app-instance IDs, device/OS model, coarse IP-derived location, gameplay analytics events (§11), crash traces, ad identifiers (IDFA only if ATT is granted; GAID subject to consent), purchase history (stores). Full inventory per SDK: `07` §13. |
| Init order (Boot) | 1) Load save (local consent cache) → 2) `IConsentService.RequestConsentUpdate()` (network, 3 s timeout) → 3) show the UMP form if required → 4) iOS ATT if required → 5) `ICrashReporter.Init()` (crash collection without advertising identifiers; *legal basis to be confirmed by OWNER*) → 6) `IAnalyticsService.Init(consent)` (if analytics consent is denied in a consent region, the analytics adapter stays disabled; the Debug sink still logs locally in dev) → 7) `IRemoteConfigService.Fetch()` (2 s timeout) → 8) `IAdsService.Init(consent)` in the background (never blocks Home). |
| Offline first launch | The UMP update fails. Treat as **denied** (D-067): unknown region → conservative (non-personalised ads, analytics off). Retry silently on the next online launch. The game is fully playable. |
| Revocation / change | Settings ▸ **Privacy choices** reopens the UMP privacy-options form (where required) and applies changes immediately (re-init adapters with new flags). Settings also has **Privacy policy** (opens an external URL from `RemoteConfigDefaults.privacyPolicyUrl`) and **Terms**. |
| Data deletion requests | Privacy policy gives a contact email. Settings ▸ "Reset analytics ID" calls the analytics vendor's reset/deletion API if available (*verify*). Local data is deleted by uninstalling. |
| Consent storage | `SaveGame.consent` (§10) mirrors the TCF/UMP state for gating our own adapters. The UMP SDK stores the authoritative TCF string itself. |
| Privacy policy | Hosted URL required **by M8** (AB-163), because the consent flow and Settings link to it; final before the closed test. Content follows the data inventory in `07` §13. **Needs OWNER/legal review.** |
| `consent_result` event | Fired only after consent is known. Analytics is not initialised before this point, so it is queued in memory and sent if permitted. |

## 10. Save data requirements — `SaveGame` JSON schema v1

File `save.json` (plus `save.bak`) in `Application.persistentDataPath`, written atomically by `JsonSaveService` (`01` §14). JsonUtility-compatible: lists, not dictionaries. Times are UTC Unix seconds (`long`).

```json
{
  "schemaVersion": 1,
  "installId": "3f2c…-guid",
  "createdUtc": 1791504000,
  "lastSaveUtc": 1791507600,
  "lifetimePlaySeconds": 734.5,
  "sessionCount": 3,
  "appVersionLastRun": "0.3.0",
  "levels": [
    { "levelId": "W1_L01", "cleared": true, "bestStars": 3, "bestArrowsUsed": 1,
      "attempts": 2, "clears": 1, "bullseye": false, "firstClearUtc": 1791504100 }
  ],
  "worldsUnlocked": [1],
  "coins": 140,
  "cosmetics": {
    "owned": ["CD_Bow_OakRanger"],
    "equippedBow": "CD_Bow_OakRanger",
    "equippedTrail": "",
    "equippedBadge": "",
    "badges": [],
    "arrowCodex": ["Oak"]
  },
  "settings": {
    "musicVolume": 0.8, "sfxVolume": 1.0, "haptics": true,
    "reducedParticles": false, "reducedMotion": false, "colorAssistOutlines": false,
    "qualityTierOverride": -1
  },
  "monetization": {
    "removeAds": false,
    "ownedProducts": [],
    "starterPackCoinsGranted": false,
    "levelsSinceInterstitial": 0,
    "lastInterstitialUtc": 0,
    "lastRewardedUtc": 0,
    "interstitialsShownTotal": 0
  },
  "daily": {
    "dateKey": "20261009",
    "levelIds": ["W1_L03","W1_L07","W1_L09"],
    "done": [false,false,false],
    "rewardClaimed": false,
    "completedTotal": 0
  },
  "consent": {
    "consentVersion": 1,
    "resolved": false,
    "analyticsAllowed": false,
    "personalizedAdsAllowed": false,
    "attStatus": 0,
    "lastUpdatedUtc": 0
  },
  "tutorial": {
    "seenPrompts": [],
    "seenArrowReveals": [],
    "firstShotDone": false
  },
  "stats": {
    "arrowsFiredTotal": 0,
    "levelsClearedTotal": 0
  }
}
```

| Field | Type | Default | Written when |
|---|---|---|---|
| `schemaVersion` | int | 1 | Every save (current version) |
| `installId` | string (GUID) | new GUID on first launch | First launch only |
| `createdUtc` / `lastSaveUtc` | long | now | First launch / every save |
| `lifetimePlaySeconds` | float | 0 | On pause/quit and each level end (accumulated by `AppLifecycle`) |
| `sessionCount` | int | 0 | Session start (cold start, or resume after > 30 min in background) |
| `appVersionLastRun` | string | current | Boot (detects updates → migration/what's-new) |
| `levels[]` | list of `LevelProgress` | empty | Level end (win or fail: `attempts`), restart (`attempts`) |
| `LevelProgress.levelId` | string `W#_L##` | — | first attempt |
| `.cleared` / `.bestStars` (0–3) / `.bestArrowsUsed` / `.attempts` / `.clears` / `.bullseye` / `.firstClearUtc` | bool / int / int / int / int / bool / long | false / 0 / 0 / 0 / 0 / false / 0 | Level end; best values only ever improve |
| `worldsUnlocked` | list<int> | [1] | On 15th clear of a world |
| `coins` | int (clamped 0…9,999,999) | 0 | Earn/spend |
| `cosmetics.owned` | list<string> | [`CD_Bow_OakRanger`] | Purchase / unlock / IAP grant / restore |
| `cosmetics.equipped*` | string ("" = none) | Oak Ranger / "" / "" | Equip |
| `cosmetics.badges` | list<string> | [] | Badge condition met |
| `cosmetics.arrowCodex` | list<string> (`ArrowType` names) | ["Oak"] | First special-arrow appearance |
| `settings.*` | float/bool/int | as shown | Settings panel close |
| `monetization.removeAds` | bool | false | IAP grant/restore |
| `monetization.ownedProducts` | list<string> | [] | IAP grant/restore |
| `monetization.starterPackCoinsGranted` | bool | false | Starter pack coin grant |
| `monetization.levelsSinceInterstitial` / `lastInterstitialUtc` / `lastRewardedUtc` / `interstitialsShownTotal` | int / long / long / int | 0 | Level clear / ad shown |
| `daily.*` | string / list / list<bool> / bool / int | regenerated per day | Daily generation, level done, claim |
| `consent.*` | int / bool / bool / bool / int / long | not resolved | Consent flow and Settings changes |
| `tutorial.*` | lists / bool | empty / false | Prompt shown, arrow reveal seen, first shot |
| `stats.*` | int | 0 | Level end |

**Migration policy:**
- `schemaVersion` increments on any **breaking** change (rename, type change, semantic change). Additive fields with safe defaults don't require a bump (JsonUtility leaves missing fields at their C# default, so constructors must set defaults).
- `SaveMigrator` runs ordered `IMigration` steps (`From`, `To`, `Apply(JObject-like DTO)`). Each step has an EditMode test with a frozen JSON fixture in `Tests/EditMode/Fixtures/save_v<N>.json`.
- Never delete player-earned data during a migration. Unknown/removed fields are dropped only after one release of read-compatibility.
- Downgrade (an older build reading a newer save) loads read-only defaults for unknown versions and **does not overwrite** the newer file. It writes to `save.newer.bak` first.
- Corruption: on a load failure, try `save.bak`, then start fresh, and report `save_load_failed` to the crash reporter (non-fatal).

## 11. Analytics event dictionary

Conventions: `snake_case` names. Parameters are typed (`str`, `int`, `float`, `bool`). Every event automatically carries the **common context** (added by `AnalyticsContext`, not by callers): `session_id` (str GUID per session), `app_version` (str), `platform` (str), `quality_tier` (str), `install_days` (int). Events in a level additionally carry the **level context**: `world_id` (int), `level_id` (str `W#_L##`), `global_level` (int), `attempt_number` (int, per level, lifetime).

Names/params follow the MVP §13 list **verbatim** where defined. Vendor limits (e.g. parameter count and name length) are checked at integration — *verify for the chosen vendor*. The dictionary is designed to stay ≤ 25 params per event and names ≤ 40 characters.

| Event | Trigger | Parameters (beyond common/level context) | Raised by | Decision enabled |
|---|---|---|---|---|
| `app_open` | Boot completes (Home visible) | `cold_start` bool, `boot_ms` int | SceneFlow | Startup perf, DAU |
| `session_start` | New session (cold start or > 30 min background) | `session_number` int | AppLifecycle | Session frequency |
| `consent_result` | Consent resolved | `region_type` str (gdpr/us_state/other/unknown), `analytics_allowed` bool, `ads_personalized` bool, `att_status` str | ConsentService | Consent rate impact on monetisation |
| `tutorial_step` | FTUE milestones | `step` str (`ghost_hand_shown`, `first_draw`, `first_release`, `callout_shown`, `callout_dismissed`, `arrow_reveal_<type>`), `elapsed_s` float | TutorialPromptController | FTUE friction |
| `first_shot` | First arrow ever fired | `seconds_since_install` float | BowController (via `ArrowFired` + tutorial flag) | §10 "shot 1 within 15 s" |
| `level_started` | Gameplay level loaded (each attempt) | `arrows_start` int, `quiver` str (e.g. `oak2_heavy1`), `is_replay` bool, `is_daily` bool | GameplayController | Funnel entry |
| `arrow_fired` | Arrow released | `arrow_type` str, `arrow_index` int, `arrows_remaining` int, `power` float (2 dp), `angle_deg` int, `ms_since_level_start` int, `is_bonus` bool | GameplayController (`ArrowFired`) | Aim behaviour, tool use |
| `object_triggered` | Rope cut, balloon pop, barrel blast, oil ignite, portal enter, spring fire, boulder release, shield ricochet | `object_type` str, `cause` str (arrow/chain/fire/blast), `arrow_type` str, `arrow_index` int | Props (`PropTriggered`) | Puzzle comprehension |
| `level_completed` | Win confirmed | `arrows_start` int, `arrows_used` int, `stars` int, `gold_par` int, `result` str=`win`, `bonus_arrow_used` bool, `bullseye` bool, `duration_s` float, `is_replay` bool, `is_daily` bool, `first_clear` bool | GameplayController (`LevelWon`) | Completion and stars |
| `level_failed` | Fail confirmed | `arrows_start` int, `arrows_used` int, `result` str (`out_of_arrows`/`protected_lost`), `protected_kind` str, `objectives_remaining` int, `duration_s` float, `bonus_arrow_used` bool | GameplayController (`LevelFailed`) | Difficulty diagnosis |
| `level_restarted` | Retry tapped (HUD or Fail panel) | `from` str (`hud`/`fail_panel`/`pause`), `arrows_used` int, `ms_since_level_start` int | GameplayController (`LevelRestarted`) | Rage-retry signal |
| `level_quit` | Leave a level unfinished (Home/Map/app killed in level) | `arrows_used` int, `shots_fired` int, `ms_since_level_start` int, `before_first_shot` bool | GameplayController (`LevelQuit`) | §13 "20%+ quit before first shot" |
| `star_improved` | Replay beats best stars | `old_stars` int, `new_stars` int | ProgressionService | Replay motivation |
| `bullseye_earned` | First Bullseye on a level | `total_bullseyes` int | ProgressionService | Meta engagement |
| `world_unlocked` | 15th clear in a world | `world_id` int (unlocked world), `total_stars` int, `minutes_played` float | ProgressionService | Progression pacing |
| `rewarded_offer_shown` | Offer visible | `placement` str (`bonus_arrow`/`double_coins`), `objectives_remaining` int | UI via MON | Monetisation exposure |
| `rewarded_offer_accepted` | Player taps the offer | `placement` str | UI via MON | Offer relevance |
| `rewarded_offer_completed` | Reward granted | `placement` str, `reward_amount` int | AdsService | Fill/completion |
| `rewarded_offer_failed` | Load/show error or early close | `placement` str, `reason` str | AdsService | SDK health |
| `interstitial_shown` | Interstitial displayed | `levels_since_last` int, `seconds_since_last` int, `lifetime_play_s` int | AdsService | Ad pressure vs retention |
| `interstitial_suppressed` | **Dev/closed-test builds only** (`AB_DEV`): policy said no at a trigger point | `reason` str (grace/fail/levels/cooldown/remove_ads/min_level/recent_rewarded/not_ready) | AdPolicy | Verify ad rules (QA) |
| `iap_started` | Purchase initiated | `product_id` str, `placement` str | IapService | Purchase funnel |
| `iap_completed` | Purchase verified and granted (§13) | `product_id` str, `price_micros` int, `currency` str | IapService | Revenue |
| `iap_failed` | Cancelled/failed | `product_id` str, `reason` str | IapService | Store friction |
| `iap_restored` | Restore granted ≥ 1 product | `product_ids` str (comma list) | IapService | Support |
| `coins_earned` | Any coin gain | `amount` int, `source` str (`first_clear`/`replay`/`bullseye`/`daily`/`rewarded_double`/`starter_pack`/`world_reward_dup`), `balance` int | EconomyService | Economy balance |
| `coins_spent` | Any coin spend | `amount` int, `sink` str (`cosmetic`), `item_id` str, `balance` int | EconomyService | Sink usage |
| `cosmetic_purchased` | Coins or IAP unlock | `item_id` str, `slot` str, `source` str | InventoryService | Cosmetic demand |
| `cosmetic_equipped` | Skin/trail/badge selected (§13) | `item_id` str, `slot` str | InventoryService | Cosmetic demand |
| `daily_challenge_started` | First daily level started that day | `date_key` str | DailyChallengeService | Daily adoption |
| `daily_challenge_completed` | 3/3 done | `date_key` str, `attempts_total` int | DailyChallengeService | Daily retention hook |
| `settings_changed` | Settings panel closed with changes | `music` float, `sfx` float, `haptics` bool, `reduced_particles` bool, `reduced_motion` bool, `color_assist` bool | SettingsPanel | Accessibility usage |
| `quality_tier_changed` | Runtime guard drops a tier | `from` str, `to` str, `median_frame_ms` float | QualityTierSelector | Device perf health |

**Analytics glue:** a single `AnalyticsBridge` component (Services, MON-owned) subscribes to `GameEvents` and maps payloads → events. Gameplay code never calls `IAnalyticsService` directly (`01` §10). In dev builds `DebugAnalyticsService` logs to the console and appends to `persistentDataPath/analytics_debug.csv`; QA uses this CSV to verify the dictionary (`07` §10, "Analytics dictionary" check).

## 12. Funnels

### 12.1 FTUE funnel (first session)

| Step | Event (filter) | Target conversion from install | Notes |
|---|---|---|---|
| 0 | `app_open` (cold_start, session 1) | 100% | — |
| 1 | `consent_result` | ≥ 98% | Consent regions only show the form |
| 2 | `level_started` W1_L01 | ≥ 97% | |
| 3 | `first_shot` (seconds_since_install ≤ 15 for the median) | ≥ 95% | §10 shot 1 < 15 s |
| 4 | `level_completed` W1_L01 | **≥ 95%** | §13 KPI |
| 5 | `level_completed` W1_L03 (first chain reaction by L3, §10) | ≥ 85% | |
| 6 | `level_completed` W1_L04 with `stars == 3` at least once in L1–L4 | ≥ 60% | §10 first 3★ by L4 |
| 7 | `level_completed` W1_L05 | ≥ 75% | denominator for the W1 KPI |
| 8 | `level_started` W1_L13 (Heavyhead reveal) | ≥ 55% | §10 special-arrow reveal by L15 |
| 9 | `level_completed` W1_L20 | ≥ 45% **of step 7** | §13 W1 completion KPI |
| 10 | `world_unlocked` world 2 | tracked | |

### 12.2 Level funnel (per level, dashboard)
Starts → first shot → completion → 3★. Per level: median attempts to first clear, `level_quit.before_first_shot` rate, median `arrows_used` vs `gold_par`, fail reason split (out_of_arrows vs protected_lost), bonus-arrow offer/accept rate. **Flags (§13):** quit-before-first-shot ≥ 20%, or median retries ≥ 8 → level review ticket for LEVEL.

### 12.3 Monetisation funnel
`rewarded_offer_shown` → `rewarded_offer_accepted` → `rewarded_offer_completed` (per placement). `iap_started` → `iap_completed` (per product and placement). `interstitial_shown` per DAU vs D1/D7 by cohort (to catch ad pressure harming retention).

## 13. Core KPIs

| KPI | Target / rule | Source |
|---|---|---|
| L1 completion | ≥ 95% of installs | §13 |
| World 1 completion | ≥ 45% of installers who reach L5 | §13 |
| Median retries per normal level | 1–3 | §13 |
| Level abandonment spike | Investigate any level with ≥ 20% quit before first shot or ≥ 8 median retries | §13 |
| Time to first shot | Median ≤ 15 s from install open | §10 |
| W1 completion time | Median 20–30 min of active play | §10 |
| D1 / D7 retention | **Benchmark against the acquisition channel and CPI** at soft launch; no fixed target in MVP (§13). Provisional bands for kill/iterate/scale are in `07` §15. | §13 |
| Rewarded acceptance | Monitor only. Optimise only after the core completion KPIs are healthy. | §13 |
| Crash-free sessions | ≥ 99.5% (closed test), ≥ 99.8% (launch) | `07` |
| Interstitials per DAU | ≤ 4 average (guardrail, not a target) | D-025 |
| ARPDAU, IAP conversion | Tracked; no MVP target | — |

## 14. Remote config candidates (whitelist)

Only these keys are honoured. Every value is clamped to its safe range by `IRemoteConfigService` + `RemoteConfigDefaults`. Values outside the range are clamped and logged. **Level geometry and arrow physics are never remote-configurable.**

| Key | Type | Default | Safe range | Owner |
|---|---|---|---|---|
| `ads.interstitial.enabled` | bool | true | — | MON |
| `ads.interstitial.first_install_grace_seconds` | int | 600 | 600–3600 (never below §9's 10 min) | MON |
| `ads.interstitial.min_levels_between` | int | 3 | 3–10 (never below §9) | MON |
| `ads.interstitial.min_seconds_between` | int | 120 | 90–600 | MON |
| `ads.interstitial.min_global_level` | int | 6 | 6–20 | MON |
| `ads.interstitial.rewarded_cooldown_seconds` | int | 60 | 30–300 | MON |
| `ads.rewarded_arrow.enabled` | bool | true | — | MON |
| `ads.rewarded_arrow.min_global_level` | int | 4 | 2–10 | MON/PO |
| `ads.rewarded_arrow.max_remaining_objectives` | int | 1 | 1–2 | PO |
| `ads.rewarded_double.enabled` | bool | true | — | MON |
| `economy.first_clear_base` | int | 20 | 10–50 | SYS |
| `economy.per_star` | int | 10 | 5–25 | SYS |
| `economy.replay_plain` | int | 5 | 0–10 | SYS |
| `economy.bullseye` | int | 15 | 0–50 | SYS |
| `economy.daily_reward` | int | 100 | 50–300 | SYS |
| `economy.price_multiplier` | float | 1.0 | 0.5–1.5 (applies to coin prices) | SYS |
| `daily.enabled` | bool | true | — | SYS |
| `daily.unlock_global_level` | int | 10 | 5–20 | SYS |
| `levels.extra_oak.<levelId>` | int | 0 | 0–1 (emergency balance: +1 Oak, star par unchanged) | PO/LEVEL |
| `levels.preview_scale_min` | float | 0.4 | 0.4–1.0 (floor applied over `LevelData.trajectoryPreviewScale`; D-073) | PO |
| `perf.force_tier_max` | int | 2 | 0–2 (cap tier on problem devices) | PLAT |
| `perf.debris_cap_low` | int | 20 | 10–40 | PLAT |
| `ui.show_rate_prompt_after_level` | int | 0 (off) | 0, 15–40 | MON |
| `app.min_supported_version` | str | "" | semver | MON (soft "please update" banner, never blocks play) |
| `app.privacy_policy_url` | str | build default | https only | MON |

`levels.extra_oak.*` is the only per-level gameplay knob. It exists to hot-fix a level that analytics shows is broken (median retries ≥ 8) before a patch ships. Use is logged in the decision log.

## 15. A/B test ideas — **post-soft-launch only**

No experiments until soft-launch baselines exist and the core KPIs (§13) are healthy. Each test needs a hypothesis, a primary metric, a guardrail metric (D1, W1 completion), and a minimum sample. Remote-config-driven only.

| Idea | Primary metric | Guardrail |
|---|---|---|
| Interstitial cadence 3 vs 4 levels | ARPDAU | D1/D7, W1 completion |
| Bonus-arrow offer from L4 vs L8 | Rewarded acceptance, L-funnel completion | 3★ rate, retention |
| Daily reward 100 vs 150 coins | D7 | Cosmetic purchase rate |
| Starter Pack shown after L5 vs after L10 | IAP conversion | D1 |
| Cosmetic price multiplier 0.8 vs 1.0 | Coins spent, cosmetic equips | — |
| Preview floor 0.5 vs 0.3 in W3 | W3 completion | 3★ rate (avoid trivialising) |

Post-MVP monetisation candidates (not in scope): seasonal cosmetic path with an optional premium track, weekly challenge map + leaderboard, themed limited bow skins, cosmetic coin packs.

## 16. Explicit anti-patterns (code review must reject)

1. Selling special arrows, or any consumable gameplay advantage (§9).
2. Ads during gameplay, on app open, on Fail, or before 10 min of lifetime play (§9, §15).
3. Showing a rewarded offer that can't be fulfilled (ad not loaded), or a fake countdown "No thanks" button.
4. Making Retry smaller, delayed or less prominent than an ad offer.
5. Energy, lives, wait timers, or gating retries behind currency (§9, §14).
6. Fake progress gates (e.g. requiring stars or ads to open a world) — the world unlock is 15 clears only.
7. Forced tutorial ads, interstitials > 1 per 3 levels, or ignoring `removeAds`.
8. Hard-coded prices instead of store-localised strings.
9. Initialising analytics/ads before consent resolves in consent regions.
10. Sending PII or free-text in analytics parameters.
11. Remote-configuring physics, level geometry or star pars (only the whitelist in §14).
12. Dark patterns: pre-checked purchases, confirm-shaming copy, hidden close buttons, auto-playing rewarded ads.
13. Daily challenge streak-loss pressure (no streaks in MVP).
14. Rewarded bonus arrow granting stars above 1★ (D-024).

## 17. Vendor selection — M8 gate scorecard (D-035)

Decided at the M8 gate after the M7 integration spike. **No eCPM or fill claims are made here.** Gather current data from vendor dashboards and documentation, and from the spike, at decision time.

### 17.1 Criteria and weights

| Criterion | Weight | How measured |
|---|---|---|
| Unity integration quality (UPM/package support, Unity 6 compatibility, docs, sample) | 20% | Spike: clean Android + iOS build, no manual Gradle hacks |
| Consent support (UMP/TCF v2.2, US states, ATT flow) | 15% | Spike: consent form + flags propagate to networks |
| Rewarded + interstitial demand in target geos (mediation breadth, bidding) | 20% | Vendor docs + account manager data at decision time |
| SDK size and build impact (APK/IPA delta, method count, iOS pods) | 10% | Spike build-size diff |
| Stability (crash rate attributed to SDK, ANRs) | 10% | Spike soak test + public changelogs |
| Privacy manifest / data-safety documentation completeness | 10% | Vendor docs |
| Reporting and payout (dashboards, ad revenue export to analytics) | 10% | Account review |
| Cost and lock-in (free tier, exit path) | 5% | Terms |

### 17.2 Mediation candidates

| Candidate | Notes to verify at decision time |
|---|---|
| AppLovin MAX | Widely used for casual games. Has its own consent/terms flow integrating UMP and ATT — verify. |
| Unity LevelPlay (ironSource) | Unity-owned. Check the Unity 6.6 package status and UMP support. |
| Google AdMob mediation | Native UMP. Pairs naturally with Firebase. Check rewarded demand breadth vs bidding-first stacks. |

### 17.3 Analytics / crash / remote config candidates

| Candidate | Covers | Notes to verify |
|---|---|---|
| Firebase (GA4 Analytics, Crashlytics, Remote Config) | All three | Default lean (one family). EDM4U/CocoaPods weight. Event/param limits. IL2CPP symbol upload. |
| Unity Gaming Services (Analytics, Remote Config, diagnostics) | Analytics + RC (+ crash product status to verify) | Native to Unity. Verify the current crash-reporting offering and consent APIs. |
| GameAnalytics | Analytics (game-centric funnels, free) | No RC/crash parity. Would pair with Crashlytics. |

**Decision output:** a decision-log entry (D-0xx) naming the vendors, with the scorecard attached. The `ArrowBuster.Integrations` adapters are implemented behind `AB_*` defines (`01` §6, §15).
