// ============================================================
// CHƯƠNG 1 — GIA BIẾN
// Scene Unity: Chapter01_GiaBien (nhà họ Vương, đêm mưa)
// Chủ đề: cô học sinh vừa xuyên không đứng đúng vào cảnh "bán mình chuộc cha". Không đổi được việc phải đi,
//         nhưng đổi được cách đi: biết rõ mình bị bán cho ai, giá bao nhiêu, và không để hắn biết mình đã biết.
// Nhịp: Đêm mưa → Trong nhà (Mẹ Kiều / tủ gỗ / sân bị lục soát / bàn thờ) → Mã Giám Sinh đến
//       → Dạm hỏi (quan sát / đối đáp hẹn giờ / túi bạc / tùy tùng, thanh nghi ngờ) → Tờ khế → Lên đường.
// Bản cũ (tuyến tính, không có NoiTam): Legacy/Chapter1_GiaBien_v1.ink
// Lời dẫn tả cảnh đã thay bằng chuỗi ảnh chớp (# montage:<id>, xem MontageCatalog).
// ============================================================

=== chapter_1 ===
# chapter:1
# scene:VuongGia_DemMua
# music:sad_strings
# montage:c1_mo
# speaker:MeKieu
# emotion:crying
Mẹ Kiều: Bốn trăm lạng... Nhà ta có bán hết cũng không đủ. Trời muốn dồn người ta đến đường cùng sao?

# speaker:NoiTam
"Bán mình chuộc cha." Trong sách chỉ có bốn chữ. Ở đây, nó là một người mẹ đang khóc trước mặt mình.

# speaker:Kieu
# emotion:sad_controlled
Kiều: Mẹ đừng khóc. Cha và em còn chờ chúng ta. Con sẽ tìm cách.

# speaker:KieuThat
# gameplay:kieu_voice
Đêm ấy ta cũng nói đúng câu đó. Rồi ta ngồi yên chờ người ta đến định giá mình. Ngươi định ngồi chờ như ta sao?

# fx:none
# music:sad_strings
# speaker:NoiTam
# objective:Tìm hiểu tình cảnh gia đình trước khi khách đến
Không. Trước khi hắn đến, mình phải biết nhà này còn gì, mẹ biết gì, và kẻ sắp đến là ai.

-> c1_nha


