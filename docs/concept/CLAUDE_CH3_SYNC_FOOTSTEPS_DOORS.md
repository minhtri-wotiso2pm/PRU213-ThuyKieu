# Prompt cho Claude — đồng bộ bước chân và tiếng cửa Ch3 với Ch4

## Yêu cầu mới của người dùng — 21/10/2026

Hãy cập nhật **âm thanh bước chân và mở/đóng cửa của Chapter 3 giống bản Chapter 4 hiện tại đã được sửa**. Ch3 vẫn đang dùng bộ âm cũ. Đây là công việc đồng bộ audio riêng, không phải đợt chuyển gameplay Ch3 sang khám phá tự do và không áp dụng cơ chế nghe/truy đuổi mới từ Ch4 cho Ch3.

Đọc `AGENTS.md`, kiểm tra Git và bảo toàn thay đổi đang có. Làm trên scene Ch3 thật và cấu hình sinh scene, không chỉ cập nhật tài liệu.

## 1. Xác minh bản Ch4 đang dùng rồi tái sử dụng

- Kiểm tra routing thực tế từ `StoryAudio` → `FoleyBank` → clip/take/gain trên scene Ch4 hiện tại. Mục 14 implementation là lịch sử tham khảo; nếu Ch4 đã được sửa tiếp, dùng phiên bản thực tế mới nhất làm chuẩn.
- Hiện `LauTuBaSetup.CreateChapter4()` gắn `_foley` từ `Ch4Foley.BankPath`, còn phần tạo Ch3 chưa có cấu hình tương đương. Xác minh đây có còn là nguyên nhân trước khi sửa.
- Tái sử dụng clip và bank/nhóm âm đã có, tránh tải lại hoặc sao chép nguyên bộ WAV sang một thư mục Ch3 khác. Giữ nguyên GUID, CREDITS và nguồn bản thu.
- Nếu bank Ch4 chứa âm riêng như ngáy, sự kiện truyện hoặc jumpscare, chỉ đồng bộ nhóm bước chân/cửa cần thiết, không tự kéo các âm đó sang Ch3. Có thể dùng chung bank nếu nội dung/phạm vi phù hợp; không bắt buộc đổi tên/move asset `Ch4Foley` chỉ để tên trung tính.

## 2. Những gì phải đồng bộ

- Bước chân theo bề mặt: gỗ, gạch/đá, đá ướt, đất/bùn và thảm/chiếu đúng vùng map Ch3. Cùng nguồn, nhóm take, luật không lặp liền và pitch/gain theo dáng như Ch4.
- Giữ khác biệt khom/đi/chạy về nhịp và lực nghe được theo bản Ch4. Chỉ áp dụng phần phát âm; không tự sửa bán kính nhận biết hoặc mức nguy hiểm của NPC Ch3.
- Kiểm tra collider/`SurfaceTag` và lớp phủ thảm/chiếu trong scene Ch3 hiện có, vì scene này có thể chưa được sinh lại sau khi bổ sung các tag ở Ch4. Không chỉ gắn bank rồi để mọi sàn vẫn phát tiếng gỗ.
- Kiểm tra bùn hẻm, sàn bếp, hiên gỗ, bậc đá và thảm. Ngày sau mưa khác đêm đang mưa, nên chọn đúng trạng thái bề mặt thực tế; không làm mọi vật liệu ướt chỉ để tên ID giống Ch4.
- Cửa bản lề và cửa trượt: mở nhẹ, mở mạnh, đóng nhẹ và đóng sập dùng đúng bộ mới tương ứng. Then/chốt/ổ khóa liên quan dùng phiên bản mới nếu Ch4 đã có bản phù hợp; những nguồn Ch4 còn tổng hợp không được tự gọi là đã có bản thu thật.
- Căn thời điểm như Ch4: tiếng va khi cánh chạm khung, tiếng trượt theo chuyển động, không va hai lần hoặc trước hình. Giữ âm tại vị trí cửa và cách suy giảm khoảng cách nhất quán.
- Sử dụng cùng gain/mix làm mốc, nghe đối chiếu với ambience ban ngày Ch3 để tránh bị chìm hoặc quá lớn. Nếu cần gain riêng Ch3, báo rõ và không làm thay đổi cân bằng Ch4.
- Không có bước giả khi đứng yên, ép tường, teleport/checkpoint hoặc khóa điều khiển. Tránh hai nguồn cùng phát một bước khi Ch3 vẫn có bộ phát cũ.

## 3. Cập nhật an toàn

- Cập nhật scene Ch3 đang chơi bằng công cụ Unity phù hợp và sửa cấu hình tạo Ch3 để các lần dựng sau không quay về âm cũ. Không sửa YAML scene thủ công.
- Ưu tiên patch đúng component/bank/tag cần thiết, tránh dựng lại toàn bộ kiến trúc chỉ để thay audio. Nếu dùng quy trình sinh lại scene đã có, kiểm tra nó không ghi đè chỉnh sửa thủ công và chỉ tác động trong phạm vi đã yêu cầu.
- Không thay bản thu hoặc routing đang được chấp nhận ở Ch4; kiểm tra cảnh Ch4 không mất bank/reference sau thay đổi.
- Không sửa cốt truyện, objective, watcher, tầm nghe, AI đuổi/bắt hoặc minigame chìa khóa trong công việc này. Việc chuyển Ch3 sang khám phá tự do vẫn là nhiệm vụ riêng làm sau.
- Không sửa Chapter 1/2/5 trở đi qua bank toàn cục, không đổi package, không xóa asset hoặc commit.

## 4. Kiểm tra và bàn giao

- Compile; kiểm tra scene Ch3 thực sự dùng bộ mới, ghi rõ sound ID và nguồn clip/take thay vì chỉ ghi có audio.
- Thử từng bề mặt và đổi bề mặt khi di chuyển, khom/đi/chạy, dừng/ép tường/checkpoint; thử cửa mở nhẹ/mạnh, đóng sập và cửa trượt.
- Nghe trực tiếp với tiếng bật nếu công cụ hỗ trợ. Nếu chưa nghe được thì báo rõ chưa duyệt mix/chất âm, không gọi test muted là nghe kiểm.
- Chạy kiểm tra tiến trình/scene Ch3 phù hợp và hồi quy routing Ch4; xác minh shared bank không thay luật NPC hoặc làm đổi âm các chương khác.
- Giữ yêu cầu dừng quay clip mới trong phiên hiện tại. Không tự build chỉ để báo hoàn thành; tuân thủ điều kiện/quyền build của `AGENTS.md`. Nếu chưa cập nhật exe thì nói rõ source/scene đã đổi nhưng exe vẫn cũ.
- Cập nhật mục mới trong `docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md`: nhóm âm đồng bộ, tag/routing đã sửa, kiểm thử, giới hạn và tình trạng exe. Phân biệt rõ đã thực hiện/đã kiểm chứng/chưa kiểm chứng.

Hãy triển khai việc đồng bộ audio này, không dừng ở kế hoạch. Việc tạo tài liệu hướng dẫn này chưa thay đổi code hoặc scene.
