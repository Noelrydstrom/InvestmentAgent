using investAPI.Dtos;
using investAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace investAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TradingController(ITradingService tradingService) : ControllerBase
{
    // Skapa köp- eller säljordförslag
    [HttpPost("propose")]
    public async Task<IActionResult> CreateTradeProposal(
    [FromBody] TradeRequestDto request)
    {
        try
        {
            // Tillfällig demoanvändare
            const int userId = 1;

            var proposal = await tradingService.CreateTradeProposalAsync(
                userId,
                request);

            return Ok(proposal);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Bekräfta och genomför order
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmTrade(
        [FromBody] TradeConfirmationDto request)
    {
        try
        {
            // Tillfällig demoanvändare
            const int userId = 1;

            var transaction = await tradingService.ConfirmTradeAsync(
                userId,
                request);

            return Ok(new
            {
                message = "Transaktionen genomfördes.",
                transactionId = transaction.Id,
                ticker = transaction.SecurityId,
                transactionType = transaction.TransactionType,
                shares = transaction.Shares,
                price = transaction.Price,
                totalAmount = transaction.TotalAmount,
                transactionDate = transaction.TransactionDate
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

}
