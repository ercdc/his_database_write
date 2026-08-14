using HisMedicalRecordFillDemo.Models;

namespace HisMedicalRecordFillDemo.Services;

public interface IFirstCourseRecordWriter
{
    Task<RecordWriteResult> WriteAsync(string xml, ToolExecutionContext context, CancellationToken cancellationToken);
}

public sealed record RecordWriteResult(string Location, string Operation);
