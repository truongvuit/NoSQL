# Tài liệu Vai trò & Tuyến API

Tài liệu này tóm tắt các tuyến API và quyền truy cập của từng vai trò, kèm các lưu ý hành vi (khả năng hiển thị, bộ nhớ đệm).

## Vai trò
- Công khai (Public): người dùng chưa đăng nhập
- Ứng viên (Candidate): người tìm việc đã xác thực
- Nhà tuyển dụng (Recruiter): người dùng doanh nghiệp đã xác thực; có thể sở hữu công ty và tạo/quản lý tin tuyển dụng
- Quản trị (Admin): quản trị viên hệ thống

## Xác thực (Authentication)
- POST /api/auth/register — Công khai
- POST /api/auth/login — Công khai
- POST /api/auth/refresh — Công khai (cần refresh token)
- POST /api/auth/logout — Đã đăng nhập (mọi vai trò)

## Người dùng (Users)
- GET /api/users/profile — Ứng viên/Nhà tuyển dụng/Quản trị (hồ sơ của chính mình)
- PUT /api/users/profile — Ứng viên/Nhà tuyển dụng/Quản trị (cập nhật hồ sơ của chính mình)
- GET /api/users?page=&pageSize= — Chỉ Quản trị
- GET /api/users/{userId} — Chỉ Quản trị
- PUT /api/users/{userId} — Chỉ Quản trị
- DELETE /api/users/{userId} — Chỉ Quản trị

## Công ty (Company)
- POST /api/company/register — Ứng viên (yêu cầu nâng cấp lên Nhà tuyển dụng và tạo công ty)
- GET /api/company/pending — Chỉ Quản trị
- POST /api/company/verify — Chỉ Quản trị (duyệt/từ chối)
- GET /api/company?page=&pageSize=&verified= — Chỉ Quản trị
- GET /api/company/{companyId} — Quản trị/Nhà tuyển dụng (chủ sở hữu) [mở rộng tùy chọn]
- PUT /api/company/{companyId} — Quản trị hoặc Nhà tuyển dụng của công ty đó
- DELETE /api/company/{companyId} — Chỉ Quản trị (xóa mềm liên kết công ty của người dùng; hạ vai trò về ứng viên)

## Công việc (Jobs)
- GET /api/jobs?page=&pageSize=
  - Công khai: chỉ thấy các công việc đã publish
  - Nhà tuyển dụng: thấy công việc đã publish + các bản nháp (draft) do chính mình tạo
  - Quản trị: thấy tất cả
  - Bộ nhớ đệm: khóa cache theo người xem — jobs:{public|admin|recruiter:{userId}}:page:{n}:size:{m} (5 phút)

- GET /api/jobs/{id}
  - Công khai/Ứng viên: chỉ khi công việc ở trạng thái publish
  - Nhà tuyển dụng: khi publish HOẶC do chính mình tạo
  - Quản trị: luôn xem được
  - Tác dụng phụ: tăng bộ đếm lượt xem; đảm bảo danh sách ứng viên (Applicants) đã khởi tạo

- POST /api/jobs — Chỉ Nhà tuyển dụng
  - Tạo công việc ở trạng thái draft; snapshot công ty lấy từ công ty của nhà tuyển dụng
  - Vô hiệu hóa cache: chỉ xóa cache theo scope của nhà tuyển dụng tạo job (public/admin chưa bị ảnh hưởng)

- PUT /api/jobs/{id} — Nhà tuyển dụng (job của mình) hoặc Quản trị
  - Cập nhật một phần thông tin qua các trường nullable trong UpdateJobRequest
  - Vô hiệu hóa cache: xóa cache của public, admin và recruiter (nội dung có thể ảnh hưởng hiển thị)

- DELETE /api/jobs/{id} — Nhà tuyển dụng (job của mình) hoặc Quản trị
  - Xóa mềm tại tầng repository
  - Vô hiệu hóa cache: xóa cache của public, admin và recruiter

