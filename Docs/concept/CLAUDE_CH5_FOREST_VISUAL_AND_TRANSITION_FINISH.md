# Prompt cho Claude — Ch5 vào thẳng cuộc chạy, rừng tre kín và âm u, dựng lại an toàn

Lượt hoàn thiện tiếp theo về âm thanh, nhạc, vật cản và POV nằm trong [CLAUDE_CH5_AUDIO_MUSIC_OBSTACLES_POV.md](CLAUDE_CH5_AUDIO_MUSIC_OBSTACLES_POV.md). Giữ kết quả hình ảnh/chuyển cảnh của lượt này khi thực hiện lượt tiếp theo.

Người dùng đã chơi Chapter 5 và yêu cầu sửa bốn mục dưới đây, kèm tham khảo rừng của các game đã phát hành. Hãy thực hiện trên scene thật và đường dựng scene tương ứng, không chỉ viết báo cáo hoặc tạo concept. Đây là bước hoàn thiện bản mới đã chạy được, không xây lại gameplay từ đầu.

## 1. Đúng project, đúng phạm vi

- Project: `D:\PRU213\ThuyKieu_Share-20261009T182626Z-1-001\ThuyKieu_Share\ThuyKieuGame_With_Unity`.
- Dấu nhận diện lần kiểm tra 25/10/2026: nhánh `dev`, HEAD `021f57d`; hiện có thay đổi Ch5 chưa commit, không reset về HEAD. Đối chiếu project đang mở trong Editor trước khi sửa vì D:\PRU213 có nhiều bản khác nhau.
- Đọc AGENTS.md, `docs/codex/CHAPTER5_BAMBOO_CHASE_IMPLEMENTATION.md` và source liên quan. Giữ công việc đang làm, không tự commit/push/merge, cài package hoặc quay thêm clip/ảnh. Không sửa scene/prefab YAML bằng tay.
- Giữ chase mới: FPS, auto-forward nhưng tự lái bằng chuột/A-D, Space nhảy, Ctrl cúi, không stamina/QTE/rail; một lính người cầm đuốc; catch theo tầm với và vật cản thật; Lam qua cầu; retry từ đầu rừng; nối Ch6.
- Không làm lại cân bằng chỉ vì sửa hình ảnh. Không giảm tốc chạy để kéo dài chương. Chỉ điều chỉnh đoạn đầu cần thiết để người chơi nhận điều khiển công bằng.

## 2. Tham khảo đã tra cứu: học gì và không nhầm nguồn

Các cách áp dụng bên dưới là đề xuất cho dự án này, không phải khẳng định các game dùng đúng implementation đó.

1. **God of War — A Single Shot**, nguồn PlayStation: https://www.youtube.com/watch?v=l_RpwPcfhB8 . Metadata/mô tả chính thức nói về camera không cắt và không ngắt trải nghiệm; Codex không xem được nội dung video trực tiếp. Học tính liên tục của góc nhìn, không hứa tái tạo hệ streaming của AAA. Áp dụng một chuyển động POV và âm thanh nối tiếp, không thêm cutscene dài trước gameplay.
2. **Alan Wake 2 — Northlight**, nguồn Remedy: https://www.remedygames.com/article/how-northlight-makes-alan-wake-2-shine . Bài mô tả scattering cho môi trường dày, vegetation, màu sắc và fog. Áp dụng nhiều lớp hình khối/lá, bố cục có chủ đích và sương có chiều sâu; không bê ECS/mesh shader/MBOIT/path tracing vào Unity URP.
3. **Alan Wake 2 — Lighting**, portfolio của người trực tiếp làm lighting: https://damians.artstation.com/projects/EzEXVq . Tham khảo tương phản, silhouette và mood; không dùng ảnh portfolio làm bằng chứng project đã đạt chất lượng đó.
4. **Black Myth: Wukong — HeiFengShan**, portfolio của 王琛, tác giả môi trường: https://wangchen-cg.artstation.com/projects/8evG26 . Tác giả mô tả dựng địa hình, vật liệu bề mặt và cây. Tham khảo quan hệ địa hình–cụm tre/cây–đá–đường đi, thay vì một mặt phẳng cắm cọc đều. Không khẳng định mọi ảnh của bài là đúng khu Bamboo Grove.
5. **Blair Witch — Forest**, portfolio của Sebastian Spłuszka, tác giả môi trường game: https://sebastianspuszka.artstation.com/projects/d00xWA . Bài có cây, bụi thấp, địa hình, vật liệu và lighting. Tham khảo lớp bụi rậm che chân rừng, khoảng tối sau thân cây và cảm giác rừng sâu ở góc FPS; không thêm cơ chế siêu nhiên/flashlight bắt buộc.
6. **Ghost of Tsushima — Crafting the world**, nguồn Environment Art Lead của Sucker Punch: https://blog.playstation.com/2020/07/09/crafting-the-world-of-tsushima/ . Tham khảo bố cục thực vật và môi trường dẫn người chơi. Nguồn search tìm được nhưng trang mở trực tiếp bị timeout. Bài `Bamboo Forest - Ghost of Tsushima Fan Art` của John Teodoro là fan art, KHÔNG phải asset chính thức: https://theoj.artstation.com/projects/58m831 . Có thể dùng như breakdown phụ, ghi đúng xuất xứ.

