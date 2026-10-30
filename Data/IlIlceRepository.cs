namespace GelesiyeWPF.Data;

/// <summary>Delphi IIlIlceRepository karşılığı. 81 il ve ilçe listesi.</summary>
public class IlIlceRepository
{
    private readonly Database _db;
    public IlIlceRepository(Database db) => _db = db;

    public List<string> GetIller()
    {
        var list = new List<string>();
        using var r = _db.Query("SELECT AD FROM ILLER ORDER BY AD COLLATE NOCASE");
        while (r.Read()) list.Add(r.GetString(0));
        // Türkçe sıralama
        var tr = new System.Globalization.CultureInfo("tr-TR");
        list.Sort((a, b) => string.Compare(a, b, tr, System.Globalization.CompareOptions.None));
        return list;
    }

    public List<string> GetIlceler(string ilAdi)
    {
        var list = new List<string>();
        using var r = _db.Query(
            "SELECT i.AD FROM ILCELER i JOIN ILLER l ON l.ID=i.IL_ID WHERE l.AD=@il ORDER BY i.AD",
            new() { ["@il"] = ilAdi });
        while (r.Read()) list.Add(r.GetString(0));
        var tr = new System.Globalization.CultureInfo("tr-TR");
        list.Sort((a, b) => string.Compare(a, b, tr, System.Globalization.CompareOptions.None));
        return list;
    }

    /// <summary>ILLER tablosu boşsa 81 il ve ilçeleri ekler.</summary>
    public static void SeedIfEmpty(Database db)
    {
        if (db.ScalarLong("SELECT COUNT(*) FROM ILLER") > 0) return;

        using var tx = db.Connection.BeginTransaction();
        try
        {
            for (int i = 0; i < IlIlceData.Items.Length; i++)
            {
                var item = IlIlceData.Items[i];
                int colon = item.IndexOf(':');
                if (colon <= 0) continue;

                var il = item[..colon];
                db.Execute("INSERT INTO ILLER (ID, AD) VALUES (@id,@ad)",
                    new() { ["@id"] = i + 1, ["@ad"] = il });

                foreach (var ilce in item[(colon + 1)..].Split(','))
                {
                    var ad = ilce.Trim();
                    if (ad.Length == 0) continue;
                    db.Execute("INSERT INTO ILCELER (IL_ID, AD) VALUES (@il,@ad)",
                        new() { ["@il"] = i + 1, ["@ad"] = ad });
                }
            }
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
