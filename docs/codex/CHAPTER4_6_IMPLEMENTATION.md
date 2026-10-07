# Chương 4, 5, 6 — Tài liệu bàn giao

Nguồn thiết kế: `Doc/Chapter 02 – Trên đường theo Mã Giám Sinh Mới.docx`, các mục *Chapter 04 – Không còn đường lui*, *Chapter 05 – Đào thoát*, *Chapter 06 – Kim Trọng*.
Nhánh git: `feature/chapter02-03` (nối tiếp Ch2–3). Chương 7 và Epilogue chưa làm: Ch6 kết thúc ở `Chapter07_Placeholder`, hồ sơ bằng chứng đã lưu sẵn trong Ink cho Ch7.

## 1. Chơi thử

| Cách | Làm gì |
|---|---|
| Chơi nối từ đầu | `Chapter01_GiaBien.unity` → Play. Ch1 → Ch2 → Ch3 → **Ch4 → Ch5 → Ch6** → màn chờ Ch7. Lựa chọn và manh mối được mang theo. |
| Vào thẳng một chương | Mở `Chapter04_TuBaNight` / `Chapter05_Escape` / `Chapter06_KimTrong` → Play. Truyện bắt đầu từ knot `chapter_4/5/6` với biến các chương trước ở giá trị mặc định. |

**Điều khiển mới:** **Q** ném sỏi đánh lạc hướng (Ch4) · **E** trốn / ra khỏi chỗ trốn · **giữ E** để móc chìa (Ch4) hoặc đẩy cửa ngõ (Ch5) · **E** khi chạy ngang để xô đổ xe hàng, thúng mủng (Ch5) · **Space / A / D / E** theo phím hiện trên màn hình trong đoạn rượt đuổi qua rừng tre.

## 2. Thiết kế đã hiện thực

### Chương 4 — Không còn đường lui (≈ 10–12 phút) · lầu Tú Bà về đêm
1. **"Dạy quy củ"**: Tú Bà vào phòng Kiều. Lựa chọn có giờ đặt `c4_canh_giac`:
   - **Cúi đầu** → 0.
   - **Hỏi khéo số nợ** → 1, và biết sổ nợ nằm ở đâu.
   - **Im lặng** → 1.
   - **Cãi lại** → 2: bị tát, và một **lính gác ngồi ngủ gật trước cửa phòng** (thức 4–6 s, gật 6–8 s).
2. Mục tiêu **TÌM CÁCH RỜI KHỎI NƠI NÀY**, khám phá tự do. Toàn bộ sân, sảnh, phòng Tú Bà, phòng sổ sách và bếp là **khu vực cấm ban đêm**; chỉ phòng Kiều và hiên phía tây là an toàn.
   - *Quan sát Tú Bà* (chuột phải 3,2 s): mụ đi lại giữa sảnh tiếp khách và phòng riêng, mỗi lần về phòng ngồi lâu hơn. Sau khi quan sát, lần về phòng tới mụ **ngủ gật**.
   - *Lấy chùm chìa*: **móc túi**. Giữ E chỉ khi mụ *thở ra*; nhích tay lúc mụ *hít vào* thì thanh "cựa mình" đầy, Kiều phải rụt tay và mất 60 % tiến độ.
   - *Mở tủ khóa đồng* (cần chìa): đọc **Giao kèo Mã Giám Sinh – Tú Bà** (lập tháng Giêng, ba cái tên trước Kiều đã gạch), kèm câu thoại theo tài liệu thiết kế. Sau đó có hai lựa chọn có hệ quả:
     - Văn tự bán thân: **mang theo / đốt** (có cảnh Kiều thật) **/ để lại** (Tú Bà phát hiện muộn).
     - Tiền: **lộ phí / cả túi bạc có dấu "T."** (bằng chứng, nhưng chạy nặng hơn và bị nghi là trộm) **/ không lấy**.
   - *Xé trang sổ nợ* trong ngăn bàn → bằng chứng `ev_giay_no`.
   - *Tìm cửa sau* trong bếp.
   - *Cô gái áo lam chưa ngủ*: **rủ đi cùng / hứa quay lại / im lặng** (có giờ).
