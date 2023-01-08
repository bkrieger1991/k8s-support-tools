using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using JsonValidatorForConfigMap.Config;
using JsonValidatorForConfigMap.Helper;
using JsonValidatorForConfigMap.KubernetesResource;
using Microsoft.Extensions.Logging;
using NJsonSchema;
using NJsonSchema.Validation;

namespace JsonValidatorForConfigMap.Commands;

/// <summary>
/// This is the console-command mapped to cli command "validate-configmaps".
/// It validates an arbitrary json-value in a K8s ConfigMap resource against a json-schema or a sample-json file.
/// The json-schema (or sample-json) must be referenced in metadata.annotations.* properties within each ConfigMap file.
/// </summary>
[Serializable]
[Command("validate-configmaps", Description = "Validates the schema of a json-value in a K8s ConfigMap resource. Schema file or sample json file must be referenced by annotations.")]
public class ValidateConfigMapSchema : ICommand
{
    private readonly ILogger<ValidateConfigMapSchema> _logger;
    private readonly ResourceFileReader _resourceFileReader;
    private readonly JsonSchemaReader _jsonSchemaReader;
    private readonly Configuration _config;
    private readonly IConsole _console;
    private bool _finalResult = true;

    [CommandOption("root-path", IsRequired = true, Description = "Provide the path of your solution folder dealing as root path for finding *.yaml files that contains k8s resources.")]
    public string? RootPath { get; init; } = default;

    public ValidateConfigMapSchema(
        ILogger<ValidateConfigMapSchema> logger, 
        ResourceFileReader resourceFileReader, 
        JsonSchemaReader jsonSchemaReader,
        IConsoleProvider consoleProvider,
        Configuration config
    )
    {
        _logger = logger;
        _resourceFileReader = resourceFileReader;
        _jsonSchemaReader = jsonSchemaReader;
        _console = consoleProvider.Console;
        _config = config;
    }

    public async ValueTask ExecuteAsync(IConsole _)
    {
        _logger.LogTrace($"Command {nameof(ValidateConfigMapSchema)} called with root-path '{RootPath}'");
        if (!Directory.Exists(RootPath))
        {
            throw new CommandException("Given root-path seems not to be an existing directory");
        }

        _logger.LogTrace("Searching for kubernetes resource files...");
        var resources = await _resourceFileReader.ReadFromPathAsync(RootPath, HandleDeserializationError);

        var configMaps = resources
            .Where(r => r.Kind == ResourceKind.ConfigMap)
            .ToArray();

        foreach (var configMap in configMaps)
        {
            await ProcessConfigMapResource(configMap);
        }

        if (!_finalResult)
        {
            throw new CommandException("Validation failed. See validation errors.");
        }

        await _console.WriteSuccessAsync("All files validated successful");
    }

    private async Task HandleDeserializationError(string filepath, Exception e)
    {
        _logger.LogWarning(e, $"Error when deserializing yaml file: {filepath}. Message: {e.Message}");
        await HandleMessageByBehavior($"Can't deserialize resource file: {filepath}", _config.Behaviors.ResourceDeserializationError);
    }

    /// <summary>
    /// Handles a certain message with a given behavior.
    /// See possible behaviors in <see cref="ValidationBehavior"/> enum.
    /// Error = Output error + remember overall failure
    /// Warn = Output warning
    /// Ignore = Suppress output
    /// </summary>
    /// <param name="message">The message that should be written to the console</param>
    /// <param name="behavior">The behavior that decides how to handle the message</param>
    /// <returns></returns>
    private async Task HandleMessageByBehavior(string message, ValidationBehavior behavior)
    {
        if (behavior == ValidationBehavior.Error)
        {
            _finalResult = false;
            await _console.WriteErrorAsync(message);
        }
        else if (behavior == ValidationBehavior.Warn)
        {
            await _console.WriteWarningAsync(message);
        }
        else if (behavior == ValidationBehavior.Ignore)
        {
            return;
        }
        _logger.LogInformation($"Message '{message}' handled with behavior {behavior}");
    }


