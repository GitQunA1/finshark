# FinShark API

Dự án học **ASP.NET Core Web API**, được làm theo khóa hướng dẫn *"ASP.NET Web API .NET 8 Tutorial 2024"* (FinShark – Teddy Smith). Mục tiêu là luyện tập cách xây dựng một backend REST API hoàn chỉnh: CRUD, Entity Framework Core, quan hệ dữ liệu, xác thực JWT và gọi API bên ngoài.

> ⚠️ Đây là project **demo/học tập**. Connection string, JWT key, API key trong `appsettings.json` được commit công khai và chỉ dùng cho môi trường local.

---

## 1. Ý tưởng

FinShark là một ứng dụng quản lý cổ phiếu đơn giản:

- Xem danh sách **cổ phiếu (Stock)**, lọc, sắp xếp, phân trang.
- Người dùng **đăng ký / đăng nhập** và nhận JWT token.
- Người dùng **bình luận (Comment)** về một mã cổ phiếu.
- Người dùng có **danh mục đầu tư (Portfolio)** riêng – thêm/xóa mã cổ phiếu mình theo dõi.
- Nếu mã cổ phiếu chưa có trong DB, hệ thống tự gọi **Financial Modeling Prep (FMP) API** để lấy thông tin và lưu lại.

## 2. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Framework | ASP.NET Core Web API – **.NET 9** |
| ORM | Entity Framework Core 9 (Code First + Migrations) |
| Database | SQL Server |
| Xác thực | ASP.NET Core Identity + **JWT Bearer** |
| Tài liệu API | Swagger (Swashbuckle) có hỗ trợ nhập Bearer token |
| JSON | Newtonsoft.Json (bỏ qua vòng lặp tham chiếu) |
| API ngoài | Financial Modeling Prep (qua `HttpClient`) |

## 3. Kiến trúc

Project dùng kiến trúc phân lớp (layered) với **Repository Pattern** và **Dependency Injection**:

```
HTTP Request
    │
    ▼
Controllers  ──►  DTOs / Mappers   (nhận request, validate, map dữ liệu)
    │
    ▼
Interfaces (IStockRepositoty, ICommentRepository, IPortfolioRepository, ITokenService, IFMPService)
    │
    ▼
Repository / Service   (xử lý logic truy xuất dữ liệu, tạo token, gọi API ngoài)
    │
    ▼
ApplicationDBContext (EF Core)  ──►  SQL Server
```

- **Controller** không truy cập DB trực tiếp, chỉ gọi qua interface → dễ thay thế, dễ test.
- **DTO** tách biệt dữ liệu trả về/nhận vào với Model trong DB (không lộ field thừa, tránh vòng lặp).
- **Mapper** là các extension method (`ToStockDto()`, `ToCommentFromCreate()`…) chuyển đổi Model ⇄ DTO.
- Các service được đăng ký trong `Program.cs` bằng `AddScoped` / `AddHttpClient`.

## 4. Cấu trúc thư mục

```
FINSHARK_Teddy/
├── FINSHARK.sln
└── api/
    ├── Controllers/     # Endpoint: Account, Stock, Comment, Portfolio
    ├── Data/            # ApplicationDBContext (IdentityDbContext), cấu hình quan hệ, seed Role
    ├── Dtos/            # Account, Comment, Stock (request/response + FMPStock)
    ├── Extensions/      # ClaimsExtensions – lấy username từ JWT claims
    ├── Helpers/         # QueryObject – tham số filter/sort/paging
    ├── Interfaces/      # Hợp đồng cho Repository & Service
    ├── Mappers/         # Chuyển đổi Model <-> DTO
    ├── Migrations/      # EF Core migrations
    ├── Models/          # Entity: AppUser, Stock, Comment, Portfolio
    ├── Repository/      # Cài đặt truy vấn EF Core
    ├── Service/         # TokenService (JWT), FMPService (gọi API ngoài)
    ├── Properties/      # launchSettings.json
    ├── Program.cs       # Cấu hình DI, Identity, JWT, Swagger, pipeline
    ├── appsettings.json # Connection string, JWT, FMPKey
    └── api.http         # File test request nhanh trong VS Code / VS
```

