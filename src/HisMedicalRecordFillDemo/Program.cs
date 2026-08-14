using HisMedicalRecordFillDemo.Options;
using HisMedicalRecordFillDemo.Services;
using HisMedicalRecordFillDemo.Skills;
using HisMedicalRecordFillDemo.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<DeepSeekOptions>(builder.Configuration.GetSection(DeepSeekOptions.SectionName));
builder.Services.AddHttpClient<IDeepSeekToolCallingClient, DeepSeekToolCallingClient>();
builder.Services.AddSingleton<IHisFixtureProvider, HisFixtureProvider>();
builder.Services.AddSingleton<IXmlRecordWriter, XmlRecordWriter>();
builder.Services.AddSingleton<ToolDefinitionLoader>();
builder.Services.AddSingleton<IModelTool, WriteFirstCourseXmlTool>();
builder.Services.AddSingleton<ToolRegistry>();
builder.Services.AddSingleton<FirstCourseSkill>();
builder.Services.AddScoped<FirstCourseGenerationService>();

var app = builder.Build();
app.MapControllers();
app.Run();

public partial class Program;
