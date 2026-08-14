using System.Text.Json.Nodes;
using HisMedicalRecordFillDemo.Exceptions;
using HisMedicalRecordFillDemo.Models;
using HisMedicalRecordFillDemo.Services;

namespace HisMedicalRecordFillDemo.Tools;

public sealed class WriteFirstCourseXmlTool(IXmlRecordWriter xmlRecordWriter) : IModelTool
{
    public string Name => "write_first_course_xml";

    public async Task<ToolExecutionResult> ExecuteAsync(
        JsonObject arguments,
        ToolExecutionContext context,
        CancellationToken cancellationToken)
    {
        if (arguments.Count != 1 || arguments["xml"] is null)
            throw new ToolCallingException("write_first_course_xml 参数必须且只能包含 xml。");

        var xml = arguments["xml"]!.GetValue<string>();
        var outputPath = await xmlRecordWriter.WriteAsync(xml, context, cancellationToken);
        return new ToolExecutionResult(
            Content: System.Text.Json.JsonSerializer.Serialize(new { success = true, outputPath }),
            OutputPath: outputPath);
    }
}
