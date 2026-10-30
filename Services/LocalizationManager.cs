using System.ComponentModel;
using System.Globalization;
using GelesiyeWPF.Data;

namespace GelesiyeWPF.Services;

/// <summary>
/// Çoklu dil (i18n / l10n) dinamik dil yönetim servisi.
/// Açık pencereleri kapatıp açmadan (restart gerektirmeden) anında arayüzü günceller.
/// </summary>
public sealed class LocalizationManager : INotifyPropertyChanged
{
    public static LocalizationManager Instance { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    private string _currentLanguage = "tr-TR";
    private CultureInfo _currentCulture = new("tr-TR");

    public CultureInfo CurrentCulture => _currentCulture;
    public string CurrentLanguage => _currentLanguage;

    public static readonly (string Code, string Name)[] DesteklenenDiller =
    [
        ("tr-TR", "Türkçe"),
        ("en-US", "English")
    ];

    private readonly Dictionary<string, Dictionary<string, string>> _translations = new(StringComparer.OrdinalIgnoreCase);

    private LocalizationManager()
    {
        DilleriYukle();
    }

    /// <summary>
    /// XAML tarafındaki {loc:Loc Key} bağlamaları için dinamik dizinleyici (Indexer).
    /// </summary>
    public string this[string key] => Get(key);

    public string Get(string key)
    {
        if (_translations.TryGetValue(_currentLanguage, out var dict) && dict.TryGetValue(key, out var val))
            return val;

        // Fallback: Türkçe varsayılanına bak
        if (_currentLanguage != "tr-TR" && _translations.TryGetValue("tr-TR", out var trDict) && trDict.TryGetValue(key, out var trVal))
            return trVal;

        return key;
    }

    public static string T(string key) => Instance.Get(key);

    public void SetLanguage(string languageCode, bool kaydet = true)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            languageCode = "tr-TR";

        _currentLanguage = languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en-US" : "tr-TR";
        _currentCulture = new CultureInfo(_currentLanguage);

        CultureInfo.DefaultThreadCurrentCulture = _currentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _currentCulture;
        Thread.CurrentThread.CurrentCulture = _currentCulture;
        Thread.CurrentThread.CurrentUICulture = _currentCulture;

        if (kaydet)
        {
            try
            {
                DbFactory.GetAyarlarRepo().SetAyar("UygulamaDili", _currentLanguage);
            }
            catch
            {
                // Veritabanı henüz hazır değilse yoksay
            }
        }

        // null parametresi tüm bağlamaların (Indexer dahil) anında yeniden değerlendirilmesini sağlar
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
    }

