# Chapter 4 Foley — nguồn và quyền sử dụng

Bộ âm bước chân và cửa của Chương 4 (`Ch4FoleyBank.asset`), dựng từ bản thu bên ngoài. Riêng tiếng thở/ngáy của Tú Bà dùng giấy phép khác (**CC BY-SA**, xem mục cuối). Tất cả nguồn bước chân và cửa đều **CC0 1.0** (public domain, https://creativecommons.org/publicdomain/zero/1.0/): được dùng, sửa và phát hành tự do, không bắt buộc ghi công. Vẫn ghi công dưới đây để nhóm tra lại nguồn.

Tải ngày 19/10/2026, với sự cho phép của người dùng. Bản gốc của các file được dùng nằm trong `Source/` (kèm `License.txt` của Kenney). Kho tải đầy đủ để ngoài project, ở `ThuyKieu_Share/FoleyDownloads/`.

| Gói | Tác giả | Link | Giấy phép | File dùng |
|---|---|---|---|---|
| Impact Sounds (130 file, `kenney_impact-sounds.zip`, 801 KB) | Kenney Vleugels (kenney.nl) | https://kenney.nl/assets/impact-sounds | CC0 (`Source/kenney_impact/License.txt`) | `footstep_wood_000–004`, `footstep_concrete_000–004`, `footstep_carpet_000, 001, 003` (002 trùng 001, 004 trùng 003), `impactWood_light/medium/heavy_000–004`, `impactPlank_medium_000–004` |
| RPG Audio (50 file, `kenney_rpg-audio.zip`, 965 KB) | Kenney Vleugels (kenney.nl) | https://kenney.nl/assets/rpg-audio | CC0 (`Source/kenney_rpg/License.txt`) | `footstep00–09`, `creak1–3`, `doorOpen_1`, `doorClose_3`, `doorClose_4`, `metalLatch`, `metalClick` |
| 25 CC0 mud sfx (`25-CC0-mud-sfx.zip`, 526 KB) | rubberduck (OpenGameArt) | https://opengameart.org/content/25-cc0-mud-sfx | CC0 | `mud_06, 08, 10, 11, 13, 24, 25` |
| Door Open, Door Close Set (`qubodup-DoorSet.7z`, 3,3 MB) | qubodup (OpenGameArt) | https://opengameart.org/content/door-open-door-close-set | CC0 | bản `ogg`: `DoorOpen01, 05, 06, 08`, `DoorClose02, 04, 07, 08` |

Các file khác đã chép vào `Source/` để tham khảo nhưng không dùng: `footstep_grass_*`, `impactSoft_*`, `creak` khác, `doorOpen_2`, `doorClose_1–2`, các `mud_*` khác, các cửa qubodup khác. Không dùng các cửa có tiếng bản lề kim loại rít (`DoorOpen02`, `DoorOpen04`, `DoorClose05`).

## Chỉnh sửa (công cụ `Assets/_Game/Scripts/Core/Editor/Ch4Foley.cs`)

- Trộn về mono.
- Cắt từ 2 ms trước lúc chạm tới hết đuôi (−40 dB so với đỉnh, cộng 30 ms), có giới hạn độ dài theo loại âm.
- Fade vào 2 ms và fade ra mềm (tối đa 25 ms), để không có tiếng click.
- Chỉnh mức theo RMS của 120 ms đầu: mọi bước chân −21 dBFS; cửa và then theo từng nhóm. Giữ 1 dB headroom đỉnh. Không peak-normalize.
- Ghép duy nhất một loại: **đá ướt** = một bước `footstep_concrete` + một đoạn `mud` ngắn ở −9 dB (cả hai đều là bản thu).
- Không đổi pitch khi dựng. Trong game có biến thiên ±2 %.
- Trong game, mỗi nhóm phát ở mức bằng âm cũ mà nó thay (`Gain` của nhóm trong bank, đo theo 120 ms đầu), để cân bằng với mưa và nhạc không đổi.

Ghi chú bản thu nào dùng cho id nào và mức đo được: `Tools/LauTuBa/QA/FoleyBuild.txt`. File so sánh cũ/mới để nghe: `Tools/LauTuBa/Foley/AB_<id>.wav`.

## Còn là âm tổng hợp hoặc thiếu nguồn thu

- `slide_soft` (trượt cửa giấy khẽ): không tìm được bản thu CC0 tiếng trượt cửa gỗ/giấy. Vẫn là âm tổng hợp cũ.
- `latch_lift`, `bolt_slow`, `lock_rattle`: vẫn là âm tổng hợp cũ.
- Cửa trượt mạnh (`slide_fast`, `slide_slam`) chỉ có tiếng gỗ va khung (`impactWood`), không có tiếng trượt.

## Tiếng thở và ngáy của Tú Bà (`snore_deep`, `snore_out`): CC BY-SA

Đây là bản thu người thật trên Wikimedia Commons. Tải ngày 20/10/2026, được người dùng cho phép. Bản gốc ở `Source/commons_snore/`; kho tải ở `ThuyKieu_Share/FoleyDownloads/Snore/`.

- Không có bản CC0 nào phù hợp, và không có bản thu giọng nữ.
- Cả bốn bản thu đều là giọng nam.
- Giấy phép **CC BY-SA**: phải ghi tác giả, và các đoạn đã biên tập dưới đây (`snore_deep_*.wav`, `snore_out_*.wav`) cũng phải phát hành theo CC BY-SA cùng phiên bản.

| File | Tác giả | Link | Giấy phép | Dùng |
|---|---|---|---|---|
| `Tongue_base_snoring.ogg` (527 KB, 150 s, mono 8 kHz) | Langenbergstefan | https://commons.wikimedia.org/wiki/File:Tongue_base_snoring.ogg | CC BY-SA 3.0 (https://creativecommons.org/licenses/by-sa/3.0/) | in-breath 01–06, out-breath 01–06 |
| `Palatal_snoring_1.ogg` (402 KB, 118 s, mono 8 kHz) | Langenbergstefan | https://commons.wikimedia.org/wiki/File:Palatal_snoring_1.ogg | CC BY-SA 3.0 | in-breath 07–10, out-breath 07–10 |
| `Palatal_snoring_2.ogg` (191 KB, 56 s, mono 8 kHz) | Langenbergstefan | https://commons.wikimedia.org/wiki/File:Palatal_snoring_2.ogg | CC BY-SA 3.0 | in-breath 11–12, out-breath 11–12 |
| `Snore_man_50_years_160_kg.ogg` (264 KB, 28 s, stereo 22 kHz) | Vladlen666 | https://commons.wikimedia.org/wiki/File:Snore_man_50_years_160_kg.ogg | CC BY-SA 3.0 | Tải về nhưng không dùng. Nhịp quá ngắn: sau mỗi tiếng ngáy chỉ yên 0,4–1,3 s, nên không có đoạn thở ra rõ ràng. |

**Đã chỉnh sửa (công cụ `Assets/_Game/Scripts/Core/Editor/Ch4Snore.cs`):**

- Mỗi chu kỳ thở được tách thành hai đoạn, cắt theo nhịp thật của bản thu:
  - tiếng ngáy hít vào: từ 50 ms trước khi ngáy tới 100 ms sau khi hết ngáy;
  - hơi thở ra ngay sau đó: dừng 0,3 s trước tiếng ngáy kế tiếp, và không dài quá 2,52 s.
- Chỉ chọn những chu kỳ đều: tiếng ngáy dài 0,95–1,6 s và sau đó yên ít nhất 2,2 s.
- Nâng cao độ 1 bán cung, chuyển sang 22,05 kHz.
- Lọc thông thấp hai lần dưới dải gốc (khoảng 3,8 kHz với bản 8 kHz), để không có tiếng rít do đổi tần số lấy mẫu.
- Fade vào và ra cho từng đoạn.
- Âm lượng:
  - mỗi đoạn ngáy được chỉnh để 300 ms to nhất đạt −16 dBFS;
  - đoạn thở ra dùng cùng mức chỉnh với tiếng ngáy của nó, rồi nâng thêm tối đa +9 dB để nghe được khi đứng cạnh sập (khoảng −31 dBFS);
  - giữ 1 dB khoảng trống trên đỉnh.
- Trong game, mỗi lần hít vào kéo dài đúng bằng đoạn ngáy được chọn (1,04–1,65 s). Thở ra luôn là 2,6 s.

Từng đoạn và mức đo: `Tools/LauTuBa/QA/SnoreBuild.txt`. File so sánh để nghe: `Tools/LauTuBa/Foley/AB_snore.wav` (4 nhịp tổng hợp cũ, 1 s im lặng, rồi 12 nhịp mới đặt theo đồng hồ trong game).

**Chưa ai nghe kiểm** chất âm của các đoạn này. Claude không nghe được âm thanh. Mọi số liệu trên chỉ là đo tín hiệu.

## Sỏi ném (`pebble_hard`, `pebble_wood`, `pebble_soft`)

Tải ngày 21/10/2026, được người dùng cho phép. Các file dùng được chép vào `Source/pebble/`; kho tải đầy đủ ở `ThuyKieu_Share/FoleyDownloads/Pebble/`. Mỗi take chỉ là **một lần chạm**. Game phát lần chạm đầu và từng lần nảy thành từng take riêng tại chỗ viên sỏi chạm, nhỏ dần khi viên sỏi chậm lại. Không take nào chứa sẵn một chuỗi nảy.

| File gốc | Tác giả | Link | Giấy phép | Dùng cho |
|---|---|---|---|---|
| `Stone-dropping-6843.wav` (50 KB, 0,53 s, stereo 24 kHz) | alegemaate | https://commons.wikimedia.org/wiki/File:Stone-dropping-6843.wav | **CC BY-SA 4.0** (https://creativecommons.org/licenses/by-sa/4.0/) | `pebble_hard_01–03`: ba lần chạm đá-trên-đá tại 0,051 / 0,251 / 0,334 s |
| `sfx100v2_stones_01–03.ogg` (gói `sfx_100_v2.zip`, 2,4 MB) | rubberduck (OpenGameArt) | https://opengameart.org/content/100-cc0-sfx-2 | CC0 | `pebble_hard_04–06`: lần chạm đầu của mỗi file |
| `bfh1_rock_falling_04.ogg`, `bfh1_rock_falling_08.ogg`, `bfh1_rock_hit_01.ogg` (gói `sfx_breaking_and_falling.zip`, 1,6 MB) | rubberduck (OpenGameArt) | https://opengameart.org/content/75-cc0-breaking-falling-hit-sfx | CC0 | `pebble_hard_07–09` |
| `bfh1_wood_hit_03.ogg`, `sfx100v2_wood_hit_03.ogg` | rubberduck (OpenGameArt) | như hai dòng trên | CC0 | `pebble_wood_01–02`: **tạm thay**, là tiếng một vật gõ lên gỗ, không biết có phải viên đá không |
| `impactSoft_medium_000, 002, 003.ogg` (Kenney Impact Sounds, đã có trong `Source/kenney_impact/`) | Kenney Vleugels | https://kenney.nl/assets/impact-sounds | CC0 | `pebble_soft_01–03`: **tạm thay**, là tiếng va mềm, không phải viên sỏi rơi xuống chiếu hay đất |

`Stone-dropping-6843.wav` là bản trên Wikimedia Commons, ghi giấy phép CC BY-SA 4.0. Bản Freesound 364711 cùng tác giả ghi CC0, nhưng phải đăng nhập mới tải được nên chưa tải. Vì file được dùng là bản Commons nên áp theo giấy phép của Commons:
- `pebble_hard_01.wav`, `pebble_hard_02.wav` và `pebble_hard_03.wav` phải ghi tác giả (alegemaate, Wikimedia Commons);
- ba file này phải phát hành theo CC BY-SA 4.0.

Tải về nhưng không dùng:
- `Small_pebbles_on_concrete.ogg`: Wikimedia Commons, earthcalling / PDSounds, public domain. File là định dạng Ogg có Skeleton/Theora, Unity không nhập được (lỗi import). Bản sao trong `Source/pebble/` đã xoá ngày 22/10/2026 theo yêu cầu người dùng; bản tải gốc vẫn ở `FoleyDownloads/Pebble/`.
- Các file khác trong `Source/pebble/` (`bfh1_falling_*`, `bfh1_hit_*`, phần còn lại của `rock_falling`, `wood_hit_01–02`, `sfx100v2_hit_*`, `wood_hit_01–02`): chép vào để đo, không dùng.

**Chỉnh sửa (công cụ `Assets/_Game/Scripts/Core/Editor/Ch4Pebble.cs`):**
- Tìm đúng điểm chạm bằng đường bao 2 ms. Cắt từ 3 ms trước lúc chạm tới trước lần chạm kế tiếp 5 ms. Độ dài tối đa: đá 0,14 s, gỗ 0,12 s, mềm 0,1 s.
- Fade vào 2 ms, fade ra tối đa 25 ms. Trộn về mono.
- Chỉnh mức theo RMS 40 ms đầu: đá −18 dBFS, gỗ −20 dBFS, mềm −30 dBFS. Giữ 1 dB headroom đỉnh, nên vài take nhọn bị nhỏ hơn mức đặt khoảng 2–4 dB.
- Không đổi pitch khi dựng. Trong game có biến thiên ±6 %.

Số đo từng take: `Tools/LauTuBa/QA/PebbleBuild.txt`. Số đo các ứng viên: `Tools/LauTuBa/QA/PebbleSources.txt`.

**Còn thiếu:**
- Chưa có bản thu thật nào của viên sỏi rơi lên **sàn gỗ** hay lên **chiếu / đất / bùn**. Bản Freesound 740544 (FOSSarts) và 570976 (dichardson) cần đăng nhập, chưa tải.
- `pebble_wood` và `pebble_soft` chỉ là bản tạm thay, ghi rõ ở trên.

**Chưa ai nghe kiểm** chất âm của các đoạn này, kể cả khi trộn với mưa và ở khoảng cách chơi thật. Mọi số liệu trên chỉ là đo tín hiệu.
