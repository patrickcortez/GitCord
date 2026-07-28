using Gitbot2.Source;
using Gitbot2.Source.Core;
using Gitbot2.Source.Utils;
using Microsoft.Extensions.Logging;

namespace GitBot2.Source;

public static class Program{


    private static ILogger logger;
    private static bool log = false;
    private static readonly string version = "0.0.1";

    static Program()
    {
        logger = LoggerFactory.Create(c => c.AddConsole()).CreateLogger("Program");
    }

    public static async Task<int> Main(params string[] args)
    {
        try
        {

            if (args.Length > 0) // flag check
            {

                string cmd = args[0].ToLower().TrimStart('-');

                if (cmd == "log")
                {
                    log = true;
                } else if(cmd == "version")
                {
                    Console.WriteLine($"Gitbot {version} by Tezzz");
                    return 0;
                }

            }

            if (!Utility.isJsonAvailable())
            {
                logger.LogWarning("Configuration files not found");
                return 1;
            }

            // Instantiate Bot
            MessageToggle.Ignore = false;
            Bot bot = new(true);
            // Run our Bot
            int exc = await bot.RunAsync();

            if (log)
            {
                logger.LogInformation("Gitbot exited with: {}", exc);
            }

            return exc;

        }
        catch(Exception ex)
        {
            logger.LogError(ex,"An Error has occured");

            string fullex = ex.ToString(),
                   errP = Path.Combine(Environment.CurrentDirectory,"error.log")
                ;

            await File.AppendAllTextAsync(errP, $"[{DateTime.Now}]\n{fullex}\n{"-".PadRight(20)}\n[pwd:{Environment.CurrentDirectory}]");

            logger.LogInformation("Details written to {}",errP);

            return 1;
        }
    }

}
