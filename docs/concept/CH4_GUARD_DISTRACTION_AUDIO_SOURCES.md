# Chapter 4 — dụ lính bằng sỏi: quyết định gameplay và nguồn Foley

Ngày đối chiếu nguồn: 21/10/2026. Đây là ghi chú thiết kế và danh sách ứng viên, chưa triển khai gameplay, chưa tải/import hay nghe thử các bản thu.

Prompt triển khai gộp cho Claude: [CLAUDE_CH4_GUARDS_DISTRACTION_REVISION.md](CLAUDE_CH4_GUARDS_DISTRACTION_REVISION.md). Dùng prompt này cho đợt sửa đã được người dùng chốt; tài liệu này giữ nguồn âm và giải thích chi tiết.

## Phương án người dùng đã chọn

- Đầu Ch4, sau đối thoại và khi Tú Bà về ngủ: một lính gác cửa phòng Tú Bà, lính còn lại gác cổng. Kiều gây tiếng bằng viên sỏi để dụ lính cửa đi kiểm tra rồi đột nhập.
- Tận dụng cơ chế ném sỏi hiện có. Đặt nguồn nhặt dễ thấy gần phòng Kiều; đề xuất sức chứa 3 viên và có thể quay lại bổ sung, tránh hết đồ gây kẹt nhiệm vụ.
- Sau khi Tú Bà phát hiện mất chìa và báo động ở J2: cả hai lính hiện có đi tuần hai khu vực bổ sung nhau. Không tự chuyển sang báo động ngay lúc lấy chìa khi NPC chưa biết chuyện.
- Lính tới vị trí tiếng va chạm thật, kiểm tra rồi đi bộ về; không biết vị trí hiện tại của Kiều chỉ nhờ tiếng động. Không ngắt cuộc truy đuổi đã xác nhận bằng một tiếng sỏi.
- Tiếng chân/ánh đèn báo trước lúc lính quay lại. Chu kỳ phải cho phép vào phòng, thao tác lấy chìa khoảng 20–30 giây tổng cộng và rút lui. Khoảng 45–60 giây vắng cửa chỉ là giá trị thử ban đầu, cần đo trên tuyến thật.
- Giữ nguyên minigame mới không hiện tay Kiều và các quy tắc 12 giây thao tác đúng nhịp; không tự sửa gameplay Ch3.

## Bổ sung người dùng đã chốt: vùng an toàn và quyền mở cửa của lính

### Phòng Kiều là vùng an toàn

- Chỉ phòng Thúy Kiều được bảo đảm an toàn; các phòng khác không có miễn trừ phát hiện/bắt chỉ vì người chơi bước vào hoặc đóng cửa.
- Trong Ch4, lính không tự mở/đóng cửa phòng Kiều, không đi vào phòng, không lôi khỏi chỗ nấp trong phòng và không bắt xuyên cửa/ngưỡng. Tú Bà khi ở vai trò truy bắt cũng không được phá quy tắc vùng an toàn.
- Kiều đã vào hẳn vùng an toàn thì kết thúc việc đuổi trực tiếp. Lính có thể kiểm tra vị trí cuối ngoài hành lang rồi rời đi, không đứng chặn cửa mãi và không quên mọi cảnh giác ngay lập tức.
- Bảo vệ người chơi bên trong không phụ thuộc cửa đang mở hay đóng. Từ ngoài hành lang vẫn có thể thấy cửa mở, nhưng không dùng điều đó để truy bắt người đang ở trong vùng an toàn. Xử lý rõ khi người chơi đứng ở ngưỡng để không có lúc an toàn/bị bắt nhấp nháy giữa các frame.
- Tiếng người chơi gây ra bên trong có thể khiến lính nghe ngóng ngoài phòng, nhưng không kéo lính vào. Tiếng viên sỏi ném từ phòng ra ngoài được tính ở điểm rơi ngoài phòng bình thường.
- Đối thoại mở đầu và J2 phải được rà soát để không vô tình cho hành vi kịch bản mở cửa/xâm nhập phòng Kiều phá quy tắc mới trong lúc chơi tự do. Chỉ sửa đoạn xung đột cần thiết, giữ nhiệm vụ và mạch chương.

