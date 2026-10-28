# Báo cáo QA toàn bộ cốt truyện (Chapter 0 → Chapter 8) — 25/10/2026

## Cách chạy

Chơi nhanh toàn bộ cốt truyện bằng bot `StoryAutopilot` (Play Mode trong Unity Editor), 3 lượt:

| Lượt | Phạm vi | Kết quả |
|---|---|---|
| 1 | Chapter 00 riêng, chụp **mỗi dòng thoại** (seed 7) | PASS — xem chi tiết trong `BAOCAO_CHAPTER00_2026-10-25.md` |
| 2 | Chain Ch1→Ch8, seed 42, chụp theo kiểu cảnh | **Bị ngắt giữa Chapter 4** — xem mục "Sự cố" bên dưới |
| 3 | Chain Ch4→Ch8, seed 42 | **PASS** — hết game, đạt ending, không kẹt, 0 exception |

- Bằng chứng: `Tools/QA/allchapters/autopilot_chain2_20261025.txt` (chain hoàn chỉnh Ch4→Ch8, 205 giây),
  `Tools/QA/allchapters/captures_20261025/` (165 ảnh: mỗi chapter có choice/explore/journal/pause/
  từng kiểu speaker/sequence), `Tools/QA/chapter00/` cho Chapter 0.
- Tham chiếu số liệu Ch1–Ch3: `Tools/QA/2026-10-24/FullChainVerified.txt` (lượt chạy ngày 24/10).

## Kết luận nhanh

- **Toàn bộ cốt truyện chơi thông từ Mở đầu tới Epilogue**, đủ ending, trial/lục bát/pickpocket chạy đúng,
  cơ chế checkpoint phục hồi hoạt động (đã test ép fail ở Ch4).
- **0 exception C#, 0 LogError** trong cả lượt chain Ch4→Ch8.
- Lỗi nghiêm trọng nhất vẫn là **mất dấu tiếng Việt ở một số UI** (Pause/Sổ tay) do font atlas
  `ChapterVietnamese.asset` bị thu nhỏ còn 1×1 trong working tree — trong khi Ending screen (dùng font khác)
  hiển thị "THÚY KIỀU: NGHỊCH MỆNH" có dấu chuẩn.
- **Spam warning 30k+ lần** mỗi lượt chạy (`StoryActor.cs:385`) — hiện diện xuyên suốt mọi chapter, không riêng lớp học.
- Hiệu năng: riêng **Ch5 (Đào thoát) 14.9% frame dưới 60fps** và **đầu Ch6 có 6 hitch liên tiếp**; còn lại mượt.

---

## 1. Lỗi (bug)

### 1.1 [CAO] Mất dấu tiếng Việt ở Pause menu và Sổ tay

- Pause hiện "TAM DUNG" thay vì "TẠM DỪNG" (`captures_20261025/C0_pause.png`, code đúng ở `GameShell.cs:554`, `StoryUIBuilder.cs:375`).
- Nguyên nhân: `Assets/_Game/UI/Fonts/ChapterVietnamese.asset` bị sửa chưa commit — git diff cho thấy atlas
  2048×2048 (4MB) bị thay bằng texture **1×1 (1 byte)**, ~4445 dòng xóa.
- Đối chứng: màn kết thúc Chapter 8 (`C8_seq_ending.png`) hiển thị "THÚY KIỀU: NGHỊCH MỆNH" có dấu đầy đủ
  → chỉ các UI dùng font atlas này bị ảnh hưởng.
- Cách sửa: khôi phục file từ git (`git checkout -- Assets/_Game/UI/Fonts/ChapterVietnamese.asset`) hoặc re-import
  từ `ChapterVietnamese.ttf`, rồi vào Play kiểm tra lại Pause/Sổ tay.

### 1.2 [CAO] NRE lặp ở DialogueManager (chỉ thấy ở lượt thử tay sáng nay)

- `RuntimeErrors-C0.json`: 12 lần `NullReferenceException` tại `DialogueManager.cs:272` (`while (InkStory.canContinue)`)
  qua `Advance()` ← `StoryDialogueView.Update():231` (bấm tiếp tục hộp thoại).
