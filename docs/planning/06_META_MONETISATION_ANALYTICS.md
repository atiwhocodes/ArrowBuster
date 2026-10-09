# 06 — Meta, Monetisation & Analytics

> Owners: Systems & Progression Engineer (`SYS`) for progression/economy/save; Monetisation & Analytics Engineer (`MON`) for ads/IAP/consent/analytics/remote config. `PO` owns fairness and scope.
> Source: [`/mvp.md`](../../mvp.md) §5 (cosmetics), §6 (worlds/map), §9 (economy & monetisation), §10 (retention), §12 (save/backend), §13 (analytics/KPIs), §14 (scope).
> Canonical names: [`01_TECHNICAL_ARCHITECTURE.md`](01_TECHNICAL_ARCHITECTURE.md). Decisions: [`10_DECISION_LOG.md`](10_DECISION_LOG.md). Status: Draft v2 — 2026-10-09 (owner decisions applied: D-082, D-084, D-090, D-091, D-093–D-098, D-101, D-102 milestone structure — meta systems in **M6**, platform SDKs in **M7**).

**Fairness contract (non-negotiable, enforced by code + tests):**
1. Nothing purchasable or ad-rewarded changes arrow physics, aim assist, damage, preview length or win chance. The single exception is the opt-in, capped **+1 arrow** on fail (Oak unless the level overrides it, D-062), which is limited to 1★ and says so **before** the player accepts ("Bonus Arrow Used — 1★ Max", D-084).
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
Clear level ──► StarRules.Compute(arrowsUsed, goldPar, bonusArrowUsed)  (D-084)
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
| Daily challenge complete (3/3) | 100 | Once per local day (D-094) |
| Rewarded 2× on Win panel | Doubles that level's **level reward** (not Bullseye, not daily) | Once per win, optional |
| Starter Pack IAP | 500 | One-time (D-091 "modest coin bundle") |

**Sinks:** cosmetics only (§4). Coins have no other use, no expiry and no cap (stored as `int`, clamped to 9,999,999).
**Expected earnings:** all 60 levels at 3★ ≈ 3,000 coins + Bullseyes (~30 × 15 = 450) + dailies (≈ 100/day). The launch cosmetic set costs 5,300 coins in total if bought with coins (Moonwood 1,200 + Royal Amethyst 2,500 + Gold Spark 1,000 + Leaf Swirl 600), so a player who finishes the campaign and plays dailies can collect everything without paying (tuned in M8 via `EconomyConfig`).

`EconomyService` API: `Balance`, `Earn(amount, source)`, `TrySpend(amount, sink) → bool`, event `CoinsChanged`. Every change raises analytics `coins_earned` / `coins_spent` with a `source`/`sink` string.

## 4. Cosmetics catalogue

Rules (§5): cosmetics never change gameplay. Equip is instant and free. Owned items are shown in the **Bow Forge** grid with a 3D turntable preview of the bow and trail. `CosmeticDefinition` SO fields: `id`, `slot` (Bow/Trail/Badge), `displayNameKey`, `unlockSource` (Default / WorldComplete / Coins / StarterPack / Achievement), `coinPrice`, `worldId`, `achievementId`, `previewPrefab`, `icon`, `sortOrder`.

### 4.1 Bow skins (3 at launch — D-101)

| ID | Name | Unlock source | Coin price | Notes |
|---|---|---|---|---|
| `CD_Bow_OakRanger` | Oak Ranger | **Default** (equipped at install) | — | Hero bow, carved wood + gold inlay |
| `CD_Bow_Moonwood` | Moonwood | **World 1 completion** (clear W1_L20) — *or* buy earlier (D-064) | 1,200 | Silver-blue wood, moon-glow string |
| `CD_Bow_RoyalAmethyst` | Royal Amethyst | Unlocked immediately by the **Cosmetic Starter Pack** (D-091) **or** bought with coins by any player | 2,500 (confirmed, D-104) | Renamed from "Royal Violet" (D-095). Never described as exclusive, premium-only, paid-only or limited (D-104). |

"World completion" means all 20 levels of the world cleared. If a player already bought Moonwood, the World 1 completion reward converts to **300 coins** so the milestone still pays out (D-064). **World 2 and World 3 completion** grant their world-clear badge (§4.3) **+ 300 coins** (no skin at launch).

