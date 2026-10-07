# Mở đầu, Chương 7, Kết, Tổng kết · Cài đặt · Ghi công — Tài liệu bàn giao

Game giờ chơi trọn một vòng: **Mở đầu → Chương 1 … 7 → Kết → Tổng kết → Ghi công → Menu chính**.

## 1. Chơi thử
- Menu chính → **Chơi mới**: bắt đầu từ Mở đầu (lớp học).
- **Chọn chương** liệt kê Mở đầu, Chương 1–7 và Kết.
- **Cài đặt** có trong menu chính, menu tạm dừng chung (Ch1) và menu tạm dừng truyện (Ch2–Kết).
- **Ghi công** có trong menu chính và tự chạy sau màn tổng kết cuối game.
- Dòng lệnh: `-chapter prologue`, `-chapter 7`, `-chapter ket` (thêm `-keepChoices` để dùng lựa chọn đã lưu).

## 2. Thiết kế đã hiện thực

### Mở đầu — Trang sách (≈ 6 phút) · `Chapter00_Prologue`
- **Lớp học** (buổi chiều, phía tây scene):
  - Mở đầu là tiết Văn đoạn Kiều bán mình. Nhân vật chính ngồi bàn thứ ba cạnh cửa sổ.
  - Cô giáo hỏi, người chơi trả lời bằng lựa chọn có giờ (`pl_che_kieu`: "dễ mà" / "phải có cách khác" / "Kiều cam chịu").
  - Chuông reo: cả lớp đứng dậy ra về.
  - Hub khám phá: đọc thơ trên bảng, nói chuyện với Ngân, nhìn ra cửa sổ, cất quyển sách (kết thúc hub).
  - Quyển sách phát sáng, giọng Kiều thật thách thức, rồi ánh sáng trắng.
- **Phòng Kiều** (buổi sáng):
  - Thẻ "MỞ ĐẦU · II — PHÒNG KIỀU". Nhân vật tỉnh dậy, màn hình hoảng loạn.
  - Hub (cần xem 2/3 vật mới ra sân được):
    - Soi gương đồng: thấy mặt Kiều.
    - Cây hồ cầm: câu thơ "Bạc mệnh".
    - Thư Kim Trọng: gieo mầm cho Chương 6.
- **Sân Vương gia**:
  - Bữa sáng yên lành.
  - Tiếng chiêng, quan sai xông vào. Người chơi chọn trong 9 giây (`pl_hanh_dong`): lao ra cản / hỏi tội danh / đứng nhìn ghi nhớ.
  - Vương Ông ngã (clip "Dying" của rig), cha và em bị lôi ra cổng.
  - Chiều đổ mưa, Kiều thật nói. Chuyển sang Chương 1 (đêm mưa).

### Chương 7 — Nghịch Mệnh (≈ 10–14 phút) · `Chapter07_HuyenDuong`
- **Sân ngoài huyện đường**: hub chuẩn bị, bao nhiêu việc tùy người chơi.
  - **Động viên nhân chứng** (`c7_nhan_chung_vung`): không ai lùi bước khi Tú Bà dọa.
  - **Bàn kế với Kim Trọng** (cần `kt_tin_tuong ≥ 3`): chàng cứu nguy **một lần** khi quan hết kiên nhẫn.
  - **Để ý Tú Bà**: thấy mụ đút lót nha dịch, có thêm bằng chứng "Túi gấm đổi tay".
  - **Đánh trống kêu oan**: vào công đường.
