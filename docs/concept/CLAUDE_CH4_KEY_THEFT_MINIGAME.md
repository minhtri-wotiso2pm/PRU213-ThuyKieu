# Prompt cho Claude — triển khai minigame lấy chìa mới ở Chapter 4

Hãy triển khai đầy đủ minigame lấy chìa mới trên scene Chapter 4 thật, theo thiết kế đã thảo luận dưới đây. Người dùng muốn cảnh cận, có thao tác gỡ chìa và căng thẳng vì phải dừng theo hơi thở Tú Bà. **Không hiển thị tay Kiều.** Không dừng ở kế hoạch hoặc demo riêng.

## 1. Phạm vi và hiện trạng

- Đọc `AGENTS.md`, kiểm tra Git, giữ thay đổi chưa commit. Không tự xóa asset, commit/push, thay package hoặc sửa YAML scene/prefab thủ công. Dùng công cụ Unity và điều kiện build theo quy định project.
- Đọc `docs/concept/KEY_THEFT_HORROR_MINIGAME_DESIGN.md` làm tham khảo, cùng mục 16 và các mục mới hơn nếu có của `docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md`. **File này là đặc tả triển khai hiện tại**, ưu tiên khi brief cũ vẫn yêu cầu tay Kiều hoặc chỉ giữ E để tự cộng tiến độ.
- Kiểm tra runtime hiện có trước khi sửa. `KeyTheft` đã có 12 giây giữ đúng nhịp, `TuBaSleeper` quản lý pha thở và audio; `LauTuBaKeyRing`/`HangingKeys` đã dựng chùm chìa mới. Tận dụng chúng, không quay về bản cũ hoặc dựng hệ thống thất bại thứ hai.
- Chỉ nâng cảnh lấy chìa Ch4. Giữ truyện, inventory, J1/J2, nhánh Lam, cửa sau, chase/catch và checkpoint. Giữ Foley chân/cửa đã được chấp nhận tạm thời. Không bắt đầu chuyển gameplay Ch3 trong đợt này.

## 2. Trải nghiệm và điều khiển

### Vào minigame

- Khi Kiều ở đủ gần, nhìn đúng chùm chìa và Tú Bà đang nằm, hiển thị gợi ý ngắn để vào thao tác.
- Người chơi chủ động kích hoạt thì chuyển camera sang góc cận mượt. Một lần bấm để mở minigame không được tự tính thành thao tác sai nhịp trong lúc camera đang chuyển. Sau khi vào xong, yêu cầu nhả E rồi bấm mới để bắt đầu nắm nếu E còn đang giữ từ lúc kích hoạt.
- Trong lúc thao tác, người chơi điều khiển vòng/chùm chìa bằng **giữ E và kéo chuột chậm theo hướng gợi ý**. Không có model tay/cánh tay Kiều và không tải thêm bộ tay.
- Mouse input đang dùng cho thao tác không đồng thời xoay camera. Khi thoát phải trả đúng input và trạng thái con trỏ trước đó. Tận dụng Unity Input System/input abstraction sẵn có.

### Luật nhịp thở được giữ nguyên

- **Thở ra:** giữ E và kéo đúng hướng mới tiến hành gỡ chìa.
- **Hít vào:** phải buông E. Nếu đang nắm và giữ E sang pha hít vào, hoặc bắt đầu nắm ngay trong pha hít vào, Tú Bà cảm thấy, tỉnh và chạy jumpscare bắt Kiều.
- Đang nắm mà không kéo chuột vẫn là đang chạm chìa: nếu giữ E sang hít vào vẫn bị bắt, không được dùng việc không di chuột để né luật.
- Nhả E chỉ dừng thao tác, giữ tiến độ và giữ camera cận để chờ nhịp kế tiếp.
- Cho phép chủ động rút lui bằng input phù hợp đang dùng, có gợi ý ngắn. Khi pause, mở menu hoặc tương tác bị hủy, phân biệt rõ với thao tác chủ động để không xử lý tiếp input cũ. Không để phím thoát minigame gây thoát game hoặc xung đột menu hiện có.

## 3. Góc nhìn cận để tạo căng thẳng

- Camera tiến nhẹ, hạ hướng về thắt lưng và thu FOV vừa phải trong khoảng 0,4–0,6 giây làm mốc thử. Với FOV gốc khoảng 72°, thử vùng 58–62° rồi điều chỉnh qua xem thực tế; lưu FOV thật của người chơi để khôi phục, không hard-code 72° lúc thoát.
- **Bố cục:** vòng/chìa/móc ở tiền cảnh, một phần ngực và mặt Tú Bà vẫn trong khung để đọc hơi thở và dấu hiệu nguy hiểm. Không zoom đến mức chỉ thấy chìa và mất hết liên hệ với bà.
- Giữ camera cận qua mọi lần buông E chờ hơi thở; không zoom ra/vào liên tục theo nhịp nút.
- Camera có kiểm tra vật cản, không xuyên sập, áo, thân, tường hay chùm chìa. Không teleport nhân vật chỉ để đặt góc đẹp. Kiểm tra cả lúc Kiều đứng và cúi, góc tiếp cận thực tế bên cạnh sập.
- Khi hoàn thành hoặc rút lui, trở về góc chơi mượt. Khi bị bắt, chuyển trực tiếp từ góc cận sang `CatchScene` của Tú Bà rồi thất bại/checkpoint, không nhảy về camera tự do một khung giữa hai cảnh.
- Luật tương tác và tầm bắt dựa vào vị trí/quan hệ thật của nhân vật, không bị thay đổi ngoài ý muốn do camera zoom.

