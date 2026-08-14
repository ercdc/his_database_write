using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using HisMedicalRecordFillDemo.Exceptions;
using HisMedicalRecordFillDemo.Models;
using HisMedicalRecordFillDemo.Options;
using HisMedicalRecordFillDemo.Tools;
using Microsoft.Extensions.Options;

namespace HisMedicalRecordFillDemo.Services;

public interface IDeepSeekToolCallingClient
{
    Task<ToolCallingResult> RunAsync(
        SkillContext skill,
        string patientId,
        string visitId,
        CancellationToken cancellationToken);
}

public sealed class DeepSeekToolCallingClient(
    HttpClient httpClient,
    IOptions<DeepSeekOptions> options,
    ToolRegistry toolRegistry) : IDeepSeekToolCallingClient
{
    private readonly DeepSeekOptions _options = options.Value;

    public async Task<ToolCallingResult> RunAsync(
        SkillContext skill,
        string patientId,
        string visitId,
        CancellationToken cancellationToken)
    {
        var apiKey = GetApiKey();
        var messages = skill.Messages.DeepClone().AsArray();
        var firstResponse = await SendAsync(messages, apiKey, skill.ToolDefinitions, skill.RequiredToolName, cancellationToken);
        var toolCall = GetSingleToolCall(firstResponse, skill);

        var tool = toolRegistry.GetRequired(toolCall.Name);
        var context = new ToolExecutionContext(
            skill.SkillId,
            patientId,
            visitId,
            skill.SchemaPath,
            skill.OutputDirectory,
            skill.OutputFilePrefix);
        var toolResult = await tool.ExecuteAsync(toolCall.Arguments, context, cancellationToken);

        messages.Add(toolCall.AssistantMessage.DeepClone());
        messages.Add(new JsonObject
        {
            ["role"] = "tool",
            ["tool_call_id"] = toolCall.Id,
            ["content"] = toolResult.Content
        });

        var secondResponse = await SendAsync(messages, apiKey, toolDefinitions: null, requiredToolName: null, cancellationToken);
        var confirmationMessage = GetAssistantMessage(secondResponse);
        if (confirmationMessage["tool_calls"] is JsonArray { Count: > 0 })
            throw new ToolCallingException("模型在工具执行完成后再次请求调用工具，当前流程只接受一次工具调用。");

        return new ToolCallingResult(
            toolResult,
            confirmationMessage["content"]?.GetValue<string>() ?? "工具已执行完成。");
    }

    private string GetApiKey()
    {
        var apiKey = string.IsNullOrWhiteSpace(_options.ApiKey)
            ? Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")
            : _options.ApiKey;
        return string.IsNullOrWhiteSpace(apiKey)
            ? throw new ToolCallingException("未配置 DeepSeek API Key。请设置 DeepSeek__ApiKey 或 DEEPSEEK_API_KEY。")
            : apiKey;
    }

    private async Task<JsonObject> SendAsync(
        JsonArray messages,
        string apiKey,
        IReadOnlyList<JsonObject>? toolDefinitions,
        string? requiredToolName,
        CancellationToken cancellationToken)
    {
        var requestBody = new JsonObject
        {
            ["model"] = _options.Model,
            ["messages"] = messages.DeepClone(),
            ["thinking"] = new JsonObject { ["type"] = "disabled" },
            ["stream"] = false
        };
        if (toolDefinitions is not null && requiredToolName is not null)
        {
            requestBody["tools"] = new JsonArray(toolDefinitions.Select(tool => tool.DeepClone()).ToArray());
            requestBody["tool_choice"] = new JsonObject
            {
                ["type"] = "function",
                ["function"] = new JsonObject { ["name"] = requiredToolName }
            };
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl.TrimEnd('/')}/chat/completions")
        {
            Content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new ToolCallingException($"DeepSeek API 调用失败（HTTP {(int)response.StatusCode}）：{body}");

        try
        {
            return JsonNode.Parse(body)?.AsObject()
                ?? throw new ToolCallingException("DeepSeek API 返回了空 JSON 对象。");
        }
        catch (System.Text.Json.JsonException exception)
        {
            throw new ToolCallingException($"DeepSeek API 返回不是合法 JSON：{exception.Message}");
        }
    }

    private static ModelToolCall GetSingleToolCall(JsonObject response, SkillContext skill)
    {
        var message = GetAssistantMessage(response);
        var toolCalls = message["tool_calls"]?.AsArray();
        if (toolCalls is not { Count: 1 })
            throw new ToolCallingException("DeepSeek 第一轮响应必须且只能包含一个工具调用。");

        var toolCall = toolCalls[0]?.AsObject() ?? throw new ToolCallingException("工具调用格式无效。");
        var function = toolCall["function"]?.AsObject() ?? throw new ToolCallingException("工具调用缺少 function。");
        var name = function["name"]?.GetValue<string>() ?? throw new ToolCallingException("工具调用缺少工具名。");
        if (!skill.AllowedToolNames.Contains(name) || name != skill.RequiredToolName)
            throw new ToolCallingException($"当前技能不允许调用工具：{name}。");
        var id = toolCall["id"]?.GetValue<string>() ?? throw new ToolCallingException("工具调用缺少 id。");

        try
        {
            var arguments = JsonNode.Parse(function["arguments"]?.GetValue<string>() ?? string.Empty)?.AsObject()
                ?? throw new ToolCallingException("工具参数必须是 JSON 对象。");
            return new ModelToolCall(id, name, arguments, message);
        }
        catch (System.Text.Json.JsonException exception)
        {
            throw new ToolCallingException($"工具参数不是合法 JSON：{exception.Message}");
        }
    }

    private static JsonObject GetAssistantMessage(JsonObject response) =>
        response["choices"]?.AsArray().FirstOrDefault()?["message"]?.AsObject()
        ?? throw new ToolCallingException("DeepSeek 响应缺少 choices[0].message。");

    private sealed record ModelToolCall(string Id, string Name, JsonObject Arguments, JsonObject AssistantMessage);
}
