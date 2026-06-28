using NEF_Filter.Core;

namespace NEF_Filter.Cli;

internal sealed class App
{
    public async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(CleanupOptionsParser.UsageText);
            WaitForUser();
            return 0;
        }

        var parseResult = CleanupOptionsParser.TryParse(args, out var options, out var errorMessage);
        if (!parseResult)
        {
            Console.Error.WriteLine(errorMessage);
            Console.WriteLine(CleanupOptionsParser.UsageText);
            return 1;
        }

        if (options!.ShowHelp)
        {
            Console.WriteLine(CleanupOptionsParser.UsageText);
            return 0;
        }

        try
        {
            var service = new NefCleanupService();
            var result = await service.ExecuteAsync(options, CancellationToken.None);

            if (result.DeletedCount == 0)
            {
                Console.WriteLine($"No NEF files needed removal; kept {result.KeptCount} file(s).");
                return 0;
            }

            Console.WriteLine(
                $"Done. Deleted {result.DeletedCount} NEF file(s) and kept {result.KeptCount} file(s).");

            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Operation canceled.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Execution failed: {ex.Message}");
            return 1;
        }
    }

    private static void WaitForUser()
    {
        if (Console.IsInputRedirected)
        {
            return;
        }

        Console.WriteLine();
        Console.Write("Press Enter to exit...");
        Console.ReadLine();
    }
}
