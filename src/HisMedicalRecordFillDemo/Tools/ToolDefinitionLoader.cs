using System.Text.Json.Nodes;
using HisMedicalRecordFillDemo.Services;

namespace HisMedicalRecordFillDemo.Tools;

public sealed class ToolDefinitionLoader(IHostEnvironment environment)
{
    public async Task<IReadOnlyList<JsonObject>> LoadManyAsync(IEnumerable<string> toolNames, CancellationToken cancellationToken)
    {
        var definitions = new List<JsonObject>();
        foreach (var name in toolNames)
        {
            var path = Path.Combine(DemoPathResolver.GetRoot(environment), "Resources", "Tools", $"{name}.json");
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            var definition = JsonNode.Parse(json)?.AsObject()
                ?? throw new InvalidOperationException($"工具声明文件不是 JSON 对象：{path}");
            var declaredName = definition["function"]?["name"]?.GetValue<string>();
            if (declaredName != name)
                throw new InvalidOperationException($"工具声明文件名与 function.name 不一致：{name}。");
            definitions.Add(definition);
        }

        return definitions;
    }
}
