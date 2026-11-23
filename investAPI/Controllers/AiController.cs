using investAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace investAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController(
IOllamaService ollamaService,
IInvestmentDataService investmentDataService) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
    [FromBody] ChatRequest request,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message cannot be empty.");
        }

        try
        {
            var marketData = await investmentDataService.GetMarketOverviewAsync(
                cancellationToken);

            var reply = await ollamaService.ChatAsync(
                request.Message,
                marketData,
                cancellationToken);

            return Ok(new { reply });
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
