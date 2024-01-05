using Microsoft.Extensions.Logging;
using NJsonSchema;

namespace JsonValidatorForConfigMap;


/// <summary>
/// This class is capable of reading a json-schema out of json-data or a json-schema file
/// </summary>
public class JsonSchemaReader
{
    private readonly ILogger<JsonSchemaReader> _logger;

    public JsonSchemaReader(ILogger<JsonSchemaReader> logger)
    {
        _logger = logger;
    }

    public async Task<JsonSchema> ReadSchemaFromSampleJson(string filepath)
    {
        CheckFileAndThrow(filepath);

        var jsonContent = await File.ReadAllTextAsync(filepath);
        var schema = JsonSchema.FromSampleJson(jsonContent);
        schema.IsExclusiveMinimum = true;

        _logger.LogDebug("Successful read sample-json and extracted schema");
        ToggleForbiddenAdditionalsInAllDefinitions(schema);
        ToggleAllPropertiesRequired(schema);

        return schema;
    }

    public async Task<JsonSchema> ReadSchemaFromFile(string filepath)
    {
        CheckFileAndThrow(filepath);
        return await JsonSchema.FromFileAsync(filepath);
    }

    private void ToggleForbiddenAdditionalsInAllDefinitions(JsonSchema schema)
    {
        schema.AllowAdditionalProperties = false;

        foreach (var definition in schema.Definitions)
        {
            _logger.LogInformation($"Toggle additional properties and items to be forbidden for definition '{definition.Key}'");
            ToggleForbiddenAdditionalsInAllDefinitions(definition.Value);
        }

        foreach (var property in schema.ActualProperties)
        {
            _logger.LogInformation($"Toggle additional properties and items to be forbidden for property '{property.Key}'");
            ToggleForbiddenAdditionalsInAllDefinitions(property.Value.ActualSchema);
        }
    }

    private void ToggleAllPropertiesRequired(JsonSchema schema)
    {
        foreach (var prop in schema.ActualProperties)
        {
            _logger.LogInformation($"Toggle property '{prop.Key}' to be required");
            schema.RequiredProperties.Add(prop.Key);
        }

        foreach (var definition in schema.Definitions)
        {
            _logger.LogInformation($"Toggle properties in definition '{definition.Key}'...");
            
            // Recursive call toggle method with schemas of each definition,
            // to set all child props to be required
            definition.Value.ActualProperties.Keys
                .ToList()
                .ForEach(definition.Value.ActualSchema.RequiredProperties.Add);
        }
    }
    
    private void CheckFileAndThrow(string filepath)
    {
        if (!File.Exists(filepath))
        {
            throw new Exception($"File not found: {filepath}");
        }
    }
}