# Skill: UI/UX Mobile Review

**Primary roles:** UI (owner), PO, QA, PLAT (safe area/devices)

## Purpose
Build and review portrait mobile UI so it is **one-thumb, readable, safe-area-correct, accessible and fast**. Covers the HUD (mvp §8: top-left level + pause, top-centre objective icons, top-right arrows + restart, clean bottom), Win/Fail, Home, World Map, Bow Forge, Settings and Consent screens, built with uGUI + TMP (D-012).

## When to invoke
- Building or changing any screen/panel in `Scripts/Runtime/UI` or `Prefabs/UI`.
- At M3 (polished HUD + Win/Fail), M6 (Home, World map, Bow Forge, full meta UI, settings), M7 (consent, store/IAP and rewarded-offer UI incl. "Bonus Arrow Used — 1★ Max", D-084), M8 (accessibility pass).
- When a playtest note mentions mis-taps, unreadable text or confusion about the next action.

**Do NOT invoke** for in-world readability of level objects (that's [`level-design-and-validation.md`](level-design-and-validation.md)) or for ad SDK screens (vendor UI; see [`ads-iap-and-consent-review.md`](ads-iap-and-consent-review.md)).

## Inputs
- Screen inventory + flow + HUD layout in [`05_ART_AUDIO_UX.md`](../planning/05_ART_AUDIO_UX.md).
- `UIStrings` keys (no literal user-facing strings in code or prefabs).
- Device list with notch/Dynamic Island/punch-hole/tablet entries (07).
- Accessibility requirements (mvp §8): haptics toggle, reduced particles, colour-assist outlines, music/SFX, restore purchases, privacy; colour never the only signal.

## Step-by-step workflow
1. **Structure:**
   - one Canvas per screen;
   - Canvas Scaler = Scale With Screen Size, reference 1080×1920, match 0.5 (portrait; re-check on 9:21 and 3:4);
   - a `SafeAreaFitter` on each screen's root panel;
   - HUD canvas = Screen Space – Camera; menus = Screen Space – Overlay;
   - static and dynamic elements split into sub-canvases to limit rebuilds.
2. **Touch targets:** every tappable ≥ 44 pt (iOS) / 48 dp (Android) ≈ ≥ 120 px at the 1080 reference; ≥ 16 px spacing; primary actions in the bottom 40% thumb zone (Win: `Next` dominant; Fail: `Retry` dominant, mvp §8).
3. **Aim-zone protection:** HUD buttons sit in the top band only. A draw that starts over UI is ignored (`BowInputReader`, D-007). Verify that no transparent `Image` with Raycast Target ON covers the aim zone (it would eat draws). Turn Raycast Target OFF on decorative graphics and text.
4. **States:** each screen defines loading/empty/error/offline states (e.g. the shop when IAP is unavailable shows "Store unavailable"; a rewarded button hides when no ad is ready — never a dead button).
5. **Flow check** against the 05 UI state diagram:
   - Gameplay → Win → Next is 1 tap;
   - Fail → Retry is 1 tap;
   - Pause → Restart/Map/Settings;
   - back button (Android hardware back) handled by `UIRouter` on every screen;
   - the interstitial never inserts itself between Fail and Retry (D-093).
6. **Animation:** `UiTween` with unscaled time (works during hit-stop/pause); entry ≤ 250 ms; star reveal ≤ 1.2 s total and skippable by tap; Reduced Motion setting → crossfades only.
7. **Accessibility:**
   - text ≥ 28 px at reference (body) and ≥ 36 px for numbers in the HUD;
   - contrast ≥ 4.5:1 for text;
   - objective icons use shape + colour;
   - colour-assist outlines toggle works in the HUD and the world;
   - every haptic has an audio/visual twin;
   - no information is conveyed by sound alone.
8. **Localisation readiness:** all strings via `UIStrings` keys; layouts tolerate +30% text length (TMP auto-size with min/max, or overflow ellipsis); no text baked into sprites.
9. **Verify in the editor:**
   - Game view at 1080×1920, 1170×2532 (iPhone notch), 1080×2400 (20:9), 1284×2778, 1536×2048 (iPad);
   - Device Simulator (Window ▸ General ▸ Device Simulator) with iPhone 11, iPhone 15 Pro, Galaxy S-series punch-hole and iPad to check safe areas;
   - `manage_editor` play + `read_console` for missing references.
10. **Verify on device:** one notch iPhone, one punch-hole Android, the Low device (canvas rebuild cost in the profiler: `Canvas.SendWillRenderCanvases` < 0.5 ms).
11. **Heuristic review** (write a findings table): clarity of the primary action, number of taps to replay, any text a 13+ casual player won't understand, any dark pattern (fake close buttons, pre-checked purchases, misleading "free" labels) → **must fix**.

## Output artefacts
- Prefabs: `Assets/_Project/Prefabs/UI/UI_<Screen>.prefab`; scripts in `Scripts/Runtime/UI/`.
- `docs/qa/ui-reviews/YYYY-MM-DD_<screen>.md` — a device screenshot set + findings (severity, fix owner).
- `UIStrings` key additions.

## Quality checklist
- [ ] Safe area correct on notch, Dynamic Island, punch-hole and tablet (screenshots attached).
- [ ] Touch targets ≥ 44 pt/48 dp; primary action in the thumb zone; Retry/Next one tap.
- [ ] No raycast-blocking graphics over the aim zone.
- [ ] Android back button handled on every screen.
- [ ] All strings keyed; +30% length tolerated.
- [ ] Reduced Motion, colour-assist, haptics, music/SFX and reduced particles all work.
- [ ] Text contrast ≥ 4.5:1; minimum sizes met.
- [ ] No dark patterns; the rewarded button only shows when an ad is ready; Remove Ads hides interstitial-related UI.
- [ ] Canvas rebuild cost measured on the Low device.

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| HUD under the notch / home indicator | `SafeAreaFitter` missing or only applied once | Apply on enable and whenever `Screen.safeArea` changes (resolution/orientation change, split view on tablets) |
| Draws don't start near the top of the aim zone | A transparent Image with Raycast Target over the play field | Disable Raycast Target on decorative UI |
| Taps on Retry also fire an arrow | Input not filtered over UI, or the release frame passes to the bow | `IsPointerOverGameObject(pointerId)` on press; ignore the draw for that pointer |
| Star animation freezes | Tween using scaled time during hit-stop/pause | `UiTween` uses unscaled time |
| Text overflow after a copy change | Fixed rect with no auto-size | TMP auto-size min/max + layout groups |
| UI stutter during collapses | Whole canvas rebuild from one changing number | Split dynamic elements into sub-canvases; update text only on change |
| Android back closes the app from gameplay | No back handling | `UIRouter` back stack; in gameplay back = pause |
| Rewarded button does nothing | Shown before an ad is loaded | Bind visibility to `IAdsService.IsRewardedReady` |

## Example task prompt for a sub-agent
```text
Agent: ab-ui-ux
Skill: docs/skills/ui-ux-mobile-review.md
Ticket: AB-023 Graybox HUD (level, objective icons, quiver, restart, pause) + Win/Fail/Pause panels graybox + SafeAreaFitter
Spec: mvp.md §8; docs/planning/05_ART_AUDIO_UX.md (HUD layout, UI state flow); D-012, D-084
Listens to: GameEvents.LevelStarted, ObjectiveCleared, ArrowFired, SoftLockPrompt, LevelWon, LevelFailed, GameplayStateChanged
Deliverables: Prefabs/UI/UI_Hud.prefab, UI_WinPanel.prefab, UI_FailPanel.prefab, UI_PausePanel.prefab; Scripts/Runtime/UI/{HudView,ObjectiveIconsView,QuiverView,WinPanel,FailPanel,PausePanel,SafeAreaFitter,UIRouter}.cs;
docs/qa/ui-reviews/2026-10-xx_hud-graybox.md with Device Simulator screenshots (iPhone 11, iPhone 15 Pro, Pixel punch-hole, iPad).
Boundaries: call only GameplayController public commands (Restart, Pause, Resume, Quit); no gameplay internals.
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