    private void DilleriYukle()
    {
        // ══════════════════════ TÜRKÇE (Varsayılan) ══════════════════════
        var tr = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Genel & Başlık
            ["App_Title"] = "Gelesiye — Basit Veresiye Takip",
            ["App_Motto"] = "  |  Basit Veresiye Takip",
            ["Firma_Demo"] = "DEMO TİCARET",
            ["ParaBirimi_TL"] = "TL",

            // Pencereler & Başlıklar
            ["Title_Ayarlar"] = "⚙  Program ve Firma Ayarları",
            ["Title_YedekYonetimi"] = "💾  Yedekleme ve Geri Yükleme Yönetimi",
            ["Title_CariKarti"] = "Cari Kartı",
            ["Title_YeniCari"] = "Yeni Cari Kartı Ekle",
            ["Title_CariDuzenle"] = "Cari Kartı Düzenle",
            ["Title_Fis"] = "Fiş",
            ["Title_FisDuzenle"] = "Fiş Düzenle",
            ["Title_Giris"] = "GELESIYE — Giriş",
            ["Title_CikisOnayi"] = "Gelesiye — Çıkış Onayı",
            ["Title_ListeToplamlari"] = "∑  Liste Toplamları",
            ["Title_RaporSecimi"] = "🖨  Yazdırma ve Rapor Seçimi",
            ["Title_Hakkinda"] = "Hakkında",
            ["Title_Uyari"] = "Uyarı",
            ["Title_Hata"] = "Hata",
            ["Title_Bilgi"] = "Bilgi",
            ["Title_Onay"] = "Onay",

            // Butonlar & Eylemler
            ["Btn_Ekle"] = "Yeni Cari (F3)",
            ["Btn_Duzenle"] = "Düzelt (F4)",
            ["Btn_Sil"] = "Sil (Del)",
            ["Btn_Yazdir"] = "Yazdır…",
            ["Btn_Ozet"] = "Özet (F2)",
            ["Btn_Hareketler"] = "Hareketler ➔",
            ["Btn_CariListesi"] = "⬅ Cari Listesi",
            ["Btn_Fisler"] = "➕ Yeni Fiş…",
            ["Btn_Kaydet"] = "💾 Kaydet",
            ["Btn_AyarlariKaydet"] = "💾 Ayarları Kaydet",
            ["Btn_Iptal"] = "İptal",
            ["Btn_IptalEsc"] = "İptal (Esc)",
            ["Btn_KaydetF2"] = "Kaydet (F2)",
            ["Btn_Kapat"] = "Kapat",
            ["Btn_Tamam"] = "Tamam",
            ["Btn_Hayir"] = "Hayır",
            ["Btn_Evet"] = "Evet",
            ["Btn_Sec"] = "Seç…",
            ["Btn_Varsayilan"] = "Varsayılan",
            ["Btn_SecileniSil"] = "🗑 Seçileni Sil",
            ["Btn_TumunuSil"] = "Tümünü Sil",
            ["Btn_SimdiYedekle"] = "💾 Şimdi Yedek Al",
            ["Btn_GeriYukle"] = "↩ Seçili Yedeği Geri Yükle",
            ["Btn_KlasordeGoster"] = "📂 Klasörde Göster",
            ["Btn_Giris"] = "Giriş Yap ➔",
            ["Btn_Cikis"] = "Çıkış",
            ["Btn_SifreGuncelle"] = "🔑 Şifreyi Güncelle",
            ["Btn_SimgeyeKucult"] = "Simgeye Küçült",
            ["Btn_TamamenCik"] = "Tamamen Çık",
            ["Btn_HesapMakinesi"] = "🧮 Hesap Makinesi",
            ["Btn_MesajAt"] = "Mesaj At",
            ["Btn_ProfiliAc"] = "Profili Aç",
            ["Btn_SayfayiAc"] = "Sayfayı Aç",

            // Tooltips
            ["Tooltip_Menu"] = "Seçenekler ve İşlemler Menüsü",
            ["Tooltip_Minimize"] = "Simge Durumuna Küçült",
            ["Tooltip_Maximize"] = "Ekranı Kapla / Önceki Boyut",
            ["Tooltip_Close"] = "Programdan Çıkış",
            ["Tooltip_OncekiCari"] = "Önceki Cari (← / Alt+←)",
            ["Tooltip_SonrakiCari"] = "Sonraki Cari (→ / Alt+→)",
            ["Tooltip_HesapMakinesi"] = "Hesap Makinesi",
            ["Tooltip_AramaYontemi"] = "Arama Yöntemi",
            ["Tooltip_HesapFiltresi"] = "Hesap Durumu Filtresi",
            ["Tooltip_BakiyeFiltresi"] = "Bakiye Filtresi",

            // Menü Öğeleri
            ["Menu_Ayarlar"] = "⚙  Ayarlar…",
            ["Menu_YedekYonetimi"] = "💾  Yedek Yönetimi…",
            ["Menu_ListeToplamlari"] = "∑  Liste Toplamları (F2)",
            ["Menu_RaporSecimi"] = "🖨  Rapor Seçimi…",
            ["Menu_HizliYedek"] = "💾  Hızlı Yedek Al",
            ["Menu_DemoYukle"] = "🧪  Demo Verileri Ekle (25 cari)",
            ["Menu_YilSonuDevir"] = "📅  Yıl Sonu Devir…",
            ["Menu_Hakkinda"] = "ℹ  Hakkında",
            ["Menu_Cikis"] = "✕  Çıkış",

            // Fiş Türleri
            ["Fis_Borc"] = "F5 — Hesaba Borç Ekle (Borç Dekontu)",
            ["Fis_Tahsilat"] = "F6 — Tahsilat Yap",
            ["Fis_Alacak"] = "F7 — Hesaba Alacak Ekle (Alacak Dek.)",
            ["Fis_Odeme"] = "F8 — Ödeme Yap",
            ["Fis_Devir"] = "Devir Fişi",
            ["Fis_Borc_Kisa"] = "Borç Dekontu",
            ["Fis_Tahsilat_Kisa"] = "Tahsilat",
            ["Fis_Alacak_Kisa"] = "Alacak Dekontu",
            ["Fis_Odeme_Kisa"] = "Ödeme",
            ["Fis_Devir_Kisa"] = "Devir Fişi",

            // Kartlar & İstatistikler
            ["Stat_ToplamBorc"] = "TOPLAM BORÇ (MÜŞTERİLER)",
            ["Stat_ToplamAlacak"] = "TOPLAM ALACAK (FİRMA)",
            ["Stat_NetBakiye"] = "NET BAKİYE",
            ["Stat_MusteriSayisi"] = "MÜŞTERİ SAYISI",
            ["Stat_IslemSayisi"] = "İŞLEM SAYISI",
            ["Stat_Bakiye"] = "BAKİYE",
            ["Stat_Durum"] = "DURUM",
            ["Stat_BorcluCariSub"] = "Aktif borçlu cari",
            ["Stat_AlacakliCariSub"] = "Aktif alacaklı cari",
            ["Stat_NetDurumSub"] = "Genel net durum",
            ["Stat_BorcGirisleri"] = "Borç girişleri",
            ["Stat_AlacakGirisleri"] = "Alacak girişleri",
            ["Stat_NetDurum"] = "Net durum",
            ["Stat_HareketKaydi"] = "Hareket kaydı",
            ["Stat_Aktif"] = "Aktif",
            ["Stat_Pasif"] = "Pasif",
            ["Stat_Borclu"] = "Borçlu",
            ["Stat_Alacakli"] = "Alacaklı",
            ["Stat_Sifir"] = "Sıfır",

            // Tablo Başlıkları
            ["Col_CariKodu"] = "Cari Kodu",
            ["Col_Unvan"] = "Firma / Şahıs Unvanı",
            ["Col_Telefon"] = "Telefon / GSM",
            ["Col_IlIlce"] = "İl / İlçe",
            ["Col_Borc"] = "Borç",
            ["Col_Alacak"] = "Alacak",
            ["Col_Bakiye"] = "Bakiye",
            ["Col_Tarih"] = "Tarih",
            ["Col_Saat"] = "Saat",
            ["Col_EvrakNo"] = "Evrak No",
            ["Col_IslemTuru"] = "İşlem Türü",
            ["Col_Aciklama"] = "Açıklama",
            ["Col_DosyaAdi"] = "Dosya Adı",
            ["Col_Boyut"] = "Boyut",
            ["Col_TamYol"] = "Tam Yol",
            ["Col_SonIslem"] = "Son İşlem",
            ["Col_YuruyenBakiye"] = "Yürüyen Bakiye",

            // Filtreler
            ["Filter_Arama"] = "Arama:",
            ["Filter_AramaPlaceholder"] = "Cari adı veya kodu ile ara…",
            ["Filter_ModYazildigi"] = "Yazıldığı Gibi (Başlayan)",
            ["Filter_ModIceren"] = "İçeren (Her yerinde)",
            ["Filter_HesapTumu"] = "Tüm Hesaplar",
            ["Filter_HesapAktif"] = "Sadece Aktifler",
            ["Filter_HesapPasif"] = "Sadece Pasifler",
            ["Filter_BakiyeTumu"] = "Tüm Bakiyeler",
            ["Filter_BakiyeBorclular"] = "Sadece Borçlular (>0)",
            ["Filter_BakiyeAlacaklilar"] = "Sadece Alacaklılar (<0)",
            ["Filter_BakiyeSifir"] = "Bakiyesi Sıfır Olanlar",
            ["Filter_BakiyeSifirOlmayan"] = "Bakiyesi Sıfır Olmayanlar",
            ["Filter_LimitAsanlar"] = "⚠ Limit Aşanlar",

            // Ayarlar Sekmeleri
            ["Tab_Firma"] = "Firma Bilgileri",
            ["Tab_Genel"] = "Genel",
            ["Tab_Yedekleme"] = "Yedekleme",
            ["Tab_Sifre"] = "Şifre Değiştir",
            ["Tab_Aciklamalar"] = "Açıklama Şablonları",
            ["Tab_CariBilgileri"] = "Cari Bilgileri",
            ["Tab_OzelNotlar"] = "Özel Notlar",

            // Form Etiketleri
            ["Lbl_FirmaUnvani"] = "FİRMA UNVANI",
            ["Lbl_CariKodu"] = "CARİ KODU",
            ["Lbl_CariUnvan"] = "CARİ UNVAN",
            ["Lbl_Telefon"] = "TELEFON",
            ["Lbl_Gsm"] = "GSM",
            ["Lbl_Faks"] = "FAKS",
            ["Lbl_Il"] = "İL",
            ["Lbl_Ilce"] = "İLÇE",
            ["Lbl_VergiDairesi"] = "VERGİ DAİRESİ",
            ["Lbl_VergiNo"] = "VERGİ NO / TC NO",
            ["Lbl_Adres"] = "ADRES",
            ["Lbl_ParaBirimi"] = "PARA BİRİMİ",
            ["Lbl_Dil"] = "UYGULAMA DİLİ",
            ["Lbl_KapatmaEylemi"] = "KAPATMA EYLEMİ (Pencere kapatıldığında)",
            ["Lbl_KapatmaSor"] = "0 — Her zaman sor",
            ["Lbl_KapatmaCik"] = "1 — Doğrudan programdan çık",
            ["Lbl_KapatmaKucult"] = "2 — Simge durumuna küçült (Saatin yanına)",
            ["Lbl_SifreKorumasi"] = "Açılışta şifre koruması aktif olsun",
            ["Lbl_GorunumVeListeler"] = "GÖRÜNÜM VE LİSTELER",
            ["Lbl_GuvenlikGiris"] = "GÜVENLİK VE GİRİŞ",
            ["Lbl_ZebraGrid"] = "Listelerde ardışık satır renklendirmesi (Zebra stili) aktif olsun",
            ["Lbl_FiltreVarsayilanlari"] = "ARAMA VE LİSTE FİLTRELERİ",
            ["Lbl_VarsayilanAramaModu"] = "Varsayılan Arama Yöntemi:",
            ["Lbl_VarsayilanHesapDurumu"] = "Varsayılan Hesap Durumu:",
            ["Lbl_VarsayilanBakiyeFiltresi"] = "Varsayılan Bakiye Filtresi:",
            ["Lbl_FiltreHatirla"] = "Son kullanılan filtreleri hatırla ve açılışta yükle",
            ["Lbl_OtoYedekleme"] = "OTOMATİK YEDEKLEME DÖNGÜSÜ",
            ["Lbl_MaksYedek"] = "SAKLANACAK YEDEK SAYISI",
            ["Lbl_YedekKlasoru"] = "YEDEKLEME KLASÖRÜ",
            ["Lbl_MevcutSifre"] = "MEVCUT ŞİFRE",
            ["Lbl_YeniSifre"] = "YENİ ŞİFRE",
            ["Lbl_YeniSifreTekrar"] = "YENİ ŞİFRE (TEKRAR)",
            ["Lbl_IslemTuru"] = "İŞLEM TÜRÜ:",
            ["Lbl_Tutar"] = "TUTAR",
            ["Lbl_Aciklama"] = "AÇIKLAMA",
            ["Lbl_RiskLimiti"] = "BORÇ RİSK LİMİTİ",
            ["Lbl_Yetkili"] = "YETKİLİ",
            ["Lbl_Eposta"] = "E-POSTA",
            ["Lbl_Web"] = "WEB SİTESİ",
            ["Lbl_KayitTarihi"] = "KAYIT TARİHİ",
            ["Lbl_HesapDurumu"] = "HESAP DURUMU",
            ["Lbl_Notlar"] = "ÖZEL NOTLAR",
            ["Lbl_Tarih"] = "TARİH",
            ["Lbl_Saat"] = "SAAT",
            ["Lbl_CariHesap"] = "CARİ HESAP",
            ["Opt_Aktif"] = "Aktif",
            ["Opt_Pasif"] = "Pasif",

            // Rapor & Toplamlar & Yazdırma
            ["Title_BaskiOnizleme"] = "🖨  Baskı Önizleme ve Yazdırma",
            ["Lbl_RaporSeciniz"] = "Lütfen yazdırmak veya önizlemek istediğiniz raporu seçiniz:",
            ["Btn_RaporuAc"] = "🖨 Raporu Aç ➔",
            ["Btn_RaporuGoruntule"] = "🖨 Raporu Görüntüle",
            ["Btn_YazdirOnizle"] = "🖨  Yazdır (Ctrl+P)",
            ["Btn_TekSayfa"] = "📄 Tek Sayfa",
            ["Btn_CiftSayfa"] = "📖 Çift Sayfa",
            ["Btn_SurekliKaydirma"] = "📜 Sürekli",
            ["Btn_Yakinlastir"] = "🔍+ Yakınlaştır",
            ["Btn_Uzaklastir"] = "🔍− Uzaklaştır",
            ["Btn_YuzdeYuz"] = "%100",
            ["Tooltip_Yazdir"] = "Belgeyi yazıcıya gönder veya PDF olarak kaydet (Ctrl+P)",
            ["Tooltip_TekSayfa"] = "Tek sayfa görünümü",
            ["Tooltip_CiftSayfa"] = "Yan yana çift sayfa görünümü",
            ["Tooltip_Surekli"] = "Sürekli dikey kaydırma görünümü",
            ["Tooltip_Yakinlastir"] = "Görünümü büyüt (Ctrl +)",
            ["Tooltip_Uzaklastir"] = "Görünümü küçült (Ctrl -)",
            ["Tooltip_YuzdeYuz"] = "Varsayılan %100 yakınlaştırmaya dön",
            ["Msg_YazdirmaHatasi"] = "Yazdırma sırasında bir hata oluştu:\n{0}",
            ["Rpr_SeciliEkstre"] = "Seçili Cari Ekstresi",
            ["Rpr_SeciliEkstreSub"] = "Hesap ekstresi ve hareket dökümü",
            ["Rpr_CariListesi"] = "Genel Cari Hesap Listesi ve Bakiyeler",
            ["Rpr_CariListesiSub"] = "Tüm kayıtlı müşteriler ve güncel borç/alacak bakiyeleri",
            ["Rpr_GenelHareketler"] = "Genel Hareket Dökümü (Tüm Fişler)",
            ["Rpr_GenelHareketlerSub"] = "Tüm carilerin tarih sırasına göre işlem dökümü",
            ["Lbl_CariHesapBakiyeToplamlari"] = "CARİ HESAP BAKİYE TOPLAMLARI",
            ["Lbl_ToplamBorc"] = "TOPLAM BORÇ",
            ["Lbl_ToplamAlacak"] = "TOPLAM ALACAK",
            ["Lbl_NetBakiye"] = "NET BAKİYE",
            ["Lbl_KayitSayilari"] = "KAYIT SAYILARI VE İSTATİSTİKLER",
            ["Lbl_ToplamCari"] = "TOPLAM CARİ",
            ["Lbl_AktifCari"] = "AKTİF CARİ",
            ["Lbl_PasifCari"] = "PASİF CARİ",
            ["Lbl_BorcluCari"] = "BORÇLU CARİ",
            ["Lbl_AlacakliCari"] = "ALACAKLI CARİ",
            ["Lbl_SifirBakiye"] = "BAKİYESİ SIFIR",

            // Çıkış Onayı & Giriş
            ["Msg_KapatmakIstiyorMusunuz"] = "Programı kapatmak istiyor musunuz?",
            ["Msg_EylemiSeciniz"] = "Lütfen gerçekleştirmek istediğiniz eylemi seçiniz:",
            ["Lbl_SecimiHatirla"] = "Bu seçimimi hatırla ve bir daha sorma",
            ["Lbl_SifreGiriniz"] = "Programa giriş için şifrenizi giriniz.",
            ["Lbl_KalanHak"] = "Kalan Deneme: {0}",
            ["Status_CariListesi"] = "📋 Cari Listesi",
            ["Status_HareketDetayi"] = "📊 Hareket Detayı",

            // Geliştirici & Hakkında
            ["Lbl_Gelistirici"] = "GELİŞTİRİCİ",
            ["Lbl_Slogan"] = "\"Verme veresiye beklersin gelesiye,\nmadem verdin veresiye sana lazım Gelesiye!\"",
            ["Lbl_Versiyon"] = "Versiyon {0} — 2026",

            // Bildirimler & Diyaloglar
            ["Msg_SilOnay"] = "Seçili cari kartı ve tüm hareketleri silinecek. Emin misiniz?",
            ["Msg_SilBaslik"] = "Cari Sil",
            ["Msg_FisSilOnay"] = "Seçili fiş silinecek. Emin misiniz?",
            ["Msg_KayitBasarili"] = "İşlem başarıyla kaydedildi.",
            ["Msg_SifreGuncellendi"] = "Giriş şifreniz başarıyla güncellendi.",
            ["Msg_HataSifre"] = "Mevcut şifreniz hatalı!",
            ["Msg_HataSifreEslesmedi"] = "Yeni şifreler birbiriyle uyuşmuyor!",
            ["Msg_TrayBilgi"] = "Program sistem tepsisinde (saatin yanında) çalışmaya devam ediyor. Tekrar açmak için simgeye çift tıklayabilirsiniz.",
            ["Tray_Goster"] = "📋  Göster (Ana Pencereyi Aç)",
            ["Tray_YedekAl"] = "💾  Hızlı Yedek Al",
            ["Tray_YedekYonetimi"] = "💾  Yedek Yönetimi…",
            ["Tray_Hakkinda"] = "ℹ  Hakkında",
            ["Tray_Cikis"] = "✕  Programdan Çıkış",

            // Validasyon ve Servis Mesajları
            ["Val_CariKoduBosOlamaz"] = "Cari kodu boş olamaz.",
            ["Val_CariKoduMaks50"] = "Cari kodu en fazla 50 karakter olabilir.",
            ["Val_UnvanBosOlamaz"] = "Cari unvanı boş olamaz.",
            ["Val_UnvanMaks150"] = "Cari unvanı en fazla 150 karakter olabilir.",
            ["Val_LimitNegatifOlamaz"] = "Risk limiti 0'dan küçük olamaz.",
            ["Val_GecersizEposta"] = "Geçersiz e-posta adresi formatı.",
            ["Val_TelefonMaks30"] = "Telefon numarası en fazla 30 karakter olabilir.",
            ["Val_GsmMaks30"] = "GSM numarası en fazla 30 karakter olabilir.",
            ["Val_VergiNoMaks20"] = "Vergi numarası en fazla 20 karakter olabilir.",
            ["Val_GecerliCariSecilmeli"] = "Geçerli bir cari seçilmelidir.",
            ["Val_GecerliFirmaSecilmeli"] = "Geçerli bir firma seçilmelidir.",
            ["Val_BorcNegatifOlamaz"] = "Borç tutarı negatif olamaz.",
            ["Val_AlacakNegatifOlamaz"] = "Alacak tutarı negatif olamaz.",
            ["Val_TutarSifirdanBuyukOlmali"] = "İşlem tutarı sıfırdan büyük olmalıdır.",
            ["Val_AciklamaMaks250"] = "Açıklama en fazla 250 karakter olabilir.",
            ["Val_FirmaKoduBosOlamaz"] = "Firma kodu boş olamaz.",
            ["Val_FirmaKoduMaks30"] = "Firma kodu en fazla 30 karakter olabilir.",
            ["Val_FirmaUnvanBosOlamaz"] = "Firma unvanı boş olamaz.",
            ["Val_FirmaUnvanMaks150"] = "Firma unvanı en fazla 150 karakter olabilir.",
            ["Val_FirmaBorcLimitiNegatifOlamaz"] = "Firma borç limiti negatif olamaz.",
            ["Msg_CariKoduZatenMevcut"] = "'{0}' kodlu bir cari zaten mevcut.",
            ["Msg_CariKoduBaskaMevcut"] = "'{0}' kodlu bir başka cari mevcut.",
            ["Msg_CariKayitHata"] = "Cari kaydedilirken hata oluştu: {0}",
            ["Msg_CariSilHata"] = "Cari silinemedi: {0}",
            ["Msg_FisKayitHata"] = "Fiş kaydedilirken hata oluştu: {0}",
            ["Msg_FisSilHata"] = "Fiş silinemedi: {0}",
            ["Msg_GecersizBilgi"] = "Geçersiz Bilgi",
            ["Title_DogrulamaUyarisi"] = "Doğrulama Uyarısı",
            ["Title_CariSil"] = "Cari Sil",
            ["Title_FisSil"] = "Fiş Sil",
            ["Title_Makbuz"] = "Makbuz",
            ["Msg_MakbuzYazdirilsinMi"] = "Makbuz yazdırılsın mı?",
            ["Msg_FisKaydedildi"] = "{0} kaydedildi.",
            ["Msg_FisGuncellendi"] = "Fiş güncellendi.",
            ["Msg_FisSilindi"] = "Fiş silindi.",
            ["Msg_CariSilindi"] = "Cari silindi: {0}",
            ["Msg_CariGuncellendi"] = "Cari güncellendi: {0}",
            ["Msg_CariEklendi"] = "Cari eklendi: {0}",

            // Risk Limiti & Onaylar
            ["Title_RiskLimiti"] = "Risk Limiti Aşılıyor",
            ["Msg_RiskLimitiAsimi"] = "Bu işlemle bakiye {0} olacak ve risk limitini ({1}) aşacak.",
            ["Msg_YinedeKaydedilsinMi"] = "Yine de kaydedilsin mi?",
            ["Msg_SilmeOnay"] = "Seçili kaydı silmek istediğinize emin misiniz?",
            ["Msg_SilmeBasarisiz"] = "Silme işlemi başarısız.",
            ["Msg_CariSilmeDetay"] = "{0} kartı ve tüm hareketleri silinecek.",
            ["Msg_CariKartiYokUyarisi"] = "Cari kartı yok — F3 ile ekleyin ya da ⚙ menüsünden demo veri yükleyin.",

            // Fiş Durum ve Hataları
            ["Msg_FisKaydedilemedi"] = "Fiş kaydedilemedi.",
            ["Msg_FisGuncellenemedi"] = "Fiş güncellenemedi.",
            ["Status_FisKaydedildi"] = "✔ {0} kaydedildi: {1}",
            ["Status_FisGuncellendi"] = "✏ Fiş güncellendi.",
            ["Status_FisSilindi"] = "🗑 Fiş silindi.",
            ["Status_CariEklendi"] = "➕ Cari eklendi: {0}",
            ["Status_CariGuncellendi"] = "✏ Cari güncellendi: {0}",
            ["Status_CariSilindi"] = "🗑 Cari silindi: {0}",
            ["Status_HareketlerCari"] = "📒 Hareketler — {0}",

            // Yedekleme, Demo & Devir
            ["Title_YedekAlindi"] = "Yedek Alındı",
            ["Msg_YedeklemeBasarili"] = "Yedekleme tamamlandı.",
            ["Msg_YedeklemeBasarisiz"] = "Yedekleme başarısız.",
            ["Status_YedekAlindi"] = "💾 Yedek alındı: {0}",
            ["Title_DemoVeri"] = "Demo Veri",
            ["Msg_DemoOnay"] = "25 adet örnek cari ve hareketleri eklenecek. Devam edilsin mi?",
            ["Msg_DemoEklendi"] = "{0} örnek cari eklendi.",
            ["Msg_DemoEklenemedi"] = "Demo veri eklenemedi.",
            ["Title_YilSonuDevir"] = "Yıl Sonu Devir",
            ["Msg_DevirOnay"] = "{0} dönemi bakiyeleri, {1} dönemi için yeni firmaya devir fişi olarak aktarılacak.",
            ["Msg_DevirDevamMi"] = "Devam edilsin mi?",
            ["Msg_DevirBasarili"] = "Yıl sonu devir işlemi başarıyla tamamlandı.",
            ["Msg_DevirBasarisiz"] = "Devir işlemi başarısız.",
            ["Msg_HesapMakinesiAcilamadi"] = "Hesap makinesi açılamadı.",

            // Sağ Tık Menüleri & Yazdırma Alt Menüsü
            ["Ctx_YeniCari"] = "➕ Yeni Cari (F3)",
            ["Ctx_Duzenle"] = "✏ Düzenle (F4)",
            ["Ctx_Sil"] = "🗑 Sil (Del)",
            ["Ctx_Hareketler"] = "📒 Hareketler (Enter)",
            ["Ctx_Yazdir"] = "🖨 Yazdır…",
            ["Ctx_Yenile"] = "🔄 Yenile",
            ["Ctx_HesabaBorcEkle"] = "F5 — Hesaba Borç Ekle",
            ["Ctx_TahsilatYap"] = "F6 — Tahsilat Yap",
            ["Ctx_HesabaAlacakEkle"] = "F7 — Hesaba Alacak Ekle",
            ["Ctx_OdemeYap"] = "F8 — Ödeme Yap",
            ["Menu_CariHesapEkstresi"] = "📑 Cari Hesap Ekstresi",
            ["Menu_SeciliFisMakbuzu"] = "🧾 Seçili Fiş Makbuzu",

            // Kur Durumu
            ["Status_KurGuncelleniyor"] = "💱 Güncelleniyor…",
            ["Status_KurAlinamadi"] = "💱 Kur alınamadı (tıkla)",

            // YedekYonetimiWindow
            ["Msg_YeniYedekAlindi"] = "Yeni veritabanı yedeği başarıyla alındı.",
            ["Title_YedekGeriYuklemeOnayi"] = "Yedek Geri Yükleme Onayı",
            ["Msg_YedekGeriYuklemeOnay"] = "Seçili yedek (\"{0}\") geri yüklenecek.\nMevcut veritabanının üzerine yazılacaktır.\n\nDevam etmek istiyor musunuz?",
            ["Title_GeriYuklemeBasarili"] = "Geri Yükleme Başarılı",
            ["Msg_VeritabaniGeriYuklendi"] = "Veritabanı başarıyla geri yüklendi!",
            ["Msg_GeriYuklemeBasarisiz"] = "Geri yükleme başarısız oldu.",
            ["Title_YedekSilmeOnayi"] = "Yedek Silme Onayı",
            ["Msg_YedekSilmeOnay"] = "Bu yedek dosyası kalıcı olarak silinecek:\n{0}\n\nEmin misiniz?",
            ["Msg_DosyaSilinemedi"] = "Dosya silinemedi.",

            // LoginWindow
            ["Title_GirisEngellendi"] = "Giriş Engellendi",
            ["Msg_GirisEngellendi"] = "3 defa hatalı şifre girdiniz. Program kapatılıyor.",
            ["Title_HataliSifre"] = "Hatalı Şifre",
            ["Msg_HataliSifre"] = "Hatalı şifre girdiniz! Lütfen tekrar deneyiniz.",

            // DekontEditWindow
            ["Title_GecersizTutar"] = "Geçersiz Tutar",
            ["Msg_TutarSifirOlamaz"] = "İşlem tutarı 0'dan büyük olmalıdır!",

            // AyarlarWindow
            ["Title_SifreGuncelleme"] = "Şifre Güncelleme",
            ["Msg_MevcutSifreHatali"] = "Mevcut program şifreniz hatalı! (Varsayılan: gel)",
            ["Msg_YeniSifreBosOlamaz"] = "Yeni şifre boş bırakılamaz!",
            ["Msg_YeniSifreUyusmuyor"] = "Yeni şifre ve tekrarı uyuşmuyor!",
            ["Title_SifreGuncellendi"] = "Şifre Güncellendi",
            ["Msg_SifreBasariylaGuncellendi"] = "Program şifreniz başarıyla güncellendi.",
            ["Title_AciklamalariSil"] = "Açıklamaları Sil",
            ["Msg_AciklamalariSilOnay"] = "Bu kategorideki tüm açıklamalar silinecek. Emin misiniz?",
            ["Msg_AyarlarKaydedilemedi"] = "Ayarlar kaydedilemedi."
        };

