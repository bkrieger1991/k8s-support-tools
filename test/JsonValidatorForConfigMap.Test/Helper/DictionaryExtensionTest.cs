using FluentAssertions;
using JsonValidatorForConfigMap.Helper;

namespace JsonValidatorForConfigMap.Test.Helper;

public class DictionaryExtensionTest
{
    [Theory]
    [InlineData("Key2", "Value2")]
    [InlineData("Nested.NestedKey2", "NestedValue2")]
    [InlineData("Nested.Nested2.DeepNestedKey", "DeepNestedValue")]
    public void Should_Output_Correct_Value_By_Path(string path, string expected)
    {
        var dictionary = CreateDictionary();
        dictionary.GetValueByPath(path).Should().Be(expected);
    }

    private Dictionary<object, object> CreateDictionary()
    {
        return new Dictionary<object, object>()
        {
            ["Key1"] = "Value1",
            ["Key2"] = "Value2",
            ["Nested"] = new Dictionary<object, object>()
            {
                ["NestedKey1"] = "NestedValue1",
                ["NestedKey2"] = "NestedValue2",
                ["Nested2"] = new Dictionary<object, object>()
                {
                    ["DeepNestedKey"] = "DeepNestedValue"
                }
            },
            ["ArrayKey"] = new Dictionary<object, object>[]
            {
                new()
                {
                    ["NestedArrayKey1"] = "NestedArrayValue1",
                    ["NestedArrayKey2"] = "NestedArrayValue2",
                    ["Nested"] = new Dictionary<object, object>()
                    {
                        ["DeepNestedArrayKey"] = "DeepNestedArrayValue"
                    }
                }
            }
        };
    }
}