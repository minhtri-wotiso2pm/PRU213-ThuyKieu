# Prompt cho Claude — hoàn thiện kiến trúc và bố trí cảnh Lầu Tú Bà

Hãy thực hiện đợt hoàn thiện hình ảnh tiếp theo cho Chapter 3 và Chapter 4 trong project này. Đọc `AGENTS.md` trước, bảo toàn công việc hiện có. Mục tiêu là hoàn thiện môi trường chơi thật trên toàn map, không tạo một scene demo tách biệt.

## 1. Cơ sở và hướng hình ảnh

Đọc và mở trực tiếp:

- `docs/concept/lau-tu-ba-primary-vibe.png`: ảnh tham chiếu chính do người dùng chọn.
- `docs/concept/PRIMARY_VIBE_DIRECTION.md`: cách áp dụng ảnh vào map và ranh giới phạm vi.
- `docs/concept/lau-tu-ba-expanded-layout.md`: bố cục chức năng của map 42 × 38 m.
- Mục 8–9 của `docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md`: shader, ánh sáng và bộ đồ gỗ mới.
- `Tools/LauTuBa/Views/compare_assets`: xem cả ảnh toàn cảnh và ảnh cận, đặc biệt `CMP_VIEW_C4night_01_Hero.png`.

Đợt trước đã cải thiện hình dáng ghế, bàn, sập, cửa, lan can và đầu cột. Tuy nhiên, ở camera người chơi, mái, tường, xà và những khoảng trống lớn vẫn làm cảnh giống các khối dựng sơ bộ. Không coi việc hoàn thành danh sách model hay tăng số polygon là bằng chứng đạt yêu cầu hình ảnh.

Hướng cần đạt: lầu gỗ nâu đỏ có hoạt động tiếp khách, đèn lồng ấm, rèm trầm, đồ đạc có chức năng, nhiều lớp không gian; đẹp nhưng tạo cảm giác bị theo dõi. Khu sân có mưa và ánh trời lạnh tiết chế. Giữ ngôn ngữ kiến trúc nhất quán với bộ nhà hiện tại; không thêm các biểu tượng trang trí rời rạc chỉ để lấp chỗ trống.

## 2. Ưu tiên một — hình khối kiến trúc chiếm phần lớn khung hình

### Mái và hiên

- Tạo độ dày mái, mép ngói, hàng ngói cuối, đường nóc và các điểm kết thúc hợp lý. Nhìn nghiêng từ hiên phải đọc được tiết diện mái.
- Dùng mô-đun ngói, mesh hoặc kỹ thuật phù hợp khoảng cách; tránh cả mái chỉ là mặt phẳng phủ texture, đồng thời tránh dựng từng viên chi tiết ở mọi vị trí xa.
- Sửa các giao điểm mái, cột và xà; loại bỏ khe hở thấy trời, phần xuyên nhau và đầu khối bị lộ từ camera chơi.
- Dầm có tiết diện, đầu dầm, mộng nối và đường viền phù hợp. Giữ và tích hợp đầu cột/tay đỡ mới, không chồng thêm chi tiết thành cụm rối.

### Tường và cửa

- Tạo chân tường, khung cửa có bề dày và hốc cửa để ánh sáng đọc được chiều sâu.
- Chia mảng tường theo chức năng kiến trúc, điều chỉnh tỉ lệ texture và tránh lặp vân quá rõ. Phân biệt vách gỗ, tường trát và phần chân tiếp đất.
- Bo/vát các cạnh quan trọng trong góc nhìn gần với mức vừa đủ bắt sáng. Không áp một mức bevel cho mọi kích thước.

### Chiều cao và lớp phía trên

- Bổ sung lớp ban công/hành lang phía trên dưới dạng kiến trúc nền ở các mặt nhìn chính để tạo cảm giác ngôi lầu có chiều cao và chiều sâu như ảnh tham chiếu.
- Đợt này tầng trên không phải khu gameplay mới: không thêm nhiệm vụ, cầu thang có thể đi hay tuyến tuần tra tầng hai. Bảo đảm đường vào vùng trang trí được ngăn hợp lý bằng kiến trúc.
- Kiểm tra tải trọng thị giác: cột, dầm, lan can và mái phải nối thành kết cấu có lý, không để tầng trên giống một dải trang trí lơ lửng.
- Giữ sân trung tâm, kích thước map và mạng đường đi đã chốt; không thu sân hoặc hạ mái để giả cảm giác dày đặc.

## 3. Ưu tiên hai — bố trí cảnh theo góc nhìn người chơi

### Sảnh và hiên

