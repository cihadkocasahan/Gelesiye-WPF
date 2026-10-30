using Dapper;
using GelesiyeWPF.Common;

namespace GelesiyeWPF.Data;

public class AyarlarRepository
{
    private readonly Database _db;
    public AyarlarRepository(Database db) => _db = db;

    public string GetAyar(string anahtar, string varsayilan = "")
    {
        try
        {
            return _db.Connection.QueryFirstOrDefault<string>(
                "SELECT DEGER FROM AYARLAR WHERE ANAHTAR=@anahtar", new { anahtar }) ?? varsayilan;
        }
        catch { return varsayilan; }
    }

    public void SetAyar(string anahtar, string deger)
    {
        _db.Connection.Execute(@"INSERT INTO AYARLAR (ANAHTAR,DEGER) VALUES (@anahtar,@deger)
                      ON CONFLICT(ANAHTAR) DO UPDATE SET DEGER=excluded.DEGER",
            new { anahtar, deger });
    }

    public bool GetBool(string anahtar, bool varsayilan = false)
        => GetAyar(anahtar, varsayilan ? "1" : "0") == "1";

    public int GetInt(string anahtar, int varsayilan = 0)
        => int.TryParse(GetAyar(anahtar, varsayilan.ToString()), out var v) ? v : varsayilan;

    public List<string> GetAciklamalar(IslemTuru tur) =>
        _db.Connection.Query<string>(
            "SELECT ACIKLAMA FROM ACIKLAMALAR WHERE ISLEM_TURU=@tur ORDER BY ID",
            new { tur = (int)tur }).AsList();

    public List<string> GetTumAciklamalar() =>
        _db.Connection.Query<string>(
            "SELECT DISTINCT ACIKLAMA FROM ACIKLAMALAR ORDER BY ACIKLAMA COLLATE NOCASE").AsList();

    public void AddAciklama(IslemTuru tur, string aciklama)
    {
        var trimmed = aciklama.Trim();
        if (string.IsNullOrEmpty(trimmed)) return;
        var exists = _db.Connection.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM ACIKLAMALAR WHERE ISLEM_TURU=@tur AND ACIKLAMA=@trimmed",
            new { tur = (int)tur, trimmed });
        if (exists == 0)
            _db.Connection.Execute("INSERT INTO ACIKLAMALAR (ISLEM_TURU,ACIKLAMA) VALUES (@tur,@trimmed)",
                new { tur = (int)tur, trimmed });
    }

    public void DeleteAciklama(IslemTuru tur, string aciklama) =>
        _db.Connection.Execute("DELETE FROM ACIKLAMALAR WHERE ISLEM_TURU=@tur AND ACIKLAMA=@aciklama",
            new { tur = (int)tur, aciklama });

    public void ClearAciklamalar(IslemTuru tur) =>
        _db.Connection.Execute("DELETE FROM ACIKLAMALAR WHERE ISLEM_TURU=@tur",
            new { tur = (int)tur });
}
