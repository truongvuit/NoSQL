## Hướng dẫn thao tác với MongoDB Compass

### 1. Truy cập MongoDB Compass
- Mở ứng dụng **MongoDB Compass** trên máy tính.
- Đăng nhập hoặc kết nối tới MongoDB thông qua connection string (ví dụ: `mongodb://localhost:27017`).

---

### 2. Tạo Database
- Nhấn **Create Database**.
- Điền:
  - **Database Name:** `RecruitmentDB`
  - **Collection Name:** `jobs`
- Chọn **Create Database** để hoàn tất.

---

### 3. Import dữ liệu vào Collection `jobs`
- Trong database **RecruitmentDB**, chọn collection **jobs**.
- Nhấn **Import Data** → chọn file `RecruitmentDB.jobs.json`.
- Chọn định dạng **JSON** → Import.

---

### 4. Tạo Collection `users` và Import dữ liệu
- Trong **RecruitmentDB**, nhấn **Create Collection** → đặt tên: `users`.
- Vào collection **users** → chọn **Import Data**.
- Chọn file `RecruitmentDB.users.json`, định dạng **JSON** → Import.

---

### Kết quả
Sau khi hoàn thành:
- Database `RecruitmentDB` tồn tại với 2 collection:
  - `jobs` (dữ liệu import từ RecruitmentDB.jobs.json)
  - `users` (dữ liệu import từ RecruitmentDB.users.json)

