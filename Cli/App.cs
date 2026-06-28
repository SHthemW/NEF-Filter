using NEF_Filter.Core;

namespace NEF_Filter.Cli;

internal sealed class App
{
    public async Task<int> RunAsync(string[] args)
    {
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
                Console.WriteLine($"未发现需要删除的 NEF 文件，共保留 {result.KeptCount} 个喵。");
                return 0;
            }

            Console.WriteLine(
                $"处理完成，已删除 {result.DeletedCount} 个 NEF 文件，保留 {result.KeptCount} 个 NEF 文件喵。");

            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("操作已取消喵。");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"执行失败: {ex.Message}喵。");
            return 1;
        }
    }
}
