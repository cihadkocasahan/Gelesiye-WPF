using Dapper;
using GelesiyeWPF.Common;
using GelesiyeWPF.Domain;

namespace GelesiyeWPF.Data;

public class HareketRepository
{
    private readonly Database _db;
    public HareketRepository(Database db) => _db = db;

    public List<Hareket> GetByCariId(int cariId)
    {
        const string sql = @"
SELECT h.*, c.UNVAN AS CariUnvan
FROM HAREKETLER h
LEFT JOIN CARILER c ON c.ID=h.CARI_ID
WHERE h.CARI_ID=@cariId AND h.IS_DELETED=0
ORDER BY h.TARIH ASC, h.ID ASC";

        var list = _db.Connection.Query<Hareket>(sql, new { cariId }).AsList();
        decimal yuruyen = 0;
        foreach (var h in list)
        {
            yuruyen += h.Borc - h.Alacak;
            h.YuruyenBakiye = yuruyen;
        }
        return list;
    }

    public int Insert(Hareket h)
    {
        const string sql = @"INSERT INTO HAREKETLER
            (FIRMA_ID, CARI_ID, TARIH, ISLEM_TURU, ACIKLAMA, BORC, ALACAK)
            VALUES (@FirmaId, @CariId, @Tarih, @IslemTuruInt, @Aciklama, @Borc, @Alacak);
            SELECT last_insert_rowid();";

        h.Id = _db.Connection.ExecuteScalar<int>(sql, new
        {
            h.FirmaId,
            h.CariId,
            h.Tarih,
            IslemTuruInt = (int)h.IslemTuru,
            h.Aciklama,
            h.Borc,
            h.Alacak
        });
        return h.Id;
    }

    public void Update(Hareket h)
    {
        const string sql = @"UPDATE HAREKETLER SET
            TARIH=@Tarih, ISLEM_TURU=@IslemTuruInt, ACIKLAMA=@Aciklama, BORC=@Borc, ALACAK=@Alacak
            WHERE ID=@Id";

        _db.Connection.Execute(sql, new
        {
            h.Id,
            h.Tarih,
            IslemTuruInt = (int)h.IslemTuru,
            h.Aciklama,
            h.Borc,
            h.Alacak
        });
    }

    public void Delete(int id)
    {
        _db.Connection.Execute("UPDATE HAREKETLER SET IS_DELETED=1 WHERE ID=@id", new { id });
    }

    public List<Hareket> GetGenelHareketler(int firmaId, DateTime basTarih, DateTime bitTarih, int tur)
    {
        var sql = @"
SELECT h.*, c.UNVAN AS CariUnvan
FROM HAREKETLER h
LEFT JOIN CARILER c ON c.ID=h.CARI_ID
WHERE h.FIRMA_ID=@firmaId AND h.IS_DELETED=0
AND h.TARIH>=@bas AND h.TARIH<=@bit ";

        var pars = new DynamicParameters();
        pars.Add("firmaId", firmaId);
        pars.Add("bas", basTarih.ToString("yyyy-MM-dd 00:00:00"));
        pars.Add("bit", bitTarih.ToString("yyyy-MM-dd 23:59:59"));

        if (tur > 0)
        {
            sql += "AND h.ISLEM_TURU=@tur ";
            pars.Add("tur", tur - 1);
        }
        sql += "ORDER BY h.TARIH ASC, h.ID ASC;";

        return _db.Connection.Query<Hareket>(sql, pars).AsList();
    }
}
