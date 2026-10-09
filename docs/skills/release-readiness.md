# Skill: Release Readiness

**Primary roles:** PLAT (owner), INT, MON, QA, PO, OWNER (store accounts, signing, legal)

## Purpose
Take a green `main` to a **signed, store-uploadable build** for TestFlight / Google Play (closed test, then production), with every release gate checked: build config, signing, versioning, privacy/compliance artefacts, store listing and rollback plan.

> Store rules, SDK requirements and API levels change often. Every item marked **(verify at time of use)** must be checked against the current Apple/Google documentation on the day of submission.

## When to invoke
- M3 (first internal device builds, Android + iOS — use the build steps only), M9 (UK closed test + submission candidate), M10 (Canada/Australia soft launch), and every later store build.

**Do NOT invoke** for day-to-day dev APKs (use `Arrow Buster ▸ Build ▸ Android Dev`), and never with open S1/S2 bugs or a red regression run.

## Inputs
- A GO regression run ([`qa-regression-and-device-test.md`](qa-regression-and-device-test.md)) for the exact SHA.
- The release gates in [`07_QA_PERFORMANCE_RELEASE.md`](../planning/07_QA_PERFORMANCE_RELEASE.md).
- Accounts (OWNER), on the owner calendar: **project week 1** verify the Google Play account type and whether the personal-account closed-test requirement applies — if so, start recruiting eligible testers immediately (D-099, verify at time of use); **week 3** Mac + Apple Developer Program ready (D-100); **week 5** App Store Connect app record, Google Play Console app with Play App Signing, and the IAP products `remove_ads` (£3.99 / USD 3.99) and `starter_pack` (£2.99 / USD 2.99) created (D-091, D-100).
- Privacy policy hosted **before M7 SDK integration starts** (D-097); its URL goes in Settings and both store listings.
- Name clearance: legal/store/domain check of "Arrow Buster" done **before** store-submission assets are produced (M8, D-081). Technical identifiers use `ArrowBuster` / lowercase `arrowbuster` where the platform requires it.
- Secrets outside git: Android upload keystore + passwords, the App Store Connect API key, `google-services.json` / `GoogleService-Info.plist` (if Firebase), IAP tangle secrets.
- Final bundle ID (currently `com.attila.arrowbuster` in `ProjectSetup` — "change before first store upload"; OWNER decides, since it can never change after upload).

## Step-by-step workflow

### A. Pre-flight (both platforms)
1. `git checkout main && git pull`; confirm the SHA matches the GO regression run. Tag `v0.<M>.<patch>-rc<n>`.
2. Set `BuildConfig` = `Release` (or `ClosedTest`):
   - `AB_DEV` and `AB_CHEATS` **off**;
   - `adsTestMode` off (closed test may keep test ads — decide per build);
   - analytics debug off;
   - log level Warning.
3. `BuildVersioning`: `bundleVersion` = `0.<M>.<patch>` (1.0.0 at launch); Android `bundleVersionCode` +1; iOS `buildNumber` +1.
4. Run `Arrow Buster ▸ Levels ▸ Validate All` (the build script also does this and aborts on errors) and the placeholder-art check (no `Art/_Placeholder` dependencies).
5. Player settings audit (`Arrow Buster ▸ Setup ▸ 1. Apply Mobile Player Settings`, then inspect):
   - portrait only;
   - IL2CPP;
   - ARM64;
   - managed stripping level Low/Medium with `link.xml` for SDKs;
   - the correct product name, version and icons (all sizes);
   - the splash screen (Unity splash setting per licence tier).

