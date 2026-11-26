using System.Net.Http.Json;
using System.Text.Json;
using investAPI.Dtos;

namespace investAPI.Services;

public interface IOllamaService
{
    Task<string> ChatAsync(
    string message,
    string marketData,
    CancellationToken cancellationToken);

    Task<AiModelResponseDto> InterpretMessageAsync(
    string message,
    string marketData,
    CancellationToken cancellationToken);
}

public class OllamaService(HttpClient httpClient) : IOllamaService
{
    public async Task<string> ChatAsync(
    string message,
    string marketData,
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

                You have access to market information retrieved
                from the application's local SQL Server database.

                IMPORTANT RULES:

                - Use the provided database information when answering
                  questions about companies and stock prices.
                - Never invent stock prices, dates, tickers or company data.
                - If information is missing, explicitly say so.
                - Always mention the date associated with a stock price.
                - Prices are historical database records and may not
                  represent real-time market prices.
                - Clearly distinguish database facts from your analysis.
                - Answer in the same language as the user.
                - Keep answers clear and concise.
                """
            },
            new
            {
                role = "user",
                content = $"""
                DATABASE INFORMATION:

                {marketData}

                USER QUESTION:

                {message}
                """
            }
        },
            think = false,
            stream = false,
            keep_alive = "30m",
            options = new
            {
                num_predict = 200,
                temperature = 0.3
            }
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

    public async Task<AiModelResponseDto> InterpretMessageAsync(
        string message,
        string marketData,
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
                    You are Investment Agent.

                    Your task is to understand the user's message
                    and return a structured JSON response.

                    RULES:

                    - Answer in the same language as the user.
                    - Only identify a trade when the user gives
                    a clear and explicit instruction to buy or sell.
                    - Questions about buying or selling are NOT trade orders.
                    - Never assume missing amounts or tickers.
                    - Never invent tickers.
                    - Only use companies from the provided database information.
                    - BUY means purchase shares.
                    - SELL means sell shares.

                    Return ONLY valid JSON using this structure:

                    {
                        "reply": "Your response",
                        "isTradeRequest": false,
                        "transactionType": null,
                        "ticker": null,
                        "amount": null
                    }

                    For an explicit trade instruction, set isTradeRequest
                    to true and provide transactionType, ticker and amount.

                    Amount must always be in SEK.
                    """
                },
                new
                {
                    role = "user",
                    content = $"""
                    DATABASE INFORMATION:

                    {marketData}

                    USER MESSAGE:

                    {message}
                    """
                }
            },

            format = "json",
            think = false,
            stream = false,
            keep_alive = "30m",

            options = new
            {
                num_predict = 250,
                temperature = 0.1
            }
        };

        var response = await httpClient.PostAsJsonAsync(
            "/api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(
            cancellationToken);

        using var document = JsonDocument.Parse(json);

        var content = document.RootElement
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Ollama returned an empty response.");

        return JsonSerializer.Deserialize<AiModelResponseDto>(
            content,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            ?? throw new InvalidOperationException("Could not interpret AI response.");
    }

}
