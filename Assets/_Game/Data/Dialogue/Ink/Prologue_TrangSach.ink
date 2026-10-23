// ============================================================
// MỞ ĐẦU — TRANG SÁCH
// Scene Unity: Chapter00_Prologue (lớp học hiện đại + phòng Kiều và sân Vương gia)
// Mở đầu 1 — Lớp học: nhân vật chính chê quyết định bán mình của Kiều; đọc lại đoạn thơ, trang sách sáng lên.
// Mở đầu 2 — Phòng Kiều: tỉnh dậy trong thân xác Kiều; quan sai bắt Vương Ông và Vương Quan; chiều đổ mưa → Chương 1.
// Kể bằng hình ảnh: lời dẫn chỉ giữ ở chỗ hình ảnh không tự nói được; camera, âm thanh, câu thơ và chuỗi ảnh chớp
// (# montage:<id>, xem MontageCatalog) gánh phần còn lại.
// Giọng Kiều là giọng của trang thơ — dịu, buồn, không đe doạ.
// ============================================================

=== prologue ===
# chapter:0
# scene:LopHoc
# music:classroom
# speaker:ThayGiao
# emotion:thoughtful
# camera:Wide_Classroom
Thầy giáo: Cha và em bị bắt oan, cần bốn trăm lạng. Kiều đã bán mình chuộc cha. Nếu là Kiều, các em có làm thế không?

# speaker:Ngan
# emotion:amused
# camera:OTS_Kieu_Ngan
Ngân: (thì thầm) Ê, giơ tay đi. Tiết trước mày chê Kiều suốt mà.

# speaker:ThayGiao
# emotion:calm_probe
# hands:pick
Thầy giáo: Bàn thứ ba. Em trả lời đi.

# timer:14
* ["Dễ mà thầy. Em sẽ làm tốt hơn Kiều nhiều."]
    ~ pl_che_kieu = 1
    ~ dong_dieu = 1
    # hands:down
    # speaker:HocSinh
    # emotion:smug
    Tôi: Dễ mà thầy. Ai đến hỏi cưới mà vội vàng trả đủ tiền như thế thì phải nghi chứ. Nếu là em, em sẽ làm tốt hơn Kiều nhiều.
    # speaker:ThayGiao
    # emotion:amused
    Thầy giáo: Tự tin quá nhỉ. Biết trước kết cục thì ai mà chẳng khôn.
* ["Em nghĩ phải có cách khác, chứ không phải bán chính mình."]
    ~ pl_che_kieu = 2
    ~ dong_dieu = 3
    # hands:down
    # speaker:HocSinh
    # emotion:thoughtful
    Tôi: Em nghĩ phải có cách khác. Vay mượn, kêu oan, tìm người làm chứng... chứ không phải đem chính mình ra đổi.
    # speaker:ThayGiao
    # emotion:thoughtful
    Thầy giáo: "Cách khác" ấy, một cô gái mười lăm tuổi thời đó có được bao nhiêu?
* ["Kiều cam chịu quá. Em không thích nhân vật này."]
    ~ pl_che_kieu = 3
    ~ dong_dieu = 2
    # hands:down
    # speaker:HocSinh
    # emotion:cold
    Tôi: Kiều cam chịu quá thầy ạ. Người ta bảo gì làm nấy, rồi khóc suốt mười lăm năm. Em không thích nhân vật này.
    # speaker:ThayGiao
    # emotion:cold
    Thầy giáo: Thích hay không là quyền của em. Nhưng phán xét một người thì phải hiểu hoàn cảnh của họ trước đã.
