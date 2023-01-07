using Newtonsoft.Json.Converters;
using NJsonSchema.Validation;

namespace JsonValidatorForConfigMap.Config;

[Serializable]
public class ValidationKindBehavior
{
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public ValidationErrorKind ErrorKind { get; init; } = ValidationErrorKind.Unknown;
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public ValidationBehavior Behavior { get; init; } = ValidationBehavior.Error;
}