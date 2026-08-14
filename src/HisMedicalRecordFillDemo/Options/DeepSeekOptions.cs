namespace HisMedicalRecordFillDemo.Options;

public sealed class DeepSeekOptions
{
    public const string SectionName = "DeepSeek";

    public string BaseUrl { get; init; } = "https://api.deepseek.com/beta";
    public string Model { get; init; } = "deepseek-v4-flash";
    public string ApiKey { get; init; } = string.Empty;
}