* [Im lặng, lắc đầu. #timeout]
    ~ pl_che_kieu = 3
    ~ dong_dieu = 2
    # hands:down
    # speaker:NoiTam
    Ngốc thật.
    # speaker:ThayGiao
    Thầy giáo: Không nói cũng là một câu trả lời. Về nhà các em nghĩ thêm nhé.

- # speaker:ThayGiao
# sfx:school_bell
# camera:Wide_Classroom
Thầy giáo: Bài về nhà: nếu em là Kiều. Viết bằng tấm lòng, đừng viết bằng cái đầu biết trước kết cục!

# speaker:NoiTam
# objective:Cất quyển Truyện Kiều vào cặp · E để tương tác
# gameplay:tutorial_classroom
Quyển Truyện Kiều vẫn mở trên bàn. Ngân, bảng và cửa sổ là tùy chọn.

-> pl_lop


=== pl_lop ===
// Hub khám phá trong lớp; lật quyển sách là kết thúc.
* [Đọc mấy câu thơ thầy chép trên bảng #explore:pl_board]
    -> pl_board
* [Nói chuyện với Ngân #explore:pl_ngan]
    -> pl_ngan
* [Nhìn ra cửa sổ #explore:pl_window]
    -> pl_window
* [Cất quyển Truyện Kiều vào cặp #explore:pl_book]
    -> pl_book


=== pl_board ===
# speaker:NoiTam
# camera:Board
Thầy cố tình bỏ trống một chữ. Luật lục bát: chữ thứ sáu câu tám phải vần với chữ cuối câu sáu.

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
Ngân: Mày ghê thật. Cả lớp chẳng ai dám cãi thầy, có mỗi mày.

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
Ngân: Ừ... nhưng thầy bảo rồi, mình biết trước kết cục. Kiều thì không. Thôi tao về trước đây!
-> pl_lop


=== pl_window ===
# speaker:NoiTam
# camera:Window
Sân trường vắng rồi. Kiều năm ấy mười lăm tuổi, bằng tuổi mình bây giờ. Nếu là mình thì...

# speaker:NoiTam
Thôi. Bài về nhà thì tối làm.
-> pl_lop


=== pl_book ===
~ nhan(ts_tai_menh)
# speaker:TrangSach
# gameplay:book_glow
# camera:BookClose
# page:ts_tai_menh
"Trăm năm trong cõi người ta, chữ tài chữ mệnh khéo là ghét nhau."

# speaker:Narrator
Trang Sách: câu thơ vừa đọc được giữ lại · B để mở

# speaker:KieuThat
# gameplay:kieu_voice
Em bảo, nếu là em, em sẽ làm tốt hơn ta?

# speaker:HocSinh
# emotion:thoughtful
Tôi: ...Ai vậy?

# speaker:KieuThat
Người em gọi là "ngốc". Mười lăm năm của ta nằm trong những trang em lật qua chẳng cần đọc kỹ.

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
* [Gấp quyển sách lại.]
    ~ soul(-1)
    # speaker:KieuThat
    Gấp sách lại không làm câu chuyện biến mất đâu em.

- # speaker:KieuThat
Hãy sống một ngày của ta. Rồi hãy nói cho ta biết... ta đã chọn sai ở đâu.

# speaker:TrangSach
# fx:page
# sfx:memory
# gameplay:into_room
"Trải qua một cuộc bể dâu, những điều trông thấy mà đau đớn lòng."

-> pl_phong


=== pl_phong ===
# fx:none
# scene:PhongKieu
# music:morning
# speaker:NoiTam
# camera:Wide_KieuRoom
# gameplay:wake_room
Giường gỗ, màn the, rèm tre... Đây không phải phòng mình.

# gameplay:than_xac

# speaker:MeKieu
# emotion:warm
Mẹ Kiều: (gọi vọng từ ngoài sân) Kiều ơi! Dậy chưa con? Ra ăn sáng với cha mẹ nào!

# speaker:NoiTam
# objective:Soi gương để hiểu chuyện gì vừa xảy ra
# gameplay:tutorial_room
"Kiều"?

-> pl_phong_hub


=== pl_phong_hub ===
* {not pl_guong} [Soi chiếc gương đồng #explore:pl_guong]
    -> pl_guong_k
* {pl_guong && not pl_dan} [Cây hồ cầm dựng bên vách #explore:pl_dan]
    -> pl_dan_k
* {pl_guong && not pl_tho} [Bàn viết bên cửa sổ #explore:pl_tho]
    -> pl_tho_k
* {pl_vat >= 2} [Ra sân #explore:pl_san]
    -> pl_san_k


=== pl_guong_k ===
~ pl_guong = true
~ pl_vat += 1
# speaker:TrangSach
# camera:Mirror
"Làn thu thủy, nét xuân sơn, hoa ghen thua thắm, liễu hờn kém xanh."

# speaker:NoiTam
# clue:pl_guong
Không phải mặt mình. Mình chớp mắt, người trong gương cũng chớp mắt. Thúy Kiều.
-> pl_phong_hub


=== pl_dan_k ===
~ pl_dan = true
~ pl_vat += 1
~ soul(1)
# speaker:TrangSach
# camera:Dan
# gameplay:dan_tu_do
"Khúc nhà tay lựa nên chương, một thiên Bạc mệnh lại càng não nhân."

# speaker:NoiTam
Mình chưa từng học đàn. Vậy mà ngón tay này vẫn nhớ đường dây.
-> pl_phong_hub


=== pl_tho_k ===
~ pl_tho = true
~ pl_vat += 1
# speaker:KimTrong
# camera:Desk
(thư) "Chú ta mất ở Liêu Dương, ta phải đi hộ tang nửa năm. Lời thề dưới trăng, ta không quên. — Kim Trọng."

# speaker:NoiTam
# clue:pl_thu_kim_trong
Kim Trọng đi rồi. Trong sách, đến lúc chàng về thì mọi chuyện đã quá muộn.
-> pl_phong_hub


=== pl_san_k ===
# scene:VuongGia_San
# speaker:VuongOng
# emotion:satisfied
# camera:Wide_Courtyard
Vương Ông: Kiều con! Dậy muộn thế? Lại thức khuya gảy đàn phải không?

# speaker:VuongQuan
# emotion:amused
Vương Quan: Chị Kiều lại ngủ quên! Để em ăn hết bánh của chị nhé.

# speaker:NoiTam
Một bữa sáng yên lành. Mình nhớ đoạn này. Rồi...

# speaker:QuanSai
# emotion:angry
# sfx:gong
# music:tension_low
# camera:Gate
Quan sai: Vương Ông, Vương Quan! Có đơn tố nhà ngươi chứa hàng gian. Trói lại!

# speaker:VuongOng
# emotion:fearful
Vương Ông: Oan cho tôi quá! Nhà tôi làm ăn lương thiện... Hàng gian nào?

# speaker:QuanSai
# emotion:mocking
Quan sai: Thằng bán tơ khai đã gửi tơ lụa ăn cắp ở nhà ngươi. Có oan thì lên huyện mà kêu!

# timer:9
* [Lao ra chắn trước mặt cha.]
    ~ pl_hanh_dong = 1
    # speaker:Kieu
    # emotion:determined
    Kiều: Khoan đã! Cha tôi không có tội!
    # speaker:QuanSai
    # emotion:angry
    Quan sai: Đàn bà con gái biết gì mà xen vào! Tránh ra!
    # speaker:NoiTam
    Đầu gối rát buốt trên nền gạch. Cái đau này là thật.
* ["Đơn tố cáo đâu? Ai làm chứng? Cha tôi phạm luật nào?"]
    ~ pl_hanh_dong = 2
    ~ tinh_tao += 1
    # speaker:Kieu
    # emotion:calm_probe
    Kiều: Xin cho xem đơn. Ai làm chứng cho lời buộc tội ấy?
    # speaker:QuanSai
    # emotion:mocking
    Quan sai: Con gái nhà này lý sự gớm. Muốn xem đơn thì mang bạc lên huyện!
    # speaker:NoiTam
    Ở đây, công lý cũng có giá. Mình phải nhớ điều đó.
* [Đứng yên, ghi nhớ từng gương mặt. #timeout]
    ~ pl_hanh_dong = 3
    # speaker:NoiTam
    Mình không làm gì được bằng tay không. Nhưng mình nhớ được. Tên quan sai sẹo má trái. Cái túi bạc lủng lẳng bên hông hắn.

- # speaker:VuongOng
# emotion:collapse
Vương Ông: Con ơi... đừng...

# speaker:MeKieu
# emotion:crying
# gameplay:arrest
Mẹ Kiều: Ông ơi! Quan lớn ơi, xin tha cho nhà tôi!

# speaker:VuongQuan
# emotion:fearful
# camera:Gate
Vương Quan: Chị Kiều! Chị Kiều ơi!

# speaker:MeKieu
# emotion:crying
# scene:VuongGia_Mua
# music:sad_strings
# montage:pl_mua
Mẹ Kiều: Họ đòi bốn trăm lạng... Bốn trăm lạng thì nhà ta lấy đâu ra...

# speaker:KieuThat
# gameplay:kieu_voice
Không ai hỏi ta có muốn hay không. Đêm nay, sẽ có một người đến gõ cửa nhà này. Em biết người ấy là ai.

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

# fx:none
-> chapter_1
