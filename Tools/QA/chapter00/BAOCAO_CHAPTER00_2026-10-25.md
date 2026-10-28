# Báo cáo QA — Chapter 00 (Mở đầu) — 25/10/2026

## Cách chạy

- Người-chơi-ảo: `StoryAutopilot` (seed 7), Play Mode trong Editor, chơi nhanh hết cốt truyện
  Chapter 00: 47 câu thoại, 3 lựa chọn, đủ 4 phân cảnh (LopHoc → PhongKieu → VuongGia_San → VuongGia_Mua).
- Tự động chụp **66 ảnh** (mỗi dòng thoại 1 ảnh + UI + sequence) và đo frame time từng frame.
- Bằng chứng kèm theo:
  - Report text: `Tools/QA/chapter00/autopilot_20261025.txt`
  - Ảnh chụp: `Tools/QA/chapter00/captures_20261025/`
  - Lỗi runtime buổi thử tay sáng nay: `Tools/QA/2026-10-25/RuntimeErrors-C0.json`

## Kết luận nhanh

- **Logic story: PASS** — hết chương, không kẹt, không exception C# nào trong lượt chạy này (0 exception trong Editor.log của session).
- Có **1 lỗi hiển thị tiếng Việt (nghiêm trọng)**, **1 lỗi crash tiềm ẩn (chưa tái hiện)**, **1 lỗi spam log 22.185 lần / 94 giây**, **7 cú giật 156–200 ms**, và **góc máy thoại quá xa ở 5+ cảnh**.

---

## 1. Lỗi (bug)

### 1.1 [CAO] Menu Tạm dừng mất dấu tiếng Việt — "TẠM DỪNG" hiển thị thành "TAM DUNG"

- Bằng chứng: `captures_20261025/C0_pause.png`.
- Code nguồn ghi đúng chuỗi có dấu: `GameShell.cs:554` và `StoryUIBuilder.cs:375` (`"TẠM DỪNG"`).
- Nguyên nhân khả dĩ: `Assets/_Game/UI/Fonts/ChapterVietnamese.asset` đang bị thay đổi **chưa commit** trong working tree —
  git diff cho thấy atlas font chính bị thu từ **2048×2048 (4 MB) → 1×1 (1 byte)**, ~4445 dòng bị xóa.
  Font bị hỏng/thiếu glyph dấu nên chữ rơi về dạng không dấu.
- Đề xuất: hoàn tác/re-import lại `ChapterVietnamese.asset` (file TTF gốc còn ở `Assets/_Game/UI/Fonts/ChapterVietnamese.ttf`)
  rồi soạn lại atlas; sau đó bật lại Enter Play và soi lại menu Pause.

### 1.2 [CAO] NullReferenceException lặp 12 lần ở DialogueManager (buổi thử tay sáng nay)

- Nguồn: `Tools/QA/2026-10-25/RuntimeErrors-C0.json` (08:14 hôm nay, chạy thử bằng tay qua UI).
- Stack: `DialogueManager.cs:272` (`while (InkStory.canContinue)`) ← `DialogueManager.Advance()` ← `StoryDialogueView.Update():231`
  (nút "tiếp tục" của hộp thoại).
- `InkStory` không bao giờ bị gán null trong code, nên NRE nhiều khả năng nổ **bên trong Ink** khi story state bị
  reset/LoadJson đúng lúc người chơi bấm tiếp tục (re-entrancy).
- **Chưa tái hiện trong lượt autopilot hôm nay** (autopilot gọi `Advance()` theo đường khác, không qua Update UI).
- Đề xuất: null-guard + try/catch quanh `AdvanceInk()`, và cố tái hiện bằng tay: bấm Enter/click liên tục đúng lúc
  chuyển sequence/restore checkpoint.

### 1.3 [TRUNG BÌNH] Spam log 22.185 lần trong 94 giây — đọc `Animator.bodyRotation` sai chỗ

- Cả session Play chỉ có 1 loại warning, lặp **22.185 lần**: 
  `"Setting and getting Body Position/Rotation, IK Goals... should only be done in OnAnimatorIK or OnStateIK"`.
- Gây ra bởi `StoryActor.cs:385`: `LateUpdate()` đọc `_animator.bodyRotation` mỗi frame cho glance/liếc nhìn.
- Hệ quả: tốn CPU mỗi frame + Editor.log phình tới **1.4 GB**.
- Đề xuất: tính hướng thân từ `transform`/bone chest 1 lần khi đặt `_glanceTarget`, hoặc đọc bodyRotation trong `OnAnimatorIK`.

### 1.4 [TRUNG BÌNH] Journal (sổ tay) chữ bị cụt, tương phản thấp

- Bằng chứng: `captures_20261025/C0_journal.png`.
- Entry bên phải có dòng chữ bị cắt cụt cuối câu (tràn khung), một số dòng trắng/vàng trên nền vàng nhạt khó đọc.

