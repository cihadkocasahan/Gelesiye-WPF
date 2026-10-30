using System.IO;
using System.Text;

namespace GelesiyeWPF.Services;

/// <summary>
/// Uygulama genelinde dosya tabanlı hata ve bilgi loglama servisi.
/// </summary>
public static class LogService
{
    private static readonly object _lock = new();
    private static readonly string _logDir;

    static LogService()
    {
        try
        {
            _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(_logDir))
                Directory.CreateDirectory(_logDir);
        }
        catch
        {
            _logDir = AppDomain.CurrentDomain.BaseDirectory;
        }
    }

    public static string LogDirectory => _logDir;

    public static void Info(string message) => Write("INFO", message);
    public static void Warning(string message) => Write("WARN", message);
    public static void Error(string message, Exception? ex = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine(message);
        if (ex != null)
        {
            sb.AppendLine($"Exception Type: {ex.GetType().FullName}");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"Stack Trace:\n{ex.StackTrace}");

            var inner = ex.InnerException;
            int depth = 1;
            while (inner != null && depth <= 5)
            {
                sb.AppendLine($"--- Inner Exception (Level {depth}) ---");
                sb.AppendLine($"Type: {inner.GetType().FullName}");
                sb.AppendLine($"Message: {inner.Message}");
                sb.AppendLine($"Stack Trace:\n{inner.StackTrace}");
                inner = inner.InnerException;
                depth++;
            }
        }

        Write("ERROR", sb.ToString().TrimEnd());
    }

    private static void Write(string level, string text)
    {
        try
        {
            lock (_lock)
            {
                string fileName = $"gelesiye_{DateTime.Today:yyyyMMdd}.log";
                string fullPath = Path.Combine(_logDir, fileName);
                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {text}{Environment.NewLine}";
                File.AppendAllText(fullPath, line, Encoding.UTF8);

                // Eğer hata ise ayrıca gelesiye_error.log dosyasına da yaz
                if (level == "ERROR")
                {
                    string errPath = Path.Combine(_logDir, "gelesiye_error.log");
                    File.AppendAllText(errPath, line, Encoding.UTF8);
                }
            }
        }
        catch
        {
            // Loglama mekanizması asla uygulamanın kendisini çökertmemeli
        }
    }
}
