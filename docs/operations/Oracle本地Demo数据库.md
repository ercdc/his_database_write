# Oracle 本地 Demo 数据库

本项目通过 Docker 的 Oracle Free 容器创建本地脱敏 HIS Demo 数据库，不在 Windows 宿主机安装 Oracle Server。

## 连接信息

| 项目 | 值 |
|---|---|
| 主机 | `localhost` |
| 端口 | `1521` |
| Service Name | `FREEPDB1` |
| 用户 | `HIS_DEMO` |
| 密码 | `HisDemo_2026` |
| C# 连接串 | `User Id=HIS_DEMO;Password=HisDemo_2026;Data Source=localhost:1521/FREEPDB1` |

仅用于本地 Demo，不能用于医院环境。

## 数据模型

- `HIS_ENCOUNTER`：病案与患者/就诊基本信息。
- `HIS_MEDICAL_ORDER`：医嘱。
- `HIS_CHARGE`：费用。
- `HIS_REPORT`：检验、影像等报告。
- `FIRST_COURSE_RECORD`：首次病程 XML 回写记录。
- `PKG_FIRST_COURSE.UPSERT_RECORD`：首次病程回写存储过程；同一患者和就诊重复写入时更新 XML。

## 启动与初始化

```powershell
docker compose -f .\docker-compose.oracle-demo.yml up -d
Get-Content -Raw .\oracle\init-demo.sql |
  docker exec -i his-oracle-demo sqlplus -s HIS_DEMO/HisDemo_2026@//localhost:1521/FREEPDB1
```

## 数据库适配器集成测试

```powershell
$env:RUN_ORACLE_INTEGRATION_TEST = "true"
dotnet test .\HisMedicalRecordFillDemo.sln --filter "Category=OracleIntegration"
```

## 以 Oracle 模式启动 Web 服务

```powershell
$env:DEEPSEEK_API_KEY = "你的 DeepSeek API Key"
$env:Database__Mode = "Oracle"
$env:Database__OracleConnectionString = "User Id=HIS_DEMO;Password=HisDemo_2026;Data Source=localhost:1521/FREEPDB1"

dotnet run --project .\src\HisMedicalRecordFillDemo `
  --urls "http://127.0.0.1:5088"
```

此模式下，请求 `POST /api/first-course/generate` 会从 Oracle 读取病案、医嘱、费用、报告，并由 `PKG_FIRST_COURSE.UPSERT_RECORD` 将通过 XSD 校验的 XML 写入 `FIRST_COURSE_RECORD`。
