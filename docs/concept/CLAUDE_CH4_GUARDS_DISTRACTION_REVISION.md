# Prompt cho Claude — Chapter 4: dụ lính, tuần tra, cửa, vùng an toàn và tầm nhìn

Hãy triển khai hoàn chỉnh trên Chapter 4 hiện tại trong project này. Đây là yêu cầu sửa game thật, không chỉ đề xuất hay làm demo. Đọc AGENTS.md, kiểm tra thay đổi chưa commit và giữ nguyên công việc hiện có.

Tài liệu chi tiết đã chốt: `docs/concept/CH4_GUARD_DISTRACTION_AUDIO_SOURCES.md`. Dùng yêu cầu hiện tại dưới đây khi tài liệu cũ có mâu thuẫn. Chỉ đọc phần liên quan trong báo cáo triển khai; không làm lại các vòng nâng cấp đã hoàn thành.

## 1. Mục tiêu và phạm vi

Đầu Ch4 hiện quá dễ đi tới phòng Tú Bà và né lính. Thay bằng nhịp: quan sát lính gác → nhặt/ném sỏi để dụ → đột nhập → gỡ chìa → lấy giao kèo → J2 báo động → lẩn tránh hai lính tuần tra để thoát.

- Dùng hai lính hiện có, không tự thêm lính thứ ba hoặc tầng/map/nhiệm vụ mới.
- Giữ minigame lấy chìa mới: góc cận không hiện tay Kiều, E + chuột, 12 giây thao tác đúng nhịp thở ra, giữ E sang nhịp hít vào thì Tú Bà bắt. Không lùi về giữ E tự tăng tiến độ.
- Giữ Foley bước chân/cửa đã duyệt, quest, nhánh Lam, J1/J2, checkpoint và catch scene. Ch3 không thuộc đợt sửa gameplay này; giữ đồng bộ âm thanh Ch3 đã làm.
- Thông số góc nhìn/thời gian dưới đây là giá trị khởi đầu để đo và cân bằng, không tăng độ khó bằng cách cho NPC biết vị trí Kiều vô điều kiện.

## 2. Hai giai đoạn của lính

Trước báo động:
- Sau đối thoại mở đầu và khi Tú Bà về ngủ, lính 1 đứng chếch cửa phòng Tú Bà, nhìn dọc lối tiếp cận. Trước thời điểm đó không phá đối thoại/đường Tú Bà về phòng và không cho bắt trong lúc khóa điều khiển.
- Lính 2 gác cổng. Lính 1 nghe tiếng dụ hợp lệ thì quay nhìn, phản ứng, đi bộ đến nguồn âm, kiểm tra và đi về vị trí gác.
- Lính không đi làm mục tiêu cố định khiến nhiệm vụ bất khả thi: đo thời gian Kiều dụ, tới cửa, vào phòng, thao tác gỡ chìa khoảng 20–30 giây tổng cộng và rút ra. Có thể bắt đầu với 45–60 giây từ lúc lính rời cửa tới lúc trở về; chỉnh theo đường thật. Không giữ lính đứng yên bí mật chỉ vì minigame đang chạy.
- Tiếng chân, ánh đèn và lời nói phải giúp người chơi nhận biết lính đang đi hoặc về. Cho phép ngắt tương tác để nấp/rút lui theo luồng hiện có; không bắt buộc chết khi đã chọn đúng thời điểm.
- J1 sau khi lấy giao kèo vẫn hoạt động; không dùng trạng thái gác cửa để chặn đoạn kiểm tra phòng sổ sách của J1. Kịch bản không được ghi đè một cuộc truy đuổi thật đang diễn ra.

