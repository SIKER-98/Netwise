using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/fact")]
public class FactController : ControllerBase
{
    private readonly IFactService _factService;

    public FactController(IFactService factService)
    {
        _factService = factService;
    }

    [HttpGet()]
    public async Task<IResult> GetFact()
    {
        return await _factService.FetchAndSaveFactAsync();
    }
}