Hướng phối hợp: hình khối và địa hình rừng tre theo tham khảo Wukong/Tsushima, sự kín và bụi rậm theo Blair Witch, mood tối và chiều sâu theo Alan Wake 2. Không copy toàn cảnh game khác hoặc biến tre thành rừng thông. Chỉ xem ảnh/video hợp lệ để tham khảo; không tải asset/texture/nhạc của các game này vào project.

## 3. Bỏ phần chuyển cảnh/mở đầu đang ngắt nhịp Ch4 → Ch5

Yêu cầu mới ưu tiên hơn brief cũ: vào Ch5 là POV Kiều đang chạy trốn ngay. Không xem lại sân bếp, cảnh rộng, cận mặt, thẻ chương chặn màn hình, thoại bấm tiếp hoặc hướng dẫn modal rồi mới được chạy. “Bỏ scene chuyển cảnh” là bỏ phần diễn xuất/UI mở đầu không cần thiết, không xóa file `Chapter05_Escape.unity` hoặc scene Ch4/Ch6.

- Kiểm tra các nguồn gây gián đoạn: `StoryDirector.Start()` hiện gọi `ChapterCard`; `CompleteChapter()` có tổng kết + chờ Continue + fade + LoadScene; Ink Ch5 có `Escape_Night`, `Escape_Alarm`, `Wide_BackYard`, `CU_Kieu`, `Escape_ForestEdge`; `Chapter05World.ForestEdge()` fade/teleport; `tutorial_forest` và khởi động `BambooChaseSequence` còn có chờ/chuẩn bị. Sửa cả luồng, không chỉ ẩn một dòng chữ hoặc tắt camera tag.
- Đề xuất chuyển cảnh cho scope này: đoạn thoát Ch4 đi vào vùng cửa/lối tối che khung hình, nối sang POV Ch5 cùng hướng chuyển động; âm bước chân, thở và tiếng đuổi nối qua điểm chuyển để không cảm thấy bắt đầu một màn phim khác. Có thể dùng cut giấu sau vật che hoặc fade rất ngắn khi dữ liệu đã sẵn sàng; không cần làm một shot streaming toàn game.
- Nếu bỏ summary Ch4 để nối liền, chỉ áp dụng đường chuyển Ch4 → Ch5, vẫn lưu kết quả/Ink/progression và đánh dấu hoàn tất Ch4 đúng một lần. Không bỏ summary và title của mọi chương trong shared StoryDirector.
- Ch5 mở trực tiếp từ Editor/menu cũng phải vào POV ở đầu rừng đang chạy, không quay về sân/ngõ cũ. Hiện thông tin điều khiển bằng chữ nhỏ không chặn input; 4–6 giây đầu là lấy đà, chưa đặt vật cản bắt buộc khó hoặc để lính chạm ngay khi chuột chưa nhận focus.
- Load/generate forest và NavMesh phải hoàn thành trước khi tính chase/catch/input. `LoadSceneAsync` đơn thuần không chứng minh đã hết hitch: script dựng mesh/NavMesh vẫn có thể chặn main thread khi activate. Đo thực tế, chuẩn bị/cache hoặc chia phần dựng phù hợp trước khi lộ khung hình, không kéo màn đen nhiều giây rồi gọi là liền mạch.
- Không spawn lính/Lam hai lần, không giữ fade/camera/cursor cũ, không bật gameplay khi người chơi còn bị khóa điều khiển. Retry không phát lại transition Ch4 hoặc mở đầu.
- Nhánh Sở Khanh đầu rừng hiện mâu thuẫn với “vào là chạy ngay”. Theo ưu tiên mới, không được chặn startup bằng nhánh hội thoại này, không tự chọn hộ một đáp án hoặc tự cộng/trừ bằng chứng/fate. Giữ knot/dữ liệu/save tương thích, tách nhánh ra khỏi luồng bắt đầu cuộc chạy và báo rõ nhánh này tạm không được gọi ở vị trí cũ. Không tự dời vào Ch4 hay bịa cuộc gặp mới sau cầu khi người dùng chưa chốt vị trí thay thế.
- Giữ nguyên các kết quả Ch4 đưa sang (Lam, bằng chứng, tiền, Định Mệnh) và đoạn cuối Ch5 nối chùa/Ch6. Không dùng thay đổi mở đầu làm lý do viết lại toàn cốt truyện.

