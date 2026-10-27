# Prompt cho Claude — Ch5: âm thanh rừng, nhạc truy đuổi, nhịp vật cản và POV

Sau khi hoàn thành lượt này, yêu cầu mới ngày 26/10/2026 về độ chân thực đồ họa nằm trong [CLAUDE_CH5_FOREST_REALISM_PASS.md](CLAUDE_CH5_FOREST_REALISM_PASS.md). Lượt đồ họa phải giữ gameplay/âm thanh/POV đã hoàn thiện.

Hãy thực hiện trên scene Chapter 5 thật và builder tương ứng. Người chơi đã thử bản mới và phản hồi: âm chạy/nhảy/tre đổ chưa thuyết phục, thiếu nhạc kịch tính, vật cản quá ít và dễ, POV chưa tạo đủ cảm giác chạy thoát thân. Đây là lượt hoàn thiện tiếp theo, giữ rừng tre kín và âm u vừa làm.

**Xác nhận mới nhất của người chơi:** đường hiện tại là **đường đất và đá**. Đây là hai bề mặt footsteps chủ đạo; không mặc định là bùn chỉ vì trời mưa hoặc vật liệu trông ướt. Không thêm tiếng cỏ/lá hoặc rải thêm cỏ/lá xuống đất chỉ để hợp với nguồn âm gợi ý trước đây.

**Đã kiểm tra project và Play Mode ngày 25/10/2026:** scene Ch5 tham chiếu đúng `C5F_Floor` và `C5F_Path`; cả hai dùng texture `brown_mud_02_diff_1k.jpg`. Đã xem texture gốc và khung đường đang chạy: bề mặt đất sẫm, gồ ghề dạng hạt/cục nhỏ, người chơi đọc là đất lẫn đá; không có đá lát đều hoặc lớp bùn nước lún. Tên texture không đủ để quyết định tiếng bùn. Quan trọng: `ForestGround.cs:361` gán `Surface.Mud` cho mọi ô near ground có collider, còn path strips chỉ là lớp hình ảnh. Vì vậy footsteps hiện có thể vẫn chọn nhóm mud dù người chơi nhìn thấy đường đất/đá. Phải sửa phân loại này cùng âm, không chỉ thay file stone rồi cho rằng đường đã đổi tiếng.

Nguồn cụ thể và các file đã tải thử: xem `CH5_GROUND_AUDIO_AUDIT.md` cùng thư mục nghe thử `audio-preview/`. Ưu tiên cú chạm đất + ít tiếng hạt/sỏi, tránh tiếng hút bùn hoặc tiếng gót trên bê tông vang cứng. Không suy rằng toàn bộ texture là sỏi rời; cần nghe mix trong cảnh thật.

**Người chơi đã duyệt mẫu tiếng sỏi:** sau khi nghe `docs/concept/audio-preview/candidate-gravel-run-preview.wav` (7,5 giây, ghép từ 10 take gravel của Ali_6868 trong bản biên tập congusbongus), người chơi phản hồi “ổn”. Chốt bộ này làm nguồn tiếng sỏi; không cần tìm lại nguồn thay thế. Khi triển khai, dùng từng take gốc đã tải và xử lý phù hợp, phát theo bước chân thật; không phát nguyên file preview thành loop. Sự chấp nhận này áp dụng cho chất âm mẫu sỏi, chưa xác nhận mix trong mưa/nhạc, tiếng đất, jump/landing hoặc âm lượng cuối cùng trong game.

## 1. Phạm vi và trạng thái cần giữ

- Xác nhận đúng project đang mở: `D:\PRU213\ThuyKieu_Share-20261009T182626Z-1-001\ThuyKieu_Share\ThuyKieuGame_With_Unity`. Lần kiểm tra 25/10/2026 là `dev`, HEAD `021f57d`, có nhiều thay đổi chưa commit. Đọc AGENTS.md và phần mới nhất của `docs/codex/CHAPTER5_BAMBOO_CHASE_IMPLEMENTATION.md`; không reset hoặc ghi đè công việc đang có.
- Giữ FPS auto-forward, tự lái chuột/A-D, Space nhảy, Ctrl cúi, E đạp cọc cầu; không rail, QTE, stamina hoặc tay Kiều bắt buộc. Giữ lính người cầm đuốc, Lam, dữ liệu Ink/checkpoint, retry từ đầu rừng và nối Ch6.
- Vào Ch5 phải đang chạy ngay; không đưa lại thẻ chương, thoại chặn, sân bếp/cutscene đầu. Nhánh Sở Khanh hiện còn trong Ink nhưng chưa có chỗ gọi: không tự đặt lại cốt truyện trong lượt này.
- Chỉ đổi Ch5. Nếu phải sửa lớp dùng chung, tính năng mới mặc định tắt, Ch3/Ch4 giữ hành vi và âm thanh đang được chấp nhận. Không đổi tầm bắt, tốc độ lính hoặc độ sáng rừng chỉ để tạo cảm giác khó hơn.
- Không tự commit/push/merge, cài package, build exe, quay clip hay xuất ảnh. Không sửa YAML scene/prefab bằng tay. Dùng đường cập nhật scene tại chỗ đang có, giữ GUID.

