using Conster.Worker;

Console.WriteLine($"\nStarting...\n<3 {typeof(Program).Assembly.GetName().Name?.ToUpper()}\n");
Console.WriteLine("q: To Quit.");
Config.Initialize();
Config.Debug();

Application.Initialize();
Application.Start();
Application.Freeze();
Application.Stop();