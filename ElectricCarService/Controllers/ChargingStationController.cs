using System.Net;
using System.Net.WebSockets;
using System.Text;
using ElectricCarService.Data;
using ElectricCarService.Websockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElectricCarService.Controllers;

[ApiController]
[Route("[controller]")]
public class ChargingStationController : ControllerBase
{
    private readonly ElectricCarServiceContext _context;
    private ILogger<ChargingStationController> _logger;

    public ChargingStationController(ElectricCarServiceContext context, ILogger<ChargingStationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost(Name = "PostChargingStation")]
    public ChargingStationEntity Post(NewChargingStationEntity newCse)
    {
        var cse = new ChargingStationEntity()
        {
            Identifier = newCse.Identifier,
            Address = newCse.Address,
            Number = newCse.Number
        };
        var savedCse = _context.Add<ChargingStationEntity>(cse);
        _context.SaveChanges();
        return savedCse.Entity;
    }
    
    [HttpGet]
    public IEnumerable<ChargingStationEntity> Get()
    {
        return _context.ChargingStations;
    }
    
    [HttpGet("{identifier}")]
    public ActionResult<ChargingStationEntity> GetByIdentifier(string identifier)
    {
        var cs = _context.ChargingStations.FirstOrDefault(cs => cs.Identifier == identifier);
        if (cs == null)
        {
            return StatusCode(404);
        }
        return cs;
    }
    
    [HttpDelete("{identifier}")]
    public int DeleteByIdentifier(string identifier)
    {
        _context.ChargingStations.Where(cs => cs.Identifier == identifier).ExecuteDelete();
        _context.SaveChanges();
        return StatusCodes.Status202Accepted;
    }
    
    [HttpPut("{identifier}")]
    public ActionResult<ChargingStationEntity> UpdateByIdentifier(string identifier, UpdateChargingStationEntity uCse)
    {
        var cs = _context.ChargingStations.FirstOrDefault(cs => cs.Identifier == identifier);
        if (cs == null)
        {
            return StatusCode(404);
        }
        cs.Address = uCse.Address;
        cs.Number = uCse.Number;
        _context.SaveChanges();
        return cs;
    }

    [HttpGet]
    [Route("{identifier}/connect")]
    public async Task<IActionResult> Connect(string identifier)
    {
        var cs = _context.ChargingStations.FirstOrDefault(cs => cs.Identifier == identifier);
        if (cs == null)
        {
            return StatusCode(404);
        }
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var handler = new Handler(identifier, webSocket, _context,cs);
            
            _logger.LogInformation($"Connection established with {identifier}");

            var buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                var str = Encoding.UTF8.GetString(buffer, 0, result.Count);

                handler.Handle(str);
            }

            if (webSocket.State == WebSocketState.CloseReceived)
            {
                _logger.LogInformation($"Close received from {identifier}");
                await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
        
            _logger.LogInformation($"Connection closed with {identifier}");
            return Empty;
        }
        return StatusCode((int)HttpStatusCode.BadRequest);
    }

    public record NewChargingStationEntity(string Identifier, string Address, int Number);
    public record UpdateChargingStationEntity(string Address, int Number);
}

