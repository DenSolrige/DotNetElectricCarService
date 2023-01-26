namespace ElectricCarService.Jobs;

public class MyBackgroundJob: BackgroundService
{
    private readonly ILogger<MyBackgroundJob> _logger;

    public MyBackgroundJob(ILogger<MyBackgroundJob> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Hi");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
        _logger.LogInformation("Bye");
    }
}