        // ══════════════════════ İNGİLİZCE (English) ══════════════════════
        var en = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // General & Title
            ["App_Title"] = "Gelesiye — Simple Account & Debt Tracker",
            ["App_Motto"] = "  |  Simple Account & Debt Tracker",
            ["Firma_Demo"] = "DEMO TRADING CO.",
            ["ParaBirimi_TL"] = "USD",

            // Windows & Titles
            ["Title_Ayarlar"] = "⚙  Application & Company Settings",
            ["Title_YedekYonetimi"] = "💾  Backup & Restore Management",
            ["Title_CariKarti"] = "Account Card",
            ["Title_YeniCari"] = "Add New Account",
            ["Title_CariDuzenle"] = "Edit Account",
            ["Title_Fis"] = "Voucher",
            ["Title_FisDuzenle"] = "Edit Voucher",
            ["Title_Giris"] = "GELESIYE — Login",
            ["Title_CikisOnayi"] = "Gelesiye — Exit Confirmation",
            ["Title_ListeToplamlari"] = "∑  List Totals",
            ["Title_RaporSecimi"] = "🖨  Print & Report Selection",
            ["Title_Hakkinda"] = "About",
            ["Title_Uyari"] = "Warning",
            ["Title_Hata"] = "Error",
            ["Title_Bilgi"] = "Information",
            ["Title_Onay"] = "Confirmation",

