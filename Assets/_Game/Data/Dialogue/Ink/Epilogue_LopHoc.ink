// ============================================================
// KẾT — TRỞ LẠI LỚP HỌC
// Scene Unity: Chapter08_Epilogue (lớp học lúc hoàng hôn)
// Nhân vật tỉnh dậy trên bàn học như chưa có gì xảy ra; mở lại đoạn bán mình, không còn chê Kiều;
// dòng chữ "Cảm ơn." hiện ở cuối trang → tựa game → tổng kết hành trình → credits.
// Kể bằng hình ảnh: nắng chiều, quyển sách và câu thơ thay cho lời dẫn.
// ============================================================

=== epilogue ===
# chapter:8
# scene:LopHoc_HoangHon
# music:{c7_ket == 0:bac_menh|epilogue}
# gameplay:wake_class
# fx:none
# speaker:Ngan
# emotion:amused
# camera:OTS_Kieu_Ngan
Ngân: Ê! Dậy! Ngủ gục từ lúc tan học đến giờ luôn hả? Tao quay lại lấy áo khoác mà mày vẫn nằm đây.

# speaker:NoiTam
# camera:Wide_Classroom
Lớp học. Nắng chiều. Đồng hồ mới qua mười lăm phút. Như chưa từng có chuyện gì.

# speaker:HocSinh
# emotion:thoughtful
# camera:OTS_Kieu_Ngan
Tôi: ...Tao ngủ bao lâu rồi?

# speaker:Ngan
# emotion:amused
Ngân: Mười lăm phút chứ mấy. Mặt mày như vừa đi đâu xa lắm về ấy.

# speaker:NoiTam
# objective:Quyển Truyện Kiều vẫn nằm trên bàn · E để mở
# gameplay:tutorial_epilogue
Quyển sách vẫn mở đúng trang cũ.

-> ep_lop


=== ep_lop ===
* [Nhìn ra cửa sổ #explore:ep_window]
    -> ep_window
* [Mở lại quyển Truyện Kiều #explore:ep_book]
    -> ep_book


=== ep_window ===
{
- c7_ket == 0:
    # speaker:NoiTam
    # camera:Window
    Ở trong trang sách kia, giờ này cánh cửa lầu xanh vẫn đóng. Và sẽ còn đóng rất lâu.
- kt_tin_tuong >= 3:
    # speaker:NoiTam
    # camera:Window
    Lần này chàng không đến muộn. Không biết ở trong trang sách kia, giờ này họ đang làm gì.
- else:
    # speaker:NoiTam
    # camera:Window
    Cha, mẹ, Vương Quan, chị Lam... Không biết giờ này họ thế nào.
}
-> ep_lop


=== ep_book ===
# speaker:TrangSach
# camera:Book
# gameplay:open_book
"Duyên hội ngộ, đức cù lao, bên tình bên hiếu bên nào nặng hơn?"

# speaker:NoiTam
Vẫn đoạn ấy. Vài tiếng trước mình còn gọi là "ngốc". Lần này, mình đọc chậm từng chữ.

{
- c7_ket == 3:
    # speaker:NoiTam
    Mình đã có những người dám đứng ra, một công đường chịu nghe, một trang sách biết trước. Kiều thì chỉ có mình nàng và bốn trăm lạng.
- c7_ket == 2:
    # speaker:NoiTam
    Mình đã thắng, nhưng không phải nhờ mình giỏi hơn nàng. Mình chỉ được nhiều hơn nàng vài thứ.
- c7_ket == 0:
    # speaker:NoiTam
    Mình biết trước từng trang, mà vẫn thua. Nàng không ngốc. Chỉ là chưa từng có ai đứng về phía nàng.
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
Tôi: Ừ. Nhưng tao nghĩ lại rồi.

# speaker:HocSinh
# emotion:warm
Tôi: Có những chuyện... chỉ khi ở trong hoàn cảnh đó mới hiểu được.

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
    # speaker:NoiTam
    Có những chuyện kể ra cũng chẳng ai tin.

- # speaker:Ngan
# emotion:amused
Ngân: Thôi về đi, trường sắp đóng cổng rồi.

{
- c7_ket == 0:
    # speaker:TrangSach
    # camera:BookClose
    # music:bac_menh
    "Đau đớn thay phận đàn bà, lời rằng bạc mệnh cũng là lời chung."
    # speaker:NoiTam
    Câu này mình vẫn đọc lướt qua. Lần này mình không lướt qua nữa.
- dong_dieu <= 3:
    # speaker:NoiTam
    # camera:BookClose
    # music:epilogue
    Cuối trang trống trơn. Mình đã thắng thay nàng. Nhưng chưa từng hỏi nàng muốn gì.
- else:
    # speaker:TrangSach
    # camera:BookClose
    # music:epilogue
    # gameplay:thank_you
    "Cảm ơn."
    # speaker:NoiTam
    Một dòng chữ nhỏ ở cuối trang, mực còn như vừa khô.
    {
    - ket_hon == 1:
        # speaker:NoiTam
        Ở đâu đó trong trang sách, nàng đang sống nốt đời mình. Lần này không ai viết sẵn.
    - ket_hon == 2:
        # speaker:NoiTam
        Nếu nàng được thấy lớp học này... nắng chiều ở đây đẹp lắm.
    }
}

# speaker:Narrator
# gameplay:ending
THÚY KIỀU: NGHỊCH MỆNH

-> END
