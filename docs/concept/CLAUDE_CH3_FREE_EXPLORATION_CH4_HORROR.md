# Prompt cho Claude — Ch3 khám phá tự do, Ch4 lén lút và bị bắt

**Thứ tự mới do người dùng chốt, 19/10/2026:** hoàn thiện Chapter 4 trước theo `CLAUDE_CH4_FIRST_FINISH.md`. Tạm hoãn toàn bộ thay đổi gameplay Chapter 3 trong tài liệu này tới đợt sau. Quyết định Ch3 sẽ khám phá tự do vẫn giữ nguyên, nhưng chưa triển khai song song với đợt Ch4.

## Quyết định mới của người dùng — 19/10/2026

Trong hai chương Lầu Tú Bà, **chỉ Chapter 4 có phát hiện, truy đuổi, bắt giữ và jumpscare**. **Chapter 3 cho người chơi tự do khám phá map, không bị NPC bắt, phạt, kéo về chỗ cũ hoặc ép chơi lại.** Yêu cầu này ưu tiên hơn mọi prompt/tài liệu cũ đã cho phép bắt ở Ch3. Phạm vi không yêu cầu thay đổi Ch5 trở đi.

Hãy thực hiện thay đổi trong project thật, đọc `AGENTS.md` và kiểm tra Git trước. Giữ công việc đang có, hoàn thiện luồng truyện Ch3 sau khi bỏ nguy cơ bị bắt, kiểm tra hồi quy Ch4. Không chỉ tắt animation jumpscare rồi giữ nguyên thất bại thường ở Ch3.

## 1. Chapter 3 — khám phá và phát hiện sự thật

- Người chơi có thể đi, chạy, nhìn đồ vật, trò chuyện và đọc manh mối trên map mà không bị NPC phát hiện theo cơ chế stealth, truy đuổi hoặc bắt giữ.
- NPC vẫn hiện diện, đi lại và sinh hoạt để giữ bối cảnh, nhưng không tăng nghi ngờ nhằm phạt người chơi, tìm chỗ nấp, bắt, báo thua, teleport về hiên hoặc khôi phục checkpoint vì đi vào vùng cấm.
- Việc khom/nấp nếu giữ lại chỉ là tương tác tùy chọn, không phải điều kiện bắt buộc để hoàn thành Ch3. Không hiển thị HUD nguy hiểm/độ nghi hoặc hướng dẫn làm người chơi hiểu phải tránh bị bắt trong chương này.
- Vẫn giữ mạch tìm hiểu thân phận, văn tự bán thân, cuộc trao đổi Tú Bà–Mã Giám Sinh, thông tin phục vụ Ch4 và kết chương. Không thay các sự kiện chính hoặc viết lại toàn cốt truyện.
- Cửa có thể mở theo tiến độ truyện và ranh giới map vẫn hợp lý. Không dùng NPC trừng phạt để ngăn khám phá. Loại bỏ hạn chế khu phía sau nếu nó chỉ tồn tại để tạo thử thách né lính.

## 2. Cảnh nghe lén phải đổi cùng cơ chế

Code hiện tại trong `Chapter03LauWorld.VoicesApproach()` có lời thúc trốn, bật `_tuBaEyes`, thất bại nếu awareness đạt ngưỡng và chỉ tiếp tục khi Kiều `IsHidden` trong văn phòng. Đây là mâu thuẫn trực tiếp với yêu cầu mới.

- Giữ cuộc nói chuyện quan trọng nhưng chuyển nó thành sự kiện khám phá: người chơi tới điểm nghe/tương tác phù hợp là có thể nghe và tiến truyện; không cần nấp đúng lúc, không đếm ngược và không bị bắt nếu đứng lộ.
- Dàn dựng vị trí Tú Bà và Mã Giám Sinh để việc nghe vẫn hợp bối cảnh, ví dụ nói sau vách/cửa thông; không để nhân vật đứng nhìn thẳng Kiều mà thoại giả vờ không thấy.
- Không để chờ vô hạn vì điều kiện nấp cũ. Khi đi khỏi điểm nghe và quay lại, sự kiện phải tiếp tục hoặc có thể kích hoạt lại hợp lý, không mất manh mối hay kẹt chương.
- Loại bỏ luồng `FailSeenByTuBa()` khỏi gameplay Ch3 và cả lời thoại/HUD nhắc sẽ bị lôi về hoặc phải trốn.
- Kiểm tra việc khóa cửa sau cuộc nói chuyện: không tạo bẫy hoặc đoạn đợi lính qua để được thoát. Giữ đường về phòng và kết chương thông suốt.

