# Chương 2 & Chương 3 — Tài liệu bàn giao

Nguồn thiết kế: `Doc/Chapter 02 – Trên đường theo Mã Giám Sinh Mới.docx` (khung truyện mới: nhân vật hiện đại xuyên vào thân Kiều).
Nhánh git: `feature/chapter02-03`.

## 1. Chơi thử

| Cách | Làm gì |
|---|---|
| Chơi nối từ Chương 1 | Mở `Assets/_Game/Scenes/Chapter01_GiaBien.unity` → Play. Hết Ch1 bấm **Tiếp tục** → Ch2 → Ch3 → màn chờ Ch4. Lựa chọn ở Ch1 (ví dụ đọc kỹ khế) được mang sang. |
| Vào thẳng Chương 2 | Mở `Chapter02_RoadToMaGiamSinh.unity` → Play (truyện tự bắt đầu từ knot `chapter_2`). |
| Vào thẳng Chương 3 | Mở `Chapter03_TuBaHouse.unity` → Play (knot `chapter_3`). |

**Điều khiển:** WASD đi · Shift chạy · Ctrl/C khom người · Chuột xoay camera · **Chuột phải giữ: quan sát** · E tương tác ·
Space/Click tiếp thoại · 1–4 chọn câu · J sổ tay manh mối · Esc tạm dừng (Tiếp tục / Sổ tay / Điều khiển / Chơi lại chương / Thoát).

## 2. Thiết kế đã hiện thực

### Chương 2 — Trên đường theo Mã Giám Sinh (≈ 7–9 phút)
Đường đất đỏ qua rặng tre, ruộng lúa lúc hoàng hôn → quán nước → cổng Lâm Tri về đêm.

1. **Lên đường**: Mã Giám Sinh ân cần (*"Nàng cứ yên tâm…"*), độc thoại nội tâm của nhân vật hiện đại, thẻ hướng dẫn.
2. **Hộ tống (NavMesh)**: Mã Giám Sinh và tùy tùng đi trước, thỉnh thoảng quay lại nhìn. **Thanh Nghi Ngờ** tăng khi tụt lại quá xa hoặc khi quan sát đúng lúc hắn nhìn lại; đầy thanh → thẻ thất bại → checkpoint.
3. **Bốn hướng điều tra** (thứ tự tùy ý, hiện ở góc trái):
   - *Quan sát* (giữ chuột phải lên hắn 2,6 s) → manh mối "bàn tay đếm bạc".
   - *Bắt chuyện* (E) → 3 cách hỏi **có giới hạn thời gian**; hỏi gắt +25 nghi ngờ, giả vờ than mệt −10.
   - *Mẩu giấy* rơi khỏi tay áo hắn giữa đường → đọc văn thư → chọn trả lại hay giấu (giấu = bằng chứng, +15 nghi ngờ).
   - *Nghỉ ở quán nước* (mở khi có ≥ 2 manh mối) → **nghe lén stealth** sau vách tre: khom người dưới tấm phên 1,15 m, hai người thỉnh thoảng liếc qua; bị thấy → làm lại.
4. **"Tú Bà đã chuẩn bị chưa?"** → flash ký ức (thơ Truyện Kiều) → **bảng suy luận** 3 câu (ghi `suy_luan`).
5. **Bỏ chạy hay giả vờ**: chọn chạy sẽ thấy cảnh tưởng tượng hậu quả (cha lại bị bắt) rồi quay lại; giả vờ → **bài kiểm tra mặt nạ** 3 câu có giờ (`gia_vo`).
6. Cổng Lâm Tri về đêm → *"Mình phải giả vờ chưa biết gì."* → bảng tổng kết chương.

### Chương 3 — Tú Bà (≈ 9–12 phút)
Lầu son treo đèn lồng đỏ: sân giếng, phòng Kiều, phòng Tú Bà (cửa sổ + bồn hoa che), phòng sổ sách, cổng khóa ngoài.