            // Buttons & Actions
            ["Btn_Ekle"] = "New Account (F3)",
            ["Btn_Duzenle"] = "Edit (F4)",
            ["Btn_Sil"] = "Delete (Del)",
            ["Btn_Yazdir"] = "Print…",
            ["Btn_Ozet"] = "Summary (F2)",
            ["Btn_Hareketler"] = "Transactions ➔",
            ["Btn_CariListesi"] = "⬅ Account List",
            ["Btn_Fisler"] = "➕ New Voucher…",
            ["Btn_Kaydet"] = "💾 Save",
            ["Btn_AyarlariKaydet"] = "💾 Save Settings",
            ["Btn_Iptal"] = "Cancel",
            ["Btn_IptalEsc"] = "Cancel (Esc)",
            ["Btn_KaydetF2"] = "Save (F2)",
            ["Btn_Kapat"] = "Close",
            ["Btn_Tamam"] = "OK",
            ["Btn_Hayir"] = "No",
            ["Btn_Evet"] = "Yes",
            ["Btn_Sec"] = "Browse…",
            ["Btn_Varsayilan"] = "Default",
            ["Btn_SecileniSil"] = "🗑 Delete Selected",
            ["Btn_TumunuSil"] = "Delete All",
            ["Btn_SimdiYedekle"] = "💾 Backup Now",
            ["Btn_GeriYukle"] = "↩ Restore Selected",
            ["Btn_KlasordeGoster"] = "📂 Open in Folder",
            ["Btn_Giris"] = "Log In ➔",
            ["Btn_Cikis"] = "Exit",
            ["Btn_SifreGuncelle"] = "🔑 Update Password",
            ["Btn_SimgeyeKucult"] = "Minimize to Tray",
            ["Btn_TamamenCik"] = "Exit Application",
            ["Btn_HesapMakinesi"] = "🧮 Calculator",
            ["Btn_MesajAt"] = "Send Message",
            ["Btn_ProfiliAc"] = "Open Profile",
            ["Btn_SayfayiAc"] = "Open Page",