**Deferred to a post-MVP content update (D-101):** `CD_Bow_Ember`, `CD_Bow_Frostglass`. Their `CosmeticDefinition` assets are not created in the MVP.

### 4.2 Arrow trails (2 at launch — D-101)

| ID | Name | Unlock source | Coin price |
|---|---|---|---|
| `CD_Trail_GoldSpark` | Gold Spark | **Cosmetic Starter Pack** (D-091) — *or* coins | 1,000 |
| `CD_Trail_LeafSwirl` | Leaf Swirl | Coins | 600 |
| *(default)* | None / subtle white streak | Default | — |

**Deferred to post-MVP (D-101):** `CD_Trail_CyanStreak`, `CD_Trail_EmberAsh`.

Trails are cosmetic VFX on the flying arrow (pooled, Low tier = 50% particles). They never obscure the preview dots, which are hidden after release anyway.

### 4.3 Quiver badges (completion rewards only — never sold, §5)

> **Scope (D-104):** quiver badges ship only if they stay low-cost (data + small 2D icons in the Bow Forge) and never block any milestone; otherwise they are dropped without affecting progression.

| ID | Badge | Condition |
|---|---|---|
| `CD_Badge_W1Clear` / `W2Clear` / `W3Clear` | Ranger of Greenwood / Sunscar / Frostspire | Clear 20/20 in the world |
| `CD_Badge_W1Perfect` / `W2Perfect` / `W3Perfect` | Perfect Range (per world) | 60/60★ in the world |
| `CD_Badge_Bullseye10` / `Bullseye30` / `BullseyeAll` | Bullseye Hunter I/II/III | 10 / 30 / all Bullseye medals |
| `CD_Badge_Daily7` | Daily Devotee | 7 daily challenges completed (lifetime, not a streak) |

One badge can be equipped. It is shown on the quiver in the HUD corner and on the Home screen.

**Launch set = the minimum set (D-101):** 3 skins (Oak Ranger, Moonwood, Royal Amethyst) + 2 trails (Gold Spark, Leaf Swirl) + badges. More skins/trails are built only if the project is genuinely ahead of schedule.

## 5. Unlock rules summary

| Unlockable | Rule | Owner |
|---|---|---|
| Levels | Linear within a world (§1) | SYS |
| Worlds | 15/20 clears of the previous world | SYS |
| Special arrows | First curated appearance, **locked** (D-082): Heavyhead global L13 / `W1_L13`, Split L30 / `W2_L10`, Fire L36 / `W2_L16`, Bounce L47 / `W3_L07`. shows a one-tap reveal card and adds the arrow to the Bow Forge codex. It is never a player-held item. | SYS + UI |
| Daily challenge | After clearing global L10 (D-094); cut-first if it risks the campaign | SYS |
| Bow Forge (shop) | Visible from the first Home visit. Purchases are possible as soon as the balance allows. | UI |
| Interstitials | Never before 600 s of cumulative active play, never during onboarding (global L1–L5), plus all §7.3 rules (D-093) | MON |
| Rewarded +1 arrow | From L4 (`ads.rewarded_arrow.min_global_level`), see §7.1 | MON |

## 6. Daily challenge (D-094)

> **Cut-first (D-101):** built in M6 only if it does not put the 60-level campaign at risk. If cut, the Home "Daily" badge and the `daily.*` save block stay dormant (schema unchanged).


