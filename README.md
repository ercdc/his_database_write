# HIS 数据首次病程 XML 生成 Demo

该 Demo 提供唯一业务接口：接收患者 ID 和就诊 ID，读取本地模拟 HIS 文本，调用 DeepSeek Tool Calling 生成首次病程 XML；仅在 XML 通过 XSD 后写入本地 `Output/`。

默认使用本地 Fixture 与 XML 文件输出；设置 Oracle 模式后，可读取本地 Oracle Demo 数据库并回写首次病程 XML。项目仍不包含鉴权、多业务类型或缺失数据补全。

项目方案、设计记录和操作命令见 [docs/README.md](./docs/README.md)。

Oracle 本地 Demo 的容器、连接信息与启动命令见 [docs/operations/Oracle本地Demo数据库.md](./docs/operations/Oracle本地Demo数据库.md)。

## 运行

需要 .NET 8 SDK / Runtime 与 DeepSeek API Key。

```powershell
$env:DEEPSEEK_API_KEY = "你的 DeepSeek API Key"
dotnet run --project .\src\HisMedicalRecordFillDemo
```

也可以用环境变量设置模型或地址（默认使用 DeepSeek strict Tool Calling Beta 地址）：

```powershell
$env:DeepSeek__BaseUrl = "https://api.deepseek.com/beta"
$env:DeepSeek__Model = "deepseek-v4-flash"
```

调用接口：

```powershell
Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5000/api/first-course/generate" `
  -ContentType "application/json" `
  -Body '{"patientId":"CASE-001","visitId":"TEST20260811001"}'
```

成功后，响应内的 `outputPath` 指向生成的 XML；默认保存在 `src/HisMedicalRecordFillDemo/Output/`。

## 核心约束

- 样例按 `Fixtures/{patientId}_{visitId}.txt` 定位，读取的是“病案、医嘱、费用、报告”的原始拼接文本。
- 第一轮 DeepSeek 请求只注册并强制调用 `write_first_course_xml`。
- DeepSeek 默认开启思考模式；为支持强制 `tool_choice`，请求固定传入 `thinking: { type: "disabled" }`。
- C# 解析工具参数、校验 `Resources/Skills/FirstCourse/schema.xsd`，成功后写文件。
- 第二轮只回传工具执行结果，请模型确认完成；不会允许再次调用工具。

## 分层结构

```text
Resources/
├─ Skills/FirstCourse/       # 首次病程的业务资源
│  ├─ skill.json             # 资源清单、允许工具、必调工具
│  ├─ prompt.md              # 模型生成规则
│  ├─ template.xml           # XML 结构参考
│  └─ schema.xsd             # 该业务的最终 XML 契约
└─ Tools/                    # 给 DeepSeek 的工具 JSON Schema
   └─ write_first_course_xml.json

src/HisMedicalRecordFillDemo/
├─ Skills/                   # 读取并组装业务上下文
├─ Tools/                    # C# 工具执行器和注册表
└─ Services/DeepSeekToolCallingClient.cs # 通用两轮 Tool Calling 协议
```

一个工具有两部分，职责明确分离：

- `Resources/Tools/*.json`：给 DeepSeek 看，包括工具名、描述、参数 JSON Schema。
- `src/.../Tools/*.cs`：由 C# 执行，包括参数校验、XSD 校验、写文件等真实操作。

要增加新业务时，新建一个 `Resources/Skills/<业务名>/` 目录、对应的 `IRecordSkill` 实现，以及新业务需要的工具声明和执行器；`DeepSeekToolCallingClient` 不需要修改。

## 验证

```powershell
dotnet test .\HisMedicalRecordFillDemo.sln
```

测试不会调用真实 DeepSeek API；它通过模拟的两轮 API 响应验证请求协议、唯一工具调用、XSD 校验与文件写入。

如需执行真实 API 联调（会消耗 DeepSeek 额度），额外设置：

```powershell
$env:RUN_DEEPSEEK_INTEGRATION_TEST = "true"
dotnet test .\HisMedicalRecordFillDemo.sln --filter "Category=Integration"
```
