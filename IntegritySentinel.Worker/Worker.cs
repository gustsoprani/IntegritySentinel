using IntegritySentinel.Worker.Configuration;
using IntegritySentinel.Worker.Domain.Entities;
using IntegritySentinel.Worker.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace IntegritySentinel.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IFileRepository _IFileRepository;
        private readonly IFileHasher _IFileHasher;
        private readonly IOptions<MonitorSettings> _MonitorSettings;

        public Worker(ILogger<Worker> logger, IFileRepository repo, IFileHasher hasher, IOptions<MonitorSettings> settings)
        {
            _logger = logger;
            _IFileRepository = repo;
            _IFileHasher = hasher;
            _MonitorSettings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                var caminho = _MonitorSettings.Value.TargetPath;
                if (!Directory.Exists(caminho))
                {
                    Directory.CreateDirectory(caminho);
                }
                var files = Directory.GetFiles(caminho);
                foreach (var file in files)
                {
                    string? hashAtual = await _IFileHasher.HashAsync(file);
                    if (hashAtual != null)
                    {
                        var fileRecord = await _IFileRepository.Search(file);
                        if (fileRecord == null)
                        {
                            var novoArquivo = new FileRecord
                            {
                                FilePath = file,
                                Hash = hashAtual,
                                LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            };
                            await _IFileRepository.Add(novoArquivo);
                            _logger.LogInformation("Novo arquivo detectado e salvo: {Arquivo}", file);
                        } else if (fileRecord.Hash != hashAtual)
                        {
                            fileRecord.Hash = hashAtual;
                            fileRecord.LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            await _IFileRepository.Update(fileRecord);

                            _logger.LogWarning("ALERTA DE SEGURANÇA: Arquivo alterado! {Arquivo}", file);
                        }
                    }
                }
                // 1. Buscamos tudo o que o banco "acha" que existe
                var dbFiles = await _IFileRepository.SearchAll();

                // 2. Comparamos com a realidade (files do disco)
                foreach (var dbFile in dbFiles)
                {
                    // Se o arquivo do banco NÃO estiver na lista do disco...
                    if (!files.Contains(dbFile.FilePath))
                    {
                        // ...significa que foi deletado!
                        await _IFileRepository.Delete(dbFile.Id);
                        _logger.LogWarning("ALERTA: Arquivo deletado! {Arquivo}", dbFile.FilePath);
                    }
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
