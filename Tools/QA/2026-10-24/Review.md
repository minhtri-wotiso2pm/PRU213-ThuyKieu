# Đánh giá Thúy Kiều — 24/10/2026

## Phạm vi kiểm tra

Project đang ở nhánh `GiaThuan`, đã tích hợp `origin/dev` tại `adf5b5a`.
Chạy trực tiếp trong Unity 6000.6.0f1, qua Pipeline và StoryAutopilot của project.

- Biên dịch thành công, không có lỗi hoặc cảnh báo trong kết quả recompile.
- Lượt có chụp ảnh đã hoàn thành từ Chapter 0 đến Chapter 8: `AUTOPILOT PASS`, không có lỗi runtime được bot ghi nhận. Bằng chứng: [FullChainVerified.txt](FullChainVerified.txt).
- Đã xem hình thực tế từ các chapter; 182 ảnh của lượt đạt nằm trong [CapturesVerified](CapturesVerified/).
- Lượt đầu trước sửa có 30 lần ghi nhận lỗi, gồm lỗi cầm đạo cụ, thiếu avatar và thiếu prefab ở Chapter 6: [FullChain.txt](FullChain.txt).
- Lượt ép thất bại/checkpoint cũng hoàn thành 9/9 phần, không có lỗi runtime: [CheckpointChain.txt](CheckpointChain.txt). Đã ghi nhận phục hồi checkpoint ở các hub; lấy chìa bị Tú Bà cựa mình; bị bắt tại sân sau, hẻm và rừng; đưa bằng chứng sai và thua một vòng đối chất rồi tiếp tục được.
- Bốn lần chạy riêng Chapter 7 đều `AUTOPILOT PASS`: [Nghịch mệnh](Ending_nghich_menh.txt) (`c7_ket=3`, đồng điệu 9, chọn ở lại cùng Kiều), [thắng kiện](Ending_thang.txt) (`c7_ket=2`, đồng điệu 5), [thắng mà mất mình](Ending_mat_minh.txt) (`c7_ket=2`, đồng điệu 2), [Định mệnh](Ending_dinh_menh.txt) (`c7_ket=0`, định mệnh 100). Đây là bốn cấu hình đại diện, chưa phủ hết bảy mục gallery hoặc cả hai lựa chọn của nhánh Nghịch mệnh.

Bot tự chọn hội thoại và có hỗ trợ đi tới tương tác, rút ngắn hoặc tự giải một số minigame. PASS xác nhận luồng được kiểm tra đi đến cuối, không chứng minh tất cả đường đi bằng WASD đều thông, hoặc độ khó/QTE đã cân bằng cho người chơi. Chưa chạy player build, chưa đánh giá đầy đủ âm thanh bằng nghe thực tế.

Save chương trước thử được sao lưu tại `SaveBackup`; save phát sinh từ QA được giữ riêng để bảo toàn dữ liệu. Các khóa tiến độ chương đã được lưu để khôi phục khi kết thúc. Gallery có thể được mở thêm ending do chạy thử; không xóa các unlock có sẵn.

## Các lỗi đã xử lý

1. **CS0433 do code cũ và mới cùng được biên dịch.** Thêm điều kiện `THUYKIEU_LEGACY` vào ba assembly cũ trong `_Project/Scripts`; bộ game hiện tại tiếp tục dùng `_Game/Scripts`. Không xóa code cũ.
2. **Metadata model bị sinh lại sai trong working tree.** Khôi phục metadata được Git theo dõi của model đi/chạy Kiều và bốn model Talking ở Chapter 6. Sau đó avatar và clip Humanoid, controller, màu trang phục và reference scene hoạt động lại. Đây là lỗi trạng thái import ở máy; cấu hình đúng đã có trong commit. Prefab player đã được trả về bản gốc, không cần giữ bản sửa reference thử nghiệm.
3. **Bot mang lỗi từ Play session cũ sang session mới khi tắt Domain Reload.** Reset log, lỗi và hook tại `SubsystemRegistration`; dùng callback có thể hủy đăng ký. Kiểm tra sentinel qua hai session xác nhận dữ liệu cũ được xóa.
4. **Cờ QA ép thất bại không tới được một số sequence khi chạy từ Editor.** Đồng bộ năm sequence dùng `StoryAutopilot.Args`, để nhận cả tham số từ biến môi trường mà bot hỗ trợ.
5. **Bot thorough chờ vô hạn vật tùy chọn đã hết hạn.** Tái hiện ở Chapter 2: tùy tùng đã nhặt lại mẩu giấy (`c2_giay_mat = true`), nhưng bot vẫn chặn lựa chọn đi nghe lén vì còn tên mục tiêu giấy trong danh sách Ink. Chỉ ưu tiên các mục tiêu có thể tương tác hiện tại; không để một mục tiêu đã ẩn chặn đường ra của bot.

## Nhận xét từng chapter

