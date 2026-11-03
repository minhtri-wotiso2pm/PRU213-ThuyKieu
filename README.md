# Thúy Kiều

Game 3D góc nhìn thứ ba kể lại *Truyện Kiều* từ bên trong trang sách: một tiết Văn cuối chiều, một lời chê Kiều, rồi quyển sách phát sáng — người chơi bước vào truyện và phải sống qua từng biến cố thay vì đọc qua chúng.

Người chơi đi trong không gian 3D của từng chương, nói chuyện bằng những lựa chọn có hệ quả, thu manh mối vào sổ tay, và ở chương cuối tự đứng ra đối chất trước huyện đường. Toàn bộ nội dung là tiếng Việt.

## Các chương

| | Chương | Nội dung |
|---|---|---|
| Mở đầu | Trang sách | Một tiết Văn cuối chiều. Một lời chê Kiều. Rồi quyển sách phát sáng. |
| 1 | Gia biến | Nhà họ Vương gặp nạn. Mã Giám Sinh tới hỏi cưới. |
| 2 | Trên đường theo Mã Giám Sinh | Con đường đất đỏ, và những nghi ngờ đầu tiên. |
| 3 | Tú Bà | Một ngôi nhà trông như mái ấm, vận hành như chiếc lồng. |
| 4 | Không còn đường lui | Đêm ở nhà Tú Bà — tìm lối ra. |
| 5 | Đào thoát | Ngõ hẻm, chợ đêm và cánh rừng phía sau. |
| 6 | Kim Trọng | Chùa Bạch Vân, thư phòng và ba nhân chứng. |
| 7 | Nghịch mệnh | Huyện đường Lâm Tri. Lần này, Kiều tự đứng ra kể chuyện của mình. |
| Kết | Trở lại lớp học | Mười lăm phút sau tiếng chuông tan học. |

Mỗi chương là một scene chơi được từ đầu đến cuối. Chương 7 có ba vòng đối chất: chọn lời khai, hỏi vặn, đưa bằng chứng, giữ lòng tin và kiên nhẫn của quan huyện, và giữ chân các nhân chứng đủ can đảm để nói hết lời.

## Công nghệ

- Unity `6000.6.0f1`, render pipeline URP `17.6.0`
- Input System `1.20.0`, AI Navigation `2.0.14`, Timeline `6.6.0`, uGUI + TextMeshPro
- [Ink](https://www.inklestudios.com/ink/) cho kịch bản hội thoại (runtime nằm trong `Assets/ThirdParty/Ink`)

## Chạy dự án

1. Cài Unity `6000.6.0f1` qua Unity Hub.
2. Add project trỏ vào thư mục này, mở scene `Assets/_Game/Scenes/MainMenu.unity`.
3. Bấm Play. Menu chính có chơi tiếp, chọn chương và thư viện kết thúc.

Không commit `Library/`, `Temp/`, `Logs/` hay các file `*.csproj` sinh tự động.

## Điều khiển

| Phím | Tác dụng |
|---|---|
| `WASD` | Đi |
| `Shift` | Chạy |
| `C` / `Ctrl` | Cúi, đi nhẹ |
| `Space` | Nhảy (chỉ ở chương cho phép) |
| `E` | Tương tác, giữ để làm hành động dài |
| Chuột phải | Quan sát kỹ vật đang nhìn |
| Chuột trái | Hành động giữ (in dấu tay, đẩy cửa) |
| `Q` | Ném sỏi đánh lạc hướng |
| `J` | Sổ tay |
| `B` | Trang Sách |
| `Tab` | Hiện lại mục tiêu |
| `V` | Camera cận mặt nhân vật |
| `Esc` | Tạm dừng, mở khoá chuột |

Trong đối chất Chương 7: `1–4` chọn lời khai, `Q` hỏi vặn, `E` đưa bằng chứng, `Backspace` cất hồ sơ.

## Cấu trúc

```text
Assets/_Game/
├── Data/Dialogue/Ink/   kịch bản Ink (Main.ink + từng chương) và Main.json đã biên dịch
├── Prefabs/UI/          canvas dùng chung cho mọi chương (StoryCanvas.prefab)
├── Scenes/              scene từng chương
├── Scripts/
│   ├── Core/            director, autopilot, editor setup dựng scene
│   ├── Dialogue/        cầu nối Ink, validator
│   ├── Gameplay/        sequence từng chương (đối chất, lục bát, đuổi bắt…)
│   ├── Interaction/     tương tác, manh mối, sổ tay
│   ├── Player/          điều khiển nhân vật
│   ├── Presentation/    camera hội thoại, ánh sáng, hiệu ứng
│   └── UI/              HUD, các bảng (hồ sơ, suy luận, cài đặt…)
└── Art/ Audio/ Characters/ Environment/ Materials/   tài nguyên
```

Scene và canvas được dựng bằng mã, không sửa tay: menu `ThuyKieu/Chapter N/…` dựng lại scene chương, `ThuyKieu/Story/Rebuild shared story canvas` dựng lại prefab UI dùng chung (giữ nguyên GUID).

## Kiểm thử

Unity chạy batch bằng `Unity.exe` trực tiếp (không dùng wrapper tự thêm `-quit`):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -nographics `
  -projectPath . -executeMethod ThuyKieu.Dialogue.Editor.InkStoryValidator.RunBatch
```

Lệnh trên biên dịch `Main.ink`, chạy hàng nghìn lượt đi ngẫu nhiên qua từng chương và ghi `Tools/Ink/InkValidationReport.txt`.

Playtest tự động (bot chơi thật trong Play Mode, có thể ép một kết thúc):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -nographics `
  -projectPath . -executeMethod ThuyKieu.Core.Editor.StoryBuild.PlaytestBatch `
  -playScene Assets/_Game/Scenes/Chapter07_HuyenDuong.unity -storyAutopilot `
  -autopilotEnding nghich_menh -autopilotReport Logs/ch7_report.txt
```

`-autopilotEnding` nhận `nghich_menh`, `thang`, `mat_minh`, `dinh_menh`. Báo cáo ghi từng dòng hội thoại, từng lựa chọn, điểm số và giá trị biến của chương.

## Tài liệu

- `Docs/GDD_KeHoach_ThietKe.md` — thiết kế tổng thể
- `Docs/Chapter07_Choice_Evidence_Matrix.md` — kiểm toán lựa chọn, lời khai và bằng chứng của Chương 7
- `Docs/Chapter07_Design_Insight.md` — vì sao Chương 7 được thiết kế như hiện tại
