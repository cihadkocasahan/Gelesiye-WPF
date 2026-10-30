namespace GelesiyeWPF.Common;

public enum IslemTuru
{
    Borc     = 0,   // F5 — Borç Dekontu
    Tahsilat = 1,   // F6 — Tahsilat
    Alacak   = 2,   // F7 — Alacak Dekontu
    Odeme    = 3,   // F8 — Ödeme
    Devir    = 4    // Yıl sonu devir fişi
}

public enum HesapDurumu
{
    Tumu  = 0,
    Aktif = 1,
    Pasif = 2
}

public enum BakiyeFiltresi
{
    Tumu         = 0,
    Borclular    = 1,
    Alacaklilar  = 2,
    BakiyesiArti = 3,
    SifirBakiye  = 4,
    LimitAsanlar = 5
}

public enum SorguTuru
{
    Unvan   = 0,
    CariKod = 1
}

public enum AramaModu
{
    YazildigiGibi = 0,
    Iceren        = 1
}

public enum DialogTur
{
    Bilgi,
    Onay,
    Uyari,
    Hata
}

public record BakiyeOzet
{
    public decimal AktifBorcluToplam    { get; init; }
    public decimal AktifAlacakliToplam  { get; init; }
    public decimal PasifBorcluToplam    { get; init; }
    public decimal PasifAlacakliToplam  { get; init; }
    public decimal GenelBorcluToplam    { get; init; }
    public decimal GenelAlacakliToplam  { get; init; }
    public decimal NetBakiye            { get; init; }
    public int     ToplamCariSayisi     { get; init; }
    public int     AktifCariSayisi      { get; init; }
    public int     PasifCariSayisi      { get; init; }
    public int     BorcluCariSayisi     { get; init; }
    public int     AlacakliCariSayisi   { get; init; }
    public int     SifirBakiyeCariSayisi{ get; init; }
}

public record CariFiltre
{
    public int           FirmaId        { get; init; }
    public string        AramaMetni     { get; init; } = "";
    public SorguTuru     SorguTuru      { get; init; } = SorguTuru.Unvan;
    public AramaModu     AramaModu      { get; init; } = AramaModu.YazildigiGibi;
    public HesapDurumu   HesapDurumu    { get; init; } = HesapDurumu.Tumu;
    public BakiyeFiltresi BakiyeFiltresi{ get; init; } = BakiyeFiltresi.Tumu;
}

public static class IslemTuruHelper
{
    public static string ToDisplayString(this IslemTuru tur) => tur switch
    {
        IslemTuru.Borc     => Services.LocalizationManager.T("Fis_Borc_Kisa"),
        IslemTuru.Tahsilat => Services.LocalizationManager.T("Fis_Tahsilat_Kisa"),
        IslemTuru.Alacak   => Services.LocalizationManager.T("Fis_Alacak_Kisa"),
        IslemTuru.Odeme    => Services.LocalizationManager.T("Fis_Odeme_Kisa"),
        IslemTuru.Devir    => Services.LocalizationManager.T("Fis_Devir_Kisa"),
        _                  => Services.LocalizationManager.T("Fis_Borc_Kisa")
    };

    public static IslemTuru FromString(string s) => s.Trim() switch
    {
        var x when x == "Borç Dekontu" || x == "Debit Voucher" || x == "BORC" => IslemTuru.Borc,
        var x when x == "Tahsilat" || x == "Collection" || x == "TAHSILAT" => IslemTuru.Tahsilat,
        var x when x == "Alacak Dekontu" || x == "Credit Voucher" || x == "ALACAK" => IslemTuru.Alacak,
        var x when x == "Ödeme" || x == "Payment" || x == "ODEME" => IslemTuru.Odeme,
        var x when x == "Devir Fişi" || x == "Transfer Voucher" || x == "DEVIR" => IslemTuru.Devir,
        _ => IslemTuru.Borc
    };

    public static string FormatPara(decimal value, string paraBirimi = "TL")
        => $"{value:N2} {paraBirimi}";
}