- **Đối chất** (`TrialSequence` + `TrialView`), 3 vòng: Tú Bà (trộm bạc), Mã Giám Sinh (cưới hỏi), Tú Bà (món nợ).
  - Mỗi vòng có 4 lời khai.
    - **Q: hỏi vặn**: người khai nói thêm, có khi tự sửa lời. Ví dụ "năm trăm lạng" thành "bốn trăm năm mươi lạng", mở ra mâu thuẫn mới.
    - **E: đưa bằng chứng** lấy từ hồ sơ Chương 6 (khế ước, văn tự, sổ nợ, túi bạc, sổ hiệu bạc, mẩu giấy, nhân chứng, lời Kim Trọng…).
  - Lòng tin của quan bắt đầu từ 60%.
    - Bằng chứng đúng: +15% mỗi điểm.
    - Bằng chứng sai: −20%.
    - Về 0%: Kim Trọng cứu (nếu đã bàn kế), không thì thẻ thua rồi làm lại vòng đó với 50%.
  - Bác bỏ **câu then chốt** là qua vòng. Mọi câu then chốt đều giải được bằng giao kèo Mã–Tú Bà (Chương 4 luôn có), nên hồ sơ mỏng vẫn qua được.
  - Hồ sơ dày mở thêm các mâu thuẫn phụ, cộng điểm `c7_diem`.
  - Sau vòng 2, Tú Bà quát dọa. Nếu chưa động viên nhân chứng, một người lùi bước và rời công đường (`c7_lui`), bằng chứng của họ biến mất ở vòng 3.
- **Phán quyết**, luôn thắng nhưng khác sắc thái (`c7_ket`):
  - 3 – trọn vẹn: `c7_diem ≥ 7` và hồ sơ ≥ 3. Lầu xanh niêm phong, đốt văn tự, tìm lại ba cô gái.
  - 2 – ấm áp: `c7_diem ≥ 4`.
  - 1 – chật vật: Kim Trọng (hoặc bà hàng nước) đứng ra bảo lãnh phút chót.
- **Đoàn tụ** ở cổng: Vương Ông vẫy tay (clip "Waving"), Vương Quan, Mẹ Kiều.
- **Kiều thật lần cuối**: mọi người đứng im. Bóng Kiều vàng trong suốt đứng đối diện, lời thoại đúng tài liệu. Sau đó ánh sáng trắng.

### Kết — Trở lại lớp học (≈ 3 phút) · `Chapter08_Epilogue`
- Lớp học lúc hoàng hôn, mười lăm phút sau giờ tan học.
- Ngân quay lại lấy áo. Đọc lại đoạn bán mình (trang sách).
- Hội thoại đúng tài liệu ("Nãy mày nói nếu là Kiều…"), câu của Ngân đổi theo `pl_che_kieu`.
- Gấp sách: dòng chữ **"Cảm ơn."** hiện dần trên trang. Tiếp theo là thẻ "HẾT — THÚY KIỀU: NGHỊCH MỆNH".
- **Tổng kết hành trình** (`RecapView`): sắc thái kết và các lựa chọn từng chương, từ Mở đầu đến Chương 7.
- Sau đó là **Ghi công** rồi về menu chính. Hoàn thành game sẽ mở khóa toàn bộ "Chọn chương".

### Hệ thống
- **Cài đặt** (`SettingsView`, lưu ở PlayerPrefs qua `GameSettings`), 4 thẻ:
  - **Âm thanh**: tổng / nhạc / hiệu ứng / môi trường / giọng.
  - **Thoại**: tốc độ chữ 5 mức, bật/tắt lồng tiếng, tự chuyển câu (đợi hết giọng đọc rồi dừng để đọc).
  - **Hình ảnh**: độ phân giải, toàn màn hình/cửa sổ, chất lượng, V-Sync.
  - **Điều khiển**: độ nhạy chuột, đảo trục Y, rung màn hình (tắt/nhẹ/đầy đủ).
- **Ghi công** (`CreditsView`): tên và vai trò 6 thành viên (theo `Doc/KeHoach_ThietKe_ThuyKieu_Unity.md`), nguyên tác Nguyễn Du, nguồn tài nguyên (Unity, Ink/MIT, Poly Haven/CC0, Mixamo, Edge TTS, âm thanh procedural).
- **Sổ tay manh mối** lật trang được (←/→), mở ở trang mới nhất.

