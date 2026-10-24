# Chương 3–4 · Lầu Tú Bà (góc nhìn thứ nhất) — Thiết kế đã chốt

Nguồn: `ThietKe_Chapter3-4_LauTuBa.docx` (bản thảo luận) + yêu cầu triển khai hoàn thiện ngày 2026-10-16.
Tài liệu này **thay** bản .docx ở những chỗ khác nhau; mục 8 ghi rõ cách xử lý 8 mâu thuẫn E1–E8.
Nhánh: `feature/chapter03-04-horror`.

## 1. Phạm vi và nguyên tắc

- Hai scene **mới**, dựng bằng code, dùng chung một ngôi lầu:
  - `Chapter03_LauTuBa`: chiều mưa tạnh, rồi sang chạng vạng.
  - `Chapter04_LauTuBaDem`: đêm mưa to, có trăng.
- Scene cũ `Chapter03_TuBaHouse` / `Chapter04_TuBaNight` và knot Ink cũ `chapter_3` / `chapter_4` **giữ nguyên**, vẫn chơi riêng được. Chuỗi chương chính đi qua scene mới: Ch2 → Ch3 mới → Ch4 mới → `Chapter05_Escape`.
- Góc nhìn thứ nhất, không chiến đấu. Nỗi sợ đến từ bị giam, bị nhìn, bị phát hiện. Không thêm ma hay lời nguyền.
  - Yếu tố mơ hồ duy nhất: giọng Kiều thật, và một thoáng bóng áo Kiều trong gương đồng (J1). Có thể hiểu là ký ức hoặc ảo giác.
- Mọi biến Ink mà Ch5–7 đọc đều được ghi với cùng nghĩa:
  - `c3_*`, `c4_van_tu`, `c4_tien`, `c4_ru_co_lam`, `c4_so_no`, `c4_bi_bat`, `c4_canh_giac`
  - `ev_khe_uoc`, `ev_giay_no`, `ev_tien`, `tien`
  - các trang Trang Sách `ts_tu_ba`, `ts_hoa_troi`, `ts_so_khanh`, `lb_ch3`, `tra_loi_kieu_that`, `dinh_menh`, `dong_dieu`

## 2. Mặt bằng (toạ độ Unity: x hướng đông, z hướng bắc, đơn vị mét)

Khuôn viên khoảng 28 × 26 m. Thêm một ngõ sau rộng 3 m dọc tường tây (x −17 → −14).

| Khu | Toạ độ | Tính chất |
|---|---|---|
| Sân mưa | x −6.5→6.5, z −6→4.5 | Đá lát ướt, cây đại, bể non bộ, 4 bậc ở giữa mỗi cạnh |
| Hiên tây | x −8.7→−6.5, z −8.2→4.7 | Có mái, cột son, nối với hành lang phục vụ ở góc tây bắc |
| Hiên đông | x 6.5→8.7, z −8.2→4.7 | Đối xứng với hiên tây |
| Hiên nam | x −8.7→8.7, z −8.2→−6 | Là thềm trước sảnh |
| Vách song bắc | z 4.5, x −6.5→6.5 | Nhìn xuyên được. Một khe giữa rộng 1.4 m (x −0.7→0.7), thẳng trục với cửa phòng Tú Bà |
| Hành lang phục vụ | z 4.7→7.0, x −14→14 | Mái thấp, có chum nước, không treo đèn lồng (khu cấm) |
| Bếp | x −14→−7.6, z 7→13 | Nền đất, mộc mạc, bếp lò, cửa sau ở tường tây |
| Kho vải | x −7.6→−3.6, z 7→13 | Chật, kệ vải. Vách song nhìn sang phòng Tú Bà |
| Phòng Tú Bà | x −3.6→4.4, z 7→13 | Sang trọng: bình phong, giường có màn, ghế bành, rương bạc |
| Phòng sổ sách | x 4.4→14, z 7→13 | Gian ngoài có bàn sổ. Gian trong (x 10→14) sau tấm bình phong có tủ khoá giao kèo và gương đồng |
| Phòng Kiều | x −14→−8.7, z −0.4→4.7 | Nhỏ, có hốc giường. Cửa sổ song sắt nhìn ra ngõ sau |
| Phòng chị Lam | x −14→−8.7, z −6→−0.4 | Chật, có dấu vá cửa |
| Góc chum | x −14→−8.7, z −8.2→−6 | Hốc mở ra hiên tây, chum nước to, chỗ nấp |
| Phòng Hồng | x 8.7→14, z −0.4→4.7 | |
| Phòng khách | x 8.7→14, z −8.2→−0.4 | Rộng; có khách đang uống, tiếng đàn |
| Sảnh tiếp khách | x −14→14, z −13→−8.2 | Lộng lẫy, mặt bắc mở ra hiên nam qua hàng cột và bình phong. Cổng chính ở giữa tường nam |

