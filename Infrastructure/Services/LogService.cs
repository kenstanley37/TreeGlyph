namespace Infrastructure.Services
{
    public static class LogService
    {
        private const long MaxTotalLogSizeBytes = 5 * 1024 * 1024; // 5 MB
        public static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TreeGlyph",
            "Logs");

        //private static readonly string DefaultLogFileName = $"exclusion-log-{DateTime.Now:yyyy-MM-dd}.txt";
        //private static readonly string DefaultLogFilePath = Path.Combine(LogDirectory, DefaultLogFileName);


        public static void Write(string category, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                    return;

                // Ensure log folder exists
                Directory.CreateDirectory(LogDirectory);

                // Always resolve today's log file dynamically
                string fileName = $"exclusion-log-{DateTime.Now:yyyy-MM-dd}.txt";
                string filePath = Path.Combine(LogDirectory, fileName);

                RotateIfNeeded();

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var formatted = $"[{timestamp}] [{category}] {message.Trim()}";

                File.AppendAllText(filePath, formatted + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LogService] Failed to write log: {ex.Message}");
            }
        }


        private static void RotateIfNeeded()
        {
            try
            {
                // Ensure the log directory exists before scanning
                if (!Directory.Exists(LogDirectory))
                    return;

                var logFiles = Directory.GetFiles(LogDirectory, "*.txt")
                    .Select(f => new FileInfo(f))
                    .OrderBy(f => f.CreationTimeUtc)
                    .ToList();

                long totalSize = logFiles.Sum(f => f.Length);

                while (totalSize > MaxTotalLogSizeBytes && logFiles.Count > 1)
                {
                    var oldest = logFiles.First();
                    try
                    {
                        oldest.Delete();
                        logFiles.RemoveAt(0);
                        totalSize = logFiles.Sum(f => f.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LogService] Failed to delete old log: {ex.Message}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LogService] Rotation failed: {ex.Message}");
            }
        }
    }
}