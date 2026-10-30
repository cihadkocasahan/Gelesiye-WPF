namespace GelesiyeWPF.Common;

public static class AppConsts
{
    // Uygulama
    public const string APP_NAME        = "Gelesiye";
    public const string APP_TITLE       = "Gelesiye — Basit Veresiye Takip";
    public const string APP_VERSION     = AppVersion.SemVer;
    public const string APP_SEMVER      = AppVersion.SemVer;
    public const string APP_BUILD_FULL  = AppVersion.BuildFull;
    public static readonly bool IS_RELEASE = AppVersion.IsRelease;

    // Veritabanı
    public const string DEF_DB_NAME     = "gelesiye.db";
    public const string DEF_CURRENCY    = "TL";
    public const double DEF_BORC_LIMITI = 10000.0;
    public const int    DEF_DONEM_YILI  = 2026;

    // İşlem Türü Metinleri
    public const string STR_TUR_BORC_DEKONTU   = "Borç Dekontu";
    public const string STR_TUR_TAHSILAT       = "Tahsilat";
    public const string STR_TUR_ALACAK_DEKONTU = "Alacak Dekontu";
    public const string STR_TUR_ODEME          = "Ödeme";
    public const string STR_TUR_DEVIR          = "Devir Fişi";

    // Hesap Durumları
    public const string STR_DURUM_AKTIF = "Aktif";
    public const string STR_DURUM_PASIF = "Pasif";

    // Hata Mesajları (Dinamik Yerelleştirme)
    public static string MSG_UNVAN_BOS_OLAMAZ   => Services.LocalizationManager.T("Val_UnvanBosOlamaz");
    public static string MSG_KOD_BOS_OLAMAZ     => Services.LocalizationManager.T("Val_CariKoduBosOlamaz");
    public static string MSG_TUTAR_SIFIR_OLAMAZ => Services.LocalizationManager.T("Msg_TutarSifirOlamaz");
    public static string MSG_SILME_ONAY         => Services.LocalizationManager.T("Msg_SilmeOnay");
    public static string MSG_DEVIR_BASARILI     => Services.LocalizationManager.T("Msg_DevirBasarili");

    public static string GetTitleWithVersion(string firmaUnvan = "")
        => AppVersion.TitleWithVersion(firmaUnvan);

    public static string GetStatusBarVersion()
        => AppVersion.StatusBarVersion();
}
