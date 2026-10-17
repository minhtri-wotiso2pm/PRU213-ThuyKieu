# Chương 1 — Đồng bộ với các chương sau (P34–P36)

Chương 1 (`Chapter01_GiaBien.unity`) trước đây là scene dựng tay chạy luồng riêng (`Chapter01Director`, khung thoại `Chapter01DialogueCanvas`, ba điểm "bấm E", Ink tuyến tính). Nay chương dùng chung hệ thống với Mở đầu và Ch2–Kết. **Ngôi nhà dựng tay giữ nguyên**: mái, tường, đèn, mưa, đồ trang trí, ánh trăng. Chỉ phần hệ thống, người chơi, camera và nhân vật được tạo lại.

## 1. Những gì giờ giống các chương khác

| Phần | Trước | Nay |
|---|---|---|
| Khung thoại | `DialogueUI` riêng của Ch1 | `StoryCanvas` dùng chung: tên người nói, kiểu chữ Nội Tâm / Kiều Thật / Trang Sách, tốc độ chữ, tự chạy, đồng hồ lựa chọn |
| Chữ hướng dẫn cơ chế | Một dòng "WASD · E" cố định | Thẻ hướng dẫn `tutorial_house`, `tutorial_suspicion`; HUD mục tiêu qua tag `# objective`; danh sách việc trong hub; dòng phím dưới màn hình |
| Camera | Preset `DialogueTagRouter` | `DialogueCameraRig` (CU/MCU/OTS tự động + neo `CAM_*`), đèn key khi thoại, điểm đứng khi tương tác |
| Tương tác | `ChapterInteractable` theo giai đoạn | `ExploreTarget` / `ObservableTarget` chọn lựa chọn `#explore` của Ink |
| Hệ thống | — | Tạm dừng (Esc), sổ tay manh mối (J), Trang Sách (B), Định Mệnh, thẻ thất bại + checkpoint, **lưu giữa chương** ("Tiếp tục" từ menu), bảng tổng kết chương |
| Âm thanh | `Chapter01Audio` lo cả nhạc | `StoryAudio` lo nhạc + hiệu ứng theo tag Ink; `Chapter01Audio` chỉ còn mưa/gió trong–ngoài nhà, bước chân gỗ/đá, tiếng cửa |

## 2. Nội dung mới (Ink `Chapter1_GiaBien.ink`, ≈ 10–12 phút)

1. **Đêm mưa**: dòng mở đầu giữ nguyên. Nội Tâm phản ứng theo hành động ở Mở đầu (`pl_hanh_dong`). Kiều Thật thách cô học sinh.
2. **Hub trong nhà** (mục tiêu *Tìm hiểu tình cảnh gia đình trước khi khách đến*):
   - Ngồi bên Mẹ Kiều → biết chuyện bà mối (`c1_biet_moi`).
   - Mở tủ gỗ → nhà chỉ còn chưa tới 30 lạng, bản sao lời khai không tên.
   - **Quan sát** khoảng sân bị lục soát (giữ chuột phải) → sổ hàng của Vương Ông.
   - Thắp hương bàn thờ → Kiều Thật, `soul(1)`.
   - Biết chuyện bà mối và làm ≥ 2 việc → có tiếng gõ cổng → **Ra mở cổng**.
3. **Mã Giám Sinh đến**: nửa trang `ts_ma_mat`, lựa chọn hẹn giờ phản ứng đầu tiên.
4. **Hub dạm hỏi** với **thanh nghi ngờ** `c1_nghi_ngo` (0–100):
   - Mã Giám Sinh đứng bên bàn, cứ 6–10 giây lại liếc Kiều 2 giây.
   - Quan sát hắn đúng lúc hắn nhìn lại: +24/giây. Không quan sát: −3/giây.
   - Đủ 100 → thẻ thất bại, về checkpoint đầu hub, thanh hạ còn 45.
   - Các việc trong hub:
     - Quan sát Mã Giám Sinh.
     - Đáp lời: lựa chọn hẹn giờ có 4 hướng (hỏi khéo / hỏi thẳng / hỏi chuyện tiền / im lặng). Hỏi khéo gợi câu "Hỏi tên rằng: Mã Giám Sinh…".
     - Xem túi bạc dấu **Phúc Ký**, gài trước sổ hiệu bạc Phúc Ký ở Ch6.
     - Hỏi tên tùy tùng ngoài hiên.
     - Sau khi đã đáp lời: **Ngồi vào bàn, xem tờ khế**.
5. **Tờ khế**: đọc kỹ (thẻ tài liệu "Tờ hôn ước", `doc_ky_khe`) rồi chọn trong thời gian giới hạn:
   - Đòi ghi bạc giao trước (chỉ có khi đã hỏi chuyện tiền).
   - Đòi xóa câu "toàn quyền định đoạt".
   - Giả vờ không hiểu.
   
   Hoặc **ký ngay** (`tu_trong − 1`, Kiều Thật lên tiếng).
6. **Quyết định**: giữ nguyên lời Mẹ Kiều và Kiều, `soul(tu_trong / 2)`, `ts_hieu` khi `tinh_tao ≥ 3`.
7. **Lên đường**: hub cuối (ôm Mẹ lần cuối, tùy chọn → ra cổng). Có 6 manh mối `c1_*`.

