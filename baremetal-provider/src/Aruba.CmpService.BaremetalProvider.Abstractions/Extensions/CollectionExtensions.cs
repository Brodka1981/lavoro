using System.Collections.ObjectModel;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Pagination;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(collection);
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> collection, IDictionary<TKey, TValue>? items) where TKey : notnull
    {
        collection.ThrowIfNull();

        if (items != null)
        {
            foreach (var item in items)
            {
                collection.Add(item.Key, item.Value);
            }
        }
    }

    public static void Replace<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(collection);

        collection.Clear();
        collection.AddRange(items);
    }

    public static ICollection<T> ToCollection<T>(this IList<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new Collection<T>(items);
    }

    public static ICollection<T> ToCollection<T>(this IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return new Collection<T>(items.ToList());
    }

    public static IOrderedEnumerable<T> SortBy<T, TField>(this IEnumerable<T> source, Func<T, TField> fieldSelector, bool ascending)
    {
        bool isOrdered = source is IOrderedEnumerable<T>;
        if (isOrdered)
        {
            if (ascending)
            {
                return ((IOrderedEnumerable<T>)source).ThenBy(fieldSelector);
            }
            else
            {
                return ((IOrderedEnumerable<T>)source).ThenByDescending(fieldSelector);
            }

        }
        else
        {
            if (ascending)
            {
                return source.OrderBy(fieldSelector);
            }
            else
            {
                return source.OrderByDescending(fieldSelector);
            }
        }
    }



    public static IEnumerable<T> Page<T>(this IEnumerable<T> source, PagingDefinition? pagingDefinition, bool external)
    {
        var offset = Math.Max(pagingDefinition?.Offset ?? 0, 0);
        var limit = Math.Min(pagingDefinition?.Limit ?? ushort.MaxValue, ushort.MaxValue);
        if (external)
        {
            limit = Math.Min(limit, 100);
        }

        return source.Skip(offset).Take(limit);
    }
}
