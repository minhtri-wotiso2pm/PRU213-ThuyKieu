// ============================================================
// THÚY KIỀU: NGHỊCH MỆNH
// Kịch bản chính — Mở đầu (Trang sách), Chương 1 (Gia Biến), Chương 2 (Trên đường theo Mã Giám Sinh),
// Chương 3 (Tú Bà), Chương 4 (Không còn đường lui), Chương 5 (Đào thoát), Chương 6 (Kim Trọng),
// Chương 7 (Nghịch Mệnh), Kết (Trở lại lớp học).
// Điểm vào: Mở đầu chạy knot "prologue" (scene Chapter00_Prologue); chơi thẳng Chương 1 thì bắt đầu từ gốc.
// Ink cũ Chương 2–4 lưu tại Legacy/ (không còn được include).
//
// Hợp đồng tag với Unity:
//   # speaker:<id>  # emotion:<id>  # camera:<id>  # scene:<id>  # music:<id>  # sfx:<id>
//   # chapter:<n>   # objective:<text>   # clue:<id>   # gameplay:<id> (chặn, chạy sau dòng)
//   # timer:<giây>  (đặt trên dòng ngay trước lựa chọn → lựa chọn có giới hạn thời gian)
//   Tag trên choice: #explore:<id> (chọn bằng tương tác trong thế giới), #timeout (mặc định khi hết giờ)
// Speaker: Narrator, Kieu, NoiTam (ý nghĩ nhân vật hiện đại), KieuThat (giọng Kiều thật), TrangSach (câu thơ ký ức),
//          MeKieu, MaGiamSinh, TuyTung, TuBa, MaKieu, CoHong, CoLam, NguoiCanh,
//          KimTrong, BaHangNuoc, ThuLai, ChuHieuBac, GiaNhan,
//          HocSinh (nhân vật chính ở thời hiện đại, hiển thị "Tôi"), Ngan, ThayGiao, VuongOng, VuongQuan, QuanSai,
//          QuanHuyen, NhaDich
// ============================================================

// --- Mở đầu ---
VAR pl_che_kieu = 0       // 1 "dễ, sẽ làm tốt hơn" · 2 "phải có cách khác" · 3 "Kiều cam chịu" / im lặng
VAR pl_vat = 0            // số vật đã xem trong phòng Kiều
VAR pl_guong = false
VAR pl_dan = false
VAR pl_tho = false
VAR pl_hanh_dong = 0      // khi quan sai đến: 1 lao ra cản · 2 hỏi tội danh · 3 đứng nhìn ghi nhớ

// --- Chương 1 ---
VAR tinh_tao = 0
VAR danh_tieng = 0
VAR tu_trong = 2
VAR tien = 0
VAR doc_ky_khe = false
VAR c1_viec = 0           // số việc đã làm trong nhà trước khi ra mở cổng (hỏi Mẹ, tìm bản khai)
VAR c1_biet_moi = false   // đã nghe Mẹ Kiều kể chuyện bà mối
VAR c1_manh_moi = 0       // số manh mối cốt lõi Chương 1 (tối đa 3)
VAR c1_nghi_ngo = 0       // 0–100: nghi ngờ của Mã Giám Sinh trong buổi dạm hỏi. Unity cộng khi nhìn lộ liễu. 100 = bị lộ (checkpoint)
VAR c1_hoi_xong = false   // đã đáp lời hắn (mở tờ khế)
VAR c1_hoi_tien = false   // đã biết lời hứa của bà mối: bạc phải giao trước
VAR c1_bac_dau = false    // đã thấy dấu Phúc Ký trên nén bạc
VAR c1_tam_biet = false   // đã từ biệt Mẹ Kiều
VAR c1_thoa = false       // bí mật: nhặt chiếc thoa vàng Kim Trọng trả lại, rơi dưới chân tủ (Unity ghi, kt_tin_tuong +1)

// --- Chương 2 ---
VAR nghi_ngo = 0          // 0–100. Unity cộng theo hành vi, Ink cộng theo lời nói. 100 = bị lộ (checkpoint). Bắt đầu ở 15 nếu Ch1 kết thúc với c1_nghi_ngo ≥ 60
VAR manh_moi = 0          // số manh mối Chương 2
VAR mm_ban_tay = false    // quan sát hắn đếm bạc
VAR mm_hoi_duong = false  // hỏi được nơi đến: Lâm Tri
VAR mm_mau_giay = false   // đọc mẩu giấy "giao hàng"
VAR mm_nghe_len = false   // nghe "Tú Bà đã chuẩn bị chưa?"
VAR giu_mau_giay = false  // giấu mẩu giấy làm bằng chứng
VAR suy_luan = 0          // điểm bảng suy luận (0–3)
VAR gia_vo = 0            // điểm giả vờ trước mặt Mã Giám Sinh (0–3)

