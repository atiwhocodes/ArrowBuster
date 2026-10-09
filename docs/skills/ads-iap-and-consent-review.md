# Skill: Ads, IAP and Consent Review

**Primary roles:** MON (owner), PLAT (build/SDK integration), PO (fairness sign-off), QA

## Purpose
Integrate and review rewarded ads, interstitials, IAP and consent so that they are **compliant, fair, robust offline, and isolated behind interfaces**. The ethical rules (mvp §9, D-084, D-093) are enforced in code by the pure, unit-tested `AdPolicy`. They are never left to convention.

## When to invoke
- M2 (interfaces + mocks: `IAdsService`, `IIapService`, `IConsentService`, `AdPolicy` tests).
- M7 SDK integration (Firebase + LevelPlay + Unity IAP + UMP/ATT, D-090) — starts only after the hosted privacy policy exists (D-097); M9 pre-submission review (UK closed test, D-098).
- Any change to ad placements, product IDs, prices, consent flow, or an SDK version bump.

**Do NOT invoke** to add new placements, currencies or offers that aren't in mvp §9 — those need PO + OWNER approval and a decision entry. Never use it to sell special arrows or any gameplay power.

## Inputs
- mvp §9 placements and the "Avoid" list; D-084 (bonus-arrow clear capped at 1★, disclosed before the ad), D-093 (interstitial policy), D-026 (coins), D-090 (vendors), D-091 (pricing), D-096 (consent gating).
- Product catalogue: `remove_ads` (non-consumable, removes interstitials only; rewarded stays opt-in) and `starter_pack` (non-consumable: bow skin + trail + coins, no power). Store IDs live in the store consoles (prices per Q-12).
- Unity LevelPlay (mediation), Google UMP + iOS ATT, Unity IAP (`com.unity.purchasing`), Firebase Analytics / Crashlytics / Remote Config (D-090). All are added through `ArrowBuster.Integrations` with `#if` defines (`AB_MAX`/`AB_LEVELPLAY`, `AB_UNITY_IAP`, `AB_FIREBASE`).
- The privacy-policy URL and the data inventory (07).

## Step-by-step workflow
1. **Policy in code** (`Services/AdPolicy.cs`, pure, EditMode tests in `Tests/EditMode/AdPolicyTests.cs`):
   - `CanOfferBonusArrow(context)`: implements the 9 conditions and the viable-state heuristic in `06` §7.1 exactly. In short: only on an `OutOfArrows` fail (never `ProtectedLost`); max one offer per attempt; not in daily runs; global level ≥ `ads.rewarded_arrow.min_global_level` (default 4, so L1–L3 stay ad-free); remaining required objectives ≤ 1, or ≤ 2 with `rewardedArrowViableWithTwo`, and none flagged `requiresSpentProp`; `allowRewardedArrow` true; kill switch on; ad loaded; consent resolved.
   - `CanOfferDoubleCoins(state)`: on the Win screen, optional, never blocks `Next`.
   - `CanShowInterstitial(state)`: all D-093 conditions (≥ 600 s cumulative active play per install, ≥ 3 completed levels since the last one, ≥ 120 s since the last one, the Win → Next/Home transition only, never after a fail, never during onboarding (global L1–L5 / first-session FTUE), never on the transition right after a rewarded ad, never after a purchase in the same session, never on app resume, `removeAds == false`; thresholds from Remote Config).
   Each condition has its own test, plus a "first 10 minutes never" test.
2. **Interfaces + mocks first:** `MockAdsService` (configurable ready/fail/cancel/timeout), `MockIapService` (success/cancel/pending/failure/restore), `MockConsentService` (EEA/non-EEA, granted/denied). Dev builds expose these toggles in the DevOverlay.
3. **SDK integration (M7; optional isolated compile spike branch late in M6, `mon/<ticket>-sdk-spike`):**
   - precondition: the privacy policy is hosted (D-097);
   - import Unity LevelPlay + Google UMP + Unity IAP + Firebase (Analytics, Crashlytics, Remote Config) — vendors are decided (D-090), no selection;
   - resolve dependencies (`Assets ▸ External Dependency Manager ▸ Android Resolver ▸ Force Resolve`; iOS CocoaPods on the Mac);
   - build an Android APK and an iOS Xcode project;
   - show test ads and complete a sandbox purchase;
   - record APK size delta and init time;
   - record sizes, init times and issues in the integration report.
4. **Init order** (Boot), in this order:
   1. Load the save.
   2. `IConsentService.Gather()` — UMP: request the consent info update, show the form if required (EEA/UK/CH, US-state regulations).
   3. iOS: show the ATT prompt (`NSUserTrackingUsageDescription` string required) **after** UMP and only if consent allows tracking prompts.
   4. Initialise Crashlytics via `ICrashReportingService` with collection **disabled** until consent allows it in UK/EEA (D-096).
   5. Initialise Firebase Analytics with collection disabled until consent; enable per the consent state.
   6. Initialise ads with the consent signals (TCF string/consent flags passed to mediation, non-personalised when denied).
   7. Initialise IAP.
   8. Fetch remote config (2 s timeout).
   9. Go to Home.
   Nothing blocks play on network failure. *Verify the exact SDK APIs/ordering requirements at time of use.*
