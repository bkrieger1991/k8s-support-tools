using System.Reflection;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JsonValidatorForConfigMap.Config;
using JsonValidatorForConfigMap.Helper;
using Newtonsoft.Json;

namespace JsonValidatorForConfigMap.Commands;

[Serializable]
[Command("install-pre-commit-hook", Description = "Installs the json-schema validator into a git-repository as pre-commit hook")]
public class InstallPreCommitHook : ICommand
{
    private const string PreCommitHookFileName = "pre-commit";
    private const string PreCommitHookTemplateFileName = "pre-commit.tmpl.sh";

    private readonly Configuration _config;
    private readonly IConsole _console;

    [CommandOption("repo", IsRequired = true, Description = "The absolute path to a git-repository, where the pre-commit hook should be installed.")]
    public string? GitRepositoryDirectory { get; init; } = default;

    public InstallPreCommitHook(IConsoleProvider consoleProvider, Configuration config)
    {
        _config = config;
        _console = consoleProvider.Console;
    }
    
    public async ValueTask ExecuteAsync(IConsole _)
    {
        if (!Directory.Exists(GitRepositoryDirectory))
        {
            throw new CommandException(
                $"Directory '{GitRepositoryDirectory}' does not exist, or is no directory"
            );
        }

        var gitHooksDir = Path.GetFullPath(Path.Combine(GitRepositoryDirectory, Configuration.GitHooksDirectory));

        if (!Directory.Exists(gitHooksDir))
        {
            throw new CommandException(
                $"The provided directory '{GitRepositoryDirectory}' seems not to be a git-repository directory. Correct the directory path or initialize a git repo using 'git init'."
            );
        }

        // Copy the configuration to the directory of the repository. So user can
        // commit the file and share same configuration with members
        await WriteConfigFile(GitRepositoryDirectory);

        // Write a pre-commit hook or update an existing
        await WriteOrUpdateHook(gitHooksDir);

        await _console.WriteSuccessAsync(
            "Installed pre-commit hook into your repository. " +
            "For individual configuration, see configuration file in repo"
        );
    }

    private async Task WriteOrUpdateHook(string gitHooksDir)
    {
        // Check if a pre-commit hook already exists. If so, append the validation-call to keep the existing hook
        var preCommitHookPath = Path.Combine(gitHooksDir, PreCommitHookFileName);
        if (File.Exists(preCommitHookPath))
        {
            await File.AppendAllTextAsync(preCommitHookPath, await BuildHook(false));
        }
        else
        {
            await File.WriteAllTextAsync(preCommitHookPath, await BuildHook(true));
        }
    }

    private async Task WriteConfigFile(string directory)
    {
        var configFilePath = Path.Combine(
            directory,
            Configuration.ConfigurationFileName
        );
        // But don't overwrite
        if (!File.Exists(configFilePath))
        {
            await File.WriteAllTextAsync(
                configFilePath,
                JsonConvert.SerializeObject(_config, Formatting.Indented)
            );
        }
    }

    private async Task<string> BuildHook(bool prependBinSh)
    {
        var prepend = prependBinSh ? "#!bin/sh" : "";
        var template = await File.ReadAllTextAsync(PreCommitHookTemplateFileName);

        return prepend + template
            .Replace("{toolPath}", Assembly.GetExecutingAssembly().Location)
            .Replace("{repoPath}", GitRepositoryDirectory);
    }
}