- Tổ chức bàn ghế theo cụm tiếp khách, có khoảng đứng/ngồi và đường phục vụ. Bổ sung bàn nhỏ tại những khoang hiên đủ rộng.
- Bố trí rèm có độ rủ và nếp gấp lớn rõ, buộc hoặc vén theo chức năng. Giữ trục đi, cửa và tầm nhận biết nguy hiểm.
- Dùng bình, chén, khay, đèn và vài dấu vết sinh hoạt có chủ đích. Mỗi cụm có vai trò, không rải vật dụng ngẫu nhiên khắp sàn.
- Cải thiện các món cũ còn xuất hiện rõ trong khung hình chính: bình phong, tủ, rương, ghế đẩu hoặc kệ. Ưu tiên theo mức nhìn thấy thực tế thay vì thay đồng loạt mọi món.

### Sân và cây

- Cải thiện cây giữa sân: thân thuôn, nhánh chuyển tiếp mượt, cụm lá có khối và độ xuyên sáng phù hợp. Tránh nhánh gãy góc và lá dạng vài mặt phẳng thưa nhìn rõ từ hiên.
- Giữ đường qua sân và các mốc định hướng. Bố trí cây/chậu/đá theo cụm có khoảng nghỉ thị giác, không lấp kín sân.
- Mưa rơi ở vùng hở; mép hiên có chuyển tiếp ướt–khô. Không để mưa xuyên mái hoặc mọi phòng đều ướt.

### Người trong cảnh

- Ch3: dùng một số khách/người làm có hoạt động nhẹ để thể hiện chức năng của lầu. Tận dụng model và animation phù hợp đang có; tránh T-pose và animation đồng loạt.
- Ch4: điều chỉnh số người và vị trí cho hợp giờ đêm, câu chuyện và tuyến lén lút. Không bê nguyên mật độ người của Ch3 sang Ch4.
- Người nền cần phân biệt được với lính gác/nhân vật nhiệm vụ qua vị trí, hành vi và ánh sáng. Không tự thêm khả năng phát hiện người chơi cho họ.
- Không tạo che chắn giả: vật nhìn như chỗ nấp phải nhất quán với line-of-sight thực tế. Nếu rèm không cản nhìn của AI, bố trí nó ngoài những vị trí người chơi sẽ hiểu là chỗ trốn.

## 4. Ưu tiên ba — vật liệu và ánh sáng phục vụ kiến trúc

- Giữ các sửa lỗi đã kiểm chứng ở đợt trước: albedo/tint gỗ, hướng vân, dữ liệu roughness/AO, tách material ngày/đêm và lantern không tự che ánh sáng của nó.
- Thêm biến thiên có nguyên nhân: cạnh tay vịn bóng do tiếp xúc, bụi ở khe, ẩm ở chân tường/cột ngoài trời. Tránh phủ một lớp bẩn đồng đều lên mọi bề mặt.
- Phân biệt gỗ sơn mài, gỗ kết cấu, ván sàn, vải và đá bằng phản ứng ánh sáng. Giữ hướng vân theo từng cấu kiện.
- Sàn trong nhà có thể bóng do đánh bóng; vùng sân/mép hiên phản sáng do nước. Đừng làm toàn nhà như phủ nhựa hoặc ướt như nhau.
- Dùng reflection probe phù hợp các khu và trạng thái ngày/đêm; kiểm tra hiện tượng phản chiếu xuyên tường hoặc sai cảnh. Thiếu SSR không phải lý do bỏ phản xạ và cũng không phải lý do thêm ngay một gói render mới.
- Đèn lồng tạo vùng sáng ấm lên vật xung quanh, có khoảng sáng/tối dẫn mắt. Không tăng exposure toàn cảnh để che vấn đề vật liệu hay tăng số đèn đổ bóng tùy ý.
- Haze nhẹ để tách lớp xa, không phủ sương dày che model. Giữ khả năng đọc đường, đồ tương tác và lính gác.
- Chưa ưu tiên hoa văn chạm nhỏ hay model bàn tay cầm đèn; chỉ làm nếu các hạng mục kiến trúc, bố cục và vật liệu phía trên đã đạt.

## 5. Cách triển khai và giới hạn

