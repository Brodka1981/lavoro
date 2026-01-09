using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering.Operators;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;

public static class ResourceQueryDefinitionExtensions
{
    public static TypedField<TField> AsField<TField>(this string fieldName) => new TypedField<TField>(fieldName);

    public static bool IsFilterFor<TField, TArgument>(
        this FilterDefinition filter,
        TypedField<TField> field,
        Func<BuiltInFilteringOperators<TField>, FilteringOperator<TField, TArgument>> @operator,
        out TArgument argument)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(@operator);

        argument = default!;

        if (!string.Equals(filter.FieldName, field.Name, StringComparison.OrdinalIgnoreCase))
            return false;

        var operatorSymbol = @operator.Invoke(BuiltInFilteringOperators<TField>.Instance).Symbol;
        if (!string.Equals(filter.OperatorSymbol, operatorSymbol, StringComparison.OrdinalIgnoreCase))
            return false;

        argument = filter.Argument.As<TArgument>();
        return true;
    }

    public static bool IsSortFor(this SortDefinition sort, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(sort);
        if (!string.Equals(sort.FieldName, fieldName, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
    public class TypedField<T>
    {
        public TypedField(string name)
        {
            Name = name;
            Type = typeof(T);
        }

        public string Name { get; }

        public Type Type { get; }
    }

}