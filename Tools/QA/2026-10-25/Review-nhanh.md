# Review nhanh Thúy Kiều — 25/10/2026

> **Cập nhật sau sửa:** xem [kết quả fix và kiểm thử MCP](Fix-results.md). R02–R04 đã sửa và kiểm thử pause/resume đạt. Camera/follower và truy vấn tương tác đã sửa. R01 không tái hiện ở phiên Play sạch; chưa kết luận nguyên nhân lịch sử. Đã thoát Play và khôi phục save. Nội dung bên dưới giữ lại kết quả của lượt review ban đầu.

**Ưu tiên:** kiểm tra lại lỗi mở đầu → sửa đồng hồ khi pause ở chapter 5/7 → kiểm tra camera đồng hành chapter 6 → đo giật khi tải cảnh.

## Phạm vi và độ chắc chắn

- Nhánh `GiaThuan`, HEAD `1777a2d`, có thay đổi chưa commit. Một số file gameplay/scene tiếp tục thay đổi trong lúc thử; đây không phải một bản build cố định.
- Đã dùng **Unity MCP**: đọc Editor/Console, chạy Play Mode, mở menu, chuyển vào chapter 0, gửi phím qua Input System và chụp Game View. Không bật StoryAutopilot trong lượt này.
- **Chỉ menu và đoạn đầu chapter 0 được kiểm tra trực tiếp trong lượt mới.** Các chapter còn lại bên dưới là review code/thiết kế và các tình huống cần thử, không phải tuyên bố đã chơi xong.
- Kết nối MCP ban đầu đã được khởi động thành công. Về cuối, lệnh tiếp theo không được thực thi vì bộ xét duyệt tự động báo hết hạn mức. Chưa thể hoàn thành lượt chơi từng chapter, kiểm tra FPS hoặc xác nhận trạng thái Play Mode cuối cùng.
- Báo cáo [24/10](../2026-10-24/Review.md) có ảnh và kết quả bot ở **bản dev cũ**. Không dùng kết quả PASS đó để kết luận bản dev hiện tại đạt, đặc biệt hai scene lầu Tú Bà mới.

## Hướng dẫn đã rút gọn

- Menu Điều khiển: 14 dòng xuống 9 dòng, dạng **Tương tác: E**, **Sổ tay: J**, **Tạm dừng: Esc**. Bỏ mô tả dài và phím bảng lệnh khỏi danh sách chính.
- Thẻ HƯỚNG DẪN theo ngữ cảnh: 2–5 dòng; giữ những quy tắc cần thiết như giữ E để mở khẽ, nhả trước khi cửa kêu, không chạy gần người của Tú Bà.
- File sửa trong lượt này: `Assets/_Game/Scripts/UI/StoryUIBuilder.cs`, `Assets/_Game/Scripts/Gameplay/Sequences/TutorialSequence.cs`. Không sửa YAML scene/prefab để thay chữ.
- Sau yêu cầu biên dịch qua MCP, Editor trở về trạng thái không compiling, không pending domain reload; Console trả về 0 lỗi. Menu đã được kiểm tra bằng [ảnh Điều khiển](Captures/Controls.png). Chưa xác nhận hình thức của từng thẻ hướng dẫn trong gameplay.
- Những thay đổi xuất hiện thêm ở `DialogueManager`, `StoryHUD`, scene, material và các script khác không phải thay đổi của lượt sửa Hướng dẫn này; không hoàn tác chúng.

## Review theo chapter

