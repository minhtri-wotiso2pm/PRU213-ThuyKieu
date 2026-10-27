// ============================================================
// CHƯƠNG 6 — KIM TRỌNG
// Scene Unity: Chapter06_KimTrong (chùa Bạch Vân → thư phòng nhà cậu Kim Trọng → phố huyện Lâm Tri)
// Chủ đề: Kim Trọng không còn là một cái tên trong sách. Hai người cùng lập hồ sơ cho Chương 7.
// Nhịp: Gặp ở chùa (độ tin tưởng) → Đọc khế ước → Đối chiếu giấy tờ → Phố huyện: tìm nhân chứng, đưa bằng chứng
//       → Sắp xếp hồ sơ → Đêm trong thư phòng → Móc nối Chương 7.
// Unity ghi: c6_doi_chieu (0–3). Hồ sơ cho Chương 7: ev_khe_uoc, ev_nhan_chung, ev_giay_no, ev_tien.
// ============================================================

=== chapter_6 ===
# chapter:6
# scene:Temple_Dawn
# music:temple
# speaker:Narrator
# camera:Wide_TempleYard
Sáng. Sương đêm đã tan. Kiều tỉnh dậy dưới mái hiên chùa Bạch Vân, trên người đắp một tấm chăn sờn. Tiếng mõ gõ đều trong chính điện.

{c4_ru_co_lam && c5_co_lam_thoat:
    # speaker:Narrator
    Cô gái áo lam ngủ say bên cạnh, tay vẫn nắm chặt vạt áo Kiều.
}

# speaker:NoiTam
Mình còn sống. Khế ước còn đây.

# speaker:Narrator
Ngoài cổng có tiếng vó ngựa dừng lại, rồi tiếng bước chân vội vã trên sân đá.

# speaker:KimTrong
# emotion:caught_off_guard
# camera:CU_KimTrong
Kim Trọng: Sư cụ nhắn có một cô gái ngất trước cổng chùa đêm qua...

# speaker:KimTrong
# emotion:caught_off_guard
Kim Trọng: Kiều? Vì sao nàng lại ở đây?

# speaker:KimTrong
# emotion:pained
Kim Trọng: Ta nghe tin nàng nhận lời về làm vợ một viên giám sinh. Ta đuổi theo suốt ba ngày... chỉ để được nhìn nàng một lần. Đám rước dâu đâu? Phu quân của nàng đâu?

# speaker:Kieu
# emotion:determined
# camera:CU_Kieu
Kiều: Ta không có thời gian giải thích.

# speaker:Kieu
# emotion:determined
Kiều: Ta cần chàng giúp ta cứu gia đình.

# speaker:NoiTam
# timer:14
Chàng đang nhìn mình như nhìn một người vừa từ cõi chết trở về. Nói gì với chàng?

