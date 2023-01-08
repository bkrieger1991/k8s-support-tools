using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace JsonValidatorForConfigMap.Test;

public class JsonSchemaReaderTest
{
    [Fact]
    public async Task Should_Throw_If_Schema_File_Not_Exist()
    {
        var reader = new JsonSchemaReader(new NullLogger<JsonSchemaReader>());
        await reader
            .Awaiting(r => r.ReadSchemaFromFile("non-existing"))
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("File not found:*");
    }

    [Fact]
    public async Task Should_Throw_If_Sample_Json_File_Not_Exist()
    {
        var reader = new JsonSchemaReader(new NullLogger<JsonSchemaReader>());
        await reader
            .Awaiting(r => r.ReadSchemaFromSampleJson("non-existing"))
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("File not found:*");
    }

    [Fact]
    public async Task Should_Toggle_Every_Property_To_Required_When_Reading_Sample_Json()
    {
        var reader = new JsonSchemaReader(new NullLogger<JsonSchemaReader>());
        var schema = await reader.ReadSchemaFromSampleJson("JsonSchemaReaderTest.SampleJson.json");

        schema.RequiredProperties
            .Should()
            .HaveCount(3)
            .And.ContainInOrder("ExampleString", "Nested", "Array");

        schema.ActualProperties["Nested"].ActualSchema.RequiredProperties
            .Should()
            .HaveCount(2)
            .And.ContainInOrder("NestedInt", "NestedBool");

        schema.Definitions["Array"].ActualSchema.RequiredProperties
            .Should()
            .HaveCount(1)
            .And.ContainInOrder("ArrayObjString");
    }

    [Fact]
    public async Task Should_Forbid_Additional_Properties_In_All_Definitions_When_Reading_Sample_Json()
    {
        var reader = new JsonSchemaReader(new NullLogger<JsonSchemaReader>());
        var schema = await reader.ReadSchemaFromSampleJson("JsonSchemaReaderTest.SampleJson.json");

        schema.AllowAdditionalItems.Should().BeTrue();
        schema.AllowAdditionalProperties.Should().BeFalse();

        schema.ActualProperties["Nested"].ActualSchema.AllowAdditionalItems.Should().BeTrue();
        schema.ActualProperties["Nested"].ActualSchema.AllowAdditionalProperties.Should().BeFalse();

        schema.ActualProperties["Array"].ActualSchema.AllowAdditionalItems.Should().BeTrue();
        schema.ActualProperties["Array"].ActualSchema.AllowAdditionalProperties.Should().BeFalse();
    }
}