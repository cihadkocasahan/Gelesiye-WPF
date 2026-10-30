using Dapper;
using GelesiyeWPF.Common;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.Data;

public class CariRepository
{
    private readonly Database _db;
    public CariRepository(Database db) => _db = db;

    public List<Cari> GetAll(CariFiltre filtre)
    {
        var sql = @"
SELECT c.*,
  COALESCE(SUM(h.BORC),0)   AS TOPLAM_BORC,
  COALESCE(SUM(h.ALACAK),0) AS TOPLAM_ALACAK,
  COALESCE(SUM(h.BORC),0)-COALESCE(SUM(h.ALACAK),0) AS BAKIYE,
  COALESCE(MAX(h.TARIH), c.KAYIT_TARIHI) AS SON_ISLEM_TARIHI
FROM CARILER c
LEFT JOIN HAREKETLER h ON (c.ID=h.CARI_ID AND h.IS_DELETED=0)
WHERE c.FIRMA_ID=@FirmaId AND c.IS_DELETED=0 ";

        var pars = new DynamicParameters();
        pars.Add("FirmaId", filtre.FirmaId);

        if (filtre.HesapDurumu == HesapDurumu.Aktif)  sql += "AND c.HESAP_DURUMU=1 ";
        else if (filtre.HesapDurumu == HesapDurumu.Pasif) sql += "AND c.HESAP_DURUMU=2 ";

        if (!string.IsNullOrWhiteSpace(filtre.AramaMetni))
        {
            var term = filtre.AramaModu == AramaModu.Iceren
                       ? "%" + filtre.AramaMetni + "%"
                       : filtre.AramaMetni + "%";
            pars.Add("Search", term);
            pars.Add("Exact", filtre.AramaMetni.Trim());
            pars.Add("Start", filtre.AramaMetni.Trim() + "%");

            if (filtre.SorguTuru == SorguTuru.CariKod)
                sql += "AND c.CARI_KODU LIKE @Search ";
            else
                sql += "AND (c.UNVAN LIKE @Search OR c.NOTLAR LIKE @Search OR c.YETKILI LIKE @Search) ";
        }

        sql += "GROUP BY c.ID ";

        sql += filtre.BakiyeFiltresi switch
        {
            BakiyeFiltresi.Borclular    => "HAVING (COALESCE(SUM(h.BORC),0)-COALESCE(SUM(h.ALACAK),0))>0.001 ",
            BakiyeFiltresi.Alacaklilar  => "HAVING (COALESCE(SUM(h.BORC),0)-COALESCE(SUM(h.ALACAK),0))<-0.001 ",
            BakiyeFiltresi.BakiyesiArti => "HAVING ABS(COALESCE(SUM(h.BORC),0)-COALESCE(SUM(h.ALACAK),0))>0.001 ",
            BakiyeFiltresi.SifirBakiye  => "HAVING ABS(COALESCE(SUM(h.BORC),0)-COALESCE(SUM(h.ALACAK),0))<=0.001 ",
            BakiyeFiltresi.LimitAsanlar => "HAVING (COALESCE(SUM(h.BORC),0)-COALESCE(SUM(h.ALACAK),0))>c.CARI_LIMIT ",
            _                           => ""
        };

        if (!string.IsNullOrWhiteSpace(filtre.AramaMetni))
        {
            sql += @"ORDER BY
  CASE WHEN LOWER(c.UNVAN)=LOWER(@Exact) THEN 1
       WHEN LOWER(c.CARI_KODU)=LOWER(@Exact) THEN 2
       WHEN LOWER(c.UNVAN) LIKE LOWER(@Start) THEN 3
       WHEN LOWER(c.CARI_KODU) LIKE LOWER(@Start) THEN 4
       ELSE 5 END ASC, c.UNVAN COLLATE NOCASE ASC;";
        }
        else
        {
            sql += "ORDER BY c.UNVAN COLLATE NOCASE ASC;";
        }

        return _db.Connection.Query<Cari>(sql, pars).AsList();
    }

    public Cari? GetById(int id)
    {
        const string sql = @"
SELECT c.*,
  COALESCE(SUM(h.BORC),0)   AS TOPLAM_BORC,
  COALESCE(SUM(h.ALACAK),0) AS TOPLAM_ALACAK,
  COALESCE(SUM(h.BORC),0)-COALESCE(SUM(h.ALACAK),0) AS BAKIYE,
  COALESCE(MAX(h.TARIH), c.KAYIT_TARIHI) AS SON_ISLEM_TARIHI
FROM CARILER c
LEFT JOIN HAREKETLER h ON (c.ID=h.CARI_ID AND h.IS_DELETED=0)
WHERE c.ID=@id AND c.IS_DELETED=0 GROUP BY c.ID";

        return _db.Connection.QueryFirstOrDefault<Cari>(sql, new { id });
    }

