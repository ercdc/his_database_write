using HisMedicalRecordFillDemo.Skills;
using HisMedicalRecordFillDemo.Tools;

namespace HisMedicalRecordFillDemo.Tests;

public sealed class FirstCourseSkillTests
{
    [Fact]
    public async Task BuildContextAsync_从技能资源加载提示词模板与工具声明()
    {
        var environment = new TestHostEnvironment(AppContext.BaseDirectory);
        var skill = new FirstCourseSkill(environment, new ToolDefinitionLoader(environment));

        var context = await skill.BuildContextAsync("【病案】测试", CancellationToken.None);

        Assert.Equal("first-course", context.SkillId);
        Assert.Contains("write_first_course_xml", context.AllowedToolNames);
        Assert.Equal("write_first_course_xml", context.RequiredToolName);
        Assert.Single(context.ToolDefinitions);
        Assert.Equal("write_first_course_xml", context.ToolDefinitions[0]["function"]!["name"]!.GetValue<string>());
        Assert.Contains("首次病程记录 XML 生成器", context.Messages[0]!["content"]!.GetValue<string>());
        Assert.True(File.Exists(context.SchemaPath));
    }
}
