using System.IO;
using GelesiyeWPF.Data;

namespace GelesiyeWPF.Services;

public static class BackupService
{
    public static string GetDefaultBackupDir()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(appDir, "backups");
    }

    public static string GetBackupDir()
    {
        var repo = DbFactory.GetAyarlarRepo();
        var dir  = repo.GetAyar("YedekKlasoru", "");
        return string.IsNullOrWhiteSpace(dir) ? GetDefaultBackupDir() : dir;
    }

    /// <summary>Veritabanını yedekler. Başarılıysa yedek dosya yolunu döner.</summary>
    public static string Backup()
    {
        var dbPath  = DbFactory.GetDb().DbPath;
        var dir     = GetBackupDir();
        Directory.CreateDirectory(dir);

        var ts       = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var destFile = Path.Combine(dir, $"gelesiye_{ts}.db");

        using (var dest = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={destFile};Pooling=False"))
        {
            dest.Open();
            DbFactory.GetDb().Connection.BackupDatabase(dest);
        }


        // Eski yedekleri temizle
        PurgeOldBackups(dir);

        var repo = DbFactory.GetAyarlarRepo();
        repo.SetAyar("SonYedekTarihi", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        return destFile;
    }

    public static void Restore(string backupFile)
    {
        var dbPath = DbFactory.GetDb().DbPath;
        // Bağlantıyı kapat, dosyayı kopyala, yeniden başlat
        DbFactory.GetDb().Dispose();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        File.Copy(backupFile, dbPath, overwrite: true);
        DbFactory.Initialize(dbPath);
    }

    public static List<FileInfo> GetBackupList()
    {
        var dir = GetBackupDir();
        if (!Directory.Exists(dir)) return [];

        return Directory.GetFiles(dir, "gelesiye_*.db")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .ToList();
    }

    /// <summary>Program kapatılırken otomatik yedek kontrol et.</summary>
    public static void CheckAutoBackup(bool onOpen)
    {
        try
        {
            var repo = DbFactory.GetAyarlarRepo();
            var mode = repo.GetInt("OtoYedek", 1);
            // 0=Yok, 1=Her kapanışta, 2=Günde 1, 3=Haftada 1, 4=Her açılışta
            if (mode == 0) return;
            if (onOpen  && mode != 4) return;
            if (!onOpen && mode == 4) return;

            if (mode == 2)
            {
                var sonStr = repo.GetAyar("SonYedekTarihi", "");
                if (DateTime.TryParse(sonStr, out var son) && son.Date == DateTime.Today) return;
            }
            if (mode == 3)
            {
                var sonStr = repo.GetAyar("SonYedekTarihi", "");
                if (DateTime.TryParse(sonStr, out var son) && (DateTime.Today - son.Date).TotalDays < 7) return;
            }

            Backup();
        }
        catch { /* Sessizce geç */ }
    }

    private static void PurgeOldBackups(string dir)
    {
        try
        {
            var repo   = DbFactory.GetAyarlarRepo();
            var maxRaw = repo.GetInt("MaksYedek", 10);
            if (maxRaw <= 0) return; // Sınırsız

            var files = Directory.GetFiles(dir, "gelesiye_*.db")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .ToList();

            foreach (var f in files.Skip(maxRaw))
                f.Delete();
        }
        catch { }
    }
}
