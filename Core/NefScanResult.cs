namespace NEF_Filter.Core;

internal sealed record NefScanResult(
    IReadOnlyList<string> FilesToDelete,
    int KeptCount,
    int DeletedCount);

internal sealed record NefCleanupResult(
    int DeletedCount,
    int KeptCount);
