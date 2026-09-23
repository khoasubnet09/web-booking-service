# SERVICE BOOKING MANAGEMENT SYSTEM

Dự án demo full-stack cho bài toán đặt lịch dịch vụ. Hệ thống có hai vai trò chính: Customer và Admin

1. Customer: Đăng nhập, xem dịch vụ, xem khung giờ trống, tạo booking, xem và hủy booking của mình.
2. Admin: Quản lý dịch vụ, nhân viên, lịch làm việc và toàn bộ booking.

Không có thanh toán, email, realtime, background job hoặc các phần bonus khác trong phạm vi bắt buộc.

# TECH STACK CỦA DỰ ÁN
- Frontend: Next.js 15 App Router, TypeScript, Tailwind CSS v4
- Backend: ASP.NET Core Web API, Entity Framework Core
- Database: Microsoft SQL Server
- Authentication: JWT Bearer
- API document/test: Swagger/OpenAPI

# CẤU TRÚC DỰ ÁN (PROJECT STRUCTURE)

```text
service-booking/
|-- src/
|   |-- backend/                                      # ASP.NET Core Web API
|   |   |-- Controllers/                              # Nhận request HTTP, định tuyến và kiểm soát quyền truy cập
|   |   |   |-- AuthController.cs                     # Đăng nhập và lấy thông tin user hiện tại
|   |   |   |-- ServicesController.cs                 # Danh sách dịch vụ public và CRUD dịch vụ cho Admin
|   |   |   |-- StaffsController.cs                   # CRUD nhân viên và API lịch làm việc
|   |   |   |-- BookingsController.cs                 # Khung giờ trống, booking của Customer và quản lý booking của Admin
|   |   |   `-- HealthController.cs                   # Kiểm tra backend còn hoạt động
|   |   |
|   |   |-- Services/                                 # Tầng xử lý nghiệp vụ
|   |   |   |-- Interfaces/                           # Interface (hợp đồng) của service
|   |   |   |-- AuthService.cs                        # Đăng nhập, user hiện tại và kiểm tra mật khẩu
|   |   |   |-- JwtTokenService.cs                    # Khởi tạo JWT chứa các Claims (user id, email và role)
|   |   |   |-- ServiceCatalogService.cs              # Danh sách, tạo, sửa, tìm kiếm và phân trang dịch vụ
|   |   |   |-- StaffService.cs                       # Danh sách, tạo, sửa nhân viên và kiểm tra email duy nhất
|   |   |   |-- WorkScheduleService.cs                # Tạo lịch làm việc và kiểm tra trùng ca làm việc của nhân viên
|   |   |   |-- BookingAvailabilityService.cs         # Tính toán logic các khung giờ còn trống
|   |   |   `-- BookingService.cs                     # Tạo, hủy, xem, cập nhật booking và xử lý quy tắc khi booking
|   |   |
|   |   |-- DTOs/                                    # Dữ liệu request/response của API
|   |   |   |-- Auth/                                 # DTO đăng nhập, đăng ký và user hiện tại
|   |   |   |-- ServiceCatalog/                       # DTO tạo, sửa và xem danh sách dịch vụ
|   |   |   |-- Staff/                                # DTO tạo, sửa và xem thông tin nhân viên
|   |   |   |-- WorkSchedule/                         # DTO tạo và xem lịch làm việc
|   |   |   |-- Booking/                              # DTO booking, hủy booking, trạng thái và khung giờ trống
|   |   |   `-- Common/                               # DTO dùng chung, ví dụ PagedResult<T> cho phân trang và giờ Việt Nam
|   |   |
|   |   |-- Entities/                                # Entity EF Core ánh xạ vào CSDL SQL Server
|   |   |   |-- User.cs                               # Tài khoản Admin/Customer, role và password hash
|   |   |   |-- Service.cs                            # Dịch vụ, thời lượng, giá và trạng thái active
|   |   |   |-- Staff.cs                              # Thông tin nhân viên và trạng thái active
|   |   |   |-- WorkSchedule.cs                       # Ngày và giờ làm việc của nhân viên
|   |   |   `-- Booking.cs                            # Thời gian booking, trạng thái, ghi chú và lý do hủy
|   |   |
|   |   |-- Enums/
|   |   |   |-- UserRole.cs                           # Vai trò Customer = 1, Admin = 2
|   |   |   `-- BookingStatus.cs                      # Trạng thái Pending, Confirmed, Completed, Cancelled
|   |   |
|   |   |-- Data/
|   |   |   |-- AppDbContext.cs                       # DbSet, quan hệ, index và ràng buộc dữ liệu
|   |   |   `-- DbSeeder.cs                           # Tạo dữ liệu mẫu cho môi trường development
|   |   |
|   |   |-- Common/
|   |   |   `-- VietnamTime.cs                        # Helper chuyển đổi múi giờ Asia/Ho_Chi_Minh
|   |   |
|   |   |-- Exceptions/
|   |   |   `-- ApiException.cs                       # Lỗi nghiệp vụ có status code và mã lỗi
|   |   |
|   |   |-- Middleware/
|   |   |   `-- ExceptionHandlingMiddleware.cs       # Chuyển exception thành JSON error response
|   |   |
|   |   |-- Migrations/                              # Migration EF Core cho SQL Server
|   |   |-- Program.cs                               # Cấu hình DI, JWT, CORS, Swagger, migration và dữ liệu mẫu
|   |   |-- appsettings.json                         # Cấu hình không chứa SECRET_KEY và SQL SERVER CONNECTION
|   |   |-- appsettings.Development.json             # Cấu hình cho môi trường development
|   |   `-- ServiceBooking.Api.csproj                # File project của backend
|   |
|   `-- frontend/                                  # Ứng dụng Next.js
|       |-- src/
|       |   |-- app/                                # Các page theo App Router
|       |   |   |-- page.tsx                          # Trang chủ
|       |   |   |-- layout.tsx                        # Layout gốc
|       |   |   |-- loading.tsx                       # UI loading dùng chung
|       |   |   |-- error.tsx                         # UI lỗi dùng chung
|       |   |   |-- globals.css                       # Import Tailwind CSS v4
|       |   |   |
|       |   |   |-- login/page.tsx                    # Form đăng nhập và điều hướng theo role
|       |   |   |-- services/page.tsx                 # Danh sách dịch vụ public/Customer
|       |   |   |-- booking/page.tsx                  # Customer chọn dịch vụ, nhân viên, ngày, slot và tạo booking
|       |   |   |-- my-bookings/page.tsx              # Customer xem và hủy booking của mình
|       |   |   |
|       |   |   `-- admin/
|       |   |       |-- services/page.tsx                # Admin quản lý dịch vụ
|       |   |       |-- schedules/page.tsx               # Admin quản lý nhân viên và lịch làm việc
|       |   |       `-- bookings/page.tsx                # Admin xem, lọc và cập nhật trạng thái booking
|       |   |
|       |   |-- components/
|       |   |   `-- AppHeader.tsx                    # Header và menu theo vai trò
|       |   |
|       |   `-- lib/
|       |       |-- api.ts                            # Fetch wrapper, ApiError và hàm tạo query string
|       |       `-- auth.ts                           # Lưu token, đăng nhập và gọi API có xác thực
|       |
|       |-- .env.example                            # Mẫu cấu hình NEXT_PUBLIC_API_URL
|       |-- package.json                            # Dependency và script của frontend
|       |-- next.config.ts
|       |-- postcss.config.mjs
|       `-- tsconfig.json
|
|-- docker-compose.yml                             # Có trong repo nhưng không bắt buộc cho luồng chạy chính
|-- guide.md                                       # Ghi chú triển khai nội bộ
|-- README.md                                      # Tài liệu chính của dự án
```

# TỔNG QUAN HỆ THỐNG (ARCHITECTURE OVERVIEW)

Hệ thống được chia thành hai phần chính: 
    1. Backend - Xử lý API/nghiệp vụ/dữ liệu, 
    2. Frontend - Hiển thị giao diện để Customer và Admin thao tác.

Phía backend, một request đi qua các bước sau:
1. Client gọi API từ Swagger hoặc giao diện frontend.
2. `Controller` nhận request, kiểm tra route, dữ liệu đầu vào và quyền truy cập.
3. `Service` xử lý nghiệp vụ chính như đăng nhập, tạo booking, kiểm tra trùng lịch, hủy booking hoặc cập nhật trạng thái.
4. `AppDbContext` dùng EF Core để đọc/ghi dữ liệu xuống CSDL SQL Server.
5. Backend trả về `DTO response` cho client. Nếu có lỗi, `ExceptionHandlingMiddleware` chuyển lỗi thành JSON thống nhất.

Vai trò của từng tầng (layer):
- `Controllers`: nơi khai báo API endpoint, nhận request và kiểm soát quyền truy cập bằng JWT/Role.
- `Services`: nơi chứa quy tắc nghiệp vụ của hệ thống, ví dụ không đặt lịch trong quá khứ, không đặt trùng giờ, không hủy booking đã hoàn thành.
- `DTOs`: định nghĩa dữ liệu request/response để frontend và backend giao tiếp rõ ràng, không trả trực tiếp entity từ database.
- `Entities`: mô tả dữ liệu thật được lưu trong database như User, Service, Staff, WorkSchedule, Booking.
- `AppDbContext`: cấu hình bảng, quan hệ, index và ràng buộc dữ liệu cho SQL Server.
- `ExceptionHandlingMiddleware`: chuẩn hóa lỗi API để frontend dễ hiển thị thông báo.

Phía frontend, các màn hình chính được tổ chức theo Next.js App Router:
- `/login`: màn hình đăng nhập.
- `/services`: danh sách dịch vụ.
- `/booking`: Customer chọn dịch vụ, nhân viên, ngày, khung giờ trống và tạo booking.
- `/my-bookings`: Customer xem và hủy booking của mình.
- `/admin/services`: Admin quản lý dịch vụ.
- `/admin/schedules`: Admin quản lý nhân viên và lịch làm việc.
- `/admin/bookings`: Admin xem, lọc, xác nhận, hoàn thành hoặc hủy booking.

# QUY TẮC NGHIỆP VỤ CHÍNH

# 1. Đăng nhập và phân quyền
- Sau khi đăng nhập thành công, backend tạo JWT cho user.
- JWT chứa các Claims cần thiết để xác thực như user id, email và role.
- Backend kiểm soát quyền truy cập bằng `[Authorize]` và `[Authorize(Roles = "...")]`.
- Frontend chỉ dùng role để hiển thị menu và điều hướng phù hợp. Quyền truy cập thật vẫn do backend quyết định.

# 2. Dịch vụ (Service)
- Người chưa đăng nhập vẫn có thể xem các dịch vụ đang active.
- Admin có thể xem toàn bộ dịch vụ, bao gồm cả dịch vụ inactive.
- Admin có thể tạo và cập nhật dịch vụ.
- Dịch vụ inactive không được dùng để tạo booking mới.

# 3. Nhân viên và lịch làm việc (Staff and Work Schedule)
- Customer chỉ xem được các nhân viên đang ở trạng thái active.
- Admin có thể tạo và cập nhật thông tin nhân viên.
- Admin có thể tạo lịch làm việc cho từng nhân viên.
- Một ca làm việc phải có giờ bắt đầu nhỏ hơn giờ kết thúc: `StartTime < EndTime`.
- Một nhân viên không được có các ca làm việc bị trùng thời gian trong cùng một ngày.

# 4. Khung giờ trống (Available Slots)
Khung giờ trống được tính dựa trên:
- Thời lượng (DurationMinutes) của dịch vụ được chọn;
- Nhân viên được chọn;
- Ngày được chọn;
- Lịch làm việc của nhân viên trong ngày đó;
- Các booking hiện có, ngoại trừ booking đã hủy.

Hệ thống đang áp dụng cách chia slot theo mỗi 15 phút. Ví dụ dịch vụ 60 phút có thể có các khung giờ như sau:
- 09:00 - 10:00
- 09:15 - 10:15
- 09:30 - 10:30
==> Cách chia này giúp Customer có nhiều lựa chọn hơn, thay vì chỉ được chọn các mốc giờ tròn.

# 5. Đặt lịch (Booking)
- Customer chỉ chọn thời gian bắt đầu booking.
- Backend tự tính thời gian kết thúc theo công thức: `EndTime = StartTime + Service.DurationMinutes`.
- Customer không gửi `CustomerId`; backend lấy Customer hiện tại từ JWT.
- Booking phải nằm trong tương lai.
- Booking phải nằm trọn trong lịch làm việc của nhân viên.
- Booking không được trùng với booking khác chưa bị hủy.
- Booking đã hủy không còn chiếm slot.
- Customer có thể hủy booking của chính mình và phải nhập lý do hủy.
- Customer không được tự chuyển booking sang trạng thái Completed.
- Admin có thể Confirm, Complete hoặc Cancel booking.
- Booking đã Completed thì không được Cancel.

Rule kiểm tra trùng lịch:
- Booking mới bị xem là trùng nếu `newStart < existingEnd && newEnd > existingStart`.
- Ví dụ một Booking trùng lịch: Booking đã đặt: 9:00-12:00
                                Booking đặt mới: 10:00-13:00 (overlap)

# 6. Cơ sở dữ liệu (Database)
Hệ thống sử dụng Microsoft SQL Server.
Các bảng chính:
- `Users`: lưu tài khoản Admin và Customer.
- `Services`: lưu thông tin dịch vụ.
- `Staffs`: lưu thông tin nhân viên.
- `WorkSchedules`: lưu lịch làm việc của nhân viên.
- `Bookings`: lưu thông tin đặt lịch.

Các ràng buộc quan trọng:
- `Users.Email` là duy nhất.
- `Staffs.Email` là duy nhất.
- `Bookings.BookingCode` là duy nhất.
- Mỗi booking liên kết với một Customer, một Service và một Staff.
- Mỗi lịch làm việc thuộc về một Staff.

Dữ liệu mẫu khi chạy development:
- 1 tài khoản Admin.
- 2 tài khoản Customer.
- 2 nhân viên.
- 5 dịch vụ.
- Lịch làm việc mẫu cho 7 ngày.
- 10 booking mẫu.

# 7. Cấu hình (Configuration)
Các giá trị nhạy cảm không lưu trực tiếp trong `appsettings.json`.

1. Cấu hình backend cần có:
- `ConnectionStrings:DefaultConnection`: chuỗi kết nối SQL Server.
- `Jwt:Key`: secret key dùng để ký JWT.
- `Jwt:Issuer`: nơi phát hành token.
- `Jwt:Audience`: đối tượng sử dụng token.
- `FrontendUrl`: địa chỉ frontend được phép gọi API.

2. Với môi trường local, `ConnectionStrings:DefaultConnection` và `Jwt:Key` nên lưu bằng User Secrets, cụ thể:
- Thiết lập connection string: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<local-sql-server-connection-string>" --project src/backend/ServiceBooking.Api.csproj`
- Thiết lập JWT key: `dotnet user-secrets set "Jwt:Key" "<local-jwt-signing-key-at-least-32-characters>" --project src/backend/ServiceBooking.Api.csproj`

