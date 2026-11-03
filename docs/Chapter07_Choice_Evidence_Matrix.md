# Chương 7 — Ma trận lựa chọn & bằng chứng

Tài liệu kiểm toán trạng thái Chương 7 **trước** đợt nâng cấp cơ chế (03/11/2026).
Mọi số liệu dưới đây đọc trực tiếp từ mã nguồn, không phải từ thiết kế mong muốn.

Nguồn:

- `Assets/_Game/Data/Dialogue/Ink/Chapter7_NghichMenh.ink` (18 knot)
- `Assets/_Game/Data/Dialogue/Ink/Main.ink` (khai báo biến toàn cục)
- `Assets/_Game/Scripts/Gameplay/Sequences/TrialSequence.cs` (luật đối chất)
- `Assets/_Game/Scripts/UI/TrialView.cs`, `StoryUIBuilder.cs` (HUD)
- `Assets/_Game/Scripts/Core/Editor/Chapter07Setup.cs` (wire scene)

## 1. Cổng huyện đường — `c7_cong`

| Lựa chọn | Điều kiện hiện | Biến ghi | Tác dụng |
|---|---|---|---|
| Động viên các nhân chứng | `ev_nhan_chung > 0 && !c7_nhan_chung_vung` | `c7_nhan_chung_vung = true`, `c7_viec += 1` | Nhân chứng không lùi khi Tú Bà dọa (bỏ nhánh `c7_lui`) |
| Bàn kế với Kim Trọng | `kt_tin_tuong >= 3 && !c7_ke_hoach_kt` | `c7_ke_hoach_kt = true`, `c7_viec += 1` | Một lần Kim Trọng cứu nguy khi lòng tin cạn |
| Để ý Tú Bà | `!c7_biet_hoi_lo` | `c7_biet_hoi_lo = true`, `c7_viec += 1` | Mở thẻ bằng chứng `hoi_lo` |
| Đánh trống kêu oan | luôn có | — | Vào công đường, không quay lại |

Nếu bỏ qua “Động viên”, sau vòng 2 Ink cho một nhân chứng lùi bước và ghi `c7_lui`
(1 = chị Lam, 2 = bà hàng nước, 3 = thư lại). `c7_lui` cũng là điều kiện để thẻ nhân chứng
tương ứng biến mất khỏi hồ sơ ở các vòng sau.

## 2. Ba vòng đối chất

Mỗi vòng bốn lời khai. Chỉ lời khai `Key` kết thúc vòng; các câu khác chỉ cộng điểm.
`Revision` là lời khai đổi sau khi hỏi vặn; `NeedsRevision` nghĩa là bằng chứng chỉ dùng được
sau khi đã hỏi vặn.

### Vòng 1 — Tú Bà (trộm bạc)

| # | Lời khai | Key | Revision | Bằng chứng bẻ được (điểm) |
|---|---|---|---|---|
| 1 | “người tôi bỏ bạc ra mua về, giấy tờ đàng hoàng” | | không | `khe_hon` (1), `van_tu` (1) |
| 2 | “đêm mười sáu trộm **năm trăm** lạng” | | có → **bốn trăm năm mươi** | `tui_bac` (1), `so_bac` (1), `kim_trong` (1) |
| 3 | “bạc ấy là tiền chợ, làm ăn lương thiện” | | không | `so_bac` (1), `hoi_lo` (1), `tui_bac` (1) |
| 4 | “Mã chỉ là người mai mối giúp” | **có** | không | `giao_keo` (1), `mau_giay` (1), `ba_nuoc` (1) |

### Vòng 2 — Mã Giám Sinh

| # | Lời khai | Key | Revision | Bằng chứng bẻ được (điểm) |
|---|---|---|---|---|
| 1 | “ta là giám sinh, danh phận đàng hoàng” | | không | `thu_lai` (1) |
| 2 | “sính lễ bốn trăm lạng, cả làng đều biết” | | không | `so_bac` (1), `van_tu` (1) |
| 3 | “trước ngày cưới chưa từng gặp Tú Bà” | **có** | không | `giao_keo` (1) |
| 4 | “Kiều là vợ ta, chồng đưa vợ đi đâu là quyền” | | không | `thu_lai` (1), `van_tu` (1) |

### Vòng 3 — Tú Bà (vòng nợ)