## 3. Cửa và trạng thái

| Cửa | Vị trí | Ch3 | Ch4 |
|---|---|---|---|
| 1 Cổng chính | Tường nam, x −1.2→1.2 | Mở lúc đến, rồi cài then (có tiếng) | Then ngang và xích. Gác cổng ngồi trước. **Không bao giờ mở** |
| 2 Phòng Kiều | Tường hiên tây, z 2.6→3.6 | Mở ban ngày. Cuối chương bị cài then ngoài | Then ngoài. Mở bằng cách luồn trâm qua khe để nâng then |
| 3 Phòng Lam | Tường hiên tây, z −2.6→−1.6 | Mở | Khoá móc ngoài. Mở bằng chìa đồng nhỏ trong chùm chìa |
| 4 Phòng Hồng | Tường hiên đông, z 2.6→3.6 | Mở | Cài then. Chỉ có tiếng nói qua cửa |
| 5 Phòng khách | Tường hiên đông, z −4→−2.8 | Khép | Khép, có khách bên trong. Không vào được, nhưng có tiếng nhạc |
| 6 Bếp → hành lang | z 7, x −10.6→−9.6 | Rèm | Rèm |
| 7 Cửa sau bếp | x −14, z 9.4→10.4 | Khoá móc và then (chỉ để nhìn thấy) | Đích đến: mở khoá bằng chìa, rồi nâng then |
| 8 Bếp ↔ kho vải | x −7.6, z 9.5→10.5 | Rèm | Rèm |
| 9 Kho vải → hành lang | z 7, x −5.9→−4.9 | Hé mở | Hé mở |
| A Phòng Tú Bà → hành lang | z 7, x −0.7→0.7 | Cửa lùa giấy, khép, không khoá | Cửa lùa: kéo chậm thì êm, kéo nhanh thì kêu |
| B Phòng Tú Bà ↔ sổ sách | x 4.4, z 11→12 | **Then nằm phía phòng Tú Bà, không cài.** Mở được từ phía sổ sách → lối thoát bắt buộc của Ch3 | Cài từ phía phòng Tú Bà. Chỉ mở được khi đứng trong phòng Tú Bà (kéo then chậm). Phía sổ sách không mở được |
| C Sổ sách → hành lang | z 7, x 7→8 | Hé lúc chạng vạng, rồi bị Tú Bà khoá từ ngoài | Khoá. Mở bằng chìa. Từ bên trong có thể khoá lại |

**Lối cụt có chủ đích:** đầu tây hành lang phục vụ (chỉ dẫn vào bếp), phòng khách, sảnh, cổng chính, góc chum.

## 4. Luồng Chương 3

1. **Đến lầu (Ink, góc nhìn thứ nhất):**
   - Tú Bà niềm nở, trao trang `ts_tu_ba`.
   - Mã Giám Sinh đưa tờ giấy gấp tư.
   - Cổng cài then sau lưng (có tiếng).
2. **Khám phá tự do.** Hub Ink gồm các mục:
   - **Phòng mình** (`c3_window`): vén rèm thấy song sắt, áo mới có thẻ ghi nợ, cửa không có then trong.
   - **Hồng** (`c3_girl_hong`): dạy luật chơi (đá ướt thì vang, người canh uống rượu), nửa trang `ts_so_khanh`.
   - **Lam** (`c3_girl_lam`): giữ màn đối đáp lục bát `lucbat_ch3`, trang `ts_hoa_troi`.
   - **Mã Kiều ở bếp** (`c3_makieu_bep`): nói chuyện nợ, chỉ cho thấy cửa sau, cho biết chìa ở thắt lưng Tú Bà.
   - **Cổng** (`c3_gate`): Tú Bà chặn lại và đọc bảng kê nợ.
