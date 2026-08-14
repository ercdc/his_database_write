using HisMedicalRecordFillDemo.Options;
using HisMedicalRecordFillDemo.Services;
using HisMedicalRecordFillDemo.Skills;
using HisMedicalRecordFillDemo.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<DeepSeekOptions>(builder.Configuration.GetSection(DeepSeekOptions.SectionName));
builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
builder.Services.AddHttpClient<IDeepSeekToolCallingClient, DeepSeekToolCallingClient>();
builder.Services.AddSingleton<IXmlRecordValidator, XmlRecordValidator>();

var databaseOptions = builder.Configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
if (string.Equals(databaseOptions.Mode, "Oracle", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IHisEncounterDataProvider, OracleHisEncounterDataProvider>();
    builder.Services.AddSingleton<IFirstCourseRecordWriter, OracleFirstCourseRecordWriter>();
}
else
{
    builder.Services.AddSingleton<IHisEncounterDataProvider, FixtureHisEncounterDataProvider>();
    builder.Services.AddSingleton<IFirstCourseRecordWriter, LocalFirstCourseRecordWriter>();
}
builder.Services.AddSingleton<ToolDefinitionLoader>();
builder.Services.AddSingleton<IModelTool, WriteFirstCourseXmlTool>();
builder.Services.AddSingleton<ToolRegistry>();
builder.Services.AddSingleton<FirstCourseSkill>();
builder.Services.AddScoped<FirstCourseGenerationService>();

var app = builder.Build();
app.MapControllers();
app.Run();

public partial class Program;
