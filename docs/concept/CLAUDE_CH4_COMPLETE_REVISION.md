# Prompt tổng hợp — hoàn thiện Chapter 4

**Bổ sung mới nhất từ chơi thực tế:** ưu tiên bốn sửa đổi trong `CLAUDE_CH4_TUBA_KEYS_REFINEMENT.md`: ngừng ngáy khi Tú Bà đang đối thoại/đứng; thay chất âm ngáy; dựng đẹp chùm chìa; lấy chìa đủ 12 giây thao tác trong nhịp thở ra, giữ E sang nhịp hít vào sẽ làm mụ tỉnh và jumpscare. Bộ Foley chân/cửa hiện đã được chấp nhận tạm thời. Các lỗi chase còn mở vẫn giữ trong danh sách theo dõi, không tự coi đã hết.

**Ưu tiên mới nhất:** người dùng chấp nhận tạm thời chất âm đợt 14 nhưng thấy lính đuổi, tìm kiếm và bỏ cuộc bất thường. Tập trung `CLAUDE_CH4_CHASE_SEARCH_FIX.md`; giữ bộ âm hiện tại làm baseline, chưa tiếp tục thay Foley. Ch3 vẫn làm sau. Các yêu cầu audio bên dưới là lịch sử các đợt đã triển khai, không tự chạy lại khi chưa có vấn đề mới.

**Phản hồi mới của người dùng:** bước chân và cửa hiện nghe giả, chất lượng chưa đạt. Thực hiện đợt thay nguồn âm theo `CLAUDE_CH4_AUDIO_FOLEY_UPGRADE.md` trước khi coi phần audio hoàn thành. Các yêu cầu tận dụng SFX hiện có bên dưới không có nghĩa giữ các clip tổng hợp đang bị chê; giữ logic tiếng động/AI và nâng chất lượng âm nghe được.

Hãy triển khai hoàn chỉnh các thay đổi dưới đây trên Chapter 4 của project hiện tại. Người dùng ưu tiên gameplay lén lút chậm, căng thẳng, có cơ hội thoát khi bị phát hiện và jumpscare khi thực sự bị bắt. Hoàn thiện trên scene chơi thật, không dừng ở đề xuất hoặc dựng demo riêng.

## A. Phạm vi và tài liệu

- **Chỉ hoàn thiện Chapter 4 trước.** Chapter 3 sẽ đổi sang khám phá tự do, không truy đuổi/bắt/phạt, ở đợt sau; chưa triển khai chuyển Ch3 lúc này. Không thêm cảnh bắt Ch3 để mở rộng kiểm thử hoặc làm đồng bộ hai chương.
- Đọc `AGENTS.md`, kiểm tra Git và bảo toàn thay đổi chưa commit. Không tự commit/push, xóa asset, đổi package hay sửa YAML scene/prefab thủ công. Dùng công cụ Unity phù hợp, tuân thủ điều kiện build của project.
- Đọc mục 11–12 của `docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md` làm lịch sử hiện trạng; xác minh lại source và scene trước khi dựa vào số liệu cũ.
- Tham khảo `docs/concept/CAUGHT_JUMPSCARE_REFERENCES.md`, `CLAUDE_CH4_FIRST_FINISH.md` và `CLAUDE_CH4_SOUND_STEALTH.md`. **File tổng hợp này là chỉ dẫn chính cho đợt hiện tại** khi tài liệu cũ mâu thuẫn về phạm vi.
- Giữ hình ảnh, kiến trúc hai tầng trang trí và mặt bằng đã hoàn thiện. Giữ cốt truyện, chìa khóa, khế ước, nhánh Lam, J1/J2, tuyến bếp/cửa sau và checkpoint. Chỉ điều chỉnh liên quan trực tiếp tới các yêu cầu dưới đây.

## B. Bước chân Kiều theo bề mặt và cách di chuyển

1. Kiều do người chơi điều khiển phải có tiếng bước chân khớp vật liệu: gỗ, gạch/đá, đá ướt, đất/bùn và chiếu/thảm theo những vùng thực tế trên map. Đổi tiếng đúng khi bước qua ranh giới bề mặt.
2. Cùng bề mặt, độ ồn và khả năng bị nghe phải theo thứ tự **khom < đi < chạy**. Chạy có nhịp nhanh và lực bước rõ; chiếu/thảm giảm tiếng, đá/gỗ/ướt có đặc trưng riêng. Đây phải là thay đổi cả âm thanh nghe được lẫn noise gameplay, không chỉ tăng volume.
3. Tận dụng `FirstPersonFootsteps`, `SurfaceTag`, `Noise`, `NoiseZone` và SFX hiện có. Không dựng một hệ thống âm thanh/nhận biết cạnh tranh.
4. Kiểm tra tag sàn/bậc/lớp phủ; sàn thiếu tag hiện mặc định thành gỗ. Hẻm đang dùng material `Mud` nhưng tag `WetStone`, cần xác minh và phân loại đúng. Sàn ướt không có nghĩa mọi nơi dùng tiếng đá ướt.
5. Đồng bộ bước theo chuyển động thực trên mặt đất. Đứng yên, ép tường không tiến được, trên không, teleport/checkpoint, bị khóa điều khiển và cảnh bắt không được sinh bước giả. Tránh đếm hai lần một bước.
6. Tận dụng vài biến thể âm thanh hoặc biến thiên nhẹ để tránh lặp máy móc; không thay độ lớn quá ngẫu nhiên làm người chơi không đoán được rủi ro. Nghe kiểm với tai nghe/loa khi có công cụ; không dùng muted QA làm bằng chứng audio đạt.

