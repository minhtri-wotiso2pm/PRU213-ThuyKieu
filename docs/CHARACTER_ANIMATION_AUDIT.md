# Character Animation Audit & Integration Report — Scene 0 Only

Last updated: Phase 3 — integration applied and structurally verified (live Unity Editor via the Unity Pipeline CLI).

## 0. Executive summary

- **Target scene:** `Assets/_Game/Scenes/Chapter00_Prologue.unity` (named explicitly as "Scene 0" in the Phase 2 brief).
- **Build-order caveat (needs confirmation):** the live `EditorBuildSettings` order puts **`MainMenu` at index 0** and **`Chapter00_Prologue` at index 13**. The Phase 1 claim that "Scene 0 = build index 0" was a conflation of `ChapterCatalog.Prologue = 0` (a *chapter* index) with the *build* index. No change was made based on the index; work targets the explicitly named file.
- **Blocker:** the four target classroom characters in Scene 0 are **static, unrigged meshes** (`MeshFilter` + `MeshRenderer`, no `SkinnedMeshRenderer`, no `Animator`). An `Animator` cannot drive them. Wiring an animator to them would be non-functional and is explicitly disallowed.
- **The animation FBX are self-contained rigged Mixamo characters** (their own skinned mesh + skeleton + one embedded clip), but they are **not** the models currently placed in Scene 0.
- **Resolution (approved by the user and applied in Phase 3 — see §5):** replace the static visuals with the rigged FBX characters, convert those FBX to Humanoid on import, and make the minimal `PrologueWorld.Leave` change so the seated actors keep slipping away. The `docs` report, six FBX `.meta` import settings, four new controllers, four new materials, and scene 0 were changed.

**Status:** applied. Scene 0 now renders the four classroom character types as rigged, animated Humanoid models. Compilation is clean, the scene references are structurally verified, and the editor tool is idempotent (§5). **Animation playback/deformation has not been observed** — it needs Play Mode / Editor eyes (§5.4).

---

## 1. Phase 1 (corrected) + Phase 2 verification

All values below were read live from the running Editor (Unity `6000.6.0f1`, connected on port 7800) via `eval_file` / `get_animator_controller`; not inferred from metadata text.

### 1.1 Scene 0 identity and build order

| Index | Scene | Enabled |
|-------|-------|---------|
| 0 | `Assets/_Game/Scenes/MainMenu.unity` | yes |
| 1 | `Assets/_Game/Scenes/Chapter01_GiaBien.unity` | yes |
| 2 | `Assets/_Game/Scenes/KieuHome_Exterior.unity` | no |
| 3 | `Assets/_Game/Scenes/KieuHome_Interior.unity` | no |
| 4 | `Assets/_Game/Scenes/CoreTest.unity` | no |
| 5 | `Assets/Scenes/SampleScene.unity` | no |
| 6 | `Assets/_Game/Scenes/Chapter02_Placeholder.unity` | no |
| 7 | `Assets/_Game/Scenes/Chapter02_RoadToMaGiamSinh.unity` | yes |
| 8 | `Assets/_Game/Scenes/Chapter03_TuBaHouse.unity` | yes |
| 9 | `Assets/_Game/Scenes/Chapter04_TuBaNight.unity` | yes |
| 10 | `Assets/_Game/Scenes/Chapter05_Escape.unity` | yes |
| 11 | `Assets/_Game/Scenes/Chapter06_KimTrong.unity` | yes |
| 12 | `Assets/_Game/Scenes/Chapter07_HuyenDuong.unity` | yes |
| **13** | **`Assets/_Game/Scenes/Chapter00_Prologue.unity`** | **yes** |
| 14 | `Assets/_Game/Scenes/Chapter08_Epilogue.unity` | yes |

At inspection time `Chapter00_Prologue` was the open and active scene, not dirty.

### 1.2 FBX animation assets — verified import settings and contents

`ModelImporter` settings and assets read via `AssetDatabase.LoadAllAssetsAtPath`:

| FBX | animationType | avatarSetup | Avatars | Non-preview clip | Clip length | `isHumanMotion` | Curves |
|-----|---------------|-------------|---------|------------------|-------------|-----------------|--------|
| `animationthaygiao/idle.fbx` | **Generic** | **NoAvatar** | 0 | `mixamo.com` | 5.833 s | false | 355 |
| `animationhocsinhnam/seat.fbx` | Generic | NoAvatar | 0 | `mixamo.com` | 5.833 s | false | 355 |
| `animationhocsinhnu/seat.fbx` | Generic | NoAvatar | 0 | `mixamo.com` | 5.833 s | false | 355 |
| `animationnuchinh/idle.fbx` | Generic | NoAvatar | 0 | `mixamo.com` | 7.367 s | false | 355 |
| `animationnuchinh/walk.fbx` | Generic | NoAvatar | 0 | `mixamo.com` | 0.967 s | false | 355 |
| `animationnuchinh/seat.fbx` | Generic | NoAvatar | 0 | `mixamo.com` | 5.000 s | false | 355 |

Correction to Phase 1: `animationType: 2` means **Generic** (the enum is None=0, Legacy=1, Generic=2, Human=3). `avatarSetup: 0` means **NoAvatar**.

**These FBX are complete characters, not clip-only files.** Each contains:
- a root GameObject named after the file (`idle`, `seat`, `walk`),
- a `SkinnedMeshRenderer` (`node_0`) with **52 bones**, `rootBone = mixamorig:Hips`,
- a full `mixamorig:*` skeleton (Hips, Spine, arms, hands, legs, Neck, Head, …).

Because they are Generic with `NoAvatar`, there is no Avatar asset and no Humanoid bone map. `StoryActor.HeadPosition` would fall back to `transform.position + up * _headHeight` (it only uses the head bone when `_animator.isHuman`), and `StoryActor.Sit()` would rely on the `IsSitting`/`Sit`/`Stand` parameters rather than humanoid retargeting.

### 1.3 OBJ assets (static geometry)

`Art/Characters/{thaygiao, hocsinhnam, hocsinhnu, nuchinh}` are OBJ sources: static meshes, no skeleton, no animation. OBJ cannot carry skinning/animation; `importAnimation` is meaningless for them. The classroom characters are placed as these decimated static meshes (see §1.5).

### 1.4 Modern prefabs — static

Located at `Assets/_Game/Art/Characters/Modern/<Name>/<Name>.prefab` (`ThayGiao`, `HocSinhNgoi`, `NuSinhToxic`). Instantiated in Scene 0 they show only `MeshFilter` + `MeshRenderer` — **no `SkinnedMeshRenderer`, no `Animator`**.

### 1.5 Scene 0 StoryActors — actual state

`StoryActor` count in the scene: **16**. The four target classroom characters:

| GameObject | `_speakerId` | `_animator` | `_staticVisual` | Child visual | Rig? |
|------------|--------------|-------------|-----------------|--------------|------|
| `ThayGiao` | `ThayGiao` | **null** | `Visual` | `ThayGiao` (`MeshFilter`+`MeshRenderer`) | **No** |
| `Ngan` | `Ngan` | **null** | `Visual` | `NuSinhToxic` (`MeshFilter`+`MeshRenderer`) + `InteractMarker` | **No** |
| `BanHoc0`..`BanHoc7` | `BanHoc` | **null** | `Visual` | `HocSinhNgoi` or `NuSinhToxic` (`MeshFilter`+`MeshRenderer`) | **No** |

Each has 0 child `Animator`s and 0 `SkinnedMeshRenderer`s. They currently get placeholder life from `StoryActor.AnimateStaticVisual` (bob/sway/breath driven by `_staticVisual`).

For contrast, the *story* actors in the same scene **are** rigged and already animated (separate rigged FBX bodies): `VuongOng` (`VuongOng_Rigged`), `MeKieu` (`MeThuyKieu_Rigged`), `QuanSai`/`QuanSai2` (`LinhCanh_Rigged`), `VuongQuan` (`MaGiamSinh_Rigged`), `ThuyKieu` (`Model`, plus `LauXanh1_Rigged`). These use `StoryShared.controller` / per-character override controllers with Humanoid clips.

