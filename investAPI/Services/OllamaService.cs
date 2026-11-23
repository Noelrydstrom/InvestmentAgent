using System.Net.Http.Json;
using System.Text.Json;

namespace investAPI.Services;

public interface IOllamaService
{
    Task<string> ChatAsync(string message, CancellationToken cancellationToken);
}

public class OllamaService(HttpClient httpClient) : IOllamaService
{
    public async Task<string> ChatAsync(
        string message,
        CancellationToken cancellationToken)
    {
        var request = new
        {
            model = "qwen3:8b",
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = """
                    You are Investment Agent, a helpful AI assistant
                    specialized in investments and financial markets.

                    Answer clearly and concisely.
                    Do not claim to have access to live market data
                    unless that data has been provided to you.
                    """
                },
                new
                {
                    role = "user",
                    content = message
                }
            },
            stream = false
        };

        var response = await httpClient.PostAsJsonAsync(
            "/api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(
            cancellationToken);

        using var document = JsonDocument.Parse(json);

        return document.RootElement
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "No response from AI.";
    }
}