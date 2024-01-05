using CliFx.Infrastructure;

namespace JsonValidatorForConfigMap.Helper;

public class ConsoleProvider : IConsoleProvider
{
    public IConsole Console { get; init; }

    public ConsoleProvider(IConsole console)
    {
        Console = console;
    }
}