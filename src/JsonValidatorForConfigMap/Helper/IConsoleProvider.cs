using CliFx.Infrastructure;

namespace JsonValidatorForConfigMap.Helper;

public interface IConsoleProvider
{
    public IConsole Console { get; }
}