using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using HisMedicalRecordFillDemo.Models;
using HisMedicalRecordFillDemo.Options;
using HisMedicalRecordFillDemo.Services;
using HisMedicalRecordFillDemo.Skills;
using HisMedicalRecordFillDemo.Tools;
using Microsoft.Extensions.Options;

namespace HisMedicalRecordFillDemo.Tests;

public sealed class DeepSeekToolCallingClientTests
{
    [Fact]
    public async Task GenerateAsync_模型调用唯一工具_执行写入并完成第二轮确认()
    {
        var handler = new RecordingHandler(LocalFirstCourseRecordWriterTests.ValidXml);
        var client = new HttpClient(handler);
        var environment = new TestHostEnvironment(AppContext.BaseDirectory);
        var writer = new LocalFirstCourseRecordWriter(
            new XmlRecordValidator(),
            environment);
        var registry = new ToolRegistry([new WriteFirstCourseXmlTool(writer)]);
        var options = Microsoft.Extensions.Options.Options.Create(new DeepSeekOptions
        {
            ApiKey = "test-key",
            Model = "deepseek-v4-flash",
            BaseUrl = "https://api.deepseek.com/beta"
        });
        var service = new DeepSeekToolCallingClient(client, options, registry);
        var skill = await new FirstCourseSkill(environment, new ToolDefinitionLoader(environment))
            .BuildContextAsync("【病案】测试", CancellationToken.None);

        var result = await service.RunAsync(skill, "CASE-001", "TEST20260811001", CancellationToken.None);

        Assert.True(File.Exists(result.ToolResult.OutputPath));
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("/beta/chat/completions", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Single(handler.Requests[0].Body["tools"]!.AsArray());
        Assert.Equal("write_first_course_xml", handler.Requests[0].Body["tools"]![0]!["function"]!["name"]!.GetValue<string>());
        Assert.True(handler.Requests[0].Body["tools"]![0]!["function"]!["strict"]!.GetValue<bool>());
        Assert.Equal("disabled", handler.Requests[0].Body["thinking"]!["type"]!.GetValue<string>());
        Assert.Equal("tool", handler.Requests[1].Body["messages"]!.AsArray().Last()!["role"]!.GetValue<string>());
        Assert.Contains("已写入", result.Confirmation);
    }

    private sealed class RecordingHandler(string xml) : HttpMessageHandler
    {
        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = JsonNode.Parse(await request.Content!.ReadAsStringAsync(cancellationToken))!.AsObject();
            Requests.Add(new RecordedRequest(request.RequestUri, body));
            var content = Requests.Count == 1
                ? JsonSerializer.Serialize(new
                {
                    choices = new[]
                    {
                        new
                        {
                            message = new
                            {
                                role = "assistant",
                                content = (string?)null,
                                tool_calls = new[]
                                {
                                    new
                                    {
                                        id = "call_1",
                                        type = "function",
                                        function = new { name = "write_first_course_xml", arguments = JsonSerializer.Serialize(new { xml }) }
                                    }
                                }
                            }
                        }
                    }
                })
                : """{"choices":[{"message":{"role":"assistant","content":"XML 已写入本地文件。"}}]}""";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed record RecordedRequest(Uri? RequestUri, JsonObject Body);
}
