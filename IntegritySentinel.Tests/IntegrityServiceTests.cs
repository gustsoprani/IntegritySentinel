using IntegritySentinel.Worker.Domain.Entities;
using IntegritySentinel.Worker.Domain.Interfaces;
using IntegritySentinel.Worker.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IntegritySentinel.Tests;

public class IntegrityServiceTests
{
    // Mocks: São os "Dublês" das nossas ferramentas reais.
    private readonly Mock<IFileRepository> _mockRepo;
    private readonly Mock<IFileHasher> _mockHasher;
    private readonly Mock<ILogger<IntegrityService>> _mockLogger;

    // O Serviço que vamos testar de verdade
    private readonly IntegrityService _service;

    public IntegrityServiceTests()
    {
        _mockRepo = new Mock<IFileRepository>();
        _mockHasher = new Mock<IFileHasher>();
        _mockLogger = new Mock<ILogger<IntegrityService>>();

        _service = new IntegrityService(_mockRepo.Object, _mockHasher.Object, _mockLogger.Object);
    }

    [Fact] // [Fact] avisa o xUnit que isso é um teste
    public async Task Deve_Adicionar_Novo_Arquivo_Quando_Nao_Existir_No_Banco()
    {
        var pastaFalsa = "C:\\Teste";
        var arquivoFalso = Path.Combine(pastaFalsa, "arquivo.txt");

        // Precisamos criar a pasta de verdade no disco temporariamente para o Directory.EnumerateFiles funcionar
        // (Isso é uma limitação de testar I/O, mas vamos simplificar criando a pasta rapidinho)
        if (!Directory.Exists(pastaFalsa)) Directory.CreateDirectory(pastaFalsa);
        File.WriteAllText(arquivoFalso, "conteudo teste");

        _mockHasher.Setup(h => h.HashAsync(arquivoFalso)).ReturnsAsync("HASH123");

        _mockRepo.Setup(r => r.Search(arquivoFalso)).ReturnsAsync((FileRecord?)null);

        await _service.ExecuteCycleAsync(pastaFalsa, CancellationToken.None);

        // Verificamos se o método Add() do repositório foi chamado EXATAMENTE 1 vez
        _mockRepo.Verify(r => r.Add(It.Is<FileRecord>(f =>
            f.FilePath == arquivoFalso &&
            f.Hash == "HASH123"
        )), Times.Once);

        Directory.Delete(pastaFalsa, true);
    }

    [Fact]
    public async Task Deve_Atualizar_E_Alertar_Quando_Arquivo_Foi_Modificado()
    {
        var pastaFalsa = "C:\\TesteModificacao";
        var arquivoFalso = Path.Combine(pastaFalsa, "arquivo_alterado.txt");

        if (!Directory.Exists(pastaFalsa)) Directory.CreateDirectory(pastaFalsa);
        File.WriteAllText(arquivoFalso, "conteudo novo");

        _mockHasher.Setup(h => h.HashAsync(arquivoFalso)).ReturnsAsync("HASH_NOVO");

        var registroAntigo = new FileRecord
        {
            Id = 1,
            FilePath = arquivoFalso,
            Hash = "HASH_ANTIGO",
            LastModified = "ontem"
        };
        _mockRepo.Setup(r => r.Search(arquivoFalso)).ReturnsAsync(registroAntigo);

        await _service.ExecuteCycleAsync(pastaFalsa, CancellationToken.None);

        // Verifica se o UPDATE foi chamado com o hash NOVO
        _mockRepo.Verify(r => r.Update(It.Is<FileRecord>(f =>
            f.FilePath == arquivoFalso &&
            f.Hash == "HASH_NOVO"
        )), Times.Once);

        // Verifica se gerou LOG de Warning (Alerta de Segurança)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ALERTA DE SEGURANÇA")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Directory.Delete(pastaFalsa, true);
    }

    [Fact]
    public async Task Deve_Detectar_E_Remover_Arquivo_Deletado()
    {
        var pastaFalsa = "C:\\TesteDelecao";

        if (!Directory.Exists(pastaFalsa)) Directory.CreateDirectory(pastaFalsa);

        var arquivoNoBanco = new FileRecord
        {
            Id = 99,
            FilePath = Path.Combine(pastaFalsa, "arquivo_sumiu.txt"),
            Hash = "HASH_QUALQUER"
        };

        _mockRepo.Setup(r => r.SearchAll()).ReturnsAsync(new List<FileRecord> { arquivoNoBanco });

        await _service.ExecuteCycleAsync(pastaFalsa, CancellationToken.None);

        _mockRepo.Verify(r => r.Delete(99), Times.Once);

        // Verifica se gerou LOG de Warning avisando da deleção
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ALERTA: Arquivo deletado")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        Directory.Delete(pastaFalsa, true);
    }
}