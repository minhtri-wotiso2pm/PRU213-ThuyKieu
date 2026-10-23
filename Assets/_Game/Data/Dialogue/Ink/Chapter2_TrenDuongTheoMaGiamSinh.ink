// ============================================================
// CHƯƠNG 2 — TRÊN ĐƯỜNG THEO MÃ GIÁM SINH
// Scene Unity: Chapter02_RoadToMaGiamSinh
// Chủ đề: điều tra mà không để lộ mình đã biết. Kết chương Kiều chọn giả vờ ngây thơ.
// Nhịp: Lên đường → Hộ tống (quan sát / hỏi / mẩu giấy) → Nghỉ ở quán nước (nghe lén)
//       → Ký ức → Suy luận → Bỏ chạy hay giả vờ → Bài kiểm tra mặt nạ → Lâm Tri.
// ============================================================

=== chapter_2 ===
# chapter:2
# scene:Road_Departure
# music:road_dusk
# montage:c2_mo
# speaker:MaGiamSinh
# emotion:polite_fake
# camera:CU_MaGiamSinh
Mã Giám Sinh: Đi kịp trước tối, đêm nay sẽ có người lo chỗ nghỉ cho nàng.

# speaker:Narrator
# objective:Theo Mã Giám Sinh · Quan sát và tìm hiểu, nhưng đừng để hắn sinh nghi
# gameplay:tutorial_road
Theo sát hắn. Tụt lại quá xa hay nhìn quá lộ liễu, hắn sẽ sinh nghi.

-> c2_road