* {ts_kim_trong == 2} [Lật trang "Người quốc sắc, kẻ thiên tài...": đọc lại câu thơ hai người từng đối nhau #page:ts_kim_trong]
    ~ dung(ts_kim_trong, 5)
    ~ kt_tin_tuong += 3
    # speaker:Kieu
    # emotion:pained
    Kiều: "Tình trong như đã, mặt ngoài còn e." Chàng còn nhớ câu ấy không? Hôm ấy chàng nhặt được cây thoa của ta bên bức tường. Giờ thì ta không còn gì để e nữa: Mã Giám Sinh đã bán ta.
    # speaker:KimTrong
    # emotion:angry
    Kim Trọng: Đúng là nàng... Hắn bán nàng? Hắn dám...
* [Nói thật: "Không có đám cưới nào. Hắn đã bán ta."]
    ~ kt_tin_tuong += 2
    # speaker:Kieu
    # emotion:pained
    Kiều: Không có đám cưới nào cả. Mã Giám Sinh mua ta bốn trăm lạng rồi bán lại cho một lầu xanh ở Lâm Tri. Đêm qua ta trốn ra.
    # speaker:KimTrong
    # emotion:angry
    Kim Trọng: Bán...? Hắn... hắn dám...
* [Không nói gì, chỉ đưa tờ khế ước ra. #timeout]
    ~ kt_tin_tuong += 1
    # speaker:Narrator
    Kiều không nói gì. Nàng rút trong áo ra xấp giấy buộc dây đỏ, đặt vào tay chàng.
* ["Chàng có tin... ta không còn là Kiều mà chàng từng quen?"]
    ~ kt_biet_xuyen_khong = true
    ~ fate(10)
    # speaker:Kieu
    # emotion:thoughtful
    Kiều: Chàng có tin không... nếu ta nói người đứng trước mặt chàng không hẳn là Kiều mà chàng từng quen? Ta đến từ một nơi rất xa. Ta đã đọc trước câu chuyện này.
    # speaker:KimTrong
    # emotion:caught_off_guard
    Kim Trọng: Nàng... sốt rồi. Chạy suốt đêm trong sương lạnh, nàng...
    # speaker:Kieu
    # emotion:firm
    Kiều: Ta không sốt. Nhưng chuyện đó để sau. Xin chàng đọc cái này trước.

- (g_gap)
# speaker:Narrator
# gameplay:document_deal_kt
Kim Trọng mở xấp giấy. Đọc đến đâu, mặt chàng tái đến đó.

# speaker:KimTrong
# emotion:angry
# camera:CU_KimTrong
Kim Trọng: "Mỗi người bốn trăm năm mươi lạng." Ba cái tên đã gạch... Đây không phải giấy cưới. Đây là sổ buôn người.

# speaker:KimTrong
# emotion:pained
Kim Trọng: Nếu ta không đi Liêu Dương... nếu ta ở lại thêm một tháng...

* [An ủi: "Không phải lỗi của chàng."]
    ~ kt_tin_tuong += 1
    # speaker:Kieu
    # emotion:calm_probe
    Kiều: Không phải lỗi của chàng. Kẻ có lỗi là kẻ đặt bút ký tờ giấy này.
    # speaker:KimTrong
    Kim Trọng: ...Nàng vẫn vậy. Đến lúc này còn nghĩ cho người khác.
* {ts_hieu == 2} [Lật trang "Duyên hội ngộ, đức cù lao...": "Bên tình bên hiếu, ta đã chọn. Giờ ta cần chàng." #page:ts_hieu]
    ~ dung(ts_hieu, 5)
    ~ kt_tin_tuong += 2
    # speaker:Kieu
    # emotion:soft_firm
    Kiều: Bên tình bên hiếu, đêm ấy ta đã phải chọn một bên. Không ai có lỗi vì chàng đi xa. Nhưng hôm nay ta không phải chọn nữa, nếu chàng ở đây.
    # speaker:KimTrong
    # emotion:determined
    Kim Trọng: Ta ở đây. Lần này ta không đi đâu cả.
* [Thẳng thắn: "Tự trách chẳng cứu được ai. Giúp ta đi."]
    ~ kt_tin_tuong += 1
    # speaker:Kieu
    # emotion:firm
    Kiều: Tự trách chẳng cứu được ai. Cha ta còn trong ngục, ba cô gái kia còn trong lầu. Chàng giúp ta, hay không?
    # speaker:KimTrong
    # emotion:determined
    Kim Trọng: Giúp. Dĩ nhiên là giúp.

- (g_trach)
# speaker:KimTrong
# emotion:thoughtful
Kim Trọng: Nhưng một tờ giấy chưa đủ đâu. Quan huyện sẽ hỏi: ai làm chứng? Bạc đâu? Nợ đâu? Mụ Tú Bà có quen biết trên huyện đường.

# speaker:KimTrong
Kim Trọng: Cậu ta có căn nhà ngay dưới phố. Về đó đã. Ta có sách luật, có giấy bút.

# speaker:NoiTam
# objective:Lập hồ sơ: Khế ước · Nhân chứng · Giấy nợ · Tiền bạc
Một bộ hồ sơ. Giống như bài thuyết trình nhóm — chỉ khác là lần này, sai một chữ là có người chết.

# scene:KimTrong_Study
# music:study
# speaker:Narrator
# camera:Wide_Study
Thư phòng nhà cậu Kim Trọng: giá sách kín tường, nghiên mực còn ướt. Kim Trọng trải hết giấy tờ ra trên án thư.

{c4_ru_co_lam && c5_co_lam_thoat:
    # speaker:Narrator
    Cô gái áo lam được đưa vào buồng trong nghỉ ngơi.
}

# speaker:KimTrong
# emotion:thoughtful
Kim Trọng: Đặt chúng cạnh nhau. Kẻ nói dối viết nhiều giấy, nhưng giấy không biết nói dối giùm hắn.

# speaker:Narrator
# gameplay:doi_chieu
Đối chiếu các giấy tờ đã thu thập.

{
- c6_doi_chieu >= 3:
    # speaker:KimTrong
    # emotion:amused
    Kim Trọng: Nàng đọc giấy tờ còn tinh hơn cả thầy đồ của ta.
    ~ kt_tin_tuong += 1
- c6_doi_chieu >= 1:
    # speaker:KimTrong
    Kim Trọng: Đủ để thấy chúng gian. Nhưng phải có người sống đứng ra nói.
- else:
    # speaker:KimTrong
    # emotion:thoughtful
    Kim Trọng: Giấy tờ rối quá. Không sao — người làm chứng sẽ nói thay giấy.
}

# speaker:KimTrong
# emotion:determined
Kim Trọng: Ra phố thôi. Nàng đội nón vào — người của Tú Bà đang hỏi han khắp chợ. Đừng chạy, chạy là bị chú ý.

# scene:LamTri_Street
# music:street
# speaker:Narrator
# objective:Tìm nhân chứng và bằng chứng trên phố huyện · đưa đúng giấy tờ cho đúng người
# gameplay:tutorial_street
Phố huyện Lâm Tri buổi trưa đông người qua lại.

-> c6_hub


=== c6_hub ===
* {not c6_ba_nuoc_xong} [Bà hàng nước đầu chợ #explore:c6_ba_nuoc]
    -> c6_ba_nuoc
* {not c6_thu_lai_xong} [Thư lại trước cổng huyện đường #explore:c6_thu_lai]
    -> c6_thu_lai
* {not c6_hieu_bac_xong} [Hiệu bạc Phúc Ký #explore:c6_hieu_bac]
    -> c6_hieu_bac
* {c4_ru_co_lam && c5_co_lam_thoat && not c6_co_lam_xong} [Cô gái áo lam trong thư phòng #explore:c6_co_lam]
    -> c6_co_lam
* {c6_xong >= 2} [Về thư phòng — sắp xếp hồ sơ #explore:c6_dossier]
    -> c6_dossier


=== c6_ba_nuoc ===
# speaker:BaHangNuoc
# emotion:suspicious
# camera:OTS_Kieu_BaHangNuoc
Bà hàng nước: Ơ... cô này. Cô là cô gái đi cùng gã họ Mã, nghỉ chân ở quán tôi dọc đường đấy à?

# speaker:BaHangNuoc
Bà hàng nước: Tôi dọn hàng lên huyện bán phiên chợ. Cô... trông cô khác quá.

~ c6_thu = 0
-> c6_ba_nuoc_present

= c6_ba_nuoc_present
# speaker:NoiTam
Bà ấy đã ở đó, ngay bên kia vách tre. Cho bà ấy thấy gì để bà ấy nhớ ra?

* {giu_mau_giay} [Đưa mẩu giấy "giao hàng" nhặt trên đường.]
    # speaker:BaHangNuoc
    # emotion:realization
    Bà hàng nước: Chữ này... cái gã tùy tùng cầm tờ giấy y như thế, đọc to "giao hàng" rồi cả hai cười ha hả sau vách nhà tôi!
    -> c6_ba_nuoc_ok
* {mm_nghe_len} [Nhắc lại câu nghe được sau vách tre: "Tú Bà đã chuẩn bị chưa?"]
    # speaker:BaHangNuoc
    # emotion:realization
    Bà hàng nước: Phải rồi! "Tú Bà đã chuẩn bị chưa" — tôi nghe rõ mồn một. Tôi còn tưởng là tên bà mối nào.
    -> c6_ba_nuoc_ok
* [Đưa khế ước Mã–Tú Bà.]
    ~ c6_thu += 1
    # speaker:BaHangNuoc
    # emotion:fearful
    Bà hàng nước: Tôi... tôi không biết chữ, cô ạ. Giấy má quan quyền, tôi không dám dính vào.
* {c4_tien == 2} [Đưa nén bạc có đóng dấu "T."]
    ~ c6_thu += 1
    # speaker:BaHangNuoc
    # emotion:fearful
    Bà hàng nước: Bạc dấu lầu xanh? Cô... cô lấy trộm à? Thôi, tôi không biết gì hết!
* [Nhờ Kim Trọng nói giúp.]
    {kt_tin_tuong >= 4:
        # speaker:KimTrong
        # emotion:calm_probe
        Kim Trọng: Bà ơi, cháu là Kim Trọng, con nhà họ Kim ở huyện bên. Bà chỉ cần nói lại những gì bà đã nghe, không hơn một chữ. Có cháu đứng ra đảm bảo.
        # speaker:BaHangNuoc
        Bà hàng nước: ...Thôi được. Nhìn cậu là biết con nhà tử tế.
        -> c6_ba_nuoc_ok
    - else:
        ~ c6_thu += 1
        # speaker:KimTrong
        Kim Trọng: Bà ơi, xin bà giúp cho...
        # speaker:BaHangNuoc
        # emotion:suspicious
        Bà hàng nước: Cậu với cô là gì của nhau mà nói giúp? Tôi chẳng quen ai cả.
    }

- (g_thu_ba)
{c6_thu >= 2:
    # speaker:BaHangNuoc
    # emotion:fearful
    Bà hàng nước: Thôi cô đi đi, tôi còn bán hàng.
    # speaker:NoiTam
    Mất một nhân chứng. Lẽ ra phải cho bà ấy thấy thứ bà ấy hiểu được.
    -> c6_ba_nuoc_done
}
-> c6_ba_nuoc_present

= c6_ba_nuoc_ok
~ nc_ba_nuoc = true
~ ev_nhan_chung += 1
# speaker:BaHangNuoc
# emotion:determined
Bà hàng nước: Già rồi, sợ gì nữa. Lên huyện đường thì tôi nói. Con gái nhà người ta chứ có phải con gà con vịt đâu.
# speaker:NoiTam
# clue:c6_ba_hang_nuoc
Nhân chứng thứ nhất.
-> c6_ba_nuoc_done

= c6_ba_nuoc_done
~ c6_ba_nuoc_xong = true
~ c6_xong += 1
-> c6_hub


=== c6_thu_lai ===
# speaker:ThuLai
# emotion:cold
# camera:OTS_Kieu_ThuLai
Thư lại: Đơn kiện à? Kiện ai? ...Tú Bà? Cô có biết mụ ấy biếu quan lớn bao nhiêu mỗi dịp Tết không?

# speaker:ThuLai
# emotion:mocking
Thư lại: Một cô gái chạy trốn khỏi lầu xanh thì lời nói nhẹ như lông hồng. Có gì làm bằng thì đưa ra.

~ c6_thu = 0
{dinh_menh >= 70 && kt_tin_tuong < 4:
    # fx:spirit
    # speaker:Narrator
    Trong tay áo thư lại lộ ra một góc túi gấm đỏ, thêu chữ "T." y như những túi bạc ở lầu Lâm Tri.
    # fx:none
    # speaker:NoiTam
    Tú Bà đã đến đây trước mình. Câu chuyện lại đẩy mình về trang cũ. Lần này chỉ được nói sai một lần thôi.
}
-> c6_thu_lai_present

= c6_thu_lai_present
* [Đưa khế ước Mã–Tú Bà, chỉ vào ngày lập: tháng Giêng.]
    {c6_doi_chieu >= 2:
        # speaker:Kieu
        # emotion:calm_probe
        Kiều: Giao kèo lập tháng Giêng. Tháng Tư Mã Giám Sinh mới đến hỏi cưới ta. Ba cái tên gạch đi là ba lần "hỏi vợ" trước. Đây không phải chuyện cưới xin — đây là một mối buôn.
        # speaker:ThuLai
        # emotion:realization
        Thư lại: Tháng Giêng... Khoan. Tháng Giêng năm nay chính tay tôi vào sổ hôn thú cho gã họ Mã. Rồi tháng Ba lại một lần nữa.
        -> c6_thu_lai_ok
    - else:
        ~ c6_thu += 1
        # speaker:ThuLai
        # emotion:cold
        Thư lại: Giao kèo mua bán giữa hai người buôn bán. Thì sao? Cô chỉ cho tôi chỗ nào là phạm luật?
        # speaker:NoiTam
        Ngày tháng. Mình đã nhìn thấy nó trên án thư mà không nhận ra điểm mấu chốt.
    }
* {c4_van_tu == 1} [Đưa văn tự bán thân có điểm chỉ của chính mình.]
    # speaker:Kieu
    # emotion:cold
    Kiều: Văn tự bán thân. Người bán ghi tên Mã Giám Sinh — kẻ đã đến nhà ta hỏi cưới, có ông mai, có sính lễ. Ngài so với sổ hôn thú của ngài xem.
    # speaker:ThuLai
    # emotion:realization
    Thư lại: Cùng một tên trong sổ hôn thú và trên văn tự bán người... Cưới vợ xong đem bán. Luật nào cho phép thế?
    -> c6_thu_lai_ok
* {c4_tien == 2} [Đặt nén bạc có dấu "T." lên bàn.]
    {kt_tin_tuong >= 4:
        # speaker:KimTrong
        # emotion:firm
        Kim Trọng: Đó là tiền mụ trả để mua người. Kiều mang ra để làm bằng, không tiêu một đồng. Họ Kim chúng tôi đứng ra bảo lãnh.
        # speaker:ThuLai
        # emotion:thoughtful
        Thư lại: Nhà họ Kim bảo lãnh... thì tôi tin là bạc làm chứng chứ không phải bạc trộm.
        -> c6_thu_lai_ok
    - else:
        ~ c6_thu += 1
        # speaker:ThuLai
        # emotion:angry
        Thư lại: Bạc của lầu Lâm Tri trong tay một cô gái bỏ trốn? Tôi mà khép cô tội trộm cắp thì cô tính sao?
    }
* [Nói rằng mình là con gái Vương viên ngoại.]
    ~ c6_thu += 1
    # speaker:ThuLai
    # emotion:mocking
    Thư lại: Cả huyện Lâm Tri này đứa nào bị bán cũng là con nhà tử tế cả, cô ạ.

- (g_thu_lai)
{c6_thu >= 2 || (c6_thu >= 1 && dinh_menh >= 70 && kt_tin_tuong < 4):
    # speaker:ThuLai
    # emotion:cold
    Thư lại: Hết giờ làm việc. Mời cô về.
    # speaker:NoiTam
    Ông ta cần thứ mà ông ta không thể chối: chữ, ngày tháng, con dấu.
    -> c6_thu_lai_done
}
-> c6_thu_lai_present

= c6_thu_lai_ok
~ nc_thu_lai = true
~ ev_nhan_chung += 1
# speaker:ThuLai
# emotion:determined
Thư lại: Bằng cấp "giám sinh" của hắn cũng mua bằng tiền — sổ của huyện ghi rõ. Nếu quan hỏi, tôi sẽ mang sổ hôn thú ra.
# speaker:NoiTam
# clue:c6_so_hon_thu
Bốn cái "đám cưới" trong một năm. Không quan nào chối được sổ của chính mình.
-> c6_thu_lai_done

= c6_thu_lai_done
~ c6_thu_lai_xong = true
~ c6_xong += 1
-> c6_hub


=== c6_hieu_bac ===
# speaker:ChuHieuBac
# emotion:businesslike
# camera:OTS_Kieu_ChuHieuBac
Chủ hiệu bạc: Đổi bạc hay cầm đồ? ...Hỏi sổ sách của khách à? Hiệu Phúc Ký giữ chữ tín ba đời, không đưa sổ cho người lạ.

{ev_tien:
    # speaker:NoiTam
    Nén bạc có dấu "T." mình đã có. Nhưng nếu sổ ở đây ghi ai đã đổi bạc, thì đó mới là bằng chứng không ai cãi được.
}

* {kt_tin_tuong >= 3} [Để Kim Trọng đứng ra.]
    # speaker:KimTrong
    # emotion:calm_probe
    Kim Trọng: Ông Phúc, cháu là con trai Kim viên ngoại, nhà cháu vẫn gửi bạc ở hiệu ông mỗi mùa thuế. Cháu chỉ xin xem một dòng.
    # speaker:ChuHieuBac
    # emotion:thoughtful
    Chủ hiệu bạc: Cậu Kim... Thôi được, một dòng thôi đấy.
    -> c6_hieu_bac_ok
* {tien >= 5} [Đặt vài lạng bạc lên quầy.]
    ~ tien -= 5
    # speaker:ChuHieuBac
    # emotion:smug
    Chủ hiệu bạc: Chữ tín ba đời... cũng có giá của nó.
    # speaker:NoiTam
    Mình vừa làm đúng cái việc mình đang tố cáo bọn chúng. Mua một con người — mua một trang sổ.
    -> c6_hieu_bac_ok
* [Cầu xin ông ấy.]
    # speaker:ChuHieuBac
    # emotion:cold
    Chủ hiệu bạc: Cô ơi, ở đất này nước mắt không đổi ra bạc được.
    -> c6_hieu_bac_done

= c6_hieu_bac_ok
~ ev_tien = true
~ c6_so_bac = true
# speaker:ChuHieuBac
Chủ hiệu bạc: Đây. Ngày mười bốn tháng Tư, người nhà Tú Bà lấy ra bốn trăm năm mươi lạng nén, giao cho "Mã công tử". Người nhận điểm chỉ.
# speaker:NoiTam
# clue:c6_so_hieu_bac
Ngày mười bốn — chính là ngày mình đến lầu. Tiền đổi tay, có điểm chỉ.
-> c6_hieu_bac_done

= c6_hieu_bac_done
~ c6_hieu_bac_xong = true
~ c6_xong += 1
-> c6_hub


=== c6_co_lam ===
# speaker:CoLam
# emotion:fearful
# camera:OTS_Kieu_CoLam
Cô gái áo lam: Ra huyện đường... đứng trước mặt mụ ấy mà nói? Chị... chị sợ lắm, Kiều.

* [Nắm tay chị: "Lần này chị không đứng một mình."]
    # speaker:Kieu
    # emotion:calm_probe
    Kiều: Lần này chị không đứng một mình. Có em, có Kim công tử, có bà hàng nước. Chị chỉ cần kể lại ba năm của chị.
    # speaker:CoLam
    # emotion:crying
    Cô gái áo lam: Ba năm... Chị tên là Lam. Chưa ai ở đó hỏi tên chị cả.
    ~ nc_co_lam = true
    ~ ev_nhan_chung += 1
* {ev_giay_no} [Đưa trang sổ nợ: "Trang của chị cũng giống thế này."]
    # speaker:CoLam
    # emotion:angry
    Cô gái áo lam: "Nợ bốn trăm năm mươi lạng." Ba năm chị trả không hết một dòng... Được. Chị sẽ nói.
    ~ nc_co_lam = true
    ~ ev_nhan_chung += 1
* [Để chị ấy nghỉ ngơi.]
    # speaker:Kieu
    Kiều: Chị cứ nghỉ đi. Chị đã chạy đủ rồi.
    # speaker:CoLam
    Cô gái áo lam: ...Cảm ơn em.

- (g_co_lam6)
{nc_co_lam:
    # speaker:NoiTam
    # clue:c6_co_lam
    Chị Lam. Một nhân chứng — và một cái tên, thay vì "cô gái áo lam".
}
~ c6_co_lam_xong = true
~ c6_xong += 1
-> c6_hub


=== c6_dossier ===
# scene:KimTrong_Night
# music:study
# speaker:Narrator
# camera:Wide_Study
Đêm. Thư phòng chỉ còn một ngọn đèn. Kim Trọng xếp từng tờ giấy vào một chiếc hộp gỗ.

~ c6_ho_so = 0
{ev_khe_uoc:
    ~ c6_ho_so += 1
}
{ev_nhan_chung > 0:
    ~ c6_ho_so += 1
}
{ev_giay_no:
    ~ c6_ho_so += 1
}
{ev_tien:
    ~ c6_ho_so += 1
}

# speaker:Narrator
# gameplay:ho_so
Hồ sơ cho ngày mai.

{
- c6_ho_so >= 4:
    # speaker:KimTrong
    # emotion:determined
    Kim Trọng: Khế ước, người làm chứng, sổ nợ, sổ bạc. Đủ cả. Lần này không quan nào bịt miệng được.
- c6_ho_so >= 3:
    # speaker:KimTrong
    # emotion:thoughtful
    Kim Trọng: Thiếu một mảnh, nhưng đủ để quan phải nghe. Phần còn lại... ta sẽ phải nói thật khéo.
- else:
    # speaker:KimTrong
    # emotion:pained
    Kim Trọng: Mỏng quá. Nhưng chúng ta không còn thời gian để tìm thêm nữa.
}

{ev_nhan_chung >= 2:
    ~ nhan(ts_be_dau)
    # speaker:TrangSach
    # page:ts_be_dau
    "Trải qua một cuộc bể dâu, những điều trông thấy mà đau đớn lòng."
    # speaker:NoiTam
    Bà hàng nước, thư lại, chị Lam... mỗi người đều đã thấy một mảnh của cuộc bể dâu này. Ngày mai họ phải đứng vững.
}

# speaker:Narrator
Kim Trọng mài mực, trải một tờ hoa tiên lên án thư. Ngày trước, hai người vẫn đối thơ với nhau như thế qua bức tường.

# speaker:KimTrong
# emotion:amused
# camera:CU_KimTrong
Kim Trọng: Ta ra câu sáu, nàng đối câu tám. Như hồi ở vườn Thúy. Nếu đúng là nàng, nàng sẽ đối được.

# speaker:KimTrong
# gameplay:lucbat_ch6
Kim Trọng: "Trăng xưa còn nhớ vườn đào..."

{
- lb_ch6 >= 3:
    ~ kt_tin_tuong += 2
    # speaker:KimTrong
    # emotion:warm
    Kim Trọng: Cây thoa bên bức tường... Chỉ có nàng mới biết chuyện ấy. Đúng là nàng rồi.
- lb_ch6 == 2:
    ~ kt_tin_tuong += 1
    # speaker:KimTrong
    # emotion:amused
    Kim Trọng: Vẫn chữ ấy, vẫn vần ấy. Có điều... giọng nàng khác xưa.
- else:
    # speaker:KimTrong
    # emotion:thoughtful
    Kim Trọng: Nàng mệt quá rồi. Thơ để dịp khác.
}

# speaker:KimTrong
# emotion:thoughtful
# camera:CU_KimTrong
Kim Trọng: Kiều. Hôm nay nàng nói chuyện với người ta... như thể nàng đã biết trước họ sẽ nói gì.

{kt_biet_xuyen_khong: -> c6_xuyen_khong | -> c6_khong_ke}

= c6_xuyen_khong
# speaker:KimTrong
Kim Trọng: Sáng nay nàng bảo nàng đến từ một nơi rất xa, đã đọc trước câu chuyện này. Ta đã nghĩ nàng sốt. Giờ thì ta không chắc nữa.
# speaker:KimTrong
# emotion:calm_probe
Kim Trọng: Ở nơi ấy... người ta kể về ta thế nào?
* ["Họ kể chàng là người tốt, nhưng luôn đến muộn."]
    ~ kt_tin_tuong += 1
    # speaker:Kieu
    # emotion:pained
    Kiều: Họ kể chàng là một người tốt. Một người yêu thật lòng... nhưng luôn đến muộn.
    # speaker:KimTrong
    # emotion:determined
    Kim Trọng: Vậy thì lần này, ta sẽ không đến muộn.
* ["Họ kể chàng chỉ là một nhân vật trong sách."]
    ~ soul(-1)
    # speaker:Kieu
    # emotion:thoughtful
    Kiều: Ở đó... chàng chỉ là một nhân vật trong sách. Ta đã đọc về chàng như đọc về một cái tên.
    # speaker:KimTrong
    # emotion:amused
    Kim Trọng: Một cái tên thì không biết giận đến run tay như ta hôm nay đâu.
- -> c6_ket

= c6_khong_ke
# speaker:Kieu
# emotion:calm_probe
Kiều: Ta chỉ đọc nhiều giấy tờ hơn người khác một chút thôi.
# speaker:KimTrong
# emotion:amused
Kim Trọng: Nàng vẫn giỏi nói dối như hồi làm thơ đối với ta.
-> c6_ket

= c6_ket
# speaker:NoiTam
Mình vẫn luôn nghĩ chàng chỉ là một cái tên trong sách. Một người si tình, đến muộn, rồi khóc.

# speaker:NoiTam
Nhưng bàn tay đang run lên vì giận kia là thật. Đôi mắt thức trắng vì một người chàng tưởng đã mất cũng là thật.

# speaker:KieuThat
# gameplay:kieu_voice
Ngươi bắt đầu nhìn thấy họ rồi đấy. Không phải những trang giấy. Là những con người.

# fx:none
# speaker:Narrator
# sfx:knock
Có tiếng gõ cửa dồn dập. Gia nhân nhà cậu Kim thở hổn hển.

# speaker:GiaNhan
# emotion:fearful
Gia nhân: Cậu ơi! Mã Giám Sinh với mụ Tú Bà đã lên huyện đường đầu hôm. Chúng đâm đơn kiện một đứa ở trốn lấy trộm bạc. Sáng mai quan xử!

# speaker:KimTrong
# emotion:angry
Kim Trọng: Chúng ra tay trước rồi.

# speaker:Kieu
# emotion:determined
# camera:CU_Kieu
# music:resolve
Kiều: Không sao. Sáng mai, ta sẽ đến huyện đường trước chúng.

# speaker:Kieu
Kiều: Và lần này, người ra trước mặt quan... sẽ là ta.

-> chapter_7
