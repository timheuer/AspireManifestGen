using Microsoft.Extensions.Logging;

public static class OutputWindowManager
{
    public static OutputWindowPane AspireOutputPane { get; set; }

    public static string GenerateOutputMessage(string message, string source, LogLevel logLevel)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        return $"[{logLevel.ToString().ToUpperInvariant()}] [{source.ToUpperInvariant()}] [{timestamp}]: {message}";
    }
}
