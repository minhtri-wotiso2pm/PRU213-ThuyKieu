# Chương 5 · Đào thoát trong rừng tre — Tài liệu bàn giao

- Brief: `docs/concept/CLAUDE_CH5_BAMBOO_CHASE_REBUILD.md`; concept: `docs/concept/chapter5-bamboo-chase-concept.png`.
- Project: `D:\PRU213\ThuyKieu_Share-20261009T182626Z-1-001\ThuyKieu_Share\ThuyKieuGame_With_Unity`, nhánh `dev`, HEAD `021f57d`, Unity `6000.6.0f1`. Chưa commit/push/merge.
- Scene: `Assets/_Game/Scenes/Chapter05_Escape.unity`, **sửa tại chỗ**, GUID giữ nguyên `9a169de324da76b4b88b8013bf07bc3b`. Sân sau, phố, chợ và rig lính (`Chapter05PursuerSetup`) không bị dựng lại.
- Hai video YouTube trong brief: **chưa xem** (không có công cụ xem video trong phiên này). Thiết kế dựa trên brief, không dựa trên nội dung video.

## 1. Chơi thử

| Cách | Làm gì |
|---|---|
| Vào thẳng | Mở `Chapter05_Escape` → Play. Màn đen ngắn (~1,3 s trong Editor) rồi hiện ngay góc nhìn Kiều **đang chạy** ở bìa rừng → cầu → chùa trong mưa → Ch6. Không thẻ chương, không thoại bấm tiếp, không hướng dẫn modal (cập nhật §11) |
| Nối chuỗi | Ch4 (`Chapter04_LauTuBaDem`) kết thúc ở cửa sau: Kiều chạy ra cửa, khung hình tối dần trong khi Ch5 nạp ngầm, **không màn tổng kết Ch4** → `Chapter05_Escape` (đang chạy) → `Chapter06_KimTrong` |
| Dựng lại rừng (giữ scene) | Menu **ThuyKieu ▸ Chapter 5 ▸ Rebuild bamboo chase (in place)**, hoặc `-executeMethod ThuyKieu.Core.Editor.Chapter05Forest.ApplyBatch` |

**Điều khiển trong cuộc chạy rừng (góc nhìn thứ nhất):**

| Phím | Tác dụng |
|---|---|
| (tự chạy) | Kiều giữ ý định chạy tiến 5 m/s theo hướng đang nhìn. W/S, Shift không cộng thêm gì, không có thể lực |
| Chuột | Chọn hướng chạy (không có ray, không tự lái, không khoá làn) |
| A / D | Né sang bên |
| Space | Nhảy thật (đỉnh nhảy 1,1 m; nhảy sớm/muộn có biên độ nhỏ), chỉ khi chạm đất |
| Ctrl (giữ) | Cúi chạy 3 m/s dưới tre thấp. Thả tay dưới tre thì vẫn cúi tới khi có chỗ đứng |
| E | Chỉ một việc: đạp đổ cọc cầu khi đứng ở bờ bên kia |
| Esc | Tạm dừng. Mất focus cửa sổ cũng tự tạm dừng |

Ngoài cuộc chạy, Ch5 vẫn là góc nhìn thứ ba như cũ (thoại, cảnh mưa). Chế độ góc nhìn thứ nhất, tự chạy, nhảy, FOV, giới hạn pitch, near clip, độ nhạy chuột, bước chân theo bề mặt, stepOffset, con trỏ, time scale, Stamina, ObserveMode và input mô phỏng đều được lưu khi vào và trả lại khi xong, khi thua/chơi lại hoặc khi scene bị huỷ.

## 2. Tuyến chạy (cố định, học được)

Tổng dài tuyến ~600 m (đo theo lối "khe trăng"), vạch xuất phát ở m 14. Một lượt chạy sạch mất **115 s** (giờ game, đo thực tế).

| Mét | Nhịp |
|---|---|
| 0–54 | Bìa rừng, dốc xuống, đoạn lấy đà; lính xuất phát sau 12 m (9 m nếu Sở Khanh hô hoán) |
| 62 | **Học nhảy**: khúc tre đổ ngang đường (đỉnh 0,43 m) |
| 104 | **Học cúi**: tre nghiêng bắc qua đường, mép dưới 1,45 m |
| 134 → 150 | Tre mục **rạn – đung đưa – đổ** ngang đường khi Kiều giẫm vùng rễ cách 16 m; đổ xong sau 1,96 s (đo); thành chỗ phải nhảy |
| 190 | **Ngã rẽ** thấy trước ~20 m, có tảng đá chia: trái là *lối khe trăng* (rộng, có đèn trăng rọi, một rãnh phải nhảy); phải là *lối tre rậm* (hẹp, tối, có đom đóm, 2 chỗ cúi + 1 cây đổ). Hai lối nhập lại |
| hợp + 10 → +142 | **Phối hợp** có đoạn lấy lại nhịp: khe đá hẹp 1,5 m (lệch tâm), cây đổ, tre thấp, tre đổ thứ hai, rãnh thứ hai, cây đổ |
| +150 → +260 | Dốc ngoằn ngoèo xuống, rồi **đoạn thẳng dài**: thấy đèn lồng chùa ở bờ bên kia khe |
| cầu | Cầu ván 13 m qua khe suối sâu 9 m, có tay vịn (có collider, né ngang không rơi). Bờ bên kia đường rẽ gắt trái; **cọc cầu** đứng ngay trước mặt ở góc. E: đạp đổ → đầu xa rơi, mặt cầu đổ úp vào bờ gần |
| cuối | Leo dốc lên cổng chùa Bạch Vân (đèn lồng, sân, chính điện) → kết thúc lượt chạy |

- Hai bên là bờ đất dốc cao 1,45–1,95 m. Người không leo được (dốc > 45°), cũng không nhảy lên được (đã kiểm: sau khi nâng mức tối thiểu thì không còn lối lên).
- Thân tre gần đường có collider bằng đúng thân (phần thấp). Lá, dương xỉ, lá rụng không có collider.
- Va vật thấp (< 0,6 m, như khúc tre đổ) là **vấp**: dừng hẳn, lấy lại đà trong 1 s.
- Va thân tre/vòm tre ngang người là **khựng**: 0,6 s, còn 25% tốc độ.
- Rơi xuống rãnh hoặc khe là thất bại; mép rãnh nhìn thấy được.

## 3. Lính đuổi và bị bắt

- Một lính chính (`GiaNhan0`, rig LinhCanh có sẵn, cầm đuốc thật) chạy theo đường NavMesh thật. NavMesh rừng được dựng nền khi vào chương (~1 s).
- Vượt cây đổ, cúi dưới tre thấp, nhảy rãnh và chạy qua cầu đều là **NavMeshLink có thời gian và tư thế** (`ChaseTraverser`). Vượt cây ~0,8 s cho 3,4 m. Cúi đi 3 m/s, thân gập bằng `GrabPose`, vì rig không có clip cúi. Nhảy rãnh là một cung ở tốc độ chạy.
- Tre đổ xong sẽ khoét NavMesh (carving) và bật link vượt.
- Không dịch chuyển tức thời, không xuyên vật cản.
- Tốc độ người thật, dải hẹp:
  - 4,95 m/s khi đã gần;
  - tối đa 5,35 m/s khi tụt quá 9,5 m;
  - Kiều chạy 5 m/s.
  Khi chạy sạch, hắn bám ~5 m sau lưng. Gần nhất đo được qua 3 lượt sạch là 5,1–5,3 m.
- **Bắt chỉ khi thật sự với tới**, tất cả các điều kiện sau phải cùng đúng:
  - chân cách chân ≤ 1,5 m (hai thân người 0,55 m + cánh tay 0,75 m + rướn);
  - chênh cao ≤ 0,8 m;
  - không có gì đặc chắn giữa ngực hai người;
  - mặt đất giữa hai người không hụt sâu quá 0,9 m (rãnh, khe). Vẫn với qua được một khúc tre đổ;
  - hắn không đang ở giữa cú nhảy qua vật cản. Lúc cúi dưới tre thì vẫn túm được.
- Bị bắt → cảnh bắt góc nhìn thứ nhất của Ch4 (`CatchScene`, kết "lôi đi"), qua overload mới nhận `StoryActor`, không kéo hệ stealth Ch4 vào.
- Âm thanh theo vị trí thật:
  - bước chân lính trên bùn/ván từ chỗ hắn đứng (bản thu Ch4, nghe được 30 m);
  - lời quát bằng giọng đã thu, phát 3D từ người hắn; chữ chỉ hiện khi hắn trong 20 m;
  - hơi thở của Kiều nhanh dần và tiếng tim (khi < 7 m) theo khoảng cách thật.
- Khi Kiều đạp cọc, lính thường đang ở trên cầu. Mặt cầu treo bản lề ở mép gần, nên hắn bị hất về phía bờ gần, bám dây leo lên rồi đứng lại bên kia khe (khớp lời dẫn "bó đuốc dừng lại").

## 4. Cốt truyện, Lam, Sở Khanh, retry

- **Ink** (`Chapter5_DaoThoat.ink`, `Main.ink`):
  - bỏ đoạn lẻn sân bếp (`sneak_out`) và chase ngõ/chợ (`chase_alley`), vì Ch4 đã là màn lén lút và brief tránh hai cuộc chase trùng nhau;
  - giữ báo động, lời Tú Bà và nhánh Sở Khanh (`dinh_menh >= 50`, 3 lựa chọn, diễn ra trước cuộc chạy);
  - giữ lời Lam, cảnh chùa trong mưa, khế ước, ý định quay lại và Kim Trọng nối Ch6;
  - thêm cảnh `Escape_ForestEdge`, thẻ `tutorial_forest` và câu dẫn sau khi cầu đổ.
