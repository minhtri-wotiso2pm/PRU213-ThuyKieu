# Chương 7 — Vì sao thiết kế như hiện tại

Chương 7 là chương duy nhất trong game mà Kiều không đi tìm manh mối nữa: nàng đã có giấy tờ trong tay và phải tự nói trước quan. Đó cũng là chương dễ hỏng nhất, vì "đối chất" rất dễ biến thành bảng trắc nghiệm: chọn đúng thì qua, chọn sai thì thử lại. Ba cơ chế của đợt nâng cấp này sinh ra để chặn đúng kiểu chơi đó.

Tài liệu này ghi lại cái đọc ra từ nguyên tác, cơ chế tương ứng, và những chỗ phải chọn giữa "đúng với truyện" và "chơi được".

## 1. Nguyên tác và cơ chế

| Điều đọc ra từ *Truyện Kiều* | Cơ chế trong game | Số liệu |
|---|---|---|
| Kiều hầu như không được tự kể mình. Nàng bị mua, bị gả, bị chuộc, bị xử — lời về nàng luôn là lời của người khác: Mã Giám Sinh, Tú Bà, quan huyện, người qua đường. | Công đường là chỗ duy nhất nàng được nói. Mỗi vòng, người chơi chọn một lời khai để bẻ, và chỉ bẻ được bằng giấy tờ hoặc bằng người đã chứng kiến. | 3 vòng × 4 lời khai; bẻ đúng `+0,15 × điểm` lòng tin, bẻ sai `−0,2` |
| Bạc là đơn vị của số phận: 400 lạng sính lễ, 450 lạng tiền mua, món nợ không bao giờ trả hết. Cùng một số bạc đi qua nhiều bàn tay và mỗi lần lại sinh ra một nghĩa vụ mới. | Hai tờ giấy trong hồ sơ — giao kèo Mã–Tú Bà và sổ hiệu bạc Phúc Ký — ghép lại thành một thẻ mới: **Vòng nợ 450**. Thẻ này chỉ tồn tại nếu người chơi đã thực sự nộp cả hai tờ trước đó. | nộp cả `so_bac` và `giao_keo` → mở `vong_no`; bẻ câu then chốt vòng 3 được **2 điểm**, bẻ lời khai 2 vòng 2 được **1 điểm** |
| Nhân chứng trong truyện ai cũng sợ. Bà hàng nước nghe được chuyện "giao hàng" nhưng chỉ dám nói sau vách quán; chị Lam ba năm trả nợ không hết; thư lại giữ sổ hôn thú mà làm ngơ. Họ nói vì thương, không phải vì nghĩa vụ. | **Can đảm nhân chứng** 0–3. Mỗi lần một thẻ nhân chứng bẻ đúng lời khai là một lần người ấy phải đứng ra: `−1`. Cuối vòng, ai không bị gọi thì hồi `+1` (tối đa 3). Về 0, người ấy rời phòng và thẻ biến khỏi hồ sơ các vòng sau. | khởi tạo **2** nếu chiêu mộ được ở Chương 6, **0** nếu không; "Động viên" ở cổng `+1` cho tất cả; `c7_lui` = 1 chị Lam, 2 bà hàng nước, 3 thư lại |
| Quan huyện xử theo giấy tờ và theo lệ, không theo lẽ phải. Thời gian của quan là thứ người nghèo không có: kiện cáo kéo dài chính là một cách thua. | **Kiên nhẫn của quan** 100 mỗi vòng. Nộp sai `−25`; hỏi vặn một câu không còn gì để nói `−10`. Về 0, quan cắt phiên ngay, không gợi ý, không cho làm lại. | `c7_kien_nhan = 100` đầu mỗi vòng; 0 → `c7_thua_vong += 1`, vòng tính 0 điểm |
| Cái đau của truyện không nằm ở việc không biết, mà ở việc biết mà vẫn không làm gì được. | Ba cơ chế trên đều là tài nguyên có hạn, nên người chơi không thể "thử hết rồi làm lại": mỗi lượt bấm đều có giá, và cái giá rơi vào đúng thứ người nghèo thiếu — thời gian, can đảm, giấy tờ. | — |

## 2. Nguyên tắc chung: mỗi hành động phải có giá

