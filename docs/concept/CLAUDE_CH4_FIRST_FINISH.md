# Prompt cho Claude — hoàn thiện Chapter 4 trước

## Phạm vi người dùng đã chốt

Hoàn thiện Chapter 4 trước, sau đó mới quay lại Chapter 3. Quyết định cuối cùng vẫn là Ch3 khám phá tự do, Ch4 lén lút và có truy đuổi/bị bắt; **đợt này chưa triển khai việc chuyển Ch3**.

Đọc `AGENTS.md`, kiểm tra Git và bảo toàn thay đổi đang có. Đọc mục 11–12 của `docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md` và `docs/concept/CAUGHT_JUMPSCARE_REFERENCES.md`. Ưu tiên file này khi tài liệu cũ yêu cầu sửa đồng thời hai chương.

## 1. Hoàn thiện cảnh bắt Ch4

- Xem các ảnh ghép/clip hiện có trong `Tools/LauTuBa/Clips`. Ảnh cho thấy mặt và nón NPC rất sáng, biểu cảm trung tính; kiểm tra runtime rồi giảm đèn mặt và điều chỉnh hướng chiếu để mặt đọc rõ mà vẫn có bóng và thể tích. Không thay exposure toàn cảnh.
- Làm rõ lực túm: tay tới đúng vị trí, ngón tay siết ở thời điểm chạm, vai/thân kéo theo rồi camera phản ứng. Giữ model và bộ rig đang có; tránh cảm giác tay chỉ vươn ra trước camera.
- Hoàn thiện trước/sau/chỗ nấp, đứng/cúi và riêng Tú Bà ở Ch4. Giữ các khác biệt hợp nhân vật; kiểm tra clip `TuBa-Yell` trước khi quyết định có dùng, không mặc định nó là animation bắt hoặc biểu cảm mặt.
- Nghe trực tiếp với âm thanh bật để căn tiếng vải, va chạm, hơi thở và cú xô. Có thể tận dụng SFX đang có. Nếu chưa có giọng tiếng Việt, ghi rõ phụ đề chưa thay thế được giọng quát; không tự bịa rằng đã có voice hay dừng các phần độc lập chỉ vì thiếu voice.
- Bổ sung kiểm tra bình phong phòng Lam, rèm và chỗ nấp thấp; không dùng cùng một chuyển động mở tủ cho mọi loại chỗ nấp.
- Kiểm tra khi cảnh bắt bị ngắt/scene đổi: bảo đảm dọn trạng thái, trả camera/input/audio và cờ `CatchScene.Playing`, không chỉ dựa vào việc coroutine chạy hết. Chỉ sửa trong phạm vi cần thiết và kiểm tra hồi quy nơi dùng chung.

## 2. Xác nhận nhịp lén lút và truy đuổi

- Giữ phát hiện → truy đuổi có cơ hội chạy thoát → bắt hợp lệ → jumpscare → checkpoint.
- Kiểm tra tuyến băng sân bằng khoảng cách, góc nhìn, vật cản và độ sáng, gồm đứng/cúi/đi/chạy tại nhiều điểm. Bài thử đứng 70 giây ở một điểm không đủ kết luận cả sân.
- Nếu có tuyến chạy thẳng bỏ qua toàn bộ nguy cơ, ưu tiên chỉnh tuyến/điểm dừng/hướng quan sát của lính Ch4 có chủ đích; không tăng tầm nhìn hoặc độ sáng toàn game. Giữ khoảng an toàn và cơ hội quan sát để người chơi tính đường.
- Bảo toàn chìa khóa, khế ước, nhánh Lam, J1/J2, cửa sau, checkpoint và nội dung truyện hiện tại. Không mở rộng map hoặc thêm nhiệm vụ trong đợt hoàn thiện này.

## 3. Kiểm tra và bàn giao

**Bổ sung của người dùng:** hoàn thiện tiếng bước chân theo vật liệu, tiếng mở/đóng cửa mạnh và phản ứng nghe của lính canh/Tú Bà theo `CLAUDE_CH4_SOUND_STEALTH.md`. Đây là phần trong phạm vi Ch4 hiện tại; tận dụng hệ thống đã có và kiểm tra cùng các ca bên dưới.

- Compile; chạy map, stealth, các ca bắt Ch4 và khôi phục checkpoint. Thử hai NPC cùng bắt, cạnh tường/cửa/bậc, đứng/cúi, từng loại chỗ nấp và Tú Bà.
- Kiểm tra lần bắt đầu tiên trên exe khi quyền build/chạy cho phép; giữ số liệu 54 ms trong baseline thay vì gọi là hoàn toàn không khựng. Dùng profiler xác định nguyên nhân nếu còn hitch, không tối ưu theo phỏng đoán.
- Thực hiện lượt chơi tay có bật tiếng nếu công cụ hỗ trợ. Nếu không thực hiện được, báo rõ; autopilot và GIF câm không xác minh được cảm giác lực hoặc đồng bộ âm thanh.
- Giữ yêu cầu dừng quay clip mới trong phiên hiện tại. Có thể dùng tài nguyên ghi hình hiện có và kiểm tra trực tiếp trên game; không tự bật lại capture.
- Không commit, không sửa scene YAML thủ công, không xóa asset. Tuân thủ điều kiện build và thao tác scene trong `AGENTS.md`.
- Tránh sửa hành vi Ch3 qua code/material dùng chung. Nếu sửa thành phần chung là cần thiết, kiểm tra hồi quy và báo phạm vi; không bắt đầu chuyển Ch3 sang khám phá tự do lúc này.
- Cập nhật mục mới trong tài liệu implementation, nêu đã làm/đã kiểm chứng/chưa kiểm chứng và tình trạng exe so với source hiện tại.

Hoàn thiện Ch4 trong phạm vi trên rồi bàn giao kết quả để đánh giá. Ch3 là công việc tiếp theo, chưa triển khai trong đợt này.
