# Skill: Asset Pipeline and Imports

**Primary roles:** ART (owner), PLAT (import budgets), UI (UI sprites/fonts), INT (LFS/meta hygiene)

## Purpose
Bring models, textures, sprites, fonts, audio and VFX into `Assets/_Project` with correct names, import settings, licences and prefab hookups, so that:
- art can be swapped from placeholder to final **without code changes or collider changes**;
- the build stays within the memory/download budgets (01 §11);
- every asset is provably original or properly licensed (D-089: licensed fonts and processed licensed SFX allowed and recorded in `docs/art/ASSET_LICENSES.md`; brand-defining visuals and music original).

## When to invoke
- Any new or replaced file under `Art/`, `Audio/`, `UI/Sprites`, `UI/Fonts`.
- Swapping graybox → world variants (`Struct_Crate_Timber_1x1` → `Struct_Crate_Timber_1x1_Greenwood`).
- Before each milestone gate: the import audit.

**Do NOT invoke** for creating gameplay prefabs or components (PHYS/PROPS), or for SFX/VFX *behaviour* hookups (use [`audio-vfx-haptics-integration.md`](audio-vfx-haptics-integration.md)).

## Inputs
- Asset list and priority from [`05_ART_AUDIO_UX.md`](../planning/05_ART_AUDIO_UX.md); naming from 01 §8.
- The base gameplay prefab to receive the visual (its collider and mass are authoritative).
- Licence info for any third-party source (font licence, SFX library licence).
- `docs/art/ASSET_LICENSES.md` (create it with the header table on the first import if it doesn't exist).

## Step-by-step workflow
1. **Originality gate:**
   - Is the asset made by us or the contractor, or licensed?
   - Is any part traced or derived from a reference-game asset or screenshot? If yes → **reject**.
   - AI-generated → concept/temporary placeholder only (labelled `Placeholder`, D-056); never shipped unless the tool licence, commercial rights and an originality review are documented (D-089).
   - Log every third-party item in `docs/art/ASSET_LICENSES.md`: file, source, licence, URL, date, modifications.
2. **Name and place** per 01 §8:
   - `SM_*.fbx` → `Art/Models/<World or Common>/`;
   - `T_*_<Map>.png` → `Art/Textures/...`;
   - `M_*` → `Art/Materials/`;
   - `SFX_<Cat>_<Name>_<nn>.wav` → `Audio/SFX/<Category>/`;
   - `MUS_<World>_<Name>.ogg` → `Audio/Music/`;
   - `UI_<Group>_<Name>.png` → `UI/Sprites/`;
   - fonts → `UI/Fonts/`;
   - placeholders → `Art/_Placeholder/` (banned from release builds by the build check).
3. **Import settings** (enforced by an `AssetImportRules` `AssetPostprocessor` in `Scripts/Editor/Validation/` — create or extend it if a rule is missing):

   | Type | Settings |
   |---|---|
   | Models | Scale factor so 1 unit = 1 m; Read/Write OFF; Import Cameras/Lights OFF; Animation OFF unless rigged; Mesh Compression Medium; optimise mesh ON; normals Import; no import-time colliders (gameplay colliders live on prefabs); ≤ 2 materials per prop; triangle targets from 05 |
   | Textures (3D) | Max size 512 (props) / 1024 (hero bow, environment cards) / 256 (palette); ASTC 6×6 (Android + iOS); mipmaps ON for 3D; sRGB for colour, linear for masks/normals; one palette texture per world |
   | Sprites (UI) | Sprite (2D and UI), no mipmaps, in a Sprite Atlas v2 per screen group (`UI/Atlases/`); ASTC 4×4 for crisp icons; max 2048 atlas |
   | Audio | SFX: Mono (except stingers), Vorbis q≈0.5, Decompress On Load for < 200 KB clips, Compressed In Memory for larger; Load In Background ON. Music: Streaming, Vorbis q≈0.4, stereo. Force To Mono for SFX. Sample rate override 44.1 kHz (or 22.05 kHz for low-frequency thuds). |
   | Fonts | TMP SDF font asset, static atlas with the needed character set + a fallback; dynamic only for future localisation |
   | VFX textures | Flipbooks ≤ 512, ASTC, shared VFX atlas where possible |

4. **Hook up a visual variant:**
   - create a prefab **variant** of the gameplay base (`manage_prefabs`);
   - replace only `MeshFilter`/`MeshRenderer`/materials/child visuals;
   - **never** change the colliders, Rigidbody mass, layer, `MaterialBody` or `PlanarBody`;
   - run `PrefabValidator` (collider bounds + mass equal to the base, 01 §12).
5. **Material setup:**
   - use the shared stylised Shader Graph lit shader;
   - SRP Batcher compatible (check the Frame Debugger);
   - GPU instancing off unless measured beneficial;
   - shadow casting OFF for small props (< 0.5 m) and debris.
6. **Readability check:**
   - material colour language (straw pale yellow, timber warm brown, stone gray-blue, ice cyan translucent, metal dark steel + gold trim — mvp §4);
   - red objectives, purple protected objects;
   - silhouette test: grayscale screenshot of the level, can you still tell the materials apart?
7. **Refresh and check:** `refresh_unity` → `read_console` (import warnings) → open a level using the variant, `manage_editor` play, screenshot.
8. **Size audit:** Editor ▸ Window ▸ Analysis ▸ Build Report (or the editor log after a build) to confirm the per-asset size; flag anything > 2 MB.
9. **Commit** binary files through LFS (`.gitattributes` covers png/jpg/psd/tga/exr/fbx/blend/wav/mp3/ogg/ttf/otf). Add rules for new types (e.g. `*.tif`, `*.aif`, `*.mp4`, `*.psb`) via INT. Always commit the `.meta` with the asset.

## Output artefacts
- Assets + `.meta` under canonical folders.
- Prefab variants `<BaseName>_<World>.prefab`.
- `docs/art/ASSET_LICENSES.md` rows.
- An import audit note in `docs/qa/test-runs/YYYY-MM-DD_asset-import-audit.md` at milestone gates.

## Quality checklist
- [ ] Originality confirmed; third-party items logged with a licence.
- [ ] Names and folders match 01 §8.
- [ ] Import settings match the table (spot-check 5 random assets).
- [ ] Variants change visuals only; `PrefabValidator` passes.
- [ ] ASTC compression; texture sizes within limits; no uncompressed audio over 200 KB.
- [ ] Binaries tracked by LFS (`git lfs ls-files` shows them); `.meta` committed.
- [ ] No `Art/_Placeholder/` references in release scenes/prefabs (build check).
- [ ] Readability and grayscale silhouette check done.

## Common failure modes
| Symptom | Cause | Fix |
|---|---|---|
| Prefab shows a pink material on device | Shader not included / stripped variant | Use the shared shader; add the variant collection; check Graphics settings |
| "Missing script"/broken references after an import | `.meta` regenerated (file copied without its meta) | Restore the `.meta` from git; never copy assets outside the Editor without metas |
| 30 MB PNG in git history | LFS rule missing for that extension | Add the `.gitattributes` rule, then `git lfs migrate import --include="*.ext"` (coordinate with INT) |
| Art swap changes the gameplay | Variant altered colliders/mass | Revert; visuals only; the validator test |
| Blurry UI icons | Mipmaps on UI sprites or atlas downscaled | Mipmaps off; check the atlas max size |
| Audio hitch on first play | Large clip set to Decompress On Load during gameplay | Compressed In Memory / preload in Boot |
| Model 100× too big or small | FBX unit mismatch | Fix the scale factor in the importer preset |
| Release build contains placeholder art | Placeholder referenced by a variant | Build check scans dependencies for `_Placeholder` |

## Example task prompt for a sub-agent
```text
Agent: ab-art-vfx-audio
Skill: docs/skills/asset-pipeline-and-imports.md
Ticket: AB-026 (M3) Greenwood VS art v1 — crate, crest target, royal vase, rope visuals
Inputs: contractor FBX/PNG drop in <path>; docs/planning/05_ART_AUDIO_UX.md asset list; base prefabs Struct_Crate_Timber_1x1, Obj_CrestTarget, Prot_RoyalVase, Prop_Rope
Deliverables: SM_/T_/M_ assets under Art/; variants Struct_Crate_Timber_1x1_Greenwood etc.; PrefabValidator green; ASSET_LICENSES.md updated;
screenshot of VS-02 before/after.
Boundaries: no collider/mass/layer/component changes; no edits to Lvl_* prefabs (ab-level-design swaps the instances).
Handoff: docs/agents/HANDOFF_TEMPLATE.md
```
