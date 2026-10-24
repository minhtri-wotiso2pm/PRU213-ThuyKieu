// ============================================================
// CHƯƠNG 3 — TÚ BÀ (bản góc nhìn thứ nhất)
// Scene Unity: Chapter03_LauTuBa · thiết kế: docs/codex/CHAPTER3_4_LAU_TU_BA_DESIGN.md §4
// Chủ đề: "Biết trước mà vẫn rơi vào bẫy." Ngôi lầu trông như mái ấm, rồi lộ ra là cái lồng.
// Nhịp: Đến lầu → khám phá (phòng mình, Hồng, Lam, Mã Kiều ở bếp, cổng) → chạng vạng: phòng sổ sách
//       (văn tự, sổ nợ, tủ khoá) → trốn trong tủ, nghe chuyện giả cưới → bị khoá trong phòng, thoát qua phòng
//       Tú Bà → về phòng: then cài, Mã Kiều, Kiều thật → móc nối Chương 4.
// Unity (Chapter03LauWorld) chọn các #explore khi người chơi làm việc tương ứng trong thế giới, và ghi c3_bi_bat.
// Lựa chọn #explore:c3_wait không bao giờ được Unity chọn: nó chỉ giữ hub mở khi thế giới chưa cho làm gì.
// ============================================================

=== chapter_3_lau ===
# chapter:3
# scene:Lau_Arrival
# music:tuba_warm
# speaker:Narrator
Chiều hôm sau, mưa vừa tạnh. Lầu son ở Lâm Tri còn bóng nước: mái ngói nhỏ giọt, đèn lồng đỏ chưa thắp, rèm lụa rủ sau hàng cột.

# speaker:TuBa
# emotion:warm_fake
Tú Bà: Ôi chao, con gái! Đi đường xa vất vả lắm phải không? Lại đây ta xem nào.

# speaker:TuBa
Tú Bà: Mặt hoa da phấn thế này... Mã công tử quả không nói sai.

~ nhan(ts_tu_ba)
# speaker:TrangSach
# page:ts_tu_ba
"Thoắt trông nàng đã biết tình, chim lồng khôn lẽ cất mình bay cao."

# speaker:NoiTam
Mụ nhìn một lần là biết mình đáng giá bao nhiêu. Mình cũng phải nhìn lại mụ như thế.

# speaker:MaGiamSinh
# emotion:businesslike
Mã Giám Sinh: Bà... à, mẹ cứ dạy bảo nàng. Ta có chút việc cần thưa với mẹ.

# speaker:Narrator
Hắn rút trong tay áo ra một tờ giấy gấp tư, đưa cho Tú Bà. Mụ liếc qua, rồi chìa cho người canh: "Đem cất vào chỗ cũ."

# speaker:Kieu
# emotion:controlled
Kiều: (Mẹ? Hắn gọi mụ là mẹ. Mã Giám Sinh làm gì có mẹ ở Lâm Tri. Và tờ giấy kia là gì?)

# speaker:Narrator
# sfx:gate_bar
Sau lưng Kiều, hai cánh cổng khép lại. Một thanh then gỗ nặng trượt vào khấc.

# speaker:TuBa
# emotion:warm_fake
Tú Bà: Con cứ xem nơi này như nhà. Phòng con ở hiên tây. Các chị em ở đây hiền lắm. Chiều ta lại nói chuyện.

# speaker:NoiTam
"Như nhà." Để xem nhà này có mấy cái cửa, và cửa nào mở được.

# scene:Lau_Explore
# objective:Tìm hiểu ngôi lầu
# gameplay:tutorial_lau
Tự do đi lại trong lầu.

-> c3l_hub


