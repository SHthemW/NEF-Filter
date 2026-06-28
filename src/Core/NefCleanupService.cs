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

        EnsureDirectoryExists(options.NefDirectory, "NEF");
        EnsureDirectoryExists(options.JpgDirectory, "JPG");

        var scanResult = Scan(options.NefDirectory, options.JpgDirectory, options.Recursive, cancellationToken);

        if (!options.Quiet)
        {
            PrintPreview(options, scanResult);
            ConfirmDeletion(scanResult);
        }

        var deletedCount = DeleteFiles(scanResult.FilesToDelete, cancellationToken);
        return Task.FromResult(new NefCleanupResult(deletedCount, scanResult.KeptCount));
    }

    private static NefScanResult Scan(
        string nefDirectory,
        string jpgDirectory,
        bool recursive,
        CancellationToken cancellationToken)
    {
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var jpgNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var nefFiles = new List<string>();

        foreach (var filePath in Directory.EnumerateFiles(jpgDirectory, "*", searchOption))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!JpgExtensions.Contains(Path.GetExtension(filePath)))
            {
                continue;
            }

            jpgNames.Add(BuildMatchKey(jpgDirectory, filePath));
        }

        foreach (var filePath in Directory.EnumerateFiles(nefDirectory, "*", searchOption))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (Path.GetExtension(filePath).Equals(".nef", StringComparison.OrdinalIgnoreCase))
            {
                nefFiles.Add(filePath);
            }
        }

        var filesToDelete = new List<string>();
        var keptCount = 0;

        foreach (var nefFile in nefFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (jpgNames.Contains(BuildMatchKey(nefDirectory, nefFile)))
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

    private static void PrintPreview(CleanupOptions options, NefScanResult scanResult)
    {
        Console.WriteLine($"NEF directory: {options.NefDirectory}");
        Console.WriteLine($"JPG directory: {options.JpgDirectory}");
        Console.WriteLine($"Recursive scan: {(options.Recursive ? "Yes" : "No")}");
        Console.WriteLine($"NEF files to delete: {scanResult.DeletedCount}");
        Console.WriteLine($"NEF files to keep: {scanResult.KeptCount}");

        if (scanResult.DeletedCount == 0)
        {
            return;
        }

        Console.WriteLine("Files to delete:");
        foreach (var filePath in scanResult.FilesToDelete.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            var relativePath = Path.GetRelativePath(options.NefDirectory, filePath);
            Console.WriteLine($"  {relativePath}");
        }
    }

    private static void ConfirmDeletion(NefScanResult scanResult)
    {
        if (scanResult.DeletedCount == 0)
        {
            return;
        }

        Console.Write("Delete the NEF files above? [y/N]: ");
        var input = Console.ReadLine();

        if (!string.Equals(input, "y", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase))
        {
            throw new OperationCanceledException("User canceled deletion.");
        }
    }

    private static string BuildMatchKey(string rootDirectory, string filePath)
    {
        var relativePath = Path.GetRelativePath(rootDirectory, filePath);
        var relativeDirectory = Path.GetDirectoryName(relativePath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(relativePath);
        return Path.Combine(relativeDirectory, fileNameWithoutExtension);
    }

    private static void EnsureDirectoryExists(string directoryPath, string label)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"{label} directory not found: {directoryPath}");
        }
    }
}