1. **Đón tiếp**: Tú Bà niềm nở *"Con cứ xem nơi này như nhà"*; Mã Giám Sinh gọi mụ là "mẹ".
2. **Khám phá tự do 9 điểm**: cô gái áo hồng / áo lam (hỏi chuyện có giờ), cổng chính (khóa từ ngoài), cửa sổ phòng mình (chấn song), nghe lén dưới cửa sổ Tú Bà, ngăn kéo (sổ nợ), hộp sơn son (**văn tự bán thân**), tủ khóa đồng (móc Ch4).
3. **Người canh tuần tra (NavMesh)** với nón tầm nhìn đỏ trên mặt đất. Chỉ bị bắt khi ở **khu vực cấm** (phòng sổ sách, dưới cửa sổ Tú Bà); khom người giảm khả năng bị thấy, bàn ghế/bồn hoa che tầm nhìn. Bị bắt → lôi về phòng (checkpoint), `c3_bi_bat` tăng.
4. Sau ≥ 3 việc, **Mã Kiều** vẫy gọi: *"Cô vẫn chưa biết mình đang ở đâu sao?" … "Hắn đã bán cô cho Tú Bà rồi."*
5. **Hoảng loạn** (tim đập, vignette nhịp, méo hình, rung) → **giọng Kiều thật** (màn vàng, chữ lớn): *"Ngươi từng nói mọi chuyện rất dễ dàng. Bây giờ thì sao?"* → 3 cách đáp (`tra_loi_kieu_that`) → móc nối Ch4: đêm nay người canh sẽ uống rượu.

## 3. Kiến trúc

```
Ink (Main.ink) ──► DialogueManager ──► StoryDialogueView (UI)
                       │  tags
                       ▼
                 StoryDirector ──► DialogueCameraRig · ScreenFx · StoryAudio · StoryHUD · StoryActor
                  │   │   └─ #explore choice ─► ExploreTarget (E) / ObservableTarget (chuột phải)
                  │   └─ # gameplay:<id> ─► StorySequence (Eavesdrop, Document, Deduction, Fx, ActionBeat, Tutorial)
                  └─ checkpoint / thất bại / hoàn thành chương ─► InkStatePersistence (mang trạng thái + manh mối sang chương sau)
Chapter02World / Chapter03World: logic riêng từng chương (hộ tống, nghi ngờ, người canh, dàn cảnh theo # scene:).
```

| Thư mục | Nội dung |
|---|---|
| `Scripts/Core/Story` | StoryDirector, StoryActor, StorySequence, ClueCatalog, StoryAutopilot (QA), StoryCapture (QA) |
| `Scripts/Gameplay` | Chapters (Chapter02World, Chapter03World), Sequences, Stealth (VisionSensor, GuardPatrol, RestrictedZone) |
| `Scripts/Presentation` | DialogueCameraRig, ScreenFx (URP Volume), StoryAudio, ProceduralAudio |
| `Scripts/UI` | StoryUIBuilder (dựng canvas), StoryHUD, StoryDialogueView, JournalView, DocumentView, DeductionView, PauseMenuView |
| `Scripts/Core/Editor` | StorySceneKit, Chapter02Setup, Chapter03Setup, StoryAnimationSetup, StoryBuild |

Chương 1 giữ nguyên. Các thay đổi chung đều là phần **thêm vào**, mặc định tắt: crouch/teleport trong `PlayerMovement`, `SnapBehindTarget` cho camera, explore/tag chặn trong `DialogueManager`.

### Hợp đồng tag Ink (bổ sung)
| Tag | Ví dụ | Ý nghĩa |
|---|---|---|
| `#explore:<id>` *(trên choice)* | `* [Kiểm tra cổng chính #explore:c3_gate]` | Choice được chọn bằng tương tác trong thế giới; hub chỉ gồm choice explore |
| `# gameplay:<id>` | `# gameplay:eavesdrop_c2` | Sequence chặn, chạy sau khi người chơi qua dòng đó |
| `# timer:<s>` + `#timeout` | `# timer:6` | Lựa chọn có giờ; hết giờ chọn choice gắn `#timeout` |
| `# objective:` `# clue:` `# fx:` `# scene:` | | Mục tiêu HUD, manh mối sổ tay, trạng thái màn hình (`memory/imagine/panic/spirit/tension/none`), dàn cảnh |
| `speaker` mới | `NoiTam`, `KieuThat`, `TrangSach`, `TuyTung`, `TuBa`, `MaKieu`, `CoHong`, `CoLam`, `NguoiCanh` | Mỗi loại có kiểu hiển thị riêng (`SpeakerCatalog`) |

