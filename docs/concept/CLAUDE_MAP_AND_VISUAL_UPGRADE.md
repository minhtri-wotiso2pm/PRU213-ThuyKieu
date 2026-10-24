# Yêu cầu mở rộng và hoàn thiện hình ảnh Lầu Tú Bà

Hãy triển khai trực tiếp yêu cầu dưới đây trong project. Mục tiêu là bản hoàn thiện Chapter 3–4, không tạo một bản demo riêng và không dừng ở kế hoạch hoặc ảnh concept.

## Project và tài liệu đầu vào

**Cập nhật hướng hình ảnh:** đọc `docs/concept/PRIMARY_VIBE_DIRECTION.md` và xem `docs/concept/lau-tu-ba-primary-vibe.png` trước. Đây là ảnh mới do người dùng chọn làm tham chiếu chính về không khí, bảng màu và cách bố trí. Các ảnh concept cũ là tham khảo phụ; kích thước và kết nối map vẫn theo tài liệu mặt bằng. Phân biệt rõ tầng trên và tay cầm đèn trong ảnh với phạm vi gameplay một tầng đã có.

Project được phép chỉnh sửa:

`D:\PRU213\ThuyKieu_Share-20261009T182626Z-1-001\ThuyKieu_Share\ThuyKieuGame_With_Unity`

Scene:
- `Assets/_Game/Scenes/Chapter03_LauTuBa.unity`
- `Assets/_Game/Scenes/Chapter04_LauTuBaDem.unity`

Đọc các file sau trước khi triển khai:
- `AGENTS.md`
- `docs/concept/lau-tu-ba-expanded-layout.md` — kích thước và kết nối của mặt bằng mở rộng.
- `docs/concept/lau-tu-ba-expanded-map.png` — ảnh minh họa bố cục rộng hơn.
- `docs/concept/lau-tu-ba-visual-upgrade.png` — chuẩn hình ảnh sân–hành lang và phòng Tú Bà.
- `docs/concept/horror-visual-references.md` — game tham khảo, nguồn chính thức và hướng áp dụng.
- `docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md` — hệ thống đang có và giới hạn đã biết.
- `docs/codex/CHAPTER3_4_LAU_TU_BA_DESIGN.md` — nhiệm vụ và quy tắc gameplay.

Mở và xem hai ảnh PNG bằng công cụ đọc ảnh, không chỉ dựa vào tên file. Các ảnh là thiết kế đề xuất, không phải bằng chứng project đã đạt chất lượng đó. Khi ảnh AI có tỷ lệ hoặc mũi tên không chính xác, dùng bảng kích thước và kết nối trong tài liệu mặt bằng làm cơ sở.

## Mục tiêu và thứ tự ưu tiên

1. Map rộng, thoáng hơn, kiến trúc và tỷ lệ hợp lý.
2. Hoàn thiện mô hình, vật liệu, ánh sáng và nội thất.
3. Giữ không khí kinh dị tâm lý, lén lút chậm và căng thẳng.
4. Nhiệm vụ, tương tác, AI và checkpoint hoạt động đầy đủ trên mặt bằng mới.
5. Hiệu năng phù hợp khi chơi thực tế.

Giữ cốt truyện hai chương. Không mở thêm tầng, chương hoặc hệ thống chiến đấu. Tiếp tục dùng góc nhìn thứ nhất và render pipeline hiện tại.

## Mở rộng mặt bằng

- Khu nhà chính từ khoảng 28 × 26 m lên khoảng 42 × 38 m, tức khoảng 2,19 lần diện tích.
- Sân giữa 18 × 14 m.
- Hiên và hành lang phục vụ rộng khoảng 3 m; sau khi đặt cột và trang trí vẫn có lối đi hữu dụng rõ ràng.
- Mở rộng phòng theo bảng trong `lau-tu-ba-expanded-layout.md`.
- Giữ một tầng với tuyến vòng quanh sân.
- Phía nam là sảnh và cổng chính.
- Phía tây là phòng Kiều và Lam; phía đông là phòng Hồng và phòng khách.
- Phía bắc là bếp, kho vải, phòng Tú Bà, phòng sổ sách.
- Bếp có cửa ra ngõ sau và lối nối kho; kho có khe quan sát sang phòng Tú Bà.
- Giữ cửa trong nối phòng Tú Bà với phòng sổ sách.

Không scale toàn bộ World. Nhân vật, camera, đồ dùng và chiều cao tương tác giữ tỷ lệ người thật. Cần thay đổi hình học mặt bằng, khoảng cách và cách bố trí, không làm giường, bàn, NPC hoặc texture phình theo map.

Các phòng lớn phải có phân khu: góc ngủ, bàn cá nhân, lưu trữ, tiếp khách. Sảnh có hai cụm tiếp khách và trục giữa dẫn vào sân. Chia phòng sổ sách thành khu làm việc và lưu trữ. Tránh một căn phòng quá rộng chỉ có vài đồ vật nhỏ ở góc, đồng thời không nhồi đồ để lấp khoảng trống.

