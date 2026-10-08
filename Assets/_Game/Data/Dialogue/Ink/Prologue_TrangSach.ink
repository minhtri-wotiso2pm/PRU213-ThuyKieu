// ============================================================
// MỞ ĐẦU — TRANG SÁCH
// Scene Unity: Chapter00_Prologue (lớp học hiện đại + phòng Kiều và sân Vương gia)
// Mở đầu 1 — Lớp học: nhân vật chính chê quyết định bán mình của Kiều; quyển sách phát sáng, giọng Kiều thách thức.
// Mở đầu 2 — Phòng Kiều: tỉnh dậy trong thân xác Kiều; quan sai bắt Vương Ông và Vương Quan; chiều đổ mưa → Chương 1.
// ============================================================

=== prologue ===
# chapter:0
# scene:LopHoc
# music:classroom
# speaker:Narrator
# camera:Wide_Classroom
Tiết Văn cuối cùng của một buổi chiều tháng Năm. Quạt trần quay chậm, nắng xiên qua khung cửa sổ.

# speaker:CoGiao
# emotion:thoughtful
# camera:CU_CoGiao
Cô giáo: Gia đình gặp nạn, cha và em bị bắt oan, cần bốn trăm lạng bạc. Và Thúy Kiều đã quyết định bán mình chuộc cha.

# speaker:TrangSach
"Duyên hội ngộ, đức cù lao, bên tình bên hiếu bên nào nặng hơn?"

# speaker:CoGiao
Cô giáo: Hôm nay cô muốn nghe các em nghĩ gì. Nếu các em là Kiều, các em có làm như thế không?

# speaker:Ngan
# emotion:amused
# camera:OTS_Kieu_Ngan
Ngân: (thì thầm) Ê, giơ tay đi. Tiết trước mày chê Kiều suốt mà.

# speaker:CoGiao
# emotion:calm_probe
Cô giáo: Em ngồi bàn thứ ba. Em nghĩ sao?

# timer:14
* ["Dễ mà cô. Em sẽ làm tốt hơn Kiều nhiều."]
    ~ pl_che_kieu = 1
    ~ dong_dieu = 1
    # speaker:HocSinh
    # emotion:smug
    Tôi: Dễ mà cô. Ai đến hỏi cưới mà vội vàng trả đủ tiền như thế thì phải nghi chứ. Nếu là em, em sẽ làm tốt hơn Kiều nhiều.
    # speaker:CoGiao
    # emotion:amused
    Cô giáo: Tự tin quá nhỉ. Biết trước kết cục thì ai mà chẳng khôn.
* ["Em nghĩ phải có cách khác, chứ không phải bán chính mình."]
    ~ pl_che_kieu = 2
    ~ dong_dieu = 3
    # speaker:HocSinh
    # emotion:thoughtful
    Tôi: Em nghĩ phải có cách khác. Vay mượn, kêu oan, tìm người làm chứng... chứ không phải đem chính mình ra đổi.
    # speaker:CoGiao
    # emotion:thoughtful
    Cô giáo: "Cách khác" ấy, một cô gái mười lăm tuổi thời đó có được bao nhiêu?
* ["Kiều cam chịu quá. Em không thích nhân vật này."]
    ~ pl_che_kieu = 3
    ~ dong_dieu = 2
    # speaker:HocSinh
    # emotion:cold
    Tôi: Kiều cam chịu quá cô ạ. Người ta bảo gì làm nấy, rồi khóc suốt mười lăm năm. Em không thích nhân vật này.
    # speaker:CoGiao
    # emotion:cold
    Cô giáo: Thích hay không là quyền của em. Nhưng phán xét một người thì phải hiểu hoàn cảnh của họ trước đã.
