# Báo cáo KIỂM TRA MODEL NHÂN VẬT (đóng vai người chơi) — 25/10/2026

> **CẬP NHẬT FIX (tối 25/10):** đã sửa xong các mục in đậm bên dưới; chi tiết ở cuối file.

Nguồn: 183 ảnh chụp lượt chơi full-chain seed 21 + soi chuyên sâu 10 cảnh bằng MCP vision
+ kiểm tra cấu trúc rig 16 actor scene Mở đầu (Animator/Avatar humanoid, chiều cao).

## Kết luận nhanh

- **Không có model nào "bể" nghiêm trọng**: không vỡ mesh, không T-pose, không thiếu chân tay,
  không texture nứt ở bất kỳ chương nào. Rig 16/16 actor ở Mở đầu đều humanoid + có Animator Controller,
  chiều cao hợp lý (học sinh ngồi 1.23–1.34m, người lớn 1.53–1.80m).
- Phát hiện các lỗi **nhẹ – trung bình** về tư thế/lơ lửng/đứng im liệt kê bên dưới, kèm cách khắc phục.

## Lỗi theo từng chương

### Ch0 — Mở đầu (lớp học) · mức NHẸ

- **1 học sinh ngồi nghiêng đầu bất thường** — đầu gật ngang sát vai, lệch so với thân so với các bạn cùng hàng.
- *Khắc phục:* kiểm tra `StoryActor` của học sinh đó trong scene `Chapter00_Prologue` — có thể do
  pose ngồi được rotate tay (Twist/MaxGlanceYaw) hoặc bone Neck bị xoay trong pose dàn dựng; cân lại pose
  hoặc giảm `MaxGlanceYaw` cho actor nền.

### Ch2 — Đường theo Mã Giám Sinh · mức TRUNG BÌNH

- **Người đi đường (đàn ông áo nâu phía xa) lơ lửng cách mặt đất ~2–3cm** — chân không tiếp đất, thấy rõ
  khoảng hở dưới bàn chân.
- *Khắc phục:* đây là dấu hiệu kinh điển của **NavMeshAgent baseOffset sai** hoặc actor bị đặt lệch trục Y.
  Chọn NPC đó trong scene `Chapter02_RoadToMaGiamSinh`, sửa `NavMeshAgent.baseOffset` (thường = 0) hoặc
  hạ transform Y xuống chạm đất; nếu là `BackgroundPatrol` thì kiểm tra waypoint Y.

### Ch3 — Lầu Tú Bà · mức NHẸ (đã biết từ review trước)

- Tú Bà ngồi giữa khung ổn, không bể — nhưng **mặt tối** do thiếu đèn fill (lỗi ánh sáng, không phải model).
- *Khắc phục:* thêm 1 đèn point/spot yếu (ấm, ~0.3–0.5 intensity) vàoDialogueCameraRig key light đã có sẵn
  (field `_keyLight` của rig — tăng giá trị cho cảnh trong lầu), không cần sửa model.

### Ch4 — Đêm Tú Bà · mức NHẸ

- Tú Bà nằm ngủ tư thế tự nhiên, không chìm giường, không gãy rig ✓.
- **Người đứng trong phòng (Lam/hình phản chiếu) đứng im như tượng** — không idle, không дыхание thấy được.
- *Khắc phục:* bật animation idle nhẹ (thở/hơi đung đưa) cho `LamCompanion`/patron đứng trong tối;
  nếu là hình trong gương thì tăng sáng reflection (đã ghi ở review trước).

### Ch5 — Đào thoát (chase) · mức NHẸ

- Kiều chạy đúng dáng, không bể, không lơ lửng ✓.
- **Người đuổi bị bỏ xa thì chuyển sang đứng thẳng tay xuôi** (như chụp ảnh tĩnh) + 1 tên có tư thế tay
  đưa trán lạ — do animation blend chạy→đứng chưa mượt khi mất tốc độ.
- *Khắc phục:* trong `Follower`/Animator của pursuer, thêm blend tree Idle↔Run với threshold theo speed
  (dùng parameter `Speed` đã có trong `PlayerMovement`/`StoryActor`), tránh cắt cứng về idle pose.

