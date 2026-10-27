# Prompt cho Claude — Ch5: rừng tre chân thực hơn trên scene thật

Người chơi đã test bản mới và yêu cầu cải thiện đồ họa thực tế hơn. Hãy hoàn thiện toàn bộ rừng Ch5 đang chơi được, trên scene thật và builder tương ứng. Giữ gameplay, âm thanh và POV vừa làm. Không dừng ở một ảnh concept, preset màu hoặc một đoạn demo riêng.

## 1. Xác nhận và giữ kết quả hiện tại

Project: `D:\PRU213\ThuyKieu_Share-20261009T182626Z-1-001\ThuyKieu_Share\ThuyKieuGame_With_Unity`.

Đọc AGENTS.md và §12 của `docs/codex/CHAPTER5_BAMBOO_CHASE_IMPLEMENTATION.md`. Dấu nhận diện kiểm tra ngày 26/10/2026: `dev`, HEAD `021f57d`, nhiều thay đổi chưa commit; xác nhận lại thay vì reset về HEAD. Đúng scene là `Assets/_Game/Scenes/Chapter05_Escape.unity`.

Giữ FPS tự chạy nhưng tự lái, nhảy/cúi thật, 25 vật cản trên mỗi tuyến cùng ngã rẽ/cọc theo report mới nhất, ba tre đổ báo trước, lính người cầm đuốc, catch vật lý, Lam, retry, vào Ch5 chạy ngay và chuyển Ch6. Không thay nhạc, tiếng sỏi người chơi đã duyệt, nhịp camera hoặc độ khó để giải quyết vấn đề đồ họa. Nếu sửa hình học bề mặt có ảnh hưởng collider, phải giữ đường vượt và kiểm lại.

Không tự commit/push, build exe, cài package, mua asset hoặc quay/xuất ảnh/clip. Không sửa YAML scene/prefab bằng tay. Không đổi pipeline/Unity version. Chỉ cập nhật Ch5; material/shader/renderer setting mới phải riêng Ch5 hoặc opt-in, không đổi hình ảnh các chương khác.

## 2. Chẩn đoán dựa trên bản hiện tại

Đây là nhận xét thiết kế dựa khung Play Mode đã thấy và source, không phải đánh giá rằng mọi góc đã được kiểm bằng mắt.

- Đường có dáng rãnh với bờ cao hai bên khá đều. `ForestGround` đặt `_bankHeight = 1.7`, `_bankWidth = 1.3`, còn giới hạn bờ thấp nhất khoảng 1,45 m để người chơi không nhảy ra khỏi tuyến. Dáng này giúp giữ người chơi nhưng làm lộ cấu trúc đường được sinh bằng code.
- `CulmMaterial` dùng texture vẽ code 64×256; lá/canopy/dương xỉ/bụi dùng texture vẽ code 256×256, litter 128×128. `BambooMesh` dùng các card cho lá/canopy/bụi. Card là kỹ thuật hợp lệ, nhưng hình lá giống nhau, card lớn và dáng cây đều sẽ lộ khi nhìn gần, nhìn ngang hoặc nhảy.
- Nền đường và bờ cùng dùng `brown_mud_02`. Trong thư mục texture hiện chỉ thấy diffuse/normal 1K; `C5F_Path.mat` có smoothness hằng 0,36, chưa thấy roughness/AO map riêng. Cần đọc shader thực tế trước khi quyết định bổ sung channel nào, không kết luận shader hỏng chỉ từ tên property.
- `StoryArtKit.Texture()` đang ép texture về 1024. Chỉ đổi setting riêng các asset Ch5 cần cận cảnh; đừng nâng helper dùng chung làm cả game tăng bộ nhớ.
- Mood hiện có moon intensity 0,7, Ch5 exposure 3,3 EV và fog riêng. Các số này không tự chứng minh đúng/sai; đánh giá kết quả cuối cùng dưới grade gameplay. Đừng chỉ hạ exposure hoặc phủ sương để che asset thô.
- Report mới nhất: 861 renderer, 39 đèn tổng cộng, Editor 1080p khoảng 19,9–21,5 ms/khung. Chưa đạt 16,7 ms. Chất lượng cần tăng từ bố cục/asset và quản lý chi phí, không tăng đồng loạt mọi độ phân giải/số cây/đèn.

