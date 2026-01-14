using IntegritySentinel.Worker.Configuration;
using IntegritySentinel.Worker.Domain.Entities;
using IntegritySentinel.Worker.Domain.Interfaces;

namespace IntegritySentinel.Worker.Services
{
    public class IntegrityService : IIntegrityService
    {
        // As ferramentas que vamos usar
        private readonly IFileRepository _repository;
        private readonly IFileHasher _hasher;
        private readonly ILogger<IntegrityService> _logger;

        // O Construtor: Quem chamar essa classe, TEM que entregar essas ferramentas
        public IntegrityService(IFileRepository repository, IFileHasher hasher, ILogger<IntegrityService> logger)
        {
            _repository = repository;
            _hasher = hasher;
            _logger = logger;
        }

        public async Task ExecuteCycleAsync(string targetPath, CancellationToken token)
        {
            var caminho = targetPath;
            if (!Directory.Exists(caminho))
            {
                Directory.CreateDirectory(caminho);
            }
            var files = Directory.EnumerateFiles(caminho, "*", SearchOption.AllDirectories);
            var diskFileSet = new HashSet<string>(files);
            foreach (var file in files)
            {
                if (token.IsCancellationRequested) break;
                string? hashAtual = await _hasher.HashAsync(file);
                if (hashAtual != null)
                {
                    var fileRecord = await _repository.Search(file);
                    if (fileRecord == null)
                    {
                        var novoArquivo = new FileRecord
                        {
                            FilePath = file,
                            Hash = hashAtual,
                            LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        await _repository.Add(novoArquivo);
                        _logger.LogInformation("Novo arquivo detectado e salvo: {Arquivo}", file);
                    }
                    else if (fileRecord.Hash != hashAtual)
                    {
                        fileRecord.Hash = hashAtual;
                        fileRecord.LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        await _repository.Update(fileRecord);

                        _logger.LogWarning("ALERTA DE SEGURANÇA: Arquivo alterado! {Arquivo}", file);
                    }
                }
            }
            var dbFiles = await _repository.SearchAll();

            foreach (var dbFile in dbFiles)
            {
                if (token.IsCancellationRequested) break;
                if (!diskFileSet.Contains(dbFile.FilePath))
                {
                    await _repository.Delete(dbFile.Id);
                    _logger.LogWarning("ALERTA: Arquivo deletado! {Arquivo}", dbFile.FilePath);
                }
            }
        }
    }
}
