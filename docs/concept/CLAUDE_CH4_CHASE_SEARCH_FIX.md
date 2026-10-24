# Prompt cho Claude — sửa hành vi đuổi, tìm kiếm và bỏ cuộc Ch4

## Theo dõi sau đợt 15 — ưu tiên hiện tại

Đợt 15 đã sửa và kiểm tra lại những lỗi bên dưới; đọc mục 15 implementation trước khi triển khai, không làm lại toàn bộ đợt sửa. Chưa chốt đuổi bắt vì còn ca exe lỗi chập chờn, thiếu vài tình huống và chưa chơi tay.

Những điểm mới đã đối chiếu source/log:

- `Tools/LauTuBa/QA/exe15_chase.txt` ghi ca ba tiếng xa dần đổi sang `Chase@6.8` rồi không về tuyến. Chưa có trace của lần lỗi. Noise trong `Watchman` bị giới hạn awareness dưới ngưỡng Chase, nên cần ghi rõ có nhìn thấy Kiều hay thông tin cũ nào còn sót; chưa kết luận đây là lỗi Chase hoặc chỉ lỗi test.
- Ca này gọi `Quiet()` và `Home()` nhưng không tự đặt lại vị trí Kiều như các ca khác; nó phụ thuộc vị trí/thế giới còn lại sau ca trước. Thiết lập rõ vị trí, cửa, trạng thái NPC và điều kiện Kiều không bị nhìn thấy trong toàn ca nếu mục tiêu là kiểm tra chỉ nghe. Giữ perception bật. Nếu NPC thực sự nhìn thấy Kiều, phân loại là tiền điều kiện test không đạt hoặc chuyển sang ca tái phát hiện, không sửa AI để bỏ qua tầm nhìn hợp lệ.
- `Forget()` chưa đặt lại `_lastSeen`, `LastKnown`, `LastHeard`; xác minh phần ký ức cũ có ảnh hưởng sau reset/test/checkpoint không trước khi quyết định sửa. Không coi tên hàm là bằng chứng đã xóa mọi ký ức.
- Bật trace không tái hiện lỗi chưa chứng minh đã hết lỗi. Giữ cùng cấu hình/seed khi có thể, đo cả bật/tắt trace; dùng buffer chẩn đoán nhẹ và dump khi thất bại nếu cần. Ghi vị trí Kiều/NPC, nhìn thấy/LOS, tuổi thông tin, noise và trạng thái path ở thời điểm đổi sang Chase. Không chỉ tăng timeout 40 s hoặc bỏ ca lỗi.
- Hai ca `lost round a corner, stays still` và `makes noise there` trong log exe báo `searched none`, `caught 1` nhưng vẫn PASS. Predicate cho phép Caught và `SearchSpots.All(...)` đúng khi danh sách trống. Kết quả có thể hợp lệ cho tái phát hiện, nhưng không chứng minh đã mất dấu và tìm kiếm. Tách ca: mất dấu thật → bắt buộc đi qua Search và có điểm tìm hợp lệ; tái phát hiện → Chase/bắt là kết quả đúng. Không buộc NPC bỏ qua người chơi chỉ để test Search chạy.
- Bổ sung hai ca chưa có: thấy Kiều nhưng không thể tiếp cận; hai lính cùng truy đuổi rồi mất dấu. Kiểm tra hai lính không bị kẹt nhau và không truyền vị trí mới mà chưa có nguồn biết hợp lệ.
- Chỉ cân chỉnh 3 s mất dấu / 20 s tìm / 45 s cảnh giác sau khi logic và ca thử ổn. Kiểm tra trực tiếp khả năng đọc trạng thái bằng quay đầu, tốc độ, động tác quan sát và lời/âm hiện có. Giữ bộ âm đã được người dùng chấp nhận; không tự quay clip mới.

Bàn giao nguyên nhân và kết quả trước/sau cho các mục trên. Chưa chơi tay thì báo rõ. Các phần phía dưới là brief đợt 15 để tra cứu nguồn gốc, không có nghĩa những lỗi đó hiện vẫn chưa sửa.

## Phản hồi mới nhất

Người dùng đã nghe và chấp nhận tạm thời bộ âm đợt 14. Vấn đề cần sửa tiếp là **lính đuổi, tìm kiếm hoặc bỏ cuộc bất thường**. Tập trung chất lượng hành vi Ch4; giữ bộ âm/bán kính nghe đã có làm baseline. Chưa chuyển Ch3 sang khám phá tự do và không mở rộng map/cốt truyện.

