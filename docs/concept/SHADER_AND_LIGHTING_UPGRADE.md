# Yêu cầu áp dụng nâng cấp shader và ánh sáng

Hãy triển khai gói nâng cấp dưới đây cho Chapter03_LauTuBa và Chapter04_LauTuBaDem trong project hiện tại. Đọc AGENTS.md, kiểm tra Git và bảo toàn thay đổi đang có.

Mục tiêu hình ảnh theo `docs/concept/lau-tu-ba-primary-vibe.png` và `PRIMARY_VIBE_DIRECTION.md`: gỗ nâu đỏ nhìn rõ vật liệu, đèn đỏ ấm thực sự làm sáng không gian gần, sân lạnh tiết chế, nội thất có chiều sâu và bóng tối vẫn đọc được.

Đây là công việc shader, material và dựng sáng. Không thay bố cục map, cốt truyện hoặc kiến trúc gameplay trong đợt này. Không dùng việc hoàn thành shader để kết luận toàn bộ đồ họa đã đạt chuẩn nếu asset và bố cục vẫn còn thiếu.

## Hiện trạng đã kiểm tra

- Các vật liệu gỗ và nền sân đang dùng `ThuyKieu/WorldTriplanarLit`.
- `StoryArtKit.Surface` đọc texture màu và normal, đặt smoothness bằng một giá trị chung cho material.
- Bộ `dark_wood` và `monastery_stone_floor` đang có màu và normal 1K; chưa có roughness/AO trong hai thư mục này.
- Shader hiện tại gán `inputData.bakedGI = SampleSH(normalWS)` và chưa có luồng đọc lightmap.
- URP và Shader Graph đã có trong PackageCache; phải xác minh lại phiên bản và cấu hình thực tế trước khi triển khai.

## 1. Shader PBR riêng cho lầu Tú Bà

Tạo một shader chuyên dụng có tên rõ, ví dụ `ThuyKieu/LauTuBaTriplanarLit`, để nâng cấp có phạm vi. Tránh sửa shader dùng chung làm thay đổi hình ảnh các chương khác.

- Giữ cách chiếu triplanar theo mét cho tường, nền và kiến trúc thiếu UV phù hợp.
- Đồ nội thất/vải có UV tốt có thể dùng URP Lit, không bắt buộc mọi đối tượng dùng cùng shader triplanar.
- Dựa vào cấu trúc Lit của đúng URP đang cài để hỗ trợ ánh sáng, bóng và phản chiếu.
- Các pass về bóng, độ sâu và normal phải khớp với hình học/normal khi render màu; kiểm tra cả SSAO nếu đang dùng.
- Giữ xử lý fog và khả năng batching/instancing khi phù hợp; không tự tuyên bố tương thích nếu chưa kiểm tra.

Các đầu vào cần có:
- Base Color/Albedo và tint.
- Normal và cường độ.
- Roughness hoặc Smoothness map, có quy ước rõ; roughness đổi sang smoothness bằng 1 − roughness khi cần.
- AO và cường độ.
- Metallic: gỗ, đá, vải thường là phi kim; chỉ phần kim loại dùng giá trị thích hợp.
- Tiling theo mét và tùy chọn biến thiên màu/độ nhám vừa phải.

Có thể đóng gói AO/roughness/metallic để giảm lần đọc texture. Quy định rõ kênh, đặt texture dữ liệu ở chế độ tuyến tính (sRGB tắt), nhập normal đúng loại. Texture mặc định phải cho kết quả hợp lệ nếu chưa có mask.

## 2. Chuẩn hóa bộ texture

Kiểm tra nguồn và CREDITS của các bộ texture đang dùng. Khi có thể, bổ sung roughness/AO từ đúng bộ gốc và nguồn được phép sử dụng. Lưu nguồn cùng asset.

Không gọi texture suy ra từ độ sáng của albedo là roughness đo thực hoặc bộ PBR gốc. Nếu thiếu map, dùng thông số dự phòng và ghi rõ giới hạn.

Ưu tiên độ phân giải theo mật độ texel và khoảng cách camera. Chỉ nâng độ phân giải ở bề mặt cần thiết, không tăng đồng loạt lên 4K.

## 3. Ba nhóm vật liệu cần duyệt

### Gỗ

- Vân gỗ đúng hướng, tint không làm mất hết chi tiết tối.
- Cột và xà nhám hơn gỗ được đánh bóng thường xuyên.
- Mặt bàn/sập có highlight mềm và biến thiên; không giống nhựa.
- Kiểm tra đặc biệt ở cột tiền cảnh và mặt dưới mái trong ảnh hiện tại.

### Đá và gỗ ẩm