* [Im lặng, lắc đầu. #timeout]
    ~ pl_che_kieu = 3
    ~ dong_dieu = 2
    # speaker:Narrator
    Tôi không trả lời, chỉ khẽ lắc đầu. Trong đầu vẫn là một câu: "Ngốc thật."
    # speaker:CoGiao
    Cô giáo: Không nói cũng là một câu trả lời. Về nhà các em nghĩ thêm nhé.

- # speaker:Narrator
# sfx:school_bell
# camera:Wide_Classroom
Chuông tan học reo vang. Cả lớp ào ra như ong vỡ tổ.

# speaker:CoGiao
Cô giáo: Bài về nhà: viết một đoạn, nếu em là Kiều. Nhớ là viết bằng tấm lòng, đừng viết bằng cái đầu biết trước kết cục!

# speaker:Narrator
# objective:Thu dọn sách vở trước khi về · E để tương tác
# gameplay:tutorial_classroom
Lớp học vãn dần. Quyển Truyện Kiều vẫn nằm mở trên bàn.

-> pl_lop


=== pl_lop ===
// Hub khám phá trong lớp; lật quyển sách là kết thúc.
* [Đọc mấy câu thơ cô chép trên bảng #explore:pl_board]
    -> pl_board
* [Nói chuyện với Ngân #explore:pl_ngan]
    -> pl_ngan
* [Nhìn ra cửa sổ #explore:pl_window]
    -> pl_window
* [Cất quyển Truyện Kiều vào cặp #explore:pl_book]
    -> pl_book


=== pl_board ===
# speaker:Narrator
# camera:Board
Trên bảng, nét phấn của cô vẫn còn nguyên. Cô chép hai câu đầu Truyện Kiều, cố tình bỏ trống một chữ cho cả lớp điền.

# speaker:NoiTam
Luật cô dạy hồi sáng: chữ thứ sáu của câu tám phải vần với chữ cuối của câu sáu. Điền thử xem.

# speaker:Narrator
# gameplay:lucbat_pl
Đối đáp lục bát: chọn chữ hợp vần và hợp thanh.

# speaker:TrangSach
"Trăm năm trong cõi người ta, chữ tài chữ mệnh khéo là ghét nhau."

# speaker:NoiTam
Chữ mệnh. Người xưa cứ đổ hết cho số phận. Số phận thì làm gì có thật.
-> pl_lop


=== pl_ngan ===
# speaker:Ngan
# emotion:amused
# camera:OTS_Kieu_Ngan
Ngân: Mày ghê thật. Cả lớp chẳng ai dám cãi cô, có mỗi mày.

{
- pl_che_kieu == 1:
    # speaker:Ngan
    Ngân: "Em sẽ làm tốt hơn Kiều nhiều." Nghe như mày từng bị bán rồi ấy.
- pl_che_kieu == 2:
    # speaker:Ngan
    Ngân: Kêu oan với tìm nhân chứng... Mày tưởng thời ấy có công an với luật sư chắc?
- else:
    # speaker:Ngan
    Ngân: Mày ghét Kiều thật đấy à? Tao đọc đoạn bán mình còn muốn khóc.
}

# speaker:HocSinh
# emotion:smug
Tôi: Thì đấy là truyện. Nhân vật làm sai thì mình được quyền chê chứ.

# speaker:Ngan
# emotion:thoughtful
Ngân: Ừ... nhưng cô bảo rồi, mình biết trước kết cục. Kiều thì không.

# speaker:Ngan
# emotion:amused
Ngân: Thôi tao về trước đây. Mai nộp bài nhớ cho tao chép ý với!
-> pl_lop


=== pl_window ===
# speaker:Narrator
# camera:Window
Ngoài cửa sổ, sân trường đã vắng. Một cơn gió thổi lật mấy trang vở trên bàn ai đó.

# speaker:NoiTam
Mười lăm tuổi. Bằng tuổi mình bây giờ. Nếu là mình thì...

# speaker:NoiTam
Thôi. Bài về nhà thì tối làm.
-> pl_lop


=== pl_book ===
# speaker:Narrator
# camera:Book
Tôi đưa tay định gấp quyển sách lại. Trang giấy dừng đúng ở đoạn Kiều bán mình.

# speaker:TrangSach
"Để lời thệ hải minh sơn, làm con trước phải đền ơn sinh thành."

# speaker:Narrator
# gameplay:book_glow
Rồi những dòng chữ bắt đầu sáng lên.

~ nhan(ts_tai_menh)
# speaker:TrangSach
# page:ts_tai_menh
"Trăm năm trong cõi người ta, chữ tài chữ mệnh khéo là ghét nhau."

# speaker:Narrator
Một trang giấy bứt khỏi gáy sách, nhẹ như lông, rơi vào lòng bàn tay tôi. Chữ trên đó vẫn còn sáng. (B: mở Trang Sách)

# speaker:KieuThat
# gameplay:kieu_voice
Ngươi nói... nếu là ngươi, ngươi sẽ làm tốt hơn ta?

# speaker:HocSinh
# emotion:fearful
Tôi: Ai... ai vừa nói đấy?

# speaker:KieuThat
Người mà ngươi gọi là "ngốc". Người đã khóc mười lăm năm trong những trang giấy ngươi lật qua chẳng cần đọc kỹ.

* ["Đúng. Biết trước mọi chuyện, tôi sẽ không để ai lừa."]
    ~ tu_trong += 1
    # speaker:HocSinh
    # emotion:determined
    Tôi: Đúng. Tôi biết Mã Giám Sinh là ai, biết Tú Bà là ai. Tôi sẽ không để ai lừa mình.
    # speaker:KieuThat
    Biết trước một điều chưa phải là thoát được nó.
* ["Tôi... tôi chỉ nói vậy thôi. Tôi đâu có ở trong hoàn cảnh đó."]
    ~ tinh_tao += 1
    # speaker:HocSinh
    # emotion:defensive
    Tôi: Tôi chỉ nói vậy thôi. Tôi đâu có ở trong hoàn cảnh đó.
    # speaker:KieuThat
    Vậy thì hãy ở trong đó một lần.
* [Gấp mạnh quyển sách lại.]
    ~ soul(-1)
    # speaker:Narrator
    Tôi gập mạnh quyển sách. Ánh sáng vẫn lọt qua kẽ giấy, mỗi lúc một chói hơn.
    # speaker:KieuThat
    Gấp sách lại không làm câu chuyện biến mất đâu.

- # speaker:KieuThat
Hãy sống một ngày của ta. Rồi hãy nói cho ta biết... ta đã chọn sai ở đâu.

# speaker:Narrator
# fx:spirit
# sfx:memory
# gameplay:into_room
Ánh sáng nuốt chửng cả lớp học.

-> pl_phong


=== pl_phong ===
# fx:none
# scene:PhongKieu
# music:morning
# speaker:Narrator
# camera:Wide_KieuRoom
Có tiếng chim sẻ. Mùi gỗ cũ và hương trầm.

# speaker:NoiTam
# gameplay:wake_panic
Đây... đây là đâu? Giường gỗ, màn the, rèm tre... Đây không phải phòng mình.

# speaker:NoiTam
Tay mình. Sao tay mình lại thế này? Áo này là áo gì?

# speaker:MeKieu
# emotion:warm
Mẹ Kiều: (gọi vọng từ ngoài sân) Kiều ơi! Dậy chưa con? Ra ăn sáng với cha mẹ nào!

# speaker:NoiTam
"Kiều"?

# speaker:Narrator
# objective:Tìm hiểu căn phòng · đi bằng WASD, E để xem, giữ chuột phải để quan sát
# gameplay:tutorial_room
Căn phòng lặng im, chờ cô bước xuống giường.

-> pl_phong_hub


=== pl_phong_hub ===
* {not pl_guong} [Soi chiếc gương đồng #explore:pl_guong]
    -> pl_guong_k
* {not pl_dan} [Cây hồ cầm dựng bên vách #explore:pl_dan]
    -> pl_dan_k
* {not pl_tho} [Bàn viết bên cửa sổ #explore:pl_tho]
    -> pl_tho_k
* {pl_vat >= 2} [Ra sân #explore:pl_san]
    -> pl_san_k


=== pl_guong_k ===
~ pl_guong = true
~ pl_vat += 1
# speaker:Narrator
# camera:Mirror
Mặt gương đồng mờ mờ. Một gương mặt hiện ra: đôi mày lá liễu, đôi mắt trong như nước mùa thu.

# speaker:NoiTam
# fx:panic
Không phải mặt mình. Mình chớp mắt, cô ấy chớp mắt. Mình đưa tay lên má, cô ấy cũng đưa tay lên.

# speaker:TrangSach
"Làn thu thủy, nét xuân sơn, hoa ghen thua thắm, liễu hờn kém xanh."

# speaker:NoiTam
# fx:none
Thúy Kiều. Mình đang ở trong thân xác Thúy Kiều.
# clue:pl_guong
-> pl_phong_hub


=== pl_dan_k ===
~ pl_dan = true
~ pl_vat += 1
~ soul(1)
# speaker:Narrator
# camera:Dan
Cây hồ cầm nằm nghiêng bên vách, dây đàn còn hơi ấm như ai vừa gảy đêm qua.

# speaker:TrangSach
"Khúc nhà tay lựa nên chương, một thiên Bạc mệnh lại càng não nhân."

# speaker:NoiTam
"Bạc mệnh" — khúc đàn Kiều tự soạn. Hồi đọc mình còn bảo: ai lại tự viết bài hát buồn cho đời mình như thế.

# speaker:NoiTam
Giờ cầm cây đàn trong tay mới thấy... nàng đã linh cảm được điều gì đó.
-> pl_phong_hub


=== pl_tho_k ===
~ pl_tho = true
~ pl_vat += 1
# speaker:Narrator
# camera:Desk
Trên bàn viết là một bức thư gấp làm tư, nét chữ cứng cỏi của một người đàn ông.

# speaker:Narrator
"Chú ta mất ở Liêu Dương, ta phải đi hộ tang nửa năm. Lời thề dưới trăng, ta không quên. — Kim Trọng."

# speaker:NoiTam
Kim Trọng. Người yêu của Kiều. Trong sách, chàng đi hộ tang, đến lúc quay về thì mọi chuyện đã quá muộn.

# speaker:NoiTam
# clue:pl_thu_kim_trong
Chàng đi rồi. Vậy là chuyện đó sắp xảy ra.
-> pl_phong_hub


=== pl_san_k ===
# scene:VuongGia_San
# speaker:Narrator
# camera:Wide_Courtyard
Sân nhà họ Vương sáng nắng. Mâm cơm sáng bày dưới hiên, chén nước chè còn bốc khói.

# speaker:VuongOng
# emotion:satisfied
Vương Ông: Kiều con! Dậy muộn thế? Lại thức khuya gảy đàn phải không?

# speaker:VuongQuan
# emotion:amused
Vương Quan: Chị Kiều lại ngủ quên! Để em ăn hết bánh của chị nhé.

# speaker:MeKieu
# emotion:warm
Mẹ Kiều: Con ngồi xuống đi. Hôm nay cha con được nghỉ, cả nhà mới ăn với nhau một bữa yên lành.

# speaker:NoiTam
Vương Ông. Vương Quan. Mẹ Kiều. Họ cười với mình như thể mình là con gái, là chị của họ thật.

# speaker:NoiTam
Mình nhớ đoạn này. Một ngày yên lành. Rồi...

# speaker:Narrator
# sfx:gong
# music:tension_low
# camera:Gate
Tiếng chiêng dồn dập ngoài cổng. Cánh cổng gỗ bật mở.

# speaker:QuanSai
# emotion:angry
Quan sai: Vương Ông, Vương Quan đâu? Có đơn tố cáo nhà các người chứa hàng gian! Trói cả lại!

# speaker:VuongOng
# emotion:fearful
Vương Ông: Oan cho tôi quá! Nhà tôi làm ăn lương thiện... Hàng gian nào?

# speaker:QuanSai
# emotion:mocking
Quan sai: Thằng bán tơ khai đã gửi tơ lụa ăn cắp ở nhà ngươi. Có oan thì lên huyện mà kêu!

# speaker:NoiTam
Thằng bán tơ. Đúng như trong sách. Một lời vu oan là đủ để cả nhà tan nát.

# timer:9
* [Lao ra chắn trước mặt cha.]
    ~ pl_hanh_dong = 1
    # speaker:Kieu
    # emotion:determined
    Kiều: Dừng tay! Cha tôi không có tội!
    # speaker:QuanSai
    # emotion:angry
    Quan sai: Đàn bà con gái biết gì mà xen vào! Tránh ra!
    # speaker:Narrator
    Hắn gạt mạnh. Cô ngã dúi xuống nền gạch. Đầu gối rát buốt. Cảm giác đau này là thật.
* ["Đơn tố cáo đâu? Ai làm chứng? Cha tôi phạm luật nào?"]
    ~ pl_hanh_dong = 2
    ~ tinh_tao += 1
    # speaker:Kieu
    # emotion:calm_probe
    Kiều: Đơn tố cáo đâu? Ai làm chứng? Cha tôi phạm vào luật nào?
    # speaker:QuanSai
    # emotion:mocking
    Quan sai: Ha! Con gái nhà này lý sự gớm. Đơn ở trên huyện. Muốn đọc thì mang bạc lên mà đọc!
    # speaker:NoiTam
    Ở đây, công lý cũng có giá. Mình phải nhớ điều đó.
* [Đứng yên, ghi nhớ từng gương mặt. #timeout]
    ~ pl_hanh_dong = 3
    # speaker:NoiTam
    Mình không làm gì được bằng tay không. Nhưng mình nhớ được. Tên quan sai sẹo má trái. Cái túi bạc lủng lẳng bên hông hắn.

- # speaker:VuongOng
# emotion:collapse
# gameplay:arrest
Vương Ông: Con ơi... đừng...

# speaker:MeKieu
# emotion:crying
Mẹ Kiều: Ông ơi! Quan lớn ơi, xin tha cho nhà tôi!

# speaker:VuongQuan
# emotion:fearful
Vương Quan: Chị Kiều! Chị Kiều ơi!

# speaker:Narrator
# camera:Gate
Dây trói siết chặt. Hai cha con bị lôi qua cổng. Hàng xóm đứng xa nhìn, không ai dám bước lại gần.

# speaker:Narrator
# scene:VuongGia_Mua
# music:sad_strings
Chiều hôm ấy, trời đổ mưa.

# speaker:MeKieu
# emotion:crying
Mẹ Kiều: Họ đòi bốn trăm lạng... Bốn trăm lạng thì nhà ta lấy đâu ra...

# speaker:KieuThat
# gameplay:kieu_voice
Ngươi đã thấy rồi đấy. Không ai hỏi ta có muốn hay không. Chỉ có một con số: bốn trăm lạng.

# speaker:KieuThat
Đêm nay, sẽ có một người đến gõ cửa nhà này. Ngươi biết hắn là ai.

{
- pl_che_kieu == 1:
    # speaker:NoiTam
    Mã Giám Sinh. Mình đã bảo là dễ. Giờ thì đến lượt mình chứng minh.
- pl_che_kieu == 2:
    # speaker:NoiTam
    Mã Giám Sinh. "Phải có cách khác" — mình đã nói thế. Giờ thì phải tìm ra nó thật.
- else:
    # speaker:NoiTam
    Mã Giám Sinh. Mình đã gọi nàng là cam chịu. Giờ thì đến lượt mình đứng ở đây.
}

# speaker:KieuThat
Hãy cho ta thấy ngươi sẽ làm gì.

# fx:none
-> chapter_1