### Lính được mở/đóng cửa các phòng khác

- Khi tuần tra, điều tra tiếng động hoặc tìm kiếm, lính có thể tự mở cửa chưa khóa ở các phòng khác để đi qua hoặc nhìn/kiểm tra bên trong. Đóng cửa đơn thuần không tạo vùng an toàn mới.
- Hành vi có mục đích theo tuyến kiểm tra và bằng chứng; không mở toàn bộ cửa ngẫu nhiên, không tự biết người chơi nằm trong phòng nào.
- Tới cửa thì dừng/quay về phía cửa, thao tác mở có thời gian, âm thanh và chuyển động cánh cửa; đi qua khi khe đủ rộng. Khi kiểm tra xong có thể khép cửa sau khi cả người đã qua, không đóng lên Kiều hoặc lính khác.
- Chỉ kiểm tra đường nhìn vào phòng sau khi cửa thật sự mở đủ. Không thấy xuyên cánh cửa còn đóng, không bắt xuyên tường, không dịch chuyển qua cửa.
- Lính không tự xuyên khóa, gỡ then bên kia hoặc mở cửa có điều kiện nhiệm vụ. Với DoorLock.Locked/Bolted/LatchedOutside, chỉ thao tác khi có quyền và đúng phía được xác định rõ; mặc định không vượt khóa. Cửa bếp thoát, cửa Lam và phòng sổ sách phải giữ điều kiện nhiệm vụ hiện có.
- Cửa khóa không đồng nghĩa phòng an toàn: Kiều còn có thể bị thấy qua lối khác hoặc bị tiếp cận bằng đường hợp lệ khác. Không được đánh dấu đã tới chỗ nấp khi lính vẫn bị chặn ngoài cửa khóa.
- Hai lính không tranh nhau liên tục đóng/mở một cửa. Tiếng cửa do NPC tạo không được khiến chính hắn hoặc đồng đội điều tra lặp vô tận; phân biệt tác nhân người chơi/NPC/kịch bản.
- Giữ tiếng cửa Foley đã được duyệt, phát tại cửa và đúng thời điểm chuyển động. Cửa do NPC thao tác phải có âm nghe được; không lạm dụng đường gọi kịch bản im lặng.

### Ghi chú cho Claude khi triển khai

- Đã kiểm tra HouseDoor có DoorLock, Gate, SetOpen và snapshot cửa. Đây chưa phải bằng chứng AI đã có luồng mở cửa an toàn. Không gọi SetOpen để lách khóa/Gate, cũng không gọi trực tiếp input E của người chơi cho NPC.
- NavMesh có thể coi cửa đóng là chặn đường: phân biệt cửa mở được với phòng thực sự không tới được, đồng thời giữ các sửa lỗi đường đi hoàn chỉnh và chống kẹt trước đó.
- Checkpoint lưu/khôi phục đúng cửa, khóa, giai đoạn gác/tuần tra và trạng thái an toàn. Kiểm thử thực tế: chạy về phòng Kiều khi bị đuổi; đứng tại ngưỡng; cửa Kiều đang mở; phòng khác đóng nhưng không khóa; phòng chốt bên trong; hai lính cùng cửa; tiếng cửa NPC; J2 và thoát bếp.
- Đây là yêu cầu triển khai bổ sung, hiện mới cập nhật tài liệu. Chưa sửa source/scene, chưa chạy Unity hay xác nhận gameplay.

## Bổ sung người dùng: mở rộng tầm nhìn và xử lý đứng sát lính không bị phát hiện

