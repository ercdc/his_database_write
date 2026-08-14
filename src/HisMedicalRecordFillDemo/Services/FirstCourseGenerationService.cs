using HisMedicalRecordFillDemo.Models;
using HisMedicalRecordFillDemo.Skills;

namespace HisMedicalRecordFillDemo.Services;

public sealed class FirstCourseGenerationService(
    IHisFixtureProvider fixtureProvider,
    FirstCourseSkill firstCourseSkill,
    IDeepSeekToolCallingClient deepSeekClient)
{
    public async Task<GeneratedXmlResult> GenerateAsync(string patientId, string visitId, CancellationToken cancellationToken)
    {
        var hisData = await fixtureProvider.ReadAsync(patientId, visitId, cancellationToken);
        var skill = await firstCourseSkill.BuildContextAsync(hisData, cancellationToken);
        var result = await deepSeekClient.RunAsync(skill, patientId, visitId, cancellationToken);
        return new GeneratedXmlResult(
            result.ToolResult.OutputPath ?? throw new InvalidOperationException("写入工具没有返回 outputPath。"),
            result.Confirmation);
    }
}