    private async Task ProcessConfigMapResource(Resource configMap)
    {
        await _console.WriteInfoAsync($"Processing ConfigMap file: {configMap.FilePath}");
        var schema = await ExtractSchemaFromConfigMap(configMap);
        if (schema == null)
        {
            return;
        }

        var dataKey = configMap.Data.GetValueByPath(
            $"metadata.annotations.{_config.DataKeyAnnotationName}"
        )?.ToString() ?? _config.DefaultDataKey;
        _logger.LogTrace($"Got json-data key: {dataKey}");

        // In some cases, data-keys might contain "." chars, so use "//" as path separator
        var jsonData = configMap.Data.GetValueByPath($"data//{dataKey}", "//")?.ToString();
        if (jsonData == null)
        {
            await HandleMessageByBehavior(
                $"No json-data found in ConfigMap with data-key '{dataKey}'. Can't validate ConfigMap: {configMap.FilePath}",
                _config.Behaviors.MissingJsonDataInConfigMap
            );
            return;
        }

        var validationResult = schema.Validate(jsonData, new JsonSchemaValidatorSettings()
        {
            PropertyStringComparer = StringComparer.CurrentCulture
        });
        await OutputValidationResult(validationResult);

        if(validationResult.Count == 0)
        {
            await _console.WriteSuccessAsync("No validation issues found.");
        }
    }

    /// <summary>
    /// This method outputs the list of <see cref="ValidationError"/> and outputs messages.
    /// The <see cref="ValidationErrorKind"/> of each error is looked up in the configuration (<see cref="Behaviors"/>)
    /// to decide, how to handle different error kinds.
    /// </summary>
    /// <param name="validationResult"></param>
    /// <returns></returns>
    private async Task OutputValidationResult(ICollection<ValidationError> validationResult, bool nested = false)
    {
        foreach (var error in validationResult)
        {
            // Lookup validation error kind in configuration
            var behaviorConfig =
                _config.Behaviors.ValidationErrorBehaviors.FirstOrDefault(
                    b => b.ErrorKind == error.Kind
                );

            // If no configuration for this certain error kind found, default to error
            var behavior = behaviorConfig?.Behavior ?? ValidationBehavior.Error;

            await HandleMessageByBehavior(
                $"{(nested ? "==> " : "")}Validation Error: {error.Kind} - {error.Path}, Line: {error.LineNumber}",
                behavior
            );

            if (error is ChildSchemaValidationError child && child.Errors.Count > 0)
            {
                foreach (var childValidationErrors in child.Errors.Values)
                {
                    await OutputValidationResult(childValidationErrors, true);
                }
            }
        }
    }

    /// <summary>
    /// This methods extracts a json-schema <see cref="JsonSchema"/> from a given K8s
    /// ConfigMap resource by analyzing values ConfigMap's annotations.
    /// </summary>
    /// <param name="configMapResource"></param>
    /// <returns></returns>
    /// <exception cref="CommandException"></exception>
    private async Task<JsonSchema?> ExtractSchemaFromConfigMap(Resource configMapResource)
    {
        var jsonDataPath = configMapResource.Data.GetValueByPath(
            $"metadata.annotations.{_config.SampleJsonFilePathAnnotationName}"
        )?.ToString();
        var jsonSchemaPath = configMapResource.Data.GetValueByPath(
            $"metadata.annotations.{_config.JsonSchemaPathAnnotationName}"
        )?.ToString();
        
        if (jsonDataPath == null && jsonSchemaPath == null)
        {
            await HandleMessageByBehavior(
                $"Annotations not set correctly in ConfigMap: {configMapResource.FilePath}",
                _config.Behaviors.MissingAnnotationsBehaviour
            );
            return null;
        }

        if (jsonDataPath != null)
        {
            _logger.LogTrace($"Got json-data path: {jsonDataPath}");
            _logger.LogTrace($"Thus, we extract a json-schema from a json-file");
            return await _jsonSchemaReader.ReadSchemaFromSampleJson(GetAbsolutePath(configMapResource, jsonDataPath));
        }

        if (jsonSchemaPath != null)
        {
            _logger.LogTrace($"Got json-schema path: {jsonSchemaPath}");
            _logger.LogTrace($"Thus, we will use the given json-schema");
            return await _jsonSchemaReader.ReadSchemaFromFile(GetAbsolutePath(configMapResource, jsonSchemaPath));
        }

        throw new CommandException(
            "Something unexpected happened. Neither json-data nor " +
            "json-schema path are successfully extracted from " +
            $"annotations of your ConfigMap file: {configMapResource.FilePath}"
        );
    }
    
    /// <summary>
    /// Helper method to get the absolute path of a relative path from the root of a K8s resource.
    /// </summary>
    /// <param name="resource"></param>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    private string GetAbsolutePath(Resource resource, string relativePath)
    {
        var combined = Path.Combine(Path.GetDirectoryName(resource.FilePath)!, relativePath);
        return Path.GetFullPath(combined);
    }
}