### B. Android (Windows PC)
1. Build Profile `Android-Release` → **Build App Bundle (AAB)** ON; Split Application Binary only if > 150 MB (Play Asset Delivery — shouldn't be needed, D-013).
2. Signing:
   - Publishing Settings ▸ custom keystore = the **upload key** (stored outside the repo, path in the local `UserSettings` only);
   - Google holds the app signing key through **Play App Signing** (enrol on first upload);
   - back up the upload keystore in 2 places (OWNER).
3. Target API level: set to the level Google Play requires for new apps/updates **(verify at time of use)**; min API 26 stays.
4. Build via `manage_build` or `-executeMethod ArrowBuster.Editor.BuildScript.BuildAndroidRelease`.
5. Inspect the AAB:
   - size (≤ 150 MB target);
   - `bundletool build-apks --bundle=<aab> --output=out.apks --connected-device` → `bundletool install-apks --apks=out.apks` on a Mid + Low device;
   - smoke test (L1–L3, purchase sandbox, consent EEA debug, rewarded test ad);
   - `adb logcat -s Unity AndroidRuntime` clean.
6. Upload IL2CPP symbols (`symbols.zip` from the build) to Play Console and to the crash vendor (Crashlytics symbol upload step).
7. Play Console:
   - internal → closed testing track (**UK testers first**, D-098; ≥ 14 days / the store-required tester count if D-099 applies — verify at time of use);
   - **Data safety** form from the 07 data inventory;
   - content rating questionnaire (13+);
   - ads declaration "contains ads";
   - target audience 13+;
   - privacy-policy URL;
   - in-app products `remove_ads`, `starter_pack` active.

### C. iOS (export on Windows, build on the Mac)
1. Build Profile `iOS-Release` → Build to `Builds/iOS/` (Xcode project). Commit nothing from `Builds/`.
2. Copy to the Mac (shared drive/zip). Run `pod install` if the CocoaPods SDKs (ads/Firebase) are present; open **`Unity-iPhone.xcworkspace`**.
3. Signing & Capabilities:
   - Team;
   - automatic signing (or a distribution profile);
   - bundle ID;
   - In-App Purchase capability.
4. Info.plist (should be set via a Unity post-process script `iOSPostProcess` in `Scripts/Editor/Build/`, so it is never hand-edited):
   - `NSUserTrackingUsageDescription` (the ATT string, clear and honest, e.g. "Allows ads that are more relevant to you. Arrow Buster works the same either way.");
   - `SKAdNetworkItems` per the mediation SDK's list **(verify at time of use)**;
   - `GADApplicationIdentifier` if AdMob demand is used;
   - `ITSAppUsesNonExemptEncryption = NO` (if only standard HTTPS).
5. **Privacy manifests:**
   - confirm `PrivacyInfo.xcprivacy` is present for the app (Unity generates one) and for every third-party SDK;
   - required-reason APIs declared **(verify at time of use)**;
   - Xcode ▸ Product ▸ Archive ▸ Generate Privacy Report and review it.
6. Archive → Validate → Distribute to App Store Connect → TestFlight. Upload dSYMs to the crash vendor.
7. App Store Connect:
   - App Privacy nutrition labels from the 07 data inventory;
   - age rating 12+/13+-equivalent questionnaire;
   - IAP products in "Ready to Submit" with review screenshots;
   - export compliance;
   - review notes (how to reach a fail screen to see the rewarded offer; that no login is required).

### D. Store listing (original assets only — D-089; name cleared first — D-081)
- Icon, screenshots (6.7"/6.5"/5.5" iPhone, iPad if universal; Android phone + 7"/10" tablet if supported), optional preview video, short/long descriptions, keywords.
- **Originality review:** compare against the reference game — different palette/frame, no cannon, no can towers, no similar wordmark, no copied copy (mvp §1, §11).

### E. Release gates and rollout
1. Gates (from `07` §16: G5 Release Candidate, then G-Release Submission; sign-off ticket AB-157):
   - GO regression;
   - crash-free sessions in the closed test ≥ the target in 07;
   - perf budgets met on Mid/Low;
   - consent/ads/IAP compliance review signed (MON + PO);
   - store forms complete;
   - OWNER approval.
2. Soft launch (M10, D-098): production release limited to **Canada and Australia** (country availability in both consoles), English only; KPI read-out per `07` soft-launch criteria before any wider release. Android production: **staged rollout** (e.g. 5% → 20% → 50% → 100%) with crash/ANR monitoring at each step. iOS: phased release ON.
3. Rollback plan:
   - Android — halt the rollout and ship a hotfix with a higher `bundleVersionCode`;
   - iOS — pause the phased release and expedite a fix;
   - remote config kill-switches (e.g. `ads.interstitial.enabled`, `ads.rewarded_arrow.enabled`) per 06.

## Output artefacts
- `docs/qa/releases/v<version>_release-checklist.md` (every step ticked with initials/role, SHA, build numbers, artefact checksums)
- Git tag `v<version>`; symbol files uploaded; store records updated
- Release notes for testers/players

## Quality checklist
- [ ] SHA = the GO regression SHA; tag created.
- [ ] Release `BuildConfig`; no dev defines; no test ad units in production; no placeholder art.
- [ ] Version/build numbers incremented on both platforms.
- [ ] Android: AAB signed with the upload key; Play App Signing enrolled; target API verified (at time of use); symbols uploaded.
- [ ] iOS: built from the `.xcworkspace`; signing OK; ATT string present; privacy manifests verified; dSYMs uploaded.
- [ ] Data safety / privacy labels match the actual SDKs and data inventory.
- [ ] IAP products configured and sandbox-tested; restore works.
- [ ] Store listing original; age rating consistent; privacy-policy URL live.
- [ ] Staged/phased rollout and rollback plan ready.

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| Play upload rejected: target API too low | Target API requirement changed | Update the target API (verify at time of use); retest |
| Play upload rejected: version code used | Not incremented | `BuildVersioning` bump; never reuse codes |
| Lost upload key | Keystore only on one PC | Play App Signing allows an upload key reset; keep 2 backups anyway |
| App Store "Missing purpose string" | ATT/camera/etc. usage string missing | Post-process adds `NSUserTrackingUsageDescription` |
| App Store privacy manifest warning/rejection | An SDK version without a manifest, or undeclared required-reason APIs | Update the SDK; declare reasons (verify at time of use) |
| Xcode link errors for SDK pods | Opened `.xcodeproj` instead of `.xcworkspace`, or `pod install` not run | Use the workspace; run `pod install` |
| Release crash not in the dev build | Stripping removed reflected types (IL2CPP) | `link.xml`; release-config device smoke test before upload |
| Crash reports unsymbolicated | Symbols/dSYMs not uploaded | Add the upload step to the checklist / build script |
| Interstitial appears in the first session in production | Remote config default wrong or the policy bypassed | `AdPolicy` tests + an RC defaults review before release |

## Example task prompt for a sub-agent
```text
Agent: ab-mobile-platform
Skill: docs/skills/release-readiness.md (A, B, E for the Android closed test)
Ticket: AB-153 (M9) Closed-test distribution — Android AAB 0.10.0-rc1 (signing pipeline from AB-152)
Inputs: GO regression docs/qa/test-runs/2026-xx-xx_M10_regression.md (SHA <sha>); keystore at <local path, not in git>
Deliverables: signed AAB uploaded to the Play closed track (OWNER performs the console upload if the agent lacks access);
docs/qa/releases/v0.10.0-rc1_release-checklist.md; symbols uploaded; tag v0.10.0-rc1 (only when OWNER says commit/tag).
Boundaries: do not change gameplay code; stop and escalate on any failing gate.
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
