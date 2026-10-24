# Prompt cho Claude — sửa Tú Bà ngủ, tiếng ngáy và lấy chìa ở Ch4

## Yêu cầu mới đã chốt

Hãy sửa trực tiếp Chapter 4 theo bốn phản hồi từ người chơi:

1. Vừa vào chương, Kiều đang đối thoại với Tú Bà mà đã nghe tiếng ngáy: phải sửa đúng vòng đời ngủ/thức.
2. Tiếng ngáy hiện nghe giả và dở: thay chất âm cho chân thật, dễ nghe và phân biệt nhịp hơn.
3. Chùm chìa khóa trên người Tú Bà còn thô: dựng đẹp và thật hơn khi nhìn cận để lấy chìa.
4. Khi đang lén lấy chìa, **giữ E lúc Tú Bà hít vào phải làm mụ tỉnh và chạy jumpscare bắt Kiều**. Người dùng đã chọn **12 giây thao tác đúng nhịp thở ra**, khoảng **20–30 giây tổng cộng khi tính chờ**. Tổng thời gian là ước lượng theo nhịp, không phải timeout ép hoàn thành.

Đọc `AGENTS.md`, kiểm tra Git, bảo toàn thay đổi chưa commit. Chỉ hoàn thiện Ch4, Ch3 làm sau. Giữ bộ Foley chân/cửa đợt 14 đã được người dùng chấp nhận tạm thời. Theo dõi lỗi chase còn mở ở mục 15; không tự viết rằng chúng đã hết. Không mở rộng nhiệm vụ hoặc quay thêm clip mới trong phiên hiện tại.

## 1. Sửa tiếng ngáy sai thời điểm

Hiện trạng đã đọc trong code:

- `TuBaSleeper.State` mặc định `Asleep`.
- `OnNoise()` đã kiểm tra `_actor.IsLying`, nhưng `Update()` chạy chu kỳ/phát ngáy khi `State == Asleep` mà chưa kiểm tra bà đã nằm/ngủ thật.
- Mở đầu `Chapter04LauWorld` đặt Tú Bà đứng tại cửa Kiều; tới sau đoạn đối thoại mới gọi `TuBaLeavesKieuDoor()` và `PutTuBaToBed()`.

Hãy xác minh trên scene rồi sửa:

- Đối thoại đầu chương, đứng, đi về phòng: không ngáy, không chạy đồng hồ lấy chìa, không nhận tương tác lấy chìa như đang ngủ.
- Chỉ bắt đầu nhịp thở/ngáy sau khi Tú Bà thực sự nằm và được chuyển vào trạng thái ngủ đúng tiến độ.
- Tỉnh, cựa mình/nằm nghe theo thiết kế, ngồi/đứng dậy, cảnh bắt, đổi scene: dừng nguồn ngáy đang phát và đồng hồ ngủ tương ứng. Không chỉ chặn phát clip kế tiếp trong khi clip cũ vẫn còn vang.
- Quản lý riêng AudioSource/handle của âm thở/ngáy nếu cần; không mute toàn bộ nhân vật/ambience để che lỗi.
- Kiểm tra bắt đầu chương mới, tiếp tục save, checkpoint trước/sau lấy chìa và sau J2. Không dùng trạng thái `Awake` sau J2 để thay cho mọi trạng thái đứng đầu chương nếu nó làm sai snapshot/truyện.
- Tạm dừng/hội thoại/cảnh bắt phải giữ đồng bộ trạng thái và thời gian; không để audio và pha thở chạy theo hai đồng hồ khác nhau.

## 2. Thay tiếng ngáy và làm rõ nhịp

