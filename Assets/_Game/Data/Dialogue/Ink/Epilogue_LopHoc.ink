// ============================================================
// KẾT — TRỞ LẠI LỚP HỌC
// Scene Unity: Chapter08_Epilogue (lớp học lúc hoàng hôn)
// Nhân vật tỉnh dậy trên bàn học như chưa có gì xảy ra; mở lại đoạn bán mình, không còn chê Kiều;
// dòng chữ "Cảm ơn." hiện ở cuối trang → tựa game → tổng kết hành trình → credits.
// ============================================================

=== epilogue ===
# chapter:8
# scene:LopHoc_HoangHon
# music:epilogue
# speaker:Narrator
# camera:Wide_Classroom
# gameplay:wake_class
Tiếng quạt trần. Tiếng ve. Tiếng ai đó gọi tên mình.

# fx:none
# speaker:Ngan
# emotion:amused
# camera:OTS_Kieu_Ngan
Ngân: Ê! Dậy! Ngủ gục từ lúc tan học đến giờ luôn hả? Tao quay lại lấy áo khoác mà mày vẫn nằm đây.

# speaker:NoiTam
Lớp học. Bàn ghế, bảng đen, nắng chiều đỏ ối ngoài cửa sổ. Đồng hồ trên tường chỉ mới qua mười lăm phút.

# speaker:NoiTam
Mọi thứ giống hệt như chưa từng xảy ra.

# speaker:HocSinh
# emotion:thoughtful
Tôi: ...Tao ngủ bao lâu rồi?

# speaker:Ngan
# emotion:amused
Ngân: Mười lăm phút chứ mấy. Mặt mày như vừa đi đâu xa lắm về ấy.

# speaker:Narrator
# objective:Quyển Truyện Kiều vẫn nằm trên bàn · E để mở
# gameplay:tutorial_epilogue
Quyển Truyện Kiều vẫn nằm trước mặt, mở đúng trang cũ.

-> ep_lop


=== ep_lop ===
* [Nhìn ra cửa sổ #explore:ep_window]
    -> ep_window
* [Mở lại quyển Truyện Kiều #explore:ep_book]
    -> ep_book


=== ep_window ===
# speaker:Narrator
# camera:Window
Hoàng hôn trải trên sân trường. Ở đâu đó, có tiếng mưa rất xa, rồi tạnh hẳn.

{kt_tin_tuong >= 3:
    # speaker:NoiTam
    Lần này chàng không đến muộn. Không biết ở trong trang sách kia, giờ này họ đang làm gì.
- else:
    # speaker:NoiTam
    Cha, mẹ, Vương Quan, chị Lam... Không biết giờ này họ thế nào.
}
-> ep_lop


=== ep_book ===
# speaker:Narrator
# camera:Book
Tôi mở lại quyển sách. Vẫn đoạn ấy. Vẫn những câu thơ mà vài tiếng trước tôi đã gọi là "ngốc".

# speaker:TrangSach
# gameplay:open_book
"Duyên hội ngộ, đức cù lao, bên tình bên hiếu bên nào nặng hơn?"

# speaker:NoiTam
Lần này, tôi đọc chậm từng chữ.

{
- c7_ket == 3:
    # speaker:NoiTam
    Mình đã có những người dám đứng ra, một công đường chịu nghe, một trang sách biết trước. Kiều thì chỉ có mình nàng và bốn trăm lạng.
- c7_ket == 2:
    # speaker:NoiTam
    Mình đã thắng, nhưng không phải nhờ mình giỏi hơn nàng. Mình chỉ được nhiều hơn nàng vài thứ.
- else:
    # speaker:NoiTam
    Biết trước mọi chuyện, mình vẫn suýt thua. Còn nàng thì không được biết trước điều gì cả.
}

{
- pl_che_kieu == 1:
    # speaker:Ngan
    # emotion:amused
    # camera:OTS_Kieu_Ngan
    Ngân: Nãy mày nói nếu là Kiều mày sẽ làm tốt hơn mà?
- pl_che_kieu == 2:
    # speaker:Ngan
    # emotion:amused
    # camera:OTS_Kieu_Ngan
    Ngân: Nãy mày bảo nếu là Kiều thì phải có cách khác mà?
- else:
    # speaker:Ngan
    # emotion:amused
    # camera:OTS_Kieu_Ngan
    Ngân: Nãy mày chê Kiều cam chịu lắm mà?
}

# speaker:HocSinh
# emotion:thoughtful
# camera:CU_Kieu
Tôi: Ừ.

# speaker:Narrator
Tôi dừng lại một chút.

# speaker:HocSinh
Tôi: Nhưng tao nghĩ lại rồi.

# speaker:HocSinh
Tôi: Có những chuyện...

# speaker:HocSinh
# emotion:warm
Tôi: ...chỉ khi ở trong hoàn cảnh đó mới hiểu được.

# speaker:Ngan
# emotion:thoughtful
Ngân: ...Mày ngủ một giấc mà nói chuyện như bà cụ non ấy.

* ["Bài về nhà, tao viết xong trong đầu rồi."]
    # speaker:HocSinh
    # emotion:amused
    Tôi: Bài về nhà "nếu em là Kiều", tao viết xong trong đầu rồi. Mai cho mày đọc.
* ["Mai tao kể cho mày nghe. Dài lắm."]
    # speaker:HocSinh
    # emotion:amused
    Tôi: Mai tao kể cho mày nghe. Dài lắm.
* [Mỉm cười, không nói gì.]
    # speaker:Narrator
    Tôi chỉ cười. Có những chuyện kể ra cũng chẳng ai tin.

- # speaker:Ngan
# emotion:amused
Ngân: Thôi về đi, trường sắp đóng cổng rồi.

# speaker:Narrator
# camera:BookClose
# music:spirit
# gameplay:thank_you
Khi tôi gấp sách lại, ở cuối trang có một dòng chữ nhỏ, mực còn như vừa khô.

# speaker:TrangSach
"Cảm ơn."

# speaker:Narrator
# gameplay:ending
THÚY KIỀU: NGHỊCH MỆNH

-> END
