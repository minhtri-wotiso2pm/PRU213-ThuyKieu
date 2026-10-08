// ============================================================
// CHƯƠNG 7 — NGHỊCH MỆNH
// Scene Unity: Chapter07_HuyenDuong
// Chủ đề: Kiều tự đứng ra trước công đường, dùng hồ sơ Chương 6 lật lại đơn kiện "đứa ở trốn lấy trộm bạc".
// Nhịp: Cổng huyện đường (chuẩn bị · đánh trống kêu oan) → Đối chất ba vòng (Tú Bà · Mã Giám Sinh · Tú Bà)
//       → Phán quyết (3 sắc thái) → Cha và em được cứu → Kiều thật lần cuối → Ánh sáng.
// Unity ghi: c7_r1, c7_r2, c7_r3 (điểm từng vòng), c7_diem (tổng), c7_long_tin (lòng tin của quan khi kết thúc), c7_thua_vong,
//            lb_ch7 (lời kết lục bát). Kết: thua kiện (c7_thua_kien) · chật vật · ấm · trọn vẹn; sau đó Kiều Thật:
//            mất mình (đồng điệu ≤ 3) · cảm ơn · Nghịch Mệnh (trọn vẹn + đồng điệu ≥ 7: chọn chia tay hay cùng sống).
// ============================================================

=== chapter_7 ===
# chapter:7
# scene:HuyenDuong_Cong
# music:resolve
# speaker:Narrator
# camera:Wide_YamenGate
Sáng sớm. Sương còn đọng trên mái ngói huyện đường Lâm Tri. Trước cổng, người hiếu kỳ đã tụ lại thành đám.

# speaker:Narrator
Ở góc sân, Mã Giám Sinh khoanh tay đứng cạnh Tú Bà. Mụ vừa thấy Kiều đã nhếch mép cười.

# speaker:TuBa
# emotion:mocking
# camera:CU_TuBa
Tú Bà: Ô kìa, con ở trốn tự dẫn xác đến! Đỡ cho tao phải sai người đi lùng.

# speaker:KimTrong
# emotion:firm
# camera:CU_KimTrong
Kim Trọng: Kiều, đừng đáp lại. Trong kia mới là chỗ để nói.

# speaker:NoiTam
Trong sách, Kiều chưa bao giờ được bước vào một công đường để kể chuyện của mình. Hôm nay, mình sẽ bước vào.

{dinh_menh >= 85:
    ~ nhan(ts_thanh_lau)
    # fx:spirit
    # speaker:TrangSach
    # page:ts_thanh_lau
    "Thanh lâu hai lượt, thanh y hai lần."
    # speaker:Narrator
    Một tên nha dịch chạy từ trong ra, cúi rạp người chào Tú Bà. Mụ cười, vỗ nhẹ lên tay áo hắn như vỗ một con chó quen.
    # fx:none
    # speaker:NoiTam
    Mình đã xé quá nhiều trang. Câu chuyện đang dồn hết sức kéo mình về: quan trên kia có lẽ đã nhận lễ của mụ. Phải lật tẩy chuyện đút lót, không thì không ai tin mình.
}
{dong_dieu >= 7:
    ~ nhan(ts_chu_tam)
    # speaker:KieuThat
    # gameplay:kieu_voice
    Người ta viết một câu ở trang cuối cuốn sách về ta. Ta chưa từng được nghe ai đọc nó cho ta. Hôm nay ta tặng lại ngươi.
    # speaker:TrangSach
    # page:ts_chu_tam
    "Thiện căn ở tại lòng ta, chữ tâm kia mới bằng ba chữ tài."
    # fx:none
    # speaker:NoiTam
    Không phải chữ tài. Là chữ tâm. Mình sẽ nhớ.
}

# speaker:Narrator
# objective:Chuẩn bị trước khi vào công đường · đánh trống kêu oan khi đã sẵn sàng
# gameplay:tutorial_gate
Trống kêu oan treo dưới mái cổng. Đánh trống rồi thì không còn đường lùi.

-> c7_cong


