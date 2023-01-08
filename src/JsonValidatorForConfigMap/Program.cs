using CliFx;
using CliFx.Infrastructure;
using JsonValidatorForConfigMap.Commands;
using JsonValidatorForConfigMap.Config;
using JsonValidatorForConfigMap.Helper;
using JsonValidatorForConfigMap.KubernetesResource;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonValidatorForConfigMap;

public static class Program
{
    private static readonly IConsole _console = new SystemConsole();

    public static async Task<int> Main()
    {
        var config = await LoadConfig() ?? new Configuration();

        var provider = new ServiceCollection()
            .AddLogging(c => c
                .AddFilter(c => c == config.LoggingSeverity)
                .AddConsole()
            )
            .AddTransient<JsonSchemaReader>()
            .AddTransient<ResourceFileReader>()
            .AddSingleton<IConsoleProvider>(new ConsoleProvider(_console))
            .AddSingleton(config)

            // Commands
            .AddTransient<ValidateConfigMapSchema>()
            .AddTransient<InstallPreCommitHook>()
            .BuildServiceProvider();

        return await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseTypeActivator(provider)
            // Provide external created console instance. It's also available via DI
            .UseConsole(_console)
            .Build()
            .RunAsync();
    }

    private static async Task<Configuration?> LoadConfig()
    {
        var configFile = Path.Combine(
            Directory.GetCurrentDirectory(),
            Configuration.ConfigurationFileName
        );

        //if (Environment.GetEnvironmentVariable("USE_DEVELOPMENT_CONFIG") == "1")
        //{
        //    configFile = "development-config.json";
        //}

        if (File.Exists(configFile))
        {
            var json = await File.ReadAllTextAsync(configFile);
            return JsonConvert.DeserializeObject<Configuration>(json);
        }

        await _console.WriteWarningAsync("No configuration file found. Using default configuration parameters.");

        return null;
    }   
}