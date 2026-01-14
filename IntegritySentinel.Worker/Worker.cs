using IntegritySentinel.Worker.Configuration;
using IntegritySentinel.Worker.Domain.Entities;
using IntegritySentinel.Worker.Domain.Interfaces;
using IntegritySentinel.Worker.Services;
using Microsoft.Extensions.Options;

namespace IntegritySentinel.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<MonitorSettings> _MonitorSettings;
        private readonly IIntegrityService _integrityService;

        public Worker(ILogger<Worker> logger, IOptions<MonitorSettings> settings, IIntegrityService integrityService)
        {
            _logger = logger;
            _MonitorSettings = settings;
            _integrityService = integrityService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var caminhoAlvo = _MonitorSettings.Value.TargetPath;
                await _integrityService.ExecuteCycleAsync(caminhoAlvo, stoppingToken);
                int delayTime = _MonitorSettings.Value.IntervalInSeconds * 1000;
                await Task.Delay(delayTime, stoppingToken);
            }
        }
    }
}