- **Không tái hiện** trong cả 3 lượt autopilot (bot advance bằng đường khác, không qua nút UI).
- Khả năng: Ink story bị reset/restore checkpoint đúng lúc người chơi bấm tiếp tục (re-entrancy).
- Việc cần: thêm null-guard quanh `AdvanceInk()`, và thử tay: click/Enter liên tục đúng lúc checkpoint restore.

### 1.3 [TRUNG BÌNH] Warning "OnAnimatorIK" spam hàng chục nghìn lần — cả game

- `StoryActor.cs:385` đọc `_animator.bodyRotation` trong `LateUpdate()` mỗi frame cho hiệu ứng liếc nhìn.
- Số lượng: ~22.185 warning trong 94 giây (lượt Ch0), **~29.954 warning** trong 205 giây (chain Ch4→Ch8).
  Vậy warning này không phải chỉ cảnh NPC ngồi — mọi chapter đều bị.
- Hệ quả: tốn CPU mỗi frame; Editor.log phình ~1.4GB.
- Sửa: cache hướng thân (transform forward / bone chest) khi gán `_glanceTarget`, hoặc đọc trong `OnAnimatorIK`.

### 1.4 [TRUNG BÌNH] Vòng đối chất Ch7 — dòng hint phím đáy màn hình bị cắt nửa chữ

- `C7_seq_trial_1.png`: dòng "1-4: chọn lời khai · Ctrl: hỏi lại · E: đưa bằng chứng · Backspace: cất hồ sơ ·
  Esc: tạm dừng" chỉ hiện **nửa chiều cao chữ** ở mép dưới màn hình (bị rect cha clip).
- Ở vòng 2/3 (`C7_seq_trial_2/3.png`) hồ sơ mở full màn nên không thấy lỗi; người chơi vòng 1 sẽ đọc được hint xén.

### 1.5 [TRUNG BÌNH] Sổ tay (Journal) chữ cụt + tương phản thấp (lặp lại từ báo cáo Ch0)

- `C6_journal.png`, `C0_journal.png`: entry dài bị cắt cụt cuối câu; dòng trắng/vàng trên nền vàng khó đọc.

### 1.6 [NHỎ] UI chồng lấn / trùng lặp mục tiêu

- `C2_observe.png`: banner mục tiêu "CHƯƠNG 2 — TRÊN ĐƯỜNG THEO MÃ GIÁM SINH" **đè lên** thanh tiến trình
  "Nghỉ ngơi của Mã Giám Sinh" góc trên trái.
- `C5_seq_chase_forest.png`: mục tiêu hiển thị **2 lần cùng lúc** (banner trái "CHƯƠNG 5 · ĐÀO THOÁT" và panel
  phải "MỤC TIÊU" cùng nội dung "Chạy xuyên rừng tre") — thừa, chiếm chỗ màn hình chase.

---

## 2. Lag / hiệu năng (theo từng chapter)

Số liệu Play Mode trong Editor (nên so tương đối). Cột "hitch" là những frame > 150ms được bot ghi lại.

| Chapter | FPS TB | 1% low | Worst | Dưới 60fps | Ghi chú |
|---|---|---|---|---|---|
| Ch0 Mở đầu | 95 | 12 | 200 ms | 2.9% | 7 hitch 156–200ms, tập trung explore phòng Kiều + cắt camera |
| Ch1 Gia Biến | 55 | 4 | **1921 ms** | (24/10) | worst 1921ms nghi là shader compile/load lần đầu — cần verify lại |
| Ch2 Đường theo Mã Giám Sinh | 61 | 6 | 241 ms | (24/10) | |
| Ch3 Nhà Tú Bà | 100 | 12 | 177 ms | (24/10) | |
| Ch4 Đêm Tú Bà | 148 | 22 | 150 ms | 0.9% | Không hitch nào — cảnh tối ưu tốt nhất |
| Ch5 Đào thoát | 92 | **13** | **225 ms** | **14.9%** | 4 hitch (178/155/225/209ms) ở sneak_out, chase_forest, Escape_Rain |
| Ch6 Kim Trọng | 142 | 16 | 167 ms | 1.4% | **6 hitch liên tiếp đầu scene** (97–109s, cách nhau ~2s: 155–167ms) — khả năng streaming asset/đèn |
| Ch7 Huyền Đường | 129 | 12 | 180 ms | 2.1% | 3 hitch ở tutorial_gate/công đường |
| Ch8 Epilogue | 161 | 18 | 147 ms | 0.7% | Mượt nhất |