3. **Mở phòng sổ sách:** sau ít nhất 2 việc, trời chạng vạng, cửa C hé.
   - Vào sổ sách là vào khu cấm. Bị người canh thấy thì bị dẫn về hiên và về checkpoint (Ch3 phạt nhẹ, không ai đuổi).
   - Văn tự 450 lạng nằm trong hộp sơn son (`c3_khe_uoc`). Sổ nợ ở ngăn bàn (`c3_so_no`). Giao kèo nằm sau song tủ khoá (`c3_cabinet`).
4. **Trốn và nghe lén (`c3_office`):**
   - Có tiếng Tú Bà và Mã Giám Sinh đi qua cửa B. Người chơi có 6 giây để vào tủ áo hoặc nấp sau bình phong gian trong.
   - Tú Bà chỉ bắt khi **thật sự nhìn thấy** Kiều. Bà nhìn theo luật nhìn, có kiểm tra vật cản. Nếu bà thấy Kiều chui vào chỗ nấp thì bà biết chỗ đó. Bị thấy → checkpoint ở cửa sổ sách. Đây là thất bại mềm, có báo trước bằng tiếng bước và ánh đèn.
   - Kiều chưa nấp mà cũng chưa bị thấy (ví dụ đã lẻn ra ngoài): hai người đứng bên bàn sổ chờ. Cuộc nói chuyện chỉ bắt đầu khi Kiều nấp trong phòng sổ sách. Không có đồng hồ ép thời gian.
   - Trong tủ, nhìn qua khe thấy hai người chia bạc, nghe chuyện giả cưới (`c3_nghe_len`, `c3_biet_chia_khoa`).
5. **Bị khoá trong phòng:**
   - Hai người ra bằng cửa C, Tú Bà khoá C từ ngoài.
   - Người chơi ra qua cửa B (đã chắc chắn không cài) sang phòng Tú Bà lúc đó vắng.
   - Bóng người canh in lên giấy cửa A. Phải đợi hắn qua rồi mới ra được.
   - Checkpoint khi bước vào phòng Tú Bà.
6. **Về phòng (`c3_end`):**
   - Cửa phòng sập, then ngoài kéo lại.
   - Mã Kiều thì thầm qua song cửa: "Hắn đã bán cô…".
   - Hoảng loạn, rồi Kiều thật hỏi (lựa chọn `tra_loi_kieu_that`).
   - Mã Kiều báo: đêm nay có khách lớn, người canh uống rượu.
   - Chuyển sang Ch4.

## 5. Luồng Chương 4

Biến tiến trình (Ink): `c4_buoc`

| Giá trị | Nghĩa |
|---|---|
| 0 | Còn trong phòng |
| 1 | Đã ra khỏi phòng |
| 2 | Đã có chìa |
| 3 | Đã có giao kèo, J1 chưa xong |
| 4 | J1 xong |
| 5 | J2 xong |

1. **Mở đầu (Ink):**
   - Tú Bà đứng sau song cửa, không vào phòng. Lựa chọn có giờ `c4_canh_giac`, giữ nguyên trang `ts_tu_ba`.
   - "Mai con tiếp khách đầu tiên." Rồi cài then. Lúc này chìa còn trên người Tú Bà, nên tiếng chìa lanh canh là hợp lý.
   - Nếu `c4_canh_giac` = 2: không có cảnh tát. Người canh thêm một điểm dừng trước cửa Kiều, và nhạy hơn.
2. **Ra khỏi phòng:**
   - Lấy trâm trên bàn phấn, giữ E ở khe cửa để nâng then.
   - Nâng chậm: tiếng nhỏ. Nhả hoặc giật: tiếng cạch bán kính 6 m.
   - CP1 khi đã nâng được then.
3. **Hero shot và quan sát:** hiên tây nhìn ra sân mưa, đèn lồng, bóng khách lướt sau rèm phòng khách. Người chơi học tuyến tuần tra qua ánh đèn lồng của người canh.
4. **Kho vải:**
   - Nhìn qua vách song thấy Tú Bà ngủ gật trên ghế bành, chùm chìa ở thắt lưng (`c4_quan_sat`).
   - CP2 khi bước vào kho vải.
