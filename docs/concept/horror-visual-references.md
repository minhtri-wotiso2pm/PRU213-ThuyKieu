# Tham khảo hình ảnh game kinh dị cho Lầu Tú Bà

Ngày đối chiếu nguồn: 17/10/2026.

Mục tiêu: nâng cấp Chapter03_LauTuBa và Chapter04_LauTuBaDem, ưu tiên kiến trúc, nội thất, vật liệu và ánh sáng. Các nhận xét về cách áp dụng dưới đây là đề xuất mỹ thuật cho project; không phải tuyên bố về kỹ thuật render nội bộ của các game tham khảo.

Ảnh concept của project: `lau-tu-ba-visual-upgrade.png` trong cùng thư mục.

## 1. Paper Dolls Original

Nguồn chính thức và thư viện hình ảnh:
https://store.steampowered.com/app/1051310/Paper_Dolls_Original/?l=english

Game kinh dị góc nhìn thứ nhất đặt trong một dinh thự cổ Á Đông. Đây là nguồn tham khảo chính cho cấu tạo không gian, cửa và nội thất cổ.

Quan sát khi xem hình và gameplay:
- Độ dày khung cửa, cách chia ô vách gỗ, chân và kết cấu đồ nội thất.
- Cách đồ vật tạo lớp nhìn qua cửa và góc khuất.
- Sự liên hệ giữa kiến trúc, vật trang trí và môi trường kinh dị.

Áp dụng:
- Thay cửa, bàn, tủ dạng khối đơn giản bằng cấu kiện có tỷ lệ, mép và mối nối rõ.
- Giữ kiến trúc thống nhất giữa cột, xà, cửa và lan can.
- Làm phòng sổ sách và kho có chiều sâu bằng tủ, kệ và bình phong có công năng.

Định hướng riêng: lầu Tú Bà đang có người sinh hoạt, vì vậy độ cũ và hư hỏng phải có chọn lọc; tránh biến toàn bộ nơi này thành phế tích.

## 2. Devotion

Nguồn chính thức, trailer và press kit:
https://redcandlegames.com/presskit/sheet.php?p=devotion

Game kinh dị không khí góc nhìn thứ nhất, bối cảnh căn hộ Đài Loan thập niên 1980. Điểm tham khảo chính là cách xây dựng một nơi ở có đời sống và bản sắc văn hóa.

Quan sát:
- Đồ vật cá nhân giúp nhận biết người sống trong phòng.
- Sự khác nhau giữa một nơi quen thuộc và cùng nơi đó khi trở nên bất an.
- Cách vật dụng và chi tiết văn hóa gắn với chuyện của nhân vật.

Áp dụng:
- Phòng Kiều: giấy thơ, trâm, vải mới và thẻ nợ.
- Phòng Lam: thư chưa gửi, đồ dùng được giữ lâu, dấu sửa cửa.
- Phòng Hồng: đồ trang điểm và lựa chọn vải có cá tính.
- Dùng hai trạng thái ngày và đêm để thay đổi cảm nhận về cùng ngôi lầu.

Chuyển nguyên tắc kể chuyện sang đồ vật cổ trang phù hợp bối cảnh, không bê nguyên vật dụng hiện đại hoặc nghi lễ đặc thù vào project.

## 3. Layers of Fear 2023

Trang game:
https://store.steampowered.com/app/1946700/Layers_of_Fear/

Phỏng vấn đội phát triển, có hình ảnh và giải thích về mỹ thuật, ánh sáng:
https://www.unrealengine.com/developer-interviews/layers-of-fear-reimagines-horror-with-unreal-engine-5

Video kỹ thuật chính thức của Bloober Team:
https://www.youtube.com/watch?v=iMkN8whZQmQ

Nguồn tham khảo cho nội thất giàu chi tiết và cách ánh sáng tạo chiều sâu trong kinh dị tâm lý góc nhìn thứ nhất.

Áp dụng:
- Phòng Tú Bà có một điểm nhấn chính là sập và khu ngồi, quanh đó là bàn trà, tủ và đồ dùng phụ.
- Đọc rõ từng loại vật liệu: gỗ, vải, gốm và đồng.
- Tổ chức khung hình qua cột, rèm, khung cửa và điểm sáng ở phía sau.
- Tạo vùng sáng có nguồn rõ và vùng tối vẫn nhìn thấy hình khối.