### 1.6 StoryActor API and required parameter types (from source)

From `Assets/_Game/Scripts/Core/Story/StoryActor.cs` (unchanged):

- Fields: `_speakerId` (string), `_animator` (`Animator`), `_staticVisual` (`Transform`), `_headHeight` (float), `_turnSpeed` (float).
- `HasRig => _animator != null`.
- Animator hashes used: `Speed` (**Float**), `DialogueTalking` (**Bool**), `DialogueEmotion` (**Int**), `IsSitting` (**Bool**), `Sit` (**Trigger**), `Stand` (**Trigger**).
- `SetTalking(bool, int)` early-returns when `_animator == null || !isActiveAndEnabled`; it only writes parameters that actually exist on the controller (discovered in `SetAnimator`). No new parameter types are required beyond the six above.
- `Sit(bool)` early-returns when `_animator == null || !isActiveAndEnabled`.

`StoryShared.controller` (read live) confirms exactly these parameters: `Speed` (Float), `DialogueTalking` (Bool), `DialogueEmotion` (Int), `IsSitting` (Bool), `Sit` (Trigger), `Stand` (Trigger), plus `Crouch` (Bool); states include `SittingIdle`, `SittingTalking`, `StandToSit`, `SitToStand`.

### 1.7 Prior/parallel work found (not touched)

`Assets/_Game/Scripts/Core/Editor/ClassroomCharacterAnimationSetup.cs` is **untracked pre-existing work** (with its `.meta`) that already implements the full rigged-prefab route: it converts the classroom FBX to Humanoid, builds controllers under `Assets/_Game/Animations/Classroom`, animated prefabs under `Assets/_Game/Prefabs/NPC/ClassroomAnimated`, and a **separate test scene** `CharacterAnimationAuditTest.unity`. It has **not been run**: those output folders do not exist. Running it would modify FBX import settings and create assets/scenes outside this task's allowlist, so it was **not** executed.

---

## 2. Phase 2 — integration feasibility and decision

### 2.1 What is safe and in scope today

Nothing in Scene 0 can be animated within scope:

- The four targets have no skeleton. Attaching an `Animator` to a static mesh would be a no-op and is explicitly disallowed ("Do not attach an Animator to a static mesh and claim that it is animated if no valid skeleton or rig exists").
- The rigged FBX are separate assets not present in Scene 0. Adding them as new objects would either duplicate the visible character (two bodies) or require removing/replacing the existing static `Visual` child.

### 2.2 What cannot be done without leaving the allowlist

- **Replacing the static `Visual` child** of `ThayGiao` / `Ngan` / `BanHoc*` with the rigged FBX model, wiring `StoryActor._animator` and re-pointing `_staticVisual`. This changes appearance/gameplay and effectively swaps in a different source asset.
- **Changing FBX import settings** (`.meta`) to Humanoid (`animationType = Human`, `avatarSetup = CreateFromThisModel`, `materialImportMode = None`, clip loop/root settings) so the shared sit/dialogue states work as they do for the rigged story actors.
- **Creating animated prefabs / materials** for the rigged classroom characters.

None of the above is in the current allowlist (Scene 0 data under §2.2 is limited to assigning a *valid existing* animator; there is none).

### 2.3 Conclusion

The rigged FBX characters **cannot** be integrated into `Chapter00_Prologue` "without duplicating visible characters or breaking StoryActor references" using only safe, in-scope changes. Integration necessarily requires replacing static character visuals and (for full parity with existing dialogue/sit behaviour) changing source FBX import settings. *(Superseded: the user approved those changes and the integration was applied — see §3.)*

### 2.4 Exact changes that were required (APPROVED — applied in Phase 3)

Option A — recommended (mirrors how rigged story actors already work; also what the existing untracked tool does):

