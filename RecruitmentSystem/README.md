# Hệ thống tuyển dụng NoSQL

Dự án này là một hệ thống quản lý tuyển dụng được xây dựng bằng .NET Core cho backend, React cho frontend website và Flutter cho ứng dụng di động.

## Cấu trúc thư mục

Dưới đây là mô tả về cấu trúc thư mục của dự án:

-   **`Demo - Backend/`**: Chứa mã nguồn cho phần backend của ứng dụng, được xây dựng bằng .NET.
    -   **`API/`**: Chứa project API chính, bao gồm các controllers, DTOs, và cấu hình.
    -   **`Core/`**: Chứa các logic cốt lõi của ứng dụng, interfaces, models, và DTOs.
    -   **`Infrastructure/`**: Chứa các triển khai cụ thể cho các dịch vụ và repositories, kết nối với cơ sở dữ liệu.
-   **`Demo - Frontend/`**: Chứa mã nguồn cho phần frontend.
    -   **`Mobile/`**: (Dự kiến) Ứng dụng di động được xây dựng bằng Flutter.
    -   **`Website/`**: (Dự kiến) Ứng dụng web được xây dựng bằng React.
    -   **`Dữ liệu - MongoDB/`**: Chứa các tệp `RecruitmentDB.jobs.json` và `RecruitmentDB.users.json`. Đây là dữ liệu mẫu có thể được import vào MongoDB để khởi tạo cơ sở dữ liệu cho ứng dụng.
    -   **`Test API/`**: Chứa tệp `RecruitmentAPI.postman_collection.json`. Đây là một Postman collection chứa các request đã được cấu hình sẵn để dễ dàng kiểm thử các endpoint của backend API.
    -   **`Tệp Báo Cáo/`**: Chứa các tài liệu báo cáo của dự án, bao gồm báo cáo công việc (`.docx`) và bài trình bày (`.pptx`) về kiến trúc và công nghệ sử dụng.

## Hướng dẫn cài đặt và chạy dự án

### Backend (.NET)

1.  **Di chuyển vào thư mục API:**
    ```sh
    cd "Demo - Backend/API"
    ```
2.  **Khôi phục các dependencies:**
    ```sh
    dotnet restore
    ```
3.  **Chạy dự án:**
    ```sh
    dotnet run
    ```
    API sẽ chạy tại địa chỉ `https://localhost:7069` hoặc `http://localhost:5138`.

### Frontend (Website - React)

*(Phần này sẽ được cập nhật chi tiết hơn. Dưới đây là hướng dẫn mẫu.)*

1.  **Di chuyển vào thư mục Website:**
    ```sh
    cd "Demo - Frontend/Website"
    ```
2.  **Cài đặt các dependencies:**
    ```sh
    npm install
    ```
3.  **Chạy ứng dụng:**
    ```sh
    npm start
    ```

### Frontend (Mobile - Flutter)

*(Phần này sẽ được cập nhật chi tiết hơn. Dưới đây là hướng dẫn mẫu.)*

1.  **Di chuyển vào thư mục Mobile:**
    ```sh
    cd "Demo - Frontend/Mobile"
    ```
2.  **Lấy các dependencies:**
    ```sh
    flutter pub get
    ```
3.  **Chạy ứng dụng:**
    ```sh
    flutter run
    ```
