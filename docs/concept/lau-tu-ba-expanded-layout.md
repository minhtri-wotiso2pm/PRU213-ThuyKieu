# Phương án mở rộng Lầu Tú Bà

Trạng thái: thiết kế đề xuất để đối chiếu khi dựng scene; chưa áp dụng vào hai scene Unity.

Ảnh tham khảo: `lau-tu-ba-expanded-map.png` trong cùng thư mục. Ảnh minh họa bố cục và cảm giác không gian; bảng kích thước và kết nối dưới đây là cơ sở dựng chính xác, không đo tỷ lệ trực tiếp từ ảnh AI hoặc sao chép hướng mũi tên.

## Kích thước

- Khu nhà chính hiện tại khoảng 28 × 26 m, chưa tính ngõ ngoài.
- Khu nhà chính đề xuất 42 × 38 m: 1.596 m², khoảng 2,19 lần diện tích cũ.
- Một tầng, vòng hiên có mái quanh sân; giữ mạch truyện và nhóm phòng hiện có.
- Sân giữa 18 × 14 m.
- Hiên hai bên, hiên phía nam và hành lang phục vụ phía bắc rộng thông thủy khoảng 3 m trước khi trừ trang trí; duy trì lối đi hữu dụng sau bố trí cột/đồ vật.
- Kích thước nhân vật, đồ dùng và chiều cao camera giữ tỷ lệ người thật. Mở rộng mặt bằng và khoảng trống, không scale toàn bộ World khiến NPC và nội thất phình to.

## Mặt bằng theo mét

Hệ tọa độ thiết kế: x tăng từ tây sang đông 0–42 m; y trên bản vẽ tăng từ bắc xuống nam 0–38 m. Khi dựng Unity chuyển sang trục x/z thống nhất.

| Khu vực | Khoảng x | Khoảng y | Kích thước |
| --- | --- | --- | --- |
| Bếp | 0–10 | 0–8 | 10 × 8 m |
| Kho vải | 10–18 | 0–8 | 8 × 8 m |
| Phòng Tú Bà | 18–29 | 0–8 | 11 × 8 m |
| Sổ sách | 29–42 | 0–8 | 13 × 8 m, chia bàn làm việc và kho hồ sơ |
| Hành lang phục vụ | 0–42 | 8–11 | Rộng 3 m |
| Phòng Kiều | 0–9 | 11–19,5 | 9 × 8,5 m |
| Phòng Lam | 0–9 | 19,5–28 | 9 × 8,5 m |
| Hiên tây | 9–12 | 11–28 | Rộng 3 m |
| Sân mưa | 12–30 | 11–25 | 18 × 14 m |
| Hiên đông | 30–33 | 11–28 | Rộng 3 m |
| Phòng Hồng | 33–42 | 11–19,5 | 9 × 8,5 m |
| Phòng khách | 33–42 | 19,5–28 | 9 × 8,5 m |
| Hiên nam | 12–30 | 25–28 | Rộng 3 m |
| Sảnh tiếp khách | 0–42 | 28–38 | 42 × 10 m, chia các cụm sinh hoạt |

Các khoảng trên là phân vùng thô; trừ bề dày tường và bố trí kết cấu khi dựng. Phòng rộng cần chia bằng nội thất hoặc bình phong, tránh khoảng trống vô nghĩa và đường nhìn xuyên suốt quá dài.

## Kết nối

- Cổng chính ở giữa cạnh nam sảnh.
- Sảnh nối hiên nam, hiên tây và hiên đông.
- Các phòng bên mở về hiên trong, không mở thẳng ra sân.
- Cả bốn phòng phía bắc mở xuống hành lang phục vụ.
- Bếp nối kho vải bằng cửa trong; bếp có cửa phía tây ra ngõ sau.
- Kho có khe quan sát sang phòng Tú Bà, không phải lối đi.
- Phòng Tú Bà có cửa trong sang sổ sách.
- Sân có bốn bậc xuống, kết nối các hiên; tuyến vòng dưới mái luôn liên thông.
- Cửa chính phòng ở khoảng 1,2–1,5 m; cửa sảnh rộng hơn theo kết cấu. Không dùng chiều rộng cửa quá lớn để bù cho sai tỷ lệ map.

## Chất lượng không gian

- Sân: một cây chính lệch tâm, ghế hoặc chum ở mép; để trống đường đi và góc quan sát.
- Phòng ở: chia góc ngủ, bàn cá nhân và tủ; không tăng kích thước đồ dùng theo kích thước phòng.
- Sổ sách: chia khu làm việc, lưu giấy và góc nấp bằng tủ/bình phong; giữ lối tiếp cận vật chứng rõ.
- Sảnh: hai cụm tiếp khách hai bên và trục vào sân ở giữa; bố trí nhịp cột hợp lý cho nhịp mái.
- Hành lang dài: nhịp cột, rèm và góc ngồi tạo các lớp nhìn, không tạo một hành lang dài hoàn toàn trống.

## Yêu cầu khi áp dụng vào Unity

1. Chỉnh nguồn dựng scene để bản mở rộng tồn tại sau Rebuild; bảo toàn thay đổi mỹ thuật đang có.
2. Cập nhật vị trí cửa, khe mở trong tường, điểm tương tác, chỗ nấp, NPC, điểm nhìn, vùng nhiệm vụ và checkpoint cùng nhau.
3. Giữ các cụm bàn/đồ trên bàn, giường/rèm và vật tương tác gắn đúng nhau khi di chuyển.
4. Dựng lại NavMesh, kiểm tra mọi tuyến chính và đường của Lam bằng NPC có kích thước thực tế.
5. Cân lại tuyến và điểm dừng người canh: tăng quãng đường không đồng nghĩa tăng bán kính nghe/nhìn theo cùng tỷ lệ.
6. Đo nhịp di chuyển mới và thời điểm J1/J2, đặc biệt khả năng rời map trước J2; không kéo dài thời gian bằng việc bắt người chơi đứng chờ.
7. Chỉnh vùng mưa, đèn, probe và culling theo mặt bằng mới; tránh tăng số đèn đổ bóng tỷ lệ với diện tích.
8. Chụp ảnh Play từ phòng Kiều ra sân, hành lang, Tú Bà, sổ sách và sảnh; đo hiệu năng trước và sau.

Không mở rộng sang tầng hai hoặc thêm nhiệm vụ mới trong phương án này. Diện tích bổ sung phục vụ cảm giác rộng, đẹp, sinh hoạt hợp lý và lén lút có lựa chọn.
