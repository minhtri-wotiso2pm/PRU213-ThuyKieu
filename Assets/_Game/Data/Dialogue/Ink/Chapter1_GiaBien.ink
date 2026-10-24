// ============================================================
// CHƯƠNG 1 — GIA BIẾN
// Scene Unity: Chapter01_GiaBien (nhà họ Vương, đêm mưa)
// Chủ đề: cô học sinh vừa xuyên không đứng đúng vào cảnh "bán mình chuộc cha". Không đổi được việc phải đi,
//         nhưng đổi được cách đi: biết rõ mình bị bán cho ai, giá bao nhiêu, và không để hắn biết mình đã biết.
// Nhịp: Đêm mưa → Chọn cách tìm sự thật trong nhà → Mã Giám Sinh đến cắt ngang
//       → Đối đáp hẹn giờ → Bóc trần tờ khế → Từ biệt → Lên đường.
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
Mẹ Kiều: Bốn trăm lạng... Bán hết nhà cũng không đủ. Biết làm sao bây giờ?

# speaker:Kieu
# emotion:sad_controlled
Kiều: Mẹ đừng khóc. Cha và em còn chờ chúng ta. Con sẽ tìm cách.

# speaker:KieuThat
# gameplay:kieu_voice
Ta cũng từng nói như thế, rồi ngồi chờ người ta đến định giá. Ngươi cũng định chờ sao?

# fx:none
# music:sad_strings
# speaker:NoiTam
# objective:Tìm bằng chứng về vụ án trước khi người của bà mối tới
Không. Muốn cứu người nhà, trước hết mình phải biết họ bị bắt dựa vào cái gì.

-> c1_nha


