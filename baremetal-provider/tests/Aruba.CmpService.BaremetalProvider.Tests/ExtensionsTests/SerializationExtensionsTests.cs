using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.ExtensionsTests;
public class SerializationExtensionsTests : TestBase
{
    public SerializationExtensionsTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Unit]
    public async Task Json_Deserialize_Value()
    {
        var jsonValue = "{\"Value\":\"A\"}";
        var test = jsonValue.Deserialize<Test>();
        test.Should().NotBeNull();
        test.Value.Should().Be("A");
    }

    [Fact]
    [Unit]
    public async Task Json_Deserialize_Null()
    {
        var jsonValue = "";
        var test = jsonValue.Deserialize<Test>();
        test.Should().BeNull();
    }
    [Fact]
    [Unit]
    public async Task Json_Serialize_Value()
    {
        var jsonValue = "{\"Value\":\"A\"}";
        var test = new Test() { Value = "A" }.Serialize();
        test.Should().NotBeNullOrWhiteSpace();
        test.Should().Be(jsonValue);
    }

    [Fact]
    [Unit]
    public async Task Json_Serialize_Null()
    {
        var test = ((Test)null)?.Serialize();
        test.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Unit]
    public async Task Yaml_Serialize_Value()
    {
        var test = new Test() { Value = "A" }.ToYaml();
        test.Should().NotBeNullOrWhiteSpace();
        test.Substring(0, 8).Should().Be("Value: A");
    }

    [Fact]
    [Unit]
    public async Task Yaml_Serialize_null()
    {
        var test = ((Test)null).ToYaml();
        test.Should().BeNullOrWhiteSpace();
    }
}

internal class Test
{
    public string Value { get; set; }
}
