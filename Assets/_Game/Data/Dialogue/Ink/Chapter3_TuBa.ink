// ============================================================
// CHƯƠNG 3 — TÚ BÀ
// Scene Unity: Chapter03_TuBaHouse
// Chủ đề: "Biết trước mà vẫn rơi vào bẫy." Căn nhà trông như mái ấm, rồi lộ ra là cái lồng.
// Nhịp: Đón tiếp → Khám phá tự do (cô gái, cửa, nghe lén, phòng sổ sách) → Mã Kiều
//       → Hoảng loạn → Giọng Kiều thật → Móc nối Chương 4.
// ============================================================

=== chapter_3 ===
# chapter:3
# scene:TuBa_Arrival
# music:tuba_warm
# speaker:Narrator
# camera:Wide_TuBaCourtyard
Sáng hôm sau. Lầu son ở Lâm Tri rực rỡ như nhà quan: rèm lụa, đèn lồng đỏ, tiếng đàn văng vẳng từ gác trên.

# speaker:TuBa
# emotion:warm_fake
# camera:CU_TuBa
Tú Bà: Ôi chao, con gái! Đi đường xa vất vả lắm phải không? Lại đây ta xem nào.

# speaker:TuBa
Tú Bà: Mặt hoa da phấn thế này... Mã công tử quả không nói sai.

# speaker:TuBa
# emotion:warm_fake
Tú Bà: Con cứ xem nơi này như nhà.

# speaker:MaGiamSinh
# emotion:businesslike
# camera:CU_MaGiamSinh
Mã Giám Sinh: Bà... à, mẹ cứ dạy bảo nàng. Ta có chút việc cần thưa với mẹ.

# speaker:Kieu
# emotion:controlled
# camera:CU_Kieu
Kiều: (Mẹ? Hắn gọi mụ là mẹ. Mã Giám Sinh làm gì có mẹ ở Lâm Tri.)

# speaker:TuBa
# emotion:warm_fake
Tú Bà: Con cứ đi dạo cho quen nhà. Các chị em ở đây hiền lắm. Chiều ta lại nói chuyện.

# speaker:NoiTam
Được thôi. "Như nhà." Để xem nhà này có mấy cái cửa, và cửa nào mở được.

# speaker:Narrator
# objective:Tìm hiểu ngôi nhà · Nói chuyện, kiểm tra cửa, nghe lén, tìm giấy tờ
# gameplay:tutorial_house
Tự do khám phá ngôi nhà. Người canh đi tuần trong sân — đừng để hắn thấy bạn ở nơi không được phép.

-> c3_hub