- Kiểm tra Git trước khi sửa; giữ nguyên thay đổi của người dùng và các đợt trước.
- Bắt đầu từ các bộ dựng hiện có trong `Assets/_Game/Scripts/Core/Editor/`, gồm `LauTuBaHouse*`, `LauTuBaKit`, `LauTuBaMeshKit`, `LauTuBaJoinery`, `LauTuBaTextures` và `LauTuBaSetup`. Chỉ đọc thêm phụ thuộc cần thiết.
- Dựng và đánh giá trên scene Ch3/Ch4 thật, sau đó áp dụng nhất quán cho các khu còn lại. Có thể dùng góc sảnh/hiên để kiểm tra nội bộ ban đầu, nhưng phải hoàn thiện phạm vi cả hai chương trước khi bàn giao.
- Dùng công cụ Unity phù hợp cho scene/prefab; không sửa YAML thủ công cho tác vụ thông thường. Tuân thủ các giới hạn thao tác asset trong `AGENTS.md`.
- Không viết lại toàn bộ shader hoặc thay render pipeline chỉ vì cảnh còn thô. Nếu phát hiện lỗi shader cụ thể, chứng minh bằng kiểm tra với cùng input trước khi sửa.
- Không thay cốt truyện, objective, vị trí chìa khóa/hợp đồng, nhánh Lam, checkpoint hoặc tuyến thoát. Không sửa Ch5 trở đi ngoài phần phụ thuộc buộc phải xử lý và báo rõ.
- Giữ collider hiện tại khi hình dáng vẫn khớp; nếu hình học mới thay đổi không gian đi hoặc chỗ nấp, sửa collision/NavMesh tương ứng và kiểm tra lại.
- Không tự xóa `Joinery_*_v1` hay asset cũ trong đợt này. Dọn asset không phải mục tiêu hình ảnh.
- Với tài nguyên bổ sung, xác minh nguồn và quyền sử dụng, ghi rõ tài nguyên đã dùng. Không trình bày mesh/texture sinh bằng code là asset scan thủ công hoặc PBR scan.

## 6. Nghiệm thu bằng hình ảnh thực tế

Chụp baseline trước khi sửa, giữ nguyên vị trí camera, FOV, độ phân giải, trạng thái chương và thời điểm ngày/đêm khi so sánh. Giữ riêng bộ ảnh sau đợt nội thất để không mất mốc đối chiếu.

Bộ tối thiểu cần có:

1. Từ sảnh nhìn qua sân, thấy hiên đối diện và lớp kiến trúc phía trên.
2. Dọc hiên, thấy nhịp đèn/rèm, bàn nhỏ và chiều sâu hành lang.
3. Từ sân nhìn vào sảnh, đọc được chức năng tiếp khách.
4. Phòng Tú Bà, nhìn rõ khu ngồi/ngủ và điểm nhấn ánh sáng.
5. Khu ở/khu phục vụ, chứng minh chất lượng được áp dụng ngoài góc chính.
6. Cận mái–xà–đầu cột và hốc cửa dưới ánh sáng trung tính.
7. Sập chụp từ góc không bị đèn đứng che.

- Các góc chính phải có Ch3 ban ngày/hoàng hôn và Ch4 ban đêm khi những trạng thái đó có trong game.
- Ảnh nghiệm thu phải lấy từ camera chơi thực tế, không dùng ảnh AI hoặc camera điện ảnh để đại diện kết quả runtime. Ảnh trung tính chỉ bổ sung kiểm tra model.
- Lưu ảnh trước/sau và ảnh ghép vào các thư mục mới rõ tên, ví dụ `before_architecture`, `after_architecture`, `compare_architecture` trong `Tools/LauTuBa/Views/`.
- Đánh giá trực tiếp bốn điểm: hình khối kiến trúc, mật độ có chức năng, khả năng đọc vật liệu, lớp gần–giữa–xa. Nêu khoảng cách còn lại so với ảnh tham chiếu, không chỉ liệt kê đã thêm bao nhiêu vật.

## 7. Kiểm tra và bàn giao

- Recompile và sửa lỗi phát sinh. Chạy map checks Ch3/Ch4, stealth probe và autopilot có liên quan; kiểm tra nhánh Lam, checkpoint, tuyến tuần, cửa sau và chuyển chương. Nếu điều kiện chạy không có, ghi rõ chưa xác minh.
- Kiểm tra đi bộ bằng camera người chơi ở cả hai chương: kẹt góc, xuyên mái, rèm che mục tiêu, cột mới chặn nhìn, mưa xuyên nhà và bóng/reflection sai.
- Đo hiệu năng ở cùng độ phân giải/cấu hình với baseline. So sánh frame time, FPS trung bình, 1% low, renderer và chi phí bóng; ghi rõ đo trong Editor hay exe. Không dựng build mới chỉ để hoàn thành tài liệu; tuân thủ điều kiện build trong `AGENTS.md` và quyền đã được người dùng cấp trong phiên làm việc.
- Cập nhật mục tiếp theo trong `docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md`: file đã sửa, ảnh, phép đo, kiểm thử, giới hạn còn lại.
- Bàn giao kèm ảnh toàn cảnh sau cùng và đường dẫn ảnh so sánh. Phân biệt rõ việc đã thực hiện, đã kiểm chứng và chưa kiểm chứng. Không tuyên bố đạt ảnh mẫu nếu góc nhìn thực tế vẫn chưa thể hiện được.

Hãy bắt đầu bằng việc xem ảnh tham chiếu và ảnh hiện tại, xác định ngắn gọn các module bị ảnh hưởng, rồi triển khai trong phạm vi trên. Đừng dừng ở một bản đề xuất hoặc vài chi tiết trang trí nhỏ.
