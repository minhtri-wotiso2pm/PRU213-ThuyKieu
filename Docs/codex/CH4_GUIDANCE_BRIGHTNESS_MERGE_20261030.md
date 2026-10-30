# Chapter 4 guidance and global brightness integration

Base: `53c4d1981f69264db5872de73a0b1ba2426221fc` (`github/dev`).

## Included changes

- `f42d1c3`: separate Unity Editor tools from the runtime player assembly. The Core Editor assembly and the Dialogue, Environment and Player Editor references are Editor-only; preserve their meta GUIDs. Runtime internals remain accessible to the Editor assembly through InternalsVisibleTo. This is the previously used Windows-build configuration repair, now included in source control.
- `ec5e7f5`: add the persistent 50–200% brightness option to Settings / Display, default 100%, resettable. ScreenFx applies the multiplier after chapter mood blending without changing scene lights or guard perception.
- `0b0ae52`: add Chapter 4 objective guidance, Tab reminders, contextual danger lines and nine visual cues. Install into the existing scene and support the chapter builder. Refresh the Ink hub after acquiring keys so the locked cabinet is available immediately and after checkpoint restore, without requiring the ledger interaction first.

The Chapter 4 scene preserves all 17,458 original serialized object IDs, adds 48 and removes none. Chapter 5 is unchanged.

Texture-import rewrites, automatically cleared TMP font data and untracked screenshots from the main workspace are excluded. No reset or deletion of those local files is required by this integration.

## Fresh verification

Unity 6000.6.0f1 in the isolated warmed QA project, with the same 32 scoped source/scene/material/meta files as this integration:

| Check | Result |
| --- | --- |
| C# compilation | PASS |
| LauTuBaChecks: Chapters 3/4 map, rules, surfaces and guidance references | PASS; Unity exit 0 |
| Brightness: all nine actual chapter scenes, neutral grade, changed values while paused, mood composition, persistence between scenes, no accumulation, unchanged world lights | PASS |
| Settings UI arrows, live percentages, 50–200% limits, five-row layout and reset to 100% | PASS |
| Chapter 4 full autopilot with -autopilotGuide | AUTOPILOT PASS; Unity exit 0 |
| Shared HUD regression: Chapter 2 full autopilot | AUTOPILOT PASS; Unity exit 0 |

The Chapter 4 guide probe covers the real key minigame/camera hand-back, direct cabinet availability, visibility of cues from the bed/store, suppressed cues during danger, the inactivity reminder, four checkpoint states, no duplicated cues, J1, and unchanged guard light/sight/sensitivity readings. The full run also completes Chapter 4.

The QA entry helper removes only Unity Search's known batch-only startup indexing callback; gameplay errors and autopilot assertions are not filtered. These are headless, muted checks, not a manual visual/audio review or a performance benchmark. No player build, screenshot or clip was produced for this merge.

Evidence:

- `Tools/LauTuBa/QA/MergeChecks_20261030.txt`
- `Tools/LauTuBa/QA/MergeBrightness_20261030.txt`
- `Tools/LauTuBa/QA/MergeGuide_20261030.txt`
- `Tools/LauTuBa/QA/MergeSharedHUD_20261030.txt`

The automated scene checks do not replace a player evaluating the glow strength, door-crack appearance or text wrapping by eye.
