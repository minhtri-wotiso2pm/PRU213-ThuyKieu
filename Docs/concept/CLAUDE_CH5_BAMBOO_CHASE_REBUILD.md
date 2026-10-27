# Prompt cho Claude — xây lại Chapter 5: Đào thoát trong rừng tre

> Cập nhật sau lượt chơi ngày 25/10/2026: đợt hoàn thiện tiếp theo dùng [CLAUDE_CH5_FOREST_VISUAL_AND_TRANSITION_FINISH.md](CLAUDE_CH5_FOREST_VISUAL_AND_TRANSITION_FINISH.md). Ưu tiên yêu cầu mới: vào Ch5 là POV đang chạy, rừng kín khi nhảy, ánh sáng âm u và đường dựng scene ổn định. Không làm lại các phần gameplay đã hoàn thành theo brief cũ.

Hãy thực hiện thiết kế dưới đây trong Unity, từ đoạn kiểm chứng cảm giác chạy tới bản Chapter 5 hoàn chỉnh. Không dừng ở phân tích, ảnh concept hoặc một demo chưa nối vào game. Giữ các thay đổi đang có; không tự commit/push/merge, không quay thêm clip.

## 1. Xác nhận đúng project trước khi chỉnh

Project đã nghiên cứu:
`D:\PRU213\ThuyKieu_Share-20261009T182626Z-1-001\ThuyKieu_Share\ThuyKieuGame_With_Unity`