## Hướng mỹ thuật từ game tham khảo

Đọc nguồn chính thức trong `horror-visual-references.md` và tham khảo hình ảnh/trailer khi cần. Áp dụng nguyên tắc hình ảnh vào bối cảnh của project:

- **Paper Dolls Original:** cấu tạo nhà cổ, cửa/vách gỗ, đồ nội thất và lớp nhìn qua khung cửa. Ngôi lầu của mình đang vận hành, nên mức hư hỏng phải phù hợp từng khu.
- **Devotion:** đời sống nhân vật và kể chuyện bằng đồ vật. Phòng Kiều, Lam, Hồng có dấu vết và cách bố trí riêng.
- **Layers of Fear 2023:** mức hoàn thiện nội thất, tương phản vật liệu, bố cục và ánh sáng có chiều sâu. Chuyển sang đồ gỗ và trang trí phù hợp bối cảnh cổ trang, giữ Unity hiện tại.
- **Incantation:** quan sát bếp, kho và không gian chuyển tiếp; áp dụng dấu khói, ẩm, mòn ở vị trí có nguyên nhân.

Kiến trúc, trang trí và vật dụng cụ thể cần thống nhất với bối cảnh Việt đã chọn. Dùng nguồn tham khảo để tạo thiết kế và asset phù hợp cho project, không trộn nguyên mẫu của nhiều nền văn hóa thành một bộ trang trí ngẫu nhiên.

## Kỹ thuật và chất lượng hình ảnh

### Mô hình và tỷ lệ

- Cửa có khung, độ dày, bản lề hoặc ray và then.
- Bàn/tủ/sập có cấu tạo, chân, mặt, thanh giằng và chi tiết mép.
- Bo cạnh nhỏ ở đồ vật nhìn gần để bắt sáng tự nhiên.
- Rèm có nếp rủ, vải có độ dày hợp lý ở mép nhìn thấy.
- Ưu tiên hình khối và silhouette của đồ dùng gần camera; giữ mức chi tiết nhất quán.

Kiểm tra asset sẵn có trước khi dựng thêm. Không chỉ phủ texture lên khối cơ bản rồi gọi là đã hoàn thiện nội thất.

### Vật liệu

- Gỗ có vân đúng hướng, độ nhám và mức mòn biến thiên hợp lý.
- Vải nhám hơn gốm tráng men; đồng, gỗ, đá và vải phải phân biệt được.
- Sàn sân giảm normal nổi quá mạnh và độ bóng đồng đều.
- Vùng ướt/khô khác nhau; vũng nước cục bộ ở nơi hợp lý.
- Vùng dưới mái khô hơn sân; tránh mưa xuyên mái và mặt sàn dưới mái bóng như gương.
- Thống nhất tỷ lệ texture, kiểm tra lặp và nhiễu khi di chuyển.
- Bổ sung dấu sử dụng bằng decal hoặc giải pháp phù hợp: ám khói ở bếp, vệt nước dưới mái, rêu chân tường và mòn ở nơi thường chạm.

### Ánh sáng

- Nguồn sáng có lý do: đèn dầu, đèn lồng và ánh trời.
- Vùng tối vẫn nhìn thấy hình khối, cửa và vật tương tác.
- Giảm cảm giác sân sáng xanh trắng trong khi phòng chìm đen.
- Cân nhắc GI, probe và phản chiếu phù hợp project; kiểm tra khả năng hỗ trợ của shader đang dùng.
- Không tăng số đèn đổ bóng theo tỷ lệ diện tích map. Đo chi phí và chọn giải pháp thực tế.
- Kiểm tra ánh sáng mới cùng hệ thống phát hiện của người canh.

Chapter 3 giữ trạng thái chiều/chạng vạng; Chapter 4 là đêm mưa. Hai chương cùng kiến trúc nhưng khác trạng thái đèn, âm thanh và hoạt động NPC.

### Trang trí và bố cục

- Sân có một cây chính lệch tâm, cây/chum ở mép và nhiều khoảng trống có công năng.
- Hành lang có nhịp cột, rèm và vật che để chia đường nhìn; không thành hành lang trống dài hoặc đường hầm đầy vật cản.
- Phòng Tú Bà có sập làm điểm nhấn, vải đỏ trầm, bàn trà, tủ và đồ gốm/đồng.
- Phòng Kiều, Lam và Hồng kể chuyện khác nhau qua đồ dùng.
- Sảnh thể hiện sự sang trọng; bếp/kho có vật liệu thô và đồ dùng đúng chức năng.
- Hậu kỳ và hiệu ứng được hoàn thiện sau mô hình, vật liệu và ánh sáng.

## Bảo toàn và đồng bộ khi dựng lại

