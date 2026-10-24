# Prompt cho Claude — hoàn thiện bề mặt và ánh sáng Ch3–Ch4

Hãy triển khai đợt hoàn thiện hình ảnh tiếp theo trên Chapter 3 và Chapter 4 hiện tại. Đọc `AGENTS.md`, kiểm tra Git và giữ các thay đổi đang có. Đây là công việc trên scene chơi thật của cả hai chương.

## Hướng đã chốt

Giữ hình khối hai tầng trang trí, mái, mặt bằng 42 × 38 m và bộ đồ gỗ đã dựng. Tập trung theo thứ tự: **vân gỗ → nước/mưa → ánh sáng và chiều sâu → vết sử dụng**. Không mở thêm tầng gameplay hay nhiệm vụ.

Mở trực tiếp `docs/concept/lau-tu-ba-primary-vibe.png` và đọc `PRIMARY_VIBE_DIRECTION.md`. Đọc mục 8–10 trong `docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md` để biết kết quả hiện có. Các yêu cầu shader cũ trong `SHADER_AND_LIGHTING_UPGRADE.md` chỉ là lịch sử tham khảo: không lặp lại những thay đổi đã hoàn thành hoặc coi nhận định ban đầu là hiện trạng.

Ảnh baseline cần xem trong `Tools/LauTuBa/Views/after_architecture/`:

- `VIEW_C3dusk_27_AcrossYard.png`: vân xoáy chéo rất nổi ở cột tiền cảnh, vũng nước dạng mảng xám.
- `VIEW_C4night_14_WestVeranda.png`: mưa vạch dài trắng, cửa sổ sáng khá phẳng, hành lang thiếu lớp không khí ở xa.
- `VIEW_C3dusk_26_YardToHall.png`: đọc được hai tầng nhưng sàn nước và vật liệu còn đồng đều.
- `VIEW_C4night_11_Hall.png`: nhiều phần gỗ chìm vào tối, các vùng sáng tập trung mạnh trên trần và tường sau.

Đây là nhận xét từ ảnh, chưa phải kết luận lỗi code. Xác minh nguyên nhân trước khi sửa.

## 1. Vân gỗ và phản ứng vật liệu

- Kiểm tra albedo gốc, tint, tiling theo mét, normal, roughness và cách định hướng texture trên một cột và một xà đại diện.
- Tách kiểm tra từng thành phần dưới ánh sáng trung tính để biết dải xoáy đến từ texture, UV/projection, biến thiên procedural hay tổ hợp các yếu tố. Không mặc định shader sai.
- Vân thân cột chạy chủ yếu dọc thân; vân xà chạy theo chiều dài. Giảm tương phản và chỉnh mật độ để đọc là gỗ kết cấu từ khoảng cách chơi, không như hoa văn quấn quanh cột.
- Giữ biến thiên tự nhiên vừa phải, không làm gỗ thành màu phẳng hoặc nâng tint tới mức mất sắc nâu đỏ.
- Giữ những sửa lỗi đã có về ARM, hướng vân, tách material Ch3/Ch4 và URP Lit cho đồ có UV phù hợp. Kiểm tra đường dữ liệu UV3/TEXCOORD thực tế trong mesh và shader nếu liên quan, không đổi kênh chỉ dựa trên tên gọi.
- Nếu sửa shader dùng chung, giới hạn thay đổi bằng tính năng tùy chọn với mặc định giữ hành vi cũ; kiểm tra các chương dùng chung. Ưu tiên sửa material/mesh riêng của lầu khi đủ giải quyết vấn đề.

## 2. Vũng nước, sàn ẩm và mưa

