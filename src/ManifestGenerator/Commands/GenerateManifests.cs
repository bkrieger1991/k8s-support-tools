using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace ManifestGenerator.Commands;
[Serializable]
[Command("generate-manifest", Description = "Generates manifest files by given parameters")]
public class GenerateManifests : ICommand
{
    [CommandOption("interactive", IsRequired = true, Description = "Starts the interactive generator")]
    public bool? Interactive { get; init; } = false;

    [CommandOption("deployments", IsRequired = true, Description = "Provide a number of deployments to generate")]
    public int? DeploymentCount { get; init; } = 1;

    [CommandOption("deployments", IsRequired = true, Description = "Provide a number of deployments to generate")]
    public int? DeploymentCount { get; init; } = 1;


    public ValueTask ExecuteAsync(IConsole console)
    {
        
    }
}