## 2. Những gì đã kiểm tra được trong source — đừng làm trùng hoặc kết luận sai

- `BambooChaseSequence` đã gọi `PlayMusic("chase")` và ambience `forest_rain`. Có file `Assets/_Game/Audio/Resources/StoryAudio/music/chase.wav`; chưa kết luận chất lượng bằng tai. `AudioBank` ưu tiên Recorded → baked → synth; `StoryAudio` còn có overrides/Foley. Trước khi thay, trace đúng clip thực sự phát trong Play mode, gain và tuyến mix. Không báo “trước đây không có nhạc” chỉ vì nhạc chưa nổi bật.
- `Chapter05Forest.ChapterFoley()` sao chép các nhóm `step_*` từ Ch4 sang `Ch5FoleyBank.asset`: footsteps đã có bản thu, không phải tất cả đều tạo bằng code. Tuy nhiên jump hiện gọi `cloth`, landing gọi `step_mud` hoặc `stumble`, tre đổ dùng `creak/whoosh/stumble/clatter`, cầu dùng cả `slide_slam`. Cần thay đúng những lớp còn giả, không chỉ tăng volume hoặc đổi tên id.
- Builder `Chapter05Forest.Course.cs` có 15 mốc gồm cả hai nhánh và cầu; một lượt chỉ đi khoảng 12 hoặc 14 mốc. J1 ở 62 m, D1 104 m, H1 150 m; chạy 5 m/s nên vài khoảng đầu dài khoảng 8–9 giây, về sau có khoảng 83 m giữa hai mốc nhảy. Đếm lại cả vật cản thực tế, khúc cua và tín hiệu trên scene; số mốc không phải toàn bộ độ khó.
- Camera đã có `_fov = 74`, `_bob = 0.035`, landing Dip, va chạm Shake. `ThirdPersonCameraController.Bob()` dùng nhịp theo quãng đường 1,3 m, trong khi bước chạy `FirstPersonFootsteps` dùng stride 1,35 m và phát `Stepped`. Hai nhịp có thể trôi nhau. `ScreenFx` đã dùng Perlin và chỉ áp shake trong lúc render, có `GameSettings.ShakeAmount`. Tận dụng thay vì cộng thêm nhiều bộ rung độc lập.
- Catch hiện dựa `_pursuer.CanCatch(player)`, không phải số lần vấp. Giữ bắt theo khoảng cách, đường đi và vật chắn thật; tuyệt đối không thêm luật “sai hai lần là chết” hoặc teleport lính cho đủ kịch tính.
- Editor 1080p lần báo cáo trước khoảng 19–21 ms/khung, đã quá ngân sách 60 FPS. Không xem việc thêm hiệu ứng là miễn phí; cần đo cùng điều kiện trước/sau.

## 3. Tham khảo: nguồn và cách áp dụng

Đây là định hướng thiết kế cho Thúy Kiều, không phải khẳng định các game dùng cùng code/các con số ở dưới. Codex tra cứu được mô tả/trang nguồn, không xem trực tiếp được các video YouTube và chưa nghe các file âm thanh. Claude chỉ ghi đã xem/nghe khi thực sự có công cụ làm được.