            // Tooltips
            ["Tooltip_Menu"] = "Options and Operations Menu",
            ["Tooltip_Minimize"] = "Minimize",
            ["Tooltip_Maximize"] = "Maximize / Restore",
            ["Tooltip_Close"] = "Exit Application",
            ["Tooltip_OncekiCari"] = "Previous Account (← / Alt+←)",
            ["Tooltip_SonrakiCari"] = "Next Account (→ / Alt+→)",
            ["Tooltip_HesapMakinesi"] = "Calculator",
            ["Tooltip_AramaYontemi"] = "Search Method",
            ["Tooltip_HesapFiltresi"] = "Account Status Filter",
            ["Tooltip_BakiyeFiltresi"] = "Balance Filter",

            // Menu Items
            ["Menu_Ayarlar"] = "⚙  Settings…",
            ["Menu_YedekYonetimi"] = "💾  Backup Manager…",
            ["Menu_ListeToplamlari"] = "∑  List Totals (F2)",
            ["Menu_RaporSecimi"] = "🖨  Report Selection…",
            ["Menu_HizliYedek"] = "💾  Quick Backup",
            ["Menu_DemoYukle"] = "🧪  Load Demo Data (25 accounts)",
            ["Menu_YilSonuDevir"] = "📅  Year-End Roll-over…",
            ["Menu_Hakkinda"] = "ℹ  About",
            ["Menu_Cikis"] = "✕  Exit",

