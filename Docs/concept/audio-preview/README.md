# Ch5 — nghe thử bước chạy đất và đá

Ghép từ take đang có trong project; không phải ghi âm Play mode hoặc bộ âm mới đã triển khai.

- Earth: step_earth_01, 04, 02, 09; gain gốc 1.0.
- Stone: step_stone_05, 01; gain gốc 1.44 theo FoleyBuild.txt. Đây là bản thu concrete hiện được dùng cho stone, cần nghe đánh giá mức phù hợp với đá trong rừng.
- Cả hai giảm chung gain 0.65 để tránh clipping khi nghe riêng ngoài game.
- Mỗi file 7,5 giây, 24 bước; khoảng bước 0,27 giây (5 m/s, stride 1,35 m). Không pitch random, spatial attenuation, mưa, nhạc hoặc hơi thở.
- Nguồn: Kenney RPG Audio (earth), Kenney Impact Sounds (stone), CC0 theo Assets/_Game/Audio/Ch4Foley/CREDITS.md.
- Không thay đổi gameplay hoặc asset âm thanh trong Assets.

## Ứng viên mới sau khi kiểm tra nền thật

- `candidate-gravel-ali6868-00.ogg`: một take gốc `footsteps/gravel/0.ogg`, tải từ bản biên tập của congusbongus trên OpenGameArt. Trang gói CC BY 3.0; license riêng nhóm gravel ghi nguồn Ali_6868 CC0. Giữ cả credit người biên tập và nguồn gốc khi dùng. Không đổi gain/pitch, không ghép thành chạy.
- `candidate-stone-tinyworlds.ogg`: take `stone01.ogg` trong gói TinyWorlds, CC0, không xử lý. Đối chiếu cú chạm đá cứng, không phải âm mặc định cho đường đất.
- Đây là file ứng viên ngắn để nghe riêng; chưa import vào game, chưa được nghe duyệt. Xem `../CH5_GROUND_AUDIO_AUDIT.md` để biết kết quả kiểm tra nền, đầy đủ nguồn và giới hạn.

## Bản WAV thay cho OGG không phát được trong chat

- `candidate-gravel-run-preview.wav`: giải mã 10 take gravel của Ali_6868 trong bản biên tập congusbongus bằng thư viện Vorbis có sẵn của Unity; không cài phần mềm mới. Ghép 24 bước cách 0,27 giây trong 7,5 giây; mono PCM 16 bit 44,1 kHz. Cắt đuôi tối đa 0,32 giây/take, fade 8 ms, chỉnh RMS đoạn đầu về khoảng −18 dBFS với giới hạn peak 0,75. Không đổi pitch. Đây là dựng nhịp chạy để nghe chất liệu, không phải bản thu chạy liên tục hoặc ghi âm trong game. Không thay file OGG gốc và không import vào Assets.
