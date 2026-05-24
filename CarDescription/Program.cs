using System;
using System.Collections.Generic;

public interface ICar
{
    string GetDescription();
    string GetBrand();
}

public interface IElectric
{
    int GetBatteryCapacity();
    int GetChargingTime();
}

public interface IMechanical
{
    string GetFuelType();
    double GetFuelConsumption();
}

public interface IAutomatical
{
    string GetTransmissionType();
    int GetGearsCount();
}

public interface IManual
{
    string GetTransmissionType();
    int GetGearsCount();
    bool HasClutchPedal();
}

public abstract class ACar : ICar
{
    protected string brand;
    protected int seatsCount;
    protected string infotainmentSystem;

    public ACar(string brand, int seatsCount, string infotainmentSystem)
    {
        this.brand = brand;
        this.seatsCount = seatsCount;
        this.infotainmentSystem = infotainmentSystem;
    }

    public virtual string GetDescription()
    {
        return $"{brand}: {GetCarType()}, {GetTransmissionDescription()}, {seatsCount} seats, {infotainmentSystem} on board";
    }

    public string GetBrand()
    {
        return brand;
    }

    protected abstract string GetCarType();
    protected abstract string GetTransmissionDescription();
}

public class Tesla : ACar, IElectric, IAutomatical
{
    private int batteryCapacity;
    private int chargingTime;

    public Tesla() : base("Tesla", 5, "Android")
    {
        batteryCapacity = 75;
        chargingTime = 8;
    }

    protected override string GetCarType() => "electric car";
    protected override string GetTransmissionDescription() => "automatic transmission";

    public int GetBatteryCapacity() => batteryCapacity;
    public int GetChargingTime() => chargingTime;
    public string GetTransmissionType() => "Automatic";
    public int GetGearsCount() => 1;
}

public class BMW : ACar, IMechanical, IManual
{
    private string fuelType;
    private double fuelConsumption;

    public BMW() : base("BMW", 4, "iDrive")
    {
        fuelType = "Petrol";
        fuelConsumption = 8.5;
    }

    protected override string GetCarType() => "regular car";
    protected override string GetTransmissionDescription() => "manual transmission";

    public string GetFuelType() => fuelType;
    public double GetFuelConsumption() => fuelConsumption;
    public string GetTransmissionType() => "Manual";
    public int GetGearsCount() => 6;
    public bool HasClutchPedal() => true;
}

public class Toyota : ACar, IMechanical, IAutomatical
{
    private string fuelType;
    private double fuelConsumption;

    public Toyota() : base("Toyota", 5, "Toyota Entune")
    {
        fuelType = "Petrol";
        fuelConsumption = 7.2;
    }

    protected override string GetCarType() => "regular car";
    protected override string GetTransmissionDescription() => "automatic transmission";

    public string GetFuelType() => fuelType;
    public double GetFuelConsumption() => fuelConsumption;
    public string GetTransmissionType() => "CVT";
    public int GetGearsCount() => 7;
}

public class PorscheTaycan : ACar, IElectric, IManual
{
    private int batteryCapacity;
    private int chargingTime;

    public PorscheTaycan() : base("Porsche Taycan", 4, "PCM")
    {
        batteryCapacity = 93;
        chargingTime = 9;
    }

    protected override string GetCarType() => "electric car";
    protected override string GetTransmissionDescription() => "manual transmission (2-speed)";

    public string GetFuelType() => "Electric";
    public double GetFuelConsumption() => 0;
    public int GetBatteryCapacity() => batteryCapacity;
    public int GetChargingTime() => chargingTime;
    public string GetTransmissionType() => "Manual";
    public int GetGearsCount() => 2;
    public bool HasClutchPedal() => true;
}

public class Mercedes : ACar, IMechanical, IAutomatical
{
    private string fuelType;
    private double fuelConsumption;

    public Mercedes() : base("Mercedes", 5, "MBUX")
    {
        fuelType = "Diesel";
        fuelConsumption = 6.5;
    }

    protected override string GetCarType() => "regular car";
    protected override string GetTransmissionDescription() => "automatic transmission 9G-TRONIC";

    public string GetFuelType() => fuelType;
    public double GetFuelConsumption() => fuelConsumption;
    public string GetTransmissionType() => "Automatic";
    public int GetGearsCount() => 9;
}

public class Audi : ACar, IMechanical, IAutomatical
{
    private string fuelType;
    private double fuelConsumption;

    public Audi() : base("Audi", 5, "MMI")
    {
        fuelType = "Petrol";
        fuelConsumption = 7.8;
    }

    protected override string GetCarType() => "regular car";
    protected override string GetTransmissionDescription() => "automatic transmission (S tronic)";

    public string GetFuelType() => fuelType;
    public double GetFuelConsumption() => fuelConsumption;
    public string GetTransmissionType() => "Automatic";
    public int GetGearsCount() => 7;
}

public class Ford : ACar, IMechanical, IManual
{
    private string fuelType;
    private double fuelConsumption;

    public Ford() : base("Ford", 5, "SYNC")
    {
        fuelType = "Petrol";
        fuelConsumption = 9.2;
    }

    protected override string GetCarType() => "regular car";
    protected override string GetTransmissionDescription() => "manual transmission";

    public string GetFuelType() => fuelType;
    public double GetFuelConsumption() => fuelConsumption;
    public string GetTransmissionType() => "Manual";
    public int GetGearsCount() => 6;
    public bool HasClutchPedal() => true;
}

public enum CarType
{
    Tesla,
    BMW,
    Toyota,
    PorscheTaycan,
    Mercedes,
    Audi,
    Ford
}

