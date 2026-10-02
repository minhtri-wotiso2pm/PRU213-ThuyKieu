# Thúy Kiều: Kiếm tiền có gì khó nhỉ?

> Game phiêu lưu kể chuyện góc nhìn thứ 3, phỏng theo *Truyện Kiều*: Kiều tìm cách kiếm **400 lạng** để cứu cha và em, bằng trí tuệ và lựa chọn của chính mình, không phải bán thân.
> Đồ án môn **PRU213**, nhóm 6 người, hạn nộp **30/10/2026**.

| | |
|---|---|
| **Engine** | Unity **6000.6.0f1** (Unity 6) · URP · Input System · uGUI + TextMeshPro |
| **Nền tảng** | Windows + WebGL |
| **Kịch bản** | Ink ([`Docs/ThuyKieu_KichBan_v2.ink`](Docs/ThuyKieu_KichBan_v2.ink)) |
| **Thiết kế chi tiết** | [`Docs/GDD_KeHoach_ThietKe.md`](Docs/GDD_KeHoach_ThietKe.md) |
| **Quy trình đóng góp** | [`CONTRIBUTING.md`](CONTRIBUTING.md) (**đọc trước khi code**) |

---

## Mục lục
1. [Giới thiệu game](#1-giới-thiệu-game)
2. [Bắt đầu nhanh (cài đặt lần đầu)](#2-bắt-đầu-nhanh-cài-đặt-lần-đầu)
3. [Hiện trạng: đã có gì](#3-hiện-trạng-đã-có-gì)
4. [Sẽ triển khai: lộ trình](#4-sẽ-triển-khai-lộ-trình)
5. [Cấu trúc thư mục](#5-cấu-trúc-thư-mục)
6. [Kiến trúc code](#6-kiến-trúc-code)
7. [Quy ước đặt tên & code](#7-quy-ước-đặt-tên--code)
8. [Làm việc nhóm với Git (tóm tắt)](#8-làm-việc-nhóm-với-git-tóm-tắt)
9. [Thành viên & phân công](#9-thành-viên--phân-công)
10. [Câu hỏi thường gặp](#10-câu-hỏi-thường-gặp)

---

## 1. Giới thiệu game

**Vòng lặp chơi:** Cinematic mở cảnh → Hội thoại & lựa chọn (Ink) → Khám phá, tương tác, mini-game → Hệ quả (chỉ số, bằng chứng) → Sang chương mới.

| Chương | Địa điểm | Gameplay chính |
|---|---|---|
| 1. Gia biến | Vương gia (đêm mưa) | Cinematic + hội thoại, tra hỏi Mã Giám Sinh, đọc/ký khế |
| 2. Cái giá của một con người | Lầu Tú Bà, phố đêm | Khám phá (chọn 2/3 điểm) → **chạy trốn** qua ngõ chợ |
| 3. Kiếm tiền có gì khó? | Thị trấn | Chọn 2/3 việc + mini-game: **Đàn tranh**, **Viết thư**, **Thương lượng** |
| 4. Không ai được định giá ta | Vương gia (bình minh) | Đối đầu cao trào, 3 kết thúc (bright / warm / reflective) |

**Chỉ số** (lưu trong biến Ink): Tiền · Danh tiếng · Tự trọng · Tỉnh táo, cùng bằng chứng và manh mối trong Sổ tay.

**Điều khiển:** WASD di chuyển · Shift chạy · Chuột xoay camera · **E** tương tác · Space/Enter tiếp thoại · 1–9 chọn đáp án · Space bỏ qua cutscene · Esc mở/khóa chuột.

---

## 2. Bắt đầu nhanh (cài đặt lần đầu)

### 2.1 Cài công cụ
| Công cụ | Ghi chú |
|---|---|
| **Unity Hub** + **Unity 6000.6.0f1** | Phải đúng phiên bản này. Khác phiên bản sẽ làm hỏng scene và tạo conflict. Lúc cài, tick thêm *Windows Build Support* và *WebGL Build Support*. |
| **Git** ≥ 2.40 | https://git-scm.com |
| **Git LFS** | `git lfs install` (chạy **một lần** trên mỗi máy, trước khi clone) |
| Visual Studio 2022 hoặc Rider | Có workload *Game development with Unity* |
| (Tùy chọn) GitHub Desktop | Nếu chưa quen dòng lệnh |
| (Tùy chọn) [Inky](https://github.com/inkle/inky/releases) | Đọc và chạy thử kịch bản `.ink` |

### 2.2 Clone & mở project
```bash
git lfs install
git clone https://github.com/minhtri-wotiso2pm/PRU213-ThuyKieu.git
cd PRU213-ThuyKieu
git checkout <tên>Dev        # nhánh riêng của bạn, ví dụ triDev (xem mục 8)
```
1. Mở **Unity Hub** → *Add* → *Add project from disk* → chọn thư mục vừa clone.
2. Lần đầu mở sẽ mất 5–15 phút để import. **Đừng tắt giữa chừng.**
3. Menu **ThuyKieu → Scenes → KieuHome Interior** → bấm **Play**. Bạn sẽ thấy cutscene Kiều tỉnh dậy (Space để bỏ qua) rồi điều khiển được nhân vật.

### 2.3 Cấu hình Git một lần (quan trọng)
```bash
git config user.name  "Họ Tên"
git config user.email "email@dùng-cho-github"
```
Bật công cụ merge scene của Unity (giảm conflict file `.unity` và `.prefab`). Thay đường dẫn cho đúng chỗ cài Unity trên máy bạn:
```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

### 2.4 Kiểm tra trong Unity (Edit → Project Settings)
- *Editor → Asset Serialization*: **Force Text**
- *Version Control → Mode*: **Visible Meta Files**

---

## 3. Hiện trạng: đã có gì

Cập nhật: **02/10/2026**, cuối Sprint 0.

### ✅ Đã hoàn thành và chạy được
| Hệ thống | File chính | Mô tả |
|---|---|---|
| **Bootstrap** | `Core/Bootstrap/PersistentSystems.cs` | Tự nạp `Resources/Systems/PF_GameSystems` trước scene đầu tiên. Mở scene nào để Play cũng được, và manager không bị mất khi đổi scene. |
| **Input** | `Core/InputReader.cs`, `Core/InputLock.cs` | Đọc input tập trung. `InputLock.Push/Pop(owner)` khóa player khi hội thoại, mini-game hoặc menu đang chạy. |
| **Chuyển scene** | `Core/SceneTransition.cs`, `Core/PlayerSpawner.cs` | Cửa đưa player sang scene khác và đặt vào đúng spawn point. |
| **Player** | `Player/PlayerMovement.cs` | CharacterController, đi theo hướng camera, Shift để chạy, có trọng lực. |
| **Camera** | `Player/ThirdPersonCameraController.cs` | Camera xoay quanh nhân vật. Có API để cutscene trả quyền điều khiển mượt mà. |
| **Tương tác** | `Interaction/PlayerInteraction.cs`, `IInteractable` | SphereCast phía trước, bấm E. Hiện prompt `[E] …` (`UI/InteractionPromptUI.cs`). |
| **Hội thoại** | `Dialogue/DialogueManager.cs`, `DialogueUI.cs`, `DialogueData` (SO) | Thoại theo node, rẽ nhánh lựa chọn, phát event cho UI. NPC đổi lời thoại theo trạng thái quest. |
| **Quest** | `Quest/QuestManager.cs`, `QuestData` (SO) | Mục tiêu TalkToNPC và InteractWithObject. Trạng thái giữ nguyên qua các scene. |
| **Cutscene mở đầu** | `Cutscene/WakeUpCutscene.cs`, `CinematicTextUI.cs` | Kiều tỉnh dậy: góc nhìn thứ nhất, mí mắt, blur, chuyển sang góc thứ 3. Có tên game, title card chương, phụ đề typewriter, letterbox. |
| **Map** | `Scenes/Levels/KieuHome_Interior`, `KieuHome_Exterior` | Phòng Kiều và sân Vương gia (nhà, tre, chuối, giếng, hàng rào, đồ nội thất). |
| **Test** | `Scripts/Tests/EditMode/` | 4 test cho Quest. Chạy trong *Window → General → Test Runner*. |
| **Editor tool** | `Scripts/Editor/SceneMenu.cs` | Menu **ThuyKieu → Scenes** để mở nhanh từng scene. |

### ⚠️ Đang tạm, sẽ thay
- `DialogueData` và `QuestData` là dữ liệu **test** (`TestFather`, `FindTheLetter`). Nội dung thật sẽ lấy từ Ink.
- `DialogueUI` và `InteractionPromptUI` đang dùng `UI.Text` cũ, sẽ chuyển sang TextMeshPro.
- Nhân vật NPC là placeholder. Chưa có Animator.
- Font Palatino thiếu ký tự `◆` ở màn tiêu đề, gây warning trong Console.

---

## 4. Sẽ triển khai: lộ trình

| Sprint | Thời gian | Mục tiêu | Demo cuối sprint |
|---|---|---|---|
| **0: Chuyển sang Unity** ✅ | 30/09 – 02/10 | Project, cấu trúc, kiến trúc, repo | 6 máy mở được project |
| **1: Hệ thống lõi** | 03/10 – 09/10 | Ink + Tag, Dialogue UI (TMP), GameState, Player animation, map C1 | Chơi hết Chương 1 bằng UI thật + build WebGL thử |
| **2: Nội dung C1–C2** | 10/10 – 16/10 | Nhân vật VRM, biểu cảm, Cinemachine, map C2–C3, HUD, Sổ tay, Audio, chạy trốn, mini-game Đàn | **Alpha:** Chương 1–2 chơi trọn |
| **3: Nội dung C3–C4 + Menu** | 17/10 – 23/10 | C3, C4, ending, Main Menu, Pause, Save, tutorial, lighting | **Beta:** New Game → kết thúc |
| **4: Polish & phát hành** | 24/10 – 30/10 | QA, tối ưu, playtest, build, video, slide | **Release 30/10** |

**Mốc khóa tính năng (feature freeze): 20/10.** Sau mốc này chỉ sửa lỗi và đánh bóng.

### Hệ thống sắp làm (tham khảo `Docs/GDD_KeHoach_ThietKe.md` mục 4)
| Hệ thống | Thư mục code | Mô tả ngắn |
|---|---|---|
| `InkStoryService`, `DialogueController`, `TagDispatcher` | `Scripts/Runtime/Narrative/` | Đọc Ink, máy trạng thái hội thoại, xử lý tag `# speaker:`, `# camera:`, `# music:`… |
| `GameStateService` + HUD | `Narrative/`, `UI/` | Theo dõi biến Ink (tiền, danh tiếng…) và cập nhật HUD |
| `ChapterManager` / `SceneDirector` | `Core/` | Title card, chuyển địa điểm theo tag `# scene:` |
| `CameraDirector`, `ExpressionController` | `Cutscene/` hoặc `Presentation/` | Cinemachine theo id, biểu cảm VRM |
| `AudioManager` | `Audio/` | Nhạc, SFX, ambience, Mixer |
| `MiniGameManager` + Đàn tranh | `MiniGames/` | Mini-game nhịp điệu, trả kết quả về Ink |
| `EscapeSequence` | `Gameplay/` | Chạy trốn dùng NavMesh, có checkpoint |
| `SaveSystem` | `Save/` | JSON ở `persistentDataPath` |

**Package sẽ cài** (cả nhóm thống nhất, một người cài rồi mở PR): `ink-unity-integration`, `Cinemachine 3`, `UniVRM`, `ProBuilder`.

---

## 5. Cấu trúc thư mục

```
├── Assets/
│   ├── _Project/                 ← MỌI THỨ CỦA NHÓM NẰM Ở ĐÂY
│   │   ├── Art/                  Environment/ (fbx + texture) · Materials/ (Environment, Shared) · Textures · VFX · Shaders
│   │   ├── Characters/<Tên>/     Models/ · Animations/      (_Shared/ cho đồ dùng chung)
│   │   ├── Audio/                Music · SFX · Ambience · Mixer
│   │   ├── Narrative/Ink/        ThuyKieu.ink: kịch bản dùng trong game
│   │   ├── Data/                 ScriptableObject: Dialogue · Quests · Characters · Scenes · Audio · Evidence
│   │   ├── Prefabs/              Characters · Environment · Gameplay · UI · VFX      (tên PF_*)
│   │   ├── Resources/Systems/    PF_GameSystems: KHÔNG đổi tên hoặc đường dẫn (bootstrap đọc theo path)
│   │   ├── Scenes/               Levels/ (scene thật, có trong Build) · Menus/ · Sandbox/ (thử nghiệm)
│   │   ├── Scripts/
│   │   │   ├── Runtime/          Assembly ThuyKieu.Runtime: thư mục = namespace
│   │   │   ├── Editor/           Assembly ThuyKieu.Editor: tool chỉ chạy trong Editor
│   │   │   └── Tests/EditMode/   Assembly ThuyKieu.Tests.EditMode
│   │   ├── Settings/             Input/ · Rendering/ (URP)
│   │   ├── Timeline/
│   │   └── UI/                   Fonts · Sprites · Prefabs
│   ├── _Sandbox/                 Đồ cá nhân, file recovery. _Project không được tham chiếu tới đây
│   └── TextMesh Pro/             Resource của package, không sửa
├── Docs/                         GDD, kịch bản Ink, bảng công việc, tool test Ink
├── Packages/ · ProjectSettings/  Cấu hình Unity (commit đầy đủ)
├── .github/                      Template Pull Request và Issue
├── CONTRIBUTING.md               Quy trình làm việc chi tiết
└── README.md
```
Thư mục trống có file `.gitkeep` để Git giữ lại. Xóa file đó khi thư mục đã có nội dung.

---

## 6. Kiến trúc code

```
Khởi động game (bất kỳ scene nào)
  └─ PersistentSystems.Boot() → nạp PF_GameSystems (DontDestroyOnLoad)
       ├─ DialogueManager          hội thoại, phát event, gọi InputLock khi đang thoại
       ├─ QuestManager             trạng thái quest
       ├─ DialogueCursorController mở/khóa chuột theo hội thoại
       └─ PF_DialogueCanvas        DialogueUI

Mỗi scene level có: PF_Player · Main Camera (ThirdPersonCameraController)
                    PlayerSpawner + spawn point · EventSystem · PromptCanvas

PlayerInteraction ──E──► IInteractable.Interact()
   ├─ NPCInteractable          → DialogueManager → (xong) → QuestManager.StartQuest / ReportProgress
   ├─ QuestObjectInteractable  → QuestManager.ReportProgress
   └─ SceneDoorInteractable    → SceneTransition.Travel → PlayerSpawner ở scene mới
```

**Nguyên tắc:**
- Mỗi class chỉ làm **một việc**. Các module nói chuyện với nhau qua **interface hoặc event**, không gọi chéo trực tiếp.
- **Manager toàn cục mới** (Audio, Save, GameState…) thì thêm component vào `PF_GameSystems`. Không đặt manager vào từng scene.
- **Cần khóa điều khiển** thì gọi `InputLock.Push(this)`, xong việc gọi `InputLock.Pop(this)`.
- **Vật tương tác mới** thì implement `IInteractable` (và `IInteractableDisplayName` nếu cần prompt). Không sửa `PlayerInteraction`.

---

## 7. Quy ước đặt tên & code

| Loại | Mẫu | Ví dụ |
|---|---|---|
| Prefab | `PF_<Tên>` · `PF_NPC_<Tên>` | `PF_Bed`, `PF_NPC_VuongOng` |
| Material | `M_<Tên>` | `M_RoofTile` |
| Texture | `T_<Tên>_<Map>` | `T_Bed_Albedo` |
| Animation / Animator | `A_<NV>_<Hành động>` · `AC_<NV>` | `A_Kieu_Walk`, `AC_Kieu` |
| Âm thanh | `MUS_` · `SFX_` · `AMB_` | `SFX_DoorOpen` |
| ScriptableObject | `Dialogue_<NPC>_<Nhánh>` · `Quest_<Tên>` | `Quest_FindTheLetter` |
| Scene | `<Địa điểm>_<Khu vực>` | `KieuHome_Interior` |
| Script | PascalCase, 1 class/file, tên file = tên class | `PlayerMovement.cs` |

**Code C#:**
1. Namespace `ThuyKieu.<TênThưMục>`.
2. Field Inspector dùng `[SerializeField] private`. `Find…` chỉ là phương án dự phòng.
3. Đọc input qua `ThuyKieu.Core.InputReader`, không dùng `Keyboard.current` trong gameplay.
4. Project **tắt Domain Reload**: mọi biến `static` phải reset trong `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.
5. Không đổi tên class MonoBehaviour hoặc ScriptableObject khi chưa báo nhóm, vì scene và prefab sẽ mất tham chiếu.

**Asset:**
- Di chuyển hoặc đổi tên **chỉ trong Unity** (cửa sổ Project), **không** làm trong File Explorer. Nếu làm ngoài Explorer, file `.meta` (chứa GUID) sẽ lệch và prefab bị mất model.
- Luôn commit file `.meta` đi kèm.

---

## 8. Làm việc nhóm với Git (tóm tắt)

Chi tiết từng lệnh có trong [`CONTRIBUTING.md`](CONTRIBUTING.md).

```
main            ← bản ổn định, chỉ cập nhật cuối mỗi sprint (đã khóa, chỉ merge qua PR)
 └ dev          ← nhánh tích hợp chung của cả nhóm (đã khóa, chỉ merge qua PR có review)
    ├ thuanDev     (TV1 Gia Thuận)
    ├ quocanhDev   (TV2 Quốc Anh)
    ├ hungDev      (TV3 Bảo Hưng)
    ├ triDev       (TV4 Minh Trí)
    ├ ngoctriDev   (TV5 Ngọc Trí)
    └ hoangDev     (TV6 Thái Hoàng)
```
Mỗi người **chỉ làm trên nhánh `<tên>Dev` của mình**.

1. Chuyển sang nhánh của mình: `git checkout triDev`.
2. Trước khi làm, lấy code mới nhất từ `dev`: `git pull origin dev`.
3. Commit nhỏ, thường xuyên: `[#STT] Động từ + nội dung`, rồi `git push`.
4. Xong một phần việc thì mở **Pull Request `triDev` → `dev`**. Cần 1 người review, sau đó merge.
5. Cuối sprint, Leader mở PR `dev` → `main`.
6. **Mỗi scene chỉ một người sửa tại một thời điểm.** Muốn sửa scene của người khác thì báo trong nhóm chat hoặc nhờ chủ scene.

---

## 9. Thành viên & phân công

| TV | Thành viên | Vai trò | Phạm vi chính |
|---|---|---|---|
| TV1 | Ngô Trần Gia Thuận | Leader · Narrative/Ink · Integrator | Ink, ghép chương, thuyết trình |
| TV2 | Lê Ngọc Quốc Anh | Main C# (Core) | Dialogue, Tag, Chapter, GameState, Save, review code |
| TV3 | Nguyễn Đỗ Bảo Hưng | Characters · Cinematic | VRoid/UniVRM, animation, biểu cảm, Cinemachine/Timeline |
| TV4 | Nguyễn Minh Trí | Level · Player/Gameplay | Map, player, interaction, chạy trốn, lighting, tối ưu |
| TV5 | Nguyễn Lê Ngọc Trí | UI/UX | Dialogue UI, HUD, sổ tay, menu, tutorial |
| TV6 | Hoàng Thái Hoàng | QA · Build · Git · Audio · Tools | Git, audio, mini-game đàn, pause/settings, QA, build |

**Chủ sở hữu scene:** `Scenes/Levels/*` thuộc TV4 (Level). Scene Menu thuộc TV5. Bảng task đầy đủ ở [`Docs/QuanLyCongViec_v2.xlsx`](Docs/QuanLyCongViec_v2.xlsx).

---

## 10. Câu hỏi thường gặp

**Mở project bị lỗi đỏ, hoặc texture/model hiện màu hồng hoặc trống?**
Bạn clone khi chưa cài Git LFS. Chạy `git lfs install` rồi `git lfs pull`, sau đó mở lại Unity.

**Prefab báo "Missing Prefab"?**
Có người đã di chuyển asset ngoài Unity, hoặc quên commit `.meta`. Báo người commit gần nhất (xem `git log -- <đường dẫn>`).

**Conflict file `.unity`?**
Đừng tự sửa tay. Giữ bản của chủ scene (`git checkout --theirs <file>` hoặc `--ours`), rồi đặt lại phần thay đổi của mình bằng prefab.

**Chạy thử kịch bản Ink không cần Unity?**
Mở [`Docs/tools/ThuyKieu_InkPlayer.html`](Docs/tools/ThuyKieu_InkPlayer.html) trong trình duyệt, hoặc dùng Inky.

**Kiểm tra mọi nhánh Ink tự động?**
```bash
cd Docs/tools && npm install inkjs && node ink_branch_test.js ../ThuyKieu_KichBan_v2.ink
```
