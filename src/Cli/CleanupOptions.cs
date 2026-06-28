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
Usage:
  neff [--nef-dir <path>] [--jpg-dir <path>] [--quiet] [--recursive]

Description:
  By default, the tool scans only the current directory for .nef / .jpg / .jpeg files.
  It keeps NEF files whose relative path and file name match a JPG file, and deletes the rest.

Options:
  --nef-dir, -n     Path to the NEF directory, defaults to the current directory
  --jpg-dir, -j     Path to the JPG directory, defaults to the current directory
  --quiet, -q       Skip preview and confirmation, delete immediately
  --recursive, -r   Scan subdirectories recursively
  --help, -h        Show help
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
                    errorMessage = $"Unsupported argument: {arg}.";
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
            errorMessage = $"Option {optionName} is missing a directory value.";
            return false;
        }

        var nextValue = args[nextIndex];
        if (string.IsNullOrWhiteSpace(nextValue) || nextValue.StartsWith('-'))
        {
            errorMessage = $"Option {optionName} is missing a valid directory value.";
            return false;
        }

        value = nextValue;
        index = nextIndex;
        return true;
    }
}
