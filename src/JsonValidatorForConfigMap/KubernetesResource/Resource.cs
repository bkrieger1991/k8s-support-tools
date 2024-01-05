namespace JsonValidatorForConfigMap.KubernetesResource;

/// <summary>
/// Contains all relevant information of a K8s resource file
/// </summary>
public class Resource
{
    /// <summary>
    /// Kind of the resource file. If kind is not contained in <see cref="ResourceKind"/>, it defaults to <see cref="ResourceKind.Unknown"/>
    /// </summary>
    public ResourceKind Kind { get; init; } = ResourceKind.Unknown;
    /// <summary>
    /// Content of the resource file as string
    /// </summary>
    public string RawData { get; init; } = "";
    /// <summary>
    /// Deserialized data of the resource file. Key of dictionary is accessible as <see cref="string"/>
    /// </summary>
    public Dictionary<object, object> Data { get; init; } = new();
    /// <summary>
    /// Absolute file path to the resource file
    /// </summary>
    public string FilePath { get; init; } = "";
}