**Biến**: giữ nguyên ý nghĩa `tinh_tao`, `tu_trong`, `doc_ky_khe`, `ts_ma_mat`, `ts_hieu`; thêm `c1_viec`, `c1_biet_moi`, `c1_manh_moi`, `c1_nghi_ngo`, `c1_hoi_xong`, `c1_hoi_tien`, `c1_bac_dau`, `c1_tam_biet`. Hệ quả sang Ch2: kết chương với `c1_nghi_ngo ≥ 60` thì Mã Giám Sinh "để mắt tới nàng" và Ch2 bắt đầu với `nghi_ngo = 15`. Bàn thờ cho thêm tối đa +1 `dong_dieu` so với bản cũ.

Bản Ink cũ lưu ở `Ink/Legacy/Chapter1_GiaBien_v1.ink`.

## 3. Mã nguồn

- `Gameplay/Chapters/Chapter01World.cs`:
  - dàn cảnh theo tag `scene:` (`VuongGia_DemMua`, `_MaDen`, `_DamPhan`, `_BanKyKhe`, `_RaDi`);
  - thanh nghi ngờ, tiếng gõ cổng;
  - túi bạc và tờ khế xuất hiện đúng lúc;
  - tổng kết chương.
- `Core/Editor/Chapter01StoryUpgrade.cs` (menu **ThuyKieu ▸ Chapter 1 ▸ Apply story systems**, batch `ThuyKieu.Core.Editor.Chapter01StoryUpgrade.ApplyBatch`):
  - xóa rồi tạo lại `StorySystems`, người chơi, camera, `StoryCanvas`, Mẹ Kiều, Mã Giám Sinh, tùy tùng, các điểm tương tác, neo camera và đạo cụ (`Chapter01Story`);
  - nối lại mọi tham chiếu từ ngôi nhà (cửa, âm thanh…) sang đối tượng mới;
  - chạy lại bao nhiêu lần cũng được.
- `StoryBuild.Rebuild` gọi upgrade ở cuối. Lý do: rebuild xóa rồi tạo lại `StoryCanvas.prefab` và các controller dùng chung; upgrade nối lại chúng cho Ch1, còn scene Ch1 không bao giờ bị tạo lại.
- Vật liệu sinh ra nằm trong `Materials/Chapter01Look` vì thư mục `Materials/Story` bị xóa mỗi lần rebuild.
- `Chapter01Atmosphere` (đèn theo khoảnh khắc) và `Chapter01Audio` nay nghe `StoryDirector`.
- `ObserveMode` tìm cả mục tiêu đang ẩn lúc vào chương, vì Mã Giám Sinh xuất hiện muộn.
- Autopilot QA:
  - chạy được Ch1;
  - Mở đầu → Ch1 → Ch2… nối chuỗi với `-autopilotChain`;
  - in các biến `c1_*`.

**Đã gỡ** (luồng Ch1 cũ, không còn dùng): `Chapter01Director`, `ChapterInteractable`, `DialogueTagRouter`, `Chapter01DialogueCanvas.prefab`, cùng các công cụ dựng/kiểm tra một lần `Chapter01Setup`, `Chapter01CameraSetup`, `Chapter01EnvironmentSetup`, `Chapter01EnvironmentCheck`, `Chapter01PlayModeCheck` (bộ hồi quy 103 kiểm tra), `MaGiamSinhAnimationSetup`, `Chapter01SharedAnimationSetup.Apply`. Bộ hồi quy cũ được thay bằng autopilot (xem mục 4).

## 4. QA

```
qa.ps1 -Scene Chapter01_GiaBien -Thorough [-Capture] [-EveryLine]
```

- Lưu giữa chương: chạy với `-autopilotMidSave -autopilotStopAtExplore 6`, sau đó chạy lại với `-autopilotResume`.
- Kiểm tra Ink: `InkStoryValidator.RunBatch`. Báo cáo ghi phân bố `c1_manh_moi`, kiểm tra `c1_nghi_ngo` nằm trong 0–100, và việc chuyển từ Mở đầu sang Ch1.

### Kết quả (2026-10-09)

- Autopilot Ch1 `-Thorough`: seed 1–5 và 8 **PASS**. Mọi điểm khám phá đều được dùng; lần ép thất bại khôi phục đúng checkpoint.
- Lưu giữa chương: dừng ở lần khám phá thứ 6 (hub dạm hỏi), sau đó `-autopilotResume` **PASS**. Mã Giám Sinh được dàn cảnh lại; nhạc và mục tiêu được phục hồi.
- Chạy nối chuỗi cả game từ Mở đầu (seed 7, `-Thorough -Chain`) **PASS** tới cảnh kết. Ch1 đạt trung bình 369 fps, khung chậm nhất 21 ms (640×480, batch). Manh mối Mở đầu → Ch1 → Ch2… được mang theo.
- Kiểm tra Ink **PASS**: Ch1 có 1500 lượt, 387–508 dòng; Mở đầu → Ch1 đúng 50/50.
- Soát camera từng dòng (`-EveryLine`, 82 ảnh): sau đó đã thêm điểm đứng khi hỏi tùy tùng (trước đó chỉ thấy gáy Kiều), đưa Mẹ Kiều lại gần cửa và làm sáng túi bạc.
- Voice: manifest 978 dòng. Sinh mới 97 clip cho Ch1 (edge-tts), 16 clip của dòng cũ bị xóa.

**Chưa kiểm**: chưa chạy trọn `StoryBuild.Rebuild` để thử nhánh nối lại prefab, vì nó dựng lại toàn bộ scene Ch2–8. Upgrade đã được chạy 2 lần liên tiếp trên scene Ch1 mà không mất tham chiếu nào, và có xử lý prefab bị mất của `StoryCanvas`.

