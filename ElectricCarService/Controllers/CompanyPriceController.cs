using ElectricCarService.Data;
using Microsoft.AspNetCore.Mvc;

namespace ElectricCarService.Controllers;

[ApiController]
[Route("[controller]")]
public class CompanyPriceController : ControllerBase
{
    private readonly ElectricCarServiceContext _context;

    public CompanyPriceController(ElectricCarServiceContext context)
    {
        _context = context;
    }

    [HttpPut]
    public int Alter(CompanyPriceInfo priceInfo)
    {
        var price = new CompanyPriceEntity()
        {
            FlatCharge = priceInfo.StartFee,
            Id = 1,
            Rate = priceInfo.PerKwhFee
        };
        var dbPrice = _context.CompanyPrice.FirstOrDefault(p => p.Id == 1);
        if (dbPrice == null)
            _context.CompanyPrice.Add(price);
        else
            _context.CompanyPrice.Update(price);

        _context.SaveChanges();
        return StatusCodes.Status202Accepted;
    }
    
    public record CompanyPriceInfo(double StartFee, double PerKwhFee);
}