- PATCH /api/jobs/{id}/publish — Nhà tuyển dụng (job của mình) hoặc Quản trị
  - Đặt trạng thái = published; cập nhật UpdatedAt
  - Vô hiệu hóa cache: xóa cache của public, admin và recruiter (job trở nên hiển thị công khai)

- PATCH /api/jobs/{id}/unpublish — Nhà tuyển dụng (job của mình) hoặc Quản trị
  - Đặt trạng thái = draft; cập nhật UpdatedAt
  - Vô hiệu hóa cache: xóa cache của public, admin và recruiter (job không còn hiển thị công khai)

- POST /api/jobs/search
  - Tìm kiếm trong các job đã publish theo bộ lọc: keyword, city, categories, page, pageSize
  - Cho phép Công khai

- POST /api/jobs/{id}/apply — Chỉ Ứng viên
  - Thêm một bản snapshot Ứng viên vào job; ngăn nộp trùng bởi cùng người dùng

- PATCH /api/jobs/applications/{applicantId}/status — Chỉ Nhà tuyển dụng (phải sở hữu job)
  - Trạng thái hợp lệ: Pending, Screening, Interview, Rejected, Hired

- GET /api/jobs/company/{companyId}
  - Công khai: danh sách job của công ty (hiện chưa áp dụng bộ lọc hiển thị như toàn cục; cân nhắc đồng bộ trong tương lai)

## Tải tệp (Uploads)
- POST /api/upload/cv — Ứng viên/Nhà tuyển dụng/Quản trị (multipart/form-data với trường file)
- POST /api/upload/image — Ứng viên/Nhà tuyển dụng/Quản trị (multipart/form-data với trường file)
- DELETE /api/upload/{fileType}/{fileName} — Quản trị hoặc Nhà tuyển dụng
- GET /api/upload/validate?fileType=&fileSize=&fileName= — Ứng viên/Nhà tuyển dụng/Quản trị
- GET /api/upload/ping — Công khai (kiểm tra DI)

Ghi chú:
- FileService lưu tệp vào wwwroot/uploads/{cv|image} và trả về fileUrl, filePath
- Tương thích Swagger thông qua DTO [FromForm] và [Consumes("multipart/form-data")]

## Bảng điều khiển (Dashboard) — Admin
- GET /api/dashboard/stats — Quản trị
- GET /api/dashboard/applications-chart — Quản trị
- GET /api/dashboard/top-employers — Quản trị
- GET /api/dashboard/moderation-queue — Quản trị

## Endpoint thử nghiệm/Phát triển
- /api/test/* — Không yêu cầu xác thực (chỉ môi trường phát triển)

## Chiến lược bộ nhớ đệm (Jobs)
- Khóa cache theo người xem:
  - public: jobs:public:page:{page}:size:{size}
  - recruiter: jobs:recruiter:{userId}:page:{page}:size:{size}
  - admin: jobs:admin:page:{page}:size:{size}
- TTL: 5 phút
- Vô hiệu hóa khi có thay đổi trạng thái (create/update/delete/publish/unpublish) theo phạm vi ảnh hưởng như trên.

## Tình huống biên & Lỗi thường gặp
- 404 Not Found: job không tồn tại hoặc không có quyền xem (ví dụ: recruiter cố xem draft của người khác)
- 403 Forbidden: vi phạm quyền (ví dụ: recruiter sửa job của người khác)
- 400 Bad Request: cập nhật/xóa thất bại từ tầng repository
- 401 Unauthorized: thiếu/sai JWT cho endpoint bảo vệ

## Tiêu chí thành công
- Kiểm tra quyền nghiêm ngặt ở từng endpoint
- Khả năng hiển thị danh sách/chi tiết job nhất quán theo vai trò
- Cache được làm mới khi có thay đổi và tự điền lại ở lần đọc tiếp theo
- Endpoint tải tệp nhận multipart/form-data và không làm vỡ Swagger