## 4. Rừng phải kín từ mắt người chơi, kể cả đỉnh nhảy

Phản hồi thực tế: rừng quá thưa; nhảy lên thấy khoảng trống và biên map. Cần sửa hình khối và phạm vi hậu cảnh, không chỉ tăng density, hạ exposure hoặc phủ fog che lỗi.

- Source hiện `BambooGrove` chỉ scatter trong dải `_reach` mặc định 14 m quanh đường; xác suất thưa dần ở xa. Đây là giả thuyết cần kiểm tra cùng culling/bounds/material khi quan sát từ trên bờ và đỉnh nhảy, không coi số thân tre lớn là đã đủ kín.
- Tạo ba lớp: cụm gần có thân/cành chi tiết + bụi thấp; lớp giữa có thân xen kẽ, lá chồng và nền đất/đá; lớp xa là tán và silhouette rừng tiếp nối che chân trời/biên. Mở rộng hậu cảnh ở nơi camera thấy, không tăng collider theo toàn bộ diện tích.
- Dựng tán tre liên tục có khoảng hở nhỏ hợp lý, thay cho nhiều thân cao trơ và các chùm lá rời. Cụm lá hai bên đan qua phía trên đường nhưng không tạo một mái phẳng kín nhân tạo. Kiểm tra vật liệu lá mặt sau, góc nhìn từ dưới và bounds khi gió uốn.
- Đo đúng eye height và chiều cao nhảy của controller; đánh giá đứng, cúi, nhảy ở đỉnh và nhìn sang hai bên. Không sửa bằng cách khóa pitch/yaw, hạ jump hoặc ép người chơi không được nhìn ra ngoài.
- Thêm đất/bờ có cao độ tự nhiên, rễ tre, đá rêu, thân gãy, lá mục và dương xỉ theo cụm. Che mặt bờ trống bằng hình học/thực vật phù hợp, không kéo texture phủ lên địa hình xấu rồi dừng.
- Tránh phân bố đều theo lưới, cùng đường kính, cùng độ nghiêng hoặc cùng chiều cao. Có cụm già/non, thân cong, thân vàng úa và khoảng âm nhỏ; mỗi đoạn có hình khối nhớ được, không lặp 600 m cùng một thiết lập.
- Đường vẫn đủ rộng cho A/D và collider traversal hiện có. Chỉ thân tre/vật cản thật có collision cần thiết; lá mềm không thành bức tường vô hình, bụi không che kín mép rãnh hay cue tre đổ.
- Riêng cầu/khe được phép nhìn ra không gian xa theo bố cục có chủ ý, nhưng vẫn có bờ đối diện, rừng và nền cảnh kết thúc; không lộ mép terrain/đáy thế giới/đường nối chunk.
- Phân biệt thiếu geometry với culling pop-in khi nhảy hoặc quay. Sửa LOD, shadow distance, chunk bounds và foliage shader đúng nguyên nhân; không tắt culling toàn rừng.