Ink cũ Ch2–4 (cốt truyện trước) lưu ở `Data/Dialogue/Ink/Legacy/`, không còn được include.

## 4. Dựng lại scene & kiểm thử

Scene Ch2, Ch3, Ch4 (placeholder), `StoryCanvas.prefab`, `StoryShared.controller` và `Materials/Story/*` đều **sinh bằng code**.
Sửa script setup xong: menu **ThuyKieu ▸ Story ▸ Rebuild Chapter 2 and 3 scenes** (chỉ xóa và tạo lại đúng các asset sinh ra này).

| Kiểm thử | Lệnh (Editor đóng, chạy từ thư mục project) |
|---|---|
| Kịch bản Ink: 4500 lượt ngẫu nhiên, nhánh kẹt, tag lạ, biên `nghi_ngo` | `unity run . -- -executeMethod ThuyKieu.Dialogue.Editor.InkStoryValidator.RunBatch` → `Tools/Ink/InkValidationReport.txt` |
| Hồi quy Chương 1 (103 mục) | `Unity.exe -batchmode -projectPath . -executeMethod ThuyKieu.Core.Editor.StoryBuild.Chapter1RegressionBatch` → `Tools/Ink/Chapter01RegressionReport.json` |
| Autopilot chơi Ch2 → Ch3 | `Unity.exe -batchmode -projectPath . -executeMethod ThuyKieu.Core.Editor.StoryBuild.PlaytestBatch -storyAutopilot -autopilotChain -autopilotSeed 7 -autopilotReport <file>` |
| …đi hết mọi nhánh + ép thất bại | thêm `-autopilotThorough` |
| …kèm ảnh chụp từng khoảnh khắc | thêm `-autopilotCapture <thư mục>` |
| Build Windows | menu **ThuyKieu ▸ Story ▸ Build Windows** (xuất ra `../Builds/Windows`, ngoài repo) |
| Autopilot trên bản build | `ThuyKieu.exe -batchmode -storyAutopilot -autopilotChain -autopilotScene Chapter02_RoadToMaGiamSinh -autopilotReport <file>` |

Lưu ý: chạy dùng Unity.exe trực tiếp (không qua `unity run`) cho playtest, vì `unity run` tự thêm `-quit`.

## 5. Thay model đã rig (Tú Bà, Lính canh)

Hiện **Tú Bà** và **Lính canh** (dùng cho tùy tùng Ch2 và người canh Ch3) là model tĩnh `.obj` có chuyển động giả lập (nhún bước, thở).
Tay của Tú Bà được hạ xuống bằng biến dạng mesh tạm (`TuBa_Relaxed.asset`), không đụng file gốc.

**Mixamo** (cho mỗi model `.obj` đã có trong `Art/Characters/TuBa/StaticV2` và `Art/Characters/LinhCanh/Static`):
1. Upload `.obj` (zip kèm texture) → Auto-Rigger → tải **FBX for Unity, With Skin** (T-pose).
2. Tải thêm (Without Skin, In Place): Tú Bà: *Idle, Talking, Walking, Standing Arguing, Agreeing, Pointing*; Lính canh: *Idle, Walking, Looking Around, Standing Arguing*.
3. Đặt vào `Art/Characters/<Tên>/Models/<Tên>_Rigged.fbx`.
4. Trong `Chapter02Setup` / `Chapter03Setup`, đổi `StaticActor(...)` thành `RiggedActor(..., "<đường dẫn fbx>", "<thư mục texture>", chiều cao, vị trí, hướng, Color.white, shared)` rồi chạy **Rebuild**. Mọi logic (thoại, đi NavMesh, tuần tra) giữ nguyên vì cùng dùng `StoryActor`.

Các cô gái và Mã Kiều tạm dùng model Kiều / Mẹ Kiều đổi màu (`RiggedActor(... tint ...)`). Có model riêng thì thay tương tự.

## 6. Âm thanh