// --- Chương 3 ---
VAR c3_viec = 0           // số việc khám phá đã làm ở nhà Tú Bà
VAR c3_noi_chuyen = 0     // số cô gái đã nói chuyện
VAR c3_kiem_cua = false
VAR c3_nghe_len = false
VAR c3_khe_uoc = false    // đã đọc văn tự bán thân
VAR c3_so_no = false      // đã thấy sổ nợ son phấn
VAR c3_biet_chia_khoa = false
VAR c3_bi_bat = 0         // số lần bị người canh bắt gặp (Unity ghi)
VAR tra_loi_kieu_that = 0 // 1 nhận sai · 2 không bỏ cuộc · 3 im lặng

// --- Chương 4 ---
VAR c4_viec = 0           // số việc đã làm trong đêm
VAR c4_canh_giac = 0      // 0–2: Tú Bà cảnh giác thế nào sau buổi "dạy quy củ" (Unity: người canh nhạy hơn)
VAR c4_biet_no = false    // đã hỏi ra số nợ
VAR c4_quan_sat = false   // đã quan sát lịch đi lại của Tú Bà
VAR c4_chia_khoa = false  // đã lấy chùm chìa
VAR c4_khe_uoc_mg = false // đã lấy giao kèo Mã Giám Sinh – Tú Bà
VAR c4_van_tu = 0         // văn tự bán thân: 0 để lại · 1 mang theo · 2 đốt
VAR c4_tien = 0           // 0 không lấy · 1 lộ phí · 2 cả túi bạc có dấu
VAR c4_so_no = false      // đã xé trang sổ nợ
VAR c4_cua_sau = false    // đã tìm ra cửa sau
VAR c4_gap_co_lam = false
VAR c4_ru_co_lam = false  // rủ cô gái áo lam cùng trốn
VAR c4_bi_bat = 0         // số lần bị bắt gặp (Unity ghi)

// --- Chương 5 ---
VAR c5_lang_le = 0        // 0–3: lẻn ra êm đến đâu (Unity ghi)
VAR c5_bi_bat = 0         // số lần bị bắt khi chạy (Unity ghi)
VAR c5_duong = 0          // 1 ngõ tắt · 2 chợ đêm (Unity ghi)
VAR c5_xo_do = 0          // số vật cản đã xô đổ (Unity ghi)
VAR c5_qte = 0            // 0–3: rượt đuổi trong rừng tre (Unity ghi)
VAR c5_co_lam_thoat = false // cô gái áo lam thoát cùng (Unity ghi)

// --- Chương 6 ---
VAR kt_tin_tuong = 0      // độ tin tưởng của Kim Trọng
VAR kt_biet_xuyen_khong = false
VAR c6_doi_chieu = 0      // 0–3: đối chiếu giấy tờ (Unity ghi)
VAR c6_thu = 0            // số lần đưa sai bằng chứng với nhân chứng hiện tại
VAR c6_xong = 0           // số nơi đã đến trên phố
VAR c6_ba_nuoc_xong = false
VAR c6_thu_lai_xong = false
VAR c6_hieu_bac_xong = false
VAR c6_co_lam_xong = false
VAR nc_ba_nuoc = false
VAR nc_thu_lai = false
VAR nc_co_lam = false
VAR c6_ho_so = 0          // 0–4: số nhóm bằng chứng đã đủ

// --- Hồ sơ cho Chương 7 ---
VAR ev_khe_uoc = false    // giao kèo Mã Giám Sinh – Tú Bà
VAR ev_nhan_chung = 0     // số nhân chứng
VAR ev_giay_no = false    // trang sổ nợ
VAR ev_tien = false       // bằng chứng tiền bạc (bạc có dấu hoặc sổ hiệu bạc)
VAR c6_so_bac = false     // đã xem được sổ hiệu bạc Phúc Ký

// --- Chương 7 ---
VAR c7_viec = 0           // số việc chuẩn bị trước cổng huyện đường
VAR c7_nhan_chung_vung = false // đã động viên các nhân chứng (không ai lùi bước khi Tú Bà dọa)
VAR c7_ke_hoach_kt = false     // đã bàn kế với Kim Trọng (chàng cứu nguy một lần khi quan mất kiên nhẫn)
VAR c7_biet_hoi_lo = false     // thấy Tú Bà đút lót nha dịch
VAR c7_lui = 0            // nhân chứng lùi bước sau lời dọa: 1 chị Lam · 2 bà hàng nước · 3 thư lại
VAR c7_r1 = 0             // điểm vòng đối chất 1–3 (Unity ghi)
VAR c7_r2 = 0
VAR c7_r3 = 0
VAR c7_diem = 0           // tổng điểm đối chất (Unity ghi)
VAR c7_long_tin = 0       // lòng tin của quan huyện khi kết thúc, 0–100 (Unity ghi)
VAR c7_thua_vong = 0      // số vòng đối chất bị thua: quan mất kiên nhẫn, tự xét giấy tờ (Unity ghi)
VAR c7_diem_pq = 0        // điểm phán quyết = lòng tin + bảo lãnh + nhân chứng + lục bát − vòng thua
VAR c7_ket = 0            // kết: 0 thua kiện (nguyên tác) · 1 chật vật · 2 ấm · 3 trọn vẹn
VAR ket_hon = 0           // kết Nghịch Mệnh: 1 hai linh hồn chia tay · 2 cùng sống trong một thân

