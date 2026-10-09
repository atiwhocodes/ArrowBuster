---
description: Author a new level (LevelData asset + layout prefab) from a level template
---
Create a new Arrow Buster level: $ARGUMENTS
Follow mvp.md §6–7 and docs/planning/04_LEVEL_PIPELINE.md (world cadence, templates, star par rules, aim window 8–12% of screen width, validation rules). Use the docs/skills/level-design-and-validation.md workflow.
1. Check the 60-level delivery tracker in docs/planning/04_LEVEL_PIPELINE.md for this slot's beat, mechanic and quiver. Propose the layout, quiver, goldPar and intended solution in a few lines. Wait for my OK.
2. Build the layout prefab `Lvl_W<N>_L<NN>` under Assets/_Project/Prefabs/Levels/World<N>/ using library prefabs only, via MCP.
3. Create the LevelData asset `W<N>_L<NN>` under Assets/_Project/ScriptableObjects/Levels/World<N>/ and add it to its WorldData.
4. Record the intended shots with the ShotRecorder. Run the LevelValidator and the solvability test for this level.
5. Write docs/levels/W<N>_L<NN>.md from the intended-solution template. Check the Console, then tell me how to playtest it.
