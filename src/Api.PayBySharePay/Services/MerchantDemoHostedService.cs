using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.PayBySharePay.Services;

/// <summary>
/// Starter http-server til Frontend.MerchantDemo på port 8081 automatisk i Development.
/// </summary>
public class MerchantDemoHostedService : IHostedService, IDisposable
{
    private readonly ILogger<MerchantDemoHostedService> _logger;
    private readonly IWebHostEnvironment _env;
    private Process? _process;

    public MerchantDemoHostedService(ILogger<MerchantDemoHostedService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment()) return Task.CompletedTask;

        var merchantDemoPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Frontend.MerchantDemo"));

        if (!Directory.Exists(merchantDemoPath))
        {
            _logger.LogWarning("MerchantDemo-mappen ikke fundet: {Path}", merchantDemoPath);
            return Task.CompletedTask;
        }

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c npx http-server . -p 8081 --cors -c-1 --silent",
                WorkingDirectory = merchantDemoPath,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            _process.Start();
            _logger.LogInformation("Pizzeria Roma demo-server startet på http://localhost:8081");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Kunne ikke starte Pizzeria Roma demo-server: {Message}", ex.Message);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_process is { HasExited: false })
            {
                _process.Kill(entireProcessTree: true);
                _logger.LogInformation("Pizzeria Roma demo-server stoppet.");
            }
        }
        catch { /* ignorér fejl ved nedlukning */ }

        return Task.CompletedTask;
    }

    public void Dispose() => _process?.Dispose();
}