            // Voucher Types
            ["Fis_Borc"] = "F5 — Add Debit (Debit Voucher)",
            ["Fis_Tahsilat"] = "F6 — Collection",
            ["Fis_Alacak"] = "F7 — Add Credit (Credit Voucher)",
            ["Fis_Odeme"] = "F8 — Make Payment",
            ["Fis_Devir"] = "Transfer Voucher",
            ["Fis_Borc_Kisa"] = "Debit Voucher",
            ["Fis_Tahsilat_Kisa"] = "Collection",
            ["Fis_Alacak_Kisa"] = "Credit Voucher",
            ["Fis_Odeme_Kisa"] = "Payment",
            ["Fis_Devir_Kisa"] = "Transfer Voucher",

            // Cards & Statistics
            ["Stat_ToplamBorc"] = "TOTAL RECEIVABLE (DEBTORS)",
            ["Stat_ToplamAlacak"] = "TOTAL PAYABLE (CREDITORS)",
            ["Stat_NetBakiye"] = "NET BALANCE",
            ["Stat_MusteriSayisi"] = "TOTAL CLIENTS",
            ["Stat_IslemSayisi"] = "TRANSACTIONS",
            ["Stat_Bakiye"] = "BALANCE",
            ["Stat_Durum"] = "STATUS",
            ["Stat_BorcluCariSub"] = "Active debtor accounts",
            ["Stat_AlacakliCariSub"] = "Active creditor accounts",
            ["Stat_NetDurumSub"] = "General net balance",
            ["Stat_BorcGirisleri"] = "Debit entries",
            ["Stat_AlacakGirisleri"] = "Credit entries",
            ["Stat_NetDurum"] = "Net balance",
            ["Stat_HareketKaydi"] = "Transaction records",
            ["Stat_Aktif"] = "Active",
            ["Stat_Pasif"] = "Inactive",
            ["Stat_Borclu"] = "Debtors",
            ["Stat_Alacakli"] = "Creditors",
            ["Stat_Sifir"] = "Zero",

            // Columns
            ["Col_CariKodu"] = "Code",
            ["Col_Unvan"] = "Company / Client Name",
            ["Col_Telefon"] = "Phone / GSM",
            ["Col_IlIlce"] = "City / District",
            ["Col_Borc"] = "Debit",
            ["Col_Alacak"] = "Credit",
            ["Col_Bakiye"] = "Balance",
            ["Col_Tarih"] = "Date",
            ["Col_Saat"] = "Time",
            ["Col_EvrakNo"] = "Doc No",
            ["Col_IslemTuru"] = "Type",
            ["Col_Aciklama"] = "Description",
            ["Col_DosyaAdi"] = "File Name",
            ["Col_Boyut"] = "Size",
            ["Col_TamYol"] = "Full Path",
            ["Col_SonIslem"] = "Last Transaction",
            ["Col_YuruyenBakiye"] = "Running Balance",

            // Filters
            ["Filter_Arama"] = "Search:",
            ["Filter_AramaPlaceholder"] = "Search by account name or code…",
            ["Filter_ModYazildigi"] = "Starts With (Exact)",
            ["Filter_ModIceren"] = "Contains (Anywhere)",
            ["Filter_HesapTumu"] = "All Accounts",
            ["Filter_HesapAktif"] = "Active Only",
            ["Filter_HesapPasif"] = "Inactive Only",
            ["Filter_BakiyeTumu"] = "All Balances",
            ["Filter_BakiyeBorclular"] = "Debtors Only (>0)",
            ["Filter_BakiyeAlacaklilar"] = "Creditors Only (<0)",
            ["Filter_BakiyeSifir"] = "Zero Balance",
            ["Filter_BakiyeSifirOlmayan"] = "Non-zero Balance",
            ["Filter_LimitAsanlar"] = "⚠ Over Limit",

            // Settings Tabs
            ["Tab_Firma"] = "Company Details",
            ["Tab_Genel"] = "General",
            ["Tab_Yedekleme"] = "Backup",
            ["Tab_Sifre"] = "Change Password",
            ["Tab_Aciklamalar"] = "Templates",
            ["Tab_CariBilgileri"] = "Account Details",
            ["Tab_OzelNotlar"] = "Private Notes",

            // Form Labels
            ["Lbl_FirmaUnvani"] = "COMPANY NAME",
            ["Lbl_CariKodu"] = "ACCOUNT CODE",
            ["Lbl_CariUnvan"] = "ACCOUNT NAME",
            ["Lbl_Telefon"] = "PHONE",
            ["Lbl_Gsm"] = "GSM",
            ["Lbl_Faks"] = "FAX",
            ["Lbl_Il"] = "CITY",
            ["Lbl_Ilce"] = "DISTRICT",
            ["Lbl_VergiDairesi"] = "TAX OFFICE",
            ["Lbl_VergiNo"] = "TAX / ID NO",
            ["Lbl_Adres"] = "ADDRESS",
            ["Lbl_ParaBirimi"] = "CURRENCY",
            ["Lbl_Dil"] = "APPLICATION LANGUAGE",
            ["Lbl_KapatmaEylemi"] = "CLOSE ACTION (When window is closed)",
            ["Lbl_KapatmaSor"] = "0 — Always ask",
            ["Lbl_KapatmaCik"] = "1 — Exit application immediately",
            ["Lbl_KapatmaKucult"] = "2 — Minimize to system tray (clock area)",
            ["Lbl_SifreKorumasi"] = "Enable password protection on startup",
            ["Lbl_GorunumVeListeler"] = "APPEARANCE & LISTS",
            ["Lbl_GuvenlikGiris"] = "SECURITY & LOGIN",
            ["Lbl_ZebraGrid"] = "Enable alternating row colors (Zebra striping) in lists",
            ["Lbl_FiltreVarsayilanlari"] = "SEARCH & LIST FILTERS",
            ["Lbl_VarsayilanAramaModu"] = "Default Search Mode:",
            ["Lbl_VarsayilanHesapDurumu"] = "Default Account Status:",
            ["Lbl_VarsayilanBakiyeFiltresi"] = "Default Balance Filter:",
            ["Lbl_FiltreHatirla"] = "Remember last used filters and restore on startup",
            ["Lbl_OtoYedekleme"] = "AUTOMATIC BACKUP CYCLE",
            ["Lbl_MaksYedek"] = "MAX BACKUPS TO RETAIN",
            ["Lbl_YedekKlasoru"] = "BACKUP DIRECTORY",
            ["Lbl_MevcutSifre"] = "CURRENT PASSWORD",
            ["Lbl_YeniSifre"] = "NEW PASSWORD",
            ["Lbl_YeniSifreTekrar"] = "CONFIRM NEW PASSWORD",
            ["Lbl_IslemTuru"] = "TRANSACTION TYPE:",
            ["Lbl_Tutar"] = "AMOUNT",
            ["Lbl_Aciklama"] = "DESCRIPTION",
            ["Lbl_RiskLimiti"] = "CREDIT RISK LIMIT",
            ["Lbl_Yetkili"] = "CONTACT PERSON",
            ["Lbl_Eposta"] = "E-MAIL",
            ["Lbl_Web"] = "WEBSITE",
            ["Lbl_KayitTarihi"] = "REGISTRATION DATE",
            ["Lbl_HesapDurumu"] = "ACCOUNT STATUS",
            ["Lbl_Notlar"] = "PRIVATE NOTES",
            ["Lbl_Tarih"] = "DATE",
            ["Lbl_Saat"] = "TIME",
            ["Lbl_CariHesap"] = "ACCOUNT",
            ["Opt_Aktif"] = "Active",
            ["Opt_Pasif"] = "Inactive",

