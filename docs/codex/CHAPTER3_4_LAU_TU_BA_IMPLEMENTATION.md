# Chương 3–4 · Lầu Tú Bà (góc nhìn thứ nhất) — Tài liệu bàn giao

- Thiết kế đã chốt: `CHAPTER3_4_LAU_TU_BA_DESIGN.md` (cùng thư mục).
- Nhánh: `feature/chapter03-04-horror`.
- Scene mới:
  - `Assets/_Game/Scenes/Chapter03_LauTuBa.unity`
  - `Assets/_Game/Scenes/Chapter04_LauTuBaDem.unity`
- Scene cũ `Chapter03_TuBaHouse`, `Chapter04_TuBaNight` và knot Ink cũ `chapter_3`, `chapter_4` **không bị sửa**. Chúng vẫn chơi riêng được.

## 1. Chơi thử

| Cách | Làm gì |
|---|---|
| Nối chuỗi | `Chapter02_RoadToMaGiamSinh` → Play. Hết Ch2 sang `Chapter03_LauTuBa` → `Chapter04_LauTuBaDem` → `Chapter05_Escape` |
| Vào thẳng | Mở một trong hai scene mới → Play. Ch3 bắt đầu từ knot `chapter_3_lau`, Ch4 từ `chapter_4_dem`, biến các chương trước ở giá trị mặc định |
| Menu chọn chương | `ChapterCatalog` đã trỏ chương 3, 4 sang scene mới |

**Điều khiển:**

| Phím | Tác dụng |
|---|---|
| WASD | Đi |
| Shift | Chạy (vang xa) |
| Ctrl / C | Khom (bước nhẹ, khó bị thấy) |
| Chuột | Nhìn |
| **E** | Tương tác với thứ đang nhìn vào. Cửa và then: **bấm** là giật mạnh (kêu to), **giữ** là làm khẽ |
| Q | Ném hạt cườm, chỉ ở Ch4 (người canh nghe tiếng và đi tới xem) |
| Chuột phải | Nhìn kỹ |
| J | Sổ tay |
| B | Trang Sách |
| Esc | Tạm dừng |

Độ nhạy chuột chỉnh trong **Cài đặt** (`GameSettings.MouseSensitivity`). Tắt rung màn hình bằng mục rung (`GameSettings.ShakeAmount`).

## 2. Đã hiện thực

**Chương 3 (chiều sau mưa → chạng vạng):**

1. **Đến lầu:** cổng cài then sau lưng.
2. **Khám phá tự do:**
   - Phòng mình: vén rèm thấy song sắt, thẻ ghi nợ trên quà.
   - Hồng: dạy luật đá ướt và người canh.
   - Lam: dấu vá cửa, đối đáp lục bát.
   - Mã Kiều ở bếp: thấy cửa sau, biết chìa ở thắt lưng.
   - Cổng: Tú Bà chặn lại và đọc bảng kê nợ.
3. **Chạng vạng** (sau 2 việc): đèn thắp, phòng sổ sách hé cửa. Đọc văn tự, xem sổ nợ, xem tủ khoá.
4. **Nghe tiếng sau cửa thông:** có khoảng 6 giây để vào tủ áo hoặc nấp sau bình phong.
   - Tú Bà chỉ bắt khi **thật sự nhìn thấy**, dùng tầm nhìn có kiểm tra vật cản.
   - Nghe lén trong tủ, nhìn qua khe chớp cửa tủ.
5. **Bị khoá trong phòng:** thoát qua cửa B sang phòng Tú Bà (có checkpoint) → về phòng → then cài → Mã Kiều → Kiều thật → sang Ch4.

**Chương 4 (đêm mưa):** stage lưu trong Ink ở biến `c4_buoc` (0–5).

1. Tú Bà sau song cửa báo mai tiếp khách. Bỏ cảnh tát: cãi lại thì người canh thêm điểm tuần qua cửa phòng Kiều.
2. Lấy trâm, giữ E để nâng then. Thả giữa chừng thì then rơi cạch, tiếng vang 6 m.
3. Hero shot: bóng khách lướt sau giấy cửa phòng khách. Kho vải có vách song nhìn sang Tú Bà ngủ trên sập.
4. Gỡ chìa theo nhịp thở: giữ E khi bà thở ra. Tiếng ngáy ngừng là bà đang cựa mình.
5. Vào phòng sổ sách qua cửa B (kéo then sát chỗ bà ngủ) hoặc cửa C (vặn chìa ngoài hành lang). Xé sổ nợ là tuỳ chọn. Mở tủ, chọn văn tự và bạc.
6. **J1 – gương đồng:** báo trước (ngọn đèn nghiêng, khăn tụt, mưa nhỏ lại) → bóng áo Kiều trong gương, dây đàn đứt → người canh tới cửa C.
   - Cửa C khoá hoặc đã kéo then trong: hắn không vào.
   - Cửa C để mở: hắn vào tìm 6 giây.
   - Đèn tắt hay sáng làm câu lẩm bẩm của hắn khác đi.
7. **J2 – cửa lùa:** chỉ xảy ra khi đủ các điều kiện trong `ScareRules`.
   - Báo trước: gác cổng gọi → ngáy ngừng → ghế kêu, vòng tay → đèn và bóng sau giấy cửa → cửa đập.
   - Sau đó: Tú Bà tới phòng Kiều rồi xuống sảnh. Người canh đổi sang tuyến tìm kiếm. Hành lang thắp thêm 2 đèn.
8. **Chị Lam** (mở khoá móc bằng chìa nhỏ): đi theo **điểm chờ** có kiểm tra an toàn (`LamCompanion`).
9. **Bếp:** Mã Kiều mở mắt rồi quay mặt vào vách. Cửa sau: khoá móc (cần chìa) → then ngang → điều kiện truyện (cần giao kèo, cần chị Lam đã tới nếu đi cùng) → Ch5.

**Biến bàn giao cho Ch5–7 giữ đúng nghĩa cũ:**
- `c4_van_tu`, `c4_tien`, `c4_ru_co_lam`, `c4_so_no`, `c4_canh_giac`, `c4_bi_bat`
- `ev_khe_uoc`, `ev_giay_no`, `ev_tien`, `tien`
- các trang `ts_*`

## 3. Kiến trúc (phần thêm mới)

| Thành phần | File |
|---|---|
| Góc nhìn thứ nhất | `PlayerMovement` (`_firstPerson`: đi ngang, hạ capsule khi khom, `EyeHeight`), `ThirdPersonCameraController` (`_firstPerson`, `LookTarget`, `LimitLook`, `LookAtNow`), `PlayerInteraction` (`_lookOnly`, `IHoldInteractable`, `Pinned`), `UI/FirstPersonReticle` |
| Tiếng động | `Gameplay/Stealth/Noise`, `NoiseZone`, `FirstPersonFootsteps`, `Environment/SurfaceTag`; `PebbleThrow` phát cả `Noise` |
| Người canh | `Gameplay/Stealth/Watchman` (6 trạng thái), `SightModel` (công thức nhận biết), `HideSpot` (sự kiện `Entered`/`Exited`, nhìn qua khe, `_crouch`) |
| Ngôi nhà | `Gameplay/House/HouseDoor` (bản lề hoặc lùa; khoá / then ngoài / then trong; `Gate` cho điều kiện truyện), `PickupItem`, `LampFlame`, `Player/KieuInventory` |
| Chương | `Gameplay/Chapters/LauTuBa/Chapter03LauWorld`, `Chapter04LauWorld`, `TuBaSleeper`, `KeyTheft`, `LamCompanion`, `ScareRules` |
| Dựng scene | `Core/Editor/LauTuBaKit` (tường có lỗ cửa thật, sàn, mái hiên, lan can, bậc, song, cửa), `LauTuBaHouse` (+ `.Rooms`), `LauTuBaSetup` |
| NavMesh | `Environment/NarrowNavMesh`: dựng lúc chạy cho tác tử bán kính 0,22 m, không đổi cài đặt navigation của project |
| Âm thanh mới | `ProceduralAudio`: `after_rain`, `rain_house`, các tiếng bước theo mặt sàn, cửa và then, chìa, ngáy, dây đàn đứt, `sting_low`… `StoryAudio.PlayAt` (âm 3D), `DuckAmbience`; `Presentation/ActorFootsteps` |
| Hiệu ứng | `Environment/LanternGlow` (vải đèn lồng chỉ sáng khi đèn thắp), `PaperShadow` (bóng người trên giấy cửa) |

Các thay đổi trên file dùng chung đều **mặc định tắt** (`_firstPerson`, `_lookOnly`, `_crouch`…), nên các chương khác vẫn chạy như cũ.

## 4. Dựng lại và kiểm thử

| Việc | Lệnh (đóng Editor, chạy trong thư mục project) |
|---|---|
| Dựng lại hai scene mới | Menu **ThuyKieu ▸ Lầu Tú Bà ▸ Rebuild Chapters 3–4**, hoặc `-executeMethod ThuyKieu.Core.Editor.LauTuBaSetup.RebuildBatch` |
| Ảnh duyệt nhanh (không Play) | `-executeMethod ThuyKieu.Core.Editor.LauTuBaSetup.LookBatch -lookOut Tools/LauTuBa/Looks` |
| Kiểm tra quy tắc và bản đồ | `-executeMethod ThuyKieu.Core.Editor.LauTuBaChecks.RunBatch` → `Tools/LauTuBa/QA/Checks.txt` |
| Autopilot | `StoryBuild.PlaytestBatch -playScene Assets/_Game/Scenes/Chapter03_LauTuBa.unity -storyAutopilot [-autopilotThorough] [-autopilotChain] [-autopilotCapture <thư mục>]` |
| Ink | `InkStoryValidator.RunBatch` (đã thêm `chapter_3_lau`, `chapter_4_dem`, mô phỏng việc lấy chìa) |
| Hiệu năng | `StoryPerfProbe.Batch -perfLau -perfOut <file>` |
| Ảnh cố định lúc Play (trước/sau) | `PlaytestBatch ... -storyAutopilot -autopilotViews -autopilotCapture <thư mục>`: ảnh `VIEW_*` 1920×1080 FOV 72 từ mọi điểm `LOOK_*`, kèm bản đồ độ sáng `EXPOSURE_*` |
| Thử lén lút, người canh bật tầm nhìn | `PlaytestBatch -playScene ...Chapter04_LauTuBaDem.unity -storyAutopilot -autopilotStealth` (5 tình huống, ghi `STEALTH ok/FAIL`) |
| Bảng props có sẵn | `-executeMethod ThuyKieu.Core.Editor.LauTuBaAssetSheet.Batch` → `Tools/LauTuBa/AssetSheet` (ảnh và số tam giác) |

Vật liệu sinh ra nằm ở `Assets/_Game/Materials/LauTuBa`. Lệnh Rebuild Ch2–7 không xoá thư mục này, và lệnh đó cũng dựng lại hai scene mới.

## 5. Kết quả kiểm thử và giới hạn

### 5.1 Kiểm thử tự động (lần chạy cuối, 17/10/2026)

| Kiểm tra | Kết quả |
|---|---|
| Biên dịch (Editor và bản build) | Không lỗi C# |
| `LauTuBaChecks` | **PASS**. Gồm: luật nhìn và nghe (SightModel, ScareRules, E2 chỗ nấp), đường đi qua các cửa 2, 3, 6, 8, 9, A, B, C, tuyến tuần của người canh, đường của chị Lam, song kho không phải lối đi, 8 chỗ nấp đủ chỗ cho người, cổng không mở được ở Ch4 |
| Autopilot Ch3 (thường, seed 5) | **PASS**, có ảnh chụp |
| Autopilot Ch3 (thorough, seed 2) | **PASS**. Cố ý bị bắt 2 lần, checkpoint phục hồi đúng |
| Autopilot Ch4 (thường, seed 5) | **PASS**. Then → chìa (theo nhịp thở) → sổ nợ → tủ khoá → J1 (thấy gương) → J2 → chị Lam → bếp → cửa sau |
| Autopilot Ch4 (thorough, seed 2) | **PASS**. Bị bắt 2 lần, phục hồi đúng stage, đồ, cửa, đèn, cờ J1/J2 |
| Chạy nối chuỗi | **PASS**. Ch3 → Ch4 → Ch5 → Ch6 → Ch7 → Ch8 |
| Ink validator | **PASS** (11 điểm vào × 1500 lượt) |

Lệnh chạy lại xem mục 4. Ảnh chụp lúc chơi nằm ở `Tools/LauTuBa/Capture3`, `Tools/LauTuBa/Capture4`.

### 5.2 Hiệu năng

Máy đo: i5-12500H, RTX 3050 Laptop, 16 GB RAM.

**Trong Editor, chỉ tính render, 1920×1080** (`StoryPerfProbe -perfLau`):

| Scene | Tam giác | Đèn điểm | Thời gian một khung hình theo 15 góc nhìn |
|---|---|---|---|
| Ch3 | khoảng 1,9 triệu | 22 | 6,4–13,8 ms |
| Ch4 | khoảng 1,9 triệu | 30 | 5,9–13,0 ms |

Góc nặng nhất là sân trong và hiên tây. Tắt bóng đổ hoặc SSAO giảm thêm 2–4 ms.

**Bản build Windows, 1920×1080:** xem bảng 5.3.

### 5.3 Bản build Windows

- Đường dẫn: `../Builds/Windows/ThuyKieu.exe`
- Dung lượng: khoảng 660 MB
- Lệnh build: `StoryBuild.BuildWindowsBatch`

FPS đo bằng autopilot chạy trên chính bản build, 1920×1080 cửa sổ, VSync mặc định bật (màn hình 144 Hz):

| Scene | FPS trung bình | 1% low | Khung hình chậm nhất |
|---|---|---|---|
| Ch3 | 140 | 63 | 28 ms |
| Ch4 | 144 | 118 | 21 ms |

Cả hai đạt mục tiêu 60 fps.

- Khi bật `-autopilotCapture`, mỗi lần chụp ảnh tốn khoảng 150–190 ms. Đó là chi phí của công cụ chụp, không phải của game.
- Autopilot đi nhanh hơn người chơi thật, nên nên đo thêm bằng tay ở sân trong lúc mưa, vì đó là góc nặng nhất.
- Bản build có 2 cảnh báo shader: `ThuyKieu/WorldTriplanarLit` báo "out of memory during compilation" ở 2 biến thể ForwardLit có `_LIGHT_COOKIES` và `_LIGHT_LAYERS`. Không đèn nào trong hai chương dùng cookie hay light layer, và ảnh chụp từ bản build không có bề mặt hồng nào. Shader này có từ trước (commit P6). Nếu muốn hết cảnh báo, cần tắt light cookies hoặc rendering layers trong URP asset, hoặc strip hai keyword đó. Việc này là đổi cấu hình render pipeline nên mình chưa tự làm.

### 5.4 Giới hạn còn lại

- **Hình ảnh chưa phải bản cuối.** Kiến trúc và đồ đạc được dựng bằng khối cơ bản, trên đó dùng texture CC0 chiếu triplanar. Đó chưa phải mô hình làm tay. Bố cục, ánh sáng và không khí đã đạt; chi tiết mô hình thì chưa. Hai chậu cây ở sân dùng prefab cây sẵn có, thu nhỏ lại.
- **Lời thoại mới chưa có giọng đọc.** Máy này chưa cài Python nên chưa chạy được `generate_voice.py`.
- **Autopilot chỉ kiểm tra luồng truyện**, không kiểm tra kỹ năng lén lút: khi tự chơi nó tắt tầm nhìn của người canh. Độ khó nghe và nhìn (SightModel, bán kính tiếng động) cần người chơi thật thử và chỉnh.
- **Ánh sáng là realtime.** Không bake lightmap. Probe là realtime, cập nhật khi scene khởi động.
- **Cửa đóng chặn đường của NPC.** Navmesh bị cánh cửa khoét kín khi cửa đóng. Người canh không tự mở cửa; mọi cảnh NPC đi qua cửa đều do script mở trước.
- **Bóng trong gương J1** được vẽ bằng shader Unlit để thấy được trong phòng tối. Gương có độ phân giải 0,5 nên hình hơi mờ. Đây là chủ ý của thiết kế, nhưng cần nhìn trên màn hình thật để chỉnh.
- **Scene cũ vẫn còn trong Build Settings.** `Chapter03_TuBaHouse` và `Chapter04_TuBaNight` không bị xoá, nên bản build lớn hơn cần thiết. Gỡ hay giữ là do nhóm quyết định.
- **Các file Unity tự sửa khi mở project** (không phải do code mới): `UniversalRenderPipelineGlobalSettings.asset`, `NavMeshAreas.asset`, `ChapterVietnamese.asset`, hai material Story, và hai file `.csproj.meta` của Ink bị xoá. Cần xem lại trước khi commit. Ngoài ra, build và đo hiệu năng xoá 4 file `Assets/Resources/PerformanceTestRun*.json(.meta)` (của Unity Performance Testing). Nếu cần giữ thì khôi phục bằng git trước khi commit.

## 6. Đợt nâng cấp hình ảnh (17/10/2026)

Mục tiêu: hoàn thiện thẩm mỹ map, kiến trúc, nội thất, vật liệu và ánh sáng. Cốt truyện và gameplay giữ nguyên. Mọi thay đổi nằm trong nguồn dựng scene (`LauTuBaKit`, `LauTuBaHouse`, `LauTuBaHouse.Rooms`, `LauTuBaHouse.Decor`, `LauTuBaMeshKit`, `LauTuBaSetup`), nên lệnh Rebuild không ghi đè mất.

### 6.1 Đã thay đổi

| Hạng mục | Trước | Sau |
|---|---|---|
| Cột hiên | Trụ tròn trơn trên đĩa đá | Chân tảng đá tiện hình chuông, thân sơn son hơi thon, đấu gỗ đỡ xà |
| Đèn lồng | Khối cầu đỏ phát sáng phẳng | Lụa có 12 gân, nắp sơn mài, vòng nan, dây và tua đỏ. Lụa chỉ sáng khi thắp. Ánh nến ấm, mềm, sân có bóng đổ mềm |
| Nền sân | Đá bóng, sáng xanh trắng đều | Đá ẩm độ bóng vừa, normal nhẹ hơn, viên to hơn. Rêu dọc mép kè. Vũng nước viền lượn tối màu. Mưa chỉ rơi chỗ có trời |
| Mái ngói | Viên ngói nhỏ gây nhiễu | Tỷ lệ ngói thật (2,1 m mỗi lần lặp texture) |
| Vật liệu | Gỗ, sơn mài, sàn cùng độ bóng như nhựa | Gỗ cột mờ, sơn mài satin, sàn bóng vừa; vữa ngả ấm. Gốm, lụa, đồng, gang, giấy có độ bóng riêng |
| Phòng Tú Bà | Sập và bàn dạng khối, thảm trơn | Sập sơn mài chân mây có nệm lụa, gối ôm, gối tựa; giường màn; tủ; bộ trà men ngọc trên kỷ, đệm ngồi; đèn dầu; bình phong khung chạm; thảm viền vàng; bình men ngọc; lư hương |
| Phòng Kiều | Khối hộp | Sập gỗ thường, gối, chăn; rèm có nếp (rèm cửa sổ kéo gọn về một bên); rương có đai; áo lụa với thẻ giá; bàn thấp trên chiếu cói, bộ trà men nâu, sách; áo treo móc |
| Phòng Lam | Khối hộp | Chiếu nằm, chăn sờn, gối; rương cũ; bàn mộc với thư, lược gãy, nghiên mực; giỏ kim chỉ; kệ nhỏ thắp hương cho mẹ |
| Phòng Hồng | Gần như trống | Rèm và chăn lụa hồng, tủ riêng, giá treo áo, hộp phấn và bình hoa, khung thêu |
| Sổ sách | Bàn khối, hộp | Bàn chân tiện có băng chạm, ngăn kéo núm đồng, sổ, bàn tính, nghiên, ống bút; ghế thư lại; chồng hộp hồ sơ; bàn bên với hộp sơn mài; kệ lưu trữ ở gian trong; đèn dầu cạnh gương đồng |
| Kho vải | Kệ khối | Kệ và cột gỗ mòn, rương đai sắt, bụi lơ lửng nhẹ |
| Bếp | Hộp đỏ phát sáng | Bếp đất hai miệng lửa, nồi gang và vung gỗ; chụp khói vuốt thon; vữa ám khói; kệ bát và vò; bàn làm bếp mộc; chum nước; chiếu của Mã Kiều |
| Sảnh | Bàn thấp, bình phong | Bàn thờ; hai ghế bành; hai bộ bàn chạm; bình phong mới; sân khấu thấp có thảm, trống, đàn nguyệt và đệm; hoành phi sơn mài viền vàng trên cổng (chưa có chữ); bình sứ cao hai bên cổng; thảm dọc trục |
| Hành lang, góc chum | Chum dạng cầu | Cặp chum gốm thật; ghế băng gỗ |
| Đèn phòng | Cây nến trên sào mảnh | Chân đèn gỗ tiện, khay đồng, đèn dầu thật |
| Ánh sáng | Nội thất tối, bóng ám xanh | Ánh sáng môi trường ban ngày ấm; thêm đèn bù sáng ở sảnh; ban đêm đèn phòng tạo quầng sáng; ánh trăng lọt vào phòng dịu hơn. Thêm 4 reflection probe trong nhà, để sơn mài phản chiếu phòng chứ không phản chiếu trời; probe ban đêm dịu hơn |
| HUD | Mục tiêu hiện hai lần, hướng dẫn phím hiện suốt | Chế độ gọn, chỉ bật cho Ch3–4: mục tiêu chỉ hiện một chỗ, mờ đi sau 9 giây và hiện lại sau checkpoint; hướng dẫn phím mờ sau 14 giây; phụ đề có lớp bóng mờ phía sau |

### 6.2 Asset và nguồn

- **Props của chính project** (không tải thêm, không mua): `Prefabs/Props` gồm Bed, Cabinet, TableSet, CeramicJar, OilLamp, Altar; `Prefabs/Vegetation` gồm ArecaPalm, BananaTree.
  - Các model gốc khoảng 500.000 tam giác mỗi cái. Bản dùng trong nhà được tạo bằng `MeshLodUtility.GenerateMeshLods` có sẵn trong Unity 6.6:
    - giường 67k, tủ 41k, chum 36k, bộ bàn 71k, bàn thờ 65k, cau 53k, đèn dầu 8k tam giác.
  - Mesh lưu ở `Assets/_Game/Art/Environment/LauTuBa/Meshes`. File FBX gốc không bị sửa.
- **Mesh thủ tục tạo bằng code** (`LauTuBaMeshKit`): gỗ vát cạnh, gối và đệm phồng, gốm tiện tròn (bình, chum, ấm, chén, khay, nồi), thân đèn lồng có gân, rèm có nếp, vũng nước viền lượn.
- **Texture:** các bộ CC0 Poly Haven đã có trong `Art/Environment/Textures`.
- **Còn thiếu:** không có model cây đại hay cây sứ, nên chậu cây dùng chuối và cau con trong chậu men.

### 6.3 Ảnh trước và sau

- Chụp lúc Play, cùng vị trí, góc, FOV 72 và độ phân giải 1920×1080, bằng `ViewTour` (`-autopilotViews`).
  - Trước: `Tools/LauTuBa/Views/before`
  - Sau: `Tools/LauTuBa/Views/after`
- Mỗi chương có 17 góc: `C3day`, `C3dusk`, `C4night`. Các góc chính:
  - `01_Hero`: từ cửa phòng Kiều nhìn ra sân.
  - `04_Corridor`: dọc hành lang.
  - `15_HallToYard`: từ sảnh nhìn vào sân.
  - `02_KieuRoom`, `06_TuBaRoom`, `17_TuBaBed`, `08_Office`, `09_Kitchen`, `11_Hall`.
- **Bản đồ độ sáng** (`EXPOSURE_*.png`) chụp kèm: giá trị `LightExposure` ở ngang ngực, nhìn từ trên xuống, để đối chiếu với ảnh.

### 6.4 Gameplay sau khi đổi môi trường

- **Kiểm tra bản đồ (`LauTuBaChecks`): PASS.** Gồm cửa, đường đi, tuyến tuần, tuyến chị Lam, 8 chỗ nấp còn đủ chỗ cho người.
  - Đồ lớn có một hộp va chạm khớp với hình, nằm ở layer Environment (chặn người và che tầm nhìn).
  - Đồ nhỏ không có va chạm, nên không có vật cản vô hình.
- **Ánh sáng khớp với hệ thống phát hiện.**
  - `LightExposure` thêm `ExposureWeight`: đèn bù sáng và trăng lọt vào phòng chỉ tính 30–35%.
  - Thêm `OpenSky`: ở sân và hẻm, trăng hoặc nắng chiều cũng làm lộ người.
  - Kết quả: chỗ trông sáng là chỗ người canh coi là sáng.
- **Thử lén lút với người canh BẬT tầm nhìn (`-autopilotStealth`, Ch4): 5/5 PASS.**

  | Tình huống | Kết quả |
  |---|---|
  | Đứng trong ánh đèn trên tuyến (độ sáng 0,56) | Bị bắt sau 15 giây |
  | Khom trong tối, cách tuyến 5 m (độ sáng 0,15) | Không bị phát hiện suốt 80 giây |
  | Có tường ở giữa, cách 1,7 m | Không thấy |
  | Nấp trước khi người canh nhìn | Không bị tìm ra |
  | Bị thấy lúc chui vào bình phong | Người canh biết chỗ, tìm ra sau 2,7 giây (E2) |

- **Autopilot:** Ch3 và Ch4 thường, thorough và chuỗi Ch3 → Ch8 đều PASS (J1 gương và J2 cửa lùa vẫn chạy đúng).

### 6.5 Hiệu năng

Máy đo: i5-12500H, RTX 3050 Laptop, 16 GB RAM. Cấu hình: 1920×1080 cửa sổ, VSync mặc định bật, màn hình 144 Hz.

**Cảnh:**

| | Trước | Sau |
|---|---|---|
| Tam giác cả ngôi nhà | khoảng 1,9 triệu | khoảng 1,61 triệu |
| Renderer | 2.755 | 2.941 |
| Đèn điểm Ch3 / Ch4 | 22 / 30 | 25 / 30 |

Đèn tăng ở Ch3 là đèn bù sáng của sảnh. Tam giác giảm vì bàn thờ và cây cau 500k được thay bằng bản giảm lưới.

**Trong Editor, chỉ tính render** (`StoryPerfProbe -perfLau`): góc nặng nhất là Ch4 hiên trước sân (15–16 ms), các phòng 5–11 ms.

**Bản build Windows chạy autopilot (`ThuyKieu.exe`):**

| Chương | FPS trung bình | 1% low | Khung chậm nhất |
|---|---|---|---|
| Ch3 | 139–140 | 64 | 21 ms |
| Ch4 | 138 | 71 | 21 ms |

- Lần chạy đầu tiên sau mỗi lần build mới có một khung khựng 312 ms ở Ch3, do nạp shader lần đầu. Hai lần chạy lại không còn.
- Build vẫn báo 2 biến thể `WorldTriplanarLit` "out of memory" như đã ghi ở 5.3. Các biến thể này không được dùng, ảnh chụp từ bản build không có bề mặt hồng.

### 6.6 Phần chưa hoàn thiện

- **Kiến trúc (tường, mái, xà) vẫn là khối cơ bản phủ texture CC0.** Đây không phải model kiến trúc làm tay. Nội thất chính đã có hình khối thật, nhưng chi tiết chạm khắc chỉ là các băng nổi đơn giản.
- **Vải và rèm dùng màu trơn,** chưa có texture hoa văn. Tranh cuộn và câu đối dùng hình vẽ thủ tục có sẵn.
- **Không có model cây đại hay cây sứ.** Nếu nhóm có model với quyền sử dụng rõ ràng thì thay được ngay trong `Yard()`.
- **Ánh sáng là realtime, không bake lightmap.** Ánh bù sáng dùng đèn điểm thay cho GI.
- **Bóng tối ban đêm là chủ ý của thể loại,** nhưng vài góc (bếp, hành lang sau) vẫn rất tối khi chơi trên màn hình thật. Cần chơi thử để chỉnh độ sáng.
- **Phép thử lén lút mới phủ Ch4 và các tình huống chính.** Độ khó tổng thể vẫn cần người chơi thật.

## 7. Đợt mở rộng map 42×38 m và hoàn thiện hình ảnh (17/10/2026)

Làm theo các tài liệu và ảnh tham chiếu:
- `docs/concept/CLAUDE_MAP_AND_VISUAL_UPGRADE.md`;
- `lau-tu-ba-expanded-layout.md`;
- `horror-visual-references.md`;
- hai ảnh concept.

Thay đổi chính:
- Nhà một tầng, mở rộng từ 28×26 m lên 42×38 m.
- **Không scale World.** Nhân vật, cửa và đồ đạc giữ tỉ lệ thật.
- Mọi thay đổi vẫn nằm trong nguồn dựng scene, nên lệnh Rebuild sinh lại đúng như vậy.
- Chỉ dựng lại Ch3 và Ch4. Scene các chương khác không bị dựng lại hay sửa.

### 7.1 Bố cục mới

Quy đổi toạ độ: Unity x = plan_x − 21, Unity z = 19 − plan_y. Các mốc nằm trong `LauTuBaHouse.Plan`.

| Khu | Kích thước, vị trí | Ghi chú |
|---|---|---|
| Sân trong | 18×14 m | Xem dưới bảng |
| Hiên Tây, Đông, Nam | Rộng 3 m | Nhịp cột 2,6–3 m. Lan can chừa lối ở các bậc. Hiên Nam có mái chái riêng áp vào sảnh |
| Hành lang Bắc | Rộng 3 m, chạy suốt chiều ngang | Song gỗ, hai mành tre buông che tầm nhìn, chum, ghế băng, 4 đèn lồng. Đoạn nằm sau dãy phòng hai cánh có trần phẳng |
| Sảnh (phía Nam) | 42×10 m | Xem dưới bảng |
| Cánh Tây | Phòng Kiều và phòng Lam, mỗi phòng 9×8,5 m | Xem dưới bảng |
| Cánh Đông | Phòng Hồng, phòng khách | Như mục 6.1, dàn lại theo kích thước mới |
| Dãy Bắc | Bếp 10 m, kho 8 m, phòng Tú Bà 11 m, sổ sách 7 m, lưu trữ 6 m | Xem dưới bảng |
| Hẻm sau | Rộng 3,5 m, dọc tường Tây | Lộ trời, có mưa |

Chi tiết từng khu:
- **Sân trong:**
  - Một cây sứ (cây đại) đặt lệch tâm trên bồn đá tròn, có hoa rụng.
  - Bồn non bộ.
  - Chum hứng mưa, ghế đá và chậu cây xếp sát mép sân.
  - Giữa sân để trống cho tuần tra và lối đi.
  - Rêu dọc kè, dải nền ướt dưới mép mái, 6 vũng nước.