## 5. Mỹ thuật rừng tre và ánh sáng âm u

- Chốt mood: rừng đêm ẩm, xanh đen/ô liu trầm, bóng sâu có chi tiết, sương xám lạnh vừa đủ, đuốc hổ phách; không xanh neon, không bầu trời sáng như chạng vạng và không trăng trắng cháy.
- Trăng chỉ là nguồn sáng môi trường bị tán che: giảm diện tích trời hở, dịu moon/sky/ambient và các spot bổ trợ. Không chỉ kéo exposure toàn cảnh xuống khiến chướng ngại chìm đen.
- Kiểm tra `Chapter05Setup.NightLook`, ambient/sky reflection, exposure/tone mapping, `_keyLight` của shot cũ và các `MoonShaft` trong `Chapter05Forest.Props`. Source có 4 spot không bóng, intensity khoảng 2.6–4.2; chúng là điểm cần đo/chỉnh theo scene, không phải cứ nhân toàn bộ intensity với một hệ số là xong.
- Dành ánh sáng cục bộ để đọc gờ cây, mép rãnh, vùng cúi và hướng rẽ. Ưu tiên cạnh/silhouette và tương phản vật liệu; không thêm hàng đèn sân khấu soi đều hết lối đi.
- Đuốc lính phải có tác động ấm lên thân người/tre/đất gần đó và đổi theo khoảng cách thật. Không dùng đuốc ở sau lưng làm nguồn sáng duy nhất của đường phía trước; cũng không thêm flashlight/lồng đèn tay Kiều ngoài brief.
- Vật liệu tre: đốt có hình và sắc độ hợp lý, vệt mưa/bám bẩn, roughness có biến thiên, normal vừa đủ; tránh thân ống nhựa xanh bóng hoặc đường đốt đen lặp đều. Lá phải có silhouette tre, alpha/normal không hiện thành mảng vuông.
- Đất ướt không phải mặt gương đồng nhất: có lớp bùn lì, lá mục, đá/rễ, vũng nhỏ và phản sáng cục bộ. Không thêm SSR package hoặc chuyển render pipeline để làm đẹp nhanh.
- Fog tạo lớp gần/trung/xa; gần camera/vật cản đủ rõ, xa chìm dần. Không phủ fog trắng toàn khung hoặc đặt một tấm fog nhìn thấy mép. Hiệu ứng chỉ dùng khả năng URP hiện có, nguồn tham khảo Northlight không có nghĩa URP mặc định hỗ trợ cùng kỹ thuật.
- Đích chùa ấm và có ý nghĩa dẫn đường ở cuối; không lộ chùa hoặc toàn tuyến qua rừng từ đoạn đầu do bố cục rỗng.
- Chỉ làm profile/material/lighting riêng Ch5; không đổi asset/profile dùng chung khiến Ch3/Ch4 sáng/tối theo.

## 6. Hoàn thiện đường dựng scene và tài nguyên