=== c3l_hub ===
* {not c3l_phong} [Xem phòng mình #explore:c3_room]
    -> c3l_room
* [Cô gái áo hồng #explore:c3_girl_hong]
    -> c3l_hong
* [Cô gái áo lam #explore:c3_girl_lam]
    -> c3l_lam
* [Người đàn bà trong bếp #explore:c3_makieu_bep]
    -> c3l_makieu
* [Cổng chính #explore:c3_gate]
    -> c3l_gate
* {c3_viec >= 2} [Hộp sơn son trong phòng sổ sách #explore:c3_box]
    -> c3l_box
* {c3_viec >= 2} [Ngăn bàn sổ sách #explore:c3_drawer]
    -> c3l_drawer
* {c3_viec >= 2} [Tủ khoá đồng #explore:c3_cabinet]
    -> c3l_cabinet
* {c3_khe_uoc} [Có người đang đến #explore:c3_office_hide]
    -> c3l_overhear
* {c3_nghe_len} [Về phòng #explore:c3_end]
    -> c3l_end
+ [Chờ thời cơ #explore:c3_wait]
    -> c3l_hub


=== c3l_room ===
# speaker:Narrator
Kiều vén tấm rèm đỏ bên cửa sổ. Sau lớp lụa là chấn song sắt, đóng đinh chặt vào khung. Ngoài song là một con ngõ hẹp, nước mưa còn đọng.

# speaker:NoiTam
Áo lụa mới, hộp phấn mới, gương đồng. Món nào cũng có một mảnh giấy đỏ cài vào: "Áo lụa — mười hai lạng — ghi sổ."

# speaker:NoiTam
Cửa phòng không có then bên trong. Chỉ có cái lỗ then ở phía ngoài.

~ c3l_phong = true
~ c3_viec += 1
# speaker:NoiTam
# clue:c3_cong_khoa
Một cái lồng chim được chạm trổ rất công phu. Quà nào cũng là một khoản nợ.
-> c3l_hub


=== c3l_hong ===
# speaker:CoHong
# emotion:amused
Cô gái áo hồng: Lại có người mới à? Đẹp thế này, mẹ Tú vui lắm đây.

# speaker:NoiTam
# timer:10
Hỏi gì trước?

* [Hỏi: "Chị ở đây lâu chưa?"]
    # speaker:CoHong
    # emotion:tired_small_smile
    Cô gái áo hồng: Ba năm. Lúc đến cũng như em — có người hứa cưới.
* [Hỏi: "Ở đây có được ra phố không?" #timeout]
    # speaker:CoHong
    # emotion:mocking
    Cô gái áo hồng: Ra phố? Có chứ. Khi nào trả hết nợ.
* [Hỏi: "Mẹ Tú là người thế nào?"]
    # speaker:CoHong
    # emotion:cold
    Cô gái áo hồng: Mẹ thương ai thì người ấy có áo đẹp. Mà áo đẹp... thì ghi vào sổ nợ.

- (g1)
# speaker:CoHong
# emotion:tired_small_smile
Cô gái áo hồng: Mà này, trời mưa thì đừng bước ra sân. Đá ướt, bước chân vang như gõ mõ. Thằng canh nó uống từ chập tối, nhưng tai nó thính lắm.

# speaker:CoHong
Cô gái áo hồng: Còn nữa. Thỉnh thoảng có một gã thư sinh áo khăn chải chuốt ghé qua, hứa đưa đứa nào đó trốn đi. Chưa đứa nào đi theo hắn mà còn quay về kể.

~ nua(ts_so_khanh)
# speaker:TrangSach
# page_half:ts_so_khanh
"Một chàng vừa trạc thanh xuân..."

~ c3_noi_chuyen += 1
~ c3_viec += 1
~ soul(1)
# speaker:NoiTam
# clue:c3_co_hong
"Có người hứa cưới." Cùng một kịch bản. Mình không phải người đầu tiên.
-> c3l_hub


=== c3l_lam ===
# speaker:CoLam
# emotion:fearful
Cô gái áo lam: (thì thầm) Khép cửa lại đã. Họ để ý đấy.

# speaker:Kieu
# emotion:calm_probe
Kiều: Cửa phòng chị... ai đóng tấm gỗ kia vào?

# speaker:CoLam
# emotion:fearful
Cô gái áo lam: Chị phá nó một lần. Họ đóng lại, rồi chị nằm ba ngày không dậy được. Ngõ sau có chó, cổng trước có then. Em đừng dại.

# speaker:Narrator
Trên bàn có một lá thư gấp dở, đề "Gửi mẹ", và nửa chiếc lược sừng. Vai chị Lam run lên từng đợt.

# speaker:NoiTam
Chị ấy khóc mà không dám ra tiếng. Ngày trước mẹ dạy: người ta buồn thì nói với nhau bằng thơ.

# speaker:Kieu
# emotion:soft_firm
# gameplay:lucbat_ch3
Kiều: (khẽ) Chị ơi...

{
- lb_ch3 >= 3:
    ~ soul(2)
    # speaker:CoLam
    # emotion:crying
    Cô gái áo lam: ...Ba năm rồi mới có người đọc thơ cho chị nghe. Cảm ơn em.
- lb_ch3 == 2:
    ~ soul(1)
    # speaker:CoLam
    Cô gái áo lam: Em nói nghe lạ tai, mà ấm. Chị... chị đỡ hơn rồi.
- else:
    # speaker:CoLam
    # emotion:fearful
    Cô gái áo lam: Thôi, em đừng nói nữa. Người canh nghe thấy bây giờ.
}

~ c3_noi_chuyen += 1
~ c3_viec += 1
~ soul(1)
{dong_dieu >= 4:
    ~ nhan(ts_hoa_troi)
    # speaker:KieuThat
    # gameplay:kieu_voice
    Ngày trước, ta cũng từng ngồi cạnh một người như thế. Ta chỉ biết nói với nàng một câu.
    # speaker:TrangSach
    # page:ts_hoa_troi
    "Hoa trôi bèo dạt đã đành, biết duyên mình biết phận mình thế thôi."
    # fx:none
    # speaker:NoiTam
    Đó là câu của người đã buông xuôi. Giữ lấy nó... để có ngày nói ngược lại.
}
# speaker:NoiTam
# clue:c3_co_lam
Người ta đã thử chạy. Và đã bị bắt lại. Chạy bừa là con đường ngắn nhất để chết.
-> c3l_hub


=== c3l_makieu ===
# speaker:MaKieu
# emotion:cold
Người đàn bà trong bếp: Người mới à. Ngồi xuống đâu thì ngồi, đừng đụng vào nồi.

# speaker:MaKieu
Mã Kiều: Ta là Mã Kiều, lo bếp núc. Ở cái nhà này, ăn một bát cơm cũng ghi nợ. Cô ăn chậm thôi.

# speaker:Narrator
Sau lưng mụ là một cánh cửa gỗ thấp, cài then ngang, khoá móc to bằng nắm tay. Ngoài kia là ngõ sau.

# speaker:Kieu
# emotion:calm_probe
Kiều: Cửa ấy mở ra đâu?

# speaker:MaKieu
# emotion:cold
Mã Kiều: Ra ngõ. Chìa thì bà chủ đeo ở thắt lưng, ngủ cũng không rời. Đừng nhìn nó lâu thế.

~ c3_biet_chia_khoa = true
~ c3_viec += 1
# speaker:NoiTam
# clue:c3_tu_khoa
Cửa sau. Chìa ở thắt lưng Tú Bà. Mình nhớ rồi.
-> c3l_hub


=== c3l_gate ===
# speaker:Narrator
# sfx:lock_rattle
Kiều đặt tay lên cánh cổng gỗ lim. Thanh then ngang và sợi xích không nhúc nhích.

# speaker:TuBa
# emotion:warm_fake
Tú Bà: (từ sau bình phong) Con định đi đâu đấy? Ngồi đây, mẹ tính cho con nghe.

# speaker:TuBa
# emotion:businesslike
Tú Bà: Tiền mua: bốn trăm năm mươi lạng. Áo lụa: mười hai. Son phấn: mười. Cơm nước, phòng ốc, tính theo ngày. Mẹ ghi cả, rành mạch.

# speaker:TuBa
# emotion:mocking
Tú Bà: Cổng này chỉ mở cho khách. Ngoan thì mai mốt mẹ cho ra phố.

~ c3_kiem_cua = true
~ c3_viec += 1
# speaker:NoiTam
# clue:c3_cong_khoa
Khoá từ bên ngoài. Nhà người ta khoá cửa để giữ trộm ở ngoài. Nhà này khoá để giữ người ở trong.
-> c3l_hub


=== c3l_box ===
# speaker:Narrator
Chiếc hộp sơn son không khoá. Ở cái nhà mà cổng khoá từ bên ngoài, Tú Bà chẳng cần khoá thêm gì.

# speaker:Narrator
# gameplay:document_contract
Trong hộp là một tờ giấy điểm chỉ son đỏ.

# speaker:NoiTam
"Văn tự bán thân. Người bán: Mã Giám Sinh. Người mua: Tú Bà. Người bị bán: Vương Thúy Kiều. Giá: bốn trăm năm mươi lạng bạc."

{doc_ky_khe:
    # speaker:NoiTam
    Câu chữ y hệt điều khoản "toàn quyền định đoạt" mình đã đọc trên tờ khế ở nhà. Hắn đã soạn sẵn từ trước.
}

~ c3_khe_uoc = true
~ c3_viec += 1
# speaker:Kieu
# emotion:cold
# clue:c3_van_tu
Kiều: (Ta không phải vợ ai cả. Ta là một món hàng có giá ghi rõ ràng.)

# speaker:NoiTam
Nếu lấy nó bây giờ, Tú Bà sẽ biết ngay. Phải để lại — nhưng mình đã biết nó nằm ở đâu.
-> c3l_hub


=== c3l_drawer ===
# speaker:Narrator
# gameplay:search_drawer
Kiều kéo ngăn bàn. Bên trong là một cuốn sổ dày, mép giấy bóng dầu tay.

# speaker:NoiTam
"Son phấn: mười lạng. Áo lụa: ba mươi lạng. Thuốc: năm lạng." Mỗi trang là một cái tên. Không trang nào có chữ "đã trả".

~ c3_so_no = true
~ c3_viec += 1
# speaker:NoiTam
# clue:c3_so_no
Nợ chồng lên nợ. Không ai ở đây trả hết được. Đó mới là ổ khoá thật.
-> c3l_hub


=== c3l_cabinet ===
# speaker:Narrator
# sfx:lock_rattle
Chiếc tủ gỗ gụ khoá bằng ổ đồng. Qua song tủ thấy một xấp giấy buộc dây đỏ, cạnh một túi bạc.

{c3_biet_chia_khoa:
    # speaker:NoiTam
    Chìa ở thắt lưng Tú Bà. Muốn mở tủ này, phải lấy được chìa từ người mụ.
- else:
    # speaker:NoiTam
    Thứ gì quý đến mức phải khoá kỹ thế này? Phải tìm ra ai giữ chìa.
}
~ c3l_tu = true
~ c3_viec += 1
# speaker:Narrator
# clue:c3_tu_khoa
Kiều ghi nhớ chiếc tủ khoá đồng sau tấm song gian trong.
-> c3l_hub


=== c3l_overhear ===
# scene:Lau_Overhear
# music:none
# speaker:MaGiamSinh
# emotion:angry
Mã Giám Sinh: Bốn trăm năm mươi lạng, như đã hẹn trong thư. Thiếu một đồng ta cũng không đi.

# speaker:TuBa
# emotion:mocking
Tú Bà: Ngươi đưa nhà họ Vương có bốn trăm, giờ đòi ta thêm năm mươi lạng công đi đường à?

# speaker:MaGiamSinh
# emotion:smug
Mã Giám Sinh: Công diễn tuồng thì đúng hơn. Không khéo thì sao lừa nổi cả nhà họ Vương? Họ còn khóc lóc tiễn con gái về nhà chồng.

# speaker:TuBa
# emotion:businesslike
Tú Bà: Thôi được. Văn tự để trong hộp kia. Bạc thì nằm trong tủ khoá đồng.

# speaker:MaGiamSinh
Mã Giám Sinh: Thì mở tủ ra đi.

# speaker:TuBa
# emotion:mocking
Tú Bà: Chìa ta đeo ở thắt lưng, ngủ cũng không rời. Tối nay ta có khách lớn từ phủ trên. Xong việc ta trả. Ngươi ngủ lại một đêm, mai hẵng lên đường.

~ c3_nghe_len = true
~ c3_biet_chia_khoa = true
~ c3_viec += 1
# speaker:NoiTam
# clue:c3_nghe_len
"Về nhà chồng." Cả nhà mình khóc tiễn mình... trong một vở tuồng hắn dựng sẵn.

# scene:Lau_Locked
# speaker:Narrator
# sfx:lock_open
Hai người ra bằng cửa trước. Tiếng chìa xoay trong ổ khoá phía ngoài. Rồi bước chân xa dần.

# speaker:NoiTam
# objective:Thoát khỏi phòng sổ sách
Khoá rồi. Cửa thông sang phòng Tú Bà — họ vừa đi qua lối đó.
-> c3l_hub


=== c3l_end ===
# scene:Lau_End
# music:none
# speaker:Narrator
# sfx:latch_drop
Kiều vừa bước vào phòng thì cánh cửa sập lại sau lưng. Then gỗ phía ngoài trượt vào khấc.

# speaker:MaKieu
# emotion:cold
Mã Kiều: (qua song cửa) Cô vẫn chưa biết mình đang ở đâu sao?

# speaker:Kieu
# emotion:caught_off_guard
Kiều: Ý bà là gì?

# speaker:MaKieu
# emotion:cold
Mã Kiều: Hắn đã bán cô cho Tú Bà rồi. Ta cũng từng được hứa một đám cưới.

{c3_khe_uoc:
    # speaker:Kieu
    # emotion:pained
    Kiều: ...Ta đã thấy tờ văn tự.
    # speaker:MaKieu
    Mã Kiều: Thấy rồi mà vẫn còn mong đó là giấy cưới à?
}

# speaker:NoiTam
# gameplay:panic
Không...

# speaker:NoiTam
Mình đã biết trước chuyện này.

# speaker:NoiTam
Mình đã đọc nó. Mình đã cười nó. Mình còn nói trước cả lớp là nếu mình là Kiều...

# speaker:NoiTam
Vậy mà vẫn tự bước vào đúng cái bẫy của Kiều.

# speaker:KieuThat
# gameplay:kieu_voice
Ngươi từng nói mọi chuyện rất dễ dàng.

# speaker:KieuThat
Bây giờ thì sao?

* ["...Không dễ chút nào. Ta đã sai khi phán xét ngươi."]
    ~ tra_loi_kieu_that = 1
    # speaker:Kieu
    # emotion:pained
    Kiều: Không dễ chút nào. Biết trước cũng không có nghĩa là thoát được. Ta đã sai khi phán xét ngươi.
    # speaker:KieuThat
    Vậy thì giờ ngươi đã bắt đầu hiểu.
* ["Chưa xong đâu. Ta biết cái bẫy này — ta sẽ tìm đường ra."]
    ~ tra_loi_kieu_that = 2
    # speaker:Kieu
    # emotion:determined
    Kiều: Chưa xong đâu. Ngươi không biết những gì ta biết. Ta sẽ tìm đường ra.
    # speaker:KieuThat
    Ta cũng từng nghĩ thế. Hãy cho ta thấy ngươi khác ta ở chỗ nào.
* {ts_hieu == 2} [Lật trang "Duyên hội ngộ, đức cù lao...": "Ngươi chọn chữ Hiếu. Không phải vì ngươi ngốc." #page:ts_hieu]
    ~ dung(ts_hieu, 5)
    ~ tra_loi_kieu_that = 1
    ~ soul(1)
    # speaker:Kieu
    # emotion:pained
    Kiều: Bên tình bên hiếu bên nào nặng hơn... Ngươi đã phải trả lời câu ấy trong một đêm, một mình. Ngươi chọn chữ Hiếu, không phải vì ngươi ngốc.
    # speaker:KieuThat
    Mười lăm năm... chưa ai nói với ta câu đó.
* [Im lặng.]
    ~ tra_loi_kieu_that = 3
    # speaker:Narrator
    Kiều không đáp. Hai bàn tay nàng run lên, rồi siết chặt lại.
    # speaker:KieuThat
    Im lặng cũng là một câu trả lời. Ta đã im lặng suốt mười lăm năm.

- (g2)
{
- tra_loi_kieu_that == 1:
    ~ soul(2)
- tra_loi_kieu_that == 2:
    ~ soul(1)
}
# fx:none
# speaker:MaKieu
# emotion:fearful
Mã Kiều: (hạ giọng) Này... cô có nghe ta nói không? Đêm nay mụ có khách lớn. Thằng canh sẽ uống rượu với bọn gia nhân.

# speaker:MaKieu
Mã Kiều: Nếu cô định làm điều gì dại dột... thì chỉ có đêm nay.

# speaker:Kieu
# emotion:determined
# music:resolve
Kiều: Không phải dại dột.

# speaker:Kieu
Kiều: Là lần đầu tiên, ta tự quyết định đời mình.

-> chapter_4_dem
