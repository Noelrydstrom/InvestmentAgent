using investAPI.Dtos;
using investAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace investAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController(
    IOllamaService ollamaService,
    IInvestmentDataService investmentDataService,
    ITradingService tradingService) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message cannot be empty.");

        try
        {
            const int userId = 1;

            var marketData = await investmentDataService
                .GetMarketOverviewAsync(cancellationToken);

            var aiResponse = await ollamaService.InterpretMessageAsync(
                request.Message,
                marketData,
                cancellationToken);

            TradeProposalDto? tradeProposal = null;

            if (aiResponse.IsTradeRequest)
            {
                if (string.IsNullOrWhiteSpace(aiResponse.Ticker) ||
                    string.IsNullOrWhiteSpace(aiResponse.TransactionType) ||
                    aiResponse.Amount is null ||
                    aiResponse.Amount <= 0 ||
                    (aiResponse.TransactionType != "BUY" &&
                     aiResponse.TransactionType != "SELL"))
                {
                    return Ok(new
                    {
                        reply = "Jag behöver en tydlig aktie, köp- eller säljorder och ett belopp i SEK för att skapa ett orderförslag.",
                        tradeProposal = (TradeProposalDto?)null
                    });
                }

                var tradeRequest = new TradeRequestDto
                {
                    Ticker = aiResponse.Ticker,
                    TransactionType = aiResponse.TransactionType,
                    Amount = aiResponse.Amount.Value
                };

                tradeProposal = await tradingService.CreateTradeProposalAsync(
                    userId,
                    tradeRequest);

                aiResponse.Reply =
                    $"Jag har skapat ett orderförslag för {tradeProposal.CompanyName}. Granska uppgifterna och bekräfta om du vill genomföra affären.";
            }

            return Ok(new
            {
                reply = aiResponse.Reply,
                tradeProposal
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AI Controller Error: {ex}");
            return StatusCode(500, new
            {
                error = "Unable to process AI request."
            });
        }
    }
}

public record ChatRequest(string Message);