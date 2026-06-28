using NEF_Filter.Cli;

namespace NEF_Filter.Core;

internal sealed class NefCleanupService
{
    private static readonly HashSet<string> JpgExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg"
    };

    public Task<NefCleanupResult> ExecuteAsync(CleanupOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(options.TargetDirectory))
        {
            throw new DirectoryNotFoundException($"目录不存在: {options.TargetDirectory}");
        }

        var scanResult = Scan(options.TargetDirectory, cancellationToken);

        if (!options.Quiet)
        {
            PrintPreview(options.TargetDirectory, scanResult);
            ConfirmDeletion(scanResult);
        }

        var deletedCount = DeleteFiles(scanResult.FilesToDelete, cancellationToken);
        return Task.FromResult(new NefCleanupResult(deletedCount, scanResult.KeptCount));
    }

    private static NefScanResult Scan(string targetDirectory, CancellationToken cancellationToken)
    {
        var jpgNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var nefFiles = new List<string>();

        foreach (var filePath in Directory.EnumerateFiles(targetDirectory, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var extension = Path.GetExtension(filePath);
            if (extension.Equals(".nef", StringComparison.OrdinalIgnoreCase))
            {
                nefFiles.Add(filePath);
                continue;
            }

            if (!JpgExtensions.Contains(extension))
            {
                continue;
            }

            jpgNames.Add(BuildMatchKey(filePath));
        }

        var filesToDelete = new List<string>();
        var keptCount = 0;

        foreach (var nefFile in nefFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (jpgNames.Contains(BuildMatchKey(nefFile)))
            {
                keptCount++;
                continue;
            }

            filesToDelete.Add(nefFile);
        }

        return new NefScanResult(filesToDelete, keptCount, filesToDelete.Count);
    }

    private static int DeleteFiles(IEnumerable<string> filesToDelete, CancellationToken cancellationToken)
    {
        var deletedCount = 0;

        foreach (var filePath in filesToDelete)
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Delete(filePath);
            deletedCount++;
        }

        return deletedCount;
    }

    private static void PrintPreview(string targetDirectory, NefScanResult scanResult)
    {
        Console.WriteLine($"扫描目录: {targetDirectory}");
        Console.WriteLine($"待删除 NEF 文件数量: {scanResult.DeletedCount}");
        Console.WriteLine($"待保留 NEF 文件数量: {scanResult.KeptCount}");

        if (scanResult.DeletedCount == 0)
        {
            return;
        }

        Console.WriteLine("待删除文件明细:");
        foreach (var filePath in scanResult.FilesToDelete.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relativePath = Path.GetRelativePath(targetDirectory, filePath);
            Console.WriteLine($"  {relativePath}");
        }
    }

    private static void ConfirmDeletion(NefScanResult scanResult)
    {
        if (scanResult.DeletedCount == 0)
        {
            return;
        }

        Console.Write("确认删除以上 NEF 文件吗? [y/N]: ");
        var input = Console.ReadLine();

        if (!string.Equals(input, "y", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase))
        {
            throw new OperationCanceledException("用户取消删除");
        }
    }

    private static string BuildMatchKey(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        return Path.Combine(directory, fileNameWithoutExtension);
    }
}