- **Sảnh:**
  - Trục đi thẳng: cổng → thảm → sân.
  - Hai nhóm tiếp khách hai bên, mỗi nhóm có thảm, bộ bàn, ghế bành.
  - Bàn thờ phía Tây, sân khấu nhỏ phía Đông.
  - Bình phong, kỷ, hoành phi, 4 cửa sổ.
- **Phòng Kiều** chia vùng:
  - chỗ ngủ (sập, màn);
  - chỗ trang điểm;
  - bàn viết trên chiếu;
  - rương;
  - tủ áo (chỗ nấp).
- **Phòng Lam:** chiếu, bàn thư, kệ thờ, bình phong (chỗ nấp).
- **Dãy Bắc:**
  - Bếp có cửa sau ra hẻm, và lối sang kho có rèm vải.
  - Kho có song gỗ nhìn sang sập Tú Bà.
  - Cửa B nối phòng Tú Bà với phòng sổ sách.
  - Phòng sổ sách chia hai khu, làm việc và lưu trữ, ngăn bằng vách song có lối ở giữa.

### 7.2 Hình ảnh đã làm thêm

- **Cửa:** đủ khung, ngưỡng và độ dày.
  - Cửa bản lề: hai cối bản lề sắt trên khung, nẹp sắt ở hai mặt cánh.
  - Cửa ván rộng tới 1,3 m: then gỗ có hai khoá sắt và núm, ở mặt cài cửa.
  - Cửa lùa: ray gỗ phía trên có gờ hai bên, ray mòn dưới sàn, tay nắm sắt ở hai mặt.
- **Cây sứ:**
  - Mesh thủ tục: thân cong, 5 cấp nhánh, cụm lá ở đầu cành, chùm hoa 5 cánh.
  - Lá và hoa hiển thị hai mặt. Lá đổ bóng, hoa không đổ bóng.
- **Phòng Tú Bà theo concept:**
  - Sập sơn mài có nệm và gối.
  - Thảm đỏ viền vàng 6,4×4,6 m.
  - Bàn trà với ba đệm ngồi.
  - Kỷ thờ với lư hương, bình, chân nến; tranh cuộn.
  - Giường màn.
  - Tủ có bình.
  - Bàn sổ có bàn tính, ống bút, sổ; ghế bành.
  - Bình phong; đèn dầu đứng.
- **Bếp:**
  - Thêm lu nước có nắp và gáo dừa cạnh cửa sau.
  - Thêm ghế thấp với nia rau.
  - Ánh trăng lọt qua song cửa bếp, giúp góc cửa sau vẫn nhìn thấy được trong đêm.
- **Hành lang:** mành tre buông ở hai nhịp song. Mành chặn tầm nhìn như vách, có collider ở layer Environment.
- **Ánh sáng:**
  - Đèn nào cũng có nguồn có lý do: đèn lồng hiên và sân, đèn cổng, đèn sảnh, đèn dầu trong phòng, lửa bếp, trăng.
  - Đèn bù sáng (ban ngày 22 đèn, ban đêm 11 đèn trăng) chỉ tính 30–35% với người canh.
  - Không thêm đèn có bóng đổ theo diện tích sàn: chỉ 6 đèn lồng ở sân có bóng.
  - Ch3 giữ nắng chiều và hoàng hôn, Ch4 giữ đêm mưa.

### 7.3 Gameplay sau khi dựng lại

Mọi điểm gameplay được dời theo bố cục mới:
- spawn, vùng nhiệm vụ, checkpoint;
- tuyến tuần, đường chị Lam, chỗ nấp;
- vùng cấm, vùng tiếng ồn;
- mưa, probe, NavMesh.

Tầm nhìn và thính giác của người canh **không đổi**. Chỉ tuyến tuần được dàn lại cho hợp với nhà mới.

**`LauTuBaChecks`: PASS cả Ch3 và Ch4.** Các mục đã kiểm tra:
- Đường chính, đường về, đường từ bếp qua kho, đường giữa Lam và sân.
- Đi qua được từng cửa và lối:
  - cửa 2, 3, 6, 8, 9, A, B, C và cửa sau;
  - lối vào phòng lưu trữ;
  - bậc Tây, bậc Đông, bậc Nam;
  - khe phía Bắc.
- Song gỗ ở kho không phải lối đi (đi vòng mất 11,7 m).
- Mọi tuyến tuần và đường của Lam đều đi được.
- 8 chỗ nấp còn đủ chỗ cho người.
- Mới thêm: khi đóng cánh, **cửa B, cửa C và cửa sau chặn đường thật**. Không còn đường đi, hoặc phải vòng trên 6 m.

**Thời gian đi giữa các mục tiêu (Ch4, đo trên NavMesh):**

| Chặng | Quãng đường | Đi thường | Đi khom |
|---|---|---|---|
| Phòng Kiều → sập Tú Bà | 20,6 m | 13 s | 22 s |
| Sập → tủ lưu trữ (qua cửa B) | 21,4 m | 14 s | 23 s |
| Tủ → phòng Lam | 45,6 m | 29 s | 48 s |
| Phòng Lam → bếp | 24,3 m | 16 s | 26 s |
| Bếp → cửa sau → hẻm | 6,9 m | 5 s | 7 s |

**Thử lén lút với người canh bật nhận biết (`-autopilotStealth`): 5/5 PASS.**

| Tình huống | Kết quả |
|---|---|
| Đứng trong ánh đèn trên tuyến (độ sáng 0,61) | Bị bắt sau 21,8 giây |
| Khom trong tối, cách tuyến 6 m (độ sáng 0,17) | Độ nghi 0 suốt 100 giây |
| Có tường bếp ở giữa, cách 1,8 m | Không thấy |
| Nấp trước khi người canh nhìn | Không bị tìm ra |
| Bị thấy lúc chui vào bình phong | Người canh biết chỗ, tìm ra sau 2,7 giây |

Quy tắc chỗ nấp giữ nguyên: người canh chỉ biết chỗ nấp nếu thấy người chơi chui vào.

**J1 và J2:**
- Khoảng nghỉ 60 giây sau J1 giữ nguyên (`ScareRules`).
- Nhà lớn hơn, nên người đi nhanh có thể tới bếp trước khi hết 60 giây.
  - Khi đó J2 vẫn xảy ra lúc người chơi đang ở trong bếp: nghe tiếng mụ thức dậy, hành lang phía sau sáng đèn.
- **Cửa sau không bao giờ bị khoá để chờ J2.**
  - Nếu người chơi ra cửa sau trước khi hết khoảng nghỉ, J2 bị bỏ qua.
  - Đây là chủ ý: Ink và điều kiện mở cửa sau chỉ cần khế ước, và chị Lam nếu chị đi cùng.
- Mỗi cảnh doạ đều có đường phản ứng: nấp, lùi vào phòng, hoặc đi vòng qua kho.
- Sau checkpoint, cảnh doạ không phát lại y hệt. Cú đập cửa của J2 chỉ chạy lần đầu; lần sau Tú Bà đã đứng sẵn ở cửa.

**Autopilot:**
- Ch3 và Ch4 chạy thường: PASS.
- Thorough Ch3 và Ch4: PASS. Gồm:
  - ép thua rồi khôi phục checkpoint;
  - bị bắt rồi khôi phục checkpoint;
  - nhánh đưa chị Lam theo (chị tới được bếp).
- Chuỗi Ch3 → Ch4 → Ch5 → … → Ch8: PASS.

**HUD:** vẫn dùng chế độ gọn như mục 6.1.

### 7.4 Ảnh trước và sau

Cách chụp:
- Chụp lúc Play bằng `ViewTour`, cùng vị trí, FOV 72, 1920×1080, HUD ẩn.
- Mỗi chương có 17 góc, kèm bản đồ độ sáng `EXPOSURE_*.png`.

Thư mục:
- Trước khi mở rộng: `Tools/LauTuBa/Views/before_expand`
- Sau khi mở rộng: `Tools/LauTuBa/Views/after_expand`
- Ảnh ghép trước và sau đặt cạnh nhau: `Tools/LauTuBa/Views/compare_expand` (`CMP_*.png`). Gồm các góc:
  - sân và hiên (`C3dusk_10_Courtyard`, `C4night_01_Hero`);
  - phòng Kiều;
  - phòng Tú Bà (`06`, `17`);
  - phòng sổ sách;
  - bếp;
  - sảnh (`11_Hall`, `15_HallToYard`);
  - hành lang.

### 7.5 Hiệu năng

| | Trước mở rộng | Sau mở rộng |
|---|---|---|
| Tam giác cả ngôi nhà | khoảng 1,61 triệu | khoảng 1,82 triệu |
| Renderer | 2.941 | 3.897 |
| Đèn điểm Ch3 / Ch4 | 25 / 30 | 26 / 34 |
| Render trong Editor, góc nặng nhất | 15–16 ms | 13,6 ms |

**Bản build Windows chạy autopilot** (`ThuyKieu.exe`, 1920×1080 cửa sổ, không bật chụp ảnh):

| Chương | Lần chạy | FPS trung bình | 1% low | Khung chậm nhất |
|---|---|---|---|---|
| Ch3 | 1 (ngay sau build) | 139 | 40 | 92 ms |
| Ch3 | 2 | 140 | 59 | 28 ms |
| Ch4 | 1 | 141 | 69 | 28 ms |
| Ch4 | 2 | 141 | 71 | 28 ms |

- FPS trung bình giữ khoảng 140 như trước khi mở rộng, vượt xa mục tiêu 60.
- Lần chạy Ch3 đầu tiên có khựng 92 ms do nạp shader lần đầu, như ở 6.5.
- 1% low của Ch3 sát mức 60, vì lượt chạy Ch3 chỉ khoảng 10 giây (1.100 khung) nên 1% gồm cả các khung lúc nạp cảnh. Ch4 chạy dài hơn và ổn định ở 69–71.

### 7.6 File đã đổi trong đợt này

**Nguồn dựng scene** (`Assets/_Game/Scripts/Core/Editor`):
- `LauTuBaHouse.cs`: mặt bằng mới, tường, mái, hiên, cửa.
- `LauTuBaHouse.Rooms.cs`: nội thất, đèn, sân.
- `LauTuBaHouse.Decor.cs`: cây sứ, ghế đá, mành tre.
- `LauTuBaMeshKit.cs`: mesh cây sứ, hoa rụng.
- `LauTuBaKit.cs`: bản lề, ray, then cửa.
- `LauTuBaSetup.cs`: mọi điểm gameplay, tuyến, vùng, mưa, probe, NavMesh.
- `LauTuBaChecks.cs`: tuyến mới, kiểm tra cửa đóng, thời gian đi.

**Gameplay** (`Assets/_Game/Scripts/Gameplay/Chapters/LauTuBa`):
- `Chapter03LauWorld.cs`: vị trí ảnh chụp.
- `Chapter04LauWorld.cs`: vị trí ảnh chụp, phép thử lén lút, J2 khi người chơi ở bếp.
- `ScareRules.cs`: chú thích.
- `ViewTour.cs`: khung bản đồ độ sáng.

**Scene sinh lại:**
- `Chapter03_LauTuBa.unity`, `Chapter04_LauTuBaDem.unity`.
- Mesh mới nằm trong `Assets/_Game/Art/Environment/LauTuBa/Meshes`.

### 7.7 Phần chưa hoàn thiện

- **Kiến trúc vẫn là khối cơ bản phủ texture CC0,** như mục 6.6. Chạm khắc chỉ là các băng nổi đơn giản. Vải và rèm vẫn màu trơn.
- **Cây sứ là mesh thủ tục.** Nhận ra được hình dáng và hoa, nhưng chưa bằng model làm tay.
- **Phòng rộng hơn nên còn những mảng sàn trống:**
  - giữa sảnh (cố ý, để giữ trục đi thẳng);
  - giữa phòng sổ sách;
  - giữa bếp.

  Muốn phòng đầy hơn thì nên thêm đồ có công năng, không rải đồ trang trí.
- **Bếp và sảnh ban đêm rất tối trên ảnh chụp,** nhưng vẫn thấy được hình khối. Cần thử trên màn hình thật để chỉnh độ sáng (gamma) chung.
- **Độ khó tổng thể trên nhà lớn cần người chơi thật đánh giá,** nhất là chặng tủ → phòng Lam (29–48 giây).

## 8. Đợt ánh sáng và vật liệu theo ảnh vibe (18/10/2026)

Làm theo `docs/concept/PRIMARY_VIBE_DIRECTION.md` và `lau-tu-ba-primary-vibe.png`. Giữ nguyên mặt bằng 42×38 m, nhiệm vụ và cốt truyện. Mọi thay đổi nằm trong nguồn dựng scene và shader, nên Rebuild sinh lại đúng kết quả.

### 8.1 Hiện trạng trước khi sửa và nguyên nhân

| Triệu chứng | Nguyên nhân tìm được |
|---|---|
| Cột chìm đen | Cột dùng texture `lacquered_cherry_wood` gần đen (sRGB trung bình 0,18/0,11/0,06), nhân thêm tint đỏ (0,54/0,17/0,12) nên albedo còn khoảng 0,05 |
| Xà, trần chìm đen | `dark_wood` nhân tint 0,4/0,27/0,21 và `dark_wood` tint 0,26 cho trần |
| Vân gỗ sai hướng | Triplanar theo trục thế giới: mọi mặt đứng có vân ngang, kể cả cột |
| Sân xanh trắng đều | Ba nguyên nhân cộng lại: (1) độ bóng hằng số 0,4 trên đá thật ra rất nhám (roughness trung bình 0,83), phản chiếu bầu trời ở góc nhìn xiên; (2) probe sân 0,4; (3) grade đêm dùng chung tô xanh mọi vùng tối (shadow tint 0,38/0,43/0,55) |
| Đèn lồng không chiếu sáng xung quanh | Cường độ 1,2–1,5 trên tầm 5,5 m quá yếu. Với 6 đèn có bóng, chính nắp dưới và tua của đèn đổ bóng xuống ngay dưới đèn và in hình đèn lên mái hiên |
| Thân đèn phẳng, không thấy chất vải | Emission đều một màu |
| Material Ch3 và Ch4 lẫn nhau | `LTB_Paving`, `LTB_PavingWet`, `LTB_Tiles`, `LTB_AlleyMud`, `LTB_DripLine` là một asset dùng chung nhưng giá trị khác nhau ngày/đêm. Chương dựng sau cùng ghi đè chương kia |
| Không có roughness/AO | Project chỉ có diffuse và normal. Shader dùng độ bóng hằng số |

**Shader `WorldTriplanarLit`:**
- GI chỉ lấy từ `SampleSH`, không đọc lightmap. Vì vậy chỉ bake thì không có tác dụng.
- Ánh sáng trong nhà là đèn realtime có bật/tắt theo gameplay (đèn hành lang sáng khi J2, đèn Tú Bà, đèn phòng). Vì vậy **không bake**, cũng không thêm hỗ trợ lightmap.

### 8.2 Đã áp dụng

**Shader** (`Assets/_Game/Shaders/WorldTriplanarLit.shader`): hai tính năng tuỳ chọn theo material, mặc định tắt.
- Material các chương khác không bật nên hiển thị như cũ.
- `_ARM_MAP`: đọc map AO/Roughness/Metal thật. Độ bóng bằng `_Smoothness × (1 − roughness)`; kênh AO đưa vào occlusion.
- `_GRAIN_FOLLOWS_OBJECT`: hình chiếu xoay để vân gỗ chạy theo chiều dài từng thanh.
  - Chiều dài lấy từ UV3 của mesh: `LauTuBaMeshKit.RoundedBox` ghi trục dài vào UV3.
  - Nếu mesh không có UV3: lấy trục scale dài nhất của object, nếu scale gần đều thì lấy trục đứng.
  - Normal map xoay theo.
  - Lớp biến thiên màu cỡ lớn cũng chạy theo vân, không cắt chéo.

**Texture:**
- 7 map ARM 1k JPG CC0 từ Poly Haven, cùng bộ với diffuse và normal đang dùng (`*_arm_1k.jpg`, ghi trong `CREDITS.md`): dark_wood, planks_brown_10, lacquered_cherry_wood, old_wooden_floor_02, monastery_stone_floor, brick_floor, plastered_wall_02.
- Map ARM của `lacquered_cherry_wood` đã tải nhưng chưa dùng, vì cột đã chuyển sang `dark_wood`.
- Các bộ còn thiếu ARM nên giữ độ bóng hằng số: `clay_roof_tiles_02`, `japanese_stone_wall`, `brown_mud_*`, `mossy_rock`.
- **Texture sinh bằng code** (`LauTuBaTextures`, không phải dữ liệu PBR quét thật):
  - vân dệt lụa và vải bông (albedo và normal) cho rèm, đệm, áo;
  - lụa đèn lồng có đường gân tre và hai vòng nẹp, khớp đúng 12 gân của mesh.

**Gỗ:**

| Material | Texture | Tint | Độ bóng |
|---|---|---|---|
| `LTB_PillarWood` (mới, thân cột) | dark_wood | 0,9/0,62/0,56 | ARM, tối đa 0,62 → trung bình khoảng 0,23 |
| `LTB_Timber` (xà, kèo, khung) | dark_wood | 0,72/0,56/0,5 | ARM, trung bình khoảng 0,19 |
| `LTB_Lacquer` (đồ sơn mài, mặt bàn, sập) | dark_wood | 0,8/0,48/0,45 | Hằng số 0,45, đều như lớp sơn phủ |
| `LTB_FloorBoards` | old_wooden_floor_02 | 0,86/0,66/0,82 | ARM, trung bình khoảng 0,3 (sàn được chân người đánh bóng) |
| `LTB_Ceiling` (trần, ván mái hiên) | planks_brown_10 | 0,56/0,42/0,36 | ARM, rất nhám |
| `LTB_DoorPlanks` | planks_brown_10 | 0,72/0,55/0,45 | ARM |

Cột và xà nhám hơn mặt bàn và sập sơn mài. Mọi material gỗ đều bật vân theo chiều dài.

**Đá:**
- Sân ban đêm có tint tối hơn (0,44/0,42/0,39). Độ bóng theo ARM (trung bình khoảng 0,14), nên đá khô và ẩm khác nhau từng viên. Normal giảm từ 0,7 xuống 0,3.
- Dải ướt sát hiên có độ bóng đều 0,55 (màng nước). Vũng nước giữ material riêng.
- Gạch sảnh và vữa tường dùng ARM thật.

**Vải:**
- Mọi material `LTB_Silk*` và `LTB_Cotton*` có vân dệt và normal. Mesh rèm vốn đã có nếp gấp và nếp xoè ở gấu.
- **Đèn lồng:**
  - Thân đèn khi thắp chỉ phát sáng theo map lụa: sáng ở bụng, gân và vòng nẹp tối.
  - Nền gần đen để ngọn nến bên trong không làm cháy màu đỏ thành màu kem.
  - Độ sáng emission dưới 1, vì grade cộng khoảng 1 stop exposure trước tonemap.
  - Độ sáng thân đèn đặt riêng, tách khỏi đèn chiếu môi trường.

**Ánh sáng:**
- Đèn lồng hiên sân: 1,3 → 3,4, tầm 5,6 → 6 m. Đèn cổng: 1,2 → 3. Đèn sảnh: 1,4 → 3,6. Đèn hành lang: 1,5 → 3,2. Màu đỏ cam hơn (1/0,47/0,22).
- **Không thêm đèn có bóng:** vẫn 6 đèn sân có bóng như trước.
- Thêm 2 đèn nhỏ không bóng trên hai bàn trà ở sảnh.
- Mọi phần của đèn lồng không đổ bóng từ chính ngọn nến của nó. Nhờ vậy dưới mỗi đèn có vùng sáng trên sàn, mái hiên không còn bóng hình đèn.
- **Trăng Ch4:** cường độ giữ 0,5, vì `LightExposure` tính trăng ở vùng trời trống. Chỉ màu bớt xanh (0,6/0,64/0,76).
- **Ambient Ch4:** giảm, không tăng (sky 0,27/0,31/0,43 → 0,23/0,26/0,33).
- Probe sân ban đêm: 0,4 → 0,22. Các probe trong nhà giữ 0,4.
- **Grade Ch4** đặt riêng trong `LauTuBaSetup`. Grade đêm dùng chung cho Ch5 không đổi.
  - Exposure 0,95 và contrast 11 giữ nguyên.
  - Shadow tint 0,38/0,43/0,55 → 0,43/0,42/0,46. Temperature −3 → −1.
- **Exposure toàn cảnh không đổi** ở cả Ch3 (0,6) và Ch4 (0,95). Ambient Ch3 không đổi.

**Bố cục sảnh:**
- Hai nhóm tiếp khách dời từ x ±10 vào x ±7, nằm trong hướng nhìn từ sân.
- Thêm hai bàn trà có ghế bành ở hàng trước (x ±3,3, z −11,2), mỗi bàn có bộ trà và một đèn nhỏ phía trên.
- Rèm lụa đỏ trầm buộc gọn vào cột ở bốn khoang trước sảnh. Khoang trục giữa để trống.
- Rèm **không có collider:** đã buộc gọn sát cột nên không tạo chỗ che mà người chơi tưởng là kín nhưng người canh vẫn nhìn xuyên qua.
- Điểm dừng tuần đêm trong sảnh dời từ (1,5; −12,5) về trục (0; −12,5), để đường tuần cách bàn trà khoảng 0,5 m.

### 8.3 Kiểm chứng shader bằng URP Lit

Công cụ `LauTuBaShaderCompare` (editor, batch) mở scene Ch4 và không lưu scene:
- Đặt hai tấm giống nhau dưới cùng một đèn lồng hiên: một dùng URP Lit (UV mesh chỉnh cùng số mét mỗi lần lặp), một dùng WorldTriplanarLit.
- Cùng texture, tint, độ bóng, camera và đèn.
- Ảnh ở `Tools/LauTuBa/Views/shader_compare`.

Kết quả:
- **`COMPARE_wood_matched`, `COMPARE_stone_matched`:** cùng đầu vào thì hai shader cho độ sáng, sắc độ và độ nổi normal gần như trùng nhau. Vậy shader triplanar chiếu sáng đúng; cột đen là do giá trị material và ánh sáng.
- **`COMPARE_*_production`:** tấm phải dùng material thật, có ARM và vân theo thanh.
- Ảnh editor không có grade của game, nên sân trong ảnh editor ấm và tối hơn lúc Play. Đó là lý do tìm ra phần xanh do grade gây ra.

### 8.4 Ảnh trước và sau

- Chụp lúc Play bằng `ViewTour`, cùng vị trí, FOV 72, 1920×1080, HUD ẩn. Mỗi chương 17 góc, kèm bản đồ `EXPOSURE_*.png`.
  - Trước: `Tools/LauTuBa/Views/before_light` (bản sau đợt mở rộng, chưa sửa ánh sáng).
  - Sau: `Tools/LauTuBa/Views/after_light`
- Ảnh ghép hai bên: `Tools/LauTuBa/Views/compare_light` (`CMP_*.png`). Gồm:
  - hiên và sân (`C4night_01_Hero`, `C3dusk_10_Courtyard`);
  - sảnh (`C3dusk_11_Hall`, `C4night_15_HallToYard`);
  - phòng Tú Bà (`C3dusk_06_TuBaRoom`, `C3dusk_17_TuBaBed`);
  - phòng Kiều, phòng sổ sách, bếp.

### 8.5 Gameplay sau khi đổi ánh sáng

- **`LauTuBaChecks`: PASS cả Ch3 và Ch4.** Đường đi, cửa, tuyến tuần (gồm điểm sảnh mới), đường Lam, chỗ nấp, cửa đóng chặn đường.
- **Thử lén lút, người canh bật nhận biết: 5/5 PASS.**

  | Tình huống | Trước | Sau |
  |---|---|---|
  | Đứng dưới đèn lồng trên tuyến | Độ sáng 0,61, bị bắt sau 21,8 s | Độ sáng 0,75, bị bắt sau 20,5 s |
  | Khom trong tối, cách tuyến 6 m | 0,17, độ nghi 0 | 0,17, độ nghi 0 |
  | Tường ở giữa / nấp trước / bị thấy lúc nấp | PASS | PASS (tìm ra sau 2,7 s) |

  Vùng dưới đèn sáng hơn trên màn hình và người canh cũng coi là sáng hơn. Vùng tối giữ nguyên. Hệ số trần 1,5 trong `LightExposure` giới hạn mức tăng.
- **Bật/tắt đèn:**
  - Đèn hành lang tắt đến khi J2, rồi sáng (`LightCorridor`). Thân đèn đổi material theo trạng thái đèn (`LanternGlow`).
  - Checkpoint lưu và khôi phục trạng thái này. Đã chạy trong autopilot thường và thorough của Ch4, gồm ép thua rồi khôi phục và nhánh chị Lam.
- **Autopilot:**
  - Ch3 và Ch4 thường: PASS.
  - Thorough Ch3 và Ch4: PASS.
  - Chuỗi Ch3 → Ch8: PASS.

### 8.6 Hiệu năng

| | Trước | Sau |
|---|---|---|
| Tam giác cả ngôi nhà | 1,82 triệu | 1,89 triệu (bàn ghế và rèm sảnh) |
| Renderer | 3.897 | 4.031 |
| Đèn điểm Ch3 / Ch4 | 26 / 34 | 26 / 36 (2 đèn không bóng ở sảnh) |
| Đèn có bóng | 6 | 6 |

**Bản build Windows chạy autopilot** (`ThuyKieu.exe`, 1920×1080 cửa sổ, không bật chụp ảnh, cùng máy và cấu hình như mục 7.5):

| Chương | Trước (lần 2) | Sau lần 1 (ngay sau build) | Sau lần 2 | Sau lần 3 |
|---|---|---|---|---|
| Ch3 · FPS trung bình / 1% low / khung chậm nhất | 140 / 59 / 28 ms | 126 / 31 / 144 ms | 132 / 54 / 30 ms | 141 / 66 / 28 ms |
| Ch4 · FPS trung bình / 1% low / khung chậm nhất | 141 / 71 / 28 ms | 139 / 62 / 40 ms | 141 / 68 / 37 ms | không chạy |

- **FPS trung bình giữ khoảng 130–141,** vượt xa mục tiêu 60.
- **Lần chạy đầu sau mỗi build có khựng do nạp shader,** như đã ghi từ 6.5. Shader có thêm biến thể `_ARM_MAP` và `_GRAIN_FOLLOWS_OBJECT`, nên lần đầu khựng lâu hơn (144 ms).
- **1% low của Ch3 dao động 54–66 giữa các lần chạy,** vì lượt chạy chỉ khoảng 10 giây và tính cả lúc nạp cảnh.
- **Ch4 hầu như không đổi.**
- **Profiler trong Editor** (`Tools/LauTuBa/QA/Perf.txt`, so với `Perf_before_light.txt`): các góc phòng 5,5–10,5 ms như trước. Góc spawn ở sảnh Ch3 đo 16,6 ms, trước là 11,8 ms. Số đo trong Editor dao động mạnh: có lần "tắt bóng" còn chậm hơn "bật bóng".

Unity báo atlas bóng 2048 phải giảm độ phân giải bóng của đèn điểm khi thấy nhiều đèn có bóng cùng lúc. Số đèn có bóng không đổi so với trước, nên đây là hiện trạng cũ.

### 8.7 File đã đổi

- `Assets/_Game/Shaders/WorldTriplanarLit.shader`: `_ARM_MAP` và `_GRAIN_FOLLOWS_OBJECT`, mặc định tắt.
- `Core/Editor/LauTuBaKit.cs`:
  - material gỗ, đá, vữa mới; material ngày/đêm tách riêng;
  - `Arm()`;
  - vân vải cho `LTB_Silk*` và `LTB_Cotton*`.
- `Core/Editor/LauTuBaTextures.cs` (mới): vân lụa, vân bông, lụa đèn lồng.
- `Core/Editor/LauTuBaShaderCompare.cs` (mới): ảnh đối chiếu URP Lit.
- `Core/Editor/LauTuBaMeshKit.cs`: UV3 trục dài cho `RoundedBox`.
- `Core/Editor/LauTuBaHouse.Decor.cs`: đèn lồng (lụa, emission, không tự đổ bóng); cột dùng `PillarWood`.
- `Core/Editor/LauTuBaHouse.Rooms.cs`: cường độ đèn, bố cục sảnh, tên dải ướt ngày/đêm.
- `Core/Editor/LauTuBaSetup.cs`: trăng, ambient, grade Ch4, probe sân, điểm tuần sảnh.
- `Art/Environment/Textures/*/*_arm_1k.jpg` (7 file), `CREDITS.md`.
- `Art/Environment/LauTuBa/Textures/*.png` (sinh tự động).
- Scene sinh lại: `Chapter03_LauTuBa.unity`, `Chapter04_LauTuBaDem.unity`.

### 8.8 Phần còn thiếu

- **So với ảnh vibe vẫn còn khoảng cách:**
  - ảnh có tầng lầu, ban công và nhiều người;
  - sàn bóng nước phản chiếu đèn mạnh;
  - lớp sương sâu trong cảnh.

  Map một tầng nên không có lớp ban công. Chưa thêm người nền hay hoạt động nhỏ (mục 7 của hướng hình ảnh). Chưa có SSR hay phản chiếu đèn thật trên sàn, URP không có sẵn.
- **Hiên sân chưa có bàn nhỏ ở các khoang.** Chưa có model bàn tay cầm đèn (mục 8, để sau khi môi trường đạt chuẩn).
- **Ngói, đá chân tảng, bùn hẻm, đá rêu chưa có map roughness/AO** (bộ CC0 đang dùng chỉ có diffuse và normal).
- **Vân dệt và lụa đèn là texture sinh bằng code,** không phải PBR quét thật.
- **Đồ nội thất có sẵn của project** (giường, tủ, bộ bàn, bàn thờ: bản giảm lưới từ model 500k) giữ material gốc, không thuộc phạm vi chuẩn hoá gỗ lần này.
- **Phòng Tú Bà mới đổi vật liệu,** sàn và trần đã thấy vân gỗ. Ánh sáng chưa làm lại: hướng hình ảnh muốn có điểm nhấn đèn quanh khu ngồi và ngủ, nhưng ban đêm phòng này là vùng lén lút lấy chìa khoá. Cần thử cân bằng trước khi thêm đèn. Ảnh ghép `CMP_C3dusk_06_TuBaRoom` cho thấy thay đổi còn nhỏ.
- **Phòng ở và bếp ban đêm vẫn rất tối trên ảnh chụp** (cố ý cho lén lút). Cần thử trên màn hình thật.

## 9. Đợt nâng cấp asset và hình học (18/10/2026)

Mục tiêu là làm lại các món chính bằng hình khối, cấu tạo và UV thật, không chỉ thêm khối nhỏ, bo cạnh hay tăng độ bóng. Mặt bằng, nhiệm vụ và gameplay giữ nguyên.

### 9.1 Chọn asset có sẵn hay dựng lại

