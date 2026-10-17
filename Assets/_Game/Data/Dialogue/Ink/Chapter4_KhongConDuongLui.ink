// ============================================================
// CHƯƠNG 4 — KHÔNG CÒN ĐƯỜNG LUI
// Scene Unity: Chapter04_TuBaNight (lầu Tú Bà về đêm, cùng bố cục Chương 3)
// Chủ đề: lần đầu tiên Kiều không chấp nhận số phận. Từ đây câu chuyện rẽ khỏi nguyên tác.
// Nhịp: Tú Bà "dạy quy củ" → Mục tiêu TÌM CÁCH RỜI KHỎI NƠI NÀY (khám phá tự do, lẩn tránh)
//       → Quan sát Tú Bà → Lấy chìa → Mở tủ (khế ước Mã–Tú Bà) → Cửa sau → Móc nối Chương 5.
// Unity ghi: c4_bi_bat (số lần bị bắt). Ink đọc: c3_* (kiến thức Chương 3).
// ============================================================

=== chapter_4 ===
# chapter:4
# scene:TuBa_Night
# music:tension_low
# speaker:Narrator
# camera:Wide_KieuRoomNight
Đêm xuống. Tiếng đàn nguyệt và tiếng cười từ sảnh trước vọng vào. Cửa phòng Kiều kẹt mở.

# speaker:TuBa
# emotion:warm_fake
# camera:CU_TuBa
Tú Bà: Con gái ngoan của ta. Đến giờ học quy củ rồi.

# speaker:TuBa
# emotion:cold
Tú Bà: Lầu này có ba điều. Một, khách là trời. Hai, nợ là nợ. Ba... kẻ nào trèo tường, ta bẻ chân kẻ đó.

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
    # sfx:slap
    Một cái tát. Má Kiều nóng rát.
    # speaker:TuBa
    # emotion:rage
    Tú Bà: Ngựa non háu đá. Người đâu! Đêm nay canh cửa phòng nó cho kỹ!
* [Hỏi khéo: "Thưa mẹ, con nợ mẹ bao nhiêu ạ?"]
    ~ c4_canh_giac = 1
    ~ c4_biet_no = true
    # speaker:Kieu
    # emotion:calm_probe
    Kiều: Thưa mẹ... con nợ mẹ bao nhiêu ạ? Để con biết mà lo.
    # speaker:TuBa
    # emotion:mocking
    Tú Bà: Bốn trăm năm mươi lạng tiền mua. Cộng tiền áo, tiền phấn, tiền cơm. Ta ghi cả vào sổ rồi — sổ nằm trong ngăn bàn, con khỏi lo nhớ.
* [Im lặng nhìn thẳng vào mụ. #timeout]
    ~ c4_canh_giac = 1
    # speaker:Narrator
    Kiều không đáp. Hai người nhìn nhau rất lâu.
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
    Tú Bà khựng lại ở bậc cửa, quay đầu nhìn Kiều thêm một lần nữa rồi gọi với ra hành lang: "Đêm nay thay chỗ cất chìa. Canh cho tỉnh!"
    # fx:none
    # speaker:NoiTam
    Mình đã đi lệch khỏi trang sách quá nhiều. Câu chuyện bắt đầu kéo mình về chỗ cũ.
}
# speaker:TuBa
# emotion:warm_fake
Tú Bà: Đêm nay ta có khách lớn từ phủ trên. Ngày mai con tiếp khách đầu tiên. Ngủ cho đẹp mặt.

# speaker:Narrator
# camera:Wide_KieuRoomNight
Mụ đi ra. Chùm chìa khóa đồng ở thắt lưng mụ kêu lanh canh dọc hành lang.

# speaker:NoiTam
# emotion:determined
Ngày mai. Vậy là mình chỉ có đêm nay.

# speaker:NoiTam
Trong nguyên tác, Kiều ở lại. Rồi Sở Khanh đến, hứa đưa nàng đi trốn — và bán nàng thêm lần nữa.

# speaker:NoiTam
Lần này không chờ ai đến cứu cả.

# speaker:Kieu
# emotion:determined
# camera:CU_Kieu
Kiều: Muốn đi, phải có chìa khóa, có lộ phí, có đường ra. Và phải mang theo thứ chứng minh được bọn chúng là ai.

# speaker:Narrator
# objective:TÌM CÁCH RỜI KHỎI NƠI NÀY
# gameplay:tutorial_night
Tự do hành động trong đêm. Danh sách việc cần làm hiện ở góc trái. Lính canh đã uống rượu, nhưng chưa say hẳn.

-> c4_hub


