using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.ExtensionsTests;
public class StringExtensionsTests :
    TestBase
{
    public StringExtensionsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    [Unit]
    public void ToBase64_NotNull()
    {
        var a = "1".ToBase64();
        a.Should().Be("MQ==");
    }
    [Fact]
    [Unit]
    public void ToBase64_Null()
    {
        var a = ((string?)null).ToBase64();
        a.Should().BeNull();
    }
}