Model môi trường có sẵn trong project gồm:
- Altar, Bed, Cabinet, CeramicJar, OilLamp, TableSet, Well: đã dùng bản giảm lưới từ đợt 6.
- DoorLeaf, Fence, KieuHouse_Exterior: của Chương 1.
- Ba loại cây.

Không có ghế bành, sập, bàn trà, lan can hay cửa bức bàn hợp phong cách. Vì vậy các món này được **dựng lại bằng code** với cấu tạo mộc thật. Các model có sẵn vẫn giữ (bàn thờ, bộ bàn ở sảnh, giường, tủ, chum, đèn dầu).

### 9.2 Bộ dựng mộc (`LauTuBaJoinery`)

Mỗi món là **một mesh**, gồm:
- **Thanh quét theo đường cong:** tiết diện chữ nhật vát cạnh, khung xoay theo đường (parallel transport), vuốt thon theo chiều dài.
- **Ván cắt theo đường viền lõm:** cắt tam giác kiểu ear clipping, cho yếm vòm và yếm mây.
- **Chi tiết tiện tròn.**

UV:
- Tính bằng mét, U chạy dọc thớ gỗ của từng thanh.
- Hướng thớ ghi vào UV3, nên vân gỗ đi đúng theo từng thanh với cả URP Lit (UV0) lẫn WorldTriplanarLit (UV3).

Material và submesh:
- Submesh: 0 khung gỗ, 1 ván và lõi, 2 kim loại hoặc chỉ vàng.
- Đồ nội thất vẽ bằng **bản URP Lit của gỗ nhà** (`LauTuBaKit.UvTwin`): cùng texture, tint, kích thước vân.
  - Map ARM Poly Haven được đóng gói lại sang dạng metallic-smoothness/occlusion của URP (`LauTuBaTextures.MetallicSmoothness`). Dữ liệu giữ nguyên, chỉ đổi kênh.
- Kiến trúc (lan can, cửa, cột) vẫn dùng triplanar, vân theo UV3.

### 9.3 Đã dựng lại

| Món | Trước | Sau | Tam giác |
|---|---|---|---|
| Ghế bành (`Armchair`, mọi ghế bành trong nhà) | Khoảng 20 hộp bo cạnh | Xem dưới bảng | khoảng 5.000 |
| Bàn (`Table`: bàn trà, kỷ, bàn sổ, bàn bếp…) | Mặt hộp, chân hộp hoặc chân tiện, thanh giằng thẳng | Xem dưới bảng | 2.300–3.400 |
| Sập (`Sap`, 3 chiếc) | Hộp mặt, hộp yếm, chân hộp | Xem dưới bảng | khoảng 2.500 |
| Lan can hiên (`LauTuBaKit.Railing`) | Thanh hộp, con song vuông cách 28 cm | Xem dưới bảng | 6.000–7.900 mỗi đoạn |
| Cửa ván (`LauTuBaKit.Door`) | Tấm phẳng phủ texture ván, hai nẹp | Xem dưới bảng | 368 |
| Đầu cột (`Column`) | Khối vuông bo cạnh, xà xuyên qua | Xem dưới bảng | 900–2.100 |

Chi tiết từng món sau khi dựng lại:
- **Ghế bành** kiểu ghế quan mạo:
  - chân sau vươn lên thành trụ tựa, ngả và cong về sau;
  - đầu ghế cong, hai đầu vểnh;
  - tựa lưng uốn chữ S (sơn mài);
  - tay vịn cong, mũi quặp; trụ tay cổ ngỗng;
  - mặt ngồi khung và ván;
  - yếm trước có vòm cong; thanh giằng so le.
- **Bàn:**
  - mặt bàn có chỉ viền và eo thắt;
  - yếm vòm, yếm mây khi có chạm;
  - chân vuông vuốt thon với chân móng ngựa, hoặc chân tiện;
  - thanh giằng gù trên bàn cao.
- **Sập:**
  - khung dày ôm mặt ván ba tấm;
  - eo thắt, yếm sâu cắt hình mây;
  - chỉ vàng trên sập của Tú Bà; chân vuông cuộn vào trong.
- **Lan can hiên:**
  - tay vịn tiết diện mui luyện, thanh dưới;
  - con tiện cách 15 cm (chân, bầu, cổ, khoanh).
  - Collider giữ nguyên kích thước.
- **Cửa ván:** cửa bức bàn.
  - Hai đố dọc 17 cm (lộ ra khỏi khuôn cửa), đố đầu, đố lưng, đố chân bằng gỗ nâu đỏ.
  - Pano ván dọc có lõi nổi hai mặt.
  - Tấm va chạm cũ chỉ còn làm collider và khoét NavMesh.
  - Cửa sau bếp giữ kiểu ván có nẹp, vì là cửa mộc.
- **Đầu cột:**
  - đấu tiện loe dưới xà; tấm kê vuông;
  - ở cột hiên có hai tay đỡ cong vươn dọc xà, nâng xà hiên. Tay đỡ dịch ngang 20 cm cho khớp với xà.

### 9.4 Chụp trước và sau ở hai kiểu ánh sáng

Thêm 8 điểm nhìn cận cảnh vào `LauTuBaSetup.Looks`:
- `LOOK_18_Chair`, `19_Table`, `20_Sap`, `21_Railing`;
- `22_Door`, `23_Pillar`, `24_Desk`, `25_HallGroup`.

Mỗi điểm được chụp theo hai cách, cùng vị trí, FOV 72 và 1920×1080:
- **Ánh sáng trong game:** chụp lúc Play bằng `ViewTour` (`VIEW_C3day_*`, `VIEW_C3dusk_*`, `VIEW_C4night_*`).
- **Ánh sáng trung tính:** công cụ mới `LauTuBaNeutralViews` (editor, batch, không lưu scene).
  - Tắt mọi đèn, probe và sương mù của scene.
  - Ambient xám phẳng, một đèn chính và một đèn phụ màu trắng không đổ bóng.
  - Ảnh `NEUTRAL_C3_*`, `NEUTRAL_C4_*`.

Thư mục:
- Trước: `Tools/LauTuBa/Views/before_assets`
- Sau: `Tools/LauTuBa/Views/after_assets`
- Ảnh ghép hai bên: `Tools/LauTuBa/Views/compare_assets`, 33 cặp. Mỗi góc chính có ba ảnh: trung tính, Ch3 hoàng hôn, Ch4 đêm.

### 9.5 Kiểm tra

- **`LauTuBaChecks`: PASS cả Ch3 và Ch4.** Collider của ghế, bàn, sập, lan can và cửa giữ nguyên kích thước. Đường đi, cửa, chỗ nấp, tuyến tuần không đổi.
- **Thử lén lút: 5/5 PASS.** Số đo gần như trùng đợt 8: dưới đèn 0,76, bị bắt sau 20,7 s; trong tối 0,17, độ nghi 0. Tay đỡ và đầu cột không có collider.
- **Autopilot:** Ch3, Ch4 thường và thorough PASS; chuỗi Ch3 → Ch8 PASS.
- **Profiler trong Editor:**
  - Tam giác: 1,89 triệu → 1,86 triệu.
  - Renderer: 4.031 → 3.515, vì mỗi ghế, bàn, sập và đoạn lan can giờ là một mesh.
  - Góc nặng nhất: 11,6 ms. Đợt 8 là 16,6 ms.

- **Bản build Windows chạy autopilot** (1920×1080 cửa sổ, cùng máy và cấu hình như 8.6):

  | Chương | Lần 1 (ngay sau build) | Lần 2 |
  |---|---|---|
  | Ch3 · FPS trung bình / 1% low / khung chậm nhất | 141 / 69 / 21 ms | 141 / 65 / 28 ms |
  | Ch4 · FPS trung bình / 1% low / khung chậm nhất | 138 / 70 / 21 ms | 142 / 70 / 21 ms |

  Ngang hoặc tốt hơn đợt 8 (Ch3 132–141 / 54–66, Ch4 141 / 68–71). Lần này lần chạy đầu không bị khựng do nạp shader.

### 9.6 File

- **Mới:**
  - `Core/Editor/LauTuBaJoinery.cs`: bộ dựng mộc.
  - `Core/Editor/LauTuBaHouse.Furniture.cs`: ghế bành, bàn, sập.
  - `Core/Editor/LauTuBaNeutralViews.cs`: chụp ánh sáng trung tính.
- **Sửa:**
  - `LauTuBaKit.cs`: lan can, cửa bức bàn, `UvTwin`.
  - `LauTuBaHouse.Decor.cs`: cột với đấu và tay đỡ. Đã bỏ `Armchair`, `Table`, `Sap` bản hộp cũ.
  - `LauTuBaMeshKit.cs`: `Has` và `Keep` để lưu mesh mộc.
  - `LauTuBaTextures.cs`: đóng gói lại ARM cho URP.
  - `LauTuBaHouse.cs`: hướng xà cho cột, cửa sau giữ kiểu ván.
  - `LauTuBaSetup.cs`: 8 điểm nhìn cận cảnh.
- **Asset sinh ra:**
  - mesh `Joinery_*` trong `Art/Environment/LauTuBa/Meshes`;
  - material `*_UV` trong `Materials/LauTuBa`;
  - texture `*_arm_1k_urp.png`.
  - Các mesh `Joinery_*_v1` của cửa và đầu cột là bản thử trước, không còn dùng, có thể xoá.

### 9.7 Còn thiếu

- **Chưa có chạm khắc nổi** (hoa lá, chữ thọ) trên yếm, đầu ghế hay ván cửa. Hiện chỉ có hình cắt và chỉ viền. Chạm thật cần model tay hoặc normal map chạm riêng.
- **Bình phong, tủ áo, rương, ghế đá, kệ sách, ghế đẩu** vẫn là bộ khối cũ hoặc bản giảm lưới cũ.
- **Mái, tường, xà ngang hiên** vẫn là khối phủ texture.
- **Ở góc chụp sập**, đèn đứng của Tú Bà che phần lớn. Thay đổi rõ hơn khi nhìn từ phía khác.
- **Đồ gỗ mới chỉ có hình học** với cùng texture gỗ của nhà. Chưa có lớp vết mòn hay bám bẩn riêng theo từng món.

## 10. Đợt kiến trúc và bố trí cảnh (18/10/2026)

Làm theo `docs/concept/CLAUDE_ARCHITECTURE_AND_SCENE_PASS.md`, đối chiếu trực tiếp `lau-tu-ba-primary-vibe.png`. Áp dụng cho cả hai chương, trên scene chơi thật. Mặt bằng, sân trung tâm, nhiệm vụ, chìa khoá, khế ước, nhánh Lam, checkpoint và tuyến thoát giữ nguyên.

### 10.1 Kiến trúc

**Tầng trên quanh sân** (`LauTuBaHouse.Upper.cs`). Đây là kiến trúc nền: không có cầu thang, không có sàn để đứng, không có tuyến tuần.
- Hành lang tầng trên chạy vòng quanh sân, trên hiên Tây, hiên Đông, hiên Nam và hành lang Bắc.
  - Sàn hành lang có ván và dầm bên dưới; mặt dưới sàn là trần của hiên tầng trệt, thay cho mái hiên dốc cũ.
  - Lan can con tiện cao 0,78 m, để hở ở các góc nơi hai hành lang nối nhau.
  - Cột trên có đấu và tay đỡ; xà hiên dưới mái.
  - 8 đèn lồng nhỏ không đổ bóng.
- **Tường phòng tầng trên:** vữa chia khoang bằng cột gỗ, mỗi khoang một cửa sổ giấy, khoảng một phần ba sáng đèn từ trong. Có khoang tường trơn nơi vách ngăn chạm tường.
- **Tường ngoài** được nâng lên tới mái.
- **Cột hiên tầng trệt:** cao thêm 35 cm (2,55 → 2,9 m) để đỡ dầm dưới sàn hành lang. Cột trước sảnh có tay đỡ dọc xà.
- Dầm có tiết diện 0,22 × 0,24 m, đầu dầm thò ra và cắt cong mặt dưới.

**Mái ngói thật**, thay khối mái dùng chung (`StorySceneKit.Roof` không đổi, vì các chương khác còn dùng):
- Tấm mái có độ dày, mặt ngói gợn theo hàng ngói âm dương mỗi 17,5 cm (khớp với cột ngói trong texture).
- Một hàng đầu ngói ống tròn dọc mép mái.
- Ván diềm, ván gió ở đầu hồi, rui cách 60 cm và ván lót bên dưới.
- Bờ nóc dày có gờ, hai đầu cong vểnh lên; các góc mép mái cong lên (đầu đao).
- Collider dốc mái vô hình giữ nguyên vai trò cũ (che nhìn, che sáng).

Chiều cao và cách các mái gặp nhau:
- Mọi hành lang có xà hiên trên ở cùng độ cao 5,95 m.
- Mái nhà chính phía Bắc dốc 35° nên nóc cao nhất (khoảng 9,9 m). Hai cánh dốc 30° (nóc khoảng 9,5 m); sảnh 30° (khoảng 9,8 m).
- Mái hai cánh chạy tiếp vào trong mái Bắc và mái Nam đến khi nóc và đầu hồi khuất hẳn, nên góc nhà khép kín, không lộ trời hay đầu khối.
- Đầu hồi mái Bắc và mái Nam (nhìn từ hẻm) có tường hồi kín.

**Mưa:**
- Giọt mưa từ mép mái dời lên mép mái tầng trên (khoảng 5,65 m).
- Mưa vẫn chỉ rơi ở vùng hở (sân, hẻm). Dải ướt sát hiên giữ nguyên.

### 10.2 Bố trí cảnh

- **Hiên:**
  - 4 bàn trà nhỏ có ghế đẩu sát lan can, dưới hoặc gần đèn lồng. Lối đi giữ hơn 2 m; tuyến tuần đêm đi ở x = ±10,5.
  - Rèm lụa đỏ buộc vào cột ở các khoang đầu hiên. Phía Tây bỏ khoang trước cửa phòng Kiều, để tầm nhìn từ cửa ra sân không bị che.
  - Rèm không có collider và được buộc gọn, nên không tạo chỗ nấp giả.
- **Sảnh:** giữ cụm tiếp khách, bàn trà, rèm khoang của đợt 8.
- **Bình phong** (sảnh, phòng Tú Bà, lưu trữ, phòng Lam, phòng khách): mỗi cánh dựng lại thành khung mộc với núm tiện, chân đế và ván chân cắt hình mây.
- **Phòng Tú Bà:** ở Ch3 (chiều, hoàng hôn) có hai nguồn sáng ấm riêng là nến trên kỷ thờ và đèn cạnh giường. Ở Ch4 phòng chỉ còn một đèn như cũ, giữ nguyên tình huống trộm chìa khoá.
- **Cây giữa sân:** vỏ cây dựng lại thành các nhánh quét liền mạch.
  - Mỗi nhánh mọc ra từ trong nhánh mẹ, thuôn dần, gốc loe.
  - Không còn khớp gãy kiểu ống 7 cạnh.
- **Người trong cảnh** (`BackgroundPatron`):
  - Không có mắt, không di chuyển, tự nói chuyện theo nhịp ngẫu nhiên riêng nên không động tác đồng loạt.
  - Phối màu khác nhân vật nhiệm vụ.
  - Ch3: khách ngồi bàn trà sảnh, khách ngồi cụm tiếp khách, một cô rót trà, một cô đứng trên hành lang tầng trên phía sảnh.
  - Ch4: chỉ một cô còn thức, đứng cạnh đèn trên hành lang Bắc, xa trên cao. Hai khách trong phòng khách giữ như cũ.
  - Người nền đều bị bỏ NavMeshAgent khi dựng, nên không bị kéo xuống NavMesh.

### 10.3 Ánh sáng và nhận biết của AI

`LightExposure`: phần sáng của trời ở vùng trời trống giờ chỉ tính khi không có mái che giữa điểm đó và mặt trăng hoặc mặt trời. Mái tầng hai đổ bóng xuống sân, nên nơi trông tối vì bóng mái là nơi người canh cũng coi là tối. `OpenSky` chỉ dùng ở Lầu Tú Bà, các chương khác không bị ảnh hưởng.

### 10.4 Ảnh trước và sau

Thêm 4 điểm nhìn:
- `LOOK_26_YardToHall`: từ sân nhìn vào sảnh;
- `LOOK_27_AcrossYard`: từ sảnh nhìn qua sân thấy dãy đối diện và tầng trên;
- `LOOK_28_SapClear`: sập không bị đèn đứng che;
- `LOOK_29_EaveCorner`: mái hiên, dầm và đầu cột.

Tổng cộng 29 góc. Mỗi góc chụp lúc Play từ camera người chơi (FOV 72, 1920×1080, `C3day`, `C3dusk`, `C4night`) và ở ánh sáng trung tính (`NEUTRAL_*`).

Thư mục:
- Trước: `Tools/LauTuBa/Views/before_architecture` (sau đợt nội thất; bộ `after_assets` giữ nguyên làm mốc).
- Sau: `Tools/LauTuBa/Views/after_architecture`
- Ảnh ghép: `Tools/LauTuBa/Views/compare_architecture`.

Các góc theo yêu cầu nghiệm thu:
1. Sảnh qua sân thấy hiên đối diện và tầng trên: `27_AcrossYard`, `15_HallToYard`.
2. Dọc hiên: `14_WestVeranda`.
3. Từ sân vào sảnh: `26_YardToHall`.
4. Phòng Tú Bà: `06_TuBaRoom`, `17_TuBaBed`.
5. Khu ở và phục vụ: `02_KieuRoom`, `09_Kitchen`, `13_LamRoom`.
6. Cận mái, xà, đầu cột và hốc cửa (trung tính): `NEUTRAL_*_29_EaveCorner`, `_23_Pillar`, `_22_Door`.
7. Sập: `28_SapClear`.

### 10.5 Đánh giá so với ảnh vibe

- **Hình khối kiến trúc:** đạt rõ nhất.
  - Nhà đọc là lầu hai tầng có hành lang vòng quanh sân, mái ngói có độ dày, mép ngói và nóc cong.
  - Cột, dầm, tay đỡ nối thành kết cấu.
  - Tầng trên không lơ lửng: nó đứng trên dầm và cột hiên.
- **Mật độ có chức năng:** sảnh và hiên có cụm bàn, người ngồi uống trà, người phục vụ, đèn và rèm theo khoang.
  - Ảnh vibe đông người hơn nhiều, và có người đứng ở lan can tầng trên khắp nơi.
  - Ở đây chỉ có 4 người nền ở Ch3 và 1 người ở Ch4.
- **Vật liệu:**
  - Gỗ, ngói, vải, đá phân biệt được bằng phản ứng ánh sáng.
  - Còn thiếu sàn gỗ bóng nước phản chiếu đèn mạnh như ảnh, vì URP không có SSR.
  - Vết bẩn và mòn có nguyên nhân (tay vịn bóng, ẩm chân cột) mới chỉ có ở mức map AO/roughness và lớp bẩn chân tường sẵn có, chưa làm riêng.
- **Lớp gần–giữa–xa:**
  - Gần: rèm, bàn, lan can. Giữa: sân, cây, mưa. Xa: hai tầng hành lang sáng đèn.
  - Haze tách lớp xa còn nhẹ hơn ảnh vibe.

### 10.6 Kiểm tra

- **`LauTuBaChecks`:** PASS cả Ch3 và Ch4 (đường đi, cửa, cửa đóng, tuyến tuần, đường Lam, 8 chỗ nấp).
- **Thử lén lút: 5/5 PASS.**
  - Dưới đèn 0,75, bị bắt sau 19,3 s; trong tối 0,17, độ nghi 0.
  - Tường ở giữa, nấp trước, bị thấy lúc nấp: PASS.
- **Autopilot:** Ch3, Ch4 thường và thorough PASS. Gồm ép thua, bị bắt, khôi phục checkpoint, nhánh chị Lam, J1/J2, cửa sau. Chuỗi Ch3 → Ch8 PASS.
- **Đi bằng camera người chơi:** autopilot đi hết tuyến của cả hai chương, cùng 29 góc chụp ở 3 trạng thái giờ.
  - Không thấy kẹt, xuyên mái hay mưa trong nhà ở các góc này.
  - **Chưa có người chơi thật đi tay** để soi hết các góc lạ.

- **Hiệu năng.** Cùng máy, 1920×1080 cửa sổ, cùng cấu hình như các đợt trước.

  | | Trước (sau đợt 9) | Sau |
  |---|---|---|
  | Tam giác cả ngôi nhà | 1,86 triệu | 2,19 triệu (tầng trên, mái ngói, lan can tầng trên) |
  | Renderer | 3.515 | 3.698 |
  | Đèn điểm Ch3 / Ch4 | 26 / 36 | 28 / 44 (8 đèn hành lang trên và 2 đèn phòng Tú Bà ở Ch3, đều không bóng) |
  | Đèn có bóng | 6 | 6 |
  | Editor, góc nặng nhất | 11,6 ms | 13,1 ms |

  **Bản build Windows (exe), 4 lần chạy mỗi chương:**

  | Chương | FPS trung bình | 1% low |
  |---|---|---|
  | Ch3 | 109 (lần đầu sau build), rồi 115–118 | 40 (lần đầu), rồi 59–62 |
  | Ch4 | 133–140 | 65–71 |

  - Ch4 có một lần đo 80 FPS trung bình trong khi 1% low vẫn 65. Đây là lần đo lệch (máy bị tải nền); hai lần đo lại cho 138–140.
  - So với trước (Ch3 141 / 65–69, Ch4 138–142 / 70), **Ch3 tốn hơn rõ**: trung bình giảm khoảng 23 FPS.
    - Nguyên nhân khả dĩ nhất là bóng nắng của mái và hành lang hai tầng.
    - Mức này vẫn khoảng gấp đôi mục tiêu 60 FPS.
    - Nếu cần lấy lại, nên tắt đổ bóng của lan can và cột tầng trên trước.
  - Ch4 không đổi.

### 10.7 File đã đổi

- **Mới:**
  - `Core/Editor/LauTuBaHouse.Upper.cs`: tầng trên, mái ngói, dầm.
  - `Gameplay/Chapters/LauTuBa/BackgroundPatron.cs`.
- **Sửa:**
  - `LauTuBaHouse.cs`: gọi tầng trên, chiều cao cột, bỏ mái hiên dốc và mái chái, mái mới.
  - `LauTuBaHouse.Rooms.cs`: bàn và rèm hiên, đèn phòng Tú Bà ở Ch3.
  - `LauTuBaHouse.Decor.cs`: bình phong mộc.
  - `LauTuBaMeshKit.cs`: cây.
  - `LauTuBaJoinery.cs`: thêm `Grid`.
  - `LauTuBaSetup.cs`: 4 điểm nhìn, người nền, giọt mưa mái.
  - `Gameplay/Stealth/LightExposure.cs`: bóng mái che trời.
- **Scene sinh lại:** Ch3, Ch4. Mesh mới `Joinery_*` (mái, dầm, đầu hồi, bình phong, cây v4).
- Không xoá asset cũ nào, kể cả `Joinery_*_v1`.

### 10.8 Còn thiếu so với ảnh mẫu

- **Không có sàn bóng nước phản chiếu đèn** như ảnh (cần SSR hoặc probe phẳng; chưa thêm gói render).
- **Người nền ít và đứng yên tại chỗ.** Chưa có khách đi lại, chưa có người ở các lan can tầng trên phía Đông và Tây.
- **Tầng trên chỉ là mặt tiền:** không có nội thất sau cửa sổ giấy, không lên được.
- **Haze tách lớp xa chưa chỉnh;** sương đêm giữ như đợt 8.
- **Mặt ngoài tường hẻm còn phẳng,** chỉ có vữa và băng gỗ.
- **Chưa có chạm khắc hoa văn** trên đầu dầm, yếm và tay đỡ.
- **Vết mòn có nguyên nhân** (tay vịn bóng, bụi khe, ẩm chân cột ngoài trời) chưa có lớp riêng, chỉ có roughness/AO từ texture.
- **Ở Ch3 ban ngày, sân dưới mái hai tầng tối hơn trước;** ở Ch4 đêm, phần sân trong bóng mái giờ được AI coi là tối. Cần người chơi thử để xác nhận độ khó băng sân không bị lệch.

## 11. Đợt hoàn thiện bề mặt và ánh sáng (19/10/2026)

Thực hiện theo `docs/concept/CLAUDE_SURFACE_AND_LIGHTING_FINISH.md`, theo thứ tự ưu tiên: vân gỗ → nước và mưa → ánh sáng và chiều sâu → vết sử dụng. Đối chiếu với `lau-tu-ba-primary-vibe.png`. Hình khối hai tầng, mặt bằng, nhiệm vụ và gameplay giữ nguyên.

### 11.1 Nguyên nhân đã xác minh trước khi sửa

| Hiện tượng trong ảnh baseline | Cách kiểm tra | Nguyên nhân |
|---|---|---|
| Vân xoáy chéo trên thân cột (`C3dusk_27_AcrossYard`) | Cùng cột, cùng texture: ảnh trung tính chụp trong Editor (không Play) cho vân dọc đúng; ảnh lúc Play cho vân xoáy ngang. Texture `dark_wood` được đo thớ chạy theo U (độ biến thiên theo U 103, theo V 463), đúng giả định của shader | **Static batching** lúc vào Play gộp mesh vào toạ độ thế giới, nên mất ma trận object. Shader dùng ma trận này để xoay vân theo cột; UV3 hướng thớ cũng không được chuyển sang toạ độ thế giới. Đây không phải lỗi texture hay tiling |
| Vũng nước là mảng xám phẳng, cạnh sắc | Đọc `Puddle()` | Đĩa mesh đục, màu gần đen, độ bóng 0,64, nằm cách đá 2 mm. Mặt đĩa phản chiếu đều bầu trời probe, mép cắt cứng. Không có vùng chuyển tiếp |
| Dải ướt dưới mái là hình chữ nhật đồng màu | Đọc `DripLine` | Khối hộp phẳng một material |
| Mưa là rèm vạch trắng | Đọc `StoryArtKit.Rain` | Vạch dài khoảng 0,8 m (velocityScale 0,045 × 18 m/s), alpha 0,38 đồng đều, rơi thẳng đứng, không mờ gần camera |
| Cửa sổ sáng phẳng | Đọc `PaperLit` | Một material phát sáng một màu cho mọi cửa |
| Sảnh đêm: gỗ chìm tối, vùng sáng dồn lên trần | Vị trí đèn | Đèn lồng sảnh treo ở 2,4 m, sát trần 2,98 m |
| Khựng đầu phiên | Bấm giờ trong báo cáo | Giả thuyết NavMesh dựng lúc chạy **bị bác bỏ**: chỉ mất 30–40 ms. Con số 3,2–3,4 s trước đây là khung đầu tiên, gồm cả thời gian khởi động và nạp cảnh, chứ không phải khựng gameplay (xem 11.5) |
| Ch3 tụt FPS | Profiler Editor có so sánh bật/tắt bóng | Bóng mặt trời tốn 2–5 ms mỗi góc nhìn, kể cả trong nhà (bếp 7,6 → 4,9 ms khi tắt). Shadow map vẽ mọi vật đổ bóng của ngôi nhà, gồm hàng nghìn con tiện lan can tầng trên |

### 11.2 Đã sửa

**Vân gỗ:**
- Mọi renderer dùng gỗ có vân theo cấu kiện (`_GRAIN_FOLLOWS_OBJECT`) được loại khỏi static batching (`LauTuBaHouse.KeepWoodUnbatched`). Vật khác vẫn batch.
- Chi tiết tiện (`Lathe`) ghi trục tiện vào UV3, nên không còn phải đoán hướng vân từ scale.
- Kết quả lúc chạy: thân cột có vân dọc thân, xà có vân theo chiều dài.
- Tay đỡ cong vẫn có hoa văn gỗ đậm, vì đó là vân của chính texture `dark_wood`.

**Nước:**
- Vũng nước dựng lại thành hai lớp alpha (URP Lit trong suốt, phản chiếu mờ dần theo alpha):
  - viền ẩm: đá tối hơn, bóng vừa, loang rộng hơn mặt nước;
  - mặt nước: sâu ở giữa, mỏng dần ra mép lượn không đều, có chỗ lộ mặt đá.
- Ch4 (đang mưa) có gợn sóng (normal map sinh bằng code). Ch3 (sau mưa) mặt nước lặng.
- Material ngày và đêm tách riêng.
- Lần thử đầu dùng chế độ "preserve specular" làm cả tấm quad hiện thành ô vuông sáng. Đã đổi sang alpha blend thường và kiểm tra lại bằng ảnh.
- Dải ướt dưới giọt mái thành dải chuyển tiếp: ướt nhất ở đường giọt, khô dần về giữa sân và về phía kè, mép khô lượn.
- Mép ván hiên phía sân có lớp ẩm mỏng, ít bóng hơn đá ngoài trời, vì nằm dưới mái.
- Bên trong nhà không ướt.

**Mưa** (chỉ Ch4; Ch3 giữ trạng thái sau mưa, chỉ có giọt từ mép mái):
- Vạch mảnh hơn (0,009–0,02 m), ngắn hơn (velocityScale 0,024), alpha thay đổi từng giọt (0,12–0,32).
- Rơi hơi xiên theo gió nhẹ.
- Mờ dần trong khoảng 0,8–4 m trước camera (camera fading của URP Particles), nên mưa gần không che tầm nhìn.
- Material riêng `LTB_Rain`; material mưa dùng chung của Ch1 không đổi.

**Ánh sáng và chiều sâu:**
- Cửa sổ giấy sáng có 4 biến thể: thớ giấy, đèn đặt ở các vị trí khác nhau phía sau, sáng nhất ở chỗ đèn, tối dần về khung, độ sáng và sắc ấm khác nhau. Các cửa cạnh nhau hiếm khi giống nhau.
- Đèn lồng sảnh hạ xuống 2,0–2,05 m; đèn bàn trà xuống 1,95 m. Ánh sáng giờ rơi lên bàn, ghế và người, trần chỉ còn ánh hắt lên.
- **Haze:**
  - Ch4: mật độ sương 0,016 → 0,021, màu bớt xanh (0,085/0,09/0,11).
  - Ch3: 0,012 → 0,016.
  - Mục đích là tách hiên gần với tầng đối diện.
- Exposure toàn cảnh và ambient không đổi.
- **Bóng:**
  - Lan can, cột và khung cửa sổ của tầng trên không đổ bóng mặt trời. Sàn hành lang, tường, dầm và mái vẫn đổ bóng.
  - Không thêm đèn đổ bóng nào (vẫn 6 đèn).

**Vết sử dụng có nguyên nhân** (shader `WorldTriplanarLit` thêm hai tính năng tuỳ chọn `_DAMP_BASE` và `_DUST_TOP`, mặc định tắt):
- **Chân cột hiên phía sân và kè đá quanh sân:** ẩm sau mưa, tối hơn và bóng hơn dưới một đường khô lượn. Cột trong nhà và cột sảnh không ẩm.
- **Tay vịn lan can:** dùng material riêng, nhẵn và bóng hơn do tay người chạm. Phần còn lại của lan can giữ nguyên.
- **Kệ kho vải và kệ lưu trữ:** có bụi trên mặt hướng lên.
- **Không bẩn hoá:** phòng Tú Bà, đồ nội thất và các phòng ở.