5. **Rewarded flow:**
   - offer UI only when `IsRewardedReady`;
   - the reward is granted only on the SDK's reward callback (not on close);
   - timeout 30 s → show "Ad unavailable", no penalty;
   - analytics `rewarded_offer_shown` / `rewarded_offer_accepted` plus a completed/failed result;
   - the +1 arrow appends an Oak to the quiver, marks `bonusArrowUsed`, and `StarRules` caps at 1★ (D-084); the offer shows "Bonus Arrow Used — 1★ Max" **before** the player accepts.
6. **IAP flow:**
   - product catalogue in `IapCatalog` data;
   - purchase → validate locally (Unity IAP validator with obfuscated tangle files, git-ignored) → grant → save → `iap_completed`;
   - **pending** purchases (Android) handled;
   - restore button in Settings (iOS required; Android auto-restore on init);
   - Remove Ads immediately hides interstitials and any "remove ads" upsell;
   - starter pack grants are idempotent (re-restore doesn't double coins — track `purchasedProducts` in the save).
7. **Offline/poor network:** airplane mode → the game is fully playable; no ad offers shown; IAP shows "Store unavailable"; consent re-attempted next launch; no blocking spinners > 2 s.
8. **Compliance review (M9, before the UK closed test):**
   - Settings has privacy-policy and consent-options entry points (UMP privacy-options form);
   - no interstitials in the first 10 minutes of cumulative play, after a fail/rewarded ad/purchase, during onboarding or on resume (D-093);
   - no forced ads;
   - rewarded clearly labelled;
   - the starter pack has no gameplay effect;
   - Data safety / privacy labels match the SDKs;
   - iOS privacy manifests present for each SDK;
   - ATT string is clear;
   - age rating consistent with ads content filters (13+, assumption A-07 in `00`; set ad content rating to Teen or lower).
   *Store policy specifics: verify at time of use.*
9. **Device QA:** test accounts (App Store sandbox, Play licence testers), test ad units only in dev/closed test builds (`BuildConfig.adsTestMode`), EEA consent simulated with the UMP debug geography, ATT on a fresh install.

## Output artefacts
- `Scripts/Runtime/Services/{IAdsService,IIapService,IConsentService,AdPolicy,Mock*}.cs`; `Tests/EditMode/AdPolicyTests.cs`
- `Scripts/Integrations/{LevelPlayAdsService,UnityIapService,UmpConsentService,FirebaseAnalyticsService,FirebaseCrashReportingService,FirebaseRemoteConfigService}.cs` (M7)
- `docs/qa/monetisation/YYYY-MM-DD_sdk-integration.md` (sizes, init times, issues)
- `docs/qa/monetisation/YYYY-MM-DD_compliance-review.md` (checklist results with screenshots)

## Quality checklist
- [ ] `AdPolicy` tests cover every D-093 condition, the rewarded-arrow conditions (D-063) and the 1★ cap + pre-accept disclosure (D-084); green.
- [ ] No ads of any kind before 10 minutes of lifetime play except opt-in rewarded on fail/win; no interstitial after a fail.
- [ ] The reward is granted only on the reward callback; no double grants; star cap applied.
- [ ] Remove Ads works instantly and after a restore/reinstall.
- [ ] Pending, cancelled and failed purchases handled; restore works on iOS.
- [ ] Consent gathered before analytics/ads init; denial → non-personalised / no tracking; ATT after UMP.
- [ ] Fully playable offline; no blocking waits > 2 s.
- [ ] Test ad units never in release builds (build check); real units never in dev builds.
- [ ] Privacy manifests, data safety and privacy labels consistent with the SDK list.
- [ ] PO fairness sign-off recorded.

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| Android build fails after an SDK import | EDM4U not resolved / Gradle conflicts / duplicate classes | Force Resolve; one mediation stack only; align Gradle/AGP versions with the Unity version (*verify at time of use*) |
| iOS build fails on the Mac | CocoaPods not installed/updated; the `.xcworkspace` not used | `pod install`; open the `.xcworkspace` |
| Reward granted but the ad was skipped | Reward given on close | Grant only on the reward callback |
| ATT prompt never appears / rejected by review | Missing usage description; prompt shown before the app is active, or before UMP | Add the Info.plist string via a post-process; show after UMP once the app is active |
| Consent form loops every launch | Consent state not persisted / UMP not given the debug geography reset correctly | Persist consent info; clear only in debug |
| Purchases restored twice → double coins | Grant not idempotent | Track granted product IDs in the save |
| SDK classes stripped in release | IL2CPP managed stripping | Add the vendor-provided `link.xml`; test release config early |
| Interstitial shows right after a fail | Policy called with the wrong trigger context | Pass the explicit transition type; unit test |

## Example task prompt for a sub-agent
```text
Agent: ab-monetisation-analytics
Skill: docs/skills/ads-iap-and-consent-review.md (steps 1–2)
Ticket: AB-127 (M7) `AdPolicy` (D-093, D-063) + exhaustive EditMode tests — service interfaces and null/mock services already exist from AB-003; extend the mocks as needed
Spec: mvp.md §9; D-084, D-093, D-090; docs/planning/06_META_MONETISATION_ANALYTICS.md (rewarded/interstitial rules)
Deliverables: Services/IAdsService.cs, IIapService.cs, IConsentService.cs, AdPolicy.cs, MockAdsService.cs, MockIapService.cs, MockConsentService.cs;
Tests/EditMode/AdPolicyTests.cs (one test per condition + "never in the first 10 minutes" + "never after fail");
ServiceInstaller registration of the mocks (coordinate the lock on ServiceInstaller.cs with ab-tech-architect).
Boundaries: no vendor SDKs, no UI.
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
