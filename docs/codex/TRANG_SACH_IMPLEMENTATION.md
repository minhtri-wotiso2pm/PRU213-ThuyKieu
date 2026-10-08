# Trang Sách · Định Mệnh · Hai linh hồn — bàn giao (P25–P30)

Thiết kế gốc: tài liệu "Thiết kế cơ chế Trang Sách" (Claude Doc). File này ghi lại **cách đã cài**, chỗ nào khác thiết kế, và cách kiểm thử.

## 1. Tóm tắt

| Hệ thống | Ink | Unity |
| --- | --- | --- |
| Trang Sách (11 trang thơ) | biến `ts_*` (0 chưa có · 1 rách · 2 giữ · 3 đã xé), hàm `nhan` / `nua` / `dung`, tag dòng `# page:<id>` / `# page_half:<id>`, lựa chọn `#page:<id>` | `Core/Story/PageCatalog.cs` (dữ liệu), `Core/Story/PageArchive.cs` (giữ trang qua các lần chơi), `UI/TrangSachView.cs` (sổ B, toast, vệt mực) |
| Định Mệnh | `dinh_menh` 0–100, hàm `fate(n)`; ngưỡng 30 · 50 · 70 · 85 viết thẳng trong Ink | vệt mực bên trái màn hình, nháy mực đỏ khi vượt ngưỡng; đối chất: ≥ 85 thì lòng tin đầu −15% (trừ khi đã thấy hối lộ), thua vòng +10 |
| Hai linh hồn | `dong_dieu` 0–10, hàm `soul(n)` | sổ Trang Sách hiện "hai bàn tay"; Kiều Thật gợi ý ở lục bát (≥ 6) và đối chất (≥ 5) |
| Lục bát | `# gameplay:lucbat_pl / _ch3 / _ch6 / _ch7`, Unity ghi `lb_*` (0–3) | `Gameplay/Sequences/LucBatCatalog.cs`, `LucBatSequence.cs`, `UI/LucBatView.cs` — tự đăng ký trong mọi chương, không cần dựng lại scene |
| Đối chất (A) | `c7_thua_vong` | `TrialSequence`: gợi ý tăng dần, thua vòng thì quan tự xét giấy tờ (không chơi lại), phím **T** = Tiên tri |
| Bốn kết (B) | `c7_diem_pq`, `c7_ket` 0–3, `ket_hon` | `EndingSequence` (tổng kết), `Chapter07World` (cảnh `HuyenDuong_BongToi` lạnh cho kết thua) |
| Sở Khanh (Ch5) | `c5_so_khanh`, cảnh `Escape_SoKhanh` | `Chapter05World.MeetSoKhanh` nhân bản rig gia nhân (Mã Giám Sinh) tô xanh nhạt, bỏ đuốc; `ForestRunSequence` rút ngắn khoảng cách 2.5 m nếu hắn hô hoán |

## 2. Trang thơ: nhận ở đâu, lật ở đâu

| Trang | Nhận | Lật (lựa chọn `#page`) | Giá |
| --- | --- | --- | --- |
| `ts_tai_menh` | Mở đầu, quyển sách phát sáng | Ch2 `c2_mask`: cười buồn như đã cam phận (nghi ngờ −25) · Tiên tri | 5 |
| `ts_hieu` | cuối Ch1 nếu `tinh_tao` ≥ 3 | Ch3 trả lời Kiều Thật · Ch6 Kim Trọng +2 | 5 |
| `ts_ma_mat` | rách ở Ch1, đủ khi quan sát Mã Giám Sinh (Ch2) | Ch2 `c2_ask`: tâng bốc cho hắn tự khai · Tiên tri | 10 |
| `ts_co_ke` | Ch2, ký ức ở quán nước | Ch2: bỏ qua bảng suy luận, đủ điểm · Tiên tri | 10 |
| `ts_tu_ba` | Ch3, gặp Tú Bà | Ch4 "dạy quy củ": cảnh giác 0 + biết số nợ | 10 |
| `ts_hoa_troi` | Ch3, chị Lam, khi đồng điệu ≥ 4 | Ch4: chắc chắn rủ được chị Lam | 0 |
| `ts_so_khanh` | rách: cô Hồng (Ch3) hoặc chị Lam (Ch4); đủ khi có cả hai | Ch5: vạch mặt Sở Khanh | 5 |
| `ts_kim_trong` | cuối Ch5 | Ch6 gặp ở chùa: Kim Trọng +3 | 5 |
| `ts_be_dau` | Ch6, khi có từ 2 nhân chứng | Ch7: giữ nhân chứng không lùi bước | 10 |
| `ts_chu_tam` | Ch7, khi đồng điệu ≥ 7 | Ch7 lục bát: tự soi sáng đáp án | 0 |
| `ts_thanh_lau` | Ch7, khi Định Mệnh ≥ 85 | không lật được | — |

