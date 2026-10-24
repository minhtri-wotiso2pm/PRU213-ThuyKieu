// ============================================================
// CHƯƠNG 4 — KHÔNG CÒN ĐƯỜNG LUI (bản góc nhìn thứ nhất)
// Scene Unity: Chapter04_LauTuBaDem · thiết kế: docs/codex/CHAPTER3_4_LAU_TU_BA_DESIGN.md §5–8
// Nhịp: Tú Bà báo ngày mai tiếp khách (sau song cửa) → nâng then bằng trâm → kho vải, nhìn Tú Bà ngủ →
//       gỡ chùm chìa → phòng sổ sách: giao kèo, văn tự, bạc (J1) → đường về (J2) → chị Lam (tuỳ chọn)
//       → cửa sau → Chương 5.
// Unity (Chapter04LauWorld) ghi: c4_buoc (tiến trình), c4_quan_sat, c4_chia_khoa, c4_bi_bat; chọn các #explore.
// Lựa chọn #explore:c4_wait không bao giờ được Unity chọn: nó chỉ giữ hub mở khi thế giới chưa cho làm gì.
// ============================================================

=== chapter_4_dem ===
# chapter:4
# scene:Dem_Opening
# music:tension_low
# speaker:Narrator
Đêm. Mưa đổ trên mái ngói. Từ phía phòng khách vọng sang tiếng đàn nguyệt và tiếng cười của khách.

# speaker:Narrator
Một ngọn đèn dừng lại sau song cửa phòng Kiều. Mùi trầm, mùi rượu.

# speaker:TuBa
# emotion:warm_fake
Tú Bà: Con gái ngoan của ta còn thức à? Đến giờ học quy củ rồi.

# speaker:TuBa
# emotion:cold
Tú Bà: Lầu này có ba điều. Một, khách là trời. Hai, nợ là nợ. Ba... kẻ nào trèo tường thì đừng mong lành lặn.

{c3_khe_uoc:
    # speaker:TuBa
    # emotion:suspicious
    Tú Bà: Hộp sơn son bên phòng sổ sách bị xê dịch. Con có biết ai động vào không?
}

# speaker:NoiTam
# timer:9
Mụ đang thử mình. Trả lời thế nào?

