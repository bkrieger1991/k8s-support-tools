using JsonValidatorForConfigMap.Config;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace JsonValidatorForConfigMap.KubernetesResource;

/// <summary>
/// This class utilizes the <see cref="YamlDotNet"/> to read K8s resource files from a dictionary.
/// If successfully deserialized, you get a list of <see cref="Resource"/>'s where you can access path,
/// resource kind and data as <see cref="Dictionary{TKey,TValue}"/>.
/// </summary>
public class ResourceFileReader
{
    private readonly ILogger<ResourceFileReader> _logger;
    private readonly Configuration _config;

    public ResourceFileReader(ILogger<ResourceFileReader> logger, Configuration config)
    {
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Reads all deserializable resource files from the given directory including all sub-directories
    /// </summary>
    /// <param name="path">Path to directory</param>
    /// <param name="deserializationErrorCallback">Decide what to do with a deserialization error without interrupting the read-process</param>
    /// <returns></returns>
    public async Task<Resource[]> ReadFromPathAsync(string path, Func<string, Exception, Task> deserializationErrorCallback)
    {
        var files = Directory.GetFiles(path, _config.YamlFilePattern, SearchOption.AllDirectories);
        return await ReadFromFilesAsync(files, deserializationErrorCallback);
    }

    private async Task<Resource[]> ReadFromFilesAsync(string[] files, Func<string, Exception, Task> deserializationErrorCallback)
    {
        var resources = new List<Resource>();
        foreach (var file in files)
        {
            _logger.LogTrace($"Reading yaml file: {file}");
            var resource = await ReadResourceFromFile(file, deserializationErrorCallback);

            if (resource == null)
            {
                continue;
            }
            resources.Add(resource);
        }

        return resources.ToArray();
    }

    private async Task<Resource?> ReadResourceFromFile(
        string filepath,
        Func<string, Exception, Task> deserializationErrorCallback
    )
    {
        var content = await File.ReadAllTextAsync(filepath);
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            var data = deserializer.Deserialize<Dictionary<object, object>>(content);
            return new Resource()
            {
                Data = data,
                FilePath = filepath,
                RawData = content,
                Kind = GetResourceKind(data)
            };
        }
        catch (Exception e)
        {
            await deserializationErrorCallback(filepath, e);
        }

        return null;
    }

    /// <summary>
    /// Helper method to extract the resource kind from the data.
    /// If no "Kind" property is set in resource file or the value does not match any value in <see cref="ResourceKind"/>
    /// It returns the Unknown type.
    /// </summary>
    /// <param name="resourceData"></param>
    /// <returns></returns>
    private ResourceKind GetResourceKind(IDictionary<object, object> resourceData)
    {
        var kind = resourceData.ContainsKey("kind") ? resourceData["kind"].ToString() : "";
        if (Enum.TryParse<ResourceKind>(kind, true, out var resourceKind))
        {
            return resourceKind;
        }

        return ResourceKind.Unknown;
    }
}