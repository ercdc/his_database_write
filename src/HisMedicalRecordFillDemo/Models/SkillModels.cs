using System.Text.Json.Nodes;

namespace HisMedicalRecordFillDemo.Models;

public sealed record SkillDefinition(
    string Id,
    string PromptFile,
    string TemplateFile,
    string SchemaFile,
    string RequiredTool,
    string[] Tools);

public sealed record SkillContext(
    string SkillId,
    JsonArray Messages,
    IReadOnlyList<JsonObject> ToolDefinitions,
    IReadOnlySet<string> AllowedToolNames,
    string RequiredToolName,
    string SchemaPath,
    string OutputDirectory,
    string OutputFilePrefix);

public sealed record ToolExecutionContext(
    string SkillId,
    string PatientId,
    string VisitId,
    string SchemaPath,
    string OutputDirectory,
    string OutputFilePrefix);

public sealed record ToolExecutionResult(string Content, string? OutputPath);

public sealed record ToolCallingResult(ToolExecutionResult ToolResult, string Confirmation);