### 11.3 Ảnh

Chụp lúc Play bằng camera người chơi, FOV 72, 1920×1080, cùng vị trí và trạng thái (`C3day`, `C3dusk`, `C4night`), kèm ảnh ánh sáng trung tính.
- Trước: `Tools/LauTuBa/Views/before_finish` (bằng `after_architecture`, giữ nguyên làm mốc).
- Sau: `Tools/LauTuBa/Views/after_finish`.
- Ghép hai bên: `Tools/LauTuBa/Views/compare_finish`, 58 cặp.
- Thêm `LOOK_30_Puddle`, chụp cận vũng nước (chỉ có trong bộ sau).

Các góc nên xem:
- `CMP_VIEW_C3day_27_AcrossYard` (vân cột);
- `CMP_VIEW_C4night_14_WestVeranda` (mưa, cửa sổ);
- `CMP_VIEW_C4night_11_Hall` (sảnh đêm);
- `CMP_VIEW_C4night_01_Hero`;
- `CMP_VIEW_C3dusk_26_YardToHall`;
- `CMP_NEUTRAL_C3_21_Railing` (tay vịn, chân cột ẩm);
- `after_finish/VIEW_C3dusk_30_Puddle`.

Chưa quay clip chuyển động mưa và phản chiếu (không có công cụ quay trong phiên này).

### 11.4 Kiểm tra gameplay

- **Kiểm tra bản đồ:** PASS cả Ch3 và Ch4.
- **Thử lén lút: 5/5 PASS**, số đo không đổi so với đợt 10 (dưới đèn 0,75, bị bắt sau 19,3 s; trong tối 0,17).
- **Sân Ch4 (đo mới, mang tính thông tin):**
  - Độ sáng dọc tuyến băng sân từ bậc Tây sang bậc Đông, mỗi mét một điểm: thấp nhất 0,12, trung bình 0,27, cao nhất 0,49. 4/18 điểm dưới 0,20 nằm trong bóng mái; phần còn lại 0,30 dưới trăng; cao nhất cạnh đèn hiên Đông.
  - Đứng hoặc khom giữa sân trống (độ sáng 0,30) trong 70 s khi người canh đi tuần: **không bị phát hiện**, độ nghi đạt 0.
  - Nghĩa là: với tầm nhìn người canh hiện tại, giữa sân dưới trăng đang an toàn hơn hiên có đèn. Đợt này không đổi cân bằng; cần người chơi và nhóm quyết định.
- **Autopilot:** Ch3, Ch4 thường và thorough PASS (checkpoint, nhánh Lam, J1/J2, cửa sau). Chuỗi Ch3 → Ch8 PASS.
- **Chơi tay:** **chưa thực hiện**, vì phiên này không điều khiển được game bằng tay. Autopilot không thay cho chơi tay. Tuyến đề nghị thử:
  1. Phòng Kiều → hiên Tây (đứng, rồi khom) → băng sân qua vùng bóng mái và vùng trăng.
  2. → Phòng Tú Bà (lấy chìa khoá) → cửa B → tủ lưu trữ → phòng Lam (đưa chị đi).
  3. → Hành lang → bếp → cửa sau.
  4. Ở giữa tuyến, cố ý để bị bắt một lần để kiểm tra khôi phục checkpoint.
  5. Xoay camera dưới mưa để xem vạch mưa và phản chiếu vũng nước.

### 11.5 Hiệu năng

Cùng máy (i5-12500H, RTX 3050 Laptop), 1920×1080 cửa sổ, VSync, màn 144 Hz, cắm điện (kiểm tra mỗi lần chạy).

Báo cáo autopilot giờ tách riêng:
- thời gian nạp: khung đầu tiên, gồm khởi động và nạp cảnh;
- khựng trong 2 s đầu, không tính khung đầu;
- gameplay từ giây thứ 2: trung vị, p99 và tỉ lệ khung chậm hơn 60 FPS.

Baseline là bản build cùng scene trước khi sửa, đo bằng cùng bộ thống kê.

| Exe | Chạy | FPS trung bình | 1% low | Khung chậm nhất | p99 | Dưới 60 FPS | Khựng 2 s đầu |
|---|---|---|---|---|---|---|---|
| Ch3 trước | lần 1 sau build | 109 | 50 | 28 ms | 13,9 ms | 0,6% | (gộp vào khung nạp) |
| Ch3 trước | lần 2–4 | 101–108 | 43–44 | 28–35 ms | 13,9–20,8 ms | 0,9–1,1% | (gộp vào khung nạp) |
| Ch3 sau | lần 1 sau build | 107 | 42 | 37 ms | 17,3 ms | 1,3% | 890 ms |
| Ch3 sau | lần 2–4 | 108–114 | 43–52 | 28–35 ms | 13,9 ms | 0,7–0,9% | 683–693 ms |
| Ch4 trước | lần 1–4 | 131–132 | 64–68 | 28 ms | 13,9 ms | 0,1–0,2% | (gộp vào khung nạp) |
| Ch4 sau | lần 1–4 | 129–139 | 47–70 | 28 / 28 / **99** / **112** ms | 13,9–14,0 ms | 0,0–0,3% | 742–820 ms |

- **Nạp cảnh:** khoảng 3,5 s đến khung đầu tiên, cả trước và sau, cả lần đầu lẫn các lần chạy lại.
- **FPS bị VSync lượng tử hoá.** Khung trung vị 6,9 ms (144 Hz). Khung nào trễ một nhịp thành 13,9 ms, nên trung bình của Ch3 thấp hơn Ch4. Thời gian khung gameplay thực tế ổn định: Ch3 dưới 60 FPS 0,7–1,3%, Ch4 0–0,3%.
- **Khựng trong 2 s đầu:** 0,68–0,89 s mỗi lần chạy, cả hai chương, không chỉ lần đầu sau build. Baseline chưa đo tách riêng mục này, nên **chưa biết đây là mới hay đã có**. Nguyên nhân chưa xác định; nghi vấn là 6 reflection probe realtime render lúc khởi tạo, hoặc biến thể shader dùng lần đầu.
- **Mới trong đợt này:** 2 trong 4 lần chạy Ch4 có một khung 99–112 ms giữa gameplay. Baseline cả 4 lần chỉ tối đa 28 ms. Nguyên nhân chưa xác định; nghi vấn là lần đầu vật liệu trong suốt hoặc biến thể mới xuất hiện trên màn hình, hoặc lúc đèn hành lang bật ở J2.
- **Trong Editor:**
  - Chi phí bóng mặt trời ở góc chính Ch3 giảm từ 5,4 ms xuống 1,9 ms.
  - Phần lớn các góc nhanh hơn 0,2–0,7 ms.
  - Riêng góc spawn ở sảnh tăng từ 12,9 lên 15,9 ms, có thể do gỗ không còn được batch, khiến số lệnh vẽ tăng.
  - Số đo Editor dao động mạnh (góc này từng đo 11,8–16,5 ms).
- **Bất thường Ch4 80 FPS (đợt 10)** lặp lại khi chạy bản exe cũ ở đầu phiên này: 74 và 92 FPS.
  - Cả ba lần cùng một dạng: 1% low 64–65, khung chậm nhất chỉ 18–22 ms, mọi khung đều khoảng 13 ms. Tức là VSync chạy cả lượt ở nửa tần số (72 Hz), không phải do tải nặng.
  - Máy đang cắm điện cả ba lần.
  - Nguyên nhân chưa rõ (tiêu điểm cửa sổ, chế độ hiển thị hoặc nguồn điện); ghi nhận là bất thường chưa giải thích.
- **Mục tiêu khoảng 60 FPS ổn định:** đạt trong gameplay cả hai chương (p99 ≤ 17,3 ms). Ba điểm còn dưới mục tiêu:
  - khung nạp cảnh khoảng 3,4 s;
  - khựng 0,7–0,9 s trong 2 s đầu;
  - khung 99–112 ms thỉnh thoảng ở Ch4.

### 11.6 File đã đổi

- **`Assets/_Game/Shaders/WorldTriplanarLit.shader`:** thêm `_DAMP_BASE` và `_DUST_TOP`, mặc định tắt. Dùng ô `_ClearCoatMask` và `_ClearCoatSmoothness` có sẵn của URP Lit nên vẫn tương thích SRP Batcher. Các chương khác không đổi.
- **`Core/Editor/LauTuBaHouse.cs`:** `KeepWoodUnbatched`; kè đá phía sân dùng material ẩm; cột phía sân.
- **`Core/Editor/LauTuBaHouse.Decor.cs`:** vũng nước hai lớp, dải ướt, material trong suốt, cột có chân ẩm, gỗ kho có bụi.
- **`Core/Editor/LauTuBaHouse.Rooms.cs`:** dải ướt thay `DripLine`, ẩm mép hiên, đèn sảnh hạ thấp, kệ có bụi.
- **`Core/Editor/LauTuBaHouse.Upper.cs`:** bỏ đổ bóng của lan can, cột và cửa sổ tầng trên.
- **`Core/Editor/LauTuBaKit.cs`:** material chân cột ẩm, kè ẩm, tay vịn bóng, gỗ có bụi; tay vịn tách submesh; cửa sổ giấy 4 biến thể.
- **`Core/Editor/LauTuBaTextures.cs`:** sinh mặt nạ vũng nước, gợn sóng, giấy cửa sổ, dải ẩm. Đều là texture sinh bằng code, không phải dữ liệu PBR quét thật.
- **`Core/Editor/LauTuBaMeshKit.cs`:** UV3 trục tiện cho `Lathe`.
- **`Core/Editor/LauTuBaSetup.cs`:** mưa (`LTB_Rain`), sương Ch3 và Ch4, `LOOK_30_Puddle`.
- **`Core/Story/StoryAutopilot.cs`:** thống kê khung hình tách riêng khung nạp, khựng 2 s đầu, p99 và tỉ lệ dưới 60 FPS.
- **`Environment/NarrowNavMesh.cs`:** ghi thời gian dựng NavMesh vào báo cáo autopilot.
- **`Gameplay/Chapters/LauTuBa/Chapter04LauWorld.cs`:** thêm hai phép đo sân vào stealth probe.
- **Scene sinh lại:** Ch3, Ch4.
- Không xoá asset nào.

### 11.7 Còn thiếu so với ảnh mẫu

- **Phản chiếu đèn trên sàn và vũng nước mới chỉ từ reflection probe** (mỗi vùng một probe; không theo đúng vị trí từng đèn). Ở vũng nước gần đèn đã thấy ánh cam, nhưng chưa có mặt sàn gỗ bóng nước như ảnh. Không thêm SSR hay gói render.
- **Tay đỡ cong vẫn có hoa văn gỗ tương phản cao** của texture `dark_wood`. Muốn dịu hơn cần texture gỗ khác ít hoa văn.
- **Mưa:** mới đánh giá qua ảnh tĩnh và lượt autopilot, chưa xem lúc xoay camera bằng tay.
- **Vết sử dụng:** chưa có mép bàn bóng, bụi trong khe đồ nội thất, vết chân trên sàn. Chỉ có tay vịn, chân cột, kè đá và kệ kho/lưu trữ.
- **Hiệu năng:** khựng 0,7–0,9 s đầu phiên và khung 99–112 ms thỉnh thoảng ở Ch4 chưa được xử lý. Cần profiler thật để xác định.
- **Cân bằng băng sân Ch4:** giữa sân dưới trăng hiện không bị phát hiện (xem 11.4).

## 12. Cảnh bị bắt (jumpscare) (19/10/2026)

Thực hiện theo `docs/concept/CAUGHT_JUMPSCARE_REFERENCES.md`. Làm trên scene chơi thật Ch3 (`Chapter03_LauTuBa`) và Ch4 (`Chapter04_LauTuBaDem`), dùng luồng phát hiện–truy đuổi–bắt–checkpoint sẵn có, không tạo hệ thống thất bại thứ hai.

### 12.1 Luồng

1. **Nghi ngờ / phát hiện** (`Watchman`, không đổi): độ nghi tăng, NPC quay về phía Kiều, quát, rồi truy đuổi. Người chơi vẫn điều khiển được, có thể chạy, cắt tầm nhìn, nấp.
2. **Bắt hợp lệ** (không đổi): `Watchman.Caught` chỉ bắn khi
   - đang đuổi, nhìn thấy Kiều, cách ≤ 1,3 m, đường từ ngực tới người không bị tường/cửa đóng chắn, đường NavMesh ≤ 2 m; hoặc
   - NPC **đã thấy** Kiều chui vào chỗ nấp, đi tới, lay chỗ nấp 1,5 s rồi bắt.
   Bị phát hiện từ xa **không** chạy cảnh bắt.
3. **Cảnh bắt** (`CatchScene.TryPlay`, mới): chỉ chạy khi kẻ bắt thật sự trong tầm tay (≤ 2,0 m; trong chỗ nấp ≤ 2,2 m). Nếu một tình huống ép bắt từ xa, game rơi về thất bại thường (ghi `CATCH not staged` vào báo cáo QA), không giả NPC dịch chuyển tới.
4. **Thất bại và checkpoint** (không đổi): `StoryDirector.Fail` → thẻ thất bại → nạp lại trạng thái, đặt lại người chơi, NPC về tuyến.

Chặn bắt lặp: `CatchScene.Playing` (một cảnh tại một thời điểm) và chương bỏ qua mọi lần bắt khi đang không ở chế độ khám phá (đang chạy thẻ thất bại). Hai NPC tới cùng lúc → một cảnh, một lần thất bại (đã kiểm tra).

### 12.2 Biến thể và ý nghĩa thất bại theo chương

Biến thể chọn theo vị trí thật của NPC lúc bắt, không ngẫu nhiên:

| Biến thể | Khi nào | Diễn biến |
|---|---|---|
| **A. Phía trước** | NPC ở trước mặt (lệch hướng nhìn ≤ 100°) | Bước vào khoảng 0,6 m, hai tay túm cổ áo (mỗi tay một bên, không bắt chéo), ngón tay nắm lại, cúi người, mặt nhìn thẳng vào mắt Kiều; camera giật theo cú nắm, bị kéo về phía mặt hắn (không gần hơn 0,42 m) |
| **B. Phía sau** | NPC ở sau lưng (> 100°) | Tay đặt lên vai từ mép khung hình, cú kéo xoay góc nhìn khoảng 2/3 vòng về phía hắn trong 1/3 giây, không quay 180° tức thì |
| **C. Kéo khỏi chỗ nấp** | NPC đã thấy Kiều chui vào và tìm tới | Tủ: hai cánh cửa bật mở quanh bản lề (tay trái hắn kéo cửa). Bình phong: bị xô sang bên và xoay. Rèm/chỗ thấp: hắn cúi vào. Rồi tay vươn vào túm, kéo ra |

| Chương | Ý nghĩa thất bại | Kết thúc cảnh |
|---|---|---|
| Ch3 (ban ngày, lẻn vào khu cấm) | Bị người canh lôi về hiên, không bị đánh | `Drag`: góc nhìn bị kéo đi theo hướng hắn xoay, hơi nghiêng, rồi tối. Lời: "Cô em đi đâu đấy? Chỗ này không phải chỗ của cô." Thẻ thất bại giữ nguyên nội dung cũ |
| Ch4 (ban đêm, trốn đi) | Bị túm, ghì xuống | `Shove`: góc nhìn chúi xuống, lệch thăng bằng, rồi tối. Lời theo người bắt: người canh "Định trốn đi đâu?", gác cổng "Đứng lại! Đi đâu đấy?", Tú Bà "Con ranh! Trốn đâu cho thoát?" |

Ch3: khi **Tú Bà** tìm ra chỗ Kiều nấp, vẫn dùng cảnh thất bại riêng có sẵn (`FailSeenByTuBa`), vì đó là tình huống cốt truyện có thoại riêng; không chèn cú túm.

### 12.3 Dựng chuyển động, ánh sáng, âm thanh

- **Chuyển động:** project không có clip grab, và rig hiện có (LinhCanh, TuBa) là humanoid. `GrabPose` dựng tư thế túm theo từng khung **sau** Animator, trên chính rig đó:
  - cột sống và ngực cúi về phía Kiều (cúi sâu hơn khi Kiều cúi hoặc nấp thấp);
  - mỗi tay giải IK hai đốt, khuỷu tay chĩa xuống và ra ngoài;
  - lòng bàn tay xoay úp vào ngực Kiều (mu bàn tay về phía camera), ngón tay co lại khi chạm;
  - đầu xoay theo mắt Kiều (cổ 40%, đầu 60%).
  Chân bước bằng chu kỳ đi có sẵn (tham số `Speed` của Animator). Tắt component là trả lại animation gốc.
- **Nhịp** (bắt trực diện; trong chỗ nấp lùi thêm 0,46 s cho cửa/bình phong mở):

| Thời điểm | Diễn biến |
|---|---|
| 0 s | Khoá điều khiển, dừng mọi NPC, hạ nhạc nền và âm môi trường |
| 0,15 s | Hắn lao tới, tiếng bước chân |
| 0,40 s | Chạm: tiếng vải bị túm, tiếng va, nốt trầm, rung và chớp nhẹ màn hình |
| 0,52 s | Kiều hít mạnh; lời quát hiện thành phụ đề |
| 0,40–1,08 s | Kéo về phía hắn, FOV hẹp lại 6° |
| 1,08 s | Tiếng gió, tiếng ngã (Ch4) hoặc bước chân (Ch3); kết thúc theo chương |
| 1,40–1,70 s | Tối hẳn → thẻ thất bại |

- **Mặt đọc rõ:** đèn điểm ấm, không đổ bóng, đặt phía Kiều hơi cao, chỉ bật trong cảnh. Đèn lồng hắn cầm được hạ xuống hông trái để không che mặt.
- **Không xuyên tường:** bước tới của hắn và mọi dịch chuyển của camera đều kiểm tra va chạm bằng spherecast trên lớp tường; nếu sát tường thì dịch ít hơn.
- **Đứng/cúi:** điểm tay nắm và độ cúi tính theo độ cao mắt Kiều thực tế.

### 12.4 Khôi phục

Trước khi gọi `Fail`, cảnh trả lại mọi thứ đã chiếm: camera controller, FOV, `Time.timeScale`, NPC (StoryActor, NavMeshAgent được Warp lại đúng chỗ, Animator), tạm dừng của các watcher, đèn mặt, đèn lồng, cửa tủ/bình phong về đúng vị trí, mức nhạc và âm môi trường. Màn hình để đen cho thẻ thất bại. `Fail` lo phần còn lại như cũ: input, HUD, trạng thái Ink, vị trí người chơi, NPC về tuyến, chỗ nấp thoát ra.

Sửa thêm: thẻ thất bại trước đây nằm **dưới** lớp tối nên bị che khi màn đã tối; giờ được đưa lên trên trong lúc hiện rồi trả lại thứ tự.

### 12.5 Tài nguyên và nguồn

Không thêm tài nguyên bên ngoài. Toàn bộ dùng thứ đã có trong project:

| Thành phần | Dùng gì | Nguồn |
|---|---|---|
| Model và rig | LinhCanh (người canh, gác cổng), TuBa | Có sẵn trong project |
| Animation | Chu kỳ đi/đứng có sẵn + tư thế túm dựng bằng code (`GrabPose`) | Tự dựng |
| Âm thanh | `cloth`, `blow`, `sting_low`, `breath_in`, `whoosh`, `stumble`, `step_wood`, `door_fast`, `slide_fast` | Bộ sfx có sẵn của project (`StoryAudio`) |
| Lời thoại | Phụ đề (bark) | Không có giọng thu âm cho người canh/Tú Bà |

**Bị vướng:**
- **Mixamo** (clip grab/punch/drag miễn phí) cần đăng nhập tài khoản Adobe nên không tải được trong phiên này. Nếu nhóm muốn clip thật thay cho tư thế dựng bằng code: đăng nhập Mixamo, tải các clip kiểu "Grab"/"Pulling" dạng FBX for Unity, without skin, rồi gắn vào layer trên Animator của LinhCanh. Code hiện tại vẫn dùng được làm IK tinh chỉnh tay.
- **Giọng nói:** chưa có nguồn giọng tiếng Việt có quyền sử dụng rõ ràng; lời quát đang là phụ đề.

### 12.6 Kiểm tra

**Bắt thật qua toàn chuỗi** (`-autopilotCatches`: NPC nhận biết → thấy → đuổi → bắt → cảnh bắt → thất bại → checkpoint), lặp lại sau mỗi checkpoint:

| Ca | Chương | Tư thế / bối cảnh | Biến thể chạy | Exe: khung chậm nhất lần chạy 1 / 2 |
|---|---|---|---|---|
| `A_front_standing` | Ch4 | Hiên Đông cạnh đèn, đứng, hắn đi tới trước mặt | Front, Shove | 14 / 54 ms (lần bắt đầu tiên) |
| `A_front_crouched` | Ch4 | Như trên, cúi | Front, Shove | 35 / 16 ms |
| `B_rear` | Ch4 | Như trên, quay lưng về phía hắn | Rear, Shove | 14 / 15 ms |
| `B_rear_crouched` | Ch4 | Quay lưng, cúi | Rear, Shove | 14 / 18 ms |
| `A_front_wall` | Ch4 | Hành lang, tường sát vai | Front, Shove | 14 / 18 ms |
| `A_two_guards` | Ch4 | Người canh và gác cổng tới từ hai phía | Front, Shove, **1 cảnh, 1 lần thất bại** | 14 / 35 ms |
| `C_screen` | Ch4 | Hắn thấy Kiều nấp sau bình phong phòng Lam | FromHiding, bình phong bị xô | Editor 8 ms (chưa đo trên exe, xem dưới) |
| `C_wardrobe` | Ch4 | Hắn thấy Kiều chui vào tủ lưu trữ | FromHiding, cửa tủ bật mở | 15 / 14 ms |
| `A_ch3_drag` | Ch3 | Ban ngày trong văn phòng, hắn bắt gặp | Front, Drag | 28 / 19 ms (lần bắt đầu tiên) |

Mọi ca đều kết thúc bằng thẻ thất bại rồi `FAIL → checkpoint restored`, sau đó ca tiếp theo chạy bình thường (tức là camera, điều khiển và NPC đã được trả lại đầy đủ).

**Lỗi gameplay tìm ra nhờ kiểm tra khoảng cách tầm tay:** bài thử "thấy Kiều chui vào chỗ nấp" cũ đặt Kiều cách bình phong 8 m rồi cho nấp. Vì `HideSpot` lấy vị trí đứng lúc nấp làm chỗ người tìm đi tới, người canh "tìm ra" Kiều từ cách 8,7 m mà không lại gần. Trong game thật, khoảng tương tác buộc Kiều phải đứng sát chỗ nấp nên không gặp, nhưng bài thử đã PASS sai. Đã đặt lại hai bài thử (stealth 5 và `C_screen`) cho Kiều đứng ngay bình phong, người canh đứng trong phòng Lam (từ ngưỡng cửa thì 9,2 m, quá tầm nhìn 9 m). Bài stealth 5 giờ còn kiểm tra hắn phải tìm thấy Kiều **ngay tại** chỗ nấp: đo được 0,0 m.

Nếu vì lý do nào đó lần bắt hợp lệ xảy ra ngoài tầm tay (các bài thorough cố ý gọi bắt từ 20 m để thử checkpoint), cảnh bắt không chạy mà về thất bại thường: đã thấy đúng như vậy trong thorough Ch3/Ch4.

**Clip runtime** (640×360, 30 fps, quay từ camera người chơi trong scene thật):
- GIF: `Tools/LauTuBa/Clips/<biến thể>.gif`;
- ảnh ghép khung hình: `Tools/LauTuBa/Clips/<biến thể>_sheet.png`.

Có clip cho: `A_front_standing`, `A_front_crouched`, `A_front_wall`, `A_two_guards`, `B_rear`, `B_rear_crouched`, `C_wardrobe`, `A_ch3_drag`. Chưa có clip cho bình phong (`C_screen`), xem 12.8.

Đã xem từng clip và sửa theo những gì thấy: tay bắt chéo, lòng bàn tay ngửa như đang xin, đèn lồng che mặt, cảnh bị tua nhanh do autopilot chạy nhanh hơn thời gian thật, màn tối đến ngay.

**Khựng lần bắt đầu tiên** (bản exe, 1920×1080, không quay khung):

| Exe | Lần bắt đầu tiên | Các lần bắt sau | Gameplay cả lượt |
|---|---|---|---|
| Ch4 lần 1 (ngay sau build) | 14 ms | 14–35 ms | trung vị 7,0 ms · p99 14,2 ms · dưới 60 FPS 0,3% |
| Ch4 lần 2 | 54 ms | 15–35 ms | trung vị 7,0 ms · p99 14,9 ms · dưới 60 FPS 0,6% |
| Ch3 lần 1 (ngay sau build) | 28 ms | (một ca) | trung vị 6,9 ms · p99 20,8 ms |
| Ch3 lần 2 | 19 ms | (một ca) | trung vị 8,3 ms · p99 19,2 ms |

- **Kết luận:** lần bắt đầu tiên **không khựng** đáng kể. Khung chậm nhất trong cảnh bắt đầu tiên là 14–54 ms, cùng mức với các lần sau (14–35 ms) và với khung chậm nhất gameplay thường.
- Một lần đo 54 ms (khoảng 3 khung ở 60 Hz) không lặp lại ở lần chạy kia.
- Các tài nguyên của cảnh bắt được nạp trước khi vào chơi: đèn mặt, `GrabPose` trên mọi NPC canh, và các âm thanh cảnh bắt (phát câm một lần).
- Trong Editor có quay khung, lần bắt đầu tiên từng đo 1,4–18 s. Đó là Editor biên dịch shader cộng ghi PNG, không xuất hiện trên exe.
- `C_screen` chỉ đo trong Editor (8 ms), vì bản exe được build trước khi sửa bài thử này. Code gameplay của cảnh bắt không đổi.

**Hồi quy:**

- **Kiểm tra bản đồ:** PASS (Ch3, Ch4).
- **Thử lén lút: 5/5 ok**, gồm bài stealth 5 đã sửa. Hai phép đo sân không đổi so với đợt 11.
- **Bắt thật:** Ch4 8/8 cảnh, Ch3 1/1, đều khôi phục checkpoint.
- **Thorough:** Ch3 và Ch4 PASS (checkpoint, nhánh Lam, J1/J2, cửa sau).
- **Chuỗi Ch3 → Ch8:** PASS.
- **Build Windows:** Success.

### 12.7 File đã đổi

- **Mới** `Scripts/Gameplay/Stealth/CatchScene.cs`: cảnh bắt, chọn biến thể, nhịp, âm thanh, đèn, chỗ nấp, khôi phục, quay clip QA.
- **Mới** `Scripts/Gameplay/Stealth/GrabPose.cs`: tư thế túm trên rig humanoid.
- `Gameplay/Chapters/LauTuBa/Chapter03LauWorld.cs`, `Chapter04LauWorld.cs`: gọi cảnh bắt từ `OnCaught`, chặn bắt lặp, lời thoại theo người bắt, thử nghiệm `-autopilotCatches`.
- `Gameplay/Stealth/HideSpot.cs`: lộ phần nhìn thấy của chỗ nấp và loại chỗ nấp thấp.
- `UI/StoryHUD.cs`: thẻ thất bại nằm trên lớp tối; `FadeAlpha`.
- `Core/Story/StoryCapture.cs`: `ShotFile` (chụp khung bất kỳ).
- `Core/Story/StoryAutopilot.cs`: tắt âm khi chạy QA.
- `Core/Editor/LauTuBaSetup.cs`: gắn `CatchScene` vào Ch3 (`Drag`) và Ch4 (`Shove`).
- **Scene sinh lại:** Ch3, Ch4. Không xoá asset nào. Chưa commit.

### 12.8 Chưa xác minh

- **Chơi tay chưa thực hiện.** Mọi kiểm tra là autopilot và clip runtime quay tự động. Cảm giác lực, độ dễ đọc khi đang chạy thật và thời điểm âm thanh khớp hình cần nhóm chơi bản exe để đánh giá.
- **Âm thanh:** đã đặt đúng khung trong code, nhưng clip GIF không có tiếng và mọi lượt QA đều tắt tiếng. Độ đồng bộ âm thanh chưa được nghe kiểm.
- **Bình phong (`C_screen`):** đã chạy và khôi phục đúng, nhưng chưa có clip (dừng quay theo yêu cầu), nên hình ảnh bình phong bị xô và tay vươn qua khe **chưa được xem**.
- **Rèm và chỗ nấp thấp** (gầm đồ, sau rèm; hắn chỉ cúi vào): chưa có ca thử riêng.
- **Ch3 mới thử bắt phía trước.** Phía sau và trong chỗ nấp dùng chung code với Ch4 nhưng chưa chạy ở Ch3.
- **Tú Bà bắt (Ch4)** chưa có ca thử. Rig Tú Bà là humanoid nên `GrabPose` áp dụng được; nếu thiếu xương, cảnh tự về thất bại thường. Project có sẵn `TuBa-Yell` chưa dùng, có thể gắn cho cú bắt của Tú Bà ở đợt sau.
- **Mặt và tay ở cận cảnh** vẫn là model gốc; cảnh không điều khiển biểu cảm khuôn mặt, độ dữ dằn đến từ tư thế, ánh sáng và nhịp.
- **Lời thoại** là phụ đề, chưa có giọng.
- **Bản exe** trong `Builds/Windows` được build trước hai sửa đổi cuối, chỉ ở code thử nghiệm và câu ghi báo cáo. Gameplay giống hệt bản hiện tại.

## 13. Hoàn thiện Chapter 4: tiếng động, nghe, Tú Bà và cảnh bắt (19/10/2026)

Thực hiện theo `docs/concept/CLAUDE_CH4_COMPLETE_REVISION.md` (chỉ dẫn chính), có tham khảo `CLAUDE_CH4_SOUND_STEALTH.md` và `CLAUDE_CH4_FIRST_FINISH.md`.

**Phạm vi:**
- Chỉ Chapter 4; Chapter 3 chưa chuyển gameplay.
- Không quay clip mới; không commit.
- Hệ thống được mở rộng, không thay mới: `FirstPersonFootsteps`, `SurfaceTag`, `Noise`, `NoiseZone`, `HouseDoor`, `Watchman`, `TuBaSleeper`, `CatchScene`, `GrabPose`.
- Nhiệm vụ, chìa khoá, giao kèo, nhánh Lam, J1/J2, tuyến bếp–cửa sau và checkpoint giữ nguyên.
- Hình ảnh và mặt bằng không đổi. Ngoại lệ: tấm bình phong kho lưu trữ giờ được gắn làm vật bị xô khi bị tìm ra.