- Kiểm tra `Puddle` trong `LauTuBaHouse.Decor.cs`: hiện tạo các mesh blob với một material chung. Xác định vì sao ảnh cho ra mảng xám phẳng trước khi chọn cách sửa.
- Tạo chuyển tiếp đá khô → đá ẩm → nước đọng, mép vũng không sắc như tấm giấy đặt trên sàn. Giữ độ nổi của đá bên ngoài; vùng nước đọng phẳng hơn và có phản ứng ánh sáng phù hợp.
- Kiểm tra lệch cao độ, z-fighting, sorting, alpha và reflection. Chọn giải pháp hợp pipeline hiện có; không bắt buộc dùng transparency nếu giải pháp opaque/mask cho kết quả ổn định hơn.
- Sàn trong nhà phản sáng do đánh bóng; sàn ngoài mép hiên phản sáng do nước. Không làm mọi phòng ướt hoặc toàn sân thành gương.
- Kiểm tra reflection probe, smoothness và hướng nguồn sáng trước khi kết luận thiếu SSR là nguyên nhân. Probe có giới hạn và không tự tạo phản chiếu đèn chính xác theo vị trí; ghi rõ mức đạt được. Không tự thêm render package hay nâng pipeline. Nếu cần giải pháp phản chiếu bổ sung, đo chi phí và tuân thủ quyền thay đổi của project.
- Giảm cảm giác mưa là các sợi trắng đứng đều: kiểm tra kích thước, độ dài, alpha, vận tốc, hướng và mật độ theo góc camera. Dùng biến thiên có kiểm soát; mưa xa nhẹ hơn, mưa gần không che đường nhìn.
- Giọt ở mép mái và splash chỉ xuất hiện ở vùng phù hợp. Giữ đúng trạng thái Ch3 sau mưa và Ch4 đang mưa; không tự bật mưa lớn cho Ch3 chỉ để ảnh đẹp.
- Kiểm tra khi đứng yên và khi xoay/di chuyển camera; không chỉ đánh giá một ảnh tĩnh. Giữ mưa ngoài mái che.

## 3. Ánh sáng và chiều sâu

- Giữ đèn đỏ cam, gỗ nâu đỏ và trời lạnh tiết chế theo ảnh tham chiếu.
- Cửa sổ giấy cần biến thiên sáng và cảm giác vật liệu; tránh tất cả cửa sáng thành các ô màu đặc giống nhau. Không cần dựng phòng tầng trên có thể vào.
- Điều chỉnh tương quan nguồn sáng, emission, phản xạ và môi trường để cột, xà, mặt bàn đọc được hình khối. Không chỉ tăng exposure/ambient toàn cảnh.
- Haze nhẹ tăng dần theo khoảng cách để tách hiên gần, sân và tầng đối diện. Duy trì độ rõ của lính gác, cửa, đồ nhiệm vụ và đường thoát.
- Giữ cơ chế bật/tắt đèn và sự nhất quán giữa hình ảnh với `LightExposure`. Thay đổi ánh sáng hoặc vùng mái che phải được kiểm tra tác động tới stealth.
- Không áp color grade hay sửa material dùng chung khiến Ch5 trở đi đổi hình ảnh. Không thêm đèn đổ bóng theo thói quen; chứng minh nhu cầu và đo chi phí nếu cần.

## 4. Vết sử dụng có nguyên nhân

- Tay vịn/cạnh bàn bóng hơn ở vùng hay chạm, bụi ở khe và mặt khuất, ẩm ở chân cột ngoài trời và mép hiên.
- Áp dụng theo chức năng và vị trí; không phủ noise/bẩn đồng đều lên mọi món.
- Ưu tiên các bề mặt gần camera và góc nhìn chính, sau đó áp nhất quán ở khu ở và khu phục vụ. Giữ sạch hơn ở những vùng được chăm sóc trong phòng Tú Bà.
- Đợt này chạm khắc li ti, nhân vật mới và bàn tay cầm đèn có mức ưu tiên thấp hơn bốn phần đã chốt.

## 5. Kiểm tra hiệu năng và gameplay

