// ============================================================
// THÚY KIỀU: KIẾM TIỀN CÓ GÌ KHÓ NHỈ?
// Demo 4 chương | Mục tiêu thời lượng: 15–25 phút
// Tone: Cổ đại, hài nhẹ, châm biếm, cảm xúc
// Engine: UNITY (ink-unity-integration) — v2
//
// HỢP ĐỒNG TAG (Unity đọc qua story.currentTags / choice.tags)
// Chi tiết đầy đủ: Doc/KeHoach_ThietKe_ThuyKieu_Unity.md, mục 5.3
//   # chapter:N        -> ChapterManager: title card + autosave
//   # scene:ID         -> SceneDirector: đổi location/spawn point (bảng SceneMap)
//   # music:ID         -> AudioManager: crossfade nhạc nền
//   # sfx:ID           -> AudioManager: phát 1 hiệu ứng
//   # speaker:ID       -> DialogueView: tên + màu + nhân vật đang nói
//   # emotion:ID       -> CharacterExpression: biểu cảm khuôn mặt (bảng EmotionMap)
//   # camera:ID        -> CameraDirector: bật Cinemachine camera cùng tên
//   # evidence:ID      -> Journal + toast "Đã tìm thấy bằng chứng"
//   # clue:ID          -> Journal + toast "Manh mối mới"
//   # minigame:ID      -> MiniGameManager (CHẶN): chạy xong ghi kq_minigame rồi mới Continue
//   # gameplay:ID      -> GameplaySequence (CHẶN): vd. đoạn chạy trốn
//   # ui:ID            -> DialogueView đổi skin (vd. ui:letter)
//   # ending:ID        -> EndingRecap: bright / warm / reflective
//   Tag trên CHOICE:
//   #explore:ID        -> choice được kích hoạt bằng vật thể trong màn chơi (Explore mode),
//                        không hiện nút bấm. Nếu MỌI choice hiện tại đều có explore: => Explore mode.
//   #req:STAT         -> choice được mở khóa nhờ chỉ số/bằng chứng; UI gắn huy hiệu (phản hồi cho người chơi)
//
// QUY ƯỚC: dòng thoại giữ tiền tố "Tên: " để dễ đọc trong Inky;
// Unity tự bỏ tiền tố này khi đã có # speaker.
// ============================================================

// ---------- CHỈ SỐ CHÍNH (hiện trên HUD) ----------
VAR tien = 0            // lạng bạc
VAR danh_tieng = 0      // cộng đồng nhìn Kiều ra sao
VAR tu_trong = 2        // giữ nguyên tắc khi cần tiền
VAR tinh_tao = 0        // quan sát, suy luận

// ---------- BẰNG CHỨNG / MANH MỐI ----------
VAR bang_chung = false  // lấy được trang sổ mua bán (Chương 2)
VAR biet_cua_sau = false
VAR nghe_len = false    // nghe được chuyện con dấu giả
VAR doc_ky_khe = false  // đọc kỹ điều khoản khế ở Chương 1
VAR ky_ngay = false     // ký khế ngay không đọc

// ---------- TIẾN TRÌNH ----------
VAR viec_c2 = 0         // số việc đã làm trong đêm ở nhà Tú Bà (tối đa 2)
VAR viec_c3 = 0         // số công việc kiếm tiền (tối đa 2)
VAR da_dan = false
VAR da_viet_thu = false
VAR da_buon_vai = false
VAR thu_that = false    // viết thư chân thành
VAR diem_thu = 0        // điểm mini-game viết thư
VAR lam_sai = 0         // số lần kiếm tiền bằng cách gây thiệt cho người khác

// ---------- DO C# GHI ----------
// -1 = chưa chơi / chạy trong Inky, 0 = trượt, 1 = đạt, 2 = xuất sắc
VAR kq_minigame = -1

CONST MUC_TIEU_TIEN = 400
CONST NGUONG_TINH_TAO = 4
CONST NGUONG_DANH_TIENG = 3
CONST NGUONG_TU_TRONG = 4

-> chapter_1


// ============================================================
// CHƯƠNG 1 — GIA BIẾN
// Chủ đề: Hiếu thảo không đồng nghĩa với từ bỏ quyền lựa chọn.
// Thời lượng dự kiến: 4–5 phút. Chế độ: Dialogue + cinematic.
// ============================================================

=== chapter_1 ===
# chapter:1
# scene:VuongGia_DemMua
# music:sad_strings
# speaker:Narrator
# camera:WS_VuongGia
Mưa quất lên mái ngói Vương gia. Trong sân, dấu chân quan sai còn lẫn với bùn đất.

# speaker:Narrator
Vương Ông và Vương Quan vừa bị áp giải đi vì một vụ án mà cả nhà đều biết là oan.

# speaker:MeKieu
# emotion:crying
# camera:MCU_MeKieu
Mẹ Kiều: Bốn trăm lạng... Nhà ta có bán hết cũng không đủ. Trời muốn dồn người ta đến đường cùng sao?

# speaker:Kieu
# emotion:sad_controlled
# camera:CU_Kieu
Kiều: Mẹ đừng khóc. Cha và em còn chờ chúng ta. Con sẽ tìm cách.

# speaker:Narrator
# sfx:knock_door
Ngoài cửa, tiếng người xướng danh vang lên.

# speaker:MaGiamSinh
# emotion:smug
# camera:Reveal_MaGiamSinh
Mã Giám Sinh: Kẻ hèn họ Mã, nghe Vương gia gặp nạn nên mạo muội đến. Ta... có thể giúp.

# speaker:MeKieu
# emotion:hopeful_uncertain
Mẹ Kiều: Công tử thật lòng muốn giúp sao?

# speaker:MaGiamSinh
# emotion:polite_fake
Mã Giám Sinh: Bốn trăm lạng, đổi lấy một cuộc hôn nhân. Vương cô nương theo ta, tiền sẽ đến tay quan sai ngay trong đêm.

# speaker:Narrator
# camera:CU_Kieu_Eyes
Kiều nhìn người đàn ông trước mặt. Quần áo sang, lời nói nhã, nhưng ánh mắt lại giống người đang định giá một món hàng.

