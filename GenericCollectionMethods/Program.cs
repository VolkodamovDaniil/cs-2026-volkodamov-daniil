using System;
using System.Collections.Generic;

public static class CollectionUtils
{
    public static List<T> Distinct<T>(List<T> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var result = new List<T>();
        var seen = new HashSet<T>();

        foreach (var item in source)
        {
            if (!seen.Contains(item))
            {
                seen.Add(item);
                result.Add(item);
            }
        }

        return result;
    }

    public static Dictionary<TKey, List<TValue>> GroupBy<TValue, TKey>(
        List<TValue> source,
        Func<TValue, TKey> keySelector) where TKey : notnull
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (keySelector == null)
            throw new ArgumentNullException(nameof(keySelector));

        var result = new Dictionary<TKey, List<TValue>>();

        foreach (var item in source)
        {
            TKey key = keySelector(item);

            if (!result.ContainsKey(key))
                result[key] = new List<TValue>();

            result[key].Add(item);
        }

        return result;
    }

    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        Dictionary<TKey, TValue> first,
        Dictionary<TKey, TValue> second,
        Func<TValue, TValue, TValue> conflictResolver) where TKey : notnull
    {
        if (first == null)
            throw new ArgumentNullException(nameof(first));
        if (second == null)
            throw new ArgumentNullException(nameof(second));
        if (conflictResolver == null)
            throw new ArgumentNullException(nameof(conflictResolver));

        var result = new Dictionary<TKey, TValue>(first);

        foreach (var kvp in second)
        {
            if (result.ContainsKey(kvp.Key))
            {
                result[kvp.Key] = conflictResolver(result[kvp.Key], kvp.Value);
            }
            else
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    public static T MaxBy<T, TKey>(List<T> source, Func<T, TKey> selector)
        where TKey : IComparable<TKey>
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (selector == null)
            throw new ArgumentNullException(nameof(selector));
        if (source.Count == 0)
            throw new InvalidOperationException("Source collection is empty");

        T maxItem = source[0];
        TKey maxKey = selector(maxItem);

        for (int i = 1; i < source.Count; i++)
        {
            TKey currentKey = selector(source[i]);
            if (currentKey.CompareTo(maxKey) > 0)
            {
                maxKey = currentKey;
                maxItem = source[i];
            }
        }

        return maxItem;
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Testing Distinct ===");
        var numbers = new List<int> { 1, 2, 3, 2, 4, 1, 5, 3, 6 };
        var distinctNumbers = CollectionUtils.Distinct(numbers);
        Console.WriteLine($"Original: [{string.Join(", ", numbers)}]");
        Console.WriteLine($"Distinct: [{string.Join(", ", distinctNumbers)}]");

        var words = new List<string> { "apple", "banana", "apple", "cherry", "banana", "date" };
        var distinctWords = CollectionUtils.Distinct(words);
        Console.WriteLine($"\nOriginal words: [{string.Join(", ", words)}]");
        Console.WriteLine($"Distinct words: [{string.Join(", ", distinctWords)}]");

        Console.WriteLine("\n=== Testing GroupBy ===");
        var fruits = new List<string> { "apple", "banana", "kiwi", "grape", "pear", "plum" };
        var groupedByLength = CollectionUtils.GroupBy(fruits, s => s.Length);

        Console.WriteLine("Grouped by length:");
        foreach (var group in groupedByLength)
        {
            Console.WriteLine($"Length {group.Key}: [{string.Join(", ", group.Value)}]");
        }

        Console.WriteLine("\n=== Testing Merge ===");
        var dict1 = new Dictionary<string, int>
        {
            { "apple", 3 },
            { "banana", 2 },
            { "cherry", 5 }
        };

        var dict2 = new Dictionary<string, int>
        {
            { "banana", 4 },
            { "date", 1 },
            { "apple", 2 }
        };

        var merged = CollectionUtils.Merge(dict1, dict2, (v1, v2) => v1 + v2);

        Console.WriteLine("Dictionary 1:");
        foreach (var kvp in dict1)
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");

        Console.WriteLine("\nDictionary 2:");
        foreach (var kvp in dict2)
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");

        Console.WriteLine("\nMerged (with sum resolver):");
        foreach (var kvp in merged)
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");

        Console.WriteLine("\n=== Testing MaxBy ===");
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Price = 1200m },
            new Product { Id = 2, Name = "Mouse", Price = 25m },
            new Product { Id = 3, Name = "Keyboard", Price = 85m },
            new Product { Id = 4, Name = "Monitor", Price = 350m }
        };

        var mostExpensive = CollectionUtils.MaxBy(products, p => p.Price);
        Console.WriteLine($"Most expensive product: {mostExpensive}");

        Console.WriteLine("\n=== Testing MaxBy with empty collection ===");
        try
        {
            var emptyList = new List<int>();
            var max = CollectionUtils.MaxBy(emptyList, x => x);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Expected exception: {ex.Message}");
        }
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    public override string ToString()
    {
        return $"{Name} (${Price})";
    }
}