# P33 — Âm thanh dựng sẵn, nhạc cho nhánh mới, voice, dàn nhân vật hiện đại

## 1. Hết khựng do âm thanh

Nguyên nhân các lần khựng 0,5–1,2 s (đo bằng dòng `HITCH` trong báo cáo autopilot, kèm dòng `TAG` ngay trước):
`ProceduralAudio` tổng hợp nhạc/tiếng nền 30–40 s **ngay trên luồng chính** lúc đổi cảnh. Ví dụ Ch6 phố huyện:
tiếng nền `street` (14 giọng rì rầm × 40 s) mất ~1,2 s.

Bây giờ:

- `Presentation/AudioBank.cs` tìm clip theo id: `Resources/StoryAudio/Recorded/<kind>/<id>` (thu sẵn) →
  `Resources/StoryAudio/<kind>/<id>` (dựng sẵn) → tổng hợp (chỉ khi thiếu, có cảnh báo).
  `kind` = `music` / `amb` / `sfx`. `StoryAudio` và `Chapter01Audio` đều đi qua đây.
- `Core/Editor/AudioBankBaker.cs` ghi mọi id trong `ProceduralAudio` ra WAV (menu *ThuyKieu > Audio > Bake procedural audio*,
  batch `AudioBankBaker.BakeBatch`). Tự chạy trước mỗi lần `StoryBuild.BuildWindows`.
  Nhạc và tiếng nền import dạng **Streaming** (không giải nén khi nạp), hiệu ứng ngắn giải nén sẵn.
  Đổi công thức một id trong `ProceduralAudio` → xoá file WAV của id đó rồi bake lại.
- QA: autopilot ghi `HITCH <ms>` cho mọi khung > 150 ms và `TAG scene/music/sfx/gameplay` để biết nguyên nhân.

## 2. Âm thanh lấy từ ThuyKieuKiemTien

Chép vào `Audio/Resources/StoryAudio/Recorded` (xem README trong thư mục): `sad_strings`, `morning` (đàn tranh),
tiếng mưa/gió Ch1, tiếng lớp học, giấy, cửa mở/đóng, UI, bước chân gỗ/đá (Ch1), chuông trường, gõ cửa.
Ch1 trước đây để trống mọi ô clip → giờ có đủ.

## 3. Nhạc cho các nhánh Trang Sách

| Chỗ | Nhạc |
|---|---|
| Ch5 gặp Sở Khanh | `so_khanh` (mới, ngọt mà gian) → quay lại `chase` khi vào rừng |
| Ch5 thoát ra, "Mình phải quay lại" | `resolve` |
| Ch7 thua kiện (bóng tối công đường) | `bac_menh` (mới, buồn chậm) |
| Ch7 Mất Mình | im lặng |
| Ch7 Nghịch Mệnh | `nghich_menh` (mới) |
| Kết, nếu thua kiện | `bac_menh` thay cho `epilogue` (tag động `# music:{c7_ket == 0:bac_menh|epilogue}`) |

Validator Ink báo lỗi nếu `# music:` / `# sfx:` không có âm thanh.

## 4. Dàn nhân vật hiện đại (Mở đầu, Kết)

- `Core/Editor/ModernCharacterImport.cs` đọc 3 OBJ của ThuyKieuKiemTien (1,5 triệu tam giác mỗi cái), giảm bằng bộ tạo
  Mesh LOD của Unity (giữ nguyên UV), lưu mesh + prefab + vật liệu (có normal map) ở `Art/Characters/Modern/<tên>`:
  ThayGiao (~38k tam giác, đứng), HocSinhNgoi (~34k, nam sinh ngồi), NuSinhToxic (~39k, nữ sinh ngồi).
  Chạy lại: *ThuyKieu > Tools > Import modern characters* (cần project ThuyKieuKiemTien nằm cạnh).
- `StorySceneKit.ModernActor(...)` đặt model; `seat: true` tự đặt hông lên ghế băng sau điểm ngồi.
- Ngân = NuSinhToxic ngồi; 8 bạn cùng lớp = HocSinhNgoi/NuSinhToxic, tô màu nhẹ; giáo viên = ThayGiao.
- Model tĩnh không đi được: khi tan học, `PrologueWorld.SlipAway` cho họ biến mất lúc camera không nhìn.
- **Giáo viên đổi từ "Cô giáo" sang "Thầy giáo"** cho khớp model (speaker `ThayGiao`, giọng nam; lời thoại đổi "cô" → "thầy").
- Người chơi (nữ sinh) vẫn dùng rig cũ: model NuSinhToxic chưa có xương, cần rig Mixamo rồi mới thay được.

## 5. Voice

`VoiceManifestExporter.RunBatch` rồi `python Tools/Voice/generate_voice.py` (edge-tts, cần mạng).
