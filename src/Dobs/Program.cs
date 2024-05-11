using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CliFx;
using CliFx.Infrastructure;
using Dobs.Data;
using System.Reflection;

namespace Dobs;

/// <summary>
/// Entry point for the Dobs application.
/// </summary>
public static class Program
{
    private const string AppName = "dobs";
    private static readonly string AppVersion =
        Assembly.GetExecutingAssembly().GetName().Version!.ToString()
        + " Copyright 2024 Pedro R. Borges";

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <returns>
    /// An integer indicating the exit code of the application:
    /// 0 for success, non-zero for error.
    /// </returns>
    public static async Task<int> Main()
    {
        /// <summary>
        /// The filename used for storing application data.
        /// </summary>
        var dataFileName = "dobsdata.json";

        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppName
        );

        var appDataPath = Path.Combine(appDataFolder, dataFileName);

        _ = Directory.CreateDirectory(appDataFolder);

        using var console = new SystemConsole();
        return await BuildApp(console, appDataPath).RunAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Builds and configures a CliFx application instance.
    /// </summary>
    /// <param name="console">The CliFx.IConsole instance used for output.
    /// <param name="appDataPath">The path to the application data file.</param>
    /// <returns>
    /// A configured CliApplication instance ready to be run.
    /// </returns>
    public static CliApplication BuildApp(IConsole console, string appDataPath) =>
        new CliApplicationBuilder()
            .SetExecutableName(AppName)
            .SetDescription("USD - VES converter")
            .SetVersion(AppVersion)
            .UseConsole(console)
            .AddCommandsFromThisAssembly()
            .UseTypeActivator(commandTypes =>
            {
                var services = new ServiceCollection()
                    .AddSingleton<ILogger>(serviceProvider =>
                    {
                        var logger = new DobsILogger(AppName, console.Output);
                        return logger;
                    })
                    .AddTransient<IDataManager>(serviceProvider =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger>();
                        return new DataManager(logger, appDataPath);
                    })
                    .AddTransient<DecimalBindingConverter>()
                    .AddTransient<NonNegativeValidator>();

                foreach (var commandType in commandTypes)
                {
                    _ = services.AddTransient(commandType);
                }

                return services.BuildServiceProvider();
            })
            .AllowDebugMode(false)
            .AllowPreviewMode(false)
            .Build();
}
