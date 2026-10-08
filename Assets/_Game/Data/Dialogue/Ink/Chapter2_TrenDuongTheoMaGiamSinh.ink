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
# speaker:Narrator
# camera:Wide_Road
Chiều muộn. Con đường đất đỏ uốn qua rặng tre, dẫn khỏi làng Vương gia.

# speaker:Narrator
Không có kiệu hoa, không có kèn trống. Chỉ có Mã Giám Sinh, một tên tùy tùng vác hòm bạc, và Kiều.

# speaker:MaGiamSinh
# emotion:polite_fake
# camera:CU_MaGiamSinh
Mã Giám Sinh: Nàng cứ yên tâm. Về đến nơi sẽ có người chăm sóc nàng.

# speaker:MaGiamSinh
# emotion:smug
Mã Giám Sinh: Đường gò đồi hơi xa, nhưng qua đồi tre là tới bến. Đi kịp trước tối thì đêm nay đã có cơm ấm.

# speaker:Kieu
# emotion:controlled
# camera:CU_Kieu
Kiều: Đa tạ công tử.

# speaker:NoiTam
Được rồi. Bình tĩnh. Mình đang ở trong thân xác Thúy Kiều, đang đi theo cái người mà cả lớp từng gọi là "kẻ buôn người".

# speaker:NoiTam
Nhưng sách là sách. Ở đây, hắn đang cười rất tử tế. Mình phải tự nhìn bằng mắt mình.

# speaker:Narrator
# objective:Theo Mã Giám Sinh · Quan sát và tìm hiểu, nhưng đừng để hắn sinh nghi
# gameplay:tutorial_road
Đi theo Mã Giám Sinh. Giữ chuột phải để quan sát. Nếu tụt lại quá xa hoặc nhìn hắn quá lộ liễu, hắn sẽ sinh nghi.

-> c2_road


