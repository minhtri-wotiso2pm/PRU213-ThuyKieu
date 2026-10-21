# Tham khảo và đề xuất nâng cấp minigame lấy chìa — Ch4

Ngày: 21/10/2026. Đây là **đề xuất thiết kế để thảo luận**, chưa phải lệnh triển khai toàn bộ cơ chế mới. Chưa sửa code, scene hoặc các yêu cầu gameplay đã chốt.

**Bước triển khai hiện tại:** người dùng đã yêu cầu prompt để Claude sửa gameplay mới. Đặc tả thực hiện nằm ở `CLAUDE_CH4_KEY_THEFT_MINIGAME.md`; dùng file đó khi triển khai. Tài liệu này giữ vai trò tham khảo nguồn và quá trình lựa chọn thiết kế.

**Cập nhật hướng thiết kế:** dùng phương án không hiển thị tay Kiều. Không yêu cầu thêm model/rig tay góc nhìn thứ nhất. Thể hiện tác động qua chuyển động vòng/chìa/móc, âm thanh và camera cận vừa phải, vẫn thấy một phần mặt/ngực Tú Bà.

## 1. Những yếu tố đang giữ làm nền

- Người dùng đã chọn 12 giây thao tác hợp lệ trong nhịp thở ra; khoảng 20–30 giây tổng cộng là mục tiêu cảm giác, không bảo đảm nếu người chơi thao tác chậm hoặc chờ thêm.
- Giữ E trong hít vào khi đang lấy chìa → Tú Bà cảm thấy tay, tỉnh và bắt/jumpscare. Buông E đúng lúc bảo toàn tiến độ giữa các nhịp.
- Source hiện tại đã có `KeyTheft` 12 giây, trạng thái tay trên chìa và `TuBaSleeper` dùng pha thở chung với audio. Cần kiểm tra runtime để biết mức hoàn thiện, không giả sử vẫn là bản 3,2 giây cũ.
- Phạm vi là cảnh lấy chìa Ch4, không mở rộng map/cốt truyện hoặc thay gameplay Ch3.

## 2. Các game tham khảo và điều nên học