3. Kiểm tra các secret đã lưu:
- `dotnet user-secrets list --project src/backend/ServiceBooking.Api.csproj`

4. Frontend đọc địa chỉ API từ file `.env.local`.
- Nội dung cần có: `NEXT_PUBLIC_API_URL=http://localhost:5000/api`

# HƯỚNG DẪN CHẠY DỰ ÁN Ở BACKEND
Yêu cầu cài đặt:
- .NET SDK phù hợp với backend project (hệ thống hiện tại đang dùng 10.0)
- `dotnet-ef`.
- Microsoft SQL Server (hệ thống hiện tại đang dùng 2025)

Cách chạy:
1. Mở terminal tại thư mục gốc dự án.
2. Chạy `cd src/backend`.
3. Chạy `dotnet restore`.
4. Chạy `dotnet ef database update`.
5. Chạy `dotnet run`.

Sau khi chạy thành công, backend dùng các URL mặc định:
- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- Health check: `http://localhost:5000/api/health`

Ở môi trường development, `Program.cs` cũng tự xử lý:
- Apply database migration.
- Seed dữ liệu mẫu.
- Bật Swagger để test API.

# HƯỚNG DẪN CHẠY DỰ ÁN PHÍA FRONTEND
Yêu cầu:
- Node.js 20+