## 5. Mô hình dữ liệu

```
AppUser (Identity) 1 ──── * Portfolio * ──── 1 Stock
AppUser            1 ──── * Comment   * ──── 1 Stock
```

- **Stock**: `Id, Symbol, CompanyName, Purchase, LastDiv, Industry, MarketCap`
- **Comment**: `Id, Title, Content, CreatedOn, StockId, AppUserId`
- **Portfolio**: bảng trung gian **nhiều-nhiều** giữa `AppUser` và `Stock`, khóa chính kép `(AppUserId, StockId)`.
- **AppUser** kế thừa `IdentityUser`. Hai role được seed sẵn: `Admin`, `User`.

## 6. Danh sách API

### Account – `api/account`
| Method | Route | Mô tả |
|---|---|---|
| POST | `/register` | Đăng ký, trả về username, email, token |
| POST | `/login` | Đăng nhập, trả về JWT token |

> Mật khẩu yêu cầu: tối thiểu 12 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.

### Stock – `api/stock`
| Method | Route | Auth | Mô tả |
|---|---|---|---|
| GET | `/` | ✅ | Lấy danh sách, hỗ trợ query: `Symbol`, `CompanyName`, `SortBy`, `IsDecsending`, `PageNumber`, `PageSize` |
| GET | `/{id}` | | Lấy chi tiết kèm comments |
| POST | `/` | | Tạo stock |
| PUT | `/{id}` | | Cập nhật stock |
| DELETE | `/{id}` | | Xóa stock |

### Comment – `api/comment`
| Method | Route | Mô tả |
|---|---|---|
| GET | `/` | Lấy tất cả comment |
| GET | `/{id}` | Lấy comment theo id |
| POST | `/{symbol}` | Tạo comment cho mã cổ phiếu (tự lấy từ FMP nếu DB chưa có) |
| PUT | `/{id}` | Cập nhật comment |
| DELETE | `/{id}` | Xóa comment |

### Portfolio – `api/portfolio` (đều yêu cầu đăng nhập)
| Method | Route | Mô tả |
|---|---|---|
| GET | `/` | Lấy danh mục của user hiện tại |
| POST | `/?symbol=XXX` | Thêm mã cổ phiếu vào danh mục |
| DELETE | `/?symbol=XXX` | Xóa mã khỏi danh mục |

## 7. Luồng xác thực JWT

1. User gọi `register` hoặc `login` → `TokenService` tạo JWT (ký HMAC-SHA512, hạn 7 ngày) chứa claim `email` và `given_name`.
2. Client gửi header `Authorization: Bearer <token>` cho các endpoint có `[Authorize]`.
3. Middleware `UseAuthentication()` kiểm tra Issuer, Audience, SigningKey từ `appsettings.json`.
4. `ClaimsExtensions.GetUsername()` đọc username từ token để biết user hiện tại.

Trong Swagger, bấm nút **Authorize** và dán token để test.

## 8. Cách chạy

**Yêu cầu:** .NET 9 SDK, SQL Server, `dotnet-ef` tool (`dotnet tool install --global dotnet-ef`).

```bash
# 1. Sửa ConnectionStrings:DefaultConnection trong api/appsettings.json cho đúng SQL Server của bạn
# 2. (Tùy chọn) Thay FMPKey bằng API key thật từ https://financialmodelingprep.com

cd api
dotnet restore
dotnet ef database update   # tạo database từ migrations
dotnet watch run            # hoặc: dotnet run
```

Mở Swagger tại: `http://localhost:5246/swagger` (xem port trong `Properties/launchSettings.json`).

## 9. Kiến thức học được

- Tạo Web API với Controller, routing, route constraint (`{id:int}`, `{symbol:alpha}`).
- EF Core Code First, migrations, quan hệ 1-nhiều và nhiều-nhiều, `Include/ThenInclude`.
- DTO + Mapper bằng extension method.
- Repository Pattern & Dependency Injection.
- Async/await trong truy vấn DB.
- Filtering, sorting, pagination với `IQueryable`.
- ASP.NET Core Identity + JWT Authentication.
- Cấu hình Swagger với Bearer token.
- Gọi API bên ngoài bằng `HttpClient` (typed client).