| Aspect | Spec |
|---|---|
| Unlock | After global L10 (`W1_L10`) is cleared |
| Selection | At first Home visit of a local calendar day: seed = hash(`yyyyMMdd` + `installId`) → pick 3 distinct **cleared** levels with `System.Random(seed)`. Prefer one per cleared world (round-robin). If fewer than 3 cleared levels are eligible, pick from all cleared levels. |
| Quiver | "Challenge quiver" = gold par of each level. Oak × par, except levels whose intended solution needs special arrows: those keep the authored special arrows, truncated to `goldPar` total in authored order. |
| Rules | Same physics. Retry is free. Rewarded +1 arrow is **not offered** in daily runs. Stars are not recorded for daily runs (they don't affect level records). |
| Reward | 100 coins when all 3 are done the same day. Claim on the daily card. |
| UI | Home "Daily Target" card: 3 level thumbnails with check marks, a claim button, and a "new in hh:mm" countdown until local midnight. |
| Offline | Fully offline. Clock changes are tolerated (cosmetic coins only). |
| Analytics | `daily_challenge_started` (first level of the day's set), `daily_challenge_completed` |
| Out of scope | Streaks, leaderboards, backend, push notifications, server-picked levels, weekly map (post-MVP, D-101) |

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
- A clear using the bonus arrow = **1★** (or the existing best is kept), counts as a completed level with normal progression (unlocks, world progress), and earns 1★ coins (D-084). `LevelResultInfo.bonusArrowUsed = true`.
- **Disclosure before acceptance (D-084):** the offer (`RewardedOfferButton` on the Fail panel) always shows **"Bonus Arrow Used — 1★ Max"** (`UIStrings` key `fail.bonus_arrow.star_cap`) next to "+1 Arrow ▶". The player sees the cap *before* the ad starts; there is no post-ad surprise.
- If the bonus arrow also fails, the Fail panel shows **Retry only** (no second offer).
- Ad failed or closed early → no reward, Fail panel stays with Retry dominant, `rewarded_offer_failed`.
- Retry is always the **dominant** button (size and position). The ad offer is secondary (§8).

### 7.2 Rewarded: 2× coins on Win (§9)

- Shown on the Win panel only if a rewarded ad is ready, `ads.rewarded_double.enabled`, and the level reward is > 0.
- The base coins are **credited immediately** when the panel opens. The ad adds the second half on `RewardGranted`.
- **Never blocks Next:** `Next` and `Replay` are always active, including while the ad is loading. Tapping Next dismisses the offer.
- Once per win. Not offered on daily-challenge rewards.

### 7.3 Interstitials (D-093, supersedes D-025)

`AdPolicy.CanShowInterstitial(ctx)` returns true only when **all** of these hold:

| # | Condition | Default key |
|---|---|---|
| 1 | `removeAds == false` | IAP state |
| 2 | Cumulative **active gameplay** time per install ≥ 600 s | `ads.interstitial.first_install_grace_seconds` = 600 |
| 3 | Completed levels since the last interstitial ≥ 3 | `ads.interstitial.min_levels_between` = 3 |
| 4 | Seconds since the last interstitial ≥ 120 | `ads.interstitial.min_seconds_between` = 120 |
| 5 | Trigger point is the **Win panel → Next / Home** transition. Never during a level, never on Fail, never on app open | hard rule |
| 6 | The **previous result was a win** (never after a fail, including fail → retry → win in the same transition chain: the last *result* must be a win) | hard rule |
| 7 | **Not during onboarding:** current global level ≥ 6 (onboarding = global L1–L5 and the first session's FTUE before W1_L05 is cleared) | `ads.interstitial.min_global_level` = 6 |
| 8 | **Never after a rewarded ad:** no rewarded ad was shown on this Win panel / attempt, and none finished in the last 60 s (the next transition after any rewarded ad is always skipped) | hard rule + `ads.interstitial.rewarded_cooldown_seconds` = 60 |
| 9 | **Never after a purchase:** no IAP completed (or restored) in the current session | hard rule (`purchasedThisSession`, in memory) |
| 10 | **Never on app resume:** the transition was not interrupted by, or started right after, a return from background (`AppLifecycle.ResumedThisTransition`) | hard rule |
| 11 | Kill switch `ads.interstitial.enabled` | true |
| 12 | Ad is loaded. If it is not ready, skip silently (no waiting, no spinner). | — |

All numeric thresholds are Remote Config values clamped to the safe ranges in §14; the "never" rules are hard-coded.

Counter rules: `levelsSinceInterstitial` increments on each first-clear or replay clear (not daily-run clears), and resets when an interstitial is **shown**. Active gameplay time counts only foreground time in Gameplay (paused time excluded), accumulated by `AppLifecycle` into `lifetimePlaySeconds`.

**`AdPolicyTests` (EditMode) — required cases:**
1. `Interstitial_BlockedBeforeGracePeriod` (599 s → false; 600 s → true when other conditions are met)
2. `Interstitial_BlockedAfterFail`
3. `Interstitial_RequiresThreeLevelsBetween` (2 → false, 3 → true)
4. `Interstitial_RespectsMinSecondsBetween`
5. `Interstitial_BlockedWhenRemoveAdsOwned`
6. `Interstitial_BlockedDuringOnboarding` (global L1–L5 → false)
7. `Interstitial_BlockedAfterRewardedAd` (same transition → false; 59 s after → false)
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
19. `Interstitial_BlockedAfterPurchaseThisSession`
20. `Interstitial_BlockedOnAppResume`
21. `BonusArrow_OfferCarriesStarCapDisclosure` (the offer object always has `starCapDisclosure == true`; UI test `RewardedOfferButton` renders the `fail.bonus_arrow.star_cap` string before acceptance)

## 8. IAP catalogue

Unity IAP behind `IIapService` (D-090). Store product IDs are identical on both stores. **Launch pricing (D-091):** Remove Ads **£3.99 / USD 3.99**, Cosmetic Starter Pack **£2.99 / USD 2.99** (set as price tiers in both consoles; other currencies follow the store's tier tables). Products are created in the consoles by project week 5 (D-100).

| Product ID | Type | Contents | Price (D-091) | Restore |
|---|---|---|---|---|
| `remove_ads` | Non-consumable | Sets `removeAds = true`. **Removes forced interstitials only.** Rewarded ads remain available and opt-in (§9). | £3.99 / USD 3.99 | Yes |
| `starter_pack` | Non-consumable (one-time) | `CD_Bow_RoyalAmethyst` (also coin-buyable, 2,500) + `CD_Trail_GoldSpark` (also coin-buyable, 1,000) + 500 coins. **No stat boosts, no arrows, no puzzle advantage** (D-091). | £2.99 / USD 2.99 | Yes. Cosmetics are re-granted. The 500 coins are granted **at most once per install**: the `starterPackCoinsGranted` save flag blocks repeat grants (e.g. tapping Restore again); a restore on a fresh install grants them once (D-065). |

Rules:
- **Positioning (D-104):** the Starter Pack is good-value convenience — immediate unlock of Royal Amethyst, one included trail (Gold Spark) and a modest 500-coin bundle. Store/shop copy never uses "exclusive", "premium-only", "paid-only" or "limited"; both cosmetics remain earnable with coins. Cosmetics never change aim assist, damage, quiver, stars, progression or any gameplay outcome.
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
| EEA/UK/Switzerland (GDPR/UK GDPR) | Google **UMP** consent form (IAB TCF v2.2). **Consent-gated (D-096):** personalised ads, Firebase Analytics collection **and non-essential crash reporting (Crashlytics)**. Before consent: no personalised ads, analytics collection disabled, Crashlytics collection disabled — only the minimum technically essential diagnostics permitted by the chosen privacy implementation. Final legal/privacy review before launch. UMP is wired through Unity LevelPlay's supported consent integration or the standalone UMP SDK (*verify the exact path at M7 integration time*). |
| US states with privacy laws | UMP's US-states regulation messaging (opt-out of sale/share) where LevelPlay supports it. *Verify at integration time.* |
| iOS ATT | Show the ATT system prompt **after** the UMP flow completes, and only when relevant (ads may personalise). No custom pre-prompt in the MVP, except an optional one-line explainer screen if a playtest shows confusion. `NSUserTrackingUsageDescription` text in `07` §12. |
| Soft-launch markets: Canada, Australia (D-098) | Same consent-aware flow; UMP regional messaging where it applies. Laws to verify with the legal review before soft launch: **Canada** PIPEDA and **Quebec Law 25** (stricter consent for tracking/profiling), **Australia** Privacy Act 1988. Until confirmed, Quebec users are treated like a consent region (analytics/crash/personalised ads off until consent). |
| Rest of world | Default to personalised ads allowed where legal. Analytics is on. Privacy choices are still accessible in Settings. |
| Data collected | No account, no name/email. Collected only by SDKs after consent: install/app-instance IDs, device/OS model, coarse IP-derived location, gameplay analytics events (§11), crash traces, ad identifiers (IDFA only if ATT is granted; GAID subject to consent), purchase history (stores). Full inventory per SDK: `07` §13. |
| Init order (Boot) | 1) Load save (local consent cache) → 2) `IConsentService.RequestConsentUpdate()` (network, 3 s timeout) → 3) show the UMP form if required → 4) iOS ATT if required → 5) `ICrashReportingService.Init(consent)` (Crashlytics collection is **off by default** in the build config and enabled only if consent was given or the user is outside a consent region, D-096 — *verify the Crashlytics data-collection API at integration*) → 6) `IAnalyticsService.Init(consent)` (Firebase Analytics collection off by default; enabled only with consent in consent regions, Consent Mode signals set to match — *verify*; the Debug sink still logs locally in dev) → 7) `IRemoteConfigService.Fetch()` (2 s timeout) → 8) `IAdsService.Init(consent)` in the background (never blocks Home). |
| Offline first launch | The UMP update fails. Treat as **denied** (D-067, D-096): unknown region → conservative (non-personalised ads, analytics off, Crashlytics collection off). Retry silently on the next online launch. The game is fully playable. |
| Revocation / change | Settings ▸ **Privacy choices** reopens the UMP privacy-options form (where required) and applies changes immediately (re-init adapters with new flags). Settings also has **Privacy policy** (opens an external URL from `RemoteConfigDefaults.privacyPolicyUrl`) and **Terms**. |
| Data deletion requests | Privacy policy gives a contact email. Settings ▸ "Reset analytics ID" calls the analytics vendor's reset/deletion API if available (*verify*). Local data is deleted by uninstalling. |
| Consent storage | `SaveGame.consent` (§10) mirrors the TCF/UMP state for gating our own adapters. The UMP SDK stores the authoritative TCF string itself. |
| Privacy policy | Hosted URL required **before SDK integration begins — at the latest at the start of M7** (D-097, AB-163). It lists analytics, crash reporting, advertising, IAP processing, consent choices and a support contact. The URL goes into Settings and **both store listings**. Content follows the data inventory in `07` §13. **Needs OWNER/legal review.** |
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
    "crashReportingAllowed": false,
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
| `consent.*` (`consentVersion`, `resolved`, `analyticsAllowed`, `crashReportingAllowed` (D-096), `personalizedAdsAllowed`, `attStatus`, `lastUpdatedUtc`) | int / bool / bool / bool / bool / int / long | not resolved | Consent flow and Settings changes |
| `tutorial.*` | lists / bool | empty / false | Prompt shown, arrow reveal seen, first shot |
| `stats.*` | int | 0 | Level end |

