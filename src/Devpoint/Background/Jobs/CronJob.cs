using Cronos;
using Timer = System.Timers.Timer;

namespace Background.Jobs;

public abstract class CronJob : IDisposable
{
    private const string Expression = @"0/10 * * ? * *";
    
    private Timer _timer;
    private readonly CronExpression _cronExpression;
    private readonly TimeZoneInfo _timeZoneInfo;

    protected CronJob()
    {
        _cronExpression = CronExpression.Parse(Expression, CronFormat.IncludeSeconds);
        _timeZoneInfo = TimeZoneInfo.Utc;
    }

    protected async void Start()
    {
        await ScheduleJob();
    }

    private async Task ScheduleJob()
    {
        var next = _cronExpression.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
        if (!next.HasValue)
            return;
            
        var delay = next.Value - DateTimeOffset.Now;
        if (delay.TotalMilliseconds <= 0)   // prevent non-positive values from being passed into Timer
        {
            await ScheduleJob();
            return;
        }
        _timer = new Timer(delay.TotalMilliseconds);
        _timer.Elapsed += async (_, _) =>
        {
            _timer?.Dispose();  // reset and dispose timer
            _timer = null;

            
            await DoWork();
            await ScheduleJob();
        };
        _timer.Start();           
    }

    protected virtual async Task DoWork()
    {
        await Task.Delay(10);
    }

    protected async Task StopAsync()
    {
        _timer?.Stop();
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}