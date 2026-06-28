namespace NEF_Filter.Cli;

internal sealed record CleanupOptions(
    string NefDirectory,
    string JpgDirectory,
    bool Quiet,
    bool Recursive,
    bool ShowHelp);

internal static class CleanupOptionsParser
{
    public const string UsageText = """
用法:
  neff [--nef-dir <目录路径>] [--jpg-dir <目录路径>] [--quiet] [--recursive]

说明:
  默认扫描当前目录下的 .nef / .jpg / .jpeg 文件。
  保留与 JPG 相对路径及文件名一致的 NEF，删除没有对应 JPG 的 NEF。

参数:
  --nef-dir, -n     指定 NEF 目录，默认当前目录
  --jpg-dir, -j     指定 JPG 目录，默认当前目录
  --quiet, -q       直接删除，不显示待删除清单，也不进行二次确认
  --recursive, -r   递归扫描所有子目录
  --help, -h        显示帮助
""";

    public static bool TryParse(
        string[] args,
        out CleanupOptions? options,
        out string errorMessage)
    {
        options = null;
        errorMessage = string.Empty;

        var quiet = false;
        var recursive = false;
        string? nefDirectory = null;
        string? jpgDirectory = null;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];

            switch (arg)
            {
                case "--help":
                case "-h":
                    options = new CleanupOptions(string.Empty, string.Empty, false, false, true);
                    return true;
                case "--quiet":
                case "-q":
                    quiet = true;
                    break;
                case "--recursive":
                case "-r":
                    recursive = true;
                    break;
                case "--nef-dir":
                case "-n":
                    if (!TryReadOptionValue(args, ref index, arg, out nefDirectory, out errorMessage))
                    {
                        return false;
                    }

                    break;
                case "--jpg-dir":
                case "-j":
                    if (!TryReadOptionValue(args, ref index, arg, out jpgDirectory, out errorMessage))
                    {
                        return false;
                    }

                    break;
                default:
                    errorMessage = $"不支持的参数: {arg}喵。";
                    return false;
            }
        }

        options = new CleanupOptions(
            Path.GetFullPath(nefDirectory ?? Environment.CurrentDirectory),
            Path.GetFullPath(jpgDirectory ?? Environment.CurrentDirectory),
            quiet,
            recursive,
            false);

        return true;
    }

    private static bool TryReadOptionValue(
        IReadOnlyList<string> args,
        ref int index,
        string optionName,
        out string? value,
        out string errorMessage)
    {
        value = null;
        errorMessage = string.Empty;

        var nextIndex = index + 1;
        if (nextIndex >= args.Count)
        {
            errorMessage = $"参数 {optionName} 缺少目录值喵。";
            return false;
        }

        var nextValue = args[nextIndex];
        if (string.IsNullOrWhiteSpace(nextValue) || nextValue.StartsWith('-'))
        {
            errorMessage = $"参数 {optionName} 缺少有效目录值喵。";
            return false;
        }

        value = nextValue;
        index = nextIndex;
        return true;
    }
}