## 4. Ba giai đoạn gỡ chìa — tổng 12 giây hợp lệ

Thể hiện thao tác trực tiếp trên đạo cụ, không chỉ thay nhãn một thanh chờ:

| Giai đoạn | Thao tác và hình ảnh | Mốc thời gian ban đầu |
|---|---|---|
| 1. Nâng chùm chìa | Giữ E, kéo theo hướng gợi ý để vòng nghiêng và chìa nâng khỏi chỗ tựa, vẫn giữ đúng kết nối với móc | 4 giây hợp lệ |
| 2. Gỡ vòng khỏi móc | Kéo vòng qua miệng móc; thay đổi góc/độ cao hợp cấu tạo thật | 4 giây hợp lệ |
| 3. Rút chìa ra | Vòng thoát khỏi móc, kéo chùm ngắn về phía Kiều rồi xuống ngoài khung; inventory nhận đúng một bộ | 4 giây hợp lệ |

- **12 giây là thời gian tối thiểu thao tác hợp lệ**, chỉ tính khi thở ra, E đang giữ và có chuyển động kéo phù hợp. Chờ, giữ E đứng yên hoặc kéo sai hướng không cộng tiến độ.
- Hướng kéo thể hiện rõ bằng gợi ý nhỏ ngay gần vòng, không bắt dò pixel. Chọn quỹ đạo đơn giản và hướng hợp cơ cấu; không ép đổi hướng liên tục chỉ để tăng khó.
- Ánh xạ mouse delta sang tiến triển có giới hạn tốc độ và lọc rung nhỏ. Một cú giật nhanh không hoàn thành cả giai đoạn; không buộc người chơi rê chuột liên tục hết bàn hoặc phụ thuộc DPI cực đoan. Xác định cách re-grip bằng nhả E để đặt lại chuột, không mất tiến độ.
- Kéo sai hướng/quá nhanh không phải lỗi chết mới: dừng/giới hạn tiến triển và phản hồi nhẹ. Chỉ áp luật bị bắt theo nhịp thở, tỉnh/cựa và các nguy cơ vốn có, không tự thêm meter hoặc QTE ngẫu nhiên.
- Giữ tiến độ giữa các nhịp và khi rút lui chủ động như quy tắc cũ. Đạo cụ phải có điểm tựa hợp lý lúc dừng, không treo lơ lửng vô cớ. Nếu cần đặt lại tư thế chìa khi rời và phục hồi khi vào lại, dùng chuyển động rõ, không nhảy hình hoặc tự hoàn tất phần còn lại.
- Mục tiêu tổng thời gian khi chơi đúng khoảng 20–30 giây; nếu người chơi chậm/chờ thêm thì có thể lâu hơn. Đo riêng thời gian hợp lệ và thời gian tổng, không rút ngắn ngầm 12 giây để đạt số đẹp.

## 5. Đạo cụ và phản hồi

- Model hiện có đang gộp móc, vòng và các chìa trong một mesh. Tách phần cố định trên thắt lưng khỏi phần vòng/chùm chuyển động hoặc sửa builder Ch4 theo cách thích hợp; không kéo cả móc khỏi người Tú Bà.
- Móc phải có miệng mở/hướng tháo hợp lý. Không diễn hoạt vòng xuyên qua một khoen kín. Giữ chất lượng model/kim loại mới và không làm ảnh hưởng `StoryPropKit.KeyRing` của các chương khác.
- Giữ gắn hông và `HangingKeys` ngoài minigame. Trong minigame cần có một bên sở hữu pose đạo cụ để animation và mô phỏng treo không ghi đè/giật nhau mỗi frame; trả lại đúng lúc thoát/bắt/checkpoint.
- Không hiển thị tay Kiều. Truyền lực bằng nghiêng/nâng/rút chìa, đung đưa nhỏ, kim loại chạm và vải áo; không dùng camera rung thay toàn bộ chuyển động.
- Hơi thở, chuyển động ngực/áo, UI và luật nguy hiểm phải dùng cùng pha thở. Giữ tín hiệu báo sắp hít vào hiện có để người chơi kịp buông E; không thay nhịp đột ngột không báo trước.
- Tiến độ nhìn thấy trên vật thể. HUD chỉ cần giai đoạn, hướng kéo và nhịp thở ngắn; tránh nhiều meter cùng lúc hoặc đoạn hướng dẫn dài giữa cảnh.
- Tiếng chìa khẽ đồng bộ với chuyển động và không tự đánh thức Tú Bà ở mọi thao tác đúng. Không nối tất cả tiếng Foley mới vào `Noise` nếu chưa có thiết kế/cân bằng riêng.
- Giữ việc Tú Bà chỉ ngáy khi thực sự nằm ngủ; đầu chương đang đối thoại/đi về thì không ngáy. Không sửa lại bộ ngáy hoặc voice ngoài phần đồng bộ cần thiết nếu không phát hiện lỗi mới.