- **DOORS / Seek chase**: hai video người dùng đã đưa: https://www.youtube.com/watch?v=SJ44-Z1ZXTQ và https://www.youtube.com/watch?v=KSiY3JuSte8 . Giữ ý tưởng chạy liên tục, nhận diện hướng đúng và tăng chuỗi thao tác. Không lấy âm nhạc/âm thanh/asset từ game đó, không thêm năng lực siêu nhiên cho lính.
- **Mirror’s Edge Catalyst**, nguồn EA: https://www.ea.com/amp/news/the-world-from-faiths-perspective ; gameplay chính thức EA UK: https://www.youtube.com/watch?v=dm1fMLmxIQo . EA nhấn mạnh trải nghiệm chuyển động ở góc nhìn thứ nhất. Đề xuất cho Ch5: POV thể hiện trọng lượng lúc bật nhảy, đáp và lách người, trong khi mắt vẫn đọc được đường. Không sao chép parkour siêu nhân hoặc ép người chơi nhìn xuống chân liên tục.
- **Dying Light**, bài giới thiệu talk của Bartosz Kulon/Techland tại GDC 2018: https://www.gamedeveloper.com/design/video-how-techland-built-i-dying-light-i-s-remarkable-parkour-system ; video GDC được bài dẫn: https://www.youtube.com/watch?v=uikbLyi-cug . Dùng làm tham khảo về chuyển động FPS và việc giải quyết trở ngại. Nếu xem được, ghi chú ngắn điều thực sự quan sát được về nhịp nhảy/đáp và camera; không bịa timestamp.

Áp dụng tổng hợp: đường chạy có nhịp như chase trong DOORS, cảm giác trọng lượng của chuyển động FPS, sợ hãi đến từ lính đang tới gần, âm rừng và hiểm họa thật. Camera hỗ trợ cảm giác này; không dùng rung liên tục để thay cho thiết kế màn chơi.

## 4. Foley và không gian âm thanh

### Footsteps

- Dùng bước chạy có lực đạp và khoảng nghỉ rõ, theo đúng tốc độ/quãng đường thực của Kiều. Hai nền chủ đạo là đất và đá: đất có cú chạm đục, mềm hơn; đá có cú chạm cứng và sắc hơn. Chỉ thêm tiếng sỏi lạo xạo nếu thực sự có đá vụn/sỏi rời; không làm đá liền khối nghe như sỏi. Gỗ cầu chỉ phát ở mặt ván. Đối chiếu hình ảnh với collider/surface tag; sửa tag sai thay vì ép âm theo một mốc thời gian. Mưa không tự biến đất thành bùn lún; tiếng dính bùn hoặc nước tóe chỉ dùng nếu scene có đúng vùng bùn/vũng nước thực sự.
- Ưu tiên 6–10 take dùng được cho các nền chính nếu nguồn có đủ take khác nhau thật; không nhân bản file rồi gọi là biến thể. Không lặp take ngay trước, pitch chỉ dao động nhỏ. Giữ cảm giác cùng một nhân vật/cùng loại giày; loại tiếng boot nặng, giáp hoặc gót cao không hợp Kiều.
- Không layer tiếng cỏ/lá dưới chân ở những đoạn đất/đá. Chỉ cân nhắc lớp này nếu scene thật có một vùng lá/cỏ nhìn thấy được và Kiều thực sự giẫm lên nó; hiện không coi đây là hạng mục cần bổ sung. Landing phải theo đúng đất, đá hoặc ván tại chỗ đáp, không luôn gọi `step_mud` chỉ vì chạy trong mưa. Tiếng lá tre trên tán hoặc khi thân tre đổ là sự kiện riêng, không phải footsteps.
- Không có bước trên không, lúc pause, fail/catch, teleport/reset, hoặc đứng đẩy vật chắn mà không di chuyển. Tiếp đất không phát cùng lúc một landing mạnh cộng hai bước thường tạo thành ba bàn chân. Âm lính ở sau phải phân biệt được với chân Kiều và có hướng/khoảng cách.

### Nhảy, đáp, cúi, va chạm

- Nhảy: một tiếng đạp nền nhẹ + vải chuyển động; không dùng whoosh lớn như võ thuật. Đáp: take theo vật liệu và vận tốc rơi thực, lực tăng có giới hạn. Vượt nhảy sạch không phát âm “vấp”.
- Cúi qua tre: vải/lá chỉ khi có tiếp xúc hợp lý. Va thân/cụng đầu có tiếng riêng, ngắn và rõ; không kêu đau hay gầm mỗi lần né thành công.
- Hơi thở Kiều tự nhiên, câu ngắn phù hợp giọng nữ trưởng thành, không kéo pitch giọng nam thành nữ. Có độ căng tăng khi chạy lâu/lính gần nhưng không chồng nhiều loop hoặc há miệng hét suốt. Nếu chưa có recording hợp lệ, giữ lớp hiện có ở mức vừa và ghi rõ còn tạm.