- `Chapter05Forest.Apply` hiện mở scene rồi RemoveOld/Build/Save; `Chapter05Setup.Create()` qua `NewScene` từ chối nếu scene đã tồn tại; `StoryBuild.Rebuild` vẫn có bước DeleteAsset trước khi dựng lại. Không chỉ ghi khuyến cáo “đừng chạy rebuild” trong báo cáo.
- Cần có đường dựng/cập nhật Ch5 rõ ràng cho scene đã có và project sạch mới: scene GUID, references, build list và linked dialogue phải ổn định. Không xóa scene/.meta hay regenerate GUID để vượt điều kiện tồn tại.
- Chạy hai lần liên tiếp phải idempotent: không nhân đôi rừng, ánh sáng, course, pursuer, Lam, collider, NavMeshLink, sequence, audio hoặc event. Các mesh/material tạo ra cũng cập nhật tại chỗ khi có reference, không DeleteAsset/CreateAsset vô điều kiện gây GUID churn.
- Giữ riêng dữ liệu được builder sở hữu với chỉnh tay cần giữ. Trước khi thay nội dung tạo tự động, xác định chính xác root/phạm vi. Không phá sửa tay sân/ngõ/nhân vật/cửa hoặc nội dung chương khác.
- Với `StoryBuild.Rebuild`, xử lý nhánh Ch5 để không xóa scene/meta và không làm nội dung Ch5 mới mất sau full rebuild. Không tự sửa lại pipeline của mọi chương. Kiểm chứng full pipeline trong bản sao thử có kiểm soát nếu lệnh hiện tại phá asset các chương khác; không chạy thử phá hủy trên bản làm việc duy nhất.
- Runtime mesh/fog/collider/NavMesh phải hoạt động khi load scene mới, reload scene, retry và Editor domain reload. Có cleanup ownership và giải phóng tài nguyên sinh ra; không Clear/Build toàn rừng mỗi frame hay spawn mesh khi validate material.
- Tăng độ kín bằng lớp xa rẻ, chunk/LOD/instancing phù hợp và shadow budget; không gắn hàng nghìn collider/đèn bóng cho tán xa. Đo lại 1080p cùng cấu hình với baseline 15.8–18.2 ms, báo CPU/GPU/frame và spike tải riêng; không gọi đã giữ 60 FPS nếu frame thường vượt 16.7 ms.
- Không đụng việc khóa `_Project`, GUID đã đổi hoặc font asset ngoài phạm vi chỉ để làm Git sạch. Không blanket revert thay đổi người dùng. Nếu Unity sinh thêm thay đổi, nêu rõ.

## 7. Nghiệm thu trên scene thật

1. Ch4 → Ch5 và mở Ch5 trực tiếp: khung gameplay đầu tiên là FPS đang chạy trong rừng; không title card/thoại/cutscene bắt người chơi chờ. Phần nạp hoàn tất trước khi chase/catch chạy; kết quả Ch4 lưu đúng một lần. Đo riêng thời gian che chuyển và hitch còn lại, không tuyên bố “không load” nếu chưa chứng minh.
2. Đánh giá rừng tại điểm xuất phát, hai nhánh, đoạn phối hợp và trước cầu: đứng/cúi/đỉnh nhảy/ngoái hai bên; không lộ map rỗng, biên đất, alpha backface hay culling pop-in. Duyệt ở exposure gameplay thật, không dùng camera beauty shot chỉnh sáng riêng.
3. Chạy thử với mood tối mới: nhận ra cây phải nhảy, tre phải cúi, mép khe và ngã rẽ đủ sớm; không thay đổi hình học gây catch/traversal vô lý. Còn nghe/nhìn được lính và tín hiệu nguy hiểm.
4. Chạy test cũ phù hợp: sạch hai lối, lỗi đơn có thể hồi phục, nhiều lỗi/catch, rơi/retry, pause/focus, Lam/cầu, abort/restore camera, Ink và Ch4 → Ch5 → Ch6. Test Sở Khanh cũ phải ghi rõ không còn là acceptance cho startup mới, không âm thầm pass bằng đường debug khác.
5. Chạy đường dựng Ch5 lần đầu, lần hai và reload; so sánh GUID/ref/component counts và chạy lại gameplay sau rebuild. Kiểm chứng integration với full rebuild bằng cách không phá bản làm việc.
6. Không quay thêm clip/ảnh xuất file. Có thể kiểm tra trực tiếp trong Editor và bằng diagnostic/test cần thiết; báo trung thực phần chưa được người thật nhìn/nghe/chơi tay. Bot pass không chứng minh rừng đẹp, đủ tối hoặc không gây say.

Bàn giao: bản Ch5 mở được để người dùng chơi, file chính và tham số đã đổi, kết quả rebuild/kiểm thử/hiệu năng, thay đổi cốt truyện mở đầu được nêu rõ và giới hạn còn lại. Không build exe hay commit nếu chưa được yêu cầu; nói rõ exe vẫn cũ nếu chưa build. Không bàn giao chỉ một scene concept hoặc một báo cáo hứa hẹn.
