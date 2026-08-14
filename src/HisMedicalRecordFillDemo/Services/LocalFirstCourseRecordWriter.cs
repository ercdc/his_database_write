using HisMedicalRecordFillDemo.Models;

namespace HisMedicalRecordFillDemo.Services;

public sealed class LocalFirstCourseRecordWriter(IXmlRecordValidator xmlRecordValidator, IHostEnvironment environment) : IFirstCourseRecordWriter
{
    public async Task<RecordWriteResult> WriteAsync(string xml, ToolExecutionContext context, CancellationToken cancellationToken)
    {
        xmlRecordValidator.Validate(xml, context.SchemaPath);
        var outputDirectory = Path.Combine(DemoPathResolver.GetRoot(environment), "Output");
        Directory.CreateDirectory(outputDirectory);
        var fileName = $"{context.OutputFilePrefix}_{context.PatientId}_{context.VisitId}_{DateTimeOffset.Now:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}.xml";
        var fullPath = Path.Combine(outputDirectory, fileName);
        await File.WriteAllTextAsync(fullPath, xml, cancellationToken);
        return new RecordWriteResult(fullPath, "local-file-created");
    }
}
