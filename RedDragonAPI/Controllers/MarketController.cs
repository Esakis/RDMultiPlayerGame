using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedDragonAPI.Models.DTOs;
using RedDragonAPI.Services;
using System.Security.Claims;

namespace RedDragonAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarketController : ControllerBase
{
    private readonly IMarketService _marketService;

    public MarketController(IMarketService marketService)
    {
        _marketService = marketService;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpGet]
    public async Task<IActionResult> GetMarket()
    {
        return Ok(await _marketService.GetMarketAsync(UserId));
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateMarketOrderDto dto)
    {
        var result = await _marketService.CreateOrderAsync(UserId, dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost("fill")]
    public async Task<IActionResult> FillOrder([FromBody] FillMarketOrderDto dto)
    {
        var result = await _marketService.FillOrderAsync(UserId, dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpDelete("orders/{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var result = await _marketService.CancelOrderAsync(UserId, id);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}