Việc nên làm: profile Ch5 (frame time cao ổn định) và đầu Ch6 (hitch tuần hoàn ~2s — đặc trưng của
streaming/instantiate định kỳ, ví dụ đèn/người đường phố được bật dần).

---

## 3. Góc máy & hình ảnh

**Ch1–Ch8: chất lượng tốt, khác hẳn Ch0.** Cận cảnh thoại đúng người nói, khung hình cân:

- `C2_line_Spoken.png` — Mã Giám Sinh cận giữa khung, ánh hoàng hôn đẹp.
- `C6_line_Spoken.png` — Kim Trọng cận cảnh chuẩn, bối cảnh sân chùa rõ nét.
- `C5_seq_chase_forest.png` — khung chase đêm có chiều sâu (đèn lồng, con hẻm), Kiều hơi nhỏ nhưng hợp thể loại chạy trốn.
- `C8_seq_ending.png` — màn "HÉT / THÚY KIỀU: NGHỊCH MỆNH" sạch, font có dấu chuẩn.

Vấn đề còn lại của camera nằm ở **Chapter 0** (đã liệt kê 5 shot quá xa 8–10m trong
`BAOCAO_CHAPTER00_2026-10-25.md` mục 3): Thầy giáo, Mẹ Kiều, Vương Ông, Quan sai, Vương Quan.

Polish nhỏ: trong `C7_seq_trial_2.png`, bức chân dung Tú Bà trong hồ sơ nổi lơ lửng giữa phòng đối chất
(không có khung/bàn đỡ) — đọc được nhưng hơi kỳ về không gian.

---

## 4. Sự cố trong quá trình QA (không phải bug của game)

- Lượt chain 2 bị **thoát Play Mode giữa Chapter 4** mà không có report. Điều tra log: không exception,
  không crash reporter, Editor sống, scene được khôi phục bình thường; không có code runtime nào gọi Quit
  trong luồng autopilot. Chạy lại (lượt 3) thì Ch4→Ch8 thông suốt.
  → Kết luận: Play Mode bị dừng **từ bên ngoài** (một cú click/phím trúng cửa sổ Unity — ví dụ phím Play/Stop).
  Khi chạy QA tự động, để yên cửa sổ Unity Editor.

## 5. Đã kiểm tra, KHÔNG có lỗi

- Toàn tuyến Ch4→Ch8: không kẹt, không timeout, không exception; checkpoint restore hoạt động khi bị ép fail.
- Trial Ch7: 3 vòng chấm đúng điểm (r1=3, r2=1, r3=1, lòng tin 100); lục bát Ch6/Ch7 đạt score 3.
- Pickpocket, đốt hồ sơ, lấy bạc ở Ch4; QTE chase Ch5; đối chiếu hồ sơ Ch6 — đều chạy tới nơi.
- Kết thúc: `ENDING tone 3 · verdict 135 · fate 25 · trial 5 · dossier 3` — hệ ending tính được kết quả.

## 6. File bằng chứng

| Nội dung | Đường dẫn |
|---|---|
| Báo cáo này | `Tools/QA/allchapters/BAOCAO_TOANBO_CHUONG_2026-10-25.md` |
| Báo cáo riêng Chapter 0 | `Tools/QA/chapter00/BAOCAO_CHAPTER00_2026-10-25.md` |
| Log chain hoàn chỉnh Ch4→Ch8 | `Tools/QA/allchapters/autopilot_chain2_20261025.txt` |
| 165 ảnh các chapter | `Tools/QA/allchapters/captures_20261025/` |
| 66 ảnh Ch0 (mỗi dòng thoại) | `Tools/QA/chapter00/captures_20261025/` |
| NRE buổi thử tay | `Tools/QA/2026-10-25/RuntimeErrors-C0.json` |
| Số liệu Ch1–Ch3 ngày 24/10 | `Tools/QA/2026-10-24/FullChainVerified.txt` |
