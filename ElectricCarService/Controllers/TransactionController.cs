using ElectricCarService.Data;
using Microsoft.AspNetCore.Mvc;

namespace ElectricCarService.Controllers;

[ApiController]
[Route("[controller]")]
public class TransactionController : ControllerBase
{
    private ElectricCarServiceContext _context;

    public TransactionController(ElectricCarServiceContext context)
    {
        _context = context;
    }
    
    [HttpGet("{id}")]
    public ActionResult<TransactionInfo> GetByIdentifier(int id)
    {
        var t = _context.Transactions.FirstOrDefault(t => t.Id == id);
        if (t == null)
        {
            return StatusCode(404);
        }
        var startCharge = t.Charges[0].ChargeAmount;
        var stopCharge = t.Charges[^1].ChargeAmount;
        return new TransactionInfo(t.Status,stopCharge-startCharge,t.StartTime,t.StopTime);
    }
    
    public record TransactionInfo(TransactionStatus Status, double ChargeTotal, DateTimeOffset StartTime, DateTimeOffset StopTime);
}