=== c4_hub ===
* {not c4_quan_sat} [Quan sát đường đi của Tú Bà #explore:c4_observe]
    -> c4_observe
* {c4_quan_sat && not c4_chia_khoa} [Lấy chùm chìa khóa của Tú Bà #explore:c4_key]
    -> c4_key
* {c4_chia_khoa && not c4_khe_uoc_mg} [Mở tủ khóa đồng trong phòng sổ sách #explore:c4_cabinet]
    -> c4_cabinet
* {not c4_so_no} [Xé trang sổ nợ của mình #explore:c4_ledger]
    -> c4_ledger
* {not c4_cua_sau} [Tìm cửa sau #explore:c4_backdoor]
    -> c4_backdoor
* {not c4_gap_co_lam} [Cô gái áo lam chưa ngủ #explore:c4_colam]
    -> c4_colam
* {c4_khe_uoc_mg && c4_cua_sau} [Ra cửa sau — rời khỏi nơi này #explore:c4_leave]
    -> c4_leave


=== c4_observe ===
# speaker:Narrator
# camera:OTS_Kieu_TuBa
Từ sau hàng cột, Kiều nhìn theo Tú Bà đi lại giữa sảnh và phòng riêng.

# speaker:NoiTam
Rót rượu cho khách. Cười. Ra sau lấy thêm một vò. Về phòng... ngồi xuống ghế — rồi lại ra.

# speaker:NoiTam
Mỗi lần về phòng mụ ngồi lâu hơn. Rượu ngấm rồi. Thêm một vòng nữa là mụ ngủ gật trên ghế.

~ c4_quan_sat = true
~ c4_viec += 1
# speaker:NoiTam
# clue:c4_lich_tu_ba
# objective:Đợi Tú Bà ngủ gật trong phòng · lấy chùm chìa ở thắt lưng mụ
Chìa ở thắt lưng. Không cần đánh nhau với ai — chỉ cần một bàn tay đủ nhẹ.
-> c4_hub


=== c4_key ===
# speaker:Narrator
# camera:OTS_Kieu_TuBa
Tú Bà ngả đầu trên ghế, miệng còn lẩm bẩm tính tiền. Chùm chìa đồng lấp lánh bên hông.

# speaker:NoiTam
Thở theo nhịp thở của mụ. Mụ hít vào thì đứng yên. Mụ thở ra thì mới đưa tay.

# speaker:Narrator
# gameplay:pickpocket
Giữ E để luồn tay. Chỉ nhích khi Tú Bà thở ra — nếu mụ cựa mình, dừng lại.

~ c4_chia_khoa = true
~ c4_viec += 1
# speaker:Kieu
# emotion:determined
# clue:c4_chia_khoa
# objective:Mở tủ khóa đồng trong phòng sổ sách
Kiều: (Năm chiếc chìa. Một chiếc có khắc hình con cá — giống ổ khóa trên tủ đồng.)
-> c4_hub


=== c4_cabinet ===
# speaker:Narrator
# camera:OTS_Kieu_Cabinet
# sfx:lock_open
Chiếc chìa khắc hình con cá xoay một vòng. Cánh tủ gỗ gụ mở ra không một tiếng động.

# speaker:Narrator
Bên trong: một túi bạc nặng, hộp sơn son quen thuộc — và một xấp giấy buộc dây đỏ.

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
# speaker:Kieu
# emotion:realization
# camera:CU_Kieu
# clue:c4_giao_keo
Kiều: Nếu mang được thứ này ra ngoài...

# speaker:Kieu
# emotion:determined
Kiều: Mình có thể chứng minh bọn chúng buôn người.

# speaker:NoiTam
Trong hộp sơn son là tờ văn tự bán thân của chính mình. Còn túi bạc...

# speaker:NoiTam
Tờ văn tự này thì sao?

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
    Và nó nặng. Chạy với thứ này sẽ không dễ.
* [Không lấy một đồng nào.]
    ~ c4_tien = 0
    # speaker:Kieu
    # emotion:firm
    Kiều: (Ta không cần tiền của mụ. Ta chỉ cần lấy lại chính mình.)

- (g_tien)
# speaker:Narrator
# sfx:lock_open
# objective:Tìm cửa sau và rời khỏi nơi này
Kiều khóa tủ lại như cũ.
-> c4_hub


=== c4_ledger ===
# speaker:Narrator
# camera:OTS_Kieu_Drawer
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
-> c4_hub


=== c4_backdoor ===
# speaker:Narrator
# camera:OTS_Kieu_BackDoor
Sau bếp có một cánh cửa gỗ thấp, cài then ngang và khóa móc. Bên ngoài là ngõ sau.

# speaker:Narrator
# sfx:dog
Có tiếng xích sắt kéo lê. Rồi một tiếng gầm gừ khe khẽ.

# speaker:NoiTam
Con chó ở ngõ sau — cô áo lam đã nói. Mở cửa phải thật chậm. Ra rồi phải đi dọc chân tường, tránh xa chỗ nó nằm.

{c4_chia_khoa:
    # speaker:NoiTam
    Ổ khóa móc này... một chiếc trong chùm chìa của mụ sẽ vừa.
- else:
    # speaker:NoiTam
    Khóa móc. Không có chìa thì đừng mơ.
}

~ c4_cua_sau = true
~ c4_viec += 1
# speaker:NoiTam
# clue:c4_cua_sau
Đường ra đây rồi.
-> c4_hub


=== c4_colam ===
# speaker:CoLam
# emotion:fearful
# camera:OTS_Kieu_CoLam
Cô gái áo lam: (thì thầm) Em... em định đi phải không? Chị thấy em đi qua phòng sổ sách.

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
    Kiều: Đi cùng em. Nhưng chị phải làm đúng như em làm — em khom, chị khom; em dừng, chị dừng.
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
    Kiều không trả lời. Sau lưng nàng, cô gái áo lam kéo chăn trùm kín đầu.
    # speaker:NoiTam
    Mình xin lỗi.

- (g_co_lam)
~ c4_gap_co_lam = true
# speaker:NoiTam
# clue:c4_co_lam
Ngõ sau có chó, cổng trước có then. Chị ấy đã ở đây đủ lâu để biết hết.

# speaker:CoLam
# emotion:fearful
Cô gái áo lam: Còn nữa... ra ngoài rồi, gặp gã thư sinh nào áo khăn thơm phức hứa đưa em đi, thì đừng tin. Năm ngoái hắn dắt một đứa đi, sáng sau nó bị lôi về đây, mặt mũi bầm dập.

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
-> c4_hub


=== c4_leave ===
# scene:TuBa_BackDoor
# music:none
# speaker:Narrator
# camera:CU_Kieu
Canh ba. Kiều đứng trong bóng tối sau bếp, tay đặt lên then cửa.

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
Vậy thì đi đi. Đừng ngoảnh lại.

# speaker:NoiTam
# music:resolve
Không còn đường lui nữa.

-> chapter_5
