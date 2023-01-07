using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;

namespace JsonValidatorForConfigMap.Config;

[Serializable]
public class Configuration
{
    public const string ConfigurationFileName = "json-schema-validator.json";
    public const string GitHooksDirectory = ".git/hooks";

    public string SampleJsonFilePathAnnotationName { get; init; } = "k8s-support-tools/sample-json-path";
    public string JsonSchemaPathAnnotationName { get; init; } = "k8s-support-tools/json-schema-path";
    public string DataKeyAnnotationName { get; init; } = "k8s-support-tools/json-data-key";
    public string DefaultDataKey { get; init; } = "config.json";
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public LogLevel LoggingSeverity { get; init; } = LogLevel.None;
    public string YamlFilePattern { get; init; } = "*.y*ml";
    public Behaviors Behaviors { get; init; } = new();
}