**Migration policy:**
- `schemaVersion` increments on any **breaking** change (rename, type change, semantic change). Additive fields with safe defaults don't require a bump (JsonUtility leaves missing fields at their C# default, so constructors must set defaults).
- `SaveMigrator` runs ordered `IMigration` steps (`From`, `To`, `Apply(JObject-like DTO)`). Each step has an EditMode test with a frozen JSON fixture in `Tests/EditMode/Fixtures/save_v<N>.json`.
- Never delete player-earned data during a migration. Unknown/removed fields are dropped only after one release of read-compatibility.
- Downgrade (an older build reading a newer save) loads read-only defaults for unknown versions and **does not overwrite** the newer file. It writes to `save.newer.bak` first.
- Corruption: on a load failure, try `save.bak`, then start fresh, and report `save_load_failed` via `ICrashReportingService` (non-fatal, only if crash reporting is permitted).

## 11. Analytics event dictionary

Conventions: `snake_case` names. Parameters are typed (`str`, `int`, `float`, `bool`). Every event automatically carries the **common context** (added by `AnalyticsContext`, not by callers): `session_id` (str GUID per session), `app_version` (str), `platform` (str), `quality_tier` (str), `install_days` (int). Events in a level additionally carry the **level context**: `world_id` (int), `level_id` (str `W#_L##`), `global_level` (int), `attempt_number` (int, per level, lifetime).