- **Biến**:
  - `c5_bi_bat`: +1 đúng một lần mỗi lần bị bắt hoặc ngã;
  - mới: `c5_vap` (số lần va ở lượt thoát), `c5_loi` (1 khe trăng · 2 tre rậm), `c5_giay` (giây của lượt thoát);
  - `c5_co_lam_thoat` giữ nguyên;
  - `c5_lang_le`, `c5_duong`, `c5_xo_do`, `c5_qte` vẫn khai báo để save cũ nạp được, nhưng không còn ghi.
  - Đã cập nhật: tổng kết chương (`Chapter05World`), màn kết (`EndingSequence`), danh sách biến autopilot, `InkStoryValidator`.
- **Lam** (`c4_ru_co_lam`): chị nói "biết đường qua khe suối" và chạy trước Kiều ~5 m trên lối khe trăng, vượt vật cản bằng cùng hệ link, không có collider khi chạy (không cản Kiều). Cọc cầu chỉ đạp được khi chị đã qua. Nếu kẹt ngoài tầm nhìn, chị được đặt lại phía trước trên tuyến (kiểm thử: 0 lần). Qua tới bờ bên kia thì `c5_co_lam_thoat = true`. Không thêm màn cứu/QTE (chưa được chốt). Nhánh Ink "chị bị bắt" vẫn còn nhưng hiện không tới được.
- **Sở Khanh**: không đổi lịch và nội dung. Chọn "từ chối" thì lính xuất phát gần hơn (9 m). Hắn được ẩn khi cuộc chạy bắt đầu. Trước đó hắn đứng chắn vạch xuất phát, đây là lỗi tìm ra bằng kiểm thử.
- **Retry**:
  - quay về đầu cuộc chạy rừng, không về đầu chương; không lặp thoại, Sở Khanh hay phần thưởng;
  - đặt lại: Kiều, lính, Lam, tre đổ (dựng lại), cầu (dựng lại), trigger, input;
  - không có checkpoint giữa cuộc chạy;
  - thoát chỉ ghi kết quả một lần.
- Không bật "Trang sách dẫn đường". Đường được làm cho đọc được bằng địa hình, dải bùn ướt, đèn trăng rọi ở lối trái và đèn lồng chùa phía cuối.

## 5. Mỹ thuật và âm thanh

- **Mặt đất và rừng tre được dựng khi nạp scene** (`ForestGround`, `BambooGrove`, `BambooMesh`) từ dữ liệu tuyến trong scene. Lưu mesh ~600k đỉnh dưới dạng asset văn bản (project dùng Force Text) sẽ nặng hàng trăm MB.
  - Chạy cả trong Editor dưới dạng object ẩn không lưu, nên vẫn xem và chụp được khi không Play.
  - Thời gian dựng lúc nạp: đất ~1,6 s, tre ~0,8 s, NavMesh ~1 s nền; khung đầu ~6,3 s gồm cả nạp scene.
- Gồm khoảng 4.800 thân tre thủ tục:
  - đốt tre vẽ bằng texture lặp theo lóng, 3 màu (xanh/vàng ô liu/sẫm);
  - thân gần đường cong và nghiêng vào đường, khép tán phía trên;
  - cụm lá cắt alpha, dương xỉ ở chân bờ, lá rụng trên bùn;
  - khoảng 2.100 collider thân tre; chỉ thân gần đường đổ bóng trăng (lá và tre xa không đổ bóng).
- Có thêm: vũng nước, đá rêu, sương thấp, lá rơi, đom đóm, suối dưới khe, 4 đèn "khe trăng" (spot lạnh, không bóng), đèn lồng chùa ấm.
- Vật liệu: bùn/đất dùng `WorldTriplanarLit` với texture Poly Haven có sẵn (`brown_mud_02`); ván cầu `planks_brown_10`, đá `japanese_stone_wall`/`mossy_rock`. Texture tre/lá/dương xỉ/lá rụng là texture sinh thủ tục, lưu PNG trong `Assets/_Game/Materials/Chapter05Forest`.
- Bước chân Kiều trong cuộc chạy dùng `FirstPersonFootsteps` theo bề mặt: bùn trên đất, gỗ trên cầu, với các bản thu Ch4 đã duyệt (bank mới `Ch5FoleyBank` chỉ chép nhóm `step_*`). Tre rạn, đổ, cầu gãy dùng các âm có sẵn (`creak`, `stumble`, `clatter`, `string_snap`, `slide_slam`). Không tải âm mới, không dùng gì của DOORS.
- Ảnh duyệt: `LOOK_Forest0…8` (StoryLookCapture). So với concept vẫn kém xa: hình khối đơn giản, cụm lá thưa và phẳng, bờ đất trơn, màu ngả lạnh do grade đêm dùng chung. Đây là **mức chơi được, chưa phải mức hoàn thiện mỹ thuật**.

## 6. Kiểm thử đã chạy (Editor, batch)

| Lệnh | Kết quả |
|---|---|
| `PlaytestBatch … Chapter05_Escape -storyAutopilot -autopilotForest` (bot chơi bằng **input thật**: chuột ảo, Space/Ctrl/E ảo qua `InputReader`, không dịch chuyển Kiều, không đổi tầm nhảy/bắt) | **AUTOPILOT PASS**, đủ 10 tình huống (bảng dưới) |
| `… -autopilotThorough -autopilotEnding dinh_menh` | **PASS**: gặp Sở Khanh (vạch mặt), lượt đầu bot cố tình lỗi → bị bắt (cảnh bắt chạy) → chơi lại → thoát 115 s; `c5_bi_bat = 1`, `c5_so_khanh = 1`; sang cảnh mưa, hoàn tất chương |
| `InkStoryValidator.RunBatch` | **PASS**: 1500 lượt/knot; `chapter_5` đủ 4 nhánh Sở Khanh |
| `StoryPerfProbe.Batch -perfCh5` (1920×1080, render đồng bộ GPU trong Editor) | Góc rừng 15,8–18,2 ms/khung; góc sân/ngõ cũ 15–18 ms. Bóng trăng ~3,5 ms, SSAO ~4,5 ms |
| Chuỗi Ch4 → Ch5 → Ch6 | xem mục 6.2 |

### 6.1 Các tình huống `-autopilotForest`

| Tình huống | Kết quả đo |
|---|---|
| Chạy sạch lối khe trăng / lối tre rậm | Thoát cả hai, 115,2 s, 0 va, 8 lần nhảy, lính gần nhất 5,1–5,3 m, cầu đổ đúng 1 lần |
| Tre đổ báo trước | H1, H2: 1,95–1,98 s từ tiếng rạn tới khi chạm đất, không trúng Kiều |
| Lính vượt/cúi/nhảy thật | Có đủ ba loại link vượt; mỗi lần vượt có thời gian riêng (log `crossed …`) |
| Một lỗi (vấp cây, phản ứng muộn) | Hồi phục, thoát; lính gần nhất 4,9 m |
| Hai lỗi liền nhau (vấp cây rồi va tre thấp ~5 s sau) | **Bị bắt thật** (cảnh bắt kiểu Rear, lôi đi); `c5_bi_bat` +1 đúng một lần |
| Không nhảy ở rãnh | Rơi → thất bại; chơi lại thì tre đổ và cầu được dựng lại |
| Nhảy vào tre thấp | Bị chặn (va), không lọt qua khi đứng |
| Thả Ctrl dưới tre thấp | Vẫn cúi suốt 52 khung kiểm, không đứng dậy được |
| Tầm với | Không với qua rãnh; cùng bờ, cách 1 m thì với được |
| Tạm dừng giữa chừng | Kiều và lính đứng yên; tiếp tục chạy rồi thoát |
| Có chị Lam | Thoát; chị qua cầu trước khi cầu đổ; đặt lại 0 lần |

"Lỗi" của bot = phản ứng muộn: vấp cây xong ~0,9 s mới nhảy, va tre thấp ~0,55 s mới cúi. Bot không cố ý thắng hay thua bằng cách đổi thông số game.

### 6.2 Chuỗi chương

| Lệnh | Kết quả |
|---|---|
| `PlaytestBatch … Chapter04_LauTuBaDem -storyAutopilot -autopilotChain` | **PASS**: Ch4 → Ch5 (thoát 115 s, lối tre rậm) → Ch6 → Ch7 → Kết, đủ 5 chương. Khi nối từ Ch4, khung nặng nhất lúc nạp Ch5 ~3 s (dựng đất/tre trong lúc hiện thẻ chương) |
| `PlaytestBatch … Chapter03_LauTuBa -storyAutopilot` | **PASS** (hồi quy code dùng chung) |

Trong chuỗi này `c4_ru_co_lam` ngẫu nhiên là `false`; nhánh có chị Lam được kiểm trong `-autopilotForest` (tình huống "Có chị Lam"). Chưa chạy nhánh Sở Khanh "từ chối" (lính xuất phát 9 m) và "đi theo" trong Play; Ink validator có phủ cả ba.

## 7. Thay đổi trên code dùng chung (đều opt-in, mặc định tắt)

- `PlayerMovement`:
  - `AutoRun`/`AutoRunSpeed`/`CrouchRunSpeed`, `JumpEnabled` (Space, buffer 0,14 s, coyote 0,12 s), chặn đầu khi va trần;
  - vấp/khựng khi va (chỉ khi `AutoRun`), sự kiện `Jumped`/`Landed`/`Bumped`/`HeadHit`;
  - `FirstPerson` bật/tắt lúc chạy.