            // Reports & Totals & Print
            ["Title_BaskiOnizleme"] = "🖨  Print Preview & Print",
            ["Lbl_RaporSeciniz"] = "Please select the report you want to print or preview:",
            ["Btn_RaporuAc"] = "🖨 Open Report ➔",
            ["Btn_RaporuGoruntule"] = "🖨 View Report",
            ["Btn_YazdirOnizle"] = "🖨  Print (Ctrl+P)",
            ["Btn_TekSayfa"] = "📄 Single Page",
            ["Btn_CiftSayfa"] = "📖 Two Pages",
            ["Btn_SurekliKaydirma"] = "📜 Continuous",
            ["Btn_Yakinlastir"] = "🔍+ Zoom In",
            ["Btn_Uzaklastir"] = "🔍− Zoom Out",
            ["Btn_YuzdeYuz"] = "100%",
            ["Tooltip_Yazdir"] = "Send document to printer or save as PDF (Ctrl+P)",
            ["Tooltip_TekSayfa"] = "Single page layout",
            ["Tooltip_CiftSayfa"] = "Side-by-side two page layout",
            ["Tooltip_Surekli"] = "Continuous vertical scroll layout",
            ["Tooltip_Yakinlastir"] = "Zoom in (Ctrl +)",
            ["Tooltip_Uzaklastir"] = "Zoom out (Ctrl -)",
            ["Tooltip_YuzdeYuz"] = "Reset zoom to 100%",
            ["Msg_YazdirmaHatasi"] = "An error occurred while printing:\n{0}",
            ["Rpr_SeciliEkstre"] = "Selected Account Statement",
            ["Rpr_SeciliEkstreSub"] = "Account statement & transaction list",
            ["Rpr_CariListesi"] = "General Account List & Balances",
            ["Rpr_CariListesiSub"] = "All clients and current debit/credit balances",
            ["Rpr_GenelHareketler"] = "General Transaction Log (All Vouchers)",
            ["Rpr_GenelHareketlerSub"] = "Chronological log of all transactions across all accounts",
            ["Lbl_CariHesapBakiyeToplamlari"] = "ACCOUNT BALANCE TOTALS",
            ["Lbl_ToplamBorc"] = "TOTAL RECEIVABLE",
            ["Lbl_ToplamAlacak"] = "TOTAL PAYABLE",
            ["Lbl_NetBakiye"] = "NET BALANCE",
            ["Lbl_KayitSayilari"] = "RECORD COUNTS & STATISTICS",
            ["Lbl_ToplamCari"] = "TOTAL ACCOUNTS",
            ["Lbl_AktifCari"] = "ACTIVE ACCOUNTS",
            ["Lbl_PasifCari"] = "INACTIVE ACCOUNTS",
            ["Lbl_BorcluCari"] = "DEBTOR ACCOUNTS",
            ["Lbl_AlacakliCari"] = "CREDITOR ACCOUNTS",
            ["Lbl_SifirBakiye"] = "ZERO BALANCE",

            // Close Confirmation & Login
            ["Msg_KapatmakIstiyorMusunuz"] = "Do you want to close the application?",
            ["Msg_EylemiSeciniz"] = "Please select the action you want to take:",
            ["Lbl_SecimiHatirla"] = "Remember my choice and don't ask again",
            ["Lbl_SifreGiriniz"] = "Please enter your password to enter the application.",
            ["Lbl_KalanHak"] = "Remaining Attempts: {0}",
            ["Status_CariListesi"] = "📋 Account List",
            ["Status_HareketDetayi"] = "📊 Transaction Details",

            // Developer & About
            ["Lbl_Gelistirici"] = "DEVELOPER",
            ["Lbl_Slogan"] = "\"Trust on credit and you wait till whenever,\nif you sold on credit, Gelesiye is clever!\"",
            ["Lbl_Versiyon"] = "Version {0} — 2026",

            // Messages & Dialogs
            ["Msg_SilOnay"] = "Selected client account and all related transactions will be deleted. Are you sure?",
            ["Msg_SilBaslik"] = "Delete Account",
            ["Msg_FisSilOnay"] = "Selected voucher will be deleted. Are you sure?",
            ["Msg_KayitBasarili"] = "Operation saved successfully.",
            ["Msg_SifreGuncellendi"] = "Password updated successfully.",
            ["Msg_HataSifre"] = "Current password is incorrect!",
            ["Msg_HataSifreEslesmedi"] = "New passwords do not match!",
            ["Msg_TrayBilgi"] = "Application continues running in the system tray. Double-click the icon to reopen.",
            ["Tray_Goster"] = "📋  Show (Open Main Window)",
            ["Tray_YedekAl"] = "💾  Quick Backup",
            ["Tray_YedekYonetimi"] = "💾  Backup Manager…",
            ["Tray_Hakkinda"] = "ℹ  About",
            ["Tray_Cikis"] = "✕  Exit Application",

            // Validation and Service Messages
            ["Val_CariKoduBosOlamaz"] = "Account code cannot be empty.",
            ["Val_CariKoduMaks50"] = "Account code cannot exceed 50 characters.",
            ["Val_UnvanBosOlamaz"] = "Account title cannot be empty.",
            ["Val_UnvanMaks150"] = "Account title cannot exceed 150 characters.",
            ["Val_LimitNegatifOlamaz"] = "Credit risk limit cannot be negative.",
            ["Val_GecersizEposta"] = "Invalid e-mail address format.",
            ["Val_TelefonMaks30"] = "Phone number cannot exceed 30 characters.",
            ["Val_GsmMaks30"] = "Mobile number cannot exceed 30 characters.",
            ["Val_VergiNoMaks20"] = "Tax number cannot exceed 20 characters.",
            ["Val_GecerliCariSecilmeli"] = "A valid account must be selected.",
            ["Val_GecerliFirmaSecilmeli"] = "A valid company must be selected.",
            ["Val_BorcNegatifOlamaz"] = "Debit amount cannot be negative.",
            ["Val_AlacakNegatifOlamaz"] = "Credit amount cannot be negative.",
            ["Val_TutarSifirdanBuyukOlmali"] = "Transaction amount must be greater than zero.",
            ["Val_AciklamaMaks250"] = "Description cannot exceed 250 characters.",
            ["Val_FirmaKoduBosOlamaz"] = "Company code cannot be empty.",
            ["Val_FirmaKoduMaks30"] = "Company code cannot exceed 30 characters.",
            ["Val_FirmaUnvanBosOlamaz"] = "Company title cannot be empty.",
            ["Val_FirmaUnvanMaks150"] = "Company title cannot exceed 150 characters.",
            ["Val_FirmaBorcLimitiNegatifOlamaz"] = "Company credit limit cannot be negative.",
            ["Msg_CariKoduZatenMevcut"] = "An account with code '{0}' already exists.",
            ["Msg_CariKoduBaskaMevcut"] = "Another account with code '{0}' already exists.",
            ["Msg_CariKayitHata"] = "Error saving account: {0}",
            ["Msg_CariSilHata"] = "Could not delete account: {0}",
            ["Msg_FisKayitHata"] = "Error saving voucher: {0}",
            ["Msg_FisSilHata"] = "Could not delete voucher: {0}",
            ["Msg_GecersizBilgi"] = "Invalid Information",
            ["Title_DogrulamaUyarisi"] = "Validation Warning",
            ["Title_CariSil"] = "Delete Account",
            ["Title_FisSil"] = "Delete Voucher",
            ["Title_Makbuz"] = "Receipt",
            ["Msg_MakbuzYazdirilsinMi"] = "Print receipt?",
            ["Msg_FisKaydedildi"] = "{0} saved.",
            ["Msg_FisGuncellendi"] = "Voucher updated.",
            ["Msg_FisSilindi"] = "Voucher deleted.",
            ["Msg_CariSilindi"] = "Account deleted: {0}",
            ["Msg_CariGuncellendi"] = "Account updated: {0}",
            ["Msg_CariEklendi"] = "Account added: {0}",