5. **Lấy chìa:**
   - Kéo cửa A chậm, khom người.
   - Giữ E khi Tú Bà thở ra, thả khi bà hít vào.
   - Ngáy ngừng = cựa mình → đứng im 3 giây. Gây thêm tiếng khi bà đang cựa mình → bà tỉnh → thất bại về CP2.
   - Lấy được chìa (`c4_chia_khoa`) → CP3, hồi sinh tại kho vải.
6. **Sổ sách:**
   - Vào bằng cửa B (kéo then sát chỗ Tú Bà ngủ) hoặc cửa C (vặn chìa ngoài hành lang, tiếng 5 m).
   - Mở tủ, đọc giao kèo, chọn văn tự và bạc (Ink `c4_cabinet`).
   - Xé sổ nợ (`c4_ledger`) là tuỳ chọn.
7. **J1** (mục 6). J1 kết thúc → CP4.
8. **Đường về và J2** (mục 6). J2 kết thúc → CP5.
9. **Chị Lam (tuỳ chọn):**
   - Mở khoá móc bằng chìa nhỏ → Ink `c4_colam`: rủ đi / hứa quay lại / im lặng, có trang `ts_hoa_troi`, `ts_so_khanh`.
   - Lam di chuyển theo luật ở mục 7.
10. **Bếp:**
    - Mã Kiều nằm cạnh bếp, mở mắt, quay mặt vào vách.
    - Cửa sau: thiếu chìa thì nhãn ghi "Khoá móc — cần chìa". Có chìa nhưng chưa có giao kèo thì nhãn ghi "Chưa đi được — giao kèo còn trong tủ" kèm một câu nội tâm (E8).
    - Đủ cả hai → mở khoá, nâng then → Ink `c4_leave` → Ch5.

## 6. Hai jumpscare và trạng thái của Tú Bà (E1, E6)

Chuỗi trạng thái bắt buộc: lấy giao kèo → kết thúc thoại tủ → **J1** → vượt qua tình huống người canh → J1 kết thúc → **ít nhất 60 giây nghỉ** → **J2** chỉ được bắt đầu khi đủ cả các điều kiện sau:

- J1 đã kết thúc.
- Đã đủ 60 giây kể từ khi J1 kết thúc.
- Người chơi không đang trốn.
- Không ai đang truy đuổi người chơi.
- Người chơi ở trong hành lang phục vụ hoặc nửa bắc của sân.
- Người chơi cách cửa A từ 3.5 đến 16 m, và không đứng trong phòng Tú Bà.

**Không có bộ đếm ép J2 xảy ra.** Mọi đường từ sổ sách về bếp đều đi qua hành lang phục vụ, nên J2 luôn xảy ra trước khi người chơi đến cửa sau.

**Tú Bà có ba trạng thái rõ ràng:**

| Trạng thái | Biểu hiện |
|---|---|
| Ngủ | Ngáy đều |
| Cựa mình | Ngáy ngừng 3–4 giây, lẩm bẩm, rồi ngáy lại. Không hề thức dậy |
| Thức | Chỉ xảy ra ở J2 |

- Trong J1, tiếng người canh lắc cửa chỉ làm bà **cựa mình**.
- Sau J2 bà **thức hẳn** và không ngủ lại.
- Checkpoint lưu đúng trạng thái này.

**J1 – Gương đồng:**

1. **Báo trước:** ngọn đèn nghiêng, khăn phủ gương tụt dần, tiếng mưa nhỏ lại.
2. **Hù** (chỉ khi gương nằm trong tầm nhìn, chờ tối đa 10 giây): khăn rơi, trong gương thoáng hiện bóng áo Kiều 0,6 giây, kèm tiếng dây đàn đứt. Kiều thật thì thầm "Có người."
3. **Mối nguy thật:** người canh rời tuyến tuần tra, có tiếng bước, đèn hắt dưới khe cửa C, rồi lắc cửa.
   - Cửa C khoá và đèn tắt → hắn đi.
   - Cửa C khoá và đèn sáng → hắn lẩm bẩm "Bà lại quên tắt đèn" rồi đi.
   - Cửa C không khoá → hắn vào tìm 6 giây theo luật nhìn bình thường.
4. **Kết thúc** khi hắn quay lại tuyến và cách cửa C hơn 6 m.

**J2 – Cửa lùa:**