* [Hỏi thẳng lai lịch của Mã Giám Sinh.]
    ~ tinh_tao += 1
    # speaker:Kieu
    # emotion:calm_probe
    Kiều: Công tử quê quán nơi nào? Gia thất ra sao? Hôn nhân là chuyện cả đời, xin đừng trách ta hỏi kỹ.

    # speaker:MaGiamSinh
    # emotion:irritated_hidden
    Mã Giám Sinh: Chuyện ấy... sau này nàng tự khắc biết. Việc trước mắt là cứu cha nàng, chẳng phải sao?

    # speaker:Narrator
    Hắn trả lời rất nhanh ở chuyện tiền, nhưng lại chậm ở chuyện thân thế.

* [Im lặng, quan sát cách hắn nói về tiền.]
    ~ tinh_tao += 1
    # speaker:Narrator
    # camera:POV_Kieu_MaHands
    Kiều không đáp. Nàng nhìn túi bạc, nhìn tờ khế, rồi nhìn ngón tay Mã Giám Sinh gõ liên tục lên bàn.

    # speaker:Kieu
    # emotion:thoughtful
    Kiều: Người thật lòng cầu thân thường hỏi về người mình cưới. Công tử từ lúc vào nhà chỉ hỏi... bao giờ ký giấy.

    # speaker:MaGiamSinh
    # emotion:defensive
    Mã Giám Sinh: Cô nương thông minh quá đôi khi cũng khổ thân đấy.

* [Chỉ hỏi một điều: "Tiền đến tay cha ta trước hay sau khi ta đi?"]
    ~ tinh_tao += 2
    # speaker:Kieu
    # emotion:firm
    Kiều: Ta chỉ hỏi một điều. Tiền đến tay cha ta trước, hay sau khi ta bước khỏi cửa này?

    # speaker:MaGiamSinh
    # emotion:caught_off_guard
    Mã Giám Sinh: ...Trước.

    # speaker:Kieu
    Kiều: Vậy xin ghi điều đó vào khế.

    # speaker:Narrator
    Nụ cười của Mã Giám Sinh khựng lại trong một nhịp.

- -> c1_contract


=== c1_contract ===
# scene:VuongGia_BanKyKhe
# camera:Table_Contract
# speaker:Narrator
Mã Giám Sinh đặt lên bàn một tờ khế đã viết sẵn. Dòng chữ "hôn ước" nằm trên cùng, nhưng phía dưới có những câu chữ cố tình viết nhỏ.

* [Đọc kỹ tờ khế trước khi ký.]
    ~ tinh_tao += 1
    ~ doc_ky_khe = true
    # speaker:Narrator
    # clue:dieu_khoan_khe
    Kiều nhận ra một câu lạ: "Người nhận có toàn quyền định đoạt nơi ở và công việc của người giao".

    # speaker:Kieu
    # emotion:cold
    Kiều: Hôn ước mà lại viết như giấy mua bán súc vật. Công tử dùng văn từ thật mới lạ.

    # speaker:MaGiamSinh
    # emotion:annoyed
    Mã Giám Sinh: Chỉ là lệ buôn... à, lệ làm giấy ở chỗ ta.

    # speaker:Narrator
    Kiều ghi nhớ từng chữ. Nàng chưa đủ sức lật bàn, nhưng ít nhất đã biết mình đang bước vào đâu.

* [Ký ngay để tiền được đưa đi cứu cha.]
    ~ tu_trong -= 1
    ~ ky_ngay = true
    # speaker:Narrator
    Kiều cầm bút. Mực chạm giấy nhanh hơn cả một hơi thở.

    # speaker:Kieu
    # emotion:pained
    Kiều: Một chữ của ta đổi lấy mạng người nhà. Nếu đây là cái giá duy nhất... ta trả.

    # speaker:Narrator
    Nhưng khi đặt bút xuống, Kiều thấy Mã Giám Sinh mỉm cười. Nàng hiểu: nếu cứ coi mình là cái giá phải trả, sẽ luôn có kẻ khác định giá nàng.

- -> c1_decision


=== c1_decision ===
# speaker:MeKieu
# emotion:crying
Mẹ Kiều: Con ơi... hay là thôi. Mẹ không thể nhìn con tự bán đời mình.

# speaker:Kieu
# emotion:soft_firm
# camera:CU_Kieu
Kiều: Con đi không phải vì con rẻ hơn bốn trăm lạng.

# speaker:Kieu
Kiều: Con đi vì cha và em cần được cứu ngay hôm nay. Nhưng chuyện đời con... con chưa giao cho ai cả.

# speaker:Narrator
Mã Giám Sinh sai người mang bạc đi. Tin báo trở lại: Vương Ông và Vương Quan được tạm thả để chờ xét lại vụ án.

# speaker:MaGiamSinh
# emotion:satisfied
Mã Giám Sinh: Giờ thì lên đường thôi, nương tử.

# speaker:Kieu
# emotion:determined
Kiều: Đừng gọi ta như thế vội.

# speaker:Narrator
# camera:Hero_Kieu_Rain
Kiều bước ra khỏi nhà. Sau lưng là chữ Hiếu. Trước mặt là một chiếc lồng chưa đóng cửa.

# speaker:Narrator
Và nàng quyết định: trước khi cánh cửa ấy khép lại, nàng phải tìm được chìa khóa.

-> chapter_2


// ============================================================
// CHƯƠNG 2 — CÁI GIÁ CỦA MỘT CON NGƯỜI
// Chủ đề: Quan sát, bằng chứng và việc tự cứu mình.
// Thời lượng dự kiến: 4–6 phút. Chế độ: Explore (hub) + Dialogue + Gameplay chạy trốn.
// ============================================================

=== chapter_2 ===
# chapter:2
# scene:TuBa_TiepKhachDuong
# music:tense_plucked_strings
# speaker:Narrator
Hai ngày sau, Kiều không được đưa về một nhà chồng.

# speaker:Narrator
Nàng bị đưa đến một căn nhà treo đèn đỏ cả đêm. Người chờ ở đó không phải mẹ chồng, mà là Tú Bà.

# speaker:TuBa
# emotion:assessing
# camera:Reveal_TuBa
Tú Bà: Đây là người ngươi nói đáng bốn trăm lạng?