=== c7_cong ===
* {ev_nhan_chung > 0 && not c7_nhan_chung_vung} [Động viên các nhân chứng #explore:c7_nhan_chung]
    -> c7_nhan_chung
* {kt_tin_tuong >= 3 && not c7_ke_hoach_kt} [Bàn kế với Kim Trọng #explore:c7_kim_trong]
    -> c7_kim_trong
* {not c7_biet_hoi_lo} [Để ý Tú Bà đang làm gì ở góc sân #explore:c7_quan_sat]
    -> c7_quan_sat
* [Đánh trống kêu oan #explore:c7_trong]
    -> c7_trong


=== c7_nhan_chung ===
~ c7_nhan_chung_vung = true
~ c7_viec += 1
# speaker:Narrator
# camera:Witnesses
Những người đã hứa làm chứng đứng nép dưới mái hiên, ai cũng xanh mặt khi nghe tiếng Tú Bà.

{nc_co_lam:
    # speaker:ChiLam
    # emotion:fearful
    Chị Lam: Kiều... mụ nhìn chị kìa. Ba năm rồi chị vẫn run khi mụ nhìn như thế.
}
{nc_ba_nuoc:
    # speaker:BaHangNuoc
    # emotion:fearful
    Bà hàng nước: Cô ạ, già này buôn bán ở đất Lâm Tri. Nói xong rồi về, mụ ấy có để yên không?
}
{nc_thu_lai:
    # speaker:ThuLai
    # emotion:cold
    Thư lại: Tôi mang sổ ra rồi. Nhưng cô nên biết: nếu quan không nghe, tôi mất chức trước tiên.
}

# speaker:Kieu
# emotion:calm_probe
Kiều: Không ai phải đứng một mình hôm nay. Mọi người chỉ cần nói đúng những gì mình đã thấy, đã nghe. Phần còn lại để cháu lo.

# speaker:Kieu
Kiều: Mụ ấy mạnh vì ai cũng sợ, ai cũng im. Hôm nay chúng ta không im nữa.

# speaker:NoiTam
# clue:c7_nhan_chung
Họ gật đầu. Một người nắm chặt vạt áo, một người hít một hơi thật sâu. Đủ rồi.
-> c7_cong


=== c7_kim_trong ===
~ c7_ke_hoach_kt = true
~ c7_viec += 1
# speaker:KimTrong
# emotion:thoughtful
# camera:OTS_Kieu_KimTrong
Kim Trọng: Quan huyện Lâm Tri là người khó đoán. Ông ta ghét nhất là bị coi thường trước công đường.

# speaker:KimTrong
Kim Trọng: Nàng chỉ đưa bằng chứng khi đã chắc chắn lời khai sai ở đâu. Đưa bừa một lần, ông ta sẽ nghĩ nàng đang làm trò.

# speaker:Kieu
# emotion:determined
Kiều: Vậy nếu ta lỡ tay?

# speaker:KimTrong
# emotion:determined
Kim Trọng: Thì ta sẽ đứng ra. Nhà họ Kim có chút danh phận, quan sẽ phải nghe ta một lần. Chỉ một lần thôi.

# speaker:NoiTam
# clue:c7_kim_trong
Một lần cứu nguy. Phải để dành cho lúc thật cần.
-> c7_cong


=== c7_quan_sat ===
~ c7_biet_hoi_lo = true
~ c7_viec += 1
# speaker:Narrator
# camera:OTS_Kieu_TuBa
Tú Bà nghiêng người sang một tên nha dịch. Một túi gấm nhỏ đổi tay, nhanh như cái chớp mắt.

# speaker:NoiTam
Đút lót. Ngay trước cổng công đường.

# speaker:NoiTam
# clue:c7_hoi_lo
Mụ "làm ăn lương thiện" thì cần gì phải thế?
-> c7_cong


=== c7_trong ===
# speaker:Narrator
# sfx:drum
# camera:Drum
Kiều cầm dùi, nện ba hồi trống. Tiếng trống vang khắp phố huyện. Đám đông im bặt.

# speaker:NhaDich
# emotion:cold
Nha dịch: Ai đánh trống kêu oan? Vào công đường!

# scene:HuyenDuong_CongDuong
# music:trial
# speaker:Narrator
# camera:Wide_Hall
Trong công đường, hai hàng nha dịch chống gậy đứng nghiêm. Trên án thư sơn son, quan huyện ngồi dưới tấm biển "Minh kính cao huyền".

# speaker:QuanHuyen
# emotion:cold
# camera:CU_QuanHuyen
Quan huyện: Sáng nay đã có đơn của Tú Bà kiện một đứa ở trốn lấy trộm bạc. Giờ chính đứa ở ấy lại đánh trống kêu oan.

# speaker:QuanHuyen
Quan huyện: Hay. Bản quan nghe cả hai bên. Nhưng nói dối trước công đường thì cứ ba mươi trượng mà lĩnh.

# speaker:Narrator
# objective:Đối chất trước công đường · tìm chỗ sai trong lời khai và đưa đúng bằng chứng
# gameplay:tutorial_trial
Lời khai của từng người sẽ hiện ra. Hỏi vặn để họ nói thêm. Thấy chỗ sai thì đưa bằng chứng ra.

# speaker:QuanHuyen
# emotion:cold
Quan huyện: Tú Bà. Khai đi.

# speaker:TuBa
# emotion:warm_fake
# camera:CU_TuBa
# gameplay:trial_1
Tú Bà: Bẩm quan lớn, mụ già này chỉ là người làm ăn, chịu oan ức thì phải kêu thôi ạ.

-> c7_sau_vong_1


=== c7_sau_vong_1 ===
# speaker:QuanHuyen
# emotion:thoughtful
# camera:CU_QuanHuyen
Quan huyện: "Mai mối" mà có giao kèo trả tiền theo đầu người... Mã Giám Sinh, bước lên.

{c7_r1 >= 3:
    # speaker:QuanHuyen
    Quan huyện: Còn Tú Bà, lời của mụ sáng nay đã lệch đến ba chỗ. Bản quan ghi nhớ cả.
}

# speaker:MaGiamSinh
# emotion:smug
# camera:CU_MaGiamSinh
Mã Giám Sinh: Bẩm quan, học trò là giám sinh, đọc sách thánh hiền. Chuyện hôm nay chỉ là đàn bà cãi nhau, xin quan đừng bận lòng.

# speaker:Kieu
# emotion:cold
# camera:CU_Kieu
Kiều: Hôm đến nhà ta, ngươi cũng nói giọng ấy. Hôm nay nói lại một lần nữa đi.

# speaker:MaGiamSinh
# emotion:annoyed
# gameplay:trial_2
Mã Giám Sinh: Được. Ta nói cho cả công đường cùng nghe.

-> c7_sau_vong_2


=== c7_sau_vong_2 ===
# speaker:QuanHuyen
# emotion:angry
# camera:CU_QuanHuyen
Quan huyện: Giao kèo lập từ tháng Giêng, hỏi cưới tháng Tư. Mã Giám Sinh, ngươi đem sách thánh hiền ra mà lừa cả bản quan sao?

# speaker:MaGiamSinh
# emotion:fearful
Mã Giám Sinh: Bẩm... bẩm quan...

# speaker:TuBa
# emotion:rage
# camera:CU_TuBa
Tú Bà: Đồ vô dụng! Quan lớn ơi, nó là con ở của tôi, nợ tôi bạc trắng! Đứa nào dám đứng ra làm chứng cho nó thì liệu hồn!

{ev_nhan_chung > 0 && not c7_nhan_chung_vung && ts_be_dau == 2: -> c7_be_dau ->}

{ev_nhan_chung > 0 && not c7_nhan_chung_vung:
    {
    - nc_co_lam:
        ~ c7_lui = 1
        # speaker:ChiLam
        # emotion:fearful
        Chị Lam: Em... em xin lỗi, Kiều... Chị không dám...
        # speaker:Narrator
        Chị Lam lùi lại sau đám đông, hai tay bịt chặt miệng.
    - nc_ba_nuoc:
        ~ c7_lui = 2
        # speaker:BaHangNuoc
        # emotion:fearful
        Bà hàng nước: Thôi... thôi, già này không biết gì cả...
        # speaker:Narrator
        Bà hàng nước cúi đầu, lặng lẽ đi ra phía cổng.
    - else:
        ~ c7_lui = 3
        # speaker:ThuLai
        # emotion:fearful
        Thư lại: Bẩm quan... sổ sách... để tôi về tra lại cho kỹ.
    }
    # speaker:NoiTam
    Mình đã không đến bên họ trước khi vào đây. Mụ chỉ cần một tiếng quát.
- else:
    {ev_nhan_chung > 0:
        # speaker:Narrator
        Không một ai trong số những người làm chứng lùi lại. Bà hàng nước còn hắng giọng thật to.
        # speaker:NoiTam
        Họ vẫn đứng đó. Lần này mụ quát không ăn thua nữa rồi.
    }
}

# speaker:QuanHuyen
# emotion:cold
Quan huyện: Im! Nợ hay không nợ, bản quan sẽ xét. Tú Bà, khai cho rõ món nợ ấy.

# speaker:TuBa
# emotion:warm_fake
# gameplay:trial_3
Tú Bà: Bẩm quan, nợ thì phải trả, xưa nay vẫn thế ạ.

-> c7_loi_ket


=== c7_be_dau ===
// Tunnel: trang "bể dâu" giữ các nhân chứng đứng vững dù chưa được động viên trước.
# speaker:NoiTam
# timer:8
Họ đang run. Mình chưa kịp đến bên họ trước khi vào đây...

* [Lật trang "Trải qua một cuộc bể dâu...": nhìn thẳng vào từng người làm chứng #page:ts_be_dau]
    ~ dung(ts_be_dau, 10)
    ~ c7_nhan_chung_vung = true
    # speaker:Kieu
    # emotion:determined
    # camera:Witnesses
    Kiều: "Trải qua một cuộc bể dâu, những điều trông thấy mà đau đớn lòng." Những gì các người đã thấy, chỉ các người nói được. Hôm nay nói ra, mai sau không ai phải thấy lại nữa.
    # speaker:NoiTam
    Không ai lùi lại. Bà hàng nước gật đầu, lần thứ nhất trong buổi sáng nay.
* [Im lặng, mặc họ tự quyết. #timeout]
    # speaker:NoiTam
    Mình không có quyền bắt ai can đảm thay mình.
- ->->


=== c7_loi_ket ===
# speaker:QuanHuyen
# emotion:thoughtful
# camera:CU_QuanHuyen
Quan huyện: Lời khai đã đủ. Vương Thúy Kiều, trước khi bản quan tuyên, ngươi còn gì muốn nói?

# speaker:NoiTam
Không phải lý lẽ nữa. Một câu thôi, nói bằng thứ tiếng mà người ở đây hiểu nhất.

# speaker:Kieu
# emotion:determined
# camera:CU_Kieu
# gameplay:lucbat_ch7
Kiều: Bẩm quan...

{
- lb_ch7 >= 3:
    # speaker:QuanHuyen
    # emotion:thoughtful
    Quan huyện: ...Đúng luật, đúng lời. Một đứa con gái mà nói được câu ấy trước công đường. Bản quan nhớ rồi.
- lb_ch7 == 2:
    # speaker:QuanHuyen
    Quan huyện: Lời thì hay. Bản quan ghi nhận.
- else:
    # speaker:QuanHuyen
    # emotion:cold
    Quan huyện: Thơ phú không phải chứng cứ.
}
-> c7_phan_quyet


=== c7_phan_quyet ===
// Điểm phán quyết = lòng tin cuối + 15 Kim Trọng bảo lãnh + 10 nhân chứng vững + 0/10/20 lục bát − 15 mỗi vòng thua.
// Kết: ≥ 90 và hồ sơ ≥ 3 → 3 trọn vẹn · ≥ 60 → 2 ấm · ≥ 35 → 1 chật vật · dưới 35 → 0 thua kiện (nguyên tác).
~ c7_diem_pq = c7_long_tin
{kt_tin_tuong >= 3:
    ~ c7_diem_pq += 15
}
{ev_nhan_chung > 0 && c7_lui == 0:
    ~ c7_diem_pq += 10
}
{
- lb_ch7 >= 3:
    ~ c7_diem_pq += 20
- lb_ch7 == 2:
    ~ c7_diem_pq += 10
}
~ c7_diem_pq -= 15 * c7_thua_vong
{
- c7_diem_pq >= 90 && c6_ho_so >= 3:
    ~ c7_ket = 3
- c7_diem_pq >= 60:
    ~ c7_ket = 2
- c7_diem_pq >= 35:
    ~ c7_ket = 1
- else:
    ~ c7_ket = 0
}

{c7_ket == 0: -> c7_thua_kien}
{c7_ket == 1: -> c7_chat_vat}

# speaker:Narrator
# camera:Wide_Hall
# music:resolve
Công đường im phăng phắc. Quan huyện nhìn chồng giấy trên án thư rất lâu.
-> c7_tuyen_an


=== c7_chat_vat ===
# speaker:QuanHuyen
# emotion:cold
# camera:CU_QuanHuyen
Quan huyện: Giấy tờ thì có, lời thì nhiều... nhưng một đứa con gái bỏ trốn khỏi lầu xanh, lời nói nhẹ lắm.

# speaker:NoiTam
Không đủ. Mình đã đi xa đến thế này mà vẫn không đủ.

{kt_tin_tuong >= 3:
    # speaker:KimTrong
    # emotion:determined
    # camera:CU_KimTrong
    Kim Trọng: Bẩm quan! Học trò Kim Trọng, con nhà Kim viên ngoại huyện bên, xin lấy danh dự cả họ ra bảo lãnh cho lời nàng. Nếu nàng nói sai một chữ, học trò xin chịu tội thay.
- else:
    # speaker:Narrator
    Từ đám đông, một giọng run run cất lên.
    # speaker:BaHangNuoc
    # emotion:determined
    Bà hàng nước: Bẩm quan... già này tận tai nghe chúng nó gọi cô ấy là "hàng". Con gái nhà người ta, chứ có phải con gà con vịt đâu!
}

# speaker:QuanHuyen
# emotion:thoughtful
Quan huyện: ...Có người dám đứng ra chịu tội thay. Vậy thì bản quan phải nghe cho hết.
-> c7_tuyen_an


=== c7_tuyen_an ===
# speaker:QuanHuyen
# emotion:firm
# camera:CU_QuanHuyen
# sfx:gavel
Quan huyện: Bản quan tuyên!

{
- c7_ket == 3:
    # speaker:QuanHuyen
    Quan huyện: Mã Giám Sinh mạo danh giám sinh, giả cưới để bán người, chứng cứ rành rành. Đánh tám mươi trượng, giam chờ phát lưu!
    # speaker:QuanHuyen
    Quan huyện: Tú Bà mua bán người lương thiện, lập sổ nợ khống, đút lót người nhà quan. Lầu xanh Lâm Tri niêm phong. Mọi văn tự, sổ nợ đốt bỏ trước công đường!
    # speaker:QuanHuyen
    Quan huyện: Ba người con gái họ Lý, họ Trần, họ Lưu ghi trong giao kèo, nha môn phải tìm cho ra, trả về quê quán.
- c7_ket == 2:
    # speaker:QuanHuyen
    Quan huyện: Mã Giám Sinh giả cưới để bán người. Đánh sáu mươi trượng, giam lại chờ xét!
    # speaker:QuanHuyen
    Quan huyện: Tú Bà phạt ba trăm lạng, văn tự và sổ nợ của Vương Thúy Kiều hủy bỏ. Từ nay cấm mụ mua người.
- else:
    # speaker:QuanHuyen
    Quan huyện: Hôn ước giữa Mã Giám Sinh và Vương Thúy Kiều là giả, hủy bỏ. Văn tự bán thân không có giá trị.
    # speaker:QuanHuyen
    Quan huyện: Mã Giám Sinh giam lại chờ xét. Tú Bà... nộp phạt một trăm lạng.
    # speaker:NoiTam
    Một trăm lạng. Mụ sẽ trả nó dễ như trả tiền một bữa rượu. Nhưng mình... mình tự do rồi.
}

# speaker:QuanHuyen
Quan huyện: Đơn kiện trộm cắp của Tú Bà, bác bỏ. Vương Thúy Kiều vô tội.

# speaker:TuBa
# emotion:rage
# camera:CU_TuBa
Tú Bà: Quan lớn! Quan lớn ơi, tôi đã biếu...

# speaker:QuanHuyen
# emotion:angry
Quan huyện: Biếu cái gì? Nói tiếp đi, để bản quan thêm tội!

{c7_ket >= 2:
    # speaker:MaGiamSinh
    # emotion:fearful
    # camera:CU_MaGiamSinh
    Mã Giám Sinh: Một... một nữ nhân như ngươi thì làm được gì...
    # speaker:Kieu
    # emotion:determined
    # camera:CU_Kieu
    Kiều: Thúy Kiều ngày trước có thể không biết chuyện gì đang chờ mình.
    # speaker:Kieu
    Kiều: Nhưng ta thì biết.
- else:
    # speaker:MaGiamSinh
    # emotion:cold
    # camera:CU_MaGiamSinh
    Mã Giám Sinh: Một nữ nhân như ngươi thì làm được gì? Ra khỏi đây rồi, ngươi vẫn chỉ là một đứa con gái không nhà.
    # speaker:Kieu
    # emotion:determined
    # camera:CU_Kieu
    Kiều: Thúy Kiều ngày trước có thể không biết chuyện gì đang chờ mình.
    # speaker:Kieu
    Kiều: Nhưng ta thì biết. Và từ hôm nay, ta không đứng một mình.
}

# speaker:Narrator
Thư lại đọc to lời tuyên án. Đám đông ngoài cổng vỡ òa.

{c7_ket == 3:
    # speaker:Narrator
    # sfx:fire
    Trước công đường, một chậu than được mang ra. Từng tờ văn tự, từng trang sổ nợ cong lại trong lửa.
    {nc_co_lam && c7_lui != 1:
        # speaker:ChiLam
        # emotion:crying
        Chị Lam: Trang của chị... trang của chị cũng cháy rồi. Kiều ơi, chị được về nhà rồi.
    }
}

-> c7_doan_tu


=== c7_thua_kien ===
// Kết nguyên tác: thua kiện. Buồn đúng như trang sách, không thêm tia hy vọng.
# music:none
# speaker:QuanHuyen
# emotion:cold
# camera:CU_QuanHuyen
Quan huyện: Giấy tờ thì nhiều, người đứng ra thì ít. Lời một đứa con gái trốn khỏi lầu xanh không đủ để buộc tội người có danh phận.

# speaker:QuanHuyen
# sfx:gavel
Quan huyện: Bản quan tuyên! Mã Giám Sinh vô can. Vương Thúy Kiều phải trả về cho chủ nợ là Tú Bà. Bạc đem theo, nộp lại đủ. Án nhà họ Vương giữ nguyên.

# speaker:NoiTam
Không...

{kt_tin_tuong >= 3:
    # speaker:KimTrong
    # emotion:angry
    # camera:CU_KimTrong
    Kim Trọng: Bẩm quan! Học trò xin lấy danh dự cả họ ra bảo lãnh...
    # speaker:NhaDich
    # emotion:cold
    Nha dịch: Lôi ra!
    # speaker:Narrator
    Hai tên nha dịch xốc nách Kim Trọng lôi ra khỏi công đường. Ngoài cổng, người nhà họ Kim đã chờ sẵn để đưa chàng về quê.
- else:
    # speaker:Narrator
    Kim Trọng đứng chết lặng ở hàng ghế dưới. Chàng mở miệng, rồi lại thôi.
}

# speaker:TuBa
# emotion:warm_fake
# camera:CU_TuBa
Tú Bà: Về nhà thôi con. Mẹ sẽ dạy lại quy củ cho con, từ đầu.

# speaker:MaGiamSinh
# emotion:smug
# camera:CU_MaGiamSinh
Mã Giám Sinh: Ta đã nói rồi. Ra khỏi đây, ngươi vẫn chỉ là một món hàng.

# speaker:Narrator
# camera:Wide_Hall
Đám đông dạt ra hai bên. Không ai lên tiếng. Bà hàng nước cúi gằm mặt. Thư lại quay đi.

# speaker:NoiTam
Mình biết trước tất cả. Mình đã cố đến thế này. Và câu chuyện vẫn đi đúng con đường của nó.

# fx:spirit
# music:spirit
# scene:HuyenDuong_BongToi
# speaker:Narrator
# camera:Spirit
Mọi thứ đứng yên. Trong bóng tối của công đường, chỉ còn hai cái bóng giống hệt nhau.

# speaker:KieuThat
# camera:Spirit
# gameplay:kieu_voice
Giờ thì ngươi đã biết.

# speaker:Kieu
# emotion:crying
# camera:Spirit
Kiều: Ta biết rồi. Ngươi không chọn sai. Là không ai cho ngươi được chọn.

{dong_dieu >= 5:
    # speaker:KieuThat
    # camera:Spirit
    Ngươi đã đứng cạnh ta đến tận đây. Vậy là đủ. Phần còn lại, để ta tự sống nốt.
- else:
    # speaker:KieuThat
    # camera:Spirit
    Ngươi từng bảo, nếu là ngươi thì ngươi sẽ làm tốt hơn. Bây giờ thì về đi. Phần còn lại là của ta.
}

# speaker:TrangSach
"Thanh lâu hai lượt, thanh y hai lần."

# speaker:Narrator
Chiều hôm ấy, cửa lầu xanh khép lại sau lưng Kiều. Tiếng then cài vang lên khô khốc. Mười lăm năm bắt đầu từ tiếng then ấy.

# speaker:TrangSach
"Trải qua một cuộc bể dâu, những điều trông thấy mà đau đớn lòng."

# speaker:Narrator
# sfx:memory
# gameplay:light
Ánh sáng tắt dần, như một trang sách bị gấp lại.

-> epilogue


=== c7_doan_tu ===
# scene:HuyenDuong_DoanTu
# music:family
# speaker:Narrator
# camera:Wide_YamenGate
Ba ngày sau, án oan của nhà họ Vương được xử lại. Thằng bán tơ khai ra kẻ đã thuê nó vu cáo.

# speaker:Narrator
Cánh cửa nhà lao huyện mở ra. Hai bóng người gầy rộc bước ra nắng.

# speaker:VuongQuan
# emotion:crying
# camera:CU_VuongQuan
Vương Quan: Chị Kiều!

# speaker:VuongOng
# emotion:satisfied
# camera:CU_VuongOng
Vương Ông: Kiều... con gái của cha. Người ta kể hết với cha rồi. Con đã không bán mình... con đã tự đứng lên.

# speaker:MeKieu
# emotion:crying
Mẹ Kiều: Con ơi... Mẹ cứ tưởng... mẹ cứ tưởng đã mất con thật rồi.

* [Ôm chầm lấy cha.]
    # speaker:Kieu
    # emotion:crying
    Kiều: Cha... Cha về rồi.
    # speaker:NoiTam
    Mình không phải con gái ông ấy. Vậy mà tim mình đau như thể là thật.
* ["Không phải mình con đâu, cha ạ. Có rất nhiều người đã giúp."]
    # speaker:Kieu
    # emotion:warm
    Kiều: Không phải mình con đâu, cha ạ. Có Kim công tử, có những người dám đứng ra làm chứng...
    {nc_co_lam:
        # speaker:Kieu
        Kiều: ...và chị Lam. Chị ấy chưa từng được ai hỏi tên.
    }
    # speaker:VuongOng
    # emotion:satisfied
    Vương Ông: Vậy thì cả nhà ta nợ họ một ân tình.

- {kt_tin_tuong >= 3:
    # speaker:KimTrong
    # emotion:warm
    # camera:OTS_Kieu_KimTrong
    Kim Trọng: Lần này, ta không đến muộn.
    # speaker:Kieu
    # emotion:warm
    Kiều: Ừ. Lần này, chàng đến đúng lúc.
}

# speaker:NoiTam
Mọi thứ trở nên chậm lại. Tiếng người như vọng về từ rất xa.

-> c7_kieu_that


=== c7_kieu_that ===
# fx:spirit
# music:spirit
# scene:HuyenDuong_AnhSang
# speaker:Narrator
# camera:Spirit
Giữa sân huyện đường, mọi người đứng yên như tượng. Chỉ còn hai cái bóng giống hệt nhau đứng đối diện.

# speaker:KieuThat
# camera:Spirit
# gameplay:kieu_voice
Ngươi đã làm được.

# speaker:Kieu
# emotion:thoughtful
# camera:Spirit
Kiều: Không.

# speaker:Kieu
# camera:Spirit
Kiều: Ta chỉ có những thứ mà ngươi ngày đó không có.

{
- c7_ket == 3:
    # speaker:Kieu
# camera:Spirit
    Kiều: Ta biết trước câu chuyện. Ta có những người dám đứng ra. Ta có một công đường chịu nghe. Ngươi thì chỉ có một mình.
- c7_ket == 2:
    # speaker:Kieu
# camera:Spirit
    Kiều: Ta biết trước câu chuyện. Ta có người tin ta. Còn ngươi, ngày đó, chẳng ai cho ngươi được mở miệng.
- else:
    # speaker:Kieu
# camera:Spirit
    Kiều: Ngay cả khi biết trước, ta vẫn suýt thua. Nếu không có người đứng ra bảo lãnh, ta cũng chỉ là "một đứa con gái không nhà".
}

# speaker:KieuThat
# camera:Spirit
Vậy ngươi còn nghĩ...

# speaker:KieuThat
# camera:Spirit
...ta đã lựa chọn sai không?

* [Không.]
    # speaker:Kieu
# camera:Spirit
    Kiều: Không.
* [Lắc đầu.]
    # speaker:Narrator
    Cô lắc đầu thật chậm.

- # speaker:Kieu
# camera:Spirit
Kiều: Bây giờ ta hiểu rồi.

{
- tra_loi_kieu_that == 1:
    # speaker:KieuThat
# camera:Spirit
    Ở nhà Tú Bà, ngươi đã nhận mình sai. Hôm nay ngươi đã sửa được điều đó.
- tra_loi_kieu_that == 2:
    # speaker:KieuThat
# camera:Spirit
    Ở nhà Tú Bà, ngươi nói ngươi sẽ không bỏ cuộc. Ngươi đã giữ lời.
- tra_loi_kieu_that == 3:
    # speaker:KieuThat
# camera:Spirit
    Ở nhà Tú Bà, ngươi chỉ im lặng. Hôm nay ngươi đã lên tiếng thay cả hai chúng ta.
}

{dong_dieu <= 3: -> c7_mat_minh}
{c7_ket == 3 && dong_dieu >= 7: -> c7_nghich_menh}

# speaker:KieuThat
# camera:Spirit
Cảm ơn ngươi đã sống một ngày của ta... và cho ta thấy một ngày ta chưa từng có.

# speaker:Narrator
# sfx:memory
# gameplay:light
Rồi ánh sáng xuất hiện.

-> epilogue


=== c7_mat_minh ===
// Thắng mà mất mình: đồng điệu quá thấp, Kiều Thật lặng đi mà không cảm ơn.
# speaker:KieuThat
# camera:Spirit
Ngươi thắng rồi. Ngươi giỏi thật, đúng như ngươi nói ở lớp học.

# speaker:KieuThat
# camera:Spirit
Nhưng suốt một ngày dài, ngươi chưa một lần nhìn ta. Ngươi lật từng trang đời ta như lật bài thi.

# speaker:Kieu
# emotion:caught_off_guard
# camera:Spirit
Kiều: Khoan... ta...

# speaker:Narrator
# camera:Spirit
Cái bóng mờ dần trước khi cô kịp nói hết câu. Không một lời từ biệt.

# speaker:NoiTam
Mình đã thắng vụ kiện. Vậy sao trong lòng lại trống rỗng thế này?

# speaker:Narrator
# sfx:memory
# gameplay:light
Ánh sáng đến, lạnh và trắng như một trang giấy chưa viết.

-> epilogue


=== c7_nghich_menh ===
// Kết trọn vẹn nhất: người chơi tự chọn hai linh hồn chia tay hay cùng sống trong một thân.
# speaker:KieuThat
# camera:Spirit
Thân này là của ta. Nhưng một ngày qua... nó cũng đã là của ngươi.

# speaker:KieuThat
# camera:Spirit
Ánh sáng sắp đến đón ngươi về. Ngươi muốn thế nào?

* [Trả lại thân xác cho nàng: "Đây là đời của ngươi. Hãy sống nốt nó."]
    ~ ket_hon = 1
    # speaker:Kieu
    # emotion:warm
    # camera:Spirit
    Kiều: Đây là đời của ngươi. Lần này không ai viết sẵn nữa. Hãy sống nốt nó, theo cách của ngươi.
    # speaker:KieuThat
    # camera:Spirit
    Vậy thì ta sẽ sống. Còn ngươi... đừng quên ta nhé.
    # speaker:Kieu
    # camera:Spirit
    Kiều: Ta sẽ không quên. Ta sẽ đọc lại ngươi, chậm từng chữ.
* [Ở lại cùng nàng: "Hai chúng ta, một thân. Không ai phải đi một mình nữa."]
    ~ ket_hon = 2
    # speaker:Kieu
    # emotion:determined
    # camera:Spirit
    Kiều: Ta không đi đâu cả. Hai chúng ta, một thân. Ngươi có ký ức, ta có những điều ngươi chưa từng được biết. Không ai phải đi một mình nữa.
    # speaker:KieuThat
    # camera:Spirit
    Hai linh hồn trong một thân... Nguyễn Du chưa từng viết trang nào như thế.
    # speaker:Kieu
    # camera:Spirit
    Kiều: Thì mình tự viết.

- # speaker:KieuThat
# camera:Spirit
Cảm ơn ngươi đã sống một ngày của ta... và cho ta thấy một ngày ta chưa từng có.

# speaker:Narrator
# sfx:memory
# gameplay:light
Rồi ánh sáng xuất hiện.

-> epilogue