Nếu dùng PowerShell:
1. Chạy `cd src/frontend`.
2. Chạy `Copy-Item .env.example .env.local`.
3. Chạy `npm install`.
4. Chạy `npm run dev`.

Nếu dùng Bash:
1. Chạy `cd src/frontend`.
2. Chạy `cp .env.example .env.local`.
3. Chạy `npm install`.
4. Chạy `npm run dev`.

Frontend chạy tại:
- `http://localhost:3000`

# TÀI KHOẢN DEMO
| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@servicebooking.com` | `Admin@123` |
| Customer | `customer1@servicebooking.com` | `Customer@123` |
| Customer | `customer2@servicebooking.com` | `Customer@123` |

Lưu ý: Các tài khoản này chỉ dùng cho local development và demo.

# CÁC ENDPOINT CHÍNH TRONG HỆ THỐNG

### Auth
POST /api/auth/login
GET  /api/auth/me

### Services
GET  /api/services
GET  /api/services/admin
POST /api/services
PUT  /api/services/{id}

### Staff and Work Schedule
GET  /api/staffs
GET  /api/staffs/admin
POST /api/staffs
PUT  /api/staffs/{id}
GET  /api/staffs/{staffId}/schedules
POST /api/staffs/{staffId}/schedules

### Bookings
GET   /api/bookings/available-slots
POST  /api/bookings
GET   /api/bookings/my-bookings
POST  /api/bookings/{id}/cancel
GET   /api/bookings
PATCH /api/bookings/{id}/status

# DATABASE MIGRATION VÀ SWAGGER

Migration / Database Schema
Dự án sử dụng EF Core Migrations để bàn giao schema database.

Migration nằm tại:
- `src/backend/Migrations`

Khi setup database, chạy:
- `cd src/backend`
- `dotnet ef database update`

Vì đã có EF Core Migrations, project không cần SQL script riêng.

# SWAGGER / API DOCUMENTATION
Dự án sử dụng Swagger để xem và test API.

Sau khi chạy backend, mở:
- `http://localhost:5000/swagger`

