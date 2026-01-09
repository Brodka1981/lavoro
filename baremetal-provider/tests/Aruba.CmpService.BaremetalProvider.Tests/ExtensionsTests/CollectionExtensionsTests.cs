using System.Collections.ObjectModel;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Pagination;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.ExtensionsTests;
public class CollectionExtensionsTests :
    TestBase
{
    public CollectionExtensionsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    [Unit]
    public void Dictionary_AddRange()
    {
        var source = new Dictionary<long, long>() { { 1, 1 }, { 2, 2 } };
        var toAdd = new Dictionary<long, long>() { { 3, 3 }, { 4, 4 } };

        source.AddRange(toAdd);

        source.Should().HaveCount(4);
        source.Should().ContainKey(4);

    }
    [Fact]
    [Unit]
    public void Dictionary_AddRange_Null()
    {
        var source = new Dictionary<long, long>() { { 1, 1 }, { 2, 2 } };

        source.AddRange(null);

        source.Should().HaveCount(2);
        source.Should().NotContainKey(3);
    }

    [Fact]
    [Unit]
    public void Collection_Replace()
    {
        var source = new Collection<long>() { 1, 2, 3, 4 };
        var toReplace = new Collection<long>() { 5, 6 };

        source.Replace(toReplace);

        source.Should().HaveCount(2);
        source.Should().Contain(5);
        source.Should().NotContain(1);
    }

    [Fact]
    [Unit]
    public void ILIst_ToCollection()
    {
        var list = new List<long>() { 1, 2, 3, 4 };
        var collection = list.ToCollection();

        collection.Should().BeOfType<Collection<long>>();
    }

    [Fact]
    [Unit]
    public void Page_NoOffset_NoLimit_Internal()
    {
        var collection = Enumerable.Range(1, 1000).ToList();
        var pagination = new PagingDefinition();
        var paged = collection.Page(pagination, false).ToList();
        paged.Should().HaveCount(1000);
        paged.First().Should().Be(1);
        paged.Last().Should().Be(1000);
    }

    [Fact]
    [Unit]
    public void Page_NoOffset_NoLimit_External()
    {
        var collection = Enumerable.Range(1, 1000).ToList();
        var pagination = new PagingDefinition();
        var paged = collection.Page(pagination, true).ToList();
        paged.Should().HaveCount(100);
        paged.First().Should().Be(1);
        paged.Last().Should().Be(100);
    }

    [Fact]
    [Unit]
    public void Page_YesOffset_NoLimit_Internal()
    {
        var collection = Enumerable.Range(1, 1000).ToList();
        var pagination = new PagingDefinition() { Offset = 1 };
        var paged = collection.Page(pagination, false).ToList();
        paged.Should().HaveCount(999);
        paged.First().Should().Be(2);
        paged.Last().Should().Be(1000);
    }

    [Fact]
    [Unit]
    public void Page_YesOffset_NoLimit_External()
    {
        var collection = Enumerable.Range(1, 1000).ToList();
        var pagination = new PagingDefinition() { Offset = 1 };
        var paged = collection.Page(pagination, true).ToList();
        paged.Should().HaveCount(100);
        paged.First().Should().Be(2);
        paged.Last().Should().Be(101);
    }

    [Fact]
    [Unit]
    public void Page_NoOffset_YesLimit_Internal()
    {
        var collection = Enumerable.Range(1, 1000).ToList();
        var pagination = new PagingDefinition() { Limit = 101 };
        var paged = collection.Page(pagination, false).ToList();
        paged.Should().HaveCount(101);
        paged.First().Should().Be(1);
        paged.Last().Should().Be(101);
    }

    [Fact]
    [Unit]
    public void Page_NoOffset_YesLimit101_External()
    {
        var collection = Enumerable.Range(1, 1000).ToList();
        var pagination = new PagingDefinition() { Limit = 101 };
        var paged = collection.Page(pagination, true).ToList();
        paged.Should().HaveCount(100);
        paged.First().Should().Be(1);
        paged.Last().Should().Be(100);
    }

    [Fact]
    [Unit]
    public void Page_NoOffset_YesLimit99_External()
    {
        var collection = Enumerable.Range(1, 1000).ToList();
        var pagination = new PagingDefinition() { Limit = 2 };
        var paged = collection.Page(pagination, true).ToList();
        paged.Should().HaveCount(2);
        paged.First().Should().Be(1);
        paged.Last().Should().Be(2);
    }

    [Fact]
    [Unit]
    public void Page_YesOffset_YesLimit_Internal()
    {
        var collection = Enumerable.Range(1, 1000).ToList();
        var pagination = new PagingDefinition() { Offset = 1, Limit = 101 };
        var paged = collection.Page(pagination, false).ToList();
        paged.Should().HaveCount(101);
        paged.First().Should().Be(2);
        paged.Last().Should().Be(102);
    }

    [Fact]
    [Unit]
    public void Page_YesOffset_YesLimit_External()
    {
        var collection = Enumerable.Range(1, 1000).ToList();
        var pagination = new PagingDefinition() { Offset = 1, Limit = 2 };
        var paged = collection.Page(pagination, true).ToList();
        paged.Should().HaveCount(2);
        paged.First().Should().Be(2);
        paged.Last().Should().Be(3);
    }

    [Fact]
    [Unit]
    public void SortBy_Ascending()
    {
        var collection = Enumerable.Range(0, 10).Select(s => new TestSort()
        {
            Id1 = s,
            Id2 = 100 - s

        }).ToList();

        var sorted = collection.SortBy(s => s.Id1, true).ToList();
        sorted.Should().HaveCount(collection.Count);
        sorted.First().Id1.Should().Be(0);
        sorted.First().Id2.Should().Be(100);
        sorted.Last().Id1.Should().Be(9);
        sorted.Last().Id2.Should().Be(91);
    }

    [Fact]
    [Unit]
    public void SortBy_Descending()
    {
        var collection = Enumerable.Range(0, 10).Select(s => new TestSort()
        {
            Id1 = s,
            Id2 = 100 - s

        }).ToList();

        var sorted = collection.SortBy(s => s.Id1, false).ToList();
        sorted.Should().HaveCount(collection.Count);
        sorted.Last().Id1.Should().Be(0);
        sorted.Last().Id2.Should().Be(100);
        sorted.First().Id1.Should().Be(9);
        sorted.First().Id2.Should().Be(91);
    }

    [Fact]
    [Unit]
    public void SortBy_Ascending_ThenByAscending()
    {
        var collection = Enumerable.Range(0, 10).Select(s => new TestSort()
        {
            Id1 = s % 2,
            Id2 = 100 - s

        }).ToList();

        var sorted = collection.SortBy(s => s.Id1, true).SortBy(s => s.Id2, true).ToList();
        sorted.Should().HaveCount(collection.Count);
        sorted.First().Id1.Should().Be(0);
        sorted.First().Id2.Should().Be(92);
        sorted.Last().Id1.Should().Be(1);
        sorted.Last().Id2.Should().Be(99);
    }

    [Fact]
    [Unit]
    public void SortBy_Ascending_ThenByDescending()
    {
        var collection = Enumerable.Range(0, 10).Select(s => new TestSort()
        {
            Id1 = s % 2,
            Id2 = 100 - s

        }).ToList();

        var sorted = collection.SortBy(s => s.Id1, true).SortBy(s => s.Id2, false).ToList();
        sorted.Should().HaveCount(collection.Count);
        sorted.First().Id1.Should().Be(0);
        sorted.First().Id2.Should().Be(100);
        sorted.Last().Id1.Should().Be(1);
        sorted.Last().Id2.Should().Be(91);
    }

    [Fact]
    [Unit]
    public void SortBy_Descending_ThenByAscending()
    {
        var collection = Enumerable.Range(0, 10).Select(s => new TestSort()
        {
            Id1 = s % 2,
            Id2 = 100 - s

        }).ToList();

        var sorted = collection.SortBy(s => s.Id1, false).SortBy(s => s.Id2, true).ToList();
        sorted.Should().HaveCount(collection.Count);
        sorted.First().Id1.Should().Be(1);
        sorted.First().Id2.Should().Be(91);
        sorted.Last().Id1.Should().Be(0);
        sorted.Last().Id2.Should().Be(100);
    }

    [Fact]
    [Unit]
    public void SortBy_Descending_ThenByDescending()
    {
        var collection = Enumerable.Range(0, 10).Select(s => new TestSort()
        {
            Id1 = s % 2,
            Id2 = 100 - s

        }).ToList();

        var sorted = collection.SortBy(s => s.Id1, false).SortBy(s => s.Id2, false).ToList();
        sorted.Should().HaveCount(collection.Count);
        sorted.First().Id1.Should().Be(1);
        sorted.First().Id2.Should().Be(99);
        sorted.Last().Id1.Should().Be(0);
        sorted.Last().Id2.Should().Be(92);
    }

    private class TestSort
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }
    }
}
