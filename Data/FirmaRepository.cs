using Dapper;
using GelesiyeWPF.Common;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.Data;

public class FirmaRepository
{
    private readonly Database _db;
    public FirmaRepository(Database db) => _db = db;

    public List<Firma> GetAll() =>
        _db.Connection.Query<Firma>("SELECT * FROM FIRMALAR ORDER BY ID").AsList();

    public Firma? GetDefault() =>
        _db.Connection.QueryFirstOrDefault<Firma>("SELECT * FROM FIRMALAR ORDER BY IS_DEFAULT DESC, ID ASC LIMIT 1");

    public Firma? GetById(int id) =>
        _db.Connection.QueryFirstOrDefault<Firma>("SELECT * FROM FIRMALAR WHERE ID=@id", new { id });

    public void SetDefault(int id)
    {
        _db.Connection.Execute("UPDATE FIRMALAR SET IS_DEFAULT=0; UPDATE FIRMALAR SET IS_DEFAULT=1 WHERE ID=@id;", new { id });
    }

    public int Insert(Firma f)
    {
        const string sql = @"INSERT INTO FIRMALAR 
            (KOD, UNVAN, DONEM, PARA_BIRIMI, IS_DEFAULT, BORC_LIMITI, TELEFON, FAKS, ADRES, IL, ILCE, VERGI_DAIRESI, VERGI_NO)
            VALUES (@Kod, @Unvan, @Donem, @ParaBirimi, @IsDefault, @BorcLimiti, @Telefon, @Faks, @Adres, @Il, @Ilce, @VergiDairesi, @VergiNo);
            SELECT last_insert_rowid();";

        f.Id = _db.Connection.ExecuteScalar<int>(sql, new
        {
            f.Kod, f.Unvan, f.Donem, f.ParaBirimi,
            IsDefault = f.IsDefault ? 1 : 0,
            f.BorcLimiti, f.Telefon, f.Faks, f.Adres, f.Il, f.Ilce, f.VergiDairesi, f.VergiNo
        });
        return f.Id;
    }

    public void Update(Firma f)
    {
        const string sql = @"UPDATE FIRMALAR SET
            KOD=@Kod, UNVAN=@Unvan, DONEM=@Donem, PARA_BIRIMI=@ParaBirimi, IS_DEFAULT=@IsDefault, BORC_LIMITI=@BorcLimiti,
            TELEFON=@Telefon, FAKS=@Faks, ADRES=@Adres, IL=@Il, ILCE=@Ilce,
            VERGI_DAIRESI=@VergiDairesi, VERGI_NO=@VergiNo WHERE ID=@Id";

        _db.Connection.Execute(sql, new
        {
            f.Id, f.Kod, f.Unvan, f.Donem, f.ParaBirimi,
            IsDefault = f.IsDefault ? 1 : 0,
            f.BorcLimiti, f.Telefon, f.Faks, f.Adres, f.Il, f.Ilce, f.VergiDairesi, f.VergiNo
        });
    }

    public void Delete(int id)
    {
        _db.Connection.Execute(@"
            DELETE FROM HAREKETLER WHERE FIRMA_ID=@id;
            DELETE FROM CARILER WHERE FIRMA_ID=@id;
            DELETE FROM FIRMALAR WHERE ID=@id;", new { id });
    }

    public bool DevirYap(int eskiFirmaId, int yeniFirmaId, bool aktarAciklamalar)
    {
        var nowIso = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        using var tx = _db.Connection.BeginTransaction();
        try
        {
            // Eski firmanın tüm carilerini al
            var cariler = _db.Connection.Query<Cari>(
                "SELECT * FROM CARILER WHERE FIRMA_ID=@fid AND IS_DELETED=0",
                new { fid = eskiFirmaId }, tx).AsList();

            foreach (var c in cariler)
            {
                // Bakiyeyi hesapla
                var bakiye = _db.Connection.ExecuteScalar<decimal>(
                    "SELECT COALESCE(SUM(BORC),0)-COALESCE(SUM(ALACAK),0) FROM HAREKETLER WHERE CARI_ID=@cid AND IS_DELETED=0",
                    new { cid = c.Id }, tx);

                // Yeni firmada cariyi oluştur
                const string sqlCari = @"INSERT INTO CARILER 
                    (FIRMA_ID, CARI_KODU, UNVAN, YETKILI, TELEFON, GSM, FAKS, ADRES, IL, ILCE,
                     VERGI_DAIRESI, VERGI_NO, EPOSTA, WEB, CARI_LIMIT, HESAP_DURUMU, NOTLAR, KAYIT_TARIHI)
                    VALUES (@FirmaId, @CariKodu, @Unvan, @Yetkili, @Telefon, @Gsm, @Faks, @Adres, @Il, @Ilce,
                            @VergiDairesi, @VergiNo, @Eposta, @Web, @CariLimit, @HesapDurumuInt, @Notlar, @KayitTarihi);
                    SELECT last_insert_rowid();";

                var newCariId = _db.Connection.ExecuteScalar<int>(sqlCari, new
                {
                    FirmaId = yeniFirmaId,
                    c.CariKodu, c.Unvan, c.Yetkili, c.Telefon, c.Gsm, c.Faks, c.Adres, c.Il, c.Ilce,
                    c.VergiDairesi, c.VergiNo, c.Eposta, c.Web, c.CariLimit,
                    HesapDurumuInt = (int)c.HesapDurumu,
                    c.Notlar,
                    KayitTarihi = nowIso
                }, tx);

                if (Math.Abs(bakiye) > 0.001m)
                {
                    decimal borc   = bakiye > 0 ? bakiye : 0m;
                    decimal alacak = bakiye < 0 ? -bakiye : 0m;

                    _db.Connection.Execute(@"INSERT INTO HAREKETLER 
                        (FIRMA_ID, CARI_ID, TARIH, ISLEM_TURU, ACIKLAMA, BORC, ALACAK)
                        VALUES (@fid, @cid, @tar, @tur, @acik, @b, @a)",
                        new
                        {
                            fid = yeniFirmaId, cid = newCariId,
                            tar = nowIso, tur = (int)IslemTuru.Devir,
                            acik = AppConsts.STR_TUR_DEVIR,
                            b = borc, a = alacak
                        }, tx);
                }
            }

            tx.Commit();
            return true;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
