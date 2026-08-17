# Oracle Demo 数据库接入任务

## 目标

在本地 Docker Oracle Free 实例中创建脱敏 HIS 演示数据，使 Web API 可按 `patientId`、`visitId` 从 Oracle 读取 HIS 多段文本，并在模型生成 XML 后将首次病程写回 Oracle。

## 阶段

1. **完成**：确认 Docker Oracle 运行条件，建立本地实例与初始化 SQL。
2. **完成**：实现 Oracle 数据读取与首次病程回写适配器。
3. **完成**：保留 Fixture/本地文件模式，增加数据库模式配置与测试。
4. **完成**：运行真实 HTTP + DeepSeek + Oracle 全链路验证。

## 固定决策

- 使用 Docker 中的 Oracle Free 作为本地 Demo 数据库，不安装宿主机 Oracle Server。
- 使用 `Oracle.ManagedDataAccess.Core` 连接 Oracle。
- 继续保留 XML XSD 校验；模型永远不直接连接数据库。