- Dùng mask phân vùng khô/ẩm/vũng nước phù hợp vị trí hứng mưa.
- Vùng ướt tối màu vừa phải và phản chiếu rõ hơn; không biến toàn sân thành mặt gương xanh.
- Giảm normal quá gắt; mặt nước đọng cần bề mặt tương đối phẳng hơn đá quanh nó.
- Phân biệt gỗ đánh bóng trong nhà với gỗ thực sự ướt ngoài mép hiên.
- Nếu thêm tính năng mask cho một khu vực nhỏ, giữ chi phí và phạm vi material phù hợp; không bắt mọi material chịu toàn bộ tính năng.

### Đèn lồng và vải

- Chất vải và gân vẫn đọc được khi sáng.
- Phân biệt phần thân phát sáng với nguồn đèn thực chiếu lên môi trường.
- Emission không được xem là tự động tạo ánh sáng gián tiếp realtime.
- Rèm đỏ trầm có nếp và phản ứng ánh sáng phù hợp; tránh bề mặt phẳng màu đỏ.

## 4. Ánh sáng gián tiếp và phản chiếu

Trước khi bake, xác nhận shader thực sự nhận lightmap, dữ liệu UV/lightmap hợp lệ và có các pass/biến thể cần thiết theo URP hiện tại. Không chỉ thêm một dòng lấy GI rồi coi là xong.

- Dùng bake/mixed cho phần kiến trúc và nguồn sáng có trạng thái ổn định khi phù hợp.
- Chapter 3 chuyển chiều sang chạng vạng, các đèn bật/tắt và đèn NPC di chuyển phải có phương án riêng. Không bake ánh sáng bật vào trạng thái mà gameplay cần tắt tối nếu không có xử lý trạng thái tương ứng.
- NPC/đồ động nhận ánh sáng qua probe phù hợp; tránh nhân vật đen giữa phòng sáng.
- Reflection probe theo phòng và sân có phạm vi hợp lý; kiểm tra phản chiếu ngoài trời lọt vào phòng.
- Nguồn sáng ấm làm rõ cột, sàn và nội thất ở gần; ánh sân lạnh giữ vai trò tương phản.
- Cân exposure và vùng tối sau khi vật liệu và nguồn sáng đúng.
- Bảo toàn liên hệ giữa ánh sáng nhìn thấy và hệ thống LightExposure/AI. Ánh sáng bake hoặc fill bổ sung không được làm luật lén lút trở nên khó hiểu.

Nếu pipeline bake hiện tại chưa phù hợp với việc chuyển trạng thái của hai chương, chọn giải pháp hỗn hợp khả thi và nêu rõ; không hy sinh gameplay để có một ảnh tĩnh đẹp.

## 5. Gắn vào material thật và nguồn dựng

- Áp dụng vào material thực sự đang được Renderer trong hai scene sử dụng, không chỉ tạo shader rồi để đó.
- Giữ GUID/reference của material khi có thể, sao lưu hoặc dùng biến thể nếu cần so sánh.
- Giới hạn phạm vi vào material Lầu Tú Bà; rà soát material dùng chung trước khi đổi.
- Cập nhật `LauTuBaKit`, các helper vật liệu liên quan và nguồn dựng để Rebuild không gắn lại shader cũ hoặc ghi đè thông số đã duyệt.
- Không tự đổi toàn bộ `StoryArtKit.Surface` cho tất cả các chương.
- Kiểm tra material dùng chung giữa ngày/đêm để việc rebuild một scene không âm thầm đổi diện mạo scene kia.

## 6. Tiêu chí xác nhận đã áp dụng

1. Shader biên dịch thành công trong Editor và nền tảng đích.
2. Renderer trong cả hai scene tham chiếu đúng material và shader mới/URP Lit đã chọn.
3. Gỗ có chi tiết ở bóng tối, đá không bóng đồng đều, đèn tạo ánh sáng lân cận rõ.
4. Nếu bake: có thể chứng minh material nhận ánh sáng gián tiếp từ lightmap, không chỉ ambient SH.
5. Các trạng thái đèn bật/tắt, chuyển chạng vạng, checkpoint và đèn NPC không bị sai.
6. Chụp trước/sau cùng camera, FOV và exposure để so sánh vật liệu/ánh sáng; ghi rõ thay đổi exposure ở bước cân cuối.
7. Kiểm tra sân, cột hiên, phòng Tú Bà và phòng sổ sách ở Play Mode.
8. Đo chi phí shader và render ở cùng cấu hình/độ phân giải. Không bật mọi tính năng cho mọi material một cách mặc định.

Báo cáo chính xác: shader nào đã gắn, material nào đổi, texture map nào có thật, ánh sáng gián tiếp đang dùng cách nào, kết quả kiểm tra và phần còn thiếu. Triển khai trong phạm vi đã nêu; không dừng ở bản đề xuất.
