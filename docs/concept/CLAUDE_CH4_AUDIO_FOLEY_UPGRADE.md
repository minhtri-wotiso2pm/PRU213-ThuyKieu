# Prompt cho Claude — thay chất âm bước chân và cửa ở Ch4

## Phản hồi cần xử lý

Người dùng đã nghe game và đánh giá bước chân/mở cửa còn giả, nghe dở. Các test logic PASS không chứng minh chất lượng âm thanh. Hãy hoàn thiện audio Ch4 bằng nguồn thu thực tế và phối trong game; giữ bước chân theo vật liệu, mức ồn khom/đi/chạy, cửa khẽ/mạnh, AI nghe/điều tra và checkpoint đã hoạt động.

Đọc `AGENTS.md`, kiểm tra Git, giữ thay đổi hiện có. Phạm vi vẫn là Ch4 trước; Ch3 làm sau. Đây là bổ sung ưu tiên cho `CLAUDE_CH4_COMPLETE_REVISION.md`.

## 1. Nguyên nhân nền đã đọc trong source

- Đính chính sau khi kiểm tra routing: trong `ProceduralAudio.cs`, nhánh dự phòng `step`, `step_wood`, `step_stone` cùng dùng `Thud(0.18f, random)`, nhưng `AudioBank` ưu tiên `Recorded` trước file bake rồi mới tổng hợp. Gỗ/đá cũ đã có bản thu riêng tại `Resources/StoryAudio/Recorded/sfx/`. Không thể suy ra clip runtime chỉ từ nhánh synth. Các tiếng ướt/đất/chiếu/bùn cũ dùng thud/noise; xác minh routing từng ID trước khi thay.
- `door_creak_soft`, `door_fast`, `door_slam`, `slide_*` dùng `Creak`, `Thud`, `Noise`, `Clicks` tổng hợp. Xuất chúng ra WAV không biến chúng thành bản thu Foley thật.
- `FirstPersonFootsteps` đã có pitch/volume biến thiên; tiếp tục thay pitch trên cùng nguồn không giải quyết được sự thiếu đặc trưng vật liệu.
- `AudioBankBaker` hiện giữ WAV đã tồn tại và chỉ sinh khi thiếu. Kiểm tra cả `AudioBank`/cache/routing để biết clip runtime thực dùng; không chỉ sửa hàm synth rồi kết luận exe đã đổi âm.

Chưa nghe kiểm từng file độc lập; phản hồi chất âm là của người dùng. Kiểm tra đường nạp đã sửa nhận định trước đó rằng cả gỗ và đá runtime đều là âm tổng hợp. Đợt 14 implementation đã thêm bank bản thu riêng Ch4; các mục dưới đây là brief của đợt đó. Đọc mục 14 trước khi tiếp tục, không tự chạy lại toàn bộ công việc đã làm.

## 2. Thay bằng bản thu phù hợp

- Tìm/chọn bản thu bước chân và cửa thực tế có nguồn, quyền sử dụng rõ; có thể tự thu nếu công cụ/người dùng cung cấp điều kiện. Giữ link tác giả, license, tên file gốc và chỉnh sửa đã làm trong CREDITS riêng cho bộ âm mới.
- Chọn loại giày/dép/chân phù hợp diện mạo Kiều; tránh tiếng giày lính nặng cho nhân vật nếu không khớp. Cùng một loại chân/giày xuyên suốt các vật liệu.
- Bước chân gỗ cần cảm giác lực bàn chân và cộng hưởng gỗ nhẹ; đá/gạch cứng hơn; đất/bùn êm hoặc có độ dính vừa phải; chiếu/thảm giảm chi tiết sắc. Nghe chọn thay vì suy ra chỉ bằng tên file.
- Mục tiêu ban đầu 4–8 one-shot khác nhau cho mỗi nhóm sàn chính, không lặp lại ngay cùng clip. Ưu tiên gỗ và đá/ướt, sau đó bùn và chiếu. Nếu thiếu nguồn cho nhóm nào, báo rõ, không tính bản đổi pitch là một bản thu mới.
- Khom/đi/chạy nên có lực tiếp xúc và nhịp phù hợp; tránh chỉ tăng volume một tiếng gõ bất biến. Có thể cắt các take nhẹ/mạnh trong cùng session thu và chọn theo dáng.
- Với cửa, chọn đúng loại cơ khí: cánh cửa gỗ bản lề, cửa gỗ trượt, then/chốt và va khung. Cửa mở nhẹ không kêu rít kéo dài ở mọi lần. Cửa đóng mạnh cần lực va gỗ, không giống tiếng nổ hoặc kim loại nếu cửa không có bộ phận đó.

Nguồn ứng viên đã tìm thấy, **chưa nghe duyệt chất lượng**, không mặc định phải dùng:

- [Rickmk2 — Footsteps on wooden flooring, CC0](https://freesound.org/people/Rickmk2/sounds/164315/): bản thu cận sàn laminate với nhiều tốc độ/lực; cần nghe xem chất giày và cộng hưởng có hợp sàn/nhân vật cổ trang không, rồi cắt one-shot.
- [Breviceps — Creaky Steel and Wood Doors, CC0](https://freesound.org/people/Breviceps/sounds/456816/): chứa cửa thép và gỗ; chỉ chọn phần gỗ phù hợp sau khi nghe. Không dùng lẫn âm kim loại cho mọi cửa gỗ.

Có thể chọn nguồn tốt hơn nếu có quyền rõ. Ưu tiên gói nhỏ đủ dùng; không tải cả kho nhiều GB hoặc mua pack khi chưa được phép. Nếu cần đăng nhập để tải, báo chính xác link/file bị vướng; tiếp tục routing, chỉnh sửa và kiểm thử độc lập, không tự dựng thêm noise rồi gọi là đã thay bản thu.

## 3. Biên tập và phát âm trong game

- Cắt một lần tiếp xúc thành one-shot, bỏ khoảng im thừa và tiếng nền không phù hợp, fade mép tránh click. Giữ phần cộng hưởng tự nhiên cần thiết; tránh khử noise mạnh làm tiếng bị rè/kim loại.
- Cân mức cảm nhận giữa các take và headroom, không peak-normalize mù quáng rồi gọi là đồng đều. Giữ khác biệt có chủ đích khom < đi < chạy và mở khẽ < mở mạnh.
- Bảo toàn vị trí, nhịp và thời điểm. Tiếng bàn chân vào lúc tiếp đất; tiếng cửa cần tách chuyển động và va/chốt nếu một clip chứa nhiều pha không khớp thời gian cánh cửa trong game. Tránh phát hai tiếng va trùng nhau từ clip và event.
- Dùng biến thiên pitch nhỏ sau khi đã có nhiều take thật; tránh âm cao thấp thất thường mỗi bước.
- Kiểm tra âm phát từ chân Kiều/cửa, falloff và hướng nghe; tránh cảm giác tiếng chân của mình ở xa hoặc sau lưng. Tách thành phần nghe của người chơi và tiếng phát ngoài thế giới nếu kiến trúc hiện tại cần, đồng thời chặn phát trùng.
- Reverb/occlusion vừa phải theo không gian đang có; không thêm vang dài lên mọi bước chân. Nghe cả khi chỉ có Foley và khi có mưa/nhạc để tránh bị chìm hoặc lấn toàn bộ cảnh.
- Bảo toàn bán kính noise và các quy tắc AI đã cân bằng trừ khi có lỗi cụ thể cần sửa. Volume trên loa không tự suy ra bán kính nghe AI; phải giữ tương quan rõ và kiểm tra riêng.

## 4. Tích hợp an toàn

- Ưu tiên bank/routing riêng cho bộ Foley mới của Ch4 để tránh vô tình đổi tất cả chương dùng chung sound ID. Kiểm tra chỗ dùng trong cảnh bắt, NPC và door story trước khi thay.
- Giữ asset cũ và GUID cần thiết, không xóa/ghi đè hàng loạt. Nếu dùng ID/file mới, nối đủ runtime và fallback, tránh silently rơi về tiếng synth bị chê.
- Đảm bảo các nguồn mới vẫn được dùng sau bake/build; ghi rõ clip nào là bản thu ngoài, clip nào tự thu/chỉnh, clip nào còn tổng hợp. Không có thao tác reset/bake nào tự thay nguồn mới mà không được báo.
- Không thêm gói audio/pipeline chỉ để thay nguồn âm. Tuân thủ công cụ Unity, quyền asset/build và Git trong `AGENTS.md`.

## 5. Nghiệm thu chất âm

- So sánh A/B nguồn cũ và mới ở mức nghe tương đương trên tuyến hiên gỗ → bậc đá → sân ướt → chiếu → bếp/hẻm; mỗi vùng đi/khom/chạy và chuyển vật liệu giữa chừng.
- Thử cùng cửa mở/đóng nhẹ và mạnh, cửa trượt, chốt và cửa khóa; nghe xem lực va có khớp chuyển động, có tiếng nền lạ, click hoặc lặp máy móc không.
- Người chơi phải nhận ra bề mặt và mức hành động bằng tai. Đứng yên/ép tường/checkpoint vẫn không có bước giả.
- Kiểm tra lính/Tú Bà nghe và phản ứng như trước; giữ nhiệm vụ, J2, bắt và checkpoint. Đánh giá audio với tiếng bật; test muted chỉ chứng minh logic.
- Chưa nghe trực tiếp được thì báo rõ chưa duyệt chất âm, không tuyên bố nghe tự nhiên nhờ waveform, tên file hoặc test PASS.
- Giữ yêu cầu dừng quay clip mới trong phiên hiện tại. Dùng nghe trực tiếp khi công cụ cho phép; không tự bật capture.
- Bàn giao danh sách nguồn/biến thể đã dùng, routing, kết quả nghe/logic, tình trạng exe và phần còn thiếu; cập nhật tài liệu implementation. Chỉ build khi có quyền và điều kiện phù hợp.

Kết quả mong muốn là Foley nghe tự nhiên, đúng nhân vật và vật liệu, với nhịp/không gian hợp cảnh; không chỉ là code phát được nhiều sound ID hơn. Tài liệu này chưa tải hay thay bất kỳ file âm thanh nào.
