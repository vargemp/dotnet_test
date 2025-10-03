using MessageBus;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MyApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;
    private readonly MessageService _msgService;

    public MessageController(ILogger<MessageController> logger, MessageService msgService)
    {
        _logger = logger;
        _msgService = msgService;
    }

    [HttpPost(Name = "PostMessage")]
    public async Task<IActionResult> Post([FromBody] string message)
    {
        Console.WriteLine($"The message is: {message}");

        await _msgService.PublishMessage(message);

        return Ok();
    }
}
