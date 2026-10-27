// ============================================================
// CHƯƠNG 5 — ĐÀO THOÁT
// Scene Unity: Chapter05_Escape (rừng tre ban đêm → cầu qua khe → cổng chùa, đêm không mưa)
// Nhịp: nối liền từ cửa sau Ch4 — mở ngay vào cuộc chạy góc nhìn thứ nhất qua rừng tre (# run:chase_forest),
//       lính cầm đuốc đuổi sau lưng → đạp đổ cầu → cổng chùa → Ch6.
// Tiếng Tú Bà hô "Bắt nó lại!" và lời chị Lam được nói trong lúc chạy (BambooChaseSequence), không chặn người chơi.
// Unity ghi: c5_bi_bat, c5_vap, c5_loi, c5_giay, c5_co_lam_thoat.
// ============================================================

=== chapter_5 ===
# chapter:5
# scene:Escape_Forest
# music:none
# run:chase_forest
# speaker:Narrator
Cây cầu tre đổ ụp xuống khe. Bên kia bờ, bó đuốc dừng lại. Tiếng chửi rủa vọng theo một lúc, rồi xa dần.

{c4_ru_co_lam:
    {c5_co_lam_thoat:
        # speaker:CoLam
        # emotion:crying
        Cô gái áo lam: (thở hổn hển) Chị... chị vẫn ở đây. Chị vẫn theo kịp em.
        ~ soul(2)
    - else:
        # speaker:CoLam
        # emotion:crying
        Cô gái áo lam: (gào lên từ phía sau) Đừng quay lại! Chạy đi, Kiều!
        # speaker:Narrator
        Tiếng chị ấy bị kéo ngược vào bóng tối. Rồi im bặt.
        # speaker:NoiTam
        # emotion:pained
        Mình đã hứa dẫn chị ấy ra. Mình đã hứa.
    }
}

# scene:Escape_Rain
# music:none
# speaker:Narrator
# camera:Hero_Kieu_Rain
Tiếng đuổi bắt nhỏ dần rồi tắt hẳn sau lưng. Gió lùa qua rặng tre, rồi lặng hẳn.

# speaker:Narrator
Kiều đứng giữa khoảng sân đất trước cổng chùa. Thở dốc. Tóc bết mồ hôi vào má, bàn chân rớm máu.

# speaker:Kieu
# emotion:pained
Kiều: Mình thoát rồi...

{c4_ru_co_lam && c5_co_lam_thoat:
    # speaker:CoLam
    # emotion:crying
    Cô gái áo lam: Thoát rồi... ba năm, chị mới thấy lại bầu trời ngoài bức tường ấy.
}

# speaker:Narrator
# camera:CU_Kieu
Cô nhìn khế ước trong tay. Mực đã nhòe ở mép, nhưng những cái tên vẫn còn nguyên.

# speaker:Kieu
# emotion:determined
Kiều: Nhưng chỉ trốn thôi chưa đủ.

# speaker:Kieu
# emotion:determined
# music:resolve
Kiều: Mình phải quay lại.

# speaker:NoiTam
Không phải để chui vào lồng lần nữa. Mà để mở nó ra — cho cả những người còn ở trong.

# speaker:KieuThat
# gameplay:kieu_voice
Một mình ngươi thì không mở nổi cánh cổng ấy đâu.

# speaker:NoiTam
Mình biết. Mình cần một người... một người mà trong câu chuyện này chưa từng phản bội Kiều.

~ nhan(ts_kim_trong)
# speaker:TrangSach
# page:ts_kim_trong
"Người quốc sắc, kẻ thiên tài. Tình trong như đã, mặt ngoài còn e."

# fx:none
# speaker:NoiTam
Kim Trọng.

# speaker:NoiTam
Chú Vương Quan có nói... chàng đã lên đường đuổi theo đoàn rước dâu. Nếu chàng đến được Lâm Tri, chàng sẽ trọ ở nhà người cậu gần chùa Bạch Vân.

# speaker:Narrator
# camera:Wide_TempleGate
Phía cuối con đường lầy lội có ánh đèn của một mái chùa. Kiều bước về phía ấy, rồi khuỵu xuống trước bậc tam cấp.

# speaker:Narrator
Có ai đó cầm đèn chạy ra từ trong cổng.

-> chapter_6