## C. Mở/đóng cửa mạnh và làm khẽ

1. Mở bật/đóng sập có tiếng rõ và noise lớn phát tại vị trí cửa đúng khoảnh khắc gây tiếng. Giữ E làm khẽ tạo chuyển động chậm, âm nhỏ và khó bị nghe hơn. Giải thích thao tác ngay trong nhãn tương tác phù hợp UI hiện có.
2. Kiểm tra cơ chế có sẵn của `HouseDoor`, gồm `Snap`, thao tác giữ, then/chốt và cửa khóa. Không spam noise mỗi frame hoặc phát tiếng mạnh khi hình ảnh đang mở rất nhẹ.
3. Phân biệt tiếng cánh cửa, trượt cửa, then/chốt và thử cửa khóa bằng SFX hiện có khi phù hợp.
4. Kiểm tra cửa NPC/cốt truyện mở bằng `SetOpen`: không tự quy mọi tiếng cửa thành hành động Kiều, không để NPC tự nghe tiếng mình rồi điều tra vô hạn. Giữ các sự kiện âm thanh truyện có chủ đích.

## D. Lính canh và Tú Bà phản ứng với tiếng động

1. NPC nghe theo khoảng cách, mức ồn, tường/cửa đóng và vùng mưa/nhạc che tiếng. Không cho cả map nghe một bước chân và không áp dụng cùng độ nhạy cho mọi trạng thái NPC.
2. Tiếng nhỏ đủ gần: ngừng/quay đầu, phản ứng ngắn. Tiếng lớn hoặc lặp lại: đi kiểm tra **vị trí phát tiếng cuối cùng**. Nếu Kiều rời đi thì điều tra nơi cũ; chỉ cập nhật vị trí theo tiếng mới thực sự nghe được hoặc quan sát hợp lệ.
3. Khi xác nhận nhìn thấy Kiều thì truy đuổi. Người chơi vẫn có cơ hội cắt tầm nhìn/nấp/chạy thoát. Bắt hợp lệ ở cự ly gần mới có jumpscare. Tiếng động không tự cung cấp vị trí Kiều liên tục xuyên tường và không gây thua từ xa.
4. Tú Bà đang ngủ: tiếng nhẹ có thể làm cựa; tiếng lớn đủ gần hoặc lặp lại có thể đánh thức, ngừng ngáy, lẩm bẩm, đứng dậy/kiểm tra hoặc gọi lính phù hợp diễn biến.
5. **Rà `TuBaSleeper.Woke` → `Chapter04LauWorld.OnTuBaWoke`: hiện có luồng thất bại ngay khi tỉnh trước J2.** Phải tách tỉnh do nghe tiếng cửa từ xa với tình huống thực sự bắt gặp Kiều bên giường. Mở cửa từ xa không được thua ngay. Giữ thử thách lấy chìa theo nhịp ngủ và tránh làm thức sớm khiến kẹt nhiệm vụ/J2.
6. Khôi phục checkpoint phải trả đúng ngủ/thức, nghi ngờ, điểm điều tra, tiếng ngáy và tuyến tuần. Không phát lại noise cũ hoặc giữ cờ gây phản ứng sai sau khôi phục.

## E. Hoàn thiện jumpscare Ch4