=== c1_nha ===
// Hai việc, làm theo thứ tự nào cũng được. Sau việc đầu tiên có tiếng gõ cổng: người chơi tự quyết khi nào ra mở,
// làm nốt việc kia trước thì vào đàm phán với cả hai lá bài.
* {not c1_me} [Hỏi Mẹ Kiều quan sai đã lấy gì làm chứng #explore:c1_me]
    -> c1_me
* {not c1_tu} [Tìm bản sao hồ sơ quan sai để lại #explore:c1_tu]
    -> c1_tu
* {not c1_san} [Quan sát khoảng sân bị quan sai lục soát #explore:c1_san]
    -> c1_san
+ {c1_me || c1_tu} [Ra mở cổng: có người đang gõ cửa #explore:c1_cong]
    -> c1_ma_den


=== c1_me ===
~ c1_viec += 1
~ c1_biet_moi = true
~ c1_manh_moi += 1
~ c1_hoi_tien = true
{
- c1_tu:
    // Đã lục tủ xong thì tiếng gõ cổng đã vang: Mẹ đứng dậy, vừa nói vội vừa giục ra mở cổng.
    # speaker:MeKieu
    # emotion:crying
    # camera:MCU_MeKieu
    Mẹ Kiều: (liếc ra cổng) Bà mối nói vị giám sinh ấy chịu đưa đủ bốn trăm lạng, bạc tới tay quan sai trước con mới phải đi... Thôi, ra mở cổng đi con, người ta gõ mãi rồi!
- else:
    # speaker:MeKieu
    # emotion:crying
    # camera:MCU_MeKieu
    Mẹ Kiều: Bà mối nói vị giám sinh ấy chịu đưa đủ bốn trăm lạng. Bạc tới tay quan sai trước, con mới phải rời nhà.
}

# speaker:NoiTam
# clue:c1_ba_moi
Hắn biết đúng giá chuộc người và tới ngay trong đêm. Phải giữ hắn đúng lời hứa ấy: bạc đi trước, người đi sau.
-> c1_nha


=== c1_tu ===
~ c1_viec += 1
~ c1_manh_moi += 1
~ c1_biet_moi = true
# speaker:Narrator
# camera:OTS_Kieu_Cabinet
# sfx:creak
Kiều lật đáy tủ. Sau đôi hoa tai bạc là bản sao lời khai: "Thằng bán tơ khai đã gửi hàng gian ở nhà họ Vương." Không tên, không người làm chứng.

# speaker:NoiTam
# clue:c1_don_kien
Lời khai không tên mà bà mối đã tìm sẵn người chịu đúng bốn trăm lạng. Vụ án và cuộc mua bán này có thể cùng một đường dây.
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


=== c1_ma_den ===
~ c1_ca_hai = c1_me && c1_tu
# scene:VuongGia_MaDen
# sfx:knock
# music:tension_low
# speaker:Narrator
# camera:Reveal_MaGiamSinh
{c1_de_cho:Tiếng gõ dồn dập đến mức Mẹ phải kéo tay Kiều ra cổng. }Cổng mở. Một người đàn ông áo gấm bước vào, theo sau là tên tùy tùng ôm hòm gỗ.

{c1_de_cho:
    # speaker:MaGiamSinh
    # emotion:irritated_hidden
    Mã Giám Sinh: (rũ nước mưa trên tay áo) Đứng ngoài mưa ngần ấy, ta còn tưởng nhà này không muốn cứu người. Kẻ hèn họ Mã... có thể giúp, nếu Vương gia còn cần.
- else:
    # speaker:MaGiamSinh
    # emotion:smug
    Mã Giám Sinh: Kẻ hèn họ Mã, nghe Vương gia gặp nạn nên mạo muội đến. Ta... có thể giúp.
}

~ nua(ts_ma_mat)
# speaker:TrangSach
# page_half:ts_ma_mat
"Quá niên trạc ngoại tứ tuần..."

# speaker:NoiTam
Là Mã Giám Sinh. Mình biết hắn là kẻ buôn người, nhưng nói ra lúc này ai sẽ tin?

{c1_ca_hai:
    # speaker:NoiTam
    Lời hứa của bà mối, bản khai không tên. Hai lá bài trong tay áo. Giờ chỉ cần biết lúc nào lật.
- else:
    # speaker:NoiTam
    Mới nắm được một nửa câu chuyện. Phải dè chừng từng lời.
}

# speaker:MaGiamSinh
# emotion:polite_fake
Mã Giám Sinh: Bốn trăm lạng, đổi lấy một cuộc hôn nhân. Vương cô nương theo ta, tiền sẽ đến tay quan sai ngay trong đêm.

# speaker:Narrator
# scene:VuongGia_DamPhan
# camera:Table_Contract
# gameplay:tutorial_suspicion
Mã Giám Sinh không ngồi. Hắn đặt một túi bạc nặng trịch xuống bàn. Tên tùy tùng đứng chờ ngoài hiên.

-> c1_dam_phan_mo


=== c1_dam_phan_mo ===
# speaker:NoiTam
# objective:Đợi Mã quay đi, giữ chuột phải nhìn túi bạc · hoặc đối chất ngay
Nhìn trộm dấu trên túi bạc khi hắn quay sang Mẹ, hoặc đối chất ngay.
-> c1_dam_phan


=== c1_dam_phan ===
// Quan sát bạc là cơ hội có rủi ro nhưng không bắt buộc; đối chất luôn đưa câu chuyện đi tiếp.
* [Quan sát dấu trên túi bạc khi Mã quay đi (giữ chuột phải) #explore:c1_tui_bac]
    -> c1_tui_bac
* [Đối chất với Mã Giám Sinh #explore:c1_hoi]
    -> c1_hoi


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

{c1_nghi_ngo >= 50:
    # speaker:NoiTam
    Hắn đã đề phòng. Lúc này mà hỏi khéo, hắn sẽ nghe ra ngay mình đang dò.
}

# speaker:NoiTam
# timer:10
Một câu thôi. Hỏi thế nào để hắn trả lời thật, mà không biết mình đang dò?

* {c1_nghi_ngo < 50} [Hỏi khéo: "Quê nhà công tử ở đâu? Thiếp về làm dâu, sợ vụng."]
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
    "Hỏi tên rằng: Mã Giám Sinh. Hỏi quê rằng: huyện Lâm Thanh cũng gần." Đúng từng chữ trong sách. Hắn vẫn tránh nhắc đến gia đình.
* [Hỏi thẳng: "Công tử đến hỏi ta, sao chưa hỏi ta lấy một câu?"]
    ~ c1_nghi_ngo = MIN(c1_nghi_ngo + 20, 100)
    # speaker:Kieu
    # emotion:cold
    Kiều: Công tử đến hỏi ta, sao từ lúc vào nhà chưa hỏi ta lấy một câu?

    # speaker:MaGiamSinh
    # emotion:irritated_hidden
    # sfx:suspicion_up
    Mã Giám Sinh: Nhà nàng cần bạc, ta có bạc. Chuyện cần nói chẳng phải đã rõ rồi sao?
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
-> c1_dam_phan_ky


=== c1_dam_phan_ky ===
# speaker:NoiTam
# objective:Đọc tờ khế trước khi điểm chỉ
Hắn đã soạn giấy trước cả khi gặp mình. Vậy đây chưa bao giờ là một cuộc hỏi cưới.
* [Ngồi vào bàn, xem tờ khế #explore:c1_khe]
    -> c1_khe


=== c1_tui_bac ===
~ c1_manh_moi += 1
~ c1_bac_dau = true
~ tinh_tao += 1
# speaker:Narrator
# camera:Table_Contract
Mã quay sang phía Mẹ. Kiều chỉ kịp nhìn qua miệng túi: những nén bạc mới tinh đều in cùng một dấu đỏ.

# speaker:NoiTam
# clue:c1_bac_phuc_ky
Phúc Ký. Cả túi vừa được rút từ một hiệu bạc, chuẩn bị sẵn cho một vụ mua bán.
-> c1_dam_phan


=== c1_tuy_tung ===
~ c1_manh_moi += 1
# speaker:TuyTung
# emotion:annoyed
# camera:CU_TuyTung
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
Mã Giám Sinh rút tờ khế viết sẵn ra khỏi hòm. Chữ "hôn ước" nằm trên cùng, phía dưới là những dòng cố tình viết nhỏ.

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

* {c1_ca_hai} [Dùng cả hai: đòi bạc giao trước, và khẽ đặt bản khai lên bàn.]
    ~ tinh_tao += 2
    ~ c1_bac_truoc = true
    ~ c1_ma_lo = true
    ~ c1_nghi_ngo = MIN(c1_nghi_ngo + 5, 100)
    # speaker:Kieu
    # emotion:firm
    Kiều: Bà mối đã hứa bạc giao trước, xin ghi rõ vào đây. Còn bản khai này... chắc công tử không lạ gì.

    # speaker:MaGiamSinh
    # emotion:caught_off_guard
    # sfx:suspicion_up
    Mã Giám Sinh: Ghi thì ghi. Còn giấy má quan sai... bên Lâm Tri người ta chỉ dặn ta mang bạc tới, ta biết gì đâu.

    # speaker:NoiTam
    # clue:c1_ma_lo
    Lâm Tri. Hắn vừa tự khai nơi hắn đưa mình tới. Và lần này, bạc cứu cha đi trước mình một bước.
* {c1_me && c1_hoi_tien} [Giữ lời bà mối: "Bạc giao đủ trước khi ta lên đường."]
    ~ tinh_tao += 1
    ~ c1_bac_truoc = true
    ~ c1_nghi_ngo = MIN(c1_nghi_ngo + 10, 100)
    # speaker:Kieu
    # emotion:firm
    Kiều: Bà mối đã hứa bạc giao trước. Xin ghi rõ: bạc tới tay quan sai, cha và em ta được thả, rồi ta mới lên đường.

    # speaker:MaGiamSinh
    # emotion:irritated_hidden
    Mã Giám Sinh: ...Được. Ghi thì ghi.

    # speaker:NoiTam
    Một dòng chữ nhỏ. Nhưng ít nhất lần này, tiền cứu cha không phụ thuộc vào lòng tốt của hắn.
* {c1_tu} [Đặt bản khai cạnh tờ khế: "Ai cho công tử biết hồ sơ vụ án?"]
    ~ tinh_tao += 1
    ~ c1_ma_lo = true
    ~ c1_nghi_ngo = MIN(c1_nghi_ngo + 15, 100)
    # speaker:Kieu
    # emotion:firm
    Kiều: Bản khai không tên này vừa đủ bắt cha ta. Tờ khế của công tử lại được viết sẵn. Ai đã cho công tử biết hồ sơ vụ án?

    # speaker:MaGiamSinh
    # emotion:caught_off_guard
    # sfx:suspicion_up
    Mã Giám Sinh: Bà mối lo liệu mọi chuyện. Bên Lâm Tri người ta chỉ dặn ta mang bạc tới. Cô nương đừng suy diễn.

    # speaker:NoiTam
    # clue:c1_ma_lo
    Hắn chối quá nhanh, và lỡ nhắc "bên Lâm Tri". Bản khai, bà mối và túi bạc chắc chắn nối với nhau.
* [Đòi xóa câu "toàn quyền định đoạt".]
    ~ c1_nghi_ngo = MIN(c1_nghi_ngo + 25, 100)
    # speaker:Kieu
    # emotion:cold
    Kiều: Câu này phải xóa. Ta về làm vợ, không phải làm đồ vật để người ta định đoạt.

    # speaker:MaGiamSinh
    # emotion:angry
    # sfx:suspicion_up
    Mã Giám Sinh: Không xóa được. Không ký thì thôi, ta đi tìm nhà khác. Bốn trăm lạng thiếu gì người nhận.

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
# gameplay:diem_chi
Mã Giám Sinh đẩy hộp son lại gần. Chỉ còn chờ một dấu tay.

# speaker:Narrator
Kiều điểm chỉ. Chưa lật được bàn, nhưng nàng biết rõ mình đang bước vào đâu.
-> c1_decision


=== c1_ky_ngay ===
~ tu_trong -= 1
# speaker:Kieu
# emotion:pained
# gameplay:diem_chi
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
* {not c1_tam_biet} [Ôm Mẹ lần cuối #explore:c1_me_cuoi]
    -> c1_me_cuoi
* {c1_tam_biet} [Bước ra cổng, theo Mã Giám Sinh #explore:c1_ra_di]
    -> c1_ra_di


=== c1_me_cuoi ===
~ c1_tam_biet = true
# speaker:MeKieu
# emotion:crying
# camera:Hug_MeKieu
Mẹ Kiều: Con đi đường xa, nhớ giữ mình. Mẹ chỉ còn đôi hoa tai này cho con.

# speaker:NoiTam
Ở nhà mình, mẹ cũng hay nắm tay mình như thế mỗi lần mình đi thi.

# speaker:Kieu
# emotion:soft_firm
Kiều: Mẹ đợi con. Con sẽ về.
-> c1_ra_di_hub


=== c1_ra_di ===
{c1_manh_moi >= 2:
    # montage:c1_ra_di
    # speaker:NoiTam
    Lời khai vô danh. Bạc dấu Phúc Ký. Tờ khế bán người. Mình chưa đủ bằng chứng, nhưng đã biết phải lần theo đâu.
- else:
    # montage:c1_ra_di
    # speaker:NoiTam
    Mình còn biết quá ít về hắn. Trên đường đi phải nhìn kỹ hơn, hỏi khéo hơn.
}

// Ra cổng với cả hai lá bài thì Mã bớt đề phòng; vội mở cổng khi mới biết một nửa thì hắn để ý hơn.
{c1_ca_hai:
    ~ nghi_ngo = MAX(nghi_ngo - 10, 0)
- else:
    ~ nghi_ngo = nghi_ngo + 5
}

// Câu người chơi đã nói về Kiều trong lớp học quay lại, từ chính miệng Kiều.
{
- pl_che_kieu == 1:
    # speaker:KieuThat
    # gameplay:kieu_voice
    "Em sẽ làm tốt hơn Kiều nhiều." Ngươi đã nói thế, ở một nơi rất xa. Giờ tờ khế mang dấu tay ngươi. Còn thấy dễ không?
- pl_che_kieu == 2:
    # speaker:KieuThat
    # gameplay:kieu_voice
    "Phải có cách khác, chứ không phải bán chính mình." Ngươi đã tìm suốt đêm nay. Giờ thì ngươi biết: có những đêm, cách khác duy nhất là bước đi mà mắt vẫn mở.
- else:
    # speaker:KieuThat
    # gameplay:kieu_voice
    Ngươi từng chê ta cam chịu. Đêm nay ngươi cũng điểm chỉ, cũng bước ra cổng. Cam chịu với chịu đựng, giờ ngươi phân biệt được chưa?
}

# fx:none
# speaker:NoiTam
Mình đã nói câu ấy dễ dàng biết bao. Giờ mới hiểu cái giá của nó.

-> chapter_2


// ============================================================
// CHƯƠNG 2 — TRÊN ĐƯỜNG THEO MÃ GIÁM SINH: xem Chapter2_TrenDuongTheoMaGiamSinh.ink