Sau báo động J2:
- Chỉ chuyển cả hai sang tuần tra khi Tú Bà thực sự phát hiện mất chìa và gọi báo động; không chuyển ngay sự kiện Taken.
- Lính 1 phụ trách hành lang phòng Tú Bà/kho/sổ sách; lính 2 đi từ cổng qua sảnh/sân và lối về bếp. Chọn waypoint phù hợp hình học hiện tại, có điểm dừng quan sát và giao tuyến nhưng vẫn có khoảng trống vượt qua.
- Di chuyển từ vị trí thật, không teleport. Không để hai lính liên tục chặn lối duy nhất hoặc tự mở cửa thoát giúp hoàn thành quest.
- Nếu phối hợp gọi hỗ trợ: người kia nhận khu vực/vị trí lần cuối được báo và đi kiểm tra; muốn đuổi chính xác phải tự thấy hoặc có thông tin cập nhật hợp lệ. Mất dấu không theo vị trí thật mới của Kiều.
- Checkpoint khôi phục đúng vai trò, tuyến, hướng, trạng thái báo động và cửa; không có hai bản NPC hoặc hai tuyến cùng điều khiển một người.

## 3. Ném sỏi và âm thanh thật

- Tận dụng PebbleThrow và input ném hiện có (Q), bổ sung hướng dẫn ngắn và chỗ nhặt sỏi dễ nhận biết gần phòng Kiều. Sức chứa khởi đầu 3 viên, có nguồn lấy lại để không kẹt nhiệm vụ.
- Quan trọng từ source hiện tại: PebbleThrow bay theo coroutine tới điểm ngắm, bỏ collider, phát `clatter` bằng PlaySfx ở cuối và gọi cả GuardPatrol lẫn Noise. Không coi đây là va chạm thật đã hoàn chỉnh. Sửa để xác định va chạm sớm với cửa/cột/trần/vật cản trên quỹ đạo, không cho sỏi bay xuyên rồi gây tiếng phía sau tường. Kiểm tra gọi AI trùng.
- Âm phát 3D tại điểm va chạm thật, đúng thời điểm, theo vật liệu và lực va. Tiếng nảy nhẹ dần, không lặp nguyên clip chứa sẵn chuỗi nảy. Các lần nảy cùng viên không được giả thành ba sự kiện độc lập để vượt ngưỡng nghe tiếng nhỏ.
- Sỏi va nền cứng đủ rõ có thể dụ lính trong phạm vi nghe; rơi lên nền mềm tạo tiếng nhỏ hơn. Vẫn áp dụng che âm/vật cản/mưa hợp lý. Ném ngay trước mặt khiến Kiều bị nhìn thấy như thường.
- Lính tới nơi phát ra tiếng, không tới vị trí người ném. Sỏi không hủy Chase đã xác nhận. Chống dụ lặp vô hạn bằng phản ứng cảnh giác/kiểm tra có giới hạn, không thêm máy dò Kiều sau lưng.
- Nguồn ứng viên đã xác minh metadata và CC0:
  - Đá nảy trên đá: https://freesound.org/people/alegemaate/sounds/364711/
  - Đá nhỏ trên gỗ (thực tế tinh thể garnet rơi lên bàn): https://freesound.org/people/FOSSarts/sounds/740544/
  - Đá trên cỏ, chỉ là ứng viên nền mềm: https://freesound.org/people/dichardson/sounds/570976/
- Các nguồn trên chưa tải hay nghe duyệt; trang hiện cần đăng nhập tải. Không hứa đã có audio nếu chưa có file. Tìm bản thu thật thay thế tải hợp lệ được khi cần; không dùng âm tổng hợp/âm đá lớn để thay rồi báo xong. Không mua hoặc tạo tài khoản thay người dùng. Nếu chỉ phần tải bị chặn, tiếp tục gameplay và báo đúng phần audio còn thiếu.
- Sỏi va chậu gốm không vỡ chưa có nguồn phù hợp được xác minh; không lấy tiếng vỡ kính hoặc kéo chậu làm bằng chứng đã có. Ưu tiên điểm dụ nền đá trước nếu chưa có Foley chậu tốt.
- Import chỉ clip dùng, ghi tác giả/URL/license/chỉnh sửa trong credits. Không sửa lại footsteps/door Foley. Kiểm thử muted không xác nhận âm hay; ghi rõ cần nghe với mưa và ở khoảng cách chơi thật nếu không nghe được.

