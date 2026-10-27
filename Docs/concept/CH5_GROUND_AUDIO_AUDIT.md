# Ch5 — kiểm tra nền và chọn âm thanh, 25/10/2026

## Đã kiểm tra trực tiếp

- Unity đang mở `Chapter05_Escape` của project hiện tại; đã vào Play Mode để xem một đoạn đường rừng rồi dừng. Không chơi hết tuyến, không ghi clip/xuất ảnh, không chỉnh code hoặc scene.
- Scene lưu `_floor` GUID `0f5e9d870129cec449171c55323a653a` và `_path` GUID `bd2528541e7383c4fb2efdb38361f197`, tương ứng `C5F_Floor.mat`/`C5F_Path.mat`.
- `Chapter05Forest.Props.cs:660–663` tạo cả hai material bằng bộ `brown_mud_02`. Đã xem ảnh gốc `Assets/_Game/Art/Environment/Textures/brown_mud_02/brown_mud_02_diff_1k.jpg`; BaseMap GUID `d3801d144ca006d4c91e1ab2f81b4d9c` khớp material. Đây là texture đất sẫm có hạt/cục gồ ghề, không phải một mặt đá lát. Khung game thấy đường thô giống đất lẫn đá nhỏ, không phải vũng bùn lún. Chỉ từ ảnh không thể khẳng định từng hạt là đá hay cục đất.
- `ForestGround.cs:360–365`: các ô near ground đều được `SurfaceTag.Add(go, Surface.Mud)`; lớp path strip vẽ bằng `_path`. Collider thuộc ô nền dưới, nên đổi màu/texture strip không tự đổi âm dưới chân.
- `FirstPersonFootsteps.FloorAt()` raycast lấy SurfaceTag của collider; `Surface.Mud` chọn nhóm step_mud. Cầu dùng SurfaceTag mặc định Wood. Landing trong `BambooChaseSequence` còn gọi cứng step_mud/stumble.

## Hướng âm phù hợp

Tiếng chính: lực chân chạm đường đất chắc, thêm vừa đủ lớp hạt/sỏi ở vùng có vẻ lẫn đá nhỏ. Đá thật có mặt tiếp xúc riêng mới dùng âm đá cứng. Không dùng tiếng bê tông/gót vang làm mặc định cho cả rừng, không dùng bùn hút hoặc nước tóe vì trời mưa. Cần sửa gán nền và landing cùng việc chọn clip. Giữ nhịp bước theo di chuyển thật và không tạo thêm tiếng chân khi đang bay.

## Nguồn và trạng thái

1. **congusbongus — Footsteps on different surfaces**: https://opengameart.org/content/footsteps-on-different-surfaces . Trang gói CC BY 3.0, archive `footsteps_0.zip` 414.824 byte tải thành công. Đã đọc `footsteps/gravel/license.txt`: derived from Gravel Footsteps by Ali_6868, https://freesound.org/people/Ali_6868/packs/21608/ , CC0. Đã tách 10 take gravel vào `FoleyDownloads/Ch5SurfaceCandidates/congusbongus-gravel/`; giữ license.txt. Đây là ứng viên cần nghe, chưa xác nhận hợp tai hoặc cùng giày Kiều. Khi dùng bản biên tập, giữ credit congusbongus (CC BY 3.0), nguồn gốc Ali_6868 (CC0) và ghi chỉnh sửa. Nhóm grass có nguồn/giấy phép khác, không dùng thay đất một cách mặc định.
2. **TinyWorlds — Different steps on wood, stone, leaves, gravel and mud**: https://opengameart.org/content/different-steps-on-wood-stone-leaves-gravel-and-mud . CC0, tác giả ghi chỉnh sửa bản thu từ pdsounds.org. Archive 77.711 byte đã tải. Tách `gravel.ogg` và `stone01.ogg` vào `FoleyDownloads/Ch5SurfaceCandidates/tinyworlds/`. Chỉ 1 take gravel và 1 take stone nên không đủ làm bộ chạy chính ít lặp; giữ như đối chiếu chất liệu. Không lấy mud/leaves làm mặc định.
3. **Joseph SARDIN & Axeline T. — Footsteps on Gravels #4**: https://bigsoundbank.com/pas-sur-graviers-4-s3216.html . CC0, mô tả bước chân trên đường núi thu POV, 24 giây, stereo 48 kHz/24 bit. Phù hợp để audition đường đất lẫn đá nhỏ; metadata không xác nhận là chạy. Trang web đọc được, tải bằng công cụ gặp HTTP 403 nên chưa có file địa phương; không thử vượt kiểm soát truy cập.
4. **Joseph SARDIN — Jumping on dirt road #1**: https://bigsoundbank.com/jumping-on-dirt-road-1-s1837.html . CC0, bản thu nhảy trên đường đất 2 giây, mono 48 kHz/24 bit. Chưa tải được do HTTP 403. Khi có file cần tách cú đạp và đáp, không phát cả hai cùng lúc.

Không chọn heyheythere Footsteps SFX Free cho yêu cầu bản thu tự nhiên lần này: tác giả công bố âm được tổng hợp bằng code với hỗ trợ AI; nhãn dirt/gravel không chứng minh là field recording.

Nguồn đất bổ sung: **SecureSubset — Footsteps - Dirt, Grass, Ground, Nature**, https://freesound.org/people/SecureSubset/sounds/813621/ . Trang tác giả xác nhận Foley dirt thu bằng MKH416 trong hố cát và vỏ pasta, CC0, 3,733 giây mono 48 kHz/24 bit. Tên có chữ grass nhưng mô tả là vật liệu mô phỏng đất; không nên suy cách dùng từ tên. Có thể audition làm lớp chạm đất nền; chưa nghe nên chưa kết luận độ tự nhiên. Bản gốc cần đăng nhập, chưa tải và không vượt yêu cầu đăng nhập.

## Bàn giao

- Chỉ tải ứng viên và cập nhật tài liệu; chưa import vào Assets hoặc đổi gameplay. Đã giữ archive gốc ngoài project.
- Bản nghe đất/đá 7,5 giây đã gửi trước là âm Kenney đang có, không phải các nguồn mới vừa tìm và không đại diện đúng nhóm Mud hiện được gán cho đường rừng.
- Chưa nghe bằng tai các ứng viên mới; không coi việc tải/đọc giấy phép là duyệt chất lượng âm thanh.

## Quyết định của người chơi

Người chơi đã nghe `audio-preview/candidate-gravel-run-preview.wav` và trả lời “ổn”. Chốt nguồn gravel Ali_6868/congusbongus theo chất âm của bản preview; chưa gắn vào game. Đây là đánh giá bằng tai của người chơi, không phải Codex nghe duyệt. Mix trong game, lớp đất và các hiệu ứng nhảy/đáp vẫn cần kiểm tra riêng. Dùng các take riêng theo sự kiện bước chân, không loop file preview.