// ------------------------------------------------------------
// TẠM KHÔNG ĐƯỢC GỌI (từ 25/10/2026): Ch5 nay mở thẳng vào cuộc chạy, không còn cảnh bìa rừng để gặp Sở Khanh.
// Giữ nguyên knot và biến (c5_so_khanh, ts_so_khanh...) để save cũ nạp được và để đặt lại khi đã chốt vị trí mới.
// ------------------------------------------------------------
=== c5_gap_so_khanh ===
# scene:Escape_SoKhanh
# music:so_khanh
# fx:spirit
# speaker:Narrator
# camera:CU_SoKhanh
Đầu bìa rừng, dưới gốc đa, có một người thanh niên dắt ngựa đứng chờ như đã hẹn từ trước. Áo khăn chải chuốt, mùi trầm thơm thoảng trong gió.

# speaker:SoKhanh
# emotion:warm_fake
Sở Khanh: Nàng! Ta là kẻ đọc sách, thấy chướng mắt cảnh mụ Tú hành hạ người ngay. Lên ngựa đi, ta đưa nàng ra khỏi Lâm Tri trước khi trời sáng.

# speaker:NoiTam
# timer:10
Đây rồi. Câu chuyện đang kéo mình về đúng trang sách cũ.

* {ts_so_khanh == 2} [Lật trang "Một chàng vừa trạc thanh xuân...": vạch mặt Sở Khanh #page:ts_so_khanh]
    ~ dung(ts_so_khanh, 5)
    ~ c5_so_khanh = 1
    # speaker:Kieu
    # emotion:cold
    Kiều: Sở Khanh. Áo khăn dịu dàng, ngựa đứng sẵn, hứa đưa người ta trốn. Rồi giữa đường thì quất ngựa bỏ đi, để người của mụ Tú đến bắt về. Ta biết rõ ngươi là ai.
    # speaker:SoKhanh
    # emotion:fearful
    Sở Khanh: Nàng... sao nàng biết tên ta?
    # speaker:Narrator
    Hắn lùi lại một bước, rồi vội vàng nhảy lên ngựa, phi mất hút vào bóng tối.
    # fx:none
    # speaker:NoiTam
    Một trang của Kiều vừa bị xé. Lần này, nàng không bị lừa.
* [Lên ngựa theo chàng.]
    ~ c5_so_khanh = 2
    # speaker:Narrator
    Kiều vừa ngồi lên lưng ngựa thì gã thư sinh huýt sáo. Từ sau gốc đa, hai gã gia nhân của Tú Bà bước ra.
    # speaker:SoKhanh
    # emotion:smug
    Sở Khanh: Hàng về đúng chỗ rồi nhé. Nhớ chia ta phần hơn.
    # speaker:Narrator
    Kiều giằng ra, ngã nhào xuống đất, rồi lăn vào bụi tre. Khi đứng dậy được, ngực áo đã nhẹ bẫng.
    {
    - c4_tien == 2:
        ~ c4_tien = 1
        ~ tien = MAX(tien - 445, 0)
        ~ ev_tien = false
        # speaker:NoiTam
        # emotion:pained
        Túi bạc dấu "T." rơi mất rồi. Thứ duy nhất chứng minh tiền mua người... giờ lại về tay chúng.
    - c4_so_no:
        ~ c4_so_no = false
        ~ ev_giay_no = false
        # speaker:NoiTam
        # emotion:pained
        Trang sổ nợ xé được... rơi mất trong lúc giằng co.
    - else:
        # speaker:NoiTam
        # emotion:pained
        May mà chúng chưa giật được khế ước. Nhưng mình đã mất hết đà chạy.
    }
    ~ fate(5)
    # fx:none
    # speaker:KieuThat
    # gameplay:kieu_voice
    Ta cũng đã từng tin hắn như thế.
* [Lắc đầu, chạy tiếp một mình. #timeout]
    ~ c5_so_khanh = 3
    # speaker:Kieu
    # emotion:firm
    Kiều: Cảm ơn. Nhưng ta không lên ngựa của người lạ.
    # speaker:SoKhanh
    # emotion:angry
    Sở Khanh: (gào to) Nó ở đây! Con bé trốn chạy ở bìa rừng này!
    # speaker:NoiTam
    Hắn là người của mụ! Chạy!
    # fx:none

- -> chapter_5
