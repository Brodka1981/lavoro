using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.ExtensionsTests;
public class PaginationExtensionsTests :
    TestBase
{
    public PaginationExtensionsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    [Unit]
    public void TestPagination_EmptyQueryString()
    {
        var list = new ListResponseDto<int>()
        {
            TotalCount = 10,
            Values = Enumerable.Range(0, 10).ToList()
        };
        var ret = list.FillPaginationData("http", "host", "path", null);
        ret.First.Should().BeNull();
        ret.Last.Should().BeNull();
        ret.Next.Should().BeNull();
        ret.Prev.Should().BeNull();
        ret.Self.Should().BeNull();
    }
    [Fact]
    [Unit]
    public void TestPagination_TotalCount_Greater_Than_Limit_Offset_0()
    {
        var list = new ListResponseDto<int>()
        {
            TotalCount = 10,
            Values = Enumerable.Range(0, 10).ToList()
        };
        var ret = list.FillPaginationData("http", "host", "path", "?limit=3&offset=0");
        ret.First.Should().BeNull();
        ret.Last.Should().Be("http://hostpath?limit=3&offset=9");
        ret.Next.Should().Be("http://hostpath?limit=3&offset=3");
        ret.Prev.Should().BeNull();
        ret.Self.Should().Be("http://hostpath?limit=3&offset=0");
    }

    [Fact]
    [Unit]
    public void TestPagination_TotalCount_Greater_Than_Limit_Offset_2()
    {
        var list = new ListResponseDto<int>()
        {
            TotalCount = 10,
            Values = Enumerable.Range(0, 10).ToList()
        };
        var ret = list.FillPaginationData("http", "host", "path", "?limit=3&offset=2");
        ret.First.Should().Be("http://hostpath?limit=3&offset=0");
        ret.Last.Should().Be("http://hostpath?limit=3&offset=9");
        ret.Next.Should().Be("http://hostpath?limit=3&offset=5");
        ret.Prev.Should().BeNull();
        ret.Self.Should().Be("http://hostpath?limit=3&offset=2");
    }

    [Fact]
    [Unit]
    public void TestPagination_Limit1_Offset_2()
    {
        var list = new ListResponseDto<int>()
        {
            TotalCount = 10,
            Values = Enumerable.Range(0, 10).ToList()
        };
        var ret = list.FillPaginationData("http", "host", "path", "?limit=1&offset=2");
        ret.First.Should().Be("http://hostpath?limit=1&offset=0");
        ret.Last.Should().Be("http://hostpath?limit=1&offset=9");
        ret.Next.Should().Be("http://hostpath?limit=1&offset=3");
        ret.Prev.Should().Be("http://hostpath?limit=1&offset=1");
        ret.Self.Should().Be("http://hostpath?limit=1&offset=2");
    }

    [Fact]
    [Unit]
    public void TestPagination_TotalCount_Lower_Than_Limit_Offset_0()
    {
        var list = new ListResponseDto<int>()
        {
            TotalCount = 10,
            Values = Enumerable.Range(0, 10).ToList()
        };
        var ret = list.FillPaginationData("http", "host", "path", "?limit=13&offset=0");
        ret.First.Should().BeNull();
        ret.Last.Should().BeNull();
        ret.Next.Should().BeNull();
        ret.Prev.Should().BeNull();
        ret.Self.Should().Be("http://hostpath?limit=13&offset=0");
    }

    [Fact]
    [Unit]
    public void TestPagination_TotalCount_Lower_Than_Limit_Offset_8()
    {
        var list = new ListResponseDto<int>()
        {
            TotalCount = 10,
            Values = Enumerable.Range(0, 10).ToList()
        };
        var ret = list.FillPaginationData("http", "host", "path", "?limit=13&offset=8");
        ret.First.Should().Be("http://hostpath?limit=13&offset=0");
        ret.Last.Should().Be("http://hostpath?limit=13&offset=0");
        ret.Next.Should().BeNull();
        ret.Prev.Should().BeNull();
        ret.Self.Should().Be("http://hostpath?limit=13&offset=8");
    }
}