### Tre đổ và cầu

- Tách ba sự kiện của tre đổ: sợi/thân bắt đầu nứt báo trước → thân/lá quét qua khi đang ngã → chạm đất và lá rung lắng xuống. Âm phát từ vị trí vật thực, không dán tất cả vào camera.
- Không phát tiếng rìu chặt nếu không ai chặt cây; không dùng nổ/kim loại/đóng cửa để thay cú tre chạm đất. Mỗi lần chỉ phát một impact chính ở thời điểm chạm, không phụ thuộc FPS hay số collider contact.
- Cầu: ván kêu khi đặt chân, dây căng, cọc bị đạp, dây đứt rồi sàn rơi. Chỉ thêm nước văng nếu dưới đó thực sự có nước và deck chạm tới; đừng thêm vì muốn âm to.
- Gió, mưa, lá tre và đuốc tạo nền có chiều sâu, nhưng không che tiếng cảnh báo hoặc hướng lính. Lính là người, không có tiếng quái vật. Tiếng gọi chỉ dùng giọng có sẵn/hợp lệ, không phát lời thoại trái nhánh Lam/cốt truyện.

### Nguồn đã xác minh trang giấy phép

Tự tải các bản miễn phí phù hợp từ trang tác giả, kiểm tra lại file và điều kiện ngay lúc tải. Chỉ import phần dùng; không tải cả thư viện. Không mua, tạo tài khoản hoặc vượt đăng nhập. Chưa nghe nên các nguồn dưới là ứng viên cần audition, không bảo đảm hay chỉ từ metadata.

| Nguồn | Giấy phép trên trang | Dùng thử cho |
|---|---|---|
| SecureSubset — Footsteps - Dirt, Grass, Ground, Nature: https://freesound.org/people/SecureSubset/sounds/813621/ | CC0 trên trang tác giả | Mô tả thực là bản thu Foley đất bằng hố cát/vỏ pasta, không phải ghi âm giẫm cỏ. Ứng viên lớp đất nền; cần nghe kiểm vì cách thu không bảo đảm chất âm. Chưa tải bản gốc do cần đăng nhập |
| congusbongus — Footsteps on different surfaces: https://opengameart.org/content/footsteps-on-different-surfaces | Trang gói CC BY 3.0; `gravel/license.txt` ghi nguồn Ali_6868 CC0. Giữ credit cả người biên tập và tác giả gốc | Đã tải archive vào `FoleyDownloads/Ch5SurfaceCandidates/`, tách 10 take gravel để audition. Chỉ dùng làm ứng viên tiếng hạt/sỏi trên đất; không dùng nhóm grass hoặc boots một cách mặc định |
| Joseph SARDIN & Axeline T. — Footsteps on Gravels #4: https://bigsoundbank.com/pas-sur-graviers-4-s3216.html | CC0 | Bản thu POV bước chân trên đường núi, ứng viên đất lẫn sỏi; chưa tải được vì máy chủ trả HTTP 403 cho công cụ. Không phải bản thu chạy đã xác minh |
| Joseph SARDIN — Jumping on dirt road #1: https://bigsoundbank.com/jumping-on-dirt-road-1-s1837.html | CC0 | Bật nhảy/đáp trên đường đất; chưa tải được vì HTTP 403. Nếu tải qua thao tác tải bình thường của trang, nghe và tách đúng thời điểm, không phát nguyên cả cú nhảy ở lúc đáp |
| kheetor — tree chop fall thud: https://opengameart.org/content/tree-chop-fall-thud | CC0 | Chọn đoạn creak/fall/impact phù hợp, bỏ tiếng chặt; đây là cây gỗ, chỉ là lớp bổ trợ tre |
| Ch4 Foley đã có trong project và credits hiện tại | Xem giấy phép từng nguồn | Ưu tiên audition nhóm earth và stone cho đường đất/đá, wood cho cầu; giữ nguyên bank Ch4. Chọn theo chất âm thực, không mặc định take đá/bê tông hoặc đất nào cũng hợp |

Hai gợi ý cũ `Feet in Leaves #1` và `Running in the Tall Grass` được bỏ khỏi danh sách triển khai cho mặt đường hiện tại vì không khớp bề mặt người chơi thấy. Gói mud cũng không phải nguồn footsteps chủ đạo. Nếu nguồn earth/stone đang có chưa tốt, tìm bản thu chạy trên đường đất và đá phù hợp, xác minh giấy phép; không lấy cỏ/lá/bùn làm lớp thay thế.

