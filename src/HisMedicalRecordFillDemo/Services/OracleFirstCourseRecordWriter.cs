using HisMedicalRecordFillDemo.Models;
using HisMedicalRecordFillDemo.Options;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace HisMedicalRecordFillDemo.Services;

public sealed class OracleFirstCourseRecordWriter(
    IXmlRecordValidator xmlRecordValidator,
    IOptions<DatabaseOptions> options) : IFirstCourseRecordWriter
{
    private readonly string _connectionString = options.Value.OracleConnectionString;

    public async Task<RecordWriteResult> WriteAsync(string xml, ToolExecutionContext context, CancellationToken cancellationToken)
    {
        xmlRecordValidator.Validate(xml, context.SchemaPath);
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("Oracle 模式缺少 Database__OracleConnectionString。 ");

        await using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new OracleCommand("PKG_FIRST_COURSE.UPSERT_RECORD", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure,
            BindByName = true
        };
        command.Parameters.Add("P_PATIENT_ID", OracleDbType.Varchar2).Value = context.PatientId;
        command.Parameters.Add("P_VISIT_ID", OracleDbType.Varchar2).Value = context.VisitId;
        command.Parameters.Add("P_RECORD_XML", OracleDbType.Clob).Value = xml;
        var resultParameter = command.Parameters.Add("P_RESULT", OracleDbType.Varchar2, 100);
        resultParameter.Direction = System.Data.ParameterDirection.Output;

        await command.ExecuteNonQueryAsync(cancellationToken);
        var result = resultParameter.Value?.ToString();
        if (result != "SUCCESS")
            throw new InvalidOperationException($"Oracle 首次病程回写失败：{result ?? "未返回结果"}。 ");

        return new RecordWriteResult(
            $"oracle://FIRST_COURSE_RECORD/{context.PatientId}/{context.VisitId}",
            "oracle-upserted");
    }
}
