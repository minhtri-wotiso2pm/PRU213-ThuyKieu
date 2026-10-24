# Hướng hình ảnh chính do người dùng chọn

Ảnh tham chiếu chính: `lau-tu-ba-primary-vibe.png` trong cùng thư mục. Người dùng cung cấp ảnh này sau khi xem kết quả mở rộng map và xác nhận đây là không khí muốn hướng tới.

## Ưu tiên thiết kế

Ảnh này được ưu tiên về không khí, bảng màu, mật độ đồ vật và cách tổ chức khung hình so với các ảnh AI đã tạo trước đó. Các file kích thước map, quy tắc nhiệm vụ và gameplay vẫn là cơ sở cho bố cục chức năng.

Hãy mở ảnh và đối chiếu trực tiếp khi dựng. Các nguồn game trong `horror-visual-references.md` tiếp tục là tham khảo bổ trợ. Không có yêu cầu xác định ảnh này thuộc game nào.

## Trải nghiệm cần đạt

Một ngôi lầu có hoạt động tiếp khách và vẻ giàu có, nhưng người chơi cảm thấy bị nhìn và bị kiểm soát. Đèn, rèm, đồ đạc và những người đứng ở xa cùng tạo cảm giác bất an. Không gian có các lớp gần, giữa và xa rõ ràng.

## Những yếu tố quan trọng trong ảnh

1. **Đỏ ấm là màu chủ đạo ở khu có người:** đèn lồng vàng cam/đỏ, rèm đỏ trầm, gỗ nâu đỏ. Ánh trời và mưa lạnh tiết chế để tách các lớp xa.
2. **Đồ đạc có mật độ và chức năng:** bàn ghế thành cụm gần hành lang, đồ gốm trên bàn, khu tiếp khách nhìn thấy được từ đường đi. Trục nhìn không chỉ có một tấm thảm giữa sảnh trống.
3. **Rèm chia không gian:** vải rủ tại các khoang cột, khung cửa và khu tiếp khách; tạo khoảng hở để nhìn qua và góc khuất để nghi ngờ.
4. **Ánh sáng có nhịp:** đèn gần rõ chi tiết chất vải, gân và tua; đèn xa nhỏ dần dẫn mắt vào hành lang. Các nguồn đèn phải làm sáng được gỗ và đồ vật lân cận.
5. **Gỗ có vật liệu rõ:** sàn ván có hướng, khe nối và độ bóng biến thiên; cột, xà và lan can vẫn đọc được hình khối trong bóng tối.
6. **Các lớp không khí:** mưa ngoài trời, lớp ẩm/haze nhẹ ở sâu trong cảnh, bóng người thấp tương phản ở xa. Hiệu ứng phải giữ khả năng nhìn đường và nhận biết nguy hiểm.
7. **Con người góp phần tạo không khí:** một vài khách hoặc người làm đứng/ngồi ở vị trí có chủ đích, hoạt động nhẹ. Không cần mỗi người một hệ thống AI đầy đủ; tránh đứng T-pose hoặc lặp animation đồng loạt.
8. **Điểm nhìn người chơi nhập vai:** ảnh có bàn tay cầm đèn lồng ở tiền cảnh. Đây là một yếu tố thiết kế riêng cần cân nhắc sau khi môi trường đạt chuẩn; không thay việc dựng ánh sáng và đồ đạc bằng thêm một model đèn trước camera.

## Phân bổ vào map hiện có

- **Sảnh:** hai bên trục di chuyển có cụm bàn ghế, rèm theo khoang, một vài người; điểm nhìn từ cửa vào phải đọc được chức năng tiếp khách.
- **Hiên sân:** rèm và đèn tạo khung nhìn xuống sân; thêm bàn nhỏ ở khoang đủ rộng, giữ lối đi rõ và không cản cửa phòng.
- **Sân:** vùng trời lạnh và mưa đóng vai trò tương phản với không gian gỗ ấm bên trong. Giữ diện tích sân đã chốt và các đường qua sân.
- **Phòng Tú Bà:** cùng ngôn ngữ vật liệu nhưng nội thất và vải được chăm chút hơn, có điểm nhấn ánh đèn quanh khu ngồi/ngủ.
- **Phòng ở và khu phục vụ:** giảm mức trang trí theo chức năng nhưng giữ chất liệu và chất lượng mô hình đồng nhất.

## Những khác biệt về phạm vi phải phân biệt

Ảnh có ban công/hành lang tầng trên, trong khi mặt bằng 42 × 38 m đã chốt là một tầng. Yếu tố cần nghiên cứu là chiều cao và lớp nhìn đứng. Không tự suy ra rằng người dùng đã yêu cầu một tầng gameplay mới cùng NPC, đường đi và nhiệm vụ mới. Nếu bổ sung tầng trên, xác định rõ nó là kiến trúc nền hay khu chơi trước khi mở rộng phạm vi nhiệm vụ.

Ảnh có mặt sàn bóng và ướt. Trong project cần giải thích nước bằng vị trí hứng mưa và mép hiên; vùng sâu dưới mái có thể dùng gỗ đánh bóng để tạo phản chiếu thay vì làm mọi phòng đều bị mưa ướt.

## Trình tự sửa hình ảnh

1. Chỉnh bố cục nhìn từ sảnh và một góc hiên: rèm, bàn ghế, nguồn sáng và người làm điểm nhấn.
2. Hoàn thiện mô hình các đồ vật gần camera và chuẩn vật liệu gỗ/vải/gốm.
3. Dựng lại ánh sáng, bảo đảm đèn đỏ ấm tạo độ sáng lân cận nhưng vẫn có các vùng tối đọc được.
4. Hoàn thiện mưa, lớp không khí, nhân vật nền và chuyển động nhỏ.
5. Áp dụng chuẩn hình ảnh sang các khu còn lại, kiểm tra gameplay và hiệu năng.

Chụp ảnh ở camera người chơi, cùng góc trước/sau. Đánh giá trực tiếp so với ảnh chính về bốn tiêu chí: màu sắc, mật độ không gian, chất liệu và chiều sâu. Không coi việc tăng số đèn hoặc thêm vài vật trang trí là đã đạt hướng hình ảnh.

Đây là tài liệu định hướng. Việc lưu ảnh và tài liệu này chưa thay đổi scene Unity.