## 3. Ưu tiên 1 — địa hình và bố cục tự nhiên

- Phá dáng hai bờ song song như mương đào. Chia hình thái theo đoạn: đường men sườn, cụm rễ, bờ đất sạt, đá lộ, khúc qua bụi tre và bờ khe. Bờ có chỗ thấp/thoải về mặt nhìn, chỗ cao, đứt nhịp và chuyển tiếp có lý do, không chỉ tăng noise nhỏ trên cùng mặt cắt.
- Giữ đường chạy, khoảng lấy đà, độ rộng thao tác và tín hiệu vật cản. Chỉ thay nền nhìn thấy hoặc chỉnh hình học có kiểm soát; không làm gồ ghề toàn tuyến khiến CharacterController rung hoặc hụt nhảy.
- Bờ hiện là ranh giới gameplay: khi thay dáng phải thay bằng giới hạn hợp lý như địa hình, đá, rễ/cụm tre đủ đặc và có va chạm khớp hình. Không mở lối thoát map, không đặt tường vô hình trên một khoảng trống trông đi được.
- Tạo cụm có quan hệ: tre mọc theo bụi, thân non quanh bụi, rễ bám bờ, đá chôn một phần xuống đất; tránh vật trang trí nằm nổi hoặc rải đều như hạt tiêu. Bụi thưa/dày và khoảng trống thay đổi theo từng đoạn, vẫn kín khi nhảy/ngó hai bên.
- Mặt đường giữ cách đọc đất lẫn đá nhỏ đã xác nhận. Không biến toàn tuyến thành bùn lún hoặc phủ cỏ/lá để hợp asset mới. Có thể đặt lá/rễ ở lề rừng, nhưng không tự làm âm footsteps đã duyệt sai bề mặt.

## 4. Ưu tiên 2 — tre và thực vật ở gần camera

- Đầu tư nơi mắt thấy rõ: thân/cành gần camera, mép đường, trước vật cản, cửa ngã rẽ và cầu. Vùng xa ưu tiên silhouette và chiều sâu, không cần cùng chất lượng hình học như vùng gần.
- Thân tre có đốt gờ, độ thuôn, độ cong và cành nhỏ nối hợp lý. Biến thể theo bụi, tuổi và điều kiện ẩm; không chỉ random scale/color từng cọc. Vết xước/rêu vừa đủ, không biến tre thành gỗ thớ hoặc phủ xanh mọi thân.
- Lá phải có dáng lá tre thon và cành nhỏ có hướng. Với card gần: atlas rõ, alpha sát silhouette, card uốn/gập có chiều sâu, ít khoảng alpha trống. Dương xỉ ra dáng tàu lá, tránh mảng quạt phẳng to như lá cọ. Không bắt buộc mỗi lá một mesh.
- Kiểm normals, mặt sau và phản ứng ánh sáng; bật two-sided đơn thuần không đủ tạo chất lá. Nếu cần truyền sáng mỏng, làm hiệu ứng tiết chế riêng Ch5, không làm lá phát sáng ban đêm. Tránh nhấp nháy/viền trắng do mip/alpha khi camera chạy nhanh.
- Gió có quan hệ: thân lắc chậm ít, cành/lá rung nhanh hơn, phase theo cụm và độ cao. Gốc bám đất, tre không uốn như cao su; tre gameplay đang đổ dùng đúng sequence, không bị gió visual làm sai vị trí collider.
- Ưu tiên asset có chất lượng nguồn tốt và giấy phép rõ cho thực vật phụ/đá. Chưa xác minh nguồn tre photoreal phù hợp: tìm và kiểm tra thật nếu cần, không gọi texture vẽ code mới là scan/photogrammetry. Nếu không có asset tre hợp lệ, cải thiện mesh/UV/atlas đang có và ghi rõ giới hạn, không báo đã thay bằng scan.

## 5. Ưu tiên 3 — chất liệu có tỉ lệ và phản ứng ánh sáng đúng

