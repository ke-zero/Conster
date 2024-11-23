namespace Conster.Worker;

public static class Application
{
    public static void Initialize()
    {
    }

    public static void Start()
    {
    }

    public static void Stop()
    {
    }

    public static void Freeze()
    {
        while (true)
        {
            var key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Q) continue;

            var quit = false;
            var reading = false;

            var time = DateTime.Now;
            Console.WriteLine($"\nWanna you quit? `{time}`\n\t Press: `y` (Yes) or `n` (No)");
            
            do
            {
                var next = Console.ReadKey(true);

                switch (next.Key)
                {
                    case ConsoleKey.Y:
                    {
                        reading = false;
                        quit = true;
                        Console.WriteLine("`y` Selected. Program will be destroyed...");
                        break;
                    }
                    case ConsoleKey.N:
                    {
                        reading = false;
                        quit = false;
                        Console.WriteLine("`n` Selected. Program will continue...");
                        break;
                    }
                    default:
                    {
                        reading = true;
                        break;
                    }
                }
            } while (reading);

            if (quit) break;
        }
    }
}