## 3. Phạm vi code cần xem

- `Assets/_Game/Scripts/Gameplay/Chapters/LauTuBa/Chapter03LauWorld.cs`: đăng ký `Caught`, `_catch`, `_tuBaEyes`, `OnExplore`, `VoicesApproach`, `OnCaught`, `OnRestored`, summary và autopilot.
- `Assets/_Game/Scripts/Core/Editor/LauTuBaSetup.cs`: cấu hình watcher, vùng cấm, HUD và gắn `CatchScene` ở Ch3. Ch3 không cần cài cảnh bắt; giữ phần cấu hình Ch4.
- Ink đang dùng cho scene Ch3 và pipeline compile sẵn có; bắt đầu từ `Assets/_Game/Data/Dialogue/Ink/Chapter3_LauTuBa.ink`, xác minh asset thực tế được scene sử dụng trước khi sửa. Không sửa nhầm bản truyện cũ không còn chạy.
- Các checks/autopilot liên quan Ch3 hiện vẫn có ép thua và `CatchProbe` kiểm tra `A_ch3_drag`; cập nhật chúng theo quy tắc mới. Bài thử không được bí mật bật lại khả năng bắt để làm test cũ PASS.

Ưu tiên cấu hình riêng cho Ch3, không tắt `Watchman`, `SightModel`, `CatchScene` hoặc cơ chế bắt toàn game. Không để thay đổi Ch3 làm Ch4 mất phát hiện/chase/jumpscare. Dùng công cụ Unity phù hợp cho scene, không sửa YAML thủ công. Không xóa asset hoặc commit ngoài quyền đã được cấp.

## 4. Chapter 4 — giữ thử thách

- Giữ phát hiện → truy đuổi → bắt hợp lệ → jumpscare → thất bại/checkpoint.
- Giữ các biến thể trước/sau/chỗ nấp, xử lý hai NPC cùng bắt và chống bắt xuyên tường/cửa.
- Các rủi ro và hướng dẫn stealth thuộc Ch4. Không tự tăng độ khó hoặc sửa cân bằng sân trong đợt tách phạm vi này.

## 5. Nghiệm thu

- Ch3: đi qua hiên, sân và khu phía sau; đứng trước lính/Tú Bà; chạy tạo tiếng động; thử nấp rồi ra. Không có chase, cảnh bắt, thẻ thất bại hoặc dịch chuyển trừng phạt.
- Ch3: hoàn thành nghe/đọc manh mối khi không nấp, thử đi khỏi rồi quay lại điểm nghe, và hoàn thành chương bình thường. Kiểm tra tiếp tục từ checkpoint/save cũ nếu đang được project hỗ trợ.
- Thay bài thử bắt Ch3 bằng bài thử khám phá không bị bắt và tiến truyện thành công; không giữ kỳ vọng có một cảnh bắt ở Ch3. Không lấy kết quả test trước thay đổi làm bằng chứng.
- Ch4: chạy checks/stealth/catch/checkpoint liên quan để xác minh tính năng vẫn hoạt động. Chạy chuỗi Ch3 → Ch4 và chuỗi chương hiện có theo phạm vi ảnh hưởng.
- Recompile, sửa lỗi phát sinh, ghi chính xác đã chạy/chưa chạy gì. Không tự quay thêm clip; việc nghiệm thu chuyển động/âm thanh có thể thực hiện trực tiếp trên game theo quyền công cụ hiện có.
- Cập nhật mục mới trong `CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md` nêu quyết định mới, không sửa số liệu lịch sử thành như đã kiểm tra với thiết kế mới.

Kết quả cần bàn giao: Ch3 khám phá tự do và tiến truyện đầy đủ; Ch4 giữ gameplay kinh dị; danh sách file thay đổi, kiểm thử mới và giới hạn chưa xác minh. Nếu chỉ sửa tài liệu hoặc chưa sinh lại scene, phải báo rõ, không tuyên bố gameplay đã đổi.