| Chapter | Vị trí/cơ chế cần chú ý | Đánh giá và việc nên sửa | Mức xác nhận |
|---|---|---|---|
| **0 — Trang sách** | Câu đầu của thầy giáo trong lớp | Trong phiên thử, nền cảnh đen và Space không tiếp tục được do lỗi null. Đây là điểm cần kiểm tra đầu tiên trước demo. Hướng dẫn nên hiện từng thao tác; bản chữ ngắn đã áp dụng. | **Đã thấy trực tiếp**, cần tái hiện trên phiên sạch vì có sửa code song song. |
| **1 — Gia biến** | Trong nhà, mở cổng, quan sát túi bạc | Nên ưu tiên một mục tiêu chính, giữ manh mối phụ trong sổ tay. Cho người chơi hiểu rõ quan sát bạc là tùy chọn và khi nào phải buông chuột phải. Kiểm tra xoay camera sát tường, bàn và cột; chưa kết luận hiện còn xuyên/che camera. | Review hướng dẫn/code; chưa chơi lại chương. |
| **2 — Theo Mã Giám Sinh** | Bám đoàn, dừng quan sát, mất khoảng cách | Nên chỉ rõ hướng trở về đoàn và lý do thanh nghi ngờ tăng. Thử dừng nhặt giấy rồi đuổi theo, đi vòng cây/vách, quay camera nhanh. Có `BuildNavMesh()` đồng bộ khi khởi tạo: cần đo thời gian tải cảnh. | Rủi ro từ code; chưa xác nhận lag hay kẹt đường. |
| **3 — Tú Bà** | Scene mới `Chapter03_LauTuBa`, cửa/then, dãy nhà sau | Phân biệt thật rõ **bấm E = làm mạnh**, **giữ E = làm khẽ**; người mới dễ bấm theo thói quen và gây tiếng động. Nên có tiến trình khi giữ E và dấu hiệu khu cấm dễ nhận ra. Cần thử góc nhìn tại cửa, hành lang và cột trong scene mới. | Quy tắc có trong setup; đề xuất UX, chưa xác nhận thiếu phản hồi tại runtime. |
| **4 — Không còn đường lui** | Scene mới `Chapter04_LauTuBaDem`, bóng tối, nấp, Tú Bà ngủ | Thông tin quan trọng: tiếng ngáy ngừng, ánh đèn làm lộ, bị nhìn thấy lúc nấp vẫn bị tìm. Nên đưa từng dấu hiệu ngay khi xuất hiện, tránh một thẻ chữ dài. Thử cửa/hốc nấp và khả năng nhìn lối đi trong tối. | Chưa chơi scene mới. Không coi lỗi pose ngủ trong báo cáo cũ là còn tồn tại. |
| **5 — Đào thoát** | QTE ở rừng tre, sau hẻm/chợ | **Đồng hồ QTE vẫn chạy khi pause** theo code; còn có nguy cơ trả timeScale khi menu pause đang mở. Đây là lỗi logic ưu tiên cao. Với góc chạy: cần thử ngã rẽ gấp và vật cản khi stamina thấp. | Lỗi logic từ code; chưa tái hiện bằng chơi đoạn rừng. |
| **6 — Kim Trọng** | Đồng hành trên phố, ba nhân chứng | Follower không có vị trí lệch bên cố định, có nguy cơ đứng giữa Kiều và camera. Nên đặt chàng sang một bên và kiểm tra chạy–dừng–quay 180°. Khi đưa giấy tờ sai, phản hồi cần rõ vì sai hai lần làm mất lời khai. | Rủi ro camera từ code; ảnh cũ từng cho thấy che nhân vật, chưa xác nhận bản mới. |
| **7 — Nghịch mệnh** | Q hỏi vặn, lời khai, bài lục bát trước phán quyết | **Pause không bảo toàn thời gian bài lục bát**; lời khai tự chạy cũng có thể tiếp tục sau menu. Nên sửa trước khi cân bằng độ khó. Cần giải thích vì sao bằng chứng đúng/sai để người chơi học được quy tắc. | Hai vấn đề pause từ code; chưa chơi lại phiên tòa. |
| **8 — Trở lại lớp học** | Đọc sách và kết thúc | Nên cho thấy ngắn gọn lựa chọn nào đã ảnh hưởng kết cục, không chỉ báo thắng/thua. Kiểm tra lại camera lớp học và nút đọc sách sau khi xử lý lỗi mở đầu. | Đề xuất; chưa chơi lại kết. |

## Lỗi và cách tái hiện

### R01 — Mở đầu: màn đen, không tiếp được lời thoại

**Đã quan sát, chặn lượt thử này.** Menu mở bình thường. Vào chapter 0, câu “Cha và em bị bắt oan, cần bốn trăm lạng...” hiện trên nền đen. Gửi Space không chuyển câu; Console ghi `NullReferenceException` tại `DialogueManager.AdvanceInk`, dòng 272 lúc thử, gọi từ `StoryDialogueView.Update`.

- Bằng chứng: [ảnh C0](Captures/C0-start.png), [Console](RuntimeErrors-All.json). Có 40 bản ghi cùng stack, **không phải 40 lỗi khác nhau**.
- Trong lúc kiểm tra, fade của ShellCanvas và StoryCanvas đều có CanvasGroup alpha 0; chưa xác định nguyên nhân nền đen. Không kết luận do fade chỉ từ màu Image.
- Cuối lượt, working tree đã có người thêm `if (InkStory == null) return;` vào `AdvanceInk`. Chưa kiểm tra lại. Chặn exception chưa chứng minh câu chuyện tiếp tục được khi InkStory bị mất.
- **Cần thử lại:** ngừng sửa code, thoát/Play lại từ MainMenu, Chơi mới, Space tại câu đầu; kiểm tra chuyện tiếp tục và hình cảnh. Nếu chỉ xảy ra sau recompile khi đang Play, phân loại lỗi phục hồi Editor thay vì lỗi của player build.

