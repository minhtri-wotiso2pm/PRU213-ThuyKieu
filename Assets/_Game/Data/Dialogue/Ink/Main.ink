// ============================================================
// THÚY KIỀU: NGHỊCH MỆNH
// Kịch bản chính — Chương 1 (Gia Biến), Chương 2 (Trên đường theo Mã Giám Sinh),
// Chương 3 (Tú Bà). Ink cũ Chương 2–4 lưu tại Legacy/ (không còn được include).
//
// Hợp đồng tag với Unity:
//   # speaker:<id>  # emotion:<id>  # camera:<id>  # scene:<id>  # music:<id>  # sfx:<id>
//   # chapter:<n>   # objective:<text>   # clue:<id>   # gameplay:<id> (chặn, chạy sau dòng)
//   # timer:<giây>  (đặt trên dòng ngay trước lựa chọn → lựa chọn có giới hạn thời gian)
//   Tag trên choice: #explore:<id> (chọn bằng tương tác trong thế giới), #timeout (mặc định khi hết giờ)
// Speaker: Narrator, Kieu, NoiTam (ý nghĩ nhân vật hiện đại), KieuThat (giọng Kiều thật), TrangSach (câu thơ ký ức),
//          MeKieu, MaGiamSinh, TuyTung, TuBa, MaKieu, CoHong, CoLam, NguoiCanh
// ============================================================

// --- Chương 1 ---
VAR tinh_tao = 0
VAR danh_tieng = 0
VAR tu_trong = 2
VAR tien = 0
VAR doc_ky_khe = false

// --- Chương 2 ---
VAR nghi_ngo = 0          // 0–100. Unity cộng theo hành vi, Ink cộng theo lời nói. 100 = bị lộ (checkpoint)
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

INCLUDE Chapter1_GiaBien.ink
INCLUDE Chapter2_TrenDuongTheoMaGiamSinh.ink
INCLUDE Chapter3_TuBa.ink

-> chapter_1


=== chapter_4 ===
# chapter:4
# scene:TuBa_Night
# speaker:Narrator
CHƯƠNG 4 — KHÔNG CÒN ĐƯỜNG LUI. (Đang được phát triển.)
-> END