**Tiên tri** (phím T trong đối chất) lật trang đang giữ đầu tiên theo thứ tự bảng, trừ `ts_be_dau` và `ts_chu_tam` (để dành cho việc riêng ở Ch7). Câu then chốt của vòng được viền đỏ; +10 Định Mệnh.

## 3. Điểm phán quyết và kết

`c7_diem_pq = c7_long_tin + 15 (kt_tin_tuong ≥ 3) + 10 (có nhân chứng, không ai lùi) + 0/10/20 (lb_ch7 1/2/3) − 15 × c7_thua_vong`

- ≥ 90 và hồ sơ ≥ 3 → `c7_ket` 3 · ≥ 60 → 2 · ≥ 35 → 1 · dưới 35 → 0 (**thua kiện**, `c7_thua_kien`).
- Sau đó Kiều Thật: đồng điệu ≤ 3 → `c7_mat_minh` (không cảm ơn) · kết 3 và đồng điệu ≥ 7 → `c7_nghich_menh` (chọn `ket_hon` 1 chia tay / 2 cùng sống) · còn lại cảm ơn như cũ.
- Kết (Epilogue) rẽ theo cùng các biến; kết thua dùng câu "Đau đớn thay phận đàn bà…", không có dòng "Cảm ơn".

## 4. Khác so với thiết kế

- Dữ liệu trang là lớp tĩnh `PageCatalog` (giống `ClueCatalog`) thay vì ScriptableObject.
- Chương 1 là scene dựng tay có bài hồi quy đếm lựa chọn, nên **không thêm lựa chọn** ở Ch1: các lần lật "sớm" của `ts_tai_menh` và `ts_hieu` chuyển sang Ch2 và Ch3.
- Đốt văn tự không trừ đồng điệu (Kiều Thật trong kịch bản tán thành việc đốt); thay vào đó +5 Định Mệnh.
- Thua vòng đối chất: lòng tin ghi 10%, vòng sau bắt đầu lại ở 40%.
- UI Trang Sách / lục bát tự dựng bằng code (không sửa prefab `StoryCanvas`), nên dùng được cả ở Ch1. Danh sách phím trong menu tạm dừng (`StoryUIBuilder`) chỉ cập nhật khi dựng lại scene.

## 5. Kiểm thử

- Kịch bản: `InkStoryValidator.RunBatch` — kiểm thêm: tag `page` chỉ hiện khi đã có trang, lựa chọn `#page` chỉ hiện khi đang giữ trang, biên `dinh_menh` / `dong_dieu`, phân bố `c7_ket` 0–3 và `ket_hon`.
- Autopilot nhắm từng kết: `PlaytestBatch -playScene Assets/_Game/Scenes/Chapter07_HuyenDuong.unity -storyAutopilot -autopilotChain -autopilotEnding <nghich_menh|thang|mat_minh|dinh_menh>`. Với `dinh_menh`, Ch5 cũng được đẩy Định Mệnh lên 60 để gặp Sở Khanh.
- Chạy Unity.exe trực tiếp (`-batchmode -projectPath .`) cho playtest; `unity run` tự thêm `-quit`.
- Ảnh QA: thêm `-autopilotCapture <thư mục>` → có `LB_<id>_1_tiles.png`, `LB_<id>_2_tones.png`, `C<n>_trang_sach.png`.
- Giữ trang qua lần chơi lưu ở PlayerPrefs `ThuyKieu.TrangSach.Archive`; xóa bằng `PageArchive.Clear()` khi cần chơi "sạch".

## 6. Việc còn lại

- Người dùng tự soát lại từng câu thơ Truyện Kiều và luật bằng/trắc của 4 cặp lục bát (`LucBatCatalog.cs`).
- Sở Khanh đang dùng rig Mã Giám Sinh tô màu; khi có model riêng, thay trong `Chapter05World.MeetSoKhanh`.
- Lời thoại mới chưa có giọng đọc: chạy `VoiceManifestExporter.RunBatch` rồi `python Tools/Voice/generate_voice.py` (cần mạng).
- Ngưỡng thua kiện (35) và giá các trang cần chơi tay để cân lại.
