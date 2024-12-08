using Conster.Worker;

Config.Initialize();

Application application = new();

Console.WriteLine($"\nStarting...\n<3 {typeof(Program).Assembly.GetName().Name?.ToUpper()}\n");

// Show environment variables
Config.Debug();

// Initialize application needs
application.OnInitialize();

// Start application works
application.OnStart();

// Suppress Lifecycle
application.Freeze();

// Stop application works
application.OnStop();

// Destroy process
application.Destroy();
