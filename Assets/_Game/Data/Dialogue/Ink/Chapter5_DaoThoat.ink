// ============================================================
// CHƯƠNG 5 — ĐÀO THOÁT
// Scene Unity: Chapter05_Escape (sân bếp → ngõ sau → chợ đêm / ngõ tắt → rừng tre → bến chùa trong mưa)
// Nhịp: Lẻn ra (canh đèn lồng, con chó, cửa kêu) → "Bắt nó lại!" → Rượt đuổi tự do ở ngõ/chợ
//       → Rượt đuổi điện ảnh trong rừng tre (QTE) → Đứng giữa mưa → Móc nối Chương 6.
// Unity ghi: c5_lang_le, c5_bi_bat, c5_duong, c5_xo_do, c5_qte, c5_co_lam_thoat.
// ============================================================

=== chapter_5 ===
# chapter:5
# scene:Escape_Night
# music:stealth
# speaker:Narrator
# camera:Wide_BackYard
Then cửa sau trượt khỏi khấc. Ngoài kia, sân bếp tối om — chỉ có một ngọn đèn lồng đung đưa trên tay người canh.

{c4_canh_giac == 2:
    # speaker:NoiTam
    Tên canh trước cửa phòng mình gục đầu ngủ rồi. Lời cãi của mình suýt nữa đã giết mình.
}
{c4_ru_co_lam:
    # speaker:CoLam
    # emotion:fearful
    Cô gái áo lam: (thì thầm) Chị theo sau em. Em khom thì chị khom.
}

# speaker:NoiTam
Đèn lồng soi đến đâu, mắt hắn nhìn đến đó. Đi trong bóng tối, đi lúc hắn quay lưng.

# speaker:Narrator
# objective:Lẻn qua sân bếp, mở cửa ngõ sau
# gameplay:tutorial_escape
Thoát khỏi lầu Tú Bà.

# speaker:Narrator
# gameplay:sneak_out
Kiều nín thở bước vào bóng tối.

{c5_lang_le >= 2:
    # speaker:NoiTam
    Không ai thấy. Chưa ai thấy.
- else:
    # speaker:NoiTam
    Tim đập như trống làng. Hắn đã ngoảnh lại một lần... nhưng chưa nhận ra.
}

# scene:Escape_Alarm
# sfx:gong
# speaker:Narrator
# camera:CU_Kieu
Cánh cửa ngõ vừa hé một gang tay thì trên lầu có tiếng cửa sổ bật mở.

{
- c4_tien == 2 || c4_van_tu > 0:
    # speaker:TuBa
    # emotion:rage
    Tú Bà: Tủ của ta! Đứa nào mở tủ của ta?!
- else:
    # speaker:TuBa
    # emotion:rage
    Tú Bà: Đứa nào lục đục dưới bếp đấy?!
}

# speaker:TuBa
# emotion:rage
# camera:Wide_BackYard
Tú Bà: Bắt nó lại!

# speaker:NoiTam
# music:chase
CHẠY!

# speaker:Narrator
# objective:CHẠY! Thoát khỏi ngõ sau
# gameplay:chase_alley
Kiều lao vào ngõ tối.

{c5_duong == 1:
    # speaker:NoiTam
    Ngõ tắt bỏ lại được chúng một đoạn — nhưng tiếng chân vẫn đuổi sát phía sau.
- else:
    # speaker:NoiTam
    Lẫn vào đám đông chợ đêm được một lúc. Nhưng chúng vẫn nhìn thấy tấm áo của mình.
}
{c5_xo_do >= 2:
    # speaker:NoiTam
    Mấy cái thúng, cái xe đổ chắn đường làm chúng chửi ầm lên. Tốt.
}

# speaker:Narrator
# camera:Wide_BambooEdge
Cuối chợ là bìa rừng tre. Không còn đèn, không còn đường — chỉ còn tiếng lá tre và tiếng bước chân phía sau.

# speaker:NoiTam
# objective:Chạy xuyên rừng tre — làm theo phím hiện trên màn hình
Vào rừng!

# speaker:Narrator
# gameplay:chase_forest
Kiều lao xuống con dốc đầy rễ tre.

{c4_ru_co_lam:
    {c5_co_lam_thoat:
        # speaker:CoLam
        # emotion:crying
        Cô gái áo lam: (thở hổn hển) Chị... chị vẫn ở đây. Chị vẫn theo kịp em.
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
Tiếng đuổi bắt nhỏ dần rồi tắt hẳn sau lưng. Trời đổ mưa.

# speaker:Narrator
Kiều đứng giữa trời mưa. Thở dốc. Tóc dính bết vào má, bàn chân rớm máu.

# speaker:Kieu
# emotion:pained
Kiều: Mình thoát rồi...

{c4_ru_co_lam && c5_co_lam_thoat:
    # speaker:CoLam
    # emotion:crying
    Cô gái áo lam: Thoát rồi... ba năm, chị mới thấy lại trời mưa ngoài bức tường ấy.
}

# speaker:Narrator
# camera:CU_Kieu
Cô nhìn khế ước trong tay. Mực đã nhòe ở mép, nhưng những cái tên vẫn còn nguyên.

# speaker:Kieu
# emotion:determined
Kiều: Nhưng chỉ trốn thôi chưa đủ.

# speaker:Kieu
# emotion:determined
Kiều: Mình phải quay lại.

# speaker:NoiTam
Không phải để chui vào lồng lần nữa. Mà để mở nó ra — cho cả những người còn ở trong.

# speaker:KieuThat
# gameplay:kieu_voice
Một mình ngươi thì không mở nổi cánh cổng ấy đâu.

# speaker:NoiTam
Mình biết. Mình cần một người... một người mà trong câu chuyện này chưa từng phản bội Kiều.

# speaker:TrangSach
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
