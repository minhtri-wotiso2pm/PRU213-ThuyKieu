# Selected GiaThuan assets integrated into dev

Base: `dc78a7b4d44df8a68e75bdad67ea5d69bb008086` (`github/dev`).
Source reviewed: `6ce50bb` (`github/GiaThuan`), including `9686745`.

## Scope

Thuy Van's rigged model and the detailed Chapter 2 tea house were already in dev through PR #10. Their existing assets/layout are preserved. The remaining vendor still used MeThuyKieu's model and texture.

- Add GiaThuan's dedicated `BaHangNuoc` FBX, four textures and original meta GUIDs.
- Point `BaHangNuocStory.mat` at the dedicated base/normal textures with a white tint.
- Change the Chapter 2 builder to use the dedicated model/texture directory.
- In the real Chapter 2 scene, replace only the visual child of the existing vendor actor, using Unity Editor APIs. Preserve the actor root, collider, controller, interactions, tea house and story references. Reconnect the actor's Animator field and retain its root-motion anchor.
- Keep Chapter 0/Thuy Van and Chapter 5 identical to the dev base. No package changes.

The source branch's complete Chapter 2 scene was not copied over dev. Its automatic scene-rebuild hook, diagnostic-on-startup script, font changes, unrelated import settings and QA images are excluded. This is a selective integration, not a merge of every outstanding GiaThuan commit.

## Verification

Unity `6000.6.0f1`, isolated warmed project copy, headless and muted:

- Asset/scene upgrade: PASS, Unity exit 0. Vendor humanoid avatar is valid, animation controller resolves, skinned mesh/bones and base textures resolve, and the result survives save/reopen.
- Preserved 4,532 unrelated scene components with identical Editor JSON before/after the model swap.
- Chapter 2 contains no GameObjects with missing MonoBehaviour scripts after reload.
- Thuy Van's inherited skinned model loads successfully.
- Chapter 2 thorough autopilot: `AUTOPILOT PASS`, Unity exit 0. Covers forced failure/checkpoint restore, observe/ask/listen interactions, choices and chapter completion (31 lines, 5 choices).
- No unresolved LFS pointers in the QA Assets tree. New vendor GUIDs have no duplicate matches in other assets.
- Chapter 0/Thuy Van and Chapter 5 files were compared with the dev base and are unchanged.

The QA copy retains the previously verified local Editor/runtime assembly separation used for the Windows build; that separate build repair is not included in this integration. No new executable was built, and no manual visual/audio playtest, screenshots or clips were produced.

## Reproduction

`Tools/GiaThuanMerge/UpgradeTeaVendor.cs` is deliberately outside `Assets`. Copy it into an isolated project's `Assets/Editor` to reproduce the surgical scene upgrade with `-executeMethod UpgradeTeaVendor.Run`. Run Chapter 2 with `-executeMethod UpgradeTeaVendor.Playtest -playScene Assets/_Game/Scenes/Chapter02_RoadToMaGiamSinh.unity -storyAutopilot -autopilotThorough -autopilotReport <report path>`.

The helper removes only Unity Search's known batch-only startup indexing callback, as in the previous merge QA. It does not suppress gameplay errors or modify autopilot assertions/speed.

Evidence: `Tools/GiaThuanMerge/ch2_thorough.txt` and `Tools/GiaThuanMerge/scene_upgrade.txt`.