Names/params follow the MVP §13 list **verbatim** where defined. Vendor limits (e.g. parameter count and name length) are checked at integration — *verify for the chosen vendor*. The dictionary is designed to stay ≤ 25 params per event and names ≤ 40 characters.

| Event | Trigger | Parameters (beyond common/level context) | Raised by | Decision enabled |
|---|---|---|---|---|
| `app_open` | Boot completes (Home visible) | `cold_start` bool, `boot_ms` int | SceneFlow | Startup perf, DAU |
| `session_start` | New session (cold start or > 30 min background) | `session_number` int | AppLifecycle | Session frequency |
| `consent_result` | Consent resolved | `region_type` str (gdpr/us_state/other/unknown), `analytics_allowed` bool, `crash_allowed` bool, `ads_personalized` bool, `att_status` str | ConsentService | Consent rate impact on monetisation |
| `tutorial_step` | FTUE milestones | `step` str (`ghost_hand_shown`, `first_draw`, `first_release`, `callout_shown`, `callout_dismissed`, `arrow_reveal_<type>`), `elapsed_s` float | TutorialPromptController | FTUE friction |
| `first_shot` | First arrow ever fired | `seconds_since_install` float | BowController (via `ArrowFired` + tutorial flag) | §10 "shot 1 within 15 s" |
| `level_started` | Gameplay level loaded (each attempt) | `arrows_start` int, `quiver` str (e.g. `oak2_heavy1`), `is_replay` bool, `is_daily` bool | GameplayController | Funnel entry |
| `arrow_fired` | Arrow released | `arrow_type` str, `arrow_index` int, `arrows_remaining` int, `power` float (2 dp), `angle_deg` int, `ms_since_level_start` int, `is_bonus` bool | GameplayController (`ArrowFired`) | Aim behaviour, tool use |
| `object_triggered` | Rope cut, balloon pop, barrel blast, oil ignite, portal enter, boulder release, shield ricochet | `object_type` str, `cause` str (arrow/chain/fire/blast), `arrow_type` str, `arrow_index` int | Props (`PropTriggered`) | Puzzle comprehension |
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
| `interstitial_suppressed` | **Dev/closed-test builds only** (`AB_DEV`): policy said no at a trigger point | `reason` str (grace/fail/levels/cooldown/remove_ads/onboarding/after_rewarded/after_purchase/app_resume/not_ready) | AdPolicy | Verify ad rules (QA) |
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
| 8 | `level_started` W1_L13 (Heavyhead reveal, D-082) | ≥ 55% | §10 special-arrow reveal by L15 |
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
| D1 / D7 retention | **Benchmark against the acquisition channel and CPI** at the Canada/Australia soft launch (D-098); no fixed target in MVP (§13). Provisional bands for kill/iterate/scale are in `07` §15. | §13 |
| Rewarded acceptance | Monitor only. Optimise only after the core completion KPIs are healthy. | §13 |
| Crash-free sessions | ≥ 99.5% (closed test), ≥ 99.8% (launch) | `07` |
| Interstitials per DAU | ≤ 4 average (guardrail, not a target) | D-093 |
| ARPDAU, IAP conversion | Tracked; no MVP target | — |