- `ThirdPersonCameraController`: `SetFirstPerson` lúc chạy (thân chỉ đổ bóng, trả lại đúng như cũ), `RunBob`, `Dip`, `FovOverride`, `Sensitivity`, `PitchLimits`. Input mô phỏng của QA được nhận cả khi con trỏ không khoá (batch không có cửa sổ).
- `InputReader`: `JumpPressedThisFrame`, `SimulateJump`, `SimulatedCrouch`.
- `CatchScene`: overload `TryPlay(StoryActor …)`.
- `StoryHUD.ShowBark(…, voiced)`, `VoicePlayer.BarkClip`, `SurfaceTag.Add`.
- `StoryAutopilot`: cờ `-autopilotEcho` (ghi log trực tiếp).
- `StoryBuild.PlaytestBatch`: gỡ Pause nếu Play mode bị dừng, nhịp log dưới `-autopilotEcho`.
- `StoryPerfProbe`: cờ `-perfCh5`.

## 8. Vấn đề có sẵn trên `dev` đã gặp và cách xử lý (đã hỏi và được duyệt)

1. Merge PR #2 (`adf5b5a`) đưa lại `Assets/_Project` (449 file, bản trùng `PlayerMovement`, `DialogueManager`… trong asmdef `ThuyKieu.Runtime`) và `Assets/_Sandbox`. Hậu quả có hai:
   - assembly Editor lỗi CS0433;
   - `PersistentSystems` trong `_Project` tự sinh prefab `PF_GameSystems` trước khi load scene. Prefab đó chứa `DialogueManager`, nên `DialogueManager` của mọi chương tự huỷ cả `StorySystems`: **mọi chương không chạy được trong Play**.
   - Đã xử lý: thêm `defineConstraints: ["THUYKIEU_LEGACY_PROJECT"]` vào 3 asmdef của `_Project` (Runtime, Editor, Tests), nên code đó không còn biên dịch. Không xoá file nào. Xoá dòng đó là hoàn tác.
2. Khi mở project, Unity tự cấp GUID mới cho 121 file `.meta` trùng GUID trong `_Project`/`_Sandbox`. GUID của `_Game` giữ nguyên. Đã giữ theo quyết định của bạn.
3. `InkStoryCompiler.Compile` báo `IOException 1224` khi ghi `Main.json` trong cùng tiến trình đang giữ file. `Main.json` vẫn là bản mới. Builder rừng không còn tự biên dịch Ink.

## 9. Chưa kiểm chứng / còn thiếu

- **Chưa chơi tay**: cảm giác lái chuột, độ khó thật với người mới, độ rõ đường khi chạy nhanh, độ căng. Bot pass không chứng minh những điều này.
- **Chưa nghe**: âm thanh (Claude không nghe được), bao gồm bước chân Ch4 trên bùn rừng, tiếng quát 3D, tre rạn.
- **Chưa xem chuyển động thật**: nhìn tận mắt lính cúi (gập lưng bằng GrabPose + hạ 10 cm), lính vượt cây, cảnh hất về bờ, chị Lam chạy. Không quay clip (theo yêu cầu).
- Hiệu năng chỉ đo trong Editor. **Bản exe cũ không chứa thay đổi này** và chưa build lại.
- Mỹ thuật còn xa concept (mục 5).
- Thời lượng chase sạch 115 s, ở cận dưới của đích 2–3 phút. Thêm vấp/ngã sẽ dài hơn. Nếu cần dài hơn: giảm `BambooChaseSequence._runSpeed` hoặc nối dài tuyến trong `Chapter05Forest.Layout`.
- `ForestRunSequence`, `ChaseSequence`, `SneakOutSequence` đã xoá (2026-10-25, theo yêu cầu). `ChaseObstacle` vẫn giữ: các chướng ngại ở phố trong scene Ch5 và `Chapter05Setup` còn dùng.
- ~~`StoryBuild.Rebuild` xoá và tạo lại scene Ch5, GUID đổi~~: đã sửa ở đợt 2 (§11.4, kết quả thử ở §11.7).
- Save/load giữa chương: Ch5 không có điểm lưu giữa chương (như trước). Tiếp tục từ menu sẽ bắt đầu lại Ch5 từ trạng thái lưu cuối Ch4.
- `Assets/_Game/UI/Fonts/ChapterVietnamese.asset` bị Unity ghi thêm glyph (font động) trong lúc chạy kiểm thử. Đây không phải thay đổi có chủ đích. Chưa khôi phục được vì lệnh khôi phục bị chặn trong phiên này. Có thể bỏ thay đổi file đó trước khi commit.

## 10. File chính

- Mới:
  - `Scripts/Gameplay/Sequences/BambooChaseSequence.cs` (+ `.Probe.cs`: bot và `-autopilotForest`, chọn riêng ca bằng `-forestOnly clean1,clean2,one,many,ditch,jumpunder,letgo,reach,pause,lam`);
  - `Scripts/Gameplay/Chase/` `ChaseCourse`, `ChasePursuer`, `ChaseTraverser`, `ChaseLink`, `ChaseRunner`, `FallingBamboo`, `DropBridge`, `IChaseReset`;
  - `Scripts/Environment/` `ForestGround`, `BambooGrove`, `BambooMesh`;
  - `Scripts/Core/Editor/Chapter05Forest.cs` (+ `.Course.cs`, `.Props.cs`);
  - `Materials/Chapter05Forest/` (vật liệu, texture thủ tục, mesh vật cản nhỏ);
  - `Audio/Ch4Foley/Ch5FoleyBank.asset`.
- Sửa: Ink Ch5 và `Main.ink`/`Main.json`, `Chapter05_Escape.unity`, `Chapter05Setup`, `Chapter05World`, `EndingSequence` và các file dùng chung ở mục 7; 3 asmdef trong `Assets/_Project/Scripts`.

## 11. Đợt 2 (25/10/2026): vào thẳng cuộc chạy, rừng kín, âm u, đường dựng an toàn

Brief: `docs/concept/CLAUDE_CH5_FOREST_VISUAL_AND_TRANSITION_FINISH.md`. Gameplay rượt đuổi giữ nguyên: tốc độ, lính, tầm bắt, vật cản, retry. Hai thay đổi nhỏ ở hành vi, đều vì công bằng lúc vào trận và ở cầu (xem 11.1).

### 11.1 Ch4 → Ch5 không còn cảnh chuyển/mở đầu ngắt nhịp

- **Ink Ch5** (`Chapter5_DaoThoat.ink`): `chapter_5` mở bằng `# run:chase_forest`.
  - Thẻ mới `run:` của `StoryDirector` chạy sequence *trước khi* dòng thoại hiện, giống `montage:`.
  - Đã bỏ khỏi luồng: `Escape_Night`, `Escape_Alarm`, `Wide_BackYard`, `CU_Kieu` mở đầu, `Escape_ForestEdge`, `tutorial_forest` (thẻ hướng dẫn modal) và dòng "Vào rừng!".
  - Tiếng Tú Bà hô bắt (vẫn theo `c4_tien`/`c4_van_tu`) và lời chị Lam về khe suối nay là câu nói nhỏ trong lúc chạy (`BambooChaseSequence.CallsBehind`). Chỉ ở lượt đầu, không phát lại khi retry, không chặn input.
- **Ch4** (`Chapter04_LauTuBaDem`): `StoryDirector._seamlessNext = true`.
  - Ink và tiến độ vẫn lưu đúng một lần như trước (`CompleteChapter`), nhưng không hiện màn tổng kết Ch4.
  - Ch5 được nạp ngầm (`LoadSceneAsync`, giữ chờ kích hoạt) trong lúc `Chapter04LauWorld.Exit()` cho Kiều chạy qua cửa sau: con trỏ vẫn khoá, có tiếng thở, khung tối dần trong 0,5 s. Xong mới cho kích hoạt scene.
  - Màn tổng kết/tiêu đề của các chương khác **không đổi**: cờ mặc định tắt.
- **Ch5**: `_openWithCard = false`, nên không hiện thẻ chương; tên chương chỉ hiện ở nhãn nhỏ.
  - `Chapter05World.Start` để màn đen từ khung đầu và đặt Kiều ở mép rừng ngay. Không lộ sân/ngõ, không còn dựng NavMesh phố lúc mở (chỉ dựng khi cần cho cảnh Sở Khanh).
  - Mở Ch5 trực tiếp từ Editor/menu cũng đi đúng đường này.
- **Khởi động cuộc chạy**: NavMesh rừng nay được nướng sẵn trong builder (`C5F_ForestNavMesh.asset`, 380 KB) nên có ngay (0,001 s). Bản dựng NavMesh lúc chạy chỉ còn là dự phòng.
  - Input, lính và luật bắt chỉ bật khi rừng + NavMesh đã sẵn sàng.
  - Thay đổi so với đợt 1: điều khiển mở **cùng lúc** khung hình bắt đầu hiện (fade 0,3 s), thay vì sau khi fade xong. Lính cũng bắt đầu chạy từ lúc đó, nên không còn 0,35 s lính chạy khi người chơi chưa điều khiển được.
  - Đoạn lấy đà trước vật cản đầu (J1) vẫn 48 m ≈ 9,6 s.
- **Sở Khanh**: knot `c5_gap_so_khanh` và biến giữ nguyên (save cũ nạp được), nhưng **tạm không được gọi** ở đâu.
  - Không dời sang chỗ khác và không tự chọn đáp án. Bộ kiểm Ink: `c5_so_khanh` = 0 ở cả 1500 lượt.
  - Ca kiểm thử Sở Khanh của đợt 1 không còn là tiêu chí nghiệm thu cho phần mở đầu mới.
- **Lam ở cầu**: điều kiện được đạp cọc nay là "chị Lam đã rời mặt cầu", thay vì "chị đã tới cọc − 1,5 m". Lý do: ngưỡng cũ bắt Kiều đứng chờ ở góc cọc trong khi lính lên cầu; ca Lam từng đạt chỉ nhờ may.
  - Chị Lam cũng chạy bù nhanh hơn khi tụt sau chỗ của mình (`ChaseRunner._sprint` 7,2 m/s). Không đụng tới cân bằng của lính.