# speaker:MaGiamSinh
# emotion:businesslike
Mã Giám Sinh: Nhan sắc, đàn, thơ, chữ — thứ nào cũng đáng tiền.

# speaker:Kieu
# emotion:disgust_hidden
# camera:CU_Kieu
Kiều: Vậy ra hai người bàn chuyện về ta ngay trước mặt ta, vì nghĩ món hàng không biết nghe?

# speaker:TuBa
# emotion:amused
Tú Bà: Ta thích con bé này. Miệng sắc. Nếu biết dùng đúng chỗ, lại càng sinh tiền.

# speaker:Narrator
# camera:Insert_Seal
# clue:con_dau_khac
Tú Bà ném tờ khế lên bàn. Kiều thoáng thấy con dấu dưới khế khác hẳn con dấu Mã Giám Sinh đã dùng ở Vương gia.

# speaker:TuBa
Tú Bà: Từ hôm nay, nợ của ngươi thuộc về ta. Ngoan ngoãn thì dễ sống.

# speaker:Kieu
# emotion:controlled
Kiều: Ta có thể xin một đêm suy nghĩ không?

# speaker:TuBa
# emotion:suspicious
Tú Bà: Một đêm thôi. Cửa khóa. Sân có người canh. Đừng làm chuyện ngu ngốc.

# speaker:Narrator
# scene:TuBa_PhongKieu
Đêm xuống. Kiều có rất ít thời gian. Nàng chỉ kịp làm hai việc trước khi nhà đổi phiên canh.

-> c2_hub


// Hub khám phá: Unity bật Explore mode vì mọi choice đều có #explore.
// Người chơi đi tới vật thể/khu vực tương ứng và bấm E để chọn.
=== c2_hub ===
{viec_c2 >= 2:
    -> c2_escape_setup
}