| # | Lời khai | Key | Revision | Bằng chứng bẻ được (điểm) |
|---|---|---|---|---|
| 1 | “nó nợ tôi bốn trăm năm mươi lạng tiền mua” | **có** | không | `so_no` (2), `giao_keo` (1), `van_tu` (1) |
| 2 | “các cô ở nhà tôi đều tự nguyện” | | không | `co_lam` (1) |
| 3 | “xưa nay chưa ai thấy tôi ép buộc ai” | | không | `thu_lai` (1), `ba_nuoc` (1), `co_lam` (1) |
| 4 | “con Kiều trốn đi là bỏ nợ, là ăn cắp” | | không | **không có** — câu này không thể bẻ |

## 3. Bằng chứng trong cặp hồ sơ

`TrialSequence.Available()` dựng lại danh sách ở đầu **mỗi vòng**, theo cờ đã ghi từ Chương 1–7.

| Id | Thẻ | Điều kiện xuất hiện | Ghi chú |
|---|---|---|---|
| `khe_hon` | Tờ khế hôn ước | luôn có | 400 lạng sính lễ |
| `giao_keo` | Giao kèo Mã – Tú Bà | luôn có | 450 lạng/người, lập tháng Giêng |
| `van_tu` | Văn tự bán thân | `c4_van_tu == 1` | người bán: Mã; người mua: Tú Bà |
| `so_no` | Trang sổ nợ | `ev_giay_no` | nợ 450 lạng + áo lụa 30 + son phấn 10 |
| `tui_bac` | Túi bạc dấu “T.” | `c4_tien == 2` | 450 lạng chưa tiêu |
| `so_bac` | Sổ hiệu bạc Phúc Ký | `c6_so_bac` | 14/4: Tú Bà giao 450 lạng cho “Mã công tử” |
| `mau_giay` | Mẩu giấy trên đường | `giu_mau_giay` | “Bà T. ở Lâm Tri nhận hàng — 450 lạng” |
| `hoi_lo` | Túi gấm đổi tay | `c7_biet_hoi_lo` | đút lót nha dịch |
| `ba_nuoc` | Nhân chứng: bà hàng nước | `nc_ba_nuoc && c7_lui != 2` | nghe “giao hàng” |
| `thu_lai` | Nhân chứng: thư lại | `nc_thu_lai && c7_lui != 3` | sổ hôn thú, bằng giả |
| `co_lam` | Nhân chứng: chị Lam | `nc_co_lam && c7_lui != 1` | ba năm trả nợ không hết |
| `kim_trong` | Lời Kim Trọng | `kt_tin_tuong >= 3` | gặp Kiều ở cổng chùa |

Tối đa 12 thẻ. Lưới hồ sơ xếp 2 cột, cao 94 px/thẻ — 12 thẻ đã kín 6 hàng.

## 4. Điểm và ngưỡng (giữ nguyên trong đợt nâng cấp)

- Lòng tin `_trust`: bắt đầu 0,6 (vòng 1) hoặc `max(0,4, c7_long_tin/100)` (vòng 2–3);
  Định Mệnh ≥ 85 mà chưa thấy đút lót thì −0,15.
- Bẻ đúng: `+0,15 × điểm` lòng tin; bẻ sai: `−0,2`; hết lòng tin thì thua vòng.
- Điểm vòng = tổng điểm các câu đã bẻ; thua vòng thì 0.
- `c7_diem_pq = c7_long_tin + 15 (Kim Trọng) + 10 (nhân chứng vững) + 0/10/20 (lục bát) − 15 × c7_thua_vong`.
- Kết: `≥ 90 && c6_ho_so ≥ 3` → 3; `≥ 60` → 2; `≥ 35` → 1; còn lại → 0.

## 5. Khoảng trống mà đợt nâng cấp phải lấp

1. **Không có giá cho thời gian của quan.** Hỏi vặn một câu không có `Revision` chẳng mất gì,
   nên người chơi có thể bấm Q liên tục. Lòng tin chỉ phạt khi nộp sai bằng chứng.
2. **Nhân chứng không có can đảm.** Sau khi được chiêu mộ, một nhân chứng có thể ra làm chứng
   ở cả ba vòng mà không mất gì; không có khái niệm “đã dùng hết lời của người ta”.
3. **Thiếu bằng chứng phái sinh.** Hai thẻ `so_bac` và `giao_keo` cùng nói về 450 lạng nhưng
   không bao giờ ghép lại thành một thẻ mới. Người chơi đã nộp cả hai vẫn phải bẻ vòng nợ
   bằng thẻ cũ.

Ba khoảng trống này là đầu vào của ba cơ chế: kiên nhẫn quan huyện, can đảm nhân chứng,
và thẻ phái sinh “vòng nợ 450”.
