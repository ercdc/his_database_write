namespace HisMedicalRecordFillDemo.Tools;

public sealed class ToolRegistry(IEnumerable<IModelTool> tools)
{
    private readonly IReadOnlyDictionary<string, IModelTool> _tools = tools.ToDictionary(tool => tool.Name, StringComparer.Ordinal);

    public IModelTool GetRequired(string name) =>
        _tools.TryGetValue(name, out var tool)
            ? tool
            : throw new InvalidOperationException($"未注册工具：{name}。");
}
