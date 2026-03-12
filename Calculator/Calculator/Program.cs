while (true)
{
    Console.WriteLine("Enter two numbers (or 'q' to exit)");
    string input = Console.ReadLine();

    if (input == "q")
    {
        break;
    }

    string[] numbers = input.Split(' ');

    if (numbers.Length != 2 || !double.TryParse(numbers[0], out double num1) || !double.TryParse(numbers[1], out double num2))
    {
        Console.WriteLine("Invalid input");
        continue;
    }

    Console.WriteLine("Choose operation (+, -, *, /)");
    string operation = Console.ReadLine();

    switch (operation)
    {
        case "+":
            Console.WriteLine(num1 + num2);
            break;
        case "-":
            Console.WriteLine(num1 - num2);
            break;
        case "*":
            Console.WriteLine(num1 * num2);
            break;
        case "/":
            if (num2 == 0)
                Console.WriteLine("Error: Division by zero");
            else
                Console.WriteLine(num1 / num2);
            break;
        default:
            Console.WriteLine("Invalid operation");
            break;
    }

    Console.WriteLine();
}