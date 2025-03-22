# 🧠 TaskManager - ASP.NET Core + Redis + SQL Server

一個任務管理系統，使用 ASP.NET Core MVC 架構，整合 Redis 快取與 SQL Server 資料庫，並支援 Docker Compose 一鍵部署。

---

## 🚀 專案特色

- ✅ RESTful API 設計
- ✅ 使用 Redis 快取任務資料
- ✅ 使用 Entity Framework Core 操作 SQL Server
- ✅ 提供 Swagger UI 方便測試
- ✅ 支援前端頁面 (index.html)
- ✅ 使用 Docker Compose 管理多容器部署

---

## 📁 專案架構

```
TaskManager/
├── Controllers/
├── Models/
├── Views/
├── Data/
├── wwwroot/
├── Dockerfile
├── docker-compose.yml
├── appsettings.json
└── README.md
```

---

## ⚙️ 使用技術

| 技術 | 說明 |
|------|------|
| ASP.NET Core 8.0 | Web API 與 MVC 架構 |
| Entity Framework Core | ORM 操作資料庫 |
| SQL Server | 儲存任務資料 |
| Redis | 快取任務清單與單筆任務 |
| Docker / Docker Compose | 輕鬆部署多服務 |
| Swagger | API 文件與測試介面 |

---

## 🧪 API 測試

啟動後，使用 Swagger UI 存取：

```
http://localhost:5000/swagger
```

你可以：
- 建立任務
- 查詢任務清單 / 指定 ID 任務
- 編輯 / 刪除任務

---

## 🐳 快速啟動（使用 Docker）

### ✅ 第一步：建構並啟動所有服務

```bash
docker-compose up --build
```

服務會包含：

- ASP.NET Core API（port 5000）
- SQL Server（port 11433）
- Redis（port 6379）

---

### ✅ 第二步：打開瀏覽器

- Swagger API: http://localhost:5000/swagger
- 前端頁面: http://localhost:5000/index.html

---

## 🛠 環境設定（可選）

若你本機要開發，請確保 `appsettings.Development.json` 包含正確的連線字串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TaskManagerDB;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}
```

---

## 📦 Migrations 資料表建立

如需建立資料表，請執行以下指令：

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## 📜 License

MIT License © 2025 YourName