- Kiểm tra clip runtime thực dùng qua bank/cache; không chỉ đọc nhánh synth rồi kết luận. Trong project hiện thấy `snore_deep`, `snore_out`, `snore` trong bank cũ, chưa thấy bản thu Recorded cùng tên khi rà file.
- Ưu tiên bản thu người thật phù hợp Tú Bà, có nguồn/quyền sử dụng rõ. Nếu tải cần đăng nhập, báo chính xác phần vướng; không gọi thêm noise tổng hợp là đã thay bản thu.
- Loại tiếng ù/rít điện tử, click, đoạn lặp lộ, tiếng nền lạ. Chọn vài chu kỳ tự nhiên, có độ khàn/ngáy vừa phải; tránh kéo pitch cực đoan một giọng không phù hợp.
- Phân biệt được **hít vào/nguy hiểm** và **thở ra/an toàn** bằng tiếng thở và tín hiệu chuyển nhịp. Phải khớp chính xác đồng hồ luật chơi; không phát clip hít vào dài vượt sang vùng đang được tính là thở ra.
- Giữ một nguồn trạng thái pha thở cho cả audio, UI và `KeyTheft`. Nếu clip có độ dài khác nhịp hiện tại, biên tập/chọn đoạn hoặc điều chỉnh lịch một cách rõ ràng rồi đo lại tổng thời gian; không kéo giãn mạnh làm hỏng chất giọng.
- Phát từ đầu/ngực Tú Bà, nghe gần rõ và xa dịu phù hợp phòng; không để ngáy lấn đối thoại hoặc phủ toàn map như âm nền 2D.
- Nghe trực tiếp nếu công cụ hỗ trợ. Nếu chưa nghe được, báo rõ chưa duyệt chất âm; muted QA chỉ xác minh trạng thái/pha, không chứng minh ngáy tự nhiên.
- Giới hạn bank/ID thay thế cho Ch4 để tránh đổi các chương khác. Giữ CREDITS và phân biệt nguồn thu thật, bản ghép và âm tổng hợp còn lại.

## 3. Dựng lại chùm chìa trên người Tú Bà

Hiện `StoryPropKit.KeyRing()` tạo vòng bằng cylinder đặc và bốn chìa bằng cube, dùng material emissive. `LauTuBaSetup.KeysAtBelt()` gắn vào xương hips.

- Dựng vòng thật có lỗ, tiết diện tròn, kèm móc/dây treo vào thắt lưng. Chìa có đầu cầm khoét lỗ, thân và răng rõ, kích thước/tỉ lệ hợp người và ổ khóa trong game; tránh chùm thanh vàng thẳng.
- Dùng vài chìa dài ngắn/góc treo khác nhau, tách được silhouette ở góc nhìn lấy chìa. Ưu tiên chất lượng cận cảnh hơn số lượng chi tiết.
- Material kim loại phù hợp bối cảnh: đồng/sắt có roughness biến thiên và chút mòn tại mép/chỗ chạm, bắt ánh đèn phòng. Không tự phát sáng để làm nổi tương tác; nếu cần gợi ý dùng hệ highlight hiện có.
- Vát cạnh vừa đủ bắt sáng; normal/UV đúng. Có thể dùng mesh tự dựng hoặc asset phù hợp đã có; không mua/tải model lớn không cần thiết.
- Giữ gắn xương hợp lý khi bà đứng, đi, nằm, cựa và ngồi dậy. Không nổi cách hông, xuyên áo/sập hoặc xoay lệch do scale của rig.
- Căn interaction target/collider với vị trí chìa thực khi nằm, không để lấy ở một ô trống hoặc xuyên người/tường. Giữ cơ chế tầm tay và điều kiện bắt hợp lệ.
- Lấy thành công thì chùm biến mất đúng một lần, inventory nhận đúng một bộ; checkpoint trả lại/giữ mất theo tiến độ. Dựng riêng Ch4 nếu sửa builder dùng chung làm ảnh hưởng đạo cụ chương khác.

## 4. Luật lấy chìa: 12 giây thở ra, giữ E lúc hít vào thì bị bắt