### R02 — Chapter 5: pause vẫn làm hết giờ QTE

**Xác định từ code; chưa tái hiện live.** `ForestRunSequence.cs:157` cộng `Time.unscaledDeltaTime`; nhánh `IsPaused` ở dòng 165 chỉ chặn nhận phím. Sau Prompt, dòng 92 phục hồi `Time.timeScale` ngay cả khi pause có thể còn mở.

- Thử: tới QTE rừng → Esc → đợi hơn cửa sổ QTE → Resume. Quan sát bị tính sai, gameplay chạy sau menu hoặc tốc độ chậm bất thường.
- Nên sửa: thời gian QTE chỉ giảm khi không pause; phối hợp khôi phục tốc độ với `PauseMenuView`, không tự ghi đè trạng thái pause.

### R03 — Chapter 7: bài lục bát hết giờ sau khi pause

**Xác định từ code; chưa tái hiện live.** `LucBatSequence.cs:45` đặt deadline bằng `Time.unscaledTime`; dòng 107 bỏ xử lý khi pause nhưng deadline vẫn trôi. `LucBatCatalog` đặt giới hạn 20 giây cho bài cuối chương 7.

- Thử: mở bài lục bát → Esc trên 20 giây → Resume; kiểm tra bị tự hết giờ.
- Nên sửa: dùng thời gian còn lại chỉ giảm khi hoạt động hoặc cộng bù thời gian pause vào deadline.

### R04 — Chapter 7: lời khai tiếp tục khi pause

**Xác định từ code; chưa tái hiện live.** `TrialSequence.cs:340` chạy thời gian `Speak` bằng `unscaledDeltaTime`; kiểm tra pause tại dòng 352 chỉ chặn phím skip.

- Thử: Q hỏi vặn → Esc lúc NPC đang nói → đợi trên 7 giây → Resume; kiểm tra có bỏ lỡ câu.
- Nên sửa: tạm dừng cả tiến trình lời nói khi menu pause mở.

## Camera và lag: chưa có số đo đáng tin cho bản hiện tại

- **Chapter 6:** `Follower.cs:37,51` đặt mục tiêu phía sau người chơi, không tính vị trí camera. Kiểm tra che nhân vật trước khi quyết định offset bên trái/phải hoặc xử lý vật che camera.
- **Camera dùng chung:** `ThirdPersonCameraController.cs:310–311` kẹp delta chuột ±30 mỗi frame. Khi FPS thấp, rê nhanh có thể quay ít hơn mong đợi. So sánh cùng thao tác tại 30/60 FPS trước khi sửa độ nhạy.
- **Tải cảnh:** chapter 2/5/6/7 gọi `BuildNavMesh()` đồng bộ trong khởi tạo. Đây là nơi cần profile nếu có khựng lúc vào cảnh; chưa chứng minh gây lag trong lúc đi lại.
- **Khám phá đông NPC:** `PlayerInteraction.cs:111,158,180` dùng `OverlapSphere`/`RaycastAll` tạo mảng. Cần đo GC.Alloc rồi mới đổi sang NonAlloc nếu có tác động thực tế.
- Log input của lượt này có frame dài ngay sau lệnh MCP biên dịch/execute C#. Không lấy các frame đó làm bằng chứng lag của game. Chưa có kết quả FPS hoặc player build.

## Việc cần làm tiếp

1. Tái hiện R01 trên phiên Play sạch và bản code không thay đổi giữa chừng.
2. Sửa và thử pause/resume tại QTE, lục bát, lời khai: R02–R04.
3. Chơi mẫu camera chapter 6 và hai scene lầu mới, sau đó đo hiệu năng không kèm screenshot/execute-code liên tục.

Save trước thử nằm ở `SaveBackup/` (hai file Ink và các khóa tiến độ). **Chưa xác nhận khôi phục save/prefs hoặc thoát Play Mode** vì lệnh MCP cuối bị chặn hạn mức. Không xóa save hoặc thay thế các chỉnh sửa đang diễn ra. Lượt này chưa commit/push.