            // Risk Limiti & Onaylar
            ["Title_RiskLimiti"] = "Risk Limit Exceeded",
            ["Msg_RiskLimitiAsimi"] = "With this transaction, balance will be {0} and exceed risk limit ({1}).",
            ["Msg_YinedeKaydedilsinMi"] = "Do you still want to save?",
            ["Msg_SilmeOnay"] = "Are you sure you want to delete the selected record?",
            ["Msg_SilmeBasarisiz"] = "Delete operation failed.",
            ["Msg_CariSilmeDetay"] = "{0} account and all its transactions will be deleted.",
            ["Msg_CariKartiYokUyarisi"] = "No account found — Add with F3 or load demo data from ⚙ menu.",

            // Fiş Durum ve Hataları
            ["Msg_FisKaydedilemedi"] = "Voucher could not be saved.",
            ["Msg_FisGuncellenemedi"] = "Voucher could not be updated.",
            ["Status_FisKaydedildi"] = "✔ {0} saved: {1}",
            ["Status_FisGuncellendi"] = "✏ Voucher updated.",
            ["Status_FisSilindi"] = "🗑 Voucher deleted.",
            ["Status_CariEklendi"] = "➕ Account added: {0}",
            ["Status_CariGuncellendi"] = "✏ Account updated: {0}",
            ["Status_CariSilindi"] = "🗑 Account deleted: {0}",
            ["Status_HareketlerCari"] = "📒 Transactions — {0}",

            // Yedekleme, Demo & Devir
            ["Title_YedekAlindi"] = "Backup Created",
            ["Msg_YedeklemeBasarili"] = "Backup completed.",
            ["Msg_YedeklemeBasarisiz"] = "Backup failed.",
            ["Status_YedekAlindi"] = "💾 Backup created: {0}",
            ["Title_DemoVeri"] = "Demo Data",
            ["Msg_DemoOnay"] = "25 sample accounts and transactions will be added. Continue?",
            ["Msg_DemoEklendi"] = "{0} sample accounts added.",
            ["Msg_DemoEklenemedi"] = "Demo data could not be added.",
            ["Title_YilSonuDevir"] = "Year-End Transfer",
            ["Msg_DevirOnay"] = "Period {0} balances will be transferred to new company for period {1}.",
            ["Msg_DevirDevamMi"] = "Continue?",
            ["Msg_DevirBasarili"] = "Year-end transfer completed successfully.",
            ["Msg_DevirBasarisiz"] = "Transfer failed.",
            ["Msg_HesapMakinesiAcilamadi"] = "Calculator could not be opened.",

            // Sağ Tık Menüleri & Yazdırma Alt Menüsü
            ["Ctx_YeniCari"] = "➕ New Account (F3)",
            ["Ctx_Duzenle"] = "✏ Edit (F4)",
            ["Ctx_Sil"] = "🗑 Delete (Del)",
            ["Ctx_Hareketler"] = "📒 Transactions (Enter)",
            ["Ctx_Yazdir"] = "🖨 Print…",
            ["Ctx_Yenile"] = "🔄 Refresh",
            ["Ctx_HesabaBorcEkle"] = "F5 — Add Debit to Account",
            ["Ctx_TahsilatYap"] = "F6 — Receive Payment",
            ["Ctx_HesabaAlacakEkle"] = "F7 — Add Credit to Account",
            ["Ctx_OdemeYap"] = "F8 — Make Payment",
            ["Menu_CariHesapEkstresi"] = "📑 Account Statement",
            ["Menu_SeciliFisMakbuzu"] = "🧾 Selected Voucher Receipt",

            // Kur Durumu
            ["Status_KurGuncelleniyor"] = "💱 Updating…",
            ["Status_KurAlinamadi"] = "💱 Rate unavailable (click)",

            // YedekYonetimiWindow
            ["Msg_YeniYedekAlindi"] = "New database backup created successfully.",
            ["Title_YedekGeriYuklemeOnayi"] = "Backup Restore Confirmation",
            ["Msg_YedekGeriYuklemeOnay"] = "Selected backup (\"{0}\") will be restored.\nCurrent database will be overwritten.\n\nDo you want to continue?",
            ["Title_GeriYuklemeBasarili"] = "Restore Successful",
            ["Msg_VeritabaniGeriYuklendi"] = "Database restored successfully!",
            ["Msg_GeriYuklemeBasarisiz"] = "Restore failed.",
            ["Title_YedekSilmeOnayi"] = "Delete Backup Confirmation",
            ["Msg_YedekSilmeOnay"] = "This backup file will be permanently deleted:\n{0}\n\nAre you sure?",
            ["Msg_DosyaSilinemedi"] = "File could not be deleted.",

            // LoginWindow
            ["Title_GirisEngellendi"] = "Access Denied",
            ["Msg_GirisEngellendi"] = "Incorrect password entered 3 times. Application is closing.",
            ["Title_HataliSifre"] = "Incorrect Password",
            ["Msg_HataliSifre"] = "Incorrect password! Please try again.",

            // DekontEditWindow
            ["Title_GecersizTutar"] = "Invalid Amount",
            ["Msg_TutarSifirOlamaz"] = "Transaction amount must be greater than 0!",

            // AyarlarWindow
            ["Title_SifreGuncelleme"] = "Password Update",
            ["Msg_MevcutSifreHatali"] = "Current password is incorrect! (Default: gel)",
            ["Msg_YeniSifreBosOlamaz"] = "New password cannot be empty!",
            ["Msg_YeniSifreUyusmuyor"] = "New password confirmation does not match!",
            ["Title_SifreGuncellendi"] = "Password Updated",
            ["Msg_SifreBasariylaGuncellendi"] = "Password updated successfully.",
            ["Title_AciklamalariSil"] = "Delete Templates",
            ["Msg_AciklamalariSilOnay"] = "All templates in this category will be deleted. Are you sure?",
            ["Msg_AyarlarKaydedilemedi"] = "Settings could not be saved."
        };

        _translations["tr-TR"] = tr;
        _translations["en-US"] = en;
    }
}
