using System.Text.RegularExpressions;
using HisMedicalRecordFillDemo.Exceptions;

namespace HisMedicalRecordFillDemo.Services;

public interface IHisFixtureProvider
{
    Task<string> ReadAsync(string patientId, string visitId, CancellationToken cancellationToken);
}

public sealed class HisFixtureProvider(IHostEnvironment environment) : IHisFixtureProvider
{
    private static readonly Regex IdPattern = new("^[A-Za-z0-9_-]+$", RegexOptions.CultureInvariant);

    public async Task<string> ReadAsync(string patientId, string visitId, CancellationToken cancellationToken)
    {
        if (!IdPattern.IsMatch(patientId) || !IdPattern.IsMatch(visitId))
            throw new FixtureNotFoundException("patientId 和 visitId 只能包含字母、数字、下划线或连字符。");

        var path = Path.Combine(DemoPathResolver.GetRoot(environment), "Fixtures", $"{patientId}_{visitId}.txt");
        if (!File.Exists(path))
            throw new FixtureNotFoundException($"未找到本地 HIS 样例：{patientId}_{visitId}。");

        return await File.ReadAllTextAsync(path, cancellationToken);
    }
}