1. **Báo trước:** gác cổng gọi vọng "Bà ơi, khách quan về!" → tiếng ngáy ngừng hẳn → ghế kẽo kẹt, giọng lẩm bẩm "Chìa… chìa đâu?" → đèn sáng sau giấy cửa, bóng người lướt qua.
2. **Hù:** cửa lùa đập mạnh, Tú Bà giơ đèn, gào gọi người canh.
3. **Khoảng phản ứng:** Tú Bà khựng lại 1,5 giây, tầm nhìn 5 m quanh ngọn đèn.
4. **Hậu quả:**
   - Bà đi tới phòng Kiều, đứng chắn hiên tây khoảng 25 giây, rồi xuống sảnh.
   - Người canh tìm quanh cánh tây khoảng 40 giây.
   - Thắp thêm 2 đèn ở hành lang.
   - Tuyến kho vải → bếp trở thành tuyến an toàn hơn.

**Chơi lại:**

- Phần hù chỉ phát **một lần mỗi lượt chơi**. Cờ "đã xem" tách khỏi tiến trình và không mất khi checkpoint.
- Sau khi bị bắt, mọi tín hiệu báo trước vẫn chạy đủ. Không rút ngắn thời gian.
- J2 không đập cửa lại. Tú Bà đã đứng ở trạng thái sau J2 và vẫn gọi tiếng.

## 7. Chị Lam (E4)

- Sau thoại, nếu `c4_ru_co_lam`: Lam đi theo **chuỗi điểm chờ**: phòng Lam → góc chum → góc tây bắc → bếp (nấp sau lò).
- Lam chỉ đi sang điểm kế tiếp khi đủ cả ba điều kiện:
  - Người chơi đã tới điểm đó (người chơi dẫn đường).
  - Không người canh nào đang truy đuổi hoặc tìm kiếm trong vòng 10 m quanh đoạn đường.
  - Không người canh nào có tầm nhìn thẳng tới đoạn đường.
- Khi chưa an toàn, Lam ngồi nấp (không bị nhìn thấy) và thì thầm "Chưa được, có người".
- **Bị kẹt:** một đoạn đi quá 15 giây, hoặc NavMesh không tìm được đường → dịch Lam tới điểm kế tiếp khi camera không nhìn thấy cô.
- **Bị thấy giữa đường:** Lam dừng, lùi về điểm trước và nấp. Người canh chỉ nghi ngờ và đi kiểm tra, không bắt Lam.
- **Người chơi bị bắt:** checkpoint khôi phục điểm chờ của Lam.
- **Đảm bảo không kẹt:** khi người chơi đã ở bếp mà Lam chờ quá 40 giây, Lam được dịch tới bếp lúc không ai nhìn.
- Có Lam ở bếp thì nâng then cửa sau nhanh và êm.

## 8. Cách xử lý mâu thuẫn E1–E8

| Mã | Đã chốt |
|---|---|
| E1 | Chuỗi J1 → khoảng nghỉ ≥ 60 giây → J2 có điều kiện (mục 6). Bỏ việc ép J2 sau 25 giây |
| E2 | Người canh chỉ biết chỗ nấp nếu **thấy tận mắt** lúc người chơi chui vào (có tầm nhìn trực tiếp trong khung hình đó). Nếu đã mất dấu: tìm quanh vị trí cuối cùng nhìn thấy. Chỉ kiểm tra chỗ nấp đã thấy người chui vào: đi tới, lẩm bẩm, giật cửa tủ trong 1,5 giây. Bắt người phải có tầm nhìn, cách dưới 1,3 m và đường đi NavMesh dưới 2 m. Không bắt xuyên tường |
| E3 | Cửa B: then phía phòng Tú Bà. Ch3 không cài (lối thoát bắt buộc). Ch4 cài, chỉ mở được từ phía phòng Tú Bà. Checkpoint Ch3 khi vào phòng Tú Bà |
| E4 | Lam đi theo điểm chờ có điều kiện an toàn, có xử lý khi kẹt hoặc bị thấy (mục 7) |
| E5 | Ảnh chụp trạng thái (người chơi, vật phẩm, cửa, khoá, đèn, Tú Bà, người canh, Lam, nhiệm vụ) được lưu **cùng lúc** với checkpoint Ink. Điểm hồi sinh cố định, an toàn theo từng bước; người canh được đặt lại ở điểm tuần tra xa. Cờ "đã xem" tách khỏi tiến trình. Bỏ chi tiết "đến sớm hơn 2 giây" |
| E6 | Ba trạng thái ngủ / cựa mình / thức (mục 6). Sau khi Kiều cầm chìa không còn dùng tiếng chìa của Tú Bà; thay bằng tiếng ngáy, ghế kêu, vòng tay và giọng gọi |
| E7 | Công thức nhận biết ghi ở mục 9. Các hệ số là giá trị khởi đầu để cân chỉnh |
| E8 | Khoá vật lý (cần chìa) tách khỏi điều kiện nhiệm vụ (cần giao kèo). Cả hai được báo bằng nhãn tương tác và một câu nội tâm ngắn |