- Giữ chất đất/đá của đường, thêm roughness/AO phù hợp bộ PBR đang dùng; kiểm map có khớp albedo/normal và scale thực hay không. Tải đúng map cần, thường 2K cho nơi nhìn gần, không tải cả thư viện hoặc mặc định 8K.
- Đọc `WorldTriplanarLit` và cách `StoryArtKit.Surface` gán property. Nếu shader đã có hỗ trợ, dùng đúng tính năng opt-in/material riêng Ch5. Nếu dùng URP Lit, roughness phải đổi sang smoothness đúng channel; không gắn thẳng ORM/ARM vào slot tùy ý. Mask/roughness/AO là dữ liệu tuyến tính; normal import đúng loại/convention.
- Hai tầng biến thiên: chi tiết nhỏ đúng tỉ lệ và mảng lớn đổi sắc/độ ẩm để giảm lộ tile. Không khử tile bằng cách trộn quá nhiều texture full-res cho mọi pixel, hoặc làm nhòe toàn bộ nền.
- Mặt ẩm có vùng đục và vùng bóng khác nhau. Đá, đất, thân tre, ván gỗ phản ứng khác nhau; không phủ cùng một lớp nhựa bóng. Vũng nước chỉ ở vùng địa hình hợp lý, mép mềm và không nổi khỏi đất.
- Rễ/đá/gốc tre cần cảm giác chạm đất nhờ tiếp xúc, AO vừa phải và chuyển vật liệu hợp lý; tránh chân đen thành vòng dán. Không dùng displacement/parallax nặng toàn màn; hình khối nhìn ngang quan trọng phải đến từ mesh.
- Giữ những texture đã chấp nhận và GUID; asset thay thế phải được builder tham chiếu thật. Không chỉ tạo file đẹp rồi scene vẫn dùng bản cũ, cũng không để builder lần sau vẽ texture cũ đè lên asset mới.

## 6. Ưu tiên 4 — ánh sáng đêm có chiều sâu

- Tổ chức gần/trung/xa: thân tre gần có khối và texture vừa đủ, lớp giữa có silhouette rõ, xa chìm vào sương xám lạnh. Sương có vùng và cao độ hợp lý, không phủ lớp trắng đồng đều hoặc thành plane chữ nhật nhìn thấy cạnh.
- Trăng tạo các mảng qua tán có hướng thống nhất. Đuốc là nguồn ấm chuyển động phía sau, ánh chạm vật gần phù hợp khoảng cách. Không làm cả rừng xanh điện, lá xanh rực hoặc các điểm gợi ý lộ như đèn sân khấu.
- Kiểm ánh sáng phụ hiện có quanh vật cản. Giữ tín hiệu để đọc đường nhưng blend với nguồn sáng môi trường, ưu tiên silhouette/giá trị sáng tối thay vì thêm đèn vào từng bụi.
- Không dùng quá nhiều bloom, sharpen, vignette, film grain hoặc blur. Đánh giá khi chạy, cúi và đáp, không chỉ một góc đứng yên. Mục tiêu đêm âm u nhưng đất/đá, thân tre và khoảng cách vẫn đọc được.
- Chỉ chỉnh profile/renderer riêng Ch5 khi cần. Không sửa SSAO/shadow asset dùng chung làm Ch3/Ch4 thay đổi. Không hứa sao chép ray tracing/volumetric engine của game tham khảo vào URP.

## 7. Nguồn tham khảo và asset đã kiểm tra trang nguồn

Các cách áp dụng ở đây là đề xuất cho dự án, không khẳng định dùng đúng kỹ thuật của game tham khảo.

