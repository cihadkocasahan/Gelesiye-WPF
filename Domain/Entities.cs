using GelesiyeWPF.Common;

namespace GelesiyeWPF.Domain;

// ============================================================
// Firma
// ============================================================
public class Firma
{
    public int    Id           { get; set; }
    public string Kod          { get; set; } = "";
    public string Unvan        { get; set; } = "";
    public int    Donem        { get; set; } = AppConsts.DEF_DONEM_YILI;
    public string ParaBirimi   { get; set; } = AppConsts.DEF_CURRENCY;
    public bool   IsDefault    { get; set; }
    public decimal BorcLimiti  { get; set; } = (decimal)AppConsts.DEF_BORC_LIMITI;
    public string Telefon      { get; set; } = "";
    public string Faks         { get; set; } = "";
    public string Adres        { get; set; } = "";
    public string Il           { get; set; } = "";
    public string Ilce         { get; set; } = "";
    public string VergiDairesi { get; set; } = "";
    public string VergiNo      { get; set; } = "";
}

// ============================================================
// Cari
// ============================================================
public class Cari
{
    public int      Id             { get; set; }
    public int      FirmaId        { get; set; }
    public string   CariKodu       { get; set; } = "";
    public string   Unvan          { get; set; } = "";
    public string   Yetkili        { get; set; } = "";
    public string   Telefon        { get; set; } = "";
    public string   Gsm            { get; set; } = "";
    public string   Faks           { get; set; } = "";
    public string   Adres          { get; set; } = "";
    public string   Il             { get; set; } = "";
    public string   Ilce           { get; set; } = "";
    public string   VergiDairesi   { get; set; } = "";
    public string   VergiNo        { get; set; } = "";
    public string   Eposta         { get; set; } = "";
    public string   Web            { get; set; } = "";
    public decimal  CariLimit      { get; set; } = (decimal)AppConsts.DEF_BORC_LIMITI;
    public HesapDurumu HesapDurumu { get; set; } = HesapDurumu.Aktif;
    public string   Notlar         { get; set; } = "";
    public DateTime KayitTarihi    { get; set; } = DateTime.Today;
    public DateTime SonIslemTarihi { get; set; } = DateTime.Today;

    // Hesaplanan alanlar (JOIN ile gelir)
    public decimal ToplamBorc   { get; set; }
    public decimal ToplamAlacak { get; set; }
    public decimal Bakiye       { get; set; }

    public string HesapDurumuText =>
        HesapDurumu == HesapDurumu.Aktif ? AppConsts.STR_DURUM_AKTIF : AppConsts.STR_DURUM_PASIF;
}

// ============================================================
// Hareket (Fiş)
// ============================================================
public class Hareket
{
    public int       Id            { get; set; }
    public int       FirmaId       { get; set; }
    public int       CariId        { get; set; }
    public string    CariUnvan     { get; set; } = "";
    public DateTime  Tarih         { get; set; } = DateTime.Now;
    public IslemTuru IslemTuru     { get; set; } = IslemTuru.Borc;
    public string    Aciklama      { get; set; } = "";
    public decimal   Borc          { get; set; }
    public decimal   Alacak        { get; set; }
    public decimal   YuruyenBakiye { get; set; }

    public string IslemTuruText => IslemTuru.ToDisplayString();
}