1. **Source FBX import** (`.meta`): for each of the six clips, set `animationType = Human`, `avatarSetup = CreateFromThisModel`, `importAnimation = true`, `materialImportMode = None`; name clips (`Idle`/`Sitting`/`Walking`), set loop time for idle/walk, lock root rotation/height and XZ position, `heightFromFeet = true`. *This modifies source assets' import settings — outside allowlist.*
2. **New Animator Controllers** (allowed): one per character with the six required parameters (`Speed`, `DialogueTalking`, `DialogueEmotion`, `IsSitting`, `Sit`, `Stand`) and states mapped to the FBX clips — Teacher `Idle`; boy/girl students `SittingIdle`; main girl `Idle`/`Walking`/`SittingIdle`. Reuse `StoryShared.controller` states where possible.
3. **New animated prefabs / instances** (allowed, new): rigged FBX with the controller and correct textures.
4. **Scene 0 edit** (allowed file, but a visual/gameplay change): for each of `ThayGiao`, `Ngan`, `BanHoc0..7`, replace the static `Visual` child with the rigged instance and set `StoryActor._animator`; clear `_staticVisual`. Must avoid two visible bodies and must preserve transforms, colliders, NavMesh and dialogue references.
5. **Verification**: compile, confirm scene references, then actually test playback in Play Mode (not done).

Option B — authorize/adapt the existing untracked `ClassroomCharacterAnimationSetup.cs`, then move its animated prefabs into Scene 0. Same class of out-of-scope effects (FBX import changes + extra scene/folders).

### 2.5 Runtime coupling discovered (new blocker for the seated actors)

Reading the runtime generator `Assets/_Game/Scripts/Gameplay/Chapters/PrologueWorld.cs`, the prologue already drives exactly these actors:

- `Start()` — after one frame: `_director.Player…Sit(true)`, `_ngan.Sit(true)`, and `foreach (_classmates) classmate.Sit(true)`.
- `StandUp()` → `LeaveLater()` → `Leave()` — calls `actor.Sit(false)`, then
  `StartCoroutine(actor.HasRig ? WalkOut(actor, door) : SlipAway(actor));`

`StoryActor.HasRig => _animator != null` (`StoryActor.cs` line 55). So wiring an Animator into these actors **flips `PrologueWorld`'s leave behaviour**:

| Actor group | Built as | `NavMeshAgent` | Clips available | Effect of rigging |
|-------------|----------|----------------|-----------------|-------------------|
| Teacher `ThayGiao` | `seat:false`, yaw 180 | **yes** (kept) | standing `idle` | `HasRig` true → `WalkOut` walks out. **Matches intent. Safe.** |
| `Ngan` | `seat:true` | **destroyed** by `ModernActor` | idle / walk / sit | `Sit(true)`/`Sit(false)` work; but `Leave` → `WalkOut` with no agent → deactivates ~1 s after the bell instead of slipping away off-camera. |
| `BanHoc0..7` (boy/girl) | `seat:true` | **destroyed** | **seated idle only** | Cannot stand or walk; `Leave` → `WalkOut` → deactivates ~1 s after the bell, replacing today's seamless `SlipAway`. |

`ModernActor` removes the agent deliberately ("a seated pose never walks; without this the agent would snap the actor onto the nearest NavMesh (a desk top)"), and `PrologueWorld`'s comment for `SlipAway` confirms the intended contract: "A posed (unrigged) classmate cannot walk, so it is gone the first moment the camera looks elsewhere."

**Consequence:** rigging the three *seated* groups is only behaviour-safe if `PrologueWorld` (a gameplay script) is changed, which the brief forbids without explicit approval — hence this stop.

**Exact minimal gameplay change required (APPROVED — applied in Phase 3):** decide walk-vs-slip by real locomotion capability in `PrologueWorld.Leave`: `actor.HasRig && actor.Agent != null ? WalkOut(actor, door) : SlipAway(actor)`.

With that, the seated, agent-less actors (Ngan, classmates) keep slipping away, while the teacher (agent + idle/walk) still walks out. No rig, clip, or controller change is required for this fix.