3. **Hệ thống lẩn tránh**:
   - **Độ lộ diện** theo ánh đèn (thanh dưới màn hình): đứng dưới đèn lồng bị thấy nhanh gấp gần 3 lần so với trong tối.
   - **6 chỗ trốn**: tủ áo, chum vại, chum nước, bếp lò, mành tre, bình phong.
   - **Ném sỏi (Q)**, có 3 viên, được bù khi quay về checkpoint: lính canh nghe tiếng và đi tới xem.
   - Lính canh **say** (đi chậm, dừng lâu, cầm đèn lồng, nón tầm nhìn đỏ) với biểu tượng **? / !** trên đầu.
   - Tú Bà cũng có tầm nhìn khi đi lại.
   - Bị bắt → thẻ thất bại → về phòng Kiều (checkpoint), `c4_bi_bat` + 1.
4. **Cửa sau** (cần có giao kèo và đã tìm ra cửa sau) → giọng Kiều thật → sang Ch5.

### Chương 5 — Đào thoát (≈ 8–10 phút) · sân sau → ngõ / chợ đêm → rừng tre → cổng chùa trong mưa
1. **Lẻn ra** (`sneak_out`):
   - Sân sau có lính canh cầm đèn đi tuần.
   - **Chó xích** canh bằng tai: chạy trong vòng 8 m, đi thẳng người trong vòng 3,6 m, hoặc khom trong vòng 1,6 m là nó sủa và gọi lính canh tới.
   - Dây phơi quần áo che tầm nhìn.
   - 3 chỗ nấp: đống rơm, đống củi, chuồng gà.
   - **Cửa ngõ**: giữ E để đẩy, thanh "tiếng bản lề" đầy là cửa kêu, lính canh tới xem.
   - Kết quả ghi vào `c5_lang_le` (0–3).
2. **"Bắt nó lại!"**: Tú Bà xuất hiện ở cửa bếp. Câu thoại khác nhau tùy việc mụ có phát hiện tủ bị mở hay không.
3. **Rượt đuổi tự do** (`chase_alley`):
   - 3 gia nhân đuổi theo trên NavMesh. "Dây thun" giữ khoảng cách: xa thì chúng chạy 6,2 m/s, gần thì 4,3 m/s. Kiều chạy nước rút được 5,5 m/s.
   - **Thể lực** (Shift): cạn thì phải đi bộ. Mang cả túi bạc làm hao nhanh hơn ×1,35, có chị Lam đi cùng thêm +0,15.
   - **Ngã rẽ**: *ngõ tắt tối* (ngắn) hoặc *chợ đêm* (dài, đông người làm người đuổi chậm lại).
   - **7 vật cản xô đổ được**, người đuổi lội qua chỉ còn 35 % tốc độ.
   - Bị bắt → làm lại đoạn rượt đuổi.
4. **Rừng tre** (`chase_forest`): chạy theo đường định sẵn, camera bám theo:
   - 4 QTE: Space nhảy rễ, A/D né cành, bấm E liên tục để trèo dốc. Thời gian chậm lại còn 55 % trong lúc QTE.
   - Có chị Lam đi cùng thì thêm QTE **kéo chị Lam dậy**. Trượt QTE này → chị bị bắt lại (`c5_co_lam_thoat = false`).
   - Bấm sai phím cũng tính là vấp ngã. Bị đuổi kịp → làm lại đoạn rừng.
5. **Mưa**: *"Mình thoát rồi… Nhưng chỉ trốn thôi chưa đủ. Mình phải quay lại."* Sau đó là câu thơ Truyện Kiều, cái tên Kim Trọng, và cổng chùa Bạch Vân.

### Chương 6 — Kim Trọng (≈ 12–15 phút) · chùa Bạch Vân → thư phòng → phố huyện Lâm Tri
1. **Gặp ở chùa**:
   - Kim Trọng mở đầu bằng *"Kiều? Vì sao nàng lại ở đây?"*.
   - Lựa chọn có giờ quyết định **độ tin tưởng** `kt_tin_tuong`: *nói thật / chỉ đưa giấy / kể chuyện xuyên không* (`kt_biet_xuyen_khong`).
   - Kim Trọng đọc giao kèo, tự trách mình, rồi quyết định giúp.
2. **Thư phòng – Đối chiếu giấy tờ** (`doi_chieu`):
   - Hai tờ giấy đặt cạnh nhau, người chơi chỉ ra chỗ mâu thuẫn. Thứ tự đáp án được xáo ngẫu nhiên.
   - Tờ khế hôn ước Ch1 đối chiếu với văn tự hoặc giao kèo: "chồng" và "kẻ bán".
   - Ngày tháng Giêng đối chiếu với tháng Tư: hắn đã sắp đặt từ trước.
   - Câu thứ 3 tùy những gì người chơi mang theo: trang sổ nợ, mẩu giấy Ch2, hoặc ba cái tên bị gạch.
   - Ghi `c6_doi_chieu`. Đạt từ 2 trở lên thì mở được cách thuyết phục thư lại bằng ngày tháng.
