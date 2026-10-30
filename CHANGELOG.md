# Değişiklik Günlüğü (Changelog)

Bu projedeki tüm önemli değişiklikler bu dosyada belgelenmektedir.

Format [Keep a Changelog](https://keepachangelog.com/tr/1.0.0/) standardına dayanmakta olup, sürümleme [SemVer](https://semver.org/lang/tr/) kurallarına uygundur.

---

## [1.0.0] - 2026-10-28

### Eklendi
- **Modern .NET 10 & WPF Mimarisi:**
  - `Microsoft.Extensions.DependencyInjection` tabanlı IoC ve servis konteyneri yapılandırması.
  - `CommunityToolkit.Mvvm` ile tam reaktif MVVM (Model-View-ViewModel) deseni.
  - Bağımsız, hafif ve sıfır yapılandırmalı yerel SQLite veritabanı motoru (`Microsoft.Data.Sqlite.Core`).
  - `Dapper` (Micro-ORM) entegrasyonu ile veri erişim katmanında tip güvenli, yüksek performanslı nesne haritalama.
  - `FluentValidation` ve merkezi CRUD iş servisleri katmanı (`ICariService`, `IHareketService`, `ServiceResult`): UI'dan bağımsız doğrulama kuralları, mükerrer kod denetimi ve merkezi loglama.
  - Doğrulama ve servis katmanlarında %100 dinamik çoklu dil (i18n) desteği: FluentValidation kuralları ve servis hata iletileri `tr-TR` ve `en-US` sözlüklerine bağlandı.
  - Uygulama genelinde kapsamlı yerelleştirme (i18n) denetimi: Tüm hardcoded stringler temizlenerek `LocalizationManager` sistemine bağlandı (risk limiti aşımları, fiş ve cari CRUD durum bildirimleri, sağ tık bağlam menüleri, yedekleme ve parola diyalogları).
  - Ekran görüntüleriyle zenginleştirilmiş kapsamlı Markdown Kullanım Kılavuzu (`docs/KILAVUZ.md`) ve WPF uygulamasından otomatik çekilen güncel ekran görüntüleri (`docs/screenshots/`).

- **Cari Hesap Yönetimi:**
  - 81 İl ve İlçe hiyerarşik seçimi ile cari kart oluşturma ve düzenleme ekranı (`CariEditWindow`).
  - Çift modlu arama algoritması: *"İçeren"* ve *"Yazıldığı Gibi"* anlık arama.
  - Hesap durumu filtreleri: *Tümü*, *Sadece Aktifler*, *Sadece Pasifler*.
  - Bakiye filtreleri: *Borçlular*, *Alacaklılar*, *Bakiyesi Sıfır Olanlar*, *Bakiyesi Sıfır Olmayanlar*.
  - Dinamik F2 Liste Toplamları ve anlık finansal özet kartları.
  - Ayarlar üzerinden varsayılan filtreleri belirleme ve *"Son kullanılan filtreleri hatırla"* desteği.

- **Cari Hareket & Fiş İşlemleri:**
  - 4 temel fiş türü: Borç Dekontu (`F5`), Tahsilat Fişi (`F6`), Alacak Dekontu (`F7`), Ödeme Fişi (`F8`).
  - Yıl sonu ve ara dönemler için Devir Fişi desteği.
  - Kategoriye göre filtrelenebilir Açıklama Şablonları kütüphanesi.
  - Satır bazında dinamik yürüyen bakiye hesaplama.
  - `Alt + Sol / Sağ` klavye kısayolu ile cariler arası seri geçiş desteği.

- **Yazdırma ve Raporlama Sistemi:**
  - Modern, kullanıcı dostu Baskı Önizleme ve Yazdırma Ekranı (`RaporOnizlemeWindow`):
    - Üst eylem çubuğunda belirgin **"🖨 Yazdır (Ctrl+P)"** butonu ile doğrudan Windows Yazdırma İletişim Kutusu (`PrintDialog`) entegrasyonu.
    - Tek sayfa, çift sayfa ve sürekli dikey kaydırma düzen modları (`FlowDocumentReaderViewingMode`).
    - Yakınlaştırma / uzaklaştırma ve %100 sıfırlama araçları (`Zoom`).
  - Geliştirilmiş Rapor Seçim Penceresi (`RaporSecimWindow`):
    - Her rapor kartı üzerinde belirgin **"🖨 Raporu Aç ➔"** eylem rozeti.
    - Alt çubukta doğrudan görünür birincil **"🖨 Raporu Görüntüle"** butonu ve klavye Enter/Esc desteği.
  - Yüksek çözünürlüklü WPF `FlowDocument` tabanlı yazdırma altyapısı (`PrintService`).
  - İki tarih arası veya dönemsel Ayrıntılı Cari Hesap Ekstresi.
  - Tüm carileri kapsayan Cari Bakiye Özet Listesi.
  - Seri işlem ve makbuz onay kutusunda klavye ergonomisi iyileştirmesi: Fiş kaydı sonrası makbuz yazdırma onay kutusunda varsayılan seçim 'Hayır' olarak ayarlandı; `Enter` veya `Esc` tuşuyla yazdırmadan hızlıca çıkış imkanı sağlandı.
  - Dönem içindeki tüm işlemleri kronolojik listeleyen Genel Hareketler Raporu.
  - Sayısal tutarları otomatik olarak metne dönüştüren `TurkceYazi` çeviricisi.

- **Yedekleme ve Güvenlik Altyapısı:**
  - SQLite Online Backup API ile canlı, kesintisiz yedekleme motoru (`BackupService`).
  - Otomatik yedekleme döngüsü (kapanışta, açılışta veya periyodik) ve saklama adedi (retention) politikası.
  - Ayrıntılı Yedekleme Yönetimi penceresi (`YedekYonetimiWindow`): yedek listeleme, boyut takibi ve tek tıkla geri yükleme.
  - Ayarlanabilir şifreli giriş koruması (`LoginWindow`) ve 3 deneme limiti.

- **Firma & Program Ayarları:**
  - 5 sekmeli kapsamlı ayarlar penceresi (`AyarlarWindow`): *Firma Bilgileri*, *Genel*, *Yedekleme*, *Şifre Değiştir*, *Açıklama Şablonları*.
  - Kapatma eylemi seçenekleri: Her zaman sor, doğrudan çık, simge durumuna küçült.
  - Windows Bildirim Alanı (**System Tray / Saatin Yanı**) tam entegrasyonu: arka planda gizlenme, ilk küçültmede bilgi balonu bildirimi, simgeye çift tıklayarak pencereyi geri getirme ve sağ tık bağlam menüsü (`TrayService`).
  - Ayarlar Genel sekmesinde *"Listelerde ardışık satır renklendirmesi (Zebra stili) aktif olsun"* seçeneği (`ZebraGrid`) ile anlık açma/kapatma.

- **Arayüz ve Finansal Liste Deneyimi (Bakiye Renklendirme & Zebra Striping):**
  - Finansal bakiye ve işlem tutarları için yüksek kontrastlı dinamik renklendirme (`BakiyeBrushConverter`):
    - Borçlu hesaplar: `#D41A1A` (Kırmızı) ve `▲` artış göstergesi.
    - Alacaklı hesaplar: `#205E1B` (Koyu Yeşil) ve `▼` azalış göstergesi.
    - Sıfır bakiyeler: `#0F172A` (Koyu Lacivert) nötr gösterim.
  - Hareketler listesinde Borç kolonu daima kırmızı, Alacak kolonu daima yeşil ve Yürüyen Bakiye duruma göre renklendirilmiştir.
  - Tüm listelerde (`lvCariler`, `lvHareketler`, `lvYedekler`) donanım hızlandırmalı ardışık satır renklendirmesi (`AlternationCount="2"`, `ZebraListViewItem`).
  - Çift satırlarda `#FFFFFF`, tek satırlarda `#F1F5F9` (Slate-100) tonlaması; hover `#EEF2F6`, seçim `#E0F2FE` ve `#E2E8F0` alt sınır çizgisi.
  - Modern GridView başlık stili (`ListHeader`) entegrasyonu.

- **Otomatik Versiyonlama ve Derleme Motoru:**
  - Sürüm takibi için tek gerçeklik kaynağı `version.json` dosyası.
  - Yerel PowerShell ve Batch scriptleri (`scripts/update_version.ps1`, `scripts/build.bat`):
    - Her derlemede otomatik `build` numarası artırma ve Git branch/commit hash bağlama.
    - Tip güvenli C# sabitleri üreten `Common/AppVersion.g.cs` oluşturucusu.
    - `GelesiyeWPF.csproj` dosyasında Windows PE dosya özelliklerini (`Version`, `FileVersion`, `InformationalVersion`) güncelleme.
    - Durum çubuğu (`lblVersion`) ve Hakkında ekranında gerçek zamanlı sürüm/commit gösterimi.

- **Antigravity Workspace Özelleştirmeleri (`.agents/`):**
  - **Kurallar (`rules/`):** `coding-standards.md`, `wpf-component-guide.md`, `ui-color-contrast.md`, `versioning-rules.md`, `git-workflow.md`.
  - **Beceriler (`skills/`):** `gelesiye-dev`, `gelesiye-version`, `gelesiye-i18n`.
  - **Özel Alt Ajanlar (`agents/`):** `wpf-architect.md` (Arayüz ve MVVM uzmanı), `qa-verifier.md` (Test ve derleme doğrulama uzmanı).
  - **Entegrasyonlar:** Yaşam döngüsü kancaları (`hooks.json`) ve MCP sunucu deklarasyonları (`mcp_config.json`).

- **Dinamik Çoklu Dil (i18n / l10n) Altyapısı:**
  - Türkçe ve İngilizce dillerini sıfır harici dosya ve sıfır uydu DLL ile doğrudan ana yürütülebilir içine gömen `LocalizationManager` servisi.
  - XAML içerisinde anında reaktif bağlama sağlayan `{conv:Loc Key}` MarkupExtension'ı (`LocExtension`).
  - Uygulamayı kapatıp açmaya gerek kalmadan (restart-free) tüm açık pencerelerde anında dil değiştirme desteği.
  - `AyarlarWindow` Genel sekmesinde dil seçildiğinde gerçek zamanlı (live preview) olarak anında arayüze yansıma, iptal durumunda otomatik geri alma ve kaydetmede kalıcı veritabanı kaydı (`UygulamaDili`).
  - Tüm ekranlar ve diyaloglar eksiksiz bağlandı: `MainWindow` (özet kartlar, filtreler, cari ve hareket tabloları, alt toolbar, durum çubuğu, sistem tepsisi ve sağ tık menüleri), `AyarlarWindow`, `YedekYonetimiWindow`, `CariEditWindow`, `DekontEditWindow`, `CloseActionWindow`, `ListeToplamlariWindow`, `RaporSecimWindow`, `LoginWindow`, `AppDialog` ve `HakkindaWindow`.
  - Dil seçimine bağlı dinamik para/ondalık biçimlendirme (`LocalizationManager.Instance.CurrentCulture`) ve fiş türleri (`ToDisplayString`).

- **Gelişmiş Teşhis ve Loglama:**
  - Thread-safe günlük dosya logları (`logs/gelesiye_YYYYMMDD.log`) ve kritik hata dökümleri (`logs/gelesiye_error.log`).
  - UI Thread (`DispatcherUnhandledException`), arka plan thread'leri (`AppDomain.UnhandledException`) ve asenkron görevler (`TaskScheduler.UnobservedTaskException`) için global hata yakalama kalkanı.
  - Hata anında log konumunu gösteren kullanıcı dostu hata iletişim kutusu.

- **Entegrasyonlar, Çözüm ve Dağıtım:**
  - Bağımsız Visual Studio çözümü (`GelesiyeWPF.sln`) ve modern .NET 10 XML çözümü (`GelesiyeWPF.slnx`).
  - Tek dosya (`PublishSingleFile=true`) taşınabilir `.exe` paketleme desteği.
  - TCMB canlı döviz kurları takip çubuğu (USD, EUR gösterge kurları).
  - Tek tıkla Windows Hesap Makinesi entegrasyonu.
  - 25 hazır cari ve 150+ hareket içeren test/demo veri yükleyicisi (`DemoSeed`).
  - Yıl sonu devir sihirbazı.

### Düzeltildi
- **Pencere Çerçevesi & Başlık Çubuğu Hizalaması (`MainWindow`):**
  - Mavi başlık çubuğunun üstünde oluşan boşluk ve sol üst köşedeki piksel kayması/beyaz piksel artefaktı tamamen giderildi.
  - Hatalı DWM koordinat kaymalarına yol açan `WindowChrome` ve kısıtlayıcı `MaxHeight` mantığı kaldırılarak, standart Win32 `WM_GETMINMAXINFO` pencere kancası (`HwndHook`) entegre edildi.
  - Pencere normal boyuttayken kesintisiz 1px mavi çerçeve; ekranı kapladığında (Maximize) görev çubuğunu örtmeyen ve sıfır boşlukla ekrana oturan kusursuz yerleşim sağlandı.
- **Finansal Bakiye Renklendirmesi & Demo Veri Dağılımı (`DemoSeed`):**
  - Demo verilerinde tüm carilerin borçlu çıkmasına (`TOPLAM ALACAK = 0,00 TL`) ve tüm liste satırlarının kırmızı görünmesine yol açan işlem tutarları dengelendi.
  - Cariler gerçekçi bir oranda **Borçlu** (Kırmızı `▲`), **Alacaklı** (Yeşil `▼`) ve **Sıfır Bakiye** (Nötr Gri `0,00 TL`) olarak çeşitlendirildi; özet kartlar anında çok renkli ve dengeli hale getirildi.
  - Önceden tohumlanmış veritabanları için ilk açılışta devreye giren otomatik düzeltme migrasyonu (`EnsureRealisticMix`) eklendi.