- Lưu baseline trước khi sửa, cùng máy/cấu hình/độ phân giải/camera/tuyến chạy. Phân biệt Editor với exe.
- Báo cáo cả lần đầu chạy sau build và các lần chạy lại: thời gian tải, hitch lớn nhất, FPS trung bình, 1% low và thời gian khung hình trong gameplay. Giữ riêng số liệu có tải cảnh và số liệu gameplay ổn định; không loại mẫu chậm chỉ để làm đẹp kết quả.
- Mục 10 ghi nhận Ch3 lần đầu 109 FPS trung bình, 1% low 40; các lần sau 115–118 và 59–62. Điều tra khựng đầu phiên và chi phí bóng ban ngày bằng profiler/đo đối chứng. Hiện chưa có bằng chứng đủ để kết luận tất cả tụt FPS là bóng nắng hoặc tải nền.
- Lần Ch4 80 FPS phải được giữ trong báo cáo kèm điều kiện đo. Nếu không xác định được nguyên nhân, ghi là bất thường chưa rõ, thay vì xác nhận là nhiễu máy.
- Nếu thử giảm shadow trên cấu kiện tầng trên, so sánh cùng góc trước/sau, giữ bóng mái quan trọng và kiểm tra stealth; không tắt hàng loạt cột/lan can mà bỏ qua tác động hình ảnh.
- Mục tiêu là trải nghiệm ổn định quanh 60 FPS trên máy kiểm tra, không chỉ FPS trung bình cao. Báo rõ các đoạn còn dưới mục tiêu.
- Chạy compile, map checks, stealth probe, các autopilot liên quan và chuỗi chương theo phạm vi ảnh hưởng. Thử tuyến qua sân Ch4 ở cả vùng bóng mái và vùng sáng, vì các probe cũ chưa chứng minh toàn tuyến giữ nguyên độ khó.
- Thực hiện một lượt điều khiển thủ công nếu công cụ cho phép. Nếu chưa làm được, báo riêng và cung cấp tuyến thử ngắn cho người dùng: phòng Kiều → hiên → băng sân → phòng Tú Bà → nhánh Lam → bếp/cửa sau, gồm đứng/cúi và khôi phục checkpoint. Không gọi autopilot là kiểm tra thủ công.
- Tuân thủ điều kiện build và các giới hạn scene/prefab/asset trong `AGENTS.md`; không tự xóa asset, thay package hoặc viết scene YAML thủ công.

## 6. Nghiệm thu và bàn giao

- Lưu ảnh vào bộ mới `before_finish`, `after_finish`, `compare_finish` dưới `Tools/LauTuBa/Views/`, giữ bộ architecture làm mốc.
- Ảnh trước/sau cùng camera, FOV, độ phân giải, thời điểm và trạng thái game. Các góc chính: AcrossYard, WestVeranda, YardToHall, Hall, phòng Tú Bà, khu phục vụ; thêm cận cột/vũng nước với ánh sáng trung tính.
- Chụp lúc Play bằng camera người chơi; không dùng ảnh AI hoặc ảnh chỉnh hậu kỳ làm kết quả game. Ghi lại chuyển động mưa và phản xạ bằng clip ngắn nếu có công cụ phù hợp.
- Đạt yêu cầu khi: vân gỗ đọc đúng tỉ lệ/hướng, nước không thành mảng xám cắt dán, mưa không thành rèm vạch trắng, cửa sổ bớt phẳng, lớp xa tách được và gameplay vẫn đọc rõ.
- Cập nhật mục tiếp theo trong `CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md`: thay đổi, nguyên nhân đã xác minh, ảnh, số liệu đo, kiểm thử và giới hạn.
- Bàn giao ảnh toàn cảnh cùng ảnh cận chứng minh từng vấn đề đã sửa; nói rõ chưa đạt ở đâu. Không tiếp tục mở rộng kiến trúc để thay cho hoàn thiện bề mặt.

Hãy xem ảnh baseline, xác định ngắn gọn nguồn gây vấn đề rồi triển khai đủ phạm vi trên. Hoàn thiện cả hai chương, giữ cốt truyện và nhiệm vụ hiện tại.