## 14. Remote config candidates (whitelist)

Only these keys are honoured. Every value is clamped to its safe range by `IRemoteConfigService` + `RemoteConfigDefaults`. Values outside the range are clamped and logged. **Level geometry and arrow physics are never remote-configurable.**

| Key | Type | Default | Safe range | Owner |
|---|---|---|---|---|
| `ads.interstitial.enabled` | bool | true | — | MON |
| `ads.interstitial.first_install_grace_seconds` | int | 600 | 600–3600 (never below §9's 10 min) | MON |
| `ads.interstitial.min_levels_between` | int | 3 | 3–10 (never below §9) | MON |
| `ads.interstitial.min_seconds_between` | int | 120 | 90–600 | MON |
| `ads.interstitial.min_global_level` | int | 6 (= onboarding ends after global L5) | 6–20 | MON |
| `ads.interstitial.rewarded_cooldown_seconds` | int | 60 | 60–300 (the same-transition rule after a rewarded ad is hard-coded) | MON |
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

Step 12 of the execution order (D-102): only after the Canada/Australia soft-launch data (D-098). No experiments until soft-launch baselines exist and the core KPIs (§13) are healthy. Each test needs a hypothesis, a primary metric, a guardrail metric (D1, W1 completion), and a minimum sample. Remote-config-driven only.

| Idea | Primary metric | Guardrail |
|---|---|---|
| Interstitial cadence 3 vs 4 levels | ARPDAU | D1/D7, W1 completion |
| Bonus-arrow offer from L4 vs L8 | Rewarded acceptance, L-funnel completion | 3★ rate, retention |
| Daily reward 100 vs 150 coins | D7 | Cosmetic purchase rate |
| Starter Pack shown after L5 vs after L10 | IAP conversion | D1 |
| Cosmetic price multiplier 0.8 vs 1.0 | Coins spent, cosmetic equips | — |
| Preview floor 0.6 vs 0.4 in W3 (never below the D-073 floor) | W3 completion | 3★ rate (avoid trivialising) |

Post-MVP monetisation candidates (not in scope): seasonal cosmetic path with an optional premium track, weekly challenge map + leaderboard, themed limited bow skins, cosmetic coin packs.

## 16. Explicit anti-patterns (code review must reject)

1. Selling special arrows, or any consumable gameplay advantage (§9).
2. Interstitials during gameplay, on app open or resume, after a Fail, after a purchase, after a rewarded ad, during onboarding, or before 10 min of cumulative active play (§9, D-093).
3. Showing a rewarded offer that can't be fulfilled (ad not loaded), or a fake countdown "No thanks" button.
4. Making Retry smaller, delayed or less prominent than an ad offer.
5. Energy, lives, wait timers, or gating retries behind currency (§9, §14).
6. Fake progress gates (e.g. requiring stars or ads to open a world) — the world unlock is 15 clears only.
7. Forced tutorial ads, interstitials > 1 per 3 levels, or ignoring `removeAds`.
8. Hard-coded prices instead of store-localised strings.
9. Initialising analytics collection, Crashlytics collection or personalised ads before consent resolves in consent regions (D-096).
10. Sending PII or free-text in analytics parameters.
11. Remote-configuring physics, level geometry or star pars (only the whitelist in §14).
12. Dark patterns: pre-checked purchases, confirm-shaming copy, hidden close buttons, auto-playing rewarded ads.
13. Daily challenge streak-loss pressure (no streaks in MVP).
14. Rewarded bonus arrow granting stars above 1★, or hiding the "Bonus Arrow Used — 1★ Max" cap until after the ad (D-084).

## 17. Vendor decision record (D-090, supersedes D-035)

**Decided by OWNER on 2026-10-09:**

| Need | Vendor | Behind | Integration milestone |
|---|---|---|---|
| Analytics | **Firebase Analytics (GA4)** | `IAnalyticsService` → `FirebaseAnalyticsService` | M7 |
| Crash reporting | **Firebase Crashlytics** | `ICrashReportingService` → `CrashlyticsService` | M7 |
| Remote config | **Firebase Remote Config** | `IRemoteConfigService` → `FirebaseRemoteConfigService` | M7 |
| Ad mediation (rewarded + interstitial) | **Unity LevelPlay** | `IAdsService` → `LevelPlayAdsService` | M7 |
| Purchases | **Unity IAP** (`com.unity.purchasing`) | `IIapService` → `UnityIapService` | M7 |
| Consent | **Google UMP** (via LevelPlay's supported integration or standalone — verify) + iOS ATT | `IConsentService` → `UmpConsentService` | M7 |

Interfaces and Debug/Mock implementations are built in **M2** (AB-003/AB-024). All adapters live in `ArrowBuster.Integrations` behind `AB_FIREBASE`, `AB_LEVELPLAY`, `AB_UNITY_IAP` defines (`01` §6, §15). **No vendor SDK enters the project before M7** (D-102 step 10); an optional isolated compile spike on a throwaway branch late in M6 is allowed.

### 17.1 M7 integration acceptance checks (formerly selection criteria)

| Check | How measured |
|---|---|
| Unity 6000.6.5f1 compatibility, clean Android + iOS builds, no manual Gradle hacks; EDM4U version pinned | Build both platforms from `BuildScript` |
| Consent propagation: UMP/TCF v2.2 flags reach LevelPlay networks; Firebase Analytics + Crashlytics collection stays off pre-consent in UK/EEA (D-096) | EEA debug geography test (`07` §10) |
| SDK size and build impact | APK/IPA size diff vs. the pre-M7 build (≤ 150 MB total budget, `01` §11) |
| Stability | Soak test, ANR/crash attribution |
| Privacy manifests / Data safety documentation | Vendor docs + `07` §13 inventory |
| IL2CPP symbol upload for Crashlytics | Test crash symbolicated on both platforms |

### 17.2 Considered and not chosen (record only)
AppLovin MAX, Google AdMob as the mediation layer, Unity Gaming Services analytics/diagnostics, GameAnalytics. They are not active options; revisiting requires a new decision entry.

**Decision output:** D-090 (vendors) — any change of vendor needs a new decision-log entry.
