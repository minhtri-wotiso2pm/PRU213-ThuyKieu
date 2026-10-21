# Bổ sung cho Claude — bước chân, tiếng cửa và khả năng nghe ở Ch4

## Yêu cầu người dùng

Hoàn thiện Chapter 4 trước. Kiều do người chơi điều khiển phải phát tiếng bước chân theo vật liệu đang giẫm lên; chạy lớn tiếng và dễ bị nghe hơn đi, khom nhẹ hơn. Mở cửa mạnh cũng gây chú ý cho lính canh và Tú Bà. Hoàn thiện cả âm thanh người chơi nghe được lẫn phản ứng gameplay. Chưa chuyển gameplay Ch3 trong đợt này.

Đọc `AGENTS.md`, `CLAUDE_CH4_FIRST_FINISH.md` và các luồng đang có trước khi sửa. Không xây một hệ thống tiếng động thứ hai.

## Hiện trạng đã đọc trong code — cần xác minh trên scene/runtime

- `Gameplay/Stealth/FirstPersonFootsteps.cs`: đã dò sàn bằng raycast/`SurfaceTag`, tích lũy bước theo tốc độ, phát SFX và `Noise.Emit`. Bán kính cơ sở khom/đi/chạy hiện là 1,5/4/9 m, nhân hệ số bề mặt. Đây là thông số hiện tại, không phải mức đã được duyệt cân bằng.
- `Environment/SurfaceTag.cs`: có Wood, Stone, WetStone, Earth, Mat, các âm `step_wood`, `step_stone`, `step_wet`, `step_earth`, `step_mat`. Sàn thiếu tag mặc định thành gỗ nên cần kiểm tra coverage.
- `Gameplay/Stealth/Noise.cs`, `NoiseZone.cs`: bus tiếng động, giảm tầm nghe khi có vật cản và vùng mưa/nhạc che tiếng.
- `Gameplay/Stealth/Watchman.cs`: đã nghe tiếng; tiếng mạnh hoặc lặp lại dẫn tới điều tra. Không đồng nghĩa tính năng đã được nghe kiểm hoặc cân bằng trên toàn tuyến Ch4.
- `Gameplay/House/HouseDoor.cs`: đã có thao tác nhanh/giữ E làm khẽ; `Snap()` phát noise mở mạnh 8 m, đóng mạnh 6 m; thao tác khẽ có tiếng nhỏ hơn. Cần xác minh hướng dẫn và thao tác thực tế.
- `Gameplay/Chapters/LauTuBa/TuBaSleeper.cs`: đã nghe noise, cựa mình và tỉnh giấc.
- `Chapter04LauWorld.OnTuBaWoke()` hiện gọi thất bại ngay trước giai đoạn nhà thức. Khi mở rộng phản ứng nghe, phải phân biệt tiếng động từ xa với việc Kiều bị bắt gặp bên giường; tránh khiến mở cửa xa cũng thua tức thì.

Các đường dẫn trên tính từ `Assets/_Game/Scripts/`.

## 1. Tiếng chân của Kiều

- Kiểm tra vật liệu dưới chân ở hiên/ván gỗ, sảnh gạch/đá, sân đá ướt, đất/bùn bếp hoặc hẻm và thảm/chiếu. Tiếng phải khớp bề mặt thực tế và đổi đúng khi đi qua ranh giới.
- Kiểm tra tag collider ở sàn, bậc và lớp trang trí: raycast không nhận nhầm tường/đồ vật hoặc xuyên mặt sàn. Đặc biệt hẻm hiện dùng material `Mud` nhưng tag `WetStone`; xác minh bề mặt mong muốn rồi sửa phân loại phù hợp, không gán tiếng đá chỉ vì sàn bị ướt.
- Dùng quãng đường thực sự di chuyển trên đất và nhịp bước phù hợp. Đứng yên, ép vào tường, trên không, teleport/checkpoint, bị khóa điều khiển hoặc trong cảnh bắt không tạo bước chân giả.
- Cùng bề mặt: khom < đi < chạy về độ lớn và khả năng bị nghe. Cùng tư thế: thảm/chiếu êm hơn gỗ/đá. Không để thay đổi volume đơn thuần trong khi AI vẫn nhận mức ồn cũ.
- Tận dụng SFX hiện có, nghe kiểm chất lượng. Có thể dùng vài biến thể và pitch/volume biến thiên nhẹ để tránh lặp máy móc; ghi rõ nếu chỉ có một clip hoặc biến thể xử lý, không tự tuyên bố có nhiều bản thu.
- Giữ cảnh giác bằng âm thanh có thể nhận ra; không buộc người chơi phải xem HUD bán kính tiếng động liên tục.

## 2. Tiếng cửa