Không dùng `Random Sounds Samples` theo nhãn collection/search “CC0”: trang hiện tại ghi OGA-BY 3.0. Nếu chọn nguồn khác phải đọc trang asset cụ thể và giấy phép trong archive, không suy giấy phép từ collection.

Đặt bank/nguồn mới trong phạm vi Ch5 hợp kiến trúc hiện có. Ghi URL, tác giả, tên file gốc, giấy phép, các đoạn cắt/layer và file đích trong credits. Lưu ý builder hiện gán lại toàn bộ `bank.Groups` bằng nhóm step Ch4: sửa đường dựng bank để Apply không xóa những nhóm Ch5 vừa thêm.

## 5. Nhạc truy đuổi có tiến triển

- Audit clip/mix thực sự đang chạy trước. Chọn nhạc tạo cảm giác bị đuổi: tiết tấu dây/trống thấp dồn dập, khoảng tối và tăng áp lực, tránh giai điệu chiến thắng hoặc EDM hiện đại lấn bối cảnh. Không dùng nhạc bản quyền của DOORS/Outlast/Dying Light.
- Một ứng viên miễn phí đã kiểm tra: **Juggernaut — Scott Buckley**, https://www.scottbuckley.com.au/library/juggernaut/ . Trang tác giả mô tả dây, brass và percussion, giấy phép **CC BY 4.0**. Đây là full mix, trang ghi không có alternative mixes. Nghe thử độ hợp với rừng tre/kinh dị trước khi chọn; nếu quá hùng tráng, tìm ứng viên hợp hơn có nguồn và giấy phép rõ. Không mặc định tải xong là đạt.
- Nếu dùng, thêm credit đúng: `'Juggernaut' by Scott Buckley - released under CC-BY 4.0. www.scottbuckley.com.au`; ghi phần chỉnh sửa nếu cắt/loop. Credit phải có ở nơi người chơi tiếp cận, không chỉ file nội bộ.
- Nhịp nhạc có ít nhất ba mức cảm nhận: chạy đầu đã căng → đoạn phối hợp vật cản cao hơn → nước rút/cầu đạt đỉnh, rồi giảm khi thoát tới chùa. Lính tới gần có cue ngắn dựa khoảng cách thực, có smoothing/hysteresis, không bật tắt mỗi frame.
- Ưu tiên track/stem có sẵn hợp lệ. Nếu chỉ có full mix, có thể chọn các đoạn tương thích và nối đúng nhịp/crossfade, hoặc mix mức độ tiết chế; không giả ba stem bằng cách phát ba bản full mix cùng lúc. Không restart nhạc ở mỗi vật cản.
- Giữ tiếng nứt tre, đáp chân và lính nghe rõ trong nhạc. Duck ngắn theo cue quan trọng nếu cần, không pumping theo từng bước. Kiểm tra clipping trên master; không normalize tất cả clip lên cùng peak rồi gọi là cân bằng.
- Loop không click, chuyển đoạn không hụt/nhân đôi phách; stream nhạc dài, chuẩn bị clip trước khi chase thay vì decode/synth nặng đúng lúc chạy. Pause, retry, catch, thoát chương không để nhạc/ambience cũ chồng lên nhau. Settings Music/Effects vẫn hoạt động.

## 6. Tăng mật độ vật cản và quyết định

Giữ đường cố định để học được. Không rải ngẫu nhiên vật chắn bất khả thi hoặc làm tối hơn để tăng độ khó. Mục tiêu đầu tiên: khoảng **22–26 quyết định có ý nghĩa trên MỖI tuyến chạy sạch**, so với khoảng 12/14 mốc hiện nay; đếm nhảy/cúi/lách/chọn hướng thật, không tính mỗi hòn đá trang trí. Đây là mốc tuning ban đầu, điều chỉnh theo điều khiển thực và thời gian quan sát, không ép con số bằng mọi giá.

