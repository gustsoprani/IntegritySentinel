using IntegritySentinel.Worker;
using IntegritySentinel.Worker.Configuration;
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

    // Futuro: Registrar Repositories e Services aqui...

    var host = builder.Build();
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