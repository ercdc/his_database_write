using HisMedicalRecordFillDemo.Options;
using HisMedicalRecordFillDemo.Services;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace HisMedicalRecordFillDemo.Tests;

[Trait("Category", "OracleIntegration")]
public sealed class OracleDatabaseIntegrationTests
{
    private const string ConnectionString = "User Id=HIS_DEMO;Password=HisDemo_2026;Data Source=localhost:1521/FREEPDB1";

    [Fact]
    public async Task Oracle适配器_读取HIS并回写首次病程XML()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_ORACLE_INTEGRATION_TEST"), "true", StringComparison.OrdinalIgnoreCase))
            return;

        var options = Microsoft.Extensions.Options.Options.Create(new DatabaseOptions
        {
            Mode = "Oracle",
            OracleConnectionString = ConnectionString
        });
        var provider = new OracleHisEncounterDataProvider(options);
        var writer = new OracleFirstCourseRecordWriter(new XmlRecordValidator(), options);

        var hisData = await provider.GetRawDataAsync("CASE-001", "TEST20260811001", CancellationToken.None);
        var writeResult = await writer.WriteAsync(
            LocalFirstCourseRecordWriterTests.ValidXml,
            LocalFirstCourseRecordWriterTests.CreateContext(),
            CancellationToken.None);

        Assert.Contains("【病案】", hisData);
        Assert.Contains("【医嘱】", hisData);
        Assert.Contains("【费用】", hisData);
        Assert.Contains("【报告】", hisData);
        Assert.Equal("oracle-upserted", writeResult.Operation);
        Assert.StartsWith("oracle://FIRST_COURSE_RECORD/CASE-001/TEST20260811001", writeResult.Location);

        await using var connection = new OracleConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new OracleCommand(
            "SELECT RECORD_XML FROM FIRST_COURSE_RECORD WHERE PATIENT_ID = 'CASE-001' AND VISIT_ID = 'TEST20260811001'",
            connection);
        var savedXml = Convert.ToString(await command.ExecuteScalarAsync());
        Assert.Contains("<首次病程记录>", savedXml);
    }
}
