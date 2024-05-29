using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace interview_matcher.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewMatcherController : ControllerBase
{
    private readonly ILogger<InterviewMatcherController> _logger;

    public InterviewMatcherController(ILogger<InterviewMatcherController> logger)
    {
        _logger = logger;
    }

    [HttpPost("AddUser/{userName}")]
    public UserEntity AddUser(string userName, [FromKeyedServices("AzureStorageClient")] IAzureStorageClient client)
    {
        UserEntity user =client.AddUser(userName, userName);
        return user;
    }

    [HttpPost("GetMatch")]
    public Dictionary<string, string> GetMatch([FromKeyedServices("AzureStorageClient")] IAzureStorageClient client)
    {
        return client.MatchGroup();
    }

    [HttpPost("DummyTest")]
    public string DummyTest([FromKeyedServices("AzureStorageClient")] IAzureStorageClient client)
    {
        return client.Test();
    }
}