3. **Phố huyện – Tìm nhân chứng** (thứ tự tùy ý). Cơ chế chung: **đưa đúng bằng chứng**, đưa sai 2 lần thì người đó không nói nữa.
   - *Bà hàng nước* (quán nước ở Ch2): thuyết phục được bằng mẩu giấy Ch2, câu nghe lén Ch2, hoặc Kim Trọng nói giúp khi tin tưởng ≥ 4. Đưa khế ước thì bà không biết chữ; đưa bạc có dấu thì bà sợ.
   - *Thư lại huyện đường*: thuyết phục được bằng giao kèo cộng ngày tháng (cần đối chiếu ≥ 2), hoặc văn tự đã mang theo, hoặc bạc có dấu kèm Kim Trọng bảo lãnh (tin tưởng ≥ 4). Ông xác nhận Mã vào sổ hôn thú 4 lần trong một năm.
   - *Hiệu bạc Phúc Ký*: Kim Trọng đứng ra khi tin tưởng ≥ 3, hoặc **đút lót** vài lạng (có câu tự vấn). Sổ ghi Tú Bà giao 450 lạng cho "Mã công tử" → `ev_tien`.
   - *Chị Lam* (nếu đã thoát cùng): an ủi hoặc đưa trang sổ nợ → nhân chứng, chị nói ra tên mình.
   - **Người của Tú Bà** đi tuần trên phố: chỉ để ý ai **chạy**. Bị nhận ra → checkpoint đầu phố.
4. **Hồ sơ** (`ho_so`): 4 ô *Khế ước · Nhân chứng · Giấy nợ · Tiền bạc* hiện **ĐỦ / THIẾU** → `c6_ho_so` (0–4).
5. **Đêm trong thư phòng**:
   - Kim Trọng hỏi người ta kể về chàng thế nào (nếu biết chuyện xuyên không).
   - Nội tâm nhận ra Kim Trọng là người thật, rồi Kiều thật lên tiếng.
   - Gia nhân báo Mã và Tú Bà đã kiện "kẻ trộm" → *"Sáng mai, ta sẽ đến huyện đường trước chúng."* → Ch7.

### Biến Ink cho Chương 7
`ev_khe_uoc`, `ev_nhan_chung` (0–3), `ev_giay_no`, `ev_tien`, `c6_ho_so` (0–4), `nc_ba_nuoc`, `nc_thu_lai`, `nc_co_lam`, `kt_tin_tuong`, `kt_biet_xuyen_khong`, `c4_tien`, `c4_van_tu`, `c4_ru_co_lam`, `c5_co_lam_thoat`. Knot `chapter_7` hiện là stub trong `Main.ink`.

## 3. Kiến trúc (bổ sung so với Ch2–3)

| Thành phần | File | Vai trò |
|---|---|---|
| Kịch bản | `Data/Dialogue/Ink/Chapter4_KhongConDuongLui.ink`, `Chapter5_DaoThoat.ink`, `Chapter6_KimTrong.ink` | Biến khai báo trong `Main.ink` |
| Thế giới | `Gameplay/Chapters/Chapter04World`, `Chapter05World`, `Chapter06World` | Nhịp NPC, chuyển cảnh, checkpoint, thẻ tổng kết |
| Lẩn tránh | `Gameplay/Stealth/HideSpot`, `PebbleThrow`, `LightExposure`, `AlertIcon`, `DogSentry`, `Stamina`; `GuardPatrol` có thêm *Investigate* và say; `VisionSensor` có thêm độ nhạy, ánh đèn, trốn, chế độ chỉ thấy người chạy | |
| Rượt đuổi | `Gameplay/Chase/ChaseObstacle`, `Follower` | Vật cản xô đổ được; người đồng hành đi theo |
| Sequence | `PickpocketSequence`, `SneakOutSequence`, `ChaseSequence`, `ForestRunSequence`, `ComparisonSequence`, `DossierSequence` | Kích hoạt bằng `# gameplay:<id>` |
| UI | `UI/DossierView` (đối chiếu, hồ sơ); `StoryHUD.SetGauge` (thể lực / lộ diện), `ShowQte` / `HideQte` | Thêm vào `StoryUIBuilder` |
| Người chơi | `PlayerMovement.IsHidden`, `CanRun`, `ScriptedMove` (chạy theo kịch bản vẫn dùng animation thật); `ThirdPersonCameraController.CutTo` (cắt cảnh xuyên tường) | |
| Dựng scene | `Core/Editor/Chapter04Setup` (dùng lại nhà Ch3 qua `Chapter03Setup.Night`), `Chapter05Setup`, `Chapter06Setup` (kèm `Chapter07_Placeholder`) | |