## 3. Kiến trúc (bổ sung)
- **Ink**:
  - `Prologue_TrangSach.ink`, `Chapter7_NghichMenh.ink`, `Epilogue_LopHoc.ink`.
  - Biến `pl_*`, `c7_*`, `c6_so_bac` trong `Main.ink`.
  - Gốc của story vẫn là `chapter_1`, nên Chương 1 không đổi. Mở đầu vào bằng knot `prologue`.
- **Đánh số**: Mở đầu là chương 0, Kết là chương 8. `ChapterCatalog.Label` cho ra "Mở đầu" / "Chương N" / "Kết".
- **Nối Mở đầu → Chương 1**: `Chapter01Director` nạp trạng thái Ink nếu có (`TryGetStateFor(1)`), nên Chương 1 chơi từ chọn chương vẫn như cũ.
- **Hai thân thể cho người chơi**: `PlayerLook` đổi qua lại giữa Kiều và nữ sinh. `PlayerMovement.SetAnimator` và `StoryActor.SetAnimator` chuyển animator theo.
  - Thay model nữ sinh thật: sửa `ClassroomKit.StudentModel` / `StudentTextures` rồi dựng lại scene.
- **Model tạm**: nữ sinh dùng rig lầu xanh 1. Ngân dùng lầu xanh 3. Cô giáo dùng Mẹ Kiều đổi màu. Bạn học và Vương Quan dùng các rig có sẵn đổi màu.
- **Trình tự mới** (`Gameplay/Sequences`):
  - `TrialSequence`: đối chất.
  - `ProcessionSequence`: nhiều nhân vật đi tới điểm đánh dấu.
  - `LightSequence`: ánh sáng trắng.
  - `RevealSequence`: chữ hiện dần.
  - `EndingSequence`: thẻ HẾT, tổng kết, ghi công.
  - `WorldBeatSequence`: chuyển cảnh lớp học sang phòng Kiều.
- **Thế giới**: `PrologueWorld`, `Chapter07World`, `EpilogueWorld`.
- **Editor**:
  - `PrologueSetup`, `Chapter07Setup`, `EpilogueSetup`, `ClassroomKit` (lớp học dùng chung).
  - `StoryBuild.Rebuild` dựng Mở đầu, Ch2–7 và Kết, đồng thời xóa placeholder Chương 7 cũ.
- **Hoạt ảnh**: `StoryAnimationSetup.CharacterController` nhận thêm clip `-Waving` (cử chỉ 3) và `-Dying` (cử chỉ 4, emotion `collapse`).
- **Âm thanh procedural mới**:
  - Nhạc: `classroom`, `morning`, `trial`, `family`, `spirit`, `epilogue`.
  - Nền: `classroom`, `classroom_evening`, `courtyard`, `yamen`, `yamen_hall`.
  - Hiệu ứng: `school_bell`, `drum`, `gavel`, `objection`, `wrong`.

## 4. Dựng lại scene & kiểm thử
- **Dựng lại tất cả scene sinh bằng code**: `-executeMethod ThuyKieu.Core.Editor.StoryBuild.RebuildBatch` (hoặc menu ThuyKieu ▸ Story). Dựng Mở đầu, Ch2–7, Kết; Chương 1 (dựng tay) không bị đụng tới.
- **Kiểm tra kịch bản**: `InkStoryValidator.RunBatch`, 9 điểm vào × 1.500 lượt. Có thêm bài kiểm tra điểm nối Mở đầu → Chương 1.
- **Autopilot**:
  - Các cờ dùng chung: `PlaytestBatch -playScene <scene> -storyAutopilot -autopilotChain [-autopilotThorough] [-autopilotCapture <thư mục>]`.
  - Mở đầu chạy riêng. Chương 1 kiểm bằng `Chapter1RegressionBatch`.
  - Ch2 → Kết chạy nối liền một mạch.
