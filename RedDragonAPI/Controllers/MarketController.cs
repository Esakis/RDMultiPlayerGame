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

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        return Ok(await _marketService.GetHistoryAsync(UserId));
    }

    /// <summary>Targ państwowy — stałe kursy wymiany złota za zasoby.</summary>
    [HttpGet("exchange")]
    public IActionResult GetExchangeRates()
    {
        return Ok(_marketService.GetExchangeRates());
    }

    /// <summary>Wymiana na targu po stałym kursie (Buy = kup zasób, Sell = sprzedaj).</summary>
    [HttpPost("exchange")]
    public async Task<IActionResult> Exchange([FromBody] ExchangeDto dto)
    {
        var result = await _marketService.ExchangeAsync(UserId, dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    /// <summary>Progi auto-sprzedaży nadwyżek na targu (null = wyłączona).</summary>
    [HttpGet("autosell")]
    public async Task<IActionResult> GetAutoSell()
    {
        var settings = await _marketService.GetAutoSellAsync(UserId);
        return settings == null ? NotFound("Nie znaleziono księstwa.") : Ok(settings);
    }

    /// <summary>Ustawia progi auto-sprzedaży (co turę sprzedawana nadwyżka powyżej progu).</summary>
    [HttpPost("autosell")]
    public async Task<IActionResult> SetAutoSell([FromBody] AutoSellDto dto)
    {
        var result = await _marketService.SetAutoSellAsync(UserId, dto);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}
