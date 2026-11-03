# Báo cáo SỬA LỖI + Test lại — 25/10/2026

> **Cập nhật vòng 2** (chiều 25/10): đã sửa tiếp các lỗi mức thấp + thiết kế lại camera thoại Ch0, xem mục "Vòng 2" bên dưới.

Người sửa: agent ZCode (chuyên C# + Unity). Nguồn lỗi: `BAOCAO_CHAPTER00_2026-10-25.md` và
`BAOCAO_TOANBO_CHUONG_2026-10-25.md`. Tất cả sửa qua Unity Editor đang mở (unity-cli eval / sửa file C# trực tiếp).

## Kết quả test sau khi sửa (quan trọng nhất)

Chạy lại toàn tuyến **Chapter 0 → Chapter 8** bằng autopilot (seed 13, 3x tốc độ, 100 giây):

| Chỉ số | Trước khi sửa | Sau khi sửa |
|---|---|---|
| Kết quả autopilot | FAIL (137 errors) | **PASS** |
| Lỗi runtime (ERROR/Exception) | 137 × `CharacterController.Move called on inactive controller` | **0** |
| Warning "OnAnimatorIK" spam | ~22.000–30.000 / lượt chạy | **0** |
| NullReferenceException DialogueManager | 12 (buổi thử tay) | 0 (đã guard, không tái hiện) |
| Kẹt story / chapter | không | không — hết game, đạt ending (verdict 115, trial 9) |

Report chạy verify: `Tools/QA/fixes/autopilot_final_20261025.txt` (PASS, 0 error)
và `Tools/QA/fixes/autopilot_verify_20261025.txt` (lượt có chụp 184 ảnh, phát hiện ra lỗi CharacterController).
Ảnh verify: `Tools/QA/fixes/captures_20261025/`.

---

## Các fix đã áp dụng

### Fix 1 — Khôi phục font `ChapterVietnamese.asset` (atlas hỏng 1×1)

- `Assets/_Game/UI/Fonts/ChapterVietnamese.asset` trong working tree bị thu atlas từ 2048×2048 (4MB)
  xuống **1×1 (1 byte)** — asset hỏng thật.
- Đã backup bản hỏng vào `Tools/QA/backup/ChapterVietnamese.asset.broken_1x1` rồi khôi phục bản tốt từ git
  (`git checkout -- <file>`). File giờ 12.3MB, git status sạch với file này.
- **Đính chính quan trọng về "TAM DUNG"**: kiểm tra kỹ sau khi sửa cho thấy TMP font `ChapterVietnamese`
  (dùng cho UI pause) **có đầy đủ glyph dấu** (`HasCharacter('Ạ')=True, HasCharacter('Ừ')=True`), TMP không bao giờ
  tự thay chữ có dấu bằng chữ không dấu (thiếu glyph sẽ hiện ô vuông). Dải pixel tiêu đề pause trước/sau giống hệt nhau.
  → Kết luận: cảnh báo "TAM DUNG" trong báo cáo trước nhiều khả năng do **vision model đọc nhầm**. Việc khôi phục
  atlas vẫn đúng (asset 1×1 là hỏng) nhưng có thể không phải nguyên nhân mất dấu. **Xin bạn nhìn mắt 5 giây** menu
  Pause (Esc) trong game để chốt: nếu vẫn mất dấu thì lỗi nằm ở chỗ khác — báo lại để tôi tìm tiếp.

### Fix 2 — Null-guard NRE `DialogueManager` (lỗi crash tiềm ẩn, mức CAO)

- File: `Assets/_Game/Scripts/Dialogue/DialogueManager.cs` (+3 dòng).
- `AdvanceInk()` giờ return ngay nếu `InkStory == null` — chống NRE khi story bị reset (restore checkpoint /
  đổi chapter) đúng khoảnh khắc người chơi bấm "tiếp tục" (đường `StoryDialogueView.Update → Advance`, lỗi từng
  lặp 12 lần trong `RuntimeErrors-C0.json` buổi sáng).

### Fix 3 — Warning spam 22–30 nghìn lần mỗi lượt chạy (mức TRUNG BÌNH, tốn CPU + phình log 1.4GB)

- File: `Assets/_Game/Scripts/Core/Story/StoryActor.cs`.
- `LateUpdate()` đọc `Animator.bodyRotation` mỗi frame → Unity warning "should only be done in OnAnimatorIK"
  hàng chục nghìn lần mỗi lượt chơi, xuyên suốt mọi chapter.
- Sửa: cache **bone Chest** một lần (`GetBoneTransform(HumanBodyBones.Chest)`, được reset khi `SetAnimator` đổi thân)
  và lấy hướng thân từ `chest.forward`. Verify: warning = **0** trong lượt chạy cuối.

### Fix 4 — Hint "Backspace / chuột phải: cất hồ sơ" bị cắt nửa chữ (màn Trial Ch7)

- File: `Assets/_Game/Scripts/UI/StoryUIBuilder.cs` (1 dòng).
- Rect chữ 18pt chỉ cao 3.5% panel → nới lên 5.3% (`0.005→0.04` thành `0.002→0.055`). Không đụng phần sửa
  khác của bạn trong file này (ControlsHelp giữ nguyên).

### Fix 5 — 137 lỗi `CharacterController.Move called on inactive controller` (phát hiện MỚI khi verify)

- File: `Assets/_Game/Scripts/Player/PlayerMovement.cs` (+2 dòng).
- Cinematic disable capsule rồi `Update()` vẫn gọi `controller.Move()` mỗi frame → 137 error trong 1 lượt chạy
  (bắt đầu từ Chapter 0). Sửa: `if (!controller.enabled) return;` đầu `Update()`. Verify: 0 lỗi.

## Hiệu năng sau khi sửa (Editor Play Mode, 3x tốc độ)

Không còn cú giật >150ms nào được ghi trong lượt cuối (worst frame các chapter 39–101ms — chủ yếu do Editor
chạy nền khi compile). Cảnh cần để mắt khi build thật: **Ch5 Đào thoát** (p99 63.5ms, 50% frame dưới 60fps ở chế độ
chạy nhanh) và **Ch1/Ch3** khi Editor bận — nên profile bằng Player build trước khi tối ưu.

## Vòng 2 — sửa lỗi mức thấp + thiết kế lại (chiều 25/10)

### 2.1 Camera thoại Ch0 lại gần người nói (thiết kế lại)

Trước: 5 shot thoại camera đứng xa 8–10m, người nói tí hon. Sau khi điều tra: 3 shot dùng **anchor `CAM_*` đặt sẵn trong scene**
(2 shot còn lại — L026 Mẹ Kiều, L043 Vương Quan — do sequence dàn dựng camera bằng code, để nguyên).

Đã kéo 3 anchor lại gần theo đúng hướng chụp cũ, hạ máy xuống tầm mắt + hướng thẳng vào người nói (scene **đã lưu**):

| Anchor | Shot | Trước | Sau |
|---|---|---|---|
| `CAM_Wide_Classroom` | Thầy giáo dạy học (L000, L005) | 7.7m, cao 2.4m | **3.2m**, 1.8m |
| `CAM_Wide_Courtyard` | Vương Ông (L034) | 10.0m | **3.5m** |
| `CAM_Gate` | Quan sai (L037) | 8.0m | **3.1m** |

Lưu ý kinh nghiệm: `CAM_Gate` tính theo vị trí actor ở **edit mode** bị lệch (0.7m — quá gần) vì sequence dàn dựng lại
vị trí actor lúc runtime; đã tính lại theo vị trí runtime. Verify bằng telemetry lượt chạy sau: cả 3 shot 3.0–3.5m ✓.

### 2.2 HUD hết chồng lấn + hết trùng lặp mục tiêu

- **Ch2 (banner đè thanh tiến trình)**: `StoryUIBuilder.BuildHud` — Header rộng tới x=0.40 trong khi Meter bắt đầu x=0.38
  → thu Header về x=0.36.
- **Ch5 (mục tiêu hiện 2 lần)**: `StoryHUD.SetObjective` giờ (a) không re-announce khi objective không đổi,
  (b) khi toast "MỤC TIÊU" hiện, Header chờ 3.4s rồi mới hiện — hết 2 khối cùng nội dung trên màn hình (Ch5 chase sạch màn hình hơn).

### 2.3 Journal (Sổ tay): lăn chuột chuyển trang

Điều tra ra: sổ tay vốn **phân trang sẵn** (TMP Page mode + ←/→) — "chữ cụt" là ngắt trang, không phải mất chữ.
Cải thiện UX: **lăn chuột lật trang** (Input System `Mouse.current.scroll`) + hint cập nhật "←/→ / lăn chuột: lật trang (X/Y)".
Sửa lần đầu dùng `UnityEngine.Input.mouseScrollDelta` bị ném InvalidOperationException 60 lần (project bật Input System
thuần) — đã thay bằng Input System và chạy lại: **0 lỗi**.

### Kết quả test vòng 2 (Chapter 00, capture từng dòng, seed 7)

- `AUTOPILOT PASS` · **0 error, 0 exception** (`Tools/QA/fixes/autopilot_c0_final.txt`).
- Khoảng cách camera thoại: 3.2m / 3.5m / 3.1m (telemetry từng dòng trong report).
- FPS: avg 98, median 9.1ms, p99 22.4ms — không đổi.
- Ảnh: `Tools/QA/fixes/captures_c0_final/` (47 shot từng dòng + UI). MCP vision service đang lỗi 400 nên khung hình
  mới chưa xem lại được bằng ảnh — telemetry khoảng cách là bằng chứng khách quan; bạn mở Editor chạy thử nhìn nhanh 3 shot:
  Thầy giáo dạy học, Vương Ông ở sân, Quan sai ở cổng.



## Vòng 3 — warning "PlayOneShot was called with a null AudioClip" (chiều muộn 25/10)

Người chơi bắt được qua Console (bot QA chỉ bắt Error/Exception nên bỏ sót warning):

- Stack: `LuteSequence.Pluck (LuteSequence.cs:107)` — nhánh autopilot gảy đàn `dan_tu_do`.
- Gốc rễ: `AudioBank.Get` **cache cả kết quả null** (`Cache[key] = clip` vô điều kiện). Với domain reload tắt,
  cache static sống qua nhiều phiên Play → một lần lấy clip thất bại transient (vd đúng lúc script recompile)
  khiến id `sfx/dan_note` bị "đầu độc" trả null mãi trong phiên Editor đó → mọi lần gảy dây PlayOneShot(null).
  (Clip thật sự có: bank có synthesis `dan_note` — đã test riêng trả clip 2.6s.)
- Đã sửa 2 lớp:
  1. `AudioBank.cs` — chỉ cache kết quả khác null (miss sẽ thử lại lần sau).
  2. `LuteSequence.Pluck` — null-guard trước khi PlayOneShot.
- Verify: chạy lại C0 (seed 7, có sequence đàn) — **PASS, 0 error, 0 warning "null AudioClip"** trong log session
  (`Tools/QA/fixes/autopilot_c0_audio.txt`).

Ghi chú 2 dòng Console khác trong ảnh người chơi gửi: `[Autopilot] Report written` là log QA (vô hại);
`UnityConnectWebRequestException: Token Exchange failed` là lỗi đăng nhập Unity Cloud của chính Editor (mạng),
không liên quan game.

## Cấu hình Unity MCP cho workspace (cho các phiên sau)

Đã ghi `D:\ThuyKieuKiemTien_Ne\PRU213-ThuyKieu\.zcode\config.json`:

```json
{ "mcp": { "servers": { "unity-mcp": { "command": "uvx", "args": ["unity-mcp-server@10.2.0"] } } } }
```

- Khớp plugin `com.coplaydev.unity-mcp` 10.2.0 trong project; máy đã có `uvx`.
- **Cần mở lại ZCode (hoặc mở lại workspace) để MCP server tự kết nối** — tool Unity MCP sẽ xuất hiện từ phiên đó.
  Trong Unity Editor: mở `Window → Unity MCP (Unity MCP For Unity)` lần đầu để bridge sẵn sàng nếu chưa bật.
- Phiên này tôi đã làm việc qua `unity-cli eval` — cùng truy cập live Editor, nên tiến độ fix không bị chờ.

## Còn lại chưa sửa (cần quyết định thiết kế sau)

1. ~~Journal: entry dài bị cụt~~ — đã rõ là ngắt trang có chủ ý; đã thêm lăn chuột (2.3). Nếu vẫn muốn cuộn liên tục
   thay vì phân trang thì cần đổi sang ScrollRect — việc thiết kế lại, chưa làm.
2. ~~HUD Ch2 banner đè thanh tiến trình; Ch5 mục tiêu trùng 2 nơi~~ — đã sửa (2.2).
3. ~~Camera thoại Ch0 quá xa~~ — đã sửa 3/5 shot bằng anchor (2.1); còn **L026 (Mẹ Kiều) và L043 (Vương Quan, cảnh bắt)**
   do sequence dàn dựng camera bằng code — muốn lại gần phải sửa trong sequence, hẹn vòng sau.
4. NRE DialogueManager đã guard nhưng chưa tái hiện được gốc rễ (Ink re-entrancy) — để ý khi chơi tay.
5. MCP vision service lỗi 400 chiều nay nên các ảnh vòng 2 chưa kiểm tra bằng mắt máy được — bạn mở Editor
   chạy Play nhìn nhanh 3 shot camera mới + menu Pause + Sổ tay là chốt.

## File thay đổi (tổng kết cả 2 vòng)

| File | Thay đổi |
|---|---|
| `Assets/_Game/UI/Fonts/ChapterVietnamese.asset` | Khôi phục atlas 2048² từ git (backup bản hỏng ở Tools/QA/backup) |
| `Assets/_Game/Scripts/Dialogue/DialogueManager.cs` | +null-guard AdvanceInk |
| `Assets/_Game/Scripts/Core/Story/StoryActor.cs` | chest bone thay bodyRotation (hết warning spam 30k) |
| `Assets/_Game/Scripts/UI/StoryUIBuilder.cs` | nới rect hint Trial; Header HUD thu về 0.36 hết đè Meter |
| `Assets/_Game/Scripts/Player/PlayerMovement.cs` | +guard controller disabled (hết 137 error) |
| `Assets/_Game/Scripts/UI/StoryHUD.cs` | SetObjective hết trùng lặp header+toast |
| `Assets/_Game/Scripts/UI/JournalView.cs` | lăn chuột lật trang (Input System) |
| `Assets/_Game/Scripts/Presentation/AudioBank.cs` | không cache kết quả null (vòng 3) |
| `Assets/_Game/Scripts/Gameplay/Sequences/LuteSequence.cs` | null-guard PlayOneShot gảy đàn (vòng 3) |
| `Assets/_Game/Scenes/Chapter00_Prologue.unity` | kéo 3 anchor CAM_* lại gần (3.0–3.5m), đã lưu |

(`TutorialSequence.cs` là thay đổi có sẵn của bạn từ trước, tôi không đụng.)

Compile sạch (0 lỗi CS). Không commit — bạn xem diff (`git diff`) rồi tự quyết định commit.
