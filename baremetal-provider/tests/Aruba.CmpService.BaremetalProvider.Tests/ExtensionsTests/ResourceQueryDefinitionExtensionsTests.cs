using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;
using FluentAssertions;
using static Aruba.CmpService.BaremetalProvider.Abstractions.Extensions.ResourceQueryDefinitionExtensions;

namespace Aruba.CmpService.BaremetalProvider.Tests.ExtensionsTests;
public class ResourceQueryDefinitionExtensionsTests :
    TestBase
{
    public ResourceQueryDefinitionExtensionsTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    [Unit]
    public void IsFilterFor_OK()
    {
        var filterDefinition = FilterDefinition.Create("Id1", "eq", 1);
        var ret = filterDefinition.IsFilterFor("id1".AsField<int>(), op => op.Equal, out var arg);
        ret.Should().BeTrue();
    }

    [Fact]
    [Unit]
    public void IsFilterFor_InvalidFieldName()
    {
        var filterDefinition = FilterDefinition.Create("Id2", "eq", 1);
        var ret = filterDefinition.IsFilterFor("id1".AsField<int>(), op => op.Equal, out var arg);
        ret.Should().BeFalse();
    }

    [Fact]
    [Unit]
    public void IsFilterFor_InvalidOperator()
    {
        var filterDefinition = FilterDefinition.Create("Id1", "eq1", 1);
        var ret = filterDefinition.IsFilterFor("id1".AsField<int>(), op => op.Equal, out var arg);
        ret.Should().BeFalse();
    }

    [Fact]
    [Unit]
    public void IsSortFor_Ok()
    {
        var sortDefinition = SortDefinition.Create("Id1");
        var ret = sortDefinition.IsSortFor("id1");
        ret.Should().BeTrue();
    }

    [Fact]
    [Unit]
    public void IsSortFor_InvalidFieldName()
    {
        var sortDefinition = SortDefinition.Create("Id1");
        var ret = sortDefinition.IsSortFor("id2");
        ret.Should().BeFalse();
    }

    [Fact]
    [Unit]
    public void TypedField_Ok()
    {
        var typedField = new TypedField<int>("Id1");
        typedField.Name.Should().Be("Id1");
        typedField.Type.Should().Be(typeof(Int32));
    }
    private class TestClass
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }
    }
}
