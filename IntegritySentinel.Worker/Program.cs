using IntegritySentinel.Worker;
using IntegritySentinel.Worker.Configuration;
using IntegritySentinel.Worker.Data;
using IntegritySentinel.Worker.Domain.Interfaces;
using IntegritySentinel.Worker.Services;
using Serilog;

// Configuração inicial do Serilog (para pegar erros de startup)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Iniciando IntegritySentinel...");

    var builder = Host.CreateApplicationBuilder(args);

    // 1. Adicionar Serilog ao Host
    builder.Services.AddSerilog();

    // 2. Configuração Tipada (IOptions)
    builder.Services.Configure<MonitorSettings>(
        builder.Configuration.GetSection("MonitorSettings"));

    // 3. Registrar o Worker
    builder.Services.AddHostedService<Worker>();

    builder.Services.AddSingleton<DatabaseBootstrap>();

    builder.Services.AddSingleton<IFileRepository, DapperFileRepository>();

    builder.Services.AddSingleton<IFileHasher, Sha256Hasher>();

    // Futuro: Registrar Repositories e Services aqui...

    var host = builder.Build();
    // 1. Criamos um escopo para pegar o serviço
    using (var scope = host.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            // 2. Pegamos o Bootstrap que estava parado
            var bootstrap = services.GetRequiredService<DatabaseBootstrap>();

            // 3. Mandamos ele trabalhar (Criar a tabela)
            bootstrap.Setup();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Erro fatal ao criar o banco de dados!");
            return; // Para o programa se não conseguir criar o banco
        }
    }
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "O serviço falhou fatalmente na inicialização.");
}
finally
{
    Log.CloseAndFlush();
}