Chuyển cách tổ chức nội thất và ánh sáng sang đồ gỗ, vải, gốm phù hợp lầu cổ. Project tiếp tục dùng Unity URP; tài liệu về UE5 là tham khảo kết quả hình ảnh, không phải yêu cầu chuyển engine hoặc sao chép tính năng render.

## 4. Incantation

Trang chính thức và hình ảnh do nhà phát hành đăng:
https://store.steampowered.com/app/2328540/Incantation/

Game kinh dị góc nhìn thứ nhất, có khám phá và lén lút trong một ngôi làng. Dùng làm tham khảo phụ cho các góc chuyển tiếp và không gian phục vụ.

Áp dụng:
- Bếp, kho và ngõ sau có vết sử dụng cụ thể, nền và tường thay đổi theo độ ẩm.
- Bố trí các lớp cửa, mái và vật che để tạo chiều sâu cho góc nhìn.
- Chọn dấu bẩn theo nguyên nhân: khói quanh bếp, ẩm chân tường, nước dưới mép mái.

Giữ sự khác biệt giữa khu tiếp khách được chăm chút và khu phục vụ thô hơn; không áp dụng độ bẩn đồng đều lên toàn map.

## Hướng mỹ thuật thống nhất cho project

- Cấu tạo nhà cổ và đường nhìn: tham khảo Paper Dolls Original.
- Đời sống nhân vật và kể chuyện bằng đồ vật: tham khảo Devotion.
- Mức hoàn thiện nội thất và tổ chức ánh sáng: tham khảo Layers of Fear.
- Bếp, kho và ngõ sau: bổ sung quan sát từ Incantation.
- Mái, đồ gỗ, trang trí và vật dụng cụ thể: đối chiếu thêm kiến trúc Việt phù hợp bối cảnh đã chọn trước khi chốt asset.

Ngôi lầu cần có vẻ giàu có và đang vận hành. Cảm giác kinh dị đến từ cách nhìn, vùng bị che, ánh sáng thay đổi và dấu kiểm soát con người. Màu chủ đạo: gỗ nâu đỏ, vữa ấm, vải đỏ trầm, gốm và đồng; ánh sân lạnh tiết chế.

## Bảng áp dụng theo khu vực

| Khu vực | Ưu tiên cải thiện | Tham khảo chính |
| --- | --- | --- |
| Sân và hiên | Cấu kiện mái/cột, tỷ lệ đá, vùng ướt/khô, chiều sâu qua khung cửa | Paper Dolls và ảnh concept |
| Phòng Tú Bà | Sập có cấu tạo, vật liệu vải, bàn trà, tủ, ánh đèn có điểm nhấn | Layers of Fear |
| Phòng Kiều/Lam/Hồng | Vật dụng riêng, bố trí sinh hoạt, dấu vết kể chuyện | Devotion |
| Sổ sách | Kệ/tủ thật, giấy tờ thành nhóm, khu làm việc rõ, chỗ nấp hợp lý | Paper Dolls và Devotion |
| Bếp/kho/ngõ | Vật liệu thô, khói và ẩm có vị trí, bố trí vật dụng đúng chức năng | Incantation |

## Cách dùng tài liệu này khi triển khai

1. Xem hình hoặc trailer từ các nguồn trên, chọn một góc ngoài sân và một góc nội thất làm đối chiếu.
2. Với mỗi góc, ghi cụ thể: cấu tạo mô hình, vật liệu, nguồn sáng, bố trí đồ và đường di chuyển.
3. Hoàn thiện cùng một góc trong project, chụp ở camera người chơi rồi so sánh trước/sau.
4. Chốt chuẩn đó và áp dụng sang các phòng còn lại.
5. Giữ cửa, collider, NavMesh, chỗ nấp và hệ thống phát hiện khớp với thay đổi hình ảnh.

Ưu tiên sửa hình khối và tỷ lệ trước, sau đó vật liệu và ánh sáng; hoàn thiện decal, mưa và hậu kỳ ở bước tiếp theo. Đánh giá chất lượng ở ảnh lúc Play và lúc di chuyển, đồng thời đo hiệu năng. Một góc chụp đẹp chưa đại diện cho toàn map.