### 11.2 Rừng kín nhiều lớp (BambooGrove, ForestGround, BambooMesh)

- **Đất**:
  - Lớp mịn 16 m quanh đường, chia ba phần: có NavMesh + collider (< 3 m), chỉ collider (< 6 m), chỉ hình.
  - Đất xa thô (2 m/ô, không collider) ra tới 74 m, dâng thành đồi thấp. Thân tre ở xa đứng trên đất này nên không lộ mép map.
- **Tre**:
  - **Lớp gần** (< 4,5 m): bụi tre non/già, xanh/ô liu/úa vàng/rêu tối, thân gãy cụt, cong; nghiêng và vồng ra phía đường. Chỉ lớp này có collider (3,2 m dưới, đúng cỡ) và đổ bóng trăng.
  - **Lớp giữa** (< 18 m): thân thưa hơn, lá chồng nhau, bụi cây, thân đổ nằm ngang.
  - **Lớp xa** (< 40 m): thân đơn giản có tán.
  - **Mái tán**: các cụm lá chéo nhau ở độ cao 7–12 m, có khe trời nhỏ theo noise, mở rộng ở chỗ có tia trăng, trên khe suối và sân chùa.
  - **Lớp chân trời** (30–70 m): thẻ đứng vẽ rừng xa.
  - Rễ tre lòi ra ở mặt bờ, lá mục, dương xỉ theo cụm.
  - Mỗi đoạn đường có kiểu rừng khác nhau (noise tỉ lệ lớn: dày/thưa, già/non, đang chết).