| Đoạn tương đối trong lượt khoảng 115 giây | Nhịp đề xuất |
|---|---|
| 0–15 s | Vào chạy ngay, vài giây nhận điều khiển; 2–3 thử thách dễ đọc, dạy nhảy và cúi riêng |
| 15–35 s | 4 quyết định, bắt đầu lách trái/phải và một tre đổ báo trước |
| 35–60 s | Khoảng 5 quyết định, hai nhánh đều có nhịp; nhánh hẹp khó thao tác hơn nhưng vẫn nhìn được vật cản |
| 60–90 s | 6–7 quyết định, cụm 2–3 thao tác nối nhau, xen đoạn thở ngắn |
| 90 s–cầu | 4–5 quyết định/nước rút, camera và âm tăng lực; chừa đủ khoảng chuẩn bị đọc cầu/cọc |
| Cầu–thoát | Cú chốt có nhịp riêng, Lam ra khỏi cầu rồi thao tác E; không đặt vật cản bắt buộc che prompt cọc |

- Bảng dùng thời gian tương đối cho dễ hiểu, không spawn theo đồng hồ toàn cục. Đặt theo tuyến/tiến độ, trigger khi người chơi tới; mỗi nhánh phải kiểm riêng. Giữ tổng thời gian quanh bản hiện tại hoặc nhỉnh hơn do thao tác, không giảm tốc giả để kéo dài.
- Đa dạng có lý do: tre gãy thấp để nhảy, tre nghiêng để cúi, cụm rễ/đá lệch để lách, khe cần lấy đà, tre sắp đổ buộc đọc cảnh báo và chọn phía. Có thể tăng từ 2 lên 3–4 sự kiện tre đổ nếu nhịp/hiệu năng cho phép; phân biệt rõ báo trước với cây trang trí.
- Sau phần dạy, một cụm khoảng 2–3 quyết định trong 7–10 giây, sau đó 2–4 giây chạy thoáng. Tránh khoảng dài hơn 8 giây không có thử thách/chuyển nhịp, trừ đoạn chủ ý như đọc ngã rẽ/cầu. Đừng san đều tất cả thành nhảy–cúi máy móc.
- Lần đầu vật cản phải đọc được tối thiểu khoảng 2 giây trước hạn thao tác ở tốc độ thực; tre đổ báo bằng âm + chuyển động trước khi chặn đường. Sương, bụi tre và góc cua không được vô hiệu thời gian báo. Cue nhìn vẫn đủ nếu tắt âm.
- Tính khoảng từ lúc đáp xong và có thể điều khiển tới vật cản tiếp theo, kể cả khi vừa bị slow. Không yêu cầu nhảy giữa lúc không thể đứng lên, cúi ở đỉnh nhảy không né được, khe rộng hơn tầm nhảy, hoặc nhảy vào trần tre. Chuỗi dày vẫn có biên sai số, không đòi input frame-perfect.
- Thua do vấp phải đến từ mất đà để lính bắt kịp thật. Giữ cơ hội phục hồi nếu xử lý tốt tiếp theo; đừng ép lính giữ đúng 5 m ở mọi lúc khiến chơi tốt không có tác dụng. Không cần mọi hai lỗi đều chết; mức phạt phải phụ thuộc vị trí và khoảng cách.
- Lính phải nhảy/cúi/đi vòng các vật mới, không xuyên và không bị kẹt. Cập nhật NavMesh/links, vật lý, gap/catch line checks, đường Lam và builder cùng layout. Không chỉ thêm mesh vào scene làm bot/lính đi xuyên.

## 7. POV: thể hiện sức nặng nhưng vẫn lái chính xác

