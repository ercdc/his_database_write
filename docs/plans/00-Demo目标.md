# Demo Goal：DeepSeek Tool Calling 生成首次病程 XML

## 目标陈述

在 `his数据病历填充` 目录内完成一个可本地运行的 C# Web API Demo。请求携带 `patientId` 与 `visitId` 后，服务从本地模拟 HIS 数据文件读取原始文本；DeepSeek 必须通过 `write_first_course_xml` Tool Calling 返回完整首次病程 XML；C# 验证 XML 后将其保存到本地文件。

## 完成条件

以下条件全部满足才视为完成：

1. `POST /api/first-course/generate` 是唯一业务接口，入参只有 `patientId`、`visitId`。
2. 只读取本地 Fixture，不访问 HIS、Oracle 或其他数据库。
3. DeepSeek 调用使用真实 Chat Completions Tool Calling 协议；模型返回工具调用、C# 执行工具、再回传 tool result 的两轮链路完整存在。
4. 只注册 `write_first_course_xml` 一个工具，并严格拒绝不匹配的工具调用。
5. 工具参数的 XML 使用 `Resources/Skills/FirstCourse/schema.xsd` 校验后才写入 `Output/`。
6. 使用 `CASE-001` 和 `TEST20260811001` 的真实 API 联调可产生一个通过 XSD 的 XML 文件。
7. 自动测试覆盖不依赖真实 API Key 的关键路径并通过。

## 不属于本 Goal

- HIS SQL、数据库连接或数据库回写；
- 鉴权、调用方身份识别、审计、重试和队列；
- 入院病历、日常病程记录及任何通用 Agent 框架；
- HIS 关键信息缺失时的补全或降级策略。

## 固定决策

- 使用 ASP.NET Core Web API 与 .NET 内置 `HttpClient`，直连 DeepSeek OpenAI 兼容接口。
- 本地样例文件按 `{patientId}_{visitId}.txt` 命名。
- XML 目标格式采用既有的 `<首次病程记录>` 契约，而非 `zlxml` 格式。
- `Resources/Skills/FirstCourse/template.xml` 是模型用结构参考；同目录的 `schema.xsd` 才是最终输出的校验依据。