Đọc `AGENTS.md`, kiểm tra Git, giữ thay đổi chưa commit. Đọc `Watchman`, `StoryActor`, `Chapter04LauWorld` và những phụ thuộc trực tiếp; không viết lại toàn bộ AI hoặc tạo hệ thống quản lý cạnh tranh. Những điểm dưới là phát hiện qua đọc code, **chưa tái hiện runtime trong lượt rà này**; cần kiểm tra và chứng minh nguyên nhân trước khi sửa.

## 1. Những điểm cần tái hiện trước

Đường dẫn tính từ `Assets/_Game/Scripts/`:

- `Gameplay/Stealth/Watchman.cs`, `Chase()`: sau mất tầm nhìn chỉ sang Search khi `Time.time - _lastSeen > 3f && !_actor.IsMoving`. `StoryActor.IsMoving` có thể còn true khi path pending hoặc còn remainingDistance dù không tiến được. Kiểm tra lính bị kẹt/đường không tới được có giữ Chase vô hạn không, đặc biệt khi đích được đặt lại mỗi 0,2 s.
- `OnNoise()`: khi đang Investigate, chỉ nhận điểm điều tra mới nếu nó gần lính hơn mục tiêu cũ. Vì vậy tiếng mới đã nghe, có `LastHeard`, nhưng `_target` vẫn có thể là điểm cũ. Tái hiện Kiều gây tiếng rồi chạy xa dần: lính có bỏ qua chuỗi tiếng mới hoặc quay đi khó hiểu không?
- `ReturnStep()`: chỉ gọi `MoveToWaypoint` khi `_stateTime < 0.05f`. Kiểm tra một khung chậm ngay sau khi vào Return có khiến bỏ qua việc tìm đường về tuần tra, đổi sang Patrol khi vẫn chưa về hoặc tiếp tục đường cũ không. Khởi tạo trạng thái cần bảo đảm chạy đúng một lần, không phụ thuộc cửa sổ thời gian một khung.
- `TryCheckKnownHide()`: bỏ qua kết quả `MoveTo(front)` và khi `IsMoving` false thì bắt đầu lay chỗ nấp, hết 1,5 s gọi Caught. Kiểm tra đường tới chỗ nấp bị chặn, path partial/invalid, hoặc dừng giữa đường: không được coi không di chuyển là đã tới. Giữ đúng việc NPC phải đến cạnh chỗ nấp trước khi bắt.
- `Pause(false)`/`Resume()`: chưa phát lại đường đi cho Chase/Search. Kiểm tra hội thoại hoặc tương tác làm tạm dừng giữa đuổi/tìm: khi tiếp tục, NPC có đứng lại rồi tính là đã tìm xong một điểm không? Giữ thời gian và trạng thái nhận biết hợp lý qua pause.
- Kiểm tra tương tác state/callback của J1/J2: lệnh đi theo cốt truyện không được kéo NPC ra khỏi truy đuổi đang hợp lệ hoặc tự gọi callback “đã tới” khi đường đi thất bại.

## 2. Hành vi người chơi cần hiểu được

1. **Thấy Kiều:** ghi vị trí/thời điểm nhìn thấy, tăng nhận biết và truy đuổi theo luật hiện tại. Không cắt chase khi vẫn nhìn thấy rõ chỉ vì timer hết.
2. **Mất tầm nhìn:** đi tới vị trí cuối thực sự biết, giữ cảnh giác trong một khoảng ngắn có cấu hình; không biết vị trí mới nếu chưa thấy hoặc nghe thêm. Khi không tới được, phải có xử lý hợp lý thay vì đuổi vô hạn tại chỗ.
3. **Nghe tiếng mới:** cập nhật dựa trên độ mới, mức đáng tin và nguồn thông tin. Phân biệt nơi nhìn thấy cuối với nơi nghe tiếng cuối; tránh tiếng cũ kéo mục tiêu ngược lại hoặc chỉ ưu tiên nơi gần nhất một cách máy móc. Chỉ nghe thì điều tra, không tự kích hoạt Chase mới.
4. **Tìm kiếm:** bắt đầu quanh điểm mất dấu hợp lệ, chọn một số điểm có đường tới được và quan sát tại mỗi điểm. Thời gian quan sát không bị tính trong khi vẫn đang đi; có giới hạn tổng và xử lý kẹt. Không chọn điểm phía bên kia tường chỉ vì gần theo đường chim bay nếu không có đường đi hợp lệ.
5. **Bỏ cuộc:** có tín hiệu ngắn để người chơi hiểu; quay về tuyến bằng đường hợp lệ rồi tuần tra. Có thể còn cảnh giác một lúc; không reset như chưa từng thấy Kiều ngay lập tức và không đứng lại vô hạn giữa hành lang.
6. **Nấp:** thấy Kiều chui vào thì kiểm tra đúng chỗ và phải tới đủ gần; mất dấu trước khi Kiều nấp thì tìm quanh nơi cuối biết, không tự biết tủ/bình phong đang chứa Kiều. Nếu Kiều rời chỗ nấp thì không tiếp tục bắt chỗ trống.
7. **Bắt:** vẫn yêu cầu khoảng cách, vật cản và đường tiếp cận hợp lệ. Không dùng thất bại thường để che lỗi NPC không đi tới được trong gameplay thật. Giữ fallback cho các lệnh ép thất bại có chủ đích trong test/cốt truyện khi cần và báo riêng.