Swagger dùng để:
- Xem danh sách API endpoint.
- Test API trực tiếp.
- Đăng nhập lấy JWT.
- Nhập token qua nút Authorize để test các API cần phân quyền.

Vì đã có Swagger, project không cần Postman collection riêng.

# TEST CASE KIỂM THỬ TỐI THIỂU

1. TC1: Không cho đặt lịch trong quá khứ
- Kết quả mong đợi: `400 BOOKING_START_TIME_IN_PAST`
- Ý nghĩa: Customer chỉ được tạo booking ở thời điểm tương lai.

TC2: Không cho đặt lịch ngoài giờ làm việc
- Kết quả mong đợi: `409 BOOKING_OUTSIDE_WORK_SCHEDULE`
- Ý nghĩa: Booking phải nằm trọn trong ca làm việc của nhân viên.

TC3: Không cho đặt hai booking trùng giờ
- Kết quả mong đợi: `409 BOOKING_SLOT_CONFLICT`
- Ý nghĩa: Một nhân viên không thể nhận hai booking bị giao nhau về thời gian, trừ booking đã hủy.

TC4: Customer không được xem hoặc hủy booking của Customer khác
- Kết quả mong đợi:
  - Customer chỉ nhìn thấy booking của chính mình ở màn hình `Booking của tôi`.
  - Nếu gọi API hủy booking của người khác, backend không cho thao tác.

