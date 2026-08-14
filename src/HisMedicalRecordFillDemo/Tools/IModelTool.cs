using System.Text.Json.Nodes;
using HisMedicalRecordFillDemo.Models;

namespace HisMedicalRecordFillDemo.Tools;

public interface IModelTool
{
    string Name { get; }

    Task<ToolExecutionResult> ExecuteAsync(
        JsonObject arguments,
        ToolExecutionContext context,
        CancellationToken cancellationToken);
}
