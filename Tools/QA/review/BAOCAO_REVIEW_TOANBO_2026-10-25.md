# Báo cáo REVIEW toàn bộ game (người chơi ảo) — 25/10/2026 chiều

Chơi lại từ đầu tới cuối: **Mở đầu → Epilogue** (9 scene), seed 21, đủ 183 ảnh chụp,
review từng chương bằng telemetry + phân tích ảnh (MCP vision).

- Report autopilot: `Tools/QA/review/autopilot_review.txt` — **AUTOPILOT PASS, 0 ERROR, 0 Exception game**.
- Ảnh: `Tools/QA/review/captures/` (183 ảnh).
- Log session: 0 warning spam (trước đây 22–30k), 0 "null AudioClip", 0 NRE.

## Điểm chung sau cả loạt fix (so với buổi sáng)

| Vấn đề cũ | Bây giờ |
|---|---|
| Font mất dấu Pause menu | ✅ "TẠM DỪNG" hiển thị **đủ dấu** (xác nhận bằng ảnh) |
| Mục tiêu Ch5 hiện trùng 2 chỗ | ✅ **Chỉ 1 chỗ** (ảnh chase rừng tre) |
| Hint Trial Ch7 cắt nửa chữ | ✅ **Đầy đủ** "Backspace / chuột phải: cất hồ sơ" |
| Camera thoại Ch0 xa 8–10m | ✅ Shot Thầy giáo/giáo viên giờ cận – trung cảnh đẹp |
| Warning spam 30k / session | ✅ 0 |
| CharacterController error 137 | ✅ 0 |
| Kẹt story | ✅ Không — hết game, ending "THÚY KIỀU: NGHỊCH MỆNH" (verdict 135) |

## Review từng chương

### Ch0 — Mở đầu (lớp học → phòng Kiều → sân → mưa) · 38s · 100 fps
- Khung lớp học giờ đẹp: thầy giáo trung cảnh, ánh sáng ấm, chữ bảng đọc rõ, dấu tiếng Việt chuẩn.
- Nhỏ: thầy hơi che một phần chữ bảng (chấp nhận được); dòng "TIẾN HỌC LỄ — HẬU HỌC VĂN" trên banner bị cắt đầu (cần xác nhận chủ ý).
- Sequence đàn `dan_tu_do` chạy êm, không còn warning audio.

### Ch1 — Gia Biến · 47 câu · 57 fps (38% frame < 60fps — chương nặng nhất)
- Cảnh nhà trưa mưa: ánh đèn dầu + bàn thờ rất có không khí; hộp thoại rõ, dấu chuẩn.
- Góp ý khung hình: khi Mẹ Kiều nói, cô che miệng (gesture) + camera sau lưng Kiều → nhìn lưng người nghe. Đề xuất: shot cận mặt Mẹ lúc thoại buồn.

### Ch2 — Đường theo Mã Giám Sinh · 63 fps · đủ 5 lựa chọn
- **Còn 1 lỗi UI nhỏ**: banner mục tiêu góc trái ("Quan sát và tìm hiểu, nhưng đừng để hắn sinh nghi") **chữ tràn phải bị thanh "Nghi ngờ của Mã Giám Sinh" cắt ngang một phần**. Fix hôm qua thu panel header lại (0.36) nhưng chữ objective dài vẫn tràn ra ngoài rect → cần bật wrap hoặc co chữ. Đây là việc còn lại duy nhất về chồng lấn UI.
- Cảnh observe có reticle + thanh nghi ngờ đọc rõ.

### Ch3 — Lầu Tú Bà · 65 fps (43% < 60fps, camera + đèn nhiều)
- Tú Bà giữa khung, khung hình ổn; **ánh sáng tối** — mặt Tú Bà chìm trong bóng, chân nhân vật bị hộp thoại che. Đề xuất: thêm đèn fill cho shot thoại ở trong lầu.

### Ch4 — Đêm Tú Bà (stealth) · 74 fps · J1/J2 + pickpocket + đốt giấy tờ chạy đủ
- Không khí đèn dầu + bóng tối rất đạt chất stealth. Vấn đề hiển thị: **nhân vật trong gương (J1) quá tối/mờ** gần như không nhận ra; bảng "ĐỊNH MỆNH PHÂN ĐOẠN" che một phần gương. Đề xuất: tăng sáng reflection hoặc hạ bảng xuống.
- Checkpoint restore hoạt động khi bị phát hiện (đã test ở lượt trước).

### Ch5 — Đào thoát (chase) · 64 fps
- Chase đêm rừng tre + mưa + đèn lồng: **khung hình đẹp nhất game** theo đánh giá ảnh; mục tiêu hiện đúng 1 chỗ (đã fix).
- Perf: p99 58ms — chương cần profile khi build thật (cảnh mưa + many lights).

### Ch6 — Kim Trọng · 118 fps (mượt)
- Shot thoại chuẩn kỹ thuật: người nói 1/3 khung, looking room đúng, dấu chuẩn. Không góp ý gì lớn.

### Ch7 — Huyền Đường (tòa) · 101 fps · trial r1/r2/r3 + lục bát chạy đúng
- UI tòa đầy đủ, hint hiển thị trọn (đã fix). Nhỏ: label vàng "Trang Sách" nhỏ trên một ghế nhìn hơi lạ (nhãn world trùng màu UI — chỉ là cảm quan).

### Ch8 — Epilogue + Ending · 96 fps
- Màn "HẾT — THÚY KIỀU: NGHỊCH MỆNH" đủ dấu, trình bày cảm xúc, recap lựa chọn rõ. Đẹp.

## Hiệu năng tổng (Editor Play Mode, có capture)

| Chương | FPS TB | % frame <60 | Nhận xét |
|---|---|---|---|
| C0 | 100 | 3.9% | ổn |
| C1 | 57 | 38.2% | nặng nhất — p99 110ms, cần profile khi build |
| C2 | 63 | 31.5% | khá nặng |
| C3 | 65 | 43.2% | nặng (nhiều đèn nội thất) |
| C4 | 74 | 20.5% | stealth dài, chấp nhận |
| C5 | 64 | 21.2% | mưa + chase |
| C6 | 118 | 1.4% | mượt |
| C7 | 101 | 1.9% | mượt |
| C8 | 96 | 5.7% | mượt |

Lưu ý: đây là Editor Play Mode + đang chụp ảnh; số thật khi build player sẽ tốt hơn.
Khuyến nghị: profile C1/C2/C3 bằng Player build (Unity Profiler) trước khi phát hành.

## Việc còn lại (ưu tiên giảm dần)

1. **Ch2: chữ mục tiêu tràn ra ngoài panel header** — bật word-wrap/co chữ cho Objective text (lỗi UI cuối cùng còn sót).
2. **C4: nhân vật trong gương J1 quá tối** — tăng cường độ reflection hoặc bố trí lại bảng Định Mệnh.
3. **C3: Tú Bà thiếu đèn fill mặt** khi thoại trong lầu.
4. **C1: shot thoại Mẹ Kiều** nên có cận mặt thay vì lưng Kiều (2 shot do sequence dàn dựng: L026/L043 tương tự ở Ch0 — cùng nhóm việc "camera do code dàn dựng").
5. Perf: profile C1/C2/C3 trên build thật.