| Tham khảo | Cơ chế | Bài học cho lấy chìa |
|---|---|---|
| [The Quarry — Don't Breathe, mô tả/tùy chọn điều khiển](https://gameaccess.info/the-quarry-motor-accessibility-related-options/) | Giữ nút để nín thở trong đoạn trốn nguy hiểm | Việc giữ/thả có ý nghĩa khi người chơi đọc được thời điểm nguy hiểm qua tín hiệu trong cảnh. Lấy nguyên tắc tín hiệu và nhịp; không đổi chiều thao tác E đã chốt của Kiều |
| [Until Dawn — Don't Move/Stay Calm, bài viết của creative director trên PlayStation Blog](https://blog.playstation.com/2024/08/14/until-dawn-rebuilt-and-enhanced-for-ps5-and-pc-launches-october-4/) | Thử thách giữ bình tĩnh/không cử động; bản làm lại có lựa chọn thay thế cho thiết bị thiếu gyro | Khoảnh khắc phải ngừng hành động có thể tạo căng thẳng. Không bê yêu cầu cảm biến tay cầm sang chuột/bàn phím |
| [FNAF: Help Wanted — nhà phát triển](https://steelwoolstudios.com/game/five-nights-at-freddys-help-wanted/) và [Parts and Service — mô tả cộng đồng](https://triple-a-fazbear.fandom.com/wiki/Parts_and_Service_%28Challenge%29) | Thao tác bảo trì trực tiếp trên nhân vật có thể gây nguy hiểm, theo từng nhiệm vụ/chỉ dẫn | Cho người chơi thấy vật đang được cầm, chốt đang được tháo và tiến triển thực tế ngay sát nhân vật nguy hiểm |
| [Dead by Daylight — Repair, trang gameplay chính thức](https://deadbydaylight.com/game/?category=survivor) | Khi sửa máy có skill check; trượt gây báo động và mất tiến độ | Một tương tác kéo dài có thể có thời điểm cần chú ý. Với cảnh Kiều, không nên thêm ngay vòng QTE ngẫu nhiên bên cạnh luật thở nếu chỉ làm rối thao tác |

Các mô tả áp dụng phía dưới là thiết kế riêng cho Lầu Tú Bà, không phải cơ chế các game kia có sẵn hoặc số đo từ chúng. Chưa xem/đo trực tiếp toàn bộ video gameplay trong lượt nghiên cứu này.

## 3. Phương án khuyến nghị: nhịp thở + gỡ chìa trực tiếp theo ba bước

### Trải nghiệm tổng thể

Từ góc nhìn thứ nhất, người chơi cúi xuống cạnh sập. Trong khung hình có chùm chìa, móc/dây treo và một phần người Tú Bà; tay Kiều không hiển thị. Có thể nghe hơi thở, vải áo và rung kim loại nhỏ. Tiến độ thể hiện bằng vị trí vật thể; HUD hỗ trợ gọn, không che mặt/động tác của Tú Bà. Điểm nắm/hướng kéo được gợi ý nhẹ ngay tại vòng chìa.

Khi vào minigame, camera tiến và thu góc mượt, chìa ở tiền cảnh và mặt/ngực Tú Bà còn thấy trong khung. Giữ góc cận khi buông E để chờ hơi thở; chỉ trả góc chơi khi hoàn thành, chủ động rút lui hoặc chuyển sang cảnh bắt. Không zoom ra/vào mỗi nhịp giữ/thả E.

Luật điều khiển đề xuất:

- **Thở ra:** giữ E để nắm, kéo chuột chậm theo một hướng gợi ý rõ để thực hiện bước hiện tại.
- **Hít vào:** buông E; tay dừng/rút khỏi tác động nguy hiểm. Giữ E qua ranh giới vẫn gây bắt như yêu cầu đã chốt.
- Chuột điều khiển thao tác khi đang nắm; camera không đồng thời xoay tự do theo cùng input. Nhả E dừng thao tác nhưng giữ góc cận; khi thật sự thoát minigame hoặc thất bại thì trả quyền camera đúng luồng. Tránh một input vừa quay đầu vừa kéo chìa.
- Mất target/quay ra khỏi tương tác phải rút tay và hủy phiên nắm rõ ràng, không lách luật vì `Tick` bị ngắt. Chỉ áp nguy hiểm cho tương tác lấy chìa hợp lệ.

### Ba bước vật lý, tổng vẫn 12 giây thao tác hợp lệ

| Bước | Người chơi làm | Thay đổi nhìn thấy |
|---|---|---|
| 1. Nâng và giữ chùm chìa | Giữ E, kéo nhẹ để đưa chìa khỏi chỗ tựa vào áo | Vòng nghiêng và các chìa nâng ít một, vẫn gắn đúng móc cho tới lúc tháo |
| 2. Gỡ vòng khỏi móc | Giữ E, kéo theo hướng mở của móc, không kéo ngược vào thắt lưng | Vòng tiến dần qua miệng móc; góc vòng thay đổi theo thao tác |
| 3. Rút chùm chìa ra | Giữ E, kéo về phía Kiều trong nhịp an toàn | Vòng thoát khỏi móc, chùm được kéo ngắn xuống ngoài khung rồi thu vào inventory một lần |

Chia ban đầu 4 + 4 + 4 giây hợp lệ làm mốc thử, không phải ba thanh chờ riêng. Thời gian chỉ cộng khi thở ra, giữ E và thao tác đúng điều kiện; đứng giữ nguyên không tự kéo vật hoàn tất. Độ nhạy và biên hướng nên rộng đủ, không biến cảnh thành bài dò pixel. Có thể chọn hướng kéo nhất quán cho mọi bước nếu thử nghiệm cho thấy đổi hướng gây rối.

Model móc phải có cấu tạo gỡ được đúng như cảnh thể hiện. Chưa chốt hình dạng móc thì xác định cơ cấu đó trước, tránh animation vòng xuyên qua một khoen kín.

## 4. Tạo căng thẳng bằng phản hồi trong cảnh

- Cuối thở ra có dấu hiệu tự nhiên báo chuyển pha: hơi thở ngắn dần, ngực/áo chuyển động phù hợp; giữ nhãn cảnh báo chuyển pha hiện có nếu người chơi cần. Audio, animation và luật dùng cùng nguồn thời gian.
- Người chơi nhìn thấy vòng gần ra khỏi móc trong khi nhịp hít sắp tới: có quyết định dừng lại hay cố thêm; luật phạt giữ E trong hít vào vẫn rõ.
- Chuyển động chìa cần có độ trễ/đà rất nhỏ để có trọng lượng, nhưng khi buông E phải dừng theo luật rõ ràng; không có quán tính ẩn khiến bị bắt sau khi đã thả đúng lúc. Chìa chỉ chuyển động do thao tác đang có và kết cấu đang giữ nó, tránh bay tự do hoặc nhảy vị trí không giải thích được.
- Tiếng kim loại nhỏ và vải đủ nhận biết, không tự làm Tú Bà tỉnh mỗi lần thao tác bình thường. Nếu sau này thêm cơ chế tiếng chìa đánh thức, cần thiết kế/kiểm tra riêng, không tự nối mọi SFX vào Noise.
- Không đổi nhịp thở bất ngờ không báo trước, không random cựa mình làm thất bại vô cớ, không để camera shake do môi trường tính thành lỗi chuột của người chơi.
- Có thể rút lui chủ động và quay lại với tiến độ đang có theo luật cũ. Checkpoint chưa lấy xong reset như hiện tại; lấy xong thì không nhận thêm chìa.

## 5. Phương án nhỏ hơn nếu cần giảm công dựng

Giữ E theo nhịp như hiện tại, nhưng thể hiện ba bước vật lý bằng animation vòng/móc/chìa, điểm nhìn rõ và âm đồng bộ, không hiển thị tay; người chơi chưa cần kéo chuột. Đây là nâng trải nghiệm trình diễn, mức tương tác thấp hơn phương án khuyến nghị. Không kéo dài thời gian hơn 12 giây để bù thiếu hoạt động.

Chỉ cân nhắc cơ chế giữ tâm/giữ tay ổn định hoặc skill check nếu sau thử nghiệm vẫn cần thêm thử thách. Không ghép cùng lúc giữ E, kéo chuột, thanh cân bằng và QTE ngẫu nhiên vào một đoạn 20–30 giây.

## 6. Những gì cần thử trước khi chốt triển khai

- Người chơi hiểu nắm/gỡ/rút từ vật thể và hướng dẫn ngắn, không phải đọc đoạn hướng dẫn dài.
- Chơi đúng cần đủ 12 giây hoạt động, tổng thời gian hợp mục tiêu khi có các nhịp chờ; không tự cộng tiến độ khi đứng yên.
- Thở vào báo rõ bằng cả tín hiệu nghe và nhìn; không phạt vì âm trễ, cập nhật lệch một frame hoặc độ nhạy chuột khác nhau.
- Thử đúng/sai nhịp, nhả E ở ranh giới, FPS thấp, đổi target, tạm dừng, bị bắt, checkpoint và inventory.
- Tiến độ và chùm chìa không xuyên mô hình khi Tú Bà nằm/cựa/ngồi. Không thêm tay Kiều; nghiệm thu chuyển động vòng/móc, âm kim loại và camera, không dùng camera rung thay thế toàn bộ chuyển động đạo cụ.
- Sau khi chọn phương án, mới cập nhật prompt triển khai và kiểm thử tương ứng. Tài liệu này chưa thay thế luật lấy chìa 12 giây đã chốt bằng một hệ thống mới.