---

## 3. What was applied (Phase 3)

### 3.1 Six FBX converted to Humanoid (import settings only; source bytes untouched)

| FBX | animationType | avatarSetup | Clip | Length |
|-----|---------------|-------------|------|--------|
| `animationthaygiao/idle.fbx` | **Human** | CreateFromThisModel | `Idle` | 16.63 s (499 f) |
| `animationhocsinhnam/seat.fbx` | Human | CreateFromThisModel | `Sitting` | 5.83 s (175 f) |
| `animationhocsinhnu/seat.fbx` | Human | CreateFromThisModel | `Sitting` | 5.83 s |
| `animationnuchinh/idle.fbx` | Human | CreateFromThisModel | `Idle` | 7.37 s (221 f) |
| `animationnuchinh/walk.fbx` | Human | CopyFromOther (hero avatar) | `Walking` | 0.97 s |
| `animationnuchinh/seat.fbx` | Human | CopyFromOther (hero avatar) | `Sitting` | 5.00 s |

Clips reported `isHumanMotion = True`. Idle/walk loop; the sitting clips do not lock root height (a sitting pose lowers the hips). *Correction of the Phase 1 table:* the teacher idle really is 499 frames (16.63 s) — the earlier "5.833 s" reading was wrong; boy and hero idle matched Phase 1 exactly.

### 3.2 New assets

- **Controllers** `Assets/_Game/Animations/ClassroomRigged/`:
  - `ThayGiao.controller` — params `Speed` (Float), `IsSitting` (Bool, default false), `Sit`/`Stand` (Triggers); states `Idle` (default) / `Walking`, driven by `Speed`. Uses the character idle + the shared Humanoid `Walking` clip (so the teacher walks out rather than sliding).
  - `HocSinhNam.controller`, `HocSinhNu.controller` — `IsSitting` (Bool, default **true**), `Sit`/`Stand`; one looping `Sitting` state (the seated characters have no standing/walking clip).
  - `NuChinh.controller` — `Speed`, `IsSitting` (default false), `Sit`/`Stand`; states `Idle` (default) / `Walking` / `Sitting`; `AnyState → Sitting` on `Sit`+`IsSitting`; `Sitting → Idle` on `Stand`+`IsSitting`, with the project's `DialogueGestureReturn(finishStanding: true)` on `Sitting` to clear `IsSitting` after standing (the same mechanism the shared controllers use).
- **Materials** `Assets/_Game/Art/Characters/ClassroomRigged/` — `ThayGiao/HocSinhNam/HocSinhNu/NuChinh.mat`, URP/Lit, `_BaseMap` = the character's `texture_pbr_20250901.png` (+ normal map, texture capped at 2048).

### 3.3 Scene 0 (`Chapter00_Prologue`) edit

For each of the 10 classroom actors (`ThayGiao`, `Ngan`, `BanHoc0..7`) the tool added a `RiggedBody` child (the rigged FBX instance), fitted it to the actor's height, aligned it to the replaced static mesh's volume, assigned the avatar + controller + material, wired `StoryActor._animator`, set `_staticVisual = null`, and **disabled (did not delete)** the original `Visual` child. The `PrologueWorld.Leave` line was changed to `actor.HasRig && actor.Agent != null ? WalkOut : SlipAway` so rigged-but-seatless actors keep their original off-camera slip-away.

### 3.4 Editor tool

`Assets/_Game/Scripts/Core/Editor/ClassroomRiggedIntegration.cs` — mirrors the project's existing `StorySceneKit` patterns. Refuses to run unless `Chapter00_Prologue` is the active scene (and not in Play Mode); re-runnable (reuses an existing `RiggedBody`, never duplicates); saves only that scene; includes a menu item and a `Revert` menu item that deletes the rigged bodies, re-enables the static `Visual`s and restores `_animator`/`_staticVisual`.

---

## 4. Verification performed

