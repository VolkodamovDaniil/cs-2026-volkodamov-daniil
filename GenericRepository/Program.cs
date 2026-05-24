using System;
using System.Collections.Generic;
using System.Linq;

public interface IEntity
{
    int Id { get; }
}

public class Repository<T> where T : IEntity
{
    private readonly Dictionary<int, T> _items = new Dictionary<int, T>();

    public int Count => _items.Count;

    public void Add(T item)
    {
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Item with Id {item.Id} already exists");

        _items.Add(item.Id, item);
    }

    public bool Remove(int id)
    {
        return _items.Remove(id);
    }

    public T? GetById(int id)
    {
        _items.TryGetValue(id, out T? item);
        return item;
    }

    public IReadOnlyList<T> GetAll()
    {
        return _items.Values.ToList().AsReadOnly();
    }

    public IReadOnlyList<T> Find(Predicate<T> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return _items.Values.Where(item => predicate(item)).ToList().AsReadOnly();
    }
}

public class Product : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    public override string ToString()
    {
        return $"Product [Id: {Id}, Name: {Name}, Price: {Price:C}]";
    }
}

public class User : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public override string ToString()
    {
        return $"User [Id: {Id}, Name: {Name}, Email: {Email}]";
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Working with Products ===");
        var productRepo = new Repository<Product>();

        productRepo.Add(new Product { Id = 1, Name = "Laptop", Price = 1200m });
        productRepo.Add(new Product { Id = 2, Name = "Mouse", Price = 25m });
        productRepo.Add(new Product { Id = 3, Name = "Keyboard", Price = 85m });

        var product = productRepo.GetById(2);
        Console.WriteLine($"GetById(2): {product}");

        Console.WriteLine("\nAll products:");
        foreach (var p in productRepo.GetAll())
            Console.WriteLine(p);

        var expensiveProducts = productRepo.Find(p => p.Price > 100);
        Console.WriteLine("\nProducts with price > 100:");
        foreach (var p in expensiveProducts)
            Console.WriteLine(p);

        Console.WriteLine($"\nTotal products count: {productRepo.Count}");

        bool removed = productRepo.Remove(3);
        Console.WriteLine($"\nRemove product with Id=3: {removed}");
        Console.WriteLine($"Count after removal: {productRepo.Count}");
        
        Console.WriteLine("\nTrying to add duplicate:");
        try
        {
            productRepo.Add(new Product { Id = 1, Name = "Phone", Price = 800m });
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Exception caught: {ex.Message}");
        }

        Console.WriteLine("\n=== Working with Users ===");
        var userRepo = new Repository<User>();

        userRepo.Add(new User { Id = 1, Name = "Alice", Email = "alice@example.com" });
        userRepo.Add(new User { Id = 2, Name = "Bob", Email = "bob@example.com" });

        Console.WriteLine("All users:");
        foreach (var u in userRepo.GetAll())
            Console.WriteLine(u);

        var user = userRepo.GetById(1);
        Console.WriteLine($"\nGetById(1): {user}");
        Console.WriteLine($"Total users count: {userRepo.Count}");
    }
}