- Đồng bộ chuyển động bước chạy với `FirstPersonFootsteps.Stepped` hoặc một phase quãng đường chung. Không để bob 1,3 m và âm 1,35 m tiếp tục trôi nhịp. Chuyển động mắt có nhịp, biên độ nhỏ; giảm khi cúi, ngừng bob khi bay và khi control khóa.
- Nhảy: anticipatory motion rất ngắn nếu không làm chậm input; chuyển động lên/xuống theo nhảy thật. Tiếp đất: dip + hồi nhẹ theo vận tốc đáp, dứt sau khoảng 0,15–0,3 giây. Không gán cùng cú giật mạnh cho mọi lần đáp.
- Lách trái/phải: nghiêng thân/camera nhẹ, trần khoảng 1–2 độ để tune, quay lại mềm. Dùng một lớp visual offset, không tích lũy xoay vào hướng điều khiển hoặc đổi collider/đường chạy.
- Vấp/cụng đầu/tre chạm gần: một impulse ngắn có hướng và decay, không kéo camera liên tục khỏi nơi người chơi đang nhìn. Khi nhiều hiệu ứng cùng lúc phải trộn và giới hạn tổng biên độ, không cộng mọi Shake/Dip đến mất đường.
- FOV mặc định hiện 74: giữ làm baseline. Nếu thêm cảm giác tốc độ, dùng đổi nhẹ khoảng 2–4 độ, blend mềm; không zoom theo từng bước hoặc phập phồng theo khoảng cách lính. Không dùng motion blur, chromatic aberration, vignette nặng để thay chuyển động. Không tự xoay nhìn lại lính hay khóa chuột lúc chạy.
- Có thể thêm lá lướt mép khung ở chỗ hẹp nếu đúng tiếp xúc; không che vùng trung tâm/vật cản. Không thêm bàn tay Kiều trong scope này.
- Tận dụng `GameSettings.ShakeAmount` cho mọi chuyển động phụ: nếu đang có lựa chọn giảm rung thì nối vào cùng hệ. Mức tắt phải thực sự tắt bob/roll/impact/FOV phụ, vẫn giữ camera vật lý theo cao độ và cúi/nhảy cần thiết. Không thêm tùy chọn trùng. Camera options không được đổi độ khó, thời điểm input hoặc collision.
- Lưu/khôi phục mọi setting/offset/state khi chết, retry, pause, abort, tới chùa, chuyển Ch6. Không để bob hay FOV Ch5 rò vào chương khác. Khi reset không giật camera từ offset cũ.

## 8. Builder, kiểm chứng và bàn giao

1. Triển khai một cấu hình hoàn chỉnh trên scene thật rồi cập nhật builder đồng bộ. Giữ scene GUID và asset GUID; chạy Apply hai lần xác nhận không nhân đôi vật cản, audio source, event subscription, bank groups hoặc NavMeshLink. Không tự chạy rebuild phá các scene khác.
2. Compile và chạy các ca rừng sẵn có, bổ sung ca cần thiết cho thay đổi thật: cả hai nhánh chạy sạch, các tổ hợp mới khả thi, pause giữa đáp/tre đổ, retry nhiều lần không nhân audio, lính/Lam đi qua tuyến mới, catch gần khe/vật chắn đúng. Không giảm độ nhạy hay cho bot miễn va chạm để ép PASS; không chỉ đổi test kỳ vọng theo code mới.
3. Kiểm tra âm thanh bằng route có Music/Effects bật, tắt riêng từng lớp để tìm lỗi mix, rồi nghe bản mix. Nếu không nghe được, ghi đúng “chưa audition/chưa kiểm bằng tai”; automation mute và waveform không chứng minh âm hay. Không xuất thêm clip/ảnh/A-B file khi chưa được yêu cầu.
4. Kiểm tra camera mức mặc định và tắt rung, chuột trái/phải nhanh, cúi → nhảy → đáp, lỗi một lần còn gỡ được và cố tình vấp liên tiếp. Bot không kết luận được cảm giác POV/độ khó. Nếu chưa có người chơi tay, để rõ phần cần người dùng test, không tuyên bố trải nghiệm đã đạt.
5. Đo cùng route/1080p trong Editor, sau warm-up, ít nhất hai nhánh; tách loading khỏi gameplay. Báo ms/frame và p95/p99, peak voices/audio memory/GC nếu đo được; không quy đổi 19–21 ms thành “đạt 60 FPS”. Không thêm xử lý nặng mỗi frame hoặc hàng chục source ngoài vùng cần nghe. Ưu tiên voice pool và nguồn ở gần, không hạ toàn bộ chất lượng rừng để che chi phí mới.
6. Chạy regression Ch4→Ch5→Ch6 và kiểm tra Ch3/Ch4 cho các lớp dùng chung đã đổi. Không cần full exe build. File exe cũ phải được ghi rõ vẫn cũ.
7. Bàn giao ngắn: những gì thay đổi thực, bảng mốc/vật cản từng tuyến, nguồn/giấy phép âm và nhạc đã dùng, camera profile, kết quả test và hiệu năng so baseline, phần chưa nghe/chơi tay. Cập nhật tài liệu implementation hiện có. Không gọi các nguồn ứng viên là đã import nếu chưa thực hiện.

Thứ tự làm: audit âm đang phát → Foley/mix và nhạc → layout vật cản cùng NavMesh → camera đồng bộ → scene/builder và regression. Hoàn thành bản có thể chơi thử trực tiếp trong Unity; không dừng ở một bản kế hoạch hay hẹn làm thêm một demo riêng.