- **Va chạm**: lá và bụi không có collider và tránh mép rãnh/vật cản (vùng trống quanh mỗi vật cản như cũ). Collider thân tre và đất ngoài bờ được bỏ khỏi NavMesh (`NavMeshModifier`).
- **Cách sinh**: tính trên luồng phụ (`Parallel.For`, noise viết bằng C# trong `ForestNoise`), collider được cook trên luồng phụ (`Physics.BakeMesh`). Tìm hành lang bằng lưới mảng đặc.
- **Không có LOD hay cull theo khoảng cách**, chỉ frustum culling theo bounds từng ô 40 m. Vì vậy không có pop-in theo LOD.
- **Dọn dẹp**: tự xoá các mesh ẩn sót lại từ phiên Editor trước, giải phóng mesh khi tắt.

### 11.3 Âm u nhưng đọc được (chỉ thay đổi riêng scene Ch5)

- **Trăng**: 0,7 (màu xanh lạnh), có cookie bóng lá 19 m để trăng chỉ lọt từng mảng; 4 tia trăng dịu (1,1–1,7, góc 34°) ở chỗ tán mở.
- **Ambient, sương, sky**:
  - Ambient trilight ô liu tối.
  - Sương exp² 0,022 màu xám lạnh, thêm 2 lớp sương hạt thấp/cao.
  - Sky của Ch5 hạ exposure 0,22, reflection 0,35.
- **Đèn gợi ý vật cản**: mỗi vật cản có một đèn điểm nhỏ lạnh, đặt *sau* vật cản (1,8–2,2, tầm 5–6 m, không bóng), để thân cây/tre thấp hiện thành viền tối trên nền sáng, còn mép rãnh sáng trên khe đen. Không phải dãy đèn soi đường.
- **Đuốc lính**: tầm 10 m, 2,6, hổ phách, vẫn chập chờn theo người.
- **Grade riêng Ch5**: exposure 3,3 EV, contrast 4, sat −6, split toning ô liu.
  - Lý do: ACES (grade filmic dùng chung) đè đêm tối thành đen ở exposure thường. Đã đo: tắt tonemapping thì sáng gấp ~7 lần. Nên cảnh vẫn được chiếu tối, rồi nâng exposure vào vùng ACES còn giữ chi tiết bóng tối.
- **Vật liệu**:
  - Thân tre có texture mới (đốt, vệt mưa, bẩn, địa y), normal map đốt/rãnh, độ bóng theo alpha.
  - Bùn đường ướt sáng hơn đất xung quanh; vũng nước nhỏ dày hơn; lá rụng không phủ giữa đường.
  - Màu albedo nâng về mức thực tế: độ tối đến từ ánh sáng, không phải từ vật liệu đen.

### 11.4 Đường dựng scene

- **`Chapter05Forest.Apply`** (menu/batch):
  - Đặt cờ nối liền cho Ch4, dựng lại phần nó sở hữu (root `BambooChase`, camera rừng, sequence `chase_forest`, component chase của lính/Lam, vẻ đêm của scene) và nướng NavMesh.
  - Ghi **kiểm đếm** trước/sau.
  - Asset được cập nhật tại chỗ: mesh vật cản bằng `CopySerialized`, PNG chỉ ghi khi pixel đổi, NavMesh `CopySerialized`. Không còn `DeleteAsset`+`CreateAsset`.
- **`StoryBuild.Rebuild`** không còn xoá Ch5. Nó gọi `Chapter05Setup.Remake()`:
  - Dựng lại scene từ đầu rồi lưu đè lên chính file đó (giữ `.meta`, nên giữ GUID và vị trí trong build list).
  - Áp lại rig lính, đặt cờ nối Ch4.
  - Báo lỗi nếu GUID đổi.
  - Lý do phải dựng lại cả scene: full rebuild tạo lại UI/animator/vật liệu dùng chung với GUID mới, scene giữ nguyên sẽ trỏ vào asset đã mất.
- `Chapter05Setup.Create()` (project chưa có Ch5) vẫn tạo mới như cũ.

### 11.5 Kiểm thử đợt 2 (Editor, batch, máy hiện tại)

| Kiểm thử | Kết quả |
|---|---|
| Biên dịch | Sạch (cảnh báo cũ của Ch4 probe không đổi) |
| Ink validator | **PASS**; `run:chase_forest` được mô phỏng; `c5_so_khanh` = 0/1500 |
| `Apply` lần 1 → lần 2 | Kiểm đếm giống hệt (3.431 object · 2.520 collider · 870 renderer · 24 đèn · 14 link · 1 rừng/1 đất/1 surface · sequences `chase_forest+kieu_voice`); scene khác 0 dòng ngoài fileID; asset md5 giống hệt; GUID scene `9a169de3…` giữ nguyên |
| `-autopilotForest` (tất cả ca) | **PASS**: sạch 2 lối 115 s (gần nhất 5,6/5,1 m), 1 lỗi hồi được, nhiều lỗi bị bắt, đếm 1 lần, rơi rãnh, reset, đầu cụng, không đứng dưới tre, tầm với 2 phía, pause, Lam qua cầu |
| Chuỗi Ch4 → Ch5 → Ch6 → Ch7 → Kết (`-autopilotChain`) | **PASS**. Ch4 thoát 0,57 s, Ch5 đã nạp 100% lúc đó; khung chạy đầu tiên hiện 2,20 s sau lúc bắt đầu thoát (≈ 1,6 s đen); biến Ch4 và manh mối mang sang đúng |
| Mở Ch5 trực tiếp (`-autopilotLook`, đi hết chương) | **PASS**; khung đầu hiện 1,26 s sau khi scene thức |
| Thời gian sinh lúc nạp | Mặt đất 0,47 s, tre 0,50 s (trước: 1,6 + 0,7 s), NavMesh 0,001 s (trước ~1,2 s); vẫn chạy trên main thread trong lúc màn đen, có thêm ~1 khung khởi động shader (~1 s trong Editor) |

**Đo hình ảnh** (`-autopilotLook`). Đo trong Play Mode với grade gameplay thật, ở 640×360 trong bộ nhớ, **không xuất ảnh**. 17 trạm (đầu rừng, trước từng vật cản 14 m ở cả hai lối, trước cầu 30 m và 6 m) × 9 tư thế (đứng/trái/phải/sau, cúi, đỉnh nhảy 2,66 m: trước/trái/phải/ngửa lên):

- Phần khung "không có gì được vẽ" ở dải chân trời khi nhìn ngang: **tối đa 0,1%** (ở cầu, nơi được phép nhìn xa; mọi trạm khác 0,0%).
- Nhìn thẳng lên từ đỉnh nhảy thấy trời qua kẽ tán: trung bình 13,3%.
- Độ sáng sau grade (0–1) trên 137 góc nhìn: trung bình 0,071, trung vị 0,050, sáng nhất (p95) 0,20, vùng tối dưới 0,03 trung bình 41% (lối tre rậm tối nhất, có chủ ý), mặt đường phía trước 0,047.
- Camera người chơi đối chiếu khớp với camera đo.
- Vật cản từ 18 m: mỗi khúc cây/tre thấp làm đổi 210–650 px của khung 640×360, chênh độ sáng tới 0,33–0,9; từ 10 m 750–1.540 px. Tảng đá ép đường (Q1), đo lại sau khi sửa cách bộ đo tìm đối tượng: 193 px từ 18 m, 902 px từ 10 m.

**Hiệu năng** (`StoryPerfProbe -perfCh5`, 1920×1080, render đồng bộ GPU trong Editor, cùng cách đo với mốc 15,8–18,2 ms):

- Các góc nhìn trong rừng hiện ~19–21 ms. Chênh giữa 2 lần đo cùng bản dựng là ±1 ms; chính các góc ngoài rừng hôm nay cũng chậm hơn lúc đo mốc 1–4 ms.
- **Chưa giữ được 60 FPS trong Editor**: khung thường trên 16,7 ms. Đã giảm từ 21,8–23,7 ms xuống bằng cách tỉa lá tầng giữa, bụi thấp, lá trên đường.
- Phần còn tốn, theo phép tắt từng lớp: lớp sau ~1,5 ms, bụi thấp ~1 ms, tán ~0,8 ms, lá gần ~0,3 ms, chân trời ~0,4 ms, đèn gợi ý ~0. SSAO (dùng chung) ~5–6 ms và bóng trăng ~3,7 ms vẫn là phần lớn nhất. Không đổi SSAO/bóng dùng chung.
- Bản build (IL2CPP/Mono release, không có overhead Editor) **chưa đo**.

### 11.6 Chưa kiểm chứng / giới hạn

- **Chưa ai nhìn bằng mắt**: rừng có đẹp/kín/âm u đúng ý không, lá có lộ mặt sau hay mảng vuông không, chuyển động gió/lá rơi. Mọi kết luận về hình ảnh ở trên là số đo, không phải đánh giá thẩm mỹ.
- **Chưa chơi tay**: đi qua cửa sau Ch4 sang Ch5 có thấy liền mạch không, có say góc nhìn thứ nhất không, lái chuột lúc vừa vào chương.
- **Chưa nghe**: tiếng thở/bước nối qua điểm chuyển, câu Tú Bà/Lam lúc chạy (chưa có file thoại cho câu mới, nên nếu không tìm được clip thì chỉ hiện chữ).
- Màn đen khi nối Ch4 → Ch5 vẫn ~1,6 s trong Editor (nạp scene + sinh rừng + khởi động shader). Chưa có số đo trên bản build.
- **Exe vẫn là bản cũ**: chưa build lại, theo yêu cầu.
- Full rebuild: xem kết quả bản sao thử ở 11.7.
- Unity ghi lại `Chapter04_LauTuBaDem.unity`: chỉ đổi cờ `_seamlessNext` qua `SerializedObject`, không sửa YAML bằng tay.

### 11.7 Full rebuild thử trên bản sao (không chạy trên bản làm việc)

Bản sao nằm ở `D:\PRU213\ThuyKieu_Share-20261009T182626Z-1-001\ThuyKieu_Share\_RebuildSandbox\ThuyKieuGame_With_Unity`, khoảng 8 GB, chép không kèm `.git`, Temp, Logs, ArtSource, docs. Trong đó chạy `-executeMethod ThuyKieu.Core.Editor.StoryBuild.RebuildBatch`:

- Kết quả: exit 0. Log ghi `[Chapter05Setup] Chapter 5 remade in place · GUID 9a169de324da76b4b88b8013bf07bc3b`.
- GUID: không scene nào đổi GUID. Ch5 vẫn có trong build list.
- Sau rebuild: `-autopilotForest -forestOnly clean1,clean2,many,lam` đạt (**PASS**), chuỗi Ch4 → Ch5 → Ch6 → Ch7 → Kết đạt (**PASS**). Khung chạy đầu tiên của Ch5 hiện 1,72 s sau lúc Ch4 bắt đầu thoát.
- Bản sao đã được xoá sau khi kiểm thử xong (25/10/2026, theo yêu cầu).

## 12. Đợt 3 (25–26/10/2026): âm thanh, nhạc truy đuổi, mật độ vật cản, POV

Brief: `docs/concept/CLAUDE_CH5_AUDIO_MUSIC_OBSTACLES_POV.md`, `docs/concept/CH5_GROUND_AUDIO_AUDIT.md`. Chỉ đổi Ch5; các phần dùng chung đều thêm tùy chọn mặc định không đổi gì (xem 12.6).

### 12.1 Kiểm tra trước khi sửa
- **Nhạc đang phát**: `music/chase.wav` là bản tổng hợp bằng code (`ProceduralAudio.Chase`, 110 BPM), dài 24 s, mono 22 kHz, RMS −20,8 dB và gần như phẳng suốt bài (±1,5 dB). Chỉ Ch5 dùng id này.
- **Hiệu ứng của cuộc chạy** đều là âm tổng hợp đã nướng ra file: `cloth`, `stumble`, `creak`, `whoosh`, `clatter`, `blow`, `string_snap`. Bước chân là bản thu Ch4.
- **Nền rừng** bị gắn `Surface.Mud` cho mọi ô có collider; lớp mud strip chỉ là hình. Tiếng đáp gọi cứng `step_mud`/`stumble`.

### 12.2 Âm thanh (bank `Ch4Foley/Ch5FoleyBank.asset`, cùng GUID)
- **Nền**:
  - Ô đất rừng giờ là `Surface.Earth`.
  - Ván cầu vẫn là `Wood`. Đá lách/ép đường là `Stone`.
  - Không còn tiếng bùn, nước, lá hay cỏ dưới chân.
- **Bước Kiều** (`step_earth`, 10 take): đất Kenney cộng một take sỏi Ali_6868 ở −9 dB, mỗi bước một cặp khác nhau, không dùng file preview.
  - Bước tính theo quãng đường thật (1,35 m), như trước. Không có bước khi đang bay, pause, catch, teleport, hay đẩy tường mà không di chuyển.
  - Không lặp take liền nhau; cao độ dao động ±2 %.
- **Bước lính** (`guard_step`): take đất nặng hơn cộng sỏi −6 dB, hạ cao độ ×0,84–0,92, phát 3D từ phía sau. Trên cầu dùng `guard_step_wood`.
- **Nhảy và đáp**:
  - Nhảy: `jump_earth/wood/stone` (tiếng đạp nền ngắn) cộng `cloth` rất nhỏ.
  - Đáp: `land_*` theo vật liệu tại chỗ (hai chân cách 40–45 ms), to theo vận tốc rơi thật. Không phát tiếng vấp khi đáp. Bước kế tiếp đến sau một sải chân, nên không có "ba bàn chân".
- **Va chạm**:
  - Vấp chân: `trip_*`.
  - Va thân tre: `culm_knock` (va gỗ nâng ×1,15).
  - Cụng đầu: `culm_knock` nhỏ, cao hơn.
  - Lính bị hất khi cầu sập: `guard_land`.
- **Tre đổ** (từ kheetor, **bỏ đoạn tiếng rìu ở 0,05 s**):
  - `bamboo_crack` ở gốc: hai tiếng nứt trong lúc thân lắc, cắt từ 0,40–1,36 s của bản thu.
  - `bamboo_sweep` ở ngọn khi bắt đầu ngã (1,36–1,66 s).
  - `bamboo_impact` một lần đúng lúc chạm (1,66 s, gồm cả cú nảy, nâng ×1,12).
  - `leaf_settle` sau đó.
  - Tất cả phát 3D từ chỗ vật thật.
- **Cầu**: tiếng đạp cọc `stake_kick`, dây đứt `rope_snap` (một tiếng nứt sắc trong đoạn kẽo kẹt, ×1,3), sàn lắc xuống `deck_swing`, sàn đập vào bờ `deck_hit`. Mỗi sự kiện đúng một lần. Thêm `rope_strain` (dây lan can căng) mỗi 1,1–1,6 s khi Kiều đang chạy trên ván. Không có nước tóe vì sàn không chạm tới nước.
- **Cách dựng**: `-executeMethod ThuyKieu.Core.Editor.Ch5Foley.BuildBatch` dựng clip và nhạc; báo cáo cắt/mức ở `Tools/Ch5/QA/Ch5FoleyBuild.txt`. Số đo nguồn: `Ch5Foley.AnalyseBatch` → `Tools/Ch5/QA/Ch5Sources.txt`.
  - `Chapter05Forest.Apply` chỉ ghép bank từ những gì đã dựng, gồm ván và đá của Ch4 cộng mọi thư mục `Audio/Ch5Foley/<id>`. Ghép xong là thay toàn bộ, không cộng dồn, nên Apply lần hai không nhân hay mất nhóm.
- **Credits**: `Audio/Ch5Foley/CREDITS.md` và màn Credits trong game, có dòng bắt buộc của Juggernaut, congusbongus/Ali_6868, kheetor, và bản ngáy CC BY-SA của Ch4 (trước đây thiếu).
- **Còn là âm tổng hợp**: `cloth`, `breath_in`, `breath_startled`, `heartbeat`, `gong`.

### 12.3 Nhạc truy đuổi (`ChaseMusic` + `Audio/Ch5Music/Ch5ChaseScore.asset`)
- **Nguồn**: 'Juggernaut' by Scott Buckley — CC BY 4.0, full mix, không có stem. Đo được 125,0 BPM, phách đầu ô nhịp ở 0,08 s, ô nhịp 1,92 s.
- **Ba vòng lặp 8 ô nhịp** (≈15,4 s), không giả stem bằng cách phát chồng full mix:

  | Cấp | Ô nhịp trong bài | Độ to (dB) | Khi nào phát |
  |---|---|---|---|
  | 0 | 2–9 | −25 | Từ đầu rừng |
  | 1 | 20–27 | −20,8 | Từ chỗ hai lối gặp lại |
  | 2 | 101–108 | −16,6 | Từ 70 m trước cầu (nước rút và cầu) |

  - Mỗi vòng nướng liền mạch: ô nhịp ngay sau được crossfade 1 phách vào đầu vòng.
  - Mức trong game là −22,5 / −21 / −19,5 dBFS, quanh mức −20,8 của nhạc cũ, nên phần còn lại của mix giữ chỗ.
- **Cách đổi cấp**: chỉ ở vạch ô nhịp kế tiếp, crossfade equal-power 1 phách. Không phát lại từ đầu khi gặp vật cản hay khi chơi lại; khi bị bắt, nhạc giảm rồi quay về cấp 0 ở vạch ô nhịp sau.
- **Ưu tiên tín hiệu**: tre nứt thì nhạc giảm còn 0,55 trong 1,6 s; khi bị bắt giảm còn 0,35. Lính dưới 5 m quá 0,6 s thì lên một cấp, xa hơn 8 m quá 2 s thì về lại.
- **Thoát và tùy chọn**: tới chùa thì nhạc tắt dần trong 4 s. Theo cài đặt Music và pause (một lần đổi đang chờ qua pause sẽ được lên lịch lại). Thẻ Ink mở chương đổi thành `music:none` để nhạc tổng hợp cũ không chen vào.
- **Lượt chạy sạch** (giờ trong game): cấp 1 ở ~57 s, cấp 2 ở ~92 s, cầu đổ ở ~110 s.

### 12.4 Vật cản: 25 vật cản mỗi tuyến cộng ngã rẽ và cọc, tổng 27 quyết định (trước 12/14 mốc)

| m (lối trăng) | Lối chung | Lối trăng | Lối rậm |
|---|---|---|---|
| 44 · 68 | nhảy J1 · cúi D1 (dạy riêng, đà 6 s) | | |
| 92 · 118 · 142 · 162 | lách trái K1 · tre đổ H1 (phải) · nhảy J6 · lách phải K2 | | |
| 190 → 276 (ngã rẽ) | | lách K3 · rãnh G1 · nhảy J7 · cúi D5 | lách K4 · cúi D2 · nhảy J2 · cúi D3 |
| 286 · 302 · 320 | đá ép Q1 · nhảy J3 · cúi D4 | | |
| 338 · 348 | cụm lách K5 + nhảy J8 | | |
| 370 · 392 | tre đổ H2 (trái) · rãnh G2 | | |
| 408 · 418 · 433 | cụm nhảy J4 + cúi D6 · lách K6 | | |
| 456 | nhảy J5 (khúc cua gấp) | | |
| 482 · 492 · 506 · 516 | cúi D7 · lách K7 · tre đổ H3 · nhảy J9 (nước rút) | | |
| 516 → 536 | chừa 20 m để đọc cầu và cọc | | |

- Khoảng trống dài nhất giữa hai quyết định: 6,6 s (lối trăng) và 6,0 s (lối rậm).
- **Vật cản mới "lách"** (`ObstacleKind.Dodge`): đá rêu và 4 gốc tre gãy có collider chắn một bên tới quá giữa đường 0,3 m, chừa 1,2–1,4 m.
  - Collider ở layer nền nên NavMesh đi vòng; lính và chị Lam tự lách theo.
  - Có đèn gợi ý phía sau. Bot lách bằng chuột như người chơi đã thuộc đường.
- Tre đổ có thể đổ từ trái hoặc phải.
- Lỗi đã sửa trong lúc làm: tre đổ phía trái từng tính sai điểm vượt và link của lính (lệch ~13 m), làm lính kẹt.
- Không đổi tốc độ chạy, tầm bắt hay luật bắt.

### 12.5 POV (`ChaseCameraMotion`, chỉ khi đang chạy Ch5)
- **Bob**: theo đúng pha bước chân của `FirstPersonFootsteps` (chung một sải 1,35 m, không còn bob 1,3 m trôi nhịp).
  - Mắt hạ thấp nhất lúc chân chạm và hơi đung đưa về chân vừa đáp.
  - Biên độ 3,4 cm; giảm một nửa khi cúi; tắt khi bay hoặc khi bị khóa điều khiển.
- **Nhảy và đáp**:
  - Nhảy: hạ 1,8 cm thoáng qua.
  - Đáp: dip 2,5–8,5 cm theo vận tốc rơi, giữ 0,06 s rồi hồi trong ~0,2 s.
- **Nghiêng và va**:
  - Lách A/D nghiêng ≤ 1,5°, vào và ra mềm.
  - Va chạm: đẩy ngắn ra khỏi điểm va cộng nghiêng có hướng, tắt nhanh.
- **Giới hạn và tùy chọn**:
  - Tổng độ lệch ≤ 11 cm, tổng góc ≤ 3°.
  - Tất cả nhân với tùy chọn "Rung màn hình"; Tắt là tắt hết.
  - Không xoay hướng chuột, không đổi FOV (giữ 74°), không blur.
- **Vòng đời**: tắt khi bị bắt, đặt lại khi chơi lại, trả camera về 0 khi kết thúc hoặc huỷ.

### 12.6 Thay đổi ở lớp dùng chung (đều mặc định không đổi gì)
- `ThirdPersonCameraController.MotionOffset/MotionTilt`: mặc định 0.
- `FirstPersonFootsteps.StridePhase/StepCount`: chỉ đọc.
- `StoryAudio.EffectPlayed`: sự kiện QA.
- `ChaseCourse.ObstacleKind.Dodge`: thêm ở cuối enum.
- `StorySceneKit.Set`: nhận `float[]`.
- `Ch4Foley`: ba hàm chuyển từ private sang internal.
- `CreditsView`: thêm dòng ghi công.

### 12.7 Kiểm thử (Editor, batch, tắt tiếng; chỉ chứng minh logic)

| Kiểm thử | Kết quả |
|---|---|
| Biên dịch | Sạch |
| `Apply` ×2 | Kiểm đếm giống hệt (3.207 object · 2.296 collider · 861 renderer · 39 đèn · 22 link); scene khác 0 dòng ngoài fileID; md5 vật liệu, NavMesh, bank, score giống hệt; GUID scene giữ |
| `-autopilotForest` tất cả ca | **PASS**. Bố cục 27/27 quyết định, khoảng trống tối đa 6,6/6,0 s. Chạy sạch 2 lối 116,8/116,4 s, không va, lính gần nhất 5,2 m. 3 tre đổ báo trước 1,96–1,98 s. Sai 1 lần gỡ được; nhiều lần bị bắt, đếm 1 lần. Rơi rãnh, chơi lại đặt lại. Cúi/nhảy dưới tre, tầm với hai phía, pause, Lam qua cầu đều đạt |
| Âm thanh (mỗi lối) | Cầu: đạp, đứt, lắc, đập đúng 1 lần mỗi thứ. Tre: nứt 2, quét 1, chạm 1, lá 1 mỗi cây. 0 bước khi bay, 0 bước chồng cú đáp. ~1 bước / 1,42–1,44 m. Số tiếng đạp nền bằng số lần nhảy; tiếng đáp ≥ số lần nhảy. 0 âm cũ (`stumble`, `step_mud`, `clatter`, `blow`, `string_snap`, `whoosh`, `creak`, `slide_slam`). Nhạc lên tới cấp 2 trước cầu |
| Pause đúng lúc tre nứt | Cây đứng yên, không âm nào phát trong lúc dừng; sau đó đổ và chạm đúng 1 lần |
| Ch4 → Ch5 → Ch6 → Ch7 → Kết | **PASS**; Ch4 thoát 0,61 s, khung chạy đầu tiên 2,09 s sau lúc bắt đầu thoát; dữ liệu Ch4 mang sang đúng |
| Ch3 (một mình) | **PASS** |
| Đo hình ảnh (`-autopilotLook`) | **PASS**; dải chân trời lộ tối đa 0,1 % (ở cầu), trời qua tán 13,2 % |
| Hiệu năng (`-perfCh5`, 1080p, 2 lần) | Góc rừng 19,9–21,5 ms (đợt trước 19–21 ms), gần như không đổi trong biên dao động của máy. 15 đèn gợi ý mới gần như không tốn. **Vẫn quá 16,7 ms** |

**Chưa đo**: số voice âm thanh tối đa, bộ nhớ âm thanh, GC (công cụ đo hiện có không lấy được). Phần mới thêm 3 vòng nhạc nén Vorbis trong RAM (~15 s stereo mỗi vòng), không có nguồn âm chạy mỗi khung; tiếng 3D dùng pool sẵn có (tối đa 24).

### 12.8 Chưa nghe / chưa chơi tay
- Chưa ai nghe bất kỳ âm nào trong game. Đặc biệt chưa ai nghe:
  - mức sỏi −9 dB dưới tiếng đất (người chơi mới duyệt chất sỏi, chưa duyệt mix);
  - Juggernaut có quá hùng tráng cho rừng tre không;
  - ba đoạn cắt và các chỗ chuyển đoạn;
  - tiếng tre nứt từ bản thu cây gỗ;
  - tiếng dây đứt (vốn là một tiếng nứt gỗ);
  - mức nhạc so với mưa và tiếng lính.
- Chưa ai chơi tay để đánh giá độ khó của 27 quyết định, cảm giác bob, đáp, nghiêng và say góc nhìn thứ nhất. Bot không chứng minh được những điều này.
- Hơi thở và tim đập vẫn là âm tổng hợp, chưa có bản thu giọng nữ.
- Exe vẫn là bản cũ.

## 13. Đợt 4 (26/10/2026): rừng tre chân thực hơn (brief `docs/concept/CLAUDE_CH5_FOREST_REALISM_PASS.md`)

Làm trên scene thật `Chapter05_Escape.unity` (GUID `9a169de3…` giữ nguyên) và builder `Chapter05Forest.Apply`; `dev`, HEAD `021f57d`, mọi thay đổi chưa commit. Gameplay, âm thanh, nhạc, POV, tốc độ, vị trí và thời gian báo trước của vật cản không đổi.

### 13.1 Địa hình (`ForestGround`)
- Đường giờ cắt vào một sườn đồi (trường cao độ `_relief` 6,5 m, nếp gấp 1/0,017 m). Hai bên đường lần lượt là:
  - bờ cắt cao dốc, mặt đất lở nhẹ, chân có đống đất sạt;
  - bờ cắt vừa, đỉnh phẳng;
  - mép đất thấp, phía sau nền phẳng hoặc dốc xuống (đường men sườn, rừng thấp dần phía dưới).
  Hình dáng mỗi bên khác nhau và đổi theo nếp đồi, không còn một mặt cắt chạy suốt tuyến.
- Bỏ dải "bùn ướt" dán trên mặt đường. Giờ một material duy nhất phủ đường, bờ và nền rừng, đọc vertex colour: r = đất đã nện, g = ẩm, a = lá rụng. Không còn mép mesh hay dải texture dán lên.
- **Đã sửa một lỗi có sẵn:** probe mới `walls` cho thấy bờ cũ không giữ được người chơi. Áp vào bờ và nhảy liên tục, cô leo ra khỏi tuyến chỉ trong khoảng 1,3 s (lên tới 1,75–1,97 m ngoài mép, cao 4,8 m). Có hai nguyên nhân:
  - phần vai bờ thoải từ khoảng 1,25 m;
  - CharacterController coi mặt dốc chạm bên hông là đất, nên cô nhảy tiếp được từ mặt dốc.
  Cách giữ hiện nay:
  - Bờ cắt dốc liên tục tới ít nhất 1,9 m.
  - Ở chỗ bờ thấp có hàng rào tre ("lũy tre") ở mép. Collider là hộp cao 3 m mỗi mét, nằm trong NavMesh, nên lính cũng tránh. Tre và bụi gai che kín collider, không có chỗ trống trông như đi qua được.
  - Vị trí hàng rào do phép quét tầm với quyết định. Phép quét tính cả bậc đứng trên vật cản (khúc tre, tảng đá né, đá kẹp, tre đổ nằm ngang), độ cao cô đứng được trên mép ở khoảng một bước nhảy trước đó, và hai mép mỗi rãnh.
  - Tổng cộng 28 hàng rào, khoảng 1,2 km.
- `PlayerMovement.SlideOffSteep` (mặc định tắt, chỉ Ch5 bật khi đang chạy): nếu dưới chân dốc quá giới hạn, hoặc không có gì mà chỗ vừa chạm là mặt dốc, cô trượt xuống và không nhảy được.
  - Mép ván, mép khúc tre vẫn tính là đứng được.
  - Kết quả kiểm: các bước chân trên cầu và khi đáp không đổi.

### 13.2 Tre, lá và cây thấp (`BambooGrove`, `BambooMesh`, shader `ThuyKieu/Ch5Bamboo`)
- **Mọc theo bụi:** tre già ở giữa bụi, tre non và măng ở rìa, xòe ra như bình. Tre chết hoặc gãy nhiều hơn ở bụi già, gần đường thì ngả và cong về phía đường.
- **Lá:**
  - Cành nhỏ (ống 3 cạnh) nối từ đốt tre tới chùm lá ở phần thấp.
  - Chùm lá gần là card 3×3 đỉnh, cong rủ về cuối và gập dọc giữa, để nhìn ngang không thành mảng phẳng.
  - Atlas lá 1024² gồm 4 kiểu (dày, thưa rủ, sáng tối, đầu lá úa). Lá thon và dài, mọc so le, dồn thành quạt ở đầu cành.
  - Viền alpha được dilate để không có viền trắng. Mip giữ độ phủ ở lá gần, bụi gai và dương xỉ vẽ nên lá không mỏng dần theo khoảng cách. Tán trên cao, card chân trời, lá rụng và bụi rậm vẫn mỏng dần như cũ để giữ chi phí.
- **Thân tre:** texture 128×512, có thớ, lỗ bì, gờ đốt, sẹo bẹ, phấn trắng dưới đốt, vệt mưa, bẩn chân và địa y lấm tấm. Normal map có gờ đốt và sẹo bẹ.
- **Gió** (`ForestWind`, global `_C5Wind`, chỉ Ch5):
  - Thân tre lắc chậm và ít, gốc đứng yên (trọng số theo bình phương độ cao). Lá và cành rung nhanh hơn. Mỗi thân một pha.
  - Gió có trong cả pass shadow và depth.
  - Mesh vật cản không có trọng số gió nên đứng yên. Collider tre không bị ảnh hưởng.
- **Dương xỉ:** gần đường dùng scan Poly Haven Fern 02 (4 cụm, Mesh LOD, cull khi nhỏ trên màn hình), khoảng 500 cây. Xa hơn dùng card vẽ có tàu lá và lá chét, không còn hình quạt cọ.
- **Rễ:** mọc từ chân bụi tre già, chạy xuống mặt bờ theo từng đoạn, bám đất.
- **Ánh trăng xuyên lá** từ phía sau: tiết chế, đang để 0 ở mọi material (giữ chi phí). Có thể bật lại riêng `C5F_Leaves` bằng `_ClearCoatSmoothness`.
- **Giới hạn:** chưa có asset tre chụp thật hợp lệ. Thân, lá, tán, bụi gai và card chân trời vẫn vẽ bằng code, không phải scan hay photogrammetry.

### 13.3 Vật liệu đất, đá
- Shader `ThuyKieu/Ch5ForestGround` (chỉ Ch5) dùng lại layout URP Lit nên tương thích SRP Batcher và DOTS instancing:
  - Bề mặt nằm ngang: Brown Mud 02 2K. Tỉ lệ thật: một lần lặp 2,6 m.
  - AO và độ nhám đóng gói vào `C5F_GroundARM.png` (AO ở r, độ nhám ở g, import tuyến tính). Smoothness = `_Smoothness × (1 − roughness)`.
  - Mặt dốc dùng `brown_mud_dry` (đất khô vụn).
  - Chống lặp: hai tỉ lệ và hai góc xoay của texture, trộn theo mảng lớn, chỉ đọc cả hai ở dải chuyển tiếp. Thêm sắc độ theo mảng hàng chục mét.
  - Đường nện hơi bóng. Chỗ ẩm chỉ bóng ở nơi độ nhám thấp, có vũng nhỏ mép mềm nằm trong hõm đất. Lá rụng làm mờ bóng.
- Bỏ đĩa vũng nước nổi trên đường. Vũng trong rãnh vẫn giữ.
- **Đá:**
  - Đá rêu trên bờ và nền rừng là scan Poly Haven Rock Moss Set 01, có Mesh LOD (8–10 mức). Gồm 129 viên:
    - đá chìm vào bờ cắt, không nhô vào chỗ chạy;
    - đá nhỏ ở chân bờ thấp;
    - cụm 2–4 tảng trên nền rừng;
    - đá dưới khe.
    Các viên này không có collider: viên nào nằm trong bờ thì đã có collider của bờ, số còn lại ở ngoài tầm với.
  - Đá thuộc đường chạy (né, kẹp, ngã rẽ, đá đáy rãnh) cũng dùng scan. Kích thước và độ nhô lấy theo đá cũ, mặt trong vẫn khớp đúng chỗ như trước. Collider là convex hull của chính mesh scan.
- Material scan dùng URP Lit đúng kênh: metallic ở r, AO ở g, smoothness = 1 − roughness ở a, normal OpenGL import đúng loại.
- Mặt đường vẫn đọc là đất lẫn đá nhỏ và surface tag vẫn là Earth. Kiểm tiếng chân: bước gần đúng mỗi 1,45 m, không bước trên không.

### 13.4 Ánh sáng và sương (chỉ trong scene Ch5)
- Vệt trăng qua tán (`MoonShaft`) đặt theo đúng hướng trăng.
- Đèn gợi ý vật cản giờ là vũng sáng cao (3,5 m) và rộng (7,5–8,5 m), màu trăng, lệch về phía trăng. Thử spot từ hướng trăng thì đường phía trước tối hơn, nên đã bỏ.
- Sương đặt ở chỗ đất thấp (hõm dưới đường men sườn), mỏng trên đường, lớp cao thưa hơn. Không còn rải đều mỗi 45 m.
- Grade, exposure, SSAO, shadow và các asset dùng chung không sửa.

### 13.5 Kiểm thử (batch, tắt tiếng)
- **Apply 2 lần:**
  - scene không đổi (0 dòng khác, bỏ qua fileID), asset md5 giống hệt;
  - census: 5840 object, 4416 collider, 1377 renderer, 39 đèn, 22 link.
- **Forest probe đầy đủ: PASS** (448 s chạy thực). Gồm:
  - bố cục 27 quyết định mỗi tuyến, đoạn trống dài nhất 6,6 / 6,0 s;
  - hai tuyến chạy sạch: 116,8 / 116,4 s, không va, lính gần nhất 5,0 m;
  - âm thanh: không bước trên không, không bước ngay lúc đáp;
  - nhạc;
  - sai một lần thì gỡ lại được; sai nhiều thì bị bắt và tính một lần;
  - rãnh, bắt lại, nhảy vào tre thấp, buông Ctrl dưới tre thấp, tầm với;
  - **walls** (mới): bốn lượt áp sát bờ cả hai tuyến và hai bên, xa nhất 0,74 m ngoài mép;
  - pause, Lam.
- **Chuỗi Ch4→Ch5→Ch6→Ch7: PASS.** Ch5 lên khung đầu 2,59 s sau khi Ch4 bắt đầu thoát.
- **Ch3: PASS.**
- **Look probe:**
  - chân trời khi nhìn ngang hở nhiều nhất 0,2 %;
  - nhìn ngang hai bên hở trời 1,6–3,4 %;
  - luma trung bình 0,116 (trước 0,081), vùng đen bệt nhìn ngang 24 % (trước 51 %);
  - mặt đường phía trước luma 0,059 (trước 0,075): hơi tối hơn;
  - nhìn thẳng lên từ đỉnh nhảy thấy trời 21 % (trước 13 %).
- **Hiệu năng** (Editor, 1080p, `StoryPerfProbe -perfCh5`, cùng góc LOOK_Forest0–8):
  - trước 19,3–20,2 ms, sau 17,4–22,3 ms;
  - LOOK_Forest3/4 chậm hơn khoảng 1,7–2 ms, LOOK_Forest8 và CAM_Wide_BambooEdge nhanh hơn;
  - **chưa đạt 16,7 ms**;
  - chi phí riêng: material nền khoảng 1–1,5 ms so với material trơn; gió tre khoảng 0,3–0,9 ms so với URP Lit; SSAO khoảng 6 ms; bóng trăng khoảng 4,5 ms.
- **Số liệu lúc load:**
  - đất dựng trong 0,73 s, tre mọc trong 0,66 s;
  - 11 389 thân, 3 051 collider (trước 11 774 / 2 150), 923k đỉnh tre (trước 782k), 499 dương xỉ.

### 13.6 Chưa kiểm / giới hạn
- **Chưa xem bằng mắt trong Play Mode.** Người dùng không cho xuất ảnh, nên mọi đánh giá hình ảnh ở trên chỉ là số đo luma và độ hở. Cần mở Ch5 xem:
  - mép bờ và hàng rào tre có tự nhiên không;
  - lá gần và cành nhỏ;
  - vũng nước;
  - đá chìm có lộ đáy scan không;
  - gió có quá mạnh hay "cao su" không;
  - đường phía trước có đủ sáng để đọc không;
  - chuyển LOD khi lia chuột.
- Lượt walls trên đường trăng bên trái dừng ở rãnh 233 m (bot rơi vào rãnh). Đoạn 233–280 m bên trái đường trăng chỉ được kiểm bằng phân tích tầm với, chưa bằng bot.
- Asset Ch5 không còn dùng, chưa xoá (cần hỏi): `Materials/Chapter05Forest/C5F_Path.mat`, `RockMoss.mat`, `Rock_0–3.asset`.
- File gốc tải về nằm ở `ThuyKieu_Share/ArtDownloads/PolyHaven` (ngoài project).
- Nguồn và giấy phép: `Art/Environment/Chapter05/CREDITS.md`. Toàn bộ CC0, Poly Haven: Brown Mud 02 (Rob Tuytel); Fern 02 (Rob Tuytel scan, Rico Cilliers model); Rock Moss Set 01 (Kless Gyzen).
- Exe vẫn là bản cũ. Chưa commit.

### 13.7 Sửa sau khi người dùng mở game (26/10/2026)
- **Lỗi sương ở shader tre (đã sửa).** Người dùng thấy tre, lá và bụi chỉ còn bóng xám phẳng, như mất hình.
  - Nguyên nhân: `Ch5Bamboo` tính sương từ `SV_POSITION.z` trong fragment, tức độ sâu màn hình chứ không phải độ sâu clip-space, nên mọi thứ dùng shader này bị sương phủ kín.
  - Cách sửa: tính hệ số sương ở vertex (`ComputeFogFactor(positionCS.z)`) rồi nội suy xuống fragment.
  - Hệ quả: các số đo luma ở §13.5 đo trên ảnh lỗi và không còn đúng. Số đúng (look probe sau sửa, so với đợt 3):
    - luma khi đứng 0,102 (trước 0,081);
    - vùng đen bệt khi nhìn ngang 32–35 % (trước 46–51 %);
    - mặt đường phía trước 0,062 (trước 0,075);
    - chân trời khi nhìn ngang hở nhiều nhất 0,2 %;
    - nhìn thẳng lên từ đỉnh nhảy thấy trời 21 %.
  - Độ bóng của đường và đèn gợi ý vật cản (đã tăng lúc đo trên ảnh lỗi) vẫn giữ.
- **Bỏ tiếng mưa trong rừng.** Theo yêu cầu, trong rừng không có mưa.
  - Tắt `SetAmbience("forest_rain")` ở hai chỗ: lúc ở bìa rừng (`Chapter05World.PlaceAtForestEdge`) và khi cuộc chạy bắt đầu (`BambooChaseSequence`). Cả đoạn chạy giờ không có nền âm.
  - Tiếng xoẹt xoẹt người dùng nghe thấy chính là nền mưa tổng hợp từ nhiễu trắng (`ProceduralAudio`).
  - Cảnh mưa sau cầu (`Escape_Rain`: mưa hạt, tiếng mưa, sương dày) vẫn giữ, đang chờ người dùng quyết định.
- **Kiểm lại sau hai bản sửa:**
  - forest probe đầy đủ PASS (51 mục, có walls và Lam);
  - look probe PASS;
  - chưa đo lại hiệu năng: máy chạy pin (19 %), lần đo cho kết quả 45–190 ms, không dùng được. Cần đo lại khi cắm điện.
- **Bỏ cả đoạn mưa sau cầu** (theo yêu cầu; cả chương giờ là một đêm khô):
  - `Chapter05World`: bỏ field `_rain`, mưa hạt, tiếng `forest_rain` và việc nhân sương 1,8 lần. Coroutine đổi tên thành `ClearOfTheForest`, vẫn giữ trăng mờ hơn ở cổng chùa.
  - `Chapter05Setup` không tạo mưa nữa.
  - `Chapter05Forest.Mood` xoá vật thể "Rain" dưới camera của scene thật. Thao tác qua Unity, không sửa YAML tay.
  - Kiểm: chuỗi Ch4→Ch5→Ch6→Ch7→Epilogue PASS.
  - Còn lại: lời thoại vẫn nhắc mưa. Ch5 ("Trời đổ mưa", "giữa trời mưa", lời chị Lam "thấy lại trời mưa"), Ch6 ("Mưa đã tạnh", "Mưa cả đêm"). Tên nội bộ `Escape_Rain`, `Hero_Kieu_Rain`, `RainSpot` giữ nguyên.
- **Lời thoại không còn mưa** (theo yêu cầu):
  - Ch5: "Gió lùa qua rặng tre, rồi lặng hẳn." / "Kiều đứng giữa khoảng sân đất trước cổng chùa… Tóc bết mồ hôi vào má…" / chị Lam: "…chị mới thấy lại bầu trời ngoài bức tường ấy."
  - Ch6: "Sáng. Sương đêm đã tan." / Kim Trọng: "Nàng... sốt rồi. Chạy suốt đêm trong sương lạnh, nàng..."
  - Cũng sửa: tóm tắt cuối Ch5 ("một mái chùa trong sương sớm...") và bằng chứng "Lời Kim Trọng" ở phiên xử ("Gặp Kiều ở cổng chùa Bạch Vân, đêm nàng trốn khỏi lầu.").
  - Mưa ở Ch1 (montage, chiếc thoa) giữ nguyên.
  - Ink biên dịch lại, INK VALIDATION PASS (chapter_5 và chapter_6, mỗi chương 1500 lượt).
  - Năm câu đã đổi giờ chưa có giọng đọc: khoá giọng là hash của người nói và lời. Cần tạo lại TTS (dịch vụ mạng, chưa chạy).

## 14. FPS toàn game (27/10/2026)
- **Nguyên nhân chính:** preset chất lượng PC (`Assets/Settings/PC_RPAsset.asset`, mức mặc định) render ở render scale 2 cộng MSAA 8×. Ảnh nội bộ có số pixel gấp 4 lần màn hình, mỗi pixel 8 mẫu.
  - Các lần đo trước dùng render texture 1×, nên thấp hơn thực tế: không tính MSAA, nhưng có tính render scale 2.
  - `StoryPerfProbe` giờ dùng MSAA của pipeline. Thêm hai cờ: `-perfQuick` (mỗi góc đo một lần) và `-perfSet` (thử thiết lập trong bộ nhớ, đo xong đặt lại, không lưu).
- **Đã đổi** (công cụ `StoryQuality.ApplyPc`, menu ThuyKieu ▸ Tools): render scale 2 → 1 và MSAA 8× → 4×. Trong asset chỉ khác hai dòng này.
  - Shadow 4096 / soft High / SSAO full giữ nguyên: thử hạ chúng chỉ được thêm khoảng 0,5 ms.
- Lá cut-out của Ch5 (shader `Ch5Bamboo` và dương xỉ scan dùng URP Lit) bật alpha to coverage (`_AlphaToMask`) để MSAA làm mượt cả mép lá.
- **Kết quả** (Editor 1080p, MSAA thật, tất cả góc máy; trung bình, trước → sau, ms):

| Chương | Trước | Sau |
|---|---|---|
| Prologue | 18,7 | 10,8 |
| Ch1 | 23,9 | 12,6 |
| Ch2 | 25,4 | 12,7 |
| Ch3 LauTuBa | 21,0 | 9,9 |
| Ch3 TuBaHouse | 16,6 | 9,2 |
| Ch4 LauTuBaDem | 22,4 | 10,8 |
| Ch4 TuBaNight | 18,4 | 8,9 |
| Ch5 | 23,2 | 9,9 |
| Ch6 | 16,7 | 7,8 |
| Ch7 | 19,9 | 9,0 |
| Epilogue | 18,0 | 8,8 |

  - Góc nặng nhất: 33,5 → 16,5 ms.
- **Kiểm:** forest probe, chuỗi Ch4→Epilogue và Ch3 đều PASS. Apply hai lần không đổi gì.
- **Đổi lại về hình:**
  - Mất siêu lấy mẫu 2×, nên răng cưa và lá xa có thể nhấp nháy rõ hơn.
  - Ch5 hở trời nhiều hơn chút: chân trời xa nhất 1,0 % ở cầu (trước 0,2 %); nhìn thẳng lên từ đỉnh nhảy 29 % (trước 21 %). Bật lại mip giữ độ phủ cho tán và card chân trời không đổi được con số này mà tốn thêm khoảng 0,5 ms, nên đã bỏ.
  - Chưa xem bằng mắt.
- Exe vẫn là bản cũ; cần build lại mới có thiết lập mới. Chưa commit.
