using HisMedicalRecordFillDemo.Options;
using HisMedicalRecordFillDemo.Services;
using HisMedicalRecordFillDemo.Skills;
using HisMedicalRecordFillDemo.Tools;
using Microsoft.Extensions.Options;

namespace HisMedicalRecordFillDemo.Tests;

[Trait("Category", "Integration")]
public sealed class DeepSeekRealIntegrationTests
{
    [Fact]
    public async Task GenerateAsync_真实DeepSeek调用_生成通过Xsd的XML()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_DEEPSEEK_INTEGRATION_TEST"), "true", StringComparison.OrdinalIgnoreCase))
            return;

        var environment = new TestHostEnvironment(AppContext.BaseDirectory);
        var fixture = new HisFixtureProvider(environment);
        var writer = new XmlRecordWriter();
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        var options = Microsoft.Extensions.Options.Options.Create(new DeepSeekOptions());
        var client = new DeepSeekToolCallingClient(
            httpClient,
            options,
            new ToolRegistry([new WriteFirstCourseXmlTool(writer)]));
        var hisData = await fixture.ReadAsync("CASE-001", "TEST20260811001", CancellationToken.None);
        var skill = await new FirstCourseSkill(environment, new ToolDefinitionLoader(environment))
            .BuildContextAsync(hisData, CancellationToken.None);

        var result = await client.RunAsync(skill, "CASE-001", "TEST20260811001", CancellationToken.None);

        Assert.True(File.Exists(result.ToolResult.OutputPath));
        Assert.EndsWith(".xml", result.ToolResult.OutputPath, StringComparison.OrdinalIgnoreCase);
    }
}