## 4. Phòng Kiều là vùng an toàn duy nhất

- Lính và Tú Bà ở vai trò thù địch không vào, không tự mở/đóng cửa phòng Kiều và không bắt/lôi khỏi chỗ nấp bên trong. An toàn kể cả cửa đang mở.
- Kiều vào hẳn phòng thì kết thúc đuổi trực tiếp. NPC có thể tìm ở vị trí cuối ngoài hành lang một lúc rồi rời, giữ cảnh giác hợp lý nhưng không cắm trại cửa mãi. Không chỉ tắt sight mà vẫn cho NavMesh/Caught/Script đi vào phòng.
- Ngưỡng an toàn rõ và ổn định; dùng cùng một quy tắc cho perception, bắt, path, noise và checkpoint. Không bắt xuyên ngưỡng, cũng không mở rộng vùng sang cả hành lang.
- Tiếng bên trong có thể dẫn NPC tới vị trí kiểm tra ngoài cửa; viên sỏi ném ra ngoài vẫn được nghe tại điểm rơi. Các phòng khác không được miễn phát hiện chỉ vì cửa đóng.
- Rà J2: TuBaAtKieuDoor hiện chờ 25 giây ngoài phòng. Giữ nhịp cảnh cáo cần thiết nhưng không xâm nhập/vô hiệu hóa vùng an toàn và không kéo dài chặn lối vô hạn. Giữ mạch truyện, sửa nhỏ đoạn xung đột.

## 5. NPC mở/đóng các cửa khác

- Lính được tự mở cửa chưa khóa khi tuyến tuần tra, điều tra hoặc tìm kiếm cần đi qua/kiểm tra phòng; không mở hàng loạt ngẫu nhiên. Khi đuổi, cửa chưa khóa không phải bức tường bất khả vượt.
- Tiếp cận cửa, dừng/quay, mở có thời gian và Foley, đợi đủ khe rồi đi qua; có thể khép khi người đã qua. Không đóng trúng Kiều hoặc đồng đội, không đóng/mở tranh nhau. Không yêu cầu thêm tay/rig mới.
- Tôn trọng DoorLock và Gate/quest. Mặc định NPC không mở khóa/cài then từ sai phía hoặc tự có chìa. Giữ điều kiện cửa Lam, phòng sổ sách và cửa sau.
- HouseDoor.SetOpen hiện là API kịch bản, không tự kiểm khóa và cố ý không phát Noise cho AI; không gọi trần để lách quyền. Thêm luồng NPC rõ quyền dùng cửa, tách tiếng nghe được khỏi noise bị hiểu là do Kiều. Không gọi input E của người chơi cho NPC.
- Tiếng cửa NPC không gây vòng lặp tự điều tra/chuyển mục tiêu của chính hắn hoặc đồng đội. Mở cửa không được kích hoạt nhầm tiến độ chỉ dành cho thao tác Kiều.
- Cửa đóng có thể chặn NavMesh: xử lý điểm đứng hai phía, đường qua cửa mở được và quyền thao tác; vẫn từ chối đường thật sự bị khóa/không tới được. Không bỏ toàn bộ kiểm tra path để vượt qua cửa.

## 6. Tầm nhìn rộng hơn, không mù khi Kiều đứng sát

