namespace GelesiyeWPF.Data;

/// <summary>Delphi uDbDemoSeed karşılığı — test için 25 örnek cari ve hareket üretir.</summary>
public static class DemoSeed
{
    private static readonly (string kod, string unvan, string yetkili, string tel, string gsm,
        string il, string ilce, string vd, string vn, string mail, double limit)[] Data =
    {
        ("00001","Ahmet Yılmaz - Yılmaz Ticaret","Ahmet Yılmaz","0212 555 1001","0532 555 1001","İstanbul","Kadıköy","Kadıköy","1234567890","ahmet@yilmazticaret.com",25000),
        ("00002","Demir İnşaat & Hırdavat Ltd. Şti.","Mehmet Demir","0216 444 2002","0533 444 2002","İstanbul","Ümraniye","Sarıgazi","2345678901","info@demirinsaat.com.tr",50000),
        ("00003","Kaya Gıda Toptan ve Perakende","Ayşe Kaya","0232 333 3003","0535 333 3003","İzmir","Konak","Konak","3456789012","siparis@kayagida.com",35000),
        ("00004","Yıldız Kırtasiye ve Büro Malz.","Canan Yıldız","0312 222 4004","0542 222 4004","Ankara","Çankaya","Çankaya","4567890123","yildiz@kirtasiye.com",15000),
        ("00005","Şahin Otomotiv Yedek Parça","Mustafa Şahin","0224 555 5005","0544 555 5005","Bursa","Nilüfer","Nilüfer","5678901234","mustafa@sahinoto.com",40000),
        ("00006","Anadolu Lojistik & Taşımacılık A.Ş.","Emre Koç","0262 641 6006","0530 641 6006","Kocaeli","Gebze","İlyasbey","6789012345","info@anadolulojistik.com",60000),
        ("00007","Örnek Tekstil ve Konfeksiyon San.","Zeynep Arslan","0258 261 7007","0536 261 7007","Denizli","Pamukkale","Gökpınar","7890123456","zeynep@ornektekstil.com",30000),
        ("00008","Marmara Kimya ve Boya Sanayi","Murat Çelik","0212 674 8008","0537 674 8008","İstanbul","Esenyurt","İkitelli","8901234567","murat@marmarakimya.com",45000),
        ("00009","Boğaziçi Elektrik ve Elektronik","Burak Doğan","0212 288 9009","0538 288 9009","İstanbul","Şişli","Mecidiyeköy","9012345678","burak@bogazicielectric.com",35000),
        ("00010","Çukurova Tarım Ürünleri Ltd.","Ali Öztürk","0322 453 1010","0539 453 1010","Adana","Seyhan","Seyhan","0123456789","ali@cukurovatarim.com",75000),
        ("00011","Güneş Ambalaj ve Plastik Sanayi","Kemal Aydın","0342 338 1111","0541 338 1111","Gaziantep","Şehitkamil","Şehitkamil","1122334455","kemal@gunesambalaj.com",40000),
        ("00012","Akdeniz Mobilya ve Ahşap Sanayi","Seda Kurt","0242 345 1212","0543 345 1212","Antalya","Muratpaşa","Muratpaşa","2233445566","seda@akdenizmobilya.com",50000),
        ("00013","Karadeniz Balıkçılık & Su Ürünleri","Hasan Çakır","0462 321 1313","0545 321 1313","Trabzon","Ortahisar","Karadeniz","3344556677","hasan@karadenizbalik.com",25000),
        ("00014","Ege Çiftliği Süt ve Besicilik","Fatma Güler","0236 231 1414","0546 231 1414","Manisa","Yunusemre","Mesir","4455667788","fatma@egeciftligi.com",35000),
        ("00015","TeknoPark Bilişim Hizmetleri Ltd.","Oğuzhan Polat","0312 210 1515","0547 210 1515","Ankara","Yenimahalle","Yenimahalle","5566778899","oguzhan@teknoparkbilisim.com",60000),
        ("00016","Başak Ekmek ve Unlu Mamulleri","İsmail Keskin","0332 350 1616","0548 350 1616","Konya","Selçuklu","Mevlana","6677889900","ismail@basakmamul.com",20000),
        ("00017","Umut Temizlik ve Hijyen Malz.","Hatice Korkmaz","0222 230 1717","0549 230 1717","Eskişehir","Tepebaşı","Battalgazi","7788990011","hatice@umuttemizlik.com",15000),
        ("00018","Barış Matbaacılık ve Yayıncılık","Barış Yalçın","0212 567 1818","0551 567 1818","İstanbul","Zeytinburnu","Zeytinburnu","8899001122","baris@barismatbaa.com",30000),
        ("00019","Zirve Endüstriyel Mutfak Ekip.","Serhat Erdem","0216 390 1919","0552 390 1919","İstanbul","Tuzla","Tuzla","9900112233","serhat@zirvemutfak.com",80000),
        ("00020","Atlas Medikal ve Sağlık Gereçleri","Elif Yavuz","0312 430 2020","0553 430 2020","Ankara","Altındağ","Ulus","1029384756","elif@atlasmedikal.com",45000),
        ("00021","Serin Soğutma & İklimlendirme","Tolga Acar","0232 464 2121","0554 464 2121","İzmir","Bornova","Bornova","2130495867","tolga@serinsogutma.com",35000),
        ("00022","Elit Yapı Malzemeleri ve Boya","Berna Aslan","0242 228 2222","0555 228 2222","Antalya","Kepez","Düden","3241506978","berna@elityapi.com",40000),
        ("00023","Mega Metal ve Alüminyum Doğrama","Kadir Şen","0224 441 2323","0556 441 2323","Bursa","Osmangazi","Osmangazi","4352617089","kadir@megametal.com",55000),
        ("00024","Doğu Tarım & Hayvancılık Paz.","Hakan Güneş","0442 233 2424","0557 233 2424","Erzurum","Yakutiye","Aziziye","5463728190","hakan@dogutarim.com",65000),
        ("00025","Güven Sigorta ve Aracılık Hiz.","Selin Çetin","0212 212 2525","0558 212 2525","İstanbul","Beşiktaş","Beşiktaş","6574839201","selin@guvensigorta.com",20000),
    };

