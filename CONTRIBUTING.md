# Quy trình làm việc nhóm

Đọc hết trang này **một lần** trước khi commit lần đầu. Mọi quy tắc ở đây đều nhằm một mục tiêu: 6 người làm song song mà không phá file của nhau.

---

## 1. Mô hình nhánh

```
main            Bản ổn định, nộp và demo. Chỉ cập nhật cuối sprint qua PR dev → main.
 └ dev          Nhánh tích hợp chung. Chỉ nhận code qua Pull Request đã review.
    ├ thuanDev     TV1 · Ngô Trần Gia Thuận
    ├ quocanhDev   TV2 · Lê Ngọc Quốc Anh
    ├ hungDev      TV3 · Nguyễn Đỗ Bảo Hưng
    ├ triDev       TV4 · Nguyễn Minh Trí
    ├ ngoctriDev   TV5 · Nguyễn Lê Ngọc Trí
    └ hoangDev     TV6 · Hoàng Thái Hoàng
```

- `main` và `dev` **bị khóa**: không ai push thẳng được, kể cả Leader.
- Mỗi người **chỉ push lên nhánh `<tên>Dev` của mình**.
- Không merge nhánh của người khác vào nhánh của mình. Mọi thứ đi qua `dev`.

---

## 2. Một ngày làm việc (làm đúng theo thứ tự)

Các ví dụ dưới đây dùng `triDev`. Hãy thay bằng nhánh của bạn.

### Bước 1: Mở đầu buổi: lấy code mới nhất của nhóm
```bash
git checkout triDev
git pull origin dev          # kéo phần người khác đã merge vào dev
```
> Làm bước này **khi Unity đang đóng** (hoặc ít nhất không mở scene sắp bị cập nhật).
> Nếu có conflict, xem mục 5.

### Bước 2: Làm việc trong Unity
- **Save scene** (Ctrl+S) và *File → Save Project* trước khi commit.
- Chỉ sửa scene mà bạn đang giữ (xem mục 4).

### Bước 3: Commit
```bash
git status                   # xem đã đổi những gì: có file lạ nào không?
git add -A
git commit -m "[#12] Add PlayerController run + input lock"
```
- **Định dạng:** `[#STT] Động từ tiếng Anh + nội dung`. STT là số thứ tự task trong `Docs/QuanLyCongViec_v2.xlsx` (sheet Checklist, cột B).
- Commit nhỏ, thường xuyên: mỗi commit một ý.
- **Luôn có file `.meta` đi kèm** asset mới.

### Bước 4: Đẩy lên GitHub
```bash
git push                     # lần đầu: git push -u origin triDev
```

### Bước 5: Mở Pull Request khi xong một phần việc
1. Vào trang repo trên GitHub → **Pull requests → New pull request**.
2. Chọn **base: `dev`** ← **compare: `triDev`**.
3. Điền template (tự hiện ra), gắn người review:
   - Code C#: **Quốc Anh** (TV2)
   - Nội dung, Ink, scene: **Gia Thuận** (TV1)
4. Người review kiểm tra rồi Approve, sau đó bấm **Squash and merge**.
5. Sau khi merge, quay lại Bước 1 để nhánh của bạn cập nhật lại từ `dev`.

---

## 3. Checklist trước khi mở Pull Request

- [ ] Đã `git pull origin dev` ngay trước khi mở PR, và project vẫn mở được.
- [ ] Console **không có lỗi đỏ** khi Play scene liên quan.
- [ ] *Window → General → Test Runner → EditMode → Run All*: tất cả xanh.
- [ ] Không commit file trong `Library/ Temp/ Logs/ UserSettings/ Build/` (`.gitignore` đã chặn, nhưng cứ kiểm tra).
- [ ] Không có asset nằm ngoài `Assets/_Project/`, trừ đồ cá nhân trong `_Sandbox/`.
- [ ] Tên file đúng quy ước (xem README mục 7).

---

## 4. Quy tắc với scene và prefab (quan trọng nhất)

File `.unity` và `.prefab` rất khó merge. Hai người cùng sửa một scene thì gần như chắc chắn một người mất việc.

1. **Mỗi scene chỉ một người sửa tại một thời điểm.** Trước khi mở để sửa, nhắn nhóm: *"Mình giữ KieuHome_Exterior tới 21h"*.
2. Scene `Levels/*` thuộc **TV4 (Minh Trí)**. Người khác muốn đặt đồ vào thì **làm prefab rồi gửi** cho chủ scene đặt vào.
3. Muốn thử nghiệm thì tạo scene riêng trong `Scenes/Sandbox/Sandbox_<TênBạn>.unity`.
4. File lớn (`.fbx`, `.vrm`, `.psd`) thì khóa trước khi sửa:
   ```bash
   git lfs lock Assets/_Project/Characters/ThuyKieu/Models/ThuyKieu.fbx
   git lfs unlock Assets/_Project/Characters/ThuyKieu/Models/ThuyKieu.fbx   # sửa xong
   ```
5. **Không di chuyển hoặc đổi tên asset trong File Explorer.** Chỉ làm trong cửa sổ Project của Unity.

---

## 5. Gặp conflict thì làm gì?

```bash
git pull origin dev
# CONFLICT (content): Merge conflict in ...
git status                   # xem file nào conflict
```
| Loại file | Cách xử lý |
|---|---|
| `.cs` | Mở bằng VS/Rider, sửa phần `<<<<<<< ======= >>>>>>>`, rồi `git add <file>` |
| `.unity` / `.prefab` / `.asset` | **Không sửa tay.** Lấy bản của `dev`: `git checkout --theirs <file>`, rồi làm lại phần của mình. Nếu đã cấu hình UnityYAMLMerge (README 2.3), Git sẽ tự merge phần lớn trường hợp. |
| `.meta` | Thường là lấy bản của `dev` (`--theirs`). Nếu không chắc thì hỏi nhóm. |

Xong thì chạy:
```bash
git add -A
git commit                   # giữ nguyên message merge mặc định
git push
```
Nếu rối quá thì **dừng lại và hỏi nhóm**, đừng `git push --force`.

---

## 6. Báo lỗi và chia việc

- **Lỗi:** tạo Issue với template *Bug report*, ghi rõ bước tái hiện. TV6 (QA) phân loại mức độ.
- **Task:** tạo Issue với template *Task*, ghi STT trong bảng Excel. Khi mở PR, viết `Closes #<số issue>` để Issue tự đóng sau khi merge.

---

## 7. Lệnh hay dùng

| Việc | Lệnh |
|---|---|
| Xem đang ở nhánh nào | `git branch --show-current` |
| Xem lịch sử gọn | `git log --oneline -15` |
| Bỏ thay đổi một file (chưa commit) | `git restore <file>` |
| Cất tạm thay đổi để pull | `git stash` → `git pull origin dev` → `git stash pop` |
| Thiếu model/texture sau khi clone | `git lfs pull` |
| Ai sửa file này gần nhất | `git log -3 -- <file>` |
