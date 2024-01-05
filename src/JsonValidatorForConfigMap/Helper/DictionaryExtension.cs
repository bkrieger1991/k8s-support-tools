namespace JsonValidatorForConfigMap.Helper;

public static class DictionaryExtension
{
    /// <summary>
    /// Lookup the given path in a dictionary of type $lt;string, object$gt;, where child objects should also be of type $lt;string, object$gt; unless it's a value.
    /// Path segments separator must match the given value. Segments are searched case insensitive.
    /// Example: access a value in nested dictionaries by "my.nested.value".
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="path">The path to a value you want to extract from dictionary</param>
    /// <param name="segmentSeparator">Separator between path segments. Defaults to "."</param>
    /// <returns>The found value or null, if the path does not exist</returns>
    public static object? GetValueByPath(this IDictionary<object, object> dictionary, string path, string segmentSeparator = ".")
    {
        var pathSegments = path.Split(segmentSeparator);
        var firstSegment = pathSegments.FirstOrDefault();

        // Try casting dictionary key to string
        var castedDictionary = dictionary.CastObjectKeyToString();

        if (firstSegment == null || !castedDictionary.ContainsKeyInsensitive(firstSegment))
        {
            return null;
        }

        var value = castedDictionary[firstSegment];

        if (value is IDictionary<object, object> nested)
        {
            return nested.GetValueByPath(string.Join(segmentSeparator, pathSegments.Skip(1)), segmentSeparator);
        }

        return value;
    }

    private static bool ContainsKeyInsensitive(
        this IDictionary<string, object> dictionary,
        string key
    )
    {
        return dictionary.Keys.FirstOrDefault(k => k.ToString().ToLower() == key.ToLower()) != null;
    }

    private static IDictionary<string, object> CastObjectKeyToString(
        this IDictionary<object, object> dictionary
    )
    {
        // Try each key is convertible to string
        var hasNullKeys = dictionary.Keys.Select(k => k.ToString()).Any(k => k == null);
        if (hasNullKeys)
        {
            throw new Exception("Given dictionary keys can't be converted to string");
        }

        return dictionary.ToDictionary(d => d.Key.ToString()!, d => d.Value);
    }
}