## 9. Quy tắc phát hiện (giá trị khởi đầu)

**Nhận biết bằng mắt** (`SightModel`), tăng mỗi giây theo công thức:

`tốc độ = (1 / T0) × hệ_số_khoảng_cách × kẹp(tư_thế × ánh_sáng × chuyển_động × tập_trung, 0.25, 1.6)`

| Thành phần | Giá trị |
|---|---|
| `T0` | 1.0 s |
| Hệ số khoảng cách | 1 khi ≤ 2 m, giảm tuyến tính về 0.35 ở tầm xa nhất |
| Tư thế | Đứng 1 · cúi 0.55 |
| Ánh sáng | 0.5 → 1.25 theo `LightExposure` |
| Chuyển động | Đứng yên 0.6 · đi 1 · chạy 1.4 |
| Tập trung | Nón giữa 1 · rìa mắt 0.5 (chỉ trong 4 m) |
| Kẹp dưới 0.25 | Ghép mọi hệ số giảm vẫn không thành tàng hình |
| Sàn khi đứng sát | Trong 2.2 m và trong nón giữa: hệ số ít nhất 0.9 → bị nhận ra dưới khoảng 1.1 s |

- Tầm nhìn: người canh 9 m; khi người chơi ở chỗ tối, tầm bị thu còn 5 m.
- Ánh sáng chỉ đổi tốc độ và tầm nhìn. **Luôn kiểm tra vật cản** (bắn tia từ mắt tới đầu và thân người chơi).
- Ngưỡng: 0.3 nghi ngờ, 0.65 kiểm tra, 1.0 truy đuổi. Mất tầm nhìn thì giảm 0.2 mỗi giây.

**Tiếng động** (bán kính, mét):

| Hành động | Bán kính |
|---|---|
| Cúi đi | 1.5 |
| Đi | 4 |
| Chạy | 9 |
| Đá ướt | ×1.5 |
| Mở cửa chậm | 1.5 |
| Giật cửa | 8 |
| Vặn chìa | 5 |
| Va đồ | 6 |
| Bạc (khi chạy) | +3 |

- Có tường chắn giữa nguồn và người nghe: bán kính ×0.6.
- Mưa ở sân ×0.8. Trong vòng 6 m quanh phòng khách ×0.6.
- Tiếng động chỉ đẩy người canh lên tối đa Kiểm tra, không bao giờ thẳng sang Truy đuổi.

**Người canh:**

- Tuần tra 1.1 m/s. Kiểm tra 1.7 m/s. Truy đuổi 3.3 m/s (người chơi chạy 3.8 m/s).
- Tìm kiếm 20 giây, qua 3 điểm.
- Sau đó cảnh giác 45 giây, tốc độ nhận biết ×1.3.

## 10. Kiểm thử bắt buộc

- **EditMode:**
  - Công thức nhận biết: kẹp dưới, sàn khi đứng sát, ánh sáng không bỏ qua vật cản.
  - Điều kiện J2.
  - Luật nhận biết chỗ nấp.
  - Điều kiện đi tiếp của Lam.
  - Khôi phục ảnh chụp checkpoint.
- **Autopilot:** chạy Ch3 → Ch4 → Ch5 theo nhiều nhánh: có/không đón Lam, cửa B hay cửa C, các lựa chọn văn tự và bạc, ép thất bại ở từng checkpoint.
- **Hiệu năng:** đo bằng `StoryPerfProbe` ở 1080p trên máy dev (i5-12500H, RTX 3050 Laptop), mục tiêu 60 fps.
- **Bản build Windows:** chạy autopilot nối chương.
