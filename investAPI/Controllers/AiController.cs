using investAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace investAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController(IOllamaService ollamaService) : ControllerBase
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

        var reply = await ollamaService.ChatAsync(
            request.Message,
            cancellationToken);

        return Ok(new { reply });
    }
}

public record ChatRequest(string Message);