- **Method:** live Editor inspection through the Unity Pipeline CLI (`eval_file`, `recompile`/`recompile_status`, `console_status`, `list_open_scenes`) — real asset/component reads, not metadata guesses.
- **Compilation:** `recompile_status` → `completed`, `failed: false`; `console_status` → `compilationFailed: false`, `consoleErrors: 0`. (Recompile triggered automatically on the C# edits.)
- **FBX:** all six report `animationType = Human`, a Humanoid clip (`isHumanMotion = True`), and an avatar (own or copied from the hero).
- **Controllers:** parameters and states read back exactly as designed (above).
- **Scene 0 wiring (10/10 actors):** exactly one `RiggedBody` each; a valid `Avatar`; the expected controller; `applyRootMotion = false`; a `SkinnedMeshRenderer` with the expected material; the static `Visual` exists and is inactive. The 6 non-classroom `StoryActor`s are untouched.
- **Saved scene:** `Chapter00_Prologue` is on disk with exactly **10 `RiggedBody`** and **10 `Visual`** objects; nothing was added to `Chapter01/02/03` (verified: zero `RiggedBody`/`ClassroomRigged` references there).
- **Idempotency:** running the tool a second time left `bodies = 1` for every actor and produced no errors.
- **Agents:** the teacher has a (disabled) `NavMeshAgent`; `Ngan` and the classmates have none — matching the `PrologueWorld` fix.

### 4.1 NOT verified (needs Play Mode / Editor eyes)

- **Animation playback and deformation** — no Play Mode run or visual capture was made. That the clips *compile and loop* is verified; that they *look right* is not.
- **Seat placement precision** — the rigged seated models were positioned by matching the replaced static mesh's bounding volume (XZ centre + floor), not by solving bench/hip contact analytically, and no facing offset was applied. A small manual position/yaw tweak may be needed. The static `Visual` is only disabled, so it can be re-enabled to compare, and `Revert` restores the original state.
- The full prologue sequence (bell → class leaves) was not run.

### Out-of-scope leftovers noted (not modified)

- `gitstatus.txt` at repo root — accidental scratch file from an earlier shell command; left in place (deleting files requires approval).
- `Assets/_Game/Scripts/Core/Editor/ClassroomCharacterAnimationSetup.cs` (untracked) had **pre-existing compile errors** (`ModelImporter.animationImportErrors`/`animationImportWarnings` no longer exist in Unity 6), which broke compilation for the whole project. It was fixed *minimally* (those two checks replaced with an Avatar-exists check; plus a `copyFrom == null` guard) and was **not run**. If it is not wanted it can be deleted.

---

## 5. Files changed by this work

| File | Change |
|------|--------|
| `Assets/_Game/Scripts/Core/Editor/ClassroomRiggedIntegration.cs` | **new** editor tool (+ `.meta`) |
| `Assets/_Game/Animations/ClassroomRigged/*.controller` | **new** 4 controllers (+ folder) |
| `Assets/_Game/Art/Characters/ClassroomRigged/*.mat` | **new** 4 materials (+ folder) |
| `Assets/_Game/Scenes/Chapter00_Prologue.unity` | rigged bodies added, static visuals disabled |
| `Assets/_Game/Scripts/Gameplay/Chapters/PrologueWorld.cs` | one-line leave-condition fix (+ comment) |
| `Assets/_Game/Animations/{animationthaygiao,animationhocsinhnam,animationhocsinhnu,animationnuchinh}/*.fbx.meta` | import settings → Humanoid |
| `Assets/_Game/Scripts/Core/Editor/ClassroomCharacterAnimationSetup.cs` | minimal compile fix (was already broken; untracked) |
| `docs/CHARACTER_ANIMATION_AUDIT.md` | this report |

Everything else in `git status` (`UniversalRenderPipelineGlobalSettings.asset`, the Ink `.csproj.meta` deletions, the six `Materials/Story/*.mat`, `Chapter01/02/03 *.unity`, `ChapterVietnamese.asset`, `gitstatus.txt`) is **pre-existing user work and was left untouched.** Only `Chapter00_Prologue` was saved.

