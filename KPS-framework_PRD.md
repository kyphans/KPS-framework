                    ┌───────────────────────────────────────────┐
                    │               Web Host (API/UI)           │
                    │        ASP.NET Core Modular Monolith      │
                    └───────────────────────────────────────────┘
                       ▲               ▲              ▲
                       │               │              │
                ┌──────┴──────┐  ┌─────┴─────┐  ┌─────┴─────┐
                │  Module A   │  │ Module B  │  │ Module C  │
                │ (Billing)   │  │  (HR)     │  │(Inventory)│
                └─────────────┘  └───────────┘  └───────────┘
                       ▲               ▲              ▲
                       └───────── In-process Event Bus ┘
                                   + Hook Registry

   ┌────────────────────────────────────────────────────────────────┐
   │                    Master / Control Plane DB                   │
   │  - Tenants (id, name, db_connection_string, enabled_modules…)  │
   │  - Global DocType templates                                    │
   │  - Hook registry (event → handlers per module/tenant)          │
   └────────────────────────────────────────────────────────────────┘

   ┌────────────────────┐   ┌────────────────────┐   ┌────────────────────┐
   │ TenantA_DB         │   │ TenantB_DB         │   │ TenantC_DB         │
   │  - Docs            │   │  - Docs            │   │  - Docs            │
   │  - DocTypes (override)│ │  - DocTypes       │   │  - DocTypes       │
   │  - Module tables    │  │  - Module tables   │  │  - Module tables   │
   └────────────────────┘   └────────────────────┘   └────────────────────┘

   CLI:
      tenant-cli new-tenant --modules billing,hr --db-type sqlserver
      └─> tạo DB tenant, chạy migrations module, seed DocType & hooks


YÊU CẦU CHUNG

- Metadata-driven + DocType

Mỗi module define DocType (metadata). Tenant có thể override trong tenant DB.

DocEngine đọc DocType -> validate, lưu JSON vào bảng Docs.

- Multi-tenant – mỗi tenant 1 DB

Master DB lưu danh sách tenant + connection string + enabled modules.

Middleware resolve tenant từ domain/header → set TenantInfo → DbContext dùng connection đó.

- Multi-app – module + monolith

Monolith ASP.NET Core, nhiều module (Billing, HR, …) cùng process.

Mỗi module tự cấu hình service, endpoint, migrations, DocType.

- Event-driven – hook registry cross-module

In-process EventBus.

HookRegistry (metadata) map event → handler module khác, có thể per tenant.

- Tenant được tạo bằng CLI

CLI tạo DB tenant, chạy migrations module được chọn, seed DocType + hooks, tạo record Tenant + TenantModules trong Master DB.