### Ch6 — Phố Kim Trọng · mức TRUNG BÌNH

- **2 NPC (1 đứng 1 ngồi sau quầy) đứng im hoàn toàn, quay mặt vào nhau "đố mắt nhau"** — không idle,
  không nhúc nhích → nhìn rất giả trong khung explore.
- *Khắc phục:* 2 NPC này là background vendor (xem `Chapter06*Setup.cs`) — thêm animation idle ngẫu nhiên
  (breath/shift weight) hoặc cho 1 người có animation làm việc; nhanh nhất: bật `StoryActor` idle sway
  nếu có, hoặc đặt họ xa camera hơn để khuất.

### Ch7 — Huyền Đường · mức NHẸ (không phải model)

- Nhân vật công đường ổn, không bể/lơ lửng ✓.
- **Bảng chân dung Tú Bà trong hồ sơ đưa bằng chứng "nổi lơ lửng" giữa phòng** — không khung/bàn đỡ.
- *Khắc phục:* đây là UI 3D của `DossierView` khi mở ở phiên bản trial — gắn panel xuống thấp ngang tầm
  bàn hoặc thêm viền khung gỗ quanh panel trong `TrialView`/`DossierView` layout.

### Ch8 — Epilogue · SẠCH

- Kiều + bạn học đứng tự nhiên, chân tiếp đất, không trùng lặp, texture sạch ✓. Không cần sửa.

## Bổ sung 2: Chương 0 — Kiều bị lệch chân + dáng chạy hở (người chơi báo, tối 25/10)

- **Chẩn đoán bằng test trực tiếp** (ép Animator Speed=4.5, camera nhìn ngang hông, chụp 4 pha chu kỳ chạy mỗi thân):
  - Thân **học sinh hiện đại**: dáng chạy CHUẨN — đối xứng, không lệch, không xòe.
  - Thân **Kiều (áo dài)** — đúng như người chơi mô tả: hông trái thấp ~1–2cm, chân trái bước rộng hơn,
    váy xô lệch một bên khi chạy (nguyên nhân: clip hanfu chạy vốn nghiêng hông 13°, hệ `BodyProportions`
    chỉ khử nghiêng theo trung bình trễ 1.5Hz chậm hơn nhịp bước → từng pha chân vẫn lệch; cộng thêm
    nghiêng người 8° + nâng váy trái 85% làm cảm giác "hở" một bên).
- **Đã sửa (3 chỉnh nhỏ, giữ chất period-drama):**
  1. `BodyProportions.AlignLegs`: tăng tốc độ bám nghiêng 1.5Hz → **4Hz** — bám theo từng nhịp bước thay vì trung bình.
  2. `_skirtLift` **85 → 60** (bớt xô lệch bên váy).
  3. `_runLeanDegrees` **8 → 6** (bớt nghiêng người).
  - Giá trị 2–3 áp lên **prefab `ThuyKieu.prefab`** (hiệu lực mọi chương) + instance scene Ch00.
- **Verify**: chạy lại đúng test trên → ảnh `Tools/QA/fixes/captures_run/kieu_fixed_*.png` — MCP vision xác nhận
  **hông cân bằng, chân đối xứng, váy tự nhiên — hết lệch**.

## Bổ sung: Chương 1 — Mẹ Kiều & Kiều tay xuyên bàn thờ (người chơi báo, tối 25/10)

- **Lỗi thật, đã xác nhận bằng ảnh người chơi**: khi dàn dựng ở bàn thờ gia tiên (các beat montage),
  Mẹ Kiều/Kiều đứng cách tâm bàn thờ chỉ **0.78m** (`Chapter01World.cs`) → tư thế đưa tay (cúng/lạy)
  làm **cẳng tay xuyên qua mép bàn thờ**, chân chìm vào bệ dưới.
- **Đã sửa**: lùi điểm đứng `AltarTarget`/`AltarTargetStand` ra **1.08m** (thoát mép bàn + bệ).
- **Verify**: chạy lại Ch1 (seed 21) PASS 0 lỗi; ảnh `Tools/QA/fixes/captures_c1_altar/MONTAGE_montage_c1_mo_1.png`
  và `..._ra_di_1.png` xác nhận **hết clipping**, chân đứng trên sàn.

