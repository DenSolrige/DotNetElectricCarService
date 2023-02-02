using System.Net.WebSockets;
using System.Text.Json;
using ElectricCarService.Data;

namespace ElectricCarService.Websockets;

public class Handler
{
    private ElectricCarServiceContext _context;
    private readonly string _identifier;
    private readonly WebSocket _ws;
    private ChargingStationEntity cs;

    public Handler(string identifier, WebSocket ws, ElectricCarServiceContext context, ChargingStationEntity cs)
    {
        _identifier = identifier;
        _ws = ws;
        _context = context;
        this.cs = cs;
    }

    public void Handle(string message)
    {
        var payload = message.Split(" ",2);
        if (payload[0].StartsWith("start"))
            HandleStart(payload[1]);
        else if (payload[0].StartsWith("charging"))
            HandleCharging(payload[1]);
        else if (payload[0].StartsWith("status"))
            HandleStatus(payload[1]);
        else if (payload[0].StartsWith("stop"))
            HandleStop(payload[1]);
    }

    private void HandleStart(string message)
    {
        Console.WriteLine($"{_identifier}: Start transaction!");
        
        var payload = JsonSerializer.Deserialize<HandleStartPayload>(message);
        var price = _context.CompanyPrice.FirstOrDefault(p => p.Id == 1);
        var transaction = new TransactionEntity();
        if (price != null)
        {
            transaction.CompanyFlatCharge = price.FlatCharge;
            transaction.CompanyRate = price.Rate;
        }
        transaction.Id = payload!.TransactionId;
        transaction.ChargingStation = cs;
        transaction.StartTime = payload.Timestamp;
        _context.Transactions.Add(transaction);
        _context.SaveChanges();

        var charge = new ChargeEntity()
        {
            Timestamp = payload.Timestamp,
            ChargeAmount = payload.MeterStart,
            Transaction = transaction
        };
        _context.Charges.Add(charge);

        cs.Status = ChargingStationStatus.Charging;
        _context.ChargingStations.Update(cs);
        _context.SaveChanges();
        
        _ws.SendStringAsync("start ack", CancellationToken.None);
    }

    private void HandleCharging(string message)
    {
        var payload = JsonSerializer.Deserialize<HandleChargingPayload>(message);
        var transaction = _context.Transactions.FirstOrDefault(t => t.Id == payload!.TransactionId);
        Console.WriteLine($"{_identifier}: Charging update - {message}");
        var charge = new ChargeEntity()
        {
            Timestamp = payload!.Timestamp,
            ChargeAmount = payload.MeterValue,
            Transaction = transaction!
        };
        _context.Charges.Add(charge);
        _context.SaveChanges();
    }

    private void HandleStatus(string message)
    {
        var payload = JsonSerializer.Deserialize<HandleStatusPayload>(message);
        var transaction = _context.Transactions.FirstOrDefault(t => t.Id == payload!.TransactionId);
        Console.WriteLine($"{_identifier}: Status change - {message}");
        if (payload!.Status == TransactionStatus.Suspended)
        {
            transaction!.Status = TransactionStatus.Suspended;
            transaction.ChargingStation.Status = ChargingStationStatus.Suspended;
        }
        else if (payload.Status == TransactionStatus.Charging)
        {
            transaction!.Status = TransactionStatus.Charging;
            transaction.ChargingStation.Status = ChargingStationStatus.Charging;
        }

        _context.SaveChanges();
    }

    private void HandleStop(string message)
    {
        var payload = JsonSerializer.Deserialize<HandleStopPayload>(message);
        var transaction = _context.Transactions.FirstOrDefault(t => t.Id == payload!.TransactionId);
        Console.WriteLine($"{_identifier}: Stop transaction!");
        
        transaction!.Status = TransactionStatus.Finished;
        transaction.ChargingStation.Status = ChargingStationStatus.Available;
        transaction.StopTime = payload!.Timestamp;
        
        var charge = new ChargeEntity()
        {
            Timestamp = payload!.Timestamp,
            ChargeAmount = payload.MeterStop,
            Transaction = transaction!
        };
        _context.Charges.Add(charge);
        _context.SaveChanges();
        
        _ws.SendStringAsync("stop ack", CancellationToken.None);
    }
    
    public record HandleStartPayload(int TransactionId, DateTimeOffset Timestamp, double MeterStart);
    public record HandleChargingPayload(int TransactionId, DateTimeOffset Timestamp, double MeterValue);
    public record HandleStatusPayload(int TransactionId, DateTimeOffset Timestamp, TransactionStatus Status);
    public record HandleStopPayload(int TransactionId, DateTimeOffset Timestamp, double MeterStop);
}
