=== c1_nha ===
// Hub khám phá trong nhà, trước khi Mã Giám Sinh đến. Mọi lựa chọn đều bằng hành động trong thế giới 3D.
* [Ngồi xuống bên Mẹ Kiều #explore:c1_me]
    -> c1_me
* [Mở tủ gỗ xem nhà còn những gì #explore:c1_tu]
    -> c1_tu
* [Quan sát khoảng sân quan sai vừa lục soát (giữ chuột phải) #explore:c1_san]
    -> c1_san
* [Thắp một nén hương trên bàn thờ #explore:c1_ban_tho]
    -> c1_ban_tho
* {c1_biet_moi && c1_viec >= 2} [Ra mở cổng: có người đang gõ cửa #explore:c1_cong]
    -> c1_ma_den


=== c1_me ===
~ c1_viec += 1
~ c1_biet_moi = true
# speaker:MeKieu
# emotion:crying
# camera:MCU_MeKieu
Mẹ Kiều: Quan sai cho ba ngày. Quá ba ngày không có bạc, cha con với thằng Quan phải chịu tra khảo.

# speaker:Kieu
# emotion:calm_probe
Kiều: Mẹ ơi, chiều nay bà mối đến nhà mình nói chuyện gì vậy? Con thấy bà ấy thì thầm với mẹ rất lâu.

# speaker:MeKieu
# emotion:hopeful_uncertain
Mẹ Kiều: Có một vị giám sinh ở phương xa muốn cầu thân, sính lễ thật hậu. Đêm nay... người ta sẽ đến xem mặt.

# speaker:NoiTam
# clue:c1_ba_moi
Sính lễ hậu. Đến xem mặt ngay trong đêm cha bị bắt. Không ai cưới vợ vội như thế.
-> c1_nha


=== c1_tu ===
~ c1_viec += 1
~ c1_manh_moi += 1
# speaker:Narrator
# camera:OTS_Kieu_Cabinet
# sfx:creak
Tủ gỗ gần trống. Còn đôi hoa tai bạc của mẹ và mấy nén bạc vụn: chưa được ba mươi lạng. Thiếu ba trăm bảy mươi.

# speaker:Narrator
# sfx:paper
Dưới đáy hộp là bản sao lời khai: "Thằng bán tơ khai đã gửi hàng gian ở nhà họ Vương." Không tên, không người làm chứng.

# speaker:NoiTam
# clue:c1_don_kien
Một lời khai không tên mà đủ trói hai người đàn ông đi. Phải giữ kỹ tờ này.
-> c1_nha


=== c1_san ===
~ c1_viec += 1
~ c1_manh_moi += 1
~ tinh_tao += 1
# speaker:Narrator
# camera:Wide_Courtyard
Hòm xiểng lật tung, tơ lụa nát trong bùn. Bên miệng giếng còn sót cuộn sổ hàng của Vương Ông, ướt sũng.

# speaker:NoiTam
# clue:c1_so_hang
Từng cuộn tơ đều ghi ngày mua, người bán. "Hàng gian" mà sổ sách đàng hoàng. Họ không cần sự thật, chỉ cần bạc.
-> c1_nha


=== c1_ban_tho ===
~ c1_viec += 1
# speaker:KieuThat
# camera:OTS_Kieu_Altar
# gameplay:kieu_voice
Ngày trước ta cũng đứng đây. Ta xin tổ tiên cho ta đủ can đảm để đi. Ta chưa bao giờ xin cho mình được quay về.

# speaker:NoiTam
Vậy thì mình sẽ xin thêm một điều. Đi, nhưng phải có ngày quay về.

~ soul(1)
# fx:none
# music:sad_strings
# speaker:KieuThat
Ngươi lạ thật. Cầu xin mà nghe như đang ra điều kiện.
-> c1_nha


=== c1_ma_den ===
# scene:VuongGia_MaDen
# sfx:knock
# music:tension_low
# speaker:Narrator
# camera:Reveal_MaGiamSinh
Cổng mở. Một người đàn ông áo gấm bước vào, theo sau là tên tùy tùng ôm hòm gỗ.

# speaker:MaGiamSinh
# emotion:smug
Mã Giám Sinh: Kẻ hèn họ Mã, nghe Vương gia gặp nạn nên mạo muội đến. Ta... có thể giúp.

~ nua(ts_ma_mat)
# speaker:TrangSach
# page_half:ts_ma_mat
"Quá niên trạc ngoại tứ tuần..."

# speaker:NoiTam
Là hắn. Mã Giám Sinh, "kẻ buôn người". Nhưng nếu mình gọi thẳng tội hắn ra, ai tin một đứa con gái mười lăm tuổi?

# speaker:MaGiamSinh
# emotion:polite_fake
Mã Giám Sinh: Bốn trăm lạng, đổi lấy một cuộc hôn nhân. Vương cô nương theo ta, tiền sẽ đến tay quan sai ngay trong đêm.

# speaker:NoiTam
# timer:9
Hắn đang chờ mình phản ứng. Phải trả lời ngay.

* [Cúi đầu: "Đa tạ công tử đã thương tình." #timeout]
    ~ c1_nghi_ngo = MAX(c1_nghi_ngo - 5, 0)
    # speaker:Kieu
    # emotion:controlled
    Kiều: Đa tạ công tử đã thương tình nhà họ Vương.

    # speaker:MaGiamSinh
    # emotion:satisfied
    Mã Giám Sinh: Ngoan lắm. Biết điều thì đời mới êm.

    # speaker:NoiTam
    "Biết điều." Hắn nói như khen một con vật vừa chịu ngồi yên.
* [Hỏi ngay: "Công tử là ai mà biết nhà ta gặp nạn nhanh vậy?"]
    ~ c1_nghi_ngo = MIN(c1_nghi_ngo + 15, 100)
    ~ tinh_tao += 1
    # speaker:Kieu
    # emotion:firm
    Kiều: Cha ta bị bắt mới từ trưa. Công tử ở phương xa, sao đã biết nhanh vậy?

    # speaker:MaGiamSinh
    # emotion:caught_off_guard
    # sfx:suspicion_up
    Mã Giám Sinh: Ơ... tin dữ thì bay nhanh. Bà mối báo cho ta, thế thôi.

    # speaker:NoiTam
    Hắn khựng mất một nhịp. Hắn biết trước. Nhưng mình vừa để lộ là mình đang đề phòng.
* [Không nói gì, nhìn thẳng vào mắt hắn.]
    ~ c1_nghi_ngo = MIN(c1_nghi_ngo + 5, 100)
    # speaker:Narrator
    # camera:CU_MaGiamSinh
    Kiều im lặng nhìn hắn. Mã Giám Sinh mỉm cười, nhưng ánh mắt hắn lướt từ mặt nàng xuống tay, xuống dáng đứng, như người đang ước cân.

    # speaker:NoiTam
    Đó không phải cái nhìn của người đi hỏi vợ. Đó là cái nhìn của người đi chợ.

- # speaker:Narrator
# scene:VuongGia_DamPhan
# camera:Table_Contract
# gameplay:tutorial_suspicion
Mã Giám Sinh không ngồi. Hắn đặt một túi bạc nặng trịch xuống bàn. Tên tùy tùng đứng chờ ngoài hiên.

-> c1_dam_phan_mo


=== c1_dam_phan_mo ===
# speaker:NoiTam
# objective:Tìm hiểu Mã Giám Sinh trước khi ký khế · đừng để hắn sinh nghi
Phải biết hắn là ai trước khi đặt bút. Nhưng hắn cũng đang nhìn mình. Nhìn lộ quá là hắn sẽ đề phòng.
-> c1_dam_phan


=== c1_dam_phan ===
// Hub dạm hỏi. Unity cộng c1_nghi_ngo khi Kiều quan sát lúc hắn đang nhìn lại; 100 = bị lộ (checkpoint).
* [Quan sát kỹ Mã Giám Sinh (giữ chuột phải) #explore:c1_quan_sat]
    -> c1_quan_sat
* [Đáp lời Mã Giám Sinh #explore:c1_hoi]
    -> c1_hoi
* [Nhìn túi bạc hắn đặt trên bàn #explore:c1_tui_bac]
    -> c1_tui_bac
* [Ra hiên, hỏi chuyện tên tùy tùng #explore:c1_tuy_tung]
    -> c1_tuy_tung
* {c1_hoi_xong} [Ngồi vào bàn, xem tờ khế #explore:c1_khe]
    -> c1_khe


=== c1_quan_sat ===
~ c1_manh_moi += 1
~ tinh_tao += 1
# speaker:Narrator
# camera:OTS_Kieu_MaGiamSinh
Ngón tay hắn gõ liên tục lên bàn, mắt chốc chốc liếc ra cổng. Nhắc chuyện cưới hỏi, hắn gật cho qua. Nhắc đến bạc, hắn đứng thẳng lưng.

# speaker:NoiTam
# clue:c1_ngon_tay
Người thật lòng cầu thân sẽ hỏi về người mình cưới. Hắn từ lúc vào nhà chỉ hỏi một câu: bao giờ ký giấy.
-> c1_dam_phan


=== c1_hoi ===
~ c1_hoi_xong = true
# speaker:MaGiamSinh
# emotion:polite_fake
# camera:OTS_Kieu_MaGiamSinh
Mã Giám Sinh: Cô nương có điều gì muốn hỏi ta trước khi thành thân chăng?

# speaker:NoiTam
# timer:10
Một câu thôi. Hỏi thế nào để hắn trả lời thật, mà không biết mình đang dò?

* [Hỏi khéo: "Quê nhà công tử ở đâu? Thiếp về làm dâu, sợ vụng."]
    ~ tinh_tao += 1
    ~ c1_manh_moi += 1
    # speaker:Kieu
    # emotion:calm_probe
    Kiều: Quê nhà công tử ở đâu? Thiếp về làm dâu, sợ phụ mẫu bên ấy chê vụng.

    # speaker:MaGiamSinh
    # emotion:smug
    Mã Giám Sinh: Huyện Lâm Thanh, cũng gần thôi. Phụ mẫu ta... ở nhà đã có người lo liệu cả.

    # speaker:NoiTam
    # clue:c1_lam_thanh
    "Hỏi tên rằng: Mã Giám Sinh. Hỏi quê rằng: huyện Lâm Thanh cũng gần." Đúng từng chữ trong sách. Và vẫn không có cha mẹ nào cả.
* [Hỏi thẳng: "Công tử cưới vợ, hay mua người?"]
    ~ tinh_tao += 1
    ~ c1_nghi_ngo = MIN(c1_nghi_ngo + 30, 100)
    # speaker:Kieu
    # emotion:cold
    Kiều: Công tử cưới vợ, hay mua người?

    # speaker:MaGiamSinh
    # emotion:irritated_hidden
    # sfx:suspicion_up
    Mã Giám Sinh: Cô nương nói thế là phụ lòng tốt của ta rồi. Bốn trăm lạng mà gọi là mua, thì cả thiên hạ này đều là chợ.

    # speaker:NoiTam
    Hắn không phủ nhận. Hắn chỉ trách mình nói khó nghe. Nhưng giờ hắn đã cảnh giác.
* [Hỏi chuyện tiền: "Bạc đến tay cha ta trước, hay sau khi ta đi?"]
    ~ tinh_tao += 2
    ~ c1_hoi_tien = true
    ~ c1_nghi_ngo = MIN(c1_nghi_ngo + 5, 100)
    # speaker:Kieu
    # emotion:firm
    Kiều: Ta chỉ hỏi một điều. Bạc đến tay cha ta trước, hay sau khi ta bước khỏi cửa này?

    # speaker:MaGiamSinh
    # emotion:caught_off_guard
    Mã Giám Sinh: ...Trước.

    # speaker:Kieu
    Kiều: Vậy xin công tử ghi điều đó vào khế.

    # speaker:Narrator
    Nụ cười của Mã Giám Sinh khựng lại trong một nhịp.
* [Không hỏi gì, để Mẹ nói thay. #timeout]
    ~ c1_nghi_ngo = MAX(c1_nghi_ngo - 10, 0)
    # speaker:MeKieu
    # emotion:hopeful_uncertain
    Mẹ Kiều: Con tôi còn nhỏ, chưa biết hỏi han. Xin công tử thương nó, đối xử với nó cho tử tế.

    # speaker:MaGiamSinh
    # emotion:satisfied
    Mã Giám Sinh: Bà cứ yên tâm. Về đến nơi, khắc có người chăm nom nàng chu đáo.

    # speaker:NoiTam
    "Khắc có người chăm nom." Không phải "ta sẽ chăm nom". Mình nghe rõ chữ ấy.

- # speaker:MaGiamSinh
# emotion:businesslike
Mã Giám Sinh: Thôi, chuyện đã rõ. Mời cô nương ngồi vào bàn, ta đã soạn sẵn giấy tờ.
-> c1_dam_phan


=== c1_tui_bac ===
~ c1_manh_moi += 1
~ c1_bac_dau = true
# speaker:Narrator
# camera:Table_Contract
Miệng túi bạc hé ra. Những nén bạc mới tinh, xếp ngay ngắn, nén nào cũng có một con dấu đỏ nhỏ.

# speaker:NoiTam
# clue:c1_bac_phuc_ky
Phúc Ký, dấu một hiệu bạc. Bạc hỏi vợ thường là bạc nhà để dành, cũ, lẫn đủ loại. Đây là bạc vừa rút ra một lượt, như có ai đưa tiền cho hắn đi... mua hàng.
-> c1_dam_phan


=== c1_tuy_tung ===
~ c1_manh_moi += 1
# speaker:TuyTung
# emotion:annoyed
# camera:OTS_Kieu_TuyTung
Tùy tùng: Cô nương ra đây làm gì? Mưa ướt hết bây giờ. Chủ ta không thích hàng... à, không thích người ốm.

# speaker:Kieu
# emotion:controlled
Kiều: Anh theo công tử lâu chưa? Công tử đã từng cưới vợ lần nào chưa?

# speaker:TuyTung
# emotion:amused
Tùy tùng: Cưới? Năm nay là lần thứ... Hừ, cô hỏi làm gì. Chủ ta hỏi vợ ở nhiều nơi lắm, ai mà đếm cho xuể.

~ c1_nghi_ngo = MIN(c1_nghi_ngo + 10, 100)
# speaker:NoiTam
# clue:c1_nhieu_vo
"Hỏi vợ ở nhiều nơi." Không ai cưới nhiều vợ đến mức phải đếm. Trừ khi đó là một nghề.

# speaker:TuyTung
# emotion:suspicious
# sfx:suspicion_up
Tùy tùng: Vào trong đi. Chủ ta mà thấy cô đứng đây nói chuyện với ta thì không hay đâu.
-> c1_dam_phan


=== c1_khe ===
# scene:VuongGia_BanKyKhe
# speaker:Narrator
# camera:Table_Contract
Mã Giám Sinh đặt lên bàn một tờ khế đã viết sẵn. Dòng chữ "hôn ước" nằm trên cùng, nhưng phía dưới có những câu chữ cố tình viết nhỏ.

# speaker:MaGiamSinh
# emotion:polite_fake
Mã Giám Sinh: Chỉ là thủ tục. Cô nương điểm chỉ vào đây là xong.

* [Đọc kỹ tờ khế trước khi ký.]
    -> c1_doc_khe
* [Ký ngay để bạc được đưa đi cứu cha.]
    -> c1_ky_ngay


=== c1_doc_khe ===
~ tinh_tao += 1
~ doc_ky_khe = true
# speaker:Narrator
# gameplay:document_khe
Kiều kéo ngọn đèn lại gần và đọc từng chữ.

# speaker:Kieu
# emotion:cold
Kiều: Hôn ước mà lại viết như giấy mua bán súc vật. "Người nhận có toàn quyền định đoạt nơi ở và công việc của người giao." Công tử dùng văn từ thật mới lạ.

# speaker:MaGiamSinh
# emotion:annoyed
Mã Giám Sinh: Chỉ là lệ buôn... à, lệ làm giấy ở chỗ ta.

# speaker:NoiTam
# timer:9
Mình đã biết đây là giấy bán người. Làm gì với điều đó bây giờ?

* {c1_hoi_tien} [Đòi ghi thêm: "Bạc giao đủ cho quan sai trước khi ta lên đường."]
    ~ tinh_tao += 1
    ~ c1_nghi_ngo = MIN(c1_nghi_ngo + 10, 100)
    # speaker:Kieu
    # emotion:firm
    Kiều: Công tử đã hứa bạc đến trước. Xin ghi thêm một dòng: bạc giao đủ cho quan sai, cha ta được thả, rồi ta mới lên đường.

    # speaker:MaGiamSinh
    # emotion:irritated_hidden
    Mã Giám Sinh: ...Được. Ghi thì ghi.

    # speaker:NoiTam
    Một dòng chữ nhỏ. Nhưng ít nhất lần này, tiền cứu cha không phụ thuộc vào lòng tốt của hắn.
* [Đòi xóa câu "toàn quyền định đoạt".]
    ~ c1_nghi_ngo = MIN(c1_nghi_ngo + 25, 100)
    # speaker:Kieu
    # emotion:cold
    Kiều: Câu này phải xóa. Ta về làm vợ, không phải làm đồ vật để người ta định đoạt.

    # speaker:MaGiamSinh
    # emotion:angry
    # sfx:suspicion_up
    Mã Giám Sinh: Không xóa được. Không ký thì thôi, ta đi tìm nhà khác. Bốn trăm lạng thiếu gì người nhận.

    # speaker:MeKieu
    # emotion:crying
    Mẹ Kiều: Công tử, xin công tử bớt giận...

    # speaker:NoiTam
    Hắn biết mình không có đường lui. Cãi nữa chỉ khiến hắn nhìn mình như kẻ thù.
* [Giả vờ không hiểu, ghi nhớ từng chữ. #timeout]
    ~ c1_nghi_ngo = MAX(c1_nghi_ngo - 10, 0)
    # speaker:Kieu
    # emotion:controlled
    Kiều: Chữ nghĩa thiếp đọc không thông. Công tử đã nói là lệ, thì chắc là lệ.

    # speaker:MaGiamSinh
    # emotion:satisfied
    Mã Giám Sinh: Phải, phải. Đàn bà con gái không cần bận tâm mấy chuyện giấy tờ.

    # speaker:NoiTam
    Cứ để hắn nghĩ mình là con cừu. Con cừu nhớ từng chữ trên tờ giấy bán nó.

- # speaker:Narrator
Kiều điểm chỉ. Chưa lật được bàn, nhưng nàng biết rõ mình đang bước vào đâu.
-> c1_decision


=== c1_ky_ngay ===
~ tu_trong -= 1
# speaker:Narrator
Kiều cầm bút. Mực chạm giấy nhanh hơn cả một hơi thở.

# speaker:Kieu
# emotion:pained
Kiều: Một chữ của ta đổi lấy mạng người nhà. Nếu đây là cái giá duy nhất... ta trả.

# speaker:KieuThat
# gameplay:kieu_voice
Ta cũng đã ký nhanh như thế. Ta tưởng như vậy là hiếu. Hóa ra chỉ là sợ.

# fx:none
# music:tension_low
# speaker:NoiTam
Lần sau, mình sẽ đọc trước khi ký. Không bao giờ ký thứ gì mà mình chưa đọc nữa.
-> c1_decision


=== c1_decision ===
# speaker:MeKieu
# emotion:crying
# camera:MCU_MeKieu
Mẹ Kiều: Con ơi... hay là thôi. Mẹ không thể nhìn con tự bán đời mình.

# speaker:Kieu
# emotion:soft_firm
# camera:CU_Kieu
Kiều: Con đi không phải vì con rẻ hơn bốn trăm lạng. Con đi để cứu cha và em. Còn chuyện đời con... con chưa giao cho ai cả.

~ soul(tu_trong / 2)
{tinh_tao >= 3:
    ~ nhan(ts_hieu)
    # speaker:TrangSach
    # page:ts_hieu
    "Duyên hội ngộ, đức cù lao, bên tình bên hiếu bên nào nặng hơn?"
    # speaker:KieuThat
    Câu ấy ta đã tự hỏi suốt một đêm. Ngươi giữ lấy nó.
}

{c1_nghi_ngo >= 60:
    ~ nghi_ngo = 15
    # scene:VuongGia_RaDi
    # music:sad_strings
    # montage:c1_sang
    # speaker:MaGiamSinh
    # emotion:cold
    Mã Giám Sinh: Nàng lanh lợi quá. Đường xa, ta sẽ để mắt tới nàng từng bước.

    # speaker:NoiTam
    Hắn đã bắt đầu đề phòng mình. Trên đường đi phải cẩn thận gấp đôi.
- else:
    # scene:VuongGia_RaDi
    # music:sad_strings
    # montage:c1_sang
    # speaker:MaGiamSinh
    # emotion:satisfied
    Mã Giám Sinh: Giờ thì lên đường thôi, nương tử.
}

# speaker:Kieu
# emotion:determined
# objective:Từ biệt Mẹ Kiều và ra cổng lên đường
Kiều: Đừng gọi ta như thế vội.

-> c1_ra_di_hub


=== c1_ra_di_hub ===
* [Ôm Mẹ lần cuối #explore:c1_me_cuoi]
    -> c1_me_cuoi
* [Bước ra cổng, theo Mã Giám Sinh #explore:c1_ra_di]
    -> c1_ra_di


=== c1_me_cuoi ===
~ c1_tam_biet = true
# speaker:MeKieu
# emotion:crying
# camera:Hug_MeKieu
Mẹ Kiều: Con đi đường xa, nhớ giữ mình. Mẹ chỉ có cái này cho con.

# speaker:Narrator
Bà gỡ đôi hoa tai bạc, đặt vào lòng bàn tay Kiều, gập những ngón tay nàng lại.

# speaker:NoiTam
Ở nhà mình, mẹ cũng hay nắm tay mình như thế mỗi lần mình đi thi.

# speaker:Kieu
# emotion:soft_firm
Kiều: Mẹ đợi con. Con sẽ về.
-> c1_ra_di_hub


=== c1_ra_di ===
{c1_manh_moi >= 4:
    # montage:c1_ra_di
    # speaker:NoiTam
    Bạc dấu Phúc Ký. Tờ khế bán người. Một kẻ "hỏi vợ ở nhiều nơi". Mình chưa có bằng chứng, nhưng mình đã có câu hỏi.
- else:
    # montage:c1_ra_di
    # speaker:NoiTam
    Mình còn biết quá ít về hắn. Trên đường đi phải nhìn kỹ hơn, hỏi khéo hơn.
}

-> chapter_2


// ============================================================
// CHƯƠNG 2 — TRÊN ĐƯỜNG THEO MÃ GIÁM SINH: xem Chapter2_TrenDuongTheoMaGiamSinh.ink
