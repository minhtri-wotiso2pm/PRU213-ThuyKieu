# Chapter 5 Foley và nhạc truy đuổi — nguồn và quyền sử dụng

Âm của cuộc chạy rừng tre Chương 5. Các file trong `Ch5Foley/<id>/` và `../Ch5Music/chase_level*.wav` đều do công cụ `Assets/_Game/Scripts/Core/Editor/Ch5Foley*.cs` dựng từ bản thu bên dưới. Bản gốc nằm trong `Ch5Foley/Source/` và `Ch5Music/Source/`; kho tải ở `ThuyKieu_Share/FoleyDownloads/`. Báo cáo cắt/ghép/mức âm: `Tools/Ch5/QA/Ch5FoleyBuild.txt` (đo bằng số, chưa ai nghe bằng tai).

| Nguồn | Tác giả | Link | Giấy phép | Dùng cho |
|---|---|---|---|---|
| Footsteps on different surfaces (`footsteps_0.zip`), nhóm `gravel` 0–9 | congusbongus (biên tập), gốc: **Gravel Footsteps** — Ali_6868 | https://opengameart.org/content/footsteps-on-different-surfaces · gốc https://freesound.org/people/Ali_6868/packs/21608/ | Trang gói **CC BY 3.0**; `gravel/license.txt` ghi bản gốc Ali_6868 **CC0** | Lớp sỏi dưới tiếng đất: `step_earth*`, `guard_step`, `jump_earth`, `land_earth`, `trip_earth`. Người chơi đã nghe bản preview ghép 10 take và chấp nhận chất âm; trong game dùng từng take riêng, không dùng file preview |
| chop-tree-fall.ogg (195 KB) | kheetor | https://opengameart.org/content/tree-chop-fall-thud | **CC0** | `bamboo_crack`, `bamboo_sweep`, `bamboo_impact`, `leaf_settle`, `rope_snap`. **Bỏ toàn bộ đoạn tiếng rìu chặt** |
| Juggernaut (MP3 320 kbps, 8,9 MB, full mix, không có stem) | Scott Buckley | https://www.scottbuckley.com.au/library/juggernaut/ | **CC BY 4.0** | Ba vòng lặp nhạc truy đuổi `chase_level0–2`. Ghi công bắt buộc (đã có trong màn Credits): `'Juggernaut' by Scott Buckley - released under CC-BY 4.0. www.scottbuckley.com.au` |
| Kenney Impact Sounds, Kenney RPG Audio (đã có từ Chương 4) | Kenney Vleugels | https://kenney.nl/assets/impact-sounds · https://kenney.nl/assets/rpg-audio | **CC0** | Tiếng đất (`footstep00–09`), ván gỗ, đá, va gỗ/ván/mềm, kẽo kẹt (xem `Ch4Foley/CREDITS.md`) |

Tải ngày 25/10/2026 với sự cho phép của người dùng (Juggernaut và chop-tree-fall). Bộ sỏi do người dùng/Codex tải trước đó.

## Chỉnh sửa

- Trộn về mono (riêng nhạc giữ stereo), 44,1 kHz.
- Cắt mỗi tiếng từ ngay trước lúc chạm tới hết đuôi. Fade vào 2 ms, fade ra mềm.
- Chỉnh mức theo RMS 120 ms đầu, giữ 1 dB headroom đỉnh, không peak-normalize.
- **Ghép lớp** (đều là bản thu):
  - bước Kiều = đất Kenney + sỏi Ali_6868 ở −9 dB (cúi: −13 dB);
  - bước lính = take đất nặng hơn + sỏi −6 dB, trong game hạ cao độ ×0,84–0,92;
  - đáp = hai chân cách 40–45 ms + sỏi;
  - vấp = va mềm + đất;
  - va cọc tre = va gỗ trung nâng ×1,15;
  - cầu = va gỗ nặng/ván + kẽo kẹt hạ cao độ.
- **Tre đổ**: tìm điểm chạm lớn nhất ở nửa sau file, các cú đánh sắc trước đó (tiếng rìu) bị loại. Lấy đoạn kẽo kẹt (hai bản), đoạn lao xuống ngay trước chạm, cú chạm (nâng ×1,12 cho thân tre rỗng nhẹ hơn), và lá lắng sau chạm. Mốc thời gian cụ thể trong báo cáo dựng.
- **Nhạc**: đo nhịp và phách đầu ô nhịp. Chọn ba đoạn 8 ô nhịp (êm nhất mà vẫn đủ dày, to nhất, và một đoạn giữa), bỏ intro và kết bài. Mỗi đoạn nướng thành vòng lặp liền: ô nhịp ngay sau được crossfade một phách vào đầu đoạn. Trong game chỉ đổi đoạn ở vạch ô nhịp, crossfade một phách, không phát chồng nhiều bản full mix.

## Còn là âm tổng hợp

`cloth` (vải lúc nhảy, để rất nhỏ), `breath_in`, `breath_startled`, `heartbeat`, `gong` (tiếng báo động đầu chương) vẫn là âm tạo bằng mã từ trước. Chưa có bản thu hơi thở giọng nữ hợp lệ.
