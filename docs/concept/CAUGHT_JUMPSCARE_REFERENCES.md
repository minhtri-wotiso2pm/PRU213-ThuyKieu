# Tham khảo và thiết kế cảnh bị bắt — Lầu Tú Bà

Ngày tổng hợp: 19/10/2026. Đây là tài liệu nghiên cứu/đề xuất, chưa thay đổi gameplay, animation hoặc âm thanh của game.

**Yêu cầu mới của người dùng, 19/10/2026:** trong phạm vi Lầu Tú Bà, truy đuổi, bắt giữ và jumpscare chỉ áp dụng Chapter 4. Chapter 3 cho khám phá tự do, không bị NPC bắt, phạt hoặc ép chơi lại. Yêu cầu này thay thế thiết kế cũ có cảnh bắt ở Ch3. Xem `CLAUDE_CH3_FREE_EXPLORATION_CH4_HORROR.md` để triển khai. Các mô tả cảnh bắt bên dưới chỉ còn áp dụng Ch4. Việc sửa tài liệu này chưa sửa code hoặc scene.

## Nguồn để nhóm và Claude mở xem

**Thứ tự thực hiện mới:** hoàn thiện Ch4 trước theo `CLAUDE_CH4_FIRST_FINISH.md`; chuyển Ch3 sang khám phá tự do ở đợt sau. Chưa sửa luồng Ch3 trong đợt Ch4.

Các video dưới đây là bản ghi gameplay của cộng đồng, không phải tài nguyên để trích xuất vào project. Mốc thời gian Alien lấy theo danh sách chapter do người đăng cung cấp. Các nhịp dựng ở phần đề xuất là thiết kế riêng cho Lầu Tú Bà, không phải số đo từ game tham khảo.

