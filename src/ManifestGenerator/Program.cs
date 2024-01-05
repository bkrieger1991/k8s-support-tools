using CliFx;
using CliFx.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ManifestGenerator;

public static class Program
{
    private static readonly IConsole _console = new SystemConsole();

    public static async Task<int> Main()
    {
        var provider = new ServiceCollection()
            
            .BuildServiceProvider();

        return await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseTypeActivator(provider)
            // Provide external created console instance. It's also available via DI
            .UseConsole(_console)
            .Build()
            .RunAsync();
    }
}