=== c2_road ===
// Hub khám phá: mọi lựa chọn đều được chọn bằng hành động trong thế giới 3D.
* {not mm_ban_tay} [Quan sát kỹ Mã Giám Sinh (giữ chuột phải) #explore:c2_observe]
    -> c2_observe
* {not mm_hoi_duong} [Bắt chuyện, hỏi hắn đang đưa mình đi đâu #explore:c2_ask]
    -> c2_ask
* {not mm_mau_giay} [Nhặt mẩu giấy rơi trên đường #explore:c2_paper]
    -> c2_paper
* {manh_moi >= 2} [Nghỉ chân ở quán nước ven đường #explore:c2_eavesdrop]
    -> c2_eavesdrop


=== c2_observe ===
# speaker:Narrator
# camera:OTS_Kieu_MaGiamSinh
Mã Giám Sinh đi chậm lại, lấy từ hòm ra một xâu bạc. Hắn đếm một lượt, rồi đếm lại, môi lẩm nhẩm như người buôn tính lãi.

# speaker:Narrator
Tên tùy tùng ghé tai hắn nói gì đó. Cả hai cùng liếc về phía Kiều — cái liếc của người xem hàng.

# speaker:NoiTam
Người đi đón vợ về nhà thì sao phải đếm bạc như đi chợ? Còn ánh mắt kia...

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
    # speaker:NoiTam
    Nhớ ra rồi. Nguyễn Du tả hắn chỉ bằng hai câu, mà câu nào cũng đúng.
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
    Hỏi về cha mẹ mà hắn trả lời về "người lo liệu". Lâm Tri... mình nhớ cái tên này.
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
    "Người lo cho nàng." Hắn nói câu này đến lần thứ hai rồi. Mà người đó không phải mẹ chồng.

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
    Mã Giám Sinh: Nàng tinh mắt đấy. Lâm Tri có một nhà quen, mối nào ta đưa về cũng được giá. Ha... à, ý ta là hàng lụa.
    # speaker:NoiTam
    Câu thơ nói hắn trước cả khi hắn mở miệng. Tâng bốc đúng chỗ, hắn tự khai hết.

- (g1)
~ mm_hoi_duong = true
~ manh_moi += 1
# speaker:Narrator
# clue:c2_lam_tri
Kiều ghi nhớ: điểm đến là Lâm Tri. Không ai nhắc đến lễ cưới.
-> c2_road


=== c2_paper ===
# speaker:Narrator
# camera:OTS_Kieu_Paper
Một mẩu giấy gấp tư nằm giữa đường, mép còn dính sáp. Nó vừa rơi khỏi tay áo Mã Giám Sinh khi hắn rút xâu bạc.

# speaker:Narrator
# gameplay:document_paper
Kiều cúi xuống nhặt, mở ra thật nhanh.

# speaker:NoiTam
"Bà T. ở Lâm Tri nhận hàng." "Giá bốn trăm năm mươi lạng." Hắn mua mình bốn trăm... để bán lại bốn trăm năm mươi?

~ mm_mau_giay = true
~ manh_moi += 1

# speaker:MaGiamSinh
# emotion:suspicious
# camera:CU_MaGiamSinh
Mã Giám Sinh: Nàng nhặt được gì đấy?

# speaker:NoiTam
# timer:7
Nhanh lên. Trả hay giữ?

* [Đưa lại, giả vờ chưa đọc: "Hình như công tử đánh rơi." #timeout]
    ~ nghi_ngo = MAX(nghi_ngo - 5, 0)
    # speaker:Kieu
    # emotion:calm
    Kiều: Hình như công tử đánh rơi. Chữ nhỏ quá, thiếp chẳng đọc được.

    # speaker:MaGiamSinh
    # emotion:polite_fake
    Mã Giám Sinh: Giấy nợ vặt thôi. Nàng đưa đây.

    # speaker:Narrator
    Hắn nhét mẩu giấy vào sâu trong ngực áo. Nhưng Kiều đã nhớ từng chữ.

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
    Hắn sờ tay áo mình, nhíu mày, rồi im lặng bước tiếp. Mẩu giấy nằm yên trong tay áo Kiều — một bằng chứng nhỏ, đổi bằng một ánh nhìn ngờ vực.

- (g2)
# speaker:Narrator
# clue:c2_mau_giay
Kiều ghi nhớ: "Bà T. ở Lâm Tri nhận hàng — giá bốn trăm năm mươi lạng."
-> c2_road


=== c2_eavesdrop ===
# scene:Road_RestStop
# speaker:Narrator
# camera:Wide_TeaHouse
Trời sẩm tối. Mã Giám Sinh dừng ở một quán nước ven đường, bảo Kiều ngồi nghỉ rồi kéo tên tùy tùng ra sau vách tre.

# speaker:NoiTam
Hắn cố ý tránh mình. Phải nghe được họ nói gì — nhưng không được để lộ.

# speaker:Narrator
# gameplay:eavesdrop_c2
Khom người (giữ Ctrl), lẻn sát vách tre và ở ngoài tầm nhìn của chúng để nghe trọn câu chuyện.

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
Tú Bà?

# speaker:NoiTam
Khoan... cái tên này...

# speaker:NoiTam
# gameplay:memory_flash
Trong Truyện Kiều...

# speaker:TrangSach
"Đắn đo cân sắc cân tài, ép cung cầm nguyệt, thử bài quạt thơ..."

# speaker:TrangSach
"Tiếc thay một đóa trà mi, con ong đã tỏ đường đi lối về."

~ nhan(ts_co_ke)
# speaker:TrangSach
# page:ts_co_ke
"Cò kè bớt một thêm hai, giờ lâu ngã giá vàng ngoài bốn trăm."

# speaker:Kieu
# emotion:realization
# camera:CU_Kieu
Kiều: (Mã Giám Sinh không cưới Kiều.)

# speaker:Kieu
Kiều: (Hắn đang bán mình.)

# speaker:NoiTam
Ghép lại từng mảnh... hay tin vào câu thơ vừa hiện lên?

* {ts_co_ke == 2} [Lật trang "Cò kè bớt một thêm hai...": cái giá đã có sẵn trong sách #page:ts_co_ke]
    ~ dung(ts_co_ke, 10)
    ~ suy_luan = 3
    # speaker:NoiTam
    Bốn trăm lạng mua về, bán lại cho Tú Bà có lãi. Không cần ghép, câu thơ đã nói hết rồi.
* [Tự ghép các manh mối.]
    # speaker:Narrator
    # gameplay:deduction_c2
    Ghép các manh mối đã thu thập để xác nhận sự thật.

- (g_suy)
{suy_luan >= 3:
    # speaker:NoiTam
    Không còn nghi ngờ gì nữa. Mọi mảnh ghép đều khớp: tiền, nơi đến, giá bán, và cái tên Tú Bà.
- else:
    # speaker:NoiTam
    Chưa đủ mọi mảnh ghép... nhưng cái tên Tú Bà thì không thể nhầm được.
}
-> c2_decision


=== c2_decision ===
# speaker:NoiTam
# fx:tension
# music:tension_low
Bây giờ... phải làm gì?

* [Bỏ chạy ngay bây giờ, khi trời còn tối.]
    # speaker:NoiTam
    # gameplay:imagine_run
    Mình chạy vào rừng tre. Hắn sẽ không đuổi được ngay...

    # speaker:NoiTam
    ...rồi hắn sẽ quay về làng, cầm tờ khế đòi lại bốn trăm lạng. Cha và em lại bị giải lên quan, lần này không ai cứu.

    # speaker:NoiTam
    Không. Chạy lúc này là ném chính gia đình mình vào ngục.
    -> c2_decision

* [Giả vờ như chưa biết gì.]
    # speaker:NoiTam
    Mình phải giả vờ chưa biết gì.

    # speaker:NoiTam
    Diễn cho tròn vai. Cho đến khi tìm được đường ra — một đường ra không khiến cha phải trả giá.
    -> c2_mask


=== c2_mask ===
# fx:none
# scene:Road_MaskTest
# speaker:MaGiamSinh
# emotion:suspicious
# camera:CU_MaGiamSinh
Mã Giám Sinh: Nàng làm gì sau vách tre thế?

# speaker:NoiTam
# timer:6
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
Mã Giám Sinh: Mai là tới Lâm Tri. Nàng có mong không?

# speaker:NoiTam
# timer:6
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

- (g4)
# speaker:MaGiamSinh
# emotion:polite_fake
Mã Giám Sinh: Nàng biết điều như vậy là tốt. Người biết điều thì sống dễ.

# speaker:NoiTam
# timer:6
Đến lúc chốt lại rồi.

* [Rót chén trà mời hắn: "Công tử đi đường cũng mệt, dùng chén trà." #timeout]
    ~ gia_vo += 1
    ~ fate(-5)
    ~ nghi_ngo = MAX(nghi_ngo - 15, 0)
    # speaker:Kieu
    # emotion:calm
    Kiều: Công tử đi đường cũng mệt, dùng chén trà cho ấm.
    # speaker:MaGiamSinh
    # emotion:satisfied
    Mã Giám Sinh: Hà... nàng đúng là ngoan.
* [Im lặng nhìn xuống đất.]
    # speaker:Narrator
    Kiều không nói gì. Mã Giám Sinh nhìn nàng thêm một lúc, rồi khẽ nhún vai.

- (g5)
{
    - nghi_ngo >= 60:
        # speaker:MaGiamSinh
        # emotion:suspicious
        Mã Giám Sinh: (nói với tùy tùng) Đêm nay canh nó cho kỹ. Ta không thích ánh mắt ấy.
        # speaker:NoiTam
        Hắn vẫn còn nghi. Từ giờ mình sẽ bị để mắt nhiều hơn.
    - else:
        # speaker:MaGiamSinh
        # emotion:smug
        Mã Giám Sinh: (nói với tùy tùng) Dễ bảo hơn ta tưởng. Đỡ tốn công.
        # speaker:NoiTam
        Hắn tin rồi. Tốt. Cứ để hắn nghĩ mình là con cừu.
  }

-> c2_ending


=== c2_ending ===
# scene:Road_LamTriGate
# music:road_night
# speaker:Narrator
# camera:Wide_LamTri
Nửa đêm, cổng Lâm Tri hiện ra sau màn sương. Phía xa, một dãy đèn lồng đỏ treo cao trên một ngôi lầu vẫn còn sáng.

# speaker:Kieu
# emotion:determined
# camera:CU_Kieu
Kiều: Kiều ngày trước bước vào đây mà không biết gì.

# speaker:Kieu
Kiều: Còn ta... ta biết. Và ta sẽ giả vờ như không biết — cho đến khi đủ sức lật ngược ván cờ này.

# speaker:NoiTam
Mình phải giả vờ chưa biết gì.

-> chapter_3