    public int Insert(Cari c)
    {
        const string sql = @"INSERT INTO CARILER
            (FIRMA_ID, CARI_KODU, UNVAN, YETKILI, TELEFON, GSM, FAKS, ADRES, IL, ILCE,
             VERGI_DAIRESI, VERGI_NO, EPOSTA, WEB, CARI_LIMIT, HESAP_DURUMU, NOTLAR, KAYIT_TARIHI)
            VALUES (@FirmaId, @CariKodu, @Unvan, @Yetkili, @Telefon, @Gsm, @Faks, @Adres, @Il, @Ilce,
                    @VergiDairesi, @VergiNo, @Eposta, @Web, @CariLimit, @HesapDurumuInt, @Notlar, @KayitTarihi);
            SELECT last_insert_rowid();";

        c.Id = _db.Connection.ExecuteScalar<int>(sql, new
        {
            c.FirmaId, c.CariKodu, c.Unvan, c.Yetkili, c.Telefon, c.Gsm, c.Faks, c.Adres, c.Il, c.Ilce,
            c.VergiDairesi, c.VergiNo, c.Eposta, c.Web, c.CariLimit,
            HesapDurumuInt = (int)c.HesapDurumu,
            c.Notlar,
            c.KayitTarihi
        });
        return c.Id;
    }

    public void Update(Cari c)
    {
        const string sql = @"UPDATE CARILER SET
            CARI_KODU=@CariKodu, UNVAN=@Unvan, YETKILI=@Yetkili, TELEFON=@Telefon, GSM=@Gsm, FAKS=@Faks,
            ADRES=@Adres, IL=@Il, ILCE=@Ilce, VERGI_DAIRESI=@VergiDairesi, VERGI_NO=@VergiNo,
            EPOSTA=@Eposta, WEB=@Web, CARI_LIMIT=@CariLimit, HESAP_DURUMU=@HesapDurumuInt, NOTLAR=@Notlar
            WHERE ID=@Id";

        _db.Connection.Execute(sql, new
        {
            c.Id, c.CariKodu, c.Unvan, c.Yetkili, c.Telefon, c.Gsm, c.Faks, c.Adres, c.Il, c.Ilce,
            c.VergiDairesi, c.VergiNo, c.Eposta, c.Web, c.CariLimit,
            HesapDurumuInt = (int)c.HesapDurumu,
            c.Notlar
        });
    }

    public void Delete(int id)
    {
        _db.Connection.Execute("UPDATE CARILER SET IS_DELETED=1 WHERE ID=@id", new { id });
        _db.Connection.Execute("UPDATE HAREKETLER SET IS_DELETED=1 WHERE CARI_ID=@id", new { id });
    }

    public string GetNextCariKodu(int firmaId)
    {
        var max = _db.Connection.ExecuteScalar<long>(
            "SELECT COALESCE(MAX(CAST(REPLACE(CARI_KODU,'C','') AS INTEGER)),0) FROM CARILER WHERE FIRMA_ID=@firmaId AND IS_DELETED=0",
            new { firmaId });
        return $"C{max + 1:D3}";
    }

    public bool ExistsKodu(string cariKodu, int firmaId, int haricId = 0)
    {
        const string sql = @"SELECT COUNT(*) FROM CARILER 
            WHERE CARI_KODU=@cariKodu AND FIRMA_ID=@firmaId AND ID!=@haricId AND IS_DELETED=0";
        return _db.Connection.ExecuteScalar<long>(sql, new { cariKodu, firmaId, haricId }) > 0;
    }

    public BakiyeOzet GetBakiyeOzet(int firmaId)
    {
        const string sql = @"
SELECT
  c.HESAP_DURUMU AS Durum,
  COALESCE(SUM(h.BORC),0)-COALESCE(SUM(h.ALACAK),0) AS Bakiye
FROM CARILER c
LEFT JOIN HAREKETLER h ON (c.ID=h.CARI_ID AND h.IS_DELETED=0)
WHERE c.FIRMA_ID=@firmaId AND c.IS_DELETED=0
GROUP BY c.ID, c.HESAP_DURUMU";

        var rows = _db.Connection.Query<(int Durum, decimal Bakiye)>(sql, new { firmaId });

        decimal aktifBorc = 0, aktifAlacak = 0, pasifBorc = 0, pasifAlacak = 0;
        int total = 0, aktif = 0, pasif = 0, borclu = 0, alacakli = 0, sifir = 0;

        foreach (var r in rows)
        {
            total++;
            if (r.Durum == 1) { aktif++; if (r.Bakiye > 0.001m) aktifBorc += r.Bakiye; else if (r.Bakiye < -0.001m) aktifAlacak -= r.Bakiye; }
            else             { pasif++; if (r.Bakiye > 0.001m) pasifBorc += r.Bakiye; else if (r.Bakiye < -0.001m) pasifAlacak -= r.Bakiye; }

            if (r.Bakiye > 0.001m)       borclu++;
            else if (r.Bakiye < -0.001m) alacakli++;
            else                         sifir++;
        }

        return new BakiyeOzet
        {
            AktifBorcluToplam    = aktifBorc,
            AktifAlacakliToplam  = aktifAlacak,
            PasifBorcluToplam    = pasifBorc,
            PasifAlacakliToplam  = pasifAlacak,
            GenelBorcluToplam    = aktifBorc + pasifBorc,
            GenelAlacakliToplam  = aktifAlacak + pasifAlacak,
            NetBakiye            = aktifBorc + pasifBorc - aktifAlacak - pasifAlacak,
            ToplamCariSayisi     = total,
            AktifCariSayisi      = aktif,
            PasifCariSayisi      = pasif,
            BorcluCariSayisi     = borclu,
            AlacakliCariSayisi   = alacakli,
            SifirBakiyeCariSayisi= sifir
        };
    }
}
