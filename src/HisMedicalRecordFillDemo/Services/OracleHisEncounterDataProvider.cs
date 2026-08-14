using System.Globalization;
using System.Text;
using HisMedicalRecordFillDemo.Exceptions;
using HisMedicalRecordFillDemo.Options;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Microsoft.Extensions.Options;

namespace HisMedicalRecordFillDemo.Services;

public sealed class OracleHisEncounterDataProvider(IOptions<DatabaseOptions> options) : IHisEncounterDataProvider
{
    private readonly string _connectionString = options.Value.OracleConnectionString;

    public async Task<string> GetRawDataAsync(string patientId, string visitId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("Oracle 模式缺少 Database__OracleConnectionString。 ");

        await using var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var encounter = await ReadEncounterAsync(connection, patientId, visitId, cancellationToken)
            ?? throw new FixtureNotFoundException($"Oracle 中未找到就诊记录：{patientId}_{visitId}。 ");
        var orders = await ReadLinesAsync(connection,
            "SELECT ORDER_CONTENT FROM HIS_MEDICAL_ORDER WHERE PATIENT_ID = :patientId AND VISIT_ID = :visitId ORDER BY ORDER_SEQUENCE",
            patientId, visitId, cancellationToken);
        var charges = await ReadLinesAsync(connection,
            "SELECT ITEM_NAME FROM HIS_CHARGE WHERE PATIENT_ID = :patientId AND VISIT_ID = :visitId ORDER BY CHARGE_SEQUENCE",
            patientId, visitId, cancellationToken);
        var reports = await ReadReportsAsync(connection, patientId, visitId, cancellationToken);

        return $"""
            【病案】
            医院：{encounter.HospitalName}
            患者ID：{patientId}
            就诊ID：{visitId}
            姓名：{encounter.PatientName}；性别：{encounter.Gender}；年龄：{encounter.Age}岁；病区：{encounter.WardName}；床号：{encounter.BedNumber}。
            住院号：{encounter.InpatientNo}；入院时间：{encounter.AdmittedAt:yyyy-MM-dd HH:mm}；首次病程记录时间：{encounter.RecordedAt:yyyy-MM-dd HH:mm}。
            发病及病程：{encounter.IllnessOnsetCourse}
            主要症状：{encounter.PrimarySymptoms}
            既往史：{encounter.PastMedicalHistory}
            查体：{encounter.PhysicalExamination}
            诊断依据：{encounter.DiagnosisBasis}
            鉴别诊断：{encounter.DifferentialDiagnosis}
            病例分型：{encounter.CaseClassification}
            带组医师：{encounter.AttendingPhysician}；书写医师：{encounter.AuthorPhysician}。

            【医嘱】
            {string.Join(Environment.NewLine, orders)}

            【费用】
            {string.Join(Environment.NewLine, charges)}

            【报告】
            {string.Join(Environment.NewLine, reports)}
            """;
    }

    private static async Task<EncounterRow?> ReadEncounterAsync(
        OracleConnection connection, string patientId, string visitId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT HOSPITAL_NAME, PATIENT_NAME, GENDER, AGE, WARD_NAME, BED_NUMBER, INPATIENT_NO,
                   ADMITTED_AT, RECORDED_AT, ILLNESS_ONSET_COURSE, PRIMARY_SYMPTOMS, PAST_MEDICAL_HISTORY,
                   PHYSICAL_EXAMINATION, DIAGNOSIS_BASIS, DIFFERENTIAL_DIAGNOSIS, CASE_CLASSIFICATION,
                   ATTENDING_PHYSICIAN, AUTHOR_PHYSICIAN
            FROM HIS_ENCOUNTER
            WHERE PATIENT_ID = :patientId AND VISIT_ID = :visitId
            """;
        await using var command = CreateCommand(connection, sql, patientId, visitId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        return new EncounterRow(
            reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3),
            reader.GetString(4), reader.GetString(5), reader.GetString(6),
            reader.GetDateTime(7), reader.GetDateTime(8), reader.GetString(9), reader.GetString(10),
            reader.GetString(11), reader.GetString(12), reader.GetString(13), reader.GetString(14),
            reader.IsDBNull(15) ? "" : reader.GetString(15), reader.GetString(16), reader.GetString(17));
    }

    private static async Task<IReadOnlyList<string>> ReadLinesAsync(
        OracleConnection connection, string sql, string patientId, string visitId, CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, sql, patientId, visitId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var lines = new List<string>();
        while (await reader.ReadAsync(cancellationToken)) lines.Add(reader.GetString(0));
        return lines;
    }

    private static async Task<IReadOnlyList<string>> ReadReportsAsync(
        OracleConnection connection, string patientId, string visitId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT REPORT_TYPE, REPORT_NAME, REPORT_RESULT
            FROM HIS_REPORT
            WHERE PATIENT_ID = :patientId AND VISIT_ID = :visitId
            ORDER BY REPORT_SEQUENCE
            """;
        await using var command = CreateCommand(connection, sql, patientId, visitId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var lines = new List<string>();
        while (await reader.ReadAsync(cancellationToken))
            lines.Add($"{reader.GetString(0)}：{reader.GetString(1)}：{reader.GetString(2)}");
        return lines;
    }

    private static OracleCommand CreateCommand(OracleConnection connection, string sql, string patientId, string visitId)
    {
        var command = new OracleCommand(sql, connection) { BindByName = true };
        command.Parameters.Add("patientId", OracleDbType.Varchar2).Value = patientId;
        command.Parameters.Add("visitId", OracleDbType.Varchar2).Value = visitId;
        return command;
    }

    private sealed record EncounterRow(
        string HospitalName, string PatientName, string Gender, int Age, string WardName, string BedNumber, string InpatientNo,
        DateTime AdmittedAt, DateTime RecordedAt, string IllnessOnsetCourse, string PrimarySymptoms, string PastMedicalHistory,
        string PhysicalExamination, string DiagnosisBasis, string DifferentialDiagnosis, string CaseClassification,
        string AttendingPhysician, string AuthorPhysician);
}