Không cố làm AI khó hơn bằng tăng tốc/tầm nhìn toàn cục trước khi sửa nguyên nhân. Các ngưỡng cần đo và cấu hình riêng Ch4 nếu ảnh hưởng cân bằng; bảo toàn hành vi các chương khác đang dùng `Watchman`.

## 3. Cách chẩn đoán và kiểm tra

- Thêm/bổ sung log chỉ trong QA hoặc tùy chọn debug: NPC, state trước/sau, lý do chuyển, vị trí cuối thấy/nghe, tuổi thông tin, target, path status/pending, remainingDistance, vận tốc và thời gian không tiến. Tránh log mỗi frame và tránh debug overlay luôn hiện trên bản chơi.
- Tái hiện các tình huống có ý nghĩa trên scene Ch4 thật, với AI nhận biết bật. Normal autopilot hiện tắt perception của lính ở một số đoạn; PASS tuyến truyện đó không đủ kiểm tra chất lượng chase/search.
- Nhìn thấy → Kiều rẽ qua góc kín → lính đến điểm mất dấu → tìm → trở về. Kiểm tra cả khi Kiều đứng yên nơi mới và khi phát tiếng ở đó.
- Gây tiếng mới xa hơn trong lúc lính đang Investigate; nhiều tiếng liên tiếp không làm state reset vô hạn hoặc giữ nhầm mục tiêu.
- Đóng cửa/có vật cản trên đường; thử path partial/invalid và kẹt không tiến; FPS thấp/một khung chậm ngay khi vào Return; pause rồi tiếp tục trong Chase/Search.
- Thấy chui vào chỗ nấp, không thấy chui vào, rời chỗ nấp trước khi lính tới; không có Caught nếu lính chưa tiếp cận được.
- Hai lính, Tú Bà gọi lính, và J1/J2 giữa lúc điều tra/tìm; không giành điều khiển hoặc callback sai khiến NPC đổi hướng khó hiểu.
- Chạy lại map/stealth/catch/checkpoint và tuyến nhiệm vụ Ch4 liên quan; không bỏ test khó hoặc thay kỳ vọng chỉ để báo PASS. Đo trường hợp ngẫu nhiên với seed cố định khi cần so sánh.
- Kiểm tra trực tiếp khi có công cụ; giữ yêu cầu dừng quay clip mới. Nếu chưa chơi tay được, báo rõ những tình huống đã tái hiện tự động và phần cảm giác vẫn chưa xác minh.

## 4. Bàn giao

Recompile và xác minh thay đổi. Chỉ thao tác scene/build theo quyền và quy định `AGENTS.md`; không xóa asset/commit hoặc sửa Ch3 ngoài hồi quy phần dùng chung. Cập nhật mục mới trong tài liệu implementation, ghi nguyên nhân đã chứng minh, sửa ở đâu, ca trước/sau và giới hạn còn lại. Giữ số liệu âm thanh đợt 14 làm mốc, không tự khởi động lại đợt thay Foley.

Mục tiêu: người chơi hiểu vì sao lính đuổi, vì sao mất dấu, nơi lính tìm và thời điểm an toàn để ra khỏi chỗ nấp. Chưa sửa gameplay bằng việc lưu tài liệu này.