- Thao tác nhanh làm cửa bật/mở mạnh hoặc đóng sập: SFX có lực, phát noise một lần đúng thời điểm cơ học gây tiếng tại cửa, không tại camera.
- Giữ E mở/đóng nhẹ: chuyển động chậm, tiếng nhỏ hơn và bán kính nghe nhỏ hơn rõ rệt. Cập nhật nhãn tương tác để người chơi biết cách làm khẽ trước khi thao tác.
- Phân biệt tiếng cánh cửa, then/chốt, thử cửa khóa và trượt cửa khi asset hỗ trợ. Tránh spam nhiều noise trong một khung hoặc mỗi frame giữ E.
- Xác minh event âm thanh và event gameplay đồng bộ. Một cửa nhìn như mở khẽ không được tạo tiếng mở mạnh vô hình.
- Tiếng cửa do NPC/cốt truyện mở không được mặc định quy là Kiều hay kích hoạt vòng NPC tự nghe tiếng mình rồi điều tra mãi. Kiểm tra `SetOpen` riêng với thao tác của người chơi và giữ các sự kiện truyện có chủ đích.

## 3. NPC nghe, điều tra và truy đuổi

- Tiếng nhẹ đủ gần: ngừng/quay đầu, có tín hiệu ngắn. Tiếng lớn hoặc nhiều tiếng liên tiếp nghe được: đến vùng phát tiếng, quan sát/tìm kiếm.
- Chỉ nghe tiếng thì NPC biết vị trí tiếng cuối cùng, không biết chính xác vị trí mới của Kiều. Nếu Kiều đã rời đi, NPC kiểm tra nơi cũ; bước chân mới nghe được có thể cập nhật điểm điều tra.
- Khi xác nhận thấy Kiều theo luật hiện tại thì truy đuổi; bắt hợp lệ mới jumpscare. Khi đang truy đuổi, âm thanh có thể giúp lần theo vị trí nghe được nhưng không xuyên mọi vật cản.
- Có giảm nghe theo khoảng cách, tường/cửa đóng và vùng mưa/nhạc. Không cho mọi NPC toàn map cùng nghe một bước chân; cũng không để cửa/tường triệt tiêu hoàn toàn mọi tiếng mạnh nếu luật đã chọn là giảm âm.
- Kiểm tra nhịp tích lũy tiếng và hồi nghi ngờ để một bước khom đơn lẻ không tự thành truy đuổi, nhưng chạy liên tục gần lính phải nguy hiểm rõ hơn.

## 4. Tú Bà

- Đang ngủ: tiếng yếu/ngắn gần đó có thể làm cựa mình; tiếng mạnh đủ gần hoặc lặp lại có thể đánh thức, có tín hiệu như ngừng ngáy, lời lẩm bẩm hoặc đứng dậy.
- Tiếng cửa từ xa chỉ dẫn tới phản ứng nghe/tỉnh/kiểm tra hoặc gọi lính phù hợp tình huống, không thua ngay và không biết Kiều đang ở đâu nếu chưa nhìn thấy.
- Phân biệt trạng thái bị tỉnh do tiếng động với tình huống trộm chìa bên người. Rà luồng `Woke`, `OnTuBaWoke`, key theft và J2 trước khi sửa; không làm mất manh mối, chìa khóa hoặc kẹt giai đoạn vì thức sớm.
- Khi thức, Tú Bà có thể điều tra nơi phát tiếng theo vai trò và cơ chế đang có; chỉ kích hoạt cảnh bắt khi đủ điều kiện, không dùng tỉnh giấc như bắt từ xa.
- Kiểm tra save/checkpoint: khôi phục đúng trạng thái ngủ/thức, nghi ngờ và hành vi; không phát lại noise cũ khi khôi phục.

## 5. Nghiệm thu

- Nghe trực tiếp có bật tiếng: từng loại sàn × khom/đi/chạy, đổi vật liệu khi đang đi; đứng yên/ép tường/teleport không có bước giả.
- Lính ở cùng khoảng cách/tầm che: đối chứng đi khẽ với chạy, mở nhẹ với mở mạnh. Ghi noise phát ra, loại bề mặt, ai nghe và trạng thái NPC, không chỉ ghi PASS.
- Mở cửa mạnh gần lính; mở cửa khẽ cùng vị trí; cửa phía sau tường/cửa đóng; chạy trên gỗ, chiếu và đá ướt; mưa/nhạc che tiếng.
- Gây tiếng rồi rời vị trí: NPC đến nơi tiếng phát ra thay vì tự bám đúng Kiều. Kiểm tra hai NPC, đối tượng không nghe được và khả năng quay lại tuần tra khi không tìm thấy.
- Tú Bà: tiếng nhỏ khi ngủ, tiếng lớn từ xa khi ngủ, tiếng gần lúc đang cựa, sau khi đã thức, trong chuỗi lấy chìa/J2, và sau checkpoint. Có đường tiếp tục hợp lệ ở mỗi giai đoạn.
- Kiểm tra các mục tiêu chính, nhánh Lam, bắt/jumpscare và checkpoint Ch4 không hồi quy. Ch3 chưa đổi gameplay, không tự thêm phản ứng nguy hiểm cho chương này qua code dùng chung.
- Recompile và chạy kiểm tra phù hợp. Không tự quay thêm clip trong phiên hiện tại. Nếu không nghe/điều khiển trực tiếp được, báo rõ phần audio và cảm giác còn chưa xác minh; muted QA không chứng minh được âm thanh đúng.
- Ghi tài liệu implementation, file đã sửa và thông số nghe cuối cùng đã đo. Tuân thủ quyền build, scene/asset và Git theo `AGENTS.md`.

Đây là chỉ dẫn bổ sung; việc lưu file này chưa thực hiện thay đổi code, scene hoặc âm thanh.