| Nguồn | Cảnh cần xem | Điều nên nghiên cứu | Áp dụng cho Lầu Tú Bà |
|---|---|---|---|
| [Alien: Isolation — All Alien Death Animations, edepot, 2:28](https://www.youtube.com/watch?v=qZB-EbAhTYw&t=148s) | Hai biến thể bắt phía trước, từ 2:28 và 2:33 | Khoảng cách áp sát, cách chuyển từ gameplay sang cảnh bắt, độ chiếm khung hình của kẻ bắt | Lính gác vươn tay nắm vai/cổ áo, kéo Kiều về phía mình, mặt tiến sát camera |
| [Cùng video, 4:06](https://www.youtube.com/watch?v=qZB-EbAhTYw&t=246s) | Hai biến thể kéo khỏi chỗ nấp, từ 4:06 và 4:19 | Làm chỗ nấp mất tác dụng bằng hành động gắn với đồ vật; biến thể nhanh/chậm | Rèm bị giật sang bên hoặc cửa tủ bật mở, bàn tay tiến vào rồi kéo người chơi ra |
| [Outlast — hướng dẫn tình huống bị tìm thấy](https://steamcommunity.com/sharedfiles/filedetails/?id=339344242&l=english) | Mục “How to Hide”: bị phát hiện trong tủ/dưới giường, kéo ra và xô đi | Cảm giác bị một người khống chế cơ thể trong góc nhìn thứ nhất | Hợp với lính gác người thật hơn biểu cảm quái vật. Lưu ý trong Outlast, bị kéo khỏi chỗ nấp không phải lúc nào cũng là chết ngay |
| [FNAF VR: Help Wanted — All Jumpscares](https://www.youtube.com/watch?v=HvWhxFmUpok) | Tuyển tập cảnh áp sát camera | Độ rõ của khuôn mặt/silhouette, nhịp lunge và âm thanh cùng điểm nhấn | Dùng để nghiên cứu nhịp ngắn và khả năng đọc hình ảnh. Tự dựng chuyển động phù hợp người thật và bối cảnh cổ trang |

Nguồn chính thức bổ trợ: [Outlast — Red Barrels](https://redbarrelsgames.com/games/outlast/), [Help Wanted — Steel Wool](https://www.steelwoolstudios.com/five-nights-at-freddys-help-wanted).

## Quy tắc trải nghiệm được đề xuất

Phân biệt **bị phát hiện** với **bị bắt**:

1. Nghi ngờ: NPC nhìn về phía tiếng động/người chơi, có lời hoặc động tác phản ứng ngắn; người chơi vẫn điều khiển được.
2. Xác nhận nhìn thấy: lời quát, bước chân và nhịp truy đuổi; người chơi có thể chạy, cắt tầm nhìn và nấp theo luật đang có.
3. Bắt được: NPC đủ gần, có tầm tiếp cận hợp lệ và không bị tường/cửa kín ngăn; lúc này mới khóa điều khiển và chạy cảnh bị bắt.
4. Kết thúc: chuyển tới màn thất bại/khôi phục checkpoint đúng hệ thống hiện tại.

Không phát một cảnh túm cận mặt ngay khi NPC thấy Kiều từ xa. Nếu một tình huống cốt truyện kết thúc ngay khi bị phát hiện, phải dàn dựng riêng cho tình huống đó; không giả NPC đã dịch chuyển xuyên sân tới người chơi.

## Ba biến thể phù hợp

### A. Bị chặn phía trước — bản chính, làm trước

Lính tiến vào khoảng bắt, nắm vai/cổ áo. Camera giật ngắn theo cú nắm, khuôn mặt áp sát, một tiếng quát dứt câu; góc nhìn chao xuống rồi tối để khôi phục checkpoint. Nguồn sáng ấm gần đó giúp đọc mắt, bàn tay và nét mặt.

Nhịp đề xuất ban đầu, cần chỉnh khi xem animation thực tế:

| Thời gian từ lúc bắt hợp lệ | Diễn biến |
|---|---|
| 0–0,15 s | Chặn việc bắt lặp, khóa di chuyển/tương tác; giảm nhẹ âm nền để tạo điểm nhấn |
| 0,15–0,45 s | Tay NPC lao vào, âm vải bị nắm và cú kéo đồng bộ |
| 0,45–1,10 s | Kéo lại gần, nét mặt rõ; lời thoại đề xuất: “Định trốn đi đâu?” |
| 1,10–1,60 s | Một chuyển động xô/kéo ngắn, camera mất thăng bằng có kiểm soát rồi tối |
| Sau đó | Hiện trạng thái thất bại/tiếp tục theo checkpoint; không thêm đoạn hội thoại dài |

Giữ biểu cảm và tỉ lệ khuôn mặt của NPC. Không làm mặt méo, kéo giãn miệng hoặc thêm diện mạo ma nếu cốt truyện chưa có yếu tố đó.

### B. Bị túm từ phía sau — bản thứ hai

Âm chân dừng sát sau lưng, bàn tay/ống tay áo xuất hiện ở mép khung hình, cú kéo khiến camera quay một phần về kẻ bắt. Kết thúc dùng chung nhịp ngắn của bản A.

- Hướng chuyển động phải dựa trên vị trí NPC thực lúc bắt.
- Tránh quay camera 180° tức thời và tránh di chuyển nhân vật qua tường chỉ để đạt đúng bố cục.
- Tình huống thiếu khoảng trống dùng biến thể ít dịch chuyển hoặc chuyển tối sớm, không ép toàn bộ cảnh vào chỗ hẹp.

### C. Bị kéo khỏi chỗ nấp — bản tạo bất an mạnh nhất

Sau khi NPC thực sự tìm đúng chỗ nấp theo luật AI: có một nhịp dừng ngắn, tiếng bước chân ngừng trước chỗ nấp, cửa/rèm dịch chuyển, mặt và tay xuất hiện qua khoảng mở rồi túm người chơi.

- Tủ, bình phong, rèm và gầm đồ vật cần animation phù hợp loại chỗ nấp; không dùng cảnh mở cửa tủ cho mọi vị trí.
- Phải khớp trạng thái NPC biết vị trí. NPC không được mặc nhiên tìm trúng mọi chỗ nấp khi chỉ có nghi ngờ.
- Giữ khoảnh khắc ngắn, đủ nhận ra chỗ nấp vừa bị khám phá; tránh kéo dài đoạn chờ mỗi lần thất bại.

## Ánh sáng, âm thanh và chất lượng dựng

- Mặt và bàn tay là trọng tâm: ánh đèn lồng hắt một bên, bóng giữ thể tích, mắt không chìm hoàn toàn vào đen. Kiểm tra Ch3, Ch4 và các vị trí thực tế, không chỉ ánh sáng thử trong Editor.
- Vải áo, chân chạm nền, động tác nắm và tiếng thở cùng phục vụ một cú bắt. Điểm nhấn âm thanh phải đúng khung chuyển động; không dùng tiếng hét cực lớn thay cho animation.
- Kiểm tra cận mặt ở khoảng cách camera cuối cùng. Nếu model/rig mặt chưa đủ tốt, giữ khuôn mặt xa hơn, dùng bàn tay và silhouette để tạo lực, thay vì ép model thô sát ống kính.
- Rung camera ngắn theo cú kéo; giữ được hình ảnh để người chơi hiểu ai bắt và từ đâu.
- Có thể dùng 2–3 câu thoại ngắn và hai biến thể nhịp bắt đã kiểm tra, tránh lặp y hệt; chọn biến thể theo bối cảnh trước khi xét ngẫu nhiên.
- Trình diễn lần đầu phải mượt. Nạp trước tài nguyên cảnh bắt theo cách phù hợp hệ thống; đo lần bắt đầu tiên vì project đang có vấn đề khựng và biến thể shader mới.

## Yêu cầu tích hợp cho bước triển khai sau

- Đọc luồng bắt hiện có của `Watchman` và `Chapter04LauWorld` trước khi thay đổi. Phân biệt sự kiện phát hiện, vào truy đuổi và bắt hợp lệ; không gắn cảnh bắt trực tiếp vào mọi lần tăng awareness.
- Đảm bảo chỉ một NPC sở hữu cảnh bắt tại một thời điểm. Chặn kích hoạt lặp khi nhiều lính tới cùng lúc.
- Kiểm tra vị trí, hướng camera, cao độ đứng/cúi, bậc thềm, cửa, lan can và tường trước khi chọn biến thể. Giữ toàn cảnh trong môi trường hợp lệ, tránh tay/mặt xuyên camera và đồ vật.
- Dừng tác động điều khiển, va chạm và AI có thể làm cảnh bắt bị đẩy lệch; khôi phục toàn bộ sau checkpoint, gồm camera, FOV, HUD, audio, time scale và tương tác. Thích nghi với hệ thống sẵn có thay vì tạo một hệ thống thất bại cạnh tranh.
- Chỉ Ch4 có luồng thất bại do NPC bắt. Ch3 không có cảnh bắt, kể cả bị đưa về hiên. Lời thoại ở trên là đề xuất mới, cần đối chiếu nhân vật và nội dung hiện có.
- Nghiệm thu tối thiểu ở Ch4: bắt phía trước/sau, đứng/cúi, trong chỗ nấp, cạnh tường/cửa/bậc, hai NPC bắt cùng lúc, lần đầu và lần bắt lại sau checkpoint. Kiểm tra trực tiếp có âm thanh để đánh giá nhịp, không chỉ dùng ảnh tĩnh hoặc autopilot PASS. Giữ yêu cầu dừng quay mới trong phiên hiện tại; không tự bật lại công cụ quay clip.

## Thứ tự đề xuất cho nhóm

Làm bản A thật chỉn chu với model/animation hiện có; sau đó thêm bản C cho chỗ nấp quan trọng và bản B cho hướng tiếp cận phía sau. Tiêu chí đạt là cú bắt có lực, đúng vị trí, đọc được khuôn mặt và khôi phục game sạch sau thất bại. Tài liệu này chưa yêu cầu thay cân bằng sân, tầm nhìn AI hoặc cốt truyện.
