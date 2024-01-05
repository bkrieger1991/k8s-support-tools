using CliFx.Infrastructure;

namespace JsonValidatorForConfigMap.Helper;

public static class ConsoleExtensions
{
    public static Task WriteLineAsync(this IConsole console, string message, ConsoleColor foreground)
    {
        using (console.WithForegroundColor(foreground))
        {
            return console.Output.WriteLineAsync(message);
        }
    }

    public static Task WriteInfoAsync(this IConsole console, string message)
    {
        return console.WriteLineAsync($"Info: {message}", ConsoleColor.DarkCyan);
    }

    public static Task WriteSuccessAsync(this IConsole console, string message)
    {
        return console.WriteLineAsync($"Success: {message}", ConsoleColor.DarkGreen);
    }

    public static Task WriteErrorAsync(this IConsole console, string message)
    {
        return console.WriteLineAsync($"Error: {message}", ConsoleColor.Red);
    }

    public static Task WriteWarningAsync(this IConsole console, string message)
    {
        return console.WriteLineAsync($"Warning: {message}", ConsoleColor.DarkYellow);
    }
}