- Tái hiện lỗi trước khi sửa: trước/bên/sau, đứng/cúi, sáng/tối. Kiểm tra Eyes.forward của rig, vị trí mắt và điểm raycast, Perceiving/pause, RestrictedZone và collider. Không kết luận chỉ do FOV nếu chưa có bằng chứng.
- Source hiện: lính chính setup halfAngle 40 (tổng 80 độ), ngoại vi tới 70 mỗi bên trong 4 m; CloseFloor chỉ trong góc chính. Thử chính tổng 120 độ (halfAngle 60), ngoại vi tổng 180 độ (halfAngle 90) khoảng 4 m, cấu hình riêng Ch4.
- Kiều ở trước/bên trong 1.5–2 m, không bị che, phải được nhận diện trong khoảng 0.5–1 giây dù cúi/đứng im/trong tối. Xử lý ngoại vi gần chứ không chỉ tăng nhìn xa. Sát trục bên cạnh phải có phản ứng quay đầu hợp lý.
- Phía sau vẫn lén qua được nếu yên lặng, không chạm; va chạm thân thực sự thì NPC phản ứng/quay lại. Không dùng bán kính 360 độ xuyên vách thay cảm giác tiếp xúc.
- Giữ raycast/che khuất, phòng Kiều và chỗ nấp hợp lệ. Awareness đầy chỉ bắt đầu đuổi; catch vẫn cần tầm với, không vật cản và tiếp cận hợp lệ.
- Watchman/SightModel dùng chung: thay đổi mặc định tương thích hoặc tùy chọn Ch4; không tăng độ khó hay đưa Chase vào Ch3 bằng tác dụng phụ.

## 7. Triển khai, kiểm thử và bàn giao

- Sửa scene Ch4 thật và đường setup/builder tương ứng bằng công cụ Unity phù hợp để rebuild không mất. Không sửa Scene/Prefab YAML bằng tay, không ghi đè thay đổi ngoài phạm vi. Kiểm tra trạng thái làm dở trước.
- Giữ các sửa lỗi chase/search/catch trước đó. Không xóa assertion hoặc nới timeout chỉ để báo pass; lỗi exe gián đoạn trước đó vẫn là vấn đề mở trừ khi tái hiện và chứng minh đã sửa.
- Dùng kiểm thử hiện có và thêm kiểm thử hành vi cần thiết trên scene thật, perception bật và Sensitivity bình thường. Không dùng teleport/bỏ perception để chứng minh tuyến lén thực sự vượt qua được; fixture đặt vị trí được cho test riêng từng cảm giác, báo rõ giới hạn.
- Cần chứng minh: lấy sỏi/giới hạn/bổ sung; quỹ đạo bị cột/cửa chặn; noise tại va; dụ lính gác đi và về; giữ Chase khi ném; có đường vào/lấy chìa/rút ra; chuyển hai tuyến ở J2; phối hợp không biết vị trí bí mật; tìm rồi quay lại, không kẹt/cắm trại cửa.
- Cần chứng minh vùng an toàn: bị đuổi chạy về, cửa mở/cửa đóng, đứng ngưỡng, tiếng trong phòng, checkpoint. Cần chứng minh cửa khác: mở được/khóa/then, hai NPC cùng cửa, không kích quest hoặc tự điều tra tiếng cửa mình.
- Sight: kiểm trước/bên/sau tại 0.5/1/2/4 m, sáng/tối, đứng/cúi, vật cản và cửa; ghi thời gian nhận diện, không tự tăng Sensitivity trong test. Kiểm cả hai lính sau J2.
- Chạy recompile và các test liên quan có sẵn: map, sound/hearing, chase, stealth, catch, key theft, checkpoint, Ch4 normal/thorough với J1/J2/Lam/cửa sau; kiểm tra Ch3 không bị tác dụng phụ. Mở rộng chain test khi thay đổi thật sự có rủi ro tới chuỗi chương.
- Không quay/chụp thêm clip hay ảnh. Không tự commit, không cài package, không full build chỉ vì sửa source; tuân thủ AGENTS.md. Nếu chưa rebuild exe, báo rõ exe cũ chưa chứa thay đổi, không dùng kết quả exe cũ để xác nhận code mới.
- Bàn giao ngắn: hành vi đã sửa, lỗi gốc được chứng minh, thông số cuối, file chính, kết quả test thực tế, phần audio chưa tải/nghe và phần cần chơi tay. Không gọi xong toàn bộ khi một hạng mục còn thiếu.