Nhân vật mới trong `SpeakerCatalog`: `KimTrong`, `BaHangNuoc`, `ThuLai`, `ChuHieuBac`, `GiaNhan`.

## 4. Dựng lại scene & kiểm thử
- **Dựng lại tất cả scene Ch2–6**: `ThuyKieu > Story > Rebuild Chapter 2–6 scenes`, hoặc chạy batch `-executeMethod ThuyKieu.Core.Editor.StoryBuild.RebuildBatch`. Lệnh này cũng dọn khỏi build settings các scene đã bị xoá.
- **Ink**: `ThuyKieu.Dialogue.Editor.InkStoryValidator.RunBatch` chạy 6 chương × 1.500 lượt. Validator mô phỏng các sequence ghi biến, ngẫu nhiên hoá kết quả chương trước, và kiểm biên `nghi_ngo`, `tien`, `c6_ho_so`.
- **Autopilot**: `StoryBuild.PlaytestBatch -playScene Assets/_Game/Scenes/Chapter04_TuBaNight.unity -storyAutopilot -autopilotChain [-autopilotThorough] [-autopilotCapture <thư mục>]` chạy nối từ chương được chọn tới hết Ch6.
  - Chế độ `-autopilotThorough` ép mỗi chương thua một lần: Ch4 qua checkpoint; Ch5 ở cả ba đoạn lẻn ra, chạy trong ngõ, chạy trong rừng; Ch6 qua checkpoint.
- **Lồng tiếng** cho câu thoại mới:
  1. Chạy `ThuyKieu > Dialogue > Export voice manifest`. Manifest đã quét đủ Ch1–6, kể cả câu hô của gia nhân.
  2. Chạy `python Tools/Voice/generate_voice.py`.

## 5. Model
- **Kim Trọng**: OBJ tĩnh (`Art/Characters/KimTrong/Static/KimTrong.obj`), tay hạ bằng `RelaxArms`, cử động giả lập nhún và thở. Khi có bản rig Mixamo: thay `StaticActor(...)` bằng `RiggedActor(...)` trong `Chapter06Setup`, rồi Rebuild.
- **Tú Bà, Lính canh**: vẫn là model tĩnh như Ch2–3. Tú Bà "ngủ gật" chỉ thể hiện bằng tiếng ngáy và lời thoại, vì model tĩnh không ngồi được.
- **Gia nhân, khách, người dân, thư lại, chủ hiệu bạc**: model Mã Giám Sinh đổi màu. **Bà hàng nước, chị Lam**: model Mẹ Kiều đổi màu. Tất cả đều có animation thật.

## 6. Âm thanh (procedural)
- **Nhạc**: `chase`, `temple`, `study`, `street`.
- **Nền**: `night_house` (nhạc tiệc vọng xa), `forest_rain`, `market_night` (tiếng người), `temple` (chuông), `street`.
- **Hiệu ứng**: chó sủa, chiêng, tát, mở khoá, lửa, gõ cửa, đổ bát, bản lề kẽo kẹt, ngáy, vấp ngã, gió ném sỏi, QTE đúng/sai.

## 7. Ghi chú & giới hạn
- Ch4 dùng lại hàm dựng nhà Ch3. Đổi bố cục nhà Ch3 thì Ch4 đổi theo; cửa sau và bếp chỉ có trong bản đêm.
- Độ khó đặt ở mức *vừa phải, checkpoint dày* theo yêu cầu. Thông số nằm trong Inspector của `GuardPatrol`, `VisionSensor`, `ChaseSequence` (`_catchDistance`, tốc độ dây thun) và `ForestRunSequence` (`_window`, `_mashCount`).
- Thanh "lộ diện" chỉ tính đèn point/spot. Đèn lồng không có `Light` thì chỉ là trang trí.