Toàn bộ nhạc / ambience / SFX được tổng hợp lúc chạy (`ProceduralAudio`): nhạc gảy kiểu đàn tranh trên âm giai ngũ cung, gió nhẹ, chim, dế, ếch, cú, chuông gió, tim đập, khóa, giấy…
(Lớp "xì xào" cũ của ambience `house` nghe như tiếng quét nhà nên đã bỏ; ambience nay dài 40 s.)
Muốn thay bằng file thật: chọn `StorySystems/StoryAudio` trong scene → mục **Overrides** → thêm `Id` (ví dụ `road_dusk`, `tuba_warm`, `heartbeat`, `clue`) và gán AudioClip.
Id nhạc: `road_dusk, road_night, tension_low, stealth, tuba_warm, resolve, none`. Ambience: `road, night, house`.

## 6b. Hình ảnh & camera (bản nâng cấp mỹ thuật)

- **Texture CC0** (Poly Haven, xem `Art/Environment/Textures/CREDITS.md`) phủ bằng shader `ThuyKieu/WorldTriplanarLit` (`Assets/_Game/Shaders`): texture lặp theo mét thế giới nên khối hộp kích thước nào cũng đúng tỉ lệ. Tạo vật liệu bằng `StoryArtKit.Surface(...)`.
- **Bầu trời HDRI** (`Art/Environment/Sky`) qua `StoryArtKit.Sky` / `UseSky`; Ch2 đổi trời theo preset ánh sáng (chiều → tối → đêm), Ch3 tối dần khi hoàng hôn.
- **Màu phim**: `ScreenFx` có thêm tonemapping ACES, cân bằng trắng, split-toning (`StoryArtKit.Grade`). SSAO của `PC_Renderer` được tăng.
- **Chi tiết dựng cảnh** trong `StoryArtKit`: cột có chân tảng, mái có đầu đao/diềm, đèn lồng có nan + tua + đung đưa (`LanternSway`), cửa sổ chấn song, câu đối, mành tre, rèm lụa, chum, chậu cây, đá, cỏ (gộp mesh), lá rơi, bụi nắng, đom đóm.
- **Camera** (`ThirdPersonCameraController`): lệch vai phải, FOV riêng cho hội thoại, va chạm kéo vào nhanh/nhả ra chậm; vùng `CameraZone` đổi khung hình theo khu vực; tường có `CameraFadeable` được ẩn khi chắn camera thay vì đẩy camera sát lưng. `DialogueCameraRig._lens` bù khoảng cách cho ống kính hội thoại và đặt người nói lệch 1/3 khung.
- **Chương 1** (scene dựng tay, không tái tạo): menu **ThuyKieu ▸ Chapter 1 ▸ Apply look upgrade** (hoặc `-executeMethod ThuyKieu.Environment.Editor.Chapter01LookUpgrade.ApplyBatch`) — đổi vật liệu sang texture tại chỗ, thêm trời mưa đêm, camera lệch vai, trang trí gian giữa. Chạy lại nhiều lần không sao. Vật liệu của Ch1 nằm ngoài `Materials/Story` (thư mục đó bị xóa khi dựng lại Ch2–3).
- **Ảnh duyệt nhanh không cần Play**: `-executeMethod ThuyKieu.Core.Editor.StoryLookCapture.Batch -lookOut <thư mục>` chụp camera chính và mọi điểm `CAM_*` / `WS_*` / `LOOK_*` của cả 3 chương.
- Lưu ý: shader mới **không được** `FallBack` về URP Lit — dự án bật GPU Resident Drawer và fallback đó làm mọi vật dùng Lit thành màu hồng tím khi Play.

## 7. Ghi chú & giới hạn đã biết

- Font `ChapterVietnamese` là TMP **Dynamic**: khi Play trong Editor, TMP tự thêm glyph và đánh dấu asset đã sửa. Đây là hành vi bình thường; có thể bỏ qua thay đổi đó khi commit.
- Model tĩnh không có cử chỉ thoại; khi thay bản rig sẽ tự có (controller `StoryShared`).
- Chương 4 hiện là màn chờ (`Chapter04_Placeholder`); trạng thái Ink và manh mối đã được lưu sẵn để Ch4 tiếp tục.
- Prologue hiện đại do nhóm khác phụ trách; Ch2–3 đã dùng giọng nội tâm (`NoiTam`) và giọng Kiều thật (`KieuThat`) để khớp khung truyện.
