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
                    You are InvestAI, an investment assistant.

                    Your primary task is to identify explicit BUY and SELL instructions
                    and convert them into structured JSON.

                    IMPORTANT TRADING RULES:

                    1. The user ALWAYS specifies the trade amount in SEK unless
                    they explicitly specify a number of shares.

                    2. If the user says:
                    "Köp 1000 kr i KRNA"
                    "Köp aktier i KRNA för 1000 SEK"
                    "Jag vill investera 1000 kronor i KRNA"

                    You MUST interpret this as:
                    transactionType = "BUY"
                    ticker = "KRNA"
                    amount = 1000

                    3. NEVER ask the user how many shares they want when they
                    have specified an amount in SEK.

                    4. NEVER interpret a SEK amount as a number of shares.

                    5. The backend calculates how many shares can be purchased
                    using the amount and the stock price.

                    6. Only identify a trade when the user gives a clear,
                    explicit instruction to buy or sell.

                    7. Questions about buying or selling are NOT trade orders.

                    8. Never invent amounts or tickers.

                    9. Only use tickers that exist in the provided database information.

                    10. If the user specifies an amount in SEK but no ticker,
                        ask which company they want to trade.

                    11. If the user specifies a ticker but no amount,
                        ask how much they want to buy or sell for in SEK.

                    12. Answer in the same language as the user.

                    EXAMPLES:

                    User: "Köp 1000 kr i KRNA"

                    {
                        "reply": "Jag skapar ett köporderförslag för KRNA på 1000 SEK.",
                        "isTradeRequest": true,
                        "transactionType": "BUY",
                        "ticker": "KRNA",
                        "amount": 1000
                    }

                    User: "Jag vill köpa aktier i HLSA för 500 kr"

                    {
                        "reply": "Jag skapar ett köporderförslag för HLSA på 500 SEK.",
                        "isTradeRequest": true,
                        "transactionType": "BUY",
                        "ticker": "HLSA",
                        "amount": 500
                    }

                    User: "Sälj KRNA för 2000 kr"

                    {
                        "reply": "Jag skapar ett säljorderförslag för KRNA på 2000 SEK.",
                        "isTradeRequest": true,
                        "transactionType": "SELL",
                        "ticker": "KRNA",
                        "amount": 2000
                    }

                    User: "Vad tycker du om KRNA?"

                    {
                        "reply": "Jag kan hjälpa dig att analysera KRNA.",
                        "isTradeRequest": false,
                        "transactionType": null,
                        "ticker": null,
                        "amount": null
                    }

                    RETURN ONLY VALID JSON.

                    The amount field MUST always represent SEK, never number of shares.
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