Kiểm tra Git và giữ toàn bộ thay đổi hiện có. Đặc biệt bảo toàn các nâng cấp mỹ thuật vừa được thực hiện.

Đọc nguồn dựng cảnh liên quan, gồm các phần `LauTuBaKit`, `LauTuBaMeshKit`, `LauTuBaHouse`, `LauTuBaHouse.Rooms`, `LauTuBaHouse.Decor`, `LauTuBaSetup` nếu chúng tồn tại. Đồng bộ thay đổi vào nguồn dựng để Rebuild không làm mất kết quả.

Chỉ dựng lại phạm vi cần thiết. Không chạy rebuild các chương không liên quan, không xóa scene gốc hoặc asset khác. Sử dụng Unity-aware tooling để chỉnh hoặc sinh scene; không sửa YAML scene thủ công cho việc này.

Khi mở rộng map, cập nhật cùng nhau:
- Sàn, tường, mái, cửa và lỗ mở thực tế.
- Nội thất và các vật đặt trên đó theo từng cụm.
- Điểm tương tác, NPC, điểm quan sát, góc chụp và camera.
- Chỗ nấp, vùng nhiệm vụ, checkpoint và các điểm đặt lại sau khi bị bắt.
- Tuyến tuần tra, đường của Lam, âm thanh, vùng mưa, đèn và probe.
- Collider và NavMesh.

Không tự mua asset, đổi Unity, đổi pipeline hoặc thêm dependency ngoài phạm vi cần thiết. Khi thiếu asset quan trọng, nêu rõ và tiếp tục các phần không bị chặn.

## Gameplay trên map mới

- Đo lại thời gian đi giữa các mục tiêu. Map rộng hơn không được chỉ tạo quãng đi bộ rỗng.
- Cân lại tuyến tuần tra và điểm dừng; không tự nhân tầm nhìn/tầm nghe với hệ số mở rộng map.
- Giữ quy tắc chỗ nấp: NPC chỉ biết chính xác khi thấy người chơi đi vào.
- Kiểm tra đủ các đường qua cửa B/C và ngõ sau, cả khi cửa đang đóng.
- Lam có điểm chờ và đường an toàn; mở map không làm lệch điểm đến hoặc kẹt đồng hành.
- Checkpoint khôi phục đúng cửa, đồ vật, đèn, nhiệm vụ và NPC.
- Giữ khoảng nghỉ phù hợp giữa J1 và J2; kiểm tra trường hợp người chơi đi nhanh ra bếp trước khi J2 đủ điều kiện. Nếu J2 là sự kiện bắt buộc, điều chỉnh tiến trình/tuyến và tín hiệu để nó diễn ra tự nhiên. Không âm thầm khóa cửa sau chỉ để bắt người chơi đứng chờ hết đồng hồ.
- Mỗi cú hù có đường phản ứng và không phát lại bất công sau checkpoint.

Thu gọn HUD: bỏ mục tiêu trùng lặp, hướng dẫn điều khiển chỉ hiện khi cần; giữ phụ đề và thông tin nguy hiểm dễ đọc.

## Trình tự và nghiệm thu

1. Đọc đầu vào, kiểm tra project và thống nhất hệ tọa độ/kích thước trong nguồn dựng.
2. Mở rộng cấu trúc map, xác minh kết nối và tuyến NPC.
3. Hoàn thiện góc sân–hành lang và phòng Tú Bà làm chuẩn, rồi áp dụng cho toàn bộ hai scene.
4. Hoàn thiện nội thất còn lại, vật liệu, ánh sáng, âm thanh và hiệu ứng.
5. Chạy thử và sửa lỗi do thay đổi mặt bằng/hình ảnh.
6. Cập nhật tài liệu bàn giao theo kết quả thật.

Chụp ảnh trước/sau ở cùng vị trí tương ứng, cùng FOV và độ phân giải: sân/hiên, Kiều, Tú Bà, sổ sách, bếp và sảnh. Có ảnh lúc Play ở camera người chơi, không chỉ ảnh LookBatch hoặc Scene View.

Kiểm tra luồng Chapter 3 → Chapter 4 → Chapter 5, nhánh Lam và checkpoint. Chơi thử với khả năng phát hiện của người canh được bật; autopilot tắt tầm nhìn chỉ xác nhận một phần luồng truyện.

Đo hiệu năng tại các góc nặng và khi chơi, ghi cấu hình, độ phân giải và cài đặt. Hướng tới 60 fps trên máy mục tiêu đã xác nhận. Không coi chi phí công cụ chụp ảnh là thời gian render thông thường.

Bàn giao ngắn gọn: file/scene đã sửa, ảnh so sánh, kết quả kiểm tra, hiệu năng và phần còn thiếu. Không báo hoàn tất nếu hình ảnh vẫn dùng đồ tạm ở vùng bắt buộc hoặc đường đi còn lỗi. Chủ động thực hiện các bước đã được phép, không dừng sau bản kế hoạch.