- Yêu cầu: trong Ch4, Kiều không được đứng sát phía trước hoặc bên cạnh lính, không vật cản, mà vẫn vô hình chỉ vì cúi/ngừng di chuyển/ở tối. Giữ phòng Kiều là vùng an toàn.
- Đã đọc source: SightModel hiện có tầm nhìn chính do Watchman._halfAngle quyết định (setup lính tuần tra là 40 độ mỗi bên, tổng 80 độ); ngoại vi tới 70 độ mỗi bên trong 4 m. CloseFloor chỉ áp dụng trong góc chính, nên tăng bán kính nhìn đơn thuần chưa xử lý góc mù bên cạnh. Đây là khả năng từ cấu hình, chưa phải nguyên nhân đã tái hiện cho lần lỗi của người chơi.
- Trước khi sửa, tái hiện đứng sát ở trước/bên/sau, đứng và cúi; kiểm tra Eyes.forward so với mặt rig, vị trí mắt/đích head-chest, RestrictedZone, Perceiving, pause/kịch bản và vật cản collider. Phân biệt lỗi không phát hiện với bị bắt chưa hợp lệ.
- Thử góc chính tổng 120 độ (halfAngle 60), ngoại vi tổng 180 độ (halfAngle 90) trong khoảng 4 m cho lính Ch4. Đây là thông số khởi đầu cần chơi thử, không mở 360 độ và không tăng đồng loạt khoảng nhìn xuyên cả sân.
- Trong khoảng 1.5–2 m phía trước/bên cạnh và có đường nhìn thật: bảo đảm nhận diện rõ sau khoảng 0.5–1 giây, cả đứng yên/cúi/trong tối; ngoại vi gần phải có ngưỡng nhận diện tối thiểu phù hợp, thay vì chỉ tăng hệ số trong góc chính. Phản ứng quay đầu phải hướng về điều hắn vừa thấy.
- Phía sau vẫn có thể lén tiếp cận nếu im lặng và không chạm. Nếu Kiều thực sự đụng thân lính, cho phản ứng va chạm/quay lại khi ở cùng không gian, không dựa riêng vào bán kính để phát hiện xuyên vách. Đây là cảm nhận tiếp xúc, không phải tầm nhìn 360 độ.
- Không bắt ngay chỉ vì Awareness đầy: vẫn phải tiếp cận trong tầm với, có đường nhìn và không bị cửa/vách ngăn, rồi mới dùng catch scene hiện có.
- Phòng Kiều, chỗ nấp kín hợp lệ và các collider cản tầm nhìn vẫn có hiệu lực. Không bỏ raycast/đổi mọi vật cản thành trong suốt để làm test qua.
- SightModel dùng chung: cấu hình thay đổi cho Ch4 bằng cơ chế hiện có hoặc tùy chọn mặc định giữ hành vi chương khác; không vô tình làm Ch3 chuyển sang đuổi bắt hay tăng độ khó toàn game.
- Kiểm thử trên scene thật với cảm giác hoạt động bình thường, không nâng Sensitivity trong test: trước/bên/sau ở 0.5/1/2/4 m, rìa góc nhìn, đứng/cúi, sáng/tối, cửa đóng/cửa mở, vật cản mỏng, vùng an toàn và ngưỡng cửa Kiều. Ghi thời gian nhận diện và nguyên nhân bị che; kiểm tra hai lính ở giai đoạn sau J2 và tuyến dụ lính vẫn có thể vượt qua.
- Hiện mới cập nhật yêu cầu cho Claude; source và scene chưa được sửa hoặc chạy kiểm thử ở lượt này.

## Nguồn ưu tiên — đã kiểm tra trang gốc

### 1. Đá nảy trên đá: ứng viên chính cho nền cứng

- Tên: Stone dropping.
- Tác giả: alegemaate / Allan Legemaate.
- Nguồn: https://freesound.org/people/alegemaate/sounds/364711/
- Mô tả tác giả: một viên đá nảy vào viên đá khác; có tag small.
- CC0; WAV stereo, 44.1 kHz, 16 bit, 0.470 giây.
- Trang hiện yêu cầu đăng nhập để tải. Chưa có file local; không dùng cách vượt đăng nhập.
- Có thể thử cho sỏi va nền đá; không tuyên bố đây là bản thu trên chính nền gạch/đá của map.

### 2. Đá nhỏ rơi trên gỗ