YourCompany.Platform.sln
└── src/
    ├── Host/
    │   ├── WebHost/                 # ASP.NET Core Web (API/UI) – entry point
    │   │   ├── Program.cs
    │   │   ├── appsettings.json
    │   │   └── ...
    │   └── Workers/                 # Background worker: outbox, hooks async
    │       ├── Workers.Host.csproj
    │       └── ...
    │
    ├── Platform/                    # Layer nền tảng (dùng chung toàn hệ thống)
    │   ├── Platform.Core/           # Abstractions, shared kernel
    │   │   ├── DocTypes/
    │   │   │   ├── DocTypeDefinition.cs
    │   │   │   ├── DocFieldDefinition.cs
    │   │   │   └── IDocTypeProvider.cs
    │   │   ├── Docs/
    │   │   │   ├── IDocEngine.cs
    │   │   │   └── DocQuery.cs
    │   │   ├── Tenancy/
    │   │   │   ├── TenantInfo.cs
    │   │   │   ├── ITenantResolver.cs
    │   │   │   └── ITenantAccessor.cs
    │   │   ├── Events/
    │   │   │   ├── IEvent.cs
    │   │   │   ├── IEventHandler.cs
    │   │   │   ├── IEventBus.cs
    │   │   │   └── HookDefinition.cs
    │   │   └── Common/
    │   │       ├── Result.cs
    │   │       └── Errors.cs
    │   │
    │   ├── Platform.Infrastructure/ # Triển khai cho Platform.Core
    │   │   ├── MasterDb/
    │   │   │   ├── MasterDbContext.cs      # DB chung: Tenants, Hooks, GlobalDocTypes
    │   │   │   ├── Entities/
    │   │   │   │   ├── TenantEntity.cs
    │   │   │   │   └── HookConfigEntity.cs
    │   │   │   └── Migrations/
    │   │   ├── Tenancy/
    │   │   │   ├── HttpTenantResolver.cs
    │   │   │   ├── TenantMiddleware.cs
    │   │   │   └── TenantAccessor.cs
    │   │   ├── DocTypes/
    │   │   │   ├── MasterDocTypeProvider.cs  # đọc DocType từ master + merge override tenant
    │   │   ├── Docs/
    │   │   │   └── GenericDocEngine.cs       # CRUD Docs + emit event
    │   │   ├── Events/
    │   │   │   ├── InProcessEventBus.cs      # event bus in-process
    │   │   │   ├── HookRegistry.cs           # đọc HookDefinition theo tenant
    │   │   │   └── Outbox/
    │   │   │       ├── OutboxMessageEntity.cs
    │   │   │       └── OutboxProcessor.cs
    │   │   └── Logging/
    │   │       └── ...
    │   │
    │   └── Platform.Shared/         # DTOs, contracts, constants để share cho module
    │       ├── Contracts/
    │       │   ├── Docs/
    │       │   │   └── DocDto.cs
    │       │   └── Tenants/
    │       │       └── TenantDto.cs
    │       └── ...
    │
    ├── Modules/                     # Mỗi module = 1 app (Billing, HR, Inventory…)
    │   ├── Billing/
    │   │   ├── Billing.Module/      # Đăng ký module vào host
    │   │   │   ├── BillingModule.cs     # IModule implement
    │   │   │   ├── BillingDocTypes/     # seed DocType cho module
    │   │   │   │   ├── Invoice.doc.json
    │   │   │   │   └── Payment.doc.json
    │   │   │   ├── Hooks/               # hook handler entry
    │   │   │   │   └── BillingHookRegistration.cs
    │   │   │   └── Billing.Module.csproj
    │   │   │
    │   │   ├── Billing.Domain/      # Domain logic riêng của Billing
    │   │   │   ├── Entities/
    │   │   │   │   ├── Invoice.cs
    │   │   │   │   └── Payment.cs
    │   │   │   ├── ValueObjects/
    │   │   │   └── DomainEvents/
    │   │   │       └── InvoiceCreatedDomainEvent.cs
    │   │   │
    │   │   ├── Billing.Application/ # Application layer: use case, handlers
    │   │   │   ├── Commands/
    │   │   │   │   └── CreateInvoiceCommand.cs
    │   │   │   ├── Queries/
    │   │   │   ├── Services/
    │   │   │   └── EventHandlers/
    │   │   │       └── OnDocCreated_InvoiceHandler.cs  # IEventHandler<DocCreatedEvent>
    │   │   │
    │   │   ├── Billing.Infrastructure/
    │   │   │   ├── TenantDb/
    │   │   │   │   ├── BillingDbContext.cs   # DB riêng của Billing, per-tenant connection
    │   │   │   │   └── Migrations/
    │   │   │   ├── Repositories/
    │   │   │   └── Config/
    │   │   └── Billing.Tests/
    │   │       └── ...
    │   │
    │   ├── HR/
    │   │   ├── HR.Module/
    │   │   │   ├── HRModule.cs
    │   │   │   ├── HRDocTypes/
    │   │   │   │   ├── Employee.doc.json
    │   │   │   │   └── LeaveApplication.doc.json
    │   │   │   ├── Hooks/
    │   │   │   └── HR.Module.csproj
    │   │   ├── HR.Domain/
    │   │   ├── HR.Application/
    │   │   ├── HR.Infrastructure/
    │   │   │   └── TenantDb/
    │   │   │       ├── HRDbContext.cs
    │   │   │       └── Migrations/
    │   │   └── HR.Tests/
    │   │
    │   └── Inventory/
    │       ├── Inventory.Module/
    │       ├── Inventory.Domain/
    │       ├── Inventory.Application/
    │       ├── Inventory.Infrastructure/
    │       │   └── TenantDb/
    │       └── Inventory.Tests/
    │
    └── Tools/
        ├── Tenant.Cli/              # CLI tạo tenant + pick module
        │   ├── Commands/
        │   │   ├── NewTenantCommand.cs
        │   │   ├── ListTenantsCommand.cs
        │   │   └── EnableModuleCommand.cs
        │   ├── Services/
        │   │   ├── TenantDbProvisioner.cs   # tạo DB, chạy migrations module
        │   │   ├── ModuleMigratorRegistry.cs
        │   │   └── SeedDocTypesService.cs
        │   ├── Program.cs            # entry của CLI (System.CommandLine)
        │   └── Tenant.Cli.csproj
        │
        └── Scripts/
            ├── migrate-all-tenants.ps1
            ├── migrate-single-tenant.ps1
            └── dev-init-sample-tenants.ps1


Khối lớn:

platform-core

platform-infrastructure

platform-host

tenant-cli

Module:

module-billing

module-hr

module-inventory

…

Integration repo (dev local):

platform-dev ← chỗ gom submodule (github)