    /// <summary>Demo verileri ekler. <paramref name="force"/> false ise 10+ cari varsa dokunmaz.</summary>
    public static int Seed(Database db, int firmaId, bool force = false)
    {
        if (!force && db.ScalarLong("SELECT COUNT(*) FROM CARILER WHERE IS_DELETED=0 AND FIRMA_ID=@f",
                new() { ["@f"] = firmaId }) >= 10)
            return 0;

        var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        int added = 0;

        using var tx = db.Connection.BeginTransaction();
        try
        {
            for (int i = 0; i < Data.Length; i++)
            {
                var d = Data[i];
                int v = i + 1;

                db.Execute(@"INSERT INTO CARILER
                    (FIRMA_ID,CARI_KODU,UNVAN,YETKILI,TELEFON,GSM,FAKS,ADRES,IL,ILCE,VERGI_DAIRESI,
                     VERGI_NO,EPOSTA,WEB,CARI_LIMIT,HESAP_DURUMU,NOTLAR,KAYIT_TARIHI,IS_DELETED)
                    VALUES(@fid,@k,@u,@y,@tel,@gsm,'',@adr,@il,@ilce,@vd,@vn,@mail,@web,@lim,1,
                           'Demo tohumlama cari kartı',@kayit,0)",
                    new()
                    {
                        ["@fid"] = firmaId, ["@k"] = d.kod, ["@u"] = d.unvan, ["@y"] = d.yetkili,
                        ["@tel"] = d.tel, ["@gsm"] = d.gsm,
                        ["@adr"] = $"{d.ilce} Mah. Atatürk Cad. No: {v * 3}",
                        ["@il"] = d.il, ["@ilce"] = d.ilce, ["@vd"] = d.vd, ["@vn"] = d.vn,
                        ["@mail"] = d.mail,
                        ["@web"] = "www." + d.mail[(d.mail.IndexOf('@') + 1)..],
                        ["@lim"] = d.limit, ["@kayit"] = now
                    });
                int cariId = (int)db.GetLastInsertRowId();

                void H(string tarih, int tur, string acik, double borc, double alacak) =>
                    db.Execute(@"INSERT INTO HAREKETLER
                        (FIRMA_ID,CARI_ID,TARIH,ISLEM_TURU,ACIKLAMA,BORC,ALACAK,IS_DELETED)
                        VALUES(@f,@c,@t,@tur,@a,@b,@al,0)",
                        new()
                        {
                            ["@f"] = firmaId, ["@c"] = cariId, ["@t"] = tarih, ["@tur"] = tur,
                            ["@a"] = acik, ["@b"] = borc, ["@al"] = alacak
                        });

                double totalBorc = (3000.0 + v * 250.0) + (5000.0 + v * 380.0) + (4500.0 + v * 310.0) + (2800.0 + v * 170.0);
                double totalAlacak = (4000.0 + v * 200.0) + (3500.0 + v * 290.0);
                if (v % 2 == 0) totalAlacak += (2000.0 + v * 150.0);
                if (v % 3 == 0) totalAlacak += (750.0 + v * 50.0);

                H("2026-01-02 09:30:00", 4, "2025 Yılı Devir Bakiyesi", 3000.0 + v * 250.0, 0);
                H("2026-01-15 14:20:00", 0, "Hizmet Bedeli / Mal Alımı", 5000.0 + v * 380.0, 0);
                H("2026-01-28 11:15:00", 1, "Banka Havalesi ile Tahsilat", 0, 4000.0 + v * 200.0);
                H("2026-02-10 16:45:00", 0, "Fatura Bedeli (#A-10492)", 4500.0 + v * 310.0, 0);
                H("2026-02-22 10:00:00", 1, "Kredi Kartı ile Tahsilat", 0, 3500.0 + v * 290.0);
                H("2026-03-05 13:30:00", 0, "Veresiye Satış Faturası", 2800.0 + v * 170.0, 0);
                if (v % 2 == 0) H("2026-03-12 15:00:00", 1, "Nakit Tahsilat", 0, 2000.0 + v * 150.0);
                if (v % 3 == 0) H("2026-03-18 17:10:00", 2, "Mal İadesi / İskonto Bedeli", 0, 750.0 + v * 50.0);

                if (v % 4 == 3)
                {
                    // Alacaklı bakiye (Avans / fazla ödeme)
                    double diff = totalBorc - totalAlacak;
                    double avans = 3500.0 + v * 250.0;
                    H("2026-03-24 14:30:00", 1, "Avans Ödemesi / Fazla Tahsilat", 0, diff + avans);
                }

                added++;
            }
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
        return added;
    }

    /// <summary>
    /// Mevcut veritabanında tüm bakiyeler borçlu kalmışsa (eski demo tohumlama),
    /// gerçekçi bir dağılım sağlamak için bir kısım cariye mutabakat ve avans hareketleri ekler.
    /// </summary>
    public static void EnsureRealisticMix(Database db, int firmaId)
    {
        try
        {
            long cariSayisi = db.ScalarLong("SELECT COUNT(*) FROM CARILER WHERE FIRMA_ID=@f AND IS_DELETED=0",
                new() { ["@f"] = firmaId });
            if (cariSayisi == 0) return;

            long nonBorcluSayisi = db.ScalarLong(@"
                SELECT COUNT(*) FROM (
                    SELECT c.ID, COALESCE(SUM(h.BORC),0) - COALESCE(SUM(h.ALACAK),0) AS BAKIYE
                    FROM CARILER c
                    LEFT JOIN HAREKETLER h ON (c.ID = h.CARI_ID AND h.IS_DELETED=0)
                    WHERE c.FIRMA_ID=@f AND c.IS_DELETED=0
                    GROUP BY c.ID
                    HAVING BAKIYE <= 0.001
                )", new() { ["@f"] = firmaId });

            // Daha önce yapay olarak eklenmiş hesap kapatma hareketleri varsa temizleyelim
            db.Execute("DELETE FROM HAREKETLER WHERE ACIKLAMA = 'Hesap Kapatma / Mutabakat Ödemesi' AND FIRMA_ID = @f",
                new() { ["@f"] = firmaId });

            if (nonBorcluSayisi > 0) return; // Zaten alacaklı cariler mevcut

            using var cmd = db.Connection.CreateCommand();
            cmd.CommandText = @"
                SELECT c.ID, COALESCE(SUM(h.BORC),0) - COALESCE(SUM(h.ALACAK),0) AS BAKIYE
                FROM CARILER c
                LEFT JOIN HAREKETLER h ON (c.ID = h.CARI_ID AND h.IS_DELETED=0)
                WHERE c.FIRMA_ID=@f AND c.IS_DELETED=0
                GROUP BY c.ID
                ORDER BY c.ID";
            cmd.Parameters.AddWithValue("@f", firmaId);

            var toAdjust = new List<(int cariId, decimal bakiye, int index)>();
            using (var reader = cmd.ExecuteReader())
            {
                int idx = 0;
                while (reader.Read())
                {
                    idx++;
                    int cid = reader.GetInt32(0);
                    decimal bak = Convert.ToDecimal(reader.GetDouble(1));
                    toAdjust.Add((cid, bak, idx));
                }
            }

            foreach (var (cid, bak, idx) in toAdjust)
            {
                if (bak <= 0) continue;

                if (idx % 4 == 3)
                {
                    // Alacaklı bakiye (Avans ödeme)
                    double avans = 3500.0 + (idx * 250.0);
                    db.Execute(@"INSERT INTO HAREKETLER (FIRMA_ID, CARI_ID, TARIH, ISLEM_TURU, ACIKLAMA, BORC, ALACAK, IS_DELETED)
                        VALUES (@f, @c, '2026-03-24 14:30:00', 1, 'Avans Ödemesi / Fazla Tahsilat', 0, @al, 0)",
                        new() { ["@f"] = firmaId, ["@c"] = cid, ["@al"] = (double)bak + avans });
                }
            }
        }
        catch (Exception ex)
        {
            Services.LogService.Warning($"EnsureRealisticMix istisnası: {ex.Message}");
        }
    }
}
