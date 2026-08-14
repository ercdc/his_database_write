using System.Text.Json;
using System.Text.Json.Nodes;
using HisMedicalRecordFillDemo.Models;
using HisMedicalRecordFillDemo.Services;
using HisMedicalRecordFillDemo.Tools;

namespace HisMedicalRecordFillDemo.Skills;

public sealed class FirstCourseSkill(
    IHostEnvironment environment,
    ToolDefinitionLoader toolDefinitionLoader) : IRecordSkill
{
    public string Id => "first-course";

    public async Task<SkillContext> BuildContextAsync(string hisData, CancellationToken cancellationToken)
    {
        var skillDirectory = Path.Combine(DemoPathResolver.GetRoot(environment), "Resources", "Skills", "FirstCourse");
        var definition = await ReadDefinitionAsync(skillDirectory, cancellationToken);
        if (definition.Id != Id)
            throw new InvalidOperationException($"技能定义 ID 必须是 {Id}。");
        if (!definition.Tools.Contains(definition.RequiredTool, StringComparer.Ordinal))
            throw new InvalidOperationException("requiredTool 必须存在于 tools 列表中。");

        var prompt = await File.ReadAllTextAsync(Path.Combine(skillDirectory, definition.PromptFile), cancellationToken);
        var template = await File.ReadAllTextAsync(Path.Combine(skillDirectory, definition.TemplateFile), cancellationToken);
        var toolDefinitions = await toolDefinitionLoader.LoadManyAsync(definition.Tools, cancellationToken);

        return new SkillContext(
            definition.Id,
            new JsonArray
            {
                new JsonObject { ["role"] = "system", ["content"] = prompt },
                new JsonObject { ["role"] = "user", ["content"] = $"【XML 模板】\n{template}\n\n【HIS 原始数据】\n{hisData}" }
            },
            toolDefinitions,
            new HashSet<string>(definition.Tools, StringComparer.Ordinal),
            definition.RequiredTool,
            Path.Combine(skillDirectory, definition.SchemaFile),
            Path.Combine(DemoPathResolver.GetRoot(environment), "Output"),
            "首次病程记录");
    }

    private static async Task<SkillDefinition> ReadDefinitionAsync(string skillDirectory, CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync(Path.Combine(skillDirectory, "skill.json"), cancellationToken);
        return JsonSerializer.Deserialize<SkillDefinition>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("skill.json 不能为空。");
    }
}