### 13.1 Bước chân Kiều

**Đã làm:**
- **Một bước = quãng đường thực sự đi trên mặt đất.** `FirstPersonFootsteps` đo vị trí trước–sau mỗi khung, không dựa vào tốc độ. Không sinh bước khi:
  - đứng yên hoặc ép tường không tiến được;
  - trên không (CharacterController không chạm đất);
  - bị dịch chuyển (checkpoint, cốt truyện; nhận biết bằng quãng nhảy quá 8 m/s);
  - bị khoá điều khiển, đang nấp, hoặc đang có cảnh bắt.
- **Nhịp theo dáng đi:** bước khom 0,55 m (khoảng 1,7 bước/s), đi 0,8 m (khoảng 1,8 bước/s), chạy 1,35 m (3,2–3,8 bước/s).
- **Độ ồn (bán kính tiếng động AI nghe được) = dáng đi × bề mặt:**

| Bề mặt | Hệ số | Khom | Đi | Chạy |
|---|---|---|---|---|
| Gỗ (hiên, phòng, hành lang) | 1,0 | 1,5 m | 4,0 m | 9,0 m |
| Gạch/đá (sảnh, bậc, kè) | 1,1 | 1,7 m | 4,4 m | 9,9 m |
| Đá ướt (sân) | 1,5 | 2,3 m | 6,0 m | 13,5 m |
| Bùn (hẻm) | 0,9 | 1,4 m | 3,6 m | 8,1 m |
| Đất nện (bếp) | 0,8 | 1,2 m | 3,2 m | 7,2 m |
| Chiếu/thảm | 0,6 | 0,9 m | 2,4 m | 5,4 m |

  Vùng tiếng che (`NoiseZone`, giữ nguyên) nhân thêm: mưa ở sân ×0,8 (chạy trên sân còn 10,8 m), nhạc phòng khách ×0,6.
- **Âm nghe được khớp độ ồn.** Âm lượng tăng theo bán kính; cao độ: khom 1,04, đi 1,0, chạy 0,94. Có biến thiên nhẹ (cao độ ±3%, âm lượng ±5%), đủ để không lặp máy móc mà vẫn đoán được rủi ro. Tiếng phát ngay dưới camera.
- **Âm thanh từng bề mặt:**
  - Gỗ và đá: bản thu sẵn của nhóm (`Recorded/sfx/step_wood`, `step_stone`), mỗi loại một bản; biến thể chỉ đến từ cao độ và âm lượng.
  - Đá ướt, đất, chiếu: âm tổng hợp có sẵn, thêm bản thứ hai (`_2`).
  - Bùn: âm tổng hợp mới, gót chạm rồi tiếng rút chân (`step_mud`, `step_mud_2`).
  - Không có bản thu ngoài nào mới.
- **Phân loại sàn:**
  - Hẻm dùng material `LTB_AlleyMud` (texture `brown_mud_02`) nhưng gắn tag `WetStone`; đã đổi sang bề mặt mới `Mud`.
  - Thảm và chiếu là đồ trang trí không có collider nên trước đây giẫm lên vẫn kêu như gỗ. Giờ là "lớp phủ" (`SurfaceTag` dạng patch, nhận theo diện tích):
    - chiếu cạnh bàn phòng Kiều;
    - hai thảm tiếp khách và thảm chạy ở sảnh;
    - thảm sân khấu;
    - thảm lớn phòng Tú Bà quanh sập.
  - Phòng khách đã gắn `Mat` từ trước.
- **Kiểm tra phủ sàn** (`LauTuBaChecks`): dò mọi điểm đi được trên NavMesh Ch4, cách nhau 0,5 m, theo đúng cách bước chân dò. Kết quả: gỗ 2387 · gạch/đá 1473 · đá ướt 856 · bùn 539 · thảm/chiếu phủ 305 · đất 257 · phòng khách (chiếu) 254 điểm. **0 điểm sàn hoặc bậc thiếu tag.** 33 điểm nằm trên mặt đồ gỗ hoặc ngưỡng cửa chưa gắn tag (sập, sân khấu, bàn, ngưỡng); những chỗ này nghe như gỗ, đúng vật liệu. Bồn cây đá giữa sân trước đây 36 điểm nghe như gỗ, nay đã gắn `Stone`.

### 13.2 Cửa

- **Nhấn E:** cửa bật mở hoặc đóng sập, tiếng có lực.
  - Tiếng động AI nghe phát **một lần, tại cửa, đúng lúc cánh cửa đập vào chặn hoặc khung** (cuối cú đẩy 0,3 s), không phải lúc bắt đầu.
  - Mở mạnh 8 m, đóng sập 6 m. Cửa chỉ hé mà đóng sập thì nhẹ hơn (tới 45%).
- **Giữ E:** mở hoặc khép khẽ, chậm. Tiếng kẽo kẹt nhỏ 1,5 m, cách nhau 0,7 s, không phát mỗi khung.
- **Then/khoá giữ nguyên:**
  - tra chìa 1,5 m mỗi nhịp, mở khoá 5 m;
  - giật then nhanh 7 m;
  - then rơi 6 m;
  - rung cửa khoá 3 m.
  Mỗi loại một tiếng riêng (key_turn, bolt_slow/fast, latch_lift/drop, lock_rattle, door_*/slide_*).
- **Cửa do NPC hoặc cốt truyện đóng mở** (`SetOpen`) không bao giờ phát tiếng động cho AI. Người chơi vẫn nghe tiếng, nhưng NPC không tự nghe tiếng mình rồi đi điều tra. Các tiếng truyện có chủ đích (J1 rung cửa, J2 cửa sập) giữ nguyên.
- **Nhãn tương tác:** "Mở cửa (nhấn E: đẩy mạnh, vang xa · giữ E: mở khẽ)" và "Đóng cửa (nhấn E: đóng sập, vang xa · giữ E: khép khẽ)".

### 13.3 Lính canh nghe

- **Luật nghe** (giữ `Noise.Hears`):
  - nghe được khi ở trong bán kính;
  - có tường hoặc cửa đóng chắn thì chỉ còn 60% bán kính;
  - mưa và nhạc đã giảm bán kính từ lúc phát.
- **Mới, chỉ bật cho lính canh, gác cổng và Tú Bà ở Ch4** (`_carefulListener`; Ch3 không đổi):
  - Độ thính theo trạng thái: tuần tra ×1,0; nghi ngờ ×1,15; đang kiểm tra hoặc lục soát ×1,25; đang làm việc cốt truyện ×0,85.
  - Tiếng nhỏ phải lặp **3 lần trong 4 s** mới khiến lính đi kiểm tra. Một hoặc hai tiếng chỉ làm lính dừng lại, quay đầu, nói một câu ngắn.
  - Tiếng lớn (≥ 6 m) khiến lính đi ngay tới **vị trí phát tiếng cuối cùng**.
- **Vị trí:** lính chỉ biết chỗ tiếng phát ra. Chỉ một tiếng mới nghe được, gần hơn, mới đổi điểm kiểm tra.
- **Tiếng động không bao giờ tự gây truy đuổi.** Chỉ khi nhìn thấy Kiều thì lính mới đuổi.
- **Khôi phục checkpoint:** xoá bộ đếm tiếng nhỏ, độ nghi và điểm điều tra (`Watchman.Forget`).

### 13.4 Tú Bà

**Đã sửa lỗi thua ngay:** trước đây, khi mụ đang cựa mình, bất kỳ tiếng nào mụ nghe được (kể cả mở cửa ở xa) đều gọi `WakeUp` → `OnTuBaWoke` → thất bại ngay. Luồng đó đã bỏ.

Trạng thái mới:

| Trạng thái | Vào khi | Hành vi | Ra khi |
|---|---|---|---|
| Ngủ | mặc định, sau checkpoint | ngáy theo nhịp hít/thở. Tiếng nghe được cộng dồn "bất an": bước khom cạnh mụ ≈ 0,2, bước đi ≈ 0,45 | bất an ≥ 1 → cựa mình. Tiếng ≥ 6 m trong phạm vi nghe (5,5 m) → thức ngay |
| Cựa mình | bất an đầy; lính rung cửa văn phòng (J1) | ngừng ngáy, ú ớ. Đứng im thì 3,5 s sau ngủ lại | nghe thêm một tiếng → thức, nằm nghe |
| **Thức, nằm nghe (mới)** | như trên | không ngáy. Gọi lính tới **chỗ tiếng phát ra** ("Thằng Tư! Tiếng gì ngoài kia? Ra xem!"). Mỗi tiếng mới nghe được kéo dài thời gian thức và gọi lính lại (giãn cách 6 s). Mụ không biết Kiều ở đâu | 14 s yên lặng → lẩm bẩm, ngủ lại, ngáy tiếp |
| Thức hẳn | J2 (mất chìa) | như cũ | — |

- Kiều chỉ bị Tú Bà bắt khi **thực sự ở bên giường**:
  - đứng trong phòng mụ, cách đầu mụ dưới 2,4 m, không nấp, khi mụ đang thức nằm nghe; hoặc
  - đặt tay vào thắt lưng (giữ E lấy chìa) khi mụ đang cựa mình hoặc đang thức.
  Khi đó mụ ngồi dậy bên phía Kiều và **cảnh bắt chạy với chính Tú Bà**, rồi tới thẻ thất bại cũ.
- Tiếng từ xa không bao giờ gây thua.
- **Không kẹt nhiệm vụ:** mụ luôn ngủ lại sau 14 s yên lặng, và việc lấy chìa trong nhịp thở ra vẫn hoạt động.
- **Không nghe khi đang đứng** (đầu chương, mụ đứng ở cửa phòng Kiều). Trước đây một tiếng từ phòng Kiều lúc này có thể dẫn tới thua.
- **Checkpoint:** khôi phục về ngủ (hoặc thức hẳn sau J2), bất an 0, thời gian thức 0. Không phát lại tiếng cũ.

### 13.5 Cảnh bắt

Dùng `CatchScene` và `GrabPose` có sẵn; luồng phát hiện → truy đuổi → bắt hợp lệ → cảnh bắt → thẻ thất bại → checkpoint giữ nguyên.

- **Ánh sáng mặt:** ảnh ghép cũ cho thấy mặt và nón bị chiếu sáng phẳng. Đèn mặt đổi thành đèn chính lệch bên:
  - cường độ 1,6 → 0,95, tầm 2,4 → 1,8 m;
  - đặt lệch 0,42 m sang một bên, thấp hơn mặt 6 cm (dưới vành nón), nên vành nón và má xa giữ bóng;
  - exposure toàn cảnh không đổi.
- **Lực túm:** tay tới đúng mép cổ áo, mỗi tay một bên, không bắt chéo, và theo thứ tự:
  1. ngón tay siết từ 0,08 s trước lúc chạm tới 0,04 s sau, siết quá mức nắm thường 12%;
  2. ngay lúc chạm, camera giật 5 cm về phía hắn (tắt dần trong khoảng 0,1 s);
  3. cú kéo có gia tốc đầu: đường cong ease-out bậc 3, khoảng 70% quãng kéo trong 1/3 thời gian đầu;
  4. thân hắn thẳng lại khi kéo (độ cúi giảm 45%), lùi 0,16 m. Vai và lưng làm lực kéo, không chỉ hai tay vươn tới camera.
- **Chỗ nấp, theo loại** (`HideKind` mới, gán trong setup):

| Chỗ nấp | Loại | Động tác |
|---|---|---|
| Tủ áo phòng Kiều, tủ kho lưu trữ | Wardrobe | tay trái kéo hai cánh tủ bật 55° quanh bản lề, rồi túm |
| Bình phong phòng Lam, bình phong kho lưu trữ (mới được gắn làm vật xô) | Screen | xô bình phong sang bên 0,35 m, xoay 30° |
| Rèm giường phòng Kiều | Curtain | tay trái quét ngang chỗ rèm, tiếng vải. Chỗ nấp này không được gắn vật thể rèm riêng nên không có rèm chuyển động |
| Dãy chum hiên, chum nước bếp, đống vải kho | Low | cúi sâu hơn 12°, với qua chỗ nấp, kéo Kiều lên và ra; tiếng của chính chỗ nấp |

  Ghi chú: phòng Kiều là vùng lính bỏ qua, nên không ai thấy Kiều nấp ở tủ hay rèm phòng mình. Hai biến thể đó có code nhưng không xảy ra trong lượt chơi Ch4 bình thường.
- **Tú Bà:** Tú Bà bắt Kiều bên giường bằng cùng cảnh, lời "Con ranh! Mày mò vào đây làm gì?!".
  - Đã xem clip `TuBa-Yell` có sẵn (đọc dữ liệu clip; không xem bằng hình): 7,83 s, lặp, chủ yếu là cẳng tay, ngón xoè, quay đầu, một tay giơ lên hạ xuống. Đó là vòng cử chỉ quát, **không phải động tác bắt**, nên không dùng cho cảnh bắt.
- **Âm thanh:**
  - lúc chạm: tiếng vải, cú va, nốt trầm;
  - hít mạnh ở +0,1 s;
  - lời quát hiện thành phụ đề;
  - gió và tiếng ngã ở cuối cú kéo.
  Chưa có giọng thu âm tiếng Việt: lời quát vẫn chỉ là phụ đề.
- **Ngắt giữa chừng:** mọi thứ cảnh bắt chiếm được lưu lại và trả về bằng `Cleanup` (camera, FOV, time scale, Animator/NavMeshAgent/StoryActor, tạm dừng của watcher, đèn, đèn lồng, đạo cụ chỗ nấp, nhạc, cờ `Playing`). Gọi khi:
  - kết thúc bình thường;
  - object bị tắt hoặc huỷ (đổi scene);
  - `Abort()`.
  Khi bị ngắt, điều khiển và màn tối cũng được nhả, và không chạy thất bại. Biến static (`Playing`, `Count`, `ClipName`) được đặt lại mỗi lần vào Play, vì project tắt Reload Domain.
- **Giữ nguyên:** chống hai NPC bắt cùng lúc, chống bắt xuyên tường/cửa, và thất bại thường khi một sự kiện ép bắt từ quá xa.

### 13.6 Băng sân

**Cách đo** (bài `-autopilotSound`, phần 7):
- Kiều băng sân từ bậc Tây sang bậc Đông bằng điều khiển thật (CharacterController), theo ba tuyến trên mặt lát trống: z = 0, −4,2, 5,5. Tuyến thẳng bậc–bậc bị bồn cây chắn.
- Mỗi tuyến chạy với cả ba dáng khom, đi, chạy, xuất phát khi lính đang ở **từng điểm dừng** trên vòng tuần tra.
- Lính tuần tra thật: nhìn, nghe, độ nhạy của chương (0,85 khi chưa cảnh giác).
- Ghi lại: lính có thấy Kiều không, có nghe tiếng bước không, có đi kiểm tra không, có đuổi không.

**Trước khi sửa** (9 điểm dừng × 3 tuyến = 27 lần mỗi dáng):
- khom: thấy 2, nghe 0;
- đi: thấy 6, nghe 6;
- chạy: thấy 9, nghe 19.
- Khom băng sân không bao giờ bị phản ứng, từ bất kỳ điểm dừng nào. Bài cũ (đứng 70 s một chỗ) cũng ra kết quả "không bị phát hiện".
- Lần đo đầu chỉ tính "đi kiểm tra" theo độ nghi do nhìn, nên chưa có số "đi kiểm tra/đuổi" so được với lần sau.

**Đã chỉnh, chỉ Ch4:** thêm một điểm dừng trên vòng tuần tra thường và vòng cảnh giác. Lính dừng 4 s ở đầu bậc sân phía Đông (10,5; 1), nhìn ngang sân về phía Tây (có quét ±50° như mọi điểm dừng), giữa hai điểm dừng hiên Đông sẵn có. Không đổi tầm nhìn, độ sáng hay độ nhạy toàn game.

**Sau khi chỉnh** (10 điểm dừng × 3 tuyến = 30 lần mỗi dáng; hai lần chạy, thời gian dừng của lính có ngẫu nhiên):

| Dáng | Thấy | Nghe | Đi kiểm tra | Đuổi |
|---|---|---|---|---|
| Khom (khoảng 15 s qua sân) | 4–5/30 | 0/30 | 0–1/30 | 0/30 |
| Đi | 10/30 | 8–9/30 | 7/30 | 3–4/30 |
| Chạy (khoảng 3 s) | 12/30 | 22/30 | 22/30 | 1–2/30 |

- Chạy nguy hiểm hơn đi khẽ: lính nghe và tới chỗ phát tiếng 22/30 lần. Kiều thường đã qua sân trước khi lính tới, nhưng lính sẽ lục soát quanh đó.
- Đi bị đuổi 3–4/30 lần.
- Khom gần như an toàn: lính thấy 4–5/30 lần nhưng độ nghi hiếm khi đủ để đi kiểm tra (0–1/30), và chưa lần nào bị đuổi. Đây là đường an toàn nhưng chậm.
- Không còn tuyến **chạy** thẳng qua sân mà không có rủi ro.
- Bài stealth cũ (đứng giữa sân 70 s) vẫn không bị phát hiện, vì một vòng tuần tra giờ dài hơn 70 s nên lính có thể chưa tới điểm dừng mới trong lúc đo.
- **Cần nhóm quyết định khi chơi thử:** có muốn khom giữa sân dưới trăng cũng có rủi ro không. Nếu có, nên chỉnh riêng điểm dừng mới (dừng lâu hơn hoặc gần sân hơn), không chỉnh mô hình nhìn chung.

### 13.7 Kiểm tra

Tất cả chạy trên scene thật `Chapter04_LauTuBaDem` (Ch3 trên `Chapter03_LauTuBa`), Editor batch, sau lần sinh lại Ch4 cuối cùng. Báo cáo nằm trong `Tools/LauTuBa/QA/`.

| Bài | Kết quả | Nội dung chính |
|---|---|---|
| Kiểm tra bản đồ (`Checks.txt`) | PASS | gồm kiểm tra phủ sàn mới |
| **Âm thanh** `-autopilotSound` (`sound_ch4.txt`) | **PASS**, 0 lỗi | xem dưới |
| Bắt `-autopilotCatches` (`catches_ch4.txt`) | PASS | 13 ca bắt + 1 ca ngắt giữa chừng, đều khôi phục checkpoint |
| Lén lút `-autopilotStealth` | PASS 5/5 | bài 5 giờ kiểm tra cả việc lính tìm thấy ngay tại chỗ nấp (0,0 m) |
| Ch4 thường, Ch4 thorough | PASS | checkpoint, J1, J2, nhánh Lam (chị Lam tới bếp), cửa sau |
| Ch3 thường, Ch3 thorough | PASS | code dùng chung, Ch3 không đổi gameplay |
| Chuỗi Ch3 → Ch8 | PASS | |

**Bài âm thanh, đo thật bằng CharacterController:**
- **7 vùng sàn × khom/đi/chạy:** đúng bề mặt, có tag, bán kính khom < đi < chạy, thảm/chiếu êm hơn gỗ ở cả ba dáng (xem bảng 13.1).
- **Đổi bề mặt khi đang đi:** hiên → bậc → sân cho Wood → Stone → WetStone; gạch → thảm chạy → gạch cho Stone → Mat → Stone.
- **Không bước giả:** đứng yên 3 s: 0 bước; ép tường 3 s: 0; 6 lần dịch chuyển 21 m: 0; khi bị khoá điều khiển: 0.
- **Lính nghe** (lính quay lưng, Kiều phía sau rồi rời đi, nên lính không nhìn thấy trước khi nghe):

| Hành động | Khoảng cách | Lính nghe | Phản ứng |
|---|---|---|---|
| Khom | 3,5 m | 0 | không |
| Đi | 3,5 m | 6 tiếng | quay lại, thấy Kiều, đuổi |
| Chạy | 3,5 m | 3 tiếng | đi kiểm tra, cách chỗ tiếng 0,1 m |
| Đi | 6 m | 0 | không |
| Chạy | 6 m | 3 tiếng | đi kiểm tra |
| Mở cửa khẽ | 4,3 m, cùng phía | 0 | không |
| Mở cửa mạnh | 4,3 m, cùng phía | 1 tiếng | đi kiểm tra đúng cửa |
| Mở cửa mạnh | 6,2 m, có tường chắn | 0 | không (qua tường chỉ còn 4,8 m) |

- **Gây tiếng rồi rời đi:** lính tới đúng chỗ phát tiếng (0,0 m), không lần nào nhắm gần hơn 8,5 m tới chỗ Kiều đã tới, quay lại tuần tra sau 6 s.
- **Hai lính, một tiếng hạt cườm:** cả hai đi kiểm tra, tới cách chỗ tiếng 0,0 và 0,2 m, không chặn nhau, về vị trí sau 6 và 7 s.
- **Tú Bà:**
  - Đang ngủ, Kiều khom ở phòng kho cạnh bên: vẫn ngủ, bất an 0.
  - Mở mạnh cửa phòng Kiều (cách 14,8 m): vẫn ngủ.
  - Mở mạnh cửa phòng mụ từ ngoài: thức, nằm nghe; gọi lính tới đúng cửa; không thua; ngủ lại sau 14 s.
  - Đang cựa mình, Kiều đi trên thảm cách 3 m: không nghe (thảm). Chạy ở chỗ đó: thức, không thua, rồi ngủ lại.
  - Đang thức nằm nghe, Kiều đứng bên giường (1,1 m): cảnh bắt của Tú Bà, thất bại, checkpoint; sau đó mụ nằm, ngủ, tắt mắt nhìn.
  - Tay ở thắt lưng khi mụ đang cựa mình: cảnh bắt của Tú Bà.
  - Sau checkpoint: không còn tiếng cũ, bất an 0, lính tuần tra.
  - Lấy chìa trong nhịp thở ra: lấy được sau 5 s, chuyển sang giai đoạn 2, mụ vẫn ngủ. Sau đó autopilot chơi tiếp J1, J2, cửa sau: PASS.
- **Băng sân:** xem 13.6.

**Bài bắt** (mọi ca đều chạy đủ chuỗi nhận biết → thấy → đuổi → bắt → thất bại → checkpoint):
- **Phía trước:** đứng, cúi, sát tường hành lang, chân bậc sân, lưng quay về cửa phòng Kiều.
- **Phía sau:** đứng, cúi.
- **Hai lính cùng lúc:** một cảnh, một thất bại.
- **Bị tìm ra khi nấp:**
  - bình phong phòng Lam (Screen);
  - tủ kho lưu trữ (Wardrobe);
  - dãy chum (Low);
  - đống vải kho (Low);
  - bình phong kho lưu trữ (Screen, mới gắn vật xô).
- **Tú Bà:** hai ca trong bài âm thanh (bên giường, tay ở thắt lưng).
- **Ngắt giữa chừng** (`Abort` sau 0,5 s): camera bật lại, FOV 72/72, time scale 3,0/3,0, điều khiển được nhả, màn tối 0, pose tắt, cờ `Playing` tắt.

**Lỗi đã tìm và sửa trong lúc kiểm tra:**
- `HouseDoor`: tiếng động phát lúc bắt đầu cú đẩy, không phải lúc cánh cửa đập.
- Hẻm bùn gắn nhầm `WetStone`; thảm/chiếu giẫm lên vẫn kêu như gỗ.
- Tú Bà: chính cái sập (collider trên lớp tường) chắn tai mụ, nên mụ không nghe được tiếng ngay trong phòng. Ngược lại, tiếng cửa phòng mụ thì bị chính khung cửa chắn.
- Tú Bà có thể bắt lặp lại nhiều lần trong một lần thất bại; nay mỗi lần thất bại chỉ một lần.
- Kiểm tra tầm tay của cảnh bắt đo từ camera, nên sai ngay sau khi dịch chuyển; nay đo từ người Kiều.
- Bài thử dùng `Debug.LogError` làm Editor dừng Play, khiến bài treo; nay dùng `StoryAutopilot.Problem`.

### 13.8 Hiệu năng

Bản exe build sau mọi thay đổi trong đợt này (build Success). Bản exe **khớp source hiện tại**: không sửa source nào sau khi build. Cùng máy và điều kiện như 11.5: 1920×1080 cửa sổ, VSync, cắm điện (BatteryStatus 2) mọi lần.

| Exe Ch4 | Lần bắt đầu tiên | Các lần bắt sau | FPS trung bình | 1% low | Khung chậm nhất | p99 | Dưới 60 FPS | Khựng 2 s đầu |
|---|---|---|---|---|---|---|---|---|
| Bắt, lần 1 sau build | 21 ms | 14–21 ms | 115 | 68 | 28 ms | 13,9 ms | 0,1% | 840 ms |
| Bắt, lần 2 | 21 ms | 14–21 ms | 112 | 68 | 21 ms | 13,9 ms | 0,1% | 799 ms |
| Đêm thường, lần 1 | — | — | 130 | 64 | 35 ms | 13,9 ms | 0,2% | 806 ms |
| Đêm thường, lần 2 | — | — | 129 | 65 | 35 ms | 13,9 ms | 0,1% | 776 ms |

- **Lần bắt đầu tiên:** khung chậm nhất 21 ms ở cả hai lần, trong khi baseline 12.6 là 14 và 54 ms.
  - Đây là số đo hai lần chạy, chưa phải bằng chứng "không bao giờ khựng". Lần 54 ms trước đây không lặp lại.
  - Chưa dùng profiler, vì không còn khung vượt khoảng 35 ms để truy.
- **Hệ thống tiếng động không làm chậm gameplay.** Đêm thường đo 129–130 FPS, p99 13,9 ms, dưới 60 FPS 0,1–0,2%. Đợt 11 đo 129–139 FPS, p99 13,9–14,0 ms, dưới 60 FPS 0–0,3%.
- **Khung chậm nhất 35 ms** trong 4 lần chạy. Đợt 11 có 2/4 lần chạy với khung 99–112 ms; lần này không gặp, nhưng chưa xác định nguyên nhân, nên chưa thể coi là đã hết.
- **Khựng 2 s đầu** 0,78–0,84 s, như đợt 11 (0,74–0,82 s); chưa xử lý.
- **Trong Editor,** bước chân, nghe và Tú Bà không tạo khung chậm nào đáng kể trong các bài thử; các cảnh bắt 3–18 ms.

### 13.9 File đã đổi

Đường dẫn tính từ `Assets/_Game/Scripts/`.

**Gameplay (runtime):**
- `Gameplay/Stealth/FirstPersonFootsteps.cs`: bước theo quãng đường thực, chặn bước giả, nhịp và cao độ theo dáng, lớp phủ, sự kiện `Stepped`.
- `Environment/SurfaceTag.cs`: bề mặt `Mud` (thêm ở cuối enum, giữ chỉ số cũ), lớp phủ (patch), biến thể âm.
- `Presentation/ProceduralAudio.cs`: `step_wet_2`, `step_earth_2`, `step_mat_2`, `step_mud`, `step_mud_2`. Bản WAV đã được `AudioBankBaker` ghi khi build (5 file mới trong `Audio/Resources/StoryAudio/sfx`).
- `Gameplay/Stealth/Noise.cs`: `Hears(..., attention)`.
- `Gameplay/Stealth/Watchman.cs`: `_carefulListener`, `Attention`, đếm 3 tiếng nhỏ, `LastHeard`, `Target`, sự kiện `Heard`. `Forget` xoá bộ đếm.
- `Gameplay/House/HouseDoor.cs`: tiếng đập phát lúc cánh chạm (`_bang`), giảm theo quãng đẩy, `SetOpen` không phát tiếng động, nhãn mới, hằng số bán kính.
- `Gameplay/Chapters/LauTuBa/TuBaSleeper.cs`: trạng thái `Roused`; sự kiện `Roused`/`Heard`/`Settled`/`HandCaught`; không nghe khi đang đứng; tường chắn tính từ tai, bỏ qua sập và nguồn tiếng.
- `Gameplay/Chapters/LauTuBa/KeyTheft.cs`: tay ở thắt lưng khi mụ không ngủ → `CatchHand`; nhãn khi mụ thức.
- `Gameplay/Chapters/LauTuBa/Chapter04LauWorld.cs` (thành `partial`):
  - bỏ thua ngay khi Tú Bà tỉnh;
  - `OnTuBaRoused`/`OnTuBaHeard` (gọi lính tới chỗ tiếng), `KieuAtBedside`, `TuBaCatches` (cảnh bắt của Tú Bà, một lần mỗi lần thất bại);
  - thêm ca bắt vào `CatchProbe`; hook `-autopilotSound`.
- **Mới** `Gameplay/Chapters/LauTuBa/Chapter04LauWorld.SoundProbe.cs`: bài kiểm tra `-autopilotSound`.
- `Gameplay/Stealth/HideSpot.cs`: `HideKind` (`_kind`, mặc định Auto suy ra), `SoundId`.
- `Gameplay/Stealth/CatchScene.cs`:
  - đèn chính lệch bên;
  - kéo có gia tốc đầu, giật lúc chạm, ngón siết lúc chạm, thân thẳng lại khi kéo;
  - động tác theo loại chỗ nấp;
  - `Cleanup` và `Abort`; dọn khi bị tắt; đặt lại biến static.
- `Player/PlayerMovement.cs`: `ForceCrouch` có tác dụng cả khi đang di chuyển theo script (chỉ QA và cốt truyện dùng). Tốc độ khom giữ nguyên.

**QA và setup (Editor):**
- `Core/Story/StoryAutopilot.cs`: `Problem()`, ghi lỗi QA vào báo cáo mà không log Error. Editor có thể dừng Play khi có Error; trước đây điều này làm bài thử treo.
- `Core/Editor/StoryBuild.cs`: `-playtestSeconds` (mặc định vẫn 900 s).
- `Core/Editor/LauTuBaSetup.cs`: `-onlyChapter4` khi rebuild; bật `_carefulListener` cho ba người canh Ch4.
- `Core/Editor/LauTuBaHouse.cs`: hẻm → `Surface.Mud`.
- `Core/Editor/LauTuBaHouse.Rooms.cs`: lớp phủ cho 6 thảm/chiếu, bồn cây gắn `Stone`, `HideKind` cho 8 chỗ nấp, bình phong kho lưu trữ làm vật xô, rèm giường phát tiếng vải.
- `Core/Editor/LauTuBaChecks.cs`: kiểm tra phủ sàn Ch4; đọc dữ liệu clip `TuBa-Yell`.

**Scene và asset:**
- Chỉ sinh lại `Chapter04_LauTuBaDem` (`-onlyChapter4`). Scene Ch3 không đổi.
- Không xoá asset; không đổi package; không commit.

### 13.10 Chưa kiểm chứng

- **Chơi tay có bật tiếng: chưa thực hiện**, vì phiên này không điều khiển và nghe được game trực tiếp. Mọi lượt QA đều tắt tiếng. Vì vậy chưa xác minh bằng tai hay bằng cảm giác:
  - âm từng bề mặt nghe có tự nhiên, có phân biệt được không;
  - độ lớn tương đối khom/đi/chạy và cao độ;
  - tiếng cửa mạnh/khẽ;
  - tiếng Tú Bà ú ớ, gọi lính;
  - lực túm, độ đọc khuôn mặt, đồng bộ tiếng vải/va/hít với chuyển động.
  Số liệu chỉ chứng minh logic: bán kính, ai nghe, phản ứng.
