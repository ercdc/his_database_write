# Oracle Demo 数据库接入：发现记录

- 2026-08-14：医院 HIS 数据库类型已确认是 Oracle。
- 2026-08-14：本机未发现 Oracle Windows 服务或 `sqlplus`；已发现 Docker CLI。
- 2026-08-14：Docker Desktop 已启动并确认引擎版本为 29.1.3，可用 `desktop-linux` context。
- 2026-08-14：采用 `gvenzl/oracle-free:23-slim-faststart` 作为本地 Oracle Free Demo 数据库；初始化脚本包含 HIS 四段数据、首次病程 XML 表和 UPSERT 存储过程。