- **Chụp màn hình menu từ bản exe**: `ThuyKieu.exe -shellPreview <thư mục>` chụp menu chính, 4 thẻ Cài đặt, Chọn chương và Ghi công, rồi tự thoát.
- **Lồng tiếng**: `VoiceManifestExporter.RunBatch`, rồi `python Tools/Voice/generate_voice.py`.

## 5. Giới hạn & việc chờ
- Chưa có model người hiện đại. Lớp học đang dùng model cổ phục đổi màu (nữ sinh mặc áo lam). Khi có model, thay đường dẫn trong `ClassroomKit` và `PrologueSetup.Classmates`.
- Nhà họ Vương ở Mở đầu là scene dựng mới, không phải scene Chương 1 dựng tay, nên kiến trúc hai nơi không giống hệt nhau.
- Độ khó đối chất (lòng tin bắt đầu, mức trừ/cộng) chưa được chơi tay để cân chỉnh. Các hằng số nằm đầu `TrialSequence`.

## 6. Kết quả kiểm thử (2026-10-08)
- **Ink**: 9 điểm vào × 1.500 lượt — PASS. Điểm nối Mở đầu → Chương 1 đúng 50/50.
- **Hồi quy Chương 1**: 103/103.
- **Autopilot trong Editor**:
  - Mở đầu (thorough, seed 3): PASS.
  - Ch6 → Ch7 → Kết (thorough, seed 4): PASS. Mỗi vòng đối chất bị ép thua một lần, sau đó chơi lại và qua.
  - Ch2 → Kết nối liền (seed 5): PASS.
  - Ch7 với hồ sơ trống (chỉ có khế hôn ước và giao kèo): vẫn thắng, kết "ấm áp".
- **Bản Windows** (`../Builds/Windows/ThuyKieu.exe`, 622 MB):
  - Autopilot thorough Mở đầu và Ch7 → Kết: PASS, không lỗi.
  - Ảnh chụp menu, Cài đặt, Chọn chương, Ghi công hiển thị đúng.
  - "6 errors" trong báo cáo build vẫn là lỗi cũ: CoreTest và KieuHome thiếu prefab Player.
- **Lồng tiếng**: 751 câu (312 câu mới), 0 lỗi.
- **Hiệu năng** (`StoryPerfProbe`, 1080p, góc camera chính): Mở đầu 9,6 ms · Ch7 6,9 ms · Kết 7,0 ms, ngang các chương trước.
- **Chưa kiểm được bằng tay**: cảm giác đối chất (độ khó, nhịp đọc lời khai), tư thế ngồi của nhân vật trong lớp học, chỉnh độ phân giải trên máy thật.

## 7. Lượt chỉnh hình ảnh, camera và độ chân thật (P24)

### Camera hội thoại (dùng chung mọi chương Ch2 → Kết)
- **Cắt cảnh thay vì trượt**: khi đổi sang góc máy mới, camera cắt thẳng (`ThirdPersonCameraController`). Trước đây camera trượt qua phòng nên xuyên qua người, kẹt vào tường, có lúc ra ngoài trời. Chỉ những lần chỉnh khung nhỏ, không có vật cản ở giữa, mới còn trượt mượt.
- **Né người đứng chắn** (`DialogueCameraRig.Compose`):
  - Nếu có người đứng giữa ống kính và người đang nói, góc máy xoay quanh người nói để tìm góc trống.
  - Người vẫn còn chắn sẽ bị ẩn khỏi khung hình trong câu đó (vẫn giữ bóng đổ), giống cách game điện ảnh làm.
  - Góc đặt sẵn (`CAM_*`) cũng tự ẩn người đứng sát ống kính hoặc chắn người nói.