Ba cơ chế nhìn có vẻ rời nhau, nhưng cùng trả lời một câu hỏi: *trước đây người chơi bấm gì cũng miễn phí, vậy cái gì khiến một lượt bấm trở nên nặng?*

- Trước đợt nâng cấp, hỏi vặn một câu không có `Revision` chẳng mất gì. Người chơi có thể bấm `Q` liên tục cho tới khi lời khai tự sửa. Kiên nhẫn biến thao tác đó thành một lần tiêu thời gian của quan.
- Trước đợt nâng cấp, một nhân chứng ra làm chứng ở cả ba vòng mà không mất gì. Can đảm biến lời của người ta thành thứ dùng một lần rồi cạn, và buộc người chơi chọn *vòng nào đáng dùng ai*.
- Trước đợt nâng cấp, hai tờ giấy nói cùng một con số 450 nhưng không bao giờ gặp nhau. Thẻ phái sinh thưởng cho người chơi đã chịu khó ghép chứng cứ: công sức ở vòng 1–2 mở ra một cách bẻ vòng 3.

Điểm chung: **không có phần thưởng nào cho việc đúng mà không có bằng chứng**. Công thức điểm và ba ngưỡng 90 / 60 / 35 giữ nguyên, nên ba cơ chế mới chỉ đổi *đường đi*, không đổi *đích*.

## 3. Hai quyết định phải chọn

**Kiên nhẫn reset về 100 ở mỗi vòng, không tích luỹ cả phiên.** Nếu giữ một thanh 100 cho cả ba vòng thì một lần nộp sai ở vòng 1 sẽ làm vòng 3 không thể chơi tốt — người chơi mất quyền cứu vãn vì một lỗi đã qua, và đó là death spiral chứ không phải áp lực. Reset theo vòng giữ được ý "quan chỉ rộng lượng trong một phiên xét", đồng thời để mỗi vòng là một lần phán xét mới, đúng như cách quan huyện nghe từng người một.

**Can đảm nhân chứng được khởi tạo ở cổng Chương 7, không sửa Chương 1–6.** Đúng ra con số này thuộc về Chương 6 — nơi người chơi chiêu mộ nhân chứng — nhưng đợt nâng cấp chỉ được đụng vào Chương 7. Vì vậy `nc_tin_*` được đặt ngay đầu `chapter_7` trong Ink: 2 nếu cờ chiêu mộ `nc_*` đã bật, 0 nếu không. Cách này giữ nguyên mọi chương trước và vẫn cho Chương 6 quyền quyết định ai sẽ có mặt.

## 4. Những gì cố ý không làm

- Không đổi công thức `c7_diem_pq`, không đổi ngưỡng 90 / 60 / 35.
- Không sửa Chương 1–6.
- Không thêm art, audio hay scene mới: HUD dùng lại đúng panel sẵn có, chỉ thêm một thanh mảnh dưới thanh lòng tin và một dòng chữ dưới danh sách lời khai.
- Không gợi ý khi quan hết kiên nhẫn. Gợi ý là thứ chỉ đến từ lòng tin còn lại, không đến từ thời gian đã cạn.

## 5. Kiểm chứng

- `Tools/Ink/InkValidationReport.txt`: `INK VALIDATION PASS`, nhánh `chapter_7` đi được cả bốn kết (`c7_ket` 0 / 1 / 2 / 3).
- Playtest tự động với bốn đích `nghich_menh`, `thang`, `mat_minh`, `dinh_menh`: cả bốn chạy hết Chương 7 tới dòng kết, không exception, không soft-lock.
- Lượt chơi có thẻ phái sinh: nộp `so_bac` ở vòng 2, `giao_keo` ở vòng 2, sang vòng 3 bẻ câu then chốt bằng `vong_no` (2 điểm).
- Lượt chơi thua cả ba vòng: mỗi vòng một lần nộp sai kéo `c7_kien_nhan` từ 100 xuống 75 và lòng tin xuống 0; quan tự xét giấy tờ, `c7_thua_vong` lên 3, chương vẫn đi tiếp tới dòng kết.

Bảng kiểm toán đầy đủ từng lựa chọn và từng thẻ bằng chứng nằm ở `Docs/Chapter07_Choice_Evidence_Matrix.md`.