| Chapter | Điểm đang tốt | Nên cải thiện tiếp |
|---|---|---|
| Mở đầu — Trang sách | Lớp học, quyển sách, chuyển sang thân Kiều và tương tác gương/đàn tạo được động cơ nhập vai. Hình cầm đàn sau sửa đã có cả nhân vật và đạo cụ đúng vị trí. | Nhiều cơ chế được giới thiệu gần nhau. Nên cho người chơi tự làm từng thao tác rồi mới hiện hướng dẫn tiếp; kiểm tra camera tay áo/đạo cụ ở nhiều góc. |
| 1 — Gia biến | Không gian nhà họ Vương, ánh sáng, hỏi mẹ, tìm giấy tờ và đàm phán tạo bối cảnh có mục đích. Các lựa chọn hỏi khéo/hỏi thẳng dễ hiểu. | Đây là cảnh cần ưu tiên kiểm tra giật khung hình và góc camera trong nhà. Nên làm phản hồi về bằng chứng và nghi ngờ rõ hơn, tránh mục tiêu dồn thành danh sách dài. |
| 2 — Trên đường theo Mã Giám Sinh | Cảnh đường quê và ánh sáng hoàng hôn nổi bật. Theo đoàn, quan sát, nghi ngờ và nghe lén tạo được khác biệt với chương trong nhà. | Khi đi xa Mã hoặc bị phát hiện, cần chỉ dẫn rõ cách quay lại nhiệm vụ. Nên thử bằng bàn phím thật để kiểm tra bám đoàn, khoảng cách tương tác và lối đi quanh cây/vách tre. |
| 3 — Tú Bà | Sân nhà, người canh, cửa khóa, nghe lén và văn tự tạo cảm giác khám phá một nơi có bí mật. Hội thoại Tú Bà có khung hình đọc rõ. | HUD lặp cùng mục tiêu ở hai góc. Dòng “+ 4 việc khác có thể làm” dễ làm người mới bỏ sót việc; nên ưu tiên nhiệm vụ chính và cho xem lại danh sách đầy đủ thuận tiện. |
| 4 — Không còn đường lui | Quan sát lịch đi của Tú Bà, lấy chìa theo nhịp thở và quyết định giữ/đốt giấy tờ tạo căng thẳng và lựa chọn có hệ quả. | Hình Tú Bà ngủ trông nổi trên giường; cần chỉnh pose/độ cao. Một số vật dụng vẫn là khối đơn giản. Nên kiểm tra khả năng đọc dấu hiệu phát hiện trong ánh sáng tối. |
| 5 — Đào thoát | Chuỗi lén qua chó/đèn, đẩy cửa, chạy hẻm và thoát rừng tạo nhịp hành động rõ. Có stamina và vật cản để người chơi quyết định khi chạy. | Hẻm tối và ít điểm nhận biết hướng đi. Nên làm ngã rẽ/chợ đêm/vật cản nổi bật hơn và thử cảm giác điều khiển, camera quay gấp, độ khó khi bị đuổi bằng người chơi thật. |
| 6 — Kim Trọng | Đối chiếu hai giấy tờ và tìm nhân chứng khiến bằng chứng thu thập trước đó có tác dụng. Giao diện đối chiếu rõ, có tiến trình 1/3. | Ảnh khám phá phố cho thấy Kim Trọng đứng giữa camera và Kiều, che nhân vật. Cần chỉnh vị trí đồng hành/camera; phân biệt vị trí ba nhân chứng và phản hồi bằng chứng còn thiếu rõ hơn. |
| 7 — Nghịch mệnh | Ba vòng đối chất và lục bát là điểm kết nối tốt giữa điều tra và chủ đề Truyện Kiều. Có nhánh kết theo kết quả vụ kiện và đồng điệu. | Nên giúp người chơi hiểu vì sao bằng chứng đúng/sai và có thể xem lại lời khai sau khi sửa. Phòng xử và một số đạo cụ còn đơn giản so với model nhân vật; camera cần tránh các mặt bàn/ghế che cảnh. |
| Kết — Trở lại lớp học | Trở lại lớp học khép được vòng mở đầu; góc nhìn về Kiều có cơ hội thay đổi sau hành trình. | Nên làm kết quả các quyết định và điều đã thay đổi nổi bật hơn khi kết thúc. Cần đọc hết từng ending ở tốc độ thường để đánh giá nhịp cảm xúc. |

## Ưu tiên trước buổi demo

1. Giữ import metadata ổn định và xử lý nguồn asset trùng giữa `_Project` và `_Game` bằng một kế hoạch riêng; chưa thực hiện xóa/move hàng loạt trong lượt này.
2. Chỉnh follower che camera ở Chapter 6 và pose ngủ của Tú Bà ở Chapter 4.
3. Gọn HUD: bỏ mục tiêu lặp, tăng độ rõ của thanh Định Mệnh/hướng dẫn nhỏ, giảm lượng chữ xuất hiện đồng thời.
4. Thử thủ công các đoạn cần điều khiển chính xác: bám đoàn ở Chapter 2, lấy chìa ở Chapter 4, chạy trốn ở Chapter 5 và đối chất ở Chapter 7.
5. Đo hiệu năng ở player build trước khi kết luận FPS. Lượt có screenshot có thêm chi phí render/ghi ảnh; Chapter 1 có khung hình khoảng 1,9 giây trong lượt này, cần kiểm tra lại ở điều kiện chơi bình thường.

Đánh giá hiện tại: game đã có luồng nội dung hoàn chỉnh và các chapter có cơ chế riêng. Phần cần ưu tiên tiếp là camera, tính rõ ràng của hướng dẫn, đồng nhất hình ảnh và kiểm thử cảm giác chơi thực tế; chưa nên coi việc bot đi hết game là tiêu chí duy nhất để chốt bản demo.
