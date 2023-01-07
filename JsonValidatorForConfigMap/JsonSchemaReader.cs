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

    public async Task<JsonSchema> ReadSchemaFromJsonData(string filepath)
    {
        CheckFileAndThrow(filepath);

        var jsonContent = await File.ReadAllTextAsync(filepath);
        var schema = JsonSchema.FromSampleJson(jsonContent);
        schema.AllowAdditionalItems = false;
        schema.AllowAdditionalProperties = false;
        schema.IsExclusiveMinimum = true;

        ToggleAllPropertiesRequired(schema);

        return schema;
    }

    public async Task<JsonSchema> ReadSchemaFromFile(string filepath)
    {
        CheckFileAndThrow(filepath);
        return await JsonSchema.FromFileAsync(filepath);
    }

    private void ToggleAllPropertiesRequired(JsonSchema schema)
    {
        foreach (var prop in schema.ActualProperties)
        {
            schema.RequiredProperties.Add(prop.Key);
            // Recursive call toggle method with schema of
            // property, to set all child props to be required
            ToggleAllPropertiesRequired(prop.Value.ActualSchema);
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