TC5: Customer không được hoàn thành booking
- Kết quả mong đợi: Customer gọi `PATCH /api/bookings/{id}/status` sẽ nhận `403 Forbidden`.
- Ý nghĩa: Chỉ Admin mới được cập nhật trạng thái Confirmed/Completed/Cancelled.

TC6: Không cho hủy booking đã hoàn thành
- Kết quả mong đợi: `409 BOOKING_COMPLETED_CANNOT_BE_CANCELLED`
- Ý nghĩa: Booking đã Completed được xem là đã hoàn tất, không được hủy ngược lại.

# KIỂM TRA TRƯỚC KHI BUILD DỰ ÁN
Backend:
- Chạy `cd src/backend`.
- Chạy `dotnet build --no-restore`.

Nếu build lỗi do file `.exe` đang bị khóa, thường là vì `dotnet run` vẫn đang chạy. Hãy dừng backend trước rồi build lại.

Frontend:
- Chạy `cd src/frontend`.
- Chạy `npm run build`.

Nếu Next.js báo lỗi stale `.next` chunk, hãy dừng frontend dev server, xóa thư mục `.next`, rồi chạy lại:
- `cd src/frontend`
- `Remove-Item -Recurse -Force .next`
- `npm run dev`

# HƯỚNG DẪN CHẠY SQL SERVER BẰNG DOCKER COMPOSE
Phần này dùng Docker Compose để chạy SQL Server cho môi trường development. Backend và frontend vẫn chạy local như hướng dẫn ở trên để dễ debug.