* {ts_tu_ba == 2} [Lật trang "Thoắt trông nàng đã biết tình...": cúi đúng kiểu mụ muốn thấy, rồi hỏi khéo về nợ #page:ts_tu_ba]
    ~ dung(ts_tu_ba, 10)
    ~ c4_canh_giac = 0
    ~ c4_biet_no = true
    # speaker:Kieu
    # emotion:controlled
    Kiều: Dạ, con biết thân con rồi, chim vào lồng còn bay đi đâu được. Chỉ xin mẹ cho con biết con nợ bao nhiêu, để con gắng làm mà trả.
    # speaker:TuBa
    # emotion:satisfied
    Tú Bà: Ngoan. Bốn trăm năm mươi lạng tiền mua, cộng áo, phấn, cơm. Sổ nằm trong ngăn bàn, ta ghi rành mạch cả.
    # speaker:NoiTam
    Mụ tưởng mình đã cam phận. Đúng như câu thơ: mụ nhìn là biết... nhưng chỉ biết điều mụ muốn thấy.
* [Cúi đầu: "Dạ... con xin nghe lời mẹ."]
    ~ c4_canh_giac = 0
    ~ fate(-5)
    # speaker:Kieu
    # emotion:controlled
    Kiều: Dạ... con mới đến, chưa biết gì. Con xin nghe lời mẹ.
    # speaker:TuBa
    # emotion:satisfied
    Tú Bà: Biết điều thế là tốt. Người biết điều thì sống lâu.
* [Cãi lại: "Tôi không nợ bà thứ gì cả."]
    ~ c4_canh_giac = 2
    # speaker:Kieu
    # emotion:determined
    Kiều: Tôi không nợ bà thứ gì cả. Người nhận bạc là Mã Giám Sinh.
    # speaker:Narrator
    # sfx:knock
    Cán quạt gõ vào song cửa, cạch một tiếng.
    # speaker:TuBa
    # emotion:rage
    Tú Bà: Ngựa non háu đá. Thằng Tư! Đêm nay đi tuần qua cửa phòng nó cho kỹ!
* [Hỏi khéo: "Thưa mẹ, con nợ mẹ bao nhiêu ạ?"]
    ~ c4_canh_giac = 1
    ~ c4_biet_no = true
    # speaker:Kieu
    # emotion:calm_probe
    Kiều: Thưa mẹ... con nợ mẹ bao nhiêu ạ? Để con biết mà lo.
    # speaker:TuBa
    # emotion:mocking
    Tú Bà: Bốn trăm năm mươi lạng tiền mua. Cộng tiền áo, tiền phấn, tiền cơm. Ta ghi cả vào sổ rồi — sổ nằm trong ngăn bàn, con khỏi lo nhớ.
* [Im lặng nhìn qua song cửa. #timeout]
    ~ c4_canh_giac = 1
    # speaker:Narrator
    Kiều không đáp. Qua song cửa, hai người nhìn nhau rất lâu.
    # speaker:TuBa
    # emotion:cold
    Tú Bà: Ánh mắt ấy... rồi ta sẽ dạy cho nó biết cúi xuống.

- (g_canh)
{dinh_menh >= 30:
    ~ c4_canh_giac = MIN(c4_canh_giac + 1, 2)
    # fx:spirit
    # speaker:TrangSach
    "Lầu xanh mới rủ trướng đào, càng treo giá ngọc càng cao phẩm người."
    # speaker:Narrator
    Tú Bà khựng lại, ngọn đèn trong tay mụ chao đi. Mụ gọi với ra hành lang: "Thằng Tư! Canh cho tỉnh!"
    # fx:none
    # speaker:NoiTam
    Mình đã đi lệch khỏi trang sách quá nhiều. Câu chuyện bắt đầu kéo mình về chỗ cũ.
}
# speaker:TuBa
# emotion:warm_fake
Tú Bà: Đêm nay ta có khách lớn từ phủ trên. Ngày mai con tiếp khách đầu tiên. Ngủ cho đẹp mặt.

# speaker:Narrator
# sfx:latch_drop
Then cửa trượt vào khấc phía bên ngoài. Tiếng chùm chìa lanh canh bên hông mụ xa dần về phía nhà sau.

# speaker:NoiTam
# emotion:determined
Ngày mai. Vậy là mình chỉ có đêm nay.

# speaker:NoiTam
Trong nguyên tác, Kiều ở lại. Rồi Sở Khanh đến, hứa đưa nàng đi trốn — và bán nàng thêm lần nữa.

# speaker:NoiTam
Lần này không chờ ai đến cứu cả.

# speaker:Kieu
# emotion:determined
Kiều: Muốn đi phải có chìa khoá, có đường ra. Và phải mang theo thứ chứng minh được bọn chúng là ai.

# scene:Dem_Night
# objective:Ra khỏi phòng
# gameplay:tutorial_dem
Đêm ở lầu Tú Bà.

-> c4d_hub


=== c4d_hub ===
* {c4_chia_khoa && not c4_khe_uoc_mg} [Mở tủ khoá đồng #explore:c4_cabinet]
    -> c4d_cabinet
* {not c4_so_no} [Xé trang sổ nợ #explore:c4_ledger]
    -> c4d_ledger
* {c4_chia_khoa && not c4_gap_co_lam} [Chị Lam #explore:c4_colam]
    -> c4d_colam
* {c4_khe_uoc_mg && c4_chia_khoa} [Ra cửa sau #explore:c4_leave]
    -> c4d_leave
+ [Chờ thời cơ #explore:c4_wait]
    -> c4d_hub


=== c4d_cabinet ===
# speaker:Narrator
# sfx:lock_open
Chiếc chìa khắc hình con cá xoay một vòng trong ổ đồng. Cánh tủ gỗ gụ hé ra.

# speaker:Narrator
Bên trong: một túi bạc nặng, và một xấp giấy buộc dây đỏ.

# speaker:Narrator
# gameplay:document_deal
Kiều mở xấp giấy buộc dây đỏ.

# speaker:NoiTam
"Giao kèo giữa Mã Giám Sinh và Tú Bà. Mã lo việc 'hỏi vợ' ở các huyện, Tú Bà trả mỗi người bốn trăm năm mươi lạng. Lập từ tháng Giêng."

# speaker:NoiTam
Tháng Giêng. Từ trước khi cha bị bắt. Hắn không tình cờ gặp nhà mình — hắn đi săn.

# speaker:NoiTam
Phía dưới còn ba cái tên đã gạch đi. Ba cô gái trước mình.

~ c4_khe_uoc_mg = true
~ ev_khe_uoc = true
~ c4_viec += 1
~ c4_buoc = 3
# speaker:Kieu
# emotion:realization
# clue:c4_giao_keo
Kiều: Nếu mang được thứ này ra ngoài... mình có thể chứng minh bọn chúng buôn người.

# speaker:NoiTam
Còn tờ văn tự bán thân của chính mình, nằm trong hộp sơn son ngoài kia. Mình làm gì với nó?

* [Cất văn tự vào áo — thêm một bằng chứng.]
    ~ c4_van_tu = 1
    # speaker:Kieu
    # emotion:cold
    Kiều: (Tờ giấy định giá mình. Giờ nó sẽ định tội chúng.)
* [Đốt nó trên ngọn đèn.]
    ~ c4_van_tu = 2
    ~ fate(5)
    # speaker:Narrator
    # sfx:fire
    Mép giấy cong lại, đỏ rực, rồi thành tro trong lòng bàn tay Kiều.
    # speaker:KieuThat
    # gameplay:kieu_voice
    Năm ấy ta chưa từng dám nghĩ đến điều này.
    # speaker:KieuThat
    Ngươi đốt nó... như thể ngươi chưa từng là của ai.
    # fx:none
    # speaker:NoiTam
    Mình chưa từng là của ai cả.
* [Để nguyên chỗ cũ — mụ sẽ không phát hiện sớm.]
    ~ c4_van_tu = 0
    # speaker:NoiTam
    Thứ gì còn nguyên chỗ cũ thì sáng mai mụ mới để ý. Mỗi khắc chậm trễ của mụ là một dặm đường của mình.

- (g_van_tu)
# speaker:NoiTam
Còn túi bạc. Bốn trăm năm mươi lạng — đúng bằng giá của mình.

* [Chỉ lấy vài lạng làm lộ phí.]
    ~ c4_tien = 1
    ~ tien += 5
    # speaker:Kieu
    Kiều: (Đủ cho vài bữa cơm và một chuyến đò. Không hơn.)
* [Lấy cả túi bạc — tiền bán mình thì phải thuộc về mình.]
    ~ c4_tien = 2
    ~ tien += 450
    ~ ev_tien = true
    ~ fate(10)
    # speaker:Narrator
    Túi bạc nặng trĩu. Mỗi nén đều đóng dấu "T." đỏ của lầu Lâm Tri.
    # speaker:NoiTam
    Có dấu của mụ. Là bằng chứng... nhưng cũng là thứ khiến người ta gọi mình là kẻ trộm.
    # clue:c4_bac_dau
    # speaker:NoiTam
    Và nó kêu. Chạy với thứ này, cả lầu sẽ nghe thấy.
* [Không lấy một đồng nào.]
    ~ c4_tien = 0
    # speaker:Kieu
    # emotion:firm
    Kiều: (Ta không cần tiền của mụ. Ta chỉ cần lấy lại chính mình.)

- (g_tien)
# speaker:Narrator
# sfx:lock_open
# objective:Mang giao kèo ra cửa sau trong bếp
Kiều khoá tủ lại như cũ.
-> c4d_hub


=== c4d_ledger ===
# speaker:Narrator
# gameplay:search_drawer
Kiều lần tìm trong ngăn bàn phòng sổ sách, lật đến trang có tên mình.

{c3_so_no:
    # speaker:NoiTam
    Cuốn sổ chiều nay. Giờ trang của mình đã có chữ: "Vương Thúy Kiều — nợ bốn trăm năm mươi lạng tiền mua. Áo lụa: ba mươi lạng."
- else:
    # speaker:NoiTam
    "Vương Thúy Kiều — nợ bốn trăm năm mươi lạng tiền mua. Áo lụa: ba mươi lạng." Mới đến một ngày đã nợ một gia tài.
}
{c4_biet_no:
    # speaker:NoiTam
    Đúng như mụ khoe. Mụ còn chẳng buồn giấu.
}

# speaker:Narrator
# sfx:paper
Kiều xé trang giấy thật chậm theo nếp gáy, gấp làm tư.

~ c4_so_no = true
~ ev_giay_no = true
~ c4_viec += 1
# speaker:NoiTam
# clue:c4_trang_no
Một khoản nợ không bao giờ trả hết — đây là sợi xích thật. Ai cũng phải thấy nó.
-> c4d_hub


=== c4d_colam ===
# speaker:CoLam
# emotion:fearful
Cô gái áo lam: (thì thầm) Kiều? Là em à? Chị nghe tiếng chìa...

# speaker:CoLam
Cô gái áo lam: Cho chị đi với. Chị không ở đây thêm một mùa nữa được.

# speaker:NoiTam
# timer:12
Hai người thì chậm hơn, dễ lộ hơn. Nhưng...

* {ts_hoa_troi == 2} [Lật trang "Hoa trôi bèo dạt đã đành...": "Câu ấy là của người đã buông tay. Mình thì chưa." #page:ts_hoa_troi]
    ~ dung(ts_hoa_troi, 0)
    ~ c4_ru_co_lam = true
    ~ fate(10)
    ~ soul(1)
    # speaker:Kieu
    # emotion:determined
    Kiều: "Hoa trôi bèo dạt đã đành"... Có người từng nói với em câu ấy. Đó là câu của người đã buông tay. Chị với em thì chưa. Đi cùng em.
    # speaker:CoLam
    # emotion:crying
    Cô gái áo lam: ...Chưa buông tay. Ừ. Chị đi.
    # speaker:KieuThat
    Ngươi trả lại câu của ta... bằng một nghĩa khác.
* [Nắm tay chị ấy: "Đi cùng em."]
    ~ c4_ru_co_lam = true
    ~ fate(10)
    # speaker:Kieu
    # emotion:determined
    Kiều: Đi cùng em. Em đi trước, chị chỉ bước khi em đã tới chỗ nấp kế tiếp. Có người thì chị ngồi im.
    # speaker:CoLam
    # emotion:crying
    Cô gái áo lam: Chị nghe em. Chị nghe hết.
* [Hứa: "Em sẽ quay lại đón chị."]
    ~ c4_ru_co_lam = false
    # speaker:Kieu
    # emotion:pained
    Kiều: Đi hai người thì cả hai sẽ bị bắt. Em hứa — em sẽ quay lại, mang theo người đủ sức phá cánh cổng này.
    # speaker:CoLam
    # emotion:fearful
    Cô gái áo lam: ...Ai cũng hứa như thế.
* [Im lặng quay đi. #timeout]
    ~ c4_ru_co_lam = false
    ~ soul(-1)
    # speaker:Narrator
    Kiều không trả lời. Sau lưng nàng, chị Lam kéo chăn trùm kín đầu.
    # speaker:NoiTam
    Mình xin lỗi.

- (g_co_lam)
~ c4_gap_co_lam = true
# speaker:NoiTam
# clue:c4_co_lam
Ngõ sau có chó, cổng trước có then. Chị ấy đã ở đây đủ lâu để biết hết.

# speaker:CoLam
# emotion:fearful
Cô gái áo lam: Còn nữa... ra ngoài rồi, gặp gã thư sinh nào áo khăn thơm phức hứa đưa em đi, thì đừng tin. Năm ngoái hắn dắt một đứa đi, sáng sau nó bị lôi về đây.

{
- ts_so_khanh == 1:
    ~ nhan(ts_so_khanh)
    # speaker:TrangSach
    # page:ts_so_khanh
    "Một chàng vừa trạc thanh xuân, hình dung chải chuốt áo khăn dịu dàng."
    # speaker:NoiTam
    Sở Khanh. Đủ câu rồi. Trong sách, hắn là kẻ lừa Kiều trốn đi rồi bỏ mặc nàng giữa đường.
- ts_so_khanh == 0:
    ~ nua(ts_so_khanh)
    # speaker:TrangSach
    # page_half:ts_so_khanh
    "Một chàng vừa trạc thanh xuân..."
    # speaker:NoiTam
    Câu thơ chỉ hiện ra một nửa. Phải chi mình đã hỏi chuyện thêm ai đó trong lầu.
}
-> c4d_hub


=== c4d_leave ===
# scene:Dem_BackDoor
# music:none
# speaker:Narrator
Canh ba. Cánh cửa sau hé ra một gang tay. Gió mưa ùa vào bếp, mùi bùn ngõ sau.

{c4_ru_co_lam:
    # speaker:CoLam
    # emotion:fearful
    Cô gái áo lam: (thì thầm) Chị sẵn sàng rồi.
}

# speaker:KieuThat
# gameplay:kieu_voice
Ngươi biết điều gì chờ ngươi ngoài kia không? Đường đêm. Chó dữ. Người của mụ.

* ["Biết. Nhưng ở lại thì ta biết chắc điều gì chờ ta."]
    # speaker:Kieu
    # emotion:determined
    Kiều: Biết. Nhưng ở lại thì ta biết chắc điều gì chờ ta sáng mai.
* ["Ngươi đã ở lại. Ta thì không."]
    # speaker:Kieu
    # emotion:firm
    Kiều: Ngươi đã ở lại, vì ngươi không còn đường nào khác. Ta thì vẫn còn một cánh cửa.

- (g_leave)
# fx:none
# speaker:KieuThat
Không phải vì ngươi biết truyện. Vì đêm nay có người say, và ngươi có chìa.

# speaker:KieuThat
Vậy thì đi đi. Đừng ngoảnh lại.

# speaker:NoiTam
# music:resolve
Không còn đường lui nữa.

-> chapter_5
