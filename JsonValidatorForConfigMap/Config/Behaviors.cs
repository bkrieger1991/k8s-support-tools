using Newtonsoft.Json.Converters;

namespace JsonValidatorForConfigMap.Config;

[Serializable]
public class Behaviors
{
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public ValidationBehavior MissingAnnotationsBehaviour { get; init; } = ValidationBehavior.Warn;
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public ValidationBehavior ResourceDeserializationError { get; init; } = ValidationBehavior.Warn;
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public ValidationBehavior MissingJsonDataInConfigMap { get; init; } = ValidationBehavior.Error;

    public ValidationKindBehavior[] ValidationErrorBehaviors { get; init; } = Array.Empty<ValidationKindBehavior>();
}