=== c2_road ===
// Hub khám phá: mọi lựa chọn đều được chọn bằng hành động trong thế giới 3D.
* {not mm_ban_tay} [Quan sát kỹ Mã Giám Sinh (giữ chuột phải) #explore:c2_observe]
    -> c2_observe
* {not mm_hoi_duong} [Bắt chuyện, hỏi hắn đang đưa mình đi đâu #explore:c2_ask]
    -> c2_ask
* {not mm_mau_giay && not c2_giay_mat} [Nhặt mẩu giấy rơi trên đường #explore:c2_paper]
    -> c2_paper
* {manh_moi >= 2} [Nghỉ chân ở quán nước ven đường #explore:c2_eavesdrop]
    -> c2_eavesdrop


=== c2_observe ===
# speaker:Narrator
# camera:OTS_Kieu_MaGiamSinh
Mã Giám Sinh đếm lại xâu bạc, môi lẩm nhẩm tính toán. Tên tùy tùng ghé tai hắn, cả hai cùng liếc về phía Kiều.

~ mm_ban_tay = true
~ manh_moi += 1
~ tinh_tao += 1

# speaker:Kieu
# emotion:thoughtful
# clue:c2_ban_tay
Kiều: (Hắn không nhìn ta như nhìn một người vợ. Hắn nhìn ta như nhìn một món hàng.)

{ts_ma_mat < 2:
    ~ nhan(ts_ma_mat)
    # speaker:TrangSach
    # page:ts_ma_mat
    "Quá niên trạc ngoại tứ tuần, mày râu nhẵn nhụi áo quần bảnh bao."
}
-> c2_road


=== c2_ask ===
# speaker:MaGiamSinh
# emotion:polite_fake
# camera:OTS_Kieu_MaGiamSinh
Mã Giám Sinh: Nàng có điều gì muốn hỏi ta sao?

# speaker:NoiTam
# timer:9
Hỏi thế nào để hắn trả lời thật, mà không biết mình đang dò?

* [Hỏi khéo: "Quê nhà công tử ở đâu? Phụ mẫu có dễ tính không?"]
    # speaker:Kieu
    # emotion:calm_probe
    Kiều: Quê nhà công tử ở đâu? Thiếp về làm dâu, sợ phụ mẫu bên ấy chê vụng.

    # speaker:MaGiamSinh
    # emotion:caught_off_guard
    Mã Giám Sinh: Phụ mẫu? À... Ở Lâm Tri. Nhà ta... ở đó có người lo liệu cả.

    # speaker:NoiTam
    # clue:c2_lam_tri
    Lúc dạm hỏi hắn nói Lâm Thanh. Giờ lại là Lâm Tri. Hắn đang nói dối.
    ~ tinh_tao += 1

* [Hỏi thẳng: "Rốt cuộc công tử đưa ta đi đâu?"]
    ~ nghi_ngo = MIN(nghi_ngo + 25, 100)
    # speaker:Kieu
    # emotion:firm
    Kiều: Rốt cuộc công tử đưa ta đi đâu?

    # speaker:MaGiamSinh
    # emotion:irritated_hidden
    # sfx:suspicion_up
    Mã Giám Sinh: Nàng hỏi lạ thật. Đã là vợ chồng, ta đưa đi đâu thì nàng theo đó. Lâm Tri, được chưa?

    # speaker:NoiTam
    # clue:c2_lam_tri
    Hỏng rồi, hắn bắt đầu để ý. Lần sau phải mềm mỏng hơn.

* [Giả vờ than mệt: "Đường xa quá, thiếp mỏi chân rồi..." #timeout]
    ~ nghi_ngo = MAX(nghi_ngo - 10, 0)
    # speaker:Kieu
    # emotion:soft_firm
    Kiều: Đường xa quá, thiếp mỏi chân rồi... Còn bao lâu nữa hả công tử?

    # speaker:MaGiamSinh
    # emotion:satisfied
    Mã Giám Sinh: Gắng chút. Tới Lâm Tri sẽ có người lo cho nàng từ miếng ăn tới manh áo... à, gia nhân nhà ta.

    # speaker:NoiTam
    # clue:c2_lam_tri
    Lại là "người lo cho nàng". Chắc chắn không phải gia đình hắn.

* {ts_ma_mat == 2} [Lật trang "Quá niên trạc ngoại tứ tuần...": khen hắn chững chạc, hỏi chuyện làm ăn #page:ts_ma_mat]
    ~ dung(ts_ma_mat, 10)
    ~ nghi_ngo = MAX(nghi_ngo - 15, 0)
    ~ gia_vo += 1
    ~ fate(-5)
    # speaker:Kieu
    # emotion:calm_probe
    Kiều: Công tử ngoài bốn mươi mà phong thái thế này, chắc buôn bán khắp nơi. Ở Lâm Tri công tử làm ăn lớn lắm nhỉ?
    # speaker:MaGiamSinh
    # emotion:smug
    # clue:c2_lam_tri
    Mã Giám Sinh: Nàng tinh mắt đấy. Lâm Tri có một nhà quen, mối nào ta đưa về cũng được giá. Ha... à, ý ta là hàng lụa.

- (g1)
~ mm_hoi_duong = true
~ manh_moi += 1
-> c2_road


=== c2_paper ===
# speaker:Narrator
# camera:OTS_Kieu_Paper
# gameplay:document_paper
Một mẩu giấy gấp tư rơi khỏi tay áo Mã Giám Sinh. Kiều nhặt lên, mở thật nhanh.

# speaker:NoiTam
# clue:c2_mau_giay
"Bà T. ở Lâm Tri nhận hàng. Giá bốn trăm năm mươi lạng." Hắn định bán lại mình?

~ mm_mau_giay = true
~ manh_moi += 1

# speaker:MaGiamSinh
# emotion:suspicious
# camera:CU_MaGiamSinh
Mã Giám Sinh: Nàng nhặt được gì đấy?

# speaker:NoiTam
# timer:9
Nhanh lên. Trả hay giữ?

* [Đưa lại, giả vờ chưa đọc: "Hình như công tử đánh rơi." #timeout]
    ~ nghi_ngo = MAX(nghi_ngo - 5, 0)
    # speaker:Kieu
    # emotion:calm
    Kiều: Hình như công tử đánh rơi. Chữ nhỏ quá, thiếp chẳng đọc được.

    # speaker:MaGiamSinh
    # emotion:polite_fake
    Mã Giám Sinh: Giấy nợ vặt thôi. Nàng đưa đây.

* [Lén giấu vào tay áo: "Chỉ là lá khô thôi."]
    ~ nghi_ngo = MIN(nghi_ngo + 15, 100)
    ~ giu_mau_giay = true
    # speaker:Kieu
    # emotion:controlled
    Kiều: Chỉ là lá khô thôi, công tử.

    # speaker:MaGiamSinh
    # emotion:suspicious
    # sfx:suspicion_up
    Mã Giám Sinh: ...Thế à.

    # speaker:Narrator
    Hắn sờ tay áo mình, nhíu mày, rồi im lặng bước tiếp.

- (g2)
-> c2_road


=== c2_eavesdrop ===
# scene:Road_RestStop
# speaker:Narrator
# camera:Wide_TeaHouse
# gameplay:eavesdrop_c2
Mã Giám Sinh kéo tên tùy tùng ra sau vách tre. Khom người (giữ Ctrl), lẻn sát vách để nghe.

# speaker:TuyTung
# camera:CU_Kieu_Listens
Tùy tùng: Công tử, bên Lâm Tri cho người nhắn: phòng đã dọn, chỉ chờ giao người là trao bạc.

# speaker:MaGiamSinh
# emotion:businesslike
Mã Giám Sinh: Tú Bà đã chuẩn bị chưa?

# speaker:TuyTung
Tùy tùng: Dạ, bà ấy hỏi con bé có biết đàn biết thơ thật không, kẻo trả hớ.

# speaker:MaGiamSinh
# emotion:smug
Mã Giám Sinh: Đàn hay, thơ giỏi, mặt hoa. Bốn trăm năm mươi là còn rẻ. Nhớ giữ mồm, con bé này sáng dạ lắm.

~ mm_nghe_len = true
~ manh_moi += 1

# speaker:NoiTam
# clue:c2_tu_ba
# music:none
# gameplay:memory_flash
Tú Bà? Khoan... cái tên này... trong Truyện Kiều...

~ nhan(ts_co_ke)
# speaker:TrangSach
# page:ts_co_ke
"Cò kè bớt một thêm hai, giờ lâu ngã giá vàng ngoài bốn trăm."

# speaker:Kieu
# emotion:realization
# camera:Kieu_Profile
Kiều: (Mã Giám Sinh không cưới Kiều. Hắn đang bán mình.)

{c2_nghe_duong >= 3:
    # speaker:NoiTam
    "Chuyến này lãi được năm mươi lạng." Mình đã nghe chính miệng họ nói, ngay trên đường.
}

# speaker:NoiTam
Ghép lại từng mảnh... hay tin vào câu thơ vừa hiện lên?

* {ts_co_ke == 2} [Lật trang "Cò kè bớt một thêm hai...": cái giá đã có sẵn trong sách #page:ts_co_ke]
    ~ dung(ts_co_ke, 10)
    ~ suy_luan = 3
    # speaker:NoiTam
    # gameplay:c2_page_deduction
    Bốn trăm lạng mua về, bán lại cho Tú Bà có lãi. Không cần ghép, câu thơ đã nói hết rồi.
* [Tự ghép các manh mối.]
    # speaker:Narrator
    # gameplay:deduction_c2
    Ghép các manh mối đã thu thập để xác nhận sự thật.

- (g_suy)
{
- suy_luan >= 3:
    # speaker:NoiTam
    Mọi mảnh ghép đều khớp: Lâm Tri, giá bán, Tú Bà.
- c2_sai_nguoi_nhan:
    # speaker:NoiTam
    Người đang chờ mình ở Lâm Tri... mình nghĩ mình đã đoán ra. Mong là mình đúng.
- else:
    # speaker:NoiTam
    Chưa chắc từng mảnh ghép... nhưng cái tên Tú Bà thì không thể nhầm được.
}
-> c2_decision


=== c2_decision ===
# speaker:NoiTam
# fx:tension
# music:tension_low
Bây giờ... phải làm gì?

* [Bỏ chạy ngay bây giờ, khi trời còn tối.]
    # speaker:NoiTam
    # gameplay:c2_flee
    Chạy. Ngay bây giờ, khi họ còn ở sau vách.
    {
    - c2_bo_chay == 1:
        # speaker:MaGiamSinh
        # emotion:cold
        Mã Giám Sinh: Đêm hôm nàng định đi đâu? Đường này lắm cướp. Ta chỉ lo cho nàng thôi.

        # speaker:NoiTam
        Bàn tay tên tùy tùng còn hằn trên cổ tay. Mình không chạy thoát, mà nếu thoát thì cha cũng không thoát.
    - c2_bo_chay == 2:
        # speaker:NoiTam
        Cha... Mình chạy được một đêm, cha phải trả cả đời. Tờ khế vẫn nằm trong tay hắn.
    - else:
        # speaker:NoiTam
        Mình tự dừng lại. Chạy thì dễ. Ở lại mà vẫn tỉnh táo mới là việc khó.
    }
    # speaker:NoiTam
    Không chạy nữa. Giả vờ như chưa biết gì, cho đến khi tìm được đường ra không khiến cha phải trả giá.
    -> c2_mask

* [Giả vờ như chưa biết gì.]
    # speaker:NoiTam
    Giả vờ chưa biết gì. Diễn cho tròn vai, cho đến khi tìm được đường ra — một đường ra không khiến cha phải trả giá.
    -> c2_mask


=== c2_mask ===
# fx:none
# scene:Road_MaskTest
# speaker:MaGiamSinh
# emotion:suspicious
# camera:CU_MaGiamSinh
Mã Giám Sinh: Nàng làm gì sau vách tre thế?

# speaker:NoiTam
# timer:9
Thở đều. Mỉm cười.

* [Mỉm cười: "Thiếp đi tìm chỗ rửa mặt cho tỉnh." #timeout]
    ~ gia_vo += 1
    ~ fate(-5)
    ~ nghi_ngo = MAX(nghi_ngo - 10, 0)
    # speaker:Kieu
    # emotion:calm
    Kiều: Thiếp đi tìm chỗ rửa mặt cho tỉnh. Đường bụi quá.
* {ts_tai_menh == 2} [Lật trang "Trăm năm trong cõi người ta...": cười buồn như người đã cam phận #page:ts_tai_menh]
    ~ dung(ts_tai_menh, 5)
    ~ gia_vo += 1
    ~ fate(-5)
    ~ nghi_ngo = MAX(nghi_ngo - 25, 0)
    # speaker:Kieu
    # emotion:sad_controlled
    Kiều: Thiếp đứng nghe gió thôi. Đời con gái, chữ mệnh đã định, thiếp còn dám nghĩ gì xa.
    # speaker:MaGiamSinh
    # emotion:satisfied
    Mã Giám Sinh: Biết phận thế là tốt.
* [Lắp bắp: "Thiếp... thiếp không nghe thấy gì cả!"]
    ~ nghi_ngo = MIN(nghi_ngo + 20, 100)
    # speaker:Kieu
    # emotion:pained
    Kiều: Thiếp... thiếp không nghe thấy gì cả!
    # speaker:MaGiamSinh
    # emotion:irritated_hidden
    Mã Giám Sinh: Ta có hỏi nàng nghe thấy gì đâu?
* [Hỏi vặn: "Công tử với người kia bàn chuyện gì mà phải giấu?"]
    ~ nghi_ngo = MIN(nghi_ngo + 30, 100)
    # speaker:Kieu
    # emotion:cold
    Kiều: Công tử với người kia bàn chuyện gì mà phải giấu thiếp?
    # speaker:MaGiamSinh
    # emotion:annoyed
    Mã Giám Sinh: Chuyện đàn ông. Đàn bà đừng xen vào.

- (g3)
# speaker:MaGiamSinh
# emotion:polite_fake
Mã Giám Sinh: Đêm nay là tới Lâm Tri. Nàng có mong không?

# speaker:NoiTam
# timer:9
Câu này là thử mình.

* [Cúi đầu e thẹn: "Phận gái theo chồng, chàng đi đâu thiếp theo đó." #timeout]
    ~ gia_vo += 1
    ~ fate(-5)
    ~ nghi_ngo = MAX(nghi_ngo - 10, 0)
    # speaker:Kieu
    # emotion:soft_firm
    Kiều: Phận gái theo chồng, chàng đi đâu thiếp theo đó.
* [Thở dài: "Thiếp chỉ mong cha được bình an."]
    ~ gia_vo += 1
    ~ fate(-5)
    # speaker:Kieu
    # emotion:sad_controlled
    Kiều: Thiếp chỉ mong cha được bình an. Còn lại... thiếp không dám mong gì.
* [Lạnh nhạt: "Mong hay không thì có khác gì đâu."]
    ~ nghi_ngo = MIN(nghi_ngo + 15, 100)
    # speaker:Kieu
    # emotion:cold
    Kiều: Mong hay không thì có khác gì đâu.

- (g5)
{
    - gia_vo >= 2:
        ~ nghi_ngo = MAX(nghi_ngo - 15, 0)
        # speaker:NoiTam
        Hắn đã tin vẻ cam chịu này. Ít nhất từ giờ đến Lâm Tri, mình còn khoảng trống để quan sát.
    - gia_vo <= 0:
        ~ nghi_ngo = MIN(nghi_ngo + 15, 100)
        # speaker:NoiTam
        Hắn không tin. Tên tùy tùng đã đi sát hơn một bước — đến Lâm Tri, chúng sẽ canh mình kỹ.
    - else:
        # speaker:NoiTam
        Hắn chưa tin hẳn, nhưng cũng chưa dám hỏi thêm.
}
{
    - nghi_ngo >= 60:
        # speaker:MaGiamSinh
        # emotion:suspicious
        Mã Giám Sinh: (nói với tùy tùng) Đêm nay canh nó cho kỹ. Ta không thích ánh mắt ấy.
    - else:
        # speaker:MaGiamSinh
        # emotion:smug
        Mã Giám Sinh: (nói với tùy tùng) Dễ bảo hơn ta tưởng. Đỡ tốn công.
  }

-> c2_ending


=== c2_ending ===
# scene:Road_LamTriGate
# music:road_night
# montage:c2_lam_tri
# speaker:NoiTam
Cổng thành Lâm Tri. Phía trong, đèn lồng đỏ treo kín hiên một ngôi nhà lớn.

// Những kết luận sai ở bảng suy luận lộ ra ngay trước cổng, không cần ai nói.
{c2_sai_noi_den:
    # speaker:NoiTam
    Lâm Tri, không phải Lâm Thanh. Mình đã tin đúng câu hắn nói lúc dạm hỏi.
}
{c2_sai_bac:
    # speaker:NoiTam
    Không có chủ nợ nào ra đón. Chỉ có người đứng chờ đếm bạc. Mình đã đoán sai cái giá của chính mình.
}
{c2_sai_nguoi_nhan:
    # speaker:NoiTam
    Chẳng có quan lớn nào, cũng chẳng có mẹ già nào chờ ở đây. Chỉ có một mụ đàn bà phấn son đứng tựa cửa.
}

{
- c2_sai_noi_den || c2_sai_bac || c2_sai_nguoi_nhan:
    # speaker:Kieu
    # emotion:determined
    # camera:CU_Kieu
    Kiều: Ta tưởng mình đã biết hết. Hóa ra vẫn đoán sai. Từ giờ, nhìn kỹ hơn, và giả vờ như chẳng biết gì.
- else:
    # speaker:Kieu
    # emotion:determined
    # camera:CU_Kieu
    Kiều: Kiều ngày trước bước vào đây mà không biết gì. Còn ta biết, và ta sẽ giả vờ như không biết, cho đến khi đủ sức lật ngược ván cờ này.
}

-> chapter_3_lau