- Remedy — How Northlight makes Alan Wake 2 shine: https://www.remedygames.com/article/how-northlight-makes-alan-wake-2-shine . Tham khảo cách tổ chức vegetation, lớp rừng và tương tác ánh sáng. Không đưa ECS/mesh shader/MBOIT của Northlight thành yêu cầu Unity.
- Poly Haven — Brown Mud 02: https://polyhaven.com/a/brown_mud_02 . Nguồn bộ nền đang dùng, có thể bổ sung map thiếu sau khi đối chiếu shader.
- Poly Haven — Fern 02: https://polyhaven.com/a/fern_02 . Trang mô tả model dương xỉ có nhiều cụm, map Alpha/AO/Normal/Roughness; tác giả Rico Cilliers/Rob Tuytel. Ứng viên thay dương xỉ vẽ code ở gần, không rải bản đầy đủ khắp 600 m.
- Poly Haven — Rock Moss Set 01: https://polyhaven.com/a/rock_moss_set_01 . Bộ đá có silhouette khác nhau, tác giả Kless Gyzen. Chỉ lấy phần cần, kiểm kích thước và giảm mesh/LOD cho realtime; không instancing nguyên cả bộ nặng tại mỗi cụm.
- License Poly Haven: https://polyhaven.com/license . Các asset nêu trên được trang nguồn ghi CC0. Kiểm lại phiên bản/file lúc tải và ghi nguồn trong credits. Không lấy ảnh preview làm texture; tải đúng asset và map. Không cần mua/cài add-on để sử dụng tài nguyên tải trực tiếp.
- Unity URP Lit: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@17.0/manual/lit-shader.html . Dùng để đối chiếu material; phiên bản thực tế của project và shader source là căn cứ triển khai.

Không dùng ảnh AI làm bằng chứng Unity đã đạt, không gọi ảnh game tham khảo là ảnh của project. Không cần xuất concept hoặc ảnh mới trong lượt này.

## 8. Hiệu năng và kiểm chứng bắt buộc

- Thiết lập LOD/culling có đo cho thực vật/đá, ưu tiên hình gần. Chuyển LOD phải hỗ trợ bởi shader thực, chuyển ở nơi khó nhận thấy; kiểm khi nhảy và lia chuột, không cull mất lớp rừng che chân trời. Không cam kết không pop-in chỉ vì chưa dùng LOD.
- Kiểm overdraw alpha, số material, shadow caster và instance; không gom toàn bộ rừng thành một mesh khiến culling mất hiệu quả. Giữ collider gameplay đủ đơn giản, không dùng mesh scan chi tiết làm collider cho mọi lá/đá.
- Giảm chi phí ở lớp xa và hiệu ứng để dành cho asset gần; giữ chất lượng đường/vật cản nơi phải phản ứng. Với mục tiêu 60 FPS, hướng tới dưới 16,7 ms, nhưng báo số đo thực. Nếu vẫn khoảng 20 ms phải ghi chưa đạt, không dùng average FPS cao của menu hoặc loading để kết luận.
- So trước/sau cùng góc, độ phân giải, quality và điều kiện máy; đo sau warm-up, tách loading. Kiểm cả hai tuyến tại đầu rừng, trước nhảy/cúi, ngã rẽ, tre đổ, cầu và nhìn ngang/ngửa từ đỉnh nhảy. Nếu không có công cụ nhìn Play Mode thì nói rõ, số pixel tối/độ kín không thay đánh giá thẩm mỹ.
- Run Apply hai lần: scene GUID/material refs giữ, không nhân đôi mesh/collider/đèn/asset, không mất vật liệu mới. Builder phải tái tạo được kết quả trên scene thật.
- Compile và chạy kiểm thử rừng hiện có: hai tuyến sạch, vấp còn gỡ được, bắt vật lý, retry/pause, Lam/cầu và chuỗi Ch4→Ch5→Ch6. Thay terrain không được làm hỏng NavMesh/gap/catch hoặc bề mặt footsteps. Không giảm độ khó hoặc sửa kỳ vọng test để bỏ qua lỗi địa hình mới.
- Bàn giao: những gì thật sự thay, asset đã dùng và giấy phép, nơi còn procedural, kết quả visual/runtime đã xem, hiệu năng và phần chưa kiểm. Không tự build exe, commit hoặc xuất ảnh/clip. Để scene sẵn sàng cho người dùng tự chơi thử khi họ yêu cầu mở game.

Thứ tự thực hiện: địa hình/bố cục → tre và lá gần → vật liệu PBR → ánh sáng/sương → LOD/chi phí → regression và builder. Hoàn thành một lượt art trên toàn tuyến, giữ tinh thần rừng tre Việt Nam ban đêm và cơ chế truy đuổi hiện có.