## 6. Vòng đời và tích hợp

- Dùng `KeyTheft`, `TuBaSleeper`, `PlayerInteraction`, camera controller và luồng `TuBaCatches`/`CatchScene` hiện có. Chỉ thêm component hẹp trách nhiệm nếu cần; không tạo một game mode toàn cục mới.
- Chuyển sang chế độ cận không làm mất target theo kiểu `Lost()` cũ khiến phiên nắm bị hủy mỗi frame. Quản lý ownership/session rõ ràng và kiểm tra ranh giới pha bằng cùng đồng hồ như code đợt 16.
- Frame vừa chuyển sang hít vào và E vẫn giữ: chỉ một lần bắt, không cộng thời gian hoặc nhận chìa cùng frame. Không lách luật bằng quay camera, mất target, mở menu hoặc thoát minigame đúng ranh giới trong khi thao tác nguy hiểm đã xảy ra.
- Nguy hiểm chỉ khi đang lấy chìa hợp lệ; giữ E ở cửa/vật khác hoặc đứng xa không làm Tú Bà bắt từ xa.
- Thành công: `_item` đúng, `Taken` đúng một lần, đạo cụ ẩn đúng, tiến trình sang bước 2 và tiếp tục J1/J2 như hiện tại.
- Thua/checkpoint: reset tiến độ chưa xong theo checkpoint, model móc/vòng đúng, camera/FOV/input/con trỏ/HUD/audio/nhân vật không bị kẹt. Checkpoint sau lấy chìa vẫn có chìa và không hiện lại đạo cụ.
- Xử lý thoát cảnh, component bị disable/destroy, NPC tỉnh hoặc cảnh bắt khác ngắt phiên. Cleanup chạy an toàn và không khôi phục đè lên camera của `CatchScene` mới bắt đầu.
- Cập nhật scene Ch4 và setup để lần tạo scene sau giữ cơ chế mới. Bảo toàn thay đổi thủ công; không sinh lại Ch3 ngoài phạm vi.

## 7. Kiểm tra và bàn giao

- Recompile và sửa lỗi phát sinh. Cập nhật bài thử lấy chìa để dùng input E + mouse qua luồng tương tác thật; không gọi thẳng `TookKeys()` rồi coi đã kiểm minigame.
- Bắt đầu khi đứng/cúi, camera vào/ra mượt, không xuyên đồ, bố cục đủ thấy chìa và Tú Bà; không có tay Kiều.
- Đúng nhịp và hướng: đủ 12 giây hoạt động mới thành công; giữ E không di chuột, kéo sai hướng, một cú giật nhanh, nhả đặt lại chuột và rút lui quay lại đều theo luật trên.
- Sai nhịp: bấm khi hít vào, giữ E xuyên pha, giữ nhưng không kéo qua pha, FPS thấp/khung chậm, frame cuối gần hoàn tất. Mỗi lần một jumpscare/thất bại, không nhận chìa đồng thời.
- Entry bằng E giữ sẵn, nhả chờ, pause/menu, thay target/rời tầm, bị bắt giữa zoom hoặc kéo, abort/đổi scene, checkpoint trước/sau lấy chìa. Camera/FOV/input trở về đúng.
- Kiểm tra âm chìa/ngáy khớp nhịp, không có ngáy đầu chương. Nghe và chơi trực tiếp nếu công cụ hỗ trợ; nếu không thì ghi rõ phần chất âm/cảm giác/bố cục chưa xác minh, không lấy test muted PASS thay cho duyệt trải nghiệm.
- Hồi quy nhiệm vụ, J1/J2, Lam, cửa sau và các ca bắt liên quan Ch4; tránh sửa lỗi chase còn mở bằng cách tắt perception trong bài thử minigame.
- Giữ yêu cầu không quay thêm clip mới trong phiên hiện tại. Không tự commit. Build theo quyền và điều kiện của `AGENTS.md`, báo rõ exe có khớp source mới không.
- Cập nhật mục mới trong implementation: cấu trúc đạo cụ, camera, điều khiển, thời gian đo được, kiểm thử, hiệu năng và giới hạn.

Bàn giao bản minigame chạy đầy đủ trên Ch4 thật, nêu rõ đã thực hiện/đã xác minh/chưa xác minh. Ưu tiên thao tác dễ hiểu và căng thẳng có nguyên nhân, không thêm luật phụ ngoài phạm vi trên.