=== c3_hub ===
* [Nói chuyện với cô gái áo hồng bên giếng #explore:c3_girl_hong]
    -> c3_girl_hong
* [Nói chuyện với cô gái áo lam ở hành lang #explore:c3_girl_lam]
    -> c3_girl_lam
* [Kiểm tra cổng chính #explore:c3_gate]
    -> c3_gate
* [Kiểm tra cửa sổ phòng mình #explore:c3_window]
    -> c3_window
* [Nghe lén Tú Bà và Mã Giám Sinh #explore:c3_eavesdrop]
    -> c3_eavesdrop
* [Lục ngăn kéo trong phòng sổ sách #explore:c3_drawer]
    -> c3_drawer
* [Mở hộp gỗ sơn son trong phòng sổ sách #explore:c3_box]
    -> c3_box
* [Thử mở tủ khóa đồng #explore:c3_cabinet]
    -> c3_cabinet
* {c3_viec >= 3} [Cô gái lạ ở góc sân đang vẫy gọi #explore:c3_makieu]
    -> c3_makieu


=== c3_girl_hong ===
# speaker:CoHong
# emotion:amused
# camera:OTS_Kieu_CoHong
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
Cô gái áo hồng: Thôi, em đi đi. Đứng lâu với chị, người canh lại để ý.

~ c3_noi_chuyen += 1
~ c3_viec += 1
# speaker:NoiTam
# clue:c3_co_hong
"Có người hứa cưới." Cùng một kịch bản. Mình không phải người đầu tiên.
-> c3_hub


=== c3_girl_lam ===
# speaker:CoLam
# emotion:fearful
# camera:OTS_Kieu_CoLam
Cô gái áo lam: (thì thầm) Đừng nhìn ra cổng lâu thế. Họ để ý đấy.

# speaker:Kieu
# emotion:calm_probe
Kiều: Chị sợ điều gì?

# speaker:CoLam
# emotion:fearful
Cô gái áo lam: Tháng trước có đứa trèo tường. Người canh lôi về giữa sân... Từ đó nó không nói được nữa.

# speaker:CoLam
Cô gái áo lam: Ngõ sau có chó, cổng trước có then. Em đừng dại.

~ c3_noi_chuyen += 1
~ c3_viec += 1
# speaker:NoiTam
# clue:c3_co_lam
Người ta đã thử chạy. Và đã bị bắt lại. Chạy bừa là con đường ngắn nhất để chết.
-> c3_hub


=== c3_gate ===
# speaker:Narrator
# camera:OTS_Kieu_Gate
# gameplay:try_gate
Kiều đặt tay lên cánh cổng gỗ lim, đẩy thử.

# speaker:Narrator
# sfx:lock_rattle
Cánh cổng không nhúc nhích. Ổ khóa đồng to bằng nắm tay — và nó được khóa từ phía bên ngoài.

# speaker:NguoiCanh
# emotion:cold
Người canh: (vọng vào) Cô em muốn đi đâu? Cổng này chỉ mở cho khách.

~ c3_kiem_cua = true
~ c3_viec += 1
# speaker:NoiTam
# clue:c3_cong_khoa
Khóa từ bên ngoài. Nhà người ta khóa cửa để giữ trộm ở ngoài. Nhà này khóa để giữ người ở trong.
-> c3_hub


=== c3_window ===
# speaker:Narrator
# camera:OTS_Kieu_Window
Cửa sổ phòng Kiều chạm hoa sen rất đẹp. Kiều đẩy thử — phía sau lớp gỗ chạm là chấn song sắt, đóng đinh chặt vào khung.

# speaker:NoiTam
Đẹp thật. Một cái lồng chim được chạm trổ rất công phu.
-> c3_hub


=== c3_eavesdrop ===
# speaker:Narrator
# gameplay:eavesdrop_c3
Phòng Tú Bà ở cuối hành lang. Khom người, lẻn đến dưới khung cửa sổ, tránh tầm nhìn của người canh.

# speaker:MaGiamSinh
# emotion:angry
# camera:CU_Kieu_Listens
Mã Giám Sinh: Bốn trăm năm mươi lạng. Thiếu một đồng ta cũng không giao văn tự.

# speaker:TuBa
# emotion:mocking
Tú Bà: Ngươi mua có bốn trăm, lãi thế còn kêu? Nhà nó tưởng gả con, ngươi khéo diễn lắm.

# speaker:MaGiamSinh
# emotion:smug
Mã Giám Sinh: Không khéo thì sao lừa nổi cả nhà họ Vương? Văn tự bán thân đã điểm chỉ, bà cất cho kỹ.

# speaker:TuBa
# emotion:businesslike
Tú Bà: Ta để trong hộp sơn son ở phòng sổ sách. Chìa tủ thì lúc nào cũng ở thắt lưng ta. Yên tâm.

~ c3_nghe_len = true
~ c3_biet_chia_khoa = true
~ c3_viec += 1
# speaker:NoiTam
# clue:c3_nghe_len
"Gả con." Cả nhà mình... đã bị lừa trắng trợn như thế.
-> c3_hub


=== c3_drawer ===
# speaker:Narrator
# camera:OTS_Kieu_Drawer
# gameplay:search_drawer
Kiều kéo ngăn bàn. Bên trong là một cuốn sổ dày, mép giấy bóng dầu tay.

# speaker:NoiTam
"Son phấn: mười lạng. Áo lụa: ba mươi lạng. Thuốc: năm lạng." Mỗi trang là một cái tên. Không trang nào có chữ "đã trả".

~ c3_so_no = true
~ c3_viec += 1
# speaker:NoiTam
# clue:c3_so_no
Nợ chồng lên nợ. Không ai ở đây trả hết được. Đó mới là ổ khóa thật.
-> c3_hub


=== c3_box ===
# speaker:Narrator
# camera:OTS_Kieu_Box
Chiếc hộp sơn son không khóa. Tú Bà quá tin vào người canh ngoài sân.

# speaker:Narrator
# gameplay:document_contract
Trong hộp là một tờ giấy điểm chỉ son đỏ.

# speaker:NoiTam
"Văn tự bán thân. Người bán: Mã Giám Sinh. Người mua: Tú Bà. Người bị bán: Vương Thúy Kiều. Giá: bốn trăm năm mươi lạng bạc."

{doc_ky_khe:
    # speaker:NoiTam
    Câu chữ y hệt cái điều khoản "toàn quyền định đoạt" mình đã đọc trên tờ khế ở nhà. Hắn đã soạn sẵn từ trước.
}

~ c3_khe_uoc = true
~ c3_viec += 1
# speaker:Kieu
# emotion:cold
# clue:c3_van_tu
Kiều: (Ta không phải vợ ai cả. Ta là một món hàng có giá ghi rõ ràng.)

# speaker:NoiTam
Nếu lấy nó bây giờ, Tú Bà sẽ biết ngay. Phải để lại — nhưng mình đã biết nó nằm ở đâu.
-> c3_hub


=== c3_cabinet ===
# speaker:Narrator
# camera:OTS_Kieu_Cabinet
# sfx:lock_rattle
Chiếc tủ gỗ gụ khóa bằng ổ đồng. Kiều kéo thử — chỉ nghe tiếng then cài lạch cạch.

{c3_biet_chia_khoa:
    # speaker:NoiTam
    Chìa ở thắt lưng Tú Bà. Muốn mở tủ này, phải lấy được chìa từ tay mụ.
- else:
    # speaker:NoiTam
    Thứ gì quý đến mức phải khóa kỹ thế này? Phải tìm ra ai giữ chìa.
}
~ c3_viec += 1
# speaker:Narrator
# clue:c3_tu_khoa
Kiều ghi nhớ chiếc tủ khóa đồng trong phòng sổ sách.
-> c3_hub


=== c3_makieu ===
# scene:TuBa_MaKieu
# music:none
# speaker:MaKieu
# emotion:cold
# camera:CU_MaKieu
Mã Kiều: Cô vẫn chưa biết mình đang ở đâu sao?

# speaker:Kieu
# emotion:caught_off_guard
# camera:CU_Kieu
Kiều: Ý cô là gì?

# speaker:MaKieu
# emotion:cold
# camera:CU_MaKieu
Mã Kiều: Hắn đã bán cô cho Tú Bà rồi.

{c3_khe_uoc:
    # speaker:Kieu
    # emotion:pained
    Kiều: ...Ta đã thấy tờ văn tự.
    # speaker:MaKieu
    Mã Kiều: Thấy rồi mà vẫn còn mong đó là giấy cưới à?
- else:
    # speaker:MaKieu
    Mã Kiều: Ta là Mã Kiều. Ta cũng từng được hứa một đám cưới. Tờ văn tự của cô nằm trong hộp sơn son ở phòng sổ sách — không tin thì tự đi mà xem.
}

# speaker:MaKieu
# emotion:fearful
Mã Kiều: Tối nay mụ sẽ "dạy quy củ" cho cô. Người mới nào cũng vậy.

# speaker:NoiTam
# gameplay:panic
Không...

# speaker:NoiTam
Mình đã biết trước chuyện này.

# speaker:NoiTam
Mình đã đọc nó. Mình đã cười nó. Mình còn nói trước cả lớp là nếu mình là Kiều...

# speaker:NoiTam
Vậy mà vẫn rơi vào đúng cái bẫy của Kiều?

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
* [Im lặng.]
    ~ tra_loi_kieu_that = 3
    # speaker:Narrator
    Kiều không đáp. Hai bàn tay nàng run lên, rồi siết chặt lại.
    # speaker:KieuThat
    Im lặng cũng là một câu trả lời. Ta đã im lặng suốt mười lăm năm.

- (g2)
# fx:none
# speaker:MaKieu
# emotion:fearful
# camera:CU_MaKieu
Mã Kiều: Này... cô có nghe ta nói không? Đêm nay mụ có khách lớn, người canh sẽ uống rượu.

# speaker:MaKieu
Mã Kiều: Nếu cô định làm điều gì dại dột... thì chỉ có đêm nay.

# speaker:Kieu
# emotion:determined
# camera:CU_Kieu
# music:resolve
Kiều: Không phải dại dột.

# speaker:Kieu
Kiều: Là lần đầu tiên, ta tự quyết định đời mình.

-> chapter_4