Ngày 25/10/2026, Codex xác nhận project này có:
- Git root đúng đường dẫn trên, nhánh `dev`, HEAD `021f57d` (merge PR #3 từ ThaiDev), working tree sạch.
- Remote `github`: `https://github.com/minhtri-wotiso2pm/PRU213-ThuyKieu.git`; `origin` là bundle local, không phải remote GitHub.
- Unity `6000.6.0f1`, scene Chapter 5 do `Chapter05Setup` dựng tên `Chapter05_Escape`.

Đây là dấu nhận diện, không phải yêu cầu reset về commit cũ. Tự kiểm tra `git rev-parse --show-toplevel`, status, branch, log, ProjectVersion và project Unity Editor đang mở ngay trước khi sửa. `D:\PRU213` có nhiều bản cùng tên; không chọn nhầm `D:\PRU213\ThuyKieuGame_With_Unity`, `My project` hoặc `TuBa_House_FirstPerson`. Nếu đang làm ở bản khác mà không xác minh được đó là bản người dùng muốn, dừng phần sửa phụ thuộc và báo đường dẫn cụ thể. Không tự chuyển/reset/ghi đè bản nào để làm dấu nhận diện khớp.

Đọc AGENTS.md và hướng dẫn module áp dụng. Ưu tiên source và công cụ Unity; không sửa tay YAML scene/prefab. Việc người dùng yêu cầu xây lại Ch5 cho phép chỉnh nội dung scene Ch5 và builder tương ứng; giữ GUID/liên kết, không xóa/recreate scene hay sửa hàng loạt asset ngoài phạm vi. Không cài package/đổi phiên bản Unity.

## 2. Hướng chơi đã chốt và tài liệu tham khảo

- Rừng tre rậm rạp ban đêm, góc nhìn thứ nhất, chạy thoát qua các chướng ngại vật và ngã rẽ. Học nhịp Seek chase trong DOORS: đọc đường nhanh, tự lái, phối hợp né/cúi/vượt, có kẻ đuổi ngay sau lưng.
- Vai trò kẻ đuổi là một lính canh của Tú Bà cầm đuốc, con người có giới hạn đường đi và chuyển động. Không thêm Seek, quái vật, xúc tu, dịch chuyển hay năng lực siêu nhiên cho lính.
- Video người dùng cung cấp: https://www.youtube.com/watch?v=SJ44-Z1ZXTQ và https://www.youtube.com/watch?v=KSiY3JuSte8 . Codex chưa truy cập được nội dung hai video (fetch lỗi/429); không coi timestamp, thao tác hoặc AI của video là đã được xác minh. Nếu xem được, ghi điều đã quan sát và chuyển thành yêu cầu phù hợp; nếu không, triển khai theo brief, không bịa rằng đã xem.
- Concept mỹ thuật: `docs/concept/chapter5-bamboo-chase-concept.png`. Đây là ảnh AI định hướng, không phải screenshot Unity. Vị trí lính ở sau trái và góc ngoái đầu trong ảnh không bắt buộc luôn xuất hiện trong camera gameplay.
- Chỉ tham khảo nhịp chơi; dùng nhân vật, map, vật liệu, âm thanh của dự án hoặc asset có nguồn hợp lệ. Không đưa asset/nhạc/nhân vật DOORS vào project.

## 3. Phạm vi code hiện có cần đối chiếu

Đọc tối thiểu các file sau và dependency trực tiếp cần thiết:
- `Assets/_Game/Data/Dialogue/Ink/Chapter5_DaoThoat.ink`
- `Assets/_Game/Data/Dialogue/Ink/Main.ink`
- `Assets/_Game/Scripts/Gameplay/Chapters/Chapter05World.cs`
- `Assets/_Game/Scripts/Gameplay/Sequences/ForestRunSequence.cs`
- `Assets/_Game/Scripts/Gameplay/Sequences/ChaseSequence.cs`
- `Assets/_Game/Scripts/Gameplay/Chase/ChaseObstacle.cs`
- `Assets/_Game/Scripts/Core/Editor/Chapter05Setup.cs`
- `PlayerMovement`, `InputReader`, camera góc nhìn thứ nhất, `StoryDirector`, follower/StoryActor, catch và checkpoint hiện có.

Hiện `ForestRunSequence` khóa free control, dùng `ScriptedMove` bám rail, tracking camera và QTE có giảm Time.timeScale. Nó bắt khi biến gap thấp chứ không cần kẻ đuổi chạm thật. Đó là phần cần thay. `ChaseObstacle` hiện thiên về xô đồ đổ, chưa phải hệ nhảy/cúi vật cản; `PlayerMovement` có crouch/gravity nhưng chưa có thao tác nhảy thực sự trong code đã đọc. Không chỉ đổi nhãn QTE hoặc chuyển camera rồi báo đã có gameplay mới.

## 4. Điều khiển và camera

- Khi chase rừng bắt đầu, tự chạy tiến ở tốc độ cấu hình: hiểu là giữ ý định chạy tiến thay người chơi, không tự tìm đường hay tự qua ngã rẽ. A/D strafe hoặc chỉnh vị trí; chuột nhìn/xoay để tự chọn hướng chạy. Không khóa vào ba làn, không kéo Kiều về spline và không tự nhảy/cúi.
- W không cần giữ, không tăng tốc cộng dồn; Shift không tạo lớp sprint/thể lực thứ hai. Ngoài chase dùng điều khiển bình thường. Không có thanh thể lực hoặc cơ chế chờ hồi thể lực trong chase Ch5.
- Space nhảy thật/đi qua vật thấp theo vùng hợp lệ; Ctrl giữ cúi để đi dưới vật cản thấp; E chỉ cho tương tác đặc biệt có mục tiêu trong tầm. Không bắt bấm phím ngẫu nhiên theo overlay, không mash để giả vượt vật cản.
- Dùng Input System/InputReader hiện có, không mở kiến trúc input mới. Cấu hình chạy/nhảy chỉ opt-in cho Ch5 hoặc mode mới, không làm các chương khác tự chạy hay nhảy ngoài ý muốn.
- CharacterController/collision quyết định nhân vật qua được hay không: xử lý grounded, gia tốc, va trần, khoảng rơi, slope, stepOffset và khoảng hở đứng dậy. Không để controller tự bước qua cây/rãnh đáng lẽ phải nhảy.
- FPS có chuột điều khiển thật; không tracking camera ngoài thân lúc chase. Tận dụng hệ camera hiện có, xử lý tóc/đầu che mắt, headroom dưới tre, bob/shake nhỏ. Không bắt buộc có tay Kiều hay tải rig tay mới chỉ vì concept có tay.
- Auto-forward theo quy tắc hướng chạy rõ ràng. Không vừa xoay camera độc lập vừa âm thầm tự lái theo rail. Nếu thêm nhìn ngoái độc lập, phải có input/mode rõ và không đảo hướng di chuyển đột ngột; không cần tính năng này để hoàn thành scope cơ bản.
- Lưu/khôi phục FOV, cursor, input locks, first-person, CanRun, crouch, ScriptedMove, Time.timeScale và thể lực cũ ở mọi nhánh finish/fail/abort/scene unload. Pause/focus loss không khiến game tiếp tục chạy, bắt hoặc ăn phím nhảy khi resume.

## 5. Thiết kế tuyến và nhịp độ

Đích thử ban đầu: chase chính khoảng 2–3 phút, cả Ch5 khoảng 4–5 phút ở lượt thuận lợi; đo thời gian gameplay và thoại riêng. Không kéo độ dài bằng đi bộ chờ, hành lang lặp hoặc thêm hội thoại.

Tuyến thiết kế cố định, bố trí có chủ ý và có thể học lại; không random ngã rẽ/vật cản trong bản đầu. Tuyến có thể uốn và chia nhánh ngắn rồi nhập, không phải phải tạo một bản đồ rừng mở khổng lồ.

1. Mở đầu ngắn: chuyển từ lối thoát Ch4 sang bìa rừng, có cảnh báo lính đuổi và khoảng chạy lấy đà. Không lặp thêm một chương lén lút dài ở sân bếp.
2. Học thao tác: lần đầu nhảy qua cây thấp, lần đầu cúi dưới tre nghiêng, mỗi thứ có khoảng chuẩn bị riêng.
3. Chọn hướng: ngã rẽ nhìn được trước khi cam kết, các nhánh có hình học/ánh sáng phân biệt. Không bắt chọn ngẫu nhiên ở góc khuất.
4. Phối hợp: cây đổ, lối hẹp, rãnh và tre thấp xen kẽ, có đoạn lấy lại nhịp. Không ép nhảy rồi cúi trong khoảng thời gian con người không phản ứng kịp.
5. Nước rút và cầu: nhìn thấy đích, vượt cầu và thực hiện thao tác chặn đường có nguyên nhân vật lý. Cầu/đường bị chặn thật khiến lính không theo kịp; không tắt NPC ngay trước mặt hoặc kết thúc do bộ đếm thời gian đủ lâu.
6. Hạ nhịp: chùa trong mưa, nối Ch6 và câu chuyện hiện có.

- Va cây/vật cản thường làm khựng/chậm có giới hạn rồi lấy lại đà; không chết ngay mọi va chạm nhỏ. Rơi sâu có thể chết, nhưng mép/rãnh và khoảng nhảy phải nhìn được, đi thử được.
- Tre đổ có tiếng rạn, chuyển động và dấu hiệu báo trước đủ để phản ứng; nguồn là gió/mưa/cấu trúc yếu, không phải lính điều khiển rừng. Không tạo hitbox tức thì trước khi vật thể tới.
- Hình ảnh rừng dày ở hai bên và phía trên, hành lang chạy rõ và đủ bề ngang cho strafe. Lá/cành mềm không có collider cứng vụn mắc người; thân tre, cây đổ và chướng ngại có collider khớp hình.
- Ước lượng chiều dài theo tốc độ/nhịp thực, rồi đo trong game. Không lấy chiều dài spline chia tốc độ làm bằng chứng thời lượng vì còn nhảy/cúi/rẽ/va.

## 6. Lính đuổi và bị bắt

- Một lính chính hiện diện liên tục, chạy theo đường người thật có thể đi; dùng rig/model lính hợp bối cảnh đã có và đuốc thật trên người. Tận dụng animation phù hợp trước khi đòi download.
- Kiều vượt tốt tạo khoảng cách thực; va/chậm làm lính áp sát. Khoảng cách quyết định bởi vị trí/đường đi thật, không biến gap ảo tự ép chết. Giới hạn chỉnh tốc độ nếu dùng để cân bằng; không teleport hoặc bù đường xuyên tường khi tụt xa.
- Lính cúi/vượt theo animation và đường traversal định trước hoặc đi vòng hợp lệ quanh vật cản. NavMeshLink/traversal cần có thời gian, collision và hình ảnh phù hợp; không trượt xuyên cây, bay qua rãnh hay chạy dưới đất. Xử lý hình học thay đổi khi tre/cầu đổ và lính bị chặn.
- Không cài trường hợp chắc chắn bắt dù chạy đúng. Không chỉnh AI để bot test thắng; phải đo đường chạy chuẩn, lỗi đơn lẻ có thể hồi phục và nhiều lỗi bị bắt hợp lý.
- Catch chỉ khi thực sự tới tầm với, có đường nhìn/không vật chắn, chênh cao hợp lệ. Lính đứng bên kia rãnh/cầu/tường không bắt qua được. Một lần bắt chỉ một catch/fail, không grab từ xa và không teleport Kiều để che sai khoảng cách.
- Tái sử dụng catch phù hợp nếu tách được dependency Ch4; không kéo toàn bộ stealth world/safe room Ch4 vào Ch5. Nếu rig/pose không hỗ trợ, báo đúng hạn chế và giữ catch đọc được, không dùng flash thay cho mọi logic.
- Dùng tiếng chân, hơi thở, lời quát và ánh đuốc để báo gần/xa; cue dựa theo vị trí thật, không fake tiếng sát gáy khi lính mắc cách xa.

## 7. Cốt truyện, Lam và retry — mặc định bảo thủ cho ý chưa chốt

Không tự coi các ý sau là đã được duyệt riêng:
- Chữ vàng “Trang sách” làm Guiding Light: mặc định chưa bật hiệu ứng dẫn đường siêu nhiên. Làm đường đọc được bằng địa hình, độ tương phản, khe trời và điểm sáng trước. Có thể để extension/config nhưng không dùng nó để chữa một map không đọc được.
- Sở Khanh: hiện nhánh có điều kiện `dinh_menh >= 50`, diễn ra trước chase rừng. Giữ nhánh/điều kiện/hệ quả hiện có ở phần trước cuộc chạy rừng liên tục, không tự đổi toàn bộ lịch gặp hoặc xóa nhánh vì làm camera mới. Không mở hội thoại chọn đáp án giữa lúc lính vẫn đang bắt.
- Cô Lam: giữ `c4_ru_co_lam` và kết quả `c5_co_lam_thoat` nối Ch6. Chưa chốt cảnh cứu mới trước nước rút nên không tự thêm màn mash/QTE hoặc ép Lam bị bắt. Mặc định nếu đi cùng, bố trí traversal companion và cho chị thoát khi theo tới đích; không để chết vì follower kẹt path. Một cảnh cứu bằng E chỉ đưa vào khi được chốt thêm hoặc giữ được ý nghĩa tương tác cũ mà không làm sai scope, và phải báo rõ lựa chọn thiết kế.
- Kết chùa trong mưa, khế ước, ý định quay lại cứu người và tìm Kim Trọng đã có trong Ink: bảo toàn mạch và nối Ch6; không tự viết lại thành chuyện siêu nhiên.

- Hiểu “bị bắt/chết quay lại từ đầu” là từ đầu lượt chase rừng, không checkpoint giữa cuộc chase. Không bắt xem lại toàn bộ thoại/chọn Sở Khanh hoặc chơi lại Ch4. Lưu trạng thái đầu lượt và đặt lại player, lính, Lam, vật cản/cầu/tre đổ, trigger và âm lặp.
- Không hoàn tác mất bằng chứng do lựa chọn trước chase, không nhân đôi reward/fate/soul/trang sách qua retry. Đếm `c5_bi_bat` đúng một lần mỗi lần thất bại; reset không cộng thêm thất bại hoặc phát callback completion hai lần.
- Các biến `c5_qte`, `c5_duong`, `c5_xo_do`, `c5_lang_le` đang có lời tổng kết cụ thể. Rà toàn bộ chỗ đọc trước khi thay; giữ tương thích save cần thiết, cập nhật summary/tutorial/Ink cho kết quả thật của gameplay mới. Không tiếp tục báo “QTE 3/3” hay “qua chợ” nếu không có hành vi đó.
- Phần sân/ngõ hiện có có thể giữ ngắn làm nối tuyến; tránh hai cuộc chase dài trùng nhau và không để stamina/chế độ camera cũ bật lại giữa Ch5. Đọc `Main.ink`/StoryDirector để sửa nguồn dialogue thật và build asset thoại đúng pipeline hiện có, không giả sử chỉ sửa .ink là runtime đã đổi.

## 8. Mỹ thuật và âm thanh

- Bám concept: nhiều lớp tre thân cong/khác kích thước, rễ, bờ đất, đá rêu, lá thấp, mặt đất ướt và vũng nhỏ. Bố cục có điểm nhớ, không chỉ clone thân trụ xanh và sàn phẳng.
- Ánh trăng lạnh vừa đủ đọc đường, ánh đuốc lính ấm, chùa xa là đích. Fog cho chiều sâu nhưng không che vật cản đúng lúc cần phản ứng; không dùng tối đen để thay cho tạo cảnh.
- Vật liệu gỗ/tre/đất/đá đúng tỷ lệ, roughness/normal và độ ướt hợp lý. Ưu tiên mesh/silhouette tốt trước khi tăng post-processing. Không hứa đạt hình concept chỉ nhờ shader hoặc mặc định URP có SSR.
- Foley chân trên bùn/gỗ cầu/đá, tiếng nhảy/tiếp đất/cúi quệt lá, tre rạn và đuốc. Tận dụng bản thu đã duyệt nếu phù hợp, không biến toàn bộ thành tiếng tổng hợp mới; âm có không gian và đồng bộ vận động. Không tải nhạc/âm từ DOORS.
- Dùng khả năng URP hiện có, instancing/LOD hoặc culling hợp lý, foliage và shadow budget có kiểm soát. Đích thử 1080p/60 FPS trên máy hiện tại; báo cấu hình và số đo thật, không lấy bot 640×480 làm số liệu 1080p hoặc cam kết FPS khi chưa đo.

## 9. Trình tự thực hiện và nghiệm thu

1. Xác nhận project, đọc source liên quan, ghi ngắn integration cần đổi. Không sửa Ch3/Ch4 hoặc nhánh dev/GitHub ngoài yêu cầu.
2. Dựng đoạn 90–120 giây để kiểm chứng chạy FPS, cua rẽ, nhảy/cúi, collider và lính traversal/catch/retry. Đây là mốc nội bộ; tiếp tục tới chương hoàn chỉnh 2–3 phút chase khi các vấn đề kỹ thuật đã giải quyết, không dừng rồi chỉ bàn giao demo.
3. Hoàn thiện tuyến, cảnh, âm và nối storyline/Ch6 trên scene Ch5 thật. Builder/scene phải nhất quán để chạy setup không mất nội dung; giữ scene GUID và liên kết build.
4. Recompile và kiểm thử liên quan bằng công cụ Unity đã có. Test phải điều khiển qua luồng input/movement mới, không kéo vị trí dọc rail, không tăng jump/catch distance riêng cho QA hay bỏ collision để pass.
5. Cần kiểm: chạy đúng tới đích; từng vật cản; cúi đủ thấp/đứng dậy dưới tre; nhảy va trần/rơi; đi sai ngã và hồi phục; lỗi đơn lẻ/nhiều lỗi; tre đổ báo trước; lính vượt/đi vòng thật; catch bị cửa/cây/rãnh chặn; fail/retry liên tiếp; pause/abort; finish chỉ một lần.
6. Kiểm solo và Lam có/không; các nhánh Sở Khanh và bằng chứng trước chase; save/load giữa các giai đoạn; retry không lặp reward. Ch4 → Ch5 → Ch6 đi đúng nhánh. Kiểm Ch3/Ch4 không bị thay đổi input/camera/collision do code dùng chung.
7. Báo riêng thời gian chase/thoại/cả chương, hiệu năng và các phần chưa nghe/chưa chơi tay/chưa nhìn animation thực tế. Bot pass không chứng minh cảm giác chạy, độ rõ đường và độ căng thẳng đã tốt.

Không tự commit/push/merge, xóa asset ngoài phạm vi, cài package, quay clip hay build player chỉ vì sửa source. Nếu chỉ xác minh Editor thì ghi exe cũ chưa chứa thay đổi. Bàn giao file chính, cách mở/chạy scene, kết quả kiểm thực tế và hạn chế còn lại; không gọi hoàn tất khi đường chơi/scene mới chưa nối vào game.
