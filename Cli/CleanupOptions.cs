namespace NEF_Filter.Cli;

internal sealed record CleanupOptions(
    string TargetDirectory,
    bool Quiet,
    bool Recursive,
    bool ShowHelp);

internal static class CleanupOptionsParser
{
    public const string UsageText = """
用法:
  neff <目录路径> [--quiet] [--recursive]

说明:
  默认只扫描当前目录下的 .nef / .jpg / .jpeg 文件。
  保留与 JPG 同目录且同名的 NEF，删除没有对应 JPG 的 NEF。

参数:
  --quiet, -q    直接删除，不显示待删除清单，也不进行二次确认
  --recursive, -r  递归扫描所有子目录
  --help,  -h    显示帮助
""";

    public static bool TryParse(
        string[] args,
        out CleanupOptions? options,
        out string errorMessage)
    {
        options = null;
        errorMessage = string.Empty;

        if (args.Length == 0)
        {
            errorMessage = "缺少目录参数喵。";
            return false;
        }

        var quiet = false;
        var recursive = false;
        string? directory = null;

        foreach (var arg in args)
        {
            switch (arg)
            {
                case "--help":
                case "-h":
                    options = new CleanupOptions(string.Empty, false, false, true);
                    return true;
                case "--quiet":
                case "-q":
                    quiet = true;
                    break;
                case "--recursive":
                case "-r":
                    recursive = true;
                    break;
                default:
                    if (arg.StartsWith('-'))
                    {
                        errorMessage = $"不支持的参数: {arg}喵。";
                        return false;
                    }

                    if (directory is not null)
                    {
                        errorMessage = "只能提供一个目录参数喵。";
                        return false;
                    }

                    directory = arg;
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(directory))
        {
            errorMessage = "缺少目录参数喵。";
            return false;
        }

        var fullPath = Path.GetFullPath(directory);
        options = new CleanupOptions(fullPath, quiet, recursive, false);
        return true;
    }
}