- **Ánh sáng mặt mới chưa được xem bằng hình**, vì không quay clip hay chụp ảnh mới theo yêu cầu. Giá trị (0,95; lệch bên 0,42 m; dưới vành nón 6 cm) chọn theo phân tích ảnh ghép cũ.
- **Âm bùn và các bản `_2`** là âm tổng hợp mới. Gỗ và đá vẫn chỉ một bản thu mỗi loại, biến thể nhờ cao độ và âm lượng. Không có bản thu ngoài mới.
- **Giọng nói:** lời quát của lính và Tú Bà vẫn chỉ là phụ đề; chưa có bản thu tiếng Việt có quyền sử dụng.
- **Rèm giường và tủ phòng Kiều:** có động tác riêng nhưng không xảy ra trong Ch4, vì lính bỏ qua phòng Kiều. Rèm không có vật thể chuyển động.
- **Tú Bà đã thức sau J2 bắt Kiều khi truy đuổi:** chưa có ca riêng. Tú Bà không tự đuổi (`_canChase` tắt) mà gọi lính. Cảnh bắt của Tú Bà mới được thử ở hai ca bên giường.
- **Khom qua sân dưới trăng gần như không có rủi ro** (xem 13.6): cần nhóm chơi thử để quyết định.
- **Ch3:**
  - Gameplay chưa đổi; scene Ch3 không sinh lại.
  - Phần code chung vẫn có hiệu lực trong Ch3: bước chân theo quãng đường thực, tiếng cửa phát lúc đập, cảnh bắt và đèn mặt mới.
  - Các thay đổi chỉ cho Ch4 (cách nghe theo trạng thái, đếm 3 tiếng, Tú Bà) không ảnh hưởng Ch3.
  - Thay đổi phân loại sàn (bùn, thảm, bồn cây) sẽ có ở Ch3 khi sinh lại scene Ch3 ở đợt sau.
  - Cảnh bắt từ đợt 12 vẫn còn trong Ch3; đợt sau chuyển Ch3 sang khám phá tự do sẽ phải gỡ.
- **Chưa commit.**

## 14. Foley Chương 4: bước chân và cửa bằng bản thu (19/10/2026)

Thực hiện theo `docs/concept/CLAUDE_CH4_AUDIO_FOLEY_UPGRADE.md`, sau khi người dùng nghe game và thấy tiếng bước chân, tiếng cửa còn giả.

> **Chưa nghe kiểm trực tiếp phần nào.** Phiên này không phát và nghe được âm thanh. Mọi lựa chọn dưới đây dựa trên tên và mô tả của gói, cùng số đo tín hiệu (mức, điểm chạm, đuôi, số lần va, độ sáng). Số đo không chứng minh âm nghe tự nhiên. Các bài kiểm tra tự động vẫn tắt tiếng, chỉ chứng minh logic. Nhóm cần nghe các file A/B (14.5) và chơi bản exe có bật tiếng trước khi duyệt.

### 14.1 Nguyên nhân âm cũ

Đã đọc trong source, khớp với tài liệu yêu cầu:
- `step_wood` và `step_stone` thật ra đã là bản thu sẵn của nhóm (`Recorded/sfx`, từ Chương 1; một bản mỗi loại).
- `step_wet`, `step_earth`, `step_mat`, `step_mud` và mọi tiếng cửa, then, chốt của Ch4 (`door_*`, `slide_*`, `latch_*`, `bolt_*`) là âm tổng hợp (`ProceduralAudio`), được bake ra WAV.
- `AudioBank` tìm theo thứ tự `Recorded` → bake → tổng hợp. Sửa hàm tổng hợp không đổi được âm nếu đã có file bake.
- Tiếng đóng sập trước đây phát lúc cánh cửa **bắt đầu** đẩy, nên tiếng va đến trước hình 0,3 s.

### 14.2 Nguồn đã tải và quyền sử dụng

Người dùng cho phép tải 4 gói CC0, không cần đăng nhập, tổng 5,6 MB, lưu ở ổ D (`ThuyKieu_Share/FoleyDownloads/`, ngoài project). Chỉ các file được chọn mới chép vào `Assets/_Game/Audio/Ch4Foley/Source/`. Chi tiết, file dùng và chỉnh sửa: `Assets/_Game/Audio/Ch4Foley/CREDITS.md`.

