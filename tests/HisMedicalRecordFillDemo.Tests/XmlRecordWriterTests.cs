using HisMedicalRecordFillDemo.Exceptions;
using HisMedicalRecordFillDemo.Models;
using HisMedicalRecordFillDemo.Services;

namespace HisMedicalRecordFillDemo.Tests;

public sealed class XmlRecordWriterTests
{
    [Fact]
    public async Task WriteAsync_合法XML_通过Xsd并创建唯一文件()
    {
        var writer = new XmlRecordWriter();
        var context = CreateContext();

        var firstPath = await writer.WriteAsync(ValidXml, context, CancellationToken.None);
        var secondPath = await writer.WriteAsync(ValidXml, context, CancellationToken.None);

        Assert.True(File.Exists(firstPath));
        Assert.True(File.Exists(secondPath));
        Assert.NotEqual(firstPath, secondPath);
    }

    [Fact]
    public async Task WriteAsync_错误节点顺序_拒绝写入()
    {
        var writer = new XmlRecordWriter();
        var invalid = ValidXml.Replace("<姓名>患者甲</姓名>\n    <性别>男</性别>", "<性别>男</性别>\n    <姓名>患者甲</姓名>");

        var exception = await Assert.ThrowsAsync<XmlValidationException>(() => writer.WriteAsync(invalid, CreateContext(), CancellationToken.None));

        Assert.Contains("XSD", exception.Message);
    }

    internal static ToolExecutionContext CreateContext() => new(
        "first-course",
        "CASE-001",
        "TEST20260811001",
        Path.Combine(AppContext.BaseDirectory, "Resources", "Skills", "FirstCourse", "schema.xsd"),
        Path.Combine(AppContext.BaseDirectory, "Output"),
        "首次病程记录");

    internal const string ValidXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <首次病程记录>
          <医院名称>示例市第一医院</医院名称>
          <患者信息>
            <姓名>患者甲</姓名>
            <性别>男</性别>
            <年龄>27</年龄>
            <病区>普外科一病区</病区>
            <床号>12A</床号>
            <住院号>TEST20260811001</住院号>
            <ID号>CASE-001</ID号>
          </患者信息>
          <记录时间>2026-08-11T08:35:00+08:00</记录时间>
          <病例特点><临床表现><发病及病程>脐周隐痛后转移至右下腹。</发病及病程><主要症状>右下腹痛。</主要症状><既往史>否认高血压。</既往史><查体>右下腹压痛。</查体></临床表现></病例特点>
          <拟诊讨论><诊断依据>右下腹痛。</诊断依据><鉴别诊断>急性胃肠炎。</鉴别诊断><入院诊断><诊断>急性阑尾炎</诊断></入院诊断></拟诊讨论>
          <诊疗计划><计划><序号>1</序号><内容>完善术前检查。</内容></计划></诊疗计划>
          <医师信息><带组医师>医生甲</带组医师><书写医师>医生乙</书写医师></医师信息>
        </首次病程记录>
        """;
}