public static class CarFactory
{
    private static Dictionary<CarType, Func<ICar>> carCreators = new Dictionary<CarType, Func<ICar>>
    {
        { CarType.Tesla, () => new Tesla() },
        { CarType.BMW, () => new BMW() },
        { CarType.Toyota, () => new Toyota() },
        { CarType.PorscheTaycan, () => new PorscheTaycan() },
        { CarType.Mercedes, () => new Mercedes() },
        { CarType.Audi, () => new Audi() },
        { CarType.Ford, () => new Ford() }
    };

    public static ICar CreateCar(CarType type)
    {
        if (carCreators.ContainsKey(type))
        {
            return carCreators[type]();
        }
        throw new ArgumentException($"Unknown car type: {type}");
    }

    public static CarType? ParseCarType(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim().ToLower();

        foreach (CarType type in Enum.GetValues(typeof(CarType)))
        {
            if (type.ToString().ToLower() == input)
            {
                return type;
            }
        }

        if (input == "porsche" || input == "porsche taycan" || input == "porschetaycan")
        {
            return CarType.PorscheTaycan;
        }

        if (input == "mercedes-benz")
        {
            return CarType.Mercedes;
        }

        return null;
    }

    public static List<string> GetAllCarBrands()
    {
        List<string> brands = new List<string>();
        foreach (CarType type in Enum.GetValues(typeof(CarType)))
        {
            brands.Add(type.ToString());
        }
        return brands;
    }

    public static List<ICar> GetAllCars()
    {
        List<ICar> cars = new List<ICar>();
        foreach (CarType type in Enum.GetValues(typeof(CarType)))
        {
            cars.Add(CreateCar(type));
        }
        return cars;
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        ShowWelcomeMessage();

        while (true)
        {
            Console.Write("\nEnter car brand, 'list' to see all cars, or 'done' to stop: ");
            string input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Please enter a command.");
                continue;
            }

            if (input.ToLower() == "done")
            {
                Console.WriteLine("Program terminated.");
                break;
            }

            if (input.ToLower() == "list" || input.ToLower() == "all" || input.ToLower() == "show all")
            {
                ShowAllCars();
                continue;
            }

            if (input.ToLower() == "help")
            {
                ShowHelp();
                continue;
            }

            CarType? carType = CarFactory.ParseCarType(input);

            if (carType.HasValue)
            {
                try
                {
                    ICar car = CarFactory.CreateCar(carType.Value);
                    Console.WriteLine($"\"{car.GetDescription()}\"");
                    PrintDetailedInfo(car);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Brand '{input}' not found. Type 'list' to see all available brands or 'help' for commands.");
            }
        }
    }

    static void ShowWelcomeMessage()
    {
        Console.WriteLine("=== CAR INFORMATION SYSTEM ===");
        Console.WriteLine("Commands: 'list' - show all cars, 'help' - show commands, 'done' - exit");
        Console.WriteLine("================================");
    }

    static void ShowHelp()
    {
        Console.WriteLine("\n=== AVAILABLE COMMANDS ===");
        Console.WriteLine("• Type a car brand (Tesla, BMW, Toyota, Porsche, Mercedes, Audi, Ford) - get car description");
        Console.WriteLine("• 'list' or 'all' - show all available cars");
        Console.WriteLine("• 'help' - show this help message");
        Console.WriteLine("• 'done' - exit the program");
        Console.WriteLine("==========================");
    }

    static void ShowAllCars()
    {
        Console.WriteLine("\n=== ALL AVAILABLE CARS ===");
        List<ICar> allCars = CarFactory.GetAllCars();

        for (int i = 0; i < allCars.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {allCars[i].GetDescription()}");
        }

        Console.WriteLine($"\nTotal: {allCars.Count} car models available");
        Console.WriteLine("==============================");

        Console.Write("\nWould you like to see detailed info for any car? (Enter brand name or 'no'): ");
        string response = Console.ReadLine()?.Trim();

        if (!string.IsNullOrWhiteSpace(response) && response.ToLower() != "no")
        {
            CarType? carType = CarFactory.ParseCarType(response);
            if (carType.HasValue)
            {
                ICar car = CarFactory.CreateCar(carType.Value);
                Console.WriteLine($"\n\"{car.GetDescription()}\"");
                PrintDetailedInfo(car);
            }
            else if (response.ToLower() != "no")
            {
                Console.WriteLine($"Brand '{response}' not found.");
            }
        }
    }

    static void PrintDetailedInfo(ICar car)
    {
        Console.WriteLine("  Detailed information:");

        if (car is IElectric electric)
        {
            Console.WriteLine($"    - Type: Electric vehicle");
            Console.WriteLine($"    - Battery capacity: {electric.GetBatteryCapacity()} kWh");
            Console.WriteLine($"    - Charging time: {electric.GetChargingTime()} hours");
        }

        if (car is IMechanical mechanical)
        {
            Console.WriteLine($"    - Type: Regular vehicle");
            Console.WriteLine($"    - Fuel: {mechanical.GetFuelType()}");
            Console.WriteLine($"    - Consumption: {mechanical.GetFuelConsumption()} L/100km");
        }

        if (car is IAutomatical auto)
        {
            Console.WriteLine($"    - Transmission: {auto.GetTransmissionType()}, {auto.GetGearsCount()} gears");
        }

        if (car is IManual manual)
        {
            Console.WriteLine($"    - Transmission: {manual.GetTransmissionType()}, {manual.GetGearsCount()} gears");
            Console.WriteLine($"    - Clutch pedal: {(manual.HasClutchPedal() ? "Yes" : "No")}");
        }
    }
}