| Gói | Tác giả | Giấy phép |
|---|---|---|
| [Impact Sounds](https://kenney.nl/assets/impact-sounds) | Kenney | CC0 |
| [RPG Audio](https://kenney.nl/assets/rpg-audio) | Kenney | CC0 |
| [25 CC0 mud sfx](https://opengameart.org/content/25-cc0-mud-sfx) | rubberduck | CC0 |
| [Door Open, Door Close Set](https://opengameart.org/content/door-open-door-close-set) | qubodup | CC0 |

**Bị vướng:**
- Hai nguồn trong tài liệu yêu cầu, [Rickmk2 — Footsteps on wooden flooring](https://freesound.org/people/Rickmk2/sounds/164315/) (8,1 MB WAV) và [Breviceps — Creaky Steel and Wood Doors](https://freesound.org/people/Breviceps/sounds/456816/) (3,4 MB WAV), đều CC0 nhưng Freesound bắt **đăng nhập mới tải được**. Chưa tải. Nếu nhóm đăng nhập và tải về, có thể thêm vào `Source/` rồi chạy lại công cụ.
- Không tìm được bản thu CC0 tiếng **trượt cửa gỗ/giấy** (shoji), cũng không có bản thu **bước trên đá ướt** riêng.

### 14.3 Bộ âm mới

Dựng bằng `Core/Editor/Ch4Foley.cs`:
- `AnalyseBatch` đo bản gốc → `Tools/LauTuBa/QA/FoleySources.txt`.
- `BuildBatch` dựng one-shot → `Assets/_Game/Audio/Ch4Foley/<id>/`, `Ch4FoleyBank.asset`, `Tools/LauTuBa/QA/FoleyBuild.txt`, và file A/B.

Biên tập mỗi take:
- trộn mono;
- cắt từ 2 ms trước lúc chạm tới hết đuôi, có giới hạn độ dài;
- fade vào 2 ms và fade ra mềm, để không có tiếng click;
- mọi take trong một nhóm có cùng mức RMS ở 120 ms đầu, giữ 1 dB headroom; không peak-normalize;
- không đổi pitch khi dựng.

| Id | Bản thu | Số take thật | Ghi chú |
|---|---|---|---|
| `step_wood` | Kenney `footstep_wood` | 5 | khom 2 take nhẹ nhất, chạy 2 take mạnh nhất (theo mức bản gốc), đi cả 5 |
| `step_stone` | Kenney `footstep_concrete` | 5 | như trên |
| `step_wet` | `footstep_concrete` + một đoạn `mud` ngắn ở −9 dB | 5 | **ghép hai bản thu**, không phải bản thu bước trên đá ướt |
| `step_mud` | rubberduck `mud` 06, 08, 10, 11, 13, 24, 25 | 7 | gói ghi là tiếng tác giả "làm trong bùn", không nói là bước chân |
| `step_earth` | Kenney RPG `footstep00–09` | 10 | gói không ghi bề mặt; có hai nhịp gót–mũi |
| `step_mat` | Kenney `footstep_carpet` | **3** | gói có 5 file nhưng 001 = 002, 003 = 004 (trùng byte). **Dưới mục tiêu 4–8 take** |
| `door_creak_soft` (giữ E) | Kenney `creak1–3` | 3 | cắt tối đa 0,45 s, để không rít kéo dài |
| `door_fast` (đẩy mở mạnh) | Kenney `doorOpen_1`; qubodup `DoorOpen01, 05, 06, 08` | 5 | bỏ `DoorOpen02, 04`: gần như toàn tần số cao, có thể là bản lề kim loại rít |
| `door_slam` (đóng sập) | qubodup `DoorClose02, 04, 07, 08`; Kenney `doorClose_3, 4` | 6 | bỏ bản có nhiều cú va rời và bản rít (`DoorClose05`) |
| `slide_fast`, `slide_slam` (cửa giấy trượt) | Kenney `impactWood_medium/heavy` | 5 + 5 | **chỉ có tiếng gỗ va khung, không có tiếng trượt** |
| `latch_drop`, `bolt_fast` | Kenney `impactWood_light`, `impactPlank_medium` | 5 + 5 | then gỗ rơi, giật then |
| `lock_open`, `key_turn` | Kenney `metalLatch`, `metalClick` | 1 + 1 | ổ khoá kim loại |

**Còn là âm tổng hợp:** `slide_soft` (trượt cửa khẽ), `latch_lift`, `bolt_slow`, `lock_rattle`, và mọi âm khác của chương (vải, hơi thở, tiếng cảnh bắt…).

**Loại giày:** cả bốn gói không ghi giày hay dép. Bước trên gỗ, đá, chiếu lấy từ cùng một gói Kenney Impact; đất lấy từ gói RPG; bùn từ gói khác. **Chưa bảo đảm cùng một loại chân/giày xuyên suốt**, và chưa biết có hợp với Kiều không. Cần nghe để quyết.

### 14.4 Phát trong game

- **Chỉ Ch4:** `FoleyBank` mới (`Presentation/FoleyBank.cs`) gắn vào `StoryAudio` của scene Ch4. Id trong bank thắng âm dùng chung cùng tên; các chương khác (Ch3 cũng vậy) không đổi âm. Đã kiểm tra: Ch4 tham chiếu bank, Ch3 không.
- **Không lặp liền:** bốc take ngẫu nhiên, không lặp lại take vừa phát.
- **Theo dáng:** bước chân hỏi trước take theo dáng (`step_wood.crouch/.walk/.run`), rồi tới id bề mặt, cuối cùng về âm cũ. Bản thu chỉ biến thiên pitch ±2 %; bỏ dịch pitch theo dáng vốn chỉ dùng cho âm tổng hợp.
- **Mức trong game:** mỗi nhóm có `Gain`, đặt để mức 120 ms đầu bằng âm cũ mà nó thay. Cân bằng với mưa và nhạc giữ như người dùng đã nghe; chỉ chất âm thay đổi.
  - Bước gỗ, đá, ướt +3–4 dB so với file; chiếu −4,9 dB; cửa đẩy mạnh −7,2 dB; đóng sập −1,9 dB; kẽo kẹt khẽ −8,2 dB.
  - Thứ tự giữ nguyên: khẽ < đẩy mạnh < đóng sập, và khom < đi < chạy (mức theo dáng vẫn do code bước chân quyết định từ bán kính tiếng động).
  - Hai nhóm chạm giới hạn ×0,35 (`slide_slam`, `bolt_fast`) nên to hơn âm cũ 1–3 dB.
- **Thời điểm cửa** (`HouseDoor`, dùng chung cả Ch3):
  - đẩy mở mạnh: phát ngay (then và cánh xoay);
  - đóng sập: phát lúc cánh chạm khung, đúng lúc AI nghe tiếng đập;
  - cửa cốt truyện đóng/mở bằng "slam" (J1, J2) cũng phát lúc cánh dừng.
  Như vậy không có tiếng va trước hình, và không có hai tiếng va trùng.
- **Lính và Tú Bà** bước bằng cùng bộ take (id bề mặt) ở vị trí của họ. Cảnh bắt dùng `door_fast` và `slide_fast` nên cũng dùng bản thu.
- **Logic nghe của AI không đổi:** bán kính tiếng động, `Noise`, `Watchman`, `TuBaSleeper` không sửa. Âm lượng loa không suy ra bán kính.

### 14.5 Nghe A/B (dành cho nhóm)

`Tools/LauTuBa/Foley/AB_<id>.wav`, một file cho mỗi id thay. Mỗi file phát âm cũ 4 lần, nghỉ 1 s, rồi các take mới, cùng mức cảm nhận (RMS 120 ms đầu). Nghe bằng tai nghe trước khi duyệt. Nếu take nào dở thì bỏ khỏi `Recipes` trong `Ch4Foley.cs` rồi chạy lại `BuildBatch`.

### 14.6 Kiểm tra

Mọi bài đều chạy **tắt tiếng**, nên chỉ chứng minh logic và routing, không chứng minh chất âm.

| Bài | Kết quả |
|---|---|
| Kiểm tra bản đồ | PASS |
| Âm thanh `-autopilotSound` (đủ bài) | PASS, 0 lỗi |
| Bắt `-autopilotCatches` | PASS: 13 ca bắt + 1 ca ngắt giữa chừng |
| Lén lút | PASS 5/5 |
| Ch4 thường, Ch4 thorough | PASS (J1, J2, nhánh Lam, cửa sau, checkpoint) |
| Ch3 thường | PASS |
| Chuỗi Ch3 → Ch8 | PASS |

- **Routing:** bài âm thanh ghi id từng bước. Trên mọi sàn và dáng, bước đều phát take thu theo dáng (`step_wood.crouch`, `step_stone.crouch`, `step_wet.crouch`, `step_mud.crouch`, `step_earth.crouch`, `step_mat.crouch`, và tương tự với walk/run).
- **Logic nghe giữ nguyên** (bán kính tiếng động không đổi):
  - khom/đi/chạy 1,5/4/9 m trên gỗ;
  - chiếu êm hơn gỗ ở cả ba dáng;
  - ở 3,5 m: lính nghe đi và chạy, không nghe khom; ở 6 m: chỉ nghe chạy;
  - cửa mở khẽ không nghe, mở mạnh thì lính tới đúng cửa;
  - gây tiếng rồi rời đi: lính tới đúng chỗ tiếng, không bám theo Kiều;
  - hai lính không chặn nhau;
  - Tú Bà: cửa phòng mụ bị đẩy mạnh làm mụ thức nhưng không thua; bắt bên giường và tay ở thắt lưng đều chạy cảnh bắt; sau checkpoint sạch;
  - không có bước giả.
- **Băng sân lần này:** khom thấy 4/30, đi kiểm tra 2/30, đuổi 1/30; đi đuổi 5/30; chạy bị nghe 22/30, đuổi 3/30. Cùng khoảng với 13.6; có dao động ngẫu nhiên.
- **Bản exe**, build sau mọi thay đổi (Success, **khớp source hiện tại**):
  - `AudioBankBaker` không ghi đè file nào (baked 0, kept 104; số mục audio trong `Resources` không đổi);
  - Ch4 ghi "C4 Foley recorded set loaded" ở cả 2 lần chạy;
  - lần bắt đầu tiên: khung chậm nhất 21 ms;
  - gameplay: p99 13,9 ms, dưới 60 FPS 0,1–0,2%, như đợt 13.

### 14.7 File

- **Mới:**
  - `Presentation/FoleyBank.cs`;
  - `Core/Editor/Ch4Foley.cs`;
  - `Assets/_Game/Audio/Ch4Foley/` (`Source/` gồm 114 file gốc và 2 License, 71 one-shot WAV, `Ch4FoleyBank.asset`, `CREDITS.md`);
  - `Tools/LauTuBa/Foley/AB_*.wav`;
  - `Tools/LauTuBa/QA/FoleySources.txt`, `FoleyBuild.txt`.
- **Sửa:**
  - `Presentation/StoryAudio.cs`: `_foley`, `HasFoley`, mức của nhóm;
  - `Gameplay/Stealth/FirstPersonFootsteps.cs`: chọn take theo dáng;
  - `Gameplay/House/HouseDoor.cs`: thời điểm tiếng đóng sập;
  - `Core/Editor/AudioBankBaker.cs`: `WriteWav` dùng chung;
  - `Core/Editor/LauTuBaSetup.cs`: gắn bank cho Ch4.
- **Scene:** chỉ sinh lại `Chapter04_LauTuBaDem`. Không xoá hay ghi đè asset cũ. Chưa commit.

### 14.8 Chưa xong hoặc chưa kiểm chứng

- **Chất âm chưa được nghe duyệt**, cả từng file lẫn trong game với mưa và nhạc.
- Chiếu chỉ có 3 take. Đá ướt là bản ghép. Cửa trượt thiếu tiếng trượt. Loại giày chưa thống nhất.
- Bản thu Freesound tốt hơn có thể cần đăng nhập (14.2).
- Chưa thử reverb hay occlusion. Không thêm vang.

## 15. Lính đuổi, tìm kiếm và bỏ cuộc ở Chương 4 (19/10/2026)

Thực hiện theo `docs/concept/CLAUDE_CH4_CHASE_SEARCH_FIX.md`. Bộ âm đợt 14 giữ nguyên làm baseline: không đổi âm, không đổi bán kính tiếng động. Ch3 không đổi gameplay. Không quay clip.

### 15.1 Cách tái hiện

Bài mới `-autopilotChase` (`Chapter04LauWorld.ChaseProbe.cs`) chạy trên scene Ch4 thật:
- **Lính bật nhận biết** (nhìn và nghe); Kiều di chuyển bằng CharacterController.
- Mỗi ca ghi lại:
  - chuỗi trạng thái kèm thời điểm;
  - lính tới gần vị trí cuối biết bao nhiêu;
  - thời gian kẹt lâu nhất (được lệnh đi mà không nhúc nhích);
  - các điểm tìm kiếm kèm trạng thái đường đi;
  - có bắt không;
  - khi trở lại tuần tra, cách điểm dừng gần nhất của tuyến đang đi bao nhiêu.
- **Ghi vết tuỳ chọn** `-traceWatch`: mỗi lần đổi trạng thái ghi một dòng `WATCH`, gồm lý do, vị trí, nơi thấy cuối kèm tuổi thông tin, nơi nghe cuối, mục tiêu, trạng thái đường đi, khoảng còn lại, vận tốc, độ nhận biết. Không ghi mỗi khung, không có overlay; tắt khi chơi bình thường.

Báo cáo: `Tools/LauTuBa/QA/chase_ch4.txt`. Bản "trước" và "sau" đều chạy cùng seed.

### 15.2 Nguyên nhân đã chứng minh và cách sửa

| Ca tái hiện | Trước khi sửa | Nguyên nhân | Sửa |
|---|---|---|---|
| Lính đang đi kiểm tra; Kiều gây 3 tiếng lớn, mỗi tiếng xa hơn | Vẫn đi tới tiếng đầu: cách tiếng thứ 2 là 5,2 m, cách tiếng thứ 3 là 10,1 m | `OnNoise` khi đang Investigate chỉ nhận tiếng **gần lính hơn** mục tiêu cũ. Ghi vết cho thấy `heard at (-10.5, 6.5)` nhưng `target (-11.4, -3.6)` | Lính đêm Ch4: tiếng mới thật sự nghe được thành mục tiêu, cách mục tiêu cũ trên 2 m thì đổi hướng. Không đổi trạng thái, không lặp câu quát. Tiếng gần mục tiêu cũ chỉ kéo dài thời gian xem |
| Một khung 150 ms ngay khi bỏ cuộc | Chuyển Patrol sau 0,2 s, đứng cách tuyến 7,7 m | `ReturnStep` chỉ ra lệnh về tuyến khi `_stateTime < 0,05 s`. Khung chậm (hoặc autopilot ×3: 1/60 × 3 = 0,05 s) bỏ qua lệnh, `IsMoving` = false nên coi là đã về | Cờ "vào trạng thái" (`_entered`) đặt lại ở mọi `SetState`: lệnh về tuyến chạy đúng một lần, không phụ thuộc độ dài khung. Điểm về chọn theo **đường đi** ngắn nhất, không theo đường chim bay |
| Cửa đóng chốt ngay sau lưng Kiều | 4 điểm tìm kiếm đều `PathPartial`, ở trong phòng khoá hoặc bên kia tường | `NextSearchPoint` chỉ dùng `SamplePosition`; `MoveTo` trả true cả khi đường cụt; tới cuối đường cụt thì "tới nơi" và nhìn quanh | Tâm tìm kiếm là nơi cuối biết nếu tới được, không thì điểm gần nhất tới được. Điểm tìm phải có đường **đầy đủ** từ tâm, dài không quá 1,6 × đường chim bay + 1,5 m. Kẹt khi đang tới thì bỏ điểm đó, không tính là đã xem |
| Đang tìm kiếm thì bị tạm dừng 2 s | Sau khi tiếp tục, đứng nguyên (3,4 → 3,4 m tới điểm) | `Resume()` không xử lý Search và Chase, mà `Pause` đã xoá đường đi | `Resume`: Search đi tiếp tới điểm đang tìm, Chase tính lại đường ngay, Return chạy lại lệnh về. Thời gian tạm dừng được cộng vào mốc thấy cuối và nghe cuối, nên hội thoại không làm thông tin "cũ đi" |
| Thấy Kiều chui sau bình phong, đường vào bị chốt cửa | **Bắt từ ngoài phòng khoá**, cách 2,0 m (qua cửa) | `TryCheckKnownHide` coi "không di chuyển" là đã tới, rồi lay và bắt | Chỉ lay và bắt khi đường tới chỗ nấp **đầy đủ** và lính dừng trong 1,8 m (sàn cạnh bình phong hoặc tủ có thể chặn sớm một chút; lần đầu đặt 1,1 m làm hỏng ca bình phong, đã nới). Đường cụt vì cửa chốt hoặc tường, hay bị kẹt, thì lính quên chỗ nấp và tìm quanh điểm gần nhất tới được. Không dùng thất bại thường để che việc này |
| Thấy Kiều nấp, Kiều ra ngay trước khi lính tới | Lính vẫn nhớ chỗ nấp | Không ai xoá `KnownHide` khi Kiều ra | `HideSpot.Exited`: chỗ nấp đã trống thì lính quên nó |
| Đang đuổi, cốt truyện (J1) sai lính đi chỗ khác | Bị kéo ra khỏi truy đuổi (`Scripted`) | `Script()` đổi trạng thái vô điều kiện | `Script()` từ chối khi đang Chase (trả false). `Alarm()` (Tú Bà gọi, ally) cũng bỏ qua khi đang Chase. J1 (vào văn phòng) và J2 (sai lính đi kiểm tra) chỉ sai khi lính không đang đuổi |
| Đi theo cốt truyện tới chỗ không vào được (văn phòng khoá) | Báo "đã tới" khi còn cách 3,5 m | `Scripted` gọi callback khi `!IsMoving` | Chỉ gọi khi thật sự tới (≤ 1,2–1,5 m). Không tới được thì bỏ lệnh, quay về tuyến, không gọi callback. J1 và J2 đã có nhánh xử lý khi không tới |

**Bổ sung chống kẹt:** đếm thời gian "được lệnh đi nhưng vận tốc < 0,05 m/s"; quá 1,5 s thì coi là kẹt.
- **Chase:** mất dấu quá `_lostSeconds` (3 s, chỉnh được) mà đã tới nơi cuối biết, hoặc đường không đầy đủ, thì chuyển sang tìm kiếm. Trễ nhất 5 s sau đó thì vẫn chuyển, nên không bao giờ đuổi mãi vào chỗ không tới được.
- Thấy Kiều mà không tới được thì đứng nhìn theo, không chạy vô hạn tại chỗ.
- Patrol và Investigate coi kẹt là đã dừng.

### 15.3 Sau khi sửa

| Ca | Kết quả |
|---|---|
| Mất dấu sau góc (vào phòng Lam), Kiều đứng yên | Lính tới, thấy lại, đuổi và bắt (hợp lệ) |
| Như trên, Kiều gây tiếng | Như trên |
| Như trên, Kiều lẻn đi | Tới đúng nơi cuối biết (0,0 m), tìm 3 điểm (đều đường đầy đủ), bỏ cuộc có câu nói, về tuyến cách 0,3 m |
| 3 tiếng xa dần | Theo tiếng mới nhất (0,0 m mỗi lần), rồi về tuyến (0,2 m) |
| Cửa chốt sau lưng (lính cách 1,8 m khi cửa đóng) | Không bắt; tìm 3 điểm bên ngoài, đều đường đầy đủ; về tuyến 0,3 m |
| Khung 150 ms khi bỏ cuộc | Đi về tuyến, tới cách 0,2 m mới tuần tra |
| Tạm dừng 2 s giữa tìm kiếm | Đi tiếp tới điểm (4,1 → 2,3 m trong 1,2 s) |
| Thấy nấp, đường bị chốt | Không bắt; tìm quanh rồi bỏ cuộc |
| Thấy nấp, Kiều ra ngay | Quên chỗ nấp; tìm rồi về tuyến; không bắt |
| Cốt truyện sai đi giữa lúc đuổi | Vẫn đuổi |
| Cốt truyện sai vào phòng khoá | Không báo tới; quay về tuyến |

### 15.4 Kiểm tra hồi quy

Editor batch, scene Ch4 thật, sau các sửa trên.

| Bài | Kết quả |
|---|---|
| `-autopilotChase` (11 ca, lính bật nhận biết, `-traceWatch`) | PASS 11/11 (trước khi sửa: 9 FAIL) |
| Kiểm tra bản đồ | PASS |
| Âm thanh `-autopilotSound` (đủ bài: nghe, hai lính, Tú Bà gọi lính, băng sân) | PASS |
| Bắt `-autopilotCatches` | PASS: 13 ca + ngắt giữa chừng, 14 cảnh bắt |
| Lén lút | PASS 5/5 (lính tìm thấy Kiều sau bình phong, cách 0,1 m) |
| Ch4 thường, Ch4 thorough (J1, J2, nhánh Lam, cửa sau, checkpoint) | PASS |
| Ch3 thường, Ch3 thorough, chuỗi Ch3 → Ch8 | PASS |

Một lỗi do chính đợt sửa gây ra đã được bắt và sửa: quy tắc chỗ nấp "≤ 1,1 m" làm ca bắt sau bình phong phòng Lam hết giờ (lính tới, dừng xa hơn 1,1 m, rồi bỏ). Nới thành "đường đầy đủ, ≤ 1,8 m" thì bắt, chase và stealth đều chạy lại PASS. Các bài tuyến truyện tắt nhận biết không chạm tới quy tắc này.

**Bản exe** build sau mọi thay đổi (Success, **khớp source hiện tại**): bài `-autopilotChase` chạy 4 lần.
- 3 lần PASS 11/11, cả 3 đều có `-traceWatch`.
- **1 lần (lần đầu, không bật ghi vết) ca "3 tiếng xa dần" FAIL:** sau tiếng thứ hai, lính cách tiếng đó 4,1 m; tới giây 6,8 lính chuyển sang đuổi và không về tuyến trong 40 s.
  - Không tái hiện được trong 3 lần exe có ghi vết và 3 lần Editor sau đó.
  - Nguyên nhân **chưa xác định**; vì không có ghi vết nên không suy đoán. Nghi vấn cần kiểm tra: một khung đầu ca nhìn thấy Kiều qua cửa phòng Kiều ngay sau dịch chuyển (trạng thái vùng cấm cập nhật trễ).
  - Ghi là lỗi chập chờn còn mở.
- Khung hình exe trong bài này: trung vị 9,0 ms, p99 14,5 ms. Bài liên tục dịch chuyển và có một khung cố ý ngủ 150 ms, nên không dùng để so hiệu năng.

### 15.5 File đã đổi

- `Gameplay/Stealth/Watchman.cs`:
  - ghi vết `-traceWatch`;
  - kẹt (`Stuck`, `Stopped`);
  - lệnh vào trạng thái chạy một lần (`_entered`);
  - Chase, Search, Return, Investigate, Scripted và kiểm tra chỗ nấp như 15.2;
  - `ReachableNear`, `NextSearchPoint` có kiểm tra đường đi;
  - `Pause`/`Resume` giữ tuổi thông tin;
  - `OnHideExited`;
  - `Script()` trả bool;
  - `Alarm()` bỏ qua khi đuổi;
  - `_lostSeconds`;
  - `Waypoints`.
- `Core/Story/StoryActor.cs`: đọc `PathStatus`, `PathPending`, `RemainingDistance`, `Destination`, `Velocity` (chỉ đọc).
- `Gameplay/Chapters/LauTuBa/Chapter04LauWorld.cs`: J1 và J2 không sai lính đi khi đang đuổi; hook `-autopilotChase`.
- **Mới** `Gameplay/Chapters/LauTuBa/Chapter04LauWorld.ChaseProbe.cs`.
- Không sinh lại scene, không đổi asset, chưa commit.

**Phạm vi dùng chung:** `Watchman` cũng dùng ở Ch3. Các sửa lỗi (về tuyến, kẹt, điểm tìm có đường, chỗ nấp phải tới được, tạm dừng, cốt truyện không cắt truy đuổi) áp dụng cho cả Ch3. Đó là sửa lỗi, không đổi luật. Riêng luật "tiếng mới nhất" chỉ bật cho lính đêm Ch4 (`_carefulListener`). Ch3 đã chạy hồi quy (15.4).

### 15.6 Chưa kiểm chứng

- **Chưa chơi tay:** chưa xác minh bằng cảm giác việc người chơi có đọc được lính đang làm gì; thời điểm an toàn để ra khỏi chỗ nấp chỉ đo bằng tự động.
- **Ca "thấy mà không tới được"** (ví dụ qua song cửa) chưa có bố trí tái hiện riêng; code đã xử lý (đứng nhìn theo, rồi tìm kiếm khi mất dấu).
- **Hai lính đuổi cùng lúc** chưa có ca riêng. Hai lính cùng nghe một tiếng và bắt cùng lúc thì đã có trong các bài âm thanh và bắt.
- **Lỗi chập chờn "3 tiếng xa dần"** trên exe: 1/7 lần, chưa rõ nguyên nhân (15.4).
- **Ngưỡng** 3 s mất dấu, 20 s tìm kiếm, 3 điểm, 45 s cảnh giác sau khi bỏ cuộc: giữ như cũ, chưa cân bằng bằng chơi thử.

## 16. Tú Bà ngủ, tiếng ngáy và lấy chìa (20/10/2026)

Thực hiện theo `docs/concept/CLAUDE_CH4_TUBA_KEYS_REFINEMENT.md`, dựa trên phản hồi từ chơi thực tế.
- Phạm vi: chỉ Chương 4. Ch3 không đổi.
- Bộ Foley bước chân và cửa của đợt 14 giữ nguyên.
- Lỗi chập chờn của đợt 15 vẫn mở.
- Không quay clip, chưa commit.

### 16.1 Vòng đời ngủ và thức

**Nguyên nhân lỗi ngáy sai lúc.**
- `TuBaSleeper.State` mặc định là `Asleep`.
- `Update()` chạy nhịp thở và phát ngáy chỉ dựa vào `State`, không kiểm tra Tú Bà đã thật sự nằm hay chưa.
- Vì vậy mụ ngáy cả khi đang đứng ở cửa Kiều trong đoạn thoại mở đầu và khi đi về phòng.

**Đã sửa:**

- **Cờ "đã lên giường".** Nhịp thở chỉ chạy khi chương đã đặt Tú Bà lên giường (`Restore(Asleep)` trong `PutTuBaToBed`) và `StoryActor.IsLying` đúng.
  - `PlaceTuBaAtKieuDoor` gọi `GetUp()`.
  - Ở cửa, khi đi về, sau J2 và khi bị bắt: không thở, không nghe như người ngủ, không nhận tương tác lấy chìa (`KeyTheft.IsAvailable` cần `InBed`).
  - Snapshot vẫn chỉ lưu `Asleep` hoặc `Awake`. Đầu chương không dùng `Awake` để thay cho trạng thái "đang đứng".
- **Nguồn âm riêng.** Ngáy phát trên AudioSource riêng tên `Breath`, đặt dưới Tú Bà (tạo bằng `StoryAudio.OwnSource`), không dùng pool chung `PlayAt`.
  - Nguồn này dừng ngay khi mụ cựa mình, thức nằm nghe, bị chộp tay, tỉnh hẳn (J2), đứng dậy hoặc khi tắt scene.
  - Chỉ dừng nguồn này; không tắt tiếng nhân vật hay nền.
  - Âm 3D, tầm 11 m, đường giảm âm như các tiếng trong nhà: gần nghe rõ, xa nhỏ dần, không phủ cả map.
- **Một đồng hồ duy nhất.**
  - `TuBaSleeper` chạy trước mọi script đọc nó trong cùng khung (`[DefaultExecutionOrder(-100)]`). Tay lấy chìa, nhãn tương tác và âm thanh đều đọc cùng một pha thở.
  - Mỗi khung ghi số giây hít vào và thở ra trong khung đó (`InhaledThisFrame`, `ExhaledThisFrame`). Một khung dài có thể vắt qua ranh giới hai pha.
  - Âm của mỗi pha phát lại từ đúng vị trí đồng hồ (`AudioSource.time = phase`).
  - Tạm dừng (timeScale 0 kèm `AudioListener.pause`): đồng hồ và âm cùng đứng.
  - Khi đang có cảnh bắt: đồng hồ giữ nguyên, âm tắt.

### 16.2 Tiếng ngáy bằng bản thu người thật

- **Nguồn.** 4 file trên Wikimedia Commons, tải không cần đăng nhập, người dùng cho phép, lưu ở ổ D.
  - Không có bản CC0 phù hợp, cũng không có bản giọng nữ. Cả 4 là giọng nam, giấy phép **CC BY-SA 3.0**: phải ghi công, và đoạn đã biên tập cũng theo CC BY-SA.
  - Chi tiết: `Assets/_Game/Audio/Ch4Foley/CREDITS.md`.
  - Dùng 3 file: `Tongue_base_snoring` và hai file `Palatal_snoring` của Langenbergstefan.
  - Không dùng `Snore_man_50_years_160_kg`: nhịp dồn, khoảng yên chỉ 0,4–1,3 s.
- **Đo trước khi cắt** (`Tools/LauTuBa/QA/SnoreSources.txt`): hai file chính có nhịp rất đều.
  - Ngáy dài 1,0–1,6 s, sau đó 2,3–3,0 s yên với tiếng thở ra nhỏ.
  - Nhịp này gần với đồng hồ game (1,7 s hít vào, 2,6 s thở ra).
- **Biên tập** (`Core/Editor/Ch4Snore.cs`, chạy bằng `-executeMethod ThuyKieu.Core.Editor.Ch4Snore.BuildBatch`).
  - Mỗi chu kỳ thật tách thành hai đoạn:
    - `snore_deep_NN` (tiếng ngáy hít vào): 1,04–1,65 s;
    - `snore_out_NN` (hơi thở ra ngay sau): tối đa 2,52 s, dừng 0,3 s trước tiếng ngáy kế tiếp.
  - 12 cặp, chỉ chọn những chu kỳ đều.
  - Nâng cao độ 1 bán cung và lọc thông thấp. Không kéo giãn thời gian.
  - Mức âm: ngáy −16 dBFS; thở ra khoảng −31 dBFS. Đoạn thở ra được nâng tối đa +9 dB để nghe được khi đứng cạnh sập.
  - Các đoạn chỉ nằm trong bank Foley của Ch4, không đổi âm chương khác.
  - `Ch4Foley.Build` nay giữ lại các nhóm ngáy khi dựng lại bank.
- **Lịch nhịp thở theo bản thu:**
  - Hít vào kéo dài đúng bằng đoạn ngáy được chọn (1,04–1,65 s). Tiếng ngáy không bao giờ tràn sang lúc luật đang tính là thở ra.
  - Thở ra cố định 2,6 s.
  - Phân biệt hai pha:
    - hít vào = tiếng ngáy khàn, to (nguy hiểm);
    - thở ra = hơi thở nhỏ, nhỏ dần trước khi hết pha (an toàn).
  - Không có bank Ch4 thì quay về âm tổng hợp cũ với 1,7 s / 2,6 s.
- **File nghe so sánh:** `Tools/LauTuBa/Foley/AB_snore.wav`, gồm 4 nhịp tổng hợp cũ, 1 s im lặng, rồi 12 nhịp mới đặt theo đồng hồ trong game.
- **Chưa ai nghe kiểm chất âm.** Kiểm thử tự động không phát tiếng: nó chỉ xác minh trạng thái và pha thở, không chứng minh tiếng ngáy nghe tự nhiên.

### 16.3 Chùm chìa trên người Tú Bà

Dựng riêng cho Ch4 (`Core/Editor/LauTuBaKeyRing.cs`). `StoryPropKit.KeyRing` dùng chung cho các chương khác vẫn giữ nguyên.

- **Hình dáng:**
  - Một mesh duy nhất, đơn vị mét, normal và UV đúng.
  - Móc chữ S: một vòng mắt quanh dây thắt lưng, một cổ, một vòng mắt dưới.
  - Vòng dây tròn có lỗ, tiết diện tròn (bán kính 3 cm, dây 2,6 mm).
  - 5 chìa dài 3,4–5,8 cm, mỗi chìa có:
    - đầu cầm khoét lỗ; riêng chìa "con cá" có đầu cầm bầu dục;
    - cổ, thân, và răng ở mũi gồm 1–3 răng dài ngắn khác nhau;
    - góc xòe và góc xoay riêng.
  - Các cạnh được vát.
- **Vật liệu:** `LTB_KeyBronze`, URP Lit, kim loại, không tự phát sáng.
  - Bản đồ màu và metallic/smoothness tạo bằng code (`LauTuBaTextures.KeyBronze`).
  - Mép vát và mặt trong vòng (chỗ chìa cọ vào) bóng và sáng.
  - Các mặt còn lại là lớp gỉ sẫm, có vệt oxy hoá và chấm xanh đồng, nhám hơn.
- **Gắn và treo:**
  - Một điểm neo trên xương hông, đặt ngay ngoài mép hông thật của mụ. Mép hông đo từ mesh đã bake.
  - `HangingKeys` cho chùm chìa rủ theo trọng lực:
    - đứng: rủ xuống cạnh đùi, xòe ra một chút để không xuyên áo;
    - nằm: chìa đổ ra phía hướng phòng (hông trái) và nằm trên chiếu. Độ nghiêng tính từ độ cao thật của móc so với mặt chiếu (raycast), nên chìa không xuyên chiếu cũng không lơ lửng;
    - đổi tư thế thì đung đưa về chỗ, không giật.
- **Tú Bà nằm lơ lửng:** phát hiện khi xem ảnh.
  - Trước khi sửa, cả người mụ nằm cách mặt chiếu khoảng 12 cm. Lỗi có từ trước: `LieDown` nâng cố định 0,45 m mà không tính thân hình của rig khi lật ngửa.
  - Ch4 nay đo điểm thấp nhất của thân (mesh đã bake, chỉ lấy phần nằm trên sập), rồi chỉnh độ nâng để mụ nằm sát chiếu (0,33 m). Chỉ đo lần đầu, các lần sau dùng lại kết quả.
  - Sửa trong `Chapter04LauWorld.LayOnSap`; `StoryActor` dùng chung không đổi.
- **Mục tiêu tương tác:** hộp trigger 0,42 × 0,34 × 0,42 m, luôn đặt đúng giữa chùm chìa (đi theo tư thế).
  - Thay cho hộp 1,3 m cũ đặt giữa sập.
  - Collider thân của Tú Bà: khi nằm thì nằm dọc theo sập, không còn là một cột đứng vô hình ở chân sập.

### 16.4 Luật lấy chìa

- **Đặt tay:** bấm E khi nhìn vào chùm chìa.
  - Nếu E đã giữ sẵn từ trước (đang mở cửa, nhìn chỗ khác) rồi mới nhìn tới chìa: chưa tính, phải bấm lại.
- **Đúng nhịp:** tay đang ở chìa và mụ thở ra thì tiến độ tăng đúng bằng thời gian thở ra trong khung đó.
  - Đủ **12 giây tích lũy** thì gỡ được chìa. Thời gian chờ không được tính.
  - Tiến độ giữ nguyên qua các nhịp thở và khi buông E. Checkpoint thì quay về 0, hoặc giữ là đã lấy nếu đã lấy xong.
  - 12 s được đặt trong setup (`Set(theft, "_seconds", 12f)`) và trên instance trong scene (`_seconds: 12`). Mặc định trong source cũng là 12.
- **Sai nhịp** (tay vẫn ở chìa khi mụ hít vào): mụ chộp tay ngay. Các trường hợp:
  - giữ E từ thở ra vắt sang hít vào;
  - bấm E trong lúc hít vào;
  - khung dài vắt qua ranh giới hai pha;
  - mụ đang cựa mình hoặc thức nằm nghe.

  Thứ tự xảy ra:
  1. Tiến độ dừng; khung đó không cộng gì.
  2. Tiếng ngáy tắt.
  3. `CatchHand`: mụ thức và nắm cổ tay. Không gọi lính, không phát sự kiện `Roused`.
  4. Mụ ngồi dậy và cảnh bắt của Tú Bà chạy.
  5. Thất bại một lần, rồi về checkpoint.

  Cờ `_tuBaCaught` chặn cảnh bắt bên giường chạy lần hai.
- **Mất mục tiêu:** quay mắt đi, khoá điều khiển hoặc có hội thoại thì coi như rút tay. Không bị bắt, không cộng tiến độ.
  - Muốn đặt tay lại phải bấm E lại.
  - Nếu ngay khung mụ bắt đầu hít vào mà E vẫn giữ và mắt vừa rời chìa, vẫn bị tính là sai nhịp.
  - Vì vậy không thể giữ E rồi liếc đi để lách luật.
- **Nhãn tương tác** (cùng đồng hồ):
  - "Thở ra — giữ E để gỡ chìa": màu xanh nhạt; chuyển vàng trong 0,5 s cuối của pha thở ra.
  - "Hít vào — buông E": màu đỏ.
  - Nhãn riêng khi mụ cựa mình hoặc thức.
- **Thông báo thất bại** giải thích lỗi giữ E sai nhịp và nhắc rằng tiến độ được giữ giữa các hơi thở.
- **Lời gợi ý khi nhìn qua khe kho** đã sửa cho khớp luật: "Mụ ngáy khi hít vào — lúc ấy phải buông tay; chỉ gỡ chìa khi mụ thở ra."
- **Lỗi tìm ra khi kiểm minigame thật (đã sửa):** ngồi khom cạnh sập và nhìn xuống chùm chìa thì không tương tác được.
  - Mắt khi khom nằm ngay trên capsule thân của chính Kiều, nên tia nhìn chạm thân mình trước (cách 2 cm) và dừng ở đó.
  - `PlayerInteraction` nay bỏ qua collider của chính người chơi. File này dùng chung, nhưng chỉ ảnh hưởng tương tác nhìn ở góc thứ nhất (Ch3–4). Không có gì đang chạy được mà bị hỏng; Ch3 đã chạy hồi quy.

### 16.5 Kiểm thử

**Bài mới `-autopilotKeys`** (`Chapter04LauWorld.KeyProbe.cs`):
- Chạy trên scene Ch4 thật, qua đường tương tác thật.
- Kiều được đặt cạnh sập, mắt quay vào chùm chìa. E được "bấm và giữ" qua `InputReader.SimulatedInteract`, nên `PlayerInteraction` tự tìm mục tiêu và gọi `KeyTheft` như với người chơi.
- Không gọi thẳng `KeyTheft.Tick`. Không dùng `TookKeys()` để bỏ qua.

**Các autopilot thường cũng lấy chìa qua minigame thật:** chạy thường, thorough, chuỗi, resume, các bài bắt, lén lút, đuổi. Bước `TookKeys()` bỏ qua minigame đã bị gỡ.

**Theo dõi nhịp thở trong mọi lần chạy autopilot:** mỗi khung kiểm tra ngáy chỉ phát khi mụ đang ngủ trên giường, và mụ chỉ có một nguồn âm thở.

Kết quả bài `-autopilotKeys` (Editor, bản source cuối):

| Ca | Kết quả |
|---|---|
| Đầu chương: thoại ở cửa Kiều, mụ đi về phòng | Không ngáy (0 khung). Không lấy được chìa. Chùm chìa ở hông, cách xương hông 24 cm, rủ xuống 6° |
| Lên giường | Bắt đầu thở bằng nhịp hít vào, ngay khi mụ nằm |
| Nằm | Chìa nằm trên chiếu, cách 1,8 cm, ra phía phòng, cách đường giữa thân 34 cm. Mục tiêu tương tác ở đúng giữa chùm chìa (lệch 0,0 cm) |
| Ngồi khom và đứng cạnh sập, nhìn vào chìa | Mục tiêu là chùm chìa (sau khi sửa lỗi tia nhìn chạm thân mình, 16.4) |
| Tạm dừng 1 s | Pha thở và âm cùng đứng: còn lại 1,151 s trước và sau |
| Cựa mình / ngủ lại / thức nằm nghe | Ngáy tắt ngay; ngủ lại thì thở lại từ hít vào; thức thì không thở |
| Giữ E ở vật khác qua lúc hít vào; đứng xa 3 m; E giữ sẵn từ trước rồi mới nhìn vào chìa; quay mặt đi khi đang giữ; khoá điều khiển khi đang giữ | Không bị bắt, không cộng tiến độ. Muốn đặt tay lại phải bấm E lại |
| Một hơi thở ra: bấm sau 0,25 s, buông trước 0,3 s | Cộng 2,05 s. Giữ nguyên qua lúc hít vào |
| Giữ vắt sang hít vào; bấm trong lúc hít vào; bấm khi mụ cựa mình; giữ vắt ranh giới ở khoảng 8 FPS; một khung 0,3 s vắt ranh giới | Mỗi ca: **một** cảnh bắt Front của Tú Bà, một lần tính bị bắt, một lần về checkpoint. Khung bị bắt không cộng gì (0,000 s). Ngáy đã tắt lúc mụ chộp tay. Sau checkpoint: tiến độ 0, mụ ngủ, chìa trở lại |
| Khoảng 8 FPS, giữ đúng nhịp | Phần cộng thêm bằng đúng thời gian thở ra lúc tay ở chìa (2,06 s) |
| Tư thế lúc bị bắt | Mụ đứng dậy, chùm chìa ở hông, rủ xuống 6° |
| **Làm đúng từ 0** | **12,00 s giữ hợp lệ qua 6 hơi thở ra, tổng 22,3–23,8 s tính cả lúc chờ** (các lần chạy; mục tiêu 20–30 s). Lấy được đúng 1 lần, inventory có 1 bộ chìa, chùm chìa ẩn, chuyển sang bước 2 |
| Đã có chìa, giữ E chỗ cũ qua lúc hít vào | Không có gì để lấy, không bị bắt |
| Checkpoint sau khi lấy chìa | Chìa vẫn đã lấy, chùm chìa vẫn ẩn, mụ ngủ tiếp |

Trong các lần chạy autopilot thường, bot (bấm sau 0,25 s, buông trước 0,3 s) lấy chìa mất 21,4–24,1 s thời gian game.

**Hồi quy** (Editor, scene Ch4 dựng lại bằng `-onlyChapter4`):

| Bài | Kết quả |
|---|---|
| Kiểm tra bản đồ | PASS |
| Âm thanh Tú Bà (`-soundOnly tuba`): cửa xa bị mở mạnh, mụ thức nằm nghe, cựa mình, bên giường, tay ở thắt lưng khi mụ cựa mình | PASS. Cửa xa chỉ làm mụ thức nằm nghe và gọi lính, không thất bại |
| Bắt (`-autopilotCatches`) | PASS: 13 ca và ngắt giữa chừng |
| Lén lút | PASS 5/5 |
| Đuổi (`-autopilotChase`, lính bật nhận biết) | PASS 11/11 |
| Ch4 thường, Ch4 thorough (J1, J2, nhánh Lam, hợp đồng, cửa sau, checkpoint) | PASS. Sau J2: mụ thức, không thở, không ngáy |
| Lưu giữa chương rồi tiếp tục (`-autopilotMidSave` → `-autopilotResume`) | PASS. Mụ lên giường ngay khi vào lại, không ngáy sai lúc, lấy chìa qua minigame |
| Ch3 thường, Ch3 thorough (sau sửa `PlayerInteraction` dùng chung) | PASS |
| Chuỗi Ch3 → Ch8 | PASS |

**Bản exe:** build Success từ **source cuối**. Chạy `-autopilotKeys` 2 lần ở 1920×1080:
- Cả 2 lần **PASS**, mỗi lần 36/36 bài.
- Làm đúng từ 0: 12,00 s giữ hợp lệ, tổng 23,0 s và 22,2 s.
- Sau J2: không thở, không ngáy. Không lần nào mụ ngáy khi không nằm ngủ trên giường.
- Trung bình 135 fps; p99 13,9 ms; 0,1 % số khung dưới 60 fps.
- Khung tệ nhất 312 ms là khung cố ý ngủ 0,3 s trong bài, nên không dùng số này để so hiệu năng.
- Các lần exe trước khi sửa bài thử có 2 bài FAIL. Nguyên nhân là lính đi tuần sau checkpoint (xem dưới), không phải luật lấy chìa.

**Lỗi bài thử đã gặp và sửa (không phải lỗi game):**
- Lần đầu, bài thử đánh thức mụ khi Kiều đang đứng bên giường, nên chính mụ bắt Kiều (đúng thiết kế).
- Bot bấm giữa chừng một hơi thở ra.
- Đếm số lần thất bại sau khi checkpoint đã khôi phục bộ đếm.
- Trên exe, sau mỗi checkpoint lính được đặt lại tuyến và đi tuần. Tiếng chân lính làm mụ thức khi Kiều đang ở bên giường, nên mụ bắt bằng nhánh "bên giường" hợp lệ. Bài thử nay giữ lính đứng yên sau mỗi checkpoint.

**Đã xem ảnh** (`Tools/LauTuBa/Views/keys16/`, ảnh tĩnh, không phải clip):
- `KEYS_lying_close`, `KEYS_lying_crouched`: mụ nằm sát chiếu; chùm chìa đồng có vòng và 5 chìa tách rời, nằm cạnh hông.
- `KEYS_standing_side`, `KEYS_standing_front`: chùm chìa ở hông trái cạnh tay, không xuyên áo.
- `KEYS_catch`: mặt Tú Bà lúc chộp, chùm chìa ra ngoài khung.

Ảnh ở độ phân giải QA trong phòng tối, chưa đủ để kết luận chất lượng cận cảnh; cần người xem trực tiếp trong game.

### 16.6 Chưa kiểm chứng

- **Chất âm ngáy chưa ai nghe.** Kiểm thử tự động không phát tiếng, chỉ xác minh trạng thái và pha. Bản thu là giọng nam nâng 1 bán cung và là CC BY-SA. Nếu phát hành rộng, phải ghi công trong màn credits của game (hiện chưa có màn đó; ghi công mới chỉ có trong `CREDITS.md`).
- **Chưa chơi tay.** Chưa biết người chơi có đọc được nhịp thở nhờ tiếng và chữ không, và 12 s (khoảng 22 s tính cả lúc chờ) có hợp cảm giác không.
- **Chùm chìa:** mới xem qua ảnh tĩnh. Chưa xem cận cảnh dưới đèn thật. Chưa có normal map cho vết khắc "con cá".
- Lúc ngồi dậy hay bị bắt, chùm chìa đung đưa về chỗ trong khoảng 0,3 s. Khi lên giường thì chìa đặt ngay xuống chiếu.
- Lỗi chập chờn của đợt 15 ("3 tiếng xa dần" trên exe) **vẫn mở**. Đợt này không đụng tới.

### 16.7 File đã đổi

- `Gameplay/Chapters/LauTuBa/TuBaSleeper.cs`:
  - cờ lên giường, `GetUp`;
  - nguồn âm riêng;
  - đồng hồ một nhịp, chạy trước các script khác;
  - nhịp hít vào theo bản thu;
  - `CatchHand(Grab)`.
- `Gameplay/Chapters/LauTuBa/KeyTheft.cs`: luật mới; mục tiêu đi theo chùm chìa; 12 s.
- **Mới** `Gameplay/Chapters/LauTuBa/HangingKeys.cs`.
- `Interaction/PlayerInteraction.cs`: tia nhìn bỏ qua thân người chơi.
- `Gameplay/Chapters/LauTuBa/Chapter04LauWorld.cs`:
  - `GetUp` ở cửa Kiều;
  - `LayOnSap`: mụ nằm sát chiếu;
  - collider thân theo tư thế;
  - thông báo thất bại;
  - lời gợi ý;
  - autopilot lấy chìa qua minigame thật.
- **Mới** `Gameplay/Chapters/LauTuBa/Chapter04LauWorld.KeyProbe.cs` (`-autopilotKeys`).
- `Gameplay/Chapters/LauTuBa/Chapter04LauWorld.SoundProbe.cs`: phần lấy chìa dùng minigame thật.
- `Presentation/StoryAudio.cs`: `OwnSource`, `Effect`, `EffectsLevel` (chỉ thêm, không đổi hành vi cũ).
- `Core/InputReader.cs`: `SimulatedInteract`, chỉ cho QA. Khi chơi bình thường không ai bật nó.
- **Mới** `Core/Editor/Ch4Snore.cs`, `Core/Editor/LauTuBaKeyRing.cs`.
- `Core/Editor/Ch4Foley.cs`: dựng lại bank thì giữ các nhóm ngáy.
- `Core/Editor/LauTuBaTextures.cs`: `KeyBronze`.
- `Core/Editor/LauTuBaSetup.cs`: chùm chìa mới, mục tiêu tương tác, 12 s. Đã bỏ hàm `KeysAtBelt` cũ.
- **Asset:**
  - `Audio/Ch4Foley/Source/commons_snore/`: 4 file nguồn;
  - `Audio/Ch4Foley/snore_deep/`, `snore_out/`: mỗi thư mục 12 file;
  - `Ch4FoleyBank.asset`: thêm 2 nhóm;
  - `CREDITS.md`;
  - mesh `LTB_KeyRing`, vật liệu `LTB_KeyBronze`, 2 texture.
- Scene `Chapter04_LauTuBaDem` sinh lại bằng `-onlyChapter4`.

## 17. Ch3: đồng bộ bước chân và tiếng cửa với Ch4 (21/10/2026, chưa kiểm thử xong)

Thực hiện theo `docs/concept/CLAUDE_CH3_SYNC_FOOTSTEPS_DOORS.md`. Đợt này bị ngắt giữa chừng; mục này ghi lại trạng thái hiện tại.

**Đã làm:**

- **Kiểm tra trước khi sửa.** Công cụ `Core/Editor/Ch3AudioSync.cs`, bản báo cáo ở `Tools/LauTuBa/QA/Ch3AudioAudit.txt`. Kết quả:
  - Scene Ch3 không gắn bank Foley nào, nên mọi bước chân và tiếng cửa đều là âm tổng hợp cũ.
  - Ch3 thiếu tag thảm/chiếu (Ch4 có 6), tag hẻm là đá ướt thay vì bùn, bồn cây chưa có tag.
  - Thông số cửa và `FirstPersonFootsteps` đã giống hệt Ch4. Đường phát bước chân cũ trong `StoryAudio` tự bỏ qua ở góc nhìn thứ nhất, nên không có hai tiếng cho một bước.
- **Bank riêng cho Ch3:** `Ch3FoleyBank.asset`.
  - Dùng lại đúng các clip, take và gain của nhóm bước chân, cửa, then, chốt và khóa trong bank Ch4. Không chép file WAV, GUID giữ nguyên.
  - Không chứa tiếng ngáy.
  - Mỗi lần `Ch4Foley.Build` chạy, bank Ch3 được làm mới theo.
- **Patch scene Ch3 thật** bằng `Ch3AudioSync.PatchBatch` (qua API Unity, không sửa YAML):
  - gắn bank;
  - thêm tag thảm/chiếu, bồn cây (đá), hẻm (bùn);
  - bản scene trước khi patch được lưu trong scratchpad của phiên.
- **Giữ nguyên cách lính Ch3 nghe bước chân.** `SurfaceTag` có thêm giá trị tùy chọn `_carry` (tầm vang). Ở Ch3, thảm và hẻm phát đúng tiếng mới, nhưng tiếng vang tới lính đúng như trước:
  - thảm vang như sàn bên dưới;
  - hẻm vang như đá ướt.
- **Âm lượng theo loại sàn như Ch4.** `FirstPersonFootsteps` lấy âm lượng bước chân theo loại sàn (như Ch4), còn tầm nghe của lính theo `_carry`. Ở Ch4 hai giá trị này bằng nhau, nên Ch4 không đổi.
- **Cấu hình tạo scene:** `LauTuBaSetup.CreateChapter3` gọi `Ch3AudioSync.Apply`, nên các lần dựng sau không quay về âm cũ.

**Chưa làm hoặc chưa kiểm chứng:**

- Bồn cây đang tag đá ×1,1 (trước đây không có tag, tức ×1,0). Cần đặt `_carry` = 1 để lính Ch3 nghe đúng như trước.
- Chưa chạy autopilot Ch3, chuỗi Ch3→Ch8, thử từng bề mặt và từng kiểu cửa ở Ch3.
- Chưa dựng lại Ch3 bằng `CreateChapter3` để kiểm cấu hình tạo scene.
- Chưa nghe âm thanh.
- Exe chưa có các thay đổi này.

## 18. Minigame lấy chìa cận cảnh (21/10/2026)

Thực hiện theo `docs/concept/CLAUDE_CH4_KEY_THEFT_MINIGAME.md`.
- Chỉ Ch4. Giữ luật nhịp thở, 12 s, cảnh bắt, checkpoint, nhiệm vụ và âm đã duyệt của §16.
- Không quay clip, chưa commit.

### 18.1 Điều khiển

1. **Vào minigame.** Nhìn chùm chìa và bấm E: Kiều cúi sát. Lần bấm này không tính là nắm, kể cả khi bà đang hít vào. Camera tiến vào góc cận trong 0,5 s.
   - Khi camera đã vào xong, nếu E vẫn đang giữ từ lần bấm mở thì phải nhả rồi bấm lại mới nắm được.
2. **Gỡ chìa.** Giữ E và kéo chuột chậm theo hướng gợi ý.
   - Hướng có ở hai chỗ: chữ trên dòng nhắc (lên / sang phải / xuống…) và một mũi tên nhỏ vẽ bằng line cạnh vòng chìa.
   - Ba giai đoạn, mỗi giai đoạn 4 s hợp lệ, tổng 12 s:

     | Giai đoạn | Hướng kéo | Đạo cụ làm gì |
     |---|---|---|
     | 1. Nâng | lên | Chùm chìa nâng lên và nghiêng quanh đáy móc |
     | 2. Gỡ khỏi móc | chéo, hướng ra ngoài | Dây vòng trượt lên và qua đầu móc chữ J |
     | 3. Rút ra | về phía Kiều và xuống | Chùm chìa rời móc, ra khỏi khung hình |

   - Hướng kéo trên màn hình được tính từ hướng chuyển động 3D của đạo cụ chiếu lên camera, nên chữ, mũi tên và hình luôn khớp nhau.
3. **Chỉ những gì được tính.** Chỉ tính thời gian thở ra mà E đang giữ và chuột đang kéo đúng hướng, với tốc độ 0,02–0,45 chiều cao màn hình mỗi giây (đã lọc khoảng 0,1 s).
   - Không tính: giữ yên, kéo sai hướng, giật nhanh, và thời gian chờ.
   - Nhả E rồi nắm lại thì bộ lọc kéo đặt lại từ đầu; tiến độ vẫn giữ.
4. **Luật nhịp thở giữ nguyên §16.** Tay đang ở chìa khi bà hít vào thì bị bắt, gồm các trường hợp:
   - giữ E vắt qua ranh giới, kể cả giữ yên không kéo;
   - bấm nắm trong lúc bà hít vào;
   - bà đang cựa mình hoặc thức;
   - frame ranh giới mà mắt rời chìa hoặc lùi ra.
5. **Nhả E** giữ nguyên góc cận và tiến độ. **S** lùi ra: về góc mắt, giữ tiến độ, bấm E lại thì vào tiếp.
   - **Pause / menu** chỉ nhả tay; góc cận và tiến độ chờ người chơi quay lại.
   - **Truyện lấy quyền điều khiển** (hội thoại): thoát góc cận, không bị bắt.

### 18.2 Camera

- **Góc cận:** `ThirdPersonCameraController.SetCloseView` / `ClearCloseView`, chuyển mượt trong 0,5 s, kiểm tra vật cản trên đường chuyển.
  - FOV 60°, lúc thoát trả về FOV của người chơi (không hard-code 72°).
  - Chuột không xoay đầu khi đang ở góc cận.
- **Vị trí** (`KeyTheftView.Begin`): đặt cạnh hông bà, lệch về phía chân, cao hơn chìa, nhìn dọc theo người bà.
  - Chìa ở tiền cảnh; ngực và mặt bà vẫn trong khung để đọc hơi thở.
  - Nếu vị trí vướng sập, thân người hay tường thì kéo dần về phía chìa cho tới khi trống.
  - Không dịch chuyển nhân vật: luật tầm tay và tầm bắt vẫn tính theo vị trí thật của Kiều.
- **Khi bị bắt:** góc cận kết thúc trước, không di camera. `CatchScene` lấy camera từ đúng chỗ đó (controller bị tắt ngay trong khung), nên không có khung nào camera tự do giữa hai cảnh. Sau checkpoint, camera, FOV và quyền điều khiển trở lại bình thường.
- **Giữ Kiều đứng yên:** dùng cờ mới `PlayerMovement.HeldStill` (chỉ chặn đi lại, vẫn cúi được), không dùng `ControlLocked`.
  - Lỗi đã gặp: dùng `ControlLocked` làm pause menu tưởng là đang hội thoại, nên sau khi tiếp tục không trả điều khiển, Kiều bị khóa mãi.
- **Không mất mục tiêu:** chùm chìa được ghim làm mục tiêu tương tác (`PlayerInteraction.Pinned`) trong suốt minigame.
- **Hơi thở nhìn thấy được:** ngực Tú Bà phồng lên khi hít vào, xẹp xuống khi thở ra (1,6°), chạy theo cùng đồng hồ với tiếng ngáy, nhãn và luật.

### 18.3 Đạo cụ

- `LauTuBaKeyRing` tách mesh cũ thành hai phần:
  - **`LTB_KeyHook`:** móc cố định trên người bà. Gồm khoen quanh dây lưng, cổ móc, và móc chữ J hở 250°, miệng mở lên phía ngoài.
  - **`LTB_KeyBunch`:** vòng và 5 chìa, xoay quanh đáy móc, là phần lấy đi.
- Lấy xong thì chỉ phần chùm chìa biến mất; móc vẫn ở trên người bà, kể cả sau checkpoint.
- `StoryPropKit.KeyRing` của các chương khác không đổi. Mesh `LTB_KeyRing` cũ vẫn để nguyên (không xóa asset).
- **Ai điều khiển tư thế:**
  - Trong minigame, `KeyTheftView` điều khiển tư thế chùm chìa và tạm tắt `HangingKeys` (không ghi đè nhau mỗi frame).
  - Khi nhả E, chùm chìa về chỗ tựa thật của giai đoạn đang làm: giai đoạn 1 về lại chiếu, giai đoạn 2 treo trên móc, giai đoạn 3 nằm trên chiếu.
  - Khi thoát minigame, `HangingKeys` chạy lại.
- **Âm thanh:** `keys_soft` khẽ khi kéo đúng, và rõ hơn một chút khi qua giai đoạn mới. Không phát ra `Noise`.

### 18.4 Kiểm thử

- **Cách thử:** bài `-autopilotKeys` (`Chapter04LauWorld.KeyProbe.cs`) đi qua luồng input thật.
  - `InputReader` có thêm `SimulatedLook` và `SimulatedMove` (chỉ dùng cho QA, giống `SimulatedInteract`).
  - `PlayerInteraction` và camera nhận E, chuột, S như từ người chơi. Không gọi thẳng `KeyTheft.Tick` hay `TookKeys()`.
- Các autopilot thường cũng đi qua minigame: vào góc cận, giữ E, kéo chuột.

**Kết quả** (Editor, scene Ch4 dựng lại): **PASS 57/57**.

| Ca | Kết quả |
|---|---|
| Bấm E lúc bà hít vào để vào minigame | Vào xong sau 0,50 s, không bị bắt, chưa nắm |
| Bố cục góc cận | Chìa ở (0,43–0,45; 0,45) của khung, cách camera 0,48 m. Ngực ở (0,48; 0,63), cách 0,80 m. Mặt ở (0,59–0,62; 0,69), cách 0,91 m. FOV 60° (của người chơi 72°). Camera không nằm trong vật nào. Lần đầu đặt cách chìa 0,66 m thì chìa quá nhỏ (xem ảnh), đã kéo camera gần lại |
| E còn giữ từ lần bấm mở, kéo đúng qua một hơi thở ra | Không tính (0,00 s) |
| Di chuột khi đang ở góc cận | Camera xoay 0,00° |
| Một hơi thở ra kéo đúng, nhả trước khi hít vào | +2,03 s, giữ nguyên qua lúc hít vào |
| Giữ E không kéo / kéo ngược hướng / giật nhanh | Lần lượt 0,00 / 0,00 / 0,02 s |
| Nhả rồi nắm lại trong cùng một hơi thở ra | Cả hai đoạn đều được tính (+2,01 s) |
| Pause menu rồi tiếp tục | Vẫn ở góc cận, tiến độ giữ nguyên, tay đã rời |
| Lùi ra bằng S, rồi vào lại | Về góc mắt, FOV 72°, Kiều đi lại được, tiến độ giữ nguyên; vào lại được |
| Truyện lấy quyền điều khiển khi đang nắm | Thoát góc cận, không bị bắt, không cộng tiến độ |
| Giữ yên vắt sang hít vào; bấm lúc hít vào; bấm khi bà cựa mình; khoảng 8 FPS; một khung 0,3 s; gần xong (11,3 s) rồi giữ sang hít vào | Mỗi ca đúng một cảnh bắt Front, camera chuyển thẳng sang cảnh bắt, một lần thất bại, không nhận chìa, khung bị bắt không cộng gì (0,000 s). Sau checkpoint: tiến độ về 0, đã thoát góc cận, FOV 71,9–72°, quyền điều khiển trả lại |
| Bị bắt khi camera mới vào được 41% | Cảnh bắt nhận camera, sau đó mọi thứ trở lại bình thường |
| Khoảng 8 FPS, kéo đúng | Cộng đúng bằng thời gian thở ra lúc kéo đúng (2,01 s) |
| **Làm đúng từ 0** | **12,00 s hợp lệ qua 6 hơi thở ra, tổng 23,2–24,0 s** qua các lần chạy (tính cả lúc vào minigame và lúc chờ). Lấy được đúng một lần; móc vẫn trên người bà; về góc mắt, FOV 72° |
| Sau khi lấy: E vào móc; checkpoint sau khi lấy | Không vào minigame, không bị bắt. Chìa vẫn đã lấy, móc vẫn hiện, bà ngủ tiếp |

**Lỗi tìm ra khi kiểm thử (đã sửa):**
- Dùng `ControlLocked` để giữ Kiều đứng yên thì sau khi pause rồi tiếp tục, Kiều bị khóa mãi (lý do ở 18.2). Đã chuyển sang `HeldStill`.
- Bộ lọc kéo còn giữ giá trị từ lần nắm trước, nên giữ yên vẫn được cộng 0,18 s. Đã đặt lại bộ lọc mỗi lần nắm mới.
- Tiếng ngáy còn phát thêm đúng một frame khi một cảnh bắt bắt đầu (13 frame qua 13 ca bắt). Đã thêm sự kiện `CatchScene.Started` để `TuBaSleeper` tắt tiếng ngay trong frame đó.

**Hồi quy** (Editor, source cuối):

| Bài | Kết quả |
|---|---|
| `-autopilotKeys` | PASS 57/57 |
| Ch4 thường, Ch4 thorough (J1, J2, nhánh Lam, hợp đồng, cửa sau, checkpoint) | PASS; autopilot lấy chìa qua minigame trong 22,8 s và 24,9 s |
| Âm thanh Tú Bà (`-soundOnly tuba`) | PASS; cửa xa chỉ làm bà thức nằm nghe |
| Bắt (`-autopilotCatches`) | PASS: 13 ca và ngắt giữa chừng. Không lần nào ngáy khi không nằm ngủ (sau khi sửa) |
| Lén lút | PASS 5/5 |
| Đuổi (`-autopilotChase`) | PASS 11/11 |
| Kiểm tra bản đồ | PASS |
| Ch3 thường (sau các sửa ở camera, `PlayerMovement`, `InputReader` dùng chung) | PASS |

**Bản exe:** build Success từ **source cuối**, đã có mọi sửa ở trên. Chạy `-autopilotKeys` 2 lần ở 1920×1080:
- Cả 2 lần PASS 57/57.
- Làm đúng từ 0: tổng 24,0 s và 23,9 s.
- Sau J2: không ngáy.
- Trung bình 135–137 fps, p99 13,9 ms, 0,2 % số khung dưới 60 fps.
- Khung tệ nhất khoảng 310 ms là khung cố ý ngủ 0,3 s trong bài.

**Ảnh QA** ở `Tools/LauTuBa/Views/keys17/`, là ảnh tĩnh, không phải clip:
- `KEYS_close_view`;
- `KEYS_close_stage2` và `KEYS_close_stage2_rest`, `KEYS_close_stage3_rest`;
- `KEYS_catch`.

Ảnh `KEYS_close_stage2`, `KEYS_close_stage2_rest` và `KEYS_close_stage3_rest` được chụp sau khi đã kéo camera gần lại; `KEYS_close_view` mới là ảnh trước khi chỉnh hướng nhìn cuối cùng.

**Lần chạy có quay ảnh bị treo một lần:** đồng hồ game dừng sau bài "roused", có thể do lỗi khóa điều khiển ở trên. Sau khi sửa thì chạy lại 2 lần đều PASS. Các vòng chờ của bài thử nay có bộ canh theo thời gian thật: nếu đồng hồ game dừng quá 3 s thì ghi lại ai đang giữ và đi tiếp, không treo nữa.

### 18.5 Chưa kiểm chứng

- **Chưa chơi tay**, nên chưa biết:
  - cảm giác kéo chuột có dễ hiểu không, tốc độ hợp lệ 0,02–0,45 chiều cao màn hình mỗi giây đã hợp với chuột thật và DPI thật chưa;
  - mũi tên và chữ hướng có đủ rõ không;
  - 23 s có đủ căng thẳng không.
- **Bố cục** mới đo bằng số (vị trí trong khung) và xem qua ảnh QA; chưa xem trong game thật. Chưa thử cả các góc tiếp cận khác ngoài vị trí bài thử đứng.
- **Đạo cụ:**
  - Đường gỡ khỏi móc là chuyển động xấp xỉ, không mô phỏng va chạm thật giữa dây vòng và móc.
  - Ở giai đoạn 3, khi nhả E, chùm chìa đặt lên chiếu theo độ cao ước tính.
- **Âm thanh:** chưa nghe tiếng chìa và tiếng ngáy.
- Lỗi chập chờn của đợt 15 vẫn mở.
- Mục 17 (đồng bộ âm Ch3) vẫn chưa kiểm thử xong.

### 18.6 File đã đổi

- **Mới:** `Gameplay/Chapters/LauTuBa/KeyTheftView.cs`.
- `Gameplay/Chapters/LauTuBa/KeyTheft.cs`: viết lại thành phiên minigame (vào, nắm, kéo, ba giai đoạn, lùi ra, pause).
- `Player/ThirdPersonCameraController.cs`: thêm góc cận (`SetCloseView` / `ClearCloseView` / `CloseWeight`). Không có góc cận thì hành vi cũ giữ nguyên.
- `Player/PlayerMovement.cs`: thêm `HeldStill`.
- `Gameplay/Stealth/CatchScene.cs`: thêm sự kiện `Started`.
- `Core/InputReader.cs`: thêm `SimulatedLook`, `SimulatedMove` (chỉ cho QA).
- `Gameplay/Chapters/LauTuBa/TuBaSleeper.cs`: ngực phồng xẹp theo nhịp thở.
- `Gameplay/Chapters/LauTuBa/HangingKeys.cs`: tính tầm với từ cả móc và chùm chìa.
- `Core/Editor/LauTuBaKeyRing.cs`: tách móc và chùm chìa.
- `Core/Editor/LauTuBaSetup.cs`: thêm `KeyTheftView`, `_bunch`, `_player`.
- `Core/Editor/LauTuBaKit.cs`: thêm `UnlitMat`.
- `Chapter04LauWorld.KeyProbe.cs` và `SoundProbe.cs`: các bài thử mới.
- **Asset mới:** `LTB_KeyHook`, `LTB_KeyBunch`, `LTB_PullHint`.
- Scene Ch4 dựng lại bằng `-onlyChapter4`.

## 19. Lính gác, dụ bằng sỏi, phòng Kiều an toàn, cửa và tầm nhìn (21/10/2026)

Thực hiện theo `docs/concept/CLAUDE_CH4_GUARDS_DISTRACTION_REVISION.md`. Chỉ Ch4. Giữ minigame lấy chìa §18, Foley bước chân/cửa đã duyệt, J1/J2, nhánh Lam, checkpoint, catch scene. Không quay clip, chưa commit.

### 19.1 Hai giai đoạn của lính

| | Lính 1 (`NguoiCanh`) | Lính 2 (`NguoiCanhCong`) |
|---|---|---|
| Trước báo động | Đứng gác chếch cửa phòng Tú Bà: (2,6; 9,6), nhìn hướng 262° dọc hành lang về phía dãy Tây; tại chỗ đảo đầu ±50° | Gác cổng (2,4; −17,9) |
| Sau báo động J2 | Tuần hành lang: (−4; 9,7) nhìn Tây 3 s → cửa Tú Bà 2 s → (5,5; 9,6) → cửa phòng sổ sách 3 s → xuống hiên Đông (10,5; 5) 2 s | Cổng 3 s → sảnh 2 s → hiên Nam (−6; −7,5) → hiên Tây (−10,5; −3) 2 s → góc Tây Bắc 3 s → lối bếp (−15,5; 9,6) 2 s → (−10,5; 0,5) → về cổng |

- `Chapter04LauWorld.SetRoles(alarm, onFoot)` đặt cả hai cùng lúc: tuyến, ai gọi ai (`Watchman.Ally`, `CallsAlly`).
- Đổi sang tuần tra **chỉ ở đúng lúc Tú Bà gào "Thằng Tư! Chìa của tao đâu?!"** trong `SecondScare`, không phải lúc `Taken`.
  - Dùng `Watchman.Rejoin`: hai lính đi bộ từ chỗ đang đứng tới điểm gần nhất của tuyến mới, không teleport.
  - Một cuộc đuổi đang diễn ra vẫn tiếp tục.
- Sau báo động, lính nào thấy Kiều và chuyển sang đuổi thì gọi người kia tới **chỗ mình vừa thấy** (`Alarm(LastKnown)`, qua `SafeZone.Porch`). Người được gọi chỉ đi kiểm tra chỗ đó; muốn đuổi phải tự thấy.
- Checkpoint (`OnRestored`, `ArmGuard`): đặt lại vai trò theo `J2Done` của snapshot, lính ở điểm của tuyến (trước báo động: tại chỗ gác).
- J1 không đổi: kịch bản vẫn đưa lính tới cửa phòng sổ sách; xong thì lính quay về chỗ gác.
- `TuBaAtKieuDoor`: Tú Bà đứng gào **ngoài** cửa phòng Kiều 12 s (trước là 25 s) rồi ra sảnh. Bà không vào, không mở cửa phòng.

### 19.2 Sỏi

- Q ném, giữ input cũ. Mang tối đa 3 viên (`PebbleThrow.Capacity`), bắt đầu đêm với 3 viên.
- **Đống sỏi** (`PebblePile`) đặt sát tường cạnh cửa phòng Kiều, tại (−11,55; 6,45): một đĩa đất nung chất sỏi xám.
  - E nhặt đầy lại 3 viên, lấy bao nhiêu lần cũng được.
  - Dòng nhắc: "Nhặt sỏi (n/3 · Q: ném để dụ lính)".
  - Lần nhặt đầu có một câu nội tâm hướng dẫn.
  - Tutorial và dòng điều khiển ghi "Q: ném sỏi để dụ lính".
- **Quỹ đạo thật** (`PebbleThrow.Fly`):
  - Tính đường cong đi qua điểm ngắm trong T = khoảng cách/12 s (0,25–1 s).
  - Mỗi frame mô phỏng 2 nửa bước, `SphereCast` bán kính 3 cm vào lớp Environment.
  - Gặp cửa, cột, mái, tường trước điểm ngắm thì dừng tại đó. Không bao giờ bay xuyên rồi phát tiếng phía sau.
- **Một tiếng cho AI mỗi viên**: lần chạm đầu tiên.
  - `Noise.Emit(Thrown)` tại điểm chạm, lùi 6 cm ra khỏi bề mặt về phía người ném. Vẫn chịu tường che (×0,6) và mưa ở sân (×0,8).
  - Bán kính = 10 m × vật liệu × lực:
    - vật liệu: đá ×1, gỗ ×0,8, mềm (chiếu, đất, bùn) ×0,38;
    - lực: 0,6–1 theo tốc độ đâm vào bề mặt (đủ lực ở 5,5 m/s).
  - Nảy 2–6 lần, mỗi lần một take riêng tại chỗ chạm, nhỏ dần. Các lần nảy **không** phát Noise.
  - `GuardPatrol.Investigate` của các chương cũ được gọi một lần ở cùng điểm.
- **Lính tới chỗ viên sỏi rơi**, không tới chỗ người ném. Khi đang đuổi, tiếng sỏi bị bỏ qua: không đổi `LastKnown`, không hủy Chase.
- **Dụ**:
  - Lính nghe tiếng sỏi đủ lớn → quay lại, nói, đi tới, nhìn 5 s.
  - Sau đó nhìn quanh 8 chỗ trong bán kính 3 m quanh điểm rơi, mỗi chỗ 3,5 s (thời gian tìm tối đa 56 s), rồi đi về chỗ gác.
  - Giới hạn: sau 3 lần bị dụ trong 300 s, viên tiếp theo chỉ làm lính quay đầu (Suspicious), không rời chỗ.
  - Cửa sổ 300 s dài hơn ba lượt dụ (mỗi lượt khoảng 60 s). Với 180 s, viên thứ tư lọt ra ngoài cửa sổ và lại dụ được lính.
- **Tiếng thật**: `pebble_hard`, `pebble_wood`, `pebble_soft` trong `Ch4FoleyBank`, dựng bằng `Ch4Pebble`. Nguồn và giấy phép xem `Assets/_Game/Audio/Ch4Foley/CREDITS.md`.
  - Gỗ và mềm chỉ là bản tạm thay, chưa có bản thu đúng.
  - Ch3 không nhận các nhóm này.

### 19.3 Phòng Kiều là vùng an toàn (`SafeZone`)

- Vùng là hộp đúng bằng phòng Kiều (x −20,95…−12,05; z −0,45…7,95). "Hiên" (porch) là `KieuDoorOutside`.
- Một luật dùng chung cho mọi watcher:
  - Kiều trong phòng thì: không bị thấy, không bị bắt, Chase chuyển thành Search tại hiên.
  - Mọi điểm đi tới nằm trong phòng (tiếng, chỗ thấy, chỗ tìm, đi theo kịch bản, đường đang đi) đổi thành hiên.
  - Chỗ tìm kiếm không chọn điểm trong phòng; chỗ nấp trong phòng không bị lục.
  - Cửa phòng Kiều `_npcCanUse = false`.
- **Ngưỡng có trễ**:
  - Vào an toàn khi chân Kiều đã qua mép trong 0,35 m (cả thân đã qua đường cửa).
  - Hết an toàn khi đứng lại lên mép (x > −12,05).
  - Đứng ngay khung cửa không nhấp nháy giữa hai trạng thái.
  - Hành lang ngoài cửa không được miễn.
- Tiếng trong phòng dẫn lính tới hiên. Viên sỏi ném từ trong phòng ra ngoài được nghe tại chỗ rơi.
- Checkpoint: `SafeZone.Refresh()` sau khi đặt Kiều.

### 19.4 Lính tự mở/đóng cửa (`DoorUser`, `HouseDoor.OpenBy/CloseBy`)

- Cửa đóng khoét NavMesh nên đường đi qua nó bị "partial".
- `DoorUser` (trên hai lính) tìm cửa:
  - cửa nằm giữa lính và đích, lính được phép mở (`NpcMayOpen`: không khóa, Gate không chặn, không phải cửa phòng Kiều, Lam, Hồng, cổng hay cửa sau);
  - phía gần lính phải đi tới được trọn vẹn.
- Lính đi tới, dừng, quay vào cửa, đẩy mở:
  - 1,1 s và `door_creak_soft`; cửa trượt là `slide_fast` khẽ;
  - đang đuổi thì xô 0,4 s với `door_fast`.
- Chờ NavMesh mở chỗ (0,65 s), rồi đi tiếp tới đích cũ. Trong lúc đó `HeldAtDoor` giữ bước chân và quyết định; mắt và tai vẫn chạy.
- Tuần tra hoặc về tuyến: qua cửa rồi khép lại sau lưng (`latch_drop` khẽ ở cuối), trừ khi có Kiều hoặc người khác trong vòng 1,4 m.
  - Đang khép mà có người bước vào thì mở lại.
  - Đang kiểm tra hoặc đuổi thì để cửa mở.
- Một tay mỗi cửa (`HouseDoor.User`): lính thứ hai chờ người kia mở xong rồi đi qua.
- Cửa khóa vẫn là tường: lính dừng ở chỗ gần nhất đi được.
- Không gọi `SetOpen` trần, không dùng input E. Tiếng cửa nghe được nhưng **không phát Noise**: lính không tự điều tra tiếng cửa của mình hay của đồng đội.
- `MovedByNpc`: các handler cửa Kiều, cửa Lam và cửa sau bỏ qua bước do lính.
- Checkpoint: `HouseDoor.Load` xóa người đang giữ cửa; `Watchman.Forget` hủy việc đang làm ở cửa.

### 19.5 Tầm nhìn

- **Tái hiện lỗi**: probe đo cùng rig, cùng chỗ, cùng ánh sáng với thông số chung cũ (nửa góc 40°, ngoại vi 70°/4 m nửa tiêu điểm, sàn gần chỉ ở góc trước, không cảm giác chạm).
  - Kiều đứng/cúi **ngay cạnh** (0,5–4 m, góc 90°): lính **không bao giờ** nhận ra trong 3 s.
  - Phía trước 0,5–2 m: 1,3 s.
  - Rig không có lỗi: mắt cách đầu 0,05–0,18 m, hướng mắt trùng hướng thân, đèn lồng và thân không chặn tia nhìn.
- **Ch4** (tùy chọn riêng trên `Watchman`, mặc định 0 = như cũ, nên Ch3 không đổi):
  - nửa góc trước 60°; ngoại vi 90°/4 m;
  - sàn gần 1,4 trong 2,1 m đo theo mặt sàn, áp cho cả trước và bên;
  - vùng gần rộng thêm 10° qua đường vai, để "đứng ngay cạnh" không rơi vào kẽ hở do vị trí mắt;
  - cảm giác chạm: khoảng cách bằng bán kính hai thân (capsule của lính + CharacterController của Kiều + skin + 0,12 m = 0,70 m), không xuyên tường, cooldown 2 s.
    Không dùng con số cố định 0,68 m: Kiều bị đẩy trượt quanh thân lính trước khi tới gần đến mức đó, nên lính thấy Kiều từ bên cạnh thay vì cảm thấy bị chạm.
    Khi chạm, mức nhận biết được đặt cao hơn ngưỡng kiểm tra một chút, để phần giảm dần trong cùng frame không kéo nó xuống dưới ngưỡng.
  - `SightModel` có thêm `FlatDistance`, `NearSideMargin` và các trường ngoại vi/gần.
- Sensitivity không đổi: lính 1 0,85 + 0,15 × mức cảnh giác, lính 2 là 1.

### 19.6 Kiểm thử trên scene thật: `-autopilotGuards`

Probe chạy trên scene Ch4 thật, perception bật, Sensitivity của chương (không tăng). Báo cáo đầy đủ: `Tools/LauTuBa/QA/guards_ch4_final.txt`. Lần chạy cuối: **30/30 ok, autopilot PASS** (chương chạy tiếp tới cửa sau).

**Rig:** mắt cách đầu 0,07–0,16 m, hướng mắt trùng thân, không collider nào của lính nằm trên lớp chặn tầm nhìn.

**Bảng tầm nhìn** (s tới chắc chắn và đuổi; ">3" = không thấy trong 3 s). Đo trên sân, chỗ trống 4,5 m quanh lính. "Sáng" là đèn lồng của lính bật, "tối" là tắt (ánh sáng ≤ 0,32).

| Lính 1 | 0,5 m | 1 m | 2 m | 4 m |
|---|---|---|---|---|
| Cũ, trước, mọi trường hợp | 1,30 | 1,31 | 1,31–1,33 | 2,5 hoặc chỉ thoáng thấy |
| **Cũ, bên trái / phải, mọi trường hợp** | **>3** | **>3** | **>3** | **>3** |
| Ch4, trước | 0,25–0,26 | 0,84 | 0,84–0,85 | 2,5–2,6 (đứng) / thoáng thấy (cúi) |
| Ch4, bên trái / phải | 0,25–0,30 | 0,84 | 0,84–0,86 | >3 |
| Ch4, sau | cảm thấy chạm, quay lại, chắc chắn 0,62–0,66 | >3 | >3 | >3 |

- Lính 2 cho kết quả tương tự. Chậm nhất trước/bên trong 2 m: lính 1 0,86 s, lính 2 0,72 s.
- Đi thẳng vào lưng lính: không bị thấy trên đường đi; chạm ở 0,69 m thì lính phản ứng (0,95 s).
- Qua cửa đóng: không thấy. Cửa mở: thấy ngay, chắc chắn sau 2,2 s (2,5 m).

**Sỏi:**
- Đống sỏi + E lấp đầy 3 viên (đã đủ thì không lấy thêm). Q ném 3 viên, lần Q thứ tư không ném. Nhặt lại được.
- Ném vào điểm sau cửa kho đóng: chạm lá cửa, 1 tiếng, nghe ở phía Kiều.
- Ném qua cột hiên ra sân: chạm cột trước.
- Mỗi viên đúng 1 tiếng tại điểm chạm đầu, dù nảy 3–6 lần. Bán kính: đá ướt 7,8 m, ván 8,0 m, chiếu 3,8 m.
- Đang đuổi mà ném sỏi: lính vẫn đuổi đủ 2,5 s, vị trí cuối biết không nhảy về chỗ viên sỏi.
- Giới hạn dụ: 3 viên dụ được, viên thứ tư chỉ làm lính quay đầu.

**Dụ và đột nhập** (Kiều di chuyển bằng character controller, input thật):
- Từ chỗ tối (−3,5; 9,3) ném sỏi ra sân. Lính rời chỗ gác 1,6 s sau.
- Kiều nhón đi, chỉ bước khi lính không nhìn về phía mình:
  - tới cửa 3,6 s, mở hé cửa giấy (0,50), vào, khép lại sau lưng;
  - chùm chìa qua minigame thật lúc 37 s;
  - kéo then cửa thông, mở, sang phòng sổ sách lúc 52,7 s.
- Không bị bắt. Mức nhận biết cao nhất 0,29, dưới ngưỡng 0,30 làm lính quay đầu.
- Lính vắng **57,7 s** từ lúc rời chỗ gác tới lúc đứng lại tại đó (mục tiêu 45–60 s).

**Phòng Kiều:**
- Bị đuổi chạy vào qua cửa mở: không bị bắt, lính không bước vào, tìm ngoài hiên rồi đi.
- Tiếng động trong phòng khi cửa đóng: lính tới hiên, không mở cửa, không vào.
- Ngưỡng: −11,95 ngoài · −12,25 ngoài · −12,60 trong · −12,20 vẫn trong · −11,95 ngoài.
- Sỏi ném từ trong phòng ra hiên: lính tới chỗ sỏi rơi.
- Checkpoint đặt lại đúng.

**Cửa:**
- Cửa giấy Tú Bà đóng, không khóa: lính đi vòng tới cửa, mở bằng tay, vào sau 4,7 s, không có Noise.
- Cửa phòng sổ sách khóa: không mở, lính dừng bên ngoài.
- Hai lính cùng một cửa: cửa chỉ mở một lần, cả hai qua được.
- Tuần tra qua cửa: khép lại sau lưng; Kiều đứng ở khung cửa thì không đóng.
- Không làm nhảy bước truyện.

**J2 (thật, do autopilot chơi):**
- Cả hai lính giữ chỗ gác tới đúng lúc báo động, đổi tuyến 6,7 s sau khi J2 bắt đầu. Bước lớn nhất trong một frame 0,04 m, nên không có teleport.
- Hai lính gọi nhau. Tú Bà rời cửa phòng Kiều trong 15 s.
- Checkpoint trước báo động: về chỗ gác. Checkpoint sau báo động: vẫn tuần tra.

**Lỗi tìm ra khi chạy probe (đã sửa):**
- `NavMeshPath` tạo ở field initializer bị null.
- Đường partial dừng ở phòng bên cạnh cách một bức tường, nên lính không nhận ra cánh cửa trên đường.
- Phía bên cạnh đúng 90° rơi vào kẽ hở của mép cone.
- "Trong 2 m" bị đo theo khoảng cách 3D.
- Chạm bị mất vì mức nhận biết tụt dưới ngưỡng ngay trong frame.
- Một cái thoáng thấy (0,29) bị tính là "đã thấy", làm hỏng lượt tìm theo viên sỏi.
- Cửa sổ chống dụ lặp 180 s ngắn hơn ba lượt dụ.
- `Forget` không xóa thời điểm thấy cũ khi checkpoint.

### 19.7 Kiểm thử hồi quy

Chạy trong Editor sau thay đổi cuối:

| Kiểm thử | Kết quả |
|---|---|
| `LauTuBaChecks` (map) | PASS |
| `-autopilotKeys` | PASS, 57/57 |
| `-autopilotChase` | PASS, 11/11 |
| `-autopilotCatches` | PASS |
| `-autopilotSound -soundOnly tuba` | PASS, 10/10 |
| `-autopilotStealth` | PASS, 5/5 |
| Ch4 normal / thorough | PASS / PASS (J1, J2, Lam, cửa sau) |
| Ch3 normal / thorough | PASS / PASS |

- `-autopilotStealth` ca 1–2 viết cho tuyến cũ trước báo động. Bây giờ trước báo động lính chỉ đứng gác, nên hai ca này đổi sang tuyến sau báo động: đứng sáng cạnh tuyến (bị bắt sau 17,6 s), cúi trong tối cách đầu tuyến 6 m (không bị thấy). Các tiêu chí giữ nguyên.
- Ch3 không có `SafeZone`, `DoorUser` hay tùy chọn tầm nhìn Ch4, nên hành vi Watchman ở Ch3 như cũ.
  - Chỉ có hai thay đổi chung: `Forget` xóa thời điểm thấy cũ, và `HouseDoor` thêm `_npcCanUse` (mặc định bật, lính Ch3 không có `DoorUser` nên không dùng).
- Không chạy `-autopilotSound` đầy đủ (khoảng 25 phút) và chain. Bước chân và cửa không đổi.

### 19.8 Chưa xong / chưa kiểm chứng

- **Âm thanh:** chưa ai nghe.
  - `pebble_wood` và `pebble_soft` là bản tạm thay, chưa có bản thu thật của sỏi trên gỗ hay trên nền mềm.
  - Ba take đá từ Commons theo giấy phép CC BY-SA 4.0.
  - File `Source/pebble/Small_pebbles_on_concrete.ogg` lỗi import (Ogg Skeleton); đã xoá khỏi project ngày 22/10/2026 theo yêu cầu người dùng (bản gốc còn trong `FoleyDownloads/Pebble/`).
- **Chưa chơi tay.** Đường lén do bot đi theo quy tắc "chỉ bước khi lính không nhìn"; độ khó thật cần người chơi thử.
- **Cửa giấy trượt:** giữ E chỉ mở được khoảng 0,5–0,57, vì khi tấm giấy trượt đi xa thì Kiều mất mục tiêu tương tác. Khe vẫn đủ đi qua. Đây là hành vi sẵn có, chưa sửa.
- **Nội thất:** NavMesh không khoét đồ đạc, nên lính có thể đi xuyên bàn trà hay sập trong phòng. Không thuộc đợt này.
- **Exe chưa build lại.** Brief cấm build đầy đủ chỉ vì sửa source, nên exe trong `Builds/` là bản cũ, chưa có thay đổi §19.
