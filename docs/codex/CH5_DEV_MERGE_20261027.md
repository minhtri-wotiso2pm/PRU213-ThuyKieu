# Chapter 5 replacement: integration with dev (2026-10-27)

User request: merge the completed Chapter 5 replacement into dev, resolve conflicts in favour of the new Chapter 5, and preserve the team's other chapters.

## Inputs and recovery

- Previous local dev: `021f57d`.
- Remote dev reviewed: `279c410` (19 additional commits).
- Local replacement snapshot: `609a4a3`.
- Recovery branch: `backup/ch5-before-dev-20261027`. Includes the complete pre-merge working tree, Unity assets and LFS pointers.

## Conflict decisions

- Keep the replacement `Chapter05_Escape.unity`, `Chapter05Setup`, bamboo forest builders, chase, materials, audio and shaders from the local snapshot.
- Keep deletion of `ChaseSequence`, `ForestRunSequence` and `SneakOutSequence`. Remove the old `Chapter05DogSetup` imported from dev because it depends on the deleted sequences and targets the superseded chase.
- Preserve dev's Prologue, Chapter 1 and Chapter 7 scenes, scripts, dialogue, characters and voice work. Checked representative scene/script/Ink files byte-for-byte against dev.
- Combine the Ink validator's `run` tag for the new chase with dev's `act` tag and validator caching.
- Preserve both changes in `StoryDirector`, `VoicePlayer` and `Main.ink`; regenerate `Main.json` from the combined Ink sources.
- Follow dev's removal of duplicate legacy `_Project` runtime files and its asset GUIDs, rather than restoring regenerated local legacy GUIDs. Keep the disabled legacy editor/test assemblies.
- Keep the local PC render-scale/MSAA quality changes and Ch3/Ch4 Foley changes already in the working tree.

## Verification

- No unresolved Git conflict entries or conflict markers in merged code/Ink.
- Git whitespace/error check passes for C#, Ink and assembly definitions. Unity-generated assets retain their existing serialization formatting.
- `git lfs fsck HEAD` and `git lfs fsck github/dev`: PASS. No remaining LFS pointer placeholders in working assets.
- No duplicate asset GUIDs. All 231 external GUIDs referenced by Ch5 resolve in Assets or installed Unity packages.
- The replacement Ch5 scene and principal builder/sequence files match the snapshot; representative other-chapter updates match dev.
- Unity compiles the merged game assembly successfully in a separate QA project; no C# errors.
- Ink validation: PASS, 1,500 randomized walks per entry, including the combined earlier-chapter changes and the replacement Ch5.
- Ch5 at normal player speed with Direct3D 11: all selected gameplay assertions passed (`layout,clean1,many,reach,lam`), including 0 bumps on the clean route, actual capture after mistakes, exactly one failure increment, reach blocked across a ditch, Lam across the bridge, audio/music events, and chapter completion. The raw report still has a FAIL header for one unrelated `UnityEditor.Search.SearchInit.IndexationOnStartup` exception. See `Tools/Ch5/QA/merge_forest_1x.txt`; this is not represented as a clean test-harness pass.
- Ch4 -> Ch5 -> Ch6 -> Ch7 -> Epilogue: **AUTOPILOT PASS**, exit code 0, Direct3D 11 at normal player speed using the QA startup entry point described below. Includes the actual Ch4 key minigame (12 seconds held, no catch), seamless exit into the replacement forest (first forest frame 2.93 seconds after Ch4 exit began), the full chase and later chapters. See `Tools/Ch5/QA/merge_chain_1x.txt`.

### QA environment and limitations

- The QA copy has its own product name/save directory. The user's open Editor and saved progress were not used for automated play.
- Only the QA copy's existing autopilot speed assignment was changed from 3x to 1x. Production autopilot code, gameplay tuning and all gameplay assertions remain unchanged.
- An earlier headless run passed its gameplay assertions but the harness failed on Editor Search and Game View errors. A rendered 3x run recorded one bump on its first clean route and was stopped. The normal-speed rerun above had no gameplay assertion failures. These earlier runs are not claimed as passing runs.
- For the subsequent chain run, the QA-only `MergeBatchQA` entry point removes exactly the delayed Editor Search startup indexing callback before invoking the existing playtest entry point. It does not filter errors, alter game state, or disable gameplay checks. The helper is retained in `Tools/Ch5/QA/MergeBatchQA.cs` for reproducibility; it is outside Assets and is not compiled into the game.
- Results are automated gameplay checks, not a manual visual/audio review or a player-build performance measurement.

The QA project is a separate copy, so the user's open Unity session is not closed or used for automated play. No player build, screenshot or video capture is part of this merge.

Two untracked root images (`f000.png`, `f001.png`) appeared during verification. They were left untouched and excluded from the commits.
