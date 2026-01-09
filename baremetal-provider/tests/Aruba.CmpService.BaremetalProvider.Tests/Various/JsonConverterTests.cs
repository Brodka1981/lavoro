using System.Text;
using System.Text.Json;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Serialization;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.Various;
public class JsonConverterTests :
    TestBase
{
    public JsonConverterTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Unit]
    public void JsonConverter_1()
    {
        var span = Encoding.UTF8.GetBytes("{\"a\":1}").AsSpan();
        var jReadr = new Utf8JsonReader(span);

        var converter = new EnumJsonConverter<ServiceType>();
        while (jReadr.Read())
        {
            if (jReadr.TokenType == JsonTokenType.Number)
            {
                var ret = converter.Read(ref jReadr, typeof(ServiceType), new JsonSerializerOptions());
                ret.Should().Be(ServiceType.WebDav);
                break;
            }
        }
    }

    [Fact]
    [Unit]
    public void JsonConverter_2()
    {
        var span = Encoding.UTF8.GetBytes("{\"a\":\"WebDav\"}").AsSpan();
        var jReadr = new Utf8JsonReader(span);

        var converter = new EnumJsonConverter<ServiceType>();
        while (jReadr.Read())
        {
            if (jReadr.TokenType == JsonTokenType.String)
            {
                var ret = converter.Read(ref jReadr, typeof(ServiceType), new JsonSerializerOptions());
                ret.Should().Be(ServiceType.WebDav);
                break;
            }
        }
    }

    [Fact]
    [Unit]
    public void JsonConverter_3()
    {
        var span = Encoding.UTF8.GetBytes("{\"a\":1}").AsSpan();
        var jReadr = new Utf8JsonReader(span);

        var converter = new EnumJsonConverter<ServiceType?>();
        while (jReadr.Read())
        {
            if (jReadr.TokenType == JsonTokenType.Number)
            {
                var ret = converter.Read(ref jReadr, typeof(ServiceType?), new JsonSerializerOptions());
                ret.Should().Be(ServiceType.WebDav);
                break;
            }
        }
    }

    [Fact]
    [Unit]
    public void JsonConverter_4()
    {
        var span = Encoding.UTF8.GetBytes("{\"a\":\"WebDav\"}").AsSpan();
        var jReadr = new Utf8JsonReader(span);

        var converter = new EnumJsonConverter<ServiceType?>();
        while (jReadr.Read())
        {
            if (jReadr.TokenType == JsonTokenType.String)
            {
                var ret = converter.Read(ref jReadr, typeof(ServiceType?), new JsonSerializerOptions());
                ret.Should().Be(ServiceType.WebDav);
                break;
            }
        }
    }

    [Fact]
    [Unit]
    public void JsonConverter_5()
    {
        var span = Encoding.UTF8.GetBytes("{\"a\":89}").AsSpan();
        var jReadr = new Utf8JsonReader(span);

        var converter = new EnumJsonConverter<ServiceType?>();
        while (jReadr.Read())
        {
            if (jReadr.TokenType == JsonTokenType.Number)
            {
                var ret = converter.Read(ref jReadr, typeof(ServiceType?), new JsonSerializerOptions());
                ret.Should().BeNull();
                break;
            }
        }
    }

    [Fact]
    [Unit]
    public void JsonConverter_6()
    {
        var span = Encoding.UTF8.GetBytes("{\"a\":\"WebDava\"}").AsSpan();
        var jReadr = new Utf8JsonReader(span);

        var converter = new EnumJsonConverter<ServiceType?>();
        while (jReadr.Read())
        {
            if (jReadr.TokenType == JsonTokenType.String)
            {
                var ret = converter.Read(ref jReadr, typeof(ServiceType?), new JsonSerializerOptions());
                ret.Should().BeNull();
                break;
            }
        }
    }
}