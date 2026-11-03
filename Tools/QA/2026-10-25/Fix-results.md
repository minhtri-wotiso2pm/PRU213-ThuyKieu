# Kết quả sửa code và kiểm thử — 25/10/2026

**Biên dịch: 0 lỗi, 0 cảnh báo. 14 kiểm tra hồi quy đạt. Console cuối lượt: 0 lỗi.**

## Đã sửa

| Vị trí | Thay đổi | Xác minh |
|---|---|---|
| Chapter 5 — QTE rừng | Đồng hồ không trôi khi pause; vòng chạy không tiếp tục xử lý trong menu. | Pause lâu hơn cửa sổ QTE vẫn chưa hết giờ; Resume còn thời gian trả lời. |
| Chapter 7 — lục bát | Dùng thời gian thực loại trừ pause cho deadline chung hai lượt chọn và phần đọc kết quả. | Không hết giờ khi pause; sau Resume mới tiếp tục đếm. |
| Chapter 7 — lời khai | Dừng thời gian đọc lời khai khi pause. | Coroutine không kết thúc sau menu; tiếp tục bình thường khi Resume. |
| Chapter 6 — Kim Trọng | Đi lệch bên 1,25 m; dùng NavMesh kiểm tra vị trí, quay về phía sau khi chỗ bên cạnh không phù hợp. Chỉ chapter 6 bật offset. | Tại điểm thử, khoảng lệch thực tế sau sample NavMesh là 0,93 m; Kiều không còn bị che ở góc camera sau lưng. Có thử W trên phố. |
| Camera góc thứ ba | Bỏ giới hạn delta chuột mỗi frame gây phụ thuộc FPS; giữ bỏ qua delta đầu khi khóa con trỏ. | Quãng rê 50 đơn vị trong 1 frame và 10 × 5 frame đều quay 7,5°. Delta đầu sau khóa bị bỏ đúng. |
| Tìm mục tiêu tương tác | Tái sử dụng buffer Physics, mở rộng khi đầy để không bỏ sót mục tiêu/vật cản. | Đủ 40 ray hits; tìm mục tiêu giữa hơn 65 collider; tường chặn, trigger không chặn. 100 ray queries sau khởi tạo: 0 byte managed allocation. |

Đồng hồ dùng chung `GamePause.UnpausedTime` không phụ thuộc slow motion và reset khi vào Play, kể cả khi tắt Domain Reload.

## File gameplay đã sửa trong lượt fix

- `Assets/_Game/Scripts/Core/GamePause.cs`
- `Assets/_Game/Scripts/Gameplay/Sequences/ForestRunSequence.cs`
- `Assets/_Game/Scripts/Gameplay/Sequences/LucBatSequence.cs`
- `Assets/_Game/Scripts/Gameplay/Sequences/TrialSequence.cs`
- `Assets/_Game/Scripts/Gameplay/Chase/Follower.cs`
- `Assets/_Game/Scripts/Gameplay/Chapters/Chapter06World.cs`
- `Assets/_Game/Scripts/Player/ThirdPersonCameraController.cs`
- `Assets/_Game/Scripts/Interaction/PlayerInteraction.cs`

Phần Hướng dẫn đã rút gọn ở lượt trước. Những sửa có sẵn ở DialogueManager, StoryHUD, scene/material và các file khác được giữ nguyên.

## Bằng chứng

- [PauseRegression.json](PauseRegression.json): 6 kiểm tra, dùng PauseMenuView.Pause/Resume thực; kiểm tra các coroutine QTE, chọn lục bát, chờ kết quả và lời khai. Giữ pause 2,2 giây, lâu hơn timeout trong tình huống test; slow motion 0,55 phục hồi đúng.
- [InteractionRegression.json](InteractionRegression.json): 5 kiểm tra mục tiêu/vật cản/buffer/cấp phát.
- [CameraRegression.json](CameraRegression.json): 2 kiểm tra delta chuột.
- [FollowerRegression.json](FollowerRegression.json): 1 kiểm tra vị trí lệch bên.
- Camera chapter 6: [trước](Captures/Fix-C6-before.png), [sau](Captures/Fix-C6-after.png), [sau di chuyển](Captures/Fix-C6-walk.png).
- Mở đầu: [cảnh và lời thoại sau Space](Captures/Fix-C0.png).

Các fixture `.cs.txt` chạy qua `execute_code` của Unity MCP, không đưa script kiểm thử vào build. Đây là kiểm thử thành phần trong Play Mode và một số tình huống dựng tại scene thật, **không phải chơi hết mọi chapter/nhánh**. Thử pause đầu tiên bị ảnh hưởng bởi thời gian biên dịch lệnh MCP; fixture đã sửa để bắt đầu và pause trong cùng lượt, rồi chạy lại đạt.

## Mở đầu và các mục chưa kết luận

- **R01 màn đen/null:** phiên mới từ MainMenu → Chơi mới hiển thị lớp học và Space chuyển sang lời Ngân, không lỗi Console. Ink tồn tại; cả chapter 0–8 đều loadable trong phiên này. Giữ guard null đã có sẵn trong working tree; không thêm cơ chế tự nhảy/reset cốt truyện để che lỗi. Chưa xác định chắc nguyên nhân của phiên cũ bị sửa code giữa lúc Play.
- **Lag khi tải/NavMesh:** chưa đổi quy trình dựng NavMesh; các chapter bật navigation ngay sau khi dựng xong, chuyển sang bất đồng bộ cần kiểm tra lại luồng khởi tạo. Chưa có phép đo chứng minh FPS/build được cải thiện; 0 allocation nêu trên chỉ áp dụng truy vấn ray đã đo.
- **Scene mới chapter 3/4, độ khó, hướng dẫn ngã rẽ và kết cục:** các nhận xét thiết kế vẫn cần lượt chơi thực tế riêng. Không sửa bố cục scene hay cân bằng khi chưa xác nhận vấn đề.
- Vị trí follower đã kiểm tra ở đoạn phố đại diện; chưa bảo đảm mọi góc xoay/lối hẹp đều không che nhân vật.

## Trạng thái bàn giao

- Unity ở **Edit Mode**, scene `Chapter00_Prologue`; không lưu các tình huống thử vào scene.
- Save trước lượt fix đã khôi phục và so nội dung khớp; tiến độ về reached=1, last=0. Các khóa PlayerPrefs có trong backup được phục hồi. [RestoreResult.json](RestoreResult.json).
- Save phát sinh giữ tại `FixGeneratedSaves/`, backup ở `FixBackup/`; không xóa dữ liệu thử.
- `git diff --check` đạt. Chưa commit/push, chưa chạy full player build.