Ý nghĩa của phần Docker Compose:
- Không cần cài SQL Server trực tiếp trên từng máy.
- Chỉ cần Docker Desktop là có thể tạo SQL Server container giống nhau giữa các máy.
- Password thật được lưu trong file `.env` local, không commit lên source code.
- File `.env.example` chỉ là file mẫu để người khác biết cần cấu hình biến nào.

Yêu cầu cài đặt:
- Docker Desktop.
- Trên Windows, nên bật WSL 2 khi Docker Desktop yêu cầu.
- .NET SDK và Node.js vẫn cần cài như hướng dẫn chạy backend/frontend ở trên.

Các file liên quan:
- `docker-compose.yml`: cấu hình SQL Server container.
- `.env.example`: file mẫu chứa biến `MSSQL_SA_PASSWORD`.
- `.env`: file cấu hình thật trên máy local, không commit lên Git.

Cách chạy SQL Server bằng Docker Compose:
1. Mở terminal tại thư mục gốc dự án.
2. Tạo file `.env` từ file mẫu:
   - PowerShell: `Copy-Item .env.example .env`
   - Bash: `cp .env.example .env`
3. Mở file `.env` và đổi `MSSQL_SA_PASSWORD` thành password mạnh của bạn.
4. Chạy SQL Server container: `docker compose up -d`
5. Kiểm tra container đang chạy: `docker compose ps`

Ví dụ connection string khi backend kết nối tới SQL Server trong Docker:
- `Server=localhost,1434;Database=service_booking;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;`

Thiết lập connection string cho backend bằng User Secrets:
- `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1434;Database=service_booking;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;" --project src/backend/ServiceBooking.Api.csproj`

Sau khi SQL Server container đã chạy:
1. Chạy `cd src/backend`.
2. Chạy `dotnet ef database update`.
3. Chạy `dotnet run`.
4. Mở Swagger tại `http://localhost:5000/swagger`.

Các lệnh Docker Compose thường dùng:
- Chạy SQL Server ở background: `docker compose up -d`
- Xem container: `docker compose ps`
- Xem log SQL Server: `docker compose logs -f sqlserver`
- Dừng container nhưng giữ dữ liệu: `docker compose down`
- Dừng container và xóa luôn dữ liệu database: `docker compose down -v`

Lưu ý:
- File `.env` đã được đưa vào `.gitignore`, không nên commit file này.
- Nếu đổi password trong `.env` sau khi volume SQL Server đã được tạo, password cũ có thể vẫn còn hiệu lực. Khi muốn reset sạch database và password, dùng `docker compose down -v`, sau đó chạy lại `docker compose up -d`.
- Cách làm này chỉ container hóa SQL Server cho development. Backend và frontend vẫn chạy local để dễ sửa code và debug.