---

## 2. Lag / hiệu năng

Số đo của cả chương (1210×629, Play Mode trong Editor — con số tuyệt đối chỉ mang tính tương đối):

| Chỉ số | Giá trị |
|---|---|
| FPS trung bình | 95 fps |
| Frame median / p99 | 9.3 ms / 25.5 ms |
| 1% low | 12 fps |
| Frame chậm nhất | 200 ms |
| Tỉ lệ frame < 60 fps | 2.9% |
| Frame đầu (load scene) | 1339 ms |

**7 cú giật (hitch) 156–200 ms**, thời điểm trùng với cắt camera / bắt đầu sequence / explore:

| Thời điểm trong story | Hitch |
|---|---|
| Lựa chọn 1 (lớp học, 42.2s) | 162 ms |
| Vào `tutorial_room` (54.9s) | 171 ms |
| Explore gương `pl_guong` (59–60s) | 168 + 156 ms |
| Vào sequence đàn `dan_tu_do` (63.4s) | 200 ms |
| Lựa chọn 3 (sân, 78.6s) | 194 ms |
| Cảnh bắt người `arrest` (81.6s) | 164 ms |

- Đề xuất: profile các thời điểm trên (Profiler/Frame Timing) — khả năng là instantiate effect/đổi camera/load material
  đồng thời; cảnh `dan_tu_do` nặng nhất (200 ms).
- Lưu ý: 1.3 (spam warning mỗi frame) cũng góp phần làm chậm Editor Play Mode.

---

## 3. Góc máy (camera)

So sánh vị trí camera với vị trí đầu nhân vật trong report (khoảng cách camera ↔ người nói) + phân tích ảnh bằng vision:

### 3.1 Camera thoại quá xa (người nói quá nhỏ trong khung)

| Shot | Người nói | Khoảng cách | Ảnh |
|---|---|---|---|
| L000, L005 | Thầy giáo | ~7.7 m | `C0_L000_ThayGiao.png` |
| L026 | Mẹ Kiều (PhongKieu) | ~8.0 m | `C0_L026_MeKieu.png` |
| L034 | Vương Ông (sân) | ~10.0 m | `C0_L034_VuongOng.png` |
| L037 | Quan sai | ~8.0 m | `C0_L037_QuanSai.png` |
| L043 | Vương Quan (cảnh bắt) | ~10.0 m | `C0_L043_VuongQuan.png` |

- Người nói chỉ chiếm ~2–5% khung hình, khó biết ai đang nói, khung hình nhiều khoảng trống chết (mái nhà, tường).
- Đề xuất: khoảng cách thoại chuẩn ~2.5–3.5 m hoặc dùng cận cảnh theo người nói (DialogueCameraRig).

### 3.2 Lỗi cụ thể từng shot

- **L000/L005 (lớp học):** học sinh ngồi mép phải bị **cắt nửa người**; camera chếch cao 30–45° làm lệch bố cục;
  chữ trên bảng đen sát/tràn mép viền.
- **L018 (Narrator):** hộp thoại che mất nhân vật ngồi giữa; góc lệch phải, thiếu cân đối.
- **L026 (Mẹ Kiều):** Kiều đứng lệch phải bị hộp thoại che mất chân; nửa khung trái trống.
- **L043 (cảnh bắt cha):** trọng tâm bị hút về bàn thờ "PHÚC ĐỨC" bên trái sáng rực, nhân vật chính kém nổi bật.
- **L034 (Vương Ông):** camera đặt trong nhà nhìn ra cửa, nhiều cột/mái nhà chắn giữa người nói và máy.

### 3.3 Điểm ổn

- Lựa chọn (choice) hiển thị rõ 4 option, dễ đọc (chỉ cần tăng tương phản nút).
- Montage mưa bố cục ổn; sequence `into_room` ổn.

---

## 4. Đã kiểm tra, KHÔNG phát hiện lỗi

- Kẹt story / deadlock: không (hết chương, thu đủ clue `pl_guong`, `pl_thu_kim_trong`).
- Exception/lỗi compile khi Play: 0 trong session này.
- Enough progression: các biến kết thúc hợp lý (`pl_che_kieu=3`, `pl_vat=3`, `pl_hanh_dong=3`, `tu_trong=3`).

---

## 5. File liên quan

| Nội dung | Đường dẫn |
|---|---|
| Báo cáo này | `Tools/QA/chapter00/BAOCAO_CHAPTER00_2026-10-25.md` |
| Autopilot log đầy đủ | `Tools/QA/chapter00/autopilot_20261025.txt` |
| 66 ảnh chụp từng dòng thoại/UI | `Tools/QA/chapter00/captures_20261025/` |
| Lỗi NRE buổi sáng (thử tay) | `Tools/QA/2026-10-25/RuntimeErrors-C0.json` |