* [Lẻn đến phòng sổ sách của Tú Bà. #explore:c2_phong_so_sach]
    -> c2_evidence

* [Quan sát lối đi và tìm cửa sau. #explore:c2_cua_sau]
    -> c2_backdoor

* [Nghe lén cuộc cãi vã giữa Tú Bà và Mã Giám Sinh. #explore:c2_hanh_lang_giua]
    -> c2_eavesdrop


=== c2_evidence ===
~ viec_c2 += 1
# scene:TuBa_SoSach
# music:stealth
# camera:OTS_Kieu_Ledger
# speaker:Narrator
Kiều men theo hành lang, dừng trước phòng sổ sách. Cửa không khóa hẳn — Tú Bà quá tin vào người canh ngoài sân.

# speaker:Narrator
Trong ngăn bàn có hai tờ giấy đặt cạnh nhau: một "hôn ước" mang tên Kiều, và một sổ mua bán ghi rõ Mã Giám Sinh nhận tiền môi giới.

* [Lấy cả trang sổ có tên mình.]
    ~ bang_chung = true
    # speaker:Kieu
    # emotion:focused
    Kiều: Một tờ giấy biến người thành món hàng. Vậy thì một tờ giấy khác cũng có thể chứng minh kẻ nào đã bán người.

    # speaker:Narrator
    # evidence:trang_so_mua_ban
    Nàng gấp trang sổ, giấu vào lớp áo trong.

* [Không lấy, chỉ ghi nhớ nội dung để tránh bị phát hiện.]
    ~ tinh_tao += 1
    # speaker:Narrator
    # clue:noi_dung_so_sach
    Kiều đọc thật nhanh từng tên, từng khoản tiền. Nàng không mang được vật chứng, nhưng đã biết trò lừa vận hành thế nào.

- -> c2_hub


=== c2_backdoor ===
~ viec_c2 += 1
~ biet_cua_sau = true
# scene:TuBa_HanhLangSau
# camera:Tracking_Kieu
# speaker:Narrator
Kiều giả vờ đi lấy nước rồi đếm từng nhịp chân của người canh.

# speaker:Narrator
Cuối hành lang có một cửa nhỏ dẫn ra ngõ chợ. Khóa cửa treo bên trong, nhưng then gỗ đã mục.

* [Thử tháo then ngay.]
    # speaker:Narrator
    # sfx:wood_creak
    # clue:cua_sau
    Kiều nới then ra vừa đủ, rồi đặt lại như cũ. Chỉ cần một cú đẩy đúng lúc.

* [Đánh dấu đường đi rồi quay về phòng.]
    ~ tinh_tao += 1
    # speaker:Kieu
    # emotion:thoughtful
    # clue:cua_sau
    Kiều: Trốn không khó. Khó là trốn sao để họ không kịp bắt lại.

- -> c2_hub


=== c2_eavesdrop ===
~ viec_c2 += 1
~ nghe_len = true
# scene:TuBa_Corridor
# camera:CU_Kieu_Listens
# speaker:MaGiamSinh
# emotion:angry
Mã Giám Sinh: Ta mang người đến, bà còn đòi bớt tiền?

# speaker:TuBa
# emotion:mocking
Tú Bà: Ngươi dùng con dấu giả mà còn muốn chia đủ? Nếu quan phủ ngửi thấy mùi, người vào ngục trước là ngươi.

# speaker:Narrator
Kiều nín thở.

# speaker:MaGiamSinh
Mã Giám Sinh: Bà cũng sạch sẽ gì? Sổ nợ trong phòng kia đủ chôn cả hai ta.

~ tinh_tao += 2

# speaker:Kieu
# emotion:realization
# clue:con_dau_gia
Kiều: Thì ra không phải ta mắc nợ họ. Chính họ đang sợ món nợ của mình bị phơi ra ánh sáng.

-> c2_hub


=== c2_escape_setup ===
# scene:TuBa_PhongKieu
# music:tension_rise
# speaker:Narrator
Canh ba. Tú Bà bước vào phòng Kiều.

# speaker:TuBa
# emotion:stern
Tú Bà: Nghĩ xong chưa? Sáng mai ta cho người dạy ngươi quy củ.

# speaker:Kieu
# emotion:calm
Kiều: Ta chỉ có một thắc mắc.

# speaker:TuBa
Tú Bà: Nói.

{nghe_len:
    # speaker:Kieu
    Kiều: Mã Giám Sinh bảo bà trả thiếu tiền cho hắn.

    # speaker:TuBa
    # emotion:angry
    Tú Bà: Hắn nói thế?

    # speaker:Kieu
    Kiều: Hắn còn nói... bà giữ sổ sách chẳng kín.
- else:
    # speaker:Kieu
    Kiều: Con dấu trên tờ khế của bà... khác con dấu Mã Giám Sinh đóng ở nhà ta. Bà bỏ bạc ra mua một tờ giấy giả mà không hay sao?

    # speaker:TuBa
    # emotion:angry
    Tú Bà: Tên họ Mã dám lừa cả ta?
}

# speaker:Narrator
Ngoài sân, đúng lúc Mã Giám Sinh vừa quay lại đòi tiền.

# speaker:MaGiamSinh
# emotion:shouting_offscreen
Mã Giám Sinh: Tú Bà! Ra đây nói cho rõ!

# speaker:TuBa
# emotion:furious
Tú Bà: Tên khốn này!

# speaker:Narrator
Tú Bà bỏ đi. Kiều chỉ có vài nhịp thở.

* {biet_cua_sau} [Chạy thẳng đến cửa sau đã chuẩn bị.]
    # speaker:Narrator
    # camera:RunCam_Kieu
    Kiều lao qua hành lang, đẩy mạnh then gỗ đã nới.

    # speaker:Narrator
    # sfx:door_burst
    Cửa bật ra. Gió đêm quất vào mặt nàng như một cái tát — đau, nhưng là cái đau của tự do.

* {not biet_cua_sau} [Dùng lúc hỗn loạn tìm đường qua bếp.]
    ~ tinh_tao += 1
    # speaker:Narrator
    Kiều luồn qua bếp, hất một rổ củi chắn lối rồi trèo qua cửa sổ thấp.

    # speaker:Narrator
    Sau lưng nàng, tiếng Tú Bà gào lên.

- -> c2_chase


=== c2_chase ===
# scene:PhoDem_Chase
# music:chase
# speaker:TuBa
# emotion:rage
# camera:TuBa_Doorway
Tú Bà: BẮT NÓ LẠI!

// Unity chạy đoạn gameplay chạy trốn (1–2 phút) sau khi người chơi đọc xong dòng này.
# speaker:Narrator
# gameplay:escape_c2
Kiều chạy xuyên qua ngõ chợ tối. Dép rơi mất một chiếc. Tóc xổ khỏi trâm. Tiếng chân phía sau mỗi lúc một gần.

# speaker:Kieu
# emotion:exhausted_determined
Kiều: Lần đầu ta rời nhà vì muốn cứu người thân.

# speaker:Kieu
Kiều: Lần này ta chạy... để cứu chính mình.

# speaker:Narrator
Nàng lao qua cầu gỗ, đẩy chiếc xe hàng chắn ngang lối. Khi bọn người đuổi tới, Kiều đã hòa vào dòng người chuẩn bị cho chợ sớm.

# speaker:Narrator
Đến khi trời sáng, trong tay nàng không có nhà, không có người che chở — và gần như không có tiền.

# speaker:Kieu
# emotion:tired_small_smile
Kiều: Tự do thật đắt.

# speaker:Kieu
Kiều: Nhưng ít nhất... lần này người trả giá là ta.

-> chapter_3


// ============================================================
// CHƯƠNG 3 — KIẾM TIỀN CÓ GÌ KHÓ?
// Chủ đề: Kiếm tiền mà không trở thành người từng lợi dụng mình.
// Thời lượng dự kiến: 5–7 phút. Chế độ: Explore (hub) + 3 hoạt động, chọn 2.
// Mọi tổ hợp 2 hoạt động đều cho >= MUC_TIEU_TIEN (tối thiểu 410 lạng).
// ============================================================

=== chapter_3 ===
# chapter:3
# scene:ThiTran_BinhMinh
# music:hopeful
# speaker:Narrator
Kiều đến một thị trấn cách đó nửa ngày đường. Nàng thuê một góc phòng nhỏ bằng chiếc trâm cuối cùng còn giữ được.

# speaker:Narrator
Tin từ quê gửi đến: Mã Giám Sinh đã mang tờ khế lên quan, đòi Vương gia hoặc giao người, hoặc hoàn lại bốn trăm lạng. Án của Vương Ông cũng chưa khép lại.

# speaker:Kieu
# emotion:thinking
# camera:CU_Kieu_Table
Kiều: Ta từng nghĩ bán mình là cách nhanh nhất để có tiền.

# speaker:Kieu
Kiều: Có lẽ vì ta chưa từng thử định giá tài năng của chính mình.

# speaker:Narrator
Trong những ngày tiếp theo, Kiều bắt đầu kiếm sống bằng điều nàng thật sự sở hữu: tiếng đàn, con chữ và đầu óc.

-> c3_hub


// Hub khám phá: 3 khu vực trong thị trấn, người chơi chọn 2.
=== c3_hub ===
{viec_c3 >= 2:
    -> c3_after_work
}

* [Nhận đàn tại một trà quán đông khách. #explore:c3_tra_quan]
    -> c3_music

* [Nhận viết thư thuê cho một khách buôn trẻ. #explore:c3_sap_thu]
    -> c3_letter

* [Giúp một cửa hàng vải thương lượng với khách lớn. #explore:c3_cho_vai]
    -> c3_trade


// ---------- HOẠT ĐỘNG 1: ĐÀN (mini-game nhịp điệu do C# chạy) ----------
=== c3_music ===
~ da_dan = true
~ viec_c3 += 1
~ kq_minigame = -1
# scene:TraQuan
# music:none
# camera:Stage_Kieu
# speaker:Narrator
Trà quán đông nghịt. Chủ quán dành cho Kiều một góc nhỏ cạnh quầy.

# speaker:Narrator
# minigame:dan
Kiều nâng đàn. Khúc nhạc đầu tiên bắt đầu.

{
- kq_minigame >= 2:
    ~ tien += 40
    ~ danh_tieng += 1
    # speaker:Narrator
    # music:dan_tranh
    Cả trà quán lặng đi. Đến nốt cuối, người ta mới nhớ ra mình còn đang cầm chén trà.
- kq_minigame == 1:
    ~ tien += 20
    # speaker:Narrator
    # music:dan_tranh
    Tiếng đàn tròn trịa. Vài người khách gõ nhịp theo lên mặt bàn.
- kq_minigame == 0:
    # speaker:Narrator
    # music:dan_tranh
    Có vài nốt lạc. Kiều cười, đàn lại từ đầu. Khách vẫn vỗ tay — vì hiếm ai dám đàn sai mà không đỏ mặt.
- else:
    # speaker:Narrator
    # music:dan_tranh
    Tiếng đàn vang lên giữa trà quán ồn ã.
}
~ kq_minigame = -1

# speaker:Narrator
Sau khúc đàn, chủ quán đặt trước Kiều hai túi bạc.

# speaker:ChuQuan
# emotion:tempting
Chủ quán: Khách ở phòng riêng muốn cô nương sang đàn thêm. Họ trả gấp đôi. Chỉ cần... biết chiều ý một chút.

# speaker:Narrator
Câu nói ấy khiến Kiều nhớ đến ánh mắt của Tú Bà.

* [Chỉ nhận đàn ở gian công khai.]
    ~ tien += 220
    ~ danh_tieng += 2
    ~ tu_trong += 1
    # speaker:Kieu
    # emotion:firm_smile
    Kiều: Ta bán tiếng đàn, không bán quyền định đoạt ta. Nếu khách muốn nghe, mời họ ngồi ngoài này.

    # speaker:Narrator
    Vài người cười. Vài người khó chịu. Nhưng sáng hôm sau, trà quán lại đông hơn vì người ta truyền nhau về "cô nương đánh đàn không chịu cúi đầu".

* [Nhận lời, nhưng yêu cầu cửa mở và chủ quán đứng làm chứng.]
    ~ tien += 250
    ~ tinh_tao += 1
    ~ danh_tieng += 1
    # speaker:Kieu
    # emotion:clever
    Kiều: Ta nhận. Nhưng cửa phải mở, chủ quán đứng ngoài, tiền trả trước.

    # speaker:ChuQuan
    Chủ quán: Cô nương đề phòng quá.

    # speaker:Kieu
    Kiều: Người từng suýt bị bán hai lần thường học nhanh.

- -> c3_hub


// ---------- HOẠT ĐỘNG 2: VIẾT THƯ (mini-game chọn câu, Ink tự chấm) ----------
=== c3_letter ===
~ da_viet_thu = true
~ viec_c3 += 1
~ diem_thu = 0
# scene:PhoSach
# camera:TwoShot_Kieu_Khach
# speaker:Narrator
Một khách buôn trẻ tìm đến nhờ Kiều viết thư cầu hôn con gái một nhà giàu.

# speaker:KhachBuon
# emotion:awkward
Khách buôn: Cô cứ viết rằng ta có ba cửa hàng, hai kho lụa và một căn nhà lớn.

# speaker:Kieu
# emotion:neutral
Kiều: Công tử có thật không?

# speaker:KhachBuon
Khách buôn: Chưa. Nhưng cưới xong biết đâu sẽ có.

# speaker:Kieu
Kiều: À. Công tử muốn thuê ta viết thư hay thuê ta nói dối?

* [Viết một lá thư chân thành, bỏ hết tài sản giả.]
    ~ tien += 210
    ~ danh_tieng += 2
    ~ tu_trong += 1
    ~ thu_that = true
    # speaker:Kieu
    # emotion:warm
    Kiều: Nếu nàng ấy thích một người không tồn tại, thì đến ngày gặp công tử thật, cả hai đều khổ.

* [Viết lá thư bóng bẩy, nhưng buộc khách ký tên vào lời mình nói.]
    ~ tien += 240
    ~ tinh_tao += 1
    # speaker:Kieu
    # emotion:dry_humor
    Kiều: Ta sẽ viết hay. Nhưng mọi điều về tài sản, công tử tự ký xác nhận phía dưới.

    # speaker:KhachBuon
    Khách buôn: Sao phải rắc rối vậy?

    # speaker:Kieu
    Kiều: Vì ta vừa học được rằng giấy trắng mực đen rất có ích khi người ta định quên lời mình.

- (viet_thu)
# speaker:Narrator
# ui:letter
# camera:Insert_Letter
Kiều trải giấy, mài mực. Câu mở đầu quyết định người đọc có đọc tiếp hay không.

* ["Kính gửi tiểu thư. Kẻ hèn này xin mạo muội gửi đôi dòng." #ui:letter]
    ~ diem_thu += 1
* ["Nàng là vầng trăng soi sáng cả ba cửa hàng của ta!" #ui:letter]
* ["Ta muốn cưới nàng. Mong hồi âm sớm." #ui:letter]

- # speaker:Narrator
# ui:letter
Còn câu kết — câu người đọc sẽ nhớ lâu nhất.

* ["Nếu tiểu thư không chê, xin cho phép được đến thăm một lần." #ui:letter]
    ~ diem_thu += 1
* ["Nàng mà không đồng ý thì ta buồn đến chết mất." #ui:letter]
* ["Hạn hồi âm: ba ngày." #ui:letter]

- {diem_thu >= 2:
    ~ tien += 30
    ~ danh_tieng += 1
    # speaker:KhachBuon
    # emotion:impressed
    Khách buôn: Chữ đẹp, lời cũng đẹp. Ta trả thêm ba mươi lạng — đáng lắm!
- else:
    # speaker:KhachBuon
    Khách buôn: Ừm... thư cũng được. Ta gửi thử xem sao.
}

{thu_that:
    # speaker:Narrator
    Vài ngày sau, hắn quay lại — không có hôn lễ, nhưng có một lời cảm ơn.

    # speaker:KhachBuon
    Khách buôn: Nàng ấy từ chối ta... nhưng giới thiệu ta làm ăn với anh trai nàng. Kỳ lạ thật.

    # speaker:Kieu
    # emotion:warm_smile
    Kiều: Thành thật đôi khi lãi chậm. Nhưng ít khi phá sản.
}

-> c3_hub


// ---------- HOẠT ĐỘNG 3: THƯƠNG LƯỢNG (lựa chọn mở khóa theo chỉ số) ----------
=== c3_trade ===
~ da_buon_vai = true
~ viec_c3 += 1
# scene:ChoVai
# camera:Market_Wide
# speaker:Narrator
Một chủ hàng vải thuê Kiều giúp thương lượng với đoàn thương nhân phương xa.

# speaker:ChuHang
# emotion:whispering
Chủ hàng: Lô này bị ẩm một ít. Cô nương cứ khen là lụa thượng hạng. Chốt được giá cao, ta chia thêm bạc.

# speaker:Kieu
# emotion:disappointed
Kiều: Nếu họ phát hiện?

# speaker:ChuHang
Chủ hàng: Khi ấy họ đã đi xa rồi.

* [Nói rõ khuyết điểm, rồi thương lượng giá hợp lý.]
    ~ danh_tieng += 2
    ~ tu_trong += 1
    # speaker:Kieu
    # emotion:confident
    Kiều: Lụa có vết ẩm, nhưng sợi tốt. Ta nói trước để các vị khỏi phải tự phát hiện.

    # speaker:Narrator
    Trưởng đoàn thương nhân nhíu mày: lụa ẩm mà vẫn đòi giá?

    ** {tinh_tao >= 3} [Chỉ ra rằng vết ẩm biến mất khi nhuộm màu đậm. #req:tinh_tao]
        ~ tien += 230
        # speaker:Kieu
        # emotion:clever
        Kiều: Giảm một phần giá. Nhuộm chàm hoặc nâu, không ai nhận ra vết ẩm — các vị vẫn lời.
    ** {danh_tieng >= 3} [Đem uy tín của mình ra bảo đảm cho lô hàng. #req:danh_tieng]
        ~ tien += 230
        # speaker:Kieu
        # emotion:confident
        Kiều: Ở thị trấn này người ta biết tên ta. Nếu lô hàng có vấn đề ngoài vết ẩm, cứ đến tìm ta.
    ** [Chấp nhận giảm giá sâu để bán nhanh.]
        ~ tien += 200
        # speaker:Kieu
        Kiều: Giảm hẳn ba phần. Đổi lại, các vị lấy trọn lô hôm nay.

    -- # speaker:Narrator
    Đoàn thương nhân đồng ý. Chủ hàng kiếm ít hơn dự tính, nhưng bán sạch lô vải và còn có khách quay lại.

    # speaker:ChuHang
    Chủ hàng: Cô nương làm ta hụt một khoản hôm nay.

    # speaker:Kieu
    Kiều: Nhưng giữ cho ông một cái tên để ngày mai còn bán hàng.

* [Dùng lời khéo che đi vết ẩm để lấy giá cao.]
    ~ tien += 270
    ~ tu_trong -= 1
    ~ lam_sai += 1
    # speaker:Narrator
    Kiều nói rất hay. Đoàn thương nhân mua hàng với giá cao.

    # speaker:Narrator
    Nhưng khi cầm phần bạc được chia, nàng chợt nhớ Mã Giám Sinh cũng từng dùng lời đẹp để che một tờ khế xấu.

    # speaker:Kieu
    # emotion:uneasy
    Kiều: Tiền này nhẹ hơn ta tưởng... mà sao cầm nặng tay thế.

- -> c3_hub


=== c3_after_work ===
# scene:PhongTro_Dem
# music:reflective
# speaker:Narrator
Sau nhiều ngày, số bạc Kiều dành dụm cuối cùng cũng vượt qua khoản gia đình cần.

# speaker:Narrator
# sfx:coins
Nàng đặt từng túi bạc lên bàn.

# speaker:Kieu
# camera:CU_Kieu_Coins
Kiều: {tien} lạng. Hơn bốn trăm lạng.

# speaker:Kieu
# emotion:small_laugh
Kiều: Hóa ra kiếm tiền... đúng là khó.

# speaker:Narrator
Nàng cười một mình. Rồi nụ cười chậm rãi biến mất.

# speaker:Kieu
# emotion:reflective
Kiều: Nhưng khó nhất không phải kiếm đủ.

# speaker:Kieu
Kiều: Khó nhất là lúc thiếu tiền, mình rất dễ tin rằng có quyền làm điều sai với người khác để sống sót.

{lam_sai == 0:
    # speaker:Narrator
    Kiều nhìn số bạc. Không đồng nào khiến nàng phải tránh ánh mắt của chính mình.
- else:
    # speaker:Narrator
    Trong số bạc ấy có vài đồng khiến Kiều không thấy thanh thản. Nàng hiểu mình chưa khác những kẻ kia chỉ nhờ một quyết định; phải khác họ trong từng quyết định nhỏ.
}

# speaker:Narrator
Sáng hôm sau, Kiều lên đường về quê.

# speaker:Narrator
Nàng nghĩ mình chỉ cần trả nợ là mọi chuyện kết thúc.

# speaker:Narrator
Nhưng trước cổng Vương gia, hai người đã chờ sẵn.

-> chapter_4


// ============================================================
// CHƯƠNG 4 — KHÔNG AI ĐƯỢC ĐỊNH GIÁ TA
// Chủ đề: Tiền giải quyết món nợ; sự thật và cộng đồng giải quyết xiềng xích.
// Happy End bắt buộc; sắc thái đổi theo lựa chọn trước đó.
// Thời lượng dự kiến: 4–6 phút.
// Lựa chọn đối đầu: tối đa 4 (3 mở khóa theo chỉ số + 1 luôn có) => không phải "choice giả".
// ============================================================

=== chapter_4 ===
# chapter:4
# scene:VuongGia_BinhMinh
# music:final_tension
# camera:Wide_Kieu_Returns
# speaker:Narrator
Kiều trở về khi trời vừa sáng. Vương Ông đã được về nhà, nhưng vụ án vẫn chờ kết luận cuối cùng.

# speaker:VuongOng
# emotion:overwhelmed
# camera:CU_VuongOng
Vương Ông: Con... còn sống.

# speaker:Kieu
# emotion:teary_smile
Kiều: Con còn nợ cha một lần trở về.

# speaker:Narrator
# sfx:slow_clap
Kiều chưa kịp bước qua ngưỡng cửa thì tiếng vỗ tay vang lên phía sau.

# speaker:MaGiamSinh
# emotion:smug
Mã Giám Sinh: Cảm động lắm.

# speaker:TuBa
# emotion:cold
Tú Bà: Nhưng người của ta thì vẫn là người của ta.

# speaker:Kieu
# emotion:steady
# camera:Hero_Kieu_Turn
Kiều: Ta tưởng hai người bận cãi nhau chuyện chia tiền.

# speaker:TuBa
# emotion:angry
Tú Bà: Con ranh!

# speaker:MaGiamSinh
Mã Giám Sinh: Khế còn đây. Nàng đã ký. Theo luật, nàng phải theo chúng ta.

# speaker:VuongOng
# emotion:guilty
Vương Ông: Là tại cha... Nếu không vì cha...

# speaker:Kieu
# emotion:firm_soft
Kiều: Không.

# speaker:Kieu
Kiều: Cha gặp nạn không biến con thành tài sản của bất kỳ ai.

# speaker:Narrator
# sfx:coins
Kiều đặt túi bạc xuống bàn đá trước sân.

# speaker:Kieu
Kiều: Đây là bốn trăm lạng Mã công tử đã đưa. Món nợ tiền bạc, ta trả bằng tiền.

# speaker:Kieu
Kiều: Còn món "nợ" mà hai người tự viết lên đời ta... hôm nay ta trả bằng sự thật.

-> c4_confront


=== c4_confront ===
// Mỗi lựa chọn mở khóa bằng việc người chơi đã làm; lựa chọn cuối luôn có.
* {bang_chung} [Đưa trang sổ mua bán ra trước mọi người. #req:bang_chung]
    -> c4_use_evidence
* {tinh_tao >= NGUONG_TINH_TAO} [Buộc Mã Giám Sinh và Tú Bà tự mâu thuẫn trước mặt nhân chứng. #req:tinh_tao]
    -> c4_use_logic
* {danh_tieng >= NGUONG_DANH_TIENG} [Kêu gọi những người từng làm ăn với Kiều đứng ra làm chứng. #req:danh_tieng]
    -> c4_use_reputation
* [Kể lại mọi chuyện, không cần ai tin trước.]
    -> c4_use_truth


=== c4_use_evidence ===
# camera:Insert_Ledger
# speaker:Kieu
# emotion:confident
Kiều: Tờ khế của ông gọi là hôn ước. Nhưng sổ của Tú Bà ghi rõ: "tiền mua người" và "phí môi giới cho Mã Giám Sinh".

# speaker:MaGiamSinh
# emotion:panic
Mã Giám Sinh: Ngươi ăn cắp sổ!

# speaker:Kieu
Kiều: Vậy ông thừa nhận sổ này là thật?

# speaker:Narrator
Mã Giám Sinh cứng họng.

# speaker:TuBa
# emotion:rage
Tú Bà: Đồ ngu!

# speaker:Narrator
Người dân trước cổng bắt đầu xì xào. Một quan sai đang chờ xét lại vụ án Vương gia bước tới nhận trang sổ.

# speaker:QuanSai
Quan sai: Giấy này đủ để mở một vụ khác. Mời hai người theo chúng ta.

~ danh_tieng += 2
-> c4_resolution


=== c4_use_logic ===
# speaker:Kieu
# emotion:calm_trap
Kiều: Mã công tử nói ta là vợ ông. Tú Bà lại nói ta là người của bà. Một người không thể vừa là vợ của người này, vừa là tài sản của người kia.

{nghe_len:
    # speaker:Kieu
    Kiều: Huống hồ đêm ấy chính tai ta nghe hai vị cãi nhau về một con dấu giả.
}

# speaker:Kieu
Kiều: Vậy xin hai vị nói trước mặt quan sai: rốt cuộc bốn trăm lạng kia là sính lễ hay tiền mua bán?

# speaker:MaGiamSinh
# emotion:panicked
Mã Giám Sinh: Tất nhiên là sính lễ!

# speaker:TuBa
# emotion:angry_interrupt
Tú Bà: Sính lễ cái gì? Ta đã trả ngươi tiền môi giới—

# speaker:TuBa
# emotion:frozen
Tú Bà: ...

# speaker:Narrator
Sân im bặt.

# speaker:Kieu
# emotion:small_smile
Kiều: Cảm ơn bà. Phần còn lại chắc quan sai hỏi sẽ rõ hơn.

# speaker:QuanSai
Quan sai: Hai người, theo chúng ta.

~ danh_tieng += 1
-> c4_resolution


=== c4_use_reputation ===
# speaker:Narrator
Kiều không cần sổ sách trong tay. Nàng có những người từng làm việc cùng mình.

# speaker:TuBa
# emotion:smug
Tú Bà: Không bằng không chứng. Ai tin ngươi?

# speaker:Narrator
Từ ngoài cổng, những người Kiều từng giúp lần lượt bước tới.

{da_dan:
    # speaker:ChuQuan
    Chủ quán: Ta tin. Cô ấy đàn ở trà quán ta, chưa từng bước vào phòng riêng nào.
}
{da_viet_thu:
    # speaker:KhachBuon
    Khách buôn: Ta cũng vậy. Cô ấy làm việc bằng tên thật, nhận tiền bằng công sức thật.
}
{da_buon_vai:
    # speaker:ChuHang
    Chủ hàng: Cô ấy thà làm ta hụt tiền còn hơn nói dối khách. Người như thế không phải hàng của ai cả.
}

# speaker:Narrator
Một người không đủ sức chống lại một tờ khế. Nhưng nhiều người cùng lên tiếng khiến quan sai không thể làm ngơ.

# speaker:QuanSai
Quan sai: Việc này còn nhiều nghi vấn. Tạm giữ hai người để đối chất.

# speaker:Kieu
# emotion:relieved
Kiều: Có những ngày danh tiếng không mua được cơm.

# speaker:Kieu
Kiều: Nhưng hôm nay, nó mua cho ta quyền được người khác lắng nghe.

-> c4_resolution


// Nhánh luôn có: yếu hơn, nhưng vẫn dẫn tới Happy End.
=== c4_use_truth ===
# speaker:Kieu
# emotion:steady
Kiều: Ta không có sổ sách, cũng chẳng có ai đứng sau. Ta chỉ có những gì ta đã thấy.

{doc_ky_khe:
    # speaker:Kieu
    # camera:Insert_Contract
    Kiều: Khế này có câu: "Người nhận có toàn quyền định đoạt nơi ở và công việc của người giao". Hôn ước nào lại viết như thế?
- else:
    # speaker:Kieu
    Kiều: Xin quan sai đọc kỹ tờ khế. Ta đã ký vội, nhưng chữ nghĩa trong đó không biết nói dối.
}

# speaker:TuBa
# emotion:smug
Tú Bà: Lời một đứa con gái bỏ trốn thì đáng mấy đồng?

# speaker:VuongOng
# emotion:determined
Vương Ông: Đáng hơn tờ giấy của các người. Con ta ra đi để cứu ta — và ta sẽ đứng trước công đường mà nói điều đó.

# speaker:Narrator
Quan sai cầm tờ khế lên, đọc lâu hơn cần thiết.

# speaker:QuanSai
Quan sai: Khế này có điều khuất tất. Mời hai người lên nha môn giải trình.

# speaker:Kieu
# emotion:relieved
Kiều: Hóa ra sự thật không cần to tiếng. Chỉ cần có người chịu đọc.

-> c4_resolution


=== c4_resolution ===
# music:release
# scene:VuongGia_Sunrise
# speaker:Narrator
Mã Giám Sinh và Tú Bà bị áp giải đi để điều tra việc dùng khế giả và mua bán người.

# speaker:Narrator
Số bạc Kiều mang về giúp Vương gia hoàn lại khoản bạc ban đầu, và thuê người theo vụ án đến cùng.

# speaker:VuongOng
# emotion:ashamed
Vương Ông: Cha từng nghĩ con hy sinh vì gia đình là điều đáng tự hào.

# speaker:VuongOng
Vương Ông: Bây giờ cha mới hiểu... nếu một gia đình chỉ sống được bằng cách đẩy một người xuống vực, thì đó không phải là cứu.

# speaker:Kieu
# emotion:gentle
Kiều: Con vẫn chọn cứu cha nếu phải làm lại.

# speaker:Kieu
Kiều: Nhưng con sẽ không còn nghĩ chữ Hiếu bắt mình phải biến mất.

# speaker:Narrator
Một giọng quen thuộc vang lên ngoài cổng.

# speaker:KimTrong
# emotion:breathless
# camera:Reveal_KimTrong
Kim Trọng: Kiều!

# speaker:Narrator
Kim Trọng vừa trở về sau chuyến đi xa. Chàng nhìn túi bạc, quan sai phía xa, chiếc dép không cùng đôi của Kiều... rồi im lặng khá lâu.

# speaker:KimTrong
# emotion:confused_worried
Kim Trọng: Ta đi có mấy tuần.

# speaker:KimTrong
Kim Trọng: Nàng... đã làm gì vậy?

# speaker:Kieu
# emotion:deadpan_then_smile
# camera:CU_Kieu_Final
Kiều: Không có gì.

# speaker:Kieu
Kiều: Cứu cha. Trốn khỏi một cuộc mua bán. Chạy qua nửa cái chợ.{da_dan: Đàn thuê.}{da_viet_thu: Viết thư thuê.}{da_buon_vai: Buôn vải.} Kiếm {tien} lạng. Rồi về nhà.

# speaker:KimTrong
# emotion:stunned
Kim Trọng: ...Không có gì?

# speaker:Kieu
# emotion:warm_smile
Kiều: Ừ.

# speaker:Kieu
Kiều: Kiếm tiền có gì khó nhỉ?

# speaker:Narrator
# camera:Wide_Family_Sunrise
Nắng sớm rơi xuống sân Vương gia.

# speaker:Narrator
Kiều đã không thắng số phận bằng cách trở nên mạnh hơn tất cả mọi người.

# speaker:Narrator
Nàng thắng bằng cách thôi tin rằng số phận của mình phải do người khác định giá.

// 3 sắc thái Happy End
{
- lam_sai > 0:
    # ending:reflective
    # speaker:Narrator
    Kiều không tự nhận mình đã luôn đúng. Có những đồng bạc nàng kiếm được bằng cách khiến người khác chịu phần thiệt. Nàng giữ chúng như lời nhắc rằng tự do không chỉ là thoát khỏi kẻ xấu — mà còn là không trở thành họ.
- tu_trong >= NGUONG_TU_TRONG:
    # ending:bright
    # speaker:Narrator
    Từ đó, Kiều mở một lớp nhỏ dạy đàn và chữ cho những cô gái muốn tự kiếm sống. Người ta nhớ tiếng đàn của nàng, nhưng nàng muốn họ nhớ hơn cả một điều: tài năng có thể nuôi mình mà không cần bán mình.
- else:
    # ending:warm
    # speaker:Narrator
    Kiều trở lại với cây đàn và trang giấy. Nàng không còn sợ bị định giá — vì lần đầu tiên, chính nàng biết mình đáng giá bao nhiêu.
}

# speaker:Narrator
# music:main_theme
# camera:TitleCard
HAPPY END — THÚY KIỀU: KIẾM TIỀN CÓ GÌ KHÓ NHỈ?

-> END


// ============================================================
// DEBUG — dành cho QA/dev. Unity gọi story.ChoosePathString("debug_menu").
// Không nằm trong luồng chơi chính.
// ============================================================

=== debug_menu ===
Chọn điểm bắt đầu:
+ [Chương 1] -> chapter_1
+ [Chương 2] -> chapter_2
+ [Chương 3 (đủ manh mối)]
    ~ tinh_tao = 4
    ~ bang_chung = true
    ~ biet_cua_sau = true
    -> chapter_3
+ [Chương 4 — có bằng chứng, trong sạch]
    ~ tinh_tao = 4
    ~ bang_chung = true
    ~ nghe_len = true
    ~ danh_tieng = 4
    ~ tu_trong = 4
    ~ tien = 460
    ~ da_dan = true
    ~ da_viet_thu = true
    -> chapter_4
+ [Chương 4 — không bằng chứng, từng làm sai]
    ~ tinh_tao = 1
    ~ danh_tieng = 1
    ~ tu_trong = 1
    ~ tien = 510
    ~ lam_sai = 1
    ~ da_dan = true
    ~ da_buon_vai = true
    ~ ky_ngay = true
    -> chapter_4