## Kết quả FIX tối 25/10

| Mục | Kết quả |
|---|---|
| **Ch0 nghiêng đầu** | Điều tra: pose cài sẵn 8/8 học sinh đồng nhất (cúi 10–14° nhìn bàn) — không bone nào lệch. Hiện tượng là **glance runtime** (học sinh liếc nhìn), là hành vi chủ ý → **không sửa** (tránh sửa mù). |
| **Ch2 NPC lơ lửng** | Đo chính xác raycast: 3 nhân vật rigged (Kiều/Mã Giám Sinh/Linh canh) đều **feetGap = 0.000** — chân chạm đất chuẩn → vision đọc nhầm ảnh chụp xa → **không có lỗi thật**. |
| **Ch3 mặt Tú Bà tối** | **ĐÃ SỬA — lỗi thật lớn hơn dự kiến**: `Chapter03_LauTuBa` (scene Ch3 chính thức) **mất sạch DialogueCameraRig** (`_shots` của director = NULL) do setup xóa object không tạo lại → cả chương không có camera thoại + không key light. Đã tạo lại rig chuẩn (lens 1.45, key light 1.3 ấm), nối vào `StorySystems`, save scene. Verify: PASS 0 lỗi, camera cắt shot từng người nói (Tú Bà trung cảnh ~3.1m), **mặt sáng rõ** (xác nhận ảnh `Tools/QA/fixes/captures_c3_rig/C3_L001_TuBa.png`). |
| **Ch4/Ch6 NPC đứng như tượng** | **ĐÃ SỬA (cả game):** thêm **hơi thở procedural** vào `StoryActor.LateUpdate` — xoay bone chest ±0.8° nhịp ~1.5Hz (phased ngẫu nhiên mỗi actor), chỉ khi đứng yên, tắt khi nằm/đi. Mọi NPC nền (Lam Ch4, vendor Ch6, đám đông Ch7) hết cảm giác tượng mà không cần clip mới. |
| **Ch7 panel hồ sơ lơ lửng** | **ĐÃ SỬA:** khi mở "Đưa bằng chứng", panel Actions đổi sang nền **đặc (0.96)** + tiêu đề "HỒ SƠ — ĐƯA BẰNG CHỨNG NÀO?" → card đọc như cầm tập hồ sơ thay vì treo giữa phòng; đóng thì trả nền bán trong suốt cũ (`TrialView.cs`). |
| **Ch5 pursuer đứng đơ khi hết đuổi** | **Chưa sửa** — cần blend tree Idle↔Run trong AnimatorController (việc asset animation, để vòng sau). Hơi thở mới giúp bớt độ "đơ" một phần. |

File đổi thêm vòng này: `StoryActor.cs` (hơi thở), `TrialView.cs` (nền hồ sơ), `Chapter03_LauTuBa.unity` (rig mới — đã save).

## Bảng tổng hợp nhanh (trạng thái sau fix)

| Chương | Lỗi | Trạng thái |
|---|---|---|
| Ch0 | Học sinh nghiêng đầu | Không phải lỗi (glance chủ ý) |
| Ch2 | NPC lơ lửng | Không có lỗi (đo = 0.000) |
| Ch3 | Mặt tối + mất camera thoại | **ĐÃ SỬA + verify ảnh** |
| Ch4 | Người đứng không idle | **ĐÃ SỬA (hơi thở)** |
| Ch5 | Pursuer đứng đơ | Còn (cần blend tree) |
| Ch6 | 2 NPC "đố mắt nhau" | **ĐÃ SỬA (hơi thở)** |
| Ch7 | Panel hồ sơ lơ lửng | **ĐÃ SỬA (nền đặc)** |
| Ch8 | — | Sạch |

## Ghi chú về MCP Unity

Setup đã hoàn tất 2 phía: `.zcode/config.json` (workspace) khai báo `uvx unity-mcp-server@10.2.0`;
bridge trong Editor đã bật (auto-start ON, port 6400 LISTENING, process server chạy kèm Editor).
Từ phiên ZCode tiếp theo các tool `unity-mcp` sẽ tự xuất hiện.
