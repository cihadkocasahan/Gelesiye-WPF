using Microsoft.Data.Sqlite;
using Dapper;
using GelesiyeWPF.Common;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.Data;

/// <summary>
/// SQLite bağlantısı ve şema yönetimi. Delphi TSQLiteConnection karşılığı.
/// </summary>
public sealed class Database : IDisposable
{
    static Database()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new SqliteDateTimeHandler());
    }

    private SqliteConnection _conn;
    private readonly string _dbPath;
    private static Database? _instance;

    public static Database Instance => _instance
        ?? throw new InvalidOperationException("Database.Initialize() henüz çağrılmadı.");

    public static void Initialize(string? dbPath = null)
    {
        _instance?.Dispose();
        _instance = new Database(dbPath);
    }

    private Database(string? dbPath)
    {
        if (string.IsNullOrWhiteSpace(dbPath))
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var dataDir = Path.Combine(appDir, "data");
            Directory.CreateDirectory(dataDir);
            _dbPath = Path.Combine(dataDir, AppConsts.DEF_DB_NAME);
        }
        else
        {
            _dbPath = dbPath;
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
        }

        _conn = new SqliteConnection($"Data Source={_dbPath};");
        _conn.Open();
        EnableWal();
        EnsureSchema();
    }

    public SqliteConnection Connection => _conn;
    public string DbPath => _dbPath;

    private void EnableWal()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
        cmd.ExecuteNonQuery();
    }

    private void EnsureSchema()
    {
        Execute(@"
CREATE TABLE IF NOT EXISTS FIRMALAR (
  ID INTEGER PRIMARY KEY AUTOINCREMENT,
  KOD TEXT NOT NULL,
  UNVAN TEXT NOT NULL,
  DONEM INTEGER DEFAULT 2026,
  PARA_BIRIMI TEXT DEFAULT 'TL',
  IS_DEFAULT INTEGER DEFAULT 0,
  BORC_LIMITI REAL DEFAULT 10000.0,
  TELEFON TEXT, FAKS TEXT, ADRES TEXT, IL TEXT, ILCE TEXT,
  VERGI_DAIRESI TEXT, VERGI_NO TEXT
);");

        Execute(@"
CREATE TABLE IF NOT EXISTS CARILER (
  ID INTEGER PRIMARY KEY AUTOINCREMENT,
  FIRMA_ID INTEGER NOT NULL,
  CARI_KODU TEXT NOT NULL,
  UNVAN TEXT NOT NULL,
  YETKILI TEXT, TELEFON TEXT, GSM TEXT, FAKS TEXT, ADRES TEXT,
  IL TEXT, ILCE TEXT, VERGI_DAIRESI TEXT, VERGI_NO TEXT,
  EPOSTA TEXT, WEB TEXT,
  CARI_LIMIT REAL DEFAULT 10000.0,
  HESAP_DURUMU INTEGER DEFAULT 1,
  NOTLAR TEXT, KAYIT_TARIHI TEXT,
  IS_DELETED INTEGER DEFAULT 0
);");

        Execute(@"
CREATE TABLE IF NOT EXISTS HAREKETLER (
  ID INTEGER PRIMARY KEY AUTOINCREMENT,
  FIRMA_ID INTEGER NOT NULL,
  CARI_ID INTEGER NOT NULL,
  TARIH TEXT NOT NULL,
  ISLEM_TURU INTEGER NOT NULL,
  ACIKLAMA TEXT,
  BORC REAL DEFAULT 0,
  ALACAK REAL DEFAULT 0,
  IS_DELETED INTEGER DEFAULT 0
);");

        Execute("CREATE INDEX IF NOT EXISTS IDX_CARILER_FIRMA ON CARILER (FIRMA_ID, IS_DELETED);");
        Execute("CREATE INDEX IF NOT EXISTS IDX_HAREKETLER_CARI ON HAREKETLER (CARI_ID, IS_DELETED);");
        Execute("CREATE INDEX IF NOT EXISTS IDX_HAREKETLER_FIRMA ON HAREKETLER (FIRMA_ID, IS_DELETED);");

        Execute(@"
CREATE TABLE IF NOT EXISTS AYARLAR (
  ANAHTAR TEXT PRIMARY KEY,
  DEGER TEXT
);");

        Execute(@"
CREATE TABLE IF NOT EXISTS ACIKLAMALAR (
  ID INTEGER PRIMARY KEY AUTOINCREMENT,
  ISLEM_TURU INTEGER NOT NULL,
  ACIKLAMA TEXT NOT NULL
);");

        Execute("CREATE INDEX IF NOT EXISTS IDX_ACIKLAMALAR_TUR ON ACIKLAMALAR (ISLEM_TURU);");

        Execute(@"
CREATE TABLE IF NOT EXISTS ILLER (
  ID INTEGER PRIMARY KEY,
  AD TEXT NOT NULL
);");

        Execute(@"
CREATE TABLE IF NOT EXISTS ILCELER (
  ID INTEGER PRIMARY KEY AUTOINCREMENT,
  IL_ID INTEGER NOT NULL,
  AD TEXT NOT NULL
);");

        Execute("CREATE INDEX IF NOT EXISTS IDX_ILCELER_IL ON ILCELER (IL_ID);");

        SeedDefaults();
    }

    private void SeedDefaults()
    {
        // Varsayılan firma
        if (ScalarLong("SELECT COUNT(*) FROM FIRMALAR") == 0)
        {
            Execute("INSERT INTO FIRMALAR (KOD,UNVAN,DONEM,PARA_BIRIMI,IS_DEFAULT,BORC_LIMITI) " +
                    "VALUES ('Frm1','TEST (Varsayılan)',2026,'TL',1,10000.0);");
        }

        // Açıklama şablonları
        if (ScalarLong("SELECT COUNT(*) FROM ACIKLAMALAR") == 0)
        {
            var seeds = new (int tur, string aciklama)[]
            {
                // Borç Dekontu (0)
                (0,"Mal Alımı"),(0,"Hizmet Bedeli"),(0,"Veresiye Satış"),(0,"Fatura Bedeli"),(0,"Kırtasiye Alımı"),
                // Alacak Dekontu (2)
                (2,"Mal İadesi"),(2,"İskonto Bedeli"),(2,"Düzeltme Fişi"),(2,"Fatura İadesi"),
                // Tahsilat (1)
                (1,"Nakit Tahsilat"),(1,"Kredi Kartı ile Tahsilat"),(1,"Banka Havalesi / EFT"),(1,"Çek ile Tahsilat"),(1,"Senet ile Tahsilat"),
                // Ödeme (3)
                (3,"Nakit Ödeme"),(3,"Banka Havalesi / EFT"),(3,"Kredi Kartı ile Ödeme"),(3,"Çek ile Ödeme"),(3,"Senet ile Ödeme"),
                // Devir (4)
                (4,"Devir Bakiyesi"),(4,"Devir Fişi"),
            };
            foreach (var (tur, aciklama) in seeds)
                Execute($"INSERT INTO ACIKLAMALAR (ISLEM_TURU,ACIKLAMA) VALUES ({tur},'{aciklama.Replace("'", "''")}');");
        }

        // Genel ayarlar
        if (ScalarLong("SELECT COUNT(*) FROM AYARLAR") == 0)
        {
            var defaults = new Dictionary<string, string>
            {
                ["SmtpHost"]      = "smtp.gmail.com",
                ["SmtpPort"]      = "587",
                ["SmtpSSL"]       = "1",
                ["SenderEmail"]   = "",
                ["SenderTitle"]   = "Örnek Ticaret",
                ["SmtpUser"]      = "",
                ["SmtpPassword"]  = "",
                ["ParaBirimi"]    = "TL",
                ["ZebraGrid"]     = "1",
                ["SifreKorumasi"] = "0",
                ["GirisSifresi"]  = "",
            };
            foreach (var kv in defaults)
                Execute($"INSERT INTO AYARLAR (ANAHTAR,DEGER) VALUES ('{kv.Key}','{kv.Value.Replace("'", "''")}');");
        }

        // 81 il + ilçe
        IlIlceRepository.SeedIfEmpty(this);
    }

    public void Execute(string sql, Dictionary<string, object?>? pars = null)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        if (pars != null)
            foreach (var kv in pars)
                cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public long ScalarLong(string sql, Dictionary<string, object?>? pars = null)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        if (pars != null)
            foreach (var kv in pars)
                cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
        var result = cmd.ExecuteScalar();
        return result == null || result == DBNull.Value ? 0L : Convert.ToInt64(result);
    }

    public long GetLastInsertRowId()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT last_insert_rowid();";
        return (long)(cmd.ExecuteScalar() ?? 0L);
    }

    public SqliteDataReader Query(string sql, Dictionary<string, object?>? pars = null)
    {
        var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        if (pars != null)
            foreach (var kv in pars)
                cmd.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
        return cmd.ExecuteReader(System.Data.CommandBehavior.Default);
    }

    public void Dispose()
    {
        _conn?.Close();
        _conn?.Dispose();
    }
}

public class SqliteDateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override void SetValue(System.Data.IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = value.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public override DateTime Parse(object value)
    {
        if (value is DateTime dt) return dt;
        if (value is string s && DateTime.TryParse(s, out var d)) return d;
        return DateTime.Today;
    }
}
