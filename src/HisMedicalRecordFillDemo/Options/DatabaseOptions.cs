namespace HisMedicalRecordFillDemo.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Mode { get; init; } = "Fixture";
    public string OracleConnectionString { get; init; } = string.Empty;
}