// --- Trang Sách · Định Mệnh · Hai linh hồn (xem docs/codex/TRANG_SACH_IMPLEMENTATION.md) ---
// Mỗi trang: 0 chưa có · 1 trang rách (mới có nửa câu) · 2 đang giữ · 3 đã dùng (đã xé).
// Lựa chọn dùng trang mang tag #page:<id>; Unity tô vàng. Trong đối chất, trang đang giữ dùng làm "Tiên tri".
VAR ts_tai_menh = 0       // "Trăm năm trong cõi người ta..." · Mở đầu
VAR ts_hieu = 0           // "Duyên hội ngộ, đức cù lao..." · Ch1, khi tỉnh táo ≥ 3
VAR ts_ma_mat = 0         // "Quá niên trạc ngoại tứ tuần..." · rách ở Ch1, đủ khi quan sát Mã Giám Sinh ở Ch2
VAR ts_co_ke = 0          // "Cò kè bớt một thêm hai..." · Ch2, quán nước
VAR ts_tu_ba = 0          // "Thoắt trông nàng đã biết tình..." · Ch3, gặp Tú Bà
VAR ts_hoa_troi = 0       // "Hoa trôi bèo dạt đã đành..." · Ch3, ký ức Kiều Thật (đồng điệu ≥ 4)
VAR ts_so_khanh = 0       // "Một chàng vừa trạc thanh xuân..." · rách ở Ch3 (cô Hồng), đủ ở Ch4 (chị Lam)
VAR ts_kim_trong = 0      // "Người quốc sắc, kẻ thiên tài..." · cuối Ch5
VAR ts_be_dau = 0         // "Trải qua một cuộc bể dâu..." · Ch6, khi có từ 2 nhân chứng
VAR ts_chu_tam = 0        // "Thiện căn ở tại lòng ta..." · Ch7, ký ức Kiều Thật (đồng điệu ≥ 7)
VAR ts_thanh_lau = 0      // "Thanh lâu hai lượt, thanh y hai lần" · Ch7, khi Định Mệnh ≥ 85 · không dùng được
VAR dinh_menh = 0         // 0–100: mức đã bẻ cong nguyên tác. Ngưỡng 30 · 50 · 70 · 85 làm câu chuyện phản đòn
VAR dong_dieu = 0         // 0–10: đồng điệu giữa cô học sinh và Kiều Thật
VAR lb_pl = 0             // điểm đối đáp lục bát 0–3 (Unity ghi): Mở đầu · Ch3 · Ch6 · Ch7
VAR lb_ch3 = 0
VAR lb_ch6 = 0
VAR lb_ch7 = 0
VAR c5_so_khanh = 0       // 0 không gặp · 1 vạch mặt · 2 đi theo, bị phục kích · 3 từ chối, bị hắn hô hoán

INCLUDE Chapter1_GiaBien.ink
INCLUDE Chapter2_TrenDuongTheoMaGiamSinh.ink
INCLUDE Chapter3_TuBa.ink
INCLUDE Chapter4_KhongConDuongLui.ink
INCLUDE Chapter5_DaoThoat.ink
INCLUDE Chapter6_KimTrong.ink
INCLUDE Prologue_TrangSach.ink
INCLUDE Chapter7_NghichMenh.ink
INCLUDE Epilogue_LopHoc.ink

-> chapter_1


// --- Hàm dùng chung ---

// Nhận trang đủ câu (trang đã xé thì không mọc lại).
=== function nhan(ref trang) ===
{trang < 2:
    ~ trang = 2
}

// Nhận nửa trang rách.
=== function nua(ref trang) ===
{trang == 0:
    ~ trang = 1
}

// Lật trang: xé nó và trả giá bằng Định Mệnh.
=== function dung(ref trang, gia) ===
~ trang = 3
~ fate(gia)

=== function fate(n) ===
~ dinh_menh = MAX(0, MIN(100, dinh_menh + n))

=== function soul(n) ===
~ dong_dieu = MAX(0, MIN(10, dong_dieu + n))