- `KeyTheft.Tick()` hiện chỉ `Disturb(0.55f * deltaTime)` khi hít vào. Thay đoạn đó bằng luồng tỉnh/bắt riêng cho hành vi **tay đang lấy chìa và vẫn giữ E trong pha hít vào**.
- Giữ E từ cuối thở ra sang đầu hít vào cũng phải bị bắt; không cần thả/bấm lại E để luật có hiệu lực. Bấm bắt đầu lấy trong pha hít vào cũng nguy hiểm tương tự.
- Hợp nhất pha thở trên cùng một nhịp cập nhật, tránh phụ thuộc thứ tự Update giữa `TuBaSleeper` và tương tác khiến UI báo an toàn nhưng luật đã phạt.
- Luồng sai nhịp: dừng tiến độ → ngừng ngáy → Tú Bà tỉnh/chộp tay → jumpscare của Tú Bà → thất bại/checkpoint. Chỉ một cảnh, một lần thất bại, không phát trùng với việc đứng bên giường hoặc sự kiện `Roused`/`HandCaught`.
- Chỉ áp luật này trong tương tác lấy chìa hợp lệ bên người bà. Giữ E mở cửa, nhìn đồ vật hoặc đứng xa không làm mụ tỉnh bắt từ xa. Mất target/đổi hướng nhìn trong một lần giữ phải có xử lý rút tay hoặc tiếp tục phiên tương tác rõ ràng, không cho phép bỏ Tick để lách luật khi tay vẫn đang gỡ chìa.
- **Tổng tích lũy đúng 12 giây giữ E trong pha thở ra** mới nhận chìa. Thời gian chờ hít vào/không giữ E không được tính. Giữ tiến độ giữa các nhịp thở và lúc chủ động thả E; chưa lấy xong mà thua/khôi phục checkpoint thì tiến độ quay về theo quy tắc checkpoint hiện có.
- Thiết lập 12 giây trong cấu hình/setup và instance scene Ch4 thực tế; chỉ sửa giá trị mặc định `_seconds` trong source không đảm bảo instance serialized đổi theo.
- Nhãn tương tác gọn và rõ: “Thở ra — giữ E để gỡ chìa” / “Hít vào — buông E”. Thông báo thất bại phải giải thích lỗi giữ E sai nhịp; cập nhật lời hướng dẫn truyện/HUD liên quan cho khớp.
- Không tăng tốc độ thanh tiến độ theo autopilot để bù test cũ. Cập nhật bài thử cho 12 giây thao tác thật, giới hạn chờ đủ dài và đo thời gian đang giữ hợp lệ riêng với tổng thời gian.

## 5. Kiểm tra và bàn giao

1. Mở đầu chương qua đủ đoạn thoại và lúc bà đi về: không có ngáy; chỉ phát khi đã nằm ngủ.
2. Đứng/ngủ/cựa/tỉnh/J2/catch/checkpoint/pause: audio và pha thở đúng trạng thái, không nguồn ngáy sót hoặc chồng.
3. Giữ E đúng thở ra, buông trước hít vào: nhiều nhịp hợp lệ, đủ 12 giây thì lấy một lần. Đo tổng thời gian thực tế, mục tiêu cảm giác khoảng 20–30 giây nếu làm đúng; báo nếu nhịp thực tế cho kết quả khác.
4. Giữ E xuyên ranh giới sang hít vào; bấm bắt đầu trong hít vào; buông ngay trước ranh giới; kiểm tra FPS thấp/khung chậm. Sai nhịp chỉ một jumpscare của Tú Bà, không được cộng tiến độ hoặc nhận chìa cùng frame bắt.
5. Giữ E ở mục tiêu khác/ngoài tầm; đổi target/quay camera khi đang giữ; sau khi đã có chìa; trong đối thoại hoặc lúc nhân vật đang bị khóa. Không bắt sai ngữ cảnh, không lách luật giữ sai nhịp.
6. Chùm chìa đúng ở các tư thế đứng/nằm/cựa/catch, target khớp mesh, inventory/checkpoint đúng. Xem trực tiếp khi có công cụ; không tuyên bố model đẹp chỉ nhờ test dữ liệu.
7. Kiểm tra hồi quy tiếng động từ xa: cửa xa vẫn chỉ gây chú ý, không vì luật lấy chìa mới mà thua ngay. Giữ nhánh Lam, J1/J2, hợp đồng, cửa sau và các ca bắt Ch4 liên quan.
8. Recompile, chạy test phù hợp, cập nhật mục mới trong implementation. Không coi bước autopilot bỏ qua lấy chìa trực tiếp bằng `TookKeys()` là đã kiểm tra minigame này.

Tôn trọng quyền scene/asset/build theo `AGENTS.md`; không commit, không xóa asset, không tự quay thêm clip. Ghi chính xác đã làm/đã kiểm chứng/chưa kiểm chứng và exe có khớp source không. Nếu chưa nghe/xem/chơi trực tiếp được, nêu rõ giới hạn đó. Chưa có thay đổi gameplay chỉ từ việc lưu tài liệu này.
