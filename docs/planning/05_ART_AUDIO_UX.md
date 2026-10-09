# 05 — Art, Audio & UX

> Owners: Art, VFX & Audio Integrator (`ART`) for §1–§11; UI/UX Engineer (`UI`) for §12–§16. Reviewers: `PO` (readability, originality), `PLAT` (budgets), `MON` (monetisation screens).
> Status: Draft v2 — 2026-10-09 (owner decisions D-080…D-103 applied: D-084, D-085, D-086, D-088, D-089, D-091, D-093, D-095, D-096, D-101, D-102 milestone structure). Source of truth: [`/mvp.md`](../../mvp.md) §4, §8, §11. Canonical names: [`01_TECHNICAL_ARCHITECTURE.md`](01_TECHNICAL_ARCHITECTURE.md). Decisions: [`10_DECISION_LOG.md`](10_DECISION_LOG.md).
> Related: object behaviour in [`03_PHYSICS_AND_OBJECTS.md`](03_PHYSICS_AND_OBJECTS.md), screen logic in [`02_GAMEPLAY_SYSTEMS.md`](02_GAMEPLAY_SYSTEMS.md), monetisation screens in [`06_META_MONETISATION_ANALYTICS.md`](06_META_MONETISATION_ANALYTICS.md), budgets verification in [`07_QA_PERFORMANCE_RELEASE.md`](07_QA_PERFORMANCE_RELEASE.md), vertical slice art scope in [`11_VERTICAL_SLICE.md`](11_VERTICAL_SLICE.md).

---

## 1. Art style guardrails

### 1.1 Direction in one sentence
**An original, stylised low-poly "toy diorama": chunky bevelled shapes, flat-shaded faces with soft gradient colour, a carved-wood-and-gold hero bow, and a scene that reads like a tabletop model lit by a warm sun** (§11).

### 1.2 Rules

| # | Rule | Why | Check |
|---|---|---|---|
| S-1 | **Gradient-palette texturing.** Every world shares one `T_<World>_Palette` (256×256, uncompressed RGB24). Meshes are UV-mapped onto palette swatches/gradients. There are no per-object albedo textures except the hero bow, UI and VFX. | Cohesion, tiny memory, fast production, SRP-batcher friendly | Import rule (§7) + review |
| S-2 | **Silhouette first.** Every gameplay object must be identifiable as a solid black silhouette at 1080×1920 at its in-game size. | Readability pillar (§2) | 5-second silhouette sheet review at M3 (G0) and M6 |
| S-3 | **Bevel, don't detail.** Use 1–2 bevel segments on edges for the toy look. No noisy surface detail; any texture noise ≤ 10% value variance. | §11 "limited texture noise" | Review |
| S-4 | **Colour language is sacred** (§4). Accent hues are reserved for gameplay meaning (§1.3). Decoration may not use reserved accent hues at full saturation. | Readability, accessibility | Palette lint: reserved swatches only in `Obj_`/`Prot_`/`Prop_`/`Haz_` meshes |
| S-5 | **Value separation.** The play-plane band is mid-to-light value. Backgrounds are lower contrast and slightly desaturated (−25% saturation, +fog). Interactive objects have ≥ 30% luminance contrast against their local background. | Objects "pop" from the diorama | Grayscale screenshot check |
| S-6 | **Fake depth of field** (D-003): background cards are pre-blurred in the source art. No real-time DOF. Near-foreground framing elements stay sharp but out of the play area. | Mobile perf | Review |
| S-7 | **One real-time directional light** (warm key), baked/ambient gradient fill, a rim term in the shader. No additional real-time lights. Fire glow is emissive + particle light cards, not lights. | D-003 | Scene audit |
| S-8 | **Material identity** follows §4: straw pale woven yellow; timber warm brown beams; stone gray-blue blocks; ice bright cyan, faux-translucent; metal dark steel with gold trims. | §4 | Material sheet |
| S-9 | **Damage states** read in < 0.3 s: cracks are dark, high-contrast decal-style geometry or a palette swap, not subtle normal detail. | ≤ 3 states (§4 safety rules) | Review |
| S-10 | **Scale consistency:** 1 Unity unit = 1 m. A standard crate is 1×1 m. A crest target is 0.8 m in diameter. The fox is ~0.7 m long. | Physics + readability | Prefab validator (bounds) |

### 1.3 Colour language (gameplay semantics, §4)

| Semantic | Hue family | Used on | Never used on |
|---|---|---|---|
| **Required objective** | Red (crimson `#D7263D` family) + crest/target icon + strong outline | CrestTarget, SupplyCrate bands, HangingLantern glass, CursedOrb core, TrainingDummy sash, BannerRope's banner | Decoration, UI chrome, hazards |
| **Helpful interactive** | Green / gold (`#3FA34D` leaf, `#E8B931` gold) | Rope/chain gold tags, balloons, oil jar glaze, powder-barrel bands, portal rings, wind fan, Bullseye ring | Protected objects |
| **Structure** | Material colours (§4) kept **mid-saturation**. Neutral pieces (platforms, metal, stone) are gray/blue. | Struct_*, Environment | Accent highlights |
| **Hazard / protected** | Purple (`#7B3FBF` family) + shield icon + halo ring | RoyalVase, SleepingFox, RoyalRelic indicator rings, spike-bed warning bands, kill-zone edge markers | Anything helpful |

> **Resolution note:** §4's "gray/blue = structure" and its warm-brown timber are reconciled as follows. Structural *materials* keep their identity colours but stay at least 20% less saturated than the reserved accent hues. Neutral structural support (platforms, stone, metal) uses gray/blue (D-051, proposed).

### 1.4 Shaders (owned by ART, reviewed by PLAT)

| Shader | Use | Notes |
|---|---|---|
| `SG_AB_StylizedLit` | All opaque gameplay and environment meshes | Palette sampling, half-Lambert wrap, rim light, per-instance tint for damage state, optional colour-assist outline pass toggle (§15) |
| `SG_AB_Ice` | Ice crystal structures | **Opaque** faux-translucency (fresnel + inner gradient + sparkle mask). No transparency sorting or overdraw. |
| `SG_AB_BackgroundCard` | Pre-blurred background layers | Unlit, fog-tinted, no shadows |
| `SG_AB_Water` | Water pit surface | Unlit scrolling gradient + foam edge; no real-time reflections |
| URP Particles Unlit/Lit | VFX | Additive/alpha-blended, VFX atlas |
| `SG_AB_UIGlow` | Bow string full-draw glow, portal rim | Additive, cheap |

### 1.5 Geometry and texture budgets per asset class

| Asset class | Triangles (LOD0) | Texture | Materials | Notes |
|---|---|---|---|---|
| Structure piece (`Struct_*`) | 150–500 | World palette | 1 | Instanced/SRP-batched; ≤ 2 bevel segments |
| Objective (`Obj_*`) | 400–1,200 | Palette (+ 128² emissive mask for lantern/orb) | 1–2 | Must read at 0.8 m |
| Protected (`Prot_*`) | 800–2,500 (fox ≤ 3,000 skinned) | Palette (+ 256² for fox fur gradient) | 1–2 | Fox: ≤ 20 bones |
| Prop (`Prop_*`) | 300–1,500 | Palette | 1 | Balloon cluster counts per balloon (≤ 200 each) |
| Hazard (`Haz_*`) | 500–2,000 | Palette + water shader | 1–2 | |
| Hero bow (per skin) | 4,000–6,000 | 512² base + 256² mask (glow/gold) | 2 | Only textured hero asset; ≤ 6 bones (limbs + string) |
| Arrow (per type) | 150–300 | Palette | 1 | Up to 17 on screen |
| Debris fragment | 20–80 | World palette | 1 | Pooled, 3–6 shapes per material |
| Environment kit piece (`Env_*`) | 100–2,000 | World palette | 1 | Whole scene background ≤ 60k tris |
| Background card | 2 quads | ≤ 1024×1024 ASTC 6×6, pre-blurred | 1 | ≤ 3 cards per world |
| Skybox / gradient | — | 256×256 gradient or procedural | 1 | |
| **Whole gameplay frame** | **≤ 150k visible (hard 250k)** | **≤ 96 MB texture memory (Mid), ≤ 64 MB (Low, mip limit 1)** | **≤ 120 SRP batches** | `01` §11 |

---

## 2. Originality guardrails

The reference boundary is §1. We use only the broad genre pattern (limited shots, destructible structures, escalating variety) and nothing else. These guardrails bind every agent, contractor and tool.

### 2.1 Do / Don't

| Do | Don't |
|---|---|
| Use a **bow with a visible drawn string** as the hero launcher, carved wood + gold inlay + glowing string | Use a **cannon**, ball launcher, slingshot or turret, or a launcher pose/shape that echoes the reference |
| Build an original **wood / leaf / parchment / gold** UI language with rounded shield-shaped buttons | Use a **purple screen frame/border**, purple-dominant UI chrome, or the reference's panel shapes |
| Design an original wordmark (letterforms derived from fletching/arrow motifs) | Use wordmark typography, colour treatment, lockup or shape similar to the reference's logo |
| Make fantasy objects: crates, crest targets, lanterns, orbs, dummies, banners, vases, foxes | Use **tin cans, stacked cans, can pyramids**, bottles, or the reference's tower/block silhouettes |
| Show a **vertical path of round crest-shield nodes on an illustrated world backdrop**, with 3-node chapter clusters (§6) | Copy the reference's map presentation: node shapes, path style, camera angle, decorations, chapter framing |
| Compose screenshots around *our* signature: drawn bow + dotted arc + a cut rope / burning fuse / portal mid-shot | Reproduce the reference's screenshot composition, caption style, colour blocking or layout order |
| Write all copy from scratch in our voice (warm, ranger-guild, light humour) | Reuse or paraphrase the reference's store text, level names, button labels, tutorial lines |
| Design levels from our templates (§7) and our own sketches | Trace, re-create or "mirror" any reference level layout or screenshot |
| Use **original music** and original or properly licensed SFX, always processed (D-089) | Use sound-alikes of the reference's SFX/music, ripped audio, or licensed/AI-generated music |

### 2.2 Licensing and provenance (D-089, supersedes D-042)
- Every third-party input (fonts, SFX libraries, Asset Store utility items, AI tools used for concepts/placeholders) is logged in **`docs/art/ASSET_LICENSES.md`**. ART creates this file at the first import. Columns: asset/path, source, licence, commercial OK (Y/N), modified (Y/N + how), attribution required, date, approver.
- **Must be original / custom-made:** bow, arrows, props, UI, logo/wordmark, environments, characters, level compositions, icons, VFX identity and **music**. These define the brand and are never licensed stock or AI output.
- **Fonts:** licensed fonts allowed — OFL (SIL Open Font License) or a purchased commercial licence. Licence text is stored next to the font in `UI/Fonts/<Font>/LICENSE.txt` and recorded in the register.
- **SFX:** custom-recorded/synthesised, or from **licensed SFX libraries** with commercial rights, **always processed/layered** and recorded in the register (source + licence). Never used raw as a signature sound (bow release, chain-reaction sting, UI stars).
- **Music:** original only — commissioned or self-composed, with a work-for-hire / assignment agreement on file (OWNER).
- **AI-generated assets (D-089):** allowed for **internal concepts and temporary placeholders only**. They live under `_Placeholder/` with the `Placeholder` label (so the D-056 build check blocks them from ClosedTest/Release builds) and an `AI` note in `PLACEHOLDER_TRACKER.md`. Never ship AI-generated hero art, logos, characters, environment art, UI, music or competitor-adjacent material unless the tool licence, commercial rights **and** an originality review are documented in the register and signed off by OWNER. AI tools must never be prompted with competitor names or screenshots.
- No reference-game screenshots are stored in the repo. Moodboards use public-domain/self-shot photography and our own sketches only.

### 2.3 Originality review gates