- **Đầu thật của nhân vật**: `StoryActor.HeadPosition` lấy xương đầu của rig. Người ngồi, quỳ, nằm được quay đúng mặt, không còn nhắm vào khoảng không cao 1,55 m.
- **Quay qua vai chỉ khi đứng gần** (< 4,2 m). Xa hơn thì quay cận mặt người nói.
- Người nói không có mặt trong cảnh: camera giữ trên Kiều đang nghe.
- **Điểm đứng khi tương tác** (`ExploreTarget._standPoint`): soi gương, nhìn ra cửa sổ, mở sách. Nhân vật bước tới đứng đúng chỗ, quay đúng hướng.

### Gương thật
- `PlanarMirror` + shader `ThuyKieu/StoryMirror`: camera phản chiếu qua mặt gương, cắt phần sau gương bằng mặt phẳng xiên. Gương đồng có màu đồng, hơi mờ, tối dần ra mép.
- Chỉ render khi gương nằm trong khung hình và camera ở gần (≤ 9 m).
- Ảnh QA cũng render lại gương theo kích thước ảnh chụp.
- Đặt gương mới bằng `StoryPropKit.Vanity` (bàn trang điểm có gương đồng).

### Mở đầu
- **Sửa lỗi gốc của nữ sinh**: thân thể nữ sinh trước đây bị đặt sai (cách lớp học hàng trăm mét, dưới đất), nên cảnh Kết không thấy nhân vật chính. Nguyên nhân là `StorySceneKit.Body` căn chỉnh model theo toạ độ thế giới; giờ căn chỉnh ở gốc toạ độ rồi mới gắn vào người chơi.
- **Lớp học**:
  - Nền gạch granito (texture sinh bằng code).
  - Cờ đỏ sao vàng, thời khóa biểu, lịch, ghế giáo viên, bình chữa cháy, chai nước trên bàn.
  - Ánh sáng quyển sách và chữ "Cảm ơn." chỉ hiện khi đến lượt.
- **Phòng Kiều**:
  - Bàn trang điểm với gương đồng thật. Soi gương là thấy mặt Kiều qua vai.
  - Cây hồ cầm đầy đủ (thân, cần, trục, phím, dây) dựa vách; xem đàn có tiếng gảy (`sfx dan`).
  - Lá thư Kim Trọng đọc được trên bàn viết, kèm bút, nghiên, giấy, bình hoa đào.
  - Khung thêu, tranh treo, kệ tường, chân nến, trần gỗ có xà.
  - Nắng xiên qua song cửa đổ bóng hoa văn xuống sàn, kèm bụi lơ lửng.
- Cánh cổng có va chạm để camera không dí vào gỗ.

### Chương 7
- **Kiều thật**:
  - Kiều quay về khoảng sân trống trước cổng, bóng Kiều hiện ở đó.
  - Kim Trọng lùi ra sau vai Kiều.
  - Góc quay ngang chọn phía không ai chắn và phông nền trống. Ai còn chắn thì ẩn.
  - Ánh sáng ấm, sương nhẹ.
- **Góc máy mới**: cổng huyện đường (trước đây nằm ngoài tường), nhân chứng, trống kêu oan, toàn cảnh công đường (trước bị cột che), cận cảnh quan huyện trên án thư (`CAM_CU_QuanHuyen`).
- **Công đường**:
  - Nắng sớm chiếu vào từ cửa trước, đèn phụ, đèn chiếu án thư.
  - Xà và đòn tay trần, khung bình phong sơn son viền vàng.
  - Biển "TÚC TĨNH / HỒI TỴ", chiếu quỳ, ghế thư lại, lư hương, đèn lồng, cờ phướn.
- Nhân chứng chưa được gọi mà lên tiếng "từ đám đông" (bà hàng nước) sẽ bước vào công đường.
- Chậu cây ở cổng dời khỏi chỗ cả nhà đoàn tụ.

### QA
- `-autopilotCaptureLines` chụp **mọi câu thoại** (đánh số `C7_L012_QuanHuyen.png`…), kèm log vị trí camera và đầu người nói. Dùng để rà góc máy.