1. Dùng `CatchScene` và `GrabPose` hiện tại, xem ảnh ghép/clip đã có trong `Tools/LauTuBa/Clips`. Không làm lại hệ thống thất bại từ đầu.
2. Kiểm tra runtime rồi giảm cảm giác mặt/nón NPC bị chiếu quá sáng trực diện. Đặt ánh sáng lệch để mặt đọc rõ và có thể tích; giữ exposure toàn cảnh và không biến NPC thành quái vật ngoài cốt truyện.
3. Cú túm cần có lực: tay tới đúng chỗ → ngón tay siết ở thời điểm chạm → vai/thân kéo → camera phản ứng. Không để chỉ có hai tay vươn về camera hoặc bàn tay xuyên người/vật thể.
4. Hoàn thiện bắt phía trước, phía sau và trong chỗ nấp; đứng/cúi, tủ, bình phong phòng Lam, rèm và chỗ nấp thấp. Chọn động tác theo đúng loại chỗ nấp; không dùng mở cửa tủ cho mọi trường hợp.
5. Kiểm tra riêng Tú Bà ở Ch4. `TuBa-Yell` có sẵn chỉ dùng nếu phù hợp sau khi xem, không mặc định nó là clip bắt hoặc biểu cảm mặt.
6. Căn tiếng vải, va chạm, hít thở, cú kéo/xô với animation khi nghe trực tiếp. Lời quát tiếng Việt nếu có phải là bản thu/tài nguyên có quyền sử dụng rõ. Nếu thiếu voice, tiếp tục phần độc lập và báo rõ phụ đề chưa thay thế giọng nói; không tự tuyên bố đã có voice.
7. Giữ chống hai NPC bắt cùng lúc, chống bắt xuyên tường/cửa và phương án thất bại thường khi một sự kiện ép bắt từ quá xa để dựng cú túm thật.
8. Xử lý hủy/ngắt cảnh bắt hoặc đổi scene: cleanup phải trả camera, FOV, input, audio, time scale, pose, đạo cụ, NPC và cờ `CatchScene.Playing`, không chỉ chờ coroutine chạy hết. Giữ chuyển sang thất bại/checkpoint đúng luồng hiện tại.

## F. Cân bằng và hiệu năng

- Kiểm tra lại nhiều điểm/tuyến băng sân với khoảng cách, hướng nhìn, vật cản, ánh sáng và âm bước chân. Kết quả cũ đứng 70 giây tại một điểm không đủ chứng minh toàn sân.
- Chạy phải nguy hiểm hơn đi khẽ ở cùng điều kiện nghe. Nếu có tuyến chạy thẳng bỏ qua mọi nguy cơ, ưu tiên chỉnh điểm dừng/hướng quan sát/tuyến lính riêng Ch4 có chủ đích, giữ khoảng an toàn cho người chơi tính đường. Không tăng tầm nhìn/độ sáng toàn game tùy tiện.
- Đo ảnh hưởng hệ thống tiếng động và lần bắt đầu tiên. Giữ baseline có khung 54 ms, không gọi là hoàn toàn không khựng. Khi có hitch, xác định bằng profiler; phân biệt loading, gameplay, Editor và exe.
- Không thêm tài nguyên/gói lớn chỉ để thay việc sửa logic. Có thể chủ động tìm tài nguyên miễn phí có nguồn rõ khi thiếu; nếu bị chặn bởi đăng nhập/mua tài nguyên, báo đúng phần vướng và tiếp tục phần khác.

## G. Kiểm tra và bàn giao bắt buộc

- Compile và chạy kiểm tra liên quan sau thay đổi. Dùng test phù hợp hành vi, không chỉ assertion rằng một hàm đã được gọi.
- Đối chứng từng vật liệu × khom/đi/chạy; chuyển bề mặt; đứng yên/ép tường/teleport không có bước giả.
- Cùng vị trí NPC: mở khẽ so với mở mạnh, đi khẽ so với chạy; cùng phòng và qua vật cản; trong vùng mưa/nhạc. Ghi ai nghe, nguồn/vị trí tiếng và phản ứng.
- Gây tiếng rồi rời đi: NPC đến nơi phát tiếng, không tự bám vị trí mới. Kiểm tra hết nghi quay về tuần tra và hai NPC phản ứng không khóa nhau.
- Tú Bà ngủ/cựa/thức, tiếng cửa xa/gần, lấy chìa, J2 và checkpoint: mọi trường hợp còn đường tiến truyện hợp lệ.
- Ch4 map/stealth/catch/checkpoint, nhánh Lam, J1/J2 và cửa sau; các ca bắt trước/sau/từng loại nấp/Tú Bà/cạnh tường-cửa-bậc/hai NPC cùng bắt. Kiểm tra hồi quy code dùng chung mà chưa chuyển gameplay Ch3.
- Thử trực tiếp có bật tiếng nếu công cụ hỗ trợ. Nếu không làm được, báo rõ phần cảm giác chuyển động/âm thanh chưa xác minh. **Giữ yêu cầu dừng quay clip mới trong phiên hiện tại**, không tự bật lại capture.
- Cập nhật mục mới trong `docs/codex/CHAPTER3_4_LAU_TU_BA_IMPLEMENTATION.md`, nêu file đổi, thông số đã cân bằng, kiểm thử, hiệu năng, giới hạn và exe có khớp source hiện tại hay chưa. Chỉ build theo quyền và điều kiện của `AGENTS.md`.

Bắt đầu bằng kiểm tra hiện trạng và xác định ngắn gọn các file cần sửa, rồi triển khai. Bàn giao Chapter 4 hoàn thiện trong phạm vi trên; tách rõ đã làm, đã kiểm chứng và chưa kiểm chứng. Chapter 3 là đợt tiếp theo.
