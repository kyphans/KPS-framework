Nice, bắt đầu “build platform” luôn 😎
Mình sẽ làm cho bạn 1 **checklist theo từng giai đoạn**, kèm giải thích ngắn, phù hợp cho 1 .NET fresher.

Bạn không cần làm hết 1 lần, cứ đi từ Phase 1 → Phase 2… là được.

---

## Phase 0 – Chuẩn bị môi trường

☑️ **Cài công cụ cơ bản**

* [ ] Cài **.NET SDK 8+**
* [ ] Cài **IDE**:

  * Visual Studio / Rider / VS Code (có C# extension)
* [ ] Cài **Git**
* [ ] Cài **SQL Server** (hoặc PostgreSQL, nhưng .NET + SQL Server dễ hơn)
* [ ] Tạo tài khoản **GitHub**

Giải thích:
Đây là nền tảng để build bất kỳ app .NET nào. SQL Server dùng làm DB cho Master + Tenant.

---

## Phase 1 – Thiết kế tổng thể & tạo repo GitHub

### 1.1. Chốt danh sách repo (tối thiểu)

☑️ **Quyết định cần các repo sau:**

* [ ] `platform-core`
* [ ] `platform-infrastructure`
* [ ] `module-billing` (module đầu tiên để thử)
* [ ] `platform-host`
* [ ] `tenant-cli`
* [ ] `platform-dev` (repo integration dùng git submodule cho dev local)

Giải thích:
Mỗi repo = 1 khối logic độc lập. Sau này bạn chỉ cần add thêm `module-hr`, `module-inventory`… là xong.

### 1.2. Tạo repo trên GitHub

☑️ Với từng repo trên:

* [ ] Tạo repo **private** (recommend) trong GitHub
* [ ] Clone về máy local:

  ```bash
  git clone git@github.com:your-org/platform-core.git
  ```

---

## Phase 2 – Tạo `platform-core` (nền tảng chung)

🎯 Mục tiêu: có solution & 2 project class library:

* `Platform.Core`
* `Platform.Shared`

### 2.1. Tạo solution & project

☑️ Trong thư mục `platform-core`:

* [ ] Tạo solution:

  ```bash
  dotnet new sln -n PlatformCore
  ```
* [ ] Tạo project `Platform.Core`:

  ```bash
  dotnet new classlib -n Platform.Core -o src/Platform.Core
  ```
* [ ] Tạo project `Platform.Shared`:

  ```bash
  dotnet new classlib -n Platform.Shared -o src/Platform.Shared
  ```
* [ ] Add vào solution:

  ```bash
  dotnet sln add src/Platform.Core/Platform.Core.csproj
  dotnet sln add src/Platform.Shared/Platform.Shared.csproj
  ```

### 2.2. Tạo các folder & interface cơ bản

Trong `Platform.Core`:

☑️ Tạo các folder:

* [ ] `DocTypes/`

  * `DocTypeDefinition.cs`
  * `DocFieldDefinition.cs`
  * `IDocTypeProvider.cs`
* [ ] `Docs/`

  * `IDocEngine.cs`
* [ ] `Tenancy/`

  * `TenantInfo.cs`
  * `ITenantResolver.cs`
  * `ITenantAccessor.cs`
* [ ] `Events/`

  * `IEvent.cs`
  * `IEventHandler.cs`
  * `IEventBus.cs`
  * `HookDefinition.cs`
* [ ] `Common/`

  * `Result.cs` (kiểu `Success/Failure`)
  * `Errors.cs`

Trong `Platform.Shared`:

* [ ] `Contracts/Docs/DocDto.cs`
* [ ] `Contracts/Tenants/TenantDto.cs`

Giải thích:
Repo này chỉ chứa **abstraction** (interface, DTO), không dính EF, không dính HTTP. Đây là “nhịp tim” của hệ thống.

---

## Phase 3 – Tạo `platform-infrastructure` (implementation cho Core)

🎯 Mục tiêu: có `MasterDbContext`, Tenancy middleware, EventBus in-process, DocEngine.

### 3.1. Tạo solution & project

☑️ Trong `platform-infrastructure`:

* [ ] Tạo solution & project:

  ```bash
  dotnet new sln -n PlatformInfrastructure
  dotnet new classlib -n Platform.Infrastructure -o src/Platform.Infrastructure
  dotnet sln add src/Platform.Infrastructure/Platform.Infrastructure.csproj
  ```
* [ ] Thêm reference tới `Platform.Core` & `Platform.Shared`
  (sau này khi build bằng package thì đổi sang `PackageReference`):

  ```bash
  dotnet add src/Platform.Infrastructure/Platform.Infrastructure.csproj reference \
    ../platform-core/src/Platform.Core/Platform.Core.csproj \
    ../platform-core/src/Platform.Shared/Platform.Shared.csproj
  ```

  (ở giai đoạn đầu bạn có thể để `platform-core` ở cùng cấp để test)

### 3.2. MasterDb – lưu Tenants & Hooks

☑️ Trong `Platform.Infrastructure`:

* [ ] Folder `MasterDb/`:

  * `MasterDbContext.cs`
  * `Entities/TenantEntity.cs`
  * `Entities/HookConfigEntity.cs`
  * `Migrations/` (dùng `dotnet ef migrations add InitMasterDb`)
* [ ] Cấu hình EF Core với SQL Server.

Giải thích:
MasterDb giữ thông tin:

* Tenant nào?
* DB connection string của tenant?
* Tenant bật module nào?
* Hook nào đang active?

### 3.3. Tenancy & DocEngine & Events

☑️ Folder `Tenancy/`:

* [ ] `HttpTenantResolver.cs` (đọc từ subdomain/header)
* [ ] `TenantMiddleware.cs` (gắn `TenantInfo` vào HttpContext)
* [ ] `TenantAccessor.cs` (lấy current tenant từ HttpContext)

☑️ Folder `Docs/`:

* [ ] `GenericDocEngine.cs`:

  * đọc `DocTypeDefinition` từ provider
  * validate data theo metadata
  * lưu vào bảng `Docs` (trong tenant DB, tạm thời bạn có thể mock – sau sẽ nối với module)

☑️ Folder `DocTypes/`:

* [ ] `MasterDocTypeProvider.cs`:

  * đọc DocType từ MasterDb hoặc từ tenant DB (sau này)

☑️ Folder `Events/`:

* [ ] `InProcessEventBus.cs`:

  * publish event
  * hỏi `HookRegistry` để biết handler nào cần gọi
* [ ] `HookRegistry.cs`:

  * đọc `HookDefinition` từ MasterDb
* [ ] (Optional) `Outbox/` – để sau cũng được.

---

## Phase 4 – Tạo module đầu tiên: `module-billing`

🎯 Mục tiêu: 1 module đơn giản có:

* `Billing.Module` (IModule)
* `Billing.Domain`
* `Billing.Application`
* `Billing.Infrastructure`

### 4.1. Tạo solution & projects

☑️ Trong repo `module-billing`:

```bash
dotnet new sln -n Billing
dotnet new classlib -n Billing.Module -o src/Billing.Module
dotnet new classlib -n Billing.Domain -o src/Billing.Domain
dotnet new classlib -n Billing.Application -o src/Billing.Application
dotnet new classlib -n Billing.Infrastructure -o src/Billing.Infrastructure
dotnet sln add src/Billing.*/*.csproj
```

☑️ Wiring cơ bản:

* `Billing.Module` reference:

  * `Platform.Core`
* `Billing.Application` reference:

  * `Billing.Domain`
  * `Platform.Core`
* `Billing.Infrastructure` reference:

  * `Billing.Domain`
  * `Platform.Infrastructure` (sau này)
  * EF Core (nếu cần)

### 4.2. Định nghĩa IModule & implementation

☑️ Trong `Platform.Core` thêm interface:

```csharp
public interface IModule
{
    string Key { get; } // "billing"
    void ConfigureServices(IServiceCollection services);
    void ConfigureEndpoints(IEndpointRouteBuilder endpoints);
}
```

☑️ Trong `Billing.Module/BillingModule.cs`:

* [ ] Implement `IModule`
* [ ] Đăng ký service cần thiết (Application, Infrastructure)
* [ ] Đăng ký DocType seed file path (VD `BillingDocTypes/Invoice.doc.json`)

### 4.3. Tạo DocType cho Invoice

☑️ Trong `Billing.Module/BillingDocTypes/Invoice.doc.json`:

* [ ] Tạo file JSON mô tả 1 DocType đơn giản:

  * fields: `customer_name`, `amount`, `currency`
  * required: `customer_name`, `amount`

Giải thích:
DocType này sẽ được seed vào tenant DB khi tạo tenant.

---

## Phase 5 – Tạo `platform-host` (Web API monolith)

🎯 Mục tiêu: chạy được ASP.NET Core Web API, có:

* TenantMiddleware
* Load module Billing
* 1 controller generic `DocsController`

### 5.1. Tạo solution & project WebHost

☑️ Trong repo `platform-host`:

```bash
dotnet new sln -n PlatformHost
dotnet new webapi -n WebHost -o src/Host/WebHost
dotnet sln add src/Host/WebHost/WebHost.csproj
```

☑️ Thêm reference (lúc dev bạn dùng project ref, sau này chuyển sang NuGet):

* `Platform.Core`
* `Platform.Infrastructure`
* `Billing.Module` (module đầu tiên)

### 5.2. Đăng ký module & middleware

☑️ Trong `Program.cs` hoặc `StartupExtensions`:

* [ ] Đăng ký `TenantMiddleware`
* [ ] Scan assembly để tìm `IModule`:

  * instantiate `BillingModule`
  * gọi `ConfigureServices`
* [ ] Sau `app.UseRouting()`:

  * gọi `module.ConfigureEndpoints(app)` cho từng module.

### 5.3. Tạo `DocsController` generic

☑️ Controller `DocsController`:

* Route: `api/{moduleKey}/docs/{docType}`
* Action:

  * `POST` → gọi `IDocEngine.CreateAsync`
  * Lấy `TenantInfo` từ `TenantAccessor`
  * ModuleKey lấy từ route

Mục tiêu nhỏ:
Gọi được `POST /api/billing/docs/Invoice` với body JSON simple, tạm thời chỉ return lại input là ok.

---

## Phase 6 – Tạo `tenant-cli` (CLI tạo tenant)

🎯 Mục tiêu: 1 console app đơn giản với lệnh:
`tenant-cli new-tenant --code tenantA --modules billing`

### 6.1. Tạo project CLI

☑️ Trong repo `tenant-cli`:

```bash
dotnet new console -n Tenant.Cli -o src/Tenant.Cli
dotnet new sln -n TenantCli
dotnet sln add src/Tenant.Cli/Tenant.Cli.csproj
```

☑️ Thêm reference:

* `Platform.Core`
* `Platform.Infrastructure`
* `Billing.Module` (để lấy migrator & DocTypes seed)

### 6.2. Skeleton command NewTenant

☑️ Tạo folder:

* `Commands/NewTenantCommand.cs`
* `Services/TenantDbProvisioner.cs`
* `Services/ModuleMigratorRegistry.cs`

Ý tưởng đơn giản (version 0):

* `NewTenantCommand`:

  * tạo record tenant trong MasterDb
  * **chưa cần** auto tạo DB tenant, bạn có thể tạo tay cho dễ
* Version nâng cấp:

  * dùng `TenantDbProvisioner` để:

    * create database
    * apply migrations cho module billing
    * seed DocType từ `BillingDocTypes/*.json`

---

## Phase 7 – Tạo `platform-dev` integration repo với git submodule

🎯 Mục tiêu: có 1 repo “tổng” để mở solution dev all-in-one.

### 7.1. Tạo repo & add submodule

☑️ Trong máy local:

```bash
git init platform-dev
cd platform-dev

git submodule add git@github.com:your-org/platform-core.git src/platform-core
git submodule add git@github.com:your-org/platform-infrastructure.git src/platform-infrastructure
git submodule add git@github.com:your-org/module-billing.git src/module-billing
git submodule add git@github.com:your-org/platform-host.git src/platform-host
git submodule add git@github.com:your-org/tenant-cli.git src/tenant-cli
```

☑️ Tạo solution tổng:

```bash
dotnet new sln -n YourCompany.Platform.Dev

# add tất cả csproj cần thiết vào solution
dotnet sln add \
  src/platform-core/src/Platform.Core/Platform.Core.csproj \
  src/platform-core/src/Platform.Shared/Platform.Shared.csproj \
  src/platform-infrastructure/src/Platform.Infrastructure/Platform.Infrastructure.csproj \
  src/module-billing/src/Billing.Module/Billing.Module.csproj \
  src/platform-host/src/Host/WebHost/WebHost.csproj \
  src/tenant-cli/src/Tenant.Cli/Tenant.Cli.csproj
```

Sau đó:

* Mở solution này lên
* Đổi các `PackageReference` thành `ProjectReference` khi dev local (cho dễ debug).

---

## Phase 8 – Mục tiêu “hello world” end-to-end

Cuối cùng, hãy đặt 1 **checkpoint cụ thể**:

☑️ **Goal v1:**

* [ ] Chạy `tenant-cli new-tenant --code demo --modules billing` (có thể fake 1 phần, chưa cần full migration tự động).
* [ ] Chạy **WebHost**.
* [ ] Gửi request:

  ```http
  POST /api/billing/docs/Invoice
  Header: X-Tenant-Code: demo
  Body: { "customer_name": "John", "amount": 100 }
  ```
* [ ] API validate theo DocType (required `customer_name`, `amount`) và trả về JSON đã lưu.

Khi bạn đạt được goal này, nghĩa là:

* Tenancy hoạt động (tenant demo).
* Module Billing được load.
* Metadata DocType chạy.
* DocEngine + EventBus (dù event chưa làm gì nhiều) hoạt động.


