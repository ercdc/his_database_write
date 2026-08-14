# HIS 数据首次病程 XML 生成 Demo：开发计划

## 目标

实现一个 ASP.NET Core Web API Demo：接收 `patientId`、`visitId`，从本地样例文件取得模拟 HIS 原始数据，调用 DeepSeek Chat Completions Tool Calling，将模型生成的首次病程 XML 写入本地文件。

## 边界

本 Demo 不连接 HIS 或 Oracle，不实现鉴权、身份识别、数据库回写、多业务类型、缺失数据补全或自动重试。只实现“首次病程记录”一条固定流程。

## 契约

### HTTP 接口

`POST /api/first-course/generate`

```json
{ "patientId": "CASE-001", "visitId": "TEST20260811001" }
```

成功响应应包含 `success`、`outputPath` 和 `message`；失败响应应包含 `success: false` 和 `error`。

### 本地 HIS 样例定位

服务按以下命名规则读取样例文件：

```text
Fixtures/{patientId}_{visitId}.txt
```

文件文本保持未来生产环境“病案、医嘱、费用、报告”多段 SQL 结果直接拼接的形态，不在 Demo 内预处理为 JSON。

### 唯一模型工具

名称：`write_first_course_xml`

参数：`{ "xml": "完整首次病程记录 XML" }`

处理：解析 XML → 基于 XSD 校验 → 写入 `Output/` → 返回文件路径。

## 实施阶段

1. 创建 `.NET 8` Web API 项目与基础目录，配置 `appsettings.Development.json` 以读取 DeepSeek API Key 与模型名（不提交真实 Key）。
2. 定义请求/响应 DTO、本地样例读取服务和 XML 文件写入服务。
3. 实现 DeepSeek Chat Completions 客户端：以 `HttpClient` 调用 OpenAI 兼容 API，发送提示词、XML 模板、样例 HIS 数据和严格模式工具定义。
4. 实现固定两轮 Tool Calling：第一轮取得 `write_first_course_xml` 调用；C# 执行工具；第二轮发送 tool result 供模型确认。仅接受一次且名称匹配的工具调用。
5. 实现控制器，串起“输入 → Fixture → DeepSeek → 工具 → 输出文件”的流程。
6. 编写单元测试：覆盖 Fixture 定位、工具参数反序列化、非法 XML/XSD 拒绝、合法 XML 写入。真实 DeepSeek 联调单独手工执行。
7. 使用 `CASE-001 / TEST20260811001` 真正调用一次 DeepSeek，验证输出文件存在、根节点为 `首次病程记录`、通过 XSD 且 HTTP 响应返回路径。

## 成功标准

- API 能读取 `Fixtures/CASE-001_TEST20260811001.txt`。
- DeepSeek 的第一轮响应包含 `write_first_course_xml` 工具调用。
- 工具参数中的 XML 可解析并通过 `Resources/Skills/FirstCourse/schema.xsd`。
- 生成的文件保存至 `Output/`，不会覆盖已有文件。
- 不出现数据库访问、HIS 网络访问和第二个模型工具。

## 实施进展（2026-08-12）

- 已完成：.NET 8 Web API、唯一 `POST /api/first-course/generate` 接口、本地 Fixture 读取、DeepSeek 两轮 Tool Calling、XSD 校验和本地 XML 写入。
- 已完成：6 个不依赖真实 Key 的自动测试，覆盖 Fixture、XSD、唯一工具定义和两轮 Tool Calling 模拟协议。
- 已完成：使用有效 Key 的真实 DeepSeek Tool Calling 联调和实际 HTTP 接口联调均已通过；生成文件已写入 `Output/`，并已由服务层 XSD 校验。

## 当前静态资源

- `Resources/Skills/FirstCourse/`：首次病程的 `skill.json`、模型规则、XML 结构骨架和最终 XSD 契约。
- `Resources/Tools/write_first_course_xml.json`：给 DeepSeek 的工具声明。
- `Fixtures/CASE-001_TEST20260811001.txt`：脱敏模拟 HIS 数据。