| Gate | When | Reviewers | Checklist |
|---|---|---|---|
| OR-1 Concept | Before the M3 art kit starts | PO + ART | Moodboard sources clean; bow, UI and logo concepts compared against §2.1 |
| OR-2 Art lock | Provisional at G0 (end of M3, after the external VS playtest); final at the start of M6 (art production) | PO + ART + OWNER | Side-by-side with the reference store listing: launcher, frame, wordmark, objects, map, palette dominance |
| OR-3 Store assets | M8 (store assets), re-checked before the M9 submission | PO + MON + OWNER | Icon, screenshots, preview video, feature graphic, store text — no similarity in composition, captions or colour blocking; licence register complete |
| OR-4 Placeholder purge | Every Release build | Automated + ART | Build check (§8) green; `ASSET_LICENSES.md` has no "pending" rows |

---

## 3. Asset naming conventions

These extend [`01` §8](01_TECHNICAL_ARCHITECTURE.md#8-naming-conventions). **World codes: `GW` Greenwood Range, `SC` Sunscar Canyon, `FK` Frostspire Keep** (D-052). Environment art prefabs use `Env_<World>_<Name>`; the world-agnostic graybox blocking set (`Env_<Shape>_<Size>`, `04` §15) gets art variants named `Env_<World>_<Shape>_<Size>` (D-052, in `01` §8).

| Thing | Pattern | Example | Folder |
|---|---|---|---|
| Environment prefab | `Env_<World>_<Name>[_Variant]` | `Env_GW_Platform_Grass_2x1` | `Art/Environments/<World>/Prefabs` |
| Graybox blocking prefab (world-agnostic) | `Env_<Shape>_<Size>` | `Env_Ledge_4x0.5` | `Art/Environments/Shared/Prefabs` |
| Mesh | `SM_<World or Shared>_<Name>` | `SM_Shared_Crate_1x1`, `SM_FK_IceSpire` | `Art/Models/...` |
| Skinned mesh | `SK_<Name>` | `SK_SleepingFox` | `Art/Models/Characters` |
| Animation clip | `ANIM_<Rig>_<Action>` | `ANIM_SleepingFox_Startle` | `Art/Animations` |
| Animator controller | `AC_<Rig>` | `AC_Bow` | `Art/Animations` |
| Texture | `T_<Name>_<Map>` | `T_GW_Palette`, `T_Bow_Moonwood_BaseMap` | `Art/Textures` |
| Render material | `M_<Name>[_Variant]` | `M_GW_Stylized`, `M_Graybox_Required` | `Art/Materials` |
| Shader Graph | `SG_AB_<Name>` | `SG_AB_StylizedLit` | `Art/Shaders` |
| VFX prefab | `VFX_<Event>[_Material]` | `VFX_Break_Timber` | `Prefabs/VFX` |
| VFX texture | `T_VFX_<Name>` | `T_VFX_Atlas_01` | `Art/VFX/Textures` |
| VFX event SO | `VE_<Event>[_Material]` | `VE_Break_Timber` | `ScriptableObjects/VFX` |
| Sound event SO | `SE_<Category>_<Name>` | `SE_Impact_Stone` | `ScriptableObjects/Audio` |
| SFX clip | `SFX_<Category>_<Name>_<nn>` | `SFX_Break_Timber_02.wav` | `Audio/SFX/<Category>` |
| Music | `MUS_<World or Menu>_<Name>` | `MUS_GW_Loop.ogg`, `MUS_Sting_Win.ogg` | `Audio/Music` |
| Ambience | `AMB_<World>_<Name>` | `AMB_SC_WindBed.ogg` | `Audio/Ambience` |
| UI sprite | `UI_<Group>_<Name>` | `UI_Icon_Arrow_Fire`, `UI_Btn_Primary` | `UI/Sprites/<Group>` |
| Sprite atlas | `SA_<Group>` | `SA_HUD`, `SA_Meta` | `UI/Atlases` |
| Font asset | `F_<Family>_<Weight>[_SDF]` | `F_Display_Bold_SDF` | `UI/Fonts` |
| Cosmetic asset | `CD_<Slot>_<Name>` (SO), visuals `SM_Bow_<Name>`, `VFX_Trail_<Name>` | `CD_Bow_Moonwood` | `ScriptableObjects/Cosmetics` |
| Placeholder | any of the above with the `PH_` prefix **and** the `Placeholder` asset label (build check, D-056) | `PH_SFX_Impact_Generic_01` | `Art/_Placeholder/...`, `Audio/_Placeholder/...` |
| Marketing | `MKT_<Store>_<Type>_<Locale>_<nn>` | `MKT_iOS_Screenshot_en_01.png` | `Marketing/` (outside `Assets/`) |

Source files (`.blend`, `.psd`, layered audio sessions) live under `ArtSource/` at the repo root (outside `Assets/`, LFS-tracked). Only exported runtime files enter `Assets/_Project`.

---

## 4. Asset list

Columns: **ID** (canonical) · **Description** · **States / variants** (≤ 3 visual states, §4) · **Budget** (tris / texture) · **Needed** (milestone the production asset must be in, per the D-102 milestone structure: VS assets M3; final world art, VFX, SFX, music and meta UI M6; consent/IAP/ads UI M7; store/marketing M8–M9. Graybox stand-ins (`PH_` assets, `M_Graybox_*`) cover M4 systems work and M5 content graybox) · **Priority** (P0 must / P1 should / P2 could).

### 4.1 Environment kits (~25 pieces per world)

The kits share one structure. Gameplay-relevant static geometry (`Environment` layer) uses simple box colliders authored by PHYS/LEVEL. Art kits are visual skins over those collider standards (1×1, 2×1, 4×1 m grid; heights in 0.5 m steps).

#### Greenwood Range (`GW`) — ranger training valley: lush grass, ruins, bright daytime

| ID | Description | States / variants | Budget | Needed | Pri |
|---|---|---|---|---|---|
| `T_GW_Palette` | World palette 256² | — | 192 KB | M3 | P0 |
| `Env_GW_<Shape>_<Size>` | Greenwood art variants of every `04` §15 blocking prefab used by the VS (`Ground_12x1`, `Ledge_4x0.5`, `Ledge_6x0.5`, `Beam_6x0.4`, `Stump`, `Awning_6`, `Perch_1.5`); the rest of the blocking set in M6 | 1 each | ≤ 400 each | M3 (VS set) / M6 | P0 |
| `Env_GW_Backdrop_TrainingGrounds` | VS backdrop composition ("Ranger Training Grounds": valley, ruined arch, archery butts, fence, tree clusters, BG card, sky) built from kit pieces | 2 variants | ≤ 40 k tris total | M3 | P0 |
| `Env_GW_Ground_Grass_4x1` | Grass-topped earth slab | 4×1, 2×1, 1×1 | ≤ 300 | M3 | P0 |
| `Env_GW_Platform_Stone_2x1` | Ruined stone ledge (static platform) | 1×1, 2×1, 3×1 | ≤ 400 | M3 | P0 |
| `Env_GW_Platform_Wood_2x1` | Wooden scaffold platform | 2×1, 3×1 | ≤ 400 | M3 | P0 |
| `Env_GW_Pillar_Ruin` | Static ruin pillar | short/tall | ≤ 500 | M3 | P0 |
| `Env_GW_Cliff_Edge` | Cliff edge framing the board sides | L/R | ≤ 1,500 | M3 | P0 |
| `Env_GW_RopeAnchor_Beam` | Overhead beam / branch for rope anchors | straight/branch | ≤ 400 | M3 | P0 |
| `Env_GW_BowStand` | Foreground carved stump/stand under the bow (bottom-centre) | 1 | ≤ 1,500 | M3 | P0 |
| `Env_GW_Foreground_Grass` | Foreground grass tufts framing the bottom edge (outside the aim zone's visual centre) | 3 variants | ≤ 300 each | M3 | P1 |
| `Env_GW_Tree_Round` | Lollipop toy tree | 3 sizes | ≤ 600 | M3 | P0 |
| `Env_GW_Bush` | Bush clusters | 3 variants | ≤ 200 | M3 | P1 |
| `Env_GW_Rock` | Rounded rocks | 4 variants | ≤ 150 | M3 | P1 |
| `Env_GW_Fence` | Training-yard fence | straight/broken | ≤ 200 | M6 | P1 |
| `Env_GW_Banner_Deco` | Guild pennant (green/cream — **not red**) | 2 | ≤ 150 | M6 | P2 |
| `Env_GW_TargetRack_Deco` | Decorative archery butts (desaturated, no red) | 1 | ≤ 400 | M6 | P2 |
| `Env_GW_RuinArch` | Mid-ground ruined arch | 1 | ≤ 1,200 | M6 | P1 |
| `Env_GW_Watchtower_Deco` | Background watchtower silhouette | 1 | ≤ 1,000 | M6 | P1 |
| `Env_GW_Waterfall` | Background waterfall (scrolling UV) | 1 | ≤ 400 | M6 | P2 |
| `Env_GW_Flowers` | Flower clusters (low-sat) | 3 | ≤ 100 | M6 | P2 |
| `Env_GW_Mushrooms` | Mushroom clusters | 2 | ≤ 100 | M6 | P2 |
| `Env_GW_Signpost` | Wooden signpost (no text) | 1 | ≤ 200 | M6 | P2 |
| `T_GW_BG_Near/Mid/Far` | 3 pre-blurred background cards (hills, forest, mountains) | — | ≤ 1024² ASTC 6×6 | M3 (1 card) / M6 | P0 |
| `T_GW_Sky` | Sky gradient + clouds | — | 256² | M3 | P0 |
| `Env_GW_Clouds` | Low-poly cloud puffs, slow drift | 3 | ≤ 150 | M6 | P2 |
| `AMB_GW_Meadow` | Ambience bed (birds, breeze) | — | — | M6 | P1 |

#### Sunscar Canyon (`SC`) — desert caravan outpost: orange stone, wind, sunset

| ID | Description | States / variants | Budget | Needed | Pri |
|---|---|---|---|---|---|
| `T_SC_Palette` | World palette | — | 192 KB | M6 | P0 |
| `Env_SC_Ground_Sand_4x1` | Sand/sandstone slab | 4×1, 2×1, 1×1 | ≤ 300 | M6 | P0 |
| `Env_SC_Platform_Mesa_2x1` | Layered mesa ledge | 1×1, 2×1, 3×1 | ≤ 400 | M6 | P0 |
| `Env_SC_Platform_CaravanCart` | Wagon-bed platform (static) | 1 | ≤ 900 | M6 | P1 |
| `Env_SC_Pillar_Hoodoo` | Rock hoodoo pillar | short/tall | ≤ 500 | M6 | P0 |
| `Env_SC_Cliff_Edge` | Canyon wall framing | L/R | ≤ 1,500 | M6 | P0 |
| `Env_SC_RopeAnchor_Awning` | Awning pole / beam anchor | 2 | ≤ 400 | M6 | P0 |
| `Env_SC_BowStand` | Carved sandstone bow stand | 1 | ≤ 1,500 | M6 | P0 |
| `Env_SC_Cactus` | Toy cactus | 3 | ≤ 300 | M6 | P1 |
| `Env_SC_Tent` | Caravan tent (striped, low-sat) | 2 | ≤ 800 | M6 | P1 |
| `Env_SC_Crates_Deco` | Desaturated cargo (no red bands) | 2 | ≤ 300 | M6 | P2 |
| `Env_SC_Windmill_Deco` | Background windmill (wind motif) | 1 | ≤ 800 | M6 | P2 |
| `Env_SC_Gate_Siege` | L40 caravan siege gate set piece | 1 | ≤ 3,000 | M6 | P0 |
| `Env_SC_Rock` | Rounded boulders (static deco) | 4 | ≤ 150 | M6 | P1 |
| `Env_SC_Bones_Deco` | Cartoon bleached bones (friendly) | 2 | ≤ 150 | M6 | P2 |
| `Env_SC_Lanterns_String` | String of decorative paper lights (gold, non-interactive, **not red**) | 1 | ≤ 300 | M6 | P2 |
| `Env_SC_Dunes` | Mid-ground dune shapes | 2 | ≤ 600 | M6 | P1 |
| `Env_SC_ShieldTrack` | Visual rail/track for moving shields (telegraphs the path) | straight/arc | ≤ 300 | M6 | P0 |
| `Env_SC_BoulderLane` | Ramp/lane guide pieces for boulder runs | ramp/curve | ≤ 400 | M6 | P0 |
| `Env_SC_LeverPivot` | Static pivot block for levers (D-022) | 1 | ≤ 300 | M6 | P0 |
| `T_SC_BG_Near/Mid/Far` | Canyon/sunset background cards | — | ≤ 1024² | M6 | P0 |
| `T_SC_Sky` | Sunset gradient | — | 256² | M6 | P0 |
| `Env_SC_Dust_Wisp` | Background dust wisps (VFX-light) | — | ≤ 20 particles | M6 | P2 |
| `Env_SC_Flag` | Wind-indicator flag (animated vertex wave) | 1 | ≤ 200 | M6 | P1 |
| `AMB_SC_WindBed` | Wind + distant camel bells (original) | — | — | M6 | P1 |

#### Frostspire Keep (`FK`) — icy mountain fortress: blue ice, aurora, snow

| ID | Description | States / variants | Budget | Needed | Pri |
|---|---|---|---|---|---|
| `T_FK_Palette` | World palette | — | 192 KB | M6 | P0 |
| `Env_FK_Ground_Snow_4x1` | Snow-capped slab | 4×1, 2×1, 1×1 | ≤ 300 | M6 | P0 |
| `Env_FK_Platform_Rampart_2x1` | Fortress rampart ledge | 1×1, 2×1, 3×1 | ≤ 400 | M6 | P0 |
| `Env_FK_Platform_IceShelf` | Static ice shelf (slippery look) | 2 | ≤ 400 | M6 | P0 |
| `Env_FK_Pillar_Tower` | Fortress pillar/turret | short/tall | ≤ 600 | M6 | P0 |
| `Env_FK_Cliff_Edge` | Glacier wall framing | L/R | ≤ 1,500 | M6 | P0 |
| `Env_FK_RopeAnchor_Chain` | Iron anchor bracket | 2 | ≤ 300 | M6 | P0 |
| `Env_FK_BowStand` | Frost-stone bow stand | 1 | ≤ 1,500 | M6 | P0 |
| `Env_FK_Pine_Snow` | Snowy toy pine | 3 | ≤ 500 | M6 | P1 |
| `Env_FK_IceSpire` | Crystal spires (static deco, **not** cyan-bright like gameplay ice) | 3 | ≤ 400 | M6 | P1 |
| `Env_FK_Brazier_Deco` | Warm brazier (emissive, contrast) | 1 | ≤ 300 | M6 | P2 |
| `Env_FK_WindFanMount` | Mount for wind fans | 1 | ≤ 300 | M6 | P0 |
| `Env_FK_PortalPlinth` | Plinth/frame mount for portal rings | 1 | ≤ 400 | M6 | P0 |
| `Env_FK_IceSlide` | Static ice slide/ramp kit that replaces moving ice platforms in L53–55 (D-101) | straight / curved | ≤ 300 | M6 | P0 |
| `Env_FK_MoverRail` | Rail for moving ice platforms — **cut (D-104)**; not produced | straight | ≤ 300 | — | Cut |
| `Env_FK_Banner_Deco` | Fortress banners (blue/silver) | 2 | ≤ 150 | M6 | P2 |
| `Env_FK_FrostSeal_Tower` | L60 finale tower set piece | 1 | ≤ 4,000 | M6 | P0 |
| `Env_FK_SnowDrift` | Mid-ground snow drifts | 2 | ≤ 400 | M6 | P1 |
| `Env_FK_Icicles` | Hanging icicles (deco) | 2 | ≤ 150 | M6 | P2 |
| `Env_FK_Gate` | Background keep gate | 1 | ≤ 1,200 | M6 | P1 |
| `T_FK_BG_Near/Mid/Far` | Mountains + keep cards | — | ≤ 1024² | M6 | P0 |
| `T_FK_Sky_Aurora` | Night-blue gradient + aurora band (animated UV) | — | 256² | M6 | P0 |
| `Env_FK_Snowfall` | Light snowfall particles (bg only) | — | ≤ 40 particles | M6 | P2 |
| `AMB_FK_Howl` | Wind howl + creaks | — | — | M6 | P1 |

### 4.2 Structures (materials, shared across worlds; world skins are prefab variants per `01` §12)

| ID | Description | States (≤ 3) | Budget | Needed | Pri |
|---|---|---|---|---|---|
| `Struct_Crate_Timber_1x1` / `_2x1` | Timber crate | intact / cracked / broken (→ debris) | ≤ 300 | M3 | P0 |
| `Struct_Post_Timber_0.5x2` | Timber stilt/post (VS-02 weak support) | intact / cracked / broken | ≤ 150 | M3 | P0 |
| `Struct_Plank_Timber_4x0.25` | Timber plank | intact / cracked / broken | ≤ 150 | M3 | P0 |
| `Struct_Beam_Timber_3x0.5` | Timber beam | intact / cracked / broken | ≤ 200 | M3 | P0 |
| `Struct_Peg_Timber` | Timber peg (holds boulders, §7 "Boulder Run") | intact / broken | ≤ 150 | M6 | P0 |
| `Struct_Bale_Straw_1x1`, `Struct_Screen_Straw_1x2` | Straw bale / wicker screen | intact / pierced / burst | ≤ 300 | M6 | P0 |
| `Struct_Block_Stone_1x1`, `Struct_Beam_Stone_3x0.5`, `Struct_Cover_Stone_1.5x1` | Stone block / beam / shielded-target cover | intact / chipped / broken (high HP) | ≤ 300 | M6 | P0 |
| `Struct_Block_Ice_1x1`, `Struct_Slab_Ice_2x0.5` | Ice crystal block / slab (`SG_AB_Ice`) | intact / cracked / shattered | ≤ 300 | M6 | P0 |
| `Struct_Plate_Metal_2x0.25`, `Struct_Plate_Metal_Marked` | Dark steel with gold trim (ricochet surface); the marked variant is the Bounce bank plate (target-ring glyph) | intact only (+ spark on hit) | ≤ 300 | M6 | P0 |
| `Struct_Lever_Timber_4x0.5` | Lever plank on hinge (D-022) | intact / broken | ≤ 250 | M6 | P0 |
| `Struct_Bridge_Timber` | Rope-held drawbridge plank | intact / broken | ≤ 300 | M6 | P1 |
| `Debris_<Material or Object>_<nn>` | 3–6 fragment meshes per material, plus object-specific sets for crest, vase, lantern, orb, jar, barrel (e.g. `Debris_Timber_01`, `Debris_Vase_02`) | — | 20–80 each | M3 (timber, crest, vase) / M6 | P0 |

### 4.3 Required objectives (§4)

| ID | Description | States (≤ 3) | Budget | Needed | Pri |
|---|---|---|---|---|---|
| `Obj_CrestTarget` | Red crest shield with target rings + crest icon (outline) | intact / cracked (optional) / shattered | ≤ 800 | M3 | P0 |
| `Obj_SupplyCrate_1x1`, `Obj_SupplyCrate_2x1` | Crate with red bands + crest stencil | intact / cracked / broken | ≤ 500 | M6 (graybox M2) | P0 |
| `Obj_HangingLantern` | Lantern with red glass, emissive glow | lit / cracked / broken | ≤ 800 | M6 | P0 |
| `Obj_CursedOrb` | Floating red-black orb with rune ring and slow pulse (direct-hit-only tell: **runic eye glyph**) | pulsing / shattered | ≤ 800 | M6 | P0 |
| `Obj_TrainingDummy` | Straw dummy with red sash, comedic face | idle wobble / hit squash / knocked down | ≤ 1,500 | M6 | P0 |
| `Obj_CursedOrb_FrostSeal` | L60 frost-seal variant of the orb (ice-crusted rune ring) | pulsing / shattered | ≤ 1,000 | M6 | P0 |
| `Obj_BannerRope` | Rope with a red banner tag marking it as required | intact / cut | ≤ 300 + rope line | M6 | P0 |
| `UI_Icon_Obj_<Kind>` | HUD icons for each objective kind (shape-distinct) | normal / cleared (crossed) | 128² in atlas | M2 graybox, M3 final | P0 |

### 4.4 Interactive props (§4)

| ID | Description | States (≤ 3) | Budget | Needed | Pri |
|---|---|---|---|---|---|
| `Prop_Rope` | Hemp rope (LineRenderer + gold anchor tags) | intact / burning / cut | line + ≤ 200 tags | M3 | P0 |
| `Prop_Rope_Chain` | Chain (gold-trim links visual) | intact / cut | line + ≤ 300 | M6 | P1 |
| `Prop_BalloonCluster_2`, `Prop_BalloonCluster_3` | 2 or 3 gold/green balloons with basket hook | full / partially popped / empty | ≤ 200 per balloon | M6 | P0 |
| `Prop_OilJar` | Green-glazed jar with oil-drop icon | intact / broken (→ oil puddle + FireZone) | ≤ 600 | M6 | P0 |
| `Prop_StrawWick` | Straw wick/fuse that carries fire to a rope (`Burnable`) | intact / burning / burnt | ≤ 200 | M6 | P0 |
| `Prop_PowderBarrel` | Barrel with gold bands + spark icon | idle / fuse lit / exploded | ≤ 700 | M6 | P0 |
| `Prop_Boulder` | Round stone boulder with moss/sand/snow skin per world | resting / rolling (dust VFX) | ≤ 500 | M6 | P0 |
| `Prop_SpringPlate` | **Cut from the launch MVP (D-086).** No art is produced. | — | — | — | Cut |
| `Prop_WindFan` | Carved fan with spinning blades + streak VFX | on (always) | ≤ 900 | M6 | P0 |
| `Prop_Shield_Rotating` | Metal shield on a pivot (KinematicMover) | moving only | ≤ 600 | M6 | P0 |
| `Prop_Shield_Patrol` | Metal shield on a track | moving only | ≤ 600 | M6 | P0 |
| `Prop_Platform_Moving_Ice` | Moving ice platform (KinematicMover) — **cut (D-104)**: L53–55 use static ice slides (`Env_FK_IceSlide`, `Struct_Slab_Ice_*`) and moving shields instead | moving only | ≤ 400 | — | Cut-first |
| `Prop_PortalPair` (2 × ring) | Portal pair; **pairs distinguished by colour (gold/green/teal) + glyph (circle/triangle/diamond)** | idle / arrow passing | ≤ 900 each | M6 | P0 |
| `Marker_Bullseye` | Gold ring decal on a weak point (`BullseyeMarker`) | idle / hit flash | ≤ 100 | M6 | P1 |

### 4.5 Protected objects and hazards (§4)

| ID | Description | States (≤ 3) | Budget | Needed | Pri |
|---|---|---|---|---|---|
| `Prot_RoyalVase` | Ornate vase with purple halo ring + crown icon | intact / broken (fail) | ≤ 1,200 | M3 (VS) | P0 |
| `Prot_SleepingFox` | Original friendly fox curled asleep on a ledge, purple halo + "Zz" | asleep / startled (fail) / knocked (fail) | ≤ 3,000 skinned | M6 | P0 |
| `Prot_RoyalRelic` | L60 relic (crowned crystal reliquary) | intact / broken | ≤ 2,000 | M6 | P0 |
| `Haz_WaterPit` | Water strip with foam edge + purple edge posts | idle / splash | ≤ 800 + shader | M3 (VS-05) | P0 |
| `Haz_SpikeBed` | Spikes with purple warning bands | idle / crunch | ≤ 1,000 | M6 | P0 |
| `Haz_Pit` | Dark void edge with purple marker stones | idle | ≤ 500 | M3 (VS-02/04) | P0 |
| Shielded target | No new asset: an `Obj_CrestTarget` behind displaceable `Struct_*` cover (`03`) | — | — | M6 | — |

### 4.6 Bow, arrows, characters

| ID | Description | States / variants | Budget | Needed | Pri |
|---|---|---|---|---|---|
| `SM_Bow_OakRanger` + `AC_Bow` | Hero bow (default skin): carved oak, gold inlay, string glow at full draw | rest / drawing (blend 0–1) / release snap | 4–6k, 512² | M3 | P0 |
| Gloved draw hand (optional) | Stylised leather glove pinching the string; **no full ranger avatar in MVP** (D-053, proposed) | rest / draw | ≤ 1,500 | M6 | P2 |
| `Arrow_Oak` | Oak shaft, green fletching | flying / embedded / spent | ≤ 250 | M1 graybox, M3 | P0 |
| `Arrow_Heavyhead` | Thick shaft, iron bulb head, gray fletching | same | ≤ 300 | M6 | P0 |
| `Arrow_Split` | Three-pronged head, gold fletching; children are slim | same + split | ≤ 300 | M6 | P0 |
| `Arrow_Fire` | Wrapped head with ember glow (flame VFX) | same | ≤ 300 | M6 | P0 |
| `Arrow_Bounce` | Rounded steel-cap head, cyan fletching | same | ≤ 300 | M6 | P0 |
| `SK_SleepingFox` anims | `ANIM_SleepingFox_Sleep` (breathing loop), `_Startle`, `_Tumble` | 3 clips | — | M6 | P0 |
| `SK_TrainingDummy` anims | `_Wobble`, `_HitSquash`, `_FallOver` (vertex/transform anim OK) | 3 | — | M6 | P1 |
| Mascot (marketing) | The fox as the brand mascot for icon/store art | poses for marketing | — | M8 | P1 |

### 4.7 Cosmetics (§5) — must not change physics, aim, damage or win chance

| ID | Description | Variants | Budget | Unlock (see `06`) | Needed | Pri |
|---|---|---|---|---|---|---|
| `CD_Bow_OakRanger` | Default bow | — | 4–6k | Start | M3 | P0 |
| `CD_Bow_Moonwood` | Pale silver wood, moon-phase inlays, soft blue string glow | — | 4–6k | World 1 completion reward, or 1,200 coins earlier (D-064, D-101) | M6 | P0 |
| `CD_Bow_Ember` | Charred wood with ember cracks, orange glow | — | 4–6k | **Post-MVP content update (D-101)** — not in the launch set | — | Deferred |
| `CD_Bow_Frostglass` | Ice-glass limbs, frost runes | — | 4–6k | **Post-MVP content update (D-101)** — not in the launch set | — | Deferred |
| `CD_Bow_RoyalAmethyst` | Amethyst lacquer + gold filigree (renamed from "Royal Violet", D-095; the bow is never in the play field) | — | 4–6k | Unlocked by the Cosmetic Starter Pack (D-091) or 2,500 coins (confirmed, D-104) — never marketed as exclusive | M6 | P0 |
| `CD_Trail_GoldSpark` | Gold spark trail | — | ≤ 30 particles | Cosmetic Starter Pack (D-091), or 1,000 coins | M6 | P0 |
| `CD_Trail_LeafSwirl` | Leaf swirl trail | — | ≤ 30 | 600 coins | M6 | P0 |
| `CD_Trail_CyanStreak` | Cyan streak ribbon | — | ribbon | **Post-MVP content update (D-101)** | — | Deferred |
| `CD_Trail_EmberAsh` | Ember + ash trail | — | ≤ 30 | **Post-MVP content update (D-101)** | — | Deferred |
| `CD_Badge_W1Clear…W3Clear`, `CD_Badge_W1Perfect…W3Perfect`, `CD_Badge_Bullseye10/30/All`, `CD_Badge_Daily7` (IDs per `06` §4.3) | Quiver badges (completion rewards only, §5) | 10 badges | 256² UI icon + 300-tri quiver charm | Completion | M6 | P1 |
| `UI_Cosmetic_Thumb_<Id>` | Bow Forge thumbnails (rendered) | per cosmetic | 256² | — | M6 | P0 |

> Trails are rendered **behind** the arrow at ≤ 60% opacity and never obscure the trajectory preview or targets. Trails are disabled while drawing, and colour-assist mode can force a neutral trail (§15).

### 4.8 UI art

| ID | Description | Variants | Budget | Needed | Pri |
|---|---|---|---|---|---|
| `F_Display_*`, `F_Body_*` | Display font (headings/numbers) + body font, OFL/licensed, TMP SDF assets with fallback | Regular/Bold | ≤ 2 SDF atlases 1024² | M3 | P0 |
| `UI_Btn_Primary/Secondary/Icon` | Rounded shield buttons: carved wood rim, leaf-green (primary), parchment (secondary) | normal / pressed / disabled | 9-slice in `SA_Common` | M3 | P0 |
| `UI_Panel_Parchment` | Panel background (9-slice), gold trim corners — **no purple frame** | 2 sizes | 9-slice | M3 | P0 |
| `UI_Icon_*` | Pause, restart, settings, back, home, shop, forge, daily, close, sound, music, haptics, info, lock, coin, star (empty/full), medal, ad (video glyph), check | normal / pressed | 128² each | M2 graybox, M3 final | P0 |
| `UI_Icon_Arrow_<Type>` | HUD quiver icons per arrow type (shape-distinct heads) | full / spent / next-highlight | 128² | M3 (Oak) / M6 | P0 |
| `UI_Star_Big` + `VFX_UI_StarBurst` | Win-screen stars | empty / filling / full | 256² | M3 | P0 |
| `UI_Map_Node` | Level node (round crest-shield) | locked / current / cleared (+ 0–3 star pips, medal pip) | 256² | M6 | P0 |
| `UI_Map_Backdrop_<World>` | Illustrated vertical world-map backdrops (tiling vertically, 3-node chapter clusters) | per world | 1024×2048 ×2–3 tiles, ASTC 6×6 | M6 | P0 |
| `UI_Map_LockedTeaser_<World>` | Locked world teaser card | — | 512² | M6 | P1 |
| `UI_Logo_Wordmark` | Original "Arrow Buster" wordmark (public name per D-081; legal/store/domain clearance before store-submission assets) | light / dark | vector source; 1024² export | M3 (temp), M8 final | P0 |
| `UI_Splash` | **Static** branded splash: drawn bow + wordmark on the Greenwood palette. The animated bow-draw/impact splash is **cut (D-101)**. | static | — | M2 (temp), M6 (final) | P0 |
| `UI_ArrowRevealCard_<Type>` | Card art for each special arrow's first appearance | 4 | 512² | M6 | P0 |
| `UI_TutorialHand` | Ghost hand sprite for the L1 drag demo | — | 256² | M3 | P0 |
| `UI_Callout_Bubble` | "Aim for the rope" bubble with pointer tail | — | 9-slice | M3 | P0 |
| `UI_Shop_StarterPack` | Starter Pack offer art: Royal Amethyst bow + Gold Spark trail + 500 coins, "Cosmetic only" badge (D-091) | — | 1024×512 | M7 | P0 |
| `UI_Icon_App` | App icon (see §4.10) | — | — | M8 | P0 |

### 4.9 VFX — see §9 for timing and budgets

### 4.10 Marketing (M8 unless noted; captures refreshed from the M9 closed-test build; verify current store specs at M8 — they change)

| ID | Description | Variants | Spec (verify at M8) | Needed | Pri |
|---|---|---|---|---|---|
| `MKT_Icon` | Fox mascot or drawn-bow motif on a leaf-green field, no text | iOS 1024² master; Android adaptive foreground/background 432² (108 dp) + 512² Play icon | No alpha on iOS | M8 (concepts M6) | P0 |
| `MKT_iOS_Screenshot_*` | 5–8 portrait screenshots: signature shots (rope cut, collapse, fire, portal, 3★ win) with short original captions | 6.9" + 6.5" iPhone; iPad 13" if universal (A-03) | Current App Store sizes | M8 | P0 |
| `MKT_GP_Screenshot_*` | Same set for Google Play | phone 9:16; 7"/10" tablet | ≥ 1080 px short side | M8 | P0 |
| `MKT_GP_FeatureGraphic` | 1024×500 key art (bow + collapsing structure + wordmark) | — | 1024×500 | M8 | P0 |
| `MKT_iOS_PreviewVideo` | 15–30 s portrait gameplay capture, no device frame, starts on a shot within 2 s | 1–3 | App Store preview spec | M8 | P1 |
| `MKT_GP_PromoVideo` | YouTube-hosted cut of the same | 1 | — | M8 | P2 |
| `MKT_KeyArt` | Hi-res key art for press/socials | portrait + landscape | 4K | M8 | P2 |

### 4.11 Audio — see §10 (SFX plan) and §11 (music). Asset count summary:

| Category | Clips (incl. variations) | Needed |
|---|---|---|
| Bow & arrows | ~30 | M3 (Oak), M6 (specials; graybox `PH_` sounds from M4) |
| Material impacts/breaks (5 materials) | ~40 | M3 (timber), M6 |
| Objectives/protected/characters | ~18 | M3 / M6 |
| Props/hazards | ~30 | M6 |
| UI | ~20 | M3 (core), M6 (meta) |
| Music (loops + stings) — **original only (D-089)** | 4 loops + 6 stings + chain layer | M3 (GW loop), M6 |
| Ambience | 3 beds | M6 |

---

## 5. Character presence decision
- **No playable ranger avatar in MVP** (D-053, proposed). The player *is* the archer. The bow, an optional gloved draw hand (P2) and the UI voice carry the fantasy. This saves rigging/animation cost and keeps the bottom of the screen uncluttered (§8 "Bottom: bow… no clutter").
- Characters on the board: **Sleeping Fox** (protected, the brand mascot for marketing) and **Training Dummy** (objective, comedic). Both are original designs: the fox is a rounded toy-like silhouette with an oversized tail and a leaf on its ear (Greenwood tie-in).

---

## 6. Placeholder asset strategy

| Rule | Detail |
|---|---|
| Location | `Assets/_Project/Art/_Placeholder/` and `Assets/_Project/Audio/_Placeholder/` only |
| Naming | `PH_` prefix + the final asset's intended name (`PH_Struct_Crate_Timber_1x1` mesh, `PH_SFX_Impact_Generic_01`) |
| Labelling | Unity asset label **`Placeholder`** (applied automatically by `AssetImportRules` for anything under `_Placeholder`) |
| Graybox materials | `M_Graybox_Required` (red), `M_Graybox_Protected` (purple), `M_Graybox_Interactive` (gold), `M_Graybox_Structure_<Material>` (§4 hues: straw yellow, timber brown, stone gray-blue, ice cyan, metal dark steel), `M_Graybox_Environment` (neutral gray-green) — graybox **already obeys the colour language** so playtests are valid |
| Swap mechanism | Gameplay prefabs reference visuals through a child `Visual` object. Art replaces only that child (or the variant's mesh/material). Colliders, mass, layer and components never change (enforced by `PrefabValidator`, `01` §12). Audio/VFX swap by changing `SoundEvent` / `VfxEvent` assets, never code. |
| Build check | `BuildScript` scans the build's dependency list. **Release/ClosedTest builds fail** if any dependency has the `Placeholder` label or a `_Placeholder` path. **Dev builds warn** and list them in the build report. |
| Tracking | `docs/art/PLACEHOLDER_TRACKER.md` (ART, created at M3): asset, used by, replacement owner, target milestone, AI-generated (Y/N) |
| AI-generated placeholders (D-089) | Allowed for internal concepts and temporary placeholders only. Same `_Placeholder` location, `PH_` prefix and `Placeholder` label, so the build check keeps them out of ClosedTest/Release builds. Never promoted to final without the documented licence + originality review. |

---

## 7. Import rules (enforced by `AssetImportRules : AssetPostprocessor`, `Scripts/Editor/Validation`)

The postprocessor applies presets by **folder + name prefix**. Divergent settings are reset on reimport and logged as warnings. A Unity Preset asset per category lives in `Assets/_Project/Art/ImportPresets/` for manual reference.

### 7.1 Textures

| Category (path/prefix) | Max size | Format iOS / Android | Mipmaps | Filter | Other |
|---|---|---|---|---|---|
| World palettes `T_*_Palette` | 256 | RGB24 uncompressed (both) | **Off** | Bilinear | sRGB on, wrap Clamp |
| Hero bow `T_Bow_*` | 512 | ASTC 6×6 | On | Trilinear, aniso 2 | Mask maps linear (sRGB off) |
| Environment textures (non-palette) | 1024 | ASTC 6×6 | On | Bilinear | — |
| Background cards `T_<W>_BG_*` | 1024 | ASTC 8×8 | On | Bilinear | Wrap Clamp |
| Sky `T_*_Sky*` | 256 | ASTC 6×6 | Off | Bilinear | — |
| Normal maps (rare) | 512 | ASTC 5×5 | On | Trilinear | Texture type Normal map |
| VFX `T_VFX_*` | 512 (atlas), 256 single | ASTC 6×6 (ASTC 4×4 for sharp sparks) | On | Bilinear | Alpha is transparency |
| UI sprites `UI_*` | 1024 (2048 for atlases) | ASTC 4×4 (icons/text-like), 6×6 (backdrops) | **Off** | Bilinear | Sprite (2D and UI), packed into `SA_*` atlases, Read/Write off |
| Map backdrops `UI_Map_Backdrop_*` | 2048 | ASTC 6×6 | Off | Bilinear | Not atlased |
| Fonts (TMP SDF) | 1024 | Alpha8 | Off | Bilinear | — |

All textures: **Read/Write off**, Streaming Mipmaps on for environment/background (Low tier mip limit 1), `globalTextureMipmapLimit` controlled by tier. Android fallback when ASTC is unsupported: ETC2 (project default). PLAT verifies the low-tier devices support ASTC (Mali-G52-class and up do).

### 7.2 Models

| Setting | Value |
|---|---|
| Scale / units | Scale factor 1, convert units on, **1 unit = 1 m**, Y-up, pivot at bottom-centre for structures (centre-of-mass handled by the Rigidbody) |
| Mesh compression | **Medium** for environment/props/structures; **Low** for hero bow and fox; Off for debris (tiny) |
| Read/Write | **Off** (except meshes flagged for runtime combine — none planned) |
| Optimize Mesh | On (polygon + vertex order) |
| Generate colliders | **Off** — colliders are primitive colliders authored on the prefab |
| Normals / tangents | Import normals (smoothing groups authored in DCC); tangents **None** unless normal-mapped |
| Blend shapes | Off unless the asset needs them (none planned) |
| Import cameras / lights | Off |
| Materials | Material creation mode **None**; assign project materials on the prefab |
| Animation | Rig None for static meshes. Generic for fox/bow/dummy. Anim compression Optimal; resample curves on. |
| LODs | None (fixed camera, small scene). Background meshes are authored low-poly instead. |
| Index format | Auto (16-bit expected) |

### 7.3 Audio

| Category (folder/prefix) | Load type | Compression | Quality | Sample rate | Channels | Preload | Notes |
|---|---|---|---|---|---|---|---|
| Short SFX < 1.5 s (`SFX_Impact_*`, `SFX_Break_*`, `SFX_Bow_*`, `SFX_UI_*`) | **Decompress On Load** | **ADPCM** | — | Override **32 kHz** (keep 44.1 kHz for ice/metal/chime sets with high-frequency content) | **Force Mono** | On | Low latency, low CPU |
| Medium SFX 1.5–5 s (explosion, fuse, stings) | Compressed In Memory | Vorbis | 70 | 44.1 kHz | Mono (stings stereo) | On | — |
| Loops (whistle, boulder roll, wind fan, fire crackle, fox snore) | Compressed In Memory | Vorbis | 60 | 32 kHz | Mono | On | Seamless loop points |
| Ambience beds `AMB_*` | Streaming | Vorbis | 50 | 44.1 kHz | Stereo | Off | Load In Background |
| Music `MUS_*_Loop` | **Streaming** | Vorbis | 55 | 44.1 kHz | Stereo | Off | Load In Background; ≤ 2 streams at once (crossfade) |
| Music stings `MUS_Sting_*` | Compressed In Memory | Vorbis | 70 | 44.1 kHz | Stereo | On | — |

Source format: WAV 48 kHz / 24-bit in `ArtSource/Audio` (LFS); exported runtime files at 44.1 kHz/16-bit WAV (SFX) or OGG (music) in `Assets/_Project/Audio`. Loudness targets: SFX peaks ≤ −1 dBTP. Music integrated ≈ −16 LUFS (mobile), ambience ≈ −24 LUFS.

---

## 8. Feedback architecture recap (owner ART, code per `01` §7)

`FeedbackDirector` subscribes to `GameEvents` (`01` §10.2) and maps each event to `SoundEvent` + `VfxEvent` + `HapticKind` + optional `HitStop`/`CameraShake` via a data table (`FeedbackMap` section of `SoundLibrary`). Gameplay never calls audio/VFX/haptics directly. All feedback reads Settings (music/SFX/haptics/reduced particles/reduced motion) at call time.

---

## 9. VFX and destruction feedback plan

### 9.1 Principles
1. **Outcome first.** The key outcome (target shattered, rope cut, vase intact/broken) must be readable *before* any obscuring effect. Opaque smoke/dust spawns ≥ 0.15 s after the outcome event and never inside a 1.2 m radius of an uncleared objective or protected object. Explosion flashes are additive, short (≤ 0.1 s) and never full-screen.
2. **Effects describe material.** Splinters = timber, straw puffs = straw, gray grit = stone, glints = ice, sparks = metal (§4).
3. **Debris is physics, not particles.** Structural break fragments are `DebrisPool` meshes (D-008). Particles add only dust/spark accents.
4. **Budgets:** ≤ 600 live particles (Mid/High), ≤ 300 (Low). Each effect also has a cap (table). Reduced Particles = ×0.4 counts, no smoke, no screen-space flashes.
5. **Pooling:** `VfxService` pools per `VfxEvent` (prewarm 2–4, max 6; overflow = skip + dev warning, `01` §13).
6. **No flashing** more than 3 times per second (photosensitivity).

### 9.2 Event table

| Gameplay event (`GameEvents`) | VFX id | Duration | Max particles (Mid / Low) | Readability rule | Needed |
|---|---|---|---|---|---|
| Full draw reached | `VFX_Bow_FullDrawGlow` (string glow + 6 motes) | while held | 12 / 6 | Never overlaps the preview dots | M3 |
| Trajectory preview (while drawing, D-085) | `VFX_Preview_ImpactRing`, `VFX_Preview_BounceMark` (first-bounce point + reflected dotted segment), `VFX_Preview_SplitMark` (Split marker + 3 child arcs), `VFX_Preview_PortalExit` (exit-ring highlight, path continues) | while drawing | dots pooled (40) + 3 markers | Same dot style before and after a bounce/portal; never hidden by other VFX; wind bends the dotted arc itself | M3 (impact ring) / M6 (others; graybox from M4) |
| `ArrowFired` | `VFX_Bow_Release` (string snap ripple) | 0.2 s | 8 / 4 | — | M3 |
| Arrow flight | `VFX_Trail_Default` (thin ribbon), cosmetic trails replace it | flight | ribbon / ≤ 30 | ≤ 60% opacity, behind the arrow | M3 |
| `ArrowImpact` embed/deflect per material | `VFX_Impact_Straw/Timber/Stone/Ice/Metal` | 0.3–0.5 s | 20 / 10 | Small (≤ 0.6 m), additive or tiny alpha | M3 (timber) / M6 |
| Ricochet (metal / Bounce) | `VFX_Ricochet_Spark` | 0.25 s | 16 / 8 | Spark direction = reflected vector (teaches geometry) | M6 |
| `ObjectBroken` per material | `VFX_Break_Straw/Timber/Stone/Ice/Metal` + debris meshes | 0.6–1.0 s; debris fades 1.2–2.0 s | 30 / 12 | Dust delayed 0.15 s; no dust on objectives | M3 / M6 |
| `ObjectiveCleared` | `VFX_Objective_Cleared` (red shards + gold ring pop + icon flies to HUD) | 0.6 s | 24 / 12 | Plays *at* the objective, then the HUD icon crosses out | M3 |
| Bullseye hit | `VFX_Bullseye_Hit` (gold ring burst) | 0.5 s | 16 / 8 | — | M6 |
| `ProtectedLost` | `VFX_Protected_Lost` (purple pulse ring + slow-mo focus) | 0.5 s | 12 / 6 | Object stays visible and is the focus | M3 |
| Rope cut / chain cut | `VFX_RopeSnap` (fibre burst) / `VFX_ChainSnap` (spark) | 0.3 s | 10 / 5 | — | M3 / M6 |
| Balloon pop | `VFX_BalloonPop` (confetti scraps, gold/green) | 0.4 s | 14 / 6 | — | M6 |
| Powder barrel fuse | `VFX_Fuse_Spark` | 0.15 s (chain delay) | 8 / 4 | Telegraphs the chain | M6 |
| Powder barrel blast | `VFX_Explosion_Powder` (flash ≤ 0.1 s, ring shockwave, embers; smoke **delayed 0.2 s**, low-opacity, rises away from the play centre) | 0.8 s | 60 / 25 | No full-screen flash; smoke never covers objectives | M6 |
| Oil jar break | `VFX_OilSplash` + puddle decal | 0.4 s | 12 / 6 | — | M6 |
| Fire ignition | `VFX_Ignite` | 0.3 s | 10 / 5 | — | M6 |
| Burning (rope/straw/FireZone) | `VFX_Fire_Burn` (looped, sized by object), `VFX_FireZone_Oil` | burn duration | 20 per source / 10; cap 3 sources simultaneously at full rate | Flames stay inside the object's bounds + 0.3 m | M6 |
| Split pre-split pulse (D-088) | `VFX_Arrow_SplitPulse` (glowing ring + trail pulse on the arrow ~0.12 s before the split) | 0.12 s | 8 / 4 | Tells the player the split is about to happen at the point the preview marked | M6 |
| Split arrow split | `VFX_Split_Burst` | 0.2 s | 10 / 5 | Marks the split point the preview showed | M6 |
| Boulder rolling | `VFX_Boulder_Dust` (trail puffs) | while rolling | 16 / 6 | — | M6 |
| ~~Spring plate launch~~ | ~~`VFX_Spring_Launch`~~ — **cut (D-086)** | — | — | — | — |
| Wind field | `VFX_Wind_Streaks` (direction streaks inside the zone) | always | 30 / 12 per zone | **Required readability**: always shows direction + rough strength (streak speed) | M6 |
| Portal idle / pass | `VFX_Portal_Idle` (rim swirl, pair colour) / `VFX_Portal_Pass` (entry + exit flash) | always / 0.3 s | 20 / 10 per ring | Entry and exit flash in the same colour + glyph | M6 |
| Ice slide | `VFX_Ice_Scrape` (glints) | while sliding | 10 / 4 | — | M6 |
| Kill zones | `VFX_Splash_Water`, `VFX_Spikes_Crunch`, `VFX_Pit_Poof` | 0.5 s | 24 / 10 | — | M3 (water, pit) / M6 (spikes) |
| Chain reaction escalation | `VFX_Chain_Combo` (small rising gold notes near the HUD, optional) | per step | 6 / 0 | UI-space only, off with Reduced Particles | M6 (P2) |
| Win | `VFX_UI_StarBurst` ×1–3, `VFX_UI_Coins` | 0.4 s per star | 40 / 20 (UI) | — | M3 |
| World unlock | `VFX_UI_WorldUnlock` | 1.2 s | 60 / 30 | — | M6 |

### 9.3 Hit-stop and camera shake (owner ART code, values in `GameplayTuning`)
- `HitStop`: 40–70 ms (§11, `GameConstants.HitStopMin/MaxSeconds`) on *meaningful direct impacts* only: an arrow breaking an objective, cutting a required rope, a structure break by direct arrow, a blast start. Max 1 per 0.3 s. Scaled by impact energy. **Kept under Reduced Motion** (it is a time pause, not motion) but capped at 40 ms.
- `CameraShake`: ≤ 0.08 m amplitude, ≤ 0.25 s, explosions and heavy collapses only, rotation-free (position jitter). **Disabled** by Reduced Motion. Does not shift the play plane's framing by more than 1% of the width.

---

## 10. SFX design plan

### 10.1 System rules
- `SoundEvent` SO fields: clips[] (variations), volume (dB), volume random ±dB, pitch random ±%, mixer group, priority (0–255, lower = more important), max concurrent voices, cooldown (s), spatial blend (always 0 — 2D mix; stereo pan from screen-x ±0.4 for orientation), loop flag.
- **24 voices** total (`01` §13). Voice stealing by priority, then age.
- **Priority tiers:** 0–20 Protected/objective/fail/win · 21–60 arrow release & impacts · 61–100 breaks & props · 101–160 debris, rolls, loops · 161–255 ambience/UI flourishes.
- Variation: impacts 3–4 clips, breaks 2–3, UI 1–2. Pitch ±5% (`CosmeticRandom`), volume ±1.5 dB. **No repeat of the same clip twice in a row.**
- **Layering:** material breaks = *transient* (crack/snap) + *body* (thud/crunch) + *tail* (rattle/settle). Tails are skipped under load (more than 6 breaks within 0.25 s → transients only, + chain-reaction percussion takes over, §11).
- **Mixer:** `Master` → `Music` / `SFX` (sub: `Gameplay`, `Ambience`) / `UI`. Snapshots: `Default`, `Paused` (−8 dB SFX low-pass), `Results` (music −6 dB), `AdPlaying` (all muted; the AudioListener is paused during ads).

### 10.2 SFX list

| SoundEvent | Content | Variations | Priority | Max voices | Needed |
|---|---|---|---|---|---|
| `SE_Bow_DrawCreak` | Wood creak loop, pitch rises with draw power | loop | 40 | 1 | M3 |
| `SE_Bow_DrawThreshold` | Soft click/tick when the draw becomes fire-able | 1 | 40 | 1 | M3 |
| `SE_Bow_FullDraw` | Faint magical shimmer | 1 | 45 | 1 | M3 |
| `SE_Bow_Release` | String snap + twang (signature, custom) | 3 | 21 | 2 | M3 |
| `SE_Bow_Cancel` | Gentle string relax | 1 | 60 | 1 | M3 |
| `SE_Arrow_Whistle` | Loop; pitch/volume rise with speed (§11) | loop | 50 | 3 | M3 |
| `SE_Impact_Straw` | Soft thwip + rustle | 3 | 30 | 3 | M6 |
| `SE_Impact_Timber` | Wood thunk (embed) | 4 | 30 | 3 | M3 |
| `SE_Impact_Stone` | Stone thunk / chip (deflect) | 4 | 30 | 3 | M6 |
| `SE_Impact_Ice` | Ice chime tick | 3 | 30 | 3 | M6 |
| `SE_Impact_Metal` | Metal ping (§11) | 4 | 25 | 3 | M6 |
| `SE_Arrow_Ricochet` | Ping + whizz | 3 | 25 | 2 | M6 |
| `SE_Break_Straw/Timber/Stone/Ice/Metal` | Layered break (crack + body + tail); ice = shatter + chime tail | 3 each | 65 | 4 | M3 / M6 |
| `SE_Debris_Settle_<Material>` | Small rattles | 3 | 140 | 2 | M6 |
| `SE_Objective_Cleared` | Bright crest-shatter "ting" + rising tone | 2 | 10 | 2 | M3 |
| `SE_Objective_AllCleared` | Short swell before the win confirm | 1 | 5 | 1 | M3 |
| `SE_Protected_Lost_Vase` | Porcelain crash + "uh-oh" musical sting | 1 | 1 | 1 | M3 |
| `SE_Protected_Lost_Fox` | Startled yelp (cartoon, original) | 2 | 1 | 1 | M6 |
| `SE_Fox_Snore` | Quiet snore loop (ambient tell) | loop | 150 | 1 | M6 |
| `SE_Dummy_Hit` / `_FallOver` | Squeaky stuffing thud / comedic flop | 2 / 2 | 30 | 2 | M6 |
| `SE_Lantern_Break` | Glass tinkle + fizz | 2 | 30 | 2 | M6 |
| `SE_Orb_Shatter` | Dark glass + magical release | 2 | 15 | 1 | M6 |
| `SE_Rope_Snap` / `SE_Chain_Snap` | Rope twang (§11) / chain clank | 3 / 2 | 25 | 2 | M3 / M6 |
| `SE_Rope_Burn` | Crackle loop | loop | 100 | 2 | M6 |
| `SE_Balloon_Pop` | Pop | 3 | 40 | 3 | M6 |
| `SE_OilJar_Break` | Clay crack + glug | 2 | 40 | 2 | M6 |
| `SE_Fire_Ignite` / `SE_Fire_Loop` | Whoomp / crackle | 2 / loop | 40 / 110 | 2 / 3 | M6 |
| `SE_Barrel_Fuse` | Short hiss | 1 | 35 | 2 | M6 |
| `SE_Barrel_Explode` | Layered boom (sub trimmed for phone speakers) + debris tail | 3 | 15 | 2 | M6 |
| `SE_Boulder_Roll` / `_Impact` | Rumble loop / heavy thud | loop / 3 | 90 / 45 | 1 / 2 | M6 |
| ~~`SE_Spring_Launch`~~ | **Cut (D-086)** | — | — | — | — |
| `SE_WindFan_Loop` | Whoosh loop | loop | 150 | 2 | M6 |
| `SE_Shield_Clank` / `_Hum` | Metal block / mechanism hum | 3 / loop | 30 / 160 | 2 / 2 | M6 |
| `SE_Portal_Enter` / `_Exit` / `_Idle` | Warp in/out + idle hum | 2 / 2 / loop | 25 / 25 / 160 | 2 / 2 / 2 | M6 |
| `SE_Arrow_Split` | Triple whoosh | 2 | 25 | 1 | M6 |
| `SE_Arrow_Bounce` | Bright magical ping | 2 | 25 | 1 | M6 |
| `SE_Arrow_FireFlight` | Flame flutter loop | loop | 50 | 2 | M6 |
| `SE_Kill_Water` / `_Spikes` / `_Pit` | Splash / crunch / distant thud | 3 / 2 / 2 | 70 | 3 | M3 (water, pit) / M6 (spikes) |
| `SE_Ice_Slide` | Scrape loop | loop | 110 | 2 | M6 |
| `SE_UI_Tap` / `_Back` / `_Toggle` / `_Error` | UI | 2 / 1 / 2 / 1 | 170 | 2 | M3 |
| `SE_UI_Star1/2/3` | Rising three-note star reveals | 1 each | 160 | 1 | M3 |
| `SE_UI_CoinTick` / `_CoinBurst` | Coin counter | 2 / 1 | 165 | 2 | M3 |
| `SE_UI_Unlock` / `_WorldUnlock` | Node unlock / fanfare | 1 / 1 | 150 | 1 | M6 |
| `SE_UI_Purchase` / `_Equip` | Purchase success / equip | 1 / 2 | 150 | 1 | M7 (purchase) / M6 (equip) |
| `SE_UI_ArrowReveal` | Special arrow card reveal | 1 | 150 | 1 | M6 |
| `SE_UI_Toast` | Soft "1 arrow left" chime | 1 | 120 | 1 | M3 |

Audio cues are **never the sole signal** (§15): every sound above has a visual counterpart.

---

## 11. Music system plan

| Element | Spec | Needed |
|---|---|---|
| `MusicPlayer` | Persistent (AppRoot). Two `AudioSource`s for 1.0 s equal-power crossfades. Plays `MUS_*` via `WorldData.music` refs. Pauses on app pause/ads. Respects the Music toggle (fades, doesn't restart). | M3 |
| `MUS_Menu_Theme` | Home + World Map. Warm, adventurous, light folk-orchestral, 60–90 s seamless loop | M6 |
| `MUS_GW_Loop` | Greenwood: acoustic guitar/lute, wooden flute, hand percussion; bright, 100–110 BPM, 60–120 s loop | M3 (VS) |
| `MUS_SC_Loop` | Sunscar: frame drums, plucked strings, warm brass pads; dusk mood. Original fantasy idiom, avoiding real-culture pastiche clichés. | M6 |
| `MUS_FK_Loop` | Frostspire: bells, celesta, string ostinato, airy choir pad | M6 |
| Set-piece layer (P1) | Every 5th level: an additional percussion stem on the world loop (`MUS_<W>_SetPieceLayer`), synced by sample position | M6 |
| Stings | `MUS_Sting_Win` (2–3 s, key-agnostic), `MUS_Sting_Fail` (gentle, short, non-punishing), `MUS_Sting_ThreeStar`, `MUS_Sting_WorldUnlock`, `MUS_Sting_ArrowReveal`, `MUS_Sting_ChainFinale` | M3 / M6 |
| **Chain-reaction percussion** (§11) | `ChainReactionTracker` (in `FeedbackDirector`) counts break/trigger events inside a sliding 0.4 s window **after a single shot**. Steps 1–6 trigger `SE_Chain_Perc_1..6` (escalating drum/ting hits, each a semitone/intensity step up). At step ≥ 3, the music ducks −4 dB. On the shot's resolution with ≥ 4 steps, play `MUS_Sting_ChainFinale` before the win sting (or instead of it if the win is immediate). | M6 |
| Ducking | Win/Fail panels: music −6 dB (`Results` snapshot). Pause: low-pass. Rewarded/interstitial ad: all audio paused. Hit-stop: no change. | M3 |
| Loudness | Music −16 LUFS integrated. Ships at 70% default volume (Settings slider 0–100% if time allows; the toggle is mandatory, §8). | M3 |

---

## 12. Haptics plan (`IHapticsService`, D-032; owner PLAT bridge, ART mapping)

| Trigger | `HapticKind` | Rule |
|---|---|---|
| Draw crosses `minFirePower` (fire-able) — §8 "light on draw threshold" | `Light` | Once per draw |
| Full draw reached | `Selection` | Once per draw |
| `ArrowFired` | — (none; the release snap is audio/visual) | — |
| Meaningful direct impact (same rule as hit-stop) — §8 "medium on impact" | `Medium` | Max 1 per 0.3 s |
| Other structure breaks during collapse | `Light` | Max 4 per second; skipped if a Medium fired within 100 ms |
| Rope cut, balloon pop, portal pass | `Light` | Max 1 per 0.15 s |
| Explosion | `Heavy` | Max 1 per 0.3 s |
| `ObjectiveCleared` | `Medium` | — |
| `LevelWon` — §8 "success pattern on clear" | `Success` | Once |
| `LevelFailed` / `ProtectedLost` | `Failure` | Once; soft |
| UI button press (primary actions only) | `Selection` | Optional, on by default; governed by the same toggle |

Global: 1 haptic per 50 ms (D-032), the **Haptics toggle** in Settings disables all (persisted), and the iOS system haptics setting is respected. Android devices without amplitude control fall back to short one-shots (Light 10 ms, Medium 20 ms, Heavy 35 ms, Success = 2 pulses).

---

## 13. UI screen inventory

Owner `UI` unless noted (co-owner in brackets). All screens use `UIRouter`, `ScreenBase`, `SafeAreaFitter`, `UiTween`, and strings via `UIStrings` keys (localisation-ready, A-02).

| Screen / panel | Class | Purpose | Entry → Exit | Key elements | Needed | Owner |
|---|---|---|---|---|---|---|
| Splash / loading | `AppRoot` overlay (Boot) | Brand moment while services init (§8) | App start → Consent or Home (≤ 3 s target) | Wordmark + drawn bow (**static only**, animated splash cut — D-101), subtle progress | M2 (temp), M6 (final) | UI |
| Consent | `ConsentPanel` | Host UMP form / ATT pre-context (no dark patterns). Consent-aware init (D-096): in UK/EEA, Firebase Analytics + Crashlytics collection and ad personalisation stay off until consent | Boot (first launch / consent change / UK-EEA) → Home; Settings ▸ Privacy → back | Platform UMP form (native), our short pre-ATT explainer ("Allow personalised ads?" neutral wording), equal-weight buttons, privacy-policy link (D-097) | M7 | MON [UI] |
| Home | `HomeScreen` | Big PLAY + current world card (§8) | Splash/Consent → WorldMap / Gameplay (PLAY = next uncleared level) / Bow Forge / Settings / Daily / Shop | PLAY (dominant), world card with progress, Daily badge, Forge, Shop, Settings, coins | M6 (graybox M3) | UI |
| World map | `WorldMapScreen`, `LevelNodeView` | Choose levels, see stars, locked-world teaser | Home → Gameplay / Home | Vertical scroll path, 20 nodes per world, chapter clusters, star pips, medal pips, locked teaser ("Clear 15 levels to unlock"), coins | M6 | UI [SYS] |
| Gameplay HUD | `HudView`, `ObjectiveIconsView`, `QuiverView`, `ToastView` | Minimal in-level info (§8) | Gameplay load → panels | §14 layout | M2 graybox, M3 final | UI |
| Tutorial overlays | `TutorialPromptController` (Gameplay) + `UI_TutorialHand`, `UI_Callout_Bubble` | L1 ghost-hand drag; callouts in designated levels ("Aim for the rope") | `LevelStarted` with `TutorialPromptData` → dismissed by `DrawStarted`/timeout | Hand, bubble anchored to a `TutorialAnchor` (world→screen), never covers the target | M3 | CORE [UI] |
| Arrow reveal card | `ArrowRevealCard` (in `UI/`) | First appearance of a special arrow (D-082: Heavyhead L13, Split L30, Fire L36, Bounce L47) | First `LevelStarted` with a new type → tap to continue → HUD | Card art, name, one-line effect ("Hits hard. Flies slow."), the arrow added to the codex | M6 | UI |
| Pause | `PausePanel` | Pause the level | HUD pause → Resume / Restart / Map / Settings | Resume (dominant), Restart, World Map, sound/haptics quick toggles | M2 graybox, M3 | UI |
| Win | `WinPanel` | Celebrate + reward + 1-tap Next (§8) | `LevelWon` → Next (Gameplay) / Replay / Map; optional 2× coins (rewarded) | Stars animate 1→3, arrows used vs par, coins (counter), medal if earned, **Next (dominant)**, Replay, Map, "2× coins ▶" (never blocks Next, §9) | M3 | UI [MON] |
| Fail | `FailPanel` | Quick message + Retry (§8) | `LevelFailed` → Retry / Map; optional +1 arrow rewarded | Reason line ("Out of arrows" / "The vase broke!" / "You woke the fox!"), **Retry (dominant)**, Map, "+1 arrow ▶" only when `AdPolicy` allows, always labelled **"Bonus Arrow Used — 1★ Max"** before the player accepts (D-084) | M3 | UI [MON] |
| Rewarded flow states | `RewardedOfferButton` | Show offer, loading, unavailable, granted. On the Fail panel the button and the confirm step show **"Bonus Arrow Used — 1★ Max"** before the player accepts the ad (D-084, key `fail.bonus_arrow.star_cap`) | Win/Fail → ad → reward toast | Loading spinner ≤ 5 s then "Not available right now" (no penalty); 1★ disclosure always visible on the +1 arrow offer | M7 | MON [UI] |
| Interstitial transition | (UIRouter hook) | Show capped interstitial only on Win → Next/Home (D-093) | Win Next/Home → (ad) → destination | Fade to the transition overlay; no UI of our own. Never after a fail, a purchase, a rewarded ad, during onboarding or on app resume (D-093) | M7 | MON |
| Bow Forge | `BowForgeScreen` | Cosmetics collection grid (§8) + arrow codex + badges/medals | Home → back | Tabs: Bows / Trails / Badges / Arrows (codex); grid with owned/locked/price; preview bow at large size; Equip/Buy; no stats | M6 | UI [SYS] |
| Shop | `ShopScreen` | IAPs: Remove Ads (£3.99 / USD 3.99), Cosmetic Starter Pack (£2.99 / USD 2.99) (D-091); restore (iOS) | Home / Forge → back | Localised store prices (never hard-coded), contents list, "Cosmetic only — no gameplay advantage" note, "Remove Ads removes interstitials only; optional rewarded videos stay" note, Restore Purchases | M7 | MON [UI] |
| Daily challenge | `DailyChallengePanel` | 3 already-cleared levels with a gold-par quiver (D-094) — **cut-first** if it risks the 60-level campaign | Home badge → Gameplay (challenge mode) → back to the panel | 3 cards with done ticks, reward 100 coins, resets at local midnight (shows hours left), locked until global L10 is cleared | M6 | SYS [UI] |
| Settings | `SettingsPanel` | §8 settings | Home / Pause → back | Music, SFX, Haptics, Reduced particles, Reduced motion, Colour-assist outlines, Restore purchases, Privacy (consent form, hosted policy link — D-097), Credits, version/build id | M3 (Music/SFX/Haptics toggles inside the Pause panel only, per `11` §3), M6 (Settings screen + accessibility toggles), M7 (Privacy + Restore) | UI |
| Purchase / error popups | `ModalDialog` | Purchase result, network issues, restore result | Any → dismiss | One primary button; no guilt copy | M7 | UI |
| Toasts | `ToastView` | "1 arrow left", "Out of arrows", "Bullseye!", "Saved" | Gameplay/UI events → auto-hide 1.2 s | Pill near the top-centre, never over the aim zone | M2 | UI |
| Transition overlay | `TransitionOverlay` (AppRoot) | Scene/level fades (≤ 0.25 s) | Any scene change | Fade + small bow-spinner if load > 0.5 s | M2 | UI |
| Credits | `CreditsPanel` | Attribution (fonts/SFX licences) | Settings → back | Scroll list from `ASSET_LICENSES.md` | M8 | UI [ART] |

---

## 14. HUD layout plan

Reference canvas **1080×1920**, CanvasScaler *Scale With Screen Size*, match = **0 (width)** on phones (taller screens gain vertical space), **1 (height)** when aspect > 9:16 (tablets). Touch targets ≥ **48 dp / 44 pt ≈ 144 px** at reference (never below 120 px). Minimum text 34 px (body), 44 px (HUD numbers).

```
 1080 px
┌──────────────────────────────────────────────┐ ← physical top (notch / Dynamic Island / punch-hole)
│░░░░░░░░░░░░░ safe-area top inset ░░░░░░░░░░░░│
├──────────────────────────────────────────────┤ ← SafeAreaFitter top
│ [⏸]  1-5        ◉ ◉ ⊘ ◉          ➶➶➶  [↻]   │  HUD band ≈ 170 px (≈ 9%)
│ pause level     objective icons   quiver restart│  pause/restart: 144×144 px buttons
│ (TL)            (TC, crossed when cleared)   (TR)│  quiver: next arrow enlarged + highlighted
├──────────────────────────────────────────────┤
│            ( toast: "1 arrow left" )           │  toast lane y ≈ 260–360 px
│                                                │
│       PLAY AREA (10 m wide at z=0)             │  structure zone (world y ≈ 4–15 m)
│         ▣▣  ⌒⌒⌒rope⌒⌒⌒   ◎ crest              │
│        ▣▣▣         ▣     ⚱ vase(purple ring)   │
│   ┌──────────────┐                             │
│   │ Aim for the  │◄─ tutorial callout (only in  │
│   │ rope!        │   designated levels, anchored │
│   └──────────────┘   to a TutorialAnchor)       │
│- - - - - - - - - - - - - - - - - - - - - - - - │ ← aim zone top (bottom 55% of safe area, D-018)
│             .  ·  ·  · (trajectory dots)       │
│                   ·                             │  AIM ZONE: press anywhere here (except
│                ·                                │  over a HUD button) and drag back
│              ⟆ BOW (bottom-centre)             │  bow pivot ≈ world (0, 1.5, 0)
│              ◡ stand                            │
├──────────────────────────────────────────────┤ ← SafeAreaFitter bottom
│░░░░░░░ safe-area bottom (home indicator) ░░░░░│  nothing interactive here
└──────────────────────────────────────────────┘
 1920 px (9:16)  — taller phones (9:19.5 / 9:21) add sky/ground above/below the play area
```

Rules:
- **Nothing interactive in the aim zone.** The only UI that may appear there is the tutorial ghost hand (non-blocking, `raycastTarget = false`).
- Quiver: shows **remaining** arrows as icons (§8 "clear arrow icons") in authored order (D-023). The next arrow is enlarged with a soft glow. More than 6 arrows → icon + "×N".
- Objective icons: one per required objective (stacked "×N" if > 5). Cleared → crossed with a check + desaturate (shape change, not colour only).
- Level label "1-5" (world-level) for the player, `W1_L05` in dev overlay only.
- Pause/restart sit in the thumb-reachable corners. Restart is immediate (no confirm) — fast recovery.
- No coins/currency in the gameplay HUD (one-thumb clarity).

---

## 15. Safe area and responsive plan

| Case | Handling |
|---|---|
| Aspect 9:16 → 9:21 (phones) | `CameraFramer` fits the 10 m play width (D-041); extra height shows sky (top) and foreground (bottom). The HUD anchors to the safe-area top; the aim zone is computed from the safe area. Environment art provides +3 m of sky and +2 m of foreground headroom. |
| Notch / Dynamic Island / punch-hole | `SafeAreaFitter` on every canvas root (reads `Screen.safeArea`, re-applies on change). The HUD band starts below the inset. Nothing is centred directly under the Dynamic Island area (the objective icons row sits in the HUD band below the safe-area top). |
| Home indicator / gesture nav bar | Bottom inset excluded from the aim zone. The bow sits above it. Android: draw edge-to-edge (target-API requirement) with insets honoured. Avoid drags starting within 24 dp of the bottom edge (system gesture conflict). |
| Tablets (3:4, 2:3) | Match-height canvas scaling. Camera fits height; **pillarbox scenery** (environment ±3 m beyond the play area, D-041). The HUD stays inside a centred 9:16-proportioned column to keep thumb distances sane. Tested on one iPad (A-03). |
| Very small phones (iPhone SE, 4.7") | Verify 144 px buttons and 34 px text still fit; the objective row compresses to "×N" sooner. |
| Foldables / split-screen | Portrait locked. Resizing (e.g. a foldable unfold) triggers a re-fit via `OnRectTransformDimensionsChange` → `CameraFramer.Refit()`. Best effort, not certified. |
| Low tier | Same layout. UI particles at 50%. |

Verification: Unity Device Simulator profiles (iPhone SE 3, iPhone 11, iPhone 15 Pro Max, Galaxy A13, Pixel 7, iPad 9th gen) + device matrix in `07`.

---

## 16. Accessibility plan

| Feature | Spec | Setting / default | Needed |
|---|---|---|---|
| **Shape + icon, not colour only** (§8) | Required = crest/target glyph + ring silhouette; protected = shield/crown glyph + halo; interactive = spark/gear glyph; cursed orb = eye-rune; portal pairs = colour **+** glyph | Always on | M3 |
| **Colour-assist outlines** (§8) | Thick (≈ 3 px at 1080 width) screen-consistent outlines: red/required, purple/protected, gold/interactive, + a category glyph billboard above each object. Implementation: inverted-hull outline pass in `SG_AB_StylizedLit` (cheap; works on Low) + world-space glyph sprites (D-054). Patterns are colour-blind-safe (checked with deuteranopia/protanopia/tritanopia simulators). | Off by default; prompted once in Settings | M4 (graybox outline pass), M6 (final) |
| **Reduced particles** (§8) | Particle counts ×0.4, no smoke/dust clouds, no UI confetti, no background ambient particles | Off; auto-on for the Low tier (visible in Settings) once tier selection exists (M8) | M6 |
| **Reduced motion** | Disables camera shake, UI bounce/punch (fades instead), parallax drift, slow-mo focus beyond 0.25 s. Hit-stop capped at 40 ms. | Off; defaults on when the OS reduce-motion flag is set (iOS `UIAccessibility.isReduceMotionEnabled`, Android animator duration scale = 0) | M6 |
| **Haptics toggle** (§8) | Disables all haptics | On by default | M3 |
| **Music / SFX toggles** (§8) | Independent | On | M3 |
| **Audio not the sole signal** | Every SFX cue has a visual counterpart (§10). Fail reasons are shown as text. The fox's snore has a "Zz" visual. | Always | M3 |
| **Text** | Min 34 px body / 44 px HUD numbers at reference. Dynamic text sizing beyond that is post-MVP (proposed: "Larger text" ×1.15 as P2). | — | M3 |
| **Contrast** | UI text ≥ 4.5:1 (WCAG AA), large text/icons ≥ 3:1. Gameplay objects ≥ 30% luminance contrast vs. the local background (S-5). | — | M6 audit |
| **One-handed / no multitouch** (§8) | Single-pointer draw anywhere in the aim zone (D-018). All buttons reachable by the thumb. Secondary touches are ignored. No gestures required beyond tap and drag. | Always | M1 |
| **Forgiving input** | Releasing below `minFirePower` cancels without spending an arrow. Restart needs no confirm. Pause is always available. | Always | M1 |
| **Timing** | No input timers. Moving shields are slow, periodic and telegraphed (track art); moving ice platforms are cut (D-104). The aim window is 8–12% of screen width (§7). | Design rule | M4 |
| **Photosensitivity** | No more than 3 flashes per second; explosion flash ≤ 0.1 s and not full-screen | Always | M6 |
| **Screen readers** | Not supported in MVP (gameplay is visual). Menus use a standard button hierarchy so a later accessibility pass is feasible (post-MVP). | — | Post-MVP |

QA checks are in `07` §Accessibility checks.

---

## 17. UI state flow (text diagram)

```
[App Launch]
   └─► BOOT (AppRoot: load save → init services)
         ├─ first launch / consent needed / consent version changed / EEA-UK
         │     └─► CONSENT (UMP form) ──► [iOS] ATT pre-context ──► ATT system prompt
         │                                                          └─► consent-aware init (D-096): Analytics/Crashlytics collection +
         │                                                              ad personalisation ON only if consented (UK/EEA)
         └─ consent known ──► (consent-aware analytics/ads/crash init; remote config fetch ≤ 2 s)
   └─► SPLASH (≤ 3 s total) ─► HOME
                                  │
   HOME ──PLAY──────────────────────────────────────────► GAMEPLAY (next uncleared level)
     │  first launch only: HOME is skipped → GAMEPLAY W1_L01 directly (first shot < 15 s, §10)
     ├──World card / Map──► WORLD MAP ──node──► GAMEPLAY(level)
     │                          └──back──► HOME
     ├──Daily badge──► DAILY PANEL ──card──► GAMEPLAY(challenge) ──result──► DAILY PANEL
     ├──Forge──► BOW FORGE ──buy (coins)──► confirm ──► equip ──back──► HOME
     ├──Shop──► SHOP ──buy (IAP)──► [store sheet] ──success──► grant + toast ──back
     │                                           └─fail/cancel──► neutral modal ──back
     └──Settings──► SETTINGS ──Privacy──► CONSENT (UMP privacy options) ──back
                              └──Restore purchases──► [store] ──► result modal

GAMEPLAY
   LevelStarted ─► [ArrowRevealCard if new arrow] ─► [Tutorial prompt if any] ─► READY
   READY ⇄ DRAWING ─► fire ─► (cooldown 0.35 s) ─► READY …  (soft-lock toast "N arrows left")
   HUD Pause ─► PAUSE ─► Resume ─► READY | Restart ─► LevelLoader.Load (same level) | Map ─► WORLD MAP
   HUD Restart ─► LevelLoader.Load (same level, attempt+1)
   ├─ all objectives cleared + settle ─► WIN PANEL
   │     ├─ "2× coins ▶" (optional) ─► rewarded ad ─► reward granted ─► coins doubled ─► WIN PANEL
   │     ├─ Next ─► [AdPolicy (D-093): interstitial allowed? never after a rewarded ad/purchase/onboarding] ─yes─► interstitial ─► GAMEPLAY(next level)
   │     │                                            └─no──────────────────► GAMEPLAY(next level)
   │     │      (next level in a new world, or world just completed ─► WORLD MAP with unlock fanfare)
   │     ├─ Replay ─► GAMEPLAY(same level)
   │     └─ Map ─► [AdPolicy interstitial check] ─► WORLD MAP
   └─ protected lost ─► (0.5 s slow-mo focus, D-038) / out of arrows ─► ("Out of arrows" toast 0.6 s, D-015) ─► FAIL PANEL
         ├─ Retry (dominant) ─► GAMEPLAY(same level)            (never an interstitial after a fail, D-093)
         ├─ "+1 arrow ▶ — Bonus Arrow Used — 1★ Max" (only if AdPolicy allows; max 1 per attempt; disclosure shown BEFORE accepting, D-084)
         │        └─► rewarded ad ─► granted ─► resume the same attempt with +1 arrow (clear capped 1★, D-084)
         │                        └─ not granted / no fill ─► FAIL PANEL ("Not available right now")
         └─ Map ─► WORLD MAP

App backgrounded (any state) ─► auto-PAUSE in Gameplay; save flush; audio paused ─► resume returns to PAUSE (no interstitial on app resume, D-093)
```

---

## 18. Asset production priority list (by milestone, D-102 execution order)

Order follows the owner's 12-step execution order: prove the five-level vertical slice with real feedback first, then build every system and all 60 levels in graybox, validate on devices, and only then produce final world art, audio, VFX, UI polish and cosmetics. SDK-facing UI comes after the core game is stable.

| # | Milestone | Graybox (placeholder, colour-language materials) | Final / near-final art & audio | Owner |
|---|---|---|---|---|
| 1 | **M1** Foundations & Core Feel | Bow (cylinder limbs + LineRenderer string), `Arrow_Oak` capsule, crates, ground, crest target, `M_Graybox_*` set, `PH_SFX_*` (2–3 generic clicks for release/impact) | — | ART (setup), CORE/PHYS (prefabs) |
| 2 | **M2** Core Loop + VS Graybox | Vase, rope, kill zones, HUD/panels with `UI_Icon_*` placeholders, toasts, temporary static splash | Fonts chosen + licensed (logged), `ASSET_LICENSES.md` created, `AssetImportRules` live | ART, UI |
| 3 | **M3** Vertical Slice + External Playtest + Feel Lock | Everything outside the VS stays graybox | **Greenwood VS section only:** `T_GW_Palette`, ground/platform/pillar/cliff/rope-anchor/bow-stand/tree, 1 background card + sky; **hero bow** `SM_Bow_OakRanger` + `AC_Bow`; `Arrow_Oak`; `Env_GW_Backdrop_TrainingGrounds` + `Env_GW_*` VS blocking variants; `Struct_Crate_Timber_*`, `Struct_Post_Timber_0.5x2`, `Struct_Plank_Timber_4x0.25`, `Struct_Beam_Timber_*`, `Obj_CrestTarget`, `Prop_Rope`, `Prot_RoyalVase`, `Haz_Pit`, `Haz_WaterPit`; timber/crest/vase debris; kill-zone splash/poof VFX + SFX; VFX: bow glow/release, trail, timber impact/break, objective cleared, rope snap, protected lost, star burst; SFX: bow set, whistle, timber set, objective, vase, rope, UI core, stars; `MUS_GW_Loop` (original) + win/fail stings; haptics map; HUD + Win/Fail/Pause polished; tutorial hand + callout. Checked on an **iOS device build** (D-100) and Android. **OR-1 originality gate; OR-2 provisional art-direction lock at G0.** | ART, UI |
| 4 | **M4** Systems Complete | Graybox variants for **every** remaining material (straw, stone, ice, metal, Earth), objective (supply crate, lantern, orb, dummy, banner rope), protected (fox, relic), prop (8; no spring plate), hazard, special arrow (Heavyhead/Split/Fire/Bounce incl. preview markers), `PH_` SFX for every `GameEvents` feedback hook, placeholder arrow reveal cards | None integrated. **Parallel track allowed (D-102):** Greenwood kit modelling may start after G0; it is not integrated into levels yet. | ART (graybox prefabs with PHYS/PROPS) |
| 5 | **M5** Content Graybox + Device Validation | All 60 levels stay graybox (validation on devices uses graybox art) | **Parallel track:** Greenwood kit continues; Sunscar and Frostspire kit modelling may run. Nothing integrated. | ART |
| 6 | **M6** Art, Audio, UI Polish & Meta | — | **OR-2 final art lock at start.** Then, world by world (GW → SC → FK): full kits + BG cards + ambience; all structure/objective/protected/prop/hazard final art and debris; fox model + 3 anims; all special-arrow art + reveal cards; full VFX table (§9.2, incl. `VFX_Arrow_SplitPulse`) and SFX list (§10.2); `MUS_Menu_Theme`, `MUS_SC_Loop`, `MUS_FK_Loop`, set-piece layer, chain-reaction percussion (all original); art passes to `Final` on all 60 levels; Home, world-map backdrops + nodes + locked teasers, Bow Forge + thumbnails, **launch cosmetic set: 3 bow skins (Oak Ranger, Moonwood, Royal Amethyst) + 2 trails (Gold Spark, Leaf Swirl)**, badges, Daily panel (cut-first), world-unlock fanfare, equip SFX, final static splash, colour-assist outlines, reduced motion/particles | ART, UI |
| 7 | **M7** Platform Services & Monetisation | — | Consent/ATT explainer + privacy-policy link, Shop + Starter Pack art (`UI_Shop_StarterPack`), `RewardedOfferButton` states incl. the **"Bonus Arrow Used — 1★ Max"** disclosure, `ModalDialog` set, purchase SFX | UI, MON, ART |
| 8 | **M8** Optimisation, Balance, QA & Release Prep | — | Per-tier URP assets/effects tuning, particle budget pass, texture memory pass, contrast audit, colour-blind check, placeholder purge (build check green, no AI placeholders left), final wordmark/logo (after name clearance, D-081), app icon, screenshots, feature graphic, key art, credits. **OR-3 store-asset originality gate.** | ART, PLAT, UI, MON, PO |
| 9 | **M9** UK Closed Test & Submission | — | Preview video + screenshot refresh captured from the closed-test build; store listing art final | ART, MON, PO |
| 10 | **M10** Soft Launch (Canada, Australia) | — | No new art; store listing variants for CA/AU only if required | MON, PO |

**Already cut (D-086, D-101):** spring plate art/VFX/SFX, animated splash, bow skins beyond 3 and trails beyond 2 (Ember, Frostglass, Cyan Streak, Ember Ash → post-MVP content update), moving ice platform art (cut-first; static ice slides instead).

Solo + AI contingency (`08`): the same order. If still behind, cut in this order: P2 environment deco → gloved hand → set-piece music layer → chain combo VFX → preview video (screenshots only). **Never cut:** colour language, silhouettes, material SFX per material (§15), outcome-first VFX, accessibility toggles listed in §8.

---

## 19. Decisions raised by this document (logged as D-051…D-058 in `10_DECISION_LOG.md`)

| Proposed ID | Proposal |
|---|---|
| D-051 | Colour-language reconciliation: structural materials keep §4 identity colours at ≥ 20% lower saturation than reserved accents; neutral supports are gray/blue (§1.3). |
| D-052 | World codes `GW`/`SC`/`FK`; environment prefab pattern `Env_<World>_<Name>` (add to `01` §8). |
| D-053 | No ranger avatar in MVP; the fox is the brand mascot; an optional gloved draw hand (P2). |
| D-054 | Colour-assist outlines via an inverted-hull pass in `SG_AB_StylizedLit` + world-space category glyphs; portal pairs identified by colour + glyph. |
| D-055 | Audio import presets and loudness targets as in §7.3; 2D mix only with screen-x panning. |
| D-056 | Placeholder build check: Release/ClosedTest builds fail on any `Placeholder`-labelled dependency. |
| D-057 | Hit-stop is retained (capped at 40 ms) under Reduced Motion; camera shake and UI bounce are disabled. |
| D-058 | First launch skips Home and drops straight into W1_L01 to meet "first shot < 15 s" (§10); Home appears after the first win. |