- Tên: small stones dropped on wooden table 1.
- Tác giả: FOSSarts.
- Nguồn: https://freesound.org/people/FOSSarts/sounds/740544/
- Thu bằng cách thả các tinh thể garnet thô lên bàn gỗ, theo mô tả tác giả; không phải bản thu một viên sỏi trên sàn.
- CC0; WAV stereo, 96 kHz, 32 bit, 3.582 giây.
- Trang yêu cầu đăng nhập để tải. Nếu tải được, nghe và chọn đoạn va riêng phù hợp; không phát cả loạt đá khi hình ảnh chỉ có một viên.

### 3. Đá rơi xuống cỏ — ứng viên bề mặt mềm

- Tên: Stone Drop on Grass 3.wav.
- Tác giả: dichardson.
- Nguồn: https://freesound.org/people/dichardson/sounds/570976/
- CC0; WAV stereo, 44.1 kHz, 16 bit, 0.532 giây.
- Trang yêu cầu đăng nhập để tải. Đây là cỏ, không phải đất bùn hoặc thảm; chỉ dùng thay thế nếu nghe thử phù hợp và ghi rõ nguồn thật.

### 4. Sỏi va chậu gốm: còn thiếu

- Chưa xác minh được bản thu đúng kích cỡ và thao tác. Không gán nhầm tiếng kéo chậu, kim loại hoặc vỡ kính thành sỏi va chậu.
- Nếu cần chậu gốm làm điểm dụ quan trọng: tìm tiếp bản thu trực tiếp hoặc tự thu sỏi nhỏ va chậu không vỡ. Chưa có nguồn phù hợp thì ưu tiên nền đá đã có ứng viên.
- Loại khỏi lựa chọn chính: https://lasonotheque.org/chute-de-pierre-s1022.html — dù CC0 và tải không cần tài khoản, tác giả mô tả đá 1–3 kg rơi lên đống đá; sai kích cỡ cho viên sỏi nhỏ.

## Tiêu chí triển khai và duyệt âm thanh cho Claude

- Dùng bản thu thật; không tự tổng hợp tiếng beep/thud thay rồi báo đã hoàn thành yêu cầu Foley.
- Các nguồn trên mới được kiểm tra metadata/giấy phép, chưa được đánh giá bằng tai. Không hứa âm thanh hay hoặc chân thật chỉ vì là CC0/WAV.
- Mỗi vật liệu cần các take khác nhau nếu có; không nhân bản một clip hoặc chỉ đổi pitch rồi gọi là nhiều bản thu. Pitch thay đổi rất nhẹ, tránh làm sỏi thành đá lớn.
- Âm phát tại điểm va chạm thật khi viên sỏi chạm bề mặt, không phát ở camera hay ngay lúc bấm ném.
- Va chạm, nảy và lăn phải khớp hình ảnh. Chặn việc nhiều callback vật lý tạo tiếng chồng; nếu clip đã chứa tiếng nảy thì không phát trùng toàn bộ clip ở mỗi lần nảy. Cũng không dùng ba lần nảy của cùng viên sỏi để giả thành ba tiếng động độc lập vượt ngưỡng nghe.
- Phân biệt nền đá/gạch, gỗ và nền mềm. Nguồn cỏ/garnet là ứng viên thay thế cần duyệt, không phải bằng chứng đã có đủ Foley đúng mọi vật liệu.
- Âm lượng phụ thuộc lực va, bề mặt và khoảng cách; phát 3D, không thêm bass lớn hoặc reverb dài làm sai kích thước viên sỏi.
- Sự kiện AI nghe phải tương ứng va chạm có âm, với ngưỡng/che âm nhất quán. Không báo vị trí Kiều qua sự kiện va chạm từ xa, không phát tiếng lại chỉ để ép lính đi.
- Chỉ import clip đã dùng; lưu tác giả, URL, giấy phép và chỉnh sửa trong credits. Nếu vướng đăng nhập, báo rõ nguồn nào chưa tải, vẫn tiếp tục phần công việc độc lập.
- Duyệt bằng tai trong Ch4, có tiếng mưa và tại khoảng cách dụ lính thực tế. Kiểm thử tự động chỉ chứng minh logic, không chứng minh chất lượng âm thanh. Không quay thêm clip khi người dùng chưa yêu cầu lại.
